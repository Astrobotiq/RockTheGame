# Kalan işler

Bu dosya, 26–27 Eylül 2026 denetiminden geriye kalan işleri anlatır. Denetimin kendisi bitti ve onaylanan düzeltmeler `main`'e alındı; burada yazanlar **bilerek yapılmamış** olanlar.

Her madde şunu söyler: ne olduğu, neden önemli olduğu, karar vermen gerekip gerekmediği ve bir akşama sığıp sığmadığı.

Teknik ayrıntı ve kanıt arıyorsan: bulgu tablosu [`audit/2026-09-26-audit.md`](audit/2026-09-26-audit.md), ne yapıldığının kaydı [`audit/2026-09-26-yapilanlar.md`](audit/2026-09-26-yapilanlar.md).

---

## 1. Sadece senin karar verebileceğin şeyler

Bunların hiçbiri hata değil. Hepsi oyunun **hissini veya görünüşünü** değiştirir, o yüzden ben dokunmadım. Testler bu tür şeyleri ölçemez; oturup oynaman gerekir.

### 1.1 Yerde sol tetikle kanca atılamıyor

**Durum:** Havadayken iki tetik de kanca atıyor. Yerdeyken **sadece sağ tetik** çalışıyor; sol tetik hiçbir şey yapmıyor. Ayrıca yerdeyken sol kol hareket ettiğin yöne bakıyor, sağ kol sağ stick'e bakıyor — yani iki kol yerde simetrik davranmıyor.

**Nereden biliyoruz:** `GroundedState.cs`'de sol tetiği kontrol eden tek bir satır bile yok, sağ tetik için bir tane var. `AirborneState.cs`'de ikisi de var. GDD'nin 12. bölümü bunu zaten "bilinçli mi, eksik mi?" diye açık soru olarak yazmış — kod tarafı artık kesinleşti, cevap sende.

**Karar:** Bu bilinçli bir tasarım mı (yerden kalkış hep sağ kolla), yoksa eksik mi? Simetrik yapmak istersen `GroundedState`'e sol tetik dalını eklemek yarım saatlik iş, ama **kontrol hissini değiştirir** — yerden kalkışın nasıl hissettiği doğrudan etkilenir.

**Akşam:** Cuma (`#code`). Karar + uygulama tek akşama sığar.

### 1.2 Fizik adımı 50 Hz

**Durum:** Fixed Timestep `0.02` yani saniyede 50 fizik adımı. Fizik tabanlı platformerlarda `0.01667` (60 Hz) daha yaygın.

**Ne değişir:** Zıplama, dash, swing ve çarpışma çözümlemesinin hepsi bu adımda çalışıyor. 60 Hz'e çıkmak hareketi biraz daha akıcı ve tepkisel yapabilir, ama **bütün tuning'ini kaydırır** — `PlayerStatsSO`'daki değerler 50 Hz'e göre ayarlanmış. CPU maliyeti de %20 artar.

**Karar:** Denemeye değer ama geri dönüşü olan bir deney olarak yapılmalı: değeri değiştir, bir seviye oyna, beğenmezsen geri al. Beğenirsen muhtemelen birkaç `PlayerStatsSO` değerini yeniden ayarlaman gerekir.

**Akşam:** Cuma (`#code`). Deneme + karar bir akşam, beğenirsen yeniden tuning ayrı bir akşam.

### 1.3 Renk uzayı Gamma

**Durum:** Proje Gamma renk uzayında. URP genelde Linear önerir.

**Ne değişir:** **Oyundaki bütün renkler.** Linear'a geçmek daha doğru ışık ve blend matematiği verir, ama pixel art + kalıcı CRT filtresi düzeninde Gamma bilinçli bir tercih olabilir — kaynak sanat neyse ekranda o çıkar.

**Karar:** Şu an bir sorun yaşamıyorsan dokunmamak makul. Geçmek istersen tek bir ayar, ama sonrasında CRT filtresini ve tüm paleti yeniden gözden geçirmen gerekir.

**Akşam:** Pazartesi (`#art`), ve muhtemelen tek akşam yetmez.

### 1.4 Piksel yoğunluğu (PPU) karışık

**Durum:** Sprite'ların Pixels Per Unit değeri dağınık: 143 dosya 16, 107 dosya 100, gerisi 32/64/24/512/48/4.

**Ne demek:** Aynı sahnede farklı "piksel büyüklüğünde" sanat yan yana duruyor. GDD'nin 8.1 bölümü bunu zaten fark etmiş ("node'lar, borular ve zemin kremasının piksel yoğunlukları birbirinden farklı, 22. odadaki arazi kenarları basamaklı").

**Karar:** Tek bir standarda geçmek görsel tutarlılığı ciddi biçimde artırır, ama PPU değiştirmek **dünya ölçeğini ve çarpışma kutularını** kaydırır — yani odaların yeniden düzenlenmesi gerekebilir. Ayrıca GDD'ye göre NPC'ler bilerek taranmış illüstrasyon, onlar pixel art değil; hepsini aynı standarda zorlamak yanlış olur.

**Akşam:** Bu tek akşamlık iş değil. Önce hangi standardın seçileceğine karar ver (bir akşam), sonra kademeli uygula.

### 1.5 Pixel Perfect Camera yok

**Durum:** Projede `PixelPerfectCamera` bileşeni hiç kullanılmıyor; kamera Cinemachine ile sürülüyor.

**Ne değişir:** Eklemek pikselleri ızgaraya oturtur, hareket sırasındaki titremeyi ve bulanıklığı azaltır. Ama **görüntüyü değiştirir** ve kalıcı CRT filtresi + post-process + taranmış NPC illüstrasyonları karışımında istenmeyebilir.

**Karar:** 1.4 ile birlikte düşünülmeli — PPU standardı belli olmadan pixel perfect kamera kurmak anlamsız.

**Akşam:** Pazartesi (`#art`), 1.4'ten sonra.

### 1.6 Sıralama katmanı (Sorting Layer) yok

**Durum:** Projede tek bir sorting layer var: `Default`. Yani arka plan, platform, oyuncu, efekt ve arayüzün önde-arkada sırası tamamen "Order in Layer" sayıları ve Z pozisyonuyla yönetiliyor.

**Ne değişir:** Katman eklemek (`Background`, `Platform`, `Player`, `VFX`, `UI` gibi) sıralamayı tahmin edilebilir kılar ve "şu neden bunun önünde çıkıyor" tipi sorunları bitirir. Mevcut görüntüyü değiştirmeden yapılabilir ama **her sprite'ın katmanının tek tek atanması** gerekir.

**Karar:** Faydalı ama sıkıcı. Yeni dünyalara geçmeden önce yapmak, sonradan yapmaktan çok daha ucuz.

**Akşam:** Pazartesi (`#art`) veya Cuma (`#level`). Birkaç akşama yayılır.

---

## 2. Gerçek hatalar

Bunlar karar meselesi değil, bozuk şeyler. Denetimde değil, ben Play Mode'a girince ortaya çıktılar.

### 2.1 Ses seviyesi ayarları hiç çalışmıyor

**Durum:** `AudioManager` ses seviyelerini mixer'a yazmaya çalışıyor ama yazamıyor. Play Mode'a girer girmez konsola şu düşüyor:

```
Exposed name does not exist: MasterVolume
Exposed name does not exist: MusicVolume
Exposed name does not exist: SFXVolume
```

**Sebebi:** `AudioManager` mixer'da `MasterVolume`, `SFXVolume`, `MusicVolume`, `AmbientVolume` adında **exposed parametre** arıyor. `MainMixer` asset'inde ise `Master`, `SFX`, `Music` adında **grup**lar var. Unity'de grup ile exposed parametre aynı şey değil: bir grubun ses seviyesini koddan değiştirebilmek için o slider'a sağ tıklayıp "Expose to script" demen ve çıkan parametreye isim vermen gerekir. Bu yapılmamış. `Ambient` diye bir grup da hiç yok.

**Sonuç:** Oyuncu ses ayarını değiştirse bile hiçbir şey olmaz. Şu an ayarlar menüsü olmadığı için görünmüyor, ama menü yapıldığında ilk patlayacak yer burası.

**Nasıl düzelir:** Çoğunlukla Editor işi. `MainMixer`'ı aç, her grubun Volume'una sağ tıkla → Expose, sonra Audio Mixer penceresinin sağ üstündeki "Exposed Parameters" listesinden isimleri tam olarak `MasterVolume`, `SFXVolume`, `MusicVolume` yap. `Ambient` için önce grup oluşturman gerekir — ya da koddaki `ambientVolumeParam` alanını boş bırakıp o özelliği kapatabilirsin.

**Akşam:** Pazartesi (`#test`) ya da Cuma (`#code`). Bir akşamın yarısı.

### 2.2 Animator'da eksik parametre

**Durum:** Play Mode'da `Parameter 'Hash 0' does not exist.` uyarısı çıkıyor.

**Sebebi:** Bir animasyon script'i, Animator Controller'da tanımlı olmayan bir parametreyi set etmeye çalışıyor. Hangi controller ve hangi parametre olduğunu daraltmadım.

**Sonuç:** Muhtemelen bir animasyon geçişi sessizce tetiklenmiyor.

**Akşam:** Pazartesi (`#test`). Önce teşhis lazım: uyarının çıktığı anı yakalayıp hangi objede olduğunu bulmak.

---

## 3. Temizlik — güvenli ama acelesi yok

### 3.1 Kullanılmayan Unity modülleri

Denetimde dört paket kaldırıldı (`visualscripting`, `ai.navigation`, `multiplayer.center`, `timeline`). Geriye `com.unity.modules.*` ailesi kaldı: `terrain`, `terrainphysics`, `cloth`, `vehicles`, `vr`, `xr`, `wind`, `umbra`, `adaptiveperformance`. 2D bir oyunda hiçbiri kullanılmıyor.

**Neden yapılmadı:** Ayrı bir karar olarak bırakıldı. Kazanç küçük (derleme süresi ve paket yüzeyi), risk düşük ama sıfır değil — Unity bazı modülleri kendiliğinden geri ekleyebilir. Ayrıca `terrainphysics`, `terrain`'e bağlı; o ikisi birlikte kaldırılmalı.

**Akşam:** Cuma (`#code`). Yarım akşam, ama sonrasında bir kere Play Mode'da dolaşıp bir şeyin bozulmadığını görmek gerekir.

### 3.2 Build'e hiç girmeyen 1082 asset

Denetim, `NewScene.unity`'den başlayıp referans zincirini takip etti ve 1453 asset'in **1082'sine** hiç ulaşamadı. En büyük adaylar: `Sprites/` altında 506 dosya, `Pixel Adventure 1/` paketinde 107 dosya, `Tiles/Zone-3` ve `Tiles/Background` klasörlerinin tamamı.

**Neden silinmedi:** Bu tarama yanılabilir. Koddan `Resources.Load` ile yüklenen, editor aracıyla kullanılan ya da ileride kullanmayı planladığın asset'ler "kullanılmıyor" görünür. 1082 dosyayı tarama sonucuna bakarak silmek fazla riskli.

**Nasıl ilerlenir:** Klasör klasör git. Bir klasörden örnek bir asset seç, Project penceresinde sağ tık → "Select Dependencies" ve sahne açıkken "Find References In Scene" ile kontrol et. Hiç sonuç yoksa o klasör gerçekten ölüdür. Silinenler git geçmişinde kalır.

**Akşam:** Pazartesi (`#art`). Her akşam bir klasör; acele etme.

---

## 4. Yapmanı önermediğim şey: Git LFS

Depoda Git LFS kurulu ama `.gitattributes` hiçbir dosyayı LFS'e yönlendirmiyor. Büyük dosyalar doğrudan git'te: `NewScene.unity` 10 MB, müzik dosyaları 5–9 MB, bazı oda prefab'ları 4 MB.

**Neden dokunmuyoruz:** LFS'i şimdi açmak **geçmişi düzeltmez**, sadece bundan sonraki commit'leri etkiler. Geçmişi gerçekten temizlemek `git lfs migrate` gerektirir, o da bütün geçmişi yeniden yazar ve force-push ister. Bu depoda force-push yasak ve zaten paylaşılan dalları bozar. Kazanç, riske değmiyor.

**Ayrıca dikkat:** `.unity` ve `.prefab` dosyaları metin formatında; onları LFS'e almak diff ve merge yeteneğini tamamen kaybettirir. İleride LFS kurulursa yalnızca `.mp3`, `.wav`, `.png` gibi gerçek binary'ler için kurulmalı.

---

## 5. Ayrıca aklında olsun

- **`26abc45` commit'i oyuncunun çarpışma kodunu değiştirdi** (deprecated fizik API'leri yenileriyle değiştirildi). Test ettin ve bir sorun görmedin, ama tek yönlü platformlar, duvar kayması ve tavana çarpma gibi kenar durumlarda tuhaf bir şey fark edersen ilk bakılacak yer orası.
- **`6998405` sprite sıkıştırmasını kapattı** — görüntü değişti (netleşti). Bir yerde beklemediğin bir renk görürsen sebebi bu olabilir.
- **Çalışma ağacında duran dosyalar:** `M_CRTFilter.mat`, `M_IrisTransition.mat`, `DefaultVolumeProfile.asset`, TMP font asset'leri, `DOTweenSettings.asset`. Bunların hepsi Unity'nin kendi kendine yazdığı şeyler; commit edip etmemek sana kalmış. `PhysicsTest.unity` ve `PhysicsTest2.unity` ile iki prompt dosyası hâlâ takipsiz.

---

## 6. Bunlar TASKS.md'ye nasıl girer

Hiçbiri henüz `TASKS.md`'de değil — oraya eklemeden önce sana sormam gerekiyor. Hazır olduğunda şöyle bölünebilirler:

| İş | Kategori | Gün | Kaç akşam |
|---|---|---|---|
| Ses mixer parametrelerini expose et | `#test` | Pazartesi | yarım |
| Animator parametre uyarısını teşhis et | `#test` | Pazartesi | yarım |
| Yerden sol tetik kararı + uygulaması | `#code` | Cuma | 1 |
| Fixed Timestep denemesi | `#code` | Cuma | 1 |
| Kullanılmayan modül paketlerini kaldır | `#code` | Cuma | yarım |
| Kullanılmayan asset klasörlerini ele | `#art` | Pazartesi | klasör başına 1 |
| Sorting Layer yapısı kur | `#art` | Pazartesi | 2–3 |
| PPU standardı kararı | `#art` | Pazartesi | 1 (sonra uygulama ayrı) |
| Pixel Perfect Camera | `#art` | Pazartesi | PPU'dan sonra |
| Renk uzayı kararı | `#art` | Pazartesi | 1+ |

Hangilerini `TASKS.md`'ye almak istersen söyle, kurallara uygun biçimde (bir akşama en fazla 3 task, due yalnızca Pazartesi/Cuma) yerleştiririm.
