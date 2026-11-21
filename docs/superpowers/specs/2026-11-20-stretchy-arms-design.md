# Uzayan kollar (kancanın yerine) — kısa tasarım

Tarih: 2026-11-20 · Durum: onay bekliyor

## Amaç ve neden

Karakterin şu an kolu yok gibi: iki kare sprite stick yönüne dönüyor, kanca ayrı bir ip (LineRenderer) olarak çiziliyor. İstenen, Astro Bot'taki maymun kollarına benzer bir his: kanca yok, **kolun kendisi node'a kadar uzanıp tutunuyor**. Boştayken kol normal boyunda, gevşek ve yarı kontrolsüz; stick'e göre "bir şey arıyormuş gibi" omuzdan eğri çizerek o yöne uzanıyor.

## Kullanıcının belirledikleri

- Kol omuzda **ince**, uca doğru **kalınlaşır**, ucunda bir **el** var. Kalınlaşma özellikle uzayınca belli olur.
- Kolun rengi **gövdeyle aynı**.
- Tetik yokken: el normal boyda, stick'in gösterdiği yere omuzdan eğri çizerek gider (bugünkü yön mantığı). Kontrolü tam değilmiş gibi, gevşek sallanır.
- Tetik basılınca: kol node'a kadar uzanır. İp çizimi kalkar.
- Yaklaşım: **A — eğri + yay** (kullanıcı seçti).

## Hedefler / hedef olmayanlar / başarısızlık sayılanlar

- **Hedef:** yalnızca görsel değişiklik. Tutunma anlık kalır, fizik, durumlar ve anchor mantığı değişmez.
- **Hedef değil:** kolun duvar ya da zeminle çarpışması (kol geometrinin içinden geçebilir), yeni mekanik, el animasyonu çizimi.
- **Başarısızlık:** el node'a geç varıyor, karakter elden önce sallanmaya başlamış görünüyor; kol titriyor ya da hızlı salınımda kopuk duruyor; stick yönü artık okunmuyor.

## Yaklaşımlar

| | Yaklaşım | Artı | Eksi |
|---|---|---|---|
| **A (seçildi)** | Omuz→el arası eğri (Bezier) + elin hedefe yay ile gitmesi + eğrinin ortası geriden gelir | Tam kontrol, titreme riski düşük, birkaç sayıyla ayarlanır | "Fizik" gerçek değil, sallantı taklit |
| B | Etek kodundaki gibi Verlet zinciri | En doğal sallantı | Nişan alma ve dümdüz gerilme zor, taşma ve titreme, uzun ayar |
| C | 2D Animation kemikli sprite kol | — | Çizim gerekiyor, çok uzayınca bozuluyor |

## Tasarım (A)

**Parçalar**
- Yeni bileşen `ArmVisual` (`Player/Visual/`), her kola bir tane. Mevcut `LeftRope`/`RightRope` LineRenderer'larını kol gövdesi olarak kullanır, uca bir el `SpriteRenderer`'ı ekler.
- `ArmController.UpdateArmRotation(dir)` çağrıları durumlarda **olduğu gibi kalır**. Yalnızca kareyi döndürmek yerine nişan yönünü saklar (`AimDirection`). Kare sprite'lar kapatılır.
- `GrappleVisualManager`'ın ip çizimi kalkar. Anchor'ı `ArmVisual` kendisi okur (`LeftAnchor`/`RightAnchor`). `TargetVisualizer` değişmez.
- Ayarlar yeni bir `ArmVisualSettingsSO`'da durur (SO-per-system deseni).

**Boşta (tetik yok)**
- El hedefi: `omuz + AimDirection × boştaUzanma` (≈ bugünkü kol boyu, ~0.6 birim). Stick bırakılmışsa el aşağı sarkar ve gövdenin hızının biraz gerisinde kalır.
- El hedefe **yaylı** gider: sönümü düşük tutulur, hafif taşar. "Kontrol edemiyor ama arıyor" hissi buradan gelir.
- Eğrinin kontrol noktası (kolun ortası) ayrı ve daha gevşek bir yayla izler. Kol omuzdan kıvrılarak gider ve hareket ettikçe sallanır.

**Tutunma (anchor var)**
- El, `UzanmaSüresi` (~0.1 sn, ease-out) içinde node'a fırlar ve orada kilitlenir. Hareketli node'da anchor'ı izler.
- Kol uzadıkça kavis azalır, gerilip neredeyse düzleşir.
- Kalınlık: omuzda ince, elde kalın (LineRenderer genişlik eğrisi).

**Bırakma**
- El yayla boştaki hedefine geri döner. Kısa bir taşma ve "lastik" geri dönüşü olur.

**Renk**
- Kol ve el gövde renginde. **Önerim (kullanıcı onaylamadı):** slingshot hakkı bitince yalnızca **el** "tükendi" rengine döner. `PlayerColorController.armRenderers` el sprite'larını gösterir, kol çizgisi gövde renginde kalır.

**Zaman**
- `Time.deltaTime` ile çalışır, böylece hitstop'ta ve oda geçişinde kollar da donar.

## Uygulayıcıya kalanlar (benim kararlarım, düzeltilebilir)

- Çizim sırası: iki kol da gövdenin arkasında, el gövdenin önünde (karar uygulamada ekran görüntüsüyle verilir).
- El sprite'ı şimdilik basit bir yer tutucu (yuvarlak ya da eldiven şekli). Asıl çizim kullanıcının.
- Yay matematiği saf bir yardımcıya (`ArmSpringMath`) konur ve EditMode testi yazılır.
- Başlangıç değerleri: yay sertliği, sönüm, orta nokta gecikmesi, boşta uzanma 0.6, uzanma süresi 0.1 sn, kalınlık omuz 0.08 → el 0.2. His kullanıcının kararı: gamepad ile denenip ayarlanır.
- `docs/systems/grapple.md` güncellenir (görsel kısmı ve "Son kod doğrulaması").
