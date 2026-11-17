# Hareketli node'lar: uygulama planı

> **Uygulayan için:** Bu plan task task uygulanır (`superpowers:executing-plans` ya da `superpowers:subagent-driven-development`). Adımlar `- [ ]` ile işaretlenir.

**Hedef:** Kanca anchor'ı hareket eden node'u izlesin. Oyuncu tutunduğu node'la taşınsın, bırakınca node'un hızını alsın.

**Mimari:** `LeftAnchor`/`RightAnchor` (`Vector2?`) olduğu gibi kalır. `PlayerController` her kol için node'un `Transform`'unu tutar ve her fizik adımında, durumdan önce, anchor'ı yeniler. Node'un o adımdaki kayması (delta) ve hızı da kaydedilir. Kanca durumları kendi hızlarını node'a göre hesaplar, delta ayrı bir konum kayması olarak eklenir. Bırakışta node hızı `AirEntry.CarriedVelocity` ile aktarılır.

**Teknoloji:** Unity 6000.3.10f1, kinematik `Rigidbody2D`, NUnit PlayMode testleri (`PlayerRuleTestBase`).

**Tasarım:** [`hareketli-node-tasarimi.md`](hareketli-node-tasarimi.md). Uygulayan iki dokümanı da okur.

## Genel kısıtlar

- Branch `feature/moving-nodes`. Her task bir commit. Push yok (CLAUDE.md kural 5).
- Kod `Assets/Scripts/New-Scripts/` altında, `New_Scripts.*` namespace'inde. Yorumlar dosyanın diliyle (bu dosyalarda Türkçe).
- Sahne ya da prefab işleri yalnızca `unity command eval` ile yapılır. `.unity` dosyası elle düzenlenmez.
- Her task sonunda derleme temiz olmalı (`unity command recompile` → `recompile_status`; hatalar `%LOCALAPPDATA%/Unity/Editor/Editor.log` içinde `error CS`). EditMode yeşil: `unity command run_tests --mode EditMode`. PlayMode yeşil: `unity command run_tests --mode playmode --async_tests true`, `unity command test_status`; sonuçlar `%USERPROFILE%/AppData/LocalLow/Astrobotiq/RockTheGame/TestResults.xml`. Çıktılar rapora yazılır.
- Sabit node'da davranış birebir aynı kalır: mevcut 57 PlayMode testi ve 441 EditMode testi değişmeden geçmeli.
- Yeni tunable: `PlayerStatsSO.MovingNodeLaunchMultiplier = 1f`. `PlayerStats.asset`'teki değer de 1.
- Yeni kural testleri (M1–M5) **gameplay-critical**: geliştirici bu planı onaylayarak isimlerini ve assert'lerini onaylamış olur. Assert eşiğini gevşetmek gerekirse durulur ve sorulur.

## Dikkat edilecekler (testlerin kapsamadığı durumlar)

1. **Node oda uyurken pasifleşir** (`Room.Sleep`) ya da yok edilir: anchor son konumda sabit kalmalı, kanca kopmamalı, NullReference olmamalı. Test Task 1'de: `M1b`.
2. **Tutunuluyken respawn:** `OnStartRespawn` anchor'larla birlikte node referanslarını da temizlemeli. Ölümden sonra eski node izlenmemeli. Test Task 1'de: `M1c`.
3. **Node oyuncuyu duvara taşır:** taşıma kayması çarpışma taramasına dahil olduğu için S1/S2 kuralı geçerli (dik yüzeyde kopar). Test Task 2'de: `M2b`.
4. **Yerden kanca (hop) hareketli node'a:** hop boyunca anchor güncellenir, ip boyu hop sonunda o anki mesafeye sabitlenir. Ayrı test yok, gamepad turunda bakılacak (Task 4).
5. **Çift kancada biri sabit biri hareketli node:** ortalama delta ile yarı hızda taşınır, yaylar farkı kapatır, NaN ya da patlama olmaz. Test Task 3'te: `M4b`.

---

### Task 1: Anchor node'u izler

**Dosyalar:**
- Değişecek: `Player/NodeDetector.cs:35` (`TryFindBestNode`)
- Değişecek: `Player/PlayerController.cs:99-100` (anchor property'leri), `:161-174` (`FixedUpdate`), `:185-188` (`TryCastGrapple`), `:274-278` (`OnStartRespawn`)
- Değişecek: kanca atılan yerler: `States/NormalState.cs:177-179` ve `:460-469`, `States/SwingingState.cs:270-273`, `States/WallSlidingState.cs:166-178`
- Test: `Assets/Tests/PlayMode/MovingNodeRulesTests.cs` (yeni), `Assets/Tests/PlayMode/PlayerRuleTestBase.cs`

**Arayüzler (sonraki task'lar bunları kullanır):**
- `NodeDetector`: `bool TryFindBestNode(Vector2 aimDirection, Vector2 origin, out Vector2 bestNodePosition, out Transform bestNode)`. Eski 3 parametreli imza buna delege eder (`TargetVisualizer` onu kullanıyor).
- `PlayerController`:
  - `bool TryCastGrapple(Vector2 direction, out Vector2 hitPoint, out Transform node)`. Eski imza kalır ve delege eder (`SwingTutorial` kullanıyor).
  - `Transform LeftAnchorNode { get; private set; }`, `Transform RightAnchorNode { get; private set; }`
  - `void AttachAnchor(ActiveArm arm, Vector2 point, Transform node)`
  - `Vector2 AnchorDelta(ActiveArm arm)`: bu adımdaki kayma. Node yoksa `Vector2.zero`.
  - `Vector2 AnchorVelocity(ActiveArm arm)`: `AnchorDelta / Time.fixedDeltaTime`.
  - `LeftAnchor`/`RightAnchor` setter'ı aynı kolun node'unu `null` yapar. Ham nokta atamak sabit anchor demektir.
- `PlayerRuleTestBase`: `protected IEnumerator MoveEachFixedStep(GameObject node, Vector2 step, int count)`. Her `WaitForFixedUpdate`'te `node.transform.position += step` yapar ve ardından `Physics2D.SyncTransforms()` çağırır.

- [ ] **Adım 1: Failing testleri yaz.** `MovingNodeRulesTests : PlayerRuleTestBase`. Ortak kurulum: oyuncu `Origin`'e ışınlanır, `EnterAir(Vector2.zero, AirEntry.Fall())`, node `Origin + (0, 3)`'te, `GrabWithRight(nodePos)`.
  - `M1_AnchorFollowsMovingNode`: `MoveEachFixedStep(node, (0.2, 0), 10)` sonrası `Vector2.Distance(Player.RightAnchor.Value, node.transform.position) < 0.01f` ve `Player.RightAnchorNode == node.transform`.
  - `M1b_DeactivatedNode_KeepsLastAnchor`: 5 adım hareket, `node.SetActive(false)`, 5 adım daha bekle. Anchor son aktif konumda (`< 0.01f`), durum hâlâ `SwingingState`, hata logu yok.
  - `M1c_Respawn_ClearsAnchorNode`: tutunuluyken `Player.OnStartRespawn()` → `RightAnchorNode == null`, `RightAnchor == null`.
  - `M1d_RawAnchorAssignment_IsStatic`: `StartSwingFrom(p)` → `RightAnchorNode == null`. Node'u hareket ettir, `RightAnchor == p`.
- [ ] **Adım 2: Çalıştır, M1/M1b'nin kırıldığını gör** (`unity command run_tests --mode playmode --async_tests true --filter RockTheGame.Tests.PlayMode.MovingNodeRulesTests`). Beklenen: M1 FAIL (anchor eski noktada kalır), `RightAnchorNode` henüz yoksa derleme hatası.
- [ ] **Adım 3: Arayüzleri uygula.** `RefreshMovingAnchors()`, `FixedUpdate`'te `_isPaused` kontrolünden sonra, `CurrentState?.FixedUpdateState()`'ten önce çağrılır. Node `null` ya da `!activeInHierarchy` ise o kolun deltası sıfır olur, anchor değişmez. Anchor backing field'a yazılır, setter node'u silmesin diye. `OnStartRespawn` mevcut `LeftAnchor = null` atamasıyla node'u da temizler (setter sayesinde). Kanca atılan 5 yer `TryCastGrapple(..., out var hitPoint, out var node)` + `AttachAnchor(arm, hitPoint, node)` kullanır.
- [ ] **Adım 4: Testler.** `MovingNodeRulesTests` yeşil. PlayMode'un tamamı (57 + 4) ve EditMode yeşil.
- [ ] **Adım 5: Commit.** `feat(grapple): anchor hareketli node'u izler`

### Task 2: Tek kanca taşır ve fırlatır

**Dosyalar:**
- Değişecek: `Player/States/SwingingState.cs` (`ApplyArcadePendulum`, `ApplyHopPhysics`, `CheckInputTransitions`), `Player/States/AirEntry.cs:73` (`SwingRelease`), `Player/PlayerStatsSO.cs` (`[Header("Slingshot")]` öncesine `[Header("Moving Node")]`), `PlayerStats.asset` (eval ile ya da varsayılan alanla)
- Test: `MovingNodeRulesTests.cs`

**Arayüzler:**
- Kullanılan: Task 1'deki `AnchorDelta`, `AnchorVelocity`.
- Üretilen: `public static AirEntry SwingRelease(Vector2 carriedVelocity = default)`. `PlayerStatsSO.MovingNodeLaunchMultiplier` (float, 1).

- [ ] **Adım 1: Failing testleri yaz.**
  - `M2_SwingingPlayerIsCarried`: ortak kurulum. Node `(0.2, 0)` adımla 25 adım (10 birim/sn, 5 birim) ilerler. Sonunda `Mathf.Abs(Player.PlayerRigidbody.position.x - node.transform.position.x) < 0.3f`.
  - `M2b_CarriedIntoWall_BreaksTheHook`: duvar `SpawnWall(Origin.x + 2f)`, node aynı hızla duvara doğru. `WaitWhileState<SwingingState>()` sonrası `AssertInAir()` ve `RightAnchor == null` (S2).
  - `M3_ReleaseFromMovingNode_InheritsNodeVelocity`: ortak kurulum, 10 adım hareket, `Input.IsRightTriggerHeld = false`, `WaitForAir()`, 1 fizik adımı. `Player.Velocity.x > 8f` (node 10, çarpan 1, hava sürtünmesi taşınan X'e uygulanmaz).
- [ ] **Adım 2: Çalıştır.** M2 ve M3 FAIL olmalı. M2b durumu not edilir (taşıma olmadan da geçebilir; asıl amaç taşımadan sonra kuralın korunması).
- [ ] **Adım 3: Uygula.**
  - `ApplyArcadePendulum`: başta `_anchorPoint` context'teki anchor'dan tekrar okunur. `playerPos`'a `AnchorDelta(_swingingArm)` eklenir. `nextPosition` ve `movementDelta` bu kaymış konuma göre hesaplanır. `CheckSweepCollision` gerçek konumdan `nextPosition`'a tarar. `_currentVelocity` node'a göre kalır.
  - Hop aşamasında anchor okunur, hop hızına dokunulmaz.
  - Bırakışta `AirEntry.SwingRelease(AnchorVelocity(_swingingArm) * _stats.MovingNodeLaunchMultiplier)`. Hız `ReleaseAnchor()`'dan önce okunur.
- [ ] **Adım 4: Testler.** `MovingNodeRulesTests`, `SwingRulesTests`, `AirEntryRulesTests` ve tam PlayMode + EditMode yeşil.
- [ ] **Adım 5: Commit.** `feat(grapple): tek kanca hareketli node'la tasinir, birakista hizini alir`

### Task 3: Çift kanca ve slingshot taşır ve fırlatır

**Dosyalar:**
- Değişecek: `Player/States/DualSwingingState.cs`, `Player/States/SlingshotState.cs`, `Player/States/AirEntry.cs` (`Fall`, `SlingshotLaunch`)
- Test: `MovingNodeRulesTests.cs`

**Arayüzler:**
- Kullanılan: `AnchorDelta`, `AnchorVelocity`, `MovingNodeLaunchMultiplier`.
- Üretilen: `public static AirEntry Fall(Vector2 carriedVelocity = default)`, `public static AirEntry SlingshotLaunch(float grappleLockout, Vector2 carriedVelocity = default)`.

- [ ] **Adım 1: Failing testleri yaz.** İki kolla tutunma `SlingshotRulesTests`'teki gibi (`GrabWithLeft` + sağ tetik/stick).
  - `M4_DualSwing_CarriedAndLaunched`: node'lar `Origin + (-1.5, 3)` ve `(1.5, 3)`, `CanSlingshot` harcanmış (`UseSlingshot()`), `DualSwingingState`. İki node birlikte 25 adım `(0.2, 0)`. Oyuncu x'i node'ların ortalama x'ine `< 0.5f` uzaklıkta. Sonra iki tetik bırakılır, 1 adım sonra `Velocity.x > 8f`.
  - `M4b_DualSwing_OneMovingOneStatic_StaysFinite`: yalnızca sağ node 25 adım hareket eder. `float.IsNaN` yok, `Velocity.magnitude < 100f`, durum hâlâ `DualSwingingState` ya da yere inmiş.
  - `M5_SlingshotFromMovingNode_InheritsNodeVelocity`: tek node `Origin + (0, 3)`, iki kol aynı node'a, `SlingshotState`. Node her adım `(0.2, 0)`. Çıkışta (`WaitForAir`) `Player.Velocity.x > 8f` (fırlatma dikey, X'i node verir).
- [ ] **Adım 2: Çalıştır.** M4 ve M5 FAIL.
- [ ] **Adım 3: Uygula.**
  - Dual: taşıma = `(AnchorDelta(Left) + AnchorDelta(Right)) / 2`. Yay hesabı (`currentVelocity`) node'a göre kalır. Kayma `KinematicPhysicsHandler.Move` üzerinden çarpışmayla birlikte uygulanır: `context.Velocity = currentVelocity + taşıma / fixedDeltaTime`, durumun kendi `currentVelocity` alanı karışmadan. Dairesel çarpışma taraması da toplam harekete göre yapılır. İki tetik bırakılınca `AirEntry.Fall(ortalama AnchorVelocity × çarpan)`. Tek tetik bırakılınca aktarım yok.
  - Slingshot: `midpoint` her `FixedUpdateState` başında güncel anchor'lardan hesaplanır. Gerilme ve fırlatma hızına taşıma eklenir (`Move` yoluyla, aynı yöntem). `cachedLaunchDirection` girişte sabit kalır. İki çıkış noktasında da `AirEntry.SlingshotLaunch(lockout, ortalama AnchorVelocity × çarpan)`. Çıkıştaki `context.Velocity` taşıma içeriyorsa `NormalState`'e taşımasız fırlatma hızı verilir, taşıma `CarriedVelocity` ile gider (çift sayılmasın).
- [ ] **Adım 4: Testler.** `MovingNodeRulesTests`, `SlingshotRulesTests` ve tam PlayMode + EditMode yeşil.
- [ ] **Adım 5: Commit.** `feat(grapple): cift kanca ve slingshot hareketli node'la tasinir`

### Task 4: Hareketli node prefab'ı ve dokümanlar

**Dosyalar:**
- Oluşturulacak: `Assets/Prefab/New/Node/Node-Moving.prefab`. `unity command eval` ile: kök `PlatformController` + kinematik `Rigidbody2D` + `LinearPingPongStrategy`, collider yok. Child'ı mevcut node prefab'ının görsel ve collider'ı (node katmanı, oda 5'teki node'larla aynı kurulum).
- Değişecek: `docs/systems/grapple.md` (Hedef seçimi son maddesi, Durumlar altında taşıma/fırlatma, tunable tablosu `MovingNodeLaunchMultiplier`, kenar durum #8 kaldırılır, "Son kod doğrulaması" 2026-11-xx), `docs/systems/platforms.md` ("Ayrılırken hız aktarımı"na kanca bırakışı), `docs/bozulmamali-davranislar.md` (yeni "Hareketli node" bölümü M1–M5), `docs/precision-platformer-plani.md:167` ve `:207` (durum güncellenir)

- [ ] **Adım 1:** Prefab'ı eval ile oluştur. Edit Mode'da bir test sahnesine (geliştiricinin seçeceği yer; varsayılan olarak takipsiz `PhysicsTest.unity`, kaydetmeden önce sorulur) bir örnek koy ve Play Mode'da `capture_game_view` ile node'un hareket ettiğini göster.
- [ ] **Adım 2:** İpin node'u görsel olarak bir adım geriden izleyip izlemediğine bak (tasarım §4). Gecikme görünüyorsa not et, düzeltme kapsam dışı: geliştiriciye sorulur.
- [ ] **Adım 3:** Dokümanları güncelle.
- [ ] **Adım 4:** Derleme, EditMode ve PlayMode yeşil.
- [ ] **Adım 5: Commit.** `feat(grapple): Node-Moving prefab'i, dokumanlar`
- [ ] **Adım 6: Geliştiriciye gamepad turu.** Taşıma, üç bırakış, yerden hop, duvara taşınma. Çarpanı (1) ve hava sürtünmesinin taşınan X'e işlememesini hissetmesi istenir (CLAUDE.md kural 4).
