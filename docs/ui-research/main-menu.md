# Ana menü (yeniden kuruldu)

Tarih: 2026-11-09. Düzen `mashup-ui.html` "ana menü" ekranının 4 katı (480x270 → 1920x1080). Ekran görüntüsü: `main-menu/menu_02.png` (Play Mode, 1280x720).

## Parçalar
- `MainMenuBuilder` (menü: "Rock The Game/Ana menüyü yeniden kur"): sahneyi temadan kurar. Çerçeve ve gölge kurulum anında okunur; tema/denge değişince yeniden kur.
- `UiExtrudedText`: başlık ekstrüzyonu (ink kopyalar, derinlik dengeden, Play'de oluşur).
- `UiSelectionVisual`: odak gelince sarı levha + halka imleci.
- Delik: gökyüzü diski, 7 bantlı kanca halkası, ink çerçeve, mavi boyalı halka (`UiArc` + `UiConcreteSurface` boya).

## Kararlar / bilinenler
1. Mockup'taki üçgen imleç yerine halka (task metni).
2. `options` etkileşimsiz ve soluk: ayar ekranı yok. Gezinmede atlanıyor (Multiplayer → aşağı = Quit doğrulandı).
3. Etiketler İngilizce (mockup), lobi Türkçe kalıyor.
4. `UiConcreteSurface` bileşeni eklenirken `OnValidate` rengi beton tonuna çeker; boya rengi bileşenden SONRA verilmeli (builder öyle yapıyor).
5. Doğrulanmadı: gerçek gamepad, 1080p'de küçük detay, Tek kişilik/Çok oyunculu/Çıkış tıklamaları (kod değişmedi, yalnızca görünüm).
