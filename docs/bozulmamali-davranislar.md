# Bozulmamalı davranışlar

*5 Kasım 2026'da geliştiriciyle yapılan soru-cevaptan derlendi. Oyuncu hareketi için yazılacak testlerin kaynağı bu liste: test, kodun bugün ne yaptığını değil, burada yazanı korur.*

Sayısal değerler `PlayerStats.asset`'ten gelir ve buraya yazılmaz; ayarlanabilirler. Korunan şey kuraldır. Bilinçli bir kuralı değiştirmeden önce önce bu dosya güncellenir.

## Dash

| ID | Kural |
|---|---|
| D1 | Oyuncunun aynı anda en fazla bir dash hakkı vardır. |
| D2 | Hak yalnızca dört durumda yenilenir: yere inmek, tek ya da çift kancaya tutunmak, launch pad ve respawn. |
| D3 | Oyun, respawn'da olduğu gibi dash hakkı dolu başlar. |
| D4 | Aynı dash, kare hızından bağımsız olarak her seferinde aynı mesafeyi gider. *(16 Kasım 2026)* |

## Swing

| ID | Kural |
|---|---|
| S1 | Sallanırken çarpılan yüzey 45°'den az eğikse karakter yere iner. |
| S2 | Çarpılan yüzey daha dikse (örneğin duvar) kanca kopar ve karakter havaya geçer. |
| S3 | Sallanırken dash atılamaz. |
| S4 | Sallanırken duvara tutunulamaz. |

## Slingshot

| ID | Kural |
|---|---|
| L1 | Hak, gerilme başlar başlamaz harcanır; gerilme sırasında tetikler bırakılsa bile geri gelmez. |
| L2 | Hak yalnızca yere inmek, slingshot yenileyici, launch pad ve respawn ile yenilenir. |
| L3 | Kancaya tutunmak slingshot hakkını yenilemez. |
| L4 | Hak yoksa aynı node'a iki kolla tutunmak çift kanca olur. |
| L5 | Slingshot çıkışı zıplama tuşundan bağımsızdır: tuşu basılı tutmak fırlatışı uzatmaz. *(16 Kasım 2026)* |

## Duvar

| ID | Kural |
|---|---|
| W1 | Tırmanma dayanıklılığı yere değmeden toplam ~6 sn'dir. |
| W2 | Dayanıklılık yalnızca yere inmek, launch pad ve respawn ile dolar; duvar zıplaması doldurmaz. |
| W3 | Tırmanırken kanca atılamaz, kayarken atılabilir. |
| W4 | Kayarken zıplamak kayma süresini yeniler; iki duvar arasında duvar zıplaması zinciri bilerek serbesttir. |
| W5 | Tırmanırken zıplamak düz yukarı, kayarken zıplamak duvardan dışarı atar. |
| W6 | Köşeden çıkmak için stick'i kısa süre (0.1 sn) yukarı tutmak gerekir. |
| W7 | Havadayken duvar `WallJumpCheckDistance` içindeyse zıplamak, temas ve stick yönü olmadan, yükselirken de duvardan dışarı atar; daha uzaktan zıplamak duvar zıplaması değildir. *(16 Kasım 2026)* |
| W8 | Duvara varmadan önce, buffer süresi içinde basılan zıplama, duvara varınca duvar zıplaması olur. *(16 Kasım 2026)* |
| W9 | Kayarken stick'i duvardan çekip hemen zıplamak duvar zıplamasıdır; duvarın dibinde dikey zıplama olmaz. *(16 Kasım 2026)* |

## Toleranslar

| ID | Kural |
|---|---|
| T1 | Yukarı giderken tavan köşesine `CeilingCornerCorrection`'dan az değen oyuncu yana kayıp geçer; daha fazla değen tavana takılır. *(16 Kasım 2026)* |
| T2 | Yatay dash'te bir köşeye `DashCornerCorrection`'dan az değen oyuncu dikey kayıp geçer; daha fazla değen durur. *(16 Kasım 2026)* |

## Havaya giriş

Yerçekimi çarpanları `PlayerStats.asset`'ten okunur; korunan şey hangi girişin hangi kuralı kullandığıdır ([`movement-core.md`](systems/movement-core.md) tablosu).

| ID | Kural |
|---|---|
| H1 | Yerden zıplamada yükseklik tuşa bağlıdır: basılı tutmak tam zıplama ve tepede kısa asılı kalma verir, erken bırakmak yükselişi keser. Düşüş hızlıdır. *(16 Kasım 2026)* |
| H2 | Tek kanca bırakınca ve launch pad'den fırlayınca yükseliş zıplama tuşundan bağımsız olarak kesik, düşüş hızlıdır; tepede asılı kalma yoktur. *(16 Kasım 2026)* |
| H3 | Zıplama boost'u olan hareketli yüzeyden zıplamada yükseliş tuştan bağımsızdır ve H2'dekinden yumuşaktır; tepede asılı kalma yoktur. *(16 Kasım 2026)* |
| H4 | Dash bitince yerçekimi sabittir: tuş, yükseliş ya da düşüş fark etmez, düşüş de hızlanmaz. *(16 Kasım 2026)* |

## Hareketli node

| ID | Kural |
|---|---|
| M1 | Kanca anchor'ı tutunulan node'u her fizik adımında izler (bir adım geriden). Node pasifleşirse anchor son konumda kalır, kanca kopmaz. Respawn node bağlantısını siler; ham nokta atanan anchor sabittir. *(17 Kasım 2026)* |
| M2 | Tek kancada oyuncu node'la birlikte ilerler ve ip boyu korunur. Node oyuncuyu dik bir yüzeye götürürse S2 geçerli: kanca kopar. *(17 Kasım 2026; atalet sonrası "node'un altında kalır"dan değişti)* |
| M3 | Hareket eden node'dan tek kancayı bırakınca node'un hızı × `MovingNodeLaunchMultiplier` (1), oyuncunun o anki salınım hızının üstüne eklenir. *(17 Kasım 2026)* |
| M4 | Çift kancada oyuncu node'larla birlikte ilerler; iki tetik birden bırakılınca dünyaya göre hız korunur (ortalama node hızı aktarılır). Biri sabit biri hareketli node'da hız sonlu kalır. *(17 Kasım 2026)* |
| M5 | Hareket eden node'dan slingshot çıkışında node hızı geçer. *(17 Kasım 2026)* |
| M6 | Node'un ivmesi salınıma geçer: duran node kalkınca oyuncu (tek ya da çift kancada) geride kalıp sallanır. *(17 Kasım 2026)* |
| M7 | Hareket eden node'u yakalayan oyuncu node'un hızını anında almaz; dünyaya göre hızı korunur, ip onu çeker. *(17 Kasım 2026)* |
| M9 | Hareketli node'dan bırakınca taşınan yatay hız `MovingNodeCarryDecayTime` (0.6 sn) içinde söner; hareketli duvardan ayrılırken taşınan hız sönmez. *(17 Kasım 2026)* |
| M8 | Sabit hızlı gerçek node'da (interpolation açık) anchor her adım aynı kaymayı görür (±%10). *(17 Kasım 2026)* |
| M1e | İp hareketli node'a ulaşır; fırlatma animasyonu yalnızca yeni tutunuşta baştan başlar. *(17 Kasım 2026)* |

## Düzeltilen sapmalar

- **S1 ve S2, 20° ve üstü eğimler (5 Kasım 2026):** Tek kanca çarpışmayı 0.5 yarıçaplı daireyle arıyordu. Oyuncunun 1x1 kutusunun köşesi yüzeye önce değiyor, daire yüzeye ulaşamıyor ve karakter sallanmada takılı kalıyordu; 50° ve 60°'de kanca da kopmuyordu. Artık arama oyuncunun kendi kutusuyla yapılıyor. Çift kanca hâlâ 0.4 yarıçaplı daire kullanıyor ve eğimde test edilmedi.

## Testler

`Assets/Tests/PlayMode/`: `DashRulesTests`, `SwingRulesTests`, `SlingshotRulesTests`, `WallRulesTests`, `ToleranceRulesTests`, `AirEntryRulesTests`, `MovingNodeRulesTests`. D3, `Assets/Tests/EditMode/PlayerStartStateTests` içinde. Süre bantları (W1'in ~6 sn'si) `PlayerStatsSanityTests`'te. Testler gerçek `NewScene`'i yükler, oyuncuyu odalardan uzak bir alana taşır ve girdiyi taklit eder; oyun hissini ölçmezler.

## Henüz kural olmayanlar

- **Çift kanca bırakma yerçekimi:** Kancadan çıkış kuralları 16 Kasım 2026'da kararlaştırıldı (H2, L5, `feel-notlari.md`). Çift kanca bırakınca sabit 1× kuralı H4 ile aynı ama ayrıca test edilmiyor.
- **Dayanıklılık uyarısı:** Son saniyelerde uyarı isteniyor ama henüz yok ve biçimi seçilmedi (`TASKS.md`). Eklenince buraya kural olarak girer.
