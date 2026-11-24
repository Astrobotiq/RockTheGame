# Çiçek node'u: animasyon hissi, karaktere/kola tepki ve polen

Tarih: 2026-11-22. K4 görünümü bitti (`flower-k4-plani.md`, branch `feature/flower-k4`). Bu plan üç araştırmanın birleşimidir: [E-animasyon](ui-research/flower-node/research/E-animasyon.md), [F-etkilesim](ui-research/flower-node/research/F-etkilesim.md), [G-polen](ui-research/flower-node/research/G-polen.md). Her satırdaki sayı **başlangıç tahminidir** (CLAUDE.md kural 4: his geliştiricinin kararı, gamepad testi ister).

## Kararlar (2026-11-22, geliştirici)

- Çiçeğin etrafındaki **çizgili uyarı halkası kaldırıldı** (kod ve test silindi, `32189b7`). Uyarı titreme + iplik tacı büzülmesi + gölge büyümesiyle verilir; yeterince okunmuyorsa Task 5/6 ve ses/titreşim kanalları tamamlar.
- Çiçeğin parçaları **karaktere ve kollarına tepki verir**; tutunma anında **polen** çıkar; **animasyon hissi** iyileştirilir.
- Öneri listesi onaylandı: 18 task, 23–28 Kasım, her gün ≤4 task (Nota ve Bounce testleri dahil), 29 Kasım tampon.

## Sıra ve bağımlılık

Hepsi `FlowerBloom.Apply` / `FlowerBloom.cs`'e dokunur, bu yüzden **paralel değil sırayla**, tek oturumda. Ortak bağ: Task 1 (tutunma bildirimi), hem tepki hem polen kullanır; `Poke` (Task 12) hem tepkinin "yakalama darbesi" hem polenin "yaprak sarsıntısı"dır.

| Gün | Task'lar |
|---|---|
| 23 Kas | T1 tutunma/bırakma bildirimi, T2 anticipation ve ipucu, T3 yaprak gecikmesi (+ Nota kulak testi) |
| 24 Kas | T4 follow-through, T5 uyarı darbesi ve katlanma sayacı, T6 snap darbesi (+ Bounce pad gamepad testi) |
| 25 Kas | T7 çiçek sesi, T8 çiçek titreşimi, **Ç9** (kapsamı animasyon hissini de içerir) |
| 26 Kas | T9 `FlowerReactMath`, T10 taç yumruğu (`grip01`) + ayar SO, T11 `FlowerReactor` |
| 27 Kas | T12 `FlowerBloom`'a bağlama, T13 tepki PlayMode testleri, T14 polen ayar SO + hesap, T15 polen prefab |
| 28 Kas | T16 polen tetikleme, T17 polen sıfırlama + 8 çiçek testi, T18 tepki + polen gamepad/CRT turu |
| 29 Kas | tampon |

## Task ayrıntıları

**T1 Tutunma/bırakma bildirimi (#code).** `IGrabReactive` (OnGrabbed/OnReleased). Tutunma tek darboğaz `PlayerController.AttachAnchor` (`:228-255`); bırakma çok noktalı olduğu için çiçek tarafında yoklama (anchor hâlâ bu node mu). Hangi elin (sol/sağ) ve dünya konumunun bilgisi gelir. Polen ve `FlowerReactor` bunu kullanır. `FlowerNodeRulesTests` yeşil kalmalı.

**T2 Anticipation ve ipucu görünür (#code).** Şimdi `open` en fazla −0.032'ye iniyor (%2 ölçek), ipucu `open`'ı hiç etkilemiyor (yaprak ilerlemesi ≤ 0.02). Dip −0.2, süre 0.12 sn; ipucu `open` yerine tomurcuk çekirdeği nabzına taşınır. (E bulgu 1–2.)

**T3 Zaman alanında yaprak gecikmesi (#code).** Şimdi beş yaprak %50 ilerlemeyi 17 ms içinde geçiyor. Yaprak başına yay: açılışta 0–0.12 sn, kapanışta 0–0.03 sn gecikme (altın oran sırası korunur). `FlowerBloomMath` için EditMode testi. (E bulgu 3.)

**T4 Yaprak ucu follow-through (#code).** `_BaseCurl`/`_Roll`'u ikinci, daha yavaş ve daha az sönümlü bir yay sürer; açılış sonrası küçük pitch flutter. Shader'a dokunulmaz. (E bulgu 4.)

**T5 Uyarı darbesi ve katlanma sayacı (#code).** Uyarı başında kısa "ürperme" darbesi; `warnCurlTarget` ~0.6; yaprak yaprak katlanma sayacı (şekil kanalı, renksiz); `trembleAmplitude` A/B değerleri. Yeni saf fonksiyon EditMode testli (`FlowerWarnMath` silindi, yeni ad serbest). (E bulgu 5–6.)

**T6 Snap darbesi (#code).** Model ölçeğinde "thud" yayı, ters yönlü "nefes çek" (wind-up); kâğıt kıymığı ve 2–4 kare hit-stop **ayrı bayrak, varsayılan kapalı**. (E bulgu 7.)

**T7 Çiçek sesi (#code).** `AudioCueSO` + kanal: uyarıda 3–4 yükselen tık, tutunma, snap. Klip yoksa boş cue (sessiz), asit/spiral deseni gibi.

**T8 Çiçek titreşimi (#code).** `FlowerVibrationSettingsSO` + kısa `RumbleProfile`; yalnızca tutulan çiçekte. `HapticManager.Vibrate` çalanı kestiği için dash ile çakışma notu (G ve E bulguları).

**Ç9 (#test, mevcut).** Kapsamı genişler: bükülmeler eski çiçek kadar canlı mı, **anticipation → açılış → follow-through → uyarı → snap hissi**, uyarı halkasız yeterince okunuyor mu, ses/titreşim, kapalı tomurcuk çiçek gibi mi, 96 px'te iplik, zincirde kare süresi, renk körlüğü.

**T9 `FlowerReactMath` (#code).** Saf hesap: mesafe falloff, yaw'ın tek fonksiyon olması, 5 yaprakla ~50° yarım genişlikte kapsama, `grip01 = 0` iken eski çıktıyla birebir eşitlik, yay aşım bandı. Dünya yönünü çiçek-yerel azimuta çeviren fonksiyon gerçek yaprak pivotlarıyla karşılaştırılarak test edilir (işaret tuzağı). EditMode.

**T10 Taç yumruğu + ayar SO (#code).** `FlowerCoronaMath.Evaluate`'a `grip01` (şeritler tutunmada toplanır; shader değişmez), `FlowerReactionSettingsSO`; yay 300/17 (yaprak), 700/36 (grip), 120/15 (yön) başlangıç tahmini. Yay testi.

**T11 `FlowerReactor` (#code).** Oyuncu ve iki elin konumunu okur (`PlayerController.LeftAnchor/RightAnchor`, omuz, gövde; önbellekli `FindFirstObjectByType` + yoklama), yayları sürer, **görsel yok**. Kapı: çiçek yalnızca tutulabilirken ve açıkken tepki verir (kapalı tomurcuk "bakarsa" tutulabilir sanılır). `NodeDetector`'ın her çağrıda dizi ayırmasına dikkat: çiçekler onu çağırmaz.

**T12 `FlowerBloom`'a bağlama (#code).** Yaw (yana itme, `Pivot.localRotation`), pitch, grip, `Poke` (yakalama darbesi: `_pop` ve `_spring` Kick); kol süpürmesi (kol çiçek merkezinden oyuncuya uzanan doğru; yapraklar saat yelkovanı gibi aralanır) ve gövde yakınlığı. FlowerTest'te önce/sonra karşılaştırma. Ekleme noktaları `F-etkilesim.md` "Kanca noktaları".

**T13 Tepki PlayMode testleri (#test).** Yaklaşınca tepki, uzak çiçek etkilenmez, tutununca `Grip01` artar, kapalı çiçek sessiz kalır, `FlowerNodeRulesTests` yeşil. Durum reactor'ın okunabilir özelliklerinden okunur (MPB'den değil).

**T14 Polen ayar SO + hesap (#code).** `FlowerPollenSettingsSO`, `FlowerPollenMath`: güç eğrisi, sayı dağılımı, `ortho/60` alt boyut, organ kaynak noktaları (5 organ, `R×1.44`, tepeden 36° kayık), koni hız. EditMode.

**T15 Polen prefab (#art).** `FX_FlowerPollen`, 9 sprite atlası, `Sprite-Unlit`, Player layer −1, `DebugBurst`, ortho 8 ve 22'de CRT'li ekran görüntüsü. Tasarım "Delgeç Konfetisi": tek patlama, 18 parçacık (9 nokta, 6 elips, 3 kâğıt parçası), boyut 0.14–0.45, ömür 1.2–1.9 sn, krem yüz + indigo hilal (gökte okunur). Amber yok (uyarıya ayrılmış).

**T16 Polen tetikleme (#code).** `FlowerPollen`: `AcidReaction` deseni (lazy çocuk ParticleSystem, `Emit(EmitParams)`), `OnGrabbed`'dan ~0.05 sn sonra patlar (yaprak sarsıntısından sonra), oyuncuya dönük 2–3 organdan ağırlıklı; Warning/Closing'de durur; hit-stop `HitStopStarted/Ended` ile Pause/Play; `Poke` (T12) kullanır.

**T17 Polen sıfırlama + 8 çiçek (#code + #test).** `IResettable` (OnDeath değil: iris kapalıyken temizlenir), `OnEnable`'da `Clear`, `maxParticles` 48; `FlowerTest` odasında 8 çiçek yük testi.

**T18 Tepki + polen gamepad ve CRT turu (#test).** Ortho 8/10/22; gökyüzü ve iplik tacı üstünde polen okunurluğu; pembe karakterle karışma; tepki yönü (yaw işareti), kol süpürmesi; değerler ayarlanır, eski/yeni `feel-notlari.md`'ye.

## Dışarıda (Someday)

- Şerit yönlenmesi shader'ı (F5), nişan çekimi (F7) + durağan çiçekte `Apply` atlama + 8 çiçekli Profiler ölçümü.
- Polen seçenek B: snap püfü, uzak katman, tutunurken damlama.
- Salınım torku (F6, `SpiralNode` ile karışma riski) ve eksen eğimi (F8).

## Riskler

- **Tek dosya yoğunluğu:** her task `FlowerBloom.cs`'e dokunur; sırayla ve her task sonunda derleme + EditMode + `FlowerNodeRulesTests`.
- **Performans:** `FlowerBloom` çiçek başına 9 renderer'a MPB yazıyor (SRP Batcher dışı); durağan çiçekte `Apply` her kare çalışıyor (8 çiçek = 72 `SetPropertyBlock`). Ölçmeden "bedava" denmez; ek maliyet T12 sonrası FlowerTest'te bakılır.
- **Okunurluk:** CRT altında küçük polen (min boyut kamera ortho'suna göre), kâğıt dili (düz, sert gölge), karaktere pembe karışmaması.
- **Doğrulanmamış varsayımlar:** tüm genlik ve süreler tahmin; Unity'de ölçülmedi. Doğrulanamayan kaynak listeleri üç araştırma dosyasının sonunda.
