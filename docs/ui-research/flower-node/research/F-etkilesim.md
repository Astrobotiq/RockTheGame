# F: Etkileşimli Bitki Teknikleri ve Depoda Kanca Noktaları — Çiçek Node'u Araştırması

Tarih: 2026-11-22. Kapsam: yalnızca araştırma; kod, shader ve sahne değişmedi. Bu dosya dışında hiçbir şeye dokunulmadı.

Etiketler: **[Doğrulandı]** = sayfayı açıp okudum, bilgi orada. **[Kısmi]** = sayfa açıldı ama bilgi dolaylı/eksik. **[Depo]** = bu repodaki koddan okudum (dosya:satır). **[Çıkarım]** = kaynağa ya da koda dayanan ama bu projeye benim uyarlamam. **[Tahmin]** = hareket hissi sayısı; geliştiricinin kararı, gamepad ile ayarlanır. Telifli metin kopyalanmadı; her şey kendi cümlemle özetlendi.

---

## 0. En değerli 6 bulgu (tek cümle)

1. **Elde edilen en önemli depo bulgusu: tutunma anında el çiçeğin merkezindedir (node collider'ı merkezde, yarıçap 0,5 dünya birimi), yani "kol" çiçeğe merkezden uzanan uzun bir doğru parçasıdır ve oyuncu onun ucunda sallanır** — kolun yapraklara etkisi "elin yaprağı itmesi" değil, saat yelkovanı gibi dönen bir doğru parçasının yaprakları aralaması olarak tasarlanmalı; bunun için `ArmVisual`'a dokunmadan `PlayerController.LeftAnchor/RightAnchor` + omuz konumu yeter ([Depo] `PlayerController.cs:101-113`, `ArmVisual.cs:79-97`, `Node-Flower.prefab`).
2. **Uzanma süresi yalnızca 0,1 sn** (`ExtendDuration`), bu yüzden "uzanan eli takip eden yaprak" tutunmadan önceki nişan evresinden (reticle) beslenmeli, el yolculuğundan değil; nişan bilgisi bugün `TargetVisualizer` içinde özel kalıyor ([Depo] `ArmVisualSettingsSO.cs:35`, `TargetVisualizer.cs:35-39`).
3. **İplik şeritleri tek mesh'te ve her köşe kendi şeridinin azimutunu `TEXCOORD1.x`'te taşıyor; halka başına tek MaterialPropertyBlock var** — şerit başına CPU yayı gerekmeden, bloğa 4 float yazıp vertex shader'da `azimut` terimiyle her şeridi ayrı yönlendirmek mümkün; şerit rijit hareket ettiği için "vertex başına dünya mesafesi" tuzağına düşülmez ([Depo] `FlowerCoronaMesh.cs:46-48`, `FlowerCorona.cginc:15-23`, `FlowerBloom.cs:650-676`; tuzak için [Doğrulandı] Restaurant on Mars yazısı).
4. **Bu projenin yaprak bükmesi XY düzleminde (yaprak düzlemine dik) ve yaprak yönü (azimut) yalnızca `Pivot.localRotation` ile belirleniyor, `Apply` bunu hiç yazmıyor** — yana itme (yaw) en kolay, en okunur kanal ve 5 yaprak için CPU'da bedava; `_Pitch`/`_BaseCurl` değişimi yukarıdan bakışta yalnızca küçük boy kısalması ve gölge değişimi verir (cos 12° → cos 18° ≈ %2,7) ([Depo] `FlowerBend.cginc:23-52`, `FlowerBloom.cs:432,558,573,577`; yüzde [Çıkarım] hesap).
5. **Doğrulanmış tekniklerin ortak noktası: etkileşimi hiyerarşik ve ucuz tutuyorlar** — Crysis "ana + detay bükme", Riftbreaker "kuvvetlerin ağırlıklı ortalaması + Idle/Bending/Recovering durumları", prime31 "yay durunca güncellemeyi bırak", Thuleanx "yay hafızası"; hepsi bizim 5 yaprak yayı + birkaç skaler tasarımına doğrudan çevriliyor ([Doğrulandı] bkz. §1).
6. **`FlowerBloom` zaten her renderer'a MaterialPropertyBlock yazıyor (çiçek başına 9 renderer); Unity belgesi MPB'nin SRP Batcher ile uyumsuz olduğunu söylüyor** — yani yeni özellikler yapısal maliyet eklemiyor, ama mevcut MPB maliyeti zaten var; 8 çiçekte kare süresi ölçülmeden "bedava" denmemeli ([Doğrulandı] Unity MPB belgesi ve SRP Batcher uyumsuzluk sayfası; [Depo] `FlowerBloom.cs:240,582,604,628,676`).

---

## 1. Doğrulanmış kaynaklar ve teknikler

Aşağıda 15 sayfa açıp okudum. Her satırda: ne yapıyor, bu proje için uygunluk.

### 1.1 Shader tabanlı itme: oyuncu konumu uniform'u + mesafe falloff'u

- **Alan Zucconi, "Shader Showcase Saturday #3: Interactive Grass"** — https://www.alanzucconi.com/2018/07/28/shader-showcase-saturday-3/ **[Doğrulandı, kısa yazı]**
  - Etkileşimin vertex fonksiyonunda yapıldığı, etkileşimcinin konumunun shader'a verildiği, birkaç nesne için konum dizisinin kullanılabildiği ama bunun "iyi ölçeklenmediği, yalnızca birkaç karakterle sınırlı olduğu" söyleniyor. Ölçeklenebilir alternatif: etkileşimli nesneleri ayrı bir dokuya çizip bükmeyi dokudan okumak; bu yol kalıcı bükülmeyi de verir. Falloff formülü ve yarıçap yazıda yok.
  - **Uygunluk:** Dizi yaklaşımı bizim durumda yeterli: etkileşimci en çok 3 nokta (gövde + 2 omuz/el). Doku yöntemi gereksiz (tek oyuncu, ≤8 çiçek).
- **hexaquo, "Grass rendering series part 3: animating and interacting with grass in Godot"** — https://hexaquo.at/pages/grass-rendering-series-part-3-animating-and-interacting-with-grass-in-godot/ **[Doğrulandı]**
  - Nesne konumu ve yarıçapı uniform; her kare script yazıyor. Falloff doğrusal: `max(R - d, 0) / R` (merkezde 1, yarıçapta 0). İtme yönü = nesneden bıçağa yatay yön, tepe noktası tabana göre daha çok oynuyor, ayrıca y biraz aşağı çekiliyor. Geri dönüş yayı yok; bıçak yalnızca nesne uzaklaşınca eski haline dönüyor. Çok nesne için dizi gerektiği yazıyor ama verilmemiş.
  - **Uygunluk:** Falloff biçimi ve "ucu daha çok oynat" kuralı doğrudan geçerli (bizde: yaprak ucu tabandan fazla). Yay yokluğu bizim için yetersiz: kâğıdın "bırakınca geri sallanması" gerekiyor (§3).
- **Restaurant on Mars Studios, "Squishy Grass"** — https://restaurantonmarsstudios.github.io/mainblog/shaders/2019/03/12/squishy-grass.html **[Doğrulandı]**
  - Oyuncu konumu global vektör; en doğru sürümde son 20 karenin konumları sabit boyutlu bir global dizide shader'a gidiyor. Mesafe, **her vertex için değil nesnenin merkezi için** hesaplanıyor; çünkü vertex başına dünya mesafesi mesh'i "kırık" gösteriyor (yalnızca bazı vertex'ler oynuyor). Tek konumlu sürüm ucuz ama oyuncu yarıçaptan çıkınca ezilme birden kesiliyor; dizi sürümü en pahalı, uzunluğu kısaltılarak hızlandırılabiliyor.
  - **Uygunluk:** "Nesne merkezi başına tek değer" kuralı bizde: yaprak başına (pivot) ve şerit başına (azimut sabit) tek değer. Konum geçmişi yerine **yay durumu** aynı hafızayı CPU'da 1 float ile verir (§3).
- **Elringus/GrassBending (Unity, MIT)** — https://github.com/Elringus/GrassBending **[Doğrulandı, README]**
  - Bükme kaynağı bileşenleri (`BendGrassWhenEnabled/Visible`) belirli yarıçapta bükme veriyor; özel kaynak için arayüz var; eşzamanlı kaynak sayısı sınırlı, aşılırsa `Priority` düşük olan önce hizmet alıyor. Veri taşıma yöntemi (dizi/doku) README'de yok.
  - **Uygunluk:** "Kaynak arayüzü + öncelikle sınırlı sayı" fikri bizde `IFlowerActor` benzeri küçük bir arayüze çevrilebilir ama v1 için gereksiz (tek oyuncu).
- **Unity blog, "Creating an interactive vertex effect using Shader Graph"** — https://unity.com/blog/engine-platform/creating-an-interactive-vertex-effect-using-shader-graph **[Doğrulandı, ama konu dar]**
  - Tek bir `_Amount` float'ı C# her kare yazıyor, zıplama girdisiyle 1'e çıkıyor ve lerp ile 0'a iniyor; shader vertex'i normal boyunca öteliyor. Falloff/yarıçap anlatılmıyor, performans notu yok.
  - **Uygunluk:** "Script bir skaleri sürer, shader vertex'te kullanır" deseni bizim `FlowerBloom → MPB → shader` hattının aynısı. Shader Graph yolu bu proje için uygun değil: çiçek shader'ları elle yazılmış CG (`FlowerBend.cginc`).
- **Unity belgesi, `Shader.SetGlobalVectorArray`** — https://docs.unity.com/en-us/engine/current/script-reference/unityengine/shader/setglobalvectorarray **[Doğrulandı, kısmi]**
  - Global özellikler, malzemede tanımlı olmayan ama shader'ın istediği özellikler için kullanılıyor; `List<Vector4>` veya `Vector4[]` alıyor. Azami dizi boyutu sayfada yok (**[Doğrulanamadı]**).
  - **Uygunluk:** Gerekmez. Çiçekler zaten renderer başına blok alıyor; global dizi, "hangi çiçeğe hangi değer" sorusunu shader'a taşırdı.

### 1.2 Hafıza ve yay: ezilen ot "geri sallanır"

- **Thuleanx, "Interactive Procedural Grass"** — https://thuleanx.github.io/graphics/grass **[Doğrulandı, kısmi]**
  - Yay-kütle-sönümleyici bıçak başına değil, 2B hız tamponunun **texel'i başına** (compute shader) integre ediliyor; kuvvet = −sönüm·hız − sertlik·yerdeğiştirme, basit Euler. Etkileyici nesne, hareket yönünde, kare falloff'lu küre içinde itiyor. Vertex shader bıçağı, yerdeğiştirmeye dik bir eksen etrafında `0,3·π·şiddet` açısıyla döndürüyor. Tampon etkileşim bitince de hatırladığı için ot "karşı salınım" yapıyor. Yazar, ticari oyun için daha kararlı bir integratör öneriyor. Yazının metni ile kodun kanal düzeni birbirini tutmuyor (kod `xy=hız, zw=yerdeğiştirme` diyor); bu **[Kısmi]**.
  - **Uygunluk:** Compute + render texture bizim ölçekte aşırı. Ama hafıza mantığı (itilen şey yayla geri döner, hafifçe aşar) aynen alınır: 5 yaprak için 5 yay.
- **prime31, "Interactive 2D Foliage"** — https://prime31.github.io/grass2d **[Doğrulandı]**
  - Tetikleyici collider'la oyuncu algılanıyor; bükme CPU'da, quad'ın üst iki vertex'ini yana kaydırarak yapılıyor. Yazar, shader yolunu **batching'i bozduğu için** reddetmiş. Oyuncu çıkınca yay devralıyor ve salınıyor; yay ivmesi küçük bir eşiğin altına inince vertex'ler nötre sıfırlanıp güncelleme duruyor (boşuna mesh güncellemesi yok). Oyuncu yukarıdan hızla inerse yöne bir yay darbesi veriliyor. Rüzgâr, oyuncu bükmüyorken yaya sinüsle ekleniyor.
  - **Uygunluk:** CPU mesh yöntemi bizde uygun değil (geometri shader'da bükülüyor). Ama üç fikir alınır: (1) yay durunca güncellemeyi bırak (`UiSpring.IsRest` zaten var, `UiSpring.cs:42`), (2) iniş/tutunma anında **darbe** (Kick), (3) dinlenme eşiği.
- **Daniel Holden (Orange Duck), "Spring-It-On: The Game Developer's Spring-Roll-Call"** — https://theorangeduck.com/page/spring-roll-call **[Doğrulandı]**
  - Saf lerp sönümleyici kare hızına bağlı ve `damping·dt > 1` olunca kararsız; semi-implicit Euler yaylar da büyük dt ya da yüksek sertlik/sönümde kararsızlaşıyor. Çözüm: kapalı formlu (exact) çözüm, parametreler **yarı ömür** (half-life) cinsinden; her dt için kararlı, kare hızından bağımsız, hızlı. Kritik sönümlü yay için `x, v, hedef, yarı ömür, dt` imzası veriliyor.
  - **Uygunluk:** Projenin `UiSpring`'i 4 alt adımlı semi-implicit Euler ve `dt`'yi 1/20'ye kırpıyor (`UiSpring.cs:32-39`). Bizim sertlikler (≤700) ve dt'ler için yeterli ([Çıkarım]: ω·h = √700·(1/240) ≈ 0,11 ≪ 2, kararlı bölge). Exact yay yalnızca kare hızı bağımsızlığı sorun olursa gerekir; `UiSpring` ile uyumlu kalmak (testler, hissin tutarlılığı) daha değerli.
- **Thomas Jakobsen, "Advanced Character Physics" (Game Developer kopyası)** — https://gamedeveloper.com/programming/advanced-character-physics **[Doğrulandı]**
  - Verlet: parçacık yalnızca mevcut ve önceki konumu tutuyor, hız örtük; mesafe kısıtları iteratif relaksasyonla (konumu doğrudan düzelterek) sağlanıyor, kuvvet kullanılmadığı için sert yaylar patlamıyor; her iterasyon tüm kısıtlar üzerinde bir geçiş; kumaşta 1 iterasyon bile yetebiliyor.
  - **Uygunluk:** Yaprağın şekli shader'da parametrik eğri (`k(t)`) ve 1 serbestlik dereceli yay bunu sürmeye yeter. Verlet zinciri yalnızca "yaprak ucu tabandan geride kalsın" (yaprak içi gecikme) istenirse düşünülür; onun da ucuz karşılığı 2. bir yay (§3, F3 notu).

### 1.3 Kuvvetlerin birleştirilmesi, durumlar, hiyerarşik bükme

- **Piotr Bomak, "Creating a Vegetation System that Reacts to the Environment (part 2)", The Riftbreaker, Game Developer** — https://www.gamedeveloper.com/design/creating-a-vegetation-system-that-reacts-to-the-environment-2 **[Doğrulandı]**
  - Girdiler: global rüzgâr, yerel rüzgâr, şok dalgası, **"bükme bileşeni" olan varlıklar (mech, düşman, roket) bitkiyi iter, mesafeyle zayıflayan kuvvet**, ateş. Kuvvetler her mantık güncellemesinde **ağırlıklı ortalama** ile birleşiyor (ağırlıksız karıştırmak "glitchy" sonuç veriyor). Bitki üç durumda: Idle, Bending, Recovering. Bükme vertex shader'da parametrik karesel fonksiyonla; yay, bir fonksiyonla yaklaşıklanıyor. Model başına ayarlanır parametreler: toparlanma süresi, salınım sönümü, salınım evresi sayısı, eğim katsayısı; büyük bitkiler "ağır", küçükler "yaylı". Yay ayarı çok yinelemeli iş.
  - **Uygunluk:** Üç şey alınır: (1) kuvvetleri toplamak yerine **ağırlıklı/max birleştir** (gövde, kol, darbe), (2) durum makinesi gereksiz ama `IsRest` ile Idle ayrımı yeter, (3) yay parametreleri bir ScriptableObject'te (`FlowerReactionSettingsSO`), yapraklar şerit/tacından daha ağır.
- **Tiago Sousa, "Vegetation Procedural Animation and Shading in Crysis", GPU Gems 3 bölüm 16** — https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch16.html **[Doğrulandı]**
  - **Ana bükme**: bitkinin tamamı rüzgâr yönünde, normalize yüksekliğe göre; yakın rüzgâr kaynakları örnek başına dünya uzayında toplanıyor; fazla bükme illüzyonu bozuyor. **Detay bükme**: yapraklar ayrı; vertex rengi kanallarıyla kenar sertliği, yaprak başına faz (yapraklar eşzamanlı oynamasın) ve genel sertlik; üçgen dalgalar sinüsten ucuz. Oyuncuyla çarpışma bükmesi anlatılmıyor; rüzgâr kaynakları statik, varlığa ya da parçacığa bağlanabiliyor. Vertex başına maliyet kaynak sayısından bağımsız.
  - **Uygunluk:** Hiyerarşi bizde şöyle çevrilir: ana = çiçek gövdesi (tilt/twist), detay = yaprak (yaw) ve şerit (azimut). "Yaprak başına faz" için `Petal.Seed` zaten var (`FlowerBloom.cs:56`); yapraklar eşzamanlı oynamasın diye yay katsayısına/gecikmeye seed'den küçük sapma. "Fazla bükme illüzyonu bozar" uyarısı = genlik tavanı (§3).

### 1.4 Kemikli yöntemler ve fizik eklemleri

- **Unity 2D Animation, SpriteSkin** — https://docs.unity3d.com/Packages/com.unity.2d.animation@13.0/manual/SpriteSkin.html **[Doğrulandı]**
  - Sprite, GameObject Transform'ları kemik olarak kullanılarak deforme ediliyor. 2D Animation 10'dan itibaren CPU ve GPU deformasyon var; GPU yalnızca URP'de, SRP Batcher açıkken; belge **MaterialPropertyBlock ve Sprite Mask'in SRP Batcher ile uyumsuz olduğunu söylüyor**. CPU deformasyon dinamik batching kullanıyor ve çok sayıda düşük poligonlu nesne için; GPU, az sayıda çok poligonlu nesne için. GPU deformasyonda ShadowCaster2D gölgeleri bozuk.
  - **Uygun değil:** Çiçek bir sprite değil; shader'da bükülen prosedürel mesh. SpriteSkin sprite asset'i + iskelet ister, GPU yolu MPB'yi dışlıyor (bizim hat MPB'ye dayalı), yaprak şeklinin `FlowerBend` ile bükülmesi zaten kemikten daha ucuz ve daha kontrollü.
- **Unity, Spring Joint 2D** — https://docs.unity.com/en-us/engine/current/manual/unity2d/2d-physics/2d-joints/spring-joint-2d/reference **[Doğrulandı]**
  - İki Rigidbody tarafından yönetilen nesneyi yay gibi bağlıyor; Distance, Damping Ratio, Frequency, Connected Anchor. Maliyet notu yok.
  - **Uygun değil:** Her yaprak için Rigidbody2D + eklem, 2B fizik adımına yük; oyuncu kinematik ve fizik katmanı ayrı; yaprağı fizik motoruna bağlamak "kapalıyken tutulamaz" gibi katı kurallarla (collider kapanır) çakışma riski taşır. Aynı yay davranışı `UiSpring` ile 10 satırda ve fizik dışı.

### 1.5 Performans zemini: MaterialPropertyBlock

- **Unity belgesi, MaterialPropertyBlock** — https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/materialpropertyblock **[Doğrulandı]**
  - SRP Batcher ile uyumsuz; URP/HDRP/özel SRP'de kullanmak "büyük olasılıkla performans düşüşü" verir; aynı malzemeyle farklı küçük özellik farkı için; tek blok oluşturup yeniden kullanmak en verimlisi (projede `static _block` böyle, `FlowerBloom.cs:240`).
- **Unity, SRP Batcher uyumsuzluk sayfası** — https://docs.unity.com/en-us/engine/current/manual/analysis/graphics-performance-profiling/in-urp/reduce-draw-calls-urp/srpbatcher/srpbatcher-incompatible **[Doğrulandı]**
  - Tek tek renderer'ları SRP Batcher dışına çıkarmanın yolu, renderer'a MaterialPropertyBlock eklemek.
- **[Çıkarım]** Çiçek renderer'ları bunu zaten yapıyor. Yeni float'lar mevcut blokların içine yazılacağı için **yeni draw call ya da yeni batch kırılması getirmez**; bu bir "daha kötü olmaz" kanıtı, "iyi" kanıtı değil. 8 çiçek × 9 renderer = 72 `SetPropertyBlock` çağrısı zaten her kare çalışıyor ve `Apply` durağan çiçekte de çalışıyor (§5, görev 8).

### 1.6 Animasyon ilkeleri ve doğa

- **Wikipedia, Twelve basic principles of animation** — https://en.wikipedia.org/wiki/Twelve_basic_principles_of_animation **[Doğrulandı]**: *follow through* (gevşek parçalar ana hareket durunca da sürer, geri sallanıp yerleşir), *overlapping action* (parçalar eşzamanlı oynamaz, bazıları geriden gelir/sürükler), *anticipation* (küçük hazırlık hareketi eylemi okutur). → yaprak yayları (follow-through), yaprakların kol açısına göre sırayla tepki vermesi (overlapping), nişan evresindeki çekim (anticipation).
- **Wikipedia, Mimosa pudica** — https://en.wikipedia.org/wiki/Mimosa_pudica **[Doğrulandı]**: dokunma, sallama, üfleme ve ısınma yaprakçıkları katlıyor; sinyal dokunulan yaprakçıktan komşulara yayılıyor; birkaç dakikada yeniden açılıyor; belirli bir hız sayısı sayfada yok. → "dokununca yapraklar çekilir" doğal bir referans; ama §3'te çiçeğin dokunmaya "kapanarak" tepki vermesi, `FlowerCycle`'ın kapanma dilini (kapanış = tutulamaz) bulandırır.

### 1.7 Kısmi / ince kaynaklar (sayıya katmadım)

- 80.lv, "Unity powered interactive grass made for a game" (https://80.lv/articles/unity-powered-interactive-grass-made-for-a-game) — yalnızca "karaktere göre nesnenin dikey dönüşü değiştirildi" deniyor, teknik yok **[Kısmi]**.
- 80.lv, "Realistic plant collision logic implemented in Unity" (https://80.lv/articles/realistic-plant-collision-logic-implemented-in-unity-for-a-game) — "özel collider'lardan üstten derinlik haritası" ve "oyuncu hızıyla büyüyen rüzgâr kuvveti" ifadeleri var, uygulama detayı yok **[Kısmi]**. Fikir olarak not: hızla ölçeklenen kuvvet (§3 F4).

### 1.8 Bu projeye uygunluk özeti

| Teknik | Karar | Neden |
|---|---|---|
| Oyuncu konumu uniform'u + falloff | **Uygun, CPU tarafında** | Hesabı CPU'da yapıp çiçek/halka başına 4-5 float'ı MPB'ye yazmak; global dizi gereksiz (§1.1) |
| Vertex başına dünya mesafesi | **Uygun değil** | Mesh'i "kırar" (Restaurant on Mars). Bizde mesafe yaprak (pivot) ve şerit (azimut) başına |
| Konum geçmişi dizisi / hız tamponu doku | Gereksiz | Aynı hafıza yaprak başına yayla CPU'da bedava |
| CPU mesh vertex bükme | Uygun değil | Geometri shader'da bükülüyor; ama "yay durunca dur" ve "darbe" alınır |
| `UiSpring` (semi-implicit, projede var) | **Uygun** | Test altyapısı ve his tutarlılığı hazır (`FlowerBloomSpringTests`) |
| Exact yay (half-life) | Şimdilik değil | `UiSpring` bu sertliklerde kararlı; yalnızca kare hızı sorunu çıkarsa |
| Verlet zinciri | Şimdilik değil | 1 DoF yay yeter; yaprak içi gecikme için ikinci yay daha ucuz |
| SpriteSkin / 2D Animation | **Uygun değil** | Sprite değil prosedürel mesh; GPU yolu MPB'yi dışlıyor |
| SpringJoint2D | **Uygun değil** | Rigidbody2D + fizik adımı yükü; `FlowerCycle` collider kurallarıyla riskli |
| Shader Graph "interactive vertex" | **Uygun değil (desen uygun)** | Çiçek shader'ları elle yazılmış CG |
| Ana/detay bükme hiyerarşisi | **Uygun** | Gövde + yaprak + şerit katmanları |
| Kuvvet birleştirme (ağırlıklı) | **Uygun** | Gövde, kol, darbe aynı yaprağı iterken toplanmaz, birleştirilir |

---

## 2. Depodan doğrulanan geometri ve hat gerçekleri **[Depo]**

Bunlar tasarımı belirleyen, koddan okuduğum somut şeyler.

- **Ölçek:** `Node-Flower` kökü ölçek 4 (`Node-Flower.prefab`, kök Transform, `m_LocalScale: 4`); `petalLength` 0,95, `coreRadius` 0,22 (`FlowerBloom.cs:154-155`). Yaprak ucu ≈ 0,2 + 0,95·cos(12°) ≈ 1,13 nesne birimi ≈ **4,5 dünya birimi** (çap ~9). Oyuncu 1×1. Tacın dış halkası ≈ (0,156 + 0,564) × 4 ≈ 2,9; iç halka ≈ 1,6 dünya birimi (`FlowerBloom.cs:219,227`). Aşağıdaki "Rf" bu ≈ 4,5'tir; kodda `(petalLength + coreRadius) × lossyScale.x` ile türetilmeli, sabit yazılmamalı.
- **Kanca noktası merkezde:** `Node` çocuğu kökte (0,0,0), `CircleCollider2D` yarıçap 0,125 (×4 = 0,5 dünya), trigger (`Node-Flower.prefab` satır ~377-382). `NodeDetector` `colliders[i].transform.position`'ı hedef alıyor (`NodeDetector.cs:63,75-76`). Yani **el her zaman tacın/kulenin üstüne gider.**
- **Bükme düzlemi:** yaprak +X'e uzanır, bükme XY'de (dikey düzlem), en Z'de (`FlowerBend.cginc:7,50`); `_Pitch` yaprağı yataydan kaldırır (`:29`), `k(t) = _Turn·smoothstep + _BaseCurl` (`:36`). Yaprağın yönü (azimut) shader'da yok; `Pivot.localRotation = Euler(0, -az·Rad2Deg, 0)` olarak **yalnızca `Rebuild`'de** yazılıyor (`FlowerBloom.cs:432`), `Apply` yalnızca `localPosition` yazıyor (`:558`). → Yaw için `Apply`'a bir satır eklenmesi yeter; MPB/shader gerekmez.
- **Gölge:** yaprak ve şerit gölgesi aynı mesh'in ikinci malzemesi, **aynı MPB'yi paylaşır** (`FlowerBloom.cs:437-439,462-464`); `FlowerPetalShadow.shader:72` `FlowerBend`, `FlowerCoronaShadow.shader:69` `FlowerCoronaVertex` çağırıyor. → Yaprakta `_Pitch/_BaseCurl/Pivot` değişimi gölgeye otomatik yansır; şerit shader değişikliği `FlowerCorona.cginc` içinde yapılırsa gölge de otomatik uyar.
- **Şerit verisi:** `FlowerCoronaMesh` her köşeye `uv2 = (azimut, boy çarpanı)` yazıyor (`FlowerCoronaMesh.cs:46-48`); `FlowerCoronaVertex` konumu `sincos(uv2.x)` ile yerleştiriyor (`FlowerCorona.cginc:19-22`). Şerit başına GameObject yok; halka başına tek MPB (`FlowerBloom.cs:650-676`).
- **Açılma tek sürücü:** `open` yayı (`FlowerBloom.cs:341`), `_pop` yayı (`:242-243`), `UiSpring.Kick` var (`UiSpring.cs:28`). `FlowerBloom._spring` ve `_pop` özel; dışarıdan itmek için yeni bir `public` metot gerekir.
- **Kapalı çiçek tutulamaz:** `FlowerCycle` collider'ı yalnızca tutulabilirken açık (`FlowerCycle.cs:72-77`). `docs/flower-k4-plani.md:30-31` `FlowerCycle`, `IGrabbableNode`, collider ve itme/bırakmanın **dokunulmaz**, `FlowerBloom` genel API'sinin değişmeden kalacağını söylüyor → yeni eklemeler yalnızca **ek** API olmalı; reaksiyon sıfırken çıktı eskisiyle aynı olmalı (`flower-k4-plani.md:19`).
- **Oda uykusu:** uyuyan odanın `dynamicContent`'i kapatılıyor (`Room.cs:114-119`); orada çiçeğin `Update`'i çalışmaz, ek maliyet yok.
- **Çok oyunculu:** rakip hayalet dünyaya dokunmuyor (`docs/systems/multiplayer.md`, "Etkileşim" satırı). Çiçek yalnızca yerel `PlayerController`'a tepki vermeli.
- **Allocation uyarısı:** `NodeDetector.TryFindBestNode` her çağrıda `new RaycastHit2D[1]` ayırıyor (`NodeDetector.cs:59`). Çiçekler bunu **çağırmamalı**; nişan bilgisi `TargetVisualizer`'ın mevcut çağrısından alınmalı (§4).

---

## 3. Bu projeye uyarlama: fikirler

Ortak kural **F0 (reaksiyon kapısı):** çiçek yalnızca tutulabilirken ve açıkken tepki verir. `gate = grabbable ? smoothstep(kenar0 = 0,5; kenar1 = 0,9; open) : 0` (`FlowerCycle.IsGrabbable` `:32`, `FlowerBloom.Open` `:254`). Neden: kapalı tomurcuğun oyuncuya "bakması", tutulabilir olduğunu ima eder; oysa tutulamaz-taş okuması bu oyunun temel kuralı (`flower-k4-plani.md:42`). Uyarı fazında (`Warn01 > 0`) genlik azalır: `gate *= 1 − 0,5·warn` **[Tahmin]**, çünkü uyarı titremesi ve kesikli halka okunurluğu taşıyor.

Genel sayı çerçevesi (hepsi **[Tahmin]**, gamepad ile ayarlanacak): yay `ζ = damping / (2·√sertlik)` (proje kuralı, `FlowerBloom.cs:99`). Aşağıdaki sertlik/sönüm değerlerinin aşım ve yerleşme süreleri, `UiSpring.Step` ile birebir aynı şemayla (4 alt adım, 1/240 sn) Python'da simüle edildi (Unity'de değil):

| Yay | sertlik / sönüm | ζ | aşım | %2 yerleşme |
|---|---|---|---|---|
| Yaprak yay (önerilen) | 300 / 17 | 0,49 | %17 | 0,47 sn |
| Yaprak yay (daha "kâğıt", az aşım) | 300 / 24 | 0,69 | %5 | 0,34 sn |
| Yaprak yay (lastik gibi, **kaçın**) | 420 / 12 | 0,29 | %38 | 0,65 sn |
| Kavrama (yumruk) | 700 / 36 | 0,68 | %5 | 0,22 sn |
| Kavrama, bırakışta "çırpınma" | 700 / 22 | 0,42 | %24 | 0,31 sn |
| Yön yayı (gecikmeli kol açısı) | 120 / 15 | 0,68 | %5 | 0,54 sn |

Kâğıt kesmesi dili için **ζ ≥ 0,5 ve en fazla bir görünür aşım** önerilir **[Çıkarım]**; 0,3 ve altı lastik/jöle okunur.

### F1: Kavrama yumruğu (şeritler tutan ele sarılır)
- **Giriş:** `grip01`: bu çiçeğin node'una tutunmuş kol sayısı/2 (0, 0,5, 1) yaylanmış.
- **Hesap:** saf; mevcut `FlowerCoronaMath.Evaluate(open, warn01, ...)` (`FlowerCoronaMath.cs:79-94`) sonuna isteğe bağlı `grip01 = 0` parametresi: `pitch += GripLift·grip`, `curl += GripCurl·grip`, `radius *= 1 − GripPinch·grip`, `length *= 1 − GripShorten·grip`. `grip01 = 0` iken çıktı birebir eskisi (mevcut `FlowerCoronaMathTests` yeşil kalmalı).
- **Çıktı:** halka MPB'sindeki `_Pitch`, `_BaseCurl`, `_BaseRadius`, `_Length` (`FlowerBloom.cs:651-659`). **Shader değişmez.**
- **Yay:** 700/36 (tutarken), bırakışta 700/22.
- **Başlangıç sayıları [Tahmin]:** `GripLift 0,35 rad (~20°)`, `GripCurl 0,3`, `GripPinch 0,25`, `GripShorten 0,1`.
- **Maliyet:** 1 yay + birkaç madde; sıfıra yakın.
- **Risk:** "Venüs kapanı kapanıyor" gibi okunabilir (kapanma = tehlike dili). Çözüm: şeritler **hafifçe** kalkar ve ele yaslanır, yapraklar (asıl "açık" işareti) kıpırdamaz; kule önde kalır. Mimosa referansı doğal ama kapanma dilini bulandırır (§1.6).

### F2: Yakalama darbesi (kule kabarır, çiçek bir an çekinir)
- **Giriş:** tutunma anı (kenar), kol sayısı.
- **Hesap:** saf değil, tek çağrı: `FlowerBloom.Poke(float centerKick, float openKick)` → `_pop.Kick(centerKick)`, `_spring.Kick(openKick)`.
- **Çıktı:** orta nokta `popScale` (`FlowerBloom.cs:594-598`) ve `open` yayı (twist, ölçek, taç açıklığı hepsi `open`'dan türüyor, `:539-541`).
- **Sayılar (simülasyonla [Çıkarım], başlangıç [Tahmin]):** `_pop.Kick(+4)` → kule en fazla +%15 büyür, yerleşme 0,37 sn (`+6` → +%23). `_spring.Kick(−2)` → `open` en fazla ≈ −0,077 dip (çiçek hafif büzülür, 2° ters twist), 0,22 sn'de yerleşir; `+2` aynı büyüklükte **aşım** verir (çiçek ~%4 büyür, yapraklar 2° yatar). "Çekinme" mi "karşılama" mı geliştiricinin kararı; iki işaret de tek parametre.
- **Maliyet:** sıfır.
- **Risk:** `open` yayı `warn` hedefiyle de sürülüyor (`SetWarn`, `:289-293`); darbe uyarı fazında gelirse küçük bir sıçrama eklenir. Sınır: `|open − hedef| < 0,12` ile kıskaçla. `FlowerCycle` mantığına etkisi yok (tutulabilirlik zamandan, `FlowerCycle.cs:48-70`).
- **Not:** kol uzanma animasyonundaki "yumruk" referansı: `GrabPunch = 7` (`ArmVisualSettingsSO.cs:50`); aynı ölçek, el büyürken kule de pop yapar, iki darbe birbirini destekler.

### F3: Kol süpürmesi (kol, yaprakları saat yelkovanı gibi aralar) — en değerli fikir
- **Giriş:** çiçek merkezi `C`, tutunan kolun omuz konumu `S` (`ArmController.transform.position`), kol açısı `θ_arm = atan2(S − C)` dünya, sonra çiçek-yerel azimuta çevrilir; her yaprağın azimutu `az_i` (`FlowerBloom.PetalAzimuth` `:508-512`). Hız hesaba gerekmez; geçişte yay hafızası "iz" bırakır.
- **Çevirme (tuzak noktası):** `local = model.InverseTransformDirection(new Vector3(dx, dy, 0))`, `az = atan2(local.z, local.x)`. Bu, `viewTilt`, `orientationDeg`, `twist` ve kök dönüşünü otomatik katar. Elle işaret türetmeyin: yaprak azimutu `Euler(0, −az, 0)` ve `localPosition = (cos az, 0, sin az)` kuralıyla kurulu (`:432,558`); `viewTilt = 90` iken yerel +Z dünya +Y'ye, yerel +X dünya +X'e gidiyor, yani `az = θ_dünya + (orientationDeg + twist)` [Çıkarım, koddan türetildi; PlayMode/EditMode testi şart, §6].
- **Hesap (saf):**
  ```
  float PartingYaw(float petalAz, float armAz, float halfWidth, float maxYaw)   // radyan, işaretli
      d = WrapPi(petalAz - armAz); x = d / halfWidth;
      return |x| >= 1 ? 0 : maxYaw * 6.75f * x * (1 - |x|)^2;      // x=±1/3'te tepe = maxYaw
  float PartingDip(float petalAz, float armAz, float halfWidth, float maxDip)   // radyan, ≥ 0
      return maxDip * Falloff(|WrapPi(petalAz - armAz)|, halfWidth);
  float Falloff(float d, float r) { t = saturate(1 - d / r); return t * t * (3 - 2 * t); }
  ```
  Yaw tek yönlü (tek işaretli) değil, **tek fonksiyon** (odd): kolun iki yanındaki yapraklar zıt yöne itilir (saç taramak gibi); tam altındaki yaprak yaw almaz ama `PartingDip` ile aşağı bastırılır.
- **Yarım genişlik:** 5 yaprak 72° arayla; `halfWidth = 50°` (0,87 rad) ile kol hangi açıda olursa olsun en az bir yaprak menzilde (en uzak komşu 36° < 50°) [Çıkarım, 360/5].
- **Çıktı:** yaprak başına `yaw` → `p.Pivot.localRotation = Euler(0, −(az + yaw)·Rad2Deg, 0)` (`FlowerBloom.cs:558` sonrasına); `pitch −= dip` (`:560`). Pivot konumu sabit kalır (yaprak tabanında menteşe).
- **Başlangıç [Tahmin]:** `maxYaw 12° (0,21 rad)` → uçta ≈ 4,5·0,21 ≈ **0,95 dünya birimi** yana kayma (oyuncu eninin ~1 katı, belirgin ve okunur), `maxDip 10° (0,17 rad)`, yay 300/17, **yaprak başına küçük sapma:** `sertlik *= 1 + (Seed − 0,5)·0,2` (Crysis'in "yaprak başına faz" fikri; yapraklar eşzamanlı oynamasın).
- **Hafıza/iz:** hedef anlık değerdir; yay gecikmeli tepki verir. Kol yelkovan gibi dönerken (sallanma) yapraklar sırayla bir "çarpma" alır ve geriden sallanır, *overlapping action* + *follow-through* bedavaya gelir.
- **Maliyet:** CPU: 5 yaprak × (WrapPi, 2 küçük polinom, 1 yay) ≈ çiçek başına birkaç yüz flop; 8 çiçek bile ihmal edilebilir [Çıkarım, ölçülmedi]. GPU: yok.
- **Risk:** (1) Yaw ≥ ~14°'de komşu yapraklar çakışır/açıklık oluşur (yaprak eni 0,48, komşusuyla zaten örtüşüyor, `FlowerBloom.cs:156`); tavan 12-14°. (2) Kol, oyuncu sprite'ı ve kol çizgisi çiçeğin **önünde**; yapraklar kolun *altında* kayıyor, okunur. (3) Kâğıt kesmesi dili: yaprak kendi tabanında menteşe, genlik küçük, ζ ≥ 0,5 → "kâğıt rüzgâr yedi" okunur, jöle okunmaz. (4) Çift tutunma (iki kol aynı çiçekte) kayma/slingshot'a gider (`SwingingState.cs:497-505`); v1'de iki kol ayrı doğru parçası olarak toplanır (max), karışıklık beklenmez.

### F4: Gövde yakınlığı (oyuncu çiçeğin üstünden geçerken yapraklar yol verir, hıza göre)
- **Giriş:** gövde konumu `B` (`PlayerRigidbody.position`/`transform.position`), hız `v` (`PlayerController.Velocity`, `:61`).
- **Hesap (saf):** yaprağın uç noktası `T_i` (dünya: pivot + yönlü uzunluk·cos(pitch) ya da yalnızca `C + Rf·dir(az_i)·0,85`), uzaklık `d = |T_i − B|`, `w = Falloff(d, bodyRadius)`; itme yönü `away = (T_i − B)/d`, yaprağın teğet yönü `tan_i = perp(dir(az_i))`; `yaw_i = maxYawBody · w · dot(away, tan_i) · speedScale`; `dip_i = maxDipBody · w`. `speedScale = clamp(|v| / vRef, 0,3, 1)` (hızla büyüyen kuvvet; 80.lv yazısındaki fikir **[Kısmi]**).
- **Başlangıç [Tahmin]:** `bodyRadius = 0,5·Rf ≈ 2,2 dünya`, `maxYawBody 8° (0,14)`, `maxDipBody 6° (0,10)`, `vRef = Stats.MoveSpeed` (okunacak, sabit yazılmayacak), yay 300/17.
- **Çıktı:** F3 ile aynı kanallar; F3 ile **toplama yok**, `yaw = Mathf.Abs(a) > Mathf.Abs(b) ? a : b` (Riftbreaker: karıştırmayı ağırlıkla yap, topla değil).
- **Maliyet:** 5 yaprak, F3 ile aynı.
- **Risk:** oyuncu çiçeğin önünden geçerken yaprakları "okumadan" her geçişte oynatmak, tutulabilirlik algısını gürültülendirir; kapı (F0) ve genlik küçüklüğü bunu sınırlar. Hızlı geçişte (dash) yaprağın aşırı savrulması: `speedScale` tavanı.

### F5: Şerit yönlenmesi (iplik tacı tutan ele/gövdeye eğilir) — shader değişikliği gerekir
- **Giriş:** F3/F4 ile aynı yön `θ` (çiçek-yerel azimut, yay ile geciktirilmiş: *yön yayı*, açıyı sürekli, sarma yapmadan izler), `lean` genliği (çekim şiddeti), `yaw`, `dip` genlikleri.
- **Hesap:** CPU yalnızca 4 float üretir (halka başına aynı `Vector4 _React = (dir, lean, yaw, dip)`); şerit başına hesap shader'da (şerit azimutu zaten var):
  ```
  float d  = WrapPi(az - _React.x);
  float lean = -_React.y * cos(d);                         // oyuncu tarafındaki şeritler yatar, karşı taraf kalkar (taç eğilir)
  float x  = d / _ReactHalf;  float prof = abs(x) < 1 ? 6.75*x*(1-abs(x))*(1-abs(x)) : 0;
  az  += _React.z * prof;                                  // aralama (yaw): şerit azimutu kayar, taban ve yön birlikte döner
  pitch += lean - _React.w * Falloff(abs(d), _ReactHalf);  // yatma + bastırılma
  ```
  `FlowerBendL`'e iki opsiyonel parametre (`pitchBias`, `curlBias`) eklenmeli; sarmalayıcı `FlowerBend(...)` sıfır verir, yaprak shader'ları ve gölge etkilenmez (`FlowerBend.cginc:54-57`).
- **Çıktı/bağlama:** `FlowerBloom.ApplyCorona` içinde `_block.SetVector(ReactId, ...)` (`FlowerBloom.cs:660` yakınına). Gölge malzemesi aynı bloğu ve `FlowerCoronaVertex`'i kullandığı için otomatik uyar (`FlowerCoronaShadow.shader:69`).
- **Neden azimut sabit şerit:** bir şeridin tüm köşeleri aynı `uv2.x`'e sahip, yani şerit **rijit** hareket eder; Restaurant on Mars'ın "vertex başına mesafe mesh'i kırar" tuzağı yok.
- **Başlangıç [Tahmin]:** `lean 0,25 rad (~14°)` en çok, `yaw 0,35 rad`, `dip 0,2`, `_ReactHalf 0,8 rad`; yön yayı 120/15.
- **Maliyet:** GPU: halka başına ~1300 köşe × (1 cos + birkaç madde + mevcut 14 adımlı döngü), gölge dahil iki geçiş; çiçek başına ~2600 köşe, 8 çiçekte ~21 bin; mevcut döngünün yanında ihmal edilebilir [Çıkarım, ölçülmedi]. CPU: yön yayı 1 adet.
- **Risk:** (1) Shader dosyası değişimi `flower-k4-plani.md` "bükme kodu değişmez" kuralına dokunuyor (`:19`); sıfır girişte çıktı birebir eski olmalı, FlowerTest'te `open 0/0,3/0,6/1/1,12` karşılaştırması şart. (2) Şeritler çok kısa (dış 2,9, iç 1,6 dünya) ve kule üstte; etki küçük olabilir, F1 ile örtüşür. İlk sürüme F5 olmadan (F1+F3) başlanıp şeritte yetmezse eklenmeli.

### F6: Salınım torku (oyuncu çiçeğin etrafında dönerken çiçek kendi ekseninde hafifçe sürüklenir)
- **Giriş:** oyuncunun çiçek merkezi etrafındaki açısal hızı `ω_p = (r × v) / r²` (r = B − C).
- **Hesap (saf):** `twistTarget = clamp(gain · ω_p, ±maxDeg)`, yay 120/15 ile izlenir (gecikme = sürükleme).
- **Çıktı:** `FlowerBloom.cs:539`: `twist = openTwist·(1−open) + reactTwist`.
- **Başlangıç [Tahmin]:** `maxDeg 6°`, `gain 2°/(rad/s)`.
- **Maliyet:** 1 yay.
- **Risk (yüksek):** `SpiralNode` ayrı bir node türü ve **oyuncuyu motor gibi çeviriyor** (`Nodes/Spiral/SpiralNode.cs`); dönen çiçek "spiral node" gibi okunur ve çiçeğin döndürmediği halde döndürüyormuş izlenimi verir. Genlik küçük, gecikmeli ve yalnızca tutunma sırasında olmalı. İlk sürüme **koyma**, gamepad testinde istenirse ekle. Kule `_TowerAngle = −orientationDeg` ile dönüş iptal ediyor (`FlowerBloom.cs:603`); twist eklenirse kule de dönecek, bu istenip istenmediği karar.

### F7: Nişan çekimi (tutunmadan önce)
- **Giriş:** bu çiçeğin nişan hedefi olup olmadığı (sol/sağ). Bugün `TargetVisualizer` bunu hesaplıyor ama dışarı vermiyor (`TargetVisualizer.cs:35`, çağırdığı aşırı yükleme node'u atıyor; node'lu aşırı yükleme `NodeDetector.cs:41` var).
- **Hesap:** `aim01` yaylanır; F0 kapısı ile çarpılır.
- **Çıktı:** (a) yapraklar çok az yukarı kalkar (`pitch += AimLift·aim`, "dikilme"), (b) şeritler çok az açılır (`F1`'in tersi: grip yerine `grip = −0,15·aim`), (c) kule hafif şişer (`_pop` hedefinin üstüne küçük ek).
- **Başlangıç [Tahmin]:** `AimLift 4°`, süre 0,15 sn yay.
- **Maliyet:** 1 yay.
- **Risk:** reticle zaten hedefi gösteriyor; çiçeğin de hareket etmesi çift sinyal, ama kullanıcının istediği "uzanan elin çekimi" yalnızca bu evrede algılanabilir (el 0,1 sn'de varıyor, `ArmVisualSettingsSO.cs:35`). Hedef bilgisini **çiçeğin kendi hesaplaması** yerine `TargetVisualizer`'dan almak şart: `NodeDetector` engel raycast'i ve "en iyi tek hedef" kuralını uyguluyor (`NodeDetector.cs:61-79`); çiçeğin kendi nokta çarpımı testi farklı hedef seçebilir.

### F8: Çekim eğimi (halat gerilimi çiçeği oyuncuya doğru yatırır)
- **Giriş:** `B − C`, hız, halat uzunluğu `r`.
- **Hesap (saf):** `pull01 = saturate((v²/r) / (g · k))` (merkezcil ivme/yerçekimi oranı), yön = `B − C`.
- **Çıktı:** `model.localRotation`'a küçük ek eğim (`FlowerBloom.cs:540`), ya da yalnızca F5'in `lean`'i.
- **Risk:** yukarıdan bakışta (ortografik, `viewTilt = 90`) 8-12° eğim yaprak boyunu yalnızca %1-2 kısaltır; görünür etki **kule/taç paralaksı** (yükseklik 0,3-0,5 nesne birimi × sin 12° ≈ 0,1 nesne ≈ 0,4 dünya). Düşük getiri, derinlik/sıralama riski (yapraklar derinlik yazıyor, `depthBehind`, `FlowerBloom.cs:192,543`). F5'in `lean`'i aynı etkiyi şeritlerde ucuza verdiği için F8 ayrı yapılmamalı.
- **Merkezi sabit tut:** el merkezde, çiçek modelini ötelemek kulenin elden kopmasına yol açar (node collider'ı ve anchor kök/`Node`'dan okunuyor, model değil; `NodeDetector.cs:63`, `PlayerController.cs:312-316`). Öteleme yerine yaw/eğim.

### F9: Bırakış geri tepmesi (follow-through)
- **Giriş:** kolun tutunmadan çıkması (anchor node'un `null` olması).
- **Hesap:** ayrı hesap yok; `grip01` hedefi 0'a döner, yaylar (700/22) aşar, yapraklar kol açısı değiştikçe yerleşir. **Zorla bırakış** (çiçek kapanırken `ReleaseByNode`, `PlayerController.cs:279-287`) sırasında reaksiyon kapısı (F0) `open`/`grabbable` düşünce hızla sıfıra iner; kapanış snap'i baskın kalır.
- **Risk:** zorla bırakışta kapı sönerken yaylar sıfırlanırken görsel sıçrama; kapıyı anında kesmek yerine yaylara `Target = 0` verip `Kick`'i yok saymak yeter.

### Ek: iki kanal bilerek önerilmedi
- **Yaprak başına `_Reach/_Turn` (rulo) ile tepki:** rulo, açılma/kapanma dilinin kendisi (`flower-k4-plani.md:19`); etkileşimle sürülürse açık/kapalı okunurluğu bozulur.
- **Sürekli rüzgâr/idle sallanma:** mevcut dil kesin (kâğıt, titreme yalnızca uyarıda). Boşta hareket eklemek uyarı titremesini gürültüye boğar. Kaynaklardaki rüzgâr (prime31, GPU Gems) bilerek alınmadı.

---

## 4. Depo kanca noktaları **[Depo]** (dosya:satır)

### (a) Oyuncu ve iki elin dünya konumları

| Veri | Nereden | Not |
|---|---|---|
| Gövde konumu | `PlayerController.PlayerRigidbody.position` (`PlayerController.cs:55`, kullanım `:224,297`); `transform.position` (`ArmVisual.cs:147,172`) | Kinematik Rigidbody2D; `Rigidbody2D.position` fizik adımında güncellenir, `Update`'te basamaklı görünebilir; interpolation durumunu doğrulamadım **[Doğrulanamadı]** |
| Gövde hızı | `PlayerController.Velocity` (`:61`, `FixedUpdate`'te yazılıyor `:205`) | Sallanırken **node'a göre** (`SwingingState.cs:327-329,499`). Çiçek statik node olduğu için dünya hızıyla aynı; hareketli node'a konursa (`hareketli-node-plani.md`) fark çıkar, o zaman konum farkından hız türetin |
| Omuz konumu | `PlayerController.LeftArm/RightArm` (`:65-66`) → `ArmController.transform.position` (`ArmVisual.cs:63,79`) | `ArmController` yalnızca `AimDirection` tutuyor (`ArmController.cs:10-16`) |
| Tutunan elin konumu | `PlayerController.LeftAnchor/RightAnchor` (`:101-110`, `Vector2?`); tutunmuşken el anchor'da (`ArmVisual.cs:97`: `_handPos = Lerp(_extendStart, anchor, progress)`, `progress` 0,1 sn sonra 1) | **`ArmVisual`'a dokunmadan yetiyor** |
| Boşta elin konumu | `ArmVisual._handPos` özel (`ArmVisual.cs:25`), `hand.position` (`:178`) | Boşta el omuzdan en fazla `IdleReach = 1` dünya birimi (`ArmVisualSettingsSO.cs:11`); yani gövde konumundan anlamlı fark vermez. İstenirse `public Vector2 HandPosition => _handPos;` tek satır |
| Hangi çiçeğe tutunuldu | `PlayerController.LeftAnchorNode/RightAnchorNode` (`:112-113`) | Çiçek için `anchorNode != null && anchorNode.IsChildOf(transform)`; mevcut desen `GetComponentInParent<IGrabbableNode>` (`:291`) |

`ArmVisual`'ın oyuncu altında mı sahnede ayrı mı durduğunu doğrulamadım (prefab/sahne YAML'ına bakmadım; `NewScene.unity` içinde geçiyor); bu v1 için gerekmiyor.

### (b) Tutunma/bırakma olayı

- **Tutunma, tek darboğaz:** `PlayerController.AttachAnchor` (`PlayerController.cs:228-255`); çağıranlar `NormalState.cs:188` (yerden atış), `NormalState.cs:479` (havadan), `SwingingState.cs:491` (ikinci kol), `WallSlidingState.cs:168,178`. Çift tutunmada `DualSwingingState`'in kendisi çağırmıyor (grep sonucu); geçiş `SwingingState`'ten (`:497-505`).
- **Bırakma, çok nokta:** `LeftAnchor = null` / `RightAnchor = null` atamaları `SwingingState.cs:509-510,515-516,533-534`, `DualSwingingState.cs:79-156`, `NormalState.cs:98-99`, `SlingshotState.cs:141-142`, `PlayerController.cs:408-409` (respawn). Tek ortak nokta anchor özelliklerinin **setter'ı** (`PlayerController.cs:104,109`: `LeftAnchorNode = null` da orada).
- **Zorla bırakış:** `ReleaseUngrabbableNodes` (`:279-287`) → `INodeReleaseHandler.ReleaseByNode` (`SwingingState.cs:469-477`).
- **Çiçek tarafı:** `FlowerCycle.IsGrabbable` (`:32`), `GrabbableChanged` olayı (`:36,76`), `Phase` (`:31`), `ReleaseImpulse` (`:33`).
- **Sonuç:** Bırakma için **yoklama (poll) daha sağlam**: her `Update`'te `anchorNode` bu çiçeğin çocuğu mu? Olay bunu setter'a koymayı gerektirir. Tutunma için de yoklama yeter (`grip01` hedefi anchor'ın varlığından gelir; "kenar" `grip01`'in yükselişinden de okunur). Olay yalnızca ses/titreşim gibi başka sistemler "tutundu" sinyalini isterse gerekir.

### (c) Çiçek oyuncuyu hangi bileşenden bilir

Üç seçenek, projenin tercihi `CLAUDE.md`: yeni **ayrık sinyal** için `EventChannelSO`, ayar için ScriptableObject.

| Seçenek | Artı | Eksi |
|---|---|---|
| **1. Doğrudan sorgu (önerilen v1):** `FlowerReactor` `OnEnable`'da önbelleğe alınmış `FindFirstObjectByType<PlayerController>()`; kaybolursa yeniden bulur | Projede emsal var (`AirborneCoinShardManager.cs:110,133,175`, `Collectible.cs:74`); sürekli veri (konum, anchor) olay değil, her kare okunur; `PlayerController` aynı assembly (`RockTheGame.Runtime`), asmdef değişmez | Çiçek→oyuncu bağı sertleşir (PlayerController zaten çiçeğe `GetComponentInParent` ile bakıyor, `:291`, yani karşılıklı) |
| **2. EventChannelSO (ayrık sinyaller için):** `NodeGrabEventChannelSO` (kol, node, nokta, tutundu/bırakıldı) `AttachAnchor` ve anchor setter'ında yükseltilir (`PlayerController.cs:228,104,109`); çiçek dinler, node'un çocuğu mu filtreler | CLAUDE.md tercihiyle uyumlu; ses/titreşim/çiçek aynı kanaldan beslenir; Unity'nin SO mimarisi yazısı bu deseni (event kanalı + sistemler birbirini bilmez) öneriyor ([Doğrulandı] https://unity.com/how-to/architect-game-code-scriptable-objects) | Konum gibi **sürekli** veriyi taşımaz; yine bir kaynak (1 veya 3) gerekir; setter'a olay eklemek `PlayerController`'ı değiştirir |
| **3. SO "çalışma zamanı çıpası":** oyuncunun kendini kaydettiği SO (Transform tutan) | Singleton'dan kaçınır (aynı Unity yazısı "singleton yerine SO referansı" diyor) | Yeni asset + oyuncuya kayıt kodu; Death sistemindeki `TransformEventChannelSO` (`Death/TransformEventChannelSO.cs`) bir olay kanalı, çıpa değil; yani birebir emsal yok |

**Öneri:** v1 = (1) + yoklama; `FlowerReactionSettingsSO` ile ayarlar SO'da. Ses/titreşim isteği çıkarsa (2) eklenir, çünkü o zaman gerçekten çok dinleyici olur. Çok oyunculu hayalet bu hattın dışında kalır (`PlayerController` yalnızca yerel oyuncuda).

### (d) `FlowerBloom.Apply`'da etki hangi noktada eklenir (`FlowerBloom.cs`)

| Etki | Satır | Eklenecek |
|---|---|---|
| Çiçek yayı itme (F2) | `:303-311` (`Snap`) yanı | yeni `public void Poke(float centerKick, float openKick)` → `_pop.Kick`, `_spring.Kick` (`UiSpring.cs:28`) |
| Reaksiyon durumu API'si | `:289-300` (`SetWarn`/`SetHint`) yanı | `public FlowerReactionState Reaction { get; set; }` (reactor'ın önceden ayırdığı diziler, kopya/allocation yok) |
| Gövde dönüşü/twist (F6, F8) | `:539-543` | `twist += reaction.TwistDeg`; eğim varsa `model.localRotation`'a ek |
| Yaprak yaw (F3, F4) | `:558` hemen sonrası (`p.Pivot.localPosition` yazıldıktan sonra) | `p.Pivot.localRotation = Quaternion.Euler(0f, -(az + yaw) * Mathf.Rad2Deg, 0f)` |
| Yaprak pitch (F3, F4, F7) | `:560` | `pitchDeg -= dip·Rad2Deg`; `+= aimLift` |
| Yaprak curl | `:577` | `openCurl * prog + reaction.PetalCurl[i]` (isteğe bağlı) |
| Orta pop/kule (F2, F7) | `:594-598` | `pop`'a ek terim, ya da `_pop.Kick` yeter |
| Şerit yumruğu (F1) | `:645-646` | `FlowerCoronaMath.Evaluate(..., reaction.Grip01)` |
| Şerit yönlenmesi (F5) | `:650-676`, `_block.SetFloat(WarnId...)` `:661` yanı | `_block.SetVector(ReactId, reaction.Corona)` |
| Reaksiyon adımını **önce** çalıştırma | `:329` (`Update`) | Reactor'a `[DefaultExecutionOrder(-50)]`; `FlowerBloom.Update` içinde `Apply` çağrısı `:374` |

`FlowerBloom` `[ExecuteAlways]` (`:18`): reactor yalnızca `Application.isPlaying` iken oyuncuyu okur; editör önizlemesi için reactor'a "önizleme kol açısı / grip" slider'ları konabilir (FlowerTest sahnesinde gamepad olmadan ayar).

---

## 5. Önerilen mimari (özet)

- `Flower/FlowerReactMath.cs` (saf, statik): `Falloff`, `WrapPi`, `PartingYaw`, `PartingDip`, `BodyYaw`, `Gate`, `RopePull`, `UnwrapAngle`.
- `Flower/FlowerReactionSettingsSO.cs`: yay katsayıları, genlikler, yarıçaplar (CLAUDE.md "ayar SO'da").
- `Flower/FlowerReactionState.cs`: düz sınıf; `PetalYaw[]`, `PetalDip[]`, `Grip01`, `Corona (Vector4)`, `TwistDeg` (okunabilir, test edilir).
- `Flower/FlowerReactor.cs` (MonoBehaviour, `RequireComponent(FlowerBloom)`, `[DefaultExecutionOrder(-50)]`): oyuncuyu bulur, anchor/gövde/omuzu okur, `FlowerReactMath` ile hedefleri üretir, `UiSpring`'lerle sürer, durumu `FlowerBloom`'a verir; `IsRest` + uzaklık ön koşuluyla (oyuncu `Rf + marj`'dan uzak ve tüm yaylar dinlenirken) **erken çıkış** (prime31 deseni).
- `FlowerCoronaMath.Evaluate`'a isteğe bağlı `grip01`.
- (F5) `FlowerCorona.cginc` + `FlowerBend.cginc`: opsiyonel bias parametreleri.

Dosya konumu `Assets/Scripts/New-Scripts/Flower/` (yorumlar Türkçe, `flower-k4-plani.md:35`).

---

## 6. Test edilebilirlik

### EditMode (saf hesap; `Assets/Tests/EditMode/`, mevcut `FlowerBloomSpringTests.cs` deseni)
1. `Falloff`: d ≥ r → 0, d = 0 → 1, monoton, [0,1] içinde.
2. `PartingYaw`: tek fonksiyon (`yaw(+x) = −yaw(−x)`), `|d| ≥ halfWidth` → 0, `|yaw| ≤ maxYaw`, sarma güvenli (π civarı sıçramasız), tam kol altında 0; `PartingDip` çift fonksiyon ve ≥ 0.
3. 5 yaprak, `halfWidth = 50°`: kol 0-360° taranırken **her açıda** en az bir yaprağın etkisi > ε (kapsama testi).
4. `BodyYaw`: yönü `away`'e göre işaretli, `speedScale` tavanı.
5. `Gate`: kapalı/tutulamaz → 0; `open` arttıkça monoton; uyarıda azalır.
6. `FlowerCoronaMath.Evaluate(..., grip01: 0)` **eski çıktıyla birebir** (mevcut `FlowerCoronaMathTests` değişmeden geçer); `grip01 = 1` → pitch ve curl artar, radius/length azalır.
7. Yay "kâğıt mı lastik mi" testi: önerilen 300/17 için aşım `[%10, %25]`, yerleşme < 0,6 sn; ζ ≥ 0,45 (`FlowerBloomSpringTests.cs:21-31` `Simulate` kopyası).
8. Kare hızı: aynı yay 30/60/144 fps'te (dt kırpması 1/20, `UiSpring.cs:32`) benzer yörünge (±%5).
9. **Azimut çevirme testi (en önemli işaret tuzağı):** gerçek bir `FlowerBloom` (EditMode'da `OnEnable` → `Rebuild` çalışır, `FlowerBloom.cs:321-327`) kurup, rastgele `orientationDeg`/`twist`/kök dönüşü için `WorldToLocalAzimuth(dünyaYönü)` sonucunun ilgili yaprağın `Pivot` dünya yönüyle (Pivot'tan uç doğrultusu) hizalandığını doğrula.
10. `UnwrapAngle`: ±π geçişinde sıçrama yok (yön yayı için).

### PlayMode (`Assets/Tests/PlayMode/`, `FlowerNodeRulesTests.cs:31-53` `SpawnFlower` deseni ve `PlayerRuleTestBase.cs:148,206` yardımcıları)
Reactor durumunu **MPB'den değil, reactor'ın okunabilir özelliklerinden** (`PetalYaw[i]`, `Grip01`, `Corona`) oku; `FlowerNodeRulesTests` çiçeği malzemesiz kuruyor, `Apply` bu durumu `IsBuilt` ile zaten güvenli geçiyor (`FlowerBloom.cs:530`) ama reactor da null-güvenli olmalı.
1. **Yaklaşınca tepki:** oyuncu açık çiçeğin 2 birim yanında bekler; birkaç kare sonra `max |PetalYaw| > ε`; oyuncu 25 birim uzaklaşınca ~sn içinde `< 1e-3`.
2. **Uzak çiçek etkilenmez:** aynı sahnede 20 birim uzakta ikinci çiçek: tüm kareler boyunca `|PetalYaw| < 1e-4` ve `Grip01 = 0` (aynı anda iki çiçek, odada zincir senaryosu).
3. **Tutununca değişir:** `GrabExistingWithRight` sonrası 0,3 sn içinde `Grip01 > 0,5`; `Corona.x` (yön) oyuncu yönüyle ±10° hizalı; tetik bırakılınca ~1 sn içinde `Grip01 < 0,05`.
4. **Kapalı çiçek sessiz:** `Timings(closed: 5, open: 5)` ile yanındaki oyuncuya rağmen tüm reaksiyon büyüklükleri ≈ 0 (F0 kapısı).
5. **Mevcut kurallar bozulmaz:** `ClosingFlower_ReleasesTheGrapple`, `ClosingFlower_PushesThePlayerAway`, `OpenFlower_CanBeGrabbed`, `ClosedFlower_CannotBeGrabbed` (`FlowerNodeRulesTests.cs:55-110`) reactor bileşeni ekliyken de yeşil. Reactor çiçeği **yalnızca okur**.
6. **Sallanırken titreşim var:** swing sırasında `PetalYaw` zamanla değişir (varyans > ε); yaprakların hepsi aynı anda değil (seed sapması).
7. **Zorla bırakışta sıçrama yok:** kapanış anında `Grip01` ve yaw yaylarının kare başına değişimi sınırlı (süreklilik).
8. Performans (otomatik değil): 8 çiçekle `FlowerReactor.Update` toplamı Profiler'da kayıt altına alınır; eşik testi flaky olur.

---

## 7. Önerilen task listesi (5-8 madde, 1-1,5 saat, bağımlılık sırasıyla)

`CLAUDE.md` planlama kuralları: bir güne en fazla 4 task, `#code` ile `#test` aynı/ertesi gün, haftada bir tampon. **`TASKS.md`'ye eklemedim; eklemeden önce sizin onayınız gerekir.** Due tarihleri planlamada belirlenir.

1. `- [ ] **Çiçek tepkisi: saf hesap + EditMode testleri** #code` - `FlowerReactMath` (Falloff, WrapPi, PartingYaw/Dip, BodyYaw, Gate, UnwrapAngle) + testler §6.1-6.5, 6.10; 1,5 sa.
2. `- [ ] **Çiçek tepkisi: grip01 (taç yumruğu) + yay testi** #code` - `FlowerCoronaMath.Evaluate`'a `grip01 = 0` (eski testler birebir yeşil), `FlowerReactionSettingsSO` + varsayılan asset (`unity command eval`), yay testi §6.6-6.8; 1,5 sa.
3. `- [ ] **FlowerReactor: oyuncu/kol okuma + yaylar (görsel yok)** #code` - `FindFirstObjectByType` önbelleği, anchor/omuz/gövde okuma, F0 kapısı, F3+F4 hedefleri, yaylar, okunabilir `FlowerReactionState`, `IsRest` erken çıkış; azimut çevirme testi §6.9; 1,5 sa.
4. `- [ ] **FlowerBloom'a reaksiyon bağlama (yaw, pitch, grip, Poke)** #code` - `Reaction` özelliği, `Apply`'da §4(d) satırları, `Poke`, `FlowerTest`'te `open` 0/0,3/0,6/1/1,12 karşılaştırması (reaksiyon 0 iken önceki ile aynı); 1,5 sa. (3'ten sonraki gün.)
5. `- [ ] **Çiçek tepkisi: PlayMode testleri** #test` - §6 PlayMode 1-6 + mevcut `FlowerNodeRulesTests` yeşil; `run_tests --mode playmode`; 1,5 sa.
6. `- [ ] **Çiçek tepkisi: gamepad ve görsel deneme + sayı ayarı** #test` - F1-F4 açıkken FlowerTest ve gerçek odada; yay/genlik **[Tahmin]** değerleri ayarlanır (eski/yeni değerler yazılır), F6/F8'in ve F5'in gerekip gerekmediğine karar verilir; 1 sa. (geliştirici)
7. `- [ ] **(İsteğe bağlı) Şerit yönlenmesi shader'ı (F5)** #code` - `FlowerCorona.cginc`/`FlowerBend.cginc` bias parametreleri, `_React` MPB yazımı, sıfır girişte eski görünüm doğrulaması; 1,5 sa. Yalnızca 6'dan sonra yetersiz kalırsa.
8. `- [ ] **(İsteğe bağlı) Nişan çekimi (F7) + durağan çiçekte Apply atlama** #code` - `TargetVisualizer`'dan hedef node'u okunur yapma (`NodeDetector.cs:41` aşırı yüklemesi), `aim01`; ayrıca tüm yaylar dinlenirken `Apply`'ı atlama ve 8 çiçekli Profiler ölçümü; 1,5 sa.

Önerilen gün dağılımı (zorunlu değil): Gün A: 1, 2, 3. Gün B: 4, 5, 6. Gün C: tampon, gerekirse 7-8.

---

## 8. Doğrulanamayanlar

- **Zelda: Breath of the Wild çim tekniği:** Nintendo'nun yöntemini anlatan bir kaynak bulamadım. Bulunanlar topluluk yeniden yapımları ve "Age of Calamity geliştiricileri BotW çimini yeniden üretmek için Nintendo'ya danıştı" haberleri; teknik yok. BotW atıfı yapmadım.
- **Journey, Ori, Hollow Knight, Rain World (bitkiler), Celeste, TUNIC** bitki/çim tepkileri: aramalar geliştirici açıklaması getirmedi. Rain World için Joar Jakobsson'ın prosedürel animasyonu genel olarak doğrulandı ama bitkilerin nasıl hareket ettirildiği ve Verlet kullanımı bulunmadı. TUNIC "tilt grass" yazısı (Medium, Roojack) 403 verdi, okunamadı. Hollow Knight için hiçbir şey bulunamadı.
- **Open Project 1 `TransformAnchor`:** yalnızca arama özetinde gördüm (sınıfın SO olduğu, `isSet`/`OnAnchorProvided`); kaynak dosyasını açamadım (yönlendirme), bu yüzden §4(c) seçenek 3'te doğrulanmış olarak sunmadım.
- **Unity 2D Renderer + MPB = dinamik batching'e düşer** iddiası yalnızca bir forum cevabından arama özetinde; sayfayı açmadım. SRP Batcher uyumsuzluğunu belge sayfalarından doğruladım, 2D Renderer ayrıntısını değil.
- **`Shader.SetGlobalVectorArray` azami boyutu:** belge sayfasında yok.
- **Thuleanx yazısı:** metin ile kodun kanal düzeni uyumsuz; hangisinin doğru olduğunu doğrulamadım (kullandığım bilgi düzenden bağımsız).
- **Crysis bölümü:** vertex rengi kanalı ayrıntıları özetleyici modelin okumasından; orijinal kitap bölümünün tam metnini ben satır satır okumadım.
- **Jakobsen'in özgün PDF'i** (CMU kopyası) ikili akış olduğu için okunamadı; Game Developer kopyasını okudum.
- **80.lv yazıları** uygulama detayı içermiyor (§1.7, [Kısmi]).
- **Depo tarafında doğrulanmayanlar:** oyuncu Rigidbody2D interpolation ayarı; `ArmVisual`'ın sahnede oyuncunun altında durup durmadığı; `PlayerController.Velocity`'nin tüm durumlarda dünya hızı olup olmadığı (swing'de node'a göre, yalnızca kodu okudum, oynamadım).
- **Hesaplar test edilmedi:** `az = θ_dünya + (orientationDeg + twist)` türetimi, yaprak ucu 4,5 dünya birimi, yay aşım/yerleşme sayıları (Python replika, Unity değil), maliyet tahminleri (ölçülmedi) ve "5 yaprakta en az biri 50° içinde" geometrisi **[Çıkarım]**. Editör içinde hiçbir şey çalıştırılmadı.
- **Hareket hissi:** tüm genlik, yay ve süre sayıları **[Tahmin]**; gamepad testi olmadan "doğru" denemez.

---

## Yöntem notu

- Araçlar: WebSearch ve WebFetch; depoda Read/Grep/Glob. WebFetch yanıtı küçük bir model tarafından sayfa üzerinden özetleniyor; ham metni değil özeti okudum. Sayfada olmayan şeyi model "yok" dediğinde bunu **[Kısmi]** ya da **Doğrulanamayanlar** olarak işaretledim. Yalnızca açıp özetini okuduğum sayfaları referans saydım; arama sonucu özetlerinde kalan hiçbir şeyi kaynak saymadım.
- Doğrulanmış kaynaklar: Zucconi, hexaquo, Restaurant on Mars, Elringus/GrassBending, Unity Shader Graph bloğu, Unity `SetGlobalVectorArray`, Thuleanx, prime31, Orange Duck, Jakobsen (Game Developer), Riftbreaker (Game Developer), GPU Gems 3 Crysis, SpriteSkin, SpringJoint2D, MaterialPropertyBlock, SRP Batcher uyumsuzluk sayfası, Unity SO mimarisi yazısı, Wikipedia (animasyon ilkeleri, Mimosa pudica). Birkaçı yüzeysel (Zucconi, Unity Shader Graph); gerekçelerini bölümlerinde yazdım.
- Yay sayılarını `UiSpring.Step` ile aynı şemayı (4 alt adım, dt 1/240) kopyalayan bir Python simülasyonuyla hesapladım; Unity'de çalıştırmadım. Kod değiştirilmedi, sahne açılmadı, Unity Editor'e bağlanılmadı.
- Projede tüketilmiş çizgiler (Bauhaus, op-art, halftone, genel "folk") öneri olarak sunulmadı. Spiral'e kayma riski (F6) açıkça işaretlendi.
- Hareket hissi geliştiricinin kararıdır (`CLAUDE.md` kural 4): sayılar başlangıç tahminidir; eski/yeni değerler ayar sırasında yazılmalı, gamepad ile denenmeli.
