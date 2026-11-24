# Teleport platformlar (takas blokları): tasarım ve uygulama planı

> **Uygulayan için:** task task uygulanır (`superpowers:executing-plans` ya da `superpowers:subagent-driven-development`). Önce bu dokümanın tamamını, sonra [`systems/platforms.md`](systems/platforms.md) "Kırılan platform" bölümünü oku.

**İlham:** Donkey Kong Bananza, The Divide katmanındaki *Switcheroo Goo* (mariowiki.com/Switcheroo_Goo). Eş boyutlu iki çerçeve var: biri pembe, biri mavi. Başta pembe çerçeve dolu (üstünde durulabilen katı malzeme), mavi çerçeve boş. Dolu çerçeveden koparılan parça, mavi çerçevede **aynı konumda** belirir. Parça hangi çerçevedeyse o çerçevenin rengini alır. Bu sistemle merdiven kurulur ya da bir yol kapatılıp başka bir yol açılır.

**Bizdeki karşılığı:** yumruk yerine **dash**. **Bloğun tamamı değil, yalnızca dash'in gövdeyle süpürdüğü kısım** eş çerçevede aynı konuma ışınlanır (geliştiricinin en önemli şartı, 2026-11-23). Bloğun ortasından yatay dash atılırsa ortada gövde yüksekliğinde bir tünel açılır, üst ve alt kısım yerinde kalır. Mavi çerçevede de aynı şekilde, tünel biçiminde bir şerit belirir:

```
pembe (önce)    pembe (sonra)    mavi (sonra)
██████████      ██████████       ┌ ─ ─ ─ ─ ┐
██████████  →   ··········       ██████████
██████████      ██████████       └ ─ ─ ─ ─ ┘
   dash →         tünel          aynı yerde şerit
```

Çapraz ya da kısmi dash da aynı kurala uyar: oyuncunun kutusu nereye değdiyse yalnızca o kısım geçer. "Bloğa dokundum, blok bütünüyle karşıya geçti" **istenmiyor**.

Örnek kullanım: barikat. Mavi çerçeve bir tehlikenin önünde durur, oyuncu pembe taraftan parça taşıdıkça tehlikenin önü duvarla kapanır.

## Kararlar

| Konu | Karar | Kimin |
|---|---|---|
| Tetik | Dash'le çarpmak | Geliştirici (2026-11-23) |
| Ne kadarı geçer | **Yalnızca oyuncunun kutusunun dash boyunca süpürdüğü alan.** Gerisi yerinde kalır. | Geliştirici (2026-11-23, en önemli şart) |
| Çözünürlük | Malzeme, gözle sürekli görünecek kadar ince bir ızgara: varsayılan hücre **0.125 birim** (1 birimde 8 hücre). İki çerçeve aynı boyda, A'daki *i* hücresi ile B'deki *i* hücresi eş. Her hücre her an ya A'da ya B'dedir, aynı anda ikisinde olamaz (değişmez kural). Tünelin kenarı ızgaraya oturur ama bu boyda tırtık okunmaz. Okunursa hücre küçültülür ya da gerçek poligon kesimine geçilir (Yaklaşımlar 3). | Claude önerisi |
| Dash sırasında | Dash **durmaz** (akış sütunu), malzemenin içinden geçerken her fizik adımında süpürülen alan karşıya geçer. Kalın bir duvardan geçmek gövde boyunda bir tünel açar, karşı tarafta da aynı şekilde bir şerit kurar. | Claude önerisi, his testinde bakılır |
| Süpürülen alan | Oyuncunun `BoxCollider2D` kutusunun adımın başından sonuna kadar taradığı alan: başlangıç kutusu + bitiş kutusu + aradaki köşeleri birleştiren şekil (iki kutunun dışbükey zarfı). Merkezi bu alanın içinde kalan hücreler geçer. Yatay ya da dikey dash'te bu bir dikdörtgen, **çapraz dash'te eğik bir bant** (altıgen) olur, dikdörtgen değil. | Claude önerisi |
| Çapraz tünelin kenarı | Bant ızgaraya oturduğu için kenarlar 0.125'lik basamaklar halinde. Fizikte eğim ya da basamak çıkma yok (`KinematicPhysicsHandler`), bu yüzden **çapraz tünelin tabanında yürüyerek yukarı çıkılamaz**: her 0.125'lik basamak duvar gibi durdurur. Aşağı doğru yürünür (her basamaktan küçük düşüşler). Dash ve zıplama tünelden geçer. İlk sürümde böyle bırakılır: çapraz tünel dash'le geçmek ya da karşı tarafta eğik bir barikat/rampa şekli kurmak için. Poligon kesimi (Yaklaşımlar 3) bunu çözmez, çünkü fizik eğimi de desteklemiyor. TP6'da sorun çıkarsa çözüm fizikte küçük bir basamak çıkma (≤ 0.13 birim) olur. Bu, oyuncu fiziğine dokunur ve ayrı bir task'tır. | Claude önerisi |
| Geri alma | Simetrik: B'deki bir hücreye dash atılırsa hücre A'ya döner. | Claude önerisi |
| Dash hakkı | Normal şekilde harcanır, blok hakkı geri vermez. | Claude önerisi |
| Hedef hücrede oyuncu varsa | Hücre "bekliyor" durumuna geçer: yarı saydam görünür, collider'ı kapalı kalır. Oyuncu hücreden çıkınca katılaşır. Hücre oyuncuyu asla ezmez, itmez ya da öldürmez (zor ama adil). | Claude önerisi |
| Basmak, tırmanmak, kanca | Hücreler zemin katmanında. Üstünde durulur, duvar tırmanma ve kayma normal şekilde çalışır. Basmak hücreyi taşımaz. Kanca hücreye tutunmaz (node değil). | Claude önerisi |
| Ölüm | `IResettable`: iki çerçeve başlangıç düzenine döner, bekleyen hücreler iptal edilir. | Proje kuralı |
| Oda uykusu | Durum korunur (yalnızca ölüm sıfırlar). | Claude önerisi |
| Alevli kapı | Oyunda yok. İlk sürümde barikat mevcut bir tehlikenin (`MovingDeath`) önüne kurulur. Alev tehlikesi ayrı bir iş (Someday). | Claude önerisi |

**Hedef dışı (ilk sürüm):** hareket eden çerçeveler, ikiden fazla çerçeve zinciri, dash dışında tetikler (kanca, basmak), çerçeveler arası görsel bağ çizgisi, son sanat (ilk görünüm düz renkli kare + çerçeve çizgisi).

**Anti-hedef:** oyuncunun bloğun neden geçmediğini anlamaması. Hangi hücrenin dolu, hangisinin boş olduğu, hangi çerçevenin hangisiyle eş olduğu bir bakışta okunmalı.

## Yaklaşımlar

1. **`SegmentedBreakablePlatform`'u genişletmek:** bloğu bütünüyle kırar (1 birimlik bloklar, tek sıra, basınca tetiklenir, dalga halinde yayılır). "Dokunduğum kadarı" şartını karşılamıyor. **Seçilmedi.**
2. **İnce ızgara + iki Tilemap (seçildi):** `SwapBlockGrid` (saf C#, hücre durumu ve kurallar) + `SwapBlockPair`. Her çerçeve bir `Tilemap` + `TilemapCollider2D` + `CompositeCollider2D` (kenarlar birleşik, oyuncunun raycast'leri tek düz yüzey görür, hücre başına collider yok). Swap = kaynaktan tile silmek, hedefe tile koymak; composite o adımda bir kez yenilenir. Kırılan bloktan yalnızca deseni ödünç alır (`LevelResetManager` kaydı, titreşim SO'su, VFX alanları).
3. **Gerçek poligon kesimi (Clipper2 gibi bir kütüphane):** kenarlar tam düz, ızgara yok. Ama poligon boolean işlemi, `PolygonCollider2D` yenileme, mesh üretme ve eş poligona birleştirme gerektiriyor; ilk sürüm için birkaç gün daha iş. 0.125'lik ızgara gözle yetmezse buna geçilir. **Şimdilik seçilmedi.**
4. **Her hücre ayrı GameObject:** 3×3 birimlik çerçevede 576 GameObject ve collider olur. **Seçilmedi.**

## Mimari

- `Assets/Scripts/New-Scripts/Platform/SwapBlockGrid.cs`: saf C#. `Cols`, `Rows`, hücre başına `Side { A, B }` + `Pending`. `CellsInSweptBox(Rect start, Vector2 delta, frameOrigin, cellSize) → hücre listesi` (süpürülen alan, yerel koordinatta; A ve B için aynı yerel kutu), `SwapCells(cells, isTargetOccupied)`, `ResolvePending(isTargetOccupied)`, `Reset()`. Başlangıç düzeni `bool[] startsInA` (varsayılan: hepsi A'da).
- `Assets/Scripts/New-Scripts/Platform/SwapBlockPair.cs`: `frameA`/`frameB` (`Tilemap`, sol-alt köşe hücre (0,0)), `cols`, `rows`, `cellSize` (Grid `cellSize`'ı bundan kurulur), `pinkTile`/`blueTile`/`pendingTile`. Tilemap'ler zemin katmanında. `IResettable`.
- **Dash algılama:** `SwapBlockPair.FixedUpdate`, oyuncu `CurrentState is DashState` ise oyuncunun kutusunu bu adımdaki hareketi kadar (`Velocity * fixedDeltaTime` + skin) ileri süpürür. Süpürülen alandaki dolu hücreler, hangi çerçevede iseler, **oyuncunun fizik çözümünden önce** karşıya geçer; böylece dash malzemeye çarpıp durmaz ve tünel tam gövde boyunda açılır. Script execution order ya da `PlayerController`'dan önce çalışma şartı uygulayan tarafından seçilir, gerekçesi bir satır yorumla yazılır. (Benzer okuma: `Death/GroundHeadPool.cs:300`.) Süpürülen alanın dışındaki hiçbir hücreye dokunulmaz.
- **Bekleyen hücre:** hedef hücre oyuncunun kutusunun içindeyse hücre `pendingTile` ile yarı saydam görünür, collider'ı yoktur (ayrı, collider'sız bir Tilemap katmanı ya da `TileFlags`/`colliderType`; uygulayan seçer). Her `FixedUpdate`'te oyuncuyla artık çakışmayanlar katılaşır.
- **Görünüm ve collider bir "görünüm katmanı"dır (A yoluna kapı açık kalsın diye, geliştirici kararı 2026-11-23):** hücre durumunun tek kaynağı `SwapBlockGrid`. Tilemap'ler yalnızca ondan beslenir: `SwapBlockPair` değişen hücreleri tek bir yerden (ör. `ApplyToView(changedCells)`) Tilemap'e yazar. Dash algılama, bekleyen hücre ve reset Tilemap'i okumaz, `SwapBlockGrid`'i okur. Böylece ileride Tilemap yerine marching squares mesh + `PolygonCollider2D` takmak yalnızca bu metodu ve prefab'ı değiştirir.
- **Maliyet:** composite yenilemesi adımda en fazla bir kez, yalnızca değişiklik olduysa. 4×4 birimlik bir çerçevede (1024 hücre) Profiler'da bir dash ölçülür ve rapora yazılır.
- **Geri bildirim:** `PlatformVibrationSettingsSO`'ya `swapBlock` rumble alanı, `AudioCuePlayEventChannelSO` üzerinden `swapCue` (klipsiz cue sessiz atlanır). Yeni tunable sabit yazılmaz.
- **Editör:** `OnDrawGizmos` iki çerçeveyi (pembe/mavi), ızgarayı ve dolu hücreleri çizer. Özel bir editör aracı ilk sürümde yok.

## Genel kısıtlar

- `main`'den yeni branch: `feature/teleport-blocks`. Her task bir commit, push yok. `feature/flower-k4` çalışma ağacındaki değişikliklere dokunulmaz.
- Sahne ve prefab işleri yalnızca `unity command eval` ile yapılır.
- Her `#code` task'ı sonunda derleme temiz, EditMode yeşil. Oyuncuya dokunan değişiklikten sonra PlayMode yeşil. Çıktılar rapora yazılır.
- `DashState`'e dokunulmaz. Dokunmak gerekirse durulur ve sorulur.

## Task'lar

### TP1: Takas ızgarası mantığı (`#code`, ~1 sa)
- `SwapBlockGrid` + `Assets/Tests/EditMode/SwapBlockGridTests.cs`.
- Testler: başlangıç düzeni; **süpürülen alan**: 3×3 birimlik malzemenin ortasından 1 birim yüksekliğinde yatay süpürme yalnızca ortadaki şeridi seçer, üst ve alt şeritler seçilmez; köşeye yarım değen süpürme yalnızca değdiği hücreleri seçer; çapraz süpürme bir şerit seçer, dikdörtgen değil; A→B, B→A aynı yerel konuma; aynı dash'te ikinci kez süpürülen hücre geri dönmez (dash bir hücreyi bir kez taşır); hedefte oyuncu varken `Pending`, oyuncu çıkınca katı; `Reset` bekleyenleri iptal eder; değişmez kural (hiçbir hücre iki tarafta birden değil).

### TP2: `SwapBlockPair` bileşeni ve dash algılama (`#code`, ~1.5 sa)
- Bileşen, iki Tilemap + composite collider, dash süpürmesi, bekleyen hücre, `IResettable`, gizmo, geçici görünüm (düz pembe/mavi tile).
- `Assets/Prefab/New/Platforms/SwapBlock/SwapBlockPair.prefab` (eval ile). Varsayılan: 3×3 birim, `cellSize` 0.125.
- Bitince `NewScene`'de bir kopyayla Play Mode'da ortadan yatay dash at, ekran görüntüsü al: pembede gövde boyunda tünel, mavide aynı yerde şerit görünmeli.

### TP3: Kural testleri (`#test`, ~1 sa)
`Assets/Tests/PlayMode/SwapBlockRulesTests.cs : PlayerRuleTestBase`. **Gameplay-critical:** geliştirici bu planı onaylayınca isimleri ve assert'leri onaylamış olur. Eşik gevşetmek gerekirse durulur ve sorulur.
- **SB1:** Dolu malzemenin ortasından yatay dash: yalnızca oyuncunun kutusunun süpürdüğü hücreler B'ye geçer, A'daki geçen hücre sayısı B'ye eklenen sayıya eşit. Kutunun üstünde ve altında kalan hücreler A'da kalır. Dash süresi ve çıkış hızı, malzemesiz dash'le aynı (dash durmaz).
- **SB1b:** Malzemenin köşesine yarım değen dash yalnızca değdiği kısmı taşır, malzemenin geri kalanı yerinde.
- **SB2:** B'de oluşan şeride dash atılınca süpürülen kısım A'ya döner.
- **SB3:** Oyuncu B'deki hedef hücrelerin içindeyken taşınan hücreler katı değildir. Oyuncu çıkınca katılaşır. Oyuncu ölmez, yeri değişmez.
- **SB4:** Yürüyüp üstüne basmak, duvar tırmanmak ya da dash'siz çarpmak hiçbir hücreyi taşımaz.
- **SB5:** Ölüp respawn olunca iki çerçeve başlangıç düzenine döner.

### TP4: İlk görünüm, ses ve titreşim (`#code`, ~1.5 sa)
- Çerçeve çizgisi (boş çerçeve de okunur), dolu malzeme çerçeve renginde. Swap'ta tünel kenarında kısa sönme, hedefte şeridin belirişi (yeni tween yazılmaz). Bekleyen kısım yarı saydam. 0.125 hücre kenarının tırtıklı görünüp görünmediğine oyun kamerasıyla bakılır, görünüyorsa not edilir.
- Rumble ve ses cue'su (yukarıda). Halftone yönüne uygun mu diye çekimle bakılır. Son sanat değil.

### TP5: Test odası ve doküman (`#level`, ~1 sa)
- Bir odada iki kurgu: **merdiven** (pembe yatay blok → mavi basamaklar) ve **barikat** (mavi çerçeve bir `MovingDeath`'in yolunda, taşınan bloklar oyuncuyu ondan ayırır).
- `docs/systems/platforms.md`'ye "Takas blokları" bölümü, GDD §6.2 Platformlar listesine bir satır.

### TP6: Gamepad testi (`#test`, ~1 sa)
- Çapraz tünel: basamaklı kenar göze batıyor mu, tabanında yürüyememek sinir bozuyor mu (basamak çıkma task'ı gerekir mi)?
- Tünelin dash'in tam izi gibi hissettirmesi ("dokunduğum kadarı gitti") en önemli soru. Hangi kısmın dolu olduğu, hangi çerçevenin hangisiyle eş olduğu okunuyor mu? Dash'in durmaması mı iyi, yoksa bloğa çarpıp durması mı (yumruk hissi)? Tünel açmak eğlenceli mi, kafa mı karıştırıyor? Bekleyen hücre anlaşılıyor mu? Notlar `feel-notlari.md`'ye.

## Sonraya (Someday)

- **Alev tehlikesi / alevli kapı:** barikatın asıl hedefi. Ayrı bir tehlike tasarımı (`hazards.md`).
- **Takas blokları son sanat:** TP6 sonrası, halftone yönünde.
- **Çapraz kenar yumuşatma (A yolu, 2026-11-23'te ertelendi, ilk sürüm basamaklı):** TP6'da basamaklı kenar ya da çapraz tünelde yürüyememek rahatsız ederse iki bağımsız task:
  1. *Yumuşak çizim:* `ApplyToView`'u marching squares ile mesh + collider üreten bir görünümle değiştir; kenar dash açısıyla düz. `SwapBlockGrid` ve testleri değişmez.
  2. *Basamak çıkma:* `KinematicPhysicsHandler`'da yürürken ≤ 0.13 birimlik basamağa otomatik çıkma (`PlayerStatsSO`'da tunable). 45°'ye kadar tünelde yürünür, daha dik basamak duvar kalır. Bütün PlayMode testleri + gamepad his testi. Takas bloklarından bağımsız, istenirse tek başına da yapılabilir.
