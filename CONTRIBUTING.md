# Contributing

Thank you for contributing to Unity DOTS Demo Hub. This repository is a learning-focused Unity DOTS project with four runnable demos covering Entities, Jobs, Burst, Unity Physics, and Entities Graphics.

## Environment Requirements

- Unity `2022.3.62f3c1`
- `com.unity.entities` `1.3.14`
- `com.unity.entities.graphics` `1.3.2`
- `com.unity.physics` `1.3.14`
- `com.unity.render-pipelines.universal` `14.0.12`

## Clone The Repository

```bash
git clone https://github.com/YIMO691/Unity-DOTS-demo.git
cd Unity-DOTS-demo
```

## Open The Project

1. Open Unity Hub.
2. Add the repository root folder.
3. Open the project with Unity `2022.3.62f3c1`.
4. Wait for Package Manager to finish importing dependencies.

## Run The Demos

Open any of the following scenes in `Assets/Scenes/` and press Play:

- `Demo01_MovingCubes.unity`
- `Demo02_BouncingBalls.unity`
- `Demo03_FlockingAgents.unity`
- `Demo04_TowerDefense.unity`

Note: These demos rely on SubScene baking. Keep authoring GameObjects inside the related SubScene and allow baking to finish before entering Play mode.

## Code Style

Use DOTS-first patterns and keep new gameplay code aligned with the existing architecture:

- Prefer `ISystem` over `SystemBase` for new ECS systems.
- Use `IJobEntity` for parallelizable entity iteration.
- Add `BurstCompile` where the code path is Burst-compatible.
- Use `EntityCommandBuffer` for structural changes.
- Use `LocalTransform` instead of legacy `Transform` data in ECS logic.
- Store pure ECS data in `IComponentData`.
- Convert authoring data with `Baker<T>`.

Please also keep changes focused, avoid unnecessary package changes, and do not commit generated Unity folders such as `Library/` or `Temp/`.

## Submit A Pull Request

1. Create a feature branch from `main`.
2. Make small, reviewable changes.
3. Verify the target demo still runs in Unity.
4. Update documentation when behavior or usage changes.
5. Open a pull request with a clear summary, validation steps, and screenshots or logs when relevant.
