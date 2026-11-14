# Tasks

> **Format:** `- [ ] **Başlık** #kategori - kısa bağlam, due YYYY-MM-DD`
> **Kategoriler:** `#art` `#test` `#level` `#code`
> **Her gün 5–6 saat, bir güne en fazla 4 task.** Planlama kuralları: `CLAUDE.md` → "Proje yönetimi ve planlama".

## Active

### Çok oyunculu

- [ ] **MP 0: Steam'i iki bilgisayarda dene** #test - MP 9 kodda; build al, ikisinde Steam açık, lobide "Bağlantı: Steam", host Host ol → davet, diğeri kabul eder; yarış, bayrak, ceza, bitiş (docs/multiplayer-arkadasla-test.md §3a), due 2026-11-09


## Waiting On

### Options ekranı (karar → veri → sekmeler → bağlama → test)

- [ ] **Lobi ve options tasarımını seç** #art - `docs/ui-research/lobby-v1.html` ve `options-v1.html`'de oy ver, notları yapıştır; dil (UR-12), Retry ölüm saysın mı (UR-9) ve ölçek kuralı (UR-3) kararlarını da ver. Sonraki lobi/options task'ları buna bağlı, due 2026-11-14
- [ ] **Ayarlar: veri ve kayıt** #code - titreşim çarpanı + açık/kapalı (`HapticManager`), CRT açık/kapalı ve gücü, pencere modu/çözünürlük; PlayerPrefs, açılışta uygula; ses zaten `AudioManager`'da; EditMode testi (docs/ui-research/options-menu.md), due 2026-11-16
- [ ] **Options ekranı: ses sekmesi** #code - seçilen düzen (`options-v1.html` A ya da B), sekme yapısı + ses sekmesi, örnek ses; hareket ui-motion.md listesine göre (options-menu.md), due 2026-11-16
- [ ] **Options ekranı: titreşim ve görüntü sekmeleri** #code - örnek titreşim, CRT önizleme, çözünürlük 10 sn onay (options-menu.md), due 2026-11-17
- [ ] **Options'ı ana menü ve pause'a bağla** #code - ana menüde `options` etkin, pause'da options dilimi `Enabled`; B ile geldiği yere döner; `docs/systems/options.md` yaz, `pause.md`'yi güncelle ve oraya unscaled zaman kuralını ekle (UR-8) (options-menu.md), due 2026-11-17
- [ ] **Options'ı gamepad ile dene** #test - ana menüden ve pause'dan aç-kapa, sekmeler, basılı tutma hızı, değerler oyunu kapatıp açınca kalıyor mu, okunurluk, due 2026-11-18

### Level

- [ ] **Bayrakları yerleştir** #level - 22 odada `RaceFlagPoint`'leri istediğin yere sürükle (sarı bayrak gizmo'su); oda 21 ve 22'ninkiler oda ortasında havada, mutlaka taşı; oda 21'deki bitiş işaretini (dama bayrağı gizmo'su, `RaceFinishPoint`) de bitiş yerine taşı, due 2026-12-07

## Someday

### Oyuncu FSM kararları (PF-7, PF-8, PF-9)

- [ ] **Hitstop sırasında platformlar da dursun mu** #code - karar: hitstop yalnızca oyuncuyu donduruyor, level 15'te dash atarken duvar 0.8 birim kaçıyor; platformlar `HitStopEvents`'e abone mi olsun, hitstop mu kısalsın (docs/audit/2026-11-14-player-fsm.md PF-7). Celeste karşılaştırması: orada 0.05 sn ve bütün dünya donuyor, burada 0.2 sn ve yalnızca oyuncu (docs/precision-platformer-plani.md §3.4 #2)
- [ ] **Duvara tutunma toleransı** #code - karar: havadan/yerden tutunmak fiilî temas istiyor, `WallSnapRaycastDistance` (2) pratikte işlemiyor; PF-2'deki toleransla tutunma (PF-8)
- [ ] **Player FSM küçükleri** #code - `SlingshotState` hakkı anchor kontrolünden önce harcıyor; `CurrentWallSlideTime` 2f sabit (PF-9)

### Oyuncu FSM: momentum (PF-4)

- [ ] **Havadaki momentum: karar ve uygulama** #code - duvara çarpınca yatay hız sıfırlansın mı, "köşe boost'u" olarak korunsun mu; kararı movement-core.md'ye yaz, sıfırlama seçilirse uygula (PF-4). Celeste: sıfırlar ama duvar 0.06 sn içinde biterse hızı geri verir (`WallSpeedRetentionTime`, precision-platformer-plani.md satır 46); seçenekler (a) düz sıfırla, (b) köşe boost'u olarak koru, (c) sıfırla + 0.06 sn geri ver (önerilen)
- [ ] **Momentum değişikliğini gamepad ile dene** #test - swing'den duvara çarpıp duvar ucundan çıkış, slingshot sonrası duvar teması (PF-4); karar task'ından sonra

### Precision toleransları (docs/precision-platformer-plani.md §3.5)

- [ ] **Airborne kilit ve coyote sayaçlarını fizik adımına al** #code - `AirborneState.DecrementTimers` coyote, yatay kilit, kanca kilidi ve duvar tırmanma kilidini `Update`'te `deltaTime` ile sayıyor; `FixedUpdateState`'e taşı, süreler kare hızından bağımsız olsun (PF-6)
- [ ] **Duvar zıplaması: değmeden mesafe ve yükselirken karar** #code - duvara ~0.35 birim yakınken (temas olmadan) ve yükselirken duvar zıplaması olsun mu; kararı `wall-movement.md`'ye yaz, W5 ile tutarlılığı kontrol et, olacaksa uygula (PF-5, precision-platformer-plani.md §3.4 #5)

- [ ] **Dash mesafesi tutarlılık testi** #test - aynı dash'i farklı kare hızlarında (örn. `Application.targetFrameRate` 30/60/144) tekrar et, mesafe aynı mı; PlayMode kural testi olarak `DashRulesTests`'e eklenebilir; dash süresi task'ından sonra
- [ ] **Köşe düzeltmesi (tavan + dash)** #code - zıplarken tavan köşesine birkaç piksel değince yana kaydırıp geçir, dash'te köşeye sürtünce dikey kaydır; Celeste değeri 0.5 birim (4 px), mesafe `PlayerStatsSO`'ya yeni alan (§3.4 #4)
- [ ] **Köşe düzeltmesi testi** #test - tavan köşesine 0.1–0.4 birim değerek zıpla, geçiyor mu; 0.6'da geçmiyor mu; dash için aynısı; köşe düzeltmesinden sonra
- [ ] **Duvar zıplaması buffer testi** #test - duvara değmeden 1–5 kare önce zıpla, duvar zıplaması çıkıyor mu; `WallRulesTests`'e eklenebilir; buffer task'ından sonra
- [ ] **Havada sınır üstü hız sönümü** #code - his kararı: `AirborneState.ApplyAirMovement` stick ileriyken 40, boştayken 20 ile söndürüyor; kancadan 25 birim/sn çıkan oyuncu ileri bastıkça daha hızlı yavaşlıyor. Hız sınırın üstünde ve stick aynı yöndeyse daha yavaş sönüm (yeni alan, ör. `AirOverspeedReduce`); yere inişte hızın tek adımda 10'a kesilmesi de aynı soru. Eski/yeni değeri `feel-notlari.md`'ye yaz (§3.4 #3)
- [ ] **Hava sönümü gamepad testi** #test - kancadan ve slingshot'tan çıkıp stick'i ileri tut / bırak; momentum korunuyor mu, kontrol kaybı var mı; sönüm task'ıyla aynı ya da ertesi gün
- [ ] **Kancadan tek çıkış kuralı** #code - tek kanca, çift kanca ve slingshot bırakışı şu an üç farklı yerçekimi kuralı; `feel-notlari.md`'deki karar verilince tek kurala indir (§3.4 #7)
- [ ] **Yer/hava durumlarını birleştirme planı** #code - büyük iş, önce yalnızca plan: `GroundedState` + `AirborneState` tek `NormalState` olursa `SurfaceCarry`, `bypass*Gravity` parametreleri ve coyote aktarımı nasıl sadeleşir; parçalara bölünmüş task listesi çıkar (§3.4 #6)

### CRT

- [ ] **CRT: slot mask prototipi** #code - mevcut shader'a dokunmadan kopya (`CRTFilter` → yeni shader); fiziksel piksel uzayında (`positionCS`) kanal bazlı slot mask (R/G/B dikey çubuklar, pitch ve gap parametre), kalın scanline bandı + aralık, gain telafisi; sanal piksel hücresine hizalı (6×6/8×8 fiziksel piksel); önce `docs/systems/`'te CRT doc'u var mı bak; alternatif: Lottes CRT shader'ını port etmek (lisansı doğrula); sebep: şu an gerçek CRT fosfor görünümü yok, referans: yakın plan fotoğrafta R/G/B çubuklar ve kalın scanline (kullanıcı şimdilik ilgilenmek istemedi)
- [ ] **CRT prototipini Game view'da karşılaştır** #test - 1x ve 5x ölçekte referans fotoğrafa (slot mask, sprite hücreleri) benziyor mu; sanal çözünürlük kararı (480×360 kalsın mı, 320×180/240×135 mi), moiré/titreşim notu; slot mask prototipinden sonra

### UI

- [ ] **Başlık ekstrüzyonunda tırtıklı kenarlar** #code - `UiExtrudedText` kopyaları çapraz kenarlarda merdiven yapıyor; önce gamepad mesafesinde görünüyor mu bak (docs/ui-research/ui-review-2026-11-12.md UR-6)

### Teknik borç ve build

- [ ] **Player build al ve doğrula** #test - RockTheGame.Runtime'daki EasyTextEffects.Editor importu build'de patlıyor mu (6 Kasım'da multiplayer için ertelendi)
- [ ] **Fixed Timestep 60 Hz denemesi** #code - 0.02 → 0.01667 dene, bir seviye oyna, beğenmezsen geri al (kalan-isler §1.2) (6 Kasım'da multiplayer için ertelendi)
- [ ] **DontDestroyOnLoad uyarısını düzelt** #code - Play Mode'da "only works for root GameObjects" çıkıyor; aday AudioManager.cs:72 ya da HapticManager.cs:26, kaynağı doğrula ve düzelt (6 Kasım'da multiplayer için ertelendi)
- [ ] **PhysicsTest sahneleri ve M_CRTFilter.mat gürültüsü** #code - takipsiz sahneleri commit'le ya da sil; `_CrtTime` izini kes

### Oda 16 ve level düzeltmeleri

- [ ] **Oda 16 kamera uzaklığı** #level - ortho 22'de zemindeki dikenler noktalı çizgi gibi, oyuncu çok küçük; oda kamera ayarını yakınlaştır ya da dikenleri büyüt (docs/systems/hazards.md) (6 Kasım'da multiplayer için ertelendi)
- [ ] **Ezici bloklar node'ları örtüyor** #level - oda 16'da beyaz `MovingDeath` blokları node'ların önünden geçerken onları tamamen gizliyor; ikisi de Default/0, node'ları öne al (docs/systems/hazards.md) (6 Kasım'da multiplayer için ertelendi)

### Tasarım ve görsel kararlar

- [ ] **PPU standardı kararı** #art - 5 Kasım'da ertelendi; ölçüm kalan-isler §1.4'te (zemin/diken/boru 16, oyuncu 24/32, büyütülmüş meyve/trambolin 5–14)
- [ ] **Pixel Perfect Camera denemesi** #art - PPU kararından sonra (kalan-isler §1.5)
- [ ] **GDD §8 "yeşil, pis asit" ile görsel uyumu** #art - metin mi sanat mı güncellenecek
- [ ] **Beyaz dikdörtgen ve daireler** #art - odalar 2, 9, 12, 16, 19, 22: bunlar tehlikeler (`MovingDeath` ezicileri, `RotatingDeath` daireleri), Unity'nin varsayılan kare/dairesiyle çizilmiş; sanat mı yapılacak, bilerek mi
- [ ] **GDD §12 karar oturumu** #test - assist modu, hamster topu, multiplayer, toplanabilirlerin amacı, zorluk eğrisi

### Asset ve sanat üretimi

- [ ] **Telifli müzikleri değiştir** #art - örn. "The Skeleton Dance"; yayın öncesi
- [ ] **Spiral node'lar** #art - mekanik yükü az, shader üretmek gerekiyor
- [ ] **İleri zıplama animasyonu** #art - şu anki animasyon yerinde zıplama gibi duruyor
- [ ] **Bounce pad asset'leri** #art - kodda `LaunchPad` olan pedler için görsel üretimi

### Karakterler ve hikâye

- [ ] **Karakter: Egg** #art - yumurta gözlü yeni karakter
- [ ] **Karakter: Dondurma adam** #art - sarı renkli dondurma; fabrikadaki kırmızımsı yeni dondurmalar gibi olmak istiyor
- [ ] **Hikâye seviyesi: Dondurma adamla tanışma** #level - birkaç diyalog duyuyoruz; hikâye seviye içinde anlatılır, oyuncuya zorla verilmez
- [ ] **Hikâye seviyesi: Conveyor belt ve eriyen dondurma** #level - dondurma adam belt üstünde zıplayarak yumurtaya ulaşmaya çalışır, ateş üfleyenler yüzünden yerinde sayar ve erir; o erimeden seviyeyi bitir, erirse oyuncu ölmüş gibi seviye restart olur
- [ ] **Hikâye seviyesi: Missing kağıtları** #level - X-Men Days of Future Past kapağındaki gibi arka planı missing kağıtlarıyla dolu seviye
- [ ] **Hikâye seviyesi: Dondurma fabrikası** #level - dondurma adam üretildiği fabrikada; yukarıdan yeni dondurmalar kapaklarından tutularak geçer (Monsters Inc. kapı sistemi gibi), onlar gibi olmak istediğini söyler
- [ ] **Hikâye seviyesi: Yaşanmamış hayatlar** #level - bir iki odada Egg ve dondurma adamın La La Land finalindeki gibi yaşamadıkları hayatlar
- [ ] **Hikâye seviyesi: Kayıp kedi Astro** #level - bir karakter kaçan kedisini bulmamızı ister; Astro mahsur kaldığı yerden kurtarılır ya da bulunur, yanında hep arkadaş kediler olur
- [ ] **End credits: Dondurma adam ve fotoğraflar** #art - dondurma adam yumurtanın fotoğraflarına kendi şeklini kesiyor

### Yeni seviye fikirleri

- [ ] **Seviye: Reels kaydırma** #level - oyuncu reels bitmeden seviyeyi tamamlar, yoksa sayfa kayarken oyuncuyu öldürür
- [ ] **Seviye: Reklam içeren seviye** #level - ayrıntısı netleşecek

### Yeni mekanik ve sistemler

- [ ] **Swing tutorial iyileştirmesi** #code - oyuncu node'a doğru zıplatılır, havada zaman yavaşlar; önce kolu hareket ettirmeyi, sonra ateşlemeyi öğrenir
- [ ] **Yalnızca odayı gösterme araştırması** #code - Celeste gibi; bazı seviyelerde yan oda görünmesin, gösterilmek istenmeyen yerler karanlık kalsın (komşu odalar şu an aktif kalıyor)
- [ ] **Hareketli node'lar** #code - hareketli platform gibi oyuncuyu taşıyan, fırlatan node'lar (anchor'lar şu an sabit `Vector2?`, node'u takip etmesi gerekir)
- [ ] **Teleport platformlar** #code - DK Bananza'daki gibi; platform parçalarına dokunup onları örneğin alevli kapının üstüne ışınlayarak barikat kurmak

## Done

- [x] ~~**Çift kancadaki çift hareketi düzelt** #code~~ (2026-11-14) - zamanından erken; `DualSwingingState` artık pozisyonu set etmiyor, yalnızca `Velocity` veriyor (`Move` zaten uyguluyor); yay değerleri değişmedi (`DualSpringStiffness` 25, `DualSpringDamping` 5, `DualSwingForceMultiplier` 15) (PF-3)
- [x] ~~**Çift kanca, duvar zıplaması ve dash'i gamepad ile dene** #test~~ (2026-11-14) - zamanından erken; çift kanca, duvar buffer'ı ve dash/ledge climb gamepad ile denendi, sorun yok (PF-3, PF-5, PF-6)

- [x] ~~**Dash süresini fizik adımına bağla** #code~~ (2026-11-14) - zamanından erken; `DashState` sayacı ve çıkışı `FixedUpdateState`'te (10 adım, frame hızından bağımsız), `LedgeClimbState` süre ve konum yazımı da fizik adımında; EditMode 362/362, PlayMode 41/41 yeşil, gamepad ile denendi. `AirborneState` kilit/coyote sayaçları hâlâ `Update`'te
- [x] ~~**Duvar zıplamasını buffer'a bağla** #code~~ (2026-11-14) - zamanından erken; `WallSlidingState` ve `WallClimbingState` `JumpBufferTimer`'ı kullanıyor (0.1 sn), W5 yön kuralları aynı; gamepad ile denendi. "Değmeden mesafe ekle" ve yükselirken duvar zıplaması yapılmadı

- [x] ~~**Duvar düzeltmelerini level 15'te gamepad ile dene** #test~~ (2026-11-14) - zamanından erken; gamepad ile oynandı, duvar yapışma ve hareketli yüzeyden zıplama sorunsuz; zeminden hız aktarımı kaldırıldıktan sonra tekrar denendi (PF-1, PF-2)
- [x] ~~**Tırmanırken duvara yapış, temas kaybını kilitleme** #code~~ (2026-11-14) - zamanından erken; `WallClimbContactTolerance` (0.15) + `WallClingMath.ClosingSpeed`, `PlayerController.TryMeasureWallGap`; tırmanma ve kayma duvara tolerans içinde yapışır, 0.2 sn kilit yalnızca bumper bırakılınca; EditMode 362/362, PlayMode 41/41 yeşil, gamepad ile denendi (PF-2)
- [x] ~~**Hareketli duvardan ayrılınca duvarın hızını aktar** #code~~ (2026-11-14) - zamanından erken; `SurfaceCarry` + `KinematicPhysicsHandler.CurrentCarrier`; duvar/zemin çıkışlarında yüzey hızı (`DeltaPosition/dt`) `AirborneState`'e referans çerçevesi olarak taşınır, boost'lu yüzeyde eski formül kalır. Gamepad denemesi ayrı task'
- [x] ~~**Lobiyi iki pencerede dene** #test~~ (2026-11-14) - üç gün erken; ana editör + sanal oyuncuyla host/katıl, mod değişimi (istemci host'un modunu alıyor), vazgeç (bekleme sırasında dahil), devam et ve uyuşmazlık mesajı doğrulandı. Bulunan iki sorun düzeltildi: varış animasyonu için sahne yüklemesi lobiden gelinirken 1 sn bekliyor, hata plakası iki satıra sarıyor. Geri sayım lobide yok (`RaceHud`'da). **Denenmedi, kullanıcı kararıyla kapatıldı:** gamepad ile gezinme, Steam daveti (MP 0'da), build'de gerçek yükleme süresi'
- [x] ~~**Çok oyunculu lobi: bekleme ve geri sayım** #code~~ (2026-11-14) - iki gün erken; `LobbyScreen`: bekleyen P2 kesik çizgili "?" küp, sağ yarıda kayan soluk sarı bantlar (`UiHazard` shader'ına `_GapAlpha` ve `_Scroll` eklendi); rakip gelince bantlar solar, sağ yarı soldan sağa boyanır, küp düşüp çakılır. **3-2-1 lobiye konmadı:** rakip bağlanınca oyun sahnesi lobinin yerine yükleniyor, geri sayım `RaceHud`'da (karar 2026-11-14). Play Mode'da bekleme ve varış ekran görüntüsü alındı (docs/ui-research/lobby-play/), yay değerleri örneklendi (düşüş ~0,5 sn, boya ~0,9 sn); varış rakip gelmeden `Flow.Phase` yansıtmayla taklit edildi, gerçek iki pencere denenmedi. EditMode 351/351. Gamepad ve gerçek yükleme süresinde animasyonun yarım kalıp kalmadığı denenmedi
- [x] ~~**Çok oyunculu lobi: ana ekran** #code~~ (2026-11-14) - düzen B 12 Kasım'da (`4115526`) kodlanmıştı, bugün B kesin kararıyla kapatıldı; iki yarı, mod sekmesi, ipucu şeridi, gamepad gezinme, hareket. Gamepad ve Steam denenmedi
- [x] ~~**UI'ı farklı çözünürlükte kontrol et** #test~~ (2026-11-14) - bir gün erken; 1440p, 1366×768, 1280×800'de ana menü, oyun HUD'u (ananas sayacı), `RaceHud` (süre, uyarı, 3-2-1) ve pause Play Mode'da çekildi, `ui-review-2026-11-12/res_*.png`. Bulgu: ana menü `ScaleWithScreenSize`'ta eski düzeninde; HUD ve pause 1366×768/1280×800'de ölçek 2'ye düşüp küçülüyor (okunur); `RaceHud` uyarı metni 800p'de küçük ve CRT kenarına yakın. Denenmedi: anahtar halkası (anahtarlı odada), stamina halkası, gerçek yarış verisi (HUD metinleri elle verildi), gamepad mesafesinden okunurluk
- [x] ~~**UI ölçek kuralını uygula** #code~~ (2026-11-14) - bir gün erken; UR-3 (a) varsayıldı (karar kaydı yoktu, öneri ve "başla" talimatı): pause (`PauseMenuPanel.BuildUi`) tam sayı sanat pikseline geçti; ana menü bilerek `ScaleWithScreenSize`'ta bırakıldı (1366×768'de tam sayı ölçek menüyü küçültüp altını boş bırakıyordu; `MainMenuBuilder`'a gerekçe yorumu, `MainMenu.unity` değişmedi). Play Mode: 1080p çarpan 1 (eskisiyle aynı), 1440p 1,25, 1280×800 0,5; görüntüler `ui-review-2026-11-12/ur3_*.png`. 1080p piksel farkı alınamadı (eski hâl yakalanmadı). EditMode 347/347. Pause 800p'de okunur, halka ~%70. Sonradan `RaceHud` (süre, bağlantı uyarısı, 3-2-1) de `UiPixelScale`'e geçti (Play Mode 1440p çarpan 1,25). `MultiplayerLobbyBuilder`'ın eski canvas'ı oyunda kapalı, dokunulmadı. Eski/yeni görsel karşılaştırma (4 çözünürlük): ana menü 1080p piksel piksel aynı; pause 1080p'de yalnızca arka plan şekillerinin 1 px kenarı farklı. 1366×768'de ana menü tam sayı ölçekte eskisinden belirgin küçük, alt ~%40 boş kalıyordu; menü bu yüzden kuraldan ayrıldı (2026-11-14, kullanıcı kararı); ayrılmış menü 1080p'de eskisiyle aynı yerleşimde, kalan fark kanca halkası ve imleç animasyonu
- [x] ~~**Anahtar göstergesini ananastan ayır** #code~~ (2026-11-14) - `HudCounterPanel`: iç içe iki şekil, ikisi bağımsız görünür. İçte ananas çeyrek dairesi (78 → 52 sanat pikseli; 48'de "12" kenara değdi), onu saran koyu mavi dış halka (52–78) anahtarlar için. Ananas toplayınca yalnız çeyrek daire gelir (2,5 sn); anahtarlı odaya girince (8, 11, 20) yalnız halka gelir ve odada açık kalır, anahtarsız odada gizli; anahtar toplamak ananası açmaz. Yalnız anahtar varken ortası boş halka kalır. İlk deneme (üst kenara asılı yarım daire) senin isteğin olmadığı için atıldı. EditMode 342/342; üç durum 1080p'de görüldü (`ui-review-2026-11-12/hud_split_*_1080p.png`). Karakter şeklin altına girince şekil %30 saydamlığa soluklaşır (oda 11'e düşerken karakter halkanın bandındaydı; `hud_fade_behind_1080p.png`).  gamepad ile oynarken, oda 11 ve 20'de ve gerçek anahtar toplamada denenmedi
- [x] ~~**Halftone aşama 2'yi gamepad ile dene** #test~~ (2026-11-14) - iniş, üstünde durma, kancayla havadan geçiş, hızlanan ve dönen platform; kanca hedefleri hâlâ baskın mı, noktalar dikkat dağıtıyor mu; değerleri `RisoSlabSettingsSO`'da ayarla, eski/yeni değeri not et
- [x] ~~**Tüm UI ekranlarını gamepad ile dene, dengeyi ayarla** #test~~ (2026-11-14) - beş ekranın denge değeri 55 uygun mu, ekran başına değiştir (`UiThemeSO`)
- [x] ~~**Yarış HUD'unu iki pencerede gamepad ile dene** #test~~ (2026-11-14) - bayrak alma, 3/3 ceza (rakip bir tik geri), aynı odada üst üste binme yok, pencere bölünmesi (fark > 10 için oda sayısını geçici artır), okunurluk; hareket hissini ayarla. 13 Kasım: iki pencerede (host + Player 2) gerçek oturumla doğrulandı (kimlik renkleri, aynı odada binme yok, bayrak/3-3 ceza zinciri, fark > 10 bölünme); kalan: gamepad ile his ve okunurluk, bayrak alırken gerçek bayrağa dokunma
- [x] ~~**Halftone aşama 2: mürekkep değişimi dalgası** #code~~ (2026-11-13) - `_ShiftFront`/`_ShiftOrigin`/`_ShiftBand`, `RisoSlabVisual` iniş anında halkayı başlatır (6 birim/sn, bant 1.2), iz geçmişi halka durumunu kaydeder: sarı iz gecikmeli dalgada pembe doğar; testli, oyunda denenmedi
- [x] ~~**Halftone aşama 2: oyuncu temasını platforma ilet** #code~~ (2026-11-13) - `RisoSlabVisual` temas algısı (zemin/duvar), `RisoSlabMath.StepInfluence` + testler; canlı denenmedi
- [x] ~~**Halftone aşama 2: nokta çekimi ve büyümesi (shader)** #code~~ (2026-11-13) - `RisoSlab.shader` `DotCell`/`InkDots`: yarıçap içinde çekim 0.4P ve büyüme 0.2P, 3x3 komşu hücre; ekran görüntüsüyle doğrulandı, oyunda denenmedi
- [x] ~~**Halftone H1: CRT 960×540'ı tek sahnede dene #test**~~ (2026-11-13) - geliştirici denedi, karar verildi
- [x] ~~**Halftone H2: platform shader'ı, şekil + çizgi + kâğıt #code**~~ (2026-11-13) - `RisoSlab.shader`
- [x] ~~**Halftone H3: üç mürekkep katmanı ve gren #code**~~ (2026-11-13) - üç mürekkep katmanı, gren, temiz şerit, ince platform bantları
- [x] ~~**Halftone H4: RisoSlabVisual, hıza bağlı kayık baskı #code**~~ (2026-11-13) - `RisoSlabVisual`, `RisoSlabSettingsSO`, `RisoSlabMath` + EditMode testi
- [x] ~~**Halftone H5: gecikmeli renk izi emitter'ı #code**~~ (2026-11-13) - `HalftoneTrailEmitter`, `HalftoneTrailLogic` + EditMode testleri
- [x] ~~**Halftone H6: iz parçacık shader'ı ve prefab'lar #code**~~ (2026-11-13) - `RisoDot.shader`, beş hareketli platform prefab'ı, platforms.md "Görünüm"
- [x] ~~**Halftone H7: platformları gamepad ile dene #test**~~ (2026-11-13) - geliştirici denedi, beğenildi
- [x] ~~**RaceHud'u pist + merceğe taşı** #code~~ (2026-11-13) - `RaceTrackView` (pist, yerel üçgen/rakip daire, 3'er flama, mercek bölünmesi `/\/\`; minimap ve fark yazısı kullanıcı isteğiyle çıkarıldı, ceza bandı + sarsıntı, rakip sıradaki flama nabzı) `RaceHud`'a bağlandı; eski metinler çizilmiyor. Play Mode'da 22 ve 100 odalık durumlarla ekran görüntüsü alındı (tek pencere, bölünme, ceza bandı); ekranı kaplayan gri plaka kullanıcı isteğiyle kaldırıldı. Gerçek iki pencereli oturumda ve gamepad ile denenmedi (sıradaki #test task'ı). EditMode 304/304
- [x] ~~**Pist penceresi hesabı (saf mantık)** #code~~ (2026-11-13) - `RaceTrackWindow.Compute` (14 odalık tek pencere, ortalı, uçlara yaslı; fark > 10 ise 6'şar odalık iki parça + `HiddenRooms`) ve `GapText`; `RaceTrackWindowTests` 10/10, EditMode 304/304. Bağlanmadı, `RaceHud` bir sonraki görevde kullanacak
- [x] ~~**Yarış HUD'u: renk kuralı ve 2/3 uyarısı** #art~~ (2026-11-13) - renk kimlikten (host mavi, istemci kırmızı); ceza bandı kırmızı değil Ink/Yellow şeritli; rakibin alacağı sıradaki boş flama nabız atar (yerelde yok, ses/titreşim yok). Kararlar docs/ui-research/race-hud-track.md'de; nabız hızı gamepad ile ayarlanacak
- [x] ~~**Oda kartı ile iris geçişi çakışmasını dene** #test~~ (2026-11-12) - bir gün erken; kart yalnızca oyuna girişte çıkar, iris (`TransitionManager`) ölüm/respawn ve MP cezada çalışır; çakışma denemesi insan tarafından yapıldı, `startDelay` değişikliği bildirilmedi (0 kaldı varsayıldı)
- [x] ~~**Çok oyunculu sonuç ekranını yeniden çiz** #code~~ (2026-11-12) - bir gün erken; `RaceResultScreen` (1. oyuncu = host mavi silindir, 2. oyuncu kırmızı küp; sütun yüksekliği süre oranı, kaybeden en az %45; rakip bitirmediyse "RAKİP …" plakası, birlikte modunda eşit boy) + saf `RaceResultMath`; `RaceHud` ekranı sürer, eski metin (`ResultText`, `LastExitReason`) veri olarak kaldı. Play Mode'da 1080p/720p ekran görüntüsüyle (kazan/kaybet/bekle) ve iki pencerede (host + Player 2) gerçek oturumla doğrulandı: host bitirince "Bitirdin! / RAKİP …", istemci bitirince host "Kazandın!", istemci "Kaybettin." (bitiş çizgisine koşulmadı, `OnLocalFinish` yansıtmayla tetiklendi; `docs/ui-research/mp-result-play.png`). Gamepad hissi ve yay değerleri denenmedi. EditMode 280/280
- [x] ~~**Oda başlık kartı** #code~~ (2026-11-12) - bir gün erken; `RoomTitleCard` (levha, kiriş, ekstrüde numara, şerit; `UiSpring`, DOTween yok), oda geçişinde değil oyuna girişte bir kez çıkar (kayıtlı yarışa devamda kayıtlı oda); `Room.RoomTitle` alanı, `RoomCardMath` EditMode testli, `NewScene`'e bağlı. Bekleyen: gamepad ile his denemesi, yarış kaydıyla devam denemesi (docs/systems/room-title-card.md)
- [x] ~~**HUD'ı gamepad ile dene** #test~~ (2026-11-12) - kullanıcı gamepad ile denedi, bir gün erken; bulgu notu verilmedi
- [x] ~~**Pause'u gamepad ile dene** #test~~ (2026-11-12) - kullanıcı gamepad ile denedi; bulgu notu verilmedi
- [x] ~~**Ana menüyü gamepad ile dene** #test~~ (2026-11-12) - kullanıcı gamepad ile denedi, bir gün gecikmeyle; bulgu notu verilmedi
- [x] ~~**Kırılan stamina halkası** #code~~ (2026-11-12) - `HudStaminaRing`: karakter etrafında 6 sarı beton parça, enerji 1/6 azalınca son parça kopup fırlar ve söner, son 1,5 sn kalanlar yanıp söner (Someday'deki "Dayanıklılık uyarısı ekle" de bununla kapandı; uyarı görsel, titreşim yok); Overlay + `UiPixelScale`, eski World Space `StaminaCanvas` `PlayerUIController.Awake`'te kapatılıyor (sahnede duruyor) (docs/systems/wall-movement.md). Play Mode'da kopma ekran görüntüsüyle görüldü; yanıp sönme ve geri toplanma görülmedi, gamepad hissi ve boyut denenmedi. EditMode 267/267, PlayMode 41/41. Kod commit'lenmedi
- [x] ~~**HUD: çeyrek daire sayaç** #code~~ (2026-11-12) - asıl iş 2026-11-09'da yapılmıştı, task açık kalmıştı: `HudCounterPanel` (NewScene `Managers/HudCounter`), sol-üst mavi çeyrek daire, ananas + sayı ve odanın anahtarları; karar olay güdümlü (toplayınca görünür, `showSeconds` sonra çekilir); eski `CollectibleUIController` (Coin UI) bağlanmadı, NewScene'de kapalı; Overlay + `UiPixelScale` (docs/systems/collectibles.md). Bugün UR-1/UR-2 düzeltmeleri eklendi. HUD kodu, `UiPixelScale` ve `HudCounterTests` henüz commit'lenmedi
- [x] ~~**Pause arka planı** #code~~ (2026-11-12) - zaten 1e7ec07'de yapılmıştı, task açık kalmıştı: `PauseBackdrop` (dither + %50 ink karartma `UiDither` shader'ıyla, kırmızı/mavi daire, `PolygonMath.CircleLens` ile mor kesişim, sağ-alt sarı üçgen, dilime göre poz) (docs/systems/pause.md). 2026-11-12 incelemesinde (UR-7) kullanıcı bugünkü hâlini onayladı; yeni kod yok
- [x] ~~**HUD düzeltmeleri: ölüm aboneliği ve ilk oda** #code~~ (2026-11-12) - iki gün erken; `HudCounterPanel`: `OnDeath` aboneliği `OnEnable`'da (çift aboneliğe karşı önce `-=`, oyuncu yoksa `Start`'ta tekrar), abonelikten sonra oyuncunun durduğu oda `RoomBounds.OverlapPoint` ile bulunup anahtarlar dolduruluyor (`LocalPlayerBroadcaster` mantığının HUD kopyası, Multiplayer bağımlılığı yok) (UR-1, UR-2). EditMode 255/255. Play Mode'da panel kapat-aç + ölüm ve oda 8 başlangıcı denenmedi
- [x] ~~**Ana menü: options soluk görünsün** #code~~ (2026-11-12) - üç gün erken; `UiMenuMotion`: pasif satır saydamlığı 1 → `labelDim * 0.5` (varsayılan 0,35), lobideki pasif satırlar için de geçerli (UR-5). Ana menü Play Mode ekran görüntüsünde options diğer satırlardan açıkça soluk. EditMode 255/255. Gamepad ile atlanma denenmedi
- [x] ~~**Radyal pause: dilim seçimi** #code~~ (2026-11-09) - iki gün erken; `PauseMenuPanel` + `RadialMenuSelection`: sol stick (`atan2`) ve D-pad dilimi seçer, LB/RB döndürür, A onaylar, B devam; Options soluk ve atlanıyor (ayar ekranı yok), Retry = `PlayerHealth.Kill()` (son checkpoint), Quit = ana menü; giriş animasyonu yeniden yazıldı, kullanıcı beğendi (docs/systems/pause.md). Play Mode'da sanal gamepad ile denendi, gerçek gamepad hissi denenmedi, boşta hareket/tık sesi/titreşim yok. EditMode 230/230
- [x] ~~**HUD ikon seti** #art~~ (2026-11-09) - iki gün erken; sekiz 24x24 piksel ikon (ananas, anahtar, dash küpü, sling silindiri; boyalı = renk, `_used` = düz beton grisi), brutalist: ink çizgi, sert gölge, yüzler arası kenar; SVG yerine piksel seçildi, `Assets/UI/Icons/` (docs/ui-research/hud-icons.md). Anahtar mockup'ta disk sırası, burada silüet: karar açık. Oyunda ve gamepad mesafesinde okunurluk denenmedi, HUD'a bağlanmadı
- [x] ~~**Ana menüyü yeniden kur** #code~~ (2026-11-09) - iki gün erken; mockup'taki menü: eğik ekstrüde başlık, liste, sarı levha + halka imleci, mavi boyalı halkalı delik (`MainMenuBuilder`, `UiExtrudedText`, `UiSelectionVisual`). `options` etkileşimsiz (ayar ekranı yok), gezinmede atlanıyor. Play Mode'da görüldü, gerçek gamepad denenmedi (docs/ui-research/main-menu.md). EditMode 215/215
- [x] ~~**Overlay canvas CRT dışında mı doğrula** #test~~ (2026-11-08) - iki gün erken; Overlay CRT dışında (test paneli tam `#FC1E00`, 1 renk), Coin UI (Camera canvas) ve StaminaCanvas (World Space) CRT içinde. Ana menü düz ama sahnede `Volume` yok, tek başına kanıt değil. Lobi ve RaceHud yakalanmadı (docs/ui-research/crt-check.md, görüntüler crt-check/). Çıkarım: sayaç ve stamina task'larında Camera/World Space'ten Overlay'e taşıma kararı verilecek
- [x] ~~**Beton yüzey shader'ı ya da dokusu** #art~~ (2026-11-08) - iki gün erken; seçilen: (a) kodla üretilen uGUI shader'ı (`RockTheGame/UI/Concrete` + `UiConcreteSurface`; benek, kalıp çizgisi, tie-hole denge eşikleriyle; uGUI Canvas hedefi doğrulandı: UV'siz `UiArc`'ta da çalışıyor). (b) CC0 doku denenmedi, kullanıcı indirmeyi istemedi; (a) ölçülerek değil gerekçeyle seçildi (docs/ui-research/concrete-surface.md). 1080p'de gözle bakılmadı, hiçbir ekran henüz kullanmıyor. EditMode 213/213
- [x] ~~**UI fontlarını hazırla** #art~~ (2026-11-08) - iki gün erken; Jost 800 (başlık) ve Space Mono Regular/Bold (mono), ikisi de OFL 1.1, TMP SDF statik atlas (205 karakter, Türkçe dahil), `UiThemeSO`'ya atandı; Tiny5'e dokunulmadı (docs/ui-research/fonts.md). Play Mode'da Türkçe harfler çizildi, küçük metin 1080p'de gözle bakılmadı. OFL metinleri `Assets/UI/Fonts/Licenses/`; yayın öncesi krediye eklenmeli. EditMode 210/210
- [x] ~~**UiThemeSO** #code~~ (2026-11-08) - iki gün erken; palet, beton tonları, fontlar, çerçeve, gölge, ekstrüzyon ve denge (0.55, ekran başına override) `Resources/UI/UiTheme.asset`'te, mockup formülleri `UiBalance`'ta (docs/ui-research/palette.md "Kodda"). Fontlar boş (font task'ı atayacak), henüz hiçbir ekran okumuyor. EditMode 209/209
- [x] ~~**UI paletini kaynak PNG'lerden örnekle** #art~~ (2026-11-08) - bir gün erken; kâğıt `#F1E6C7`, kırmızı `#FC1E00`, sarı `#FCAE04`, mavi `#597DCE`, ink `#140C1C` kaynak piksellerden (boru `#433646` hiçbir sprite'ta yok, `#442434`/`#4E4A4E`), docs/ui-research/palette.md. Beton tonları kaynakta yok: karar sende (mockup'unki / türetilmiş / soğuk taş). Mockup güncellenmedi
- [x] ~~**UI şekil çizim yöntemini seç** #code~~ (2026-11-08) - bir gün erken; karar: statik şekiller PNG 2x, dinamik halka/dilim/çokgen için kendi `UiArc` mesh bileşeni (hazır paketlerin hiçbiri Unity 6'da doğrulanmadı, lisans/atıf yükü yok), docs/ui-research/shape-method.md. Play Mode'da şekiller çizildi; kenar yumuşatma 1080p'de gözle doğrulanmadı, UV yok (beton shader task'ı için not). EditMode 201/201
- [x] ~~**Pause sistemini yaz** #code~~ (2026-11-08) - bir gün erken; Start duraklatır/devam ettirir (`PauseManager`, `timeScale 0`, ses ve titreşim kesilir), çok oyunculu yarışta kapalı, müzik çalmaya devam ediyor ve boğuk/uzak bir efekt alıyor (Lowpass + Reverb, `PauseAudioSettings.asset`) (docs/systems/pause.md). Sanal gamepad ile Play Mode'da doğrulandı; gerçek gamepad, his ve efektin kulakla ayarı denenmedi (Pause'u gamepad ile dene task'ı). EditMode 194/194, PlayMode 41/41
- [x] ~~**MP 9: Steam taşıyıcısı** #code~~ (2026-11-07) - Facepunch.Steamworks 2.5.2 ayrı assembly'de, Steam relay taşıyıcısı, lobide Steam daveti; Steam yoksa IP'ye düşüyor (build'de doğrulandı). Ayrıca yarış bitişi ve sonuç ekranı (iki pencerede doğrulandı). Steam bağlantısı iki bilgisayarda denenmedi (MP 0). EditMode 189/189, PlayMode 41/41
- [x] ~~**MP 8: Yarış kaydı, devam etme, kopma** #code~~ (2026-11-06) - yedi hafta erken; `multiplayer-race.json`, lobide Devam et (`Hello.Resume`, protokol v3), kopunca donma + 30 sn yeniden bağlanma + 3-2-1, dönülmezse kayıt ve lobi. İki pencerede kopma → bekleme → lobi → Devam et ile kendi odalarından sürdürme doğrulandı; 30 sn içinde geri dönüş elle denenemedi (Player 2'nin Play'e girişi uzun sürdü). EditMode 178/178, PlayMode 41/41
- [x] ~~**MP 7: Lobi sahnesi ve multiplayer HUD** #code~~ (2026-11-06) - altı hafta erken; `MultiplayerLobby.unity` (build index 1), host/katıl, modu host seçer, 3-2-1 geri sayım (`Ready`, protokol v2), HUD; `MultiplayerSession` başlatıcının yerine geçti. İki pencerede lobiden yarışa ve istemcinin 3 bayrağıyla host'a ceza ağ üzerinden doğrulandı (MP 5–6'nın iki pencere kontrolü de); başlama farkı editörde 0.4 sn. EditMode 164/164, PlayMode 41/41
- [x] ~~**MP 6: Ceza akışı** #code~~ (2026-11-06) - beş hafta erken; `PenaltyHandler` respawn dizisiyle önceki odanın checkpoint'ine götürüyor, kamera ve oda geçiyor, sonraki ölüm orada doğuyor; oda 22 → 11; respawn/geçiş/ölüm sırasında bekliyor. EditMode 151/151, PlayMode 41/41 (5 yeni ceza testi); Play'de tek pencerede doğrulandı, iki pencerede ağ üzerinden denenmedi
- [x] ~~**MP 5: Bayrak işaretleri** #code~~ (2026-11-06) - dört hafta erken; `RoomNew-22` ID 22, 22 odaya `RaceFlagPoint` kondu (menü komutu, yatay çıkışta zemine iner), yarış modunda dokununca kalkan bayrak; tek pencerede gerçek trigger'la doğrulandı (sayaç 1, 2, 0), iki pencerede denenmedi; EditMode 144/144, PlayMode 36/36
- [x] ~~**MP 4: Hayalet** #code~~ (2026-11-06) - üç hafta erken; iki pencerede rakip yarı saydam hayalet olarak görünüyor, takip ediyor, uzak odada gizleniyor (ajan iki pencereyi CLI ile sürerek doğruladı); EditMode 120/120, PlayMode 36/36. Gamepad iki pencerede hâlâ denenmedi
- [x] ~~**MP 3: Unity Transport taşıyıcısı** #code~~ (2026-11-06) - iki hafta erken; UTP adaptörü (gerçek localhost testleriyle) ve editör için geliştirici başlatıcısı; EditMode 100/100, PlayMode 36/36; MP 1–3 bir akşamda bitti, NGO'ya geçiş koşulu oluşmadı. Gamepad'in iki pencerede davranışı denenmedi
- [x] ~~**MP 2: Yarış kuralları (`RaceSession`)** #code~~ (2026-11-06) - planlanandan bir hafta erken; gözden geçirmeden sonra ceza hedefi uygulama anında hesaplanıyor, ceza hep gönderiliyor, rakip sayacı 3'te sıfırlanıyor; EditMode 84/84
- [x] ~~**MP 1: Assembly'ler, taşıyıcı arayüzü, bellek içi taşıyıcı, mesajlar** #code~~ (2026-11-06) - `feature/multiplayer` dalında 4 commit; EditMode 60/60, PlayMode 36/36; gözden geçirmenin 6 küçük bulgusu ertelendi (docs/systems/multiplayer.md)
- [x] ~~**Multiplayer mod kararı ve ağ yaklaşımı** #code~~ (2026-11-06) - hayalet rakip, yarış + birlikte, Steam; kendi taşıyıcı arayüzü (docs/systems/multiplayer.md)
- [x] ~~**Kullanılmayan asset klasörleri** #art~~ (2026-11-06) - karar: silinmeyecek, ileride işe yarayabilir (Pixel Adventure 1, Tiles/Zone-3, Tiles/Background, Sprites/)
- [x] ~~**Oda 9 dikenleri + Play Mode görselleri** #test~~ (2026-11-06) - oda 9 dikenleri oyun kamerasında okunuyor; oyuncu ve HUD doğru katmanda (bulgular docs/systems/hazards.md)
- [x] ~~**Renk uzayı kararı (Gamma/Linear)** #art~~ (2026-11-06) - karar: Gamma kalıyor; 2D ışık eklenirse yeniden değerlendirilir
- [x] ~~**Sorting Layer 2: arka plan, VFX, dekor** #art~~ (2026-11-06) - asit parçacıkları VFX'e, dekor (gnome, iskelet, duş borusu, yatan adam) Background'a; tehlikeler, node'lar, yazılar Default'ta kaldı
- [x] ~~**Sorting Layer 1: katmanları tanımla, oyuncu ve platformlar** #level~~ (2026-11-06) - Background → Default → Platform → Player → VFX → UI; oyun içinde gözle kontrol bekliyor
- [x] ~~**"Bozulmamalı" davranışlar listesini yaz** #test~~ (2026-11-05) - `docs/bozulmamali-davranislar.md`; testler henüz yazılmadı
- [x] ~~**Kullanılmayan modül paketlerini kaldır** #code~~ (2026-11-05) - 8 modül kaldırıldı; `terrain` kaldı, çünkü `render-pipelines.core` ona bağlı
- [x] ~~**Animator 'Hash 0' uyarısını teşhis et** #test~~ (2026-11-05) - `GnomePukeAnimator` hash'i `OnEnable`'dan sonra hesaplıyordu; `Awake`'e alındı, uyarı kalktı
- [x] ~~**Ses mixer parametrelerini expose et** #test~~ (2026-11-05) - dört parametre expose edildi; boş `Ambient` grubu eklendi, henüz hiçbir ses ona bağlı değil
- [x] ~~**Ping-pong stratejilerinde period=0 NaN düzeltmesi** #code~~ (2026-11-05) - `period <= 0` iken başlangıç noktasında kalır, EditMode testleri eklendi
- [x] ~~**Git geçmişindeki şirket içi doküman izine karar ver** #code~~ (2026-11-05) - karar: geçmiş olduğu gibi kalıyor, yeniden yazılmayacak
