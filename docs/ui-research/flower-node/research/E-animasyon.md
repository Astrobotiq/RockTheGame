# E - Çiçek açılış / kapanış animasyonu teknikleri

Tarih: 2026-11-22. Kapsam: zamanlı çiçek node'unun (K4 Kâğıt Pasiflora) açılış, kapanış (snap), uyarı ve anticipation hissi. Kod, shader ve sahneye dokunulmadı; yalnızca okundu.

Etiketler:
- **[Doğrulandı]** sayfa açılıp okundu, bilgi o sayfadan.
- **[Çıkarım]** kaynaklardan ve projenin kodundan benim vardığım yorum; kanıt değil.
- **[Hesap]** projedeki `UiSpring` ve `FlowerBloomMath` formüllerini birebir taklit eden sayısal simülasyon (240 Hz adım). Kodda yazan sayılardan türetildi, oyunda ölçülmedi.
- Tüm sayısal öneriler **başlangıç tahminidir**; hareket hissi geliştiricinin kararıdır, her değişiklik gamepad ile denenmelidir.

Projede tüketilmiş çizgiler (Bauhaus, op-art, halftone, genel "folk") hiçbir öneride kullanılmadı.

---

## 0. Kısa özet

1. Mevcut animasyonun en büyük sorunu "yapılmıyor" değil, **görünmüyor**: anticipation, ipucu (hint) ve yaprak gecikmesi `open` ekseninde tanımlı, ama yapraklar `open`'ı smoothstep ile alıyor ve yay çok hızlı; sonuç alt-algı eşiğinde (bölüm 5).
2. Uyarı halkası kalkınca uyarının taşıyıcısı geriye kalan kanallar olur; bunların yapraklardaki etkisi bugün birkaç derece. Uyarıyı **şekil** (yaprak yaprak katlanma), **hareket** (başlangıç darbesi) ve **ses/titreşim** kanallarına dağıtmak halkanın yerini tutar (bölüm 4).
3. Kapanış 3 karede bitiyor ve darbe hissi yok; uyarı fazı snap ile aynı yöne gittiği için klasik anlamda wind-up değil (bölüm 5, madde 7).
4. Mario Wonder, boruları/platformları/düşmanları ritme oturtuyor; çiçeğin zamanlı döngüsü için ritim bir telegraph kanalı olabilir (bölüm 2 ve 4).

---

## 1. Animasyon prensipleri ve çiçek açılışı

### 1.1 Doğrulanmış kaynaklar

| # | Kaynak | Ne için kullanıldı |
|---|---|---|
| K1 | [Twelve basic principles of animation (Wikipedia)](https://en.wikipedia.org/wiki/Twelve_basic_principles_of_animation), Thomas & Johnston, *The Illusion of Life* (1981) özeti | Prensip tanımları |
| K2 | [Figma Help: offset / stagger](https://help.figma.com/hc/en-us/articles/41239278373271) | Gecikme süreleri, örtüşme |
| K3 | [SVGator: Offset & delay in motion design](https://www.svgator.com/blog/offset-delay-motion-design/) | Tipik offset aralığı |
| K4 | [NN/g: Executing UX Animations](https://www.nngroup.com/articles/animation-duration/) | Süre ve easing aralıkları |
| K5 | [Game Developer: Instant Game Feel - Springs Explained (Chad Cable, 2022)](https://gamedeveloper.com/game-platforms/instant-game-feel---springs-explained) | Yayla game feel |
| K6 | [Ori and the Will of the Wisps animasyonu (RPS, orithegame.com)](https://www.orithegame.com/rockpapershotgun-how-animation-powers-ori-and-the-will-of-the-wisps/) | Squash/stretch ve ikincil hareket |
| K7 | [Game Developer: Ori ve Will of the Wisps Q&A](https://www.gamedeveloper.com/design/q-a-designing-the-gorgeous-metroidvania-i-ori-and-the-will-of-the-wisps-i-) | Oyuncuya tepki veren çevre nesneleri |
| K8 | [Harvard Gazette: How the lily blooms (2011)](https://news.harvard.edu/gazette/story/2011/03/how-the-lily-blooms/) ve [Liang & Mahadevan, PNAS 2011 (PMC3078366)](https://pmc.ncbi.nlm.nih.gov/articles/PMC3078366) | Gerçek çiçek açılışı |
| K9 | [Nyctinasty (Wikipedia)](https://en.wikipedia.org/wiki/Nyctinasty) | Nastik bitki hareketi |
| K10 | [BYU: origami "bloom" desenleri](https://news.byu.edu/intellect/the-next-chapter-in-byus-origami-engineering-student-discovered-patterns-unfold-like-blooming-flowers-have-major-applications), [Kirigami](https://en.wikipedia.org/wiki/Kirigami), [Pop-up kitap](https://en.wikipedia.org/wiki/Pop-up_book) | Kâğıt açılışı |

### 1.2 Kaynaktan doğrulananlar

**Prensipler (K1).** Anticipation: ana hareketten önce küçük bir hazırlık hareketi (diz bükmek) seyirciyi hazırlar ve inandırıcılığı artırır. Squash & stretch: hızlanma/çarpma anında şekil bozulur, hacim sabit tutulursa uzayan şey incelir; ağırlık ve esneklik hissi verir. Follow-through ve overlapping action: gevşek parçalar (saç, kuyruk) ana gövde durduktan sonra da hareket eder ve yerleşir; farklı parçalar farklı zamanlamayla hareket eder, atalet hissi buradan gelir. Slow in / slow out: hareketin başı ve sonu yumuşak. Arcs: doğal hareket yaya yakın izler. Secondary action: ana hareketi destekleyen ama dikkati çalmayan küçük hareketler. Timing: hafif nesne hızlı, ağır nesne yavaş tepki verir. Exaggeration: stile uygun abartı.

**Gecikme / örtüşme (K2, K3).** Aynı gruptaki parçalar arasında tipik aralık 50-100 ms; 150 ms'nin üstü yavaş (bir "yürüyüş" gibi) hissedilir; toplam dizi ~2 sn altında. Aralık, parçanın süresinden kısaysa parçalar örtüşür ve akış (dalga) oluşur; yakınsa "marş" gibi görünür. Lider parça gecikmesiz başlar.

**Süre ve easing (K4).** UI animasyonu çoğunlukla 100-500 ms; küçük geri bildirim ~100 ms, büyük değişim 200-300 ms, 400 ms üst sınır, 500 ms'den sonra "sürükleniyor" hissi. Giren öğede ease-out varsayılan (hızlı başlar, yavaş yerleşir). Sık görülen animasyon kısa ve ince olmalı. (Bu UI için; oyun içi nesne farklı olabilir, ama referans aralığı verir.)

**Yaylar (K5).** Spring bir değeri hedefte tutar, aradaki ayrıntıyı kendisi çözer; damping 1'de aşmadan oturur, 0'da sonsuz zıplar; frequency tepkinin sertliği. Bir yayın değeri -1..1 / 0..1'e normalize edilip ölçek, alfa, konum için yeniden kullanılabilir; yaya hız "dürtmesi" (nudge) eklemek yeni kod yazmadan zıplama verir; squash/stretch için ikinci bir yay kullanılabilir. Tune ederken damping ve frequency'yi teker teker değiştirmek öneriliyor.

**Ori (K6, K7).** Yönetmen Thomas Mahler squash & stretch'i savunuyor: kolu %150'ye uzatıp geri çekmek mocap'in vermediği "vuruş" katıyor. Ori'nin kulak/kuyruğu ayrı bir fizik katmanıyla gövdenin ivmesini takip ediyor (follow-through); Mahler sprite tabanlı sistemde bunun olamayacağını söylüyor. Ekip "yüzlerce" çevre nesnesini (esneyen dallar, sallanan halatlar) Ori'nin hareketine tepki verecek şekilde fizikle kurmuş; bu dünyayı "daha canlı" kılmış. Ekip hit-stop da denemiş.

**Gerçek çiçek (K8).** Casablanca zambağı tomurcuktan açılışa ~4,5 günde ulaşır; ilk ~4 gün tomurcuk kapalı kalır, iç yapraklarda kenar buruşukları (iç gerilme) birikir; gün sonunda büyüme gerilimi yaprak-çanak yaprağı kilidini yener ve çiçek kısa sürede açılır, eğrilik tersine döner. Kenarlar merkezden çok daha hızlı büyür (kenar gerinimi çanak yaprakta ~%20-50, orta damar ~%10); ideal küresel yaprakta eğrilik tersinmesi eşikte ani, daha asimetrik yapraklarda daha yumuşak. Yani doğada açılış "yavaş birikim, kilit, ani bırakma"dır. Bu, kâğıt çiçeğin zaten yaptığı şeye (kilitli rulo, ani açılma) fiziksel dayanak veriyor.

**Nyctinasty (K9).** Bazı bitkilerde yaprak/çiçek hareketi karanlıkla gelir, pulvini hücrelerinde su/potasyum akışıyla olur; lale gibi çiçekler akşam kapanır. Çiçek kapanışının "korunma" amaçlı olduğu öne sürülüyor. (Metinde epinasti/hiponasti geçmiyor.)

**Kâğıt (K10).** BYU'nun origami "bloom" desenleri düz katlanır, açılırken merkezden dönerek dışa yayılır, ortada küresel bir ara şekilden geçer; kirigami = kesmeli origami; pop-up'ta sayfa açılırken parçalar "fırlar". Bu sayfalar hareketin hız profilini vermiyor, yalnızca açılışın merkezden dönerek yayıldığını doğruluyor (projedeki `openTwist` ile tutarlı).

### 1.3 [Çıkarım] Çiçeğe uygulama

- **Anticipation.** Gerçek zambakta kilit-gerilim-bırakma var; oyunda tomurcuğun açılmadan önce küçük bir "gerilmesi" bu kilit hissinin yerini tutar. Tomurcuk büzülür (squash) ya da yapraklar bir an daha sıkı kapanır, sonra patlar. Anticipation süresi ana hareketin yaklaşık %20-30'u kadar olduğunda okunur (kaba kural, kaynaksız; deneyle ayarlanmalı).
- **Overshoot.** Açılışta yaprakların hedefi aşıp geri dönmesi esneklik, "kâğıt yay" hissi verir; mevcut %12,6 aşım makul bir başlangıç. Aşım yönü: yaprak yatışı (pitch), ölçek, kendi ekseninde dönüş. Üçünün aynı anda aşması tek bir yay hareketi gibi okunur; farklı fazlarda aşması (ör. dönüş geç aşsın) daha zengin ve "organik" görünür.
- **Follow-through / overlapping.** Yaprak tabanı durduktan sonra ucun biraz daha gitmesi ve titreyerek yerleşmesi, kâğıdın bir yay gibi davrandığını söyler. Bunu tek yay yapamaz; en az iki yay (taban ve uç) gerekir.
- **Stagger.** Yaprakların aynı anda hareket etmemesi kritik; ama gecikme **zamanda** 40-100 ms mertebesinde olmalı (K2, K3), yoksa yayın hızlı olduğu bir sistemde fark edilmez (bölüm 5, madde 3).
- **Squash & stretch.** Kâğıt hacim korumaz, ama gövde ölçeği için kullanılabilir: açılış öncesi büzülme (squash), açılışta hafif uzama, snap sonrası ezilme. Kâğıt kesmesi dilini bozmamak için yalnızca bütün-model ölçeğinde, yaprak şeklini bozmadan.
- **Arcs.** Yaprağın ucu şu an tabandan merkez eksene göre yay çiziyor (bükme shader'ı); iyi. Kapanışta rulo tersine sarılırken ucun düz bir çizgiyle değil yayla gitmesi için `_Reach` ve `_Turn` zamanlaması farklılaştırılabilir [Çıkarım].
- **Slow in / slow out.** Açılış yayı başlangıçta hızlanır (ease-in) ve sonda yavaşlar; snap için yay sert.

---

## 2. Oyunlarda bitki, çiçek ve tuzak animasyonları

Önceden söyleyeyim: çiçek animasyonunun hissi üzerine **oyun geliştiricilerinden doğrudan alıntılanabilir kaynak az**. Aşağıda doğrulananlar ve doğrulanamayanlar ayrı.

### 2.1 Doğrulananlar

- **Super Mario Bros. Wonder.** Nintendo "Ask the Developer" ([Bölüm 2](https://www.nintendo.com/sg/interview/aqmx/02.html), [Bölüm 3](https://www.nintendo.com/sg/interview/aqmx/03.html), [Bölüm 1](https://nintendo.com/us/whatsnew/ask-the-developer-vol-11-super-mario-bros-wonder-part-1)):
  - Animasyonun görevi karakterin durumunu oyuncuya iletmek; ekip önceki oyuna göre iki kat animasyon ve 12 eklem kullanmış (Bölüm 2).
  - Müzikle senkron fikri ilk olarak **Piranha Plant'lerin borudan müzikle birlikte çıkmasıyla** denenmiş; sonra borular, platformlar, düşmanlar ritme göre hareket eder olmuş (Bölüm 3).
  - Wonder Flower toplanınca bölüm dramatik biçimde dönüşür; ses de bu geçişi taşır (Bölüm 1, 3). Çiçeğin kendi animasyonuna dair ayrıntı verilmemiş.
  - Piranha Plant için [Mario Wiki](https://www.mariowiki.com/Piranha_Plant): oyuncu borunun yanında/üstündeyken bitki çıkmıyor; oyun, tuzağı kural düzeyinde okunur kılıyor. [Wonder Flower sayfası](https://www.mariowiki.com/Wonder_Flower) çiçeğin hareket animasyonunu anlatmıyor (yalnızca dünyaya göre renk değiştirdiğini ve toplanınca grileşip saydamlaştığını söylüyor).
- **Cuphead, Cagney Carnation.** [Cuphead wiki: Floral Fury](https://cuphead.wiki.gg/wiki/Floral_Fury): yüzünü ileri atmadan önce **yaprakları kapatarak** çömelir ya da kalkar; bu, fırlayışın yüksekliğini önceden gösteren ipucu. Faz 2'de sarmaşıklar bir platformun altına önce sarılarak uyarı verir, kısa bir gecikmeden sonra yukarı sarmal çıkar. Sayfa kare düzeyinde süre vermiyor ("kısa bir an" gibi anlatıyor). [Cuphead (Wikipedia)](https://en.wikipedia.org/wiki/Cuphead): animasyon 24 kare/sn, oyun 60; [Game Developer](https://gamedeveloper.com/art/how-i-cuphead-i-nailed-the-look-and-feel-of-classic-1930s-cartoons) çizgi kalınlığı/film kusurları anlatıyor ama Carnation'a dair bir şey yok.
- **Ori ve Will of the Wisps.** Bölüm 1.2'de (K6, K7): çevre nesneleri oyuncuya tepki veriyor, ikincil hareket ayrı fizik katmanında.
- **Celeste.** Düşen blok ve ufalanan blok oyuncu üzerinde/tutunurken 0,6 sn (36 kare) sonra düşer/yok olur, kenardan tutunmada ufalanan blok için 1 sn, 2 sn sonra yeniden oluşur ([Everest/Celeste entity belgeleri](https://github-wiki-see.page/m/coloursofnoise/Resources/wiki/Entity-and-Trigger-Documentation)). Bu belge titreme/telegraph anlatmıyor; yalnızca süreleri veriyor. Çiçeğin 0,7 sn'lik uyarısı bu "0,6 sn'lik" referansla aynı büyüklükte.
- **Pikmin.** [Pikmin Wiki, Bud](https://www.pikminwiki.com/Bud): tomurcuk/yaprak/çiçek aşamalarını ve fırlatmada dönüş farkını anlatıyor; **açılış animasyonunu anlatmıyor**. Pikmin Bloom'da olgunlaşma anında bir ışık parlaması çıkıyor (kısa bir "olay" işareti).
- **Hollow Knight, Hades.** [Game Informer: Making of Hollow Knight](https://www.gameinformer.com/2018/10/16/the-making-of-hollow-knight) ve [MCV: Hades sanatı](https://mcvuk.com/business-news/behind-the-art-of-hades-we-value-artistic-integrity-and-excellence-in-artistic-craft-at-supergiant-however-were-first-and-foremost-a-game-design-lead-team/) açıldı; telegraph ya da bitki animasyonu içermiyor (Hades'te animatör mocap'i bir temel olarak kullanmış; ~942 bin karakter animasyon karesi sayısı veriliyor).

### 2.2 [Çıkarım] Çiçek için dersler

- Cuphead'in ipucu **kapanan yapraklardır**: bir çiçek için en okunur uyarı dilinin "yaprakların toparlanması" olması çiçeğin doğasıyla örtüşüyor ve renge/halkaya gerek bırakmıyor. Mevcut `warnCurlTarget` (yaprakların geri kıvrılması) doğru yönde ama etkisi küçük (bölüm 5, madde 6).
- Cuphead'in sarmaşığı "tehlike gelmeden önce tehlikenin geleceği yerde bir ön şekil" gösteriyor; bu çizgili halkanın ruhuydu. Halka kalkınca aynı rolü **çiçeğin kendi gövdesindeki bir şekil değişimi** üstlenmeli.
- Mario Wonder'ın ritim yaklaşımı (düşmanlar/platformlar ritme göre) çiçek döngüsüne uyarlanabilir: zamanlama bir tempoya oturursa oyuncu sadece görmez, **tahmin eder** (bölüm 4, fikir 5).
- Ori'nin "dünya oyuncuya tepki verir" yaklaşımı: çiçek, kanca tutulduğunda küçük bir ikincil hareketle (yaprakların hafif sarsılması) tepki verebilir [Çıkarım]; tutulmuşken kapanma uyarısı da bu tepkinin bir tonu olabilir.
- Doğrulanamayan oyun bölümleri için bölüm 6.

---

## 3. Teknik uygulama

### 3.1 Doğrulananlar

- **Spring-damper.** [Daniel Holden / theorangeduck: Spring-It-On](https://theorangeduck.com/page/spring-roll-call): sönüm oranı r = d / (2·√s); 1 kritik sönüm (en hızlı, salınımsız), <1 salınır, >1 yavaşça döner. Yayları "yarı ömür" (halflife) ve frekansla parametrelemek tasarımcıya daha sezgisel. Kritik sönümlü çözüm kapalı formda, kararlı ve dt'den bağımsız. "Double spring" (S eğrisi), "timed spring" (belli sürede hedefe ulaşan) gibi türevler var.
- **Allen Chou.** [Precise Control over Numeric Springing](https://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing): ζ=1 kritik; 0<ζ<1 salınımlı; ζ>1 "draggy". Tasarımcı için "frekans (Hz) + belli sürede genliğin ne kadarı kaybolur" parametrelemesi öneriyor (ör. 2 Hz'de 0,5 sn'de %90 düşüş ≈ ζ 0,367). Örtük (implicit) Euler çözümü veriyor.
- **t3ssel8r: Giving Personality to Procedural Animations using Math** (video, doğrudan izlenemedi). İkincil kaynaklarla doğrulananlar: [Hackaday](https://hackaday.com/2022/06/30/a-math-based-personality-for-games) sistemin frekans, sönüm ve **ilk tepki** olmak üzere üç parametresi olduğunu, çok büyük adımda patladığını ve kararlılık için adım bölme önerildiğini söylüyor; [chemicalcrux dokümantasyonu](https://chemicalcrux.github.io/vrc-docs/docs/code/best.crux.animator-dynamics/sources/second-order-dynamics) üçüncü parametreyi "responsiveness" diye adlandırıyor: negatifse sistem önce ters yöne gidip sonra tepki veriyor, 1'in üstündeyse hedefi aşıyor. Yani **tek bir yayla anticipation ve overshoot parametresi**. (Formül sabitleri bölüm 6'da doğrulanamayanlar.)
- **Easing.** [easings.net kaynak kodu](https://raw.githubusercontent.com/ai/easings.net/master/src/easings/easingsFunctions.ts): `easeOutBack = 1 + c3·(x-1)^3 + c1·(x-1)^2`, c1 = 1,70158, c3 = c1 + 1; `easeOutElastic = 2^(-10x)·sin((10x - 0,75)·c4) + 1`, c4 = 2π/3.
- **MaterialPropertyBlock ve SRP Batcher.** [Unity (URP) SRP Batcher uyumsuzluk sayfası](https://docs.unity.com/en-us/engine/current/manual/analysis/graphics-performance-profiling/in-urp/reduce-draw-calls-urp/srpbatcher/srpbatcher-incompatible): renderer'a `MaterialPropertyBlock` eklemek, o renderer'ı SRP Batcher'dan çıkarmanın belgelenmiş yolu. Yani çiçek yaprak renderer'ları (5 yaprak, 2 taç, merkez, halo, ikinci gölge materyali) SRP Batcher dışında çiziliyor; şu an bilinçli bir seçim ve bu ölçekte sorun değil, ama çiçek sayısı artarsa çizim sayısı doğrusal artar.
- **Shader bitki animasyonu.** [Unity Shader Graph animated materials](https://unity.com/blog/engine-platform/creating-animated-materials-with-shader-graph): vertex pozisyonuna maske ile offset eklenir, taban UV maskesiyle sabit kalır, sinüs + zaman ile salınım. Mevcut sistem bunun daha ileri halini (`FlowerBend.cginc` bükme integrali) zaten yapıyor; eksik olan **zamana bağlı ikincil salınım** (aşağıda).
- **Hit-stop.** [Sakurai'nin hitstop notları](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/): çarpma anında kısa dondurma güçlü hissettirir; daha güçlü hamle daha uzun dondurur ama üst sınır konur; dondurma sırasında hafif titreşim eklenip statik görünmesi engelleniyor; uzun donma hızı/adaleti bozar. [arXiv 2208.06155](https://arxiv.org/abs/2208.06155): Steam aksiyon oyunu yorumlarına bakan çalışma, hit stop, ses tutarlılığı ve kamera kontrolünün çarpma hissini en çok etkileyen özellikler olduğunu bildiriyor.

### 3.2 [Çıkarım] Projeye bağlantı

- **`UiSpring` zaten ikinci derece sistem** (semi-implicit Euler, 4 alt adım, dt ≤ 1/20 sabitlenmiş). Kritik/aşırı sönüm gerekmez; tasarım ζ 0,55 (açılış), 0,8 (snap), 0,37 (pop). Yarı ömür parametrelemesi (Holden) yalnızca editör kolaylığı olur; hissi değiştirmez.
- t3ssel "ilk tepki" parametresi, `FlowerBloom`'daki `anticipationDip` + `anticipationTime` hack'inin (hedefi kısa süre negatife çekmek) kompakt hali. Hack çalışır ama genlik ve süreyi iki ayrı sayıyla taşır; ikisini birden değiştirmek gerekir (bölüm 5, madde 1). Kick (hız dürtmesi) yaklaşımı bu hızlı yayda zayıf kalıyor [Hesap]: başlangıçta -4 b/sn hız dürtmesi yalnızca -0,03 dip veriyor; hedef-dip yaklaşımı daha etkili.
- **Ek yay katmanları** (squash/thud, uç gecikmesi, flutter) `UiSpring` örnekleri olarak eklenir; 5 yaprak için 5 yay CPU açısından önemsiz. `Time.deltaTime`'a bağlı olduklarından hit-stop (`timeScale 0`) sırasında donarlar; istenirse çiçek bilerek donar (iyi) ya da `unscaledDeltaTime` kullanılır (karar geliştiricinin).
- **Particle.** Yaprak kâğıt parçacığı (konfeti gibi 3-6 kâğıt kıymığı) snap anında Unity `ParticleSystem` ile verilebilir; kâğıt kesmesi diliyle tutarlı. Şu an yok; isteğe bağlı bir "darbe" katmanı.
- **Hit-stop ve ekran sarsıntısı.** Projede `HitStopEvents` mevcut (CLAUDE.md). Çiçek snap'inde oyuncu tutuluyorsa çok kısa bir hit-stop (2-4 kare) fırlatma anını vurgulayabilir; `HitStopDuration` 0,2 sn slingshot içindir, çiçek için çok uzun [Çıkarım]. Ekran sarsıntısı yalnızca tutulmuş çiçekte, çok küçük.

---

## 4. Uyarı (telegraph): çizgili halka olmadan

### 4.1 Doğrulananlar

- **Telegraph tanımı ve kanallar.** [Mike Stout, Game Developer, "Enemy Attacks and Telegraphing" (2015)](https://gamedeveloper.com/design/enemy-attacks-and-telegraphing): saldırıdan önce niyeti belli etme; kanallar: animasyon (wind-up), ses (şarj sesi), görsel efekt (parçacık toplanması, flaş), ses/diyalog, kuvvet geri bildirimi (titreşim). İpuçları katman katman verilmeli; ipucu kısa ama tanımaya yetecek uzunlukta olmalı; sayısal süre vermiyor.
- **Süre referansı.** [Jerry Zhang, "Anatomy of an Enemy Attack in Dark Souls 3" (2022)](https://www.gamedeveloper.com/game-platforms/anatomy-of-an-enemy-attack-in-dark-souls-3): saldırı sinyali + vuruş en az ~340 ms olmalı, tanımadık hamlede daha uzun; bazı erken düşmanlar bunu bilerek kısaltmış. Açılış pozunun silueti okunur olmalı, hamlenin ağırlığını sezdirmeli; yazar ses'in boss ritmini öğrenmede çok önemli olduğunu söylüyor.
- **Okunabilirlik.** [Alex Kubodera, "Designing for Difficulty: Readability in ARPGs" (2022)](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs): büyük jestler, tutarlı renk eşleştirmesi, ses/ses efekti ipuçları ve uyarı simgeleri; simge "koltuk değneği" olarak anılıyor.
- **Adalet.** [Jon Shafer, Spelunky adaleti](https://gamedeveloper.com/design/fairness-discovery-spelunky): tuzaklar önceden fark edilebiliyor, düşmanlar öğrenilebilir desenle davranıyor; adaletsizlik, oyuncunun kontrol edemediği/öngöremediği cezadan doğuyor.
- **Tepki süresi.** [PMC4456887](https://pmc.ncbi.nlm.nih.gov/articles/PMC4456887) (120 tıp öğrencisi, boşluk tuşu testi): kendi ortalamalarını metinde değil görselde vermiş; ama literatürden ışık için ~180-200 ms, ses için ~140-160 ms alıntılıyor (bir başka çalışmada 331 ms görsel / 284 ms işitsel). Sonuç: basit tepki ~150-300 ms aralığında, **ses görsele göre genelde daha hızlı**; seçmeli tepkide fark kaybolabilir. Seçim + motor eylem (bırak, dash) bunun üstüne eklenir.
- **Erişilebilirlik.** [Game Accessibility Guidelines: renge yalnız güvenme](https://gameaccessibilityguidelines.com/ensure-no-essential-information-is-conveyed-by-a-colour-alone/): renk yedek olmalı; şekil, desen, simge ekle; kontrast kontrol et. [WCAG 2.x, üç flaş](https://www.w3.org/WAI/WCAG21/Understanding/three-flashes-or-below-threshold.html): saniyede üçten fazla flaş yok; flaş = parlaklıkta ≥%10 değişim (genel flaş için), ayrıca doymuş kırmızı için sıkı test. (GAG'ın flaş sayfası 429 verdi; WCAG sayfası kullanıldı.) Konum titremesi flaş sayılmaz, ama kullanıcı "hareketi azalt" isteyebilir [Çıkarım].
- **Cuphead Carnation.** Bölüm 2.1: yaprakların kapanması ipucu.

### 4.2 [Çıkarım] Hangi kanal ne kadar güvenilir

| Kanal | Güçlü yanı | Zayıf yanı | Çiçekte |
|---|---|---|---|
| Şekil / silüet (yaprak katlanması, ölçek) | Renk ve ışıktan bağımsız; gamepad'e bakarken periferide bile okunur | Yavaş değişirse fark edilmez | **En güvenilir birincil kanal** olmalı |
| Hareket (titreme, darbe) | Yeni olay dikkati çeker | Küçük genlik kaybolur; sürekli titreme alışılır | Başlangıçta keskin bir **darbe**, sonra yükselen titreme |
| Ses | Tepki süresi genelde daha kısa; ekrana bakmayı gerektirmez | Ses kapalı/kalabalık; çok çiçekte karışır | Tıklar, **yalnızca yakın / tutulan** çiçek için |
| Titreşim (gamepad) | Kancaya bağlıyken tek güvenilir "bu çiçek" kanalı | Tüm çiçekler titreşirse anlamsız | **Yalnızca tutulan** çiçek |
| Ölçek | Basit, renksiz | Perspektifsiz 2D'de küçük ölçek farkı zor görülür | Şekille birlikte |
| Renk | Hızlı | Projede tüketildi; erişilebilirlik riski (kodda da not edilmiş) | İkincil, yalnız yedek |

**Süre** (referanslar: ~150-300 ms basit tepki, Dark Souls ≥340 ms, Celeste blokları 0,6 sn): 0,7 sn'lik uyarı karar vermek için yeterli; riskli olan ilk ~0,3 sn'de **hiçbir şeyin görünmemesi**, çünkü şu an her kanal 0'dan doğrusal artıyor (bölüm 5, madde 6 ve 8).

### 4.3 Çiçek için 5 somut fikir

Halka kalktığını varsayar; yeniden kullanılacak kodlar: `FlowerBloom.SetWarn`, `FlowerWarnMath`, per-yaprak `_Warn` zaten bloğa yazılıyor.

1. **Yaprak yaprak katlanma sayacı (şekil).** Uyarı 0,7 sn boyunca beş yaprak sırayla ~%35-40 kıvrılır (hedef: yaprak ilerlemesi ~0,6) ve orada kalır; her biri kendi kısa yayıyla (~60-80 ms) "çat" diye iner. Açık kalan yaprak sayısı kalan süreyi gösterir; renk gerekmez, ritim sayılabilir. Sıralama: dairesel sıra (saat gibi) ya da altın oran (organik) ikisi de denensin. Dosya: `FlowerBloom.Apply` (yaprak başına `WarnId`/`ReachId`), `FlowerWarnMath` (yeni saf fonksiyon: uyarı ve yaprak indeksinden yaprak katlanma ilerlemesi, EditMode testli).
2. **Nefes çek, sonra snap (ters yönlü anticipation).** Uyarının son ~0,1-0,15 sn'sinde çiçek `open ≈ 1,06-1,1`'e doğru hafifçe açılır (gerilir) ve ardından snap. Klasik anticipation hareketin tersine yönde olur; mevcut uyarı snap ile aynı yöne gidiyor. Risk: "daha açık = daha güvenli" okunabilir; titreme ve katlanma sayacıyla birlikte test edilmeli, A/B bayrağıyla. Dosya: `FlowerBloom.SetWarn` (hedef eğrisi), `FlowerCycleMath` (son dilim için bir `Hint`-benzeri değer).
3. **Taç ve kule geri çekilmesi (derinlik / şekil).** Uyarıda iplik tacı şu an %30 kısalıyor; kuleyi de aşağı indirmek (`towerHeight` 0,5 → ~0,25) ve gölgeyi büyütmek (mevcut) birlikte "çiçek içe çöküyor" siluetini verir. Dosya: `FlowerBloom.Apply` (merkez yüksekliği), `FlowerCoronaMath.WarnShrink` (0,3 → ~0,4-0,45 dene).
4. **Hızlanan tıklar ve titreşim (ses + dokunma).** 3-4 tık (ör. uyarıda t = 0, 0,3, 0,5, 0,6 sn; son tık = snap), her biri perde olarak biraz yükselir; sadece yakın çiçekler (mesafeye göre ses). Tutulan çiçek için gamepad titreşimi yükselen profil. Mevcut desen: `AudioCueSO` + `AudioCuePlayEventChannelSO`, `HapticManager` + yeni bir `FlowerVibrationSettingsSO` (CLAUDE.md'nin SO-per-system kuralı). Ses ve titreşim görselden bağımsız olduğundan renksiz erişilebilirlik kanalıdır; ses kapalı oyuncular için görsel kanal zaten yeterli olmalı.
5. **Ritme oturtma (Wonder yaklaşımı).** Döngü zamanlamalarını bir tempoya (örn. 1 vuruş ≈ 0,35 sn) yuvarlayıp uyarıyı 2 vuruş (görünür nabız), snap'i bir sonraki vuruşa koymak; zincirdeki çiçeklerin `phaseOffset`'ini vuruş katı yapmak. Mario Wonder'ın "borular, platformlar, düşmanlar ritme göre" yaklaşımına benzer (bölüm 2.1). Ritim oyuncunun olayı tahmin etmesini sağlar [Çıkarım]. Maliyet: sabit tempo ve büyük olasılıkla müzikle ilişki; bu yüzden en son denenecek, en riskli fikir. Mevcut `FlowerCycleTimings` toplamı 5,35 sn (1,8 + 0,5 + 2,2 + 0,7 + 0,15); 0,35 sn'lik bir vuruşta ~15 vuruşa denk gelir, yani değerlerin yarım vuruşa yuvarlanması küçük bir kayma yaratır.

**Ortak ilke (hepsine uygulanır):** uyarının **başlangıcında keskin bir olay** (ilk ~80 ms'de belirgin bir darbe: ilk yaprak katlanışı ya da ilk tık) olsun, sonra ilerleme yükselsin. Tüm kanallar sıfırdan doğrusal başlarsa algılanma gecikir (bölüm 5, madde 8) [Çıkarım].

---

## 5. Mevcut animasyonun teşhisi

Kod ve `docs/systems/grapple.md` okundu; `FlowerBloom.cs`, `FlowerCycle.cs`, `FlowerCycleMath.cs`, `FlowerBloomMath.cs`, `FlowerCoronaMath.cs`, `FlowerWarnMath.cs`, `FlowerBend.cginc`, `UiSpring.cs`. Sayılar **[Hesap]**: kodun formüllerini (yay 180/14,8, snap 900/48, `PetalProgress`, `anticipationDip` -0,08 / 0,08 sn, `hintSwell` 0,07) simüle ettim. Oyunda ölçülmedi.

Genel bulgu: yaylar `open` değerini sürüyor, ama görünür sonuç `open`'dan **smoothstep ile** türüyor (`PetalProgress`, `FlowerCoronaMath.Fan`); smoothstep başlangıçta ~x² gibi davrandığı için `open`'daki küçük oynamalar ekranda neredeyse hiç görünmüyor. Anticipation, hint ve yaprak gecikmesi tam bu bölgede.

### Madde 1. Anticipation neredeyse görünmez
- **Kanıt:** `open` en fazla -0,032'ye iniyor (hint ile birlikte +0,008, yani hiç negatif olmuyor). Yaprak ilerlemesi `Clamp01(open)` ile 0'da kalıyor; tek görünür etki `model.localScale`'in `under * 0,6` terimi: ölçek 0,981 (hint varken 1,0). Ayrıca 1° dönüş. Süre 0,08 sn = ~5 kare.
- **Denenecek:** `anticipationDip` -0,2, `anticipationTime` 0,12 → en düşük `open` -0,134 (hint'li -0,112), ölçek ~0,92-0,93, 0,125 sn civarında dip. İsterseniz dip genliği `under * 0,6` yerine ayrı bir tomurcuk "squash" ölçeğine bağlanır (örn. X/Z %6 daralır, Y %3 uzar) ki hacim hissi gelsin.
- **Dosya/parametre:** `FlowerBloom`: `anticipationDip` (Range(-0,3, 0) içinde), `anticipationTime` (Range(0, 0,2) içinde), `Apply()` içinde `under * 0.6f`.
- **Not:** dip büyürse grab penceresi (0,3 sn) ve ilk aşım zamanı (tepe ~0,37 sn) de kayar; bunun grab'a etkisi yok ama görsel "ne zaman tutulur" algısı değişir.

### Madde 2. Hint (açılış ipucu) görünmez
- **Kanıt:** `hintSwell` 0,07 → `open` 0,07; yaprak ilerlemesi en fazla 0,019 (diğerleri ≤0,005), taç hiç görünmüyor (`Fan` eşik 0,35). Kapalı fazın son 0,4 sn'sinde ekranda fark edilir bir şey olmuyor.
- **Denenecek:** `open` yerine **ayrı bir kanal**: tomurcuk çekirdeği (zaten görünür, `budScale` 0,8) nabız atsın (ör. 0,8 → 0,9 → 0,8, 0,4 sn içinde 1-2 nabız) ya da bütün model +%3-4 şişsin. İsteğe bağlı: hint hiç gerekmeyebilir; anticipation (madde 1) görünür olunca ipucu rolünü kısmen üstlenir; ikisini birbirinden ayırarak A/B (hint kapalı / açık).
- **Dosya/parametre:** `FlowerBloom.SetHint`, `Apply()` içinde `popScale`; `FlowerCycleTimings.hint` (0,4).

### Madde 3. Yaprak gecikmesi tek karelik; fiilen yok
- **Kanıt:** yaprak başına gecikme `open` biriminde (`indexStagger` 0,15). Yay 0→1'e ~0,28 sn'de gidiyor; sonuç olarak beş yaprağın %50 ilerlemeyi geçtiği an 0,179-0,196 sn: **17 ms fark** (60 fps'te 1 kare). `indexStagger` 0,5'e (Range üst sınırı) çıkarılsa bile fark 63 ms olur. Hâlbuki akış hissi için kaynaklar parça başına 50-100 ms offset öneriyor (K2, K3). Kapanışta fark 5 ms (snap 0,05 sn'de %50'yi geçiyor): hiç görünmez.
- **Denenecek:** gecikmeyi **zaman alanına** taşı: her yaprağa kendi `UiSpring`'i (5 örnek) ve başlangıç gecikmesi 0-0,12 sn (ör. 30 ms'lik basamaklar; sırayla ya da altın oranla). Kapanışta gecikme 0-0,03 sn tutulabilir (snap hızını bozmaz). Karşılaştırma: gecikme 0 / mevcut / 0-0,12 sn.
- **Dosya/parametre:** `FlowerBloom` (yeni `petalSprings[]`, `petalDelay` alanı), `FlowerBloomMath.PetalProgress` (artık yalnızca eğri; gecikme dışarı alınır). Saf mantık değiştiği için EditMode testi gerekir (CLAUDE.md kural 3).

### Madde 4. Açılışta tek yaylı, tek tempolu hareket: "çiçek açılıyor" yerine "pop"
- **Kanıt:** `open` 0,1'i 0,129 sn'de, 0,9'u 0,254 sn'de geçiyor; yani 10-90 yükselişi ~0,125 sn (7-8 kare). Tepe 0,37 sn'de (%12,8 aşım), %2 bandına 0,52 sn'de giriyor (Opening fazı 0,5 sn). Bu bir "patlama"; çiçeğin doğasındaki "yavaş gerilim + ani bırakma" (K8) ile uyumlu olsa da gerilim kısmı yok (madde 1) ve patlamadan sonra ikincil hareket yok (madde 5).
- **Denenecek:** önce madde 1 ve 5; ardından gerekirse açılış yayı 180/14,8 → ~140/13 (ζ ≈ 0,55 kalır, yükseliş ~%14 yavaşlar) denenebilir. Önce eklemeler, sonra yay, çünkü yay sertliğini değiştirmek grab penceresinin hissini de kaydırır.
- **Dosya/parametre:** `FlowerBloom.openStiffness` / `openDamping`.

### Madde 5. Follow-through ve overlapping yok: yaprak ucu tabanı takip ediyor
- **Kanıt:** `FlowerBend.cginc` bükmeyi bir `(pitch, reach, curl, roll)` fonksiyonu olarak çözüyor; `FlowerBloom.Apply` hepsini aynı `prog` ile sürüyor (`PitchId`, `ReachId`, `BaseCurlId = openCurl * prog`, `RollId`). Taban durunca uç da aynı anda duruyor; zamana bağlı ikincil salınım yok (bitki sallanması gibi). K1 follow-through, K6 (Ori ikincil hareket).
- **Denenecek:** `_BaseCurl`/`_Roll`'u ikinci bir daha yavaş yaydan (ör. 90/6, ζ ≈ 0,32) sür: uç, taban durduktan ~0,15-0,25 sn sonra yerleşsin; ayrıca açılışın hemen ardından yaprak yatışı (`pitch`) üzerinde ±2-4°'lik sönümlü bir flutter (yay dürtmesiyle `Kick`). Shader değişmez; yalnızca C# tarafında bloğa yazılan değerler.
- **Dosya/parametre:** `FlowerBloom.Apply` (`BaseCurlId`, `RollId`, `PitchId`), yeni iki `UiSpring` alanı.

### Madde 6. Uyarının yapraklardaki etkisi birkaç derece; asıl yük taçta ve titremede
- **Kanıt:** uyarı sonunda `open` ≈ 0,82 (hedef 0,8); yaprak ilerlemesi 0,89-0,99 → yalnızca %1-11 geri kıvrılma, yani yaprak yatışında birkaç derece. Taç `Fan` 0,77 + %30 büzülme: görünür, ama taç yaprakların altından başlıyor. Titreme genliği `trembleAmplitude` 0,025 model-yerel birim; kök ölçek 4 ile 0,1 dünya birimi = çiçek çapının ~%1'i (≈9 birim), oyuncunun (1 birim) %10'u. Halka kalkınca uyarı bu kanallara kalıyor.
- **Denenecek:** `warnCurlTarget` 0,8 → 0,6 (yaprak ilerlemesi ~0,57-0,65, yani %35-43 geri kıvrılma), `trembleAmplitude` 0,025 → 0,045-0,05; bölüm 4.3 fikir 1 (yaprak yaprak katlanma). Üçünü ayrı ayrı denemek için küçük bir test odası senaryosu yeterli.
- **Dosya/parametre:** `FlowerBloom.warnCurlTarget`, `trembleAmplitude`, `trembleHz`.
- **Dikkat:** uyarı çok güçlüyse çiçek "zaten kapandı" diye okunup oyuncu erken bırakabilir; geliştirici kuralı: uyarıda hâlâ tutulabilir (docs/systems/grapple.md).

### Madde 7. Kapanış (snap) darbesiz ve wind-up'sız
- **Kanıt:** snap 0,054 sn'de %50'ye (3 kare), 0,104 sn'de %10'a iniyor; en düşük `open` -0,014 (aşım yok, ölçek 0,99). Ses, parçacık, ölçek tepkisi, hit-stop yok; uyarı snap ile aynı yöne gittiğinden (açık → kapanma) klasik wind-up yok: yavaş sürünme + hızlı kapanma.
- **Denenecek:** (a) `CloseBloom` anında bütün model ölçeği için ikinci bir yay (örn. 400/14) ve -0,07'lik hız dürtmesi: snap sonrası kısa bir ezilme + geri seken tepki (squash, K1); (b) bölüm 4.3 fikir 2 (son 0,12 sn'de ters yönlü nefes); (c) oyuncu o çiçeği tutuyorsa 2-4 karelik hit-stop (`HitStopEvents` üzerinden), süre için 0,2 sn slingshot değeri çok uzun; (d) kâğıt kıymığı parçacığı. Hepsi opsiyonel katmanlar; her biri ayrı ayrı açılıp kapanabilmeli.
- **Dosya/parametre:** `FlowerBloom.CloseBloom`, `Apply()` ölçek; `FlowerCycle.Update` (Closing fazı); hit-stop için projedeki `HitStopEvents` (bu çalışmada koduna bakılmadı; CLAUDE.md'den biliniyor).

### Madde 8. Uyarı rampası sıfırdan doğrusal başlıyor, başlangıç olayı yok; ve halka kodu artık ölü
- **Kanıt:** `FlowerCycleMath.Warn01 = into/warning` doğrusal. Titreme genliği `warn` ile orantılı ve frekans 8→20 Hz; uyarının ilk ~0,2-0,3 sn'sinde genlik ve kıvrılma çok küçük (genlik ≤ 0,3 × son değer), yani bir insanın tepki süresi (~0,15-0,3 sn, bölüm 4.1) kadar bir "kör" aralık var [Çıkarım]. Ayrıca warn rengi/halka koda hâlâ bağlı: `FlowerWarnMath.BlinkOn/RingVisible`, `warnRing*` alanları, `Flower/Halo` uyarı modu, `warnTint` iç halka rengi.
- **Denenecek:** uyarı başlangıcına keskin bir olay ekle: ilk ~80 ms'de ani bir "ürperme" (tek yaprak katlanışı ya da kısa, büyük genlikli titreme tepesi) ve sonra rampa; rampaya ease-out de uygulanabilir (`1 - (1-w)²`) ki erken kısım güçlü başlasın. Halka kaldırılacağı için ayrı bir temizlik işi: `FlowerWarnMath` (ve testleri), `FlowerBloom` halka alanları, `Flower/Halo` uyarı modu, `warnTint`.
- **Dosya/parametre:** `FlowerCycleMath.Evaluate` (Warn01), `FlowerBloom.SetWarn`, `FlowerBloom.Apply` (`_tremblePhase`, `pulse`).

(Teşhis 8 madde; bu listeye eklenmeyen küçük bulgu: açılış halo dalgası `haloDuration` 0,35 sn, snap'te halo hiç çalışmıyor; kapanışın "iz" bırakması bir seçenek, ama kâğıt dilinde anlamı tartışılır.)

---

## 6. Doğrulanamayanlar

- **t3ssel8r videosu** doğrudan izlenemedi (YouTube sayfası yalnızca başlığı verdi, Patreon 403, özet servisi içerik üretememiş). k1/k2/k3 formülleri ve kararlılık sınırı bu çalışmada kaynaktan doğrulanmadı; yalnızca üç parametre ve "negatif tepki = ters yöne önce gitme, >1 = aşım" ikincil kaynaklardan (Hackaday, chemicalcrux dokümanı) doğrulandı.
- **"Juice It or Lose It" (Jonasson & Purho)** ve **Vlambeer "Art of Screenshake"** konuşmalarının içeriği açılan bir sayfadan doğrulanmadı (yalnızca arama sonuçları var); bu yüzden referans verilmedi. Hit-stop için Sakurai notları ve arXiv çalışması kullanıldı.
- **Super Mario Bros. Wonder'daki Wonder Flower'ın kendi animasyonu** (salınma, ifade, aktivasyon animasyonu) ve **Piranha Plant'in Wonder'daki şarkı/animasyon ayrıntıları**: Nintendo röportajı ve Mario Wiki bunları anlatmıyor; yalnızca ritme göre hareket ve borudan çıkış testi doğrulandı.
- **Pikmin tomurcuk/çiçek açılış animasyonu**: wiki açılış hareketini anlatmıyor.
- **Hollow Knight** (telegraph, flora animasyonu), **Gris**, **Spiritfarer**, **Hades** (telegraph): bu konularda geliştirici kaynağı bulunamadı ya da sayfa açılamadı (Gris MCV sayfası 429). Dolayısıyla bu oyunlar için çıkarım yapılmadı.
- **Ori ve Blind Forest (ilk oyun)** bitki tepkisi: bulunamadı; yalnızca Will of the Wisps röportajları var.
- **Celeste'de ufalanan/düşen blokların titremesi**: doğrulanamadı; sadece süreler (Everest belgesi). Celeste donma karesi/ekran sarsıntısı notu doğrulanamadı (GameSpot 403).
- **Spelunky'nin tuzak görsel ipuçları**: Shafer yazısı genel adaleti anlatıyor, animasyon ayrıntısı yok.
- **Çiçek açılış hız profili (sigmoid/ease eğrisi)**: Arama, petal açısının zamana göre bir sigmoid uydurması bulmadı; yalnızca zambak çalışması (ani kilit bırakma). "Gerçek çiçekler yavaş-hızlı-yavaş açılır" ifadesi kaynakla doğrulanmadı ve kullanılmadı. Ijiri vd. "Surface-based growth simulation for opening flowers" PDF'i 10 MB sınırını aştığı için açılamadı.
- **GAG "3 flaş" sayfası** 429 verdi; yerine WCAG sayfası kullanıldı.
- **İnsan tepki süresi**: tek bir temiz kaynak yok; PMC4456887 kendi ortalamasını görselde verdiği için okunamadı, literatür alıntıları (180-200 ms / 140-160 ms) ve arama özeti (331 / 284 ms) kullanıldı; **yaygın "200-250 ms" rakamı doğrulanmadı**.
- Bu belgedeki "kaba kural" (anticipation süresi hareketin %20-30'u) kaynaksızdır; deneyle ayarlanmalı.

---

## 7. Yöntem notu

- Arama + sayfa okuma ile 40'tan fazla sayfa açıldı; bölüm 1-4'te yalnızca açılıp okunanlar [Doğrulandı] sayıldı. Başarısız olan sayfalar bölüm 6'da. `WebFetch` küçük bir model ile özetlediği için bazı ayrıntılar (örn. Cuphead'in "yaprakları kapatma" ipucu, Celeste süreleri) tek sayfa özetine dayanıyor; kritik karar alınmadan önce kaynak sayfa elle kontrol edilmeli.
- Telifli içerik kopyalanmadı; kaynaklar kendi cümlelerimle özetlendi.
- Bölüm 5'teki sayılar `UiSpring` ve `FlowerBloomMath` kodundan türetilen Python simülasyonudur (yay hesabı: dt 1/240, `UiSpring`'in 4 alt adımlı semi-implicit Euler'i birebir; `PetalProgress` kodun kendisi). Gerçek oyunda kare hızı ve `Time.deltaTime` farklı olabilir (Step içinde dt ≤ 1/20 sabitlenmiş). Oyunda ölçüm yapılmadı, sahne ve prefab açılmadı.
- Kod, shader ve sahne değiştirilmedi. `FlowerBloom`'daki uyarı halkası alanları hâlâ duruyor; halkanın kaldırılması geliştiricinin kararı olarak ele alındı, burada yalnızca etkisi (bölüm 5, madde 8) not edildi.

---

## 8. Önerilen task listesi (1-1,5 saatlik parçalar; `TASKS.md`'ye eklemeden önce kullanıcıya sorulmalı)

1. **Anticipation ve hint görünür olsun** #code: `anticipationDip` -0,2 / `anticipationTime` 0,12 (başlangıç tahmini), hint'i tomurcuk çekirdeği nabzına taşı; bütün-model squash ayrı kanal. Dosya: `FlowerBloom`. Ardından ayrı bir `#test` görevi: gamepad ile hissi onay.
2. **Zaman alanında yaprak gecikmesi** #code: yaprak başına `UiSpring` ve 0-0,12 sn gecikme, kapanışta 0-0,03 sn; `FlowerBloomMath` için EditMode testi (saf zamanlama fonksiyonu).
3. **Yaprak ucu follow-through** #code: `_BaseCurl`/`_Roll` için ikinci (daha yavaş, ζ ≈ 0,3) yay + açılış sonrası pitch flutter; shader değişmez.
4. **Uyarı: başlangıç darbesi + yaprak yaprak katlanma sayacı** #code: `FlowerWarnMath`'a yeni saf fonksiyon (testli), `FlowerBloom.Apply` yaprak başına `_Warn`/`_Reach`; `warnCurlTarget` ve `trembleAmplitude` A/B değerleri.
5. **Snap darbesi** #code: model ölçeği için "thud" yayı, isteğe bağlı kâğıt kıymığı parçacığı; hit-stop'u ayrı bayrakla (tutulan çiçek, 2-4 kare). Hit-stop ve parçacık opsiyonel, ayrı denenmeli.
6. **Uyarı ses ve titreşim kanalı** #code: `AudioCueSO` ile 3-4 yükselen tık (yakın çiçek), `FlowerVibrationSettingsSO` ve `RumbleProfile` ile yalnızca tutulan çiçeğe yükselen titreşim (SO-per-system deseni).
7. **Halka kodu temizliği** #code: `FlowerWarnMath` halka/blink kısımları (ve testleri), `warnRing*` alanları, `Flower/Halo` uyarı modu, `warnTint`; docs/systems/grapple.md güncellemesi (Son kod doğrulaması tarihi ile).
8. **Gamepad turu: uyarı ve açılış hissi** #test: yukarıdaki A/B'ler (anticipation, stagger, katlanma sayacı, ters nefes, thud/hit-stop), değerleri `FlowerBloom`'a işleme; ritim senkronu (bölüm 4.3, fikir 5) yalnızca bu tur sonunda gerekirse ayrı bir görev.

Plan notu (CLAUDE.md): bir güne en fazla 4 task; 1+2 aynı gün, 3+4 ertesi gün, 5+6 sonraki gün, 7 ve 8 tampon günüyle birlikte. `#code` görevlerinin `#test` doğrulaması aynı ya da ertesi gün.
