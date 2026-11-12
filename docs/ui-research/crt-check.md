# Overlay canvas CRT dışında mı? Doğrulama

Tarih: 2026-11-08. `supervisor-review.md` §2.1 #1 kod ve URP davranışından "Overlay CRT'siz" demişti, ekran görüntüsü alınmamıştı. Burada Play Mode'da yakalandı. Görüntüler: `docs/ui-research/crt-check/`.

## Sonuç

| UI | Canvas | CRT içinde mi? | Kanıt |
|---|---|---|---|
| **Overlay canvas** (test paneli, CRT'li NewScene üstünde) | Screen Space - Overlay | **Hayır, dışında** | `04_overlay_vs_world.png`: kırmızı panel tam `#FC1E00`, 1 benzersiz renk, satır sapması 0,00; metin keskin piksel |
| **Coin UI** (ananas sayacı, sol üst) | Screen Space - Camera (`Canvas`, Main Camera, sıra 5) | **Evet, içinde** | `01_newscene_default.png`: dünya ile aynı ekran bükülmesi, tarama çizgisi ve bulanıklık; bölgede 1315 benzersiz renk, satır sapması 20,5 |
| **StaminaCanvas** (oyuncunun üstündeki çubuk) | World Space (Main Camera) | **Evet, içinde** | `02_newscene_stamina.png`: bükülme/bulanıklık dünyayla aynı; bölgede 164 benzersiz renk |
| **Ana menü** (`MainMenuCanvas`) | Screen Space - Overlay | CRT'siz, **ama bu yakalama tek başına yerleşimi kanıtlamaz** | `03_mainmenu.png`: düz ve keskin (1 renk, sapma 0,00). Menü sahnesinde `Volume` yok; CRT pass'i volume yoksa hiçbir şey yapmıyor |

Referans: CRT'li dünyanın düz mavi gökyüzü bölgesi 246 benzersiz renk, satır sapması 19,1; turuncu zemin 887 renk, 18,7. Overlay panelinde 1 renk, 0,00.

## Nedeni (koddan, doğrulandı)

- Aktif renderer (`Assets/Settings/New Universal Render Pipeline Asset_Renderer.asset`, varsayılan indeks 0, kamera indeksi -1): `CrtRendererFeature` aktif, `injection = AfterRenderingPostProcessing`.
- Screen Space - Camera ve World Space canvas'lar kamera hattında (saydam geçişte) çizilir, CRT'den önce; CRT hepsini birlikte işler.
- Overlay canvas kamera hattının dışında, backbuffer'a CRT'den sonra çizilir; CRT onu görmez.
- `CrtRenderPass`: `VolumeManager`'da aktif `CrtFilterVolume` yoksa pass atlanır. Ana menüde ve (büyük olasılıkla) lobi sahnesinde volume yok; bu sahnelerde CRT zaten çalışmıyor.

## Çıkarımlar

1. **Yeni UI Overlay'de kalırsa CRT'siz ve piksel-keskin olur.** Tam sayı 4x piksel garantisi (bkz. `supervisor-review.md` §0) yalnızca Overlay'de geçerli.
2. **Mevcut Coin UI CRT içinde** (480×360 hücre, bükülme). "HUD: çeyrek daire sayaç" task'ı yeni sayacı Overlay'de çizerse mevcut Camera canvas sayacı kaldırılmalı ya da gizlenmeli; ikisi birden görünür.
3. **StaminaCanvas World Space**, yani CRT içinde. "Kırılan stamina halkası" task'ı onu karakter üstünde world space bırakırsa CRT'li (bulanık, bükülmüş) kalır; Overlay'e taşımak halkayı karakteri takip eden ekran koordinatına çevirmeyi gerektirir. Karar o task'ta.
4. UI dünyadan koyu/doygun görünecek (CRT dünyayı aydınlatıyor); bkz. `palette.md` "CRT farkı".

## Sınırlar

1. Lobi (`MultiplayerLobby`) ve `RaceHud` yakalanmadı. Overlay olduklarına kod ve sahne dosyası işaret ediyor (`supervisor-review.md`), burada ekranda görülmedi.
2. Overlay testi, NewScene üstüne eklenen geçici bir canvas'la yapıldı (sahneye kaydedilmedi); gerçek bir oyun ekranıyla değil.
3. Coin UI ve stamina bölgelerindeki "satır sapması" ve "renk sayısı" içerik de taşıyor (ikon, metin), saf tarama çizgisi ölçüsü değil. Düz Overlay paneline karşı ayırt edici olan benzersiz renk sayısı ve görsel bükülme.
4. Yakalama `capture_game_view --source screen` ile 1280×720; araç bazen eski kareyi döndürüyor, `crt-check` görüntüleri yeni kare doğrulanarak alındı.
