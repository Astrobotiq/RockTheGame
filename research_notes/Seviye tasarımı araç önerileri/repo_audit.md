# Rock The Game repo denetimi: seviye tasarımı araçları için sürtünme noktaları ve bağlantı yerleri

Kapsam: depo kodu, prefab/sahne YAML'ı ve git geçmişi okunarak yapılan, salt okunur bir denetim (2026-11-18, `feature/spiral-nodes` dalı). Kaynaklar depo yollarıdır (göreli, depo kökü `D:/PERSONAL/Unity/Projects/RockTheGame`). Unity Editor çalıştırılmadı; sahne sayıları `Assets/Scenes/NewScene.unity` üzerinde grep ile çıkarıldı.

## 1. Bugün yeni bir oda adım adım nasıl ekleniyor, her adım ne kadar sürüyor ve ne kadar hataya açık?

### Takeaway
Bir oda; tilemap geometrisine sahip bir prefab variant'ı (`RoomNew.prefab` tabanından) ile `NewScene.unity` içinde yapılan sahne seviyesindeki elle bağlantıların toplamı. Her oda kabaca 10–15 elle sürükle-bırak referansı ve sayı alanı gerektiriyor (roomId, komşular, RoomManager kaydı, geçiş başına targetRoom/coordinator/checkpoint/direction). Bunların hiçbiri doğrulanmıyor. Tek çalışma anı koruması, `associatedCheckpoint` için bir `Debug.LogWarning` ve `startingRoom` için bir `Debug.LogError`.

### Cited Findings
**Oda tabanı ve içeriği**
- `RoomNew.prefab` kök şablon. Hiyerarşisi: `RoomNew` (Room bileşeni) → `Grid` → tilemap'ler (`Ground`, `Spike`, `Pipes`, `PipesWithCollider`); ayrıca `DynamicContent`, `Checkpoint2`, iki `Circle` (node), `Tutorial-Text (2)`. 4 Tilemap, 3 CompositeCollider2D, 2 BoxCollider2D, 5 iç içe prefab örneği (iki `RoomTransition`, bir `Checkpoint`, iki `Circle-BG` node, bir tutorial yazısı) — [Assets/Prefab/New/Room/RoomNew.prefab](Assets/Prefab/New/Room/RoomNew.prefab)
- `Room` alanları `roomBounds`, `initialCheckpoint`, `dynamicContent` prefab içinde zaten bağlı geliyor (satır 1767–1771). Variant'lar bunları devralıyor — [Assets/Prefab/New/Room/RoomNew.prefab](Assets/Prefab/New/Room/RoomNew.prefab)
- 22 oda, `RoomNew.prefab`'ın variant'ı (`RoomNew-N Variant.prefab`). Variant dosyaları çok büyük (0.55–5.2 MB, 10k–94k satır): ezici çoğunluğu tilemap override'ı. Örnek: `RoomNew-7 Variant.prefab` 1274× `m_Tiles[].first.x/y`, 1012× `m_ColliderPaths` X/Y, 930× `m_CompositePaths` x/y override içeriyor. Geometri sprite karelerle değil, **Tilemap + TilemapCollider2D + CompositeCollider2D** ile yapılmış. Pişmiş collider yolları da variant'a override olarak yazılıyor — [Assets/Prefab/New/Room/RoomNew-7 Variant.prefab](Assets/Prefab/New/Room/RoomNew-7%20Variant.prefab)
- Oda başına iç içe prefab örneği sayısı 2–30 arası (ör. oda 16: 30, oda 15: 23, oda 11 ve 20: 21). Oda 7'de kullanılan iç içe prefab'lar: `AccelDeath Variant` (16×), `Platform_Breakable` (9×), `LaunchPad` (6×), `OneWayPlatform` (3×), `Circle-BG` node (4×), `Collectable-Slingshot` (4×), `Collectable-Keyshard`, `Door`, `RoomKeyManager`, `MovingPlatform-Transform Variant`, `WrappingDeathManager Variant`, `Collectable-Pineaplle`, 2× `RoomTransition` — [Assets/Prefab/New/Room/](Assets/Prefab/New/Room/) (prefab GUID'leri .meta dosyalarından çözüldü)
- `RoomTransition.prefab` = BoxCollider2D (trigger, 3×3) + `RoomTransitionTrigger` — [Assets/Prefab/New/RoomTransition/RoomTransition.prefab](Assets/Prefab/New/RoomTransition/RoomTransition.prefab)
- Test odaları (`RoomNew-SpiralTest`, `-SlingshotTest`, `-MovingNodeTest`) variant değil, ayrı prefab'lar. Her birinde 1 Tilemap ve 1 CompositeCollider2D var — [Assets/Prefab/New/Room/](Assets/Prefab/New/Room/)

**Sahne seviyesindeki elle bağlantı (prefab'a konamayan sahne referansları)**
- `Room` serileştirilmiş alanları: `roomId` (int), `roomTitle`, `roomBounds`, `triggers` (liste), `initialCheckpoint`, `dynamicContent`, `neighborRooms` (List<Room>), `useRoomCameraOverride`, `cameraOverrideSettings`, `cameraOverridePriority` — [Assets/Scripts/New-Scripts/LevelChange/Room.cs](Assets/Scripts/New-Scripts/LevelChange/Room.cs)
- `Room.Awake`, alt nesnelerdeki `RoomTransitionTrigger`'ları kendisi topluyor, yani `triggers` listesi aslında gereksiz. Sahnede yalnızca 2 `triggers` override'ı var — [Room.cs](Assets/Scripts/New-Scripts/LevelChange/Room.cs); [NewScene.unity](Assets/Scenes/NewScene.unity)
- `RoomTransitionTrigger` alanları: `targetRoom`, `coordinator` (sahnedeki `RoomTransitionCoordinator`), `associatedCheckpoint`, `direction` (varsayılan `Right`). `targetRoom`/`coordinator` sahne nesneleri olduğundan her trigger örneğinde sahnede elle sürükleniyor — [Assets/Scripts/New-Scripts/LevelChange/RoomTransitionTrigger.cs](Assets/Scripts/New-Scripts/LevelChange/RoomTransitionTrigger.cs)
- NewScene'de 32 `targetRoom`, 32 `coordinator`, 32 `associatedCheckpoint` override'ı; 23 `roomId`; 13 `neighborRooms.Array.size` (değerler 1–3) ve 45 `neighborRooms.Array.data[i]`; 22 `cameraOverrideSettings` ve 12 `useRoomCameraOverride`; tek bir `rooms` (RoomManager, `rooms.Array.size` = 25); tek bir `startingRoom` var. Sahnede toplam 356 PrefabInstance bulunuyor — [Assets/Scenes/NewScene.unity](Assets/Scenes/NewScene.unity)
- roomId değerleri 1–22 ve 97 (spiral test). Her değer bir kez geçiyor; RoomManager listesinde ise 25 oda var — [NewScene.unity](Assets/Scenes/NewScene.unity)
- `direction` 32 trigger'dan yalnızca 7'sinde override edilmiş (değerler: 1× Right, 4× Left, 1× Up, 1× Down). Kalan 25 trigger varsayılan `Right` ile çalışıyor — [NewScene.unity](Assets/Scenes/NewScene.unity); [TransitionDirection.cs](Assets/Scripts/New-Scripts/LevelChange/TransitionDirection.cs)
- `RoomManager` yalnızca elle doldurulan bir `List<Room> rooms` tutuyor ve `TransitionToRoom` ile komşuluğa göre oda başına `SetAsCurrent`/`SetAsNeighbor`/`Sleep` çağırıyor. Komşuluk tek yönlü (yeni odanın `NeighborRooms` listesi), simetri garantisi yok. Kullanılmayan bir `FindRoomById` var — [Assets/Scripts/New-Scripts/LevelChange/RoomManager.cs](Assets/Scripts/New-Scripts/LevelChange/RoomManager.cs)
- `LevelBootstrapper`: `startingRoom`, `roomManager`, `respawnManager`, `playerTransform` ve `cameraHandlerComponent` elle bağlanıyor. `startingRoom` null ise `Debug.LogError`. Spawn noktası `startingRoom.InitialCheckpoint` (yoksa oda pivotu) — [Assets/Scripts/New-Scripts/LevelChange/LevelBootstrapper.cs](Assets/Scripts/New-Scripts/LevelChange/LevelBootstrapper.cs)
- Trigger, checkpoint atanmamışsa yalnızca çalışma anında uyarıyor: "No checkpoint associated with this transition trigger" — [RoomTransitionTrigger.cs](Assets/Scripts/New-Scripts/LevelChange/RoomTransitionTrigger.cs)
- `RoomTransitionCoordinator` hedef odanın `RoomBounds`'ını kamera pan/confiner için kullanıyor. Trigger, coordinator'a kendi `transform.position`'ını spawn konumu olarak veriyor; oyuncu orada değil, kendi yerinde donup çözülüyor (`FreezeForTransition`/`UnfreezeFromTransition(direction)`) — [Assets/Scripts/New-Scripts/LevelChange/RoomTransitionCoordinator.cs](Assets/Scripts/New-Scripts/LevelChange/RoomTransitionCoordinator.cs)

**Son oda eklemelerinden belgeli süreç örnekleri**
- Spiral test odası (`af42fb1`): commit mesajına göre "NewScene bağlantısı (x −480, RoomManager kaydı) commit'lenmedi". Prefab oluşturma ile sahne kaydı ayrı ve elle yapılan adımlar — `git show af42fb1`
- TASKS.md: "Spiral 7: test odası … NewScene kaydedildi ama commit'lenmedi" — [TASKS.md](TASKS.md) satır 21
- Oda 4 kamera çerçevesi: "plan koordinatları yanlış çıktı, gerçek dikey boşluk ve `Square` yoluna göre ölçüldü, sen sonra elle ayarladın"; prefab değişikliği commit'lenmedi — [TASKS.md](TASKS.md) satır 140
- Oda konumları dünya koordinatında elle seçiliyor. Test odaları için bilinçli olarak uzak x değerleri kullanılıyor ("id 97, x −480, slingshot odasının yanı") — [TASKS.md](TASKS.md) satır 21
- Variant prefab'ların başka bir ajan modeli tarafından düzenlendiği commit'ler var (ör. `1fd5227 chore(level): oda 3 varyanti prefab duzenlemeleri`, +117 satır YAML) — `git show 1fd5227`

### Inferences
- Bugünkü iş akışı için tahmini adım listesi: (1) `RoomNew.prefab` variant'ı oluştur ya da var olan bir odayı kopyala; (2) tile'ları boya (collider'lar composite olarak pişiyor); (3) iç içe prefab'ları (node, tehlike, platform, toplanabilir) `DynamicContent` altına yerleştir; (4) iki `RoomTransition` örneğini taşı ve boyutlandır, `Checkpoint2`'yi taşı; (5) sahneye örnekle ve dünya koordinatında konumla (odalar arası boşluk/hizalama elle); (6) `roomId` ve `roomTitle`'ı ayarla; (7) RoomManager `rooms` listesine ekle; (8) bu odanın ve komşu odaların `neighborRooms` listelerini düzenle; (9) her trigger'a `targetRoom`, `coordinator`, `associatedCheckpoint` sürükle ve `direction` seç; (10) gerekirse kamera override'ı veya `CameraOverrideZone` ekle; (11) MP `RaceFlagPoint` menü komutunu çalıştır ve bayrağı elle taşı; (12) moodboard ekran görüntüsü al (GDD §8.1). Adım 7–9, oda başına en az 7 referans ve iki odayı birden düzenlemeyi gerektiriyor.
- Prefab'a sahne referansı konamadığı için `coordinator` (32 trigger'ın hepsinde aynı nesne) her trigger için tekrar tekrar sürükleniyor. Bu tamamen mekanik bir iş ve otomatikleştirilebilir (coordinator `FindFirstObjectByType` ile veya bir menü komutuyla atanabilir).
- `direction` alanının 25 trigger'da varsayılan değerde kalması iki şekilde açıklanabilir: ya bu geçişlerin hepsi gerçekten sağa açılıyor, ya da elle seçilmesi unutulmuş. Yön, haptik profilini, `UnfreezeFromTransition(direction)` davranışını ve `RaceFlagPlacer.InFrontOf`'u etkiliyor. Trigger kutusunun hedef odanın `RoomBounds`'ına göre konumundan yön geometrik olarak çıkarılabilir. Uyuşmazlıklar bir doğrulayıcının ilk bulgu adayı (Editor'de doğrulanmadı).
- Variant'ların MB'larca tilemap override'ı yüzünden prefab diff'i okunamıyor ve elle YAML düzenlemek pratik değil. Bu durum, Editor-içi araçlara (`unity command eval`) yönelmeyi destekliyor.

### Gaps
- Adım başına süre için depoda ölçüm yok. TASKS.md'deki `#level` görevleri süre kaydı tutmuyor. Süre tahmini geliştiriciye sorulmalı.
- 25 oda kaydı ile 23 roomId override'ı arasındaki fark doğrulanmadı. Muhtemelen iki test odası (`MovingNodeTest`, `SlingshotTest`) roomId 0 (varsayılan) ile duruyor, ama Editor'de bakılmadı.
- Hangi trigger'ın yönünün yanlış olduğu, sahne nesne kimlikleri çözülmeden tespit edilemedi.

## 2. Git geçmişinde, bir doğrulayıcının yakalayabileceği bağlantı hatalarına işaret eden hangi hatalar ve düzeltmeler var?

### Takeaway
Geçmişte açıkça bağlantı ya da veri hatası olan birkaç düzeltme var: yanlış roomId, başlangıç odası referansı, kamera override'ının odalar arasında sızması ve commit'lenmemiş sahne kayıtları. Çoğu, sahnedeki odaları tarayan statik bir EditMode testi veya menü doğrulayıcısıyla yakalanabilirdi. Toplam 277 commit taranarak oda/kamera/checkpoint/geçiş başlıklı olanlar incelendi.

### Cited Findings
- `6a3aef9 fix(level): RoomNew-22'nin oda kimligi 22`. Commit mesajı: yarış oda kimliğini sıra numarası olarak kullanıyor; 0, "bilinmiyor" anlamıyla karışıyordu. Yalnızca `NewScene.unity` değişti (+4 satır). Tekil/sıralı/sıfır olmayan roomId kontrolüyle yakalanırdı — `git show 6a3aef9`
- `933631c chore(scene): NewScene baslangic odasi referansi`: `startingRoom` referansının düzeltilmesi, tek satırlık sahne değişikliği — `git show 933631c`
- `336b8f7 fix(camera): ışınlamada eski odanın kamera override'ı yeni odaya taşınmasın`: komşuda kalan bir oda, geri dönülen odanın zoom'unu eziyordu. Düzeltme `Room.cs`'e (ayrılırken `UnregisterOverride`) ve `CameraController.cs`'e girdi. Bu bir kod hatası, ama override'lı oda/komşu kombinasyonlarını çalışma anında tarayan bir test bunu gösterirdi — `git show 336b8f7`
- `6464072 fix(camera): oda geçişinin sonundaki kamera titremesini gider`: Play Mode ölçümüyle (oda 2→3) bulunmuş; kod düzeltmesi — `git show 6464072`
- `2f29a34 fix(death): iris merkezi kamerayi izlesin`: oda 20'de kamera override'ı varken ölünce ekranın köşesi kapalı kalıyordu — [TASKS.md](TASKS.md) satır 138
- `af42fb1` ve TASKS satır 21: oda prefab'ı commit'lendi, ama sahne kaydı (RoomManager, konum) commit'lenmeden kaldı. Prefab var olduğu hâlde sahnede kayıtlı olmayan odayı bir "orphan room" kontrolü yakalardı — `git show af42fb1`
- MP bayrak yerleştirme (`d5a0038`): menü komutu, eksik odalara bayrağı RoomId+1 odasına giden trigger'ın önüne koyuyor; bulamazsa "merkezde, elle taşı" diye raporluyor. Oda 21 ve 22'nin bayrakları oda ortasında havada kaldı ("mutlaka taşı") — `git show d5a0038`; [TASKS.md](TASKS.md) satır 45
- `RaceFlagPlacer.PlaceMissing`, RoomId'si 0 olan odaları atlıyor ("sıra bilinmiyor"). `PlaceFinishIfMissing`, "RoomId − 1'den geçişle girilen en büyük kimlikli oda" mantığıyla ana yolu çıkarıyor. Yani roomId sırası ve trigger grafiği kodun güvendiği bir sözleşme, ama doğrulanmıyor — [Assets/Scripts/New-Scripts/Multiplayer/Flags/RaceFlagPlacer.cs](Assets/Scripts/New-Scripts/Multiplayer/Flags/RaceFlagPlacer.cs)
- `PenaltyHandler`, `RoomId` ve `NeighborRooms` üzerinden ceza hedef odasını buluyor. `RoomPlacement` ise ışınlamada `InitialCheckpoint`'i kullanıyor, yoksa oda pivotuna düşüyor. Eksik checkpoint, MP cezasında oyuncuyu oda pivotuna koyar — [Assets/Scripts/New-Scripts/Multiplayer/Penalty/PenaltyHandler.cs](Assets/Scripts/New-Scripts/Multiplayer/Penalty/PenaltyHandler.cs); [RoomPlacement.cs](Assets/Scripts/New-Scripts/Multiplayer/Penalty/RoomPlacement.cs)
- `LocalPlayerBroadcaster` ve HUD, oyuncunun odasını `RoomBounds.OverlapPoint` ile buluyor. Örtüşen veya boşluklu `RoomBounds`, yanlış oda kimliğine yol açar — [LocalPlayerBroadcaster.cs](Assets/Scripts/New-Scripts/Multiplayer/Ghost/LocalPlayerBroadcaster.cs); [TASKS.md](TASKS.md) satır 184
- Swing tutorial bulgusu: node menzile tepede ~9.65/10 ile giriyor ("marj küçük", ~0.35 birim). Bu bir erişilebilirlik (reachability) marjı sorunu ve PlayMode testinde elle ölçüldü — [TASKS.md](TASKS.md) satır 132–135
- grapple.md: spiral/dönen node'un etrafında yaklaşık 10.5 birimlik bir dairenin boş kalması gerekiyor (`detectionRadius` 10 + ip), aksi hâlde duvar kancayı koparır. Bu, geometrik olarak kontrol edilebilir bir level-design kuralı — [docs/systems/grapple.md](docs/systems/grapple.md) satır 103
- camera.md: `FrameArea` görüşü oda sınırını aşınca gizmo turuncuya dönüyor ("taşan yerde uyuyan oda/boşluk olmasın"); şu an yalnızca görsel uyarı — [docs/systems/camera.md](docs/systems/camera.md) satır 35, 42

### Inferences
Bir oda doğrulayıcısının yakalayabileceği kurallar (kod ve geçmişten türetildi):
- (a) roomId tekil, >0 ve RoomManager listesindeki her oda için tanımlı olmalı.
- (b) Sahnedeki her `Room` RoomManager listesinde olmalı (orphan yok); `startingRoom` listede olmalı.
- (c) Her trigger'ın `targetRoom`, `coordinator` ve `associatedCheckpoint` alanı dolu olmalı.
- (d) `targetRoom`, kaynak odanın `neighborRooms` listesinde olmalı. Aksi hâlde geçiş anında hedef oda uyuyor olabilir; koordinatör pan'den sonra uyandırsa da ön yükleme kaybolur.
- (e) Komşuluk simetrik olmalı, en azından uyarı düzeyinde.
- (f) Trigger kutusu hedefin `RoomBounds`'ına bitişik veya örtüşük olmalı; `direction` geometriyle uyumlu olmalı.
- (g) `initialCheckpoint` atanmış olmalı ve `RoomBounds` içinde durmalı.
- (h) `RoomBounds`'lar birbiriyle örtüşmemeli.
- (i) Her odada RaceFlagPoint bulunmalı ve yerleştirici fallback'e düşmemiş olmalı.
- (j) Spiral/dönen node etrafında boşluk kuralı; node'un en yakın zeminden `detectionRadius` içinde olması.

### Gaps
- Geçmişte "trigger yanlış odaya gidiyor" gibi açık bir hata commit'i bulunmadı. Sahne düzeltmeleri çoğunlukla açıklamasız `chore(scene)` commit'lerine gömülü, bu yüzden bağlantı hatalarının gerçek sıklığı bilinmiyor.
- Eski commit'ler (ör. `90dcf76`, "platform bug'u var") ayrıntısız; sınıflandırılamadı.

## 3. Araçların yeniden kullanabileceği hangi veriler ve kod parçaları var?

### Takeaway
Erişilebilirlik gizmosu için gereken fiziğin hepsi `PlayerStatsSO`'da türetilmiş özellikler olarak hazır (Gravity, JumpVelocity, DashSpeed). `LaunchPad` zaten `PlayerStatsSO` ile adım adım yörünge simülasyonu çiziyor. Platform stratejilerinde `TryGetPathBounds` var. `RaceFlagPlacer`, odaları ve trigger'ları tarayıp Undo'lu menü komutu çalıştıran hazır bir örnek. Ghost `SnapshotBuilder`/`GhostInterpolator`, yörünge kaydı için uygun bir veri modeli sunuyor.

### Cited Findings
**Oyuncu sayıları (PlayerStats.asset'teki gerçek değerler, koddaki varsayılanlardan farklı)**
- `MoveSpeed` 10, `JumpHeight` 4, `TimeToApex` 0.5 (kod varsayılanı 0.4); `Gravity = -(2·JumpHeight)/TimeToApex²` (= −32), `JumpVelocity = |Gravity|·TimeToApex` (= 16); `FallGravityMultiplier` 1.5, `JumpEndEarlyGravityMultiplier` 3, `ApexThreshold` 3.5, `ApexHangGravityMultiplier` 0.5, `TerminalVelocity` −32, `AirAcceleration` 60, `AirOverspeedReduce` 20, `CoyoteTimeDuration` 0.1 — [Assets/Scripts/New-Scripts/Player/SO/PlayerStats.asset](Assets/Scripts/New-Scripts/Player/SO/PlayerStats.asset); [PlayerStatsSO.cs](Assets/Scripts/New-Scripts/Player/PlayerStatsSO.cs)
- Dash: `DashDistance` 7, `DashDuration` 0.16, `DashSpeed => DashDistance/DashDuration` — [PlayerStats.asset](Assets/Scripts/New-Scripts/Player/SO/PlayerStats.asset)
- Duvar: `WallJumpForce` (11, 14), `WallSlideJumpForce` (11, 13), `ClimbSpeed` 8, `MaxWallStamina` 6, `MaxWallSlideTime` 2 — [PlayerStats.asset](Assets/Scripts/New-Scripts/Player/SO/PlayerStats.asset)
- Swing/slingshot: `MaxSwingSpeed` 28, `SwingGravity` 25, `SwingInitialBoost` 5, `MaxSlingshotSpeed` 65, `SwingHopForwardSpeed` 10, `SwingHopUpwardSpeed` 12 — [PlayerStats.asset](Assets/Scripts/New-Scripts/Player/SO/PlayerStats.asset)
- `PlayerStatsSO.HoldJumpSpeed(...)` statik yardımcısı mevcut — [PlayerStatsSO.cs](Assets/Scripts/New-Scripts/Player/PlayerStatsSO.cs) satır 145
- Kanca menzili: `NodeDetector.detectionRadius` sahnede **10** (kod varsayılanı 15), `minimumDotProduct` 0.5. Görüş hattı `obstacleLayerMask` raycast'iyle kontrol ediliyor. Bu değer `PlayerStatsSO`'da değil, sahnedeki bileşende duruyor — [Assets/Scripts/New-Scripts/Player/NodeDetector.cs](Assets/Scripts/New-Scripts/Player/NodeDetector.cs); [NewScene.unity](Assets/Scenes/NewScene.unity) satır 125372; [docs/systems/grapple.md](docs/systems/grapple.md) satır 22, 136
- Katmanlar: Ground = 6, Grapple (node) = 7 (PlayMode test sabitleri); `RaceFlagPlacer.SnapToGround` "Ground" ve "OneWay" katmanlarını kullanıyor — [PlayerRuleTestBase.cs](Assets/Tests/PlayMode/PlayerRuleTestBase.cs); [RaceFlagPlacer.cs](Assets/Scripts/New-Scripts/Multiplayer/Flags/RaceFlagPlacer.cs)

**Var olan gizmo ve simülasyon kodu**
- `LaunchPad.DrawTrajectoryGizmo`: `playerStats.Gravity`, `FallGravityMultiplier`, `JumpEndEarlyGravityMultiplier`, `TerminalVelocity`, `AirDrag` ile `trajectorySteps` adımlık yörünge çiziyor. Zıplama/dash yayı gizmosu için doğrudan şablon — [Assets/Scripts/New-Scripts/Platform/LaunchPad.cs](Assets/Scripts/New-Scripts/Platform/LaunchPad.cs) satır 243+
- `AcceleratingPingPongStrategy`, `CircularMovementStrategy` ve `TransformPingPongStrategy` `OnDrawGizmos` ile yol ve uç noktaları çiziyor; `AcceleratingPingPong` bir `Handles.Label` ile max yükseklik yazıyor — [Assets/Scripts/New-Scripts/Platform/](Assets/Scripts/New-Scripts/Platform/)
- `MovementStrategy.TryGetPathBounds(out Rect)` sanal metodu altı stratejide override edilmiş (`AcceleratingPingPong`, `Circular`, `LinearPingPong`, `SinePingPong`, `TransformPingPong`). `CameraOverrideZoneEditor` bunu "Çerçeveyi tehlikeye sığdır" düğmesinde kullanıyor — [MovementStrategy.cs](Assets/Scripts/New-Scripts/Platform/MovementStrategy.cs) satır 36; [CameraOverrideZoneEditor.cs](Assets/Scripts/New-Scripts/Editor/CameraOverrideZoneEditor.cs)
- `GrappleGizmoVisualizer`: node katmanındaki tüm collider'ların çevresine sabit `swingRadius` (5) daire çiziyor. `detectionRadius`'u okumuyor ve her `OnDrawGizmos`'ta `OverlapCircleAll(…, float.MaxValue)` yapıyor; menzil gizmosu için zayıf ama var olan bir başlangıç — [Assets/Scripts/New-Scripts/Player/GrappleGizmoVisualizer.cs](Assets/Scripts/New-Scripts/Player/GrappleGizmoVisualizer.cs)
- `RaceFlagPoint`/`RaceFinishPoint`, `OnDrawGizmos` + `Handles.Label("Bayrak {RoomId}")` ile oda kimliğini sahnede gösteriyor — [RaceFlagPoint.cs](Assets/Scripts/New-Scripts/Multiplayer/Flags/RaceFlagPoint.cs)
- `LevelChange/` içinde hiçbir gizmo yok: oda sınırları, trigger→hedef okları ve komşuluk grafiği sahnede görselleştirilmiyor — `grep OnDrawGizmos Assets/Scripts/New-Scripts` sonucu

**Editör aracı kalıpları**
- `CameraOverrideZoneEditor`: `[CustomEditor]` + `BoxBoundsHandle` sahne tutamacı + `SerializedProperty` ile güvenli yazım + Türkçe `HelpBox` — [Assets/Scripts/New-Scripts/Editor/CameraOverrideZoneEditor.cs](Assets/Scripts/New-Scripts/Editor/CameraOverrideZoneEditor.cs)
- `SegmentedBreakablePlatformEditor` (43 satır), `PlatformStrategyConverterWindow` (`[MenuItem("Tools/Platform Strategy Converter")]` EditorWindow), `RampTextureGeneratorWindow`, `NPCDialogueSetupEditor` (GameObject/ ve Tools/ menü kurulum komutları, validate fonksiyonlarıyla) — [Assets/Scripts/New-Scripts/Editor/](Assets/Scripts/New-Scripts/Editor/)
- Yeni projeye özgü menü kökü "Rock The Game/…" (ör. `Rock The Game/Multiplayer/Bayrak işaretlerini yerleştir`, `Rock The Game/Ana menü sahnesini kur`, `Rock The Game/Build/Windows`). Kalıp: `FindObjectsByType<Room>(FindObjectsInactive.Include, …)`, `Undo.RegisterCreatedObjectUndo`, `EditorSceneManager.MarkSceneDirty`, `Physics2D.SyncTransforms()` ardından raycast, `Debug.Log` ile Türkçe rapor. Saf mantık runtime'da (`PlaceMissing` bir rapor nesnesi döndürüyor), menü kısmı `#if UNITY_EDITOR` içinde — [RaceFlagPlacer.cs](Assets/Scripts/New-Scripts/Multiplayer/Flags/RaceFlagPlacer.cs); [MainMenuBuilder.cs](Assets/Scripts/New-Scripts/Multiplayer/Lobby/MainMenuBuilder.cs)
- Assembly'ler: `New-Scripts` → `RockTheGame.Runtime`, `New-Scripts/Editor/` → `RockTheGame.Editor`; Multiplayer'ın kendi asmdef'i var (`RockTheGame.Multiplayer.asmdef`) — [CLAUDE.md](CLAUDE.md); [Assets/Scripts/New-Scripts/Multiplayer/](Assets/Scripts/New-Scripts/Multiplayer/)

**Play-from-here bağlantıları**
- `RespawnManager.OverrideInitialSpawnPoint(Transform)` public. `LevelBootstrapper` başlangıç odasının `InitialCheckpoint`'ini buna veriyor ve kamerayı `cameraHandler.SnapToRoomBounds(bounds, spawnPos, size, zoom)` ile kuruyor, sonra `roomManager.TransitionToRoom(startingRoom)` çağırıyor — [LevelBootstrapper.cs](Assets/Scripts/New-Scripts/LevelChange/LevelBootstrapper.cs); [RespawnManager.cs](Assets/Scripts/New-Scripts/Death/RespawnManager.cs)
- MP `RoomPlacement`, bir odaya ışınlamanın (spawn = `InitialCheckpoint`, `ActivateCheckpoint()`, kamera ve oda geçişi) kodlanmış hâli. Ceza ve "kayıtlı yarışa devam" bunu kullanıyor. Play-from-room için doğrudan yeniden kullanılabilir — [Assets/Scripts/New-Scripts/Multiplayer/Penalty/RoomPlacement.cs](Assets/Scripts/New-Scripts/Multiplayer/Penalty/RoomPlacement.cs)
- `Checkpoint.ActivateCheckpoint()`, respawn noktasını `TransformEventChannelSO` üzerinden yayınlıyor — [Checkpoint.cs](Assets/Scripts/New-Scripts/Death/Checkpoint.cs)
- Sahnede `startingRoom` değiştirmek, şu an başka bir odadan başlamanın tek yolu (`933631c` bu alanın commit'lenmiş bir değişikliği) — `git show 933631c`

**Yörünge kaydı için ghost verisi**
- `PlayerPoseInput`/`SnapshotMessage`: RoomId, Position, Velocity, FacingRight, IsGrounded/IsSwinging/IsDashing/IsWallClimbing/IsWallSliding, LeftAnchor/RightAnchor, Teleported, IsHidden. `SnapshotBuilder.Build(input, realtimeSeconds)`, sıra numarasını `GhostTiming.SnapshotRate` ile üretiyor; ışınlanmayı hız eşiğiyle tespit ediyor — [Assets/Scripts/New-Scripts/Multiplayer/Ghost/SnapshotBuilder.cs](Assets/Scripts/New-Scripts/Multiplayer/Ghost/SnapshotBuilder.cs)
- `GhostInterpolator` en fazla 32 snapshot tamponluyor ve ağ zaman ekseni için tasarlanmış (offset tahmini, ışınlanmada tampon temizleme). Çevrimdışı tam kayıt için değil — [GhostInterpolator.cs](Assets/Scripts/New-Scripts/Multiplayer/Ghost/GhostInterpolator.cs)
- `LocalPlayerBroadcaster`, `PlayerPoseInput`'u `PlayerController`'dan dolduruyor ve odayı `RoomBounds.OverlapPoint` ile buluyor — [LocalPlayerBroadcaster.cs](Assets/Scripts/New-Scripts/Multiplayer/Ghost/LocalPlayerBroadcaster.cs)

**Testler (doğrulayıcı testi için kalıplar)**
- EditMode'da asset tabanlı "nöbetçi" testi: `PlayerStatsSanityTests`, `AssetDatabase.LoadAssetAtPath<PlayerStatsSO>` ile değerleri GDD bantlarına karşı kontrol ediyor, Türkçe hata mesajları veriyor — [Assets/Tests/EditMode/PlayerStatsSanityTests.cs](Assets/Tests/EditMode/PlayerStatsSanityTests.cs)
- PlayMode tabanı: `PlayerRuleTestBase`, `NewScene`'i yüklüyor, `FakeInput`'u yansımayla enjekte ediyor, oyuncuyu odalardan uzağa (5000, 5000) taşıyor ve geometriyi testin kendisi kuruyor — [Assets/Tests/PlayMode/PlayerRuleTestBase.cs](Assets/Tests/PlayMode/PlayerRuleTestBase.cs)
- EditMode'da 40 civarı test dosyası var, çoğu saf matematik (`CameraZoneMathTests`, `MovementStrategyTests`, `IrisMathTests` …). Sahne veya prefab tarayan bir oda doğrulama testi yok — [Assets/Tests/EditMode/](Assets/Tests/EditMode/)
- Stop hook'u (`check-compile.sh`) yalnızca .cs/.asmdef parmak izi değişince derleme ve EditMode testi çalıştırıyor. Sahne veya prefab değişikliği bu kapıyı tetiklemiyor — [.claude/hooks/check-compile.sh](.claude/hooks/check-compile.sh)

**Claude Code otomasyonu**
- `.claude/settings.json`: `*.unity` dosyalarında Edit/Write yasak; `git push` onay istiyor; force-push/reset/clean/branch -D yasak. SessionStart hook'u `deadlines.ps1`, Stop hook'u `check-compile.sh`. Depoda proje skill'i (`.claude/skills`) yok — [.claude/settings.json](.claude/settings.json)
- Sahne işi `unity command eval '<C#>'` ile yapılıyor. Ekran görüntüsü `capture_game_view` ile alınıyor; `save_path` `Assets/` altında olmalı, sonra dosya dışarı taşınıp kalan `.meta` siliniyor — [CLAUDE.md](CLAUDE.md)
- Moodboard: 22 oda görseli `docs/moodboard/level_01..22.png` altında (1920×1080, CRT dahil, oyuncu/HUD yok, oyun kamerasından 2–5 kat uzak). Elle/ajanla çekilmiş, bunu üreten bir script yok — [docs/GDD.md](docs/GDD.md) satır 178; [docs/moodboard/](docs/moodboard/)

### Inferences
- `detectionRadius` sahnedeki bileşende durduğu için bir erişilebilirlik aracı onu `NodeDetector`'dan okumalı (ya da değer `PlayerStatsSO`'ya taşınmalı). Taşımak bir tasarım ve kod kararı; CLAUDE.md tunable'ların SO'da olmasını tercih ediyor.
- `LaunchPad`'in simülasyon döngüsü ile `RaceFlagPlacer`'ın tarama ve rapor yapısı birleştirilirse, yeni kod yazmadan önce kalıp yeniden kullanılmış olur (CLAUDE.md kural 2: "Reuse an existing helper").
- Ghost snapshot yapısı, bir playtest kaydedicisi için hazır bir şema. Ancak `GhostInterpolator` 32'lik tamponu nedeniyle kayıt için değil, yalnız oynatma/ara değer için uygun; kayıt tarafında `SnapshotBuilder` + bir liste yeterli. Not: Multiplayer ayrı bir assembly; Runtime'dan bağımlılık yönü kontrol edilmeli.

### Gaps
- `AirDrag`, `MomentumDecay` ve `AirControlMultiplier` asset değerleri okunmadı (yalnızca kod varsayılanları: 5, 2, 0.5).
- `GhostTiming.SnapshotRate`'in değeri okunmadı.
- `RockTheGame.Multiplayer.asmdef`'in Runtime'a ya da Runtime'ın ona referans verip vermediği kontrol edilmedi.

## 4. Beklenen zaman kazancına göre sıralı araç fırsatları (dokunacağı dosyalarla)

### Takeaway
En yüksek getiri; ucuz, sıfır gameplay riski taşıyan ve var olan kalıplara oturan araçlarda: (1) bir oda bağlantı doğrulayıcısı (menü + EditMode testi), (2) oda/trigger/komşuluk gizmoları, (3) "yeni odayı kaydet/otomatik bağla" komutu, (4) buradan oyna (play-from-room). Erişilebilirlik gizmoları ve yörünge kaydı daha pahalı ama level tasarım kalitesine doğrudan dokunuyor.

### Cited Findings (fırsatların dayandığı kanıt)
- Oda başına elle bağlantı sayısı ve tekrar (32 coordinator/targetRoom/checkpoint, 13 komşu listesi, 25'lik RoomManager listesi) — [NewScene.unity](Assets/Scenes/NewScene.unity)
- Geçmişteki roomId ve startingRoom düzeltmeleri, commit'lenmemiş sahne kayıtları — `git show 6a3aef9`, `933631c`, `af42fb1`
- Var olan tarama, Undo ve menü kalıbı — [RaceFlagPlacer.cs](Assets/Scripts/New-Scripts/Multiplayer/Flags/RaceFlagPlacer.cs)
- Var olan yörünge simülasyonu — [LaunchPad.cs](Assets/Scripts/New-Scripts/Platform/LaunchPad.cs)
- GDD hedefi ~60 oda, şu an 22. Altı hikâye seviyesi ve yeni seviye fikirleri TASKS'ta bekliyor — [docs/GDD.md](docs/GDD.md) satır 3, 18, 279; [TASKS.md](TASKS.md) satır 84–95

### Inferences (sıralı liste; zaman kazancı tahmini, ölçülmedi)
1. **Oda bağlantı doğrulayıcısı** (kurallar a–j, §2). `Rock The Game/Level/Odaları doğrula` menüsü ve aynı mantığı çağıran bir EditMode testi (`EditorSceneManager.OpenScene("Assets/Scenes/NewScene.unity")` ile). Getirisi her yeni odada ve her sahne değişikliğinde tekrar ediyor, CI yerine Stop hook'u veya elle çalıştırılabiliyor. Dosyalar: yeni `Assets/Scripts/New-Scripts/Editor/RoomValidator.cs` (veya saf mantık için `LevelChange/RoomValidation.cs` + Editor menüsü), yeni `Assets/Tests/EditMode/RoomWiringTests.cs`; okumak için `Room.cs`, `RoomTransitionTrigger.cs` ve `RoomManager.cs`'e yalnızca public getter (ör. `RoomManager.Rooms`, trigger `Coordinator`/`AssociatedCheckpoint`) eklemek gerekebilir. `RaceFlagPlacer` `TargetRoom`/`Direction` getter'larını zaten eklemiş. İsteğe bağlı: `.claude/hooks/check-compile.sh`'e sahne değişikliğinde bu testi `--filter` ile koşturmak.
2. **LevelChange gizmoları.** `Room.OnDrawGizmos` (RoomBounds kutusu + `Handles.Label("Oda {roomId} {roomTitle}")`), trigger → hedef odaya ok ve yön oku, komşuluk çizgileri, eksik referansta kırmızı renk. Dosyalar: `Room.cs`, `RoomTransitionTrigger.cs` (`#if UNITY_EDITOR` bloğu; `RaceFlagPoint` kalıbı). Düşük risk, eksik bağlantıyı anında gösterir.
3. **Oda kaydı ve otomatik bağlama komutu.** Seçili odayı RoomManager'a ekler, boş roomId'ye bir sonraki tekil kimliği verir, tüm trigger'lara sahnedeki tek `RoomTransitionCoordinator`'ı atar, `associatedCheckpoint`'i hedef odanın `InitialCheckpoint`'i yapar (mevcut davranışa bakılarak doğrulanmalı), trigger'ın hedef bounds'a göre konumundan `direction` önerir, targetRoom'u komşu listelerine (iki yönlü) ekler. Hepsi Undo ile. Dosyalar: yeni `Editor/RoomWiringTool.cs`; `RaceFlagPlacer` kalıbı. Alternatif olarak `coordinator` alanı kaldırılıp çalışma anında bulunabilir (`RoomTransitionTrigger.cs`), ama bu bir kod/tasarım değişikliği.
4. **Buradan oyna.** Sahnede seçili oda (veya fare konumu) ile Play'e gir: `EditorPrefs`'e oda kimliği yaz, `LevelBootstrapper.SetupLevel` bunu okuyup `startingRoom` yerine kullansın (yalnız Editor'de). `RoomPlacement` ile aynı ışınlama yolunu izler. Sahnede `startingRoom` değiştirip commit'leme hatasını (`933631c`) ortadan kaldırır. Dosyalar: `LevelBootstrapper.cs`, yeni `Editor/PlayFromRoom.cs`; muhtemelen `Multiplayer/Penalty/RoomPlacement.cs` yeniden kullanılır (assembly yönü kontrol edilmeli).
5. **Erişilebilirlik ve menzil gizmoları.** Seçili node'un çevresine `NodeDetector.detectionRadius` dairesi, spiral/dönen node için 10.5 birimlik boş disk kontrolü, yerden zıplama/dash/duvar zıplama yayları (`PlayerStatsSO` değerleriyle, `LaunchPad.DrawTrajectoryGizmo` döngüsü üzerinden). Dosyalar: `Player/GrappleGizmoVisualizer.cs` (menzil kaynağını düzelt), yeni `Editor/ReachabilityGizmo.cs`; ortak simülasyon için `LaunchPad.cs`'teki döngü saf bir yardımcıya çıkarılabilir (EditMode testli). Sınır: gizmo, `KinematicPhysicsHandler` çarpışma çözümünü, apex hang'i ve hava kontrolünü kabaca taklit eder; kesin değildir (oyun hissi kararı insanda, kural 4).
6. **Moodboard ve oda ekran görüntüsü komutu.** Tüm odaları sırayla kadrajlayıp `docs/moodboard/level_NN.png` üretir; `capture_game_view` sınırlamalarını (Assets/ altına yazıp dışarı taşıma) sarmalar. Dosyalar: yeni `Editor/RoomSnapshotter.cs` ya da bir Claude skill/script (`.claude/skills/` şu an yok).
7. **Playtest yörünge kaydedicisi.** Play'de `SnapshotBuilder` ile oda başına poz akışını JSON'a yazar, Editor'de çizgi olarak gösterir (ölüm noktaları ısı haritası). Dosyalar: yeni `Multiplayer/Ghost/` veya ayrı bir `Debug/` kaydedici; `LocalPlayerBroadcaster.cs` doldurma mantığı yeniden kullanılır. Getirisi daha belirsiz, çünkü oyun hissi değerlendirmesi insanın işi.
8. **Odaya özgü belge.** docs/systems/README, "oda ve geçiş sistemi (`LevelChange/`)" dosyasının henüz olmadığını söylüyor. Doğrulayıcının kuralları için bir `docs/systems/rooms.md` gerekecek — [docs/systems/README.md](docs/systems/README.md)

### Gaps
- Zaman kazancı tahminleri ölçüme dayanmıyor. Geliştiricinin bir oda eklemek için harcadığı süre ve en sık yaptığı hatalar ona sorulmalı.
- Hikâye seviyeleri (conveyor, fabrika, reels) yeni mekanik gerektiriyor. Mevcut `RoomNew` şablonunun bunlara uyup uymadığı değerlendirilmedi.
- "Yalnızca odayı gösterme" araştırması (TASKS satır 99) komşu odaların aktif kalmasını değiştirebilir; doğrulayıcının komşuluk kurallarını etkiler.
- Kullanıcının hafızasındaki bir kural: Opus planlar ve TASKS.md'ye görev yazar, Sonnet uygular. Uygulama sırası ve onay geliştiricide.
