# Arkadaşla multiplayer testi (Steam ya da IP)

*6–7 Kasım 2026. Steam ile davet (§3a) ve IP ile bağlanma (§2, §3) ikisi de var.*

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

## 3a. Steam ile (en kolay, henüz denenmedi)

1. İkinizde de Steam açık ve giriş yapılmış olsun.
2. İkiniz de `Multiplayer.bat`'ı açın. Lobide "Bağlantı: Steam" yazmalı; yazmıyorsa Steam başlatılamamıştır (Player.log'da `[MP] Steam`), IP yoluna geçin.
3. Host **Host ol** der; Steam'in davet arayüzü açılır (açılmazsa Shift+Tab). Arkadaşını davet eder.
4. Arkadaşın daveti kabul eder; oyun kendiliğinden katılır (oyun kapalıysa Steam açar). Steam'de oyun "Spacewar" olarak görünür: kendi AppID'miz yok, Steam'in test uygulamasını kullanıyoruz.
5. Modem ve güvenlik duvarı ayarı gerekmez (Steam relay).

## 3. Farklı evlerdeyseniz (IP ile): Tailscale

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

- Son odada (21) bitiş kupası var; dokunan bitirir, iki taraf bitirince sonuç çıkar, A ile lobiye dönülür. Bitiş ve oda 21'in bayrağı şimdilik oda ortasında üst üste; geliştirici taşıyacak.
- Lobi açılırken logda birkaç DOTween uyarısı çıkıyor ("Tween startup failed"); büyük ihtimalle `NewScene` lobiye geçilmeden bir an yüklenip nesneleri animasyon başlatıyor. Oyunu etkilemiyor.
- Gerçek internette (Tailscale) hiç denenmedi. Editör ile build arasında aynı bilgisayarda doğrulandı.
- Oyuncu logu: `%USERPROFILE%\AppData\LocalLow\Astrobotiq\RockTheGame\Player.log`. Sorun olursa `[MP]` satırları buradadır.
