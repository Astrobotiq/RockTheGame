# Solo Unity geliştirici için AI iş akışı önerisi

*21 Eylül 2026. İnternet taramasına ve Spark mimari dokümanının incelenmesine dayanır.*

## Kaynak kalitesi hakkında

Tam olarak "kendi başına proje geliştiren biri şunu kullanıyor" diyen sağlam bir kaynak bulunamadı. Bulunanlar çoğunlukla blog yazıları, satıcı sayfaları ve GitHub araç setleri. Reddit veya forum tartışması çıkmadı. Aşağıdakiler kesin gerçek değil, tekrar eden örüntülerdir. Summer Engine yazısı kendi ürününü satan bir sayfa, Claude Lab yazısı genel bir rehber. İkisi de bağımsız veri sunmuyor.

## İnsanlar ne yapıyor

- **CLAUDE.md en yüksek getirili adım.** Etkili olanlar genel değil, somut kurallar içeriyor. Örnek: "çıplak `Task` yasak, UniTask kullan", çünkü Claude aksi hâlde `await Task.Delay()` yazıyor ve MonoBehaviour yaşam döngüsünü izlemiyor.
- **Hook'lar:** bir `.cs` dosyası düzenlenince otomatik derleme, commit öncesi etkilenen testlerin çalıştırılması.
- **Editor'u görebilen araç:** Unity MCP veya CLI ile sahne, konsol ve ekran görüntüsü. Bu projede Pipeline ile zaten var.
- **Testler headless koşturuluyor** ve çıktı filtrelenip ajana yalnızca hatalar veriliyor.
- **Küçük adımlar, sık commit.** Bir kaynak "tüm oyunu tek prompt'ta istemek, bozuk bir proje ve hangi adımın bozduğunu bilmemek demektir" diyor.
- **Sınır:** AI hareket, durum makineleri ve UI bağlamada iyi, ama oyun hissi, denge ve entegrasyon hatalarında zayıflıyor. Bir kaynağın tahmini "ilk %70". Eğlenceli olup olmadığına karar vermek hâlâ geliştiricinin işi.

Aynı kaynaklar "en iyi solo akış, en az araca sahip olandır" diyor. "49 ajan, 72 skill" gibi dev stüdyo şablonlarının işe yaradığına dair kanıt görülmedi.

## Bu proje için öneri

1. **Önce git.** `docs/`, `PhysicsTest*.unity` ve son oturumdaki değişiklikler izlenmiyor. Her göreve commit veya branch'le başla. Böylece kod incelemesi "her dosyayı oku" yerine `git diff` okumaya iner.
2. **Derleme kapısı, hook olarak.** `.cs` düzenlenince `unity command recompile` ve konsol hata kontrolü. Komutların bu Pipeline sürümünde tam bu adlarla çalıştığı henüz denenmedi. Headless batch mod yerine açık Editor kullanılmalı, çünkü Unity aynı projeyi iki kez açmaz.
3. **Az sayıda EditMode testi, yalnızca saf mantık için.** Stamina zamanlayıcıları, dash/slingshot hakkı, oda komşuluğu, `PlayerStatsSO` tabanlı durum geçişleri. `KinematicPhysicsHandler` test etmesi zor, sonraya bırakılır.
4. **Hissi kanıta bağlama.** Tuning `PlayerStatsSO` üzerinden yapılır. AI'dan yalnızca SO değerlerini değiştirmesi istenir, gamepad'le sen denersin. Kısa "his notları" (örneğin "dash çok kısa") sonraki değişikliklerin ölçütü olur.
5. **Bir mekanik, bir branch, küçük diff.** CLAUDE.md'ye "en küçük diff, gereksiz soyutlama yok" kuralı eklenebilir.
6. **Skill, subagent ve linter yok.** Aynı iş akışı üç kez tekrarlanınca skill yazılır. "Tüm odaları çek" akışı ilk aday, ama bir kez yapıldı, henüz erken.

## "Kod bakma zorunluluğu" için gerçekçi beklenti

Kaynakların hiçbiri bunu kaldırdığını söylemiyor. Biri "AI geliştiriciyi hızlandırır ama yargısının yerini almaz, her değişikliği oku ve test et, özellikle performans kritik olanları" diyor. Yük iki katmana ayrılır:

- **Kapılara bırakılabilecekler:** derleme, testlerin geçmesi, diff'in küçük kalması.
- **Geliştiricide kalanlar:** oyun hissi, tasarım kararları, performans açısından kritik yerler.

## Kaynaklar

- [How to Use Claude Code as a Solo Developer in 2026](https://devtoolpicks.com/blog/how-to-use-claude-code-solo-developer-2026)
- [Claude Code Hooks: Automate Your Indie Game Dev Workflow](https://www.arsturn.com/blog/automate-your-game-dev-grind-a-guide-to-claude-code-hooks)
- [Claude Code for Unity Game Development: Practical | Claude Lab](https://claudelab.net/en/articles/claude-code/claude-code-unity-game-development-complete)
- [AI Tools for Indie Game Developers (The Honest 2026 Stack) | Summer Engine](https://www.summerengine.com/blog/ai-tools-for-indie-game-developers)
- [everything-claude-unity (GitHub)](https://github.com/XeldarAlz/everything-claude-unity)
- [Claude-Code-Game-Studios (GitHub)](https://github.com/donchitos/claude-code-game-studios)
