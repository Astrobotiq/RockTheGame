# Claude Code built-in levers for token savings (as of 2026-11-11)

Sources are the official Claude Code docs on code.claude.com, fetched 2026-11-11, plus one Anthropic engineering post. Model names follow the docs (Opus 5.5, Sonnet 5.5, Haiku 5.5, Fable 5.2). Priority order for the report writer: (1) context hygiene and /clear, (2) cache-safe habits, (3) model and effort, (4) CLAUDE.md slimming, (5) deny rules and hook filtering, (6) subagents used selectively, (7) measuring.

## 1. Prompt caching: TTL, what breaks it, how to keep hits high

### Takeaway
Caching is automatic and prefix-based. Model switches, MCP and tool-set changes (when not deferred), fast-mode toggles, compaction and Claude Code upgrades break the cache. Editing CLAUDE.md mid-session does not break it, but the edit also does not apply until `/clear`, `/compact` or a restart. Pick model and effort at session start and avoid mid-task changes.

### Cited Findings
- Claude Code re-sends the full context every request. The API matches the exact prefix, so a change anywhere in the prefix recomputes everything after it. There is no per-file caching. Request order is: system prompt and tool definitions, then project context (CLAUDE.md, auto memory, unscoped rules), then conversation. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- TTL is 5 minutes or 1 hour. Each cache hit resets the timer. The 1h TTL costs more per cache write. By default Claude Code requests 1h for the main conversation only on a Claude subscription within included plan usage. With an API key, cloud provider or usage credits the main conversation gets 5m. Subagents, workflows, teammates, forks, compaction and session titles get 5m by default. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- TTL controls (v2.1.242 or later): `promptCacheTtl` setting or `CLAUDE_CODE_PROMPT_CACHE_TTL` for the main conversation; `subagentPromptCacheTtl` or `CLAUDE_CODE_SUBAGENT_PROMPT_CACHE_TTL` for everything else. Values are `5m` or `1h`. `FORCE_PROMPT_CACHING_5M=1` and `ENABLE_PROMPT_CACHING_1H=1` also exist. Check which TTL was used with `claude -p "hello" --output-format json` and read `usage.cache_creation`. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- Actions that invalidate the cache: switching models (each model has its own cache); changing effort level on most models; toggling fast mode; connecting or removing an MCP server when tools are not deferred; enabling or disabling a plugin that ships MCP servers; denying an entire tool when tool search is off; `/compact`; accumulating many images (oldest are dropped in batches); upgrading Claude Code. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- Effort changes keep the cache on Opus 5.5, Sonnet 5.5, Haiku 5.5 and Fable 5.1+ when signed in with an API key or subscription. This does not hold on Bedrock or Agent Platform. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- `opusplan` switches model on every plan-mode toggle, so each toggle is a model switch and starts a fresh cache. A skill whose frontmatter names a different `model` also causes a one-turn model switch. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- With MCP tool search active (the default), Claude Code keeps the first request's tool list for the whole conversation. A server connecting or disconnecting mid-session does not disturb the cache. Bare-tool deny rules also keep the cache while tool search is on. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- Actions that keep the cache: editing repo files (a system-reminder is appended), editing CLAUDE.md (not applied until `/clear`, `/compact` or restart), changing permission mode, changing output style, invoking skills and commands, `/recap`, `/rewind`, and spawning a subagent. Plugin skills, commands, agents and hooks are appended without invalidation. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- Official tip: pick model and effort at the top of the session, and save `/compact` for natural breaks. `/rewind` hits an earlier cached prefix, so it is cheaper than compacting an abandoned path. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- Fast mode: turning it on or off starts a separate cache. The first enable in a conversation reads the whole context uncached at fast-mode rates, so enable it at session start. — [Fast mode](https://code.claude.com/docs/en/fast-mode.md)
- Cache scope is effectively one machine and directory. Parallel sessions in the same directory can share cache. Different directories do not. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- `/usage` adds a `Prompt cache (main)` line (v2.1.251+) with hit share, misses and warm or cold state. v2.1.260+ names a likely miss cause, for example "tool definitions changed". — [Costs](https://code.claude.com/docs/en/costs)
- Disabling caching (`DISABLE_PROMPT_CACHING` and per-family variants) is meant only for debugging; docs say leave it on. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- `/model` asks for confirmation only while the cache is still warm (v2.1.238+). — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)

### Inferences
- The Unity workflow here (long sessions with an Editor connection, plugins, hooks) benefits most from not toggling model, effort or fast mode mid-task, and from compacting only at task boundaries.
- The user's per-turn cost is dominated by cache reads of a growing context. Clearing between tasks is more valuable than cache tuning.
- If the user runs on a subscription, the 1h main-conversation TTL is already the default. The TTL settings matter mostly for API-key or credit billing.

### Gaps
- Exact cache read and write price multipliers are on the API pricing page, which I did not fetch. The commonly cited 0.1x read and 1.25x (5m) or 2x (1h) write figures are from my memory and unverified here.
- I did not open Anthropic's blog post "Lessons from building Claude Code: Prompt caching is everything" (linked from the docs).

## 2. Context hygiene: /clear, /compact, auto-compact, /context, resume

### Takeaway
Every request carries the full history, so long sessions get expensive even when cached. Use `/clear` between unrelated tasks (free), `/compact <focus>` at natural breaks, and `/rewind` to abandon dead ends. Resuming an old session and then compacting is the most expensive case.

### Cited Findings
- "Clear between tasks": stale context wastes tokens on every subsequent message. Use `/rename` before `/clear`, then `/resume` to return later. `/clear` costs nothing. Session cost totals reset on `/clear` since v2.1.211. — [Costs](https://code.claude.com/docs/en/costs)
- `/compact` itself reads the whole conversation, so compacting a large context is a large request. While the cache is warm it reads the prefix from cache. After a break longer than the TTL it reprocesses everything uncached. This is why compaction costs most when resuming an old session. — [Costs](https://code.claude.com/docs/en/costs); [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- Custom compaction instructions: `/compact Focus on code samples and API usage`, or a "Compact instructions" section in CLAUDE.md. — [Costs](https://code.claude.com/docs/en/costs)
- Partial compaction: `/rewind`, pick a message, choose "Summarize from here" or "Summarize up to here". — [Context window](https://code.claude.com/docs/en/context-window.md)
- Auto-compact window: `/autocompact 500k`; `CLAUDE_CODE_AUTO_COMPACT_WINDOW` (100000 to 1000000); `CLAUDE_AUTOCOMPACT_PCT_OVERRIDE` (1-100, can only lower the threshold; applies to subagents too). Haiku 5.5 auto-compacts at about 967K by default. — [Env vars](https://code.claude.com/docs/en/env-vars.md); [Model config](https://code.claude.com/docs/en/model-config)
- Native 1M context on Fable, Sonnet 5+, Haiku 5.5 and Opus 4.7+. There is no price premium beyond 200K on the Anthropic API except Haiku 5.5 (costlier above 100K). `CLAUDE_CODE_DISABLE_1M_CONTEXT=1` treats these models as 200K. — [Model config](https://code.claude.com/docs/en/model-config)
- After compaction, project-root CLAUDE.md, unscoped rules and auto memory are re-injected from disk. Up to 5 recently read or edited files are re-read; files over 5,000 tokens come back as path references. Invoked skill bodies are capped at 5,000 tokens each and 25,000 total. Path-scoped rules and nested CLAUDE.md are summarized away and reload on demand. — [Context window](https://code.claude.com/docs/en/context-window.md)
- `/context` gives a live breakdown by category with optimization suggestions. `/context all` lists loaded Memory files. — [Context window](https://code.claude.com/docs/en/context-window.md); [Memory](https://code.claude.com/docs/en/memory)
- Why long sessions cost more: full conversation sent each request; cache misses after idle gaps; session-scoped scheduled tasks, cross-session messages and goal check-ins fire on idle sessions and resend full context; subagents and teammates add their own requests; `/compact` is a large request. `/usage` flags behaviors behind 10% or more of recent usage. — [Costs](https://code.claude.com/docs/en/costs)
- Idle compaction is on by default for conversations meeting its conditions (default threshold `CLAUDE_CODE_IDLE_COMPACT_MIN_TOKENS` 200000, v2.1.290+). It applies when a conversation on the 1h TTL is left idle. Turn it off with `idleCompaction: false`. Session recap and prompt suggestions also use some tokens in the background; disable suggestions with `CLAUDE_CODE_ENABLE_PROMPT_SUGGESTION=false`. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md); [Env vars](https://code.claude.com/docs/en/env-vars.md); [Costs](https://code.claude.com/docs/en/costs)
- Cost anchors from Anthropic: average about $13 per developer per active day and $150-250 per developer per month; below $30 per active day for 90% of users (enterprise deployments, not Unity-specific). — [Costs](https://code.claude.com/docs/en/costs)

### Inferences
- Rule of thumb consistent with the docs: one task, one session; `/clear` at each task boundary; `/compact <focus>` only mid-task when context is large and the cache is warm; avoid resuming a stale session just to ask a small question.
- Combined with the project's `TASKS.md` and docs, a fresh session re-loads state cheaply from files, so `/clear` rarely loses anything important.

### Gaps
- Default auto-compact percentage threshold was not captured (it is in "Default auto-compact thresholds" in the model-config page, in the unread tail).
- No Anthropic-published numeric "sweet spot" for when to compact versus clear.

## 3. CLAUDE.md, memory, rules and skills

### Takeaway
Target under 200 lines per CLAUDE.md. Move specialized or file-specific instructions to path-scoped rules or skills, which load on demand. Imports organize but do not save tokens. The project's own CLAUDE.md is far larger than the guideline.

### Cited Findings
- Size guidance: "target under 200 lines per CLAUDE.md file"; longer files cost more context and reduce adherence. Claude Code skips a CLAUDE.md over 4 MiB. — [Memory](https://code.claude.com/docs/en/memory)
- `@path` imports expand into context together with the importing file (max depth four hops), so they organize but "don't reduce its context cost". — [Memory](https://code.claude.com/docs/en/memory)
- Nested CLAUDE.md in subdirectories loads on demand when Claude reads, writes or edits a file there. Parent-directory CLAUDE.md files load at launch. — [Memory](https://code.claude.com/docs/en/memory)
- `.claude/rules/*.md` without `paths` loads at launch. With `paths:` frontmatter it loads only when Claude uses Read, Write or Edit (or a single-file `cat`/`head`) on a matching file. — [Memory](https://code.claude.com/docs/en/memory)
- Skills load only when invoked or judged relevant. The costs page says to move detailed workflow instructions out of CLAUDE.md into skills and keep CLAUDE.md under 200 lines. Skill descriptions load at session start and are rebuilt after compaction. — [Costs](https://code.claude.com/docs/en/costs); [Context window](https://code.claude.com/docs/en/context-window.md)
- Block-level HTML comments in CLAUDE.md are stripped before injection (free maintainer notes). — [Memory](https://code.claude.com/docs/en/memory)
- Auto memory: the first 200 lines or 25KB of `MEMORY.md` load every session, whichever comes first. Topic files are read on demand. Disable with `CLAUDE_CODE_DISABLE_AUTO_MEMORY=1` or `autoMemoryEnabled: false`. — [Memory](https://code.claude.com/docs/en/memory); [Env vars](https://code.claude.com/docs/en/env-vars.md)
- `claudeMdExcludes` setting skips specific CLAUDE.md or rules files by glob. The "Project instructions" config chooses between CLAUDE.md and AGENTS.md loading. — [Memory](https://code.claude.com/docs/en/memory)
- `/doctor prompt-audit` (v2.1.283+) finds outdated or conflicting instruction files. `/usage` may recommend trims with per-request token counts (example row: "Trim ~/code/api/CLAUDE.md · 49k tokens per request"). — [Memory](https://code.claude.com/docs/en/memory); [Costs](https://code.claude.com/docs/en/costs)
- Edits to root CLAUDE.md mid-session keep the cache but do not apply until `/clear`, `/compact` or restart. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- Content belongs in CLAUDE.md if it is a fact needed every session (build and test commands, conventions, layout, "always do X"). Multi-step procedures or single-area rules go to skills or path-scoped rules. — [Memory](https://code.claude.com/docs/en/memory)

### Inferences
- This repo's CLAUDE.md is long (well over 200 lines of dense architecture prose), plus a 9-entry user memory index. Candidates to move out: per-system architecture sections (Platforms, Rooms, Death/respawn, Event channels) into path-scoped rules keyed to `Assets/Scripts/New-Scripts/Platform/**` etc., or into the existing `docs/systems/` files read on demand; UI motion rules into a skill or a path-scoped rule for `UI/**`. I did not measure the actual token count; `/context` would show it.
- Path-scoped rules and nested CLAUDE.md are summarized away at compaction and reload on demand, so rules that must always hold should stay in the root file.
- The Turkish planning/calendar section only matters during planning sessions, a good skill candidate.

### Gaps
- I did not confirm the skill-listing character budget (for example `SLASH_COMMAND_TOOL_CHAR_BUDGET`) in the current docs. Not found in the fetched excerpts.
- The per-file limit for path-scoped rules and subdirectory CLAUDE.md ("a smaller per-file limit of their own") was referenced but not read.

## 4. MCP and tool overhead

### Takeaway
MCP tool definitions are deferred by default (tool search), so only names and server instructions occupy context. CLI tools remain more context-efficient than MCP. Disable unused servers and plugins, and check `/context`.

### Cited Findings
- Tool search is on by default; only tool names and server instructions load at session start. There is no fixed per-server tool cap. — [MCP](https://code.claude.com/docs/en/mcp.md)
- `ENABLE_TOOL_SEARCH`: `auto` loads tools upfront while definitions total under 10% of the context window and defers at 10% or more; `auto:N` sets a custom percentage; `false` disables; `alwaysLoad: true` per server exempts it from deferral. Tool search applies on Opus 4, Sonnet 4, Haiku 4.5 and later (Opus 4.5, Sonnet 4.5, Haiku 4.5 and later on Agent Platform). — [MCP](https://code.claude.com/docs/en/mcp.md)
- Costs page: "Prefer CLI tools" (`gh`, `aws`, `gcloud`, `sentry-cli`) because they add no per-tool listing. "Disable unused servers" via `/mcp`. — [Costs](https://code.claude.com/docs/en/costs)
- MCP output limits: warning at 10,000 tokens (fixed); default cap 25,000 tokens; raise with `MAX_MCP_OUTPUT_TOKENS`; text results over 50,000 characters are saved to a file. — [MCP](https://code.claude.com/docs/en/mcp.md)
- Disable a server for one project in `/mcp` (stored in `disabledMcpServers` in `~/.claude.json`); project `.mcp.json` servers use `disabledMcpjsonServers`; permanent removal with `claude mcp remove`; `--strict-mcp-config` loads only `--mcp-config` servers. — [MCP](https://code.claude.com/docs/en/mcp.md)
- Caching impact: with deferred tools, connecting or removing a server mid-session does not break the cache. Without tool search, adding a tool does, and so does removing one on purpose (for example disabling a server). — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- `/usage` attributes plan usage to individual MCP servers, skills, plugins and subagents. — [Costs](https://code.claude.com/docs/en/costs)
- Subagent definitions: combined `description` fields of non-built-in subagents above 15,000 tokens produce a startup warning. — [Sub-agents](https://code.claude.com/docs/en/sub-agents.md)
- Code intelligence plugins (LSP) reduce file reads via symbol navigation and report type errors after edits. — [Costs](https://code.claude.com/docs/en/costs)

### Inferences
- Because tool definitions are deferred, the main MCP cost is no longer definitions but large tool results (screenshots, listings) and server instruction text. For this repo the Unity Pipeline CLI (`unity command ...`) is already the "CLI over MCP" pattern. A C# language server plugin may help, but its Unity value was not verified.
- Unused plugin MCP servers (several unauthenticated ones appear in this environment) cost little when deferred but still add names and instructions.

### Gaps
- No per-server token numbers (such as the often-quoted "10-20K tokens per server") were found in current docs. The docs now describe deferral instead. Treat older community numbers as outdated.
- Whether Unity's `com.unity.pipeline` or any C# LSP plugin exists for Claude Code was not checked.

## 5. Model and effort selection

### Takeaway
Defaults are `medium` effort on Opus 5.5, Sonnet 5.5, Haiku 5.5 and Fable 5.2. Use Sonnet for most coding, Opus for hard design, Haiku for simple subagents, and lower effort for simple tasks. Thinking cannot be disabled on the 5.5 models, so effort is the only lever.

### Cited Findings
- Aliases: `opus` is Opus 5.5, `sonnet` is Sonnet 5.5, `haiku` is Haiku 5.5, `fable` is Fable 5.2, `opusplan` uses Opus in plan mode and Sonnet for execution. `default` is currently Opus 5.5 on most accounts. — [Model config](https://code.claude.com/docs/en/model-config)
- Costs guidance: "Sonnet handles most coding tasks well and costs less than Opus"; reserve Opus for complex architectural decisions. Unexplained high API spend usually traces to long uncleared sessions or Opus left as the default. — [Costs](https://code.claude.com/docs/en/costs)
- Effort levels `low`, `medium`, `high`, `xhigh`, `max` on the 5.x and Opus 4.7+ models. `/effort`, `--effort`, `CLAUDE_CODE_EFFORT_LEVEL`. `effortLevel` in settings accepts low to xhigh, not max. `max` applies to the session only unless set by env var. — [Model config](https://code.claude.com/docs/en/model-config)
- Thinking tokens are billed as output tokens. You cannot turn thinking off on Opus 5.5, Sonnet 5.5, Haiku 5.5 or Fable; the model decides per step from effort. `MAX_THINKING_TOKENS` only affects fixed-budget models (Opus 4.6, Sonnet 4.6); adaptive models ignore nonzero budgets. — [Costs](https://code.claude.com/docs/en/costs); [Model config](https://code.claude.com/docs/en/model-config)
- `ultrathink` in a prompt raises that single turn without changing the session setting. — [Model config](https://code.claude.com/docs/en/model-config)
- Fast mode: up to 2.5x faster at higher per-token price: $8/$40 per MTok on Opus 5.5, $16/$80 on Fable 5.2, $10/$50 on Opus 5 and 4.8. Only Opus and Fable models support it. On subscriptions it draws on usage credits only, not plan limits. Not recommended for long autonomous or cost-sensitive work. — [Fast mode](https://code.claude.com/docs/en/fast-mode.md)
- Subagent model resolution (v2.1.251+): per-invocation parameter, then frontmatter `model`, then `CLAUDE_CODE_SUBAGENT_MODEL`, then the main model. `CLAUDE_CODE_SUBAGENT_MODEL_FORCE=1` (v2.1.257+) forces one model on every subagent. — [Sub-agents](https://code.claude.com/docs/en/sub-agents.md)
- The built-in Explore agent runs on Opus (the `opus` alias) on a subscription, Console account or gateway, and on the main model on Bedrock, Google Cloud, Foundry and similar providers. To run exploration on a cheaper model, define your own subagent named `Explore` with `model: haiku`. — [Sub-agents](https://code.claude.com/docs/en/sub-agents.md)
- Agent teams: about 7x the tokens of a standard session when teammates run in plan mode; use Sonnet for teammates and keep teams small. Teams are disabled by default. — [Costs](https://code.claude.com/docs/en/costs)

### Inferences
- Setting `effortLevel` to `low` or `medium` as default and using `ultrathink` or `/effort high` per task gives most of the saving at the start of a session, without mid-session cache loss on the 5.5 models.
- The Explore-on-Opus default is a notable hidden cost when the user is on a subscription; an `Explore` override with Haiku is a cheap fix.
- Standard (non-fast) per-token prices for the 5.5 models were not retrieved, so the Haiku/Sonnet/Opus ratio cannot be stated from these sources.

### Gaps
- Standard API list prices for Opus 5.5 / Sonnet 5.5 / Haiku 5.5 (platform.claude.com pricing page) were not fetched.
- Which is the best default effort for Unity/C# work is untested; no Anthropic benchmark found.

## 6. Subagents: when they save tokens and when they cost more

### Takeaway
Subagents save main-context tokens by isolating verbose work and returning summaries, but each spawns its own context and requests, so total tokens rise. Anthropic's published research-system figures: agents about 4x chat tokens, multi-agent about 15x.

### Cited Findings
- Each subagent has its own context window, system prompt and tools, and sees only the delegation message, not your conversation. Only its summary returns to the main thread. Use them for test runs, doc fetching and log processing. — [Sub-agents](https://code.claude.com/docs/en/sub-agents.md); [Costs](https://code.claude.com/docs/en/costs)
- Cost caveat from the docs: subagent requests count toward the same usage limits, each spends tokens while it runs, and many detailed returns can consume significant main context. Pick a smaller model for simple subagents. — [Sub-agents](https://code.claude.com/docs/en/sub-agents.md); [Costs](https://code.claude.com/docs/en/costs)
- A subagent's first request does not read the parent's cache (different prefix). It builds its own, with a 5m TTL by default. A fork inherits the parent's system prompt and history, so its first request reads the parent's cache. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)
- Built-in Explore and Plan agents skip CLAUDE.md and git status, which keeps their startup context small. Explore is read-only with quick, medium or very thorough thoroughness levels. — [Sub-agents](https://code.claude.com/docs/en/sub-agents.md)
- Anthropic's multi-agent research system: agents use about 4x more tokens than chat, multi-agent about 15x more; token usage alone explained about 80% of performance variance on BrowseComp. It is economical only when task value is high; it is a poor fit for heavily interdependent work "such as most coding tasks". — [Anthropic engineering](https://www.anthropic.com/engineering/multi-agent-research-system)
- Defaults: 20 concurrent subagents (`CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS`), nesting depth 3 (`CLAUDE_CODE_MAX_SUBAGENT_SPAWN_DEPTH`). Subagents spawned from the main conversation run in the background by default. — [Sub-agents](https://code.claude.com/docs/en/sub-agents.md)
- Subagent frontmatter supports `model`, `effort`, `tools` / `disallowedTools`, and an experimental `cacheTtl`. — [Sub-agents](https://code.claude.com/docs/en/sub-agents.md); [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md)

### Inferences
- Use a subagent when the raw output is large and only a conclusion is needed (a big grep sweep, a long test log, a docs fetch). Skip it for small reads or tightly coupled edits, where the cold start (own system prompt, tools, delegation text) outweighs the savings.
- For Unity work, delegating `Editor.log` triage or full test-result parsing to a Haiku subagent is the most obvious saver.
- The 4x/15x figures describe Anthropic's own research agents, not Claude Code subagents specifically; they are indicative only.

### Gaps
- No Anthropic number for the per-spawn overhead of a Claude Code subagent (tokens of system prompt plus tools). Not found.
- The docs state no break-even point for when delegation pays off.

## 7. Keeping junk out of context: deny rules, hooks, output limits, partial reads

### Takeaway
Use `Read` deny rules (the replacement for the ignored `.claudeignore`) for generated folders, a PreToolUse hook to trim noisy command output, and the built-in size limits. Prefer offset/limit reads for large files.

### Cited Findings
- `.claudeignore` has no effect; use `Read(path)` deny rules, for example `Read(./secrets/**)`. Claude makes a best-effort attempt to apply `Read` deny rules to Grep, Glob and `@file` mentions. `Grep(...)` and `Glob(...)` path rules do not match; write `Read(...)` rules instead. — [Permissions](https://code.claude.com/docs/en/permissions.md)
- Patterns use gitignore syntax. A deny rule like `Read(secrets/**)` matches a directory named `secrets` at any depth under the current directory. `//path` is absolute, `~/path` is home-relative. A `!` negation can carve out exceptions. — [Permissions](https://code.claude.com/docs/en/permissions.md)
- Hooks can preprocess output: the official example is a PreToolUse hook that rewrites test commands to pipe through `tail -n 100`; the docs say a hook grepping for ERROR can cut "tens of thousands of tokens to hundreds". — [Costs](https://code.claude.com/docs/en/costs)
- Bash output cap: `BASH_MAX_OUTPUT_LENGTH` default 30000 characters, maximum 150000 (the `bashOutputMaxChars` setting takes precedence). File read cap: `CLAUDE_CODE_FILE_READ_MAX_OUTPUT_TOKENS` default 25,000 tokens; a read with `allow_large` can exceed it. `CLAUDE_CODE_MAX_OUTPUT_TOKENS` raises or lowers the response cap and reduces effective context when raised. — [Env vars](https://code.claude.com/docs/en/env-vars.md)
- MCP output: warn at 10,000 tokens, default cap 25,000, `MAX_MCP_OUTPUT_TOKENS` to change. — [MCP](https://code.claude.com/docs/en/mcp.md)
- Specific prompts reduce exploration ("add input validation to the login function in auth.ts" beats "improve this codebase"). Plan mode (Shift+Tab) avoids rework; Escape and `/rewind` stop wrong paths early; give verification targets. — [Costs](https://code.claude.com/docs/en/costs)
- Code intelligence plugins replace grep-then-read cycles with "go to definition". — [Costs](https://code.claude.com/docs/en/costs)

### Inferences
- For Unity, a deny list such as `Read(Library/**)`, `Read(Temp/**)`, `Read(Logs/**)`, `Read(obj/**)`, `Read(**/*.meta)` and large generated assets would block accidental reads. The repo already denies Edit/Write on `*.unity`. Note the hook-based Editor.log reading (`error CS` lines) is the right kind of filtering and could be a small script that returns only error lines.
- `.meta` denial is a judgment call: Claude sometimes needs GUIDs, so deny only if not needed. I did not test this.
- The docs say "best-effort" for non-Read tools, so deny rules are not a hard guarantee that Bash (for example `cat`) cannot read these paths.
- `bashOutputMaxChars` or `BASH_MAX_OUTPUT_LENGTH` could be lowered below 30000 for chatty Unity commands; no doc source gives a recommended value.

### Gaps
- The `respectGitignore` setting and Glob/Grep default ignore behavior were not confirmed (the permissions page grep returned no hit for it).
- No doc guidance found for reading files with offset and limit beyond the read-cap behavior; that is tool behavior rather than a documented setting.

## 8. Usage monitoring

### Takeaway
`/usage` (session cost, cache line, plan breakdown with recommendations) is the primary built-in tool. The status line can show context and cache fields continuously. Anthropic suggests measuring a pilot baseline first.

### Cited Findings
- `/usage` Session block: total cost (computed locally at list price, an estimate), API and wall duration, code changes, per-model token breakdown including cache read and write. On Pro, Max and Ultra the dollar figure is not what you are billed. Totals reset on `/clear` (v2.1.211+). — [Costs](https://code.claude.com/docs/en/costs)
- `Prompt cache (main)` line (v2.1.251+): request count, share of input tokens from cache, misses, expected rebuilds from compaction, warm or cold with TTL. It covers the main conversation only, not subagents. — [Costs](https://code.claude.com/docs/en/costs)
- Plan usage breakdown on subscription plans: attribution to skills, subagents, plugins and MCP servers; recommendations for behaviors at 10% or more of recent usage (24h or 7d, key `d`/`w`); loop rows for `/loop` tasks (v2.1.242+); `f` applies fixes such as effort changes (v2.1.305+). Figures are approximate and local to the machine. — [Costs](https://code.claude.com/docs/en/costs)
- Status line scripts can read cost fields, a `prompt_cache` object (v2.1.251+) and `current_usage` (`cache_creation_input_tokens`, `cache_read_input_tokens`); a high read-to-creation ratio means caching works. — [Prompt caching](https://code.claude.com/docs/en/prompt-caching.md); [Costs](https://code.claude.com/docs/en/costs)
- `/insights` writes an HTML report of usage patterns and friction from up to 200 recent sessions to `~/.claude/usage-data/report.html`; it consumes tokens itself. — [Costs](https://code.claude.com/docs/en/costs)
- The spinner token count shows only the current turn's output, not input or history. `/context` shows context composition. — [Costs](https://code.claude.com/docs/en/costs)
- OpenTelemetry export reports per-user token and cost metrics, including cache read and creation. — [Costs](https://code.claude.com/docs/en/costs)
- ccusage (community CLI that reads local transcripts) was not verified in this research.

### Inferences
- A practical A/B method: record `/usage` totals and the cache line for a representative task before and after each change (for example, trimming CLAUDE.md or switching default effort). The cache line distinguishes cache-driven cost from context-size-driven cost.

### Gaps
- ccusage: not checked against any source, so no claims made.
- Whether `/cost` still exists as a command was not confirmed; the current docs describe `/usage` and show session cost there.
- Exact status line JSON schema beyond the fields named above was not read.
