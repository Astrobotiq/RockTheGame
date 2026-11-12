# Yön C: "Minimal ve Oyun-Önce"

Tarih: 2026-11-08. Durum: öneri, karar geliştiricinin. Okunanlar: `skills-digest.md`, GDD §1-§5 ve §7 (+§8), moodboard 01, 05, 10, 15, 20, 21, `MainMenuBuilder.cs`, `unity:2d-pixel-perfect` skill'i. Bu belgedeki hex değerleri moodboard ekran görüntülerinden **gözle** çıkarıldı (CRT filtresi renkleri kaydırıyor); piksel örneklemesi yapılmadı, bkz. §2.

---

## 1. Vizyon ve neden uyar

**Tek cümle:** UI, oyunun içinde bir "tutorial yazısı" gibi davranır: beyaz-krem piksel yazı, gökkuşağı halka, başka hiçbir şey.

- **Hız/akış sütunu (§2.1).** Oyun sırasında ekranda sabit HUD yok. Bilgi ya karakterin üstünde/yanında, ya da yalnızca değiştiği an 1,5 saniye görünür. Göz kancaya ve node'lara kalır; moodboard'da node'lar zaten en baskın öğe, UI onlarla yarışmamalı.
- **Hızlı respawn (§2.3, §5.3).** Ölümde hiçbir ekran, sayaç, "tekrar dene" paneli çıkmaz. UI ölümü kesmez; yalnızca "geçici ananas" sayısı sessizce sıfırlanır.
- **Zor ama adil (§2.3).** Okunurluk bahane bırakmaz: yüksek kontrast, tek vurgu rengi, anlam yalnızca renkle değil şekille de verilir.
- **Kanca merkezde (§2.2).** Gökkuşağı halka = "tutunulabilir/seçilebilir" anlamı. Menüde odak göstergesi de küçük bir halka olur: menü maddesi bir node'dur, ona "kancalanırsın".
- **Tek geliştirici, haftada iki akşam.** Toplam ~10 küçük sprite, 1 panel, 1 font. Menüde düğme kutusu yok, yalnızca metin + halka. Ham beyaz taslakla başlayıp sprite'ları sonradan takmak mümkün (bkz. §7.3).

Bu yönün dezavantajı dürüstçe: "sevimli şeker dünyası" kimliğini (§8) UI'ya taşımıyor; UI nötr ve bilgi odaklı kalıyor. Menüde kimliği iki büyük halka ve başlık taşır (§5.1).

---

## 2. Palet (14 renk)

Tek palet, tüm UI sprite'ları ve TMP renkleri buradan. Saf siyah/beyaz yok; gölgeler mora, ışıklar sarıya kayar. **Doğrulanmadı:** hex'ler tahmini; Aseprite'te level_01'den bir `Eyedropper` turuyla (CRT'siz kare çekip) kalibre edilmeli. Aşağıdaki "rol" sütunu hex'ten önemli.

| # | İsim | Hex | Rol |
|---|---|---|---|
| 1 | ink | `#1B1530` | Outline, gölge, "siyah" yerine |
| 2 | night | `#2D2650` | Panel dolgusu |
| 3 | dusk | `#4A3F7A` | Pasif/disabled, panel iç çizgi, boş pip |
| 4 | cream | `#FFF4D6` | Ana metin, ikon gövdesi ("beyaz" yerine) |
| 5 | sky | `#7CC8F2` | Menü arka planı (oyun gökyüzüyle aynı) |
| 6 | pink | `#F25AA3` | **Tek vurgu**: seçili madde, birincil eylem (zemin kremasıyla akraba) |
| 7 | amber | `#FFB000` | Ananas/koleksiyon, halka ortası |
| 8 | yellow | `#FFE84A` | Odak parlaması, halka çekirdeği, +1 flaşı |
| 9 | orange | `#F09A5C` | Zemin rengi, ikincil vurgu |
| 10 | redorange | `#FF4B2B` | Tehlike/tükendi (dash/slingshot yok, stamina bitiyor) |
| 11 | acid | `#4CD94C` | Hazır/olumlu (asit yeşiliyle akraba) |
| 12 | teal | `#2A8CA0` | Halka dış bandı |
| 13 | olive | `#9A9A2E` | Halka orta bandı |
| 14 | rose | `#E0707A` | Halka en dış bandı |

Kurallar:
- **Gökkuşağı halka (4, 12, 13, 7, 10, 8 + 14) yalnızca "kanca hedefi / odak / kazanan" için.** Dekor olarak başka yerde kullanılmaz; anlamı yoksulaşır.
- Metin her zaman cream (#4) ya da pink (#6); ink (#1) 1 sanat pikseli aşağı-sağ gölge.
- Alpha yalnızca 0 veya 255. Tek istisna: pause karartması, o da yarı saydam değil **dither** (bkz. §4.4).
- Kontrast: cream/night yaklaşık 12:1 (hesaplandı, gözle değil formülle yaklaşık; kesin değerler doğrulanmadı). **Sorun:** moodboard'daki beyaz tutorial yazısı sky mavisi üstünde yaklaşık 1,8:1; WCAG-benzeri eşiğin çok altında. Çözüm: dünya içi yazıya da ink gölge (1 sanat pikseli) zorunlu, §3.

---

## 3. Tipografi

**Tek font: Tiny5** (`Assets/UI/Fonts/Tiny5-Regular SDF.asset` mevcut). Başka font yok. Kenney fontları (klasörde var) UI'dan çıkarılmalı; ikinci piksel yoğunluğu getirir (skills-digest: "karıştırma").

| Rol | Ekran boyutu (TMP) | Sanat pikseli karşılığı | Kullanım |
|---|---|---|---|
| Başlık | 128 | 4x4 grid'in 32'si | Yalnızca ana menü başlığı, sonuç "KAZANDIN" |
| Başlık-2 | 64 | 16 | Pause başlığı, ekran adı |
| Gövde | 32 | 8 | Menü maddeleri, sonuç satırları, tutorial |
| HUD | 32 | 8 | Ananas sayısı (rakamlar) |

- **Varsayım, doğrulanmadı:** Tiny5'in em'i 8 font-pikseline karşılık geliyor ve 4x ölçekte 32 px = tam 4 ekran pikseli/glif pikseli. Fonta ve TMP atlasına bakarak (büyük harf yüksekliği 5 glif pikseli olmalı) doğrulanmalı; eğer em farklıysa sadece tabloyu değiştir, kural aynı: **yalnızca tam katlar** (32/64/96/128), ara boyut yok.
- Gövde 32 px, digest'teki konsol eşiği (≥26) üzerinde.
- TMP: Point sampling, anti-aliasing kapalı, atlas "Raster" ya da SDF ama `Sharpness=1` (Unity 6 notu). **Doğrulanmadı:** mevcut `Tiny5-Regular SDF.asset` bu şekilde mi üretilmiş (git status'ta değişmiş görünüyor); pikselde kenar yumuşaması varsa Raster atlas'a geçmek gerekebilir. Bu, ilk `#code` akşamının ilk işi (bkz. §8).
- **Gölge:** TMP Underlay, dilate 0, softness 0, offset 1 sanat pikseli (4 ekran px) aşağı-sağ, renk ink. Dış çizgi (outline) kullanma; piksel fontta temiz çıkmaz.
- **Türkçe karakter (ç ğ ı İ ö ş ü):** **doğrulanmadı** Tiny5 atlasında var mı. Mevcut menü Türkçe ("Tek kişilik") ve çalışıyor görünüyor ama atlasın o glifleri taşıdığı kontrol edilmedi. Eksikse Dynamic atlas ya da karakter listesi eklenmeli.
- **Dil kararı bekliyor:** menüler Türkçe, moodboard tutorial'ları İngilizce ("USE L TO WALK"). Bu belge Türkçe örnek veriyor; tek dil seçilmeli (UI yerelleştirilecekse metinler en baştan string tablosunda).
- Metin container'a göre büyüsün; sabit genişlikli buton yok (zaten buton kutusu yok).

---

## 4. Bileşen dili

### 4.1 Referans ızgara
- **480x270 sanat ızgarası, tam 4x** (1920x1080). Tüm UI sprite'ları 1x'te çizilir, import'ta ölçek yok, Canvas Scaler 1920x1080 ve `Match 0.5` (16:9 dışında pillarbox, HUD köşelere anchor).
- Safe margin: yatay 24, dikey 14 sanat pikseli (yaklaşık %5; 96x56 ekran px). CRT vinyeti köşeleri yiyor; köşeye kritik bilgi koyma.
- Tüm `anchoredPosition` 4'ün katı. Boyutlar çift sayı.

### 4.2 Buton: yok
Menü maddesi = tek satır metin. Kutu, çerçeve, arka plan yok. Üç durum:

| Durum | Görünüm |
|---|---|
| Normal | cream metin, ink gölge |
| Odakta | pink metin, soluna odak halkası, metin 1 sanat piksel sağa kayar |
| Basıldı | 2 kare yellow flaş (bkz. §6) |
| Disabled | dusk metin, halka yok, atlanır |

Gerekçe: en az asset, en yüksek okunurluk, odak hem renk hem **şekil** (halka) hem **konum** (kayma) ile belli, yani renk körü dahil okunur.

### 4.3 Odak göstergesi: mini gökkuşağı halka
- 13x13 sanat piksel, 3 eş merkezli band (rose, cream, amber çekirdek yellow ile), proseduel çizilir. 4 kareli dönüş yok; 2 kare "nabız" (çekirdek yellow <-> amber), 8 fps.
- Metnin sol kenarından 2 sanat pikseli boşlukla, metin ortasına dikey hizalı.
- Neden halka: node dili; kullanıcı menüde de "kancaya" yönelir. Neden mini: dev halka menü maddesinden dikkat çalar.

### 4.4 Panel ve 9-slice (tek hero asset)
- `ui_panel_9s`: 16x16 kaynak, **border 4** (sol/sağ/üst/alt), `Image Type = Sliced`, `Fill Center` açık, `Pixels Per Unit Multiplier` Canvas ölçeğine göre 1.
- Katman: 1 px ink dış çizgi, 1 px cream iç çizgi, dolgu night; köşelerde 1 piksel kesik (yuvarlak hissi). Gölge yok.
- Kullanım: **yalnızca pause, ayarlar, MP sonuç.** HUD ve ana menüde panel yok.
- Karartma: `ui_dither_50` (2x2: iki ink, iki saydam, çapraz), tam ekran **tile** edilmiş Image. Yarı saydam alpha değil; piksel diline uyar ve CRT altında temiz.

### 4.5 Gamepad ikonları (8 adet, 12x12)
Marka-bağımsız: Xbox/PlayStation ayrımı yok (GDD "A / ✕" diyor). Yüz tuşları 4 noktalı **elmas**, basılacak olan dolu cream, diğer üçü dusk. Harf/sembol yok.

| İsim | Çizim |
|---|---|
| `icon_south` | elmas, alt nokta dolu (zıplama/onay) |
| `icon_east` | elmas, sağ nokta dolu (geri/B) |
| `icon_west` | elmas, sol nokta dolu (dash) |
| `icon_north` | elmas, üst nokta dolu (şimdilik kullanım yok; opsiyonel) |
| `icon_stick_l` | daire + "L" (moodboard'daki beyaz L dairesinin temizi) |
| `icon_lt` / `icon_rt` | yamuk tetik + L / R |
| `icon_lb` / `icon_rb` | yatay çubuk + L / R |
| `icon_start` | üç yatay çizgi |

TMP **Sprite Asset** olarak bağlanır; metin içinde `<sprite name="icon_west">` ile akar (tutorial yazılarında ve ipuçlarında). Gereken minimum 6; north ve lb/rb'nin kullanımı oyun kontrollerine göre budanabilir (GDD: bumper duvara tutunma için var, kalmalı).

### 4.6 Küçük parçalar
- **Pip:** 5x5, dolu (cream/amber) ve boş (dusk). Anahtar sayacı, bayrak sayacı, ses ayarı slider'ı aynı iki sprite'ı kullanır.
- **Ananas ikonu:** oyundaki mevcut ananas sprite'ı olduğu gibi (ayrı UI sürümü çizme).
- **Stamina çubuğu:** sprite yok, düz renkli dikdörtgen Image, genişlik tam sayı piksel.

---

## 5. Ekranlar

Koordinatlar 480x270 sanat ızgarasında. `[ ]` = boşluk, `(o)` = odak halkası.

### 5.1 Ana menü
Mevcut `MainMenuBuilder`: tek `VerticalLayoutGroup`, 900x700 panel, 120 px başlık. Yeni hali moodboard `level_01`'in kopyası gibi: sky arka plan, iki büyük halka "göz" gibi, başlık ve üç madde.

```
 ┌──────────────────── 480 x 270 ────────────────────┐
 │                                                    │
 │      ◎◎◎◎◎                    ◎◎◎◎◎   (mevcut node │
 │     ◎ halka ◎                ◎ halka ◎   sprite'ı, │
 │      ◎◎◎◎◎                    ◎◎◎◎◎    arkada)     │
 │            R O C K  T H E  G A M E    <- 128 px    │
 │                                                    │
 │                (o) Tek kişilik        <- odakta    │
 │                    Çok oyunculu                    │
 │                    Çıkış                           │
 │                                                    │
 │  [south] Seç                    [east] Geri        │
 └────────────────────────────────────────────────────┘
```
- Arka plan: düz sky (kamera `backgroundColor`, mevcut hali zaten böyle). İki halka: oyundaki node sprite'ı integer ölçekle. **Dikkat:** node sprite'ının piksel yoğunluğu UI gridinden farklı (moodboard'da ~5-6 ekran px/piksel görünüyor, 4 değil). Dekor olduğu için kabul edilebilir, ama UI parçalarıyla yan yana aynı pikselde değil; **doğrulanmadı**, gerçek sprite'ta ölçülmeli.
- Alt şerit: iki ikon ipucu, `icon_south` + "Seç", `icon_east` + "Geri". Her ekranda aynı yerde (alt sol/sağ safe margin).
- İlk seçili: Tek kişilik (`EventSystem.firstSelectedGameObject`, mevcut kodda zaten var).
- Çıkış en altta, Steam daveti kabul edilirse `MainMenuUI` akışı mevcut davranışı korur.

### 5.2 Pause
Tetik: Start. Zaman durur (`Time.timeScale = 0`, tüm UI tween'leri `SetUpdate(true)`).
```
 ┌────────────────────────────────────────────────────┐
 │  (oyun, dither 50% ile karartılmış)                │
 │               ┌────────────────────┐               │
 │               │      DURAKLADI     │ <- 64 px      │
 │               │                    │               │
 │               │  (o) Devam         │               │
 │               │      Son checkpoint│               │
 │               │      Ayarlar       │               │
 │               │      Ana menü      │               │
 │               └────────────────────┘               │
 │  [south] Seç                  [east/start] Devam   │
 └────────────────────────────────────────────────────┘
```
- Panel 9-slice, merkez, yaklaşık 200x130 sanat piksel (800x520 ekran).
- "Son checkpoint": takılıp kalan oyuncu için (GDD'de yok, **öneri**, yardım modu kararı bekliyor, §12.1).
- Ayarlar: üç satır, her biri etiket + 10 pip (sol/sağ stick ile): Müzik, Efekt, Titreşim. Mevcut ayarlar sistemi olmadığı için (GDD §11: "ayarlar yok") bu ek iş (bkz. §8). **Ekran/çözünürlük/tam ekran ayarı eklemedim**; Steam/Unity varsayılanı yeterli.
- B (east) veya Start: Devam. Ekran yığını: Ayarlar B ile Pause'a döner.

### 5.3 HUD (neredeyse görünmez)
Kural: **Hiçbir şey sürekli görünmez.** Olay olunca belirir, 1,5 sn sonra çıkar; ekranın sol üst safe margin'inde.

```
 sol üst (24,14):                     [ananas] 7   <- sadece toplayınca / odaya girince 1,5 sn
 oda içinde, kapı üstü / yanı:        o o . .      <- anahtar pip'leri: DÜNYA içinde, kapıda (diegetik)
 karakter yanında:                    (aşağıda)
```

**Ananas/anahtar sayacı**
- Ananas: ikon + sayı (`32 px`, cream + ink gölge). Toplanınca belirir, +1'de sayı 1 sanat piksel yukarı sıçrar ve 2 kare yellow olur; 1,5 sn sonra çıkar. Checkpoint'te sayı kalıcı olur: kısa bir `acid` flaş (ölümde kaybolan "bekleyen" sayı ile kalıcı sayı **aynı** gösterilirse oyuncu kaybı anlamaz: iki sayı gösterme önerisi `7 (+2)`; bekleyen kısım dusk rengi, **tasarım kararı bekliyor**).
- Anahtar: toplanan/toplam pip'leri **kapının üstünde** (dünyada) sürekli, çünkü tüm bilgi orada anlamlı. Ekrana sabit sayaç yok. Toplayınca HUD'da 1,5 sn aynı pip satırı da çıkar. **Doğrulanmadı:** `RoomDoor` üstüne world-space Canvas eklemek mevcut kapı prefab'ını nasıl etkiler; kod okunmadı.
- Event-driven: `CollectibleInventorySO`'ya ve `PlayerHealth.OnDeath`'e abonelik; `Update`'te polling yok (CLAUDE.md mimarisi).

**Dash / slingshot göstergesi (karakter üstünde, ekran HUD'ı yok)**
- Mevcut: tükenince karakterin rengi değişiyor (GDD §5.2). Bunu **korumak** birincil çözüm; HUD maliyeti sıfır. **Doğrulanmadı:** bu tint'in şu anki rengi ve okunurluğu oyun içi çekimde görülmedi (moodboard'da oyuncu yok).
- Ek öneri (opsiyonel): karakterin hemen üstünde, **yalnızca tükendiğinde** iki küçük işaret: dash = 3x3 nokta, slingshot = 5x5 mini halka; tükenikken redorange, yenilenince bir kare yellow parlayıp acid olur ve 0,3 sn sonra kaybolur. Hazırken görünmez. Gerekçe: göz zaten karakterde.
- **Wall stamina:** son ~2 sn'de (GDD ~6 sn, "azalırken uyarı") karakterin yanında 12x2 sanat piksel çubuk, redorange; tam doluyken görünmez. Uyarı zaten var olduğu için (ses/titreşim?) bu görsel ek **doğrulanmadı**, PlayerController'da neye bağlanacağı okunmadı.

### 5.4 MP sonuç ekranı
Mevcut ekran (commit `72d7e43`) okunmadı; bu bir yeniden tasarım önerisi, `RaceHud`/`RaceSession` alanları **doğrulanmadı**.
```
 ┌────────────────────────────────────────────────────┐
 │  (dither 50%)                                      │
 │         ┌──────────────────────────────┐           │
 │         │   ◎ KAZANDIN ◎   <- 128px,   │           │
 │         │     (kaybedene: BİTİRDİN)    │           │
 │         │                              │           │
 │         │   SEN        03:42.18  o o o │ <- süre + │
 │         │   RAKİP      04:05.77  o o . │    bayrak │
 │         │                              │    pip    │
 │         │  (o) Lobiye dön              │           │
 │         │      Ana menü                │           │
 │         └──────────────────────────────┘           │
 └────────────────────────────────────────────────────┘
```
- Kazanan satırın başında dönen halka (kazanan = gökkuşağı halka kuralı).
- Satırlar 120 ms arayla belirir (sayı tırmanmaz; süre doğrudan yazılır, okunurluk).
- Bayrak pip'i = yarış bayrakları (GDD §9: 3 bayrak ceza).
- "Tekrar oyna / rövanş" lobi mantığına bağlı, **doğrulanmadı**.

### 5.5 Oda geçişi
**UI yok.** Mevcut iris geçişi (`M_IrisTransition`) kalır. Oda adı/numarası kartı yok (akış kesilmez, GDD §7). İsteğe bağlı (varsayılan kapalı): geçişte sol altta 1 sn "21 / 60" küçük gövde metni; ilerleme hissi için, ama sütuna katkısı tartışmalı.

---

## 6. Hareket ve juice

Menü/ekran animasyonları kısa (<= 200 ms), **adımlı** (kare kare), yumuşak ease yok; piksel dili ve CRT'de temiz.

| Olay | Hareket |
|---|---|
| Odak değişimi | halka yeni maddeye 1 kareda atlar (anında), metin 1 sanat piksel sağa, 80 ms ses "tik" + hafif titreşim (mevcut `HapticManager`) |
| Basma | madde 2 kare yellow, 120 ms sonra eylem |
| Panel açılış | 3 adımlı ölçek (%50, %75, %100), 60 ms/adım, `SetUpdate(true)` |
| Panel kapanış | 2 adım, 40 ms/adım |
| Ananas +1 | sayı 4 ekran px yukarı, 2 kare yellow, sonra normal; HUD girişi kenardan 3 adımda |
| Dash/sling tükendi | işaret 1 kare belirir; yenilenince 1 kare yellow, 0,3 sn sonra kaybolur |
| Kazanan | halka 4 kare döner (8 fps) |
| Ölüm | UI hareketi yok |

- Adımlı ease: DOTween özel `EaseFunction` (`floor(t * n) / n`). **Doğrulanmadı:** projedeki DOTween sürümünde `SetEase(EaseFunction)` imzası; yoksa `AnimationCurve` stepped.
- Pause/sonuç ekranı tween'leri unscaled time. Pause sırasında tüm oyun UI'ı durur ama gamepad girişi çalışır.
- Ses: tek `AudioCueSO` seti (`ui_move`, `ui_confirm`, `ui_back`), mevcut `AudioCuePlayEventChannelSO` üzerinden.

---

## 7. Üretim planı

### 7.1 Boyut
- Referans: **480x270 (4x)**. Sprite boyutları: panel 16x16 (border 4), halka 13x13 (2 kare), ikonlar 12x12 x8, pip 5x5 x2, dither 2x2, işaretler 3x3 ve 5x5. Hepsi tek atlas, toplam <= 128x64 piksel civarı.
- Ters yönde eklemeyi tercih edin: 3x (640x360) alternatif; 4x seçildi çünkü tam sayı ve Tiny5 gövde 32 px.
- Bu `2d-pixel-perfect` skill'inin tablosuyla uyumlu (480x270: 1080p'de 4x). **Doğrulanmadı:** oyun dünyasındaki sprite PPU'su; moodboard'da dünya pikseli ekran pikseliyle 4x değil gibi, yani UI ızgarası dünya ızgarasıyla **hizalı olmayacak**. UI Screen Space Overlay olduğu için sorun değil, ama "aynı piksel boyutu" izlenimi tam olmaz.

### 7.2 Araçlar (dürüst)

| Araç | Pixel UI'da işe yarar mı? |
|---|---|
| **Aseprite** | Evet, ana araç. 12x12 ikon, 9-slice, 2 kareli halka, palet kilidi. Ücretli (yaklaşık 20 USD, **fiyat doğrulanmadı**) ya da kaynaktan derlenir. |
| **LibreSprite** | Aseprite'in eski GPL çatalı, ücretsiz; bu işe yeter, bazı yeni özellikler yok. Güncellik **doğrulanmadı**. |
| **Piskel** | Ücretsiz web aracı; ikon için yeterli, palet kilidi ve tag/slice yönetimi zayıf. Yedek. |
| **Figma** | Pixel sprite çizmek için **uygun değil** (vektör, ızgara kayması). Yalnızca ekran yerleşimi tahtası için iyi; bu yön zaten metin wireframe ile yetiniyor, gerek yok. |
| **Kodla üretim (Claude + Unity eval/Pillow)** | 12x12 ikon, halka, panel, pip, dither gibi **geometrik** parçalar için gerçekten işe yarar: ASCII/dizi tanımı -> PNG, palet kodda kilitli, diff okunur, hatalı pikseli düzeltmek bir satır. Organik şeyde (karakter, node dekoru) zayıf. Üretilen her şey Aseprite'te gözle kontrol edilmeli. |
| **AI görüntü üreticileri (Midjourney/DALL-E/difüzyon, SpriteCook vb.)** | **UI için pratikte yaramaz.** Gerçek piksel ızgarasına uymaz, "sahte piksel" üretir (yarım piksel, tutarsız boyut), 12x12'de anlamlı çıktı vermez, 9-slice sınırı ve palet tutmaz. Yalnızca moodboard/renk ilhamı olur. Final asset'e girerse temizlik, doğrudan çizimden uzun sürer. SpriteCook hesap + kredi ister, Godot odaklı; önerilmez (skills-digest). |

Önerilen akış: **geometrik parçaları kodla üret, Aseprite'te son rötuş + atlas.** Sanatçı olmayan tek geliştirici için en ucuz yol bu.

### 7.3 Ham taslak mı, final mı?
İkisi birden, iki aşamada:
1. **Aşama 1, ham ama işlevsel (sprite yok):** düz renkli `Image`'lar + TMP + Tiny5, palet SO'dan. Menü, pause, HUD, sonuç tamamen çalışır, odak göstergesi geçici bir `>` metni. Sprite'sizlik "ham beyaz" değil, **palette uygun düz renk**; test ve oyun hissi hemen alınır.
2. **Aşama 2, final sprite'lar:** panel, halka, ikonlar takılır. Aynı prefab'lar, yalnızca sprite referansı değişir.

Gerekçe: sanat bekleyerek kod bloke olmaz; sprite'lar küçük olduğu için aşama 2 ucuz.

### 7.4 Export ve Unity import
- PNG-32, şeffaf arka plan, 1-2 px padding. İsim: `ui_panel_9s`, `ui_focus_ring`, `ui_icon_south` ... (`[type]_[object]_[variant]_[state]`).
- Kaynak `.aseprite` dosyaları repoda `Assets/UI/Source/` gibi bir yerde (Unity'nin içe aktarmaması için uzantı zaten yok sayılır; **doğrulanmadı** projede Aseprite importer yüklü mü).
- Import: **Sprite (2D and UI)**, Pixels Per Unit **1** (UI için; dünya sprite'larıyla karıştırma), Filter **Point (no filter)**, Compression **None**, Mip Maps **kapalı**, Generate Physics Shape kapalı, `Sprite Mode` Single (ikonlar Multiple ya da tek tek).
- 9-slice: Sprite Editor'da 4/4/4/4, `Image Type = Sliced`.
- **URP 2D Pixel Perfect Camera:** UI Screen Space Overlay'de PPC'den bağımsız. Dünyada `upscaleRT` açılırsa CRT/post-process ve UI metni sorun çıkarır (skill: bilinen Unity hatası); şu an sahnede `m_PixelPerfect: 0` görünüyor, ne yapılacağı ayrı karar. **UI için PPC gerekmez**, Overlay canvas bunu atlar.
- TMP sprite asset: ikon atlasından `Create > TextMeshPro > Sprite Asset`.
- Doğrulama: 1920x1080, CRT açık, gerçek arka plan üzerinde; Play Mode'da `capture_game_view --source screen` (Overlay UI yalnızca Play Mode'da çıkar).
- **CRT UI'a uygulanıyor mu: doğrulanmadı.** Screen Space Overlay genellikle post-process'ten sonra çizilir, yani UI keskin kalır ve CRT'siz görünür; oyunla "tüp TV" kimliği çelişebilir. İki seçenek: (a) UI keskin kalsın (okunurluk öncelikli, bu yönün tercihi), (b) Screen Space - Camera + CRT altında. (a) önerilir, ama görsel olarak denenmeli.

---

## 8. İş yükü tahmini

Kural: akşam başına en fazla 3 task, her biri 1-1,5 saat. Pazartesi `#art`/`#test`/en fazla 2 `#level`, Cuma `#code`/`#level`. **Bunlar tahmin, `TASKS.md`'ye eklenmedi** (CLAUDE.md: önce sorulur).

| # | Akşam | Task | Kategori |
|---|---|---|---|
| 1 | Cuma 1 | Tiny5 atlasını doğrula (nokta örnekleme, Türkçe glif, tam kat boyutlar) + `UITheme` SO (palet, boyutlar) | #code |
| 2 | Cuma 1 | `MenuList`: odakta metin rengi/kayma, ilk seçim, B=geri, ekran yığını (push/pop) | #code |
| 3 | Cuma 1 | Ana menüyü aşama-1 kur (`MainMenuBuilder` güncelle, başlık + iki halka dekoru) | #code |
| 4 | Pazartesi 1 | Palet kalibrasyonu (Aseprite) + panel 9-slice + dither + pip | #art |
| 5 | Pazartesi 1 | Odak halkası (2 kare) + ananas/anahtar yerleşim ölçümü | #art |
| 6 | Cuma 2 | Pause ekranı + zaman durdurma + ayarlar (3 pip slider, ses/titreşim kaydı) | #code |
| 7 | Cuma 2 | HUD olay hattı (EventChannelSO) + ananas sayacı | #code |
| 8 | Cuma 2 | Kapı üstü anahtar pip'leri (world-space) | #code |
| 9 | Pazartesi 2 | Gamepad ikonları (8) + TMP sprite asset | #art |
| 10 | Cuma 3 | MP sonuç ekranı yeniden tasarım | #code |
| 11 | Cuma 3 | Karakter üstü dash/slingshot/stamina işaretleri | #code |
| 12 | Cuma 3 | Sprite'ları takma (aşama 2) + juice (adımlı ease, ses) | #code |
| 13 | Pazartesi 3 | 1080p/CRT açık/yalnızca gamepad test turu + kontrast kontrolü | #test |
| 14 | Cuma 4 | Tutorial/dünya yazılarını yeni stile (Tiny5 + ink gölge + ikon) geçir | #level |

**Toplam: 7 akşam** (4 Cuma + 3 Pazartesi) = yaklaşık 3,5 hafta **iyimser**. Slip payıyla (özellikle ayarlar kaydı, MP ekran ve Türkçe glif sorunu) **9-10 akşam, ~5 hafta** gerçekçi. Bu süre diğer işlerle (oda üretimi 38 oda, GDD §12) yarışıyor; UI'nın tamamı yerine **ilk üç Cuma (ana menü + pause + HUD sayacı) "yeterli" kabul edilebilir**.

---

## 9. Riskler

1. **Kimlik sorunu.** Nötr/minimal UI, "şeker dünyası" kimliğini taşımaz; menü dışında oyun kendi kimliğini dünya sanatından alır. Gelecek "uyumsuz dünyalar" (GDD §8) için nötr UI avantajdır, ama yavan görünme riski var.
2. **CRT ve UI ilişkisi doğrulanmadı** (§7.4). UI keskin kalırsa oyunla çelişebilir; CRT altına alınırsa küçük yazı bulanabilir. Erken denenmeli (task 1).
3. **Tiny5 doğrulanmayanlar:** em/glif piksel oranı, Türkçe glifler, mevcut atlas ayarları (git'te değişmiş). Yanlışsa boyut tablosu ve bazı ekranlar değişir.
4. **Gökkuşağı halka anlamının aşınması.** Menü ve sonuç ekranında halkayı çok kullanmak "kanca hedefi" anlamını bulandırabilir. Kural: yalnızca odak ve kazanan. Oyun içinde aynı halka ile karışma riski var (oyuncu menüdeki halkayı tutunulabilir sanabilir; menüde sorun değil).
5. **Çok az bilgi = kaçırılan bilgi.** Olaya bağlı HUD'da oyuncu anahtar/ananas durumunu kaçırabilir; özellikle ananas bekleyen/kalıcı ayrımı (§5.3) sezgisel değil. Playtest şart; gerekirse pause'da toplam özet satırı.
6. **Dünya ve UI piksel ızgarası uyumsuzluğu** (§7.1). Moodboard'da tutorial yazıları iki farklı font/boyutta; hepsini tek stile çekmek (task 14) dünya içi metni de yeniden yazmayı gerektirir.
7. **Yardım modu ve "Son checkpoint" kararları** (GDD §12.1) pause menüsünü etkiler; karar verilmeden eklenen madde sonradan sökülebilir.
8. **Ayarlar sistemi yok** (GDD §11); ses/titreşim kaydı kalıcı kayıt sisteminin yokluğuna çarpar. En küçük çözüm `PlayerPrefs`; bu kapsam dışı genişleme riski.
9. **Gamepad-only test eksikliği.** Fare bilinçli kapalı; EventSystem'de ilk seçili nesne kaybolursa (panel kapanınca) gamepad kilitlenir; her ekran açılışında `SetSelectedGameObject` zorunlu.
10. **Kodla sprite üretimi** küçük geometrik parçalarda iyi, ama "elle çizilmiş" sıcaklığı vermez; bu yönün bilinçli bedeli.
