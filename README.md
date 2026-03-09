# 🪨 Rock The Game

> A fast-paced, grappling-hook driven action platformer built with Unity.

Welcome to the **Rock The Game** repository! This project focuses on high-speed traversal, dynamic camera perspectives, and physics-based platforming mechanics. The core gameplay revolves around grappling hooks, momentum control, and navigating through hazard-filled environments.

---

## 🏃‍♂️ Core Mechanics & Traversal

The movement system is designed to be fluid and highly responsive, giving players advanced mobility options:

* **Grappling System:** Core mechanics implemented via `grapler` and `leftGrapler` scripts, allowing players to swing, pull, and maneuver through the air.
* **Advanced Mobility:** Features like `SpecialMove` and `SpeedControl` handle momentum, dashing, or time-manipulation abilities to keep the pacing fast.
* **Modern Inputs:** Utilizes the new **Unity Input System** (`Input.inputactions` & `Input.cs`) for robust, cross-platform controller and keyboard support.

---

## ⚙️ Systems & Architecture

The project is heavily modularized to handle complex platforming environments:

| System | Description | Key Classes |
| :--- | :--- | :--- |
| **Dynamic Camera** | Manages multi-perspective views, tracking, and smooth transitions. | `CameraController`, `SwitchCamera`, `followPlayer` |
| **Hazards & Traps** | Physics-based obstacles that react to the player. | `Spike`, `BouncySpike`, `ColliderHandler` |
| **Dynamic Platforms** | Environmental objects that require precise timing. | `fallablePlatform`, `jumpPad`, `door` |
| **Game Flow** | Manages level progression, timing, and win/loss states. | `GameCondition`, `checkpoint`, `Timer` |

---

## 🧩 Environment & Collectibles

* **Interactive Elements:** The world features interactable nodes (`Node.cs`), specific entities (like `whale.cs`), and objects that change properties dynamically (`ColorOrganizer.cs`).
* **Progression:** Players gather `Collectables` and `Ring` objects while racing against the `Timer` to reach the next `checkpoint`.

---

## 🛠 Technical Notes

* **Physics & Collisions:** The `ColliderHandler` acts as a centralized script to manage complex collision events between the fast-moving player and environmental hazards, preventing physics clipping.
* **State & Condition Management:** Game states are cleanly tracked using `GameCondition`, ensuring that level transitions, checkpoint respawns, and failure states trigger correctly without breaking the game loop.
