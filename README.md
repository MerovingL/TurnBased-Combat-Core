# Turn-Based Combat Core — Architecture Sample

Core Turn-Based Battle System extracted from the commercial RPG project **"The Prophecy of Horn"** (released on VK Play, coming to Steam).

This repository demonstrates how a monolithic production combat manager was decoupled into a clean, testable, asynchronous state machine using interfaces, adapters, and serialized interface references resolved at Awake — without breaking existing game architecture.

## Repository Structure

* **`/TurnBasedSystem6Awaitable`** — Built for **Unity 6** using the native `Awaitable` API.
* **`/TurnBasedSystem22Unitask`** — Built for **Unity 2022.3 LTS** using **Cysharp UniTask**.

*Note: Both projects include mock components and dummy scenes configured for clean compilation and inspection without external asset dependencies.*

## Key Architectural Highlights

* **Non-Blocking Async State Machine:** Replaces legacy Coroutines with an explicit `while` loop state machine (`PEACE`, `START`, `AIPrepare`, `AI`, `HeroTurns`, `WIN`, `LOSE`), preventing deep call stacks and simplifying flow control.
* **Concurrent Wave-Based AI Execution:** Groups enemies by unit type (e.g., large units vs. standard units) and awaits each wave simultaneously (`WhenAll`), allowing up to 100 units to act smoothly without frame drops.
* **Decoupled via Interfaces & Adapters:** Isolates core combat logic from legacy singletons (`GlobalData`, `Sound`, `HudManager`) using clean abstractions (`IHeroParty`, `IDeveloperSettings`, `ISaveLoadSystem`, etc.) resolved via serialized interface references at Awake.
* **Safe Lifecycle & Error Handling:** Wraps the async execution loop in `try/catch` blocks to prevent silent state freezes and ties execution strictly to the object destruction token (`destroyCancellationToken`).
* **Defensive Collection Iteration:** Prevents `InvalidOperationException` during dynamic mid-combat list modifications via safe copying (`_actors`, `_enemies`, `_bosses`).

## Known Limitations & Design Trade-offs

* **Party Size Constraint:** Turn logic (`Hero00`, `Hero01`, `Hero02`) is explicitly structured for a fixed 3-character party, prioritizing shipping speed and specific UI/camera bindings over dynamic N-sized party scaling.
* **Target Detection Monolith:** `TargList` handles spatial queries and NavMesh obstacle toggling in a single pass for simplicity; in a larger-scale project, this would be extracted into a dedicated spatial partitioning service.

## How to Inspect

1. Clone the repository.
2. Open either folder via **Unity Hub** (matching the respective Unity version).
3. Open the main sample scene and inspect the `BattleManager` and adapter bindings in the Hierarchy.
