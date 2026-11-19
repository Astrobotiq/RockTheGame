# B — Bauhaus / Yapısalcılık / Süprematizm / İsviçre Stili

## Hareketin özü ve ödünç aldığım ilke
Bauhaus ve akrabaları şunu savunur: biçim, işlevinden ve en yalın geometrik öğelerden (nokta, çizgi, daire, üçgen, kare) kurulur; süs eklenmez, renk az ve düz kullanılır. Kandinsky "Nokta ve Çizgi'den Düzleme" kitabında çizgiyi *hareket eden noktanın izi* olarak tanımlar. Moholy-Nagy dönen bir aygıtla (Light Prop / Light-Space Modulator) hareketin kendisini estetik malzeme yapar. Albers "Homage to the Square" serisinde şemayı sabit tutup renkleri 3-4'e indirerek algının komşuluğa bağlı olduğunu gösterir. Lissitzky'nin Kırmızı Kama'sı ise iki düz renk ve bir diyagonalle dinamizm kurar. Ödünç aldığım ilke: **tek bir geometrik fikir + en fazla 2 düz renk + hareketi desen değil dönüş yaratsın.** Kandinsky'nin "iz" fikri ve Albers'in "tek şema, iki renk" mantığı B1-B3'ün omurgası.

## Dönüş davranışı (oyunda ~350-610 °/s, kare başına 6-10°)
Üç tasarım da düşük açısal tekrarlı (1 veya 3 kat simetri), wagon-wheel riski yok. Radyal ayrıntı büyük ölçekli.
| Hız | B1 (3 kol, periyot 120°) | B2 (tek çizgi, 1 kat) | B3 (eksantrik daireler) |
|---|---|---|---|
| 100 °/s (1,7°/kare) | sakin, kollar net okunur | çizgi yavaşça dışa/içe akar, hipnotik | belirgin "sallanan göz" |
| 300 °/s (5°/kare) | kollar akışkan, yön belli | akış 0,28 R/sn, çok pürüzsüz | sarmal-sarsıntı, yön zayıflar |
| 600 °/s (10°/kare) | 12 kare/periyot, strobe yok | pitch=R/3, kare başı kayma 0,009R, titreşim yok | hızlı titreşen hedef; yön okunmaz |

## B1 — Üç Kollu Girdap (ilk tercih)
- Fikir: üç eşit kol, 50/50 kaplama, kollar arkaya kıvrık (dış uç dönüşün gerisinde) = "dışa akan" pervane.
- Renkler: indigo `#362d70` + sarı `#fcae04` (tamamlayıcı çift, paletten; sahnedeki mavi ve kırmızı/siyah asit ile karışmaz).
- Bilerek çıkarılanlar: noktalar, halka, degrade, üçüncü renk. Merkezde tek indigo disk + sarı nokta (hedef 1/3 yarıçap net).
- Riskler: indigo kollar koyu zeminde (sahne mavisi üstünde sorun yok) ama karanlık odada kaybolabilir; kol sayısı 3 olduğundan "pervane/ventilatör" çağrışımı. Yön net: kıvrım el yönünü verir.
- Shader: `p.x*=dir; th=atan2(p.y,p.x); u=r/R; k=fract((th-ang+2.1*u)*3/TAU); kol = k<0.5; hub = r<0.15R`; AA için `smoothstep` + `fwidth(k)`.

## B2 — Nokta ve Çizgi (en minimal)
- Fikir: Kandinsky'nin çizgisi: merkezdeki tek nokta + 3 tur Arşimet sarmalı, çizgi genişliği = boşluk (tam 50/50). Beş yaşındaki çocuğun çizebileceği şey.
- Renkler: turuncu `#ff5104` + soluk `#eeeac7` (paletten). Turuncu platformlarla akraba ama zemin mavisi üstünde disk olarak ayrılır; kırmızı/siyah değil, tehlike okunmaz.
- Bilerek çıkarılanlar: kol çoğaltma, merkez süsü, dış halka. Yalnız nokta + çizgi.
- Riskler: ince çizgi 96 px'te 3 tur ≈ 16 px aralık (okunur ama sınırda); merkezde çizgi başlangıcı nokta ile birleşir (kasıtlı); klasik "hipnoz spirali" yani oyunda en çok "dönüyor" hissi veren ama biraz çocuksu.
- Shader: `p.x*=dir; th=atan2(p.y,p.x); f=fract(3.0*u+(th-ang)/TAU); cizgi = f<0.5` (u=r/R); merkez nokta `r<0.12R`.

## B3 — Albers İç İçe Daireler
- Fikir: Albers'in "tek şema, iki renk, komşuluk" mantığı. 6 düz daire, iç içe, renkler dönüşümlü; her daire ebeveynine göre kaydırılmış, merkezler bir tur boyunca sarmal çiziyor ve en içteki daire merkeze yakın kalıyor.
- Renkler: kâğıt `#f1e6c7` + derin turkuaz `#04586c`.
- Bilerek çıkarılanlar: kol, çizgi, vurgu; yalnız daire ve ofset.
- Riskler: **yön okunurluğu en zayıf olan** (sarmal yalnız hilal kalınlıklarından çıkar); normal node'un (10 eş merkezli halka) uzak akrabası, 96 px'te karışabilir; yüksek hızda "sallanan göz". Bu yüzden son sırada.
- Shader: `for i in 0..5: c_i = c_(i-1) + 0.62*(R_(i-1)-R_i)*(cos,sin)(-i*2pi/5 + ang)` (CPU'da hesaplanıp uniform dizisi olabilir); `if(length(p-c_i)<R_i) renk=i&1?turkuaz:kagit` (son eşleşen kazanır).

## Genel notlar
- Yön: tüm tasarımlar `dir=-1`'de ayna görüntü (`scale(dir,1)` + `rotate(angle*dir)`); yön yalnız sarmalın el yönüyle okunur, renkle değil.
- `held`: B1'de hub biraz büyür, B2'de çizgi %12 kalınlaşır; B3 yok sayar.
- Performans: vektör yol, ~40-220 nokta; 120 px yarıçapta ölçülebilir maliyet ihmal edilebilir (dolgu hızı tarayıcıya bağlı, doğrulanmadı).
- Görsel doğrulama: kendi küçük rasterleyicimle statik kareler üretildi (CW/CCW, 96 ve 300 px); gerçek tarayıcı/oyun testi yapılmadı.

## Kaynaklar
- Moholy-Nagy, Light Prop / Lichtspiel: https://www.moma.org/collection/works/300671 ve https://monoskop.org/Moholy
- Albers, Homage to the Square (az renk, göreli algı): https://infiniteimages.toledomuseum.org/?p=953
- Lissitzky, Kırmızı Kama (iki düz renk + diyagonal dinamizm): https://smarthistory.org/?p=44345
- Kandinsky, Point and Line to Plane (1926): https://ia800506.us.archive.org/16/items/pointlinetoplane00kand/pointlinetoplane00kand.pdf
- Somut Sanat / Max Bill (sistematik varyasyon): https://www.theartstory.org/movement/concrete-art/history-and-concepts/
