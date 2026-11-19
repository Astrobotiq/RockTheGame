# Unity-specific token sinks and fixes when using Claude Code (as of Nov 2026, Unity 6000.3, Windows 10)

Method note: ~14 web calls. Many sources are vendor blogs or tool authors' own READMEs; flagged below. No Reddit/HN threads were found with measured Unity+agent token data (search returned none), so community "how people handle it" is thin and mostly from blogs/tool READMEs.

## Q1. Which Unity files blow up context, and how to exclude them (deny rules, ignore files)

### Takeaway
Raw scene/prefab/asset YAML is the one clearly Unity-specific context sink; community advice is to block text edits to `.unity/.prefab/.asset/.meta` and route all scene work through Editor APIs/CLI. In Claude Code the working exclusion mechanism is `Read(...)` deny rules in settings.json (not `.claudeignore`, which is ignored).

### Cited Findings
- Claude Code: to block file tools from reading a path, add a `Read` deny rule such as `Read(./secrets/**)`; a `.claudeignore` file "has no effect" and its entries should be moved into `Read` deny rules. — [Claude Code permissions docs](https://code.claude.com/docs/en/permissions.md)
- Claude Code applies `Read(path)` deny rules on a "best-effort" basis to Grep and Glob as well; `Edit` rules cover all built-in editing tools; `Grep(path)`/`Glob(path)`/`Write(path)` rules with a path do NOT match anything (use `Read`/`Edit` rules instead). Directory listings omit names matched by a Read deny. — [Claude Code permissions docs](https://code.claude.com/docs/en/permissions.md)
- Bash is separate: Read/Edit deny rules govern file tools; shell commands (`cat`, `type`, `Get-Content`) are only partly covered (input redirects `< file` are checked against Read rules from v2.1.257; other cases need Bash deny rules, a sandbox, or a PreToolUse hook). Deny Bash rules don't match same program via other path or inside `sh -c`. — [Claude Code permissions docs](https://code.claude.com/docs/en/permissions.md) (reading of the page; exact Bash-cat coverage not verified)
- A Unity CLAUDE.md guide recommends blocking direct text edits to `*.unity`, `*.prefab`, `*.asset`, `*.meta` outright (deny, not prose), doing scene/prefab changes via C# scripts or Editor-backed MCP calls, and stopping instead of falling back to YAML. — [thepromptshelf Unity guide](https://thepromptshelf.dev/blog/claude-code-unity-guide-2026) (blog, not authoritative). Note: that guide concerns edit safety, not token cost; it has no token budget section.
- Same guide: keep `Library/ Temp/ Obj/ Build/ Builds/ .vs/ .idea/` out of VCS; mark `*.unity *.prefab *.asset` as `-diff` in `.gitattributes`; `LightingData.asset` and baked `NavMesh.asset` stay binary even with Force Text. — [thepromptshelf](https://thepromptshelf.dev/blog/claude-code-unity-guide-2026)
- Claude Code generic cost guides: whole file goes into context on Read; compounding via resent history; use `/cost`, `/compact`. — [morphllm summary via search](https://www.morphllm.com/claude-code-token-usage) (secondary; not fetched in full)
- Purpose-built "summarize Unity YAML" tools exist: `unity-ctx` (Go CLI; `summarize`, `context-pack --task --max-tokens`, dry-run-by-default writes). Its own benchmark is on tiny hand-written fixtures: scene 175 -> 22 tokens (-87%), prefab 186 -> 21 (-89%), tokens = bytes/4; the README only expects larger reductions on real multi-MB scenes (not demonstrated). — [pkg.go.dev unity-ctx](https://pkg.go.dev/github.com/Kubonsang/unity-ctx)
- A community "Unity Prefab Parser MCP" claims 70-90% reduction (example ~25,000 -> ~2,000 tokens for a prefab). Author claim, unverified. — [lobehub listing](https://lobehub.com/fr/mcp/luckynee-unity-prefab-parser-mcp)
- Force Text serialization makes scene YAML readable/diffable (also what makes it readable-and-huge for the agent). — [StraySpark what works](https://www.strayspark.studio/blog/claude-code-unity-what-works-2026) (vendor)

### Inferences
- Suggested deny set (my synthesis, untested here): `Read(./Library/**)`, `Read(./Temp/**)`, `Read(./Logs/**)`, `Read(./obj/**)`, `Read(./Builds/**)`, `Read(./UserSettings/**)`, `Read(./**/*.unity)`, `Read(./**/*.prefab)` (scene/prefab YAML is the biggest), optionally `Read(./**/*.meta)` and `Read(./**/*.sln)`/`*.csproj`, plus `Edit` denies for the same. Downside: a Read deny on `*.prefab`/`*.asset` also blocks legit inspection of ScriptableObject `.asset` data; prefer reading those through Editor eval instead of a blanket `*.asset` deny.
- TMP SDF font assets (`*SDF.asset`, e.g. this repo's `Tiny5-Regular SDF.asset`) embed large atlas/glyph tables in YAML and are the classic multi-MB `.asset`; no source found quantifying this, but it is a safe deny/avoid target. The repo's modified `Tiny5-Regular SDF.asset` also shows up in `git status`, so `git diff` without a path filter can dump it into context; use `git diff --stat` or pathspec excludes. (inference)
- `.meta` files are tiny and mostly harmless individually; the cost is noise in `git status`/`diff`/Glob listings. Glob/Grep with a type filter (`--glob '*.cs'`) avoids it. (inference)
- Generated `*.csproj/*.sln` are large and regenerated; they should be denied from reads and ignored by git (standard Unity .gitignore). (inference; no source on token impact)

### Gaps
- No measured data on token size of `.meta`, TMP SDF assets, csproj, or Library in agent sessions.
- Whether ripgrep-based Grep in Claude Code respects `.gitignore` by default for Library/Temp was not verified in this pass (Claude Code docs fetched did not state a `respectGitignore` setting; I did not find it). Check `/settings-reference` yourself.
- Exact Windows behaviour of Bash/PowerShell reads vs Read deny rules not verified.

## Q2. Editor integration approaches and their token cost (MCP vs CLI vs eval), and log verbosity

### Takeaway
MCP's cost is the always-present tool schema list (measured at roughly 4k to 30k tokens for Unity servers, by crude chars/4 estimates), while Unity's direct CLI sends no tool list at all; Unity itself now steers to CLI + skills and has deprecated its in-Editor MCP server. Per-call result verbosity (console dumps) is a separate sink that no source quantifies well.

### Cited Findings
- Unity published an official Claude Code plugin on 2026-09-09: 29 skills at launch, `/unity-cli` skill (live Editor control, C# execution, builds, logs). Unity says skill-assisted output is "generally more token efficient"; no measurements given. A Korean report dates the launch Aug 10 (date discrepancy). — [Unity blog](https://unity.com/blog/unity-plugin-for-claude-code); date conflict in [secondary search result](https://www.gamemeca.com/en/mv.php?gid=1780303)
- Unity docs (via StraySpark) say direct CLI commands "are faster and use fewer tokens than MCP"; no numbers published. The in-Editor MCP server in `com.unity.ai.assistant` is deprecated in favor of the CLI; `unity mcp` (CLI's MCP mode) remains supported; CLI is labelled experimental (1.0.0-beta.13, 2026-10-07). — [StraySpark MCP vs CLI](https://www.strayspark.studio/blog/unity-mcp-vs-unity-cli-2026) (vendor selling a competing MCP server)
- Measured tool-list sizes (chars/4 or other estimators, not comparable): Unity CLI `unity mcp` 171 tools ~27,700 tokens (110,931 bytes); CoplayDev MCP for Unity 49 tools 29,720 tokens (Qwen3-4B tokenizer, issue #1482, 2026-10-19); StraySpark default front door 20 tools 4,372; Vangardo facade 48 -> 6 tools 23,280 -> 915 (estimate). — [StraySpark token-usage post](https://www.strayspark.studio/blog/unity-mcp-token-usage-tool-list)
- Unity's older AI Assistant MCP docs list no tool-filtering/enable-disable setting (only connection approval), and name tools like `Unity_ManageScene`, `Unity_ManageGameObject`, `Unity_ReadConsole`. — [Unity docs](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.6/manual/integration/unity-mcp-get-started.html)
- Per-call output can dwarf schema: a CoplayDev issue (#302) reportedly shows ~13.3k tokens for 20-30 log lines (cited secondhand). "A paged search or a filtered console read costs less than a dump." — [StraySpark token-usage post](https://www.strayspark.studio/blog/unity-mcp-token-usage-tool-list)
- Claude Code mitigation of MCP schema cost: tool definitions deferred by default (Tool Search; names only until needed); `/context` shows MCP share. Claims such as "GitHub MCP ~42k tokens" and "Playwright ~13.6k" come from blogs. — [Gurubase](https://gurubase.io/blog/2026/hidden-token-cost-mcp-servers/), [Unblocked](https://getunblocked.com/blog/mcp-token-budget-autopsy/)
- Third-party self-benchmark: `unityctl` claims 6.8x cheaper than CoplayDev MCP in a short session; author-published, unverified. — surfaced in search; [unityctl on NuGet](https://www.nuget.org/packages/unityctl/0.6.2)
- Domain reloads can drop MCP connections, a common reason people abandon MCP tools; some devs skip live connections for editor scripts / batch mode. — [StraySpark what works](https://www.strayspark.studio/blog/claude-code-unity-what-works-2026) (vendor)
- Unity CLI trimming flags: not documented in the pages fetched (only `--tool/--args/--idempotency_key` of StraySpark). Not found.

### Inferences
- With Claude Code's deferred tool loading, the practical schema tax of an MCP server is lower than the raw numbers imply, but each loaded schema still persists in the context. The CLI/eval route (this repo's `unity command eval`) avoids it entirely, matching Unity's recommendation. Cost then moves to command output, so the lever is trimming output (filtering to `error CS`, test summaries), not schema.
- Eval-style C# execution can return arbitrarily large strings (e.g. dumping a hierarchy); returning compact, purpose-built strings (counts, names, specific fields) is the equivalent of "filtered console read".

### Gaps
- No independent, reproducible benchmark of CLI vs MCP tokens per task; all numbers are vendor/author.
- No measured numbers for IvanMurzak Unity-MCP tool count/tokens (directories say 70+ tools; one listing shows 0; marketing "efficient token usage" unverified).
- No data on `unity command console` output size options.

## Q3. Compile/test loop efficiency

### Takeaway
No Unity-specific measured guidance was found; sources agree the loop needs compile result + test results delivered as text, and that filtered, failure-only reads beat dumps. The concrete tactics below are mostly inference from general output-compression practice.

### Cited Findings
- Closed-loop framing: change, read value back, get compile result after domain reload, run tests, play game; the missing feedback is why devs copy console errors by hand. — [StraySpark what works](https://www.strayspark.studio/blog/claude-code-unity-what-works-2026) (vendor)
- Test strategy: EditMode for pure C# logic, PlayMode only where MonoBehaviour lifecycle matters (speed); CLI form `Unity -runTests -batchmode -projectPath . -testResults results.xml`. Compile check alone won't catch broken YAML references. — [thepromptshelf](https://thepromptshelf.dev/blog/claude-code-unity-guide-2026)
- General: run tools that show only failures; RTK-style filters show failing tests only (see Q6). — [RTK repo](https://github.com/rtk-ai/rtk)
- Claude Code permissions docs describe PreToolUse/other hooks as the enforcement layer (usable to rewrite/deny commands). — [permissions docs](https://code.claude.com/docs/en/permissions.md)

### Inferences
- Tactics (untested): (1) grep Editor.log for `error CS` with a line cap and `tail` instead of reading the whole log, ideally only lines after the last "Compilation started" marker; (2) never `Read` TestResults.xml whole; extract `<test-case result="Failed">` names + `<message>` via a small script (PowerShell `Select-Xml`/Python) and print counts when green; (3) use `run_tests --filter` for the touched test only, full suite once at the end; (4) a Stop/PostToolUse hook that prints a one-line summary (`compile OK, 142 passed`) and dumps details only on failure; (5) keep the project's existing Stop-hook design but make its success output minimal. The user's CLAUDE.md already does several of these (filtered tests, hook).
- Editor.log accumulates across the session; re-reading it fully each loop is a classic waste, so use byte-offset/last-N reads.

### Gaps
- No source measured actual savings for these tactics in Unity.
- No Unity-specific hook templates found for summarizing test XML.

## Q4. Code organization that makes agent work cheaper

### Takeaway
Community and tool guidance converge on: pin versions and conventions in a lean CLAUDE.md, define asmdef boundaries, keep scene/prefab wiring out of agent-readable YAML via Editor scripts, and keep tunables in ScriptableObjects. Token-economy claims for these are reasoning, not measurement.

### Cited Findings
- CLAUDE.md template sections recommended for Unity: pinned Unity version (ProjectSettings/ProjectVersion.txt), render pipeline, input system, UI approach, scripting backend, async rules; hard rules on file types; project structure including Runtime/Editor/ScriptableObject and `.asmdef` boundaries; MonoBehaviour conventions (cache in Awake, `[SerializeField] private`, Unity null checks, unsubscribe in OnDestroy); test commands; VCS settings. — [thepromptshelf](https://thepromptshelf.dev/blog/claude-code-unity-guide-2026)
- General CLAUDE.md advice: list only what the agent can't infer from code; avoid vague guidance. — [CLAUDE.md best-practice roundup](https://claudhq.com/claude-md-best-practices-guide/) (secondary; surfaced via search)
- One dev's CLAUDE.md for Unity: use MCP to create/modify GameObjects/prefabs, never edit YAML directly; write a temporary editor script only for large batches. — [CoplayDev unity-mcp CLAUDE.md mirror](https://gh.mise.run.place/CoplayDev/unity-mcp/blob/beta/CLAUDE.md)
- Editor scripts are reviewable and version-controlled, arguably better than live edits. — [StraySpark what works](https://www.strayspark.studio/blog/claude-code-unity-what-works-2026)

### Inferences
- asmdefs reduce recompile time (not tokens directly) which shortens the loop and reduces how much compile output appears per change; small focused files cut Read cost per edit. The repo's `docs/systems/` per-mechanic docs plus a short CLAUDE.md fit this pattern; keep CLAUDE.md short because it is loaded every turn (re-sent each request; cache hits help but size still counts toward the window).
- ScriptableObject-based data means tuning changes touch small `.asset` files instead of scene YAML; reading them via Editor eval keeps raw YAML out of context.
- Naming conventions and namespaces (like the repo's `New_Scripts.*`) make Grep results targeted, shrinking search output. (inference)

### Gaps
- No quantitative study linking asmdef/file-size practices to agent token use in Unity.

## Q5. Existing community guides, templates, plugins, and what they say about token economy

### Takeaway
There is an official Unity plugin for Claude Code (skills + CLI), plus several community CLAUDE.md templates and Unity MCP servers; token economy is addressed mostly through "CLI over MCP", "smaller default tool list", and "load tools/skills on demand", with claims that are unmeasured or vendor-sourced.

### Cited Findings
- Official: Unity plugin for Claude Code (2026-09-09, 29 skills incl. `/unity-cli`, `/unity-package-management`, `/new-unity-project`, `/ui-uitk`, `/validate-urp-render-graph-renderer-feature`); Codex counterpart has 31 skills. This session's skill list shows `unity:*` skills, i.e. the plugin appears already installed here. — [Unity blog](https://unity.com/blog/unity-plugin-for-claude-code), [StraySpark MCP vs CLI](https://www.strayspark.studio/blog/unity-mcp-vs-unity-cli-2026)
- Skills load on demand, MCP schemas are always in context; one vendor claims ~8 tokens per skill in idle context (unverified). — search result summary on [unityctl/UniCli sources](https://claudepluginhub.com/marketplaces/yucchiy-unicli)
- Community plugins: niqibiao unity-cli-plugin (CLI via skills rather than MCP), UniCli, unity-agentic-tools, "Unity AI Bridge". — [awesome.ecosyste.ms entry](https://awesome.ecosyste.ms/projects/github.com%2Fniqibiao%2Funity-cli-plugin), [skillselion](https://skillselion.com/plugin/taconotsandwich/unity-agentic-tools)
- MCP servers: CoplayDev MCP for Unity (47-49 tools; Python 3.10+ via uv; Unity 2021.3 to 6.x), IvanMurzak Unity-MCP, CoderGamester mcp-unity, Unity AI Assistant relay (deprecated). — [thepromptshelf](https://thepromptshelf.dev/blog/claude-code-unity-guide-2026), [Unity docs](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.6/manual/integration/unity-mcp-get-started.html)
- StraySpark's recommendations: commit before each agent session, Force Text, install Unity's plugin, give agent compile/test access via CLI/MCP, define "done" with a concrete check, keep feel decisions with the developer. — [StraySpark](https://www.strayspark.studio/blog/claude-code-unity-what-works-2026)
- One Unity Discussions comment (secondhand) argued MCP is unnecessary if you only use Claude Code to write code. — surfaced in search; original not verified

### Inferences
- The user's setup (Unity Pipeline package + `unity command ...` + eval + hooks + deny rules in settings.json) already matches the direction Unity's own docs recommend (CLI over MCP), so the biggest remaining wins are output trimming and Read denies, not switching tooling.

### Gaps
- No official Unity doc found that gives token-economy guidance beyond the "fewer tokens" claim.
- No high-quality Reddit/HN evidence found.
- Plugin install command not shown on the Unity blog page fetched; docs at docs.unity.com/en-us/ai/unity-plugin/claude-code were not fetched.

## Q6. Tools that compress shell output (RTK etc.): Windows/PowerShell, measured savings, risks

### Takeaway
RTK (rtk-ai/rtk) rewrites shell commands to compact summaries via a Claude Code hook; headline savings (60-90% of command output) are vendor estimates, and the few independent end-to-end measurements show roughly zero net cost change. On Windows the native Bash-tool hook works, but it does not fire for Claude Code's PowerShell tool, which matters for this repo.

### Cited Findings
- RTK is a single Rust binary, "CLI proxy that reduces LLM token consumption by 60-90% on common dev commands", 100+ commands, <25 ms overhead; `rtk init -g` registers a hook rewriting `git status` to `rtk git status`; built-in Read/Grep/Glob unchanged; `rtk bypass` runs unfiltered; `rtk recall` recovers cut output; tokens estimated as bytes/4. The README itself warns "a 90% cut in command output is not a 90% cut in cost". — [rtk-ai/rtk](https://github.com/rtk-ai/rtk)
- Independent/semi-independent measurements (per Wavect review of RTK 1.0.x, documentation/source review not a benchmark): JetBrains (RTK 0.43.0, 86 tasks) +7.6% median cost at low effort (p=0.004), +0.1% at high effort; RTK team follow-up (0.45.0, 12 tasks) -4.8% (p=0.305, not significant); Quesma (0.45.0, 1,740 attempts) -3% for one model, +7% for another. No verified end-to-end savings for 1.0.1. — [Wavect review](https://wavect.io/blog/rtk-ai-token-savings-review/)
- Windows: README mentions winget (`winget install rtk-ai.rtk`) and native Windows; `rtk init -g` registers a native `rtk hook claude` that works on native Windows for Bash tool calls. Open issue: hook matcher hardcoded to Bash, so with `CLAUDE_CODE_USE_POWERSELL_TOOL=1` commands are NOT rewritten (issues #2115, #1319 PowerShell hook proposal; no evidence of resolution). Older DEV guide said Windows had no hook mode (likely outdated). Separate Claude Code bugs report hooks always run via bash wrapper on Windows / PowerShell-tool hooks silently not running. — [rtk issue #1319](https://www.goodfirstissue.org/rtk-ai/rtk/issues/1319), [issue #2115](https://goodfirstissue.org/rtk-ai/rtk/issues/2115), [Claude Code issue 32930](https://claudeissues.com/issue/32930-bug-hooks-always-executed-via-usr-bin-bash-on-windows-ignoring-shell-setting), [issue 90077](https://claudeissues.com/issue/90077-bug-hooks-with-shell-powershell-spawn-pwsh-with-no-powershell-exe-fallback-so-ho)
- Risks: filters can hide detail; failing tests must stay visibly failed and raw CI remain the acceptance source; licensing split (rtk-oss Apache-2.0, full rtk mixes Apache-2.0 and BUSL-1.1, rtk-proxy BUSL-1.1); proxy forwards provider credentials, keep on loopback; opt-in CCR stores originals 7 days; telemetry off by default; unrelated crate named "rtk" exists. — [Wavect](https://wavect.io/blog/rtk-ai-token-savings-review/), [rtk-ai/rtk](https://github.com/rtk-ai/rtk)
- Best community pipeline cut total cost about 20.2% in its author's benchmark; demo session 93.3% output savings (vendor figures). — [rtk-ai/rtk](https://github.com/rtk-ai/rtk)

### Inferences
- RTK has no Unity-specific filters in the sources found; Unity output (`unity command ...`, Editor.log, NUnit XML) would likely pass through unfiltered or need custom filters, so custom hook/script summarizers for these three outputs likely beat a generic proxy here. (inference; not tested)
- This repo's environment mixes Bash and PowerShell tools, and Unity commands are the high-volume output; the PowerShell-tool gap makes RTK's benefit uncertain on Windows here. Test with `rtk gain` on a real session before adopting.

### Gaps
- No Windows-specific RTK benchmark found.
- No source tested RTK (or similar, e.g. other output compressors) on Unity/NUnit/Editor.log output.
- Did not verify the current state of issues #1319/#2115 directly (only via search snippets).
