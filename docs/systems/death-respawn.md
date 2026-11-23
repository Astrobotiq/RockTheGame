# Ölüm ve yeniden doğma

*GDD §2 (sütun 3), §5.3. Son kod doğrulaması: 2026-11-23 (`PlayerHealth.LastKiller`; önceki: 2026-11-19 ölüm efekti `DeathBurst`, `AcidHeadPool`).*

## Tasarım niyeti

Can barı yok, tek temas öldürür. Ölüm hızlı ve ucuz olmalı ki "bir daha dene" hissi kopmasın. Ölünce oda başlangıç durumuna döner. Checkpoint'ten sonra toplananlar ölünce kaybolur, checkpoint'e ulaşınca kalıcı olur.

## Ölüm anı

1. Oyuncu bir **trigger** collider'a girer ve o collider'da `DeathZone` vardır. `DeathZone`, girenin `IKillable` olup olmadığına bakar; oyuncuda bu `PlayerHealth`'tir.
   - Önce `DeathZone.Killing` yayınlanır: oyuncunun hızı ve konumu hâlâ geçerli (asit tepkisi burada okur, [`hazards.md`](hazards.md)). Dinleyici hata atsa da `Kill()` çağrılır (`try/finally`).
   - `Kill()`'den sonra `DeathZone.Killed` yayınlanır (asit titreşimi genel `Death` titreşimini burada ezer).
2. `PlayerHealth.Kill()` (`DeathZone` bundan önce `PlayerHealth.NoteKiller`/`LastKiller` ile öldüreni yazar; yalnızca `OnDeath` dinleyicileri sırasında okunabilir, ardından null; multiplayer rakibe bildirmek için kullanır): `Death` titreşimi, ölüm efekti ([aşağıda](#ölüm-efekti-blok-çözülme)), oyuncu objesi **devre dışı** (`SetActive(false)`), `OnDeath` olayı.
3. `OnDeath`'i dinleyenler **hemen**, iris kapanmadan önce çalışır:
   - `RespawnManager`: respawn dizisini başlatır.
   - `Collectible`: kalıcı olmayan toplanmışları geri getirir.
   - `CollectibleInventoryManager`: envanterdeki geçici altınları geri alır.
   - `RoomKeyManager`: anahtarlar kalıcı değilse kapıyı eski hâline döndürür.

Ölüm katmanı (`PlayerController.deathLayerMask`) öldürmez. Yalnızca kenardan çıkmanın tehlikeli bir yüzeye tırmanmasını engellemek için kullanılır (bkz. [`wall-movement.md`](wall-movement.md)). Öldüren her zaman `DeathZone`'dur. Tehlike türleri: [`hazards.md`](hazards.md).

## Ölüm efekti (karakterin kendi pikselleri)

`PlayerHealth.burstSettings` (`DeathBurstSettingsSO`) atanmışsa `Kill()` bunu oynatır; boşsa eski `deathParticlePrefab` (`FX_PlayerDeath`) çalışır. Önizleme: `docs/ui-research/death-particle/death-particle-v2.html` (ilk deneme olan ızgara blokları kötü görünüp bırakıldı: `death-particle.html`).

- **Parçacıklar (`DeathBurst`).** Görünen karakterin (`PlayerController.SpriteRenderer`, `Animation` çocuğu) o anki animasyon karesinin dolu pikselleri `cellTexels × cellTexels` hücrelere bölünür (`DeathBurstMath.BuildPixelCells`); her hücre kendi ortalama renginde kare bir parçacık olur (2 piksel: ~250 parçacık). Pikseller RenderTexture üzerinden okunur (doku okunabilir olmak zorunda değil), sprite karesi başına bir kez hesaplanıp saklanır; renderer'lar önceden üretilip yeniden kullanılır. Parçacıklar merkezden dışa üstel yavaşlayan hızla (yayılma ≈ `spread` birim), gecikmeli yerçekimiyle saçılır; dönme ve şekil bozulması yok; sönerken yanıp sönüp kaybolur (`duration` 0,6 sn). Oyuncunun ölüm hızının bir kısmı (`velocityInherit`) parçacıklara geçer.
- **Beyaz flaş.** `Mat_Player`'ın shader'ına (`PaletteSwap`) `_FlashAmount` eklendi. `DeathBurst`, gövde sprite'ının kopyasını aynı materyalle ve `_FlashAmount = 1` ile `flashFrames` kare gösterir (oyuncunun kendi renderer'ına dokunulmaz, çünkü oyuncu `SetActive(false)` olur). Palet değişimi (`_Blend`) açıksa parçacık renkleri ham dokudan okunduğu için farklı olabilir.
- **Hitstop ve sarsıntı.** `HitStopEvents.RequestHitStop(hitstopSeconds)` (0,09 sn) ve `CameraController.ShakeImpulse` (ölüm hızı yönünde, `shakeForce`). Parçacıklar hitstop boyunca yerinde durur. Sarsıntının px karşılığı önizlemeyle birebir değil; `shakeForce` oyunda ayarlanır.
- **CRT notu.** Ekran 480×360'a indiği için 2 sprite pikseli (≈1,3 sanat pikseli) titreyebilir; asit damlaları bu yüzden en az 2 sanat pikseli. Titrerse `cellTexels` 3'e çıkarılır.
- **Kafa.** Şimdilik `DeathHeadPlaceholder.png` (pembe daire); gerçek pixel art gelince `DeathBurstSettings.asset` içindeki `headSprite` ve `headStartOffsetY` (şu an 0,55) değişir. Kafayı `DeathBurst` yönetmez: asitte `AcidHeadPool`, diğer ölümlerde `GroundHeadPool` bırakır (aşağıda).

### Asit dışı ölümlerde kalan kafalar (`GroundHeadPool`)

Diken, hareketli tuzak gibi asit olmayan bir `DeathZone` öldürünce kafa fiziksel bir nesne olarak kalır.

- **Fizik:** `Rigidbody2D` (dinamik) + `CircleCollider2D`. Hitstop bitince fırlatma hızıyla fırlar, düşer, zeminde ve diğer kafalarda durur, seker (`headBounciness`), yuvarlanır (`headAngularDamping` dönmeyi söndürür). Collider döner ama sprite dik kalır (`View` çocuğu her kare dönüşsüz yerleştirilir). Kafa nesneleri `DeadHead` katmanında (11, `TagManager`'a eklendi); collider'ı yalnızca `headCollisionMask` ile (şu an `Ground` + `DeadHead`) çarpışır.
- **Oyuncuyla iç içe geçer.** Oyuncunun zemin sorguları yalnızca `Ground` katmanına baktığı için kafa onun için yoktur: üstünde duramaz, yolunu kesmez. Kafa `Death` katmanını da görmez; dikenden geçip zemine düşer, dikenin üstünü örtemez.
- **İtme.** Oyuncunun collider'ı kafanınkine değince (`Collider2D.Distance`) kafa oyuncudan uzağa, hafif yukarı eğimle itilir (`headPushSpeed` 3,5, oyuncunun hızından `headPushInherit` payı, `headPushInterval` 0,12 sn aralıkla). Kafalar birbirini fizikle iter, üst üste binmez.
- **Dash vuruşu.** Oyuncu dash atarken (`PlayerController.CurrentState is DashState`) kafaya değerse normal itme yerine yumruk gibi bir vuruş uygulanır: kafanın hızı doğrudan `headDashPushSpeed` (10, normal itme 3,5) olur, yön ağırlıklı olarak dash yönüdür (%75) ve oyuncudan yatay olarak uzağadır (%25), `headDashLiftDeg` (25°) kadar yukarı eğimlidir (`DeathBurstMath.DashPush`). Eğim yalnızca yatay yönle hesaplanır: oyuncunun merkezi kafadan yüksek olduğu için ham "oyuncudan kafaya" vektörü aşağı bakar ve eğimi eritirdi. Aynı kafaya art arda vuruş en az `headDashPushInterval` (0,2 sn) arayla; vuruş `headDashSquash` kadar yassılma verir.
- **Squash and stretch.** Hızlı giden kafa hız yönünde uzar (`headStretchPerSpeed`, en çok `headStretchMax` %25; diğer eksen daralır). Çarpınca (`OnCollisionEnter2D`, `GroundHeadBody`) çarpma ekseninde yassılır (hıza göre, en çok `headSquashMax` %30) ve yaylanarak söner (`headSquashFreq`, `headSquashDecay`). Oyuncunun itmesi de küçük bir yassılma verir (`headPushSquash`). Ölçek sprite genişliğinin piksel adımlarına oturtulur (`QuantizeScale`; 11 piksellik kafada adımlar ≈0,09), alt kenar sabit kalır, sprite dönmez.
- **Havuz.** Tüm oyunda en fazla `maxGroundHeads` (12). Havuz taşarsa en eski kafa `LevelResetManager.ResetAll` sırasında, yani iris kapalıyken silinir (`GroundHeadPool` `IResettable`'dır): oyuncu görmez. Taşma geçici olarak izinlidir (iki yedek nesne).
- **Silinme:** oyuncu odadan çıkınca (`Room.OnRoomExited`; kafanın odası ölüm noktasından `RoomBounds` ile bulunur) ve oda sınırının 40 birim altına düşünce (çukur). Ölünce ya da respawn olunca silinmez. Asit kafaları (`AcidHeadPool`) ayrı havuzdur ve kendi yanıp sönme çözülmesini kullanır.

### Asitte kalan kafalar (`AcidHeadHost` + `AcidHeadPool`)

Asit öldürünce kafa asitte kalır, ölümler arasında birikir.

- `AcidHeadHost`, `DeathZone.Killing`'i dinler, `DeathBurst.Claim` ile ölümü üstlenir (kafa fırlatılmaz, parçacıklar sabit asitte asidin arkasında çizilir) ve kafayı `AcidHeadPool`'a bırakır.
- **Pool modu** (`Acid.prefab`): kafa yüzeyden dibe doğru batar ve derinliğin %35–65'inde (rastgele) durur, dibe inmez. Yatayda ölüm noktasına en yakın boş yuvaya oturur (yuva aralığı 0,45 birim). Derinlik, asidin collider yüksekliği ve `_AcidLevel` değerinden okunur.
- **Blob modu** (`MovingDeath`, `AccelDeath`, `RotatingDeath`, `WrappingDeath` içindeki `DeathZone`'lu nesneler): kafa asidin içine çekilir ve onunla gider (konum her kare asidin merkezine göre).
- **Havuz** (`AcidHeadPool`): tüm oyunda en fazla `maxHeads` (12) kafa. Dolunca en eski kafa yanıp sönerek çözülür, nesnesi yeniden kullanılır.
- **Silinme:** oyuncu odadan çıkınca (`Room.OnRoomExited`) o odadaki asitlerin kafaları silinir; asit devre dışı kalınca da (`OnDisable`). Ölünce ya da respawn olunca silinmez. `FlowingAcid` henüz kafa tutmuyor.
- Kafa sahnede kendiliğinden oluşan `AcidHeadPool` nesnesinde, asit tonu `SpriteRenderer.color` ile verilir (derindeki daha yeşil).

## Respawn dizisi (`RespawnManager.RespawnSequenceAsync`)

| Adım | Süre (sahne değeri) |
|---|---|
| `PlayerController.OnStartRespawn`: hız sıfır, iki anchor bırakılır, dash + slingshot + dayanıklılık dolar, sürekli titreşim durur | — |
| İris kapanır, merkez ölüm noktası (`TransitionManager`) | `_transitionDuration` 0.5 sn |
| `LevelResetManager.ResetAll()` | — |
| Oyuncu respawn noktasına taşınır ve tekrar aktif edilir (hâlâ donuk) | — |
| Kameranın yetişmesi beklenir | `_cameraCatchUpDelay` 0.3 sn |
| İris açılır, merkez respawn noktası | 0.5 sn |
| `OnEndRespawn`: havaya geçilir (`NormalState`, `AirEntry.Fall`), kontrol oyuncuda | — |

Ölümden kontrolün geri gelmesine kadar yaklaşık **1.3 sn** geçer. Değerler `Prefab/New/Managers/RespawnManager.prefab` ve `TransitionManager.prefab` içinde (`_maxRadius` 2).

## Ne sıfırlanır, ne sıfırlanmaz

| Şey | Ölünce | Nasıl |
|---|---|---|
| Kırılan platformlar (`BreakablePlatform`, `SegmentedBreakablePlatform`) | Sıfırlanır | `IResettable` |
| Hareketli platformlar ve tuzaklar (`PlatformController`) | Sıfırlanır | `IResettable` |
| Tetiklenen platformlar (`TriggeredPlatformController`) | Sıfırlanır | `IResettable` |
| Sınırdan sınıra akan nesneler (`WrappingPlatformManager`) | Sıfırlanır | `IResettable` |
| Checkpoint sonrası toplanan ananas/altın, anahtar parçaları | Geri gelir | `Collectible`, `OnDeath`'i kendisi dinler |
| Envanterdeki geçici altın sayısı | Geri alınır | `CollectibleInventorySO.Revert` |
| Anahtarlarla açılan/kapanan kapı | Eski hâline döner (anahtarlar kalıcı değilse) | `RoomKeyManager` |
| Süreli yeniden doğan toplanabilirler (örn. slingshot yenileyici) | Hemen geri gelir | `Collectible.respawnsAfterDelay` |
| Oyuncunun kaynakları (dash, slingshot, dayanıklılık) | Dolar | `OnStartRespawn` |
| Duvar kayma süresi | İlk yere inişte dolar | `NormalState.OnLand` |
| Dönen node, `OncePerNode` modu | **Sıfırlanmaz** | `FullRotationNode` `IResettable` değil |
| Uyuyan odalardaki nesneler | **Sıfırlanmaz** | `IResettable` kaydı `OnEnable`/`OnDisable`'da yapılıyor; yalnızca o an aktif olan oda ve komşuları sıfırlanır |

## Checkpoint'ler

- `Checkpoint` bir trigger. Oyuncu ilk kez girince `TransformEventChannelSO` üzerinden kendi `transform`'unu yayınlar; `RespawnManager` bunu yeni respawn noktası yapar. Trigger yolu her checkpoint için **yalnızca bir kez** çalışır.
- **Oda kapıları da checkpoint'tir.** `RoomTransitionTrigger.associatedCheckpoint` atanmışsa her geçişte `ActivateCheckpoint()` çağrılır. Bu yol "bir kez" kontrolünü atlar, yani geri dönülen odanın checkpoint'i yeniden respawn noktası olur. Atanmamışsa konsola uyarı düşer.
- Aynı kanal toplanabilirleri de kalıcı yapar: her checkpoint ve her kapı geçişi o ana kadar toplananları "commit" eder ([`collectibles.md`](collectibles.md)).
- Başlangıç: `LevelBootstrapper`, başlangıç odasının `InitialCheckpoint`'ini ilk respawn noktası yapar. `Room.InitialCheckpoint` bunun dışında hiçbir yerde okunmuyor.

## Kod

- `Death/PlayerHealth.cs` (`IKillable`), `DeathZone.cs`, `AcidHazard.cs`
- `Death/DeathBurst.cs`, `DeathBurstMath.cs`, `DeathBurstSettingsSO.cs`, `AcidHeadHost.cs`, `AcidHeadPool.cs`, `GroundHeadPool.cs`, `GroundHeadBody.cs`; ayar `Death/SO/DeathBurstSettings.asset`
- `Death/RespawnManager.cs`, `TransitionManager.cs` (iris, `M_IrisTransition.mat`)
- `Death/Checkpoint.cs`, `TransformEventChannelSO.cs`
- `Death/LevelResetManager.cs`, `IResettable.cs`
- `LevelChange/RoomTransitionTrigger.cs`, `LevelBootstrapper.cs`
- `Player/PlayerController.cs`: `OnStartRespawn`, `OnEndRespawn`

## Bilinen kenar durumlar ve GDD ile farklar

1. **Respawn "anında" değil.** GDD §5.3 "anında" diyor; kodda iki iris geçişi ve bir bekleme var, toplam ~1.3 sn. Celeste ölçeğinde bu kısa sayılır ama bilinçli bir sayı olarak tutulmalı: üç değer iki prefab'a dağılmış durumda.
2. **Ölüm sıfırlaması iki ayrı mekanizmayla yapılıyor.** Platformlar `IResettable` + `LevelResetManager` (iris kapandıktan sonra), toplanabilirler/anahtarlar/envanter `PlayerHealth.OnDeath` (ölüm anında, iris kapanmadan). Oyuncu iris kapanırken toplanabilirlerin yerine döndüğünü görebilir. Yeni sıfırlanabilir bir sistem eklerken hangisinin kullanılacağına karar verilmeli; CLAUDE.md yalnızca `IResettable`'ı anlatıyor.
3. **`IResettable` kaydı `OnEnable`'da.** `LevelResetManager.Awake`'ten önce `OnEnable` alan bir nesne kaydolmaz. Odalar `LevelBootstrapper.Start`'ta önce uyutulup sonra açıldığı için oda içindekiler sonunda kaydolur. Oda dışında duran bir sıfırlanabilir nesne ise bu yarışı kaybedebilir.
4. **Dönen node'un `OncePerNode` modu ölümü bilmiyor** (bkz. [`grapple.md`](grapple.md)).
5. **Kapı geçişi, geçiş reddedilse bile checkpoint'i etkinleştirir.** `ExecuteTransition` başka bir geçiş sürerken erken dönüyor, ama `ActivateCheckpoint()` yine de çağrılıyor.
6. **"Checkpoint öncesi toplananlar kaybolur" kuralı pratikte "kapı geçişinden önce" demek.** Kapılar checkpoint olarak bağlandığı için, ayrı bir checkpoint objesi olmadan da her oda geçişi toplananları kalıcı yapar.
