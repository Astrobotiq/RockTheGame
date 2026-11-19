# Internal level-design tools and workflows of shipped indie precision platformers / metroidvanias

Scope note: about 20 search/fetch calls. Good primary or near-primary coverage for Celeste (Everest debug-mode docs, EXOK podcast transcript), Dead Cells (Bénard's own article), LDtk, Spelunky, N/N++ (Metanet GDC 2016 coverage), Super Meat Boy, Ori (Korol interview), Hollow Knight (Unity case study) and Pizza Tower (TCRF). **I did not reach** Hyper Light Drifter, Katana Zero, Webbed, Haiku the Robot or Bionic Commando-style games. There are no claims about them below, so treat them as gaps.

## Q1: Which level editors did they use (custom, Ogmo, LDtk, Tiled, in-engine) and why?

### Takeaway
The successful small teams either built a **custom external/in-game editor fitted exactly to their game's entity set** (Celeste, Super Meat Boy, N, Earthblade) or **bent their engine's editor into a production tool** with custom inspectors and multi-scene editing (Ori, Hollow Knight on Unity). Generic tools showed up where data needed structure (Dead Cells used CastleDB, and its director later built LDtk). The trend is to move the editor *inside* the game project, so new entity types appear in the editor automatically.

### Cited Findings
- **Celeste:** Noel Berry says "we did an external-level editor for Celeste". The transcript does not name it (garbled). — [Software Engineering Daily ep. 1691, EXOK Games transcript](https://softwareengineeringdaily.com/wp-content/uploads/2024/06/SED1691-EXOK-Games.txt)
- **Earthblade (EXOK's next game):** the map editor is built *into* the game project. Because C# reflection makes new game objects appear in the editor automatically, Noel calls the integration "really nice". They also built an in-engine "sequence editor" for animation and sound timing plus hitbox frame data. Sound designers place sounds in the editor instead of hard-coding them. — [SED1691 transcript](https://softwareengineeringdaily.com/wp-content/uploads/2024/06/SED1691-EXOK-Games.txt)
- **Celeste's sound timing was hard-coded** to animation frames in player code, which Noel calls a "very rare thing to have in your player code". It was the pain point that motivated Earthblade's sequence editor. — [SED1691](https://softwareengineeringdaily.com/wp-content/uploads/2024/06/SED1691-EXOK-Games.txt)
- **Ogmo Editor** is a free, open-source, general-purpose level editor written in Haxe and credited to Matt Makes Games (the Celeste studio). Maddy Thorson's GitHub hosts `Ogmo-Editor-v1` ("Generic level and tilemap editor for indie game developers"). **I found no source confirming Ogmo was the Celeste editor.** — [gamefromscratch](https://gamefromscratch.com/?p=11144); [github.com/maddythorson](https://github.com/maddythorson)
- **Celeste origin:** the game began as a PICO-8 game Thorson and Berry made in a few days (a weekend jam). Noel later wrote a C# API that remapped PICO-8 calls in order to port the Lua code. — [Wikipedia: Celeste](https://en.wikipedia.org/wiki/Celeste_(video_game)); [SED1691](https://softwareengineeringdaily.com/wp-content/uploads/2024/06/SED1691-EXOK-Games.txt)
- **Celeste community editors** (Lönn, Ahorn, Maple, Snowberry, Starforge) edit the shipped `.bin` map format: maps are made of rooms, and you add rooms with Room → Add. Lönn claims about 99% save compatibility with vanilla maps. The `celeste_rs` crate documents reading and writing the binary format. — [Lönn repo](https://github-redirect.dependabot.com/CelestialCartographers/Loenn); [Everest First Custom Map](https://github-wiki-see.page/m/EverestAPI/Resources/wiki/First-Custom-Map); [celeste_rs source](https://docs.rs/crate/celeste_rs/latest/source/src/maps/mod.rs)
- **Super Meat Boy:** Tommy Refenes built an in-game WYSIWYG level editor. It was "invaluable" because it let Edmund McMillen build levels with a "what you see is what you get" mindset, and McMillen waited on it for much of development. It later shipped to PC players as the Super Meat World editor. It stayed PC-only partly because it would need a controller interface. — [Game Developer postmortem: Super Meat Boy](https://gamedeveloper.com/audio/postmortem-team-meat-s-i-super-meat-boy-i-); [GameZone](https://gamezone.com/news/super-meat-boy-level-editor-is-live/amp); [Game Informer afterwords](https://www.gameinformer.com/b/news/archive/2011/01/01/super-meat-boy-afterwords.aspx)
- **N / N++ (Metanet):** the original N shipped with its built-in editor "Ned", with a manual by Burns and Sheppard. Metanet says some levels grow while "mindlessly sketching in the editor". They hand-crafted N-series levels for about ten years. — [N editor manual](https://discmaster.textfiles.com/view/23972/GSSH0804.iso/Geschicklichkeit/N/N.exe/editor_manual.txt); [PlayStation Blog: Designing N++ Levels](https://blog.playstation.com/?p=145712)
- **Dead Cells:** hand-designed room "tiles" were authored and stored with **CastleDB**, a structured, spreadsheet-like database tool by the same Haxe/Shiro ecosystem. — [deepnight.net: The Level Design of Dead Cells](https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/)
- **LDtk** is free and open source, by Deepnight Games (Sébastien Bénard, "director of Dead Cells"). Features: a world view with smooth level↔world zoom, "Grid-vania"/linear/free world layouts with drag-to-arrange levels, rule-based auto-layers for automatic tile skinning, typed custom entities (HP, patrol paths, inventories), documented JSON export plus a "Super Simple Export" (per-level PNG plus small JSON), and Unity listed among supported engines. — [ldtk.io](https://ldtk.io/)
- **Ori (Moon Studios, Unity):** Gennadiy Korol said Unity's biggest strength "by far was how flexible and extensible its Editor is", turning it into "the perfect, fine-tuned 'Ori production engine'". Because the world is continuous, they built a tool to **load and edit several scenes at once**, which later became the base of their cinematic tools. His advice: "Learn to write custom inspectors and editor tools." For Ori 2 they used a heavily modified Unity ("Moonity") with source access. — [MCV: Unity focus, making Ori](https://www.mcvuk.com/development-news/unity-focus-making-ori-and-the-blind-forest/); [Unity Moon Studios case study](https://create.unity3d.com/moon-studios-case-study)
- **Hollow Knight (Team Cherry, Unity):** built with 2D assets layered in a 3D environment, using Unity's stock 2D features (2D Physics, Sprite Packer, Particle System) plus **2D Toolkit** from the Asset Store. Per the Unity page, enemies and interactive elements were built in **PlayMaker** visual scripting. — [Unity: Made with Unity, Hollow Knight](https://unity.com/made-with-unity/hollow-knight)
- **Pizza Tower (GameMaker):** an in-game level editor object/room (`obj_editor` / `rm_editor`) existed. It was released publicly from the "Boss build" in Feb 2020, then removed, but the room and object remain in the final game. A `-debug` launch flag enables a console (F5) with an `editor` command. — [TCRF: Pizza Tower](https://www.tcrf.net/Pizza_Tower); [Proto: Pizza Tower/Level Editor](https://new.tcrf.net/Proto:Pizza_Tower/Level_Editor); [gameplay.tips](https://gameplay.tips/guides/pizza-tower-cheats-console-debug-mode.html)
- **General Unity in-engine editor pattern** (third-party): GDC 2020 "creating custom tools for game design" and "keeping level design in the zone with level editors". — [80.lv GDC 2020 custom tools](https://80.lv/articles/gdc-2020-creating-custom-tools-for-game-design); [80.lv level editors](https://80.lv/articles/keeping-level-design-in-the-zone-with-level-editors)

### Inferences
- For a solo Unity dev the Ori/Hollow Knight route (Unity Tilemap or Scene view plus custom inspectors/editor windows plus a multi-scene/prefab "room" workflow) matches EXOK's lesson: when the editor lives in the project, every new entity (grapple node, spiral node, platform strategy) is placeable at once with no exporter. An external editor (LDtk) mainly pays off for **world/room-graph layout and rule-based autotiling**, and Unity's RuleTile covers part of that.
- Super Meat Boy's case shows that the editor's real value is WYSIWYG speed for the designer. If a Unity Scene-view workflow already gives that, the gap is mainly "play from here" and room-graph overview (see Q2/Q3).

### Gaps
- No source confirming which editor the 2018 Celeste used (Ogmo is plausible but unverified).
- Tool choices for Hyper Light Drifter, Katana Zero, Grapple Dog, Webbed and Haiku the Robot were not found or researched.
- No first-party N++ editor-internals doc found (the GDC talk is at [GDC Vault: Empowering the Player](https://gdcvault.com/play/1023282/Empowering-the-Player-Level-Design)).

## Q2: Which debug/iteration features did they rely on (play-from-anywhere, room teleport map, instant restart, noclip, slow-mo, input replay, heatmaps, timers)?

### Takeaway
Celeste's shipped (hidden) debug suite is the best-documented model: **F5 reloads the current room, Ctrl+F5 reloads the game, F6 opens a zoomable world map of all rooms where right-click teleports you to the nearest spawn point (Shift = exact cursor position), plus a dev console and an everything-unlocked debug save.** Fast compile/reload loops were treated as a core productivity factor.

### Cited Findings
- **Celeste debug-mode hotkeys:** F5 reloads the active level/room. F6 opens the debug map ("level editor") for teleporting. Ctrl+F5 reloads the whole game. `.` or `~` opens a debug console (`help`, `help [command]`, `q`). F1–F3 reload assets. A `~DEBUG~` save has everything unlocked, and strawberries reset when a level starts. — [Everest wiki: debug mode](https://github.com/EverestAPI/Resources/wiki/debug-mode); [Celeste Wiki: Debug Mode](https://celestegame.fandom.com/wiki/Debug_Mode)
- **Celeste debug map details:** it zooms with the scroll wheel or right stick and pans with keys, Space+drag or middle-drag. Each room is a rectangle you can recolor with keys 1–7 (white/red/green/cyan/blue/purple/magenta), so designers color-code progress or status. Red dots are spawn points. Right-click a room to spawn at the nearest spawn point, Shift+right-click to spawn at the exact cursor position. It highlights jump-throughs (yellow), strawberries (pink, Q shows `checkpoint:order` IDs), keys (gold, F1 shows order) and checkpoints (green). The header shows room name, size and position (×8, the tile size). Rooms can be dragged and snap to neighbours (Alt disables snapping), overlaps blink red, and there is box/Ctrl selection, Ctrl+A and Ctrl+Z. **Filler rooms** (gray, no spawn, not enterable) are added and removed with F+click to fill space near transitions. Saving was removed from the shipped build (F1/Ctrl+S do nothing), which implies it **was a real editing tool internally**. — [Everest wiki: Debug Map](https://github.com/EverestAPI/Resources/wiki/Debug-Map)
- **Iteration speed:** EXOK moved to C#/FNA, which brought compiles to about a second, and Noel says that made iteration much faster than their C++ version. In Celeste they "learned where to edit the constants" in a big "spaghetti" player class. — [SED1691](https://softwareengineeringdaily.com/wp-content/uploads/2024/06/SED1691-EXOK-Games.txt)
- **Instant respawn** is cited as a Celeste design choice that makes high difficulty less punishing. — [Celeste (wiki mirror)](https://wiki.krisyotam.com/content/wikipedia_en_all_nopic_2026-03/Celeste_(video_game))
- **Pizza Tower:** has a `-debug` console (F5) with commands including `editor` (jump to the editor room). TCRF notes the editor room was kept and is relevant for room reloading during development. — [TCRF Pizza Tower](https://www.tcrf.net/Pizza_Tower); [gameplay.tips](https://gameplay.tips/guides/pizza-tower-cheats-console-debug-mode.html)
- **Metanet** used the level archive plus the editor tools to compare and revert over-tweaked levels case by case late in development, so level version history was an iteration tool. — [Thumbsticks: GDC 2016 N++ level design](https://www.thumbsticks.com/gdc-2016-the-secret-to-nplusplus-level-design/)

### Inferences
- Directly copyable in Unity: (1) an **editor window or in-game overlay listing all `Room`s as rectangles at their world positions**, with click-to-teleport to the nearest checkpoint/spawn and Shift-click for the exact spot. RockTheGame already has `RoomManager`, `Room.NeighborRooms`, `Checkpoint` and `RespawnManager`, so a teleport just needs to call SetAsCurrent plus a player warp. (2) A **"reload room" hotkey** that resets the current room (via the existing `LevelResetManager`/`IResettable`) without reloading the scene. (3) **Room status colors** stored per room (todo / blocked-out / art pass / done) shown in the map. (4) An everything-unlocked debug save. (5) "Play from Scene-view cursor" (Shift-teleport equivalent) is the Unity analogue of Celeste's exact-position teleport.
- Treat compile and domain-reload time as a level-design metric too. Unity's "Enter Play Mode Options" (disable domain reload) is the analogue of EXOK's one-second compile. This is an inference, not from a source.

### Gaps
- No sources found on noclip, slow-mo, input replay/ghosts, death heatmaps or analytics in any of these titles' internal tools. Speedrun-community tools such as CelesteTAS exist but were not researched. The Celeste console command list was not retrieved.

## Q3: How did they author rooms/screens (grid size, room templates, connectivity/world-map views)?

### Takeaway
Rooms are **rectangles on a shared world grid with an 8 px tile unit** (Celeste), laid out and snapped in a world view. Procedural or semi-procedural games use **small hand-made room templates classified by their entrances and exits** (Spelunky's 4×4 grid of 10×8-tile rooms; Dead Cells' biome room tiles placed on a designer-authored concept graph).

### Cited Findings
- **Celeste:** the debug map header shows room position and "position × 8", so the tile unit is 8 px. Rooms snap edge-to-edge to neighbours, rooms with a cyan inner border at the origin are starting rooms, and filler rooms pad non-playable space near transitions. — [Everest wiki: Debug Map](https://github.com/EverestAPI/Resources/wiki/Debug-Map)
- **Celeste map format:** a map is a set of rooms, each with tiles, entities and spawn points, edited in Lönn/Ahorn. — [Lönn](https://github-redirect.dependabot.com/CelestialCartographers/Loenn); [Everest Mapping FAQ](https://github.com/everestapi/resources/wiki/Mapping-FAQ)
- **Spelunky:** each level is a 4×4 grid of 16 rooms. Each room is 80 tiles, stored as an 80-char string (10 wide × 8 high), per a 2010 modding tutorial. Derek Yu hand-made templates by role on the solution path: drop-down rooms, landing rooms, corridors and off-path rooms. "Probabilistic tiles" inside templates add variety. — [Tiny Subversions: Modding Spelunky's Level Generator](https://www.tinysubversions.com/2010/01/tutorial-modding-spelunkys-level-generator/index.html); [GMTK: How and Why Spelunky Makes its Own Levels (transcript)](https://amara.org/subtitles/p6MHlmreohsY/en-gb/1/download/How%20and%20Why%20Spelunky%20Makes%20its%20Own%20Levels%20%20Game%20Makers%20Toolkit.en-gb.txt); [Tiny Subversions: Spelunky's Procedural Space](https://tinysubversions.com/2009/09/spelunkys-procedural-space/index.html)
- **Dead Cells:** a fixed hand-designed layer covers the island map, level interconnections and key locations, the same on every seed. Hand-designed room tiles each have "a specific layout of platforms designed for a specific purpose" (treasure, merchant, combat), parameterized mostly by entrance/exit count and purpose, and each room belongs to one biome. A per-biome concept graph is authored by placing the entrance and exit first, then special rooms, then combat/exploration tiles. The graph controls length, special-room count, how labyrinthine the biome is and the entrance→exit distance. The generator picks random rooms per node and retries until they fit. Monster count scales with combat-tile length (e.g. 1 per 5 tiles). — [deepnight.net Dead Cells LD](https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/); [Game Developer mirror](https://www.gamedeveloper.com/design/building-the-level-design-of-a-procedurally-generated-metroidvania-a-hybrid-approach-)
- **Unity port of the Dead Cells approach:** Edgar-Unity implements graph plus hand-made room templates, with a Dead Cells example. — [Edgar-Unity Dead Cells example](https://ondrejnepozitek.github.io/Edgar-Unity/docs/examples/dead-cells)
- **LDtk "Grid-vania" world layout** directly models a metroidvania room grid with drag-arranged levels. — [ldtk.io](https://ldtk.io/)
- **Ori:** a continuous world split into multiple Unity scenes, edited several at a time with a custom multi-scene tool. — [MCV Ori](https://www.mcvuk.com/development-news/unity-focus-making-ori-and-the-blind-forest/)
- **N++:** single-screen levels on a tile grid. Beyond 0/45/90° surfaces they added 22.5° and 67.5° slopes and curves for new movement shapes. — [Thumbsticks GDC 2016](https://www.thumbsticks.com/gdc-2016-the-secret-to-nplusplus-level-design/)

### Inferences
- For a Celeste-like Unity game: keep every `Room` bound on an integer tile grid and snap room rects edge-to-edge (Celeste style). Visualize rooms and their `NeighborRooms` links as a gizmo or editor-window graph, so missing or one-way links and overlaps (Celeste's red blink) are caught automatically.
- Even without procgen, the Spelunky/Dead Cells habit of **tagging each room by role** (intro / practice / test / breather / secret, plus entrances and exits) is a cheap planning aid.

### Gaps
- Celeste's standard room dimensions (in tiles) were not retrieved from a primary source. Hollow Knight's room/scene sizing and the Team Cherry map-planning process were not retrieved; the PC Gamer article fetch failed.

## Q4: How did they validate difficulty/reachability (playtest analytics, jump-distance metrics, move "toolkit")?

### Takeaway
The sources describe **human playtesting loops, not automated metrics**: designers repeatedly play each other's levels, strip out "unfair" or bug-like behaviour, offer safe vs. hard routes, and add the hardest content only after the layout is final. Metanet warns that over-tweaking erodes simplicity and keeps an archive to revert.

### Cited Findings
- **N++:** the process is build → test → refine → re-test. Sheppard and Burns play each other's levels ("pass things back and forth"). Repeated designer runs strip anything "overly frustrating or unfair". Many levels offer a safer low-gold route and a harder high-gold route so players self-select difficulty. Balancing levers are difficulty, length, required movement types, object combinations, pacing, and patience vs. reflex. Sheppard compares level design to cooking ("keep tasting") and notes that years of play make designers poor judges of difficulty. Late in development many levels had lost simplicity through over-adjustment, and they used the archive to revert case by case, which inspired the secret levels. — [Thumbsticks GDC 2016](https://www.thumbsticks.com/gdc-2016-the-secret-to-nplusplus-level-design/); [GDC Vault: Empowering the Player – Level Design in N++](https://gdcvault.com/play/1023282/Empowering-the-Player-Level-Design)
- **Celeste:** playtesting removed unintended behaviour ("basically almost like bugs") but kept intentional challenge. — [SED1691](https://softwareengineeringdaily.com/wp-content/uploads/2024/06/SED1691-EXOK-Games.txt)
- **Celeste GDC 2017 level-design talk** (Thorson) covers iterating on room designs, offering options and adding skips for skilled players and speedrunners. It is now free on GDC's YouTube channel. A secondary write-up claims harder additions only come once the layout is "110% finalised" (unverified against the talk). — [Game Developer: The level design of Celeste (video)](https://gamedeveloper.com/design/video-the-level-design-of-i-celeste-i-); [PrimedPixel write-up](https://primedpixel.co.uk/posts/lessons-ive-learnt-from-celestes-level-design/)
- A secondary blog claims early Celeste playtests were frustrating for most testers; it gives no origin, so treat it with caution. — [Wayline blog](https://www.wayline.io/blog/pivoting-celeste-keeping-the-climb)
- **Dead Cells** used "many trials, errors, adjustments", with no formal method described. — [deepnight.net](https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/)

### Inferences
- A solo dev lacks a second designer to "pass things back and forth", so cheap substitutes are: (a) a per-room death counter and time-in-room logged to a CSV during playtests, a minimal homemade heatmap (inference); (b) version-controlling rooms as prefabs so Metanet-style reverts are possible; (c) the existing PlayMode rule tests in `docs/bozulmamali-davranislar.md` as a "move toolkit" contract, so level geometry built on measured jump/grapple distances stays valid when tuning changes.
- A "move toolkit" document of max jump height/distance, dash length and grapple range drawn as Scene-view gizmos around the player or a node is a common practice. **No source in this research confirmed a specific shipped team doing it.**

### Gaps
- No primary source found on jump-distance metric sheets, reachability checkers, telemetry or death heatmaps for any listed game.

## Q5: Grapple/swing-specific level design lessons and tools

### Takeaway
Little first-party material exists. Grapple Dog **deliberately constrained the hook to straight up or 45°** and built on momentum, and reviewers praised teaching through level design but criticized under-use of the grapple in late mechanics.

### Cited Findings
- Grapple Dog's hook fires only straight up or at 45°, with momentum as the key feature. — [PC Gamer: Grapple Dog](https://pcgamer.com/canine-flinging-platformer-grapple-dog-has-a-hook-to-rival-titanfalls)
- Grapple Dog's early grapple prototype ("Grapple Hook Platformer", 2019, Patreon builds by Joseph Gribbin) is documented on TCRF. — [TCRF: Grapple Dog prototype](https://new.tcrf.net/Proto:Grapple_Dog/Grapple_Hook_Prototype)
- Reviewers say Grapple Dog teaches the grapple through level design, not pop-ups (NME), but that some mechanics don't fully exploit it (NME) and the hook "never truly takes centre stage" (Well Played). — [NME review](https://www.nme.com/reviews/grapple-dog-review-3155573); [Well Played review](https://www.well-played.com.au/?p=422041)
- Hobby devlogs on grapple level design exist but are not authoritative. — [euanleith grapple game devlog](https://euanleith.itch.io/grapple-game/devlog/648542/1-level-design-combat-and-art-assets)

### Inferences
- Restricting grapple angles (or snapping to authored nodes, as RockTheGame's `NodeDetector` does) makes levels authorable and readable. A tooling corollary is to draw each grapple node's reachable radius and swing arc as a gizmo, so geometry clearance is checked visually (inference).
- The Grapple Dog critique suggests making sure every new room mechanic interacts with the grapple, not only with run/jump.

### Gaps
- No postmortems or talks found for Webbed, Haiku the Robot, Bionic Commando (Rearmed) or other swing-focused games. No first-party Grapple Dog design writing found.

## Q6: Specific talks, postmortems, blogs and open-sourced tools (reading list)

### Takeaway
The highest-value primary materials are Thorson's GDC 2017 Celeste talk, Metanet's GDC 2016 N++ talk, Bénard's Dead Cells article, the Super Meat Boy postmortem, Korol's Ori/Unity interview, and the Everest Debug Map doc (a precise spec of a shipped room-teleport tool). For open tools: LDtk, Ogmo, and the Celeste community editors and format libraries.

### Cited Findings
- Celeste level design, GDC 2017 (Thorson). — [Game Developer video page](https://gamedeveloper.com/design/video-the-level-design-of-i-celeste-i-)
- Empowering the Player: Level Design in N++, GDC 2016 (Sheppard and Burns). — [GDC Vault](https://gdcvault.com/play/1023282/Empowering-the-Player-Level-Design); [PlayStation Blog](https://blog.playstation.com/?p=145712)
- The Level Design of Dead Cells: a hybrid approach (Bénard). — [deepnight.net](https://deepnight.net/tutorial/the-level-design-of-dead-cells-a-hybrid-approach/); Bénard's GDC 2019 design-tips talk is announced at [GDC](https://gdconf.com/article/get-game-design-tips-and-tricks-from-dead-cells-co-creator-at-gdc-2019/)
- Postmortem: Team Meat's Super Meat Boy. — [Game Developer](https://gamedeveloper.com/audio/postmortem-team-meat-s-i-super-meat-boy-i-)
- Unity focus: making Ori and the Blind Forest (Korol). — [MCV](https://www.mcvuk.com/development-news/unity-focus-making-ori-and-the-blind-forest/)
- EXOK Games on Software Engineering Daily (Celeste and Earthblade tooling). — [transcript](https://softwareengineeringdaily.com/wp-content/uploads/2024/06/SED1691-EXOK-Games.txt)
- Everest Debug Mode and Debug Map docs. — [debug-mode](https://github.com/EverestAPI/Resources/wiki/debug-mode); [Debug-Map](https://github.com/EverestAPI/Resources/wiki/Debug-Map)
- Open tools: LDtk ([ldtk.io](https://ldtk.io/)), Ogmo ([gamefromscratch](https://gamefromscratch.com/?p=11144)), Lönn ([repo](https://github-redirect.dependabot.com/CelestialCartographers/Loenn)), Celeste PICO-8 editor Everhorn ([repo](https://github.com/CelesteClassic/everhorn)), celeste_rs ([docs.rs](https://docs.rs/crate/celeste_rs/latest/source/src/maps/mod.rs)), Edgar-Unity graph-plus-room-template generator ([docs](https://ondrejnepozitek.github.io/Edgar-Unity/docs/examples/dead-cells)).
- GMTK on Spelunky's level generation. — [transcript](https://amara.org/subtitles/p6MHlmreohsY/en-gb/1/download/How%20and%20Why%20Spelunky%20Makes%20its%20Own%20Levels%20%20Game%20Makers%20Toolkit.en-gb.txt)

### Inferences
- A prioritized copy list for a solo Unity room-based grapple platformer, synthesized from the above:
  1. A Celeste-style debug map (room rects, status colors, click-to-teleport to the nearest checkpoint, Shift for the exact spot).
  2. A reload-current-room hotkey.
  3. An everything-unlocked debug save.
  4. Custom inspectors and gizmos for every placeable entity (Ori advice; EXOK's editor-in-project lesson).
  5. Room prefabs under version control so levels can be reverted (Metanet).
  6. Optionally, LDtk only if world-graph layout or autotiling in Unity becomes a bottleneck.

### Gaps
- The Celeste autotiler source and Noel Berry's tooling blog posts were not retrieved. No Noclip documentary or GMTK video specific to these tools was checked beyond the Spelunky transcript.
