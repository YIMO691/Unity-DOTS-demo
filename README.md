# Unity DOTS Demo Hub

[English](README.md) · [中文](README_CN.md)

High-performance Unity DOTS/ECS demonstrations — from basic entity movement to
full gameplay loops. Built for learning, interviews, and performance benchmarking.

Unity 2022.3 LTS · Entities 1.3.14 · Unity Physics 1.3.14 · Entities Graphics 1.3.2 · URP 14.0.12

## Demos

| # | Demo | Core Concepts | Scale | Key Pattern |
|---|------|--------------|-------|-------------|
| 01 | **Moving Cubes** | IJobEntity, Burst, ECB, boundary wrap | 1k–50k | Parallel entity update |
| 02 | **Bouncing Balls** | Unity Physics, PhysicsVelocity, collider baking | 100–1k | Physics + reset loop |
| 03 | **Flocking Agents** | Boids, Basic/SpatialHash modes, neighbor sampling | 500–10k | Parallel flocking |
| 04 | **Tower Defense** | Wave spawning, targeting, projectiles, win/loss | 100–500 | Gameplay loop + entity pool |
| 05 | **Flow Field** | BFS gradient, BufferLookup, Burst IJobEntity | 100–1k | Grid pathfinding |

All scenes are in `Assets/Scenes/`. Each has an auto-setup script under `Assets/Editor/`.

## Quick Start

1. Install **Unity 2022.3.62f3c1** (or compatible 2022.3 LTS).
2. `git clone https://github.com/YIMO691/Unity-DOTS-demo.git`
3. Open the project root folder with Unity Hub. Wait for Package Manager.
4. Auto-setup scripts run on first open — scenes, materials, and prefabs are created automatically.
5. Open `Assets/Scenes/DemoHub.unity` for the main menu, or any demo scene directly.
6. Press **Play**.

SubScene baking is mandatory — authoring GameObjects live inside SubScenes.
Wait for baking to finish before entering Play Mode.

## Architecture

Every demo follows the same pipeline:

```
MonoBehaviour Authoring  →  Baker<T>  →  IComponentData  →  ISystem / IJobEntity
```

Shared infrastructure in `Assets/Scripts/Shared/`:

| File | Purpose |
|------|---------|
| `CommonComponents.cs` | Shared `MoveSpeed` and `Velocity` structs |
| `SpawnerHelper.cs` | Disposable ECB wrapper for one-shot spawners |
| `GUIStyleHelper.cs` | Factory methods for common GUI styles |
| `DemoHUD.cs` / `DemoHubUI.cs` / `DemoBackButton.cs` | Runtime UI overlay + menu |

### Demo04 System Pipeline

```
TowerSpawn → WaveProgression → EnemySpawn → EnemyMovement → TowerTargeting
    → ProjectileMovement → Damage → Cleanup → BaseHealth → GameState
```

Entity pooling: enemies pre-spawn and reuse via `PooledEnemy` tag.
All 10 systems exclude pooled entities with `.WithNone<PooledEnemy>()`.

### Demo05 Flow Field

BFS propagates from target cell outward through a 40×40 grid. Each cell stores
a direction vector toward the target. 200+ agents read the gradient via
`BufferLookup<FlowFieldCell>` in a Burst-compiled `IJobEntity` — zero per-agent
pathfinding cost. Click and drag on the ground to move the target in real time.

## Performance

### Batchmode (Headless, CPU-Only)

| Demo | Entities | Frame Time | GC Alloc |
|------|----------|------------|----------|
| Moving Cubes | 1,000 | 1.22 ms | 0 B |
| Bouncing Balls | 200 | 1.23 ms | 0 B |
| Flocking (Basic) | 500 | 1.25 ms | 0 B |
| Flocking (SpatialHash) | 500 | 1.36 ms | 0 B |
| Tower Defense | 5 waves | 1.44 ms | 0 B |

### Editor Play Mode (with GPU Rendering)

| Demo | Entities | Avg FPS | Frame ms | GC/frame |
|------|----------|---------|----------|----------|
| Moving Cubes | 1,009 | 217 | 4.60 | 1.4 KB |
| Bouncing Balls | 219 | 218 | 4.59 | 1.2 KB |
| Flocking (Basic) | 515 | 212 | 4.71 | 1.7 KB |
| Flocking (SpatialHash) | 516 | 208 | 4.81 | 1.4 KB |
| Tower Defense | 64 peak | 193 | 5.19 | 2.4 KB |
| Flow Field | 211 | 171 | 5.84 | 0.9 KB |

GC in Play Mode is Editor/rendering overhead — DOTS ECS code itself produces zero allocation.
Full methodology and high-stress configs in [`Documentation~/Benchmark.md`](Documentation~/Benchmark.md).

## Tech Stack

| Technology | Role |
|------------|------|
| **ECS** (Entities 1.3.14) | Data-oriented architecture, chunk-based component storage |
| **C# Job System** | Multi-threaded parallel processing |
| **Burst Compiler** | Native code generation for Jobs |
| **Unity Physics** | DOTS-native collision and dynamics |
| **Entities Graphics** | GPU-instanced rendering for ECS entities |
| **SubScene Baking** | Authoring-to-entity conversion at edit time |
| **URP** | Universal Render Pipeline, Forward+ rendering |

## Project Structure

```
Assets/
├── DOTS_DemoAssets/Demo01–05/    Prefabs, materials, setup markers
├── Editor/
│   ├── DemoHubSetup.cs            Hub scene + back button injection
│   ├── Demo01MovingCubesSetup.cs  Auto-create scenes & assets
│   ├── Demo02BouncingBallsSetup.cs
│   ├── Demo03FlockingAgentsSetup.cs
│   ├── Demo04TowerDefenseSetup.cs
│   └── Demo05PathfindingSetup.cs
├── Scenes/Demo01–05 + DemoHub     Main scenes & SubScenes
├── Scripts/
│   ├── Shared/                    CommonComponents, SpawnerHelper, GUIStyleHelper, UI
│   └── DOTS/Demo01–05 + Templates ECS systems, components, authoring
├── Tests/
│   ├── EditMode/                  10 unit tests (components, configs, algorithms)
│   └── PlayMode/                  7 smoke tests + 6 benchmark tests
└── Settings/                      URP pipeline assets
```

## Testing

**17 tests** — all passing.

```powershell
# EditMode (headless)
& "<Unity>/Editor/Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -quit

# PlayMode (headless)
& "<Unity>/Editor/Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -quit
```

Or use **Window > General > Test Runner** in the Unity Editor.

## Contributing

- Prefer `ISystem` and `IJobEntity`. Use `[BurstCompile]` where the code path allows.
- Use `EntityCommandBuffer` for structural changes. Use `LocalTransform` (not legacy `Transform`).
- Keep components small. Convert authoring data with `Baker<T>`.
- Verify the target demo runs before opening a PR.

## Documentation

| Document | Content |
|----------|---------|
| [`CLAUDE.md`](CLAUDE.md) | AI assistant guidance and architecture reference |
| [`CHANGELOG.md`](CHANGELOG.md) | Version history |
| [`Documentation~/Benchmark.md`](Documentation~/Benchmark.md) | Methodology, batchmode + Play Mode results, high-stress guide |
| [`Documentation~/Interview_Guide.md`](Documentation~/Interview_Guide.md) | Interview talking points |
| [`Documentation~/LEARNING_CHECKLIST.md`](Documentation~/LEARNING_CHECKLIST.md) | Hands-on learning exercises |
| [`Documentation~/DOTS_Performance_Optimization.md`](Documentation~/DOTS_Performance_Optimization.md) | DOTS performance theory (中文) |

## License

MIT — see [LICENSE](LICENSE).
