# Bounce pad: neon + nota önerisi

Tarih: 2026-11-21. Task: **Bounce pad asset'leri** #art (kodda `LaunchPad`).
İstek: neon kenarlı ped; her fırlatmada rastgele bir nota, her seferinde farklı enstrüman; ped o notanın frekansına göre renk ve şekil değiştirir. Referans: "Neon Visualizer, Electric Blue and Pink" (YouTube, dairesel spektrum çubukları). Psikedelik yön de kabul.

Ayrıntılı araştırma:
- Ses: [research/ses-ve-nota.md](research/ses-ve-nota.md)
- Görsel: [research/gorsel-neon.md](research/gorsel-neon.md)

Etkileşimli önizleme (sesli, üç yön yan yana): [bounce-pad-yonler.html](bounce-pad-yonler.html). Unity'de henüz denenmedi. Bu belge iki araştırmanın birleşik önerisi.

## Tek cümlede öneri

Ped boşta koyu gövdeli, kırmızı neon çerçeveli durur. Her fırlatmada odanın müzik tonundaki pentatonik gamdan bir nota, 4 kısa enstrümandan biriyle çalar. Notanın derecesi 5 sabit renkten birini, enstrüman şekil ailesini seçer. Kenar tel gibi titrer, temas noktasından halka açılır, üstünde 0.3–0.6 sn notanın imzası (Chladni deseni) yanıp söner.

## Ses

| Karar | Öneri | Neden |
|---|---|---|
| Gam | Pentatonik, odanın müzik tonunda, müziğin melodisinden 2 oktav yukarıda | Pentatonikte yanlış nota yok; müzikle çatışmaz |
| Seçim | Tek zıplamada tekrarsız rastgele yürüyüş; zincirde gamda tırmanış (Peggle) | Rastgele ama melodik; zincir ödül hissi verir |
| Enstrüman | 4 kısa sönümlü mallet/pluck (marimba, kalimba, celesta, pluck synth gibi), aynısı arka arkaya gelmez | "Farklı" ama aynı aileden: kaos olmaz |
| Üretim | Enstrüman başına 2–3 kök örnek + `2^(yarıton/12)` pitch, ±4 yarıtonu aşmadan | Az dosya, doğal tını |
| Mevcut kod | `AudioCueSO` varsayılan 0.95–1.05 rastgele pitch'i nota cue'larında **1–1** olmalı (yoksa ±0.85 yarıton detune). `AudioCuePlayParams.PitchMultiplier` zaten var, `AudioManager`'a dokunmak gerekmez | |
| Yeni veri | Bir "nota paleti" SO: gam, kök, enstrüman → cue listesi, renk tablosu | Proje SO deseni |
| Miks | Ayrı Notes mixer grubu, müziği çok az ya da hiç duck etme, en fazla 3 ses, ~80 ms global aralık | |
| Ritim | İlk sürüm sadece tona kilitli; beat-sync sonra | |
| Örnek kaynak | VCSL (CC0) ya da Iowa MIS (kısıtsız). Freesound'dan yalnız CC0 | Lisans temiz |

## Görsel

Projeden tespit edilenler:
- Oyunda bugün **Bloom yok** (HDR açık). Neon önce shader içinde sahte parıltıyla yapılır; global Bloom ayrı karar.
- CRT 480×360'ta çalışıyor: 2 birimlik ped ≈ 56 CRT pikseli. Neon çizgi ≥ 2 px (~0.07 birim) olmalı.
- Referansın pembesi oyuncunun pembesiyle (`#FF51DE`), elektrik mavisi gökyüzüyle (`#71BAEA`) çakışıyor. Neon koyu gövde (`#140C1C`) üstünde okunur.

Katmanlar:
1. **Taban (her fırlatmada aynı):** neon çerçeve parlar, üst kenar tel gibi titrer (`sin(kπx)`, k = harmonik), temas noktasından 1–3 halka. Mevcut squash/stretch ile birleşir. "Ped tepki verdi" bilgisini taşır.
2. **İmza (notaya göre değişen):** üç yön.

| Yön | Ne | Artı | Eksi |
|---|---|---|---|
| **N1 Neon Spektrum** | Pedden yukarı fışkıran 8–16 neon çubuk ya da yarım daire fan; zarf notanın harmoniklerinden (gerçek FFT yok) | Referansa en yakın, en ucuz, en okunur | Klişe |
| **N2 Chladni plakası** (önerilen) | Ped yüzeyinde kum çizgisi gibi düğüm hatları: `cos(nπx)cos(mπy) − cos(mπx)cos(nπy)`, nota → (n,m) | Bauhaus geometrisine en yakın, özgün, renk körü için şekil bilgi taşır | Yalnız n,m ≤ 5 okunur |
| **N3 Psikedelik kaleydoskop** | Pedin üstünde açı katlamalı, gürültüyle bükülen renk akışı; simetri sayısı = enstrüman | En çarpıcı | Hızda renkli leke; en yüksek flaş riski |

Lissajous figürü (notanın köke oranı: 3:2, 5:4 …) N1/N2'ye alternatif imza olarak LineRenderer ile yapılabilir.

Eşleme: gam derecesi → 5 elle seçilmiş sabit renk, oktav → parlaklık/boyut, enstrüman → şekil ailesi, perde → desen parametresi. Renk paleti CRT'li çekimde ayarlanmalı (CRT doygunluğu artırıyor).

Uygulama: elle yazılmış HLSL, ped başına `MaterialPropertyBlock` (nota rengi, n/m, vuruş zamanı), `SpiralNode.shader` deseni. Partikül patlaması nota renginde.

Erişilebilirlik: flaş yalnız ped alanında, asla tam ekran (WCAG 2.3.1: saniyede 3'ten az flaş). Bilgiyi renk değil şekil taşımalı.

## Alınan kararlar

- **2026-11-21, N3 geliştirildi:** ped içinde sürekli akan "sıvı ışık" shader'ı (kullanıcının referans görseli: domain-warp alan, mor → magenta → kırmızı → turuncu → pembe-beyaz paleti). Boşta yavaş akar, vuruşta hızlanır ve parlar. Kaleydoskop patlaması artık yalnız yukarı değil, pedin şeklini (SDF) takip ederek **her yöne** dalga gibi açılır; simetri sayısı enstrümandan (3/4/5/6). "Nota rengine döndür" seçeneği paleti notanın rengine kaydırır (açık mı kapalı mı: karar bekliyor).
- **2026-11-21, N3 kenarı = sıvı kenar (çerçevesiz).** Neon çerçeve yok; pedin kendi kenarı sıvı gibi dalgalanır (boşta hafif, vuruşta güçlü), ince pembe-beyaz bir çizgiyle belirginleşir. Mockup'ta karşılaştırma için diğer kenarlar (mürekkep blok, RGB kayması, akan palet, halftone hale, neon) duruyor. Açık risk: ped dikdörtgen okunmuyor, oyunda vuruş alanının okunurluğu test edilmeli.

- **2026-11-21, şekiller:** dikdörtgenin yanında **üçgen ve daire** bounce pad de üretilecek. Shader SDF tabanlı tek shader olmalı (şekil parametresi); `LaunchPad` collider'ı ve algılaması (`BoxCollider2D` + `OverlapBox`) şekle göre genişletilmeli. Fırlatma yönü karar bekliyor.

- **2026-11-21, "Bounce pad kararları" task'ı kapandı** (kullanıcı onayı; 1–4 ve 6 önerilen şekliyle onaylandı, 5 ve 7 kullanıcı cevabıyla):
  1. **Görsel yön:** N3 onaylı (sıvı ışık gövde + sıvı kenar). Açık risk: dikdörtgenin okunurluğu, gamepad testinde bakılacak.
  2. **"Nota rengine döndür": açık.** Boşta palet mor-magenta-turuncu; vuruşta palet notanın rengine kayar, sonra geri döner. Bilgiyi şekil de taşır (simetri sayısı = enstrüman).
  3. **Nota:** rastgele, tekrarsız. Zincirde (yere basmadan ardışık pad) gamda yukarı tırmanış. Ped başına sabit nota ileride bulmaca odası için ayrı seçenek.
  4. **Enstrümanlar:** ~~aynı aile, 4 kısa mallet/pluck~~ → **güncellendi (aynı gün, kullanıcı): düz enstrüman yerine psychedelic sesler.** 4 kısa, karakterli ses (sitar/tanpura tarzı pluck, FM çan, ters çevrilmiş pluck, detune synth), kendimiz sentezleyip örnek olarak üretiyoruz (lisans derdi yok, indirme yok), Notes mixer grubunda hafif echo + chorus/flange. **Sesler kısa kalır** (uzun swell/glide yok: zincirde üst üste binince çamur yapar). Aynısı arka arkaya gelmez. VCSL/Iowa örnek planı bırakıldı.
  5. **Gam: A minör pentatonik** (A C D E G). Müziklerin tonu bilinmiyor; müzik tonları belli olunca kök/mod oda başına veri olarak değiştirilir. Kulak testinde ayarlanır.
  6. **Bloom:** global Bloom yok, shader içinde sahte parıltı.
  7. **Fırlatma yönü:** dikdörtgen ve üçgen `transform.up` (üçgende tepe noktasının baktığı yön). **Daire: temas noktasından dışa** (daire merkezinden oyuncuya doğru vektör). Daire için yön oyuncunun düştüğü yere bağlı olduğundan gamepad testinde öngörülemezlik kontrol edilmeli.

- **2026-11-21, kenar düzeltmesi (kullanıcı):** N3 kenarı **"Akan palet kenar"** (mockup'taki `rainbow` seçeneği); yukarıdaki "sıvı kenar (çerçevesiz)" kaydı geçersiz. Unity uygulaması mockup'la yan yana karşılaştırıldı (boşta, vuruş tepe noktası t=0.05, patlama t=0.4; sky zemini) ve ölçekleri mockup'tan alındı: ped 170 px = 3.2 birim, gövde 30 px = 0.5647 birim, çerçeve 5→8 px, zarf 0.05/0.22, patlama hızı 270 px/sn (+60 üst oktav). Bilinen farklar: Unity'de gerçek CRT shader'ı (doygunluğu artırır) mockup'taki CSS çizgilerinden farklı görünür; patlama mockup'ta 4 px'lik hücrelerle yumuşatılıyor, Unity'de sürekli.

- **2026-11-21, gövde kalınlığı (kullanıcı):** ped gövdesi mockup oranında kalır (3.2 × 0.5647 birim, 170×30 px); üstü collider üstüyle aynı hizada, gövde altına doğru kalın. Collider (3.2 × 0.3) değişmedi. Gamepad testinde vuruş alanı okunurluğuna bakılacak.

- **2026-11-21, üçgen = 90-45-45 (kullanıcı):** üçgen pad ikizkenar "tepe yukarı" değil, seviye köşelerine konacak dik üçgen: dik açı yerelde sol-altta, dik kenarlar 2 birim, pivot dik açı köşesinde (90°'lik adımlarla çevrilir). Fırlatma yönü hipotenüsün dışa bakan normali (`launchAngleOffset` -45, `TransformUp` modu). Önceki "üçgende tepe noktasının baktığı yön" kaydı geçersiz.

- **2026-11-21, mevcut seviye pad'leri (kullanıcı):** nota seviyelerdeki tüm pad'lerde çalar. `noteSystem` taban `LaunchPad .prefab`'ta atanmış durumda; Room prefab'ları ve `MovingLauncPad`/`WrappingLaunchPadManager` varyantları bunu devralır. Görsel (trampolin sprite) ve collider değişmedi; yalnız `LaunchPad-LiquidLight*` prefab'ları yeni görseli taşır.

## Karar bekleyen sorular

> Not: 1, 2, 3, 4 ve 6 numaralı sorular yukarıdaki "Alınan kararlar"da cevaplandı. 5 (kırmızı kimlik) neon çerçeve kalktığı için geçersiz. 7 (efekt pedin dışına taşar mı), 8 (pixel/pürüzsüz) ve 9 (beat-sync, ilk sürümde yok) açık.

1. **Görsel yön:** N1 spektrum, N2 Chladni, N3 psikedelik, ya da karışım?
2. **Nota rastgele mi sabit mi?** Tamamen rastgele = "her seferinde farklı". Ped başına sabit nota = oda bir melodi olur, oyuncu pedi renginden tanır. Arası: rastgele ama zincirde tırmanış.
3. **Enstrümanlar ne kadar farklı?** Aynı aile (mallet/pluck) mi, tamamen farklı tınılar (piyano → trompet → synth) mı? Gerçek örnek mi, chiptune/sentez mi?
4. **Gam karakteri:** minör pentatonik (karanlık) mı, majör (neşeli) mi, odaya göre mi? Müziklerin ton bilgisi var mı?
5. **Ped kırmızı kimliğini korusun mu?** Boşta kırmızı neon, vuruşta nota rengi mi?
6. **Bloom:** global Bloom eklensin mi (her şeyi etkiler), sahte parıltıyla mı kalınsın?
7. **Efekt pedin dışına taşabilir mi?** (yarım daire/fan okunurluğu artırır, oyuncu sprite'ıyla çakışır)
8. **Neon çizgi pixel mi pürüzsüz mü?** (CRT ızgarasına snap ya da halftone gibi pürüzsüz)
9. **Beat-sync** ileride istenir mi?

## Task'lar

`TASKS.md` → Active → "Bounce pad: sıvı ışık + nota" (2026-11-22 … 11-24, 9 task).
