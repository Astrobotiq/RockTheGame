# Uzayan Kollar — Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Kancanın ip çizimini ve kare kolları kaldırıp yerine omuzdan ele giden, boşta yaylı ve gevşek sallanan, tutununca node'a kadar uzanan bir kol koymak. Değişiklik yalnızca görseldir.

**Architecture:** Kolun şeklini saf bir yardımcı (`ArmShapeMath`) hesaplar: eğri noktaları, kavis, boştaki el hedefi ve uzanma ilerlemesi. Her kola bir `ArmVisual` bileşeni takılır. Bu bileşen, mevcut `UiSpring`'leri kullanarak eli ve eğrinin orta noktasını yayla sürer ve sonucu mevcut `LeftRope`/`RightRope` LineRenderer'larına ve bir el sprite'ına çizer. Durumlar `ArmController.UpdateArmRotation` çağırmaya devam eder; `ArmController` bundan böyle yalnızca nişan yönünü saklar.

**Tech Stack:** Unity 6000.3.10f1, URP 2D, LineRenderer, `New_Scripts.UI.Theme.UiSpring`, NUnit EditMode.

**Spec:** `docs/superpowers/specs/2026-11-20-stretchy-arms-design.md`

## Global Constraints

- Yalnızca görsel değişir. `PlayerController`, durumlar, anchor ve fizik mantığı değişmez. Tutunma anlık kalır.
- Ayarlar `ArmVisualSettingsSO`'da durur. MonoBehaviour'da tunable inspector alanı olmaz.
- Başlangıç değerleri: `idleReach` 0.6, `extendDuration` 0.1 sn (ease-out), genişlik omuz 0.08 → el 0.2.
- Renk: kol çizgisi ve el gövde renginde. Slingshot hakkı bitince yalnızca **el** "tükendi" rengine döner.
- `Time.deltaTime` ile çalışır, böylece hitstop'ta ve oda geçişinde kollar da donar.
- Sahne işleri yalnızca `unity command eval` ile yapılır, `.unity` dosyası elle düzenlenmez.
- Yeni yay yazılmaz, `UiSpring` kullanılır (CLAUDE.md kural 2).
- Yorum dili Türkçe (çevredeki dosyalar gibi).
- Commit atılmaz, kullanıcı isteyene kadar beklenir (kullanıcı tercihi). Planın "Commit" adımları bu yüzden yok.

## Review Focus

1. **Hareketli node ve hızlı salınım:** el anchor'a kilitliyken kol titrememeli, anchor her adım değişse de uzanma animasyonu baştan başlamamalı. Uzanma yalnızca anchor `null`'dan değere geçince ya da `AnchorNode` değişince başlar (eski `GrappleVisualManager` kuralı). Testi Task 2 Step 5'te.
2. **Stick bırakılınca:** `AimDirection` sıfır olur, el aşağı sarkar ve hızın tersine biraz kayar. Son yönde donup kalmaz. Testi Task 1'de (`IdleHandTarget_NoAim_Droops...`).
3. **Çok uzun kol (node 15+ birim uzakta):** kavis sıfıra yaklaşır, kol düz durur, `NaN` çıkmaz. Testi Task 1'de (`BowForLength_LongArm...`, `BowForLength_ZeroLength...`).
4. **Hitstop / timeScale 0:** `deltaTime` 0 olunca yay adımı bir şey değiştirmez ve kol donar. `UiSpring.Step(0)` bunu zaten sağlıyor, Task 2'de elle kontrol edilir.
5. **Ölüm ve respawn ışınlanması:** gövde ışınlanınca el eski konumdan ekranı boydan boya kat etmemeli. `ArmVisual`, el ile omuz arasındaki mesafe `idleReach × 4`'ü aşarsa ve anchor yoksa yayları omuza snap'ler. Testi Task 2 Step 5'te.

---

### Task 1: Kol şekli matematiği (saf)

**Files:**
- Create: `Assets/Scripts/New-Scripts/Player/Visual/ArmShapeMath.cs`
- Test: `Assets/Tests/EditMode/ArmShapeMathTests.cs`

**Interfaces:**
- Produces (`namespace New_Scripts.Player.Visual`, `public static class ArmShapeMath`):
  - `Vector2 Bezier(Vector2 a, Vector2 control, Vector2 b, float t)`: kuadratik Bezier.
  - `float BowForLength(float length, float idleReach, float idleBow)`: kavis = `idleBow × clamp01(idleReach / length)`. `length ≤ 0` ise `idleBow` döner.
  - `Vector2 IdleHandTarget(Vector2 shoulder, Vector2 aim, Vector2 bodyVelocity, float reach, float droopTrail)`: `aim.sqrMagnitude > 0.01` ise `shoulder + aim.normalized × reach`. Değilse `shoulder + ClampMagnitude(Vector2.down × reach − bodyVelocity × droopTrail, reach)`.
  - `float ExtendProgress(float elapsed, float duration)`: ease-out kübik `1 − (1 − p)³`, `p = clamp01(elapsed / duration)`. `duration ≤ 0` ise 1.

- [ ] **Step 1: Write the failing tests** (`ArmShapeMathTests`, tolerans 1e-4)

```csharp
[Test] public void Bezier_Endpoints_AreAAndB() {
    Assert.AreEqual(new Vector2(0,0), ArmShapeMath.Bezier(Vector2.zero, new Vector2(1,5), new Vector2(2,0), 0f));
    Assert.AreEqual(new Vector2(2,0), ArmShapeMath.Bezier(Vector2.zero, new Vector2(1,5), new Vector2(2,0), 1f)); }
[Test] public void Bezier_Mid_IsHalfwayToControl() // (0,0),(1,4),(2,0), t=.5 → (1,2)
[Test] public void BowForLength_AtIdleReach_IsIdleBow()      // (0.6, 0.6, 0.25) → 0.25
[Test] public void BowForLength_LongArm_ShrinksTowardZero()  // (15, 0.6, 0.25) → 0.01
[Test] public void BowForLength_ZeroLength_IsIdleBowNotNaN()  // (0, 0.6, 0.25) → 0.25
[Test] public void IdleHandTarget_WithAim_ReachesAlongAim()  // shoulder (1,1), aim (0,3) → (1,1.6)
[Test] public void IdleHandTarget_NoAim_DroopsDownWhenStill() // aim 0, vel 0 → (1,0.4)
[Test] public void IdleHandTarget_NoAim_TrailsBehindVelocity() // aim 0, vel (10,0), trail .04 → x < 1, |target-shoulder| ≤ 0.6+1e-4
[Test] public void ExtendProgress_Bounds() // (0,.1)→0, (.1,.1)→1, (.2,.1)→1, (.05,.1)→0.875, (.05,0)→1
```

- [ ] **Step 2: Run, expect FAIL** (compile error: `ArmShapeMath` yok)
  `unity command recompile`, ardından `recompile_status`. Hata `Editor.log`'da `error CS0103`.

- [ ] **Step 3: Implement `ArmShapeMath`**: imzalar yukarıda.

- [ ] **Step 4: Run, expect PASS**
  `unity command run_tests --mode EditMode --filter ArmShapeMathTests`: hepsi yeşil. Ardından tüm EditMode da yeşil olmalı.

---

### Task 2: `ArmVisualSettingsSO` + `ArmVisual` + `ArmController` nişan yönü

**Files:**
- Create: `Assets/Scripts/New-Scripts/Player/Visual/ArmVisualSettingsSO.cs`
- Create: `Assets/Scripts/New-Scripts/Player/Visual/ArmVisual.cs`
- Modify: `Assets/Scripts/New-Scripts/Player/ArmController.cs` (bütün dosya, 21 satır)
- Test: `Assets/Tests/EditMode/ArmShapeMathTests.cs` (Review Focus 1 ve 5 için iki saf yardımcı daha)

**Interfaces:**
- Consumes: Task 1'deki `ArmShapeMath.*`; `UiSpring(float stiffness, float damping)` ile `Value/Target/Velocity/Step(dt)`; `PlayerController.LeftAnchor/RightAnchor` (`Vector2?`), `LeftAnchorNode` / `RightAnchorNode` (`Transform`), `Velocity` (`Vector2`); `ActiveArm` enum.
- Produces:
  - `ArmController.AimDirection` (`Vector2`, sadece okunur). `UpdateArmRotation(dir)` artık pivot döndürmez: `dir.sqrMagnitude > 0.01` ise `dir.normalized`, değilse `Vector2.zero` saklar. Omuz konumu = `ArmController.transform.position` (değişmez).
  - `[CreateAssetMenu(fileName="ArmVisualSettings", menuName="Player/Arm Visual Settings")] ArmVisualSettingsSO` alanları (public get, private set, varsayılanlarla): `IdleReach` 0.6, `IdleBow` 0.25, `HandStiffness` 220, `HandDamping` 12, `BowStiffness` 90, `BowDamping` 6, `DroopTrail` 0.04, `ExtendDuration` 0.1, `ShoulderWidth` 0.08, `HandWidth` 0.2, `Resolution` 20, `ArmColor` (Color).
  - `ArmVisual : MonoBehaviour`. Serialize alanlar: `PlayerController player`, `ActiveArm arm`, `ArmController armController`, `LineRenderer line`, `Transform hand`, `ArmVisualSettingsSO settings`.
  - `ArmShapeMath.IsNewGrab(bool wasActive, Transform prevNode, Transform node, Vector2? prevAnchor, Vector2 anchor) → bool`: `!wasActive || node != prevNode || (node == null && prevAnchor != anchor)`.
  - `ArmShapeMath.ShouldSnap(Vector2 hand, Vector2 shoulder, float idleReach) → bool`: mesafe > `idleReach × 4`.

- [ ] **Step 1: Failing tests** (Review Focus 1 ve 5)

```csharp
[Test] public void IsNewGrab_SameMovingNode_IsFalse()  // wasActive, aynı node, farklı anchor → false
[Test] public void IsNewGrab_FromNothing_IsTrue()      // wasActive=false → true
[Test] public void IsNewGrab_StaticPointMoved_IsTrue() // node null, anchor değişti → true
[Test] public void ShouldSnap_Teleport_IsTrue()        // hand (0,0), shoulder (10,0), reach .6 → true
[Test] public void ShouldSnap_NormalSway_IsFalse()     // hand (0,0), shoulder (1,0), reach .6 → false
```

- [ ] **Step 2: Run, expect FAIL.** Ardından iki yardımcıyı `ArmShapeMath`'e ekle, **PASS**.

- [ ] **Step 3: `ArmController` ve `ArmVisualSettingsSO`'yu yaz** (imzalar yukarıda). `rotationPivot` alanı kalkar.

- [ ] **Step 4: `ArmVisual.LateUpdate` algoritması** (`dt = Time.deltaTime`; el için `UiSpring` x/y, orta nokta için `UiSpring` x/y):

```
shoulder = armController.transform.position
anchor   = arm==Left ? player.LeftAnchor : player.RightAnchor
node     = arm==Left ? player.LeftAnchorNode : player.RightAnchorNode
if anchor varsa:
    if IsNewGrab(...): extendStart = handPos; elapsed = 0
    elapsed += dt
    handPos = Lerp(extendStart, anchor, ExtendProgress(elapsed, ExtendDuration))
    el yaylarının Value = handPos, Velocity = 0      // bırakınca buradan geri döner
else:
    if ShouldSnap(handPos, shoulder, IdleReach): el ve orta yaylarını omuza snap'le (Velocity 0)
    target = IdleHandTarget(shoulder, armController.AimDirection, player.Velocity, IdleReach, DroopTrail)
    el yaylarının Target = target; Step(dt); handPos = (Value.x, Value.y)
dir = handPos − shoulder; len = |dir|; perp = (−dir.y, dir.x)/len (len < 1e-4 ise perp = up)
midTarget = (shoulder + handPos)/2 + perp × BowForLength(len, IdleReach, IdleBow)
orta yaylarının Target = midTarget; Step(dt)
anchor varsa orta yayın Value'sunu midTarget'a doğru  Lerp(…, ExtendProgress(elapsed,ExtendDuration))  çek   // gerilince düzleşsin
Resolution noktayı Bezier(shoulder, mid, handPos, i/(n−1)) ile line'a yaz
hand.position = handPos; hand.right = dir (len > 1e-4 ise)
```
Awake: `line.useWorldSpace = true`, `line.positionCount = Resolution`, `line.widthCurve = (0 → ShoulderWidth, 1 → HandWidth)` (`widthMultiplier` 1), `line.startColor = line.endColor = ArmColor`. Yayları omuz konumuyla başlat.

- [ ] **Step 5: Compile temiz + EditMode yeşil.** Çıktıyı raporla.

---

### Task 3: Sahne kablolaması, renk, eski ipin kaldırılması, doküman

**Files:**
- Create: `Assets/Scripts/New-Scripts/Player/SO/ArmVisualSettings.asset` (eval ile `ScriptableObject.CreateInstance` + `AssetDatabase.CreateAsset`)
- Delete: `Assets/Scripts/New-Scripts/Player/GrappleVisualManager.cs` (+ `.meta`), ardından sahnedeki component'i kaldır
- Modify (eval ile): `NewScene.unity` içindeki Player
- Modify: `docs/systems/grapple.md`

- [ ] **Step 1: `ArmColor`'ı seç.** Gövde sprite'ının ana dolgu rengini (`Animation` altındaki SpriteRenderer'ın sprite'ının ortasındaki piksel, `sprite.texture.GetPixel`, texture okunabilir değilse ekran görüntüsünden) oku ve asset'e yaz. Değeri raporla.

- [ ] **Step 2: Player'ı eval ile kablola:**
  - `VisualsManager`'dan `GrappleVisualManager` component'ini kaldır, ardından script dosyasını sil. Başka referans var mı diye kontrol et: `grep -rn GrappleVisualManager Assets docs`.
  - `LeftArmPivot/Square` ve `RightArmPivot/Square` SpriteRenderer'larını kapat (GameObject `SetActive(false)`).
  - `LeftRope`'a `ArmVisual` ekle (`arm = Left`, `armController = LeftArmPivot`, `line =` kendi LineRenderer'ı). `RightRope` için aynısı (`Right`).
  - Her rope'un altına `Hand` adlı bir child + `SpriteRenderer` ekle. Yer tutucu sprite: projedeki yuvarlak sprite (yoksa Unity'nin built-in `Knob`'u), scale ≈ 0.3. `ArmVisual.hand` = bu transform.
  - Çizim sırası: rope'lar gövdenin bir alt `sortingOrder`'ında, `Hand` bir üstünde (gövdenin değeri SpriteRenderer'dan okunur).
  - `PlayerColorController.armRenderers` = iki `Hand` SpriteRenderer'ı, `defaultArmColor` = iki kez `ArmColor`. `slingshotExhaustedColor` değişmez.
  - Sahneyi kaydet (`EditorSceneManager.SaveScene`).

- [ ] **Step 3: Görsel kanıt.** Play Mode'da `RoomNew-FlowerTest` odasında `capture_game_view --source screen` ile 3 kare al: boşta, tek kol tutunmuş, iki kol tutunmuş. PNG'leri `docs/ui-research/stretchy-arms/` altına taşı, `Assets/` altında kalan klasörü ve `.meta` dosyalarını sil.

- [ ] **Step 4: Testler.** EditMode yeşil, ayrıca PlayMode kural testleri (`run_tests --mode playmode --async_tests true`, `test_status`, `TestResults.xml`) yeşil. Çıktıyı raporla.

- [ ] **Step 5: `docs/systems/grapple.md`'yi güncelle.** "İp animasyonu (`GrappleVisualManager`)" geçen satırları `ArmVisual` ile değiştir. Kod listesinde `GrappleVisualManager.cs` yerine `Visual/ArmVisual.cs`, `Visual/ArmShapeMath.cs`, `Visual/ArmVisualSettingsSO.cs` yaz. Testler bölümüne `ArmShapeMathTests` ekle. "Son kod doğrulaması: 2026-11-2x (uzayan kollar)" diye güncelle.

- [ ] **Step 6: Kullanıcıya teslim.** Başlangıç değerlerini listele ve gamepad ile denemesini iste (CLAUDE.md kural 4).
