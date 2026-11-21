# Çiçek node'u: yeniden tasarım önerisi

Tarih: 2026-11-20. Dört araştırma raporunun birleşimi: [A-gelenek](research/A-gelenek.md), [B-oyunlar-hareket](research/B-oyunlar-hareket.md), [C-renk](research/C-renk.md) (+ [önizleme](research/C-renk-onizleme.html)), [D-doga-bilim](research/D-doga-bilim.md). Bu belge benim sentezim; hangi yönün oyuna yakıştığı geliştiricinin kararı. Hiçbir görsele Unity'de bakılmadı (Editor bağlı değildi); şekil önerileri metin kaynaklarına dayanıyor.

## Sorun neden var (ölçüldü)

- Çiçeğin gövde ve gölge rolleri karakterin `#FF51DE`'sinden yalnızca 13–19° (OKLCH) uzakta. Gölge karakterin kendisine ΔE00 10.8.
- Asıl neden hue değil **değer**: karakter L* 63.7, zemin 66.7, gökyüzü 72.6, asit 73.4. Dünyanın dört kimlik rengi 10 L* birimlik bir şeritte; mevcut çiçek gövdesi (Base L* 75.9, Shadow 60.7) tam o şeritte. Gri tonlamada çiçek gölgesi karakterle özdeş.
- Gökyüzü üstünde Base 1.10:1; silüeti yalnızca ince koyu çizgi taşıyor.
- Uyarı turuncusu `#FF9E61` turuncu zeminde kayboluyor (1.25:1).
- Dünya düzeyi ek bulgu: deuteranopide asit yeşili ile turuncu zemin ΔE00 1.8. Tehlike sinyali olan asit bu oyuncuda zemin gibi görünebilir (çiçekten bağımsız bir sorun).

**Kural:** çiçek yalnızca hue değiştirmemeli, o 10 birimlik değer şeridinin dışına çıkmalı (L* ≥ ~82 ya da ≤ ~50).

## Üç ortak ilke (raporların örtüştüğü yerler)

1. **Durumu silüet taşısın, renk değil.** Pikmin olgunluğu şekille okutuyor (B); İznik'te kapalı lale tomurcuğu, açık tırtıklı karanfil (A). Wonder Flower her dünyada renk değiştiriyor, kimliği davranış ve şekil taşıyor (B): çiçeği pembeden çıkarmak çiçeğin kimliğini bozmaz.
2. **Uyarıyı renkten ayır.** Amber uyarı protanopide asit yeşiline ΔE00 2–4 (8 paletin 6'sında) ve turuncu zeminde kayboluyor (C). Uyarı zaman/şekil kanalına yaslansın: titreme + ölçek + baskı kayması (A: dolgu ile kontürün kayması). Renk yalnızca ikincil ipucu.
3. **Kapanış gerçek bir snap olsun.** Venüs tuzağı eğrilikte elastik enerji biriktirip ~0,1 sn'de ters yüze döner (B ve D bağımsız olarak bulundu). Mevcut "titreme birikir, sonra snap" yapısı doğayla uyumlu. Uyarı süresi 0,7 sn; kaynaklardan çıkan tahmini alt sınır 0,5–0,6 sn (çıkarım, gamepad testi ister).

## Dört yön (şekil × boya × palet)

Palet eşleşmeleri değiştirilebilir; hex tabloları `C-renk.md` §3'te.

### K1 · Mühürlü Yıldız
- **Şekil (D tohum 9, Geastrum):** kapalı hali dikişli mühürlü küre; açılırken 5–7 sivri ışın dışa devrilir, ortada küre ve koni ağız.
- **Boya:** koyu mürekkep yapraklar, mavi-mor kenar ışığı, neon camgöbeği çekirdek.
- **Palet:** P6 Mürekkep Neon. Gökyüzüne 9.6:1, zemine 8.0:1; karakterden gri tonlamada ΔL* 31; asit yeşilinden en uzak.
- **Artı:** en özgün kapalı hal; snap kapanışa doğal uyar; SDF ve mevcut bükülebilir mesh ile yapılabilir.
- **Risk:** çiçek gibi okunmayabilir (bu bir artı da olabilir); koyu boru (ΔE ~13) ve koyu mavi halkaya yakın, "boru parçası" sanılabilir; kapalı-açık yaprak değeri yakın, durumu çekirdek ışığı taşımak zorunda; ekranın en "bayram şekeri olmayan" seçeneği.

### K2 · İznik Lale → Karanfil
- **Şekil (A):** kapalı = sivri lale tomurcuğu; açık = tırtıklı kenarlı karanfil. Siluet geçişi durumu tek başına söyler.
- **Boya:** düz renk, koyu mürekkep-lacivert dış çizgi, kontürlü çini hissi.
- **Palet:** P1 İznik Kobalt (kobalt gövde, nane-turkuaz uç, açık krem-sarı çekirdek). Hue farkı 74°, gökyüzüne 8.3:1 (çizgiyle), gri ΔL* 23. Üçü içinde sanatsal kimliği en güçlüsü.
- **Risk:** protanopide Base karaktere yalnızca 12.6 (değer farkı kurtarıyor); koyu gölgeler koyu mavi halkayla akraba; mavi gökyüzüyle karışmaması için dış çizgi şart; karanfil dişleri shader'da ek iş. Dişli kenar ve sakura çentiği gibi şekil ayrıntıları kaynakta yok, görsel bilgi.

### K3 · Vitray Venüs
- **Şekil (D tohum 3):** 4 böbrek lob, kenarda iri dişler, merkezde üç tetik direkçiği (tutunma noktasını işaretler); kapalı hal dişlerin fermuar gibi kilitlendiği haç; uyarıda loblar gerilip ters döner.
- **Boya (A, vitray/came):** cel-shade bantları cam parçalarına dönüşür, kurşun çizgiyle ayrılır; açıkken "arkadan ışık" parlaklığı.
- **Palet:** P3 Porselen Kâğıt'la eşleşir (kırık-beyaz cam, lavanta-gri gölge, kobalt çekirdek). Karaktere renk körlüğünde en az 15.1 ayrışma; kapalı-açık değer farkı ΔL* ~48.
- **Artı:** mekanikle birebir anlam uyumu (tutulurken kapanıp fırlatma); gerçek bir snap modeli.
- **Risk:** iç yüz kırmızıya kayarsa fırlatma pedleriyle karışır, kırmızıdan uzak tutulmalı; gökyüzü üstünde gövde 1.88:1, silüeti lacivert çizgi taşır; uyarıda krem-sarı kayması ince, ikincil ipucu şart.

### K4 · Kâğıt Pasiflora
- **Şekil (D tohum 1):** iki katlı kalın iplik tacı + 5 yaprak, ortada kule; kapalı = dikişli 5 parçalı sivri tomurcuk. Doğal "bullseye" halkaları merkezi işaretler.
- **Boya (A, Matisse kesmesi):** kontürsüz, el kesimi siluet; her yaprak biraz farklı. Shader'da en ucuz dil.
- **Palet:** P3 Porselen Kâğıt (ya da P4 İndigo Altın).
- **Artı:** çiçek hissini korur ama jenerik çiçekten en çok kopan seçenek.
- **Risk:** iplik tacı küçük detaylar yaratır, 9 birimde okunurluk denenmeli; kontürsüz bırakılırsa gökyüzü üstünde silüet zayıflar.

## Diğer araştırma bulguları (şimdilik kullanılmadı)
- Elenen paletler: P2 (vermilyon çekirdek kırmızı rezervini bozuyor), P4 (çekirdek protanopide asit yeşiline ΔE00 10.9), P5 (gökyüzü ailesi, 1.7:1), P7 (asit/halka sarısıyla çakışıyor), P8 (gölgeler teal halkayla aynı, ΔE00 3.9).
- Murakami tarzı yüzlü merkez: çiçek NPC ya da düşman gibi okunabilir, ayrı karar ister.
- Diğer tohumlar: sürahi ağzı, çan çiçeği, nilüfer, edelweiss, protea, deniz anemonu, bugenvil (D).

## Önerilen sıra
1. **Önce statik mockup.** K1 ve K2'yi SVG/HTML'de, sahne renkleri ve karakterle yan yana çiz. Shader'a dokunmadan karar verilir.
2. **Sonra yalnızca renk testi.** Seçilen paleti `Flower/Petal` materyal özelliklerine yaz (kod değişikliği yok), Unity'de oynayarak bak.
3. **En son şekil.** Mesh/shader değişikliği (`FlowerPetalMesh`, `FlowerBloom`), gamepad testiyle.

## Sınırlar
- Oyun içi görüntü alınmadı; CRT modeli kaba (yalnızca mertebe veriyor).
- Zemin kenarı pembesi `#E040C0` moodboard'dan gözle alındı.
- Gerçek renk körü oyuncu testi yapılmadı.
- Hareket (uyarı titreşimi, halo) ve gerçek shader çıktısı değerlendirilmedi.
- Süre ve overshoot sayıları tahmin; game feel geliştiricinin kararı.
