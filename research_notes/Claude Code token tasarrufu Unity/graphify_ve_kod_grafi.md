# Code-graph / indexing tools for Claude Code token savings (Graphify and alternatives), as of Nov 2026

Note: ~13 tool calls, mostly via search summaries and single-page fetches. Marketing vs independent is flagged per line. Nothing found is Unity-specific and measured.

## 1. What is Graphify, and do its savings claims hold up?

### Takeaway
Graphify (PyPI `graphifyy`, CLI `graphify`, org Graphify-Labs, ~146k stars) builds a tree-sitter code graph (plus an LLM pass for docs/images) and nudges Claude Code toward it via a hook. The famous 71.5x comes from a 52-file mixed corpus (code+papers+images); independent tests on real code show 7.3x per-query on one Python repo, and about 0-8% net session savings in another, with high build cost. Savings are real only for orientation/architecture queries on larger codebases.

### Cited Findings
- Code is parsed locally with tree-sitter ("deterministic, no LLM"); docs/PDFs/images go through an LLM semantic pass; edges tagged EXTRACTED vs INFERRED; outputs graph.json, GRAPH_REPORT.md, graph.html; Leiden clustering. Python 3.10+, `uv tool install graphifyy`, `graphify install`, then `/graphify .` — [Graphify README](https://github.com/safishamsi/graphify) (page fetched redirects/refers to Graphify-Labs/graphify; fetch summary named the org as Graphify-Labs and sponsor links to safishamsi; exact individual author not confirmed by me)
- C# (`.cs`) and YAML (`.yaml/.yml`) are supported; `.unity`/`.prefab` are not mentioned in the supported-inputs page ("and more" so not exhaustive) — [Graphify supported inputs](https://docs.graphify.com/concepts/supported-inputs)
- Claude Code integration: `graphify install --project --strict` / `graphify claude install` adds a hook before search-style tool calls and file reads. Strict mode: Read and Grep share one denial per session unless a graph query happened in the last ~30 min; Bash and Glob are advisory only — [Graphify README](https://github.com/safishamsi/graphify)
- The "80% token spend" claim belongs to the paid Graphify Cloud product with no methodology given; open-source benchmarks in BENCHMARKS.md: ERPNext cross-tool key-fact coverage 82.0% vs 70.8% for grep/read (n=6 questions); LOCOMO QA accuracy 45.3% (below supermemory 49.7%); LongMemEval-S 76% tied with dense RAG (n=50). The page is partly promotional — [Graphify README](https://github.com/safishamsi/graphify)
- Headline 71.5x/"~1.7k vs ~123k tokens per query" is from a 52-file corpus of code, papers and images (marketing/self-reported) — [Honest Benchmark](https://exchangepedia.com/articles/graphify-honest-benchmark-real-codebase.html), [MindStudio summary](https://www.mindstudio.ai/blog/graphify-claude-code-knowledge-graph-large-codebase-70x)
- Independent benchmark (author: blog, Python/FastAPI repo, 140 files, ~154k raw tokens): 7.3x average per-query reduction (range 6.5x-8.1x) on 5 architecture questions; initial build cost ~240k tokens, ~40k for a 3-file `--update`; built-in tracker reported zero (misses subagent usage); 52% of edges INFERRED at flat 0.5 confidence; 96 of 118 communities isolated; `--update` fragile on Windows (encoding, bash quoting, graph merge corruption); for specific lookups grep was often faster; in focused implementation sessions the graph was not used at all; author says under ~200 files overhead competes with benefit (rough judgment, not measured) — [Exchangepedia](https://exchangepedia.com/articles/graphify-honest-benchmark-real-codebase.html)
- Search-result summary of other independent write-ups: a 10-question Claude Code session on browser-use measured ~113k tokens with Graphify vs ~120k without (~7%); a GitHub catalog issue says no advertised figure is a task-success rate, edges are static (no dynamic dispatch/config wiring), and three user reports showed equal or higher cost. I did NOT open these pages myself (secondary summary) — [Graphify Review (wavect/codeconductor/BenchKeeper issue results)](https://github.com/ESchuderer/BenchKeeper/issues/1000), [Inside Graphify](https://pabloformoso.com/en/graphify-code-graph-no-embeddings/)
- Wavect review: "worth a controlled pilot, not a sure ROI"; fits large monorepos/onboarding, skip for small familiar services; costs include Python deps, semantic-pass provider usage, rebuilds, pinning (fast-moving releases), repo-governance if graphify-out is committed — [Wavect](https://wavect.io/blog/graphify-review-codebase-knowledge-graph/)
- Repo has ~739 open issues / 895 PRs — [Graphify README](https://github.com/safishamsi/graphify)

### Inferences
- A ~hundreds-of-.cs-files Unity project is at the threshold where the independent author said the tool is marginal. C# class graph (inheritance, calls) would be extracted; scene/prefab YAML wiring (the thing Unity devs often need: which GameObject references which script via GUID) is likely invisible to the graph because tree-sitter on `.unity` YAML gives no GUID-to-script semantics.
- The per-query ratio compares against reading raw files, not against what Claude Code actually does (grep then targeted reads), so ratios overstate real session savings.

### Gaps
- No Graphify measurement on a C#/Unity project found. Exact author identity and whether `.unity/.prefab` are indexed: unverified. wavect/codeconductor/pabloformoso pages not read in full.

## 2. Alternatives: how they work, evidence, overhead, C#/Unity support

### Takeaway
Tree-sitter graph tools (code-review-graph, codebase-memory-mcp) report big ratios vs whole-corpus reading but modest ones (~4-7x) against a realistic grep baseline, and the only third-party session-level test found shows code-review-graph at -5% tokens. Serena (LSP) has no published token benchmark and has documented Unity/.NET setup friction. Claude Context needs Zilliz/Milvus + an embedding API.

### Cited Findings
- **code-review-graph** (tirth8205): tree-sitter -> SQLite graph, blast-radius query, C# supported, 30 MCP tools by default (restrictable via `--tools`/`CRG_TOOLS`), auto-update via editor/git hooks and watch mode with SHA-256 diff. Self-reported: median ~63x vs whole-corpus tokens across 6 repos (FastAPI, flask, gin, httpx, express...); vs a grep baseline the median is only 6.65x (18 questions, range 4.2x-72.6x); impact-analysis F1 ~0.69 — [repo](https://github.com/tirth8205/code-review-graph)
- Third-party headless test (Claude Code 2.1.116, Sonnet 4.5, ky repo 52 TS files, one task "propose 3 improvements"): baseline 284,473 tokens/$0.2666; code-review-graph 269,252 tokens (-5%, $0.2538, 17 turns). Article says its 49x figure came from a separate Next.js monorepo run and own Express data showed 0.7x (I could not confirm the 0.7x on the repo page). This test did not include Graphify, Serena, repomix; Claude Context install-verified only, not benchmarked. Single task, single run each — [computingforgeeks](https://computingforgeeks.com/reduce-claude-code-token-usage-tools/)
- **Serena** (oraios, ~31k stars): MCP server with symbol-level retrieval/edit via LSP (free) or paid JetBrains plugin; C# listed among 40+ languages; says symbolic editing is "much more token-efficient" but gives no numbers; in harnesses like Claude Code basic file tools are disabled by default; GPL-3.0 (SolidLSP MIT); LSP backend lacks move/inline refactors, no Rider support — [Serena README](https://github.com/oraios/serena)
- Serena has no token benchmark harness in-repo (analysis of its source); one Japanese user saw ~1.7x higher consumption, later corrected as stack difference not Serena. Anecdotal — [pantheon analysis](https://pantheon-org.github.io/agentic-context/benchmarks/oraios-serena/), [search summary of Zenn post](https://zenn.dev/iraken/articles/8f9aa646520a78)
- Serena + Unity friction: GitHub issue reports the C# language server init failing for Unity projects (tries .NET 10 runtime; workaround sets dotnet_version 6.0 and manual OmniSharp with `unity = true`+solution). Status of the fix unknown to me. Unity solutions with many csproj are slow to load in OmniSharp-class servers — [Serena issue #1014](https://github.com/oraios/serena/issues/1014)
- Serena-slim wrapper estimates Serena tool schemas ~23.9k -> ~11.9k tokens (est., schema size not task savings) — [search summary](https://glama.ai/mcp/servers/mcpslim/serena-slim)
- **Claude Context** (Zilliz): semantic vector search MCP; C# supported; needs Milvus/Zilliz Cloud + embedding provider (OpenAI/Voyage/Ollama/Gemini), Node 20+; Merkle-tree incremental re-index; claims "~40% token reduction at equivalent retrieval quality", methodology not on README — [repo](https://github.com/zilliztech/claude-context)
- turbopuffer's Kuba Rogut benchmarked semantic search on Claude Code; gain not attributable to semantic search alone (restricting reads also contributes) — [AI Engineer talk](https://ai.engineer/talks/zKk7sDMGDEQ-benchmarking-semantic-code-retrieval-on-claude) (via search snippet; not read in detail)
- **codebase-memory-mcp** (DeusData): tree-sitter graph in SQLite, arXiv 2603.27277; headline 99.2% = ~3.4k tokens vs ~412k for 5 structural queries on one repo; no controlled multi-repo study; C# support status inconsistent across listings; a review notes a quality tradeoff — [arXiv](https://arxiv.org/abs/2603.27277), [analysis](https://pantheon-org.github.io/agentic-context/analysis/deusdata-codebase-memory-mcp/)
- **TokenSaver.Mcp** (.NET/Roslyn): claims 50-95%, vendor claim only; a Unity prefab parser MCP claims 70-90% on scene/prefab files, third-party, unverified — [NuGet](https://nuget.org/packages/tokensaver.mcp), [Unity Prefab Parser MCP](https://lobehub.com/ja/mcp/luckynee-unity-prefab-parser-mcp)
- **repomix / aider repo-map / Sourcegraph-Cody**: not researched in depth (no sources opened). Repomix packs the whole repo into one prompt (the opposite of saving tokens in an agentic loop); aider repo-map is a tree-sitter+PageRank map built into aider, not for Claude Code — this is general knowledge, not sourced here.

### Inferences
- Ratios of 50-300x all use "read the whole corpus" as baseline, which Claude Code never does. Realistic grep baselines give ~4-7x on a per-query basis and near zero net on whole tasks.
- For Unity, Roslyn-class LSP is the most semantically accurate C# option but has the worst setup/startup friction (Unity csproj generation, Mono/netstandard2.1, .NET SDK version).

### Gaps
- No independent benchmark for Serena, Claude Context, repomix, or Sourcegraph on any agent task. No Unity-project tests for any tool. Aider repo-map/Cody not covered.

## 3. Does Claude Code's own Grep/Glob/Explore approach already cover most of this value?

### Takeaway
Anthropic deliberately moved from RAG/indexing to agentic search; the team concedes it costs more tokens/latency but avoids stale indexes. Official docs treat indexing/LSP as relevant for "millions of lines"/monorepos and recommend deny rules, nested CLAUDE.md, subagent exploration and LSP plugins first.

### Cited Findings
- Boris Cherny (Claude Code lead), as quoted by OfficeChai (May 2025): "Eventually, we landed on just agentic search as the way to do things"; "It outperformed everything else by a lot"; evidence was "internal vibes... some internal benchmarks also, but mostly vibes"; RAG index "drifts out of sync"; trade-off "at the cost of latency and tokens" — [OfficeChai](https://officechai.com/ai/claude-researcher-explains-how-agentic-search-performed-better-than-rag-for-code-generation/) (secondary report of a video, I did not view the original)
- Official docs: Claude Code works at any size, but for large codebases the defaults can fill context; recommended steps are per-directory CLAUDE.md, `permissions.deny` Read rules for generated/vendored code, code-intelligence (LSP) plugins to "find a symbol's definition or callers... instead of scanning files", subagent exploration, and exposing an existing code-search/RAG index as an MCP tool if the org already has one. Content searches respect .gitignore — [Anthropic large-codebases docs](https://code.claude.com/docs/en/large-codebases)
- Official marketplace LSP plugins documented for TypeScript, Python, Go, Rust "and other common languages"; whether a C# plugin exists in the marketplace I could not confirm from the page — [same docs](https://code.claude.com/docs/en/large-codebases)
- MCP overhead: Claude Code's Tool Search defers MCP tool definitions and loads them on demand; reported threshold ~10% of context (attributed quote, unconfirmed against official docs); blog estimates of 85-95% schema reduction, third-party. Therefore per-turn schema cost of a 30-tool MCP server (code-review-graph) is much smaller than before, but the always-present server instructions/tool names remain — [Tool Search deep dive](https://oldeucryptoboi.substack.com/p/tool-search-deep-dive), [Agent SDK tool-search docs](https://code.claude.com/docs/en/agent-sdk/tool-search)
- Unity-specific hygiene: deny `Read` on `Library/`, `Temp/`, `Logs/`, `obj/`, big vendored packages; .unity/.prefab/.asset are YAML with GUID references; third-party guides advise treating them as non-text-editable — [Unity CLAUDE.md guide](https://thepromptshelf.dev/blog/claude-code-unity-guide-2026), [Anthropic docs](https://code.claude.com/docs/en/large-codebases)

### Inferences
- For ~hundreds of .cs files the agentic grep + Explore subagent already fits in budget; the main token sinks in Unity are likely accidental reads of huge scene/asset YAML and `.meta`, which graph tools do not fix but deny rules and CLAUDE.md guidance do (this project already has `*.unity` Edit/Write denied; Read denies on big YAML and `Library/` were not checked).
- Graph hooks that deny first Read/Grep (Graphify strict) add friction and can break the existing Stop-hook/Unity CLI workflow; non-strict is advisory only and Claude often ignores it (independent author did not use the graph in implementation).

### Gaps
- Original Cherny source not viewed; no Anthropic-published token numbers for agentic search vs indexing. Current Tool Search default threshold unconfirmed.

## 4. Who benefits most/least, recommendation, how to measure

### Takeaway
Benefit scales with repo size and with orientation/cross-cutting questions (large monorepos, onboarding); a single mid-size Unity project with strong CLAUDE.md and docs is the least favorable case. Recommendation: skip Graphify as a default; at most run a time-boxed, measured trial of one low-friction tool (code-review-graph or Graphify code-only, non-strict hook), after cheaper fixes.

### Cited Findings
- Sources agree the sweet spot is large monorepos/multi-repo/onboarding and that small, familiar codebases with good docs gain little; recommend piloting with questions from your own backlog — [Wavect](https://wavect.io/blog/graphify-review-codebase-knowledge-graph/), [Exchangepedia](https://exchangepedia.com/articles/graphify-honest-benchmark-real-codebase.html)
- Measurement method used by an independent benchmark: headless `claude -p` JSON output, total tokens = input + cache creation + cache read + output (as `/cost`), same task, same model, baseline vs tool — [computingforgeeks](https://computingforgeeks.com/reduce-claude-code-token-usage-tools/)

### Inferences (my recommendation, not sourced fact)
- Verdicts: Graphify = skip by default / optional trial (marketing numbers inflated, build cost 240k tokens, Windows `--update` bugs, no .unity/.prefab understanding). code-review-graph = trial candidate only (C# supported, auto-update hooks, but only -5% in independent test and mainly aimed at review/blast radius). Serena = skip unless symbol-level refactors needed; Unity .NET setup friction, no token evidence. Claude Context = skip (external vector DB + paid embeddings, ~40% self-claim). codebase-memory-mcp = watch, unverified on C#. repomix = wrong tool for agents. Official LSP plugin (if a C# one works with Unity) = the most "native" option to try first.
- Do first (free): deny Read for `Library/**`, `Temp/**`, `obj/**`, large `.unity`/`.asset`/`*.meta` where not needed; keep CLAUDE.md lean; use Explore subagents; check `/context` and `/usage` to see where tokens actually go before adding tools.
- A/B protocol: pick 8-10 real tasks (e.g. "where is X wired", "what breaks if I change PlayerController's dash", "add a field to PlayerStatsSO"), run each with `claude -p --output-format json` in clean sessions, same model, 2-3 repeats, baseline vs tool; record total tokens, cost, turns, wall time, and correctness (does it find the right files, passes the EditMode tests). Include build/update cost amortized. Adopt only if net tokens drop >=25% with equal correctness; otherwise remove.

### Gaps
- No evidence on Unity scene-reference queries. Run-to-run variance of agentic sessions is high; single-run benchmarks (including the cited one) are weak evidence.
