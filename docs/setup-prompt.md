Bu projede bir proje yönetim sistemi kuracağız. Önce planını madde madde 
göster, onayımı al, sonra uygula.

ÇALIŞMA DÜZENİM: Tam zamanlı çalışıyorum. Oyuna sadece haftada iki akşam 
bakıyorum: Pazartesi ve Cuma, saat 20:00. Hafta içine task yığma.

1. TASKS.md: Repo kökünde productivity plugin'inin şablonuyla oluştur 
   (Active / Waiting On / Someday / Done). Plugin'in dashboard.html 
   dosyasını da repo köküne kopyala. Task formatı:
   - [ ] **Başlık** #kategori - kısa bağlam, due YYYY-MM-DD
   Kategoriler: #art, #test, #level, #code

2. PLANLAMA KURALLARI (CLAUDE.md'ye yaz):
   - Due tarihi SADECE Pazartesi veya Cuma olabilir.
   - Pazartesi: #art ve #test task'ları, ek olarak en fazla 2 #level.
   - Cuma: #code ve #level task'ları.
   - Bir akşama en fazla 3 task. Her task tek akşamda (1-1.5 saat) 
     bitebilecek büyüklükte olmalı, değilse parçalara böl.
   - Sığmayan task'ı bir sonraki uygun güne kaydır ya da Someday'e koy. 
     Kaydırdığında bana söyle.
   - Tartıştığımız fikirlerden task çıkar ama TASKS.md'ye eklemeden önce 
     bana sor.
   - Bir oturumda bitmeyen task'ı sonraki uygun güne taşımayı öner.

3. CLAUDE.md: Mevcut içeriği SİLME, değiştirme. Sonuna yukarıdaki 
   planlama kurallarını ve şunları ekle:
   - Bilgi yapısı: Oyun tasarımı docs/GDD.md'de, mekanik detayları 
     docs/systems/ altında (mekanik başına bir dosya). Yeni bir özellik 
     üzerinde çalışmadan önce ilgili dosyaya bak.
   - Oyun terimleri sözlüğü: boş bir tablo olarak başlat.
   - Takvim senkronizasyonu: Bir Pazartesi veya Cuma'nın task'larını 
     eklediğinde, değiştirdiğinde veya kaydırdığında, o tarihteki 
     "RockTheGame" takvim etkinliğinin açıklamasını o akşamın güncel 
     task listesiyle güncelle (sadece o tarihteki tekrarı, serinin 
     tamamını değil). Tek bir tekrarı düzenleyemiyorsan, o tarihe task 
     listesini içeren ayrı bir etkinlik aç. Güncellediğini bana söyle.
   Eklenen bölüm kısa kalsın.

4. docs/systems/ klasörünü oluştur. docs/GDD.md'de mekanik bölümleri 
   varsa ayrı dosyalara bölmeyi öner, onayım olmadan taşıma.

5. SessionStart hook: .claude/hooks/deadlines.ps1 adında bir PowerShell 
   script'i yaz. TASKS.md'nin Active bölümünü okusun:
   - Bugün Pazartesi veya Cuma ise: due tarihi bugün olan task'ları 
     "Bu akşamın işleri" başlığıyla listelesin.
   - Diğer günlerde: bir sonraki Pazartesi/Cuma oturumunun tarihini ve 
     o günün task'larını listelesin.
   - Her durumda: due tarihi geçmiş task'ları "Gecikenler" başlığıyla 
     ayrıca listelesin.
   - TASKS.md yoksa hata vermeden çıksın.
   Bunu .claude/settings.json'a SessionStart hook'u olarak ekle, komut:
   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/deadlines.ps1
   Sonra geçici test task'ları ekleyip (biri bugüne, biri geçmiş bir 
   tarihe) script'i elle çalıştır, çıktıyı göster, test task'larını sil.

6. Google Calendar: Takvim araçlarının bu oturumda kullanılabilir olup 
   olmadığını kontrol et ve bana söyle. Kullanılabilirse iki tekrarlayan 
   etkinlik oluştur: her Pazartesi 20:00-22:00 "RockTheGame: Art & Test" 
   ve her Cuma 20:00-22:00 "RockTheGame: Code & Level". İkisinde de 1 saat 
   önceden bildirim olsun. Oluşturmadan önce detayları göster.

Bitince neyi kurduğunu ve neyi test ettiğini özetle.
