# Arkadaşla multiplayer testi (Unity Transport, Steam öncesi)

*6 Kasım 2026. Steam (MP 9) gelince bu yol kalkar; davetle girilir.*

## 1. Build al

Editörde **Rock The Game → Build → Windows (multiplayer testi)**. Çıktı `Builds/Windows/` klasörüne yazılır (git dışında):

- `RockTheGame.exe`: tek kişilik oyun (normal açılış).
- `Multiplayer.bat`: oyunu doğrudan lobiden açar.
- `Host.bat`: lobiden açar ve kendiliğinden host olur.
- `OKU-BENI.txt`: bu sayfanın kısası.

Arkadaşına klasörü zipleyip gönder. `RockTheGame_BurstDebugInformation_DoNotShip` klasörünü zipe koyma. **İkiniz de aynı build'i kullanmalısınız**; protokol sürümü farklıysa bağlantı reddedilir.

## 2. Aynı ağdaysanız

1. Host `Host.bat`'ı açar. Windows Güvenlik Duvarı sorarsa **İzin ver** der.
2. Host IP'sini öğrenir: `cmd` → `ipconfig` → "IPv4 Address" (192.168.x.x).
3. Arkadaşın `Multiplayer.bat`'ı açar, adres alanına o IP'yi yazar ve **Katıl** der.

## 3. Farklı evlerdeyseniz: Tailscale (önerilen)

Bağlantı doğrudan IP üzerinden kuruluyor (UDP 7777); relay sunucusu yok. Tailscale ikinizi aynı sanal ağa koyar, modem ayarı gerekmez.

1. İkiniz de <https://tailscale.com/download> adresinden kurup giriş yapın.
2. Host, Tailscale yönetim panelinden arkadaşını kendi ağına davet eder (Users → Invite users), ya da kendi bilgisayarını onunla paylaşır (Machines → bilgisayarın → Share).
3. Host'un Tailscale IP'si tepsi simgesinde görünür (100.x.x.x).
4. Host `Host.bat`'ı açar, güvenlik duvarı sorarsa izin verir.
5. Arkadaşın `Multiplayer.bat` ile girer, 100.x.x.x adresini yazar ve **Katıl** der. Kısayol da olur: `RockTheGame.exe -mp-join 100.x.x.x`.

Alternatifler:
- **Radmin VPN** (ücretsiz, aynı mantık).
- **Port yönlendirme:** host modeminde UDP 7777'yi yönlendirir, arkadaşı host'un dış IP'sini yazar. Operatör CGNAT kullanıyorsa çalışmaz.

## 4. Oyunda ne olur

- Bağlanınca ikiniz de oyunu yüklersiniz. HUD'da 3-2-1 sayılır, **BAŞLA!**'da serbest kalırsınız.
- Rakip yarı saydam bir hayalettir. Aynı ya da komşu odadaysanız görünür.
- **Yarış modunda** her odada bir bayrak vardır. 3 bayrak toplayan, rakibini bir oda geri gönderir.
- Sağ üstte "Bayrak N/3" ve "Rakip: Oda X · Bayrak N/3" yazar.
- **Bağlantı koparsa** ikiniz de donarsınız ve 30 sn beklenir. Dönülürse 3-2-1 ile sürer; dönülmezse yarış kaydedilir ve lobiye dönülür.
- **Kaldığınız yerden devam etmek için:** ikiniz de lobide "Devam et: Açık" iken Host / Katıl deyin.
- Oyun gamepad ile oynanır. Lobide gamepad ile gezilir, yalnız IP klavyeyle yazılır.

## 5. Bilinen eksikler

- Yarışın bitişi yok (son odada bir şey olmaz). Kayıt silinmez; yeni yarış için Devam et'i kapatın.
- Lobi açılırken logda iki DOTween uyarısı çıkıyor ("Tween startup failed"); oyunu etkilemiyor.
- Gerçek internette (Tailscale) hiç denenmedi. Editör ile build arasında aynı bilgisayarda doğrulandı.
- Oyuncu logu: `%USERPROFILE%\AppData\LocalLow\Astrobotiq\RockTheGame\Player.log`. Sorun olursa `[MP]` satırları buradadır.
