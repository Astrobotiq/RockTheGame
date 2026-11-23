# Multiplayer cezası: balonla geri taşınma

*Durum: **tasarım onaylandı** (2026-11-24, kullanıcı önerilerin hepsini kabul etti; açık sorular aşağıda kararlarıyla). **Kodda: BB1, BB2, BB4, BB5, BB6 (2026-11-24); BB3 testleri yazıldı ve onaylandı, iki pencere denemesi bekliyor; BB7 (his testi) açık.** Planla farklar: protokol v4 → v5 değil **v5 → v6** (v5'i `LeaveMessage` almıştı); yol tek tip Catmull-Rom değil kord uzunluğuyla ölçekli Hermite (kısa kapı bacağı + uzun oda döngü yapıyordu); kapı, ayrılınan odada hedefe açılan **ya da hedefte ayrılınana açılan** tetikleyicidir (geçişler çoğunlukla tek yönlü kurulu); uçma pozu için görsel çocuk yerine oyuncu kökü döndürülür (gözler ve kollar ayrı çocuklar, birlikte yatsın diye). İlgili: [`systems/multiplayer.md`](systems/multiplayer.md) (Ceza akışı), `Multiplayer/Penalty/PenaltyHandler.cs`.*

## Sorun

Bugün ceza ölüm gibi görünüyor: `PenaltyHandler` `RespawnManager` ile aynı diziyi oynatıyor (iris kapanır → sıfırlama → ışınlama → iris açılır, toplam ~1,5 sn). Oyuncu ne olduğunu anlamıyor, "öldüm mü?" diye düşünüyor. Ceza bir **ölüm değil, rakibin hamlesi**; öyle hissettirmeli.

İstek: oyuncu havaya kalkar, uçma pozuna girer, etrafında bir balon oluşur, balon onu yavaşça önceki odanın başına taşır.

## Mario'da nasıl

| Oyun | Balona nasıl girilir | Nasıl hareket eder | Nasıl patlar |
|---|---|---|---|
| New Super Mario Bros. Wii / U | Çok oyunculuda can kaybedince balonda doğarsın; A ile isteyerek de girilir. Balon dokunulmazlık verir. | Kumandayı sallayınca balon balonda olmayan oyunculara doğru süzülür; karakter yardım ister. | Yalnızca balonda olmayan bir oyuncu patlatır (dokunarak ya da ateş topu, buz topu, fıçı atarak). Balondaki oyuncu kendi çıkamaz. |
| New Super Mario Bros. 2 | Dokunmatik düğme. Önde giden oyuncu ekrandan çok uzaklaşırsa geride kalan **ölmeden** otomatik balona girer. | B ile diğer oyuncuya doğru gider. | Aynı. |
| Super Mario 3D World | Wii/U gibi; geride kalan oyuncu ekran dışında kalınca balonla öndekine döner. Tek aktif oyuncu kendini balona sokamaz. | Öndekine doğru süzülür. | Aynı. Otomatik kayan bölümlerde ekran dışında kalmak yine can götürebiliyor. |
| Super Mario Run | Balonlar ek can; yenilen oyuncu balonla sola fırlatılır, sonra bölümün ana yolu boyunca **geriye** süzülür. Balondayken süre akmaz. | Ana yol boyunca; kapılarda ve hayalet evi/kule duvarlarında durur. | Fırlatmadan sonra dokununca patlar, oyuncu düşer, kısa dokunulmazlık. |
| Super Mario Bros. Wonder | Balon yerine **hayalet** formu: yenilen oyuncu birkaç saniye (yaklaşık 5) hayalet olur, biri dokunursa döner. | — | — |

**Tasarım gerekçesi (Iwata Asks).** Balon NSMB Wii'nin dört kişilik testinde çıktı: deliğe düşen oyuncu oyun dışında bekleyip duruyordu. Balon üç sorunu birden çözdü:

1. **Hızlı dönüş.** Oyuncu oyundan çıkmaz, ekranda kalır.
2. **Ceza görünür.** Balondaki oyuncu ve diğerleri ne olduğunu ve kimin yüzünden olduğunu görür. Önceki deneme (oyuncunun ? bloğundan çıkması) bu yüzden atıldı: blok içindeki oyuncu görünmüyordu, bekleme belirsizdi.
3. **Beceri farkı.** Usta oyuncu zayıf olanı taşıyabilir.

**Bizim için çıkarılacaklar.**

- Balon "öldün" demez, "taşınıyorsun" der. Bizim sorunumuzun karşılığı tam olarak bu.
- Mario'da balon **duvarların içinden geçer** ve dokunulmazdır (NSMB). Bizde de öyle olmalı; yoksa yol bulma gerekir.
- Mario'da balonu başka oyuncu patlatır. Bizde rakip hayalettir, dünyaya dokunmaz; **balon hedefe varınca kendi patlar.**
- Balondaki oyuncu Mario'da sallayarak yönü/hızı etkileyebilir; bu, cezaya küçük bir kontrol hissi katar (aşağıda açık soru 1).
- Kaynaklar balonun hızını, boyutunu, saydamlığını ve içindeki pozu anlatmıyor; bunlar bizim kararımız ve `SO` değerleri.

## Bizde nasıl olur

### Dizi

| Evre | Süre (başlangıç değeri) | Ne olur |
|---|---|---|
| 1. Kalkış | 0,35 sn | Oyuncu dondurulur (`OnStartRespawn`), 1,5 birim yükselir (ease-out). Görsel gidiş yönüne doğru yatar: uçma pozu (Süpermen gibi, ~70°). |
| 2. Balon | 0,25 sn (kalkışla üst üste) | Balon oyuncunun çevresinde 0 → 1,15 → 1 ölçeğiyle şişer (OutBack). "Bloop" sesi, hafif titreşim. |
| 3. Yolculuk | 2,5–5 sn (mesafeye göre) | Balon yolu izler, hafifçe salınır (±0,25 birim, ~1,2 Hz) ve hız yönünde esner. Duvarların içinden geçer, hiçbir şey ona zarar vermez. |
| 3a. Kapı | yolculuğun içinde | Balon iki oda arasındaki geçişe varınca normal oda geçişindeki gibi `RoomManager.TransitionToRoom` + kamera kaydırma; sıfırlamalar bu anda yapılır. |
| 4. Varış | 0,3 sn bekleme + patlama | Balon checkpoint'in 1,5 birim üstünde durur, kısa salınır, patlar (halka partikül, "pop" sesi, titreşim). Oyuncu `NormalState` + `AirEntry.Fall` ile düşer (`OnEndRespawn`). |

Toplam ~3,5–6 sn. Bugünkü cezadan 2–4,5 sn uzun; bu cezayı biraz sertleştirir (açık soru 2).

### Yol

Düz çizgi yerine **kapı üzerinden** gider: oyuncunun konumu → ayrılınan odadaki hedefe giden `RoomTransitionTrigger`'ın merkezi → hedef odanın `InitialCheckpoint`'i + 1,5 birim yukarı. İki köşeli yol Catmull-Rom ile yumuşatılır.

Neden kapı: `RoomManager` yalnızca mevcut odayı ve komşularını açık tutar. Düz çizgi uyuyan bir odanın üstünden geçebilir; kamera orada boş ekran gösterir. Kapı üzerinden giden yol hep iki açık odanın içinde kalır ve kamerayı normal oda geçişiyle aynı şekilde taşır.

Kapı bulunamazsa (sahne düzeni hatası) düz çizgiye düşer ve uyarı yazar.

**Ölçülen mesafeler** (`NewScene`, odanın başından önceki odanın başına; oyuncu odanın sonundaysa daha uzun):

| Ceza | Yaklaşık yol |
|---|---|
| 2 → 1 | ~60 birim |
| 7 → 6 | ~70 |
| 12 → 11 | ~40 |
| 13 → 12 (13 en geniş oda, 195 birim) | 150–340 |
| 21 → 20 | 30–290 |
| 22 → 11 | ~50 |

Sabit hızla gitmek uzun odalarda 10 sn'yi aşar. Bu yüzden süre `clamp(mesafe / seyirHızı, 2.5, 5)` olur; "yavaş" his hızdan değil salınım ve yumuşak hızlanma/yavaşlamadan gelir.

### Kamera

`CameraController` kamerayı Cinemachine confiner'ıyla oda sınırına hapsediyor ve confiner'ı açıp kapamak kamerayı sıçratıyor (kodda not var). Bu yüzden confiner'a dokunulmaz:

- Ayrılınan oda içinde kamera balonu normal takip eder (balon oyuncunun kendisi; `PlayerRigidbody.position` da güncellenmeli ki takip hedefi okusun).
- Kapıda normal oda geçişinin kamera kaydırması (`PanAndZoomCameraAsync` ya da `RoomTransitionCoordinator`'daki aynı yol) çalışır; zoom hedef odanın değerine geçer.
- Hedef odada yine normal takip.

### Sıfırlama

Bugün iris kapalıyken yapılıyor (`LevelResetManager.ResetAll`, `RoomStateReset.ResetRoom` hedef ve ayrılınan odaya). İris olmayınca platformların yerine zıplaması görünebilir. Öneri: hepsi **kapı anında**, kamera kaydırması başlarken. Ayrılınan oda o andan sonra ekran dışında kalır; hedef oda kaydırma sırasında görünmeye başlar, sıfırlama ondan önce biter. Gözle kontrol edilmeli.

### Dokunulmazlık

Oyuncu duraklatılmış olsa da kinematik `Rigidbody2D` tehlike trigger'larının içinden geçerken `OnTriggerEnter` alabilir (asit, `DeathZone`, testereler). Balon boyunca `PlayerCollider` kapatılır, patlamada geri açılır. Bu multiplayer kodundan yapılır, runtime'a dokunulmaz.

### Uç durumlar

- **Oyuncu ölüyken, duraklatılmışken ya da oda geçişindeyken ceza gelirse:** bugünkü gibi bekler.
- **Balondayken ikinci ceza:** kuyrukta bekler; patlamadan sonra yeni balon hedef odadan bir öncekine gider. (İleride doğrudan zincirlenebilir.)
- **İptal (sahne değişti, oturum kapandı):** collider açılır, balon yok edilir, `OnEndRespawn` çağrılır; bugünkü `catch` bloğunun genişletilmişi.
- **Bağlantı balon sırasında koparsa:** balon yereldir, varışa kadar sürer; `ConnectionWatch`'ın donması ondan sonra uygulanır. Kontrol edilmeli: ikisi de `OnStartRespawn`/`OnEndRespawn` kullanıyorsa patlama donmayı çözmemeli.
- **Pause:** multiplayer pause'un oyunu durdurup durdurmadığına göre (bkz. `multiplayer-pause-plani.md`) balon tween'i de durmalı ya da sürmeli; aynı kurala uyar.
- **Kayıt:** kapıda `RoomEntered` gider ve kayıt yazılır. Balon sırasında oyun kapanırsa devam etme oyuncuyu zaten odanın başına koyar.

### Rakibin gördüğü

Bu, cezanın en tatmin edici kısmı olabilir: cezayı yollayan oyuncu rakibinin balonla geri taşındığını görür.

- Snapshot'lar balon boyunca akar ve `Teleported` biti gönderilmez, hayalet yumuşakça süzülür.
- `SnapshotMessage`'a `InBubble` biti eklenir (protokol v4 → v5). `GhostView` hayaletin çevresine kontur hâlinde balon çizer.
- Hayalet görünürlük kuralı (aynı ya da komşu oda) değişmez; rakip yanından geçerken görür.

### Görsel ve ses

- **Balon:** sanat yönüyle uyumlu olmalı. Halftone yumuşak baskı (piksel değil), sıvı-ışık bounce pad'iyle aynı ailede: ince parlak kenar, halftone nokta dolgusu, hafif yanardöner renk. Shader ile (UiConcrete/UiDither deseni gibi), doku ile değil. İlk sürüm için basit bir daire shader'ı yeter.
- **Uçma pozu:** Animator'da böyle bir durum yok. İlk sürümde görsel çocuk transform'u döndürülür ve hafifçe esnetilir (multiplayer kodundan). Gerçek animasyon ayrı bir #art işi; Animator parametresi eklemek runtime'a "yalnız ekleme" kuralına uyar.
- **Ses:** şişme ("bloop"), yolculukta hafif uğultu döngüsü, patlama ("pop"). `AudioCueSO` + `AudioCuePlayEventChannelSO` ile.
- **Titreşim:** şişme ve patlama için hafif darbe. Yeni `PenaltyVibrationSettingsSO` ya da mevcut multiplayer ayarlarına alan; CLAUDE.md'deki SO-başına-sistem desenine uyar.

### Ayarlar

Hepsi yeni `PenaltyBubbleSettingsSO`'da (multiplayer assembly): kalkış yüksekliği/süresi, poz açısı, şişme süresi ve aşımı, seyir hızı, min/maks süre, salınım genliği ve frekansı, varış yüksekliği ve bekleme, patlama efekti. Hissi insan ayarlar (CLAUDE.md kural 4).

### Tek kişilik oyunun korunması

Hepsi `RockTheGame.Multiplayer` içinde. Runtime'a beklenen dokunuş yok; uçma animasyonu gelirse yalnız bir Animator parametresi eklenir. 36+ PlayMode testi her adımda yeşil kalmalı.

## Testler

- **EditMode (saf mantık):** `PenaltyBubbleMath`: kapı noktalı yol, süre sınırlaması, salınım, kapıyı geçme anı. Bunlar ajanın yazabileceği deterministik testler.
- **PlayMode (oynanış, insan onaylar):** mevcut `PenaltyFlowTests` (3 → 2, 22 → 11, oda 1'de işlemez, ölüyken bekler, sonraki ölüm hedef checkpoint'te) yeni süreyle geçmeli. Yeni: balon yolu asitten geçerken oyuncu ölmez; patlamadan sonra collider açık; kapıda oda değişir; iptalde oyuncu serbest kalır.
- **Elle:** iki pencerede gamepad ile his (süre, hız, salınım), rakip penceresinde balonlu hayalet.

## Kararlar (2026-11-24)

Kullanıcı dört sorunun hepsinde öneriyi seçti:


1. **Balondaki oyuncu bir şey yapamaz** (ilk sürüm). "Zıplamaya bastıkça hızlanır" ileride düşünülebilir.
2. **Cezanın ~2–4,5 sn uzaması kabul.** Süreler `PenaltyBubbleSettingsSO`'da; BB7'de his testiyle ayarlanır.
3. **Yol kapıdan geçer.**
4. **Rakip balonu görür** (protokol v5).

## Önerilen işler

Her biri ~1–1,5 saat. `TASKS.md`'de "Çok oyunculu: balon cezası" başlığında (2026-11-29 – 2026-12-01).

| Kod | Kategori | İş |
|---|---|---|
| BB1 | #code | `PenaltyBubbleSettingsSO` + `PenaltyBubbleMath` (yol, süre, salınım) + EditMode testleri |
| BB2 | #code | `PenaltyHandler`: iris dizisi yerine balon dizisi (kalkış, yolculuk, kapıda oda + kamera + sıfırlama, varış), collider kapama, iptal; `systems/multiplayer.md` güncellemesi |
| BB3 | #test | PlayMode ceza testlerini güncelle ve yenilerini ekle (insan onayı); iki pencerede ilk deneme |
| BB4 | #art | Balon shader'ı (halftone, parlak kenar, esneme), patlama efekti, prosedürel uçma pozu |
| BB5 | #code | Ses ve titreşim (bloop, uğultu, pop) |
| BB6 | #code | Rakip görünümü: `InBubble` biti, protokol v5, `GhostView` balonu |
| BB7 | #test | Gamepad his testi, iki pencere |

## Kaynaklar

- [Bubble — Super Mario Wiki](https://www.mariowiki.com/Bubble)
- [Iwata Asks: New Super Mario Bros. Wii, Cilt 3, Bölüm 1 "Solving Multiple Problems with Bubbles"](https://www.nintendo.co.uk/Iwata-Asks-New-Super-Mario-Bros-Wii/Volume-3/1-Solving-Multiple-Problems-with-Bubbles/1-Solving-Multiple-Problems-with-Bubbles-219417.html)
- [Ghost (form) — Super Mario Wiki](https://mariowiki.com/Ghost_(form))
- [Super Mario 3D World çok oyunculu rehberi — Prima Games](https://primagames.com/eguides/super-mario-3d-world-eguide/how-to-play/multiplayer-eight-legs-are-better-than-two)
- [Super Mario Bros. Wonder çevrim içi oyun — Shacknews](https://shacknews.com/article/136869/super-mario-bros-wonder-online-ghosts)
