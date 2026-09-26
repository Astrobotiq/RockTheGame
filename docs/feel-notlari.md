# Oyun hissi (game feel) günlüğü

Hissi hiçbir test ölçmez; yalnızca gamepad'le oynayan kişi değerlendirebilir. Bu dosya, "neden bu değer?" sorusunun cevabını ve her tuning değişikliğinin sonucunu kaydeder. AI ajanları hissi etkileyen bir değeri (çoğunlukla `PlayerStatsSO`) değiştirdiğinde buraya eski/yeni değeri yazar, oyuncu da oynadıktan sonra sonucu ekler.

`PlayerStatsSanityTests` yalnızca GDD bantlarını korur. Bant dışına çıkmak bilinçliyse önce `docs/GDD.md`, sonra o testi güncelle.

## Nasıl girilir

| Tarih | Değer (SO alanı) | Eski → Yeni | Neden | Oynayınca sonuç |
|---|---|---|---|---|
| *(YYYY-AA-GG)* | *(`AlanAdı`)* | *(eski → yeni)* | *(neden)* | *(oyuncu doldurur: iyi / kötü / karışık)* |

## Notlar

*(Örn: "yerden swing yalnızca sağ tetikle çalışıyor" bilinçli mi eksik mi, GDD §12 madde 6.)*
