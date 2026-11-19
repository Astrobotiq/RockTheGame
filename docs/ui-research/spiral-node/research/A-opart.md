# A - Op Art ve Kinetik Sanat lensi

## Hareketin ana fikri
Op Art, düz bir yüzeyde "hareket/titreşim/derinlik" hissini yalnızca yüksek kontrast, tekrar ve geometriyle üretir; kinetik sanat ise hareketi gerçekten döndürerek ekler. Üç ilke aldım:
- **Duchamp, Rotoreliefs (1935)**: diskler plak çalara konup 33 devirde döndürülünce düz baskı derinlik illüzyonu verir; resim durağanken sadece daire geometrisidir, hareket optiği tamamlar.
- **Vasarely (Zebra, 1938)**: eğri bantlar, kontur çizgisi olmadan figür/zemin arasında gidip gelir; boyut ve genişlik gradyanı "kabarma" hissi verir.
- **Riley**: aralık, açı ve sıkışmadaki küçük değişiklikler titreşim yaratır; iki ton yeter.
- **Spiral aftereffect (bilim)**: dönen sarmal izlenince genişleme/daralma algısı doğar; yani sarmalın dönüşü kendiliğinden "disa akan" bir hareket gibi görünür.

Seçim: kollar **arkada iz bırakır** (CCW dönen node'da kollar dışa doğru saat yönünde kıvrılır, pervane/galaksi gibi). Döndükçe desen dışa doğru genişliyor gibi görünür (itme/fırlatma hissi, tehlike değil). dir=-1 ayna görüntüsüdür.

## Hız davranışı (oyun içi 100 / 300 / 600 °/s, 60 fps = 1.7 / 5 / 10 °/kare)
Tüm tasarımlarda açısal tekrar sayısı düşük (2, 5, 2), bu yüzden wagon-wheel eşiğine (periyodun yarısı: 90 / 36 / 90 °/kare) çok uzak.
- **A1** periyot 180°: 100'de sarmal yavaş akar, net okunur; 300'de net genişleme akışı (hipnotik tatlı nokta); 600'de hâlâ akış (kare başı kayma bant genişliğinin ~%12'si), bulanıklaşma yok.
- **A2** periyot 72°: 100'de pervane okunur; 300'de kanatlar akıcı döner; 600'de kare başı 10° (periyodun %14'ü) = yumuşak, hafif titreşim ama strobe yok.
- **A3** periyot 180° (iki kol): 100'de toplar yavaş sürünür; 300-600'de iz bırakan spiral akış hissi, nokta nokta olduğu için 600'de en dağınık okunan (ama kare atlaması yok).

## Tasarımlar (en iyiden en sadeye)
**A1 - Duchamp Diski**: iki kollu Arşimet sarmalı, kol ve aralık eşit kalınlıkta, dışarıya taşar. Renk: indigo `#362d70` + sarı `#fcae04` (tamamlayıcı çift, yüksek parlaklık kontrastı, palet içi). Bilerek yok: halftone, ok halkası, hub rengi, kontur, gradyan. Risk: hafif "şeker/lolipop" havası; 96 px'te 3 tur bile okunur ama kol kalınlığı ~7 px. Yön: kol kıvrımından net okunur. Merkez: iki kolun birleştiği indigo nokta. `held`: kol biraz kalınlaşır.
**A2 - Vasarely Kanatları**: 5 kalın, hafif burulmuş (~110°) kanat, tek merkezde buluşur; dışa doğru açılan kama. Renk: derin teal `#04586c` + kağıt `#f1e6c7`. Bilerek yok: sarmalın çoklu turu, ince çizgi, halka. Risk: normal pervane/fan gibi okunabilir ("bu bir vantilatör"); çift sayıda kanat strobe yapardı, o yüzden 5 (tek). Yön net. Merkez: kanatların sivri birleşimi. `held`: kanatlar %10 genişler.
**A3 - Vasarely Boncukları**: iki kolda 4'er büyüyen daire + merkez noktası (en sade; 5 yaşında çizer). Boyut gradyanı derinlik, dönüşte "dışa kayan ve büyüyen toplar". Renk: indigo `#362d70` + pembe `#ee7284`. Risk: sadece 9 daire, yönü tek karede en zayıf okunan (iz kıvrımı seyrek); "çok az" / halftone'a kayma ihtimali. Merkez: açık büyük nokta, en net. `held`: toplar %8 büyür.

## Shader notu (p = piksel - merkez, rn = |p|/R, th = atan2(p.y, p.x), y yukarı; angle = birikmiş dönüş)
- A1: `ph = frac(3.1*rn + dir*2*(th-angle)/TAU); d = abs(frac(ph+0.5)-0.5);` kol = `1 - smoothstep(0.25-fw, 0.25+fw, d)`, fw = fwidth(d). (2 kol, R/3.1 radyal aralık; dir=-1 ayna.)
- A2: `ph = frac(5*(th - angle + dir*1.95*rn)/TAU);` kanat = `1 - smoothstep(0.5-fw, 0.5+fw, ph)` (ph<0.5 kanat, kenar yumuşatma için fw = fwidth(ph)); 5 kanat, burulma 1.95 rad.
- A3: tek formülle olmaz; 9 daire SDF'i (`min(length(p - c_i) - rho_i)`, `c_i = r_i*(cos a_i, sin a_i)`, `a_i = angle - dir*1.25*i + PI*k`, `r_i = 0.2*1.42^i`, `rho_i = 0.06+0.13*r_i`) veya sprite daha uygun.

## Kaynaklar
- Duchamp Rotoreliefs: https://www.toutfait.com/?p=1565 (Betancourt, "Precision Optics / Optical Illusions") ve https://masp.org.br/en/collections/works/rotoreliefs
- Riley / titreşim: https://www.artbasel.com/stories/bridget-riley-op-art-british
- Op Art genel ilkeler (kontrast, figür-zemin, afterimage, Vasarely Zebra): https://en.wikipedia.org/wiki/Op_art
- Spiral aftereffect: https://pmc.ncbi.nlm.nih.gov/articles/PMC5549815
- Kinetik sanat: Christie's Op Art özeti https://www.christies.com.cn/en/art-movement/op-art
