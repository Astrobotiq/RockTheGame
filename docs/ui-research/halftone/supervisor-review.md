# Halftone hareketli platformlar: supervisor değerlendirmesi

Tarih: 2026-11-13. İncelenen: `art-language-research.md`, `texture-research.md`, `textures/*.png` (cmyk_sparse/dense, gradient_ramp, classic_benday_red, riso_grain, dört trail sprite'ı), `directions.html` (kodu satır satır okundu, tarayıcıda çalıştırılmadı), GDD §8/§8.1, `moodboard/level_05.png` ve `level_12.png`, `palette.md`, UI `supervisor-review.md` ve `mashup-ui.html`, `systems/platforms.md`. Ek olarak Unity tarafında `CRTFilter.shader`, `M_CRTFilter.mat`, platform prefab ölçekleri ve `CameraController` okundu.

Etiketler: **[DOĞRULANDI]** kodu/asset'i ben okudum. **[HESAP]** okuduğum değerlerden hesapladım, oyunda görmedim. **[SORU]** karar senin.

Özet hüküm: umut var, ama mockup'lar oyunun gerçek koşullarında değil, "güzel bir web canvas'ında" değerlendirildi. Gerçek ölçekte ve CRT'den geçince D yönü bugünkü haliyle reddettiğin çamurlu piksel görünümüne geri döner. Senin seçtiğin B ve D, D'nin iskeleti ve B'nin malzemesiyle tek bir yönde birleştirilebilir (§3). Ölçek, palet ve CRT'nin düzeltilmesi şart.

---

## 0. En önemli bulgu: CRT her şeyi 480x360'a yapıştırıyor ve bu çalışma bunu yine kaçırdı

UI supervisor raporu bunu §0'da yazmıştı. Halftone araştırması aynı hatayı tekrarladı.

- **[DOĞRULANDI]** `Assets/Shader/CRTFilter.shader` satır 162-165: `snappedUV = floor(uv * _PixelResolution) / _PixelResolution`. `M_CRTFilter.mat` ve `Global Volume Profile.asset`: `480x360`. 1080p'de bir CRT hücresi **4 px geniş, 3 px yüksek**, yani kare olmayan piksel. Ön filtre yok, yalnızca `blur 0.5` var. Ekran örnekleniyor, yani küçük desen yumuşamıyor, aliasing yapıyor.
- **[DOĞRULANDI]** Tarama çizgisi `sin(uv.y * 360 * PI)`: periyot 2 hücre, yani 6 ekran pikseli. Aperture grille x'te 480 frekansında.
- Sonuç: araştırmanın 12. ilkesi ("tam sayıya yuvarlama yok, `smoothstep` + `fwidth` ile pürüzsüz") shader seviyesinde doğru, ama **ekrana çıkan görüntüde geçersiz**. Platform shader'ı ne kadar pürüzsüz olursa olsun, CRT sonunda onu 4x3'lük bloklara örnekliyor. Küçük nokta piksel lekesi olur. 1 hücreden küçük nokta, platform alt-piksel hareket ettikçe yanıp söner. "Ekranda ~6 px alt sınır" (directions.html tablosu) bu yüzden yanlış birimle verilmiş. Doğru birim CRT hücresi.
- **Pratik kural:** pürüzsüz görünmesi istenen bir nokta CRT'de en az **~6 hücre çapında** olmalı (1080p'de ≈ 24 px geniş / 18 px yüksek). 3-4 hücre çapındaki nokta "piksel daire" olarak okunur. 2 hücrenin altı titrer.

### 0.1 İkinci bulgu: mockup ölçeği oyunun 2-5 katı

- **[DOĞRULANDI]** Kamera: sahnede `OrthographicSize 6.8`, `CameraController.minOrthoSize = 8`. **[HESAP]** 1080p'de 1 dünya birimi ≈ 68-79 px ≈ 17-20 hücre geniş / 23-26 hücre yüksek.
- **[DOĞRULANDI]** Prefab'lar: `MovingPlatform` 2x1 birim, `AccelPlatform Variant` 4x1, `OneWay-MovingPlatform Variant` **3x0.2 birim**. **[HESAP]** Tek yönlü hareketli platform 1080p'de yaklaşık 15 px, yani **5 hücre** yüksek. Bu bir çizgi. Gövdesine nokta dolgusu sığmaz.
- `directions.html` 300 px'lik bir sahnede 50-62 px yüksekliğinde platformlar çiziyor. Platform ekranın 1/5'i kadar. Oyunda 1 birimlik platform ekranın 1/14'ü, tek yönlüsü 1/68'i. Moodboard ise oyun kamerasından 2-5 kat uzakta (GDD §8.1); orada (level_12) hareketli platformlar zaten birkaç piksellik kırmızı çubuklar.
- **[HESAP]** D yönünü gerçek ölçeğe taşıyınca (h ≈ 75 px, `sp = h/4.6` ≈ 16 px ≈ 4 hücre): sarı noktalar 2-3 hücre çapında piksel lekesi olur. Camgöbeği/macenta noktalarının yarıçapı `0.05·sp`'ye kadar iniyor, bu da **1 hücrenin altında**, yani hareket ederken yanıp sönen kum. Bu, reddettiğin "muddy" mockup'ın aynısı. Tek fark, oraya bu kez başka bir yoldan varılması.
- **Düzeltme:** nokta aralığı platform yüksekliğinin kesri olarak değil, **dünya biriminde** sabit tanımlanmalı. Alt sınır da CRT hücresine göre konmalı (§3).

### 0.2 Üçüncü bulgu: palet karışımı çamur üretiyor

**[HESAP]** Multiply sonuçları, oyun paletiyle:

| Üst üste | Sonuç | Okunuş |
|---|---|---|
| Sarı `#FCAE04` × mavi `#597DCE` | `#575503` | **Zeytin/çamur.** `cmyk_sparse_game` ve `cmyk_dense_game`'deki kahve-zeytin lekeler bundan |
| Sarı `#FCAE04` × teal `#289AA4` | `#276902` | Temiz baskı yeşili, referanstaki gibi |
| Sarı × pembe `#FF51DE` | `#FC3703` | Referanstaki kırmızı. Ayrı kırmızı katmana gerek yok |
| Pembe × teal | `#28318F` | Mor-lacivert, kabul edilebilir |

Araştırma "camgöbeği ≈ teal/gökyüzü" diyor ama dokular ve D yönü `#597DCE` (mavi) kullanıyor. Referanstaki temiz yeşilin yerine zeytin çıkmasının nedeni bu. **Camgöbeği = teal `#289AA4`** olmalı.

---

## 1. Yön puanları

1-5, 5 en iyi. "Maliyet" sütununda 5 = ucuz. Puanlar mockup ölçeğinde verildi; parantez içindeki değer gerçek ölçekte, CRT açıkken beklediğim puan **[HESAP]**.

| | (a) Referansa sadakat | (b) Okunurluk | (c) Dünya + UI uyumu | (d) İz gücü | (e) Unity maliyeti | (f) Aşama 2 hazırlığı |
|---|---|---|---|---|---|---|
| **A** Pop-art panel | 2 | 4 (3) | 3 | 2 | 4 | 4 |
| **B** Riso zine | 3 | 1 (1) | 2 | 4 | 2 | 2 |
| **C** Spider-Verse ofset | 1 | 5 (4) | 4 | 3 | 3 | 2 |
| **D** Karma | 4 (2) | 3 (2) | 3 | 3 | 3 | 4 |
| **B+D sentezi** (§3) | 4 | 4 | 3 | 5 | 3 | 4 |

Geliştirici `directions.html`'i gördükten sonra **B ve D'yi seçti**, A ve C seçilmedi. A ve C aşağıda yalnızca bütünlük için kısa tutuldu. Sentezin puanları hedef puan; uygulanıp CRT açıkken bakılmadan doğrulanmış sayılmaz.

### A, pop-art panel
Kodda gövde iki katman: kırmızı 45° (`sp = h/4.2`, yarıçap `0.36·sp`) ve küçük mavi 15°. Altında tam siyah 6,6 sert gölge var. Bu Lichtenstein'ın kırmızısı, senin referansındaki CMYK geçişi değil. Sert ofset gölgesi UI'ın dili. Dünyada gölge yok (moodboard düz). Hareketli bir nesnenin altındaki ikinci siyah silüet "collider nerede?" sorusunu doğuruyor. İz dünyaya sabit 45° kafese yapışıyor ve renk ömrün yarısında sertçe değişiyor (`q>0.5 ? shift : col`). Kafese yapışan, düzenli bir iz CRT'nin 4x3 ızgarasından geçince en çok "piksel" okunan iz olur. **Hüküm:** okunur ve ucuz, ama referans değil. İzi en zayıfı. Kafes fikrini ele.

### B, riso zine
Üç multiply katman (teal 15°, sarı 75°, pembe 45°), çizgi yok, yalnızca üst kenarda platform genişliğinin %80'i kadar ince lacivert bir çizgi. %55 alfa gren ve `0.07h` kenar dalgası var. İz en güçlüsü: üç gecikmeli emitter (0 / 0.08 / 0.16 sn), toplam yaklaşık 1.1 parçacık/px. Ancak referansındaki **kalın siyah mürekkep çizgisi** burada yok. Silüet gökyüzünde eriyor. Gren, CRT örneklemesinden geçince parıltılı kuma döner. **Hüküm:** tek başına platform olamaz, ama **malzemesi en değerli yön** (şeffaf mürekkep, kayma, gecikmeli iz). Geliştirici de B'yi seçti. Sentezde (§3) çizgi, kırpma ve opak kâğıt tabanıyla zayıflıkları kapatılarak kullanılıyor. Üç emitter yerine tek emitter + konum geçmişi tamponu aynı etkiyi verir.

### C, Spider-Verse ofset
Düz sarı gövde, alt %40'ta lacivert halftone, dolgu çizgiden 1.5-5.5 px kayıyor, çizgi saniyede 8 kez 3 durum arasında "kaynıyor" (`floor(t*8)%3`). Okunurluk en iyisi, ama senin istediğin "noktalarla dolu nesne" değil. Kaynayan çizgi üstünde durduğun bir yüzeyde titreme gibi okunur, CRT ise bunu hücre atlamasına çevirir. İzdeki "hayalet" çift (`r*0.3` ofsetli multiply) iyi bir fikir. Twos (boyutun 12 fps adımlanması) CRT ile birleşince piksel hissini geri getirme riski taşıyor. **Hüküm:** referansa en uzak yön. Hayalet çift fikri alınacak, kaynama alınmayacak.

### D, karma (araştırmanın önerisi)
Beş nokta katmanı var: sarı 0°, mavi 15°, pembe 75°, siyah 45°, kırmızı 105°. Bunlara kâğıt, %35 alfa gölge ve 4 px çizgi ekleniyor. Üst %20 temiz. Dolgu hız yönünün tersine 1-4.5 px kayıyor. İz parçacıkları `[y,y,c,m,k,r]` arasından rastgele renk alıyor, gövdenin arka yarısında rastgele bir noktadan doğuyor, yarıçapları `h·(0.13…0.28)`. Referansa en yakın yön ve doğru omurga, ama:
- 5 katman fazla. Kırmızı zaten sarı × pembeden çıkıyor. Siyah noktalar + siyah çizgi + koyu gölge alt kenarı ağırlaştırıyor ve gökyüzünde kirli duruyor.
- Dolgunun tamamı çizgiden kayıyor. Bir tarafta dolgu çizginin dışına taşıyor, öbür tarafta çizgiyle dolgu arasında boşluk açılıyor. Bu collider sınırını bulandırıyor ve 1 hücrelik kayma CRT'de 1 ile 2 hücre arasında titreyerek görünür.
- 0° sarı ızgara yatay sıralar üretiyor. Bu sıralar tarama çizgileriyle hizalanınca moiré çıkar. Hiçbir katman 0°/90°'de olmamalı.
- İz bir "konfeti bulutu": rastgele konum, rastgele renk, gövdedeki noktalardan **daha büyük** (16-34 px'e karşı 7-11 px). Gövdeyle bağı yok, yapısı yok.

**Hüküm:** doğru yön. Sadeleştirilmeli, ölçeği yeniden kurulmalı ve izi gövdeye bağlanmalı. §3 bunu yapıyor.

---

## 2. Gerçekten zayıf olanlar

### Mockup'larda
1. **Ölçek ve CRT yok sayılmış** (§0). En büyük hata bu. Mockup'taki "Ölçek" kaydırıcısı (%70-140) gerçek ölçeğe hiç yaklaşmıyor.
2. **Nokta ölçeği platform yüksekliğine bağlı** (`sp = h/4.2…h/6`). Prefab'lar `transform.scale` ile boyutlanıyor (3x0.2, 4x1). Nesne uzayı UV'si scale'i telafi etmezse noktalar **elipse** döner. Aralık yüksekliğe bağlı olduğu için 0.2 birimlik platformda noktalar kaybolur. Aralık dünya biriminde sabit olmalı, desen platformla birlikte taşınmalı ve dönmeli, ama scale'den bağımsız kalmalı.
3. **Renk sayısı fazla.** D'de beş mürekkep + kâğıt + gölge var. Referanstaki zenginlik üç mürekkebin üst üste basılmasından geliyor, beş ayrı renkten değil.
4. **İz fazla düzgün (A) ya da fazla rastgele (B, D).** Hiçbiri izi gövdenin deseninden "kopan" noktalar olarak kurmuyor. Bu oysa en ucuz ve en anlamlı bağ.
5. **İzin sonu titrer.** Bütün yönlerde parçacıklar doğrusal olarak 0 yarıçapa küçülüyor. Son %30'luk kısımda nokta 2 hücrenin altına iniyor ve CRT'de yanıp sönüyor. Küçülme bir alt sınırda durmalı, ondan sonra alfa ile sönmeli.
6. **Şekil hâlâ "kutu".** `squircle(w,h,5,3)` üst ve alt üssü farklı, ama uzun platformda bu fark köşelerde 2-3 px'lik bir değişikliğe iniyor, yani pratikte yuvarlak köşeli dikdörtgen. "Mükemmel dikdörtgen değil" isteği karşılanmamış.
7. **Gölge.** A ve D'deki ofset gölge dünyada başka hiçbir nesnede yok. Platformu "sahneye yapıştırılmış UI etiketi" gibi gösteren şeylerin başında bu geliyor.
8. **Teleport ve ölüm yok.** Mockup'ta ölüm sıfırlaması da sarma (wrap) da yok. Unity'nin hazır `Rate over Distance` ayarı 20 birimlik bir ışınlanmada odayı enine kesen bir parçacık şeridi basar (`WrappingPlatformManager`, oda 15; ölümde `PlatformController` başlangıca ışınlanıyor, bkz. `platforms.md`).
9. **Dönen platform.** `CircularMovementStrategy` `keepUpright` kapalıyken platformu döndürüyor. "Üst şerit temiz" kuralı yerel üste bağlıysa, dönen platformda temiz şerit yana ya da alta geçer.

### Araştırmada
1. **"Doğrulananlar" tablosu tam doğrulanmış değil.** Yöntem notu "yalnızca içeriği okunarak doğrulanan" diyor, ama Lichtenstein, riso, fxguide ve Jet Set Radio satırları kendi içinde "arama özeti; sayfa tam okunmadı" notu taşıyor. 10 satırın 4'ü arama özeti. İçerik makul, ama etiket yanlış.
2. **Moodboard açılmamış** (araştırma bunu kendisi söylüyor). "Dünyaya uyum" bölümü GDD metnine dayanıyor. Ben açtım: dünyada hiç gölge yok. Node'lar parlak, büyük, gökkuşağı halkalı. Zemin kenarları basamaklı. Platformun "sticker" riski gerçek ve büyük.
3. **`UiThemeSO.BorderWidthFor` ile platform çizgisi** önerisi kategori hatası. O değer UI sanat pikseli (480x270) cinsinden, dünya biriminde anlamı yok.
4. **Palet uyarısı uygulanmamış.** Bkz. §0.2, mavi ile teal.
5. **Küçük hata:** `texture-research.md` "~15° = (13,-4)" diyor, oysa atan(4/13) ≈ 17°. (19,-5) ≈ 14.7° doğru.
6. **Node ile yarış eksik ele alınmış.** Platform paleti (sarı, kırmızı, teal, kâğıt) **node halka paletinin birebir aynısı** (`RampTex_10Rings`). Platform "dekoratif mi?" sorusundan önce "node'un dilini mi konuşuyor?" sorusu var. Ayırıcı olan şey şekil dili: node = iç içe halka, çizgisiz ve parlak. Platform = nokta, kalın mürekkep çizgili. Bu ayrım korunmalı: platformda halka ya da konsantrik desen hiç olmamalı, node'da mürekkep çizgisi olmamalı.

---

## 3. Daha iyisi: "Riso levha" (B + D sentezi)

**Geliştirici geri bildirimi (2026-11-13):** `directions.html`'i görünce B (risograph) ve D (karma) seçildi, A ve C seçilmedi. Bu bölüm o yüzden bir B+D sentezi: **iskelet D'den** (düz temiz üst, okunur silüet, seyrek CMY dolgu, altta yoğun bölge), **malzeme B'den** (şeffaf multiply mürekkep katmanları, gren, kayık baskı, renk başına gecikmeli iz). B'nin üç zayıflığı (silüet, collider kenarı, gökyüzünde kararan mürekkep) aşağıda tek tek kapatıldı (§3.2).

Bir cümle: **kâğıt rengi, yumuşak alt köşeli bir levha. Üstüne üç şeffaf riso mürekkebi (sarı, pembe, teal) birbirinden hafif kayık basılmış. İri sarı noktalar seyrek, alt kenarda üç mürekkep üst üste binip yoğunlaşıyor. Collider'ın sınırını tek bir kalın mürekkep çizgisi çiziyor. Platform hareket edince her mürekkep ayrı bir gecikmeyle geride kalıyor ve iz üç renk şeridine ayrılıyor.**

Malzeme kuralı: *hareket eden her şey baskıdır.* Arazi piksel sanat kalır. Platformun dünyadan farklı görünmesi bir hata değil, "bu hareket eder" sinyali. Sticker riskini gölgeyi kaldırarak, çizgiyi boru kenarıyla aynı mürekkepte tutarak ve dünya paletine bağlı kalarak azaltıyoruz. Araziye halftone eklemiyoruz, en azından v1'de.

### 3.1 Şekil (shader içinde SDF, mesh değil)
- **Üst kenar = collider'ın üst kenarı**, düz ve dalgasız. Görsel hiçbir yerde collider'ın dışına taşmaz. Tek istisna çizginin yarı kalınlığı.
- **Köşe yarıçapı:** üst köşeler `0.22·h` (en az 0.06 birim), alt köşeler `0.5·h` (alt kenar hap gibi). Kutuyu kıran şey bu asimetri; uzun platformda bile alt uçlar belirgin yuvarlak kalır.
- **Riso kenarı (B'den):** yanlarda ve altta statik, düşük frekanslı bir kenar sapması var: genlik `0.03·h`, dalga boyu yaklaşık 0.6 birim. **Üstte 0.** Sapma collider'ın içine doğrudur, dışına taşmaz. B'nin `0.07·h`'si gerçek ölçekte 1-2 CRT hücresine iner ve titrer; 0.03 bile CRT kararına bağlı (Soru 1). Kaynama (boiling) hiçbir zaman yok.
- **Gölge:** yok (A ve D'deki ofset gölge dünyada başka hiçbir nesnede yok).

### 3.2 B'nin zayıflıkları sentezde nasıl kapanıyor

| B'nin sorunu | Kök neden (kodda) | Sentezdeki çözüm |
|---|---|---|
| **Silüet zayıf** | Çizgi yok; şekli yalnızca dalgalı renk lekesi veriyor | Kalın mürekkep çizgisi geri geliyor (§3.3), ama riso dilinde: çizgi de bir "baskı katmanı" gibi davranıyor, renk katmanları ondan bağımsız kayıyor. Silüeti çizgi verir, dokuyu mürekkepler |
| **Collider kenarı belirsiz** | Üç katman farklı yönlere kayıyor ve gövdenin dışına taşıyor; tek net çizgi üst kenarın yalnızca %80'i | (1) Çizgi **asla kaymaz** ve collider'a oturur. (2) Bütün mürekkep katmanları çizginin **içinde** kırpılır, kayan mürekkep dışarı taşamaz. (3) Kayma çizginin içinde bir "mürekkep boşluğu" ya da "taşma" olarak görünür, sınırın kendisini oynatmaz. (4) Üst şerit kâğıt, nokta yok |
| **Mürekkep gökyüzünde kararıyor** | Multiply gökyüzü mavisiyle çarpınca: sarı × `#71BAEA` = `#717F04` (zeytin), pembe × gök = `#713BCC` | (1) Gövdede multiply yalnızca **opak kâğıdın** üstünde, kâğıt maskesinin dışında mürekkep yok (B mockup'ı bunu kısmen yapıyordu). (2) İz parçacıkları gökyüzünün üstünde **normal (opak) karışımla** çizilir. Multiply yalnızca bir parçacığın kendi içindeki çift dairede, shader içinde hesaplanır (§3.7). (3) Gren mürekkebin kapsamasını oynatır, kâğıdı koyulaştırmaz |
| **Gren CRT'de kum** | 160 px'lik piksel gürültüsü, %55 alfa | Gren piksel gürültüsü değil, **düşük frekanslı mürekkep yoğunluğu** (§3.6) |
| **Üç emitter, pahalı** | Her renk için ayrı gecikmeli emitter | Tek `ParticleSystem` + bir script. Gecikmeyi platformun konum geçmişinden okur (§3.7) |

### 3.3 Çizgi
- `#140C1C` (boru kenarıyla aynı mürekkep), tek ağırlık, kalınlık `max(0.08 birim, 0.08·h)`. **[HESAP]** 0.08 birim ≈ 6 px ≈ 2 CRT hücresi yüksekliği; daha incesi kesik görünür.
- Riso seçeneği (Soru 2): çizgi koyu riso mavisi `#30346D` olsun ve renk katmanları onunla multiply olsun. Önerim mürekkep `#140C1C`, çünkü referansındaki çizgi siyah ve gökyüzünde en net ayrışan o.
- Çizgi asla kaymaz. Yalnızca renk katmanları kayar ve onlar çizginin içinde kırpılır.

### 3.4 Dolgu: üç şeffaf riso katmanı
Desen platformun yerel uzayında üretilir ve **scale'i telafi eder**: `p = localPos * lossyScale` (dünya birimi; platformla birlikte döner ve taşınır, ama gerilmez). Katmanlar kâğıdın üstüne sırayla multiply ile basılır: teal, sonra sarı, sonra pembe (B'nin sırası). Hepsi gövde SDF'iyle kırpılır.

| Katman | Renk | Açı | Aralık | Çap (v = 0 üst … 1 alt) |
|---|---|---|---|---|
| Kâğıt | `#F1E6C7` (opak) | – | – | taban |
| Sarı (ana Ben-Day) | `#FCAE04` | 45° | **P** | `P·(0.50 + 0.35·v)` |
| Pembe | `#FF51DE` | 75° | P | v < 0.2'de yok; sonra `P·(0.26 + 0.40·v)` |
| Teal | `#289AA4` | 15° | P | v < 0.35'te yok; sonra `P·(0.24 + 0.34·v)` |

- Mürekkep opaklığı katman başına %90 (B'deki 0.92 gibi). Riso'nun "tam kapatmayan mürekkep" hissi buradan geliyor ve üçlü bindirmenin siyaha kaçmasını engelliyor.
- **P = 0.42 dünya birimi** (başlangıç). Referans platform (h = 1) için `0.42·h`, gövdede yaklaşık 2.5 sıra nokta. **[HESAP]** Sarı noktanın çapı 0.21-0.36 birim ≈ 4-7 CRT hücresi: pürüzsüz okunmanın sınırı. CRT 480x360 kalırsa P = 0.5.
- **Hiçbir katman 0° ya da 90°'de değil** (D'deki 0° sarı tarama çizgileriyle hizalanır).
- **Temiz üst şerit:** üstten `max(0.18 birim, 0.22·h)` düz kâğıt.
- **Yoğun bölge (D'den):** yalnızca alt %30 (v > 0.7). Sarı noktalar birbirine değer, pembe ve teal binince kırmızı (`#FC3703`), yeşil (`#276902`) ve mor (`#28318F`) kendiliğinden çıkar. **Siyah nokta katmanı ve ayrı kırmızı katman yok** (D'nin beş katmanı üçe iniyor).
- **Alt sınır:** hesaplanan çap `dMin = 0.14 birim`in altına düşerse nokta `dMin` çapında çizilir ve alfası `d/dMin` olur. Böylece CRT'de titreyen alt-hücre noktalar oluşmaz.
- **Dönen platform** (`keepUpright = false`): değer rampası `v` dünya yukarısına göre hesaplanır, nokta konumları nesne uzayında kalır. Temiz şerit her an yukarıya bakan kenarda olur.
- **İnce platform** (h < 0.45 birim; örn. `OneWay-MovingPlatform` 0.2): gövdede nokta yok. Kâğıt dolgu, üstüne kayık iki düz mürekkep bandı (sarı ve pembe, %90 opak, her biri gövdenin %60'ı yüksekliğinde) ve çizgi. Riso hissi kayık düz renkten, halftone kimliği izden gelir.
- **Bilgi taşıyan yoğunluk (öneri):** `AcceleratingPingPongStrategy` kullanan platformlarda (zıplama momentumu veren tek tür) yoğun bölge %30 yerine %45. Ek maliyeti yok ve "bu platform fırlatır" sinyalini verir.

### 3.5 Kayık baskı (B'nin hissi, D'nin disiplini)
- **Statik:** pembe katman sarıya göre `(+0.04, -0.03)` birim, teal `(-0.04, +0.03)` birim kayık. Platform dururken de görünen sabit baskı kusuru.
- **Hıza bağlı (B'den):** teal hızın tersine `2k·|v|`, pembe hızın yönünde `k·|v|` kayar (B'deki `ox*2` / `-ox` oranı). `k = 0.012 sn`, toplam üst sınır 0.06 birim. Hız görsel olarak yumuşatılmış bir değerden alınır (üstel, ~0.1 sn). Hızlanan platform uçta durunca kayma 0.15 sn içinde geri yaylanır ve oyuncuya "durdu" bilgisini verir.
- Sarı katman ve çizgi kaymaz. Collider sınırı her zaman çizginin kendisi.

### 3.6 Gren (B'den, CRT'ye göre yeniden tanımlı)
- Piksel gürültüsü **yok**. Onun yerine mürekkep kapsamasını oynatan düşük frekanslı bir yoğunluk alanı var: dünya biriminde ~0.25 birim ölçekli değer gürültüsü (≈ 5 CRT hücresi), noktanın çapını ±%8 değiştirir. Etki gerçek riso'daki "mürekkep bazı yerlerde az geçmiş" görünümü.
- Kâğıda ayrıca çok hafif, 0.6 birim ölçekli lekelenme: parlaklık ±%3.
- İkisi de platforma kilitli ve statik, animasyon yok.
- Piksel ölçeğinde ince gren yalnızca CRT çözünürlüğü yükselirse denenir (Soru 3).

### 3.7 İz: renk başına gecikmeli riso katmanları (pazarlıksız kısım)
Tek `ParticleSystem` + bir script (`HalftoneTrailEmitter`). Script `ParticleSystem.Emit` çağırır. Hazır `Rate over Distance` kullanılmaz; teleport koruması ve gecikme aynı yerde çözülür.

- **Gecikmeli katmanlar (B'den):** script platformun son ~0.3 sn'lik konum ve dönüş geçmişini bir halka tamponda tutar. Her mürekkebin emisyonu platformun **geçmişteki** bir kopyasından yapılır: teal 0 sn, sarı **0.06 sn**, pembe **0.12 sn**. (B'deki 0.08 / 0.16 hızlı platformda şeritleri gövdeden koparıyor; başlangıç biraz daha sıkı.) Hareket üç renk şeridine ayrılır, durunca şeritler üst üste biner.
- **Doğum yeri (D'den, gövdeye bağlı):** gecikmeli kopyanın arka %40'lık ve alt yarısındaki (v > 0.4) kafes hücrelerinden biri rastgele seçilir. Parçacık o hücrenin dünya konumunda, o katmanın rengi ve o hücrenin çapıyla doğar. Yani iz, gövdedeki noktaların kopup geride kalması olarak okunur, konfeti gibi değil.
- **Yoğunluk:** mürekkep başına, katedilen her dünya birimi için `1.2 × (genişlik / 3)` parçacık (genişlik birim cinsinden, 1 ile 2 kat arasında kırpılır). Yani toplam ≈ 3.6/birim. **[HESAP]** 4 birimlik hızlanan platform ~8 birim/sn tepe hızında saniyede yaklaşık 38 parçacık üretir; 0.9 sn ömürle ~35 canlı parçacık eder.
- **Boyut:** doğduğu hücrenin çapı (0.5P-0.85P). Parçacıkların %15'i 1.2P çapında "mürekkep damlası" vurgusu. Ömrün ilk %40'ında boyut sabit; sonra `dMin`'e kadar küçülür, oradan sonra alfa ile söner. 0 yarıçapa inmez.
- **Renk ve karışım:** yalnızca üç mürekkep (sarı, pembe, teal); siyah ve ayrı kırmızı yok. Gökyüzünün üstünde **opak normal karışım** (multiply gökyüzünü zeytine çevirir, §3.2). Riso bindirmesi parçacığın içinde çözülür: pembe ve teal parçacıkların yarısı, öbür mürekkepten çapın %22'si kadar kayık bir "hayalet" daire taşır. Kesişim rengi shader'da `renkA × renkB` olarak hesaplanır. Bu, sprite değil **analitik SDF iki daire** (`trail_misreg_pair.png`'deki ~%35 kayma ve tüylü kenar fazla). Ayrı parçacıklar birbirinin üstüne opak biner; bu kabul edilebilir bir sapma.
- **Gren:** parçacık dairesinin kenarı §3.6'daki gibi ±%8 düşük frekanslı sapma taşır (parçacık başına rastgele tohum, statik).
- **Hareket:** dünya uzayında simülasyon. Başlangıç hızı platform hızının `-0.15` katı + dik yönde ±0.2 birim/sn titreme. Sürtünme ~3/sn. Yerçekimi yok. Hareket **pürüzsüz**.
- **Ömür:** 0.7-1.1 sn.
- **Üst sınır:** platform başına 54 parçacık (mürekkep başına 18), ayrıca sahne geneli bir bütçe. Doluysa yeni doğum atlanır.
- **Sıralama:** parçacıklar platformun ve node'ların arkasında. İz hiçbir zaman bir kanca hedefinin üstüne binmez.
- **Teleport ve ölüm:**
  - Bir karede platform 1.5 birimden fazla yer değiştirirse o kare emisyon yok, birikmiş mesafe ve **konum geçmişi tamponu** sıfırlanır. Tampon sıfırlanmazsa gecikmeli katmanlar teleport öncesi konumdan doğmaya devam eder. Wrap'te eski parçacıklar silinmez, kadraj dışında kendiliğinden söner.
  - **Ölüm sıfırlamasında** (`IResettable`) `ParticleSystem.Clear()`, tampon ve birikmiş mesafe sıfırlanır.
  - Oda `Sleep`'ten uyanınca da aynısı.
  - Tetiklenen platform `t = 0`'da bekliyorsa emisyon doğal olarak sıfır.

### 3.8 Animasyon
- Gövde: pürüzsüz. Kaynama yok, adımlama yok, gren statik. Gövdedeki tek hareket hıza bağlı kayık baskı.
- İz: pürüzsüz. Twos (12 fps adımlı boyut) yalnızca test seçeneği. CRT zaten zamansal bir doku ekliyor; üstüne adımlama piksel ve titreme hissini çağırır.

### 3.9 Aşama 2 nasıl takılacak (şimdi yapılmayacak, yalnızca yer ayrılacak)
Shader'a şimdiden dört property eklenir, değerleri 0 olur. `MaterialPropertyBlock` ile platform başına verilir:
- `_InfluencePos` (dünya, oyuncunun ayakları), `_InfluenceRadius` (başlangıç 2.5 birim), `_InfluenceAmount` (0-1, iniş anında yaylanır). Yarıçap içindeki hücrelerin merkezi oyuncuya doğru `amount·(1 - d/R)·0.4P` kadar çekilir, çapları `+0.2P` büyür.
- `_ShiftFront`: oyuncudan dışarı doğru ~6 birim/sn ile genişleyen halka. Riso dilinde bu bir **mürekkep değişimi**: halkanın içinde sarı katmanın mürekkebi pembeye (ya da oyuncunun o anki rengine) döner. Katman yapısı sabit kaldığı için bu yalnızca bir renk uniform'u.
- Gecikmeli iz bunu doğal taşır: geçmiş tamponu mürekkep rengini de kaydederse renk değişimi izde gecikmeli dalga olarak görünür. Ek sistem gerekmez.
- Tetik: "oyuncu bu yüzeye bastı" olayı. `TriggeredPlatformController` bunu zaten algılıyor; `PlatformController` için `KinematicPhysicsHandler`'ın bildiği aktif `IMovingSurface` kullanılır. Bu aşama 2'nin işi.

### 3.10 Ayarlanacak üç parametre (başlangıç değerleri)

| Parametre | Başlangıç | Aralık | Ne yapar |
|---|---|---|---|
| `DotPitch` (P) | **0.42 birim** | 0.36-0.55 | Bütün nokta ölçeği. CRT 480x360 kalırsa 0.5 |
| `TrailLayerDelay` | **0 / 0.06 / 0.12 sn** (teal/sarı/pembe) | 0.03-0.16 adım | Renk şeritlerinin ayrışması. Büyüdükçe daha "riso", ama hızlı platformda şeritler gövdeden kopar |
| `MisregVelocityK` | **0.012 sn** (üst sınır 0.06 birim; statik 0.04) | 0-0.02 | Hıza bağlı katman kayması |

Ayrıca: `TrailPerUnitPerInk` 1.2, `TrailLife` 0.9 sn, `InkOpacity` 0.90, `GrainAmount` 0.08, `CleanTopBand` 0.22·h, `DenseZone` 0.30 (hızlanan platformda 0.45). Hepsi bir `HalftonePlatformStyleSO`'ya girer (projenin SO-başına-sistem deseni). Oyun hissi kararı senin; değerler başlangıç, gamepad'le dene.

### 3.11 Maliyet tahmini (1-1.5 saatlik task'lar)
1. SDF levha + üç multiply katmanlı dolgu shader'ı (URP 2D unlit, HLSL ya da Shader Graph custom function), scale telafisi, statik gren: 2 task.
2. `HalftoneTrailEmitter` (geçmiş tamponu, renk başına gecikme) + çift daireli parçacık SDF shader'ı + teleport/reset/uyku: 2 task.
3. Test odası (`PhysicsTest` sahneleri), CRT açık ekran görüntüleri, prefab'lara bağlama: 1-2 task.

Yaklaşık 5-6 task, yani 1.5 gün. Aşama 2 hariç.

---

## 4. Karar soruları (B+D sentezinin içinde)

1. **[SORU] CRT'nin 480x360 hücre örneklemesi ne olacak?** Riso'nun inceliği (kayma, gren, kenar sapması) 1 hücrelik ölçekte yaşıyor ve 4x3'lük kare olmayan hücre bunu yiyor. Seçenekler: (a) `pixelResolution`'ı 960x540'a çıkar (kare 2x2 hücre) ve tarama çizgisini 360'ta tutmak için shader'a ayrı bir `_ScanlineResolution` ekle; (b) 480x360 kalsın, P = 0.5, statik kayma 0.06, gren yalnızca yoğunluk alanı. **Önerim: önce (a)'yı tek bir sahnede dene.** Arazinin piksel sanatı 2x2 hücrede de bozulmaz. Tarama çizgisi ayrı tutulursa CRT hissi korunur. `DefaultVolumeProfile`'daki 320x240 değeri de aynı değere çekilmeli.
2. **[SORU] Çizgi saf mürekkep `#140C1C` mu olsun, riso koyu mavisi `#30346D` mi?** **Önerim: `#140C1C`.** Referansında çizgi siyah, gökyüzünde en net silüeti o veriyor ve B'nin bir numaralı sorununu kapatıyor. `#30346D` daha "riso" durur ama gökyüzü `#71BAEA` üstünde kontrastı düşer.
3. **[SORU] Gren ne kadar olsun?** (a) Yalnızca düşük frekanslı mürekkep yoğunluğu ±%8 + kâğıt lekesi ±%3; (b) bunlara ek ince piksel greni. **Önerim: (a).** (b) yalnızca Soru 1'de (a) seçilirse ve CRT açıkken titreme yapmıyorsa.
4. **[SORU] İz renk şeritleri ne kadar ayrışsın?** Seçenekler: sıkı 0 / 0.06 / 0.12 sn, ya da B'deki gibi 0 / 0.08 / 0.16 sn. **Önerim: sıkı başla.** Hızlanan platformun tepe hızında 0.16 sn gecikme pembe şeridi gövdenin ~1.3 birim gerisine atar ve iz platformdan kopuk görünür. Gamepad testinde "yeterince riso değil" dersen aç.
5. **[SORU] Camgöbeği olarak teal `#289AA4` kullanılsın mı (D'deki mavi `#597DCE` yerine)?** **Önerim: evet.** B zaten teal kullanıyordu. Sarı × mavi zeytin çamuru verir, sarı × teal referanstaki temiz yeşili.

İkincil, karar değil teyit: 0.2 birimlik tek yönlü platformlar için §3.4'teki "kayık iki düz bant + iz" varsayılanı; hızlanan platformda yoğun bölgenin %45 olması.

---

## 5. Unity'de doğrulama listesi (uygulama sonrası)

Hepsi CRT **açıkken**, sahnenin gerçek volume profiliyle yapılacak.

- [ ] **Çözünürlük:** 1080p (ana hedef). 1440p ve 1280x800 (Deck) desteklenecekse onlar da. Her birinde CRT hücresinin piksel cinsinden boyutu farklı; noktaların `dMin` altına düşmediğini gözle kontrol et.
- [ ] **Kamera:** ortho 6.8 ve 8 (`minOrthoSize`), oda bazlı zoom override'ı olan en uzak oda, bir de moodboard uzaklığında (genel bakış) ekran görüntüsü. Uzakta nokta dolgusu düz renge dönüşebilir; bu kabul, ama titrememeli.
- [ ] **Moiré ve titreme:** platform çok yavaş hareket ederken (≈0.3-0.5 birim/sn) 10 sn izle. Nokta kenarlarında "yürüyen" desen ya da yanıp sönen nokta var mı? Dolgu ve iz ayrı ayrı kontrol edilmeli.
- [ ] **Scale:** 2x1, 3x0.2, 4x1 prefab'larında noktalar daire mi (elips değil)? Aralık hepsinde aynı mı?
- [ ] **Bütün sürücüler ve stratejiler:** Linear, Transform, Accelerating (uçta ani duruş, kayık baskı geri yaylanıyor mu), Circular `keepUpright` açık ve kapalı (temiz şerit yukarıda kalıyor mu), Triggered (beklerken iz yok), Wrapping (oda 15, teleportta şerit yok), `MovingLauncPad`, `OneWay-MovingPlatform Variant`.
- [ ] **Ölüm sıfırlaması:** öl ve yeniden doğ; eski iz siliniyor mu, ışınlanma izi yok mu? Gecikmeli katmanlar (sarı, pembe) teleport öncesi konumdan doğmaya devam ediyor mu (geçmiş tamponu sıfırlanmalı)? Oda değiştir ve geri dön (uyku/uyanma).
- [ ] **Gökyüzünde mürekkep:** iz parçacıkları ve gövde kenarı gökyüzünün üstünde zeytin/kirli tona dönüyor mu? (Multiply kâğıdın dışına sızmamalı.)
- [ ] **Collider kenarı:** hızlanan platform tepe hızındayken ve uçta dururken, kayan mürekkep çizginin dışına taşıyor mu? İniş noktası her an net mi?
- [ ] **Hiyerarşi testi:** ekran görüntüsünü bulanıklaştır ya da göz kıs. İlk okunan hâlâ node halkaları mı? İz node'un önüne geçiyor mu?
- [ ] **Gri tonlama testi:** platform gökyüzünden ve turuncu arazi önünden değer olarak ayrılıyor mu? Ayrıştıran şey çizgi olmalı, dolgu değil.
- [ ] **Sticker testi:** platform arazinin yanında "yapıştırılmış" duruyor mu? Notunu yaz, araziye dokunma kararı ayrı.
- [ ] **Performans:** Profiler'da en kalabalık odada toplam parçacık sayısı ve shader maliyeti. Bütçe dolunca doğum atlanıyor mu?
- [ ] **Gamepad playtest (senin kararın):** üstünde durmak, inmek, hızlanan platformdan fırlamak. Yürünen kenar her an net mi? İz hızı okumaya yardım ediyor mu, dikkat mi dağıtıyor? Değerleri `HalftonePlatformStyleSO`'da ayarla, eski ve yeni değeri not et.
- [ ] Bittiğinde `docs/systems/platforms.md`'ye görsel bölümü ekle ve "Son kod doğrulaması" tarihini güncelle.
