# Reachability Visualization, Automated Playability Checks and Playtest Analytics for a Grapple/Swing 2D Platformer

Scope note: target is a solo dev on Unity 6 (6000.3) with a kinematic Rigidbody2D FSM controller (`PlayerController` + `KinematicPhysicsHandler`) and all tuning in `PlayerStatsSO`. Sources were fetched 2026-11-18. Several items are marked as inference where no source covered grapple/swing directly.

## 1. Jump-arc / movement-envelope visualization from physics parameters

### Takeaway
The proven pattern is to put an editor gizmo in the level that runs the same jump simulation as the game, with no Play Mode, and redraw whenever tuning changes. Psychonauts 2 shipped exactly this. Closed-form kinematics (v0 = 2h/t_peak, g = 2h/t_peak²) cover simple jumps. A controller with phases, dash, wall stamina and grapples needs a step-by-step forward simulation at fixed dt instead.

### Cited Findings
- Double Fine's Psychonauts 2 "Jump Metrics Tool" is an object placed in the level that "runs the jump simulation code at edit time, while the game is not running" and draws Raz's arc, so designers can place geometry within reach of a given jump type. Changes to jump tuning update the preview arc live. The image shows single-jump and double-jump previews side by side. — [Double Fine: Behind the Code, Designing Raz's Jump](https://www.doublefine.com/news/devin-article-raz-jump)
- Architecture detail that made this possible: all jump functions operate on a `P2JumpState` struct rather than the character. The editor sim had no character instance, so it used a separate editor velocity field. The jump is split into independently tunable phases (Launch 1560 cm/s at 1.45x gravity, Float A/B at 0.26x gravity, Fall at 1.50x). Early button release can cut Launch (min 0.05 s) and Float A (min 0.09 s) short, so the tool must show both min and max arcs. — [Double Fine](https://www.doublefine.com/news/devin-article-raz-jump)
- Design warning from the same article: adding moves that go higher or farther invalidates existing reach assumptions and forces layout rework. That is the main argument for automated reach checks after tuning changes. — [Double Fine](https://www.doublefine.com/news/devin-article-raz-jump)
- Closed-form jump design from height/time: jump speed `v = 2h / t_peak`, rising gravity `g_up = 2h / t_peak²`, falling gravity `g_fall = 2h / t_descent²`, horizontal speed `vx = d / (t_peak + t_descent)` (signs per axis convention). — [GDQuest: Kinematic Jump Formulas](https://www.gdquest.com/library/kinematic_jump_formulas)
- Kyle Pittman's GDC 2016 talk "Building a Better Jump" (Math for Game Programmers) derives jump gravity and velocity from designer-facing constraints (gap distance, peak height) and covers separate ascent/descent gravity. Secondary summaries only; the slides/video are on GDC Vault. — [GDC Vault 1023148](https://gdcvault.com/play/1023148/contactUs); [JCCC GDC16 blog summary](https://blogs.jccc.edu/dpatter/2016/03/15)
- Comparable off-the-shelf tools: the GDevelop "Platformer trajectory" extension configures jump height and draws trajectories to check a level design (experimental). Dial-A-Platformer defines arcs from height and distance. — [GDevelop wiki](https://wiki.gdevelop.io/gdevelop5/extensions/platformer-trajectory/); [Dial-A-Platformer](https://geokureli.itch.io/dial-a-platformer)
- Unity can step 2D physics outside Play Mode: `PhysicsScene2D.Simulate` works in edit mode (Rigidbody2D, Collider2D, Joint2D, Effector2D simulated, contacts generated, script callbacks not called). It moves Rigidbody2D objects, so use a separate physics scene. Pass `Time.fixedDeltaTime` per step for determinism. — [Unity 6000.3 docs: PhysicsScene2D.Simulate](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/physicsscene2d/simulate)
- Unity's multi-scene physics manual names trajectory prediction as a use case: create a local physics scene (`LocalPhysicsMode.Physics2D`), copy colliders in, step manually. Community gist example: [Unity manual: Multi-scene physics](https://docs.unity3d.com/2022.2/Documentation/Manual/physics-multi-scene.html); [gist](https://gist.github.com/biggmackk17/63b95a6a565191f14b61094397a4ac74)
- The McGill "I Can Jump!" Unity tool (2014) had to write its own simplified physics simulator because the full built-in physics was not available in edit mode. It applied gravity until a platform collision, with limited collision resolution. — [Tremblay, Borodovski, Verbrugge, AIIDE 2014 workshop, "I Can Jump!"](https://www.cs.mcgill.ca/~jtremb59/Papers/ICanJump.pdf)

### Inferences
- For RockTheGame the Psychonauts recipe maps directly. Refactor (or wrap) the pure velocity math of `NormalState` / `DashState` / `SwingingState` so it can step a plain struct (position, velocity, timers, stamina) from `PlayerStatsSO` without a MonoBehaviour. Then draw it with `Handles.DrawAAPolyLine` in an `[ExecuteAlways]`/`OnDrawGizmosSelected` component, or a custom Editor. Because the controller is kinematic and resolves collisions itself (casts rather than dynamic bodies), collision queries like `Physics2D.BoxCast` work in edit mode without `Simulate`. The 2014 limitation no longer applies if the resolver can be called with an injected state.
- Envelope = union of arcs over the input space. Sample a small discrete set at a fixed dt of 1/50 or 1/60 (hold-jump 0..max frames, stick left/neutral/right, dash in 8 directions at frame k, wall-jump), draw min/max-hold arcs, and fill the hull as the "reach envelope" polygon. Colour by required precision (how many sampled input sequences reach a cell). That is a cheap leniency proxy.
- Grapple/swing envelope (standard pendulum physics, not from a source): with rope length L, attach speed v and gravity g, energy conservation caps the swing apex at Δh = v²/(2g) above the current height. Max height is the anchor height + L if the rope stays taut, and the rope goes slack when centripetal demand v²/L < g·cosθ component. Draw (a) the grapple range circle around each node from `PlayerStatsSO`, (b) the circle of radius L at attach time, (c) release trajectories sampled every N degrees along the arc as ballistic parabolas using the tangential velocity. The union of release parabolas is the swing's reach envelope. The engine's actual swing integrator (not the ideal formula) must generate these, because a kinematic swing with damping or speed caps (e.g. AirOverspeedReduce) will differ from the textbook.
- Also draw: wall-climb max height = climb speed × stamina seconds (GDD band ~6 s), and dash reach = dash speed × dash duration plus post-dash air carry.

### Gaps
- No Unity-specific open-source "jump arc gizmo" for a custom kinematic FSM was found. The Psychonauts article gives no code.
- The exact Pittman equations were not confirmed from primary slides (GDC Vault page was login-limited).

## 2. Automated playability checks (search over movement primitives, bots, reachability graphs)

### Takeaway
The standard approach is search over discretized action sequences, with the game's own physics used as the forward model. Variants: Baumgarten A* for Mario, and A*/MCTS/RRT in a Unity tool (Tremblay et al.). Rhythm/constraint generators (Launchpad, Tanagra) instead guarantee playability by building geometry from a physics model of jump capabilities. For grapple/swing, nobody has published a planner. The practical method is to treat "attach to node N, release at angle θ" as macro-actions and simulate them forward.

### Cited Findings
- Robin Baumgarten's A* agent won the 2009 Mario AI Competition (CIG). It used a physics simulator to project the resulting positions of each action, i.e., it searched in simulated future states. It lost its edge in later years when levels needed backtracking. Code is in the Mario AI Framework (`robinBaumgarten` agent); an archived clone exists. — [VenusPatrol](https://venuspatrol.com/2009/09/a-better-mario-built-robin-bau); [GitHub clone](https://www.github.com/jumoel/mario-astar-robinbaumgarten); [Mario AI Framework listing](https://awesome.ecosyste.ms/projects/github.com%2Famidos2006%2FMario-AI-Framework)
- Nick Jonas's blog walks through an A* implementation for MarioAI (practical write-up). — [A* for MarioAI](https://jonasnick.github.io/blog/2012/12/20/a-star-for-marioai)
- "I Can Jump!" (Tremblay, Borodovski, Verbrugge, McGill, AIIDE 2014): open-source Unity tool comparing A*, MCTS and RRT for platformer level testing. Action set A = {jump, jump-left, jump-right, left, right, wait}, each applied for a fixed duration t (search granularity), so A* is not optimal. RRT samples random points in free space, finds the nearest tree node, and uses A* or MCTS as the local motion planner to connect them. The tree, sampled states and found path are drawn as a debug view. MCTS used UCT with a reward of inverse-square distance to goal, penalties for death and a visit-count grid to avoid over-searching. Findings: A* fast on simple levels but fails when time (moving hazards) is added as a dimension. RRT is non-optimal but surfaces alternative solutions, which the authors frame as useful for designers. Their previous work showed aggregate heatmaps of many solution paths. — [I Can Jump! PDF](https://www.cs.mcgill.ca/~jtremb59/Papers/ICanJump.pdf)
- Same paper cites Bauer, Cooper & Popović 2013 (RRT reachable-state exploration with parabolic jumps) and Sturtevant 2013 (BFS over all solutions for the puzzle game Fling!). — [I Can Jump! PDF](https://www.cs.mcgill.ca/~jtremb59/Papers/ICanJump.pdf)
- Launchpad (Smith, Treanor, Whitehead, Mateas, FDG 2009): two-tier grammar. A rhythm (beats = move/jump/wait, with length 5/10/15/20 s, density low/med/high, beat type regular/swing/random) is generated, then geometry is chosen to fit it. A ballistic physics model (avatar size, max speed, initial jump velocity, jump height for short/medium/long press, air time per jump type, spring velocity) constrains geometry "ensuring that all generated levels are playable". Double-jumps and wall-jumps were explicitly not supported. — [Rhythm-Based Level Generation for 2D Platformers (PDF)](https://expressiveintelligence.github.io/papers/smith-fdg-09.pdf)
- Tanagra (Smith, Whitehead, Mateas 2010) is a mixed-initiative level design assistant for 2D platformers built on the same rhythm/beat model; the designer edits and the system fills in geometry so the level stays playable. — [researchr: Tanagra](https://researchr.org/publication/SmithWM10) (abstract-level only; full paper not fetched)
- Platformer A* on a grid can be extended with extra movement abilities (ledge grabbing) as new edge types, with changes to both the search and the bot. A useful template for adding a "grapple" edge. — [Tuts+: A* Pathfinding for 2D Grid-Based Platformers: Ledge Grabbing](https://code.tutsplus.com/a-pathfinding-for-2d-grid-based-platformers-ledge-grabbing--cms-25456t)
- Search for published bots/planners that plan pendulum swings found nothing specific. — (negative result; see Gaps)

### Inferences
- Recommended architecture for RockTheGame, cheapest first:
  1. **Reachability graph over "stand points" + nodes (offline, editor button).** Vertices = floor segments, wall segments and grapple nodes in a room. For each vertex, run the forward simulator from item 1 over a small macro-action set: run-jump with hold 0..max in K steps × stick directions; dash in 8 directions at apex or at takeoff; wall-jump; grapple-attach to each node within `GrappleRange` with line of sight, then release every 10–15° of swing; slingshot. Every landing on another vertex adds a directed edge. BFS from the room entrance tells whether every exit, collectible and key is reachable. Draw edges as coloured arcs in the scene view, red for unreachable targets. This is Launchpad's "physics model guarantees playability" idea inverted into a checker, with the Baumgarten/Tremblay forward simulation as the edge generator.
  2. **A* bot with frame-level actions** only for specific hard sections, using the same simulator. Heuristic = distance / max speed. Discretize state (position bucket ~0.25 units, velocity bucket, grounded/wall/swing flag, dash available, stamina bucket) for the closed set. Expect a state-space blowup from swing angle × angular velocity × dash × stamina, which is why the macro-action graph comes first.
  3. **RRT for exploring alternative/cheese routes**, matching Tremblay et al.'s finding that non-optimal diverse paths reveal unintended shortcuts.
- Edge weights can carry difficulty: the number of input sequences (out of all sampled) that realize an edge is a direct "precision/leniency" score per edge (see item 5).
- Run it as an EditMode test: "room X: all exits reachable from entrance". After a `PlayerStatsSO` change, the test flags rooms that broke. That catches the Psychonauts "new move invalidates layouts" problem automatically, and also catches the reverse case where a tuning change opens unintended shortcuts (new edges that skip a designed beat).
- Determinism caveat: the simulator must call the same velocity/state code as runtime. If it is a reimplementation, it will drift from the real controller. Make the controller's step function pure (state in, state out) and use it in both.

### Gaps
- No published work found on automated playability checking with grappling hooks, pendulum swing or rope physics. Treating attach/release as macro-actions is my own suggestion.
- Tanagra and Mario AI Framework internals were not read in full.

## 3. Input recording & deterministic replay for regression tests

### Takeaway
Celeste TAS and Trackmania both work because replays store only inputs per fixed simulation frame, and the simulation is a deterministic function of state + input. In Unity: record per-`FixedUpdate`-tick inputs from the FSM's input layer (not wall-clock), replay them in PlayMode tests, and assert end positions/room reached with a tolerance. Determinism holds on the same machine/build, not reliably across CPUs.

### Cited Findings
- CelesteTAS `.tas` format is plain text, one line per input span: `(Frames),(Actions)`, e.g. `123,R,J` = hold Right+Jump for 123 frames. The analog stick is an integer angle 0–359 with optional magnitude. `***` is a breakpoint. `Read,File,StartLine` includes other files. — [EverestAPI CelesteTAS via search summary](https://github.imc.re/EverestAPI/CelesteTAS-EverestInterop); [TASVideos CelesteTAS resource](https://tasvideos.org/EmulatorResources/CelesteTAS); [original tbodt/celestetas](https://github.com/tbodt/celestetas)
- The CelesteTAS ecosystem bundles: Celeste Studio (editor), savestates via the Speedrun Tool mod, fixed-framerate video via TAS Recorder, GhostModForTas to compare TASes, Radeline Simulator (brute-forces inputs to reach a target position), Radeline Optimizer (mutates inputs to optimize), Featherline (genetic algorithm for feather movement). — [CelesteTAS-EverestInterop GitHub](https://github.com/EverestAPI/CelesteTAS-EverestInterop)
- TASVideos recommends `EnforceLegal` at the top of a file so debug/cheat commands aren't executed. Converting CelesteTAS analog angles to libTAS's raw 16-bit X/Y uses floats, so small analog differences can creep in. — [TASVideos CelesteTAS](https://tasvideos.org/EmulatorResources/CelesteTAS)
- Trackmania replay files store all inputs and can be replayed and analysed in-game (rediscovered by donadigo in 2019). TASVideos describes the format as "only consists of inputs and is fully deterministic", and TMInterface is the TAS tool for TMNF/TMU. One 2025 Reddit commenter claimed positions are stored; that claim was rebutted in-thread. — [donadigo: TMX Replay Investigation](https://hn.jakechampion.name/reader/https://donadigo.com/tmx1); [TASVideos Trackmania forum](https://tasvideos.org/Forum/Topics/22766)
- Unity Input System has `InputEventTrace` (capture, write to/read from disk, `Replay()` returning a `ReplayController` that can preserve original timing, feeding events in `onBeforeUpdate`, one frame's events per update). There is also an `InputRecorder` MonoBehaviour sample in Package Manager. — [Unity Input System 1.2: ReplayController](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.LowLevel.InputEventTrace.ReplayController.html); [InputRecorder](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.1/api/UnityEngine.InputSystem.InputRecorder.html); [Input events manual](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Events.html)
- Unity physics determinism: Enhanced Determinism gives the same results between runs on the same machine but does not guarantee cross-device/platform determinism. — [Unity Discussions: Intel vs AMD determinism](https://discussions.unity.com/t/floating-point-determinism-in-unity-intel-vs-amd/813566); [Unity Discussions: Physics.Simulate reproduction](https://discussions.unity.com/t/struggles-reproducing-physics-simulation-with-physics-simulate/918136)
- Community guidance (non-authoritative): key inputs on the fixed-step frame index, not time. The Input System's "Fixed" update mode loses events between fixed steps. Put a replay format version / build hash in the header so CI rejects stale replays. A diverging replay signals a logic change. — [Bugnet: making a simulation deterministic for replays](https://bugnet.io/blog/how-to-make-a-game-simulation-deterministic-for-replays); [Bugnet: Input System update mode vs physics tick](https://bugnet.io/blog/fix-unity-input-system-update-mode-conflict-with-physics-tick); [Tessl: engine record/replay APIs](https://tessl.io/registry/testland/gameplay-recording-replay/files/references/engine-record-replay-apis.md)
- Game Developer postmortem on implementing a replay system in Unity (lessons learned). — [Game Developer: Implementing a replay system in Unity](https://gamedeveloper.com/programming/implementing-a-replay-system-in-unity-and-how-i-d-do-it-differently-next-time)

### Inferences
- Record at the FSM's input boundary rather than raw `InputEventTrace`. Each `FixedUpdate` tick, write the already-buffered logical input the states consume (move vector quantized to e.g. 1/127, jump held/pressed, dash pressed, left/right grapple pressed/held, target node id chosen by `NodeDetector`). That avoids Update-vs-FixedUpdate event timing issues and makes replay independent of `NodeDetector` scoring changes, if the chosen target is stored. Store a CelesteTAS-style run-length text format (`frames,flags,angle`) so humans can diff and hand-edit replays.
- Because the controller is kinematic and does its own resolution, it is far more deterministic than dynamic PhysX/Box2D bodies. Main risks: `Time.deltaTime` used anywhere in states, DOTween/UniTask timers driving gameplay, `Random` without a seed, hitstop (`HitStopEvents`) that pauses on wall-clock. Hitstop should count fixed ticks during replay.
- Regression test shape (fits existing `Assets/Tests/PlayMode/`): load room prefab, place player at recorded start, feed N ticks, assert final position within ε, expected room reached, no death, plus optionally a checkpoint trace of position every 30 ticks. After a `PlayerStatsSO` tuning change, a failing replay shows which recorded routes broke. This is expected for tuning, so the test is a report to review, not a hard gate. Store the stats-SO hash in the replay header so a stats change marks replays "stale" rather than "failed".
- Ghosts: the same recording gives a free ghost overlay (render a translucent sprite following the recorded position trace), like Super Meat Boy / Trackmania ghosts.

### Gaps
- No primary source on how Super Meat Boy stores its replays (inputs vs. positions). Only that it shows all attempts simultaneously (see item 4).
- The current Input System (1.8+) `InputEventTrace` full page was not fetched. Check the version in the project.

## 4. Ghost/trajectory overlays, death heatmaps, per-room timers, telemetry

### Takeaway
For a solo dev, a custom local logger (CSV/JSON per session: room enter/exit timestamps, death position + cause, sampled position trace at ~10 Hz) plus an editor window that splats deaths into a heatmap over the room is cheaper and more useful than a SaaS analytics SDK. Birk et al.'s DPLI metric flags problem rooms from as few as 5 players. Super Meat Boy's "all attempts at once" replay is the classic aggregated-path visualization.

### Cited Findings
- Super Meat Boy: after clearing a level, the replay plays all attempts simultaneously (one reviewer describes ~100 Meat Boys each reenacting one attempt). The developers framed it as turning death into a reward. At launch, boss levels lacked replays. — [PC Gamer review](https://www.pcgamer.com/super-meat-boy-review/); [Wikipedia: Super Meat Boy](https://wikipedia.com/wiki/Meat_Boy); [Kotaku visual guide](https://kotaku.com/a-visual-guide-to-super-meat-boy-5674536); [Steam discussion](https://steamcommunity.com/app/40800/discussions/0/343786745996446746)
- Birk, Lürig, Mandryk (AcademicMindTrek 2015, "A Metric for Automatically Flagging Problem Levels in Games from Prototype Walkthrough Data"), built with indie Team Stendec on *Angus Hates Aliens* using walkthroughs from only five players:
  - Death positions counted per pixel, then low-pass filtered (13×13 kernel, σ = 2, "splatting").
  - Total Deaths (TD); Maximum Death Density (MDD) = max of filtered array; Relative MDD = MDD / TD (concentration).
  - **DPLI = RMDD × √TD**, combining concentration and magnitude, with the square root for non-linear effects of total deaths.
  - DPLI correlated negatively with per-level enjoyment (r = .70 for the fit Enjoyment = 4.62 − 29.4·DPLI as printed). Authors conclude it is "not about the magnitude of deaths, but when and how they occur".
  - Cites Valve's Half-Life 2 death maps from 4M+ sessions, and Drachen & Canossa's death-cause-diversity maps.
  — [Hochschule Trier Informatik-Bericht 2015-01 (PDF)](https://www.hochschule-trier.de/fileadmin/Hauptcampus/Fachbereich_Informatik/Forschung/Informatik_Berichte/2015/Informatik-Bericht-2015-01.pdf)
- Tremblay et al. previously visualized aggregate heatmaps of many simulated solution paths (stealth games). The same visualization works for bot-generated paths. — [I Can Jump! PDF](https://www.cs.mcgill.ca/~jtremb59/Papers/ICanJump.pdf)
- Celeste shows per-chapter death counts as a "badge of honor". No published per-screen developer death heatmaps for Celeste were found. — [search summary of GameSpot/Vice coverage](https://www.gamespot.com/articles/celeste-dev-explains-how-they-made-their-game-feel/1100-6474775/)

### Inferences
- Minimal telemetry schema (append-only JSONL per session, written to `Application.persistentDataPath`):
  - `room_enter {t, room, from}`, `room_exit {t, room, to, duration}`
  - `death {t, room, pos, vel, state, cause}` (cause from `DeathZone`/`AcidHazard`)
  - `respawn {t, checkpoint}`
  - `trace {t, pos, state}` at 10 Hz
  - `stats_hash`, build id
  Hook into existing signals: `PlayerHealth.OnDeath`, `RoomManager` current-room switches, `TransformEventChannelSO` respawns. A dedicated `TelemetryEventChannelSO` would match the project's channel-SO pattern.
- Editor visualization: an EditorWindow/Scene GUI that loads all session files, groups by room, draws (a) a death heatmap (Gaussian splat into a texture aligned to room bounds, σ ≈ player width), (b) traces as faint polylines (SMB-style aggregate), (c) per-room table with median/90th percentile time, deaths, DPLI, sorted by DPLI. Colour deaths by FSM state at death (e.g. "died in SwingingState") to separate grapple problems from jump problems.
- Ghost overlay in-game: replay the `trace` of the player's best or most recent attempt as a translucent sprite (Trackmania ghost) for self-testing tuning changes.
- SaaS options (Unity Analytics custom events, GameAnalytics heatmaps) exist but were not researched in depth here. For a gamepad-only single-player game in development, local files are simpler and keep playtester data private.

### Gaps
- No primary source found for how Celeste or Super Meat Boy used aggregated death data during development.
- Unity Analytics / GameAnalytics heatmap features not verified for Unity 6 in this pass.

## 5. Difficulty metrics for precision platformers (rhythm groups, frame windows, leniency, tolerance)

### Takeaway
Academic metrics (Launchpad leniency/linearity, rhythm density) are coarse, component-based scores. For precision platformers, a better per-obstacle metric is the size of the successful input window: how many frames/positions/angles out of the sampled input space succeed. The same forward simulator gives this, and coyote time and jump buffering widen it measurably.

### Cited Findings
- Rhythm groups are short, non-overlapping level sections encapsulating a rhythm (e.g., three short hops). Parameters: length (5/10/15/20 s), density (low/medium/high = number of actions), beat type (regular, swing, random). Complete levels chain rhythm groups with rest-area platforms. — [Smith et al. FDG 2009 PDF](https://expressiveintelligence.github.io/papers/smith-fdg-09.pdf)
- Launchpad leniency: per-beat component scores, +1.0 gaps/enemies/falls, +0.5 springs/stompers, −0.5 moving platforms, −1.0 jumps with no gap (higher = less lenient, as defined by the authors). Linearity: linear regression over platform midpoints, mean absolute deviation, normalized to [0,1] (rarely > 0.7). Used for expressive-range hex-bin plots. — [Smith & Whitehead, "Analyzing the Expressive Range of a Level Generator", PCG workshop 2010 (PDF)](https://www.pcgworkshop.com/archive/smith2010analyzing.pdf)
- Coyote time in Celeste: the community wiki states 5 frames after leaving a ledge. Another low-reliability article claims 6 frames (0.1 s). Not confirmed from source code. — [Celeste Wiki (fandom)](https://celestegame.fandom.com/wiki/File:Wave_hyper.gif); [Gigazine summary of Thorson's "game feel" thread](https://gigazine.net/gsc_news/en/20200317-celeste-action-feel-things); [Blizzard Watch coverage](https://blizzardwatch.com/2020/03/13/celeste-developer-game-feel/)
- Tremblay et al. used per-action duration t as search granularity; coarser t misses narrow solutions, which implicitly measures how narrow a solution window is. — [I Can Jump! PDF](https://www.cs.mcgill.ca/~jtremb59/Papers/ICanJump.pdf)

### Inferences
- Per-edge "precision" metric from the reachability graph in item 2: for each edge (A→B), sweep takeoff frame offset (−n..+n ticks around the ideal), hold duration, stick angle and, for grapple, release angle (1° steps). Record the size of the success region in each dimension, e.g. "takeoff window 4 ticks, release window 7°". A room's difficulty = product/min of windows along the intended path. That metric suits leniency features directly: report windows with and without coyote time / jump buffer / grapple-assist, to show how much each forgiveness value buys.
- Rhythm-group view: annotate the intended path with a timeline of beats (jump, dash, attach, release, wall-kick) and the time gaps between them. Very short gaps (< ~150 ms) or many consecutive tight windows flag a section as high-demand. Thresholds are a designer judgment, not sourced.
- Combine with telemetry: if DPLI is high in a section whose computed windows are wide, the problem is readability/telegraphing, not execution precision. If windows are tight and DPLI high, widen them via `PlayerStatsSO` (game-feel rule: human playtests).

### Gaps
- No published difficulty metric found that targets grapple/swing release windows.
- Celeste's actual coyote/buffer constants not verified from primary source.

## 6. Physics edge cases specific to grapple/swing and visualizing swing arcs

### Takeaway
Little published material exists. Engine forums document common failures: velocity blowup from naive dot-product swing forces, and collision not firing while position-constrained. Visualization should show the actual simulated arc (from the runtime integrator), rope-clip points and slack zones, not just the ideal circle.

### Cited Findings
- A naive grapple implementation using the dot product of grapple-to-character vector and velocity gave pendulum-like motion, but velocity grew exponentially and the swing broke. — [search summary: Unreal dev report, itch.io devlog](https://itch.io/devlog/481449/grappling-hook.amp)
- GDevelop's built-in platformer behaviour doesn't interface with position-based rope swinging, so collisions don't fire while swinging. — [GDevelop forum: Ninja Rope](https://forum.gdevelop.io/t/ninja-rope-pendulum-swinging-with-platformer-behavior/22310)
- Joint-based approach (Unity DistanceJoint2D between player and anchor), including rope wrapping around corners by adding intermediate anchor points on raycast hits. — [Kodeco/raywenderlich: Make a 2D Grappling Hook Game in Unity](https://www.raywenderlich.com/348-make-a-2d-grappling-hook-game-in-unity-part-1?page=3)
- Validating a grapple target with a collision cast (thin box/ray to the anchor) is the standard line-of-sight check. Informal. — [Godot forum: 2D grappling hook](https://forum.godotengine.org/t/2d-grappling-hook-where-do-i-start/88250)

### Inferences
- Edge cases worth visualizing or testing for this controller (dual-arm, kinematic, instant `Vector2?` anchors):
  - **Energy drift**: kinematic swing integrators gain or lose energy per tick. Test: release-free swing for N seconds should not exceed the starting apex height; plot apex per cycle.
  - **Rope through geometry**: anchors are instant, so the line from player to anchor can pass through tiles mid-swing. Draw the rope line red when a linecast hits.
  - **Collision during swing**: the swing's intended velocity still has to go through `KinematicPhysicsHandler`. Visualize where the simulated arc collides and what happens (stop, slide, detach).
  - **Slack/taut transitions**: when the player is inside radius L (e.g., attaching from above), the rope should be slack. Draw the slack region.
  - **Dual anchors** (`DualSwingingState`): the reachable set is the intersection of two disks. Draw both circles and the lens; the equilibrium point is a useful gizmo.
  - **Release-velocity caps**: speed limits (e.g. AirOverspeedReduce, recently tuned 12→20) clip release parabolas. Draw the capped vs. uncapped trajectory so tuning effects are visible.
  - **Hitstop/room transitions mid-swing**: replays should include them, as noted in the recent spiral fix commit (direction lock on death).
- Swing-arc gizmo: for each grapple node in the room, draw the range circle; for a selected incoming trajectory (e.g. from a ghost replay or a picked jump arc), draw the attach point, the simulated swing for one half-cycle, and release parabolas every 15°, coloured by whether they land on a valid vertex. This gives the designer a "can I get from A to B via this node?" view directly.

### Gaps
- No academic or GDC source found on planning or verifying grapple/swing traversal. Recommendations above are engineering inference from pendulum physics and the cited search-based methods.
