# Pause

*GDD §11 (pause yok denmişti). Son kod doğrulaması: 2026-11-14.*

## Tasarım niyeti

Start tuşu oyunu durdurur, tekrar basınca devam ettirir. Süre dururken UI çalışır: radyal pause menüsü (aşağıda). Çok oyunculu yarışta pause yok: rakip hayalet akıyor, dünyayı durdurmak anlamsız.

**Karıştırma:** `PlayerController.IsPaused` ve `HitStopEvents` hit-stop / respawn içindir, oyun pause'u değildir. Pause durumu `PauseManager.IsPaused`.

## Nasıl çalışıyor

1. `Player/Pause` action'ı (`<Gamepad>/start`) `PauseManager.Update`'te okunur.
2. `PauseManager.CanPause()` izin sorar (`PauseRules.CanPause`): oyuncu aktif olmalı; respawn/hit-stop (`IsPaused`), oda geçişi (`PlayerTransitionState`) ve `Block()` ile kapatılmış durum (çok oyunculu) izin vermez. **Devam etmek için izin gerekmez.**
3. Duraklayınca: `Time.timeScale = 0` (önceki değer saklanır, devamda geri verilir), `AudioListener.pause = true`, `HapticManager.ResetHaptics()`, müzik efekti (aşağıda). `PauseChanged(bool)` olayı tetiklenir.
4. `PlayerController.Update`, `PauseManager.BlocksGameplay` true iken çalışmaz. Bu, devam edilen karede de true: devam için basılan tuş (A) aynı karede zıplama olarak okunmaz. Fizik zaten `timeScale 0` ile durur.
5. `PauseManager` kapanırsa (sahne değişimi) süre ve ses geri açılır.

Saf mantık `Pause/PauseState.cs` ve `PauseRules.cs`'te (EditMode testli: `PauseStateTests`). `PauseManager` NewScene'de `Managers/PauseManager` altında; `pauseAction` ve `player` atanmış.

## Radyal menü

`PauseMenuPanel` (NewScene, `Managers/PauseMenu`) `PauseManager.PauseChanged`'e bağlanır, Overlay canvas'ı kodla kurar (`UiPixelScale`: tam sayı sanat pikseli ölçeği, 1 sanat pikseli = 4 canvas birimi; UR-3, 2026-11-14), dört `UiArc` dilimi çizer. Dilim 0 tepede, saat yönünde: **resume** (yukarı), **options** (sağ), **quit** (aşağı), **retry** (sol).

| Girdi | Ne yapar |
|---|---|
| Sol stick | Yönündeki dilimi seçer (`atan2`, ölü bölge 0,5). Seçilemeyen dilim ya da ölü bölge seçimi değiştirmez. |
| D-pad | Yönündeki dilimi doğrudan seçer (stick ile aynı eşleme). |
| LB / RB | Seçimi saat yönünün tersine / yönünde döndürür, seçilemeyenleri atlar ve sarar. |
| A | Seçili dilimi onaylar (açılıştan sonraki ilk 0,4 sn kilitli). |
| B / Start | Devam eder (kilitlenmez). |

- **Resume:** `PauseManager.Resume()`.
- **Options:** ayar ekranı yok; soluk ve seçilemez, gezinme atlar (ana menüdeki `options` gibi). Ayar ekranı gelince `Slice.Enabled` açılır ve eylemi bağlanır.
- **Quit:** devam eder, sonra `MainMenu` sahnesine döner. Onay sorusu yok.
- **Retry:** devam eder, sonra `PlayerHealth.Kill()`: mevcut ölüm/respawn akışı (son checkpoint, ölüm sayılır). Sahneyi baştan yüklemez.
- **Hareket (`ui-motion.md` listesi):** seçili dilim sarıya boyanır, dışarı kayar (6 birim), büyür (106→118), gölgesi çakılır. Hepsi `UiSpring` + unscaled zaman. Değerler başlangıç önerisi, gamepad ile ayarlanır (`PauseMenuPanel` Inspector alanları).
- **Giriş (2026-11-09 yeniden yazıldı):** ilk sürümde dilimler 0,05–0,23 sn merkezde yığılı bekleyip sonra dağılıyordu ve yalnızca topluca soluyordu; kötü göründü. Şimdi her dilim saat yönünde 0,05 sn arayla, beklemeden, kendi yönünde içeriden (20 piksel) gelir: konum + ölçek (0,7→1) + görünürlük birlikte, hafif aşımlı yay (stiffness 340, damping 22). Halka 14° dönerek oturur. Seçili dilim önce gri gelir, ~0,28 sn'de sarıya boyanır ve gölgesi çakılır. Toplam ~0,5 sn; onay ilk 0,4 sn kilitli. **Çıkış** kısa (~0,15 sn), aşımsız, topluca solar ve küçülür. Süre/ease rehberi: üçüncü taraf motion-design skill metinleri (giriş 300–500 ms, stagger toplamı ≤ 500 ms, ease-out, opaklık tek başına değil, çıkış girişten kısa), skill kurulmadı, yalnızca okundu. Hissi gamepad ile sen değerlendireceksin.
- **Girdi:** panel kendi `InputAction`'larını kodla kurar (`<Gamepad>` leftStick, dpad, buttonSouth/East, shoulders); yalnızca açıkken etkin. Oyun içi `Player` haritasına dokunmaz.
- **Saf mantık:** `RadialMenuSelection` (açı → dilim, seçilemeyeni atlayarak döndürme), EditMode testli: `RadialMenuSelectionTests`.
- **Arka plan** (`PauseBackdrop`, panelin canvas'ında ringin arkasında): dither + %50 ink karartma tek tam ekran Image'da, `RockTheGame/UI/Dither` shader'ıyla (doku ve CPU mesh/uv yok: 2x2'lik sanat pikseli damasını mesh konumundan üretir; `UiThemeSO.DitherMaterial`, `UiConcrete` ile aynı kalıp). Kaymaz: yavaş kayma denendi, kareler kafa karıştırdığı için kapatıldı (`DitherScrollPixelsPerSecond` = 0; >0 yapılırsa tam piksel adımlarıyla çapraz kayar, CPU yalnızca malzemeye `_Scroll` yazar), iki boyalı daire (kırmızı, mavi; mockup ölçüleri) ve kesişimleri (`UiThemeSO.Purple`, `PolygonMath.CircleLens` ile hesaplanan mercek çokgeni), sağ-alt köşeye bağlı sarı üçgen (sert gölgeli). Çerçeve/gölge `UiThemeSO` denge değerinden. Dokusuz, düz boya (ikonlardaki gibi). Daireler tek grupta; seçili dilime göre sabit poza yerleşir (`PauseBackdrop.Pose`: grup konumu, dönüş, ölçek, üçgen kayması), boşta yavaşça sürüklenir, girişte daireler sol-üstten, üçgen sağ-alttan kayarak gelir. Yalnızca transform oynar (mesh yeniden üretilmez). Yeni şekil bileşeni: `UiPolygon` (dışbükey çokgen, tüylü kenar).
- Merkez disk (oda, ölüm sayısı) henüz yok: ayrı task.

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
- `Pause/PauseMenuPanel.cs`, `RadialMenuSelection.cs` (radyal menü), `PauseBackdrop.cs` (arka plan)
- `UI/Shapes/UiPolygon.cs`, `PolygonMath.cs` (çokgen, mercek, ofset; `PolygonMathTests`)
- `Player/PlayerController.cs`: `Update` başındaki `BlocksGameplay` kontrolü
- `Multiplayer/MultiplayerSession.cs`: `Block`/`Unblock`
- `Pause/PauseAudioSettingsSO.cs` + `Audio/SO/PauseAudioSettings.asset`, `Audio/SO/MainMixer.mixer`
- `Audio/AudioManager.cs`: müzik kaynaklarında `ignoreListenerPause`
- `Input/PlayerControls.inputactions`: `Pause` action'ı

## Bilinen kenar durumlar ve açık kararlar

1. **Müzik efekti kulakla ayarlanmadı.** Değerler tahmin; sesi ben duyamadım, yalnızca parametrelerin hedefe gittiği ve geri döndüğü doğrulandı.
2. **Süreye bağlı tween/animator donar.** Pause menüsü UI'ı `SetUpdate(true)` / Unscaled Time kullanmalı (`CollectibleUIController` zaten kullanıyor). `AudioManager` müzik çapraz geçişi `Time.deltaTime` kullandığı için pause anında bir geçiş varsa orada durur.
3. **Hit-stop sırasında Start yutulur** (`IsPaused` true, çok kısa).
4. **Doğrulama:** Play Mode'da sanal gamepad ile Start → `timeScale 0`, ses duruyor, oyuncu sabit; A'ya basmak zıplama tamponuna yazmıyor; Start → devam. Radyal menü: stick, D-pad, LB/RB seçimi (options atlanıyor), A ile Retry (`OnDeath` tetiklendi, süre 1) ve Quit (`MainMenu`, süre 1, ses açık) denendi; B ile devam kodla bağlı ama sanal gamepad ile ayrıca denenmedi. Gerçek gamepad, his ve hareket değerleri playtest edilmedi. Not: editör odakta değilken Input System sanal gamepad'i sıfırlıyor (`backgroundBehavior`), testte geçici değiştirilip geri alındı.
