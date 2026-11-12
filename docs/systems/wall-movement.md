# Duvar: tırmanma, kenardan çıkma, kayma, duvar zıplaması

*GDD §5.1, §5.2 "Duvar". Son kod doğrulaması: 2026-11-12 (stamina halkası).*

## Tasarım niyeti

Celeste kökenli "oyuncu dostu" katman. Duvar bir kurtarma alanı: yakalanırsın, biraz tırmanırsın, kayarsın, oradan zıplar, dash atar ya da kanca fırlatırsın. Sınırlı dayanıklılık (~6 sn) ve sınırlı kayma (~2 sn), duvarın kalıcı bir sığınak olmasını engeller.

## İki ayrı durum

| | Tırmanma (`WallClimbingState`) | Kayma (`WallSlidingState`) |
|---|---|---|
| Nasıl girilir | Duvara değerken **o taraftaki bumper**'ı basılı tut (yerden ya da havadan) | Havada **düşerken**, duvara değerken sol stick'i **duvara doğru** it |
| Ne kadar | `MaxWallStamina` (6 sn), yere inene kadar toplam | `MaxWallSlideTime` (2 sn), her yenilemeye kadar toplam (aşağıya bak) |
| Hareket | Sol stick Y ile yukarı/aşağı, `ClimbSpeed` | Aşağı doğru, hız `WallSlideMaxSpeed`'e `WallSlideFriction` ile yaklaşır |
| Zıplama | **Dikey**: `(0, ClimbVerticalJumpVelocity)` | **Dışarı**: `(−duvarYönü × WallSlideJumpForce.x, WallSlideJumpForce.y)` |
| Dash | Var | Var |
| Kanca | **Yok** | Var (iki tetik de, kendi stick'iyle) |
| Bırakınca | Bumper bırakılırsa hız sıfırlanıp düşer, 0.2 sn tekrar tutunamaz | Stick duvardan çekilirse düşer, 0.2 sn tutunma kilidi + coyote penceresi |
| Titreşim | `WallClimbLeft/Right` (sürekli) | `WallSlideLeft/Right` (sürekli) |

Kayarken bumper'a basılırsa tırmanmaya geçilir (`CanWallClimb` ise).

## Tırmanma ayrıntıları

- **Girişte duvara yapışma:** `WallSnapRaycastDistance` mesafesine kadar duvara doğru ışın atılır ve karakter `WallSnapSafetyOffset` boşlukla duvara oturtulur. Dikey hız sıfırlanır.
- **Dayanıklılık** hareket edip etmemekten bağımsız, her saniye 1 azalır (`ConsumeWallStamina(Time.deltaTime)`). Göstergesi **kırılan stamina halkası**dır (`HudStaminaRing`, aşağıya bak); tırmanmaya girince açılır.

### Kırılan stamina halkası (`HudStaminaRing`)

- Karakterin (collider merkezinin) etrafında 6 sarı boyalı beton parça. Her `MaxWallStamina / 6` kaybında **son parça** kopar (6 sn'de saniyede bir): dışarı ve yukarı fırlar, döner, düşer, 0,7 sn'de söner. Sağlam parça sayısı `StaminaRingMath.IntactPieces` (EditMode testli).
- Enerji `StaminaWarningThreshold` (1,5 sn) altına inince kalan parçalar sarı/kırmızı yanıp söner. Uyarı **görsel**; titreşim profili yok.
- Yere inince / launch pad / respawn `RefillWallStamina` → parçalar yaylanarak geri gelir, 0,45 sn sonra halka çekilir.
- Overlay canvas'ta (CRT dışında, keskin), `UiPixelScale` ile tam sayı sanat pikseli; konum her kare `WorldToScreenPoint` + piksele yuvarlama. Eski World Space `StaminaCanvas` `PlayerUIController.Awake`'te kapatılır (sahnede duruyor, görünmez). Unscaled zaman.
- Boyut ve hareket değerleri `HudStaminaRing` Inspector'ında (dış yarıçap 22, kalınlık 5 sanat pikseli, fırlama 70/50, yerçekimi 300, dönüş 240°/sn): başlangıç önerisi, gamepad ile ayarlanır.
- **Dayanıklılık yalnızca şunlarla dolar:** yere inmek, launch pad, respawn. Duvar zıplaması doldurmaz; 6 saniye bir yere değişten diğerine kadar toplam bütçe.
- **Bitince:** `CanWallClimb = false`, karakter hızı sıfırlanıp düşer ve 0.5 sn tutunamaz. Yere inene kadar tekrar tırmanılamaz, ama kayılabilir.
- Tırmanmaya girmek kayma süresini yeniler (`ResetWallSlideTime`).
- **Hareketli duvar:** tutunulan duvar `IMovingSurface` ise karakter onunla taşınır (`KinematicPhysicsHandler.ClingingWallDirection`). Zıplarken `JumpBoostMultiplier > 0` ise duvarın hızı zıplamaya eklenir. Bkz. [`platforms.md`](platforms.md).

## Kenardan çıkma (`LedgeClimbState`)

- Tırmanırken her karede `PlayerController.CheckLedge` çalışır. Bel hizasındaki ışın duvara çarpıp baş hizasındaki ışın boşa gidiyorsa köşe adayı var demektir. Sonra hedef noktaya yukarıdan aşağı ışın atılır, üstte zemin olup olmadığı ve karakterin oraya sığıp sığmadığı kontrol edilir.
- **Tehlikeye tırmanılmaz:** köşenin üstü ölüm katmanındaysa ya da hedef kutuda ölüm katmanı varsa köşe yok sayılır.
- Köşe bulunduğunda yukarı tırmanma kilitlenir. Sol stick **yukarı (`y > 0.5`)** `LedgeClimbHoldTime` (0.1 sn) basılı tutulursa çıkış başlar. Zaten yukarı tırmanan oyuncu için bu otomatik gibi hissedilir.
- Çıkış `LedgeClimbDuration` (0.2 sn) sürer: önce %40 dikey, sonra %60 yatay, ikisi de ease-in-out. Konum kinematik olarak `Update`'te yazılır. Sonunda karakter `GroundedState`'e geçer, yani bütün kaynaklar dolar.
- Gizmo hata ayıklaması: Play Mode'da Scene view'de ışınlar ve hedef kutu çizilir (`PlayerController.OnDrawGizmos`).

## Kayma ayrıntıları

- Giriş koşulu `AirborneState.ProcessWallInteraction`'da: `vy < 0`, kayma süresi kalmış, stick duvara doğru `> 0.1`.
- **Kayma süresi yalnızca şunlarla yenilenir:** yere inmek, tırmanmaya girmek, **kayarken zıplamak** ve launch pad. Kayarken zıplamak süreyi yenilediği için iki duvar arasında sınırsız duvar zıplaması zinciri mümkün.
- Süre bitince karakter mevcut hızıyla düşer (`isJumping: false`). Stick'i bırakıp ayrılınca `CoyoteTimeDuration` boyunca zıplama hâlâ çalışır, ama bu normal **dikey** zıplamadır, duvar zıplaması değil.
- **Duvar zıplaması:** `isJumping: true` (değişken yükseklik geçerli) ve `WallJumpInputLockoutTime` (0.1 sn) boyunca yatay girdi yok sayılır. Böylece oyuncu stick'i hâlâ duvara doğru tutuyorsa hemen geri yapışmaz.

## Tunable'lar (`PlayerStats.asset`)

| Alan | Değer | Not |
|---|---|---|
| `MaxWallStamina` | 6 | `PlayerStatsSanityTests` 5–7 bandı |
| `StaminaWarningThreshold` | 1.5 | halkada parçaların yanıp sönmeye başladığı kalan süre |
| `ClimbSpeed` | 8 | |
| `ClimbVerticalJumpVelocity` | 14 | tırmanırken zıplama |
| `WallJumpForce` | (11, 14) | **hiçbir yerde okunmuyor** |
| `WallSnapRaycastDistance` / `WallSnapSafetyOffset` | 2 / 0.02 | |
| `LedgeClimbHoldTime` / `Duration` / `SafetyOffset` | 0.1 / 0.2 / 0.05 | |
| `MaxWallSlideTime` | 2 | `PlayerStatsSanityTests` 1.5–2.5 bandı |
| `WallSlideMaxSpeed` / `WallSlideFriction` | 3 / 10 | |
| `WallSlideJumpForce` | (11, 11) | kayarken duvar zıplaması |
| `WallJumpInputLockoutTime` | 0.1 | |

Sabit kodlu değerler: tutunma kilitleri 0.2 sn (bırakma) ve 0.5 sn (dayanıklılık bitti), köşe tespitinin ışın ofsetleri (`CheckLedge` içinde 0.3, 0.5, 0.8, 0.05).

## Kod

- `Player/States/WallClimbingState.cs`, `WallSlidingState.cs`, `LedgeClimbState.cs`
- `Player/PlayerController.cs`: `CheckLedge`, dayanıklılık ve kayma süresi kaynakları
- `Player/KinematicPhysicsHandler.cs`: `IsTouchingLeftWall/RightWall`, `ClingingWallDirection`
- `Player/UI/PlayerUIController.cs`: halkaya köprü; `UI/Hud/HudStaminaRing.cs` (çizim, hareket), `UI/Hud/StaminaRingMath.cs` (saf mantık)
- `Player/PlayerVibrationSettingsSO.cs`: sürekli duvar titreşimleri

## Bilinen kenar durumlar ve GDD ile farklar

1. **Dayanıklılık uyarısı yalnızca görsel.** GDD §5.2 "azalırken uyarı verir": halka parçaları kopar ve son 1,5 sn yanıp söner. Titreşim uyarısı yok. `WallClimbingState`'teki `warningTriggered` bayrağı ve `PlayerController.TriggerStaminaWarning` / `OnStaminaWarning` hâlâ kullanılmıyor (halka eşiği kendisi okuyor); ölü kod.
2. **Tırmanırken duvar zıplaması dikey (bilinçli).** Dışarı zıplama yalnızca **kayarken** var; tırmanırken zıplamak dümdüz yukarı atar (`WallJumpForce` hiç kullanılmıyor).
3. **Kenardan çıkma tam otomatik değil (bilinçli).** 0.1 sn yukarı basılı tutmak gerekiyor.
4. **Tırmanırken kanca atılamıyor**, kayarken atılabiliyor.
5. **Kayma yalnızca düşerken başlar.** Yukarı doğru giderken duvara değmek kaymaya geçirmez; tırmanma için bumper gerekir.
6. **Sınırsız duvar zıplaması zinciri (bilinçli).** Kayarken zıplamak kayma süresini yenilediği için iki duvar arasında sonsuza kadar yükselmek mümkün olabilir. Serbest kalıyor; seviye tasarımı bunu hesaba katmalı. Oynayarak doğrulanmadı.
