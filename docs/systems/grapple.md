# Kanca ailesi: swing, dual swing, slingshot, dönen node, spiral node

*GDD §2 (sütun 2), §5.1, §5.2 "Kanca ailesi". Son kod doğrulaması: 2026-11-20 (uzayan kollar).*

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
- Tutunma anlıktır: mermi ya da uçuş süresi yok. Kol çizimi (`ArmVisual`: turuncu, koyu dış çizgili kalın kol; omuzlar gövdede ±0,3; havada boşta yaylı ve gevşek, yana açık sarkan açık eldiven; yerdeyken kol zeminde sürünür ve hızın gerisinde kalır (sol kol yerdeyken sol stiği izlemez, sağ kol sağ stikle nişan alabilir); tutununca node'a 0,1 sn'de uzanıp yumruğa dönen eldiven) yalnızca görsel; ip çizimi yok. Eldiven sprite'ları `Assets/Sprites/Arm/Glove_Open.png`, `Glove_Closed.png` ve `Glove_WallGrip.png` (duvarda tutma anı, parmaklar yandan duvara kıvrılır) (26×26, 24 PPU, pivot kol ucu). `TargetVisualizer` her iki stick için o an seçilecek node'u nişangâhla gösterir.
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

- **İzleme:** `PlayerController.FixedUpdate`, durumdan önce `RefreshMovingAnchors()` ile anchor'ı node'un konumuna çeker (node bir `Rigidbody2D` altındaysa konum gövdenin fizik pozundan okunur: interpolation açık Transform adımlar arasında düzgün değişmez, atalet bu gürültüyü titremeye çevirir); o adımdaki kayma `AnchorDelta(kol)`, hız `AnchorVelocity(kol)`. Node `PlatformController` (`Rigidbody2D.MovePosition`) ile hareket ettiği için anchor node'u **bir fizik adımı geriden** izler. Node pasifleşir ya da yok olursa anchor son konumda kalır, kanca kopmaz.
- **Ham atama sabittir:** `LeftAnchor`/`RightAnchor`'a doğrudan nokta atamak node bağlantısını siler (testler ve anchor'ı silen yerler bunu kullanır).
- **Atalet (17 Kasım 2026, playtest: "node hareketi swing'e hiç etki etmiyor"):** node'un hızlanması, durması ve dönmesi salınıma geçer. Her adımda node hızı `MovingNodeMaxPivotAccel` (60 b/sn²) sınırıyla yumuşatılır; bu yumuşatılmış hızdaki değişim × `MovingNodeInertia` (0.6; 0 = yalnızca taşıma, 1 = tam fizik) node'a göre hızdan çıkarılır. Radyal kısmı ip kısıtı atar. Çift kancada iki node'un ortalaması kullanılır; slingshot'ta atalet yok. Sınır, `LinearPingPong`'un tek adımlık dönüşünü ~0.17 sn'ye yayar; `SinePingPongStrategy`'de ivme zaten süreklidir ve sınıra takılmaz.
- **Yakalama ve durum geçişleri:** kanca durumları birbirine **dünyaya göre** hız devreder; durum içinde node'a göre hız = dünya − node. `AttachAnchor` node hızını `IMovingSurface.DeltaPosition`'dan anında aldığı için hareket eden node'u yakalayan oyuncu node'un hızını almaz, ip onu çeker.
- **Taşıma:** durumların hızı node'a göre kalır; node kayması ayrıca eklenir. Tek kancada sarkaç kısıtından önce konuma eklenir (çarpışma taraması dahil), çift kanca ve slingshot'ta iki anchor'ın ortalaması hız olarak `Move`'a verilir. Yerden kanca hop'u sırasında taşıma yok, yalnızca anchor güncellenir.
- **Fırlatma:** tek kanca bırakışı, çift kancada iki tetiği birden bırakma ve slingshot çıkışında (ortalama) node hızı × `MovingNodeLaunchMultiplier` `AirEntry.CarriedVelocity` ile aktarılır: Y normal hıza eklenir, X taşınır ve hava sürtünmesinden etkilenmez (duvardan ayrılma kuralı, [`platforms.md`](platforms.md)); duvar kuralından farkı, taşınan X `MovingNodeCarryDecayTime` (0.6 sn) içinde doğrusal olarak sıfıra iner (`AirEntry.CarriedDecayTime`). Çarpışmayla kopma ve çift kancadan tek kancaya düşüş aktarmaz.
- **Prefab:** `Assets/Prefab/New/Node/Node-Moving.prefab`: kök `PlatformController` + kinematik `Rigidbody2D` + `SinePingPongStrategy` (uçlarda yavaşlayan gidip gelme, period 4), child `Node` (node katmanı, oda node'larının kopyası). `startPoint`/`endPoint` **dünya koordinatı**: her yerleştirmede ayarlanmalı, yoksa node (0,0)–(6,0) arasına ışınlanır. Başka strateji (dairesel vb.) aynı şekilde takılabilir.

### Zamanlı çiçek node'u (`FlowerCycle`)

*Görsel ve zamanlama: `Assets/Scripts/New-Scripts/Flower/` (`FlowerBloom`, `FlowerCycle`, `FlowerCycleMath`), shader'lar `Assets/Shader/Flower*.shader`, prefab `Assets/Prefab/New/Node/Node-Flower.prefab`.*

- **Döngü:** kapalı 1.8 → açılış 0.5 → açık 2.2 → uyarı 0.7 → snap 0.15 sn (`FlowerCycleTimings.Default`, başlangıç değerleri; hissi gamepad ile ayarlanır). Çiçekleri senkronsuzlaştırmak için `phaseOffset`.
- **Tutulabilirlik:** açılışın `grabAt` (0.3) sn sonrasından snap başladıktan `coyote` (0.1) sn sonrasına kadar. Uyarı fazında hâlâ tutulur. Prefab'ın `Node` çocuğu (Grapple katmanı, `CircleCollider2D`) yalnızca tutulabilirken açıktır: `NodeDetector` kapalı çiçeği görmez, hedef işareti de çıkmaz.
- **Kapanırken tutuluyorsa:** `PlayerController.ReleaseUngrabbableNodes` (her fizik adımı, `RefreshMovingAnchors`'tan sonra) tutulan node'un `IGrabbableNode.IsGrabbable`'ına bakar. Tutulamaz olduysa durumun `INodeReleaseHandler.ReleaseByNode`'unu çağırır:
  - `SwingingState`: tetik bırakılmış gibi tek kanca bırakışı (`AirEntry.SwingRelease`, node hızı taşınır), üstüne çiçekten dışa `ReleaseImpulse` (6 b/sn) hız. Spiral çıkış boost'u yok.
  - `DualSwingingState`: o kol bırakılır, diğer kolla tek kancaya düşülür (hop yok), itme hıza eklenir.
  - `SlingshotState` ve diğer durumlar bu kuralı **uygulamaz**: gerilme 0.1 sn sürer, anchor'lar sabitlenmiştir.
- **Test:** `FlowerNodeRulesTests` (PlayMode): kapalı tutulmaz, açık tutulur, kapanınca kanca kopar, oyuncu dışa itilir. Bu davranış `bozulmamali-davranislar.md`'ye henüz kural olarak eklenmedi.
- **Test odası:** `RoomNew-FlowerTest` (roomId 100, NewScene'de x −640, spiral test odasının yanında; `RoomManager.rooms`'a kayıtlı, komşusuz). Spiral odasının zemin/duvar/tavan geometrisi (sütun ve havadaki platformlar silindi), hepsi zeminden 8 birim yukarıda, yerden tutunulur. Sırayla: `Cicek-Tekli` (açılıp kapanma ve kapanırken bırakılma), `Cicek-Cift-A`/`B` (ters fazda; çift kanca, biri kapanınca tek kancaya düşüş), `Node-Normal` (karşılaştırma), `Cicek-Zincir-1..4` (10 birim arayla, fazlar 1.3 sn kayık: sırayla açılan yol), `Cicek-Slingshot` (aynı çiçeğe iki kol: kapanırken slingshot kuralı dışında kalır). Oraya başlamak için `LevelBootstrapper.startingRoom` = `RoomNew-FlowerTest`.
- **Boyut:** `Node-Flower` prefab kökünün ölçeği 4 (çiçek çapı ~9 birim; oyuncu 1×1, diğer node görselleri 20 birim). Çiçeği büyütüp küçültmek için yalnızca kök ölçeği değiştirilir (geometri, halo, titreme ve collider birlikte ölçeklenir); `Node` collider'ının yarıçapı 0.5 / ölçek olarak tutulur ki dünyada 0.5 kalsın. Zincir ve çift düzeninde komşu çiçekler 10 birim arayla durur: ölçek 4'ün üstüne çıkarsa çakışırlar.
- **Derinlik:** yapraklar derinlik yazar (ZWrite), kameraya doğru çıkan yaprak z = 0'daki oyuncu sprite'larının derinlik testini geçerdi ve oyuncu çiçeğin arkasında kalırdı. `FlowerBloom.depthBehind` (1.25 × petal boyu) çiçeğin tamamını kameradan uzağa iter; en öne çıkan yaprak (kapalı hal, ~1 × petal boyu) bile oyuncunun arkasında kalır. Anchor node'un Vector2 konumunu kullandığı için z etkilemez. Oyuncu `Player` sorting layer'ında, çiçek `Default`'ta.
- **Ayar:** `FlowerCycle.releaseImpulse` (6) çiçeğin kendi alanı; PlayerStats'ta değil. Çiçek görseli ve yayları (açılış 180/14.8, snap 900/48) `FlowerBloom`'da.
- **Görünüm (K4 Kâğıt Pasiflora, 2026-11-22):** el işi kâğıt kesmesi dili; karar gerekçesi `docs/ui-research/flower-node/oneri.md`, uygulama `docs/flower-k4-plani.md`. Palet P3 Porselen Kâğıt. Oyun kuralları (döngü, tutulabilirlik, itme) aynı kaldı, yalnızca görünüm değişti.
  - **Yapraklar:** 5 yaprak, tek katman; boy/en/uç yatıklığı her yaprağın `Seed`'inden hafifçe farklı (`FlowerCutMath`, `FlowerBloom.handCut`). `Flower/Petal`: düz iki ton, orta çizgiden katlanmış iki yüz, kapalıda koyu `#4B4C58` (gökte 4.0:1). Bükme/rulo `FlowerBend.cginc`'de, yaprak, iplik tacı ve gölgede ortak; boya bükmeden bağımsız.
  - **Kesim gölgesi:** `Flower/PetalShadow` ve `Flower/CoronaShadow` parçanın **ikinci materyali** (aynı mesh'e ikinci kez çizilir, kuyruk AlphaTest+1). Ofset çiçek boyuyla orantılı (`_CutShadowRel` 0.042, alt sınır 2 px), uyarıda ×1.6; eğimli parçada kendi üstüne düşmesin diye derinlik eğime göre geri itilir (`FlowerShadow.cginc`).
  - **İplik tacı:** halka başına tek mesh (`FlowerCoronaMesh`, 10 şerit; dış halka soluk taban + koyu uç, iç halka kobalt), `Flower/Corona`; açılış/uyarı duruşu saf `FlowerCoronaMath` (yapraklardan sonra açılır, uyarıda büzülür). Halka mesh'leri prefab'a gömülmez, `OnEnable`'da ortak önbellekten atanır (mesh'siz görünürse `Rebuild` değil `OnEnable` bakılır).
  - **Merkez kule:** `Flower/Center`: koyu disk, 3 beyaz stigma, diskin dışında 5 krem organ ucu; kapalıda düz tomurcuk diski. Kule iplik şeritlerinin üstünde durur (`towerHeight`), tepesi hep yukarı bakar (`orientationDeg` iptali).
  - **Uyarı renge güvenmez:** titreme + iplik tacı büzülmesi + kesim gölgesi büyümesi (kesikli halka denendi, geliştirici kararıyla kaldırıldı). Yapraklarda sıcak tint ve parlaklık titreşimi yok (turuncu zeminde kayboluyor, renk körlüğünde asit yeşiline yaklaşıyor). Uyarının yeterince okunup okunmadığı gamepad testinde bakılacak; yetmezse alternatif araştırması `docs/ui-research/flower-node/research/E-animasyon.md`'de.
  - **Ölçüler:** mockup R = 4.5 birim = 1 nesne birimi; `FlowerBloom` varsayılanları mockup'a göre (yaprak boyu 0.95, eni 0.48, iplik halkaları, kule). Kök ölçek 4'te kaldı.
  - **Bilinen sınırlar:** zincirde (10 birim arayla) iki çiçek aynı anda uyarıdaysa halkalar ölçek 4'te hafif çakışır (`phaseOffset` seyrekleştirir); gölge shader'ı opak (mockup'ta yarı saydam); kapalı tomurcuk koyu bir damla kümesi gibi okunuyor, "çiçek gibi mi" gamepad/göz testine kaldı.

### Dönen node (`FullRotationNode`)

- Yalnızca **tek kanca** sırasında sayılır. Tutunulan noktada `FullRotationNode` varsa bir `FullRotationTracker` oluşur. Tracker ardışık yönler arasındaki işaretli açıyı biriktirir, iki yön de sayılır.
- `targetDegrees` (varsayılan 360) dolunca node'un efekti tetiklenir. Kısıt modları: `OncePerConnection` (her tutunuşta bir kez) ve `OncePerNode` (node başına bir kez; ölünce sıfırlanmıyor, `IResettable` değil).
- Bağlantı kopunca ilerleme halkası `lerpBackSpeed` ile geri sıfırlanır.
- Efektler (`IFullRotationEffect`):
  - `SpeedBoostRotationEffect`: hızı bir kez ×1.2 yapar ve `boostDuration` (1.2 sn) boyunca `ActiveSpeedMultiplier`'ı 1.8 yapar. Bu çarpan yalnızca tek kancadaki `MaxSwingSpeed` sınırını yükseltir. Prefab'ı `Node-FullRotation` hiçbir odada kullanılmıyor.
  - `RotationEventTrigger`: UnityEvent. `Node-EventTrigger` prefab'ı oda 17'de bir kapının `Open`'ını çağırıyor.

### Spiral node (`SpiralNode`)

Tasarım: [`../spiral-node-tasarimi.md`](../spiral-node-tasarimi.md). Kurallar SP1–SP8. Çıkış hızı: [`../spiral-cikis-hizi.md`](../spiral-cikis-hizi.md).

- **Kontroller:** normal tek kanca gibi tutunulur. Stick spinin teğeti yönünde iterse hızlanır, tersine iterse yavaşlar (±`SpiralInputInfluence`, 0.4), ama dönüşü asla durdurmaz ya da çevirmez.
- **Bırakış (SP5, SP8):** tetik bırakılınca, motor çalışmışsa, node etkinse ve oyuncu spin yönünde en az `SpiralExitMinSpeedRatio` (0.5) × hedef hızla gidiyorsa: teğete `SpiralExitBoost` (6) eklenir ve `AirEntry.SpiralRelease` ile havaya geçilir; `SpiralExitHoldTime` (0.45 sn) boyunca `AirDrag`/`AirOverspeedReduce` uygulanmaz (stick'le hızlanma ve ters basma çalışır), yerçekimi H2 ile aynı. Aynı anda iz açık kalır (`NormalState.IsInAirBrakeHold`), spiral `_Glow` darbesi, `SpiralRelease` titreşimi (0.12 sn) ve `spiralReleaseCue` (boş). Kapılar tutmazsa (hop, spinin tersi, yavaş) normal `SwingRelease`. Kopma, çarpışma, çift kanca ve respawn bu yoldan geçmez.
- **Motor:** yalnızca tek kancada, hop bittikten sonra. Her adımda yerçekimi aynen eklenir, sonra `SwingingState` pompalama ve `MaxSwingSpeed` sınırı yerine node'a göre teğet hızı hedefe (`targetSpeed`, 32, × stick × `ActiveSpeedMultiplier`) yaklaştırır: spin yönünde altındaysa `SpiralDriveAccel` (60), tersine gidiyorsa `SpiralOpposeAccel` (45), üstündeyse fazla hız korunur (`SpiralOverspeedDecay` 0). İki ivme de |`Gravity`| (32) üstünde olduğu için spiral her ip boyunda tam tur attırır (`PlayerStatsSanityTests` korur). İp boyu tutunma anındaki mesafedir.
- **Tersine tutunma:** oyuncu yavaşlar, durur ve spiral yönüne döner (−25 b/sn'den ~0.6 sn). Anlık değildir.
- **Takılma:** tutunma çarpışması yalnızca radyal düzeltmeyi tarar; spiral oyuncuyu bir duvara bastırınca teğet hareket sessizce durur. Bu yüzden spiralde her adım gerçekleşen teğet hız komutla karşılaştırılır; art arda `SpiralBlockedSteps` (3) adım `SpiralBlockedRatio` (0.25) altında kalırsa kanca kopar (SP4).
- **Çift kanca / slingshot:** motor çalışmaz. Spiral + normal node normal çift kancadır, aynı spirale iki kol normal slingshot'tır. Kol spiralde kalırsa görsel soluklaşır.
- **Dönen node ile birlikte kullanılmaz:** spiral varken `FullRotationTracker` bağlanmaz; `SpiralNode.OnValidate` aynı node'da `FullRotationNode` görünce uyarır.
- **Ters dönen spiral** (`reverseEveryDegrees` > 0, prefab'da 360): tutunurken spin yönünde biriken net açı (`SpiralReverseCounter`, geriye dönüş sayılmaz) eşiğe ulaşınca yön değişir, sayaç sıfırlanır ve tutundukça tekrarlar. Bırakınca, respawn'da ya da çift kancaya geçişte ilk yönüne döner. `Reversed` olayı ses, titreşim ve görseli tetikler.
- **Sprite yönü:** motor çalışırken spinden gelir (CCW sağ, CW sol); stick ve hız dalı atlanır (`PlayerController.SpiralFacingSign`).
- **Ses / titreşim:** tutununca döngü sesi (`spiralLoopCue`, sabit perde) ve `SpiralOrbit` sürekli titreşim; tersine tutunmada ve çevrilmede tek seferlik. Cue'lar şimdilik boş. Döngü sesinin durması için sahnedeki `PlayerAudioController.sfxStopChannel` atanmalı.
- **Görsel:** `UI/SpiralNode` shader'ı (merkez nokta + tek Arşimet çizgisi, 3 tur). Ters dönen spiral (`Node-SpiralReversing`) ayrı görsel kullanır: `UI/SpiralNodeReversing` / `Mat_SpiralNodeReversing`, A2 "Vasarely Kanatları" (5 burulmuş kanat, #04586c / #f1e6c7). Orada `_Curl` = 1 − `ReverseProgress`: tutundukça kanatlar düzleşir, çevrilmede düz kanatlardır, sonra `rewindDuration` (0.4 sn) içinde ters yöne sarılır; bırakınca da aynı hızla sarılır. Açı script'ten sürülür, hit-stop ve pause'da durur. Bağlı değilken `targetSpeed / idleReferenceRadius` (3) rad/sn döner.
- **Test odası:** `RoomNew-SpiralTest` (roomId 97, NewScene'de x −480, slingshot test odasının yanında): kaldırıcı, duvar kopması, fırlatıcı, zıt yönlü zincir, tersine tutunma, ters dönen spiral; karşılaştırma için fırlatıcı ile zincir arasında bir normal node (x −2). Hepsi yerden tutunulur (zeminden 8 birim). Oraya başlamak için `LevelBootstrapper.startingRoom`.
- **Level tasarımı:** dönüş diski ip boyu kadar yarıçaplıdır; tutunulabilecek en uzak mesafe `NodeDetector.detectionRadius` (NewScene'de 10, varsayılan 15) olduğundan yaklaşık 10.5 birimlik (varsayılanla 15.5) bir daire boş kalmalı, ya da duvarın kancayı koparması bilinçli olmalı.

## Tunable'lar (`PlayerStats.asset`)

| Alan | Değer | Okuyan |
|---|---|---|
| `SwingForceMultiplier` | 28 | tek kanca |
| `MaxSwingSpeed` | 28 | tek kanca (spiralde kullanılmaz) |
| `SwingCollisionRadius` | 0.5 | hiçbir yer (ölü alan; tek kanca artık oyuncu kutusunu kullanıyor) |
| `MaxWallAngle` | 45 | tek + çift kanca |
| `SwingGravity` | 25 | **yalnızca** çift kanca |
| `SwingInitialBoost` | 5 | hiçbir yer (ölü alan) |
| `SwingHopForwardSpeed` / `UpwardSpeed` / `Duration` | 8 / 12 / 0.15 | yerden kanca |
| `DualSpringStiffness` / `Damping` | 25 / 5 | çift kanca |
| `DualSwingForceMultiplier` | 20 | çift kanca |
| `DualSwingCollisionRadius` | 0.4 | çift kanca |
| `NodeCoincidenceThreshold` | 0.5 | slingshot koşulu |
| `MovingNodeLaunchMultiplier` | 1 | hareketli node bırakışında aktarılan hız oranı |
| `MovingNodeCarryDecayTime` | 0.6 | bırakışta taşınan yatay hızın sıfıra indiği süre (sn); 0 = sönmez |
| `MovingNodeInertia` | 0.6 | node ivmesinin salınıma geçen oranı (tek + çift kanca) |
| `MovingNodeMaxPivotAccel` | 60 | ataletin hesaplandığı node hızının değişim sınırı (b/sn²) |
| `MaxSlingshotSpeed` | 65 | |
| `SlingshotLaunchDuration` | 0.01 | bir fizik adımından kısa |
| `SlingshotGrappleLockout` | 0.25 | çıkıştan sonra tekrar kanca atılamayan süre |
| `SlingshotAnticipationDuration` / `Speed` | 0.1 / 10 | |
| `HitStopDuration` | 0.2 | fırlatma anı (dash ile ortak) |
| `SpiralDriveAccel` / `SpiralOpposeAccel` | 60 / 45 | spiral motoru; ikisi de \|`Gravity`\|'den büyük olmalı |
| `SpiralInputInfluence` | 0.4 | stick'in hedef hızı değiştirme oranı (< 1) |
| `SpiralOverspeedDecay` | 0 | hedefin üstündeyken yavaşlama; 0 = fazla hız korunur |
| `SpiralExitBoost` / `SpiralExitHoldTime` / `SpiralExitMinSpeedRatio` | 6 / 0.45 / 0.5 | spiral çıkışı (SP8; 2026-11-20 gamepad: 4 / 0.3 yavaş kaldı): teğet itme, frensiz süre, eşik |
| `SpiralBlockedRatio` / `SpiralBlockedSteps` | 0.25 / 3 | takılınca kopma |

`SpiralNode` alanları: `direction`, `targetSpeed` (32), `reverseEveryDegrees` (0; ters dönen prefab'da 360), `idleReferenceRadius` (3), `visual`.

Sahnedeki `NodeDetector`: `detectionRadius` 10, `minimumDotProduct` 0.5.

## Kod

- `Player/NodeDetector.cs`, `Player/TargetVisualizer.cs`, `Player/ArmController.cs` (omuz + `AimDirection`), `Player/Visual/ArmVisual.cs`, `Player/Visual/ArmShapeMath.cs`, `Player/Visual/ArmVisualSettingsSO.cs` (ayarlar: `Player/SO/ArmVisualSettings.asset`)
- `Player/States/SwingingState.cs`, `DualSwingingState.cs`, `SlingshotState.cs`
- Kanca girişleri: `NormalState.CheckGroundGrappleInput` (yerde), `NormalState.ProcessGrappleInput` (havada), `SwingingState.CheckOppositeGrappleCast`, `WallSlidingState` (kayarken). Hepsi node'u `PlayerController.AttachAnchor` ile bağlar.
- `Player/Nodes/Spiral/`: `SpiralNode`, `SpiralDriveMath` ve `SpiralReverseCounter` (saf C#, testli); motor `SwingingState.DriveSpiral`, takılma `IsSpiralBlocked`. Shader `Assets/Shader/SpiralNode.shader`, prefab'lar `Node-Spiral`, `Node-SpiralReversing`.
- `Player/Nodes/Rotation/`: `FullRotationNode`, `FullRotationTracker` (saf C#, testli), `SpeedBoostRotationEffect`, `RotationEventTrigger`
- `Player/Nodes/IGrabbableNode.cs`, `Player/States/INodeReleaseHandler.cs` (zamanlı node bırakışı), `PlayerController.ReleaseUngrabbableNodes`; çiçek: `Flower/`
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
8. **Hareketli node bir adım geriden izlenir** ve hareketli dönen node test edilmedi (17 Kasım 2026, kapsam dışı bırakıldı). Kolun (`ArmVisual`) elinin node'un gerisinde kalıp kalmadığı gamepad turunda bakılacak.
9. **Çift kanca çarpışması hâlâ daire.** Tek kanca 5 Kasım 2026'dan beri oyuncu kutusuyla arıyor (eğimde takılma düzeltildi); çift kanca `DualSwingCollisionRadius` (0.4) daireyle arıyor ve eğimde test edilmedi. Ayrıntı: [`../bozulmamali-davranislar.md`](../bozulmamali-davranislar.md).

## Testler

`ArmShapeMathTests`, `FullRotationTrackerTests`, `SpiralDriveMathTests`, `SpiralReverseCounterTests`, `SwingRulesTests`, `SlingshotRulesTests`, `MovingNodeRulesTests`, `SpiralNodeRulesTests`. Kurallar: [`../bozulmamali-davranislar.md`](../bozulmamali-davranislar.md).
