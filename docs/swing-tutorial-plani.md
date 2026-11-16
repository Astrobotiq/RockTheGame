# Swing tutorial planı (oda 5, ilk node)

*Plan: 2026-11-15 (Opus). Uygulama: Sonnet, aşağıdaki task'larla. Kod henüz yok.*

## Amaç

Oda 5 kancanın tanıtıldığı oda. Bugün ilk node'un altında statik iki satır var: "Move Right Arm" (sağ stick animasyonu, `UIAnimations.RS_All`) ve "Right Grapple" (`RT_Button`). Oyuncu ikisini birden okuyup havadayken aynı anda yapmak zorunda; çoğu kişi asidin içine düşüyor.

Yeni akış: oyuncu **kendisi zıplar**. Node menzile girdiği anda zaman yavaşlar ve tepe noktasında bekler. Önce kolu node'a çevirmeyi, sonra ateşlemeyi öğrenir. Tutununca zaman normale döner ve sarkaç yavaştan hızlanarak başlar.

## Ölçümler (2026-11-15, `NewScene`)

| Şey | Değer |
|---|---|
| Kalkış kenarı | x ≈ 161–166.5, üst yüzey y = 61.2 |
| İlk node (`RoomNew-5/DynamicContent/Circle (1)`, katman `Grapple`) | (177.46, 69.0) |
| Asit yüzeyi | y ≈ 53.6 (kenarın ~7.5 altı) |
| `NodeDetector.detectionRadius` (sahne) | 10 |
| Zıplama | `JumpHeight` 4, `TimeToApex` 0.5 |
| Input System | `ProcessEventsInDynamicUpdate` (yavaş zamanda girdi gecikmez) |
| Fizik adımı | `fixedDeltaTime` 0.02 |

Sonuç: kenarda dururken node ~13 birim uzakta, **menzil dışında**; yerden sağ tetik hiçbir şey yapmaz. Kenardan dik yukarı zıplayınca da menzile girmez (~12). Öne zıplayınca yükselişte menzile girer, tepe noktasında node ~7 birim uzakta. Yani "node menzile girdi" koşulu kendiliğinden yalnızca doğru zıplamada tetiklenir; elle yerleştirilmiş bir tetik bölgesine gerek yok.

## Oyuncu zıpladığında ne olur (adım adım)

| # | Durum | Ne olur | Bitiş koşulu |
|---|---|---|---|
| 0 | **Bekliyor** | Hiçbir şey. Node'un statik iki ipucu satırı gizli. | Oyuncu `AirborneState`'te, hiçbir anchor'ı yok, node `NodeDetector` kurallarıyla erişilebilir. |
| 1 | **Yavaşlıyor** | `timeScale` 1 → `SlowScale` (0.1), `RampInDuration` (0.15 sn, unscaled) içinde. 1. ipucu ("Move Right Arm" + RS animasyonu) yaylanarak girer. | Oyuncunun dikey hızı ≤ 0 (tepe). |
| 2 | **Kolu çevir** (tepede bekler) | `timeScale` → `HoldScale` (0). Oyuncu tepe noktasında asılı kalır. | Sağ stick node'u seçiyor (`TryFindBestNode(RightStick)` bu node'u döndürüyor) ve bu `AimConfirmTime` (0.2 sn, unscaled) boyunca sürüyor. |
| 3 | **Ateşle** | 1. ipucu çıkar, 2. ipucu ("Right Grapple" + RT animasyonu) girer. Zaman hâlâ bekliyor. Nişan `AimLostGrace` (0.3 sn) boyunca kaybolursa 2. adıma döner. | Oyuncu bu node'a tutundu (`RightAnchor` ya da `LeftAnchor` node konumuna eşit). |
| 4 | **Bitti** | `timeScale` `SlowScale` → 1, `RampOutDuration` (0.3 sn, unscaled). İpucu çıkar. Tutorial bu oturumda kapanır. | — |

1. adım 2. adıma geçmeden de bitebilir: oyuncu yükselirken zaten nişan alıp ateşlerse doğrudan 4'e geçilir. Akış hiçbir zaman oyuncuyu cezalandırmaz.

### Neden tepede tamamen durma (`HoldScale` 0)

Tepe noktasında node ~7 birim uzakta, menzil 10. `HoldScale` 0.02 olsa 20 saniyelik beklemede oyuncu ~4 birim düşer ve node menzilden çıkabilir; tutorial kilitlenir. 0'da bu imkânsız. "Yavaş akmaya devam etsin" istenirse `HoldScale` artırılır, ama o zaman 2. adıma "menzilden çıktıysa 0'a in" güvencesi eklenmeli. Bu bir his kararı; başlangıç 0.

`timeScale` 0 iken de çalışanlar: `PlayerController.Update` → `AirborneState.UpdateState` → `ProcessGrappleInput` (kanca Update'te alınır), `ArmController` ve `TargetVisualizer` (deltaTime kullanmıyorlar, kol ve nişangâh anında döner). Çalışmayanlar: `FixedUpdate` (yerçekimi, hareket), kamera yumuşatması, ip atma animasyonu (`GrappleVisualManager` `Time.deltaTime`; çıkış rampasında yavaş oynar, bu iyi görünebilir).

## Dikkat edilecek etkileşimler

1. **Pause.** `PauseManager` duraklarken o anki `timeScale`'i saklıyor ve devamda geri veriyor (`pause.md` §3). Tutorial `PauseManager.BlocksGameplay` true iken `Time.timeScale`'e **yazmamalı** ve rampa sayaçlarını ilerletmemeli (rampa unscaled zamanla sayıyor, pause'da da akar).
2. **Ölüm / Retry.** `PlayerHealth.OnDeath` → zamanı hemen 1'e döndür, ipuçlarını gizle, akışı 0'a al (bitmediyse tekrar oynar). `timeScale` 0'da hazard'lar da donuk, yani ölüm pratikte yalnızca pause menüsündeki Retry ile olur.
3. **`fixedDeltaTime`.** Yavaşken `fixedDeltaTime = base × timeScale` yap ki yavaş hareket kesik kesik görünmesin; `base`'i `Awake`'te oku, sabit yazma. `timeScale` 0 iken dokunma (FixedUpdate zaten çalışmıyor). Her çıkışta geri yükle.
4. **Disable / oda değişimi.** `OnDisable`'da `timeScale` ve `fixedDeltaTime` geri yüklenir (oda uyutulursa ya da sahne kapanırsa zaman takılı kalmasın).
5. **Çok oyunculu.** Oda 5 yarış pistinde (`RaceFlagPoint` var). `timeScale` yerel; rakibin hayalet interpolasyonunu da yavaşlatır ve yarışta haksızlık olur. `MultiplayerSession.Current != null` ise tutorial hiç çalışmaz, statik ipuçları da gösterilmez.
6. **Hit-stop.** `HitStopManager` unscaled sayıyor, bağımsız. Tutorial sırasında hit-stop beklenmiyor.
7. **İpucu animasyonları.** İki grubun `Animator.updateMode`'u şu an `Normal`: yavaş zamanda gamepad ikonu donar. `UnscaledTime` yapılmalı. `EasyTextEffects.TextEffect` ("SWING" yazısı) kendi zamanını nasıl saydığı doğrulanmadı; Sonnet kontrol etsin, donuyorsa sorun değil (node'un üstündeki yazı, ipucu değil).
8. **Tetik yalnızca sağ kol mu?** 3. adımın bitişi her iki kolu da kabul eder: oyuncu sol kolla tutunursa da başarılı sayılır. İpuçları sağ kolu öğretir (yerden kanca yalnızca sağ tetikle çalışıyor, `grapple.md`).

## Tasarım (kod yapısı)

Yeni klasör `Assets/Scripts/New-Scripts/Tutorial/`, namespace `New_Scripts.Tutorial`.

- **`SwingTutorialSettingsSO`** — tunable'lar: `SlowScale` 0.1, `HoldScale` 0, `RampInDuration` 0.15, `RampOutDuration` 0.3, `AimConfirmTime` 0.2, `AimLostGrace` 0.3. Asset: `Assets/ScriptableObjects/Tutorial/SwingTutorialSettings.asset` (ya da projedeki SO klasörü neredeyse orası).
- **`SwingTutorialFlow`** — saf C# (MonoBehaviour değil), EditMode testli. Her kare girdileri alır: `isAirborne`, `hasAnyAnchor`, `nodeReachable`, `aimedAtNode`, `anchoredToNode`, `velocityY`, `unscaledDt`, `paused`. Çıktı: `Step` (Waiting / Slowing / Aim / Fire / Done) ve `TargetTimeScale`. Yukarıdaki tabloyu birebir uygular. `Reset()` ölüm için.
- **`SwingTutorial`** (MonoBehaviour, oda 5'te node'un yanında) — referanslar: `PlayerController` (yoksa `FindFirstObjectByType`), node `Transform`, `NodeDetector`, iki ipucu kökü, settings SO. Her `Update`'te:
  - `nodeReachable`: `nodeDetector.TryFindBestNode((node - player).normalized, playerPos, out p)` ve `p ≈ node` (detector'ın kendi yarıçap, koni ve engel kurallarını kullanır; yeni API yok).
  - `aimedAtNode`: aynısı `Input.RightStick` ile (stick ölü bölgedeyse false).
  - `anchoredToNode`: `LeftAnchor`/`RightAnchor` node konumuna `NodeCoincidenceThreshold` içinde.
  - `isAirborne`: `CurrentState is AirborneState`.
  - Akıştan gelen `TargetTimeScale`'i `Time.timeScale`'e yazar (yalnızca akış Waiting/Done dışındayken ya da çıkış rampasındayken; pause'da yazmaz), `fixedDeltaTime`'ı ölçekler.
  - `Step` değişince ipucu gruplarını gösterir/gizler.
- **İpucu hareketi** — mevcut iki grup (`Tutorial-Animation-TextEffect` = Move Right Arm + RS, `Tutorial-Animation-TextEffect (1)` = Right Grapple + RT) yeniden kullanılır, yeni grafik yok. Giriş: ölçek 0 → 1 `UiSpring` ile (yeni tween yazma, `ui-motion.md`), unscaled. Çıkış: yayla küçülüp kapanır. Boşta ~%3 nefes. `ui-motion.md` kontrol listesinden geçirilir; değerler his kararı.

Neden tetik bölgesi yok: "node erişilebilir + havada" koşulu ölçümlere göre yalnızca öne zıplamada doğru. Oda düzeni değişirse bu koşul yine geçerli kalır; bir bölge collider'ı ise elle güncellenmek zorunda kalırdı.

## Kapsam dışı (şimdilik)

- ~~İkinci node (sol kol)~~ — 2026-11-15'te yapıldı: aynı bileşen, `arm` alanı (Right/Left) ve `Circle` node'u; bkz. `systems/swing-tutorial.md`.
- Yavaş zamanda ses (pitch/lowpass), titreşim tıkı, ekran efekti — his eklemeleri; playtest sonrası.
- Pompalamayı öğretmek (stick X) — node'daki "SWING" yazısı bunu zaten söylüyor.
- Kalıcı kayıt: tutorial oturum başına bir kez. Kayıt sistemi gelince "bir daha gösterme" oraya bağlanır.

## Task'lar

| # | Task | Bitti sayılır |
|---|---|---|
| ST-1 | **Akış mantığı + testler** #code — `SwingTutorialSettingsSO` + `SwingTutorialFlow` + EditMode testleri (`Assets/Tests/EditMode/SwingTutorialFlowTests.cs`): tablo geçişleri, yükselişte ateşleyince doğrudan Done, nişan kaybında Aim'e dönüş (grace içinde dönmez), pause'da sayaç durur, Reset. | Derleme temiz, testler yeşil, çıktı raporlandı. |
| ST-2 | **Sahne bileşeni ve zaman** #code — `SwingTutorial` MonoBehaviour: oyuncu/node sorguları, `timeScale` + `fixedDeltaTime` yazımı ve geri yükleme (OnDisable, ölüm), pause ve çok oyunculu korumaları. Oda 5'e `unity command eval` ile eklenir (scene YAML elle yazılmaz). | Derleme + tüm EditMode ve PlayMode testleri yeşil (`bozulmamali-davranislar.md`). Play Mode'da sanal gamepad: zıpla → zaman yavaşlar → tepede durur → sağ stick → RT → tutunur, zaman 1'e döner. Pause sırasında ve Retry sonrası `timeScale`/`fixedDeltaTime` doğru. |
| ST-3 | **İpuçları ve belge** #code — iki grubun gösterim/gizlemesi `UiSpring` ile, Animator'lar `UnscaledTime`, başlangıçta gizli; `ui-motion.md` listesi. `docs/systems/swing-tutorial.md` yaz (bu plandaki bölümlerle, "Son kod doğrulaması" tarihli), `docs/systems/README.md` tablosuna ve `grapple.md` "Kod" bölümüne bağla. | Derleme + testler yeşil. Play Mode `--source screen` ekran görüntüsü: 2. ve 3. adımda doğru ipucu görünüyor. |
| ST-4 | **Swing tutorial'ı gamepad ile dene** #test — geliştirici. Kontrol: kenardan öne zıpla, dik zıpla (tetiklenmemeli), kenardan yürüyerek düş, yükselişte erken ateşle, sol kolla tutun, beklerken pause → devam, Retry, ikinci geçiş (çalışmamalı). His: `SlowScale`, `HoldScale`, rampalar, ipucu yayı. | Notlar `SwingTutorialSettings` değerleriyle birlikte yazıldı. |

Sıra: ST-1 → ST-2 → ST-3 → ST-4. ST-2 ST-1'in tipine bağlı; ST-3 ST-2'nin `Step` olayına.
