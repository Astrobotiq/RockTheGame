# Sonraki oturumda yapılacaklar ve cevaplanacaklar

*22 Eylül 2026. Bu oturumdan kalan açık noktalar. Bitirdiğin maddeyi silebilirsin.*

## 1. Önce bunlara karar ver / cevap ver

- [ ] **Git geçmişindeki şirket içi doküman izi.** Şirket içi bir dokümanın adı yerel (henüz push edilmemiş) commit geçmişinde duruyor: `577078b`'in mesajında ve `577078b` / `afa7d77` commit'lerinin içeriğinde (`.gitignore`, `CLAUDE.md`, `docs/ai-workflow-onerisi.md`). Güncel dosyalar temizlendi, dokümanların kendileri silindi. Geçmişi yeniden yazmak (filter-branch veya rebase) otomatik izin sınıflandırıcısı tarafından "yıkıcı git işlemi" diye engellendi. Ya sen yaz, ya da o oturumda açıkça izin ver. **Ne olursa olsun bunu çözmeden `git push` yapma.**
- [ ] **Push kararı.** `Character_movementUpdate` üzerinde `origin`'in önünde çok sayıda yerel commit var (23 MB'lık moodboard PNG'leri `cf96d83` içinde). Push'tan sonra PNG'ler geçmişte kalır; gerekirse önce o commit'i çıkar.
- [ ] **GDD §8 "yeşil, pis asit" ile görsel uyuşmuyor.** Görsel mavi/turuncu/pembe, parlak ve şekerli; yeşil yalnızca asit şeritleri. Metni mi görsele göre, sanatı mı metne göre güncelleyeceksin? (`docs/GDD.md` §8.1)
- [ ] **Beyaz dikdörtgen ve daireler** (odalar 2, 9, 12, 16, 19, 22). Placeholder mı, tetikleyici görseli mi, bilerek mi?
- [ ] **`LinearPingPongStrategy` `period = 0` iken `NaN` konum üretiyor.** Düzeltilsin mi (örn. `period` için alt sınır)? Düzeltirsen test de eklenir.

## 2. Doğrulanması gerekenler

- [ ] **Player build hâlâ derleniyor mu?** `New-Scripts` artık ayrı bir assembly (`RockTheGame.Runtime`) ve `Room.cs` / `CameraOverrideSettings.cs` bir `EasyTextEffects.Editor...` namespace'ini import ediyor. Editor'de derleme temiz ama **gerçek bir Player build denenmedi**; runtime assembly'de editör-only bir referans build'de patlayabilir. Bir kez build al.
- [ ] **Yeni oturumda Stop hook'u devrede mi?** `/hooks` ile `.claude/settings.json` içindeki `Stop` hook'unun yüklendiğini gör. Bilerek bozuk bir `.cs` ile bir kez dene.
- [ ] **Oda 9 dikenleri** oyun içi kamera zoom'unda okunuyor mu? (Çekimler 2-5 kat uzaktandı.)
- [ ] **Play Mode görselleri:** oyuncu + HUD'lu, oyun içi kameradan çekimler (`--source screen`). Karakter, NPC illüstrasyonları ve game feel hâlâ görsellerden değerlendirilemedi.

## 3. Test kapsamını genişletmek için sana ihtiyacım var

- [ ] **"Bozulmamalı" davranışlar listesi.** `PlayerController` ve state'ler (dash, swing, slingshot, duvar) için test yazmadım: kinematik fizik test etmesi zor ve neyin doğru olduğunu ben tahmin edersem "kendi kendini notlama" olur. Örnek: "dash bir seferde bir hak verir, yere inince yenilenir", "stamina 6 sn'de biter", "slingshot havada bir kez". Bunları söyle, testleri buna göre yazayım.
- [ ] **GDD §12'deki bekleyen kararlar** (assist modu, hamster topu, multiplayer yarış/birlikte, toplanabilirlerin amacı, 60 oda zorluk eğrisi, yerden swing yalnızca sağ tetik). Hazırsan bu maddeler için "grill-me" tarzı soru-cevap oturumu yapılabilir.

## 4. Temizlik / küçük işler

- [ ] `Assets/Material/CRT/M_CRTFilter.mat` her zaman değişmiş görünüyor (`_CrtTime`, Editor kendisi yazıyor). Geri al ya da bu alanı izlemeyi bırak.
- [ ] `PhysicsTest.unity` ve `PhysicsTest2.unity` izlenmiyor (senin çalışmandı). Commit'le ya da sil.
- [ ] `docs/feel-notlari.md` boş şablon: bir sonraki tuning yaptığında ilk satırı doldur.
- [ ] GDD §12 risk: klasördeki müzik parçalarından bazıları telifli (örn. "The Skeleton Dance"). Yayın öncesi değişmeli.
- [ ] "Tüm odaları çek" akışını üçüncü kez yaptığında skill'e çevir (şu ana kadar bir kez yapıldı).

## 5. Nasıl doğrularım (kısa)

```bash
unity status                                        # state: ready olmalı
unity command run_tests --mode EditMode             # 22/22 geçmeli
bash .claude/hooks/check-compile.sh; echo $?        # 0 olmalı
git log origin/Character_movementUpdate..HEAD --oneline   # push edilmemiş commit'ler
```
