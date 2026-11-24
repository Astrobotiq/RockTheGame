# Platformlar

*GDD §6.2 "Platformlar". Son kod doğrulaması: 2026-11-21 (launch pad nota bağlantısı ve sıvı ışık görseli; önceki: 2026-11-17 hareketli node bırakışı aynı aktarım yolunu kullanıyor).*

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
- **Ayrılırken hız aktarımı (yalnızca duvar):** oyuncu tutunduğu hareketli duvardan her ayrıldığında (zıplama, bumper/stick bırakma, süre bitişi) duvarın o anki hızı (`DeltaPosition / fixedDeltaTime`) ×1 oyuncunun hızına eklenir ve hava sürtünmesinden etkilenmez (duvar kaçmasın). **Zeminden** (zıplama, yürüyüp düşme) hız aktarılmaz: stick'in hava hızının üstüne binip çok hızlandırdığı için 2026-11-14'te kaldırıldı; boost'suz zeminden zıplayınca hava hızı yalnızca oyuncunun kendi hızıdır. Duvar durağansa 0'dır. `SurfaceVelocity` bu iş için kullanılmaz (son 3 adımın en büyüğü, boost için tasarlandı). Hareketli node'dan kanca bırakışı da aynı yolu kullanır (`AirEntry.CarriedVelocity`, × `MovingNodeLaunchMultiplier`), ama orada taşınan X `MovingNodeCarryDecayTime` içinde söner: [`grapple.md`](grapple.md) "Hareketli node".
- **Zıplama momentumu:** yüzeyin `JumpBoostMultiplier`'ı 0'dan büyükse zıplamada yukarıdaki taban yerine `SurfaceVelocity × JumpBoostMultiplier` eklenir (çarpan toplamı belirler, taban üstüne eklenmez). Bu yerden, coyote'tan, kayarken ve tırmanırken zıplamada geçerli. Pratikte yalnızca **hızlanan platformlar** bunu yapar. Yerçekimi kuralları: [`movement-core.md`](movement-core.md).
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
| Kullanım | Hiçbir odada kullanılmıyor (2026-11-12'de bloklu sisteme geçildi; `BreakablePlatform.prefab` boşta) | Yedi oda prefab'ında (3, 4×2, 9, 10, 11×2, 15, 20) + `NewScene`'deki şablon |
| Editör aracı | — | `SegmentedBreakablePlatformEditor`, blok üretme ve test |

GDD §6.2'deki "basılan yerden başlayarak dalga hâlinde kırılır" tarifi **yalnızca bloklu sistem** için doğru. Odalardaki kırılan platformların hepsi artık bloklu sistemde; basit sistem yalnızca kodda ve boşta duran prefab'ında kaldı. CLAUDE.md yeni işler için bloklu sistemi öneriyor.

### Tek yönlü platform

Bileşen değil, **katman**. `KinematicPhysicsHandler.oneWayPlatformLayerMask` içindeki collider'lar yalnızca yukarıdan aşağı hareket ederken engeller; aşağıdan ve yandan geçilir. Aşağı inme (drop-through) yok. Prefab'lar: `Prefab/New/Platforms/OneWayPlatform/`, `OneWay-MovingPlatform Variant`.

### Launch pad (`LaunchPad`)

- `transform.up` yönünde `launchSpeed` (varsayılan 15, her örnekte ayrı) hızla fırlatır. Pad döndürülerek yönü değiştirilir.
- Algılama `FixedUpdate`'te bir `OverlapBox` (`detectionOffset`, `detectionSize`). Tetiklendikten sonra `triggerCooldown` (0.2 sn) boyunca yeniden tetiklenmez.
- **Şekiller:** `detectionCollider` atanmışsa algılama kutu yerine bu collider'ın şekliyle yapılır (`Collider2D.Overlap`); boşsa yukarıdaki `OverlapBox` aynen çalışır. Üçgen ve daire pad'in algılama collider'ı pedin 0.12 birim büyütülmüş tetik hali, ayrı bir `Detection` çocuğunda ve **Ignore Raycast katmanında** durur (oyuncu/zemin/kanca sorgularına karışmasın); pedin kendisi zemin katmanında düz collider (`CircleCollider2D`/`PolygonCollider2D`). Fırlatma yönü `directionMode` ile: `TransformUp` (dikdörtgen; `launchAngleOffset` derece döndürülebilir, dik üçgende -45) ya da `OutwardFromCenter` (daire: `outwardCenterOffset` merkezinden oyuncunun collider merkezine doğru, `LaunchDirectionMath`). Daire alttan temasta aşağı fırlatır (kasıtlı, "temas noktasından dışa"). Prefab'lar: `LaunchPad-LiquidLight-Triangle` (**90-45-45 dik üçgen, seviye köşeleri için**: dik kenarlar 2 birim, prefab'ın pivotu dik açı köşesinde, yerelde dik açı sol-altta ve hipotenüs sağ-üste bakar; köşeye koyup 90°'lik adımlarla çevirilir; fırlatma hipotenüsün dışa bakan normali, yani yukarıdan 45° saat yönünde) ve `-Circle` (LiquidLight varyantının varyantları, kendi materyalleri `_Shape` 1/2). Gizmo: kutu için eskisi gibi, şekilli pad'de algılama collider'ı hattı; dairede yukarıdan ±60° arası beş olası yönün yörüngesi.
- **Bütün kaynakları doldurur:** dash, slingshot, dayanıklılık, kayma süresi ve renkler.
- Oyuncuyu `AirEntry.LaunchPad()` profiliyle fırlatır: yükselirken 3×, düşerken 1.5× yerçekimi, zıplama tuşundan bağımsız.
- Juice: squash/stretch (UniTask), VFX, ses kanalı, `LaunchPadProfile` titreşimi, `onLaunch`/`onLaunchComplete` UnityEvent'leri.
- **Nota:** `noteSystem` (`BounceNotePlayerSO`) atanmışsa her fırlatmada, anlık 'thump' cue'sunun üstüne bir nota da çalar. Seçim `NoteSequencer`'da (zincir dışında tekrarsız rastgele yürüyüş, zincirde gamda tırmanış, aynı enstrüman arka arkaya gelmez), zaman kapısı `NoteGate`'te: son notadan 1.5 sn içindeki vuruş zincir, en az 80 ms aralık, aynı anda en fazla 3 ses; kapı doluysa yeni nota **atlanır** (eski ses kesilmez). Zincir, sınır ve geçmiş pad başına değil asset'te ortak. Pad seçilen notayı `LastNote` ve `NotePlayed` olayıyla görsele (shader) verir. Gam A minör pentatonik (A4–G6), örnekler `Assets/Audio/SFX/Notes/`. Tasarım: `docs/ui-research/bounce-pad/oneri.md`.
- **Sıvı ışık görseli (deneme, henüz oyun sahnelerinde yok):** `Prefab/New/Platforms/JumpPad/LaunchPad-LiquidLight.prefab` (LaunchPad varyantı). Visual, 1×1 birimlik beyaz quad sprite (`Shader/LiquidLightPadQuad.png`) + `Sprites/LiquidLightPad` shader'ı (`Mat_LiquidLightPad_Rect`). Tek SDF shader: `_Shape` 0 dikdörtgen / 1 üçgen / 2 daire. **Kenar: "Akan palet kenar"** (mockup'ta kullanıcının seçtiği; dalgalı sıvı kenar ve boşta parıltı YOK). Katmanlar mockup'taki sırayla: koyu mürekkep gövde → 3 px içeriden akan sıvı dolgu (desen dolgu dikdörtgenine göre 4.2×1.1 aralıkta eşlenir) → gövdeyi saran 5 px'lik (vuruşta 8 px) çerçeve, paleti pedin merkezi etrafında dönen konik gradyan (`_FrameSpin`) → en dışta 1.5 px'lik koyu halka. **Ölçek mockup'tan:** ped 170 px = 3.2 birim (1 px = 0.018824 birim), gövde 3.2 × 0.5647 (30 px), köşe 6 px. Gövdenin üstü collider üstüyle aynı hizada (collider 3.2 × 0.3, gövde onun altına doğru kalın). `LiquidLightPad` script'i her kare `_Flow`, `_Hit` (zarf: 0.05 sn yükselir, τ 0.22 sn söner) ve `_HueShift`'i (nota rengine palet kayması) `MaterialPropertyBlock` ile verir; `LaunchPad.Launched`/`NotePlayed` olaylarını dinler. LaunchPad squash'ı Visual'ı merkezinden ölçekler, `anchorBottom` ile alt kenar sabit tutulur (mockup'ta ped zemine oturur). Matematik `LiquidLightMath` (EditMode testli). Ortak SDF/palet/ton kodu `Shader/LiquidLightCommon.cginc`. Nota derece renkleri mockup'tan: A `#FCAE04`, C `#7CFF4F`, D `#22E6FF`, E `#A36BFF`, G `#FF2E8A`.
- **Odalardaki yerleşim (2026-11-21):** `RoomNew-11/13/14/15/16/20/21/22 Variant` içindeki 17 dikdörtgen pad `LaunchPad-LiquidLight` varyantıyla değiştirildi; konum, açı, ölçek, sıra, ad, `launchSpeed` ve `detectionSize` korundu, sprite renk tonu (oda 11'deki hızlı pad'in yeşilimsi sarısı) yeni görsele taşındı. **Oda 12 (hareketli ve sınırdan sınıra akan pad'ler):** bunlar `Rigidbody2D` + `PlatformController` taşıyan, yöneticiye bağlı pad'ler olduğu için nesneler değiştirilmedi; `MovingLauncPad.prefab` ve `WrappingLaunchPadManager Variant.prefab` içindeki 5 pad'in yalnız `Visual` çocuğu yeni görsele çevrildi (sprite, `Mat_LiquidLightPad_Rect`, `LiquidLightPad`) ve bir `Burst` çocuğu eklendi; collider, hareket ve fırlatma aynı. **Üçgen/daire pad'ler hiçbir odada yok.** **Shader notu:** pad ve patlama shader'ları nesne uzayı konumuna dayandığı için `DisableBatching` taşır; yoksa yan yana pad'ler birleştirilince çizilmez.
- **Kaleydoskop patlaması:** varyantın `Burst` çocuğu (`Sprites/LiquidLightBurst` shader'ı, `Mat_LiquidLightBurst_Rect`, `LiquidLightBurst` script'i). Her fırlatmada pedin SDF'ini takip ederek her yöne açılan sıvı ışık dalgası (hız 270 px/sn = 5.08 birim/sn, üst oktavda +60 px/sn; ömür 1.3 sn; hepsi mockup ölçeğinden); açı simetri sayısı kadar katlanır: enstrüman 0..3 → 3/4/5/6; palet nota rengine kayar. Pad'in Visual'ından ayrı, gövde + 13.6 birimlik quad (ömrü bitince renderer kapanır); doğduğu şekil o anki ezilmiş pedin şekli (`LiquidLightPad.SquashRatio`, mockup'taki `padRect(t)` gibi). **Flaş güvenliği (WCAG 2.3.1):** `FlashLimiter` pad'ler arası ortak; herhangi bir 1 sn'de en fazla 2 tam şiddetli patlama, fazlası 0.3× şiddetle oynar. Pedin gövde parlaması (`_Hit`) bu sınıra bağlı değil, yalnız pedin kendi alanında.
- Editörde seçiliyken yörünge gizmo'su çizer
- Editörde seçiliyken yörünge gizmo'su çizer (`playerStats` atanmışsa). Gizmo hava kontrolü girdisini hesaba katmaz.
- "Hareketli ve döngüsel versiyonlar" (GDD) ayrı kod değil, prefab kompozisyonu: `JumpPad/MovingLauncPad.prefab` (hareketli platform + pad) ve `PlatformWrapper/WrappingLaunchPadManager Variant.prefab` (sınırdan sınıra akan pad'ler).

### Sınırdan sınıra akan platformlar (`WrappingPlatformManager`)

Bir grup `PlatformController`'ı (stratejisiz) `leftBound`'dan `rightBound`'a taşır. Ucu geçen platform taşan mesafeyle birlikte başa ışınlanır. Hız `minSpeed`–`maxSpeed` arasında rastgele; iki değer eşitse deterministik. Üç kullanımı var: düz platformlar (oda 15), launch pad'ler (`WrappingLaunchPadManager Variant`) ve ölümcül nesneler (`WrappingDeathManager Variant`).

### Takas blokları (`SwapBlockPair`)

İlham: Donkey Kong Bananza'daki Switcheroo Goo. Eş boyutlu iki çerçeve: **A (pembe)** ve **B (mavi)**, varsayılan 3×3 birim, hücre 0.125 (1 birimde 8 hücre). Başta pembe dolu, mavi boş. **Dash**, oyuncunun kutusunun süpürdüğü hücreleri karşı çerçevede **aynı yerel konuma** taşır; bloğun tamamını değil, yalnızca dash'in izini. Ortadan yatay dash gövde boyunda bir tünel açar, mavide aynı yerde bir şerit belirir. Geri alma simetrik: mavideki şeride dash atılırsa süpürülen kısım pembeye döner. Dash hakkı normal harcanır.

- **Kurallar:** bir dash bir hücreyi bir kez taşır. Hedef hücrede oyuncu varsa hücre **bekler** (yarı saydam, collider yok) ve oyuncu çıkınca katılaşır; hücre oyuncuyu ezmez, itmez, öldürmez. Basmak, yürümek, duvar tırmanmak, kanca ve dash'siz çarpmak hiçbir hücreyi taşımaz. Ölünce iki çerçeve başlangıç düzenine döner (`IResettable`, bekleyenler iptal). Oda uykusunda durum korunur.
- **Süpürülen alan:** gövdenin dash boyunca taradığı alan: kutu + kutu+delta'nın dışbükey zarfı (çaprazda eğik bant) + `KinematicPhysicsHandler`'ın gerçekte izlediği X-sonra-Y yolu (`SwapBlockSweep.AxisLegs`; zarf tek başına yetmiyor: kutunun sağ-altında kalan hücre yatayı engelliyordu). Süpürme oyuncunun FixedUpdate'inden **önce** çalışır (`DefaultExecutionOrder(-100)`), dash çarpıp durmaz.
- **Tünel payı:** tünel gövdeye sıfır boşlukla açılırsa oyuncu geçemez (handler temastaki hücreyi çakışma sayıp skin kadar iter, oyuncu alt satıra gömülüp sıkışır). Bu yüzden gövdeye `skin × 0.75` yaklaşan hücreler de oyulur: tünel **gövde + en çok bir satır pay** (hizalı yerleşimde 10, hizasızda 9 satır; hücre küçülürse pay da küçülür). Oyuncu zeminin tam skin kadar üstünde durduğu için zemin oyulmaz. Bekleme kontrolü aynı payı kullanır.
- **Kod:** `SwapBlockGrid` (saf mantık: hücre durumu, süpürülen hücreler, takas, bekleyen hücre, reset), `SwapBlockSweep` (saf: oyuncu kutusundan süpürme girdisi), `SwapBlockPair` (görünüm katmanı: iki `Tilemap` + `TilemapCollider2D` + elle üretilen `CompositeCollider2D`; hücre durumunun tek kaynağı `SwapBlockGrid`, Tilemap yalnızca `ApplyToView` ile beslenir). Prefab: `Prefab/New/Platforms/SwapBlock/SwapBlockPair.prefab`.
- **Geri bildirim (ilk sürüm, son sanat değil):** çerçeve çizgisi (boş çerçeve de okunur), kaynakta sönen hayalet hücre ve hedefte beliren flaş (`ParticleSystem`), bekleyen hücre yarı saydam. Ses ve titreşim **dash başına bir kez**: `SwapBlock` cue'su (`maximize_007`/`minimize_007`, Kenney Interface Sounds, CC0; kaynak ve lisans `Audio/SFX/SwapBlock/`), `PlatformVibrationSettingsSO.SwapBlockProfile` (0.12 sn, sol 0.5→0, sağ 0.3→0; başlangıç değeri, his gamepad'de ayarlanır).
- **Bilinen sınırlar:** çapraz tünelin tabanında yürüyerek çıkılamaz (0.125'lik basamaklar, fizikte basamak çıkma yok; dash ve zıplama geçer). Dash'in son fizik adımında (çıkış adımı) ~0.45 birim fazla oyulur. `MovingDeath` hücrelerden geçer (kinematik tetik): barikat tehlikeyi durdurmaz, oyuncunun üstünde durduğu ya da arkasına saklandığı **köprü/kalkan** olarak çalışır. İki çerçeve birbirine yakınsa oyulan şerit B'de dash'in önünde duvar olur (dash o hücreleri aynı dash'te geri almaz).
- **Test odası:** `RoomNew-SwapBlockTest` (id 101, NewScene'de x −800, spiral test odasının yanında): *Merdiven* (pembe blok zeminde, mavi çerçeve yüksek bir sekizintiye bitişik; bloğa bitişik durup çapraz dash atınca mavide basamaklı eğim) ve *Barikat* (8 genişliğinde çukurda `MovingDeath`, mavi çerçeve çukurun üstünde zemin hizasında; pembeden sağa dash atınca mavide köprü şeridi). Gamepad testi TP6.
- **Testler:** `SwapBlockGridTests` (18), `SwapBlockSweepTests` (9), `SwapBlockRulesTests` PlayMode: SB1/SB1b/SB1c (yalnızca süpürülen kısım, kısmi ve çapraz dash durmaz), SB2 (geri alma), SB3 (bekleyen hücre), SB4 (basmak/tırmanmak/çarpmak taşımaz), SB5 (respawn) ve FB1 (ses dash başına bir kez).

## Ölünce sıfırlama

`PlatformController`, `TriggeredPlatformController`, iki kırılan platform, `WrappingPlatformManager` ve `SwapBlockPair` `IResettable`. Kayıt `OnEnable`'da, bu yüzden yalnızca o an aktif odalar sıfırlanır. Ayrıntı: [`death-respawn.md`](death-respawn.md).

## Görünüm: "Riso levha"

Hareketli platformlar (`MovingPlatform`, `-Transform`, `OneWay-`, `AccelPlatform`, `RotatingPlatform` prefab'ları) baskı görünümlü; arazi, kırılan platformlar ve launch pad'ler değişmedi. Tasarım: `docs/ui-research/halftone/` (uygulama planı `implementation-plan.md`).

- **`RisoSlab.shader`** (`M_RisoSlab`): SDF levha, `#140C1C` çizgi (collider sınırı), kâğıt, üç mürekkep katmanı (teal 15° / sarı 45° / pembe 75°, multiply), temiz üst şerit (dünya yukarısına göre), gren, h < 0.45 için düz bantlar. Boyutu `lossyScale`'den okur (sprite 1×1 birim; `DisableBatching`). `_DotPitch` materyalde, aşama 2 için `_Influence*`/`_ShiftFront` boş.
- **`RisoSlabVisual`** (Square objesinde): `SurfaceVelocity`'yi yumuşatıp `_Velocity`'ye verir; `JumpBoostMultiplier > 0` ise `_Accelerating = 1`.
- **`HalftoneTrailEmitter`** (Square'in `Trail` çocuğu): tek `ParticleSystem`, mürekkep başına gecikmeli (0 / 0.06 / 0.12 sn) doğum, teleport (> 1.5 birim/kare) ve `IResettable`/`OnEnable`'da temizlik. Saf mantık `HalftoneTrailLogic.cs`.
- **`RisoDot.shader`** (`M_RisoDot`): opak normal karışımlı SDF daire, pembe/teal parçacıklarda kayık "hayalet" çift. Sıralama: Default katmanı, order -5 (platformun arkasında).
- **Ayarlar:** `Platform/SO/RisoSlabSettings.asset` (`RisoSlabSettingsSO`). His geliştiricinin kararı; değişiklikte eski/yeni değeri not et.
- **Henüz yok:** `WrappingPlatformManager`'ın sahnede atanmış platformları (prefab'da çocuk yok, sahnelerde elle bağlanmalı). Gamepad testi H7.

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

1. **`period <= 0` hareket etmez:** `LinearPingPongStrategy` ve `TransformPingPongStrategy` bu durumda başlangıç noktasında kalır (`AcceleratingPingPong` ile aynı davranış). `Circular` ise `period = 0` iken 1 sn'ye düşer.
2. **Kullanılmayan kod:** `MovingPlatform` + `LinearWaypointPath` + `IWaypointPath` (waypoint tabanlı eski sürücü) ve `IPassenger` hiçbir sahne/prefab'da yok. CLAUDE.md bunları aktif sistemin parçası olarak anlatıyor.
3. **Algılama kutuları yanları da kapsıyor.** Kırılan, tetiklenen platformlar ve launch pad "üstüne basma"yı collider'ın tamamını kaplayan bir kutuyla algılıyor. Yandan tutunmak ya da yandan değmek de tetikleyebilir.
4. **Sürekli titreşim tek kanallı.** Kırılan platform bitince `StopContinuousVibration` çağırıyor. Oyuncu o sırada duvara tutunuyorsa duvar titreşimi de kesilir.
5. **Oda uyurken kırılan platform yarıda kalır.** Kırılma sırasında oda `Sleep`'e geçerse (iki oda uzaklaşmak) `OnDisable` diziyi iptal eder ve platform kırık kalır. Bir sonraki ölüm sıfırlamasına kadar böyle kalır. Komşu odalar uyumadığı için pratikte nadir.
6. **Rastgele hızlı wrapping** `minSpeed ≠ maxSpeed` olursa ölümden ölüme farklı davranır. Ölümcül varyant için "zor ama adil" (GDD §2) ile çelişebilir.
