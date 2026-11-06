# Tasks

> **Format:** `- [ ] **Başlık** #kategori - kısa bağlam, due YYYY-MM-DD`
> **Kategoriler:** `#art` `#test` `#level` `#code`
> **Due yalnızca Pazartesi veya Cuma olabilir.** Planlama kuralları: `CLAUDE.md` → "Proje yönetimi ve planlama".

## Active

- [ ] **MP 0: Steam duman testi** #test - Facepunch.Steamworks 2.5.2 ayrı asmdef'te, `SteamClient.Init(480)`, iki bilgisayar arasında bağlantı; iki PC ve iki Steam hesabı gerekir (docs/systems/multiplayer.md §Aşamalar), due 2026-11-09
- [ ] **MP 4: Hayalet** #code - `LocalPlayerBroadcaster` + `GhostPlayer` prefab'ı; `Room.OnRoomEntered`'ın her girişte çağrıldığını doğrula, due 2026-11-27
- [ ] **MP 5: Bayrak işaretleri** #code - önce `RoomNew-22` ID'si 22; `RaceFlagPoint`, varsayılan yere yerleştirme komutu, `RaceFlag`, due 2026-12-04
- [ ] **Bayrakları yerleştir** #level - 22 odada `RaceFlagPoint`'leri istediğin yere sürükle, due 2026-12-07
- [ ] **MP 6: Ceza akışı** #code - önceki odaya ışınlama, kamera, checkpoint; PlayMode testi; `PenaltyHandler` hedefi uygularken `TryGetPenaltyTarget` ile okur, due 2026-12-11
- [ ] **MP 7: Lobi sahnesi ve multiplayer HUD** #code - `MultiplayerLobby.unity`, mod seçimi, host/katıl, bayrak sayacı, rakibin odası, due 2026-12-18
- [ ] **MP 8: Yarış kaydı, devam etme, kopma** #code - ayrı kayıt dosyası, `ProgressSync`, 30 sn bekleme, due 2026-12-25
- [ ] **MP 9: Steam taşıyıcısı** #code - Facepunch.Steamworks, AppID 480, iki bilgisayar ve iki hesapla test, due 2027-01-01

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
