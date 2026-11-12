# Options menüsü (plan, 2026-11-12)

*Tasarım: `options-v1.html` (A: liste + çubuk, B: mikser düğmeleri). Uygulama Sonnet ile, TASKS.md'deki "Options" task'larından. Seçim yapılmadan uygulama task'ı başlamaz.*

## Kapsam

Ana menüdeki `options` satırından ve pause'daki options diliminden açılan tek ekran. Üç sekme, LB/RB ile:

| Sekme | Ayarlar | Kodda durum | Yapılacak |
|---|---|---|---|
| ses (daire) | master, music, sfx, ambient, 0–100 | `AudioManager.SetMasterVolume/SetSFXVolume/SetMusicVolume/SetAmbientVolume` var, `MainMixer`'da exposed, PlayerPrefs'e (`Audio_*Volume`) yazıyor | Yalnızca UI bağlanır. Değer değişince o kanaldan kısa örnek ses (`AudioCuePlayEventChannelSO` ile, yeni kanal açma) |
| titreşim (kare) | açık/kapalı, güç 0–100 | `HapticManager`'da genel çarpan yok | `HapticManager`'a `Strength` (0–1) + `Enabled`; `SetMotorSpeeds` içinde çarp; PlayerPrefs `Haptics_Strength`, `Haptics_Enabled`. Değer değişince 0,15 sn örnek titreşim |
| görüntü (üçgen) | pencere modu (fullscreen / windowed), çözünürlük, CRT açık/kapalı, CRT gücü | Unity `Screen` API; CRT `Assets/Scripts/CRTEffect/CrtFilterVolume.cs` (Volume) | CRT ayarını okuyup Volume ağırlığına uygulayan küçük bir bileşen; PlayerPrefs `Display_*`. Çözünürlük değişince 10 sn "korunsun mu?" onayı, yoksa geri al |

**Ekranda olmayanlar:** yardım modu (GDD §12, karar yok), dil (UR-12, karar yok), tuş atama (oyun yalnızca gamepad).

## Kurallar

- **Veri:** PlayerPrefs anahtarları yukarıda. Yeni bir `SettingsSO` gerekirse varsayılan değerleri orada tut (SO-per-system deseni); oyun açılışında okunup uygulanmalı (ana menü sahnesine girmeden NewScene'den başlansa bile).
- **Girdi:** LB/RB sekme, yukarı/aşağı satır (B'de sol/sağ düğme), sol/sağ ya da stick değer (basılı tutunca hızlanır), B geri (değerler anında kaydedilir, ayrı "kaydet" yok). Kendi `InputAction`'ları, `PauseMenuPanel.BuildInputs` gibi.
- **Pause'dan açılınca:** süre durmuş; tüm hareket unscaled. B ile pause halkasına dönülür (oyuna değil). `PauseMenuPanel`'de options dilimi `Enabled = true` olur ve paneli açar.
- **Ana menüden açılınca:** `MainMenuBuilder`'da options `interactable = true`, menü listesi kapanır, options açılır; B ile listeye dönülür.
- **Hareket:** `docs/systems/ui-motion.md` listesi. `UiSpring` / `UiMenuMotion` yeniden kullanılır; çubuk ve düğme değerleri yayla gider; sekme değişince satırlar kayarak gelir. Değerler başlangıç önerisi, gamepad ile ayarlanır.
- **Ölçek:** UR-3 kararına göre (`UiPixelScale`).
- **Dil:** etiketler UR-12 kararına göre; tasarımda İngilizce küçük harf.

## Bitti sayılır

- EditMode yeşil; ayar mantığı (değer sınırları, PlayerPrefs okuma/yazma, titreşim çarpanı) için EditMode testi.
- Play Mode'da: ses kanalları mixer'da değişiyor, oyunu kapatıp açınca değerler geri geliyor, titreşim kapalıyken `Vibrate` motor çalıştırmıyor, CRT kapalıyken efekt yok.
- Gamepad testi (ayrı task): ana menüden ve pause'dan aç-kapa, sekmeler, basılı tutma hızı, okunurluk.
- `docs/systems/`'te yeni `options.md` (mekanik başına bir dosya) ve `pause.md`'de options diliminin güncellenmesi.
