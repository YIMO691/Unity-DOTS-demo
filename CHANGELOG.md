# Changelog

## [0.1.0] - 2025-01-01

### Added
- Demo01: Moving Cubes - 1000+ entity movement with Burst + IJobEntity
- Demo02: Bouncing Balls - Unity Physics simulation
- Demo03: Flocking Agents - Boids behavior with neighbor sampling
- Demo04: Tower Defense - ECS-based tower defense prototype
- Shared: DemoHUD runtime debug overlay

## [0.2.0] - 2026-04-29

### Added
- Demo01: explicit inspector-configurable `SpawnCount` for 1k / 5k / 10k / 50k testing.
- Demo03: SpatialHash boids mode with runtime mode switcher and cell-size UI.
- Demo04: base health, game state, victory/defeat loop, HUD, enemy health bars, and tower range visualization.
- Demo04: configurable five-wave difficulty progression with normal, fast, and boss enemy stats.
- Tests: EditMode component/config tests and PlayMode scene smoke tests.
- CI: GitHub Actions workflow for Unity EditMode and PlayMode tests.
- Documentation: benchmark template, interview guide, and Demo Hub planning document.

### Changed
- Demo01: wrap-around preserves overshoot to reduce boundary clumping.
- Demo04: projectiles now synchronize `LocalToWorld` at spawn and movement time to avoid origin-frame flicker.
