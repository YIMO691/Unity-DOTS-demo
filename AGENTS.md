# AGENTS.md - Unity DOTS Demo Hub

## Project Identity

- Unity: `2022.3.62f3c1` / 2022.3 LTS
- Core packages: Entities `1.3.14`, Unity Physics `1.3.14`, Entities Graphics `1.3.2`, URP `14.0.12`
- Language: C#
- Runtime assembly: `Assets/Scripts/UnityDotsDemo.asmdef`
- Test assemblies:
  - `Assets/Tests/EditMode/EditModeTests.asmdef`
  - `Assets/Tests/PlayMode/PlayModeTests.asmdef`
- Main namespace pattern:
  - `UnityDotsDemo.Demo01`
  - `UnityDotsDemo.Demo02`
  - `UnityDotsDemo.Demo03`
  - `UnityDotsDemo.Demo04`

## Current Completion State

- Demo01 Moving Cubes: implemented
- Demo02 Bouncing Balls: implemented
- Demo03 Flocking Agents: implemented, including Basic and SpatialHash modes
- Demo04 Tower Defense: implemented
- Runtime asmdef: implemented
- EditMode test asmdef: updated to reference `UnityDotsDemo`, `Unity.Entities`, `Unity.Entities.Hybrid`, `Unity.Collections`, and `Unity.Mathematics`
- Runtime asmdef references `Unity.Entities.Hybrid` because `Baker<T>` is defined there in Entities 1.3.x.
- PlayMode test asmdef: updated to reference `UnityEngine.TestRunner` and `UnityDotsDemo`
- Demo Hub scene: implemented (`Assets/Scenes/DemoHub.unity`, `DemoHubUI.cs`, `DemoHubSetup.cs`)
- Demo Back Button: implemented (`DemoBackButton.cs`), added via `DOTS Demos > Add Back Buttons To Demo Scenes`
- Performance benchmark values: not recorded yet

## Architecture Per Demo

Each demo follows this pattern:

```text
MonoBehaviour Authoring
  -> Baker<T>
    -> IComponentData / IBufferElementData
      -> ISystem / IJobEntity
```

SubScene baking is mandatory. Authoring GameObjects must live inside the related SubScene, and baking must finish before entering Play Mode.

## DOTS Code Rules

- Prefer `ISystem` for new ECS systems.
- Use `IJobEntity` for parallel entity iteration when practical.
- Add `[BurstCompile]` to systems and jobs where compatible.
- Use `EntityCommandBuffer` for structural changes such as instantiate, destroy, add component, and remove component.
- Use `LocalTransform`; do not use deprecated `Translation`, `Rotation`, or `Scale`.
- Use `Baker<T>` for authoring-to-entity conversion.
- Avoid naming a nested baker class `Baker : Baker<T>` because the nested class name shadows the generic `Baker<T>` type. Use a specific nested class name such as `CubeSpawnerBaker : Baker<CubeSpawnerAuthoring>`.
- Keep components small and focused.

## Directory Layout

| Path | Purpose |
| --- | --- |
| `Assets/Scripts/UnityDotsDemo.asmdef` | Runtime assembly definition |
| `Assets/Scripts/DOTS/Demo01_MovingCubes/` | Demo01 ECS code |
| `Assets/Scripts/DOTS/Demo02_BouncingBalls/` | Demo02 ECS code |
| `Assets/Scripts/DOTS/Demo03_FlockingAgents/` | Demo03 ECS code |
| `Assets/Scripts/DOTS/Demo04_TowerDefense/` | Demo04 ECS code |
| `Assets/Scripts/DOTS/Templates/DemoTemplate/` | Starter template for future demos |
| `Assets/Scripts/Shared/DemoHUD.cs` | Shared runtime debug HUD |
| `Assets/Editor/` | Editor setup scripts (Demo01-04 + DemoHub) |
| `Assets/DOTS_DemoAssets/Demo01-04/` | Demo prefabs, materials, and setup markers |
| `Assets/Scenes/` | Main scenes, SubScenes, and DemoHub |
| `Assets/Tests/EditMode/` | NUnit EditMode tests |
| `Assets/Tests/PlayMode/` | PlayMode smoke tests (includes DemoHub) |
| `Documentation~/` | Benchmark template, interview guide, DemoHub plan |

## Editor Setup Scripts

The editor setup scripts can create or rebuild scenes, prefabs, materials, and SubScenes:

- `Assets/Editor/Demo01MovingCubesSetup.cs`
- `Assets/Editor/Demo02BouncingBallsSetup.cs`
- `Assets/Editor/Demo03FlockingAgentsSetup.cs`
- `Assets/Editor/Demo04TowerDefenseSetup.cs`

They write marker files under `Assets/DOTS_DemoAssets/DemoNN/.demoNN_setup_complete` to avoid repeated setup.

Use the Unity menu `DOTS Demos > Rebuild Demo NN` if a demo scene must be rebuilt.

## Testing

Run tests from Unity Test Runner or batchmode.

EditMode:

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe" `
  -batchmode `
  -projectPath "<project-path>" `
  -runTests `
  -testPlatform EditMode `
  -testResults "Logs\EditModeResults.xml" `
  -logFile "Logs\EditModeBatch.log" `
  -quit
```

PlayMode:

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe" `
  -batchmode `
  -projectPath "<project-path>" `
  -runTests `
  -testPlatform PlayMode `
  -testResults "Logs\PlayModeResults.xml" `
  -logFile "Logs\PlayModeBatch.log" `
  -quit
```

Batchmode cannot run while the project is already open in another Unity Editor instance.

## Known Work Remaining

- Complete the learning checklist tasks in `LEARNING_CHECKLIST.md`.
- Re-run benchmarks at higher entity counts (5k, 10k, 50k) by changing SpawnCount in Inspector.
- Capture GPU rendering data in Play Mode with Unity Profiler for end-to-end metrics.

## Unity Gotchas

- Do not delete `.meta` files.
- Do not commit generated/cache directories:
  - `Library/`
  - `Temp/`
  - `obj/`
  - `.vs/`
  - `Logs/`
  - `UserSettings/`
- Do not manually edit generated `.csproj` or `.sln` files.
- Let Unity Package Manager finish resolving packages before changing scripts.
- Scene and SubScene references depend on `.meta` GUIDs.

## Demo04 System Order

The intended gameplay flow is:

```text
WaveSpawner
  -> TowerTargeting
  -> ProjectileMovement
  -> Damage
  -> Cleanup
  -> BaseHealth
  -> GameState
```

## Adding A Future Demo

1. Copy `Assets/Scripts/DOTS/Templates/DemoTemplate/`.
2. Rename it to `Demo05_YourName`.
3. Update namespaces to `UnityDotsDemo.Demo05`.
4. Create an editor setup script following the existing Demo01 pattern.
5. Create a main scene and SubScene.
6. Put authoring GameObjects inside the SubScene.
7. Add smoke tests and update README.
