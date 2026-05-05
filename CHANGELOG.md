# Changelog

## [1.0.0] - 2026-05-05

### Added
- Demo05: Flow Field Pathfinding — BFS gradient field + Burst IJobEntity parallel agent movement.
- Demo05: editor auto-setup script (scene, SubScene, Capsule agent prefab, orange material).
- Demo05: PlayMode smoke test (300 frames) and benchmark template (200/500/1000 agents).
- Entity pooling: Demo04 enemies pre-spawned and reused via `PooledEnemy` tag, eliminating per-frame Instantiate/DestroyEntity.
- `EnemyPoolUtility`: shared pool helpers (ReturnToPool, GrowPool, GrowPoolIfNeeded).

### Changed
- Refactored: split Demo04 `WaveSpawnerSystem` (280 lines) into `TowerSpawnSystem` + `WaveProgressionSystem` + `EnemySpawnSystem` with shared `EnemyPoolUtility`.
- System order updated: `TowerSpawn → WaveProgression → EnemySpawn → EnemyMovement → TowerTargeting → ProjectileMovement → Damage → Cleanup → BaseHealth → GameState`.
- All Demo04 systems now exclude pooled entities via `.WithNone<PooledEnemy>()`.
- Benchmark.md: added Demo05 section, high-stress benchmark guide, updated key takeaways.

### Fixed
- CI: reverted push/PR triggers to manual dispatch (requires UNITY_LICENSE secret).
- Demo05: removed `[ReadOnly]` attributes incompatible with IJobEntity.
- Demo04 split: added missing `DOTSDemo.Shared` and `Unity.Mathematics` using directives.

## [0.4.0] - 2026-05-05

### Changed
- Documentation: consolidated 7 root .md files to 3 (README, CLAUDE, CHANGELOG). Moved reference docs to `Documentation~/`.
- Refactored: extracted shared DOTS components (`MoveSpeed`, `Velocity`) into `Assets/Scripts/Shared/CommonComponents.cs`, eliminating 3 duplicate struct definitions across Demo01/Demo04/Template.
- Refactored: added `SpawnerHelper` (disposable ECB wrapper) to reduce boilerplate in all spawner systems.
- Refactored: added `GUIStyleHelper` (shared style factories) to centralize duplicated `EnsureStyles()` pattern across 6 UI files.
- Template: fixed namespace from `DOTS.Templates.DemoTemplate` to `UnityDotsDemo.Template`, now uses shared components.
- CI: enabled `push` and `pull_request` triggers for automated test runs.

### Removed
- Deleted `AGENTS.md` (merged into `CLAUDE.md`).
- Deleted `CONTRIBUTING.md` (merged into `README.md`).

## [0.3.0] - 2026-05-04

### Added
- DemoHub: main menu scene with navigation and Back buttons on all demo scenes.
- Performance benchmarks: 5 PlayMode benchmark tests with real batchmode data.
- Benchmark results in `Documentation~/Benchmark.md` and README.

### Fixed
- Demo02: corrected GUID mismatch in SubScene causing missing-script warnings.
- Demo04: corrected GUID references for GameStateAuthoring and BaseHealthAuthoring.
- Demo04: hidden debug Marker spheres, raised path segments to fix Z-fighting.
- All setup scripts: fixed `NewSceneMode.Additive` → `Single` to prevent reload loops.
- AGENTS.md: removed machine-specific paths.
- CI: switched to manual dispatch to avoid failing without configured Unity license.

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

## [0.1.0] - 2026-04-27

### Added
- Demo01: Moving Cubes - 1000+ entity movement with Burst + IJobEntity
- Demo02: Bouncing Balls - Unity Physics simulation
- Demo03: Flocking Agents - Boids behavior with neighbor sampling
- Demo04: Tower Defense - ECS-based tower defense prototype
- Shared: DemoHUD runtime debug overlay
