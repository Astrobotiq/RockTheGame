# Oda başlık kartı

*Son kod doğrulaması: 2026-11-12. Görsel kaynak: `docs/ui-research/mashup-ui.html` `uiCard`. Hareket kuralları: [`ui-motion.md`](ui-motion.md).*

## Tasarım niyeti

Oyuna girince (oda geçişlerinde değil) ekranın solundan eğik bir beton levha, levhadan taşan sarı kiriş, ekstrüde kırmızı oda numarası, kırpılmış mavi disk ve tehlike şeridi gelir; ~1 sn kalıp geri çekilir. **Oyunu durdurmaz** (girişi kilitlemez, `timeScale` dokunmaz), bu yüzden kart küçük tutulur (ekranın yarı yüksekliği) ve ekranın solunda durur.

## Nasıl çalışır

- `RoomTitleCard` (`New-Scripts/UI/Hud/`): kod ile kurulan Overlay canvas (sorting 110: sayaç 100'ün üstünde, pause 200'ün altında), `UiPixelScale` ile tam sayı sanat pikseli, CRT dışı. Geometri `RoomCardMath` (saf, EditMode testli): numara biçimi, kiriş eğimi, levhadan taşan diskin kırpılması.
- **Tetik: oyuna giriş, bir kez.** Oyun sahnesi başlayınca `Start`'ta `entryWaitFrames` (4) kare beklenir, sonra oyuncunun içinde durduğu oda gösterilir. Bekleme şart: `LevelBootstrapper.Start` oyuncuyu başlangıç odasına koyar, kayıtlı yarışa devamda `MultiplayerSession` oyuncuyu 2 kare sonra kayıtlı odaya taşır (`RoomPlacement`); kart taşınmadan sonraki odayı gösterir (geri sayım sırasında, oyuncu donukken). Oda geçişlerinde ve ölüp doğunca kart **çıkmaz**. İleride tek kişilik kayıt yüklenince yükleme kodu `RoomTitleCard.ShowRoom(room)` çağırır (tek kişilik kayıt sistemi henüz yok; mevcut tek kayıt yarış kaydı).
- **Veri:** numara `Room.RoomId` (iki hane: `03`, `12`); `RoomId ≤ 0` ise kart gösterilmez. Kirişteki başlık yeni `Room.RoomTitle` alanından gelir (Inspector, "Room" bileşeni); boşsa kirişte yazı olmaz. Sahnede şu an hiçbir odanın başlığı doldurulmamış.
- **Bileşenler (yeniden kullanılanlar):** `UiPolygon` (üç katman: gölge, çerçeve, dolgu; `PolygonMath.OffsetConvex`), `UiConcreteSurface` (levha beton, disk/kiriş boya), `UiExtrudedText` (numara), `UiHazardStripes` (şerit, `UiThemeSO.HazardMaterial`), `UiThemeSO` (palet, font, çerçeve, gölge; ekran `UiScreen.RoomCard`).
- **Hareket:** `UiSpring`, unscaled zaman. Levha, kiriş ve şerit soldan sırayla kayar (gecikme 0 / 0,08 / 0,26 sn, sertlik 240, sönüm 22: hafif aşım); numara 1,7 katından çakılır (sertlik 320, sönüm 16, 0,2 sn gecikme); disk levhanın sol-alt köşesinden birlikte gelir. Çıkışta hepsi aynı anda sola çekilir.
- DOTween kullanılmaz: yay unscaled zamanda yeniden hedeflenebilir (ui-motion.md "Uygulama kuralları").

## Tunable'lar (Inspector, başlangıç önerisi, gamepad ile ayarlanır)

| Alan | Değer | Anlam |
|---|---|---|
| `startDelay` | 0 sn | Karttan önceki ek bekleme. **İris geçişiyle çakışma için bunu ayarla** (`RoomTransitionCoordinator`; ayrı #test task'ı) |
| `holdSeconds` | 0,45 sn | Oturduktan sonra kalma. Toplam ≈ 0,35 + 0,45 + ~0,3 çıkış ≈ 1,1 sn |
| `beamDelay` / `numberDelay` / `hazardDelay` | 0,08 / 0,2 / 0,26 sn | Giriş sırası |
| `numberSlamScale` | 1,7 | Numaranın çakılma öncesi ölçeği |
| `cardScale` | 0,5 | Mockup kartının ölçeği (1 = tam ekran yüksekliği) |

## Bağlama

`RoomTitleCard` bir sahne nesnesine eklenir (örn. `HudCounterPanel`'in olduğu nesne); başka referans gerekmez. `NewScene`'de `HudCounter` nesnesine eklendi. Başka sahneye eklemek için:

```
unity command eval 'var hud=UnityEngine.Object.FindFirstObjectByType<New_Scripts.UI.Hud.HudCounterPanel>(); hud.gameObject.AddComponent<New_Scripts.UI.Hud.RoomTitleCard>(); return 1;'
```

## Bilinen eksikler / kenar durumlar

- Mockup'taki kırık çizgi (denge > 0,35'te levhada çatlak) yapılmadı.
- Tık/çarpma sesi ve titreşim yok (HUD'daki gibi).
- Oyuna girişte iris/geri sayım ile çakışma #test task'ında `startDelay` ile ayarlanacak.
- Doğrulama: EditMode `RoomCardMathTests`; Play Mode'da oyuna girişte kartın kendiliğinden çıktığı, süre sonunda kapandığı ve oda geçişinde (`OnRoomEntered`) tekrar çıkmadığı doğrulandı; ekran görüntüsü (`docs/ui-research/room-card-play.png`). His (süre, eğim, yay) insanın kararı.
