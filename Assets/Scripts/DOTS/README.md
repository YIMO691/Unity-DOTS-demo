# Unity DOTS Demo Pack

This folder contains four independent beginner DOTS demos:

1. `Demo01_MovingCubes`: spawns and moves 1000 cubes.
2. `Demo02_BouncingBalls`: uses Unity Physics for bouncing balls.
3. `Demo03_FlockingAgents`: runs a simple flocking simulation.
4. `Demo04_TowerDefense`: builds a small ECS tower defense simulation.

## Project Requirements

The project manifest includes:

- `com.unity.entities`
- `com.unity.entities.graphics`
- `com.unity.physics`
- `com.unity.render-pipelines.universal`

After opening the project, let Unity Package Manager resolve and import these packages before creating scenes.

## Recommended Learning Order

1. Open and finish Demo 01 first.
2. Move to Demo 02 when basic spawning and movement are clear.
3. Use Demo 03 to study parallel simulation.
4. Use Demo 04 to study multiple systems working together.

## Common Setup Pattern

Each demo expects:

1. A visible prefab with a mesh and material.
2. An empty authoring GameObject with the demo's authoring component.
3. The authoring GameObject placed inside a SubScene.
4. Play Mode after the SubScene has baked.

## Debugging Tools

Use these Unity windows while learning:

- Entities Hierarchy
- Entities Journaling
- Systems
- Profiler

## Hard Rules Used by These Demos

- Components are pure `IComponentData` structs.
- Authoring data lives in `MonoBehaviour` classes and is converted by `Baker<T>`.
- Movement uses `LocalTransform`.
- Large update loops use `IJobEntity` and Burst where appropriate.
- Spawn and destroy operations use `EntityCommandBuffer`.
- Deprecated `Entities.ForEach`, `Translation`, `Rotation`, and `Scale` are not used.
