# UI paleti: kaynak sprite'lardan örneklenen değerler

Tarih: 2026-11-08. Mockup'taki (`mashup-ui.html`) hex'ler CRT'li ekran görüntülerinden gözle alınmıştı. Burada **CRT'siz kaynak PNG'lerin piksellerinden** sayıldı (PowerShell `System.Drawing`, yalnızca opak pikseller, tam renk eşleşmesi, yüzde = opak piksel payı). Gökyüzü sprite değil, `Main Camera` arka plan rengi.

## Önerilen palet

| Rol | Öneri | Kaynak | Mockup'taki | Not |
|---|---|---|---|---|
| Mürekkep (ink, metin/çerçeve) | `#140C1C` | `plums.png` en koyu renk (%46,9), boru kenar çizgisi | `#151515` | Kaynak saf siyah değil, mor-siyah |
| Boru gövdesi / sönük metin | `#442434` | `plums.png` (%6,9) | `#433646` (CRT'li göz) | `#433646` hiçbir sprite'ta yok; en yakınları `#442434` ve `#4E4A4E` |
| Sönük gri (muted) | `#4E4A4E` | `plums.png` (%9,8) | `#4a4844` | Nötr koyu gri |
| Kâğıt | `#F1E6C7` | `RampTex_10Rings.png` krem halka | `#f2ebd3`, `#F5EFCB` | Mockup'a çok yakın. Daha açık vurgu: `#FAF7CD` (`Orange.png`) |
| Kırmızı | `#FC1E00` | `RampTex_10Rings.png` halka kırmızısı | `#e8432b`, `#FF4300` | Mockup'taki `#e8432b` ≈ `Platform_Breakable.png` kırmızısı `#EA4335`. Halka kırmızısı `#FC1E00`'dır |
| Sarı | `#FCAE04` | `RampTex_10Rings.png` halka sarısı | `#f6be1f`, `#FFB11A` | Limon tonu istersen `#E1CA33` (`Orange.png`, zemin) |
| Mavi (boyalı halka) | `#597DCE` | `plums.png` açık mavi (%2,5) | `#2d5fa8`, `#3A6EA5` | Koyu karşılığı `#30346D` (`plums.png`, %7,6; halka rampında `#362D70`). `#4285F3` yalnızca `Platform_Breakable.png`'de |
| Gökyüzü | `#71BAEA` | `Main Camera` arka plan | — | UI zemini olarak kullanılırsa dünyayla aynı |
| Teal (halka) | `#289AA4` | `RampTex_10Rings.png` | — | Derin teal `#04586C` |
| Pembe (oyuncu) | `#FF51DE` | `Candy/Run.png` (%29,4) | — | Gölge `#C73FAD`, koyu `#97338A` |

## Beton tonları: kaynakta yok

Oyunda beton yüzey yok, o yüzden "beton" tonları örneklenemedi. En yakın gri tonlar:

| Ton | Hex | Kaynak |
|---|---|---|
| Açık, soğuk | `#AFBBC3` | `Terrain Sliced (16x16).png` taş (%4,0) |
| Orta, soğuk | `#858D99` | `Terrain Sliced (16x16).png` (%4,0), `plums.png` `#8595A1` |
| Orta, sıcak | `#757161` | `plums.png` (%3,9) |
| Koyu | `#4E4A4E` | `plums.png` |

Mockup'un sıcak gri tonları (`--wall #c9c6bf`, `--slab #e2dfd8`, `--mid #9a9791`) kaynak değil, el ile seçilmiş. Seçenekler: (a) mockup'unkini koru, (b) `#757161` ve kâğıt tonu arasında türetilmiş tonlar kullan (ör. `lerp(#F1E6C7, #757161, 0,35) = #C6BDA3`), (c) soğuk taş grileri (`#AFBBC3`, `#858D99`). **Karar (2026-11-08): (a), mockup'unkini koru.** Beton tonları: duvar `#c9c6bf`, levha `#e2dfd8`, orta `#9a9791`. Bunlar kaynaktan örneklenmedi, el ile seçildi; "Beton yüzey shader'ı ya da dokusu" task'ında yüzey bu tonlara göre kurulacak. Sönük gri (muted) bu karara dahil değil, yukarıdaki tabloda `#4E4A4E` önerisi duruyor.

## Kaynak PNG'lerin tam dökümü (en sık 10 renk)

| Dosya | Renkler |
|---|---|
| `Pixel Adventure 1/.../Terrain Sliced (16x16).png` (46 renk) | `#211F30` %27,0 · `#A14744` · `#B96E54` · `#CF9E6E` · `#E44A4A` · `#E68D41` · `#E1CA33` · `#858D99` · `#AFBBC3` · `#843346` |
| `Sprites/Pipe/plums.png` (16 renk) | `#140C1C` %46,9 · `#4E4A4E` · `#30346D` · `#442434` · `#854C30` · `#757161` · `#D27D2C` · `#8595A1` · `#597DCE` · `#D2AA99` |
| `Sprites/Textures/RampTex_10Rings.png` (10 renk) | `#FF5104` · `#888C03` · `#289AA4` · `#F1E6C7` · `#04586C` · `#362D70` · `#FC1E00` · `#EEEAC7` · `#EE7284` · `#FCAE04` |
| `Sprites/top-ring.png`, `bottom-ring.png` | `#C89A31` · `#D2B33E` · `#D2C165` · `#9D744C` (+ siyah); yüzük sprite'ı gökkuşağı rampından ayrı, altın/kahve |
| `Sprites/Platform/Platform_Breakable.png` | `#EA4335` %56 · `#FBBC05` %30 · `#34A853` %12 · `#4285F3` %2 |
| `Pixel Adventure 1/.../Pineapple.png` | `#E68D41` · `#D2612F` · `#211F30` · `#F2BC7E` · `#72A11D` · `#28860F` · `#076029` |
| `Pixel Adventure 1/.../Orange.png` | `#211F30` · `#D2612F` · `#E68D41` · `#E1CA33` · `#F7EC8A` · `#FBDFB1` · `#FAF7CD` |
| `Sprites/Candy/Run.png` | `#FF51DE` · `#FF6736` · `#FFF157` · `#97338A` · `#CE532B` · `#C73FAD` · `#E25B2F` |

`plums.png` ve Pixel Adventure paletleri iyi bilinen bir 32 renklik piksel paletiyle örtüşüyor (`#140C1C`, `#442434`, `#30346D`, `#4E4A4E`, `#854C30`, `#757161`, `#597DCE`, `#D27D2C`, `#8595A1`, `#D2AA99`). `Platform_Breakable.png` ise Google renkleri (`#EA4335`, `#FBBC05`, `#34A853`, `#4285F3`): bu sprite büyük olasılıkla yer tutucu; kırmızı/sarı/mavi için ondan değer alma, halka rampını ve `plums.png`'yi kullan.

## CRT farkı: UI dünyadan daha koyu görünecek

CRT içindeki dünya, CRT dışındaki UI'dan parlak çıkar. Aynı rolün CRT'li ölçümüyle kaynağı (farklı pikseller, kaba karşılaştırma):

| Rol | Kaynak | CRT'li ölçüm (mockup/supervisor) |
|---|---|---|
| Gökyüzü | `#71BAEA` | `#84D2F6` – `#92DFFF` |
| Halka kırmızısı | `#FC1E00` | `#FF4300` |
| Halka sarısı | `#FCAE04` | `#FFB11A` |

Koyu kanallar yaklaşık +15…+35 yükseliyor. UI'da kaynak değerleri kullanınca ekranın yanındaki dünya biraz soluk, UI biraz koyu/doygun kalır. Bu, "denge 55" ayarında gözle düzeltilecek bir fark; paleti CRT'ye göre kaydırma.

## Sınırlar

1. Pikseller tek tek sayıldı, ama "UI'da hangi rol hangi renk" eşlemesi benim önerim. Gerçek ekranda gözle bakılmadı.
2. Beton tonları kaynaktan örneklenemedi (yukarıda).
3. Mockup (`mashup-ui.html`) güncellenmedi; hâlâ eski hex'leri taşıyor. Paleti `UiThemeSO` task'ında koda alırken bu dosya referans alınmalı.
4. CRT farkı için eşleşen piksel çiftleri sayılmadı; yalnızca üç rolün kaba karşılaştırması var.

## Kodda: `UiThemeSO`

Palet, fontlar, çerçeve kalınlığı, gölge ofseti ve denge `Assets/Resources/UI/UiTheme.asset`'te (`UiThemeSO`, `UiThemeSO.Load()`; `MultiplayerUISettingsSO` ile aynı desen). Varsayılanlar bu dosyadaki kararlar: ink `#140C1C`, kâğıt `#F1E6C7`, kırmızı `#FC1E00`, sarı `#FCAE04`, mavi `#597DCE`, koyu mavi `#30346D`, gökyüzü `#71BAEA`, sönük `#4E4A4E`, beton `#C9C6BF` / `#E2DFD8` / `#9A9791`.

- **Denge:** `Balance` = 0,55 (0 Bauhaus, 1 brutalist); `ScreenOverrides` ile ekran başına (`MainMenu`, `Pause`, `Hud`, `RoomCard`, `MultiplayerResult`). `BalanceFor(screen)`, `BorderWidthFor`, `ShadowOffsetFor`, `ExtrusionDepthFor`, `ConcreteSurfaceFor`.
- **Formüller** `UiBalance`'ta, mockup'tan (`bwOf`, `shOf`, `mass`): çerçeve `taban*(0,35+0,65*v)` (v < 0,06'da yok), gölge `round(taban*v*1,6)`, ekstrüzyon `round(taban*v*1,5)`, beton karışımı `min(1, v*1,6)`. Doku v > 0,12, tehlike çizgileri v ≥ 0,3, kalıp çizgileri v > 0,4, tie-hole v > 0,6. Birim sanat pikseli (480x270); 4x ekran çevirmesi çağıranda. Testler: `UiThemeTests`.
- **Fontlar atanmış:** `TitleFont` (Jost 800), `MonoFont` / `MonoBoldFont` (Space Mono), bkz. `fonts.md`.
- **Henüz kimse okumuyor:** Ana menü, HUD ve pause hâlâ eski sabitleri kullanıyor; `UiThemeSO`'ya bağlamak ilgili task'larda.
