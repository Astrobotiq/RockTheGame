# MP 1: Multiplayer assembly, taşıyıcı arayüzü ve mesajlar — Uygulama planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tek kişilik koda dokunmadan, ayrı bir `RockTheGame.Multiplayer` assembly'sinde taşıyıcı arayüzünü, bellek içi taşıyıcıyı ve yarış mesajlarının byte'a yazılıp okunmasını kurmak; hepsi EditMode testleriyle.

**Architecture:** `New-Scripts/Multiplayer/` kendi asmdef'iyle `RockTheGame.Runtime`'dan ayrılır (Unity en yakın asmdef'i kullanır). Oyun kodu ileride yalnızca `IRaceTransport` ile konuşur; bu aşamada tek uygulaması aynı süreçte iki ucu bağlayan `InMemoryTransport`. Mesajlar `RaceMessageCodec` ile tip baytı + `BinaryWriter` (little-endian) olarak yazılır, bozuk veride istisna atmadan `false` döner.

**Tech Stack:** Unity 6000.3, C# (System.IO `BinaryWriter`/`BinaryReader`), NUnit EditMode testleri, Unity Pipeline CLI.

**Spec:** [`docs/systems/multiplayer.md`](../../systems/multiplayer.md) (Bileşenler, Mesajlar, Tek kişilik oyunun korunması)

## Global Constraints

- Namespace: `New_Scripts.Multiplayer` (alt klasörler alt namespace: `.Transport`, `.Messages`). Yorumlar Türkçe, isimler İngilizce.
- `RockTheGame.Runtime` **hiçbir koşulda** `RockTheGame.Multiplayer`'a referans vermez. Runtime asmdef'i bu aşamada değişmez.
- `RockTheGame.Multiplayer.asmdef`: `references: []`, `autoReferenced: false` (eski `Assembly-CSharp` ona bağlanmasın).
- `Send(mesaj, güvenilir)` imzası: spec'teki "güvenilir / güvenilmez" kanal ayrımı bu aşamada da arayüzde var.
- `Snapshot` kodlanmış hali ≤ 50 byte (spec: "~50 byte").
- Protokol sürümü sabiti `RaceProtocol.Version = 1`.
- Dash animasyonu `bool DashTrigger` olarak değil, her dash'te artan `byte DashCount` olarak gider (senior kararı, 6 Kasım: güvenilmez kanalda kaybolan tek paket dash'i kaçırmasın).
- Oyuncu adı en fazla 32 karakter; uzunsa kodlanırken kesilir.
- Her görevden sonra derleme temiz, EditMode yeşil. Son görevde PlayMode 36/36 da yeşil.
- Commit yalnızca o görevin dosyalarıyla (`git add <yollar>`), `feature/multiplayer` dalında. Çalışma ağacındaki ilgisiz değişiklikler (sahne, sorting layer, docs) eklenmez. Push yok.

## Review Focus

1. **Bozuk ya da kesik paket** (boş dizi, bilinmeyen tip baytı, yarım kalmış mesaj) → `TryDecode` `false` döner, istisna atmaz. Test: Task 2.
2. **Gönderen tamponu yeniden kullanırsa** alıcı yine gönderildiği andaki byte'ları görür. Test: Task 3.
3. **Bağlı değilken ya da koptuktan sonra `Send`** → `false`, istisna yok (ileride broadcaster kopma anında yollamaya devam edebilir). Test: Task 3.
4. **Türkçe karakterli ya da 32'den uzun oyuncu adı** → Türkçe ad aynen döner, uzun ad 32 karaktere kesilir. Test: Task 2.
5. **Olay işleyicisi içinden `Send`/`Disconnect`** (`MessageReceived` sırasında cevap yollamak) → "collection modified" hatası yok; cevap karşı tarafın bir sonraki `Poll`'unda gelir. Test: Task 3.

---

### Task 1: Ayrı assembly ve ayrılık güvencesi

**Files:**
- Create: `Assets/Scripts/New-Scripts/Multiplayer/RockTheGame.Multiplayer.asmdef`
- Modify: `Assets/Tests/EditMode/RockTheGame.Tests.EditMode.asmdef` (`references`'a `"RockTheGame.Multiplayer"`)
- Create: `Assets/Scripts/New-Scripts/Multiplayer/RaceProtocol.cs`
- Test: `Assets/Tests/EditMode/Multiplayer/AssemblySeparationTests.cs`

**Interfaces:**
- Produces: assembly `RockTheGame.Multiplayer`; `public static class RaceProtocol { public const ushort Version = 1; public const int MaxPlayerNameLength = 32; }` (namespace `New_Scripts.Multiplayer`).

- [ ] **Step 0: Dal aç** — `git switch -c feature/multiplayer` (çalışma ağacındaki commit'lenmemiş değişiklikler olduğu gibi kalır).

- [ ] **Step 1: Testi yaz**

```csharp
[Test] public void Runtime_DoesNotReferenceMultiplayer()
{
    var refs = typeof(New_Scripts.Player.PlayerController).Assembly.GetReferencedAssemblies();
    Assert.IsFalse(refs.Any(r => r.Name == "RockTheGame.Multiplayer"));
}
[Test] public void Multiplayer_LivesInItsOwnAssembly()
{
    Assert.AreEqual("RockTheGame.Multiplayer", typeof(New_Scripts.Multiplayer.RaceProtocol).Assembly.GetName().Name);
}
```

- [ ] **Step 2: Derle, testin derlenmediğini gör** — `unity command recompile`, `recompile_status` → `Editor.log`'da `RaceProtocol` bulunamadı (`error CS0234`).

- [ ] **Step 3: asmdef'i (Global Constraints'teki ayarlarla), `RaceProtocol`'ü yaz, test asmdef'ine referansı ekle.**

- [ ] **Step 4: Derle ve test et** — derleme `completed`, `unity command run_tests --mode EditMode` → 30/30 (28 + 2).

- [ ] **Step 5: Commit** — `feat(mp): multiplayer icin ayri assembly ve ayrilik testi`

---

### Task 2: Mesajlar ve codec

**Files:**
- Create: `Assets/Scripts/New-Scripts/Multiplayer/Messages/RaceMessages.cs` (enum'lar + mesaj sınıfları)
- Create: `Assets/Scripts/New-Scripts/Multiplayer/Messages/RaceMessageCodec.cs`
- Test: `Assets/Tests/EditMode/Multiplayer/RaceMessageCodecTests.cs`

**Interfaces:**
- Consumes: `RaceProtocol.MaxPlayerNameLength` (Task 1).
- Produces (namespace `New_Scripts.Multiplayer.Messages`):
  - `enum RaceMessageType : byte { Hello = 1, Snapshot = 2, RoomEntered = 3, FlagCaptured = 4, Penalty = 5, ProgressSync = 6, Finished = 7 }`
  - `enum RaceMode : byte { Race = 0, Coop = 1 }`
  - `interface IRaceMessage { RaceMessageType Type { get; } }`
  - `sealed class` mesajlar, public alanlarla:
    - `HelloMessage { ushort ProtocolVersion; RaceMode Mode; Guid RaceId; string PlayerName; }`
    - `SnapshotMessage { uint Sequence; int RoomId; Vector2 Position; bool FacingRight; float Speed; bool IsGrounded; float VerticalVelocity; bool IsSwinging; bool IsDashing; bool IsWallClimbing; bool IsWallSliding; byte DashCount; bool Teleported; Vector2? LeftAnchor; Vector2? RightAnchor; }`
    - `RoomEnteredMessage { int RoomId; }`
    - `FlagCapturedMessage { int RoomId; int NewCount; }`
    - `PenaltyMessage { int TargetRoomId; }`
    - `ProgressSyncMessage { int RoomId; int FlagCount; int[] CapturedRoomIds; float ElapsedSeconds; }`
    - `FinishedMessage { float ElapsedSeconds; }`
  - `static class RaceMessageCodec { byte[] Encode(IRaceMessage message); bool TryDecode(byte[] data, out IRaceMessage message); }`

- [ ] **Step 1: Testleri yaz**

```csharp
// Her mesaj tipi için gidiş-dönüş: Encode → TryDecode true → alanlar eşit.
[Test] public void Hello_RoundTrips()            // Version 1, Coop, sabit Guid, "Çağrı Öz"
[Test] public void Snapshot_RoundTrips_WithAnchors()   // bütün bool'lar farklı değerlerde, DashCount 255, iki anchor dolu
[Test] public void Snapshot_RoundTrips_NullAnchors()   // LeftAnchor == null, RightAnchor == null kalır
[Test] public void Snapshot_EncodedSize_IsAtMost50Bytes()  // iki anchor dolu → Assert.LessOrEqual(bytes.Length, 50)
[Test] public void RoomEntered_FlagCaptured_Penalty_Finished_RoundTrip()
[Test] public void ProgressSync_RoundTrips([Values(0, 3)] int capturedCount)  // boş dizi de boş döner, null değil
[Test] public void PlayerName_LongerThan32_IsTruncated()  // 40 karakter → 32 karakter, ilk 32'si aynı
[Test] public void TryDecode_Empty_ReturnsFalse()          // new byte[0] ve null
[Test] public void TryDecode_UnknownType_ReturnsFalse()    // new byte[] { 99, 0, 0 }
[Test] public void TryDecode_Truncated_ReturnsFalseWithoutThrowing()
// Hello ve ProgressSync kodlanmış halinin 1..Length-1 uzunluğundaki her öneki için
// Assert.DoesNotThrow ve Assert.IsFalse(TryDecode(...)).
```

Float ve Vector2 karşılaştırması tam eşitlik (`Assert.AreEqual`); yazılan değer aynen okunur.

- [ ] **Step 2: Derle, testlerin derlenmediğini gör** (`RaceMessageCodec` yok).

- [ ] **Step 3: Mesajları ve codec'i yaz.**
  - Biçim: ilk bayt `RaceMessageType`, ardından alanlar tanımlandıkları sırayla `BinaryWriter` ile.
  - `SnapshotMessage`'ın 7 bool'u tek bir bayrak baytına, `DashCount` ayrı bir bayta, anchor'ların varlığı ikinci bir bayta (bit 0 sol, bit 1 sağ); yalnızca dolu anchor'lar yazılır.
  - `Guid` 16 bayt (`ToByteArray`), `string` `BinaryWriter.Write(string)` (UTF-8, uzunluk önekli), `int[]` önce uzunluk sonra elemanlar.
  - `TryDecode`: `EndOfStreamException`/`IOException`/`ArgumentException`'ı yakalayıp `false`; veri sonuna kadar okunmamışsa da `false`. ProgressSync dizi uzunluğu negatif ya da kalan bayttan büyükse `false` (dev dizi ayırma olmasın).

- [ ] **Step 4: Derle ve test et** — EditMode 30 + Task 2 testleri, hepsi yeşil.

- [ ] **Step 5: Commit** — `feat(mp): yaris mesajlari ve byte codec'i`

---

### Task 3: Taşıyıcı arayüzü ve bellek içi taşıyıcı

**Files:**
- Create: `Assets/Scripts/New-Scripts/Multiplayer/Transport/IRaceTransport.cs`
- Create: `Assets/Scripts/New-Scripts/Multiplayer/Transport/InMemoryTransport.cs`
- Test: `Assets/Tests/EditMode/Multiplayer/InMemoryTransportTests.cs`
- Modify: `docs/systems/multiplayer.md` (durum satırı ve "Kod" bölümü)

**Interfaces:**
- Produces (namespace `New_Scripts.Multiplayer.Transport`):

```csharp
public enum TransportState { Disconnected, Connecting, Connected }
public enum DisconnectReason { LocalRequest, RemoteLeft, Timeout, Rejected }
public interface IRaceTransport : IDisposable
{
    TransportState State { get; }
    event Action Connected;
    event Action<DisconnectReason> Disconnected;
    event Action<byte[]> MessageReceived;
    void Host();
    void Join(string address);          // UTP: "ip:port", Steam: SteamID; InMemory yok sayar
    void Disconnect();
    bool Send(byte[] payload, bool reliable);   // bağlı değilse false
    void Poll();                         // olaylar yalnızca burada, ana thread'de tetiklenir
}
public sealed class InMemoryTransport : IRaceTransport
{
    public static (InMemoryTransport host, InMemoryTransport client) CreatePair();
}
```

Davranış: `Host()` ve `Join()` çağrılınca uç `Connecting` olur; **iki uç da** çağırdıktan sonra her uç kendi `Poll`'unda `Connected` olur ve olayı bir kez tetikler. `Send` payload'ı kopyalayıp karşı tarafın kuyruğuna koyar; mesaj yalnızca alıcının `Poll`'unda, gönderim sırasıyla gelir (güvenilir/güvenilmez fark etmez). `Disconnect()` → yerel uç bir sonraki `Poll`'da `Disconnected(LocalRequest)`, karşı uç `Disconnected(RemoteLeft)`; ikisi de `Disconnected` durumuna geçer, bekleyen mesajlar atılır. `Poll` kuyruğun anlık kopyası üzerinde döner (Review Focus 5). `Dispose` = `Disconnect`.

- [ ] **Step 1: Testleri yaz**

```csharp
[Test] public void HostAndJoin_BothConnectAfterPoll()          // Poll'dan önce Connecting, sonra Connected; olay bir kez
[Test] public void OnlyHost_StaysConnecting()                  // client Join çağırmadan iki Poll → Connecting
[Test] public void Message_ArrivesOnlyAfterReceiverPolls()     // Send sonrası receiver Poll'dan önce 0 mesaj, sonra 1
[Test] public void Messages_ArriveInSendOrder([Values(true, false)] bool reliable)  // 1,2,3 sırayla
[Test] public void Send_CopiesPayload()                         // gönderdikten sonra tamponu değiştir → alıcı eski byte'ları görür
[Test] public void Send_BeforeConnect_ReturnsFalse()
[Test] public void Disconnect_RaisesLocalRequestAndRemoteLeft() // iki uç Disconnected, sebepler doğru
[Test] public void Send_AfterDisconnect_ReturnsFalse()
[Test] public void ReplyInsideHandler_IsDeliveredOnNextPoll()  // client MessageReceived içinde Send yapar; istisna yok, host sonraki Poll'da alır
[Test] public void CodecOverTransport_EndToEnd()                // host Hello yollar, client TryDecode ile aynı Hello'yu okur
```

- [ ] **Step 2: Derle, testlerin derlenmediğini gör.**

- [ ] **Step 3: `IRaceTransport` ve `InMemoryTransport`'u yaz.**

- [ ] **Step 4: Derle ve test et** — EditMode tümü yeşil.

- [ ] **Step 5: Tek kişilik oyunu doğrula** — `unity command run_tests --mode playmode --async_tests true`, `test_status` bitince `TestResults.xml` → 36/36 geçti (Runtime değişmediği için beklenen; değişmediğini `git diff --stat -- Assets/Scripts/New-Scripts ':!Assets/Scripts/New-Scripts/Multiplayer'` boş çıkarak da gösterir).

- [ ] **Step 6: `docs/systems/multiplayer.md`'yi güncelle** — durum satırı "MP 1 kodda (6 Kasım 2026): assembly, `IRaceTransport`, `InMemoryTransport`, mesajlar"; Bileşenler tablosunun altına "Kod" bölümü (dosya yolları), "Son kod doğrulaması: 2026-11-06".

- [ ] **Step 7: Commit** — `feat(mp): tasiyici arayuzu ve bellek ici tasiyici`

---

## Bu aşamada bilerek olmayanlar

`RaceSession` kuralları (MP 2), Unity Transport (MP 3), herhangi bir MonoBehaviour ya da sahne değişikliği. `Runtime` asmdef'ine referans da MP 4'te (broadcaster `PlayerController`'ı okuyacağı zaman) eklenecek.
