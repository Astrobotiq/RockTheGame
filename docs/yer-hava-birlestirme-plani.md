# Yer/hava durumlarını birleştirme planı

*16 Kasım 2026. Kaynak: [`precision-platformer-plani.md`](precision-platformer-plani.md) §3.4 #6. Yalnızca plan; kod değişmedi. Kararı ve task listesine eklemeyi geliştirici verir.*

## Sorun ne

`GroundedState` ile `AirborneState` ayrı sınıflar. Oyuncu yerden havaya her geçtiğinde yeni bir `AirborneState` kuruluyor. Havanın nasıl davranacağı (yerçekimi kuralı, kilitler, coyote, taşıyan yüzeyin hızı) constructor'a parametre olarak veriliyor.

- `AirborneState` constructor'ı **12 parametre** alıyor: hız, `isJumping`, üç kilit, coyote, üç `bypass*Gravity` bayrağı, `endEarlyGravityMultiplier`, `carriedVelocity`.
- Kodda **24 yerde** `new AirborneState(...)` var (9 dosya: üç duvar/ip durumu, `DashState`, `GroundedState`, `SlingshotState`, `LaunchPad`, `PlayerController`). Her biri parametre kombinasyonunu kendisi seçiyor.
- [`movement-core.md`](systems/movement-core.md)'deki "havaya giriş → yerçekimi kuralı" tablosunun dört satırı (tam kural, `bypassJumpGravity`, boost'lu yüzey, sabit 1×) bu kombinasyonların **sonucu**. Tablo kodda hiçbir yerde tek parça olarak yazmıyor.

Sonuç: yeni bir mekanik (örneğin PF-5'in havadan duvar zıplaması) havaya geçerken doğru kombinasyonu seçmek zorunda. Yanlış seçerse hata derlemede çıkmıyor, oynayınca çıkıyor. `SurfaceCarry`, `_carriedVelocityX` ve `bypass*` bayrakları hep bu sınırda bilgi taşıma zorunluluğundan doğdu.

## Öneri: iki aşama, ikincisi isteğe bağlı

### Aşama 1: "havaya giriş profili" (asıl kazanç, düşük risk)

Durumları birleştirmeden, parametre yığınını adı olan tek bir değere indirmek:

```csharp
public readonly struct AirEntry
{
    // yerçekimi kuralı, kilitler, coyote, taşıma
    public static AirEntry Jump(...);          // yerden zıplama, coyote zıplaması
    public static AirEntry WallJump(...);      // kayarken ve havadan duvar zıplaması
    public static AirEntry ClimbJump(...);     // tırmanırken dikey zıplama
    public static AirEntry SwingRelease(...);  // tek kanca bırakma (bypassJumpGravity)
    public static AirEntry SlingshotLaunch(...);
    public static AirEntry LaunchPad(...);
    public static AirEntry Fall(...);          // dash sonu, dual bırakma, duvardan düşme, kopma, respawn
    public static AirEntry WalkOff(...);       // kenardan yürüyerek düşme (coyote açık)
    public static AirEntry WallDrop(...);      // duvardan bırakma/düşme: sabit 1×, taşıma, duvar kilidi, coyote
}
```

- `new AirborneState(context, velocity, AirEntry.SwingRelease())`: okuyan kişi kuralı adından anlıyor.
- Tablo, fabrika metotlarının listesi olarak tek bir dosyada duruyor. Yeni bir mekanik ya mevcut bir profili seçiyor ya da yeni bir profil ekliyor, ve o profil tabloya ekleniyor.
- **Davranış hiç değişmez.** Her fabrika bugünkü parametre kombinasyonunu birebir üretir. Çağrı yerlerini taşımak mekanik bir iş.
- **Bilinçli farklar korunur.** Üç kanca çıkış kuralı (16 Kasım 2026 kararı, [`feel-notlari.md`](feel-notlari.md)) üç ayrı profil olarak kalır, adları farkı belgeler.

### Aşama 2: `NormalState` (büyük, isteğe bağlı)

`GroundedState` ve `AirborneState` tek bir `NormalState`'te birleşir (Celeste `StNormal`). Yerde olmak bir alan olur (`IsOnGround`), durum değişikliği olmaz.

- **Yere iniş** bugünkü `GroundedState.EnterState`'in yan etkilerini (dash, slingshot, stamina, kayma süresi yenileme, renk sıfırlama, iniş sesi, toz ve titreşim) aynı sırayla bir `OnLand()` içinde yapar, ardından hava profilini sıfırlar.
- **Yerden ayrılma** coyote'u başlatır. Hız ve taşıma zaten durumun içinde olduğu için aktarılacak bir şey kalmaz.
- Yerde ve havada farklı olan kurallar (koşu hızı anında `stick × 10`, yerde yalnızca sağ tetikle kanca, kol yönlendirme) `if (IsOnGround)` dallarına dönüşür.

Kazancı Aşama 1'in üstüne az: yer↔hava sınırındaki son iki aktarımı (coyote ve hız) kaldırır. Bunun bedeli, durum tipine bakan dış kodun hepsini değiştirmek.

**Envanter (16 Kasım 2026, Aşama 1 bittikten sonra).** `CurrentState` okuyan dış kodun çoğu `GroundedState`/`AirborneState`'e hiç bakmıyor:

| Yer | Neye bakıyor | Aşama 2 etkisi |
|---|---|---|
| `PlayerController.FixedUpdate` | köşe düzeltmesi: `is AirborneState \|\| is DashState` | **Değişir:** "normal durum ve yerde değil" |
| `SwingTutorial.ReadInput` | `IsAirborne = CurrentState is AirborneState` | **Değişir:** aynı soru |
| `PlayerController.OnDrawGizmos` | durum adını yazar | Yalnızca etiket ("NormalState") |
| `PlayerAnimationController` | Dash, Swing, DualSwing, WallClimb, WallSlide; yer için `IsGrounded` özelliği | Yok |
| `PlayerVFXManager` | Swing, DualSwing, Dash | Yok |
| `LocalPlayerBroadcaster` | Swing, DualSwing, Dash, WallClimb, WallSlide; yer için `IsGrounded` özelliği | Yok |
| `PenaltyHandler`, `PauseManager` | yalnızca `PlayerTransitionState` | Yok |
| `DoorJuicyVisual` | oyuncuya bakmıyor (kendi `SnapToCurrentState` metodu, isim benzerliği) | Yok |

Durumların kendi içinde: 25 `new AirborneState(...)` (hepsi artık `AirEntry` ile) ve 5 `new GroundedState(...)` (`AirborneState`, `SwingingState`, `DualSwingingState`, `WallSlidingState`, `LedgeClimbState`). Testlerde 24 yer (`WaitForState<GroundedState>`, `IsInstanceOf<AirborneState>` vb., 7 dosya; `PlayerRuleTestBase.LandOnFloor` dahil) ve `SwingTutorialFlowTests`'te yalnızca bir test adı.

Sonuç: Aşama 2'nin dış bedeli beklenenden küçük, üretim kodunda iki yer. Asıl iş durumların içi ve testler.

### Aşama 2'nin parçaları (yapılacaksa)

Her biri ~1–1.5 saat, sırayla:

1. **Testleri durum tipinden ayır** #test: `PlayerRuleTestBase`'e `WaitForGround()` / `WaitForAir()` (yer/hava sorusunu tip yerine `IsGrounded` ve "normal durumda mı" ile soran) yardımcıları ekle, 24 yeri onlara taşı. Davranış değişmez; Aşama 2'den sonra testlere dokunmak gerekmez. Kodla aynı gün değil, **önce**.
2. **`NormalState` iskeleti** #code: `GroundedState` + `AirborneState` kodunu tek sınıfa al; `IsOnGround` alanı, `OnLand()` (bugünkü `GroundedState.EnterState` yan etkileri aynı sırayla) ve `OnLeaveGround()` (`AirEntry.WalkOff`). Eski iki sınıf bu adımda `NormalState`'e yönlendiren ince sarmalayıcı olarak kalır, çağrı yerleri değişmez.
3. **Çağrı yerleri ve dış kod** #code: 30 çağrı yerini `NormalState`'e taşı, `PlayerController` köşe düzeltmesini ve `SwingTutorial`'ı yeni soruya bağla (`PlayerController`'a tek bir `IsInNormalAir` özelliği), eski sınıfları sil, `movement-core.md`'yi güncelle.
4. **Aşama 2 testi** #test: EditMode + PlayMode yeşil; gamepad turu (zıplama, iniş, üç kanca çıkışı, duvar, launch pad, dash'in yerde bitmesi). Riskler bölümündeki "dash yerde biterse bir adım hava" farkına özellikle bak. 3 ile aynı gün ya da ertesi gün.

**Öneri:** Aşama 1 yapıldı (16 Kasım 2026). Aşama 2'yi ancak yer↔hava sınırında yine hata çıkarsa düşün; envanter bedelin küçük olduğunu gösteriyor ama kazanç da küçük (coyote ve hız aktarımı).

## Riskler

- **İniş yan etkilerinin sırası.** `GroundedState.EnterState` hızın `y`'sini sıfırlıyor, kancaları bırakıyor ve kaynakları yeniliyor. Sıra değişirse D2/W2/L2 kuralları bozulur. PlayMode testleri bunu koruyor.
- **"Dash yerde biterse önce havaya, sonraki adımda yere" garipliği** ([`movement-core.md`](systems/movement-core.md)). Aşama 2'de bu bir adımlık hava kaybolur: iniş yan etkileri bir adım erken olur. Muhtemelen fark edilmez, ama davranış değişikliği sayılır.
- **Coyote ile boost'lu yüzey** (`LastMovingSurface`, yalnızca apex hang kapanır). Profilde ayrı bir bayrak olarak kalmalı.
- **His değişmemeli.** Bu iş bir yeniden düzenleme. Bitince gamepad ile hızlı bir tur gerekir: zıplama, kanca çıkışı, duvar zıplaması, launch pad.

## Önerilen task'lar (16 Kasım 2026'da TASKS.md Someday'e eklendi)

Her biri ~1–1.5 saat.

1. **Havaya giriş tablosunun kural testleri** #test: tablonun dört satırı için PlayMode testi. Örneğin "swing bırakınca tuş basılı olsa da yükselişte 3×", "dash sonu sabit 1×". Kural metinleri `bozulmamali-davranislar.md`'ye, geliştirici onayıyla. Aşama 1'den **önce**.
2. *(Yapıldı, 16 Kasım 2026: `Player/States/AirEntry.cs`, `AirEntryTests`. Kod okununca `WallDrop` eklendi; çift kanca da bu adımda taşındı.)* **`AirEntry` profili: tip ve fabrikalar** #code: struct'ı ve fabrikaları yaz, `AirborneState`'e profil alan bir constructor ekle (eskisi geçici olarak kalır). Dash, yer ve swing çağrı yerlerini taşı.
3. *(Yapıldı, 16 Kasım 2026.)* **`AirEntry` profili: kalan çağrı yerleri** #code: duvar durumları, slingshot, `LaunchPad`, `PlayerController`; eski 12 parametreli constructor'ı sil, `movement-core.md` tablosunu profillere bağla.
4. **Profil geçişi testi** #test: EditMode + PlayMode yeşil, gamepad ile hızlı tur (zıplama, üç kanca çıkışı, duvar zıplaması, launch pad). 3 ile aynı gün ya da ertesi gün.
5. *(Yapıldı, 16 Kasım 2026: yukarıdaki envanter ve parçalar.)* **`CurrentState` okuyan dış kodun envanteri** #code: tablodaki altı dosyanın durum tipine bakıp bakmadığını çıkar, `NormalState`'in kesin parçalara bölünmüş planını yaz.
