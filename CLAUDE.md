# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Environment

- Unity `2022.3.62f3c1` (2022.3 LTS)
- Core DOTS packages: Entities `1.3.14`, Unity Physics `1.3.14`, Entities Graphics `1.3.2`
- Render pipeline: URP `14.0.12`
- Language: C# (4-space indent, LF line endings)

## Commands

Run tests from the Unity Editor (Window > General > Test Runner), or via batchmode:

```powershell
# EditMode tests
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe" `
  -batchmode -projectPath "." -runTests -testPlatform EditMode `
  -testResults "Logs\EditModeResults.xml" -logFile "Logs\EditModeBatch.log" -quit

# PlayMode tests (smoke tests + benchmarks)
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe" `
  -batchmode -projectPath "." -runTests -testPlatform PlayMode `
  -testResults "Logs\PlayModeResults.xml" -logFile "Logs\PlayModeBatch.log" -quit
```

Batchmode cannot run while the project is open in another Unity Editor instance. Use `-quit` to auto-close Unity after tests finish.

CI is manual-dispatch only by default (`.github/workflows/test.yml`). See README for CI setup instructions.

## Assemblies

| Assembly | Path | Key References |
|---|---|---|
| `UnityDotsDemo` | `Assets/Scripts/UnityDotsDemo.asmdef` | Entities, Entities.Hybrid, Collections, Mathematics, Transforms, Physics, Burst |
| `UnityDotsDemo.EditModeTests` | `Assets/Tests/EditMode/EditModeTests.asmdef` | UnityDotsDemo, Entities, Entities.Hybrid, Mathematics, Collections, TestRunner |
| `UnityDotsDemo.PlayModeTests` | `Assets/Tests/PlayMode/PlayModeTests.asmdef` | UnityDotsDemo, Entities, Collections, TestRunner |

The runtime assembly references `Unity.Entities.Hybrid` because `Baker<T>` is defined there in Entities 1.3.x.

## Directory Layout

| Path | Purpose |
|------|---------|
| `Assets/Scripts/UnityDotsDemo.asmdef` | Runtime assembly definition |
| `Assets/Scripts/DOTS/Demo01_MovingCubes/` | Demo01 ECS code |
| `Assets/Scripts/DOTS/Demo02_BouncingBalls/` | Demo02 ECS code |
| `Assets/Scripts/DOTS/Demo03_FlockingAgents/` | Demo03 ECS code |
| `Assets/Scripts/DOTS/Demo04_TowerDefense/` | Demo04 ECS code |
| `Assets/Scripts/DOTS/Demo05_Pathfinding/` | Demo05 ECS code |
| `Assets/Scripts/DOTS/Templates/DemoTemplate/` | Starter template for new demos |
| `Assets/Scripts/Shared/` | Shared runtime code (HUD, UI, common DOTS components) |
| `Assets/Editor/` | Editor setup scripts (Demo01-04 + DemoHub) |
| `Assets/DOTS_DemoAssets/Demo01-04/` | Demo prefabs, materials, setup markers |
| `Assets/Scenes/` | Main scenes, SubScenes, DemoHub |
| `Assets/Tests/EditMode/` | NUnit EditMode tests |
| `Assets/Tests/PlayMode/` | PlayMode smoke tests + benchmarks |
| `Documentation~/` | Benchmark template, interview guide, learning checklist, performance doc |

## Architecture

Every demo follows the same conversion pipeline:

```
MonoBehaviour Authoring → Baker<T> → IComponentData/IBufferElementData → ISystem/IJobEntity
```

Authoring GameObjects live inside SubScenes and bake into ECS entities at edit time. All four demos use one-shot spawner systems: the spawner entity self-destructs after instantiating entities via `EntityCommandBuffer`.

## Demo-Specific Architecture

### Demo01 — Moving Cubes
- `CubeSpawnerAuthoring` → `CubeSpawnerConfig` (one-shot spawner) → entities with `MoveSpeed`, `MoveDirection`, `WrapArea`
- `MoveSystem` is a `[BurstCompile] ISystem` with a nested `IJobEntity` that does position update + wrap-around. Wrap preserves overshoot to reduce boundary clumping.

### Demo02 — Bouncing Balls
- Uses Unity Physics (`PhysicsVelocity`, `PhysicsCollider`). The spawner sets random initial linear/angular velocity via `PhysicsVelocity`.
- `BallResetSystem` detects fallen balls (y below `ResetHeight`) and repositions them at the spawn area with new random velocity. Uses `[EntityIndexInQuery]` + per-frame tick hash for deterministic-but-distinct Random per entity.

### Demo03 — Flocking Agents
- Two simulation modes: **Basic** (brute-force neighbor sampling, 8 samples per entity) and **SpatialHash** (grid-based neighbor lookup via `NativeParallelMultiHashMap<int2, int>`).
- `BoidMovementSystem` runs in Basic mode; `SpatialHashBoidSystem` runs in SpatialHash mode. Both check a singleton `BoidSimulationModeData` component and early-out if wrong mode.
- **Important**: `SpatialHashBoidSystem` calls `state.Dependency.Complete()` before building the hash map — this sync point is required because the hash map is written by `SpatialHashInsertJob` and read by `SpatialHashBoidJob` on the main thread before scheduling.

### Demo04 — Tower Defense
- Mandatory system execution order (systems must run in this sequence):
  `TowerSpawn → WaveProgression → EnemySpawn → EnemyMovement → TowerTargeting → ProjectileMovement → Damage → Cleanup → BaseHealth → GameState`
- Uses `IBufferElementData` for `Waypoint` paths and `TowerSpawnPoint` positions, and `DamageEvent` as a buffer for damage accumulation.
- `WaveSpawnerSystem` spawns towers once (`TowersSpawned` byte flag), then spawns enemy waves. It reads `WaveDefinition` buffers for per-wave enemy type counts and stats.
- `GameStateComponent` holds a `GamePhase` enum (Playing/Victory/Defeat). The `GameStateSystem` checks win (all waves cleared, no enemies alive) and loss (base health ≤ 0) conditions.

### Demo05 — Flow Field Pathfinding
- `PathfindingAuthoring` bakes `FlowFieldGrid` + `PathTarget` + `DynamicBuffer<FlowFieldCell>` onto a singleton entity.
- `FlowFieldSystem` runs BFS from the target cell outward, populating each cell's `Direction` vector (pointing toward target) and `Cost` (distance). Uses `NativeList<int>` as a manual queue.
- `AgentSpawnerSystem` spawns agents in a circle around the grid center, then self-destructs.
- `AgentMovementSystem` is a `[BurstCompile] IJobEntity` that converts each agent's world position to a grid cell, reads the flow field direction via `BufferLookup<FlowFieldCell>`, and moves accordingly.
- Key algorithm: BFS propagation from target → gradient field → hundreds of agents follow the gradient in parallel without per-agent pathfinding.

## Key Conventions

- Always use `ISystem`, not `SystemBase`.
- Use `IJobEntity` for parallel entity iteration; fall back to `IJobChunk` only when chunk-level control is needed.
- Add `[BurstCompile]` to systems and jobs where the code path is Burst-compatible.
- Use `EntityCommandBuffer` for structural changes (instantiate, destroy, add/remove component).
- Use `LocalTransform` — never the deprecated `Translation`, `Rotation`, or `Scale` components.
- Baker nested classes must use a specific name (e.g., `CubeSpawnerBaker : Baker<CubeSpawnerAuthoring>`), not `Baker : Baker<T>`, to avoid shadowing the generic `Baker<T>` type.
- Namespaces: `UnityDotsDemo.Demo01`–`Demo05` for demo code, `DOTSDemo.Shared` for shared runtime code and common DOTS components (`MoveSpeed`, `Velocity`), `UnityDotsDemo.Template` for templates, `UnityDotsDemo.Tests` for tests.

## Editor Auto-Setup

Each demo has an `[InitializeOnLoad]` editor script (e.g., `Demo01MovingCubesSetup.cs`) that auto-creates the scene, SubScene, materials, prefabs, and URP pipeline on first project open. They write marker files under `Assets/DOTS_DemoAssets/DemoNN/.demoNN_setup_complete` to skip re-creation. Use menu `DOTS Demos > Rebuild Demo NN` to force rebuild. `DemoHubSetup.cs` also manages Build Settings scene list and Back Button injection.

## Unity Gotchas

- Do not delete `.meta` files — they contain GUIDs that scene/prefab references depend on.
- Do not commit generated/cache directories: `Library/`, `Temp/`, `obj/`, `.vs/`, `Logs/`, `UserSettings/`.
- Do not manually edit generated `.csproj` or `.sln` files — Unity regenerates them.
- Let Unity Package Manager finish resolving packages before changing scripts.
- Scene and SubScene references depend on `.meta` GUIDs. When scenes break, use `DOTS Demos > Rebuild Demo NN` to regenerate.

## Adding A Future Demo

1. Copy `Assets/Scripts/DOTS/Templates/DemoTemplate/`.
2. Rename to `Demo05_YourName`.
3. Update namespaces to `UnityDotsDemo.Demo05`.
4. Create an editor setup script following the Demo01 pattern in `Assets/Editor/`.
5. Create a main scene and SubScene under `Assets/Scenes/`.
6. Put authoring GameObjects inside the SubScene (baking is mandatory).
7. Add smoke tests and update README.

## Current State

- All four demos + DemoHub scene are implemented and tested.
- Known work remaining: re-run benchmarks at higher entity counts (5k, 10k, 50k), capture GPU rendering data with Unity Profiler, and complete the learning checklist tasks in `Documentation~/LEARNING_CHECKLIST.md`.

## .gitignore Note

The `.gitignore` excludes `.claude/` (Claude Code internal files). Claude Code's memory and settings won't be committed, but CLAUDE.md itself IS committed.
