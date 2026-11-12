# UI art direction: supervisor değerlendirmesi

Tarih: 2026-11-08. Girdi: `skills-digest.md`, yönler A/B/C/D, `docs/GDD.md` (§1-§5, §7-§8, §11-§12), moodboard 01, 05, 09, 16, 21 (Read ile görüldü), kod ve asset dosyaları. TASKS.md, kod ve takvime dokunulmadı; bu dosya tek çıktı.

Kısaltmalar: **[DOĞRULANDI]** kodu/asset'i ben okudum. **[KISMEN]** kanıt var ama oyunda görülmedi. **[SORU]** insan karar/teyit vermeli.

---

## 0. En önemli bulgu: ajanların hiçbiri CRT'nin 480x360 olduğunu fark etmedi

Dört rapor "480x270 sanat gridi, 4x = 1080p" üzerine kurulu. CRT ise bununla çakışıyor:

- `Assets/Scenes/NewScene/Global Volume Profile.asset`: `pixelResolution = 480x360`, `screenBend = 10`, `blur = 0.5`, `scanlines = 0.1`, `aperture = 0.05`, `vignetteSize = 1.676`.
- `Assets/Shader/CRTFilter.shader` satır ~162-165: UV'yi `floor(uv * _PixelResolution) / _PixelResolution` ile 480x360 hücreye **yapıştırıyor**. 16:9 ekranda hücre 4 px geniş x 3 px yüksek, yani **kare olmayan piksel**.
- Sonuç: CRT'den geçen her şey (dünya, dünya içi yazılar) dikeyde 3 px'lik hücrelere örnekleniyor. Moodboard'daki "Move Right Arm" yazısının testere dişli, eşitsiz görünmesinin nedeni bu (level_05'te net görülüyor). 4 ekran px'lik font pikseli 3 px'lik hücreye oturmuyor.
- Ek: `DefaultVolumeProfile.asset` içinde aynı volume `320x240` ile ayrı bir değer taşıyor; sahneye göre CRT çözünürlüğü değişiyor.

Sonuç: **CRT'den geçecek UI'da "tam sayı 4x piksel" garantisi yoktur.** Bu yüzden yeni UI'ın CRT'nin dışında (Overlay) durması teknik zorunluluğa yakın. Aşağıda detay (§2).

---

## 1. Yönlerin puanlaması (1-5, 5 = en iyi; "Risk" sütununda 5 = düşük risk)

| Ölçüt | A CRT Arcade | B Dünya Malzemesi | C Minimal | D Setlist |
|---|---|---|---|---|
| Mevcut görsel dile uyum | **4** | **5** | **3** | **3** |
| Tasarım sütunlarına uyum (hız/akış, hızlı respawn) | **3** | **4** | **5** | **3** |
| Gamepad okunabilirliği | **4** | **3** | **4** | **4** |
| Tek geliştirici iş yükü gerçekçiliği | **3** | **2** | **5** | **3** |
| Risk (düşük = 5) | **3** | **2** | **4** | **3** |
| **Toplam /25** | **17** | **16** | **21** | **16** |

### Gerekçeler

**A: CRT Arcade (17)**
- Uyum 4: Pembe damla şeridi, gökkuşağı halka, krem-lacivert palet moodboard'la birebir akraba. Ama "arcade" teması dünyada yok (dünya şekerli/boru dolu, arcade değil), yani dil yeni bir katman ekliyor.
- Sütun 3: Menü açılışında CRT çizgisi, aberasyon, harf harf yazılan başlık, sürekli ses/juice; oyuncuyu "kesen" bir şey değil ama arcade enerjisi oyunun akışıyla yarışıyor. HUD kısmı iyi (ölümde UI yok, sayaç sönükleşiyor).
- Gamepad 4: Kontrastlı koyu panel + harfli ikon (Xbox renkleri paletin dışına çıkıyor), 3 katmanlı odak (renk + halka + `>`) güçlü.
- İş 3: 12 akşam, 15 task; ~6 art akşamı. Başlık logosu, halka imleç, ikon seti elle çizim. PS/Xbox çift set de konuşuluyor.
- Risk 3: "CRT altında okunabilirlik" riskini kendisi UI'ı Overlay'e alarak çözüyor (doğru); asıl risk arcade-ses-juice kapsam kayması.

**B: Dünya Malzemesi (16)**
- Uyum 5: Moodboard'daki üç ana malzemeyi (turuncu zemin, pembe damla, koyu boru) doğrudan UI'a taşıyor; en "bu oyunun menüsü" duran yön. level_01/21 ile en tutarlı.
- Sütun 4: Metin her zaman mürekkep yuvada, HUD olay güdümlü, ölümde UI yok, açılış <=150 ms. Ama boru/vana/buhar süsü kapsam kaymasına açık ve "ağırlıklı" his akışla biraz çelişiyor. Kendi içinde "ananas sayacı belirir-çekilir" önerisi mevcut kalıcı sayacı değiştiriyor (bkz. §3).
- Gamepad 3: Dokulu turuncu çerçeve + 8 px metin; metin yuvası mürekkep olduğu için okunur, ama çerçeve sprite'ları küçük metinle yarışır; ikon seti 12x12 (B kendisi "sanatçı değil" riskini kabul ediyor).
- İş 2: **45-55 sprite**, 14 akşam (aralık 12-18), tamamı UI'a gider, **7 hafta level üretimi durur** (GDD §12'de 38 oda kaldı). B'nin kendi uyarısı: "bu belgenin en önemli maliyeti". Gerçekçilik açısından en kırılgan plan.
- Risk 2: Dünya değişince (GDD §8 farklı temalı dünyalar) "dünya malzemesi UI" her dünyada yeniden çizim demek; Gnome 128 px illüstrasyon + pixel UI çatışması; ağır sanat yükü.

**C: Minimal ve Oyun-Önce (21)**
- Uyum 3: Moodboard'daki "tutorial yazısı" estetiğini (krem piksel yazı + gökkuşağı halka) taşıyor ve oyunun en baskın öğesini (node) menüde odak imleci yapıyor; ama şeker/pembe kimliği UI'ya geçirmiyor (C kendisi "yavan görünme riski" diyor). Kimlik menüde iki dev halka + başlıkla taşınıyor.
- Sütun 5: "Hiçbir şey sürekli görünmez", ölümde UI yok, buton kutusu yok, pause/sonuç dışında panel yok. Hız/akış ve "hızlı respawn" sütunlarına en az sürtünme.
- Gamepad 4: Kutusuz metin + halka + 1 piksel kayma (renk-şekil-konum üçlü); elmas tuş ikonları platform bağımsız (GDD "A / ✕" sorununu çözüyor). Dezavantaj: kutusuz menü, parlak arka planda (gökyüzü mavisi) krem metin kontrastı ink gölgeye bağımlı, test edilmeli.
- İş 5: ~25 küçük sprite (panel 16x16, halka 13x13, 8 ikon, pip). Sprite'sız "aşama 1" ile hemen çalışır; en çok iş kod/akışta, tam da Cuma'ya düşen iş.
- Risk 4: Ana risk "çok az bilgi = kaçırılan bilgi" ve kimlik. Teknik riskler küçük ve iyi tanımlı.

**D: Setlist (16)**
- Uyum 3: Fikir zekice (gökkuşağı halka = plak etiketi, cüce = grup üyesi, "Rock The Game" adıyla örtüşüyor) ama dünyada kaset/afiş/bilet yok; yeni bir metafor katmanı ekliyor. Menü dili İngilizce'ye kayıyor (oyun içi öğreticiler İngilizce, mevcut menü Türkçe).
- Sütun 3: Ölümde "REW" ikonu 0.3 sn (GDD §5.3 "anında, UI yok" ile hafif çelişiyor ama kesmiyor), "TRK 05" etiketi iyi. Pause sistemi sıfırdan gerekiyor (diğer yönler de gerektiriyor ama D'nin tasarımı buna daha bağımlı).
- Gamepad 4: Bilet paneli + setlist satırı + plak imleci net. Nötr tuş glifleri iyi.
- İş 3: 9-11 akşam, ~25 sprite, ama **bitmap logo + cüce animasyonu + 8 kareli dönen plak halkaları** sanatçı olmayan biri için en belirsiz kalemler. 24 renk palet gereksiz geniş.
- Risk 3: Metafor yorgunluğu, cüce sprite'ının UI'da yeniden kullanılabilirliği (boyut/palet), "marka" sorunu yok. D en çok "fikir" üretti: tek ödünç alınabilir somut parça: sonuç ekranında iki cüce.

---

## 2. Doğrulama: ortak "doğrulanmadı" listesi

### 2.1 Doğrulananlar

| # | İddia / soru | Sonuç | Kanıt |
|---|---|---|---|
| 1 | **CRT efekti Overlay canvas UI'ı etkiliyor mu?** | **Hayır [DOĞRULANDI: kod + URP davranışı].** CRT `CrtRendererFeature` (RenderPassEvent.AfterRenderingPostProcessing) ile kamera hattına enjekte; Screen Space - Overlay canvas kamera hattının dışında çiziliyor. Sahnelere göre durum: **MainMenu, MultiplayerLobby ve `RaceHud` Overlay** (CRT yok); **NewScene'deki `Canvas` (Coin UI, IrisTransition, DialogueBubbleUI) Screen Space - Camera** (CRT içinde), `StaminaCanvas` World Space (CRT içinde). Ekranda görsel teyit yapılmadı. | `CrtRendererFeature.cs`; `NewScene.unity` satır 2188 (`m_RenderMode: 1`, kamera 963359375), 4145 (`m_RenderMode: 2`); `MainMenuBuilder.cs:56`, `RaceHud.cs:40`, `MultiplayerLobbyBuilder.cs:56`; `MainMenu.unity:805`, `MultiplayerLobby.unity:515` (`m_RenderMode: 0`). Renderer asset'inde `CrtRendererFeature` aktif. |
| 2 | **Tiny5 native piksel boyutu** | **Em = 8 piksel [DOĞRULANDI].** `m_UnitsPerEM 1024`, `m_CapLine 56` @ `m_PointSize 90` => cap yüksekliği ~637 birim = 5 x 128 birim; 128 = em/8. Yani **TMP font size 32 = 4 ekran px / font pikseli**, 64 = 8x, 128 = 16x. A/B/C/D'nin varsayımı doğru. Ölçek 4x'in katları (32, 64, 96, 128) kullanılırsa piksel tam oturur. | `Tiny5-Regular SDF.asset`: `m_FaceInfo` |
| 3 | **Türkçe glif desteği** | **Var [DOĞRULANDI].** TTF `cmap` tablosu elle ayrıştırıldı: ç Ç ğ Ğ ı İ ö Ö ş Ş ü Ü â î û ve ASCII 32-126 hepsi mevcut (1155 kod noktası). Google Fonts da Latin Plus kapsamı veriyor. **Lisans OFL 1.1** (Stefan Schmidt; GitHub + Google Fonts + Fontsource). | `Assets/UI/Fonts/Tiny5-Regular.ttf` (131 KB). Kaynaklar: bkz. §8 |
| 4 | **Tiny5 SDF atlas pixel font için uygun mu?** | **Hayır, değiştirilmeli [DOĞRULANDI].** Asset **Dynamic** (`m_AtlasPopulationMode: 1`, glyph/character tablosu boş, çalışırken doluyor; git status'ta değişmiş görünmesinin nedeni bu), render modu **SDF (4165)**, padding 9, 1024x1024. SDF kenarları yumuşatır; pixel font için **Raster** (hinted) + Point filtre gerekir. Dynamic atlas build'de `m_ClearDynamicDataOnBuild: 1` ile temizleniyor. Yeni Raster asset Static populate edilebilir (Latin + Latin Extended-A). | `Tiny5-Regular SDF.asset` satır 168-230 |
| 5 | **Pause sistemi var mı?** | **Yok [DOĞRULANDI].** `Assets/Scripts` içinde `timeScale`, `PauseManager`, `IsPaused` araması yalnızca `PlayerController.IsPaused` ve `OnPauseStarted/Ended` döndürdü; bunlar **hit-stop** (`HitStopEvents`), oyun pause'u değil. Yani pause sıfırdan: `Time.timeScale`, Start girdisi, unscaled tween, ekran stack'i. **Tuzak:** `IsPaused` ismi yanıltıcı; `PenaltyHandler` onu "oyuncu meşgul" kontrolünde kullanıyor, pause menüsüyle karıştırma. | `PlayerController.cs:107,251-258`; GDD §11 "Ana menü, pause, ayarlar, bitiş: Yok" (ana menü artık var, commit 4641934) |
| 6 | **"Tek oyunculu HUD yok" (D)** | **Yanlış [DOĞRULANDI].** Mevcut bir **Coin UI** var (`CollectibleUIController`: `CollectibleInventorySO.OnCoinsChanged` aboneliği, "00" biçimi, DOTween punch, `SetUpdate(true)`); NewScene'de `Canvas` (Screen Space - Camera, sort 5) altında. Yani kalıcı bir ananas/altın sayacı zaten var; B ve C'nin "olay olunca belirir, çekilir" önerisi mevcut davranışı **değiştirmek** demek, sıfırdan yapmak değil. | `CollectibleUIController.cs`; `NewScene.unity` Canvas çocukları |
| 7 | **Wall stamina göstergesi yok (C: "12x2 çubuk ekle")** | **Zaten bir `StaminaCanvas` (World Space) var [KISMEN].** İçeriği/çizimi okunmadı; C'nin yeni çubuk önerisi çakışıyor olabilir. `PlayerController.OnStaminaWarning` olayı da mevcut. Eklemeden önce mevcut görseli gör. | `NewScene.unity` GameObject `StaminaCanvas`; `PlayerController.cs:112` |
| 8 | **Pixel Perfect Camera kullanılıyor mu?** | **Hayır [DOĞRULANDI].** Sahne/prefab'larda `PixelPerfectCamera` bileşeni geçmiyor (`m_PixelPerfect: 0` Canvas özelliği, kamera değil). Dünya sprite'ları PPU 16 (`plums.png` meta), Point filtre. Dünya pikseli ekran pikseline tam oturmuyor (kamera ortho boyutuna bağlı), CRT ile ikinci bir yeniden örnekleme daha eklenmiş. | Grep sonucu; `plums.png.meta` |
| 9 | **Pillow Python ile çalışıyor mu? (B "çalışmıyor", C/A "çalışabilir")** | **B haklı [DOĞRULANDI].** Bu makinede (Anaconda Python 3.8, numpy 1.20.1, Pillow 10.4) `import PIL.Image` `numpy.typing.NDArray` hatasıyla kırılıyor. `System.Drawing` (PowerShell) ve Unity `eval` çalışıyor; piksel örneklemesini onunla yaptım. Python yolu istenirse temiz venv gerekir. | Ben çalıştırdım |
| 10 | **Aseprite fiyatı** | **~19.99 USD [DOĞRULANDI],** tek seferlik; itch.io alımı Steam anahtarıyla geliyor; en düşük kayıtlı Steam fiyatı 9.99. | Kaynaklar §8 |
| 11 | **Palet hex'leri** | **Dört raporun hex'i de birbirinden ve gerçekten saptı [DOĞRULANDI].** Bkz. §3.3. | `level_01.png` piksel örnekleri |
| 12 | **Tek tema/bayrak/oda numarası "ODA 07" bilgisi var mı?** | Odaların `Room` nesneleri var; numaralandırma kaynağı okunmadı. [SORU, bkz. 2.3] | |
| 13 | **Aseprite importer paketi (`com.unity.2d.aseprite`) kurulu mu?** | **Kurulu görünmüyor [KISMEN].** `Packages/manifest.json`'da `com.unity.2d.*` satırı yok (sadece inputsystem 1.18.0, ugui 2.0.0 vb. görüldü). PNG export zaten yeterli; gerek yok. | `manifest.json` |

### 2.2 Doğrulama sonucunun yönlere etkisi

- **Overlay = CRT dışı**: tüm yönler menü/pause/MP HUD/sonuç için Overlay + 4x grid kullanabilir. Bu doğru ve düşük riskli.
- **Oyun içi HUD (Coin UI) şu an CRT içinde ve 4x3 hücreli**: yeni HUD'ı Overlay'e taşımak gerekir, aksi halde 4x4 piksel garantisi yok. Bedeli: HUD "OSD gibi keskin", dünya "eğri tüplü TV" görünür. Bu bilinçli bir tercih olarak kabul edilmeli (GDD §8 "CRT kalıcı" ile çelişmez, çünkü CRT dünyayı kaplıyor; UI cihazın OSD'si gibi okunur). [SORU #5]
- **Font ölçeği**: yalnızca 32/64/96/128 (4x katları). Gövde 32 px, konsol eşiğinin (>=26) üstünde.
- **Dünya içi öğretici yazılar** (moodboard'da jagged) CRT içinde kalıyor; bunları UI sistemiyle birleştirme "Overlay'e taşı" demek olur ya da kabul edilir. Kapsam dışı bırak.

### 2.3 Doğrulanamayanlar: insana sorulacak sorular

1. **CRT, Overlay'i ekranda gerçekten etkilemiyor mu?** Kod ve URP mimarisi "hayır" diyor ama Play Mode'da `capture_game_view --source screen` ile bir ana menü + RaceHud karesine bakılmadı. (2 dk'lık teyit.)
2. **Dünya yazıları ve Iris/Dialogue UI'ın CRT içinde kalması kabul mü?** `IrisTransition` ve `DialogueBubbleUI` Camera canvas; Overlay'e çekmek davranışı değiştirir (iris tüm ekranı kapatmalı, Overlay iris'i CRT'nin üstüne çizerdi).
3. **Coin UI ve StaminaCanvas'ın şu anki görünümü** (kalıcı mı, ne kadar süre, hangi renk) oyun içinde bakılmadı; tasarım kararları (kalıcı sayaç mı, olay güdümlü mü) buna bağlı. Playtest karesi gerekir.
4. **Dash/slingshot "tükendi" karakter rengi şu an yeterince okunuyor mu?** Üç yön ek ikon önerdi, biri (B, D) yok dedi. Bu "game feel" sınıfı (CLAUDE.md kural 4), insan playtest etmeli; ben önermiyorum.
5. **MP'de ananas sonuç ekranında var mı, ölüm sayısı ağda taşınıyor mu** (A ve D'nin sonuç ekranı satırları bunlara dayanıyor). `multiplayer.md` bu soruyu kesin kapatmadı; `RaceSession` verisine bakılmadı. Çıkar: yalnızca süre + bayrak ile başla.
6. **Aseprite alınır mı, LibreSprite yeter mi?** Fiyat doğrulandı ama LibreSprite'ın güncelliği doğrulanamadı (arama sonuç vermedi).
7. **Hedef çözünürlükler:** Steam Deck (1280x800 = 2.96x) ve 1440p (5.33x) desteklenecek mi? Tam sayı ölçek stratejisi buna bağlı (§7.2).
8. **Gamepad ikon seti:** Xbox harfleri (A) / nötr elmas (C) / PS+Xbox çift set? GDD §5.1 "A / ✕" der; karar sende.
9. **Moodboard'daki beyaz dikdörtgen/daireler placeholder mı?** (GDD §8.1 da soruyor.) B'nin "UI'da saf beyaz yok" kuralı bunun cevabından bağımsız.
10. **Kenney fontları (12 adet) projede kalacak mı?** MainMenuBuilder `MultiplayerUISettingsSO.Font`'u kullanıyor; hangi font atanmış okunmadı. (Tiny5 tek font önerisi için `.asset` içine bak.)

---

## 3. Raporlar arası çelişkiler ve nedenleri

### 3.1 İş yükü: 7 ile 14 akşam arası

| Yön | Rapor tahmini | Yaklaşık sprite | Neden fark var |
|---|---|---|---|
| C | 7 akşam (gerçekçi 9-10) | ~25 küçük | Task'ları akşama 3'er sıkıştırıyor; kutusuz menü, panel yok, sprite'sız aşama 1 |
| D | 9 akşam (9-11) | ~25 + 2 animasyon | Pause/ayarlar sistemini "dışarıda" bırakıyor; bitmap logo/cüce kalemlerini iyimser sayıyor |
| A | 12 akşam (14-15 tamponlu) | ~30 | Logo + halka + 2 ikon seti + SFX bağlama + oda bandı kalemleri ayrı akşam |
| B | 14 akşam (12-18) | **45-55** | Boru kiti, 3 panel, 3 buton boyutu, vana/buhar juice, Gnome, ikon seti; ayarlar menüsü ve ikinci ikon seti hâlâ dışarıda (+2-4) |

Çelişkinin kaynakları:
1. **Kapsam farkı**: ekran sayısı aynı (ana menü, pause, HUD, MP sonuç, oda geçişi) ama sprite sayısı 25'ten 55'e çıkıyor. Akşam sayısı büyük ölçüde sprite sayısıyla orantılı.
2. **Pazartesi/Cuma kuralı kimse tarafından modellenmedi**: Pazartesi `#art`/`#test`, Cuma `#code`. Bir art akşamını bir kod akşamı izlemek zorunda; toplam akşam = 2 x max(art akşamları, kod akşamları). Art ağırlıklı planlar (A, B) Pazartesi'de tıkanır. C bunu kod ağırlıklı yaparak kaçıyor, D dengeli.
3. **Sanatçı olmayan kişi çarpanı**: A "+%50", B "1.5-2x" dedi, C ve D tahmine katmadı. Tahmin edilen akşam sayılarını bu çarpanla normalize ettim (aşağıda).
4. **"Akşam başına 3 task" iyimserliği**: C'nin 14 task'ı 7 akşama paketlenmiş, ama bir akşamda 3 task 1-1.5 saat kuralı gerçekçi değil; ben akşamı 2-3 task olarak saydım.

Normalize edilmiş (ben, +%50 sanat çarpanı, her ekran sadece **menü + pause + MP sonuç + HUD**, ayarlar/ikinci ikon seti hariç, Pazartesi/Cuma kuralıyla):

| Yön | Normalize akşam | Not |
|---|---|---|
| C | 7-9 | Dış kaynaklı risk: Pause sistemi (yeni), Raster atlas |
| D | 10-12 | Cüce animasyonu, logo, pause bağımlılığı |
| A | 12-14 | |
| B | 14-18 | 7-9 hafta level durması |

### 3.2 Hangi araç işe yarar: çelişkili iddialar

| Araç | Rapor iddiaları | Gerçek durum |
|---|---|---|
| **Python/Pillow ile sprite üretimi** | C: "gerçekten işe yarar (ASCII->PNG)", A: "umut verici, denenmedi", B: "bu makinede kırık", D: "çizim için hayır" | **Pillow bu makinede çalışmıyor** (§2.1 #9); geometrik asset üretimi için **Unity `eval` ya da PowerShell `System.Drawing`** kullanılabilir; ben pikselleri o yolla okudum. Hepsi "karakterli çizim değil, geometrik placeholder için" konusunda hemfikir; gerçek fark makine durumu |
| **Figma** | Dördü de "pixel çizmek için hayır, yalnızca yerleşim" | Tutarlı. Figma MCP bu oturumda yetkilendirilmemiş; ASCII wireframe + Unity greybox yeter |
| **Genel AI görsel (Midjourney/DALL-E)** | Dördü de "UI sprite'ı için yaramaz, konsept için olur" | Tutarlı ve doğru (bkz. §8 rehber) |
| **Pixel'e özel AI (SpriteCook, Retro Diffusion, PixelLab)** | A: önermem, B: "denemeden güvenme", C: önermem, D: "sınırlı fayda" | Hiçbiri denenmedi, fiyat/kalite doğrulanmadı. Digest SpriteCook'u "kurma" diyor (hesap/kredi, Godot odaklı). Kararım aynı: kullanma |
| **Aseprite** | Dördü de ana araç; A/B/C "~20 USD" | Doğrulandı: 19.99 USD |
| **LibreSprite/Piskel** | Yedek | LibreSprite güncelliği doğrulanamadı (§2.3 #6) |
| **Kodla üretim (Claude)** | B ve C "placeholder/greybox için mükemmel", D "layout/prefab için evet" | Doğru; en çok değer kod tarafında (builder, atlas, prefab) |

### 3.3 Hex değerleri: neden birbirini tutmuyor

Tüm ajanlar renkleri **CRT'li moodboard ekran görüntülerinden gözle** çıkardı (A/C/D açıkça "tahmini" dedi, B kısmen pikselden okudu ama CRT'li). CRT: tarama çizgisi ±%10-15 parlaklık modülasyonu, `blur 0.5`, vinyet, 480x360 örnekleme. Aynı rengin tek pikseli bile çizgiye göre değişiyor. Ben `level_01.png`'den PowerShell ile birkaç piksel okudum (tek piksel, tarama çizgisine bağlı, kesin değil):

| Rol | Ölçülen (CRT'li) | A | B | C | D |
|---|---|---|---|---|---|
| Gökyüzü | `#84D2F6`-`#92DFFF` | `#6FB8EC` (çok koyu) | `#86D4F7` | `#7CC8F2` | `#7EC4EE` |
| Boru | `#433646` | `#2A2E55` (mavi, hatalı) | `#433646` (tam) | `#2D2650` | `#2B2A4C` |
| Zemin turuncu | `#FDAA66` | `#EE9F5E` | `#F2A465` | `#F09A5C` | `#E8A06A` |
| Halka kırmızı | `#FF4300` | `#F2481C` | `#FF3A00` | `#FF4B2B` | `#F2511C` |
| Halka teal | `#259EA0` | `#2A8EA0` | `#176D83` | `#2A8CA0` | `#2A9A9A` |
| Pembe | scan çizgisine göre `#FF88D0`-`#AD2485` | `#F25CB4` | `#EE3FA8` | `#F25AA3` | `#F05AAA` |

Sonuç: Boru koyusu aslında **mor-gri (`#433646`)**, A'nın lacivert paneli (`#2A2E55`) oyunun boru rengi değil; B bunu yakalamış. Kimsenin değerine güvenme; **kaynak sprite PNG'lerinden (CRT'siz) örnekle**. Palet task'ının ilk işi bu (A/B/C/D hepsi bunu zaten öneriyor).

### 3.4 Diğer çelişkiler

| Konu | A | B | C | D | Karar / öneri |
|---|---|---|---|---|---|
| Tuş ikonları | Xbox harf, renkli | nötr yuvarlak harf | nötr elmas, harfsiz | nötr şekil | Palet dışı renk getirdiği için A elenir; **nötr elmas (C)** en az sürtünmeli. Karar senin (GDD "A / ✕") |
| Dash/slingshot HUD | köşede sürekli | yok (opsiyonel tükenince) | karakter yanında tükenince | yok | Mevcut karakter rengi ana kanal. **v1'de HUD'a ekleme**; playtest sonrası. (feel = senin kararın) |
| Ölüm | UI yok, sayaç opsiyonel | UI yok, sayaç yok | UI yok | REW ikonu 0.3 sn | GDD §5.3 + sütun 3 => **UI yok** |
| Oda geçişi | "ODA 07" 0.8 sn sol alt | plaket opsiyonel | UI yok | "TRK NN" 2 sn | v1'de **yok**; ilerleme hissi gerekirse sonra |
| Ananas sayacı | sönükleşir | belirir-çekilir (kalıcı yok) | olayda belirir + bekleyen/kalıcı ayrımı | olayda 2 sn | Mevcut **kalıcı** Coin UI var (§2.1 #6). Kalıcı kalsın, sadece Overlay + panel diline taşı; "belirir-çekilir" bir **GDD §12.4** kararı (toplanabilirlerin amacı belirsiz) |
| Dil | TR (BÜYÜK HARF) | TR menü + EN öğretici | TR örnek, tek dil iste | **EN** (afiş dili) | Tiny5 TR glifleri var (§2.1 #3); karar senin. `ToUpper()` yerine TMP `<allcaps>` (İ/I sorunu, A'nın uyarısı doğru) |
| Dithering/karartma | dither | dither | dither | %50 + dither | Dördü de dither: pause arkasında **2x2 dither**, alpha yok. Ancak Overlay'de alpha bu kural için sorun değil (CRT yok); sadece okunabilirlik tercihi |
| Palet boyutu | 14 | 16 | 14 | 24 | 14-16 yeter; D'nin 24 renk fazla |
| Ölçek | 4x, fraksiyonda kesirli | 4x + `Constant Pixel Size` seçeneği | 4x, letterbox | 4x | Hepsi 480x270; **CRT 480x360 meselesini kimse yakalamadı** (§0). UI için tam sayı ölçek scripti (§7.2) |
| Odak imleci | halka, 2 kare nabız | node 16x16, 2 kare | halka 13x13, 2 kare nabız | plak 12x12, 4 kare dönüş | Dönen 4 kare piksel kaymasız ise olur ama **2 kare nabız en ucuz**; üç yön hemfikir |
| Ana araç tahmini "Aseprite importer" | var (doğrulanmadı) | `com.unity.2d.aseprite` | PNG | PNG | Kurulu değil; PNG export yeterli |

---

## 4. Öneri: en iyi 3 seçenek

Genel ilke: **önce yalın çekirdek (C), karakter katmanı sonra.** Gerekçe: (1) GDD sütun 1 ve 3 (akış, anında respawn) ve "sanatçı değil" kısıtı; (2) kalan iş yükü (38 oda); (3) hiçbir yönün ürettiği "kimlik" katmanı, çalışan bir ana menü/pause/HUD olmadan değerlendirilemez. Tüm seçeneklerde: **tüm yeni UI Screen Space - Overlay**, Tiny5 Raster atlas, greybox önce.

### Öneri 1 (birincil): "C çekirdek + B'nin panel dili" (hibrit)

**Özet.** UI'nın akış/HUD davranışı C'den: kutusuz metin menüsü, gökkuşağı halka odak imleci, olay güdümlü HUD (ölümde hiçbir şey), sadece pause/sonuçta panel. Panel görünümü B'den: tek hero panel ("turuncu çerçeve + pembe damla şerit + mürekkep metin yuvası `#433646`"), böylece UI oyunun malzemesinden doğmuş görünür ama sadece **panel olan 3 ekranda** (pause, sonuç, ayarlar). Boru/vana/buhar süsü **yok**; ana menüde yalnızca dünyadan bir kadraj (iki dev halka, pembe damla zemin şeridi). Metin her zaman krem + 1 piksel ink gölge.
**Fikirler:** C (yapı, ekranlar, HUD, ikonlar, iş akışı), B (panel dili, mürekkep yuva kuralı, `UIStyleSO` fikri: mevcut `MultiplayerUISettingsSO`'yu genişlet), A (pause/start davranışı, odak "çek" hissi), D (sonuç ekranında iki cüce, ikinci aşama).

**Tahmini akşam:** **9** (aralık 8-11), yaklaşık 4-5 Cuma + 4 Pazartesi.

**En küçük çalışan sürüm (MVP): 3 akşam.**
| Akşam | Gün | İş |
|---|---|---|
| 1 | Cuma `#code` | Tiny5 Raster atlas (Latin + Latin Ext-A) + TR testi; `UITheme` (mevcut SO'yu genişlet); odak/ekran stack yardımcısı (`SetSelectedGameObject`, B = geri) |
| 2 | Pazartesi `#art` | Palet (kaynak PNG'lerden örnekle); hero panel 9-slice (kodla geometri + elle rötuş); odak halkası 2 kare |
| 3 | Cuma `#code` | Ana menüyü yeniden kur (`MainMenuBuilder` stili), Overlay + tam sayı ölçek scripti, yalnızca gamepad turu |

**3 akşamda görünen:** Tiny5 raster fontlu, krem-ink gölgeli, halka imleçli, sky arka planlı **ana menü**. Pause ve HUD henüz yok.

Devamı: Pzt `#art/#test` (ikon seti 8, pip, dither, 1080p/1440p turu) -> Cum `#code` (Pause sistemi: timeScale + unscaled tween + Start/B) -> Pzt `#art` (HUD ikonları, sonuç sprite'ları) -> Cum `#code` (Coin UI Overlay'e + MP sonuç + RaceHud) -> Pzt `#test` (son tur) vb.

**Riskler:** (1) Pause sıfırdan yazılıyor; MP'de oyunu durdurmamalı (hayalet yarış), `IsPaused` ismine dikkat. (2) HUD'ın Overlay'e taşınması CRT'siz "keskin OSD" görünümü verir; kabul edilmeli. (3) Hero panel "tek şeritli" kalmazsa B'nin kapsam kayması sızar; kural: ekran başına tek panel tipi, süs yok. (4) Kutusuz menü parlak maviye karşı kontrastı ink gölgeye bağımlı; test et. (5) Gökkuşağı yalnızca odak/kazanan; fazla kullanımda "tutunulabilir" anlamı aşınır.

### Öneri 2: "Saf C" (en ucuz, sanatsız başla)

**Özet.** C'yi olduğu gibi uygula: buton kutusu yok, panel yalnızca pause/sonuç/ayarlar, 1 hero panel (16x16, ink + krem iç çizgi + night dolgu), halka imleç, 8 elmas ikon, pip. Sprite'sız "aşama 1" (düz renkli Image'lar) gerçekten oynanabilir menü/pause/HUD verir; sanat sonra takılır. Kimlik menüdeki iki halka + başlıktan gelir.
**Fikirler:** C (tamamı), A (palet kuralları, `>` odak katmanı), skills-digest (greybox, hero asset önce).

**Tahmini akşam:** **7** (aralık 6-9).

**MVP: 2 akşam.** Cum #code: Tiny5 Raster + `UITheme` + odak yardımcısı + ana menü düz renk (sprite yok, `>` ile odak); Pzt `#art`: palet + panel 9-slice + halka 2 kare. **2 akşamda görünen:** Tiny5 raster fontlu, düz renk, çalışan gamepad menüsü (ve halka imleç eklenmiş ana menü).

**Riskler:** Kimlik ("yavan"); geliştirici "kutusuz menü" estetiğini sevmeyebilir; HUD'ın az bilgisi kaçırılabilir (özellikle ananas bekleyen/kalıcı ayrımı, C'nin kendi uyarısı).

### Öneri 3: "C çekirdek + D'nin Setlist menü/sonuç afişi"

**Özet.** C çekirdeği (HUD, pause, ikonlar) + D'nin yalnızca **iki ekrandaki** tema katmanı: ana menü "afiş" (sky arka plan, iki dev halka plak gibi 8 kareli yavaş döner, bitmap logo, pembe bant), MP sonucu "BATTLE OF THE BANDS" (iki cüce, kazanan zıplar). Pause ve HUD'da tema yok. D'nin metafor yorgunluğu riskine karşı: HUD yalnızca sade; "TRK 05"/REW eklenmez.
**Fikirler:** C (çekirdek), D (afiş fikri, plak halka, iki cüce, İngilizce menü), A (pause).

**Tahmini akşam:** **10** (aralık 9-12). **Ön koşul:** dil kararı (İngilizce menü) ve logo çizim yeteneği.

**MVP: 3 akşam.** Öneri 1'in MVP'siyle aynı çekirdek (Cuma #code, Pzt #art, Cuma #code), ardından afiş katmanı sonra. **3 akşamda görünen:** C'li çalışan ana menü; afiş/logo/plak sonraki akşamlarda.

**Riskler:** Bitmap logo ve cüce animasyonu sanatçı olmayan biri için en belirsiz kalemler; "kimlik" kazancı sadece iki ekranda; İngilizce menü kararı yeniden Türkçe çevirisi/yerelleştirme iş yükü getirir; GDD §7 4. duvar tonuyla uyumlu ama dünyada kaset/afiş yok.

### Önermediğim: tam B ve tam A
- **Tam B (14-18 akşam):** Kimlik açısından en doğru, ama 7-9 hafta level üretimi durur (38 oda kalmışken), ~50 sprite sanatçı olmayan biri için gerçekçi sınırın üstünde, ve dünya temaları değişince yeniden çizim riski. Hibrit (Öneri 1) B'nin değerli %60'ını ~%35 maliyetle alıyor (B'nin kendi 10. risk notu da aynısını söylüyor).
- **Tam A (12-14 akşam):** Arcade teması dünyada yok; sütun 1 (akış) ile ses/juice dozu çelişebilir. İyi parçaları (pause davranışı, `>` odak katmanı) hibride alındı.

---

## 5. Kullanıcıya sorulacak kararlar (max 5)

1. **Dil:** Menü/HUD Türkçe mi, İngilizce mi, ikisi mi? (Tiny5 TR glifleri var; ikisi isteniyorsa metinler en baştan string tablosuna.)
2. **Sanat:** Aseprite alınır mı (19.99 USD) ve haftalık 1-2 Pazartesi'yi çizime ayırır mısın, yoksa **greybox + en az sprite** (Öneri 2)?
3. **Kapsam ve level bedeli:** İlk sürüm "ana menü + pause + HUD" (yaklaşık 5-6 akşam) yeterli mi; UI'a toplam kaç akşam feda edebilirsin (9 akşam level üretimini ~4.5 hafta yavaşlatır)?
4. **Sayaçlar:** Ananas/altın sayacı **kalıcı** mı kalsın, olay güdümlü mü olsun (GDD §12.4 amaç belirsiz); dash/slingshot için HUD'a bir şey eklensin mi (mevcut karakter rengi yeterli mi, playtest senin)?
5. **CRT ve UI:** Yeni UI'ın CRT'nin **dışında (keskin OSD)** durmasını kabul ediyor musun? (Teknik olarak 480x360 CRT hücresi 4x4 pikseli bozduğu için önerim evet.)

Ayrıca (karar değil, teyit): gamepad ikon seti (nötr elmas / Xbox harfleri), Steam Deck/1440p desteği, Coin UI/StaminaCanvas'ın bugünkü görünümü.

---

## 6. Üretim araçları rehberi (yönden bağımsız)

### 6.1 Çözünürlük ve ölçek

- **Üretim gridi: 480x270 sanat pikseli, 4x = 1920x1080.** 1x'te çiz, **hiç büyütülmüş PNG'yi kaynak yapma**, ölçeği Unity'de tam sayı ile ver. 640x360 (3x) daha ince detay ama sanatçı olmayan biri için fazla iş; 3x'te Tiny5 em 8 px de 24 ekran px olur (konsol eşiği 26'nın altında), yani **4x doğru seçim**.
- **Font:** Tiny5 em = 8 px (doğrulandı). TMP font size **32 / 64 / 96 / 128** (4x katları). Ara değer kullanma.
- **Boyutlar çift sayı**, güvenli alan %5 (24x13 sanat px), kritik bilgi köşeye değil.
- **Dünya pikselleri ayrı:** dünya sprite'ları PPU 16 ve kamera zoom'una bağlı; UI gridi onunla hizalanmaz, bu normal (UI Overlay).
- **Dikkat (bu projeye özgü):** CRT 480x360 hücre kullanıyor (§0). Overlay UI bundan etkilenmez. CRT içindeki UI yapma.

### 6.2 Beyaz taslak mı final mı

Dört ajan da hemfikir ve doğru: **önce greybox, sonra hero asset, sonra aile.**
1. **Greybox (palet renginde düz dikdörtgenler + Tiny5 raster + doğru 9-slice sınırları)**, tüm ekranlar Unity'de, yalnızca gamepad ile gezilir. Kod ve akış sanat beklemeden ilerler. "Beyaz" değil, **palete uygun düz renk** (beyaz placeholder dünyada zaten karışıklık yaratıyor, GDD §8.1 #2).
2. **Hero asset onayı:** tek panel + tek odak imleci elle çizilir, ana menüde görülür, onaylanır.
3. **Aile üretimi:** ikon/pip/varyant hero'dan türetilir.
4. Final: juice ve ses.

### 6.3 Araç tablosu: ne zaman hangisi

| İş | Araç | Neden / ne zaman |
|---|---|---|
| Piksel çizim, animasyon, 9-slice kaynağı, palet kilidi | **Aseprite** (~19.99 USD, tek seferlik; itch.io alımı Steam anahtarı verir) | Bu iş için en uygun; tag ile durum/animasyon, slice, indexed palet. Alırsan hero asset aşamasında al, greybox'ta gerekmez |
| Ücretsiz yedek | **LibreSprite** (Aseprite eski GPL çatalı), **Piskel** (web) | LibreSprite temel iş için yeter, güncelliği doğrulanamadı; Piskel küçük ikon taslağına iyi, palet/9-slice zayıf |
| Yerleşim taslağı | **ASCII wireframe + Unity greybox** | Figma'ya gerek yok: pixel çizmek için uygun değil (vektör, AA); bu oturumda Figma bağlantısı zaten yetkilendirilmemiş |
| Geometrik asset (9-slice çerçeve, dither, pip, atlas), builder/prefab | **Claude + Unity `eval` veya PowerShell `System.Drawing`** | Deterministik, palet kodda kilitli, diff okunur. Python/Pillow bu makinede kırık (§2.1 #9) |
| Moodboard, "bu panelde ne olsun" fikri, ikon ilhamı, logo kompozisyon fikri | **Genel AI görsel araçları** | Yalnızca referans; sonucu kopyalama, elle yeniden çiz |
| Hazır pixel UI paketi (placeholder/ilham) | itch.io / Kenney CC0 | Proje zaten Kenney fontlarıyla geliyor; Kenney UI paketleri stil olarak oyunla uyuşmaz ama greybox için hızlı |

### 6.4 AI ve pixel UI: dürüst ayrım

**Zayıf kaldığı yerler (pixel UI sprite'ı için):**
- Gerçek piksel gridine oturmuyor ("sahte piksel": yarım piksel, tutarsız boyut, AA, soluk ara tonlar).
- Palet tutmuyor; 12x12 ikonda anlamlı çıktı vermiyor.
- 9-slice için simetri/tile sınırı üretmiyor; bir panel ailesi (aynı border, 4 durum) tutarlı çıkmıyor.
- Temizleme çoğu zaman sıfırdan çizmekten uzun sürüyor; Aseprite'te piksel gridine zorlamak ayrı bir iş.
- Telif/lisans belirsizliği (ticari Steam yayını); SpriteCook gibi servisler hesap + kredi ister, Godot odaklı, fiyat/kalite doğrulanmadı.

**İşe yaradığı yerler:**
- **Konsept ve moodboard:** "afiş mi, panel mi, boru mu" diye 5 dakikada 20 görsel fikri.
- **İkon ilhamı:** "ananas, anahtar, bayrak ikonu nasıl okunur" referansı (sonra elle çiz).
- **Kompozisyon:** ekran yerleşim fikri, renk uyumu denemesi.
- **Yazı/kopya:** menü metinleri, sonuç ekranı esprileri (GDD §7 tonu).
- **Kod/prefab/builder:** UI kurulum kodu, TMP ayarı, test; asıl gerçek kazanç burada.

### 6.5 Unity import ayarları

| Ayar | Değer |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Pixels Per Unit | **25** (Canvas `Reference Pixels Per Unit = 100` ile 1 sanat px = 100/25 = **4** canvas birimi). Doğrula: 16x16 test sprite'ı ekranda 64x64 görünmeli |
| Filter Mode | **Point (no filter)** |
| Compression | **None** |
| Generate Mip Maps | **Kapalı** |
| Wrap Mode | Clamp (tile sprite'larda Repeat) |
| Mesh Type | **Full Rect** (9-slice için; Tight kenarı bozar) |
| Sprite Mode | Single (panel/buton), Multiple (ikon sayfası) |
| 9-slice | Sprite Editor'da border L/B/R/T = **tam sayı sanat pikseli** (panel 4-6), `Image Type = Sliced`, Fill Center açık, kenar tile; tüm durumlar (normal/focus/pressed/disabled) **aynı boyut ve slice** |
| Sprite Atlas | UI için tek atlas, Point, padding >= 4, Tight Packing kapalı |
| Canvas | **Screen Space - Overlay** (CRT dışı); `CanvasScaler` Scale With Screen Size 1920x1080 |
| Ölçek | **Tam sayı** (aşağıda) |
| TMP | **Raster (hinted)** atlas, Point, AA yok; Static, Latin + Latin Ext-A; 32/64/96/128. Gölge: Underlay 1 sanat piksel (4 ekran px), softness 0, renk ink |
| Metin | TMP `<allcaps>` (kültür sorunu yok, `ToUpper()` değil) |
| İsimlendirme | `ui_[nesne]_[varyant]_[durum]` (örn. `ui_panel_main_9s`, `ui_cursor_ring_01`, `ui_icon_south`) |

### 6.6 Tam sayı ölçek (3 satırlık script önerisi)

Scale With Screen Size 1440p'de 5.33x verir (kenarlar bulanır), Steam Deck 1280x800'de 2.96x. Küçük bir script (`CanvasScaler.uiScaleMode = ConstantPixelSize`, `scaleFactor = Mathf.Max(1, Mathf.RoundToInt(Screen.height / 270f))`) 1080p = 4, 4K = 8, 1440p = 5, Deck = 3 verir; ölçek kadar UI boyutu ekrana göre biraz değişir ama kenar hep tam piksel. Bu **öneridir, uygulanmadı**; yapılacaksa Öneri 1'in ilk Cuma'sına girer. Not: 1440p/Deck desteklenecek mi sorusu (§2.3 #7) buna bağlı.

### 6.7 Doğrulama listesi (her hero asset sonrası)

1080p, gerçek arka plan, yalnızca gamepad, nearest-neighbor contact sheet ile aile tutarlılığı; `unity command capture_game_view --source screen` (Play Mode, Overlay UI yalnızca orada çıkar). CRT açık sahnede **dünya** ile yan yana bak, UI'ın Overlay olduğunu unutma.

---

## 7. Ek: bir sonraki adım önerisi (hiçbiri yapılmadı)

1. Tiny5 Raster atlas ve palet örnekleme (en riskli bilinmeyenler kapanır).
2. Play Mode'da `--source screen` ile ana menü, RaceHud ve NewScene karelerine bak: Overlay'in CRT'siz olduğunu ve mevcut Coin UI/StaminaCanvas görünümünü gör (2 dk).
3. Karar sorularını (§5) yanıtla; seçilen yön için task'lar `TASKS.md`'ye **senin onayınla** eklenir, o zaman Pazartesi/Cuma tarihleri ve takvim güncellemesi (CLAUDE.md) yapılır.

---

## 8. Kaynaklar

- Aseprite fiyatı: [dacap.itch.io/aseprite](https://dacap.itch.io/aseprite), [SteamDB Aseprite](https://steamdb.info/app/431730), [Aseprite blog](https://blog.aseprite.org/2026/06/30/new-website-account/)
- Tiny5 lisans/kapsam: [github.com/gissio/font_tiny5](https://github.com/gissio/font_tiny5), [Google Fonts Tiny5](https://fonts.google.com/specimen/Tiny5/about), [Fontsource Tiny5](https://fontsource.org/fonts/tiny5/about)
- Doğrudan okunan dosyalar: `Assets/Scripts/CRTEffect/CrtRendererFeature.cs`, `CrtRenderPass.cs`, `Assets/Shader/CRTFilter.shader`, `Assets/Scenes/NewScene/Global Volume Profile.asset`, `Assets/DefaultVolumeProfile.asset`, `Assets/UI/Fonts/Tiny5-Regular SDF.asset` ve `.ttf`, `Assets/Scenes/NewScene.unity`, `MainMenu.unity`, `MultiplayerLobby.unity`, `MainMenuBuilder.cs`, `RaceHud.cs`, `CollectibleUIController.cs`, `PlayerController.cs`, `docs/moodboard/level_01/05/09/16/21.png`.
- Aranıp bulunamayanlar: Unity `com.unity.2d.aseprite` paketi, LibreSprite bakım durumu (arama sonuç vermedi).
