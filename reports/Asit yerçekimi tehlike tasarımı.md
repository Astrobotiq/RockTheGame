# Sevimli makine asidi tutsun, siluet öldürsün

Beyaz dikdörtgen ve daireleri "yerçekimi cihazının havada tuttuğu asit"e çevirmenin en sağlam yolu şu: **küçük, koyu, Toriyama tarzı tombul bir mekanik çekirdek; onu saran parlak asit yeşili, kenarı canlı (dalgalı ya da dikenli) bir sıvı kütlesi; ve görseli asla çarpıştırıcıdan küçük göstermeyen tek bir ortak SDF shader'ı.** Altı araştırma notunun hepsi aynı çekirdek kurala varıyor. Asit yeşili oyunda zaten zemindeki şeritlerle "ölüm" demek, bu yüzden hareketli tehlikeye geçen her şey bu tek renk kanalını korumalı ve grapple node'larının dilinden (gökkuşağı halka, dış parıltı, temiz eş merkezli halkalar) uzak durmalı. Merkez gövde ve etrafında ölümcül halka kalıbı oyunlarda kanıtlanmış (Sonic'teki Orbinaut). Taşıyıcı alanla tehlikeli maddeyi ayrı renk kanallarına ayırmanın da örneği var (Portal 2'deki Excursion Funnel ve jel). Ama ölümcül bir sıvıyı mekanik bir cihazla havada tutan bir 2D platformer örneği hiçbir notta **bulunamadı**. Bu boşluk Rock The Game'e özgün bir görsel kimlik fırsatı veriyor. Aşağıdaki 12 aday konsept, okunurluğu en güvenli "Kapsül No.7"den sürreal "Ters Damla"ya kadar uzanıyor. Hepsi tek kişinin Shader Graph ile yapabileceği ölçekte. Ancak notların üç noktada ciddi biçimde ayrıştığını baştan söylemek gerekiyor: mürekkep kontur mu ışıyan kenar mı, parıltı olsun mu, çekirdek saf beyaz mı koyu mu. Bu üç karar jüriye kalıyor. Okunurluk iddialarının hiçbiri oyun içinde test edilmedi. Son söz gamepad ile yapılacak playtest'te.

## Oyuncu "yeşil = ölüm" kuralını zaten öğrendi, yeni tasarım onu taşımalı

Tasarımın dayanağı oyun okunurluğu literatüründen geliyor. Death's Gambit tasarımcısının GDC konuşmasından türeyen yazı, okunurluğu **telegraphing** (öncesi: tehlikenin geldiğini görmek) ve **expectations** (sonrası: görüntüyle sonucun tutarlı olması) diye ikiye ayırıyor. Yazıya göre büyük görünen bir şeyden ölmek, küçük görünen bir vuruştan ölmekten daha adil hissettiriyor ([Game Developer](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs)). Çevre tehlikeleri ancak beceriyle görülüp kaçınılabiliyorsa adil ([Bugnet](https://bugnet.io/blog/how-to-design-environmental-hazards-that-are-fair)). Aynı renk ya da şekil her zaman aynı işlevi taşımalı ([Game Design Cards](https://game-design-cards.pages.dev/)). Rock The Game için bunun anlamı açık: yeni asit tehlikesi, oyuncunun zemindeki asit şeritlerinden öğrendiği kuralı otomatik olarak devralmalı. Hollow Knight da asidi lore'a bağlıyor. Isma's Grove aşağı Hallownest'teki asidin kaynağı ([Hollow Knight Wiki](https://hollowknight.wiki/w/Royal_Waterways)) ve aynı sıvı Isma's Tear ile güvenliye dönüşüyor ([Hollow Knight Wiki](https://hollowknight.wiki/w/Isma's_Tear)). Asidin anlamını değiştirmek mümkün, ama ancak açıkça ve bir ödülle öğretilirse.

Renk tek başına yetmiyor. **Renk tonu şekilden zayıf bir sinyal.** Okunurluk hatalarının çoğu değer (açıklık) hatası, bu yüzden gri tonlamada kontrol etmek gerekiyor ([Bugnet: Readable game art](https://bugnet.io/blog/readable-game-art-why-clarity-beats-detail); [Game Developer](https://gamedeveloper.com/design/how-to-reduce-visual-confusion-in-your-game)). Sivri şekiller tehlike, yuvarlak şekiller yumuşaklık çağrıştırıyor ([AIE UX dersi](https://vanphelan.github.io/AIE-Lecture-UX.pdf); [UXPin](https://www.uxpin.com/studio/blog/how-to-use-visual-language-for-intuitive-level-design/)). Fazla ayrıntı hızda okunurluğu düşürüyor. Sluggerfly, Super Meat Boy 3D'de en büyük zorluğun küçük kahraman ve kalabalık ortamda okunurluğu korumak olduğunu söylüyor ([Epic Games Store](https://store.epicgames.com/en-US/news/how-super-meat-boy-3d-keeps-its-gory-identity)). Bir indie geliştirici kırmızı dikenlerin kahverengi/turuncu mağarada kaybolduğunu görmüş. Çözümü koyu gri bir gövde ve üstünde tehlike renginde çatlaklar olmuş ([Lost Caves devlog](https://adscomics.itch.io/lost-caves/devlog/173099/lost-caves-devlog-10-subtle-indicators)). Turuncu zeminli bu oyunda aynı ders birebir geçerli. Asit yeşilinin gökyüzü mavisi #71BAEA ve turuncu zemin karşısındaki değer kontrastı ölçülmedi (asidin tam hex değeri notlarda yok). Bu, motorda gri tonlamalı bir ekran görüntüsüyle kontrol edilmeli.

Çekirdek-halka fikrinin oyunda kanıtlanmış karşılığı Orbinaut. Merkezdeki küresel robotun etrafında dönen dikenli toplar hem silah hem kalkan ([Sonic Wiki](https://sonic.fandom.com/wiki/Orbinaut_%28Uni-Uni%29); [Zombie Orbinaut](https://sonic.fandom.com/wiki/Zombie_Orbinaut)). Merkez gövde, yörüngenin pivotunu kendiliğinden gösteriyor. Unidasu varyantında toplar fırlatılmadan önce yerinde donuyor ve bu donma kısa bir telegraf işi görüyor ([BreezeWiki](https://breezewiki.discard.no/sonic/wiki/Unidasu)). "Alanda asılı sıvı"nın en açık örneği Portal 2. Excursion Funnel'ın mavi silindirik huzmesi içine gireni ağırlıksız taşıyor, jeller duvara çarpana kadar akışla sürükleniyor ([Portal Wiki](https://theportalwiki.com/wiki/Excursion_Funnel)). Jelin rengi de işlevini anlatıyor ([Valve Developer Community](https://developer.valvesoftware.com/wiki/Gel_%28Portal_2%29)). Buradan çıkan ders: **taşıyıcı alan bir renk kanalında (teal/mavi ince çizgiler), ölümcül madde başka bir kanalda (asit yeşili).** Mario Galaxy ise görünmez yerçekimi alanlarını ok, renk ve dokuyla anlatıyor. Battle Belt Galaxy'de mavi içe, kırmızı dışa çekişi gösteriyor ve renk müzikle birlikte değişiyor ([MarioWiki](https://www.mariowiki.com/Gravity_field)).

## Yedi ilham ailesi aynı şeyi söylüyor: sıvı cihaza tepki vermeli

Notların en değerli sentezi bilimkurgu ve gerçek bilim tarafından geliyor. Bir cihazın bir şeyi "tuttuğu" dört ipucuyla okunuyor: **görünür bir kaynak** (kapak, bobin, çekirdek), **boşlukta görünür bir yapı** (halka, düğüm, basınç bandı), **sıvının alana tepkisi** ve **durum ışığı**. Çıplak bir sıvı damlası "tutulmuş" okunmuyor. Sıvı cihaza tepki vermeli. Gerçek bilim bunun en güçlü kanıtlarını veriyor. Sachiko Kodama'nın ferrosıvı heykellerinde dikenler tabanda oluşuyor, demir spiralin kenarında tırmanıp titreyerek dönüyor, boyları elektromıknatısın akımıyla büyüyüp küçülüyor ([CACM](https://cacm.acm.org/magazines/2008/6/5384-dynamic-ferrofluid-sculpture/fulltext); [Digital Art Archive](https://digitalartarchive.at/database/work/2230)). Rosensweig kararsızlığında kritik alan aşılınca düz ferrosıvı yüzey **altıgen diken dizisine** sıçrıyor ([Europhysics News](https://www.europhysicsnews.org/articles/epn/pdf/2011/03/epn2011423p17.pdf)). Küçük bir mıknatıs yaklaştıkça dikenler çoğalıyor ve sonunda ayrı damlalar halinde kopuyor ([arXiv 1711.09942](https://arxiv.org/pdf/1711.09942)). Bu, gerçek dünyadan hazır bir "aşırı yük" telegrafı. Akustik levitasyonda damlalar bir transdüser ile reflektör arasındaki duran dalganın düğümlerine **eşit aralıklarla diziliyor** ve alanın içinde **basılıyor** ([PMC9500997](https://pmc.ncbi.nlm.nih.gov/articles/PMC9500997); [arXiv 2307.00400](https://arxiv.org/pdf/2307.00400)). ISS'deki su küreleri yüzey gerilimiyle bir arada duruyor ve hava üflendiğinde hem gövdenin içinden hem yüzeyinden dalgalanıyor ([NASA](https://www.nasa.gov/news-release/dancing-droplets-rock-out-on-space-station/); [Boing Boing](https://boingboing.net/2009/11/28/saturday-morning-sci-7.html)). Don Pettit'in efervesan tabletli küresi büyüyor ve fışkıran küçük kürecikler saçıyor ([SlashGear](https://www.slashgear.com/watch-an-effervescent-tablet-fizz-inside-a-ball-of-water-in-space-28394568)). Gıda boyasıyla yaptığı "Jüpiter" küresinde renkler bantlar halinde girdap yapıyor ([Space.com](https://www.space.com/nasa-astronaut-don-pettit-jupiter-water-ball-iss)).

Bilimkurgu tasarımı bu davranışlara biçim veriyor. Event Horizon'un yerçekimi sürücüsü, merkezdeki bir kürenin etrafında dönen üç halka. Halkalar hizalanınca bir geçit açılıyor ([Wikipedia](https://en.Wikipedia.com/wiki/Event_Horizon_%28film%29); [RetroZap](https://retrozap.com/event-horizon-1997-sci-fi-saturdays/)). Half-Life 2'nin Combine alan köprülerinin iki ucunda emitör ışıkları var. Arayüz, alan kapanınca maviden kırmızıya yanıp sönüyor ([Combine OverWiki: Powered bridge](https://combineoverwiki.net/wiki/Powered_bridge); [Combine Interface](https://combineoverwiki.net/wiki/Combine_Interface)). Alanlar görünür Enerji Topları'yla besleniyor ve küre çıkarılınca alan kapanıyor ([Combine OverWiki: Energy Ball](https://combineoverwiki.net/wiki/Energy_Ball)). Oyunlardaki kalkan VFX geleneği fresnel kenar parlaması, kayan UV ve altıgen hücre kullanıyor ([godotshaders](https://godotshaders.com/shader/scifi-shield/); [Construct 3 Force Field](https://www.construct.net/en/make-games/addons/1591/force-field-effect/documentation)). Ama notların ortak uyarısı şu: bu parıltı silüetin dışına taşarsa tehlike çarpıştırıcısından büyük görünür ve haksızlaşır.

Akira Toriyama çekirdeğin karakterini veriyor. Toriyama makineleri gerçek araçların **yumurta/kapsül biçimine deforme edilmiş** hali ([Zimmerit](https://www.zimmerit.moe/akira-toriyama-mechanical-designs/)). Dışta büyük, yumuşak, sade biçimler karmaşık kablo ve boruları örtüyor. Araçlar içindekilere göre hep biraz komik derecede küçük ([Gizmodo](https://gizmodo.com/akira-toriyama-mechanical-design-dragon-ball-sand-land-1851320681)). Toriyama'nın kendi sözleri en önemli kuralı veriyor: birebir çizmek "absürt derecede uzun sürüyor", deforme edince küçük hatalar göze batmıyor. Özgün makineleri "nasıl binildiğini ve motorun nerede olduğunu" düşünerek tasarlıyor ve dövüşler yoğunlaştıkça daha sade çizgilere geçiyor ([Daizenshuu 1, Kanzenshuu](https://www.kanzenshuu.com/translations/daizenshuu-1-akira-toriyama-super-interview)). Capsule Corp araçları kapsül numarasıyla anılıyor ("Capsule No. 9 Motorcycle") ([Kanzenshuu wiki](https://www.kanzenshuu.com/wiki/Vehicles)). DBZ'deki Yerçekimi Makinesi bir enerji küresi değil, Dr. Brief'in küresel gemisine konmuş, 100x'e ayarlanabilen bir ev aleti ([Dragon Ball Wiki](https://dragonball.fandom.com/wiki/Gravity)). Bölüm 57'de manyetik fırtına cihazı bozuyor ve Goku'yu 100x yerçekimi vuruyor ([Kanzenshuu wiki](https://www.kanzenshuu.com/wiki/Dragon_Ball_Z_Episode_57)). "Asidi tutan arızalanabilir ev aleti" anlatısı buradan geliyor. Odanın fiziksel tasarımı (merkezde sütun, kırmızı ışık) **doğrulanamadı**. Bölüm karelerinden kontrol edilmeli. Referans görseller: [Mr. Ho Porsche hovercar](https://www.zimmerit.moe/wp-content/uploads/2017/01/mr_ho001-1024x961.jpg), [Tong Poo](https://www.zimmerit.moe/wp-content/uploads/2017/01/tongpoo.jpg), [Sand Land tankı](https://www.zimmerit.moe/wp-content/uploads/2017/01/sandland-1.jpg), [Lisa kiti talimatı](https://www.zimmerit.moe/wp-content/uploads/2024/09/lisa_instructions.jpg), [Capsule gemisi](https://dragonballuniverse.fandom.com/wiki/File:Goku_Capsule_Ship.png), [Jalopnik galerisi](https://www.jalopnik.com/jalopnik/images/1c9e50aef0589e36f800e5bb452edba5.jpg).

Javier Rodríguez'in Absolute Martian Manhunter'daki rengi asit için bir disiplin öneriyor. Rodríguez **iki renk rejimi** kuruyor: insan dünyası soluk, Marslı ise altı renk artı iki varyantlık kısıtlı bir paletle çiziliyor. **Saf beyaz tek bir düşmana, 000'a ayrılmış.** Kâğıt bile kirli beyaz ([AIPT](https://aiptcomics.com/2025/03/25/javier-rodriguez-absolute-martian-interview/)). Çevredeki paleti, Marslı'nın renkleri fosforlu görünecek şekilde ayarlıyor. Rengin gerçekçi ışığı değil duyguyu izlediğini söylüyor ve Magritte, Warhol, Lichtenstein ile Peellaert'i anıyor ([SKTCHD](https://sktchd.com/art-feature/javier-rodriguez-absolute-art-feature/)). Popverse, Marslı devraldıkça cetvel çizgilerinin gevşediğini, bazen tamamen kaybolup yerini renk desenlerine bıraktığını yazıyor ([Popverse](https://www.thepopverse.com/comics-absolute-martian-mahunter-dc-comics-art-javier-rodriguez/)). Despero'nun kuyruksuz balonları duvara fırlatılıp aşağı akan bir şeye benziyor ([Comic Watch #9](https://comic-watch.com/comic-book-reviews/absolute-martian-manhunter-9-the-color-of-despair)). Görseller: [#1 kapak](https://sktchd.com/wp-content/uploads/2025/04/Absolute-Martian-Manhunter-1.jpg), [künye sayfası](https://sktchd.com/wp-content/uploads/2025/09/Absolute-Martian-Manhunter-1-Credits.jpg), [iç sayfa](https://sktchd.com/wp-content/uploads/2025/09/image-3.jpeg), [son sayfalar 1](https://sktchd.com/wp-content/uploads/2025/09/Absolute-Martian-Manhunter-1-Final-Pages-1-scaled.jpg), [son sayfalar 2](https://sktchd.com/wp-content/uploads/2025/09/Absolute-Martian-Manhunter-1-Final-Pages-2.jpg), [Survivor sayfası](https://geekdad.com/wp-content/uploads/2025/03/Absolute-Martian-Manhunter-1-2.jpg), [#9 kapak](https://spcdn.shortpixel.ai/spio/ret_img+q_cdnize/comic-watch.com/wp-content/uploads/2026/02/absolute-martian-manhunter-9-1.jpg). Komşu referanslar da işe yarıyor. Kirby Krackle, yönsüz büyük enerjiyi gösteren yuvarlak nokta kümeleri ([Wikipedia](https://en.wikipedia.org/wiki/Kirby_Krackle); [Kirby Museum](https://kirbymuseum.org/blogs/simonandkirby/wp-json/wp/v2/posts/2600)). Riso baskıda katmanlar yarı saydam film gibi üst üste biniyor ve tamburlar arasında 0–3 mm kayma kaçınılmaz ([Possible Worlds](https://possibleworldsshop.squarespace.com/risograph-set-up); [Oxford Green Print](https://oxfordgreenprint.com/home/design-for-riso/)).

Wildcard notu en uzak kaynakları somut biçimlere çeviriyor. Güneş gülünün (Drosera) dokunaç uçları yaklaşık pH 5 asidik, yapışkan damlalar üretiyor ([Drosera](https://ipfs.ssi.eecc.de/ipns/en.wikipedia-on-ipfs.org/wiki/Drosera)) ve damlaların parıltısı avı çekiyor ([EPFL](https://dlab.epfl.ch/wikispeedia/wpcd/wp/s/Sundew.htm)). Satürn halkasının "spoke"ları elektrik yüklenip halkaların üstünde havada asılı kalan toz ([JPL PIA07731](https://www.jpl.nasa.gov/images/pia07731-finally-spokes/)). Tokamak, toroidal ve poloidal alanı birleştirerek plazmayı **bükülmüş** bir alanla hapsediyor ([DOE](https://www.energy.gov/node/4444676)). ISO 3864 sarı-siyahı risk noktaları için standartlaştırıyor ve uyarı sarısı RAL 1003 ≈ #F9A900, yani paletteki #FCAE04'e neredeyse eşit ([Wikipedia: ISO 3864](https://en.wikipedia.org/wiki/ISO_3864)). Biyotehlike sembolü bilerek "akılda kalıcı ama anlamsız" tasarlandı ki anlamı öğretilebilsin ([99% Invisible](https://99percentinvisible.org/article/biohazard-symbol-designed-to-be-memorable-but-meaningless/)). Moholy-Nagy'nin Light Prop'unda bir motor, delikli diskleri farklı hızlarda döndürüp duvara hareketli desenler düşürüyor ([Harvard Gazette](https://news.harvard.edu/gazette/story/2007/07/light-prop-shines-again/); [Vimeo](https://vimeo.com/111474831)). Bu ses Bauhaus UI yönüyle doğrudan akraba.

Son aile render tekniği. Elle çizilen FX animasyonunda sıvıyı saydamlık, yansıtıcılık, kabarcık ve dalgacık okutuyor. Kabarcıklar akış yönüne bakıyor ve ana kütleden uzaklaştıkça küçülüyor ([VFX Apprentice](https://www.vfxapprentice.com/blog/how-to-draw-stylized-water)). Joseph Gilland sıvıyı yoğunluk, yerçekimi ve viskozite üzerinden ele alıyor ([O'Reilly, Elemental Magic Bölüm 4](https://www.oreilly.com/library/view/elemental-magic/9780240811635/000_9781136137259_chapter4.html); [Parka Blogs](https://www.parkablogs.com/node/1783)). Akademik çalışmalara göre stilize sıvı yalnızca yüzeyi değil şekli de değiştirmeli ve hıza bağlı uzamış damlalar hareketi anlatıyor ([Eurographics 2025](https://diglib.eg.org/handle/10.2312/exw20251069)). Bütün görünüm tek bir unlit shader'da yapılabiliyor: analitik 2D SDF, polar koordinat ([Unity Polar Coordinates](https://docs.unity3d.com/Packages/com.unity.shadergraph@8.1/manual/Polar-Coordinates-Node.html)), gürültü ve eşik ([Mirza Beig](https://mirzabeig.substack.com/p/unity-tutorial-dissolve-shader-part-1)), rounded box SDF ([Infinite Canvas](https://scour.ing/p/https://infinitecanvas.cc/guide/lesson-009); [NedMakesGames](https://nedmakesgames.github.io/blog/2dsdf)). Inigo Quilez'in [2D SDF kataloğu](https://iquilezles.org/articles/distfunctions2d/) ve [smin makalesi](https://iquilezles.org/articles/smin/) kanonik referanslar, ama bu oturumda açılamadılar.

## Ortak kurallar

Notların büyük çoğunluğunun bağımsız olarak vardığı kurallar aşağıda. Bunlar hangi konsept seçilirse seçilsin geçerli.

| # | Kural | Neden / dayanak |
|---|---|---|
| O1 | **Asit yeşili yalnızca ölüm demek** ve zemindeki asit şeritleriyle aynı renk (mümkünse aynı shader). Yeşil başka hiçbir dost öğede, UI'da ya da yol çizgisinde kullanılmaz. | Beş notun hepsi; tutarlı renk kodu ([Unity blog](https://unity.com/blog/level-design-basics)) |
| O2 | **Çarpıştırıcı dürüstlüğü:** görsel hiçbir anda çarpıştırıcıdan küçük görünmez. Kenar dalgalanması yalnızca dışa doğru (`d = dBase − noise01·amp`). Çarpıştırıcı, dalgasız taban şeklin biraz içinde (önerilen başlangıç yarıçapın ~%5–10'u). Değer gamepad testine kalır. | Hitbox'ı sprite'tan küçük tutma geleneği ([itch.io](https://itch.io/post/9301743); [itch.io](https://itch.io/post/2678115)); sanat/hitbox uyumsuzluğu şikâyetleri ([GB Studio Central](https://gbstudiocentral.com/?p=2094)) |
| O3 | **Varsayılan: cihazın tamamı öldürür.** Asit çekirdeği tamamen sardığı sürece çekirdeğin güvenli olup olmaması oynanışta önemsiz. Kapak, halka gibi mekanik parçalar ya asit silüetinin içinde kalır ya da onlar da öldürür. Güvenli çekirdek ya da grapple'lanabilir çekirdek yeni bir mekanik demek: GDD kontrolü ve insan kararı gerekir. | Oyun, bilimkurgu ve VFX notları |
| O4 | **Damlalar, iz, aura ve parlamalar çarpıştırıcısız**, ana gövdeden belirgin biçimde küçük ve kısa ömürlü. | VFX, bilimkurgu ve oyun notları |
| O5 | **Grapple node dilinden kaçın:** gökkuşağı halka yok, temiz eş merkezli halka ile parıltının birlikte kullanımı yok. Oyuncu pembesi #FF51DE tehlikede yok. Kırmızı #FC1E00 yalnızca küçük bir lamba ya da noktada (büyük kırmızı alan launch pad demek). Sky #71BAEA asit konturunda yok. Ananas collectible'larıyla karışmasın diye sarı parlak noktalar da yok. | Beş not; Orbinaut kalıbının en büyük riski çekirdeğin node'a benzemesi |
| O6 | **Hiyerarşi: önce ölüm, sonra sevimlilik.** En doygun eleman asit. Çekirdek daha düşük kontrastlı nötrlerde. Çekirdek çapı (ya da çubuk kalınlığı) toplamın ~%30–40'ı. 32 px'te en fazla 3, 64 px'te en fazla 5 detay. | Toriyama ve oyun notları; SMB 3D dersi |
| O7 | **Şekil renkten önce gelir:** her tehlike renk olmadan da (dalgalı/dikenli siluet, çekirdek) tanınmalı. Gri tonlama testi zorunlu. Deuteranopide yeşil asit ile kırmızı launch pad birbirine yaklaşıyor. | [Bugnet](https://bugnet.io/blog/readable-game-art-why-clarity-beats-detail); [IGDA](https://igda.org/news-archive/color-stories-in-game-design/) |
| O8 | **Sıvı hep "yaşar":** dünya düz ve durgun bir baskı, tehlike ise kendi içinde hareket ediyor (dalgalanma, akış, dönen desen). Çekirdek yavaş ya da sabit, sıvı hızlı. Bu kontrast "cihaz sıvıyı tutuyor" der. | Bilimkurgu, AMM ve VFX notları |
| O9 | **Yol gösterimi** arka plan katmanında: ince, kesikli, düşük kontrastlı ink/paper, **asla yeşil değil**. Dairesel ve dönen yollarda pivotta sabit bir montaj ve kesikli yörünge. Lineer ping-pong'da uçlarda tampon ve noktalı ray. Hızlanan yolda nokta aralığı hızla orantılı. Kalabalık odalarda yalnızca pivot ve uçlar gösterilir. | Oyun notu; Mario Galaxy okları ([MarioWiki](https://www.mariowiki.com/Gravity_field)); bilimkurgu tractor beam ipi ([Theme Ontology](https://themeontology.org/theme/tractor-beam.html)) |
| O10 | **Telegraf siluette:** dönüş noktasından önce lamba yanıp söner, kenar kısa titrer. Hızlanınca asit hız yönünde gerilir ya da dikenler uzar. Gerilme çarpıştırıcıyı aşmamalı. Unidasu tarzı bir duraksama oynanış zamanlamasını değiştirir, bu yüzden insan kararı. | Bilimkurgu ve oyun notları |
| O11 | **Shader öncelikli, tek ortak materyal:** boyut transform scale'den, rastgelelik pozisyon hash'inden ya da `SpriteRenderer.color`'dan gelir. MaterialPropertyBlock kullanılmaz, partikül için Shuriken. | [Unity Manual](https://docs.unity3d.com/Manual/SRPBatcher-Incompatible.html); [Unity forum](https://discussions.unity.com/t/srp-batcher-spriterenderer/918871) |

### Notların ayrıştığı yerler

Notlar beş konuda birbirine açıkça karşı çıkıyor. Bunlar jürinin vereceği kararlar. Tablonun son sütunu bu raporun önerisi.

| Konu | Taraf A | Taraf B | Öneri |
|---|---|---|---|
| **Kontur** | Koyu mürekkep kontur (2–3 px, #140C1C ya da ink'e kayan koyu yeşil). Gri tonlamada ve turuncu zeminde siluet patlar (oyun, bilimkurgu, Toriyama ve VFX notları). | **Kontur yok, kenar ışıyor.** Oyundaki her güvenli nesnenin konturu var, o yüzden "konturu olmayan şey = enerji/ölüm" okuması kurulur (AMM notu, Popverse'teki çizgisizleşmeden türetilmiş). | Varsayılan kontur. Ayrım için tehlikenin konturu platformlarınkinden farklı olsun: daha kalın ya da koyu yeşil. K9'da kontursuz yol ayrı bir deney olarak denensin. Not: VFX'in "koyu kenar bandı" iki tarafın ortası. |
| **Parıltı / bloom** | Dış hale yok. Parıltı varsa silüetin içinde kalır, çünkü node'lar parlıyor ve dış hale hitbox'ı büyük gösteriyor (bilimkurgu, wildcard ve oyun notları). | Bloom **yalnızca asitte**, dünyanın geri kalanı düz baskı (AMM notu). | İç parıltı ve rim serbest, dış hale yasak. Bloom kullanılırsa eşik, asit ile node'lar dışında hiçbir şey aşmayacak şekilde ayarlanmalı. Node ile ayrım şekil ve renk sayısıyla yapılır (tek renk ile gökkuşağı). |
| **Çekirdek rengi** | Koyu ink/plum metal, boru ailesinden (oyun, bilimkurgu, VFX ve AMM #1). | Paper #F1E6C7 gövde, ink kontur ve vidalar, halftone gölge (Toriyama kural 9, Dieter Rams fikri). | Koyu çekirdek. Parlak halkanın içinde koyu bir merkez, ışıyan node'un tam tersi. Paper yalnızca tek bir parlama ya da vurgu olarak kullanılır. |
| **Saf beyaz** | Saf #FFFFFF yalnızca çekirdekteki sıcak noktaya ayrılır. Bugünkü "beyaz şekil = ölüm" alışkanlığını korur ve geçişi yumuşatır (AMM 000 kuralı). | Beyaz yerine paper ya da krem parlama (VFX, Toriyama); güneş gülü damlalarında küçük beyaz parlama (wildcard). | AMM fikri geçişte değerli ama UI ve VFX'te saf beyazın denetlenmesini gerektiriyor. K9'da tam haliyle denensin, diğerlerinde paper parlama kalsın. |
| **Lamba rengi** | Boşta paper, harekete geçmeden sarı, tam hızda kırmızı (bilimkurgu, HL2'den). Arızada kırmızı (Toriyama). | Yalnızca sarı. Kırmızı launch pad'le karışır (oyun notu). | Sarı varsayılan. Kırmızı yalnızca 2–3 px'lik tek bir nokta olarak, eğer kullanılacaksa. |
| **Asitte halftone** | Asit düz ve doygun kalır. Halftone yalnızca çekirdekte ya da gölgede, yoksa asit platform dokusuna benzer (oyun ve Toriyama notları). | Asitte koyu yeşil Ben-Day noktaları (bilimkurgu), sıvının gölge bandında nokta ızgarası (VFX), hatta tamamen halftone bir disk (wildcard'daki spoke). | Varsayılan düz. Halftone yalnızca gölge bandında ve platformlardan farklı bir yoğunlukta. Tam halftone disk ayrı bir konsept (K8). |
| **Renk = durum** | Hızlanınca asit sarı-yeşile kayar. Gökkuşağı ve camgöbeği yasak (AMM #8). | Yeşil → sarı → **asit-pembe** kenarlar (wildcard #15). | Pembe reddedilmeli, çünkü oyuncunun rengi. Yalnızca yeşil ↔ sarı-yeşil bandı. |

Bir gerilim de teal alan halkalarında var. Oyun, Toriyama ve VFX notları çekirdekle asit arasına ince teal #289AA4 alan halkaları koymayı öneriyor. Wildcard ise temiz eş merkezli halkaları node diline en yakın siluet sayıyor. Uzlaşma noktası, halkaların asidin **içinde**, çok ince ve soluk kalması. Halka asidin dışına taşarsa oyuncu tehlikeyi olduğundan büyük sanar.

## Aday konseptler

Her konsept kendi başına bir tasarım. Bazıları başka konseptlere katman olarak eklenebilir (özellikle K7 ve K9). Maliyetler notlardaki tahminler. Bu projenin mevcut shader'larına karşı ölçülmediler. Ölçek: **S** = sprite artı transform scripti, **SH** = tek SDF shader, **P** = Shuriken partikül, **L** = LineRenderer.

### K1. Kapsül No.7 — Capsule Corp ev aleti (güvenli taban)

*Sevimli bir Capsule Corp aleti, nefes alan bir asit küresini kadranla havada tutuyor.*

**Daire:** Hafif basık yumurta ya da yatay hap biçimli çekirdek, toplam çapın %30–40'ı. Çevresinde düşük modlu (2–3 lob) yavaşça dalgalanan bir asit diski var: `r(θ,t) = R + a·sin(2θ+ωt) + b·sin(3θ−ω₂t)`. Genlik yarıçapın ≤%5–8'i.

**Çubuk:** İki ucu şişkin stadyum kapsül çekirdek. Uç kapakları iki dikey ek yeriyle gövdeden ayrılıyor. Asit kılıfı üstte ve altta kalın, uzun kenarlarda yürüyen bir sinüs dalgası var.

**Çekirdek:** Hiçbir köşe keskin değil. Hap gibi ortadan bölen tek bir ek yeri, tek bir sarı #FCAE04 mercek, 1–2 vida. 64 px ve üstünde küçük bir "No.7" plakası ve tek bir asimetrik anten var. Yüzey düz ink/plum, tek yönlü halftone gölge, gradyan ve krom yok. İsteğe bağlı olarak teneke oyuncak dokunuşu: arkada hızla dönen bir kurma anahtarı.

**Asit:** Düz parlak asit yeşili, 2–3 bant posterize. Koyu kontur. Sol üstte dünyaya sabit tek bir sert paper parlama (çubuk dönse de ışık dönmez).

**Hareket ve telegraf:** Mercek boşta yavaş nabız atıyor, ping-pong dönüşünden önce hızlı yanıp sönüyor. Asit hız yönünde hafifçe geriliyor. Anten yay gibi sallanıp hareket yönünü gösteriyor. Kurma anahtarı varsa dönüş hızı tehlikenin hızını gösteriyor (anahtar yavaşlıyorsa tehlike de yavaşlıyor).

**İlham:** Toriyama yumurta biçimi ve deformasyonu ([Zimmerit](https://www.zimmerit.moe/akira-toriyama-mechanical-designs/); [Daizenshuu 1](https://www.kanzenshuu.com/translations/daizenshuu-1-akira-toriyama-super-interview); [Daizenshuu 5](https://www.kanzenshuu.com/translations/daizenshuu-5-akira-toriyama-super-interview/); [Gizmodo](https://gizmodo.com/akira-toriyama-mechanical-design-dragon-ball-sand-land-1851320681); [Jalopnik](https://jalopnik.com/the-creator-of-dragon-ball-is-an-unabashed-gearhead-1849136374)), numaralı kapsüller ([Kanzenshuu Vehicles](https://www.kanzenshuu.com/wiki/Vehicles)), Yerçekimi Makinesi ([Dragon Ball Wiki](https://dragonball.fandom.com/wiki/Gravity)). ISS su küresi ([NASA](https://www.nasa.gov/news-release/dancing-droplets-rock-out-on-space-station/); [Astronomy.com](https://www.astronomy.com/science/watch-iss-tests-how-water-droplets-merge-in-space/)). Teneke oyuncak ([Science Museum Group: Atom Robot](https://collection.sciencemuseumgroup.org.uk/objects/co8538872/atom-robot)). Dieter Rams'ın tek aksan düğmesi ([Wikipedia](https://en.wikipedia.org/wiki/Dieter_Rams)). FX çizim kuralları ([VFX Apprentice](https://www.vfxapprentice.com/blog/how-to-draw-stylized-water)).

**Karışma riski:** Orta-düşük. Yuvarlak çekirdek ve halka, node siluetine en yakın kombinasyon. Çekirdeği koyu tut, gökkuşağı ve dış parıltı kullanma. Oyuncakların "dost" okunmasını asit baskınlığı dengeler.

**Maliyet:** Düşük-orta. Çekirdek sprite'ı ve SDF asit shader'ı, VFX notunun Tarif 1 ve 2'si (~8–13 saat, tahmin).

### K2. Diken Tacı — ferrosıvı aşırı yükü

*Asit, cihazın alanına diken dikerek tepki veriyor. Alan güçlendikçe dikenler uzuyor ve sonunda damla olarak kopuyor.*

**Daire:** Asit halkasının dış kenarı eşit aralıklı dikenler oluşturuyor: `r = R + A·max(0,cos(nθ))^k`. Dikenler Kodama'nın tırmanan dikenleri gibi **halka boyunca kayıyor**, boyları "alan gücüyle" nabız atıyor.

**Çubuk:** Uzun kenarlarda deniz kestanesi ya da tırtıl gibi yürüyen dikenler. Hızlanan ping-pong'da diken boyu hızla birlikte artıyor.

**Çekirdek:** Kodama'nın Morpho Tower'ı gibi koyu demir bir koni ya da spiral. Dönüşü dikenlerin dönüşüyle senkron. Tek bir float hem lamba parlaklığını hem diken boyunu sürüyor.

**Asit:** Yeşil, dikenli kenar. Renkten bağımsız olarak platformların evrensel "ölümcül" silueti. AMM notunun "sivri taç" kenarıyla da örtüşüyor.

**Hareket ve telegraf:** Dönüş yönü diken eğiminden okunuyor. Aşırı yükte (tam hız ya da dönüş öncesi) dikenler çoğalıyor ve uçları çarpıştırıcısız küçük damlalar halinde kopuyor. Bu, gerçek bilimden hazır bir telegraf.

**İlham:** Rosensweig kararsızlığı ([Europhysics News](https://www.europhysicsnews.org/articles/epn/pdf/2011/03/epn2011423p17.pdf); [arXiv nlin/0102004](https://arxiv.org/pdf/nlin/0102004); [elysiatools görselleştirmesi](https://elysiatools.com/en/visualizations/ferrofluid-rosensweig-instability)), dikenlerin çoğalıp kopması ([arXiv 1711.09942](https://arxiv.org/pdf/1711.09942)), Sachiko Kodama ([CACM](https://cacm.acm.org/magazines/2008/6/5384-dynamic-ferrofluid-sculpture/fulltext); [Digital Art Archive](https://digitalartarchive.at/database/work/2230); [Pellas Gallery: Sprout](https://www.pellasgallery.com/artworks/9723-sachiko-kodama-morpho-tower-sprout-2019); [Breathing Chaos](https://www.sachikokodama.com/en/works/breathing-chaos); [Makezine](https://makezine.com/article/craft/morpho-towers-two-standin)), sivri şekil = tehlike ([AIE](https://vanphelan.github.io/AIE-Lecture-UX.pdf)).

**Karışma riski:** En düşük. Tek risk diken uçlarının çarpıştırıcıyı aşması. Dikenler kısa ve küt kalmalı ya da uçlar hitbox'a dahil edilmeli.

**Maliyet:** Düşük-orta (SH). K1'in shader'ına tek bir kenar fonksiyonu ekleniyor.

### K3. Güneş Gülü — saplı damla tacı

*Mekanik çekirdekten saplar çıkıyor. Her sapın ucunda titreyen bir asit damlası var ve damlalar birlikte halkayı oluşturuyor.*

**Daire:** Ink/plum bir göbek, 10–16 radyal pim. Pim uçlarında birbirinden faz dışı titreyen yeşil damlalar ve her damlada küçük bir parlama noktası. Radyolarya varyantında asit, birkaç kalın dikenli bir kafesin hücrelerini vitray gibi dolduruyor.

**Çubuk:** Dört kenarında saplar olan tarak ya da saç fırçası. Damlalar hareket yönünde geriliyor. Radyolarya varyantı dikenli kafesli bir baget.

**Çekirdek:** Pim kökleri. Haeckel'in public domain levhaları çizim referansı olarak izlenebilir.

**Asit:** Ayrık damlalar. Aralarında soluk, ince bir asit zarı olmalı ki tehlike tek bir dolu disk olarak okunsun. Çarpıştırıcı pim uçlarına kadar tek bir disk.

**Hareket ve telegraf:** Damlaların sinüs titremesi. Hızlanınca damlalar teğet yönde geriliyor. Biyolojiden gelen bir lore kancası da var: gerçek güneş gülü damlası asidik.

**İlham:** Drosera ([Wikipedia aynası](https://ipfs.ssi.eecc.de/ipns/en.wikipedia-on-ipfs.org/wiki/Drosera); [EPFL](https://dlab.epfl.ch/wikispeedia/wpcd/wp/s/Sundew.htm); [Naturwissenschaften 2017](https://link.springer.com/article/10.1007/s00114-017-1502-4); [SAPS](https://saps.org.uk/teaching-resources/resources/736/using-sundews-in-the-lab)). Haeckel radyolaryaları ([Mediamatic](https://mediamatic.net/de/id/224285); [John Coulthart](https://www.johncoulthart.com/feuilleton/2020/02/14/haeckels-radiolaria); [Kunstformen der Natur](https://en.wikipedia.org/wiki/Kunstformen_der_Natur)).

**Karışma riski:** Düşük, çünkü siluet dikenli. Dikkat: parıltı avı çekmek için var, yani gözü çeker ve ödül gibi okunabilir. Parlama noktası küçük kalmalı. Damlalar arasındaki boşluklar "geçilebilir mi?" sorusunu doğurur.

**Maliyet:** S–M. Radyal olarak çoğaltılmış tek bir sap ve damla, ölçekte sinüs titremesi. Radyolarya kafesi elle çizim ister (M).

### K4. Levitatör Köprüsü — iki emitör arasında tutulan asit

*İki tombul emitör kapağı arasında asit, duran dalganın düğümlerinde basık damlalar halinde asılı duruyor.*

**Çubuk (en güçlü uyum):** İki uçta hoparlör ızgarası gibi 2×3 küçük daire ya da bobin şeritli tombul kapaklar ve kapaklarda durum lambaları. Arada 3–5 hafif basık asit damlası ve düğümleri işaretleyen soluk, kesikli basınç bantları. Sürekli varyantında kapaklar arasında gergin bir asit levhası var (Combine alan köprüsü, AMM'nin kelepçe emitörleri). Asit kapaklarda biraz geri çekiliyor, sanki tutuluyormuş gibi.

**Daire:** Halka biçimli bir duran dalga: çekirdeğin etrafında dönen 6–8 asit boncuğundan bir kolye. Ya da asit diskinin içinde 3–4 kısa direği olan bir göbek.

**Çekirdek:** Kapaklar emitör. Çubuk uzunluğu değişince damla sayısı dünya birimine göre artıyor, damla boyu sabit kalıyor.

**Asit:** Basık elips damlalar ya da düz yeşil levha. Levhada kayan gürültü var.

**Hareket ve telegraf:** HL2 kuralı: uç lambaları harekete geçmeden önce sarı yanıyor. Damlalar hız arttıkça daha çok basılıyor.

**İlham:** Akustik levitasyon ([PMC9500997](https://pmc.ncbi.nlm.nih.gov/articles/PMC9500997); [arXiv 2307.00400](https://arxiv.org/pdf/2307.00400); [LSBU IUS2023](https://openresearch.lsbu.ac.uk/download/0e87207296252f9a6248cbd4040a72b2cf45c5416f6a51906d5df19214ebf2c8/518934/IUS2023_proceedings_Investigation%20of%20ultrasonically%20levitated%20droplets%20for%20sonochemistry%20using%20high%20speed%20camera%20imaging_v4.pdf); [IEEE IUS 2019](https://ewh.ieee.org/conf/ius/2019/media/files/0761.pdf)), TinyLev ([Open Electronics](https://www.open-electronics.org/?p=21501); [Andrade/Camargo/Marzo](https://typeset.io/pdf/automatic-contactless-injection-transportation-merging-and-566nhflvhy.pdf)), akustik tractor beam ([Scientific American](https://www.scientificamerican.com/article/real-life-tractor-beam-can-levitate-objects-using-sound-waves/); [NBC News](https://www.nbcnews.com/id/wbna54154146)). Combine alanları ([Force Field](https://combineoverwiki.net/wiki/Forcefield); [Powered bridge](https://combineoverwiki.net/wiki/Powered_bridge); [Combine Interface](https://combineoverwiki.net/wiki/Combine_Interface); [Power Generator](https://half-life.fandom.com/wiki/Combine_Power_Generator)).

**Karışma riski:** Düşük, kapaklar da öldürüyorsa. Asıl risk boşluklar: ya gerçekten güvenli olmalılar (ayrı çarpıştırıcılar, bilinçli bir "iğne deliğinden geçme" tasarımı, insan kararı) ya da ince bir asit filmiyle doldurulmalılar.

**Maliyet:** Düşük-orta (S ve SH). Dikdörtgenlerin bugünkü scale ile uzatılmasına en doğal uyan konsept bu.

### K5. Yerçekimi Sürücüsü — ters dönen halkalar (klasik ama riskli)

*Küçük bir çekirdeğin etrafında farklı eğimlerde ters dönen halkalar asidi yörüngede tutuyor. Halkaların hizalanması bir olay.*

**Daire:** Ink bir çekirdek küre. 2–3 ince halka farklı eğimli elipsler olarak ters yönlerde dönüyor. Asit diski çekirdekle dış halka arasında. Dönen yollarda halkalar yol yönünün tersine dönüyor.

**Çubuk:** Makara: çubuğun iki ucunda halka grupları, aralarında asit kılıfı. Armil varyantında çubuğun etrafında dikdörtgen bir gimbal çerçevesi dönüyor.

**Çekirdek:** Ink küre, halkalar ink ya da paper, asla renkli değil.

**Asit:** Bütün halkalar asit silüetinin **içinde** kalıyor ya da halkanın kendisi ölümcül dış hat oluyor.

**Hareket ve telegraf:** Event Horizon'da halkaların hizalanması geçidi açıyor. Burada hizalanma anı yön değişiminin telegrafı olabilir. Halkalar dönüş noktasında hizalanıyor, oyuncu ritmi okuyor.

**İlham:** Event Horizon ([Wikipedia](https://en.Wikipedia.com/wiki/Event_Horizon_%28film%29); [RetroZap](https://retrozap.com/event-horizon-1997-sci-fi-saturdays/); [CGTrader model](https://cgtrader.com/3d-print-models/miniatures/memorabilia/event-horizon-gravity-drive); [American Cinematographer](https://theasc.com/article/needs-pics-unearthly-terrors-event-horizon/)). Armil küre ([RMG](https://www.rmg.co.uk/collections/objects/rmgc-object-10800); [RMG kâğıt halkalı küre](https://www.rmg.co.uk/collections/objects/rmgc-object-19913); [V&A yüzük](https://collections.vam.ac.uk/item/O118457/ring-unknown); [British Museum](https://www.britishmuseum.org/collection/object/H_AF-1761)).

**Karışma riski:** **Yüksek.** İç içe halkalar grapple node siluetine en yakın biçim. Armil formları tarihte mücevher olarak yapılmış, yani collectible gibi okunabilir. Jüri için "en tanıdık bilimkurgu, en riskli okuma" seçeneği.

**Maliyet:** Düşük (S). 2–3 halka sprite'ı ve bir döndürme scripti.

### K6. Manyetik Şişe — helis şeritlerle sarılmış asit simidi

*Asit halkası, onu yerinde büken bir manyetik şişe gibi görünen helis alan şeritleriyle sarılmış.*

**Daire:** Önden görülen bir asit torusu. Etrafında ink ve sarı köşegen şeritler kayıyor (halkaya sarılmış berber direği). 3–4 noktada kelepçeli bobin blokları var. Asidin içinde Pettit'in "Jüpiter" küresi gibi iki yakın yeşil tonu yavaşça girdap yapıyor. Girdabın yönü dönen tehlikenin dönüş yönünü gösteriyor.

**Çubuk:** Çekirdek çubuğu saran stadyum biçimli bir torus, köşelerde bobinler, uzun kenarlarda yürüyen helis bantları: `frac((p.x + p.y·twist)·k − time·speed)`. Ters varyant (diamanyetik kurbağa): Bauhaus şeritli bobin kılıfı dışta, asit içte yüzüyor ve uçlardan ve yarıklardan görünüyor.

**Çekirdek:** Bobin blokları, Toriyama tarzı vidalarla.

**Asit:** Yeşil gövde ve kayan ink/sarı şeritler. Şeritler asla beyaz ya da pembe değil.

**Hareket ve telegraf:** Şerit kayma hızı tehlikenin hızına bağlı. Hızlanırken şeritler sıklaşıyor.

**İlham:** Tokamak ([DOE](https://www.energy.gov/node/4444676); [ITER](https://www.iter.org/sci/goals); [ITER Technical Gallery](https://iter.org/node/18394); [ITER: The Tokamak](https://www.iter.org/node/2304)), Pettit'in Jüpiter küresi ([Space.com](https://www.space.com/nasa-astronaut-don-pettit-jupiter-water-ball-iss); [NASA iss072e742508](https://www.nasa.gov/image-detail/amf-iss072e742508)), diamanyetik levitasyon ([Wikipedia](https://en.wikipedia.org/wiki/Diamagnetism); [MRIquestions](https://mriquestions.com/how-to-levitate-a-frog.html); [Physics World](https://physicsworld.com/a/floating-femtodroplets/)), VFX çubuk tarifi.

**Karışma riski:** Orta. Şeritli halka şeker ya da simit gibi okunabilir. Krem şekerli zeminle uyumlu ama tehdidi yumuşatıyor. Ink ve sarı şeritler bunu dengeliyor. Sarı-siyah şerit ISO uyarı diline de yaklaşıyor.

**Maliyet:** Orta (SH). Halka/stadyum üzerinde UV kaydırma.

### K7. Uyarı Grameri — sarı-siyah bant ve oyuna ait glif

*Dış asit ne yaparsa yapsın, çekirdek hep sarı-siyah bantla sarılı ve ilk odada öğretilen, oyuna ait bir glif taşıyor.*

**Daire:** Çekirdek diskinde 45° sarı #FCAE04 / ink #140C1C şeritler, etrafında asit halkası. Glif varyantında üç asit lobu üç kelepçe kolla çekirdeğe tutturulmuş ve dönüyor.

**Çubuk:** Çekirdek çubuğun kendisi uyarı bandı, asit onu kılıf gibi sarıyor. Gövdede Toriyama tarzı stencil bir kod (oda numarası olarak da işlev görebilir). Glif varyantında kenar boyunca tekrar eden yarım daire asit şişkinlikleri var.

**Çekirdek:** Şerit dokusu ya da şerit shader'ı. Lost Caves'in çözümüyle aynı mantık: koyu gövde, üstünde tehlike rengi.

**Asit:** Herhangi bir konseptin asidi. Bu konsept esas olarak bir **gramer katmanı**.

**Hareket ve telegraf:** Bant UV'si hızla kayıyor. Glif ilk karşılaşmada güvenli bir ortamda öğretiliyor (Baldwin dersi: akılda kalıcılık birebir anlamdan önemli).

**İlham:** ISO 3864 ([ISO 3864-1984 örnek PDF](https://cdn.standards.iteh.ai/samples/9454/f60dff81d94842b5988b657765f43a63/ISO-3864-1984.pdf); [Wikipedia](https://en.wikipedia.org/wiki/ISO_3864); [ISO 3864-1:2011](https://standards.iteh.ai/catalog/standards/iso/e38ba1f3-bbc1-4939-8df8-3078467e1f7c/iso-3864-1-2011)), biyotehlike sembolü ([Science History Institute](https://www.sciencehistory.org/collections/blog/the-biohazard-warning-symbol/); [99% Invisible](https://99percentinvisible.org/article/biohazard-symbol-designed-to-be-memorable-but-meaningless/)), koyu gövde ve tehlike çatlakları ([Lost Caves](https://adscomics.itch.io/lost-caves/devlog/173099/lost-caves-devlog-10-subtle-indicators)).

**Karışma riski:** Çok düşük. Tek risk "fazla endüstriyel" bir his. Üç loblu glif fidget spinner ya da oyuncak gibi okunabilir. Damla ve ses bunu dengeler.

**Maliyet:** Düşük (S). Asıl maliyet sanat değil, bölüm tasarımında öğretme anı.

### K8. Delikli Baskı Diski — halftone alanı (baskı diline en yakın)

*Asit, halftone noktalarından oluşan bir disk. Üstünde farklı hızlarda dönen delikli Bauhaus diskleri ve koyu "spoke" kamaları var, dışarıdan Kirby Krackle noktaları çekirdeğe akıyor.*

**Daire:** Küçük, yoğun bir çekirdek. Polar UV üzerinde halftone noktalı bir asit diski ve diskten farklı hızda dönen 2–3 koyu kama. Moholy varyantında iki delikli disk (paper ve ink) asidin üzerinde ters dönüyor, asit yalnızca deliklerden görünüyor ve moiré titremesi bedavaya geliyor. Krackle katmanında ink nokta kümeleri halkaya doğru içe akıp kayboluyor ve cihazın asidi tuttuğunu görünür kılıyor.

**Çubuk:** Çekirdek çubuğun etrafında stadyum biçimli halftone asit bandı. Spoke'lar bant boyunca kayan dik koyu şeritlere dönüşüyor. Moholy varyantında delikli plakalar obtüratör gibi birbirinin üstünden kayıyor, asit kenarlardan sızıyor. Krackle noktaları emitörlere doğru akıyor.

**Çekirdek:** Küçük ve koyu. Asıl "makine" dönen diskler.

**Asit:** Halftone yeşil noktalar. Dış kenar **düzensiz ya da damlayan** olmalı, çünkü pürüzsüz bir disk platform ya da halka hedef gibi okunur. Halftone ızgarası dünya uzayında olmalı, yoksa uzatılan çubukta noktalar elipsleşir.

**Hareket ve telegraf:** Spoke'ların karşı dönüşü "çalışan makine" diyor. Dönüş hızı tehlike hızına bağlı.

**İlham:** Moholy-Nagy Light Prop ([Met](https://www.metmuseum.org/art/collection/search/899299); [Harvard Gazette](https://news.harvard.edu/gazette/story/2007/07/light-prop-shines-again/); [Vimeo](https://vimeo.com/111474831); [Tate Papers](https://www.tate.org.uk/research/tate-papers/08/replicas-of-laszlo-moholy-nagys-light-prop-busch-reisinger-museum-and-harvard-university-art-museums); [metaLAB](https://metalab.harvard.edu/p/light-prop)). Satürn spoke'ları ([JPL PIA07731](https://www.jpl.nasa.gov/images/pia07731-finally-spokes/); [Photojournal PIA07731](https://photojournal.jpl.nasa.gov/catalog/PIA07731); [PIA08877](https://photojournal.jpl.nasa.gov/catalog/PIA08877); [PIA10450](https://www.jpl.nasa.gov/images/pia10450-like-spokes-of-old/); [NASA Science](https://science.nasa.gov/resource/finally-spokes)). Kirby Krackle ([Wikipedia](https://en.wikipedia.org/wiki/Kirby_Krackle); [Kirby Museum](https://kirbymuseum.org/blogs/simonandkirby/wp-json/wp/v2/posts/2600)). Mario Galaxy'nin dokuyla alan anlatımı ([MarioWiki](https://www.mariowiki.com/Gravity_field)).

**Karışma riski:** Orta. Pürüzsüz disk platform gibi okunabilir, Krackle noktaları para ya da collectible gibi okunabilir. Noktalar ink renkli ve küçük kalmalı, asla sarı ya da parlak olmamalı. Halftone asit, "asitte halftone yok" diyen notlarla çelişiyor (bkz. Ortak kurallar).

**Maliyet:** Orta (tek polar SH, mevcut halftone bilgisi yeniden kullanılıyor). Moholy diskleri iki maskeli dönen sprite.

### K9. Fosforlu Rejim — iki kez basılmış asit ve saf beyaz çekirdek

*Dünya basılı ve durgun. Asit ışıyor ve iki kez basılmış. Ortasında oyunun tek saf beyazı yanıyor.*

**Daire:** Koyu bir çekirdek diski, merkezinde saf #FFFFFF bir sıcak nokta. Tek tonlu fosforlu yeşil bir halka, kenarı ışıyor (bu konseptte mürekkep kontur yok). Halkanın arkasında 2–4 px kaymış sarı #FCAE04 ikinci bir baskı var. Çakıştıkları yer koyulaşıyor (multiply) ve doğal bir kenar oluşturuyor. Kayma yönü hareketin tersine sürükleniyor.

**Çubuk:** Uçlarda boru eriği renginde kelepçe emitörler, aralarında asılı bir asit levhası. Emitörlerin asitle birleştiği yerde saf beyaz yarıklar var. Hızlanınca riso kayması artıyor ve bir hız göstergesine dönüşüyor.

**Çekirdek:** Düz baskı, koyu. Saf beyaz yalnızca burada. Bu, bugünkü "beyaz şekil = ölüm" alışkanlığını yeni tasarıma köprülüyor.

**Asit:** Bir ana ton, bir ofset (sarı), bir koyu ton, bir saf beyaz. Daha fazlası hızda okunmuyor. Renk durumu izliyor: sakin halde saf yeşil, hızlanırken (AcceleratingPingPong) `_Heat` parametresiyle sarı-yeşile kayıyor. Gökkuşağı döngüsü, camgöbeği ve pembe yasak.

**Hareket ve telegraf:** Isı kayması ve riso ofseti hızla birlikte büyüyor.

**İlham:** AMM'nin iki rejimi, 6+2 paleti, 000'ın saf beyazı ([AIPT](https://aiptcomics.com/2025/03/25/javier-rodriguez-absolute-martian-interview/); [SKTCHD](https://sktchd.com/art-feature/javier-rodriguez-absolute-art-feature/); [Popverse](https://www.thepopverse.com/comics-absolute-martian-mahunter-dc-comics-art-javier-rodriguez/); [Comic Watch #9](https://comic-watch.com/comic-book-reviews/absolute-martian-manhunter-9-the-color-of-despair); [GamesRadar](https://www.gamesradar.com/comics/marvel-comics/70-years-after-the-martian-manhunters-debut-the-justice-league-mainstay-has-been-reinvented-in-the-absolute-dc-universes-most-daring-and-compelling-series-yet); [Comic Watch #1](https://comic-watch.com/comic-book-reviews/absolute-martian-manhunter-1-written-in-full-color); [DC blog](https://www.dc.com/blog/2025-12-05/absolute-martian-manhunter-shows-you-your-absolute-self)). Riso baskı ([Moniker Press](https://monikerpress.ca/colours); [Possible Worlds](https://possibleworldsshop.squarespace.com/risograph-set-up); [Oxford Green Print](https://oxfordgreenprint.com/home/design-for-riso/); [Riso Club](https://uxpilot.ai/labs/creative-direction-playground/riso-club)).

**Karışma riski:** Orta. Işıma node'larla karışabilir, ayrım tek renk ile gökkuşağı farkına dayanıyor. Saf beyaz disiplini UI ve VFX denetimi gerektiriyor. Fazla riso kayması "hatalı render" gibi görünebilir.

**Maliyet:** Düşük. Shader'da ikinci bir ofset örneklemesi ve bir `_Heat` parametresi. Hızı stratejiden okuyan küçük bir MonoBehaviour.

### K10. Ters Nazar — oyuncuyu izleyen göz çekirdek (cesur)

*Nazar boncuğu kötüleşmiş: kobaltın yerinde asit var, göz bebeği mekanik çekirdek ve oyuncuyu izliyor.*

**Daire:** Eş merkezli katmanlar: ink çekirdek (keskin iris diyafram bıçakları ve küçük kırmızı #FC1E00 göz bebeği), paper halka, asit halkası, ince sarı kenar (geleneksel altın kenar). Göz bebeği oyuncuya doğru kayıyor.

**Çubuk:** Çubuk boyunca uzanan badem ya da mercek biçimli bir göz, veya bir nazar dizisi gibi çubukta üç göz. Emitörlerde yatay bir göz yarığı oyuncu yaklaştıkça açılıyor.

**Çekirdek:** Ma.K.'nin tek büyük merceği, Marslı'nın tek kırmızı gözü, Fantastic Planet'teki Draag'ların kırmızı gözleri.

**Asit:** Yeşil halka, kenardan damlıyor. Damlama düşmanca okumayı güçlendiriyor.

**Hareket ve telegraf:** Göz bebeği oyuncuyu izliyor ("uyanık" hissi). Yaklaşınca diyafram açılıyor.

**İlham:** Nazar ([Wikipedia](https://en.wikipedia.org/wiki/Nazar_%28amulet%29); [Turkey Travel Planner](https://turkeytravelplanner.com/details/NazarBonjuk/index.html); [Anatolian Archaeology](https://www.anatolianarchaeology.net/?p=6754)), Marslı'nın kırmızı tek gözü ([Comic Watch #2](https://comic-watch.com/comic-book-reviews/absolute-martian-manhunter-2-streams-of-colorful-consciousness)), Fantastic Planet ([TCM](https://www.tcm.com/articles/1299220/fantastic-planet); [Animation Magazine](https://animationmagazine.net/2023/12/fantastic-planet-at-50-revisiting-rene-lalouxs-cult-classic); [Skwigly](https://skwigly.co.uk/laloux-toppers-beautiful-world-the-50th-anniversary-of-fantastic-planet); [kareler](https://screenmusings.org/movie/blu-ray/Fantastic-Planet/pages/Fantastic-Planet-009.htm)), Edelmann'ın renk anlatısı ([Wikipedia](https://en.wikipedia.org/wiki/Heinz_Edelmann); [AWN](https://www.awn.com/mag/issue3.4/3.4pages/3.4cohen.html)), Ma.K. ([Wikipedia](https://en.wikipedia.org/wiki/Ma.K._in_SF_3D)).

**Karışma riski:** **Yüksek.** Eş merkezli halkalar node dili ve nazar kültürel olarak koruyucu ve uğurlu, yani collectible ya da tılsım gibi okunabilir. Göz bademsi değil keskin diyafram olmalı, kırmızı nokta küçük kalmalı. Ancak takip eden göz ve damlalar onu tartışmasız düşmanca yaparsa denenmeli. Türk temalı bir dünyaya özel varyant olarak daha güvenli.

**Maliyet:** Orta (S–M). Katmanlı daireler, oyuncuya bakan bir script, diyafram animasyonu.

### K11. Ters Damla — Magritte penceresi (en sürreal)

*Asit yanlış yöne damlıyor, çekirdeğe doğru. İçinden de gökyüzünün tersi görünüyor.*

**Daire:** Halka kenarından kopan damlalar dışarı değil çekirdeğe doğru "düşüyor". Halkanın alt yarısı hafifçe sarkıyor: sıvı düşmek istiyor, cihaz onu tutuyor. Halkanın içi arka planı değil başka bir şeyi gösteriyor: ters çevrilmiş gök, negatif renk ya da iki yakın yeşilde dönen, düşük kontrastlı Steranko op-art halkaları.

**Çubuk:** Levhanın alt kenarından damlalar yukarı, levhaya geri damlıyor. Emitörlerin altında asılı damla uçları var. Levhanın içinde diyagonal moiré çizgiler ya da akan ters bir gök.

**Çekirdek:** Herhangi bir koyu çekirdek. Sürreallik asitte.

**Asit:** Siluet kenarı net ve yüksek kontrastlı kalıyor. Sürreal içerik yalnızca içeride ve düşük kontrastlı (AMM kural 1: siluet sabit, içerik sürreal).

**Hareket ve telegraf:** Boşta alttan bir damla sarkıyor ve geri çekiliyor (başarısız kaçış). Pettit tabletindeki kürecikler gibi dönüş öncesinde damlalar fışkırıp geri dönüyor.

**İlham:** Despero'nun akan balonları ([Comic Watch #9](https://comic-watch.com/comic-book-reviews/absolute-martian-manhunter-9-the-color-of-despair)), Magritte ve düşünce dumanı ([SKTCHD](https://sktchd.com/art-feature/javier-rodriguez-absolute-art-feature/); [GeekDad](https://geekdad.com/2025/03/review-absolute-martian-manhunter-1-colors-of-the-mind/)), Steranko ([Book Riot](https://bookriot.com/celebrating-the-psychedelic-surrealism-of-jim-steranko/); [Book Riot 2](https://bookriot.com/revisiting-the-psychedelic-surrealism-of-jim-steranko/)), Gilland'ın yerçekimi ve viskozite vurgusu ([Parka Blogs](https://www.parkablogs.com/node/1783)), Pettit'in tableti ([SlashGear](https://www.slashgear.com/watch-an-effervescent-tablet-fizz-inside-a-ball-of-water-in-space-28394568); [NASA iss072e069828](https://www.nasa.gov/image-detail/amf-iss072e069828)).

**Karışma riski:** Orta-yüksek. Op-art hızda göz yorabilir ve gürültü yaratabilir. Damlalar çarpıştırıcısız olmalı. Node karışması düşük.

**Maliyet:** Orta. Desen sürümü ucuz (polar/kayan UV). Ters gök için URP 2D'de Camera Sorting Layer Texture gerekiyor, bu daha pahalı. Damlalar için smin ve negatif yerçekimli Shuriken.

### K12. Kıvılcım Kubbesi — güç küresi ve arklar

*Parlatılmış bir kubbe ya da güç küresi çekirdek. Asit halkası, çekirdekten sıçrayan arklarla yerinde tutuluyor.*

**Daire:** Halftone gölgeli paper bir kubbe, ya da HL2'nin Enerji Topu ve Laputa'nın kristali gibi sarı ya da teal bir güç küresi. Çekirdekle asit halkası arasında 3–5 kısa, kırık ink/sarı ark titriyor.

**Çubuk:** Çubuk boyunca küçük kubbeler dizisi ya da pil yuvasındaki 2–3 küre. Arklar asit kılıfına köprü kuruyor.

**Çekirdek:** Kubbe ya da küre. HL2 dersi: küre çıkarılınca alan kapanıyor, neden-sonuç tek bakışta okunuyor.

**Asit:** Yeşil halka, arklarla "bağlı".

**Hareket ve telegraf:** Arklar hızlanmadan önce yoğunlaşıyor. Elektrik evrensel bir "dokunursan ölürsün" işareti.

**İlham:** Van de Graaff ve Tesla bobinleri ([Atlas Obscura](https://atlasobscura.com/places/worlds-largest-airinsulated-van-de-graaff-generator); [Museum of Science](https://mos.org/inside-the-theater-of-electricity); [Museums Victoria](https://museumsvictoria.com.au/scienceworks/visiting/lightning-room)), HL2 Enerji Topu ve Yerçekimi Silahı ([Energy Ball](https://combineoverwiki.net/wiki/Energy_Ball); [Gravity Gun](https://combineoverwiki.net/wiki/Gravity_Gun)), Laputa kristali ([grape Japan](https://grapeejapan.com/?p=199710)).

**Karışma riski:** Node karışması düşük ama parlayan küre çekirdek node parıltısına yaklaşıyor. Asıl sorun **iki tehlike türünü** (elektrik ve asit) tek nesnede karıştırması. Hangisinin öldürdüğüne karar verilmeli.

**Maliyet:** Orta-yüksek (L ya da flipbook ile prosedürel ark ve shader).

### Konseptler bir bakışta

| | Konsept | Cesaret | Okunurluk | Node/collectible karışması | Maliyet | En güçlü yanı |
|---|---|---|---|---|---|---|
| K1 | Kapsül No.7 | Güvenli | Yüksek | Orta-düşük | Düşük-orta | Taban: her şey üzerine kurulur |
| K2 | Diken Tacı | Güvenli | En yüksek | En düşük | Düşük-orta | Renkten bağımsız ölüm silueti |
| K3 | Güneş Gülü | Orta | Yüksek | Düşük | S–M | Organik, lore kancası |
| K4 | Levitatör Köprüsü | Güvenli | Yüksek | Düşük | Düşük-orta | En iyi çubuk çözümü |
| K5 | Yerçekimi Sürücüsü | Orta | Orta | **Yüksek** | Düşük | Hizalanma telegrafı |
| K6 | Manyetik Şişe | Orta | Yüksek | Orta | Orta | Helis akış, hız okunur |
| K7 | Uyarı Grameri | Güvenli | En yüksek | Çok düşük | Düşük | Her konsepte katman |
| K8 | Delikli Baskı Diski | Cesur | Orta | Orta | Orta | Halftone ve Bauhaus'a en yakın |
| K9 | Fosforlu Rejim | Cesur | Orta-yüksek | Orta | Düşük | AMM rengi, beyazdan köprü |
| K10 | Ters Nazar | Cesur | Orta | **Yüksek** | Orta | Türk kimliği, "uyanık" tehlike |
| K11 | Ters Damla | En cesur | Orta | Düşük | Orta | Dünyanın kuralları burada bozuk |
| K12 | Kıvılcım Kubbesi | Orta | Yüksek | Orta | Orta-yüksek | Güçlü telegraf, ama iki tehlike türü |

Notlarda yer alıp elenen fikirler de var. Calder'in karşı ağırlıklı mobilleri sarkaç hareketiyle yeni bir oynanış kalıbı açıyor ([Calder Foundation](https://calder.org/works/standing-mobile/little-ball-with-counterweight-1931/)), ama bu bir görünüm değil yeni bir tehlike **türü**. GDD kontrolü gerektirdiği için listeye girmedi. Googie starburst'ü saf bir süs ([Wikipedia](https://en.wikipedia.org/wiki/Googie_architecture)) ve "parıltı/ödül" okunuyor. Ay denizanasının pembe-mor organları ([Naturalis](https://sat-zooplankton.linnaeus.naturalis.nl/linnaeus_ng/app/views/species/taxon.php?id=25681)) oyuncu pembesiyle çakışıyor ve sevimli okunuyor. Teneke kurmalı oyuncak K1'e, Satürn spoke'ları K8'e, armil küre K5'e, akustik kolye K4'e eklendi.

## Tek shader ve tek çarpıştırıcı kaynağı, ilk okunur sürüm birkaç günde

Konsept seçimi uygulama mimarisini pek değiştirmiyor, çünkü 12 konseptten dokuzu aynı omurgayı paylaşıyor: tek bir Sprite Unlit Shader Graph'ı, daire ve dikdörtgen modu, ortak Sub Graph'lar (`AcidBands`, `EdgeWobble`, `Halftone`). Dikdörtgenlerde UV uzayında çalışan Rounded Rectangle node'u kullanılmamalı, çünkü scale ile uzatılınca köşeleri elipsleşiyor ([Unity Procedural Nodes](https://docs.unity3d.com/Packages/com.unity.shadergraph@17.1/manual/Procedural-Nodes.html)). Doğru yol, boyutu bilen bir rounded box SDF'si: `p = (uv − 0.5)·size`. Böylece köşe yarıçapı, kontur, dalgalanma ve halftone uzunluktan bağımsız kalıyor, 9-slice gerekmiyor. Burada test edilmemiş bir tuzak var. 2D renderer SpriteRenderer'ları dinamik olarak birleştirirse Object Scale (1,1) gelebilir. Yedek yol ekran türevlerinden boyut almak (`fwidth(worldPos)/fwidth(uv)`) ya da quad MeshRenderer kullanmak. İki farklı ölçekli örnek ve Frame Debugger ile doğrulanmalı. Örnek başına MaterialPropertyBlock kullanılmamalı, çünkü renderer'ı SRP Batcher'dan çıkarıyor ([Unity Manual](https://docs.unity3d.com/Manual/SRPBatcher-Incompatible.html)). `SpriteRenderer.color` ise batching'i bozmuyor ([Unity forum](https://discussions.unity.com/t/batching-and-sprite-renderers/792099)). Doku kullanmamak, 2D SRP Batcher'ın desteklemediği `_TexelSize` sorununu da ortadan kaldırıyor ([Unity forum](https://discussions.unity.com/t/srp-batcher-warnings-for-spriterenderers-with-urp-lit-shader/1542677)).

Çarpıştırıcı ve shader aynı veriden beslenmeli. CircleCollider2D ya da BoxCollider2D aynı transform'la ölçeklenmeli, BoxCollider2D'nin `edgeRadius`'u SDF köşe yarıçapına eşitlenmeli, değerler projenin kalıbına uygun bir ScriptableObject'te durmalı. Shader'a bir `_DebugCollider` anahtarı eklenip çarpıştırıcı sınırı ince bir çizgi olarak çizilirse görsel-çarpıştırıcı uyumu sahnede gözle doğrulanabilir. Mevcut tehlike çarpıştırıcılarının boyutları notlarda okunmadı. Uygulamadan önce bakılmalı. Halkanın ortası geçilebilir olacaksa Unity'de tek bir halka primitifi yok, PolygonCollider2D ya da EdgeCollider2D gerekiyor. Bu, O3'teki "tamamı öldürür" varsayılanını destekleyen pratik bir neden. Partiküller için Shuriken doğru seçim. VFX Graph'ın URP'de diğer saydam nesnelerle sıralanması "Won't Fix" olarak kapanmış eski bir sorun ([Unity Issue Tracker](https://issuetracker.unity3d.com/issues/urp-sorting-vfx-cannot-be-sorted-with-other-transparent-objects-in-urp)). Shuriken ise sorting layer kullanabiliyor. Damlalar Emit from Edge, World simulation, Inherit Velocity ve Stretched Billboard ile hıza göre uzuyor. Emission Rate over Distance sayesinde duran tehlike damla üretmiyor. CRT filtresi ince detayları yediği için kontur ve parlama ≥2–3 ekran pikseli olmalı.

VFX notunun saat tahminleri kaynaklı değil, ama bir sıra veriyor. Daire tarifi ~4–6 saat, çubuk tarifi ~3–5 saat, çarpıştırıcı ve debug katmanı ~1–2 saat. İlk okunur sürüm toplam ~8–13 saat. Damla ve halftone cilası ~5–7 saat daha. Projenin 1–1.5 saatlik task kuralına göre bu ~10–14 task ediyor. Jüri için önerilen prototip sırası şöyle. Önce **K1 çekirdeği, K2 kenarı ve K7 bandı** tek bir daire örneğinde birleştirilir (en güvenli taban). Sonra aynı aile **K4'ün sürekli levha çubuğu** ile denenir. Ardından aynı shader'a parametre olarak bir cesur aday eklenir: K9'un riso ofseti ve ısı kayması en ucuzu, K8 ve K11 en ayırt edicileri. Hepsi tam hızda, bir grapple node'unun ve bir ananasın hemen yanında, gri tonlamalı bir ekran görüntüsüyle ve gamepad ile yan yana oynanmalı.

## Sonuç

Araştırmanın asıl ortaya çıkardığı şey "yerçekimi cihazı" fikrinin bir kostüm değil, bir **okunurluk aracı** olduğu. Gerçek bilimde sıvının alana verdiği tepkiler (diken çıkarma, düğümlerde dizilme, aşırı yükte kopma) tam olarak bir platformerın telegraf için ihtiyaç duyduğu sinyaller. Ferrosıvının "alan güçlendikçe dikenler uzar ve kopar" davranışı, hızlanan ping-pong'un hızını oyuncuya renk kullanmadan anlatıyor. Akustik düğümler, scale ile uzatılan çubukların neden daha fazla damla taşıdığını açıklıyor. Toriyama'nın "nereden binilir, ne yanar" ilkesi her detayı bir işleve bağladığı için çekirdeği gürültüden koruyor. Bu yüzden önerilen yaklaşım bir görünümü tutturmak değil, fiziği tutarlı bir küçük makine ailesi kurmak. Bir kez öğretilince her yeni yol tipi (dairesel, dönen, hızlanan) aynı dilden okunur.

İkinci çıkarım, asıl tasarım riskinin tehlikenin ölümcül görünmemesi değil, **güvenli öğelere benzemesi**. Notların neredeyse tüm anlaşmazlıkları (kontur, parıltı, beyaz, halkalar) aynı soruya çıkıyor: grapple node'larının parlayan, gökkuşağı halkalı dilinden ne kadar uzak durulmalı? Dört ayrı notun bağımsız olarak koyu çekirdeği ve dış halesi olmayan asidi seçmesi tesadüf değil. Parlak halkanın içindeki koyu merkez, ışıyan node'un tam negatifi. Sürreal ve cesur konseptler (K8–K11) değerini ancak bu negatif kural korunduğu sürece taşıyor. Bu yüzden jürinin ilk kararı hangi konseptin seçileceği değil, kontur ve parıltı tartışmasının nasıl çözüleceği olmalı. Geri kalan her şey o karardan sonra tek bir shader parametresi.
