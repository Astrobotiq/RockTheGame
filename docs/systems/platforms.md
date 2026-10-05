# Platformlar

*GDD §6.2 "Platformlar". Son kod doğrulaması: 2026-10-05.*

## Tasarım niyeti

Platformlar hareket setinin oyun alanı. Hareketli, dönen ve hızlanan platformlar zamanlama ister. Kırılan platformlar oyuncuyu acele ettirir, launch pad'ler hareketi zincirler. Aynı hareket altyapısı tehlikeleri de taşır ([`hazards.md`](hazards.md)).

## Mimari: sürücü + strateji

Hareketli her şey iki parçadan oluşur:

- **Sürücü** (`PlatformController` ya da `TriggeredPlatformController`): kinematik `Rigidbody2D`'yi her `FixedUpdate`'te taşır. `IMovingSurface` (oyuncuyu taşımak için) ve `IResettable` (ölünce başa dönmek için) arayüzlerini uygular.
- **Strateji** (`MovementStrategy` alt sınıfı): `GetPositionAtTime(t)` ve isteğe bağlı `GetRotationAtTime(t)` döndüren saf bir zaman fonksiyonu. Hareket hep **zamandan hesaplanır**, adım adım birikmez. Aynı zamanda aynı konum çıkar ve ölünce sıfırlamak kolaydır.

| Strateji | Hareket | Ayarlar | Zıplama desteği |
|---|---|---|---|
| `LinearPingPongStrategy` | İki sabit nokta (`Vector2`) arasında gidip gelme, sabit hız | `period`, `phaseOffset` | Yok |
| `TransformPingPongStrategy` | Aynısı, uçlar `Transform` (açılışta önbelleğe alınır) | `period`, `phaseOffset` | Yok |
| `AcceleratingPingPongStrategy` | Başlangıçta 0 hızdan uca kadar **hızlanır**, uçta `endDelay` bekler, geri dönerken yavaşlayarak durur. Uçlar canlı `Transform`'lar. | `forwardDuration`, `endDelay`, `returnDuration`, `phaseOffset` | **Var**, `jumpBoostMultiplier` (varsayılan 1.5) |
| `CircularMovementStrategy` | Merkez `Transform` etrafında daire. `keepUpright` kapalıysa yörünge boyunca döner. | `radius`, `period`, `clockwise`, `initialAngle`, `rotationOffset` | Yok |

`Editor/PlatformStrategyConverterWindow`: `LinearPingPongStrategy` kullanan platformları `TransformPingPongStrategy`'ye çevirir (uçları sahnede sürüklenebilir yapar).

## Oyuncuyu taşıma (`IMovingSurface`)

- `KinematicPhysicsHandler`, oyuncu bir `IMovingSurface`'in **üstünde** duruyorsa ya da ona **tutunuyorsa** (`ClingingWallDirection`), her adımda yüzeyin `DeltaPosition`'ını oyuncuya ekler. Yalnızca öteleme taşınır; platformun dönüşü oyuncuyu döndürmez.
- **Zıplama momentumu:** yüzeyin `JumpBoostMultiplier`'ı 0'dan büyükse zıplamaya `SurfaceVelocity × JumpBoostMultiplier` eklenir. Bu yerden, coyote'tan, kayarken ve tırmanırken zıplamada geçerli. Pratikte yalnızca **hızlanan platformlar** bunu yapar. Yerçekimi kuralları: [`movement-core.md`](movement-core.md).
- `SurfaceVelocity` son **3 fizik adımındaki en büyük hız**dır. Hızlanan platform uçta aniden durduğunda oyuncu yaklaşık 0.06 sn geç zıplasa bile tam fırlatmayı alır. Bu, platformun durmasıyla zıplama arasında küçük bir tolerans penceresi demek.

## Platform türleri

### Hareketli platform (`PlatformController`)

Sürekli döngüde, sahne başından beri geçen zamanla çalışır (`Time.time − _timeOffset`). Ölünce `_timeOffset` sıfırlanır ve platform başlangıç konumuna ışınlanır; yani ölümden sonra bütün platformlar fazlarını baştan alır.

### Tetiklenen platform (`TriggeredPlatformController`)

Oyuncu üstüne basana kadar `t = 0`'da bekler, basınca stratejinin **bir tam turunu** (`Period`) oynatır. Tur bittiğinde oyuncu hâlâ üstündeyse yeni bir tura geçer, değilse başlangıçta durur. Algılama, collider kutusunun 0.05 birim yukarı uzatılmış bir `OverlapBox`'ı.

### Kırılan platform: iki sistem

| | `BreakablePlatform` (basit) | `SegmentedBreakablePlatform` (bloklu) |
|---|---|---|
| Tetik | Collider kutusuyla oyuncu çakışması | Herhangi bir bloğun kutusuyla çakışma |
| Kırılma | `breakDelay` (0.5 sn) boyunca görsel titrer + sürekli titreşim, sonra **tamamı** birden yok olur | Basılan bloktan başlayıp her iki yana `propagationDelay` (0.08 sn) aralıklarla **dalga** hâlinde. Her blok kendi sprite karelerini oynatır, bitince collider'ı kapanır. Titreme yok. |
| Yeniden oluşma | `respawnTime` (3 sn), son 0.4 sn'de reform VFX | Aynı |
| Kullanım | Prefab'ı 7 dosyada | Sahnede **tek** örnek |
| Editör aracı | — | `SegmentedBreakablePlatformEditor`, blok üretme ve test |

GDD §6.2'deki "basılan yerden başlayarak dalga hâlinde kırılır" tarifi **yalnızca bloklu sistem** için doğru. Odalarda ağırlıklı olarak basit sistem kullanılıyor ve o dalga yapmıyor. CLAUDE.md yeni işler için bloklu sistemi öneriyor.

### Tek yönlü platform

Bileşen değil, **katman**. `KinematicPhysicsHandler.oneWayPlatformLayerMask` içindeki collider'lar yalnızca yukarıdan aşağı hareket ederken engeller; aşağıdan ve yandan geçilir. Aşağı inme (drop-through) yok. Prefab'lar: `Prefab/New/Platforms/OneWayPlatform/`, `OneWay-MovingPlatform Variant`.

### Launch pad (`LaunchPad`)

- `transform.up` yönünde `launchSpeed` (varsayılan 15, her örnekte ayrı) hızla fırlatır. Pad döndürülerek yönü değiştirilir.
- Algılama `FixedUpdate`'te bir `OverlapBox` (`detectionOffset`, `detectionSize`). Tetiklendikten sonra `triggerCooldown` (0.2 sn) boyunca yeniden tetiklenmez.
- **Bütün kaynakları doldurur:** dash, slingshot, dayanıklılık, kayma süresi ve renkler.
- Oyuncuyu `AirborneState(isJumping: true, bypassJumpGravity: true)` ile fırlatır: yükselirken 3×, düşerken 1.5× yerçekimi, zıplama tuşundan bağımsız.
- Juice: squash/stretch (UniTask), VFX, ses kanalı, `LaunchPadProfile` titreşimi, `onLaunch`/`onLaunchComplete` UnityEvent'leri.
- Editörde seçiliyken yörünge gizmo'su çizer (`playerStats` atanmışsa). Gizmo hava kontrolü girdisini hesaba katmaz.
- "Hareketli ve döngüsel versiyonlar" (GDD) ayrı kod değil, prefab kompozisyonu: `JumpPad/MovingLauncPad.prefab` (hareketli platform + pad) ve `PlatformWrapper/WrappingLaunchPadManager Variant.prefab` (sınırdan sınıra akan pad'ler).

### Sınırdan sınıra akan platformlar (`WrappingPlatformManager`)

Bir grup `PlatformController`'ı (stratejisiz) `leftBound`'dan `rightBound`'a taşır. Ucu geçen platform taşan mesafeyle birlikte başa ışınlanır. Hız `minSpeed`–`maxSpeed` arasında rastgele; iki değer eşitse deterministik. Üç kullanımı var: düz platformlar (oda 15), launch pad'ler (`WrappingLaunchPadManager Variant`) ve ölümcül nesneler (`WrappingDeathManager Variant`).

## Ölünce sıfırlama

`PlatformController`, `TriggeredPlatformController`, iki kırılan platform ve `WrappingPlatformManager` `IResettable`. Kayıt `OnEnable`'da, bu yüzden yalnızca o an aktif odalar sıfırlanır. Ayrıntı: [`death-respawn.md`](death-respawn.md).

## Titreşim

`PlatformVibrationSettingsSO`: `LaunchPadProfile`, `BreakablePlatformProfile`.

## Kod

- `Platform/PlatformController.cs`, `TriggeredPlatformController.cs`, `MovementStrategy.cs` + dört strateji
- `Platform/BreakablePlatform.cs`, `SegmentedBreakablePlatform.cs`, `BreakableBlockUnit.cs`, `Editor/SegmentedBreakablePlatformEditor.cs`
- `Platform/LaunchPad.cs`, `WrappingPlatformManager.cs`, `IMovingSurface.cs`
- `Player/KinematicPhysicsHandler.cs`: taşıma ve tek yönlü platform
- Prefab'lar: `Assets/Prefab/New/Platforms/`
- Testler: `MovementStrategyTests`

## Bilinen kenar durumlar

1. **`period = 0` NaN üretiyor:** hem `LinearPingPongStrategy` hem `TransformPingPongStrategy` `% period` ve `/ (period / 2)` yapıyor. `AcceleratingPingPong` ve `Circular` korumalı. Düzeltme `TASKS.md`'de (9 Ekim).
2. **Kullanılmayan kod:** `MovingPlatform` + `LinearWaypointPath` + `IWaypointPath` (waypoint tabanlı eski sürücü) ve `IPassenger` hiçbir sahne/prefab'da yok. CLAUDE.md bunları aktif sistemin parçası olarak anlatıyor.
3. **Algılama kutuları yanları da kapsıyor.** Kırılan, tetiklenen platformlar ve launch pad "üstüne basma"yı collider'ın tamamını kaplayan bir kutuyla algılıyor. Yandan tutunmak ya da yandan değmek de tetikleyebilir.
4. **Sürekli titreşim tek kanallı.** Kırılan platform bitince `StopContinuousVibration` çağırıyor. Oyuncu o sırada duvara tutunuyorsa duvar titreşimi de kesilir.
5. **Oda uyurken kırılan platform yarıda kalır.** Kırılma sırasında oda `Sleep`'e geçerse (iki oda uzaklaşmak) `OnDisable` diziyi iptal eder ve platform kırık kalır. Bir sonraki ölüm sıfırlamasına kadar böyle kalır. Komşu odalar uyumadığı için pratikte nadir.
6. **Rastgele hızlı wrapping** `minSpeed ≠ maxSpeed` olursa ölümden ölüme farklı davranır. Ölümcül varyant için "zor ama adil" (GDD §2) ile çelişebilir.
