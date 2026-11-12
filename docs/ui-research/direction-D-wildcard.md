# Yön D: "Wildcard / Aykırı" — SETLIST (konser afişi + kaset/plak)

Tarih: 2026-11-08. Ajan: D. Diğer yönler (CRT arcade, dünya malzemeleri, minimal) ile çakışmamak için bilerek farklı bir eksen seçildi.
Girdiler: `skills-digest.md`, `GDD.md` §1-§5 ve §7-§8, `docs/moodboard/` içinden 01, 05, 09, 13, 17, 21 (Read ile görüldü), mevcut `MainMenuBuilder.cs`, `RaceHud.cs`, `MultiplayerUISettingsSO.cs`; `unity:2d-pixel-perfect` ve `unity:ui-ugui` skill'leri yüklendi.
İşaret: **(doğrulanmadı)** = koda/Unity'ye bakmadan yazılmış varsayım, uygulamadan önce kontrol edilmeli.

---

## 0. Üç aykırı fikir ve seçim

| # | Fikir | Artı | Eksi |
|---|---|---|---|
| 1 | **SETLIST: rock konseri afişi + kaset/plak.** Menü bir gig afişi, odalar bir parça listesi (TRK 05), ölüm = "geri sar", MP sonucu = "Battle of the Bands" afişi. | Oyunun adındaki "rock" esprisi yerine oturur. Gökkuşağı halkalar zaten plak/hoparlör gibi görünüyor. Cüceler "roadie/grup" olur. Az sprite ile çok kimlik üretir. | Metin dili İngilizce olmak zorunda gibi (afiş dili); Tiny5 ile afiş tipografisi sınırlı. |
| 2 | **İP/KANCA UI.** Paneller tavandan iple sarkar, odak bir kancayla tutunur, geçişler sallanma. | Kanca sütununa (§2.2) doğrudan bağlı. | Her ekran için sarkaç animasyonu, çarpışma ve ip render'ı gerekir; gamepad menüsünde sürekli sallanan panel okunurluğu bozar; sanatçı olmayan tek geliştirici için en pahalı. |
| 3 | **KAYA UI.** Kaya/taş harfler, çatlak çerçeve, "rock" = kaya. | Tek bakışta anlaşılır espri. | Oyunun dünyasında kaya yok (zemin turuncu krema); taş dokusu pixel art'ta iş yükü ve okunurluk riski; fikir tek şakadan ibaret. |

**Seçim: 1 (SETLIST).** Neden: (a) mevcut dünyayla örtüşüyor: gökkuşağı halka = plak etiketi, düz mavi gökyüzü = sahne perdesi, pembe krema = afiş şeridi; (b) en az özgün sprite ile en çok kimlik; (c) hız/akış ve "hızlı respawn" sütunlarına metaforla hizmet ediyor (parça atlama, geri sarma); (d) ip fikrinden bir parça (odak halkası bir kancayla "tutunma" hissi) ucuz bir süs olarak alınabilir.
Seçilmeyenler: **2 (İp/kanca):** güzel ama animasyon/okunurluk maliyeti fazla, sadece odak imleci için ödünç alınır. **3 (Kaya):** tek şaka, dünyayla bağı yok; gerekirse sadece başlık logosundaki "ROCK" kelimesine taş dokusu verilir.

---

## 1. Vizyon ve neden uyar

**Tek cümle:** Oyunun UI'ı, bir rock grubunun turne evrakı gibi davranır: menü = afiş, oda listesi = setlist, ölüm = kasedeki geri sarma, MP sonucu = "Battle of the Bands".

- **İsimle oynar.** "Rock The Game" zaten "oyunu sallamak" demek. Menü afişte grup adı, odalar parça numarası. GDD §7'deki 4. duvarı yıkan, esprili NPC tonuyla aynı ağırlıkta: UI de ara sıra göz kırpar ("Encore?" yerine "Tekrar dene", bkz. metinler).
- **Dünyadan beslenir, yeni dünya icat etmez.** Mavi sahne perdesi, turuncu zemin şeridi, pembe krema şerit, gökkuşağı plak etiketi (moodboard 01, 05, 09, 13). Cüceler (moodboard 21) afişteki "grup üyeleri" olur: iki cüce = ikili grup = MP'nin iki oyuncusu.
- **Sütunlarla uyum.**
  - *Hız/akış:* tüm geçişler <= 150-200 ms; HUD yalnızca TRK numarası ve ananas sayacı; aksiyon sırasında UI sessiz.
  - *Zor ama adil:* ölüm ekranı yok. Ölünce yalnızca küçük "REW" göstergesi (kaset geri sarma ikonu) ~0.3 sn, oyun kesilmez.
  - *Hızlı respawn:* "geri sarma" metaforu respawn'ın anında olduğunu görsel olarak da söyler.
- **Gamepad only.** Odak hep plak/kanca imleci; B = geri, ok yönleri = parça seçimi; fare yok.
- **Tek dev dostu.** Afişin "karmaşası" (xerox, kesik harfler) pixel art'ta basit şekillerle yapılır: dikdörtgen bantlar, bir logo, 3 ikon. Karmaşık illüstrasyon yok.
- **CRT filtresiyle uyum.** Afiş zaten "TV'de oynanan konser" gibi okunur; CRT filtresi çelişmez, katkı yapar. (CRT'nin overlay canvas'ı kapsayıp kapsamadığı doğrulanmadı, bkz. §9.)

---

## 2. Palet

Moodboard ekran görüntülerinden gözle çıkarılmış, CRT ile renkleri kaymış değerler. **Nihai hex'ler ham sprite/atlas dosyalarından damlalıkla alınmalı (doğrulanmadı).** 24 renk, tek sabit palet, alpha yalnızca 0 veya 255, saf siyah/beyaz yok.

| Rol | Ad | Hex (tahmini) | Kullanım |
|---|---|---|---|
| Sahne perdesi (zemin) | SKY | `#7EC4EE` | Menü arka planı, panel dışı alan |
| Perde gölgesi | SKY-D | `#5A9FD0` | Perde kıvrımı, panel gölgesi |
| Perde ışığı | SKY-L | `#A6DAF5` | Spot ışığı bandı |
| Panel koyu | INK | `#2B2A4C` | Panel gövdesi, boru rengi, ana outline |
| Panel koyu 2 | INK-L | `#45456E` | Panel iç gölgeleme, disabled arka plan |
| Metin | CREAM | `#F6EBC8` | Gövde metni, plak etiketi dış halka |
| Afiş şeridi | PINK | `#F05AAA` | Başlık bandı, odak halkası, vurgu |
| Afiş şeridi koyu | PINK-D | `#B83888` | PINK gölgesi |
| Zemin | ORANGE | `#E8A06A` | Kraft kağıt/afiş altlığı, buton gövdesi |
| Zemin koyu | ORANGE-D | `#B87848` | Kraft gölgesi |
| Plak 1 | AMBER | `#F5B31A` | Gökkuşağı halka halkası, "seçili" |
| Plak 2 | VERMILION | `#F2511C` | Halka iç, uyarı, MP "ceza" |
| Plak 3 | YELLOW | `#FFE04A` | Halka merkezi, ananas, skor |
| Plak 4 | OLIVE | `#8A8A20` | Halka ara tonu |
| Plak 5 | TEAL | `#2A9A9A` | Halka, "bilgi/kanca" |
| Plak 6 | TEAL-D | `#1C6E86` | Halka dış, dış çizgi |
| Rakip | ROSE | `#D8707A` | Halka dış gölge, rakip/ghost vurgusu |
| Başarı/asit | ACID | `#27C227` | Yalnızca "hazır/bağlandı" (asit yeşilini UI'da çok kısıtlı kullan) |
| Tehlike | RED | `#E02828` | Hata, bağlantı koptu (fırlatma pedi kırmızısı) |
| Disabled | GREY | `#8E92A6` | Devre dışı metin |
| Cüce kırmızı | GNOME | `#C0242C` | Yalnızca afiş cüce illüstrasyonu ve "P2" etiketi |
| Cüce mavi | GNOME-B | `#2C5C9A` | "P1" etiketi |
| Gümüş | SILVER | `#C8CCD8` | Diken/ikon parıltısı, kaset gövdesi detayı |
| Koyu kontur | EDGE | `#171630` | Tek outline stili, 1 sanat pikseli |

Kurallar:
- **Gökkuşağı yalnızca "tutunulabilir/aktif" demektir** (digest). UI'da gökkuşağı halka = odak imleci ve logo plağı; başka yerde kullanılmaz.
- Metin: CREAM veya YELLOW, INK üstünde (değer kontrastı yüksek). INK panel üstünde CREAM kontrastı ~12:1 (hesap yaklaşık, doğrulanmadı).
- Anlam yalnızca renkle verilmez: ikon + metin ekle (REW ikonu, "P1/P2" etiketi, bayrak ikonu).

---

## 3. Tipografi

- **Tek font: Tiny5** (mevcut, TMP asset `Tiny5-Regular SDF.asset`). İkinci font eklenmez; "afiş hissi" tipografiden değil, **boyut/bant/renk oyunundan** gelir.
- **Piksel gridi:** Tiny5'in native em yüksekliği (muhtemelen 8 piksel civarı) **doğrulanmadı**. Önce bir sahnede TMP Point sampling + AA kapalı ile 4x, 6x, 8x, 12x ekran pikselinde karşılaştır, bulanık/eşitsiz olmayan çarpanları seç. Plan (varsayım): 4x gövde, 8x başlık, 12x logo.
- **Okunabilirlik eşiği:** gövde >= 26 ekran px (Xbox eşiği, digest). Tiny5 4x'te bunu geçiyorsa gövde = 4x; geçmiyorsa 5x/6x. Küçük ipucu metni (örn. tuş etiketi) >= 26 px.
- **Roller:**
  - LOGO: bitmap (pixel art, Aseprite'te çizilmiş "ROCK THE GAME"), metin değil.
  - H1 (parça başlığı): Tiny5 büyük, CREAM + EDGE 1 sanat pikseli gölge (TMP outline yerine ikinci bir kopya, 1 sanat px aşağı-sağ, INK).
  - Body: Tiny5, CREAM, sol hizalı.
  - Tag (etiket): Tiny5 küçük, büyük harf, INK üstünde PINK bant içinde.
- **Dil:** menü metinleri **İngilizce** (afiş dili, oyun içi öğreticiler zaten İngilizce: "USE L TO WALK"). Mevcut menü Türkçe ("Tek kişilik"). Tiny5'in Türkçe karakterleri (ş, ı, ğ, İ) destekleyip desteklemediği **doğrulanmadı**; İngilizce bu riski de ortadan kaldırır. Dil kararı geliştiricinin.
- Metin görsele gömülmez (logo hariç); butonlar 9-slice, metin container'a göre büyür.

---

## 4. Bileşen dili

Her şey 480x270 art gridinde, 4x ile 1920x1080. Tek outline (EDGE, 1 px), alpha 0/255, outline'ı dışarıda yarım piksel yok.

| Bileşen | Metafor | Boyut (art px) | Not |
|---|---|---|---|
| **Panel** (`ui_panel_ticket_9s`) | Bilet/afiş kartı | min 24x24, köşe 4 px | INK gövde, CREAM 1 px iç çizgi, köşelerde delik/delik izi (perfore). 9-slice. |
| **Buton** (`ui_button_track_*`) | Setlist satırı: numara + parça adı | 96x16 | Durumlar: normal (INK), focus (PINK bant + plak imleç), pressed (1 px aşağı, ORANGE), disabled (GREY). Aynı boyut/slice (focus'ta zıplama yok). |
| **Odak imleci** (`ui_cursor_vinyl`) | Mini plak/gökkuşağı halka, kancayla "tutunma" | 12x12, 4 kare | Seçili satırın solunda döner (4 kare, 8 fps). Tek renk yerine şekil + hareket. |
| **Bant (banner)** (`ui_tape_pink_9s`) | Afiş şeridi | yükseklik 12, yatay 9-slice | Başlıklar için PINK bant, metin INK/CREAM. |
| **Etiket (tag)** | P1/P2, TRK, NEW | 24x9 9-slice | PINK/ROSE/GNOME. |
| **Tuş glifi** | Gamepad: yön, A(south), B(east), Start | 12x12 | Tarafsız şekiller; marka harfi (A/B/X/Y) ve PlayStation şekilleri sonra, tema SO ile değiştirilir. |
| **İlerleme/süre** | Kaset şeridi | 80x8 | Yalnızca MP sonuç ve yarış zamanı. |
| **Kaset ikonu** (`ui_icon_cassette`) | Pause/geri sarma | 16x12 | REW, PAUSE, PLAY ikonları (3 sprite). |
| **Bayrak ikonu** | MP bayrağı | 9x9 | 3 bayrak = 3 delik/slot. |
| **Ananas ikonu** | Toplanabilir (ananas/altın) | 9x9 | Oyundaki ananasın küçük hâli, aynı palet. |

Bütün bileşenler **tek sprite sheet'te** (UI atlas); isimlendirme `ui_[object]_[variant]_[state]`.

---

## 5. Ekranlar

Wireframe birimi: 480x270 art px (4x = 1920x1080). Güvenli alan: kenardan 24 px yatay, 13 px dikey (yaklaşık %5, digest).
Not: metinler İngilizce, bkz. §3.

### 5.1 Ana menü ("Gig Poster")

Mevcut: `MainMenuBuilder` ile düz panel, 3 buton (Tek kişilik, Çok oyunculu, Çıkış), 120 pt başlık, VerticalLayoutGroup, matchWidthOrHeight 0.5.

Hedef: afiş. Arka plan SKY, üstte iki dev gökkuşağı halka (moodboard 01'deki "gözler") **plak** gibi yavaş döner, ortada logo, altında setlist.

```
+--------------------------------------------------------------+
|                                                              |
|   (   ◎ plak L  )                    (   ◎ plak R  )         |   iki halka = iki plak, yavaş döner
|                                                              |
|        [  R O C K   T H E   G A M E  ]  <- logo (bitmap)     |
|      ======== LIVE ON YOUR SCREEN ========  <- PINK bant     |
|                                                              |
|          01   > SOLO SET                    <- odak (plak)   |
|          02     TOGETHER   (2 PLAYERS)                       |
|          03     EXIT                                         |
|                                                              |
|  [A] SELECT   [B] BACK                    v0.x  (cüce x2)    |
+--------------------------------------------------------------+
```

- Menü öğeleri: 01 SOLO SET (= Tek kişilik), 02 TOGETHER (= Çok oyunculu), 03 EXIT. Metin yeniden adlandırması geliştiricinin kararı.
- İlk açılışta odak 01'de (`EventSystem.firstSelectedGameObject`, mevcut kod bunu yapıyor).
- Alt sağda iki küçük cüce (afişin "grubu"): P2 bağlıysa ikinci cüce renkli, değilse gri siluet. Bu, lobi/davet durumunu (Steam daveti) sessizce gösterir. (MainMenuUI davet bilgisi `IInviteService.HasPendingJoin` ile var; UI'a göstergeyi bağlamak ek iş, doğrulanmadı.)
- Alt şerit: tuş ipuçları (A seç, B geri).

### 5.2 Pause ("Kaset çalar")

GDD §11: pause yok (❌). Bu ekran yeni bir sistem gerektirir (Time.timeScale + input haritası), bu yüzden en riskli ekran.

```
+--------------------------------------------------------------+
|  (oyun donuk, SKY-D %50 karartma, CRT açık)                   |
|                                                              |
|            +-----------------------------+                   |
|            |  [||]  PAUSED   TRK 05      |   <- PINK bant    |
|            |-----------------------------|                   |
|            |  > RESUME                   |                   |
|            |    RESTART ROOM  (REW)      |                   |
|            |    SETTINGS                 |                   |
|            |    MAIN MENU                |                   |
|            +-----------------------------+                   |
|                                                              |
|  [A] SELECT   [B] RESUME                                     |
+--------------------------------------------------------------+
```

- "Restart room" = oyuncunun kendi seçtiği hızlı geri sarma (respawn'la aynı mekanik, bkz. §6).
- B = devam. Start = aç/kapa.
- Settings ekranı kapsam dışı (yalnızca ses ve titreşim varsayımı); ayrı iş.

### 5.3 HUD ("Parça numarası")

Mevcut: `RaceHud` MP için sağ üstte metin yığını (timer, flags, opponent), üstte ortada durum, ortada geri sayım; tek oyunculu HUD yok.

İlke: aksiyon sırasında minimum. Tek oyuncu HUD'ı **yalnızca** iki şey:

```
+--------------------------------------------------------------+
| [TRK 05]                                       [ananas] x 12 |
|                                                              |
|                       (oyun alanı)                           |
|                                                              |
+--------------------------------------------------------------+
```

- Sol üst: `TRK 05` etiketi, **yalnızca oda değiştiğinde 2 sn görünür**, sonra söner (sürekli ekranda durmaz).
- Sağ üst: ananas sayacı, yalnızca toplayınca 2 sn görünür.
- Ölümde: sol altta REW kaset ikonu ~0.3 sn (başka bir şey yok).
- **MP HUD:**
```
| [TRK 05]   0:41.3                              [P1 ⚑⚑·]     |
|                                                [P2 TRK 07 ⚑·]|
```
  Üst ortada bağlantı durumu şeridi (kopunca RED bant, "WAITING FOR P2"). Bayrak sayacı 3 delikli slot (⚑⚑·). Rakip satırı ROSE etiket, oda numarası ile.
- Her şey köşelerde, güvenli alan içinde; ortası boş.

### 5.4 MP sonuç ekranı ("Battle of the Bands")

Yarış bitişi MP 9'da kodda (`72d7e43`); mevcut sonuç metni `RaceHud._result` ile düz metin.

```
+--------------------------------------------------------------+
|        ======= BATTLE OF THE BANDS =======  (PINK bant)       |
|                                                              |
|     [ cüce P1 ]     VS      [ cüce P2 ]                      |
|     WINNER ★                                                 |
|     12:41.3                 13:02.8                          |
|     FLAGS 5                 FLAGS 3                          |
|     DEATHS 41               DEATHS 56                        |
|                                                              |
|          > ENCORE (rematch)      MAIN MENU                   |
|  [A] SELECT                                                  |
+--------------------------------------------------------------+
```

- Kazanan cüce zıplar (sprite 2 kare), kaybeden oturur. Aynı cüce sprite'ı iki tonla (GNOME / GNOME-B).
- Süre, bayrak, ölüm sayısı: ölüm sayısı mevcut veride var mı **doğrulanmadı**; yoksa satır kaldırılır.
- Birlikte modunda kazanan yok: "SESSION COMPLETE" başlığı, iki cüce yan yana, toplam süre.
- Bağlantı koparsa kayıttan devam bilgisi: "TRK 07 SAVED" etiketi (sistem `multiplayer.md`'de, doğrulanmadı).

### 5.5 Oda geçişi ("Parça değişimi")

Mevcut: `M_IrisTransition.mat` (iris geçişi) ve `RoomTransitionCoordinator`. Yeni iş yok denecek kadar az:

- Iris'in merkezi **plak deliği** gibi kapanır (kapanırken küçük bir "needle" tık sesi için ses kanalı `AudioCuePlayEventChannelSO` ile, ses işi kapsam dışı).
- Geçiş bitince 2 sn `TRK NN` etiketi (HUD'ın sol üstü).
- Süre: mevcut geçiş süresi korunur (akışı bozmamak için yeni gecikme eklenmez).
- Iris materyalinin plak şekli için shader/mask değişikliği gerektirip gerektirmediği **doğrulanmadı**; gerekirse yalnızca TRK etiketi yapılır, iris dokunulmaz.

---

## 6. Hareket / juice

DOTween ve UniTask projede var; menü animasyonları DOTween, sıralamalar UniTask. Hepsi `Time.unscaledDeltaTime` ile (pause'ta çalışmalı).

| Olay | Efekt | Süre |
|---|---|---|
| Odak değişimi | Plak imleç yeni satıra 2 art-px adımlarla kayar (tam sayı konum), satır 1 px sağa kayar | 80 ms |
| Seçme (A) | Satır 1 px aşağı (pressed), PINK flash 2 kare | 100 ms |
| Ekran açılışı | Panel yukarıdan 8 art-px kayar + görünür (ease-out, adımlı) | 150 ms |
| Ekran kapanışı | Aynı, ters | 100 ms |
| Ölüm | REW ikonu sol altta 0.3 sn; ekran **donmaz**, geçiş yok | 300 ms |
| Plaklar (menü) | 2 halka yavaş döner (yazılım dönüşü yerine 8 kare sprite animasyonu: pixel kayması olmasın) | döngü |
| Logo | Giriş: harfler tek tek (12 sanat-px) düşer, bir kez | 400 ms toplam |
| Bayrak alma (MP) | Slot dolar, kısa titreşim (mevcut `HapticManager`, yeni `*VibrationSettingsSO` alanı pattern'ine uygun) | 150 ms |
| Sonuç | Kazanan cüce 2 kare zıplama döngüsü | döngü |

Kurallar: menü geçişleri <= 200 ms (digest, çıkarım). Dönen animasyonlar kareli (sprite) olmalı; UI `Image` rotasyonu pixel bulanıklığı ve eşitsiz piksel yapar. Fare yok: imleç gizli.

---

## 7. Üretim planı

### 7.1 Boyutlar ve kurallar
- **Art grid:** 480x270, ekran 1920x1080, tam 4x. UI Canvas: Screen Space - Overlay (pixel perfect skill: overlay UI, PP Camera / upscaleRT'den bağımsız kalır; metin bulanıklığı bug'ından kaçınır), Canvas Scaler = Scale With Screen Size, 1920x1080, Match 0.5 (mevcut kurulumla aynı).
- **Sprite import:** Point (no filter), Compression None, Mip Maps kapalı, PPU = 1 art px = 4 ekran px için Canvas Scaler üzerinden (Image `Pixels Per Unit Multiplier` ile tam sayıya ayarla, doğrulanmadı).
- **Boyutlar çift sayı**, 9-slice köşe 4 px, kenar tile.
- İsimlendirme: `ui_panel_ticket_9s`, `ui_button_track_focus`, `ui_cursor_vinyl_01..04`, `ui_icon_cassette_rew`, `ui_logo_rockthegame`.
- Klasör: `Assets/UI/Setlist/` (Sprites, Atlas, Theme). **Asset manifest** (isim, boyut, border, kaynak, lisans, onay) `docs/ui-research/` altında değil, `Assets/UI/Setlist/MANIFEST.md` benzeri tek dosya; üçüncü taraf/AI içeriğin kaynağı yazılır.

### 7.2 Araçlar (dürüst değerlendirme)

| Araç | Rol | Gerçekten işe yarıyor mu? |
|---|---|---|
| **Aseprite** | Ana araç: sprite, 9-slice kaynağı, animasyon (imleç/cüce), palet kilidi, tag/spritesheet export | **Evet, bu iş için en uygun.** Ücretli (fiyat doğrulanmadı), kaynak koddan derlenebilir (lisans koşulları doğrulanmadı). Palet dosyası (.gpl/.pal) ile 24 renk kilitlenir. |
| **Piskel** (ücretsiz, web) | Yedek, küçük ikonlar | Çalışır ama palet yönetimi ve katman/tag desteği zayıf; 9-slice için zahmetli. Aseprite alınmazsa tek seçenek bu kadar. |
| **Figma** | Wireframe/düzen taslağı (480x270 frame) | Pixel art çizmek için **hayır**: vektör, AA üretir. Yalnızca yerleşim kararı için, ve "Figma'daki netlik CRT'de bozulur" notu geçerli. Figma MCP bu oturumda yetkilendirilmemiş; zaten gerekmiyor. |
| **AI görsel araçları** (genel amaçlı üreticiler) | Moodboard/afiş konsepti | **Pixel UI üretiminde güvenilmez:** gerçek piksel gridi, tutarlı palet ve temiz 9-slice vermiyor; yeniden çizim/temizleme çoğu zaman sıfırdan çizmekten uzun sürüyor. Yalnızca "afiş nasıl görünebilir" konsepti için, ve sonuçları referans olarak (kopyalamadan). |
| **Pixel-art odaklı AI araçları** (örn. Retro Diffusion, PixelLab, SpriteCook) | Taslak ikon/sprite | **Sınırlı fayda:** ikon taslağı verebilir ama palet kilitlemek ve piksel gridine oturtmak elle yapılır; hesap/kredi/fiyat ve lisans koşulları **doğrulanmadı**; SpriteCook digest'te "kurma" önerildi. Bu yönde önerilmez. |
| **Claude/kod ile üretim** | Wireframe-prefab kurulumu (Builder script), TMP ve atlas ayarı, **placeholder sprite'lar** | Layout/prefab/script işinde **evet**; çizim için **hayır** (digest: elle piksel üretimi düşük değer). Placeholder olarak düz renkli 9-slice (kodla üretilmiş) hızlıca kullanılabilir, sonra yerini sanat alır. |
| **Cüce referansı** | Mevcut sprite'ı yeniden kullan (moodboard 21) | Yeni cüce çizmek yerine sahnedeki mevcut cüce sprite'ını UI'a taşı: maliyet ~0. Sprite'ın boyutu ve palet uyumu doğrulanmadı. |

Sonuç: **Aseprite + palet dosyası + elle çizim.** AI, sadece konsept. Tek geliştirici için en gerçekçi yol: az ama tutarlı sprite (toplam ~25 sprite).

### 7.3 Export ve Unity import
1. Aseprite: tek `.aseprite` dosyası (katman=sprite, tag=animasyon), palet kilitli. Export: PNG-32, şeffaf, 1-2 px padding, tek spritesheet + animasyonlar için ayrı strip.
2. Unity: Aseprite Importer (2D Aseprite package, Unity 6'da var; projede kurulu mu **doğrulanmadı**) ya da PNG export. PNG ile gitmek daha az bağımlılık.
3. Sprite Editor: 9-slice border (1x kaynakta tam sayı art px), Image Type = Sliced.
4. Sprite Atlas (UI), Point filtre; atlas padding >= 4 (tilemap sızıntı tablosunda önerilen, UI için de güvenli).
5. **UITheme SO** (`PlayerStatsSO` desenine uygun): palet renkleri, font, sprite referansları, süreler. `MultiplayerUISettingsSO` ile birleşebilir (mevcut `Font`, `TextColor`, `WarningColor` alanları var) ya da onu kullanıp genişletir; **karar:** yeni SO yerine mevcut olanı genişlet (smallest diff).
6. Kod: mevcut `MainMenuBuilder`/`MultiplayerLobbyBuilder.Label/Button` yardımcıları temaya bağlanır; ayrı bir kütüphane yazılmaz. Ekran stack'i (push/pop, B = geri) küçük bir `UIScreenStack` sınıfı; boolean bayrak yok.

---

## 8. İş yükü tahmini

Kural: Pazartesi `#art`+`#test` (ek en fazla 2 `#level`), Cuma `#code`+`#level`; bir akşama en fazla 3 task, her biri 1-1.5 saat. Aşağıdaki liste **öneridir; `TASKS.md`'ye eklemeden önce kullanıcıya sorulmalı.** Tarih verilmedi (due yalnızca Pazartesi/Cuma olabilir, takvim durumuna göre atanır).

| Akşam | Gün | Task'lar |
|---|---|---|
| 1 | Pzt | #art 24 renk palet dosyası + moodboard'dan eyedropper doğrulama; #art hero asset: panel 9-slice; #test Tiny5 native grid + TMP Point/AA kapalı testi (4x/6x/8x, CRT açık) |
| 2 | Cuma | #code UITheme/`MultiplayerUISettingsSO` genişletme + Canvas temel kurulum (Overlay, sabit çözünürlük); #code Buton bileşeni (focus/pressed/disabled, aynı boyut) ve odak imleci scripti; #code UIScreenStack (push/pop, B geri) |
| 3 | Pzt | #art buton ailesi (4 durum) + bant + etiket; #art odak imleci plak (4 kare); #test panel+buton 1080p'de CRT açık, gamepad ile gez |
| 4 | Cuma | #code ana menüyü temaya taşı (builder); #code menü animasyonları (açılış, odak kayması); #code logo/arka plan plak döngüsü |
| 5 | Pzt | #art logo bitmap "ROCK THE GAME"; #art arka plan halkaları (plak) 8 kare; #art kaset ikonları (PAUSE, REW, PLAY) |
| 6 | Cuma | #code HUD: TRK etiketi + ananas sayacı (event güdümlü, `PlayerHealth.OnDeath` ile REW); #code oda geçişinde TRK etiketi; #code pause: input + `timeScale` + ekran |
| 7 | Pzt | #art HUD ikonları (bayrak, ananas, slot); #art cüce sprite uyarlama (P1/P2 tonları, zıplama/oturma 2 kare); #test pause/HUD gamepad turu |
| 8 | Cuma | #code MP HUD'ı temaya taşı (RaceHud); #code MP sonuç ekranı; #code SETTINGS ekranı iskeleti (ses/titreşim; kapsam belirsiz) |
| 9 | Pzt | #art sonuç afişi kompozisyonu; #test tüm akış gamepad ile baştan sona (CRT açık, 1080p); #test küçük metin okunabilirliği |

**Toplam: ~9 akşam (yaklaşık 4.5 hafta)**, bunun ~5'i art/test, ~4'ü kod. Settings ekranının asıl işlevi ve pause sistemi bu tahminin dışında bırakılabilir: pause'un input/oyun durdurma tarafı ayrı bir mekanik işi sayılabilir (+1 akşam). Gerçekçi aralık: **9-11 akşam.** Sanat süreleri bir sanatçı olmayan geliştirici için iyimser olabilir; en belirsiz kalem logo ve cüce animasyonu (doğrulanmadı). Sığmayan işler `Someday`'e (settings ekranı, oda listesi/setlist seçici) gider.

Bonus (isteğe bağlı, kapsam dışı): oda seçim ekranı = gerçek setlist ("TRK 01-22"); bu GDD'de yok, yeni özellik olarak ayrı karar.

---

## 9. Riskler

1. **CRT filtresi overlay UI'ı etkiliyor mu?** Edit Mode çekimleri overlay'i göstermiyor (CLAUDE.md); post-process/CRT materyalinin Screen Space - Overlay'e uygulanıp uygulanmadığı **doğrulanmadı**. Etkiliyorsa küçük metin ve 1 px çizgiler bozulabilir; etkilemiyorsa UI "CRT'den bağımsız" görünür ve dünyayla uyumsuz kalabilir. Erken test (akşam 1/3).
2. **Tiny5 native grid/Türkçe karakter** doğrulanmadı; ara boyutlarda bulanıklık. Çözüm: İngilizce metin + doğrulanmış çarpanlar.
3. **Aykırılık okunurluğu bozabilir.** Afiş estetiği (bant, bilet) gereğinden fazla süslenirse "hız/akış" sütunuyla çelişir; kural: süs yalnızca menü ve sonuç ekranında, HUD sade.
4. **Metafor yorgunluğu.** "Rock/konser" şakası üç ekranda ayrıntılı, geri kalanında zayıflayabilir; HUD'da yalnızca TRK/REW düzeyinde tut.
5. **Pause yok** (GDD §11): bu yön pause ekranına bağlı; sistem işi (timeScale, input, DOTween unscaled) bu yönün maliyetini artırır.
6. **Tek geliştirici sanat kapasitesi.** ~25 sprite ve 2 animasyon bile sanatçı olmayan biri için riskli; yedek: yalnızca panel/buton/imleç/logo çizilir, geri kalan düz renk bloklar ve metinle gider.
7. **Cüce kullanımı:** moodboard 21'deki cüce sprite'ının UI'da yeniden kullanılabilirliği (boyut, palet) doğrulanmadı.
8. **Gökkuşağı = tutunulabilir** kuralı (digest) imleç ve logoda bozuluyor: gökkuşağı UI'da "etkileşim" anlamı taşır, ama logo plağı dekor. Karışırsa dekor plakları monokrom (TEAL tonları) yap.
9. **Marka/lisans:** "Rock", plak ve kaset genel öğelerdir, ama hiçbir gerçek grup/ürün adı veya logo kullanılmaz; AI üretimi referanslar kaynak kaydına yazılır.
10. **Çıkarım uyarısı:** hex değerleri ve tüm boyutlar ekran görüntülerinden gözle çıkarıldı; kesin değerler asset'lerden alınmalı.
