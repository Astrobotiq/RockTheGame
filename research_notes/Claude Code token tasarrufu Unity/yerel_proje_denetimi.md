# Yerel proje denetimi: RockTheGame Claude Code token yuku

Olcum tarihi 2026-11-11. Tahmin kurali: Ingilizce ~chars/4, Turkce ~chars/3 token. Tum "token" rakamlari TAHMINDIR, tum bayt/satir/dosya sayilari olculmustur. Kaynak: yerel dosya sistemi (web kaynagi yok).

## 1. Her oturumda otomatik yuklenen baglam (CLAUDE.md, MEMORY.md, docs)

### Takeaway
Her oturumda sabit yuk: CLAUDE.md (13.6 KB, ~3.8-4k token) + MEMORY.md indeksi (1.5 KB, ~0.5k) + SessionStart hook ciktisi (485 karakter, ~0.15k). Asil buyuk kalem skill/plugin listesi (asagida). Docs ve TASKS.md istege bagli okunur; ama CLAUDE.md "oku" dedigi icin on-demand maliyet yuksek (docs/systems 102 KB toplam).

### Cited Findings (yerel olcum)
- CLAUDE.md: 13,584 B, 71 satir (karisik Turkce/Ingilizce, ~3.8-4.5k token tahmini). Her oturum yuklenir. `D:\PERSONAL\Unity\Projects\RockTheGame\CLAUDE.md`
- MEMORY.md (indeks): 1,551 B, 9 satir (~0.5k token). Otomatik yuklenir. Memory dizini: `C:\Users\e-r-e\.claude\projects\D--PERSONAL-Unity-Projects-RockTheGame\memory\`
- Memory dosyalari (indeks haric, istek uzerine okunur; toplam 12,940 B, ~4k token ancak hepsi birden yuklenmez): unity-editor-cli-integration 2,973 B; work-rhythm-daily 1,633 B (20 satir); prefer-shader-for-ui-effects 1,482; ui-art-direction-bauhaus-brutalist 1,477; prefer-existing-libraries 1,372; ui-must-be-alive 1,132; gamepad-only-by-design 1,111; gdd-consistency-check 933; no-commit-after-every-change 827. Toplam 10 dosya (indeks dahil) 14,491 B.
- Tekrar/cakisma: `ui-must-be-alive` memory'si CLAUDE.md "UI work" paragrafi ve docs/systems/ui-motion.md ile ayni kuralin 3. kopyasi; `work-rhythm-daily` memory'si CLAUDE.md "Planlama kurallari" ile ayni; `gdd-consistency-check` CLAUDE.md "Design docs" ile ayni; `no-commit-after-every-change` CLAUDE.md kural 5 ile kismen cakisiyor. Indeks satirlari bunlari ayri ayri listeliyor.
- CLAUDE.md icindeki "Oyun terimleri sozlugu" tablosu bos (basliktan baska satir yok), "Proje yonetimi ve planlama" (~2.5 KB) her oturum yukleniyor ama sadece planlama gorevlerinde gerekli; "Takvim senkronizasyonu" da yalniz takvim islerinde.
- CLAUDE.md "Architecture" bolumu (~6 KB, ~45% dosya) kod okuyarak da cikarilabilir ve docs/systems ile kismen yinelenir.
- TASKS.md: 18,218 B, 85 satir (~6k token); bolumler Active/Waiting On/Someday/Done. Otomatik yuklenmez, ama CLAUDE.md ve skill'ler (productivity:task-management) okutabilir; Done bolumu sismis olabilir (arsivleme adayi).
- docs/GDD.md: 19,293 B (~6k token). CLAUDE.md ve memory "yeni mekanikten once oku" diyor.
- docs/systems/*.md: 11 dosya, toplam 102,262 B (~30k+ token tum set). En buyukleri: multiplayer.md 27,658 B; grapple.md 11,020; pause.md 8,815; platforms.md 8,731; collectibles.md 8,653; movement-core.md 8,571; ui-motion.md 7,672; wall-movement.md 7,198; death-respawn.md 6,489; hazards.md 5,130; README.md 2,325. Yalnizca ilgili mekanik icin okunmali; multiplayer.md tek basina ~8-9k token.
- Diger buyuk docs: docs/audit/2026-09-26-audit.md 24,021 B ve ...-yapilanlar.md 20,879 B (eski denetim); docs/kalan-isler.md 15,619; docs/superpowers/plans/ 8 plan, toplam ~62 KB (2026-11-06 MP planlari, buyuk olasilikla bitmis); docs/audit-fix-prompt.md 8,944 (izlenmiyor).

### Inferences
- Sabit CLAUDE.md+MEMORY yuku (~4.5k token) tek basina buyuk degil; kazanc: Architecture'i docs/systems'e tasimak, planlama/takvim bolumunu ayri dosyaya (TASKS.md ustune ya da skill/komut) almak ile ~1.5-2.5k token/oturum.
- Asil risk "agent bir dosyayi tamamen okur" davranisi: GDD + ilgili system doc + TASKS.md birlikte ~15-20k token.

### Gaps
- Gercek yuklu token sayisi `/context` ile dogrulanmadi (bu denetimde calistirilamadi).

## 2. .claude/ klasoru, hook'lar, izinler

### Takeaway
Hook'lar kucuk. SessionStart hook ~485 karakter/12 satir (~150-200 token) enjekte ediyor, ucuz. Stop hook ciktisi basarida sifir; maliyet token degil sure (duvar saati) ve hatada stderr.

### Cited Findings
- `.claude/` icerigi: settings.json 954 B, settings.local.json 74 B (yalnizca `Bash(git fetch *)` allow), hooks/check-compile.sh 3,245 B, hooks/deadlines.ps1 3,789 B. Proje skill'i yok (`.claude/skills` yok), `.mcp.json` yok.
- settings.json: `ask: Bash(git push*)`; `deny`: Edit/Write `**/*.unity`, git push --force/-f, reset --hard, clean -f, checkout -- *, branch -D. Hook'lar: SessionStart (deadlines.ps1, timeout 20) ve Stop (check-compile.sh, timeout 240).
- SessionStart (deadlines.ps1) calistirildi: cikti 485 karakter, 12 satir: bugunun/yarinin task'lari + "Gecikenler". Her oturum acilisinda baglama girer (~150-200 token). Sorun: TASKS.md'yi UTF-8 okuyup yazdiriyor; PS 5.1 icin `[Console]::OutputEncoding` UTF8 set edilmis. Gecikenler listesi sinirsiz buyuyebilir (su an 2 kalem).
- Stop hook (check-compile.sh), her tur sonunda: (1) `git ls-files -co --exclude-standard -- '*.cs' '*.asmdef' | sort | xargs sha1sum | sha1sum` ile parmak izi hesaplar; son dogrulanmis parmak iziyle ayniysa hemen `exit 0` (cikti yok). (2) `unity status` ile Editor hazir degilse tek satir uyari stderr'e (exit 0). (3) Parmak izi degismisse `unity command recompile`, 45x2sn'ye kadar poll, console_status, hata varsa Editor.log'dan en fazla 15 `error CS` satiri (stderr, exit 2), sonra `run_tests --mode EditMode --timeout 110`, hata varsa 2 satirlik mesaj. MAX_BLOCKS=3. Basari/atlama yolunda token maliyeti ~0; hata yolunda ~200-600 token + ajan duzeltme turu.
- Stop hook zaten "yalnizca .cs/.asmdef degisince" calisiyor (icerik parmak izi); git status'a bagli degil. Dolayisiyla "degisiklik yoksa atla" ek optimizasyonu zaten var. Ince nokta: parmak izi `*.cs` tum repo (Plugins/DOTween dahil, 216 dosya) uzerinde sha1sum, her tur ~birkac yuz ms-1 sn; ayrica Unity'de her tur sonrasi calisan state dosyasi `$TMPDIR`'da, yeni oturumda veya temp temizlenince ilk turda tam derleme+test kosar.
- `git status` suanda 46 degisiklik/izlenmeyen dosya (M .asset/.mat otomasyon gurultusu dahil) – `git status` ciktisi her oturum basinda sistem baglamina giriyor (yukaridaki gitStatus blogu ~700 token).

### Inferences
- Stop hook token acisindan zaten verimli; ek sadelestirme: parmak izini yalniz `Assets/Scripts`, `Assets/Tests`, `*.asmdef` ile sinirlamak (sure kazanci, token degil). Hata yolu cikti limiti (15 satir) iyi.
- Gecikenler listesine ust sinir (ornegin 5) koymak SessionStart ciktisini sabitler.

### Gaps
- Hook'un gercek calisma suresi (sn) olculmedi; Unity Editor'u calistirmak read-only kapsam disinda.

## 3. Arama/okuma dislamalari ve kod/sahne boyutlari

### Takeaway
Projede Claude'u buyuk dosyalardan koruyan bir deny kurali YOK (yalniz `*.unity` Edit/Write engelli, Read/Grep serbest). Tek koruma `.gitignore`; Grep/Glob bunu (ripgrep) uyguladigi icin Library/Logs/obj/Builds vb. aramalarda atlanir, ama dogrudan Read engelsiz. `.claudeignore` yok.

### Cited Findings
- `.gitignore` dislar: /Library, /Temp, /obj, /Build(s), /Logs, /UserSettings, /ProfilerCaptures, *.csproj, *.sln, .vs. Dislanmayanlar (Grep'in tarayabilecegi): `.idea` (12 dosya 4 MB, izlenmiyor mu belirsiz), `Assets/**` tum YAML, `docs/moodboard/*.png`, `docs/ui-research/**`, `research_notes`, `dashboard.html`.
- Boyutlar (olculen): Library 46,480 dosya / 3,143 MB; Builds 202 dosya / 116 MB; Logs 10 dosya 6 MB; obj 20 dosya 2 MB; .idea 4 MB.
- `settings.json` deny kurali yalniz git ve `*.unity` Edit/Write. `Read(**/*.unity)`, `*.prefab`, `*.asset`, `*.meta`, TMP font asset, `TestResults.xml` icin DENY YOK. Yani yanlislikla `Read NewScene.unity` 9.4 MB (~2.4M token, baglam siginirdi) acilabilir; Read araci buyuk dosyayi reddeder ama Grep `-n` ile sahne icerigi satir satir donebilir.
- YAML boyutlari: `Assets/Scenes/NewScene.unity` 9,399,887 B; 9 sahne 10.5 MB toplam; 90 prefab 43.2 MB; en buyuk prefab'lar RoomNew-19 Variant 4.66 MB, RoomNew-11 Variant 4.36 MB, RoomNew-15 Variant 4.19 MB, RoomNew-6 2.75 MB, RoomNew-16 2.37 MB, RoomNew-7 2.36 MB, RoomNew-13 2.29 MB; 783 .asset 16.8 MB; en buyuk asset'ler TMP font atlaslari: Jost-800-Heavy SDF.asset 8.5 MB, LiberationSans SDF 2.26 MB, SpaceMono-Bold/Regular SDF ~2.2 MB her biri. 1,707 .meta dosyasi 2.3 MB. Hepsi tek satirlik buyuk YAML/base64 atlas icerir: Grep sonucu patlatir.
- `Assets/_Recovery/0.unity` (+ .meta) izlenmeyen (untracked) bir kurtarma sahnesi; arama sonuclarinda ve `Assets/Scenes/PhysicsTest*.unity` (untracked) cop olarak cikar.
- TestResults.xml proje icinde bulunmadi (`%USERPROFILE%/AppData/LocalLow/Astrobotiq/RockTheGame/` altinda, repo disi, dolayisiyla repo aramasina girmez).
- Kod hacmi: Assets/Scripts 182 .cs / 20,786 satir / 819,834 B (~205k token tum set); Assets/Tests 34 .cs / 4,740 satir / 184,284 B (~46k token). Assets altinda baska .cs: Plugins/Demigiant (DOTween) 9 dosya.
- En buyuk 10 .cs (satir; B): DOTweenModuleUI.cs 662 (43,254, plugin, okunmamali); MultiplayerSession.cs 568 (24,431); AudioManager.cs 553 (20,713); PlayerController.cs 479 (17,972); CameraController.cs 442 (16,942); PauseMenuPanel.cs 422 (18,781); UiMenuMotion.cs 399 (17,774); PlayerEyesController.cs 390 (14,049); DOTweenModuleUnityVersion.cs 389 (18,321, plugin); HudCounterPanel.cs 375 (16,430). Hepsi okunabilir boyutta (<~11k token); sorun tek dosya degil, sahne/prefab/atlas YAML.

### Inferences
- Onerilen deny: `Read(**/*.unity)`, `Read(**/*.prefab)`, `Read(**/*SDF.asset)`, `Read(Assets/Plugins/**)`, `Read(Library/**)`, `Read(Builds/**)`, `Read(Logs/**)`, `Read(docs/moodboard/**)`, `Read(docs/ui-research/*.html)`, `Read(dashboard.html)` ve gerekirse `Read(Assets/_Recovery/**)`. Bu, kazara 9 MB / 8.5 MB okumalari engeller (token deger kaybi dev). Unity islemleri zaten `unity command eval` ile yapiliyor (CLAUDE.md), yani okuma ihtiyaci yok.
- `.meta`/.asset arama sismesi icin `.rgignore`/`.ignore` (ripgrep) dosyasi alternatif olabilir; denetimde dogrulanmadi.

### Gaps
- Read/Grep'in `.gitignore`'i bu kurulumda tam olarak nasil uyguladigi test edilmedi.

## 4. Plugin / skill / MCP envanteri

### Takeaway
Bu denetimde gorulen oturumun skill listesi ~110 giris; skill adi+aciklamalari her oturumun sistem isteminde. Unity disi buyuk kismi gereksiz. CLI'daki gercek yuk bu uygulamadan (Cowork/desktop) biraz farkli olabilir.

### Cited Findings
- `~/.claude/settings.json` `enabledPlugins`: unity@unity-agent-plugin (v0.1.6-beta, 31 SKILL.md), superpowers@claude-plugins-official (v7.0.0, 15 skill), skill-creator@claude-plugins-official (1 skill). Model "opus". `extraKnownMarketplaces`: unity-agent-plugin (GitHub). Not: cache'te superpowers 6.4.1 ve skill-creator'in 8 eski surum dizini de duruyor (disk, token degil).
- Kullanici skill dizini `~/.claude/skills`: frontend-design (9,461 B), supabase (14,462 B), supabase-postgres-best-practices (3,295 B), vercel-react-best-practices (7,400 B), web-design-guidelines (1,270 B) + `synced/<id>/` altinda desktop'tan senkronlanan skill'ler (docs, docx, google-workspace 13,869 B, import-memory, learn, morning 18,163 B, pdf, pptx 28,618 B, requirements-interview, skill-creator 33,351 B, theme-factory, web-artifacts-builder, xlsx). Yalniz name+description sistem isteminde (govde yalniz cagrilinca yuklenir).
- Bu oturumun gorunen skill listesinde (agent baglami) ~110 skill var: Unity disi ve bu projeyle ilgisiz olanlar: supabase, supabase-postgres-best-practices, vercel-react-best-practices, web-design-guidelines, anthropic-skills (career-helper, docx, pptx, xlsx, pdf, google-workspace, morning, portfolio-copy, chrome/computer-use...), proficiently:* (apply, cover-letter, job-search, jobsearch-telegram, network-scan, setup, tailor-resume = is arama), engineering:* (architecture, deploy-checklist, incident-response, standup, system-design...), design/design-skills:* (~20), productivity:*, dataviz, artifact-*. Unity'ye ait ve ilgili: unity:2d-pixel-perfect, unity:ui-ugui, unity:unity-cli, unity-urp-*, shader-graph; cogu digeri (levelplay, IAP, vivox, localization, build-live-game, optimize-web) bu proje icin muhtemelen gereksiz.
- MCP: proje `.mcp.json` yok. Bu oturumda gorunen MCP sunuculari: Google Calendar, Claude Docs, claude-in-chrome, Claude_Browser, terminal, scheduled-tasks, mcp-registry + auth bekleyen plugin MCP'leri (figma, intercom, asana, atlassian, datadog, github, linear, notion, pagerduty, slack, clickup, monday). Deferred tool adlari sistem isteminde adlariyla listelenir (~100+ ad).
- Maliyet tahmini (TAHMIN, olculmedi): bu skill listesi ~110 giris x ~35-60 token = ~4-7k token; deferred tool adlari ~100+ ad x ~12 token = ~1.5k; MCP sunucu talimatlari ~0.5k. Toplam "kullanicinin hic yazmadigi" sabit yuk ~7-10k token. Bir bolumu CLI'ya ozel degil (desktop/Cowork eklentisi), kullanici CLI'da yalniz unity+superpowers+skill-creator plugin'lerini ve ~/.claude/skills'i gorur: ~31+15+1 + 5 + ~12 synced = ~64 skill.

### Inferences
- En yuksek kazanc/risk orani: `~/.claude/skills` icinden supabase, supabase-postgres-best-practices, vercel-react-best-practices, web-design-guidelines (Unity solo projesinde alakasiz) ve synced is arama/ofis skill'lerini bu proje icin devre disi birakmak veya baska scope'a tasimak; unity plugin'inde kullanilmayan ~20 skill'i kapatmak mumkun degilse (plugin bazli aciliyor) plugin'i tutmak mantikli, ama alakasiz plugin'ler (proficiently, engineering, design vb.) hesapta/desktop tarafinda kapatilabilir.
- superpowers skill'leri (brainstorming zorunlu "MUST use before any creative work", test-driven-development, writing-plans) CLAUDE.md "smallest diff"/"plan before multi-file" kurallariyla ve "human writes tests" kurali ile cakisabilir; yan etki: her kucuk is icin brainstorming skill govdesi yuklenir (~gercek token yuku). Olculmedi.

### Gaps
- Skill listesi karakter/token sayisi bu denetimde olculemedi (sistem istemi dosyada degil); `/context` veya `/skills` ile dogrulanmali.
- CLI ile desktop'taki skill listelerinin farki dogrulanmadi.

## 5. Yaniltici / yanlislikla okunabilecek dosyalar

### Takeaway
Repo kokunde ve docs'ta, agent'in aramada karsilasabilecegi bir dizi buyuk, eski veya izlenmeyen dosya var; en tehlikelisi dev HTML/PNG/sahne dosyalari.

### Cited Findings
- Kok: `dashboard.html` 101,514 B (git'te izlenen, 26 Eyl; ~25-30k token, buyuk olasilikla eski dashboard), `README.md` (legacy sistemi anlatiyor, CLAUDE.md "guncel degil" diyor), `.superpowers/sdd`, `.idea/` (4 MB), `reports/` (bos), `ProfilerCaptures/` (bos), `Builds/` (116 MB, ignored), `*.csproj/*.sln` (ignored ama diskte; Grep'te gurultu degil).
- `research_notes/` bu denetim ciktisi dizini disinda bos.
- `Assets/_Recovery/0.unity` + meta: untracked Unity kurtarma sahnesi (sahne ac/okuma riski, `Assets/Scenes/PhysicsTest.unity`, `PhysicsTest2.unity` de untracked).
- docs/ui-research/: 15 izlenen + izlenmeyen dosya: 5 HTML (bauhaus-moodboard 20,273 B; bauhaus-ui-v2 21,982; brutalist-ui 23,692; brutalist-ui-v2 24,807; mashup-ui 26,906; mockups 13,544 -> ~131 KB), 4 yonlendirme dosyasi direction-A/B/C/D (28,660 + 37,299 + 26,859 + 26,424 = ~119 KB), supervisor-review.md 36,596 B, skills-digest.md 10,973 B (hepsi izlenmeyen ya da arastirma ciktisi; toplam ~300 KB, ~90k+ token). Kucuk kararli dosyalar (palette.md, fonts.md, shape-method.md, hud-icons.md, main-menu.md, menu-juice.md, concrete-surface.md, crt-check.md) ise dogru yerde ve kisa.
- docs/moodboard/: 22 PNG (~1 MB her biri, ~23 MB; 23 dosya izlenen). docs/ui-research/crt-check/ 4 PNG (~2.1 MB), main-menu/ 3 PNG. Gorsel Read icin buyuk token maliyeti (goruntu basina ~1-2k token ama coklu okuma birikir).
- `docs/audit/2026-09-26-*.md` (45 KB), `docs/kalan-isler.md` (15.6 KB), `docs/sonraki-session.md`, `docs/ai-workflow-onerisi.md`, `docs/setup-prompt.md`, `docs/audit-fix-prompt.md` (izlenmeyen, 8.9 KB): eski/oturumdan-oturuma plan dosyalari; icerik "bozulmamali-davranislar.md" (3.2 KB, test dokumani) gibi canli dokumanlarla karisabilir.
- `docs/superpowers/plans/` 8 MP plani (~62 KB) 2026-11-06; multiplayer.md ile kapsam ortusmesi muhtemel (okunmadi, icerik dogrulanmadi).
- `docs/.obsidian/` (4 kucuk json) – Obsidian vault kalintisi, zararsiz.
- Plugin eski surum dizinleri `~/.claude/plugins/cache` (disk).

### Inferences
- Kok `dashboard.html` ve `docs/ui-research/*.html` arastirma/mockup artefaktlari: agent'in "docs'a bak" aramasi bunlari getirebilir; `docs/archive/` altina ya da Read-deny kapsamina almak guvenli. Dosyalarin icerigi okunmadi (yalniz boyut).

### Gaps
- docs/ui-research dosyalarinin hangisinin hala kullanildigi (UI motion/palette ile ilgili) icerik okunmadan belirlenemedi.

## 6. Stop hook sadelestirme (ozet) ve oncelikli bulgu listesi

### Takeaway
En buyuk kontrol edilebilir kalemler: (1) alakasiz skill/plugin tanimlari her oturumda (~4-7k token tahmini), (2) Read engeli olmayan dev YAML/atlas/HTML dosyalari (tek yanlis okumada yuzbinlerce token), (3) CLAUDE.md'nin mimari/planlama bolumleri (~1.5-2.5k token/oturum) ve memory-CLAUDE.md tekrarlari, (4) docs'ta eski arastirma/denetim dosyalarinin ayrilmamasi.

### Cited Findings
- Stop hook zaten degisiklik yokken `exit 0` (parmak izi). Optimizasyon firsati: parmak izi kapsamini `Assets/Scripts`, `Assets/Tests`, `*.asmdef` ile sinirlamak (plugin dosyalari haric) ve her turda 216 dosya sha1sum yerine mtime tabanli hizli on kontrol. Token etkisi ~0; sadece sure.
- SessionStart hook 485 karakter; gecikenler listesi icin ust sinir eklenmesi onerilir.
- Stop hook basarisizlik yolunda stderr 15 satir sinirli (iyi).

### Inferences
- Hook'larda token tasarrufu potansiyeli dusuk; kazanc dosya/skill/izin tarafinda.

### Gaps
- Hook calisma suresi ve `/context` dokumu alinmadi. Hicbir repo dosyasi degistirilmedi; yalniz bu not dosyasi yazildi.
