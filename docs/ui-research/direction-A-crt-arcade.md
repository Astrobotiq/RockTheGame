# Yön A: "CRT Arcade"

Tarih: 2026-11-08. Hazırlayan: UI art direction ajanı (A yönü). Durum: öneri, karar geliştiricide.
Kaynaklar: `skills-digest.md`, `GDD.md` (§1-5, §7, §8), moodboard (01, 05, 10, 15, 20, 21 görüntülendi), `MainMenuBuilder.cs`, `RaceHudText.cs`, `MainMenu.unity` (yalnızca okundu), `unity:2d-pixel-perfect` skill'i.
Bilmediğim/doğrulamadığım her şey **(doğrulanmadı)** ile işaretli.

---

## 1) Vizyon

UI, oyunun içindeki tüplü TV'nin devamı olsun: ekran bir arcade kabininin menüsü gibi açılır. Koyu lacivert (borulardaki lacivert) paneller, krem metin, tek parlak vurgu rengi (oyundaki kırmızı-turuncu "bullseye" merkezi) ve gökkuşağı halkalı odak imleci. Menüler "INSERT COIN" enerjisinde: kısa, vurucu, bloklu başlıklar, yanıp sönen "START" satırları, scanline ve hafif sapma (aberasyon) yalnızca geçişlerde.

Neden bu oyuna uyar:
- Oyun zaten CRT filtresini kalıcı kılıyor (GDD §8); UI aynı filtrenin içinde görünürse "tek ekran" bütünlüğü bedavaya gelir. Ayrı bir stil icat etmek yerine mevcut dili genişletiyoruz.
- Moodboard'daki en güçlü imza gökkuşağı halkalı node'lar. Bunların odak imleci olarak yeniden kullanılması, UI'ı "kanca her şeyin merkezinde" sütununa (GDD §2.2) bağlar: menüde de bir şeye "kancalanıyorsun".
- Hız/akış ve hızlı respawn sütunları: arcade menüleri hızlıdır (anında açılır, 150 ms animasyon, tek tuşla devam). Ölünce UI hiç araya girmez.
- Tek geliştirici, sanatçı değil: kutu + düz renk + az sprite. Arcade estetiği zaten "kaba pikselli, blok, yüksek kontrast" olduğu için eksik çizim becerisi daha az görünür.
- Ton uyumsuzluğu bilinçli olarak serbest (GDD §8). Gelecek dünyalar farklı temada olsa da UI "kabin" olarak sabit kalır, dünyalar değişir, çerçeve değişmez.

Dürüst not: Bu yön, 3 yönün en "gürültülü"sü. Okunabilirlik için panelleri düz ve koyu tutmak şart; scanline'ı UI'ın üstüne ikinci kez bindirmeyeceğiz (bkz. §9 risk 1).

---

## 2) Palet (14 renk)

Hex değerleri moodboard ekran görüntülerinden gözle tahmin edildi; görüntüler CRT filtreli olduğu için gerçek sprite renklerinden sapabilir. **Aseprite'ta ilgili sprite'lardan damlalıkla (eyedropper) örnekleyip bu tabloyu düzeltmek ilk iştir (doğrulanmadı).** Saf siyah/beyaz yok.

| # | Ad | Hex (tahmini) | Rol |
|---|---|---|---|
| 1 | Gece (ink) | `#14102A` | Panel dolgusu, en koyu gölge, outline |
| 2 | Boru (navy) | `#2A2E55` | Panel ikinci ton, pasif buton |
| 3 | Boru açık | `#4A5280` | Panel çerçevesi, ayırıcı, disabled metin |
| 4 | Krem | `#F6EED2` | Ana metin, ikon dolgusu |
| 5 | Gök | `#6FB8EC` | Menü arka planı (oyun gökyüzü), ikincil vurgu |
| 6 | Gök koyu | `#3C86C8` | Arka plan gölgesi, parallax bandı |
| 7 | Şeker (pembe) | `#F25CB4` | Başlık/üst şerit, "yeni/seçili" işareti |
| 8 | Zemin (turuncu) | `#EE9F5E` | Sıcak panel kenarı, ananas ikonu gövdesi |
| 9 | Bullseye (kırmızı-turuncu) | `#F2481C` | TEK birincil vurgu: odak, seçili buton, uyarı |
| 10 | Altın | `#F8B820` | Ananas sayacı, puan, kazanan |
| 11 | Limon | `#F8E448` | Parlak vurgu, bullseye merkezi, "BAŞLA!" |
| 12 | Turkuaz | `#2A8EA0` | Gökkuşağı halkası, bilgi (anahtar sayacı) |
| 13 | Asit yeşili | `#22C428` | Yalnızca olumlu durum ("hazır", dash/slingshot dolu) |
| 14 | Kırmızı ped | `#D83030` | Yalnızca tehlike/kayıp ("Kaybettin", hata) |

Kurallar:
- Gökkuşağı (4 renk: 9, 11, 10, 12 + krem) yalnızca **odak imleci ve grapple/etkileşim** anlamında. Başka yerde kullanma (skills-digest kuralı).
- Metin: krem (#4) üstüne gece (#1) paneli. Kontrast oranı yüksek (hesaplamadım, kabaca >12:1 beklenir (doğrulanmadı)).
- Anlam yalnızca renkle verilmez: dolu/boş durum için şekil de değişir (dolu kare / boş çerçeve).
- Hue-shift: gölgeler 1-2-3 (mora/laciverte), ışıklar 11-10 (sarıya) kayar.
- Tüm UI sprite'ları bu 14 renkle çizilir; alpha yalnızca 0/255.

---

## 3) Tipografi

**Ana font: Tiny5** (zaten projede: `Assets/UI/Fonts/Tiny5-Regular.ttf` + SDF asset). Ek font **gerekmez**; ikinci font kapsam büyütür.

Ölçek (1920x1080 referans; sanat gridi 4x, yani 1 font pikseli = 4 ekran pikseli):

| Kullanım | Ekran px | Not |
|---|---|---|
| Başlık (ROCK THE GAME) | 128 | 16 px em x 8 = 8x ölçek, tek başlık için |
| Ekran başlığı (PAUSE, SONUÇ) | 64 | |
| Buton / gövde | 32 | konsol eşiği 26 px (Xbox 101) üstünde |
| HUD sayaç | 40 | rakamlar için biraz büyük |
| İpucu / alt yazı (A: Seç) | 32 | 32'nin altına inme |

Önemli uyarılar:
- Tiny5'in **native piksel gridinin tam olarak kaç px olduğu doğrulanmadı**. Skill'in önerdiği gibi önce TMP'de 8, 16, 24, 32 punto deneyip hangisinde tüm çizgiler tam piksel olduğunu gözle kontrol et; tabloyu ona göre düzelt. 32 ekran px'in 8 px em x 4 olduğunu varsayıyorum (doğrulanmadı).
- Mevcut SDF atlas (`m_PointSize: 90`, SDF render modu görünüyor, doğrulanmadı) piksel font için yanlış araç: SDF kenarları yumuşatır. Önerim: Font Asset Creator'da **Raster / Point sampling, anti-aliasing yok**, Padding 2, atlas 512x512, yalnızca gereken karakterler. Bu, küçük metinde CRT altında da daha keskin kalır.
- **Türkçe karakter (ğ ş ı İ ö ü ç) Tiny5'te var mı doğrulanmadı.** Mevcut menüde "Çıkış / Tek kişilik" var; sahnede çalışıp çalışmadığı oyunda görülmeli. Yoksa TMP fallback gerekir (ikinci font burada devreye girer; aday: aynı piksel boyutlu başka bir font, lisansı kontrol edilerek).
- Başlıklarda büyük harf, gövdede büyük/küçük karışık Tiny5'te okunur ama küçük harfler küçük boyutta sıkışabilir. UI metni genelde BÜYÜK HARF seçilirse arcade havası da artar. Türkçe İ/I ayrımına dikkat (`"i".ToUpper()` kültür sorunu, TMP `<allcaps>` kullan).
- Metin container'a göre büyüsün, resme gömülmez. Oyun içi öğretici yazılar (moodboard'da iki farklı font var) bu fontta birleştirilebilir; UI kapsamı dışı ama tutarlılık için öneri.

---

## 4) Bileşen dili

### Temel biçim
- 480x270 gridinde (4x) çizilir. Tüm kenarlar **sert 1 px**, köşeler **kesik (chamfer) 2 px**, yuvarlama yok. Tek outline stili: gece (#1).
- Paneller koyu (#1 dolgu, #3 çerçeve), **üst kenarda pembe (#7) "damla" şeridi**: moodboard'daki pembe krem kenarının UI karşılığı. Bu, UI'ı dünyadan tanınabilir kılan tek imza detay; maliyeti bir tile satırı.
- Panel içi: 1 px alternatif satır (scanline) deseni #1/#2, yalnızca büyük panellerde, çok düşük kontrastla.

### 9-slice
| Bileşen | Sprite boyutu (sanat px) | Border (sanat px) | Not |
|---|---|---|---|
| `ui_panel_main_9s` | 24x24 | 6 | pembe şerit üst 3 px içinde |
| `ui_panel_dark_9s` | 16x16 | 4 | HUD/ipucu kutusu |
| `ui_button_normal_9s` | 24x16 | 4 | #2 dolgu, #3 çerçeve |
| `ui_button_focus_9s` | 24x16 | 4 | #9 dolgu, krem çerçeve, **aynı boyut** (zıplama yok) |
| `ui_button_pressed_9s` | 24x16 | 4 | focus'un 1 px aşağı kayık hali |
| `ui_button_disabled_9s` | 24x16 | 4 | #1 dolgu, #3 çerçeve, metin #3 |

Image Type = Sliced, Fill Center açık, kenarlar tile (Stretch değil). Border'ı Sprite Editor'da 1x kaynakta ayarla.

### Focus göstergesi
Üç katman, hepsi renge bağımlı olmayacak şekilde:
1. Buton dolgusu #9'a döner (renk).
2. Butonun soluna **mini gökkuşağı halka imleci** (12x12 sanat px, 4 renkli iç içe daire, oyundaki node'un küçültülmüşü). Halka yavaşça dönmez (pixel art'ta dönüş bulanır); bunun yerine 2 kareli "nabız" (çap +1 px) animasyonu, 4 fps.
3. Metin başına `>` işareti ve hafif sağa kayma (2 sanat px = 8 ekran px).

Böylece grapple dili menüye taşınır: halka "kancanın hedefi" gibi seçili öğeye kilitlenir (kilitlenirken 80 ms'lik kısa "çek" hareketi, bkz. §6).

### Gamepad ikonları
- **Tek set: Xbox düzeni, harfli** (A/B/X/Y, LB/RB, LT/RT, L-stick). Moodboard'da oyun içi öğreticilerde zaten yuvarlak "L" ikonu var; aynı yuvarlak form kullanılır, tutarlı olur.
- 16x16 sanat px, krem daire + gece harf; A=asit yeşili, B=kırmızı ped, X=gök, Y=altın (Xbox renkleri paletle uyumlu hale getirilmiş).
- PlayStation (✕ □) için GDD §5.1 ikinci isim veriyor. Cihaza göre ikon değişimi **kapsam dışı öneri**: önce Xbox seti, PS seti için aynı şablondan 6 ikon daha (1 akşam), `Gamepad.current` ile seçim.
- İkonlar metne gömülmez; TMP Sprite Asset ile satır içi (`<sprite name="btn_a">`).

---

## 5) Ekranlar

Genel kurallar: kritik UI kenardan %5 içeride (1920x1080'de ~96x54 px; sanat gridinde 24x13). CRT vinyeti köşeleri karartıyor, köşeye bilgi koyma. Her ekranda bir öğe seçili açılır (`EventSystem.SetSelectedGameObject`). B = geri. Fare yok, imleç gizli.

### 5.1 Ana menü
Arka plan: gökyüzü mavisi (#5), yavaşça kayan 2 büyük halka (moodboard 01/21'deki "göz" halkaları, UI'da dev dekor); alt kenarda zemin şeridi (#8 + pembe #7 damla). Halkalar ekran dışına taşar, gerçek node ile karıştırılmasın diye %50 sönük (palet dışına çıkmadan: yalnızca #5/#6 + krem halka bandı).

```
+--------------------------------------------------------------+
|                                                              |
|   (halka)                                      (halka)       |
|                                                              |
|              R O C K   T H E   G A M E                       |
|              ==========================                      |
|                                                              |
|                  (o)> TEK KISILIK                            |
|                       COK OYUNCULU                           |
|                       AYARLAR            [yok: bkz. not]     |
|                       CIKIS                                  |
|                                                              |
|   [A] SEC   [B] GERI                         v0.x  (c) 2026  |
|~~~~~~~~~~~~~~~~~ zemin + pembe damla ~~~~~~~~~~~~~~~~~~~~~~~~~|
+--------------------------------------------------------------+
```
- Başlık: 128 px, krem, 2 px gece gölgesi, pembe alt çizgi. Açılışta harf harf "yazılır" (30 ms/harf).
- Mevcut: 4 buton dikey (Tek kişilik, Çok oyunculu, Çıkış; Ayarlar GDD §11'e göre yok, sonraya).
- Alt satırdaki "A: SEÇ / B: GERİ" ipucu şeridi gamepad ikonlarıyla.
- Gamepad notu: ilk açılışta "Tek kişilik" seçili; kayıt sistemi gelirse "DEVAM" en üste.

### 5.2 Pause
Oyun dondurulur; arka plan %60 gece rengiyle kararır (sabit renk, yarı saydam tam ekran Image: alpha yalnızca panel dışı kuralının tek istisnası; ya da 2x2 dither deseni ile alpha'sız, **dither tercih**, çünkü piksel kuralına uyar).

```
+--------------------------------------------------+
|   (oyun, dither ile kararmış)                    |
|        +------------------------------+          |
|        |~~~~~~~~~~~ pembe ~~~~~~~~~~~~|          |
|        |        D U R A K L A T I L D I|          |
|        |                              |          |
|        |  (o)> DEVAM ET               |          |
|        |       ODAYI YENIDEN BASLAT   |          |
|        |       AYARLAR                |          |
|        |       ANA MENU               |          |
|        |                              |          |
|        |  Oda 7   Ananas 12   Anahtar 2/3        |
|        +------------------------------+          |
|   [START] DEVAM       [B] GERI                   |
+--------------------------------------------------+
```
- Start açar ve kapatır; B de devam eder. "Odayı yeniden başlat" respawn hızı pillar'ına uygun (onay penceresi yok, tek basış).
- MP'de pause oyunu durdurmaz (GDD §9: her oyuncu kendi dünyasında; durdurma yalnızca yerel) **(MP davranışı doğrulanmadı, `multiplayer.md` okunmadı)**. Panel başlığında "OYUN DURMADI" uyarısı gerekebilir.

### 5.3 HUD
HUD **minimum**: aksiyon sırasında dikkat çekmez. Sol üst: sayaçlar; sağ üst: yalnızca MP'de rakip bilgisi; alt orta/yalnızca olay anlarında beliren ipuçları. Kritik bilgi her zaman %5 güvenli alanın içinde.

```
 (24,13 sanat px içeriden)
  [ananas] x 12   [anahtar] 2/3                      [MP: Rakip Oda 7  F 1/3]
                                                      
                       (oyun alanı)

  [dash ■]  [sling ■]
```
- **Ananas/anahtar sayacı**: koyu #1 mini kutu (panel_dark_9s), 24 px ikon + 40 px rakam. Toplayınca rakam 120 ms "zıplar" ve ikon parlar (2 kare). Oda geçişinde veya 3 sn hareketsizlikte sayaç tamamen **sönükleşir** (opaklık 255 yerine ikon paleti koyuya kayar, alpha kullanmadan), yeni olay olunca geri gelir; böylece oyun alanı rahat kalır.
- **Dash / slingshot göstergesi**: iki küçük kare ikon, hakkın durumunu gösterir: dolu = renkli çerçeve+dolgu (dash=gök #5, slingshot=bullseye #9), boş = yalnızca çerçeve + ortada nokta (şekil farkı, renk değil). Karakterin renk değişimi (GDD §5.2 "his") zaten ana geri bildirim; HUD ikinci kanal. **Karakterin yanına mı köşeye mi konacağı bir playtest sorusu**: karakter yakını (dünya uzayı küçük ikon) hıza bakan gözü daha az kaçırır, ama kamera zoom'uyla boyut değişir (doğrulanmadı). Önerim: önce köşe, sonra karakter-yakını prototipi.
- Duvar stamina (6 sn) için ayrı HUD **önermiyorum**; GDD uyarı veriyor, karakter üstü efekt yeterli (kapsam).
- MP bölümü mevcut metinleri (`RaceHudText`: "Bayrak 1/3", "Rakip: Oda 7") kullanır, sadece panele oturtulur. Geri sayım "3-2-1-BAŞLA!" ekran ortasında 128 px, her sayı 1 sn, "BAŞLA!" limon (#11) renginde ve 0.4 sn.

### 5.4 MP sonuç ekranı
Mevcut durum: metin tabanlı (`RaceHudText.Result`: "Kazandın! 02:31.4 / Rakip: 02:40.0 / A: Lobiye dön"). Arcade yaklaşımı: "skor tablosu".

```
+--------------------------------------------------+
|                                                  |
|           * * *   K A Z A N D I N !   * * *      |
|                                                  |
|        +----------------------------------+      |
|        | 1.  SEN      02:31.4   [ananas] 12|      |
|        | 2.  RAKIP    02:40.0   [ananas]  9|      |
|        +----------------------------------+      |
|                                                  |
|        FARK: -8.6 SN                             |
|                                                  |
|        (o)> LOBIYE DON                           |
|             TEKRAR OYNA   (host ise)            |
+--------------------------------------------------+
```
- Kazanan satırı altın (#10) çerçeve + küçük taç ikonu (oyun karakterinin tacına gönderme, GDD §8); kaybeden satırı sade. "Kaybettin" kırmızı ped (#14), ama metin tonu cezalandırıcı değil: "BİR DAHA?" butonu odakta.
- Birlikte modu: "BİRLİKTE BİTİRDİNİZ" tek başlık, iki süre yan yana.
- Rakip bitirmediyse: "RAKİP BEKLENİYOR…" satırında 3 noktalı döngü animasyonu.
- Not: ananas sayısı sonuçta var mı doğrulanmadı (`multiplayer.md` okunmadı); yoksa satırdan çıkar.

### 5.5 Oda geçişi
Mevcut sistem: `RoomTransitionCoordinator` kamera + oyuncu dondurma (kod okunmadı, **iris geçişi materyali `M_IrisTransition.mat` var, doğrulanmadı**). Oyun akışını kesmemek için UI **hiç araya girmez**.
- Odanın adı/numarası köşede (sol alt, %5 içeride) **0.8 sn** belirir, "ODA 07" 32 px, geçiş bitince kendiliğinden söner. Oyuncu kontrolünü kaybetmez, hiçbir tuş beklenmez.
- İsteğe bağlı: yeni dünyaya ilk girişte (asit dışı tema, ileride) tek seferlik "DÜNYA 2" arcade bandı (1.2 sn, yatay kayan şerit, pembe #7).
- Ölüm/respawn sırasında **hiçbir UI yok** (GDD §5.3 anında). Ölüm sayacı (ister isterse) HUD köşesinde 1 kare "x14" olarak artar, ekran ortasına "ÖLDÜN" yazısı **koyma**.

```
 yeni oda girişi, sol alt köşe, ~0.8 sn:
 +-----------+
 | ODA  07   |  (panel_dark, pembe şerit)
 +-----------+
```

---

## 6) Hareket ve juice

Prensip: UI animasyonu bilgi taşır; <= 200 ms; pixel art'a uygun olarak **adım adım (kareli, 8-12 fps)**; yumuşak ease yerine `Ease.Linear` veya `Ease.OutBack` kısa; piksel gridine oturan tam sayı hareket (`anchoredPosition` tam sayı katları, 4 px = 1 sanat px).

| Olay | Animasyon | Süre |
|---|---|---|
| Menü açılış | panel yukarıdan 8 px adımlarla iner + CRT "açılış" çizgisi (tek yatay beyaz çizgi genişler) | 150 ms |
| Menü kapanış | tersi, 2 kare | 100 ms |
| Odak değişimi | halka imleç yeni butona "atlar" (kancalanma hissi: 2 ara kare), buton 8 px sağa | 80 ms |
| Onay (A) | buton dolgusu limon, 1 kare 1 px aşağı | 60 ms |
| Sayaç artışı | rakam 8 px yukarı zıplar, ikon 2 kare parlar | 120 ms |
| Hak yenilendi (dash/sling) | ikon 1 kare beyaza yakın krem, sonra doluya | 80 ms |
| Sonuç satırları | sırayla soldan kayarak girer, 120 ms aralık | toplam ~400 ms |
| Geçişler | **yalnızca geçişte** 1-2 kare yatay RGB kayması (aberasyon) | 60 ms |

Araçlar: DOTween (projede, `Resources/DOTweenSettings.asset`) + `SetUpdate(true)` (pause'ta çalışsın); mevcut EasyTextEffects/`UIEffects` (`UIAnimatorController` ve `UIEffects/*.asset` var, ne kadarının UI ekranlarına uygun olduğu doğrulanmadı). `Time.timeScale = 0` altında animasyon: `SetUpdate(true)`.

Ses (SFX; gerçek ses üretimi bu yönün işi değil, tür öneriyorum, GDD §8: geçici chiptune):
- Odak değişimi: kısa "tik" (square wave, 30 ms, hafif yükselen ton).
- Onay: iki notalı yükselen "bip-bop".
- Geri: düşen tek nota.
- Sayaç: coin benzeri 2 nota.
- Menü müziği: loop eden kısa chiptune; mevcut ses sistemi `AudioCueSO` event channel ile oynatılır (CLAUDE.md), UI tarafında doğrudan AudioSource yerine `AudioCuePlayEventChannelSO` kullan.
- Lisans: GDD §12 üçüncü taraf müzik riski; UI sesleri kendi üretimin (sfxr/jsfxr gibi araçlar, lisans sorunu yok) olsun. Aracın tam lisans metni doğrulanmadı.

Haptik: odak değişimine çok hafif titreşim **önermiyorum** (gamepad pil/his gürültüsü); onayda isteğe bağlı kısa tık, mevcut `HapticManager` + SO deseni ile, tamamen opsiyonel.

---

## 7) Üretim planı

### Referans çözünürlük ve ölçek
- **480x270 sanat gridi, 4x = 1920x1080.** Skill tablosuna göre 4K'da tam 8x, 1440p'de kesirli (5.33x) olur; 1440p için ya `Canvas Scaler` Reference 1920x1080 ile kesirli ölçek kabul edilir (bulanıklık riski) ya da Windowbox. PC'de çoğu oyuncu 1080p veya 4K varsayıyorum; 1440p testi gerekir.
- Alternatif 640x360 (3x): daha ince detay, ama geliştirici için fazla iş; **önermiyorum**.
- Oyun dünyası piksel yoğunluğu ile UI piksel yoğunluğunun aynı olup olmadığı **doğrulanmadı**; moodboard'da node/boru/zemin yoğunlukları da tutarsız (GDD §8.1). Aynı kabaca 4x yoğunluğa getirmek hedef, ama dünya sprite'ları UI kapsamı dışı.

### Araçlar (dürüst değerlendirme)
| Araç | Karar |
|---|---|
| **Aseprite** | Ana araç. 9-slice, palet kilidi, indexed mod, sprite sheet export, animasyon. Ücretli (tahmini ~20 USD; güncel fiyat doğrulanmadı). Kaynaktan derleme ücretsiz ama zahmetli. |
| **LibreSprite / Piskel** | Ücretsiz yedek; Aseprite'a göre zayıf (özellikle 9-slice/etiketleme/tilemode). Piskel tarayıcıda hızlı taslak için iyi. LibreSprite fork'u, güncelliği doğrulanmadı. |
| **Figma** | Yalnızca **ekran düzeni (wireframe)** için. Piksel çizimi için değil: skill'e göre "Figma'daki netlik CRT'de bozulur"; ayrıca bu ortamda Figma bağlantısı kimlik doğrulaması gerektiriyor. Gerek yok; ASCII + Unity beyaz kutu yeterli. |
| **Unity'nin kendisi** | Beyaz taslak (greybox) burada kurulur, `MainMenuBuilder` benzeri bir builder ile (mevcut desen). |
| **AI görsel araçları** | **UI sprite'ı için pratikte işe yaramaz**: gridden/palete taşan, tutarsız piksel boyutlu, bulanık kenarlı çıktı verir; temizlemek sıfırdan çizmekten uzun sürer. Sınırlı yararı: (a) konsept/mood taslağı, (b) başlık logosu için fikir, hepsi elle yeniden çizilmeli. SpriteCook gibi servisler hesap/kredi ister, Godot odaklı, **önermiyorum** (digest ile aynı). AI üretimi görsellerin lisansı/telif durumu da belirsiz (ticari yayında risk; doğrulanmadı). |
| **Claude/Python ile piksel üretimi** | `pixel-art-studio` gibi script yaklaşımı kutu/9-slice/ikon gibi geometrik şeyler için **gerçekten işe yarayabilir** (deterministik, palet kilitli). Denenmedi, scriptleri okunmadı; kullanmadan önce incelenmeli. Bu yön için en umut verici otomasyon: panel ve buton çerçeveleri koddan üretilir. |

### Taslak mı final mı
**Önce beyaz taslak, sonra final.** Sıra:
1. Tek renkli (flat) beyaz/gri 9-slice kutularla **tüm ekranlar Unity'de** kurulur (yerleşim, odak akışı, gamepad navigasyonu test edilir). Bu, sanat harcamadan önce akışı kanıtlar.
2. Akış onaylanınca "hero asset": **tek panel + tek buton ailesi** final çizilir, CRT açıkken 1080p'de değerlendirilir.
3. Gerisi (ikonlar, imleç, başlık) bu iki asset'ten türetilir.

### Dosya ve isimlendirme
`ui_[nesne]_[varyant]_[durum]`: `ui_button_primary_focus`, `ui_panel_main_9s`, `ui_cursor_ring_01..04`, `ui_icon_pineapple`, `ui_btn_a`. Sprite sheet'ler Aseprite'tan PNG-32 olarak; ham `.aseprite` kaynakları `Assets/UI/Source/` altında (veya repo dışı; büyük değil). Bir **asset manifest** (isim, boyut, border, durum, kaynak, lisans) `docs/ui-research/` altında bir tablo olarak tutulur.

### Export ve Unity import ayarları
- Aseprite: **1x** çiz, export'ta ölçekleme yok (4x ölçek Unity'de Canvas Scaler ile gelir). PNG-32, şeffaf, 1-2 px padding. Boyutlar çift sayı.
- Unity Texture: Sprite (2D and UI), **PPU 1** (UI için) **veya** sprite başına tutarlı bir PPU; Canvas Scaler Reference 1920x1080 ile 1 sanat px = 4 ekran px elde etmek için `Image.pixelsPerUnitMultiplier` ya da PPU değerini **tam olarak 0.25** değil, kendi `CanvasScaler.referencePixelsPerUnit` (varsayılan 100) ile sprite PPU'yu eşleyerek hesapla (**bu hesabın Unity 6 uGUI'de tam sonucu doğrulanmadı**; prototipte 16x16 test sprite'ı ile 64x64 ekran pikseli çıkıyor mu ölç).
- Filter Mode **Point (no filter)**, Compression **None**, Generate Mip Maps **kapalı**, Wrap Mode Clamp (9-slice/tile için `Repeat` yalnızca tile sprite'larda).
- 9-slice: Sprite Editor'da Border L/B/R/T = 1x piksel; Image Type Sliced.
- Sprite Atlas: UI için tek atlas, Point, Tight Packing kapalı, Padding >= 4.
- **Canvas ve CRT** (önemli): Mevcut menü `Screen Space - Overlay` + Canvas Scaler 1920x1080, Match 0.5 (`MainMenuBuilder`). Overlay canvas post-process/CRT'ye **girmez** (kamera dışı; Edit Mode captures bile Overlay'i göstermiyor, CLAUDE.md). CRT'nin UI'ı da etkilemesi isteniyorsa iki yol: (a) `Screen Space - Camera` (CRT'li kamera/renderer feature'ı kapsıyorsa; CRT'nin kamera renderer feature'ı mı ekran materyali mi olduğu **doğrulanmadı**, `M_CRTFilter.mat` var) ya da (b) overlay üzerinde ayrı hafif scanline Image. Önerim: **UI'ı CRT'nin dışında bırak, dekoratif tek scanline overlay ile "yaklaştır"**: okunabilirlik (A yönü için bile) daha değerli, ve bu seçim en az riskli.
- `Pixel Perfect Camera` (URP, `UnityEngine.Rendering.Universal`) yalnızca dünya kamerası için; UI için `upscaleRT` **kapalı** olmalı (skill: UI metni bulanıklaşır, bilinen sorun). Projenin sahnelerinde Pixel Perfect Camera var mı bu çalışmada doğrulanmadı (sahnelerde bir eşleşme çıktı ama detayına bakılmadı).
- `InputSystemUIInputModule` zaten kullanılıyor (iyi).

---

## 8) İş yükü tahmini

Kurallar: haftada 2 akşam (Pazartesi: #art/#test, + en fazla 2 #level; Cuma: #code/#level), akşam başına en fazla 3 task, her biri 1-1.5 saat. Tahminler kaba; sanatçı olmayan biri için çizim süresi **doğrulanmadı**, +%50 tampon önerilir.

| # | Akşam | Task | Tür | Süre |
|---|---|---|---|---|
| 1 | Pzt | Palet: moodboard sprite'larından örnekle, 14 renk Aseprite paleti | #art | 1 sa |
| 1 | Pzt | Tiny5 testi: native px, Türkçe karakter, Raster atlas | #test | 1 sa |
| 2 | Cum | Beyaz taslak: ortak UI kit (panel prefab, buton prefab, odak/CanvasScaler ayarları) | #code | 1.5 sa |
| 3 | Pzt | Hero asset: `ui_panel_main_9s` + 9-slice import | #art | 1.5 sa |
| 3 | Pzt | Hero asset: buton 4 durumu | #art | 1 sa |
| 4 | Cum | Ana menü yeniden kurulum (beyaz taslak -> sprite) + gamepad focus | #code | 1.5 sa |
| 5 | Pzt | Odak imleci halka (4 kare) + ok işareti | #art | 1.5 sa |
| 5 | Pzt | Gamepad ikonları (A/B/X/Y/LB/RB/LT/RT/Stick) 16x16 | #art | 1.5 sa |
| 6 | Cum | Pause ekranı + DOTween açılış/kapanış (`SetUpdate(true)`) | #code | 1.5 sa |
| 7 | Pzt | HUD ikonları (ananas, anahtar, dash, slingshot dolu/boş) | #art | 1.5 sa |
| 8 | Cum | HUD: sayaç + dash/slingshot göstergesi (olay güdümlü) | #code | 1.5 sa |
| 9 | Cum | MP sonuç ekranı yeniden tasarım | #code | 1.5 sa |
| 10 | Pzt | Başlık logosu "ROCK THE GAME" (el çizimi, 128 px) + menü arka planı halkaları | #art | 1.5 sa |
| 11 | Cum | Oda adı bandı + UI SFX bağlama (AudioCue channel) | #code | 1.5 sa |
| 12 | Pzt | CRT altında 1080p/1440p doğrulama, kontrast + safe area, gamepad ile baştan sona test | #test | 1.5 sa |

**Toplam: yaklaşık 12 akşam (~6 hafta)**, tampon ile 14-15 akşam (7-8 hafta). Kritik: akışı doğrulayan beyaz taslak (akşam 2) ilk sırada olmalı, sanat ondan sonra. Bu liste `TASKS.md`'ye **eklenmedi**; ekleme kullanıcı onayı bekler (CLAUDE.md). Mevcut MP/diğer işlerle çakışma bilinmiyor.

---

## 9) Riskler

1. **CRT + UI okunabilirliği (en büyük risk).** Küçük Tiny5 metin scanline altında bulanabilir. Önlem: UI'ı CRT dışında tut veya gövde metnini >= 32 px, test 12. akşamda. Kenar vinyeti köşe bilgisini karartır.
2. **Sanat becerisi.** Sanatçı değil; chamfer'lı kutular ve ikonlar yapılabilir, ama başlık logosu/halka imleç düşük kalitede çıkabilir. Önlem: geometrik (kutu/çizgi) odaklı tasarım, hero asset onay kapısı.
3. **Gürültülü stil + oyun hızı.** Arcade yön animasyon/ses dolu; yanlış doz oyuncuyu yorar ve "akışı keser". Önlem: oyun içinde (HUD) minimum, juice yalnızca menüde.
4. **Gökkuşağı çakışması.** Odak halkası oyundaki grapple node'larıyla karışabilir (aynı dil). Önlem: UI halkası küçük, yalnızca menüde; HUD'da kullanma.
5. **Palet tahmini.** Hex'ler gözle alındı, CRT renk kaydırması dahil olabilir. İlk iş örnekleme.
6. **Tiny5 kapsamı** (Türkçe karakter, native boyut) doğrulanmadı; SDF atlas pixel fontla uyumsuz olabilir.
7. **Ölçekleme.** 1440p/ultrawide'da kesirli ölçek; Windowbox/letterbox kararı gerek. Steam oyuncu kitlesi 1080p ağırlıklı varsayımı doğrulanmadı.
8. **Tek dünya paleti.** GDD §8: ileride çok farklı dünyalar gelecek; UI'ın "sabit kabin" olması iyi, ama dünya paleti (örneğin pembe-mavi-turuncu) UI'ın pembe/gök rengiyle çakışabilir. Önlem: UI'ı koyu çekirdek (#1-#3) üstünde tut, renkli kısım küçük.
9. **GDD §11'e göre ayarlar, kayıt, bitiş ekranları yok.** Ayarlar/kayıt ekranlarının tasarımı bu belgede yok (menüde yer ayrıldı); sonra eklenecek ekranlar aynı bileşenleri kullanmalı.
10. **İçerik kapsamı:** MP sonuçtaki ananas/ek bilgiler, MP'de pause davranışı, `RoomTransitionCoordinator` ile bant zamanlaması gibi noktalar kod okunmadan varsayıldı.
11. **A'nın zayıf yanı:** Parlak/enerjik ton, "sevimsiz taraf da var" (GDD §8) ve ileride karanlık dünyalarla uyumsuz kalabilir. Bu bilinçli uyumsuzluk kabul edilirse sorun değil; etmezse nötr bir yön (B/C) daha esnek olur.

---

## Özet (5 satır)
1. Yön: UI = oyunun CRT'sinin devamı: koyu lacivert paneller, krem metin, tek vurgu (bullseye turuncu), gökkuşağı halkası odak imleci, pembe damla şeridi imzası.
2. Teknik: 480x270 sanat gridi (4x), 14 renk (hex'ler gözle tahmin, örneklenmeli), Tiny5 32/64/128 px (native boyut ve Türkçe karakter doğrulanmadı), 9-slice + Point filtre.
3. Öneri: UI'ı CRT'nin dışında bırak (overlay), tek hafif scanline ile yaklaştır; önce beyaz taslak ve hero asset, sonra final.
4. Araçlar: Aseprite ana araç; AI görsel araçları UI sprite'ında pratikte yaramaz; script ile geometrik üretim denenebilir (doğrulanmadı).
5. İş yükü: ~12 akşam (tamponla 14-15), akşam başına <=3 task; en büyük riskler CRT altında okunabilirlik ve sanat becerisi.
