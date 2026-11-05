# Bozulmamalı davranışlar

*5 Kasım 2026'da geliştiriciyle yapılan soru-cevaptan derlendi. Oyuncu hareketi için yazılacak testlerin kaynağı bu liste: test, kodun bugün ne yaptığını değil, burada yazanı korur.*

Sayısal değerler `PlayerStats.asset`'ten gelir ve buraya yazılmaz; ayarlanabilirler. Korunan şey kuraldır. Bilinçli bir kuralı değiştirmeden önce önce bu dosya güncellenir.

## Dash

| ID | Kural |
|---|---|
| D1 | Oyuncunun aynı anda en fazla bir dash hakkı vardır. |
| D2 | Hak yalnızca dört durumda yenilenir: yere inmek, tek ya da çift kancaya tutunmak, launch pad ve respawn. |
| D3 | Oyun, respawn'da olduğu gibi dash hakkı dolu başlar. |

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

## Duvar

| ID | Kural |
|---|---|
| W1 | Tırmanma dayanıklılığı yere değmeden toplam ~6 sn'dir. |
| W2 | Dayanıklılık yalnızca yere inmek, launch pad ve respawn ile dolar; duvar zıplaması doldurmaz. |
| W3 | Tırmanırken kanca atılamaz, kayarken atılabilir. |
| W4 | Kayarken zıplamak kayma süresini yeniler; iki duvar arasında duvar zıplaması zinciri bilerek serbesttir. |
| W5 | Tırmanırken zıplamak düz yukarı, kayarken zıplamak duvardan dışarı atar. |
| W6 | Köşeden çıkmak için stick'i kısa süre (0.1 sn) yukarı tutmak gerekir. |

## Düzeltilen sapmalar

- **S1 ve S2, 20° ve üstü eğimler (5 Kasım 2026):** Tek kanca çarpışmayı 0.5 yarıçaplı daireyle arıyordu. Oyuncunun 1x1 kutusunun köşesi yüzeye önce değiyor, daire yüzeye ulaşamıyor ve karakter sallanmada takılı kalıyordu; 50° ve 60°'de kanca da kopmuyordu. Artık arama oyuncunun kendi kutusuyla yapılıyor. Çift kanca hâlâ 0.4 yarıçaplı daire kullanıyor ve eğimde test edilmedi.

## Testler

`Assets/Tests/PlayMode/`: `DashRulesTests`, `SwingRulesTests`, `SlingshotRulesTests`, `WallRulesTests`. D3, `Assets/Tests/EditMode/PlayerStartStateTests` içinde. Süre bantları (W1'in ~6 sn'si) `PlayerStatsSanityTests`'te. Testler gerçek `NewScene`'i yükler, oyuncuyu odalardan uzak bir alana taşır ve girdiyi taklit eder; oyun hissini ölçmezler.

## Henüz kural olmayanlar

- **Kancadan çıkış yerçekimi:** Tek kanca, çift kanca ve slingshot çıkışlarının yerçekimi farklı. Geliştirici oynayıp karar verecek, o zamana kadar test yazılmaz (`docs/feel-notlari.md`).
- **Dayanıklılık uyarısı:** Son saniyelerde uyarı isteniyor ama henüz yok ve biçimi seçilmedi (`TASKS.md`). Eklenince buraya kural olarak girer.
