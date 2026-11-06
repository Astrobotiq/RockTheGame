# MP 7: Lobi sahnesi ve multiplayer HUD — Uygulama planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** `MultiplayerLobby.unity`'den gamepad ile mod seçilsin, host olunsun ya da adrese katılınsın. Bağlantı kurulunca iki taraf `NewScene`'i yüklesin; ikisi de hazır olunca ortak 3-2-1 geri sayım olsun. Oyunda bir HUD bayrak sayacını, rakibin odasını ve bağlantı durumunu göstersin.

**Kullanıcı kararları (6 Kasım):**
- Lobi ayrı sahnedir; şimdilik elle açılır. Build'de ikinci sahnedir, tek kişilik açılış aynı kalır.
- Geri sayım 3-2-1'dir.
- Adres için metin alanı vardır, `127.0.0.1` hazır gelir (Steam gelince kalkar).

## Architecture

- **`RaceLobbyFlow` (saf C#):** el sıkışma ve başlangıç.
  - Bağlanınca `Hello` yollanır.
  - Sürüm uyuşmazsa `Failed`.
  - Modu host belirler: istemcinin `RaceSession`'ı host'un `Hello`'sundan kurulur.
  - İki `Hello` görülünce `ProgressSync` yollanır ve `LoadSceneRequested` tetiklenir.
  - Sahne hazır olunca `Ready` yollanır. İki `Ready` görülünce 3 sn geri sayım başlar, bitince `RaceStarted`.
  - Diğer mesajlar `RaceSession.Receive`'e gider.
  - EditMode'da `InMemoryTransport` ile uçtan uca test edilir.
- **Protokol:** `ReadyMessage` (tip 8, alan yok). `RaceProtocol.Version` 1 → 2.
- **`MultiplayerSession` (MonoBehaviour, `DontDestroyOnLoad`, kök):** taşıyıcıyı ve akışı tutar.
  - Başlatıcıdaki dünya kurulumu buraya taşınır: yayıncı, hayalet, bayraklar, ceza ve HUD.
  - `LoadSceneRequested` gelince `NewScene`'i `LoadSceneAsync` ile yükler; geliştirici başlatıcısında sahne zaten yüklüdür.
  - Dünya kurulunca oyuncuyu dondurur (`OnStartRespawn`), "Başla"da çözer (`OnEndRespawn`).
- **`LocalPlayerBroadcaster`:** kurulurken oyuncunun içinde durduğu odayı ilk oda sayar. Sahne önceden yüklüyse ilk `OnRoomEntered` kaçırılmış olur; bu onu telafi eder.
- **`RaceHud`:** kodla kurulan overlay canvas, `UI` katmanında. Metinler saf `RaceHudText`'ten gelir. Font `Resources/Multiplayer/MultiplayerUISettings` SO'sundan okunur (Tiny5).
- **`MultiplayerLobbyUI`:** sahnedeki düğmelere serileştirilmiş referanslarla bağlanır.
  - Düğmeler: Mod (Yarış ↔ Birlikte), Host ol, adres alanı, Katıl, Vazgeç, durum satırı.
  - Sahne, editör komutuyla eval'den kurulur: `MultiplayerLobbyBuilder`, `#if UNITY_EDITOR`.
  - EventSystem'de `InputSystemUIInputModule` vardır; ilk seçili düğme "Host ol".
- **`MultiplayerDevLauncher`:** incelir. `MultiplayerSession`'ı "sahne yüklü" kipinde kurar, Host ya da Join çağırır, istemci zaman aşımında yeniden dener.
- Multiplayer asmdef'e `Unity.TextMeshPro`, `UnityEngine.UI` ve `Unity.InputSystem` referansları eklenir.

## Review Focus

1. Sürüm uyuşmazlığı → bağlantı reddedilir, lobide mesaj görünür.
2. İstemci kendi modunu değil, host'un modunu kullanır.
3. Geri sayım yalnızca iki taraf da hazırken başlar; ilk yükleyen beklerken donuktur.
4. Ceza ya da bayrak, geri sayım bitmeden işlemez. Oyuncu donukken `PenaltyHandler` bekler; bayrağa zaten dokunamaz.
5. Tek kişilik açılış (`NewScene` build index 0) değişmez. Lobi olmadan multiplayer nesnesi oluşmaz.

## Tasks

1. `ReadyMessage`, codec, sürüm 2; codec testi.
2. `RaceLobbyFlow` + `RaceLobbyFlowTests` (iki akış `InMemoryTransport` ile).
3. `RaceHudText` + test.
4. `MultiplayerSession` (dünya kurulumu başlatıcıdan taşınır), yayıncıda ilk oda düzeltmesi, `RaceHud`, `MultiplayerUISettingsSO`; başlatıcı inceltilir, başlatıcı testleri güncellenir.
5. `MultiplayerLobbyUI` ve `MultiplayerLobbyBuilder`; sahne eval ile kurulur; build listesine index 1 olarak eklenir.
6. Doğrulama:
   - EditMode ve PlayMode testleri;
   - lobi sahnesinde Play (host bekliyor);
   - MPPM ile iki pencere (geri sayım, HUD);
   - belgeler ve commit'ler.
