# Demo 02: DOTS Physics Bouncing Balls

Goal: spawn many physics sphere entities that fall, bounce, and reset when they leave the arena.

## Required Packages

- Entities
- Entities Graphics
- Unity Physics
- Burst
- URP

## Prefab Setup

1. Create a sphere prefab with a mesh renderer and material.
2. Add `BallPhysicsAuthoring`.
3. Keep the default radius, mass, damping, friction, and restitution values for a dynamic bouncing ball.

## Scene Setup

1. Create a SubScene.
2. Add the sphere prefab to the project.
3. Create an empty GameObject named `DOTS Ball Spawner` inside the SubScene.
4. Add `BallSpawnerAuthoring`.
5. Assign the sphere prefab.
6. Create a floor and four wall GameObjects inside the SubScene.
7. Add `StaticBoxColliderAuthoring` to the floor and walls.
8. Press Play.

This repository also includes `Demo02BouncingBallsSetup`, which automatically creates
`Assets/Scenes/Demo02_BouncingBalls.unity`, the SubScene, sphere prefab, floor, and walls.

## Expected Result

200 sphere entities fall under Unity Physics, bounce inside the arena, and reset to the top when they fall below `ResetY`.

## Troubleshooting

- If `PhysicsVelocity` errors appear, confirm the ball prefab has a dynamic Physics Body before baking.
- If balls fall through the floor, confirm the floor and walls are inside the SubScene and have `StaticBoxColliderAuthoring`.
- If rendering is missing, confirm Entities Graphics and URP are installed.
