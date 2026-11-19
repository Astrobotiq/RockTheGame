# AI-assisted and agent-assisted level design for Unity (2025-2026), with Claude Code driving the Editor

Research date: 2026-11-18. ~18 tool calls. Project context: Unity 6000.3, already driven from Claude Code via `com.unity.pipeline` (`unity command eval`, `capture_game_view`, `run_tests`), rooms are prefab variants of sprite blocks, and the human decides game feel.

## 1. Unity–LLM bridges: what exists and what operations they expose

### Takeaway
By late 2026 there are three mature routes into the Editor: CoplayDev "MCP for Unity" (the largest, ~16k stars, v10.3.0 Oct 2026), IvanMurzak "Unity-MCP / AI Game Developer" (~4.7k stars, Roslyn `script-execute`, isolated screenshots, runtime tools), and Unity's own MCP server, which ships inside the Unity AI Assistant package (open beta, needs a subscription). All of them cover scene/GameObject CRUD, console, tests and custom C# tools. None ships level-design-specific tools. The project's existing `unity command eval` + `capture_game_view` + `run_tests` bridge already covers the same core capability set. Practitioners in 2026 increasingly prefer thin CLIs over MCP for speed and token cost.

### Cited Findings
- **CoplayDev MCP for Unity**: 16.1k stars, ~1.6k forks. Latest release v10.3.0 (2026-10-04). Advertises "50 focused MCP tool entrypoints" for creating scenes and GameObjects, editing C# scripts, managing assets, running tests, profiling and building. Supports Unity 2021.3 LTS through 6.x. Requires Python 3.10+ via `uv`. Supports multi-instance routing and a remote-hosted server with auth. The full tool catalog is at coplaydev.github.io/unity-mcp/reference/tools. — [GitHub CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp)
- Third-party listings name CoplayDev tools such as `manage_scene`, `manage_script`, `manage_gameobject`, `manage_material`, `manage_physics`, `run_tests`, `read_console` and `unity_docs`. `manage_input` needs com.unity.inputsystem. The package is MIT-licensed and separate from Coplay's commercial in-editor assistant. Version numbers differ between mirrors (v8.x to v9.5.4), so these listings are older snapshots. — [mcpservers.org listing](https://mcpservers.org/servers/github-com-coplaydev-unity-mcp), [glama.ai](https://glama.ai/mcp/servers/@CoplayDev/unity-mcp)
- **IvanMurzak Unity-MCP ("AI Game Developer")**: ~4.7k stars, 411 forks.
  - Scenes, GameObjects, prefabs and assets are read and changed through `rql-query` and `rql-mutate`.
  - It reads console and Editor state, takes screenshots with `screenshot-isolated`, runs EditMode and PlayMode tests with `tests-run`, and manages packages.
  - `script-execute` compiles and runs C# in the Editor via Roslyn, which is the equivalent of this project's `unity command eval`.
  - The same tools can run inside a compiled game for runtime debugging.
  - Custom tools are a class marked `[AiToolType]` with methods marked `[AiTool]`.
  - It has a Node CLI (`unity-mcp-cli`, Node 22.14+) and can auto-generate Skills as an alternative to MCP tools.
  - [GitHub IvanMurzak/Unity-MCP](https://github.com/IvanMurzak/Unity-MCP)
- A vendor listing reports Apache-2.0 and release 0.82.4 (July 2026) for Unity-MCP. This is a commercial listing, so treat it with caution. — [dev.co](https://dev.co/ai/mcp/ivanmurzak-unity-mcp)
- **Unity's own MCP server** (blog post dated 2026-05-11) is part of the AI Assistant package.
  - Tool categories: scene management (read the hierarchy; create, modify and delete GameObjects; manage scenes), script editing, console access, GameObject component read/write, build settings, and custom C# MCP tools.
  - Requirements: Unity 6 (6000.0+), a project connected to Unity Cloud, and an active trial or subscription to the AI beta.
  - Claude Code, Cursor, Windsurf and Claude Desktop can be auto-configured.
  - It is open beta, and Unity warns that features "may change, be limited, or be discontinued".
  - It has no dedicated level-design tools.
  - [Unity blog: Unity AI Open Beta – MCP](https://unity.com/blog/unity-ai-mcp-how-to-get-started)
- In the Unity 6.2 manual, the `com.unity.ai.assistant` package covers project-aware retrieval, troubleshooting, code generation and scene editing. — [Unity 6.2 docs](https://docs.unity3d.com/6000.2/Documentation/Manual/com.unity.ai.assistant.html)
- Unity AI has three core parts: an in-editor assistant, an AI Gateway and an MCP server. The Gateway lets you connect a verified third-party agent you already pay for, bypassing Unity credits. The MCP server needs a subscription but does not consume AI credits. A first-time client connection is approved under Project Settings > AI > Unity MCP. — [Unity AI product page](https://unity.com/products/ai), [buildfastwithai guide (secondary)](https://buildfastwithai.com/blogs/unity-ai-open-beta-guide-2026)
- A secondary write-up says the assistant has Ask, Agent and Plan modes, where Plan proposes a plan for user approval before it executes. I could not confirm this on Unity's own pages. — [buildfastwithai (secondary)](https://buildfastwithai.com/blogs/unity-ai-open-beta-guide-2026)
- **Practitioner sentiment, Unity forum (Mar–Jul 2026)**:
  - One user moved from MCP to CLI tools and found them "much faster". Recommended CLIs: unity-cli (youngwoocho02, single binary) and UCP (Unity Control Protocol, a Rust CLI with an Editor bridge).
  - The same user's AGENTS.md uses UnityCli for quick checks and UCP for multi-step scene, prefab and asset workflows.
  - The same user removed about 90% of their skills as unnecessary.
  - "Scene-level AI features are still basic", while Codex could analyze and edit many prefab YAML files.
  - Another user found the Coplay MCP high-friction.
  - Opus "burns through credits very quickly". One user splits Opus 4.6 for planning and Sonnet 4.6 for implementation.
  - The thread never mentions com.unity.pipeline.
  - [Unity Discussions thread, page 2](https://discussions.unity.com/t/cli-mcp-ai-ide-chatgpt-codex-cursor-antigravity-claude-code-windsurf-next-steps-for-unity-workflow-automation/1705679?page=2)

### Inferences
- The project does not need to adopt an MCP server to gain capability. `eval` (≈ Roslyn `script-execute`), `capture_game_view` (≈ `screenshot-isolated`) and `run_tests` (≈ `tests-run`) are the same primitives. The forum trend (CLI faster and cheaper than MCP) supports staying on the existing command bridge.
- The real gain is project-specific "level tools" built as editor C# methods that the agent calls through `eval`, for example `RoomTools.PlaceBlock(room, prefab, gridX, gridY)`, `RoomTools.DumpAscii(room)` and `RoomTools.ValidateRoom(room)`. Every bridge's custom-tool mechanism points to this pattern, and it keeps C# snippets short and deterministic.
- Unity's MCP server is tied to Unity Cloud and a subscription and is still beta. It is not worth switching to for a solo project that already has a working bridge.

### Gaps
- justinpbarnett/unity-mcp was not checked (stars, activity). It was likely the early 2025 original that CoplayDev forked or superseded, but this is unverified.
- I did not fetch the exact CoplayDev tool catalog, so I cannot confirm whether it has screenshot or play-mode tools.
- Unity AI pricing and credit details were not found.

## 2. Workflows: text-grid → room, sketch → blockout, brief → prefab placement, validation, screenshot review, docs, scripted playtests

### Takeaway
The patterns that work in practice put a shared, discrete coordinate system between human and agent: an ASCII grid, labeled grid cells, or tile coordinates. They also give the agent a closed edit → run → screenshot/trace → self-check loop. Free-form "nudge it left" spatial instructions fail. There is community evidence for scripted playtests with screenshot traces (Godot, 2026), but I found no first-party Unity case study of agent-built levels.

### Cited Findings
- **Claude Code driving Godot without opening the editor** (2026-03-04):
  - Claude edits text scenes and launches via CLI.
  - Each test step is a standalone script with `run()` returning pass/fail. A JSON manifest chains steps. `fast_*` steps set state through API shortcuts, and `ui_*` steps simulate real input.
  - A live command queue (`.cmd-queue` / `.cmd-result` files) drives a running game.
  - Each step saves a timestamped screenshot, and Claude reads the JSONL/Markdown traces and screenshots to diagnose failures. The loop is "self-correcting".
  - A shared grid overlay with named cells was "the piece that made CLI-driven layout viable". Vague wording like "nudge it a bit left" caused repeated back-and-forth.
  - Long step chains did not always run cleanly.
  - No hooks were used. The author suggests a `PostToolUse` hook for live reload. Skills like `/client` launch the game or run tests.
  - The article covers UI layout, not level building.
  - [vivecuervo7 dev blog](https://vivecuervo7.github.io/dev-blog/p/claude-code-godot)
- **Community Claude Code level-design skills exist** (quality unverified):
  - A "team-level" skill in Claude-Code-Game-Studios coordinates level designer and QA agents to write critical-path tests, find sequence breaks and softlocks, and build a playtest checklist. — [openskillindex](https://openskillindex.com/skills/donchitos-claude-code-game-studios-team-level)
  - A level-designer skill says to playtest a grey box before art-dressing it, with before/after screenshots documenting each change. — [agent-skills.md](https://agent-skills.md/skills/prorise-cool/prorise-claude-skills/level-designer)
  - A Unity skill applies C# edits, triggers Play Mode, simulates inputs, and validates through logs, screenshots and profiler data. — [mcpmarket Unity Development Loop](https://mcpmarket.com/tools/skills/unity-development-loop)
- **Text-grid → level is the dominant research representation.** MarioGPT, Todd et al. and Word2World all generate tile/character grids (see §3). — [MarioGPT](https://arxiv.org/pdf/2302.05981), [Word2World](https://arxiv.org/abs/2405.06686)
- **Word2World's decomposition**: story → extract tileset, walkable tiles, goals and important tiles → build the environment, then place characters and key tiles → evaluate with the LLM plus A* → feed results back. It works better than one-shot "make a level" prompts. — [Word2World arXiv](https://arxiv.org/html/2405.06686v1)
- **Vision review of game screenshots is still weak.** GlitchBench (CVPR 2024; 593 real glitches from r/GamePhysics across 205 games) found glitch detection "presents a new challenge" for state-of-the-art multimodal models. This is 2023–24 data and newer models have not been checked here. — [GlitchBench](https://glitchbench.github.io), [CVPR paper](https://openaccess.thecvf.com/content/CVPR2024/html/Taesiri_GlitchBench_Can_Large_Multimodal_Models_Detect_Video_Game_Glitches_CVPR_2024_paper.html)
- On the Unity forum, users report better results when the AI builds editor scripts and custom tooling than when it writes gameplay code. — [Unity Discussions](https://discussions.unity.com/t/cli-mcp-ai-ide-chatgpt-codex-cursor-antigravity-claude-code-windsurf-next-steps-for-unity-workflow-automation/1705679?page=2)

### Inferences
These patterns, mapped onto this project:
- **ASCII round-trip**: an editor method serializes a room prefab's block grid to text (`#` solid, `^` spikes, `o` node, `S` spawn, `>` exit) and parses text back into placed prefab instances. The agent edits text, which is cheap and diffable. The human reviews the diff and a `capture_game_view` PNG.
- **Brief → blockout**: the human writes the design intent (beat, mechanic, target difficulty). The agent proposes 2–3 ASCII variants. The human picks one and the agent instantiates it. Feel stays with the human.
- **Validation**: deterministic C# validators (spawn and exit present, no overlapping colliders, nodes inside grapple range of a reachable position, room bounds match neighbors, a death zone under every pit) run as EditMode tests over all room prefabs. The agent can run and fix these without judging feel.
- **Screenshot review**: useful for gross problems such as missing sprites, wrong layer order or an off-screen camera. GlitchBench shows it is unreliable for subtle issues, so pair it with numeric checks.
- **Scripted playtest**: a PlayMode test that replays a recorded input sequence and asserts that the player reaches the exit and takes no damage. This is the Godot `ui_*` step pattern applied to Unity PlayMode tests the project already has. It is a regression check that a route is still possible after edits, not a feel check.
- **Auto room docs**: the agent generates a per-room doc (ASCII map, mechanics used, node count, screenshot) from the validator output.

### Gaps
- I found no published Unity case study of sketch/image → blockout with Claude, or of LLM prefab placement in a 2D platformer.
- I did not check whether current Claude vision models do better than GlitchBench-era models.

## 3. Research: LLM/PCG level generation and mixed-initiative tools, and lessons for a hand-designed precision platformer

### Takeaway
LLMs can generate tile-grid platformer levels, but playability comes from external checkers: A* agents, physics simulation, or repair passes. It does not come from the LLM itself. MarioGPT reaches 88.4% completable levels only with a fine-tuned model and an A* evaluator. The mixed-initiative tradition (Tanagra, Morai Maker) shows that designers want the AI in different roles, and that tools without a physics model produce unplayable suggestions. For a precision platformer, the most transferable lesson is a reachability/physics validator. Generative output is not.

### Cited Findings
- **Todd et al., "Level Generation Through Large Language Models"** (FDG 2023): LLMs can generate playable Sokoban levels, and performance "scales dramatically with dataset size". Level datasets are scarce, which is a problem for data-hungry models. The paper includes preliminary controllability experiments. — [arXiv 2302.05817](https://arxiv.org/abs/2302.05817v2)
- **MarioGPT** (NeurIPS 2023): fine-tuned GPT-2, text-prompted, combined with novelty search for diversity. 88.4% of 250 generated levels were completable by Baumgarten's A* agent, against ~31% for the best LSTM baseline. The model's predicted path deviated 1.15 tiles from the agent's real path in playable levels and 4.56 tiles in unplayable ones. Code: github.com/shyamsn97/mario-gpt. — [arXiv 2302.05981](https://arxiv.org/pdf/2302.05981), [NeurIPS poster](https://neurips.cc/virtual/2023/poster/71191)
- **Word2World** (Nasir, James, Togelius, 2024): needs no fine-tuning. A multi-step LLM pipeline plus A* and LLM evaluation feedback rounds improves playability and coherence over direct generation in ablations. The repo supports only OpenAI models (gpt-4-turbo-2024-04-09). — [arXiv 2405.06686](https://arxiv.org/abs/2405.06686), [GitHub](https://github.com/umair-nasir14/Word2World)
- **Kid Icarus GPT-2 thesis** (Cal Poly): a fine-tuned GPT-2 generates levels, then a post-hoc repair algorithm makes small changes verified by game physics to close gaps in the solution path. This is the closest work to "LLM + physics-verified platformer". — [Cal Poly thesis](https://digitalcommons.calpoly.edu/theses/3377)
- **Tanagra** (Smith, Whitehead, Mateas; AIIDE 2010):
  - It uses reactive planning plus constraint programming.
  - A player-movement model and geometry constraints keep every level playable.
  - The designer places or pins geometry and edits pacing. The system fills the rest, or reports that no level meets the requirements.
  - It is described as the first AI-assisted level design tool for 2D platformers.
  - It builds on rhythm-based generation (FDG 2009): a first tier generates a rhythm and a second tier generates geometry from it, so the intended rhythm survives whatever geometry is chosen.
  - [AAAI AIIDE](https://ojs.aaai.org/index.php/AIIDE/article/view/12379), [Smith dissertation](https://escholarship.org/uc/item/0fn558gq), [Rhythm FDG09](https://expressiveintelligence.github.io/papers/smith-fdg-09.pdf)
- **Morai Maker** (Guzdial, Riedl et al.): a turn-taking co-creative Mario editor in which the AI's additions can be kept or erased and the AI adapts through RL. None of its four AI agents had an explicit physics model, so its suggestions were not guaranteed playable. A later demo added A* pathing and challenge estimation. — [arXiv 1809.09420](https://arxiv.org/pdf/1809.09420.pdf), [arXiv 1911.09219](https://www.arxiv.org/abs/1911.09219)
- **"Friend, Collaborator, Student, Manager"** (Guzdial et al. 2019): designers vary in the role they want the AI to play, and they adapt their own practice to the tool. — [arXiv 1901.06417](https://arxiv.org/abs/1901.06417)
- **Integrated framework for AI-assisted 2D platformer design** (2018): checks whether jump trajectories can reach target platforms. — [arXiv 1804.09153](https://arxiv.org/pdf/1804.09153)
- **HDPCG** (arXiv, Feb 2026): reachability validation over time-expanded graphs with A* and DP. It notes that mixed-initiative systems like Tanagra link layout to pacing and jump physics. — [arXiv 2602.18943](https://www.arxiv.org/pdf/2602.18943)
- **Surveys**: Maleki & Zhao, "PCG in Games: A Survey with Insights on Emerging LLM Integration" (AIIDE 2024). Gallotta et al., "LLMs and Games: A Survey and Roadmap" (2024). — [arXiv 2410.15644](https://arxiv.org/pdf/2410.15644), [arXiv 2402.18659](https://arxiv.org/pdf/2402.18659)

### Inferences
Lessons that transfer to a hand-designed precision platformer:
1. **Playability is a checker's job, not the generator's.** Every successful LLM generator relies on A*, physics or repair. This project needs a reachability validator built from `PlayerStatsSO` (jump arc, dash distance, grapple range, wall stamina), as in Tanagra's movement model or a time-expanded graph, before trusting any AI-proposed layout.
2. **Two-tier rhythm → geometry** (Smith 2009) matches the GDD-first workflow. The human authors the beat/rhythm (the "rhythm tier") and the agent proposes geometry to fill it, with constraints the designer pinned.
3. **Pin-and-fill (Tanagra)** is the right human-in-the-loop shape. The human pins critical blocks and nodes, and the agent may only touch unpinned cells. Report "unsatisfiable" instead of silently breaking a constraint.
4. **Designer-chosen AI role.** Use the agent as validator/QA (manager/student role) by default and as a generator only on request.
5. **Data scarcity.** A handful of hand-made rooms cannot fine-tune anything. Prompting with ASCII examples of existing rooms plus validator feedback (Word2World-style iteration) is the only realistic generative mode.
6. Generative PCG targets variety, but precision platformers (Celeste-like) depend on authored, tuned challenges. Expect AI value mainly in blockout speed, validation, documentation and regression playtests, not in final layouts.

### Gaps
- Sentient Sketchbook was not researched in this pass.
- "Practical PCG Through Large Language Models" (arXiv 2305.18243) showed up in search but was not read.
- I found no 2025–2026 paper on frontier-LLM (Claude/GPT-class, no fine-tune) generation of precision-platformer levels with physics validation.

## 4. Limitations, failure modes and human-in-the-loop patterns

### Takeaway
The documented failure modes are weak spatial precision from vague instructions, no internal physics model (so suggestions are unplayable), unreliable visual glitch detection, long action chains that break, and token/credit cost. The recommended mitigations are discrete shared coordinates, deterministic external validators, short verified loops, pin-and-fill constraints, and human sign-off on feel.

### Cited Findings
- Vague spatial instructions cause back-and-forth, and a named grid overlay fixes it. Long step chains do not always run end to end. — [vivecuervo7](https://vivecuervo7.github.io/dev-blog/p/claude-code-godot)
- AI co-creators without a physics model produce non-guaranteed-playable content. — [Morai Maker, arXiv 1809.09420](https://arxiv.org/pdf/1809.09420.pdf)
- Unplayable MarioGPT levels show a much larger predicted-vs-actual path error (4.56 vs 1.15 tiles), so the model's own path is a weak playability signal. — [MarioGPT](https://arxiv.org/pdf/2302.05981)
- Multimodal models struggle to detect game glitches. — [GlitchBench](https://glitchbench.github.io)
- Direct one-shot prompt-to-level tends to fail. Decomposition plus feedback rounds improves results. — [Word2World](https://arxiv.org/html/2405.06686v1)
- "Scene-level AI features are still basic". Opus is credit-hungry, and splitting Opus for planning and Sonnet for implementation is common. — [Unity Discussions](https://discussions.unity.com/t/cli-mcp-ai-ide-chatgpt-codex-cursor-antigravity-claude-code-windsurf-next-steps-for-unity-workflow-automation/1705679?page=2)
- Unity's AI tools are beta and may change or be discontinued. — [Unity blog](https://unity.com/blog/unity-ai-mcp-how-to-get-started)

### Inferences
- **Recommended loop**: human brief → agent ASCII proposal → validator (EditMode) → instantiate via `eval` → `capture_game_view` → human diff review → human gamepad playtest → commit. The agent never declares a room "fun" or "fair".
- Numbers beat pixels. Validators should report numeric margins (gap width vs. max jump, node distance vs. grapple range) rather than rely on screenshot interpretation.

### Gaps
- I found no quantitative 2025–26 benchmark of frontier-LLM spatial accuracy on ASCII game maps.

## 5. Claude Code specifics: hooks, skills, subagents, slash commands for level work

### Takeaway
The current Claude Code hook system has blocking `PreToolUse`, `Stop`/`SubagentStop`, `TaskCompleted` and `PostToolBatch` events. Matchers support regex over tool names, including MCP tools. Skills and subagents can carry their own frontmatter hooks. This is enough for a `/new-room` skill with a scoped validator hook, and for a QA subagent that must pass room validation before it can finish.

### Cited Findings
- **Hook events**: `SessionStart`, `UserPromptSubmit`, `PreToolUse`, `PermissionRequest`, `PostToolUse`, `PostToolUseFailure`, `PostToolBatch`, `Stop`, `SubagentStart`, `SubagentStop`, `TaskCreated`, `TaskCompleted`, `PreCompact`, `FileChanged` and others. — [Claude Code hooks docs](https://code.claude.com/docs/en/hooks)
- **Blocking**: exit code 2 blocks on `PreToolUse`, `Stop`, `SubagentStop`, `TaskCompleted`, `PostToolBatch`, `UserPromptSubmit` and others. `PostToolUse` cannot block because the tool already ran, but its stderr goes back to Claude. — [hooks docs](https://code.claude.com/docs/en/hooks)
- **Matchers**: exact names or `|` lists. Anything else is treated as an unanchored JS regex. MCP tools are named `mcp__<server>__<tool>`, matched with e.g. `mcp__memory__.*`. An `if` field takes permission-rule syntax such as `"Bash(git *)"`. — [hooks docs](https://code.claude.com/docs/en/hooks)
- **Skill and subagent hooks**:
  - Skills and subagents can define hooks in YAML frontmatter.
  - Subagent hooks run only while the subagent is active, and a `Stop` hook in a subagent becomes `SubagentStop`.
  - Skill hooks stay registered for the rest of the session after invocation, and `once: true` removes a hook after its first successful run.
  - [hooks docs](https://code.claude.com/docs/en/hooks)
- Community: Claude Code skills like `/client` launch the game or run tests by intent, and `.claude/rules/` gave conventions to up to five parallel background agents. — [vivecuervo7](https://vivecuervo7.github.io/dev-blog/p/claude-code-godot)
- Counterpoint: one practitioner removed about 90% of their skills because they added complexity. — [Unity Discussions](https://discussions.unity.com/t/cli-mcp-ai-ide-chatgpt-codex-cursor-antigravity-claude-code-windsurf-next-steps-for-unity-workflow-automation/1705679?page=2)

### Inferences
Concrete options for this project:
- **`/new-room` skill**: takes the brief, reads `docs/GDD.md` and existing room ASCII as examples, and proposes ASCII. After human approval it instantiates a prefab variant via an `eval`-called editor tool, then captures a screenshot. Its frontmatter carries a `Stop` hook that runs the room validator tests.
- **Room validator hook**: add room-validation EditMode tests to the existing `Stop` hook (`.claude/hooks/check-compile.sh` already runs EditMode), or attach them to `TaskCompleted`.
- **`PostToolUse` on `Bash(unity command eval*)`**: auto-capture a game-view screenshot or dump ASCII after scene edits, so the human always gets a visual diff. This hook cannot block.
- **Level-QA subagent**: read-only access plus `run_tests` and `capture_game_view`. It lists violations and margins and never edits. This fits the existing "Opus plans, Sonnet implements" memory rule.
- Keep the skill count small, given the forum feedback.

### Gaps
- I did not fetch the Claude Code skills and subagents docs pages. Skill frontmatter fields beyond hooks, such as `allowed-tools` and model selection, were not verified here.
