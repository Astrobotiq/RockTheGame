# Pause

*GDD §11 (pause yok denmişti). Son kod doğrulaması: 2026-11-08.*

## Tasarım niyeti

Start tuşu oyunu durdurur, tekrar basınca devam ettirir. Süre dururken UI çalışabilmeli (pause menüsü sonraki task'ta). Çok oyunculu yarışta pause yok: rakip hayalet akıyor, dünyayı durdurmak anlamsız.

**Karıştırma:** `PlayerController.IsPaused` ve `HitStopEvents` hit-stop / respawn içindir, oyun pause'u değildir. Pause durumu `PauseManager.IsPaused`.

## Nasıl çalışıyor

1. `Player/Pause` action'ı (`<Gamepad>/start`) `PauseManager.Update`'te okunur.
2. `PauseManager.CanPause()` izin sorar (`PauseRules.CanPause`): oyuncu aktif olmalı; respawn/hit-stop (`IsPaused`), oda geçişi (`PlayerTransitionState`) ve `Block()` ile kapatılmış durum (çok oyunculu) izin vermez. **Devam etmek için izin gerekmez.**
3. Duraklayınca: `Time.timeScale = 0` (önceki değer saklanır, devamda geri verilir), `AudioListener.pause = true`, `HapticManager.ResetHaptics()`, müzik efekti (aşağıda). `PauseChanged(bool)` olayı tetiklenir.
4. `PlayerController.Update`, `PauseManager.BlocksGameplay` true iken çalışmaz. Bu, devam edilen karede de true: devam için basılan tuş (A) aynı karede zıplama olarak okunmaz. Fizik zaten `timeScale 0` ile durur.
5. `PauseManager` kapanırsa (sahne değişimi) süre ve ses geri açılır.

Saf mantık `Pause/PauseState.cs` ve `PauseRules.cs`'te (EditMode testli: `PauseStateTests`). `PauseManager` NewScene'de `Managers/PauseManager` altında; `pauseAction` ve `player` atanmış.

## Müzik: "uzaktaki odadan geliyor" efekti

Müzik pause'ta çalmaya devam eder (`AudioManager` müzik kaynaklarında `ignoreListenerPause = true`); efektler (SFX, ambient) durur. Müziğe boğuk, uzak bir ses verilir:

- `MainMixer`'daki Music grubunda iki efekt var: **Lowpass Simple** (tizleri keser) ve **SFX Reverb** (hafif yankı). Parametreler exposed: `MusicLowpassCutoff`, `MusicReverbRoom`, `MusicDryLevel` (kuru sesi kısar, yankıyı öne çıkarır).
- `PauseManager`, `Assets/Audio/SO/PauseAudioSettings.asset` (`PauseAudioSettingsSO`) değerlerine DOTween `DOSetFloat(...).SetUpdate(true)` ile geçer; `timeScale 0` iken de işler. Devamda varsayılana (22000 Hz, −10000, 0) döner.
- Başlangıç değerleri: cutoff **1000 Hz**, reverb room **−1800 mB**, dry **−600 mB**, geçiş **0,3 sn**. Bunlar tahmin; **kulakla ayarlanacak** (Play Mode'da asset'i değiştirip pause'a bas).
- Snapshot kullanılmadı: `TransitionTo` exposed parametreleri snapshot değerlerine çeker ve oyuncunun kayıtlı ses ayarlarını (`MusicVolume` vb.) ezerdi. Parametreler kodla sürülüyor.
- Efekt eklemek/değiştirmek için mixer asset'i Editor'de açılır (Audio Mixer penceresi); parametre adları SO'daki alanlarla eşleşmeli.

## Çok oyunculu

`MultiplayerSession.Create` `PauseManager.Block(session)`, `Shutdown` `Unblock` çağırır. Editor'de NewScene'i doğrudan Play'lemek `MultiplayerDevLauncher` ile oturum açar, bu yüzden **orada pause bilerek çalışmaz**. Tek oyunculu denemek için ana menüden gir ya da oturumu kapat.

## Kod

- `Pause/PauseManager.cs`, `PauseState.cs`, `PauseRules.cs`
- `Player/PlayerController.cs`: `Update` başındaki `BlocksGameplay` kontrolü
- `Multiplayer/MultiplayerSession.cs`: `Block`/`Unblock`
- `Pause/PauseAudioSettingsSO.cs` + `Audio/SO/PauseAudioSettings.asset`, `Audio/SO/MainMixer.mixer`
- `Audio/AudioManager.cs`: müzik kaynaklarında `ignoreListenerPause`
- `Input/PlayerControls.inputactions`: `Pause` action'ı

## Bilinen kenar durumlar ve açık kararlar

1. **Müzik efekti kulakla ayarlanmadı.** Değerler tahmin; sesi ben duyamadım, yalnızca parametrelerin hedefe gittiği ve geri döndüğü doğrulandı.
2. **Süreye bağlı tween/animator donar.** Pause menüsü UI'ı `SetUpdate(true)` / Unscaled Time kullanmalı (`CollectibleUIController` zaten kullanıyor). `AudioManager` müzik çapraz geçişi `Time.deltaTime` kullandığı için pause anında bir geçiş varsa orada durur.
3. **Hit-stop sırasında Start yutulur** (`IsPaused` true, çok kısa).
4. **Doğrulama:** Play Mode'da sanal gamepad ile Start → `timeScale 0`, ses duruyor, oyuncu sabit; A'ya basmak zıplama tamponuna yazmıyor; Start → devam. Gerçek gamepad ve his playtest'i yapılmadı.
