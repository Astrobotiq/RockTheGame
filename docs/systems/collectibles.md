# Toplanabilirler, anahtarlar ve kapılar

*GDD §5.3, §6.2 "Toplanabilirler" ve "Diğer", §12 madde 4. Son kod doğrulaması: 2026-10-05.*

## Tasarım niyeti

Toplanabilirler hareket setini ödüllendirir ve ek bir meydan okuma katmanı ekler: bazı ananaslar yere basmadan tek nefeste toplanmalı, anahtar parçaları kapıları açar ya da kapatır, slingshot yenileyici havada ikinci bir slingshot imkânı verir. Checkpoint'ten sonra toplananlar ölünce kaybolur, checkpoint'e ulaşınca kalıcı olur. Ananasın oyun içindeki amacı henüz belli değil (GDD §12 madde 4).

## Ortak çekirdek: `Collectible` + efekt bileşenleri

Her toplanabilir aynı `Collectible` bileşenini taşır. Ne yaptığı, yanındaki `CollectibleEffect` alt sınıflarından gelir (kompozisyon: bir objede birden fazla efekt olabilir).

- Oyuncu (`PlayerController`) trigger'a girince: görsel gizlenir (`hideVisualOnCollect`), collider kapanır, ses kanalına olay gider, bütün efektler `Apply` edilir, `OnCollected` olayı yayınlanır.
- **Ölüm ve checkpoint davranışı** (`resetOnDeath`, varsayılan açık):
  - Checkpoint kanalı (`TransformEventChannelSO`) yayınlandığında, toplanmış olanlar **kalıcı** (`IsCommitted`) olur.
  - Oyuncu öldüğünde (`PlayerHealth.OnDeath`), toplanmış ama kalıcı olmayanlar geri gelir.
  - Kapı geçişleri de checkpoint'i yayınladığı için pratikte "oda değiştirince kalıcı" demek ([`death-respawn.md`](death-respawn.md)).
- **Süreli yeniden doğma** (`respawnsAfterDelay`, `respawnDelay`): toplandıktan sonra kendiliğinden geri gelir. Ölünce de hemen geri gelir.

| Tür | Prefab | Efekt | Ayarlar |
|---|---|---|---|
| **Ananas (altın)** | `Collectable-Pineaplle` | `CoinEffect`: `CollectibleInventorySO.AddCoin()` | ölünce sıfırlanır |
| **Anahtar parçası** | `Collectable-Keyshard` | `KeyShardEffect`: yalnızca log; asıl iş `RoomKeyManager`'da | ölünce sıfırlanır |
| **Slingshot yenileyici** | `Collectable-Slingshot` | `SlingshotRefillEffect`: `PlayerController.ResetSlingshot()` | **3 sn'de yeniden doğar** |
| **Göz şekli (kozmetik)** | — | `EyeShapeCollectibleEffect`: `PlayerEyesController.SetEyeInsideShape` | **Hiçbir prefab ya da sahnede yok** |

Prefab'lar: `Assets/Prefab/New/Collectable/`.

## Envanter: `CollectibleInventorySO`

- `totalCoins` (kalıcı) + `sessionCoins` (son checkpoint'ten beri). Gösterilen sayı ikisinin toplamı.
- `CollectibleInventoryManager` checkpoint kanalında `Commit()`, ölümde `Revert()` çağırır. Sahne açılışında envanteri sıfırlar; **kalıcı kayıt (save) yok**, oyun her açılışta 0'dan başlar.
- `CollectibleUIController`, `OnCoinsChanged` olayıyla sayacı günceller.

## "Tek nefeste" ananas: `AirborneCoinShardManager`

Prefab: `PineapplePartManager` (+ `Collectable-Pineaplle-Part Variant` parçaları; parçalar `CoinEffect`'i kaldırır, sayılmazlar).

1. Hedef ananas başta gizlidir. Parçalar havada toplanır ve toplanan her parça oyuncunun etrafında yörüngeye girer (`orbitRadius`, `orbitSpeed`).
2. **Bütün parçalar toplanmadan yere basılırsa** parçalar kavis çizerek yerlerine uçar (`flybackDuration`) ve yeniden toplanabilir olur.
3. Bütün parçalar toplandıktan sonra ilk yere basışta parçalar hedefe uçup birleşir (`mergeFlyDuration`), sonra **hedef ananas belirir**. Sayaca giren yalnızca bu hedef ananastır, ve onu da ayrıca toplamak gerekir.
4. Ölünce, hedef ananas kalıcı değilse bütün bulmaca başa döner.
5. Hedef ananas zaten toplanmış ya da kalıcıysa yönetici `Start`'ta kendini kapatır.

"Yere basmak" = `PlayerController.IsGrounded`, yani FSM state'i değil, fiziksel zemin sensörü (`KinematicPhysicsHandler`). Kancada sallanırken ya da yere çok yakın tırmanırken sensör zemini görürse bu da yere basmak sayılır. Tek yönlü platformlar da zemindir.

## Anahtar ve kapılar

### `RoomKeyManager`

- Bir anahtar parçası listesini (`Collectible`) ve bir `RoomDoor`'u bağlar.
- Bütün parçalar toplanınca kapıya bir eylem uygular: `Open`, `Close` ya da `Toggle` (`KeyActionType`). Yani anahtarlar kapı açabildiği gibi kapatabilir de (GDD: "açar ya da kapatır").
- Ölünce: parçaların hepsi kalıcı değilse kapı eylemden önceki hâline döner; parçalar da `Collectible` kuralıyla geri gelir.

### `RoomDoor` + `DoorJuicyVisual`

- `RoomDoor`: `Open()`/`Close()`, collider'ı kapatıp açar (`disableColliderOnOpen`), ses kanalına olay gönderir, `OnOpened`/`OnClosed` olaylarını yayınlar. Başlangıç durumu `startOpen`.
- `DoorJuicyVisual`: kapı olaylarını dinleyen görsel/animasyon katmanı.
- Kapıyı anahtarlardan başka şeyler de açabilir: oda 17'de bir dönen node, `RotationEventTrigger` üzerinden `Open`'ı çağırıyor ([`grapple.md`](grapple.md)). Bu yolla açılan bir kapı ölünce kapanmaz.

## Kod

- `Collectible/Collectible.cs`, `CollectibleEffect.cs` + dört efekt
- `Collectible/CollectibleInventorySO.cs`, `CollectibleInventoryManager.cs`, `CollectibleUIController.cs`
- `Collectible/AirborneCoinShardManager.cs`, `RoomKeyManager.cs`
- `Door/RoomDoor.cs`, `Door/DoorJuicyVisual.cs`
- `Player/Visual/PlayerEyesController.cs` (göz şekli)

## Bilinen kenar durumlar ve GDD ile farklar

1. **Kozmetik göz şekli oyunda yok.** GDD §6.2 listeliyor; efekt sınıfı yazılmış ama hiçbir prefab ya da sahne kullanmıyor. Kullanılsa bile ölümde ya da oturumlar arasında saklanmıyor.
2. **Ananasın amacı yok** (GDD §12.4). `TotalCoins`'i yalnızca sayaç arayüzü ve iki log satırı okuyor. Save sistemi olmadığı için oturumdan oturuma da taşınmıyor.
3. **Ölüm sıfırlaması `IResettable` kullanmıyor.** Toplanabilirler, anahtar yöneticisi, parça yöneticisi ve envanter `PlayerHealth.OnDeath`'i kendileri dinliyor. Hepsi `FindFirstObjectByType<PlayerHealth>()` ile bağlanıyor ve ölüm anında (iris kapanmadan) çalışıyor. Ayrıntı: [`death-respawn.md`](death-respawn.md) madde 2.
4. **Slingshot yenileyici her zaman yeniden doğuyor.** 3 sn sonra aynı yerden tekrar alınabilir; aynı noktada döngü kurulabilir. Bilinçli olabilir ama seviye tasarımında hesaba katılmalı.
5. **`KeyShardEffect` boş.** Parçanın anahtar olduğunu bildiren şey efekt değil, parçanın bir `RoomKeyManager` listesinde olması.
6. **Kapı ilk durumu collider'ın kendi durumuna güveniyor.** `startOpen: false` iken `Awake`'teki `Close()` erken döner (`IsOpen` zaten false) ve collider'a dokunmaz. Kapalı başlaması gereken bir kapının collider'ı prefab'da kapalı bırakılırsa kapı açık davranır.
