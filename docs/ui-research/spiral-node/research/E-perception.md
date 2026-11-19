# E - Algi bilimi: hipnotik ama sade sarmal dugum

Kaynaklar (kendi cumlelerimle ozetlendi):
- Michael Bach, Spiral aftereffect - https://michaelbach.de/ot/mot-adaptSpiral/ (Plateau 1849; ~20 sn bakis, etki ~30 sn'de doyar, ters yonde)
- Bach, Pinna-Brelstaff - https://www.michaelbach.de/ot/mot-PinnaBrelstaff/ (yerel parlaklik profili hareket algilayicilarini 45 derece saptirir)
- Bach, Rotating Snakes / snakeAdLib - https://michaelbach.de/ot/mot-snakes/ ve https://michaelbach.de/ot/mot-snakeAdLib/ (illuzyon asimetrik parlaklik sirasindan gelir; simetrik olunca yok olur; iki gri yer degisince genisleme daralmaya doner)
- Fraser spiral illusion - https://en.wikipedia.org/wiki/Fraser_spiral_illusion (halka + egik parcalar = sahte sarmal; yani dar aralikli sarmal "halka" gibi okunabilir)
- Wagon-wheel / temporal aliasing - https://en.wikipedia.org/wiki/Wagon-wheel_effect (ornekleme siniri: kare basina adim, desen periyodunun yarisini gecerse yon ters okunur)
- Epilepsy Foundation / ITU-R BT.1702 ozeti (3 Hz, genis alan, >%10 kontrast) - https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/118

## Bulgular -> parametre
| Parametre | Bulgu | Secim |
|---|---|---|
| Yon/akis | Sabit konumda kolun yaricapi r = a(phi - donus). Kollar donusun ARKASINDA kalirsa kenarlar disa akar (genisleme), onde olursa ice (daralma). Sarmal sonrasi-etkisi ters yonde. | Arkada kalan kol (FLOW=+1): "firlatan" his, vakum/girdap degil |
| Kol sayisi | Bir noktadan gecis frekansi = N * hiz/360 Hz. >=3 Hz genis alanda riskli; dugum ekranin ~%3'u | N=1-2 (N=3 sadece <=300 der/sn) |
| Kol/bosluk | 50/50 en guclu kenar enerjisi; ince cizgi (%30) daha "cizilmis", daha az titresim | E1,E3: 50/50; E2: %30 cizgi |
| Arsimet vs log | Arsimet: sabit aralik, kucukte temiz ama dis kenarda kol neredeyse halka (egim ~85 derece) -> radyal akis zayif, Fraser benzeri "halka" riski. Log: sabit egim, akis her yaricapta ayni oranda (v_r = w*r/k) | Yon okunurlugu icin log (E1,E3); E2 Arsimet, ~2 tur ile sinirli |
| Tekrar | 96 px'te bant >=5 px olmali -> en fazla ~2.5 radyal tekrar | E2'de 2 tur, digerleri ~1 tur |
| Kontrast | Esparlak (isoluminant) renkler hareket algisini zayiflatir; iki duz seviye = yanlis ters kayma yok | Koyu/acik cift (indigo-krem, teal-sari) |
| Merkez | Sarmalin baslangici = net tek nokta | r=0.16-0.17R duz disk (vurma alani R/3'un icinde) |
| Parlaklik merdiveni | Asimetrik 3 seviye statik karede bile yonlu kayma ipucu verir (Kitaoka/Pinna); yon ters ise E3_STAIR=-1 | Sadece E3 (3. renk bunun icin) |

## Hiz tablosu (60 fps)
| Gorsel hiz | adim/kare | gecis Hz: N=1 / 2 / 3 / 6 | E1 radyal akis | E2 radyal akis |
|---|---|---|---|---|
| 100 der/sn | 1.7 | 0.3 / 0.6 / 0.8 / 1.7 | 0.8 x teget | ~%8 teget (akis net) |
| 300 der/sn | 5 | 0.8 / 1.7 / 2.5 / 5 | ayni oran | ayni oran |
| 600 der/sn | 10 | 1.7 / **3.3** / **5** / **10** | ayni oran | akis fark edilmez, "pervane" gibi |
Wagon-wheel (yon tersi) icin adim >= periyot/2 gerekir (N=2: 90 derece/kare = 5400 der/sn; N=12: 15 derece/kare). 600 der/sn'de gercek aliasing yok; asil risk titresim ve N>=6'da ziplama. **Guvenli aralik:** N=1 icin <=540, N=2 icin <=360, N=3 icin <=300 der/sn. Hipnotik bolge 90-250 der/sn. Oneri: gorsel hizi yumusak sinirla (ornegin 360*tanh(w/360)); yon korunur, hizli oyunda bile gecis <=2 Hz kalir. Kenar hizi 600 der/sn'de R=1.5 birimde ~16 birim/sn: oyuncu (1 birim) kadar buyuk bir sey ~0.1 sn'de doner, yine de okunur.

## E1 - Iki kanat (en iyisi)
Fikir: iki kalin, arkada kavisli kanat; N=2, 50/50, sabit egimli (log) sarmal. Bulgu: yon/akis kurali + gecis frekansi + log.
Renk: #362d70 indigo, #f1e6c7 krem (gri/siyah/kirmizi yok, tehlike degil).
Dislanan: halka, nokta, ok, parilti, ek renk, ince cizgi. Risk: iki kanat "yin-yang/pervane"ye benzer; ana dugumun halkalarindan cok farkli (iyi), ama kucukte kanat uclari dar. 
Shader: `a = atan2(p.y,p.x)*dir; u = a + k*ln(r/r0)*FLOW - angle*dir; m = fract(u/(TAU/N)) < 0.5; col = m?A:B; hub = r<0.17R` (k=1.25, N=2). AA: smoothstep(fwidth).

## E2 - Tek cizgi
Fikir: tek renkli tek kalem cizgisi sarmal (Arsimet, N=1, ~2 tur), "5 yasinda cizer". Bulgu: N=1 en dusuk gecis frekansi (600 der/sn'de bile 1.7 Hz) + Arsimet sabit bant.
Renk: #04586c derin teal cizgi, #fcae04 sari zemin.
Dislanan: merkez ornament, halka, ikinci kol. Risk: dis kenarda halkaya benzer (Fraser, ana dugumle karisma); yon okumasi merkeze yakin kisimdan yapilir; 600 der/sn'de akis yok, sade donus.
Shader: `u = r/pitch + (a*dir)/TAU*N ...` yani `f = fract(r/pitch - (a*dir - angle*dir)/TAU*FLOW)`; `line = f < 0.34`; `hub = r<0.17R`. pitch=0.5R.

## E3 - Parlaklik merdiveni
Fikir: iki kol, her kol koyu -> orta -> acik 3 duz basamak; asimetrik parlaklik statik karede bile bir yon hissi ve yerel kayma verir. Bulgu: Kitaoka/Pinna-Brelstaff asimetri.
Renk: #362d70 koyu, #ee7284 orta (pembe, kirmizi degil), #f1e6c7 acik. 3. renk: asimetrik parlaklik icin zorunlu.
Dislanan: 4. basamak (gercek Rotating Snakes), halka, doku. Risk: "lolipop/seker" gorunumu; pembe oyunun kirmizi tehlikesine yaklasabilir; illuzyon yonunun (yardim mi ters mi) ekranda gozle dogrulanmasi lazim (E3_STAIR); orta basamak gokyuzune yakin tonlarda kaybolmaz ama kontrasti E1'den dusuk.
Shader: `f = fract(u/(TAU/N))` (E1'deki u); `col = f<0.30 ? D : f<0.68 ? M : L`.

Siralama: E1 > E2 > E3. Dosya: E-perception.js (3 hizli vektor cizim, R=120'de ~0.03 ms).
