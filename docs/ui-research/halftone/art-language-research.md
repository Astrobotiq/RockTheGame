# Halftone hareketli platformlar: sanat dili araştırması

Tarih: 2026-11-13. Kapsam: hareketli platformların (`PlatformController` / `TriggeredPlatformController`) halftone / Ben-Day nesnelere dönüşmesi, hareketlerinde renkli nokta izi (trail) bırakması. Aşama 2: oyuncu üstündeyken noktalar oyuncuya toplanır, renk oyuncudan dışarı doğru kayar (bu belgede yalnızca sanat dili açısından değerlendirildi).

**Sabit kısıt:** piksel sanat gibi durmayacak. GDD §8'deki "basit pixel art" satırı bu özellik için bırakıldı (geliştirici kararı). Piksel ızgarasına oturan nokta, nearest filtreleme, tam sayı konumlandırma yok. Düşünce çerçevesi: pürüzsüz baskı, mürekkep, kâğıt, üst üste baskı (overprint).

**Yöntem notu.** Web araması yapıldı, sayfalar tek tek açıldı. Aşağıdaki tabloya yalnızca içeriği okunarak doğrulanan kaynaklar girdi. İstenen listeden şu başlıklar için **halftone kullandığına dair doğrulanabilir kaynak bulunamadı**, bu yüzden referans olarak sayılmadı: Persona 5 (UI), Bomb Rush Cyberfunk, Neon White, Pizza Tower, Ultrakill, Arco, Gris, Trombone Champ, Viewfinder. Hi-Fi Rush ve Comix Zone için bulunan kaynaklar halftone'dan söz etmiyor (aşağıda "Doğrulanamayanlar"). Bu başlıkların "halftone kullanır" diye yazılması tahmin olurdu. Ayrıca `docs/moodboard/` görselleri bu çalışmada açılmadı; mevcut görünüm GDD §8.1 metninden ve `palette.md`'den alındı.

---

## 1. Referans tablosu (doğrulananlar)

| Başlık | Ortam | Çalınacak şey | Bağlantı |
|---|---|---|---|
| Spider-Man: Into the Spider-Verse, comp tarafı (Foundry Colorway) | Film / Nuke araçları | Noktalar **ışığa bağlı kalınlıkta**: aydınlıkta ince, gölgede kalın. Her çekimde nokta boyutu, açısı ve aralığı sanatçıda. Parlama ve glow'lar da noktayla yapılmış, noktaların "ışık gibi, doku gibi değil" durması için seyrek tutulmuş. Hızlı harekette noktalar kaydırılırsa karakter "desenin içinde yüzüyor" gibi görünüyor, bu yüzden kenar ışığı noktaları karaktere kilitlenebiliyor. Renk ofseti için ChromaShifter, derinlik yerine hareket vektörü verilince hareket izi çizgileri çıkıyor. | https://colorway.foundry.com/insights/film-tv/graphic-look-in-comp-spiderman |
| Into the Spider-Verse, yapım yazısı (AWN) | Film | Ofset baskı tekniğiyle "arkada kalan kenar" (trailing edge) yapıldı. **Twos**: her çizim 2 kare tutulur, saniyede 12 görüntü. **Motion blur kapalı**, her kare ayrı bir çizgi roman karesi gibi okunuyor. Animatör istediği an ones/twos arasında geçebiliyor. Mürekkep çizgileri rig'e gömülü, istisnalar elle yerleştirilmiş. | https://www.awn.com/animationworld/creating-stylized-universe-sonys-spider-man-spider-verse |
| Into the Spider-Verse (fxguide) | Film | Duman, kıvılcım, patlamalar sanatçı eliyle çizilmiş; renk tanımlı şekillere bölünmüş; halftone ve çapraz tarama baskısı. Patlamalar önce twos, sonra ones'a geçirilerek vurgu alıyor (arama sonucu özeti, sayfanın kendisi ayrıntı vermiyor). | https://www.fxguide.com/?p=244678 |
| Ben Day process (Wikipedia) | Baskı tarihi | **Ben-Day**: bir alanda eşit boyutlu, eşit dağılımlı nokta, çizgi sanatına uygulanır. **Halftone**: nokta boyutu değişerek ton oluşturur. Lichtenstein noktaları büyüttü ve abarttı. İç içe geçen camgöbeği + sarı noktalar orta yeşil veriyor (yani renk karışımı gerçekten baskı gibi, üst üste). | https://en.wikipedia.org/wiki/Ben_Day_process |
| Lichtenstein'ın noktaları (publicdelivery, madegooddesigns) | Pop art | Noktalar makineyle değil **elle boyanmış**, bu yüzden hafif düzensiz. Düz, kalın siyah dış çizgi + düz renk alan + nokta alan üçlüsü. (Arama özeti; sayfalar tam okunmadı.) | https://publicdelivery.org/what-is/ben-day-dots |
| Risograph baskı rehberleri (Moniker Press, Oxford Greenprint, Calverts) | Baskı | Her renk ayrı geçiş: renkler **şeffaf katman gibi üst üste biner**, tam hizalanmaz. Baskı sırası overprint derecesini belirler. Renk sayısı arttıkça kayma artar. Kayma kimi zaman "3B" bir etki yaratabilir. Halftone'lar dokuya kusurlu bir his katar. Dijital taklit önerisi: Multiply karışımı + gren. (Arama özeti; birincil sayfalar tam açılmadı, tasarım-aracı reklam sayfaları dışarıda bırakıldı.) | https://monikerpress.ca/printing |
| Halftone shader notları (Alexander Ameye) | Teknik not | Ekran uzayı UV ile tam ekran efekt. Her kanal (C/M/Y/K) için ayrı döndürme; moiré'yi önlemek için. Düzensizlik / gürültü parametresi. Not: `step` ile sert eşik kullanıyor, kenar yumuşatma yok. Bizde `smoothstep` + `fwidth` gerekir. | http://ameye.dev/notes/halftone-shader/ |
| Shades of Halftone (Maxime Heckel) | Teknik yazı | Nokta yarıçapı belli bir eşiği aşınca tek tek nokta değil "mürekkep/boşluk oranı" okunur. Eşiğin altında noktalar gürültü ve moiré kaynağı. Izgara dışı seçenek: Voronoi. | https://blog.maximeheckel.com/posts/shades-of-halftone/ |
| Return of the Obra Dinn dithering (Hackaday) | Oyun / teknik | Ekrana sabit desen, nesne hareket edince desenin nesnenin üstünde **kaydığı** hissi veriyor ("shower door"). Çözüm: noktaları nesneye bağlamak. Bizim durumumuzda platform hareketli, yani nesne uzayı şart. | https://hackaday.com/2022/07/04/dithering-makes-everything-cooler-now-even-animated |
| Sable, GDC 2022 özeti (Game Developer) | Oyun | Kontur çizgileri okunabilirlik ve derinlik için "çok faydalı araç". Üretim kısıtı (hızlı, tutarlı) sanat yönünü şekillendirmiş. Sade detay. İnceleme notu: palet düzleşince okunabilirlik zayıflayabiliyor. Not: Sable'da misregistration yok, yalnızca çizgi dili ve okunabilirlik için referans. | https://www.gamedeveloper.com/marketing/how-shedworks-refined-the-art-of-sable-in-pursuit-of-readability |
| Jet Set Radio (Wikipedia, arama özeti) | Oyun | Kalın siyah kontur; öne çıkan nesnelerde daha kalın çizgi (çizgi kalınlığı = vurgu). Halftone **kullanmıyor** (doğrulanmadı), yalnızca "kalın çizgili, düz renkli baskı gibi nesne" için referans. | https://en.wikipedia.org/wiki/Jet_Set_Radio |

### Doğrulanamayanlar (referans olarak sayılmadı)

- **Hi-Fi Rush:** 80.lv yazısı (deferred toon renderer, çizgi film shader'ı) halftone, nokta, tarama, twos'tan hiç söz etmiyor. Halftone iddiası yalnızca bir hayran blogunda geçiyor. Halftone referansı olarak kullanma.
- **Comix Zone:** Kaynaklar çizgi roman çerçevesini (panel, balon, el çizimi taranmış sanat) anlatıyor, nokta/halftone'u doğrulamıyor. Panel dili için ilham olabilir, halftone için kanıt yok.
- **Persona 5, Bomb Rush Cyberfunk, Neon White, Pizza Tower, Ultrakill, Arco, Gris, Trombone Champ, Viewfinder:** halftone kullanımı doğrulayan kaynak çıkmadı. Persona 5 için bulunan kaynaklar yalnızca baskın renk (kırmızı-siyah-beyaz) ve kes-yapıştır estetiğini anlatıyor. Bunları incelemek istenirse birincil kaynak (sanat kitabı, GDC, geliştirici röportajı) aranmalı.
- **Spider-Verse misregistration:** Foundry yazısı "misregistration"ı doğrudan anlatmıyor. Doğrulanan tek şey ofset baskı tekniğiyle yapılan arka kenar (AWN) ve ChromaShifter (Foundry). "Katman kayması" olarak bilinen görünüm için doğrudan kaynak yok, o yüzden aşağıda baskı mantığına (riso) dayandırıldı, Spider-Verse'e değil.
- **VFX'te halftone nokta partikülü** için GDC / 80.lv / ArtStation kırılımı bulunamadı. Aşağıdaki VFX önerileri (nokta izi, patlama) baskı mantığından ve yukarıdaki doğrulanmış ilkelerden türetilmiş tasarım önerisidir, bir oyundan alıntı değildir.

---

## 2. Ana ilkeler (8 madde + 4 uyarlama)

1. **Noktayı ton değil, malzeme olarak düşün.** Halftone (değişken boyut) ışık verir, Ben-Day (eşit boyut) desen/yüzey verir. Platform gövdesi için Ben-Day gibi davranan eşit noktalar, iz ve etkileşim için boyutu değişen halftone daha uygun. (Wikipedia)
2. **Nokta boyutu bilgi taşısın.** Spider-Verse'te nokta kalınlığı ışığı izler. Burada: boyut = hız ya da oyuncuya uzaklık ya da "enerji". Rastgele boyut yok, boyutun bir anlamı olsun. (Foundry)
3. **Noktalar nesneye bağlı (object space) olmalı.** Ekrana sabit desen hareketli platformda "duş kapısı" etkisi yapar, platform desenin içinde yüzer. (Hackaday, Foundry "swimming through a pattern") Platformun yerel koordinatında nokta ızgarası, platformla birlikte taşınır.
4. **Okunan şey nokta değil mürekkep/boşluk oranıdır.** Nokta ekranda çok küçük kalınca gürültü ve moiré olur. Dot pitch'i kamera ölçeğine göre sabit bir **ekran piksel aralığının altına inmeyecek** şekilde sınırla. (Heckel)
5. **Üst üste baskı tek renkli düz kutudan daha "basılı" durur.** Renkler şeffaf katman gibi çarpılır (Multiply), kesişimde üçüncü renk oluşur, hafif kayma (1-3 piksel) baskı hissi verir. (Riso kaynakları, Wikipedia: cyan + sarı = yeşil)
6. **Kayma küçük ve tutarlı olsun.** Katmanlar arası ofset platformun hareket yönüyle ilişkili (arkada kalan kenar) olabilir. Rastgele titreme okunabilirliği bozar. (AWN: ofset baskıyla arka kenar)
7. **Düz, kalın, tek ağırlıkta bir mürekkep çizgisi bütün deseni toplar.** Çizgi hem silüeti verir hem noktaların gürültüsünü bastırır. Kalınlık vurgu aracıdır (Jet Set Radio, Lichtenstein). Sable de çizgiyi okunabilirlik aracı olarak kullanıyor.
8. **Hareket stilize ve okunur olsun.** Spider-Verse motion blur'u kapatıp twos'a geçiyor, yani iz bulanıklıkla değil **ayrık şekillerle** anlatılıyor. Noktalar bu yaklaşıma doğal uyuyor: her nokta ayrı bir mürekkep damlası. (AWN)
9. **(Uyarlama) Yürünen üst kenar temiz.** Desen yoğunluğu platformun üst kenarında en düşük, alt kısımda en yüksek olsun. Oyuncunun bastığı çizgi düz renk + kalın çizgi. Bu bir araştırma bulgusu değil, platform oyunu okunabilirliği için tasarım kararı; Sable'ın okunabilirlik dersiyle aynı yönde.
10. **(Uyarlama) Nokta ölçeği oyuncudan büyük olmasın, kancadan küçük olsun.** Node'lar (halkalı, parlak) odanın baskın öğesi (GDD §8.1). Platform noktası node halkalarıyla yarışmamalı: platform bir "yüzey", node bir "hedef".
11. **(Uyarlama) CRT ile çakışma.** CRT tarama çizgisi ve vinyet düzenli bir ızgara; nokta ızgarası da düzenli. İki ızgara çakışınca moiré çıkar. Önlem: noktaları ızgaradan hafif kırık (açılı, 15° / 45° / 75° gibi) tut, nokta aralığı tarama çizgisi aralığının tam katı olmasın, kenarları `smoothstep` ile yumuşat. Önce CRT açık test et.
12. **(Uyarlama) Piksel hissinden kaçış.** Konumları ve nokta merkezlerini **tam sayıya yuvarlama**, doku filtresi bilinear/trilinear, nokta alanı SDF/analitik (shader'da), sprite atlas'tan nokta basma. Piksel ızgarası görünürse yönü kaybedersin.

---

## 3. Üç sanat yönü önerisi

Ortak teknik not (üçü için de geçerli): nokta deseni platformun **yerel uzayında** shader ile üretilir (UiConcrete/UiDither deseni gibi, `prefer-shader-for-ui-effects`), kenar yumuşatma `fwidth`, desen tamamen analitik. Palet `palette.md`'den: mürekkep `#140C1C`, kâğıt `#F1E6C7`, kırmızı `#FC1E00`, sarı `#FCAE04`, mavi `#597DCE`, gökyüzü `#71BAEA`, pembe `#FF51DE`, teal `#289AA4`. CMYK ihtiyacı için camgöbeği ≈ teal/gökyüzü, macenta ≈ `#FF51DE`/kırmızıya yakın.

### Yön A: "Pop-art çizgi roman paneli" (Lichtenstein + Ben-Day)

**Tanım.** Platform, kâğıt rengi bir zemin üstünde **eşit boyutlu büyük Ben-Day noktalarıyla** doldurulmuş, kalın siyah mürekkep çizgili bir "panel parçası". Tek ana renk (sarı ya da kırmızı), seyrek ikincil noktalar (camgöbeği, macenta). Bir referans görseldeki gibi: seyrek bölge → yoğun bölgeye geçiş. Platformun bir ucu seyrek, diğer ucu yoğun olabilir ya da hareket yönünde yoğunlaşır.

- **Şekil:** yumuşak köşeli levha (squircle, köşe yarıçapı yükseklikin ~%35-45'i), kenarlar tam düz değil: çok hafif, düşük frekanslı dalga (±1-2 piksel, sabit, animasyonsuz). Kutu görünümünü kırmak için iki ucu farklı yuvarlak.
- **Dolgu:** Ben-Day, **tek boyut** (platform yüksekliğinin ~1/6'sı nokta aralığı, 1080p'de ~10-14 px aralık), tek açı 45° ana renk için, ikincil renkler 15° ve 75°. Yürünen üst şerit (platform yüksekliğinin ~%20'si) düz kâğıt/sarı, nokta yok.
- **Çizgi:** `#140C1C`, kalın ve tek ağırlık (UI çerçeve kalınlığıyla aynı, `UiThemeSO.BorderWidthFor`), köşeler yuvarlak, hafif ofsetli gölge (UI'daki sert gölgeyle eş: beton + boya dili).
- **Trail VFX:** platform hareket ederken arkada **eşit boyutlu noktalar** bırakır. Her nokta platformdaki bir noktanın "kopyası", yani izin deseni platformun deseniyle aynı ızgara ve renkte başlar. Zamanla küçülür (yarıçap → 0, kâğıda "sönme"), rengi sarı → camgöbeği/macenta'ya kayar. Noktalar izgara üzerine **hizalı** doğar (ızgara dünya uzayında kilitli), böylece iz kendi içinde düzenli bir Ben-Day şeridi olur. Yoğunluk hıza bağlı: hızlıyken daha çok nokta.
- **Animasyon:** pürüzsüz (ones). Yalnızca iz noktaları için opsiyonel 12 fps "twos" ölçeği; ancak piksel ızgarası hissi yaratmaması için yalnızca **boyut** adım adım değişir, konum akıcı kalır.
- **Dünyaya uyum:** gökyüzü mavisi üzerinde sarı/kırmızı/kâğıt, turuncu zemin ve pembe kremayla aynı sıcak-şeker ailesi. UI'ya en yakın yön (beton + boya, kalın çizgi, sert gölge): platform "UI levhası" gibi durur.
- **Riskler:** en "dekoratif" olan. Büyük, sık nokta, node halkalarıyla görsel yarışa girebilir. Tek boyut nokta uzaktan düz renk alana dönüşebilir (Heckel eşiği), uzak odalarda (2-5 kat uzaktan) nokta okunmaz. Kalın siyah çizgi zeminin turuncu/pembesi ve boruların koyu tonuyla karışabilir, kontrast kontrolü gerek.

### Yön B: "Risograph zine" (iki-üç renk, grenli, üst üste baskı)

**Tanım.** Platform bir riso baskısı gibi: 2-3 mürekkep (örn. macenta-pembe + teal + sarı) **şeffaf katmanlar** halinde, kesişimlerde üçüncü renk. Çizgi yok ya da çok ince; şekli renk lekesi ve nokta yoğunluğu belirler. Kâğıt grenli, mürekkep kenarı hafif düzensiz.

- **Şekil:** yumuşak, hafif asimetrik "mürekkep lekesi levha". Kenar, nokta alanının yoğunluk düşüşüyle biter (nokta küçülerek kaybolur), yani sert kontur yok. Üst yürünen kenar ise **keskin ve düz**, tek yerde net çizgi.
- **Dolgu:** katman başına halftone (nokta boyutu değişen, soldan sağa/alttan üste ton geçişi). Üç katman, açıları 15° (camgöbeği/teal), 75° (sarı), 45° (macenta), nokta aralığı 8-12 px. Multiply karışım. Katmanlar arası **1-3 px kayma** (platform hareket yönüne ters, arka kenarda renkli kenar sızıntısı). Üzerine ince gren (düşük frekans, nokta ölçeğinden bağımsız) eklenir.
- **Çizgi:** yok, yalnızca üst yürünen kenarda ince `#140C1C`, ya da koyu mavi `#30346D` kenar.
- **Trail VFX:** arkada renk katmanlarının **ayrı ayrı kalması**: her katman platformdan farklı gecikmeyle (örn. teal 0 sn, sarı 0,08 sn, macenta 0,16 sn) nokta bırakır, hareket bir "baskı ofseti" gibi üç renk şeridine ayrılır. Noktalar küçülürken kâğıda karışır. Platform durunca iz gren içinde erir.
- **Animasyon:** pürüzsüz. Gren çok yavaş "kaynama" (boiling): saniyede 4-6 kez yeniden örneklenen düşük genlikli gürültü, nokta konumlarına dokunmaz.
- **Dünyaya uyum:** en az uyumlu ve en çok fark yaratan yön. Düz mavi gökyüzü ve düz renkli zemin üstünde grenli, şeffaf katmanlı platform "başka bir baskı"dan gelmiş gibi durur; bu bilinçli bir kontrast olarak da kullanılabilir (GDD §8: uyumsuz temalı dünyalar kabul). UI (beton ve boya) ile uyumu orta: UI'da sert çizgi ve gölge var, bu yönde yok.
- **Riskler:** çizgi yok, silüet okunabilirliği zayıf. En büyük risk bu. Şeffaf katmanlar sky blue üstünde renk kaybeder (Multiply gökyüzünü koyulaştırır). Üç katman farklı kaydığı için hareketli platformda platformun gerçek collider sınırı belirsiz görünebilir (oyuncu "içinden geçiyor" sanır). Gren + CRT tarama çizgisi birlikte gürültü yığar. Önerilen güvenlik: collider sınırında her zaman net bir üst çizgi.

### Yön C: "Spider-Verse ofset" (ofset renk kenarı + ışığa bağlı nokta)

**Tanım.** Düz renkli, çizgili bir platform; renk dolgusu mürekkep çizgisinden **hafifçe kaymış** (offset baskı), gölge ve ışık bölgeleri **değişken boyutlu halftone** ile verilmiş. Noktalar yüzeyin her yerinde değil, yalnızca gölge tarafında (alt kenar) ve hareket yönünün arkasında. Çizgi roman karesi gibi: yüzey okunur, nokta vurgudur.

- **Şekil:** yuvarlatılmış kalın "hap" (pill) ya da squircle. Çizgi hafif **el çizimi dalgalı** (düşük frekanslı gürültü ile kenar sapması ±1-2 px), kapanış noktasında küçük bir çizgi açıklığı (mürekkep çizgisi tam kapanmıyor, baskı kusuru).
- **Dolgu:** ana renk düz (sarı/kırmızı/teal). Alt %35'te halftone: yukarıdan aşağı nokta boyutu büyür (0 → platform yüksekliğinin ~%12'si), açı 45°, koyu ton olarak `#30346D`/`#97338A`. Renk dolgusu çizgiden hareket yönünün tersine 2-4 px kaymış: arkada ince bir "boş kâğıt/ikinci renk" şeridi (Spider-Verse'teki arka kenar ofseti mantığı, AWN).
- **Çizgi:** `#140C1C`, kalın, tek ağırlık, köşeler yuvarlak.
- **Trail VFX:** hareket yönünde arka kenardan **değişken boyutlu noktalar** fırlar: platformun yakın arkasında büyük, uzaklaştıkça küçük (halftone gradyanı olarak iz). İz, platformun hız vektörüne göre **ofset renk şeridi** ile birlikte: nokta + 2-3 px renk kayması. Hızlı hareketle (hızlanan platform stratejisi) iz uzar ve nokta aralığı seyrekleşir.
- **Animasyon:** **twos** (12 fps) yalnızca iz noktaları için, platformun kendisi akıcı. Motion blur yok. Platformun yüzey çizgisi (el çizimi dalga) her 2-3 karede bir yeniden örneklenir (boiling), çok hafif. Not: bu animasyon dili "piksel gibi" değil "çizim gibi", ama adım adım değişen çizgi oyuncunun fiziksel hareketine uymazsa (özellikle üzerinde duran oyuncu) titreme sanılır. Çizgi boiling'i yalnızca görsel katmanda, collider ile ilişkisiz.
- **Dünyaya uyum:** üç yönün dünyaya **en uyumlusu**: düz renk + kalın çizgi, mevcut node, boru, zemin dilinin devamı. Halftone yalnızca vurgu, çarpışma az. UI (kalın çizgi, ofset gölge) ile en güçlü eşleşme: UI'daki "sert gölge ofseti" ile platform ofseti aynı fikir.
- **Riskler:** yön A ve B'ye göre "halftone" daha az hissedilir; geliştirici "dolu noktalı nesne" istiyorsa bu yön yetersiz kalabilir. Ofset boyutu 2-4 px, 1080p'de hoş durur ama oyun 4x ölçekli sanat pikseliyle (480x270) çalışıyorsa ofset ölçeği tam sayıya yapışabilir, dünya ölçeğinde (float) tanımlanmalı. Çizgi boiling ve twos yanlış uygulanırsa "piksel/stepped" hissi geri gelir.

### Karşılaştırma

| | A: Pop-art panel | B: Riso zine | C: Spider-Verse ofset |
|---|---|---|---|
| Nokta rolü | Ana yüzey dokusu (Ben-Day) | Ana tonlama (halftone katmanları) | Vurgu, gölge, iz |
| Çizgi | Kalın mürekkep | Yok / çok ince | Kalın mürekkep, el çizimi |
| Okunabilirlik | İyi (üst şerit temiz) | Zayıf (çizgi yok) | En iyi |
| Dünyaya uyum | İyi | Düşük | Yüksek |
| UI ile uyum | Çok iyi | Orta | Çok iyi |
| Halftone hissi | Çok güçlü | Güçlü | Orta |
| Uygulama maliyeti | Orta (tek shader) | Yüksek (3 katman + gren) | Orta-yüksek (ofset + iz sistemi) |

Önerilen hibrit: **A'nın dolgusu + C'nin ofset kenarı ve iz davranışı** (A'nın Ben-Day yüzeyi, C'nin kayan renk kenarı ve değişken boyutlu iz noktaları). B'nin yalnızca "iz renk katmanlarının gecikmeli ayrılması" fikri izde kullanılabilir.

---

## 4. Mevcut dünya görünümüyle çakışma (dürüst not)

- **Arazi düz ve piksel sanat.** Zemin turuncu + pembe krema, boru koyu, node'lar gökkuşağı halkalı; hepsi düz renk alan ve piksel sanat dokusu (GDD §8.1). Pürüzsüz, noktalı, çizgili platform bu dünyanın içinde **farklı bir malzeme** gibi durur. Bu kısmen iyi bir şey: hareketli platform zaten "dokunulabilir/dinamik" nesne, farklı görünmesi oyuncuya "bu hareket eder" sinyali verir (ve oyunda "uyumsuz temalar yan yana" bilinçli bir tercih). Ama kötü olabilir: platform yapıştırma gibi (sticker) durabilir.
- **Piksel sanat zemin yanında pürüzsüz platform.** Piksel sanatın kenarı basamaklı, halftone platformun kenarı yumuşak. Yan yana gelince pürüzsüz taraf "daha yüksek çözünürlüklü" görünür, yani ölçek tutarsızlığı. GDD §8.1 zaten piksel ölçeği tutarsızlığına dikkat çekiyor (node, boru, krema yoğunlukları farklı); platform bu tutarsızlığı daha da görünür kılar.
- **CRT filtresi.** Tarama çizgisi + hafif eğrilik nokta ızgarasıyla moiré yapabilir. Platform hareket ederken moiré "yürür", çok dikkat dağıtıcı olabilir. CRT açık olarak test edilmeden hiçbir yön "tamam" sayılmamalı.
- **Renk:** gökyüzü mavisi üstünde Multiply (yön B) renk kaybettirir. Sarı/kırmızı/kâğıt (A, C) mavi üstünde güçlü, ama turuncu zemin ve pembe krema ile sıcak tonlar yarışır.
- **Node'larla görsel yarış.** Parlak, halkalı node'lar odanın en baskın öğesi. Büyük, yüksek kontrastlı nokta desenli platform bunun önüne geçebilir. Node'lar kancanın hedefi, platform ise yüzey; hiyerarşi bozulmamalı.

**Dünyayı bütünleştirmek için en küçük değişiklikler** (öncelik sırasıyla):
1. **Kâğıt/mürekkep kenarı:** platformların kalın `#140C1C` çizgisini, zeminin ya da boruların kenar çizgisiyle aynı kalınlığa yaklaştır (borularda zaten mor-siyah kenar çizgisi var, `palette.md`). Böylece platform "aynı mürekkeple çizilmiş" olur. Tek değişiklik platformda; arazide dokunma.
2. **Nokta paleti dünya paletinden:** yalnızca `palette.md` renkleri (kırmızı, sarı, mavi, teal, pembe, kâğıt). Yeni renk ekleme.
3. **Nokta ölçeği = dünyadaki "sanat pikseli" katı:** nokta aralığını 16x16 zemin karosunun kesirlerine bağla (örn. karo/2 ya da karo/3) ki ölçek ilişkisi hissedilsin. Ama noktaları piksele **yuvarlama**.
4. **Gerekirse tek küçük arazi dokunuşu:** zemin kremasının (pembe) üstüne düşük yoğunluklu bir halftone (statik, aynı shader) eklemek dünya ile platformu bağlar. Bu bir **öneridir**, gerekliliği test edilmeli; zemini değiştirmek geliştirici kararı.
5. **Gökyüzü dokunmadan kalsın.** Düz mavi arka plan platformların "kâğıdı" gibi çalışır; gökyüzüne desen koymak okunabilirliği bozar.

**Açık sorular (geliştiriciye):**
- Platform "dünyadan ayrı bir malzeme" mi olsun (Yön B/C gibi bilinçli fark), yoksa dünyayla aynı baskı dilini mi paylaşsın (arazide de hafif halftone)?
- Nokta ve ofset 4x ekran ölçeğinde mi (sanat pikseli 480x270) yoksa tam çözünürlükte mi tanımlansın? Piksel ızgarasından kaçmak için çözünürlük-bağımsız (dünya birimi) önerilir.
- Aşama 2 (oyuncuya toplanan noktalar) için nokta sayısı bütçesi: hareketli platform başına kaç partikül kabul edilir (mobil/low-end ayarı var mı)?

---

## 5. Sonraki adım önerisi

Üç yönün hepsi için tek bir test odasında (`PhysicsTest` sahneleri uygun) shader prototipi, CRT açık/kapalı, 1080p ve 4K, kamera 2x ve 5x uzaklıkta ekran görüntüsü. Karar geliştiricinin (oyun hissi ve estetik kararı); bu belge tasarım önerisidir, doğrulanmış kaynaklar yalnızca tablo 1'dekilerdir. TASKS.md'ye ekleme geliştiriciye sorulmadan yapılmadı.
