# MP 2: Yarış kuralları (`RaceSession`) — Uygulama planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Yarış ve birlikte modlarının kurallarını (bayrak, 3'te ceza, rakibin odası, bitiş, ilerleme senkronu) taşıyıcıdan ve Unity'den bağımsız, saf C# bir `RaceSession` sınıfında toplamak; EditMode testleriyle.

**Architecture:** `RaceSession` ağ bilmez: yerel olaylar için çağrılan metotlar (`OnLocalRoomEntered`, `OnLocalFlagReached`, `OnLocalFinished`) karşıya gidecek mesajları **liste olarak döndürür**; karşıdan gelen mesajlar `Receive(IRaceMessage)` ile verilir ve sonuçları C# olaylarıyla (`PenaltyReceived` vb.) bildirilir. MP 4+ bu listeleri `RaceMessageCodec` + `IRaceTransport` ile gönderir. Böylece kurallar ağ olmadan, deterministik test edilir.

**Tech Stack:** C#, NUnit EditMode, Unity Pipeline CLI.

**Spec:** [`docs/systems/multiplayer.md`](../../systems/multiplayer.md) (Kararlar, Mesajlar, Oyuna bağlanma → Ceza akışı)

## Global Constraints

- Namespace `New_Scripts.Multiplayer.Session`, dosyalar `Assets/Scripts/New-Scripts/Multiplayer/Session/`. `UnityEngine` kullanılmaz (saf C#).
- Sabitler `RaceRules` içinde: `FlagsPerPenalty = 3`, `FirstRoomId = 1`.
- Her oyuncu her odanın bayrağını **bir kez** alır (ceza ile geri gönderilse bile).
- 3. bayrakta sayaç **0'a döner**; rakip ilk odadaysa (ya da odası henüz bilinmiyorsa) ceza **boşa gider** ama sayaç yine sıfırlanır.
- Birlikte modunda bayrak sayılmaz, ceza yok; oda ve bitiş bildirimleri sürer.
- Mevcut mesaj tipleri (MP 1) değişmez; `RaceProtocol.Version` 1 kalır.
- Tek kişilik koda dokunulmaz; EditMode tümü ve PlayMode 36/36 yeşil kalır.
- Commit yalnız bu planın dosyalarıyla, `feature/mp2-race-session` dalında; push yok.

## Kararlar (spec'in açık bıraktığı yerler)

- **Ceza hedefini alıcı belirler.** Gönderen `PenaltyMessage.TargetRoomId`'ye rakibin bildiği son odasının bir öncesini yazar, ama alıcı **cezayı uyguladığı an** kendi odasının bir öncesine gider (`TryGetPenaltyTarget`). `TargetRoomId` yalnızca bilgi. Alıcı ilk odadaysa ceza yok sayılır. *(Gözden geçirme sonrası: gönderen cezayı artık her zaman yollar; `PenaltyReceived` argümansız.)*
- **Rakibin odası yalnız güvenilir mesajlardan** (`RoomEntered`, `ProgressSync`) güncellenir, `Snapshot.RoomId`'den değil: güvenilmez ve güvenilir kanal birbirine göre sıralı değil (MP 1 sözleşmesi), geç gelen bir snapshot odayı geri alabilir.
- **Kazanan:** MP 2 yalnız iki tarafın bitişini kaydeder (`LocalFinished`, `OpponentFinished`, süreler). Kazananın ekranda gösterilmesi MP 7'de.

## Review Focus

1. **Aynı odanın bayrağına ikinci kez dokunmak** (ceza ile geri gelince ya da trigger iki kez tetiklenince) → mesaj yok, sayaç değişmez. Test: Task 1.
2. **Rakibin odası bilinmezken 3. bayrak** (bağlanır bağlanmaz, `RoomEntered` gelmeden) → sayaç sıfırlanır; *gözden geçirme sonrası:* `Penalty` yine gönderilir, boşa gidip gitmediğine alıcı karar verir. Test: Task 1.
3. **Ceza, alıcı ilk odadayken gelir** → `PenaltyReceived` tetiklenmez. Test: Task 2.
4. **Bitişten sonra gelen olaylar** (yerel oyuncu bitirdikten sonra bayrak/oda) → yok sayılır, mesaj üretilmez. Test: Task 1.
5. **`ProgressSync` ile gelen sayaç ve oda** rakibin durumunu tamamen değiştirir (yeniden bağlanmada eski değer kalmaz); `CapturedRoomIds` null gelirse çökmez. Test: Task 2.

---

### Task 1: Kurallar ve yerel olaylar

**Files:**
- Create: `Assets/Scripts/New-Scripts/Multiplayer/Session/RaceRules.cs`
- Create: `Assets/Scripts/New-Scripts/Multiplayer/Session/RaceSession.cs`
- Test: `Assets/Tests/EditMode/Multiplayer/RaceSessionLocalTests.cs`

**Interfaces:**
- Consumes (MP 1): `New_Scripts.Multiplayer.Messages` → `RaceMode`, `IRaceMessage`, `RoomEnteredMessage`, `FlagCapturedMessage`, `PenaltyMessage`, `FinishedMessage`, `ProgressSyncMessage`.
- Produces:

```csharp
public static class RaceRules { public const int FlagsPerPenalty = 3; public const int FirstRoomId = 1; }
public sealed class RaceSession
{
    public RaceSession(RaceMode mode, int startRoomId = RaceRules.FirstRoomId);
    public RaceMode Mode { get; }
    public int LocalRoomId { get; }
    public int LocalFlagCount { get; }
    public IReadOnlyCollection<int> LocalCapturedRoomIds { get; }
    public bool LocalFinished { get; }
    public int OpponentRoomId { get; }          // 0 = henüz bilinmiyor
    public int OpponentFlagCount { get; }
    public bool OpponentFinished { get; }
    public float OpponentElapsedSeconds { get; }
    public IReadOnlyList<IRaceMessage> OnLocalRoomEntered(int roomId);
    public IReadOnlyList<IRaceMessage> OnLocalFlagReached(int roomId);
    public IReadOnlyList<IRaceMessage> OnLocalFinished(float elapsedSeconds);
    public ProgressSyncMessage CreateProgressSync(float elapsedSeconds);
}
```

Boş sonuç `Array.Empty<IRaceMessage>()`.

- [ ] **Step 1: Testleri yaz**

```csharp
[Test] public void RoomEntered_UpdatesLocalRoomAndSendsMessage()      // OnLocalRoomEntered(4) → LocalRoomId 4, tek RoomEnteredMessage{RoomId=4}
[Test] public void RoomEntered_SameRoomAgain_SendsNothing()           // aynı oda iki kez → ikincisi boş
[Test] public void FlagReached_Race_CountsAndSendsFlagCaptured()      // OnLocalFlagReached(2) → count 1, FlagCaptured{2,1}
[Test] public void FlagReached_SameRoomTwice_IsIgnored()              // ikincisi boş, count 1 (Review Focus 1)
[Test] public void ThirdFlag_SendsPenaltyForOpponentsPreviousRoom()   // rakip oda 7 (Receive RoomEntered 7) → 3. bayrakta [FlagCaptured{..,3}, Penalty{6}], count 0
[Test] public void ThirdFlag_OpponentInFirstRoom_PenaltyWastedCounterResets()   // rakip oda 1 → yalnız FlagCaptured, count 0
[Test] public void ThirdFlag_OpponentRoomUnknown_PenaltyWastedCounterResets()   // hiç RoomEntered yok → yalnız FlagCaptured, count 0 (Review Focus 2)
[Test] public void SixFlags_SendTwoPenalties()                         // 6 farklı oda → iki Penalty, count 0
[Test] public void Coop_FlagReached_SendsNothingAndDoesNotCount()
[Test] public void LocalFinished_SendsFinishedOnce()                   // ikinci çağrı boş
[Test] public void AfterFinish_RoomAndFlagEventsAreIgnored()           // (Review Focus 4)
[Test] public void CreateProgressSync_ReflectsLocalState()             // oda, sayaç, alınmış odalar (sıralı), süre
```

Not: `ThirdFlag_*` testleri rakibin odasını `Receive(new RoomEnteredMessage{RoomId=7})` ile verir; bu yüzden Task 1'de `Receive`'in yalnız `RoomEnteredMessage` dalı da yazılır (Task 2 kalanını tamamlar).

- [ ] **Step 2: Derle, testlerin derlenmediğini gör** (`RaceSession` yok).
- [ ] **Step 3: `RaceRules` ve `RaceSession`'ı yaz** (alınmış odalar `HashSet<int>`; `CreateProgressSync` diziyi sıralı verir).
- [ ] **Step 4: Derle ve EditMode'u koş** — tümü yeşil.
- [ ] **Step 5: Commit** — `feat(mp): yaris kurallari ve yerel olaylar (RaceSession)`

---

### Task 2: Rakipten gelen mesajlar

**Files:**
- Modify: `Assets/Scripts/New-Scripts/Multiplayer/Session/RaceSession.cs`
- Test: `Assets/Tests/EditMode/Multiplayer/RaceSessionRemoteTests.cs`
- Modify: `docs/systems/multiplayer.md` (durum satırı, "Kod" bölümü, "Kararlar" altına yukarıdaki iki kural)

**Interfaces:**
- Consumes: Task 1'in `RaceSession`'ı.
- Produces:

```csharp
public void Receive(IRaceMessage message);          // Hello ve Snapshot yok sayılır
public event Action<int> PenaltyReceived;           // argüman: yerel oyuncunun gideceği oda (LocalRoomId - 1)
public event Action<int> OpponentRoomChanged;
public event Action<int> OpponentFlagCountChanged;
public event Action<float> OpponentFinishedRace;    // argüman: rakibin süresi
```

- [ ] **Step 1: Testleri yaz**

```csharp
[Test] public void RemoteRoomEntered_UpdatesOpponentRoomAndRaisesEvent()
[Test] public void RemoteSnapshot_DoesNotChangeOpponentRoom()          // Snapshot{RoomId=9} sonrası OpponentRoomId değişmez
[Test] public void RemoteFlagCaptured_UpdatesOpponentCount()
[Test] public void RemotePenalty_RaisesWithLocalPreviousRoom()        // yerel oda 5, Penalty{TargetRoomId=9} → PenaltyReceived(4)
[Test] public void RemotePenalty_LocalInFirstRoom_IsIgnored()         // (Review Focus 3)
[Test] public void RemotePenalty_InCoop_IsIgnored()
[Test] public void RemoteFinished_SetsOpponentFinishedAndRaises()     // süre 812.5 aynen
[Test] public void RemoteProgressSync_ReplacesOpponentState()         // önce RoomEntered 3 + FlagCaptured{..,2}, sonra ProgressSync{Room 8, Count 0} → 8 ve 0 (Review Focus 5)
[Test] public void RemoteProgressSync_NullCapturedRooms_DoesNotThrow()
[Test] public void TwoSessions_ThreeFlagsOverInMemoryTransport_DeliverPenalty()
// Uçtan uca: iki RaceSession + InMemoryTransport + RaceMessageCodec. B oda 6'ya girer, A üç bayrak alır,
// A'nın dönen mesajları kodlanıp gönderilir, B Poll'da decode edip Receive eder → B.PenaltyReceived(5).
```

- [ ] **Step 2: Derle, testlerin derlenmediğini / başarısız olduğunu gör.**
- [ ] **Step 3: `Receive` ve olayları tamamla.**
- [ ] **Step 4: Derle ve EditMode'u koş** — tümü yeşil.
- [ ] **Step 5: PlayMode'u koş** — 36/36 (Runtime değişmedi; `git diff --stat session-2026-11-05 -- Assets/Scripts/New-Scripts ':!Assets/Scripts/New-Scripts/Multiplayer*'` boş).
- [ ] **Step 6: `docs/systems/multiplayer.md`'yi güncelle** (durum "MP 1–2 kodda", Kod listesine `Session/`, Kararlar'a "ceza hedefini alıcı belirler" ve "rakibin odası yalnız güvenilir mesajlardan").
- [ ] **Step 7: Commit** — `feat(mp): rakip mesajlarini isleyen RaceSession.Receive`

## Bu aşamada bilerek olmayanlar

MonoBehaviour, sahne ya da Unity bağımlılığı; ceza sırasında oyuncunun taşınması (MP 6); kazananın gösterimi (MP 7); kayıt dosyası (MP 8).
