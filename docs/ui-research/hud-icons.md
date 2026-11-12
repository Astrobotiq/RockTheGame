# HUD ikon seti

Tarih: 2026-11-09. Sekiz ikon, `Assets/UI/Icons/`: `hud_pineapple`, `hud_key`, `hud_dash`, `hud_sling` ve her birinin `_used` hâli.

## Kurallar

- **Piksel, 24x24, 1x.** Sanat ızgarası 480x270; Canvas büyütür (1080p'de 4x). "SVG, 2x PNG" planından vazgeçildi: kenar yumuşatma piksel fontla ve dünyayla çelişir, SVG içe aktarıcı sorunu da devre dışı kaldı.
- **Boyalı = renk, boyasız (`_used`) = çıplak beton.** Ananas turuncu, anahtar sarı, dash kırmızı `#FC1E00`, sling mavi `#597DCE`; boyasız hâl hepsinde düz beton grisi `#9A9791`.
- **Brutalist dil (mockup `mass()`):** 1 px ink dış çizgi, sağ alta 2 px sert ink gölge, küp ve silindirde yüzler arası ink kenar, yüzler aynı düz renk (gölge tonlaması yok). Beton benek dokusu denendi, beğenilmedi, kaldırıldı.
- **Import:** Sprite (Single), Point filtre, PPU 16, mipmap yok, sıkıştırmasız.

## Açık noktalar

- **Anahtar:** Mockup'ta anahtar ikonu yok, üç yuvalı disk sırası (`ANAHTAR 2/3`) var. Burada bir anahtar silueti çizildi; HUD kodu hangisini kullanacak, karar verilmedi.
- **Ölçekleme:** `NewScene`'deki canvas'lar "sabit piksel" modunda (`m_UiScaleMode: 0`). Kesirli ölçekte (1440p 5,33x, 720p 2,67x) piksel genişlikleri eşit çıkmaz. HUD'a bağlanırken tam sayı ölçek düşünülmeli.
- **Okunurluk:** Gamepad mesafesinde dört ikonun ayırt edilmesi oyunda gözle doğrulanmadı.
- Üretim betiği repoda yok (Python/PIL ile tek seferlik üretildi); PNG'ler kaynak.
