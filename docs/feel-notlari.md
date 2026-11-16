# Oyun hissi (game feel) günlüğü

Hissi hiçbir test ölçmez; yalnızca gamepad'le oynayan kişi değerlendirebilir. Bu dosya, "neden bu değer?" sorusunun cevabını ve her tuning değişikliğinin sonucunu kaydeder. AI ajanları hissi etkileyen bir değeri (çoğunlukla `PlayerStatsSO`) değiştirdiğinde buraya eski/yeni değeri yazar, oyuncu da oynadıktan sonra sonucu ekler.

`PlayerStatsSanityTests` yalnızca GDD bantlarını korur. Bant dışına çıkmak bilinçliyse önce `docs/GDD.md`, sonra o testi güncelle.

## Nasıl girilir

| Tarih | Değer (SO alanı) | Eski → Yeni | Neden | Oynayınca sonuç |
|---|---|---|---|---|
| *(YYYY-AA-GG)* | *(`AlanAdı`)* | *(eski → yeni)* | *(neden)* | *(oyuncu doldurur: iyi / kötü / karışık)* |
| 2026-11-16 | `CeilingCornerCorrection` / `DashCornerCorrection` (yeni) | yok (0) → 0.5 → 0.25 | Celeste köşe düzeltmesi; 0.5 gamepad'de çok kaygan geldi | 0.5: kötü (kaygan). 0.25: *denenmedi* |
| 2026-11-16 | `WallJumpCheckDistance` (yeni) | yok (temas şart) → 0.375 | Havadan duvar zıplaması, Celeste 3 px (PF-5) | *denenmedi* |
| 2026-11-16 | Slingshot çıkış yerçekimi (`SlingshotState`, alan değil) | tuşa bağlı (A basılı: 1× + apex hang, değil: 3×) → hep 3× yükselirken, 1.5× düşerken (tek kanca bırakışıyla aynı) | Geliştirici: slingshot'tan sonra tuşu basılı tutup uzun havada kalmak saçma. A basılı tutan oyuncunun dikey slingshot'u kısalır | iyi (2026-11-16) |
| 2026-11-16 | `AirOverspeedReduce` (yeni) | 40 (`AirAcceleration`) → 16 | Hız sınırın (10) üstündeyken stick aynı yöndeyse; eskiden ileri basmak bırakmaktan (20) hızlı söndürüyordu. Celeste oranı 0.4 × ivme. Yere inişte hız hâlâ anında 10'a kesiliyor (bilinçli) | iyi (2026-11-16) |
| 2026-11-16 | `CelesteVariableJump` / `VarJumpTime` (yeni, deneme) | kapalı (tuşu bırakınca yükselişte 3×) → açılınca: tuş basılıyken 0.2 sn yukarı hız sabit, bırakınca 1×, tepede asılı kalma yalnızca tuş basılıyken | Geliştirici Celeste'in değişken zıplamasını denemek istedi. Tam yükseklik aynı kalsın diye başlangıç hızı düşüyor (yerden 16 → 10.8; duvar 11 → 6.3, tırmanma 14 → 9.0). Boost'lu yüzey, kanca çıkışları ve dash değişmez. Varsayılan kapalı; Inspector'dan aç-kapa | "güzel olmuş" (ilk deneme); kalıcı olup olmayacağı karar verilmedi |
| 2026-11-16 | Duvardan ayrılınca coyote (`WallSlidingState`, alan değil) | 0.1 sn coyote (dikey zıplama) → yok; hemen zıplamayı havadan duvar zıplaması karşılar | Geliştirici: tırmanmıyorken duvarda zıplayınca bazen düz yukarı zıplıyor. Kaynak coyote'un duvar zıplamasından önce gelmesiydi (W9) | iyi (2026-11-16) |

## Notlar

*(Oynarken fark ettiğin, henüz değer değişikliğine dönüşmemiş his notları.)*

- **Kancadan çıkış yerçekimi (karar: böyle kalır, 16 Kasım 2026):** Tek kanca bırakılınca yükselirken hep 3×, düşerken 1.5×. Çift kanca bırakılınca sabit 1×. Slingshot çıkışı o gün tuşa bağlıydı (basılıysa 1×, değilse 3×). Tek kurala indirme önerisi reddedildi: geliştirici bu hale emek vererek getirdi; kural tutarlı olur ama his ve oynanış açısından doğru gelmiyor. "Swing'den ya da slingshot'tan sonra zıplama tuşunu basılı tutup daha uzun havada kalmak saçma" (geliştirici). Bunun üzerine slingshot çıkışı tuştan bağımsız yapıldı, artık tek kanca bırakışıyla aynı (yukarıdaki tablo).
