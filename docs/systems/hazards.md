# Tehlikeler

*GDD §2 (sütun 3), §5.3, §6.2 "Tehlikeler". Son kod doğrulaması: 2026-11-18 (asit temas tepkisi).*

## Tasarım niyeti

Düşman yok; tehlike her zaman ortamdan gelir. Tek temas öldürür, bu yüzden her tehlike **okunur** ve **öngörülebilir** olmalı (zor ama adil). GDD §6.3'e göre en sık kullanılan üçlü: launch pad + hareketli/dönen ölümcül tuzak + tek yönlü platform.

## Nasıl öldürür

Bütün tehlikeler aynı tek bileşene dayanır: **trigger** bir collider üzerinde `DeathZone`. Oyuncu trigger'a girince `IKillable.Kill()` çağrılır. Sonrası [`death-respawn.md`](death-respawn.md).

- Tehlikeler **katı değildir**: oyuncu üstlerinde duramaz, onlara tutunamaz, içlerine girer ve ölür.
- Diken, asit ve `MovingDeath`'in öldürücü collider'ı `Death` katmanında (8). Bu katmanın tek kod etkisi, kenardan çıkmanın tehlikeli bir yüzeye tırmanmayı reddetmesi ([`wall-movement.md`](wall-movement.md)). Öldüren katman değil, `DeathZone`. Yeni bir tehlikeyi `Death` katmanına koymayı unutmak öldürmeyi bozmaz, ama kenardan çıkma onu güvenli zemin sanabilir.

## Türler

| Tür | Prefab | Kurulum | Hareket |
|---|---|---|---|
| **Diken** | `RoomNew.prefab` içindeki `Spike` Tilemap'i | Tilemap + trigger `CompositeCollider2D` + `DeathZone`, her odada bir tane | Sabit; odaya boyanır |
| **Asit** | `Death/Acid.prefab` | `AcidHazard` + `DeathZone` | Sabit |
| **Akan asit** | `Death/FlowingAcid.prefab` (`FX_AcidFog` varyantı) | `AcidHazard` + `DeathZone` + sis efekti | Sabit; akış görsel |
| **Hareketli ölümcül nesne** | `Death/MovingDeath.prefab` | `PlatformController` + `TransformPingPongStrategy` + `DeathZone` | İki nokta arası sabit hızla gidip gelir |
| **Hızlanan ölümcül nesne** | `Death/AccelDeath Variant.prefab` | `MovingDeath` + `AcceleratingPingPongStrategy` | Hızlanarak gider, uçta bekler, yavaşlayarak döner |
| **Dönen ölümcül nesne** | `Death/RotatingDeath Variant.prefab` | `RotatingPlatform Variant` + `DeathZone`, `keepUpright: 0` | Merkez etrafında daire çizer ve yörünge boyunca döner (prefab varsayılanı `radius` 5, `period` 4) |
| **Sınırdan sınıra akan ölümcül nesneler** | `Death/WrappingDeathManager Variant.prefab` | `WrappingPlatformManager` + `PlatformController`'lar + `DeathZone` | Tek yönde akar, uçtan başa sarar |

Hareketli tehlikeler platformlarla aynı sürücü/strateji altyapısını kullanır. Hareket, zamanlama ve sıfırlama kuralları için bkz. [`platforms.md`](platforms.md). Kendi ekseni etrafında yerinde dönen bir "testere" tipi yok; "dönen" olan, bir yörüngede dolaşan nesne.

### `AcidHazard`

Yalnızca görsel ve kurulum yardımcısı:
- Collider'ı otomatik trigger yapar (`autoConfigureCollider`).
- Asit materyalini atar ve objenin dünya boyutuna göre shader'a `_PixelScale` gönderir. Böylece asit hangi boyuta çekilirse çekilsin pikselleri kare ve `referencePPU` (16) ölçeğinde kalır.
- `[ExecuteAlways]`: editörde ölçek değişince kendini günceller.
- Shader'a son yazdığı ızgara boyutunu `PixelScale` olarak verir (`AcidReaction` okur).

### Temas tepkisi (`AcidReaction`, 18 Kasım 2026)

Oyuncu sabit asite değdiğinde asit temas noktasında tepki verir. Plan ve araştırma: [`../asit-tepki-plani.md`](../asit-tepki-plani.md). Şu an yalnızca `Acid.prefab`'da; akan asit ve hareketli tehlike kabuğu planın B aşamasında.

**Olay sırası.** `DeathZone` iki C# olayı yayınlar:
- `Killing`: `Kill()`'den **önce**. Oyuncunun hızı ve konumu burada okunur, çünkü `OnDeath` içinde `RespawnManager` `Velocity`'yi sıfırlar.
- `Killed`: `Kill()`'den **sonra**. Asit titreşimi burada verilir; `HapticManager.Vibrate` çalanı kestiği için genel `Death` titreşimini bilerek ezer.

`Kill()` `try/finally` içinde: dinleyici hata atsa da oyuncu ölür. EventChannelSO yerine C# olayı, çünkü sinyal aynı objedeki bileşenler arasında, örnek başına ve `Kill()`'den önce senkron çalışmalı. SO kanal her ölümü her asite yayınlar ve göndereni filtrelemek gerekirdi.

**Ölümden sonra da süren iz (19 Kasım 2026).** İris ≈0.33 sn'de kapandığı ve ölüm anında genel ölüm parçacığı (`FX_PlayerDeath`) temas noktasının üstüne bindiği için ölüm anındaki tepki neredeyse görünmüyor. Bu yüzden asıl okunur iz ölümden **sonra**: temas noktasından iki yana yayılan dalgalar ve kaynayan kabarcıklar 4 sn sürer. Ölüm sıfırlaması (`ResetToDefault`, iris kapalıyken) yalnızca damlaları temizler, darbe yuvasını bilerek bırakır; iris açıldığında (≈1.3 sn) yüzey hâlâ dalgalı olur. Aynı asitte yeni bir ölüm yuvayı yeniden başlatır. Dalga kendi `_RippleDecay`'i ve süre sonuna doğru doğrusal bir sönümle tam 0'a iner.

**Temas noktası.** Trigger fizik adımından sonra gelir; oyuncu merkezi çoğu zaman zaten asidin içindedir ve `ClosestPoint` içerideki noktayı aynen döndürür. Bu yüzden konum bir adım (`Velocity × fixedDeltaTime`) geri sarılır, en yakın sınır noktası alınır, yüzey satırına (`_AcidLevel`) kilitlenir.

**Shader.** Tek darbe yuvası `_AcidImpact` = (x px, y px, yaş, güç), `MaterialPropertyBlock` ile; `AcidHazard`'ın `_PixelScale`'i korunur (önce Get). Ortak kod `Assets/Shader/AcidImpact.cginc`: temas sütununda çöken ve toparlanan çukur, temas çevresinde parlama, iki yana yayılan sönümlü dalga treni (`AcidRipple`) ve temas çevresinde kaynama (`AcidBoil`, kabarcık eşiğini düşürür). Darbe kendi 12 FPS'ine kesilir, ortam dalgası materyaldeki 8 FPS'te kalır. Ayarlar `Mat_PixelArtAcid` üzerinde; şu anki değerler test için **abartılı** (19 Kasım 2026, gamepad testinde ayarlanacak): `_ImpactDuration` 4, `_CraterDepth` 10 px, `_FlashRadius` 20 px, `_FlashTime` 0.5, `_RippleAmp` 9 px, `_RippleSpeed` 60 px/sn, `_RippleWavelength` 22 px, `_RippleDecay` 0.3, `_BoilRadius` 60 px.
- **Yaşı CPU sayar** (`Update`'te `Time.deltaTime`). Shader `_Time` kullanmaz: URP'de `_Time` sahne yüklemede sıfırlanmaz, `timeSinceLevelLoad` ile karşılaştırılsaydı ana menüden gelince tepki hiç görünmezdi.
- Yuva canlı değilse shader fonksiyonları tam 0 döndürür: darbe yokken görüntü eskisiyle aynı (donmuş zamanla ölçüldü, eşik üstünde 0 piksel fark).
- Bir asitte tek darbe yuvası var; süre (4 sn) respawn'dan uzun olduğu için aynı asitte yeni bir ölüm eski izi yeniden başlatır.

**Damlalar.** `FX_AcidImpact` (`Assets/Prefab/New/VFX/`), ilk temasta asidin child'ı olarak bir kez oluşturulur; `EmitParams` ile 2 px ve 3 px kare damlalar, oyuncunun hızına göre eğilir. Materyal `M_FX_AcidPixel` (Sprite-Unlit; 2D Renderer Particles/* shader'larını desteklemez). En küçük damla 2 px, çünkü CRT ekranı 480×360'a indiriyor ve 1 px titrer. Oda uyanınca ve ölüm sıfırlamasında (`IResettable`) temizlenir.

**Ayarlar.** `New-Scripts/Death/SO/AcidReactionSettings.asset`: güç (hız 4 → 40, taban 0.35), damla sayısı/hızı/boyu, koni 55°, oyuncu hızından pay, damla hız tavanı 9 (iris dairesinden kaçmasın), `acidDeathRumble` (0.35 sn), `splashCue`/`sizzleCue`. Sesler 2D çalar; cue boşsa ya da klibi yoksa sessiz geçer. Ses dosyaları henüz yok.

**İris penceresi.** İris yarıçapı `maxRadius`'tan −1'e lerp ettiği için görünen daire ≈0.33 sn'de kapanır. Tepkinin bütün değerleri ilk 0.25 sn'de okunacak şekilde seçildi.

**Doğrulama.** `AcidReaction.DebugImpact(localPoint, age, velocity)` yalnızca Play Mode'da çalışır: `Time.timeScale = 0` iken yaş ilerlemez, kare deterministik olur. Kareler `docs/ui-research/acid-reaction/`.

**Kapsam dışı.** Çok oyunculu rakip (`GhostView`) collider'sız; ölümünde tepki yok.

## Hareketli tehlikelerin görünüşü

`MovingDeath` (ve `AccelDeath Variant`), `RotatingDeath Variant` ve `WrappingDeathManager Variant` aynı görünüşü kullanır: yerçekimi makinesinin havada tuttuğu asit. Tasarım araştırması `reports/Asit yerçekimi tehlike tasarımı.md`.

- **Asit kabuğu:** collider'lı objenin `SpriteRenderer`'ı. Sprite `HazardAcid_Quad` (1.5 birimlik dolgulu quad), materyal `M_HazardAcid_Circle` / `M_HazardAcid_Square`, shader `HazardAcid`. Taban şekil collider'ın kendisi; dalga ve küt dikenler yalnızca **dışa** taşar, görsel öldüren alandan küçük görünmez. Kutu, boyutunu transform ölçeğinden okur (`DisableBatching`, RisoSlab ile aynı).
- **Çekirdek:** `Core` child'ı, `HazardCore_Circle` / `HazardCore_Square` prefab'ı (çizilmiş sprite, shader `HazardCore`). Daire çekirdeğin 8 lambası sırayla yanar, kare çekirdeğin ızgara yarıkları yavaşça nefes alır (`*_LampMask` / `*_VentMask` dokuları).
- **`HazardCoreVisual`:** çekirdeği kare ve ortada tutar; boyutu kısa kenarın `_CoreFraction` katı (materyalden okunur, varsayılan 0.55). Hesap `HazardCoreMath`, test `HazardCoreMathTests`.
- Cihazın tamamı öldürür; çekirdek ayrı bir güvenli bölge değildir.
- Duvardan çıkan ezici bloklarda (oda 16) çekirdek başlangıçta duvarın arkasında kalır ve blok dışarı çıktıkça görünür. Bilerek böyle bırakıldı.
- Ayarlar (lamba hızı, dalga, diken, çekirdek boyu) `Assets/Material/Hazard/` materyallerinde.

## Ölünce sıfırlama

Hareketli tehlikeler `PlatformController` / `WrappingPlatformManager` üzerinden `IResettable`. Ölünce başlangıç konumuna ve fazına dönerler, yani her denemede aynı ritmi görürsün. Sabit tehlikelerde sıfırlanacak bir şey yok.

## Kod

- Asitte kalan kafalar: `Death/AcidHeadHost.cs` (sabit asitte Pool, hareketli tehlikelerde Blob), `AcidHeadPool.cs`; ayrıntı [`death-respawn.md`](death-respawn.md)
- `Death/DeathZone.cs`, `Death/AcidHazard.cs`, `Death/AcidReaction.cs`, `Death/AcidImpactMath.cs`, `Death/AcidReactionSettingsSO.cs`, `Death/IKillable.cs`, `Death/PlayerHealth.cs`, `Death/HazardCoreVisual.cs`
- Görünüş: `Assets/Shader/HazardAcid.shader`, `Assets/Shader/HazardCore.shader`, `Assets/Sprites/Hazard/`, `Assets/Prefab/New/Death/HazardCore/`
- Hareket: `Platform/PlatformController.cs`, `WrappingPlatformManager.cs`, stratejiler
- Prefab'lar: `Assets/Prefab/New/Death/`, diken Tilemap'i `Assets/Prefab/New/Room/RoomNew.prefab`

## Bilinen kenar durumlar ve tasarım notları

1. **Hit-stop sırasında hareketli tehlikeler durur (16 Kasım 2026).** Dash başlangıcında ve slingshot fırlatışında oyuncu `HitStopDuration` (0.2 sn) donarken `PlatformController` / `TriggeredPlatformController` / `WrappingPlatformManager` de yerinde bekler, yani "hareketli tuzak + dash" kombinasyonunda tuzak kaymaz ([`movement-core.md`](movement-core.md)). Gamepad ile denenmedi; asit parçacıkları ve animasyonlar durmaz, bu görsel olarak yadırganırsa ayrıca bakılmalı.
2. **Okunurluk (oyun içi kamerayla, 6 Kasım 2026).** Oda 9'da dikenler gümüş ve koyu çerçeveli, mavi zeminde net okunuyor. En zayıf yer, node arkasındaki pembe ve lila halkaların üstündeki sıralar; çerçeve sayesinde yine seçiliyor. Oda 16'da kamera çok uzak (ortho 22) ve zemindeki dikenler ince bir noktalı çizgi gibi duruyor. Aynı odada beyaz ezici bloklar node'ların önünden geçerken node'u tamamen örtüyor. O zamanki beyaz kare ve daireler 15 Kasım 2026'da asit + çekirdek görünüşüne geçti (aşağıda "Hareketli tehlikelerin görünüşü").
3. **Rastgele hızlı wrapping** `minSpeed ≠ maxSpeed` ise ölümden ölüme farklı davranır. Ölümcül varyantta deterministik tutulmalı.
4. **Asit ve diken görsel olarak farklı katmanlarda yaşıyor.** Diken bir Tilemap (oda prefab'ının parçası), asit ayrı prefab. Diken eklemek boyamakla, asit eklemek prefab koymakla yapılıyor.
5. **GDD §6.2 tehlike listesi dikeni saymıyor**, ama her oda prefab'ında bir diken Tilemap'i var ve §8.1 dikenleri oda 9 ve 22'de gösteriyor.
