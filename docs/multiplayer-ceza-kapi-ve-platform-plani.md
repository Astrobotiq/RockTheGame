# Multiplayer: cezadan sonra kapalı kalan kapı ve havada uçan hayalet

*Oluşturma: 2026-11-23 (Opus planı; uygulama Sonnet oturumunda). İlgili sistem dokümanları: [`systems/multiplayer.md`](systems/multiplayer.md), [`systems/collectibles.md`](systems/collectibles.md), [`systems/platforms.md`](systems/platforms.md).*

## Playtest bildirimi (2026-11-23)

Yarış modunda iki oyuncu da oda 12'deydi. Biri odayı bitirdi, 3. bayrak cezasıyla diğeri oda 11'e ışınlandı.

1. **Kapı:** 11'den 12'ye inerken kapanan kapı, ceza sonrası 11'e dönüldüğünde hâlâ kapalıydı.
2. **Hareketli platform:** Rakip hareketli bir platformun üstündeyken hayaleti havada uçuyor gibi görünüyor.

---

## Sorun 1: Ceza odayı "ilk giriş" hâline getirmiyor

### Oda 11'in kurgusu (sahneden okundu, `NewScene`)

| Nesne | Konum | Ayar |
|---|---|---|
| Başlangıç checkpoint'i | (293.2, 117.2), sol üst | |
| `RoomNew-11/DynamicContent/Door` | (328.5, 117.5), sağda | `startOpen: false`. Arkasında yan oda 22'ye geçiş, (338.0, 117.3) |
| `RoomKeyManager` | anahtar (313.0, 119.8) | `Open`: kapıyı açar, oda 22'nin yolu açılır |
| `RoomKeyManager (1)` | anahtar (302.9, 98.9), 12'ye iniş yolunda | `Close`: aynı kapıyı kapatır |
| 12'ye geçiş | (293.5, 86.8), sol alt | |

Yani 12'ye inen oyuncu ikinci anahtarı alır ve oda 22'nin kapısı kapanır. Tek kişilik oyunda bu tek yönlü bir kapı, sorun yok.

### Neden kapalı kalıyor

Ceza (`Multiplayer/Penalty/PenaltyHandler.cs` `ApplyAsync`) yalnızca `LevelResetManager.ResetAll()` çağırıyor. Bu da yalnızca `IResettable` olan **platformları** sıfırlıyor. Anahtar, kapı ve toplanabilirler kendilerini yalnızca `PlayerHealth.OnDeath` ile sıfırlıyor ([`systems/death-respawn.md`](systems/death-respawn.md)):

- Ceza ölüm değil, `OnDeath` tetiklenmiyor. `RoomKeyManager.HandlePlayerDeath` hiç çalışmıyor.
- Çalışsa da işe yaramazdı: iki anahtar da oda geçişinde etkinleşen checkpoint ile kalıcı (`IsCommitted`) oldu, kalıcı anahtarlar ölünce geri gelmez.
- Açan anahtar (313.0, 119.8) zaten toplanmış durumda, oyuncu kapıyı yeniden açamıyor.

Sonuç: ceza sonrası oyuncu 12'ye yine inebilir ama **oda 22'ye (ve oradaki bayrağa) giremez.** Kök neden genel: tek kişilik kod "bir odaya her zaman ilk kez girilir" varsayımıyla yazılmış, ceza bu varsayımı bozan tek yol. Aynı sorun oda 8 (açan anahtarlar) ve oda 20'de de çıkar: ceza oyuncuyu o odalara geri atarsa kapı açık kalır, anahtarlar toplanmış görünür.

### Çözüm

Ceza, oyuncuyu gönderdiği odadaki anahtar–kapı durumunu **ilk girişteki hâline** döndürür. Tek kişilik koda yalnızca ekleme yapılır (multiplayer.md "Tek kişilik oyunun korunması" madde 3):

1. **`RoomDoor.ResetToInitial()`** (yeni, public): `startOpen`'a göre `Open()`/`Close()` uygular, ama **sessiz**: ses çalmaz (iris kapalıyken kapı sesi duyulmasın). `OnOpened`/`OnClosed` yine yayınlanır, `DoorJuicyVisual` görseli doğru pozu alsın. Sessizlik için `Open`/`Close`'un ses satırı bir `bool playSound` iç parametresine bağlanabilir; mevcut public imzalar değişmez.
2. **`RoomKeyManager.ResetForRoomReentry()`** (yeni, public): her anahtarda `Collectible.ForceReset()` (zaten var: kalıcılığı kaldırır, görseli ve collider'ı açar), kapıda `ResetToInitial()`, `wasActionExecuted = false`.
3. **`Multiplayer/Penalty/RoomStateReset.cs`** (yeni, statik): `ResetRoom(Room room)` odanın altındaki bütün `RoomKeyManager`'ları (`GetComponentsInChildren<RoomKeyManager>(true)`, uyuyan oda kapalı olabilir) bulup `ResetForRoomReentry()` çağırır.
4. **`PenaltyHandler.ApplyAsync`:** `LevelResetManager.ResetAll()`'dan sonra, `RoomPlacement.PlaceAt`'tan önce `RoomStateReset.ResetRoom(target)` ve ayrılınan oda için `RoomStateReset.ResetRoom(current)`. **Karar (2026-11-23, kullanıcı): "bulunduğum oda ve bir sonraki" sıfırlanır.** Ceza her zaman bir oda geri attığı için "bir sonraki", ceza anında içinde bulunulan (ayrılınan) odadır: 12 → 11'de 11 ve 12, yan oda 22 → 11'de 11 ve 22. Oyuncu ikisine de yeniden "ilk kez" girecek. `PlaceAt` sonradan checkpoint'i etkinleştirir; o anda toplanmış anahtar olmadığı için hiçbir şey kalıcı olmaz.

Kayıtlı yarışa devam (`RoomPlacement` ortak) bu değişiklikten etkilenmez: sahne yeni yüklendiği için oda zaten ilk hâlinde.

**Kapsam dışı (bilerek; ikisi de 2026-11-23'te kullanıcı kararı):**
- **Ananas ve diğer toplanabilirler sıfırlanmaz.** `CollectibleInventorySO` sayacı ikinci kez artardı. Yarışta ananasın anlamı yok (GDD §12 madde 4).
- **Anahtarsız açılan kapılar** (oda 17, `RotationEventTrigger`) sıfırlanmaz. 18 → 17 cezasında kapı açık kalır, bu zararsız. Kapatılırsa dönen node'un tetiği tek seferlikse oda kilitlenebilirdi.

### Kabul

- PlayMode testi (`Assets/Tests/PlayMode/` içinde, `PenaltyFlowTests`'e ek): oyuncu oda 11'deki iki anahtarı alır, 12'ye geçer, ceza uygulanır. Beklenen: oyuncu 11'de, kapı `IsOpen == false` ve collider açık, iki anahtar `IsCollected == false && IsCommitted == false`, açan anahtar yeniden alınınca kapı açılır. Ayrıca oda 8 senaryosu: anahtarlarla açılmış kapı, 9 → 8 cezasından sonra yeniden kapalı. **Test senaryosu insan onayına sunulur** (CLAUDE.md kural 3; bu plan onaylanınca test onayı sayılır).
- `PenaltyFlowTests` ve EditMode testleri yeşil, mevcut tek kişilik testler (`bozulmamali-davranislar.md`) yeşil.
- `systems/multiplayer.md` (ceza akışı satırı), `systems/collectibles.md` (yeni metotlar, "Bilinen kenar durumlar") güncellenir, "Son kod doğrulaması" tarihi yenilenir.

---

## Sorun 2: Hayalet, rakibin dünyasındaki platformun konumunu kullanıyor

### Neden uçuyor

Hayalet, rakibin **dünya konumunu** çiziyor (`SnapshotMessage.Position`). Platformlar iki dünyada farklı yerde, çünkü platformun fazı her oyuncunun kendi geçmişine bağlı:

- `PlatformController` konumu `GetPositionAtTime(Time.time - _timeOffset)` ile hesaplıyor. `_timeOffset` her ölümde (`ResetToDefault` → `_timeOffset = Time.time`) ve her hit-stop'ta (`HoldForHitStop`) kayıyor. İki oyuncu aynı anda başlasa bile ilk ölümden sonra platformlar ayrışıyor.
- `TriggeredPlatformController` yalnızca o dünyanın oyuncusu basınca hareket ediyor. Rakip bastıysa senin dünyandaki platform hâlâ yerinde.
- Kırılan platformlar, takas blokları da aynı şekilde oyuncuya özel.

Yani rakip kendi platformunun üstünde dururken senin ekranında platform başka yerde, hayalet boşlukta.

### Neden platformları eşitlemek çözüm değil

- Saat eşitlemek yalnızca sürekli dönen `PlatformController`'ları kurtarır. Ölüm ve hit-stop her dünyada fazı yeniden kaydırır. Tetiklenen ve kırılan platformlarda hiç işe yaramaz.
- Platformu ağdan sürmek (ortak platform, host otoritesi) tasarımın birinci kuralına aykırı: "Rakip senin dünyana dokunmaz". Bu, multiplayer.md "Kararı değiştirecek koşullar"daki NGO'ya geçiş koşulu olurdu. Netcode for GameObjects de burada yardım etmez: platformların ayrı olması bir hata değil, tasarım kararı.

### Karar (2026-11-23, kullanıcı): seçenek B, rakibin platformu da hayalet olarak çizilir

Üç seçenek konuşuldu: A) hayaleti yerel platforma göre çizmek (aşağıdaki bölüm), B) rakibin platformunu da hayalet olarak çizmek, C) yalnızca soluklaştırmak. **Kullanıcı B'yi seçti.** Hayalet platformun görüntüsü baştan tartışılacak, gerçek platformdan **farklı** olacak (yalnızca yarı saydam kopya değil). Görüntü kararı verilmeden hayalet task'ları açılmaz. Sıra: önce kapı (MPF1–2), sonra hayalet.

**Kurallar (2026-11-23, kullanıcı):**
- Hayalet platform **yalnızca rakip üstüne basarken** görünür (hareketli duvara tutunma ve hareketli node'a tutunma da aynı). Önerilen zamanlama: ~0.1 sn giriş, ayrılınca ~0.4 sn kalıp solma; yerel platformla arası ~0.25 birimden azsa çizilmez.
- **Rakibi öldüren hareketli tehlike** (oyunda bunlar da `PlatformController` ile sürülen `Death` nesneleri) ölüm anında rakibin dünyasındaki pozunda ~0.6 sn görünüp solar. Sabit diken/asit gösterilmez. Bugün `PlayerHealth.Kill()` öldüreni bilmiyor; tek kişilik koda öldüreni bildiren bir okuma özelliği eklenir.
- **Hayaletler hiçbir zaman yerel platformların ve tehlikelerin önüne geçmez:** sıralamada hepsinin arkasında çizilir.
- **Görünüm: A, kesik kontur, renk kâğıt beyazı `#FFF6E0` (2026-11-23, kullanıcı).** İçi boş, ~5 px'e denk kalınlıkta kesik çizgi (mockup'ta 14 dolu / 9 boş), nesnenin collider şeklini izler. Tehlikede aynı kontur, ölüm anındaki pozda donmuş. Hayaletin kendi rengi (#8CD9FF) değişmiyor. Not: halkaların krem bantlarında zayıf kalabilir; ilk gamepad testinde bakılacak.
- Görünüm gerçek platformdan farklı olacak; yönler `docs/ui-research/hayalet-platform/` mockup'ında (A kesik kontur, B tek mürekkep baskı, C çapraz tarama, D yalnızca temas izi). Not: hayaletin bugünkü rengi #8CD9FF gökyüzüne (#71BAEA) çok yakın.

B'de hayalet bugünkü gibi rakibin dünya konumunda çizilir. Snapshot taşıyıcı platformun kimliğini ve **rakibin dünyasındaki** konumunu/dönüşünü taşır. Alıcı o platformun hayalet görüntüsünü bu konumda çizer. Hayalet ile hayalet platform aynı dünyadan geldiği için hep üst üste durur, tetiklenmemiş platform sorunu da kalkar. Aşağıdaki kimlik (`SceneObjectId`, registry) ve codec parçaları B'de de geçerli. Yerel offset interpolasyonu ve bağlanma geçişi B'de gerekmez.

### Seçenek A (seçilmedi, kayıt için): hayaleti senin dünyandaki platforma göre çiz

Rakip bir platformun üstündeyse (ya da hareketli bir duvara tutunuyorsa) snapshot, dünya konumunun yanında **"hangi platform + platforma göre konum"** bilgisini de taşır. Alıcı hayaleti **kendi dünyasındaki aynı platforma** göre yerleştirir. Hayalet senin platformunun üstünde durur, onunla birlikte taşınır. Rakip platformdan ayrılınca hayalet kısa bir geçişle dünya konumuna döner.

Bu, "hayalet dünyana dokunmaz" kuralını bozmaz: dünyaya hiçbir şey yazılmaz, yalnızca çizim değişir.

#### Parçalar

1. **Kararlı nesne kimliği: `Multiplayer/Ghost/SceneObjectId.cs`** (saf C# + küçük Unity yardımcısı). Bir `Transform`'un sahne kökünden yolunu **kardeş sırasıyla** birlikte (`"RoomNew-11:4/DynamicContent:2/Platform:7"`) FNV-1a 32 bit ile özetler. İki oyuncu aynı build'i ve aynı `NewScene`'i oynadığı için yol iki tarafta aynı. Aynı adlı kardeşler kardeş sırasıyla ayrılır. Yalnızca sahne yüklemesinde var olan nesneler için geçerli (çalışırken yaratılanlar kimlik almaz, düz dünya konumuna düşer).
2. **`Multiplayer/Ghost/MovingSurfaceRegistry.cs`:** sahne yüklenince bütün `PlatformController` ve `TriggeredPlatformController`'ları (`FindObjectsInactive.Include`) ve hareketli node `Transform`'larını kimlikleriyle bir sözlüğe koyar. Çakışmada uyarı loglar ve o kimliği kullanmaz.
3. **Snapshot'a alan:** `SnapshotMessage`'a `uint? CarrierId` + `Vector2 CarrierLocal`. Gönderen tarafta `SnapshotBuilder`/`LocalPlayerBroadcaster`, `KinematicPhysicsHandler.CurrentCarrier` (zaten public, yerde + duvarda taşıyan yüzeyi veriyor) boş değilse kimliği ve `carrierTransform.InverseTransformPoint(position)` değerini yazar. Dönen platformlar da doğru çalışsın diye yerel uzayda tutulur. `RaceMessageCodec`'e bayrak biti + alanlar, `RaceProtocol.Version` 3 → 4 (iki oyuncu da yeni build'e geçmeli), `RaceMessageCodecTests` güncellenir.
4. **Alıcı (`GhostInterpolator` + `GhostView`):**
   - Ardışık iki snapshot aynı `CarrierId`'yi taşıyorsa **yerel konum** interpole edilir, sonra yerel platformun **o anki** transform'uyla dünyaya çevrilir (`TransformPoint`). Platform kendi hızında akar, hayalet onunla birlikte gider.
   - Kimlik registry'de yoksa ya da platform kapalıysa (uyuyan oda): dünya konumu, bugünkü gibi.
   - **Bağlanma/ayrılma geçişi:** taşıyıcı değişince (platforma iniş, zıplayıp ayrılma, platformdan platforma geçiş) son çizilen konumdan yeni hedefe `GhostTiming.CarrierBlendSeconds` (başlangıç 0.15 sn) içinde yumuşak geçilir, ani sıçrama olmaz. Fark `CarrierSnapDistance`'tan (başlangıç 6 birim) büyükse geçiş yerine ışınlanma gibi yer değiştirilir (uzun kayma görünmesin). Değerler `GhostTiming` sabiti; his kararı insanın.
   - `Teleported` biti gelince taşıyıcı geçişi de sıfırlanır.
5. **Kanca ipleri (aynı mekanizma):** rakip **hareketli bir node'a** tutunmuşsa ipin ucu da kayar. `SnapshotMessage`'ın `LeftAnchor`/`RightAnchor`'ına aynı yöntemle isteğe bağlı node kimliği + yerel nokta eklenir. Gönderen `PlayerController.LeftAnchorNode`/`RightAnchorNode`'u (zaten public) okur. `GhostArms` ipi yerel node'a bağlar. Bu adım ayrı task, platform düzeltmesi önce.

**Bilinen sınır:** Rakip senin dünyanda henüz tetiklenmemiş bir `TriggeredPlatformController`'ın üstündeyse hayalet senin duran platformunun üstünde yerinde koşar. Zıplayınca gerçek konumuna geçer, bu uzaksa ışınlanır gibi yer değiştirir. Bunu çözmenin tek yolu rakibin platformunu da hayalet olarak çizmek (yarı saydam platform kopyası). Gerekirse sonraya.

### Kabul

- EditMode: `SceneObjectId` (aynı yol aynı kimlik, kardeş sırası ayırt eder), codec gidiş-dönüş (taşıyıcılı/taşıyıcısız, eski sürüm reddedilir), `GhostInterpolator` taşıyıcılı örnekleme (iki örnek aynı taşıyıcıda: yerel offset interpole edilir, verilen platform transform'una göre dünya konumu), taşıyıcı değişiminde geçiş ve eşik üstü ışınlanma.
- Multiplayer Play Mode (iki sanal oyuncu) ile elle: bir pencerede oyuncu ölüp platform fazını kaydırır, diğerinde hayalet platformun üstünde durur ve onunla gider. Önce-sonra kısa kayıt ya da iki ekran görüntüsü.
- `systems/multiplayer.md` "Mesajlar" ve hayalet bölümleri güncellenir, "Son kod doğrulaması" tarihi yenilenir.
- Arkadaşla Steam üzerinden tekrar denenir (iki build de protokol 4).

---

## Önerilen task'lar

| Kod | Task | Kategori | Önerilen gün |
|---|---|---|---|
| MPF1 | Ceza sonrası oda sıfırlama: `RoomDoor.ResetToInitial`, `RoomKeyManager.ResetForRoomReentry`, `RoomStateReset`, `PenaltyHandler` çağrısı | `#code` | 2026-11-28 |
| MPF2 | Ceza oda sıfırlama PlayMode testi (oda 11 ve 8 senaryoları) + doküman | `#test` | 2026-11-28 |
MPF1–2 `TASKS.md`'ye eklendi (2026-11-28; o günkü T18 ve DE11 ile 4 task).

### Hayalet task'ları (seçenek B + kontur; 2026-11-23 TASKS.md'ye eklendi)

| Kod | Task | Kategori | Gün |
|---|---|---|---|
| MPG1 | **Kimlik ve kayıt:** `SceneObjectId` (kardeş sıralı yol → FNV-1a), `MovingSurfaceRegistry`: bütün `PlatformController` + `TriggeredPlatformController` (hareketli platformlar, `Death` tehlikeleri, hareketli node'lar ve hareketli launch pad'ler bu sürücüyü kullanıyor), çakışmada uyarı; EditMode testleri | `#code` | 2026-11-29 |
| MPG2 | **Ağ verisi:** `SnapshotMessage`'a isteğe bağlı taşıyıcı (kimlik + gönderenin dünyasındaki konum/dönüş; `KinematicPhysicsHandler.CurrentCarrier`) ve sol/sağ tutunulan node (kimlik + poz; `LeftAnchorNode`/`RightAnchorNode`); ölüm için öldüreni bildiren mesaj (kimlik + poz). Tek kişilik koda yalnız ekleme: `PlayerHealth`'e öldüreni bildiren okuma özelliği (`DeathZone` `Kill`'den önce yazar). Codec, `RaceProtocol.Version` 3 → 4, `RaceMessageCodecTests` | `#code` | 2026-11-29 |
| MPG3 | **Kontur görünümü:** `GhostOutlineView`: yerel nesnenin collider şeklinden kâğıt beyazı kesik kontur (kesik çizgi shader'da, doku değil), gelen pozda çizilir ve interpole edilir; platform/node: ~0.1 sn giriş, ayrılınca ~0.4 sn kalıp solma, yerel nesneyle arası ~0.25 birimden azsa çizilmez; tehlike: ölüm pozunda donmuş ~0.6 sn. **Bütün yerel platform ve tehlikelerin arkasında** sıralanır. Değerler `MultiplayerUISettingsSO` ya da yeni bir SO'da; saf zamanlama mantığına EditMode testi | `#code` | 2026-11-29 |
| MPG4 | **İp ucu:** `GhostArms` ipi, rakip hareketli node'a tutunuyorsa gelen node pozuna (konturun ortasına) bağlar; Multiplayer Play Mode çekimi | `#code` | 2026-11-29 |
| MPG5 | **Hayalet kontur testi:** Multiplayer Play Mode'da bir pencerede ölüp platform fazını kaydır, diğerinde kontur hayaletin altında mı, tehlike ölümde görünüyor mu, gerçek nesnelerin arkasında mı, krem halka bantlarında okunuyor mu; sonra arkadaşla Steam'de (iki build protokol 4); notlar `feel-notlari.md`'ye | `#test` | 2026-11-30 |

2026-11-29 boş, 4 task. MPG5 2026-11-30'a (tampon önerdiğim gün) düşer; tampon gün olarak 2026-12-01 önerilir.
