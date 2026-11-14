# Player FSM incelemesi — 2026-11-14

Kapsam: `Assets/Scripts/New-Scripts/Player/` (`PlayerController`, `KinematicPhysicsHandler`, `States/*`), ilgili platform sürücüleri (`PlatformController`, `WrappingPlatformManager`), `HitStopManager`.
Yöntem: kod okuma + Editor'de sayısal doğrulama (`unity command eval` ile sahnedeki gerçek collider'lar üzerinde sensör taraması) + geçici bir PlayMode teşhis testi (level 15 kurulumunun kopyası, FakeInput ile). Teşhis testi silindi; bu incelemede kod, sahne ya da asset değişmedi.

Çıkış noktası: geliştirici "tırmanırken bir iki frame tutunmuyormuş gibi" hissediyor, özellikle **level 15**'te (`RoomNew-15`) hareketli duvarlara tutunarak geçilen bölümde.

Önem: Yüksek / Orta / Düşük / **Karar** (oyun hissini değiştirir; değer ya da davranış seçimi geliştiricinin, CLAUDE.md kural 4).
Durum: **Doğrulandı** (ölçüldü ya da yeniden üretildi) / **Kodda görüldü** (okumayla kesin, ölçülmedi).

## Özet

| ID | Başlık | Önem | Durum |
|---|---|---|---|
| PF-1 | Hareketli duvardan ayrılınca duvarın hızı aktarılmıyor (level 15'teki kopma) | Yüksek + Karar | Doğrulandı |
| PF-2 | Tırmanırken duvara yapışma yok; temas kaybı "bumper bırakıldı" gibi kilitleniyor | Yüksek | Doğrulandı |
| PF-3 | Çift kancada karakter her adımda iki kez hareket ediyor | Yüksek + Karar | Kodda görüldü |
| PF-4 | Havadaki hız duvar çarpmasını öğrenmiyor (momentum duvarın içinden korunuyor) | Orta + Karar | Kodda görüldü |
| PF-5 | Duvar zıplaması jump buffer'ı kullanmıyor | Orta | Kodda görüldü |
| PF-6 | Dash süresi `Update`'te sayılıyor; ledge climb Rigidbody'yi `Update`'te taşıyor | Orta | Kodda görüldü |
| PF-7 | Hitstop yalnızca oyuncuyu donduruyor, platformlar akmaya devam ediyor | Karar | Kodda görüldü |
| PF-8 | Duvara tutunma toleransı çok dar; `WallSnapRaycastDistance` pratikte ölü | Karar | Doğrulandı |
| PF-9 | Küçükler | Düşük | Kodda görüldü |

Elenen şüpheler (tekrar araştırma, sebebi yazıyor):
- **Collider offset / scale:** Player `BoxCollider2D` offset (0,0), size (1,1), scale 1. Snap hesabı doğru.
- **Düz duvarda sensör güvenilirliği:** sahnedeki tüm Ground composite'lerinde 17 345 snap pozunda sağ/sol `BoxCast` (0.04) hiç ıskalamadı; snap sonrası cast mesafesi 0.015 (polygon radius ~0.005).
- **Platform/oyuncu çalışma sırası, sabit hızda:** `WrappingPlatformManager`'da `DefaultExecutionOrder` yok (oyuncuyla aynı, 0), `PlatformController`/`TriggeredPlatformController` −100'de. Sabit hızda `DeltaPosition` her adım aynı olduğundan sıra taşımayı bozmuyor: PlayMode'da hareket hâlindeki duvarı havadan yakalayıp tırmanmak 60 adımda 0 kopma verdi (duvar uzaklaşırken de).

---

## PF-1 — Hareketli duvardan ayrılınca duvarın hızı aktarılmıyor

**Kurulum (level 15):** `DynamicContent/WrappingPlatformManager-Ready`, beş adet 1×17 `MovingPlatform/Square` (`PlatformController`, strateji yok), `leftBound` x=559.5 → `rightBound` x=459.5, hız 4–4 birim/sn (adım başına 0.08), sonda başa ışınlanır (`TeleportTo`).

**Ne oluyor:**
- Tırmanırken zıplama `new Vector2(0f, stats.ClimbVerticalJumpVelocity)` verir ([WallClimbingState.cs:126](../../Assets/Scripts/New-Scripts/Player/States/WallClimbingState.cs)). Yüzeyin hızı yalnızca `JumpBoostMultiplier > 0` ise eklenir (:132-136). `PlatformController.JumpBoostMultiplier` strateji yoksa 0 döner, yani level 15 duvarlarında hiç hız eklenmez.
- `AirborneState` taşınmaz (`ClingingWallDirection = 0`, [KinematicPhysicsHandler.cs:83-94](../../Assets/Scripts/New-Scripts/Player/KinematicPhysicsHandler.cs)). Karakter dünyada yerinde zıplar, duvar 0.08/adım uzaklaşır.
- Tekrar tutunma `IsTouching*Wall` ister (sensör 0.04, [KinematicPhysicsHandler.cs:194](../../Assets/Scripts/New-Scripts/Player/KinematicPhysicsHandler.cs)), üstüne 0.2 sn kilit var. Kilit bittiğinde duvar 0.8 birim uzaktadır.
- Duvarın **önündeki** yüzde (duvar oyuncuya doğru gelirken) duvar oyuncuyu ittiği için tekrar tutunulur; **arkadaki** yüzde kaybedilir. "Bazen" hissi buradan.

**Kanıt (PlayMode, level 15 kopyası):** uzaklaşan duvarda tırmanırken zıplama, bumper basılı, stick yukarı ya da nötr: 40 adımın 40'ı `Airborne`, boşluk 0.08, 0.16, 0.24 … 0.8 sn sonunda 2.4 birim. Hiç tekrar tutunulmadı.

Aynı kusur aynı nedenle şu çıkışlarda da var: bumper bırakma (`WallClimbingState` :155), dayanıklılık bitişi (:67), kayarken zıplama ve stick bırakma (`WallSlidingState` jump :101-133, release :59, süre bitişi), yerden zıplama (`GroundedState` :85-105) ve yürüyüp düşme (:112). Bunların hepsi oyuncunun **çarpışma sonrası** hızını ya da sabit bir vektörü kullanıyor; taşıyan yüzeyin hızını hiçbiri eklemiyor.

**Önerilen düzeltme (Karar):**
1. Önce `docs/systems/wall-movement.md` satır 39'u ("`JumpBoostMultiplier > 0` ise duvarın hızı zıplamaya eklenir") ve `docs/systems/platforms.md`'deki ilgili cümleyi yeni kurala göre güncelle: *hareketli yüzeyden ayrılırken yüzeyin o anki hızı ×1 aktarılır; `JumpBoostMultiplier` bunun üstüne ekstradır.* Kullanıcı onayı al.
2. Taşıyan yüzeyin hızını tek bir yerden ver (örn. `KinematicPhysicsHandler`'da `CarrierVelocity`: yerdeyse `CurrentMovingSurface`, `ClingingWallDirection` −1/1 ise ilgili duvar yüzeyi; `DeltaPosition / Time.fixedDeltaTime`). `SurfaceVelocity`'yi **kullanma**: o son 3 adımın en büyüğünü döndürür, boost için tasarlanmış.
3. Yukarıdaki çıkışlarda `AirborneState`'e verilen hıza bu değeri ekle. En küçük his değişikliği için: `JumpBoostMultiplier > 0` olan yüzeylerde mevcut formül aynen kalsın (zaten yüzey hızı ekleniyor), yalnızca boost'u 0 olan yüzeylerde taban hız eklensin. Statik duvar ve zeminde değer 0 olduğundan W5 ("tırmanırken zıplamak düz yukarı") duvara göre korunur.

**Bitti sayılır:**
- Derleme temiz, EditMode yeşil, PlayMode `WallRulesTests` dahil yeşil (`unity command run_tests --mode playmode --async_tests true`).
- Doğrulama tarifi (aşağıda "Teşhis testi tarifi") H/I senaryosunda karakter zıpladıktan sonra duvarla birlikte gider ve 0.2 sn sonra bumper basılıysa tekrar `WallClimbingState`'e girer.
- Eski/yeni davranış raporlandı, geliştiriciden level 15'te gamepad denemesi istendi.

---

## PF-2 — Tırmanırken duvara yapışma yok; temas kaybı kilitleniyor

**Ne oluyor:**
- Snap yalnızca girişte bir kez yapılıyor ([WallClimbingState.cs:32-41](../../Assets/Scripts/New-Scripts/Player/States/WallClimbingState.cs)). Sonra her adımda `Velocity = (0, inputY * ClimbSpeed)` (:108): X sabit, duvara doğru hiç itme yok.
- Duvar yüzeyi ~0.03'ten fazla geri çekilirse (misaligned collider, ya da hareketli duvarda tek bir taşıma adımı kaçarsa 0.08) sensör kopar.
- `isHoldingCurrentWall` (:150-156) iki farklı durumu birleştiriyor: *bumper bırakıldı* ve *bumper basılı ama temas koptu*. İkisi de `AirborneState(Vector2.zero, wallClimbLockout: 0.2f)`. Dokümandaki 0.2 sn kilit ([wall-movement.md:19](../systems/wall-movement.md)) yalnızca bırakma için tanımlı.
- `WallSlidingState` aynı durumda: stick duvara itse de X'e yansımıyor ([WallSlidingState.cs:63](../../Assets/Scripts/New-Scripts/Player/States/WallSlidingState.cs)), temas `IsStillOnWall` (:70) ile kopar.

**Kanıt:**
- Statik geometri: sahnede 12 508 simüle tırmanışın 27'sinde duvar devam ederken sensör koptu (geri çekilme 0.045–0.075). Noktalar `Square` ve `LaunchPad` collider'larında, örn. (338.52, 128.6), (338.52, 135.8), (605.52, 273.6).
- Hareketli duvar: platform tutunduktan sonra harekete başlarsa (ilk adımda `DeltaPosition` = 0) karakter 2 adımda düşüyor ve bir daha yakalayamıyor. Toparlanma payı yok: 0.08 > 0.04.

**Önerilen düzeltme:**
1. `PlayerStatsSO`'ya `WallClimbContactTolerance` ekle (başlangıç 0.15; adım başına 0.08'lik bir taşıma kaçağını karşılar). Sabit kodlama yok.
2. `WallClimbingState.FixedUpdateState`: `wallDirection` yönüne collider merkezinden tolerans kadar ray at. Duvar bulunursa ve boşluk `WallSnapSafetyOffset`'ten büyükse, X hızını boşluğu bu adımda kapatacak değere ayarla (`(gap - offset) / fixedDeltaTime * dir`), böylece `KinematicPhysicsHandler` çarpışmayı kendisi çözer. Doğrudan `rb.position` yazma (Interpolate açık).
3. `CheckInputTransitions`'ı ikiye ayır: bumper bırakıldı → bugünkü gibi 0.2 sn kilit. Bumper basılı ama tolerans içinde duvar yok → `AirborneState(..., wallClimbLockout: 0f)`, böylece duvar geri gelince hemen tutunulabilir.
4. `WallSlidingState` için de aynı tolerans/yapışma (stick duvara itiyorsa).
5. `docs/systems/wall-movement.md`: yeni stat'ı tabloya, yapışma ve "temas kaybı kilitlemez" kuralını metne ekle, "Son kod doğrulaması" tarihini güncelle.

**Bitti sayılır:** derleme temiz, EditMode + PlayMode yeşil; teşhis testi tarifinin A senaryosu (platform tutunduktan sonra harekete başlar) artık düşmüyor; yukarıdaki üç statik nokta tırmanılabiliyor (eval ile tekrar tara). Bu davranış için yeni bir PlayMode kural testi önerilebilir, ama oyun-kritik test yazımını insan onaylar (CLAUDE.md kural 3).

---

## PF-3 — Çift kancada çift hareket

[DualSwingingState.cs:128-129](../../Assets/Scripts/New-Scripts/Player/States/DualSwingingState.cs): `PlayerRigidbody.position = playerPos + currentVelocity * dt` ve ardından `context.Velocity = currentVelocity`. `PlayerController.FixedUpdate` ([PlayerController.cs:163](../../Assets/Scripts/New-Scripts/Player/PlayerController.cs)) aynı hızla `physicsHandler.Move(Velocity * dt)` çağırır: karakter adım başına `2·v·dt` gider. İlk yarı yalnızca 0.4 yarıçaplı `CircleCast` ile denetleniyor (:107), gövde 1×1 kutu, yani köşelerde duvara girebilir; çarpışma çözümü sadece ikinci yarıya uygulanır.

(`SwingingState` farklı: orada `rb.position` yazımı ip uzunluğuna izdüşüm, teğet hareketi `Move` yapıyor. Bu bilinçli bir kısıt izdüşümü, hata değil.)

**Düzeltme:** `rb.position` yazımını kaldır, yalnızca `context.Velocity = currentVelocity` bırak; çarpışmayı `Move` çözsün. Yüzey açısına göre Grounded/Airborne kararı için mevcut cast kalabilir ama oyuncu kutusuyla (`SwingingState.CheckSweepCollision` gibi `BoxCast`).
**Karar:** etkin hız yarıya iner. `DualSpringStiffness` / `DualSwingForceMultiplier` / `SwingGravity` yeniden ayar ister. Eski/yeni değerleri yaz, gamepad denemesi iste.
**Bitti sayılır:** derleme temiz, EditMode + PlayMode (`SwingRulesTests`, `SlingshotRulesTests`) yeşil.

## PF-4 — Havadaki hız duvar çarpmasını öğrenmiyor

`AirborneState` kendi `_currentVelocity`'sini tutar, `context.Velocity`'ye yazar ([AirborneState.cs:74](../../Assets/Scripts/New-Scripts/Player/States/AirborneState.cs)) ama `Move`'un döndürdüğü çözülmüş hızı hiç geri okumaz. Tavan için ayrı kontrol var (`HandleVerticalObstructions`), duvar için yok. Örnek: swing'den 25 hızla duvara çarp (`SwingingState.BreakGrapple` → `AirborneState(_currentVelocity)`), duvar boyunca kay; duvar bittiği an saklı hızla fırlarsın (`AirDrag` 20/sn ile yavaş söner).
**Karar:** bu bir "köşe boost'u" olarak istenebilir. Geliştirici seçsin: (a) duvara çarpınca o eksendeki hız sıfırlansın (`IsTouchingLeft/RightWall` ve hız işaretine göre, tavandaki gibi), (b) korunsun ve dokümana yazılsın.
**Bitti sayılır:** karar `docs/systems/movement-core.md`'de; (a) seçildiyse derleme + testler yeşil.

## PF-5 — Duvar zıplaması jump buffer'ı kullanmıyor

[WallClimbingState.cs:119](../../Assets/Scripts/New-Scripts/Player/States/WallClimbingState.cs) ve [WallSlidingState.cs:101](../../Assets/Scripts/New-Scripts/Player/States/WallSlidingState.cs) yalnızca `Input.IsJumpPressed` (o frame) okur. Duvara değmeden 1–2 frame önce basılan zıplama kaybolur (Airborne'da coyote 0 olduğu için buffer orada da harcanmaz). Düzeltme: iki yerde `context.JumpBufferTimer > 0f` kullan ve `context.ConsumeJumpBuffer()` çağır (`GroundedState.CheckAirborneTransitions` örneği). `JumpBufferDuration` (0.1) zaten var.
**Bitti sayılır:** derleme + EditMode + PlayMode (`WallRulesTests` W4/W5) yeşil.

## PF-6 — Zamanlama: dash ve ledge climb

- [DashState.cs:46](../../Assets/Scripts/New-Scripts/Player/States/DashState.cs): süre `Update`'te `Time.deltaTime` ile sayılıyor, hız 50 Hz fizik adımında uygulanıyor (Fixed Timestep 0.02). Dash 0.2 sn = 9–11 adım, yani 7 yerine 6.3–7.7 birim, frame hızına bağlı. Düzeltme: sayaç ve çıkış `FixedUpdateState`'te, `Time.fixedDeltaTime` ile.
- [LedgeClimbState.cs:61](../../Assets/Scripts/New-Scripts/Player/States/LedgeClimbState.cs): `PlayerRigidbody.position` `Update`'te yazılıyor; Interpolate açıkken yüksek fps'te titreme yapabilir. Düzeltme: konum hesabı ve yazımı `FixedUpdateState`'e.
**Bitti sayılır:** derleme + EditMode + PlayMode (`DashRulesTests`) yeşil; dash mesafesi sabit (PlayMode'da başlangıç/bitiş farkı `DashDistance` ± bir adım değil, tam).

## PF-7 — Hitstop platformları dondurmuyor (Karar)

`NotifyImpact` ([PlayerController.cs:186-194](../../Assets/Scripts/New-Scripts/Player/PlayerController.cs)) her dash başında ve sapan fırlatışında `HitStopDuration` (0.2 sn) hitstop ister. `HitStopManager` yalnızca `IFramePausable` dinleyicilerini durdurur, tek dinleyici oyuncu. Platformlar akmaya devam eder: level 15'te dash atarken duvar 0.8 birim uzaklaşır. Seçenekler: platformlar da `HitStopEvents`'e abone olsun, ya da hitstop kısalsın/hareketli yüzeye yakınken atlansın. Geliştirici seçer.

## PF-8 — Tutunma toleransı (Karar)

Havadan ve yerden tutunmak `IsTouching*Wall` ister (fiilî temas, ~0.045). `WallSnapRaycastDistance = 2` yalnızca girişteki snap ışınında kullanılıyor ve giriş zaten temas şartına bağlı olduğundan bu mesafe pratikte hiç devreye girmiyor ([wall-movement.md:26](../systems/wall-movement.md) "o mesafeye kadar" diyor). Seçenek: `ProcessWallInteraction` (Airborne :251) ve `GroundedState.CheckWallClimb` küçük bir tolerans (PF-2'deki `WallClimbContactTolerance`) içinde duvar görünce de tutunsun, snap gerisini halleder. `WallSnapRaycastDistance` ya küçültülsün ya da dokümanda gerçek rolü yazılsın.

## PF-9 — Küçükler

- [SlingshotState.cs:30](../../Assets/Scripts/New-Scripts/Player/States/SlingshotState.cs): `UseSlingshot()` anchor kontrolünden önce; geri dönüş yolunda hak boşa harcanır. Kontrolden sonraya al.
- [PlayerController.cs:288](../../Assets/Scripts/New-Scripts/Player/PlayerController.cs): `CurrentWallSlideTime = 2f` sabit; `Awake`'te `Stats.MaxWallSlideTime` ile başlat.

---

## Teşhis testi tarifi (doğrulama için)

İnceleme sırasında geçici olarak `Assets/Tests/PlayMode/` altında `PlayerRuleTestBase`'ten türeyen bir test yazılıp silindi. PF-1/PF-2 doğrulamasında aynısı kurulabilir:

- Duvar: `SpawnBox("MovingWall", …, new Vector2(1f, 17f), 0f)` + `Rigidbody2D` (Kinematic, Interpolate) + `PlatformController` (strateji yok).
- Sürücü: `WrappingPlatformManager` `AddComponent`, private alanlar reflection ile: `platforms` = [duvar], `leftBound` = duvar x, `rightBound` = duvar x − 100, `minSpeed` = `maxSpeed` = 4.
- Her `WaitForFixedUpdate`'te `Player.CurrentState` ve `Physics2D.Distance(Player.PlayerCollider, wallCollider).distance` kaydedilir.

| Senaryo | Kurulum | Bugünkü sonuç |
|---|---|---|
| A | Oyuncu duvarın arka yüzünde (duvar uzaklaşıyor) tutunur, sürücü **sonra** başlar | 2. adımda `Airborne`, boşluk 0.08/adım büyür, geri dönmez |
| C | Ön yüz (duvar iter), stick nötr | 70/70 adım `WallClimbing` |
| E | Sürücü önce 10 adım koşar, oyuncu stick sola + LB ile havadan yakalar | 60/60 adım `WallClimbing`, boşluk 0 |
| H / I | E gibi yakala, 5 adım sonra zıpla (stick yukarı / nötr), LB basılı | 40/40 adım `Airborne`, boşluk 0.8 sn'de 2.4 |

PF-1 sonrası H/I tekrar tutunmalı; PF-2 sonrası A düşmemeli. Teşhis dosyası iş bitince silinmeli; kalıcı kural testine dönüşecekse insan onayıyla `WallRulesTests`'e taşınır.
