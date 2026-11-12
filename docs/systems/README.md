# Mekanik dosyaları

`docs/GDD.md` oyunun ne olduğunu anlatır; bu klasör **nasıl çalıştığını** anlatır. Mekanik başına bir dosya: tasarım niyeti, ilgili script'ler, tunable'lar (`PlayerStatsSO` alanları), bilinen kenar durumlar.

Yeni bir özellik üzerinde çalışmaya başlamadan önce ilgili dosyayı oku; dosya yoksa oluştur.

## Dosyalar

| Dosya | Kapsam | GDD |
|---|---|---|
| [`movement-core.md`](movement-core.md) | Koşma, zıplama, havaya giriş yoluna göre yerçekimi, coyote/buffer, dash, hit-stop | §5.1, §5.2 |
| [`grapple.md`](grapple.md) | Hedef seçimi, yerden kanca, tek/çift kanca, slingshot, dönen node | §5.2 |
| [`wall-movement.md`](wall-movement.md) | Tırmanma, kenardan çıkma, kayma, duvar zıplaması | §5.2 |
| [`death-respawn.md`](death-respawn.md) | Ölüm anı, respawn dizisi, neyin sıfırlandığı, checkpoint'ler | §5.3 |
| [`platforms.md`](platforms.md) | Sürücü + strateji, taşıma, tetiklenen/kırılan/tek yönlü platform, launch pad, wrapping | §6.2 |
| [`hazards.md`](hazards.md) | Diken, asit, hareketli/hızlanan/dönen/akan ölümcül nesneler | §6.2 |
| [`collectibles.md`](collectibles.md) | Ananas, anahtar-kapı, slingshot yenileyici, tek nefeste ananas, envanter | §5.3, §6.2 |
| [`pause.md`](pause.md) | Start ile duraklat/devam, `timeScale`, ses/titreşim, çok oyunculuda kapalı | §11 |
| [`ui-motion.md`](ui-motion.md) | UI hareket kuralları ve kontrol listesi: yay, poz, kayan imleç, giriş; UI donuk bırakılmaz | UI |
| [`room-title-card.md`](room-title-card.md) | Odaya girişte çıkan başlık kartı: levha, kiriş, ekstrüde numara, şerit; tetik, zamanlama, tunable'lar | UI |
| [`multiplayer.md`](multiplayer.md) | Hayalet rakip, yarış ve birlikte modları, bayrak cezası, taşıyıcılar, yarış kaydı (MP 1 kodda: assembly, taşıyıcı arayüzü, mesajlar) | §9 |

Her dosyada aynı bölümler var: tasarım niyeti, nasıl çalıştığı, tunable'lar (asset'teki gerçek değerler), kod yolları ve **bilinen kenar durumlar / GDD ile farklar**. Son bölüm, kodun GDD'den ayrıldığı yerleri listeler; hangisinin doğru olduğu geliştiricinin kararıdır.

Dosyalar 2026-10-05'te koda karşı doğrulandı. Kod değiştiğinde ilgili dosyayı güncelle ve "Son kod doğrulaması" tarihini yenile.

Henüz dosyası olmayan sistemler (gerektiğinde oluştur): oda ve geçiş sistemi (`LevelChange/`, kamera), NPC diyalogları (GDD §7), ses, titreşim (`HapticManager`).
