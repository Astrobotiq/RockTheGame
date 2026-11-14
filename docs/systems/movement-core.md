# Temel hareket: koşma, zıplama, dash

*GDD §5.1, §5.2 "Temel hareket" ve "His". Son kod doğrulaması: 2026-11-14 (dash süresi fizik adımında).*

## Tasarım niyeti

Celeste'ten alınan "oyuncu dostu" katman. Kanca oyunun kalbi, bu dosyadaki hareketler onu taşıyan zemin: hızlı tepki veren koşma, basılı tutma süresine göre yükselen zıplama, kenardan düşünce kısa bir tolerans ve tek hakkı olan bir dash.

## Kontroller

| Girdi | Ne yapar | Kod |
|---|---|---|
| Sol stick X | Koşma ve hava kontrolü (analog) | `GroundedState.FixedUpdateState`, `AirborneState.ApplyAirMovement` |
| Sol stick (2D) | Dash yönü (8 yön değil, serbest analog yön). Stick boşsa karakterin baktığı yön. | `DashState` ctor |
| Güney tuşu | Zıplama. Basılı tutmak yüksekliği artırır. | `PlayerController.Update` (buffer), `GroundedState`, `AirborneState` |
| Batı tuşu | Dash | `GroundedState.CheckDashTransition`, `AirborneState.HandleTransitions` |

Sağ stick de okunuyor (`IInputReader.RightStick`), ama temel hareket için değil, sağ kolun nişanı için. Bkz. [`grapple.md`](grapple.md).

## Nasıl çalışıyor

Karakter kinematik bir `Rigidbody2D`. Her `FixedUpdate`'te aktif state istenen hızı `PlayerController.Velocity`'ye yazar. `KinematicPhysicsHandler.Move` bu hızı çarpışmalara karşı çözer ve ulaşılabilen gerçek hızı geri döndürür. Yerçekimi Unity fiziğinden gelmez; her state kendisi uygular.

**Koşma.** Yerdeyken yatay hız doğrudan `LeftStick.x × MoveSpeed`. İvmelenme ya da sürtünme yok: stick bırakılınca karakter anında durur.

**Zıplama.** Zıplama tasarım değerlerinden türetilir:
`Gravity = −2·JumpHeight / TimeToApex²`, `JumpVelocity = |Gravity|·TimeToApex`. Havada yerçekimi çarpanı duruma göre değişir:

| Durum | Çarpan |
|---|---|
| Düşüyor (`vy < 0`) | `FallGravityMultiplier` |
| Yükseliyor, tuş bırakılmış | `JumpEndEarlyGravityMultiplier` (değişken yükseklik budur) |
| Tepe noktası yakını (`\|vy\| < ApexThreshold`) | `ApexHangGravityMultiplier` (tepede asılı kalma) |
| Diğer | 1 |

Bu çarpanlar yalnızca `isJumping: true` ile girilen havada uygulanır. Havaya hangi yoldan girildiği bu yüzden önemli:

| Havaya giriş | Yerçekimi kuralı |
|---|---|
| Yerden zıplama, coyote, duvar zıplamaları (kayarken ve tırmanırken), slingshot çıkışı | Tablodaki tam kural (tuşa bağlı) |
| Swing bırakma, launch pad (`bypassJumpGravity`) | Yükselirken hep 3×, düşerken 1.5×, apex hang yok; tuştan bağımsız |
| `JumpBoostMultiplier > 0` olan hareketli zeminden/duvardan zıplama | Aynısı ama yükselirken 1.5× (`endEarlyGravityMultiplier` 0.5). Coyote ile yapılırsa yalnızca apex hang kapanır. |
| Dash sonu, dual swing bırakma, duvardan düşme, kanca kopması, oda geçişi, respawn | Sabit 1× (düşüş bile hızlanmaz) |

Örneğin dash sonrasındaki düşüş, zıplama sonrasındaki düşüşten daha yavaştır.

Düşüş hızı `TerminalVelocity` ile sınırlı. Tavana çarpınca dikey hız sıfırlanır.

**Hava kontrolü.** Yatay hız `MoveTowards` ile hedefe yaklaşır. Stick basılıysa `AirAcceleration`, boştaysa `AirDrag` kullanılır. Böylece kancadan ya da launch pad'den gelen momentum hemen kaybolmaz, ama yönlendirilebilir.

**Coyote time ve jump buffer.** Zıplamaya basmak `JumpBufferTimer`'ı doldurur. Yere değen ilk karede buffer doluysa zıplama hemen gerçekleşir. Kenardan yürüyerek düşünce `AirborneState` `CoyoteTimeDuration` süresiyle açılır, bu sürede zıplama hâlâ çalışır.

**Hareketli zeminden zıplama.** Üstünde durulan yüzey `IMovingSurface` ise ve `JumpBoostMultiplier > 0` ise, yüzeyin hızı zıplamaya eklenir ve değişken zıplama yerçekimi devre dışı kalır. Coyote zıplamasında da son yüzey (`LastMovingSurface`) kullanılır. Ayrıntı: [`platforms.md`](platforms.md).

**Dash.** `DashSpeed = DashDistance / DashDuration` hızla, yerçekimsiz, sabit yönde gider. Bitince `AirborneState`'e geçer ve hızın yalnızca `DashEndMomentumMultiplier` kadarını taşır. Yerde bitse bile önce havaya geçer, sonraki fizik adımında yere döner.

**Dash hakkı.** `HasDashCharge` tek bir bool. Şunlar yeniler: yere inmek (`GroundedState`), tek ya da çift kancaya tutunmak (`SwingingState`, `DualSwingingState`), launch pad ([`platforms.md`](platforms.md)) ve respawn.

## Game feel bağlantıları

| Olay | Ne olur | Kod |
|---|---|---|
| Dash başlangıcı | Hit-stop (`HitStopDuration`) + kamera sarsıntısı + `HighImpact` titreşimi + `Dash` titreşimi + ses | `DashState.EnterState` → `PlayerController.NotifyImpact` |
| Dash süresince | Gölge izi (ghost afterimage) | `PlayerVFXManager.HandleGhostEffect` |
| Zıplama / iniş | Toz, ses, `Jump` / `Land` titreşimi | `GroundedState` |
| Koşma | Koşu tozu | `PlayerVFXManager.HandleRunDust` |
| Dash harcandı | Taç rengi griye döner | `PlayerColorController.SetDashExhausted` |

**Hit-stop yalnızca oyuncuyu durdurur.** `HitStopManager` `Time.timeScale`'e dokunmaz; yalnızca `HitStopEvents` yayınlar ve `PlayerController` hem `Update` hem `FixedUpdate`'i atlar. Dünya (platformlar, tehlikeler) akmaya devam eder. Dash zamanlayıcısı `FixedUpdateState`'te (fizik adımında) ilerler ve hit-stop sırasında `FixedUpdate` atlandığı için hit-stop dash süresinden yemez: her dash önce `HitStopDuration` kadar donar, sonra `DashDuration` kadar gider.

## Tunable'lar (`PlayerStats.asset`)

Asset'teki gerçek değerler. Koddaki varsayılanlar farklı, asset esas.

| Alan | Değer | Not |
|---|---|---|
| `MoveSpeed` | 10 | yer ve hava hedef hızı |
| `JumpHeight` / `TimeToApex` | 4 / 0.5 | → `Gravity` −32, `JumpVelocity` 16 |
| `FallGravityMultiplier` | 1.5 | |
| `JumpEndEarlyGravityMultiplier` | 3 | |
| `ApexThreshold` / `ApexHangGravityMultiplier` | 3.5 / 0.5 | |
| `TerminalVelocity` | −40 | |
| `AirControlMultiplier` | 1 | havada da tam hız |
| `AirAcceleration` / `AirDrag` | 40 / 20 | |
| `CoyoteTimeDuration` / `JumpBufferDuration` | 0.1 / 0.1 | `PlayerStatsSanityTests` 0.01–0.25 bandını korur |
| `DashDistance` / `DashDuration` | 7 / 0.2 | → `DashSpeed` 35 |
| `DashEndMomentumMultiplier` | 0.3 | dash sonu ~10.5 birim/sn kalır |
| `DashImpactMultiplier` | 0.5 | yalnızca kamera sarsıntısının büyüklüğü |
| `HitStopDuration` | 0.2 | dash ve slingshot fırlatışı ortak |
| `MomentumDecay` | 5 | hiçbir yerde okunmuyor |

## Kod

- `Player/PlayerController.cs`: FSM bağlamı, jump buffer, dash hakkı, `NotifyImpact`
- `Player/KinematicPhysicsHandler.cs`: çarpışma çözümü, `IsGrounded`, duvar ve tavan sensörleri, tek yönlü platform, hareketli yüzey taşıma
- `Player/States/GroundedState.cs`, `AirborneState.cs`, `DashState.cs`
- `Player/InputHandler.cs` (`IInputReader`): tetik eşiği 0.1
- `Player/IFramePauseable/HitStopManager.cs`, `Player/PlayerVFXManager.cs`, `Player/Visual/PlayerColorController.cs`

## Bilinen kenar durumlar ve GDD ile farklar

1. **Dar bir durumda taç rengi yanlış kalıyor.** `ResetDash()` yalnızca bool'u açar, rengi ayrıca sıfırlamak gerekir. Yer, tek kanca ve launch pad bunu yapıyor, `DualSwingingState` yapmıyor. Normalde dual swing'e tek kancadan geçildiği için sorun çıkmaz. Ama duvarda kayarken iki tetiğe aynı karede basılırsa doğrudan dual swing'e girilir: dash hakkı geri gelir, taç gri kalır.
2. **Sahne açılışında dash hakkı dolu.** `HasDashCharge` `true` başlar; respawn'la aynı davranış (5 Kasım 2026'da düzeltildi).
3. **GDD §5.2 "sert bir çarpmada hit-stop" diyor.** Kodda çarpma algılanmıyor: hit-stop yalnızca dash başlangıcında ve slingshot fırlatışında tetikleniyor. Ya GDD cümlesi düzeltilmeli ya da gerçekten çarpma tabanlı bir tetik eklenmeli.
4. **GDD §5.1 kontrol tablosunda sağ stick yok.** Sağ kol sağ stick'le, sol kol sol stick'le nişan alıyor (bkz. [`grapple.md`](grapple.md)).
5. **Tek yönlü platformdan aşağı inilemiyor.** Aşağı + zıpla gibi bir "drop through" yok. Bilinçli olabilir; GDD §6.2 yalnızca "aşağıdan geçilir, üstünde durulur" diyor.
6. **`MomentumDecay` ölü alan.** Asset'te 5, kodda okunmuyor.
7. **Fizik adımı 50 Hz.** Bütün değerler `0.02` sabit adıma göre ayarlı. 60 Hz denemesi `TASKS.md`'de (16 Ekim).

## Testler

`PlayerStatsSanityTests`: coyote/buffer bantları ve `DashSpeed` tutarlılığı. Dash kuralları: `DashRulesTests` ve `PlayerStartStateTests`, liste [`../bozulmamali-davranislar.md`](../bozulmamali-davranislar.md).
