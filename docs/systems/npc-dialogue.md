# NPC diyalog levhası

Son kod doğrulaması: 2026-11-12 (EditMode 294/294; Play Mode'da levha açıldı, ekran görüntüsü alındı). GDD §7: diyalog kısa, esprili, çoğu zaman 4. duvarı yıkan bir "araya giren ses"; oyunun akışını kesmez.

## Tasarım niyeti

Ekranı kaplamayan küçük bir levha. Kesik köşeli beton levha, solda sarı rozet (içinde ink daire), üstte ince tehlike şeridi, sağda mono metin. Ekranın alt köşesinde durur (~%30 genişlik, 2-3 satır), oyuncuyu kapatmamak için gerekirse karşı köşeye kayar. Görsel dil: `docs/ui-research/mashup-ui.html` (Bauhaus + brutalist), HUD sayacı ve oda kartıyla aynı parçalar.

## Nasıl çalışır

- **Tetik:** `NPCDialogueController` aynen eskisi gibi: oyuncu trigger'a girince satırı seçer (Single / Sequential / Random, `OnTriggerEnter` ya da `OnPlayerDeath` ilerleme), çıkınca kapatır. Satır mantığı değişmedi.
- **Levha:** `NPCDialogueBubble` (`Instance`, `ShowDialogueAsync`, `HideDialogueAsync` API'si aynı kaldı). Arayüzü kodla kurar: ayrı overlay canvas (sıra 105: HUD 100'ün üstünde, oda kartı 110'un altında), `UiPixelScale`, `UiPolygon` şekilleri, `UiConcreteSurface` (levha, rozet), `UiHazardStripes` (şerit), TMP mono metin. Renk/font/çerçeve/gölge `UiThemeSO`'dan (`UiScreen.Hud` dengesi).
- **Boyut:** metin önce yerleşir, levha tam metnin yüksekliğine göre kurulur (harf harf büyümez); harfler sonra tek tek açılır (`NPCDialogueController.charRevealSpeed`). Harf sayısı `textInfo.characterCount` (etiketler sayılmaz).
- **Hareket (`UiSpring`, unscaled zaman):** levha ait olduğu kenardan yaylanarak gelir (240/22), rozet büyükten çakılır (`badgeSlamScale` 1,7) ve gölge ofseti de aynı yayla büyükten oturur, rozet boşta ~%3 nefes alır, kapanışta levha kenara çekilir.
- **Oyuncuyu kapatmama** (`DialogueSlabMath`, saf mantık, EditMode testli):
  1. Oyuncunun viewport x'i orta çizgiyi `sideHysteresis` (0,1) kadar levhanın yarısına geçince levha karşı köşeye yaylanarak kayar (yeni yay, 200/20). Orta çizgide gidip gelirken titremez (histerezis) ve iki kayma arası en az `flipCooldown` (0,6 sn) bekler. Gösterim başlarken taraf kayma olmadan doğrudan seçilir.
  2. Yine de oyuncu levhanın dikdörtgeninin (`playerClearance` 0,04 payla) içindeyse yalnızca arka plan (`CanvasRenderer.SetAlpha`, mesh kirlenmez) `overlapAlpha` (0,4) alfaya solar; yazı solmaz.
- **Sahnedeki eski balon:** sahnedeki `DialogueBubbleUI` nesnesinin eski `Image` + TMP görselleri `Awake`'te kapatılır (`Graphic.enabled = false`), yeni levha kendi canvas'ında çizilir. Sahne dosyasına dokunulmadı.

## Tunable'lar (Inspector, başlangıç önerisi; gamepad ile ayarlanır)

| Alan | Değer | Anlamı |
|---|---|---|
| `textWidthArt` | 104 | Metin genişliği (sanat pikseli); ~21 karakter/satır |
| `fontSizeArt` | 8 | Metin boyu |
| `badgeWidthArt` | 26 | Rozet genişliği |
| `edgeMarginArt` / `bottomMarginArt` | 8 / 12 | Kenar ve alt boşluk |
| `sideHysteresis` | 0,1 | Köşe değişimi için orta çizgi payı (viewport) |
| `flipCooldown` | 0,6 sn | İki köşe değişimi arası en kısa süre |
| `playerClearance` | 0,04 | Soluklaşma payı (viewport) |
| `overlapAlpha` / `fadeSpeed` | 0,4 / 8 | Soluk alfa ve geçiş hızı |
| `badgeSlamScale` | 1,7 | Rozetin çakılmadan önceki ölçeği |
| `breathAmount` / `breathSpeed` | 0,03 / 2 | Rozet nefesi |

## Kod yolları

`Assets/Scripts/New-Scripts/NPC/`: `NPCDialogueController.cs` (tetik, satır seçimi), `NPCDialogueBubble.cs` (levha arayüzü ve hareketi), `DialogueSlabMath.cs` (taraf seçimi, çakışma). Test: `Assets/Tests/EditMode/DialogueSlabMathTests.cs`. Editör menüsü `Tools/NPC/Create Global Dialogue UI` (`NPCDialogueSetupEditor`) hâlâ eski görselleri kurar; bu görseller artık kapatılıyor, ileride menü sadece bileşeni ekleyecek şekilde sadeleşebilir.

## Bilinen kenar durumlar

- **Görsel/hareket değerlendirmesi yapılmadı:** tek kare ekran görüntüsü doğrulandı (levha sağ altta, oyuncu solda). Yay sertliği, köşe kayması hissi, soluklaşma gamepad ile test edilmeli (CLAUDE.md kural 4).
- Konuşan NPC'nin adı/simgesi yok; rozetteki daire sabit. NPC başına simge ya da ad istenirse `NPCDialogueController`'a alan eklenir.
- Çıkış anında oyuncu levhanın yakınındaysa soluklaşma da biter; kapanış hep kenara çekilerek olur.
- Tık sesi ve titreşim yok.
- Oyuncu `FindFirstObjectByType<PlayerController>()` ile bulunur ve önbelleğe alınır; çok oyunculuda yerel oyuncu seçimi henüz düşünülmedi.
- Yazı kamera arkası noktada (`z < 0`) oyuncu konumu yok sayılır; taraf değişmez, soluklaşma olmaz.
