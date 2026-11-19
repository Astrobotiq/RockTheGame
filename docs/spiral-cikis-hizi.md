# Spiral çıkış hızı: tasarım

*2026-11-20. Beş rolün raporlarından derlendi: oyun tasarımı, sistem tasarımı, fizik/sayılar, görsel/ses, QA. Kararlar geliştiricinin.*

## Niyet

Spiral node'da dönüp bırakan oyuncu, motorun verdiği hızla gerçekten fırlamalı. Bugün hız havada ilk karelerde eriyor: yükselişte 3× yerçekimi, 10'un üstündeki yatay hızda havadaki fren (`AirOverspeedReduce` 20, stick nötrken `AirDrag` 24). Bunun üstüne bırakış anı sessiz ve iz bırakışta kesiliyor. Ek hız garip görünmemeli; sebebi aynı karede görülmeli.

**Kapsam:** yalnızca spiral node'dan tek kanca bırakışı. **Slingshot, normal swing, çift kanca ve dönen node (`FullRotationNode`) değişmez.** Slingshot sayıları burada sadece "spiral onu geçmesin" referansıdır.

Bu, `spiral-node-tasarimi.md`'deki "bırakış boost'u hedef değil" kararını ve SP5 kuralını bilerek geri alır. İkisi de güncellenir.

## Kararlar (geliştirici, 2026-11-20)

| Konu | Karar |
|---|---|
| Yöntem | **Hız tutma + küçük itme**: bırakıştan sonra kısa bir süre havadaki fren yok, ayrıca bırakışta küçük bir itme |
| İtme yönü | **Yatay ve dikey**: itme spinin teğeti yönünde, iki eksende |
| Slingshot | Olduğu gibi kalır |

## Sayılar (fizik simülasyonu, 45° bırakış, mesafe y = −4'e kadar)

| | Hız 32 (stick nötr) | Hız 45 (stick spin yönünde) |
|---|---|---|
| Bugün | 11.3 | 20.7 |
| 0.3 sn tutma | 14.7 | 25.4 |
| +6 teğet itme | 15.2 / apex 2.4 → 3.5 | 26.2 |
| **0.3 sn tutma + itme** (seçilen, +6 ile ölçüldü) | **19.4** | **31.5** |
| Slingshot 65 (referans) | 41.7 | 41.7 |

Seçilen paket, en kötü durumda slingshot mesafesinin ~%75'inde kalır. Fizik ajanı itme için +4'ü önerdi (32 → 36, ~%12 sıçrama); +6'nın sayıları ölçüldü. Başlangıç değeri **+4**; gamepad testinde 4–6 arası ayarlanır.

Fizik notu: spiral gerçekten normal swing'den hızlı (R4'te 32'ye karşı ~25), ama 45° ve dik bırakışta nötr stick'te hep 32'ye kilitli. Fark tabanda ve stick spin yönündeyken açılıyor.

## Tasarım

### 1. Mekanizma (`SwingingState` gönüllü bırakışı)

- **İtme:** `_currentVelocity += spinTangent × SpiralExitBoost` (teğet yönünde, iki eksende). Sabit ekleme, çarpan değil: çarpan hızlı spiralde fazla büyür.
- **Tutma:** yeni hava profili `AirEntry.SpiralRelease(nodeVelocity, carryDecay, holdTime)`. `SwingRelease` ile aynı (H2: yükselirken 3×, düşerken 1.5×, apex hang yok), tek farkı `holdTime` boyunca `ApplyAirMovement`'ta `AirDrag` ve `AirOverspeedReduce` uygulanmaz. Stick ile normal hızlanma devam eder.
- **Yerçekimi değişmez.** Dikey kazanç yalnızca itmenin dikey bileşeninden gelir.
- Normal node bırakışı birebir aynı kalır: `_spiral == null` ise eski çağrı.

### 2. Ne zaman verilir (sistem tasarımcısının kapıları, benim kararım)

- Yalnızca tetiği bırakarak yapılan bırakışta. Kanca kopması (SP4, duvar), çarpışma, çift kancaya ya da slingshot'a geçiş, respawn ve node pasifleşmesinde verilmez.
- Motor başlamadan (yerden tutunmada hop sırasında) verilmez.
- Spinin tersine giderken (SP2'nin yavaşlama evresi) ya da hız hedefin yarısının altındayken verilmez. Böylece "boş yere fırlama" olmaz, ters dönen spiralin yön değişim anı da kendiliğinden yumuşar.
- Ters dönen spiralde bırakış anındaki yönle verilir.

### 3. Yeni tunable'lar (`PlayerStatsSO`, "Spiral Node" başlığı)

| Alan | Başlangıç | Anlamı |
|---|---|---|
| `SpiralExitBoost` | 6 (ilk 4; 2026-11-20 gamepad: yavaş) | bırakışta teğet yönündeki itme (b/sn) |
| `SpiralExitHoldTime` | 0.45 (ilk 0.3) | bırakıştan sonra havadaki frenin uygulanmadığı süre (sn) |
| `SpiralExitMinSpeedRatio` | 0.5 | hız / hedef bu oranın altındaysa çıkış hızı verilmez |

Saf hesap `SpiralDriveMath.ExitBoost(...)` içinde, EditMode testli.

### 4. Okunurluk: garip görünmemesi için (görsel/ses ajanı)

Hepsi hızın değiştiği karede, slingshot'tan belirgin hafif:

- **İz kesilmesin (şart):** `PlayerVFXManager.HandleTrailEffect` bugün izi yalnızca swing/dash state'lerinde açık tutuyor; bırakış karesinde siliniyor. Tutma süresince iz açık kalır, hafifçe uzar.
- **Spiral tepkisi (şart):** bırakışta `_Glow = 1` darbesi (`Connect`'teki ile aynı, `Disconnect` yolunda).
- **Titreşim (şart):** yeni tek seferlik `SpiralRelease` profili, ~0.12 sn, `HighImpact`'ten hafif.
- **Ses (şart):** tek seferlik kısa "kırbaç" cue, döngü sesinin kesildiği karede. Klip kullanıcıda; boşsa sessiz.
- **Yok:** hit-stop (akışı keser), ekran sarsıntısı, hız çizgileri, piksel partikül, dash ghost'ları.

### 5. Kurallar ve testler (QA)

- **SP5 yeni metni:** spiralden tek kancayı bırakınca oyuncu, SP8 dışında normal tek kanca bırakışıyla (H2) havaya geçer.
- **SP8:** spiralden tetiği bırakarak çıkış, bırakış hızından hızlıdır (spin teğetinde itme + kısa süre havada fren yok). Yalnızca spiral node'a özel; kopma, çift kanca, slingshot ve respawn'da verilmez; spinin tersine ya da çok yavaşken verilmez.
- **H2:** "(spiral node hariç: SP5, SP8)" notu.
- Testler: SP5 yeniden yazılır; SP8a (çıkış hızı ≥ bırakış + itme − 0.5), normal node bırakışı değişmez, kopma / çift kanca / slingshot'ta yok, ters yönde ve yavaşken yok, tutma süresi sonunda fren geri gelir, tekrarlı tutunmada birikme yok. `AirEntryRulesTests` H2 testleri değişmeden geçer.

## Bilinen sınırlar

- Bırakışta taşınan yatay hız (hareketli node kanalı) ters stick'le iptal edilmiyor ve duvara çarpınca sıfırlanmıyor; mevcut davranış, bu tasarım dokunmuyor.
- Değerler oyun hissi kararıdır (CLAUDE.md kural 4); gamepad testinde ayarlanır.
