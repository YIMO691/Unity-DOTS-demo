# Demo 04: Mini DOTS Tower Defense

Goal: enemies spawn in waves, follow waypoints, towers find nearby enemies, projectiles hit targets, and health/cleanup systems remove dead entities.

## Required Packages

- Entities
- Entities Graphics
- Burst
- URP

## Scene Setup

This project already includes a generated setup:

- Scene: `Assets/Scenes/Demo04_TowerDefense.unity`
- SubScene: `Assets/Scenes/Demo04_TowerDefense/Demo04_TowerDefense_SubScene.unity`
- Enemy prefab: `Assets/DOTS_DemoAssets/Demo04/Demo04_Enemy.prefab`
- Tower prefab: `Assets/DOTS_DemoAssets/Demo04/Demo04_Tower.prefab`
- Projectile prefab: `Assets/DOTS_DemoAssets/Demo04/Demo04_Projectile.prefab`
- URP asset: `Assets/Settings/Demo_URP_Asset.asset`

Open `Assets/Scenes/Demo04_TowerDefense.unity` and press Play.

To rebuild the generated assets, use `DOTS Demos > Rebuild Demo 04 Tower Defense`.

Manual setup, if needed:

1. Create a SubScene.
2. Inside it, create an empty GameObject named `DOTS Wave Spawner`.
3. Add `WaveSpawnerAuthoring`.
4. Assign the enemy, tower, and projectile prefabs.
5. Optionally create waypoint GameObjects and assign them to `Waypoints`.
6. Optionally create tower position GameObjects and assign them to `Tower Positions`.
7. If no waypoint/tower lists are assigned, the Baker uses simple defaults.
8. Press Play.

## Expected Result

Enemies walk along the path, towers fire projectiles, projectiles apply damage, and enemies disappear at zero health or when reaching the end.

## Troubleshooting

- If no entities spawn, confirm all three prefabs are assigned.
- If enemies do not move, confirm the spawner is inside an open SubScene and has baked waypoints.
- If towers do not shoot, increase `TowerRange` or make sure enemies pass near tower positions.
- If projectiles are invisible, confirm the projectile prefab has a visible mesh and material.
