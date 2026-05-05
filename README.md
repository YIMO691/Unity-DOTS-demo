# Unity DOTS Demo Hub

High-performance Unity DOTS/ECS demonstrations featuring Jobs, Burst,
Unity Physics, and Entities Graphics.

Unity 2022.3 LTS · Entities 1.3.14 · URP 14.0.12

## Demos

| # | Demo | Scene Path | Core DOTS Concepts | Entity Scale | Status |
|---|------|------------|-------------------|--------------|--------|
| 01 | Moving Cubes | `Assets/Scenes/Demo01_MovingCubes/` | `IJobEntity`, Burst, ECB, boundary wrap | 1k-50k | Done |
| 02 | Bouncing Balls | `Assets/Scenes/Demo02_BouncingBalls/` | Unity Physics, `PhysicsVelocity`, collider baking, reset loop | 100-1k | Done |
| 03 | Flocking Agents | `Assets/Scenes/Demo03_FlockingAgents/` | Boids, Basic/SpatialHash modes, separation/alignment/cohesion | 500-10k | Done |
| 04 | Tower Defense | `Assets/Scenes/Demo04_TowerDefense/` | ECS gameplay loop, waves, targeting, projectiles, win/lose state | 100-500 | Done |
| 05 | Flow Field | `Assets/Scenes/Demo05_Pathfinding/` | BFS flow field, parallel agent movement, grid-based pathfinding | 100-500 | Done |

## Quick Start

1. Install Unity `2022.3.62f3c1` or a compatible `2022.3 LTS` editor.
2. Clone: `git clone https://github.com/YIMO691/Unity-DOTS-demo.git`
3. Open the project root folder with Unity Hub.
4. Open `Assets/Scenes/DemoHub.unity` for the main menu, or any demo scene directly.
5. Press Play.

Each demo depends on SubScene baking. If you duplicate or create scenes, keep the authoring GameObjects inside the related SubScene and wait for baking to finish before entering Play mode.

## Project Structure

```text
Assets/
|-- DOTS_DemoAssets/
|   |-- Demo01/
|   |-- Demo02/
|   |-- Demo03/
|   |-- Demo04/
|   `-- Demo05/
|-- Editor/
|   |-- DemoHubSetup.cs
|   |-- Demo01MovingCubesSetup.cs
|   |-- Demo02BouncingBallsSetup.cs
|   |-- Demo03FlockingAgentsSetup.cs
|   `-- Demo04TowerDefenseSetup.cs
|-- Scenes/
|   |-- DemoHub.unity
|   |-- Demo01_MovingCubes/
|   |-- Demo02_BouncingBalls/
|   |-- Demo03_FlockingAgents/
|   |-- Demo04_TowerDefense/
|   |-- Demo05_Pathfinding/
|   |-- Demo01_MovingCubes.unity
|   |-- Demo02_BouncingBalls.unity
|   |-- Demo03_FlockingAgents.unity
|   `-- Demo04_TowerDefense.unity
|-- Scripts/
|   |-- Shared/
|   |   |-- DemoHUD.cs
|   |   |-- DemoHubUI.cs
|   |   `-- DemoBackButton.cs
|   `-- DOTS/
|       |-- Demo01_MovingCubes/
|       |-- Demo02_BouncingBalls/
|       |-- Demo03_FlockingAgents/
|       |-- Demo04_TowerDefense/
|       |-- Demo05_Pathfinding/
|       `-- Templates/
`-- Settings/
```

## Tech Stack

- ECS (Entity Component System) - data-oriented architecture
- C# Job System - multi-threaded parallel processing
- Burst Compiler - high-performance native code generation
- Unity Physics - DOTS-native physics simulation
- Entities Graphics - GPU-instanced rendering
- SubScene Baking - authoring-to-entity conversion
- URP - Universal Render Pipeline

## Why DOTS?

DOTS eliminates GC allocation and leverages Burst + Job System for cache-friendly
parallel execution. Batchmode (headless) measurements on this hardware:

| Scenario | Frame Time | GC Alloc | Notes |
|----------|-----------|----------|-------|
| 1,000 moving cubes | 1.22 ms | 0 B | IJobEntity + Burst |
| 200 physics balls | 1.23 ms | 0 B | Unity Physics |
| 500 flocking agents | 1.25 ms | 0 B | Boids + Burst |
| Tower defense (5 waves) | 1.44 ms | 0 B | Full ECS gameplay loop |

Full benchmark tables at `Documentation~/Benchmark.md`.

## Performance Benchmark

Measured in batchmode (headless) on Windows 10 Pro / Unity 2022.3.62f3c1.
Real-Editor FPS will be lower due to GPU rendering. See `Documentation~/Benchmark.md` for methodology.

### Demo01 Moving Cubes

| Entity Count | Avg FPS | Frame Time | GC Alloc |
|--------------|---------|------------|----------|
| 1,000 | 819 | 1.22 ms | 0 B |

### Demo02 Bouncing Balls

| Ball Count | Avg FPS | Frame Time | GC Alloc |
|------------|---------|------------|----------|
| 200 | 815 | 1.23 ms | 0 B |

### Demo03 Flocking Agents

| Agent Count | Mode | Avg FPS | Frame Time | GC Alloc |
|-------------|------|---------|------------|----------|
| 500 | Basic | 802 | 1.25 ms | 0 B |
| 500 | SpatialHash | 735 | 1.36 ms | 0 B |

### Demo04 Tower Defense

| Scenario | Avg FPS | Frame Time | Enemy Peak | GC Alloc |
|----------|---------|------------|------------|----------|
| Default run | 696 | 1.44 ms | ~25 | 0 B |

## Testing

### Unity Editor

1. Open `Window > General > Test Runner`.
2. Select `EditMode`.
3. Run all EditMode tests.
4. Select `PlayMode`.
5. Run all PlayMode smoke tests.

PlayMode smoke tests load each demo scene and run for 300 frames. SubScene baking must be complete before running them.

### GitHub Actions

The workflow at `.github/workflows/test.yml` uses GameCI. CI is configured as
**manual dispatch** by default (no failing CI badge on push).

To enable automated CI on push/PR:
1. Set `UNITY_LICENSE` (or `UNITY_EMAIL` + `UNITY_PASSWORD` + `UNITY_SERIAL`) in Settings > Secrets and variables > Actions.
2. Uncomment the `push` and `pull_request` triggers in `.github/workflows/test.yml`.
3. See [GameCI docs](https://game.ci/docs/github/activation) for license setup.

## Roadmap

- [x] Demo01: Entity movement with Burst
- [x] Demo02: Physics simulation
- [x] Demo03: Boids flocking (basic)
- [x] Demo03: Spatial hash optimization mode
- [x] Demo04: Tower defense gameplay loop (win/lose, UI, enemy wave variants)
- [x] Performance benchmarks with Profiler data (batchmode, headless)
- [x] CI/CD with GitHub Actions
- [x] Demo Hub main menu scene

## Documentation

- `Documentation~/Benchmark.md` - benchmark recording template.
- `Documentation~/Interview_Guide.md` - interview explanation guide.
- `Documentation~/DemoHub_Plan.md` - future main menu scene plan.

## Contributing

Follow DOTS-first patterns consistent with the existing architecture:
- Prefer `ISystem` over `SystemBase`, use `IJobEntity` for parallel iteration.
- Add `[BurstCompile]` where the code path is Burst-compatible.
- Use `EntityCommandBuffer` for structural changes and `LocalTransform` in ECS logic.
- Keep components small and focused, convert authoring data with `Baker<T>`.

Submit a PR:
1. Create a feature branch from `main`.
2. Make small, reviewable changes.
3. Verify the target demo still runs in Unity.
4. Update documentation when behavior or usage changes.
5. Open a pull request with a clear summary and validation steps.

## License

MIT
