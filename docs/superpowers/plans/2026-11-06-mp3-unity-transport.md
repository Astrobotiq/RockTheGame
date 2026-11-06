# MP 3: Unity Transport taşıyıcısı ve geliştirici başlatıcısı — Uygulama planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `IRaceTransport`'un ilk gerçek uygulamasını (Unity Transport, localhost / yerel ağ / doğrudan IP) yazmak ve aynı bilgisayarda iki kopyayı tek sahne değişikliği yapmadan birbirine bağlayan, yalnız editörde çalışan bir geliştirici başlatıcısı eklemek.

**Architecture:** `UnityTransportAdapter`, bir `NetworkDriver` ve iki pipeline (`ReliableSequenced`, `UnreliableSequenced`) ile tek bağlantılı host/istemci kurar; olayları yalnız `Poll()` içinde tetikler, MP 1'de `IRaceTransport` XML belgesine yazılan sözleşmeye uyar. Geliştirici başlatıcısı `NewScene`'e konmaz: editörde bir menü anahtarı açıksa Play Mode başında `RuntimeInitializeOnLoadMethod` ile kökte bir `GameObject` oluşturur, rolü (host/istemci) Multiplayer Play Mode oyuncusundan ya da anahtardan alır, bağlanınca `Hello` + `ProgressSync` yollar ve gelen mesajları loglar.

**Tech Stack:** `com.unity.transport` 2.6.x (NGO ve Relay yok), `com.unity.multiplayer.playmode` (yalnız editör aracı), NUnit, Unity Pipeline CLI.

**Spec:** [`docs/systems/multiplayer.md`](../../systems/multiplayer.md) (Bileşenler: `UnityTransportAdapter`; Tek kişilik oyunun korunması; Testler)

## Global Constraints

- Ayarlar: `disconnectTimeoutMS: 5000`, `heartbeatTimeoutMS: 500` (oyun); testlerde bağlanma denemesi kısa tutulur. Varsayılan port `7777`.
- `IRaceTransport` sözleşmesi (MP 1 XML belgesi) birebir: olaylar yalnız `Poll`'da; `Send` kopyalar ve bağlı değilken `false`; yerel `Disconnect` → bir sonraki `Poll`'da `Disconnected(LocalRequest)`; `Host`/`Join` hatası istisna değil, sonraki `Poll`'larda `Disconnected(Timeout|Rejected)`; karşı uç ayrılınca dinlemeye dönmez.
- Güvenilir kanalda `NetworkSendQueueFull` gelirse mesaj kaybolmaz: yerel kuyruğa alınır, sonraki `Poll`'da tekrar denenir.
- Host yalnız **bir** istemci kabul eder; ikinci bağlantı hemen kapatılır.
- `NewScene.unity`'ye ve tek kişilik koda dokunulmaz. Başlatıcı yalnız `UNITY_EDITOR`'da derlenir ve menü anahtarı kapalıyken hiçbir şey oluşturmaz.
- `RockTheGame.Multiplayer.asmdef`'e `Unity.Networking.Transport`, `Unity.Collections` referansları eklenir. `RockTheGame.Runtime.asmdef` değişmez.
- EditMode tümü ve PlayMode 36/36 yeşil; commit yalnız bu planın dosyaları, `feature/mp3-unity-transport` dalında; push yok.

## Kararlar

- **UTP adaptörü ayrı asmdef'e konmaz**, `RockTheGame.Multiplayer` içinde durur. Spec'in "taşıyıcılar ayrı asmdef" kuralının amacı Steam kütüphanesini ana koddan uzak tutmak; UTP bir Unity paketi. Steam adaptörü (MP 9) yine ayrı asmdef'te olacak.
- Gönderimden sonra `ScheduleFlushSend` hemen çağrılır (senior notu: yoksa bir kare gecikme).
- **Gamepad:** Multiplayer Play Mode'da iki pencerenin aynı gamepad'i okuyup okumadığı bu aşamada denenip `multiplayer.md`'ye yazılır; çözüm gerekiyorsa MP 4'te ele alınır.

## Review Focus

1. **Kapalı porta bağlanmak** → istisna yok; birkaç `Poll` sonra `Disconnected(Timeout)`. Test: Task 1.
2. **Host'a ikinci istemci** → ilk bağlantı bozulmaz, ikinci reddedilir. Test: Task 1.
3. **Aynı portta ikinci host** (port dolu) → istisna yok, `Disconnected(Rejected)`. Test: Task 1.
4. **Kuyruk dolarken güvenilir mesaj yığını** (örn. 100 mesaj art arda) → hepsi sırayla ulaşır. Test: Task 1.
5. **Başlatıcı anahtarı kapalıyken Play Mode** → sahnede başlatıcı nesnesi yok, tek kişilik oyun aynen. Test: Task 2.

---

### Task 1: Paketler ve `UnityTransportAdapter`

**Files:**
- Modify: `Packages/manifest.json`, `Packages/packages-lock.json` (Package Manager API ile: `com.unity.transport`, `com.unity.multiplayer.playmode`)
- Modify: `Assets/Scripts/New-Scripts/Multiplayer/RockTheGame.Multiplayer.asmdef`
- Create: `Assets/Scripts/New-Scripts/Multiplayer/Transport/UnityTransportAdapter.cs`
- Test: `Assets/Tests/EditMode/Multiplayer/UnityTransportAdapterTests.cs` (UTP sürücüsü EditMode'da çalışmazsa `Assets/Tests/PlayMode/Multiplayer/`, ledger'a ruling)

**Interfaces:**
- Consumes: `IRaceTransport`, `TransportState`, `DisconnectReason` (MP 1).
- Produces:

```csharp
public struct UnityTransportSettings
{
    public ushort Port;                 // varsayılan 7777
    public int DisconnectTimeoutMS;     // 5000
    public int HeartbeatTimeoutMS;      // 500
    public int ConnectTimeoutMS;        // 1000
    public int MaxConnectAttempts;      // 10
    public static UnityTransportSettings Default { get; }
}
public sealed class UnityTransportAdapter : IRaceTransport
{
    public UnityTransportAdapter(UnityTransportSettings settings);
    // Join(address): "ip" ya da "ip:port"; port yoksa settings.Port
}
```

- [ ] **Step 1: Paketleri ekle** (`Client.AddAndRemove`), asmdef referanslarını yaz, derlemenin temiz olduğunu gör.
- [ ] **Step 2: Testleri yaz** — yardımcı `PumpUntil(Func<bool>, int maxIterations)` iki ucu `Poll` eder, araya `Thread.Sleep(5)` koyar; her test kendi boş portunu seçer.

```csharp
[Test] public void HostAndJoin_Localhost_BothConnect()
[Test] public void ReliableMessages_ArriveInOrder()                    // 1..20
[Test] public void UnreliableMessage_Arrives()                          // localhost'ta kayıp beklenmez
[Test] public void Send_CopiesPayload()
[Test] public void Send_BeforeConnect_ReturnsFalse()
[Test] public void Disconnect_RaisesLocalRequestAndRemoteLeft()
[Test] public void Join_ClosedPort_RaisesTimeoutWithoutThrowing()       // Review Focus 1
[Test] public void SecondClient_IsRejected_FirstStaysConnected()        // Review Focus 2
[Test] public void Host_PortInUse_RaisesRejectedWithoutThrowing()       // Review Focus 3
[Test] public void HundredReliableMessages_AllArriveInOrder()           // Review Focus 4
[Test] public void CodecAndRaceSession_OverUnityTransport_DeliverPenalty()  // MP 2'deki uçtan uca testin UTP sürümü
```

- [ ] **Step 3: Derle, testlerin derlenmediğini gör.**
- [ ] **Step 4: `UnityTransportSettings` ve `UnityTransportAdapter`'ı yaz.**
- [ ] **Step 5: Derle, EditMode'u koş** — tümü yeşil.
- [ ] **Step 6: Commit** — `feat(mp): Unity Transport tasiyicisi`

---

### Task 2: Geliştirici başlatıcısı (yalnız editör)

**Files:**
- Create: `Assets/Scripts/New-Scripts/Multiplayer/Dev/MultiplayerDevLauncher.cs` (`#if UNITY_EDITOR`)
- Create: `Assets/Scripts/New-Scripts/Multiplayer/Dev/MultiplayerDevSettings.cs` (menü anahtarı ve rol; `EditorPrefs`/`SessionState` ya da MPPM oyuncu etiketi — hangisinin kopyalar arasında doğru ayrıldığı uygulamada denenir, ledger'a ruling)
- Test: `Assets/Tests/EditMode/Multiplayer/MultiplayerDevLauncherTests.cs`
- Modify: `docs/systems/multiplayer.md` (durum, Kod listesi, "Editörde nasıl test edilir" bölümü, gamepad bulgusu)

**Interfaces:**
- Consumes: `UnityTransportAdapter`, `RaceSession`, `RaceMessageCodec`.
- Produces:

```csharp
public enum DevRole { Off, Host, Client }
public static class MultiplayerDevSettings
{
    public static DevRole Role { get; set; }          // menü: "Rock The Game/Multiplayer/Dev: Kapalı|Host|Client"
    public static string Address { get; set; }        // varsayılan "127.0.0.1"
    public static RaceMode Mode { get; set; }         // varsayılan Race
}
public sealed class MultiplayerDevLauncher : MonoBehaviour
{
    public RaceSession Session { get; }
    public IRaceTransport Transport { get; }
    public static MultiplayerDevLauncher SpawnIfEnabled();   // Role Off ise null döner, nesne oluşturmaz
}
```

Davranış: kökte `"[MP Dev Launcher]"` nesnesi, `DontDestroyOnLoad`; `Update`'te `Poll`; bağlanınca `Hello` (`RaceProtocol.Version`, mod, sabit dev yarış kimliği, makine adı) + `ProgressSync`; gelen her mesaj tip ve özetiyle `Debug.Log` (`[MP]` önekli); `OnDestroy`'da `Dispose`.

- [ ] **Step 1: Testleri yaz**

```csharp
[Test] public void SpawnIfEnabled_RoleOff_CreatesNothing()             // Review Focus 5
[Test] public void SpawnIfEnabled_Host_CreatesRootObjectWithSession()
[TearDown] → oluşan nesneyi DestroyImmediate, Role'ü eski değerine döndür
```

- [ ] **Step 2: Derle, testlerin derlenmediğini gör.**
- [ ] **Step 3: Ayarları ve başlatıcıyı yaz** (menü öğeleri dahil).
- [ ] **Step 4: Derle, EditMode'u koş.**
- [ ] **Step 5: Elle duman testi (ajan):** Rol Host iken Play Mode'a gir, Console'da `[MP] Host dinliyor :7777` logunu gör; aynı anda EditMode'dan ikinci bir `UnityTransportAdapter` ile `127.0.0.1`'e bağlanıp başlatıcının `Hello` aldığını logda gör. Rol Kapalı iken Play Mode'da `[MP Dev Launcher]` nesnesi olmadığını doğrula.
- [ ] **Step 6: PlayMode 36/36** ve `Multiplayer` dışı `New-Scripts` farkının boş olduğunu göster.
- [ ] **Step 7: Belgeyi güncelle** — "Editörde nasıl test edilir": Multiplayer Play Mode'da ikinci oyuncuyu aç, ana editörde `Dev: Host`, kopyada `Dev: Client`, ikisinde Play; gamepad bulgusu.
- [ ] **Step 8: Commit** — `feat(mp): editor icin gelistirici baslaticisi`

## Bu aşamada bilerek olmayanlar

Hayalet ve oyuncu durumunun yollanması (MP 4), bayrak ve ceza akışı (MP 5–6), lobi (MP 7), Steam (MP 9), kopmada 30 sn bekleme (MP 8).
