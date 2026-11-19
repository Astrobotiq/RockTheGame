# Level design processes, methods and lightweight tools for a solo room-based precision platformer

Context: Rock The Game, a 2D grappling platformer (dual-arm grapple nodes, dash, wall climb, slingshot, launch pads, breakable platforms, acid). Solo developer working 5-6 h/day in Unity. Notes cover the path from idea to shipped room. Research date: 2026-11-18. ~16 tool calls. Many popular claims about Celeste and Nintendo come from secondary write-ups; this is flagged where it matters.

## 1. Design frameworks (teach-test-twist, kishōtenketsu, Celeste structure, metrics, beat charts, intensity, checkpoints, collectibles)

### Takeaway
The frameworks with the best sources (kishōtenketsu / teach-test-twist, the Portal-style "pile of beats", Valve-style intensity graphs, and metrics "zoos") all come down to the same loop for a room-based platformer: introduce one element safely, test it, twist it or combine it with earlier ones, and space out the twists. Celeste adds short rooms, a checkpoint at each room, instant respawn, and optional collectibles or skips on top of that loop.

### Cited Findings
**Kishōtenketsu / Nintendo 4-step**
- Game Maker's Toolkit's analysis of Super Mario 3D World breaks Nintendo level design into four steps: introduction, development, twist, conclusion — [MCV/Develop](https://www.mcvuk.com/development/video-nintendos-level-design-secrets-in-four-steps)
- Director Koichi Hayashida tied the model to kishōtenketsu, a narrative structure from Asian storytelling — [MCV/Develop](https://mcvuk.com/business-news/publishing/video-nintendos-level-design-secrets-in-four-steps/); [Engadget](https://www.engadget.com/2015/03/17/super-mario-3d-world-design/)
- In 3D Land and 3D World the first ("ki") section introduces the mechanic with little risk. Optional, penalty-free challenges usually sit just before the goal (fan analysis) — [Kitsune blog](https://kitsunethegame.blogspot.com/2016/03/kishotenketsu-nintendo-formula-for.html)
- Most sources here are secondary or fan analyses; no official Nintendo document was found.

**Teach, test, twist (Level Design Book, using Portal as the example)**
- Teach: introduce an activity (Portal chamber 10 teaches the basic fling). Test: the player repeats it with prompting. Twist: change the frame and check whether the player recalls it unprompted (chambers 12, 15 and 18 add taller flings, mid-air double flings and an infinite fling). **Space the twist beats apart rather than putting them right after the test** — [Level Design Book: Pacing](https://book.leveldesignbook.com/process/preproduction/pacing)
- "Pile of beats" method: block out and prototype one isolated puzzle, playtest until it proves promising or fails, repeat until there are dozens of prototypes, then arrange the best ones by shared or contrasting elements. Portal was prototyped as disconnected test chambers and assembled into a campaign later — [Level Design Book: Pacing](https://book.leveldesignbook.com/process/preproduction/pacing)
- Robert Yang (Big Hops, 2026): the team built "beat zoos", large test levels full of short gameplay situations, and kept the strongest ones. Each beat got a short in-world name so the concept stayed clear and feedback was easier to discuss. They built bad prototypes on purpose to learn which parts of the design space to avoid. The best tests were kept for the final game and art-passed — [Radiator blog](https://www.blog.radiator.debacle.us/2026/01/notes-on-level-design-workflow-for-3d.html)
- Big Hops chapter template: a start level, a town hub, 3-4 item types, 3-4 levels introducing each item, then a climax. The team built chapters in reverse play order and credits this with a stronger opening. Each chapter began from a 1-2 page brief (story premise, level ideas, mechanics to introduce) — [Radiator blog](https://www.blog.radiator.debacle.us/2026/01/notes-on-level-design-workflow-for-3d.html)

**Celeste**
- Maddy Thorson's GDC 2017 talk "Level Design Workshop: Designing Celeste" covers the tools and process for 300+ hardcore platformer stages and how the chapter maps were arranged. It is free on GDC's YouTube channel — [GDC Vault](https://gdcvault.com/play/1024307/Level-Design-Workshop-Designing-Celeste); [Game Developer](https://gamedeveloper.com/design/video-the-level-design-of-i-celeste-i-)
- A summary of the talk says the team iterated on several designs per level to give players several ways to play, and added skips for skilled players and speedrunners. A blogger reads the talk as urging designers to think of rooms as "stories" rather than puzzles with a single solution (secondary) — [Medium: How to Design Breathtaking 2D Platformer Levels](https://medium.com/swlh/how-to-design-breathtaking-2d-platformer-levels-e8a261cdac93); see also [USgamer: designed with speedrunning in mind](https://www.usgamer.net/articles/how-celeste-was-designed-with-speedrunning-in-mind)
- Per a secondary analysis, each chapter's new mechanic first appears in a simple setting, then in harder challenges, and is finally combined with earlier mechanics for the hardest part — [Medium article](https://medium.com/swlh/how-to-design-breathtaking-2d-platformer-levels-e8a261cdac93)
- Celeste introduces a mechanic and moves on "long before wearing out its welcome". The same critic objects that one mechanic appears only in the very last level of the hardest difficulty, with no safe place to practise it — [Game Wisdom](https://game-wisdom.com/analysis/celeste)
- Checkpoints sit at the start of each room and respawn is instant, with no death screen. A smaller number of "hard" checkpoints per chapter work as save/break points, so rooms and checkpoints are two separate tools — [Game Wisdom](https://game-wisdom.com/analysis/celeste); [Daily Trojan](https://dailytrojan.com/?p=175624)
- The game's first gameplay screen teaches the dash right after the opening scene (Thorson narrative review, GDC 2023) — [GDC 2023 PDF](https://media.gdcvault.com/gdc2023/GNR/Will+Farhat+Game+Narrative+Review.pdf)

**Super Meat Boy (short rooms, instant restart)**
- McMillen designed levels to be "bite-sized" and built them in an in-game WYSIWYG editor — [Game Developer postmortem](https://gamedeveloper.com/audio/postmortem-team-meat-s-i-super-meat-boy-i-); [McMillen on difficulty](https://www.gamedeveloper.com/game-platforms/-i-super-meat-boy-i-s-mcmillen-explains-why-so-hard-)
- Hazards (saws, spikes, lasers) are placed to push the player toward a particular way of moving. In level 5-3 the final hurdle is often unrelated to the earlier ones and easier, and near-instant restart sends the player from the hard finish straight back to the practice start — [Game Developer: Making Meat Boy Move](https://gamedeveloper.com/design/making-meat-boy-move); [Game Developer: level 5-3](https://gamedeveloper.com/design/untold-riches-super-meat-boy-level-5-3)

**Metrics documents and gyms ("toolbox of moves")**
- Build a developer-only "metrics zoo" test map with every gameplay object. Prototype max jump height and distance, running jump, gravity, fall speed, minimum corridor width and ceiling height. Test usability and feel, and walk the map again after every metrics change. Use world-aligned grid textures; Unity's Triplanar node is the suggested way. Psychonauts 2 had an edit-time "jump preview" arc tool — [Level Design Book: Metrics](https://book.leveldesignbook.com/process/blockout/metrics)
- Expected outputs of a metrics pass: a blockout at the correct scale plus a text file that defines the metrics — [Methods & Tools for Level Design](https://methods-and-tools-for-level-design.netlify.app/metrics)
- Example 2D metric spec: regular jump peaks at 5 units and travels about 7 units horizontally; jump length was measured with a series of gaps — [Corey DeLorenzo: Level Design Metrics](https://coreydelorenzo.squarespace.com/designblog/2019/6/22/level-design-metrics)
- Precision 2D platformers need strict, grid-based ("hard") metrics, unlike the loose, eyeballed metrics of a forgiving 3D platformer — [Radiator blog](https://www.blog.radiator.debacle.us/2026/01/notes-on-level-design-workflow-for-3d.html)
- "You cannot measure your way to a good game experience." Metrics are necessary but not sufficient — [Level Design Book: Metrics](https://book.leveldesignbook.com/process/blockout/metrics) (via search snippet)

**Beat charts and intensity curves**
- A pacing plan should cover scope, hierarchy, causality, information and intensity. A "beat" is a small self-contained chunk. Musical techniques can be applied to beats: pulse, accent, rest, motif, variation, syncopation — [Level Design Book: Pacing](https://book.leveldesignbook.com/process/preproduction/pacing)
- Three planning artifacts: (1) a **beat sheet** of cards/rows you rearrange (Trello, Notion, Miro, Codecks, a wall); (2) a **flowchart** that flows in one direction, is colour-coded by beat type, and gets split up if it turns to spaghetti; (3) an **intensity graph**. Valve's HL2:Ep2 and Left 4 Dead graphs sorted beats into Explore, Combat, Choreo and Puzzle, with time in minutes on X and intensity 0-100% or 0-10 on Y. Journey used a spreadsheet with a row per experience goal — [Level Design Book: Pacing](https://book.leveldesignbook.com/process/preproduction/pacing)
- Pacing advice: start slow and quiet, alternate highs and lows (sustained intensity numbs players), and avoid a maximum-intensity final encounter (finales are often simple and readable). Intensity is not difficulty; it is closer to suspense or emotional stakes — [Level Design Book: Pacing](https://book.leveldesignbook.com/process/preproduction/pacing)
- Limits of pacing documents: they go stale quickly and can't be playtested. Once the game exists, favour the game over the plan — [Level Design Book: Pacing](https://book.leveldesignbook.com/process/preproduction/pacing)
- Pete Ellis wrote a multi-part series on pacing and gameplay beats — [World of Level Design: Pete Ellis part 3](https://worldofleveldesign.com/categories/wold-members-tutorials/peteellis/level-design-pacing-gameplay-beats-part3.php) (not fetched)

### Inferences
- For Rock The Game, one practical unit is a "mechanic arc" per area: 1 safe intro room for the new element (e.g. breakable platform), 2-3 test rooms, then twist rooms spaced through the area that combine it with grapple, dash or slingshot. This is teach-test-twist / kishōtenketsu applied room by room.
- A Unity "gym" scene with grid-textured blocks that measures grapple rope length, slingshot launch distance, dash distance, wall-stamina climb height and launch-pad apex would act as the metrics doc. The numbers already live in `PlayerStatsSO`, so the doc can be a table generated from or checked against it. A jump-arc/grapple-reach preview gizmo (like Psychonauts 2's) would be a high-value editor tool for placing nodes.
- A checkpoint at each room plus instant respawn fits the existing RoomManager/Checkpoint architecture. The Celeste reading suggests keeping rooms short enough that a death costs seconds.
- Optional strawberry-style collectibles and secret routes double as the "skill skip" layer for experts, which matches the Celeste talk summary.

### Gaps
- I did not get a transcript of Thorson's GDC talk, so details of her room-iteration process and tooling (Celeste used a custom/Ogmo-style editor) are unverified here. Watch the talk directly.
- I found no authoritative source on a "mechanic matrix" specifically for platformers; see section 3.
- Mark Brown's Boss Keys is relevant mainly to non-linear lock-and-key layouts; it is cited only through Boris the Brave, not fetched directly.

## 2. Sketching, blockout and flow-diagram tools (paper, Excalidraw, Miro, Figma, LDtk, lock-and-key graphs, Dormans)

### Takeaway
Use cheap layers in order: a beat/mission graph (what the player must do, in what order), then a rough layout sketch, then a playable blockout. The Level Design Book stresses that the sketch is for communication, not art, and that a rough sketch is enough for a small solo project. LDtk is the most capable free non-engine 2D editor with a maintained Unity importer, but it is optional given the existing prefab/room pipeline.

### Cited Findings
- Stages of the process: pre-production → layout (a top-down 2D plan; "a rough sketch may be enough for a small solo project") → blockout (a playable rough draft from simple shapes; playtest for size, clarity and fun) → scripting → lighting → environment art (don't rush the art pass; mistakes get expensive once art exists) → release. "Day to day, most level designers focus on layout and blockout." — [Level Design Book: Process overview](https://book.leveldesignbook.com/process/overview)
- The book's advice on scripting order: doors are among the hardest scripting problems, and trains and moving platforms are harder still; start with buttons and collectibles — [Level Design Book: Process overview](https://book.leveldesignbook.com/process/overview)
- **Lock-and-key / mission graphs**: draw an arrow from one item to another when the first is required for the second, and leave out room layout. The graph shows real choices, optional paths and impossible encounters. Distinguish hard locks from soft ones (bypassable, e.g. a tough monster guarding a path). Draw the graph first because moving things around is cheap at that stage. Pair it with a physical map or Mark Brown's Boss Keys diagrams to see backtracking — [Boris the Brave: Lock and Key Dungeons](https://www.boristhebrave.com/2021/02/27/lock-and-key-dungeons)
- **Dormans mission/space**: a mission graph of player tasks and how they relate, and a separate space graph for the level's spatial structure. Graph grammars generate each, and a later step fits the mission into space ("layout solving") — [Dormans 2010 PCG paper](https://www.pcgworkshop.com/archive/dormans2010adventures.pdf); [Dormans 2011](https://www.pcgworkshop.com/archive/dormans2011design.pdf); [Karavolos/Bouwer TU Delft](https://publications.graphics.tudelft.nl/rails/active_storage/blobs/redirect/eyJfcmFpbHMiOnsibWVzc2FnZSI6IkJBaHBBdHNIIiwiZXhwIjpudWxsLCJwdXIiOiJibG9iX2lkIn19--8016c0f08ec157c4dc0e637175c1036827198c9b/KBB15.pdf); cyclic generation (Unexplored): [Boris the Brave on Unexplored](https://www.boristhebrave.com/?p=780)
- **LDtk** (by Deepnight, the Dead Cells lead) is a 2D level editor for side-view and top-down games. It is pay-what-you-want, free for commercial use. The Cammin LDtkToUnity importer uses ScriptedImporter, so the project re-imports automatically when the LDtk file is saved. It supports entity → prefab replacement, entity and level fields, and tilesets packed into a SpriteAtlas; install via OpenUPM — [ldtk.io](https://ldtk.io/?p=497); [LDtkToUnity GitHub](https://github.com/Cammin/LDtkUnity); [OpenUPM](https://openupm.com/packages/com.cammin.ldtkunity/)
- Beat sheets can be kept in Trello, Notion, Miro, Codecks or on a physical wall — [Level Design Book: Pacing](https://book.leveldesignbook.com/process/preproduction/pacing)
- Speed up iteration with a Unity "play from scene-view position" button (Toolbar Extender package or Unity 6.3's MainToolbarElement API) — [Radiator blog](https://www.blog.radiator.debacle.us/2026/01/notes-on-level-design-workflow-for-3d.html)
- Edward Kay (DinoBoss, Hang Line, a mountain-swinging game, so close to the grapple genre) gave "Moving Mountains: Speedy Level Creation as a Desperate Indie Dev" at the GDC 2023 Level Design Summit (59 min). Content not retrieved; free on YouTube — [GDC Vault](https://gdcvault.com/play/1029215/Level-Design-Summit-Moving-Mountains); [Class Central](https://classcentral.com/course/youtube-moving-mountains-speedy-level-creation-as-a-desperate-indie-dev-206247)

### Inferences
- A sensible lightweight stack for this project: (a) paper or graph-paper thumbnails, or a whiteboard tool such as Excalidraw, for room sketches using a fixed tile grid that matches Unity units; (b) one mission graph per area (keys, doors, coins, NPC story beats); (c) blockout directly in Unity with greybox prefabs. The room prefabs and the `unity command eval` workflow already exist, so a separate tile editor like LDtk adds an import layer the project may not need. It is worth considering only if room authoring in Unity proves slow.
- Hang Line is the closest genre reference (swinging/grappling). Kay's talk is a must-watch.

### Gaps
- I did not fetch sources on Excalidraw, Figma, Krita or Aseprite as level tools. Recommending them is my inference (they are common whiteboard and pixel tools), not a cited finding.
- Mark Brown's Boss Keys method was not fetched directly.

## 3. Documentation templates (one-page room docs, mechanic matrix, coverage spreadsheets)

### Takeaway
The sourced templates are the 1-2 page chapter/area brief (Big Hops), the beat sheet / flowchart / intensity graph trio, and a metrics text file. One-page level docs take a few hours to write. I found no authoritative "mechanic matrix" template; it has to be built as a spreadsheet by inference.

### Cited Findings
- Big Hops: a 1-2 page design brief per chapter (story premise, level ideas, mechanics to introduce), reviewed by the team before prototyping — [Radiator blog](https://www.blog.radiator.debacle.us/2026/01/notes-on-level-design-workflow-for-3d.html)
- A solo dev logged about 4.5 h for a one-page platformer level design doc (45 min learning, 1 h draft, 2.5 h final) — [itch.io devlog: one-page level design](https://itch.io/devlog/910885/pre-feedback-one-page-level-design.amp)
- God of War Ragnarök "boss flow" documents outlined key elements and design rationale for boss set pieces. Journey tracked intensity per experience goal in a spreadsheet — [Level Design Book: Pacing](https://book.leveldesignbook.com/process/preproduction/pacing)
- Categories of metrics worth codifying include pacing (level length, distance between objectives, backtracking limits, resource density) and storytelling (dialogue length versus travel distance, density of narrative collectibles) — [Level Design Book: Metrics](https://book.leveldesignbook.com/process/blockout/metrics)

### Inferences
- A one-page room card could hold: room ID; area; beat role (teach/test/twist/rest/set piece); mechanics used; new element; critical path in one sentence; optional route or collectible; target first-attempt death count or time; checkpoint/entry and exit directions; story beat (if any); status (sketch/blockout/tested/art). This mirrors the beat-sheet fields above.
- A coverage spreadsheet (rows = rooms, columns = grapple single/dual, dash, wall climb, slingshot, launch pad, breakable, acid, moving platform, spiral node, NPC) makes the mechanic matrix visible. Empty cells show combinations never explored; long runs of one column show overuse (the risk Celeste avoids). A 1-5 difficulty column plotted over room order gives the difficulty/intensity curve.
- Keep the card in markdown next to `docs/` or as a row in TASKS-adjacent tracking; the Level Design Book warns that documents go stale.

### Gaps
- I found no published one-page room template from a shipped precision platformer.

## 4. Playtesting methodology for solo devs

### Takeaway
Watch behaviour, not opinions. Run short, hypothesis-driven sessions (e.g. 15 minutes on one area with one question), take silent notes or a remote OBS recording, then debrief with comprehension and subjective questions. Steam Playtest gives free, gated remote distribution that doesn't affect store metrics.

### Cited Findings
- Formats: daily self-playtest; critique by a fellow designer after each phase (layout, blockout, scripting, art); blind public playtest where you watch behaviour more than feedback. Note that ordinary players find blockouts hard to read — [Level Design Book: Playtesting](https://book.leveldesignbook.com/process/blockout/playtesting)
- Treat each test as an experiment: set a hypothesis, scope (e.g. "first half of level 2, max 15 minutes"), focus (e.g. "is the exit clear?"), and decide in advance what data to collect (e.g. time to boss 1, deaths) — [Level Design Book: Playtesting](https://book.leveldesignbook.com/process/blockout/playtesting)
- During the session: sit a short distance behind the player, not over their shoulder; don't apologise or explain; intervene only if they are truly stuck. Remote option: the player records screen and voice with OBS and sends an unlisted video, or streams privately — [Level Design Book: Playtesting](https://book.leveldesignbook.com/process/blockout/playtesting)
- Debrief with comprehension questions ("what did the orange light mean?") and subjective ones ("did it feel easy or hard?"). The suggested fix may be poor while the problem behind it is real. The book cites de Jongh's GDC 2017 talk as advising against forms and questionnaires — [Level Design Book: Playtesting](https://book.leveldesignbook.com/process/blockout/playtesting)
- Tester biases: new players are too polite; experienced players nitpick or perform for the observer — [Level Design Book: Playtesting](https://book.leveldesignbook.com/process/blockout/playtesting)
- Cadence example: Valve cabals of 6-8 people worked a weekly cycle with a Friday playtest by an outsider and Monday goal-setting — [Level Design Book: Playtesting](https://book.leveldesignbook.com/process/blockout/playtesting)
- Basic stats (play time, deaths) need no special tools beyond careful notes. Telemetry heatmaps of deaths help spot clustering — [Level Design Book: Playtesting](https://book.leveldesignbook.com/process/blockout/playtesting)
- **Steam Playtest**: a free Steamworks feature where players request access from the store page, the dev admits them from a queue in batches, and a separate playtest app/branch is used. It doesn't affect wishlists, reviews or playtime stats, has its own community hub, and is removed from libraries at release. Not commerce/Early Access. Valve can refuse key requests — [Steamworks docs](https://partner.steamgames.com/doc/features/playtest); [Game Developer](https://gamedeveloper.com/game-platforms/steam-s-built-in-playtest-feature-is-now-live-for-all-devs); [Engadget](https://www.engadget.com/steam-playtest-beta-043428799.html)

### Inferences
- Per-room death and time telemetry is cheap to add: log room ID, deaths and time per room to a CSV on room exit. It turns playtests into data for the difficulty column of the coverage spreadsheet, and death-position dots per room act as a heatmap.
- Because the game is gamepad-only, every playtest brief should hand testers a controller, and the debrief should ask about grapple target readability (which node gets selected).
- itch.io restricted or password-protected pages are the low-friction alternative before a Steam page exists (my inference, not fetched).

### Gaps
- No source gave a recommended tester count per round. The often-quoted "5 users" rule comes from web usability (Nielsen) and was not verified for games here.

## 5. Story-driven and set-piece levels (environmental storytelling, chase sequences)

### Takeaway
Set pieces are expensive, unskippable and heavily scripted. Plan one you can realistically finish, prototype an easier version of the moment first, and keep chases moving and readable.

### Cited Findings
- Set pieces are expensive (unique assets, constant iteration), usually unskippable and heavily scripted. A set piece with three outcomes is effectively three set pieces. Plan at least one per project, but modest; "the best set piece is one you can realistically finish" — [Level Design Book: Pacing](https://book.leveldesignbook.com/process/preproduction/pacing)
- Chase advice: don't make the player stop during a high-speed chase (a boat chase broken up by valve doors is the cautionary example) — [World of Level Design: Mirror's Edge](https://worldofleveldesign.com/categories/level_design_tutorials/one-mistake-in-mirrors-edge.php). Keep the route visible: in Mirror's Edge the author missed the intended path because it sat outside the view — [World of Level Design: flow tips](https://worldofleveldesign.com/categories/level_design_tutorials/flow-tips-mirrors-edge.php)
- Chases should be fair enough that players succeed through skill rather than memorisation (blog, lower authority) — [Bugnet: how to design a chase sequence](https://bugnet.io/blog/how-to-design-a-chase-sequence)
- Rayman Legends' music levels and chase worlds are a strong reference for rhythm-timed scripted platforming — [GMTK: Breaking Down the Best World in Rayman Legends (subtitles)](https://amara.org/subtitles/XrnrkoeBp29g/en/1/download/Breaking%20Down%20the%20Best%20World%20in%20Rayman%20Legends%20%20Game%20Makers%20Toolkit.en.srt)
- Environmental storytelling is a dedicated Level Design Book page, tied to the art pass — [Level Design Book: Storytelling](https://book.leveldesignbook.com/process/env-art/storytelling) (linked from the overview; not fetched)
- Plan pacing around "information" (what the player is told, and when) and add Choreo beats (scripted dialogue/sequences) to the intensity graph as their own category — [Level Design Book: Pacing](https://book.leveldesignbook.com/process/preproduction/pacing)

### Inferences
- For the ice-cream-man storyline: treat each NPC story room as a "rest/choreo" beat on the intensity graph, placed after high-intensity rooms. A chase (e.g. pursuing the truck) can be greyboxed as a sequence of short rooms with a scripted scroller/pursuer and room checkpoints, so failures restart only one segment, in line with the Celeste and Meat Boy instant-restart norms. Prototype the chase with placeholder rectangles and timing before any art.

### Gaps
- No primary-source postmortem on prototyping 2D platformer chases (e.g. Celeste's Badeline chases) was retrieved.

## 6. Time management and production passes for solo level production

### Takeaway
No reliable "rooms per day" benchmark exists in the sources. The consistent advice is to separate passes (layout → blockout + playtest → scripting → art), avoid early art, front-load the first chapter (which takes longest while the "unit of content" is defined), and make the playtest loop as fast as possible.

### Cited Findings
- Process passes and the warning against an early art pass — [Level Design Book: Overview](https://book.leveldesignbook.com/process/overview)
- The first chapter takes longest because the team has to define the "unit of content", how emergent levels are, and what a finale looks like; later chapters reused the pattern faster — [Radiator blog](https://www.blog.radiator.debacle.us/2026/01/notes-on-level-design-workflow-for-3d.html)
- A one-page level doc took about 4.5 h for a learning solo dev — [itch.io devlog](https://itch.io/devlog/910885/pre-feedback-one-page-level-design.amp)
- Valve weekly cadence (build Monday-Thursday, outsider playtest Friday) — [Level Design Book: Playtesting](https://book.leveldesignbook.com/process/blockout/playtesting)
- Solo dev advice: for practice, make a linear platformer rather than a Metroid-like — [itch.io: Level Designs and the Solo Dev Grind](https://unbearablestudios.itch.io/singular-event/devlog/391136/level-designs-and-the-solo-dev-grind)
- Kine's solo developer spent 14 months on one phase of development — [Unreal Engine blog: Kine level design](https://www.unrealengine.com/en-US/tech-blog/from-blockout-to-launch---a-behind-the-scenes-look-at-kine-s-level-design)

### Inferences
- Suggested daily rhythm that fits the project's 4 tasks/day rule: (1) sketch 3-5 room thumbnails for one beat arc; (2) block out 2-3 of them in Unity; (3) self-playtest with a gamepad and log deaths/time; (4) revise or cut. Do the art pass in weekly batches only after rooms survive an external playtest. Spend the first area's extra time building the gym, room template and coverage spreadsheet, then reuse them.
- Build several more rooms than needed and keep the best (the "pile of beats" approach). The cut rooms become B-side or optional material.

### Gaps
- No source gave a measured rooms/day or hours-per-room figure for a shipped precision platformer. Celeste's 300+ rooms over its development time could be used to estimate one, but the development length and team split were not retrieved here.
