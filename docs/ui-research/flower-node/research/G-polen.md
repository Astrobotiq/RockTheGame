# G: Çiçek node'u polen partikül efekti teknikleri

Tarih: 2026-11-22. Yazan: VFX/teknik sanat araştırması (salt okunur; kod, shader, sahne, prefab değişmedi). Kapsam: oyuncu kancayı çiçeğe attığında/tutunduğunda çiçekten polen çıkması.

Etiketler: **[Doğrulandı]** = sayfa/dosya açılıp okundu. **[Çıkarım]** = benim yorumum veya hesabım. **[Başlangıç tahmini]** = hareket/his/süre sayısı, geliştiricinin kararı, gamepad testi ister.

## 0. Özet (en değerli bulgular)

1. Projede polen için en yakın hazır kalıp **`AcidReaction` + `FX_AcidImpact`**: tek ParticleSystem, emisyon kapalı, ilk temasta çocuk olarak bir kez kurulur, `ParticleSystem.Emit(EmitParams)` ile her parçacığa boyut/hız verilir, `IResettable` ile `Clear()` edilir. Polen aynı iskeleti kullanmalı, ama görsel olarak **piksel kare değil, pürüzsüz kâğıt şekli** olmalı.
2. Çiçeğin kaynak noktaları CPU'da hesaplanabilir: organ uçları diskin dışında `_OrganRing × R` yarıçapında 5 nokta, tepeden yarım adım (36°) kayık (`FlowerCenter.shader:20,109`). Polen "organ uçlarından dökülür" demek biyolojik olarak da doğru (organ = anter).
3. **Renk okunurluğu hesabı:** krem `#FFF3C4` gökyüzü `#71BAEA` üstünde yalnızca **1.90:1**, beyaz 2.12:1, lavanta 1.76:1; indigo `#303778` **5.10:1**, kobalt `#1536E8` 3.64:1. Yani yalnızca krem/beyaz polen gökte kaybolur. Çözüm: **iki ton**: krem yüz + indigo kesim gölgesi (çiçeğin kendi gölgesiyle aynı yön ve renk: `FlowerShadow.cginc:26-27`, `_CutShadowCol` = `#303778`), küçük taneler ise koyu (indigo/kobalt).
4. **CRT alt sınırı kameraya bağlı:** CRT 480×360'a indiriyor; ortho 8'de 1 birim ≈ 22.5 CRT px, yani projenin "min 2 sanat pikseli" (0.125 birim) ≈ 2.8 CRT px. Ortho 22'de (oda 16) aynı 0.125 birim yalnızca ≈ 1 px olur ve titrer. Min çap `ortho/60` ile ölçeklenmeli [Çıkarım, §3.4].
5. **Hit-stop bayrak, `timeScale` değil** (`HitStopManager.cs:25-48`), bu yüzden partiküller hit-stop'ta durmaz (`hazards.md:92` bunu zaten kenar durum olarak not ediyor). Polen hit-stop ile aynı ana denk gelirse `HitStopStarted/Ended` ile `Pause/Play` yapılabilir.
6. **Flaş sınırlayıcı polen için gerekmez, ama kuralı bozmamak için hesaplandı:** 36 parçacık × ~6 px² ≈ ekranın %1'i; WCAG eşiği eşzamanlı alanın 10° görüş alanının %25'i (yaklaşık ekranın %11'i) ve **10% göreli parlaklık değişimi çifti** ister. Polen ışık yanıp sönmesi değil, küçük opak kâğıt; yine de patlamada "beyaz flaş diski" eklenirse `FlashLimiter` (`FlashLimiter.cs:11-37`) üzerinden geçmeli.

## 1. Depodan okunan kalıplar (dosya:satır)

### 1.1 Partikül kurulum kalıpları

| Kalıp | Nerede | Polen için anlamı |
|---|---|---|
| Lazy çocuk PS: `impactFxPrefab` alanı, ilk temasta `Instantiate(prefab, transform)` bir kez | `Death/AcidReaction.cs:26-27, 133` | Hiç tutulmamış çiçekte sıfır maliyet. `LaunchPad.cs:208-211` ise her fırlatmada `Instantiate` ediyor (havuzsuz); polen onu kopyalamamalı. |
| Emisyon kapalı, tek PS, `Emit(EmitParams, 1)` döngüsü; `startSize`, `position`, `velocity` kod ile | `AcidReaction.cs:146-165`; `Platform/HalftoneTrailEmitter.cs:58-59, 213-222` | Boyut sınıfı, kaynak, koni yönü tam kontrol. `applyShapeToPosition = false` (`AcidReaction.cs:152`). |
| Prefab: `maxNumParticles 128`, `looping 1`, `playOnAwake 1`, EmissionModule `enabled 0`, Size/Color over Lifetime açık; sorting layer **VFX** (id 399991616), order 5; `m_MaxParticleSize 0.5` | `Prefab/New/VFX/FX_AcidImpact.prefab` (satır 868, 978, 1311, 4793-4821) | Sayısal başlangıç şablonu. `FX_LandingDust`: emisyon açık, **Limit Velocity açık**, `maxNumParticles 1000`. |
| World uzay, `scalingMode Local`, `gravityModifier 0`, `maxParticles` ayardan | `HalftoneTrailEmitter.cs:49-56` | Çiçek ölçeği 4 (`grapple.md:91`): `Local` olmazsa parçacık boyutu 4× olur. |
| Sürtünme: `limitVelocityOverLifetime`, `limit 0`, `dampen = 1 - exp(-drag/60)` ("kare hızına bağımlı, yaklaşık") | `HalftoneTrailEmitter.cs:88-95` | Kare hızından bağımsız olması için Unity'nin **Drag** alanı tercih edilebilir (§2, Limit Velocity). |
| Boyut/alfa eğrisi: boyut (0→1, 0.4→1, 0.75→0.5, 1→0.5), alfa 1 → 0.75'te 1 → 1'de 0 | `HalftoneTrailEmitter.cs:62-72` | Küçük parçacık sönerken sıfır boyuta inmez, alfa ile biter (CRT titremesini azaltır). Yalnızca teknik kalıp; halftone görsel dili kullanılmayacak. |
| Bütçe: `TrailBudget.TryReserve` ile ink başına + toplam sınır, dolunca doğum atlanır | `Platform/HalftoneTrailLogic.cs:219-255` | Polen için daha basit karşılığı `ps.particleCount + n <= maxParticles` kontrolü. Not: `TrailBudget` 3 mürekkebe sabit. |
| Saf doğum biriktirici `Accumulate(ref acc, distance, perUnit)` | `HalftoneTrailLogic.cs:52-58` | "Yaprak sarsıldıkça polen dök" için mesafe yerine yay hızı verilerek yeniden kullanılabilir. |
| Sprite-Unlit materyal: 2D Renderer `Particles/*` shader'larını desteklemez; en küçük damla 2 px (CRT 480×360) | `docs/systems/hazards.md:57`; `Assets/Material/VFX/M_FX_AcidPixel.mat` | Polen materyali de `Sprite-Unlit` tabanlı, alfa karışım. |
| Sorting layer'lar: Background < Default < Platform < Player < VFX < UI | `ProjectSettings/TagManager.asset:41-57`; çiçek `Default` (`Node-Flower.prefab:756`) | Polen çiçeğin önünde, oyuncunun arkasında olmalı → **Player katmanı, order −1** [Çıkarım; kanca/ip aynı katmanda mı bakılmadı, test et]. VFX katmanı oyuncunun üstüne biner ve 1×1 pembe sprite'ı örter. |
| Derinlik: yapraklar ZWrite, `depthBehind` ile geri itilir; oyuncu `Player`, çiçek `Default` | `docs/systems/grapple.md:92` | Z=0'daki sprite partiküller yaprakları geçmeli [Çıkarım; doğrulanmadı]. |

### 1.2 Tetikleme, sıfırlama, geri bildirim kalıpları

- **Hook noktası:** `PlayerController.AttachAnchor(arm, point, node)` (`PlayerController.cs:227-257`): kanca node'a her bağlanışta (yerden `NormalState.cs:188`, havada `AttemptGrappleTransition`) tek merkezden geçer. `node.GetComponentInParent<IGrabbableNode>()` zaten tutulan node için kullanılıyor (`PlayerController.cs:289-295`). **Çiçeğe "tutunuldu" bildiren bir olay şu an yok**: `FlowerCycle` yalnızca `GrabbableChanged` yayınlıyor (`FlowerCycle.cs:36`).
- **Çiçek döngüsü:** `FlowerCycleTimings.Default`: kapalı 1.8, açılış 0.5, açık 2.2, uyarı 0.7, snap 0.15 sn, `grabAt 0.3`, `coyote 0.1` (`FlowerCycleMath.cs:35-45`). Yaylar: `OpenBloom/CloseBloom/SetWarn/SetHint/Snap` (`FlowerBloom.cs:260-292`); açılış yayı 180/14.8, snap 900/48, pop 260/12.
- **Sıfırlama:** `IResettable.ResetToDefault()` + `LevelResetManager.Register` `OnEnable`'da (`AcidReaction.cs:52-60`, `HalftoneTrailEmitter.cs:98-106`). `AcidReaction.ResetToDefault` yalnızca `_fx.Clear()` yapar (`:73-76`). **İki mekanizma farkı:** `IResettable` iris kapandıktan sonra, `PlayerHealth.OnDeath` ölüm anında çalışır (`death-respawn.md:12-19, 107`). Polen için `IResettable` doğru, çünkü ölüm anında temizlemek polenin gözün önünde kaybolması demek.
- **Hit-stop:** `HitStopEvents.RequestHitStop/HitStopStarted/HitStopEnded/IsActive` (`IFramePausable.cs:17-24`), `HitStopManager` `Time.unscaledDeltaTime` ile sayar (`:34-48`); `PlayerController` `OnEnable/OnDisable` ile abone (`PlayerController.cs:169-176`). Dash/slingshot hit-stop'u `Stats.HitStopDuration` = 0.15 sn (`PlayerStatsSO.cs:105`, `PlayerController.cs:330`); ölümde `hitstopSeconds` = 0.09 (`DeathBurstSettingsSO.cs:14`). `SpiralNode.cs:129` zaman kapısını `!HitStopEvents.IsActive && !PauseManager.BlocksGameplay` ile yapıyor.
- **Flaş:** `FlashLimiter(maxPerWindow, window, reducedIntensity)`, `Acquire(now)` 1 ya da `reducedIntensity` döner; pad'ler arası statik paylaşım (`LiquidLightBurst.cs:55-57, 113-119`; ayar: 2 flaş/sn, azaltılmış 0.3).
- **Ayar SO deseni:** `AcidReactionSettingsSO`: güç aralığı (`minSpeed 4`, `maxSpeed 40`, `strengthFloor 0.35`), parçacık sayısı `Vector2Int` (taban, tam güç), hız `Vector2`, boyut px, koni 55°, `inheritX 0.3`, `inheritY 0.1`, `maxDropletSpeed 9`, `pixelsPerUnit 16`, ve aynı SO içinde `RumbleProfile` + `AudioCueSO` alanları (`AcidReactionSettingsSO.cs:14-47`); `OnValidate` ile sıralama (`:49-61`). Güç eğrisi saf fonksiyon: `AcidImpactMath.Strength(speed, min, max, floor)` (`AcidReaction.cs:87-89`).
- **Titreşim:** `HapticManager.Vibrate(RumbleProfile)` çalanı iptal eder (`HapticManager.cs:56-65`); `RumbleProfile` = süre + sol/sağ motor eğrisi (`RumbleProfile.cs:6-11`). Sistem başına SO (`PlatformVibrationSettingsSO.cs:7-14`) ya da `AcidReactionSettingsSO` gibi aynı SO içinde. Dash titreşimini çiçek titreşimi **keser**, tersi de olur; kısa ve hafif tutmak gerekir.
- **Ses:** `AudioCuePlayEventChannelSO.RaisePlayEvent(cue, AudioCuePlayParams)`; 2D çalınır; cue/klip boşsa sessiz geçer; ses seviyesi ve perde güce göre (`AcidReaction.cs:106-113`).
- **Ölüm efekti ölçekleri (karşılaştırma):** `DeathBurst` `MaxParticles = 400` önceden üretilmiş SpriteRenderer (`DeathBurst.cs:17, 86`), süre 0.6 sn, `cellTexels 2`, `shakeForce 15` (`DeathBurstSettingsSO.cs:16, 23, 40`). Yani ölüm ≈ 400 parçacık; polen ölüm olayından çok daha küçük ve yumuşak kalmalı (ölüm hikâyenin en yüksek sesi).

### 1.3 Ölçü bilgileri

- Çiçek kök ölçeği 4, çap ~9 birim; oyuncu 1×1 (`grapple.md:91`). Disk yarıçapı = `coreRadius 0.22 × ölçek 4 ≈ 0.88` birim, yaprak boyu `0.95 × 4 ≈ 3.8` birim (`FlowerBloom.cs:144-145`) [Çıkarım: kök ölçeğinin tamamı çarpan].
- Kamera: `minOrthoSize` 8 (`camera.md:14`), oda 4 çerçevesi 10.25 (`camera.md:48`), oda 16 ortho 22 (`hazards.md:93`).
- Kesim gölgesi: ofset (+1, −1) görünüm uzayı (sağ-alt), `_CutShadowRel 0.042`, alt sınır 2 px, renk `(0.188, 0.216, 0.471)` = `#303778` (`FlowerShadow.cginc:26-27`, `Mat_FlowerPetalShadow.mat`).
- Organlar: `_OrganCount 5`, `_OrganRing 1.44` (diskin yarıçapı oranı), `_OrganDot 0.34`, krem `(1, 0.953, 0.769)`; stigma `_TriRing 0.53`, 3 beyaz nokta (`FlowerCenter.shader:13-22`, satır 109 "tepeden yarım adım kayık").

## 2. Kaynaktan doğrulananlar (okunan sayfalar)

| # | Kaynak | Doğrulanan bilgi (kısaltılmış, kendi cümlelerimle) |
|---|---|---|
| 1 | [Unity: Emission modülü](https://docs.unity.com/en-us/engine/6000.0/manual/visual-effects/particle-systems/particle-system-modules/part-sys-emission-module) | Rate over Time, Rate over Distance ve Burst (Time, Count, Cycles, Interval, Probability). Burst, sistem başladıktan sonra `Time` saniyede ateşlenir; Probability her ateşlemede doğup doğmayacağını belirler. |
| 2 | [Unity: Noise modülü](https://docs.unity.com/en-us/engine/6000.0/manual/visual-effects/particle-systems/particle-system-modules/part-sys-noise-module) | Strength, Frequency, Scroll Speed, Damping (açıkken güç frekansla orantılı, alan ölçeklenince davranış korunur), Octaves (maliyetin ana kaynağı), Quality (düşürmek maliyeti çok azaltır; dokümana göre istenen davranışı veren en düşük kalite seçilmeli), Remap, Position/Rotation/Size Amount. |
| 3 | [Unity: Limit Velocity over Lifetime](https://docs.unity.com/en-us/engine/current/manual/visual-effects/particle-systems/particle-system-modules/part-sys-limit-vel-over-life-module) | Speed (limit), Dampen (limiti aşınca hızın azalma oranı), **Drag** (doğrusal sürtünme), Multiply by Size (büyük parçacık daha çok sürtünür), Multiply by Velocity (hızlı parçacık daha çok sürtünür). Hava direnci için uygun olduğu not edilmiş (arama özetinden; sayfanın giriş cümlesi bu bağlantıdan okunamadı, tablo okundu). |
| 4 | [Unity: Force over Lifetime](https://docs.unity.com/en-us/engine/current/manual/visual-effects/particle-systems/particle-system-modules/part-sys-force-over-life-module) | X/Y/Z kuvvet, Space (local/world), **Randomize** (iki sabit/eğri modunda her karede aralıkta yeni yön seçer: türbülanslı, düzensiz hareket). |
| 5 | [Unity: Sub Emitters](https://docs.unity.com/en-us/engine/6000.0/manual/visual-effects/particle-systems/particle-system-modules/part-sys-sub-emit-module) | Birth/Collision/Death/Trigger/Manual; Emit Probability. Birth tüm Emission ayarlarını ve çoklu burst'ü destekler; diğerleri tek burst ile sınırlı. `Manual` = `TriggerSubEmitter` ile koddan. |
| 6 | [Unity: Texture Sheet Animation](https://docs.unity.com/en-us/engine/current/manual/visual-effects/particle-systems/particle-system-modules/part-sys-tex-sheet-anim-module) | Grid ve **Sprites** modu; Frame over Time, Start Frame (parçacıkları rastgele fazlamak için), Cycles; Time Mode (Lifetime/Speed/FPS); Single Row + Random row ile parçacık başına rastgele satır. |
| 7 | [Unity: Particle System Renderer](https://docs.unity.com/en-us/engine/current/manual/visual-effects/particle-systems/particle-system-modules/part-sys-renderer-module) | Sorting Layer ID + Order in Layer, Sorting Fudge (tüm sistemi saydam nesnelere göre kaydırır, sistem içi sıralama yapmaz), Sort Mode (sistem içi), **Min/Max Particle Size** (viewport oranı olarak, yalnızca Billboard kipleri), Custom Vertex Streams. |
| 8 | [Unity: Main modülü](https://docs.unity.com/en-us/engine/current/manual/visual-effects/particle-systems/particle-system-modules/part-sys-main-module) | Stop Action (Disable/Destroy/Callback; `OnParticleSystemStopped` ile havuza dönüş), Culling Mode (tek seferlik efektler için Always Simulate önerilir), Ring Buffer (kısa efektte Disabled), Max Particles (limit aşılınca fazlası kaldırılır), Gravity Modifier (0 = yerçekimi yok), Simulation Space, Scaling Mode (Local yalnızca kendi ölçeğini kullanır). |
| 9 | [Unity Learn: Create 2D particle effects](https://learn.unity.com/tutorial/create-2d-particle-effects) | 4×4 atlas dilimleme, Texture Sheet + rastgele başlangıç karesi, iki sabit arası rastgele lifetime/speed/size, Color/Size over Lifetime ile sönme (boyut eğrisi önce sabit sonra düşer), World simulation space, `Stop()` ile yeni doğumu kesip mevcutların bitmesine izin verme, tek seferlik efektte Rate 0 + tek burst. |
| 10 | [Unity Learn: VFX for 2D projects](https://learn.unity.com/pathway/game-development/unit/visual-effects/tutorial/vfx-for-2d-projects) | Yerleşik PS 2D'de de çalışır; Gravity Source **2D Physics** seçeneği ve **Circle/Square gibi 2D Shape** şekilleri 2D projede daha verimli. |
| 11 | [Unity: 2D Renderer Data](https://docs.unity.com/en-us/engine/current/manual/unity2d/2d-urp/2drenderer-data-overview) | Default Material Lit/Unlit/Custom (`Sprite-Lit-Default` 2D ışıkla etkileşir, `Sprite-Unlit-Default` etkilemez); Transparency Sort Mode. Sayfa partiküllerden söz etmiyor. |
| 12 | [Unity blog: Happy Harvest 2D teknikleri](https://unity.com/en/blog/games/happy-harvest-demo-latest-2d-techniques) | Yerleşik PS CPU'da çalışır, VFX Graph GPU'da; örnekler: düşen yapraklar, flipbook güveler (yerleşik PS), yağmur 2D Lit shader ile (VFX Graph). Sayfa sıralamayı anlatmıyor. |
| 13 | [vfxdoc: Authoring VFX](https://vfxdoc.readthedocs.io/en/latest/vfx/authoringvfx/) | **3-3-3 kuralı** (üç boyut, üç hız, üç ritim; küçük/hızlı/kesik, orta/orta/senkoplu, büyük/yavaş/sürekli); büyük-yavaş ile küçük-hızlı karışımı hiyerarşi verir; önemli efekt öncesinde uyarı (en az ~0.25 sn, tepki bekleniyorsa ~0.65 sn); emisyonu tek patlama, yükselen akış ya da rastgele patlamalar olarak kurma; efekti ses zarfı gibi attack/decay/sustain/release düşünme; tekrar eden efektte örnekler arası çeşitlilik. |
| 14 | [PocketGamer.biz: VFX optimisation (Azur Games)](https://www.pocketgamer.biz/vfx-optimisation-in-midcore-games-commonly-overlooked-techniques-and-advanced-methods/) | Aynı anda çoğalan efektler için bütçe örnek başına 3-10 parçacık kadar sıkı olabilir; 200 küçük yerine daha az, daha büyük parçacık; overdraw (genişlik ve yükseklik ikiye çıkınca maliyet dört katı, dokuyu şekle sıkı kırp); Collision, Noise ve Trails'i idareli kullan, Limit Velocity ve hız eğrileri ile taklit et. Havuzlama/culling anlatılmıyor. |
| 15 | [WCAG 2.2 Understanding 2.3.1](https://www.w3.org/WAI/WCAG22/Understanding/three-flashes-or-below-threshold.html) | Saniyede 3'ten fazla yanıp sönme yok **ya da** eşiğin altında; genel flaş eşiği: ters yönlü göreli parlaklık değişimi çifti ≥ %10 ve koyu durum < 0.80; kırmızı flaş ayrı; eşzamanlı flaş alanı ≤ 0.006 steradyan (10° görüş alanının %25'i; 1024×768'de ≈ 341×256 px). |
| 16 | [WCAG 2.2 Understanding 1.4.11](https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast.html) | İçeriği anlamak için gereken grafik nesneler komşu renge karşı ≥ 3:1; süs amaçlı grafik ya da aynı bilgi başka biçimde verilmişse şart değil. (Polen süs ama "tutundun" geri bildirimi taşıdığı için 3:1 hedefi iyi bir ölçüt [Çıkarım].) |
| 17 | [Wikipedia: Twelve basic principles of animation](https://en.wikipedia.org/wiki/Twelve_basic_principles_of_animation) | Anticipation, follow-through/overlapping action (gevşek parçalar ana gövde durduktan sonra da hareket eder, farklı zamanlarla), secondary action (ana hareketi desteklemeli, dramatik harekette öncesine/sonrasına konmalı), slow in/slow out, timing, exaggeration. |
| 18 | [Wikipedia: Pollen](https://en.wikipedia.org/wiki/Pollen) | Boyut aralığı çok geniş (≈2.5-5 µm unutma-beni'den ≈90-100 µm mısıra; çim ≈20-25 µm); şekil çeşitliliği (küremsi, kanatlı, çıkıntılı/dikenli, ağsı yüzey); rüzgârla taşınan bitkiler çok miktarda hafif polen salar. |
| 19 | [GDC Vault: Juice It or Lose It](https://gdcvault.com/play/1016487/Juice-It-or-Lose) | Yalnızca oturum açıklaması okundu: Martin Jonasson ve Petri Purho, Independent Games Summit (GDC Europe 2012), "her oyuna uygulanabilecek küçük hileler". Parçacık/sarsıntı ayrıntıları bu sayfada yok (aşağıda Doğrulanamayanlar). |
| 20 | [freegamesprites: 2D combat juice](https://freegamesprites.com/es/news/game-feel-2d-combat-juice-tricks) | **Zayıf kaynak (blog)**: hitstop 2-10 kare (60 fps), hafif sarsıntı 1-2 px ~100 ms, ağır 4-8 px ~250 ms; çarpmada tek patlama 6-12 parçacık, 200-400 ms ömür, sıfıra küçülme, hafif yerçekimi. Sayısal ölçüt olarak değil, "mertebe" olarak kullanıldı. |

## 3. Yorum ve tasarım gerekçeleri [Çıkarım]

### 3.1 Hangi şekil çiçek diline yakışır

Çiçek: düz renk, hafif farklı el kesimi yapraklar, indigo hard shadow, piksel sanat değil. Polen adayları:

| Aday | Kâğıt kesmesi uyumu | Küçükte okunurluk | Karar |
|---|---|---|---|
| Düz yuvarlak nokta | iyi (bir delgeçle çıkmış konfeti) | iyi | **Ana ağırlık (≈%50)** |
| Elips (hafif dönük) | iyi; "tohum/polen" okunur, dönünce nabız verir | iyi | **%30** |
| Küçük kâğıt parçası (yuvarlatılmış kare / yaprak-balık) | çok iyi; iki tonlu kesim gölgesi taşır; hareket "kâğıt" der | boyut ≥ 0.3 birim gerekir | **%15, nadir, "hero" parçacık** |
| Çift tonlu "tohum" (krem yüz + indigo hilal) | en çiçek diline özgü | ≥ 0.3 birim | Chip'in bir varyantı olarak |
| Gerçekçi polen (diken/ağ yüzey) | kâğıt diline ters (detay) | 3-6 px'te zaten kaybolur | Kullanma |
| Yumuşak/ışıltılı toz, additive | kâğıda ters; CRT'de titrer ve flaş riski | | **Kullanma** |

Sprite'lar **pürüzsüz vektör** (anti-aliased, 32×32 ya da 64×64 atlas), pikselleştirilmiş değil; konum **piksel ızgarasına oturtulmaz** (`DeathBurst`'ün `Snap` kalıbı kullanılmaz, `DeathBurst.cs:206`). Hand-cut hissi için her şeklin 3 hafif düzensiz varyantı (poligon 8-10 kenar), `Texture Sheet → Sprites` kipi ve rastgele `startFrame` ile dağıtılır (Kaynak 6, 9).

### 3.2 Renk (çiçek paleti, gökyüzü `#71BAEA` üstünde)

Hesaplanan WCAG göreli parlaklık oranları (benim hesabım, `python` ile):

| Renk | Gökyüzüne karşı | Karakter pembesine `#FF51DE` karşı | OKLCH (L / C / h°) | Not |
|---|---|---|---|---|
| Krem `#FFF3C4` | **1.90:1** | 2.52 | 0.96 / 0.06 / 95 | tek başına gökte zayıf |
| Beyaz `#FFFFFF` | 2.12:1 | 2.81 | 1.00 / 0 / 90 | stigma ile aynı |
| Lavanta `#E5E9FF` | 1.76:1 | 2.33 | 0.94 / 0.03 / 278 | en zayıf |
| Indigo `#303778` | **5.10:1** | 3.85 | 0.37 / 0.11 / 275 | gökte en iyi |
| Kobalt `#1536E8` | 3.64:1 | 2.75 | 0.47 / 0.26 / 266 | gökte iyi |
| Amber `#FFC94D` | 1.38:1 | 1.83 | 0.86 / 0.15 / 85 | uyarıya ayrılmış, polende yok |
| Karakter pembesi `#FF51DE` | 1.32:1 | 1.00 | 0.72 / 0.25 / 336 | polen bu hue'ya yaklaşmaz |

Sonuçlar:
- Krem hue 95° ile pembe 336° arası ≈ 119°; karışma riski yok. Amber hue 85°, krem ile 10° fark ama kroma (0.06 / 0.15) ve L (0.96 / 0.86) farkı var; yine de krem polenin **tonu sarıya kaydırılmaz** ve amber uyarı fazında polen **hiç çıkmaz**.
- Gökte krem/beyaz tek başına 3:1'e ulaşmadığı için okunurluk **iki tonla** (krem yüz + indigo kesim gölgesi) ya da **koyu tane** ile gelir. Kural önerisi: ≥ 0.3 birim parçacık krem + indigo hilal; 0.12-0.28 birim taneler indigo/kobalt ağırlıklı ve az sayıda krem. Bu, çiçeğin üstünde (koyu indigo ip tacı) kremin, gökte indigonun okunmasını sağlar.
- Pembe/lila halkalar ve oyuncu rengiyle karışmama: polen paletinde pembe, magenta, lila yok; lavanta `#E5E9FF` yalnızca %10 ve yalnızca büyük parçalarda (zaten gökte en zayıf).

Önerilen renk dağılımı [Başlangıç tahmini]: krem %40, beyaz %15, lavanta %10, indigo %25, kobalt %10.

### 3.3 Tutunma anında "burst + drift", tutunma süresince ne olmalı

Vfxdoc'taki üç emisyon biçimi (tek patlama, yükselen akış, rastgele patlamalar; Kaynak 13) ve animasyon prensibi (anticipation → ana hareket → follow-through; Kaynak 17) birlikte okunduğunda:

- **Tek patlama + kısa sönümlü damlama** en uygun. Oyuncu çiçekte 1-3 sn asılı kalabiliyor (açık faz 2.2 sn). Sürekli sabit emisyon 8 çiçek × iki kol senaryosunda gürültü yaratır ve "tutundun" bilgisini sulandırır. Çözüm: tutunma anında patlama, sonraki ~0.6 sn içinde üstel azalan 2-5 parçacık (isteğe bağlı), sonra sessizlik.
- Çiçek **kapanırken** (uyarı fazı 0.7 sn) polen **çıkmaz**: uyarı, projede bilinçli olarak titreme + iplik büzülmesi + gölge + kesikli halka ile (`grapple.md:99`) renksiz kanaldan veriliyor; polen o kanala karışmamalı.
- **Snap** (0.15 sn) ve oyuncunun fırlatılması: küçük bir "püf" (6-10 parçacık, hızlı, dışa doğru) kapanma enerjisini görselleştirir; kapanma enerjisi çiçeğin en güçlü olayı olduğu için polen burada en geniş açıda ve hızda çıkabilir. İlk sürüme zorunlu değil (§7, Aşama 2).
- **Anticipation → burst → drift ritmi:** kancanın değdiği an (t=0) çiçek yapraklarını sarsar; polen **yaprak sarsıntısından 2-4 kare (≈0.03-0.07 sn) sonra** çıkar. Bu gecikme "follow-through/overlapping action" prensibidir: gevşek parçalar ana gövdeden geç tepki verir (Kaynak 17). [Başlangıç tahmini: 0.05 sn.]

### 3.4 CRT altında küçük partikül ve kamera ölçeği

Proje kuralı: asit damlası ≥ 2 sanat pikseli (`hazards.md:57`, `death-respawn.md:30`); CRT ekranı 480×360'a indiriyor. 360 piksel dikey için 1 CRT px = `2·ortho/360` birim.

| Ortho | CRT px / birim | 0.125 birim (2 sanat px) kaç CRT px | 3 CRT px kaç birim |
|---|---|---|---|
| 8 (`minOrthoSize`) | 22.5 | 2.8 | 0.133 |
| 10.25 (oda 4) | 17.6 | 2.2 | 0.171 |
| 22 (oda 16) | 8.2 | 1.0 | 0.367 |

Yani 0.125 birimlik sabit alt sınır normal kamerada 2.8 CRT px olup projenin kuralıyla örtüşüyor, ama geniş kamerada 1 px'e iner ve titrer. [Çıkarım]: emit anında `minDiameter = ortho / 60` ile alt sınır ver (3 CRT px). Unity'nin Renderer `Min Particle Size` alanı (viewport oranı, Billboard; Kaynak 7) aynı işi kamera bağımsız yapabilir (≈ 3/360 ≈ 0.0083), ama (a) hangi eksene göre oranlandığı sayfada yazmıyor, (b) küçülme eğrisinde parçacığı sabit tutup sonunda "pop" ile yok eder; bu yüzden ilk tercih script ile alt sınır, Renderer sınırı yalnızca test edilirse ikinci seçenek.

## 4. Katman, sıralama, derinlik

- **Katman:** `Player` sorting layer, order −1 [Çıkarım, doğrulanması gerek]: çiçeğin (Default) önünde, oyuncunun arkasında. `VFX` katmanı oyuncunun üstüne biner; pembe 1×1 sprite'ı (22 CRT px) örtebilir.
- **Derinlik (2D):** iki sınıf: **yakın** (büyük, hızlı, çiçeğin önünde) ve isteğe bağlı **uzak** (çok küçük, yavaş, **çiçeğin arkasında**, `Default` katmanında çiçek order'ının altı). Uzak katman yapraklarla kısmen örtülür ve derinlik verir; ama ikinci PS (ikinci renderer) demek. İlk sürümde yalnızca yakın sınıf; uzak sınıf Seçenek B'de.
- **Karışım:** alfa karışım, `Sprite-Unlit`. Additive yok: kâğıt diline ters, CRT altında parıldar, flaş riskini artırır.
- **Sort Mode:** `Oldest in Front` ya da None; sistem içinde sıralama gerekmiyor (Kaynak 7).
- **Z:** yaprak ZWrite'ı çiçeği kameradan uzağa itiyor (`grapple.md:92`); polen z=0'da kalacak, depth testini geçmeli. [Doğrulanmadı: sprite shader'ının ZTest'i ve `depthBehind` ile etkileşimi test edilmeli.]

## 5. Performans, havuz, sıfırlama

- **Sayı üst sınırı:** çiçek başına `maxParticles` = 48 [Başlangıç tahmini]. Aynı anda en fazla iki kol tuttuğu için (çift kanca) pratik tavan ≈ 2 patlama + son 1-2 sn'nin kalıntıları ≈ 40-70 canlı parçacık; 8 çiçeğin hepsi dolsa bile toplam < 400 (ölüm efekti `MaxParticles` 400, `DeathBurst.cs:17`). Azur'un uyarısına uyularak (Kaynak 14) küçük taneler yerine **daha az, daha büyük** parçacık tercih edilir: Seçenek A'da 18 parçacık yeterli.
- **Doldurma politikası:** `ps.particleCount + n > maxParticles` ise n kırp; ayrıca Unity zaten fazlayı kaldırır (Kaynak 8). Birden çok çiçeğin aynı karede aynı sesi çalmaması için **global kısa bekleme** (ses ve titreşim için ~0.1 sn) [Başlangıç tahmini].
- **Havuz:** ayrı havuza gerek yok; **lazy çocuk PS** (AcidReaction kalıbı) yeterli: ilk tutunmada oluşur, çiçek odayla uyur/uyanır, parçacık sayısı sabit tavanlı. Çiçek başına bir PS = 8 renderer; draw call'ı azaltmak istenirse aynı materyal + aynı sorting → dinamik batching olasılığı [Çıkarım, ölçülmedi]. Alternatif "oda başına tek paylaşımlı polen PS" tek draw call + tek `Clear` verir, ama çiçekleri odaya bağlamak ve oda uyku/uyanıklığıyla yönetmek ekstra kod getirir; ilk sürümde gerekmiyor.
- **Stop and clear:** döngüsüz (`looping` kapalı) ya da `FX_AcidImpact` gibi `looping 1` + emisyon kapalı; her ikisi de Emit ile çalışır. `ResetToDefault()` → `Clear()` + tutma/damlama durumunu sıfırla; `OnEnable`'da da `Clear()` (oda uyanınca donmuş polen kalmasın, `AcidReaction.cs:52-60`). Polen `PlayerHealth.OnDeath`'e değil `IResettable`'a bağlanmalı (iris kapalıyken temizlenir, görünmez).
- **Hit-stop/pause:** `HitStopEvents.HitStopStarted` → `main.simulationSpeed = 0` (ya da `Pause`), `HitStopEnded` → 1; `PauseManager.BlocksGameplay` iken damlama emisyonu durur (`SpiralNode.cs:129` kalıbı). `useUnscaledTime` kapalı kalsın (pause'ta partikül de dursun).
- **Modül maliyeti:** Noise yalnızca 1 oktav, Quality Low, çünkü 48 parçacıkta ihmal edilebilir ama 8 çiçekte gereksiz olmasın (Kaynak 2, 14). Collision, Trails, Sub Emitter kullanılmaz.
- **Flaş güvenliği hesabı** [Çıkarım]: 36 parçacık × ~(6 px)² ≈ 1300 px² ≈ 480×360 tamponun ≈ %0.75'i; WCAG eşzamanlı alan eşiği ≈ %11 (341×256/1024×768) ve eşik çifti göreli parlaklıkta ≥ %10 ister (Kaynak 15). Polen opak kâğıt ve çok küçük alan; sorun yok. Patlamaya "beyaz flaş diski" eklenirse `FlashLimiter` kullanılmalı (ayar 2/sn, azaltılmış 0.3).

## 6. Tetikleme, kaynak ve geri bildirim eşleşmesi

### 6.1 Olay akışı [Çıkarım]

```
Kanca değer (AttachAnchor, PlayerController.cs:227)
  -> node.GetComponentInParent<IGrabReactive>()?.OnGrabbed(arm, point, playerVelocity)   // yeni küçük arayüz, IGrabbableNode kalıbı
       t = 0      : yaprak sarsıntısı (FlowerBloom.Jolt, yeni) + haptik (hafif, kısa)
       t = +0.05  : polen patlaması (EmitParams) + ses cue'su (aynı kare)
       t = +0.05..0.65 : isteğe bağlı sönümlü damlama (yalnızca tutuluyorsa ve faz Open)
Çiçek Warning fazına girer -> damlama durur, yeni polen yok
Çiçek Closing/Snap -> (Aşama 2) dışa "püf" (6-10 parçacık) + ReleaseImpulse ile eşzamanlı
Ölüm/respawn -> IResettable.ResetToDefault -> Clear
```

`FlowerCycle` faz bilgisini (`Phase`, `GrabbableChanged`) zaten yayınlıyor (`FlowerCycle.cs:31-36`); damlamayı ve snap püfünü bu olaydan sürmek yeni bağımlılık gerektirmez. Tek yeni parça "tutunuldu" bildirimi.

### 6.2 Kaynak konumu ve yön

- **Kaynak:** 5 organ ucu (anterler). Konum = çiçek merkezi + `R · 1.44` yarıçaplı halkada, tepeden 36° kayık, 72° aralıklı 5 nokta (`FlowerCenter.shader:20, 109`; `R` = disk yarıçapı, kule `orientationDeg` iptal ettiği için tepe hep yukarı: `FlowerBloom`'daki `_TowerAngle`). Her patlamada parçacıkların ağırlığı, **oyuncuya dönük 2-3 organdan** gelir (tutunma noktasına en yakın organlar). 3 beyaz stigma noktası polen kaynağı **değil** (dişi organ); yalnızca isteğe bağlı küçük "parlama" noktası olabilir.
- **Yön:** organ ucundan dışa (radyal) + oyuncuya doğru ek eğilim (`towardPlayerBias` ≈ 0.4-0.6 [Başlangıç tahmini]) + oyuncunun tutunma hızının küçük bir payı (`inheritX/Y` kalıbı: 0.15-0.25, üst sınır 3-4 birim/sn). Koni ±70° [Başlangıç tahmini].
- **Güç:** `AcidImpactMath.Strength(speed, 4, 40, 0.35)` ile aynı eğri (hız arttıkça sayı ve hız artar); tutunma hızı düşükken taban güç.
- **Kafayı örtmeme:** parçacıklar oyuncunun arkasında (Player order −1), ve oyuncuya doğru yön ilk 0.15 sn ile sınırlı; sonra drift.

### 6.3 Ses ve titreşim

- **Ses:** `AudioCueSO` + `AudioCuePlayEventChannelSO` (kalıp: `AcidReaction.cs:106-113`). Cue: kısa (80-150 ms), yumuşak "kâğıt hışırtısı + hafif tık" [Başlangıç tahmini]; kuvvete göre ses ±%25, perde hafif rastgele (aynı sesin 8 çiçekte tekrarını azaltır). Polen sesi, hook'un "çarpma" sesinin **üstüne binmeli, onu örtmemeli**: patlama ile aynı kare (t=+0.05 sn).
- **Titreşim:** SO içinde `RumbleProfile` (`AcidReactionSettingsSO` gibi aynı SO'da ya da `FlowerVibrationSettingsSO` sistem başına). Hafif ve kısa: ~0.06 sn, sağ motor tepe 0.2-0.3 → 0 [Başlangıç tahmini]. **Dikkat:** `HapticManager.Vibrate` çalanı keser (`HapticManager.cs:56-65`): dash/slingshot titreşimi çiçek titreşimini ve çiçek, dash'inkini keser; çiçek titreşimi daha zayıf ve çok kısa kalmalı.
- **Ekran sarsıntısı:** önerilmez (8 çiçek, sık tutunma; ölüm `shakeForce 15` ile en yüksek ses). Hit-stop: tutunma için **istenmez**; mevcut hit-stop yalnızca dash/slingshot/ölümde.

## 7. Somut tasarım önerisi (hepsi [Başlangıç tahmini])

### Seçenek A: "Delgeç Konfetisi" (tek patlama + kısa drift) **[Önerilen]**

Fikir: yaprak sarsılınca organ uçlarından birkaç düz renkli kâğıt konfeti ve nokta dökülür, birkaç saniyelik süzülüşle kaybolur. Çok sade, 18 parçacık, 8 çiçekte bile gürültü yapmaz.

| Özellik | Değer |
|---|---|
| Parçacık sayısı (tam güç; taban güçte ~10) | 18: 9 nokta + 6 elips + 3 kâğıt parçası |
| Boyut (birim; ≈CRT px @ortho 8) | nokta 0.14-0.20 (3-4.5 px); elips 0.22-0.30 (5-7 px, uzun eksen); parça 0.32-0.45 (7-10 px) |
| Ömür | 1.2-1.9 sn (parçalar uzun) |
| Başlangıç hızı | nokta 3.5-6 b/sn; elips 2.5-4.5; parça 2-3.5 |
| Yerçekimi / drag | gravity 0.06-0.12 × g (parça 0.12-0.18); Limit Velocity drag 1.6-2.2 (parçada Multiply by Size açık) |
| Şekil | pürüzsüz yuvarlak nokta, hafif düzensiz elips, yuvarlatılmış kâğıt parçası (3 el kesimi varyant) |
| Renk dağılımı | krem %40, beyaz %15, lavanta %10, indigo %25, kobalt %10; parçalar krem+indigo hilal (sağ-alt, çiçek gölgesi yönünde), küçük taneler koyu ağırlıklı |
| Kaynak konumu | 5 organ ucu; oyuncuya dönük 2-3 organdan %70 |
| Ek efekt | yaprak sarsıntısı (Jolt) 0.05 sn önce; snap'te "püf" Aşama 2 |
| Damlama | yok (ya da ilk 0.6 sn'de 2-4 parçacık, isteğe bağlı) |

### Seçenek B: "Nefesli Polen" (patlama + hold damlaması + snap püfü + uzak katman)

Fikir: A'ya ek olarak tutunma sürerken zayıf akış, kapanırken (Snap) dışa püf ve çiçeğin arkasında ikinci, çok küçük "uzak" katman. Daha canlı ama 2 PS, çok daha fazla ayar ve gürültü riski.

| Özellik | Değer |
|---|---|
| Patlama | A ile aynı ama 14 parçacık |
| Tutunma damlaması | 3-5 parçacık/sn (hold süresi boyunca, ilk 1.5 sn sonra 1/sn'ye düşer); en fazla 12 canlı |
| Snap püfü | 8-10 parçacık, hız 5-8 b/sn, tüm halka (açı geniş), ömür 0.7-1.0 sn |
| Uzak katman (2. PS) | 6-8 çok küçük (0.12-0.16 birim, indigo), hız 0.5-1.2 b/sn, ömür 2-3 sn, **çiçeğin arkasında** (`Default` altı), yaprakların arasından görünür |
| Maliyet | çiçek başına 2 PS; `maxParticles` 48 + 16; toplam tavan 8 çiçekte ≈ 512 |
| Risk | görsel gürültü, "açık faz" polenle karışırsa tutulabilirlik sinyali bulanır; uyarıdaki sadelik bozulabilir |

### Öneri

**A ile başla.** B'den yalnızca iki şey seçmeli olarak eklenebilir: (1) snap püfü (çiçeğin en güçlü anı), (2) uzak katman (derinlik). Hold damlaması ilk sürümde yok: asılı kalma süresi kısa ve sürekli efekt uyarı kanalını sulandırır.

### Unity Particle System modül tablosu (A, çiçek başına tek PS, kod ile `Emit`)

| Modül | Ayar | Gerekçe |
|---|---|---|
| Main | Looping açık + Emission kapalı (`FX_AcidImpact` gibi) ya da Looping kapalı; Play On Awake açık; **Simulation Space World**; **Scaling Mode Local**; Max Particles 48; Gravity Modifier 0.08 (başlangıç; 2D gravity kaynağı seçilebilir, Kaynak 10); Start Lifetime/Size/Speed `EmitParams` ile (kodda); Culling Mode Automatic; Stop Action None | Kaynak 8, 9; `HalftoneTrailEmitter.cs:49-56` |
| Emission | Kapalı (Rate 0, burst yok); doğum `Emit(EmitParams, 1)` döngüsüyle | Boyut sınıfı/yön/kaynak kod kontrolünde (Kaynak 1 yerine kod, çünkü Burst parçacık başına boyut/yön veremez) |
| Shape | Kapalı (kod konum verir) ya da 2D Circle radius 0 | Kaynak 10 |
| Velocity over Lifetime | Kapalı | hız `EmitParams`'ta |
| Limit Velocity over Lifetime | Açık, **Drag** 1.6-2.2, Multiply by Size açık, Dampen 0 | Kaynak 3; kare hızından bağımsız (HalftoneTrail'in `dampen` yaklaşımına alternatif) |
| Force over Lifetime | Kapalı ya da çok zayıf sabit rüzgâr (x 0.05-0.15 b/sn²) | Kaynak 4 |
| Noise | Açık, Strength 0.25-0.4, Frequency 0.35-0.6, Scroll Speed 0.3, **Octaves 1, Quality Low**, Damping açık, Position Amount 1 | "süzülme" hissi; Kaynak 2, 14 |
| Rotation over Lifetime | ±60-160°/sn rastgele (yalnızca parça ve elips) | kâğıt dönüşü |
| Size over Lifetime | `HalftoneTrailEmitter` eğrisi (0→1, 0.4→1, 0.75→0.5, 1→0.5) ya da hızlı ilk büyüme 0→1 (ilk %6) + son %25'te 0.5 | Kaynak 9; `HalftoneTrailEmitter.cs:62-65` |
| Color over Lifetime | Alfa: 1 → 0.75'te 1 → 1'de 0 | `HalftoneTrailEmitter.cs:66-72` |
| Texture Sheet Animation | **Sprites** modu, 9 sprite (3 şekil × 3 varyant), `Start Frame` rastgele, Frame over Time sabit (animasyon yok) | Kaynak 6, 9 |
| Sub Emitters / Trails / Collision | Kapalı | maliyet (Kaynak 14) |
| Renderer | Billboard; materyal `Sprite-Unlit` tabanlı alfa karışım; Sorting Layer **Player**, Order **−1**; Sort Mode Oldest in Front; Min/Max Particle Size: ilk sürümde varsayılan | Kaynak 7; `hazards.md:57` |
| Script | `FlowerPollen` (yeni): `EmitParams` ile boyut/hız/renk/konum; strength, minDiameter `ortho/60`; `IResettable`; HitStop abonelikleri | §1, §3.4, §5 |

### Yapraklara/şeritlere "tepki" ve polenin buna bağlanması

Hedef: "yapraklar sarsılınca polen dökülsün." `FlowerBloom` zaten yay sürücüsü (`_spring`, `_pop`, titreme: `trembleAmplitude/trembleHz`, `FlowerBloom.cs:99-135`). Bağlama yolları, kolaydan zora:

1. **Sabit gecikme (ilk sürüm):** `Jolt()` (yeni) yaprak açıklık yayına kısa bir negatif itki + overshoot verir; polen 0.05 sn sonra patlar. Uyum "zamanlama" ile sağlanır; kod basit.
2. **Yay hızına bağlı emisyon (Aşama 2):** her karede `abs(spring.Velocity)` biriktirilir (`HalftoneTrailMath.Accumulate`, `HalftoneTrailLogic.cs:52-58`: mesafe yerine yay hızı) ve eşiği geçen her tam sayı bir parçacık olur; böylece sarsıntı güçlüyken çok, sönerken az polen çıkar ("sarsıntı ile orantılı döküntü"). Kaynak konumu: organ uçları (bilinen) ya da yaprak uçları (shader bükmesi CPU'da bilinmiyor; `pivot + dönmüş yön × yaprak boyu × 0.85` yaklaşımı gerekir, görsel örtüşme testi ister).
3. **İplik tacı tepkisi:** halka mesh'i (`FlowerCoronaMesh`) shader'da büzülüp açılıyor; "polen şeritlerden dökülsün" için kaynak ağırlığını iç halka (kobalt) üzerine kaydır ve renk dağılımında kobalt payını artır [Çıkarım]. Mesh/shader tarafına yeni kod gerekmez, yalnızca kaynak yarıçapı ayarı.

Bu üç yoldan yalnızca 1. ilk sürüme girer; 2 ve 3 gamepad testinden sonra "tepki yeterince bağlı mı" sorusuna göre eklenir.

## 8. Önerilen task listesi (1-1.5 saatlik, bağımlılık sırasıyla)

Format `TASKS.md` ile uyumlu; due tarihi geliştiricinin kararı (burada verilmedi). `TASKS.md`'ye eklemeden önce kullanıcıya sorulmalı.

1. **Polen ayar SO + saf hesap + EditMode test** `#code`: `FlowerPollenSettingsSO` (sayı, boyut, hız, drag, renk dağılımı, koni, `inherit`, `towardPlayerBias`, rumble + cue alanları; `AcidReactionSettingsSO` deseni, `OnValidate` ile sıralama) ve `FlowerPollenMath` (güç eğrisi, sayı dağılımı, `ortho/60` alt boyut, 5 organ kaynak noktası, koni hız). Saf mantık için test (CLAUDE.md kural 3).
2. **Polen prefab ve partikül kurulumu** `#art`: `FX_FlowerPollen` (9 sprite atlası: nokta/elips/parça × 3 varyant, krem+indigo hilal), `Sprite-Unlit` materyal, §7 modül tablosu, Player katmanı order −1. `AcidReaction.DebugImpact` benzeri `DebugBurst(age)` (Play Mode, `Time.timeScale 0` ile deterministik kare) + CRT açıkken ortho 8 ve 22 ekran görüntüsü.
3. **Tetikleme** `#code`: `IGrabReactive` küçük arayüzü; `PlayerController.AttachAnchor`'dan çağrı; `FlowerPollen` bileşeni (lazy çocuk PS, 0.05 sn gecikme, Warning/Closing'de durma); `FlowerBloom.Jolt` basit sürüm; hit-stop/pause kapısı. (Snap püfü ve yay hızlı emisyon bu task'ta yok.)
4. **Sıfırlama, tavan, 8 çiçek testi** `#code` + `#test`: `IResettable` (`LevelResetManager`), `OnEnable`'da `Clear`, `maxParticles` 48 + sayı kırpma, ses/titreşim için ortak ~0.1 sn bekleme; `FlowerTest` odasında `Cicek-Zincir-1..4` + çift kanca ile 8 çiçek yük testi (profiler: PS sayısı, batches).
5. **Ses ve titreşim** `#code`: `AudioCueSO` + kanal (kısa hışırtı), `RumbleProfile` (0.06 sn hafif), SO'ya bağlama; `HapticManager` kesme etkileşimi notu (dash ile çakışma).
6. **Gamepad ve CRT okunurluk testi** `#test`: oyuncu gamepad ile tutunur; ortho 8/10/22 odalarda polen okunuyor mu; gökyüzü + ip tacı üstünde; karakter pembesiyle karışma; flaşsız; sayıları SO'dan ayarla, eski/yeni değerleri bu dosyanın §7'sine not et (CLAUDE.md kural 4).

**Bu listenin dışında (sonraki karar):** snap püfü, yay hızına bağlı emisyon, uzak katman, iplik tacı kaynağı. Yaprak-polen tepkisi ancak ilk sürüm hissedildikten sonra derinleştirilmeli.

## 9. Doğrulanamayanlar

- **Ori, Hollow Knight, Hades, Spiritfarer VFX analizleri:** arama yaptım, ama geliştirici kaynaklı bir polen/toz/parçacık dökümü sayfa içeriğiyle doğrulanamadı. Hades "effects" 80.lv sayfası açıldı: yalnızca efektleri ve sanatçıyı (Josh Barnett) adlandırıyor, teknik içerik (parçacık, sprite/flipbook, okunurluk) yok. PC Gamer Hollow Knight sanat yazısı iki sürümde de **kesik** geldi; arama özetindeki "Hollow Knight'ta parçacıklar rastgele ve serbestçe döner" iddiası bu yüzden kaynak olarak sayılmadı. Hollow Knight/Unity "2D toolkit + Unity Particle System" bilgisi yalnızca arama özetinden; kaynak sayfası açılmadı.
- **Ghibli ve Pixar toz/polen hareketi:** arama sonuçlarında doğrudan "toz zerreleri nasıl çizilir" anlatan okunabilir bir kaynak çıkmadı; Pixar SIGGRAPH PDF'i ve Ghibli/Miyazaki CG sayfası açılmadı/doğrulanmadı. Bu yüzden "Ghibli dokusu" argümanı kullanılmadı.
- **Riot VFX Style Guide:** PDF bağlantısı ana sayfaya yönlendirdi, okunamadı. Okunurluk ve değer aralığı için içeriği alıntılanmadı.
- **GDC "Juice It or Lose It" ve Vlambeer "Art of Screenshake":** yalnızca GDC Vault oturum açıklaması ve haber özeti okundu; konuşma içeriği (parçacık ve sarsıntı sayıları) doğrulanamadı. `gamejuice.pages.dev` rehberi "yakında" sayfasıydı. Sarsıntı/hitstop sayıları yalnızca zayıf bir blogdan (Kaynak 20), mertebe olarak.
- **Hit-stop sırasında partikül doğum zamanı:** polenin hit-stop'ın ilk karesinde mi sonunda mı doğması gerektiğini söyleyen okunabilir kaynak bulunamadı. Öneri (donarken çıkarma, bitince oynat) benim çıkarımım; tutunma zaten hit-stop tetiklemediği için kritik değil.
- **Unity "Size over Lifetime Separate Axes ile kâğıt çevirme" ve `Min Particle Size` hangi eksen:** sayfa okunmadı / belirtmiyor. Kâğıt çevirme ilk sürüme dahil edilmedi.
- **`Sprite-Unlit` partiküllerin ZTest/ZWrite etkileşimi ve ip/kanca görsellerinin sorting order'ı:** depo içinde okunmadı; Player katmanı order −1 yerleşimi sahnede test edilmeli.
- **Çiçeğin gerçek organ konumu ile CPU hesabı:** shader yorumuna (`FlowerCenter.shader:109`) güvenildi; Unity'de görsel karşılaştırma yapılmadı (Editor bağlı değildi).
- **Gerçek frame time/draw call etkisi:** ölçülmedi; "8 çiçek × 48" tavanı hesaptan, profil sonucu değil.

## 10. Yöntem notu

- **Depo (salt okunur):** `LiquidLightBurst.cs`, `FlashLimiter.cs`, `AcidReaction.cs`, `AcidReactionSettingsSO.cs`, `DeathBurst.cs`, `DeathBurstSettingsSO.cs`, `HalftoneTrailEmitter.cs`, `HalftoneTrailLogic.cs` (Accumulate/TrailBudget), `LaunchPad.cs`, `HapticManager.cs`, `RumbleProfile.cs`, `PlatformVibrationSettingsSO.cs`, `HitStopManager.cs`, `IFramePausable.cs`, `PlayerController.cs` (`AttachAnchor`, `NotifyImpact`), `FlowerCycle.cs`, `FlowerCycleMath.cs`, `FlowerBloom.cs`, `FlowerCenter.shader`, `FlowerShadow.cginc`, `Mat_FlowerPetalShadow.mat`, `FX_AcidImpact/FX_LandingDust/FX_AcidSplash.prefab` (ana modüller `grep/awk` ile), `TagManager.asset`, `docs/systems/{hazards,death-respawn,grapple,camera}.md`, `docs/ui-research/flower-node/oneri.md`. Kodda hiçbir değişiklik yapılmadı; yazılan tek dosya bu.
- **Web:** WebSearch ile aday arandı; yalnızca **WebFetch ile açılıp içeriği okunan** sayfalar §2 tablosuna girdi. Unity sayfaları `docs.unity3d.com` → `docs.unity.com` yönlendirmesiyle okundu; Unity 6.0 ve "current" (6.6 etiketli) sürüm karışık, modül davranışları bu sayfalar arasında tutarlıydı. Özetleme aracı sayfa içeriğini kısaltarak verdi; tırnak/uzun metin kopyalanmadı.
- **Hesaplar:** kontrast oranları WCAG göreli parlaklık formülüyle, OKLCH değerleri standart Oklab dönüşümüyle `python -I` ile hesaplandı (§3.2). CRT px/birim = `360 / (2·ortho)`; tamponun 480×360 olması `death-respawn.md:30` ve `hazards.md:57`'ye dayanıyor.
- **Sınırlar:** hiçbir görsele Unity'de bakılmadı (Editor bağlı değildi); renk ve boyutlar mockup düzeyinde; his ve süre sayıları başlangıç tahmini, geliştirici gamepad testiyle karar verir. Projede tüketilmiş çizgiler (Bauhaus, op-art, halftone, genel folk) öneri olarak sunulmadı; `HalftoneTrailEmitter`'dan yalnızca **teknik** kalıplar (eğriler, birikim fonksiyonu) önerildi, görsel dili önerilmedi.
