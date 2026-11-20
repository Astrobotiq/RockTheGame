# Stamina göstergesi: karakter yanıp sönmesi (tasarım)

Tarih: 2026-11-20. Durum: kullanıcı sohbette onayladı. Boyut: küçük değişiklik.

## Amaç ve gerekçe

Kırılan stamina halkası (`HudStaminaRing`) karakterin etrafını kapatıyor; yeni tırmanma animasyonu halka yüzünden görünmüyor, karakter "kapana kısılmış" hissi veriyor. Referans: Celeste. Enerji bitmeye başlayınca karakterin kendisi yanıp söner, ek arayüz yoktur.

## Hedefler

- Karakterin etrafında ve önünde hiçbir arayüz öğesi kalmaz.
- GDD §5.2 "azalırken uyarı verir" karşılanır (uyarı görsel).
- `PlayerController` çağrıları değişmez; `WallClimbingState.ExitState`'e tek satır eklenir (duvardan inince flash dursun).

## Hedef dışı ve anti-hedefler

- Titreşim ve ses uyarısı (ayrı iş; `TriggerStaminaWarning` kancası ölü kod olarak duruyor).
- Kalan sürenin ilk ~4,5 sn'de gösterilmesi. Bilinen bedel: o aralıkta bilgi yok, Celeste ile aynı.
- Yeni bir HUD öğesi eklemek anti-hedeftir.

## Yaklaşımlar

Önce halka çevresinde 8 yerleşim denendi (blok dizisi, duvara gömülü sütun, kafa üstü şerit, ekran köşesi, duvar damgası, nokta-çizgi, aşınan eller, köşe dilimleri); hiçbiri seçilmedi. Kullanıcı arayüzsüz, karakterin kendisinin yanıp sönmesini önerdi ve seçti. Kandinsky şekil dönüşümü (daire→kare→üçgen) fikri kapsam dışı kaldı.

## Tasarım

- `HudStaminaRing`, `StaminaRingMath` ve testi silinir. `PlayerUIController` aynı genel API'yi korur (`ShowStaminaBar`, `UpdateStamina`, `RefillAndHideStaminaBar`), içinde oyuncu sprite'ını yanıp söndüren yeni bir bileşene yönlendirir.
- Eşik `PlayerStatsSO.StaminaWarningThreshold` (1,5 sn): altında karakter normal renk ile kırmızı/beyaz flash arasında gider gelir. Başlangıç önerisi 7 Hz, bitişe doğru ~12 Hz'e hızlanır. Hız ve renk "game feel": değerler Inspector'da/SO'da, gamepad ile ayarlanır.
- Yanıp sönme durur: duvardan inince, yere basınca, respawn'da, enerji dolunca. Unscaled zaman.
- Flash fazı saf bir fonksiyon (kalan süre → flash var/yok), EditMode testli.

## Kısıtlar

- Oyuncu sprite'ı `PlayerController.SpriteRenderer`. Malzeme flash rengini destekliyorsa onu kullan; desteklemiyorsa en yakın yolu seç ve nedenini yaz. Çarpımsal renk pembede soluk kalır.
- Başka sistemlerin sprite rengini (hitstop, ölüm) ezmemeli; bitişte rengi geri vermeli.
- Dokümanlar: `docs/systems/wall-movement.md` (halka bölümü), `docs/systems/ui-motion.md` (HUD stamina satırı) güncellenir, "Son kod doğrulaması" tarihi yenilenir.

## Uygulayıcıya kalanlar

Flash'ın teknik yolu (malzeme özelliği ya da renk), bileşenin yeri, hız eğrisinin tam biçimi, test dosyasının adı. Doğrulama: compile + EditMode yeşil; his kararı kullanıcıda.
