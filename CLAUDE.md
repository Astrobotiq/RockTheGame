# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**Rock The Game** — a fast-paced, grappling-hook driven 2D action platformer built in Unity 6000.3.10f1 (URP, 2D feature set). Core gameplay revolves around a dual-arm grappling system, momentum-based movement, and physics-based platforming through hazard-filled rooms.

## Commands

This is a Unity project with no CLI build/test/lint pipeline — there is no `package.json`, Makefile, or CI config. All development happens through the Unity Editor:

- **Open/run:** Open the project in Unity Editor `6000.3.10f1` (see `ProjectSettings/ProjectVersion.txt`) and press Play. There is no headless run script in this repo.
- **Compile check:** Use the Unity Editor's Console window, or `mcp__ide__getDiagnostics` if available, to check for compile errors after editing scripts.
- **Tests:** `com.unity.test-framework` is a package dependency, but there are no actual test assemblies or `*Tests.asmdef` in the repo — testing is done manually via Play Mode. Don't assume `dotnet test` or similar works here.
- **No `.asmdef` files exist** — all scripts compile into the default `Assembly-CSharp`.

## Architecture

### Two coexisting codebases

The repo contains two generations of gameplay code:

- `Assets/Scripts/New-Scripts/` — the **active, current** system (namespace `New_Scripts.*`). All work referenced in recent commits/diffs happens here. Organized by feature folder: `Player/`, `Platform/`, `LevelChange/`, `Death/`, `Collectible/`, `Door/`, `NPC/`, `Audio/`, `UI/`, `Animation/`, `Editor/`.
- `Assets/Scripts/` (top level, no namespace) — **legacy** scripts from an earlier prototype (`grapler.cs`, `leftGrapler.cs`, `fallablePlatform.cs`, `jumpPad.cs`, `GameCondition.cs`, `checkpoint.cs`, etc., matching the README's description). Treat these as superseded unless a scene explicitly still uses them — don't extend this system for new work; use `New-Scripts` equivalents instead (e.g. `Grappler.cs`, `LaunchPad.cs`, `Checkpoint.cs`).

The root `README.md` describes the **legacy** system's class names and is out of date relative to `New-Scripts`; prefer reading `New-Scripts` code directly over trusting the README for architecture.

### Player: finite state machine over a shared context

`PlayerController` (`Player/PlayerController.cs`) is the FSM context: it owns physics/input/audio/VFX component references, exposes shared mutable state (velocity, dash/slingshot/wall-stamina resources, anchors for the dual grapple arms), and drives `Update`/`FixedUpdate` by delegating to `CurrentState`. States implement `IPlayerState` (`EnterState`/`UpdateState`/`FixedUpdateState`/`ExitState`) and live in `Player/States/`: `GroundedState`, `AirborneState`, `DashState`, `WallClimbingState`, `WallSlidingState`, `SwingingState`, `DualSwingingState`, `LedgeClimbState`, `SlingshotState`, `PlayerTransitionState`. States transition each other via `PlayerController.TransitionToState(new XState(this, ...))`.

Key supporting pieces:
- `KinematicPhysicsHandler` resolves the FSM's intended velocity against collisions each `FixedUpdate` (kinematic Rigidbody2D, not dynamic) and reports back the resolved velocity.
- `PlayerStatsSO` is the single ScriptableObject data source for tuning values (speeds, timers, thresholds) — prefer adding new tunables there over hardcoding constants in states.
- `NodeDetector` + `Grappler`/`ArmController` implement grapple-target detection and the two-armed swing/pull mechanic (`LeftArm`/`RightArm`, `LeftAnchor`/`RightAnchor`).
- `HitStopEvents` (static C# events) drive brief pause-on-impact (i-frame/hitstop) independent of the FSM; `PlayerController` subscribes in `OnEnable`/`OnDisable`.
- Haptics run through `HapticManager` + per-system `*VibrationSettingsSO` assets (`PlayerVibrationSettingsSO`, `PlatformVibrationSettingsSO`, `TransitionVibrationSettingsSO`) and `RumbleProfile` — new feedback should follow this SO-per-system pattern rather than hardcoding rumble values.

### Rooms and level transitions

`RoomManager` holds all `Room`s and switches between them (`SetAsCurrent`/`SetAsNeighbor`/`Sleep`) based on adjacency (`Room.NeighborRooms`), so only the current room and its neighbors are active. `RoomTransitionCoordinator` + `RoomTransitionTrigger` + `TransitionDirection` drive the actual transition sequence (camera, player freeze/unfreeze via `PlayerController.FreezeForTransition`/`UnfreezeFromTransition`, and `ICameraTransitionHandler`/`IPlayerTransitionable` interfaces). `LevelBootstrapper` initializes a level/room on scene load.

### Event-driven, ScriptableObject-based systems

Audio and cross-system signals follow a channel-SO pattern rather than direct references: e.g. `AudioCuePlayEventChannelSO`/`AudioCueStopEventChannelSO` (raised to trigger sounds, consumed by `AudioManager`/`RoomAudioController`) and `TransformEventChannelSO` (Death system). When adding a new decoupled signal between systems, prefer adding an `EventChannelSO` over a direct component reference or static event, consistent with existing ones.

Similarly, most systems keep their tunable/data payloads in ScriptableObjects rather than inspector fields on MonoBehaviours: `PlayerStatsSO`, `CollectibleInventorySO`, `AudioCueSO`, `PlayerAudioDataSO`, the `*VibrationSettingsSO` family. Follow this when adding new configurable systems.

### Platforms

`Platform/` implements moving/breakable/launch platforms via a strategy pattern: `MovementStrategy` subclasses (`LinearPingPongStrategy`, `CircularMovementStrategy`, `AcceleratingPingPongStrategy`, `TransformPingPongStrategy`) plug into `MovingPlatform`/`PlatformController`, with `IWaypointPath`/`LinearWaypointPath` supplying paths and `IPassenger`/`IMovingSurface` used for carrying the player. Breakable platforms come in two forms: the simpler `BreakablePlatform`, and the newer segmented system `SegmentedBreakablePlatform` + `BreakableBlockUnit` (with a custom `SegmentedBreakablePlatformEditor` for editor-time authoring) — prefer the segmented system for new breakable platform work, as it's the actively developed one.

### Death, respawn, checkpoints

`PlayerHealth` + `IKillable` handle player death; `DeathZone`/`AcidHazard` are hazard triggers. `Checkpoint` + `RespawnManager` + `LevelResetManager` (implementing `IResettable` on resettable objects) coordinate respawn state, using `TransformEventChannelSO` to communicate the respawn point.

### Async

`Cysharp.Threading.Tasks` (UniTask) is a project dependency — prefer `UniTask`/`UniTaskVoid` over coroutines for new async sequencing code (transitions, respawns, etc.) to match existing usage (see `RoomTransitionCoordinator`, `PlayerController` imports).

### Comments and naming

Existing code mixes Turkish-language XML doc comments/summaries with English identifiers (see `PlayerController`, `RoomManager`). Match the surrounding file's language when adding comments rather than forcing consistency across the codebase.
