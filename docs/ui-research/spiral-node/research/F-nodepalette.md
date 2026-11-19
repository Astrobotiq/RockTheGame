# F - Node paleti (kisit-once hat)

Kural: yeni suslemek yok; sadece normal node'un rampasi (turuncu `#ff5104`, sari `#fcae04`, kagit `#f1e6c7`) + gercek node'un cekirdek diski (`#F0D420`). Duz baski, nokta/chevron/glow yok. Hepsi tek daire (R), kenar sert.

**Olcek varsayimi.** Spiral node gorseli 3 birimlik daire (R = 1.5 birim). Normal node'un cekirdegi 1 birim cap (collider r=0.5) -> burada cekirdek `u = 1/3` = tutma hedefi, duz ve bos. Normal node halkalari 1 birim kalinlikta ve 20 birimlik hale uzerinde 4 birimden sonra soluyor; 1.5 birimlik dairede tek halka sigar ve solma gorunmez. Bu yuzden bantlar cekirdekten kenara kalan 1 birimde, ~0.33 birim kalinlikta (3 bant) cizildi; solma kullanilmadi (disk kenari AA ile kesilir).

## F1 - Sarili seritli sarmal (onerim)
- **Fikir:** Halkalarin renkleri halka olmak yerine cekirdegin etrafinda 3 esit sarmal kol olarak sarilir (turuncu, sari, kagit; bir tam tur).
- **Renkler:** `#ff5104`, `#fcae04`, `#f1e6c7` + cekirdek.
- **Yok:** nokta, siyah kontur, hub susu, 4. renk, solma.
- **Riskler:** yon: dusuk (3 kol belirgin kivrim). Normal node ile karisma: dusuk, halka yok. Tehlike: dusuk (kirmizi/siyah yok). Kagit-sari yan yana kontrasti orta.
- **Shader:** `v=(u-1/3)/(2/3); f = 3*v + s*3*theta/2pi; col = ramp[floor(f) mod 3]` (`s=+-1` = yon, `theta = atan2(y,x) - angle`). Normal halka shader'inin `floor(r*N)` satirinin sadece `+ s*K*theta/2pi` eklenmis hali; K renk sayisinin kati olmali (dikis cizgisi cikmaz).

## F2 - Kaymis diskler
- **Fikir:** Normal node'un ic ice halkalari korunur, ama her halka bir oncekine gore kaydirilir; ay-kesitleri sarmal merdiven olusturur, merkez cekirdek sabit.
- **Renkler:** `#ff5104`, `#f1e6c7`, `#fcae04` + cekirdek (5 disk: turuncu, kagit, sari, kagit, turuncu).
- **Yok:** gercek sarmal egri, bant; sadece daire (vektor, 5 `arc`).
- **Riskler:** yon: orta (kayma yonu ay-kesitinden okunur, kol kadar net degil). Normal node ile karisma: orta, ic ice daire halka ailesine en yakin tasarim. Tehlike: dusuk.
- **Shader:** disk k icin merkez `c_k = e_k*(cos(a), sin(a))`, `a = angle - s*k*70deg`; dis halkadan ice boya (painter). Fragment shader'da: kucukten buyuge `if (length(p-c_k) < rho_k) col = C_k`.

## F3 - Tek kalin kol (en sade)
- **Fikir:** Kagit zemin, ortada sari cekirdek, ondan cikan TEK kalin turuncu kol (1 baslangicli sarmal, 1.25 tur).
- **Renkler:** `#ff5104`, `#f1e6c7` + cekirdek.
- **Yok:** ikinci kol, ucuncu rampa rengi, desen, solma.
- **Riskler:** yon: en dusuk (tek kol, virgul gibi). Normal node'dan ayirt: en kolay, ama zemin kocaman kagit disk, normal node'dan agir. Donerken F1'den az hipnotik. Tehlike: dusuk.
- **Shader:** `f = 1.25*v + s*theta/2pi; arm = frac(f) < 0.46; col = arm ? orange : paper`.

## Normal node'dan bir bakista fark
Normal node: dairesel, simetrik halka (bullseye), donse bile gorunum ayni. Spiral node'lar: halka simetrisi bozuk, kollar donuste kivrilir; ayni renk ailesi (turuncu/sari/kagit + sari cekirdek) oldugu icin "bu bir node ama sariliyor" okunur. 96 px'te (R=48) F1 3 kolu, F3 tek kolu net; F2 en zayif (ay-kesitleri ~4 px).

## Yon ve ayna
`dir=+1` (CCW): kollar donusun arkasinda (pervane/fan gibi, kol disa dogru saat yonune kivrilir); `dir=-1` birebir ayna. Kol egimi tum tasarimlarda `s = dir * TRAIL`; JS basindaki `TRAIL = -1` ile tersine cevrilir (ters hissederse tek satir).

## Uygulama notu
Canvas JS per-pixel (analitik AA); R=120'de ~4 ms (Node harness olcumu, tarayicida drawImage daha ucuz). F2 vektor. Unity'de F1/F3 tek `fragment` shader'dir, ring shader'a `theta` terimi eklemek yeter, ek doku gerekmez (rampa 3 renk `float3` olarak).
