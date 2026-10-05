# Tehlikeler

*GDD §2 (sütun 3), §5.3, §6.2 "Tehlikeler". Son kod doğrulaması: 2026-10-05.*

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

## Ölünce sıfırlama

Hareketli tehlikeler `PlatformController` / `WrappingPlatformManager` üzerinden `IResettable`. Ölünce başlangıç konumuna ve fazına dönerler, yani her denemede aynı ritmi görürsün. Sabit tehlikelerde sıfırlanacak bir şey yok.

## Kod

- `Death/DeathZone.cs`, `Death/AcidHazard.cs`, `Death/IKillable.cs`, `Death/PlayerHealth.cs`
- Hareket: `Platform/PlatformController.cs`, `WrappingPlatformManager.cs`, stratejiler
- Prefab'lar: `Assets/Prefab/New/Death/`, diken Tilemap'i `Assets/Prefab/New/Room/RoomNew.prefab`

## Bilinen kenar durumlar ve tasarım notları

1. **Hit-stop sırasında tehlikeler durmuyor.** Dash başlangıcında ve slingshot fırlatışında oyuncu `HitStopDuration` (0.2 sn) boyunca donuyor, dünya donmuyor ([`movement-core.md`](movement-core.md)). Hareketli bir tehlike bu sürede oyuncuya çarpabilir. Sık kullanılan "hareketli tuzak + dash" kombinasyonunda haksız bir ölüm gibi hissedilebilir; oynayarak kontrol edilmeli.
2. **Okunurluk henüz oyun içinden doğrulanmadı.** GDD §8.1 oda 9'daki dikenlerin uzaktan soluk durduğunu yazıyor. Oyun içi kamerayla kontrol `TASKS.md`'de (26 Ekim).
3. **Rastgele hızlı wrapping** `minSpeed ≠ maxSpeed` ise ölümden ölüme farklı davranır. Ölümcül varyantta deterministik tutulmalı.
4. **Asit ve diken görsel olarak farklı katmanlarda yaşıyor.** Diken bir Tilemap (oda prefab'ının parçası), asit ayrı prefab. Diken eklemek boyamakla, asit eklemek prefab koymakla yapılıyor.
5. **GDD §6.2 tehlike listesi dikeni saymıyor**, ama her oda prefab'ında bir diken Tilemap'i var ve §8.1 dikenleri oda 9 ve 22'de gösteriyor.
