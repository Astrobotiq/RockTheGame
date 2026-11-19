# Workflow-level patterns for token-efficient agentic coding (Claude Code), Nov 2026

## Plan-then-execute, spec-driven, small verifiable steps

### Takeaway
Anthropic's docs recommend plan mode, a written spec, and incremental testing, and claim they prevent expensive rework. I found no controlled measurement of the token or rework effect, so this is vendor guidance, not measured evidence.

### Cited Findings
- Cost docs: "Use plan mode for complex tasks... preventing expensive re-work when the initial direction is wrong." Also "Test incrementally: Write one file, test it, then continue." (Vendor claim, no numbers.) — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Best-practices doc: after a `SPEC.md` exists, start a fresh session to build from it, so the build begins with clean context and a written definition of done. — [Best practices](https://code.claude.com/docs/en/best-practices)
- Best-practices doc: the docs say defaults (auto-compaction, delegation to subagents) already handle most cases. Extra setup pays off only for things Claude can't see (deploy process, must-pass checks, budgets). — [Best practices](https://code.claude.com/docs/en/best-practices)
- Course-correction: "Press Esc the moment Claude drifts, and after two failed corrections run /clear and write a better prompt instead of a third." A third correction adds to a context that already holds two failed attempts. — [Best practices](https://code.claude.com/docs/en/best-practices)
- `/rewind` (or Esc twice) restores code and/or conversation to an earlier checkpoint, which makes trying the bolder approach first low-risk. — [Best practices](https://code.claude.com/docs/en/best-practices)
- Default effort is `medium` for Opus 5.5 / Sonnet 5.5 / Haiku 5.5 / Fable 5.2. Raise to `high`/`xhigh` for bugs in unfamiliar code or flaky tests; extra time and tokens go to edge-case checking. — [Best practices](https://code.claude.com/docs/en/best-practices)
- The costs doc says you can't disable thinking on Opus 5.5, Sonnet 5.5, Haiku 5.5 or Fable models; effort level is the cost lever instead. — [Claude Code costs](https://code.claude.com/docs/en/costs)

### Inferences
- For a solo dev, the cheap form of planning is a 5-15 line plan or SPEC file for tasks touching more than 2-3 files, then a fresh session to execute. Single-file tweaks (e.g. one PlayerStatsSO value) don't need it.
- The "two failed corrections, then /clear and rewrite the prompt" rule is the most concrete anti-waste heuristic here.

### Gaps
- No published measurement (A/B) of plan mode or spec-driven development on total tokens or rework rate. Only vendor assertions.

## Verification loops, TDD, hooks that return only failures

### Takeaway
Docs advise giving the agent a self-check (tests, expected output) and pre-filtering output with hooks to cut tokens from tens of thousands to hundreds. Quantified cost-vs-wasted-iteration trade-offs are not published.

### Cited Findings
- "Give verification targets: include test cases, paste screenshots, or define expected output... When Claude can verify its own work, it catches issues before you need to request fixes." — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Hooks can preprocess data: instead of reading a 10,000-line log, a hook greps for ERROR, "reducing context from tens of thousands of tokens to hundreds." Example PreToolUse hook keeps only the last 100 lines of test output. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Best-practices doc: the most useful line in CLAUDE.md is often the command that runs the project's check. `/verify` has Claude build and run the app and look at the result, catching what tests and typechecks miss. — [Best practices](https://code.claude.com/docs/en/best-practices)
- Rules with zero exceptions belong in hooks/deny rules, not CLAUDE.md. "CLAUDE.md is for guidelines, and Claude is allowed to decide a guideline doesn't apply." Don't overdo hooks. — [Best practices](https://code.claude.com/docs/en/best-practices)
- Code-intelligence (LSP) plugins for typed languages give go-to-definition instead of grep + reading candidate files, and report type errors after edits without running a compiler. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Always-on cost: a Stop hook that blocks on failure adds a turn per failure but, with output filtered to error lines, keeps each loop small. (Design consideration from the above, not a measured figure.)

### Inferences
- Unity-specific: compile errors live in Editor.log, not the console. A hook that greps `error CS` and returns only those lines, plus a test runner that returns only failing test names, matches the documented pattern. Never pipe the full Editor.log or TestResults.xml into context.
- A verify step is worth its cost when failures are cheap to describe (compile error, failing assert). It is poor value for "game feel", which the agent cannot judge.

### Gaps
- No published numbers on tokens saved or iterations avoided by TDD/hooks with agents. No Unity-specific evidence found.
- The superpowers TDD skill and similar are practitioner claims; I did not find measured outcomes.

## Session structure, context rot, handoff notes, worktrees

### Takeaway
Long context measurably degrades model accuracy (several independent sources), and in Claude Code every request re-sends the whole history, so long sessions cost more per turn. `/clear` between tasks, notes in files, and worktrees for parallel work are the recommended structure.

### Cited Findings
- Anthropic: context rot: recall decreases as tokens grow, across all models, a "performance gradient rather than a hard cliff." Attention budget framing: n^2 pairwise relationships, training skewed to shorter sequences. — [Effective context engineering](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents)
- Anthropic: compaction keeps decisions, unresolved bugs, implementation details, and the five most recent files; clearing old tool results is the lightest compaction. Structured note-taking (NOTES.md, to-do lists) lets agents resume after resets. Sub-agents work in clean windows and return ~1,000-2,000-token summaries. — [Effective context engineering](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents)
- Chroma (July 2025, vendor study, 18 LLMs incl. Claude 4, GPT-4.1, Gemini 2.5): performance degraded with input length in every experiment; one distractor hurt, four hurt more; on LongMemEval, ~113k-token prompts scored significantly lower than ~300-token focused prompts for all models, Claude's gap largest mainly from abstentions. Needle position showed no notable effect. — [Chroma Context Rot](https://www.trychroma.com/research/context-rot)
- Liu et al. 2023 "Lost in the Middle": accuracy highest when relevant info is at the start or end of context and degrades in the middle; also drops as context grows, even for long-context models (multi-doc QA, key-value retrieval). Older models; Chroma found no position effect on its tasks, so applicability to current models is mixed. — [arXiv 2307.03172](https://arxiv.org/abs/2307.03172v3)
- Claude Code cost mechanics: full conversation is sent with every request; cached history is re-read at the cached rate, so a one-line question in an all-day session still draws usage for the whole conversation. First message after a break longer than the cache lifetime misses the cache and reprocesses everything. `/compact` of a large context is itself a large request; `/clear` costs nothing. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Docs: `/clear` between unrelated tasks; "a focused context gets you better answers and costs less" even though most current models have a 1M window and auto-compaction. Unexpected high spend "usually traces back to long sessions that were never cleared or to Opus left as the default model." — [Best practices](https://code.claude.com/docs/en/best-practices); [Claude Code costs](https://code.claude.com/docs/en/costs)
- Worktrees: `claude -w name` for separate parallel pieces of work so edits don't collide; gitignored files and dependencies are not copied (`.worktreeinclude`, `worktree.symlinkDirectories`). `/batch` splits big mechanical changes into 5-30 worktree units. — [Best practices](https://code.claude.com/docs/en/best-practices)
- Background usage: idle compaction, session recap, prompt suggestions, scheduled `/loop` tasks and cross-session messages can spend tokens while idle. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Docs: `/usage` attributes usage to skills, subagents, plugins and MCP servers and recommends fixes for behaviours behind 10%+ of usage. — [Claude Code costs](https://code.claude.com/docs/en/costs)

### Inferences
- A daily 5-6 h routine maps to: one task per session, `/clear` (or `/rename` then `/clear`) at task boundaries, a short handoff note in a file (TASKS.md or similar) rather than carrying a session across the day.
- Parallel sessions multiply tokens roughly linearly; for a solo dev with a single Unity Editor instance (one project folder, one Library), a second worktree would need its own Editor/import, so parallel worktrees are likely poor value for Unity work. This is my inference; no source addresses Unity.
- Coming back after lunch/break implies a full-context cache miss; with a big context, a fresh session is cheaper than resuming.

### Gaps
- No measured comparison of "one task per session" vs long sessions on real cost or quality in coding agents specifically.
- Chroma and Lost in the Middle are not coding-agent studies; transfer to agentic coding is an inference.

## Prompt precision, reading efficiently, stopping runaway loops

### Takeaway
Specific prompts naming the file/function avoid broad scanning; Anthropic's design for Claude Code is just-in-time retrieval (grep/glob, head/tail). Evidence is guidance plus one measured study on context files.

### Cited Findings
- "Vague requests like 'improve this codebase' trigger broad scanning. Specific requests like 'add input validation to the login function in auth.ts' let Claude work efficiently with minimal file reads." — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Claude Code retrieves just-in-time via lightweight identifiers (paths, queries) plus grep/glob and head/tail, with CLAUDE.md loaded upfront (hybrid). Tradeoff: runtime exploration is slower and needs good guidance to avoid wasted context. — [Effective context engineering](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents)
- Prefer CLI tools (`gh`, etc.) to MCP servers; MCP tool definitions are deferred by default so only names and server instructions enter context until used; disable unused servers (`/mcp`), check `/context`. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Subagents isolate high-volume operations (tests, docs fetching, logs) and return only a summary. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Stopping loops: Esc immediately on drift; `/rewind`; after two failed corrections `/clear` with a better prompt. — [Best practices](https://code.claude.com/docs/en/best-practices)
- Docs: don't ask an agent to re-verify after every edit unnecessarily is not stated; the "don't re-read file after edit" tip is tool-harness behaviour (Edit errors on failure), not a cited study. (No source; see Gaps.)

### Inferences
- Prompt template that follows from the sources: goal + exact file/symbol + constraint + how to verify (command) + "don't touch X". Request a diff/summary, not full-file output.
- For logs and XML (Editor.log, TestResults.xml) read with grep/offset-limit, not whole files.

### Gaps
- No quantified token saving for specific-vs-vague prompts or offset/limit reads. No source on re-reading files after edits.

## Delegation economics: model choice, subagents, multi-agent

### Takeaway
Anthropic's measured numbers: agents ~4x chat tokens, multi-agent ~15x. Token use alone explained 80% of research-quality variance, but coding tasks have fewer parallelizable parts, so multi-agent is a poor default for coding.

### Cited Findings
- "Agents typically use about 4x more tokens than chat interactions," and multi-agent systems about 15x more than chats. — [Anthropic multi-agent research system](https://www.anthropic.com/engineering/multi-agent-research-system)
- On BrowseComp, three factors explained 95% of performance variance; token usage alone explained 80% (the others: tool calls and model choice). — [Anthropic multi-agent research system](https://www.anthropic.com/engineering/multi-agent-research-system)
- Effort-scaling guidance: simple fact-finding 1 agent / 3-10 tool calls; comparisons 2-4 subagents / 10-15 calls each; complex research 10+ subagents. Multi-agent is a poor fit where agents share context or have many dependencies; "most coding tasks have fewer truly parallelizable parts than research." — [Anthropic multi-agent research system](https://www.anthropic.com/engineering/multi-agent-research-system)
- Model guidance: "Sonnet handles most coding tasks well and costs less than Opus. Reserve Opus for complex architectural decisions or multi-step reasoning." A switch to Opus also applies to subagents that inherit the session model; use `model: haiku` for simple subagent tasks. Haiku is "a good fit for subagents doing simple jobs." — [Claude Code costs](https://code.claude.com/docs/en/costs); [Best practices](https://code.claude.com/docs/en/best-practices)
- Subagents' own requests still draw on usage; the saving is context isolation, not tokens. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Agent teams use ~7x more tokens than standard sessions when teammates run in plan mode; use Sonnet for teammates, keep teams small. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Enterprise average ~$13 per developer per active day, $150-250 per month, below $30/day for 90% of users (API-billing figures, enterprise deployments). — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Default model in Claude Code docs is Opus 5.5 at medium effort on most plans, so the model is expensive unless changed. — [Best practices](https://code.claude.com/docs/en/best-practices)

### Inferences
- Solo Unity dev: default to Sonnet/medium effort; use Opus/high only for design decisions or hard debugging; use Haiku subagents for mechanical search/summaries. Avoid multi-agent teams for sequential code changes.
- The 80% token-explains-variance finding is for research tasks; it supports "don't starve hard tasks of tokens" as much as "save tokens". Savings should come from waste (rework, rereads, bloat), not from under-resourcing difficult work.

### Gaps
- No published cost split for Haiku vs Sonnet subagents on coding tasks. No source comparing quality of Sonnet vs Opus for Unity/C#.

## Documentation-as-context and stale-doc risk

### Takeaway
Measured evidence (ETH Zurich, ICLR 2026) says generic repo-overview context files do not improve success and add >20% cost; only non-obvious, project-specific rules help. Anthropic's docs agree: write down only what Claude can't find on its own.

### Cited Findings
- Gloaguen et al. (ETH Zurich / LogicStar.ai, ICLR 2026): across Claude Code (Sonnet 4.5), Codex, Qwen Code, context files gave no improvement in task success; LLM-generated ones reduced success by ~0.5-2% (one secondary summary), developer-written ones gave ~4% gains; inference cost rose >20% (LLM-generated files: +3.92 steps, +23% cost). Agents follow the files and explore/test more. Repository overviews not helpful; useful for non-standard practices. Python-only benchmarks. (Figures via secondary summaries; secondary sources partly disagree on human-written files' benefit.) — [summary of arXiv 2602.11988](https://arxiviq.substack.com/p/evaluating-agentsmd-are-repository); [aiweekly](https://aiweekly.co/node/5405)
- Docs: "Describe your architecture and file layout in CLAUDE.md" is listed as advice Claude no longer needs: "Claude reads the repo itself. Only write down what isn't in there." — [Best practices](https://code.claude.com/docs/en/best-practices)
- Keep CLAUDE.md under ~200 lines; move workflow-specific instructions to skills (loaded on demand: only name+description always in context); path-scoped `.claude/rules/` with `paths:` filters load only where relevant; `/doctor` suggests cuts. — [Claude Code costs](https://code.claude.com/docs/en/costs); [Best practices](https://code.claude.com/docs/en/best-practices)
- If Claude ignores a CLAUDE.md line, the file may be too long so the line gets lost; unbreakable rules go into hooks. — [Best practices](https://code.claude.com/docs/en/best-practices)
- A "codebase-overview" skill can give domain knowledge so Claude doesn't explore (docs example). Anthropic's own context-engineering post recommends structured notes and CLAUDE.md upfront plus on-demand retrieval. — [Claude Code costs](https://code.claude.com/docs/en/costs); [Effective context engineering](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents)

### Inferences
- Per-system docs (like docs/systems/*.md) are good value if they hold intent, invariants and gotchas that code doesn't reveal, and are read on demand (referenced, not auto-loaded). Docs that restate code structure are the kind the ETH study found unhelpful and costly.
- Stale docs are a risk: no source quantifies it, but an agent that "follows files closely" (ETH finding) will follow wrong ones too. A "last verified" date plus update-on-behaviour-change rule limits drift.

### Gaps
- No measured study of per-feature docs vs reading code, nor of stale-doc harm.
- ETH figures come from secondary summaries; I did not read the paper PDF.

## Anti-patterns that waste tokens

### Takeaway
Documented wasters: never-cleared long sessions, default Opus, vague prompts, bloated CLAUDE.md, unused MCP servers, repeated corrections, idle scheduled tasks and large unfiltered outputs.

### Cited Findings
- Long sessions never cleared and Opus left as default are the usual cause of unexpectedly high spend. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Third correction in a polluted context, rather than `/clear` + better prompt. — [Best practices](https://code.claude.com/docs/en/best-practices)
- Raw 10,000-line logs in context instead of hook-filtered output. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- MCP servers/plugins left enabled; check `/context` and `/mcp`. Deferred tool loading reduces but doesn't remove the listing cost; CLI tools have no per-tool listing. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Auto-generated, overview-style context files (+20-23% cost, no success gain). — [summary of arXiv 2602.11988](https://arxiviq.substack.com/p/evaluating-agentsmd-are-repository)
- Idle-time spend: session-scoped scheduled tasks, cross-session messages, goal check-ins, idle compaction and cache misses after breaks. — [Claude Code costs](https://code.claude.com/docs/en/costs)
- Multi-agent/agent-team use for tightly-coupled coding tasks (~7x-15x tokens). — [Claude Code costs](https://code.claude.com/docs/en/costs); [Anthropic multi-agent research system](https://www.anthropic.com/engineering/multi-agent-research-system)
- Perceived vs actual productivity: METR RCT (July 2025; 16 experienced OSS devs, 246 tasks, early-2025 tools incl. Cursor with Claude 3.5/3.7) found AI allowed made tasks 19% slower while developers believed 20% faster. Small sample, old tools, mature repos; cautionary about self-assessed speedups, not a verdict on current tools. — [The Register summary](https://www.theregister.com/2025/07/11/ai_code_tools_slow_down/)

### Inferences
- Habit list for solo dev: clear per task; Sonnet default; measure with `/usage` weekly rather than guess; keep CLAUDE.md to non-derivable rules; filtered hooks only.
- Given METR's gap between perception and measurement, track actual outcomes (tasks done/day, rework count) alongside tokens.

### Gaps
- No source found on "re-reading files after edits" as a measured waste; the Edit tool erroring on failure is tool behaviour. Treat as anecdote.
- METR is dated (2025); I did not check for later follow-ups. Chroma is a vendor study; not independently replicated here.
