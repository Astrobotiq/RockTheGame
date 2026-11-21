# B: Oyunlar, Animasyon ve Hareket Dili — Çiçek Node'u Araştırması

Tarih: 2026-11-20. Kapsam: yalnızca araştırma; kod/sahne değişmedi.

Etiketler: **[Doğrulandı]** = sayfayı açıp okudum, bilgi orada. **[Kısmi]** = sayfa açıldı ama bilgi dolaylı/ikincil. **[Çıkarım]** = kaynağa dayanan ama bu projeye benim uyarlamam. Telifli metin kopyalanmadı; her şey kendi cümlemle özetlendi.

---

## 0. En değerli 5 bulgu (tek cümle)

1. **Venus flytrap kapanışı çift kararlı (bistable) bir "snap"tir: yaprak, eğrilik biçiminde enerji depolar ve ~0,1 sn'de ters yüze döner** — çiçeğin "titreme (enerji birikir) → tek hamlede snap" uyarı/kapanış yapısı için doğadan gelen hazır model ([Harvard Magazine](https://www.harvardmagazine.com/2005/05/leaves-that-lunch-html)).
2. **Wonder Flower'ın rengi dünyaya göre değişiyor (mavi, camgöbeği, kırmızı-magenta, sarı, yeşil, mor, turuncu, gri-sarı); kimliği renk değil davranış/şekil taşıyor** — çiçeğin pembesini karakterden uzaklaştırmak "çiçek kimliğini" bozmaz ([Super Mario Wiki](https://www.mariowiki.com/Wonder_Flower)).
3. **Pikmin'de olgunluk (yaprak → tomurcuk → çiçek) başın üstündeki şekille okunuyor, durum rengi ayrı bir katman** — kapalı/açık durumunun silüet farkıyla taşınması kanıtlı bir yöntem ([Pikipedia: Bud](https://www.pikminwiki.com/Bud)).
4. **Zamanlı platform uyarısı için elimizdeki en somut sayılar: Celeste crumble bloğu 0,6 sn'de çöker, 2 sn'de geri gelir; Dark Souls 3 analizine göre windup+vuruş en az ~340 ms, insan algısı ~100 ms** — uyarı süresi için alt sınır tahmini bu verilerden çıkıyor ([Everest/Celeste dokümantasyonu](https://github-wiki-see.page/m/coloursofnoise/Resources/wiki/Entity-and-Trigger-Documentation), [Game Developer: DS3](https://www.gamedeveloper.com/game-platforms/anatomy-of-an-enemy-attack-in-dark-souls-3)).
5. **Karakter rengini dünyadan ayırmanın kanıtlı üç kolu var: hue/doygunluk/değer hiyerarşisi (oyuncu > düşman > etkileşim > arka plan), sönük dünya + tek parlak karakter (Journey'nin kırmızı pelerini), ve rengi "sinyal" olarak ayırmak (Mirror's Edge kırmızı, Ori'de tehlike pembesi)** ([Game Developer: Color in Games](https://gamedeveloper.com/design/color-in-games-an-in-depth-look-at-one-of-game-design-s-most-useful-tools), [Journey](https://en.wikipedia.org/wiki/Journey_(2012_video_game))).

---

## 1. Çiçek / bitki / ağız-tuzak mekaniği olan oyunlar

### 1.1 Super Mario Bros. Wonder — Wonder Flower ve Piranha Plant
- **Ne yapıyor [Doğrulandı]:** Wonder Flower'ın rengi her dünyada farklı (mavi, camgöbeği, kırmızı-magenta, sarı, yeşil, mor, turuncu; özel dünyada gri+sarı vurgu). Tohumu alınınca gri ve yarı saydam oluyor — yani "tükenmiş" durum renk söndürmeyle gösteriliyor. Takashi Tezuka'nın aktardığına göre ekip efekt için yaklaşık 2000 fikir toplamış; basit ışınlanma "boru zaten bunu yapıyor" diye elenmiş ([Super Mario Wiki](https://www.mariowiki.com/Wonder_Flower)).
- **Piranha Plant [Doğrulandı]:** Tehlike dili sert: parlak baş, beyaz benekler, keskin dişler, ısırık dudağı; model Venus flytrap. Kırmızı ve yeşil baş yaygın; tür farkı palet kaymasıyla (siyah, açık mavi, kahverengi+turuncu benek) gösteriliyor. Zamanlama kimliğin yarısı: borudan çıkıp inme döngüsü, oyuncu yanındayken geri çekilme; Mario Kart 7'de müziğin vuruşuna baş sallama ([Super Mario Wiki](https://www.mariowiki.com/Piranha_Plant)).
- **Animasyon felsefesi [Doğrulandı]:** Nintendo geliştiricileri animasyonun ana işinin "karakterin o anki durumunu göstermek" olduğunu, boruya girmeden önceki kısa ön-hareketin sesle eşlendiğini söylüyor; düşmanlara temel davranışı değiştirmeden küçük ifadeler eklenmiş ([Ask the Developer, bölüm 2](https://www.nintendo.com/sg/interview/aqmx/02.html), [Miketendo64 özeti](https://miketendo64.com/2023/10/18/mario-devs-on-new-animations-and-movement/)). Sanatsal yön tarafında Sato'nun "Wonder efekti"ni alışılmadık görsellikle çerçevelediği not edilmiş ([Ask the Developer](https://www.nintendo.com/my/interview/aqmx/index.html)).
- **Bu oyuna aktarılacak fikirler:**
  1. Çiçeğin ana rengi sabit bir "marka" değil; **şekil + hareket + merkez nokta** kimliği taşısın, ton serbest bırakılabilir (Wonder kanıtı).
  2. "Tükenmiş/etkisiz" durumu **gri-saydam** göstermek okunur bir dil; kapalı tomurcuğu (tutunulamaz) söndürülmüş/yarı saydam çizmek bu dille uyumlu [Çıkarım].
  3. Piranha'dan: tehlike **dişli/sivri siluetle** okunur. Kapanma uyarısında yaprak uçlarının keskinleşmesi (düz → sivri) renkten bağımsız bir tehlike kanalı olabilir [Çıkarım].
- **Bulunamayan:** Wonder Flower'ın kendi açılış/animasyon süreci hakkında geliştirici beyanı (aranan kaynaklarda yok, bkz. Doğrulanamayanlar).

### 1.2 Pikmin — yaprak / tomurcuk / çiçek
- **Ne yapıyor [Doğrulandı]:** Üç olgunluk aşaması başın üstünde okunuyor: yeşil yaprak (en yavaş), yuvarlak tomurcuk (orta), çiçek (en hızlı, yaklaşık yaprağın iki katı). Tomurcuk çoğunlukla beyaz, bazı türlerde magenta/lavanta/sarı; çiçek tomurcukla aynı renk. Nektar anında çiçeğe çeviriyor ([Pikipedia: Bud](https://www.pikminwiki.com/Bud)). Dört tür renkle (kırmızı/sarı/mavi) görev ayrımı yapıyor ([Wikipedia: Pikmin](https://en.wikipedia.org/wiki/Pikmin_(video_game))).
- **Aktarım:**
  1. **Durum = şekil kademesi**: yaprak → yuvarlak → açık taç. Kapalı/açık çiçekte de aynı kademe mantığı (küçük yuvarlak = kapalı, geniş taç = açık) [Çıkarım].
  2. Durumu taşıyan şekil, oyuncunun zaten baktığı yerde (baş/merkez) dursun; çiçekte bu **merkez nokta** olabilir.

### 1.3 Plants vs. Zombies — "görünüş işlevi söyler"
- **Ne yapıyor [Doğrulandı]:** George Fan'ın GDC anlatımına göre her karakterin görünüşü işlevini işaret ediyor (Peashooter'ın büyük ağzı = mermi atar). Eğitim metinle değil eylemle; dikkat çeken ögeler yanıp sönerek veya okla gösteriliyor; ekran metni ~8 kelimeyle sınırlı ([Game Developer: GDC 2012 özeti](https://gamedeveloper.com/design/gdc-2012-10-tutorial-tips-from-i-plants-vs-zombies-i-creator-george-fan)).
- **Aktarım:** Çiçeğin "açık = tutunulabilir" bilgisi **yazı/ikon olmadan** form üzerinden okunsun; ilk karşılaşmada oyuncu kendi hareketiyle öğrensin [Çıkarım].

### 1.4 Cuphead — botanik boss, parry-pembesi
- **Ne yapıyor [Doğrulandı]:** Pembe nesneler parry'lenebilir, yani **pembe = etkileşilebilir/güvenli-faydalı** anlamına gelen kalıcı bir renk sözü. Görsel dil 1930'lar çizgi filmi; animasyonlar 24 fps elle çizilmiş, oyun 60 fps ([Wikipedia: Cuphead](https://en.wikipedia.org/wiki/Cuphead)). Stilin kaynağı Mickey ve Popeye'li deneme sahneleri, ince çizgi kalınlığı varyasyonu ([Game Developer](https://gamedeveloper.com/art/how-i-cuphead-i-nailed-the-look-and-feel-of-classic-1930s-cartoons)).
- **Kısmi:** Cagney Carnation'ın "Flowers and Trees" (1932) ve diğer 1930'lar çizgi filmlerinden esinlendiği iddiası arama özetlerinde geçti; ilgili sayfa (TV Tropes) açılmadı → "Doğrulanamayanlar".
- **Aktarım:** Bir rengi **tek bir anlama** bağlamak (Cuphead'in pembesi). Bu projede **magenta-pembe = oyuncu**; çiçeğin pembesi bu söze zarar veriyor [Çıkarım].

### 1.5 Okami — "çiçek açtırma" ödül olarak
- **Ne yapıyor [Doğrulandı]:** Celestial Brush solmuş bitkileri canlandırıyor; ağaçları çiçeklendirmek isteğe bağlı bir faaliyet ve Amaterasu'ya "Praise" kazandırıyor. Erken prototipte kurdun ardından çiçeklerin açtığı bir orman vardı. Görsel dil sumi-e (mürekkep), ukiyo-e, ahşap baskı ve suluboya karışımı ([Wikipedia: Okami](https://en.wikipedia.org/wiki/Okami)).
- **Aktarım:** Çiçeğin açılması **"yaşam geri geliyor" ödülü** olarak sunulabilir: açılırken çevreye kısa renk/parıltı yayılması [Çıkarım].

### 1.6 Flower (thatgamecompany)
- **Ne yapıyor [Doğrulandı]:** Çiçeğe yaklaşmak onu açtırıyor ve arkanızdaki taç yaprak zincirine bir yaprak ekliyor. Çiçekler ölü alana rengi geri getiriyor; çiçeğin içinden geçmek müzikle uyumlu bir çınlama çalıyor ([Wikipedia: Flower](https://en.wikipedia.org/wiki/Flower_(video_game))).
- **Aktarım:** Açılma anına **müzikal bir nota/çınlama** bağlamak; aynı nota kapanışta ters/kesik duyulursa oyuncu ritmi kulaktan da öğrenir [Çıkarım].

### 1.7 Journey, Gris, Ori, Rayman Legends, Sayonara Wild Hearts
- **Journey [Doğrulandı]:** Dünya genel olarak sönük; kumaş şeritler gri başlıyor, oyuncu şarkı söyleyince parlak kırmızıya dönüyor; gezginin kırmızı pelerini kumda öne çıkıyor. Eşarpta parlayan semboller şarjı gösteriyor. Ekip "Japon bahçesi gibi" gereksizi ayıklamış ([Wikipedia: Journey](https://en.wikipedia.org/wiki/Journey_(2012_video_game))).
- **Gris [Kısmi]:** Renksiz başlayıp ilerledikçe renk kazanan dünya, suluboya ([MCV](https://www.mcvuk.com/development/50-shades-of-gris-how-nomada-studio-created-a-game-that-evolves-visually-not-only-mechanically)) — makale renk-sinyal ayrıntısı vermiyor, sadece konsept.
- **Ori [Doğrulandı, tek cümle]:** Yeşil-mavi palete eklenen pembe vurgu tehlikeyi işaretliyor ([Game Developer: Where Colour meets Design](https://gamedeveloper.com/design/where-colour-meets-design---colour-palettes-and-game-design)). Wikipedia sayfası elle çizilmiş görünüm hedefini ve Miyazaki etkisini söylüyor ama palet ayrıntısı yok ([Wikipedia](https://en.wikipedia.org/wiki/Ori_and_the_Blind_Forest)).
- **Rayman Legends [Doğrulandı, sınırlı]:** UbiArt'ta dinamik ışık karakteri çevresine göre parlatıyor veya siluete çeviriyor; müzik bazlı seviyeler var ([Wikipedia](https://en.wikipedia.org/wiki/Rayman_Legends)).
- **Sayonara Wild Hearts [Doğrulandı, sınırlı]:** Neon görünüm; her seviye bir şarkıya bağlı, eylem zamanını gösteren ekran işaretleri bilinçli tasarlanmış ([Wikipedia](https://en.wikipedia.org/wiki/Sayonara_Wild_Hearts)).
- **Aktarım:** Journey'nin "gri → canlı renk, oyuncu etkileşimiyle" geçişi, kapalı tomurcuğun **söndürülmüş**, açılışta **renk dolması** olarak çizilmesi için doğrudan emsal [Çıkarım]. Ori'nin pembe-tehlike örneği, bu projede pembenin **oyuncu** rengi olması nedeniyle ters yönde uyarı: pembeyi çiçeğe verme.

### 1.8 Hollow Knight (dünya durumunu renkle işaretleme)
- **[Doğrulandı, sınırlı]:** Enfekte Kavşak'ta bozulmuş bölgeyi turuncu cıvık madde görünür kılıyor; düşmanlar daha ölümcül versiyonlara mutasyona uğruyor ve görünüşleri tehdit seviyesini işaretliyor. Sanat yönü sadeliği hedeflemiş ([Game Informer](https://www.gameinformer.com/2018/10/16/the-making-of-hollow-knight)).
- Gibson'ın silüet/okunurluk konuşmaları bulunamadı (bkz. Doğrulanamayanlar). Queen's Gardens'taki dikenli bitkiler dışında bitki-tuzak tasarımı hakkında kaynaklı sonuç yok.

### 1.9 Celeste (okunurluk)
- **[Doğrulandı]:** Takım piksel sanatı seçmiştir çünkü bilgiyi daha net aktarıyor ([Wikipedia: Celeste](https://en.wikipedia.org/wiki/Celeste_(video_game))). Madeline'in saç rengi dash durumunu gösteriyor ([ludo.guide: UI](https://www.ludo.guide/guide/celeste/getting-started/understanding-the-ui)). Hangi renk neyi gösteriyor sorusu için sayfa renk adı vermiyor; arama özetinde kırmızı=1 dash, mavi=0, pembe=2 deniyor ama o sayfa açılamadı (bkz. Doğrulanamayanlar).
- **Aktarım:** "Durum bilgisini karaktere/nesnenin kendisine renk olarak giydirmek" (saç rengi) ile çiçekte **merkez noktanın renk/parıltısı = "tutunabilirsin" durumu** arasında mantık benzerliği [Çıkarım].

### 1.10 Hades, Katamari, Spiritfarer, Alba, Chicory, Sable, Splatoon
- **Hades:** Jen Zee'nin silüet/kontur/rim ışığı konuşması bulunamadı; Creative Bloq ve Game Informer sayfaları ya kesikti ya da açılamadı → "Doğrulanamayanlar". Genel analiz makaleleri yetkisiz.
- **Splatoon [Doğrulandı, sınırlı]:** Takım rengi mürekkep rengi; erken aşamada tavşan karakterler (siyah/beyaz kürk) ekip ayrımını ve parlak mürekkeple kontrastı kolaylaştırdığı için düşünülmüş ([Wikipedia](https://en.wikipedia.org/wiki/Splatoon_(video_game))). Zıt renk çiftleri (mor/yeşil, mavi/turuncu) ve kırmızı-yeşilden kaçınma ise Game Developer'dan ([Where Colour meets Design](https://gamedeveloper.com/design/where-colour-meets-design---colour-palettes-and-game-design)).
- **Katamari, Spiritfarer, Alba, Chicory, Sable:** Bu araştırmada açılıp doğrulanmış kaynak yok; referans olarak sayılmadı.

---

## 2. Zamanlı / ritmik platform nesneleri ve uyarı dili (telegraphing)

### 2.1 Telegraph'ın genel anatomisi
- **Game Developer (telegraph) [Doğrulandı]:** Telegraph = oyuncuya "ne olacak" bilgisini önceden vermek; aşamalar: hazırlık (setup), şarj, bırakma. Kanallar: animasyon (uzun windup), görsel efekt (parçacık birikimi, parlama), ses (şarj sesi), seslendirme, kontrolcü titreşimi. Makale "küçük bir gecikme" öneriyor, sayı vermiyor ([Game Developer](https://gamedeveloper.com/design/enemy-attacks-and-telegraphing)).
- **Dark Souls 3 analizi [Doğrulandı]:** Windup + vuruş birlikte en az ~340 ms; bitiş duruşu ~170 ms (bait ise ~240 ms); algı ~100 ms; vuruş penceresi 2-4 kare. Silüet ve duruş tehdidin ağırlığını söylüyor ([Game Developer](https://www.gamedeveloper.com/game-platforms/anatomy-of-an-enemy-attack-in-dark-souls-3)).
- **ARPG okunurluğu [Doğrulandı]:** Renk bir sonucu işaret edebilir (kırmızı parlayan = bloklanamaz) ve anlam tüm düşmanlarda sabit kalmalı; Sekiro'nun tehlike sembolü + ses birlikte çalışıyor ([Game Developer](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs)).
- **Aktarım:** Çiçeğin uyarı fazı **birden fazla kanalda** (şekil + hareket + ses + haptik) verilmeli; proje zaten haptik altyapısına sahip (CLAUDE.md: `HapticManager`, `*VibrationSettingsSO`) → kapanma uyarısına titreşim profili eklenebilir [Çıkarım].

### 2.2 Zamanlı platform örnekleri
| Oyun | Uyarı dili | Kaynak |
|---|---|---|
| **Celeste crumble bloğu** | Etkileşimle başlar; üstten inince 0,6 sn (36 kare), yandan tutununca 1 sn'de yok olur; 2 sn sonra geri gelir. Uyarı fazı olarak sallanma bilgisi bu kaynakta yok. | [Everest/modlama dokümantasyonu](https://github-wiki-see.page/m/coloursofnoise/Resources/wiki/Entity-and-Trigger-Documentation) [Kısmi: resmi değil, mod topluluğu] |
| **Super Mario Galaxy 2 kaybolan platform** | Platformun yeri **hayalet (şeffaf) bir kontur** ile gösterilir; Mario o noktaya varınca **mavi yanıp** yok olur; süre sayısı verilmemiş. | [Super Mario Wiki](https://www.mariowiki.com/Disappearing_platform) [Doğrulandı] |
| **Spelunky** | Ok tuzakları "önceden fark edilebilir"; ceza adil sayılıyor çünkü her tehlike dünyada görünür. Karanlık XBLA seviyelerinde tuzaklar görünmediği için "ucuz". | [Game Developer: Fairness & Discovery](https://gamedeveloper.com/design/fairness-discovery-spelunky), [analiz pt.2](https://www.gamedeveloper.com/design/a-spelunky-game-design-analysis-pt-2) [Doğrulandı, ama tuzak zamanlaması yok] |
| **Mario Kart 7 Piranha** | Müzik vuruşuna baş sallama = ritim duyulabilir bir tehlike | [Super Mario Wiki](https://www.mariowiki.com/Piranha_Plant) [Doğrulandı] |

- Super Meat Boy, Geometry Dash, Hollow Knight ve Cuphead için **zamanlı nesne uyarı dili** ile ilgili açılıp doğrulanmış kaynak bulunamadı (bkz. Doğrulanamayanlar).

### 2.3 Uyarı nasıl okunur yapılır? (kanıtlanan ve çıkarılan)
- **Hareket:** Windup/silüet duruşu (Game Developer, DS3) → çiçekte titreme ve yaprak ucu çekilmesi [Çıkarım].
- **Renk:** Sekiro sembolü ve anlam-sabit kırmızı (ARPG makalesi) → çiçekte uyarı fazı için **tek ve sabit bir renk anlamı** [Çıkarım].
- **Şekil:** Piranha dişleri → uyarıda sivrilen uçlar [Çıkarım].
- **Ses:** Şarj sesi, Sekiro ses işareti → çiçekte hızlanan tik/çınlama [Çıkarım].
- **Yer işareti:** Galaxy 2 hayalet kontur → kapalı çiçeğin "burada bir çiçek var" kontur/gölgesi [Çıkarım].

---

## 3. Animasyon prensipleri ve gerçek bitki hareketleri

### 3.1 Prensipler
- **[Doğrulandı]** Disney'in 12 prensibinden ilgili olanlar ([Wikipedia](https://en.wikipedia.org/wiki/Twelve_basic_principles_of_animation)): beklenti (hareketi önceden hazırlayan zıt hareket), squash & stretch (hacim korunur), follow-through ve overlapping (gevşek parçalar ana gövde durunca da hareket eder, farklı zamanlarda başlar-durur), slow-in/slow-out, abartı, zamanlama (kare sayısı hızı ve duyguyu belirler).
- **Çiçek açılışına uygulama [Çıkarım]:**
  1. **Beklenti:** tomurcuk kapanırken **hafif içe büzülür** (küçülür) sonra açılır; açılış öncesi mikro şişme.
  2. **Overshoot:** yapraklar hedef açıdan ~%10-15 fazla açılıp geri yerleşir.
  3. **Follow-through/overlap:** yapraklar sırayla (stagger) açılır; merkez nokta en son pop yapar → "açık" işaretinin zamanlaması sonuncu.
  4. **Slow-in/out:** açılış yumuşak (ease-out), kapanış snap (neredeyse lineer, çok kısa).
  5. **Squash/stretch:** pop sırasında merkez nokta dikey uzayıp yatay daralır, sonra yerleşir.

### 3.2 Gerçek hareketler (hepsi açılıp okundu)
- **Venus flytrap [Doğrulandı]:** Kapanma ~0,1 sn; tuzak, iki kararlı şekil (dışbükey/içbükey) arasında atlıyor ("lens gibi ters yüz"); su basıncı değişimi tek başına çok yavaş, hızı yaprak geometrisindeki depolanmış elastik enerji sağlıyor; tetikleyici iç yüzeydeki tüycükler ([Harvard Magazine](https://www.harvardmagazine.com/2005/05/leaves-that-lunch-html)). 
  - **Aktarım:** Uyarı fazı = "enerji depolama" (artan titreme + yavaş içe eğilme), kapanış = **tek hamle ters yüz** (petal eğriliğinin işaret değiştirmesi). Shader'da kapanma parametresini iki aşamaya bölmek: yavaş yükleme + çok kısa (≈0,1 sn) snap [Çıkarım].
- **Mimoza (Mimosa pudica) [Doğrulandı]:** Dokunuşla yaprakçık çiftleri **kitap kapanır gibi** birbirine katlanıyor; hareketi tabandaki pulvini adı verilen şişkinlikler sağlıyor (iyon/su kayması); hareket geri dönüşlü ([Science News](https://www.sciencenews.org/?p=3121461)).
  - **Aktarım:** Kapanış tek seferde değil **yaprak çiftleri ardışık** (dıştan içe veya iki yandan merkeze) katlanabilir; ama bu gerçek zamanlı ~0,1-0,2 sn'de bitmeli [Çıkarım].
- **Nyctinasty [Doğrulandı]:** Karanlıkta yaprak/taç hareketi sirkadiyen saatle yönetilir; laleler akşam kapanır; fitokromlar ışığı algılar, pulvinus'ta potasyum akışı su kaydırır ([Wikipedia](https://en.wikipedia.org/wiki/Nyctinasty)). 
  - **Aktarım:** "Akşam kapanması" yavaş bir kapanma; oyun içi **uyarı fazını bir "alacakaranlık"** gibi (ton koyulaşması + ağır eğilme) tasarlamak mümkün, ama hızlı snap ayrı katman olsun [Çıkarım].
- **Zambak açılışı [Doğrulandı]:** Harvard araştırması, açılışı **yaprak kenarlarındaki** hızlı büyüme/kıvrılmanın sürdüğünü, tomurcuğun kritik gerilime gelene kadar çanak yapraklarla kilitli kaldığını ve sonra patlayarak açıldığını gösteriyor ([Harvard Gazette](https://news.harvard.edu/gazette/story/2011/03/how-the-lily-blooms/)).
  - **Aktarım:** "Kilitli → birikim → ani açılma" ritmi; açılışın önce **kenarlardan dışa kıvrılması** (orta damar değil) görsel olarak farklı bir silüet yaratır [Çıkarım].
- **Time-lapse [Doğrulandı]:** F. Percy Smith 1910 "The Birth of a Flower"; John Ott'un çiçek açma görüntüleri Disney'in "Secrets of Life" (1956) filmine girdi; time-lapse geranyum'un iki saatlik açılışını birkaç saniyeye sıkıştırır; Ott bitkilerin müziğe eşlenmiş hareket etmesini de denemiş ([Wikipedia: Time-lapse](https://en.wikipedia.org/wiki/Time-lapse_photography)).
  - **Aktarım:** Zaman-lapse'in "sabit hızda değil, **ivmeli** açılan" hissi için ease-out; müzikle eşlemek için açılış-kapanışı BPM'e kilitlemek (ritim oyunlarıyla paralel) [Çıkarım].
- **Disney "Flowers and Trees" (1932) [Doğrulandı, sınırlı]:** İlk ticari üç şeritli Technicolor film, ilk Akademi Ödüllü çizgi film; çiçekler, mantarlar ve ağaçlar insan gibi egzersiz yapıyor ([Wikipedia](https://en.wikipedia.org/wiki/Flowers_and_Trees)). Animasyon tekniği ayrıntısı bu kaynakta yok.
- **Ghibli/Pixar bitki animasyonları:** Totoro'nun tohum-büyüme sahnesi için sahnenin içeriği doğrulanmadı (arama özetinde sadece anlatım), zamanlama/anticipation analizi bulunamadı. Pixar için kaynak açılmadı. **Doğrulanamayanlar'a bak.**
- **Origami "bloom" [Doğrulandı]:** BYU'nun 2025 "bloom patterns" katlaması merkezden **dönerek** radyal açılıyor, düz katlanıyor ve açılırken ara bir **küre/kase benzeri** şekilden geçiyor ([BYU News](https://news.byu.edu/intellect/the-next-chapter-in-byus-origami-engineering-student-discovered-patterns-unfold-like-blooming-flowers-have-major-applications)).
  - **Aktarım:** Açılışta yaprakların hafif **döne döne** açılması ve yarı-açık halin bir "kase" silüeti olması; bu hareket bir desen değil, geçiş animasyonu olduğundan "spiral desen" yasağıyla çakışmaz (yine de dönüş miktarı küçük tutulmalı) [Çıkarım].
- **Pop-up / kirigami [Kısmi]:** Kirigami = kesilip katlanan kâğıt, yapıştırıcısız, düz sayfadan öne çıkan 3B ([Wikipedia](https://en.wikipedia.org/wiki/Kirigami)). Pop-up kitap sayfası "snap" mekaniğini açıklamıyor ([Wikipedia](https://en.wikipedia.org/wiki/Pop-up_book)).

---

## 4. Güçlü karakter rengine sahip oyunlarda dünya–karakter ayrımı

| Oyun | Ne yapıyor | Durum |
|---|---|---|
| **Mario** | İlk tasarımda kırmızı tulum + mavi gömlek, arka plandan ayırma ve kol hareketini gösterme gerekçesiyle; kasket saç çizmeyi önlemek için. | [Doğrulandı: Wikipedia](https://en.wikipedia.org/wiki/Mario) |
| **Sonic** | Mavi, Sega logosuyla eşlemek için; kırmızı ayakkabılar kontrast için ve hızlı koşan bacakları vurgulamak için. | [Doğrulandı: Wikipedia](https://en.wikipedia.org/wiki/Sonic_the_Hedgehog_(character)) |
| **Kirby** | Pembe, Sakurai'nin niyeti; Miyamoto sarı düşünmüş. Yuvarlak, basit form çizim kolaylığı için. 3B'de küresel gövde yüzü okunmaz kıldığı için Forgotten Land'e hafif otomatik nişan eklenmiş. Pembenin çevreyle kontrastı hakkında kaynak yok. | [Doğrulandı, sınırlı: Wikipedia](https://en.wikipedia.org/wiki/Kirby_(character)) |
| **Journey** | Dünya sönük/kum rengi; yolcunun kırmızı pelerini ve kırmızıya dönen kumaşlar öne çıkıyor. | [Doğrulandı: Wikipedia](https://en.wikipedia.org/wiki/Journey_(2012_video_game)) |
| **Mirror's Edge** | Dünya neredeyse renksiz/beyaz; **kırmızı "runner vision"** ilerlemeye yardım eden nesneleri işaretliyor; renk sınırlı olduğu için görünür. | [Doğrulandı: Game Developer röportajı](https://gamedeveloper.com/design/the-philosophy-of-faith-a-mirror-s-edge-interview) |
| **Celeste** | Oyun renkleri çoğunlukla soğuk-mavi, sıcak vurgu yüz ve kırmızı saçta; (fan sanatı makalesi: gri tonlama kontrast testi tavsiyesi). | [Kısmi: 80.lv fan sanatı röportajı](https://80.lv/articles/celeste-fan-art-highlighting-character-with-texturing-pose) |
| **Rayman Legends** | Dinamik ışık karakteri çevresine göre parlatıyor veya siluete çeviriyor. | [Doğrulandı: Wikipedia](https://en.wikipedia.org/wiki/Rayman_Legends) |
| **Hades/Zagreus** | Kaynak bulunamadı. | Doğrulanamadı |

**Genel ilke [Doğrulandı]:** Sahnede öncelik sırası oyuncu > düşman > etkileşimli nesne > arka plan; değer, doygunluk ve hue ile ayrıştırılır ([Game Developer: Color in Games](https://gamedeveloper.com/design/color-in-games-an-in-depth-look-at-one-of-game-design-s-most-useful-tools)).

**Sonuçlar (çıkarım):**
- Güçlü örneklerin çoğu **iki-üç eksende birden** ayırıyor (renk + değer + sönük arka plan); "karakter rengi dünyada hiç kullanılmıyor" kuralı kaynaklarda **açıkça** yazılı değil, ama Journey ve Mirror's Edge pratikte bunu yapıyor.
- Bu projede magenta-pembe oyuncuya ait; çiçeğin pastel pembesi hem hue hem değer olarak karaktere yakın. **Önerilen yaklaşım:** çiçeğin ana hue'sunu pembe/magenta aralığından uzaklaştırmak (krem, sarı, camgöbeği, açık mor-mavi gibi), değeri (açık/koyu) karakterden farklı tutmak, koyu dış çizgiyi korumak.

---

## 5. Kendi keşfettiğim referanslar (beklenmedik çiçek/bitki dili)

### 5.1 Harvard "Glass Flowers" (Blaschka cam çiçekleri)
- **Ne yapıyor [Doğrulandı]:** Leopold ve Rudolf Blaschka 1887-1936 arasında cam, tel, yapıştırıcı, organik malzeme ve boya/emaye ile botanik modeller üretmiş; lampwork (alevde cam çekme/kesme/üfleme). Modeller o kadar gerçekçi ki ziyaretçiler gerçek bitki sanıyor; renkleri sabit kalıyor ([Wikipedia: Glass Flowers](https://en.wikipedia.org/wiki/Glass_Flowers)).
- **Fikirler:** (1) Kapalı tomurcuğu **buzlu cam**, açık çiçeği **içten aydınlanan şeffaf cam** yapraklar olarak çizmek; (2) cam gibi **kenarlarda parlak kenar vurgusu + koyu dış çizgi**; (3) snap kapanışta **cam çınlaması** sesi [Çıkarım].

### 5.2 Wagashi / nerikiri (Japon yenilebilir çiçekler)
- **Ne yapıyor [Doğrulandı]:** Nerikiri, beyaz fasulye ezmesi ve şekerle yoğrulan, mevsim çiçeklerini (sabah sefası, krizantem, wisteria) ve doğayı şekil ve renkle çağrıştıran tatlılar; Kyoto'da Edo döneminde gelişmiş ([Wikipedia: Wagashi](https://en.wikipedia.org/wiki/Wagashi)). Hanabiramochi gibi düz-kırmızı-beyaz yaprak biçimli mochi de var.
- **Fikirler:** Karakter bayram şekeri olduğuna göre çiçek **şekerleme/lokum yaprakları** gibi: pürüzsüz, yumuşak köşeli, **tek renkli iki tonlu** (kırmızı-beyaz mochi dili); yüzeyde yumuşak parlaklık, ince pudra şekeri tozu efekti açılışta [Çıkarım]. Pastel pembe yerine şeker ambalajı canlılığı (krem + turkuaz).

### 5.3 Hoberman küresi / makaslı mekanizmalar
- **Ne yapıyor [Doğrulandı]:** Chuck Hoberman'ın kinetik yapısı, makas eklemlerle genişleyip küçülür; bir eklem hareket edince hepsi hareket eder; Estonya'daki büyük model müzik, ışık ve efektle koreografilidir ([Wikipedia: Hoberman sphere](https://en.wikipedia.org/wiki/Hoberman_sphere)).
- **Fikirler:** Çiçek yapraklarını **mekanik bir diyafram/makas** gibi senkron açılıp kapanan paneller olarak kurmak: açık = tam genişlemiş, kapalı = çember içine toplanmış. Proje zaten bir "iris geçişi" materyaline sahip (`M_IrisTransition.mat`) → aynı görsel dil [Çıkarım].

### 5.4 Tengami (pop-up kâğıt oyunu)
- **Ne yapıyor [Doğrulandı, sınırlı]:** Eski Japonya'da geçen, geleneksel Japon resmini 3B pop-up kitap görünümüyle birleştiren oyun; Nyamyam ([Wikipedia: Tengami](https://en.wikipedia.org/wiki/Tengami)). Katlama mekaniğinin ayrıntısı sayfada yok.
- **Fikirler:** Kapalı çiçek = **düz katlanmış kâğıt**, açık = **katmanlı pop-up** (yapraklar derinlik kazanır, aralarında yumuşak gölge) [Çıkarım]; kirigami'nin yapıştırıcısız, kesilip katlanan yapısı çiçek yapraklarının "kesik uçlu" silüeti için ilham.

### 5.5 Yoshi's Crafted World / Tearaway / Kirby Epic Yarn (malzemeyle çizilen dünya)
- **[Kısmi]:** Crafted World'de ekip önce Epic Yarn gibi iplik çizgiyle başlamış, sonra karton kesiklere kaymış; Tearaway'in kâğıt dünyası, oyuncunun gerçek dünyayla kesişimini istemekten çıkmış; Epic Yarn'da hedef yumuşaklık ve doku hissi ([Nintendo Everything](https://nintendoeverything.com/yoshis-crafted-world-devs-on-the-art-style-kindergarten-inspiration-flip-side-gimmick-more), [Game Informer](https://gameinformer.com/b/news/archive/2013/03/28/media-molecule-talks-about-crafting-the-world-of-tearaway.aspx)) — **bu üç sayfa açılmadı; yalnızca arama özetiyle geçiyor**, referans olarak sayılmadı. Bkz. Doğrulanamayanlar.

---

## 6. Çiçek node'u için okunurluk kuralları

1. **Her durum en az iki bağımsız kanalda ayrışsın (şekil + hareket/renk).** Kırmızı-yeşil gibi renge tek başına güvenilmemeli; şekil, desen veya yazı yedek olmalı. *Kaynak:* [Color in Games](https://gamedeveloper.com/design/color-in-games-an-in-depth-look-at-one-of-game-design-s-most-useful-tools).
2. **Durum silüetle okunsun: kapalı = küçük yuvarlak tomurcuk, açık = geniş taç.** *Kaynak:* Pikmin yaprak/tomurcuk/çiçek kademeleri ([Pikipedia](https://www.pikminwiki.com/Bud)); DS3'te silüet ve duruşun tehdidi taşıması ([Game Developer](https://www.gamedeveloper.com/game-platforms/anatomy-of-an-enemy-attack-in-dark-souls-3)).
3. **Bir renk = bir anlam.** Uyarı fazının rengi sadece uyarıda, açık fazın rengi sadece açıkta kullanılsın. *Kaynak:* anlam-sabit renk ilkesi ([ARPG okunurluğu](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs)), Cuphead pembesi ([Wikipedia](https://en.wikipedia.org/wiki/Cuphead)).
4. **Magenta-pembe oyuncuya ayrılsın; çiçekte hue ve değer olarak karakterden uzak durulsun.** Koyu dış çizgi korunsun. *Kaynak:* hue/değer/doygunluk hiyerarşisi ([Color in Games](https://gamedeveloper.com/design/color-in-games-an-in-depth-look-at-one-of-game-design-s-most-useful-tools)); sönük dünya + tek parlak karakter (Journey); Wonder Flower'ın dünyaya göre ton değiştirmesi (kimlik rengi değil). Özel tavsiye [Çıkarım].
5. **Uyarı fazı en az ~0,5–0,6 sn sürsün ve ilerlesin (tekrar eden titreme).** *Kaynak:* Celeste crumble 0,6 sn ([Everest dok.](https://github-wiki-see.page/m/coloursofnoise/Resources/wiki/Entity-and-Trigger-Documentation)); DS3 windup+vuruş ≥ ~340 ms, algı ~100 ms. Tam süre **[Çıkarım]**, playtest ister.
6. **Kapanış çok kısa (≈0,1 sn), uyarı uzun: "enerji depolanır, sonra snap".** *Kaynak:* Venus flytrap ~0,1 sn, bistable snap ([Harvard Magazine](https://www.harvardmagazine.com/2005/05/leaves-that-lunch-html)).
7. **Açılışta anticipation + overshoot + sıralı yaprak; merkez nokta en son pop yapıp "tutunulabilir" işareti olsun.** *Kaynak:* animasyon prensipleri ([Wikipedia](https://en.wikipedia.org/wiki/Twelve_basic_principles_of_animation)); merkezde durum işareti için Celeste saç rengi emsali ([ludo.guide](https://www.ludo.guide/guide/celeste/getting-started/understanding-the-ui)). Uygulama **[Çıkarım]**.
8. **Kapalı çiçeğin yeri de görünür kalsın (söndürülmüş/hayalet kontur).** *Kaynak:* Galaxy 2 kaybolan platformun şeffaf kontur emsali ([Super Mario Wiki](https://www.mariowiki.com/Disappearing_platform)); Wonder Flower'ın tükenince gri-saydam olması. Uygulama **[Çıkarım]**.
9. **Uyarıyı ses ve haptikle de ver.** *Kaynak:* telegraph kanalları listesi: ses, kontrolcü titreşimi ([Game Developer](https://gamedeveloper.com/design/enemy-attacks-and-telegraphing)); Sekiro'da sembol + ses ([ARPG makalesi](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs)). Haptik için projenin SO deseni kullanılabilir **[Çıkarım]**.
10. **Gri tonlamada üç durum (kapalı/açık/uyarı) birbirinden ayrılabilmeli.** *Kaynak:* gri tonlama kontrast testi tavsiyesi ([80.lv, fan sanatı röportajı](https://80.lv/articles/celeste-fan-art-highlighting-character-with-texturing-pose), düşük otorite). Uygulama **[Çıkarım]**.

---

## 7. Tuhaf ama denemeye değer 5 fikir

Hepsi **[Çıkarım]**; dayanak kaynaklar parantezde. Bauhaus, op-art, halftone, folk, spiral/hipnotik desen önerilmedi; piksel sanat yok.

1. **Şeker poşeti tomurcuk:** Kapalı çiçek, karakterin iki ucu büzgülü poşetinin **soluk/şeffaf ikizi** (büzgülü kapalı uç); açılışta büzgü çözülür (unwrap) ve poşet yaprağa dönüşür. Renk karakterden farklı (krem/turkuaz selofan). *Risk:* karakterle karışabilir; bu yüzden ton ve değer ayrımı şart. (Karakter bağlamı; Journey/Mirror's Edge renk ayrımı ilkesi.)
2. **Jöle-cam yaprak:** Yapraklar yarı saydam jöle/cam; kapalıyken buzlu, açıkken içten aydınlanmış, kapanışta parlak kenar çakması ve çınlama. (Blaschka cam çiçekler, wagashi'nin yumuşak şekerleme formu.)
3. **Diyafram çiçeği:** Yapraklar kamera iris bıçakları gibi makasla açılır/kapanır; kapanma = iris kapanır, merkez nokta son anda kaybolan küçük delik. Projede iris geçişi zaten var. (Hoberman makas mekanizması.)
4. **Pop-up kâğıt katman çiçeği:** Kapalıyken düz katlı kâğıt tomurcuk; açılışta yapraklar farklı derinliklerde yukarı kalkar, katmanlar arası yumuşak gölge; kapanışta düz sayfaya "çarpıp" yatar. (Kirigami ve Tengami; BYU bloom'un dönerek açılma hareketi.)
5. **Diş açan çiçek (Piranha-vari uyarı):** Güvenli fazda yapraklar yuvarlak uçlu; uyarı fazında uçlar **sivrilir/dişlenir**, kapanışta karşılıklı iki yaprak ağız gibi kenetlenir (Venus flytrap'in çift kararlı snap'i + Piranha Plant'in diş dili). Tehlikeyi renkten bağımsız, şekille söyler.

---

## 8. Doğrulanamayanlar

Aşağıdakiler ya arama sonucunda göründü ama sayfa açılamadı/okunamadı, ya da hiç bulunamadı. **Referans olarak sayılmadı.**

- **Wonder Flower'ın animasyon/sanat süreci:** Nintendo "Ask the Developer" bölümleri açıldı (index, 02, 04) ama çiçeğin tasarımı veya animasyonu hakkında geliştirici beyanı yok.
- **Celeste:** Saç renklerinin tam eşlemesi (kırmızı/mavi/pembe) yalnızca arama özetinde; Fandom sayfası 403 verdi, ludo.guide sayfası renk adı vermedi. Pedro Medeiros/Amora Bettany'den birincil renk-okunurluk açıklaması bulunamadı. Crumble bloğunda **sallanma uyarısı** olduğu doğrulanmadı.
- **Cuphead / Cagney Carnation:** Fandom ve GameSpot sayfaları 402/403 verdi; Flowers and Trees etkisi yalnızca arama özetinden. Cagney'nin fazları ve pembe-parry nesneleri çiçekle nasıl birleşiyor açılamadı.
- **Hades / Jen Zee (okunurluk, kontur, rim ışığı):** Creative Bloq sayfası kesildi, diğeri 404; Game Informer videosu içeriği okunamadı.
- **Hollow Knight (Ari Gibson, silüet/tehlike renkleri), Super Meat Boy, Geometry Dash, Spelunky tuzak görsel uyarısı:** ilgili birincil kaynak bulunamadı veya erişilemedi (Geometry Dash TV Tropes sayfası 403, Spelunky makaleleri uyarı dili değil adalet konusunda).
- **Kirby'nin pembesinin çevreyle kontrastı / Forgotten Land'in pembe çevre tasarımı:** hiçbir kaynakta yok. NintendoLife 1993 röportajı sayfası 402 verdi.
- **Pixar/Ghibli bitki animasyonları (Totoro tohum-büyüme sahnesi dahil):** sahne anlatımı yalnızca arama özetinde; zamanlama/anticipation analizi bulunamadı.
- **Pikmin tomurcuk/çiçek aşamalarının tasarım gerekçesi (Miyamoto):** yalnızca mekanik ve görünüm doğrulandı; gerekçe yok.
- **Splatoon takım renklerinin okunurluk gerekçesi:** Wikipedia anlatmıyor; "zıt renk çiftleri" bilgisi Game Developer makalesinden.
- **Yoshi's Crafted World / Tearaway / Epic Yarn:** arama özetinde var, sayfalar açılmadı.
- **Katamari, Spiritfarer, Alba, Chicory, Sable, Hollow Knight'ın bitki-tuzakları, Rayman'in bitki dili:** araştırılmadı/doğrulanmadı.
- **Motion-graphics "bloom reveal" örnekleri (Kurzgesagt vb.):** bu çalışmada kaynak bulunmadı.
- **BYU bloom origami'nin hakem denetimli makalesi** (Proc. R. Soc. A, 2025): doğrudan açılmadı; bilgi BYU News sayfasından.
- **Süre önerileri (0,5–0,6 sn uyarı, ~0,1 sn snap, overshoot yüzdeleri):** bunlar kaynaklardaki sayılardan türetilmiş **tahminlerdir**; Game feel insana aittir (CLAUDE.md kural 4), kullanıcı gamepad ile test etmeli.

---

## 9. Yöntem notu

- Araçlar: WebSearch (arama) + WebFetch (sayfa açma). WebSearch sonuçları özet döndürüyor; bu özetlerdeki iddiaları **yalnızca sayfayı WebFetch ile açabildiysem** doğrulanmış saydım. Açılamayan sayfalar (403/402/404/yönlendirme) "Doğrulanamayanlar"da.
- WebFetch yanıtları küçük bir model tarafından özetlendiği için ayrıntılı rakamlar (örn. 340 ms, 0,6 sn) ilgili sayfaya göre aktarıldı; birincil kaynak (GDC videosu, oyun kodu) ile çapraz doğrulama yapılmadı.
- Wikipedia sayfaları genel bilgi için kullanıldı; tasarım gerekçesi gibi iddialarda otorite düşük kabul edildi ve "sınırlı" etiketlendi.
- "Aktarılacak fikirler" ve "kurallar" bölümlerinde **[Çıkarım]** etiketi, kaynağın yalnızca emsal olduğunu, uygulamanın benim yorumum olduğunu belirtir.
- Projede tüketilmiş çizgiler (Bauhaus, op-art, halftone, folk, spiral/hipnotik desen) ve piksel sanat önerilmedi. Kod ve sahneye dokunulmadı; yalnızca bu dosya yazıldı (`A-gelenek.md` gibi diğer dosyalara dokunulmadı).
