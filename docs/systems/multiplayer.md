# Çok oyunculu

*GDD §9. Durum: **tasarım onaylandı; MP 1–9 kodda** (mesajlar, `RaceSession` kuralları, Unity Transport ve Steam taşıyıcıları, lobi ve Steam daveti, rakip hayaleti, bayraklar, ceza, geri sayım, HUD, kayıt, devam etme, kopma, bitiş ve sonuç). **Steam bağlantısı iki bilgisayarda henüz denenmedi** (MP 0). Son kod doğrulaması: 2026-11-14 (Steam ilk deneme: kayıt ayrımı, linger).*

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
2. **Ayrı giriş.** Oyun `MainMenu.unity` ile açılır (7 Kasım): Tek kişilik `NewScene`'i bugünkü gibi açar. Çok oyunculu `MultiplayerLobby.unity`'yi açar; lobi bağlantı kurulunca `NewScene`'i yükler ve kendi nesnelerini çalışırken ekler.
3. **Runtime'a dokunuş yalnızca ekleme.** Gerekirse yeni bir olay ya da okuma özelliği eklenir; mevcut davranış değişmez. (6 Kasım: `PlayerEyesController`'a hayalet gözleri için yalnız okunur özellikler eklendi.)
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
| `SteamTransport` | Steam relay bağlantısı (SteamNetworkingSockets `CreateRelaySocket` / `ConnectRelay`, adres SteamID); gönderimler `Reliable\|NoNagle` / `Unreliable\|NoNagle`; bağlantı linger ile kapanır (el sıkışmayı reddeden tarafın Hello'su karşıya ulaşır, iki taraf da nedeni görür); `SteamClient.Shutdown` yalnız uygulama kapanırken. Davet: `SteamInviteService` (iki kişilik, yalnız arkadaşlara açık Steam lobisi + davet arayüzü; kabul edilince lobi sahibinin SteamID'sine bağlanılır, bağlanınca lobiden çıkılır; oyun kapalıyken davet `+connect_lobby` ile açar) | Facepunch.Steamworks 2.5.2 (`Assets/Plugins/Facepunch`, yalnız Win64, `isExplicitlyReferenced`; `RockTheGame.Multiplayer.Steam` asmdef) |
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
- `Multiplayer/MultiplayerSession.cs` — `[MP Session]`, `DontDestroyOnLoad`; taşıyıcı + `RaceLobbyFlow`. El sıkışma bitince `NewScene`'i `LoadSceneAsync` ile yükler (başlatıcıda sahne zaten yüklü), `sceneLoaded`'da (Start'tan önce) yayıncı, hayalet, ceza, bayraklar ve HUD'u kurar, oyuncuyu geri sayım boyunca `OnStartRespawn` ile dondurur, "Başla"da `OnEndRespawn`
- `Multiplayer/Save/` — `RaceSaveData` (JsonUtility: yarış kimliği, mod, rakip adı, oda, sayaç, alınmış bayraklar, süre, rakibin odası/sayacı, zaman), `RaceSaveStore` (build `persistentDataPath/multiplayer-race.json`; editör `multiplayer-race-editor.json`, Multiplayer Play Mode'un sanal oyuncusu `multiplayer-race-<id>.json` yazar. Editör ile build aynı klasörü paylaşır: 14 Kasım'daki ilk Steam denemesinde editörün geliştirici kaydı build'de "Devam et"i kendiliğinden açtı, kaydı olmayan arkadaşın oyunu el sıkışmayı reddetti, host yalnız "Rakip ayrıldı" gördü). Yerel durum değişince (oda, bayrak, ceza), kopunca ve lobiye dönerken yazılır
- `Multiplayer/Session/ConnectionWatch.cs` — 3 sn paket gelmezse sessiz; kopunca 30 sn pencere
- `Multiplayer/Penalty/RoomPlacement.cs` — oyuncuyu odanın başlangıcına koyar (ışınlama, checkpoint, kamera, oda); ceza ve devam etme ortak
- `Multiplayer/Session/RaceLobbyFlow.cs` — saf: `Hello` (sürüm uyuşmazsa `Failed`, modu host belirler, istemcinin `RaceSession`'ı host'un Hello'sundan) → `ProgressSync` + sahne yükle → `Ready` → iki taraf hazırsa 3 sn geri sayım → `Racing`. Devam (`resume`) ve yarış içinde yeniden bağlanma (`existingSession`): `Hello.Resume` iki tarafta aynı ve kimlikler eşit olmalı, yoksa iki taraf da açık mesajla `Failed`. Yeni yarışta istemci `Guid.Empty` yollar, host'un kimliğini alır
- `Multiplayer/Hud/` — `RaceHud` (`[MP HUD]`, overlay canvas, sortingOrder 100: sağ üstte "Bayrak N/3" ve "Rakip: Oda X · Bayrak N/3", üst ortada bağlantı uyarısı, ortada 3-2-1 / BAŞLA!), `RaceHudText` (saf), `MultiplayerUISettingsSO` (`Resources/Multiplayer/MultiplayerUISettings`, Tiny5 font)
- `Multiplayer/Hud/RaceResultScreen.cs`, `RaceResultMath.cs` — **sonuç ekranı** (12 Kasım; mockup `uiResult`, `ui-research/mashup-ui.html`; kayıt: `ui-research/mp-result-play.png`). Yerel oyuncu bitirince `RaceHud` ekranı açar (`[MP Result]`, overlay sorting 120, `UiPixelScale`, CRT dışı). **1. oyuncu = host = mavi silindir, 2. oyuncu = istemci = kırmızı küp**; yerelin hangisi olduğunu sütun üstündeki plaka söyler (yerel sarı "SEN", bitirmiş rakip siyah "RAKİP", bitirmemiş rakip kâğıt "RAKİP …"). Sütun yüksekliği sıralamayı gösterir: kazanan (ya da berabere / birlikte) tam boy, kaybeden `kazanan süre / kaybeden süre` oranında, en az %45 (`RaceResultMath`, EditMode testli). Başlık levhada: Bitirdin! (rakip bekleniyor) / Kazandın! / Kaybettin. / Berabere! / Birlikte bitirdiniz!. Rakip bitirmeden yalnız yerel sütun yükselir, rakip bitirince onun sütunu da yükselir (yerel kaybedense yerelin sütunu kısalıp yeniden yükselir). Ağdan yalnız süre geldiği için ekranda ölüm/bayrak/ananas yok. "A: Lobiye dön" `CanReturnToLobby` olunca belirir. Eski düz metin (`RaceHud.ResultText`, `RaceHudText.Result`) veri olarak kaldı: `LastExitReason` ve testler onu kullanır, ekranda gösterilmez; sonuç ekranı açıkken sağ üstteki süre/bayrak satırları gizlenir. Hareket: `ui-motion.md` tablosu. Değerler (`riseDelay` 0,35 sn, `winnerExtraDelay` 0,18, `slamScale` 2,4, `bobAmount` 8) başlangıç önerisi, gamepad ile ayarlanır
- `Multiplayer/Lobby/` — `MultiplayerLobbyUI` (Mod Yarış↔Birlikte, Host ol, adres `127.0.0.1`, Katıl, Vazgeç, durum satırı; gamepad seçimi kapanan düğmeden kurtarılır), `MultiplayerLobbyBuilder` (menü **Rock The Game → Multiplayer → Lobi sahnesini kur**; `Assets/Scenes/MultiplayerLobby.unity`, build listesinde NewScene'den sonra)
- `Multiplayer/Steam/` — `SteamTransport`, `SteamInviteService`, `SteamBootstrap` (açılışta `SteamClient.Init(480)`; başarısızsa IP'ye düşer; editörde menü **Rock The Game → Multiplayer → Steam (editör)** ile açılır, varsayılan kapalı). `Multiplayer/Transport/IInviteService.cs` + `InviteServices`: Multiplayer çekirdeği Steam'i bilmez, Steam assembly'si kendini kaydeder. Lobide Steam açıksa "Bağlantı: Steam / IP" düğmesi
- `Multiplayer/Flags/RaceFinishPoint.cs`, `RaceFinishLine.cs` — bitiş: menü **Bitiş çizgisini yerleştir** ana yolun son odasına (bir öncekinden girilen en büyük kimlik; 21) işaret koyar. Dokunan bitirir (iki modda): süre durur, oyuncu donar, kayıt silinir, `Finished` gider. İki taraf da bitirince sonuç ekranı çıkar (aşağıda); A ya da Enter ile lobiye dönülür. Biri bitirdikten sonra kopma beklenmez
- `Multiplayer/Lobby/MainMenuUI.cs`, `MainMenuBuilder.cs` — ana menü (build index 0): Tek kişilik, Çok oyunculu, Çıkış; gamepad ile gezilir. Menüdeyken Steam daveti kabul edilirse (`IInviteService.HasPendingJoin`) lobiye geçilir, lobi bekleyen daveti alıp katılır. Lobide **Ana menü** düğmesi oturumu kapatıp menüye döner. Menü **Rock The Game → Ana menü sahnesini kur** (sahneyi bir kez kurar, build listesinin başına koyar) ve **Rock The Game → Multiplayer → Lobiye Ana menü düğmesini ekle**
- `Multiplayer/Lobby/MultiplayerEntry.cs`, `MultiplayerBuild.cs` — build'de `-multiplayer` (ya da `-mp-host`, `-mp-join <ip>`, Steam'in `+connect_lobby`'si) ile ana menü atlanıp lobi açılır, lobi `-mp-host`/`-mp-join`'de kendiliğinden başlar. Menü **Rock The Game → Build → Windows (multiplayer testi)** `Builds/Windows/`'a exe + `steam_appid.txt` + `OKU-BENI.txt` yazar; `.bat` dosyaları 7 Kasım'da kalktı (eskileri silinir). Arkadaşla deneme: [`../multiplayer-arkadasla-test.md`](../multiplayer-arkadasla-test.md). 6 Kasım: build ilk kez alındı (0 hata, 114 MB); build host, editör istemci olarak yarış başladı
- `Multiplayer/Dev/MultiplayerDevSettings.cs`, `MultiplayerDevLauncher.cs` — yalnız editör (`#if UNITY_EDITOR`); `NewScene`'de doğrudan Play'e basınca `MultiplayerSession`'ı "sahne yüklü" kipinde kurar. Lobiden girildiyse devreye girmez
- `Multiplayer/Ghost/` — `GhostTiming` (20 Hz, 0.1 sn gecikme; ışınlanma eşiği hızla ölçekli: 100 b/sn × geçen süre + 1, en hızlı hareket sapan 65 b/sn; sıra numarası gönderenin gerçek saatinden), `SnapshotBuilder`, `GhostInterpolator`, `GhostVisibility` (saf C#); `GhostView` (görseli çalışırken oyuncunun `Animation` sprite/animator'ından, ipinden ve gözlerinden kurar; prefab yok, sahne kökünde `[MP Ghost]`; bütün sprite'lar 10 sıra geride, kendi içindeki sıra korunur), `GhostEyes` (yön çevirme, `EyePosition`'a göre dikey kayma, hıza göre göz bebeği; göz kırpma yok), `GhostArms` (kol yalnız o taraf tutunmuşken görünür ve kancaya bakar, ip elin ucundan çıkar; oyuncunun kolu girdi yönüne bakar, rakibin girdisi ağdan gelmez), `GhostDashFx` (dash'te `PlayerVFXManager` ayarlarıyla solan kopyalar, salınırken ve dash'te iz; ışınlanma ve yeniden belirmede iz temizlenir). Runtime'a ekleme: `PlayerVFXManager`'a dört okuma özelliği, `LocalPlayerBroadcaster` (`[MP Broadcaster]`, oda girişlerini `Room.OnRoomEntered`'dan alır)
- `Multiplayer/Flags/` — `RaceFlagPoint` (işaret; Room'un doğrudan çocuğu, editörde sarı bayrak gizmo'su ve "Bayrak N" etiketi), `RaceFlagPlacement` (varsayılan yer: trigger'ın giriş tarafında 1.5 birim geri; yatay çıkışta ve merkezde aşağıdaki 6 birim içinde Ground/OneWay zemine iner), `RaceFlagPlacer` (menü **Rock The Game → Multiplayer → Bayrak işaretlerini yerleştir**, yalnız eksik odalar, Undo'lu), `RaceFlag` (`[MP Flag]`, Pixel Adventure `Flag.controller`: direk → dokununca bayrak kalkar; trigger 1.5×3 birim, tabanı işarette), `RaceFlagSettingsSO` (`Assets/Resources/Multiplayer/RaceFlagSettings.asset`). Runtime'a ekleme: `RoomTransitionTrigger.TargetRoom` / `.Direction` okuma özellikleri
- `Multiplayer/Penalty/PenaltyHandler.cs` — `[MP Penalty]`; `RespawnManager` dizisinin aynısı (iris kapanır → `LevelResetManager.ResetAll` → hedef odanın `InitialCheckpoint`'i → checkpoint etkinleşir → kamera `SnapToRoomBounds` → `RoomManager.TransitionToRoom` → 0.5 sn → iris açılır). Oyuncu kapalı, `IsPaused` ya da `PlayerTransitionState`'teyken bekler; cezalar sırayla. Runtime'a ekleme: `PlayerController.IsPaused`. Multiplayer asmdef'e `UniTask` referansı eklendi
- `Multiplayer/Session/RaceRules.cs` (`FlagsPerPenalty = 3`, `FirstRoomId = 1`, `PreviousRoomId`: `RoomId − 1` komşuysa o, değilse komşulardan küçük en büyük; yan oda 22 → 11), `RaceSession.cs` — ağ bilmez; yerel olaylar gidecek mesajları döndürür, `Receive` gelenleri işler
- Testler: `Assets/Tests/EditMode/Multiplayer/` (`AssemblySeparationTests`, `RaceMessageCodecTests`, `InMemoryTransportTests`, `RaceSessionLocalTests`, `RaceSessionRemoteTests`, `UnityTransportAdapterTests` (gerçek localhost bağlantısı), `MultiplayerDevLauncherTests`, `GhostLogicTests`, `GhostViewTests`, `RaceFlagPlacementTests`, `RaceFlagTests`, `PenaltyRulesTests`); PlayMode `PenaltyFlowTests` (gerçek `NewScene`: 3 → 2, 22 → 11, oda 1'de işlemez, ölüyken bekler, ceza sonrası ölüm hedef checkpoint'te doğar)

### Editörde nasıl test edilir

**Kopma ve devam (6 Kasım'da iki pencerede doğrulandı):** yarış sürerken Player 2 Play'den çıkınca host donar, HUD'da "Rakip bağlantısı koptu, bekleniyor… (N)" görünür, host yeniden dinler. 30 sn içinde dönülmezse kayıt yazılır ve lobiye dönülür; lobi nedeni gösterir, "Devam et: Açık (Oda N, Bayrak K/3)" kendiliğinden açıktır. İki taraf Devam et ile Host / Katıl deyince herkes kendi kayıtlı odasının başlangıcında 3-2-1 ile sürdürür; süre kayıttan devam eder. Bu denemede ana editörün Console'undaki **Error Pause** CLI zaman aşımı hatalarıyla Play'i duraklattı; duraklayan host eski bağlantıyı kabul edip yenisini reddetti. Elle denerken Error Pause'u kapalı tut.

**Lobiden (asıl yol, 6 Kasım'da iki pencerede doğrulandı):** `MultiplayerLobby.unity`'yi aç, Player 2'yi etkinleştir, Play. Ana pencerede **Host ol**, Player 2'de **Katıl**. İki taraf `NewScene`'i yükler, HUD'da 3-2-1 görünür, oyuncular donuktur; **BAŞLA!**'da serbest kalırlar. Player 2 kendi son açtığı sahneyle başlayabilir (6 Kasım'da `NewScene` açtı); o zaman Player 2'de de lobiyi aç ya da geliştirici başlatıcısını kapat. İki tarafın başlama anı editörde ~0.4 sn farklı ölçüldü: arka plandaki ana editör `Ready`'yi geç işliyor. Gerçek ağda fark tek yönlü gecikme kadar olur.

**Doğrudan `NewScene`'den (geliştirici başlatıcısı):**

1. Menü **Rock The Game → Multiplayer → Geliştirici başlatıcısı**'nı aç (işaretli olur; Play Mode dışında değiştirilebilir). Kapalıyken tek kişilik oyun aynen çalışır, sahnede ağ nesnesi oluşmaz.
2. **Window → Multiplayer → Multiplayer Play Mode**'da bir sanal oyuncu (Player 2) etkinleştir. **İlk açılışta Player 2 penceresinin sahneyi yüklemesini bekle** (Game görünümünde oda görünene kadar); erken Play'e basılırsa Player 2 boş bir sahneyle başlar (6 Kasım'da görüldü, Multiplayer Play Mode'un ilk açılış zamanlaması).
3. Play'e bas. Başlatıcı Play süresince `Application.runInBackground`'u açar; yoksa odakta olmayan pencere durur ve 5 sn'de bağlantı kopar (PlayerSettings değişmez). Ana editör host olur (`[MP] Host dinliyor :7777`), sanal oyuncu `127.0.0.1`'e bağlanır; istemci host'tan önce açılırsa saniyede bir yeniden dener.
4. Console'da `[MP] Bağlandı`, `→ Hello`, `← ProgressSync` satırlarını gör. Yarış modunda `[MP] 22 bayrak kuruldu`.
5. Her pencerede rakibin karakteri açık mavi, yarı saydam bir hayalet olarak, kendi karakterinin arkasında görünür; ~0.1 sn geriden takip eder, animasyonlarını ve kanca iplerini oynatır. Rakip senin odanda ya da komşu odada değilse gizlenir. Rakip ölünce hayaleti ölüm noktasında kaybolur (snapshot'ın `IsHidden` biti, oyuncu nesnesi kapalıyken gider) ve respawn noktasında kaydırmadan yeniden belirir; `RespawnManager` oyuncuyu iris kapanınca açtığı için hayalet, ölen oyuncunun kendi ekranındaki iris açılmadan biraz önce görünür.
6. Yarış modunda her odada bir direk durur; dokununca bayrak kalkar, Console'da `[MP] Bayrak: oda N, sayaç K` ve `→ FlagCaptured` görünür, karşı pencerede `← FlagCaptured`. 3. bayrakta sayaç 0'a döner ve `→ Penalty` gider; karşı pencerede iris kapanır, rakip bir önceki odanın başlangıcında açılır (`[MP] Ceza: oda X → Y`). Birlikte modunda bayrak yok.

Ajan da iki pencereli akışı CLI ile sürebiliyor: Player 2 açıkken `unity` CLI varsayılan olarak kopyaya bağlanır; ana editör için `UNITY_PROJECT_PATH` proje köküne, kopya için `Library/VP/<id>` yoluna ayarlanır.

Rol elle seçilmez: Multiplayer Play Mode'un `CurrentPlayer.IsMainEditor` değeri belirler. Başlatıcı `NewScene`'e konmaz, `RuntimeInitializeOnLoadMethod` ile kökte `[MP Dev Launcher]` nesnesini oluşturur.

**Gamepad:** iki pencerenin aynı gamepad'i okuyup okumadığı henüz denenmedi. Klavyeyle (6 Kasım) odaktaki pencere kontrol ediliyor.

## Mesajlar

İki oyuncu eşittir. Host yalnızca bağlantıyı açan taraftır, oyun hakemi yoktur. Herkes kendi oyuncusunun tek yetkilisidir. Arkadaşlar arası oynandığı için hile koruması kapsam dışı.

| Mesaj | Ne zaman | Kanal | İçerik |
|---|---|---|---|
| `Hello` | Bağlanınca | Güvenilir | Protokol sürümü, mod, yarış kimliği, oyuncu adı, devam biti (v3) |
| `Snapshot` | Saniyede 20 kez | Güvenilmez | Sıra no, oda, konum, yön, 7 animasyon değeri + dash sayacı, iki kanca noktası, "ışınlandı" biti (~50 byte) |
| `RoomEntered` | Oda değişince | Güvenilir | Oda ID |
| `FlagCaptured` | Bayrak alınınca | Güvenilir | Oda ID, yeni sayaç |
| `Penalty` | 3. bayrakta | Güvenilir | Rakibin gideceği oda |
| `ProgressSync` | Bağlanınca / yeniden bağlanınca | Güvenilir | Oda, sayaç, alınmış bayraklar, süre |
| `Finished` | Son oda bitince | Güvenilir | Süre |
| `Ready` | Sahne yüklenince | Güvenilir | Yok (protokol v2, MP 7); iki taraf görünce 3 sn geri sayım |

Animasyon için `PlayerAnimationController`'ın animator'a yolladığı 8 değer yeterli: `Speed`, `IsGrounded`, `VerticalVelocity`, `IsSwinging`, `IsDashing`, `IsWallClimbing`, `IsWallSliding`, `DashTrigger`. **`DashTrigger` bir bool olarak değil, her dash'te bir artan `DashCount` sayacı olarak gider:** güvenilmez kanalda tek bir paket kaybolsa da alıcı sayacın değiştiğini görüp dash animasyonunu oynatır.

Hayalet gelen konumları göndericinin sıra numarasıyla sıralar ve ~100 ms gerisinden çizer (20 Hz'de bir paket kaybını tolere eder; gerekirse 30 Hz + 67 ms). "Işınlandı" biti gelince tampon temizlenir ve geçiş yapmadan yer değiştirir (ölüm ve ceza sonrası kaymasın).

## Oyuna bağlanma

**Lobi.** Gamepad ile gezilen ekran: mod (yarış / birlikte), Host / Katıl (Unity Transport'ta IP, Steam'de davet), kayıtlı yarış varsa "Devam et".

**Oda takibi.** `Room.OnRoomEntered` olayı dinlenir. Bu olayın her oda girişinde çağrıldığı uygulamada doğrulanmalı; çağrılmıyorsa `RoomManager`'a ek bir olay konur.

**Oda sırası.** `Room.RoomId` sıra numarası olarak kullanılır; yeni alan yok. 1–21 ana yol; `RoomNew-22` (ID 22, 6 Kasım'da 0'dan düzeltildi) oda 11'den sağa açılan, tek çıkışı 11'e dönen bir **yan oda**. Kullanıcı kararıyla (6 Kasım) onda da bayrak var. Cezada öncesi 11 sayılır (`RaceRules.PreviousRoomId`, oda grafiğinden). Eski `RoomNew-MayIncluded` sahneden silindi.

**Bayrak yerleri.** Bir editör menü komutu, `RaceFlagPoint`'i olmayan her odaya onu varsayılan yere koyar: bir sonraki odaya (`RoomId + 1`) giden `RoomTransitionTrigger`'ın hemen önü. Geliştirici sonra sahnede sürükleyip istediği yere taşır. Komut tekrar çalıştırıldığında yalnızca eksik odaları doldurur, taşınmış işaretlere dokunmaz. Böyle bir geçişi olmayan odalarda (21 son oda, 22 yan oda) işaret oda merkezine konur ve havada kalabilir; elle taşınmalı. İşaretler 6 Kasım'da 22 odaya kondu (`NewScene`).

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

**Devam etme.** Lobide kayıt varsa "Devam et" düğmesi görünür (açıkken mod kayıttan gelir). İki taraf da Devam et açıkken Host / Katıl der; `Hello.Resume` ve yarış kimlikleri karşılaştırılır. Uyuşmazsa iki taraf da kendi lobisinde açık bir mesaj görür; uyuşursa `ProgressSync` ile durumlar paylaşılır ve her oyuncu kendi kayıtlı odasının başlangıcına konur. Yarış bitişi henüz yok; kayıt silinmez, Devam et kapatılıp yeni yarış başlatılınca üzerine yazılır.

**Kopma.** Oturum katmanı 3 saniye rakipten paket gelmezse rakibi "koptu" sayar (taşıyıcıdan bağımsız); taşıyıcının kopma olayı da aynı yola girer. Hayalet gizlenir, "Rakip bağlantısı koptu, bekleniyor… (N)" yazısı çıkar. Adil kalsın diye oyuncu dondurulur (`OnStartRespawn`, geri sayımdaki gibi) ve süre durur. Host yeniden dinler, istemci saniyede bir yeniden katılmayı dener. 30 saniye içinde rakip dönerse el sıkışma yenilenir ve 3-2-1 ile sürer; dönmezse kayıt alınır ve lobiye dönülür.

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

0. **Steam'i iki bilgisayarda dene** (MP 9'un kodu 7 Kasım'da yazıldı, bu testi bekliyor): iki bilgisayar, iki Steam hesabı, ikisinde de Steam açık; build lobisinde "Bağlantı: Steam", host Host ol → davet, diğeri daveti kabul eder. Bkz. [`../multiplayer-arkadasla-test.md`](../multiplayer-arkadasla-test.md).
1. Multiplayer assembly'leri, `IRaceTransport`, `InMemoryTransport`, `RaceMessages` ve testleri
2. `RaceSession` kuralları ve testleri
3. `UnityTransportAdapter` ve iki kopyanın bağlanması
4. `LocalPlayerBroadcaster` ve `GhostPlayer` prefab'ı
5. `RaceFlagPoint`, yerleştirme komutu, `RaceFlag`; önce `RoomNew-22`'nin ID'si 22 yapılır (bayrak yeri `RoomId + 1` ile bulunur, ve ID 0 "bilinmiyor" ile karışıyor); ardından geliştiricinin `#level` akşamı: bayrakları yerleştirmek
6. Ceza akışı ve PlayMode testi
7. Lobi sahnesi ve multiplayer HUD
8. Yarış kaydı, devam etme, kopma yönetimi (`RaceSession`'a kayıttan yerel durumu geri yükleme eklenir; ilk bağlantı dahil her bağlantıda `ProgressSync` yollanır, yoksa rakip ilk odada "bilinmiyor" görünür)
9. `SteamTransport` (AppID 480), Steam daveti; yarış bitişi ve sonuç ekranı (7 Kasım)

## Kapsam dışı

- Rastgele eşleşme (yalnızca arkadaşla oynanır).
- Hile koruması.
- İkiden fazla oyuncu.
- Fiziksel etkileşim (itme, birbirine tutunma, paylaşılan platformlar).
- Tek kişilik kayıt sistemi (ayrı bir iş; yarış kaydı ondan bağımsız tasarlandı).
