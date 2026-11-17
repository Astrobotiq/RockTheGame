# Kanca ailesi: swing, dual swing, slingshot, dönen node

*GDD §2 (sütun 2), §5.1, §5.2 "Kanca ailesi". Son kod doğrulaması: 2026-11-17 (hareketli node: anchor node'u izler, taşır, bırakışta hızını verir).*

## Tasarım niyeti

Oyunun kalbi. İki bağımsız kol var, her biri bir node'a tutunabiliyor. Tek kol sarkaç, iki kol yay gibi gerilen bir salınım, aynı node'daki iki kol da geri gerilip fırlayan bir sapan. Yeni bir mekanik ya da oda fikri kancayla konuşmuyorsa gözden geçirilir (GDD §2).

## Kontroller

| Girdi | Ne yapar |
|---|---|
| Sol tetik (basılı tut) | Sol kol, **sol stick** yönündeki node'a tutunur. Bırakınca kopar. |
| Sağ tetik (basılı tut) | Sağ kol, **sağ stick** yönündeki node'a tutunur. Bırakınca kopar. |
| Tutunan kolun stick'i, X ekseni | Sarkacı pompalar (sol kolla sallanırken sol stick, sağ kolla sallanırken sağ stick) |
| İki stick'in X ortalaması | Dual swing'de pompalama |

Kanca bir tetikleyici değil, **basılı tutulan** bir eylem: tetik eşiği `InputHandler.triggerThreshold` = 0.1. Stick boştayken tetiğe basmak hiçbir şey yapmaz (`NodeDetector` yön ister). GDD §5.1'deki kontrol tablosunda sağ stick'in bu rolü yazmıyor.

## Hedef seçimi (`NodeDetector`)

- Oyuncunun etrafında `detectionRadius` yarıçapında node katmanındaki collider'lar taranır. Sahnedeki değer **10**, koddaki varsayılan 15.
- Stick yönüyle node yönü arasındaki nokta çarpımı `minimumDotProduct` (0.5) ve üstündeyse aday olur. Bu, stick'in iki yanında ±60°'lik bir koni demek.
- Adaylardan **en yakını değil, stick'le en iyi hizalanan** seçilir. Oyuncu ile node arasında engel (`obstacleLayerMask`) varsa aday elenir.
- Tutunma anlıktır: mermi ya da uçuş süresi yok. İp animasyonu (`GrappleVisualManager`) yalnızca görsel. `TargetVisualizer` her iki stick için o an seçilecek node'u nişangâhla gösterir.
- Anchor, node'un `transform.position`'ıdır. Kol node'un `Transform`'unu da tutar (`PlayerController.AttachAnchor`) ve anchor her fizik adımında node'un konumunu izler. Bkz. "Hareketli node".

## Durumlar

### Yerden kanca (swing hop)

Yerdeyken **yalnızca sağ tetik** çalışır (`NormalState.CheckGroundGrappleInput`), sol tetik dalı yok. Tutununca `SwingingState(wasGrounded: true)` önce `SwingHopDuration` boyunca node tarafına doğru bir sıçrama yapar (`SwingHopForwardSpeed` yatay, `SwingHopUpwardSpeed` dikey, normal yerçekimi). Sıçrama bitince ip uzunluğu o anki mesafeye sabitlenir ve sarkaç başlar. Yerde sol kol görsel olarak hareket yönüne bakar, sağ kol sağ stick'e.

Bu asimetri bilinçli bir tercih (GDD §5.1).

### Tek kanca (`SwingingState`)

- İp boyu tutunma anında sabitlenir. Yerçekimi normal `Gravity` (−32); `SwingGravity` burada **kullanılmıyor**.
- Her fizik adımında hız ipe teğet olacak şekilde izdüşürülür, oyuncu tekrar ip boyuna oturtulur.
- Pompalama `SwingForceMultiplier` ile teğet hıza eklenir. `MaxSwingSpeed × ActiveSpeedMultiplier` üstüne pompalayarak çıkılamaz, ama yerçekimiyle bu sınır aşılabilir (sınır yalnızca girdinin eklenmesini engeller).
- Girişte dash hakkı ve taç rengi yenilenir.
- **Çarpışma:** Oyuncunun kendi kutusuyla (1x1) BoxCast. Çarpılan yüzey yataydan en fazla `MaxWallAngle` (45°) eğikse karakter yere iner (`NormalState`, yerde). Daha dikse kanca kopar ve karakter havaya geçer.
- Swing sırasında dash ve duvara tutunma yok.
- **Bırakma:** tetik bırakılınca `AirEntry.SwingRelease()`. Bu modda yükselirken hep `JumpEndEarlyGravityMultiplier` (3×), düşerken `FallGravityMultiplier` (1.5×) uygulanır; apex hang yok. Yani swing'den yukarı fırlayış zıplama tuşundan bağımsız ve hızla söner.

### Çift kanca (`DualSwingingState`)

- Tek kancadayken diğer tetiğe basıp diğer stick'le nişan alınca girilir. Havadayken iki tetik aynı karede basılırsa önce sol, sonraki karede sağ işlenir.
- Fizik farklı: iki ip birer **tek yönlü yay**. Yalnızca gerildiğinde (`mesafe > ip boyu`) `DualSpringStiffness` ve `DualSpringDamping` ile çeker, gevşekken hiçbir şey yapmaz. Yerçekimi `SwingGravity` (25), yani tek kancadan daha hafif.
- Pompalama iki stick'in X ortalaması × `DualSwingForceMultiplier`. Hız sınırı yok.
- Durum yalnızca `Velocity`'yi hesaplar; pozisyonu `PlayerController.FixedUpdate` içindeki `KinematicPhysicsHandler.Move` ilerletir. (2026-11-14'e kadar durum pozisyonu da kendisi set ediyordu, hareket adım başına iki kez uygulanıyordu.)
- Bir tetik bırakılırsa diğer kolla tek kancaya döner (hop yapmadan). İkisi birden bırakılırsa `AirEntry.Fall()`: sabit yerçekimi, değişken zıplama çarpanları yok.
- Çarpışmada açı kuralı tek kancayla aynı (`MaxWallAngle`), ama arama oyuncu kutusuyla değil `DualSwingCollisionRadius` (0.4) daireyle yapılıyor.
- Girişte dash hakkı yenilenir ama taç rengi sıfırlanmaz. Tek kancadan geçildiği için pratikte zaten sıfırlanmış olur.

### Slingshot (`SlingshotState`)

- **Koşul:** iki anchor arası mesafe `< NodeCoincidenceThreshold` (0.5) **ve** `CanSlingshot`. Hak yoksa aynı node'a iki kolla tutunmak dual swing olur.
- Pratikte: aynı node'a önce bir tetikle, sonra diğer tetikle ve **diğer stick'le** nişan almak gerekir.
- **Girişte hak harcanır** (`UseSlingshot`, kollar "tükendi" rengine döner). Gerilme sırasında tetikler bırakılsa bile hak gider.
- **Gerilme (anticipation):** `SlingshotAnticipationDuration` boyunca node'un tersine `SlingshotAnticipationSpeed` hızla geri çekilir.
- **Fırlatma:** kamera hedefe atlar (`OnSlingshotLaunch`), ses çalar, `NotifyImpact` → hit-stop (`HitStopDuration`) + kamera sarsıntısı + titreşim. Sonra hız `SlingshotLaunchDuration` içinde ease-out ile `MaxSlingshotSpeed`'e çıkar. Yön, **girişteki konumdan node'a** doğru sabittir.
- Asset'te `SlingshotLaunchDuration` 0.01, fizik adımı 0.02: fırlatma tek adımda tam hıza ulaşır ve aynı adımda havaya (`NormalState`) geçer. Ease-out pratikte devre dışı.
- **Çıkış:** `AirEntry.SlingshotLaunch(SlingshotGrappleLockout)`. Tek kanca bırakışıyla aynı kural: yükselirken hep 3×, düşerken 1.5×, apex hang yok, **zıplama tuşundan bağımsız** (16 Kasım 2026). Eskiden A basılıyken 1× + apex hang veriyordu; geliştirici "slingshot'tan sonra tuşu basılı tutup daha uzun havada kalmak saçma" dedi. Erken bırakma (gerilme sırasında iki tetik bırakılırsa) da aynı kuralı kullanır. Yatay hız hava kontrolüyle söner: stick boşsa `AirDrag` 20 birim/sn², stick ileri basılıysa hedef 10'a `AirAcceleration` 40 ile iner.
- **Yenilenme:** yere inmek, slingshot yenileyici toplanabilir ([`collectibles.md`](collectibles.md)), launch pad ([`platforms.md`](platforms.md)) ve respawn. Kancaya tutunmak yenilemez.

### Hareketli node

*Tasarım: [`../hareketli-node-tasarimi.md`](../hareketli-node-tasarimi.md). Kurallar M1–M5: [`../bozulmamali-davranislar.md`](../bozulmamali-davranislar.md).*

- **İzleme:** `PlayerController.FixedUpdate`, durumdan önce `RefreshMovingAnchors()` ile anchor'ı node'un konumuna çeker; o adımdaki kayma `AnchorDelta(kol)`, hız `AnchorVelocity(kol)`. Node `PlatformController` (`Rigidbody2D.MovePosition`) ile hareket ettiği için anchor node'u **bir fizik adımı geriden** izler. Node pasifleşir ya da yok olursa anchor son konumda kalır, kanca kopmaz.
- **Ham atama sabittir:** `LeftAnchor`/`RightAnchor`'a doğrudan nokta atamak node bağlantısını siler (testler ve anchor'ı silen yerler bunu kullanır).
- **Atalet (17 Kasım 2026, playtest: "node hareketi swing'e hiç etki etmiyor"):** node'un hızlanması, durması ve dönmesi salınıma geçer. Her adımda node hızı `MovingNodeMaxPivotAccel` (60 b/sn²) sınırıyla yumuşatılır; bu yumuşatılmış hızdaki değişim × `MovingNodeInertia` (0.6; 0 = yalnızca taşıma, 1 = tam fizik) node'a göre hızdan çıkarılır. Radyal kısmı ip kısıtı atar. Çift kancada iki node'un ortalaması kullanılır; slingshot'ta atalet yok. Sınır, `LinearPingPong`'un tek adımlık dönüşünü ~0.17 sn'ye yayar; `SinePingPongStrategy`'de ivme zaten süreklidir ve sınıra takılmaz.
- **Yakalama ve durum geçişleri:** kanca durumları birbirine **dünyaya göre** hız devreder; durum içinde node'a göre hız = dünya − node. `AttachAnchor` node hızını `IMovingSurface.DeltaPosition`'dan anında aldığı için hareket eden node'u yakalayan oyuncu node'un hızını almaz, ip onu çeker.
- **Taşıma:** durumların hızı node'a göre kalır; node kayması ayrıca eklenir. Tek kancada sarkaç kısıtından önce konuma eklenir (çarpışma taraması dahil), çift kanca ve slingshot'ta iki anchor'ın ortalaması hız olarak `Move`'a verilir. Yerden kanca hop'u sırasında taşıma yok, yalnızca anchor güncellenir.
- **Fırlatma:** tek kanca bırakışı, çift kancada iki tetiği birden bırakma ve slingshot çıkışında (ortalama) node hızı × `MovingNodeLaunchMultiplier` `AirEntry.CarriedVelocity` ile aktarılır: Y normal hıza eklenir, X taşınır ve hava sürtünmesinden etkilenmez (duvardan ayrılma kuralı, [`platforms.md`](platforms.md)). Çarpışmayla kopma ve çift kancadan tek kancaya düşüş aktarmaz.
- **Prefab:** `Assets/Prefab/New/Node/Node-Moving.prefab`: kök `PlatformController` + kinematik `Rigidbody2D` + `SinePingPongStrategy` (uçlarda yavaşlayan gidip gelme, period 4), child `Node` (node katmanı, oda node'larının kopyası). `startPoint`/`endPoint` **dünya koordinatı**: her yerleştirmede ayarlanmalı, yoksa node (0,0)–(6,0) arasına ışınlanır. Başka strateji (dairesel vb.) aynı şekilde takılabilir.

### Dönen node (`FullRotationNode`)

- Yalnızca **tek kanca** sırasında sayılır. Tutunulan noktada `FullRotationNode` varsa bir `FullRotationTracker` oluşur. Tracker ardışık yönler arasındaki işaretli açıyı biriktirir, iki yön de sayılır.
- `targetDegrees` (varsayılan 360) dolunca node'un efekti tetiklenir. Kısıt modları: `OncePerConnection` (her tutunuşta bir kez) ve `OncePerNode` (node başına bir kez; ölünce sıfırlanmıyor, `IResettable` değil).
- Bağlantı kopunca ilerleme halkası `lerpBackSpeed` ile geri sıfırlanır.
- Efektler (`IFullRotationEffect`):
  - `SpeedBoostRotationEffect`: hızı bir kez ×1.2 yapar ve `boostDuration` (1.2 sn) boyunca `ActiveSpeedMultiplier`'ı 1.8 yapar. Bu çarpan yalnızca tek kancadaki `MaxSwingSpeed` sınırını yükseltir. Prefab'ı `Node-FullRotation` hiçbir odada kullanılmıyor.
  - `RotationEventTrigger`: UnityEvent. `Node-EventTrigger` prefab'ı oda 17'de bir kapının `Open`'ını çağırıyor.

## Tunable'lar (`PlayerStats.asset`)

| Alan | Değer | Okuyan |
|---|---|---|
| `SwingForceMultiplier` | 20 | tek kanca |
| `MaxSwingSpeed` | 25 | tek kanca |
| `SwingCollisionRadius` | 0.5 | hiçbir yer (ölü alan; tek kanca artık oyuncu kutusunu kullanıyor) |
| `MaxWallAngle` | 45 | tek + çift kanca |
| `SwingGravity` | 25 | **yalnızca** çift kanca |
| `SwingInitialBoost` | 5 | hiçbir yer (ölü alan) |
| `SwingHopForwardSpeed` / `UpwardSpeed` / `Duration` | 8 / 12 / 0.15 | yerden kanca |
| `DualSpringStiffness` / `Damping` | 25 / 5 | çift kanca |
| `DualSwingForceMultiplier` | 15 | çift kanca |
| `DualSwingCollisionRadius` | 0.4 | çift kanca |
| `NodeCoincidenceThreshold` | 0.5 | slingshot koşulu |
| `MovingNodeLaunchMultiplier` | 1 | hareketli node bırakışında aktarılan hız oranı |
| `MovingNodeInertia` | 0.6 | node ivmesinin salınıma geçen oranı (tek + çift kanca) |
| `MovingNodeMaxPivotAccel` | 60 | ataletin hesaplandığı node hızının değişim sınırı (b/sn²) |
| `MaxSlingshotSpeed` | 65 | |
| `SlingshotLaunchDuration` | 0.01 | bir fizik adımından kısa |
| `SlingshotGrappleLockout` | 0.25 | çıkıştan sonra tekrar kanca atılamayan süre |
| `SlingshotAnticipationDuration` / `Speed` | 0.1 / 10 | |
| `HitStopDuration` | 0.2 | fırlatma anı (dash ile ortak) |

Sahnedeki `NodeDetector`: `detectionRadius` 10, `minimumDotProduct` 0.5.

## Kod

- `Player/NodeDetector.cs`, `Player/TargetVisualizer.cs`, `Player/GrappleVisualManager.cs`, `Player/ArmController.cs`
- `Player/States/SwingingState.cs`, `DualSwingingState.cs`, `SlingshotState.cs`
- Kanca girişleri: `NormalState.CheckGroundGrappleInput` (yerde), `NormalState.ProcessGrappleInput` (havada), `SwingingState.CheckOppositeGrappleCast`, `WallSlidingState` (kayarken). Hepsi node'u `PlayerController.AttachAnchor` ile bağlar.
- `Player/Nodes/Rotation/`: `FullRotationNode`, `FullRotationTracker` (saf C#, testli), `SpeedBoostRotationEffect`, `RotationEventTrigger`
- Prefab'lar: `Assets/Prefab/New/Node/` (hareketli: `Node-Moving.prefab`)
- Oda 5'teki yavaş zaman tutorial'ı (`SwingTutorial`, ilk node): [`swing-tutorial.md`](swing-tutorial.md). `NodeDetector` kurallarını `PlayerController.TryCastGrapple` ile kullanır.

## Bilinen kenar durumlar ve GDD ile farklar

1. **Yerden yalnızca sağ tetik:** bilinçli tercih (GDD §5.1).
2. **Kancadan üç çıkış, üç farklı yerçekimi.** Tek kanca bırakışında yükselirken her zaman 3× (tuştan bağımsız). Slingshot çıkışı da 3× (16 Kasım 2026'dan beri tuştan bağımsız). Dual swing bırakışında sabit 1×. Tek kurala indirmeme kararı bilinçli (`feel-notlari.md`). Tam tablo: [`movement-core.md`](movement-core.md).
3. **Slingshot fırlatma eğrisi fiilen yok.** `SlingshotLaunchDuration` (0.01) fizik adımından (0.02) kısa olduğu için ease-out tek adımda bitiyor.
4. **Slingshot hakkı gerilme başında harcanıyor.** Gerilme sırasında vazgeçmek hakkı geri vermez.
5. **GDD "dönen node hız patlaması verir" diyor.** Kodda bu, 1.2 sn boyunca yalnızca tek kanca sarkaç hız sınırının yükselmesi + tek seferlik ×1.2 hız. Hız patlaması prefab'ı hiçbir odada yok; sahnede kullanılan tek dönen node kapı açıyor.
6. **`OncePerNode` ölünce sıfırlanmıyor.** Ölmeden önce bir kez tamamlanan node, respawn'dan sonra tekrar tetiklenmez. Kapı açan node için bu sorun olmayabilir; bkz. [`death-respawn.md`](death-respawn.md).
7. **Ölü kod:** `Player/Grappler.cs` hiçbir sahne ya da prefab'da kullanılmıyor (CLAUDE.md onu aktif sistemin parçası sayıyor). `SwingInitialBoost` okunmuyor.
8. **Hareketli node bir adım geriden izlenir** ve hareketli dönen node test edilmedi (17 Kasım 2026, kapsam dışı bırakıldı). İpin görsel olarak node'un gerisinde kalıp kalmadığı gamepad turunda bakılacak.
9. **Çift kanca çarpışması hâlâ daire.** Tek kanca 5 Kasım 2026'dan beri oyuncu kutusuyla arıyor (eğimde takılma düzeltildi); çift kanca `DualSwingCollisionRadius` (0.4) daireyle arıyor ve eğimde test edilmedi. Ayrıntı: [`../bozulmamali-davranislar.md`](../bozulmamali-davranislar.md).

## Testler

`FullRotationTrackerTests`, `SwingRulesTests`, `SlingshotRulesTests`, `MovingNodeRulesTests`. Kurallar: [`../bozulmamali-davranislar.md`](../bozulmamali-davranislar.md).
