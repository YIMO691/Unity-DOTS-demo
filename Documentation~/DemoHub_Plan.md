# Demo Hub Plan

## Goal

Create a single entry scene that lets users launch each DOTS demo without browsing the project hierarchy.

## Scene

`Assets/Scenes/DemoHub.unity`

## UI

- Title: `Unity DOTS Demo Hub`
- Four buttons:
  - Moving Cubes
  - Bouncing Balls
  - Flocking Agents
  - Tower Defense
- A short description panel for the selected demo.
- A unified Back button in each demo scene.

## Navigation

Use `SceneManager.LoadScene` with build-setting scene names:

- `Demo01_MovingCubes`
- `Demo02_BouncingBalls`
- `Demo03_FlockingAgents`
- `Demo04_TowerDefense`

## Style

- Dark translucent panels.
- Compact technical descriptions.
- Consistent keyboard hint line.
- No benchmark claims on the menu screen.

## Future implementation notes

The hub can stay in classic Unity UI. It is navigation/presentation work, not high-volume simulation, so DOTS would add complexity without meaningful performance value.
