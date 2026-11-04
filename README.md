# Rock The Game

A fast, demanding 2D pixel-art platformer built around two independent grappling hooks. Swing, launch and climb through connected rooms where one hit kills you and respawn is instant.

**Unity 6 (URP) · C# · solo project · in development, about 22 of 60 planned rooms · PC, gamepad only**

## How it started

This began as my first Unity game, with a grapple built on Spring Joints. It taught me a lot, and it also showed me where built-in physics stops giving you precise control. I rebuilt the movement from scratch around a custom kinematic controller, so every acceleration and collision response is now under direct control.

## Mechanics

- **Hooks:** swing, dual swing and slingshot launches, plus rotating nodes that reward full rotations
- **Traversal:** dash, wall climb, wall slide, wall jump and ledge climb
- **Rooms:** moving and breakable platforms, launch pads, hazards, collectibles and keys

## Architecture

| Area | Approach | Key classes |
|:---|:---|:---|
| **Movement** | Kinematic controller with its own collision resolution and ground and wall sensing | `KinematicPhysicsHandler`, `PlayerController` |
| **Player logic** | State pattern, with one class per movement state | `IPlayerState`, `SwingingState`, `DualSwingingState`, `SlingshotState`, `DashState` |
| **Grapple** | Hook logic and node detection kept apart from visuals | `Grappler`, `NodeDetector`, `GrappleVisualManager`, `FullRotationTracker` |
| **Platforms** | Strategy pattern for movement paths, with interfaces for carrying the player | `MovementStrategy`, `CircularMovementStrategy`, `IMovingSurface`, `IPassenger` |
| **Rooms and respawn** | Coordinated room transitions and state reset on death | `RoomManager`, `RoomTransitionCoordinator`, `LevelResetManager`, `RespawnManager` |
| **Game feel** | Hit stop, gamepad rumble and afterimages | `HitStopManager`, `HapticManager`, `GhostAfterimage` |
| **Data and audio** | ScriptableObject tuning data and audio cues | `PlayerStatsSO`, `AudioCueSO`, `AudioManager` |

Gameplay code is in `Assets/Scripts/New-Scripts`.

## Tests and tools

- 22 EditMode tests in `Assets/Tests/EditMode` cover platform movement strategies, rotation tracking and tuning data sanity
- Custom editor tools include `RampTextureGeneratorWindow` and `PlatformStrategyConverterWindow`

## Running the project

Open the project in Unity 6000.3.10f1. A gamepad is required. The design document is in `docs/GDD.md` (in Turkish).
