# Asit temas tepkisi: araştırma ve uygulama planı (v2)

> **Uygulayan için:** Task task uygulanır (`superpowers:executing-plans` ya da `superpowers:subagent-driven-development`). Adımlar `- [ ]` ile işaretlenir. "Açık kararlar" (§5) kullanıcı onaylamadan uygulanmaz; buradaki ad ve değerler bağlayıcıdır.
>
> **v2 (2026-11-18):** v1 taslağı beş paralel inceleme ile yeniden yazıldı: kod/asset taraması, shader tekniği, oyun referansları, parçacık/ses/titreşim, plan eleştirisi. v1'deki yanlış varsayımlar §2'de listelendi.

**Hedef:** Oyuncu asite değdiğinde asit **temas noktasında, ilk 0.2 sn içinde** görünür tepki versin. Shader'da parlama + çukur, parçacıkta damla tacı. İkinci aşamada dalga, kaynama ve buhar. Asit oyuncu yeniden doğmadan sakinleşmiş olsun.

**Mimari özeti:**
- `DeathZone` iki C# olayı yayınlar: `Kill()`'den önce `Killing`, sonra `Killed`.
- Asit objesindeki `AcidReaction` olayları dinler ve temas noktasını asidin piksel ızgarasına çevirir.
- Tek darbe yuvasını (`_AcidImpact`) `MaterialPropertyBlock` ile shader'a yazar. **Darbenin yaşını her kare kendisi sayar**; shader'ın `_Time`'ına güvenmez.
- Damlaları `ParticleSystem.Emit(EmitParams)` ile basar.
- Saf hesaplar `AcidImpactMath`'te ve EditMode'da test edilir. Sayısal ayarlar `AcidReactionSettingsSO`'da, görsel ayarlar shader varsayılanlarında ve materyalde.

---

## 1. Gerçek durum (kod ve asset taraması)

### Asit örnekleri

Hepsi oda varyantlarında, `DynamicContent` altında. Kimse döndürülmemiş, çevrilmemiş, sorting ya da materyal override'ı yok.

| Oda | Obje | Ölçek | `_PixelScale` (X×Y) |
|---|---|---|---|
| 2 | Acid | 3×4 | 48×64 |
| 2 | **FlowingAcid** | 0.8×7 | **6.4×56** (`referencePPU` **8**) |
| 3 | Acid (2), Acid (1) | 10×4, 25×4 | 160×64, 400×64 |
| 5 | Acid (1) | **75×10** | 1200×160 (SwingTutorial burada slow-motion açar) |
| 12 | Acid | **135×4** | 2160×64 |
| 18 | Acid (1) | 41.1×4 | 657.6×64 (kesirli) |
| 22 | Acid | 29.82×**2** | 477×32 (kesirli, en alçak) |

### Prefab'lar

**`Acid.prefab`**
- Katman 8, sorting Default/0.
- Sprite Unity'nin varsayılan `Square`'i: FullRect, pivot ortada, UV 0–1.
- BoxCollider2D trigger: offset (0, −0.05), size (1, 0.89). Yerel üst sınır 0.395, yani UV 0.895.
- Child'lar: `FX_AcidBubble` ve pasif `FX_AcidFog`. `FX_AcidSplash` **yok**.

**`FlowingAcid.prefab`**
- `referencePPU` 8, collider 1×1.
- Child'lar: `FX_AcidFog` ve `FX_AcidSplash` (sürekli, Stretched).

### Materyal değerleri

`Mat_PixelArtAcid`: `_AcidLevel` **0.9**, `_FPS` **8**, `_BubbleThreshold` **0.9**, `_WaveAmplitude` 2, `PixelSnap` açık. Görünen yüzey (0.9 ± 2 px) collider üstüyle örtüşüyor.

### Ölüm akışı

- `DeathZone` `TryGetComponent(IKillable)` çağırır, koruma yok.
- `PlayerHealth.Kill` sırası: Death titreşimi → `FX_PlayerDeath` → `SetActive(false)` → `OnDeath`.
  - `OnDeath` içinde `RespawnManager` senkron olarak `OnStartRespawn` çalıştırır ve **`Velocity` sıfırlanır**. Hız bu yüzden `Kill()`'den önce okunmalı.
- `Kill()`'de `isDead` koruması **yok**. Her odada diken Tilemap'i var, asitle üst üste gelebilir; iki `Kill` iki respawn dizisi başlatabilir. Bu mevcut bir hata.

### İris (en önemli kısıt)

- `TransitionManager` yarıçapı `maxRadius` (2) değerinden **−1**'e lerp ediyor. Görünen daire ≈**0.33 sn**'de kapanıyor.
- t=0.25'te yarıçap ≈6.4 birim, t=0.3'te ≈2.6 birim.
- **Tepkinin görünür penceresi ≈0.3 sn ve daralıyor.**
- İris merkezi her kare oyuncunun ölüm konumu. İris UI katmanında, VFX'in üstünde. CRT bütün ekrana, iris dahil, uygulanıyor.

### Zaman

- URP 6000.3'te shader `_Time.y` = Play'de `Time.time`, Edit'te `realtimeSinceStartup`. Sahne yüklemede sıfırlanmıyor.
- Ana menüden gelindiğinde `Time.timeSinceLevelLoad` ile uyuşmaz.
- Pause `timeScale` 0 yapar, ama ölüm dizisinde pause zaten engelli.
- `HitStopManager` `timeScale`'e dokunmaz; oyuncuyu durdurur ve `Velocity`'yi 0'a çeker (önbelleğe alır).

### Oyuncu

- Tek collider kökte: BoxCollider2D 1×1, katman 9. Kolların ve iplerin collider'ı yok, asite giremezler.
- `Velocity` çözülmüş (resolved) hız. Trigger fizik adımından **sonra** gelir, yani oyuncu merkezi çoğu zaman zaten asidin içindedir.
- Adım başına 0.64 birim (terminal −32) ile 1.3 birim (slingshot 65) yol alır.

### Multiplayer

- Rakip `GhostView`: collider yok, öldüğünde sadece gizleniyor. `Ghost.prefab` ise dash afterimage, rakiple ilgisi yok.

### Odalar

- `Room.Sleep` `DynamicContent`'i kapatır, asit ve parçacıkları da kapanır.

### Parçacık

- `FX_AcidSplash` materyali `New Material 1.mat`: **Sprite-Lit-Default**, doku `Acid-Drop.png`. Doku 16×16, gölgeli, yuvarlak, teal paletli bir damla; kare piksel değil.
- Sahnede Light2D yok, yani Lit bugün tam parlak çiziyor, ama ileride ışık eklenirse kararır.
- `FX_PlayerDeath` sorting Default/0–1, asitle aynı. Çizim sırası belirsiz.
- VFX Graph kurulu değil.

### CRT

- Ekranı 480×360'a indiriyor. Ortho 6.4'te 1 oyun pikseli (1/16 birim) ≈1.76 CRT pikseli.
- Kamera odaya göre uzaklaşabiliyor (oda 16'da ortho 22), bu yüzden **1 px damlalar titreyip kaybolur**.

### Ses ve titreşim

- Ölüm ya da asit sesi yok.
- Ses kanalı konumlu çağrıda 3D çalıyor; kamera ≈10 birim uzakta olduğundan ses çok kısık çıkar. Boş cue'da kanal uyarı basıyor.
- `HapticManager.Vibrate` çalan tek seferlik titreşimi keser. `Kill()`'den önce verilen asit titreşimini Death profili hemen ezer.

### Diğer

- **Yeniden kullanılacak kalıplar:**
  - `HalftoneTrailEmitter`: World space, `scalingMode` Local, `playOnAwake` + `loop` açık + Emission kapalı + `Emit`; `OnEnable` ve `IResettable`'da `Clear()`.
  - `LaunchPad` ses çağrısı.
  - SO'lar `New-Scripts/<Feature>/SO/` altında. `Assets/Data` yok.
- **Testler:** EditMode'da saf hesap testleri var (`HazardCoreMathTests` benzeri). DeathZone ya da ölüm testi yok. `PlayerRuleTestBase` NewScene'i yüklüyor.

## 2. v1'de düzeltilen yanlışlar

| v1 varsayımı | Gerçek | v2'deki karşılığı |
|---|---|---|
| C# `timeSinceLevelLoad` yazar, shader `_Time.y` okur | URP'de iki saat sahne geçişinde ayrışır; ana menüden gelince tepki **hiç görünmez** | Yaşı CPU sayar, her kare `_AcidImpact.z = yaş` |
| 4 yuvalı `float4` dizisi + `PickSlot` | Bir asitte aynı anda iki canlı darbe olamaz (respawn 1.3 sn > tepki 1.0 sn). MPB dizileri boy kilitler | Tek `Vector4 _AcidImpact`, `[HideInInspector]` property |
| `ClosestPoint(playerPos)` temas noktası verir | Nokta collider içindeyse noktanın kendisini döndürür | Bir adım geriye sar; hâlâ içerideyse x = oyuncu x, y = yüzey satırı |
| Bazı asitler döndürülmüş | Hiçbiri | Gövde teması / kenar normali dalı çıkarıldı; `InverseTransformPoint` yine kullanılıyor (bedava) |
| İris 0.5 sn'de kapanır | Görünür daire ≈0.33 sn'de kapanır | Tepkinin tepe noktası ilk 0.15 sn; buhar v1'den çıktı; iris kararı §5 AK4 |
| Damla 1–2 px | CRT altında 1 px titrer ya da kaybolur | En az 2 px (0.125 birim) |
| `FX_AcidSplash` materyali yeniden kullanılır | Lit + yuvarlak teal damla | Yeni `M_FX_AcidPixel` (Sprite-Unlit-Default + 4×4 beyaz doku) |
| `playOnAwake` ve `looping` kapalı | Emit edilen parçacıkların simüle olması için sistem çalışıyor olmalı | Halftone kalıbı: ikisi de açık, Emission kapalı |
| Darbe yokken çıktı aynı | `e^(−kt)` sıfıra inmez; kesirli kalıntı piksel çevirebilir | Shader'da sert kesme: `yaş < süre && güç > 0` değilse ofset tam 0; C# süre dolunca yuvayı sıfırlar |
| Bağlam menüsüyle editörde önizleme | Edit Mode'da zaman ve parçacık akmaz | Önizleme yalnızca Play Mode; doğrulama donmuş zamanla (§6) |
| Akan asitte 6 px parlama, 12 px kaynama | Akan asit 6.4 px genişliğinde, 1 px = 1/8 birim | Akan asidin değerleri kendi materyalinde ayrı (§3) |
| Ses konumlu çalınır | 3D, neredeyse duyulmaz | 2D overload; cue ve klip kontrolü AcidReaction'da |
| Asit titreşimi `Killing`'de | Death profili hemen ezer | `Killed` olayında, `Kill()`'den sonra |
| SO `Assets/Data/` altında | Klasör yok | `New-Scripts/Death/SO/` |
| `_AcidLevel` 0.85, `_FPS` 6 | 0.9 ve 8 | Hesaplar gerçek değerlerle yapıldı |
| `Ghost.prefab` rakip | Dash afterimage | Rakip `GhostView`; ölüm tepkisi kapsam dışı |

## 3. Araştırma bulguları

### 3.1 Oyun referansları

**Celeste (decompile edilmiş kaynaktan ölçülen değerler)**
- Ölüm anı: girdi kilitlenir, ekran sallanır, hafif titreşim.
- Hemen ardından: **50 ms donma**, sprite **1.5×** büyür, sonra tehlikeden **uzağa** 24 px savrulur.
- 0.375 sn'de patlama: 8 daire, güçlü titreşim, ölüm sesi.
- Wipe ≈0.97 sn'de başlar; ölümden kontrole ≈2 sn.
- Ekran sallanması varsayılan ayarda **Half**; ayrıca `DisableFlashes` seçeneği var.

**Ders:** Celeste'nin ölüm vuruşunun çoğu okunurluğa harcanıyor. Önce "sen, burada" karesi, sonra "tehlike seni itti", sonra patlama. **Bizim en büyük eksiğimiz sıvının tepkisi değil, oyuncunun temas karesinde yok olması.** Bunun bir "sen buradasın" karesi yok. ([kaynak](https://github.com/BloodLantern/Celeste), [Player.cs](https://github.com/NoelFB/Celeste/blob/master/Source/Player/Player.cs))

**Diğer oyunlar**
- **Super Meat Boy:** respawn ≈0 sn; kan izi kalıcı "neden öldüm" kaydı olarak kalıyor ([röportaj](https://www.gamedeveloper.com/game-platforms/-i-super-meat-boy-i-s-mcmillen-explains-why-so-hard-)).
- **Hollow Knight:** asit ölüm değil hasar; zamanlama modeli olarak uygun değil.
- **Noita / Terraria:** buhar = "sıcak ya da aşındırıcı" ([GDC](https://gdcvault.com/play/1025695/Exploring-the-Tech-and-Design)). Asidi sudan ayıran renksiz işaret buhar.
- **Dövüş oyunları** (Dead Cells, Katana Zero): 100 ms'yi aşan hit-stop ve tam ekran flaş kullanıyorlar. 10–20 saniyede bir gelen ölümler için yanlış.

**İlkeler**
- "Neden öldüm" ilk 100 ms içinde, temas noktasında ve tehlikenin kendi paletiyle cevaplanmalı.
- Sık gelen olayda geri bildirim maksimuma çıkarılmamalı.
- Her ölümde küçük rastgelelik olmalı (damla açıları, boyutlar).
- Tam ekran flaş kullanılmamalı. Saniyede 3'ten fazla flaş ekranın %25'inden büyük alanda olmamalı ([XAG 118](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/118)). Bizim yerel, tek flaşımız güvenli.
- Yakınlığa tepki veren *öldürücü* sıvı için emsal bulunamadı. Eklenirse temastan açıkça farklı bir dille yapılmalı: flaş, çukur ve buhar olmadan. ([juice rehberi](https://uhiyama-lab.com/en/notes/unity/unity-game-feel-hit-feedback/), [Art of Screenshake](https://www.youtube.com/watch?v=AJdEqssNZ-U))

**Piksel sıçrama kalıbı**
- Fazlar: yüksel → tepe → damlalar kopar → geri düş → halka.
- Taç şekli: ortada iki uzun sütun, yanlarda küçük damlalar.
- 10–12 FPS'te 4–6 kare; asidin kendi rampasından 3–4 renk.

### 3.2 Shader tekniği

- **Yöntem:** Yay sütunları ya da CPU yükseklik dizisi, kareler arası durum taşımak içindir. Bizde asit başına tek darbe var, o yüzden **analitik, sönümlü yürüyen dalga** yeterli ([Prime31](https://prime31.github.io/water2d-part1), [Tuts+](https://code.tutsplus.com/make-a-splash-with-dynamic-2d-water-effects--gamedev-236t)).
- **Piksel ızgarası:** Mevcut `pixelCoords.y > surfaceHeight` karşılaştırması tam sayı satırı kesirli yükseklikle kıyaslıyor. `px` tam sayı olduğu sürece ek ofset ızgarada kalır.
- **Batching:** Bu `CGPROGRAM` shader'lar URP 2D'de unlit olarak çalışıyor. `UnityPerMaterial` CBUFFER'ları olmadığından zaten SRP Batcher dışındalar, AcidHazard'ın MPB'si de öyle. **Yeni bir maliyet yok, dönüştürmeye gerek yok** ([SRP Batcher](https://docs.unity3d.com/Manual/SRPBatcher-Incompatible.html)).
- **Dizi kullanmama nedeni:** MPB dizileri ilk yazımda boy kilitler ([SetVectorArray](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/materialpropertyblock/setvectorarray)). `UnityPerMaterial` içindeki diziler de SRP Batcher'ı bozar. Tek adlı `Vector4` kullanılıyor.
- **Zaman:** URP `_Time` sahne yüklemede sıfırlanmıyor ([issue](https://issuetracker.unity3d.com/issues/global-shader-variables-time-and-timeparameters-do-not-reset-when-a-scene-is-loaded-in-urp)). Yaşı CPU sayıyor.
- **Tepe payı (headroom):** `pixelScale.y × (1 − _AcidLevel) − _WaveAmplitude − 1`.
  - Oda 2/3/12/18'de: 6.4 − 2 − 1 = 3.4 px.
  - Oda 22'de: 3.2 − 3 = 0.2 px.
  - Dalga tepesi `min(ofset, payı)` ile kırpılır, çukur kırpılmaz. Alçak asitte dalga tepesi düz kalır, yükselen kısmı damlalar taşır.
- **Maliyet:** Boş yuvada tek uniform dal, dolu yuvada ≈3 `exp` + 1 `sin`. Masaüstünde ihmal edilir. `#pragma target 3.0`.
- **Ters çevirme:** Shader'lar `_Flip` uygulamıyor. Eşlemede flip hesaba katılmaz; `flipX/Y` açıksa `OnValidate` uyarır.

### 3.3 Parçacık

- **Kurulum:** Shuriken + `Emit(EmitParams)`. Hız oyuncunun hızına bağlı olduğu için "konumda Play" kullanılmıyor. VFX Graph 2D'de sprite'larla parçacık başına sıralanamıyor ve yeni bir paket demek; hayır ([2D VFX uyumu](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@15.0/manual/2d-visual-effect-graph-compatibility.html)).
- **Materyal:** URP Particles/* shader'ları 2D Renderer'da desteklenmiyor. **Sprite-Unlit-Default** + 4×4 beyaz doku (point, clamp, mip yok), renk parçacık başına verilir.
- **Renk ve kare dili:** Color over Lifetime **Fixed** (kademeli) gradyan; alfa 0.85'e kadar 1, sonra 0 (yarı saydam piksel yok). Kare damlaya kontur sığmaz.
- **Geri düşen damlalar:** Asit trigger olduğu için World collision çalışmaz; Collision **Planes** + **Collision** tipi sub-emitter gerekir (burst). v1'de kapalı ([Sub Emitters](https://docs-alpha.unity3d.com/2020.2/Documentation/Manual/PartSysSubEmitModule.html)).

### 3.4 Ses ve titreşim

**Ses katmanları**
- Sıçrama: 0.25–0.4 sn, 3 varyasyon, pitch 0.92–1.08.
- Cızırtı kuyruğu: 0.8–1.1 sn, sıçramanın 6 dB altında.
- İsteğe bağlı fokurtu.
- Güçle ölçeklenir: ses `lerp(0.75, 1)`, pitch `lerp(1.05, 0.95)`.
- 2D çalınır. Mono WAV 48 kHz, SFX mixer grubu.
- CC0 kaynaklar: Freesound (CC0 filtresiyle "sizzle", "fizz", "slime splash"), [OpenGameArt 6 short water splashes](https://opengameart.org/content/6-short-water-splashes), jsfxr/ChipTone. Lisans klip sayfasında doğrulanmalı.
- Proje genelinde bir "ölüm vuruşu" sesi de eksik.

**Titreşim (başlangıç değerleri, his kullanıcının kararı)**
- `AcidDeath` profili, 0.35 sn.
- Sağ (yüksek frekans) motor: 0.9 → 0.15'te 0.3. Sıçramanın çıtırtısı.
- Sol (düşük frekans) motor: 0.6 → 0.3'te 0.5 → 1'de 0. Batma gürlemesi.
- Death profilinin yerine `Killed`'de verilir.

**Erişilebilirlik**
- Tepki parlaklık ve hareketle okunuyor, ton ile değil; kırmızı-yeşil renk körlüğünde de çalışır.
- Bugün titreşim, flaş ya da efekt seçeneği yok. Options işi gelince `HapticManager`'a bir çarpan ve global `_AcidFlashScale` eklenebilir.

## 4. Tepkinin anatomisi (başlangıç değerleri, his kullanıcının kararı)

t = 0 ölüm karesi. İris ≈0.33 sn'de kapanıyor, o yüzden her şey ilk 0.25 sn için tasarlandı.

| Zaman | Shader (A aşaması) | Parçacık (A aşaması) | B aşaması (testten sonra) |
|---|---|---|---|
| 0–0.1 sn | **Parlama:** temas çevresinde r = 6 px içinde köpük rengi `_FlashColor`'a döner (#F4FFE0), `_FlashTime` 0.1 sn | **Taç:** `round(lerp(6, 14, güç))` kare damla, 2 px (0.125 birim). Ortadakiler uzun, yanlar kısa. Hız 4–7 × güç | Oyuncu silüeti (AK1) |
| 0–0.35 sn | **Çukur:** yüzey temas sütununda 3 px çöker (genişlik 4 px), `e^(−12t)` ile toparlanır | 2–3 "büyük" damla, 3 px (0.1875 birim), hız 2.5–4. Yerçekimi çarpanı 2.5; yay irisin içinde okunur | Dalga: iki yana 40 px/sn, λ 10 px, genlik 3 px, `e^(−4t)` |
| 0–0.9 sn | — | Damla ömrü 0.35–0.6 sn | Kaynama (eşik 0.9 → 0.65, r 12 px), buhar 4–5 puf (yalnızca AK4 iris gecikmesi seçilirse) |
| ≥ 0.6 sn (A) / 1.0 sn (B) | Etki tam 0, yuva sıfırlanır | — | — |

**Kurallar**
- **Damla yönü:** yüzey normali etrafında ±35° koni; yatay hıza `0.3 × oyuncuVx`, düşey hıza `0.25 × |oyuncuVy|` eklenir. Büyüklük en fazla 9 birim/sn, yoksa damlalar iris dairesinden çıkar. Celeste'nin "tehlikeden uzağa savrulma"sının karşılığı.
- **Güç:** `Lerp(0.35, 1, InverseLerp(4, 40, |v|))`. Üst sınır 40, çünkü 30 sık sık doyuyor. Hit-stop sırasında ölümde hız 0 gelir ve taban güç kullanılır; kabul edildi.
- **Zaman ızgarası:** Darbe kendi `_ImpactFPS`'iyle (12) kesilir, ortam dalgası 8 FPS'te kalır (AK5).
- **Akan asit:** her temas gövde teması, yüzey yok. Parlama ve 1 px halka var, çukur yok. Değerleri kendi materyalinde ayrı: `_FlashRadius` 2, `_RingWidth` 1, damla sayısı yarı.
- **Rastgelelik:** açı ve boyut her ölümde biraz değişir.

## 5. Açık kararlar (uygulamadan önce kullanıcı onayı)

- **AK1. "Sen buradasın" karesi.** Öneri: oyuncu kaybolurken mevcut sprite'ın bir kopyası 0.06 sn `#C7FF70`, 1 kare `#59E047` dolgulu parlar, sonra sıçramanın içinde kaybolur. `PlayerHealth` değişmez; `GhostAfterimage` havuzu kalıbı kullanılır. Tüm tehlikelerde mi (genel ölüm imzası), yalnızca asitte mi? Öneri: tüm tehlikeler, ayrı task (B4). Daha pahalı alternatif: eriyerek batma (2–3 sa, `PaletteSwap` riski).
- **AK2. Hareketli tehlike kabuğu (`HazardAcid`).** Öneri: B aşaması, yerel uzayda açıya bağlı çukur + kabuk boyunca yayılan wobble. Shader taslağı shader incelemesinde hazır.
- **AK3. Yakınlık tepkisi.** Öneri: `Someday`. Eklenirse yalnızca hafif menisküs ya da fazladan kabarcık; flaş, çukur ve buhar olmadan.
- **AK4. Ölüm vuruşu.** Seçenekler:
  - (a) Hiçbir şey; tepki 0.3 sn'lik pencereye sığdırılır. A aşaması buna göre tasarlandı.
  - (b) İris başlangıcını 0.1–0.15 sn geciktirmek; ölümden kontrole ≈1.4 sn.
  - (c) 50 ms donma (Celeste değeri).

  Öneri: önce (a) ile gamepad testi, sonra karar.
- **AK5. Darbe 12 FPS, ortam 8 FPS.** Karışık ritim bir stil kararı. Öneri: 12 ile dene.
- **AK6. `FX_PlayerDeath`.** Asitle aynı sorting'de (Default) olduğu için çizim sırası belirsiz ve sıçramanın üstüne biniyor olabilir. Öneri: VFX katmanına taşı, A testinde bak; asit ölümünde gizlemek ölüm nedeni bilgisi ister, şimdilik hayır.
- **AK7. Çift `Kill` koruması.** `PlayerHealth.Kill` başına `if (!gameObject.activeSelf) return;`. Mevcut bir hata; oynanışa dokunduğu için onay ve PlayMode testi senden. Öneri: A aşamasıyla birlikte, ayrı commit (A0).
- **AK8. Ses dosyaları.** Kanca A'da kurulur, dosyalar ayrı `#art` işi. Genel bir ölüm vuruşu sesi de istiyor musun?

## 6. Doğrulama yöntemi (bütün task'larda aynı)

`unity command` her çağrıda yüzlerce ms sürüyor, o yüzden zamanlı kare yakalamak mümkün değil. **Zaman dondurulur, darbe geriye tarihlenir:**

```
unity command eval 'Time.timeScale = 0; var r = Object.FindObjectsByType<New_Scripts.Death.AcidReaction>(FindObjectsSortMode.None)[0]; r.DebugImpact(new Vector2(0f, 0.4f), 0.05f); return "ok";'
unity command capture_game_view --source screen --width 1920 --height 1080 --save_path AcidTest/a_005.png
```

- `DebugImpact(Vector2 localPoint, float age)` yuvayı `yaş = age` ile yazar.
- Parçacık için aynı çağrı damlaları basar ve `ps.Simulate(age, true, false)` çalıştırır.
- Zaman 0'dayken CPU sayacı ilerlemez, yani kare deterministiktir.
- **"Darbe yokken birebir aynı":**
  - Shader değişikliğinden önce donmuş zamanla A karesi alınır.
  - Shader değişir, yeniden içe aktarılır, B karesi alınır.
  - A ve B dosya hash'leri karşılaştırılır.
  - Ortam animasyonu `_Time`'a bağlı olduğundan iki karenin aynı `_Time`'da olması gerekir. Kamera, yalnızca asidin bulunduğu ve `_FPS` adımına denk gelen bir anda dondurulur. Hash tutmazsa piksel farkı yüzdesi raporlanır (yalnızca dalga bölgesinde fark beklenir).
- **Karelerin yeri:** `Assets/AcidTest/` altına yazılır, sonra repo dışına (`docs/ui-research/acid-reaction/`) taşınır; `Assets/AcidTest` ve `.meta`'ları silinir (CLAUDE.md).

---

## Genel kısıtlar

- **Branch ve commit:** `feature/acid-reaction`, `main`'den. Her task bir commit, push yok. İlgisiz değişikliklere (`M_CRTFilter.mat`, URP asset'leri, `NewScene.unity` …) dokunulmaz.
- **Kod yeri:** `Assets/Scripts/New-Scripts/Death/`, namespace `New_Scripts.Death`, yorumlar Türkçe. SO asset'leri `New-Scripts/Death/SO/`. Shader'lar `Assets/Shader/`. Doku `Assets/Sprites/VFX/`.
- **Sahne ve prefab:** yalnızca `unity command eval`.
- **Her task sonunda:** derleme temiz, EditMode yeşil, çıktı rapora yazılır. `DeathZone` ya da `PlayerHealth`'e dokunan task'larda PlayMode da koşulur.
- **Ölüm her zaman olur:**

  ```csharp
  try { Killing?.Invoke(other); }
  finally { killable.Kill(); }
  Killed?.Invoke(other);
  ```

  Dinleyici hata atsa da oyuncu ölür. `Killed` dinleyicisinin hatası ölümü etkilemez.
- **C# olayı mı, EventChannelSO mu:** C# olayı doğru seçim. Sinyal aynı objedeki bileşenler arasında, örnek başına, `Kill()`'den önce senkron çalışmalı ve `Collider2D` taşıyor. SO kanal her ölümü her asite yayınlar ve göndereni filtrelemeyi gerektirirdi. Bu gerekçe `hazards.md`'ye yazılır.
- **Shader:** `Properties`'e `[HideInInspector] _AcidImpact ("", Vector) = (0,0,0,0)` ve darbe ayarları eklenir. Include'a property bloğu konamadığından iki shader'da da tekrar edilir. **Shader varsayılanları** başlangıç değerleridir. Akan asidin farklı değerleri eval ile `Mat_FlowingPixelAsit`'e yazılır; `Mat_PixelArtAcid`'e yazılmaz.
- **MPB:** her yazımdan önce `GetPropertyBlock`. `sharedMaterial.SetX` ve `renderer.material` kullanılmaz. `_PixelScale` MPB'den okunmaz; `AcidHazard`'a `public Vector2 PixelScale` getter'ı eklenir ve hesap oradan gelir.
- **His değerleri** (§4) başlangıçtır; gamepad testinde kullanıcı ayarlar (kural 4).

---

## A aşaması: okunur ilk sürüm

### Task A0 (AK7 onaylanırsa): Çift `Kill` koruması — #code, 0.5 sa

**Dosyalar:** `Death/PlayerHealth.cs`, `Assets/Tests/PlayMode/` (yeni test, kullanıcı onaylar), `docs/bozulmamali-davranislar.md` (ölüm kuralı).
- `Kill()` başında `if (!gameObject.activeSelf) return;`.
- Test: aynı karede iki `Kill()` → tek `OnDeath`, tek respawn dizisi.
- **Kabul:** PlayMode ve EditMode yeşil, çıktı raporda.

### Task A1: Kanca ve saf hesap — #code, 1 sa

**Dosyalar:**
- Değiştir: `Death/DeathZone.cs` (≤8 satır), `Death/AcidHazard.cs` (`PixelScale` getter, ≤3 satır)
- Oluştur: `Death/AcidImpactMath.cs`
- Test: `Assets/Tests/EditMode/AcidImpactMathTests.cs`

**`DeathZone`:** `public event Action<Collider2D> Killing;` ve `public event Action<Collider2D> Killed;`, genel kısıtlardaki try/finally ile.

**`public static class AcidImpactMath`:**
- `Vector2 BacktrackPoint(Vector2 pos, Vector2 velocity, float dt)` → `pos − velocity·dt`.
- `Vector2 LocalToPixel(Vector2 local, Rect spriteLocalRect, Vector2 pixelScale)` → `(local − min) / size × pixelScale`, `[0.5, pixelScale − 0.5]` aralığına sıkıştırılır.
- `Vector2 SurfaceContact(Vector2 px, float acidLevel, float pixelScaleY, bool hasSurface)` → yüzeyli asitte `y = floor(acidLevel × pixelScaleY)`, yüzeysizde dokunmaz.
- `float Strength(float speed, float minSpeed, float maxSpeed, float floor)`.
- `float CrestHeadroom(float pixelScaleY, float acidLevel, float waveAmplitude)` → `max(0, pixelScaleY × (1 − level) − amp − 1)`.
- `Vector2 SplashVelocity(Vector2 normal, float coneDeg, float random01, float speed, float strength, Vector2 playerVel, float inheritX, float inheritY, float maxSpeed)`.
- `bool ImpactLive(float age, float duration, float strength)` → `age ≥ 0 && age < duration && strength > 0`.

**Testler:**
- Köşeler ve sıkıştırma; kesirli `pixelScale` (657.6).
- Geri sarma.
- Yüzey satırı (64 × 0.9 → 57).
- Güç tabanı ve tavanı.
- Tepe payı: oda 22 → 0.2, oda 5 → 13.
- Hız tavanı.
- `ImpactLive` sınırları (yaş = süre → false, negatif yaş → false).

**Kabul:** testler yeşil, PlayMode yeşil, `DeathZone` diff'i küçük, oyuncu yine ölüyor (PlayMode'daki mevcut `PenaltyFlowTests` ölüm testi geçiyor).

### Task A2: Shader — parlama ve çukur — #code, 1–1.25 sa

**Dosyalar:** Oluştur `Assets/Shader/AcidImpact.cginc`; değiştir `Assets/Shader/PixelArtAcid.shader`.

**`AcidImpact.cginc`:**
- Değişkenler: `float4 _AcidImpact` (x px, y px, yaş, güç); `_ImpactFPS` 12, `_ImpactDuration` 0.6, `_CraterDepth` 3, `_CraterWidth` 4, `_CraterRecover` 12, `_FlashColor` #F4FFE0, `_FlashRadius` 6, `_FlashTime` 0.1.
- Fonksiyonlar:
  - `AcidLive()`.
  - `AcidAge()`: `floor(yaş × _ImpactFPS) / _ImpactFPS`.
  - `AcidCrater(px)`: `−depth · e^(−dx²/w²) · e^(−recover·t) · güç`, `dx = |px + 0.5 − x|`.
  - `AcidFlash(p)`: `yaş ≤ _FlashTime` iken `step(length(p + 0.5 − xy), r)`.
- Canlı değilse fonksiyonlar **tam 0** döndürür.

**`PixelArtAcid`:**
- `#pragma target 3.0`, `#include "AcidImpact.cginc"`, property'ler eklenir.
- `surfaceHeight = baseAcidHeight + wave + min(AcidCrater(pixelCoords.x), headroom)`. Çukur negatif olduğundan `min` yalnızca B'deki dalga tepesi için anlam kazanır; satır şimdiden hazır.
- Köpük dalında `finalColor = lerp(_FoamColor, _FlashColor, AcidFlash(pixelCoords))`.
- Gövdede parlama yarıçapı içinde de flaş rengi (yüzeyin hemen altı): `AcidFlash > 0` ise `lerp(finalColor, _FlashColor, 0.6)`.

**Kabul:**
- Darbe yokken A/B karesi aynı (§6) ya da fark yok.
- `DebugImpact` henüz yok (A3), o yüzden eval ile geçici bir MPB yazılıp 0.05 ve 0.3 sn kareleri alınır. Çukur ve parlama görünür.
- Derleme temiz, EditMode yeşil.

### Task A3: `AcidReaction` bileşeni — #code, 1–1.25 sa

**Dosyalar:** Oluştur `Death/AcidReaction.cs`; `Acid.prefab`'a eval ile ekle.

**`AcidReaction`**
- `[RequireComponent(DeathZone, SpriteRenderer, Collider2D, AcidHazard)]`, `IResettable`.
- `[ExecuteAlways]` **değil**.

**Abonelik**
- `OnEnable`: `Killing` ve `Killed` aboneliği, `LevelResetManager` kaydı, yuvayı sıfırla.
- `OnDisable`: aboneliklerden ve kayıttan çık.

**`Killing(other)`**
1. `other.TryGetComponent(out PlayerController pc)`; `?.` kullanılmaz.
2. `pos = other.attachedRigidbody.position` ve `v = pc.Velocity`.
3. `prev = BacktrackPoint(pos, v, Time.fixedDeltaTime)` → `world = _collider.ClosestPoint(prev)`.
4. `world` hâlâ collider içindeyse (`_collider.OverlapPoint(world)`): x için `pos`, y için collider'ın üst sınırı kullanılır.
5. `local = transform.InverseTransformPoint(world)` → `LocalToPixel` (`sprite.rect`'ten yerel rect) → `SurfaceContact`.
6. `_impact = (px.x, px.y, 0, güç)` → MPB'ye yaz, `enabled = true` ile `Update` açılır. Temas verileri A5 için `_lastContact` alanında saklanır.

**`Killed(other)`:** A6'da doldurulur, şimdilik boş.

**`Update`:** `_impact.z += Time.deltaTime`. `!ImpactLive` olunca `_impact = 0`, MPB'ye yaz, `enabled = false`. Sabit asitlerde yalnızca ≈0.6 sn çalışır.

**`ResetToDefault`:** yuvayı sıfırla (iris kapalıyken, 0.5 sn).

**Debug**
- `public void DebugImpact(Vector2 localPoint, float age)`: yalnızca Play Mode.
- `[ContextMenu("Test Impact")]`: Play Mode'da ortanın üstüne darbe.

**`OnValidate`:** sprite `drawMode` Simple değilse ya da `flipX/Y` açıksa uyarı.

**Kabul**
- Donmuş zamanda `DebugImpact` ile 0.05, 0.3 ve 0.7 sn kareleri; 0.7 sn karesi darbesiz kareyle aynı.
- `AcidHazard.UpdateMaterialScale()` eval ile çağrıldıktan sonra `_AcidImpact` korunuyor (`GetPropertyBlock` ile okunur).
- Oda 5'te gerçek düşüşte çukur oyuncunun x'inde (ekran karesi).
- **Ana menüden oyuna geçiş sonrası tepki görünüyor** (v1'in P0 hatası).
- EditMode yeşil.

### Task A4: Damla materyali ve FX prefab'ı — #code + #art, 1–1.25 sa

**Dosyalar:** Oluştur `Assets/Sprites/VFX/Px4.png` (4×4 beyaz; point, clamp, mip yok, PPU 16), `Assets/Material/VFX/M_FX_AcidPixel.mat` (Sprite-Unlit-Default), `Assets/Prefab/New/VFX/FX_AcidImpact.prefab` (eval ile).

**`FX_AcidImpact`: tek `ParticleSystem` (`Droplets`)**
- **Main:** Simulation Space World, **Scaling Mode Local**, `playOnAwake` açık, `looping` açık, Emission kapalı, Shape kapalı, max particles 64.
- **Ömür ve hız:** lifetime 0.35–0.6, start speed 0 (kod verir), gravity modifier 2.5.
- **Color over Lifetime (Fixed):** #C7FF70 → 0.15'te #59E047 → 0.6'da #1FBA29; alfa 0.85'e kadar 1, sonra 0.
- **Size over Lifetime:** 1 → 0.7'de 0.67.
- **Renderer:** Billboard, Oldest in Front, sorting **VFX**, order 5, materyal `M_FX_AcidPixel`.
- Collision ve sub-emitter kapalı (B aşaması).

**Kabul:** eval prefab ayarlarını raporlar; Play Mode'da 12 damla emit edilip `Simulate(0.2)` ile alınan karede kare damlalar görünüyor ve CRT altında kaybolmuyor (ortho 6.4 ve en uzak oda kamerasında birer kare).

### Task A5: Emit ve ayar SO'su — #code, 1–1.25 sa

**Dosyalar:** Oluştur `Death/AcidReactionSettingsSO.cs` ve `New-Scripts/Death/SO/AcidReactionSettings.asset`; değiştir `Death/AcidReaction.cs`; `Acid.prefab` alanları eval ile atanır.

**`AcidReactionSettingsSO`** (`[CreateAssetMenu]`), `OnValidate` ile aralıklar sıralanır, test yazılmaz:

| Alan | Değer |
|---|---|
| `minSpeed` | 4 |
| `maxSpeed` | 40 |
| `strengthFloor` | 0.35 |
| `dropletCount` | (6, 14) |
| `dropletSpeed` | (4, 7) |
| `bigDropletCount` | (2, 3) |
| `bigDropletSpeed` | (2.5, 4) |
| `coneDeg` | 35 |
| `inheritX` | 0.3 |
| `inheritY` | 0.25 |
| `maxDropletSpeed` | 9 |
| `dropletSizePx` | 2 |
| `bigDropletSizePx` | 3 |
| `pixelsPerUnit` | 16 |
| `RumbleProfile acidDeathRumble` | A6 |
| `AudioCueSO splashCue`, `AudioCueSO sizzleCue` | A6 |

**`AcidReaction`**
- `[SerializeField] AcidReactionSettingsSO settings`, `[SerializeField] ParticleSystem impactFxPrefab`, `[SerializeField] bool hasSurface = true` (`FlowingAcid`'de false, B2).
- **İlk temasta** FX lazy instantiate edilir (asidin child'ı). Odaların her biri boşta sistem yaratmasın diye `Awake`'te değil.
- `Killing`'de `Emit(EmitParams)` çağrılır: konum dünya temas noktası; hız `SplashVelocity` (`random01` her damla için ayrı); boyut `px / pixelsPerUnit`.
- `OnEnable` ve `ResetToDefault`'ta `Clear()`. Uyuyan oda ve respawn'da donmuş damla kalmaz.
- `DebugImpact` damlaları da basar ve `Simulate(age, true, false)` çalıştırır.

**Kabul:** donmuş zamanda 0.05, 0.2 ve 0.3 sn kareleri; damlalar temas noktasından çıkıyor ve yatay hızla eğiliyor (biri sola, biri sağa koşarak girişte, `DebugImpact`'e hız parametresi eklenebilir); EditMode ve PlayMode yeşil.

### Task A6: Titreşim, ses kancası, `FX_PlayerDeath` sırası, dokümanlar — #code, 1 sa

**Dosyalar:** `Death/AcidReaction.cs`, `AcidReactionSettings.asset` (yeni `AcidDeath` `RumbleProfile` değerleri, §3.4), `FX_PlayerDeath.prefab` (AK6 onaylanırsa sorting VFX, eval), `docs/systems/hazards.md`, `docs/systems/death-respawn.md`.

**`Killed`**
- `acidDeathRumble` doluysa `HapticManager.Instance.Vibrate(...)`; Death profilini ezer, bilerek. Doğrulanacak: respawn yolu ya da oda geçişi titreşimi bunu erken kesiyor mu?
- `splashCue` ve `sizzleCue` için `cue != null && cue.AudioClips.Length > 0` ise **2D** `RaisePlayEvent(cue)`. Kanal `SFX-Play.asset`. Boşsa sessiz geç.

**Dokümanlar**
- `hazards.md`: "`AcidHazard`" altına "Temas tepkisi" (akış, olay sırası ve gerekçesi, yuva, SO, materyal ayarları, çok oyunculu kapsam dışı, AK kararları), "Son kod doğrulaması" tarihi.
- `death-respawn.md` "Ölüm anı": `Killing` (Kill'den önce) ve `Killed` (Kill'den sonra) adımları; dinleyici hatasının ölümü engellemediği notu.

**Kabul:** titreşim gamepad'de A7'de denenir. Kod derleniyor, EditMode ve PlayMode yeşil, dokümanlar kodu anlatıyor.

### Task A7: Gamepad testi — #test, kullanıcı, 1 sa

**Odalar:** 2, 3, 5 (slow-motion açıkken de), 12, 18, 22.

**Kontroller**
- Düşerek, yandan koşarak, swing'den, dash ve slingshot ile gir. Tepki temas noktasında mı? İris kapanmadan okunuyor mu?
- Yavaş girişte görünür mü (`strengthFloor`)? Hızlıda damlalar irisin dışına mı kaçıyor (`maxDropletSpeed`)?
- Oda 22 (alçak asit): çukur okunuyor mu?
- Ana menüden gelerek de dene.
- Arka arkaya hızlı ölümlerde tekrar eden, yapay bir görüntü var mı?
- Titreşim profili.

**Karar ver:** AK1, AK4, AK5, AK6 ve B aşamasından hangileri.

**Kayıt:** değişen değerler `feel-notlari.md`'ye.

---

## B aşaması: testten sonra, kullanıcı hangilerini isterse

- **B1. Dalga + kaynama** — #code, 1.25 sa
  - `AcidImpact.cginc`'e `AcidRipple(px)` eklenir: önden bir dalga boyu inceltmeli, sönümlü yürüyen dalga, tepe payıyla kırpılır. `AcidBoil(p)` kabarcık eşiğini lerp'ler. `_ImpactDuration` 1.0.
  - Kabul: dalga duvara (sprite kenarına) yansımadan çıkıyor; 1.0 sn karesi darbesiz kareyle aynı.
- **B2. Akan asit** — #code, 1 sa
  - `FlowingPixelAcid`: `AcidRing` (Bayer 4×4 ile titreşimli sönüm), flaş, kaynama, halka önünde akış bükülmesi. Kontur bloğu en sonda kalır.
  - `Mat_FlowingPixelAsit`'e küçük yarıçaplar yazılır; `FlowingAcid.prefab`'a `AcidReaction` (`hasSurface` false).
- **B3. Buhar** — #code, 0.75 sa. Yalnızca AK4 (b) ya da (c) seçilirse.
  - `FX_AcidImpact`'e `Steam` child'ı: 4–5 kare puf, 4–6 px, yukarı 0.4–0.9, gravity −0.15, noise 0.3, alfa kademeli 0.6 → 0.3 → 0.
- **B4. "Sen buradasın" silüeti (AK1)** — #code, 1–1.25 sa
  - Ölüm anında oyuncu sprite'ının dolgulu kopyası (küçük bir fill shader + proxy bileşen), `GhostAfterimage` havuz kalıbı.
  - `PlayerHealth`'e dokunmadan, `PlayerHealth.OnDeath` öncesi konumla.
  - Tüm tehlikelerde çalışacaksa `DeathZone.Killing`'i genel dinleyen tek bir bileşen olur.
- **B5. İris gecikmesi ya da donma (AK4)** — #code, 0.5 sa. `RespawnManager`'a tek tunable, varsayılan 0 (davranış aynı).
- **B6. Plop** — #code, 0.75 sa
  - `Droplets`'a Collision Planes (`SurfacePlane` child'ı yüzey yüksekliğinde) + Collision tipi sub-emitter `Plop`: burst 2, 2 px, ömür 0.15–0.25.
- **B7. Hareketli tehlike kabuğu (AK2)** — #code, 1.25 sa
  - `HazardAcid`'e tek `_HazardImpact` (yerel temas, yaş, güç). Mevcut çevre parametresi kodu bir fonksiyona taşınır; açıya bağlı çukur ve iki yana açılan wobble paketleri, çukur `wobble + spike + outline` ile sınırlanır.
  - Aynı `AcidReaction` (yüzey yok) ve yuvarlak damla varyantı (`Acid-Drop.png`, Billboard).
- **B8. Ses dosyaları** — #art. Sıçrama ×3, cızırtı ×2–3, isteğe bağlı fokurtu (§3.4 kaynakları); genel ölüm vuruşu sesi (AK8).
- **B9. Erişilebilirlik** — #code, Options işiyle birlikte. Flaş azaltma (`_AcidFlashScale` global), efekt azaltma (damla sayısı çarpanı), titreşim çarpanı (`HapticManager.SetMotorSpeeds`).

**Someday:** yakınlık tepkisi (AK3), rakip oyuncunun ölümünde tepki (multiplayer, `GhostView`'e ölüm bilgisi gerekir).

## Riskler ve dikkat edilecekler

1. **İris penceresi ≈0.3 sn.** A aşamasının bütün değerleri buna göre. Testte yetersiz gelirse çözüm efekti büyütmek değil, AK4.
2. **MPB:** `AcidHazard` editörde `transform.hasChanged` ile tekrar yazıyor; Get yaptığı için yuva korunur (A3 kabulünde doğrulanır).
3. **Olay sırası:** veri `Killing`'de, titreşim `Killed`'de. `OnDeath` içinde `Velocity` sıfırlanıyor.
4. **Slow-motion (oda 5):** CPU yaşı `Time.deltaTime` ile sayıldığı için tepki de yavaşlar. Kabul edilebilir; test edilir.
5. **Çift ölüm:** AK7 onaylanmazsa iki asit aynı anda tepki verebilir ve iki respawn dizisi başlayabilir (mevcut hata).
6. **Kesirli `_PixelScale`** (oda 18, 22): sıkıştırma ve `floor` buna dayanıklı; testte var.
7. **Edit Mode çekimleri** parçacık ve zaman oynatmaz; doğrulama hep Play Mode + donmuş zaman.
8. **Tiled draw mode** bu shader'larda zaten bozuk (her tile kendi UV'si); `OnValidate` uyarısı yeterli, düzeltme kapsam dışı.

## Önerilen takvim (TASKS.md'ye eklenmeden önce onay)

| Gün | Task'lar |
|---|---|
| 2026-11-19 Per | A1, A2, A3 (+ mevcut "Hareketli node gamepad testi") |
| 2026-11-20 Cum | A0 (AK7 onaylanırsa), A4, A5, A6 |
| 2026-11-21 Cmt | A7 gamepad testi — günün geri kalanı tampon |
| Testten sonra | Seçilen B task'ları; B8 (#art) ve B9 `Someday` |
