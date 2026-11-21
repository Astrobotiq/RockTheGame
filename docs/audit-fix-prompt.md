Bu Unity projesinin (2D piksel-art platformer, Unity 6.3 LTS) kapsamlı bir 
denetimini yapmanı ve bulduğun sorunları düzeltmeni istiyorum. Bu iş task 
sistemine (TASKS.md) dahil DEĞİL, TASKS.md'ye hiçbir şey ekleme. Şimdi 
çözeceğiz.

Çalışma üç aşamalı:
AŞAMA 1: Denetle ve rapor yaz (hiçbir şeyi değiştirmeden)
AŞAMA 2: Düzeltme stratejisini göster, onayımı al
AŞAMA 3: Onayladığım düzeltmeleri uygula

## 0. BAŞLAMADAN ÖNCE

1. Unity MCP araçlarının bu oturumda kullanılabilir olup olmadığını 
   kontrol et ve bana söyle. Unity Editor'daki sahneleri, GameObject'leri, 
   component'leri, asset'leri ve proje ayarlarını incelemek için ÖNCELİKLE 
   Unity MCP'yi kullan. Unity MCP yoksa Unity CLI'a (unity command / 
   unity eval) geç ve bunu bana belirt. İkisi de yoksa dur ve bana sor.
2. .unity ve .prefab dosyalarını metin olarak elle düzenleme. Sahne, 
   prefab ve asset değişikliklerini her zaman Unity Editor üzerinden 
   (Unity MCP veya CLI ile) yap.
3. Git durumunu kontrol et. Commit edilmemiş değişiklik varsa bana söyle 
   ve devam etmeden önce ne yapacağımızı sor.

## AŞAMA 1: DENETİM (SADECE OKU)

Bu aşamada hiçbir dosyayı, sahneyi, asset'i veya ayarı değiştirme. Yazacağın 
tek dosya rapor.

Önce CLAUDE.md, docs/GDD.md ve docs/systems/ altındaki dosyaları oku ki 
oyunun ne olmaya çalıştığını bil.

Her bulgu için kanıt göster: dosya yolu ve mümkünse satır numarası, ya da 
sahne/GameObject yolu. Bulguları "Doğrulandı" (açıkça gördün) ve "Şüpheli" 
(emin değilsin) olarak ayır. Emin olmadığın bir şeyi kesin bir sorun gibi 
yazma. Bir alan bu projede yoksa "Yok / uygulanmaz" diye geç, olmayan bir 
sistem için sorun uydurma.

### A. Dosya ve klasör düzeni
- Assets altındaki klasör yapısı: tutarlı bir mantık var mı (türe göre mi, 
  özelliğe göre mi), dağınık veya kökte kalmış dosyalar var mı?
- İsimlendirme tutarlılığı: script, prefab, sahne, sprite, animasyon 
  isimlerinde karışık stil (PascalCase / snake_case / boşluklu / Türkçe-
  İngilizce karışık) var mı?
- Script adı ile içindeki sınıf adı uyuşmayan dosyalar.
- Namespace kullanımı: hiç yok mu, tutarsız mı?
- Kullanılmayan asset'ler, script'ler, sahneler, materyaller (hiçbir yerden 
  referans verilmeyenler). Emin olmadıklarını "Şüpheli" işaretle.
- Aynı asset'in birden fazla kopyası.
- Test/deneme dosyalarının (örn. PhysicsTest sahnesi) ana içerikle karışık 
  durması.
- Resources klasörü kullanımı ve gereksiz yere oraya konmuş asset'ler.
- Assembly Definition (.asmdef) kullanımı ve gerekip gerekmediği.

### B. Proje genel ayarları
Aşağıdaki Project Settings ve Package ayarlarını incele, 2D piksel-art 
platformer için uygun olup olmadığını değerlendir:
- Player: şirket/ürün adı, sürüm numarası, çözünürlük ve pencere modu, 
  Color Space, Scripting Backend, API Compatibility Level, Active Input 
  Handling (eski / yeni / ikisi birden).
- Editor: Enter Play Mode Options (Domain Reload kapalı mı?), Asset 
  Serialization Mode (Force Text olmalı), Version Control ayarı 
  (Visible Meta Files olmalı).
- Quality: kullanılan kalite seviyeleri, VSync, Anti Aliasing (piksel-art 
  için genelde kapalı olmalı), Texture Quality.
- Physics 2D: Gravity, Default Contact Offset, Layer Collision Matrix 
  (gereksiz çarpışan katmanlar), Simulation Mode.
- Time: Fixed Timestep değeri.
- Graphics / URP: Render Pipeline asset'i, 2D Renderer ayarları.
- Tags and Layers: kullanılmayan ya da tutarsız isimli tag/layer/sorting 
  layer'lar.
- Build Settings / Build Profiles: sahne listesi ve sırası.
- Packages/manifest.json: kullanılmayan paketler, eski sürümde kalmış 
  paketler, preview/experimental paketler.

### C. Kod kalitesi ve hatalar (C#)
- Derleme hataları ve uyarıları (Console'u kontrol et).
- Null reference riski: kontrol edilmeyen Inspector alanları ve 
  GetComponent sonuçları.
- Update içinde pahalı çağrılar: GetComponent, Find, FindObjectOfType, 
  Camera.main, her karede yeni obje/string/list oluşturma.
- Fizik: Rigidbody2D hareketinin FixedUpdate'te yapılıp yapılmadığı, 
  input'un Update'te okunup FixedUpdate'te uygulanıp uygulanmadığı, fizik 
  objelerinin transform.position ile taşınması.
- Hardcoded sihirli sayılar.
- Eski Input Manager ile yeni Input System'in karışık kullanımı.
- TODO/FIXME yorumları, boş metotlar, hiç çağrılmayan sınıflar.

### D. Fast Enter Play Mode uyumluluğu
Domain Reload kapalıysa:
- Play Mode'lar arasında sıfırlanmayan static alanları, static event'leri 
  ve singleton instance'larını listele.
- [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] 
  ile sıfırlanmayanları işaretle.
- OnDisable/OnDestroy'da aboneliği kaldırılmayan static event'leri bul.

### E. Piksel-art ve 2D render
- Sprite import ayarları: Filter Mode (Point olmalı), Compression (None 
  olmalı), Pixels Per Unit tutarlılığı. Farklı PPU kullananları listele.
- Pixel Perfect Camera var mı, ayarları PPU ile uyumlu mu?
- Kamera takip kodu: titreme/bulanıklık riski (LateUpdate kullanımı, 
  alt piksel pozisyon, Rigidbody2D interpolation).
- Sorting Layer / Order in Layer düzeni.
- Tilemap varsa: Composite Collider kurulumu, tile arası çizgi (bleeding) 
  riski.

### F. Sahneler, prefab'lar ve referanslar
- Missing Script olan GameObject'ler.
- Kırık veya boş Inspector referansları.
- Prefab olması gerekirken sahnede elle çoğaltılmış objeler.
- Build'e dahil olmayan ama kullanılan sahneler.

### G. Mimari
- Sistemler arası sıkı bağımlılıklar, her şeyin tek bir manager'a bağlı 
  olması.
- Tekrarlanan kod.
- Temel akışlar (sahne geçişi, pause, ölüm/yeniden başlama, save/load) 
  var mı, varsa sağlam mı?

### H. Tasarım ve kod tutarlılığı
- GDD'de olup kodda olmayan, kodda olup GDD'de olmayan mekanikler.

### I. Repo sağlığı
- .gitignore Unity için doğru mu? Library/, Temp/, Obj/, Logs/, 
  UserSettings/ repoya girmiş mi?
- Git'te büyük binary dosyalar var mı, Git LFS gerekiyor mu?

### Rapor
Raporu docs/audit/ klasörüne bugünün tarihiyle yaz 
(örn. docs/audit/2026-09-28-audit.md). İçeriği:
1. Özet (en fazla 10 satır): genel durum ve en kritik 3 sorun.
2. Bulgular tablosu: Kimlik (A1, B2...), başlık, önem (Kritik / Yüksek / 
   Orta / Düşük), durum (Doğrulandı / Şüpheli), kanıt, kısa açıklama.
3. Şüpheli bulgular için Unity'de elle nasıl doğrulayacağım (1-2 adım).

## AŞAMA 2: DÜZELTME STRATEJİSİ (ONAY AL)

Rapordan sonra sohbette bana bir düzeltme stratejisi göster:
- Düzeltmeleri gruplara ayır ve sırala. Önce diğer düzeltmelerin önünü 
  açanlar (örn. derleme hataları, klasör düzeni), sonra kritik hatalar, 
  sonra ayarlar, en son temizlik.
- Her grup için ne yapacağını, hangi dosyaları/ayarları etkileyeceğini ve 
  riskini (düşük / orta / yüksek) yaz.
- Birbirine bağımlı düzeltmeleri belirt.
- Hangi düzeltmelerin oyunun hissini veya davranışını değiştirebileceğini 
  ayrıca işaretle (örn. fizik ayarları, Fixed Timestep, zıplama değerleri). 
  Bunları ben karar vermeden yapma.

Stratejiyi gösterdikten sonra DUR ve hangi grupları onayladığımı sor.

## AŞAMA 3: UYGULAMA

Onayladığım gruplar için:
1. Başlamadan önce "audit-fixes" adında yeni bir git branch'i aç.
2. Grupları sırayla uygula. Her grup bitince:
   - Unity Console'da yeni hata/uyarı çıkıp çıkmadığını kontrol et.
   - Grubu ayrı bir commit olarak kaydet (açıklayıcı commit mesajıyla), 
     böylece gerekirse tek tek geri alabilelim.
   - Ne yaptığını kısaca bana söyle.
3. DOSYA TAŞIMA VE YENİDEN ADLANDIRMA: Asset'leri ASLA dosya sistemi 
   komutlarıyla (mv, Move-Item, git mv) taşıma veya yeniden adlandırma. 
   Bu, .meta dosyalarını ve GUID referanslarını koparır. Her zaman Unity 
   üzerinden taşı (AssetDatabase.MoveAsset / RenameAsset, Unity MCP veya 
   unity eval ile). Script taşırken sınıf adı ile dosya adının uyumunu 
   koru.
4. SİLME: Bir asset'i silmeden önce hiçbir yerden referans verilmediğini 
   doğrula ve silinecekleri listeleyip bana onaylat. "Şüpheli" kullanılmayan 
   asset'leri silme, raporda bırak.
5. Proje ayarlarını değiştirirken önceki ve yeni değeri bana göster.
6. Beklenmedik bir durumla karşılaşırsan (bir düzeltme başka bir şeyi 
   bozarsa, bir referans koparsa) dur ve bana sor, kendi başına geniş 
   çaplı bir çözüm deneme.

## BİTİRİNCE

Şunları özetle:
- Uygulanan düzeltmeler (commit listesiyle)
- Uygulanmayan veya ertelenen bulgular ve nedeni
- Benim Unity'de elle test etmem gerekenler (Play Mode'da kontrol edilecek 
  şeyler)
- Branch'i main'e birleştirmeden önce yapmam gereken kontroller
