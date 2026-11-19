# Halftone platformlar ("Riso levha"): uygulama planı

Tarih: 2026-11-13. Geliştirici onayladı. Görsel hedef: [`riso-levha.html`](riso-levha.html) (canlı önizleme, oyun ölçeğinde). Tam tarif ve gerekçeler: [`supervisor-review.md`](supervisor-review.md) §3, doğrulama listesi §5. Bu belge `TASKS.md`'deki "Halftone" task'larının ayrıntısıdır. Her task tek başına, yeni bir oturumda yapılabilecek şekilde yazıldı.

## Verilmiş kararlar (değiştirme)

| Konu | Karar |
|---|---|
| Yön | "Riso levha": B (risograph) + D (karma) sentezi |
| Piksel art | **Yok.** Pürüzsüz baskı görünümü; nokta merkezlerini ve konumları piksele yuvarlama, `smoothstep` + `fwidth` ile kenar yumuşat |
| CRT | 960×540 denenecek (Task H1); karar H1'den sonra geliştiricide |
| Çizgi | `#140C1C`, kalınlık `max(0.08, 0.08·h)` dünya birimi, hiç kaymaz, collider sınırı |
| Mavi kanal | Teal `#289AA4` (mavi `#597DCE` sarıyla çamur yapıyor) |
| İz gecikmesi | teal 0 / sarı 0.06 / pembe 0.12 sn |
| Kapsam | Yalnızca **hareketli** platformlar (`PlatformController`, `TriggeredPlatformController`, `WrappingPlatformManager` altındaki düz platformlar). Kırılan platformlar, launch pad'ler ve sabit zemin değişmez |
| Aşama 2 | Şimdi yapılmaz; shader'a yalnızca boş property'ler konur (H3) |

## Sabit değerler (başlangıç; his geliştiricinin kararı, CLAUDE.md kural 4)

| Ad | Değer | Not |
|---|---|---|
| P (nokta aralığı) | 0.42 dünya birimi | CRT 480×360 kalırsa 0.5 |
| dMin | 0.14 birim | Bunun altındaki nokta dMin çapında, alfa `d/dMin` |
| Temiz üst şerit | `max(0.18, 0.22·h)` | Nokta yok |
| Köşe yarıçapı | üst `max(0.06, 0.22·h)`, alt `0.5·h` | |
| Riso kenar sapması | `0.03·h`, dalga boyu ~0.6 birim, **içe**, üstte 0 | Statik, kaynama yok |
| Katmanlar | teal 15°, sarı 45°, pembe 75°; %90 opak, multiply, kâğıt `#F1E6C7` üstünde | Hiçbiri 0°/90° değil |
| Çaplar (v: 0 üst, 1 alt) | sarı `P·(0.50+0.35v)`; pembe v<0.2 yok, `P·(0.26+0.40v)`; teal v<0.35 yok, `P·(0.24+0.34v)` | Hızlanan platformda `v *= 1.18` (yoğun bölge geniş) |
| Gren | çap ±%8, 0.25 birim ölçekli değer gürültüsü; kâğıt parlaklığı ±%3, 0.6 birim ölçek | Statik |
| Durağan kayma | pembe `(+0.04, -0.03)`, teal `(-0.04, +0.03)` birim | Sarı ve çizgi kaymaz |
| Hıza bağlı kayma | teal hızın tersine `2k·|v|`, pembe hız yönünde `k·|v|`, `k = 0.012 sn`, toplam üst sınır 0.06 birim | Hız ~0.1 sn üstel yumuşatılmış |
| İnce platform (h < 0.45) | nokta yok; kâğıt + iki kayık düz bant (sarı, pembe; %90; her biri gövdenin %60'ı) + çizgi | `OneWay-MovingPlatform Variant` 0.2 birim |
| İz yoğunluğu | mürekkep başına birim başına `1.2 × clamp(genişlik/3, 1, 2)` | |
| İz boyutu | doğduğu hücrenin çapı; %15 olasılıkla `1.2P` | İlk %40 sabit, sonra dMin'e küçül, sonra alfa sön |
| İz ömrü / hareket | 0.7–1.1 sn; başlangıç hızı platform hızı × −0.15 + dik ±0.2 birim/sn; sürtünme 3/sn; yerçekimi yok | Pürüzsüz, twos yok |
| İz üst sınırı | platform başına 54 (mürekkep başına 18) | Doluysa doğum atlanır |
| Teleport eşiği | bir karede > 1.5 birim | O kare emisyon yok, geçmiş tamponu ve birikmiş mesafe sıfırlanır |

Önizlemedeki karşılıkları `riso-levha.html` içindeki `LAYERS`, `slabPts`, `emitFrom`, `drawParts` fonksiyonlarında; Canvas kodu referanstır, kopyalanacak kod değildir.

---

## H1. CRT 960×540'ı tek sahnede dene (#test, insan)

**Neden:** CRT tüm ekranı hücreye yapıştırıyor (`Assets/Shader/CRTFilter.shader:162-165`). Malzemede `_PixelResolution` 480×360 (`Assets/Material/CRT/M_CRTFilter.mat:59`), volume profile'da `pixelResolution` 320×240 (`Assets/DefaultVolumeProfile.asset:548-550`). Hangisinin oyunda etkin olduğunu `Assets/Scripts/CRTEffect/CrtRenderPass.cs`'de doğrula.

**Yap:** Commit'lemeden, geçici olarak etkin değeri 960×540 yap; bir odada oyna, 480×360 ile ekran görüntüsü karşılaştır (`unity command capture_game_view`, CLAUDE.md "Screenshots"). Tarama çizgisi sıklığı aynı değere bağlıysa çizgilerin incelmesi beklenir; ayrı `_ScanlineResolution` gerekebilir (not et, ayrı task). Sonra değeri geri al.

**Bitti:** Geliştirici 960×540 / 480×360 kararını verdi ve `implementation-plan.md` "Verilmiş kararlar"a yazıldı. P buna göre 0.42 ya da 0.5.

## H2. Halftone platform shader'ı: şekil, çizgi, kâğıt (#code)

**Dosya:** yeni `Assets/Shader/RisoSlab.shader` (el yazımı HLSL, URP 2D sprite shader'ı; `FlowingPixelAcid.shader` ve `UiConcrete.shader` iskelet olarak). Yeni materyal `Assets/Material/Platform/M_RisoSlab.mat`.

- Sprite tek beyaz kare (mevcut `MovingPlatform.prefab` SpriteRenderer, Simple draw mode). Platform boyutu `lossyScale`'den okunur: `p = (uv - 0.5) * size` dünya birimi; desen gerilmez, platformla taşınır ve döner.
- SDF yuvarlatılmış levha: üst ve alt köşe yarıçapları ayrı; riso kenar sapması yalnızca yanlarda ve altta, içe.
- Görsel collider'ın dışına taşmaz (çizginin yarı kalınlığı hariç); üst kenar collider üstüne oturur.
- Kâğıt dolgu + kâğıt lekelenmesi; çizgi `#140C1C`. Kenar yumuşatma `fwidth`; piksel yuvarlama yok.
- Bu task'ta mürekkep katmanı yok.

**Bitti:** Derleme temiz, EditMode yeşil (çıktıyı raporla). Test sahnesinde (`PhysicsTest` ya da boş bir sahne) 2×1, 4×1, 3×0.2 boyutlarında üç quad'ın ekran görüntüsü; üst kenar düz ve collider gizmo'suyla çakışıyor.

## H3. Shader: üç mürekkep katmanı, gren, ince platform, aşama 2 yeri (#code)

**Dosya:** `RisoSlab.shader` (H2).

- Üç katman (yukarıdaki tablo): her biri kendi açısında döndürülmüş kafes, hücre merkezine uzaklık < yarıçap → nokta; `smoothstep` + `fwidth`; katmanlar kâğıt üstünde multiply; çizginin içinde kırpılır.
- Temiz üst şerit, dMin kuralı, gren (çap ±%8), durağan kayma.
- `_Accelerating` (0/1) → `v *= 1.18`.
- İnce platform yolu (h < 0.45): düz kayık bantlar.
- `_Velocity` (float2, dünya birimi/sn) property'si: hıza bağlı kayma (formül tabloda). Değeri H4 verir; 0 iken yalnızca durağan kayma.
- Aşama 2 için boş property'ler, değer 0, kodda kullanılmıyor ya da etkisiz: `_InfluencePos` (float2), `_InfluenceRadius` (float, 2.5), `_InfluenceAmount` (float, 0), `_ShiftFront` (float, 0). Ayrıntı `supervisor-review.md` §3.9.

**Bitti:** Derleme temiz, EditMode yeşil. H2'deki üç boyutun ekran görüntüsü, önizlemeyle (`riso-levha.html`, CRT benzetimi H1 kararına göre) yan yana. Kırmızı/yeşil/mor bindirmesi alt bölgede görünüyor; üst şerit temiz.

## H4. `RisoSlabVisual` bileşeni: hız ve parametreler (#code)

**Dosya:** yeni `Assets/Scripts/New-Scripts/Platform/RisoSlabVisual.cs` (namespace `New_Scripts.Platform`, Türkçe XML yorum).

- Aynı objedeki `IMovingSurface`'ten (ya da transform delta'sından) hızı okur, ~0.1 sn üstel yumuşatır, `MaterialPropertyBlock` ile `_Velocity` verir. `_Accelerating`'i strateji `AcceleratingPingPongStrategy` ise 1 yapar.
- Hızlanan platform uçta durunca kayma 0.15 sn içinde geri yaylanır (yumuşatma bunu doğal yapıyorsa ek kod yok).
- Ayarlanabilir değerler (k, üst sınır, yumuşatma süresi) bir ScriptableObject'te: `RisoSlabSettingsSO` (CLAUDE.md: tunable'lar SO'da). İz ayarları da (H5) aynı SO'ya girer.
- Saf hesap (yumuşatma, kayma vektörü + üst sınır) statik bir yardımcıda → EditMode testi (`Assets/Tests/EditMode/`).

**Bitti:** Derleme temiz, EditMode yeşil (yeni testler dahil). Play Mode'da hızlanan platformda renklerin hıza göre kaydığı ve durunca geri geldiği ekran görüntüsü ya da kısa kayıt.

## H5. `HalftoneTrailEmitter`: gecikmeli renk izleri (#code)

**Dosya:** yeni `Assets/Scripts/New-Scripts/Platform/HalftoneTrailEmitter.cs` + saf mantık için ayrı statik sınıf (geçmiş tamponu, gecikmeli örnekleme, teleport algılama, emisyon birikimi).

- Tek `ParticleSystem` (Simulation Space: World, Emission modülü kapalı); script `ParticleSystem.Emit(EmitParams)` çağırır. Hazır Rate over Distance kullanılmaz (teleport ve gecikme burada çözülür).
- Son ~0.3 sn'lik konum geçmişi halka tamponda. Her mürekkep platformun **gecikmeli** kopyasından doğar: teal 0, sarı 0.06, pembe 0.12 sn.
- Doğum yeri: gecikmeli kopyanın arka %40'ı ve alt yarısı (v > 0.4); o katmanın kafes hücrelerinden biri, hücrenin çapıyla. (Kafes hesabı shader'la aynı formül; C# tarafında bir kopyası.)
- Yoğunluk, boyut, ömür, hareket, üst sınır: yukarıdaki tablo.
- Teleport (> 1.5 birim/kare): o kare emisyon yok, tampon ve birikim sıfır; eski parçacıklar silinmez.
- `IResettable.ResetToDefault()` → `ParticleSystem.Clear()` + tampon + birikim sıfır. `LevelResetManager` kaydı `OnEnable`'da (docs/systems/platforms.md "Ölünce sıfırlama"). Oda uyanınca da aynısı: `OnEnable`'da temizle (oda `Sleep` → `SetAsNeighbor/SetAsCurrent` objeyi yeniden etkinleştiriyor mu, `Room.cs:69-113`'te doğrula).
- `TriggeredPlatformController` `t = 0`'da beklerken hareket yok → emisyon doğal olarak sıfır.
- EditMode testleri: gecikmeli örnekleme, teleport sıfırlaması, birikimden emisyon sayısı, üst sınır.

**Bitti:** Derleme temiz, EditMode yeşil (yeni testler dahil). Bu task'ta parçacık görünümü varsayılan olabilir (H6 yapar).

## H6. İz parçacık shader'ı ve prefab'lara uygulama (#code)

- Parçacık shader'ı (yeni `Assets/Shader/RisoDot.shader`): analitik SDF daire, kenar yumuşatma, gökyüzü üstünde **opak normal karışım** (multiply değil: gökyüzünü zeytine çevirir). Pembe ve teal parçacıkların yarısı, diğer mürekkepten çapın %22'si kayık bir "hayalet" daire taşır; kesişim rengi `renkA × renkB` shader içinde. Hayalet rengi ve var/yok bilgisi custom vertex stream ya da `EmitParams` rengiyle gelir.
- Sıralama: parçacıklar platformun ve node'ların **arkasında** (Sorting Layer; docs: Background → Default → Platform → Player → VFX → UI).
- Prefab'lar (yalnızca görsel; collider'a dokunma): `Prefab/New/Platforms/MovingPlatform.prefab`, `MovingPlatform-Transform Variant.prefab`, `OneWay-MovingPlatform Variant.prefab`, `AccelPlatform/AccelPlatform Variant.prefab`, `RotatingPlatform/RotatingPlatform Variant.prefab`, `PlatformWrapper/WrappingPlatformManager.prefab` altındaki düz platformlar. Her birine `M_RisoSlab` + `RisoSlabVisual` + çocuk obje olarak `HalftoneTrailEmitter`. `MovingLauncPad`, `WrappingLaunchPadManager Variant`, kırılan platformlar kapsam dışı.
- Sahne/prefab işi yalnızca `unity command eval` ile (CLAUDE.md); `.unity` YAML'ı elle yazma.
- `docs/systems/platforms.md`'ye "Görünüm" bölümü ekle (shader, bileşenler, SO, sıfırlama) ve "Son kod doğrulaması" tarihini güncelle.

**Bitti:** Derleme temiz, EditMode + PlayMode yeşil (oyuncu kodu değişmedi ama prefab'lar değişti; CLAUDE.md "Tests"). Üç odada (bir hızlanan, bir dairesel, bir wrap olan oda) Play Mode ekran görüntüsü; ölüp doğunca ve wrap'te ekranı çizen iz yok.

## H7. Halftone platformları gamepad ile dene (#test, insan)

`supervisor-review.md` §5 listesi. Özet: CRT açık (H1 kararı), 1080p; kamera yakın ve oda 16 gibi uzak odada okunurluk; ince tek yönlü platform; dönen platform (temiz şerit yukarıda mı); hızlanan platform (yoğun bölge + kayma "fırlatır/durdu" diyor mu); ölüm, oda geçişi ve wrap'te iz temizliği; node'lar hâlâ baskın mı; iz dikkat dağıtıyor mu. Değer değişikliği gerekiyorsa `RisoSlabSettingsSO`'da, eski ve yeni değeri not et.

**Bitti:** Geliştirici notları bu belgeye ya da `docs/systems/platforms.md`'ye yazıldı; ayar gerekiyorsa yeni task açıldı.

## Sıra ve bağımlılık

H1 → (P kararı) → H2 → H3 → H4 → H5 → H6 → H7. H4 ve H5 birbirinden bağımsız ama ikisi de H3'teki property adlarına dayanır.
