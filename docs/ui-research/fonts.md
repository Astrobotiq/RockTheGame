# UI fontları

Tarih: 2026-11-08. Mockup'taki geometrik başlık fontu (Jost benzeri) ve mono (Space Mono benzeri) hazırlandı. **Tiny5'in atlas'ına dokunulmadı.**

## Seçilenler

| Rol | Font | Dosya (`Assets/UI/Fonts/`) | TMP asset | Lisans |
|---|---|---|---|---|
| Başlık | Jost 800 (Heavy) | `Jost-800-Heavy.ttf` | `Jost-800-Heavy SDF.asset` | SIL OFL 1.1 (`Licenses/OFL-Jost.txt`) |
| Gövde / etiket | Space Mono Regular | `SpaceMono-Regular.ttf` | `SpaceMono-Regular SDF.asset` | SIL OFL 1.1 (`Licenses/OFL-SpaceMono.txt`) |
| Düğme / vurgu | Space Mono Bold | `SpaceMono-Bold.ttf` | `SpaceMono-Bold SDF.asset` | aynı |

`UiThemeSO.TitleFont`, `MonoFont`, `MonoBoldFont` bunlara atanmış.

## Kaynak ve lisans

- Jost: `github.com/indestructible-type/Jost`, release 3.5 (`Jost.zip`, 9,06 MB; projeye yalnızca `Jost-800-Heavy.ttf` alındı). Lisans metni `github.com/google/fonts/ofl/jost/OFL.txt`'ten ("Copyright 2020 The Jost Project Authors"). Google Fonts'taki Jost yalnızca değişken font (`Jost[wght].ttf`) ve Unity TMP değişken eksenleri kullanamadığı için statik 800 ağırlığı bu release'ten alındı.
- Space Mono: `github.com/google/fonts/ofl/spacemono/` (`SpaceMono-Regular.ttf` 99 KB, `SpaceMono-Bold.ttf` 98 KB, `OFL.txt`). "Copyright 2016 The Space Mono Project Authors".
- İki OFL.txt'te de "Reserved Font Name" bir isim olarak tanımlanmamış (yalnızca lisansın tanım cümlesi var). Fontlar değiştirilmeden kullanılıyor.
- **OFL yükümlülüğü:** fontlarla birlikte dağıtım yapıldığında lisans metni yanında olmalı. Lisans dosyaları projede (`Licenses/`); oyun içi krediler ya da build klasörüne eklenmesi yayın öncesi yapılacak iş.

## Doğrulama

- Türkçe glifler (ç Ç ğ Ğ ı İ ö Ö ş Ş ü Ü â î û) ve tüm ASCII (32–126), üç fontta da Unity'nin `FontEngine`'i ile doğrulandı. (Elle yazdığım `cmap` ayrıştırıcısı yanlış sonuç verdi ve güvenilmedi; kaynak olarak Unity'nin kendi motoru kullanıldı.)
- Play Mode'da geçici bir Overlay canvas'ta dört satır çizildi: "rock the game" (Jost 150), "ÇĞIİÖŞÜ çğıiöşü — oynat" (Jost 64), mono Regular ve Bold satırları. Türkçe harfler düzgün ve yumuşak çıktı. Sahneye kaydedilmedi.
- `UiThemeTests.ThemeAsset_FontsAreAssigned_AndCoverTurkishLetters` fontların atanmış ve Türkçe harfleri içermesini koruyor (test asmdef'ine `Unity.TextMeshPro` referansı eklendi).

## Atlas ayarları

SDF (SDFAA), statik; Jost için örnekleme 64, dolgu 8, atlas 2048; Space Mono için 48 / 6 / 1024. Her asset 205 karakter içeriyor: ASCII (32–126), Latin-1 Supplement (U+00A0–00FF), Ğ ğ İ ı Ş ş, tipografik noktalama (– — ‘ ’ “ ” … •). Tek atlas sayfası.

## Sınırlar

1. Statik atlas yalnızca bu 205 karakteri içerir. Başka bir karakter (ör. diğer Latin Extended-A harfleri, Kiril) gerekirse atlas yeniden doldurulmalı; yoksa karakter çizilmez.
2. `₺` ve `▶` bu fontlarda yok (ekleme denendi, atlandı). Oynat üçgeni için `UiArc` (3 kenarlı) kullanılabilir.
3. SDF ile çizilen küçük metin (Space Mono 44 px altı) gözle 1080p'de ayrıca bakılmalı; yalnızca 1280×720 yakalamaya baktım.
4. Jost 3.5 (2019) Google Fonts'taki güncel sürüm değil; Türkçe kapsamı doğrulandı ama başka glifler farklı olabilir.
5. Ana menü, HUD ve pause hâlâ eski fontları (Kenney / Tiny5) kullanıyor; geçiş ilgili task'larda.
