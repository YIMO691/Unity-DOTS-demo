# Demo 01: Moving Cubes

Goal: spawn 1000 cube entities once, then move them on the XZ plane with a Burst-compiled `IJobEntity`.

## Required Packages

- Entities
- Entities Graphics
- Burst
- URP

## Scene Setup

This project already includes a generated setup:

- Scene: `Assets/Scenes/Demo01_MovingCubes.unity`
- SubScene: `Assets/Scenes/Demo01_MovingCubes/Demo01_MovingCubes_SubScene.unity`
- Cube prefab: `Assets/DOTS_DemoAssets/Demo01/Demo01_Cube.prefab`
- Cube material: `Assets/DOTS_DemoAssets/Demo01/Demo01_Cube_Material.mat`
- URP asset: `Assets/Settings/Demo_URP_Asset.asset`

Open `Assets/Scenes/Demo01_MovingCubes.unity` and press Play.

To rebuild the generated assets, use `DOTS Demos > Rebuild Demo 01 Moving Cubes`.

Manual setup, if needed:

1. Create a cube prefab with a mesh renderer and material.
2. Create a SubScene.
3. Inside the SubScene, create an empty GameObject named `DOTS Cube Spawner`.
4. Add `CubeSpawnerAuthoring`.
5. Assign the cube prefab.
6. Keep the defaults: `CountX = 25`, `CountZ = 40`.
7. Press Play.

## Expected Result

1000 cubes appear in a grid, move smoothly, and wrap around the configured area.

## Troubleshooting

- If nothing appears, confirm the spawner is inside an open SubScene.
- If cubes are invisible, confirm Entities Graphics and URP are installed and the prefab has a valid material.
- If cubes keep multiplying, confirm `SpawnCubesSystem` destroys the spawner entity after spawning.
