# Oyun hissi (game feel) günlüğü

Hissi hiçbir test ölçmez; yalnızca gamepad'le oynayan kişi değerlendirebilir. Bu dosya, "neden bu değer?" sorusunun cevabını ve her tuning değişikliğinin sonucunu kaydeder. AI ajanları hissi etkileyen bir değeri (çoğunlukla `PlayerStatsSO`) değiştirdiğinde buraya eski/yeni değeri yazar, oyuncu da oynadıktan sonra sonucu ekler.

`PlayerStatsSanityTests` yalnızca GDD bantlarını korur. Bant dışına çıkmak bilinçliyse önce `docs/GDD.md`, sonra o testi güncelle.

## Nasıl girilir

| Tarih | Değer (SO alanı) | Eski → Yeni | Neden | Oynayınca sonuç |
|---|---|---|---|---|
| *(YYYY-AA-GG)* | *(`AlanAdı`)* | *(eski → yeni)* | *(neden)* | *(oyuncu doldurur: iyi / kötü / karışık)* |
| 2026-11-16 | `CeilingCornerCorrection` / `DashCornerCorrection` (yeni) | yok (0) → 0.5 → 0.25 | Celeste köşe düzeltmesi; 0.5 gamepad'de çok kaygan geldi | 0.5: kötü (kaygan). 0.25: *denenmedi* |
| 2026-11-16 | `WallJumpCheckDistance` (yeni) | yok (temas şart) → 0.375 | Havadan duvar zıplaması, Celeste 3 px (PF-5) | *denenmedi* |
| 2026-11-16 | `AirOverspeedReduce` (yeni) | 40 (`AirAcceleration`) → 16 | Hız sınırın (10) üstündeyken stick aynı yöndeyse; eskiden ileri basmak bırakmaktan (20) hızlı söndürüyordu. Celeste oranı 0.4 × ivme. Yere inişte hız hâlâ anında 10'a kesiliyor (bilinçli) | *denenmedi* |

## Notlar

*(Oynarken fark ettiğin, henüz değer değişikliğine dönüşmemiş his notları.)*

- **Kancadan çıkış yerçekimi (karar: böyle kalır, 16 Kasım 2026):** Tek kanca bırakılınca yükselirken hep 3×, düşerken 1.5×. Çift kanca bırakılınca sabit 1×. Slingshot çıkışında zıplama tuşuna bağlı (basılıysa 1×, değilse 3×). Tek kurala indirme önerisi reddedildi: geliştirici bu hale emek vererek getirdi; kural tutarlı olur ama his ve oynanış açısından doğru gelmiyor. "Swing'den ya da slingshot'tan sonra zıplama tuşunu basılı tutup daha uzun havada kalmak saçma" (geliştirici). Not: slingshot çıkışı bugün tam da tuşa bağlı; bunun da değişmesi istenip istenmediği sorulacak.
