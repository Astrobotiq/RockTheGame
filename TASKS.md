# Tasks

> **Format:** `- [ ] **Başlık** #kategori - kısa bağlam, due YYYY-MM-DD`
> **Kategoriler:** `#art` `#test` `#level` `#code`
> **Due yalnızca Pazartesi veya Cuma olabilir.** Planlama kuralları: `CLAUDE.md` → "Proje yönetimi ve planlama".

## Active

- [ ] **Git geçmişindeki şirket içi doküman izine karar ver** #code - `577078b` origin/main'de; geçmiş yeniden yazılacak mı, force-push gerekir (bkz. docs/sonraki-session.md §1), due 2026-10-09
- [ ] **Ping-pong stratejilerinde period=0 NaN düzeltmesi** #code - Linear ve TransformPingPongStrategy; `period` için alt sınır + EditMode testi (docs/systems/platforms.md), due 2026-10-09
- [ ] **Yerden sol tetik kararı + uygulaması** #code - GroundedState'te sol tetik dalı yok; bilinçli mi, eksik mi (GDD §12.6, docs/kalan-isler.md §1.1), due 2026-10-09
- [ ] **Ses mixer parametrelerini expose et** #test - MainMixer'da MasterVolume/SFXVolume/MusicVolume yok, Ambient grubu yok (kalan-isler §2.1), due 2026-10-12
- [ ] **Animator 'Hash 0' uyarısını teşhis et** #test - Play Mode'da çıkan eksik parametre uyarısının kaynağını bul (kalan-isler §2.2), due 2026-10-12
- [ ] **Player build al ve doğrula** #test - RockTheGame.Runtime'daki EasyTextEffects.Editor importu build'de patlıyor mu, due 2026-10-12
- [ ] **Fixed Timestep 60 Hz denemesi** #code - 0.02 → 0.01667 dene, bir seviye oyna, beğenmezsen geri al (kalan-isler §1.2), due 2026-10-16
- [ ] **Kullanılmayan modül paketlerini kaldır** #code - terrain+terrainphysics, cloth, vehicles, vr, xr, wind, umbra, adaptiveperformance (kalan-isler §3.1), due 2026-10-16
- [ ] **"Bozulmamalı" davranışlar listesini yaz** #test - dash/swing/slingshot/duvar için test yazılabilmesi için beklenen davranışlar (sonraki-session §3), due 2026-10-19
- [ ] **PPU standardı kararı** #art - 16/100/32... karışık; hangi standart, NPC'ler hariç (kalan-isler §1.4), due 2026-10-19
- [ ] **Sorting Layer 1: katmanları tanımla, oyuncu ve platformlar** #level - Background/Platform/Player/VFX/UI (kalan-isler §1.6), due 2026-10-23
- [ ] **Sorting Layer 2: arka plan, VFX, dekor** #art - kalan sprite'ları katmanlara ata, due 2026-10-26
- [ ] **Renk uzayı kararı (Gamma/Linear)** #art - geçilirse CRT ve palet yeniden gözden geçirilir (kalan-isler §1.3), due 2026-10-26
- [ ] **Oda 9 dikenleri + Play Mode görselleri** #test - oyun içi kamerada okunurluk, oyuncu+HUD çekimleri (sonraki-session §2), due 2026-10-26
- [ ] **Pixel Perfect Camera denemesi** #art - PPU kararından sonra (kalan-isler §1.5), due 2026-11-02
- [ ] **Kullanılmayan asset: Pixel Adventure 1 klasörü** #art - Select Dependencies / Find References ile doğrula, sonra sil (kalan-isler §3.2), due 2026-11-02
- [ ] **Kullanılmayan asset: Tiles/Zone-3 ve Tiles/Background** #art - aynı yöntem, due 2026-11-02
- [ ] **Multiplayer mod kararı ve ağ yaklaşımı** #code - öncelikli, yeni özelliklerden önce sistem kurulacak; yarış mı birlikte mi (GDD §9, §12.3), ağ çözümü araştırması, sonraki adımlar buradan bölünür, due 2026-11-06

## Waiting On

## Someday

- [ ] **Kullanılmayan asset: Sprites/ klasörü** #art - 506 dosya; alt klasör başına bir akşam
- [ ] **GDD §8 "yeşil, pis asit" ile görsel uyumu** #art - metin mi sanat mı güncellenecek
- [ ] **Beyaz dikdörtgen ve daireler** #art - odalar 2, 9, 12, 16, 19, 22: placeholder mı, bilerek mi
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
