# Fırlatma pedi: neon, notaya tepki veren görsel (araştırma)

Tarih: 2026-11-21. Konu: `LaunchPad` her fırlatmada rastgele bir nota (her seferinde farklı enstrüman) çalsın, pedin neon kenarı o notaya göre **renk ve şekil** değiştirsin. Referans: elektrik mavisi + pembe, dairesel spektrum çubuklu "neon visualizer". Ses tarafı (nota/enstrüman seçimi, sentez) kardeş dosyanın konusu; bu dosya yalnızca görseli ele alıyor. Unity'de hiçbir şey denenmedi, mockup çizilmedi; sayılar proje dosyalarından okundu, gerisi kaynak ve çıkarım.

## 0. Projeden okunan kısıtlar (önce bunlar)

| Ne | Değer | Neden önemli |
|---|---|---|
| Pedin bugünkü kimliği | Kırmızı (GDD §8.1, oda 16) | Çiçek araştırması kırmızıyı "ped rezervi" saydı (`flower-node/oneri.md`, K3 riski). Ped her vuruşta renk değiştirirse **pedi ped yapan şey renk olamaz**; çerçeve/silüet taşımalı. |
| Arka plan | Gökyüzü `#71BAEA`, zemin turuncu, asit parlak yeşil | Elektrik mavisi neon gökyüzü üstünde boğulur (aynı hue ailesi). Neonun okunması için **koyu bir gövde** şart. |
| Oyuncu rengi | Pembe `#FF51DE` | Referanstaki neon pembe tam oyuncunun rengi. Fırlatma anında oyuncu pedin üstünde: pembe neon = oyuncu pede karışır. |
| Koyu mürekkep | `#140C1C` (palette.md) | Neon gövdesi için hazır, kaynaktan örneklenmiş koyu. Çiçek K1 "Mürekkep Neon" da aynı fikri kullanıyor. |
| Bloom | Sahne profili (`Assets/Scenes/NewScene/Global Volume Profile.asset`) yalnızca `CrtFilterVolume` içeriyor, **Bloom override yok**. `DefaultVolumeProfile.asset`'te Bloom var ama `intensity 0`, threshold 0.9. | Bugün oyunda bloom **yok**. HDR açık (`m_SupportsHDR: 1`). Bloom eklemek global bir karar. |
| CRT | `CrtRendererFeature`, `AfterRenderingPostProcessing`'de; sahnede `pixelResolution 480×360`, blur 0.5, scanline 0.1 | Bloom (post-process) CRT'den **önce** olur; parıltı da CRT pikseline kuantize edilir. Kamera ortho size 6.4 → dikeyde 12.8 birim / 360 px ≈ **28 CRT pikseli/birim**. 2 birimlik ped ≈ 56 px genişlik. 1 px'lik çizgi blur ile kaybolur; neon çizgi **≥2 CRT pikseli (~0.07 birim)** olmalı. |
| Renderer | `Renderer2DData` (URP 2D), Shader Graph kullanılmıyor (yalnızca TMP'ninkiler) | Projede tüm efektler elle yazılmış shader (`SpiralNode`, `RisoSlab`, `HazardAcid`...) + `MaterialPropertyBlock` (`SpiralNode.cs`, `RisoSlabVisual.cs`, `FlowerBloom.cs`, `AcidReaction.cs`). Aynı yolu izlemek tutarlı. |
| Hafıza notları | Efekt için önce shader; halftone pürüzsüz baskı, pixel art değil; UI zevki Bauhaus + brutalist | Desen texture değil prosedürel shader olmalı. |

## 1. Nota → renk eşlemesi

### Seçenekler

| Yaklaşım | Nasıl | Artı | Eksi |
|---|---|---|---|
| **Scriabin / clavier à lumières** | 12 lamba beşli çemberi sırasında: C kırmızı, G turuncu, D sarı... | Tarihsel, beşli çemberi yakın tonlara yakın renk verir | Sentezi algısal değil, çağrışımsal; araştırmacılar Scriabin'in gerçek sinestet olduğundan şüpheli. Komşu beşliler komşu hue: pentatonik (5 komşu beşli) yalnızca ~150° hue kaplar, renkler birbirine yakın kalır. |
| **Newton (Opticks)** | 7 renk ↔ 7 diyatonik nota (dorian sırası) | Tarihsel | Kromatik değil, keyfi; oyun için anlamsız |
| **Kromatik çember** (pitch class × 30°) | C=0°, C#=30°... | Basit | Yarım ton = yakın renk, oyuncu ayırt edemez |
| **Beşli çemberi × 30°** | Scriabin'in düzenli hali | Uyumlu aralıklar komşu renk | Yukarıdaki dar-pentatonik sorunu |
| **Frekans → log hue** | `hue = frac(log2(f / f0))`, bir oktav = tam tur | Fiziksel, oktav denkliği korunur | Kromatik çemberle aynı sorun; ayrıca sürekli ton, ayrık okunmaz |
| **Ayrık palet / derece** | Ölçek derecesi başına sabit, el seçimi bir renk | Oyuncu "bu turuncuydu, o maviydi" diye ayırt eder; yasak tonlar (oyuncu pembesi, asit yeşili) atlanır | "Bilimsel" değil, ama oyun okunurluğu için en iyisi |

**Enstrüman → şekil/doku**: Kandinsky'nin "her sesin rengi, dinamiği, şekli var" fikri (Rez'in çıkış noktası) burada iş görür. Tını ailesi şekli seçer, perde rengi seçer:

| Enstrüman ailesi | Tını | Şekil önerisi |
|---|---|---|
| Plucked / mallet (marimba, harp, kalimba) | Keskin atak, hızlı sönüm | Kısa, dişli çubuklar; tek atımlık halka |
| Bell / glock | Parlak, inharmonik | Yıldız/çok kollu Chladni, uzun sönüm |
| Pad / string | Yumuşak atak | Akan dalga, yuvarlak Lissajous |
| Square/chip synth | Kare dalga, chiptune | Köşeli, basamaklı çubuklar (Bauhaus kare) |
| Bass / tom | Düşük, kalın | Az sayıda kalın çubuk, büyük tek halka |

### Öneri (oynanış okunurluğu için)

1. **Ses tarafında pentatonik** (kardeş dosyanın kararı, ama görseli de etkiler): rastgele nota her zaman uyumlu duyulur ve yalnızca **5 renk** gerekir. 5 renk akılda kalır, 12 kalmaz.
2. **Ayrık 5 renk, derece başına**, beşli çemberi sırasıyla ama hue'lar eşit aralıklı yayılmış (Scriabin'in mantığı, dar-hue sorunu olmadan). Yasak bölgeler: oyuncu pembesi (~313°, ±20°), asit yeşili, gökyüzü mavisi (koyu gövde varsa gevşer).
3. **Oktav → parlaklık/boyut** (yüksek nota daha ince ve parlak desen, alçak nota kalın).
4. **Enstrüman → şekil ailesi** (yukarıdaki tablo), **(n, m) desen parametresi → perde**. Böylece renk körü oyuncu da şekil farkını görür.

Başlangıç paleti (gözle seçildi, CRT'de denenmedi, hepsi `#140C1C` gövde üstünde):

| Derece (C pent.) | Renk | Hex (HDR öncesi) | Not |
|---|---|---|---|
| C (1) | Kırmızı-mercan | `#FF3B3B` | Pedin eski kırmızısına köprü; "varsayılan" ton |
| G (5) | Amber | `#FFB31A` | Halka sarısına yakın (`#FCAE04`), boğulmaz çünkü gövde koyu |
| D (2) | Elektrik camgöbeği | `#2EF2FF` | Referansın mavisi; gökyüzünden ayrışması koyu gövdeye bağlı |
| A (6) | Mor-mavi | `#7C5CFF` | Pembe yerine referansın "mor" ucu |
| E (3) | Sıcak beyaz | `#FFF4D6` | En yüksek değer; renk körlüğünde de ayrışır |

Referansın pembesi isteniyorsa oyuncudan ayrılması için `#FF2E8A` (daha kırmızı, ~330°) ve **yalnızca kenar çizgisinde**, dolguda değil. Bu açık soru (§6).

## 2. Frekans → şekil: ~2 birimlik pedde, 0.3–0.6 sn içinde

Okunurluk ölçütü: oyuncu pedin üstünden fırlarken gözü pedde değil, varacağı yerde. Desen **ilk 100 ms'de** şeklini vermeli, sonra sönmeli. 56×(~14–28) CRT pikselinde ince detay ölür.

| Desen | Nasıl görünür | Maliyet | 0.3–0.6 sn'de okunurluk | Not |
|---|---|---|---|---|
| **Radyal / doğrusal spektrum çubukları** | Ped kenarından yukarı fışkıran 8–16 neon çubuk, boyları bir "spektrum" zarfı (nota temel frekansı + harmonikler). Dairesel versiyon referansa en yakın. | Çok ucuz: shader'da `floor(x*N)` + yükseklik fonksiyonu | **Çok iyi.** Yükselip düşen çubuk saniyenin onda birinde okunur. | Ped düz bir şerit: dairesel spektrum ancak ped üstünde yarım daire (fan) olarak sığar. Çubuk sayısı/aralığı enstrümandan, zarf tepe noktası perdeden. Gerçek FFT gerekmez; notanın harmonik dizisinden sahte zarf yeter. |
| **Chladni / cymatics** | Ped yüzeyinde (ya da üstünde yarım daire içinde) kum çizgisi gibi düğüm hatları. Formül: `cos(nπx)cos(mπy) − cos(mπx)cos(nπy)`, `abs(v) < eşik` çizgi; n≠m olmalı. | Ucuz: piksel başına 4 cos | **Orta.** n, m ≤ 4–5 iken 56 px'te okunur; yüksek modlar CRT bulanıklığında griye döner. 2:1 en-boy pedde kare plaka formülü gerilir, `x` ölçeklenmeli. | En "bilimsel" ve en özgün. Nota → (n, m) tablosu, enstrüman → kenar koşulu (cos/sin varyantı) veya çizgi kalınlığı. Bauhaus geometrisine çok yakın. Oyuncu "desenin farklı olduğunu" görür ama hangi notaya ait olduğunu bilmez: dekoratif bilgi. |
| **Lissajous / osiloskop** | Ped üstünde bir neon düğüm: frekans oranı aralıktan (3:2 beşli, 2:1 oktav, 5:4 büyük üçlü). Rasyonel oranlar kapalı eğri verir. | Shader'da eğriye mesafe pahalı (örnekleme döngüsü, 64–128 nokta); LineRenderer ile ucuz (bkz. §3) | **İyi**, eğer düşük oranlar (≤ 5:4) seçilirse; şekiller birbirinden belirgin farklı. | Tek nota tek frekans: oranı **notanın tonik ile aralığından** almak gerekir (C'ye göre G = 3:2). Pentatonikte 5 farklı figür: 1:1 (daire/elips), 3:2, 9:8 karmaşık, 5:4, 5:3. 9:8 küçük pedde karmakarışık; tabloda sadeleştir. Eşit tamperde eğri kapanmaz ve döner: aslında güzel bir "yaşayan" efekt. |
| **Ped kenarında duran dalga (tel)** | Pedin üst kenarı gitar teli gibi titrer: `sin(kπx)·cos(ωt)·e^(−t/τ)`, k = harmonik numarası. | En ucuz; vertex ya da fragment'te kenar ofseti | **Çok iyi** ve fiziksel olarak "ped yaylandı" hissiyle birleşir; mevcut squash/stretch'i tamamlar. | Tek başına renk/şekil çeşitliliği az (yalnızca düğüm sayısı 1–5). Diğer desenlerle birlikte "taban katman" olarak ideal. |
| **Halka dalgaları (ripple)** | Temas noktasından dışa açılan 1–3 neon halka, aralık perdeye göre. | Ucuz (`frac(r*k − t)`) | **Çok iyi** ama şekil çeşitliliği az. | Tetris Effect / Rez'in vuruş halkası. Halka **dünya uzayında** pedin dışına taşabilir (ayrı quad). |
| **Kaleydoskop / domain-warp patlaması** | Pedin üstünde yarım daire içinde, açı katlanmış (`atan2` → `mod(θ, 2π/N)`) ve gürültüyle bükülmüş renk akışı. N simetri = enstrüman, renk kayması = perde. | Orta (gürültü 2–3 oktav) | **Zayıf–orta.** Çok güzel ama hızlıyken "renkli leke" okunur; şekil bilgisi kaybolur. | Psikedelik yön için. Renk değişimi dar alanda kalmalı (fotosensitivite, §3.6). |

**Sonuç:** okunurluk sırası: duran dalga ≈ spektrum çubukları ≈ ripple > Lissajous > Chladni > kaleydoskop. Özgünlük sırası tersine yakın. Pratik çözüm katmanlamak: **taban = kenar teli + ripple** (her zaman aynı, "ped tepki verdi" der), **imza = spektrum / Chladni / Lissajous** (nota başına değişen kısım).

## 3. Unity URP 2D uygulama notları

### 3.1 Shader Graph mı, elle mi?
Elle yazılmış HLSL. Gerekçe: projede tüm görsel shader'lar elle yazılmış ve `[PerRendererData]` + `MaterialPropertyBlock` desenini kullanıyor (`SpiralNode.shader`); Chladni/spektrum gibi formüller Shader Graph'ta node spagettisi olur; CRT'nin piksel ızgarasına göre çizgi kalınlığını `fwidth`/sabit piksel ile ayarlamak kodda daha kolay. Shader Graph'ın tek avantajı (görsel önizleme) bu projede kullanılmıyor.

### 3.2 Neon kenar
- Pedin dikdörtgeninin **SDF**'i (`sdRoundBox`), kenarda ince çekirdek çizgi + `exp(-d*k)` hale. Çekirdek beyaza yakın, hale renkli: gerçek neon tüpleri merkezde beyaz görünür; tek renk düz çizgi "neon" okunmaz.
- **İki yol:**
  1. **Sahte parıltı (önerilen ilk adım):** hale shader'da çizilir, Bloom gerekmez. Global post-process ayarına dokunmaz, ped dışında hiçbir şeyi etkilemez, CRT'nin 480×360 kuantizasyonuyla öngörülebilir.
  2. **HDR + Bloom:** renk `[HDR]` özellik, yoğunluk >1; sahne Volume'üne Bloom eklenir, **threshold ≥ 1.0** ki yalnızca HDR değerler parlasın. Risk: bugün oyunda bloom yok, eklenince başka şeyler (beyaz UI, krem halkalar, spiral `_Glow`) parlayabilir; her sahne çekimiyle karşılaştırma gerekir. Bloom CRT'den önce olduğu için hale kare piksel blok olarak çıkar ve CRT blur 0.5 bunu yumuşatır: muhtemelen hoş, ama denenmeli.
- Çizgi kalınlığı dünya birimiyle değil **CRT pikseliyle** düşünülmeli: ≥2 px ≈ 0.07 birim (kamera zoomu değişirse yeniden hesap). Sahnede kamera başka ortho kullanıyorsa (Cinemachine) değer değişir.

### 3.3 CRT etkileşimi
- Scanline 0.1 ve blur 0.5: ince yatay neon çizgi scanline'a denk gelirse titreşir. Yatay çizgileri 2 px yap, ya da çizgiyi CRT ızgarasına snap etme (bilerek hafif kayma = "tüp" hissi, ama test edilmeli).
- Doymuş camgöbeği ve kırmızı CRT'de daha da doyar (palette.md: kanallar +15…+35). Neon değerleri CRT'li çekimde ayarlanmalı, kaynak hex'e göre değil.

### 3.4 Lissajous: LineRenderer mı, shader mı?
- **LineRenderer:** 64–128 nokta, CPU'da bir kez hesaplanır (nota başına sabit eğri), sonra yalnızca faz/sönüm animasyonu. Kalınlık ve uç yuvarlama hazır; renk `startColor/endColor`. Yan maliyet: ayrı bir renderer, sorting order ayarı, HDR renk için kendi materyali.
- **Shader:** eğriye en yakın mesafe için döngü (64 adım/piksel), quad küçük olduğundan kaldırılabilir ama gereksiz. Alternatif: Lissajous'u parametrik değil, `|sin(a·u)−x|` kapalı form yaklaşımıyla çizmek; düşük oranlarda çalışır, yüksek oranlarda bozulur.
- Öneri: **LineRenderer**. Neon haleyi ince ikinci bir LineRenderer (kalın, düşük alfa, aynı renk) ya da Bloom verir.

### 3.5 Per-pad parametreler (MaterialPropertyBlock)
`SpiralNode.cs` / `RisoSlabVisual.cs` deseni: tek paylaşılan materyal, ped başına MPB. Önerilen özellikler:

| Özellik | Tip | Kaynak |
|---|---|---|
| `_NoteColor` | Color [HDR] | Derece → palet tablosu (SO) |
| `_PatternN`, `_PatternM` | float | Nota → (n, m) tablosu (SO) |
| `_ShapeFamily` | float (enum) | Enstrüman → şekil |
| `_PulseTime` | float | Fırlatma anı (`Time.time`); shader `t = _Time.y − _PulseTime` ile kendi sönümünü hesaplar, her kare MPB yazmaya gerek kalmaz |
| `_Octave` | float | Parlaklık/kalınlık |

Haritalar kodda değil bir ScriptableObject'te olmalı (CLAUDE.md: tunable'lar SO'da). Nota seçimini ses sistemi yapar, görsel seçilen notayı bir olayla alır (ör. `LaunchPad.onLaunch` yerine nota bilgisi taşıyan bir event channel, projedeki `*EventChannelSO` desenine uygun). Hit-stop sırasında efektin durması isteniyorsa `_Time` yerine script'ten ilerletilen zaman gerekir (`SpiralNode._Angle` gibi).

### 3.6 Eşlik eden dokunuşlar
- **Parçacık patlaması:** mevcut `launchVFXPrefab`'in `ParticleSystem.MainModule.startColor`'ı notanın rengine ayarlanır (instantiate sonrası). Az sayıda (8–16), kare/çizgi parçacık; renk neon ile aynı, çekirdek beyaz.
- **Oyuncu tint'i:** oyuncunun `PlayerColorController`'ı fırlatmada zaten `ResetAllColors` çağırıyor; üstüne 100–150 ms nota renginde kenar parıltısı. Oyuncu pembesiyle karışmaması için yalnızca kontur/rim, dolgu değil. Dash/slingshot renk sinyalleriyle (varsa) çakışıyor mu kontrol edilmeli.
- **Ekran tint'i:** önerilmez ya da çok hafif (vinyet kenarında %5–10, tek atım). Tam ekran renk değişimi fotosensitivite riski.

### 3.7 Erişilebilirlik
- **Fotosensitivite (WCAG 2.3.1):** saniyede 3'ten fazla flaş yok, ya da flaş eşik altında kalmalı. Ped tek seferlik bir patlama; ama bir odada üst üste peddan pede zıplama 3 Hz'i geçebilir (cooldown 0.2 sn = teorik 5 Hz tek pedde). Kurallar: (a) parlaklık değişimi yalnızca ped alanında, ekranın küçük bir payında; (b) tam ekran flaş yok; (c) kaleydoskop yönünde renk dönüşü yavaş (<3 Hz) ve doygun kırmızı↔başka geçişler sınırlı ("red flash"); (d) ayarlarda "efektleri azalt" anahtarı düşünülmeli.
- **Renk körlüğü:** bilgi şekilde olmalı. Renk paleti deuteranopide asit yeşili–turuncu zemin zaten karışıyor (çiçek araştırması bulgusu). Önerilen paletteki 5 rengin değerleri (L*) de ayrık seçildi (beyaz > amber > camgöbeği > kırmızı > mor) ki gri tonlamada da ayrışsın; ölçülmedi, ölçülmeli.
- **Pedi tanıma:** renk her vuruşta değiştiği için pedin "boştaki" hali sabit ve tanınır olmalı (sabit silüet + sabit boştaki renk). Aksi halde oyuncu pedi diğer platformlardan ayıramaz.

## 4. Oyun referansları: neyi çalmalı

| Oyun | Ne yapıyor | Çal |
|---|---|---|
| **Rez** (Mizuguchi, 2001) | Tüm efekt sesleri müziğe kuantize; vuruşlar müziğe katman ekliyor. "K-Project" adı Kandinsky'den. | Notanın **ölçekte** ve mümkünse ritimde kalması; şekil-renk-ses üçlüsü Kandinsky'den. Tel-çerçeve neon dili. |
| **Tetris Effect** | Her hamle bir nota/perküsyon, parçacıklar müziğe tepki veriyor | Patlamanın "sahnenin müziğine ait" hissi; parçacık rengi = sesin rengi. |
| **Geometry Dash** | Pad/orb renkleri etkinin türü ve gücünü kodluyor (sarı, pembe, kırmızı, mavi...) | **Ters ders:** GD'de renk = mekanik anlam. Bizim pedde renk değişiyor ama mekanik değişmiyor; oyuncu "farklı renk = farklı zıplama" sanmamalı. Ped silüeti sabit kalmalı, renk yalnızca imza katmanında. |
| **Thumper** | Ritim çarpışmasında krom + neon, ekranın merkezinde patlama | Tek atımlık, sert atak, hızlı sönüm. 0.3 sn'lik zarf. |
| **Just Shapes & Beats** | Pembe = tehlike, her şey ritimle şekil değiştiriyor; oyuncu camgöbeği | Renk rezervi disiplini: tehlike rengi asla ödül/ped rengine sızmaz. Bizde oyuncu pembesi ve asit yeşili rezerv. |
| **Sayonara Wild Hearts** | Pembe-mor-mavi neon, müzik video estetiği | Psikedelik yönün paleti; ama yoğun siyah zemin üstünde çalışıyor, bizim parlak gökyüzümüzde çalışmaz → koyu gövde zorunlu. |
| **Audiosurf** | Parçanın spektrumu yolun rengini ve şeklini belirliyor | Spektrumdan sahte zarf: gerçek FFT yerine notanın harmonikleri. |
| **Beat Saber / Pistol Whip** | Kırmızı/mavi neon, vuruşta kısa flaş + halka | Neon çekirdek beyaz, hale renkli; vuruş halkası (ripple). |
| **Hyper Light Drifter** | Pixel art + doygun neon vurgular, sınırlı palet | Pixel dünyada neonun **küçük alanlarda** kalması; her şey parlarsa hiçbir şey parlamaz. |
| **Celeste** (yay ve bumper) | Yay sıkışıp açılır; bumper vurulunca kısa genişleme + parlama, sonra kısa "boş" durum | Mevcut squash/stretch ile birlikte: neon kısa süre **söner/kararır** (cooldown göstergesi), sonra yeniden yanar. Okunur bir "dolu/boş" durum verir. |

## 5. Üç görsel yön

### N1 · Neon Spektrum (referansa en yakın)
- **Görünüm:** koyu mürekkep gövde (`#140C1C`), 2 px neon çerçeve (boşta sabit renk). Fırlatmada üst kenardan 12–16 çubuk yarım daire/fan şeklinde fışkırır, boyları notanın harmonik zarfı; çubuk aralığı/köşeliliği enstrüman. Taban katman: kenar teli titrer + bir ripple halka.
- **Artı:** referansla birebir; en okunur; en ucuz; renk + çubuk sayısı/zarf iki ayrı kanal.
- **Eksi:** "müzik visualizer" klişesi, oyunun Bauhaus/şeker diline uzak; nota farkları ince (zarf farkı) olduğu için şekil kanalı zayıf, renk ağır yük taşır.

### N2 · Chladni Plakası (cymatics)
- **Görünüm:** ped bir titreşen plaka. Boşta düz koyu yüzey, ince kenar çizgisi. Fırlatmada yüzeyde (ve üstünde kısa yarım daire içinde) nota başına farklı (n, m) düğüm deseni neon olarak belirir, 0.4 sn'de söner. Enstrüman çizgi stilini seçer (sürekli / noktalı-halftone / kalın).
- **Artı:** Bauhaus geometrisine en yakın (kare, daire, simetri); her nota **gerçekten farklı şekil** → renk körlüğüne dayanıklı; ucuz shader; halftone/noktalı varyantı mevcut Riso dilini (`RisoDot`, `RisoSlab`) devam ettirir.
- **Eksi:** 56 px'te yalnızca düşük modlar okunur (n, m ≤ 4–5); hız sırasında "desen" okunur ama hangi nota olduğu okunmaz; yatay ped (2:1) kare plaka formülünü gerer.

### N3 · Psikedelik Kaleydoskop
- **Görünüm:** fırlatmada pedin üstünde yarım daire bir pencere açılır; içinde N-katlı kaleydoskop (N = enstrüman), domain-warp ile akan renk bantları (perdeye göre ana renk + iki komşu). Kenar neonu bantların rengini döngüler.
- **Artı:** en göz alıcı, "Mario gibi sınırsız yaratıcı dünya" ve uyumsuz tonlara izin veren GDD tonuna uyar; ileride farklı dünyalara taşınabilir.
- **Eksi:** hızda leke olarak okunur; fotosensitivite riski en yüksek; en pahalı; pixel/CRT dünyada en yabancı duran seçenek.

### Hangisi sanat yönüne en uygun?
**N2 (Chladni), N1'in taban katmanıyla.** Yani: koyu gövde + neon çerçeve + kenar teli + ripple (her vuruşta aynı, "ped tepki verdi") ve imza katmanında nota başına Chladni deseni. Gerekçe: Bauhaus geometrisi, shader-önce kuralı, renk körlüğü (şekil bilgi taşır), ucuz, ve referansın neon dilini çerçevede koruyor. Referansa sadakat öncelikse N1; Chladni yerine çubuk fanı aynı altyapıyla değiştirilebilir (aynı MPB parametreleri, farklı fragment fonksiyonu). N3 bir "dünya teması" varyantı olarak ileriye bırakılabilir.

Önerilen sıra (çiçek önerisiyle aynı mantık):
1. Statik HTML/SVG mockup: N1 ve N2'yi sahne renkleri, oyuncu ve CRT ızgarası (480×360) üstünde yan yana, 5 nota × 2 yön.
2. Shader'da yalnızca neon çerçeve + sahte parıltı (Bloom yok), oyunda bak.
3. İmza deseni + nota event'i; gamepad testi.
4. Bloom ekleme kararı en sonda, ayrı.

## 6. İnsan için açık sorular

1. **Ped kırmızı kimliğini korusun mu?** Boşta kırmızı neon, vuruşta nota rengi mi; yoksa boşta nötr (beyaz/krem) mi?
2. **Referansın pembesi:** oyuncu `#FF51DE` ile çakışıyor. Pembe tamamen çıksın mı, yoksa yalnızca çerçevede daha kırmızı bir pembe mi?
3. **Bloom:** oyuna global Bloom eklemek (diğer her şeyi etkiler) kabul mü, yoksa sahte parıltıyla mı kalalım?
4. **Nota sabit mi rastgele mi?** Her ped kendi notasını sabit taşırsa (ped A = C, ped B = G) oda bir melodi olur ve oyuncu pedleri renkten tanır; tamamen rastgele olursa "her seferinde farklı" hissi gelir ama bilgi taşımaz. (Ses dosyasıyla birlikte karar.)
5. **Görsel alan:** efekt pedin içinde mi kalsın, yoksa üstünde bir yarım daire/fan olarak dışa taşabilir mi? Dışa taşmak okunurluğu artırır ama oyuncu sprite'ıyla çakışır.
6. **Pixel mi pürüzsüz mü?** Neon çizgi CRT ızgarasına snap edilsin mi (pixel art), yoksa halftone notu gibi pürüzsüz mü kalsın?
7. Üst üste hızlı ped zinciri olan odalar var mı? Varsa fotosensitivite için yoğunluk sınırı gerekir.

## Kaynaklar

- Scriabin, clavier à lumières ve beşli çemberi renkleri: https://en.wikipedia.org/wiki/Clavier_%C3%A0_lumi%C3%A8res · https://interlude.hk/writing-music-light/ · https://scalar.usc.edu/works/at-the-crossroads-of-the-senses/media/dh-42a-scriabin-color-tonality-for-prometheus-1909-10 · Scriabin'in sinestezisi tartışması: https://pmc.ncbi.nlm.nih.gov/articles/PMC4791385
- Chladni formülü ve uygulamaları: https://editor.p5js.org/pattvira/sketches/5LAASlRY- · https://didgety.github.io/blog/2023/chladni_plate_simulation · https://observablehq.com/@tmcw/chladni-patterns · https://vec2pt.github.io/projects/chladni.html · https://resources.wolframcloud.com/FunctionRepository/resources/ChladniFigure · https://arxiv.org/pdf/2605.09846
- Lissajous ve müzik aralıkları: https://www.whipplemuseum.cam.ac.uk/explore-whipple-collections/acoustics/lissajous-tuning-forks-standardisation-musical-sound · https://lecdem.physics.umd.edu/h/h4/h4-18.html · https://www.physics.wisc.edu/ingersollmuseum/?p=314 · https://www.physics.umd.edu/hep/drew/music/beats.html
- Fotosensitivite (WCAG 2.3.1): https://www.w3.org/WAI/WCAG22/Understanding/three-flashes-or-below-threshold.html
- URP 2D HDR/Bloom/emission: https://learn.unity.com/course/2d-lighting-for-pixel-art/tutorial/add-emission-shaders · https://github.com/elringus/sprite-glow · https://discussions.unity.com/t/how-do-i-objects-to-have-independent-glowing-effects/827106 · https://discussions.unity.com/t/trying-to-add-a-glow-to-textures-and-line-renderers-in-2d/936474
- Rez, kuantize ses, Kandinsky: https://en.wikipedia.org/wiki/Rez_(video_game) · https://blog.eu.playstation.com/2017/10/20/classic-levels-deconstructed-tetsuya-mizuguchi-musician-adam-freeland-dissect-rez-infinites-area-5 · https://timeextension.com/features/the-making-of-rez-tetsuya-mizuguchis-timeless-masterpiece · https://newstatesman.com/culture/2017/07/how-tetsuya-mizuguchi-reinvented-video-games-his-love-synaesthesia
- Geometry Dash pad/orb renk kodlaması: https://tvtropes.org/pmwiki/pmwiki.php/VideoGame/GeometryDash · https://www.ludo.guide/guide/geometry-dash/the-editor-level-creation/pads-orbs-jump-yellow-pink-red-green-blue-dash
- Diğer oyun gözlemleri (Tetris Effect, Thumper, JSB, Sayonara Wild Hearts, Audiosurf, Beat Saber, Pistol Whip, Hyper Light Drifter, Celeste) genel bilgiden; bu oturumda kaynakla doğrulanmadı.
- Proje içi: `docs/GDD.md` §8, `docs/ui-research/palette.md`, `docs/ui-research/flower-node/oneri.md`, `Assets/Scenes/NewScene/Global Volume Profile.asset`, `Assets/DefaultVolumeProfile.asset`, `Assets/Settings/New Universal Render Pipeline Asset*.asset`, `Assets/Scripts/CRTEffect/CrtRendererFeature.cs`, `Assets/Shader/SpiralNode.shader`.
