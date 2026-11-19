# D - Hareketli grafik ve oyun gorsel dili

## Lens ve ana fikir
- **Saul Bass / Vertigo**: Bass tek bir sekle indirger; Vertigo jenerigindeki sarmallar John Whitney'in sarkac + M5 top yonlendiricisi duzeneginden cikti, yani elle degil matematikle, dogru ve temiz [Rhizome](https://old.rhizome.org/editorial/2013/may/09/did-vertigo-introduce-computer-graphics-cinema), [Art of the Title](https://artofthetitle.com/title/vertigo).
- **Duchamp Rotoreliefs / Anemic Cinema**: ic ice daire desenli diskler pikapta donunce derinlik ve sisme-sonme izlenimi verir [Boijmans](https://www.boijmans.nl/en/collection/artworks/150019/rotoreliefs-optical-discs), [Hyperallergic](https://hyperallergic.com/duchamps-spinning-optical-experiments).
- **Super Hexagon**: dunya cizgi, ucgen, altigene indirgenmis; tek merkez sekil, hizli dondurulen ayni dil [Time Extension](https://timeextension.com/features/the-making-of-super-hexagon).
- **Thumper / Sayonara Wild Hearts**: hizli oyunda gorsel bilgi tek bakista okunmali; okunurluk yuksek kontrast + geometrik sekilden gelir [Game Developer](https://gamedeveloper.com/design/how-i-thumper-i-uses-striking-visuals-to-complement-gameplay-at-every-turn), [Apple Developer](https://developer.apple.com/news/?id=33kvkagk).

**Odak kilitleme icin minimum**: (1) donme simetrisini kiran en az bir isaret (yoksa donus gorunmez), (2) iki zit ton, (3) tekrar sayisi az (1-3). Sarmal kenarlar donerken radyal akis (disa dogru sisme) yaratir; bu "hipnoz" etkisinin tamami, ek susleme gerekmez.
Odunc aldigim ilke: **Bass'in tek-sekil indirgemesi + Duchamp'in donerken derinlik veren eksantrik daireleri**. Hepsi duz dolgu, 2 renk (D3'te 2. renk sadece merkez nokta).

## Hiz davranisi (oyunda ~350-610 der/s, 6-10 der/kare; sarmal ~3 birim, oyuncu ~1)
| Tasarim | Aci tekrari | 100 der/s | 300 der/s | 600 der/s |
|---|---|---|---|---|
| D1 | 180 der (2 kol) | yavas, kollar disa akar | net, akis hizli | 10 der/kare, bant kaymasi ~R/72, strob yok |
| D2 | 360 der (tek) | hilal yavasca doner | hipnotik atim | tek isaret oldugu icin alias imkansiz |
| D3 | 120 der (3 kanat) | kanatlar ayri secilir | akici | 12 kare/dongu, sinirda ama okunur |
Hepsi 12 tekrar esiginin cok altinda; D2 en guvenli, D3 en hizli-sinirda.

## D1 - Bass sarmali (iki kol) [en iyi]
- **Fikir**: koyu teal zemin ustunde %50/%50 bantli iki kollu Arsimet sarmali; Vertigo ve Duchamp'in en yalin hali. Kollar donusun tersine sarilir (arkada iz birakir) -> disa akis.
- **Renk**: `#04586c` (zemin), `#f1e6c7` (kollar). Koyu siluet mavi gokte, turuncuda, krem platformda net.
- **Cikarildi**: nokta, halka, ok, gradyan, kenar cizgisi (kollar r=0.93R'de biter, koyu kenar kendiliginden siluet olur).
- **Risk**: 96 px'te bant 6 px, sinirda ama okunur. Kollar kenarda biter; merkez nokta hit alani icin acik. Yon: sarmal elligi tek karede net.
- **Shader**: `s = r*T/R - dir*(atan2(y,x)+angle)/TAU` (T=2); kagit rengi `fract(2*s) < 0.5` ve `r < 0.93R`; merkez `r < 0.085R`.

## D2 - Rotorelief (ice teget daireler)
- **Fikir**: 5 ic ice daire, her biri oncekine ictenteget; teget noktasi halka basina 60 der doner -> nautilus sarmali. Duz Rotorelief'te yon okunmaz (ayna simetrik), bu yuzden teget kaydirma eklendi. Donerken ince-kalin hilal darbesi.
- **Renk**: `#362d70` indigo, `#fcae04` sari (tamamlayici, tehlike rengi degil).
- **Cikarildi**: tum susleme; sadece daire.
- **Risk**: "goz" gibi okunabilir (4 halkada daha belirgindi, 5 halkada nautilus'a yaklasti); yon cok ince hilal uclarina bagli, 96 px'te zayif. Normal dugumun es merkezli halkalariyla karisma riski dusuk (merkez kaymis).
- **Shader**: merkezler `c_i = c_{i-1} + lam*(r_{i-1}-r_i)*(cos a_i, sin a_i)`, `a_i = dir*(i-1)*60 der - angle`; sondaki sapma dogrusal geri cekilir; `max i` ile `length(p-c_i) < r_i*R` olan en yuksek i rengi alir.

## D3 - Uc kanat (turbin) [en minimal]
- **Fikir**: ucu sivri, kenarda genisleyen uc kavisli kanat + paper rengi merkez nokta; zemin yok (seffaf), Super Hexagon'un kama dilinin sarmal hali.
- **Renk**: `#362d70` kanat, `#f1e6c7` merkez nokta (yalniz merkez).
- **Cikarildi**: zemin, kenar, ic sarmal, halka.
- **Risk**: seffaf aralik arka plana bagli (krem platformda indigo yine okunur). 3-kat simetri 600 der/s'te 12 kare/dongu; yonde belirsizlik yok. Tehlikeye benzemez (kirmizi/siyah yok) ama pervane/vantilator gibi "bicak" sezdirebilir.
- **Shader**: `u=r/R; loc = mod(dir*(atan2(y,x)+angle) - sweep*u, TAU/3); mavi = loc < TAU/3*(0.3+0.4*u)`; sweep=130 der; merkez `r<0.14R`.

## Siralama
D1 > D3 > D2. D1 en cok Bass/Whitney, en okunur, en az risk. Dosya: `D-motion.js` (D1-D3). Dogrulama: `node --check` temiz; tum varyantlar 120 yaricapta saf vektor yoludur.
