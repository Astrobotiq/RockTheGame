# Çok oyunculu

*GDD §9. Durum: **tasarım onaylandı; MP 1–4 kodda** (mesajlar, `RaceSession` kuralları, bellek içi ve Unity Transport taşıyıcıları, editör başlatıcısı, rakip hayaleti). Bayrak ve ceza henüz yok (MP 5–6). Son kod doğrulaması: 2026-11-06.*

## Tasarım niyeti

İki kişi aynı oyunu online ve aynı anda oynar. Rakibini ekranda görürsün ama o senin dünyana dokunmaz. "Beraber oynuyormuş" hissi verir, tek kişilik oyunun zorluğunu ve ritmini bozmaz.

**Tek kişilik oyun bu sistemden etkilenmez.** Bu, tasarımın birinci kuralıdır, diğer bütün kararlar ona uyar (bkz. "Tek kişilik oyunun korunması").

## Kararlar

| Konu | Karar |
|---|---|
| Etkileşim | Rakip bir **hayalet**: çarpışmaz, platformlara ve tehlikelere dokunmaz. Ağdan yalnızca birkaç olay geçer (bayrak, ceza). Her oyuncunun dünyası kendine aittir. |
| Modlar | **Yarış** ve **birlikte** ilk sürümden itibaren var, lobide seçilir. Altyapı aynı; birlikte modunda bayrak ve ceza kapalı. |
| Yarışın uzunluğu | Oyunun tamamı (60 oda). |
| Bayrak | Her odanın sonunda bir bayrak. Her oyuncu her odanın bayrağını **bir kez** alabilir (yoksa geri gönderilen oyuncu aynı bayrağı tekrar toplar). |
| Ceza | **Otomatik.** 3. bayrakta rakip bir önceki odanın (`RoomId - 1`) başlangıç checkpoint'ine gider, sayaç 0'a döner. Rakip oda 1'deyse ceza boşa gider. |
| Bağlantı koparsa | Yarış kaydedilir, aynı iki kişi sonra kaldıkları yerden devam eder. |
| Ceza hedefi | **Alıcı belirler.** Gönderen 3. bayrakta cezayı her zaman yollar (bildiği oda eski olabilir); `PenaltyMessage.TargetRoomId` yalnız bilgi. Alıcı cezayı **uyguladığı an** kendi odasının bir öncesine gider (`RaceSession.TryGetPenaltyTarget`); ceza oda geçişi ya da respawn sırasında bekletilirse hedef o sırada değişmiş olabilir. İlk odadaki, birlikte modundaki ya da bitirmiş oyuncuya ceza işlemez. Rakibin bayrak sayacı da 3'te alıcıda 0'a döner. |
| Rakibin odası | Yalnız güvenilir mesajlardan (`RoomEntered`, `ProgressSync`); `Snapshot.RoomId` kullanılmaz, çünkü iki kanal birbirine göre sıralı değil. |
| Platform | Steam. Geliştirme ve testte Steam'in ücretsiz test uygulaması (Spacewar, AppID 480); kendi AppID'si (100 $) mağaza sayfası / dış test zamanı alınır. |
| Ağ çözümü | Kendi ince arayüzümüz (`IRaceTransport`) + değiştirilebilir taşıyıcılar. Netcode for GameObjects ve Unity Relay/Lobby kullanılmaz. |

### Neden Netcode for GameObjects değil

6 Kasım 2026'da iki savunucu ve bir hakem agent'la karşılaştırıldı (kaynak kodu ve repo geçmişi kontrol edilerek). Karar: kendi ince katmanımız.

- **Hız:** NGO yerel ağda oynanabilir sürümü 1–2 akşam erken getirir, ama Steam aşamasında bu kazanç geri gider. NGO'nun Steam taşıyıcısı (community-contributions, Facepunch) RTT'yi sabit `0` döndürüyor (issue #247, 2024'ten beri açık; NGO gecikme telafisi yapamıyor), `Shutdown()` içinde `SteamClient.Shutdown()` çağırıyor (#267; yeniden bağlanma ve davetle çakışır), `NoNagle` kullanmıyor. Paket hâlâ 2.0.0, Windows DLL'i Ekim 2021'den. Net fark 0–1 akşam, tahmin hatası içinde.
- **Bakım:** Unity 6000.3 yalnızca NGO 2.x'i destekliyor. 6000.7'ye geçişte NGO 3.x, Netcode for Entities + Entities + Burst'ü zorunlu getiriyor.
- **Gecikme:** İki yol da ayarla tek yön + ~70–100 ms'ye iner (ölçülmedi); bizde tampon, Nagle ve saat tamamen kontrolümüzde.
- **Tek kişilik koruma:** NGO paketi, ILPP, `NetworkManager` ve prefab listesi yok; uçtan uca testler EditMode'da kalır (Stop hook yalnız EditMode koşuyor).

**Kararı değiştirecek koşullar:** tasarım paylaşılan duruma kayarsa (ortak platform/nesne, host otoritesi, 2'den fazla oyuncu, ~10'dan fazla mesaj tipi) ya da MP 1–MP 3 dört akşamı aşarsa NGO'ya geçilir. Facepunch.Steamworks 6000.3'te çalışmazsa karar değişmez, Steamworks.NET'e geçilir.

## Tek kişilik oyunun korunması

1. **Ayrı assembly.** Kod `RockTheGame.Multiplayer` assembly'sinde. Bu assembly `RockTheGame.Runtime`'ı görür, Runtime onu **göremez**. Tek kişilik kodun multiplayer'a bağlanması derleme hatası verir.
2. **Ayrı giriş.** Tek kişilik oyun `NewScene`'i bugünkü gibi açar. Multiplayer `MultiplayerLobby.unity`'den girer, bağlantı kurulunca `NewScene`'i yükler ve kendi nesnelerini çalışırken ekler.
3. **Runtime'a dokunuş yalnızca ekleme.** Gerekirse yeni bir olay ya da okuma özelliği eklenir; mevcut davranış değişmez.
4. **Oda prefab'larında yalnız işaret noktası.** `RaceFlagPoint`'in görüntüsü, collider'ı, `Update`'i yok; tek kişilik oyunda hiçbir şey yapmaz.
5. **Ayrı kayıt dosyası.** Yarış kaydı tek kişilik kayıttan bağımsız.
6. **Her aşamada testler.** 28 EditMode ve 36 PlayMode testi (bkz. [`../bozulmamali-davranislar.md`](../bozulmamali-davranislar.md)) her multiplayer aşamasından sonra yeşil olmalı.

## Bileşenler

Kod `Assets/Scripts/New-Scripts/Multiplayer/` altında. Taşıyıcılar ayrı asmdef'lerde, böylece Steam kütüphanesi ana koda girmez.

| Parça | Görevi | Bağımlılığı |
|---|---|---|
| `IRaceTransport` | Host ol / katıl / ayrıl, `Send(mesaj, güvenilir)`, bağlandı / koptu / mesaj geldi olayları | Yok |
| `InMemoryTransport` | İki ucu aynı süreçte bağlar | EditMode testleri |
| `UnityTransportAdapter` | Localhost, yerel ağ, doğrudan IP; `ReliableSequenced` ve `UnreliableSequenced` pipeline; `disconnectTimeoutMS: 5000`, `heartbeatTimeoutMS: 500`; gönderimden sonra `ScheduleFlushSend`; `NetworkSendQueueFull` gelirse kuyruk | `com.unity.transport` 2.6 (Relay yok) |
| `SteamTransport` | Arkadaş daveti, Steam P2P (SteamNetworkingSockets); gönderimler `Reliable\|NoNagle` / `Unreliable\|NoNagle`; `SteamClient.Shutdown` yalnız uygulama kapanırken | Facepunch.Steamworks 2.5.2 (ayrı asmdef, Win64) |
| `RaceMessages` | Mesajları byte'a yazar / okur | Yok |
| `RaceSession` | Kurallar: mod, bayrak kaydı, 3'te ceza, rakibin odası, bitiş | Saf C# |
| `MultiplayerSession` | Kök nesne, `DontDestroyOnLoad`; taşıyıcıyı ve `RaceSession`'ı tutar, sahne yüklenince aşağıdakileri ekler | — |
| `LocalPlayerBroadcaster` | Saniyede 20 kez `Snapshot`; oda girişinde `RoomEntered` | `PlayerController`, `Room` (yalnız okur) |
| `GhostPlayer` (prefab) | Gelen konumlar arasında yumuşak geçiş, animator'ı sürer, odaya göre görünür/gizli. **Sahne kökünde durur, hiçbir `Room`'un altında değil** (oda uyutulunca kapanmasın) | Animator |
| `RaceFlagPoint` | Odadaki bayrak işaret noktası (editörde bayrak gizmo'su) | Yok |
| `RaceFlag` | Multiplayer'da işaret noktasına konan bayrak; dokununca `RaceSession`'a bildirir | `RaceSession` |
| `PenaltyHandler` | Gelen cezayı uygular | Mevcut oda, kamera, geçiş parçaları |
| Multiplayer HUD | Bayrak sayacı, "Rakip: Oda 7", bağlantı uyarıları; kendi canvas'ı, `UI` katmanı | — |

### Kod (yazılanlar)

- `Assets/Scripts/New-Scripts/Multiplayer/RockTheGame.Multiplayer.asmdef` — `references: []`, `autoReferenced: false`
- `Multiplayer/RaceProtocol.cs` — `Version = 1`, `MaxPlayerNameLength = 32`
- `Multiplayer/Messages/RaceMessages.cs`, `RaceMessageCodec.cs` — 7 mesaj; `Snapshot` 44 bayt (iki kanca noktası doluyken)
- `Multiplayer/Transport/IRaceTransport.cs`, `InMemoryTransport.cs`
- `Multiplayer/Transport/UnityTransportAdapter.cs` — `com.unity.transport` 2.7.4; `UnityTransportSettings.Default`: port 7777, kopma 5000 ms, heartbeat 500 ms. Her pakete 1 baytlık tür öneki ekler (veri / "kabul edildin"); istemci host'un kabulünü görene kadar `Connecting` kalır, bu sırada kapanırsa `Rejected` olur (host dolu).
- `Multiplayer/Dev/MultiplayerDevSettings.cs`, `MultiplayerDevLauncher.cs` — yalnız editör (`#if UNITY_EDITOR`), MP 7'deki lobiye kadar geçici
- `Multiplayer/Ghost/` — `GhostTiming` (20 Hz, 0.1 sn gecikme; ışınlanma eşiği hızla ölçekli: 100 b/sn × geçen süre + 1, en hızlı hareket sapan 65 b/sn; sıra numarası gönderenin gerçek saatinden), `SnapshotBuilder`, `GhostInterpolator`, `GhostVisibility` (saf C#); `GhostView` (görseli çalışırken oyuncunun `Animation` sprite/animator'ından ve ipinden kurar, prefab yok, sahne kökünde `[MP Ghost]`), `LocalPlayerBroadcaster` (`[MP Broadcaster]`, oda girişlerini `Room.OnRoomEntered`'dan alır)
- `Multiplayer/Session/RaceRules.cs` (`FlagsPerPenalty = 3`, `FirstRoomId = 1`), `RaceSession.cs` — ağ bilmez; yerel olaylar gidecek mesajları döndürür, `Receive` gelenleri işler
- Testler: `Assets/Tests/EditMode/Multiplayer/` (`AssemblySeparationTests`, `RaceMessageCodecTests`, `InMemoryTransportTests`, `RaceSessionLocalTests`, `RaceSessionRemoteTests`, `UnityTransportAdapterTests` (gerçek localhost bağlantısı), `MultiplayerDevLauncherTests`, `GhostLogicTests`, `GhostViewTests`)

### Editörde nasıl test edilir

1. Menü **Rock The Game → Multiplayer → Geliştirici başlatıcısı**'nı aç (işaretli olur; Play Mode dışında değiştirilebilir). Kapalıyken tek kişilik oyun aynen çalışır, sahnede ağ nesnesi oluşmaz.
2. **Window → Multiplayer → Multiplayer Play Mode**'da bir sanal oyuncu (Player 2) etkinleştir. **İlk açılışta Player 2 penceresinin sahneyi yüklemesini bekle** (Game görünümünde oda görünene kadar); erken Play'e basılırsa Player 2 boş bir sahneyle başlar (6 Kasım'da görüldü, Multiplayer Play Mode'un ilk açılış zamanlaması).
3. Play'e bas. Başlatıcı Play süresince `Application.runInBackground`'u açar; yoksa odakta olmayan pencere durur ve 5 sn'de bağlantı kopar (PlayerSettings değişmez). Ana editör host olur (`[MP] Host dinliyor :7777`), sanal oyuncu `127.0.0.1`'e bağlanır; istemci host'tan önce açılırsa saniyede bir yeniden dener.
4. Console'da `[MP] Bağlandı`, `→ Hello`, `← ProgressSync` satırlarını gör.
5. Her pencerede rakibin karakteri açık mavi, yarı saydam bir hayalet olarak, kendi karakterinin arkasında görünür; ~0.1 sn geriden takip eder, animasyonlarını ve kanca iplerini oynatır. Rakip senin odanda ya da komşu odada değilse gizlenir. Rakip ölünce hayaleti ölüm noktasında kaybolur (snapshot'ın `IsHidden` biti, oyuncu nesnesi kapalıyken gider) ve respawn noktasında kaydırmadan yeniden belirir; `RespawnManager` oyuncuyu iris kapanınca açtığı için hayalet, ölen oyuncunun kendi ekranındaki iris açılmadan biraz önce görünür.

Ajan da iki pencereli akışı CLI ile sürebiliyor: Player 2 açıkken `unity` CLI varsayılan olarak kopyaya bağlanır; ana editör için `UNITY_PROJECT_PATH` proje köküne, kopya için `Library/VP/<id>` yoluna ayarlanır.

Rol elle seçilmez: Multiplayer Play Mode'un `CurrentPlayer.IsMainEditor` değeri belirler. Başlatıcı `NewScene`'e konmaz, `RuntimeInitializeOnLoadMethod` ile kökte `[MP Dev Launcher]` nesnesini oluşturur.

**Gamepad:** iki pencerenin aynı gamepad'i okuyup okumadığı henüz denenmedi (sanal oyuncu penceresini ajan açamıyor). İlk elle denemede bakılacak; gerekirse MP 4'te ele alınır.

## Mesajlar

İki oyuncu eşittir. Host yalnızca bağlantıyı açan taraftır, oyun hakemi yoktur. Herkes kendi oyuncusunun tek yetkilisidir. Arkadaşlar arası oynandığı için hile koruması kapsam dışı.

| Mesaj | Ne zaman | Kanal | İçerik |
|---|---|---|---|
| `Hello` | Bağlanınca | Güvenilir | Protokol sürümü, mod, yarış kimliği, oyuncu adı |
| `Snapshot` | Saniyede 20 kez | Güvenilmez | Sıra no, oda, konum, yön, 7 animasyon değeri + dash sayacı, iki kanca noktası, "ışınlandı" biti (~50 byte) |
| `RoomEntered` | Oda değişince | Güvenilir | Oda ID |
| `FlagCaptured` | Bayrak alınınca | Güvenilir | Oda ID, yeni sayaç |
| `Penalty` | 3. bayrakta | Güvenilir | Rakibin gideceği oda |
| `ProgressSync` | Bağlanınca / yeniden bağlanınca | Güvenilir | Oda, sayaç, alınmış bayraklar, süre |
| `Finished` | Son oda bitince | Güvenilir | Süre |

Animasyon için `PlayerAnimationController`'ın animator'a yolladığı 8 değer yeterli: `Speed`, `IsGrounded`, `VerticalVelocity`, `IsSwinging`, `IsDashing`, `IsWallClimbing`, `IsWallSliding`, `DashTrigger`. **`DashTrigger` bir bool olarak değil, her dash'te bir artan `DashCount` sayacı olarak gider:** güvenilmez kanalda tek bir paket kaybolsa da alıcı sayacın değiştiğini görüp dash animasyonunu oynatır.

Hayalet gelen konumları göndericinin sıra numarasıyla sıralar ve ~100 ms gerisinden çizer (20 Hz'de bir paket kaybını tolere eder; gerekirse 30 Hz + 67 ms). "Işınlandı" biti gelince tampon temizlenir ve geçiş yapmadan yer değiştirir (ölüm ve ceza sonrası kaymasın).

## Oyuna bağlanma

**Lobi.** Gamepad ile gezilen ekran: mod (yarış / birlikte), Host / Katıl (Unity Transport'ta IP, Steam'de davet), kayıtlı yarış varsa "Devam et".

**Oda takibi.** `Room.OnRoomEntered` olayı dinlenir. Bu olayın her oda girişinde çağrıldığı uygulamada doğrulanmalı; çağrılmıyorsa `RoomManager`'a ek bir olay konur.

**Oda sırası.** `Room.RoomId` sıra numarası olarak kullanılır; yeni alan yok. 6 Kasım'da 1–21 doğru, `RoomNew-22`'nin ID'si 0 (22 yapılacak). Eski `RoomNew-MayIncluded` sahneden silindi.

**Bayrak yerleri.** Bir editör menü komutu, `RaceFlagPoint`'i olmayan her odaya onu varsayılan yere koyar: bir sonraki odaya (`RoomId + 1`) giden `RoomTransitionTrigger`'ın hemen önü. Geliştirici sonra sahnede sürükleyip istediği yere taşır. Komut tekrar çalıştırıldığında yalnızca eksik odaları doldurur, taşınmış işaretlere dokunmaz.

**Hayalet görünürlüğü.** Rakibin odası senin odan ya da komşu odalarından biriyse görünür, değilse gizlenir. Yarı saydam ve farklı renk; `Player` katmanında, gerçek oyuncunun arkasında.

**Ceza akışı.** `RespawnManager` hep aynı odada doğurur, ceza oda değiştirir; bu yüzden multiplayer assembly'de ayrı bir akış olur ve mevcut parçaları sırayla kullanır:

1. İris kapanır.
2. `RoomManager.TransitionToRoom(önceki oda)`.
3. Oyuncu o odanın `InitialCheckpoint`'ine taşınır.
4. `CameraController.SnapToRoomBounds(...)`.
5. Checkpoint kanalına yeni nokta yollanır, böylece sonraki ölüm doğru odada olur.
6. İris açılır.

Ceza, oyuncu oda geçişinin ya da respawn'ın ortasındaysa o bitene kadar bekletilir.

## Kayıt ve bağlantı kopması

**Kayıt.** Her oyuncu kendi ilerlemesini ayrı bir dosyaya yazar (`Application.persistentDataPath`): yarış kimliği, mod, rakip, oda, bayrak sayacı, alınmış bayraklar, süre. Oda girişinde, bayrakta ve cezada yazılır.

**Devam etme.** İki taraf "Devam et" seçer; `Hello`'daki yarış kimlikleri karşılaştırılır. Uyuşmazsa bağlantı açık bir mesajla reddedilir, uyuşursa `ProgressSync` ile durumlar paylaşılır.

**Kopma.** Oturum katmanı 3 saniye rakipten paket gelmezse rakibi "koptu" sayar (taşıyıcıdan bağımsız). Hayalet gizlenir, "Rakip bağlantısı koptu, bekleniyor…" yazısı çıkar. Adil kalsın diye oyuncu `FreezeForTransition` ile dondurulur. 30 saniye içinde rakip dönerse sürer; dönmezse kayıt alınır ve lobiye dönülür.

**Sürüm uyuşmazlığı.** Protokol sürümleri farklıysa bağlantı baştan reddedilir.

## Testler

- **EditMode:**
  - mesajların yazılıp okunması;
  - `RaceSession` kuralları (bayrak bir kez, 3'te ceza ve sayaç sıfırlanması, oda 1'de boşa giden ceza, birlikte modunda ceza yok);
  - `InMemoryTransport` ile iki oturum arasında uçtan uca akış;
  - kayıt dosyasının yazılıp okunması.
- **PlayMode:**
  - ceza sonrası oyuncunun ve kameranın önceki odada olması;
  - hayaletin oda görünürlüğü;
  - mevcut 36 kuralın hâlâ yeşil olması.
- **Elle:**
  - Aynı bilgisayarda iki kopya: Multiplayer Play Mode paketi (yalnız editör, oyuna girmez) ya da bir build + Editor.
  - En sonda iki bilgisayar ve iki Steam hesabı. İkinci hesap ücretsiz açılabilir; hiç harcama yapılmamış "sınırlı" hesap arkadaşlık isteği gönderemez ama kabul edebilir.

## Aşamalar

Her biri tek akşamlık (Cuma, `#code`), sırayla:

0. **Steam duman testi** (yarım akşam, en riskli parça önce): Facepunch.Steamworks 2.5.2 ayrı asmdef'te, `SteamClient.Init(480)`, iki bilgisayar arasında relay bağlantısı. İki bilgisayar ve iki Steam hesabı gerekir.
1. Multiplayer assembly'leri, `IRaceTransport`, `InMemoryTransport`, `RaceMessages` ve testleri
2. `RaceSession` kuralları ve testleri
3. `UnityTransportAdapter` ve iki kopyanın bağlanması
4. `LocalPlayerBroadcaster` ve `GhostPlayer` prefab'ı
5. `RaceFlagPoint`, yerleştirme komutu, `RaceFlag`; önce `RoomNew-22`'nin ID'si 22 yapılır (bayrak yeri `RoomId + 1` ile bulunur, ve ID 0 "bilinmiyor" ile karışıyor); ardından geliştiricinin `#level` akşamı: bayrakları yerleştirmek
6. Ceza akışı ve PlayMode testi
7. Lobi sahnesi ve multiplayer HUD
8. Yarış kaydı, devam etme, kopma yönetimi (`RaceSession`'a kayıttan yerel durumu geri yükleme eklenir; ilk bağlantı dahil her bağlantıda `ProgressSync` yollanır, yoksa rakip ilk odada "bilinmiyor" görünür)
9. `SteamTransport` (AppID 480) ve iki bilgisayarlı test

## Kapsam dışı

- Rastgele eşleşme (yalnızca arkadaşla oynanır).
- Hile koruması.
- İkiden fazla oyuncu.
- Fiziksel etkileşim (itme, birbirine tutunma, paylaşılan platformlar).
- Tek kişilik kayıt sistemi (ayrı bir iş; yarış kaydı ondan bağımsız tasarlandı).
