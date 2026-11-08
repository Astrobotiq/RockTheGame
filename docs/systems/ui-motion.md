# UI hareket kuralları: UI hiçbir zaman donuk olmaz

Son kod doğrulaması: 2026-11-09 (ana menü). Kaynak araştırma: [`../ui-research/menu-juice.md`](../ui-research/menu-juice.md). Görsel yön: [`../ui-research/mashup-ui.html`](../ui-research/mashup-ui.html) (Bauhaus + brutalist beton).

**Kural:** Bu projede yeni ya da değiştirilen her UI ekranı aşağıdaki kontrol listesini geçmeden "bitti" sayılmaz. Statik, yalnızca renk değiştiren bir seçim vurgusu eksik iştir. (Gerekçe: ana menü ilk hâlinde "donuktu", seçimde yalnızca dikdörtgen yer değiştiriyordu.)

## Kontrol listesi (her ekran için)

1. **Seçim yer değiştirmez, hareket eder.** İmleç/levha hedefe kayar (gerilir), anında zıplamaz.
2. **Seçilen öğe tepki verir:** büyür (~1,08x), hafif kayar (~28 birim), diğerleri soluklaşır (~%70). Sadece renk değişimi yetmez.
3. **Sahne öğeleri seçime cevap verir:** şekiller, başlık, çizgiler her seçenek için *sabit bir pozda* yerleşir. Aynı seçenek her seferinde aynı pozu alır; rastgele titreme (jiggle) yok.
4. **Sert gölge "çakılır":** seçim değişince gölge ofseti kısa süre büyür, sonra oturur.
5. **Giriş animasyonu var, ≤ ~0,6 sn:** parçalar sırayla yerine gelir; bu sürede gezinme kilitli.
6. **Boşta sürekli ama hafif hareket:** en az bir öğe (halka nefesi, derinlik sürüklenmesi). Abartı yok: okunabilirlik ve hızlı gezinme önce gelir.
7. **Çıkış/geçiş de hareketli** (en azından kısa bir kapanış), ani kesilme değil.
8. **Okunabilirlik testi:** hareket halindeyken metin okunuyor mu? Okunmuyorsa hareketi azalt (Persona'nın dersi).

## Neyi kullanırız, neyi kullanmayız

Kullan: yay (spring) hareketi, poz sistemi, kayan imleç, gölge çakılması, dalga/stagger, açık halka imleç, geometrik şekillerin boyut değişimi, ekstrüde yazı.
Kullanma: aşırı eğim/kolaj (Persona'nın punk dili, bizimki geometrik), su/akışkanlık (betonla çelişir), uzun geçişler, sürekli hareketli geniş arka plan (platformer menüsü hızlı gezilmeli), kanca ipi (denendi, iptal edildi).

## Hazır parçalar (önce bunları kullan, yenisini yazma)

| Parça | Ne yapar |
|---|---|
| `UiSpring` | Hedefe aşarak yerleşen yay. Her an yeniden hedeflenebilir (DOKill yok), hep aynı noktada durur. Hareketin temeli. |
| `UiMenuMotion` | Menü hareketi: poz, kayan imleç, etiket, delik, giriş+kilit. Yeni menü ekranı için bunu kopyala/genelleştir; sıfırdan yazma. |
| `UiExtrudedText` | Ekstrüde yazı (derinlik dengeden). |
| `UiArc` | Dinamik daire/halka/yay (boyut değişimi `localScale` ile, mesh yeniden üretilmez). |
| `UiConcreteSurface` | Beton/boya yüzey. |
| `UiThemeSO` | Palet, font, denge, çerçeve/gölge. Renk/font elle yazılmaz. |

## Uygulama kuralları ve tuzaklar

- **Unscaled zaman:** menü ve pause hareketi `Time.unscaledDeltaTime` kullanır (`timeScale = 0` iken çalışmalı). DOTween kullanılırsa `SetUpdate(true)` + her yeniden başlatmada `DOKill`. Yay bu sorunların ikisini de yaşatmaz; tercih yay.
- **Mesh yeniden üretme:** animasyon için `transform` (ölçek/konum/açı) oynat, `UiArc` alanlarını her kare değiştirme (`SetVerticesDirty` Canvas'ı kirletir). Sürekli animasyonlu öğeler gerekirse ayrı alt Canvas'a.
- **Tween süresi < gezinme tekrar hızı:** imleç geride kalmasın (`moveRepeatRate` ile birlikte ayarla).
- **İlk seçim:** `OnSelect` ilk seçimde tetiklenmeyebilir; başlangıç pozunu kodla kur (`GoTo(0, snap: true)`).
- **Boya rengi:** `UiConcreteSurface` eklenirken `OnValidate` rengi beton tonuna çeker. Boyalı yüzeyde rengi bileşenden SONRA ver.
- **Pozlar veridir:** miktarlar Inspector'da (`Pose[]`); sayıları kodun içine gömme.
- **Overlay canvas CRT dışındadır** (bkz. `../ui-research/crt-check.md`); piksel-keskin kalır.
- **Gamepad-only:** fare hover'ı için hareket yazma.

## Başlangıç değerleri (hepsi gamepad ile ayarlanır; kaynaklarda ölçü yok)

Seçim geçişi ~0,15 sn, hafif aşımlı (yay sertlik ~320, sönüm ~20). Giriş toplam ≤ 0,6 sn, öğe başına ~0,06 sn gecikme. Boşta nefes ~%3 ölçek, sürüklenme ~9 birim. Etiket büyüme 0,08, kayma 28, soluk 0,7.

## Doğrulama

Hareket gözle ve gamepad ile değerlendirilir; ajan yalnızca değerleri (hedef pozlara yerleşme) ve ekran görüntüsünü doğrular. `capture_game_view` eski kare verebilir (md5 karşılaştır). His kararı insanındır (bkz. CLAUDE.md kural 4).

## Ekran durumu

| Ekran | Hareket |
|---|---|
| Ana menü | Tamam (poz, imleç, etiket, delik, giriş). Tık sesi ve titreşim yok. |
| Pause (radyal) | Yapılacak: seçili dilim dışarı çıkar + gölge çakılır, dilimler arası geçiş, açılış/kapanış. |
| HUD sayaç / stamina halkası | Yapılacak: değer değişince kısa tepki; stamina parçası kopunca fırlar. |
| Oda başlık kartı | Yapılacak: levha eğik girer, oda numarası çakılır. |
| Çok oyunculu sonuç | Yapılacak: sütunlar yükselir, sıra vurgusu. |
