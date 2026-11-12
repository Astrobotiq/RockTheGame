# Yön B: "Oyun Dünyasından Doğan UI" (dünya malzemesi)

Tarih: 2026-11-08. Durum: araştırma önerisi, uygulanmadı. Okunanlar: `skills-digest.md`, GDD §1-§5 ve §7-§8, moodboard 01/05/10/12/15/16/20/21, `UI/UIAnimatorController.cs`, `MainMenuBuilder.cs` (ana menü), `unity:2d-pixel-perfect` ve `unity:ui-ugui` skill'leri.
Doğrulanmayan her şey "doğrulanmadı" diye işaretli.

---

## 1) Vizyon ve neden uyar

**Fikir.** Menü, oyunun içinde duran bir nesne gibi görünür: turuncu karton/toprak levhalar, üstünde pembe kremalı şerit kenar, çevresinde level_01 ve level_05'teki koyu borular, köşelerde vana ve dirsek. Başlık bir tabela, ayarlar bir pano. Odak imleci **kanca node'unun minyatürü** (konsantrik halka). Butonlar zaten "tutunulacak hedef" gibi davranır, yani menüde gezmek oyunun ana fiilini (hedefe tutun) prova eder.

**Neden bu oyuna uyar**
- **Sütun 2 (kanca merkezde).** Node, moodboard'daki en baskın öğe. Odak halkasını node'dan türetmek menüyü bu dile bağlar; yeni görsel dil icat etmez.
- **Sanatçı olmayan tek geliştirici.** Gerekli parçaların çoğu oyunda zaten var: toprak dokusu, pembe şerit, boru sprite sayfası (`Assets/Sprites/Pipe/plums.png`, 800x608, PPU 16, Point filtre; kare boyutu doğrulanmadı), node halkaları, ananas, kırmızı pad, Gnome'lar. Sıfırdan stil yok, yeniden kullanım ve kırpma var.
- **Sütun 1 ve 3 (akış, hızlı respawn).** Dünya malzemesi dili, süslü giriş/çıkış animasyonlarını gerektirmez; statik doku zaten "ağırlıklı" hisseder. Animasyon bütçesi kısa kalır (bkz. §6). Ölümde UI hiç çıkmaz.
- **Tek dünya şimdilik.** Tüm 22 oda aynı paleti paylaşıyor (GDD §8.1). İleride başka dünyalar (GDD §8) gelirse paleti tek bir `UIStyleSO` ile değiştirmek mümkün (bkz. §4.6).
- **CRT ve Gnome.** Gnome'lar zaten "çocuk kitabı illüstrasyonu x pixel dünya" çatışması olarak bilinçli kullanılıyor (GDD §8). Sonuç ekranında iki Gnome (level_21'deki gibi) kullanmak yeni bir stil karışımı eklemez.

**Bedeli (dürüst).** Bu yön "temiz ve hızlı okunur UI"dan çok "karakterli UI". Karakter, okunurluğun üstüne çıkarsa kaybedilir: dokulu turuncu üstünde krem metin yeterli kontrast vermez. Bu yüzden her metin **koyu mürekkep panel** üstünde durur (turuncu karton yalnızca çerçeve ve süs, metin zemini değil). Bkz. §2 ve §9.

---

## 2) Palet

Kaynak: moodboard'dan piksel örnekleri (level_01, 05, 21). **Uyarı:** çekimler CRT filtreli ve tarama çizgili; tek piksel okumaları gölgeli çıkabilir (ör. pembe şerit #AD2485 okundu, ama ekranda daha parlak görünüyor). "Örnek" sütunu ham okumadır; "Öneri" kullanılacak değerdir. Öneriler **kaynak sprite'lardan pipetle doğrulanmalı** (doğrulanmadı).

| # | Rol | Öneri | Örnek (CRT'li çekim) | Kullanım |
|---|---|---|---|---|
| 1 | Gök mavisi | `#86D4F7` | `#8EDCFF` | Menü arka planı (sahne zemini), boş alan |
| 2 | Toprak turuncu | `#F2A465` | `#F4A463` / `#FFBD77` | Panel çerçeve gövdesi |
| 3 | Toprak gölge | `#C97A4A` | tasarım | Çerçeve alt/iç gölge (hue-shift: kırmızıya) |
| 4 | Toprak koyu | `#7C3F3A` | tasarım | Çerçeve dış kontur, pressed durumu |
| 5 | Pembe krem şerit | `#EE3FA8` | `#AD2485` (gölgeli) | Panel üst kenar şeridi, odak vurgusu |
| 6 | Pembe koyu | `#AD2485` | `#AD2485` | Şeridin damla altı |
| 7 | Boru koyu (mürekkep) | `#433646` | `#433646` | Metin paneli zemini, boru gövdesi, tek outline rengi |
| 8 | Boru gece | `#2A2233` | tasarım | Metin gölgesi, en koyu nokta (saf siyah yok) |
| 9 | Boru çelik | `#9C9AA3` | gri borulardan gözle | Boru parlak çizgisi, pasif ikon |
| 10 | Krem | `#F5EFCB` | `#DCE1C1` (halka) | Metin, node dış halka |
| 11 | Teal | `#176D83` | `#176D83` | Node halkası, ikincil vurgu |
| 12 | Zeytin | `#6E8F3A` | `#508E3E` | Node halkası, yeşil boru |
| 13 | Sarı | `#FFC21A` | `#FFE714` (çekirdek) | Seçili/odak merkezi, ananas |
| 14 | Turuncu-kırmızı | `#FF3A00` | `#FF2E00` | Node halkası, uyarı (stamina) |
| 15 | Asit yeşil | `#21D225` | `#21D225` | "Tamam/kazandın" ve asit; UI'da dar kullan |
| 16 | Kırmızı pad/NPC | `#F04229` | `#F04229` | Tehlike, "kaybettin", Gnome şapkası |

16 renk. Sınırlar (skills-digest'ten):
- **Gökkuşağı halka yalnızca "tutunulabilir/etkileşim" anlamında** (odak halkası, seçili buton işareti). Başlıkta, süste veya arka planda kullanma.
- **Anlam yalnızca renkle verilmez.** Kazandın/kaybettin için asit yeşil/kırmızı yanında ikon ve metin var (§5.5).
- **Alpha yalnızca 0/255**, yarı saydam kenar yok (CRT'li sahnede halo yapar). Tek istisna: node'un dış parıltısı dünyada var (moodboard'da yarı saydam pembe-mor halka); UI odak halkasında **kullanma**, sert pikselle çiz.
- Metin zemini: `#433646` üstünde `#F5EFCB`. Kontrast hesabı yapılmadı; yaklaşık olarak çok yüksek olmalı (koyu mor-gri / krem), ama CRT'li görüntüde ölç (doğrulanmadı).

---

## 3) Tipografi

- **Font:** Tiny5 (`Assets/UI/Fonts/Tiny5-Regular.ttf` + SDF asset mevcut). Projede ayrıca ~12 Kenney fontu duruyor; **hepsi menüden çıkarılmalı**, tek font = Tiny5. Dünyadaki öğretici yazılar (Move Right Arm vs.) iki farklı font ve boyutta (GDD §8.1), UI'a geçerken bunlar da Tiny5'e toplanmalı.
- **Boyut kuralı:** yalnızca Tiny5'in **native piksel gridinin tam katları**. Native em boyutu **doğrulanmadı** (Tiny5 5 piksel yüksekliğinde bir font olarak biliniyor; yüksek ihtimalle 8 px em; Aseprite'te `.ttf`'i 8 pt ile rasterleyip bak). Varsayım em = 8 sanat pikseli, referans grid 480x270 (4x):

| Rol | Sanat px (em) | Ekran px (1080p) | Not |
|---|---|---|---|
| Gövde, buton | 8 | 32 | skills-digest eşiği >= 26 px'i sağlar |
| Başlık (panel) | 16 | 64 | |
| Logo/ana başlık | 24-32 | 96-128 | tabela üstü |
| İpucu/alt yazı | 8 | 32 | alt sınır; 8'den küçük kullanma |

- **TMP ayarı:** Point sampling (SDF atlasında mümkün olduğunca), outline yok, gölge yerine **1 sanat piksel (4 ekran px) sert gölge** (`#2A2233`, +1,-1 ofset); Rich text yok. SDF yerine **bitmap/raster TMP atlas** (Render Mode Raster ya da Raster Hinted) pixel-art için daha sağlam; mevcut asset `m_AtlasRenderMode: 4165` yani SDF-türü (SDFAA); raster'a çevirmek keskinliği artırır (doğrulanmadı, test et).
- **Türkçe karakter** (ş ğ ı İ ö ü ç): mevcut ana menü "Tek kişilik, Çok oyunculu, Çıkış" yazıyor, yani en azından Tiny5 ile çalışıyor gibi; **tüm karakter seti atlasta mı doğrulanmadı**, atlas karakter listesine Latin Extended-A ekle.
- **Dil:** oyun içi yazılar şu an İngilizce (USE L TO WALK), menüler Türkçe. Karar geliştiricinin; öneri: menü Türkçe + ikon ağırlıklı, öğretici İngilizce kalabilir. Metin container'a göre büyür, resme gömülmez.
- **CRT:** CRT filtresi `RenderPassEvent.AfterRenderingPostProcessing`'e enjekte (`CrtRendererFeature.cs`). **Screen Space - Overlay canvas'ın CRT'ye girip girmediği doğrulanmadı.** URP'de Overlay UI genellikle post-process'ten sonra çizilir, yani büyük ihtimalle CRT'siz kalır. Bu yön için kritik karar: bkz. §9 risk 1.

---

## 4) Bileşen dili

### 4.1 Ölçek ve grid
- **Referans:** 480x270 sanat pikseli, 4x = 1920x1080 (skills-digest ve `2d-pixel-perfect` skill'i ile uyumlu). Bütün UI sprite'ları bu grid'in tam katı. Sprite boyutları **çift sayı**.
- Canvas: Screen Space Overlay (ya da Camera, bkz. §9), `CanvasScaler` Scale With Screen Size 1920x1080, Match 0.5 (mevcut menü builder'ı zaten böyle). Pixel-art'ta alternatif: Constant Pixel Size + `Reference Pixels Per Unit` (skill: "pixel-art için Constant Pixel Size gerekçe olabilir"); 1080p tek hedef olduğundan Scale With Screen Size + 4x yeterli. Fraksiyonlu çözünürlükte (1440p = 5,33x) kenarlar bozulur; öneri **Windowbox/integer fit** (doğrulanmadı: projede PixelPerfectCamera kullanılıp kullanılmadığına bakmadım).
- Sprite import: Point (no filter), Compression None, Mip Maps kapalı, PPU **16** (projede mevcut: plums.png ve UI-Sprites PPU 16; UI Image'larda PPU önemli çünkü `Pixels Per Unit Multiplier` ile 9-slice kalınlığı ölçeklenir. Aşağıdaki 4x ölçeği `Image.pixelsPerUnitMultiplier` ile ya da CanvasScaler ile sağla, ikisini birden değil).

### 4.2 Panel (hero asset 1)
"Toprak levha": turuncu karton gövde, üstte pembe şerit (damla kenarlı, level_01'deki gibi), dışta 1 piksel koyu kontur, **içinde `#433646` metin yuvası**.

```
 pembe şerit (3 px, damla)
+================================+
|##  turuncu karton gövde  ####  |  <- dış çerçeve 6 px
|##  +----------------------+ ## |
|##  |  mürekkep metin yuvası| ##|  <- iç yuva 1 px krem iç çizgi
|##  +----------------------+ ## |
+================================+
```
- **9-slice:** kaynak 24x24 sanat px (96x96 ekran), köşe/border **6 px** her yönde. Kenar tile (stretch değil). Image Type = Sliced, Fill Center açık.
- Damla şerit kenarı yatay tile'lanır; 9-slice'ın üst orta bölgesi 8 px'lik tekrar eden desen olmalı (3x3 tekrar testi: dikişsiz mi).
- **Varyantlar (hepsi aynı slice, aynı border):** `ui_panel_main_9s` (büyük), `ui_panel_plaque_9s` (küçük tabela), `ui_panel_dark_9s` (HUD, yalnızca mürekkep + çelik kenar, turuncu yok). 3 sprite yeter.

### 4.3 Boru süsü (hero asset 2)
Panel köşelerinden ve kenarlarından çıkan boru: yatay/dikey tile boru (8 px kalınlık), dirsek (4 adet köşe), T-bağlantı, vana (tekerlek). Kaynak: `plums.png`'den kırp (zaten oyunda, stil eşleşir). **Süs olarak** kullan: butonlara bağlanma, etkileşimi kesmez. Boru genişliği 8 sanat px, çelik parlak çizgisi (#9C9AA3) 1 px.
- Kullanım kuralı: bir ekranda en fazla 2-3 boru parçası; fazlası görsel gürültü (level_01'deki boru yoğunluğu ekranın tamamını kaplayan arka plan içindi, UI'da küçük tut).
- Vana tekerleği **hareketli süs**: odak değişince 1 kare döner (bkz. §6).

### 4.4 Buton ailesi (hero asset 3)
Buton = "küçük boru tabela". Aynı sprite boyutu ve slice sınırı 4 durumda (focus'ta zıplamaz):

| Durum | Görünüm |
|---|---|
| Normal | Mürekkep (#433646) gövde, turuncu çerçeve, krem metin |
| Focus | Çerçeve pembe şerit rengine (#EE3FA8), solda node imleci (§4.5), metin sarı (#FFC21A), gövde 1 piksel yukarı |
| Pressed | Çerçeve koyu (#7C3F3A), gövde 1 piksel aşağı (basılmış), metin krem |
| Disabled | Boru çelik (#9C9AA3) çerçeve, metin #9C9AA3, node yok, üstünde "kilit" yok (ikon karmaşası) |

- Boyut: 128x24 sanat px (512x96 ekran), border 6; genişlik container'a göre uzar (Layout Group), metin taşmaz.
- Boyut ailesi: geniş (128x24), orta (80x24), kare ikon (24x24). Üç boyut.

### 4.5 Focus göstergesi: node imleci
- **Sol tarafta 16x16 sanat px (64x64 ekran) mini node**: dıştan içe krem, teal, zeytin, sarı, turuncu-kırmızı, çekirdek sarı (moodboard'daki halka sırası). **Hepsi sert piksel**, parıltı yok.
- Hareket: iki kareli "nefes" animasyonu (halka 1 piksel genişler/daralır), 4 FPS. Yatayda butonla birlikte 1 piksel kayar (zıplayan ikon, rengin tek başına anlam taşımaması kuralı).
- Aynı anda buton çerçevesi pembeye döner: **ok/ikon + şekil + renk** üçlüsü sağlanır.
- Stretch fikir (zorunlu değil, §8'de ayrı kalem): A'ya basınca node'dan butona tek karelik "kanca çizgisi" atılır. Güzel ama menü hızını düşürebilir (Sütun 1); varsayılan kapalı.

### 4.6 Veri: `UIStyleSO`
Projedeki kural (SO başına sistem, `PlayerStatsSO` deseni): renk token'ları (§2 rolleri), sprite referansları, font ve boyutlar tek `UIStyleSO`'da. Ekranlar renkleri kodda sabitlemez. Gelecekte yeni dünya = yeni SO.

### 4.7 Gamepad ikonları
- Projede UI hareketli ikonları var: `UIAnimatorController` + `UIAnimations` enum'u (LS yatay/dikey/tam, RS, A, X, LB, RB, LT, RT) ve Animator ile oynuyor; dünya öğreticilerinde beyaz daire içinde "L" görünüyor (level_01, level_05). Bu hâli **placeholder** kabul et (çubuk üstüne beyaz daire, GDD §8.1 uyumsuzluk 2).
- Öneri: tuş ikonu = **kalın daireli/yuvarlak tuş + harf**, 12x12 (ya da 14x14) sanat px, mürekkep zemin + krem harf + 1 px çelik kontur. Renk kodu *yok* (Xbox A yeşil gibi platform renkleri kullanma; palet dışı renk getirir).
- Kapsam: A, B, X, Y(opsiyonel), LT, RT, LB, RB, sol stick, D-pad, Start. **PlayStation sembolleri** (GDD §5.1 "A / ✕" der): ya tek harfli nötr set (A/B/X/Y) ya da iki set. Öneri: **tek nötr set** (yüz tuşu konumuna göre: Güney, Batı, Doğu, Kuzey sembolleri harf yerine). Karar geliştiricinin; ikinci set +2-3 akşam, v1'de yok.
- İkon + etiket satırı alt şeritte: `[A] Seç   [B] Geri` (sağ alt, güvenli alan içinde).
- Öğretici yazılardaki ikonları da aynı sete geçirmek tutarlılık için gerekir, ama o iş level işi (`#level`), bu belgenin kapsamı dışı.

### 4.8 Güvenli alan
Kritik UI kenardan %5 (96x54 ekran px = 24x13 sanat px) içeride. CRT vinyeti ve ekran eğriliği köşeleri yutuyor (moodboard'da köşeler koyulaşıyor, kenar bükülüyor), bu yüzden HUD köşelerinde **kritik** bilgi yok, sayaçlar köşeden >= 24 sanat px içeride.

---

## 5) Ekranlar

Hepsi 1920x1080. Metin wireframe'leri 480x270 grid'in ~1/4 ölçeğinde. `[ ]` = buton, `(o)` = odak node'u, `~` = boru, `#` = toprak levha.

### 5.1 Ana menü
Arka plan: gök mavisi (#86D4F7) düz zemin + level_01'deki gibi üstte ve altta toprak şerit (pembe damla kenarlı) boru süsüyle. İki büyük node (level_01'deki "göz" gibi) başlık arkasında dekor olabilir; ama **gökkuşağı yalnızca etkileşim** kuralı yüzünden dekor node'lar **sönük/tek tonlu** (teal tonları) çizilmeli, yoksa odak ile karışır.

```
+----------------------------------------------------------------+
| ####pembe damla####################################~~~~~~~~~~  |
| ~~~~~~~~~~                                                     |
|                                                                |
|             +--------------------------------+                 |
|             |        ROCK THE GAME           |  <- tabela      |
|             +--------------------------------+     (başlık)    |
|                                                                |
|                  (o) [ Tek kişilik   ]                         |
|                      [ Çok oyunculu  ]                         |
|                      [ Ayarlar       ]  (v2)                   |
|                      [ Çıkış         ]                         |
|                                                                |
|  [A] Seç                                                       |
| ~~~~~~~~~~~                                   ~~~~~~~~~~~~~~~~ |
| ####pembe damla#######################################        |
+----------------------------------------------------------------+
```
- **Gnome:** köşede küçük Gnome (128x128 illüstrasyon, ekranda 128-192 px; tam sayı katı olmak zorunda değil çünkü zaten pixel-art dışı) düdük gibi 1 satır diyalog (4. duvar şakası, GDD §7); isteğe bağlı, v2.
- Açılışta `EventSystem.SetSelectedGameObject` = "Tek kişilik" (mevcut builder `firstSelectedGameObject` set ediyor; doğru). Wrap açık (aşağı basınca son butondan ilke dön).
- Mevcut durum: builder düz renkli Kenney butonları kuruyor (başlık `120f`, Layout Group spacing 32). Yeniden kurulumda aynı `MainMenuBuilder` yapısı (`VerticalLayoutGroup`) korunur; yalnızca sprite/stil atanır. Yani kod değişikliği küçük, iş çoğunlukla sanat.

### 5.2 Pause
Dünyanın üstünde, oyun donuk; arkada sahne **koyu mor filtre değil**, 2 renkli dither maske (#2A2233, 50% sabit dama deseni: yarı saydam değil, alpha 0/255 kuralı korunur).

```
+--------------------------------------------------+
|        (oyun donuk, dama deseni ile karartılmış)   |
|            +------------------------+              |
|            |        DURAKLADI       |  <- plaket   |
|            +------------------------+              |
|        ~~  | (o)[ Devam            ] |  ~~          |
|        ~~  |    [ Odayı yeniden doğ ]|  ~~          |
|            |    [ Ayarlar          ] |              |
|            |    [ Ana menü         ] |              |
|            +------------------------+              |
|   [A] Seç    [B] Devam    odaya: 07                |
+--------------------------------------------------+
```
- B/Start = hemen devam. Açılış/kapanış **<= 100 ms**, yoksa hızlı akış bozulur.
- "Odayı yeniden doğ" yeni bir mekanik değil mevcut respawn'u çağırır; GDD'de karar yok (**GDD'de pause tanımı yok**; "Ana menü, pause, ayarlar, bitiş: yok" §11). Menü kalemleri geliştiricinin kararı. Onay penceresi gereken yalnızca "Ana menü" (kayıt yok, kayıp ilerleme: GDD §11 "save: yok").
- Çok oyunculuda pause **oyunu durdurmaz** (hayalet yarış, her oyuncunun dünyası kendine ait); bu panel MP'de "oyun devam ediyor" uyarısı taşımalı. MP tasarımı doğrulanmadı, `systems/multiplayer.md`'ye bakılmalı.

### 5.3 HUD
**İlke:** aksiyon sırasında minimum. Her şey olay güdümlü (EventChannelSO / `PlayerHealth.OnDeath`), `Update`'te polling yok.

```
+--------------------------------------------------+
|   [anahtar ▢▢■]                      [ananas x 12]|  <- yalnızca olay olunca
|                                                    |
|                                                    |
|                (oyun alanı, UI yok)                |
|                                                    |
|  (dash ●)(sling ●)  <- yalnızca TÜKENİNCE görünür  |
+--------------------------------------------------+
```
- **Ananas sayacı:** 16x16 ananas ikonu (oyundaki sprite'ın aynısı) + Tiny5 rakam, sağ üst, `ui_panel_dark_9s` içinde. Toplayınca **belirir (1,5 sn) sonra çekilir**; sürekli durmaz. Neden: GDD §12.4 "toplanabilirlerin amacı belirsiz"; kalıcı sayaç anlamı olmayan bir sayı gösterir. Karar bekliyor.
- **Anahtar parçaları:** oda içinde yuva sayısı kadar küçük anahtar yuvası (boş=mürekkep çizgi, dolu=sarı). Yalnızca anahtarlı odalarda ve toplanınca belirir; ölünce sıfırlanır (GDD §5.3: anahtarlar başlangıca döner), yuvalar boşalma animasyonu **yok**, anında boşalır (hızlı respawn).
- **Dash/slingshot:** GDD §5.2 zaten **karakter renk değişimiyle** bilgi veriyor ("hak tükenince rengi değişir"). HUD'da **ikinci bir kanal eklemek** gürültü. Öneri: HUD göstergesi **yok** varsayılan; istenirse tükenince beliren iki küçük "vana ışığı" (dash=sol, slingshot=sağ, boru üstünde, sönük=tükenmiş), karakterin yakınında değil ekran alt-ortada. Karakter rengi ana kanal kalır. Hakkı doğrulamak için playtest (feel geliştiricinin kararı, CLAUDE.md kural 4).
- **Stamina uyarısı:** duvar stamina'sı zaten uyarı veriyor (GDD §5.2); çubuk yok, mevcut uyarı yeterli. Kırmızı (#FF3A00) 1 piksel kontur karakter etrafında zaten karakter tarafında; UI'ya dokunma.
- **Ölüm:** hiçbir UI. Respawn anında. Ölüm sayacı yok (zor ama adil, öğrenmek kolay; sayaç suçluluk yaratır). Opsiyonel, kapalı: odadaki ölüm sayısını küçük boru plaketinde göstermek (geliştirici isterse).

### 5.4 Çok oyunculu HUD (mevcut `RaceHud` restyle)
Mevcut: sağ üstte "Bayrak N/3", "Rakip: Oda X · Bayrak N/3", üst ortada bağlantı uyarısı, ortada 3-2-1 / BAŞLA! (`systems/multiplayer.md`). Öneri: aynı konumlar, `ui_panel_dark_9s` içinde. Bayrak ikonu 16x16. 3-2-1 sayısı **büyük Tiny5 (32 sanat px = 128 ekran px)**, krem + 1 piksel gece gölgesi, geri sayım ritmine göre 1 kare ölçek vuruşu (100 ms).

### 5.5 MP sonuç ekranı
Mevcut: Kazandın / Kaybettin / Berabere / Birlikte bitirdiniz; A ya da Enter ile lobiye dönülür (`RaceHud`).

```
+--------------------------------------------------+
|  ~~~~                                      ~~~~    |
|         +----------------------------+             |
|         |   (★) KAZANDIN   (★)       |  <- ikon+metin|
|         +----------------------------+             |
|         |  Süre         04:12.3      |             |
|         |  Bayrak       3            |             |
|         |  En hızlı oda 07  (00:21)  |  <- opsiyonel |
|         +----------------------------+             |
|     [Gnome]     (o)[ Lobiye dön ]    [Gnome]       |
|  ##pembe damla######################################|
|   [A] Seç                                           |
+--------------------------------------------------+
```
- Durum her zaman **ikon + şekil + metin** (renk tek başına taşımıyor): Kazandın = yıldız + asit yeşil (#21D225) şerit, Kaybettin = X/çatlak + kırmızı (#F04229) şerit, Berabere = eşit işareti + teal, Birlikte = iki ele sıkışma/iki yıldız + sarı.
- İki Gnome (level_21'deki gibi) iki yanda; kazanan Gnome zıplar, kaybeden sönük (2 kareli animasyon; Gnome sprite'ı 128x128, animasyon controller'ı var: `Gnome.controller`, `IdleWait.anim` vs.). Gnome çözünürlüğü pixel grid dışı; kenar bulanıklığını Point filtre ile ver, ölçeği tam sayı (1x = 128 px ya da 1,5x yok) tut.
- Süre kayıttan devam ediyor (MP notları); süre biçimi `MM:SS.d`. Rakip süresi eklenecekse karşılaştırma satırı (GDD'de yok; veri ağda mevcut mu doğrulanmadı).
- Zorunlu bekleme yok: sonuç görününce A anında çalışır.

### 5.6 Oda geçişi
Mevcut: iris geçiş (`M_IrisTransition`), kamera kayması, oyuncu dondurma (`RoomTransitionCoordinator`). **UI eklentisi minimum**:
- Geçişte köşede 0,7 sn **oda plaketi** ("ODA 07", `ui_panel_plaque_9s`, sol üst, güvenli alan içi), kamera kayarken belirir ve kaybolur; **oyuncu girdisini bloklamaz**. Kapalı kalabilir; sütun 1 için opsiyonel. Çok oyunculuda "Rakip: Oda 7" zaten HUD'da.
- Iris geçişine UI yazısı karışmasın; plaket iris açıldıktan sonra çıkar.
- Level 21'in "bitiş" odası zaten bir tabela (YOU COMPLETE THE DEMO, iki Gnome, yeşil vana). Bu, ana menüdeki başlık tabelasıyla aynı dil olduğu için (turuncu + pembe damla + Gnome) tutarlı bir kapanış; ama bu yazı şu an dünyada düz beyaz metin, panele alınmalı.

### 5.7 Ayarlar (v2, kısa)
Panel + satırlar (Ses, Müzik, Titreşim, CRT şiddeti?). Kaydırıcı = **yatay boru + vana kolu**: vana konumu değeri gösterir, sol/sağ stick ile 5 adım (adım adım, sürekli değil, Tiny5 ile sayı). CRT filtresi **kalıcı** (GDD §8), kapatma seçeneği bilinçli bir karar ister; öneri yalnızca şiddet (yumuşak/normal). v1 kapsamı dışı.

---

## 6) Hareket ve juice

Kural: menü animasyonları **<= 150 ms** (skills-digest <= 200 ms), aksiyon sırasında UI sessiz. DOTween (projede var) ya da UniTask; her yeni sıra için yeni `UIJuice` yardımcısı yazma, mevcut `UIEffects` (EasyTextEffects tabanlı olduğu tahmin ediliyor, doğrulanmadı) varsa önce onu kullan.

| Olay | Hareket | Süre |
|---|---|---|
| Odak değişimi | node imleci hedefe **adım** (lerp değil, 1-2 ara kare), çerçeve rengi anında | 60-80 ms |
| Seçim (A) | buton 1 piksel aşağı, node halkası 1 kare büyür, "tık" SFX | 80 ms |
| Panel açılışı | alttan 8 sanat piksel yukarı kayar (tam sayı adım), 3 kare | 100-150 ms |
| Panel kapanışı | tek kare kaybolur | <= 50 ms |
| Vana süsü | odak değişince 1/4 tur (4 sprite karesi) | 80 ms |
| Boru buharı | bağımsız, rastgele 2-3 kareli duman, yalnızca ana menüde | ~1 sn aralık |
| Ananas sayacı | belirir 100 ms, 1,5 sn kalır, çekilir 100 ms | |
| 3-2-1 | rakam ölçek vuruşu 1 kare | 100 ms |

- **Piksel kuralı:** tüm hareket **tam sanat pikseli adımıyla** (4 ekran px); alt piksel/ease'li akış CRT'li ekranda "titrer" ve pixel-art dilini bozar. DOTween `SetEase(Ease.Linear)` + `Snapping` (ya da kendi step easing) kullan.
- **Ses:** menü için 3 SFX (odak tıkı, seçim, geri); boru çınlaması gibi malzeme sesli (metal/ahşap tok ses). Ses lisansı kuralı (GDD §12: üçüncü taraf risk) geçerli: yalnızca kendi yapımı/telifsiz kullan.
- **Titreşim:** projedeki SO-per-system desenine uyarak `UIVibrationSettingsSO` (yeni) ya da hiç yok; menüde titreşim gereksiz, öneri **yok**.
- Dikkat: hitstop (`HitStopEvents`) menüye sızmasın; menüler `Time.unscaledDeltaTime` kullanmalı (pause'ta timeScale 0 olursa DOTween `SetUpdate(true)`).

---

## 7) Üretim planı

### 7.1 Boyutlar özeti
- **Referans çözünürlük:** 480x270 sanat pikseli, 4x -> 1920x1080 (alternatif 640x360 / 3x; o halde sanat metin boyutları 1,33x küçülür, Tiny5 em'i 8 px'in katı olmaz. 480x270 öneri).
- Sprite'lar 1x kaynak olarak çizilir; büyütülmüş PNG kaynak yapılmaz.

| Asset | Sanat boyutu | 9-slice border | Adet |
|---|---|---|---|
| Panel ana | 24x24 | 6 | 1 (+2 varyant) |
| Buton | 24x24 (esnek) | 6 | 4 durum |
| Boru parçaları (yatay/dikey/dirsek/T/vana) | 8x8, 16x16 | yok (tile) | ~8 |
| Node imleci | 16x16 | yok | 2 kare |
| Tuş ikonları | 12x12 | yok | ~12 |
| HUD ikonları (ananas, anahtar, bayrak, yıldız, X, eşit) | 16x16 | yok | ~8 |
| Tabela (logo zemini) | 64x24+ | 8 | 1 |
| Dither maske | 2x2 tile | yok | 1 |

Toplam ~45-55 sprite. Sayı sanatçı olmayan biri için gerçekçi sınırın üst tarafında; öncelik sırası §8'de.

### 7.2 Araçlar (dürüst değerlendirme)

| Araç | Pixel UI için | Not |
|---|---|---|
| **Aseprite** | **En iyisi.** Palet kilidi, katman, tag ile animasyon, slice (9-slice meta), tilemap modu, `.aseprite` dosyasını doğrudan Unity'ye alan importer var (Unity 6 için `com.unity.2d.aseprite`, doğrulanmadı: sürüm uyumu) | Ücretli (~20 USD, fiyat doğrulanmadı) ya da Steam. Kaynak koddan derlenebilir (lisans sınırlı, doğrulanmadı) |
| **LibreSprite** | Eski Aseprite'in GPL fork'u, ücretsiz. Temel işler iyi | Yenilikler eksik (ör. yeni slice/tilemap özellikleri); bakım yavaş (doğrulanmadı). Ücretsiz başlangıç için makul |
| **Piskel** | Web, ücretsiz, animasyon tamam | 9-slice/slice yok, palet yönetimi zayıf. Tek tek küçük ikon için olur, ana asset için yetmez |
| **Krita / GIMP** | Pixel modda çizim yapılır ama grid, palet, onion skin ergonomisi kötü | Aseprite alternatifi değil |
| **Figma** | Pixel art çizmek için **uygun değil** (vektör, anti-aliasing). **Yalnızca** ekran yerleşim taslağı için | Ham taslakta düzen kontrolü için ucuz, yoksa Unity'de beyaz kutu kurmak daha hızlı |
| **Unity içi** (Texture2D/eval ile prosedürel) | Geometrik (düz çerçeve, dama deseni, boru tile'ı, 9-slice gövdesi) Claude ile **script üreterek** yapılabilir; sanat karakteri yok | Placeholder ve test için mükemmel; final için yeterince karakterli olmaz |

**AI araçları (genel görüntü üreticileri: Midjourney, DALL-E vb.)**
- **Yaramaz:** "pixel art" diye üretilen görüntüler piksel ızgarasına oturmaz (sahte pikseller, tutarsız boyut, anti-aliasing, soluk ara tonlar), palet tutmaz, 9-slice'a uygun simetri vermez. Düzeltmesi sıfırdan çizmekten uzun.
- **Yarar (kısıtlı):** ilham/mood tahtası, "bu panelde ne olsun" fikri. Üretimde kullanılmaz.
- **Pixel'e özel AI servisleri** (SpriteCook ve benzerleri): skills-digest bu repoda kurulmamasını öneriyor (hesap/kredi, harici servis, Godot odaklı). UI kit ve 9-slice meta vaat ediyor; **kalite ve fiyat doğrulanmadı**, denemeden güvenme. Stil eşleşmesi (mevcut boru/node dili) yine elle zorlanır.
- **Claude ile üretim:** Python/Pillow ile piksel piksel sprite çizdirme (skill: pixel-art-studio) teknik olarak mümkün ama sanat hâli düşük; bu makinede Pillow şu an çalışmıyor (numpy sürüm çakışması, `import PIL` hata verdi), yani Unity `eval` ya da PowerShell `System.Drawing` yolu kullanılmalı. **Geometrik asset'ler (9-slice çerçeve, dither, boru tile'ı, ikon yerleşimi, atlas) için güvenilir; "karakterli çizim" için değil.**
- **Ağırlıklı gerçek iş:** oyunda **var olan asset'leri kırp-yeniden kullan** (boru sayfası, node halkası, ananas, pad, Gnome) ve yalnızca birkaç hero asset'i elle çiz.

### 7.3 Ham beyaz taslak mı, final mi?
**Önce beyaz taslak (greybox), sonra final. Kesin öneri.**
1. **Greybox:** düz renkli dikdörtgenlerle (palet rolleriyle), Tiny5 ile, doğru 9-slice sınırlarıyla **tüm ekranlar** Unity'de kurulur ve yalnızca gamepad ile gezilir. Yerleşim, odak akışı, metin boyutu CRT altında test edilir. Sprite = geometrik olarak üretilmiş (script) placeholder.
2. **Hero asset onayı:** panel + buton + node imleci elle çizilir, bir ekranda (ana menü) görülür, onaylanır.
3. **Aile üretimi:** geri kalan ikon/boru/varyantlar o stilden türetilir.
4. **Final polish:** juice ve ince ayar.
Sebep: sanatçı olmayan biri için en büyük risk, 50 sprite çizdikten sonra yerleşimin yanlış çıkması. Greybox bu riski sıfırlar; ve kod tarafı (Friday) sanat beklemeden ilerler.

### 7.4 Export ve Unity import
- Aseprite'te: tek `.aseprite` dosyası ailesi (`ui_panels.aseprite`, `ui_buttons.aseprite`, `ui_icons.aseprite`), paleti `rtg_ui.gpl` olarak kilitle (renk modu Indexed ya da RGB + palet kilidi), tag'lerle durumlar (`normal`, `focus`, `pressed`, `disabled`).
- Export: PNG-32, şeffaf arka plan, alpha 0/255, 1-2 px padding sprite sayfasında. Büyütme yok.
- İsimlendirme: `[type]_[object]_[variant]_[state]` (örn. `ui_button_wide_focus`, `ui_panel_main_9s`, `ui_icon_pineapple`).
- Unity import: Texture Type Sprite (2D and UI), Sprite Mode Single/Multiple, **PPU 16**, Filter **Point (no filter)**, Compression **None**, Mip Maps **kapalı**, Wrap Clamp, Mesh Type Full Rect (9-slice için; Tight ile kenarlar bozulur). 9-slice border'ı **Sprite Editor**'da kaynak pikseliyle (6/6/6/6) gir; `Image.type = Sliced`, `Fill Center`, `Pixels Per Unit Multiplier` ile ölçek (4x) tek yerden.
- Sprite Atlas: UI için tek atlas (Point, padding >= 4, Tight Packing kapalı, `unity:manage-sprite-atlas` ile). Doğrulanmadı: mevcut proje Sprite Atlas kullanıyor mu.
- **Canvas:** Screen Space Overlay + `CanvasScaler` 1920x1080 (mevcut). Pixel Perfect Camera + `upscaleRT` kullanılıyorsa UI metni bulanıklaşır (bilinen Unity sorunu); Overlay buna çözümdür. `upscaleRT` kullanıp kullanmadığı **doğrulanmadı**.
- **Aseprite -> Unity otomasyonu:** Unity 6'da `.aseprite` doğrudan import desteği var (`com.unity.2d.aseprite`); paket sürümü ve projede kurulu olup olmadığı doğrulanmadı. Yoksa PNG export + slice meta elle girilir.
- **Doğrulama (her hero asset sonrası):** 1920x1080, CRT açık, gerçek arka plan üstünde, yalnızca gamepad; nearest-neighbor contact sheet'te aile tutarlılığı. `unity command capture_game_view --source screen` (Play Mode, HUD için).

### 7.5 Asset manifest
Bir tablo (ad, boyut, pivot, 9-slice border, kaynak, lisans, onay). Üçüncü taraf/AI kaynaklı hiçbir şey olmayacak; boru ve node zaten projenin kendi sanatı. Fontlar: Tiny5 lisansı (OFL olduğu bilinir) doğrulanmadı, **yayın öncesi kontrol et**; Kenney fontları CC0 olduğu bilinir (doğrulanmadı).

---

## 8) İş yükü tahmini

Çalışma ritmi: Pazartesi (`#art`, `#test`, en fazla 2 `#level`) ve Cuma (`#code`, `#level`), bir akşama en fazla 3 task, her task 1-1,5 saat. **Sanat süresi sanatçı olmayan biri için tahminin 1,5-2 katı uzayabilir**; tahmin buna göre sıkıştırılmadı, aralık verildi.

| Akşam | Gün | Task'lar |
|---|---|---|
| 1 | Pzt `#art` | (a) rtg_ui paleti (.gpl) + Tiny5 native boyut/Türkçe karakter testi (b) mevcut boru/node/ananas sprite'larını UI sayfasına kırp (c) greybox sprite seti: script ile üretilmiş 9-slice panel + buton |
| 2 | Cuma `#code` | (a) `UIStyleSO` (b) ekran stack + odak yönetimi yardımcısı (SetSelected, wrap, geri) (c) CanvasScaler/Overlay-CRT kararı için test sahnesi |
| 3 | Pzt `#art` | Hero asset: panel (elle çizim, 24x24, 9-slice) + tabela |
| 4 | Cuma `#code` | Ana menüyü greybox stil ile yeniden kur (`MainMenuBuilder` stil ataması), gamepad turu |
| 5 | Pzt `#art` | Hero asset: buton 4 durum + node imleci 2 kare |
| 6 | Cuma `#code` | Odak göstergesi + buton durumları sprite swap, ana menü final bakışı |
| 7 | Pzt `#art` | Gamepad ikon seti (A/B/X/Y/LT/RT/LB/RB/LS, ~12 ikon) |
| 8 | Cuma `#code` | Pause menüsü (stack ile) + alt şerit ikon yardımı |
| 9 | Pzt `#art` | HUD ikonları: ananas, anahtar yuvası, bayrak, dash/sling ışığı (opsiyonel) |
| 10 | Cuma `#code` | HUD (olay güdümlü ananas/anahtar), MP HUD restyle |
| 11 | Pzt `#art` | Sonuç ekranı sprite'ları (yıldız, X, eşit, Gnome pozisyonları), boru parçaları |
| 12 | Cuma `#code` | MP sonuç ekranı + oda plaketi |
| 13 | Pzt `#art`/`#test` | Juice sprite'ları (vana kareleri, buhar) + 1080p/CRT/gamepad turu |
| 14 | Cuma `#code` | Juice (DOTween adım), SFX bağlama, fine-tune |

- **Tahmin: 14 akşam = 7 hafta** (aralık 12-18 akşam). Ayarlar menüsü (§5.7), ikinci ikon seti ve kanca çizgisi stretch'i **dahil değil** (+2-4 akşam).
- Bu 7 haftanın tamamı UI'ya gider; level üretimi (38 oda kaldı, GDD §12) paralel yürümez. **Bu belgenin en önemli maliyeti.** Daraltılmış sürüm: yalnızca 1-10. akşamlar (ana menü + pause + HUD) = 5 hafta.
- Kural 5 gereği her akşam sonunda tek mantıksal commit önerilir; TASKS.md'ye eklemeden önce kullanıcıya sorulur, takvim güncellemesi de yapılır (CLAUDE.md "Takvim senkronizasyonu"). Bu belge TASKS.md'ye dokunmadı.

---

## 9) Riskler

1. **CRT ile Overlay çakışması (yüksek).** Menüler dünya malzemesinden doğmuş gibi görünmek istiyor ama Overlay canvas'ın CRT'den muaf olması muhtemel (doğrulanmadı): UI "temiz", dünya "tüplü TV" görünür ve diegetic fikir çöker. Çözüm seçenekleri: (a) Screen Space - Camera canvas (CRT'ye girer, ama bulanıklık/`upscaleRT` riski, 3D) (b) CRT shader'ına UI'ı dahil etme (renderer feature kapsamı) (c) kabul et: UI CRT dışı, menü "ekran üstü OSD" gibi algılansın. **İlk akşamlarda test sahnesiyle karar ver** (Akşam 2, 3. task).
2. **Metin okunurluğu.** Turuncu doku üstünde metin = düşük kontrast. Önlem: metin her zaman mürekkep yuvasında. Küçük (8 px) Tiny5 + CRT = yumuşak; 26 px eşiği (gamepad/TV) 32 px ile sağlanıyor, CRT altında ölç.
3. **Sanat işi sanatçı olmayan birine ağır.** 45-55 sprite; ilk onayda stil tutmazsa tüm aile yeniden. Önlem: greybox önce, hero asset onayı sonra, mevcut asset'leri kırp.
4. **Kapsam kayması: dekor.** Boru süsü, buhar, vana animasyonu çekicidir ve bitmez. Önlem: ekran başına en fazla 2-3 boru parçası, animasyon bütçesi §6 tablosu.
5. **Dünya değişimi.** İleride asit dışı dünyalar (GDD §8) gelirse "dünya malzemesi" UI'ı her dünyada yeniden çizim demek olabilir. Önlem: `UIStyleSO` + paleti kaynaktan ayıran sprite tasarımı; ya da UI'ı "ana karakterin dünyası" (menü hep aynı) olarak sabit tut. Karar bekliyor.
6. **Gnome çözünürlük çatışması.** 128x128 scan illüstrasyon pixel UI'ın yanında; GDD bunu bilerek kabul ediyor, ama ekranda 1,5x/2x gibi fraksiyonlu ölçek bulanık görünür. Tam sayı ölçek ya da küçültmeyi Point yerine bilinçli seç.
7. **Gamepad ikonları ve platform.** A/✕ sorunu (GDD §5.1). Tek nötr set seçilirse oyuncuya yabancı gelebilir. Karar bekliyor.
8. **Beyaz placeholder'lar dünyada.** Moodboard'da düz beyaz dikdörtgen/daire (oda 2, 9, 12, 16, 19, 22) UI paletiyle çelişir; "beyaz = placeholder" kuralı UI'a bulaşmasın (UI'da saf beyaz yok, #F5EFCB krem).
9. **Doğrulanmayanlar:** Tiny5 native em ve tam Türkçe karakter seti; Overlay-CRT davranışı; PixelPerfectCamera kullanımı; `plums.png` kare boyutu; Aseprite paketi uyumu; Sprite Atlas varlığı; SpriteCook kalite/fiyat; Aseprite/LibreSprite fiyat/bakım durumu; palet pipet değerleri; MP'de pause davranışı.
10. **Diğer yönlerle kıyas notu:** bu yön karakter ve tutarlılıkta güçlü, tasarım/okunurluk ve iş yükünde zayıf. Yalın bir yön (düz mürekkep panel + krem metin + tek node imleci) bu yönün **%60'ını** daha az sanatla verir; B'yi seçersen bile greybox aşamasında en yalın hâlden başla ve dünya malzemesini (boru, pembe şerit) katman katman ekle.

---

## Özet (5 satır)
1. Yön B: UI turuncu karton/toprak levha + pembe damla şerit + boru/vana süsünden kurulur, odak imleci mini kanca node'u; metin her zaman koyu mürekkep yuvada (`#433646` üstünde krem).
2. 16 renk palet moodboard'dan çıkarıldı (CRT'li örnekler, pipetle doğrulanmalı); tek font Tiny5, 480x270 grid x4, ~45-55 sprite, 9-slice border 6 px.
3. Ekranlar: ana menü, pause, olay güdümlü minimal HUD (ananas/anahtar belirip çekilir, dash/sling için ek gösterge yok), MP sonuç (ikon+metin+iki Gnome), oda plaketi.
4. Üretim: önce script üretimli greybox, sonra elle çizilen hero asset (Aseprite); genel AI görüntü araçları pixel UI için yaramaz, mevcut boru/node asset'lerini kırp-yeniden kullan.
5. Tahmin ~14 akşam (7 hafta, aralık 12-18); en büyük riskler: Overlay canvas'ın CRT'ye girmemesi (diegetic fikri bozar), sanat yükü, 7 haftalık level duraklaması.
