# Çiçek node'u: geleneksel ve sanatsal çiçek dilleri (Araştırma A)

Tarih: 2026-11-20. Odak: ŞEKİL ve DİL (siluet, yaprak sayısı/katman, merkez, kontür, iç işaretler, durum okunurluğu). Renk ağırlığı başka agent'ta; hex'ler yalnızca ipucu.

## Nasıl okunmalı

- "Doğrulandı" = sayfayı açıp içeriğini okudum (WebFetch). Sayfa metninin söylemediği şeyi kaynağa yazmadım.
- "Çıkarım" = kaynakta yazmayan, benim bu oyun için ürettiğim fikir/yorum. Fikir bölümlerindeki somut öneriler çıkarımdır; kaynak yalnızca dilin dayanağını verir.
- Yasaklı çizgiler (Bauhaus/geometrik soyut, op-art, halftone, genel "folk", spiral) öneri olarak kullanılmadı. Khokhloma'nın kıvrımları spiral'e, Kusama'nın noktaları halftone/op-art'a çok yakın olduğu için elendi (sonda "Elenenler").
- Oyunla ilgili sabitler: gökyüzü #71BAEA (mavi çiçek zeminde kaybolur), kanca node'ları turuncu/sarı/krem/teal/koyu mavi, fırlatma pedi kırmızı #FC1E00, karakter magenta-pembe #FF51DE. Çiçeğin bu listeden ayrışması gerekiyor.

## Hızlı özet tablo

| # | Yön | Ana kazanç | Durum okunurluğu | Shader zorluğu | Karaktere karışma |
|---|-----|-----------|------------------|----------------|-------------------|
| 1 | İznik Quatre Fleurs (lale tomurcuğu, karanfil açık) | Kapalı ve açık halin SİLUETİ tamamen farklı | Çok yüksek | Orta | Düşük |
| 2 | Vitray / kurşun çizgi | Cel-shade bantlarının yerine "cam parçaları"; arkadan ışık hissi | Yüksek | Düşük-orta | Düşük |
| 3 | Baskı kayması / overprint (Warhol, risograf) | Durum sinyali: dolgu ile kontür arası kayma | Yüksek (uyarı) | Düşük | Orta |
| 4 | Matisse kağıt kesme | Kontürsüz el kesimi siluet, her yaprak biraz farklı | Orta-yüksek | Düşük | Düşük |
| 5 | Kamon (kiku) çift katman + ters çevirme | Durum = pozitif/negatif ters çevirme | Yüksek | Düşük | Düşük |
| 6 | Papel picado (delik işlemeli yaprak) | Delikler, pinked kenar, kıpırdanma | Orta | Orta | Düşük |
| 7 | Murakami gülen çiçek (yüz merkez) | Merkezde yüz: uyuyor/gülüyor/endişeli | Çok yüksek | Düşük-orta | Düşük (ton riski var) |
| 8 | Kalamkari (önce kontür, sonra dolgu) | Kalem kontürü + mum-rezist dolgu mantığı | Orta | Orta | Düşük |
| 9 | Ukiyo-e / hanafuda (anahtar çizgi + düz renk + bokashi) | Tek bir taban gradyanı; kart çiçekleri (sakura, şakayık) | Orta | Düşük | Orta |
| 10 | Aizuri-e + katazome (tek hue + tek kırmızı vurgu) | Merkez noktasının kırmızı pop'u = açık sinyali | Yüksek | Düşük | Düşük |
| 11 | Art Nouveau (Gallé cameo, Mucha) | Oyma katman: kesilen yerde alt renk görünür | Orta | Orta | Düşük |
| 12 | Gzhel (tek hue fırça darbesi) | Tek fırça darbesi damla yaprak, tonlu | Orta | Düşük | Düşük |

(Sıralama = ben tablo içinde önemli olanları üste çektim; asıl öneri en sonda.)

---

## 1. Osmanlı / İznik çini: Quatre Fleurs ve saz

### Doğrulanmış bağlantılar
- Wikipedia, İznik çinisi: https://en.wikipedia.org/wiki/Iznik_pottery (okundu; ilk 100.000 karakter)
- Courtauld, İznik tabağı: https://sites.courtauld.ac.uk/illuminating-objects/illuminating-objects-home/iznik-dish/ (okundu; renk/kontür anlatmıyor, sadece kompozisyon)

### Kaynaktan doğrulanan
- Quatre Fleurs: stilize lale, karanfil, gül, sümbül; 16. yy ikinci yarısı, saray ressamı Kara Memi ile ilişkilendiriliyor (Wikipedia, Courtauld).
- Saz üslubu: uzun, tırtıklı, saz kamışı benzeri yapraklar, dinamik süpürmelerle; karşıt olarak sabit rozetler (Wikipedia).
- Palet sırası: önce yalnız kobalt, sonra turkuaz (~1520), orta yüzyılda mor/yeşil/gri/siyah, ~1560'ta kalın "bole" kırmızı, ~1567'de parlak zümrüt (Wikipedia).
- Çiçekler "net kontürlü, tekrarlanan sade biçimlere" indirgenip ritmik düzene konuyor (Wikipedia). Kompozisyon: ortada bir lale, çevresinde halkalar; bir karanfil laleyi sarıyor; en dışta erik çiçeği (Courtauld).

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Kapalı = lale tomurcuğu, açık = karanfil.** Kapalı hal: sivri uçlu, 3 görünür yapraklı lale/tomurcuk siluet (dar, yüksek, "kapalı yumruk" yerine "keskin taç"). Açık hal: top-down karanfil, kenarı 3-4 diş tırtıklı yapraklar. Siluet farkı 1-2 sn ve CRT altında bile okunur; bu yönün en güçlü yanı. (Karanfilin tırtıklı kenarı benim bilgim; çekilen sayfalarda yazmıyor.)
2. **Kontür: kalın, koyu, tek tonlu, 4-5 piksel eşdeğeri (dünya biriminde ~0,18-0,25).** İznik'te siyah/koyu kontür çok renkli alanları ayırıyor (kaynak: siyah çizginin palete eklenmesi). Burada kontür node'un dünya içinden ayrılmasını da sağlar. Kontür rengi saf siyah yerine #2B1F4A gibi koyu mor-lacivert (karakterin #97338A gölgesinden ayrı tutmak için lacivert ağırlıklı).
3. **Katmanlı rozet + merkez:** Dış katman 6-8 geniş tırtıklı yaprak, iç katman 5 küçük yaprak (lale taç yaprağı gibi sivri). Merkez: küçük koyu halka içinde krem nokta (pop animasyonu için). "Saz" yaprağı olarak 2-3 uzun tırtıklı yaprak çiçeğin altından/dışından, kapalıyken çiçeğin etrafında sarılı, açılırken "süpürerek" dışarı açılır (rulo çözülme zaten vertex shader'da var).

### Renk ipucu (şekil odaklı)
Beyaz/krem yaprak gövdesi #FFF6E3, kobalt gölge #2B4BA8, turkuaz vurgu #2FB5B8, bole kırmızı merkez/uç #D9472B. Dikkat: kırmızı fırlatma pedine (#FC1E00) yakın olduğu için kırmızıyı yalnız küçük merkez/uç vurgusunda tut.

### Riskler
- Kobalt/turkuaz gökyüzü mavisine ve mevcut teal/koyu mavi node'lara yakın; çiçek gövdesi mavi olmasın, yalnızca gölge/kontür olsun.
- 3-4 diş CRT'de gürültüye dönmesin diye diş sayısı az, derin tutulmalı.
- Shader'da zor değil: yaprak SDF'ine kenar boyunca `sin` ile diş eklenir. Lale tomurcuğu için mevcut yaprak ızgarasını kapalıyken yukarı doğru toplayan açı parametresi yeterli olur.

---

## 2. Vitray / Tiffany: kurşun çizgi (came)

### Doğrulanmış bağlantılar
- Wikipedia, Stained glass: https://en.wikipedia.org/wiki/Stained_glass (okundu; ilk 100.000 karakter)
- Wikipedia, Tiffany glass: https://en.wikipedia.org/wiki/Tiffany_glass (okundu; ilk 100.000 karakter)
- Wikipedia, Art Nouveau (Tiffany/Gallé anılıyor): https://en.wikipedia.org/wiki/Art_Nouveau (okundu)

### Kaynaktan doğrulanan
- Küçük renkli parçalar H kesitli kurşun şeritlerle (came) tutulur, bunlar renk alanları arasında koyu çizgi oluşturur; ayrıca fırınlanmış siyah cam boyası ince çizgileri çizer (Stained glass).
- Cam, metal oksitlerle tüm kütlesinde boyanır; mücevher renkler. Pencereler ışığı denetlemek ve "aydınlatılmış duvar süsü" olmak için yapılmış (Stained glass).
- Tiffany: opak/opalesan, benekli, çizgili camlar; yaprak ve taç yaprak dokusu için ayrı cam türleri (Tiffany glass).

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Cel-shade bantlarının yerine cam parçaları.** Her yaprak 3-4 parçaya bölünür (taban, gövde, uç; arada 1-2 damar yerine kalın "came" çizgisi). Her parça düz bir tek renk. Bu, zaten kullanılan 2-3 ton bandının yapısal karşılığı; outline + damarlar tek bir "came" dilinde birleşir.
2. **Durum = arkadan ışık.** Kapalı: parçalar donuk/koyu, came çizgileri baskın (karanlık cam). Açık: parçalar en parlak ton, merkezden dışa doğru hafif ışıma (şu anki şeftali taban parıltısının mantıklı devamı). Uyarı: parçalar sırayla yanıp sönerek (rastgele sıralı) titrer; kapanışta hepsi birden söner. Yani durumu renk değil parlaklık taşır, karakterin pembesine bağımlılık azalır.
3. **Merkez: gül pencere "göbeği".** Küçük daire içinde 5-6 dilimli basit bir cam diski; dış kontür kalın came. Yaprak sayısı 8 (gül pencere mantığı: yarım tur simetrik, okunur).

### Renk ipucu
Cam tonları doygun ama koyu kontürle çevrili olduğu için pastel yerine "mücevher" tonlar: rubin #D8325F, kehribar #F5A623, zümrüt #1FA67A, kobalt vurgu #3A4FB0. Came rengi #1D1530.

### Riskler
- Magenta-pembe bir cam parçası yine karaktere yakın görünür; rubin/kehribar/zümrüt ağırlığı pembeden ayırır.
- Shader'da kolay: yaprak UV'sini 3-4 banda bölen `step` çizgileri; ışıma additive bir terim.
- Çok ince came CRT'de kaybolur; kalınlığı outline ile aynı veya biraz üstü tutulmalı.

---

## 3. Baskı kayması / overprint: Warhol Flowers ve risograf

### Doğrulanmış bağlantılar
- Wikipedia, Flowers (Warhol): https://en.wikipedia.org/wiki/Flowers_(Warhol) (okundu)
- Wikipedia, Risograph: https://en.wikipedia.org/wiki/Risograph (okundu)

### Kaynaktan doğrulanan
- Warhol: hibiscus fotoğrafı büyütülüp fotokopiyle renksizleştirilerek yalnız çiçek silueti ve pistil konturları bırakıldı; bu şablonlarla gövdeler el ile ya da sprey ile Day-Glo renklerle boyandı; üstüne siyah ekran basıldı; ara sıra kayıt (registration) kayması baskıya "tahta baskı" kabalığı verdi (Warhol Flowers).
- Risograf: spot renk, her renk ayrı geçiş; mürekkepler yarı saydam, üst üste binince yeni tonlar doğar; sanatçılar hizasızlığı kasten kullanır; floresan pembe/mavi/sarı standart mürekkepler arasında (Risograph).

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Dolgu ve kontür ayrı "plaka".** Çiçeğin renk dolgusu, kontür/damar plakasından birkaç piksel kaydırılmış dursun (sabit küçük kayma: ~0,06 dünya birimi). Bu, "baskı" hissini veren tek bir hareket; kontür yerinde, renk kaymış.
2. **Durum = kayma miktarı.** Kapalı: kayma 0 (dolgu kontürün içinde tam oturur, "sabit/güvenli"). Açık: hafif kayma. Uyarı: kayma 3-4 katına çıkıp rastgele titrer (renk plakası kontürden ayrılıp sarsılıyor). Snap kapanış: plaka tam oturur. Uyarı, "gerçek bir titreme" olarak değil, "baskının dağılması" olarak okunur.
3. **İki katmanlı çiçek = iki mürekkep, çarpma (multiply) ile.** Mevcut 2 katmanlı yapraklar zaten üst üste; ön katman #F5C542 (sarı), arka katman #2FB5B8 (teal) gibi iki yarı saydam mürekkep olsun; kesişim kendiliğinden üçüncü bir ton (yeşilimsi koyu) verir, böylece 2-3 sert ton bandı "bedavaya" gelir. Merkez: tek renk disk, kontür plakasında küçük "+" kayıt işareti benzeri bir iz (isteğe bağlı).

### Renk ipucu
Floresan pembe karaktere çok yakın; bu yönde pembe KULLANMA. Sarı + teal + turuncu üçlüsü (risograf standart mürekkep listesinde teal, sarı ve turuncu var).

### Riskler
- Kayma küçük dünya biriminde CRT altında bulanıklık sanılabilir; kayma değeri sıfır/tam durum ayrımı net olmalı.
- Kontür plakasını koyu yapmak karakterin koyu gölgeleriyle birleşmez; fakat yarı saydam multiply, gökyüzü mavisi üstünde renk kaydırabilir (mavi zemin + sarı = yeşilimsi). Multiply'ı yalnız çiçeğin kendi katmanları arasında kullan, zemine karıştırma.
- Shader'da çok kolay (iki ayrı SDF örnekleme, UV ofset).

---

## 4. Matisse kağıt kesmeleri (gouaches découpées)

### Doğrulanmış bağlantılar
- Tate, Matisse cut-outs: https://www.tate.org.uk/whats-on/tate-modern/exhibition/henri-matisse-cut-outs (okundu)
- Wikipedia, Henri Matisse (kesme bölümü): https://en.wikipedia.org/wiki/Henri_Matisse (okundu; kesme bölümü yarım)
- MoMA sergi sayfası (bilgi için arandı): erişilemedi, "Doğrulanamayanlar"a bak.

### Kaynaktan doğrulanan
- Hastalık sonrası Matisse boyamaya güçlükle yetiyordu; yardımcılar gouache ile boyanmış kağıt hazırladı, o da bunları makasla kesip kompozisyon yaptı (Wikipedia). Tate bunu "renge oyarak çalışmak" (carving into colour) diye anlatıyor ve eserlerin arasında "snowflowers"ı (kar çiçekleri) sayıyor.
- Eserler resim ile heykel arası, katmanlı ve üç boyutlu bir his veriyor (Wikipedia).

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Kontürsüz, el kesimi siluet.** Outline yok; çiçek tamamen düz renk. Yaprak kenarına düşük frekanslı gürültüyle küçük, kasıtlı "makas sapması" ver; her yaprağın şekli biraz farklı (yaprak indeksine göre sabit tohum). Çiçek "kesilmiş" gibi görünür, hesaplı değil.
2. **Negatif boşluk okunur.** Yapraklar arasında bilerek ince ama net boşluk bırak; arkadaki gökyüzü görünsün. Açılışta boşluklar büyür, kapanışta kapanır (kapalı hal: boşluksuz tek, yaprak şeklinde yumru, "kesilmiş yosun/yaprak" gibi). Böylece açık/kapalı fark kontürsüz de siluetle okunur.
3. **5-7 yaprak, asimetrik dağılım, 2 renk.** Büyük yapraklar tek renk (#FFF6E3 krem ya da #FF8A3D turuncu), merkez küçük, tek zıt renk "kesik" bir kağıt parçası (düzensiz, elmas/oval değil). Gölge bandı yok ya da tek bir koyu ton ikinci kesilmiş parça olarak yapılır (gölgeyi "ikinci kağıt" gibi üst üste koy).

### Riskler
- Kontürsüz çiçek gökyüzü mavisine karşı krem/turuncu ile okunur ama aynı ışıkta karakterin pembesiyle ayrılması için pembe KULLANMA; krem+turuncu+sarı daha güvenli.
- CRT'nin kenar bulanıklığı el kesimi sapmasını yutabilir; sapma yeterince büyük (yaprak genişliğinin ~%8'i) olmalı.
- Shader'da kolay (SDF + noise), ama "her yaprak farklı" için tohum başına parametre gerekir.

---

## 5. Kamon: kiku (krizantem) ve ters çevirme

### Doğrulanmış bağlantılar
- Wikipedia, Mon (emblem): https://en.wikipedia.org/wiki/Mon_(emblem) (okundu; ilk 100.000 karakter)
- Wikipedia, Imperial Seal of Japan: https://en.wikipedia.org/wiki/Imperial_Seal_of_Japan (okundu)
- Columbia JAANUS, kikkamon: https://projects.mcah.columbia.edu/jaanus/node/3071 (okundu; yalnız çatı kiremiti kullanımı)

### Kaynaktan doğrulanan
- Mon özünde tek renklidir; renk tasarımın parçası değildir. Siyah-beyaz "kage mon" gayri resmi, beyaz-siyah "hinata mon" resmi; tasarımı ters çevirmeye "jinuki" denir, çoğu mon her iki yönde de çalışır (Mon).
- İmparatorluk mühürü: iki katman yaprak; ön 16, arka 16, arka katman kaydırılmış, her biri iki ön yaprağın arasında; ortada merkez disk. Genelde sarı/turuncu ve siyah/kırmızı kontür. Alt aile üyeleri 14 tekil yaprak, bazı resmi kullanımlar 16 tekil yaprak kullanır (Imperial Seal).
- Kiku/paulownia en yaygın motiflerden; imparator mührü 16 yapraklı, daha az yapraklılar alt üyelerce kullanılır (Mon). Kiku motifi Meiji'den sonra imparator mührü olduğu için kullanımı kısıtlandı (Columbia, kiremit bağlamında).

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Çift katman, kaydırılmış (yarım adım) yaprak.** Arka katman ön katmanın yaprak aralarına oturur; iki katman "ön 8 + arka 8" gibi sadeleştirilmiş. Zaten 2 katmanlı olan mevcut çiçek için doğal bir yapı; tek farkı arka katmanın tam yarım adım kayması ve uçlarının ön yaprakların arasından görünmesi.
2. **Durum = jinuki (ters çevirme).** Kapalı hal: koyu zemin üzerine açık tek renk "silüet" (hinata gibi, ters). Açık hal: açık yapraklar koyu kontürle (kage gibi). Uyarı: iki hal arasında hızlı yanıp sönen ters çevirme. Durum yalnızca açık-koyu kontrastı ile değişir, renk gerekmez; CRT altında en okunur sinyallerden biri.
3. **Merkez: tek disk (maru).** Düz küçük daire, pop animasyonunda halka olup genişler. İç damar yok, parlama çizgisi yok; sadelik kamon'un özü.

### Riskler (önemli)
- 16+16 yapraklı, sarı/turuncu + siyah kontürlü çiçek, imparatorluk mührünün neredeyse aynısıdır ve kültürel olarak hassastır. Bu yüzden 8+8 veya 7+7 ve farklı renk kullan; birebir kopya yapma.
- Çok ince yapraklar (16) 9 birimlik çiçekte CRT'de bulanır; 8 sade yaprak yeterli.
- Shader'da kolay.

---

## 6. Papel picado (Meksika kesme kâğıdı)

### Doğrulanmış bağlantılar
- Horniman, Papel picado: https://www.horniman.ac.uk/story/papel-picado-mexicos-festive-paper-art/ (okundu)
- Wikipedia, Papel picado: https://en.wikipedia.org/wiki/Papel_picado (okundu)

### Kaynaktan doğrulanan
- İnce ipek/doku kâğıdına desen kesilir; kesilen yerler görünen deseni oluşturur; motifler çiçek, kuş, haç, kafatası vb. (Horniman). Renk anlamlıdır: mor/siyah ölüm, turuncu/sarı kadife çiçeği (marigold) (Horniman).
- Kâğıt hafif ve kırılgandır, sokaklara/ofrendalara asılır, rüzgârda kıpırdar ve geçiciliği hatırlatır (Horniman).
- Kalıpla 40-50 kat üst üste istif, keskilerle kesme; ya da katlayıp makasla kesme (Wikipedia).

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Delik işlemeli yapraklar.** Her yaprağın içinde 2-3 küçük damla/oval delik; gökyüzü/arka plan görünür. Yaprak "kesilmiş kâğıt" gibi, düz tek renk, ince kontür yok (kenar zaten desenin kendisi).
2. **Uyarı = pinked (zikzak) kenar + kıpırdanma.** Normal hâlde yaprak kenarı yumuşak; uyarıda kenar zikzaka döner ve yaprak rüzgârdaki kâğıt gibi hafif sallanır (Horniman'ın "kıpırdama" tarifi doğrudan dayanak). Okunurluğu yüksek: şekil değişimi + hareket.
3. **Kapalı = deliksiz, tek kat dolu "katlanmış kâğıt".** Açıldıkça deliklerin ortaya çıkması "kâğıdın açılması" hissi verir.

### Renk ipucu
Kadife çiçeği turuncusu #FF8A1F + sarı #FFC72C, kâğıt yoğun, tek renk. Dikkat: turuncu ve sarı kanca node'larıyla aynı palet; farklılığı şekilden (delikler) almalı.

### Riskler
- Deliklerden gökyüzü mavisi görünür; sarı/turuncu yaprak üzerinde kontrast iyi, ama bulanık CRT'de küçük delikler kapanır; delik çapı yaprak genişliğinin en az %15'i olmalı.
- Shader'da orta zorluk: delik SDF çıkarmaları ve alpha clip; uyarı kenarı için açısal `abs(sin)`.

---

## 7. Murakami gülen çiçek (merkezde yüz)

### Doğrulanmış bağlantılar
- Culted, Murakami çiçekleri: https://culted.com/takashi-murakamis-flowers/ (okundu; az bilgi)
- Zarastro, Murakami çiçeği: https://zarastro.art/takashi-murakami-flower/ (okundu; az bilgi)

### Kaynaktan doğrulanan (sınırlı)
- Çok renkli, gülen çiçek; Nihonga çiçek resimlerinden doğdu ve Murakami nihai tasarıma varmadan önce 50'den fazla kez yeniden çizdi (Culted). Gözleri kapalı, geniş sırıtışlı yüz; canlı renkler, çarpıcı renkli zeminler; birçok çiçek tek sapta (Zarastro). Superflat hareketiyle ilişkilendirilir (Zarastro).
- Yaprak sayısı, kontür kalınlığı ve düz renk bilgisini sayfalar vermedi; bunlar için "Doğrulanamayanlar"a bak.

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Yüz yalnızca merkezde ve durum bilgisi taşır.** Kapalı: gözler kapalı, ağız düz çizgi (uyuyor). Açılış: gözler açılır (pop anı). Açık: geniş gülüş. Uyarı: ağız dalgalanır/gözler şaşkın. Kapanış: gözler sıkıca kapanır. Çocuklar dahil herkes yüzü 1 saniyede okur.
2. **Yapraklar basit, tekrar eden damla; yüz hariç iç damar/parlama yok.** Yüzün sade işaretleri (iki nokta göz, tek yay ağız) yaprak çizgilerinden ayrı kalmalı.
3. **Yaprak sayısı 8-12, hepsi aynı şekil, hafif renk kaydırmalı** (her yaprağa bir ton). Yüzün zemini düz sarı disk.

### Riskler
- Oyun tonuna uymayabilir; yüzlü çiçek "düşman" veya "NPC" gibi okunabilir ve mekanik "tutunulabilir" mesajı yerine "bakıyor/tehdit" mesajı verebilir. Bu yönü geliştiriciye açıkça sor.
- Karakter şeker, merkezi yüzlü çiçek karakterle yarışabilir.
- Shader'da kolay-orta (SDF yüz).

---

## 8. Hint kalamkari (önce kontür, sonra dolgu)

### Doğrulanmış bağlantılar
- Wikipedia, Kalamkari: https://en.wikipedia.org/wiki/Kalamkari (okundu)

### Kaynaktan doğrulanan
- Machilipatnam/Pedana tarzı: iç içe yaprak ve çiçekler, lotus çeşitleri, kıvrık dallar (creeper), papağan/tavus, çark; "yaşam ağacı" popüler (Wikipedia).
- Doğal boyalar: kırmızı, siyah, kahverengi, mor alanlar mordan ile; alizarin ile kırmızı; çivit ile mavi; kuru myrobalan çiçeğiyle sarı (Wikipedia).
- Teknik: kontür mordanla (çoğu zaman blokla); çivitte boyanmayacak alanlar mumla örtülür, boyadan sonra mum kazınır, kalan alanlar elle boyanır; sivri bambu/hurma kalemi (kalam) ince saç demeti ile kontür çizer (Wikipedia).

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Kontür önce, dolgu sonra: dolgu "taşar".** Kontür çizgisi kesin ve koyu kahverengi-kırmızı; dolgu, çizginin hemen dışına çok az taşar (el boyama toleransı). Fark 3. yön (baskı kayması) ile benzer; burada kayma yok, taşma var.
2. **Lotus katmanı: yapraklar 3 sıra, içten dışa küçülen sivri uç.** Her sıra farklı tek renk; kapalı hal: dış sıra kapalı, iç iki sıra görünmüyor.
3. **Merkez: küçük "çark" (8 uçlu yıldız) ya da yarım daire bandı.** Hint tekstilinde çark motifi doğrulanmış; ancak 8 uçlu yıldız geometrik soyuta kaçabilir, tek halka yeter.

### Riskler
- Kalamkari tarzında sık iç ayrıntı çoktur; CRT'de kaybolur. Sadece "kontür + 3 sıra yaprak" kısmı alınmalı.
- Doğal boya paleti (kırmızı #A63C2A, çivit #2C3E7A, hardal #D8A32A) oyunun canlı paletinden çok mat; kısmen doygunlaştırmak gerekir.
- Shader'da orta zorluk (taşan dolgu için ikinci SDF).

---

## 9. Ukiyo-e kacho-e ve hanafuda (anahtar çizgi, düz renk, bokashi)

### Doğrulanmış bağlantılar
- Wikipedia, Ukiyo-e: https://en.wikipedia.org/wiki/Ukiyo-e (okundu; ilk 100.000 karakter)
- Wikipedia, Hanafuda: https://en.wikipedia.org/wiki/Hanafuda (okundu)
- Wikipedia, Bird-and-flower painting (kacho-ga): https://en.wikipedia.org/wiki/Bird-and-flower_painting (okundu)

### Kaynaktan doğrulanan
- Ukiyo-e'de dış hat (outline) baskın biçim; Hokusai kalın, düz, soyut renk alanları kullandı; Lautrec düz renk ve konturlu biçimleri beğendi (Ukiyo-e). Renk başına ayrı blok; 1760'larda on ve üstü blok yaygın; baskıcı tek blokta renk geçişi (bokashi) yapabiliyordu (Ukiyo-e).
- 1841-43 Tenpō reformlarından sonra birçok sanatçı kuş-çiçek konusuna yöneldi; Hiroshige bunda uzmanlaştı (Ukiyo-e).
- Hanafuda: her ay bir çiçek (çam, erik, kiraz, wisteria, süsen, şakayık, ... kasımpatı, akçaağaç, söğüt, paulownia); her ay 4 kart; arka yüz düz kırmızı ya da siyah; çiçek renkleri Edo orta döneminde soluk, sonra koyu (Hanafuda).
- Kacho-ga, Muromachi'den beri bir tür; shin-hanga Koson, Keinen gibi sanatçıları getirdi (Bird-and-flower painting).

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Tek bir bokashi: yalnızca yaprak tabanında.** Düz renk + kontür; tek gradyan, yaprağın tabanından ortasına kadar. Şu anki "şeftali taban parıltısı"nın baskı diliyle yeniden yazılması: yumuşak ışıma değil, baskıcının tek hamlede yaptığı gradyan.
2. **Hanafuda "kart çiçeği" siluet sözlüğü:** sakura (5 yaprak, ucu çentikli), şakayık (çok katlı, yuvarlak), süsen (3 büyük, 3 küçük). Bu çiçeklerden biri alınıp "kart gibi" sade çizilir. (Yaprak şekilleri benim bilgim; çekilen sayfalar şekil anlatmıyor.)
3. **Kapalı hali "kart arka yüzü" gibi düz tek renk kapalı yuvarlak** (hanafuda arka yüz mantığı): kapalıyken tek renk koyu bir daire, açık hal "kart yüzü" gibi renkli ve desenli. Bu bir çıkarımdır; hanafuda'da kapalı/açık kavramı yoktur.

### Riskler
- Sakura tipik pembe; karakterin pembesine doğrudan çarpar. Sakura alınırsa pembe değil krem-beyaz ve koyu kontür kullanılmalı.
- Bokashi gradyanı CRT altında bandlaşır; zaten bant hedefleniyor ama gradyan keskin 3 adıma bölünebilir.

---

## 10. Aizuri-e ve katazome (tek hue + tek kırmızı vurgu)

### Doğrulanmış bağlantılar
- Wikipedia, Aizuri-e: https://en.wikipedia.org/wiki/Aizuri-e (okundu)
- Wikipedia, Katazome: https://en.wikipedia.org/wiki/Katazome (okundu)

### Kaynaktan doğrulanan
- Aizuri-e: tamamen ya da çoğunlukla maviyle basılmış; ikinci renk görünürse genellikle kırmızı. Prusya mavisi 1820'lerde Avrupa'dan geldi, daha parlak ve geniş ton aralığı veriyor, derinlik hissinde iyi (Aizuri-e). Ton farkı, aynı tasarımın bazı kısımlarının birden çok baskısı ya da bokashi ile (Aizuri-e).
- Katazome: şablon + pirinç unu direnç macunu; macunun geçtiği yerler boyayı almaz; şablon tekrar kullanıldığı için motif tekrar eder; ince kumaşta desen arka yüze geçer (Katazome).

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Tek hue + tek kırmızı nokta kuralı.** Çiçeğin tüm gövdesi tek bir renk ailesinden 3 ton; yalnız merkez noktası ters/zıt (kırmızı ya da sıcak). Durum: kapalıyken kırmızı nokta YOK (tomurcuk tek hue), açılış pop anında nokta belirir; açık süresince nokta sabit; uyarıda nokta yanıp söner. "Kırmızı = tutunulabilir" tek sinyal olur.
2. **Şablon (stencil) negatif yaprak:** yaprağın içinde ince "direnç" damarlar açık bırakılmış (ışık rengi), gövde koyu; damarlar birkaç tane, tek yönde.
3. **Taban rengi mavi OLMAMALI:** gökyüzü #71BAEA ile kaybolur. Aynı kuralı sıcak bir hue ile uygula (ör. turuncu ailesi + tek teal nokta) ya da çok koyu mor-lacivert + krem.

### Riskler
- Mavi aileyi seçmek gökyüzü ve teal/koyu mavi node'larla çakışır; sıcak ters çevrimi şart.
- Shader'da kolay.

---

## 11. Art Nouveau: Gallé cameo katmanları, Mucha çerçeveleri

### Doğrulanmış bağlantılar
- Wikipedia, Émile Gallé: https://en.wikipedia.org/wiki/%C3%89mile_Gall%C3%A9 (okundu)
- Wikipedia, Art Nouveau: https://en.wikipedia.org/wiki/Art_Nouveau (okundu)
- Wikipedia, Alphonse Mucha: https://en.wikipedia.org/wiki/Alphonse_Mucha (okundu; ilk 100.000 karakter)

### Kaynaktan doğrulanan
- Gallé: iki ya da daha çok renkli cam katmanı, dış katman oyularak altındaki renk görünür; gerçek bitkilerden çalışıldı, iris/orkide/begonya gibi isimli vazolar (Gallé).
- Art Nouveau: bitki ve çiçeklerin kıvrımlı eğrileri; "kırbaç çizgisi", asimetrik, hareketli kompozisyon; Mucha afişlerinde süslü çerçeveleme ve bitkiyle kaynaşmış figürler (Art Nouveau).
- Mucha: figürün başının arkasında gökkuşağı biçimli süslü kemer/hale; soluk, pastel renkler (Mucha).

### Bu oyuna aktarılacak 3 somut fikir (çıkarım)
1. **Cameo mantığıyla açılış:** Yaprağın dış rengi, "oyularak" altındaki rengi gösterir. Açılışta rulo çözülürken dış kabuk rengi geri çekilip iç renk (parlak) ortaya çıkar; kapalı hal dış kabuk rengi (donuk koyu), açık hal iç renk (parlak). Durum rengi, yaprağın "içi/dışı" ile verilir.
2. **Asimetrik "kırbaç" yaprak ucu:** yaprak kenarlarından biri hafif S-eğrisi (S-kıvrımı), tüm yapraklar aynı yöne eğik. Tek merkezden dışa dönen bir his, ama spiral olmayacak kadar hafif (eğim 10-15°).
3. **Mucha halesi:** çiçeğin arkasına yalnız üst yarıda yarım ay biçimli koyu şerit, kameraya karşı kontrast sağlar. (Mucha sayfası hale'yi "kemer" diye anlatıyor.)

### Riskler
- S-kıvrım fazla olursa gözde spirale kayar; geliştiricinin "spiral artık yok" kuralına çarpar. Eğim çok düşük tutulmalı.
- Pastel Mucha paleti karaktere karışır, kaçın.
- Cameo shader'ı: iki ton lerp + kenar maskesi; kolay-orta.

---

## 12. Gzhel (kobalt fırça darbesi)

### Doğrulanmış bağlantılar
- Wikipedia, Gzhel: https://en.wikipedia.org/wiki/Gzhel (okundu)

### Kaynaktan doğrulanan
- Geleneksel olarak beyaz zemin üzerinde mavi desen; 1945-49 canlanmasından sonra kobalt boya beyaz kile uygulandı. Usta A. B. Saltykov üslubu korumak için fırça darbelerinin atlasını derledi (Gzhel). Çiçek motifini ve tonlu geçişi sayfa doğrulamadı.

### Bu oyuna aktarılacak fikirler (çıkarım, kısa)
- Yaprak = tek "fırça darbesi" damla; uç tarafı açık, taban koyu (tek hue, 2 ton). Kontürsüz, beyaz zemin üstünde kobalt; ancak gökyüzü mavisi + kobalt çakışması yüzünden yalnız yardımcı fikir (ör. kobalt GÖLGE bandı, beyaz-krem gövde).

---

## Elenenler (öneri değil)
- **Khokhloma** (https://en.wikipedia.org/wiki/Khokhloma, okundu): kırmızı/siyah/altın, kıvrık dal-meyve-yaprak; kıvrımlar spiral'e yakın ve "folk" ağırlıklı, kullanıcının tükettiği çizgiye giriyor. Doğrulanan tek yeni fikir: "gold silhouette on colored ground" (arka plan yöntemi), şimdilik bırakıldı.
- **Kusama** (https://en.wikipedia.org/wiki/Yayoi_Kusama, okundu): nokta/polka desenli çiçekler halftone/op-art çizgisine çok yakın.
- **William Morris** (https://en.wikipedia.org/wiki/William_Morris, okundu): sayfa desen/çiçek şekli ayrıntısı vermedi; sürekli desen dili tek çiçek node'una aktarması zor.
- **Memphis** (https://en.wikipedia.org/wiki/Memphis_Group, okundu): çiçekle doğrudan ilişkisi yok; terrazzo/üçgen dili geometrik soyut çizgisine yakın.
- **Alebrije/Oaxaca** (https://en.wikipedia.org/wiki/Alebrije, okundu): iki aşamalı boyama (taban + çok renkli üst desen) fikri var, ama çiçek yok; "folk" ağırlıklı.
- **Sumi-e** (https://en.wikipedia.org/wiki/Sumi-e, okundu): mürekkep geçişi pürüzsüz vektör/cel-shade hedefine ters; "en az çizgi" ilkesi ilginç ama aktarım kolay değil.

---

## Öneri (benim sıralamam)

En güçlü 5 yön:
1. **İznik Quatre Fleurs (lale tomurcuğu → karanfil):** Kapalı ve açık halin siluetini tamamen değiştirerek durumu renge bağlamadan okutur; en güçlü okunurluk kazancı.
2. **Vitray (came çizgisi):** Cel-shade bantlarını cam parçalarına, durumu da "arkadan ışık" parlaklığına çevirir; karakterin pembesine bağımlılığı azaltır.
3. **Baskı kayması / overprint:** Dolgu-kontür kayması uyarı durumunu kendi başına sinyal yapar; iki katman multiply ile ücretsiz üçüncü ton verir.
4. **Matisse kağıt kesmesi:** Kontürsüz el kesimi siluet ve net negatif boşluk; en farklı görünüm, shader'da en ucuz.
5. **Kamon ters çevirme (jinuki):** Durumu yalnız açık-koyu kontrastıyla taşır; CRT altında çok dayanıklı (ama imparatorluk mührünü birebir kopyalama).

Öne çıkardığım 3 yön:
- **A) İznik lale → karanfil siluet geçişi** (şekil).
- **B) Vitray came + arkadan ışık** (boya dili ve açık durum).
- **C) Baskı kayması ile uyarı durumu** (uyarı sinyali).

Üçü birbirini dışlamaz: İznik şekil (siluet ve diş sayısı), vitray boya (parçalı bantlar + came), baskı kayması da yalnızca uyarı anının animasyonu olarak katmanlanabilir. Renk tarafında ortak kural: gövde gökyüzü mavisinden ve karakter pembesinden uzak (krem/turuncu/sarı/rubin/zümrüt ağırlığı), koyu kontür mor-lacivert tonlu.

---

## Doğrulanamayanlar

Erişilemeyen sayfalar:
- DIA İznik tabağı (https://dia.org/collection/dish-carnations-tulips-roses-and-saz-leaf-96236): HTTP 403.
- Web-Japan kamon (https://web-japan.org/trends/11_fashion/fas181202.html): HTTP 403.
- MoMA sergi sayfası (https://www.moma.org/interactives/exhibitions/2014/matisse/the-cut-outs.html): HTTP 403. MoMA PDF (matisse-cut-outs-artworks.pdf): içerik okunamadı (ikili akış).
- Met Museum (Snow Flowers, https://www.metmuseum.org/art/collection/search/490000 ve İznik essay'i): HTTP 429.
- Wikipedia "Katagami" (şehir sayfası çıktı, el sanatı değil), "Jazz (Matisse)" 404, V&A katagami 404, Paulownia emblem 404, Wikipedia "Cut-outs (Matisse)" 404.

Doğrulanamayan ya da yalnızca arama özeti düzeyindeki iddialar (referans kabul etme):
- Katagami şablonlarında "köprü" (bridge) bağlantıları, hurma/kaki tanenli dut kâğıdı, ipek iplik desteği: sayfa bulunamadığı için doğrulanmadı. Fikir 10'daki "direnç damarları" bu nedenle yalnız katazome (doğrulandı) mantığına dayanıyor.
- Matisse'in "makasla çizmek" (drawing with scissors) sözü ve "kesikler kütleyi çevreler" yorumu (Hyperallergic): arama özetinde göründü, sayfa metni (yalnız giriş okundu) doğrulamadı.
- Met "Snow Flowers" tarifi (beyaz bitki/yaprak biçimleri, renkli yama zemin): yalnız arama özetinde.
- Murakami çiçeğinin 12 yapraklı olması, kalın siyah kontür ve düz tek renk: yalnız arama özetlerinde; açılan iki sayfa bunları doğrulamadı.
- Mucha'nın kalın kontür ve düz renk kullanımı: sayfa doğrulamadı (yalnız pastel renk ve hale/kemer doğrulandı).
- Tiffany'nin bakır folyo tekniği: sayfa ayrıntı vermedi.
- Sumi-e "Dört Beyefendi" konuları, tek vuruş yaprak tekniği: sayfa yarım kaldı.
- Kamon yapımında bambu pergel (bun-mawashi) ile daire çizimi: yalnız arama sonucu özetinde; Sydney Üniversitesi sayfası açılmadı.
- Hanafuda kartlarının düz renk + kalın kontür görünümü: Wikipedia sayfası bunu anlatmıyor.
- Karanfilin tırtıklı yaprak kenarı, sakura yaprağının ucu çentiği, süsenin 3+3 yapısı: çekilen hiçbir sayfa şekli tarif etmiyor; bunlar benim görsel bilgim.
- "Kapalı/açık" veya "durum" ile ilgili tüm somut öneriler (ters çevirme, kayma, cameo, arka ışık vb.): kaynaklarda yok; benim tasarım çıkarımlarım.

## Yöntem notu

- WebSearch ile 5 tarama yapıldı, fakat arama özetleri güvenilir kabul edilmedi; her iddiayı WebFetch ile açtığım sayfa metnine karşı kontrol ettim ve sayfa söylemiyorsa "doğrulanamayan" saydım.
- WebFetch, kısa bir model aracılığıyla sayfa özeti veriyor; ham metin göremedim. Uzun Wikipedia sayfalarında yalnız ilk 100.000 karakter okundu; yeniden okuma yapmadım. Bu yüzden her kaynak için "okundu" ifadesi, ilgili soruya sayfanın verdiği cevabı ifade ediyor, sayfanın tamamını değil.
- Kaynaklarda şekil ayrıntısı (yaprak sayısı, diş sayısı, kontür kalınlığı) çoğunlukla yok; sayısal öneriler (dünya birimi kalınlıkları, kayma oranı, delik çapı %'si) benim tahminim ve Unity'de görsel testle ayarlanmalı.
- Görsel örneklere (resimlere) bakamadım; yalnız metin kaynaklarından çalıştım. Her yön için geliştirici birkaç referans görseli (Iznik quatre fleurs tabakları, Tiffany lambaları, Matisse snow flowers, papel picado, Warhol Flowers) elle açıp bakmalı; bu rapordaki şekil önerileri görsel doğrulamadan geçmedi.
- Telif: Hiçbir kaynaktan alıntı yapılmadı; hepsi kendi cümlelerimle özetlendi.
