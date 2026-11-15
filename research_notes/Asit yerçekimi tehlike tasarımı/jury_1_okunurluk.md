# Jüri 1: Oynanış okunurluğu ve adalet

*Bakılanlar: rapor, GDD §2/§6/§8, `hazards.md`, `platforms.md` (Riso levha bölümü), moodboard 02/09/12/16/22, `MovingDeath` / `RotatingDeath` prefab'ları.*

## Ekranda gördüklerim (kararların dayanağı)

- **Node'lar dev boyutta.** Eş merkezli gökkuşağı halkaları var: krem, teal, zeytin yeşili, turuncu, kırmızı. Merkezde krem/beyaz-sarı bir sıcak nokta, dışta pembe-mavi yumuşak bir hale. Oda 12 ve 16'da beyaz tehlike çubukları node'ların hemen üstünde ve altında, 5'li sıralar halinde duruyor ve halenin **içine giriyor**. Yeni tehlike en çok bu halenin üstünde okunmak zorunda.
- **Hareketli platformlar zaten "baskı" dilinde** (`platforms.md`, RisoSlab). `#140C1C` ince kontur, paper gövde, teal/sarı/pembe halftone, hıza bağlı riso kayması ve arkada nokta izi var. Tehlikeler aynı sürücüleri ve aynı yolları kullanıyor. Bu yüzden asıl karışma riski node değil, **güvenli hareketli platform**. Rapor bunu atlıyor.
- **Launch pad** T ayaklı, ince, yassı bir kırmızı çubuk. Bugünkü tehlike çubuklarıyla aynı orana sahip.
- **Ananasın yaprağı yeşil**, çilekler küçük ve kırmızı. Zemin asidi parlak yeşil, üstü hafif dalgalı. Pembe kremanın kenarı yuvarlak damla girintili.
- **Boyut:** `RotatingDeath` daireleri 1 birim çapında (`m_Radius` 0.5). `referencePPU` 16 ile bu yaklaşık 16 texel ediyor. `MovingDeath`'te sprite ve collider ikisi de 1×1, yani bugün görsel ile collider birebir aynı. Yörünge yarıçapı 5, periyodu 4 sn (yaklaşık 7.9 birim/sn). Hızlanan tip 2 sn gidiyor, 1 sn bekliyor, 2 sn dönüyor.

## Puan tablosu

Kriterler: A tam hızda anında okunurluk, B node/ananas/pad/oyuncu/platformla karışmama, C collider dürüstlüğü, D hareket ve hız telegrafı, E hem dairede hem uzatılmış çubukta çalışma.

| | Konsept | A | B | C | D | E | Toplam | Gerekçe |
|---|---|---|---|---|---|---|---|---|
| K1 | Kapsül No.7 | 7 | 6 | 9 | 7 | 8 | **37** | Sağlam bir taban. Yumuşak loblar kremanın damla kenarına benziyor, 16 texel'de detaylar kayboluyor. |
| K2 | Diken Tacı | 9 | 9 | 7 | 9 | 8 | **42** | Gümüş dikenlerin "ölüm" şeklini devralıyor. Diken boyu = hız telegrafı renk gerektirmiyor. Tek risk uçların collider dışına taşması. |
| K3 | Güneş Gülü | 6 | 7 | 3 | 6 | 5 | **27** | Damlaların arası boş görünüyor ama öldürüyor. Collider yalan söylüyor. |
| K4 | Levitatör Köprüsü | 7 | 8 | 5 | 7 | 5 | **32** | Levha varyantı çubukta çok iyi. Boncuklu ve kolyeli hali boşluk yalanı. Dairede zayıf. |
| K5 | Yerçekimi Sürücüsü | 5 | 2 | 4 | 7 | 5 | **23** | İç içe halkalar node'un ta kendisi. |
| K6 | Manyetik Şişe | 7 | 6 | 6 | 8 | 7 | **34** | Şerit hızı iyi bir telegraf. Ama simit "ortası geçilir" diye okunuyor, şeker gibi duruyor. |
| K7 | Uyarı Grameri | 8 | 8 | 9 | 6 | 8 | **39** | Evrensel uyarı dili. Tek başına konsept değil, bir katman. |
| K8 | Delikli Baskı Diski | 4 | 3 | 6 | 6 | 5 | **24** | Halftone ve nokta akışı hareketli platformun imzası. Platform gibi okunuyor. |
| K9 | Fosforlu Rejim | 6 | 3 | 6 | 7 | 7 | **29** | Riso kayması zaten platform hız göstergesi. Saf beyaz sıcak nokta node'un merkezi. Kontursuz ışıma haleye karışıyor. |
| K10 | Ters Nazar | 5 | 2 | 7 | 5 | 4 | **23** | Eş merkezli halkalar ve tılsım okuması. Bakan göz yolu anlatmıyor. |
| K11 | Ters Damla | 6 | 7 | 7 | 6 | 6 | **32** | Siluet temiz ama iç desen hızda gürültü. Telegrafı zayıf. |
| K12 | Kıvılcım Kubbesi | 6 | 5 | 5 | 8 | 6 | **30** | İki ölüm türü. Titreyen arklar silueti bulanıklaştırıyor, parlak küre node'a yakın. |

## İlk 3

1. **K2 Diken Tacı.** Okunurluk yükünü taşıyan şey kenar. Oyuncu "dikenli = ölür" kuralını oda 9 ve 22'de zaten öğrendi. Kural yeşil asitle birleşince çifte sinyal oluyor ve gri tonlamada da çalışıyor. Hızlanan yolda diken boyu hızı renksiz gösteriyor. Node'ların pürüzsüz dairesel dilinin tam tersi.
2. **K1, sadeleştirilmiş çekirdek olarak.** Koyu ve küçük bir gövde, tek sarı mercek. 16 texel'lik dairelerde plaka, anten ve vida yok. Sevimlilik çekirdekte kalıyor, ölüm kenarda.
3. **K7, katman olarak.** Yalnızca çubuk çekirdeğinde ve emitör kapaklarında 45° sarı-ink bant. Kırmızı launch pad çubuğuyla şekil olarak aynı orandaki tehlike çubuğunu renk körlüğünde bile ayırıyor.

**Hibrit önerim:**
- **Daireler:** K1 koyu çekirdek + K2 dikenli asit halkası.
- **Çubuklar:** K4'ün *sürekli levha* varyantı. İki uçta öldüren tombul emitör kapakları (K7 bantlı), uzun kenarlarda kısa ve küt K2 dikenleri. Boncuk ya da boşluk yok.

## Veto

- **K5, K10:** Temiz eş merkezli halka ile yuvarlak merkez birlikte node'un siluetini veriyor. Oda 16'da node'un yanında dururken ayırt edilemez.
- **K8:** Halftone noktalar ve noktaların akışı, güvenli hareketli platformun imzası (RisoSlab + HalftoneTrail). Ölümcül olanla güvenli olan aynı yolda aynı dili konuşamaz.
- **K9 (tam hali):** Riso ofseti platform hız göstergesiyle aynı. Saf beyaz merkez node merkeziyle aynı. Isı kayması tek başına kalırsa izin verilebilir, gerisi değil.
- **K3:** Görünür boşluk + dolu collider = haksız ölüm. Zar eklemek, konseptin kendisini ortadan kaldırıyor.
- **K12:** Ölümün kaynağı belirsiz (elektrik mi asit mi). Ark titremesi ölümcül kenarı oynatıyor.

## Anlaşmazlıklar hakkında kararlar

- **Kontur ve ışıyan kenar: kontur, ama platform konturu değil.** Işıyan kenarı reddediyorum. Node halesi, zeytin yeşili halka ve CRT bulanıklığı üstünde kenar kayboluyor ve tehlike olduğundan büyük görünüyor. Kontur **çok koyu asit yeşili** (ink'e kayan) olmalı ve ekranda ≥3 px kalınlıkta. RisoSlab'ın `#140C1C` ince çizgisinden hem renk hem kalınlıkla ayrılmalı.
- **Dış hale: tamamen yasak.** Bloom tehlikeye uygulanmaz. İç rim serbest ama silüetin içinde kalır.
- **Çekirdek rengi: koyu (ink/plum).** Paper çekirdek, RisoSlab'ın paper gövdesi gibi, yani "içi platform" okunur. Bu yüzden paper'ı reddediyorum.
- **Saf beyaz: hiçbir yerde yok.** Merkezde beyaz sıcak nokta node'un merkeziyle birebir aynı. Bugünkü "beyaz = ölüm" alışkanlığı placeholder'dan geliyor, korunacak bir kural değil. Parlama paper renginde, küçük ve tek.
- **Lamba: yalnızca sarı.** Durum yanıp sönme hızıyla anlatılır. Kırmızı launch pad'le ve deuteranopide yeşille karışıyor, 2 px'lik nokta bile gereksiz.
- **Asitte halftone: hiç yok, gölge bandında da.** Bu konuda rapordan daha katıyım. Hareketli dünyada düz ve doygun olan tek şey tehlike olmalı. Halftone platformun dili.
- **Teal alan halkaları: yasak.** Teal hem platformun mürekkebi hem node halkası.
- **Renk = durum:** Yeşil ile sarı-yeşil arası küçük bir kayma serbest, ama asıl sinyal diken boyu. Pembe veto.
- **Mekanik çekirdek ölümcül mü? Evet, cihazın tamamı öldürür.** Güvenli ya da tutunulabilir çekirdek "kancalanır mı?" sorusunu doğurur. Yuvarlak, merkezi ve ortası işaretli bir şey bu oyunda kanca hedefi demek. Ayrıca halka biçimli collider gerektiriyor.

## Kazanan konsept için zorunlu kurallar

1. Collider dalgasız taban şekil olur. Görsel hiçbir karede collider'dan küçük olmaz. Dikenler ve dalgalar yalnızca dışa çıkar, diken boyu taban yarıçapın en fazla ~%10'u kadardır ve uçları küttür. `_DebugCollider` çizgisi zorunlu.
2. Daire ve çubuk aynı SDF'yi, collider aynı transform'u kullanır. `BoxCollider2D.edgeRadius` = SDF köşe yarıçapı.
3. Tehlikede halftone, riso kayması, nokta izi ve yuvarlak nokta partikülü olmaz. Damlalar hız yönünde **uzamış çizgi** olur, collider'ı yoktur.
4. Tehlikede `#FFFFFF`, teal, pembe `#FF51DE`, dış hale ya da temiz eş merkezli halka bulunmaz. Kırmızı da olmaz.
5. Asit rengi `FlowingAcid` zemin şeridiyle birebir aynıdır.
6. Çekirdek toplam çapın en fazla %35'i kadardır. 1 birimlik dairede en fazla 2 detay olur (gövde + mercek).
7. Hız diken boyuyla, dönüş yönü diken eğimiyle anlatılır. AccelDeath'in 1 sn'lik bekleyişinin son kısmında lamba hızlanır ve dikenler parlar. Duraksama eklemek ya da süreyi değiştirmek insanın kararı.
8. Yol kesikli ink çizgiyle gösterilir, asla yeşil değildir. RotatingDeath'in pivotu (yarıçap 5) mutlaka gösterilir.
9. Kabul testi: gri tonlama + deuteranopi simülasyonu. Kadrajda node halesinin içine giren bir çubuk (oda 16 dizilimi), bir ananas, bir launch pad ve bir hareketli RisoSlab platform birlikte olmalı.

## Yalnızca gamepad playtest'iyle çözülebilecekler

- Collider'ın içe çekilme oranı (%5 mi %10 mu) ve diken uçlarına sürtünüp hayatta kalmanın adil mi yoksa kafa karıştırıcı mı hissettirdiği.
- Diken boyunun tam hızda (yaklaşık 8 birim/sn yörünge) gerçekten okunup okunmadığı.
- Bekleyiş sonu telegrafının ne kadar önce başlaması gerektiği.
- Oyuncuların koyu çekirdeğe kanca atmaya çalışıp çalışmadığı.
- Oda 16 ve 12'deki 5'li çubuk sıralarının node yanında görsel yükü kaldırıp kaldırmadığı.
- CRT filtresinden sonra 3 px konturun yetip yetmediği.
- Dash ve slingshot hit-stop'u (0.2 sn) sırasında hareketli tehlikeye çarpıp ölmenin "haksız" hissettirip hissettirmediği. Bunu görsel tasarım çözemez.
