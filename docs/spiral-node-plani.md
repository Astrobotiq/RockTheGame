# Spiral node'lar: uygulama planı

> **Uygulayan için:** Bu plan task task uygulanır (`superpowers:executing-plans` ya da `superpowers:subagent-driven-development`). Adımlar `- [ ]` ile işaretlenir.

**Hedef:** Tek kancayla tutunulan spiral node oyuncuyu kendi yönünde döndürsün; ayrı bir ters dönen spiral türü 360°'de bir yön değiştirsin.

**Mimari:** Node'da bir `SpiralNode` bileşeni. `SwingingState` tutununca onu kolun node'undan okur ve sarkaç adımında normal pompalamanın yerine bir motor çalıştırır (teğet hızı, node'a göre, hedef hıza sınırlı ivmeyle yaklaştırır; yerçekimi aynen işler). Hesaplar saf `SpiralDriveMath` ve `SpiralReverseCounter`'da, EditMode'da test edilir. Bırakış, çarpışma, çift kanca/slingshot geçişi ve hareketli node mantığı değişmez.

**Teknoloji:** Unity 6000.3.10f1, kinematik `Rigidbody2D`, URP 2D shader, NUnit EditMode + PlayMode (`PlayerRuleTestBase`).

**Tasarım:** [`spiral-node-tasarimi.md`](spiral-node-tasarimi.md). Uygulayan iki dokümanı da okur; buradaki adlar ve değerler bağlayıcıdır, gerekçe tasarımdadır.

## Genel kısıtlar

- Branch `feature/spiral-nodes`, `feature/moving-nodes`'un ucundan (spiral `LeftAnchorNode`/`RightAnchorNode`'u kullanır). Her task bir commit. Push yok (CLAUDE.md kural 5). Çalışma ağacındaki ilgisiz değişikliklere dokunulmaz (kural 6).
- Kod `Assets/Scripts/New-Scripts/` altında. Yeni dosyalar `Player/Nodes/Spiral/`, namespace `New_Scripts.Player.Nodes.Spiral`. Yorumlar Türkçe.
- Sahne/prefab/asset işleri yalnızca `unity command eval` ile; `.unity` elle düzenlenmez.
- Her task sonunda derleme temiz (`unity command recompile` → `recompile_status`; hatalar `%LOCALAPPDATA%/Unity/Editor/Editor.log` içinde `error CS`), EditMode yeşil (`unity command run_tests --mode EditMode`), oyuncu kodu değiştiyse PlayMode yeşil (`unity command run_tests --mode playmode --async_tests true`, `unity command test_status`, sonuçlar `%USERPROFILE%/AppData/LocalLow/Astrobotiq/RockTheGame/TestResults.xml`). Çıktılar rapora yazılır.
- **Normal node'da davranış birebir aynı:** mevcut PlayMode (67) ve EditMode (441) testleri değişmeden geçer. Spiral kodu yalnızca `_spiral != null` iken çalışır.
- Yeni tunable'lar `PlayerStatsSO`, `[Header("Spiral Node")]`: `SpiralDriveAccel = 60f`, `SpiralOpposeAccel = 45f`, `SpiralInputInfluence = 0.4f`, `SpiralOverspeedDecay = 0f`, `SpiralBlockedRatio = 0.25f`, `SpiralBlockedSteps = 3`. `PlayerStats.asset`'e aynı değerler yazılır.
- `SpiralNode` varsayılanları: `targetSpeed = 18f`, `reverseEveryDegrees = 0f` (ters dönen prefab'da `360f`), `idleReferenceRadius = 3f`.
- İşaret kuralı: `+1` = saat yönünün tersi (CCW), `−1` = saat yönü (CW). `SwingingState`'te `tangent = (d.y, −d.x)` (d oyuncudan anchor'a) yönünde pozitif hız CCW'dir; `Vector2.SignedAngle(öncekiYön, şimdikiYön)` (anchor'dan oyuncuya) CCW'de pozitiftir.
- SP1–SP7 kullanıcı tarafından onaylandı (2026-11-17). Assert eşiğini gevşetmek gerekirse durulur ve sorulur. His değerleri (yukarıdakiler) kullanıcının kararı; değiştirilmez, gamepad testinde ayarlanır (kural 4).

## Dikkat edilecekler (testlerin kapsamadığı ya da az kapsadığı durumlar)

1. **Yerden kanca ile ters yönde spirale tutunma:** hop aşaması aynen kalır, motor hop bitince başlar, ters yön kuralı (SP2) geçerli. Test Task 2'de: `SP2b`.
2. **Dönen node bileşeni de olan spiral:** tracker bağlanmaz, 360° tamamlansa da efekt tetiklenmez, `OnValidate` uyarı verir. Test Task 3'te: `SP_NoFullRotationOnSpiral`.
3. **Spiraldeyken ölüm/respawn:** respawn anchor'ları siler → `SwingingState.ExitState` → `SpiralNode.Disconnect`; ters dönen spiral ilk yönüne döner, döngü sesi ve titreşim durur. Test Task 4'te: `SP7c`; ses/titreşim durması Task 5'te elle kontrol.
4. **Hit-stop sırasında spiral:** oyuncu duraklar (motor zaten çalışmaz); spiral görseli de durmalı. Task 6'da elle kontrol (EditMode testi yok, görsel).
5. **Oda uyurken spirale tutunulu:** node pasifleşir, anchor son konumda kalır (M1), motor çalışmaya devam eder, NullReference olmaz (`SpiralNode` pasifken çağrılan metotlar güvenli). Test Task 3'te: `SP_NodeDeactivated_NoException`.

---

### Task 1: Saf motor matematiği, tunable'lar, kurallar

**Dosyalar:**
- Oluştur: `Player/Nodes/Spiral/SpiralDriveMath.cs`, `Player/Nodes/Spiral/SpiralReverseCounter.cs`
- Değiştir: `Player/PlayerStatsSO.cs` (yeni header), `Player/SO/PlayerStats.asset` (eval ile değerler)
- Değiştir: `docs/bozulmamali-davranislar.md` (yeni "Spiral node" bölümü, SP1–SP7 tasarım §12'deki metinle, tarih 17 Kasım 2026; "Testler" paragrafına `SpiralNodeRulesTests`)
- Test: `Assets/Tests/EditMode/SpiralDriveMathTests.cs`, `Assets/Tests/EditMode/SpiralReverseCounterTests.cs`, `Assets/Tests/EditMode/PlayerStatsSanityTests.cs`

**Arayüzler (sonraki task'lar kullanır):**
- `public static class SpiralDriveMath`
  - `float EffectiveTarget(int spinSign, float baseSpeed, float input, float influence, float speedMultiplier)` → `spinSign × baseSpeed × (1 + influence × Clamp(input, −1, 1)) × speedMultiplier`. İşaretli.
  - `float StepTangentialSpeed(float speed, float target, float driveAccel, float opposeAccel, float overspeedDecay, float dt)`: `speed` ve `target` işaretli. Zıt işaret → `MoveTowards(speed, target, opposeAccel·dt)`. Aynı yön, `|speed| < |target|` → `MoveTowards(..., driveAccel·dt)`. Aynı yön, üstünde → `overspeedDecay > 0` ise `MoveTowards(..., overspeedDecay·dt)`, değilse `speed` aynen. `speed == 0` aynı yön sayılır.
  - `bool GuaranteesLoop(float driveAccel, float opposeAccel, float gravity)` → ikisi de `> |gravity|`.
  - `bool IsBlocked(float commandedSpeed, float actualSpeed, float ratio)` → `|commanded| > 0.5` ve `actual × Sign(commanded) < ratio × |commanded|`.
- `public class SpiralReverseCounter`
  - `SpiralReverseCounter(float degrees)`
  - `bool Tick(Vector2 playerPos, Vector2 anchorPos, int spinSign)`: ilk çağrı yalnızca yönü kaydeder. Sonra `SignedAngle × spinSign` biriktirir, toplam `0`'ın altına inmez; `≥ degrees` olunca sıfırlar ve `true` döner.
  - `float Progress` (0..1), `void Reset()`.

- [ ] **Adım 1: `docs/bozulmamali-davranislar.md`'ye SP1–SP7'yi ekle** (tasarım §12 tablosundan birebir).
- [ ] **Adım 2: Başarısız testleri yaz**

`SpiralDriveMathTests`:
- `EffectiveTarget_InputNeverFlipsSign`: `spinSign` ±1, `input` ∈ {−1, 0, 1}, `influence` 0.4 → sonuç işareti `spinSign` ile aynı; `input = 1` → `18 × 1.4`, `input = −1` → `18 × 0.6`.
- `Step_SameDirectionBelow_AcceleratesAtDrive`: `(0, 18, 60, 45, 0, 0.02)` → `1.2`.
- `Step_SameDirectionAbove_KeepsSpeed`: `(25, 18, 60, 45, 0, 0.02)` → `25`; decay 10 ile → `24.8`.
- `Step_Opposite_UsesOpposeAccel`: `(−25, 18, 60, 45, 0, 0.02)` → `−24.1`.
- `Step_Opposite_ReversesWithinExpectedTime`: `−25`'ten başlayıp adım adım; işaretin pozitife dönmesi `25/45 s ± 0.02` içinde.
- `GuaranteesLoop`: `(60, 45, −32)` → true; `(60, 30, −32)` → false.
- `Simulated_FullLoop_AnyRopeLength` (`[TestCase(2f)] [TestCase(5f)] [TestCase(10f)]`): anchor altında durgun başlayan nokta, her adımda yerçekimi `(0, −32)` teğete izdüşülür + `StepTangentialSpeed(target 18)` + konum ip çemberine oturtulur; `FullRotationTracker` 6 sn içinde 360°'yi tamamlar ve biriken açı pozitiftir (CCW).
- `IsBlocked`: `(18, 2, 0.25)` → true; `(18, 10, 0.25)` → false; `(0.2, 0, 0.25)` → false; `(−18, −2, 0.25)` → true.

`SpiralReverseCounterTests` (`FullRotationTrackerTests.PositionAt` deseni):
- `Tick_CompletesAfterDegreesInSpinDirection`: CCW, 5° adımlarla; 72. adımda (360°) `true`, öncesinde `false`.
- `Tick_IgnoresMotionAgainstSpin`: CCW sayaç, önce 90° CW (Progress 0 kalır), sonra 360° CCW → tam 360° CCW'den sonra `true`.
- `Reset_ClearsProgress`.

`PlayerStatsSanityTests`: `SpiralMotor_GuaranteesLoop` → asset değerleriyle `GuaranteesLoop(SpiralDriveAccel, SpiralOpposeAccel, Gravity)`.

- [ ] **Adım 3: Testlerin derleme/başarısızlıkla düştüğünü gör.**
- [ ] **Adım 4: `SpiralDriveMath`, `SpiralReverseCounter` ve `PlayerStatsSO` alanlarını yaz; asset değerlerini eval ile yaz.**
- [ ] **Adım 5: EditMode yeşil.**
- [ ] **Adım 6: Commit** (`feat(spiral): motor matematigi, tunable'lar ve SP kurallari`).

---

### Task 2: `SpiralNode` ve `SwingingState` motoru (SP1, SP2, SP3, SP5)

**Dosyalar:**
- Oluştur: `Player/Nodes/Spiral/SpiralNode.cs`
- Değiştir: `Player/States/SwingingState.cs` (`EnterState`, `FixedUpdateState`, `ApplyArcadePendulum`, `ExitState`)
- Değiştir: `Assets/Tests/PlayMode/PlayerRuleTestBase.cs` (yardımcılar), `Assets/Tests/PlayMode/MovingNodeRulesTests.cs` (private `SetField` silinir, tabandakini kullanır)
- Test: `Assets/Tests/PlayMode/SpiralNodeRulesTests.cs` (yeni)

**Arayüzler:**
- Tüketir: Task 1'deki `SpiralDriveMath`, `PlayerStatsSO.Spiral*`.
- Üretir:
  - `public enum SpinDirection { Clockwise, CounterClockwise }`
  - `SpiralNode : MonoBehaviour` (`[RequireComponent(typeof(Collider2D))]`), serileştirilen alanlar `direction`, `targetSpeed`, `reverseEveryDegrees`, `idleReferenceRadius`.
    - `int SpinSign { get; }` (o anki yön; başlangıçta `direction`'dan)
    - `float TargetSpeed`, `float ReverseEveryDegrees`, `bool IsReversing => ReverseEveryDegrees > 0`, `bool IsConnected { get; }`
    - `void Connect(PlayerController player)`, `void Disconnect(bool stillHeld)` (yönü başlangıca döndürür, `IsConnected = false`), `void Flip()`, `void ReportAngularSpeed(float degreesPerSecond)` (görsel için; bu task'ta yalnızca saklar)
    - `OnValidate`: aynı objede ya da parent'ta `FullRotationNode` varsa `Debug.LogWarning`.
    - `OnDrawGizmos`: yön oku + R ∈ {2, 5, 10} için tur süresi etiketi (`2πR / targetSpeed`).
  - `PlayerRuleTestBase`:
    - `protected static void SetField(object target, string name, object value)` (MovingNodeRulesTests'ten taşınır)
    - `protected IEnumerator GrabExistingWithRight(GameObject node)` ve `...WithLeft`: node spawn etmez; stick + tetik, `SwingingState` bekler.
    - `protected IEnumerator MeasureRotation(Vector2 anchor, int steps, System.Action<float> result)`: her `WaitForFixedUpdate`'te `SignedAngle` biriktirir, toplamı döndürür.

`SwingingState` değişiklikleri:
- `EnterState`: `TryBindSpiralNode()` → kolun `LeftAnchorNode`/`RightAnchorNode`'undan `GetComponentInParent<SpiralNode>()`; bulunursa `Connect(_context)` ve `TryBindFullRotationNode()` çağrılmaz.
- `ApplyArcadePendulum`: `_spiral != null` ve hop bitmişse, teğete izdüşümden sonra pompalama + `MaxSwingSpeed` bloğunun yerine: `input = Dot(stick, spinSign × tangent)` (stick: tutunan kolun stick'i, iki eksen), `target = EffectiveTarget(SpinSign, TargetSpeed, input, SpiralInputInfluence, ActiveSpeedMultiplier)`, `speedAlongTangent = StepTangentialSpeed(...)`. `_spiral.ReportAngularSpeed(speed / ropeLength × Rad2Deg)`.
- `ExitState`: `_spiral?.Disconnect(stillHeld: kolun anchor node'u hâlâ bu spiral)`.

- [ ] **Adım 1: Başarısız testleri yaz** (`SpiralNodeRulesTests : PlayerRuleTestBase`; `SpawnSpiral(Vector2 pos, SpinDirection dir, float reverseEvery = 0f)` = `SpawnNode` + `AddComponent<SpiralNode>` + `SetField`):
  - `SP1_FromRest_CompletesLoopInSpinDirection` (`[TestCase(2f, SpinDirection.CounterClockwise)] [TestCase(6f, SpinDirection.CounterClockwise)] [TestCase(6f, SpinDirection.Clockwise)]`): oyuncu `EnterAir(Vector2.zero, AirEntry.Fall())` ile node'un `rope` kadar altında; `GrabExistingWithRight`; `MeasureRotation` 300 adım (6 sn) → `|açı| ≥ 360` ve işareti yönle aynı.
  - `SP2_GrabAgainstSpin_SlowsThenReverses`: CCW spiral, oyuncu altında, `EnterAir((−12, 0))`; tutunduktan sonraki ilk adımda teğet hız negatif (anında dönmez); 2 sn içinde pozitife geçer; sonraki 0.5 sn boyunca pozitif kalır. Teğet hız: `Dot(Velocity, (d.y, −d.x))`.
  - `SP2b_GroundGrabAgainstSpin_ReversesAfterHop`: CW spiral, oyuncu yerde, node sağ üstte (hop sağa = CCW, spinin tersi); 2.5 sn içinde dönüş yönü negatif (CW).
  - `SP3_StickAgainstSpin_NeverStopsOrReverses`: CCW, durgun tutunuş, 1 sn bekle; sonra 3 sn boyunca stick spin teğetinin tersine (her adım güncellenen) → her adımda açısal değişim ≥ 0 (tolerans −0.01°).
  - `SP3b_StickWithSpin_FasterThanNeutral`: aynı kurulum iki kez (nötr / spin yönünde), 2. ve 4. saniye arası ortalama `|teğet hız|`: spin yönünde > nötr × 1.15.
  - `SP5_Release_IsNormalSwingRelease`: spiralde 1.5 sn sonra tetik bırak → `IsInNormalAir`; bırakıştan sonraki ilk adımdaki hız bırakış anındaki hızla ±0.5 aynı; yükselirken yerçekimi çarpanı 3× (`AirEntryRulesTests.H2_SwingRelease_IgnoresJumpButton`'daki ölçümü kullan).
- [ ] **Adım 2: Testlerin düştüğünü gör.**
- [ ] **Adım 3: `SpiralNode`, test yardımcıları ve `SwingingState` değişikliklerini yaz.**
- [ ] **Adım 4: EditMode + PlayMode yeşil; mevcut testler değişmeden geçiyor.**
- [ ] **Adım 5: Commit** (`feat(spiral): SwingingState motoru ve SpiralNode`).

---

### Task 3: Takılma kontrolü, çift kanca, dönen node dışlaması (SP4, SP6)

**Dosyalar:**
- Değiştir: `Player/States/SwingingState.cs`
- Test: `Assets/Tests/PlayMode/SpiralNodeRulesTests.cs`

**Arayüzler:**
- Tüketir: `SpiralDriveMath.IsBlocked`, `PlayerStatsSO.SpiralBlockedRatio`/`SpiralBlockedSteps`, Task 2'deki `SpiralNode.IsConnected`.
- Davranış: spiral varken, her pendulum adımının başında bir önceki adımın gerçek teğet hızı = `(konum − öncekiKonum − AnchorDelta) · öncekiTeğet / dt`. `IsBlocked(öncekiKomut, gerçek, ratio)` art arda `SpiralBlockedSteps` kez → mevcut `BreakGrapple()`. Hop'tan ilk pendulum adımına geçişte ölçülmez. Normal node'da çalışmaz.

- [ ] **Adım 1: Başarısız testleri yaz:**
  - `SP4_SpiralPushesIntoWall_BreaksGrapple`: CCW spiral, oyuncu 4 birim altında durgun; node'un 2 birim sağında yerden tavana dik duvar (`SpawnWall`); 1.5 sn içinde `IsInNormalAir` ve `RightAnchor == null`; sonraki 0.5 sn `SwingingState`'e dönmez (tetik bırakılmış).
  - `SP6_DualSwing_MotorOff`: spirale sağ, 4 birim soldaki normal node'a sol kolla tutun (`GrabExistingWithRight` + `GrabWithLeft`) → `DualSwingingState`; `spiral.IsConnected == false`.
  - `SP6b_SameSpiralBothArms_IsNormalSlingshot`: slingshot hakkı dolu, aynı spirale iki kol → `SlingshotState`.
  - `SP_NoFullRotationOnSpiral`: aynı node'da `SpiralNode` + `FullRotationNode` + `RotationEventTrigger` (UnityEvent'e sayaç bağlı); 6 sn tutun (≥ 360° döner) → sayaç 0. `LogAssert.Expect(LogType.Warning, ...)` gerekiyorsa ekle.
  - `SP_NodeDeactivated_NoException`: spiraldeyken node objesi `SetActive(false)`; 1 sn boyunca hata yok, oyuncu `SwingingState`'te kalır.
- [ ] **Adım 2: Düştüklerini gör.** (SP6 ve SP6b mevcut kodla geçebilir; geçerse not et.)
- [ ] **Adım 3: Takılma kontrolünü yaz.**
- [ ] **Adım 4: EditMode + PlayMode yeşil; `SwingRulesTests` S1/S2 değişmeden geçiyor.**
- [ ] **Adım 5: Commit** (`feat(spiral): duvara bastirilinca kanca kopar`).

---

### Task 4: Ters dönen spiral (SP7)

**Dosyalar:**
- Değiştir: `Player/Nodes/Spiral/SpiralNode.cs`, `Player/States/SwingingState.cs`
- Test: `Assets/Tests/PlayMode/SpiralNodeRulesTests.cs`

**Arayüzler:**
- Tüketir: `SpiralReverseCounter`, `SpiralNode.Flip()`, `Disconnect`.
- Üretir: `SpiralNode.ReverseProgress` (0..1, görsel için), `event System.Action Reversed` (ses/titreşim/görsel için).
- Davranış: `IsReversing` ise `SwingingState` motor çalıştığı her adımda `counter.Tick(pos, anchor, SpinSign)`; `true` → `spiral.Flip()` (Reversed tetiklenir). Sayaç `Connect`'te sıfırlanır; `Disconnect` yönü başlangıca döndürür. Çift kancaya geçiş `ExitState` → `Disconnect` olduğundan sıfırlar (tasarım §7).

- [ ] **Adım 1: Başarısız testleri yaz:**
  - `SP7_Reversing_FlipsAfter360AndRepeats`: CCW, `reverseEvery 360`, durgun tutunuş; `SpinSign` ilk 360° CCW birikene kadar +1, sonra −1; 3 sn içinde oyuncunun dönüşü negatif; tutunmaya devam → ikinci bir çevrilmede tekrar +1 (12 sn sınır).
  - `SP7b_Release_RestoresInitialDirection`: çevrildikten sonra tetik bırak → `SpinSign == +1`.
  - `SP7c_Respawn_RestoresInitialDirection`: çevrildikten sonra respawn (`ResetPlayerAt` ya da `PlayerHealth` ölümü, mevcut testlerdeki respawn yolu) → `SpinSign == +1`, `IsConnected == false`.
  - `SP7d_NormalSpiral_NeverFlips`: `reverseEvery 0`, 720° dön → `SpinSign` hep +1.
- [ ] **Adım 2: Düştüklerini gör.**
- [ ] **Adım 3: Yaz.**
- [ ] **Adım 4: EditMode + PlayMode yeşil.**
- [ ] **Adım 5: Commit** (`feat(spiral): ters donen spiral`).

---

### Task 5: Sprite yönü, ses ve titreşim

**Dosyalar:**
- Değiştir: `Player/PlayerController.cs` (`public int SpiralFacingSign { get; set; }`, 0 = yok), `Player/Visual/PlayerAnimationController.cs:101-109`, `Player/States/SwingingState.cs`
- Değiştir: `Player/Audio/PlayerAudioDataSO.cs` (`spiralLoopCue`, `spiralOpposeGrabCue`, `spiralReverseCue` + getter'lar), `Player/Audio/PlayerAudioController.cs` (`[SerializeField] AudioCueStopEventChannelSO sfxStopChannel`; `PlaySpiralLoop()`, `StopSpiralLoop()`, `PlaySpiralOpposeGrab()`, `PlaySpiralReverse()`)
- Değiştir: `Player/PlayerVibrationSettingsSO.cs` (`[Header("Spiral Node")] ContinuousRumbleProfile SpiralOrbit; RumbleProfile SpiralOpposeGrab; RumbleProfile SpiralReverse;`)
- Asset (eval): `PlayerVibrationSettings` asset'inde üç profilin başlangıç değerleri (`WallSlide` ve `Dash`'in yarısı yoğunlukta); oyuncu prefab'ında `sfxStopChannel`'ı `AudioManager`'ın dinlediği stop kanalına bağla. Ses klipleri kullanıcıda: cue'lar boşsa sessiz kalır, hata vermez.

**Davranış:**
- Spiraldeyken (motor çalışırken) `SpiralFacingSign = SpinSign` (CCW → sağa bak, CW → sola); `ExitState`'te 0. `PlayerAnimationController`: `SpiralFacingSign != 0` ise `flipX = SpiralFacingSign < 0` ve stick/hız dalı atlanır. `PlayerEyesController` (`:223`, göz yönü `Velocity`'den) aynen kalır; gözlerin dönüşte gezinmesi istenmiyorsa gamepad testinde bakılır.
- `Connect`'te döngü sesi + `HapticManager.Instance.StartContinuousVibration(SpiralOrbit)`; `ExitState`'te ikisi de durur. Perde sabit (`AudioManager` çalarken perde değiştiremiyor).
- Tutunma anında teğet hız spinin tersine ve `|hız| > 1` ise `PlaySpiralOpposeGrab()` + `Vibrate(SpiralOpposeGrab)`.
- `Reversed` olayında `PlaySpiralReverse()` + `Vibrate(SpiralReverse)`.

- [ ] **Adım 1: Başarısız test:** `SP_FacingLockedToSpin`: CW spiral, durgun tutunuş, 2 sn boyunca her adımda `SpriteRenderer.flipX == true`; bırakınca `SpiralFacingSign == 0`.
- [ ] **Adım 2: Düştüğünü gör.**
- [ ] **Adım 3: Yaz; asset bağlamalarını eval ile yap.**
- [ ] **Adım 4: EditMode + PlayMode yeşil.**
- [ ] **Adım 5: Elle kontrol (Play Mode):** spirale tutun → döngü sesi/titreşim başlıyor (cue/profil atanmışsa), bırakınca ve spiraldeyken ölünce duruyor. Sonucu rapora yaz.
- [ ] **Adım 6: Commit** (`feat(spiral): sprite yonu, ses ve titresim`).

---

### Task 6: Spiral shader ve prefab'lar

**Dosyalar:**
- Oluştur: `Assets/Shader/SpiralNode.shader` (`"UI/SpiralNode"`, `CircularRotationRing.shader` yapısında), `Assets/Shader/Mat_SpiralNode.mat`
- Oluştur (eval): `Assets/Prefab/New/Node/Node-Spiral.prefab`, `Assets/Prefab/New/Node/Node-SpiralReversing.prefab`
- Değiştir: `Player/Nodes/Spiral/SpiralNode.cs` (görsel sürme)

**Shader özellikleri:** `_RampTex` (node'ların ramp'i), `_Angle` (radyan, script'ten), `_Direction` (+1/−1), `_ArmCount`, `_Twist`, `_Progress` (ters dönen için kalan ilerleme halkası, 0 = gizli), `_Glow` (0..1), `_ChevronCount` (dış halkada yönü gösteren diş/ok). Pürüzsüz baskı / halftone; **piksel art değil**. Durağan bir karede yön, dış halkadaki dişlerden okunur. Ters dönen spiral ayrıca çift yönlü işaret taşır ve ilerleme halkası `CircularRotationRing`'in halkasından farklı görünür (örn. dişlerin dolması).

**`SpiralNode` görsel davranışı:** `MaterialPropertyBlock` ile `_Angle` her `Update`'te ilerler: bağlı değilken `TargetSpeed / idleReferenceRadius` rad/sn, bağlıyken `ReportAngularSpeed` değeri, işaret `SpinSign`. `HitStopEvents.HitStopStarted`/`HitStopEnded` arasında ve `PauseManager.BlocksGameplay` iken ilerlemez. `Connect`'te `_Glow` kısa bir darbe; `Disconnect(stillHeld: true)` → soluk (`_Glow` negatif ya da `_Dim`), kol node'dan ayrılınca (Update'te `Connect`'te alınan oyuncunun anchor node'larına bakar) normale döner. `_Progress = ReverseProgress`.

**Prefab'lar:** `Node-EventTrigger.prefab`'ın kopyası temel alınır (Grapple katmanı 7, `CircleCollider2D` r=0.5, `Circle` sprite), `FullRotationNode`/`RotationEventTrigger` çıkarılır, `SpiralNode` + `Mat_SpiralNode`'lu görsel child eklenir. `Node-SpiralReversing`: aynı, `reverseEveryDegrees = 360`.

- [ ] **Adım 1: Shader, materyal ve `SpiralNode` görsel kodunu yaz; prefab'ları eval ile oluştur.**
- [ ] **Adım 2: Derleme temiz, EditMode + PlayMode yeşil.**
- [ ] **Adım 3: Ekran görüntüsü:** iki prefab'ı boş bir alana koyup Edit Mode capture (CW, CCW, ters dönen); görüntüleri rapora ekle, `Assets/` altındaki geçici klasörü sil. Durağan karede yön okunuyor mu, kullanıcıya sor (görsel kullanıcının kararı).
- [ ] **Adım 4: Elle kontrol:** Play Mode'da slingshot'la hit-stop tetikle (yakındaki bir spiralle), spiral görseli duruyor mu.
- [ ] **Adım 5: Commit** (`feat(spiral): spiral shader ve prefab'lar`).

---

### Task 7: Test odası ve dokümanlar

**Dosyalar:**
- Oluştur (eval): spiral test odası, `RoomNew-MovingNodeTest.prefab`'ın kuruluş deseniyle (oda prefab'ı + `RoomManager` kaydı). Nereye konacağını (NewScene'de hangi odanın yanına) uygulamadan önce kullanıcıya sor.
- Değiştir: `docs/systems/grapple.md` (yeni "Spiral node" bölümü: kontroller, motor kuralları, ters dönen spiral, level tasarımı notu: dönüş diski NewScene'de ~10.5, varsayılanla ~15.5 birim; tunable tablosuna `Spiral*`; `MaxSwingSpeed` 25 → 28 düzeltmesi; "Kod" ve "Testler"; "Son kod doğrulaması" tarihi), `docs/GDD.md` §5.2 "Kanca ailesi" (spiral node maddesi), `docs/systems/README.md` gerekiyorsa.

**Oda içeriği (tasarım §11):** kaldırıcı (çıkıntı altında CCW spiral), fırlatıcı (boşluk + hedef platform), zıt yönlü iki spiral (zincir), ters yönde tutunmaya zorlayan düzen, ters dönen spiral (uzun tur alanı), dönüş diskine giren bir duvar (takılma → kopma).

- [ ] **Adım 1: Kullanıcıya odanın yerini sor.**
- [ ] **Adım 2: Odayı eval ile kur; Edit Mode ekran görüntüsünü rapora ekle.**
- [ ] **Adım 3: Dokümanları güncelle.**
- [ ] **Adım 4: EditMode + PlayMode yeşil.**
- [ ] **Adım 5: Commit** (`feat(level): spiral node test odasi` + docs).

---

### Task 8: Gamepad testi (kullanıcı)

Kullanıcı test odasında dener: durgun/aynı yönde/ters yönde/hızlı tutunma, stick ile hızlandırma-yavaşlatma, zamanlamalı bırakış, kaldırıcı, zincir, ters dönen spiral, duvar kopması, sprite yönü, ses/titreşim, shader'ın yönü ve hızı okunuyor mu. Değer değişiklikleri yalnızca `PlayerStats.asset` / `SpiralNode` alanlarında, eski ve yeni değer `feel-notlari.md`'ye yazılır.
