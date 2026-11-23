# Çok oyunculuda pause ve yarıştan çıkış

*Durum: plan, kod yok (2026-11-24). Görevler `TASKS.md` → "Çok oyunculu: pause ve yarıştan çıkış" (MPP1–MPP3). Bağlı dokümanlar: `systems/pause.md`, `systems/multiplayer.md`.*

## Amaç

Çok oyunculu yarışta da Start ile pause menüsü açılsın; oyuncu menüden oyundan çıkabilsin. Bugün `MultiplayerSession` `PauseManager.Block(...)` çağırdığı için pause kapalı.

## Kararlar

1. **Dünya yerelde durur** (`timeScale 0`), tek oyunculuyla aynı. Adil: yarış süresi `Time.unscaledDeltaTime` ile sayılıyor (`MultiplayerSession.TickRace`), pause'da da akar. Paket gönderimi (`LocalPlayerBroadcaster`) ve 3 sn sessizlik kontrolü (`ConnectionWatch`) gerçek zamanlı, rakip seni koptu saymaz.
2. **Retry çok oyunculuda kapalı** (öneri; kullanıcı onayı bekliyor): ölüm sayılır ve son checkpoint'e atar, yarışta kaçış yolu olabilir. **Options** zaten pasif.
3. **Quit lobiye döner, ana menüye değil.** Rakibe anında haber verilir; rakip 30 sn "koptu, bekleniyor" beklemez.

## Değişecek yerler

- `Pause/PauseManager.cs`, `PauseRules.cs`: `Block` mantığı. Dikkat: `IsPauseBlocked` `SwingTutorial` tarafından "çok oyunculu açık mı" sinyali olarak kullanılıyor, bu anlam korunmalı (tutorial çok oyunculuda çalışmamalı).
- `Pause/PauseMenuPanel.cs`: çok oyunculuda Retry/Options dilimi `Enabled = false`; Quit eylemi kancadan geçer.
- `Multiplayer/Messages/` (`RaceMessages`, `RaceMessageCodec`, `RaceProtocol.Version` → 5): `LeaveMessage`.
- `Multiplayer/MultiplayerSession.cs`: `LeaveMessage` gönder, al; alınca `BeginReconnectWait` yerine doğrudan lobi (`LastExitReason = "Rakip yarıştan ayrıldı"`). `GiveUp` mesajı: yarış kaydı kapalıyken "yarış kaydedildi, Devam et ile sürdürebilirsiniz" yanlış, kayıt durumuna göre düzelt.
- Runtime assembly `MultiplayerSession`'ı görmediği için `PauseManager`'a statik bir `QuitOverride` kancası (`Action`) eklenir; `MultiplayerSession` oturum açılınca kaydeder, kapanınca siler.

## Riskler

- Hayaletin görünümü `Time.deltaTime` kullanıyor (`GhostView`, `GhostOutlineView`); pause'da donar, devamda sıçrayabilir. Konum enterpolasyonu gerçek zamanlıysa sorun yok; MPP3'te iki bilgisayarda gözle bak.
- Pause'dayken rakibin cezası gelirse `PenaltyHandler` oyuncu meşgulken bekletiyor; pause `PlayerController.IsPaused` değil, ceza uygulanması pause bitene kadar beklemeli mi bakılacak (MPP1'de doğrula).
- Kayıt sistemi kapalı (`RaceSaveStore.Enabled = false`), Quit sonrası yarış sürdürülemez; bu bilinçli.

## Testler

- EditMode: `PauseRules` (çok oyunculu pause'a izin), `LeaveMessage` codec gidiş-dönüş, protokol sürümü.
- Elle: MPP3.
