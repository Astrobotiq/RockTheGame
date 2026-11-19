# Halftone / Ben-Day doku araştırması (hareketli platformlar)

Tarih: 2026-11-13. Hedef: referanstaki baskı halftone görünümü (sol: seyrek CMYK rozet, sağ: yoğun overprint), **pixel-art değil**, pürüzsüz ve yüksek çözünürlüklü. Contact sheet: `textures/index.html`. Üretici: `textures/generate_textures.py`.

## 1. Bulunan hazır kaynaklar (hiçbir dosya indirilmedi)

Dürüst özet: referansa yakın, **CC0 ve renkli CMYK** bir halftone dokusu bulamadım. Bulunanların çoğu siyah-beyaz Photoshop overlay'i veya ücretli paket. Bu yüzden oyun için dokuyu kendimiz üretmek mantıklı.

| Kaynak | URL | Lisans | Çözünürlük / tileable | Referansa yakınlık |
|---|---|---|---|---|
| Kenney Pattern Pack Extra | https://opengameart.org/content/pattern-pack-extra-pattern003png-0 | CC0 | 80+ PNG/SVG, tileable desenler; halftone olduğu doğrulanmadı | Uzak (büyük olasılıkla tek renk desen) |
| Graphics Fuel, 16 Free Halftone Seamless Patterns | https://www.graphicsfuel.com/16-free-halftone-seamless-patterns/ | Sayfada açık değil; "ticari hak" ücretli sürüme yönlendiriyor, yani ticari kullanım riskli | ~998x998, .PAT + .PNG, tileable | Orta-uzak (siyah-beyaz nokta rampaları, renk yok) |
| Free Halftone Dot + Grain Texture (Gumroad) | https://mikeliszarins.gumroad.com/l/HalftoneDotGrainTexture | Doğrulanmadı | Overlay dokusu | Uzak (tek renk) |
| Risograph Textures (itch.io, aosmiles) | https://aosmiles.itch.io/risograph-textures | Ücretli (min. 5 USD), lisans sayfadan kontrol edilmeli | Taranmış riso dokuları | Orta (riso tanesi için kıyas) |
| Seamless Halftone Dot Patterns (Graphic Spirit) | https://creativemarket.com/GraphicSpirit/743918-Seamless-Halftone-Dot-Patterns | Ücretli | Tileable | Orta, ama paralı |
| Vecteezy / Adobe Stock / Creative Fabrica halftone sonuçları | (arama sonuçları) | Atıf gerekli veya abonelik; ticari kısıtlar var | Vektör/PNG | Kullanmayın (lisans yükü) |
| Wikimedia Commons "Benday_Dots.svg" | https://en.wikipedia.org/wiki/Ben_Day_process | Dosya sayfasında lisans doğrulanmadı | SVG | Eğitici örnek; oyun dokusu değil |

## 2. Shader için teknik referanslar

| Kaynak | URL | Lisans | Ne öğretiyor |
|---|---|---|---|
| Godot Shaders, CMYK Halftone (OskarGosbol) | https://godotshaders.com/shader/canvas-item-halftone-shader/ | Kod MIT | CMYK ayrımı, kanal başına açı + ofset (kayık baskı), kenar AA için çoklu örnekleme, kağıt rengini mürekkeple çıkarma |
| paper-design/shaders | https://github.com/paper-design/shaders | Apache-2.0 (ticari, atıfsız; LICENSE/NOTICE korunmalı) | Web için üretim kalitesinde shader koleksiyonu; halftone-CMYK varlığını doğrulayamadım, dokümantasyon sitesinden bakılmalı |
| daniel-ilett/shaders-halftone | https://github.com/daniel-ilett/shaders-halftone | MIT | Unity URP + Shader Graph halftone (ışığa bağlı nokta boyutu). CMYK yok ama Shader Graph kurulumu için doğrudan işe yarar |
| Shadertoy 4dSSRG (halftone fonksiyonu) | https://www.shadertoy.com/view/4dSSRG | Shadertoy varsayılanı CC BY-NC-SA 3.0, **ticari değil**; sayfayı fetch edemedim (403) | Yalnızca teknik okuma. Kod kopyalamayın |
| Standart CMYK açıları | (Photoshop/baskı literatürü) | - | Y 0°, C 15°, M 75°, K 45° (açılar ~30° arayla; rozet = istenen moiré) |

Üretimde kullandığım ve shader'a aynen taşınabilecek formüller:
- Dönük kafes: `uv_ch = rot(angle_ch) * (uv * ölçek + ofset_ch)`, hücre merkezi `round(uv_ch)`, nokta yarıçapı `r = sqrt(kapsama / pi)` (hücre birimi), kenar `smoothstep` ile `fwidth` genişliğinde (AA).
- Tam tileable olması için açıyı rasyonel seç: kafes taban vektörleri `a = L(m,-n)/N`, `b = L(n,m)/N`, `N=m^2+n^2` (aşağıdaki script bunu kullanıyor). 45° = (k,-k), 0° = (k,0), ~15° = (13,-4) veya (19,-5).
- Overprint: kanal başına `T = min(1, mürekkep/kağıt)`, `renk = kağıt * Π (1 - kapsama_i * (1 - T_i))` (multiply).
- Kayık baskı: kanal başına 3-9 piksellik (1024 doku birimi) kaydırma.
- Nokta kazancı (dot gain): yarıçapı %3-5 büyüt + kenarı 1-2 px yumuşat; mürekkep tanesi: kapsamayı düşük frekanslı olmayan (sigma ~1 px) gürültüyle çarp.
- Kağıt tanesi: sigma 0.8 px ince gürültü (%3-5) + sigma ~22 px yumuşak lekelenme (%2).

## 3. Üretilen dosyalar (`textures/`, hepsi 1024x1024 PNG, opak)

| Dosya | Açıklama |
|---|---|
| `classic_benday_red_game.png` | Kırmızı #FC1E00 nokta, krem kağıt #F1E6C7, 45°, tileable |
| `classic_benday_pink_game.png` | Pembe #FF51DE nokta, aynı kağıt, 45°, tileable |
| `classic_benday_magenta_print.png` | Baskı magentası #EC008C, kağıt #FFFBEE, 45°, tileable |
| `cmyk_sparse_game.png` | Referansın sol yarısı: büyük sarı 0°, küçük mavi 15°, pembe 75°, minik siyah 45°, hafif kayık baskı; oyun paleti; tileable |
| `cmyk_sparse_print.png` | Aynısı, gerçek CMYK paleti |
| `cmyk_dense_game.png` | Referansın sağ yarısı: iri, çakışan noktalar, overprint ikincil renkleri, siyah noktalar; oyun paleti; tileable |
| `cmyk_dense_print.png` | Aynısı, gerçek CMYK paleti (kırmızı/yeşil/mavi daha net) |
| `gradient_ramp_game.png` | Üstte seyrek, altta yoğun (sarı, pembe, mavi sırayla devreye girer, en altta siyah); hacim gölgesi rampası; sadece yatayda tileable |
| `gradient_ramp_print.png` | Aynısı, baskı paleti |
| `riso_grain_game.png` | Risograph: pembe + mavi, mürekkepte tane, kayık baskı, güçlü kağıt tanesi; tileable |
| `riso_grain_print.png` | Aynısı, magenta + cyan |
| `trail_ink_dot.png` | 256x256 şeffaf: tek yuvarlak mürekkep noktası, hafif sızma kenarı |
| `trail_misreg_pair.png` | 256x256 şeffaf: kayık baskı pembe+mavi nokta çifti, kesişim multiply |
| `trail_dot_cluster.png` | 256x256 şeffaf: küçük, üç mürekkepli halftone nokta kümesi |
| `trail_ink_splat.png` | 256x256 şeffaf: yumuşak mürekkep sıçraması + uydu damlalar |
| `generate_textures.py` | Yeniden üretici (numpy + Pillow, 4x süpersample, ~25 sn) |
| `index.html` | Contact sheet (gök mavisi arka plan, her doku yumuşak yuvarlak platform şeklinde iki ölçekte) |

Teknik not: bu makinede varsayılan Anaconda Python 3.8'de numpy 1.20 ile Pillow 10 uyumsuz; script'i Python 3.11 + güncel numpy/Pillow ile çalıştırdım (geçici venv, repoda değil). Tileable kontrolü: sol-sağ kenar farkı komşu piksel farkından büyük değil (dikişsiz). 4x4 alan ortalaması kullanıldı (LANCZOS yerine; halkalanma yapmaz).

## 4. Dürüst değerlendirme

Platform olarak (gök mavisi #71BAEA üzerinde, 4:1 siyah konturlu yumuşak şekil) kontrol ettim:

1. **`cmyk_sparse_game.png` (en iyi referans eşleşmesi + okunurluk).** Krem kağıt zemin üzerindeki sarı büyük noktalar mavi gökten net ayrılıyor; pembe/mavi/siyah küçük noktalar rozet ve kayık baskı hissini veriyor. Üst yüzey, açık krem + düzenli büyük noktalarla "buraya basılır" diye okunuyor.
2. **`cmyk_dense_game.png` (referansın sağ yarısı).** En yakın görsel karakter ve en çok enerji, ama tek başına platform yüzeyi için fazla gürültülü: iniş yüzeyini okumak zorlaşır. Platformun **alt/yan gövdesi** (üst şerit hariç) veya tetiklenen/tehlikeli platform varyantı için iyi.
3. **`gradient_ramp_game.png` + `classic_benday_red_game.png`.** Rampa, platform hacmini verir: kullanım önerisi, platformun üst kenarı seyrek/krem (iniş yüzeyi temiz), gövdeye doğru yoğunlaşan noktalar; shader'da dikey UV ile sürmek daha iyi. Kırmızı klasik Ben-Day ise en okunaklı ve en "oyun paleti" seçenek ama referansın CMYK rozetini taşımıyor; yedek/tehlike platformu için uygun.

Diğer notlar:
- Oyun paleti, baskı paletine göre daha tutarlı: sarı #FCAE04 krem kağıtta sıcak duruyor; baskı paleti (#FFE600 + beyaza yakın kağıt) gökten daha fazla ayrışıyor ama oyunun diğer UI'ından kopuk.
- `riso_grain_*` platform olarak zayıf: mavi-mor çakışması gökle karışıyor, tane küçük ölçekte parıltı yapıyor. Arka plan/pano için daha uygun.
- Overprint çarpımı üçlü bindirmede hızla siyaha kayıyor; dense dokuda siyah mürekkep payını bilerek düşük tuttum. Shader'da canlı üretecekseniz siyah kanalı ayrı bir "koyu nokta" katmanı olarak kısın.
- Referanstaki kalın siyah mürekkep çizgisi doku değil, platform kenar konturu olmalı (shader/SDF ya da sprite outline).
- Doku ölçeği: sparse için platform genişliğinde ~6-8 nokta civarı iyi okunuyor (karo ~512 px'e ölçeklenince hücre ~36 px).

## 5. Lisans uyarıları

- Üretilen tüm dokular ve script bu projeye özgü, üçüncü taraf varlık içermiyor.
- Graphics Fuel ve Gumroad paketleri için ticari kullanım hakkı doğrulanmadı; kullanmayın.
- Shadertoy kodları varsayılan olarak CC BY-NC-SA (ticari değil); yalnızca teknik okuma için.
- MIT (Godot Shaders, shaders-halftone) ve Apache-2.0 (paper-design) kodları ticari kullanılabilir; MIT için lisans metni, Apache için LICENSE/NOTICE korunmalı.
