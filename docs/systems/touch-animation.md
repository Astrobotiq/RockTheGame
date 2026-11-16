# Temasta animasyon (TouchAnimationTrigger)

## Tasarım niyeti
Dekor nesneleri (tabela, zincir, çalı...) oyuncu dokununca tek seferlik sallanır. Her nesne için ayrı kod yazılmaz; aynı bileşen takılır.

## Nasıl çalışır
- Kod: `Assets/Scripts/New-Scripts/Animation/TouchAnimationTrigger.cs` (`New_Scripts.Animation`).
- Nesnede trigger `Collider2D` + `Animator` + bu bileşen olur. Animator controller'ında parametre veya geçiş gerekmez.
- Başlangıçta animator ilk karede donar (`speed = 0`), yani klip kendiliğinden oynamaz.
- `PlayerController` taşıyan collider tetikleyiciye girince state baştan oynar, `cycles` tur sonra ilk poza dönüp tekrar donar.
- `Play()` public: UnityEvent veya başka koddan elle de tetiklenebilir.
- Oda uyuyup uyanınca (`OnEnable`) dinlenme pozuna sıfırlanır.

## Tunable'lar (inspector)
| Alan | Varsayılan | Anlamı |
|---|---|---|
| `animator` | boş | Boşsa nesnede, sonra çocuklarda aranır |
| `stateName` | boş | Boşsa controller'ın varsayılan state'i |
| `cycles` | 1 | Klip kaç tur oynasın |
| `restartOnRetouch` | kapalı | Oynarken yeniden dokunulursa baştan başlasın mı |

## Kullanım
1. Nesneye trigger `BoxCollider2D` ekle (sprite'ı kaplasın).
2. `TouchAnimationTrigger` ekle. Animator aynı nesnede veya çocuktaysa başka bir şey atamaya gerek yok.
3. Şu an kullanan: Oda 5 `Jump-sign` (`RoomNew-5 Variant.prefab`).

## Bilinen kenar durumlar
- `Animator` zaman ölçeğine bağlı: swing tutorial'daki yavaşlamada sallanma da yavaşlar.
- Tetik yalnızca giriş anında çalışır (`OnTriggerEnter2D`); içeride durmak tekrar oynatmaz.
- Tek katman (layer 0) varsayılır.

Son kod doğrulaması: 2026-11-15
