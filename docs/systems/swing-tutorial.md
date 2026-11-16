# Swing tutorial (oda 5, ilk node)

*Tasarım planı: [`../swing-tutorial-plani.md`](../swing-tutorial-plani.md). Son kod doğrulaması: 2026-11-15 (derleme, EditMode 418/418, PlayMode 41/41; oda 5'te Play Mode ölçümü ve ekran görüntüsü, geçici FakeInput testleriyle; gamepad denemesi yapılmadı, ST-4).*

## Tasarım niyeti

Oda 5 kancanın tanıtıldığı oda. Eskiden node'un altında statik iki satır vardı ("Move Right Arm", "Right Grapple"); oyuncu ikisini birden okuyup havadayken aynı anda yapmak zorundaydı ve çoğu kişi asidin içine düşüyordu. Şimdi oyuncu **kendisi zıplar**; node menzile girince zaman yavaşlar ve tepede durur. Önce kolu node'a çevirmeyi, sonra ateşlemeyi öğrenir. Tutununca zaman normale döner ve sarkaç yavaştan hızlanarak başlar. Akış oyuncuyu cezalandırmaz: beklenmedik durumda zamanı bırakıp başa döner.

## Nasıl çalışıyor

| Adım | Ne olur | Biter |
|---|---|---|
| `Waiting` | Hiçbir şey. İpuçları gizli. | Oyuncu `AirborneState`'te, anchor yok, node `NodeDetector` kurallarıyla erişilebilir. |
| `Slowing` | `timeScale` 1 → `SlowScale` (0.1), `RampInDuration` (0.15 sn). 1. ipucu girer. | Rampa bitti ve dikey hız ≤ 0 (tepe). |
| `Aim` | `timeScale` = `HoldScale` (0). Oyuncu tepede asılı. 1. ipucu ("Move Right Arm" + RS) görünür. | Sağ stick node'u seçiyor ve `AimConfirmTime` (0.2 sn) sürdü. |
| `Fire` | 1. ipucu çıkar, 2. ipucu ("Right Grapple" + RT) girer. Zaman hâlâ bekliyor. | Oyuncu node'a tutundu. Nişan `AimLostGrace` (0.3 sn) kaybolursa `Aim`'e döner. |
| `Done` | `timeScale` `SlowScale` → 1, `RampOutDuration` (0.3 sn). İpuçları çıkar. Bu oturumda bir daha çalışmaz. | — |

- Yükselirken zaten nişan alıp ateşlenirse `Slowing`/`Aim`'den doğrudan `Done`'a geçilir. `Waiting`'de node'a tutunulursa da `Done`.
- **Güvence (zaman kilitlenmesin):** `Slowing/Aim/Fire` sırasında oyuncu havadan çıkarsa (iniş, dash), başka bir anchor'a tutunursa ya da (`Aim/Fire`'da ya da tepede) node menzilden çıkarsa akış `Waiting`'e döner ve zaman hemen 1 olur. Dash menzile girmek için kullanılabilir: dash sırasında `Waiting` kalır, bitince havadaysa başlar (Play Mode'da doğrulandı).
- Tüm sayaçlar unscaled zamanla, pause'da ilerlemez. Rampa tepede bekleyişten önce bitmek zorunda: yürüyerek düşüşte de yavaşlama görünür.
- Tutorial yalnızca tek oyunculuda çalışır: `PauseManager.IsPauseBlocked` (çok oyunculu oturum pause'u kapatır) true ise hiçbir şey yazmaz, zamanı bırakır, ipuçları gösterilmez.

### Neden tepede tamamen durma (`HoldScale` 0)

Node tepede menzilin sınırında (aşağıya bak). `HoldScale` 0.02 olsa 20 sn beklemede oyuncu ~4 birim düşer, node menzilden çıkar, tutorial kilitlenir. 0'da bu imkânsız. "Yavaş akmaya devam etsin" istenirse `HoldScale` artırılır; o zaman `Aim`'e "menzilden çıktıysa 0'a in" güvencesi eklenmeli (şu an `Aim/Fire`'da menzil kaybı tutorial'ı bırakıyor, bu bir yedek). Bu bir his kararı.

`timeScale` 0 iken çalışanlar: `PlayerController.Update` → `AirborneState.UpdateState` → `ProcessGrappleInput`, `ArmController`, `TargetVisualizer` (kol ve nişangâh anında döner). Çalışmayanlar: `FixedUpdate` (yerçekimi, hareket), kamera yumuşatması, `Time.deltaTime` sayaçları.

## Kod

- `Tutorial/SwingTutorialSettingsSO.cs` + `Tutorial/SO/SwingTutorialSettings.asset`: `SlowScale`, `HoldScale`, `RampInDuration`, `RampOutDuration`, `AimConfirmTime`, `AimLostGrace`. His kararı; gamepad ile ayarlanır.
- `Tutorial/SwingTutorialFlow.cs`: saf C#, girdilerden `Step` ve `TargetTimeScale` üretir (`SwingTutorialFlowTests`, 30 test). `IsControllingTime` false'a döndüğü karede `TargetTimeScale` bir kez daha yazılmalı (değer tam 1).
- `Tutorial/SwingTutorial.cs`: oyuncudan girdileri toplar. `nodeReachable` ve `aimedAtNode` için yeni API yok, `PlayerController.TryCastGrapple` (detector'ın yarıçap, koni, engel kuralları) ve `NodeCoincidenceThreshold`. `timeScale` + `fixedDeltaTime` (= taban × ölçek, 0'da dokunulmaz) yazar; yalnızca kendisi değiştirdiyse geri yükler (pause'un 0'ını ezmez). `OnDeath`'te ve `OnDisable`'da geri yükler, akışı sıfırlar (bitmişse kapalı kalır). `StepChanged` olayı.
- `Tutorial/SwingTutorialHints.cs`: `StepChanged`'e göre iki ipucunu gösterir (`VisibleHints` tablosu, `SwingTutorialHintsTests`). Bir ipucu grubun yalnızca ikon (`Animation`) ve yazı (`Tutorial-Text (1)`) parçalarıdır; her parça kendi merkezinden `UiSpring` ile (stiffness 340, damping 22) büyür/küçülür, unscaled zaman; başta gizli; görünürken **düz durur** (nefes 15 Kasım'da kaldırıldı, playtest). Parçalar kapanıp açılınca Animator parametreleri sıfırlandığı için `UIAnimatorController` kapatılıp açılarak doğru animasyon (RS / RT) yeniden yazdırılır. Grubun kökü ve içindeki "SWING" `TextEffect` yazısı hiç kapanmaz: ipuçlarının dışında, hep sallanır. Çıkış girişle aynı yay (hedef 0), ölçek 0'ın altına inmez.
- `Pause/PauseManager.IsPauseBlocked`: çok oyunculu oturum göstergesi.
- `SwingTutorial.arm` (`SwingTutorialArm` Right/Left): nişanın hangi stick ile alındığı (`Input.RightStick` ya da `LeftStick`). Ateş kolu tutorial'ın işi değil: tutunma hangi kolla olursa olsun başarılı sayılır; ipuçları hangi kolu öğrettiğini söyler.
- Sahne (NewScene): `RoomNew-5/DynamicContent/SwingTutorial` (ilk node `Circle (1)`, sağ kol; `SwingTutorial` + `SwingTutorialHints`) ve `SwingTutorial (Left)` (ikinci node `Circle` (202.4, 69.0), sol kol; hint grupları `Tutorial-Animation-TextEffect (2)` = "Move Left Arm" + LS, `(3)` = "Left Grapple" + LT). İki bileşen ayrı akış tutar (her biri oturumda bir kez); node'lar 25 birim arayla, menzil 10, aynı anda erişilebilir olmazlar. Üçüncü node (`Circle (3)`, 222.6) tutorial'sız. İpucu grupları `Tutorial-Animation-TextEffect` (RS) ve `Tutorial-Animation-TextEffect (1)` (RT); ikisinin `Animator.updateMode`'u `UnscaledTime`. `DynamicContent` altında, çünkü `Room.Sleep` yalnızca onu kapatıyor: oda uyuyunca tutorial da kapanır, zaman geri yüklenir. 

## Ölçümler (2026-11-15, NewScene oda 5)

- Kenar üst yüzeyi y 61.2, ilk node (177.46, 69.0), asit y ≈ 53.6, `detectionRadius` 10.
- Kenardan sağa tam stick ile zıplayınca node menzile **tepede** giriyor (yükselişte değil): tepe konumu ~(168.4, 65.7), node uzaklığı **~9.65**, yani marj ~0.35 birim. Plan ~7 demişti. Pratikte yavaş bir yükseliş yok, ~0.15 sn rampadan sonra tepede duruş. Biraz kısa zıplayan oyuncuda node menzile hiç girmeyebilir ve tutorial başlamaz (statik ipucu da yok).
- İkinci node (sol kol): Play Mode'da ((193, 63)'ten havada başlatılıp) `Slowing` node menzile (10) yükselişte 10,48'de girince, tepede mesafe ~9,0'da `Aim`. Sağ stick bu tutorial'ı ilerletmiyor, sol stick `Fire`'a geçiriyor, LT tutunduruyor, zaman 1'e dönüyor. **Birinci node'dan salınıp ikinciye gelen gerçek yol denenmedi**: sahte girdiyle birinci node'dan bırakınca oyuncu ikinci node'a ~17 birim uzakta düşüp asite girdi; gerçek oyuncu başka bir yerden bırakıyorsa gamepad ile (ST-4) bakılmalı. Birinci node'un kancası hâlâ takılıyken (`HasAnyAnchor`) ikinci tutorial başlamaz.
- Dik zıplama tetiklemiyor. Dash ile menzile girmek çalışıyor (dash sonrası ~6 birim, `Aim`'e varıyor).

## Bilinen kenar durumlar ve açık kararlar

1. **Menzil marjı küçük.** Yukarıdaki ölçüm; çözüm için `detectionRadius` (tüm kancayı etkiler), tutorial'a özel daha geniş erişim kontrolü ya da node yerini oynatmak seçenekleri var. Karar geliştiricinin.
2. **Kanca kilidi donabilir.** `AirborneState` kilit sayaçları `Time.deltaTime` ile düşer; `timeScale` 0 iken sayaç durur. Tepede kilit sürüyorsa (örneğin bir sallanmadan hemen sonra) ateş çalışmaz, çıkış pause → Retry. Normal kenar zıplamasında kilit 0.
3. **Sahne pause'dayken kapanırsa** `PauseManager.OnDisable` saklı değeri geri verir, `SwingTutorial.OnDisable`'ın sırası belirsiz. Quit menüsü önce devam ettiği için sorun yok.
4. **"SWING" yazısı** (`Tutorial-TextEffect (1)`, `<link=Rotate>`; `UI/UIEffects/Swing.asset` = `Effect_Rotate`) hep sallanır. Efekt `ScaledTime` iken tepede (`timeScale` 0) donuyordu; `Swing.asset`'in `timeType`'ı `UnscaledTime` yapıldı (15 Kasım). Bu paylaşılan asset, aynı efekti kullanan diğer "SWING" yazıları da (oda 21) artık zaman ölçeğinden bağımsız sallanır; pause'da da sallanır. İpucu ikonlarının Animator'ı `UnscaledTime`: yavaş zamanda akar.
5. **Gamepad ile denenmedi.** `SlowScale`, `HoldScale`, rampalar, ipucu yayı his kararıdır (ST-4).
6. **Kalıcı kayıt yok:** tutorial oturum başına bir kez; kayıt sistemi gelince "bir daha gösterme" oraya bağlanır.
