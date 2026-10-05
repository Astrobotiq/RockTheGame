# Ölüm ve yeniden doğma

*GDD §2 (sütun 3), §5.3. Son kod doğrulaması: 2026-10-05.*

## Tasarım niyeti

Can barı yok, tek temas öldürür. Ölüm hızlı ve ucuz olmalı ki "bir daha dene" hissi kopmasın. Ölünce oda başlangıç durumuna döner. Checkpoint'ten sonra toplananlar ölünce kaybolur, checkpoint'e ulaşınca kalıcı olur.

## Ölüm anı

1. Oyuncu bir **trigger** collider'a girer ve o collider'da `DeathZone` vardır. `DeathZone`, girenin `IKillable` olup olmadığına bakar; oyuncuda bu `PlayerHealth`'tir.
2. `PlayerHealth.Kill()`: `Death` titreşimi, ölüm partikülü, oyuncu objesi **devre dışı** (`SetActive(false)`), `OnDeath` olayı.
3. `OnDeath`'i dinleyenler **hemen**, iris kapanmadan önce çalışır:
   - `RespawnManager`: respawn dizisini başlatır.
   - `Collectible`: kalıcı olmayan toplanmışları geri getirir.
   - `CollectibleInventoryManager`: envanterdeki geçici altınları geri alır.
   - `RoomKeyManager`: anahtarlar kalıcı değilse kapıyı eski hâline döndürür.

Ölüm katmanı (`PlayerController.deathLayerMask`) öldürmez. Yalnızca kenardan çıkmanın tehlikeli bir yüzeye tırmanmasını engellemek için kullanılır (bkz. [`wall-movement.md`](wall-movement.md)). Öldüren her zaman `DeathZone`'dur. Tehlike türleri: [`hazards.md`](hazards.md).

## Respawn dizisi (`RespawnManager.RespawnSequenceAsync`)

| Adım | Süre (sahne değeri) |
|---|---|
| `PlayerController.OnStartRespawn`: hız sıfır, iki anchor bırakılır, dash + slingshot + dayanıklılık dolar, sürekli titreşim durur | — |
| İris kapanır, merkez ölüm noktası (`TransitionManager`) | `_transitionDuration` 0.5 sn |
| `LevelResetManager.ResetAll()` | — |
| Oyuncu respawn noktasına taşınır ve tekrar aktif edilir (hâlâ donuk) | — |
| Kameranın yetişmesi beklenir | `_cameraCatchUpDelay` 0.3 sn |
| İris açılır, merkez respawn noktası | 0.5 sn |
| `OnEndRespawn`: `AirborneState`'e geçilir, kontrol oyuncuda | — |

Ölümden kontrolün geri gelmesine kadar yaklaşık **1.3 sn** geçer. Değerler `Prefab/New/Managers/RespawnManager.prefab` ve `TransitionManager.prefab` içinde (`_maxRadius` 2).

## Ne sıfırlanır, ne sıfırlanmaz

| Şey | Ölünce | Nasıl |
|---|---|---|
| Kırılan platformlar (`BreakablePlatform`, `SegmentedBreakablePlatform`) | Sıfırlanır | `IResettable` |
| Hareketli platformlar ve tuzaklar (`PlatformController`) | Sıfırlanır | `IResettable` |
| Tetiklenen platformlar (`TriggeredPlatformController`) | Sıfırlanır | `IResettable` |
| Sınırdan sınıra akan nesneler (`WrappingPlatformManager`) | Sıfırlanır | `IResettable` |
| Checkpoint sonrası toplanan ananas/altın, anahtar parçaları | Geri gelir | `Collectible`, `OnDeath`'i kendisi dinler |
| Envanterdeki geçici altın sayısı | Geri alınır | `CollectibleInventorySO.Revert` |
| Anahtarlarla açılan/kapanan kapı | Eski hâline döner (anahtarlar kalıcı değilse) | `RoomKeyManager` |
| Süreli yeniden doğan toplanabilirler (örn. slingshot yenileyici) | Hemen geri gelir | `Collectible.respawnsAfterDelay` |
| Oyuncunun kaynakları (dash, slingshot, dayanıklılık) | Dolar | `OnStartRespawn` |
| Duvar kayma süresi | İlk yere inişte dolar | `GroundedState` |
| Dönen node, `OncePerNode` modu | **Sıfırlanmaz** | `FullRotationNode` `IResettable` değil |
| Uyuyan odalardaki nesneler | **Sıfırlanmaz** | `IResettable` kaydı `OnEnable`/`OnDisable`'da yapılıyor; yalnızca o an aktif olan oda ve komşuları sıfırlanır |

## Checkpoint'ler

- `Checkpoint` bir trigger. Oyuncu ilk kez girince `TransformEventChannelSO` üzerinden kendi `transform`'unu yayınlar; `RespawnManager` bunu yeni respawn noktası yapar. Trigger yolu her checkpoint için **yalnızca bir kez** çalışır.
- **Oda kapıları da checkpoint'tir.** `RoomTransitionTrigger.associatedCheckpoint` atanmışsa her geçişte `ActivateCheckpoint()` çağrılır. Bu yol "bir kez" kontrolünü atlar, yani geri dönülen odanın checkpoint'i yeniden respawn noktası olur. Atanmamışsa konsola uyarı düşer.
- Aynı kanal toplanabilirleri de kalıcı yapar: her checkpoint ve her kapı geçişi o ana kadar toplananları "commit" eder ([`collectibles.md`](collectibles.md)).
- Başlangıç: `LevelBootstrapper`, başlangıç odasının `InitialCheckpoint`'ini ilk respawn noktası yapar. `Room.InitialCheckpoint` bunun dışında hiçbir yerde okunmuyor.

## Kod

- `Death/PlayerHealth.cs` (`IKillable`), `DeathZone.cs`, `AcidHazard.cs`
- `Death/RespawnManager.cs`, `TransitionManager.cs` (iris, `M_IrisTransition.mat`)
- `Death/Checkpoint.cs`, `TransformEventChannelSO.cs`
- `Death/LevelResetManager.cs`, `IResettable.cs`
- `LevelChange/RoomTransitionTrigger.cs`, `LevelBootstrapper.cs`
- `Player/PlayerController.cs`: `OnStartRespawn`, `OnEndRespawn`

## Bilinen kenar durumlar ve GDD ile farklar

1. **Respawn "anında" değil.** GDD §5.3 "anında" diyor; kodda iki iris geçişi ve bir bekleme var, toplam ~1.3 sn. Celeste ölçeğinde bu kısa sayılır ama bilinçli bir sayı olarak tutulmalı: üç değer iki prefab'a dağılmış durumda.
2. **Ölüm sıfırlaması iki ayrı mekanizmayla yapılıyor.** Platformlar `IResettable` + `LevelResetManager` (iris kapandıktan sonra), toplanabilirler/anahtarlar/envanter `PlayerHealth.OnDeath` (ölüm anında, iris kapanmadan). Oyuncu iris kapanırken toplanabilirlerin yerine döndüğünü görebilir. Yeni sıfırlanabilir bir sistem eklerken hangisinin kullanılacağına karar verilmeli; CLAUDE.md yalnızca `IResettable`'ı anlatıyor.
3. **`IResettable` kaydı `OnEnable`'da.** `LevelResetManager.Awake`'ten önce `OnEnable` alan bir nesne kaydolmaz. Odalar `LevelBootstrapper.Start`'ta önce uyutulup sonra açıldığı için oda içindekiler sonunda kaydolur. Oda dışında duran bir sıfırlanabilir nesne ise bu yarışı kaybedebilir.
4. **Dönen node'un `OncePerNode` modu ölümü bilmiyor** (bkz. [`grapple.md`](grapple.md)).
5. **Kapı geçişi, geçiş reddedilse bile checkpoint'i etkinleştirir.** `ExecuteTransition` başka bir geçiş sürerken erken dönüyor, ama `ActivateCheckpoint()` yine de çağrılıyor.
6. **"Checkpoint öncesi toplananlar kaybolur" kuralı pratikte "kapı geçişinden önce" demek.** Kapılar checkpoint olarak bağlandığı için, ayrı bir checkpoint objesi olmadan da her oda geçişi toplananları kalıcı yapar.
