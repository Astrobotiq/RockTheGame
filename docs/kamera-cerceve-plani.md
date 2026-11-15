# Kamera çerçeve bölgeleri — plan

Tarih: 2026-11-15. Durum: tasarım, uygulanmadı.

## Sorun

Oyuncular oda 4'teki (`RoomNew-4`) bir bölümde sürekli hazırlıksız yakalanıyor: diken (`Spike`, ~(156, 17)) bölgesine girildiğinde odanın sonundaki hareketli tehlike (`Square`, `TransformPingPongStrategy`, ~(161, 36)) kamera oyuncuyu takip ettiği için görünmüyor. İstenen: bu alana girince kamera **sabit bir konuma geçsin ve tehlikenin bütün hareket yolunu gösterecek kadar genişlesin**.

Araç genel ve tekrar kullanılabilir olmalı, ama **yalnızca oyuncuların gerçekten takıldığı yerlere elle konur**. Bütün hareketli tehlikelere otomatik ya da toplu uygulanmaz; şimdilik tek yer oda 4.

Çerçeve, odanın kamera sınırından (`Room.RoomBounds`, confiner) **daha büyük olabilmeli** (oda 4'te öyle olacak), ve bu durumda kamera zıplamamalı, kırpılmamalı, titrememeli.

## Mevcut kod neden yetmiyor

Mevcut sistem: `CameraOverrideZone` (trigger) → `CameraController.RegisterOverride` → `GetHighestPriorityOverride` → `UpdateFollowTarget` içinde `CameraOverrideSettings`'e göre odak/zoom. `Room` da aynı arayüzle (öncelik 0) oda geneli override taşıyor. Altyapı doğru, ama:

1. **Elle sayı girmek gerekiyor.** "Şu alanı göster" demenin yolu yok; `StaticPosition` için dünya koordinatı, `cameraSize` için ortho boyu tahmin edilip yazılıyor. En-boy oranına göre doğru boyu bulmak deneme-yanılma. Bu yüzden sahnede neredeyse hiç kullanılmamış.
2. **Bölge kaydı kırılgan.** `PrepareForTransition` ve `SnapToRoomBounds` `activeOverrides.Clear()` yapıyor, ama bölgenin `isActiveZone`'u `true` kalıyor ve oyuncu bölgeden çıkıp girmeden yeniden kaydolmuyor. Oyuncu oda geçişinden ya da `RoomPlacement` ışınlamasından sonra bir bölgenin içinde başlarsa override sessizce kayboluyor (`CameraController.cs` `PrepareForTransition`, `SnapToRoomBounds`; `CameraOverrideZone.cs` `OnTriggerEnter2D/Exit2D`).
3. **Kenarda titreme.** Giriş/çıkış oyuncu collider'ının kenarına bağlı; bölge sınırında zıplayan oyuncu kamerayı ileri geri çeker.
4. **Geçiş yavaş.** Konum `followLerpSpeed` (8), zoom `zoomLerpSpeed` (3) ile lerp ediliyor; zoom-out yaklaşık bir saniye sürüyor, tehlike yine geç görünüyor.
5. **Oda sınırı kırpıyor.** `ClampFollowTargetToBounds` takip hedefini, `CinemachineConfiner2D` de kadrajı oda sınırına sıkıştırıyor. Override odadan büyük bir alan gösteremiyor: çerçeve oda dışına taşarsa ya da ortho boyu odadan büyükse kamera istenen yeri göstermiyor. Confiner'ı kaba biçimde açıp kapatmak da çıkışta tek karede sıçrama yapar.

## Çözüm: "çerçeve" odak tipi + sağlam bölge algılama

Yeni bir sistem yazmıyoruz; mevcut override altyapısını genişletiyoruz.

### 1. `CameraFocusType.FrameArea`

`CameraOverrideSettings`'e yeni odak tipi ve alanları:

- `Rect frame` — **bölgenin transform'una göre yerel** dikdörtgen (bölge prefab'ı taşınınca çerçeve de taşınır).
- `float framePadding` (varsayılan 1) — kenar payı, birim.

`FrameArea` seçiliyken kamera merkezi `frame`'in merkezi, ortho boyu **otomatik**:

```
size = max(frame.height / 2, frame.width / 2 / aspect) + padding
```

`overrideZoom` açıksa elle girilen `cameraSize` yine kazanır (kaçış yolu). Hesap saf statik bir yardımcıda olsun (`CameraFraming.Fit(Rect worldFrame, float aspect, float padding) → (Vector2 center, float size)`) ve EditMode testiyle korunsun: geniş çerçeve genişliğe göre, uzun çerçeve yüksekliğe göre boyutlanır, padding eklenir, sıfır/negatif boyut güvenli.

Öne bakış (`_currentLookAheadOffset`) `FrameArea`'da kapalı (bugünkü `StaticPosition` gibi).

### 2. Bölgeyi oyuncunun konumuyla algıla

Trigger olaylarını bırak. Bölgeler `OnEnable`'da `CameraController`'a kaydolsun, `OnDisable`'da çıksın (uyuyan odaların bölgeleri zaten pasif). `CameraController.LateUpdate` her kare kayıtlı bölgeler için "oyuncunun merkezi (`targetRigidbody.position`) bölgenin collider'ında mı" diye baksın (`Collider2D.OverlapPoint`). `IsActive` bu sonuçtan gelsin.

- Oda geçişi ve ışınlama sonrası kayıp override sorunu kendiliğinden biter; `activeOverrides.Clear()` yalnızca `Room` override'larını etkiler.
- **Çıkış payı (histerezis):** `exitMargin` (varsayılan 1 birim). İçerideyken, oyuncu collider'dan `exitMargin`'den fazla uzaklaşana kadar aktif kal (`Collider2D.ClosestPoint` mesafesi). Kenarda titreme biter.
- `Room` override'ı bugünkü gibi `RegisterOverride` ile çalışmaya devam eder; ikisi aynı öncelik listesinden seçilir.

### 3. Hızlı ve sabit süreli geçiş

Aktif override değiştiğinde (girerken de çıkarken de) konum ve boy `Vector2.SmoothDamp` / `Mathf.SmoothDamp` ile `blendTime` sürede hedefe gitsin (`CameraOverrideSettings.blendTime`, varsayılan 0.35 sn; çıkışta bırakılan bölgenin değeri kullanılır). Geçiş bitince normal takip mantığı devralır. Normal takip (override yokken) davranışı **değişmez**.

`blendTime` hissiyat değeri (kural 4): başlangıç değeri 0.35, kullanıcı gamepad ile dener.

### 4. Çerçeve oda sınırından büyük olabilir

`FrameArea` aktifken çerçeve oda sınırına bağlı değildir:

- **Girerken:** `ClampFollowTargetToBounds` atlanır ve confiner kapatılır (`confiner.enabled = false`). Kamerayı zaten tamamen çerçeve belirliyor, sınırlamaya gerek yok. Kapatma anında kamera oda sınırının içinde olduğu için sıçrama olmaz. Blend, kameranın o anki gerçek konumundan başlar (takip hedefinden değil).
- **İçerideyken:** konum ve boy doğrudan çerçeveden gelir; oda sınırına ya da `Room` override'ının zoom'una bakılmaz.
- **Çıkarken:** confiner hemen açılmaz. Blend, takibe dönülecek konumun **oda sınırına göre sınırlanmış** hâline (`CalculateConfinedPosition`, mevcut) ve odanın normal boyuna gider. Blend bitince confiner açılır ve `InvalidateLensCache()` + `InvalidateBoundingShapeCache()` çağrılır; o anda kamera zaten sınırın içinde olduğu için sıçrama olmaz. Blend sırasında oyuncu yeniden bölgeye girerse confiner kapalı kalır.
- **Oda geçişi / ışınlama:** `PrepareForTransition` ve `SnapToRoomBounds` bugünkü gibi confiner'ı kendileri yönetir; çerçeve blend durumu (blend'de mi, confiner kimde) sıfırlanır.
- **Görünen boşluk:** oda sınırı dışında kalan alan ekrana girer. Komşu odalar aktif, ama daha uzaktaki odalar `RoomManager` tarafından uyutulduğu için o alanda görünmez. Oda 4'te çerçeve yazılırken bu kontrol edilir; sorun olursa çerçeve daraltılır.

### 5. Editörde görünür

- Gizmo: çerçeve dikdörtgeni + **kameranın gerçekte göstereceği alan** (16:9, padding dahil). Trigger alanı ayrı renkte. Oda sınırı da ince çizgiyle çizilir; kamera görüşü onu aşıyorsa bu hata değil, sadece bilgi (turuncu): "oda dışı görünecek, uyuyan oda olmasın".
- Sahne tutamacı: `BoxBoundsHandle` ile çerçeve sürüklenerek ayarlanır (`New-Scripts/Editor/` altında küçük bir custom editor).
- "Çerçeveyi tehlikeye sığdır" düğmesi (opsiyonel, aynı editor): seçilen `PlatformController`'ın hareket yolunun sınırlarını + trigger alanını çerçeveye yazar. `TransformPingPongStrategy` / `AcceleratingPingPongStrategy` uç noktaları, `CircularMovementStrategy` yarıçapı.

### 6. Respawn ve oda başlangıcı

`SnapToRoomBounds` sonunda bölgeleri hemen değerlendirip, oyuncu bir çerçeve bölgesindeyse kamerayı doğrudan o çerçeveye oturtsun (blend yok, confiner kapalı). Checkpoint'i bir çerçeve bölgesinin içinde olan odada ölünce kamera önce oyuncuya, sonra çerçeveye kaymasın.

## Oda 4 uygulaması

> **Düzeltme (2026-11-15, uygularken):** Aşağıdaki koordinatlar yanlıştı. `Spike` ~(156, 17) sadece tilemap'in transform'u; gerçek diken 3 karo, duvarda x≈159, y 41–43. Dikey boşluk (x 158.5–162.5) ve `Square` yolu (x 161.43, y 36.4–45.4) odanın üst kısmında. Uygulanan bölge ve çerçeve: x 149.5–163.5, y 29.5–48 (bkz. `docs/systems/camera.md`).

`RoomNew-4`'e bir `CameraOverrideZone` (`FrameArea`, öncelik 10):

- Trigger: dikenli bölgeye girilen alan (ekran görüntüsündeki boru ve diken bölümü, ~x 145–160, y 10–25).
- Çerçeve: trigger alanı + `Square`'in bütün ping-pong yolu (odanın üst ucu dahil). "Tehlikeye sığdır" düğmesiyle.
- Çerçeve oda 4'ün kamera sınırından büyük olacak (kullanıcı isteği). Oda sınırı dışında kalan alanda uyuyan oda ya da boşluk görünmemeli; görünürse çerçeveyi daralt.

Başka odalara **konmaz**. İleride başka bir yerde oyuncular aynı sorunu yaşarsa, o yer için ayrı karar verilir.

## Bitti sayılma ölçütleri

- `CameraFraming.Fit` EditMode testi yeşil; mevcut testler yeşil; derleme temiz.
- `docs/bozulmamali-davranislar.md` PlayMode testleri yeşil (kamera oyuncu fiziğine dokunmuyor ama kontrol edilsin).
- Oda 4'te: bölgeye girince kamera 0.35 sn içinde çerçeveye geçiyor, `Square`'in bütün yolu ekranda; çıkınca takibe dönüyor; sınırda zıplayınca titremiyor.
- Çerçeve oda sınırından büyükken: girişte, çıkışta ve sınırda gidip gelirken kamera sıçramıyor, kırpılmıyor; çıkıştan sonra confiner yeniden çalışıyor (kamera oda dışına kaymıyor).
- Bölge içindeki bir noktadan respawn olunca kamera doğrudan çerçevede başlıyor.
- Oda geçişinden hemen sonra bir bölgenin içinde başlanırsa override çalışıyor.
- `docs/systems/camera.md` yazıldı (`docs/systems/README.md`'ye eklendi): takip, öne bakış, oda override'ı, bölgeler, `FrameArea`, öncelik, geçiş, editör uyarıları.
