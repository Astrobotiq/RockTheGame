# Bounce pad: rastgele nota, değişen enstrüman, frekansa tepki veren neon

> Araştırma notu, 2026-11-21. Kod değişikliği yok. Konu: `LaunchPad` her zıplatmada **rastgele bir nota**, **her seferinde başka bir enstrümanla** çalsın, pad'in neon görseli o notanın frekansına göre (renk/şekil) tepki versin. Kural: kulağa **müzikal** gelmeli, kaotik değil, ve odanın arka plan müziğinin üstüne oturmalı.
>
> Hiss ve ses tercihi insanın kararı (CLAUDE.md kural 4). Buradaki sayılar başlangıç değeri, gamepad'le dinlenip ayarlanmalı.

---

## 0. Mevcut koddan çıkan iki kritik bulgu

1. **`AudioCueSO`'nun rastgele pitch aralığı notayı detune eder.** Varsayılan `pitchMin = 0.95`, `pitchMax = 1.05`. Bu yaklaşık ±0.85 yarım ses demek: nota "akortsuz" duyulur ve müziğin tonuyla çatışır. Nota çalan cue'larda **`pitchMin = pitchMax = 1`** olmalı. Varyasyonu pitch'le değil başka şeyle verin (ses şiddeti, enstrüman, hafif filtre).
2. **Altyapı zaten yarı hazır.** `AudioCuePlayParams.PitchMultiplier` var ve `AudioManager` bunu `source.pitch = Random(min,max) * PitchMultiplier` olarak uyguluyor. `LaunchPad` şu an `RaisePlayEvent(cue, position)` çağırıyor. Pitch'i notaya göre geçmek için `RaisePlayEvent(cue, params)` overload'ı kullanılabilir, `AudioManager`'a dokunmak gerekmez.
   - Sınır: `AudioCueSO.GetRandomClip()` rastgele klip seçer. Enstrüman başına birden fazla örnek (multi-sample) kullanılacaksa klip seçimi deterministik olmalı. En küçük çözüm: **her örnek bölgesi için ayrı bir `AudioCueSO`** (tek klip, pitch 1–1). `AudioCueSO`'yu değiştirmeye gerek kalmaz.
   - `AudioManager` havuzlu `AudioSource` kullanır (`initialPoolSize 10`, `maxPoolSize 30`). Pad'in kendine ait bir `AudioSource`'u yok. Bu, aşağıdaki "spectrum okuma" seçeneğini zaten zorlaştırıyor (bkz. 2d).

---

## 1. Rastgele notayı güzel duyurmak

### 1.1 Gam kuantizasyonu (en önemli adım)

Rastgele **frekans** değil, rastgele **gam derecesi** seçilir. Seçenekler:

| Gam | Neden | Risk |
|---|---|---|
| **Majör pentatonik** (1-2-3-5-6, örn. C D E G A) | Yarım ses aralığı yok, iki nota yan yana hiç "çarpmaz". Rastgeleliği en iyi tolere eden gam. | Çok "mutlu/çocuksu" duyulabilir. |
| **Minör pentatonik** (1-b3-4-5-b7, örn. A C D E G) | Aynı güvenlik, daha karanlık / rock tonu. Adı "Rock The Game" olan oyuna uygun. | Yok denecek kadar az. |
| Diyatonik (7 nota) | Daha zengin, Peggle 2 böyle yapıyor. | Müziğin akoruyla 4. ve 7. dereceler çatışabilir. Akor takibi gerekir. |
| Akor tonları (o anki akorun 1-3-5-7'si) | Müzikle %100 uyum. | Müziğin akor zaman çizelgesini bilmeyi gerektirir. |

**Hangi ton?** Odanın müziğinin tonu. Pentatonik, akor değişimlerine karşı oldukça dayanıklıdır, ama ton farklıysa her şey bozulur. Bu yüzden **ton (root + mod) oda başına veri olmalı** (müzik cue'su yanında). Odanın müziği tonsuz ya da belirsizse (ambient, perküsyon) majör/minör pentatonik serbestçe seçilebilir.

Peggle örneği: Peggle 2'de peg vuruşu "hem SFX hem müziğin parçası". Her müzik cümlesinin kendi diyatonik gamı var (26 nota, 3.5 oktav) ve müzik bir sonraki cümleye geçince gam da değişiyor. Peggle Blast'ta nota listeleri elle yazılmamış: gam adım desenlerinden (ör. 2-2-1-2-2-2-1) çalışma anında üretilmiş. Bu, bizim "NotePalette SO + root" fikrinin aynısı.

### 1.2 Tekrarı önlemek ve melodik yürüyüş

Saf rastgele (her seferinde gamdan bağımsız seçim) bile pentatonikte "yanlış" duyulmaz, ama **amaçsız** duyulur. Daha müzikal seçenekler:

- **Tekrar yasağı:** aynı nota üst üste gelmesin (bir önceki derece hariç rastgele).
- **Melodik yürüyüş (random walk):** her seferinde ±1 ya da ±2 gam derecesi adım. Ağırlık örneği: ±1 %60, ±2 %30, ±3 %10. Sınıra (oktav aralığı) gelince yön değiştir. Kulak bunu "melodi" olarak algılar.
- **Ağırlıklı dereceler:** 1. ve 5. derece (tonik ve beşli) daha sık gelsin, böylece ton "çözülür" hissi verir.
- **Shuffle bag:** gamın 5 notasını karıştırıp sırayla çal, bitince yeniden karıştır. Tekrar olmaz, dağılım eşit kalır.

### 1.3 Ardışık zıplamalarda dizi / kombo

Oyuncu yere basmadan pad'den pad'e zıplıyorsa bu bir **kombo**. Referanslar:

- **Peggle:** PopCap patentine göre topun her peg temasında ses çalıyor ve aynı atışta daha çok peg'e çarptıkça pitch yükseliyor. Eleştirmenler bunu "gamı tırmanıp yüksek skora varan" ses olarak anlatıyor. Tüm turuncu peg'ler temizlenince "Ode to Joy" finali giriyor.
- **Mario coin / 1-up zinciri:** ardışık düşman ezmede yükselen ses dizisi. Mario serisinde SFX'lerin müziğin tonundaki notalardan kurulması 1985'ten beri bilinçli bir tercih.
- **Tetris Effect:** kabul edilen girdiler, çalan şarkıyla armonize olan bir SFX ya da müzikal stinger çalıyor. Wwise içinde gama akort edilmiş müzikal cümleler var ve şarkının belirli noktalarında state'le değiştiriliyor.

Bizim için öneri: **kombo = gamda yukarı tırmanma.** 1. zıplama tonik, her ardışık zıplama +1 gam derecesi (pentatonikte 5 adımda bir oktav). Kombo kırılınca (yere basma ya da N saniye pad yok) toniğe dön. İstenirse kombo 5'te (oktav tamamlanınca) küçük bir "çözülme" stinger'ı (tonik + oktav birlikte, ya da kısa arpej).

"Rastgele nota" isteğiyle uzlaştırma: **kombo içinde yön sabit (yukarı), adım büyüklüğü rastgele (1 ya da 2 derece)**; kombo dışındaki tek zıplamada random walk.

### 1.4 Müziğin tonuna ve BPM'ine senkron

- **Ton senkronu (zorunlu):** 1.1'deki gibi, oda başına root/mod.
- **Ritim senkronu (isteğe bağlı, dikkatli):**
  - **Rez:** hedef kilitleme ve ateş sesleri, seviye müziğinin üstünde onun ritmine oturan müzikal sesler. Büyük patlama sekiz atış birikince sekizlik notalara oturtulup çalınıyor, böylece gürültüye dönmüyor. Planck geliştiricisinin yazısındaki bir başka desen: yenilen düşman bir sonraki izinli notaya kadar duman çıkarıp **o notada** patlıyor.
  - **Hi-Fi Rush:** dünya iç metronoma göre animasyon yapıyor. Tango'nun baş programcısına göre oyuncunun her vuruşunun zamanlaması ritme oturacak şekilde ayarlanıyor. Oyuncu ritim dışında bassa bile animasyon ritimde iniyor.
  - **Lumines:** skin'ler kendi müziği ve SFX'iyle geliyor. Timeline müziğin temposuna kilitli, oyun alanı 16 sütun olduğu için parçalar 4/4.
  - **Chime:** beatline müzikle birlikte ızgarayı tarıyor, yerleştirilen şekillere çarpınca olay tetikliyor. Her şekil müzik parçalarından birinin sesine bağlı.
  - **Patapon:** oyuncu dört vuruşluk davul komutu giriyor ("pata", "pon"...), Patapon'lar şarkıyla cevap veriyor. Ritim kaçınca kombo ve fever kayboluyor. Görsel çerçeve ritimle nabız gibi atıyor.
  - **Mini Metro / Mini Motorways (Disasterpeace):** ses tasarımı ile müzik arasında neredeyse fark yok. Loop yerine serialism, yani perde, şiddet ve ritim eksenlerinde ardışık veri dizileriyle ses üretiliyor. Küçük ağ küçük ses, büyüyen ağ yoğun ses; simülasyon hızlanınca müzik de hızlanıyor. Motorways'de yol gürültüsü müziğe dönüşüyor, armoni ve ritim prosedürel.
  - **Sound Shapes:** toplanan nota objeleri seviyenin müziğine katman ekliyor. Nota kaçırılırsa şarkıda boşluk kalıyor.
  - **Proteus:** prosedürel adanın sesleri oyuncunun konumuna, saate ve mevsime göre birbirine karışıyor.
  - **Super Mario Bros. Wonder:** serinin son ana oyununda SFX'lerin çoğunun müzikle armonize olduğu söyleniyor (kaynak oyunu adıyla anmıyor, büyük ihtimalle Wonder). Düşmanların müzikteki "Paah" sesine tepki vermesi NSMB serisinden.
  - **Thumper, Celeste:** bu araştırmada kaynakla doğrulanmadı, örnek olarak kullanılmadı.

  **Platformer için sorun:** zıplama fizik olayıdır, oyuncuyu bekletemezsin. Sesi bir sonraki 1/16'ya kaydırmak 120 BPM'de 125 ms'ye kadar gecikme demek. Bu, aksiyon geri bildirimi için fazla. Rez'in tolerasyonu var çünkü hedef kilitleme zaten gecikmeli. **Öneri: hibrit.** Fiziksel "thump" (perküsif, tonsuz) **anında** çalsın. Tonal notayı ancak bir sonraki ızgara çizgisi ~40–50 ms içindeyse ona kaydır, değilse anında çal. Unity'de `AudioSettings.dspTime` + `AudioSource.PlayScheduled` ile yapılır. İlk sürümde **sadece ton senkronu** yeterli. Ritim kuantizasyonu ikinci aşama.

---

## 2. Unity'de uygulama seçenekleri

### (a) Enstrüman başına tek örnek + pitch shift — **önerilen**

`pitch = 2^(yarımSes/12)` (1 yarım ses ≈ ×1.05946). Unity'de `AudioSource.pitch` resampling yapar: hız ve perde birlikte değişir. CPU'su en ucuz yöntem, glitch üretmez. Sınır: `AudioClip` için [-3, 3]. (Ayrı bir efekt olan Audio Pitch Shifter 0.5x–2x ile sınırlı.)

**Ne kadar kaydırılabilir?** Kaynaklarda kesin bir eşik yok. Pratikte bilinen kural (dinleyerek doğrulanmalı):
- Akustik enstrümanlar (piyano, marimba, gitar): **±3–5 yarım ses** sonrası formant kayar, "chipmunk" ya da "çamur" sesi başlar. Yukarı kaydırma aynı zamanda decay'i kısaltır, aşağı kaydırma uzatır.
- Sentetik / basit dalga sesleri (sine, square pluck, FM bell): **±1 oktav** rahat.
- Çözüm **multi-sample:** her minör üçlüde ya da her 4–6 yarım seste bir örnek, en yakın örnekten küçük kaydırma. Unity forumunda da geniş aralık için birden fazla örnek + küçük pitch önerilir.
- Pentatonik ve **2 oktavlık aralık** için enstrüman başına **2–3 kök örnek** (ör. C4, G4, D5) yeterli. Her biri ±3–4 yarım ses içinde kalır.

Uygunluk: mevcut `AudioCueSO` + `AudioCuePlayParams.PitchMultiplier` hattına doğrudan oturur.

### (b) Her nota için önceden render edilmiş klip

Pentatonik × 2 oktav = 10–11 nota × N enstrüman. 4 enstrümanda ~44 kısa klip. Ses kalitesi en iyi, pitch artefaktı sıfır. Bellek: kısa (≤1.5 s) mono 44.1 kHz klipler ADPCM/Vorbis ile küçük kalır. Dezavantaj: ton değişirse (oda başına farklı root) ya tüm kromatik aralık render edilir (12 × oktav) ya da yine pitch shift gerekir. **Hibrit (a)+(b)** mantıklı: kök örnekler (b) kalitesinde, ara notalar (a) ile.

### (c) `OnAudioFilterRead` ile prosedürel sentez

Karplus-Strong pluck (gürültü burst'ü + gecikme hattı + ortalama filtre), basit FM çan, additive sine. Avantajlar: sonsuz nota, sıfır asset, sesin parametreleri (parlaklık, decay) kombo ile değiştirilebilir. Görsele birebir aynı parametreler verilebilir.

Dezavantajlar:
- Audio thread'de çalışır. Unity API çağrılamaz, GC allocation yasak, thread-safe parametre geçişi gerekir.
- Mixer routing'i bileşene bağlıdır. Mevcut `AudioManager` havuzu ile entegre olmaz, ayrı bir "synth voice" havuzu gerekir.
- "Her seferinde farklı enstrüman" için her enstrüman ayrı bir sentez algoritması. İyi duyulan bir sentez yazmak, iyi bir örnek bulmaktan uzun sürer.

Sonuç: retro / chiptune estetiği istenirse güçlü. "Gerçek enstrüman çeşitliliği" isteniyorsa (a) daha hızlı sonuç verir.

### (d) Görsel için spectrum okumak mı, bilinen frekansı geçmek mi? — **bilinen frekansı geç**

- `AudioSource.GetSpectrumData` / `GetOutputData` ya kaynağın ya da (`AudioListener` sürümü) **tüm mix'in** FFT'sini verir. Tüm mix'te müzik de var: notayı ayırt etmek gürültülü ve gecikmeli. Kaynak başına okumak da pad'in kendi kaynağı olmadığı için (havuz) zor.
- Notayı biz seçiyoruz, yani frekansı (`f = 440 · 2^((midi−69)/12)`), gam derecesini, oktavı ve enstrümanı **zaten biliyoruz.** Görsele doğrudan bu veriyi geçmek deterministik, sıfır gecikmeli, test edilebilir ve ucuz.
- Genlik zarfı (glow'un sönmesi) için de FFT gerekmez. Enstrümanın bilinen attack/decay süresiyle bir eğri (AnimationCurve ya da exponential decay) yeterli.
- Spectrum okuma sadece "pad müziğin genel enerjisine de nabız atsın" gibi ayrı bir istek için anlamlı. O da `AudioListener.GetSpectrumData` ile ve düşük çözünürlükle yapılır.

**Görsel eşleme önerisi (neon):**
- **Renk = nota sınıfı (pitch class).** 12 yarım sesi renk çemberine dağıt. Pentatonikte 5 nota beş belirgin renge düşer. Alternatif: beşliler çemberine göre sıralama, bu durumda komşu notalar komşu renkler olur ve kombo tırmanışı yumuşak bir renk geçişi gibi görünür.
- **Parlaklık / kalınlık = oktav** (yüksek nota daha ince ve parlak).
- **Şekil = notanın köke oranı → Lissajous eğrisi.** Pentatonik aralıkların oranları basittir: 1:1, 9:8, 5:4, 3:2, 5:3, 2:1. `x = sin(a·t), y = sin(b·t + φ)` ile (a:b = oran) her nota kendine özgü, temiz bir neon figür çizer. Saf aralıklar sade figür, karmaşık oranlar karmaşık figür verir: "müzikal uyum = görsel uyum." Shader'da ya da `LineRenderer` ile yapılabilir (projede UI efektlerinde shader önceliği var).
- **Enstrüman = dalga karakteri** (sine için yumuşak çizgi, square/pluck için köşeli ya da kesikli çizgi), ya da enstrüman başına ikincil renk.

### Mevcut desene oturtma (öneri, kod değil)

```
NotePaletteSO              (yeni, CreateAssetMenu "Audio/Note Palette")
  - scaleSteps: int[]      // ör. minör pentatonik {0,3,5,7,10}
  - rootMidi: int          // oda override edebilir
  - octaveRange: 2
  - instruments: NoteInstrument[]
NoteInstrument (Serializable)
  - zones: { AudioCueSO cue; int rootMidi; }[]   // multi-sample bölgeleri, cue pitch 1–1
  - neonColor / shape parametreleri (opsiyonel)
BounceNoteSequencer         (runtime state: son nota, son enstrüman, kombo sayacı, zaman)
  → seçilen nota için en yakın zone'u bul, pitch = 2^((hedef−zoneRoot)/12)
  → sfxPlayChannel.RaisePlayEvent(zone.cue, new AudioCuePlayParams{ Position=..., PitchMultiplier=pitch, VolumeMultiplier=... })
  → NotePlayedEventChannelSO.Raise(NoteInfo{ midi, frequency, degree, octave, instrumentIndex, combo })
LaunchPadNeon (görsel)      NotePlayedEventChannelSO'yu dinler ya da LaunchPad'den doğrudan NoteInfo alır
```

- Oda tonu: `RoomAudioController` zaten oda müziği cue'sunu çalıyor. Oda başına `rootMidi` / gam override'ı oraya (ya da müzik cue'suna eşlik eden küçük bir SO'ya) konabilir.
- Kombo state'i pad başına değil **global** olmalı (pad'den pad'e zıplama tek kombo). Bu yüzden sequencer tek bir yerde durur: SO runtime state ya da sahnede tek bileşen.
- CLAUDE.md'deki "yeni sinyal için EventChannelSO" kuralına uygun olarak görsel ↔ ses bağlantısı `NotePlayedEventChannelSO` ile kurulabilir. Tek pad'in kendi görseli için doğrudan çağrı da yeterli olabilir.
- Gam/nota seçimi saf mantık. EditMode testine uygun (gam derecesi → MIDI, MIDI → frekans, zone seçimi, tekrar yasağı).

---

## 3. Ücretsiz enstrüman örnek kaynakları (lisanslar doğrulandı / notlu)

| Kaynak | Lisans (doğrulama) | Oyun için not |
|---|---|---|
| **University of Iowa MIS** | Resmi sayfa: "may be downloaded and used for any projects, without restrictions". 1997'den beri ücretsiz. Bildirim ricası var, şart değil. | Anekoik oda, kuru, nota nota kayıt. Mono 16-bit 44.1 kHz AIFF. Marimba, xylofon, vibrafon, gitar, piyano var. Kesip normalize etmek gerekiyor. Format kayıtları ikincil kaynaktan. |
| **Philharmonia Orchestra sound samples** | Resmi sayfa: ticari eser içinde kullanım serbest. Tek kısıt: örnekler "olduğu gibi" (örnek ya da sampler enstrümanı olarak) satılamaz/dağıtılamaz. Resmi sayfada atıf şartı yok. (GitHub aynası CC BY-SA 3.0 diyor; resmi sayfa esas alınmalı.) | Kısa MP3 notalar, orkestral. Oyun içinde işlenmiş SFX olarak kullanmak uygun. Unity build'inden ham dosyaların çıkarılabilmesi gri alan: kırpıp işlemek güvenli taraf. |
| **Versilian VCSL** (VSCO 2 CE'nin genişlemesi) | GitHub README: **CC0**. Ticari dahil, atıfsız, telifsiz. | Perküsyon, mallet (glockenspiel, kalimba vb.), klavye ve "chordophone" çeşitliliği geniş. Bu iş için **en temiz seçenek.** |
| **VSCO 2 CE** | Genelde CC0 olarak biliniyor ama bu araştırmada resmi LICENSE dosyası açılamadı. **İndirmeden önce repo LICENSE'ı kontrol edilmeli.** | Orkestral. |
| **Sonatina Symphonic Orchestra** | **CC Sampling Plus 1.0** (eski, artık önerilmeyen lisans). İçinde Iowa ve Philharmonia kaynaklı malzeme var. Renoise dönüşümü CC BY-SA 4.0'a taşınmış. | Lisans karmaşık. VCSL/Iowa varken gerek yok. |
| **Freesound** | Ses başına lisans değişiyor (CC0 / CC BY / CC BY-NC). **Sadece CC0 filtresi kullanılmalı**, CC BY'de atıf takibi, NC'de ticari yasak var. | Tek nota "kalimba C4" gibi aramalarla iyi örnekler bulunur. Her dosyanın lisansı ayrı kaydedilmeli. |

Öneri: **VCSL (CC0) + Iowa MIS.** İkisi de atıfsız ve ticari kullanıma açık. Yine de credits'e teşekkür satırı eklemek iyi pratik. İndirilen dosyaların kaynağı ve lisansı repo içinde bir `LICENSES/audio.md` benzeri dosyada tutulmalı.

---

## 4. Öneri: somut başlangıç tasarımı

**Gam ve ton**
- Oda müziğinin tonunda **minör pentatonik** (müzik majörse majör pentatonik). Ton bilinmiyorsa A minör pentatonik (A C D E G).
- Aralık: **2 oktav**, müziğin melodisinin **üstünde** (ör. A4–A6). Yüksek register müziğin bas ve orta frekanslarıyla daha az çatışır, mix'te ayrışır.

**Enstrümanlar**
- **4 enstrüman**, hepsi **kısa decay'li, perküsif-tonal:** marimba, kalimba / music box, glockenspiel, plucked synth (ya da vibrafon). Uzun sustain'li enstrümanlar (yaylı, pad) üst üste binince çamur yapar.
- Rotasyon: **bir önceki enstrüman hariç rastgele** (shuffle bag). Tamamen farklı tınılar "kaotik" algıyı artırabilir. 4 enstrümanın hepsi aynı aileden (mallet/pluck) seçilirse çeşitlilik var ama bütünlük korunur.
- Enstrüman başına 2–3 kök örnek, pitch shift ±4 yarım sesi geçmesin.

**Nota seçimi ve kombo**
- Tek zıplama: random walk (±1 %60, ±2 %30, ±3 %10), tekrar yok, 1. ve 5. dereceye hafif ağırlık.
- Kombo (yere basmadan ardışık pad, pencere ör. **1.5 s**): her zıplama **+1 ya da +2 gam derecesi yukarı**. 5. komboda oktav tamamlanınca kısa "çözülme" (tonik + oktav). Kombo kırılınca toniğe dön.
- İleride: kombo ilerledikçe parlaklık filtresi açılsın, neon kalınlaşsın.

**Mix ve müzikle ilişki**
- Ayrı bir **`SFX/Notes` mixer grubu.** Hafif reverb ya da tempoya göre kısa delay (müzikle aynı "oda"da duyulsun). Müziğin altına 150–250 Hz'i kesen low-cut.
- **Her zıplamada müziği duck etme**, pompalama yapar. Gerekirse Unity mixer'ın **Duck Volume** efekti çok hafif (−2/−3 dB, hızlı attack, ~150 ms release), ya da hiç kullanmayıp register ve EQ ile ayrıştırma.
- Not ses seviyesi müzikten biraz düşük başlasın. Bu bir "süs", ana geri bildirim fiziksel thump + titreşim (`PlatformVibrationSettingsSO.LaunchPadProfile`).

**Teknik sınırlar**
- **Voice limit: aynı anda en fazla 3 nota sesi.** 4. gelirse en eskiyi hızlı fade-out ile kes (havuz zaten en eskiyi kesiyor, ama fade'siz keser, tık sesi riski var).
- **Cooldown:** `LaunchPad.triggerCooldown = 0.2 s` zaten var. Nota için ayrıca **global minimum aralık ~80 ms**: iki pad aynı anda tetiklenirse ikinci nota ya atlanır ya da bir üst derece olarak akor gibi birlikte çalar.
- Cue'larda **`pitchMin = pitchMax = 1`** (bkz. bölüm 0). Varyasyon için `volumeMin/Max` 0.85–1.0.
- Ritim kuantizasyonu ilk sürümde **yok**. Sadece ton senkronu. Thump anında çalar.

**Görsel**
- Nota → renk (pitch class), oktav → parlaklık, köke oran → Lissajous neon figürü, enstrüman → çizgi karakteri. Veri `NoteInfo` olarak doğrudan geçer, FFT yok. Decay eğrisi enstrümanın ses decay'iyle eşleşir.

---

## 5. İnsana açık sorular

1. **Müzikler hangi tonda?** Oda başına ton verisi var mı, yoksa her müzik dosyası için root/mod elle mi girilecek? Tonsuz/ambient odalar var mı?
2. **Gam karakteri:** minör pentatonik (karanlık/rock) mı, majör pentatonik (neşeli) mi, yoksa odanın müziğine göre değişsin mi?
3. **"Her seferinde farklı enstrüman" ne kadar farklı?** Aynı aile (mallet/pluck, bütünlüklü) mi, tamamen farklı tınılar (piyano → trompet → synth) mı? İkincisi kaos riskini artırır.
4. **Kombo tırmanışı isteniyor mu,** yoksa her zıplama bağımsız rastgele mi kalsın?
5. **Ritim senkronu** isteniyor mu (BPM verisi + hibrit kuantizasyon), yoksa sadece ton yeterli mi?
6. **Estetik:** gerçek enstrüman örnekleri mi, prosedürel/chiptune sentez mi? (GDD'deki görsel dil ve halftone/Bauhaus UI yönüyle hangisi uyumlu?)
7. **Neon tepkisi:** sadece renk mi, şekil (Lissajous) de mi? Pad'in mevcut squash/stretch animasyonuyla nasıl birleşecek?
8. **Bu davranış tüm pad'lerde mi,** yoksa belirli "müzikal" pad'lerde mi (ör. bulmaca odası: doğru sırayla zıplayınca melodi çıkıyor)?

---

## Kaynaklar

**Oyunlar ve müzikal geri bildirim**
- Peggle patenti (yükselen pitch): https://patents.justia.com/patent/8398476
- Peggle 2 incelemesi (GameSpot): https://www.gamespot.com/reviews/peggle-2-review/1900-6415588/
- Peggle 2 ses (Audiogang, "Sonic Joy"): https://www.audiogang.org/peggle2-sonic-joy/
- Peggle Blast peg vuruşları ve müzik sistemi (Audiokinetic): https://blog.audiokinetic.com/en/peggle-blast-peg-hits-and-the-music-system
- Scoring Peggle Blast (Audiokinetic): https://www.audiokinetic.com/blog/scoring-peggle-blast-new-dog-old-tricks
- Peggle 2 müzikalitesi (AV Club): https://www.avclub.com/peggle-2-hearkens-back-to-the-musicality-of-classic-car-1798242529
- Rez / olay kuantizasyonu (Planckogenesis, Game Developer): https://gamedeveloper.com/design/planckogenesis-part-i-quantizing-events
- Unreal Quartz (sesi müzikal ızgaraya zamanlama, referans kavram): https://dev.epicgames.com/documentation/unreal-engine/overview-of-quartz-in-unreal-engine
- Tetris Effect ses analizi (Game Developer): https://gamedeveloper.com/audio/game-audio-analysis---tetris-effect
- Tetris Effect, ritme göre oynamak (Audiokinetic): https://blog.audiokinetic.com/en/playing-an-iconic-puzzle-game-to-the-beat/
- Hi-Fi Rush geliştirici röportajı (Unreal): https://www.unrealengine.com/en-US/developer-interviews/hi-fi-rush-was-inspired-by-shaun-of-the-dead-and-futurama
- Mini Metro müziği, Disasterpeace röportajı: https://designingsound.org/2016/02/18/the-programmed-music-of-mini-metro-interview-with-rich-vreeland-disasterpeace
- Mini Motorways, yol gürültüsünden müzik: https://www.gamedeveloper.com/design/turning-road-noise-into-music-mini-motorways
- Mini Motorways konuşması (Disasterpeace): https://disasterpeace.com/blog/mini-motorways.talk
- Mario serisinde müzikal SFX: https://medium.com/game-audio-lookout/musical-sound-effects-in-the-super-mario-series-b14872fb2d94
- Sound Shapes (Engadget incelemesi): https://www.engadget.com/2012/08/09/sound-shapes-review-a-live-concept-album/
- Proteus ses ve müzik vaka çalışması: https://www.gamedeveloper.com/audio/the-sound-and-music-of-proteus-an-academic-case-study
- Chime: https://en.wikipedia.org/wiki/Chime_(video_game)
- Lumines: https://en.wikipedia.com/wiki/Lumines
- Patapon: https://en.wikipedia.org/wiki/Patapon
- Patapon yapımcı röportajı: https://blog.playstation.com/?p=193468

**Unity**
- AudioSource.pitch (aralık [-3,3]): https://docs.unity.com/en-us/engine/7000.0/script-reference/unityengine/audiosource/pitch
- Audio Pitch Shifter Effect (0.5x–2x): https://docs.unity3d.com/Manual/class-AudioPitchShifterEffect.html
- AudioSource.pitch nasıl çalışır (resampling, multi-sample tavsiyesi): https://discussions.unity.com/t/my-audio-sources-are-unaffected-by-the-pitch-setting-fixed/383599

**Örnek kütüphaneleri ve lisanslar**
- University of Iowa MIS: https://theremin.music.uiowa.edu/MIS.html
- Philharmonia sound samples (lisans bölümü): https://philharmonia.co.uk/audio-sound-samples/
- Philharmonia GitHub aynası (CC BY-SA 3.0 iddiası): https://github.com/skratchdot/philharmonia-samples/
- Versilian VCSL (CC0): https://github.com/sgossner/VCSL
- CC0 1.0 yasal metni: https://creativecommons.org/publicdomain/zero/1.0/legalcode
- Sonatina Symphonic Orchestra (CC Sampling Plus 1.0): https://sfzinstruments.github.io/orchestra/sso
- SSO Renoise dönüşümü (CC BY-SA 4.0 notu): https://files.renoise.com/forum/downloads/Instruments/Zeograd-Sonatina_Symphonic_Orchestra_XRNI_version/description.html

**Doğrulanamayanlar:** VSCO 2 CE'nin resmi LICENSE dosyası (CC0 olduğu biliniyor, açılamadı), Thumper ve Celeste'in aksiyon başına müzikal SFX detayı, Super Mario Bros. Wonder'ın adıyla anılan bir kaynak, pitch shift için sayısal "artefakt eşiği" (pratikteki kural verildi, dinleyerek doğrulanmalı).
