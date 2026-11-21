# Çiçek node'u: doğadaki biçimler ve bilim (araştırma D)

Tarih: 2026-11-20. Açı: jenerik papatya/gül yerine doğadan şaşırtıcı bazlar. Kod ve sahneye dokunulmadı.

Okuma notu: Her olgunun yanında kaynak güvenilirliği etiketi var.
- **[A]** Sayfa gerçekten açıldı ve okundu.
- **[B]** Yalnızca arama sonucu özetinde gördüm, sayfayı açamadım. Referans olarak zayıf.
- **[T]** Ben ürettim: tasarım yorumu / öneri. Doğa olgusu değil.

Oyun kısıtları (kısa hatırlatma): kameraya bakan (top-down) görünüm, ~9 birim çap (oyuncu 1x1), shader/SDF/polar çizim, 2-3 ton bandı + dış çizgi. Dünya paleti: gökyüzü mavisi, turuncu zemin, pembe krema, gökkuşağı node'lar, parlak yeşil asit, kırmızı fırlatma pedleri, magenta-pembe/turuncu/sarı karakter. Bauhaus, op-art, halftone, folk ve spiral/hipnotik desen kullanılmış sayılıyor; aşağıda öneri olarak sunulmadı.

---

## 0. Önce okunabilirlik kuralları (tohumları değerlendirirken kullandığım ölçüt) [T]

9 birimlik bir node'da oyuncu 1 birim. Ekranda ince ayrıntı hemen kaybolur. Bu yüzden:
- Dış siluette en çok 5-12 belirgin parça. Daha fazlası "gürültü" olur.
- Bir parçanın en dar yeri en az ~0.5-0.7 birim olmalı. Kirpik, tüy, diş gibi ince şeyler abartılıp iri "dişe" çevrilmeli (diş boyu ~0.5-0.8 birim).
- Merkez "tutun buraya" işareti ayrı bir okunur nesne olmalı (halka, koyu göz, direk grubu). Yaprak şeklinden bağımsız çalışmalı.
- Gerçek çiçeklerin birçok özelliği yandan görünüm içindir (sürahi derinliği, mahmuz, bükülmüş boru). Top-down'da bunlar ya kaybolur ya da yeni bir desene dönüşmeli. Aşağıda her tohumda bunu "risk" altında belirttim.

---

## 1. Alışılmadık gerçek çiçekler ve bitkiler (12 seçim)

Her biri için: siluet, renk, ve kapalı -> açık geçişin oyuna çevrilmesi.

### 1.1 Pasiflora (Passiflora)
- **Doğrulanan:** Beş taç yaprak ve beş çanak yaprak (pentamer) [A: Wikipedia Passiflora]. Asıl dikkat çeken yapı korona: ince renkli iplikler, merkez sütunun çevresinde iki katman halinde yelpaze gibi açılır; içinden geçen eşmerkezli renk halkaları polinatörü nektara yönlendiriyor sayılır [A: gardendesign.com]. Bazı türlerde korona sekiz eş merkezli seriye ve yüzü aşkın ipliğe çıkabilir [A: Wikipedia Passiflora]. Merkez sütun (androgynophore) organları taşır; tabanda nektarı örten koyu lif halkası (operculum) polinatörü içeri iterek organlara sürtünmeye zorlar [A: gardendesign.com]. Anterler açılıştan yaklaşık bir saat sonra yukarıdan aşağı döner [A: gardendesign.com]. Çoğu çiçek yalnızca bir gün açık kalır [A: Wikipedia Passiflora]. Arı, yarasa ve sinekkuşu türlerinde korona boyu ve sütun şekli değişir; arı türlerinde iplik uzun, yarasa ve sinekkuşunda kısa [A: innerworlds.jic.ac.uk].
- **Siluet özelliği:** 10 yaprak (5+5) + yoğun iplikli taç + ortada yükselen sütun. Üç katmanlı net bir sıralama: yaprak, iplik tacı, merkez kule.
- **Renk:** Çoğu türde mavi-beyaz; bazılarında kırmızı/gül ve sarı [A: Wikipedia Passiflora]. Koyu halka/açık halka bantlaşması karakteristik.
- **Kapalı -> açık:** Gerçekte tomurcuk yaprakları ve çanak yaprakları dışta kapalı durur; açılınca düzleşip iplik tacı görünür [T, genel gözlem, kaynak yok]. Oyunda: kapalıyken iplikler içeride sıkışmış bir "fırça", açılınca yelpaze gibi saçılır (shader'da polar katmanlı iplik).

### 1.2 Protea (özellikle kral protea, P. cynaroides)
- **Doğrulanan:** Çiçek tek çiçek değil, kümedir: ortada çok sayıda küçük çiçek, çevresinde iri gösterişli brakteler. Baş çapı yaklaşık 12-30 cm; türün adı enginar benzerliğine gönderme yapar; brakte rengi krem beyazdan koyu kırmızıya uzanır, gümüş parlaklı soluk pembeler makbul sayılır [A: Wikipedia Protea cynaroides]. Genel olarak Protea çiçek başı, odunsu bir tabanda sıkışık çok sayıda küçük çiçekten oluşur ve ortada krem renkli karpel görünür [A: Wikipedia Protea].
- **Katman düzeni:** Sayfalar bunu anlatmıyor; "dışta uzun sivri braktelerin çok sıralı tacı, ortada yastıksı kubbe" betimlemesi benim genel bilgimden, doğrulanmadı [T].
- **Siluet:** Uzun, sivri, kama biçimli brakte tacı (3 sıra) + ortada kubbe (iğne yastığı).
- **Kapalı -> açık:** Tomurcuk kapalıyken enginar gibi: üst üste binmiş sivri pullar (imbricate) kapalı bir koni. Açılırken en dış pullar önce dışa yatar, sonra iç sıralar, en son merkez kubbe görünür. Çok güçlü bir "kapalı hal".

### 1.3 Nilüfer (Nymphaea) ve lotus (Nelumbo)
- **Doğrulanan, nilüfer:** Dışta 4-5 yeşil çanak, içeride yaklaşık 6-50 dar, mızrak/kaşık biçimli taç yaprak; yapraklar kademeli olarak organlara (20-750 stamen) dönüşür, organlar bazen yaprağa benzer [A: Wikipedia Nymphaea]. Çiçek suyun yüzeyinde durur. Gündüz ya da gece açılan türler var [A].
- **Doğrulanan, lotus:** Bazı kültivarlarda binlerce yaprak (2000-5000'e kadar, "en çok yapraklı bitki") [A: Wikipedia Nelumbo nucifera]. Çiçek kendi sıcaklığını 30-35 °C'de tutar (hava ~10 °C'ye düşse bile) [A]. Kurumuş tohum başı sulama kabı ağzına benzer [A].
- **Siluet:** Sivri uçlu, birbirine geçen çok sayıda dar yaprak; dıştan içe boyut ve renk gradyanı.
- **Kapalı -> açık:** Yapraklar tek bir gövde ekseninde dikey toplanır (kapalı bir mızrak kovanı). Oyunda: sıralı katmanlar sırayla açılır; gradyan renk bantları hazır.

### 1.4 Orkide (dudak / labellum)
- **Doğrulanan:** Orkide çiçeği iki yönlü simetriktir. Üç çanak, üç taç; bunlardan biri güçlü biçimde değişmiş bir "dudak" olup böceği çeker, bazı türlerde iniş platformu ya da tuzak işlevi görür. Tomurcuk gelişirken çiçek yaklaşık 180 derece döner (resupination). Erkek ve dişi organlar tek bir sütunda (column) kaynaşır [A: Wikipedia Orchidaceae].
- **Siluet:** Üç + iki + bir (dudak) düzeni: üçlü yıldız bazı, iki yan yaprak ve dilimsi bir dil.
- **Renk:** Çok geniş; dudak çoğunlukla farklı renk.
- **Kapalı -> açık:** Oyuna çevirisi zor, çünkü bilateral. Fikir: node'u 3 katlı (üçgen) simetriye indirgemek: üç sivri çanak yıldızı + iki yan kanat + öne uzanan dudak olarak kullanmak. Dudak = "buradan tutun" işareti (iniş platformu). Dönme (resupination) açılış animasyonuna 180 derece dönüş olarak girebilir [T].

### 1.5 Ters çiçek: Aquilegia (columbine)
- **Doğrulanan:** Beş dış çanak yaprak taç yaprağa benzer; beş gerçek taç yaprağın her birinde ön yüzü açık bir kanat ve geriye uzanan içi boş bir nektar mahmuzu var. Mahmuz biçimi boynuz gibi kancalı, düz ya da kıvrık olabilir. Merkezde beş stamen halkası ve aralarında küçük kısır yapılar (staminodlar) var. Çiçekler tomurcukta aşağı bakar; çoğu yabani türde böyle kalır [A: Wikipedia Aquilegia]. Mahmuz uzunluğu 16 kat arasında değişir, polinatör dil boyuyla eşleşir [B: arama özeti, USDA sayfası 403 verdi].
- **Siluet:** Beş geniş çanak yıldızı + arasından dışa kıvrılan beş boynuz.
- **Renk:** Avrupa'da mavi-mor ve neredeyse siyah, Kuzey Amerika'da sarı-kırmızı [A].
- **Kapalı -> açık:** Boynuzlar içe doğru sarılı kalır, açılırken dışa doğru süpürür. Risk: boynuz sırası yön hissi verir (pinwheel okunur), spiral'e kayabilir, dikkat.

### 1.6 Kamelya (Camellia japonica)
- **Doğrulanan:** Beş biçim: tek (5-8 gevşek yaprak, organlar görünür), yarı katmerli (iki ve üzeri halka, merkezde kesintisiz organ kümesi), resmi katmerli (yüzün üzerinde yaprak, düzenli sıralı, organ görünmez, merkez "sıkıca sarılmış yapraklardan koni"), anemon biçimi (düz dış yapraklar ve ortada yaprak+organ karışımı), şakayık biçimi [A: Wikipedia Camellia japonica].
- **Siluet:** Eşmerkezli yaprak sıraları, her sıra bir öncekine göre kayık. Resmi katmerli merkez konisi çok karakteristik.
- **Kapalı -> açık:** Kapalı hal zaten merkezdeki sıkı koni. Açılırken sıralar dıştan içe sırayla gevşer ve yatar. Not: Mevcut 2 katmanlı çiçeğe en yakın aday; "jenerik" riski yüksek.

### 1.7 Edelweiss (Leontopodium nivale)
- **Doğrulanan:** Yıldız biçimli çiçek başı. Beyaz tüylü "yaprak"lar aslında brakte. Çift yıldız gibi yayılırlar; ortada 5-6 küçük sarı çiçekçik kümesi var. Yapraklar ve çiçek parçaları yoğun beyaz tüyle kaplı (yün görünüm); bu tüylerin ince yapısı yakın morötesi ışığı saçıyor ve soğuyor; yüksek rakımda UV'ye karşı uyum olduğu düşünülüyor [A: Wikipedia Edelweiss].
- **Siluet:** İki yıldız (alt ve üst), yumuşak/keçeleşmiş kenar, orta kümede sarı noktalar.
- **Renk:** Beyaz-gri-sarı.
- **Kapalı -> açık:** Kapalı olarak yüke eğilen, yünlü, topaklı bir tüy topu (brakteler yukarı toplanır). Kapanma gerçek bitkide belgelenmedi [T].

### 1.8 Anemon (Anemone coronaria)
- **Doğrulanan:** 5-8 gösterişli taç yaprak biçimli tepal; yabanda genelde kırmızı, beyaz ve mavi biçimleri de var. Çiçek 3-8 cm. Merkezde yoğun bir pistil kümesi, çevresinde taç gibi bir organ halkası; merkez siyah, beyaz çeşitte açık yeşil [A: Wikipedia Anemone coronaria].
- **Siluet:** Basit, geniş 5-8 yaprak; merkezdeki koyu göz ve organ halkası hakimdir.
- **Kapalı -> açık:** Geceleri kapanır mı sayfa söylemiyor [doğrulanmadı]. Oyuna çevirisi: koyu merkez "gözü" kapalıyken yaprakla örtülü, açılınca beliriyor. Çok net bir "tutunma" odağı verir.

### 1.9 Rafflesia arnoldii
- **Doğrulanan:** Kırmızımsı kahverengi, beyaz benekli; çürük et kokusu; çap yaklaşık 1 m, 11 kg'a kadar; üreme organları dikenli bir diskin üstünde. Tomurcuk koyu bir lahanaya benzer (~30 cm), gelişmesi aylar-yıl sürer; çiçek yalnızca birkaç gün açık kalır [A: Wikipedia Rafflesia arnoldii].
- **Doğrulanamayan:** Beş lob ve siğilimsi yapılar sayfada yok [doğrulanmadı].
- **Oyuna çevirisi:** Kalın, etli, benekli büyük lobların ortasında dikenli bir disk. "Lahana tomurcuk" kapalı hali: yuvarlak, katmanlı, kabuklu top. Benekler cel-shading'de büyük yuvarlak lekeler olarak işlenebilir. Tehlikeli çiçek estetiğine de uygun [T].

### 1.10 Devasa arum (Amorphophallus titanum)
- **Doğrulanan:** Çan biçimli spata: dışı koyu yeşil ve krem benekli, içi koyu bordo; kenarı nervürlü, buruşuk/fırfırlı. İçinde yüksek, kokulu bir spadiks. Çiçeklenmeden kısa süre önce tabandaki iki yaprak benzeri brakte kurur; spata yavaş açılırken spadiks yaklaşık 37 °C'ye ısınır ve kokuyu darbeler halinde salar. Çiçeklenme 24-36 saat sürer; yükseklik 3 m'yi aşabilir [A: Wikipedia Amorphophallus titanum].
- **Siluet:** Tek büyük huni/yaprak + ortada dikey kule. Top-down'dan bakınca halka (açılmış spata kenarı) ve merkez noktası.
- **Kapalı -> açık:** Spata sargısı kule etrafında sıkı sarılı (silindirik). Açılırken fırfırlı kenar dışa kıvrılır, iç bordo yüzü görünür. Isı titreşimi (ısı dalgalanması) shader'da bir "ısı parlaması" efekti olabilir [T].

### 1.11 Lale (Tulip)
- **Doğrulanan:** İki halkada üç + üç tepal; fincan ya da yıldız biçimi; iç segmentlerin ucunda küçük çentik, dış segmentler yuvarlak oval; her tepalin tabanında farklı renkte bir leke bulunabilir ve içeride daha koyu görünür; altı stamen iki halkada; üç loblu stigma; nektar yok [A: Wikipedia Tulip].
- **Hareket:** Lale ve çiğdem çiçeklerinin açılması sıcaklığa bağlı (termonasti) [A: Wikipedia Thermonasty]. Bkz. bölüm 3.
- **Siluet:** Altı eşit yaprak, tabanda koyu leke halkası (tabandan merkeze doğru "gözlük"). Altı parçalı dönen koyu leke merkezde bir yıldız oluşturur; güçlü, doğal bir merkez işareti.
- **Kapalı -> açık:** Kapalı halde yumurta gibi tek bir oval (fincan); açılırken uçlar dışa kıvrılır, yıldız olur. En basit ve en okunur dönüşüm, ama "bilindik çiçek" riski.

### 1.12 Çan çiçeği (Campanula) ve gentian (Gentiana)
- **Doğrulanan, campanula:** Beş loblu çan biçimli taç (2-5 cm), çoğu mavi-mor; altında beş yaprağımsı çanak; bazı türlerde çanak yaprakları arasında küçük yaprağımsı uzantılar var; tür ayrımında kullanılır [A: Wikipedia Campanula].
- **Doğrulanan, gentian:** Boru ya da çan biçimi, çoğu derin mavi; beş lob ve beş çanak; loblar arasında küçük kıvrımlar (plicae) var [A: Wikipedia Gentiana].
- **Siluet:** 5 sivri loblu yıldız, loblar arasında kıvrım üçgenleri ve dışta çanak dişleri: aslında iki ayrı yıldız (iç ve dış) iç içe.
- **Kapalı -> açık:** Kapalı halde kıvrımlar (plicae) kapalı bir şemsiye gibi katlı. Açılırken kıvrımlar düzleşir ve lobları dışa iter. Bu, "katlanmış şemsiye" tipi çok net bir kapanma biçimidir [T].

### Kısa ek: hibiscus ve gece kraliçesi
- **Hibiscus rosa-sinensis:** beş yaprak; ortadan uzun tüp biçimli pistil yükselir, ucunda beş küçük tüylü nokta (stigma); organlar tüpü çevreler, turuncu uçlu anterlerle [A: Wikipedia Hibiscus rosa-sinensis]. Siluet fikri: ortadan çıkan tek uzun "direk" ve halka halinde nokta uçlar. Merkez direk, top-down'da küçük bir daire + beş nokta olur.
- **Gece kraliçesi kaktüsü (Selenicereus grandiflorus):** Dış yapraklar dar, sivri, açık kahve-somon; iç yapraklar beyaz, geniş, sivri uçlu; çap ~38 cm'ye kadar; yalnızca bir gece açık kalır, saatler içinde solar [A: Wikipedia Selenicereus grandiflorus]. İki zıt halka (dış koyu sivri çember + iç beyaz çember) şu anki çiçeğe göre çok farklı bir silüet verebilir [T].

---

## 2. Etçil bitkiler ve tuzak çiçekler

### 2.1 Venüs sinekkapanı (Dionaea muscipula)
- **Doğrulanan:**
  - Tuzak, orta damarda birleşen iki lobdan oluşur; lob kenarları sert, kirpik gibi çıkıntılarla (cilia) çevrilidir; kapanınca bunlar kenetlenir. Büyük av kaçamaz; boşluklardan küçük av çıkabilir [A: Wikipedia Venus flytrap].
  - Her lobun üst yüzeyinde üç tetik tüyü var. Tek dokunuş yetmez; yaklaşık 20 saniye içinde iki dokunuş ya da aynı tüye hızlı iki dokunuş gerekir [A: Wikipedia Venus flytrap]. UZH sayfası aynı pencereyi "yaklaşık 30 saniye" diye veriyor [A: news.uzh.ch]; iki kaynak arasında tutarsızlık var, bunu "20-30 sn" diye okuyorum.
  - 2020 UZH/ETH çalışması: yavaş tek dokunuş da iki sinyal üretebilir, tuzak kapanır [A: news.uzh.ch].
  - Kapanma yaklaşık 0,1 saniye; lobun dışbükeyden içbükeye çevrilmesiyle olur [A: Wikipedia Venus flytrap]. Açık lobların dışa doğru gerilimle bükülü olduğu, "sıkıca sarılmış yay" gibi tutulduğu, küçük bir eğrilik değişikliğinin ani kapanmaya yol açtığı belirtiliyor [A: news.uzh.ch].
  - Lob üst yüzeyinde kırmızı antosiyanin pigmenti var [A: Wikipedia Venus flytrap]. Kenarlar yapışkan müsilaj salgılar [A].
  - Sindirim yaklaşık on gün sürer; sonra tuzak yeniden açılır. Küçük av kaçarsa genelde 12 saat içinde açılır [A].
- **Şekil ipuçları:** Kenarda iri kirpik/diş sırası (karşılıklı kenetlenen), iç yüzde üç direkçik, iki lob. Kapalı hali: diş sırasının çapraz kilitlendiği dikişli bir "ağız".
- **Oyun fikri:** Uyarı evresi = lobların dışa doğru fazladan gerilmesi (gerçek mekanizma: yay gibi gergin eğrilik), sonra bir anda ters dönmesi (snap). Bu, "gerilim -> flip" olarak nesnel bir fiziğe dayanan çok iyi bir uyarı animasyonu [T, doğa olgusuna dayalı].

### 2.2 Güneş çiyi (Drosera)
- **Doğrulanan:** Yapraklar ince, saplı dokunaçlarla kaplı; uçlarında yapışkan damlalar ışıkta çiy gibi parlar. Av dokunulunca dokunaç merkeze doğru eğilir, komşular da izler. Bazı dokunaçlar saniyeler içinde, bir tür saniyenin kesri içinde tepki verir. Bazı türlerde tüm yaprak ava kıvrılır (yaklaşık 30 dakika). Yaprak biçimleri yuvarlaktan ipliksiye kadar çeşitli; çiçekler çoğunlukla beyaz-pembe [A: Wikipedia Drosera].
- **Şekil ipucu:** Kenara doğru uzayan, ucunda parlak damla olan çubuklar (radyal). Top-down'da "ışınlar + uç noktalar".
- **Oyun fikri:** Yaprak yerine uzun, ince, ucu yuvarlak parmak dizileri (örneğin 12-16). Nokta uçlar, yakalama anında merkeze doğru sırayla eğilir (dalga etkisi).

### 2.3 Sürahi bitkileri (Nepenthes, Sarracenia)
- **Doğrulanan, Nepenthes:** Tuzak, saplı bir sürgünün ucunda küçük bir tomurcuktan yuvarlak ya da boru biçimli bir kaba büyür; ağız kenarı (peristome) "kaygan ve çoğunlukla renkli"; üstte kapak (operculum) yağmuru uzak tutar; kapağın alt yüzü nektar bezleri taşıyabilir [A: Wikipedia Nepenthes]. Kenarın nervürlü olduğu sayfada geçmiyor.
- **Doğrulanan, Sarracenia:** Dikey boru biçimli yaprak, açıklığı bitkinin merkezine bakar; kapüşon biçimli kapak, ağızda yuvarlanmış nektarlı kenar; bazı türlerde kapakta beyaz, saydam "pencereler"; çiçek tek başına uzun saplı, beş uzun sarkık yaprak ve organların üstünde şemsiye biçimli bir stil [A: Wikipedia Sarracenia].
- **Şekil ipuçları:** Top-down'da: renkli halka kenar (peristome), onu yarı örten bir kapak/kapüşon, ortada koyu boşluk (derinlik).
- **Oyun fikri:** Kapalı = kapak ağzı örtmüş; açık = kapak yukarı kalkmış, koyu merkez ve renkli halka. Not: "kapak" bir yön getirir (asimetri), radyal bir node için sorun olabilir.

### 2.4 Mimoza (Mimosa pudica)
- **Doğrulanan:** Dokunma, sallama, üfleme ve ısınma yaprakçıkların katlanmasını tetikler; tepki bir aksiyon potansiyeli olarak dokunulan yaprakçıktan eklem şişkinliklerine (pulvini) ve oradan yaprağa yayılır; pulvini hücrelerinde su girişi/çıkışıyla turgor değişir ve yaprak sarkar; yapraklar birkaç dakika içinde yeniden açılır, geceleri de kapanır; çiçekler 8-10 mm'lik yuvarlak-oval başlar [A: Wikipedia Mimosa pudica].
- **Oyun fikri:** Sinyalin uçtan kökte doğru yayılması = "kapanma dalgası": parçalar aynı anda değil, sırayla kapanır. Sıralı kapanma hem güzel hem de oyuncuya zaman verir [T].

### 2.5 Su değirmeni (Aldrovanda vesiculosa)
- **Doğrulanan:** Su altında minik kapan tuzakları; kapanma 10-20 milisaniye (en hızlı bitki hareketlerinden); kapağın çevresinde 4-6 sert kıl; tuzaklar 5-9'luk çember halinde gövde etrafında dizilir [A: Wikipedia Aldrovanda vesiculosa].
- **Şekil ipucu:** Halka halinde dizilmiş kapanlar, çevrede sert kıllar, radyal simetri: tam top-down uyumlu "çark".

### 2.6 Tuzak çiçeği estetiğinden çıkan şekil ipuçları (özet) [T]
- Kenarda **diş/kirpik sırası** (kenetlenen) = tehlike, ama dişleri iri ve az tutun (8-14).
- **Üç direkçik** merkez işareti (Venüs'ün tetik tüyleri gibi) = hem "tutun" hem "dokunma = kapanma" ima eder.
- **İç yüz rengi dış yüzden belirgin farklı**: kapanınca dış yüz görünür (renk değişimi hazır bir geri bildirim).
- **Kapak/kapüşon** = asimetri; radyal olması için çoklu (3-4) kapak.
- **Yapışkan damla uç** = parlak yuvarlak uçlar (vurgu noktası).

---

## 3. Çiçeğin hareketi: fizik ve zamanlama

### 3.1 Hücre şişmesi ve eşitsiz büyüme
- Genel bulgu: çiçek açılması çoğunlukla hücre genişlemesidir; bazı türlerde yaprağın iki yüzü arasındaki eşitsiz büyüme açıyor [B: arama özeti, PMC makalesi bot doğrulamasına takıldı].
- Lilyumda tepal bölgeleri arasındaki eşitsiz genişleme önemli; kenarlar açılırken kırışıyor [B: arama özeti].
- Çiğdem/Colchicum: Hava ısındıkça tepalin iç yüzü daha hızlı büyür ve yaprakları dışa iter; soğuyunca (gece) iç hücreler yavaşlar, dış hücreler göreli olarak hızlanır ve çiçek kapanır. Zaman atlamalı video ~70 dakika boyunca 10 saniyede bir kare alınmış [A: plantsinmotion.bio.indiana.edu].
- Termonasti: yönsüz, sıcaklığa bağlı hareket. Lale ve çiğdem çiçeklerinin açılması buna örnek; hareket genellikle eşitsiz hücre uzamasıyla olur; lale/çiğdemde bu geri dönüşsüz büyümeyle olur ki bitki hareketlerinde nadirdir [A: Wikipedia Thermonasty]. Sıcaklık eşiği ve hız sayfada yok [doğrulanmadı].

### 3.2 Gece kapanması (nyctinasti)
- İç saat (sirkadiyen) tarafından yürütülür; ışık ve sıcaklık değişimleri ayar yapar; birçok türde hareket yaprak sapı tabanındaki şişkin eklemlerde (pulvini) olur: potasyum iyonlarıyla su iki taraf arasında kayar. Pulvinisi olmayan bitkiler bir tarafı daha hızlı büyüterek bükülür. Önerilen işlevler: ısı korumak, polenı nemden korumak, gece uçan polinatör için gece açılmak [A: Wikipedia Nyctinasty].

### 3.3 Hızlı mekanik hareketler (zamanlama referansı)
| Olay | Süre | Kaynak |
|---|---|---|
| Venüs kapanı kapanması | ~0,1 sn | [A] Wikipedia Venus flytrap |
| Aldrovanda kapanması | 10-20 ms | [A] Wikipedia Aldrovanda |
| Mimoza yeniden açılma | birkaç dakika | [A] Wikipedia Mimosa pudica |
| Güneş çiyi tüm yaprak kıvrılma | ~30 dk | [A] Wikipedia Drosera |
| Çiğdem/Colchicum açılma (zaman atlamalı video) | ~70 dk | [A] Indiana |
| Pasiflora çiçeği ömrü | çoğunlukla 1 gün | [A] Wikipedia Passiflora |
| Devasa arum çiçeklenmesi | 24-36 saat | [A] |
| Gece kraliçesi | 1 gece | [A] |

Oyun için çıkarım [T]: Doğada asimetri hakim: açılış yavaş ve sürekli (turgor/büyüme), kapanış ya çok hızlı (snap) ya da dalga halinde (mimoza). Mevcut döngü (uzun açılış, kısa uyarı, ani kapanış) bunun zaten doğa ile uyumlu olduğunu gösteriyor. Eklenebilecek doğa ilhamlı iki fikir: (a) açılışta sıralı dalga (dıştan içe ya da içten dışa), merkez en son "pop"; (b) kapanışta ya "gerilim sonra flip" (Venüs) ya da "uçtan köke dalga" (mimoza).

### 3.4 Kapalı halin güzel biçimleri ("yumruk" yerine)

Doğadan:
- **Enginar/koni (imbricate):** Üst üste binen pullar. Protea başı enginara benzetilir [A: Wikipedia Protea cynaroides]. Aestivation sözlüğünde "imbricate" çatı kiremiti gibi bindirme demek [A: Wikipedia Aestivation].
- **Çarkıfelek/dönmüş (contorted):** Her parça komşusunun bir kenarı altında, diğer kenarı üstünde; pinwheel (fırıldak) benzeri bükülme [A: Wikipedia Aestivation]. Uyarı: Bu spiral'e çok yaklaşır, projede tüketilmiş çizgiye dönebilir.
- **Valvate:** Parçalar kenarlarında dikişle buluşur, bindirme yok [A]. Top-down'da "dikişli top" = temiz bir yıldız kesimli küre.
- **İçe/dışa katlanmış (induplicate/reduplicate):** Kenarlar içe ya da dışa katlı [A]. Bu, "katlanmış şemsiye".
- **Kâğıt top gibi buruşuk:** Haşhaş petalleri tomurcukta buruşuktur ve açılınca düzleşir [B: arama özeti, Wikipedia Poppy]. Bazı türlerde tomurcuk açılmadan önce origami benzeri katlı biçim alıyor, süreç saatler içinde bitiyor [B: arama özeti, California Academy blogu].
- **Sert kabuklu top:** Rafflesia tomurcuğu koyu lahana gibi, ~30 cm, aylar/yıl [A: Wikipedia Rafflesia arnoldii].
- **Mühürlü küre:** Yıldız mantarın (Geastrum) dış katı yıldız ışınlarına yarılır, ortadaki spor kesesi koni biçimli bir ağız taşır, bazı türlerde nemde ışınlar açılır, kuruyunca kapanır; bazı türlerde dış kat ayrılır ve spor kesesi kemer üstünde yükselir [A: Wikipedia Geastrum].
- **Kozalak:** Pullar nemde kapanıp kuruyunca açılır; enerji harcamaz [B: arama özeti, Freiburg sayfası sertifika hatası verdi].
- **Rose of Jericho:** Kuruyunca dalları sıkı yumruk gibi bir topa kıvrılır, nemle açılır, bu tekrarlanabilir [A: Wikipedia Anastatica].
- **Yumuşak torba (deniz anemonu):** Rahatsız olunca anemon söner; dokunaçlar ve ağız diski gövdeye geri çekilir, bazı türler parlak bir topa büzülür [A: Wikipedia Sea anemone].
- **Modüler kağıt top (kusudama):** Eş katlanmış (çoğunlukla piramit) birimlerin çokyüzlü geometriyle birleştirilmesi [A: Wikipedia Kusudama].
- **Origami "bloom pattern":** Çiçek benzeri, dönel simetrik kıvrım desenleri; düz ve sıkı katlanır, katlıyken kamalar bir sarmal gibi üst üste istiflenir; tek kâğıttan yapılır [A: humanprogress.org, BYU çalışması].
- **Kirigami/pop-up:** Tek bir kesikle itme, yunuslama, yalpa ve kaldırma hareketleri yapılabiliyor [B: arama özeti, arXiv 1707.05477]. Çiçeğe özgü bir kirigami kaynağı bulamadım [doğrulanamadı].

Oyun için en cazip kapalı hal sıralaması [T]: (1) mühürlü yıldız küre (earthstar), (2) enginar pul konisi (protea), (3) dikişli (valvate) yıldız küre, (4) sıkı sarılı merkez konisi (kamelya), (5) katlı şemsiye (plicae), (6) Venüs "fermuar ağız".

---

## 4. Polen ve böcek görüşü: "tutunma noktası burası" işaretlerinin doğadaki karşılıkları

- **Nektar kılavuzları:** Çiçeğin ödülüne (nektar, polen ya da ikisi) yönlendiren çizgi, nokta ya da leke işaretleri; insan gözüyle görünenler de, yalnızca UV'de görünenler de var; böcekler bunları görebilir ve "arı moru" (bee violet) denen renk algılanır; kılavuzlar ödüle ulaşmayı hızlandırır ve nektar hırsızlığını azaltabilir [A: Wikipedia Nectar guide].
- **UV "bullseye":** Taç yaprağın tabanı UV'yi emer, ucu yansıtır; saha çalışmasında UV deseni arı ve sinek çekimini artırmış, ancak kanıt "karışık" [B: arama özeti]. Bu yüzden "tuzak merkez" fikrini bilimsel olarak kesin sunmuyorum.
- **Arı fotoreseptörleri:** UV, mavi ve yeşil için üç tür (tepe ~350, 440, 540 nm) [A: Wikipedia Bee learning and communication]. Arıların kırmızıyı nasıl gördüğü o sayfada yok [doğrulanamadı].
- **Polinatör sendromları:** Arı çiçekleri çoğunlukla sarı-mavi, çoğu UV kılavuzlu; kuş çiçekleri büyük kırmızı-turuncu borular; böcek (böcek/kınkanatlı) çiçekleri büyük, yeşilimsi-kirli beyaz, dish biçimli; güve çiçekleri beyaz, geceleri açık; konan kuşlar için sağlam iniş platformu var [A: Wikipedia Pollination syndrome].
- **Merkez/halka işaretlerinin doğal örnekleri (hepsi doğrulandı):**
  - Pasiflora: eş merkezli renkli koronal halkalar; tabanda koyu lif halkası [A: gardendesign.com].
  - Anemon: siyah merkez, çevresinde organ tacı [A].
  - Lale: her tepalin tabanında farklı renkli leke [A].
  - Edelweiss: ortada sarı çiçekçik kümesi [A].
  - Venüs sinekkapanı: lob başına üç tetik tüyü [A].
  - Hibiscus: ortadan çıkan uzun direk, ucunda beş nokta [A].
  - Kuş cenneti: sağlam sivri spata (kuş kafası/gaga) polinatör için iniş yeri [A: Wikipedia Strelitzia reginae]; üç turuncu çanak, üç mor-mavi taç (ikisi ok biçimli nektar kabı) [A].
- **Aposematizm (uyarı renkleri):** En yaygın ve etkili uyarı renkleri "kırmızı, sarı, siyah ve beyaz"; yüksek kontrast yapraklarla zıtlaşır, ışık/gölge değişiminde de dayanıklı, doymuş [A: Wikipedia Aposematism]. Bitkilerde uyarı sinyali hipotez düzeyinde [A].
- **Oyun için çıkarım [T]:** "Yakında kapanacak" uyarısı için doğal bir dil: yaprak uçlarında iri, az sayıda sarı-siyah bant ya da koyu uç (3-4 geniş bant, ince çizgi değil). Halftone/çizgi desenine kaçmaması için bantları yaprağın geometrisine göre (uca doğru koyulaşan tek bir ton bandı) tutmak daha güvenli. "Açık = tutun" için merkez halkasının parlaması/büyümesi (nektar kılavuzu gibi).
- **Renk uyarısı [T]:** Dünyada gökyüzü mavisi, pembe, kırmızı (ped), yeşil (asit), turuncu (zemin) ve karakterde magenta-turuncu-sarı zaten dolu. Çiçek için boş kalan bölge: koyu mor/indigo, beyaz-krem (krema şeridine dikkat), derin lacivert. Karakterle karışmama için pembe-magenta ve parlak sarıdan uzak durmak, merkez işaretini koyu/açık kontrast olarak ayırmak mantıklı. Karar geliştiricinin.

---

## 5. Yeni bir çiçek anatomisi: botanik terimlerle ek parçalar

Temel (doğrulandı): çanak yaprak/çanak (calyx): koruyucu, genelde yeşil, sert; taç (corolla): ince, renkli; androecium: stamenler (filament + anter); gynoecium: pistil (stigma, stil, ovaryum); brakte: değişmiş yaprak; nektarlar; **korona:** organlara benzeyen taç gibi çıkıntılar, "pseudonectary" nektarsız taklitler [A: Wikipedia Flower].

Kendine özgü yapan ekler ve oyun karşılıkları:

| Parça | Doğadan örnek (doğrulama) | Oyunda ne olur [T] |
|---|---|---|
| **Çanak (sepal) yıldızı** | Aquilegia'da beş taç gibi çanak; Nigella'da yalnızca çanaklar renkli, taçlar minik [A] | Dıştaki "taç" asıl çanak olur; iç yapraklar farklı renk/biçim. İki katman farklı kimlikte, aynı pembe-damla değil. |
| **Brakte (renkli yaprak)** | Kornel: 4 beyaz brakte, ortada küçük çiçek kümesi; bugenvil: 3 ya da 6 kağıtsı brakte [A: Bract, Bougainvillea] | Büyük "yaprak" aslında koruyucu kağıt; ortada minik çiçekçik tutamı = tutunma işareti. |
| **Involucre (brakte halkası)** | Havuç/papatya ailelerinde çiçek kümesi altında brakte çemberi [A: Bract] | Çiçeğin altında ikinci bir sert ve koyu halka (açılınca kenarda dişler gibi görünen). |
| **Korona (iplik taç)** | Pasiflora, iki katman, sekiz seriye çıkabilir [A] | İplikli taç = ayırt edici ana özellik (tohum 1). |
| **Spata + spadiks** | Arum: kenarı nervürlü/fırfırlı çan + yükselen kule; ısınıp kokuyu darbeyle verir [A] | Merkez kule + halka kenar; ısı titreşimi VFX'i. |
| **Organ halkası (stamen)** | Anemon: organ tacı; şakayık 50-160 serbest stamen; hibiscus turuncu uçlu anterler [A] | Merkezin etrafında iri noktalı halka; yaprak açıldığında gösterişli "pop" halkası. |
| **Staminodlar (kısır organ)** | Aquilegia ve Japon şakayığı: yassı/içe kıvrık yaprak biçimli kısır organlar [A] | Merkezde yaprağa benzeyen minik "yedek" yapraklar, ikinci mikro-katman. |
| **Stil (şemsiye)** | Sarracenia: organların üstünde şemsiye biçimli 5 uçlu stil [A] | Merkez çatı: beş uçlu minik şemsiye, "tutunma" simgesi. |
| **Nektar mahmuzu** | Aquilegia [A] | Dışa uzanan kısa boynuzlar, yalnızca siluette (ince değil, kalın uçlu). |
| **Polen tozu bulutu** | Şakayık anterleri yarık boyunca açılır [A]; polen bulutu görseli doğrudan kaynaklı değil | Açılış "pop"ında sarı toz parçacıkları (VFX); kapanırken geri çekilen toz [T]. |

---

## 6. Çiçek benzeri mikroskopik ve deniz yapıları

- **Kar tanesi:** Altıgen bazdan altı dendrit; her kol yan dallar çıkarır; kollar benzer koşullarda büyüdüğü için benzer olur; gerçek kristallerin %0,1'inden azı kusursuz altılı simetride [A: Wikipedia Snowflake]. Siluet olarak altı dallı yıldız, dallı. Çiçek gibi görünmez ama çiçek yerine oturabilir. Risk: simetrik, bilindik "kar tanesi".
- **Diatom:** Merkezi (radyal simetrik: yuvarlak, üçgen, kare, eliptik) ve pennat (iki yanlı) olarak iki grup; silika kabuk (frustule) iki yarım kapaktan oluşur; çizgisel gözenek sıraları, altıgen delikler, kaburgalar, minik dikenler [A: Wikipedia Diatom]. Kapak (valv) yapısı = "kapaklı kutu" mantığı; kapalı hal doğal.
- **Radyolarya:** Delikli küresel kapsüller; iç içe geçmiş küreler (bazı fosillerde 4 iç içe küre); düzenli aralıklı dikenler (2, 6, 12, 20 sayıları saptanmış); bazıları düzgün çokyüzlü (ikosahedron: 12 diken, 20 yüz); 0,1-0,2 mm [A: Wikipedia Radiolaria]. Siluet: dikenli top. Çiçek gibi görünmez; "dikenli koza" kapalı hal olarak ilginç.
- **Deniz anemonu:** Çok katlı eşmerkezli dokunaç halkaları; merkezde yarık biçimli ağız; tabanı yapışık gövde; sakin hâlde söner ve dokunaç/disk gövdeye çekilir; bazı türler parlak bir topa büzülür [A: Wikipedia Sea anemone].
- **Mercan polipleri:** Ağzın çevresinde dokunaç halkaları; taşlı mercanda altılı simetri, yumuşak mercanda sekiz dallı dokunaç; polipler kadeh biçimli oyuklara (corallite) çekilebilir [A: Wikipedia Coral].
- **Denizyıldızı:** Merkez disk, genelde beş kol, ama altı-yedi, bazı türlerde 10-15, hatta 50'den fazla kol; yüzey düz, yumrulu ya da dikenli, üst üste binmiş plakalar; kırmızı-turuncu-mavi-gri-kahve renkler [A: Wikipedia Starfish].
- **Yıldız mantar (Geastrum):** Yukarıda 3.4'te; dış kat yıldız ışınlarına yarılır [A].
- **Mantar halkası:** Bu açıda yeterince okuyup doğrulayamadım [doğrulanamadı].

---

## 7. Her fikir için "oyunda nasıl görünür?" (özet paragraflar)

**Pasiflora tacı.** Beş geniş, hafif sivri yaprak; üstünde iki kat halinde, yelpaze gibi açılmış kalın iplik tacı; iplikler uca doğru renk değiştirir (iki halka bandı); ortada kısa kule ve üç noktalı üst yapı. Kapalıyken iplikler içeride sıkışık, yapraklar çanak gibi sivri dikişli kapanır.

**Protea/enginar.** Üç sıra uzun sivri brakte (kama), her sıra biraz kaymış; ortada yastıksı kubbe. Kapalıyken üst üste binmiş pullardan oluşan sivri bir koni. Açılırken dıştan içe pullar yatar; merkez en son görünür.

**Nilüfer/lotus.** Ortada sıkışan, uçları sivri 12-18 dar yaprak, dıştan içe renk gradyanı. Kapalıyken yapraklar tek bir mızrak demetinde. Gradyan aynı zamanda açık/kapalı geri bildirimi.

**Orkide (üçlü).** Üç sivri çanak yıldızı + iki yan kanat + öne uzanan dudak. Üç katlı simetri, tanıdık "çiçek" şablonundan uzak. Dudak = iniş platformu.

**Aquilegia.** Beş geniş çanak yaprağı arasından dışa kıvrılan beş kalın boynuz, ortada organ halkası. Spiral hissi riski.

**Kamelya/şakayık.** Kademeli, kayık eşmerkezli sıralar, ortada sıkı koni. En "çiçek" olan tohum.

**Edelweiss.** İki yünlü yıldız, keçemsi taraklı kenar, ortada sarı çiçekçik kümesi. Kapalıyken yünlü tüy topu.

**Anemon.** Beş-sekiz geniş yaprak, koyu "göz" ve organ tacı. Kapalıyken göz kapalı gibi yaprakla örtülü.

**Rafflesia.** Beş kalın etli lob, büyük yuvarlak lekeler, ortada dikenli disk. Kapalıyken katmanlı "lahana" topu.

**Devasa arum.** Fırfırlı kenarlı huni halkası, ortada yükselen kule ve "ısı" parıltısı. Kapalıyken sarılı silindir.

**Lale.** Altı yaprak, tabanda koyu leke yıldızı. Kapalıyken yumurta fincan.

**Çan/gentian.** İç yıldız + dış çanak dişleri, loblar arası kıvrım. Kapalıyken katlı şemsiye.

---

## 8. ŞEKİL TOHUMLARI (10 adet)

Her tohum: siluet, katman, merkez, kapalı hal, risk.

### Tohum 1: İplik tacı + beş yaprak (pasiflora)
- **Siluet:** 5 geniş yaprak, üstünde sık iplik tacı; yaprak ucu yuvarlak-sivri karışık.
- **Katman:** 3 (yaprak, iplik tacı, merkez kule).
- **Merkez:** Kısa kule + üç noktalı tepe (koyu halka tabanı).
- **Kapalı hal:** Dikişli (valvate) beş parçalı sivri tomurcuk; iplikler içeride.
- **Risk:** İplikler shader'da 40-80 ince çizgi olursa 9 birimde sadece "tüylü halka" okunur; 12-16 kalın iplik gerekir. Spiral tuzağı: koronayı çarpık sıralama ile çizmemeye dikkat. Orta zorlukta.

### Tohum 2: Sürahi ağzı + çok kapaklı (Nepenthes/Sarracenia)
- **Siluet:** Renkli kalın halka (peristome) + 3-4 kapak kanadı yıldız gibi.
- **Katman:** 3 (halka, kapaklar, koyu derinlik).
- **Merkez:** Koyu boşluk; ortadaki koyu delik "tutunma" odağı.
- **Kapalı hal:** Kapaklar ağzı örter, kapalı yıldız.
- **Risk:** Derinlik hissi top-down'da yalnızca gölge/koyu halkayla verilir; "delik" tutunma yerine "tehlike" gibi okunabilir. Orta.

### Tohum 3: Dört lobu Venüs tuzağı (kirpik ağzı + üç direkçik)
- **Siluet:** 4 (ya da 6) büyük böbrek biçimli lob, kenarda 8-12 iri diş.
- **Katman:** 2 (lob, diş kenarı) + merkezde direkler.
- **Merkez:** Üç kısa direkçik (tetik tüyleri) + açık merkez alan.
- **Kapalı hal:** Lobların diş sıraları karşılıklı kilitlenmiş "fermuar" haç; uyarıda lobların dışa gerilmesi, sonra ani kapanma.
- **Risk:** Dişler çok küçük olursa kaybolur; kırmızı iç yüz fırlatma pedleriyle karışabilir (iç yüz rengi bilerek seçilmeli). Diş çizgisi SDF'de kolay (polar tırtık). Düşük-orta zorluk. Tehlike anlatımı en güçlü olan tohum.

### Tohum 4: Beş uçlu yıldız + iç çan (campanula/gentian)
- **Siluet:** 5 sivri lobu dış yıldız, aralarında çanak dişleri; içte 5 loblu daha küçük çan ağzı.
- **Katman:** 2 yıldız (dış çanak, iç taç) + merkez.
- **Merkez:** Küçük yıldız biçimli koyu delik.
- **Kapalı hal:** Plicae kıvrımlı katlı şemsiye (beş dilimli kapalı koni).
- **Risk:** Mevcut 6 yapraklı çiçeğe en çok benzer genel "yıldız çiçeği" okuması riski; iç/dış iki yıldızın zıt tonu şart. Düşük zorluk.

### Tohum 5: Kademeli sivri nilüfer
- **Siluet:** 14-18 dar sivri yaprak, 3 sıra, dıştan içe boyut ve ton gradyanı.
- **Katman:** 3.
- **Merkez:** Yaprağa benzeyen organ halkası + altın nokta.
- **Kapalı hal:** Tek bir mızrak demeti, yapraklar dikey toplu.
- **Risk:** Yaprak sayısı çok olunca desen gibi okunur; ince yapraklar 0,5 birimin altına düşebilir. Düşük-orta.

### Tohum 6: Yünlü çift yıldız (edelweiss)
- **Siluet:** İki üst üste yıldız, 7-9 kısa, keçemsi taraklı uç; ortada 5-6 sarı yuvarlak.
- **Katman:** 2 yıldız + merkez.
- **Merkez:** Sarı noktalar kümesi.
- **Kapalı hal:** Yünlü tüy topu; brakteler yukarı toplanır.
- **Risk:** Yünlü kenar = gürültü ile modüle edilmiş SDF sınırı; küçük pürüz 9 birimde kaybolur, iri taraklar (kalın tutam) gerekir. Yünlü doku "cel-shaded temiz vektör" hissine aykırı düşebilir. Orta.

### Tohum 7: Enginar pul tacı + iğne yastığı (protea)
- **Siluet:** Üç sıra uzun sivri kama; ortada dolu kubbe, kubbede iri noktalar.
- **Katman:** 4 (iki-üç kama sırası, kubbe).
- **Merkez:** Kubbe, noktalı: kendiliğinden iyi bir "tutun" hedefi.
- **Kapalı hal:** Pullardan sivri koni (imbricate), çok okunur.
- **Risk:** Kama sıraları 3 katman dönerse yeniden "damla yapraklı çiçek"e benzemesin diye kamaları uzun ve keskin tutmak gerekir. Kubbe shader'da 2-3 ton bandıyla kolay. Düşük zorluk.

### Tohum 8: Dokunaç halkaları (deniz anemonu / mercan)
- **Siluet:** 12-16 iri yuvarlak uçlu parmak, iki halka halinde kaymış; ortada yarık biçimli ağız.
- **Katman:** 2 halka + merkez.
- **Merkez:** Yarık ağız (yarım ay biçimli koyu şerit).
- **Kapalı hal:** Büzülmüş parlak top (söner ve gövdeye çekilir).
- **Risk:** "Çiçek" gibi okunmayabilir (bu bir artı da olabilir); parmaklar el/kanca temasıyla tematik uyum sağlayabilir [T]. Organik, hoş olmayan hissi riski. Parmakların eğilmesi için bükülebilir ızgara mesh uygun. Orta.

### Tohum 9: Yıldız mantar (mühürlü küre -> yarılan ışınlar)
- **Siluet:** 5-7 sivri ışın, merkezde spor kesesi küre; kesenin üstünde küçük koni ağız; ışınların altından kemer/halka.
- **Katman:** 3 (dış ışınlar, kemer halkası, merkez küre).
- **Merkez:** Küre + koni ağız (nokta); koni tutunma odağı.
- **Kapalı hal:** Dikişli yuvarlak küre (mühürlü); açılırken ışınlar dışa devrilir.
- **Risk:** Altı sivri ışın "genel yıldız" gibi okunabilir; merkez küre+koni ve kemer halkası ayırt edici olmalı. Mantar çağrışımı çiçek beklentisinden uzaklaşabilir (bunun getirisi: çok özgün). Bükülebilir mesh, kapanma "snap" için çok uygun. Düşük zorluk.

### Tohum 10: Üç kağıt brakte (bugenvil/kornel)
- **Siluet:** 3 (ya da 4) büyük, yuvarlak-üçgen kağıtsı yaprak; ortada 3 minik beyaz çiçekçik tutamı.
- **Katman:** 2 (brakte, çiçekçik).
- **Merkez:** Üç küçük noktacık = tutunma işareti.
- **Kapalı hal:** Brakteler yukarı katlı üç kanatlı çadır/kağıt poşet.
- **Risk:** Karakterin iki ucundaki poşetle (taç/pelerin) görsel çatışma/karışma riski; yaprak "kağıt" hissi karakterin şekerlemesiyle tematik uyumlu ama karışabilir. Çok basit; üç parça 9 birimde çok okunur. Düşük zorluk.

(İsteğe bağlı 11. tohum, daha zor: Aquilegia boynuzlu yıldız; spiral hissi riski yüzünden sıraya almadım.)

---

## 9. En umut vadeden 3 tohum (görüşüm)

1. **Tohum 9, Yıldız mantar.** Kapalı hal en güçlü ve en özgün (mühürlü, dikişli küre); doğada da gerçekten böyle açılıyor [A]; snap kapanış için doğal; shader'da SDF + bükülebilir mesh ile kolay. Risk: "çiçek gibi okunmaz" ama bu bir güç olabilir.
2. **Tohum 3, Dört lobu Venüs tuzağı.** Oyun mekaniğiyle (tutulurken kapanıp fırlatma) anlamsal olarak birebir uyum; uyarı animasyonu için gerçek fiziğe dayanan bir ipucu var (gergin eğrilik, sonra ani flip [A]); üç direkçik merkez işaretini çözüyor. Dikkat: kırmızı iç yüz fırlatma pedleriyle karışabilir.
3. **Tohum 1, Pasiflora iplik tacı.** Çiçek hissini koruyup jenerik çiçekten en çok kopan (iplik tacı + merkez kule); merkezde doğal "bullseye" halkaları var [A]. Yedek: Tohum 7 (protea/enginar), çünkü kapalı hali çok okunur.

---

## Doğrulanamayanlar

- Kew blogu ve USDA (columbine ve görsel ipuçları) sayfaları 403 verdi; PMC makaleleri (Venüs mekaniği, çiçek açılma derlemesi) bot doğrulamasına takıldı; Freiburg kozalak sayfası sertifika hatası verdi. Bu kaynaklardaki bilgiler yalnızca arama özeti düzeyinde [B].
- Haşhaş tomurcuğunda buruşuk yaprak ve origami benzeri katlanma yalnızca arama özetinden [B]; "ptychosis" terimi doğrulanmadı.
- Kirigami/pop-up çiçek açılışı için çiçeğe özgü kaynak bulunamadı. Chris Palmer "flower tower" yalnızca arama sonucu listesinde geçti, sayfası açılmadı; Ushio Ito eserine dair bir şey bulunamadı.
- Aquilegia'nın gece davranışı, anemonun gece kapanması, kardelenin sıcaklıkla açılması, lisianthus tomurcuk sarmalı, laledeki tomurcuk biçimi, kuş cennetinin top-down görünümü, Rafflesia'nın beş lobu: sayfalarda yoktu ya da doğrulanamadı.
- Protea braktelerinin katman düzeni doğrulanmadı (sayfa anlatmıyordu); "çok sıralı sivri kama + kubbe" betimlemesi benim genel bilgimden.
- Çiğdem/lale termonastisinin sıcaklık eşiği ve hızı (kaç derece, kaç dakika) bulunamadı; yalnızca Colchicum zaman atlamalı videosunun (~70 dk) bilgisi var.
- Arıların kırmızıyı görüp görmediği doğrulanmadı.
- UV bullseye'ın polinatör çekmedeki etkisi arama özetinde "karışık"; bu yüzden bilimsel kesinlik iddia edilmedi.
- Mantar halkaları bu açıda yeterince okunmadı.
- Venüs tuzağı iki dokunuş penceresi: Wikipedia ~20 sn, UZH ~30 sn; çelişki çözülmedi.
- Aestivation'da "crumpled" ve "decussate" parçalarının biçimi sayfada anlatılmadı.
- Kapalı hal betimlemeleri (ör. pasiflora tomurcuk dikişi, edelweiss kapanması, kornel/bugenvil çadır hali) benim tasarım yorumum [T], doğa olgusu değil.

## Yöntem notu

- Araçlar: WebSearch ve WebFetch. WebFetch'in yanıtı küçük bir model tarafından sayfa üzerinden özetleniyor; yani ham metni değil özeti okudum, bu yüzden sayfada olmayan şeyleri model "sayfada yok" diye bildirdiğinde bunları ayrıca doğrulanamayan olarak işaretledim.
- Çoğu doğrulama Wikipedia sayfalarından ve birkaç üniversite/kurum sayfasından (UZH, Indiana, JIC, BYU haberi) geldi. Wikipedia ikincil kaynak; kritik sayısal değerler (ör. 0,1 sn, 10-20 ms) için birincil makale doğrulaması yapılmadı.
- Telifli metin kopyalanmadı; tüm ifadeler kendi cümlelerimle özetlendi.
- Projede tüketilmiş çizgiler (Bauhaus, op-art, halftone, folk, spiral) öneri olarak sunulmadı. Contorted tomurcuk ve Aquilegia'da spiral'e kayma riski açıkça işaretlendi.
- Tohum puanlaması ve "en umut vadeden 3" öznel bir değerlendirme; sahnede deneme yapılmadı. Görsel his geliştiricinin kararı.
