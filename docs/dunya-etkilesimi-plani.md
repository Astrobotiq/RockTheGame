# Dünya etkileşimi: node halkası kırpma, halka birleşmesi, asit çekimi

> **Uygulayan için:** task task uygulanır (`superpowers:executing-plans` ya da `superpowers:subagent-driven-development`). Önce bu dokümanın tamamını, sonra ilgili sistem dokümanını oku: halkalar için [`systems/grapple.md`](systems/grapple.md), asit için [`systems/hazards.md`](systems/hazards.md) ve [`asit-tepki-plani.md`](asit-tepki-plani.md) (B aşaması, AK2/AK3/B7). Üç bölüm birbirinden bağımsız; sıra: **K → A → B**. Branch önerisi: `feature/world-interaction`.

**Kaynak:** geliştiricinin 2026-11-23 isteği (su damlasının yüzeyden ayrılırken yaptığı boyun görüntüsü). Araştırma aynı gün üç ayrı araştırmayla yapıldı; bulgular aşağıda özetlenmiştir.

Hepsi **yalnızca görsel**: oyun kuralları, öldürme alanları, collider'lar, oyuncu fiziği değişmez. Bütün "his" değerleri (`k`, mesafe, yükseklik, hız) geliştiricinin gamepad testine kalır (CLAUDE.md kural 4).

## Ortak teknik: dünyayı shader'a vermek

Shader sahneyi göremez; ona C#'tan veri verilir. Proje bunu zaten yapıyor: `AcidReaction`, temas noktasını `MaterialPropertyBlock` ile `_AcidImpact` olarak yazıyor (`GetPropertyBlock` → değiştir → `SetPropertyBlock`, yaşı CPU sayar). Bu plandaki her şey aynı kalıp:

- Veri `MaterialPropertyBlock` ile yazılır, materyal kopyalanmaz. `FullRotationNode`'un kendi materyal kopyası (`_materialInstance`, `_OuterRadius`) ile çakışmaz: farklı property'ler.
- Node shader'ları `CGPROGRAM`, CBUFFER yok, SRP Batcher zaten devre dışı; MPB bir şey kaybettirmez.
- MPB dizileri: dizi boyutu ilk `SetVectorArray` çağrısında kilitlenir. **Her zaman sabit boyutta** dizi gönder (boş eleman `z = 0`).
- Zaman: shader `_Time` kullanmaz, yaşları C# sayar. `HitStopEvents.IsActive` iken yaş ilerlemez.

## Kararlar

| Konu | Karar | Kimin |
|---|---|---|
| Halkanın komşu odaya taşması | Oda dünya dikdörtgeninde shader kırpması (`_RoomClipRect`) | Claude önerisi, geliştirici onayı (2026-11-23) |
| Stencil / SpriteMask | Kullanılmaz: yalnızca duvar karolarını gizler, duvarın arkasındaki oda alanı yine görünür | Araştırma |
| 2D ışık + gölge | Kullanılmaz: halkalar unlit, Unity 6'da tilemap gölge kaynağı doğrulanamadı, maliyet orantısız | Araştırma |
| Halka birleşmesi | Quad başına smooth-min (Quilez polinom `smin`), Voronoi sahipliği, renk karışımı smin ağırlığından | Claude önerisi |
| Spiral node'lar birleşmeye | **Katılmaz** (ilk sürüm). Faz merkeze bağlı, boyunda dikiş olur. Kırpmaya katılır. | Claude önerisi |
| Render texture metaball pass | Kullanılmaz: global pipeline işi, halo başına ramp/progress kaybolur | Araştırma |
| Asit çekimi | Havuz kabarır + kabuk sarkar + aradaki köprü ayrı quad, kopmada damla emit | Claude önerisi |
| Köprünün stili | Havuzun piksel ızgarasında (16 PPU, `floor`), havuz renkleri, konturlu değil. Kabuk pürüzsüz kalır. | Claude önerisi, testte bakılır |
| Köprü öldürür mü | Hayır. Öldüren yalnızca mevcut `DeathZone`'lar. Okunurluk testte sorulur. | Claude önerisi |

## Bölüm K: oda sınırında kırpma

**Sorun:** normal node halkası `Assets/Prefab/New/Node/Circle-BG.prefab` (ölçek 20, `Assets/Shader/UI_CircularRings.mat`, `CircularRings.shader`). Background katmanı, order -2: duvarların (Platform, order 0–2) **arkasında** çiziliyor, yani duvarın üstüne binmiyor. Yarıçap duvar kalınlığından büyük olduğu için duvarın ötesindeki boş alana, yani komşu odaya taşıyor (oda 12'nin node'u oda 11'de görünüyor).

**Oda sınırı:** `Room.RoomBounds` bir `Collider2D` (`RoomNew.prefab`'da oda kökünde `BoxCollider2D`). `roomBounds.bounds` dünya dikdörtgeni. Node'lar odanın altında: `GetComponentInParent<Room>()`.

### K1 (DE1): Oda kırpması

Dosyalar:
- `Assets/Shader/CircularRings.shader`, `CircularRotationRing.shader`, `SpiralNode.shader`, `SpiralNodeReversing.shader`
  - Property: `_RoomClipRect ("Room Clip Rect (world minX,minY,maxX,maxY)", Vector) = (-100000,-100000,100000,100000)`. Varsayılan dev dikdörtgen: script yoksa hiçbir şey kırpılmaz.
  - Property: `_RoomClipSoftness ("Room Clip Softness (world)", Float) = 0.25`. Kenarda sert kesim yerine kısa sönme.
  - Vertex: `o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;`
  - Fragment, son alfaya çarpan: `float2 inside = min(p - _RoomClipRect.xy, _RoomClipRect.zw - p); float m = saturate(min(inside.x, inside.y) / max(_RoomClipSoftness, 1e-4)); col.a *= m;` (premultiplied değil, `Blend SrcAlpha OneMinusSrcAlpha`; shader'ın blend'ine bak, premultiplied ise `col *= m`).
- Yeni `Assets/Scripts/New-Scripts/Player/Nodes/RoomClippedRenderer.cs` (`[RequireComponent(typeof(Renderer))]`):
  - `Start`'ta `GetComponentInParent<Room>()`, `RoomBounds.bounds` → `Vector4(min.x, min.y, max.x, max.y)`, MPB Get→Set.
  - Oda yoksa ya da `RoomBounds` null ise bir kez `Debug.LogWarning` ve hiçbir şey yazma.
  - Node hareket etse de oda değişmez; her karede yazmaya gerek yok. `OnEnable`'da yeniden yaz (oda uyanınca MPB korunur ama güvenli olsun).
  - `SpiralNode.cs` zaten MPB yazıyor: **Get→Set** kalıbı şart, yoksa birbirinin değerini siler. Sıra bağımsız olsun diye ikisi de önce `GetPropertyBlock` yapmalı. Gerekirse `SpiralNode.cs`'in MPB'yi sıfırdan oluşturmadığını kontrol et.
- Bileşeni prefab'lara ekle (`unity command eval`, `PrefabUtility.LoadPrefabContents` → `AddComponent` → `SaveAsPrefabAsset`): `Circle-BG.prefab` (varyant miras alır), `Node-Spiral.prefab`, `Node-SpiralReversing.prefab`, `Node-Moving.prefab`. Bileşen halka renderer'ının olduğu objeye gider (root değilse çocuğa).
- **Önce kontrol:** bütün `RoomNew-* Variant` prefab'larında `roomBounds` atanmış mı. Atanmamış varsa listele ve kullanıcıya sor, kendin atama.
- `docs/systems/grapple.md`: kısa "Oda kırpması" paragrafı + "Son kod doğrulaması" tarihi.

Kabul:
- Derleme temiz, EditMode yeşil.
- Play Mode ya da Edit Mode kamera çekimi: oda 11'de oda 12'nin halkası görünmüyor; oda 12'de halka oda kenarında yumuşak sönüyor. Önce/sonra iki görüntü.
- Saf hesap (dikdörtgen → maske) shader'da; C# tarafında test edilecek deterministik mantık yok, test yazma.

### K2 (DE2): Kırpma gözle kontrol (#test)

22 odayı sırayla gez: hiçbir halka komşu odada görünmüyor; oda kenarına yakın node'larda halka mantıksız erken kesilmiyor (kesilme oyuncunun bakış alanında rahatsız ediyorsa `_RoomClipSoftness` artırılır); dönen node'un progress halkası ve spiral'ler bozulmadı.

## Bölüm A: hareketli tehlike ile asit havuzu arasında çekim

**Mevcut altyapı (yeniden kullanılır):**
- `AcidHazard`: `_PixelScale` (dünya boyutu × 16 PPU) yazar, `PixelScale` olarak verir.
- `AcidReaction`: tek darbe yuvası `_AcidImpact` = (x px, y px, yaş, güç); `IResettable`; `DebugImpact` kalıbı deterministik kare için.
- `AcidImpactMath`: `LocalToPixel`, `SurfaceContact`, `CrestHeadroom`.
- `PixelArtAcid.shader`: `surfaceHeight = base + wave + min(AcidCrater + AcidRipple, headroom)`, yüzeyin üstü `discard`. Ortak kod `AcidImpact.cginc` (Gauss çukur, dalga, kaynama, flaş).
- `HazardAcid.shader`: pürüzsüz SDF (taban şekil − wobble − diken), collider'dan büyük 1.5 birimlik quad. MPB okumaz.

**Kritik kısıt: tepe payı.** Havuz quad'ında yüzeyin üstündeki boşluk oda 2/3/12/18'de ≈3.4 px, oda 22'de ≈0.2 px. Kabarma bunu aşamaz (`min(..., headroom)`). Bu yüzden asıl uzanan şekil **ayrı köprü quad'ı**dır; havuz kabarması küçük ve tepe payıyla kırpılır.

**Kapsam riski:** hangi odalarda bir hareketli tehlikenin bir havuzun yanından geçtiği taranmadı. **A1'in ilk adımı bu tarama.**

### A1 (DE3): Oda taraması, yakınlık kaydı, havuz kabarması

1. **Tarama** (`unity command eval`): her odada `DeathZone` + `HazardAcid` materyalli renderer'lar ile `AcidReaction`'lı havuzlar; her çift için tehlikenin yolu (strateji uç noktaları ya da yörünge) ile havuz yüzeyi arasındaki en kısa mesafe. Tabloyu raporla. **Hiçbir odada 3 birimden yakın çift yoksa dur ve kullanıcıya sor** (seçenekler: test odası kurmak, kapsamı yalnızca kabarma + sıçramaya indirmek).
2. Yeni `Assets/Scripts/New-Scripts/Death/AcidAttractor.cs`: hareketli tehlikeye eklenen işaretçi; statik `IReadOnlyList<AcidAttractor> Active` (OnEnable/OnDisable kayıt). Kendi `Collider2D`'sini ve renderer'ını verir. Trigger kullanılmaz (iki taraf da trigger, kinematik sürülüyor; katman 8'e dokunmak kenardan çıkma kuralını etkiler).
3. `AcidReaction`: `Update`'te `AcidAttractor.Active` içinden en yakını: önce genişletilmiş `bounds` AABB testi, sonra `ClosestPoint` ile mesafe. Eşik dışında iş yapma.
   - Yeni yuva `_AcidAttract` = (x px, y px, yakınlık 0..1, mesafe px). Yakınlık = `1 - saturate((d - contactDist) / (attractDist - contactDist))`, eğri `SmoothStep`.
   - Saf hesap `AcidImpactMath`'e: `AttractStrength(distance, attractDist, contactDist)`.
4. `AcidReactionSettingsSO`: `attractDistance` (başlangıç 2.5 birim), `contactDistance` (0.3), `bulgeHeightPx` (3), `bulgeWidthPx` (6).
5. `AcidImpact.cginc`: `AcidAttractBulge(px, attract)` = Gauss tepe (çukurun ters işaretlisi). `PixelArtAcid.shader`: `surfaceHeight`'e eklenir, aynı `min(..., headroom)` kırpması içinde.
6. EditMode test: `AttractStrength` sınırları (eşik dışında 0, temasta 1, monoton). Tablo testi, mevcut `AcidImpactMath` testlerinin yanına.

Kabul: derleme temiz, EditMode yeşil; Play Mode'da tehlike yaklaşırken iki kare (uzak / yakın) çekimi, yüzeyde tehlikenin altında kabarma.

### A2 (DE4): Kabuk sarkması

- `HazardAcid.shader`: MPB `_HazardAttract` = (yerel yön xy, güç, —). SDF'te `d -= _DripAmount * strength * pow(max(0, dot(n, dir)), _DripSharpness);` burada `n` = merkezden piksele normalize yön. Quad 1.5 birim; sarkma quad'ı aşmasın, `_DripAmount` başlangıç 0.12 birim, kırp.
- Bu, asit planındaki B7 "açıya bağlı çukur" ile **aynı fonksiyon yeri, ters işaret**. B7 daha önce yapıldıysa onun yönlü bloğunu yeniden kullan; yapılmadıysa fonksiyonu B7'nin de kullanabileceği genellikte yaz (işaretli genlik).
- `AcidReaction` en yakın tehlikeyi bildiği için ona yazar: tehlikenin renderer'ına MPB (yön = havuz yüzeyindeki en yakın noktaya doğru, tehlikenin yerel uzayında; `RotatingDeath` döndüğü için yerel uzay şart). Birden fazla havuz aynı tehlikeye yazarsa en güçlüsü kazanır: `AcidAttractor` kare başında sıfırlar, havuzlar `max` ile yazar, `LateUpdate`'te `AcidAttractor` uygular.

Kabul: derleme temiz, EditMode yeşil; dönen tehlikede sarkma her açıda havuza doğru (iki farklı açıda çekim).

### A3 (DE5): Köprü

- Yeni `Assets/Shader/AcidBridge.shader` (Sprites/Custom altında): dikey kapsül/trapez SDF. Alt genişlik `wBottom`, boyun `wNeck`, üst `wTop`; `_PixelScale` ile aynı ızgarada `floor` (kare piksel, 16 PPU). Renk: havuzun `_AcidColor`/`_FoamColor` değerleri (materyalden kopyalanmaz, `AcidBridge` bileşeni havuz materyalinden okuyup MPB ile yazar). Kontur yok.
- Yeni `Assets/Scripts/New-Scripts/Death/AcidBridge.cs`: havuzun çocuğu, ilk ihtiyaçta bir kez oluşturulur (`AcidReaction`'ın `impactFxPrefab` kalıbı). Her kare: alt uç = havuz yüzeyindeki en yakın nokta (yüzey yüksekliği, piksel ızgarasına oturtulmuş), üst uç = tehlike kabuğunun alt kenarı. Quad bu iki nokta arasına konumlanır/ölçeklenir.
- Görünme: yakınlık bir eşiği (`bridgeStart`, başlangıç 0.7) geçince boyun 0'dan büyür.
- Sorting: havuz ile aynı katman, havuzun hemen önünde; tehlikenin arkasında (kabuk köprünün ucunu örtsün, stil dikişi kabuk konturunda kalsın).

Kabul: derleme temiz, EditMode yeşil; Play Mode çekiminde köprü havuzdan tehlikeye kadar kesintisiz, kare piksel, tehlikenin konturunda bitiyor.

### A4 (DE6): Kopma, hit-stop, reset, test

- Saf hesap `AcidImpactMath.BridgeNeckWidth(stretch, ...)`: köprü uzadıkça boyun incelir (hacim korunumu kabaca: `wNeck ∝ 1/sqrt(length)`); `wNeck < breakWidth` olunca kopma.
- Kopma anı: köprü kapanır, `FX_AcidImpact` üzerinden boyun noktasında tek damla `Emit` (mevcut parçacık sistemi, yeni FX yok); havuz yüzeyine küçük bir `_AcidImpact` darbesi (damla düşüşü). İsteğe bağlı küçük cue: mevcut asit seslerinden biri, düşük ses; yoksa ekleme.
- Hit-stop: `HitStopEvents.IsActive` iken boyun yaşı ve kopma zamanlayıcısı ilerlemez (konum zaten durur).
- `IResettable.ResetToDefault`: köprü kapalı, yaş sıfır, `_AcidAttract` ve tehlike MPB'si temiz. Tehlike başa ışınlandığında aynı karede sahte bir kopma/damla oluşmamalı (reset karesinde kopma bastırılır).
- EditMode test: `BridgeNeckWidth` monoton azalır, kopma eşiği; reset sonrası durum sıfır (saf yapı üzerinde).
- `docs/systems/hazards.md`: "Çekim ve köprü" bölümü + tarih; `asit-tepki-plani.md`: AK3 ve B7 durumu.

Kabul: derleme temiz, EditMode ve PlayMode (`bozulmamali-davranislar` testleri) yeşil; uzaklaşma sırasında boyun incelir, kopar, damla düşer (3 karelik çekim).

### A5 (DE7): Asit çekimi gamepad testi (#test)

Sorular: çekim mesafesi doğru mu (çok erken / geç); kabarma ve köprü okunuyor mu; pürüzsüz kabuk + pikselli köprü birleşimi göze batıyor mu; köprü "burası da öldürür mü" kafa karışıklığı yaratıyor mu (görsel collider'dan büyük görünüyor mu); boyun inceleme hızı ve kopma; hit-stop'ta donma hissi; oda 22'de kabarma neredeyse sıfır, kabul mü yoksa quad büyütülsün mü. Değerler `feel-notlari.md`'ye.

## Bölüm B: halkaların birbirine bağlanması

**Shader yapısı:** `CircularRings` ve `CircularRotationRing` UV uzayında `dist = length(uv - _Center)`; `_InnerRadius`/`_OuterRadius` dışı erken `return 0`; `normalizedRadius` → `floor(nr * _RingCount)` halka indeksi → `_RampTex`; `_OuterFadeStart`/`_OuterFadePower` sönme. RotationRing ek olarak `atan2` ile `_Progress`. **Birleşme tek bir yere oturur:** `dist`'in `normalizedRadius`'a çevrildiği an. Gerisi aynen kalır.

**Beklenen görüntü:** `smin(d1, d2, k)`'nin eş-mesafe çizgileri: iç halkalar ayrı daireler kalır; yaklaştıkça dış halkalar önce ince bir boyunla bağlanır, sonra yer fıstığı şekline kaynar. Birleşme bölgesinde mesafe en fazla `k/4` küçülür, halkalar boyunda biraz sıkışır: `k` küçük başlar.

**Tuzaklar:**
- Mesafe UV yerine **dünya biriminde ve işaretli** olmalı (`length(p - c) - R`), yoksa farklı boyutlu node'lar yanlış birleşir.
- Boyun iki dairenin dışında oluşur: quad vertex'te `_Pad` (başlangıç 1.3) ile büyütülür ve UV buna göre ölçeklenir, inspector değerleri değişmez. SpriteRenderer culling bounds'u büyümez; ekran kenarında küçük kırpılma kabul.
- Örtüşen bölgeyi iki quad çizmesin: **Voronoi sahipliği**, piksel kendi node'una komşudan daha uzaksa `discard`. Eşitlikte kimliğe göre karar (ör. `_SelfId < _NbrId`) ki dikiş çizgisinde iki kez ya da hiç çizilmesin.
- K bölümündeki `_RoomClipRect` aynen kalır; birleşme kırpmadan önce hesaplanır.
- `SimulatedNodeVelocity` node'u hareket ettirmez. Gerçekten hareket eden node'lar `Node-Moving` ve hareketli platforma parent edilmiş olanlar.

Çekirdek (taslak; uygulayan uyarlar):

```hlsl
float4 _Nbr[4];      // xy = dünya merkezi, z = dış yarıçap (0 = boş), w = kimlik
float4 _NbrTint[4];
float4 _Self;        // xy merkez, z dış yarıçap (dünya), w = kimlik
float _K, _RingWidthW, _TintMix;

float smin(float a, float b, float k, out float h) {
    h = saturate(0.5 + 0.5 * (b - a) / k);
    return lerp(b, a, h) - k * h * (1.0 - h);
}
// frag:
float2 p = IN.worldPos;
float dS = length(p - _Self.xy) - _Self.z;
float d = dS; float3 tint = 0; float wsum = 0;
[unroll] for (int i = 0; i < 4; i++) {
    if (_Nbr[i].z <= 0) continue;
    float dn = length(p - _Nbr[i].xy) - _Nbr[i].z;
    if (dn < dS || (dn == dS && _Nbr[i].w < _Self.w)) discard;
    float h; d = smin(d, dn, _K, h);
    tint += _NbrTint[i].rgb * (1 - h); wsum += 1 - h;
}
float normalizedRadius = 1.0 + d / _RingWidthW;   // eski akış buradan
// ... halka indeksi / ramp / fade aynen; sonra:
// col.rgb = lerp(col.rgb, tint / max(wsum, 1e-4), saturate(wsum) * _TintMix);
```

`_RingWidthW` = dünya biriminde (`_OuterRadius - _InnerRadius`) × quad ölçeği; `_InnerRadius` > 0 ise iç sınır da işaretli mesafeye çevrilir.

### B1 (DE8): Halka shader'larında dünya mesafesi + smin

- `CircularRings.shader`, `CircularRotationRing.shader`: `worldPos`, `_Pad`, `_Self`, `_Nbr[4]`, `_NbrTint[4]`, `_K` (başlangıç 0.6 birim), `_TintMix` (0.5), smin, sahiplik. Komşu yokken (`_Nbr` boş) görüntü **piksel piksel eskisiyle aynı** olmalı: `_Pad` UV ölçeklemesi ve dünya mesafesine geçiş yalnızca matematik eşdeğerliği; önce/sonra çekimiyle doğrula.
- RotationRing'de `_Progress` açısı kendi merkezinden kalır.
- Spiral shader'larına dokunma.

Kabul: derleme temiz, EditMode yeşil; komşusuz bir node'un önce/sonra çekimi aynı; elle MPB ile iki node'a birbirini komşu yazınca boyun görünüyor (`unity command eval` ile geçici yazım, sahneye kaydetme).

### B2 (DE9): `NodeHaloLink` bileşeni

- Yeni `Assets/Scripts/New-Scripts/Player/Nodes/NodeHaloLink.cs` (≈80 satır): statik kayıt listesi (OnEnable/OnDisable), halka renderer'ı, dünya yarıçapı (renderer bounds ya da `_OuterRadius` × ölçek), kendi rengi (ramp'in dış rengi ya da inspector), kimlik (`GetInstanceID`).
- Yönetim: tek bir statik `LateUpdate` sürücüsü yok; her bileşen kendi `LateUpdate`'inde yalnızca kendi komşularını bulur: listede aynı `Room`'da olanlar ve `dist < rA + rB + k` olanlar, en yakın 4. Statik node'lar için sonuç önbelleğe alınır, yalnızca kendisi ya da bir komşusu `transform.hasChanged` olduğunda yeniden hesaplanır.
- MPB Get→Set (K1'deki `RoomClippedRenderer` ve `SpiralNode` ile aynı blok; birbirinin değerini silmesin). Dizi her zaman 4 eleman.
- Prefab'lar: `Circle-BG.prefab`, `Circle-BG-For-Rotation Variant.prefab`, `Node-Moving.prefab` (`unity command eval`). Spiral prefab'larına **eklenmez**.
- Saf hesap (`SelectNeighbours(selfPos, selfR, candidates, k, max=4)`) ayrı statik fonksiyon + EditMode testi: uzaktaki dahil edilmez, en yakın 4 seçilir, kendisi dahil edilmez, farklı odadaki dahil edilmez.

Kabul: derleme temiz, EditMode yeşil; çekim: yan yana iki node bağlı, uzak node etkilenmiyor, `Node-Moving` yaklaşıp uzaklaşırken boyun oluşup kopuyor (iki kare).

### B3 (DE10): Test odası ve doküman (#level)

- Mevcut bir test odasında (yoksa `RoomNew` kopyası, sahnede yalnızca test için) üç düzen: iki eş node üst üste binen, farklı boyutlu iki node, `Node-Moving` sabit bir node'un yanından geçen. Ayrıca bir spiral'in normal node'a değdiği düzen (spiral katılmıyor, dikiş kabul edilebilir mi diye).
- Oyundaki odalarda halkaları üst üste binen node çiftlerini listele (eval ile) ki testte bakılsın.
- `docs/systems/grapple.md`: "Halka birleşmesi" paragrafı + tarih.

### B4 (DE11): Halka birleşmesi gamepad testi (#test)

Sorular: boyun damla gibi okunuyor mu; `k` ne kadar yapışkan olmalı; renk karışımı (`_TintMix`) çok mu az/çok; halkaların boyunda sıkışması rahatsız ediyor mu; hareketli node'da kopma anı güzel mi; spiral'in katılmaması göze batıyor mu (batıyorsa ayrı task: yalnızca dış sönme bandının birleşmesi); okunurluk: iki node birleşik görünürken oyuncu hâlâ iki ayrı tutunma noktası olduğunu anlıyor mu. Değerler `feel-notlari.md`'ye.

## Hedef dışı

- Halkanın aynı odadaki duvarın içinde gizlenmesi (stencil). K2'de istenirse ayrı task.
- Spiral node'ların birleşmesi.
- Render texture / tam ekran metaball pass'i.
- Köprünün öldürmesi ya da yeni collider.
- Akan asit (`FlowingPixelAcid`) için çekim; ilk sürüm yalnızca `Acid.prefab`.
