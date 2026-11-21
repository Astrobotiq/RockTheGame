# C — Renk araştırması: çiçek node'u paleti

Tarih: 2026-11-20. Kapsam: yalnızca araştırma ve ölçüm; kod, shader, sahne değişmedi. Hesap betikleri proje dışında (scratchpad): `colorlib.js`, `palettes.js`, `measure.js`, `build-md.js`, `build-html.js`. Görsel önizleme: `C-renk-onizleme.html` (aynı klasör).

## 0. Kısa sonuç

- Mevcut pembe çiçek üç ayrı eksende karakterle aynı bölgede: hue (tüm roller karakterin ±19° OKLCH hue kamasında), değer (Shadow L* 60.7 vs karakter L* 63.7, fark 3.0) ve renk körlüğü (tritanopide Shadow-karakter ΔE00 5.0).
- Daha derin neden: dünyanın dört kimlik rengi (gökyüzü, karakter, zemin, asit) hepsi L* 64–73 şeridinde. Mevcut çiçek gövdesi de (Base L* 75.9, Shadow L* 60.7) tam bu şeritte; yani yalnızca hue ayırıyor, değer ayırmıyor. Çözüm hue'yu değiştirmek kadar, çiçeği bu değer şeridinin dışına (L* >= ~82 veya L* <= ~50) taşımak.
- Önerilen sıra: 1) P6 Mürekkep Neon, 2) P3 Porselen Kâğıt, 3) P1 İznik Kobalt (gerekçeler bölüm 5'te).

## 1. Ölçüm: mevcut çiçek neden iç içe geçiyor

### 1.1 Dünyanın değer şeridi

Aynı hesap betiğiyle (sRGB -> CIELAB, D65) ölçülen L* değerleri. Sky/CRT satırı proje notundaki gözlenen CRT değeri.

| Öğe | Hex | L* | OKLCH (L C h) |
|---|---|---|---|
| Karakter | `#FF51DE` | 63.7 | 0.72 0.25 336° |
| Karakter gölge | `#C73FAD` | 50.4 | 0.60 0.21 336° |
| Karakter koyu | `#97338A` | 39.2 | 0.50 0.17 333° |
| Gökyüzü | `#71BAEA` | 72.6 | 0.76 0.10 239° |
| Gökyüzü (CRT sonrası, gözlenen) | `#84D2F6` | 80.5 | 0.83 0.09 229° |
| Zemin turuncu | `#E68D41` | 66.7 | 0.72 0.14 58° |
| Asit yeşili | `#28D028` | 73.4 | 0.75 0.24 143° |
| Fırlatma pedi | `#FC1E00` | 53.6 | 0.63 0.25 31° |
| Zemin kenarı (gözle) | `#E040C0` | 55.4 | 0.65 0.23 337° |
| Koyu boru #442434 | `#442434` | 19.0 | 0.31 0.05 349° |
| Halka krem | `#F1E6C7` | 91.4 | 0.93 0.04 91° |
| Halka teal | `#289AA4` | 58.3 | 0.63 0.10 204° |
| Halka koyu mavi | `#362D70` | 23.1 | 0.35 0.11 285° |
| Halka çekirdek sarı | `#F0D420` | 84.9 | 0.87 0.17 99° |

Okuma: karakter 63.7, zemin 66.7, gökyüzü 72.6, asit 73.4. Dört kimlik rengi 10 L* birimlik bir şeritte; sadece hue ile ayrışıyorlar. Fırlatma pedi (53.6) bu şeridin biraz altında. Bu şeritte duran her yeni renk, gri tonlamada ve renk körlüğünde dünyanın dört öğesinden birine karışır.

Dünya düzeyi ek bulgu (çiçekten bağımsız): deuteranopi simülasyonunda asit yeşili ile turuncu zemin ΔE00 1.8 (neredeyse aynı), asit ile fırlatma pedi ΔE00 9.6. Tehlike sinyali olan asit, deutan oyuncuda zemin gibi görünebilir; asidin şekil/animasyon gibi ikinci bir kanalı olmalı (XAG 103 mantığı, bölüm 2).

### 1.2 Mevcut çiçeğin rolleri, karaktere ve zemine göre

| Rol | Hex | L* | ΔE00 karakter | ΔE00 karakter gölge/koyu (min) | OKLCH hue farkı | CR gökyüzü | CR zemin | ΔE00 karakter, trit simülasyonu |
|---|---|---|---|---|---|---|---|---|
| Hi | `#FFE0EC` | 92.0 | 30.8 | 30.8 | 16° | 1.73 | 2.08 | 28.8 |
| Tip | `#FFC4DA` | 84.9 | 24.7 | 24.7 | 18° | 1.43 | 1.72 | 21.5 |
| Base | `#FF9EC4` | 75.9 | 17.4 | 17.4 | 19° | 1.10 | 1.33 | 12.4 |
| Shadow | `#E06AA6` | 60.7 | 10.8 | 10.8 | 14° | 1.46 | 1.22 | 5.0 |
| Deep | `#C24C8C` | 50.1 | 16.0 | 7.5 | 13° | 2.11 | 1.75 | 12.1 |
| Outline | `#9E2F6E` | 38.6 | 27.1 | 7.2 | 13° | 3.21 | 2.67 | 24.1 |
| Glow | `#FFC9A3` | 84.8 | 43.7 | 43.7 | 80° | 1.43 | 1.71 | 21.7 |
| Warn | `#FF9E61` | 73.9 | 44.7 | 44.7 | 75° | 1.04 | 1.25 | 12.1 |
| Core | `#FFF157` | 93.8 | 78.5 | 78.5 | 128° | 1.81 | 2.18 | 33.1 |

Karakter: OKLCH 0.72 0.25 336.48 (L C h), HSV hue 311°, L* 63.7, bağıl parlaklık 0.32. Çiçek Base: OKLCH 0.81 0.12 355.18.

### 1.3 Neden karışıyor, sayılarla

1. **Hue kaması.** Karakter OKLCH hue 336°. Çiçeğin Base/Shadow/Deep/Outline rolleri 350°–355° arasında; fark 13–19°. Hue farkı 20°'nin altı aynı renk ailesi sayılır (çalışma kuralı, kaynak açmadım); çiçek karakterin "açık tonu" gibi okunuyor. Albers'in gösterdiği gibi (bölüm 2) bir rengin görünümü komşusuna göre değişir; bundan çıkarım (ölçüm değil): aynı hue'nun açık/koyu tonları yan yana gelince ayrı nesne değil, aynı nesnenin tonları olarak algılanır.
2. **Gölge katmanları çakışıyor.** Çiçeğin Shadow rolü karakterin kendisine ΔE00 10.8, Deep/Outline rolleri karakterin gölge/koyu tonlarına ΔE00 7.5 ve 7.2. ΔE00 < ~10 yan yana zor ayırt edilir (çalışma eşiği; JND yaklaşık 2.3 civarı, ben kaynak açmadım). Oyuncu çiçeğe tutunduğunda iki nesnenin gölge katmanları üst üste biniyor; bu iç içe geçmenin ölçülmüş karşılığı.
3. **Değer aynı.** Shadow L* 60.7 (karakter 63.7, fark 3.0); gri tonlamada çiçeğin gölge katmanı karakterle özdeş, Deep fark 13.6. Game Developer'ın görsel karışıklık yazısı (bölüm 2) tam bunu söylüyor: karakter ile çevresi aynı değerdeyse silüet kaybolur, düzeltme değer katmanını ayırmak.
4. **Renk körlüğü.** Tritanopi simülasyonunda Shadow-karakter ΔE00 5.0; tüm roller içinde en kötü durum 5.0. Pembe-magenta ailesi içinde renk körlüğü farkı hiç açmıyor.
5. **Gökyüzü kontrastı çiçek gövdesinde yok.** Base gökyüzüne 1.10:1, Tip 1.43:1, Shadow 1.46:1. Yalnızca Outline 3.21:1 ile XAG 102'nin 3:1 eşiğini geçiyor; yani çiçek silüeti ince dış çizgiye bağlı. Gökyüzüne yaslanan en yaygın durumda gövde neredeyse görünmez.
6. **Zemin kenarındaki pembe şerit.** Şerit (#E040C0, gözle) karaktere ΔE00 7.5, çiçek Shadow'a 10.4, Deep'e 10.1. Çiçek, karakter ve zemin kenarı tek ailede; oyuncu bu aileyi "bana ait / güvenli yüzey" diye öğreniyor ve çiçek bu etiketi yanlış devralıyor. (Bu son cümle çıkarım, ölçüm değil.)
7. **Uyarı tonu zeminde kayboluyor.** Warn #FF9E61 turuncu zemine ΔE00 6.9, kontrast 1.25:1; gökyüzüne 1.04:1. Yani "turuncuya kayan titreşim" yalnızca bir hue değişimi ve hue, bu zemin ve gökyüzü karşısında değer kontrastı üretmiyor. Ayrıca Warn karakterin turuncusuna (#FF6736) ΔE00 13.3, halka sarısına 16.3.
8. **CRT filtresi sorunu çözmüyor.** Kaba CRT modeliyle (koyu kanal +≤35) Base-karakter ΔE00 16.9 (filtresiz 17.4); filtre ikisini birlikte kaydırıyor, ilişki değişmiyor.

Özet: çiçek hue olarak karakterden 13–19° uzakta, değer olarak 3–12 L* uzakta, ve gölge katmanları ΔE00 ~7–11. Üçü birden olduğu için ayırma marjı yok.

## 2. Web bulguları

Yalnızca açıp okuduklarım. Her madde kaynağın söylediği ile benim çıkarımımı ayırıyor.

**Kontrast eşikleri ve renk yalnız taşıyıcı olmamalı**
- [Xbox Accessibility Guideline 102: Contrast](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/102): büyük ölçekli görsel öğeler arka plana karşı en az 3:1, standart boyutlu önemli öğeler 4.5:1, yüksek kontrast modunda 7:1. Karakter ve platformları dış çizgiyle (outline) çevirmek ve çizgi renginin tüm arka planlarda kontrast vermesi öneriliyor. Ölçüm araçları olarak Colour Contrast Analyser ve Color Oracle anılıyor. Çıkarım: çiçeğin dış çizgisi gökyüzü ve zemin karşısında en az 3:1 olmalı; bu çalışmada "en iyi (Base veya Outline)" sütununun nedeni bu.
- [Xbox Accessibility Guideline 103: Additional channels for visual and audio cues](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/103): renk tek başına bilgi taşımamalı; renkle birlikte şekil, desen, ikon ya da metin gibi ikinci bir işaret verilmeli. Gri yapılan (devre dışı) öğeler için de ikinci yöntem (kilit ikonu, etiket) isteniyor. Renk körlüğü simülasyonlarının gerçek oyuncu testinin yerini tutmadığı, geliştirme sırasında yardımcı araç olduğu belirtiliyor. Çıkarım: kapalı/açık/uyarı durumu renkle birlikte parlaklık, ışık (çekirdek lambası), şekil (tomurcuk vs çiçek) ve hareketle de söylenmeli.
- [Xbox Accessibility Guideline 118: Photosensitivity](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/118): saniyede yaklaşık 3'ten fazla, ekranın yaklaşık %20'sini kaplayan, %10 ve üstü parlaklık değişimi (flaş) sorun; doymuş kırmızı flaş için eşik daha düşük. Çıkarım: uyarı titreşimi saniyede en çok 2-3 döngü, küçük alanlı (tek çiçek) ve kırmızı değil amber olmalı; kırmızıdan kaçınmak burada da haklı.

**Değer kontrastı (açık-koyu) ve ton kontrastı**
- [Reducing Visual Confusion In Your Game (Game Developer)](https://gamedeveloper.com/design/how-to-reduce-visual-confusion-in-your-game): ekranı gri tonlamaya çevirip okunabilirliği sınamayı öneriyor; karakter, düşman ve zemin neredeyse aynı değerdeyse silüet kayboluyor; düzeltme arka planı koyulaştırmak, basılabilir yüzeyleri ve karakteri açmak; "her yerde yüksek kontrast, hiçbir yerde kontrast demek". Makale hue-değer karşılaştırması yapmıyor; sadece oyunun renksiz de okunması gerektiğini savunuyor. Bu çalışmadaki gri tonlama (ΔL*) sütunu bu testin sayısal karşılığı.
- [Color in games: an in-depth look at one of game design's most useful tools (Game Developer)](https://www.gamedeveloper.com/design/color-in-games-an-in-depth-look-at-one-of-game-design-s-most-useful-tools): sahne öğelerinde oyuncu, düşman, etkileşimli nesne, arka plan hiyerarşisi; renk (değer, doygunluk, hue) bu hiyerarşiyi ayırabilir; sıcak renkler soğuğun yanında öne çıkar; renk etkileşimli nesneyi işaretleyebilir (Mirror's Edge'de yararlı nesneler kırmızı); renk körlüğü erkeklerin yaklaşık 12'de 1'inde, ve en yaygın biçim kırmızı-yeşil. Aktardığı örnek: Black Ops ekibi kırmızı-yeşil yerine mavi-turuncuyu alternatif sunmuş.
- Bu çalışmanın kendi ölçümü (kaynak değil): dünyanın dört kimlik rengi L* 64–73 şeridinde, yani bu oyunda hue ayırıyor, değer ayırmıyor.

**Albers / eşzamanlı kontrast**
- [Brown CS, Albers açıklama sayfası](https://cs.brown.edu/courses/cs092/VA10/HTML/AlbersExplanation.html): aynı yeşil kare farklı arka planlarda iki farklı ton gibi görünüyor; renk her zaman çevresiyle ilişki içinde görülüyor. Çıkarım: pembe çiçeğin pembe zemin şeridi ve magenta karakter yanında "aynı ailenin tonu" okunması beklenen bir etki; aynı çiçek rengi turuncu ya da mavi çevrede başka görünür. Bu yüzden palet ölçümü yalnızca hex değil, hex-arka plan çifti olarak yapıldı.

**Sahiplik / rezerve hue**
- Bir oyunda karakterin hue'sunu başka hiçbir şeye vermeme kuralını açıkça yazan kaynak bulamadım (aşağıda Doğrulanamayanlar). Elimdeki dolaylı destek: Game Developer renk makalesi (yukarıda) oyuncunun hiyerarşinin en üstünde olması gerektiğini söylüyor ve [Hollow Knight renk çalışması (C. Moloney, hayran çalışması)](https://charles-moloney.github.io/colorstudy.html) paletin kısıtlı tutulmasının doygun vurguları öne çıkardığını, yazarın denediği alternatif paletlerde Şövalye'nin zıt (tamamlayıcı veya tamamen farklı sıcaklıkta) bir vurguyla öne çıktığını gösteriyor; ama geliştirici açıklaması değil. Bu proje için çıkarım: pembe/magenta karakter hue'su rezerve; gerekirse kırmızı (tehlike) ve yeşil (asit) de rezerve; çiçek bu üçünün ve turuncu zemin ailesinin dışına çıkmalı.
- [Ask the Developer, Kirby and the Forgotten Land (Nintendo)](https://www.nintendo.com/ph/interview/arzg/03.html): yalnızca parlak mavi gökyüzü ve renkli bitki örtüsünün neşeli ton için eklendiğini söylüyor; Kirby'nin pembesinin arka planla ilişkisi konusunda bir şey söylemiyor. Yani "pembe karakter + mavi gökyüzü" ikilisi bu oyunda da var ama gerekçesi kaynakta yok.

**Tehlike renginin tutarlılığı**
- [Designing for Difficulty: Readability in ARPGs (Game Developer)](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs): rengin sabit bir anlamı olmalı (kırmızı parlama her zaman aynı sonucu doğurmalı); renk telegraph kanallarından yalnızca biri, hareket ve ses ile destekleniyor. Çıkarım: kırmızı ve asit yeşili tehlikeye, turuncu/amber uyarıya ayrıldıysa çiçek bu üçünü başka anlamda kullanmamalı; çiçekte "uyarı" için amber seçmek bu tutarlılığa uyuyor, ama amber halkaların mevcut sarısına (#FCAE04) çok yakın (P1 Warn ΔE00 0.6), bu çakışma bilinçli bir seçim olmalı.
- [Hades II Has a Legibility Problem (nex-3.com, bir oyuncunun eleştirisi)](https://nex-3.com/blog/hades-ii-has-a-legibility-problem/): kırmızı telegraph bölgelerinin toz ve yoğun efektler altında kaybolması bir okunabilirlik sorunu olarak anlatılıyor. Geliştirici açıklaması değil, oyuncu görüşü. Çıkarım: sinyal rengi (burada amber uyarı) yoğun efekt altında kaybolabilir; ölçek titreşimi gibi hareket kanalı gerekli.

**Renk körlüğü ve benzetim yöntemi**
- [Machado, Oliveira, Fernandes 2009 (UFRGS sayfası)](https://www.inf.ufrgs.br/~oliveira/pubs_files/CVD_Simulation/CVD_Simulation.html): severity 1.0 için protanomali/deuteranomali/tritanomali matrisleri (bu çalışmada aynen kullanıldı). Sayfa renk uzayını söylemiyor; [colorspace R paketi dokümanı](https://colorspace.r-forge.r-project.org/articles/color_vision_deficiency.html) matrislerin lineer RGB üzerinde uygulanmasını "daha uygun" buluyor ve varsayılan yapıyor; betik de lineer RGB kullanıyor.
- [Oklab (B. Ottosson)](https://bottosson.github.io/posts/oklab/): lightness, chroma ve hue'yu birbirinden olabildiğince bağımsız tutan algısal renk uzayı; mavi-beyaz karışımlarında mora kaymıyor. Betik bu katsayıları kullandı (lineer sRGB -> Oklab) ve OKLCH hue farkları ile uyarı karışımları (OKLab'ta) bununla hesaplandı.
- [WCAG 2.1](https://www.w3.org/TR/WCAG21/): kontrast oranı (L1+0.05)/(L2+0.05); metin dışı öğeler için 3:1. Betik siyah-beyazı 21:1 verdi.

**"Kapalı -> açık -> uyarı" durumunu renkle söyleme: bulgulardan çıkan yöntem**
(Kaynak yalnızca "renk tek başına yetmez, ikinci kanal ekle" ve "rengin anlamı sabit olsun" diyor; geri kalanı bu çalışmanın ölçümlerinden çıkarım.)
1. Kapalı = soluk + koyu + tek ton: doygunluk ve değer düşer, katmanlar (Hi/Tip/Shadow) tek tona çöker. Kapalı hal ayrıca tomurcuk şekli ve sönük çekirdek ile söyleniyor (ikinci kanal).
2. Açık = katman açılır: Hi/Tip ışır, çekirdek lambası yanar, halo yayılır. Yani durum değişimini rengin yanı sıra ışık ve şekil taşıyor.
3. Uyarı = hue'yu tek başına değiştirme: ölçümlerde mavi/turkuaz yapraklara sarı tint karıştırmak gri/yeşilimsi bir ara tona gidiyor (P5 ve P8'de yeşile kaçtı), bu yüzden tint yalnızca ısıtılabilir paletlerde (P3, P7) güçlü, soğuk paletlerde yaprak sabit bırakılıp çekirdek + kenar + parıltı amber'e çevrilmeli.
4. Uyarı için değer ve zaman kanalı: ölçek titreşimi ve kenar halkasında 2 Hz altı yanıp sönme; amber dış çizgi gökyüzüne yalnızca 1.1:1–1.6:1 verdiği için (aşağıdaki tablolarda), uyarı flaşı koyu dış çizgi ile amber arasında değişirse değer kontrastı da zamanla taşınmış olur.
5. Amber, protanopide asit yeşiline çok yakın (Warn-asit protanopi ΔE00 2–4 çoğu palette). Bu yüzden uyarı hue'ya değil zamansal ve şekilsel ipucuna dayanmalı (bölüm 1.1'deki asit-zemin deuteranopi sorunuyla aynı kök).

## 3. Paletler

Her palet için sabit geometri (6 yapraklı, damla yaprak, 2 katman, merkez, dış çizgi) `C-renk-onizleme.html` içinde. Ölçüm eşikleri (İYİ / SINIRDA / ZAYIF) benim çalışma eşiklerim: ΔE00 karakter >= 20 / 10; kontrast >= 3:1 / 2:1 (XAG 102'nin 3:1'i ile uyumlu); asit ΔE00 (renk körlüğü dahil, normal değer yarıya bölünerek) >= 10 / 5; renk körlüğünde karakterden ΔE00 >= 15 / 8; gri ΔL* >= 15 / 8. "Gövde" = Tip, Base, Shadow; çekirdek ayrıca dahil edildi.

Not: Fırlatma pedinin kırmızısı ve asit yeşili için ayrı karışma ölçümü yalnızca asit yeşili için yapıldı; kırmızıya yakınlık yalnızca P2 çekirdeğinde (vermilyon) söz konusu ve metinde anıldı.

### P1 — İznik Kobalt

**Fikir:** İznik çinisi: kobalt gövde, turkuaz-nane uç, açık krem-sarı çekirdek (mercan çekirdek bilerek bırakıldı: kırmızı/turuncu rezerve; parlak sarı #FFD23F protanopide asit yeşiline ΔE00 4.9 verdiği için açık tona çekildi).

| Rol | Hex | L* | Karaktere ΔE00 |
|---|---|---|---|
| Hi (parlama) | `#E2FFFA` | 97.9 | 34.1 |
| Tip (uç) | `#9CF2E8` | 90.0 | 45.1 |
| Base (ana) | `#2358CC` | 40.7 | 37.1 |
| Shadow (gölge) | `#183C96` | 28.5 | 43.3 |
| Deep (damar) | `#102468` | 17.3 | 47.8 |
| Outline (dış çizgi) | `#0A1444` | 8.8 | 51.5 |
| Glow (taban parıltı) | `#7FE8F0` | 86.3 | 48.5 |
| Warn (uyarı tint) | `#FFB000` | 77.5 | 64.1 |
| Merkez | `#FFEE9A` | 93.8 | 66.9 |
| Halo (açılış yayılımı) | `#8FEFE6` | 88.6 | 46.7 |
| Kapalı hal tonu | `#3B4868` | 30.8 | 43.3 |

**Ölçümler**

| Ölçüt | Sonuç | Durum |
|---|---|---|
| Karaktere mesafe (ΔE00, #FF51DE) | Base 37.1; gövde+çekirdek min, karakter gölge/koyu tonları dahil 24.6; hue farkı OKLCH 74° / HSV 90° | İYİ |
| Gökyüzüne kontrast (WCAG, #71BAEA) | Base 2.97:1; en iyi (Base veya Outline) 8.29:1; CRT gökyüzü #84D2F6 ile en iyi 10.47:1 | İYİ |
| Zemine kontrast (WCAG, #E68D41) | Base 2.47:1; en iyi (Base veya Outline) 6.90:1 | İYİ |
| Asit yeşiline (#28D028) karışma, ΔE00 min (Tip/Base/Shadow/Çekirdek) | normal 28.0; protanopi 14.1; deuteranopi 16.3 | İYİ |
| Renk körlüğünde karakterden ayrışma (ΔE00) | Base: prot 12.6 / deut 27.6 / trit 60.2; tüm roller min 12.6 | SINIRDA |
| Gri tonlamada ayrışma (ΔL*) | Base-karakter 23.0; en yakın gövde rolü-karakter 23.0; gökyüzü (en iyi) 63.8; zemin (en iyi) 57.9 | İYİ |

Ek: CRT sonrası (kaba model) Base-karakter ΔE00 35.2; kapalı hal ile açık Base ΔE00 15.0 (ΔL* 9.9); çekirdek açık-kapalı ΔE00 66.8; uyarı yaprak kayması (tint 0.25) açık Base'ten ΔE00 11.7; Warn rengi zemine ΔE00 14.8; Base koyu borulara min ΔE00 28.0.

**Açık / kapalı / uyarı geçişi**

- Kapalı: tüm yapraklar tek koyu-soluk çelik mavisi, çekirdek aynı tona söner (lamba kapalı), halo yok.
- Açık: kobalt Base yanar, nane-turkuaz Tip ışır, krem-sarı çekirdek yanar; halo turkuazdan dışa yayılır.
- Uyarı: yapraklar kobalttan amber yönüne %25 kayar (soğuk -> ılık, doygunluk düşer, değer artar), çekirdek amber, dışta amber kesikli halka yanıp söner.
- Uyarı halinde yaprak Base yaklaşık `#5D77B4` (tint 0.25); Warn tonu `#FFB000`.

**Risk:** Koyu kobalt gölgeler koyu mavi halkayla (#362D70) akraba; protanopide kobalt-magenta ton olarak yaklaşıyor (değerle kurtarılıyor); krem-sarı çekirdek halka çekirdek sarısıyla (#F0D420) akraba (ΔE00 ~12).

**Neden iyi:** Kobalt gökyüzünden ve karakterden değerce belirgin koyu (L* 41 vs 64/73), hue pembeden ~74 derece uzak; açık-kapalı arasında "turkuaz uç + sarı çekirdek yanıyor/sönüyor" net.

### P2 — Aizuri-e Prusya

**Fikir:** Aizuri-e baskı: tek mavi ailesi (prusya) + soluk beyaz-mavi uç, çekirdekte tek vermilyon nokta.

| Rol | Hex | L* | Karaktere ΔE00 |
|---|---|---|---|
| Hi (parlama) | `#F4F8FF` | 97.5 | 35.4 |
| Tip (uç) | `#C3D6F2` | 85.0 | 34.3 |
| Base (ana) | `#2A5CA8` | 39.5 | 41.2 |
| Shadow (gölge) | `#1B3F76` | 27.0 | 46.9 |
| Deep (damar) | `#102850` | 16.5 | 50.8 |
| Outline (dış çizgi) | `#08142C` | 6.6 | 54.7 |
| Glow (taban parıltı) | `#9DBDEB` | 75.8 | 36.1 |
| Warn (uyarı tint) | `#FFB347` | 78.4 | 58.6 |
| Merkez | `#FF4B2B` | 58.5 | 37.3 |
| Halo (açılış yayılımı) | `#D5E3F8` | 89.8 | 34.2 |
| Kapalı hal tonu | `#5C6C86` | 45.2 | 34.8 |

**Ölçümler**

| Ölçüt | Sonuç | Durum |
|---|---|---|
| Karaktere mesafe (ΔE00, #FF51DE) | Base 41.2; gövde+çekirdek min, karakter gölge/koyu tonları dahil 28.0; hue farkı OKLCH 78° / HSV 95° | İYİ |
| Gökyüzüne kontrast (WCAG, #71BAEA) | Base 3.10:1; en iyi (Base veya Outline) 8.65:1; CRT gökyüzü #84D2F6 ile en iyi 10.93:1 | İYİ |
| Zemine kontrast (WCAG, #E68D41) | Base 2.58:1; en iyi (Base veya Outline) 7.19:1 | İYİ |
| Asit yeşiline (#28D028) karışma, ΔE00 min (Tip/Base/Shadow/Çekirdek) | normal 44.5; protanopi 25.2; deuteranopi 6.1 | SINIRDA |
| Renk körlüğünde karakterden ayrışma (ΔE00) | Base: prot 15.4 / deut 29.7 / trit 59.4; tüm roller min 13.9 | SINIRDA |
| Gri tonlamada ayrışma (ΔL*) | Base-karakter 24.2; en yakın gövde rolü-karakter 21.4; gökyüzü (en iyi) 66.0; zemin (en iyi) 60.1 | İYİ |

Ek: CRT sonrası (kaba model) Base-karakter ΔE00 38.1; kapalı hal ile açık Base ΔE00 10.9 (ΔL* 5.7); çekirdek açık-kapalı ΔE00 41.2; uyarı yaprak kayması (tint 0.25) açık Base'ten ΔE00 11.8; Warn rengi zemine ΔE00 12.0; Base koyu borulara min ΔE00 23.8.

**Açık / kapalı / uyarı geçişi**

- Kapalı: tek ton kurşuni mavi, vermilyon çekirdek söner (gri), halo yok.
- Açık: prusya mavisi Base, soluk Tip, vermilyon nokta yanar (tek sıcak lamba).
- Uyarı: yapraklar %25 amber'e kayar, çekirdek vermilyondan amber'e döner (kırmızıyı tehlikeden ayırmak için), amber kesikli halka.
- Uyarı halinde yaprak Base yaklaşık `#62779C` (tint 0.25); Warn tonu `#FFB347`.

**Risk:** Vermilyon çekirdek kırmızı/turuncu rezervini bilerek esniyor: fırlatma pedi kırmızısına yakın ve deuteranopide asit yeşiline yaklaşıyor (red-green karışması).

**Neden iyi:** Monokrom mavi + tek sıcak nokta: en temiz odak; prusya gökyüzünden ve karakterden değerce ayrışır.

### P3 — Porselen Kâğıt

**Fikir:** Kâğıt çiçek: kırık-beyaz fildişi yapraklar, soğuk lavanta-gri gölgeler (sıcak-soğuk hue kaydırma), ortada tek canlı elektrik kobalt çekirdek.

| Rol | Hex | L* | Karaktere ΔE00 |
|---|---|---|---|
| Hi (parlama) | `#FFFFFF` | 100.0 | 38.4 |
| Tip (uç) | `#FAF9F6` | 97.9 | 39.5 |
| Base (ana) | `#F4F1EA` | 95.2 | 40.6 |
| Shadow (gölge) | `#CDD2E4` | 84.3 | 31.5 |
| Deep (damar) | `#5A6088` | 41.7 | 34.4 |
| Outline (dış çizgi) | `#303778` | 25.9 | 43.2 |
| Glow (taban parıltı) | `#FFF3C4` | 95.7 | 57.5 |
| Warn (uyarı tint) | `#FFC94D` | 83.7 | 65.9 |
| Merkez | `#1536E8` | 35.2 | 36.7 |
| Halo (açılış yayılımı) | `#E5E9FF` | 92.6 | 34.1 |
| Kapalı hal tonu | `#6E6F78` | 47.0 | 31.8 |

**Ölçümler**

| Ölçüt | Sonuç | Durum |
|---|---|---|
| Karaktere mesafe (ΔE00, #FF51DE) | Base 40.6; gövde+çekirdek min, karakter gölge/koyu tonları dahil 22.5; hue farkı OKLCH 111° / HSV 91° | İYİ |
| Gökyüzüne kontrast (WCAG, #71BAEA) | Base 1.88:1; en iyi (Base veya Outline) 5.10:1; CRT gökyüzü #84D2F6 ile en iyi 6.45:1 | İYİ |
| Zemine kontrast (WCAG, #E68D41) | Base 2.26:1; en iyi (Base veya Outline) 4.25:1 | İYİ |
| Asit yeşiline (#28D028) karışma, ΔE00 min (Tip/Base/Shadow/Çekirdek) | normal 32.9; protanopi 29.0; deuteranopi 27.8 | İYİ |
| Renk körlüğünde karakterden ayrışma (ΔE00) | Base: prot 39.2 / deut 29.6 / trit 34.4; tüm roller min 15.1 | İYİ |
| Gri tonlamada ayrışma (ΔL*) | Base-karakter 31.5; en yakın gövde rolü-karakter 20.6; gökyüzü (en iyi) 46.7; zemin (en iyi) 40.8 | İYİ |

Ek: CRT sonrası (kaba model) Base-karakter ΔE00 37.9; kapalı hal ile açık Base ΔE00 37.8 (ΔL* 48.2); çekirdek açık-kapalı ΔE00 28.4; uyarı yaprak kayması (tint 0.35) açık Base'ten ΔE00 12.3; Warn rengi zemine ΔE00 19.6; Base koyu borulara min ΔE00 53.4.

**Açık / kapalı / uyarı geçişi**

- Kapalı: yapraklar koyu gri-lavanta tek ton (kâğıt "soldu"), çekirdek söner.
- Açık: kırık-beyaz yapraklar + soğuk lavanta gölge belirir, elektrik kobalt çekirdek yanar, halo lavanta-beyaz.
- Uyarı: yapraklar %35 amber'e kayar (beyazdan krem-sarıya, ılık), çekirdek kobalttan amber'e döner (lamba rengi değişir), amber kesikli halka.
- Uyarı halinde yaprak Base yaklaşık `#F8E4BA` (tint 0.35); Warn tonu `#FFC94D`.

**Risk:** Krem halkaya (#F1E6C7) ve gümüş dikene yakın aile (saf krem yerine soğuk kırık-beyaz seçilerek ΔE açıldı); gökyüzü üstünde gövde düşük kontrastlı, silüeti lacivert dış çizgi taşıyor; uyarıda krem-sarıya kayma ince, ikincil ipucu şart.

**Neden iyi:** Dünyada neredeyse hiç olmayan "kâğıt beyazı" + kobalt çekirdek; renk körlüğü ve gri tonlamada karakterden en güvenli aile; asit yeşilinden uzak.

### P4 — İndigo Altın

**Fikir:** Mor-indigo yapraklar, altın sarı çekirdek ve uyarı: gece çiçeği / bizans mozaiği hissi.

| Rol | Hex | L* | Karaktere ΔE00 |
|---|---|---|---|
| Hi (parlama) | `#EEEBFF` | 93.8 | 33.6 |
| Tip (uç) | `#D4CFFF` | 84.9 | 28.8 |
| Base (ana) | `#4D44D8` | 39.0 | 32.7 |
| Shadow (gölge) | `#3427A4` | 26.3 | 40.5 |
| Deep (damar) | `#241878` | 17.4 | 45.1 |
| Outline (dış çizgi) | `#120C4A` | 8.0 | 50.2 |
| Glow (taban parıltı) | `#A9A4FF` | 71.0 | 24.7 |
| Warn (uyarı tint) | `#FFC83D` | 83.4 | 67.7 |
| Merkez | `#FFE680` | 91.4 | 68.8 |
| Halo (açılış yayılımı) | `#B8B3FF` | 75.9 | 25.8 |
| Kapalı hal tonu | `#3E3D66` | 27.6 | 42.2 |

**Ölçümler**

| Ölçüt | Sonuç | Durum |
|---|---|---|
| Karaktere mesafe (ΔE00, #FF51DE) | Base 32.7; gövde+çekirdek min, karakter gölge/koyu tonları dahil 19.9; hue farkı OKLCH 59° / HSV 68° | SINIRDA |
| Gökyüzüne kontrast (WCAG, #71BAEA) | Base 3.16:1; en iyi (Base veya Outline) 8.42:1; CRT gökyüzü #84D2F6 ile en iyi 10.64:1 | İYİ |
| Zemine kontrast (WCAG, #E68D41) | Base 2.63:1; en iyi (Base veya Outline) 7.01:1 | İYİ |
| Asit yeşiline (#28D028) karışma, ΔE00 min (Tip/Base/Shadow/Çekirdek) | normal 28.9; protanopi 10.9; deuteranopi 14.4 | İYİ |
| Renk körlüğünde karakterden ayrışma (ΔE00) | Base: prot 14.0 / deut 27.8 / trit 58.4; tüm roller min 12.8 | SINIRDA |
| Gri tonlamada ayrışma (ΔL*) | Base-karakter 24.7; en yakın gövde rolü-karakter 21.2; gökyüzü (en iyi) 64.6; zemin (en iyi) 58.7 | İYİ |

Ek: CRT sonrası (kaba model) Base-karakter ΔE00 28.5; kapalı hal ile açık Base ΔE00 18.5 (ΔL* 11.4); çekirdek açık-kapalı ΔE00 73.1; uyarı yaprak kayması (tint 0.25) açık Base'ten ΔE00 14.6; Warn rengi zemine ΔE00 20.3; Base koyu borulara min ΔE00 29.2.

**Açık / kapalı / uyarı geçişi**

- Kapalı: yapraklar koyu soluk indigo-gri, çekirdek söner.
- Açık: parlak indigo Base, açık lavanta Tip, soluk altın çekirdek; halo lavanta.
- Uyarı: yapraklar %25 altın yönüne kayar (indigo gri-mora açılır), çekirdek zaten altın tonunda olduğundan uyarıyı çekirdek yanıp sönmesi + kenardaki amber halka taşır.
- Uyarı halinde yaprak Base yaklaşık `#7071C3` (tint 0.25); Warn tonu `#FFC83D`.

**Risk:** Magenta komşusu hue (karaktere 59 derece; gövde-min 47); soluk altın çekirdek protanopide asit yeşiline ΔE00 ~11 (ilk taslaktaki parlak altın #FFC21A değerinde ~2 idi, açık tona çekildi); indigo gölgeler koyu mavi halkayla akraba.

**Neden iyi:** Soluk altın-indigo tamamlayıcı çift; koyu değer gökyüzünden ve zeminden güçlü ayrışır; açık hal dünyada en "değerli" görünen nesne.

### P5 — Buz Camı

**Fikir:** Cam/buz: neredeyse beyaz şeffaf yapraklar, derin lacivert gölge ve çerçeve, beyaz-sıcak çekirdek.

| Rol | Hex | L* | Karaktere ΔE00 |
|---|---|---|---|
| Hi (parlama) | `#FFFFFF` | 100.0 | 38.4 |
| Tip (uç) | `#F0FBFF` | 97.9 | 28.1 |
| Base (ana) | `#C6EEFF` | 91.9 | 35.8 |
| Shadow (gölge) | `#4B6BC0` | 46.7 | 34.7 |
| Deep (damar) | `#2D3F94` | 30.0 | 41.6 |
| Outline (dış çizgi) | `#121E58` | 14.2 | 49.1 |
| Glow (taban parıltı) | `#D6F4FF` | 94.4 | 31.6 |
| Warn (uyarı tint) | `#FFE08A` | 90.0 | 64.0 |
| Merkez | `#FFFFFF` | 100.0 | 38.4 |
| Halo (açılış yayılımı) | `#DDF6FF` | 95.4 | 30.0 |
| Kapalı hal tonu | `#60708C` | 46.9 | 34.0 |

**Ölçümler**

| Ölçüt | Sonuç | Durum |
|---|---|---|
| Karaktere mesafe (ΔE00, #FF51DE) | Base 35.8; gövde+çekirdek min, karakter gölge/koyu tonları dahil 27.2; hue farkı OKLCH 111° / HSV 113° | İYİ |
| Gökyüzüne kontrast (WCAG, #71BAEA) | Base 1.72:1; en iyi (Base veya Outline) 7.33:1; CRT gökyüzü #84D2F6 ile en iyi 9.27:1 | İYİ |
| Zemine kontrast (WCAG, #E68D41) | Base 2.07:1; en iyi (Base veya Outline) 6.10:1 | İYİ |
| Asit yeşiline (#28D028) karışma, ΔE00 min (Tip/Base/Shadow/Çekirdek) | normal 34.9; protanopi 32.2; deuteranopi 31.4 | İYİ |
| Renk körlüğünde karakterden ayrışma (ΔE00) | Base: prot 30.8 / deut 18.2 / trit 49.4; tüm roller min 8.1 | SINIRDA |
| Gri tonlamada ayrışma (ΔL*) | Base-karakter 28.2; en yakın gövde rolü-karakter 17.0; gökyüzü (en iyi) 58.4; zemin (en iyi) 52.5 | İYİ |

Ek: CRT sonrası (kaba model) Base-karakter ΔE00 31.5; kapalı hal ile açık Base ΔE00 36.7 (ΔL* 44.9); çekirdek açık-kapalı ΔE00 41.4; uyarı yaprak kayması (tint 0) açık Base'ten ΔE00 0.0; Warn rengi zemine ΔE00 24.2; Base koyu borulara min ΔE00 54.0.

**Açık / kapalı / uyarı geçişi**

- Kapalı: yapraklar mat çelik-mavi tek ton (donmuş, bulanık), çekirdek söner.
- Açık: beyaz-camgöbeği şeffaf yapraklar, lacivert gölge/damar, beyaz-sıcak çekirdek, halo buz-mavisi.
- Uyarı: yapraklar sabit kalır (sarı+mavi karışımı yeşile kaçıyor); çekirdek ve dış kenar amber'e döner, taban parıltısı amber, kesikli halka yanıp söner.
- Uyarı halinde yaprak Base yaklaşık `#C6EEFF` (tint 0); Warn tonu `#FFE08A`.

**Risk:** Gökyüzü mavisine en yakın aile: gövde gökyüzü üstünde 1.7:1, yalnız koyu lacivert çizgi/gölge ayırıyor; CRT gökyüzü parlayınca açık gövde daha da sönük; sarı-mavi karışımı yeşile kaydığı için uyarıda yaprak tint yapılmıyor.

**Neden iyi:** Hue olarak pembeye uzak, "soğuk=kapalı, beyaz-parlak=açık" sezgisel; asit yeşilinden hem normal hem renk körlüğünde en uzak aile.

### P6 — Mürekkep Neon

**Fikir:** Koyu mürekkep/obsidyen yapraklar, mavi-mor kenar ışığı, parlayan neon camgöbeği çekirdek.

| Rol | Hex | L* | Karaktere ΔE00 |
|---|---|---|---|
| Hi (parlama) | `#6870C8` | 50.3 | 28.7 |
| Tip (uç) | `#3F4590` | 32.5 | 39.3 |
| Base (ana) | `#242650` | 17.2 | 47.7 |
| Shadow (gölge) | `#171833` | 9.6 | 51.8 |
| Deep (damar) | `#0D0D20` | 4.3 | 55.0 |
| Outline (dış çizgi) | `#04040C` | 1.3 | 57.1 |
| Glow (taban parıltı) | `#2CF5FF` | 88.5 | 60.2 |
| Warn (uyarı tint) | `#FFB020` | 77.5 | 62.5 |
| Merkez | `#2CF5FF` | 88.5 | 60.2 |
| Halo (açılış yayılımı) | `#7CF9FF` | 91.3 | 53.1 |
| Kapalı hal tonu | `#3A3C58` | 26.3 | 43.9 |

**Ölçümler**

| Ölçüt | Sonuç | Durum |
|---|---|---|
| Karaktere mesafe (ΔE00, #FF51DE) | Base 47.7; gövde+çekirdek min, karakter gölge/koyu tonları dahil 20.7; hue farkı OKLCH 58° / HSV 74° | İYİ |
| Gökyüzüne kontrast (WCAG, #71BAEA) | Base 6.75:1; en iyi (Base veya Outline) 9.65:1; CRT gökyüzü #84D2F6 ile en iyi 12.19:1 | İYİ |
| Zemine kontrast (WCAG, #E68D41) | Base 5.62:1; en iyi (Base veya Outline) 8.03:1 | İYİ |
| Asit yeşiline (#28D028) karışma, ΔE00 min (Tip/Base/Shadow/Çekirdek) | normal 36.8; protanopi 41.0; deuteranopi 45.0 | İYİ |
| Renk körlüğünde karakterden ayrışma (ΔE00) | Base: prot 34.8 / deut 46.1 / trit 53.5; tüm roller min 13.4 | SINIRDA |
| Gri tonlamada ayrışma (ΔL*) | Base-karakter 46.5; en yakın gövde rolü-karakter 31.2; gökyüzü (en iyi) 71.3; zemin (en iyi) 65.5 | İYİ |

Ek: CRT sonrası (kaba model) Base-karakter ΔE00 44.0; kapalı hal ile açık Base ΔE00 8.2 (ΔL* 9.1); çekirdek açık-kapalı ΔE00 64.2; uyarı yaprak kayması (tint 0.25) açık Base'ten ΔE00 16.6; Warn rengi zemine ΔE00 13.7; Base koyu borulara min ΔE00 13.1.

**Açık / kapalı / uyarı geçişi**

- Kapalı: koyu ink gri-mor tek ton, neon çekirdek tamamen söner, kenar ışığı yok.
- Açık: yapraklar siyah-lacivert kalır ama mavi-mor kenar ışığı (Hi) belirir, neon camgöbeği çekirdek + halo yanar. Durum farkı yapraktan çok çekirdek/ışıktan gelir.
- Uyarı: yapraklar %25 amber'e kayar (sıcak kömür), neon çekirdek amber'e döner, kenar ışığı amber, kesikli halka yanıp söner.
- Uyarı halinde yaprak Base yaklaşık `#544956` (tint 0.25); Warn tonu `#FFB020`.

**Risk:** Koyu boru renkleri (#140C1C/#442434/#4E4A4E) ve koyu mavi halkayla (#362D70) yakın (ΔE 6.8-13); kapalı-açık yaprak değeri yakın (L* 26 vs 17), durumu çekirdek açık/kapalı taşımak zorunda; çok koyu çiçek "boru parçası" sanılabilir.

**Neden iyi:** Gökyüzü (6.8:1) ve turuncu zemin (5.6:1) üstünde en yüksek kontrast; karakterden gri tonlamada (Base ΔL* 46.5) ve Base renk körlüğü ayrışmasında (ΔE00 >= 34.8) en uzak; sıcak vurgu (neon çekirdek, amber uyarı) tek odak.

### P7 — Limon Sarısı

**Fikir:** Limon sarısı yapraklar, kehribar-kahve gölge, lacivert-mor çekirdek (sarı-mor tamamlayıcı).

| Rol | Hex | L* | Karaktere ΔE00 |
|---|---|---|---|
| Hi (parlama) | `#FFFCD6` | 98.3 | 56.8 |
| Tip (uç) | `#FFF28A` | 94.6 | 71.9 |
| Base (ana) | `#FFDF2E` | 89.0 | 76.3 |
| Shadow (gölge) | `#A86B00` | 50.5 | 56.9 |
| Deep (damar) | `#7A4A00` | 36.1 | 58.2 |
| Outline (dış çizgi) | `#3F2400` | 17.1 | 59.6 |
| Glow (taban parıltı) | `#FFF4A0` | 95.4 | 68.3 |
| Warn (uyarı tint) | `#FF8A2E` | 69.2 | 49.1 |
| Merkez | `#3A2BB8` | 29.6 | 38.6 |
| Halo (açılış yayılımı) | `#FFF5A8` | 95.7 | 66.9 |
| Kapalı hal tonu | `#6E6748` | 43.5 | 52.8 |

**Ölçümler**

| Ölçüt | Sonuç | Durum |
|---|---|---|
| Karaktere mesafe (ΔE00, #FF51DE) | Base 76.3; gövde+çekirdek min, karakter gölge/koyu tonları dahil 19.9; hue farkı OKLCH 121° / HSV 99° | SINIRDA |
| Gökyüzüne kontrast (WCAG, #71BAEA) | Base 1.60:1; en iyi (Base veya Outline) 6.77:1; CRT gökyüzü #84D2F6 ile en iyi 8.56:1 | İYİ |
| Zemine kontrast (WCAG, #E68D41) | Base 1.92:1; en iyi (Base veya Outline) 5.64:1 | İYİ |
| Asit yeşiline (#28D028) karışma, ΔE00 min (Tip/Base/Shadow/Çekirdek) | normal 26.8; protanopi 7.6; deuteranopi 13.9 | SINIRDA |
| Renk körlüğünde karakterden ayrışma (ΔE00) | Base: prot 64.4 / deut 54.1 / trit 27.7; tüm roller min 17.2 | İYİ |
| Gri tonlamada ayrışma (ΔL*) | Base-karakter 25.3; en yakın gövde rolü-karakter 13.2; gökyüzü (en iyi) 55.5; zemin (en iyi) 49.6 | SINIRDA |

Ek: CRT sonrası (kaba model) Base-karakter ΔE00 72.9; kapalı hal ile açık Base ΔE00 41.7 (ΔL* 45.6); çekirdek açık-kapalı ΔE00 50.6; uyarı yaprak kayması (tint 0.35) açık Base'ten ΔE00 10.0; Warn rengi zemine ΔE00 4.9; Base koyu borulara min ΔE00 59.8.

**Açık / kapalı / uyarı geçişi**

- Kapalı: yapraklar koyu soluk zeytin-hardal tek ton, çekirdek söner.
- Açık: parlak limon Base + pale Tip + kehribar-kahve gölge, lacivert-mor çekirdek (tamamlayıcı) yanar.
- Uyarı: yapraklar %35 turuncuya kayar (limon -> amber, ılık), çekirdek turuncuya döner, kesikli halka. Turuncu zeminde kaybolma riski yüksek: ikincil ipucu (ölçek titreşimi) şart.
- Uyarı halinde yaprak Base yaklaşık `#FFC22F` (tint 0.35); Warn tonu `#FF8A2E`.

**Risk:** Asit yeşiline (chartreuse komşusu) renk körlüğünde en yakın aile (Base protanopide ΔE ~8, uyarı turuncusu deuteranopide ~2); gövde halka çekirdek sarısıyla (#F0D420) neredeyse aynı (ΔE ~3); turuncu zemin üstünde uyarı turuncusu kayboluyor (ΔE ~5).

**Neden iyi:** Pembeden hue olarak en uzak sıcak aile; karaktere ΔE 76; açık hal ekrandaki en parlak nesne.

### P8 — Deniz Camı Petrol

**Fikir:** Deniz camı: petrol-turkuaz yapraklar, açık aqua uç, kum-krem çekirdek.

| Rol | Hex | L* | Karaktere ΔE00 |
|---|---|---|---|
| Hi (parlama) | `#E4FFF6` | 97.9 | 37.0 |
| Tip (uç) | `#A9F5E2` | 91.3 | 45.1 |
| Base (ana) | `#0A7C86` | 47.2 | 45.3 |
| Shadow (gölge) | `#075A66` | 34.6 | 46.8 |
| Deep (damar) | `#053F4A` | 23.9 | 48.2 |
| Outline (dış çizgi) | `#02262F` | 13.3 | 50.2 |
| Glow (taban parıltı) | `#9AF0DC` | 89.1 | 46.4 |
| Warn (uyarı tint) | `#FFC247` | 82.0 | 64.2 |
| Merkez | `#FFF0C0` | 94.9 | 57.0 |
| Halo (açılış yayılımı) | `#A8F2E0` | 90.4 | 44.4 |
| Kapalı hal tonu | `#3F5D60` | 37.4 | 34.0 |

**Ölçümler**

| Ölçüt | Sonuç | Durum |
|---|---|---|
| Karaktere mesafe (ΔE00, #FF51DE) | Base 45.3; gövde+çekirdek min, karakter gölge/koyu tonları dahil 34.9; hue farkı OKLCH 132° / HSV 127° | İYİ |
| Gökyüzüne kontrast (WCAG, #71BAEA) | Base 2.34:1; en iyi (Base veya Outline) 7.51:1; CRT gökyüzü #84D2F6 ile en iyi 9.49:1 | İYİ |
| Zemine kontrast (WCAG, #E68D41) | Base 1.95:1; en iyi (Base veya Outline) 6.25:1 | İYİ |
| Asit yeşiline (#28D028) karışma, ΔE00 min (Tip/Base/Shadow/Çekirdek) | normal 26.8; protanopi 19.7; deuteranopi 19.8 | İYİ |
| Renk körlüğünde karakterden ayrışma (ΔE00) | Base: prot 18.4 / deut 21.5 / trit 58.9; tüm roller min 18.4 | İYİ |
| Gri tonlamada ayrışma (ΔL*) | Base-karakter 16.5; en yakın gövde rolü-karakter 16.5; gökyüzü (en iyi) 59.3; zemin (en iyi) 53.5 | İYİ |

Ek: CRT sonrası (kaba model) Base-karakter ΔE00 43.4; kapalı hal ile açık Base ΔE00 13.2 (ΔL* 9.9); çekirdek açık-kapalı ΔE00 51.7; uyarı yaprak kayması (tint 0) açık Base'ten ΔE00 0.0; Warn rengi zemine ΔE00 17.5; Base koyu borulara min ΔE00 31.7.

**Açık / kapalı / uyarı geçişi**

- Kapalı: yapraklar koyu soluk gri-teal tek ton, çekirdek söner.
- Açık: petrol Base, aqua Tip, kum-krem çekirdek yanar; halo aqua.
- Uyarı: yapraklar sabit (sarı+teal yeşile kaçıyor); çekirdek ve dış kenar amber'e döner, taban parıltısı amber, kesikli halka yanıp söner.
- Uyarı halinde yaprak Base yaklaşık `#0A7C86` (tint 0); Warn tonu `#FFC247`.

**Risk:** İki yönden karışma: asit yeşili (hue ~50 derece) ve gökyüzü mavisi arasında; gölge/koyu tonlar koyu teal halkayla (#04586C) neredeyse aynı (ΔE ~4) = başka bir node sanılabilir; sarı-teal karışımı yeşile kaydığı için uyarıda yaprak tint yapılmıyor.

**Neden iyi:** Pembeden hue olarak en uzak (~132 derece, ΔE min 35); koyu petrol değerce gökyüzünden ayrışır; protan/deutan ve asit ayrışması dengeli.

## 4. Karşılaştırma

| Palet | ΔE00 karakter (min aile) | hue° (OKLCH) | CR gökyüzü (en iyi) | CR zemin (en iyi) | ΔE00 asit (norm / prot / deut) | CVD karakter min | ΔL* gövde-min | Halka renklerine min ΔE00 (gövde) |
|---|---|---|---|---|---|---|---|---|
| MEVCUT | 10.8 | 19 | 3.2 | 2.7 | 27.1 / 10.6 / 15.6 | 5.0 | 3.0 | 11.9 |
| P1 İznik Kobalt | 24.6 | 74 | 8.3 | 6.9 | 28.0 / 14.1 / 16.3 | 12.6 | 23.0 | 9.2 |
| P2 Aizuri-e Prusya | 28.0 | 78 | 8.6 | 7.2 | 44.5 / 25.2 / 6.1 | 13.9 | 21.4 | 10.0 |
| P3 Porselen Kâğıt | 22.5 | 111 | 5.1 | 4.2 | 32.9 / 29.0 / 27.8 | 15.1 | 20.6 | 9.1 |
| P4 İndigo Altın | 19.9 | 59 | 8.4 | 7.0 | 28.9 / 10.9 / 14.4 | 12.8 | 21.2 | 9.0 |
| P5 Buz Camı | 27.2 | 111 | 7.3 | 6.1 | 34.9 / 32.2 / 31.4 | 8.1 | 17.0 | 15.8 |
| P6 Mürekkep Neon | 20.7 | 58 | 9.6 | 8.0 | 36.8 / 41.0 / 45.0 | 13.4 | 31.2 | 6.8 |
| P7 Limon Sarısı | 19.9 | 121 | 6.8 | 5.6 | 26.8 / 7.6 / 13.9 | 17.2 | 13.2 | 2.8 |
| P8 Deniz Camı Petrol | 34.9 | 132 | 7.5 | 6.2 | 26.8 / 19.7 / 19.8 | 18.4 | 16.5 | 3.9 |

MEVCUT satırı referans. ΔE00 karakter (min aile) = Tip/Base/Shadow/Çekirdek rollerinin, karakterin üç tonuna (#FF51DE, #C73FAD, #97338A) en yakın mesafesi. CVD karakter min = tüm rollerin protan/deutan/tritan benzetimlerinde karaktere en yakın mesafesi (en kötü durum). Halka sütunu bilgi amaçlı: çiçek bir node olduğundan, mevcut halka renkleriyle (kırmızı-turuncu, sarı, krem, teal, koyu teal, koyu mavi, pembe-kırmızı, çekirdek sarı) çok yakın bir gövde "aynı tür node" sanılabilir; ΔE00 < 6 olanlar yalnızca P7 (halka çekirdek sarısı) ve P8 (koyu teal halka).

### Ayrıca ölçümden çıkan paletler arası dersler

1. **Mavi/mor aileler protanopide magentaya hue olarak yaklaşıyor.** P1, P2, P4 gövdesi protanopi benzetiminde karaktere ΔE00 12.6 / 15.4 / 14.0; protan ve deutan gözde magenta mavi gibi kaydığı için bunu yalnızca değer kurtarıyor (Base ΔL* 23.0 / 24.2 / 24.7). Kural: mavi/mor ailelerde gövde L* <= ~45 ya da >= ~82 tutulmalı; ilk taslakta orta tonlu lavanta Tip (#8C86F0, L* 61) protanopide karaktere ΔE00 4.3 verdi ve açık tona çekildi.
2. **Orta değer şeridi (L* 55–80) tüm paletlerde sorun.** İlk taslakta Shadow'ları orta tonda bırakınca gökyüzüne 1.0:1 kontrast çıktı (P3, P5 ve P7 ilk sürümlerinde); gölgeleri şeridin dışına (daha koyu ya da çok açık) taşıyınca düzeldi. Son paletlerde gövde rolleri L* >= 82 veya L* <= ~50.
3. **Sıcak-soğuk karışımı:** soğuk gövdeye amber tint karıştırmak yeşilimsi/gri ara ton üretiyor (P5, P8), bu yüzden o paletlerde uyarı tint'i 0.

## 5. Seçim ve sıra

**1. P6 Mürekkep Neon.** Karaktere ΔE00 (min aile) 20.7 (mevcut 10.8), renk körlüğünde en kötü ΔE00 13.4 (mevcut 5.0), gövde-min ΔL* 31.2 (mevcut 3.0). Gökyüzüne 9.6:1, zemine 8.0:1; asit yeşiline normal 36.8, protanopi 41.0, deuteranopi 45.0: tüm paletler arasında asit ayrışması en güçlü. Sıcak vurgu (neon çekirdek, amber uyarı) tek odak. Bedeli: koyu boruya (ΔE00 13.1) ve koyu mavi halkaya yakın, kapalı-açık yaprak değeri yakın (durumu çekirdek ışığı taşımalı) ve sanat yönü "bayram şekeri" dünyasından en uzak seçenek. Karaktere değer olarak da en uzak palet (Base ΔL* 46.5).

**2. P3 Porselen Kâğıt.** Renk körlüğünde karakterden ayrışma Base için en az 29.6 (tüm roller min 15.1); asit yeşilinden normal 32.9, protan 29.0, deutan 27.8 uzak (P6 ve P5'ten sonra üçüncü). Gri tonlamada gövde-min ΔL* 20.6. Bedeli: gökyüzü üstünde gövde 1.88:1, silüeti lacivert dış çizgi (5.1:1) taşıyor; krem halkaya ΔE00 yaklaşık 9.1; uyarı tint'i ince (ΔE00 12.3), ikincil ipucu (çekirdek rengi + ölçek titreşimi) şart. Kapalı-açık farkı en net olanlardan: ΔE00 37.8, ΔL* 48.2.

**3. P1 İznik Kobalt.** Hue farkı 74°, ΔE00 min aile 24.6, gökyüzüne 8.3:1, zemine 6.9:1, gri ΔL* 23.0. Üç seçimin içinde tek kromatik (renkli) ve sanatsal kimliği en güçlü olanı: P6 ve P3'ün nötr/koyu-açık mantığına karşı bir "renkli" alternatif sunuyor. Bedeli: protanopide Base karaktere yalnız 12.6 (değer farkı ΔL* 23.0 kurtarıyor), açık sarı çekirdek halka çekirdek sarısına yakın; koyu gölgeler koyu mavi halkayla akraba.

Neden P1 üçüncü, P8 ya da P4 değil: P8 renk körlüğü ve asit ayrışmasında daha dengeli (CVD min 18.4, asit 26.8) ve hue olarak karaktere en uzak (ΔE min 34.9), ama gölge/koyu tonları teal halkalarla aynı (ΔE00 3.9) = başka bir node sanılabilir. P4 soluk altın çekirdeği protanopide asit yeşiliyle ΔE00 10.9 ve hue farkı P1'den az (59° vs 74°). P7 karaktere en uzak (ΔE 76.3) ama asit/uyarı ve halka sarısı çakışmaları en kötü. P5 gökyüzü ailesinde, gövde 1.72:1. P2'nin vermilyon çekirdeği kırmızı/turuncu rezervini bozuyor ve deuteranopide asit yeşiline ΔE00 6.1.

Bu sıralamayı ölçümler destekliyor; ama "hangisi bu oyunun sanat yönüne yakışır" kararı insanın. His farkı: P6 çiçeği ekrandaki tek koyu-neon nesne, P3 tek beyaz nesne, P1 çini renklerinden bir "renk adası" yapıyor.

## 6. Doğrulanamayanlar

- Celeste (Madeline'in saç rengi dash sayısına göre değişir mi, bu kararın tasarım gerekçesi): arama sonuçları saç boyası sayfalarıyla doldu, kaynak bulamadım. Proje bağlamında ima edilen "dash hakkı bitince renk değişir" bilgisinin Celeste'te de olduğunu ben doğrulamadım.
- Hades: Jen Zee'nin okunabilirlik/silüet/renk üzerine kendi sözlerini bulamadım (arama yalnızca ikincil blog ve Game Informer video sayfa açıklamasını verdi; videoyu izleyemedim). Elimdeki yalnızca bir oyuncunun Hades II okunabilirlik eleştirisi.
- Hollow Knight: Team Cherry/Ari Gibson'ın kendi açıklamasını bulamadım; yalnızca bir hayran renk çalışması ve ikincil yazılar.
- Kirby: Kirby'nin pembesinin arka planlarla ilişkisini anlatan geliştirici kaynağı bulamadım (Nintendo röportajı yalnızca gökyüzü ve bitki rengini anıyor).
- "Karakter rengine sahiplik / rezerve hue" kuralını açıkça yazan bir yayın bulamadım; bölüm 2'deki çıkarım dolaylı.
- Değer kontrastının ton kontrastından daha önemli olduğu iddiası: ZURB dersi ve birkaç blog arama özetinde gördüm ama açıp okumadım; kontrollü bir çalışma bulunmadı (arama sonucu da bunu söylüyor). Bu çalışma bunu sayısal olarak yalnızca bu oyunun renkleri için gösteriyor (L* şeridi).
- Bang Wong 2011 (Nature Methods) renk körü dostu palet: yalnızca arama özetini gördüm, makaleyi açmadım; paletlerde kullanmadım.
- CIEDE2000 uygulamasını Sharma ve ark. 2005 test çiftleriyle (üç çift: 2.0425, 27.1492, 1.2644) sınadım; bu referans değerleri belleğimden aldım, kaynak sayfasını bu oturumda açmadım. Üç çiftin tamamı birebir tuttu.
- "ΔE00 < ~10 zor ayırt edilir" ve ΔE00 ~2.3 JND ifadeleri yaygın kullanılan çalışma eşikleri; kaynak açmadım. İYİ/SINIRDA/ZAYIF eşikleri benim.
- Hedef cihazlarda CRT filtresinin gerçek renk dönüşümü: yalnızca proje notundaki gözlenen gökyüzü değerinden (#71BAEA -> #84D2F6) biliyorum; kaba modelim (koyu kanal +35 * (1 - c/255)) bunu birebir tutturmuyor (gökyüzü G kanalında +24 gözlenen, modelde +9). CRT sonrası satırları yalnızca mertebe için.
- Zemin kenarı pembe şerit (#E040C0) moodboard'dan gözle; kaynak sprite pikselinden ölçülmedi.
- Gerçek renk körü oyuncu testi yapılmadı; simülasyon yalnızca ön elemedir (XAG 103 de böyle diyor).

## 7. Yöntem notu

- Hesap betikleri `C:\Users\e-r-e\AppData\Local\Temp\claude\0de9a593395e2887\75bb5a5d-87fa-4cf3-9216-e775269c8412\scratchpad\` altında (Node 22): `colorlib.js` (sRGB, OKLab/OKLCH, CIELAB, CIEDE2000, WCAG, Machado matrisleri, grayscale), `palettes.js` (renkler), `measure.js` (ölçümler), `build-md.js`, `build-html.js` (bu iki dosyayı üretir).
- Renk uzayı: sRGB -> lineer; CIELAB için D65 (X 0.95047, Z 1.08883); OKLab katsayıları Ottosson'dan. ΔE: CIEDE2000 (kL=kC=kH=1). Kontrast: WCAG 2.x bağıl parlaklık. Gri tonlama: CIE L* (Rec.709 katsayılı Y üzerinden), CSS `grayscale(1)` ile HTML'deki gri sütun görsel olarak aynı sonucu verir.
- Renk körlüğü: Machado ve ark. 2009, severity 1.0 (tam dikromasi), lineer RGB üzerinde, sonra sRGB'ye geri. Viénot 1999 kullanılmadı (matrislerini kaynaktan doğrulayamadım).
- "Uyarı hali" ölçümü: yaprak renkleri OKLab'ta Warn rengine palet başına belirtilen oranda (0–0.35) karıştırıldı; çekirdek Warn'a döner.
- CRT: kaba model, yalnızca mertebe gösterir (bölüm 6).
- Ölçülen "dünya" renkleri `docs/ui-research/palette.md` değerlerinden ve brief'ten alındı; palette.md'yi bu çalışmada açmadım (brief'teki değerler kullanıldı).
- Neye bakılmadı: gerçek oyun içi ekran görüntüsü, shader'ın gerçek çıktısı, hareket/animasyon (uyarı titreşimi, açılış halosu) ve çiçeğin gerçek boyutu; hepsi görsel/hareket testiyle insan tarafından doğrulanmalı (feel insanın kararı).
