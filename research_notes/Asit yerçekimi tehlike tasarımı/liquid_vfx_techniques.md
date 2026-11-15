# Stilize sıvı VFX teknikleri: yerçekimi cihazında tutulan asit tehlikeleri (Unity 6 URP 2D)

Kapsam: hareketli öldürücü tehlikeleri (daire / dikdörtgen, ping-pong / dairesel / hızlanan / dönen yollar, örnek başına farklı boyut, dikdörtgenler scale ile uzatılıyor) "yerçekimi cihazının havada tuttuğu asit" olarak shader ağırlıklı ve ucuz biçimde çizmek. Bütün bulgular şu bağlama göre değerlendirildi: yarım ton (halftone) baskı görünümü (piksel art değil), üstte CRT filtresi, düz gökyüzü mavisi arka plan, parlak asit yeşili zaten oyunda "öldürücü" anlamında kullanılıyor, tek kişilik geliştirici, doku/ekstra obje yerine shader tercih ediliyor.

Araştırma notu: Bu oturumda Inigo Quilez'in sayfaları (distfunctions2d, smin) araç tarafından 404/yönlendirme hatasıyla açılamadı ve Cyanilux, Daniel Ilett, Minions Art, Harry Alisavakis, Freya Holmér, Ben Cloward için hedefli aramalar doğrudan sayfa döndürmedi. Bu kaynakların adları ve URL'leri aşağıda kanonik referans olarak verildi ama içerikleri bu oturumda doğrulanmadı; ilgili satırlar böyle işaretlendi.

## 1. 2D animasyonda sıvı FX gelenekleri (sakuga, Batı FX animasyonu, az renkle sıvı)

### Takeaway
Elle çizilen FX animasyonunda sıvı, detayı azaltıp ağırlık, yoğunluk, viskozite ve yerçekimi hissini koruyan basit, yuvarlak, akış yönünü gösteren şekillerle okunur: koyu siluet, birkaç sert kenarlı parlama, akış yönünde dizilmiş ve ana kütleden uzaklaştıkça küçülen kabarcık/damlacıklar. Bu yaklaşım posterize edilmiş 2–3 tonlu bir shader'a doğrudan çevrilebilir.

### Cited Findings
- Joseph Gilland'ın *Elemental Magic* (Cilt I, 2009) kitabı sıvı, ateş, duman, patlama ve büyü efektlerini kapsar. Kitabın sıvılar bölümü (Bölüm 4) suyu tasarım ve fizik ilkelerinin bir toplamı olarak ele alır ve bu ilkelerin ortamdan bağımsız olarak her efekt türüne uygulanabildiğini savunur — [O'Reilly, Bölüm 4 Liquids](https://www.oreilly.com/library/view/elemental-magic/9780240811635/000_9781136137259_chapter4.html); [Routledge kitap sayfası](https://www.routledge.com/Elemental-Magic-Volume-I-The-Art-of-Special-Effects-Animation/Gilland/p/book/9780240811635)
- Bir incelemeye göre Gilland sıvılarda yoğunluk, kuvvet, yerçekimi, viskozite ve "sıçrama sonrası" (after splash) gibi etkenleri inceler. Kitap, efekti gerçekçi tutarken detayın nasıl ayıklanacağını gösterir — [Parka Blogs incelemesi](https://www.parkablogs.com/node/1783)
- Gilland eski bir Disney efekt animatörüdür (Lilo & Stitch, Mulan). Cilt II (2012) tekniğe odaklanır (dalgalanan su dahil) — [Parka Blogs](https://www.parkablogs.com/node/1783); [O'Reilly Cilt II tanımı](https://oreilly.com/library/view/elemental-magic/9780240814797)
- VFX Apprentice'in stilize su/sıvı FX çizim ipuçları: (1) sıvıyı 2D'de okutan dört öğe saydamlık, yansıtıcılık, kabarcık ve dalgacıktır; (2) dalgacık ve kabarcıklar akış yönünü gösterir, boyutları değişken olmalıdır; (3) kabarcık, köpük ve dalgacıklar tek yönde akmalı, bütün kenarlar sert değil yuvarlak olmalıdır; (4) temel olarak tek çizgilik bir girdap (vortex) rehberi çizilip akışkan şekiller onun etrafına kurulur; (5) kabarcıklar ana kütleden uzaklaştıkça küçülür, parçalanır ve akış yönüne bakar — [VFX Apprentice: 5 Pro Tips on Drawing Stylized Water](https://www.vfxapprentice.com/blog/how-to-draw-stylized-water)
- Pixar'ın *Elemental* su karakterleri üzerine makalesine göre kabarcık, parıltı ve specular parlamalar stilize yöne itilirken bulanıklık ve kostikler daha gerçekçi tutuldu. Yani stilizasyon her katmana eşit uygulanmadı — [Pixar, Elemental Water paper](https://graphics.pixar.com/library/ElementalWater/paper.pdf)
- Bir boyama rehberi, su siluetini biraz daha koyu bir tonla çizip üzerine beyaz parlamalar eklemeyi ve ışık yönünü tutarlı tutmayı önerir — [MediBang: effect for expressing water](https://medibangpaint.com/en/use/2020/09/effect-for-expressing-water)
- Akademik çalışmalar: stilize sıvı efektleri yalnızca yüzey görünümünü değil şekli de değiştirmelidir. Hıza bağlı bir alan uzamış damlacıklar üretir ve kıvrılan dalga tepeleri hareketi anlatmak için kullanılır — [Implicit Field-Based Stylization of 2D and 3D Liquid Animations (2025)](https://diglib.eg.org/handle/10.2312/stag20251324); [Expressive Rendering for 2D Animations of Liquids (2025)](https://diglib.eg.org/handle/10.2312/exw20251069); [GI 2007: cartoon liquid](https://graphicsinterface.org/proceedings/gi2007/gi2007-8)
- Sakuga yüksek kaliteli animasyondur ve ekiplerde bazı animatörler yalnızca efekt çizer — [Nihon Narratives: Sakuga](https://nihonnarratives.substack.com/p/sakuga-animation)
- Realtime VFX forumunda stilize/çizgi film efektleri için kaynak listesi derleyen bir başlık var — [realtimevfx.com: resources for stylized/cartoon effects](https://realtimevfx.com/t/looking-for-ressources-for-stylized-cartoon-effects/8769)

### Inferences
- Az renkli asit için kullanılabilir bir "FX animatörü" sözlüğü (kaynaklardan derlendi): **koyu kenar/siluet tonu** (ink #140C1C'ye kayan koyu yeşil), **gövde tonu** (oyundaki parlak asit yeşili), **sert kenarlı parlama** (paper #F1E6C7 veya beyaza yakın, ışık yönüne göre hep aynı tarafta, örneğin sol üst), **akış yönünde kabarcıklar**. Gövdede degrade yerine 2–3 bant posterizasyon, yarım ton baskı dünyasına uyar.
- Cihazın sıvıyı "tuttuğunu" okutmak için Gilland'ın yerçekimi ve viskozite vurgusu tersine çevrilebilir: sıvı aşağı sarkmak ister ama cihaz onu yörüngede tutar. Halkanın alt yarısında hafif kalınlaşma/sarkma ve alttan kopan seyrek damlalar bu gerilimi anlatır.
- Hız okunurluğu: "girdap rehberi" ve "akış yönüne bakan kabarcık" ipuçları, dönen/dairesel hareket eden tehlikede kabarcıkları ve parlamaları teğet yönünde uzatmanın (hıza bağlı uzama) en ucuz hareket ipucu olduğunu düşündürüyor.
- Cuphead/Rubberhose ve Trigger/Ghibli su için birincil, alıntılanabilir kaynak bulunamadı (bkz. Gaps). Genel eğilim (kalın kontur, sınırlı palet, abartılı damla şekilleri) yukarıdaki kaynaklarla uyumlu ama bu oturumda doğrulanmadı.

### Gaps
- Cuphead (StudioMDHR) sıvı/damla çizim kuralları, Studio Trigger ve Ghibli su efekti için birincil kaynak (röportaj, artbook, GDC konuşması) bulunamadı.
- Gilland'ın sıvı bölümündeki adım adım teknikler serbestçe okunamıyor; yalnızca inceleme özetleri var. Detay için kitabın kendisine bakmak gerekir.
- Riot/Blizzard stilize VFX GDC konuşmalarına bu oturumda ulaşılmadı.

## 2. Shader teknikleri (SDF, polar UV, metaball, kenar dalgalanması, rim, yarım ton, posterize, köpük, kabarcık)

### Takeaway
Bütün görünüm tek bir unlit sprite/quad shader'ında yapılabilir: şekil için analitik 2D SDF (daire, halka, yuvarlatılmış kutu), kenar dalgalanması için SDF'ye zamanla kayan gürültü eklemek, akan halka için polar koordinatlarda kaydırılan gürültü, iç bantlar için SDF'yi eşiklemek (posterize + rim + köpük çizgisi), gölge bandında yarım ton noktaları. Damlalar için SDF smooth-union (smin) kullanılır. Doku gerekmez.

### Cited Findings
- **Polar Coordinates node:** Girdi UV'yi bir merkeze uzaklık ve bir referans yönden açı olarak polar koordinata çevirir. Akan halka için temel yapı taşıdır — [Unity Shader Graph: Polar Coordinates Node](https://docs.unity3d.com/Packages/com.unity.shadergraph@8.1/manual/Polar-Coordinates-Node.html)
- Polar koordinatlarla su borusu/akan sıvı simülasyonu üzerine bir topluluk örneği var. Alternatif olarak forumda halka şekilleri için polar UV yerine düz UV'li disk mesh veya halka maskesi öneriliyor. Bir yorumcu, değiştirilmiş UV'li mesh'in polar koordinattan daha iyi çalışabildiğini söylüyor — [r/Unity3D: water pipe with polar coordinates](https://redlib.hackliberty.org/r/Unity3D/comments/1ioel31/you_can_simulate_a_water_pipe_by_using_polar); [realtimevfx: polar filter in Shader Graph](https://realtimevfx.com/t/how-does-shader-graph-connect-the-polar-filter-effect-of-ps/8263)
- **Gürültü + eşik (dissolve) kalıbı:** Noise → Step → Alpha, eşik değeri zamanla değiştirilerek canlandırılır. Kenar çözülmesi ve sıçrama/yeniden oluşma için aynı kalıp kullanılır — [gamedev.net: Shader Graph dissolve](https://gamedev.net/tutorials/visual-arts/unity-shader-graph-dissolve-and-inverse-dissolve-shaders-overview-and-download-r5589); [Mirza Beig: Dissolve Shader Part 1](https://mirzabeig.substack.com/p/unity-tutorial-dissolve-shader-part-1)
- **Shader Graph prosedürel şekil node'ları:** Ellipse ve Rounded Rectangle node'ları UV'den genişlik/yükseklikle şekil üretir. Rounded Rectangle'da Radius köşe yarıçapıdır. Döşenince otomatik tekrarlamaz (tekrar için önce Fraction node'u) — [Unity: Procedural Nodes](https://docs.unity3d.com/Packages/com.unity.shadergraph@17.1/manual/Procedural-Nodes.html); [Rounded Rectangle Node](https://docs.unity3d.com/Packages/com.unity.shadergraph@7.4/manual/Rounded-Rectangle-Node.html)
- Unity'nin UGUI Shader Graph örneklerinde SDF subgraph'ları var: Fill çıkışı şekli, SDF çıkışı ham mesafeyi verir. Ham mesafe bantlama/kontur için gereklidir — [Unity: UGUI Shaders Subgraph nodes](https://docs.unity3d.com/Packages/com.unity.shadergraph@17.0/manual/Shader-Graph-Sample-UGUI-Shaders-Subgraph-nodes.html); [Resolution-independent how-to](https://docs.unity3d.com/Packages/com.unity.shadergraph@17.1/manual/Shader-Graph-Sample-UGUI-Shaders-How-tos-Res-indepenent.html)
- **Rounded box SDF formülü:** `q = abs(p) - b + r; d = length(max(q,0)) + min(max(q.x,q.y),0) - r` — [Infinite Canvas lesson 009](https://scour.ing/p/https://infinitecanvas.cc/guide/lesson-009)
- NedMakesGames'in 2D SDF video serisi segment ve (yuvarlatılmış) dikdörtgen SDF'lerinin matematiğini anlatıp Shader Graph'a çeviriyor — [NedMakesGames: Drawing Shapes with 2D SDFs](https://nedmakesgames.github.io/blog/2dsdf)
- Inigo Quilez'in 2D mesafe fonksiyonları kataloğu (sdCircle, sdBox, sdRoundedBox, sdSegment/kapsül, sdArc, sdRing ve `opRound`, `opOnion` (halka/kabuk) operatörleri) ve smooth minimum (smin) makalesi bu alanın kanonik referansıdır — [IQ: 2D distance functions](https://iquilezles.org/articles/distfunctions2d/); [IQ: smooth minimum](https://iquilezles.org/articles/smin/) (**not: iki sayfa da bu oturumda araçla açılamadı. İçerik doğrulanmadı, fonksiyon adları genel bilgiden.**)
- **2D sprite su:** Kayan UV dokuları ve gürültü desenleriyle sprite su anlatan bir Unity eğitimi, x ekseninde zamanla kaydırılan Gradient Noise'un çizgisel akış (streak) etkisi verdiğini gösteriyor — [80.lv / Unity nature shaders blog](https://unity.com/blog/engine-platform/nature-shaders-with-shader-graph-in-2022-lts) (arama özeti, video doğrudan izlenmedi)
- **Toon su referansları:** Roystan'ın toon water eğitimi köpüğü derinlik/normal tamponundan, dalgaları gürültü + distorsiyon dokularıyla üretir (3D). Alexander Ameye'nin stilize su notları fiziksel doğruluk yerine kontrol edilebilirliği hedefler — [Alexander Ameye: stylized water shader](https://alexanderameye.github.io/notes/stylized-water-shader); [Toon Water (Godot Shaders)](https://godotshaders.com/shader/toon-water/); [Stylized Toon Water (Godot Shaders)](https://godotshaders.com/shader/stylized-toon-water/)
- Cyanilux (Shader Graph breakdown'ları, ör. polar/dissolve), Daniel Ilett (2D/URP Shader Graph eğitimleri), Minions Art (toon sıvı, Patreon), Harry Alisavakis (teknik sanat blogu), Freya Holmér (şekil/SDF ve Shapes kütüphanesi), Ben Cloward (Shader Graph/UE video serisi) istenen kaynaklardı. Hedefli aramalar doğrudan sayfalarını döndürmedi — [Minions Art Patreon (arama sonucu)](https://www.patreon.com/posts/shader-graph-30490169)

### Inferences
- **Akan halka (daire tehlikesi):** `uv' = uv - 0.5` → polar (r, θ). Halka maskesi `abs(r - R) - w` (onion/annulus), kenara `noise(θ*N + time*speed, r*M)*amp` eklenerek dalgalandırılır. Gövde içi `θ + time` kaydırmalı gürültü ile akış bantları çıkarılır. θ = ±π dikişinde gürültü kopar; çözüm: periyodik gürültü (θ yerine `cos θ, sin θ` ile 2D gürültü örneklemek, yani `noise(dir * k + time)`) veya tam sayı tekrar sayısı.
- **Posterize + rim + köpük çizgisi:** Tek bir SDF değeri `d` (içeride negatif) şu bantlara bölünür: `d > 0` dışarısı; `-t1 < d < 0` koyu kontur (ink'e yakın); `-t2 < d < -t1` rim/parlak kenar; daha içerisi gövde. Bir "köpük/akış çizgisi" için gövde içinde `abs(noise - 0.5) < ε` ile ince eş-değer çizgileri çıkarılır. Hepsi Step/Smoothstep; doku yok.
- **Kenar yumuşatma (AA):** SDF'yi `saturate(-d / fwidth(d))` ile eşiklemek (DDXY node'u) farklı ölçeklerde keskin ama tırtıksız kenar verir. Ölçeği değişen tehlikeler için sabit `smoothstep` aralığından daha güvenli. CRT filtresi zaten yumuşattığından 1 piksellik AA yeter.
- **Yarım ton gövde:** Projede platformlar için yumuşak baskı tarzı yarım ton ve UI'da shader tabanlı desenler (UiConcrete/UiDither) zaten var. Sıvının gölge bandında (ör. ışığın karşı tarafındaki alt yarım) ekran uzayında sabit ızgaralı nokta deseni, nokta yarıçapı = `1 - lighting` kullanılarak tehlike platformlarla aynı "baskı" dilini konuşur. Izgara dünya/ekran uzayında olmalı (UV'de olursa uzatılan dikdörtgende noktalar elips olur).
- **Metaball/smin:** Damla sarkması ve kopması için halka SDF'si ile küçük bir daire SDF'si (damla) `smin(a, b, k)` ile birleştirilir. Damlanın y konumu zamanla aşağı kayıp `k` küçüldükçe ayrılıyormuş gibi görünür. Shader içinde 1–3 damla ucuzdur. Daha fazlası partiküle bırakılmalı.
- **Vertex wobble:** Analitik SDF fragment'ta çalıştığından vertex wobble gerekmez. Quad'ın dalgalanma genliği + damla payı kadar büyük olması yeter (bkz. bölüm 3 ve 5).
- **Kabarcıklar:** Gövde içinde hücresel/Voronoi node'unun küçük eşikli hali, akış yönünde (θ + time) kaydırılarak kabarcık noktaları verir. VFX Apprentice'in "uzaklaştıkça küçülen" kuralı için eşik `d` ile ölçeklenebilir.

### Gaps
- IQ kataloğunun tam fonksiyon imzaları bu oturumda doğrulanamadı (sayfa açılmadı).
- Cyanilux / Daniel Ilett / Freya Holmér / Ben Cloward'ın bu konuya özel (halka, sıvı, metaball) eğitim URL'leri bulunamadı. Uygulama öncesinde doğrudan sitelerinde aranmalı.
- Shader Graph'ta θ dikişsiz gürültü için hazır bir resmî örnek bulunamadı.

## 3. Farklı uzunluktaki dikdörtgenler için 9-slice gerektirmeyen çözüm

### Takeaway
Dikdörtgeni UV'de değil, gerçek boyutunu bilen nesne/dünya uzayında çizmek gerekir: yuvarlatılmış kutu SDF'sine yarım boyut olarak nesnenin ölçeği verilir. Böylece köşe yarıçapı, kontur kalınlığı, dalgalanma ve yarım ton noktaları uzunluktan bağımsız kalır. Doku, 9-slice ya da örnek başına materyal gerekmez.

### Cited Findings
- Rounded Rectangle node'u UV'ye göre Width/Height ile şekil üretir. Yani UV 0–1 uzayında çalışır ve nesne scale ile uzatılınca köşeler elipsleşir — [Unity: Procedural Nodes](https://docs.unity3d.com/Packages/com.unity.shadergraph@17.1/manual/Procedural-Nodes.html)
- Rounded box SDF'si yarım boyut `b` ve yarıçap `r` ile analitik olarak yazılabilir — [Infinite Canvas lesson 009](https://scour.ing/p/https://infinitecanvas.cc/guide/lesson-009)
- Unity'nin UGUI örneklerinde çözünürlükten bağımsız SDF şekilleri için ayrı bir "how-to" var (boyut farkındalıklı SDF) — [Unity: Res-independent how-to](https://docs.unity3d.com/Packages/com.unity.shadergraph@17.1/manual/Shader-Graph-Sample-UGUI-Shaders-How-tos-Res-indepenent.html)
- Unity'nin 2D renderer'ında MaterialPropertyBlock'lu renderer'lar SRP Batcher dışına düşer ve dynamic batching'e döner. Bu yüzden boyutu örnek başına MPB ile göndermenin batching maliyeti vardır — [Unity forum: SRP Batcher & SpriteRenderer](https://discussions.unity.com/t/srp-batcher-spriterenderer/918871); [Unity Manual 6000.3: SRP Batcher incompatible](https://docs.unity3d.com/Manual/SRPBatcher-Incompatible.html)

### Inferences
- **Yöntem A (önerilen, scale'den boyut):** `p = (uv - 0.5) * size`, `size` = Shader Graph Object node'unun Scale çıkışı (xy). SDF `sdRoundedBox(p, size*0.5 - pad, r)`. `pad` = dalgalanma genliği + kontur payı, dünya biriminde sabit. Uzunluk değişince yalnızca düz orta kısım uzar.
- **Batching tuzağı:** SpriteRenderer'lar 2D renderer'da dinamik olarak birleştirilirse vertex'ler dünya uzayına pişer ve nesne matrisi/scale kimlik olarak okunabilir. Bu durumda Object Scale yanlış (1,1) gelir. İki sağlam alternatif: (B) ekran-uzayı türevlerinden boyut: `size ≈ fwidth(worldPos.xy) / fwidth(uv)` (matristen bağımsız, batching'de de doğru). (C) SpriteRenderer yerine quad mesh + MeshRenderer: SRP Batcher nesne başına matrisi korur. Frame Debugger'da doğrulanmalı. (Bu uyarı mimari çıkarımdır, Unity 6.3 2D renderer'da test edilmedi.)
- **Dönen dikdörtgen:** SDF nesne uzayında olduğundan dönüşte şekil bozulmaz. Ama yarım ton noktaları ve "ışık yönü parlaması" dünya uzayında hesaplanmalı (dünyaya sabit ışık), yoksa parlama tehlikeyle birlikte döner ve sıvı hissi kaybolur.
- **Çubuğa sarılı akış:** Dikdörtgende polar yerine uzunluk boyunca bir parametre kullanılır: `s = p.x` (dünya birimi), akış gürültüsü `noise(s*k - time*speed, p.y*m)`. Dünya biriminde olduğu için kısa ve uzun çubukta akış hızı ve desen yoğunluğu aynı kalır. "Sarılma" hissi için bant deseni `sin(s*k + p.y*k2 - time)` ile hafif çapraz (helis) yapılabilir.
- Alternatif `SpriteRenderer.drawMode = Tiled/Sliced + size` kullanmak bir sprite dokusu gerektirir ve UV'leri böler. Shader-öncelikli tercihe ve SDF yaklaşımına ters düşer.

### Gaps
- Unity 6000.3 2D renderer'ında Shader Graph Sprite Unlit içinde Object node Scale'inin batched SpriteRenderer'da doğru gelip gelmediğine dair resmî bir belge bulunamadı. Projede bir test sahnesinde (2 farklı ölçekli örnek + Frame Debugger) doğrulanmalı.

## 4. Partiküller: dönerken fırlayan damlacık, damlama, reset/çarpışma sıçraması; VFX Graph mi Shuriken mi

### Takeaway
2D URP'de bu ölçekte (oda başına birkaç tehlike, kısa ömürlü damlalar) Shuriken (Particle System) daha güvenli seçim: 2D sıralama katmanlarıyla çalışır ve olgun sıralama kontrolleri vardır. VFX Graph'ın 2D sprite'larla sıralaması tarihsel olarak zayıf ve geçici çözümler gerektiriyor. Damla görselleri yine SDF shader'ıyla (doku yok) çizilebilir.

### Cited Findings
- URP'de VFX Graph'ın diğer saydam nesnelerle sıralanması eski bir issue'da "Won't Fix" olarak kapandı (gerekçe: URP desteği o tarihte tamamlanmamıştı). Kaynaklar 2020–2021 tarihli, güncel olmayabilir — [Unity Issue Tracker: VFX cannot be sorted with other transparent objects in URP](https://issuetracker.unity3d.com/issues/urp-sorting-vfx-cannot-be-sorted-with-other-transparent-objects-in-urp); [Unity forum: VFX Graph Sorting Order](https://discussions.unity.com/t/vfx-graph-sorting-order/750060)
- VFX Graph'ın Sort ayarları (Auto / Depth / Custom sort key) yalnızca bir sistemin kendi partiküllerini sıralar, 2D renderer'ın sprite sıralama eksenine göre değil — [VFX Graph: Output Shared Settings](https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@17.0/manual/Context-OutputSharedSettings.html)
- Bir 2022.3 topluluk geçici çözümü: tüm sprite/VFX/partikülleri depth'e yazdırıp Z'yi Y'ye göre ofsetlemek (resmî değil) — [Unity forum: per-particle sorting in 2D with VFX graph and URP](https://discussions.unity.com/t/per-particle-sorting-in-2d-with-custom-axis-using-vfx-graph-and-urp/1637345)
- Ayrı bir issue: VFX ve sprite arasında tutarsız sıralama modu — [Unity Issue Tracker: inconsistent sorting mode with sprite](https://issuetracker.unity.com/issues/12538/inconsistent-sorting-mode-with-sprite)
- Shuriken partikülleri Renderer modülünde sorting layer/order kullanabilir. Shader Graph shader'larıyla Sorting Fudge sorunu eski bir pakette düzeltildi — [Unity Issue Tracker: sorting fudge with Shader Graph](https://issuetracker.unity3d.com/issues/urp-sorting-fudge-does-not-sort-particles-when-shader-graph-and-urp-shaders-are-used)
- 2D Renderer, 2D nesneleri saydam geçişte çizer ve bir Transparency Sort Mode/Axis'e sahiptir — [Unity 6000.3: 2D Renderer Data overview](https://docs.unity.com/en-us/engine/6000.3/manual/unity2d/2d-urp/2drenderer-data-overview)
- Çizim tarafında damlacıklar ana kütleden uzaklaştıkça küçülmeli, parçalanmalı ve akış yönüne bakmalı. Hıza bağlı uzamış damlacık stilize sıvıda hareket anlatır — [VFX Apprentice](https://www.vfxapprentice.com/blog/how-to-draw-stylized-water); [Expressive Rendering for 2D Animations of Liquids](https://diglib.eg.org/handle/10.2312/exw20251069)

### Inferences
- **Fırlayan damlacık (dönen/dairesel tehlike):** Shuriken, Shape = Circle (Arc, radius = halka dış yarıçapı, "Emit from Edge"), Simulation Space = World, Inherit Velocity (Velocity modülü) ile tehlikenin teğet hızını devralır, Gravity Modifier ~0.5–1, Render Mode = Stretched Billboard (hıza göre uzama; çizimdeki uzamış damla kuralı), Size over Lifetime azalan. Emission Rate over Distance (hareket ettikçe yayar) duran tehlikede damla üretmez, hıza orantılı görünür.
- **Damlama:** Çubuğun alt kenarında Shape = Edge, düşük oran (0.5–2/sn), Gravity 1, kısa ömür. Kameradan çıkınca/oda uyuyunca sistem duraklatılmalı (RoomManager zaten oda dışını uyutuyor).
- **Reset/çarpışma sıçraması:** `IResettable` reset anında tek seferlik Burst (8–16 damla, radyal hız, kısa ömür). Shader'da "reform" için dissolve eşiği 0→1 tween'i ile sıvı yeniden oluşuyormuş gibi görünür.
- **Okunabilirlik kuralı:** Damlalar öldürücü değildir. Bu yüzden ana gövdeden belirgin biçimde küçük, daha açık/yarı saydam ve çarpıştırıcısız olmalı ki oyuncu "yeşil = ölüm" kuralını damlalara genellemesin ya da tersine gövdeyi damla sanmasın.
- Damla materyali için aynı SDF shader'ının basit bir varyantı (yuvarlak/kapsül SDF + kontur + tek parlama) Shuriken'in vertex color'ı ile renklenir. Doku gerekmez.

### Gaps
- Unity 6.3'te VFX Graph'ın 2D renderer + sprite sorting layer desteğinin güncel durumu için resmî, güncel bir kaynak bulunamadı. Bulunanlar eski issue'lar ve forum geçici çözümleri.

## 5. Çarpıştırıcıyı dürüst tutmak (görsel öldürücü kenar = collider)

### Takeaway
2D aksiyon oyunlarında yaygın kural, tehlike çarpıştırıcısını görselden biraz küçük tutmaktır. Oyuncu "kıl payı kurtuldum" hisseder ve haksız ölüm algısı azalır. Bu projede bunun karşılığı: collider, sıvının dalgalanmanın en çekik anındaki minimum görsel kenarının içinde kalmalı. Dalgalanma yalnızca dışa doğru olmalı, damlalar ve aura öldürücü olmamalı.

### Cited Findings
- Geliştiriciler genellikle hitbox'ların bağlı oldukları sprite'tan küçük olmasını, özellikle dikenlerde, öneriyor. Platform/duvar değilse 2D oyunda hitbox'ı sprite'tan küçük yapmak tavsiye ediliyor — [itch.io gönderisi](https://itch.io/post/9301743); [itch.io gönderisi](https://itch.io/post/8415890)
- Gerekçe: Oyuncuya daha iyi kaçtığı izlenimi verir. Hitbox sprite'la aynı ya da daha büyükse çarpışma ani ve haksız hissettirebilir — [itch.io gönderisi](https://itch.io/post/2678115); [itch.io gönderisi](https://itch.io/post/5977118)
- Metanet'in *N* oyununda iki farklı hitbox var: hızlı bıçakların kutuları daha küçük. Rabi-Ribi'de karakter hitbox'ı sprite'tan çok küçük — [arama sonucu, itch.io/pressreader](https://www.pressreader.com/australia/pcpowerplay/20200803/284721979049651); [TV Tropes: Hitbox Dissonance](https://TVTropes.org/pmwiki/pmwiki.php/Main/HitboxDissonance)
- Hareket çarpıştırıcısı ile hasar hitbox'ını ayırmak, zemin çarpışmasını bozmadan hasar algılamayı ayarlamayı sağlar — [GDevelop forum](https://forum.gdevelop.io/t/animation-flickering-on-edge-of-platform/32562)
- Oyuncu incelemelerinde aşırı büyük tehlike collider'ları haksız bulunuyor — [itch.io gönderisi](https://itch.io/post/13991235)
- Phaser kılavuzunda "küçük sprite çarpışma kutuları" ayrı bir oyun mekaniği bölümü olarak işleniyor — [Phaser: Smaller Sprite Collision Boxes](https://3m-phaser.flossmanuals.net/docs/ch013)
- (Kaynakların çoğu forum/topluluk tavsiyesi. Resmî bir çalışma bulunamadı.)

### Inferences
- **Somut kural seti (bu proje için):**
  1. Collider = SDF'nin taban şekli (dalgalanmasız halka/kutu), bunun bir "fairness inset" kadar içi (ör. 0.05–0.1 dünya birimi veya yarıçapın ~%5–10'u). Değer oyuncunun gamepad testine bırakılmalı (CLAUDE.md kural 4).
  2. Kenar gürültüsü yalnızca pozitif yönde: `d = dBase - noise01 * amp`. Sıvı taban şeklinden içeri hiç çekilmez, dolayısıyla görsel hiçbir anda collider'dan küçük görünmez.
  3. Kontur (koyu dış çizgi) görselin parçası ve dışarıda kalır. Oyuncu kontura değip ölmezse kurtulmuş hisseder.
  4. Merkezdeki mekanik çekirdek (daire/çubuk) öldürücü mü? Halkada çekirdek sıvının içinde kalır ama oyuncu ortadan geçemiyorsa çekirdek + sıvı tek bir dolu disk collider'dır. Halkanın ortası boşsa ve oyuncu geçebilecekse collider olarak iki daire farkı gerekir (Unity'de tek primitive yok; EdgeCollider2D/PolygonCollider2D ile halka ya da çekirdeğin de öldürücü olduğu tasarım). Tasarım kararı insana kalır.
  5. Damlalar, aura ve parlamalar çarpıştırıcısız.
  6. Collider ile shader aynı veriden beslenmeli: shader yarıçap/boyutu scale'den okuyorsa collider da aynı transform'la ölçeklenen CircleCollider2D/BoxCollider2D olmalı. Dikdörtgende BoxCollider2D'nin `edgeRadius` özelliği SDF'deki köşe yarıçapıyla eşleşebilir.
- **Hız etkisi:** Hızlı hareket eden tehlikede görsel bir kare geride kalabilir (Update'te hareket ve FixedUpdate'te fizik). Platform stratejileri zaten interpolasyon kullanıyorsa sorun yok, kullanmıyorsa küçük inset bu farkı da yutar.
- **Debug:** Shader'a `_DebugCollider` toggle'ı eklenip taban SDF'si (collider sınırı) ince bir çizgi olarak gösterilirse görsel–collider uyumu sahnede gözle doğrulanabilir.

### Gaps
- Çarpıştırıcı/görsel oranı için yayımlanmış sayısal bir endüstri standardı bulunamadı. Değer oyun testiyle belirlenmeli.

## 6. Çok örnek için performans (MaterialPropertyBlock, SRP Batcher, 2D URP'de GPU instancing)

### Takeaway
Bu ölçekte (oda başına onlarca değil birkaç tehlike, yalnızca aktif oda + komşular açık) performans sorun değil. Yine de doğru kalıp şu: tüm tehlikeler tek materyali paylaşır, örnek başına farklılık transform scale'den ve `SpriteRenderer.color`'dan gelir. MaterialPropertyBlock kullanılmaz, çünkü 2D renderer'da SRP Batcher'ı bozar.

### Cited Findings
- Bir renderer'a MaterialPropertyBlock eklemek onu SRP Batcher ile uyumsuz yapar (resmî Unity 6.3 belgesi) — [Unity Manual 6000.3: Remove SRP Batcher compatibility](https://docs.unity3d.com/Manual/SRPBatcher-Incompatible.html)
- Unity çalışanı: MPB SRP Batcher'ı kapatır, özel MPB'li 2D renderer'lar dynamic batching'e döner. Aynı başlıkta Unity 6'da MPB'nin 2D çizimi bozduğu bir hata bildirildi ve düzeltme için işaretlendi (düzeltmenin yayımlandığına dair not bulunamadı) — [Unity forum: SRP Batcher & SpriteRenderer](https://discussions.unity.com/t/srp-batcher-spriterenderer/918871)
- `SpriteRenderer.color` batching'i bozmaz (vertex color üzerinden gider). Örnek başına bir float/renk göndermenin batching dostu yolu budur — [Unity forum: Batching and Sprite Renderers](https://discussions.unity.com/t/batching-and-sprite-renderers/792099); [bugnet blog (üçüncü taraf)](https://bugnet.io/blog/fix-unity-sprite-renderer-color-property-not-batched-mpb)
- 2D SRP Batcher shader'da `_TexelSize` erişimini desteklemez. `_ST`/TexelSize kullanan materyal 2D renderer'da SRP batching'i kapatır uyarısı verir — [Unity forum: SRP batcher warnings for SpriteRenderers](https://discussions.unity.com/t/srp-batcher-warnings-for-spriterenderers-with-urp-lit-shader/1542677)
- Unity'nin yol haritasında "MPB'nin SRP Batcher ile uyumu ya da eşdeğer bir yerine geçen" maddesi "Under Consideration" durumunda — [Unity roadmap: MPB SRP Batcher compatibility](https://activation.unity3d.com/roadmap/75-material-property-block-srp-batcher-compatibility)
- Unity 6 sürümlerinde URP 2D varsayılan shader'larının SRP Batcher uyumluluğu için düzeltmeler yayımlandı — [Unity forum özeti / Issue Tracker: sprite renderer draw calls with SRP batcher](https://issuetracker.unity3d.com/issues/sprite-renderer-does-not-issue-draw-calls-correctly-when-using-srp-batcher)

### Inferences
- **Örnek başına parametre bütçesi (MPB'siz):** (1) Boyut → transform scale (Object node veya türevden, bkz. bölüm 3). (2) Faz/rastgelelik → dünya konumundan hash: `hash(objectPosition)` veya batching'de güvenli olması için `SpriteRenderer.color.a` ya da kullanılmayan bir renk kanalı. (3) Dönme hızı/akış yönü → tehlikenin hareket stratejisi zaten biliyor. Gerekirse `color.r/g`'ye kodlanır ve shader'da renk olarak değil parametre olarak okunur.
- **Doku kullanmamak** `_TexelSize` sorununu da ortadan kaldırır (2D SRP Batcher'ın desteklemediği özellik). SDF + prosedürel gürültü bu açıdan da avantajlı.
- **Shader maliyeti:** Fragment başına birkaç SDF + 2–3 gürültü örneği (Gradient/Simple Noise) + Voronoi, ekranın küçük bir kısmını kaplayan quad'larda önemsiz. Asıl risk yarı saydam aşırı çizim (overdraw) ve quad'ın gereksiz büyüklüğü. Quad'ı dalgalanma + damla payı kadar şişirip fazlasından kaçınmak, dış bölgede `clip` yerine erken alpha = 0 kullanmak yeterli.
- **GPU instancing:** SpriteRenderer, MeshRenderer gibi standart GPU instancing yoluna girmez. 2D'de yol SRP Batcher veya dynamic batching'dir. Bu ölçekte instancing'e gerek yok. (Çıkarım. Bu oturumda Unity 6.3 belgesinden doğrulanmadı.)
- Doğrulama: Frame Debugger'da tehlikelerin tek "SRP Batch" ya da az sayıda çağrıda toplandığı görülmeli.

### Gaps
- Unity 6.3'te Shader Graph Sprite Unlit hedefinin 2D renderer'da SRP Batcher uyumluluğunun kesin durumu ve MPB hatasının düzeltilip düzeltilmediği teyit edilemedi.

## 7. Somut render tarifleri (adım adım, node/fonksiyon, geliştirici-saati tahmini)

### Takeaway
Önerilen sıra: (1) tek bir ortak "AcidHazard" Sprite Unlit Shader Graph'ı (daire ve dikdörtgen modu, keyword ya da iki küçük graph), (2) collider eşleşmesi ve debug çizgisi, (3) Shuriken damlaları, (4) yarım ton ve reform cilası. Toplam ~12–20 saat; ilk okunabilir sürüm ~5–7 saatte çıkar. Saatler bu raporun tahminidir, kaynaklı değildir.

### Cited Findings
- Tariflerin yapı taşları: Polar Coordinates node — [Unity docs](https://docs.unity3d.com/Packages/com.unity.shadergraph@8.1/manual/Polar-Coordinates-Node.html); Rounded box SDF — [Infinite Canvas](https://scour.ing/p/https://infinitecanvas.cc/guide/lesson-009); SDF şekil node'ları — [Unity Procedural Nodes](https://docs.unity3d.com/Packages/com.unity.shadergraph@17.1/manual/Procedural-Nodes.html); gürültü + eşik — [Mirza Beig dissolve](https://mirzabeig.substack.com/p/unity-tutorial-dissolve-shader-part-1); MPB/SRP Batcher — [Unity Manual](https://docs.unity3d.com/Manual/SRPBatcher-Incompatible.html); çizim kuralları — [VFX Apprentice](https://www.vfxapprentice.com/blog/how-to-draw-stylized-water); hitbox — [itch.io](https://itch.io/post/9301743); IQ SDF/smin (içerik doğrulanmadı) — [IQ 2D SDF](https://iquilezles.org/articles/distfunctions2d/), [IQ smin](https://iquilezles.org/articles/smin/)

### Inferences

**Tarif 1: Daire asit-halka tehlikesi (temel, ~4–6 saat)**
1. Shader Graph → Sprite Unlit (2D renderer, Transparent). Sprite olarak düz beyaz kare (veya quad). Quad, collider yarıçapı + dalgalanma genliği + kontur payı kadar büyük.
2. `p = (UV - 0.5) * size` (size: Object Scale ya da `fwidth(worldPos)/fwidth(uv)` yolu, bkz. bölüm 3). `r = length(p)`, `dir = p / r`.
3. Taban SDF'leri: çekirdek `dCore = r - Rcore`, sıvı diski `dLiquid = r - Rliq`. İç boşluklu halka isteniyorsa `abs(r - Rmid) - w` (onion).
4. Kenar dalgalanması: `n = GradientNoise(dir * k + time * flowSpeed * tangent)` (dikişsiz, çünkü θ yerine `dir` kullanılıyor). `dLiquid -= saturate(n) * amp` (yalnızca dışa).
5. Akış: polar θ = `atan2(p.y, p.x)`. Gövde deseni `noise(float2(θ * N/(2π) + time * ω, r * m))` (N tam sayı ise dikiş yok). Bunu 2 seviyeye posterize et (Step), koyu/açık akış bantları çıksın.
6. Bantlar (hepsi Step/Smoothstep + DDXY ile AA): dışarı alpha 0 → koyu kontur (`-t1 < d < 0`, ink'e yakın koyu yeşil) → gövde (asit yeşili) → akış bandı (bir ton açık) → sol üstte sabit dünya-yönlü sert parlama (`dot(dir, lightDir) > 0.7` ve `d` iç yakını, krem/beyaz).
7. Çekirdek: `dCore` ile koyu plum (#140C1C'ye yakın, borularla aynı aile) metal daire + ince teal (#289AA4) "alan" halkası (`abs(dCore - g) < ε`). Çekirdek dönmez ya da yavaş döner; sıvı hızlı akar. Bu kontrast "cihaz sıvıyı tutuyor" mesajını verir.
8. Renk/rastgelelik: Faz `hash(objectPos)` veya `SpriteRenderer.color.a`. MPB yok.
- Maliyet: graph kurulumu 2–3 saat, renk/bant ayarı 1–2 saat, scale/batching doğrulaması 1 saat.

**Tarif 2: Dikdörtgen asit-çubuk tehlikesi (~3–5 saat, Tarif 1'in üzerine)**
1. Aynı graph, `_Shape` keyword'ü (veya ayrı graph ama ortak Sub Graph'lar: `AcidBands`, `EdgeWobble`, `Halftone`).
2. `p = (UV - 0.5) * size`. Çekirdek çubuk `dCore = sdRoundedBox(p, half - liquidThickness, rCore)`, sıvı kılıfı `dLiquid = sdRoundedBox(p, half - pad, rLiq)` (Custom Function node'u, HLSL tek satır). Rounded Rectangle node'u kullanılmaz (UV-uzayı, uzatınca elipsleşir).
3. Kenar dalgalanması: kenar boyu parametresi `s = p.x` (dünya birimi; çubuk ince ise `p.y`), `n = noise(float2(s * k - time * speed, side))`. Kısa ve uzun çubukta aynı yoğunluk.
4. "Sarılı" akış: gövde deseni `frac((p.x + p.y * twist) * k - time * speed)` → Step ile çapraz helis bantları. Çubuk etrafında dönen sıvı izlenimi verir.
5. Bantlar, parlama ve renkler Tarif 1 ile aynı Sub Graph'tan. Parlama dünya yönünde (çubuk dönse de ışık sabit).
6. Collider: BoxCollider2D (`edgeRadius` ≈ rLiq) — boyutu taban SDF'si eksi inset.
- Maliyet: SDF custom function + boyut 1–2 saat, helis/akış ayarı 1–2 saat, dönme testi 1 saat.

**Tarif 3: Collider dürüstlüğü + debug katmanı (~1–2 saat)**
1. Shader'a `_DebugCollider` toggle'ı: taban SDF − inset sınırını 1 px kırmızı (#FC1E00) çizgi olarak çiz.
2. Küçük bir editor/runtime yardımcı bileşen collider boyutunu shader'ın kullandığı formülle aynı değerlerden kursun (tek kaynak). Değerler bir ScriptableObject'te dursun (projenin SO kalıbı).
3. Tüm hareket tiplerinde (ping-pong, dairesel, hızlanan, dönen) en hızlı örnekte gözle kontrol, sonra inset değerini gamepad testine bırak.

**Tarif 4: Damlalar ve reform (Shuriken, ~3–4 saat)**
1. Prefab'a çocuk Particle System: Shape Circle/Edge (tehlike tipine göre), Simulation Space World, Emission "Rate over Distance", Inherit Velocity ~0.6, Gravity 0.7, Stretched Billboard (speed scale küçük), Size over Lifetime 1→0, Max Particles ~20.
2. Damla materyali: aynı shader ailesinden küçük "Droplet" graph'ı (UV-uzayında daire/kapsül SDF + kontur + tek parlama, renk vertex color'dan). Doku yok.
3. Reset/çarpışmada Burst 8–16 + shader dissolve eşiği 0→1 tween'i (DOTween projede var) ile "yeniden toplanma". Yalnızca görsel, collider anında açık/kapalı mantığı mevcut sistemde kalsın.
4. Oda uyuyunca (`Room.Sleep`) partiküller zaten devre dışı kalır. Ayrıca `Stop()` gerekmez, test edilmeli.

**Tarif 5: Yarım ton baskı cilası + smin damlası (opsiyonel, ~2–3 saat)**
1. Işığa uzak bant (`dot(dir, lightDir) < -0.2`) içinde ekran- veya dünya-uzayı nokta ızgarası: `cell = frac(worldPos.xy * freq) - 0.5`, `dot = length(cell) < radius(shade)`. Projede platform yarım tonu varsa aynı fonksiyon/Sub Graph yeniden kullanılmalı (en küçük diff).
2. Halkanın altında 1–2 sarkan damla: `dDrop = length(p - float2(0, -Rliq - drop(t))) - rd`, `d = smin(dLiquid, dDrop, k)`. `drop(t)` frac(time·hız) ile döngüsel; damla ayrılınca Shuriken'den gerçek bir damla doğabilir.
3. CRT uyarısı: CRT filtresi ince detayları yediği için kontur ve parlama ≥ 2–3 ekran pikseli olmalı, yarım ton noktaları da platformlardakinden ince olmamalı.

**Türkçe sonuç (özet)**
- Okunurluk önceliği: asit yeşili gövde + koyu kontur + tek yönlü sert parlama + hızlı akış ve yavaş/sabit koyu çekirdek. Bu kombinasyon hızda bile "öldürücü sıvı" diye okunur. Yeşil oyunda zaten asit şeritleriyle "ölüm" demek.
- Shader tek ve ortak olmalı. Boyut scale'den, rastgelelik pozisyon hash'inden ya da `SpriteRenderer.color`'dan gelmeli, MaterialPropertyBlock kullanılmamalı.
- Dikdörtgenlerde UV-uzayı node'ları (Rounded Rectangle) değil, dünya biriminde rounded-box SDF kullanılmalı. 9-slice gerekmez.
- Collider taban şeklin biraz içinde kalmalı. Görsel dalgalanma yalnızca dışa olmalı, damlalar öldürücü olmamalı. Inset değeri insanın gamepad testine bırakılmalı.
- Partikül için Shuriken: 2D sıralama sorunsuz. VFX Graph 2D'de sıralama riski taşıyor ve bu ölçekte gereksiz.
- Tahmini toplam: Tarif 1+2+3 (çekirdek, okunabilir sürüm) ~8–13 saat; Tarif 4+5 cilası ~5–7 saat. CLAUDE.md'deki 1–1.5 saatlik task kuralına göre ~10–14 task'a bölünebilir.

### Gaps
- Saat tahminleri kaynaklı değil, bu raporun tahminidir. Geliştiricinin Shader Graph hızına göre değişir.
- Object Scale'in batched SpriteRenderer'da güvenilir olup olmadığı (Tarif 1/2 adım 2) projede test edilmeden kesinleşmez. Türev tabanlı yol yedek olarak önerildi.
- Halkanın ortasının oyuncu için geçilebilir olup olmadığı (collider şekli) bir tasarım kararıdır, araştırmayla cevaplanamaz.
