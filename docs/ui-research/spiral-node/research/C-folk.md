# C - Halk / Doğu sarmal gelenekleri (sadece geometri)

**Çekirdek fikir:** Halk sanatlarında sarmal bir "süs" değil, tek bir hareketin işaretidir: tomoe'de 3 virgül birbirini kovalar, Ege/Newgrange çömleklerinde tek çizgi dönüp durur, koru'da bir fidenin kıvrımı açılır. Hepsi 1-3 öğeyle, düz iki renkle çalışır. Ödünç aldığım ilke: **kafa-kuyruk asimetrisi** (yuvarlak kafa önde, sivri kuyruk geride; yön tek karede okunur) ve **tek sürekli çizgi**. Dini anlam taşıyan hiçbir sembol olduğu gibi alınmadı; sadece eğri geometrisi.

Kaynaklar (kendi cümlelerimle özetlendi):
- Tomoe / mitsudomoe, sol-sağ (hidari/migi) yön adlandırması, 1-5 virgüllü aileler: https://en.wikipedia.org/wiki/Tomoe (ayrıca JAANUS tomoemon: https://www.aisf.or.jp/~jaanus/deta/t/tomoemon.htm)
- Triskelion: dönel simetrili üçlü sarmal, Newgrange/Miken/Malta örnekleri, "Kelt" etiketinin sonradan geldiği: https://en.wikipedia.org/wiki/Triskelion
- Koru: açılan eğrelti otu filizi, dış kıvrım hareket, iç kıvrım köken; kōwhaiwhai'de ana öğe: https://en.wikipedia.org/wiki/Koru
- Hokusai "Büyük Dalga" (tepe merkezde sarmal, köpük pençeleri) ve Hiroshige Naruto girdabı: https://en.wikipedia.com/wiki/Great_Wave_off_Kanagawa (Naruto için Philadelphia ve Denver müzesi koleksiyon kayıtları)

Genel kural: kollar `dir=+1` (düğüm CCW döner) iken merkezden dışarı saat yönünde kıvrılır (kuyruk geride kalır), `dir=-1` ayna. Dönünce desen dışa doğru genişliyor gibi görünür (kaçış değil, "fırlatma" hissi).

## C1 - Mitsudomoe (üç virgül) - önerilen
- **Fikir:** 3 virgül (yuvarlak kafa + kenara yaslanan sivri kuyruk), 120° simetri, kafalar merkezde küçük bir üçgen boşluk bırakır.
- **Renkler:** derin teal `#04586c` zemin, kağıt `#f1e6c7` virgüller.
- **Bilerek çıkarılan:** iç halka, nokta, gradyan, kenar çizgisi, merkez süsü.
- **Riskler:** merkez boşluğu küçük (hedef alan ~R/3 ama içi teal-kağıt karışık; istenirse kafaları biraz büyüt). Gökyüzü mavisine yakın teal kenarda silüet zayıflayabilir. Kültürel: Hachiman/kamon çağrışımı var, 3 virgül + ayna oyunu yine de soyut geometri; "sembol kopyası" izlenimi vermemek için kuyruk daha sivri ve kafalar daha ayrık tutuldu.
- **Dönüş hızı:** açısal tekrar 120°. 100°/s: yavaş, yön net. 300°/s (5°/kare): kafa-kuyruk ayrımı korunur, akıcı. 600°/s (10°/kare, periyot 12 kare): hâlâ aliasing yok, 3 kol net bir "pervane" olur; wagon-wheel ancak ~60°/kare üstünde başlar.
- **Shader:** `p` merkezden normalize, `a = atan2(p.y,p.x)*dir - ang`, `a = mod(a, 2π/3)` ile tek sektöre katla. Kafa: `length(p - hc*(cos0,sin0)) < rh`. Kuyruk: `ro(a)` = dış yarıçap, `w(s)=rh*(1-s)^1.25`, `s=a/sweep`; `r in [ro-2w, ro]` ise kağıt. Hepsi `fwidth` ile `smoothstep` kenarı.

## C2 - Tek çizgi sarmal
- **Fikir:** Tek bir sürekli Arşimet sarmalı (Ege çömleği / Newgrange tek çizgisi), çizgi kalınlığı = boşluk (%50 doluluk), uç yuvarlak, merkez nokta.
- **Renkler:** indigo `#362d70` zemin, sarı `#fcae04` çizgi.
- **Bilerek çıkarılan:** çift kol (S-sarmal), kafa topu, süs halkası, kalınlık değişimi. Sadece çizgi.
- **Riskler:** en "klasik hipnoz spirali"; özgünlüğü en düşük ama en hipnotik. Merkezde çizgi başlangıcı tek nokta (hit alanı R/3 içinde ~1 tur çizgi + boşluk karışık). Normal düğümün halkalarına benzeyebilir (uzaktan eşmerkezli halka gibi okunma riski düşük ama var, özellikle 96 px).
- **Dönüş hızı:** tek kol, tekrar 360°, radyal adım 0.4R. 100°/s: yavaş dışa akış. 300°/s: akış 0.2R/s, ritmik; 600°/s: akış ~0.67R/s, "tünel" etkisi güçlü, strobe yok (kare başına kayma 0.011R'den küçük, çizgi kalınlığı 0.2R).
- **Shader:** `r=length(p)`, `a=atan2(p.y,p.x)*dir - ang`; `u = r/pitch - a/2π`; `f = fract(u)`; çizgi = `abs(f-0.5) > 0.25`-benzeri band (`smoothstep` ile). Merkez için `r < h` tam dolgu.

## C3 - Koru (tek şekil)
- **Fikir:** merkezde top, ondan çıkıp kenarda sivrilen tek bir kıvrım; tüm düğüm tek dolu şekil.
- **Renkler:** kağıt `#f1e6c7` zemin, indigo `#362d70` şekil.
- **Bilerek çıkarılan:** ikinci kol, halka, nokta, ikinci ton. En az öğe.
- **Riskler:** simetri yok, dönerken kütle merkezi kayar (hızda "sallanan" görünebilir, ama dönüş en okunur). Kuyruk ucu 96 px'te incelip kaybolabilir. Salyangoz / "@" gibi okunabilir. Açık zemin gökyüzünde parlak görünür, oyuncu ve diğer nesnelerle kontrast dikkat ister.
- **Dönüş hızı:** tek öğe, tekrar 360°. 100°/s: salyangoz gibi sakin. 300°/s: kıvrımın dönüşü çok net. 600°/s: kütle kayması "pulse" gibi görünür, yine aliasing yok.
- **Shader:** `s = (a_unwrapped)/Phi` ile merkezden dışa parametre; `r0 + (r1-r0)*s` merkez eğri, `w=wMax*(1-s)^0.85`; `abs(r - center(s)) < w` içi indigo + `r < r0+0.04R` top. `a` için `a + 2πk` seçimi `k = round((center_r_inverse - a)/2π)` ile.

## Sıralama ve öneri
1. **C1** (en iyi: tanınır, 3 öğe, yön net, sakin ama hızlı döner). 2. **C2** (en hipnotik, en basit). 3. **C3** (en az öğe, riskli okunurluk).
Tüm tasarımlar `docs/ui-research/spiral-node/research/C-folk.js` içinde; `st.held` yalnızca +%2-3 kalınlık ekler. `R=120`'de çizim süresi ~0.06 ms.
