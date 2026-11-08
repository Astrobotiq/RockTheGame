# UI şekil çizim yöntemi: karar

Tarih: 2026-11-08. Kapsam: daire, halka, yay, çeyrek daire, dilim, çokgen. Proje uGUI + TMP (UI Toolkit yok).

## Kararı

- **Statik şekiller** (ana menü deliği, panel süsleri, ikon zemini): **PNG, 2x** (mockup'taki plan değişmedi).
- **Dinamik şekiller** (HUD sayacı, kırılan stamina halkası, radyal pause dilimleri): **`UiArc`**, kendi yazdığımız küçük uGUI `MaskableGraphic`.
- **Hazır paket alınmadı.** Aşağıdaki adaylardan hiçbiri bu projede kurulup denenmedi, hiçbirinin Unity 6'daki durumu kanıtlanmadı; ihtiyaç tek bir parametrik mesh ile karşılandı.

## İhtiyaç (mockup: `mashup-ui.html`)

Halka, yay, dilim (pause), çeyrek daire (sayaç), tam daire, üçgen (clip-path'li). Hepsi aynı geometri ailesi: **dış yarıçap, iç yarıçap, başlangıç açısı, süpürme açısı, kenar sayısı**. Üçgen/beşgen, 3/5 kenarlı dolu dilim olarak çıkıyor.

## Adaylar (arama sonuçlarından; hiçbiri denenmedi)

| Aday | Lisans (kaynakta yazan) | Kapsam | Not |
|---|---|---|---|
| [Unity UI Extensions](https://unity-ui-extensions.github.io/Controls/UICircle.html) (uGUI) | BSD-3-Clause | `UICircle` (yay, kalınlık), `UIPolygon` + çok sayıda ilgisiz kontrol | 3.0.0 "Unity 6" sürümü ama basın kitinde **pre-release**; UICircle/UIPolygon için Unity 6 testi bulunamadı. Tüm paket projeye girer |
| [UI Shapes Kit](https://github.com/thisotherthing/ui-shapes-kit) | MIT (atıf ister) | Prosedürel UI şekilleri | Unity 6 doğrulanmadı |
| [Unity-UI-Polygon](https://github.com/FeelsFeels/Unity-UI-Polygon) / [CiaccoDavide](https://awesome.ecosyste.ms/projects/github.com%2FCiaccoDavide%2FUnity-UI-Polygon) | MIT (yalnızca dizin sitesine göre) | Yalnızca çokgen | Halka/dilim yok |
| [eppz meshes](https://awesome.ecosyste.ms/projects/github.com%2Fgeri-borbas%2Funity.library.eppz.meshes) | MIT | Halka/daire mesh üretir | uGUI bileşeni değil, bağlama gerekir |
| UI Toolkit Vector API / Unity-SDF-UI-Toolkit | — | — | Proje uGUI; uygun değil |
| Asset Store (ör. Procedural Circular Progress Bar Pro) | Ücretli | İlerleme halkası | Denenmedi, lisans/boyut bilinmiyor |

Neden hazır paket değil: (1) ihtiyaç tek parametreli bir mesh ve ~100 satıra sığdı, (2) hiçbir aday Unity 6'da doğrulanmış değil, (3) lisans ve atıf yükü eklemek gerekmiyor, (4) UI Extensions'ın tamamını projeye almak istemiyoruz.

## `UiArc`

- Dosyalar: `New-Scripts/UI/Shapes/ArcMeshBuilder.cs` (saf geometri), `UiArc.cs` (bileşen, menü: RockTheGame/UI/Arc).
- Özellikler: `InnerRadiusRatio` (0 = dolu, 0.99'a kadar halka), `FillAmount` (0–1), `StartDegrees` (0 = 12 yönü, saat yönünde), `CounterClockwise`, `segmentsPerCircle` (3 = üçgen, 5 = beşgen), `FeatherPixels`. Renk `Graphic.color`. Dış yarıçap RectTransform'un kısa kenarının yarısı. `raycastTarget` varsayılan kapalı.
- Kenar yumuşatma: Overlay canvas MSAA görmediği için dış ve iç kenarda alfası 0'a inen 1 piksellik şerit var (`FeatherPixels`, canvas ölçeğine bölünür).
- Testler: `Tests/EditMode/ArcMeshBuilderTests.cs` (7 test: köşe/üçgen sayısı, geçerli indeksler, sıfır süpürme, kırpma, saat yönü, tüy halkası, yay segment oranı). EditMode 201/201.
- Görsel doğrulama: Play Mode'da geçici Overlay canvas ile halka, çeyrek daire, çeyrek halka, 4 aralıklı dilim (biri seçili), 6 parçalı halka (ikisi sönük), üçgen, beşgen, yarım halka çizildi; hepsi beklenen yerde ve biçimde çıktı (sahneye kaydedilmedi).

## Sınırlar ve bilinenler

1. **Kenar yumuşatmanın etkisi doğrulanmadı.** Yakalanan ekran görüntüsü 1280x720'ye küçültülmüştü, sert ve yumuşak kenar ayırt edilemedi. 1080p'de gözle bak; gerekirse `FeatherPixels` 1.5–2 yap ya da 0'a çek.
2. **Radyal kesim kenarları (dilimin yan kenarları) yumuşatılmıyor**, yalnızca dış/iç yay. Pause dilimleri arasındaki boşlukta bu sert kenar görünebilir.
3. **UV yok** (hepsi 0). Beton yüzey shader'ı ya da dokusu şekillere uygulanacaksa (TASKS: "Beton yüzey shader'ı ya da dokusu") önce UV eklenmeli ya da shader ekran/dünya koordinatından örneklemeli.
4. Dilim seçimi, Retry/Options davranışı ve pause menüsü bu task'ın dışında ("Radyal pause: dilim seçimi").
5. `capture_game_view --source screen` bu oturumda `--width/--height` verilince Overlay'i çizmedi, vermeyince 1280x720 verdi. Native çözünürlükte Overlay yakalamak için başka yol gerekebilir.
