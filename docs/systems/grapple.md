# Kanca ailesi: swing, dual swing, slingshot, dönen node

*GDD §2 (sütun 2), §5.1, §5.2 "Kanca ailesi". Son kod doğrulaması: 2026-10-05.*

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
- Anchor, node'un `transform.position`'ıdır. Node hareket etse bile anchor tutunma anındaki noktada kalır.

## Durumlar

### Yerden kanca (swing hop)

Yerdeyken **yalnızca sağ tetik** çalışır (`GroundedState.CheckGrappleInput`), sol tetik dalı yok. Tutununca `SwingingState(wasGrounded: true)` önce `SwingHopDuration` boyunca node tarafına doğru bir sıçrama yapar (`SwingHopForwardSpeed` yatay, `SwingHopUpwardSpeed` dikey, normal yerçekimi). Sıçrama bitince ip uzunluğu o anki mesafeye sabitlenir ve sarkaç başlar. Yerde sol kol görsel olarak hareket yönüne bakar, sağ kol sağ stick'e.

Bu asimetri GDD §12 madde 6'da açık soru; karar `TASKS.md`'de (9 Ekim).

### Tek kanca (`SwingingState`)

- İp boyu tutunma anında sabitlenir. Yerçekimi normal `Gravity` (−32); `SwingGravity` burada **kullanılmıyor**.
- Her fizik adımında hız ipe teğet olacak şekilde izdüşürülür, oyuncu tekrar ip boyuna oturtulur.
- Pompalama `SwingForceMultiplier` ile teğet hıza eklenir. `MaxSwingSpeed × ActiveSpeedMultiplier` üstüne pompalayarak çıkılamaz, ama yerçekimiyle bu sınır aşılabilir (sınır yalnızca girdinin eklenmesini engeller).
- Girişte dash hakkı ve taç rengi yenilenir.
- **Çarpışma:** `SwingCollisionRadius` yarıçaplı CircleCast. Çarpılan yüzey yataydan en fazla `MaxWallAngle` (45°) eğikse karakter yere iner (`GroundedState`). Daha dikse kanca kopar ve karakter havaya geçer.
- Swing sırasında dash ve duvara tutunma yok.
- **Bırakma:** tetik bırakılınca `AirborneState(isJumping: true, bypassJumpGravity: true)`. Bu modda yükselirken hep `JumpEndEarlyGravityMultiplier` (3×), düşerken `FallGravityMultiplier` (1.5×) uygulanır; apex hang yok. Yani swing'den yukarı fırlayış zıplama tuşundan bağımsız ve hızla söner.

### Çift kanca (`DualSwingingState`)

- Tek kancadayken diğer tetiğe basıp diğer stick'le nişan alınca girilir. Havadayken iki tetik aynı karede basılırsa önce sol, sonraki karede sağ işlenir.
- Fizik farklı: iki ip birer **tek yönlü yay**. Yalnızca gerildiğinde (`mesafe > ip boyu`) `DualSpringStiffness` ve `DualSpringDamping` ile çeker, gevşekken hiçbir şey yapmaz. Yerçekimi `SwingGravity` (25), yani tek kancadan daha hafif.
- Pompalama iki stick'in X ortalaması × `DualSwingForceMultiplier`. Hız sınırı yok.
- Bir tetik bırakılırsa diğer kolla tek kancaya döner (hop yapmadan). İkisi birden bırakılırsa `AirborneState(isJumping: false)`: sabit yerçekimi, değişken zıplama çarpanları yok.
- Çarpışma kuralı tek kancayla aynı (`DualSwingCollisionRadius`, `MaxWallAngle`).
- Girişte dash hakkı yenilenir ama taç rengi sıfırlanmaz. Tek kancadan geçildiği için pratikte zaten sıfırlanmış olur.

### Slingshot (`SlingshotState`)

- **Koşul:** iki anchor arası mesafe `< NodeCoincidenceThreshold` (0.5) **ve** `CanSlingshot`. Hak yoksa aynı node'a iki kolla tutunmak dual swing olur.
- Pratikte: aynı node'a önce bir tetikle, sonra diğer tetikle ve **diğer stick'le** nişan almak gerekir.
- **Girişte hak harcanır** (`UseSlingshot`, kollar "tükendi" rengine döner). Gerilme sırasında tetikler bırakılsa bile hak gider.
- **Gerilme (anticipation):** `SlingshotAnticipationDuration` boyunca node'un tersine `SlingshotAnticipationSpeed` hızla geri çekilir.
- **Fırlatma:** kamera hedefe atlar (`OnSlingshotLaunch`), ses çalar, `NotifyImpact` → hit-stop (`HitStopDuration`) + kamera sarsıntısı + titreşim. Sonra hız `SlingshotLaunchDuration` içinde ease-out ile `MaxSlingshotSpeed`'e çıkar. Yön, **girişteki konumdan node'a** doğru sabittir.
- Asset'te `SlingshotLaunchDuration` 0.01, fizik adımı 0.02: fırlatma tek adımda tam hıza ulaşır ve aynı adımda `AirborneState`'e geçer. Ease-out pratikte devre dışı.
- **Çıkış:** `AirborneState(isJumping: true, grappleLockout: SlingshotGrappleLockout)`. Normal zıplama kuralları geçerli. Zıplama tuşu basılı değilse yukarı fırlayışa 3× yerçekimi uygulanır, yani **dikey slingshot yüksekliği A'yı basılı tutmaya bağlı**. Yatay hız hava kontrolüyle söner: stick boşsa `AirDrag` 20 birim/sn², stick ileri basılıysa hedef 10'a `AirAcceleration` 40 ile iner.
- **Yenilenme:** yere inmek, slingshot yenileyici toplanabilir ([`collectibles.md`](collectibles.md)), launch pad ([`platforms.md`](platforms.md)) ve respawn. Kancaya tutunmak yenilemez.

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
| `SwingCollisionRadius` | 0.5 | tek kanca |
| `MaxWallAngle` | 45 | tek + çift kanca |
| `SwingGravity` | 25 | **yalnızca** çift kanca |
| `SwingInitialBoost` | 5 | hiçbir yer (ölü alan) |
| `SwingHopForwardSpeed` / `UpwardSpeed` / `Duration` | 8 / 12 / 0.15 | yerden kanca |
| `DualSpringStiffness` / `Damping` | 25 / 5 | çift kanca |
| `DualSwingForceMultiplier` | 15 | çift kanca |
| `DualSwingCollisionRadius` | 0.4 | çift kanca |
| `NodeCoincidenceThreshold` | 0.5 | slingshot koşulu |
| `MaxSlingshotSpeed` | 65 | |
| `SlingshotLaunchDuration` | 0.01 | bir fizik adımından kısa |
| `SlingshotGrappleLockout` | 0.25 | çıkıştan sonra tekrar kanca atılamayan süre |
| `SlingshotAnticipationDuration` / `Speed` | 0.1 / 10 | |
| `HitStopDuration` | 0.2 | fırlatma anı (dash ile ortak) |

Sahnedeki `NodeDetector`: `detectionRadius` 10, `minimumDotProduct` 0.5.

## Kod

- `Player/NodeDetector.cs`, `Player/TargetVisualizer.cs`, `Player/GrappleVisualManager.cs`, `Player/ArmController.cs`
- `Player/States/SwingingState.cs`, `DualSwingingState.cs`, `SlingshotState.cs`
- Kanca girişleri: `GroundedState.CheckGrappleInput`, `AirborneState.ProcessGrappleInput`, `SwingingState.CheckOppositeGrappleCast`
- `Player/Nodes/Rotation/`: `FullRotationNode`, `FullRotationTracker` (saf C#, testli), `SpeedBoostRotationEffect`, `RotationEventTrigger`
- Prefab'lar: `Assets/Prefab/New/Node/`

## Bilinen kenar durumlar ve GDD ile farklar

1. **Yerden yalnızca sağ tetik** (GDD §12.6, `TASKS.md` 9 Ekim).
2. **Kancadan üç çıkış, üç farklı yerçekimi.** Tek kanca bırakışında yükselirken her zaman 3× (tuştan bağımsız). Slingshot çıkışında A basılı değilse 3×, basılıysa 1× + apex hang (tuşa bağlı). Dual swing bırakışında sabit 1×. Bilinçli mi, değil mi, oynayarak karar verilmeli; `feel-notlari.md`'ye yazılmaya değer. Tam tablo: [`movement-core.md`](movement-core.md).
3. **Slingshot fırlatma eğrisi fiilen yok.** `SlingshotLaunchDuration` (0.01) fizik adımından (0.02) kısa olduğu için ease-out tek adımda bitiyor.
4. **Slingshot hakkı gerilme başında harcanıyor.** Gerilme sırasında vazgeçmek hakkı geri vermez.
5. **GDD "dönen node hız patlaması verir" diyor.** Kodda bu, 1.2 sn boyunca yalnızca tek kanca sarkaç hız sınırının yükselmesi + tek seferlik ×1.2 hız. Hız patlaması prefab'ı hiçbir odada yok; sahnede kullanılan tek dönen node kapı açıyor.
6. **`OncePerNode` ölünce sıfırlanmıyor.** Ölmeden önce bir kez tamamlanan node, respawn'dan sonra tekrar tetiklenmez. Kapı açan node için bu sorun olmayabilir; bkz. [`death-respawn.md`](death-respawn.md).
7. **Ölü kod:** `Player/Grappler.cs` hiçbir sahne ya da prefab'da kullanılmıyor (CLAUDE.md onu aktif sistemin parçası sayıyor). `SwingInitialBoost` okunmuyor.
8. **Hareketli node'lar anchor'ı taşımaz.** Anchor bir `Vector2` kopyası. Node hareket ederse ip eski noktada kalır.

## Testler

`FullRotationTrackerTests`. Swing, dual swing ve slingshot için test yok.
