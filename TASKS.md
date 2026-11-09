# Tasks

> **Format:** `- [ ] **Başlık** #kategori - kısa bağlam, due YYYY-MM-DD`
> **Kategoriler:** `#art` `#test` `#level` `#code`
> **Her gün 5–6 saat, bir güne en fazla 4 task.** Planlama kuralları: `CLAUDE.md` → "Proje yönetimi ve planlama".

## Active

- [ ] **MP 0: Steam'i iki bilgisayarda dene** #test - MP 9 kodda; build al, ikisinde Steam açık, lobide "Bağlantı: Steam", host Host ol → davet, diğeri kabul eder; yarış, bayrak, ceza, bitiş (docs/multiplayer-arkadasla-test.md §3a), due 2026-11-09
- [ ] **Ana menüyü gamepad ile dene** #test - odak, geri, hızlı gezinme, okunabilirlik; denge 55 uygun mu notla, due 2026-11-11
- [ ] **Pause arka planı** #code - dither karartma, iki boyalı daire ve kesişim rengi, sarı üçgen, due 2026-11-12
- [ ] **Pause'u gamepad ile dene** #test - seçim hissi, çıkış, süre durması (oyuncu/hazard/ses), okunabilirlik notu, due 2026-11-12
- [ ] **HUD: çeyrek daire sayaç** #code - ananas + anahtar sayacı, `CollectibleUIController`'a bağla; kalıcı mı olay güdümlü mü kararını ver, due 2026-11-12
- [ ] **Kırılan stamina halkası** #code - karakter etrafında 6 beton parça, enerji azalınca kopar; mevcut `StaminaCanvas`'ın yerine ya da üstüne; Someday'deki "Dayanıklılık uyarısı ekle" ile aynı iş, due 2026-11-12
- [ ] **HUD'ı gamepad ile dene** #test - hızlı bir odada sayaç ve halka fark ediliyor mu, dikkat dağıtıyor mu, due 2026-11-13
- [ ] **Oda başlık kartı** #code - çapraz levha, ekstrüde oda numarası, sarı kiriş; ~1 sn, oyunu durdurmaz (DOTween), due 2026-11-13
- [ ] **Çok oyunculu sonuç ekranını yeniden çiz** #code - silindir (1. oyuncu) ve küp (2. oyuncu), yükseklik sıralama; mevcut ekranı bozmadan (docs/systems/multiplayer.md), due 2026-11-13
- [ ] **Oda kartı ile iris geçişi çakışmasını dene** #test - `RoomTransitionCoordinator`, iris geçiş ve kart aynı anda; sıra ve süre ayarı, due 2026-11-13
- [ ] **Tüm UI ekranlarını gamepad ile dene, dengeyi ayarla** #test - beş ekranın denge değeri 55 uygun mu, ekran başına değiştir (`UiThemeSO`), due 2026-11-14
- [ ] **Bayrakları yerleştir** #level - 22 odada `RaceFlagPoint`'leri istediğin yere sürükle (sarı bayrak gizmo'su); oda 21 ve 22'ninkiler oda ortasında havada, mutlaka taşı; oda 21'deki bitiş işaretini (dama bayrağı gizmo'su, `RaceFinishPoint`) de bitiş yerine taşı, due 2026-12-07

## Waiting On

## Someday

- [ ] **Player build al ve doğrula** #test - RockTheGame.Runtime'daki EasyTextEffects.Editor importu build'de patlıyor mu (6 Kasım'da multiplayer için ertelendi)
- [ ] **Fixed Timestep 60 Hz denemesi** #code - 0.02 → 0.01667 dene, bir seviye oyna, beğenmezsen geri al (kalan-isler §1.2) (6 Kasım'da multiplayer için ertelendi)
- [ ] **DontDestroyOnLoad uyarısını düzelt** #code - Play Mode'da "only works for root GameObjects" çıkıyor; aday AudioManager.cs:72 ya da HapticManager.cs:26, kaynağı doğrula ve düzelt (6 Kasım'da multiplayer için ertelendi)
- [ ] **Dayanıklılık uyarısı ekle** #code - GDD §5.2 son saniyelerde uyarı istiyor, kodda yok; çubuk mu titreşim mi seç, sonra uygula (docs/systems/wall-movement.md) (6 Kasım'da multiplayer için ertelendi)
- [ ] **Oda 16 kamera uzaklığı** #level - ortho 22'de zemindeki dikenler noktalı çizgi gibi, oyuncu çok küçük; oda kamera ayarını yakınlaştır ya da dikenleri büyüt (docs/systems/hazards.md) (6 Kasım'da multiplayer için ertelendi)
- [ ] **Ezici bloklar node'ları örtüyor** #level - oda 16'da beyaz `MovingDeath` blokları node'ların önünden geçerken onları tamamen gizliyor; ikisi de Default/0, node'ları öne al (docs/systems/hazards.md) (6 Kasım'da multiplayer için ertelendi)
- [ ] **PPU standardı kararı** #art - 5 Kasım'da ertelendi; ölçüm kalan-isler §1.4'te (zemin/diken/boru 16, oyuncu 24/32, büyütülmüş meyve/trambolin 5–14)
- [ ] **Pixel Perfect Camera denemesi** #art - PPU kararından sonra (kalan-isler §1.5)
- [ ] **GDD §8 "yeşil, pis asit" ile görsel uyumu** #art - metin mi sanat mı güncellenecek
- [ ] **Beyaz dikdörtgen ve daireler** #art - odalar 2, 9, 12, 16, 19, 22: bunlar tehlikeler (`MovingDeath` ezicileri, `RotatingDeath` daireleri), Unity'nin varsayılan kare/dairesiyle çizilmiş; sanat mı yapılacak, bilerek mi
- [ ] **GDD §12 karar oturumu** #test - assist modu, hamster topu, multiplayer, toplanabilirlerin amacı, zorluk eğrisi
- [ ] **Telifli müzikleri değiştir** #art - örn. "The Skeleton Dance"; yayın öncesi
- [ ] **PhysicsTest sahneleri ve M_CRTFilter.mat gürültüsü** #code - takipsiz sahneleri commit'le ya da sil; `_CrtTime` izini kes
- [ ] **Karakter: Egg** #art - yumurta gözlü yeni karakter
- [ ] **Karakter: Dondurma adam** #art - sarı renkli dondurma; fabrikadaki kırmızımsı yeni dondurmalar gibi olmak istiyor
- [ ] **Hikâye seviyesi: Dondurma adamla tanışma** #level - birkaç diyalog duyuyoruz; hikâye seviye içinde anlatılır, oyuncuya zorla verilmez
- [ ] **Hikâye seviyesi: Conveyor belt ve eriyen dondurma** #level - dondurma adam belt üstünde zıplayarak yumurtaya ulaşmaya çalışır, ateş üfleyenler yüzünden yerinde sayar ve erir; o erimeden seviyeyi bitir, erirse oyuncu ölmüş gibi seviye restart olur
- [ ] **Hikâye seviyesi: Missing kağıtları** #level - X-Men Days of Future Past kapağındaki gibi arka planı missing kağıtlarıyla dolu seviye
- [ ] **Hikâye seviyesi: Dondurma fabrikası** #level - dondurma adam üretildiği fabrikada; yukarıdan yeni dondurmalar kapaklarından tutularak geçer (Monsters Inc. kapı sistemi gibi), onlar gibi olmak istediğini söyler
- [ ] **Hikâye seviyesi: Yaşanmamış hayatlar** #level - bir iki odada Egg ve dondurma adamın La La Land finalindeki gibi yaşamadıkları hayatlar
- [ ] **End credits: Dondurma adam ve fotoğraflar** #art - dondurma adam yumurtanın fotoğraflarına kendi şeklini kesiyor
- [ ] **Hikâye seviyesi: Kayıp kedi Astro** #level - bir karakter kaçan kedisini bulmamızı ister; Astro mahsur kaldığı yerden kurtarılır ya da bulunur, yanında hep arkadaş kediler olur
- [ ] **Seviye: Reels kaydırma** #level - oyuncu reels bitmeden seviyeyi tamamlar, yoksa sayfa kayarken oyuncuyu öldürür
- [ ] **Seviye: Reklam içeren seviye** #level - ayrıntısı netleşecek
- [ ] **Spiral node'lar** #art - mekanik yükü az, shader üretmek gerekiyor
- [ ] **Swing tutorial iyileştirmesi** #code - oyuncu node'a doğru zıplatılır, havada zaman yavaşlar; önce kolu hareket ettirmeyi, sonra ateşlemeyi öğrenir
- [ ] **İleri zıplama animasyonu** #art - şu anki animasyon yerinde zıplama gibi duruyor
- [ ] **Bounce pad asset'leri** #art - kodda `LaunchPad` olan pedler için görsel üretimi
- [ ] **Yalnızca odayı gösterme araştırması** #code - Celeste gibi; bazı seviyelerde yan oda görünmesin, gösterilmek istenmeyen yerler karanlık kalsın (komşu odalar şu an aktif kalıyor)
- [ ] **Hareketli node'lar** #code - hareketli platform gibi oyuncuyu taşıyan, fırlatan node'lar (anchor'lar şu an sabit `Vector2?`, node'u takip etmesi gerekir)
- [ ] **Teleport platformlar** #code - DK Bananza'daki gibi; platform parçalarına dokunup onları örneğin alevli kapının üstüne ışınlayarak barikat kurmak

## Done

- [x] ~~**Radyal pause: dilim seçimi** #code~~ (2026-11-09) - iki gün erken; `PauseMenuPanel` + `RadialMenuSelection`: sol stick (`atan2`) ve D-pad dilimi seçer, LB/RB döndürür, A onaylar, B devam; Options soluk ve atlanıyor (ayar ekranı yok), Retry = `PlayerHealth.Kill()` (son checkpoint), Quit = ana menü; giriş animasyonu yeniden yazıldı, kullanıcı beğendi (docs/systems/pause.md). Play Mode'da sanal gamepad ile denendi, gerçek gamepad hissi denenmedi, boşta hareket/tık sesi/titreşim yok. EditMode 230/230
- [x] ~~**HUD ikon seti** #art~~ (2026-11-09) - iki gün erken; sekiz 24x24 piksel ikon (ananas, anahtar, dash küpü, sling silindiri; boyalı = renk, `_used` = düz beton grisi), brutalist: ink çizgi, sert gölge, yüzler arası kenar; SVG yerine piksel seçildi, `Assets/UI/Icons/` (docs/ui-research/hud-icons.md). Anahtar mockup'ta disk sırası, burada silüet: karar açık. Oyunda ve gamepad mesafesinde okunurluk denenmedi, HUD'a bağlanmadı
- [x] ~~**Ana menüyü yeniden kur** #code~~ (2026-11-09) - iki gün erken; mockup'taki menü: eğik ekstrüde başlık, liste, sarı levha + halka imleci, mavi boyalı halkalı delik (`MainMenuBuilder`, `UiExtrudedText`, `UiSelectionVisual`). `options` etkileşimsiz (ayar ekranı yok), gezinmede atlanıyor. Play Mode'da görüldü, gerçek gamepad denenmedi (docs/ui-research/main-menu.md). EditMode 215/215
- [x] ~~**Overlay canvas CRT dışında mı doğrula** #test~~ (2026-11-08) - iki gün erken; Overlay CRT dışında (test paneli tam `#FC1E00`, 1 renk), Coin UI (Camera canvas) ve StaminaCanvas (World Space) CRT içinde. Ana menü düz ama sahnede `Volume` yok, tek başına kanıt değil. Lobi ve RaceHud yakalanmadı (docs/ui-research/crt-check.md, görüntüler crt-check/). Çıkarım: sayaç ve stamina task'larında Camera/World Space'ten Overlay'e taşıma kararı verilecek
- [x] ~~**Beton yüzey shader'ı ya da dokusu** #art~~ (2026-11-08) - iki gün erken; seçilen: (a) kodla üretilen uGUI shader'ı (`RockTheGame/UI/Concrete` + `UiConcreteSurface`; benek, kalıp çizgisi, tie-hole denge eşikleriyle; uGUI Canvas hedefi doğrulandı: UV'siz `UiArc`'ta da çalışıyor). (b) CC0 doku denenmedi, kullanıcı indirmeyi istemedi; (a) ölçülerek değil gerekçeyle seçildi (docs/ui-research/concrete-surface.md). 1080p'de gözle bakılmadı, hiçbir ekran henüz kullanmıyor. EditMode 213/213
- [x] ~~**UI fontlarını hazırla** #art~~ (2026-11-08) - iki gün erken; Jost 800 (başlık) ve Space Mono Regular/Bold (mono), ikisi de OFL 1.1, TMP SDF statik atlas (205 karakter, Türkçe dahil), `UiThemeSO`'ya atandı; Tiny5'e dokunulmadı (docs/ui-research/fonts.md). Play Mode'da Türkçe harfler çizildi, küçük metin 1080p'de gözle bakılmadı. OFL metinleri `Assets/UI/Fonts/Licenses/`; yayın öncesi krediye eklenmeli. EditMode 210/210
- [x] ~~**UiThemeSO** #code~~ (2026-11-08) - iki gün erken; palet, beton tonları, fontlar, çerçeve, gölge, ekstrüzyon ve denge (0.55, ekran başına override) `Resources/UI/UiTheme.asset`'te, mockup formülleri `UiBalance`'ta (docs/ui-research/palette.md "Kodda"). Fontlar boş (font task'ı atayacak), henüz hiçbir ekran okumuyor. EditMode 209/209
- [x] ~~**UI paletini kaynak PNG'lerden örnekle** #art~~ (2026-11-08) - bir gün erken; kâğıt `#F1E6C7`, kırmızı `#FC1E00`, sarı `#FCAE04`, mavi `#597DCE`, ink `#140C1C` kaynak piksellerden (boru `#433646` hiçbir sprite'ta yok, `#442434`/`#4E4A4E`), docs/ui-research/palette.md. Beton tonları kaynakta yok: karar sende (mockup'unki / türetilmiş / soğuk taş). Mockup güncellenmedi
- [x] ~~**UI şekil çizim yöntemini seç** #code~~ (2026-11-08) - bir gün erken; karar: statik şekiller PNG 2x, dinamik halka/dilim/çokgen için kendi `UiArc` mesh bileşeni (hazır paketlerin hiçbiri Unity 6'da doğrulanmadı, lisans/atıf yükü yok), docs/ui-research/shape-method.md. Play Mode'da şekiller çizildi; kenar yumuşatma 1080p'de gözle doğrulanmadı, UV yok (beton shader task'ı için not). EditMode 201/201
- [x] ~~**Pause sistemini yaz** #code~~ (2026-11-08) - bir gün erken; Start duraklatır/devam ettirir (`PauseManager`, `timeScale 0`, ses ve titreşim kesilir), çok oyunculu yarışta kapalı, müzik çalmaya devam ediyor ve boğuk/uzak bir efekt alıyor (Lowpass + Reverb, `PauseAudioSettings.asset`) (docs/systems/pause.md). Sanal gamepad ile Play Mode'da doğrulandı; gerçek gamepad, his ve efektin kulakla ayarı denenmedi (Pause'u gamepad ile dene task'ı). EditMode 194/194, PlayMode 41/41
- [x] ~~**MP 9: Steam taşıyıcısı** #code~~ (2026-11-07) - Facepunch.Steamworks 2.5.2 ayrı assembly'de, Steam relay taşıyıcısı, lobide Steam daveti; Steam yoksa IP'ye düşüyor (build'de doğrulandı). Ayrıca yarış bitişi ve sonuç ekranı (iki pencerede doğrulandı). Steam bağlantısı iki bilgisayarda denenmedi (MP 0). EditMode 189/189, PlayMode 41/41
- [x] ~~**MP 8: Yarış kaydı, devam etme, kopma** #code~~ (2026-11-06) - yedi hafta erken; `multiplayer-race.json`, lobide Devam et (`Hello.Resume`, protokol v3), kopunca donma + 30 sn yeniden bağlanma + 3-2-1, dönülmezse kayıt ve lobi. İki pencerede kopma → bekleme → lobi → Devam et ile kendi odalarından sürdürme doğrulandı; 30 sn içinde geri dönüş elle denenemedi (Player 2'nin Play'e girişi uzun sürdü). EditMode 178/178, PlayMode 41/41
- [x] ~~**MP 7: Lobi sahnesi ve multiplayer HUD** #code~~ (2026-11-06) - altı hafta erken; `MultiplayerLobby.unity` (build index 1), host/katıl, modu host seçer, 3-2-1 geri sayım (`Ready`, protokol v2), HUD; `MultiplayerSession` başlatıcının yerine geçti. İki pencerede lobiden yarışa ve istemcinin 3 bayrağıyla host'a ceza ağ üzerinden doğrulandı (MP 5–6'nın iki pencere kontrolü de); başlama farkı editörde 0.4 sn. EditMode 164/164, PlayMode 41/41
- [x] ~~**MP 6: Ceza akışı** #code~~ (2026-11-06) - beş hafta erken; `PenaltyHandler` respawn dizisiyle önceki odanın checkpoint'ine götürüyor, kamera ve oda geçiyor, sonraki ölüm orada doğuyor; oda 22 → 11; respawn/geçiş/ölüm sırasında bekliyor. EditMode 151/151, PlayMode 41/41 (5 yeni ceza testi); Play'de tek pencerede doğrulandı, iki pencerede ağ üzerinden denenmedi
- [x] ~~**MP 5: Bayrak işaretleri** #code~~ (2026-11-06) - dört hafta erken; `RoomNew-22` ID 22, 22 odaya `RaceFlagPoint` kondu (menü komutu, yatay çıkışta zemine iner), yarış modunda dokununca kalkan bayrak; tek pencerede gerçek trigger'la doğrulandı (sayaç 1, 2, 0), iki pencerede denenmedi; EditMode 144/144, PlayMode 36/36
- [x] ~~**MP 4: Hayalet** #code~~ (2026-11-06) - üç hafta erken; iki pencerede rakip yarı saydam hayalet olarak görünüyor, takip ediyor, uzak odada gizleniyor (ajan iki pencereyi CLI ile sürerek doğruladı); EditMode 120/120, PlayMode 36/36. Gamepad iki pencerede hâlâ denenmedi
- [x] ~~**MP 3: Unity Transport taşıyıcısı** #code~~ (2026-11-06) - iki hafta erken; UTP adaptörü (gerçek localhost testleriyle) ve editör için geliştirici başlatıcısı; EditMode 100/100, PlayMode 36/36; MP 1–3 bir akşamda bitti, NGO'ya geçiş koşulu oluşmadı. Gamepad'in iki pencerede davranışı denenmedi
- [x] ~~**MP 2: Yarış kuralları (`RaceSession`)** #code~~ (2026-11-06) - planlanandan bir hafta erken; gözden geçirmeden sonra ceza hedefi uygulama anında hesaplanıyor, ceza hep gönderiliyor, rakip sayacı 3'te sıfırlanıyor; EditMode 84/84
- [x] ~~**MP 1: Assembly'ler, taşıyıcı arayüzü, bellek içi taşıyıcı, mesajlar** #code~~ (2026-11-06) - `feature/multiplayer` dalında 4 commit; EditMode 60/60, PlayMode 36/36; gözden geçirmenin 6 küçük bulgusu ertelendi (docs/systems/multiplayer.md)
- [x] ~~**Multiplayer mod kararı ve ağ yaklaşımı** #code~~ (2026-11-06) - hayalet rakip, yarış + birlikte, Steam; kendi taşıyıcı arayüzü (docs/systems/multiplayer.md)
- [x] ~~**Kullanılmayan asset klasörleri** #art~~ (2026-11-06) - karar: silinmeyecek, ileride işe yarayabilir (Pixel Adventure 1, Tiles/Zone-3, Tiles/Background, Sprites/)
- [x] ~~**Oda 9 dikenleri + Play Mode görselleri** #test~~ (2026-11-06) - oda 9 dikenleri oyun kamerasında okunuyor; oyuncu ve HUD doğru katmanda (bulgular docs/systems/hazards.md)
- [x] ~~**Renk uzayı kararı (Gamma/Linear)** #art~~ (2026-11-06) - karar: Gamma kalıyor; 2D ışık eklenirse yeniden değerlendirilir
- [x] ~~**Sorting Layer 2: arka plan, VFX, dekor** #art~~ (2026-11-06) - asit parçacıkları VFX'e, dekor (gnome, iskelet, duş borusu, yatan adam) Background'a; tehlikeler, node'lar, yazılar Default'ta kaldı
- [x] ~~**Sorting Layer 1: katmanları tanımla, oyuncu ve platformlar** #level~~ (2026-11-06) - Background → Default → Platform → Player → VFX → UI; oyun içinde gözle kontrol bekliyor
- [x] ~~**"Bozulmamalı" davranışlar listesini yaz** #test~~ (2026-11-05) - `docs/bozulmamali-davranislar.md`; testler henüz yazılmadı
- [x] ~~**Kullanılmayan modül paketlerini kaldır** #code~~ (2026-11-05) - 8 modül kaldırıldı; `terrain` kaldı, çünkü `render-pipelines.core` ona bağlı
- [x] ~~**Animator 'Hash 0' uyarısını teşhis et** #test~~ (2026-11-05) - `GnomePukeAnimator` hash'i `OnEnable`'dan sonra hesaplıyordu; `Awake`'e alındı, uyarı kalktı
- [x] ~~**Ses mixer parametrelerini expose et** #test~~ (2026-11-05) - dört parametre expose edildi; boş `Ambient` grubu eklendi, henüz hiçbir ses ona bağlı değil
- [x] ~~**Ping-pong stratejilerinde period=0 NaN düzeltmesi** #code~~ (2026-11-05) - `period <= 0` iken başlangıç noktasında kalır, EditMode testleri eklendi
- [x] ~~**Git geçmişindeki şirket içi doküman izine karar ver** #code~~ (2026-11-05) - karar: geçmiş olduğu gibi kalıyor, yeniden yazılmayacak
