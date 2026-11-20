# Stamina yanıp sönmesi Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Kırılan stamina halkasını kaldır; duvar dayanıklılığı son 1,5 sn'ye girince karakterin gövdesi kırmızıya yanıp söner (Celeste gibi), ek arayüz yok.

**Architecture:** Yanıp sönme mantığı saf bir sınıfta (`StaminaFlashState`, EditMode testli). İnce bir MonoBehaviour (`StaminaWarningFlash`) onu her karede sürer ve sonucu `PlayerColorController.SetFlash` ile `PaletteSwap` shader'ının `_FlashAmount`/`_FlashColor` özelliğine yazar (dash'in `_Blend`'ine dokunmaz). `PlayerUIController` aynı genel API ile halkanın yerine bu bileşene yönlendirir.

**Tech Stack:** Unity 6000.3.10f1, URP 2D, C# (`RockTheGame.Runtime`), NUnit EditMode, `Sprites/Custom/PaletteSwap` shader.

**Spec:** `docs/superpowers/specs/2026-11-20-stamina-flash-design.md`

## Global Constraints

- Eşik `PlayerStatsSO.StaminaWarningThreshold` (1,5 sn); yeni eşik eklenmez.
- Zaman: unscaled (`Time.unscaledDeltaTime`).
- Tunable değerler `PlayerStatsSO`'da (CLAUDE.md: yeni tunable'lar oraya), hardcode yok. Renk `UiThemeSO.Load().Red` (`#FC1E00`), elle hex yok.
- Yanıp sönme yalnız `WallClimbingState` aktifken; duvardan inince, yere basınca, respawn'da, enerji dolunca anında durur ve gövde normale döner.
- Kapsam dışı: titreşim/ses; kollar (kol renkleri dash/sling bilgisini taşıyor, flash yalnız gövdede).
- "Game feel" insanın kararı: değerleri yaz, eski/yeni değeri raporla, gamepad testi iste. `PlayerStatsSanityTests` bandları değişmez.
- Commit küçük, `feature/flower-node` dışında yeni dal aç; **push yok**. Çalışma ağacındaki ilgisiz değişiklikleri (örn. `M_CRTFilter.mat`, kol/ArmVisual dosyaları) commit'leme; yalnız bu plana ait dosyaları `git add` ile tek tek ekle.
- Her `.cs`/`.shader` değişikliğinden sonra: `unity command recompile` → `recompile_status` `completed`/`up_to_date`; derleme hatası `%LOCALAPPDATA%/Unity/Editor/Editor.log` içinde `error CS` satırlarıdır.

## Review Focus

1. Eşik ≤ 0 veya enerji 0 iken sıfıra bölme / sonsuz yanıp sönme olmasın; flash çıkmasın (Task 1 testi).
2. Oyuncu bir kare takılırsa (hitstop, 0,5 sn'lik dt sıçraması) faz taşsın, NaN/negatif olmasın (Task 1 testi).
3. Yanıp sönerken duvardan inip tekrar tutunma: inince normal renk, tekrar tutununca eşikteyse hemen yanıp sönme (Task 1 testi).
4. Yanıp sönerken ölüm: `DeathBurst` beyaz silüeti kırmızı çıkmasın (aynı materyal örneğini paylaşıyorlar; Task 2).
5. Dash tükenmiş (`_Blend = 1`) ve flash aynı anda: ikisi birbirini ezmesin (Task 2 doğrulaması).

---

### Task 1: Yanıp sönme mantığı (saf)

**Files:**
- Create: `Assets/Scripts/New-Scripts/Player/Visual/StaminaFlashState.cs`
- Test: `Assets/Tests/EditMode/StaminaFlashStateTests.cs`

**Interfaces:**
- Produces (namespace `New_Scripts.Player.Visual`):
  - `public static float StaminaFlashState.FlashHz(float remaining, float threshold, float hzAtThreshold, float hzAtZero)`: `Mathf.Lerp(hzAtZero, hzAtThreshold, Mathf.Clamp01(remaining / threshold))`; `threshold <= 0` ise `hzAtThreshold`.
  - `public sealed class StaminaFlashState` ile `public void SetActive(bool active)`, `public void Stop()` (active=false, faz=0), `public bool Step(float dt, float remaining, float threshold, float hzAtThreshold, float hzAtZero)` → bu karede flash gösterilsin mi.

- [ ] **Step 1: Testleri yaz** (`StaminaFlashStateTests`, namespace `RockTheGame.Tests`). Her test `hzAtThreshold = 7f, hzAtZero = 12f, threshold = 1.5f` kullanır:
  - `FlashHz_AtThreshold_IsStartHz`: `FlashHz(1.5f,1.5f,7f,12f)` = 7; `FlashHz(0f,...)` = 12; `FlashHz(0.75f,...)` = 9.5 (±0.001); `FlashHz(3f,...)` = 7 (eşik üstü kıskaç).
  - `FlashHz_NonPositiveThreshold_ReturnsStartHz`: `FlashHz(1f, 0f, 7f, 12f)` = 7.
  - `Step_NotActive_NeverFlashes`: yeni durumda `Step(0.016f, 1f, 1.5f, 7f, 12f)` false.
  - `Step_ActiveAboveThreshold_NoFlash`: `SetActive(true)`, `Step(0.016f, 2f, ...)` false.
  - `Step_ActiveInWarning_FirstFrameIsOn`: `SetActive(true)`, `Step(0.001f, 1f, ...)` true.
  - `Step_ActiveInWarning_TogglesOverTime`: 7 Hz'de 0,05 sn adımlarla 20 adım at; en az bir true ve en az bir false dönmeli.
  - `Step_ZeroRemaining_NoFlash`: `SetActive(true)`, `Step(0.016f, 0f, ...)` false.
  - `Step_NonPositiveThreshold_NoFlash`: `SetActive(true)`, `Step(0.016f, 1f, 0f, ...)` false.
  - `Step_HugeDt_WrapsPhase`: `SetActive(true)`, `Step(0.5f, 1f, ...)` çağrısı istisna atmaz ve sonraki `Step(0.001f, 1f, ...)` bool döner (NaN olmaz; `float.IsNaN` ile değil, çağrının dönmesiyle).
  - `Stop_ClearsFlashAndPhase`: `SetActive(true)`, uyarıda birkaç `Step`, `Stop()`, sonra `Step(0.001f, 1f, ...)` false (aktif değil); `SetActive(true)` sonrası `Step(0.001f, 1f, ...)` yine true (faz sıfırlanmış).
  - `ReGrab_InWarning_FlashesImmediately`: `SetActive(true)`, uyarıda `Step`, `SetActive(false)`, `Step` false, `SetActive(true)`, `Step(0.001f, 1f, ...)` true.
- [ ] **Step 2: Çalıştır, başarısız olsun**

Run: `unity command recompile` (derleme hatası beklenir: tip yok), hatayı `Editor.log`'da gör.
Expected: `StaminaFlashState` bulunamadı hatası.

- [ ] **Step 3: `StaminaFlashState`'i uygula.** Faz 0..1 arası; her `Step`'te `faz += FlashHz(...) * dt`, `faz -= Mathf.Floor(faz)`; flash = `faz < 0.5f`. Etkin değilken veya `remaining <= 0f`, `threshold <= 0f`, `remaining > threshold` iken fazı 0'a çek ve false dön. Dosyanın üstüne Türkçe XML özeti.
- [ ] **Step 4: Testler geçsin**

Run: `unity command recompile`, `recompile_status` bekle, sonra `unity command run_tests --mode EditMode --filter RockTheGame.Tests.StaminaFlashStateTests`
Expected: 10 test passed, 0 failed.

- [ ] **Step 5: Commit** (`Assets/Scripts/New-Scripts/Player/Visual/StaminaFlashState.cs`, `.meta` dosyaları ve test dosyası): `feat(stamina): yanip sonme mantigi (saf, testli)`

---

### Task 2: Shader ve renk kontrolcüsü: flash rengi

**Files:**
- Modify: `Assets/Shader/PaletteSwap.shader` (Properties `Flash` başlığı satır ~30, `frag` satır ~149)
- Modify: `Assets/Scripts/New-Scripts/Player/Visual/PlayerColorController.cs`
- Modify: `Assets/Scripts/New-Scripts/Death/DeathBurst.cs:222-224`

**Interfaces:**
- Produces: `public void PlayerColorController.SetFlash(float amount, Color color)`: gövde materyalinin `_FlashAmount`/`_FlashColor` özelliğini yazar; `bodyMaterial == null` ise sessizce çıkar; `_Blend`e dokunmaz.
- Shader: yeni özellik `_FlashColor ("Flash Color", Color) = (1,1,1,1)`.

- [ ] **Step 1: Shader'a `_FlashColor` ekle.** Properties'e `_FlashColor` (varsayılan beyaz), `CGPROGRAM` içine `fixed4 _FlashColor;`, ve `finalColor.rgb = lerp(finalColor.rgb, _FlashColor.rgb, _FlashAmount);` (şu an `fixed3(1,1,1)` yerine). Yorumu güncelle ("silüet `_FlashColor`a çekilir; varsayılan beyaz").
- [ ] **Step 2: `PlayerColorController.SetFlash(float amount, Color color)` ekle.** `Shader.PropertyToID` önbellekli iki `static readonly int` alan; `bodyMaterial.SetFloat/SetColor`. Aynı sınıfta `_Blend` kodu değişmez.
- [ ] **Step 3: `DeathBurst`'te block'a `_flashBlock.SetColor(FlashColorId, Color.white);` ekle** (`FlashColorId = Shader.PropertyToID("_FlashColor")`, `FlashAmountId` gibi static alan). Gerekçe: silüet `body.sharedMaterial` (stamina flash'ın yazdığı aynı örnek) kullanıyor; yanıp sönerken ölüm silüeti kırmızı çıkmasın.
- [ ] **Step 4: Derleme temiz**

Run: `unity command recompile`, `recompile_status`
Expected: `completed`/`up_to_date`, `Editor.log`'da `error CS` ve shader hatası yok.

- [ ] **Step 5: EditMode testleri hâlâ yeşil**

Run: `unity command run_tests --mode EditMode`
Expected: tümü passed (özellikle `StaminaFlashStateTests`, `PlayerStatsSanityTests`).

- [ ] **Step 6: Commit** (üç dosya): `feat(stamina): PaletteSwap _FlashColor ve PlayerColorController.SetFlash`

(Görsel doğrulama, `_Blend=1` ile flash birlikte, Task 4'te sahnede yapılır; flash bileşeni burada henüz yok.)

---

### Task 3: Yanıp sönme bileşeni ve ayarlar

**Files:**
- Modify: `Assets/Scripts/New-Scripts/Player/PlayerStatsSO.cs:74` (`StaminaWarningThreshold` satırının altına)
- Create: `Assets/Scripts/New-Scripts/Player/Visual/StaminaWarningFlash.cs`

**Interfaces:**
- Consumes: `StaminaFlashState` (Task 1), `PlayerColorController.SetFlash` (Task 2), `UiThemeSO.Load().Red`.
- Produces (`New_Scripts.Player.Visual.StaminaWarningFlash : MonoBehaviour`):
  - `void Bind(PlayerColorController colors, PlayerStatsSO stats)`
  - `void SetActive(bool active)`: tırmanma başladı/bitti; `false` ise anında `Stop()`.
  - `void SetStamina(float current, float threshold)`: son değerleri saklar.
  - `void Stop()`: durumu sıfırla ve `colors.SetFlash(0f, red)` çağır (zaten durmuş olsa bile; ölüm sonrası temizlik için).
- Yeni `PlayerStatsSO` alanları (başlangıç önerisi, hepsi `[Tooltip]`li): `public float StaminaFlashHzAtThreshold = 7f;` `public float StaminaFlashHzAtZero = 12f;` `[Range(0f,1f)] public float StaminaFlashAmount = 0.7f;`

- [ ] **Step 1: `PlayerStatsSO`'ya üç alanı ekle** (Türkçe tooltip: "Uyarı eşiğinde yanıp sönme hızı (Hz)", "Enerji bittiğinde hız (Hz), eşikten sıfıra doğru hızlanır", "Flash'ın gövdeyi kırmızıya çekme oranı (1 = düz kırmızı)").
- [ ] **Step 2: `StaminaWarningFlash`'i yaz.** `Update`'te `Time.unscaledDeltaTime` ile `StaminaFlashState.Step(...)` çağır, sonuç değişince `colors.SetFlash(on ? stats.StaminaFlashAmount : 0f, red)`. `Bind` öncesi `Update` güvenli çıkar (null kontrolü). `OnDisable`'da `Stop()`. `red` = `UiThemeSO.Load().Red`, `Awake`'te bir kez alınır.
- [ ] **Step 3: Derleme temiz + EditMode yeşil**

Run: `unity command recompile`, `recompile_status`, `unity command run_tests --mode EditMode`
Expected: derleme hatası yok; tümü passed. (`PlayerStatsSanityTests` bantları değişmedi.)

- [ ] **Step 4: Commit** (`PlayerStatsSO.cs`, `StaminaWarningFlash.cs` + `.meta`): `feat(stamina): StaminaWarningFlash bileseni ve PlayerStatsSO ayarlari`

---

### Task 4: Bağlama, halkayı silme, dokümanlar, doğrulama

**Files:**
- Modify: `Assets/Scripts/New-Scripts/Player/UI/PlayerUIController.cs`
- Modify: `Assets/Scripts/New-Scripts/Player/States/WallClimbingState.cs:176-186` (`ExitState`)
- Delete: `Assets/Scripts/New-Scripts/UI/Hud/HudStaminaRing.cs`, `.../StaminaRingMath.cs`, `Assets/Tests/EditMode/StaminaRingMathTests.cs` (+ `.meta`)
- Modify: `docs/systems/wall-movement.md` ("Kırılan stamina halkası" bölümü satır ~29-37, dosya listesi ~85, "Bilinen eksikler" ~90), `docs/systems/ui-motion.md:74` (HUD stamina satırı), ikisinin "Son kod doğrulaması" tarihi

**Interfaces:**
- Consumes: `StaminaWarningFlash` (Task 3).
- `PlayerUIController` genel API aynı kalır: `ShowStaminaBar()`, `UpdateStamina(float current, float max, float warningThreshold)`, `RefillAndHideStaminaBar()`; yeni: `public void StopStaminaWarning()`.

- [ ] **Step 1: `PlayerUIController`'ı yeniden bağla.** `Awake`'te eski `staminaBarGroup` kapatma kalır; `HudStaminaRing` yerine `gameObject.AddComponent<StaminaWarningFlash>()`; `Start`'ta `GetComponentInParent<PlayerController>()` ile `Bind(player.ColorController, player.Stats)`. Eşleme: `ShowStaminaBar` → `SetActive(true)`; `UpdateStamina(c, m, t)` → `SetStamina(c, t)`; `RefillAndHideStaminaBar` ve `StopStaminaWarning` → `Stop()`. Sınıf özetini ve `staminaBarGroup` tooltip'ini güncelle ("halka" → "karakter yanıp sönmesi"). `ShowStaminaBar` adı yanıltıcı kalır, bilinçli: genel API değişmez; XML özetinde belirt.
- [ ] **Step 2: `WallClimbingState.ExitState`'in sonuna `context.UIController.StopStaminaWarning();` ekle.**
- [ ] **Step 3: Halka dosyalarını ve testini sil** (`git rm`); başka referans kalmadığını doğrula: `Grep` `StaminaRing|HudStaminaRing|IntactPieces` yalnız docs/spec/TASKS.md'de çıkmalı.
- [ ] **Step 4: Derleme ve testler**

Run: `unity command recompile`, `recompile_status`, `unity command run_tests --mode EditMode`
Expected: derleme temiz; EditMode tümü passed (StaminaRingMathTests artık yok).

- [ ] **Step 5: PlayMode kuralları**

Run: `unity command run_tests --mode playmode --async_tests true`, `unity command test_status` ile bekle, sonuçları `%USERPROFILE%/AppData/LocalLow/Astrobotiq/RockTheGame/TestResults.xml`'den oku.
Expected: `WallRulesTests` (özellikle `W2_Landing_RefillsStamina`, `W2_LaunchPad_RefillsStamina`, `W2_Respawn_RefillsStamina`) ve diğer PlayMode testleri passed; yeni kırılan yok (öncekiyle karşılaştır).

- [ ] **Step 6: Sahnede görsel doğrulama (Play Mode).** `unity command eval` ile Play Mode'da oyuncunun `PlayerUIController`'ında `ShowStaminaBar()`, ardından `UpdateStamina(1f, 6f, 1.5f)` çağır; `capture_game_view --source screen` ile iki kare yakala (md5 farklı olmalı; `capture_game_view` eski kare verebilir) ve gövdenin kırmızıya çekildiğini gör. Sonra `PlayerController.UseDash()` ile `_Blend = 1` yap, flash sürerken taç grileşmiş olmalı (iki efekt birbirini ezmez). `RefillAndHideStaminaBar()` sonrası gövde normal. Sonuçları (görüntü yolları, md5) raporla.
- [ ] **Step 7: Dokümanları güncelle.** `wall-movement.md`: halka bölümünü "Stamina yanıp sönmesi" olarak yeniden yaz (eşik, Hz değerleri 7→12, `_FlashAmount` 0,7, durma koşulları, `ColorController.SetFlash`, kollar kapsam dışı, titreşim yok), dosya listesini ve "Bilinen eksikler"i (halka referansları) düzelt. `ui-motion.md` HUD stamina satırı: "HUD'da stamina göstergesi yok; karakter yanıp söner, bkz. `wall-movement.md`". İki dosyada "Son kod doğrulaması" = 2026-11-20 (tarihi gerçek çalışma gününe çevir).
- [ ] **Step 8: Commit** (silinen dosyalar dahil, yalnız bu plana ait dosyalar): `feat(stamina): halka yerine karakter yanip sonmesi`
- [ ] **Step 9: İnsana devret.** Raporda eski/yeni değerleri yaz (halka: 7 Hz sabit, kopan parçalar → yeni: 7→12 Hz, `StaminaFlashAmount` 0,7) ve gamepad ile test iste: son 1,5 sn okunuyor mu, kırmızı pembede yeterince görünüyor mu, tırmanma animasyonu rahat görünüyor mu. Ayar yeri: `PlayerStatsSO` (`StaminaFlash*`).
