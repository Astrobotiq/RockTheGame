# Kamera

*Son kod doğrulaması: 2026-11-15 (derleme, EditMode testleri ve Play Mode'da kare kare ölçüm; gamepad denemesi yapılmadı).*

## Tasarım niyeti

Kamera oyuncuyu hızına göre öne bakarak takip eder; oda sınırında durur. Bazı yerlerde oyuncular hazırlıksız yakalandığı için (ör. oda 4'ün dikenli dikey boşluğu, hareketli kare) kamera **elle konmuş bir çerçeveye** geçer ve tehlikenin bütün yolunu gösterir. Çerçeve yalnızca oyuncuların gerçekten takıldığı yerlere elle konur, toplu uygulanmaz. Plan ve gerekçe: [`../kamera-cerceve-plani.md`](../kamera-cerceve-plani.md).

## Nasıl çalışır

`CameraController` (`Player/`), `cameraFollowTarget` Transform'unu sürer; Cinemachine bu hedefi izler, `CinemachineConfiner2D` kadrajı oda sınırına (`Room.RoomBounds`) sıkıştırır.

- **Takip:** hedef, oyuncunun konumu + öne bakış (hıza göre, `lookAheadMultiplier`) ve çift kancada kanca ortasına `swingFocusWeight` kadar kaymış hâlidir; `followLerpSpeed` ile lerp edilir. Hedef ayrıca oda sınırına kenetlenir (`ClampFollowTargetToBounds`).
- **Zoom:** `minOrthoSize` (8), `zoomLerpSpeed` ile lerp.
- **Oda geçişi:** `PrepareForTransition` → `PanAndZoomCameraAsync` → `FinalizeTransition`; ışınlamada `SnapToRoomBounds`. Ayrıntı: oda/geçiş sistemi.
- **Override sağlayıcıları:** `ICameraOverrideProvider` (`Priority`, `IsActive`, `Settings`, `FrameSpace`). Aktifler arasından en yüksek öncelik seçilir. İki tür var: `Room` (oda geneli, öncelik 0, `RegisterOverride` ile) ve `CameraOverrideZone` (bölge, `IPositionCameraOverride`).

## Bölgeler (`CameraOverrideZone`)

- Trigger olayı değil **konum**: `CameraController.LateUpdate` her karede kayıtlı bölgelere oyuncunun `transform.position`'ını sorar (`Collider2D.OverlapPoint`). Bölge `OnEnable`'da kaydolur (sahne açılışında kamera hazır değilse `Start`'ta tekrar dener), `OnDisable`'da çıkar; uyuyan odanın bölgesi pasiftir.
- **Çıkış payı:** `exitMargin` (1 birim). İçerideyken oyuncu collider'dan bu kadardan fazla uzaklaşana kadar aktif kalır; sınırda zıplamak kamerayı ileri geri çekmez. Karar `CameraZoneMath.IsActive` (saf, EditMode testli).
- Oda geçişi ve ışınlama yalnızca `Room` override'larını temizler; bölgeler kayıtlı kalır, oyuncu bölgenin içinde başlarsa override çalışır.
- Collider yalnızca alan tanımıdır (trigger, çarpışma yok).

## `FrameArea` odak tipi

`CameraOverrideSettings`: `frame` (bölge transform'una göre yerel `Rect`), `framePadding` (1), `blendTime` (0.35 sn, blend'in tamamı). Kamera merkezi çerçevenin merkezi, ortho boyu `max(h/2, w/2/aspect) + padding` (`CameraFraming.Fit`, EditMode testli). `overrideZoom` açıksa elle girilen `cameraSize` kazanır. Öne bakış kapalı. Dönen bölgede çerçeve eksen hizalı sınır kutusuna indirgenir.

**Ayrı çerçeve kamerası:** Çerçeve, oyuncu kamerasından ayrı, sabit bir `CinemachineCamera`'dır (composer ve confiner yok; `CameraController` çalışma anında iki tane yaratır, `FrameCamera_0/1`). Bölge aktifken önceliği 10'a çıkar; geçişi (konum + ortho boyu) **Cinemachine Brain** `EaseInOut` blend'iyle yapar, süre `blendTime` (Brain'in `DefaultBlend`'i her geçişten hemen önce ayarlanır; çıkışta bırakılan bölgenin süresi). Böylece:

- Kamera dead zone / damping / lookahead'e takılmadan **tam çerçevenin** merkezine ve boyuna oturur.
- Confiner'a hiç dokunulmaz. (Önceki sürüm confiner'ı açıp kapıyordu: confiner uzantısı composer'dan *sonra* kamerayı çeker, composer bunu görmez; kapatınca kamera tek karede 1–4 birim sıçrıyordu. 2026-11-15 ölçümü.)
- Oyuncu kamerası altta normal takibe devam eder (çerçeveyi atlayarak bir sonraki override'ı kullanır); çıkış blend'i güncel bir kameraya yapılır.
- İki çerçeve bölgesi arası geçiş de blend'lenir (iki kamera dönüşümlü kullanılır).
- Çerçeve odadan büyük olabilir: oda sınırı yalnızca oyuncu kamerasına uygulanır. Oda sınırı dışında kalan alanda komşu oda uyuyorsa orası boş görünür; çerçeve yazılırken kontrol edilir.
- Hit sarsıntısı (`CinemachineImpulseListener`) çerçeve kameralarına da kopyalanır.

**Oda geçişi / ışınlama:** `PrepareForTransition` çerçeve kamerasını kesip bırakır (pan, şu an ekranda görünen boyla başlar). `SnapToRoomBounds` kesip bırakır ve bir bayrak koyar; bir sonraki `LateUpdate`'te oyuncu çerçeve bölgesindeyse çerçeve kamerası blend'siz (kesme) devreye girer (oda o anda henüz uyuyor olabileceği için hemen değil).

## Editörde

- Gizmo: çerçeve (sarı), kameranın göstereceği 16:9 alan (cyan), oda sınırı (gri). Kamera görüşü oda sınırını aşıyorsa turuncu: hata değil, bilgi; taşan yerde uyuyan oda/boşluk olmasın.
- Sahne tutamacı: `CameraOverrideZoneEditor` (`Editor/`), sarı kutuyu sürükle (`BoxBoundsHandle`).
- "Çerçeveyi tehlikeye sığdır": seçilen `PlatformController`'ın hareket yolu (`MovementStrategy.TryGetPathBounds`) + platform boyutu + tetik alanı çerçeveye yazılır.

## Kullanılan yerler

- **Oda 4, `CameraFrame_SpikeShaft`** (`RoomNew-4 Variant.prefab`, oda kökünün çocuğu, öncelik 10): dikey boşluğun girişi (kırılan platformlar, y≈30) ile üst çıkış (y≈48) arası, x 149.5–163.5; çerçeve = tetik alanı. `Square`'in yolu (x 161.43, y 36.4–45.4), duvardaki diken (x≈159, y 41–43) ve boşluk çerçevenin içinde. Ortho boy 10.25, merkez (156.5, 38.75). Görüş oda sınırını yatayda ve üstte aşıyor (oda 23 birim genişlikte; normal 8 boyda bile görüş 28 geniş); görüşe giren tek başka oda `RoomNew-5` ve o komşu (aktif).

## Bilinen kenar durumlar

- **Respawn:** ölüm sonrası respawn kameraya dokunmaz. Checkpoint çerçeve bölgesindeyse ama oyuncu bölge dışında ölmüşse kamera ölüm noktasından `blendTime` ile çerçeveye gider. `SnapToRoomBounds` yolu (oda başlangıcı, ışınlama) doğrudan oturur.
- Brain'in `DefaultBlend`'i çalışma anında değiştirilir (sahnede `Cut` kalır); `PlayerCamera` dışında başka sanal kamera eklenirse bu hesaba katılmalı.
- `blendTime`, `exitMargin`, `framePadding` hissiyat değerleridir: başlangıç 0.35 / 1 / 1, gamepad ile denenmeli.
