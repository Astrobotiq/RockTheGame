# Beton yüzey: karar

Tarih: 2026-11-08. Karar: **(a) kodla üretilen uGUI shader'ı.** (b) CC0 doku denenmedi: aday ambientCG Concrete 034 (CC0 1.0, 3,7 MB) bulundu ama indirme istenmedi.

## Neden (a)

- Mockup'taki yüzey kurallarını (benek, kalıp çizgisi, tie-hole) denge değerine bağlı üretiyor; doku bunu veremezdi, her denge için ayrı resim gerekirdi.
- Sanat pikseli ızgarasına oturuyor (`_ArtPixel`), CRT dışı Overlay UI'da keskin kalıyor.
- UV istemiyor: `UiArc` gibi UV'siz mesh'lerde de çalışıyor.
- Dış dosya, lisans ya da build boyutu yükü yok.

(b) **karşılaştırılmadı**: (a)'nın (b)'den iyi olduğu ölçülmedi, yalnızca yukarıdaki gerekçelerle seçildi.

## Parçalar

- `Assets/Shader/UiConcrete.shader` (`RockTheGame/UI/Concrete`, `UI/Default` iskeleti: maske, kırpma, alfa kırpma çalışır).
- `Assets/Material/UI/M_UiConcrete.mat`, `UiThemeSO.ConcreteMaterial` (build'e girmesi için temada).
- `New-Scripts/UI/Theme/UiConcreteSurface.cs`: `Graphic`'e eklenir; denge, ton (`Wall`/`Slab`/`Mid`), boya (`paint`), `artPixel`, `seed`, `useBalanceOverride`/`balanceOverride`.
- Kurallar `UiBalance` eşikleriyle aynı: benek > 0,12; kalıp çizgisi > 0,4; tie-hole > 0,6 (yalnızca beton). Açık ton taban + 0,075, koyu ton taban − 0,082.
- Beton zemin rengi: `Lerp(Paper, ton, min(1, denge × 1,6))`.
- Testler: `UiConcreteSurfaceTests` (zemin rengi, tema materyali, `Apply` sonucu).

## Bilinenler

1. uGUI mesh'i shader'a **kök canvas uzayında** verir. Desen origin'i bu yüzden canvas uzayında hesaplanıyor ve eleman hareket edince `LateUpdate`'te güncelleniyor. Dönme ve ölçek desteklenmiyor (eksene hizalı varsayım).
2. `UiConcreteSurface` etkinleşince betonda `Graphic.color`'ı kendi hesapladığı renkle yazar; rengi bileşenden **sonra** ver. `paint` açıksa renge dokunmaz.
3. Boya yüzeyinde yalnızca benek var (yarı yoğunluk); renk sayısal doğrulandı, boyalı halka ekranda ayrıca görülmedi.
4. Görsel doğrulama Play Mode'da geçici canvas ile (sahneye kaydedilmedi): beş denge değeri, yuvarlak (UV'siz) beton, kalıp çizgisi ve tie-hole yakınlaştırması mockup'a uyuyor. Yakalama 1280×720 olduğundan 1080p'de gözle ayrıca bakılmalı.
5. Kenar yumuşatma (`UiArc` feather) ile beton kenarı birlikte 1080p'de denenmedi.
6. Hiçbir ekran henüz kullanmıyor; ana menü ve pause task'larında `UiConcreteSurface` takılacak.
