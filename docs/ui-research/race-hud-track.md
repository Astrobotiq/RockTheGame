# Yarış HUD'u: pist + mercek (bayrak sayacı)

*Karar: 2026-11-13. Kullanıcı üç yönden (disk, pist, direk) **pisti** seçti, 100 oda için de **mercek** çözümünü onayladı. Mockup'lar sohbette çizildi (repo'da HTML yok); aşağıdaki değerler o mockup'lardan alındı. Mevcut kod: `Multiplayer/Hud/RaceHud.cs` (sağ üstte düz metin "Bayrak N/3", "Rakip: Oda X · Bayrak N/3"), `RaceHudText.cs`. GDD §9 kuralı: 3 bayrak toplayan rakibini 1 oda geri gönderir, sayacı sıfırlanır.*

## Neden pist

- İki oyuncunun ilişkisini gösteriyor: kim önde, kaç oda fark var. Eski iki ayrı kutu bunu göstermiyordu.
- Oyunun tek etkileşimi olan ceza (rakip bir oda geri) pistte görünür bir kayma olarak okunuyor.

## Yerleşim

Ekranın üstünde, yatay bir şerit (overlay canvas, CRT dışı, `UiPixelScale` kuralı neyse ona uyar; UR-3).

- **Pist çizgisi:** kalın mürekkep (`Ink`) çubuk. Oda başına bir tik, her 10. oda uzun tik. Bitiş odası kalın dikey blok.
- **Yerel oyuncu:** çizginin **üstünde**, aşağı bakan büyük üçgen, içinde oda numarası (iki hane, `07`).
- **Rakip:** çizginin **altında**, daha küçük daire, içinde oda numarası. Üst/alt ayrımı sayesinde aynı odadayken de üst üste binmezler.
- **Bayraklar:** her işaretçinin sağında 3 flama (üçgen). Dolu = `Yellow`, boş = yalnız mürekkep kontur. Rakibinki daha küçük.
- ~~**Fark yazısı:** şeridin sağ altında `you +2` / `rival +3` / `level`.~~ **Kaldırıldı (2026-11-13, kullanıcı istedi).** `RaceTrackWindow.GapText` saf mantık ve testli olarak kodda duruyor, HUD kullanmıyor.

### Renk: dikkat, sonuç ekranıyla çakışıyor

Mockup'ta yerel = kırmızı, rakip = mavi çizildi. Ama sonuç ekranı (`RaceResultScreen`) rengi **kimliğe** bağlıyor: host = mavi, istemci = kırmızı; yereli "SEN" plakası söylüyor. HUD da aynı kuralı izlemeli: **renk kimlikten gelir** (host mavi, istemci kırmızı), **yerel oyuncu şekil ve konumdan** anlaşılır (üstte, büyük üçgen, "you" etiketi). Şekiller (üçgen/daire) kimliğe değil yerel/rakip ayrımına bağlı kalır. **Karar verildi (2026-11-13):** renk kimlikten gelir. Ceza bandı kırmızı olmaz (kırmızı istemci rengi); Ink zemin üstüne Yellow şeritli çizilir.

## Mercek (oda sayısı büyüyünce)

Pist her zaman bütün yarışı göstermez; oyuncuların etrafında bir pencere gösterir.

- **Pencere genişliği:** 14 oda (her oda okunur bir tik). Pencere iki oyuncunun ortasına ortalanır, uçlarda kenara yaslanır.
- **Bölünme:** fark 10 odadan büyükse pencere ikiye ayrılır. Her oyuncunun etrafında ~6 odalık bir parça, ortada zikzak kırılma ve `N rooms` yazısı (aradaki oda sayısı).
- ~~**Minimap:** şeridin altında ince gri çizgi, bütün yarış orantılı; iki küçük renkli dikey çentik oyuncuların yeri.~~ **Kaldırıldı (2026-11-13, kullanıcı istedi).**
- 22 odada (bugünkü oyun, `RoomNew-22`) pencere neredeyse bütün pisti kaplar; aynı kod 100 odada da çalışır.

## Hareket (ui-motion.md'ye göre; değerler başlangıç önerisi, gamepad ile ayarlanır)

| Olay | Hareket | Mockup değeri |
|---|---|---|
| Bayrak alma | Flama dolar, ölçek 1.6 + -12° dönüşten yaya oturur (`UiSpring`) | ~0.35 sn |
| Oda değişimi | İşaretçi yeni tike yaylanarak kayar; pencere gerekiyorsa yumuşakça kayar | — |
| 3/3, ceza | 0.45 sn bekle, eğik Ink/Yellow şeritli bant (`rival ‹‹ room 04`; kırmızı değil, bkz. Renk), rakip işaretçisi sarsılır (±12 px, 0.45 sn) ve bir tik geri kayar, ceza verenin flamaları sıfırlanır | bant 1.2 sn |
| Pencere bölünmesi/birleşmesi | Zikzak açılır/kapanır; ani geçiş olmaz | **[AÇIK]** |
| Rakip 2/3 | **Karar (2026-11-13, seçenek A; B'den vazgeçildi):** rakibin **alacağı sıradaki boş flama** yanıp söner (boş kontur ↔ Yellow dolgu, ~0.7 sn döngü). Yani 0/3'te 1., 1/3'te 2., 2/3'te 3. flama. Bayrak alınınca nabız bir sonraki boş flamaya geçer; 3/3 ve ceza sonrası sıfırlanınca yine 1. flamada başlar. Yalnız rakipte; yerelde yok. Ses ve titreşim yok. Başlangıç değeri, gamepad ile ayarlanır | karar verildi |

Bayrak alma titreşimi: mevcut `HapticManager` + `*VibrationSettingsSO` deseni (direction-D önerisi), yeni sabit yazılmaz.

## Uygulama (2026-11-13)

`RaceTrackView` (`Multiplayer/Hud/`, overlay canvas `[MP Track]`, sortingOrder 105, `UiPixelScale`) + `RaceTrackWindow`; `RaceHud` her karede `RaceTrackState` verir, eski "Bayrak N/3" / "Rakip: …" metinleri veri olarak kalır ama çizilmez. Toplam oda = bitiş işaretinin odası (`RaceFinishPoint`).

- **Zemin yok:** ekranı kaplayan plaka ve çizgilerin arkasındaki beyaz ray denendi, ikisi de kaldırıldı (kullanıcı beğenmedi; çizgiler zaten Ink). Yalnız metinlerin (`you`, fark, `N rooms`) arkasında küçük Ink plakalar var.
- **Bölünme işareti:** istenen `/\/\` zikzak: pist çizgisi boşlukta sürer, ortada çizgiyle aynı kalınlıkta (3 sanat pikseli) 24 piksel genişliğinde, ±5 yüksekliğinde zikzakla kırılır; altında `N rooms` plakası. Denenip atılanlar: ekranı kaplayan plaka, çizgi arkasında beyaz ray, "<" gibi okunan zikzak, `//` eksen kırılması.
- **Boş flama** "yalnız kontur" yerine kâğıt (Paper) dolgu + Ink kontur çizilir (UiPolygon dolu çizer, saydam delik yok); dolu = Yellow. Rakibin sıradaki flaması Paper↔Yellow nabız atar.
- **Ceza** sayacın 2'den 0'a düşmesinden çıkarılır (oturum 3'ü hiç göstermez): 3. flama dolar, 0.45 sn sonra Ink/Yellow şeritli bant (`rival << room 04` / `you << room 04`, ~1.2 sn) ve ceza alan işaretçi sarsılır; ceza işlemeyecekse (alan ilk odada) bant ve sarsıntı yok. Bilinen sınır: yeniden bağlanmada `ProgressSync` ile 2→0 gelirse yanlış ceza animasyonu çıkabilir.
- **Başlangıç değerleri** (`RaceTrackView` alanları, gamepad ile ayarlanır): flama patlaması ölçek 1.6 / -12°, ceza bekleme 0.45 sn, bant 1.2 sn, sarsıntı ±3 sanat pikseli 0.45 sn, nabız 0.7 sn, yerel işaretçi nefesi %3 / 2.4 sn.
- **İki pencerede doğrulandı (13 Kasım, host + Player 2, gerçek oturum; bayrak ve oda mesajları oturumun kendi yoluyla yollandı, oyuncular fiziksel hareket etmedi):** kimlik renkleri iki pencerede ters ve doğru, aynı odada üçgen/daire üst üste binmiyor, rakip 3. bayrağı alınca 3. flama doluyor, ardından `you << room 01` metni çıkıyor, istemci 2. odadan 1. odaya dönüyor ve bayraklar sıfırlanıyor, host tarafında `rival << room 01`; istemci 14. odaya geçince iki pencere de 1–6 / 12–17 bölünüyor (`5 rooms`). Bant ve sarsıntının gerçek penceredeki görüntüsü yakalanamadı (komut gecikmesi), tek pencerede görüldü.
- **Eksik:** ses, titreşim, çıkış animasyonu, bölünme/birleşme hissi ve pist boyutu gamepad ile denenmedi.

## Saf mantık (EditMode testli olmalı)

Pencere hesabı deterministik; `RaceHudText` gibi saf bir sınıfa (ör. `RaceTrackWindow`) konmalı:

- girdi: toplam oda, yerel oda, rakip oda, pencere genişliği (14), bölünme eşiği (10)
- çıktı: tek pencere `[lo, hi]` ya da iki parça + aradaki oda sayısı
- fark yazısı (`you +2` / `rival +3` / `level`)

## Açık sorular

1. ~~Renk kuralı~~ Karar: kimlikten (host mavi, istemci kırmızı); ceza bandı Ink/Yellow.
2. ~~Rakip 2/3 uyarısı nasıl olacak?~~ Karar: rakibin alacağı sıradaki flama nabız atar (A), yukarıdaki tabloda.
3. Bölünme geçişinin animasyonu.
4. Ses: bayrak alma ve ceza için `AudioCueSO` var mı, yoksa yeni cue mu?
