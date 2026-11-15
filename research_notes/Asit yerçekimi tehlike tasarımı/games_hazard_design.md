# Oyunlarda hareketli ölümcül tehlike, havada tutulan sıvı ve yerçekimi teknolojisi tasarımı (Rock The Game asit-yerçekimi tehlikeleri için)

> Kapsam notu: 17 web çağrısıyla yapıldı. Arama sonuçları çoğunlukla wiki/blog düzeyinde kaldı; Celeste, Super Meat Boy veya Rayman için hazard okunabilirliğine özel bir GDC konuşması ya da geliştirici röportajı **bulunamadı**. Bu oturumda kaynakla doğrulanamayan oyun bilgileri "Cited Findings"e değil, "Inferences" bölümüne "(genel bilgi, bu oturumda doğrulanmadı)" etiketiyle ya da "Gaps"e yazıldı.

## 1. Oyunlar hareketli tehlikeleri ve havada duran / kapalı sıvıları nasıl gösteriyor?

### Takeaway
Doğrulanabilen örneklerde ortak desen şu: ölümcül nesne **tek, tutarlı bir görsel kimlik** taşıyor (testere, dikenli top, asit havuzu) ve hareket türü nesnenin kendisinden okunuyor (dönen testere, merkez etrafında dönen dikenli toplar). "Havada tutulan sıvı" en açık haliyle Portal 2'de var: Excursion Funnel'ın mavi ışın tüpü, içine giren jeli ağırlıksız hale getirip akış yönünde taşıyor. Yani "alan + içinde asılı madde" fikrinin oyunda doğrulanmış bir örneği var.

### Cited Findings
- **Super Meat Boy:** 300'ü aşkın bölümde oyuncu çöken bloklardan, testerelerden ve başka ölümcül engellerden kaçıyor; testere oyunun imza tehlikesi. — [Wikipedia: Super Meat Boy](https://wikipedia.com/wiki/Meat_Boy)
- **Super Meat Boy 3D:** Sluggerfly'ın geliştiricilerine göre en büyük zorluk ayrıntılı ortamı korurken okunabilirliği dengelemekti. Fazla ayrıntı bölümün okunmasını zorlaştırıyor, çünkü Meat Boy kamerada küçük kalıyor. — [Epic Games Store haberi](https://store.epicgames.com/en-US/news/how-super-meat-boy-3d-keeps-its-gory-identity); [PlayStation Blog](https://blog.playstation.com/?p=417022)
- **Super Meat Boy postmortem** (Game Developer Magazine yeniden basımı) var, ama arama özetleri yalnızca araçları ve geliştirme sürecini kapsadı; testere tasarımına dair bir alıntı alınamadı. — [Gamasutra/Game Developer postmortem](https://gamedeveloper.com/audio/postmortem-team-meat-s-i-super-meat-boy-i-)
- **Sonic, Orbinaut (Uni Uni):** Merkezdeki küresel robotun etrafında dikenli toplar dönüyor. Bu toplar hem silah hem kalkan işi görüyor. 2.5D bölümlerde yanlamasına, 3D bölümlerde yatay düzlemde dönüyorlar. — [Sonic Wiki: Orbinaut (Uni Uni)](https://sonic.fandom.com/wiki/Orbinaut_(Uni-Uni))
- **Sonic, Unidasu varyantı:** Sonic'i fark edince toplar dönmeyi bırakıp tek tek ve hızla oyuncuya fırlatılıyor. — [BreezeWiki: Unidasu](https://breezewiki.discard.no/sonic/wiki/Unidasu)
- **Sonic, Gohla:** Dikenli top yerine ateş topları kullanıyor. Oyuncu yaklaşınca ateş toplarını birkaç saniye onun etrafında döndürüp sonra çarptırıyor. — [BreezeWiki: Gohla](https://breezewiki.discard.no/sonic/wiki/Gohla)
- **Sonic, Zombie Orbinaut:** Gövdesini korumak için etrafında üç dikenli nesne döndürüyor. — [Sonic Wiki: Zombie Orbinaut](https://sonic.fandom.com/wiki/Zombie_Orbinaut)
- **"Spike Balls of Doom"** türler arası bir kalıp olarak belgelenmiş (dikenli top / gürz tehlikesi). — [TV Tropes: Spike Balls of Doom](https://www.tvtropes.org/pmwiki/pmwiki.php/Main/SpikeBallsOfDoom)
- **Hollow Knight, asit:** Asit havuzları, Isma's Tear yeteneği alınana kadar hasar veren bir çevre tehlikesi. Yetenek alınınca şövalye asitte hasar almadan yüzebiliyor, yani aynı sıvı oyun ilerledikçe ölümcülden geçilebilire dönüşüyor. — [Hollow Knight Wiki: Isma's Tear](https://hollowknight.wiki/w/Isma's_Tear)
- Hollow Knight'ta Isma's Grove, aşağı Hallownest'te biriken asidin kaynağı olarak gösteriliyor (sıvıya lore'da bir köken verilmiş). — [Hollow Knight Wiki: Royal Waterways](https://hollowknight.wiki/w/Royal_Waterways)
- **Portal 2, Excursion Funnel:** Oyuncunun istediği an girip çıkabildiği geniş, silindirik mavi bir ışık huzmesi. İçine giren her şey ağırlıksız kalıyor ve akış yönünde taşınıyor. Galeri görselleri ileri ve geri akış için ayrı. — [Portal Wiki: Excursion Funnel](https://theportalwiki.com/wiki/Excursion_Funnel)
- **Portal 2, jel + funnel:** Huzmeye yakalanan jeller bir duvara çarpana ya da funnel kapanana kadar akış yönünde yavaşça sürükleniyor; yani sıvı alanın içinde asılı ve hareketli. — [Portal Wiki: Excursion Funnel](https://theportalwiki.com/wiki/Excursion_Funnel)
- **Portal 2, jeller:** Üç ana tür var: Repulsion, Propulsion, Conversion. Jel değdiği yüzeyi kaplayıp hem fiziksel özelliğini hem rengini değiştiriyor. Conversion jeli başta saf beyazdı, sonra daha koyu bir griyle değiştirildi. — [Valve Developer Community: Gel (Portal 2)](https://developer.valvesoftware.com/wiki/Gel_(Portal_2))
- **Donkey Kong Country Returns, silüet bölümleri:** Düşmanlar da silüet olarak çiziliyor ve bazıları saldırıya geçene kadar arka plan nesnesi gibi görünüyor. Bu, okunabilirliği bilerek düşüren bir sürpriz tasarımı. — [Siliconera playtest](https://www.siliconera.com/2010/11/21/donkey-kong-country-returns-playtest-a-barrel-blast-from-the-past/)
- **Rayman Legends:** İnceleme ilk saatte çevrende açılan dikenli dokunaçlardan ve oyuncuyu platformdan platforma kovalayan dev ejderhalardan söz ediyor. — [Trusted Reviews](https://www.trustedreviews.com/reviews/rayman-legends)
- Game Maker's Toolkit'in Rayman Legends videosu, bir dünyanın öne çıkan mekaniğinin o dünyanın bölümleri boyunca nasıl taşındığını inceliyor (altyazı dökümü). — [GMTK, Breaking Down the Best World in Rayman Legends (Amara altyazı)](https://amara.org/videos/XrnrkoeBp29g/en/2090847)
- **Celeste:** Bir analize göre sanatta yapılan değişiklikler, okunabilirlik ve oyuncuyla iletişim için yapılmış (örneğin odaların renk kodlanması). — [Game Developer, Josh Bycer: Dissecting Design, Celeste](https://gamedeveloper.com/design/dissecting-design----the-in-accessible-nature-of-celeste)

### Inferences
- (Genel bilgi, bu oturumda doğrulanmadı.) **Celeste, "crystal spinner" / dust bunny:** Statik ya da bir yol boyunca hareket eden dikenli kristal kümeleri. Renk paleti bölüm temasına göre değişiyor ama siluet (sivri kristal yıldız) hep aynı kalıyor. Biçim "ölüm" anlamını taşıyor, renk değişebiliyor. Bizim acid-green tutarlılığımız bunun tersi: renk sabit, biçim değişiyor. İki yaklaşım da işe yarıyor, yeter ki **bir kanal sabit kalsın**.
- (Genel bilgi.) **Super Mario serisi, Fire Bar:** Merkez bloktan uzanan ateş topu zinciri. Dönen tehlikenin yarıçapını ve pivotunu nesnenin kendisi gösteriyor, ek bir yol çizgisine gerek kalmıyor. Bizim "core + acid kolu/halkası" fikrimiz bu kalıpla örtüşüyor.
- (Genel bilgi.) **Splatoon:** Mürekkep, takım rengiyle "güvenli / tehlikeli zemin" anlamı taşıyan bir sıvı. Sıvının rengi doğrudan oyun kuralını anlatıyor; acid-green = ölüm eşlemesinin en güçlü ana akım örneği.
- (Genel bilgi.) **Prey (2017), GLOO:** Havada asılı kalıp sertleşen köpük. "Havada duran madde" fikrine teknolojik bir açıklama veriyor ama ölümcül değil, bir araç.
- (Genel bilgi.) **Cuphead:** Parry edilebilir mermiler pembe renkle ayrılıyor. "Tek renk = tek kural" ilkesinin klasik örneği. Kendi renk kurallarımızla çakışma riskine dikkat: bizde pembe oyuncunun rengi.
- Hollow Knight ile ortak nokta: bizim asit şeritlerimiz de parlak yeşil. Hareketli tehlikeleri **aynı asit yeşiliyle** göstermek, oyuncunun zemindeki asitten öğrendiği "yeşil = ölüm" kuralını havadaki tehlikeye otomatik olarak aktarır.

### Gaps
- Pizza Tower, Katana Zero, Inside/Limbo, Ori, Dead Cells, Gravity Rush, Control, Kirby, Spelunky, Super Mario Wonder ve N++ için hareketli tehlike ya da sıvı görselleştirmesi üzerine kaynaklı bulgu toplanamadı (çağrı bütçesi; "Inside zero gravity water" araması alakasız sonuç döndü).
- Celeste crystal spinner için wiki sayfası bulunamadı (celeste.ink 404 verdi, fandom aramasında ayrı bir sayfa çıkmadı).
- Super Meat Boy postmortem'inin testere bölümü okunamadı.

## 2. Bir tehlikeyi hızda "ölümcül" okutan ilkeler neler? GDC / geliştirici kaynakları

### Takeaway
En sağlam kaynak, Death's Gambit tasarımcısının GDC konuşmasının yazılı versiyonu. Okunabilirlik ikiye ayrılıyor: **telegraphing** (öncesi: geleceği görmek) ve **expectations** (sonrası: görüntüyle sonucun tutarlı olması). Platformer'a uyarlanınca: tehlike hareketini önceden belli etmeli ve "ölümcül görünen = ölümcül, öyle görünmeyen = güvenli" kuralı hiç bozulmamalı. Ayrıca fazla ayrıntı hızda okunabilirliği düşürüyor (SMB 3D), ön plan öğeleri arka plandan görsel ipuçlarıyla ayrılmalı (Cuphead analizi).

### Cited Findings
- Okunabilirlik, telegraphing (öncesi) ve expectations (sonrası) olarak ayrılıyor. Kontroller ve hitbox'lar iyi ayarlıysa oyuncu hayal kırıklığını en çok bu belirliyor. — [Game Developer: Designing for Difficulty: Readability in ARPGs](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs); [Medium kopyası](https://medium.com/p/c3d04f642c9a)
- Expectations, saldırının sonucuyla örtüşen ve tutarlı kalan bir görsel dil kurmak demek. Büyük görünen bir şeyden ölmek, küçük görünen bir vuruştan ölmekten daha adil hissettiriyor. — [Game Developer, aynı makale](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs)
- Zorluğu artırmak için okunabilirlikten ödün verilmemeli. Görsel dil erken öğretilmeli, tutarlı tutulmalı. — [Game Developer, aynı makale](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs)
- Oyuncu hasardan kaçınacaksa önce hasarın geldiğini bilmeli. Telegraphing, satranç ve pokerdeki istemsiz sinyalin bilinçli tersi. — [Game Developer: Enemy Attacks and Telegraphing](https://gamedeveloper.com/design/enemy-attacks-and-telegraphing)
- Çevre tehlikeleri okunabilir ve kaçınılabilir olduğunda adil. Oyuncunun beceriyle görüp kaçabileceği şekilde açıkça telegraph edilmeliler. — [Bugnet: How to design environmental hazards that are fair](https://bugnet.io/blog/how-to-design-environmental-hazards-that-are-fair)
- Bir şeyin tehlikeli olup olmadığını oyuncu dokunup denemeden anlayabilmeli. Tutarlı görsel dil, bir şeyin gerçekten tehlikeli mi diye test etme ihtiyacını ortadan kaldırıyor. ("Hard vs unfair" üzerine bir geliştirici blogu, arama özetinden.) — [Medium](https://medium.com/p/c3d04f642c9a)
- Aynı renk/şekil her zaman aynı işlev demek. Etkileşimli nesneler öne çıkmalı, dekor etkileşimli gibi görünmemeli. (İkincil kaynak, kontrol listesi.) — [Game Design Cards](https://game-design-cards.pages.dev/)
- Cuphead hızlı ve dinamik bir oyun. En öncelikli öğelerinin ortamdan görsel ipuçlarıyla ayrışması gerekiyor (makaleye 403 nedeniyle erişilemedi, arama özetinden). — [Super Jump: Creating Clarity in Cuphead](https://medium.com/super-jump/creating-clarity-in-cuphead-b938eabb89f3)
- Üçgen biçimler tehlikeli algılanıyor (blog iddiası, kaynak verilmemiş, zayıf). — [arama özeti, gbstudiocentral](https://gbstudiocentral.com/?p=2094)
- SMB 3D: ayrıntı arttıkça okunabilirlik düşüyor, küçük kahraman ve kalabalık ortamda daha da fazla. — [Epic Games Store](https://store.epicgames.com/en-US/news/how-super-meat-boy-3d-keeps-its-gory-identity)
- DKC Returns silüet bölümlerinde arka plan gibi görünen düşmanlar, okunabilirliği bilerek kırıyor. — [Siliconera](https://www.siliconera.com/2010/11/21/donkey-kong-country-returns-playtest-a-barrel-blast-from-the-past/)

### Inferences
- "Hard but fair" için oyunumuza uygun ilke sıralaması: (1) **renk kanalı sabit**: asit yeşili sadece ölüm demek, başka hiçbir yerde kullanılmamalı; (2) **siluet**: düz beyaz dikdörtgen bunu taşımıyor, kenarı "canlı" (dalgalı, damlalı, dişli) bir biçim gerekiyor; (3) **hareket**: sıvının kendi iç hareketi (kaynama, akış) hareketsizken bile "aktif" der; (4) **kontrast**: gökyüzü mavisi (#71BAEA) ve turuncu zemin üzerinde mürekkep (#140C1C) kontur, asit yeşilini her iki arka plandan da ayırır; (5) **ses**: konumsal uğultu.
- Death's Gambit'teki "büyük görünen şeyden ölmek adil" ilkesi, asit kütlesinin hitbox'ı kadar büyük, hatta biraz daha büyük görünmesi gerektiğini söylüyor. Asla hitbox'tan küçük görünmemeli. Platformer geleneğinde hitbox görselden biraz küçük tutulur; bu oyuncu lehine bir sapma.
- Bizim paletimizde pembe = oyuncu, kırmızı = launch pad (etkileşim), gökkuşağı halkalı parlak düğüm = grapple node. Tehlikenin core'u bu üç kimlikten **hiçbirine** benzememeli. Yoksa "expectations" bozulur.

### Gaps
- Celeste (Thorson/Berry), Super Meat Boy (McMillen/Refenes) veya Rayman (Ubisoft Montpellier) ekiplerinden hazard okunabilirliğine özel GDC konuşması bulunamadı. GDC Vault'ta elle aranması önerilir.
- Mark Brown / GMTK'nin hazard telegraphing'e özel bir videosu bulunamadı (yalnızca Rayman Legends ve Mario Maker analizlerinin altyazıları çıktı).
- Sesle tehlike bildirme üzerine kaynaklı bulgu yok. Yalnızca bir itch.io yorumunda platformların müzik vuruşuyla belirip kaybolarak telegraph edildiği geçiyor. — [itch.io yorumu](https://itch.io/post/14414505) (anekdot)

## 3. Dönen / yörüngedeki tehlikenin yolu ve zamanlaması nasıl gösteriliyor?

### Takeaway
Doğrulanan örneklerde en yaygın yöntem **görünür pivot**: Orbinaut'un gövdesi, toplarının döndüğü merkezi gösteriyor. Mario Galaxy ise yön ve alanı ok ve renk kodlarıyla anlatıyor, renk değişimini müzikle senkronluyor. Ray / iz çizgisi kullanan 2D örnekler için bu oturumda kaynak bulunamadı. Fikir genel türe ait ama burada doğrulanmadı.

### Cited Findings
- Orbinaut: merkez gövde + etrafında dönen toplar. Merkez, yörüngenin pivotunu doğal olarak gösteriyor. Toplar hem silah hem kalkan. — [Sonic Wiki: Orbinaut (Uni Uni)](https://sonic.fandom.com/wiki/Orbinaut_(Uni-Uni))
- Unidasu: davranış değişikliği (dönmeyi bırakıp fırlatma) oyuncuyu fark etme anına bağlı. Toplar önce yerinde donuyor, sonra fırlatılıyor; donma kısa bir telegraph işlevi görüyor. — [BreezeWiki: Unidasu](https://breezewiki.discard.no/sonic/wiki/Unidasu)
- Mario Galaxy: yerçekimi alanının yönü kimi zaman yakındaki oklarla gösteriliyor. Flipsville Galaxy'de arka plandaki oklar 2D bir yerçekimi alanını ima ediyor. — [MarioWiki: Gravity field](https://www.mariowiki.com/Gravity_field)
- Mario Galaxy: yöne göre renk kodlaması var. Battle Belt Galaxy'deki dairesel alanda mavi içe, kırmızı dışa çekişi gösteriyor ve renk müzikle birlikte değişiyor (wiki tablosu, topluluk kaynağı). — [MarioWiki: Gravity field](https://www.mariowiki.com/Gravity_field)
- Mario Galaxy: yerçekimi yönü gezegenciğin biçiminden okunuyor, ama asıl çekişi görünmez oyun nesneleri yönetiyor. Görünen biçim kuralı anlatıyor, kuralın kendisi ayrı bir sistem. — [MarioWiki: Gravity](https://mariowiki.com/Gravity)
- Mario Galaxy: "gravity spotlight", kendi çekişi olan bir ışık huzmesi; yüzeye vuran ışık, alanın sınırını gösteriyor. — [MarioWiki: Gravity spotlight](https://mariowiki.com/Gravity_spotlight)
- Mario Galaxy 2, Toy Time Galaxy: pembe, altıgen dokulu yüzey çekiyor (Mecha-Bowser üzerinde yeşil). Alanın dokusu davranışını anlatıyor. — [MarioWiki: Gravity field](https://www.mariowiki.com/Gravity_field)
- Excursion Funnel'da ileri ve geri akış için ayrı görsel durumlar var (galeri). Yön değişimi görselde okunuyor. — [Portal Wiki: Excursion Funnel](https://theportalwiki.com/wiki/Excursion_Funnel)

### Inferences
- **Pivot görünür olmalı.** Dairesel ve dönen tehlikelerimizde merkez ya da çapa (anchor) noktası, sabit bir mekanik "montaj" olarak çizilirse oyuncu yarıçapı ve dönüş merkezini tek bakışta okur (Orbinaut ve Fire Bar mantığı). Çapa ile tehlike arasında ince bir "çekim ışını" ya da kol, Fire Bar'ın gövdesi gibi yarıçapı da gösterir.
- **Lineer ping-pong** için uç durakları (iki küçük mekanik "tampon" veya ray ucu) ve aralarında arka planda düşük kontrastlı noktalı bir ray, nesnenin nereye kadar gideceğini anlatır.
- **Hızlanan ping-pong** için noktaların aralığı hızla orantılı olabilir (yolun hızlı kısmında seyrek, yavaş uçlarda sık). Bu "nerede hızlanıyor" bilgisini statik olarak verir.
- **Zamanlama:** Mario Galaxy'nin renk değişimini müzikle senkronlaması gibi, core üzerindeki bir ışık dönüş noktasına yaklaşırken yanıp sönebilir. Unidasu'nun "donma"sı gibi kısa bir duraksama da yön değişimini telegraph eder (ama oynanış zamanlamasını değiştirir; bu karar insanın).
- Yol çizgileri hiçbir zaman asit yeşili olmamalı. Yoksa "çizginin kendisi ölümcül mü?" sorusu doğar.

### Gaps
- Celeste'teki iz / track çizgili hareketli bloklar, N++ drone yolları, Rayman'daki dönen tehlikeler için yol gösterimini belgeleyen kaynak toplanamadı.

## 4. "Alanda tutulan sıvı" / "yerçekimiyle tutulan tehlike" görselleri kullanan oyunlar var mı? Ne işe yaradı?

### Takeaway
Doğrulanan en yakın örnek Portal 2: mavi, silindirik, saydam bir huzme jeli ağırlıksız taşıyor. Alan **ayrı renkte bir taşıyıcı** (mavi ışık), madde ise **kendi renginde** (jel) kalıyor. Mario Galaxy ise alanın varlığını ok, renk ve doku ile anlatıyor. Ölümcül sıvıyı bir cihazla havada tutan doğrudan bir 2D platformer örneği **bulunamadı**. Bu, Rock The Game için özgün bir kimlik fırsatı da olabilir.

### Cited Findings
- Excursion Funnel: alan = mavi silindirik ışık; içindeki nesneler ağırlıksız ve akışla hareket ediyor; jeller duvara çarpana ya da alan kapanana kadar sürükleniyor. — [Portal Wiki: Excursion Funnel](https://theportalwiki.com/wiki/Excursion_Funnel)
- Portal 2 jelleri yüzeylere işlevlerini renkleriyle birlikte taşıyor; jelin rengi işlevini anlatıyor. — [Valve Developer Community: Gel (Portal 2)](https://developer.valvesoftware.com/wiki/Gel_(Portal_2))
- Mario Galaxy'de alan görünmez nesnelerle yönetiliyor, oyuncuya ok, renk (mavi/kırmızı, mavi/turuncu) ve özel dokularla (pembe altıgen) gösteriliyor. — [MarioWiki: Gravity field](https://www.mariowiki.com/Gravity_field)
- Hollow Knight'ta asit, oyunun hikâyesinde bir kaynağa (Isma's Grove) bağlanmış bir dünya öğesi. Tehlike lore'la gerekçelendirilmiş. — [Hollow Knight Wiki: Royal Waterways](https://hollowknight.wiki/w/Royal_Waterways)

### Inferences
- Portal'ın dersi: **taşıyıcı alanı ve tehlikeli maddeyi farklı renk kanallarına ayır.** Bizde alan = teal (#289AA4) veya mavi (#597DCE) ince kontur halkaları, madde = asit yeşili. Böylece "yeşil kısım öldürür, mavi çizgiler sadece cihazın alanı" ayrımı kurulur.
- Mario Galaxy'nin altıgen dokusu, Bauhaus/brütalist yönümüzle ve halftone baskı görünümüyle uyumlu bir "alan" dokusuna ilham verebilir: core etrafında halftone nokta yoğunluğu dışa doğru azalan bir hale.
- (Genel bilgi, doğrulanmadı.) Control'deki havada asılı nesneler ve Gravity Rush'taki havada süzülen moloz, "yerçekimi bozulması = nesneler yavaş salınır, hafifçe döner" görsel dilini yerleştirdi. Asidin yüzeyinde yavaş, nefes alan bir salınım ve havada asılı küçük damlacıklar bu dili 2D'ye taşır.
- Hollow Knight'ın yaptığı gibi asidin lore'da bir kaynağı olması (borulardan, yani koyu mor (dark plum) boru ağından gelen asit) cihazı dünyaya bağlar. Mevcut borularımız ve asit şeritlerimiz zaten var.

### Gaps
- Ölümcül sıvıyı mekanik bir cihazla havada asılı tutan bir 2D/2.5D platformer örneği bulunamadı. Aranabilecek adaylar (doğrulanmadı): Rain World, Ori and the Will of the Wisps (su/zehirli su), Hollow Knight: Silksong, Pizza Tower, Splatoon'daki havada duran mürekkep dalgaları.
- Prey'in GLOO'su ve Control için kaynaklı sayfa çekilemedi.

## 5. Güvenli "core" ile ölümcül "kabuk" görsel olarak nasıl ayrılır? Core da ölümcül mü olmalı?

### Takeaway
Orbinaut tam olarak bu kalıbı kullanıyor: merkez gövde + etrafında ölümcül bir halka (dikenli toplar). Halka hem "silah hem kalkan", yani merkeze ancak halkadan geçerek ulaşılıyor. Bizim tasarımda asit core'u **tamamen sarıyorsa** core'un ölümcül olup olmadığı oynanışta pratikte önemsiz. Asıl karar şu: **hitbox = asidin dış silueti**, core ise salt görsel ve "cihaz" anlatımı. Asıl risk core'un ölümcül olup olmadığı değil, core'un grapple node'a benzemesi.

### Cited Findings
- Orbinaut: dönen dikenli toplar merkez gövdeyi koruyan hem silah hem kalkan. Gövdeye erişmek için ölümcül halkayı aşmak gerekiyor. — [Sonic Wiki: Orbinaut (Uni Uni)](https://sonic.fandom.com/wiki/Orbinaut_(Uni-Uni)); [Sonic Wiki: Zombie Orbinaut](https://sonic.fandom.com/wiki/Zombie_Orbinaut)
- Hollow Knight: aynı asit, bir yetenekten sonra güvenli hale geliyor. Bir maddenin anlamı oyun kuralıyla değişebiliyor, ama değişim açıkça ve bir ödülle öğretiliyor. — [Hollow Knight Wiki: Isma's Tear](https://hollowknight.wiki/w/Isma's_Tear)
- Görsel dil tutarlı olmalı ve sonuçla örtüşmeli (expectations). — [Game Developer: Readability in ARPGs](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs)
- Dekoratif nesne etkileşimli gibi görünmemeli; aynı şekil ve renk aynı işlevi taşımalı. — [Game Design Cards](https://game-design-cards.pages.dev/)

### Inferences
**Tasarım seçenekleri:**
- **A) Tüm cihaz ölümcül (önerilen varsayılan).** Kural en basit hali: "cihaza değersen ölürsün." Asit core'u tamamen sarıyorsa zaten farkı yok. Hitbox tek bir şekil (daire/kapsül) olarak kalır, kod değişikliği minimum.
- **B) Core güvenli, sadece asit ölümcül.** Ancak core açıkta kalan bir parça varsa anlamlı (ör. dikdörtgende çubuğun asitle sarılmayan uç kapakları ya da "C" biçiminde açık bir asit halkası). Bu durumda core'un üstüne basılabilir ya da çarpılabilir olması ilginç bir mekanik açar ama okunabilirlik riski yüksek: oyuncu hızda "metal kısım güvenli" ile "yeşil kısım ölümcül"ü saniyeler içinde ayırt etmek zorunda kalır. Expectations ilkesine göre bu kural erken ve tek başına öğretilmeli.
- **C) Grapple'lanabilir core.** Grapple oyunu için cazip (asidin içinden kancayla çekilip geçme riski/ödülü) ama gökkuşağı halkalı grapple node'larıyla karışma tehlikesi çok yüksek. GDD ile uyum kontrol edilmeden önerilmemeli; insanın kararı.
- **Görsel ayrım yöntemleri (A ve B için):** core'u **mat, koyu, halftone gölgeli metal** (ink #140C1C + paper #F1E6C7 vurgular + sarı #FCAE04 uyarı şeritleri) yapmak; asidi **parlak, düz, kenarı hareketli** göstermek. Malzeme farkı (mat-katı vs. parlak-sıvı) renk farkından bağımsız ikinci bir kanal sağlar; renk körü oyuncular için de işe yarar.
- **Kaçınılacak kimlikler:** core'da gökkuşağı halka ya da parıltı (grapple node), kırmızı #FC1E00 dolgu (launch pad), oyuncu pembesi #FF51DE.

### Gaps
- Core ve shell ayrımını açıkça tartışan bir geliştirici kaynağı bulunamadı. Yukarıdaki seçenekler çıkarım.

## 6. Asit-yerçekimi tehlikelerimize aktarılabilir fikirler (Türkçe özet)

### Takeaway
Önerilen çekirdek formül: **parlak asit yeşili, dalgalanan sıvı kabuk + koyu mürekkep konturu + mat, Toriyama tarzı vidalı/panelli mekanik core + core'dan asidi "tutan" kısa emitter çıkıntıları + teal renkte ince alan halkaları + arka planda düşük kontrastlı yol/ray çizgisi.** Hitbox her zaman asidin dış siluetiyle aynı ya da ondan biraz küçük. Hissiyat ve renk yoğunluğu insanın kararı (kural 4); aşağıdaki maliyetler tahmindir.

### Cited Findings
- Tutarlı görsel dil + telegraphing + expectations çerçevesi. — [Game Developer: Readability in ARPGs](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs)
- Merkez gövde + ölümcül yörünge halkası kalıbı (Orbinaut). — [Sonic Wiki](https://sonic.fandom.com/wiki/Orbinaut_(Uni-Uni))
- Taşıyıcı alan ayrı renkte, içindeki madde kendi renginde (Excursion Funnel + jel). — [Portal Wiki](https://theportalwiki.com/wiki/Excursion_Funnel)
- Alanı ok/renk/doku ile gösterme, renk değişimini müzikle senkronlama (Mario Galaxy). — [MarioWiki](https://www.mariowiki.com/Gravity_field)
- Fazla ayrıntı hızda okunabilirliği düşürür (SMB 3D). — [Epic Games Store](https://store.epicgames.com/en-US/news/how-super-meat-boy-3d-keeps-its-gory-identity)

### Inferences
Her fikir için: **Daire** / **Dikdörtgen** / **Okunabilirlik riski** / **2D uygulama maliyeti** (URP 2D, tek geliştirici).

1. **Tek asit rengi = ölüm (zemindeki asit şeritleriyle aynı yeşil ve mümkünse aynı shader).**
   - Daire: core'u saran tam asit halkası/diski. Dikdörtgen: core çubuğunu saran asit kapsülü.
   - Risk: düşük. Yeşil başka hiçbir dost öğede (UI dahil) kullanılırsa kural kırılır.
   - Maliyet: çok düşük (mevcut materyal/renk).

2. **"Canlı" sıvı kenarı: dalgalanan veya kaynayan siluet (SDF shader'da yarıçapa sinüs ya da noise).**
   - Daire: kenar radyal olarak dalgalanır, dönen tehlikelerde dalga dönüş yönünde akar. Dikdörtgen: uzun kenarlarda yürüyen dalga, uçlarda yuvarlak menisküs.
   - Risk: orta. Dalga genliği hitbox'ı aşarsa oyuncu "değmedim ama öldüm" der. Genliği hitbox'ın **içine** doğru tut (görsel ≥ hitbox kuralı).
   - Maliyet: düşük-orta (tek fragment shader, SDF daire/kapsül; Shader Graph ile de yapılabilir).

3. **Koyu mürekkep konturu (#140C1C) + iç parlak kenar ışığı.** Hem gökyüzü mavisinde hem turuncu zeminde siluet patlar.
   - Daire/dikdörtgen: aynı, kalın dış çizgi + asidin içinde açık yeşil bir "rim".
   - Risk: düşük. Kontur kalınlığı platformlarla aynı olursa tehlike platform gibi okunabilir; tehlikeye ayrı (daha kalın veya titreyen) kontur ver.
   - Maliyet: düşük (shader'da SDF'den kontur).

4. **Mekanik core: Toriyama tarzı yuvarlatılmış gövde, vidalar, panel çizgileri, küçük gösterge ışığı, sarı-siyah uyarı şeridi; mat ve halftone gölgeli.**
   - Daire: ortada küçük yuvarlak hub, asit halkasına doğru uzanan 3-4 kısa "emitter dişi" (asidi tutan çatal hissi). Dikdörtgen: iki ucunda kalın kapak (emitter) olan bir çubuk, aradaki asit kapakların arasında gergin tutulan bir "tüp" gibi; asit kapaklarda biraz geri çekilir, sanki tutuluyormuş gibi.
   - Risk: **yüksek**. Yuvarlak core + halka = grapple node siluetine çok yakın. Gökkuşağı halka ve parıltı kullanma, core'u koyu tut, node'lar parlak kalsın. Ayrıca çok ayrıntılı core hızda gürültü olur (SMB 3D dersi); 2-3 büyük biçim yeter.
   - Maliyet: orta (tek sprite çizimi + halftone materyal; mevcut halftone platform görünümü yeniden kullanılabilir).

5. **Yerçekimi alanı göstergesi: core'dan dışa doğru ince teal (#289AA4) ya da mavi (#597DCE) eş merkezli halkalar, yavaşça dışa "nefes alır".** Mario Galaxy'nin renk/doku ile alan anlatımı ve Portal'ın mavi taşıyıcı huzmesinden.
   - Daire: asit diskinin içinde ya da hemen dış kenarında çok ince 2-3 kontur halkası. Dikdörtgen: kapsül biçimli eş merkezli konturlar veya kapaklar arasında uzanan alan çizgileri.
   - Risk: orta-yüksek. Halkalar asidin **dışına** taşarsa oyuncu tehlikeyi olduğundan büyük sanar (aşırı temkin; adil ama sinir bozucu). Halkalar asidin içinde ya da çok soluk kalmalı.
   - Maliyet: düşük (aynı shader'a ek halka fonksiyonu).

6. **Yol ve çapa telegraph'ı (arka plan katmanı, düşük kontrastlı ink/paper, asla yeşil değil).**
   - Daire (circular path): pivotta sabit bir mekanik "montaj" + pivottan tehlikeye uzanan ince bir çekim ışını veya kol (Fire Bar mantığı) + kesikli yörünge çemberi. Dönen (rotating) dikdörtgen: aynı pivot montajı. Lineer / hızlanan ping-pong: uçlarda tampon + noktalı ray; hızlananlarda nokta aralığı hızla orantılı.
   - Risk: düşük-orta. Fazla çizgi sahneyi kalabalıklaştırır; çok sayıda tehlike olan odalarda çizgileri yalnızca pivot ve uçlarla sınırla.
   - Maliyet: düşük (LineRenderer veya tek sprite; strateji sınıflarındaki yol verisinden editörde otomatik üretilebilir).

7. **Hız ipuçları: hız yönünde hafif uzama (squash/stretch) ve arkada kısa ömürlü damlacık izi.**
   - Daire: hızlandıkça hız yönünde elipsleşir, damlalar teğet doğrultuda kopar. Dikdörtgen: hareket yönündeki uçta sıvı geride kalır (sürüklenme).
   - Risk: orta. İz ve damlalar tam yeşil ve opak olursa "iz de öldürür mü?" sorusu doğar. İz soluk, küçük ve hitbox'sız olmalı ya da çok kısa yaşamalı.
   - Maliyet: düşük-orta (ParticleSystem + shader'a velocity parametresi).

8. **Yön değişimi telegraph'ı: core ışığı ping-pong dönüş noktasına yaklaşırken hızlanarak yanıp söner.** Hızlanan stratejide ivmeyle senkron.
   - Daire/dikdörtgen: core göstergesi aynı.
   - Risk: düşük. Işık rengi kırmızı olursa launch pad'le karışabilir; sarı (#FCAE04) öner.
   - Maliyet: düşük (strateji fazından shader parametresi).

9. **Ses: konumsal uğultu, perdesi hıza bağlı; asit "fokurtu" katmanı.**
   - Her iki varyant için aynı. Risk: çok tehlikeli odada ses kirliliği; mesafe sönümü kısa tutulmalı.
   - Maliyet: düşük (mevcut AudioCueSO / event-channel deseni).

10. **Ölüm geri bildirimi: temasta asit oyuncunun üzerine sıçrar (kısa splash + hitstop).** Oyuncunun "beni asit öldürdü" diye öğrenmesini pekiştirir (expectations).
    - Risk: düşük. Maliyet: düşük (mevcut HitStopEvents + bir partikül).

11. **Halftone ile uyum: asit gövdesi düz ve doygun kalır, halftone yalnızca gölge veya alan halesinde kullanılır.**
    - Risk: halftone asidin üzerine binerse kontrast düşer, asit platform dokusuna benzer. Asidi düz renk bırakmak "sıvı ≠ katı platform" ayrımını korur.
    - Maliyet: düşük (mevcut halftone shader'ı core'a uygula).

12. **Absolute Martian Manhunter etkisi (düz, doygun, gerçeküstü renk bloklar): asit yeşili ile oyuncu pembesi zaten tamamlayıcı renkler; tehlike, oyuncuya karşı maksimum kontrast verir.**
    - Risk: yeşil ve pembe yan yana hızda "titreşim" yaratabilir; bu bilinçli çatışma estetiğine uyar, ama oyuncunun kendi siluetini kaybettirmemeli.
    - Maliyet: sıfır (palet kararı).

**Core ölümcül mü? Öneri:** Varsayılan **A (tüm cihaz ölümcül, hitbox = asidin dış silueti)**. Asit core'u tamamen sardığı sürece oyuncu core'a zaten dokunamaz; ayrım yalnızca görsel "cihaz" hikâyesi için var. Core'un açıkta kaldığı varyantlar (C biçimli açık halka, uçları açık çubuk) yeni bir mekanik olur. GDD kontrolü ve insan kararı gerektirir.

### Gaps
- Bu fikirlerin hiçbiri oyun içinde test edilmedi. Hız ve oda yoğunluğu altında okunabilirlik ancak gamepad ile oynayarak doğrulanabilir (kural 4).
- "Dalgalanan kenar" ve "alan halkası" için hitbox/görsel tolerans payı ölçülmedi. Mevcut tehlike collider boyutları okunmadı.
- Akira Toriyama mekanik çizimleri ve Javier Rodríguez'in Absolute Martian Manhunter renk dili için görsel kaynak taraması yapılmadı (kapsam dışı / bütçe).
