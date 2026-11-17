# Hareketli node'lar: tasarım

*2026-11-17. Branch `feature/moving-nodes` (`feature/normal-state`'in ucundan). TASKS.md: "Hareketli node'lar".*

## Niyet

Node'lar hareketli platformlar gibi hareket edebilsin. İki kullanım da olsun:

1. **Taşıma:** oyuncu ileri giden bir node'a tutunur ve boşluğu onunla geçer.
2. **Fırlatma:** bırakınca node'un hızı oyuncuya geçer. Oyuncu node'un hızını kullanarak kendini fırlatır.

Bugün anchor, tutunma anındaki `Vector2` kopyası. Node gidince ip havada kalıyor (`docs/systems/grapple.md` kenar durum #8).

## Hedefler / hedef olmayanlar

- **Hedef:** anchor node'u izler. Üç kanca durumunda da (tek, çift, slingshot) oyuncu node'la birlikte taşınır. Bırakışta node hızı aktarılır.
- **Hedef:** sabit node'larda davranış birebir aynı kalır. Mevcut EditMode ve PlayMode testleri değişmeden yeşil kalmalı.
- **Hedef değil:** hareketli dönen node (`FullRotationNode`). Teoride çalışır (tracker anchor'ın güncel konumunu okur), ama test edilmeyecek. Kullanıcı kararı.
- **Hedef değil:** yeni bir mover. Node'u hareket ettirmek için mevcut `PlatformController` + `MovementStrategy` kullanılır.
- **Hedef değil:** level tasarımı. Odalara yerleştirme ayrı iş.
- **Anti-hedef:** sabit node'da his değişirse, anchor'ı okuyan kamera, nişangâh, ghost ya da tutorial bozulursa, iş başarısızdır.

## Yaklaşımlar

| | Ne | Artı | Eksi |
|---|---|---|---|
| **A (seçildi)** | `LeftAnchor`/`RightAnchor` (`Vector2?`) kalır. Yanına node'un `Transform`'u tutulur, anchor her fizik adımında yenilenir. | Küçük diff. Anchor'ı okuyan 15'ten fazla yer (kamera, nişangâh, ghost, multiplayer, tutorial) değişmez. | İki alanın tutarlı kalması gerekir. Çözüm aşağıda. |
| B | Anchor bir struct'a çevrilir (`node`, `Position`, `Velocity`). | Daha temiz model. | Bütün okuyucular ve multiplayer snapshot kodu değişir. Bu task için fazla. |

Kullanıcı A'yı seçti (2026-11-17).

## Tasarım

### 1. Anchor node'u izler (`PlayerController`)

- `public Transform LeftAnchorNode { get; private set; }` ve aynısı sağ kol için eklenir.
- `LeftAnchor`/`RightAnchor` setter'ı node'u temizler. Ham nokta atamak (`= null`, `= hitPoint`, testlerdeki `Player.RightAnchor = anchor`) sabit anchor demektir. Böylece kodda anchor'ı silen ~15 satıra dokunulmaz.
- Yeni bir `AttachAnchor(ActiveArm arm, Vector2 point, Transform node)` metodu noktayı ve node'u birlikte kurar.
- `NodeDetector.TryFindBestNode` ve `PlayerController.TryCastGrapple`, buldukları collider'ın `Transform`'unu da döndürür (ek `out Transform node`). Kanca atılan 5 yer `AttachAnchor`'a geçer:
  - `NormalState`: 2 yer
  - `SwingingState.CheckOppositeGrappleCast`: 1 yer
  - `WallSlidingState`: 2 yer
- `PlayerController.FixedUpdate`, `CurrentState.FixedUpdateState()`'ten **önce** `RefreshMovingAnchors()` çağırır. Node'u olan her kol için:
  - `LeftAnchorDelta = node.position − eski anchor`
  - `LeftAnchorVelocity = delta / fixedDeltaTime`
  - Anchor, setter'ı atlayarak (backing field) yeni konuma yazılır.
  - Node'u olmayan kolda delta ve hız `Vector2.zero`'dır.
- Node yok olursa ya da pasifleşirse (oda uyur, `transform == null`, `!activeInHierarchy`) anchor sabit kalır. Kanca kopmaz, node izlemesi durur.

### 2. Taşıma: referans çerçevesi

Durumların kendi hızı **node'a göre** kalır. Node'un hareketi, hareketli platformdaki `DeltaPosition` gibi, ayrı bir konum kayması olarak eklenir. Böylece sarkaç ve yay matematiği ile hız sınırları (`MaxSwingSpeed`) değişmez, damping de node'un hareketine direnmez.

- **Tek kanca (`SwingingState`):** `_anchorPoint` her adımda context'ten tekrar okunur. Sarkaç kısıtından önce oyuncunun konumuna o kolun `AnchorDelta`'sı eklenir, yani ip boyu korunur. Çarpışma taraması bu kaymayı da kapsar. Hop aşamasında anchor güncellenir, hop hızı değişmez. İp boyu hop bitince o anki mesafeye sabitlenir (bugünkü gibi).
- **Çift kanca (`DualSwingingState`):** yaylar güncel anchor'lara çeker. Taşıma için iki anchor'ın delta ortalaması eklenir. Biri sabit, biri hareketliyse oyuncu yarı hızla taşınır, yaylar farkı kapatır. Kayma, durumun hız hesabına karışmadan `KinematicPhysicsHandler.Move` üzerinden, çarpışma çözülerek uygulanmalı.
- **Slingshot (`SlingshotState`):** `midpoint` her adımda güncel anchor'lardan yeniden hesaplanır. Gerilmede ve fırlatmada oyuncuya node deltası eklenir. Fırlatma yönü bugünkü gibi girişte sabitlenir. Hedefi "geçti mi" kontrolü güncel `midpoint`'e göre yapılır.

### 3. Fırlatma: bırakışta hız aktarımı

- Yeni tunable: `PlayerStatsSO.MovingNodeLaunchMultiplier`, varsayılan **1**.
- Bırakışta aktarılan hız = (bırakan kolların node hızı) × çarpan. Bu hız, duvardan ayrılırken kullanılan mekanizmayla aynı yoldan verilir: `AirEntry.CarriedVelocity`. Y normal hıza eklenir, X referans çerçevesi olarak taşınır ve hava sürtünmesinden etkilenmez (`docs/systems/platforms.md` "Ayrılırken hız aktarımı").
- Hangi bırakış nereden geçer:
  - Tek kanca bırakışı: `AirEntry.SwingRelease(carriedVelocity)` (yeni opsiyonel parametre)
  - Çift kancada iki tetiği birden bırakma: `AirEntry.Fall(carriedVelocity)`, iki anchor hızının ortalaması
  - Slingshot çıkışı: `AirEntry.SlingshotLaunch(lockout, carriedVelocity)`, iki anchor hızının ortalaması
- Çarpışmayla kopma (`BreakGrapple`, duvara çarpma, yere inme) hız aktarmaz.
- Çift kancadan tek kancaya düşüş (bir tetik bırakılır) bir bırakış değildir: aktarım olmaz, kalan kol taşımaya devam eder.
- Sabit node'da hız 0'dır, yani davranış birebir aynı kalır.

### 4. Node'u hareket ettirmek

- `Node-Moving` prefab'ı: bir `PlatformController` (`LinearPingPongStrategy`) objesinin child'ı olarak mevcut node. Node collider'ı node katmanında, platform kendi collider'ı olmadan.
- `PlatformController`, `Rigidbody2D.MovePosition` kullanıyor. Node'un `transform.position`'ı fizik adımından sonra güncellenir, yani oyuncu node'u bir adım (20 ms) geriden izler. Delta ve hız yine doğrudur. Görsel olarak ip bir adım geride kalırsa, inşa eden kişi `GrappleVisualManager`'ın node konumunu nereden okuduğuna bakmalı.
- `PlatformController` ölünce başa dönüyor (`IResettable`). Respawn'da anchor'lar zaten siliniyor (`OnStartRespawn`).

### 5. Testler (onay gerekir, CLAUDE.md kural 3)

Önerilen PlayMode kural testleri. `PlayerRuleTestBase` hareketli node spawn edebilecek hale getirilir (child + `PlatformController` ya da testte elle hareket ettirilen node):

1. Tek kancada node yatay hareket edince anchor node konumunu izler ve oyuncu–node mesafesi ip boyunda kalır.
2. Hareket eden node'dan bırakınca oyuncunun yatay hızı ≈ kendi teğet hızı + node hızı.
3. Sabit node'da bırakış hızı değişmez (mevcut `SwingRulesTests` bunu zaten kapsıyor).

Yeni bir EditMode testi yok. Saf mantık yalnızca toplama işlemi.

### 6. Dokümanlar

- `docs/systems/grapple.md`: "Hedef seçimi" son maddesi, kenar durum #8 ve tunable tablosu güncellenir. "Son kod doğrulaması" tarihi.
- `docs/systems/platforms.md`: "Ayrılırken hız aktarımı" maddesine kanca bırakışı eklenir.
- `docs/bozulmamali-davranislar.md`: yeni kurallar.

## Ek: atalet (17 Kasım 2026, playtest sonrası)

Gamepad turunda "node hareketi swing'e hiç etki etmiyor" geri bildirimi geldi. Sebep §2'deki saf öteleme: node çerçevesinde sarkaç, node'un ivmesini hiç görmüyordu. Üç açıdan (fizik, oyun referansları, kod) fikir toplandı; kullanıcı "atalet + yumuşak hareket" paketini seçti:

- Yumuşatılmış node hızının değişimi × `MovingNodeInertia` (0.6) node'a göre hızdan çıkarılır (tek + çift kanca), değişim `MovingNodeMaxPivotAccel` (60) ile sınırlı.
- Kanca durumları birbirine dünya hızı devreder; yakalama anında dünya hızı korunur (`AttachAnchor` node hızını anında alır).
- `SinePingPongStrategy`: ivmesi sürekli gidip gelme, `Node-Moving` prefab'ının varsayılanı.
- M2/M3/M4 ataletle uyumlu yeniden tanımlandı; M6, M6b, M7 eklendi.
- Ertelenenler: titreşim/ip gerginliği/kamera öne bakışı (yalnızca sunum), hop sırasında taşıma, bırakışta hizalama bonusu.

## Kısıtlar

- Unity 6000.3.10f1. Sahne işleri yalnızca `unity command eval` ile yapılır, `.unity` dosyası elle düzenlenmez.
- Yeni tunable `PlayerStatsSO`'ya girer. His testi gamepad ile, kullanıcı tarafından yapılır (kural 4).
- EditMode ve PlayMode testleri yeşil kalmalı.

## İnşa edene kalanlar

- `RefreshMovingAnchors`'ın tam yeri ve isimlendirme.
- Çift kancada taşıma kaymasının `Move`'a nasıl verileceği (ayrı delta mı, adım hızına ekleme mi).
- Test sahnesinde hareketli node'un nasıl kurulacağı.
- Ayrı oyun içi deneme düzeneği: kullanıcıya gamepad turu için `PhysicsTest` gibi bir scratch sahne ya da mevcut bir odanın kopyası önerilir. Nereye konacağı kullanıcının kararı.
