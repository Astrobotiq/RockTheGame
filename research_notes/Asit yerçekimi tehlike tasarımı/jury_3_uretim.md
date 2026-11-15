# Jüri 3: Üretim fizibilitesi ve teknik risk

*Mercek: tek kişilik üretim maliyeti, yeniden kullanım, teknik risk. Proje dosyaları salt okunur incelendi, Unity komutu çalıştırılmadı (2026-11-14).*

## Mevcut durum (dosyalardan)

- **Prefab'lar** `Assets/Prefab/New/Death/`: `MovingDeath.prefab` (Unity varsayılan `Square.png`, `Sprite-Lit-Default`, `BoxCollider2D` 1×1 trigger, katman 8, `PlatformController` + `TransformPingPongStrategy`, ölçek 0.8). `AccelDeath Variant` ölçeği 3 ve `AcceleratingPingPongStrategy`. `RotatingDeath Variant`, `RotatingPlatform Variant`'ın platform bileşenleri silinip **üç ayrı `Circle` çocuğu** eklenmiş hali: y = −1/0/1, her biri `CircleCollider2D` r 0.5 + `DeathZone`. Yani dairesel tehlike aslında birbirine değen üç boncuktan oluşan bir zincir.
- **Collider = görsel**, ikisi de birim şekil. Ölçek transform'dan geliyor. Boyutlar explicit (`m_Size 1`, `m_Radius 0.5`), sprite'tan türetilmiyor.
- **Kullanım:** oda 16'da 16 adet `AccelDeath` var, ölçekleri **4×7** (düzgün olmayan). Oda 19'da 2×5 / 4×5 boyutlarında. Oda 13'te 8, oda 20'de 4 `RotatingDeath` var (yarıçap 7–8.5). Gerilmiş dikdörtgen istisna değil, kural.
- **Shader'lar** `Assets/Shader/`: 13 shader'ın hepsi elle yazılmış CG/HLSL, **hiç Shader Graph yok**. En önemlisi `RisoSlab.shader`: boyutu `unity_ObjectToWorld`'den okuyor (`"DisableBatching"="True"`), `sdSlab` rounded box SDF, `valueNoise` kenar sapması, `fwidth` AA, hıza bağlı kayık baskı (`_Velocity`, `_Accelerating`). Bu shader'ı `RisoSlabVisual.cs` + `RisoSlabSettingsSO` + `RisoSlabMath` (test edilebilir saf mantık) besliyor ve hızı `IMovingSurface.SurfaceVelocity`'den MaterialPropertyBlock ile veriyor. `UiHazard.shader` 45° sarı/ink şerit kuralını içeriyor. `PixelArtAcid.shader` zemin asidi (piksel ızgaralı, `_AcidColor` 0.12/0.73/0.16), `AcidHazard.cs` de MPB ile `_PixelScale` gönderiyor. `HalftoneTrailEmitter` + `RisoDot.shader` hazır bir Shuriken kalıbı.
- **CRT** (`implementation-plan.md` H1): materyalde 480×360, volume'da 320×240, karar açık. Oda 16'da ortho 22 → 1 dünya birimi ≈ 5.5–8 CRT pikseli.

**Raporla çelişen iki nokta:** (1) Rapor Shader Graph ve "MPB yok" diyor. Projenin kanıtlanmış kalıbı ise HLSL + MPB + `DisableBatching`. Özel CG shader'lar zaten SRP Batcher'a girmiyor, MPB'nin bir maliyeti yok. (2) Dışa taşan dalga ya da diken için **dolgulu bir mesh şart**. Varsayılan Square/Circle sprite'larının kenarı collider'a tam oturuyor, dışa taşan her şey kırpılır.

## Puan tablosu (1–10, yüksek = iyi)

Sütunlar: Mal = maliyet, Yen = yeniden kullanım, Risk = teknik risk (yüksek = düşük risk), Perf = performans, Kap = tek shader kapsamı, Bak = bakım.

| | Mal | Yen | Risk | Perf | Kap | Bak | **Top** | Gerekçe |
|---|---|---|---|---|---|---|---|---|
| K1 | 7 | 8 | 7 | 9 | 8 | 8 | **47** | Çekirdek de SDF yapılırsa tamamı `RisoSlab` iskeleti; sprite çizimi gerekmez |
| K2 | 7 | 7 | 6 | 9 | 7 | 8 | **44** | Tek kenar fonksiyonu. Rounded box çevresinde dikeni parametrelemek ve CRT'de alt-piksel diken riskli |
| K3 | 5 | 5 | 5 | 8 | 5 | 6 | 34 | Ayrık damlalar ve boşluk-collider çelişkisi, çubukta tarak tekrarı zor |
| K4 | 6 | 7 | 6 | 9 | 5 | 7 | 40 | Çubukta iyi. Daire karşılığı zayıf, boşluklar hitbox sorusu açıyor |
| K5 | 7 | 4 | 4 | 7 | 3 | 5 | 30 | Ekstra halka objeleri, gerilmiş çubukta halka bozulur, node karışması |
| K6 | 6 | 6 | 7 | 9 | 7 | 7 | 42 | SDF uzayında kayan şerit güvenli. Bir katman daha |
| K7 | 8 | 9 | 8 | 10 | 9 | 9 | **53** | `UiHazard` şerit matematiği birebir taşınır. Bir katman, tek başına konsept değil |
| K8 | 5 | 8 | 4 | 6 | 7 | 5 | 35 | `InkDots` var ama dönen tehlikede moiré ve CRT titremesi var. Platform halftone'uyla karışır, 3×3 döngü 4×7 çubukta pahalı |
| K9 | 8 | 9 | 5 | 9 | 8 | 6 | 45 | Kayık baskı kodu hazır. Ama 0.06 birimlik kayma CRT'de <1 piksel, bloom global ve saf beyaz denetimi gerekiyor |
| K10 | 4 | 4 | 4 | 8 | 4 | 4 | 28 | Oyuncu takibi, diyafram animasyonu. Çubukta göz anlamsız |
| K11 | 4 | 5 | 3 | 6 | 6 | 5 | 29 | Ters gök için Camera Sorting Layer Texture ek pass istiyor, op-art CRT'de titrer |
| K12 | 3 | 3 | 3 | 5 | 4 | 4 | 22 | Örnek başına LineRenderer/flipbook, sıralama ve titreme. İki tehlike türü var |

## İlk üç ve hibrit

1. **K1** (gövde ve çekirdek, tamamı SDF)
2. **K7** (çekirdekte şerit katmanı)
3. **K2** (kenar modu)

**Hibrit "AcidHazard Moving" shader'ı:** `RisoSlab`'ın vertex iskeleti (boyut matristen, yerel uzay dünya biriminde), `_Shape` (0 daire, 1 kutu) ve `_EdgeMode` (0 düşük modlu dalga K1, 1 diken K2). Çekirdek iç SDF'te (~%35, stadyum ya da hap, tek ek yeri, sarı mercek), çekirdekte `UiHazard` kuralıyla 45° şerit (K7). Dalga ve diken yalnızca dışa doğru, dalga boyu dünya biriminde. Bir `_MinFeature` tabanı var (`RisoSlab`'daki `_DotMinDiameter` mantığı): hiçbir detay ~2 CRT pikselinin altına inmez. Hız ve telegraf, `RisoSlabVisual`'ın kopyası küçük bir `AcidHazardVisual` + `AcidHazardSettingsSO` + saf `AcidHazardMath` ile gelir. K9'un ısı kayması ileride aynı MPB'ye tek bir float olarak eklenebilir. K4 emitör kapakları ve damlalar faz 2.

## Vetolar

- **K12:** maliyet en yüksek ve örnek başına LineRenderer gerekiyor. Elektrik ve asit aynı nesnede iki ayrı ölüm dili oluyor.
- **K5:** tek renderer / tek collider sözleşmesini bozuyor, halkalar 4×7 ölçekte eliptikleşiyor, node siluetine en yakın biçim.
- **K11'in ters gök varyantı:** 2D renderer'a ek pass ve doku maliyeti getiriyor. Desen sürümü ancak sonradan parametre olarak eklenebilir.
- **K10 (varsayılan olarak):** çubuk ve zincir formlarına uymuyor. En fazla tema dünyasına özel bir varyant olabilir.
- **Rapordaki uygulama tercihleri:** Shader Graph + Sub Graph ve "collider'ı %5–10 küçült". İlki proje konvansiyonunu bozuyor. İkincisi hitbox'ı değiştiriyor, yani oyun hissi kararı (kural 4). Faz 1'de collider aynı kalır, görsel taban şekli collider sınırına oturur, dalga dışa taşar.

## Görev kırılımı (en iyi seçim, 1–1.5 saat)

1. **#code**: Dolgulu quad sprite (Full Rect, ~1.25 birim) ve `AcidHazardMoving.shader` iskeleti: matristen boyut, `DisableBatching`, daire/kutu SDF, dolgu ve kontur, `_DebugCollider` çizgisi. (1.5 s)
2. **#test**: Spike. 1×1, 4×7 ve dönen bir örnek, Frame Debugger, CRT açık, oda 16 kamerası. Köşede görselin collider'dan küçük kalmadığını gösteren ekran görüntüsü. (1 s)
3. **#code**: Kenar fonksiyonları: düşük modlu dalga + diken, dünya biriminde dalga boyu, `_MinFeature`. (1.5 s)
4. **#code**: Çekirdek SDF (hap ya da stadyum, ek yeri, mercek) ve `UiHazard` şerit katmanı. (1.5 s)
5. **#art**: Renk değerleri: asit yeşili `PixelArtAcid._AcidColor` ile eşleşir, plum çekirdek. Node ve ananas yanında gri tonlamalı ekran görüntüsü. (1 s)
6. **#code**: `AcidHazardVisual` + `AcidHazardSettingsSO` + `AcidHazardMath` (hız, dönüş öncesi mercek yanıp sönmesi). MPB, `RisoSlabVisual` kalıbında. (1.5 s)
7. **#test**: `AcidHazardMath` için EditMode testleri, derleme ve testler yeşil. (1 s)
8. **#code**: Prefab geçişi `unity command eval` ile: `MovingDeath`, `AccelDeath`, `RotatingDeath`, `WrappingDeath`. Sprite, materyal ve bileşen değişir, katman 8 korunur. `hazards.md` güncellenir. (1–1.5 s)
9. **#test (insan)**: Gamepad playtest, oda 4/13/16/19/20. (1 s)

Toplam ~11–12 saat, 9 task, günde 4 task ile 3 gün. Shuriken damlaları (`HalftoneTrailEmitter` kalıbı) `Someday`'e.

## Önce test edilecek teknik riskler

1. **Dolgulu mesh ve köşe dürüstlüğü:** 4×7 gibi düzgün olmayan ölçekte ve dönerken, yuvarlatılmış SDF köşesi keskin `BoxCollider2D` köşesinin içinde kalmamalı. Köşe yarıçapı kontur ve dalga payını geçmemeli. `edgeRadius`'un düzgün olmayan ölçekteki davranışı ayrıca doğrulanmalı.
2. **CRT çözünürlüğü kararı (H1):** 320×240 ile 480×360 arasında detay tabanı iki katı değişiyor. Bu karar gelmeden diken sayısı ve kontur kalınlığı sabitlenmemeli.
3. **`RotatingDeath` zinciri:** üç değen dairenin her biri kendi konturunu çizerse boncuk dikişleri oluşur. Tek bir kapsül renderer + `CapsuleCollider2D` mi, yoksa bilinçli bir "kolye" mi? Karar geliştiricinin, çünkü hitbox değişiyor.
4. **Lit'ten unlit'e geçiş:** `Sprite-Lit-Default`'tan çıkınca tehlikeler 2D ışığa tepki vermeyi bırakır. Işıklı odalarda kontrast yeniden kontrol edilmeli.
5. **Batching:** ~30 aktif örnekte `DisableBatching` + MPB, Frame Debugger'da doğrulanmalı (beklenti <50 draw call, sorunsuz).
