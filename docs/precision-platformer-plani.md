# Sıfırdan precision platformer planı ve Rock The Game ile karşılaştırma

*2026-11-14. Soru: "Unity'de precision platformer yapacaksın, karakter birebir benimki gibi olacak. Nasıl yapardın? Celeste'i reverse engineer ederek başla, sonra benimkiyle karşılaştır."*

Kaynaklar: Celeste'in açık kaynak `Player.cs` dosyası (NoelFB/Celeste deposu; §1.2'deki sabitler oradan birebir okundu, §1.3'teki davranış açıklamaları dosyanın özetlenen kısmından ve bilinen Celeste davranışından, satır satır doğrulanmadı), RTG'nin `docs/GDD.md`, `docs/systems/*.md`, `Assets/Scripts/New-Scripts/Player/` kodu ve sahnedeki oyuncu (BoxCollider 1×1, Rigidbody2D Interpolate, fizik adımı 0.02).

Bu bir analiz belgesi. Kodda hiçbir şey değiştirilmedi.

---

## 1. Celeste'i söküyoruz

### 1.1 Mimari

| Konu | Celeste nasıl yapıyor |
|---|---|
| Döngü | Sabit 60 Hz. Girdi, durum makinesi, yerçekimi, hareket ve çarpışma **aynı karede, aynı sırada** çalışır. Render ayrı bir adım değil; oyun 60 kare. |
| Fizik | Fizik motoru yok. `Actor` (oyuncu) ve `Solid` (duvar, platform) sınıfları. Konum **tam sayı piksel**, küsurat ayrı bir sayaçta birikir (`MoveH` / `MoveV`, Monocle'ın `Actor` sınıfında). Önce yatay, sonra dikey hareket; her piksel tek tek denenir. |
| Hareketli platform | Platform kendini hareket ettirirken üstündeki ya da yapışık Actor'ları **kendisi** iter ve taşır. Ayrılan oyuncu platformun hızını "lift speed" olarak alır (üst sınırlar `LiftXCap` 250, `LiftYCap` −130). |
| Durum makinesi | Tek `Player` sınıfı, `StateMachine` içinde `StNormal`, `StClimb`, `StDash`, `StBoost` ... Her durumun `Begin/Update/End` ve isteğe bağlı bir coroutine'i var. **Yer ve hava ayrı durum değil**: ikisi de `StNormal`, `onGround` bayrağıyla ayrılır. Duvar kayması da `StNormal` içinde. |
| Hitbox | Normal 8×11 px, eğilince 8×6 px. Hasar kutusu çarpışma kutusundan küçük (8×9). Tile 8 px. |
| Ölüm | Anında; oda başındaki spawn noktasına döner, oda sıfırlanır. |

### 1.2 Sabitler (Unity birimine çevrilmiş)

Çeviri: **1 Unity birimi = 1 tile = 8 px** varsayımıyla `px/8`. RTG'de oyuncu 1×1 birim, yani karakter "bir tile boyunda" kabul edildi. Celeste'te karakter 1 tile genişliğinde, 1.375 tile boyunda.

| Celeste sabiti | Değer (px) | Unity birimi | Ne işe yarar |
|---|---|---|---|
| `MaxRun` | 90 | 11.25 /sn | koşu hızı |
| `RunAccel` / `RunReduce` | 1000 / 400 | 125 / 50 /sn² | 0'dan tam hıza 0.09 sn; **hız sınırın üstündeyse** daha yavaş (400) söner |
| `AirMult` | 0.65 | | havada ivme çarpanı |
| `Gravity` | 900 | 112.5 /sn² | |
| `HalfGravThreshold` | 40 | 5 /sn | zıplama tuşu basılı ve \|vy\| < 40 iken yerçekimi yarıya iner (tepede asılı kalma) |
| `MaxFall` / `FastMaxFall` | 160 / 240 | 20 / 30 /sn | aşağı basılıysa daha hızlı düşüş |
| `JumpSpeed` | 105 | 13.1 /sn | |
| `JumpHBoost` | 40 | 5 /sn | zıplarken stick yönüne ek yatay hız |
| `VarJumpTime` | 0.2 sn | | tuş basılı kaldıkça 0.2 sn boyunca dikey hız zıplama hızında tutulur; bırakınca tutma biter |
| `JumpGraceTime` (coyote) | 0.1 sn | | |
| Zıplama buffer'ı | 0.08 sn | | `VirtualButton` buffer süresi |
| `UpwardCornerCorrection` | 4 px | 0.5 | tavan köşesine kafa vurunca 4 px'e kadar yana kaydırıp geçirir |
| `WallJumpCheckDist` | 3 px | 0.375 | duvara **değmeden** 3 px uzaktan duvar zıplaması |
| `WallJumpForceTime` | 0.16 sn | | duvar zıplamasından sonra yatay girdi kilidi |
| `WallJumpHSpeed` | 130 | 16.25 /sn | |
| `WallSlideStartMax` / `WallSlideTime` | 20 / 1.2 sn | 2.5 /sn | yavaş kayma süresi |
| `WallSpeedRetentionTime` | 0.06 sn | | duvara çarpınca kaybolan yatay hız 0.06 sn içinde duvar biterse geri verilir |
| `DashSpeed` / `DashTime` | 240 / 0.15 | 30 /sn, **4.5 tile** | |
| `EndDashSpeed` / `EndDashUpMult` | 160 / 0.75 | 20 /sn | dash sonunda hızın 2/3'ü korunur |
| `DashCornerCorrection` | 4 px | 0.5 | dash'te köşeye takılınca kaydırma |
| `DashHJumpThruNudge` | 6 px | | yatay dash'te tek yönlü platformun üstüne itme |
| `DashVFloorSnapDist` | 3 px | | yere yakın yatay dash'te zemine yapıştırma |
| Dash donması | `Celeste.Freeze(.05f)` | 3 kare | **bütün oyun** donar (dünya dahil) |
| `ClimbMaxStamina` | 110 | | |
| `ClimbUpCost` / `ClimbStillCost` / `ClimbJumpCost` | 45.5 /sn, 10 /sn, 27.5 | | yukarı tırmanmak ~2.4 sn, sabit tutunmak ~11 sn, tırmanma zıplaması bütçenin 1/4'ü |
| `ClimbUpSpeed` / `ClimbDownSpeed` | 45 / 80 | 5.6 / 10 /sn | |
| `ClimbHopY` / `ClimbHopX` | 120 / 100 | | duvarın tepesinde **otomatik** zıplayıp üste çıkma |
| Super / hyper / wallbounce | `SuperJumpH` 260, `SuperWallJumpSpeed` 160 ... | | dash'i zıplamayla iptal etmekten doğan ileri teknikler |

### 1.3 Celeste'i "adil" yapan şeyler

Celeste'in zorluğu sabitlerden değil, oyuncunun **niyetini** okuyan küçük toleranslardan geliyor. Bunlar listede en önemli kısım:

1. **Coyote (0.1 sn) ve zıplama buffer'ı (0.08 sn).**
2. **Köşe düzeltmeleri.** Tavan köşesi (4 px), dash köşesi (4 px), tek yönlü platform itmesi (6 px), zemine yapıştırma (3 px).
3. **Duvara değmeden duvar zıplaması** (3 px). Oyuncu "duvara yapıştım" sanıyorsa yapışmıştır.
4. **Hız koruma.** Duvara çarpınca kaybolan hız 0.06 sn geri verilebilir; sınırın üstündeki hız yavaş söner (`RunReduce` < `RunAccel`), yani ileri basmak momentumu **hiçbir zaman** öldürmez.
5. **Dash girdi gecikmesi.** Dash yönü basıldığı kare değil, donmadan sonraki karede okunur; tuşa stick'ten bir kare önce basmak cezalandırılmaz. Yön 8'e yuvarlanır.
6. **Otomatik kenar çıkışı** (climb hop). Tepeye yaklaşan oyuncu ek girdi vermeden üste çıkar.
7. **Dünyayı da donduran kısa hit-stop** (0.05 sn). Tehlikeler donma sırasında yerinde durur, yani donma oyuncunun zamanlamasını bozmaz.
8. **Determinizm.** Aynı girdi her zaman aynı sonucu verir. Speedrun ve TAS topluluğu bunun üstüne kurulu.

---

## 2. Sıfırdan nasıl yapardım

Karakterin yapabilecekleri RTG ile birebir aynı: koşma, değişken zıplama, coyote/buffer, tek dash, tek kanca (swing + yerden hop), çift kanca, slingshot, dönen node, tırmanma (stamina), kayma, duvar zıplaması, kenardan çıkma, hareketli yüzeyde taşınma, oda geçişi. Kontroller de GDD §5.1 ile aynı.

### 2.1 Temel kararlar

| Karar | Seçim | Neden |
|---|---|---|
| Simülasyon adımı | **Tek sabit adım, 60 Hz.** Girdi örnekleme, bütün zamanlayıcılar, durum geçişleri ve hareket aynı adımda. `Update` yalnızca görsel. | Precision oyunda "aynı girdi = aynı sonuç" pazarlıksız. 60 Hz monitörde her render karesi tam bir simülasyon adımı olur, kare kayması olmaz. |
| Girdi | `Update`'te "son adımdan beri basıldı mı" mandalı (latch), adım içinde tüketilir. Buffer'lar adım sayısıyla. | `WasPressedThisFrame` iki fizik adımı arasında kaybolmaz ya da iki kez okunmaz. |
| Fizik | Unity fiziği yok, kendi `Actor`/`Solid` çözücüm. Float konum + BoxCast, ama sonuç bir "simülasyon ızgarasına" (örn. 1/16 birim) yuvarlanır. | Unity'nin Rigidbody'si (kinematik bile) belirsizlik ve kontak toleransı getiriyor. Celeste'in tam sayı pikseli Unity'de garip durur; ızgaraya yuvarlama determinizmin çoğunu verir. |
| Hareketli platformlar | Platform kendini hareket ettirirken üstündeki ve yapışık oyuncuyu **kendisi** taşır ve iter (Celeste `Solid.MoveH`). Ayrılırken taşıma hızı oyuncuya "lift speed" olarak geçer, kısa bir tolerans süresiyle. | Taşıma, oyuncunun durumundan bağımsız tek bir yerde çözülür; her durum kendi taşıma kuralını yazmaz. |
| Durum makinesi | Celeste düzeni: `Normal` (yer + hava + kayma bir arada, `onGround` bayrağıyla), `Dash`, `Climb`, `LedgeHop`, `Swing`, `DualSwing`, `Slingshot`, `Frozen` (oda geçişi, respawn). | Yer/hava ayrı durumlarken her geçişte hız, buffer, taşıma bilgisi kopyalanmak zorunda. Tek `Normal` durumu bu sınıf hataları baştan yok eder. |
| Kaynaklar | `DashCharges` (int, ileride 2 dash için), `SlingshotCharge`, `Stamina` (maliyet bazlı), `WallSlideTime`. Tek bir `RefillAll(kaynak)` fonksiyonu. | Yenilenme kuralları tek tabloda okunur. |
| Veri | Tek `MovementTuningSO`, Play Mode'da canlı değiştirilebilir; her alanın "türetilmiş" karşılığı (zıplama yüksekliği, dash mesafesi) Inspector'da görünür. | |
| Hit-stop | Bütün simülasyonu (dünya dahil) donduran global sayaç, dash'te 3 kare (0.05 sn). | Donma oyuncunun dünyayla zamanlamasını bozmamalı. |

### 2.2 Kanca kısmı (Celeste'te yok, kendim tasarlardım)

Celeste'te kanca yok, ama aynı ilkeler geçerli: niyeti oku, sonucu öngörülebilir yap.

1. **Anlık tutunma** ve görünür hedef. Nişangâh, tetiğe basınca hangi node'un seçileceğini **her zaman** önceden göstersin.
2. **Hedef seçiminde histerezis.** Seçili node, yeni aday belirgin şekilde daha iyi olmadıkça değişmesin (stick titremesi hedefi zıplatmasın).
3. **Kanca buffer'ı.** Tetik, node menzile girmeden ~0.1 sn önce basıldıysa menzile girdiği adımda tutun.
4. **İp, bir konum kısıtı.** Her adımda: yerçekimi + girdi → aday konum → ip boyuna izdüşüm → hızı teğete izdüşüm. Anchor node'un **kendisine** referans; node hareket ederse ip de gider ve node'un hızı oyuncuya ayrılırken aktarılır.
5. **Tek çıkış kuralı.** Tek kanca, çift kanca ve slingshot bırakışları aynı hava kuralına girer: hız aynen korunur, zıplama tuşu basılıysa normal yerçekimi, değilse yükselişte erken bitirme çarpanı. Oyuncu "kancadan nasıl çıktığım" konusunda üç ayrı kural ezberlemez.
6. **Hız sınırı yalnızca girdiyi sınırlar**, yerçekiminden gelen hızı kesmez (RTG'de zaten böyle).
7. **Slingshot hakkı fırlatma anında** harcanır; gerilme sırasında vazgeçilebilir. (RTG'de bilinçli olarak başta harcanıyor; tasarım kararı.)

### 2.3 Toleranslar listesi (baştan koyardım)

| Tolerans | Başlangıç değeri |
|---|---|
| Coyote | 0.1 sn (6 adım) |
| Zıplama buffer'ı (yer, duvar, kanca bırakma dahil **her** zıplama) | 0.1 sn |
| Tavan köşe düzeltmesi | 0.5 birim |
| Dash köşe düzeltmesi | 0.5 birim |
| Duvar zıplaması mesafesi (değmeden) | 0.35 birim |
| Duvar zıplaması, yükselirken de | evet (Celeste gibi) |
| Kenar çıkışı | otomatik, tepeye 0.3 birim kala |
| Dash yön okuma gecikmesi | 1 adım |
| Kanca buffer'ı | 0.1 sn |
| Sınır üstü hız sönümü (yer ve hava) | ivmeden yavaş, ileri basmak momentumu öldürmez |

### 2.4 Araçlar (oyunun kendisi kadar önemli)

1. **Girdi kaydı ve tekrar oynatma.** Simülasyon deterministik olduğu için 60 Hz girdi akışı kaydedilip birebir oynatılabilir. Hata raporu = girdi dosyası. Multiplayer hayaleti de bu kayıttan beslenebilir.
2. **Yörünge çizici.** Scene view'de son 2 saniyenin yolu, her adımda durum rengiyle.
3. **Debug paneli.** Aktif durum, hız, kaynaklar, buffer/coyote sayaçları.
4. **Kural testleri** (`bozulmamali-davranislar.md` gibi). Deterministik çekirdek sayesinde sahne açmadan, saf C# ile EditMode'da koşar.
5. **Canlı tuning.** SO değerleri Play Mode'da değişir, "son değişiklikleri geri al" düğmesi.

### 2.5 Aşamalar

| Aşama | İçerik | Bitti sayılır |
|---|---|---|
| 0 | Sabit 60 Hz döngü, girdi mandalı, `Actor`/`Solid` çözücü, debug paneli | Kutu, tile haritasında çarpışarak yürüyor; aynı girdi kaydı iki kez aynı konuma varıyor |
| 1 | `Normal` durumu: koşu, zıplama, coyote, buffer, köşe düzeltmesi | Celeste sabitleriyle "Celeste gibi" hissettiren yer/hava hareketi |
| 2 | Dash + global hit-stop + köşe düzeltmeleri | |
| 3 | Duvar: tırmanma (maliyet bazlı stamina), kayma, duvar zıplaması, otomatik kenar çıkışı | |
| 4 | Hareketli `Solid`'ler, taşıma, lift speed | Hareketli duvar ve zemin aynı kodla taşıyor |
| 5 | Kanca: tek, yerden hop, çift, slingshot, dönen node | Tek çıkış kuralı, hareketli node |
| 6 | Oda geçişi, ölüm/respawn, checkpoint | |
| 7 | Görsel katman (interpolasyon, kollar, efektler, titreşim) | |
| 8 | Kural testleri + girdi tekrarı ile regresyon | |

Tuning stratejisi: önce Celeste sabitleriyle başla (bilinen iyi bir nokta), sonra RTG'nin kimliğine doğru kaydır (daha yüksek zıplama, daha hızlı kanca). Sıfırdan sayı aramaktansa iyi bilinen bir noktadan uzaklaşmak daha hızlı.

---

## 3. Rock The Game ile karşılaştırma

### 3.1 Genel tablo

| Alan | Celeste | Sıfırdan planım | RTG bugün |
|---|---|---|---|
| Döngü | Tek sabit 60 Hz | Tek sabit 60 Hz | **İkiye bölünmüş:** girdi, zamanlayıcılar ve durum geçişleri `Update`'te (render hızında), hız ve çarpışma `FixedUpdate`'te (50 Hz) |
| Fizik | Kendi Actor/Solid, tam sayı piksel | Kendi çözücü, ızgaraya yuvarlama | Kendi çözücü (kinematik Rigidbody2D + BoxCast, önce yatay sonra dikey). Celeste'e yakın, iyi |
| Yer/hava | Tek `StNormal` | Tek `Normal` | Ayrı `GroundedState` / `AirborneState`; her geçişte hız, coyote, taşıma parametreleri constructor'la taşınıyor (`AirborneState` 12 parametre) |
| Koşu | 0.09 sn ivme, sınır üstü yavaş söner | Celeste gibi | Anlık; yere inince hız **anında** `stick × 10`'a kesiliyor |
| Hava kontrolü | Sınır üstündeyken ileri basmak yavaş söndürür | Celeste gibi | İleri basmak (40/sn²) bırakmaktan (20/sn²) **iki kat hızlı** söndürüyor |
| Zıplama | 13.1 /sn, ~3.5 tile, tepeye 0.36 sn | | 16 /sn, 4 birim, tepeye 0.5 sn (daha yüksek ve havada daha uzun; kanca oyunu için mantıklı) |
| Değişken zıplama | Tuş basılıyken hız 0.2 sn sabit | | Tuş bırakılınca 3× yerçekimi. İkisi de geçerli yöntem |
| Coyote / buffer | 0.1 / 0.08 | 0.1 / 0.1 | 0.1 / 0.1, ama buffer **yalnızca yer ve coyote zıplamasında**; duvarda `IsJumpPressed` doğrudan okunuyor |
| Köşe düzeltmesi | Tavan 4 px, dash 4 px | Var | **Yok** |
| Dash | 8 yön, 4.5 tile, 0.15 sn, sonunda 2/3 hız | | Serbest analog yön, 7 birim, 0.2 sn, sonunda 0.3 hız |
| Dash süresi | Adım sayısıyla, sabit | Adım sayısıyla | `Update`'te `Time.deltaTime` ile; dash'in kaç fizik adımı süreceği render zamanlamasına bağlı |
| Hit-stop | 0.05 sn, **tüm dünya** donar | Celeste gibi | 0.2 sn, **yalnızca oyuncu** donar, tehlikeler ve platformlar akar |
| Duvar zıplaması | 3 px uzaktan, yükselirken de | Celeste gibi | Yalnızca kayma durumunda (düşerken + stick duvara) ya da tırmanırken; temas şart |
| Stamina | Maliyet bazlı (yukarı 2.4 sn, sabit 11 sn, zıplama 1/4) | Maliyet bazlı | Sabit 1/sn, 6 sn; hareketten bağımsız. Basit ve okunur, HUD halkasıyla iyi eşleşiyor |
| Kenar çıkışı | Otomatik climb hop | Otomatik | 0.1 sn yukarı tutma + 0.2 sn animasyon (bilinçli) |
| Hareketli yüzey | Solid oyuncuyu iter/taşır, lift speed | Celeste gibi | Oyuncu tarafında çözülüyor (`KinematicPhysicsHandler.Move` sonunda `DeltaPosition` ekleniyor) + `SurfaceCarry` + `_carriedVelocityX`. Çalışıyor ama kural her durumda ayrı ayrı yazılmış |
| Kanca hedefi | — | Histerezis + buffer | En iyi hizalanan node, ±60° koni, engel kontrolü, nişangâh var. Histerezis ve buffer yok |
| Hareketli node | — | Anchor node'u izler | Anchor bir `Vector2` kopyası; node gider, ip kalır |
| Kancadan çıkış | — | Tek kural | Üç farklı kural (tek: hep 3×, çift: sabit 1×, slingshot: tuşa bağlı). `feel-notlari.md`'de karar bekliyor |
| Kural testleri | Yok (açık değil) | Var, saf C# | Var (PlayMode, sahne yükleyerek) + EditMode bant testleri. Celeste'te bile olmayan bir şey |
| Tuning verisi | Kodda sabit | SO | SO (`PlayerStatsSO`), türetilmiş değerler (gravity, dash hızı) var. İyi |
| Determinizm / tekrar | Var | Var | Yok; MP hayaleti konum akışıyla çalışıyor |

### 3.2 RTG'nin iyi yaptıkları

- **Kendi çarpışma çözücüsü.** Dinamik Rigidbody yerine kinematik + BoxCast, önce yatay sonra dikey: Celeste'in yaklaşımına en yakın Unity çözümü. Zor kısım zaten doğru seçilmiş.
- **Tasarım değerlerinden türetilen zıplama** (`Gravity = −2h/t²`). Tasarımcı "4 birim, 0.5 sn" diye düşünüyor, fizik sabitiyle değil.
- **Kural belgesi + testler.** `bozulmamali-davranislar.md` ve onu koruyan testler, "bir şeyi düzeltirken başka bir şeyi bozma" sorununu çözüyor. Çoğu indie platformerda yok.
- **Kanca anlık ve nişangâhlı.** Mermi uçuşu yok, sonuç önceden görünüyor. Precision için doğru.
- **Hız sınırı yalnızca girdiyi kesiyor** (`SwingingState`), yerçekimiyle kazanılan hız korunuyor.
- **Sistem belgeleri** (`docs/systems/`) koddaki gerçek davranışı ve sapmaları açıkça yazıyor.

### 3.3 Bilinçli farklar (dokunulmamalı, tasarım kararı)

- Anlık koşu (ivmesiz) — Meat Boy'a yakın, keskin. Sorun yalnızca momentumu öldürdüğü yerde (aşağıda #3).
- Daha yüksek, daha uzun zıplama — kanca node'larına ulaşmak için.
- Serbest analog dash — Celeste 8 yön. Gamepad'de çapraz hedefleme zorlaşır; oynayarak karar verilmeli. Bir seçenek: 16 yöne yuvarlama ya da yalnızca eksenlere ±10° yapışma.
- Sabit stamina, tutma gerektiren kenar çıkışı, yerden yalnızca sağ tetik, slingshot hakkının başta harcanması.

### 3.4 Precision açısından riskler (önem sırasıyla)

Hepsi koddan doğrulandı; hiçbiri oynanarak ölçülmedi. Değer değişikliği gerektirenler "his" kararıdır (CLAUDE.md kural 4).

1. **Dash mesafesi kare hızına göre değişiyor.** `DashState.UpdateState` (`States/DashState.cs:46`) süreyi `Update`'te `Time.deltaTime` ile sayıyor, hızı `FixedUpdate` uyguluyor. 0.2 sn'lik dash, render zamanlamasına göre 9, 10 ya da 11 fizik adımı sürebilir: mesafe 6.3 ile 7.7 birim arasında oynar (±%10). Precision oyunda aynı dash'in her seferinde aynı yere gitmesi gerekir. Aynı desen öteki zamanlayıcılarda da var (coyote, kilitler, ledge climb süresi). Çözüm yönü: zamanlayıcıları `FixedUpdate`'e taşımak, girdiyi mandalla almak.

2. **Hit-stop yalnızca oyuncuyu donduruyor, 0.2 sn.** Her dash'te oyuncu 0.2 sn (10 fizik adımı) yerinde duruyor, dönen ve hareket eden tehlikeler akmaya devam ediyor. Celeste'te bu 0.05 sn ve bütün dünya donuyor. Hareketli tuzak + dash kombinasyonunda oyuncu dash'e basınca tuzağın konumu 0.2 sn kayıyor; bu "haksızlık" hissinin tipik kaynağı (GDD sütun 3). `PlayerController.NotifyImpact` → `HitStopManager`.

3. **Havada ileri basmak momentumu öldürüyor.** `AirborneState.ApplyAirMovement` (`States/AirborneState.cs:194`): stick basılıysa `AirAcceleration` (40), boşsa `AirDrag` (20). Kancadan 25 birim/sn ile çıkan oyuncu stick'i ileri tutarsa hız 0.375 sn'de 10'a düşüyor, bıraksa 0.75 sn sürüyor. Oyuncu "ileri gitmek istiyorum" dediği anda yavaşlıyor. Celeste'te sınırın üstünde aynı yöne basmak daha **yavaş** bir sönüm (`RunReduce`) kullanıyor. GDD sütun 1 ("hızlanmalı, ritim bulmalı") ile doğrudan çelişiyor. Yere inişte de aynısı: `GroundedState.FixedUpdateState` (`States/GroundedState.cs:56`) hızı tek adımda `stick × 10`'a kesiyor.

4. **Köşe düzeltmesi yok.** Zıplarken tavan köşesine birkaç piksel değmek dikey hızı sıfırlıyor (`HandleVerticalObstructions`), dash'te bir köşeye birkaç piksel sürtmek o eksendeki hareketi kesiyor (karakter köşeye takılıyor). Oyuncu "geçtim sandım" anlarının bir numaralı kaynağı. Tamamen kodla eklenebilir, his değeri değil.

5. **Duvar zıplaması dar.** Yalnızca kayma durumundayken (düşerken ve stick duvara itilirken) ya da tırmanırken çalışıyor, temas şart. Yükselirken duvarın yanında zıplamak, ya da stick'i duvara itmeden zıplamak normal zıplama/hiçbir şey veriyor. Duvar zıplaması `IsJumpPressed`'i doğrudan okuyor (`States/WallSlidingState.cs:110`), buffer'ı kullanmıyor: duvara değmeden bir kare önce basılan zıplama kayboluyor.

6. **Yer ve hava ayrı durumlar.** Bugüne kadarki birçok düzeltme (`SurfaceCarry`, `_carriedVelocityX`, `bypassJumpGravity` / `bypassApexHangGravity` / `bypassEndEarlyGravity` / `endEarlyGravityMultiplier`) bu sınırda bilgi taşıma zorunluluğundan doğmuş. `movement-core.md`'deki "havaya giriş → yerçekimi kuralı" tablosunun beş satırı var. Bu bir hata değil, ama yeni her mekanikte aynı sınıf hatayı üretecek bir yapı.

7. **Kancadan üç ayrı çıkış kuralı.** Zaten `feel-notlari.md`'de karar bekliyor. Precision açısından öneri: tek kural.

8. **Hareketli node ipi taşımıyor** (`grapple.md` #8). Hareketli node'lu oda yapılırsa bu, ilk testte görünecek.

9. **50 Hz fizik, 60/144 Hz ekran.** Interpolasyon açık olduğu için görüntü akıcı, ama girdi gecikmesi kare başına değişiyor (0–20 ms). 60 Hz denemesi `TASKS.md`'de zaten var; #1 ile birlikte düşünülmeli.

### 3.5 Kısa sonuç

RTG'nin temeli doğru seçilmiş: kendi çarpışma çözücüsü, türetilmiş zıplama, SO tuning, kural testleri. Sıfırdan yapsaydım değiştireceğim iki büyük şey var: **tek sabit adımlı döngü** (girdi, zamanlayıcı ve fizik aynı adımda) ve **yer/havayı tek durumda birleştirmek**. Geri kalan farklar Celeste'in tolerans listesinden eksik kalanlar (köşe düzeltmesi, değmeden duvar zıplaması, her yerde buffer, momentumu öldürmeyen hava kontrolü, dünyayı da donduran kısa hit-stop). Bunlar baştan yazmadan, mevcut koda tek tek eklenebilir.

Önerilen sıra (her biri ayrı task, `#code` + aynı/ertesi gün `#test`):

1. Zamanlayıcıları `FixedUpdate`'e taşı, girdi mandalı (dash mesafesi sabitlensin). His değişmez, yalnızca tutarlılık.
2. Köşe düzeltmesi (tavan + dash). His değeri yok, yalnızca tolerans.
3. Duvar zıplamasını buffer'a bağla ve değmeden mesafe ekle.
4. Hava kontrolünde sınır üstü sönüm (yeni `PlayerStatsSO` alanı, ör. `AirOverspeedReduce`). **His kararı:** gamepad testi gerekir.
5. Hit-stop: dünyayı da dondurmak ya da süreyi kısaltmak. **His kararı.**
6. Kancadan tek çıkış kuralı (`feel-notlari.md` kararı verilince).
7. (Büyük, isteğe bağlı) Yer/hava birleştirme.
