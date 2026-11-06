# MP 8: Yarış kaydı, devam etme, kopma — Uygulama planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Yarış sürerken her oyuncunun ilerlemesi ayrı bir dosyaya yazılsın. Bağlantı koparsa iki oyuncu donup 30 sn beklesin; rakip dönerse 3-2-1 ile devam edilsin, dönmezse lobiye dönülsün. Lobide "Devam et" ile aynı iki kişi kaldıkları yerden sürdürebilsin.

## Kararlar

- **Kayıt:** tek yuva, `persistentDataPath/multiplayer-race.json` (JsonUtility); tek kişilik kayıttan bağımsız.
  - İçerik: yarış kimliği, mod, rakip adı, oda, bayrak sayacı, alınmış bayraklar, süre, rakibin odası ve sayacı, kayıt zamanı.
  - Oda girişinde, bayrakta, kopmada ve lobiye dönerken yazılır.
- **Devam etme:** `Hello`'ya `Resume` biti eklenir, protokol v3. Kurallar:
  - **Yeni yarış:** host rastgele kimlik yollar; istemci `Guid.Empty` yollar ve host'un kimliğini alır.
  - **Devam:** iki taraf da kayıttaki kimliği yollar.
  - **Uyuşmazlık:** iki taraf da kendi tarafında açık bir mesajla `Failed` olur:
    - biri devam ediyor, diğeri yeni yarış istiyor;
    - ya da kimlikler farklı.
  - Devam edilen yarışta mod kayıttan gelir. Yerel durum `RaceSession.RestoreLocal` ile geri yüklenir; oyuncu sahne kurulunca kayıttaki odanın başlangıç checkpoint'ine konur.
- **Kopma (yalnızca yarış sırasında):**
  - Taşıyıcı koparsa ya da 3 sn rakipten hiç paket gelmezse (`ConnectionWatch`) yarış duraklar: oyuncu donar, hayalet gizlenir, HUD'da "Rakip bağlantısı koptu, bekleniyor… (N)" görünür, kayıt yazılır.
  - Host yeniden dinler; istemci saniyede bir yeniden katılmayı dener (devam kipinde, aynı kimlikle).
  - 30 sn içinde bağlanılırsa el sıkışma yeniden yapılır; sahne yüklü olduğu için hemen `Ready`, ardından 3-2-1 ve devam.
  - Bağlanılmazsa kayıt yazılır ve lobiye dönülür.
- **Süre:** yalnızca yarış sürerken ve duraklama yokken sayılır; kayda girer.
- **Ortak parça:** oyuncuyu bir odaya yerleştirme (ışınlama, checkpoint, kamera, oda) `RoomPlacement`'a çıkarılır; ceza da onu kullanır.
- **Kapsam dışı:** yarış bitişi (Finished tetikleyicisi yok) ve bitince kaydın silinmesi.

## Review Focus

1. Uyuşmayan kayıtla bağlanma iki tarafta da açık bir mesajla reddedilmeli.
2. Devam edince alınmış bayraklar kalkık, sayaç ve oda kayıttaki gibi olmalı.
3. Kopma sırasında ceza ya da bayrak işlememeli (oyuncu donuk). Süre durmalı.
4. 30 sn dolunca kayıt yazılmalı ve lobiye dönülmeli; oyuncu donuk kalmamalı.

## Tasks

1. `HelloMessage.Resume`, codec, v3; `RaceSession.RestoreLocal`; testler.
2. `RaceSaveData` ve `RaceSaveStore` (yol verilebilir); testler.
3. `RaceLobbyFlow` devam kuralları ve var olan oturumla yeniden bağlanma; testler.
4. `ConnectionWatch` (3 sn sessizlik, 30 sn pencere); testler.
5. `RoomPlacement` çıkarılır.
6. `MultiplayerSession`: kayıt, süre, kopma ve yeniden bağlanma, devam yerleşimi. Lobi: "Devam et" anahtarı ve kayıt özeti. HUD: kopma satırı.
7. Doğrulama: EditMode ve PlayMode testleri. İki pencerede şunlar denenir:
   - Player 2 Play'den çıkıp yeniden katılır;
   - 30 sn dolunca lobiye dönülür;
   - Devam et ile sürdürülür.
