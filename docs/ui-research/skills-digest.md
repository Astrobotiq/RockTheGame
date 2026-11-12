# Community skill özeti: oyun UI / art direction

Tarih: 2026-11-08. Kaynaklar yalnızca metin olarak okundu (curl ile raw SKILL.md/README). Hiçbir skill kurulmadı, hiçbir script çalıştırılmadı.
Not: Kuralların bir kısmı skill'lerden, bir kısmı bu projeye uyarlanmış çıkarımdır; "(çıkarım)" ile işaretli.

## A) Skill'ler

### 1. game-art (davila7/claude-code-templates) - Değer: 2/5
- URL: https://github.com/davila7/claude-code-templates/blob/main/cli-tool/components/skills/creative-design/game-development/game-art/SKILL.md
- Sağlıyor: stil karar ağacı, stil karşılaştırma tablosu, 2D asset pipeline (Aseprite/TexturePacker), palet stratejileri, animasyon kare sayıları, isimlendirme `[type]_[object]_[variant]_[state]`.
- Pixel art referansı: taban çözünürlük 320x180 - 640x360, karakter 16-32 px. "1x çalış, yukarı ölçekle, asla aşağı değil."
- Güvenlik: yalnızca Read/Glob/Grep. Script/ağ/hesap yok. Güvenli, ücretsiz.
- Yorum: genel ve yüzeysel, UI'a özel bir şey yok. Anti-pattern tablosu iyi (stil karıştırma, sadece final çözünürlükte çalışma).

### 2. team-ui + design-system (Donchitos/Claude-Code-Game-Studios) - Değer: 2/5
- URL: https://github.com/Donchitos/Claude-Code-Game-Studios/blob/main/.claude/skills/team-ui/SKILL.md (ve .../design-system/SKILL.md)
- Sağlıyor: çok ajanlı UX hattı: UX spec (ux-spec / hud-design şablonları) -> art-director görsel tasarım -> uygulama -> UX/art/erişilebilirlik review -> polish. design-system ise GDD bölümü yazım iskeleti.
- Güvenlik: Bash çalıştıran hook (`yaml-helper.sh`), Agent spawn, Write/Edit izinleri, o repoya özgü `.claude/hooks` ve `.claude/docs` bağımlılığı. Tek başına çalışmaz, bu repoya kurulmamalı. Hesap/ücret yok.
- Yorum: kurulacak bir şey değil; sadece fikir: UI işini "spec -> görsel -> kod -> review" diye ayır, spec'i koddan önce yaz. Tek geliştirici, haftada 2 akşam için fazla ağır.

### 3a. game-ui-ux (gamedev-skills/awesome-gamedev-agent-skills) - Değer: 4/5
- URL: https://github.com/gamedev-skills/awesome-gamedev-agent-skills/blob/main/skills/disciplines/game-ui-ux/SKILL.md
- Sağlıyor: anchor + container, referans çözünürlük ölçekleme, safe area, gamepad focus, ekran stack'i (push/pop), event-driven HUD. Unity 6.3 uGUI karşılıkları var (CanvasScaler, Screen.safeArea, InputSystemUIInputModule).
- Güvenlik: saf metin rehber, script yok. Güvenli, ücretsiz.
- Yorum: en kullanışlısı. Mimarimizle (EventChannelSO) örtüşüyor.

### 3b. create-game-assets (aynı repo) - Değer: 3/5
- URL: https://github.com/gamedev-skills/awesome-gamedev-agent-skills/blob/main/skills/disciplines/create-game-assets/SKILL.md
- Sağlıyor: önce mevcut sanatı incele, teknik çerçeveyi kilitle (boyut, palet, filtreleme), asset manifest, tek "hero asset" onayla sonra aile üret, deterministik normalize et, native çözünürlükte doğrula, provenance kaydet. Prompt blokları (ROLE/VIEW/ART DIRECTION/LOCKS/EXCLUDE).
- Güvenlik: paketle gelen `asset_report.py` ve `build_preview_sheet.py` Python/Pillow scriptleri var (okunmadı, çalıştırılmadı; kullanmadan önce incelenmeli). Metinde ağ erişimi yok. Üretim için harici `imagegen` skill'i varsayıyor (o hesap/ücret isteyebilir). Skill kendisi ücretsiz.
- Yorum: manifest + "hero asset önce" disiplini değerli.

### 4. pixel-art-studio (Gamezxz) - Değer: 2/5
- URL: https://github.com/Gamezxz/pixel-art-studio (SKILL.md)
- Sağlıyor: Claude'un Python/Pillow ile piksel piksel çizdiği sprite/tileset/animasyon; hue-shifted ramp, dither, selout, palet kilitleme, spritesheet+JSON export, bozuk/AI üretimi PNG temizleme (`pixelpipe.py`).
- Güvenlik: `scripts/*.py` çalıştırmayı ve `pip install pillow` yapmayı ister; kod çalıştırır. Ağ/hesap/ücret yok. Kurulmadan önce scriptler okunmalı.
- Yorum: UI için değil sprite üretimi için. İlginç parça: "çiz -> bak -> eleştir -> düzelt" döngüsü ve palet kilitleme. Sanat elle üretildiği için düşük değer.

### 5a. game-ui-design (omer-metin/skills-for-antigravity) - Değer: 2/5
- URL: https://github.com/omer-metin/skills-for-antigravity/blob/main/skills/game-ui-design/SKILL.md
- Sağlıyor: persona + ilkeler (controller-first, safe zone, animasyon iletişimdir, her eleman yerini hak etmeli, CRT filtresinde Figma'daki netlik bozulur). Somut içerik `references/*.md` içinde; bu çalışmada yalnızca SKILL.md okundu.
- Güvenlik: sadece markdown (Vibeship Spawner Skills'ten AI ile dönüştürülmüş, Apache 2.0). Güvenli.

### 5b. pixel-art (aynı repo) - Değer: 2/5
- URL: https://github.com/omer-metin/skills-for-antigravity/blob/main/skills/pixel-art/SKILL.md
- Sağlıyor: ilkeler: sınırlı palet, anti-aliasing genelde hata, 1x'te okunabilirlik, daha az renk/kare daha çok etki, farklı sprite boyutlarını karıştırma, tile dikişleri. Ayrıntılar references'ta (okunmadı).
- Güvenlik: sadece markdown, güvenli.

### 6. SpriteCook skills - Değer: 1/5
- URL: https://github.com/spritecook/skills
- Ne yapıyor: SpriteCook (spritecook.ai) AI asset üretim servisinin MCP araçlarını kullanma rehberleri: sprite üretimi, polish, UI kit (9-slice metadata dahil), animasyon, Godot export.
- Gereksinim: SpriteCook MCP bağlantısı (`npx spritecook-mcp setup`) ve kredi ("check credits" adımı var): hesap ve büyük olasılıkla ücret gerekir. Fiyat doğrulanmadı. Godot odaklı, Unity yok. Skill'ler MIT.
- Öneri: kurma. Harici servis ve ağ bağımlılığı getiriyor.

## B) Damıtılmış kurallar (bu proje için)

Hiçbir skill Unity pixel-art UI'ı uçtan uca çözmüyor; en çok `game-ui-ux` + `create-game-assets` işe yarar. Kurulu `unity:2d-pixel-perfect` ve `unity:ui-ugui` skill'leri Unity tarafını kapsar.

### Referans çözünürlük ve ölçekleme
- Çıktı 1920x1080. UI sanatını 480x270 gridinde (tam 4x) çiz; alternatif 640x360 (3x). (çıkarım; game-art 320x180-640x360 aralığını veriyor)
- 1 sanat pikseli = sabit sayıda ekran pikseli; UI'da farklı piksel yoğunluğu ve farklı sprite boyutu karıştırma.
- Integer scaling: 1x'te çiz, yalnızca tam sayıyla büyüt, asla aşağı ölçekleme, kesirli ölçek yok. 16:9 dışı ekranda letterbox/pillarbox ya da HUD'ı köşelere anchor'la.
- Unity: Canvas Scaler = Scale With Screen Size, Reference 1920x1080, Match 0.5 (HUD yüksekliğe bağlıysa 1.0). Sprite import: Filter Point (no filter), Compression None, Mip Maps kapalı.
- Screen Space Overlay'de yarım piksel konumdan kaçın (anchoredPosition tam sayı katı).

### 9-slice
- Panel/buton/diyalog çerçeveleri 9-slice: köşeler sabit (ör. 4-6 sanat pikseli), kenarlar tile, Image Type = Sliced. Stretch değil, tile.
- Border'ı Sprite Editor'da 1x kaynakta, tam sayı sanat pikseli olarak ayarla.
- Buton durumları (normal/focus/pressed/disabled) aynı sprite boyutu ve slice sınırını paylaşsın, yoksa focus'ta zıplar.
- Tek bir panel ve tek bir buton ailesi çiz (hero asset), gerisini ondan türet.

### Gamepad focus navigasyonu
- Her ekran açılırken bir kontrol seçili olmalı (`EventSystem.SetSelectedGameObject`); aksi halde gamepad hiçbir şey yapmaz.
- Navigation Explicit; grid, wrap ve disabled öğe atlamayı elle ayarla, Automatic yalnızca basit listede.
- Input System kullanılıyorsa `InputSystemUIInputModule` (`StandaloneInputModule` değil).
- Focus göstergesi yalnızca renk olmasın: ok imleci, kalın çerçeve ya da zıplayan ikon. Gökkuşağı halka diline uygun bir odak halkası düşünülebilir. (çıkarım)
- Ekranlar stack: push/pop, B = geri, boolean bayrak yok.
- Sadece gamepad (bilinçli karar): fare desteği ekleme, imleci gizle, klavye artıklarını UI'dan temizle.
- Test: fareyi bırak, baştan sona yalnızca gamepad ile gez.

### Safe area
- Kritik UI'ı kenardan içeri al: yaklaşık %5 (1920x1080'de ~96x54 px, sanat pikseli ~24x13). (çıkarım; skill sadece "inset" diyor)
- `Screen.safeArea` ile bir SafeArea paneli; PC'de değer ~0 olacağı için elle %3-5 margin de ver.
- CRT/scanline filtresi kenarları etkiliyorsa köşelere kritik bilgi koyma. (çıkarım)

### Font boyutları
- Eşik (game-ui-ux, Xbox Accessibility Guideline 101): 1080p'de konsol/TV >= 26 px, PC >= 18 px. Gamepad/TV kullanımı olası, hedef gövde metni >= 26 ekran pikseli.
- Tiny5 yalnızca fontun native piksel gridinin tam katlarında render edilsin (4x/8x gibi); ara boyutlar bulanıklık ve eşitsiz piksel yapar. Native boyutu doğrula. (çıkarım)
- TMP atlas: Point sampling, anti-aliasing yok; CRT filtresi AÇIKKEN küçük metni test et.
- Metin container'a göre büyüsün (sabit buton genişliği taşmasın); metin resme gömülmesin.

### Palet kısıtları
- Tek sabit palet, tüm UI asset'leri ondan; UI için ~16-32 renk (pixel-art skill'i 16 renk savunuyor).
- Mevcut dilden rol çıkar: açık mavi arka plan, turuncu zemin, pembe/krem vurgu, gökkuşağı = grapple node, kırmızı = NPC. UI: koyu nötr panel + krem metin (ton farkı değil değer kontrastı), tek vurgu rengi. Gökkuşağını yalnızca "grapple hedefi/etkileşim" anlamına ayır. (çıkarım)
- Hue-shifted ramp (gölge maviye/mora, ışık sarıya), saf siyah/beyaz yok.
- Anti-aliasing ve yarı saydam kenar yok (halo yapar); alpha yalnızca 0 veya 255.
- Anlam yalnızca renkle verilmesin (ikon + şekil + metin). Tek bir outline stili.

### Asset pipeline
- Üret: 1x sanat boyutunda (Aseprite/Krita), ölçekleme yalnızca import/export'ta tam sayıyla. Büyütülmüş PNG'yi asla kaynak yapma.
- Boyutlar çift sayı (pivot/merkezleme), 4x'te tam ekran pikseline düşer.
- Export: PNG-32, şeffaf arka plan, 1-2 px padding. Tile'lar 3x3 tekrar testiyle dikişsiz doğrulanır. Sprite Atlas, Point filtre.
- İsimlendirme: `[type]_[object]_[variant]_[state]`, örn. `ui_button_primary_focus`, `ui_panel_dialog_9s`.
- Asset manifest (isim, boyut, pivot, 9-slice border, kaynak, lisans, onay). Üçüncü taraf/AI üretimi her şeyin kaynağı ve lisansı kayıtlı.
- Doğrula: 1920x1080'de, CRT filtresi açık, gerçek arka plan üstünde; nearest-neighbor contact sheet ile aile tutarlılığı.
- AI üretimi yalnızca taslak; ızgara, palet ve kümeleri piksel araçlarıyla elle zorla.

### HUD ve akış
- HUD olay güdümlü: EventChannelSO / `PlayerHealth.OnDeath` aboneliği, `Update`'te polling yok (mevcut mimariyle uyumlu).
- Animasyon iletişimdir: menü geçişleri kısa (<= ~200 ms, çıkarım), aksiyon sırasında HUD minimum.

## C) Öneri
- Kurulacak skill yok. `game-ui-ux` ve `create-game-assets` fikirleri bu dosyaya aktarıldı; gerekirse kısa bir ui-style-guide yazılabilir.
- Kurma/çalıştırma: SpriteCook (hesap/kredi, harici servis), team-ui (hook + ajan hattı), pixel-art-studio (Python çalıştırır; önce scriptler okunmalı).
- Okunmayanlar: omer-metin `references/` dosyaları, create-game-assets `references/` ve scriptleri.
