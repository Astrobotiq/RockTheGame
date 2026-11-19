# Spiral node'lar: tasarım

*2026-11-17. TASKS.md: "Spiral node'lar" (eskiden yalnızca `#art`). Kararlar kullanıcıyla soru-cevapla alındı; üç açıdan (fizik, oyun tasarımı, kod entegrasyonu) agent brainstorming'i yapıldı.*

## Niyet

Spiral node, üstünde dönen bir spiral olan bir kanca node'udur. Oyuncu **tek kancayla** tutununca node onu spiralin dönme yönünde (saat yönü ya da tersi) kendiliğinden döndürür: motorlu bir mil gibi. Normal node'un yapamadığı şeyi yapar: momentumun olmadan seni tepeden aşırır, bir engelin etrafından dolaştırır, belli bir yöne fırlatmaya hazırlar.

Kullanıcının odalarda düşündüğü kullanımlar (hepsi):

1. **Zamanlamalı fırlatıcı:** doğru anda bırakıp bir boşluğa ya da hedefe fırlamak. Nişan stick'le değil zamanlamayla.
2. **Kaldırıcı:** pompalayarak ulaşılamayan bir çıkıntının üstüne taşımak.
3. **Engelin etrafından taşıma:** diken/asit ya da duvarın etrafından dolanıp güvenli yerde bırakmak.
4. **Spiral zincirleri:** zıt yönlü spirallerle S ya da sekiz çizen akışlar.

## Hedefler / hedef olmayanlar

- **Hedef:** tek kancada motor. Durgun tutunan oyuncu, ip boyu ne olursa olsun, spiral yönünde tam tur atar.
- **Hedef:** normal node'larda davranış birebir aynı. Mevcut EditMode ve PlayMode testleri değişmeden yeşil kalır.
- **Hedef:** v1'de shader (yön işaretli), döngü sesi + titreşim, test odası, ayrı bir **ters dönen spiral** türü (§7).
- **Hedef değil:** hareketli spiral node. Kullanıcı: "bunu hareketli node'larla kullanmayı düşünmüyorum, bu ayrı bir node türü." Motor node'a göre hızda çalıştığı için teoride birleşir, ama prefab, test ve destek yok.
- **Hedef değil:** çift kancada ya da slingshot'ta motor.
- **Hedef değil:** spiral + dönen node (`FullRotationNode`) aynı node'da.
- **Hedef değil:** ~~spirale özel hava profili ya da bırakış boost'u.~~ 2026-11-20'de geri alındı: çıkış hızı `docs/spiral-cikis-hizi.md` (SP8).
- **Hedef değil:** gerçek odalara yerleştirme (test odası hariç). Ayrı level işi.
- **Anti-hedef:** oyuncu spiral tarafından bir duvara bastırılıp takılı kalırsa; spiral "binek"e dönüşüp oyuncu kontrolü kaybettiğini hissederse; shader'ın gösterdiği yön ya da hız hareketle çelişirse; durağan bir karede dönme yönü okunamazsa; normal node'un hissi değişirse, iş başarısızdır.

## Alınan kararlar

| Konu | Karar |
|---|---|
| Model | **Motor + yerçekimi.** Sarkaç aynen çalışır; node, teğet hızı hedef hıza doğru sınırlı ivmeyle iter. Tepede yavaşlar, dipte hızlanır, ama tur hep tamamlanır. |
| İp boyu | **Tutunulan mesafe** (bugünkü gibi). Hedef hız birim/sn; açısal hız ip boyuna göre değişir. |
| Ters yönde gelerek tutunma | **Yavaş mücadele:** motor oyuncuyu yavaş yavaş durdurup çevirir (~0.6–1 sn, yaklaşık yarım tur). |
| Spiralden hızlı gelerek tutunma | Fazla hız **korunur** (yalnızca ters yönde motor fren yapar). |
| Stick | Hedef hızı **±%30–50** değiştirir (dönüş yönüne basınca hızlanır, tersine basınca yavaşlar). Asla durdurmaz, çevirmez. |
| Bırakış | ~~Normal tek kanca bırakışı, ek hız yok.~~ 2026-11-20: spin teğetinde itme + kısa süre havada fren yok (`AirEntry.SpiralRelease`, SP8, `docs/spiral-cikis-hizi.md`). |
| Çift kanca / slingshot | Motor **yalnızca tek kancada**. Spiral + başka node = normal çift kanca; tek kancaya dönünce motor geri gelir. Aynı spirale iki kol = normal slingshot. |
| Dönen node ile aynı node | **Hayır**, editör uyarısı. |
| Hareketli spiral | Kapsam dışı. |
| Yön değiştirme | **Ayrı node türü:** ters dönen spiral, tutunurken 360° (ayarlanabilir) dönünce yön değiştirir, tutundukça tekrarlar, bırakınca ilk yönüne döner. Normal spiral hep aynı yönde. |

## Yaklaşımlar

| | Ne | Artı | Eksi |
|---|---|---|---|
| **A (seçildi)** | Motor `SwingingState`'in içinde. Node'daki `SpiralNode` bileşeni tutunurken okunur; varsa sarkaç adımına motor eklenir. Matematik saf `SpiralDriveMath`'te. | Bırakış, çarpışma, çift kanca geçişi ve hareketli node mantığı paylaşılır, S1/S2, H2, M3, M7 kendiliğinden korunur. Küçük diff. | `SwingingState` biraz büyür. |
| B | Ayrı `SpiralSwingingState`. | İzole. | Bırakış, çift kanca/slingshot geçişi ve çarpışma kopyalanır, zamanla ayrışır. |
| C | Genel "sarkaç sürücüsü" arayüzü. | Rüzgâr/akıntı gibi ileride gelebilecek sürücülere hazır. | Tek kullanıcı spiral; erken soyutlama. |

Kullanıcı A'yı seçti (2026-11-17).

## Tasarım

### 1. `SpiralNode` bileşeni (`Player/Nodes/Spiral/`)

Node kökünde (collider'ın olduğu objede) duran MonoBehaviour. Per-node alanlar:

- `direction`: `Clockwise` / `CounterClockwise`.
- `targetSpeed`: hedef teğet hız, birim/sn. Başlangıç değeri **18** (`PlayerStats.asset`'teki `MaxSwingSpeed` 28'in altında; `grapple.md` hâlâ 25 yazıyor, o da düzeltilecek).
- `reverseEveryDegrees`: 0 = normal spiral (hep aynı yön); > 0 = ters dönen spiral, tutunurken bu kadar dönünce yön değiştirir (bkz. §7). Ters dönen prefab'da 360.
- Görsel/ses referansları (shader renderer'ı vb.).
- `OnValidate`: aynı objede ya da parent'ta `FullRotationNode` varsa `Debug.LogWarning`.
- `OnDrawGizmos`: yön oku ve birkaç örnek ip boyu için tur süresi etiketi (`2πR / targetSpeed`), level tasarımı için.

Ortak his değerleri `PlayerStatsSO`'da yeni `[Header("Spiral Node")]` altında (başlangıç değerleri, his kullanıcının kararı):

| Alan | Başlangıç | Anlamı |
|---|---|---|
| `SpiralDriveAccel` | 60 | Dönüş yönünde, hedefin altındayken hızlanma (b/sn²). 0'dan 18'e ~0.3 sn. |
| `SpiralOpposeAccel` | 45 | Ters yönde giderken motorun fren + çevirme ivmesi (b/sn²). 25 b/sn ters gelişte ~1 sn. |
| `SpiralInputInfluence` | 0.4 | Stick'in hedef hıza etkisi (0.4 = ±%40). |
| `SpiralOverspeedDecay` | 0 | Hedefin üstündeki hızın sönme ivmesi (b/sn²); 0 = korunur. |

**Döngü garantisi:** yerçekiminin teğet bileşeni en fazla |`Gravity`| (32). `SpiralDriveAccel` ve `SpiralOpposeAccel` bunun üstündeyse oyuncu hiçbir noktada takılıp geri sallanmaz; her ip boyunda tam tur atar. Bu koşul bir EditMode sanity testiyle korunur (`PlayerStatsSanityTests`'e ya da yeni teste).

### 2. Bağlama (`SwingingState.EnterState`)

- Spiral, `TryBindFullRotationNode`'daki `OverlapPointAll` yerine kolun node'undan okunur: `(_swingingArm == Left ? LeftAnchorNode : RightAnchorNode)?.GetComponentInParent<SpiralNode>()`. Ham anchor (testlerdeki `StartSwingFrom`) spiral değildir.
- Spiral bulunursa `FullRotationNode` bağlanmaz (editör uyarısının çalışma zamanı karşılığı).
- Yerden kanca: hop aşaması aynen kalır, motor `InitializePendulum`'dan sonra başlar (dönen node tracker'ı gibi). Hop'un X yönü spiralin tersi olabilir; bu da "ters yönde tutunma" kuralına düşer.
- `SpiralNode`'a "bağlandı" bildirilir (görsel parlama, ses, titreşim başlar).

### 3. Motor (`ApplyArcadePendulum` içinde, yalnızca spiral varken)

İşaret kuralı: `tangent = (d.y, −d.x)`, `d` oyuncudan anchor'a. Pozitif teğet hız **saat yönünün tersi** (oyuncu anchor'ın altındayken sağa gitmek). `spinSign` = CCW için +1, CW için −1.

Her fizik adımında, yerçekimi ve node ataleti eklendikten, teğete izdüşümden sonra:

1. `s` = teğet hız (node'a göre, işaretli).
2. Stick: `input = dot(stick, spinSign × tangent)`, [−1, 1]. Bu, "gittiğin yöne bas" demek; tepede X'in ters dönmesi sorunu yok. Pompalamayı süren stick bugünkü gibi tutunan kolun stick'i.
3. `target = spinSign × targetSpeed × (1 + SpiralInputInfluence × input) × ActiveSpeedMultiplier`.
4. `s = SpiralDriveMath.StepTangentialSpeed(s, target, ...)`:
   - `s` ters yöndeyse (işaretleri farklı): `SpiralOpposeAccel` ile hedefe doğru (sıfırdan geçip çevirir).
   - Aynı yönde ve hedefin altındaysa: `SpiralDriveAccel` ile hedefe doğru.
   - Aynı yönde ve hedefin üstündeyse: motor kuvvet uygulamaz; `SpiralOverspeedDecay` > 0 ise o ivmeyle hedefe iner.
5. Normal pompalama (`SwingForceMultiplier`) ve `MaxSwingSpeed` sınırı spiralde **uygulanmaz**: motor kendi hedefini kullanır, aksi halde `MaxSwingSpeed`'in (asset'te 28) üstündeki bir hedef sessizce takılır.

Yerçekimi aynen işler; tepede yavaşlama, dipte hızlanma kasıtlı. Hedefin altındaki her bölgede motor enerji eklediği için hız turdan tura sınırlı bir döngüye oturur (örn. R=5'te dipte ~31 b/sn). Bu kabul edildi (motor + yerçekimi seçimi).

### 4. Takılma kontrolü (önkoşul, yalnızca spiralde)

Bugün `CheckSweepCollision` yalnızca küçük radyal düzeltmeyi tarıyor. Teğet hareket `KinematicPhysicsHandler.Move` içinde sessizce engelleniyor ve `SwingingState` sonucu geri okumuyor. Normal sarkaç duvara takılınca yerçekimiyle geri sallanıyor; motor ise oyuncuyu duvara sonsuza kadar bastırır.

Kural: spiraldeyken oyuncunun gerçekte gittiği teğet hız (konum değişimi − anchor kayması, teğete izdüşülmüş) art arda birkaç fizik adımı boyunca komut verilen hızın küçük bir kesrinin altındaysa (başlangıç: 3 adım, %25), kanca **kopar**: S2 gibi `BreakGrapple` → `AirEntry.Fall()`. Ölçümün yeri ve sabitleri inşa edene kalır; normal sarkaca uygulanmaz (S1/S2 testleri değişmesin).

Level tasarımı notu (`grapple.md`'ye girer): spiralin etrafındaki dönüş diski, oyuncunun tutunabileceği en uzak mesafe + 0.5 (yarım oyuncu kutusu) kadar boş olmalı ya da çarpma kasıtlı olmalı. `detectionRadius` NewScene'de 10 (en kötü ~10.5 birim), koddaki varsayılan 15 (~15.5 birim); başka sahnede oyuncu varsa varsayılan geçerli.

### 5. Bırakış ve kopma

- Tetik bırakılınca: 2026-11-20'den beri `AirEntry.SpiralRelease(...)`, teğet hız + `SpiralExitBoost`, `SpiralExitHoldTime` boyunca havada fren yok (SP8, `docs/spiral-cikis-hizi.md`). Kapılar karşılanmazsa eski yol (`SwingRelease`).
- Kopma (S1 iniş, S2 duvar, takılma) bugünkü gibi.
- `ExitState`: spirale "bağlantı bitti" bildirilir (görsel, ses ve titreşim durur).

### 6. Çift kanca ve slingshot

- `SwingingState` → `DualSwingingState` geçişinde motor biter (state değişiyor). Çift kancada spiral görsel olarak "boşta" (soluk) görünür.
- Çift kancadan spirale tutunan kolla tek kancaya düşülünce yeni `SwingingState` spirali yeniden bağlar ve motor o anki hızdan devam eder (hop yok, §3 kuralları).
- Aynı spirale iki kol: normal slingshot (yön girişteki konumdan node'a).

### 7. Ters dönen spiral (ayrı node türü)

Kullanıcı kararı (2026-11-17): yön değiştirme her spiralin seçeneği değil, **ayrı bir node türü**. Normal spiral hep aynı yönde döner. Ters dönen spiral, onunla belli bir açı döndüğünde tutunurken yön değiştirir.

- Ayrı prefab `Node-SpiralReversing`. Kodda aynı `SpiralNode` bileşeni, `reverseEveryDegrees` alanıyla: 0 = normal spiral, ters dönen prefab'da **360** (varsayılan). *(Ayrı bileşen yerine alan: benim kararım, en küçük diff.)*
- Sayaç yalnızca motor çalışırken ilerler (tek kanca, yerden kanca hop'u bittikten sonra). O anki dönüş yönündeki net açıyı biriktirir, 0'ın altına inmez: ters yöne fırlatılıp henüz çevrilmemişken sayılmaz. İşaretli açı biriktirme `FullRotationTracker`'daki gibi.
- Sayaç `reverseEveryDegrees`'e ulaşınca yön çevrilir, sayaç sıfırlanır. **Tutundukça her 360°'de tekrarlar** (ileri-geri).
- Çevrilince oyuncu artık spiralin tersine gidiyor demektir: motor onu `SpiralOpposeAccel` ile yavaşlatıp çevirir (SP2'deki "yavaş mücadele"nin aynısı, ek kural yok).
- **Bırakınca ilk yönüne döner**, sayaç sıfırlanır. Tutunmadan önce görülen yön her zaman alınan yöndür. Respawn için ek sıfırlama gerekmez (durum yalnızca bağlantı süresince var, respawn bağlantıyı siler).
- Çift kancaya geçmek de bağlantıyı bitirmiş sayılır (motor çalışmıyor): tek kancaya dönünce ilk yönden ve sıfırdan başlar. *(Benim kararım.)*
- **Okunurluk:** halkada çevrilmeye kalan ilerleme gösterilir (`CircularRotationRing`'in `_Progress`'i gibi); çevrilme anında kısa parlama + titreşim "tık"ı. İki tür durağan karede birbirinden ayırt edilir (ters dönende çift yönlü işaret vb.), ama ters dönen spiralin ilerleme halkası dönen node'un (`FullRotationNode`) 360° halkasıyla karıştırılmamalı.
- Ters dönen spiral de `FullRotationNode` ile aynı node'a konmaz (aynı uyarı).

### 8. Sprite yönü

Bugün `PlayerAnimationController.cs:102-108` önce **sol stick**'in X'ine (sağ kolla sallanırken bile), stick boşsa `Velocity.x`'e bakıyor; tam turda stick boşken sprite her yarım turda çevrilir. `flipX` oyun mantığına da sızıyor: `PlayerController.FacingDirection` ondan okunuyor ve stick boşken dash yönünü belirliyor (`DashState.cs:22`). Swing'den dash atılamadığı için etkisi küçük, ama bırakıştan hemen sonraki dash yönü sabitlenmiş yöne göre olur. **Benim kararım** (kullanıcının cevabı bu soruyla eşleşmedi; düzeltebilir): spiraldeyken sprite dönüş yönüne göre sabit baksın (CCW → sağa, CW → sola), bırakınca normale dönsün. Uygulama yeri `PlayerAnimationController` (`flipX`) ve `PlayerEyesController`; bir `PlayerController` özelliği üzerinden okunur.

### 9. Görsel: spiral shader

- Yeni shader, mevcut `CircularRings` / `CircularRotationRing` aile stilinde (node'un gökkuşağı ramp'ini kullanır). Pürüzsüz baskı / halftone, **piksel art değil**.
- **Dönme hızı gerçek hareketle eşleşir:** boştayken `targetSpeed / referansYarıçap` açısal hızla döner (referans yarıçap node'da ayarlı, başlangıç 3 birim); tutunulduğunda oyuncunun o anki açısal hızını (`s / ip boyu`) gösterir.
- **Yön durağan karede okunur:** spiralin kendisi yetmez; halkada dönüş yönünü gösteren ok/diş (cırcır gibi). CW/CCW renk farkı yalnızca destek olabilir, tek ipucu olmaz.
- Tutunulunca kısa bir parlama ("devreye girdi"), çift kancada soluk.
- Hit-stop sırasında spiral durur (görsel ve fizik ayrışmasın). Hit-stop `Time.timeScale`'i değiştirmiyor, yalnızca oyuncuyu duraklatıyor (`PlayerController.cs:362-372`); bu yüzden shader'ın `_Time`'ı kullanılamaz, dönüş açısı script'ten (`SpiralNode`) bir shader parametresi olarak sürülür ve `HitStopEvents` sırasında ilerlemez.

### 10. Ses ve titreşim

- **Döngü sesi:** tutunurken çalan döngü (vınlama), perdesi o anki hıza bağlı. `PlayerAudioController`'da stop kanalı yok; `AudioCueStopEventChannelSO` ile eklenecek. `AudioManager` perdeyi yalnızca ses başlarken ayarlıyor (`AudioManager.cs:203`), çalarken değiştirilemiyor: v1'de **sabit perde**. Hıza bağlı perde ses sistemine eklenmesi gereken ayrı bir iş (Someday).
- **Sürekli titreşim:** `PlayerVibrationSettingsSO`'ya yeni continuous profil (`SpiralOrbit`), `WallSlidingState`'teki Start/Stop deseni (`HapticManager.StartContinuousVibration` / `StopContinuousVibration`).
- **Ters tutunuş:** tek seferlik "tak" titreşimi (`SpiralOpposeGrab` profili) + ses. Hit-stop yok.

### 11. Prefab ve test odası

- `Assets/Prefab/New/Node/Node-Spiral.prefab`: oda node'larının kopyası (Grapple katmanı 7, `CircleCollider2D` r=0.5) + `SpiralNode` + spiral shader'lı görsel child.
- Test odası, hareketli node test odası gibi (`RoomNew-MovingNodeTest.prefab` örneği): bir kaldırıcı (çıkıntı altında), bir fırlatıcı (boşluk + hedef platform), zıt yönlü iki spiral (zincir), ters yönde tutunmaya zorlayan bir düzen, bir ters dönen spiral (360° sonra çevrilmeyi görmek için uzun bir tur alanı), bir de dönüş diskine giren bir duvar (takılma → kopma). Sahne işleri yalnızca `unity command eval` ile.

### 12. Testler (onay gerekir, CLAUDE.md kural 3)

**EditMode `SpiralDriveMathTests`:** aynı yönde yavaş/eşit/hızlı, ters yön, sıfırdan başlama; stick hedefin işaretini asla çevirmez; döngü garantisi (ivme > |g|) ve R ∈ {2, 5, 10} için küçük bir simülasyonda biriken açının 360°'yi geçmesi (`FullRotationTracker` yeniden kullanılabilir).

**Kurallar (`bozulmamali-davranislar.md`, yeni "Spiral node" bölümü; kullanıcı 2026-11-17'de onayladı) ve PlayMode `SpiralNodeRulesTests`:**

| ID | Kural |
|---|---|
| SP1 | Tek kancayla spirale durgun tutunan oyuncu spiralin yönünde tam tur atar; ip boyundan bağımsız. |
| SP2 | Spiralin tersine giderek tutunan oyuncu yavaşlar, durur ve spiral yönüne döner; dönüş anlık değildir. |
| SP3 | Stick spiralin hızını değiştirir ama dönüşü durdurmaz, çevirmez. |
| SP4 | Spiral oyuncuyu bir yüzeye bastırırsa kanca kopar; oyuncu takılı kalmaz. |
| SP5 | Spiralden tek kancayı bırakınca oyuncu, SP8 dışında normal tek kanca bırakışıyla (H2) havaya geçer. |
| SP8 | Spiralden tetiği bırakarak çıkış, bırakış hızından hızlıdır (`docs/spiral-cikis-hizi.md`). |
| SP6 | Çift kancada motor çalışmaz; aynı spirale iki kol normal slingshot'tır. |
| SP7 | Ters dönen spiral, tutunurken kendi yönünde `reverseEveryDegrees` (360°) dönünce yön değiştirir ve tutundukça her seferinde tekrarlar; bırakınca ilk yönüne döner. Normal spiral hiç yön değiştirmez. *(SP1–SP6 onaylandı; SP7 kullanıcının 2026-11-17 tarifiyle yeniden yazıldı.)* |

Test kurulumu: `SpawnNode` → `AddComponent<SpiralNode>` + reflection `SetField` → `EnterAir` → mevcut node'a gerçek cast. Bugünkü `GrabWithRight` (`PlayerRuleTestBase.cs:189-195`) kendi node'unu spawn ettiği için spiralin üstüne düz bir node koyar; `StartSwingFrom` node bağlamaz. Bu yüzden test tabanına "var olan node'u yakala" yardımcısı eklenir ve `MovingNodeRulesTests`'teki private `SetField` (`:131`) `PlayerRuleTestBase`'e taşınır. Yön, (oyuncu − anchor) × hız çapraz çarpımının işaretiyle ölçülür.

### 13. Dokümanlar

- `docs/GDD.md` §5.2 "Kanca ailesi": spiral node maddesi.
- `docs/systems/grapple.md`: yeni "Spiral node" bölümü, tunable tablosu, level tasarımı notu (dönüş diski), kenar durumlar; "Son kod doğrulaması".
- `docs/bozulmamali-davranislar.md`: SP1–SP7 (kodu yazmadan önce, kullanıcı onayıyla).
- `TASKS.md`: Someday'deki `#art` task'ı yerine Active'de "Spiral node'lar" grubu (task'lar eklenmeden önce kullanıcıya sorulur).

## Kısıtlar

- Unity 6000.3.10f1. Sahne/prefab işleri yalnızca `unity command eval` ile; `.unity` elle düzenlenmez.
- Yeni tunable'lar `PlayerStatsSO`'ya. His testi gamepad ile, kullanıcı tarafından (kural 4); başlangıç değerleri yukarıda.
- EditMode ve PlayMode testleri yeşil kalmalı; normal node'larda davranış değişmez.

## Kendi kararlarım (kullanıcı kontrol etsin)

- Başlangıç değerleri: `targetSpeed` 18, `SpiralDriveAccel` 60, `SpiralOpposeAccel` 45, `SpiralInputInfluence` 0.4, `SpiralOverspeedDecay` 0.
- Stick etkisi "gittiğin yöne bas" (`dot(stick, dönüş teğeti)`), bugünkü yalnızca X pompası değil.
- Spiralde `MaxSwingSpeed` sınırı ve normal pompalama yok; `ActiveSpeedMultiplier` hedefi ölçekler.
- Takılma kontrolü yalnızca spiralde.
- Ters dönen spiral ayrı bileşen değil, `SpiralNode`'da `reverseEveryDegrees` alanı + ayrı prefab.
- Ters dönen spiralde çift kancaya geçmek bağlantıyı bitirir (sayaç ve yön sıfırlanır).
- Spiraldeyken sprite dönüş yönüne sabit bakar.
- Shader boştayken referans yarıçapa göre, tutunulunca oyuncunun gerçek açısal hızıyla döner.

## İnşa edene kalanlar

- Takılma ölçümünün tam yeri (bir sonraki adımın başında konum farkı mı, tam teğet tarama mı) ve sabitleri.
- `SpiralNode`'a bağlantı bildiriminin biçimi (doğrudan çağrı, `FullRotationNode` deseni).
- Ters dönen spiralin ilerleme halkasının görsel biçimi (dönen node halkasından ayrışacak şekilde).
- Sprite yönü sabitlemenin `PlayerController` üzerinden nasıl okunacağı.
- Shader'ın parametre adları ve ramp kullanımı.
- Test odasının tam yerleşimi.
