# Unity 6 (6000.3) 2D level-building tooling ecosystem: built-ins, external editors, productivity assets, validation, fast iteration

Context for the report writer: Rock The Game builds rooms out of sprite-block prefabs (no Tilemap yet). The player is a kinematic Rigidbody2D with a custom collision resolver (`KinematicPhysicsHandler`), and the project uses static C# events (`HitStopEvents`). Research date: 2026-11-18. I used about 15 tool calls, so some items below have only partial verification. Those are flagged.

## Unity built-ins: Tilemap/Tilemap Extras, Sprite Shape, Composite Collider, editor extension APIs, Search, Play Mode options, and what changed in Unity 6 for 2D

### Takeaway
Tilemap plus 2D Tilemap Extras (Rule Tile, GameObject Brush, Random/Line/Group brushes, Animated Tile) is the cheapest big win and has no extra cost. Unity 6.3 itself added almost nothing tilemap-specific. Its 2D headline features are Box2D v3 low-level physics APIs and "render 3D as 2D". Its editor headline features are dynamic Overlays, a new Hierarchy window, toolbar customization, and an LMDB-based Search index.

### Cited Findings
- 2D Tilemap Extras version 6.0.1 is released for Unity 6000.3. — [Unity 6.3 manual, Tilemap Extras](https://docs.unity3d.com/Manual/com.unity.2d.tilemap.extras)
- Tilemap Extras (current docs, v9.0.1 for 6000.6) contains:
  - Brushes: GameObject Brush, Group Brush, Line Brush, Random Brush.
  - Tiles: Animated Tile, Auto Tile, Rule Tile, Rule Override Tile.
  - Other: Grid Information and Custom Rules for Rule Tile.

  — [Unity docs, com.unity.2d.tilemap.extras](https://docs.unity.com/en-us/engine/current/manual/packages-list/packages-all/pack-safe/com-unity-2d-tilemap-extras)
- Tilemap changes in the 6000.3 alpha notes:
  - The Tilemap Inspector now warns when a Tilemap is not attached to an enabled Grid.
  - GameObject instantiation from Tiles is disabled in that case.
  - The recursion limit for instantiating GameObjects from Tiles dropped from 11 to 6.

  — [Unity 6000.3.0a3 release notes](https://unity.com/releases/editor/alpha/6000.3.0a3) (alpha-stage notes; I did not confirm they reached the final notes)
- New 2D features in Unity 6.3:
  - Low-level 2D physics APIs: a Box2D v3 integration in `UnityEngine.LowLevelPhysics2D`, with `PhysicsWorld` as the entry point. Unity cites multi-threading, better determinism, and visual debugging/gizmos.
  - "Render 3D as 2D": Mesh Renderers can take 2D lights, interact with Sprite Masks, and sort with sprites.

  — [New in Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/manual/whats-new/unity63)
- Editor workflow changes in Unity 6.3:
  - Dynamic Overlays can be collapsed, expanded, resized, dragged to the toolbar, or docked to any Scene view edge.
  - A new Hierarchy window (preview, enabled in Preferences > General).
  - Toolbar customization with saved presets, and Quality level switching from the toolbar.
  - `EditorDialog` dialogs with opt-out checkboxes.

  — [New in Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/manual/whats-new/unity63)
- Search indexing in Unity 6.3:
  - A new LMDB back end with a single index at `UserSettings/Search.index`. Custom indices are ignored and the Search Index Manager is removed.
  - Dependencies, Types, and Properties are always indexed.
  - "Deep scene and prefab indexing" is off by default.

  — [New in Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/manual/whats-new/unity63)
- Tilemap Collider 2D adds collision to tiles. An optional Composite Collider 2D merges neighbouring tile shapes and reduces physics work. When a composite is used, most of the Tilemap Collider's properties are controlled by the Composite instead. — [Unity 6.3 manual: Enable collision detection for tiles](https://docs.unity.com/en-us/engine/6000.3/manual/unity2d/tilemaps/tilemap-collider-2d/2d.md)
- Collider shape updates are batched in LateUpdate by default, and there is an API to force immediate updates. TilemapCollider2D exposes `Raycast` and `Cast` methods. — [TilemapCollider2D scripting reference, 6000.0](https://docs.unity3d.com/kr/6000.0/ScriptReference/Tilemaps.TilemapCollider2D.html)
- In one forum case, a tilemap with a kinematic RB and a Composite Collider showed no hitboxes because the tiles were too small to generate shapes. — [Unity Discussions](https://discussions.unity.com/t/tilemap-collider-2d-doesnt-work-for-me/1541428)
- Enter Play Mode without domain reload:
  - Static fields and static event handlers persist between Play sessions, and Unity does not unregister static event handlers on exit.
  - Runtime code must reset statics with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
  - With domain reload disabled, `OnDisable` is skipped. With scene reload disabled, `OnDestroy` is skipped.

  — [Unity 6.0 manual: domain reloading](https://docs.unity3d.com/6/Documentation/Manual/domain-reloading.html); [Unity 6.3 manual](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/code-reloading-editor/domain-reloading)
- The sources disagree on the default:
  - The 6.6 manual says the Editor does not domain-reload on entering Play mode by default.
  - The 6.5 manual says it does by default.
  - 6.5+ adds static-cleanup attributes, plus an analyzer that is enabled when Fast Enter Play Mode is on.

  — [Unity 6.6 manual](https://docs.unity3d.com/Manual/domain-reloading.html); [Unity 6.5 manual](https://docs.unity3d.com/6000.5/Documentation/Manual/domain-reloading.html). Neither applies directly to 6000.3, so check Project Settings > Editor in the installed version.

### Inferences
- **Tilemap fit for this project.** Rooms are sprite-block prefabs. The GameObject Brush, which paints prefabs onto a grid, lets the team keep the existing block prefabs and their behaviour, such as breakable or moving blocks, while gaining grid painting. This needs no change to the collision resolver. Static terrain could move to Tilemap + Rule Tile + TilemapCollider2D + CompositeCollider2D.
- **Collider check before migrating terrain.** `KinematicPhysicsHandler` must be checked to see how it queries geometry (`Rigidbody2D.Cast` or `Physics2D.*Cast` against layers). The Composite's geometry type matters: Outlines produces edge-like colliders that have no inside, while Polygons produces solid shapes. Outlines can change cast and overlap behaviour for a custom resolver. I did not verify this in this session, so it should be tested in a PhysicsTest scene first.
- **Box2D v3 low-level API.** It is not relevant for level building. Adopting it would be a physics rewrite, so treat it as out of scope.
- **Domain reload risk.** `HitStopEvents` uses static C# events, and other statics may exist. Disabling domain reload would require `SubsystemRegistration` resets, or subscriber leaks and stale state will appear. The setting is moderately risky but costs nothing, and a quick audit is needed first.
- **Editor extension APIs** (from prior knowledge, not re-fetched this session): EditorTool, Overlay/ToolbarOverlay, ShortcutManager `[Shortcut]`, Handles, Scene view context menus, PrefabStage, Scene Templates, and Presets all exist in Unity 6. They are the building blocks for a small custom "room block placer" overlay if the brushes are not enough. Scene Templates suit a "new room" skeleton (Room root, camera bounds, transition triggers, checkpoint). Presets suit default component settings on new blocks.

### Gaps
- I did not fetch the 6000.3 manual pages for Overlays API, EditorTool, ShortcutManager, Scene Template, Presets, increment snapping, or Sprite Shape, so there are no specific citations for their 6.3 behaviour.
- I found no full list of 6.3 tilemap or Sprite Shape changes.
- Whether Fast Enter Play Mode is the default in 6000.3 remains unresolved because the sources conflict.

## External editors and Unity importers: LDtk + LDtkToUnity, Tiled + SuperTiled2Unity, Ogmo 3, others

### Takeaway
LDtk + LDtkToUnity is the strongest external option. The importer is MIT-licensed, actively maintained (6.12.3, April 2026), re-imports when LDtk saves, and replaces entities with prefabs. However, LDtk itself has had no release since 1.5.3 (January 2024). Tiled is maintained (1.11.2, January 2025), and SuperTiled2Unity reportedly works on Unity 6. Ogmo 3 has no maintained Unity importer that I could find. Every external option means re-authoring the existing hand-made rooms.

### Cited Findings
- LDtkToUnity (Cammin):
  - About 428 stars and about 2,848 commits, MIT license, Unity 2019.3+ badge.
  - Features: entity prefab replacements, collision options, animated tiles, tilesets packed into a SpriteAtlas, and custom post-import scripting.
  - It re-imports when the LDtk project is saved.

  — [GitHub Cammin/LDtkToUnity](https://github.com/Cammin/LDtkToUnity)
- The latest LDtkToUnity release is 6.12.3 (April 10, 2026). — [GitPedia mirror](https://gitpedia.pages.dev/wiki/Cammin/LDtkToUnity) (aggregator; GitHub Releases did not render for me). It is also available on OpenUPM as `com.cammin.ldtkunity`. — [OpenUPM](https://openupm.com/packages/com.cammin.ldtkunity/)
- The itch.io page for LDtkToUnity is stale (last devlog 5.0.0, May 2024) and recommends installing from GitHub. — [itch.io](https://cammin.itch.io/ldtk-to-unity)
- The LDtk editor's latest stable release I found is 1.5.3 (January 15, 2024). A JSON page previews an "upcoming" 1.5.4. — [GitHub deepnight/ldtk releases](https://github.com/deepnight/ldtk/releases/latest); [LDtk JSON next](https://ldtk.io/json/next). LDtk 1.5 added automatic auto-layer rule sizing (1x1 to 9x9). — [GameFromScratch](https://gamefromscratch.com/ldtk-1-5-released/)
- Tiled releases:
  - 1.11: June 27, 2024 (scripting API, automapping, export plugins).
  - 1.11.1: January 11, 2025 (direct Aseprite loading).
  - 1.11.2: January 28, 2025 (crash fixes).

  — [mapeditor.org 1.11](https://www.mapeditor.org/2024/06/27/tiled-1-11-released.html); [1.11.1](https://mapeditor.org/2025/01/11/tiled-1-11-1-released.html); [1.11.2](https://thorbjorn.itch.io/tiled/devlog/876392/tiled-1112-released)
- SuperTiled2Unity:
  - The developer said in itch.io comments that it is compatible with Unity 6 and the latest Tiled.
  - The latest version I found is 2.4.0, and the itch.io page was updated July 1, 2025.
  - One user reported a "missing dependencies" error on 2.2.4 with 6000.0.36f1.
  - It can be installed from disk via package.json.

  — [itch.io SuperTiled2Unity](https://seanba.itch.io/supertiled2unity); [OpenUPM](https://openupm.com/packages/com.seanba.super-tiled2unity/) (no versioned release note confirms Unity 6)
- Ogmo Editor 3: my search found no Unity importer at all. — (negative result; search "Ogmo Editor 3 Unity importer maintained")

### Inferences
- **Round-trip workflow.** LDtk and Tiled are the source of truth, and Unity re-imports on save. Hand edits to imported room prefabs are lost on re-import unless they are made through postprocessor scripts or as overrides in a wrapper prefab. This conflicts with the project's habit of authoring rooms directly in scenes and prefabs.
- **Entity mapping.** In LDtkToUnity, LDtk entities (spiral nodes, launch pads, checkpoints, doors, keys, moving platforms with waypoint fields) map to prefabs via "entity prefab replacements", with fields passed to components through post-processing. The rooms' `Room`/`RoomManager` adjacency, transition triggers, and camera bounds would need a postprocessor. LDtk's world/level layout could feed `NeighborRooms`.
- **Migration cost.** No tool imports existing Unity scenes back into LDtk or Tiled, so every hand-made room would have to be rebuilt in the external editor. For a project mid-production with dozens of rooms, this is high cost.
- **Risk.** LDtk's stalled core development (no release in about 2 years) is a medium long-term risk. The importer is healthy.
- **Recommendation.** In-editor tooling (Tilemap + GameObject Brush + Scene Template + small custom Overlay) is lower risk than an external editor for this project. LDtk is worth a one-room spike only if level-design throughput is the bottleneck and new rooms can be built from scratch.

### Gaps
- I could not see LDtkToUnity's exact GitHub release list or dates (the Releases section did not render).
- I did not verify whether LDtk had any 2025–2026 release beyond 1.5.3.
- I found no current Unity-6-specific issue lists for either importer.
- No other editors (Sprytile, Pyxel Edit, Edgar for Unity procedural rooms) were researched.

## Asset Store / OSS editor productivity tools: which are worth it for level design

### Takeaway
Hot Reload is a one-time per-seat purchase that patches method bodies, fields, and signatures without a domain reload. It is the strongest iteration accelerator for tuning platforming code. Odin is the heaviest option, at €50 personal or €235/yr enterprise, and its Validator is the main reason to buy it for level design. Free attribute libraries (NaughtyAttributes, Tri-Inspector) cover inspector polish. I did not verify ALINE, Shapes, Editor Console Pro, Vertx, or Recorder in this session.

### Cited Findings
- Hot Reload: "Buy once, use forever", priced per seat at $39.99 and $79.99 excluding VAT, with Indie and Business tiers. Companies on Unity Pro/Enterprise with more than 10 employees need Business. There is a free evaluation, but the terms were not shown. — [hotreload.net/pricing](https://hotreload.net/pricing)
- Hot Reload applies changes without a domain reload, compiling only the changed code. — [hotreload.net](https://hotreload.net/)
- Hot Reload supported changes:
  - Method bodies, lambdas, and properties.
  - Adding, removing, or editing fields, methods, and parameters.
  - Changing return types, static, or async.
  - Generics and Burst code.

  — [Hot Reload features docs](https://hotreload.net/documentation/features)
- Hot Reload unsupported changes:
  - Adding or removing attributes, adding classes, structs, or enums, and adding new C# files.
  - Changing `virtual`, `override`, or `abstract`.
  - Coroutines or async methods that are already running.
  - Any `.asmdef` or define change requires a full recompile.

  — [Hot Reload features docs](https://hotreload.net/documentation/features)
- Odin licensing:
  - Personal: €50 for Inspector + Serializer. A cart page shows €40 for add-ons; reading that as the Validator price is my inference.
  - Enterprise: €235/seat/yr. An older source says $250, and Unity Store shows $250.
  - Enterprise includes source and the Project Validator.
  - Enterprise is required if revenue or funding is at least $200k in the last 12 months.
  - Licenses are per seat. Students get free Inspector + Validator.

  — [odininspector.com/pricing](https://odininspector.com/pricing); [Unity Odin page](https://unity.com/products/odin); [Unity support: educational licenses](https://support.unity.com/hc/en-us/articles/29430387545108-How-do-I-access-the-free-Odin-Inspector-and-Odin-Validator-educational-licenses)
- Tri-Inspector (codewriter-packages): free, on OpenUPM and npm, minimum Unity 2020.3. The latest indexed version is 1.15.1, and the last push was around March 2025 per an indexer that may be stale. — [GitHub Tri-Inspector](https://github.com/codewriter-packages/Tri-Inspector); [OpenUPM](https://openupm.com/packages/com.codewriter.triinspector/); [ecosyste.ms](https://awesome.ecosyste.ms/projects/github.com%2Fcodewriter-packages%2FTri-Inspector)
- NaughtyAttributes (dbrizov): free and on the Asset Store. I found no last-commit date or explicit Unity 6 claim. — [Asset Store](https://assetstore.unity.com/packages/p/naughtyattributes-129996)

### Inferences
- **Hot Reload** has high fit, about $40–80 cost, and low risk. Its limits (no new classes or attributes, no `.asmdef` changes) fit tuning state logic in `New-Scripts`. It does not help with tuning values in `PlayerStatsSO`, which already apply live in Play mode. It has the same static-state caveats as disabled domain reload.
- **Odin Inspector** has medium fit and €50+ cost. It mostly helps editor UX for authoring (buttons, validation attributes). Its value for level design comes largely from the Validator (next section).
- **Tri-Inspector and NaughtyAttributes** are free, low-risk ways to get `[Button]` and validation-style attributes on room/platform components. Adding either means another asmdef reference in `RockTheGame.Runtime.asmdef`.
- **ALINE and Shapes** (paid gizmo/drawing libraries), **Editor Console Pro**, **Vertx debugging**, **Unity Recorder** (free Unity package for capturing playtest video), and the **Smart Scene tools** category were not researched this session. Recorder is the only one with obvious level-design value (recording playtests for review).

### Gaps
- I did not verify current price, Unity 6 compatibility, or maintenance status for ALINE, Shapes, Editor Console Pro, Vertx, Unity Recorder, or Smart Scene tools.
- Hot Reload's Unity version matrix and the terms of its free trial were not visible.
- I could not confirm which Hot Reload price maps to which tier.

## Validation tooling: Odin Validator, Project Validation, Unity Test Framework EditMode scene checks

### Takeaway
Odin Validator is the off-the-shelf option, bundled in Enterprise and an add-on for Personal. For a solo or small project that already runs EditMode tests via the CLI and a Stop hook, custom EditMode tests that open room prefabs or scenes and assert invariants are the cheapest validation and fit the existing pipeline.

### Cited Findings
- The Odin Enterprise license includes the Odin Project Validator. — [Odin enterprise announcement](https://odininspector.com/blog/enterprise-announcement); [odininspector.com/pricing](https://odininspector.com/pricing)
- Students get free Odin Validator licenses. — [Unity support article](https://support.unity.com/hc/en-us/articles/29430387545108-How-do-I-access-the-free-Odin-Inspector-and-Odin-Validator-educational-licenses)
- In Unity 6.3, Search always indexes Dependencies, Types, and Properties, and deep scene/prefab indexing is optional. This makes Search queries usable for finding things like "all prefabs with component X". — [New in Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/manual/whats-new/unity63)

### Inferences
- **Room invariants to test in EditMode** (each prefab opened via `PrefabUtility.LoadPrefabContents`):
  - Every Room has camera bounds.
  - Neighbor links are symmetric.
  - Every RoomTransitionTrigger has a direction.
  - There are no missing scripts.
  - Every block sits on the grid (positions are multiples of the cell size).
  - DeathZones are on the right layer.
  - No `IResettable` platform lacks a reference.
- **Fit with the existing pipeline.** These tests run via `unity command run_tests --mode EditMode` and would be enforced by the existing Stop hook at zero cost. This is a stronger fit than buying Odin Validator.
- **Unity Project Validation** (the Project Settings validation framework used by XR/Core) was not researched. My prior knowledge suggests it is aimed at package and settings rules, not scene content.

### Gaps
- I found no source on the details of Unity's built-in Project Validation API or its suitability for scene content.
- I did not fetch Odin Validator's feature list (rule types, scene scanning, CI support).

## Fast iteration: Hot Reload, disabled domain reload, Multiplayer Play Mode

### Takeaway
Disabled domain reload (free) and Hot Reload (paid) are the two real accelerators, and both need static-state hygiene. Multiplayer Play Mode is irrelevant for a single-player game.

### Cited Findings
- With domain reload disabled, statics and static event handlers persist, and runtime code must reset them with `RuntimeInitializeOnLoadMethod(SubsystemRegistration)`. — [Unity 6.0 manual: domain reloading](https://docs.unity3d.com/6/Documentation/Manual/domain-reloading.html)
- Unity 6.5+ adds static-cleanup attributes and an analyzer for Fast Enter Play Mode. These are not in 6000.3. — [Unity 6.5 manual](https://docs.unity3d.com/6000.5/Documentation/Manual/domain-reloading.html)
- Hot Reload patches running code without a domain reload, but running coroutines and async methods keep their old code. — [Hot Reload features docs](https://hotreload.net/documentation/features)

### Inferences
- **Order of adoption.** First, audit static fields and events: `HitStopEvents`, any singletons, UniTask static state, and DOTween, which has its own static manager. Then enable Enter Play Mode Options with domain reload off (and possibly scene reload off) and run the PlayMode player-rule tests to catch leaks. After that, consider Hot Reload.
- **Multiplayer Play Mode** is not applicable because the game is single-player. Skip it.

### Gaps
- I found no measured data on Play-mode entry time savings for a project of this size.
- I did not verify DOTween's or UniTask's behaviour with domain reload disabled in this session.
