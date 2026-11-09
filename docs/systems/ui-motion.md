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

## Önce shader'ı düşün

Yüzey etkisi (desen, örtü, şerit, kayan doku, benek) **shader'da** yapılır; doku üretip `uvRect` oynatmak ya da çok kopya nesneyle taklit etmek değil. Gerekçe: shader mesh'i ve canvas'ı kirletmez (CPU her kare en çok bir malzeme vektörü yazar), ekran çözünürlüğünden bağımsız çizer, doku/nesne yükü yok. Kalıp: UI shader'ı `Assets/Shader/`, konumu mesh köşesinden alır (kök canvas uzayı, `_ArtPixel`), malzeme `UiThemeSO`'da tutulur (build'e girsin diye), bileşen malzemenin örneğini alıp özelliklerini yazar. Örnekler: `UiConcrete` (beton benek/kalıp), `UiDither` (pause karartma). Shader olmayan işler: konum/ölçek/dönüş animasyonu (`UiSpring` + transform), yerleşim. Aday listesi ve durum: aşağıda "Shader adayları".

## Shader adayları (2026-11-09 denetimi)

| Aday | Şu an | Neden shader | Durum |
|---|---|---|---|
| Pause karartma/dither | `UiDither` shader | Doku + `uvRect` her adımda canvas'ı kirletiyordu | Yapıldı |
| Tehlike şeridi (oda kartı, MP sonuç bandı) | `UiHazard` shader + `UiHazardStripes` bileşeni | 45° çapraz sarı/ink şerit (16 pikselde 8), piksel basamaklı, her uzunlukta dikişsiz, doku yok; denge < 0,3'te düz sarı. Hiçbir `Graphic`'e eklenebilir (Image, `UiPolygon`) | Yapıldı; henüz hiçbir ekran kullanmıyor. Oda başlık kartı / MP sonuç task'ında `UiHazardStripes` ekle (malzeme `UiThemeSO.HazardMaterial`) |
| Dilim/daire/çokgen: dolgu + çerçeve + sert gölge | Her şekil için 2-3 `UiArc`/`UiPolygon` (pause'da ~26 grafik) | SDF shader'ı tek dörtgende dolgu+çerçeve+gölge çizer, kenar yumuşatma her ölçekte, çerçeve/gölge malzeme parametresi | Öneri, yapılmadı |
| Dilim/etiket renk geçişi (gri→sarı) | `Graphic.color` her karede değişiyor, mesh/TMP köşe rengi yeniden üretiliyor | Renk malzeme parametresi olursa mesh değişmez | SDF shader işiyle birlikte |
| Ekstrüde yazı | `UiExtrudedText`: derinlik kadar (7) TMP kopyası | Özel TMP SDF shader'ı çok örnekli ekstrüzyonu tek nesnede yapar | Öneri, yapılmadı (menü statik, maliyet küçük) |

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
| Pause (radyal) | Dilimler: seçili dilim sarıya boyanır, dışarı çıkar, gölge çakılır; açılış (sıralı) ve kapanış (solma) var (`PauseMenuPanel`, bkz. `pause.md`). Arka plan (`PauseBackdrop`): daireler/üçgen dilime göre sabit pozda, boşta sürüklenir, girişte kayarak gelir. Eksik: merkez disk, tık sesi/titreşim, dilimlerin kendi boşta hareketi. |
| HUD sayaç / stamina halkası | Yapılacak: değer değişince kısa tepki; stamina parçası kopunca fırlar. |
| Oda başlık kartı | Yapılacak: levha eğik girer, oda numarası çakılır. |
| Çok oyunculu sonuç | Yapılacak: sütunlar yükselir, sıra vurgusu. |
