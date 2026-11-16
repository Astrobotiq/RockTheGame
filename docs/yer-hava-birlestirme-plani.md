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

| Durum tipine bakan yer | Ne yapıyor |
|---|---|
| `PlayerController.FixedUpdate` | köşe düzeltmesi (`is AirborneState`) |
| `SwingTutorial` / `SwingTutorialFlow` | `IsAirborne = CurrentState is AirborneState` |
| PlayMode testleri (5 dosya, ~15 yer) | `WaitForState<GroundedState>`, `IsInstanceOf<AirborneState>` |
| `PlayerAnimationController`, `PlayerVFXManager`, `LocalPlayerBroadcaster`, `PenaltyHandler`, `DoorJuicyVisual`, `PauseManager` | `CurrentState` okuyor. Tipe mi bakıyor, bakılmadı. Aşama 2'nin ilk işi |

**Öneri:** Aşama 1'i yap. Aşama 2'yi ancak Aşama 1'den sonra yer↔hava sınırında yine hata çıkarsa düşün.

## Riskler

- **İniş yan etkilerinin sırası.** `GroundedState.EnterState` hızın `y`'sini sıfırlıyor, kancaları bırakıyor ve kaynakları yeniliyor. Sıra değişirse D2/W2/L2 kuralları bozulur. PlayMode testleri bunu koruyor.
- **"Dash yerde biterse önce havaya, sonraki adımda yere" garipliği** ([`movement-core.md`](systems/movement-core.md)). Aşama 2'de bu bir adımlık hava kaybolur: iniş yan etkileri bir adım erken olur. Muhtemelen fark edilmez, ama davranış değişikliği sayılır.
- **Coyote ile boost'lu yüzey** (`LastMovingSurface`, yalnızca apex hang kapanır). Profilde ayrı bir bayrak olarak kalmalı.
- **His değişmemeli.** Bu iş bir yeniden düzenleme. Bitince gamepad ile hızlı bir tur gerekir: zıplama, kanca çıkışı, duvar zıplaması, launch pad.

## Önerilen task'lar (TASKS.md'ye eklenmedi, onay bekliyor)

Her biri ~1–1.5 saat.

1. **Havaya giriş tablosunun kural testleri** #test: tablonun dört satırı için PlayMode testi. Örneğin "swing bırakınca tuş basılı olsa da yükselişte 3×", "dash sonu sabit 1×". Kural metinleri `bozulmamali-davranislar.md`'ye, geliştirici onayıyla. Aşama 1'den **önce**.
2. **`AirEntry` profili: tip ve fabrikalar** #code: struct'ı ve fabrikaları yaz, `AirborneState`'e profil alan bir constructor ekle (eskisi geçici olarak kalır). Dash, yer ve swing çağrı yerlerini taşı.
3. **`AirEntry` profili: kalan çağrı yerleri** #code: duvar durumları, slingshot, `LaunchPad`, `PlayerController`; eski 12 parametreli constructor'ı sil, `movement-core.md` tablosunu profillere bağla.
4. **Profil geçişi testi** #test: EditMode + PlayMode yeşil, gamepad ile hızlı tur (zıplama, üç kanca çıkışı, duvar zıplaması, launch pad). 3 ile aynı gün ya da ertesi gün.
5. *(İsteğe bağlı, Aşama 2)* **`CurrentState` okuyan dış kodun envanteri** #code: tablodaki altı dosyanın durum tipine bakıp bakmadığını çıkar, `NormalState`'in kesin parçalara bölünmüş planını yaz.
