# Demo 03: Flocking Agents

Goal: spawn many agents that use simple separation, alignment, and cohesion steering.

## Required Packages

- Entities
- Entities Graphics
- Burst
- URP

## Scene Setup

This project already includes a generated setup:

- Scene: `Assets/Scenes/Demo03_FlockingAgents.unity`
- SubScene: `Assets/Scenes/Demo03_FlockingAgents/Demo03_FlockingAgents_SubScene.unity`
- Boid prefab: `Assets/DOTS_DemoAssets/Demo03/Demo03_Boid.prefab`
- Boid material: `Assets/DOTS_DemoAssets/Demo03/Demo03_Boid_Material.mat`
- URP asset: `Assets/Settings/Demo_URP_Asset.asset`

Open `Assets/Scenes/Demo03_FlockingAgents.unity` and press Play.

To rebuild the generated assets, use `DOTS Demos > Rebuild Demo 03 Flocking Agents`.

Manual setup, if needed:

1. Create a capsule, cone, or small triangle-like prefab with a mesh renderer and material.
2. Create a SubScene.
3. Inside the SubScene, create an empty GameObject named `DOTS Boid Spawner`.
4. Add `BoidSpawnerAuthoring`.
5. Assign the boid prefab.
6. Start with `Count = 500`.
7. Press Play.

## Expected Result

Agents move in a loose flock, turn toward nearby sampled agents, avoid crowding, and stay inside the configured bounds.

## Performance Tests

- Try `Count = 500`.
- Try `Count = 2000`.
- Try `Count = 5000`.
- Use the Profiler, Entities Hierarchy, and Systems window to inspect behavior.

## Troubleshooting

- If motion looks too random, increase `AlignmentWeight` and `CohesionWeight`.
- If agents clump together, increase `SeparationWeight` or `SeparationRadius`.
- The beginner version samples a few neighbors instead of using a spatial hash grid.
