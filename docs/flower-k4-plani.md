# Çiçek node'u K4 "Kâğıt Pasiflora": uygulama planı

Tarih: 2026-11-22. Bu çiçeğin yalnızca **görünümü** değişir; döngü, tutulabilirlik ve fırlatma kuralları aynen kalır.

**Kaynaklar:** karar ve gerekçe [`ui-research/flower-node/oneri.md`](ui-research/flower-node/oneri.md), referans çizim [`ui-research/flower-node/mockup/K4-kagit-pasiflora.html`](ui-research/flower-node/mockup/K4-kagit-pasiflora.html) (SVG yaklaşımı, shader çıktısı değil), renk tabloları [`ui-research/flower-node/research/C-renk.md`](ui-research/flower-node/research/C-renk.md) §3 "P3 Porselen Kâğıt", şekil dayanağı `research/D-doga-bilim.md` tohum 1 (pasiflora), boya dayanağı `research/A-gelenek.md` (Matisse kâğıt kesmesi, baskı kayması).

## Alınan kararlar (2026-11-22, geliştirici)

| Konu | Karar |
|---|---|
| Yön | **K4 Kâğıt Pasiflora**. K1, K2, K3 elendi. |
| Palet | **P3 Porselen Kâğıt**. P4 İndigo Altın denenmeyecek. |
| Kontür | **Kesim gölgeli**: kontür yok, ofsetli koyu kopya var (kontürsüz seçenek elendi). |
| İplik | **Kalın 10+10**. İnce 16+12 elendi (96 px'te tüylü halka oluyor, CRT çizgileriyle yarışıyor). |
| Görünüm hedefi | **El işi kâğıt gibi** durmalı, **ama mevcut çiçekteki eğilme, bükülme ve rulo çözülmesi kaybolmamalı.** |

## Ana ilke: kâğıt görünümü fragment'ta, hareket vertex'te

Mevcut çiçeğin canlılığı `Flower/Petal` vertex shader'ındaki bükmeden gelir (boyuna eğrilik, rulo, enine sarılma; `_Pitch`, `_Reach`, `_Turn`, `_BaseCurl`, `_Roll`, `_Soft`). **Bu kod ve `FlowerBloom.Apply`'daki bu parametreleri süren hesap değişmez.** Kâğıt görünümü yalnızca boyadan gelir:

- düz renk alanlar, gradyan ve damar yok, parlama çizgisi yok;
- yaprağın orta çizgisinden iki yüz (katlama izi): bükülünce iki yüz farklı tona düşer, böylece kıvrım kâğıt kıvrımı gibi okunur;
- kontür yerine kesim gölgesi (ekran uzayında ofsetli koyu kopya);
- her yaprak biraz farklı kesilmiş (boy, en, uç asimetrisi; `Seed` ile).

**Doğrulama kuralı:** her shader task'ında FlowerTest sahnesinde aynı `open` değerleri (0, 0.3, 0.6, 1, 1.12) için yaprak silüetinin önceki ve sonraki çekimi karşılaştırılır; bükülme farklıysa durulur.

## Bozulmaması gerekenler

- `FlowerCycle`, `FlowerCycleMath`, `IGrabbableNode`, node collider'ı, itme/bırakma: dokunulmaz. `FlowerNodeRulesTests` (PlayMode) ve mevcut EditMode çiçek testleri değişmeden geçer.
- `FlowerBloom` genel API'si (`OpenBloom`, `CloseBloom`, `SetWarn`, `SetHint`, `Snap`, `Open`, `Warn01`) aynı kalır.
- Prefab `Node-Flower` kök ölçeği 4, `depthBehind` mantığı (çiçek kameradan uzağa itilir, oyuncu önde kalır), sorting layer `Default`: aynı.
- Rulo çözülme sırası (`FlowerBloomMath.PetalProgress`, altın oran sırası) kalır; tek katman olunca `layerDelay` etkisiz olur, sorun değil.
- Performans: bir odada zincir halinde birden çok çiçek olabilir (10 birim arayla). İplik tacı yaprak başına ayrı renderer yerine tek mesh olmalı (aşağıda). Önce/sonra Game view'da kare süresine bakılır.
- Oyuna özgü Unity kuralları: sahne/prefab/materyal işleri yalnızca `unity command eval` ile; `.unity` elle düzenlenmez. Kod `Assets/Scripts/New-Scripts/Flower/`, yorumlar Türkçe, yeni saf hesaplar EditMode testli.

## Hedef görünüm (mockup'tan)

- **Yapraklar:** 5 geniş yaprak, tek katman, her biri orta çizgiden iki yüzlü, dış kenarda indigo kesim gölgesi (`#303778`), gövde `#F4F1EA`/`#FAF9F6`, gölge yüzü `#CDD2E4`.
- **İplik tacı:** iki halka, halka başına 10 kalın şerit (uçları yuvarlak). Renkler halkanın içinden dışına: kobalt (`#1536E8`) → açık boşluk halkası (`#E5E9FF`) → indigo uç (`#303778`). Açılırken yelpaze gibi açılır, kapanırken büzülür.
- **Merkez kule:** koyu zemin üstünde 3 beyaz nokta ve çevrede 5 krem organ ucu (`#FFF3C4`). Açılışta pop yapar (mevcut pop yayı kalır).
- **Kapalı hal:** 5 parçalı dikişli tomurcuk, düz **koyu** gri-lacivert (`#4B4C58`; gökte 4.0:1, turuncu zeminde 3.3:1). Mockup'taki açık gri (`#6E6F78`) gökte 2.3:1 kalıyordu. **Karar (2026-11-22, geliştirici):** aday b seçildi; kobalt uç ipucu (a) ve mevcut gri (c) elendi. Koyu tomurcuk çiçek olduğunu söylemiyor, bunu açılış hareketi ve tutulamaz-taş okuması taşıyor; gamepad testinde (Ç9) "çiçek gibi mi" tekrar bakılır.
- **Uyarı:** renge güvenmez. Titreme + iplik tacı büzülmesi + kesim gölgesinin büyüyüp kayması + koyu kasalı kesikli amber halka (2 Hz). Eski turuncu tint kalkar (turuncu zeminde kayboluyor, renk körlüğünde asit yeşiline yakın).
- **Açılış halkası:** mevcut `Flower/Halo` kalır; rengi P3 halo tonuna (`#E5E9FF`) çekilir.

Palet (P3, `C-renk.md`): Hi `#FFFFFF`, Tip `#FAF9F6`, Base `#F4F1EA`, Shadow `#CDD2E4`, Deep `#5A6088`, Outline/kesim `#303778`, Glow `#FFF3C4`, Warn `#FFC94D`, Merkez `#1536E8`, Halo `#E5E9FF`, Kapalı `#4B4C58` (yaprak; ortadaki tomurcuk çekirdeği `#6E6F78` kaldı).

Başlangıç değerleri (his geliştiricinin kararı, gamepad/Game view ile ayarlanır): kesim gölgesi ~2–3 px (1080p), uyarıda ×2; yaprak varyasyonu boyda ±8 %, enda ±8 %; titreme genliği mevcut `trembleAmplitude`.

## Mockup uyumu (2026-11-22, Ç8'den önce)

Unity açık hali HTML mockup'ının açık haliyle FlowerTest'te ayni zeminde yan yana karşılaştırıldı ve ölçüler mockup'a çekildi (mockup R = 4.5 dünya birimi = 1 nesne birimi). Gözle benzedi; birebir değil (yaprak bükülmeleri bilerek kaldı).

- Yaprak: uç yarıçapı ~1.0 (`petalLength` 0.95), en 0.48, şekil 1.27 / 0.62, katlanan yüz 0.62, katlama izi ve kâğıt kenarı çizgisi kapalı (`_CreasePx`/`_EdgePx` 0).
- Yön: bir yaprak yukarı (`orientationDeg` −90); kule tepesi bu dönüşü iptal eder.
- İplik tacı: dış halka soluk taban + koyu uç bandı (`#E5E9FF` → `#303778`, %62), iç halka kobalt ve uyarıda amber; yatık duruş (10°), boy/en mockup oranlarında.
- Kule: koyu indigo disk, 3 büyük beyaz stigma, 5 krem organ ucu diskin dışında yaprak aralarına bakar; iplik şeritlerinin üstünde (`towerHeight`).
- Uyarı halkası: yarıçap 1.065, 16 dash, ince kasa; kesim gölgesi 4 px.
- **Bilerek farklı:** mockup'ta uyarıda yapraklar amber'e kayar, Unity'de kaymaz (Ç7 kararı: renk körlüğü ve turuncu zemin). Mockup gölgeleri yarı saydam (%30–34), Unity'de tek renk (shader opak).

## Task'lar

Her task bir commit, push yok. Her task sonunda derleme temiz, EditMode yeşil; çiçek koduna dokunan task'larda PlayMode `FlowerNodeRulesTests` yeşil.

### Ç1: Kâğıt yaprak boyası (`Flower/Petal` fragment)
- [ ] Fragment'tan kaldır: kontür, damar, parlama çizgisi, kenar ışığı, taban parıltısı, uç/taban 3 basamak gradyanı, iç/dış yüz solgunlaştırması. Vertex bloğuna (bükme/rulo) dokunma.
- [ ] Yerine: orta çizgi (`v = 0`) iki yüz; bükülmüş normalle 2 bant (ışık/gölge) aynı kalır, ama düz renk. Kenar boyunca ince açık "kâğıt kalınlığı" çizgisi (isteğe bağlı, `_EdgePx`).
- [ ] P3 renklerini `Mat_FlowerPetal.mat`'a yaz (`unity command eval`). Eski özellikler (`_VeinCount` vb.) shader'dan silinir; materyalde artık kalmaz.
- [ ] Doğrulama: FlowerTest'te `open` 0 / 0.3 / 0.6 / 1 / 1.12 çekimleri, bükülme önceki ile aynı. Çekimler `docs/ui-research/flower-node/` altına değil geçici yere; commit'e girmez.

### Ç2: Kesim gölgesi (ikinci pass)
- [ ] `Flower/Petal`'e ikinci pass: aynı mesh, vertex'te bükme aynen, clip uzayında sabit piksel ofseti (`_CutShadowPx`, uyarıda `_Warn` ile büyür, yön sağ-alt), tek renk `_CutShadowCol`, `ZWrite Off`, `ZTest LEqual`, çiçeğin zaten uzağa itilmiş derinliğine ufak bias.
- [ ] Gölge yalnızca altındaki yaprakların üstüne düşer (depth testi); en dıştaki yaprağın dışında gökyüzünde de görünür (mockup'taki gibi). Oyuncuyu etkilemez.
- [ ] Risk: sıralama. Yaprakların `sortingOrder`'ı sabit; gölge pass'ı kendi yaprağından önce çizilmeli. Sorun çıkarsa durup sor, kontür yerine eski çözüme dönme.
- [ ] Doğrulama: gökyüzü, turuncu zemin ve asit üstünde çekim; gri tonlama ve CRT'li çekim (`C-renk.md`: kenar kontrastı hedefi gökte ≥ 5:1).

### Ç3: Yaprak şekli ve elle kesim varyasyonu (+ test)
- [ ] `FlowerBloom` varsayılanları: `petalCount` 5, `layerCount` 1, yaprak şekli geniş (shader `_Shape`/`_Roundness`; mockup'taki yuvarlak-sivri uç), `petalLength`/`petalWidth` yeniden ayarlanır; çiçek çapı ~9 birim kalır.
- [ ] Saf `FlowerCutMath` (`Flower/`): `Seed` → boy çarpanı, en çarpanı, uç asimetrisi (hepsi `[1 − j, 1 + j]` aralığında, aynı seed aynı sonuç). Yaprak `Seed`'i zaten `FlowerBloom`'da var; `_Width`/`_Length` blok değerleri ve shader'a yeni `_TipSkew` buradan gelir.
- [ ] EditMode `FlowerCutMathTests`: sınırlar, determinizm, komşu yapraklar farklı, ortalama ≈ 1.
- [ ] Mevcut `FlowerBloomMathTests` (layerCount = 1 için de geçmeli) yeşil kalır.

### Ç4: İplik tacı shader'ı ve mesh'i
- [ ] Yeni `Flower/Corona` shader (`Assets/Shader/FlowerCorona.shader`): şerit mesh'i, yuvarlak uçlu şerit şekli (fragment SDF), halkaya göre boyuna 3 bant (kobalt / boşluk / indigo uç) için `_InnerCol`, `_GapCol`, `_TipCol`, aynı kesim gölgesi pass'ı, uyarıda ofset büyümesi.
- [ ] **Bükme kaybolmasın:** şeritler yaprakla aynı boyuna eğrilik modelini kullanır (`_Pitch`, `_BaseCurl`, `_Roll` küçük). Bükme kodunun yaprak ve iplik arasında kopyalanmaması için ortak `FlowerBend.cginc` çıkarılır; `Flower/Petal` çıktısı bu çıkarma ile **piksel olarak değişmemeli** (Ç1 çekimleriyle karşılaştır). Çıkarma riskliyse durup sor, ikinci seçenek: iplik için küçük ayrı bükme.
- [ ] `FlowerCoronaMesh.Build(stripsPerRing, ...)`: tek mesh, `uv2.x` = şerit indeksi; halka başına bir renderer (toplam 2 draw call, yaprak başına renderer yok). `FlowerPetalMesh` desenini izle, `bounds` büyük.
- [ ] EditMode `FlowerCoronaMeshTests` (köşe/üçgen sayısı, `uv2` aralığı, bounds), `FlowerPetalMeshTests` desenine benzer.

### Ç5: İplik tacını `FlowerBloom`'a bağla (+ test)
- [ ] `FlowerBloom`: iki halka, 10 + 10 şerit, dış halka uzun, iç halka kısa, iç halka yarım adım dönük. `Rebuild` bunları da üretir, `IsBuilt` bunları da kontrol eder; eski `Model` temizliği aynı.
- [ ] Saf `FlowerCoronaMath` (`Flower/`): şerit azimutları, halka ofseti, açıklığa göre yelpaze açısı (kapalı: şeritler yaprakların içinde toplu ve gizli, açık: yelpaze), uyarıda büzülme çarpanı, overshoot'ta hafif fazla açılma. Yaprak ilerlemesi (`PetalProgress`) ile aynı eşiği gecikmeli kullanır: iplik taç, yapraklar açılınca açılır.
- [ ] EditMode `FlowerCoronaMathTests`: kapalıda sıfır yelpaze, tam açıkta maksimum, monoton, uyarıda büzülür, azimutlar eşit aralıklı ve halkalar yarım adım kaymış.
- [ ] `FlowerCycle`/`SetWarn`/`Snap` çağrıları aynen çalışır (PlayMode `FlowerNodeRulesTests` yeşil).

### Ç6: Merkez kule (`Flower/Center`)
- [ ] Sarı kubbe yerine kule: koyu zemin diski, 3 beyaz nokta (ortada üçgen düzen), çevrede 5 krem organ ucu; kenarda kesim gölgesi ile aynı dil. `_RevealPop` (açılışta pop) ve `_Warn` (uyarıda nabız) mantığı `FlowerBloom`'dan aynen sürülür.
- [ ] Kapalıyken merkez tomurcuk çekirdeği olarak P3 "Kapalı" tonuna döner (mevcut `_Bud*` alanları yeniden adlandırılmadan renk değişir). Uyarıda çekirdek kobalttan amber'e döner (ikincil ipucu, asıl ipucu şekil/zaman).
- [ ] Çekim: kapalı, açılış, açık, uyarı.

### Ç7: Kapalı hal okunurluğu ve uyarı halkası (bitti, 2026-11-22; **uyarı halkası sonradan kaldırıldı**, geliştirici kararı)
- [x] **Kapalı hal:** üç aday çekildi (a kobalt uçlar, b koyu gövde, c mevcut gri), **b seçildi**: `Mat_FlowerPetal._ClosedCol` = `#4B4C58`. Seçilmeyen uç ipucu kodu (`_ClosedTipCol`/`_ClosedTipFrom`) shader'dan silindi.
- [x] **Uyarı:** `Flower/Petal`'dan turuncu tint ve parlaklık titreşimi kalktı; `_Warn` yalnızca titreme, iplik büzülmesi ve kesim gölgesi büyümesini sürer. Kesikli amber halka `Flower/Halo`'nun uyarı modu (`_Dashes > 0`: amber dash + koyu indigo kasa), 2 Hz, açılış dalgasıyla aynı renderer, hiç üst üste gelmez.
- [x] EditMode: `FlowerWarnMathTests` (yanıp sönme periyodu, 3 Hz kırpma, halka görünürlüğü); uyarı büzülme sınırları Ç5'teki `FlowerCoronaMathTests`'te.

### Ç8: Prefab, çekimler ve doküman
- [ ] `Node-Flower` prefab'ı ve `FlowerTest` sahnesi yeni materyal/ayarlarla (eval); oyuncunun ve dünyanın (gökyüzü, turuncu zemin, asit, pembe şerit) yanında gerçek Game view çekimi (CRT'li ve CRT'siz, gri tonlama).
- [ ] `docs/systems/grapple.md` çiçek bölümü güncellenir ("Son kod doğrulaması" tarihi dahil): yeni görünüm, kesim gölgesi, iplik tacı, uyarı dili. `docs/GDD.md` §8'e dokunma (ayrı karar).
- [ ] EditMode ve PlayMode tam çalıştırılır, çıktılar rapora yazılır.

### Ç9: Çiçek gamepad ve göz testi (#test)
- [ ] Oyuncuyla birlikte: karakterle ayrışma (tutunmuşken, yanında), açık/kapalı/uyarı 1–2 sn içinde okunuyor mu, kapalı hal çiçek gibi mi, 96 px'te iplik, kesim gölgesi kalınlığı, bükülmeler eski çiçek kadar canlı mı, zincir halinde kare süresi, CRT altında.
- [ ] Renk körlüğü: protan/deutan/tritan benzetimiyle çekim (sayfadaki anahtarlar yeterli olmazsa gerçek çekimle).
- [ ] Notlar `feel-notlari.md`'ye; ayarlanan değerler (gölge px, varyasyon, titreme) `FlowerBloom` alanlarında.

## Takvim

Önerilen dağılım `TASKS.md`'de: Ç1–Ç3 22 Kasım, Ç4–Ç5 23 Kasım, Ç6–Ç7 24 Kasım, Ç8–Ç9 25 Kasım; 26 Kasım tampon.

## Açık noktalar

1. Koyu kapalı tomurcuğun çiçek gibi okunması (Ç9 göz testi; kobalt uç ipucu elenmişti, gerekirse geri gelebilir).
2. Kesim gölgesinin sıralama davranışı (Ç2) ve `FlowerBend.cginc` çıkarmanın güvenliği (Ç4): ikisi de sorun çıkarırsa durulur ve sorulur.
3. Mockup SVG yaklaşımıydı; gerçek shader'da iplik şerit oranları, organ uçları ve halka renkleri gözle ayarlanır (başlangıç değeri).
4. Süreler (açılış 0.5, uyarı 0.7, snap 0.15) ve titreme bu işte değişmez; his değişikliği ayrı gamepad turu ister.
