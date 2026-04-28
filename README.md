# Unity DOTS Demo Hub

中文 | [English](#english)

面向学习与扩展的 Unity DOTS Hub（Entities / Jobs / Burst / Unity Physics / Entities Graphics）。

## 项目目标

- 提供 4 个可运行 Demo，覆盖 DOTS 常见高频场景。
- 提供按 Demo 的学习任务清单，便于循序推进。
- 提供统一扩展模板目录，降低新增 Demo 成本。

## 环境要求

- Unity：`2022.3.62f3c1`
- 核心包版本（见 `Packages/manifest.json`）：
  - `com.unity.entities` `1.3.14`
  - `com.unity.entities.graphics` `1.3.2`
  - `com.unity.physics` `1.3.14`
  - `com.unity.render-pipelines.universal` `14.0.12`

## 快速开始

1. 使用 Unity Hub 打开本项目根目录。
2. 等待 Package Manager 完成依赖解析与导入。
3. 依次打开场景运行：
   - `Assets/Scenes/Demo01_MovingCubes.unity`
   - `Assets/Scenes/Demo02_BouncingBalls.unity`
   - `Assets/Scenes/Demo03_FlockingAgents.unity`
   - `Assets/Scenes/Demo04_TowerDefense.unity`

说明：每个 Demo 都依赖 SubScene Baking。若复制/新建场景，请确保对应 Authoring GameObject 在 SubScene 中，并在 Play 前完成烘焙。

## Demo 导航（推荐顺序）

1. Demo01 Moving Cubes：批量生成与移动（ECS + Burst/Jobs）
2. Demo02 Bouncing Balls：Unity Physics 批量物理模拟
3. Demo03 Flocking Agents：并行友好的群集行为
4. Demo04 Tower Defense：多系统协作（刷怪/寻敌/伤害/投射物/清理）

代码路径统一位于：`Assets/Scripts/DOTS/`

## 学习与扩展入口

- 学习任务清单：`LEARNING_CHECKLIST.md`
- 扩展模板目录：`Assets/Scripts/DOTS/Templates/DemoTemplate/`
- 性能优化文档：`DOTS_Performance_Optimization_Engineering_Document.md`
- 脚本总览：`Assets/Scripts/DOTS/README.md`

## v1.1 变更

- 新增双语首页（中英）。
- 新增分 Demo 学习任务清单。
- 新增统一扩展模板目录（组件、Authoring、System、README）。

---

## English

Learning-and-extension Unity DOTS hub covering Entities, Jobs, Burst, Unity Physics, and Entities Graphics.

### Goals

- Provide 4 runnable demos for common DOTS scenarios.
- Provide demo-based learning checklists.
- Provide a unified extension template for new demos.

### Requirements

- Unity: `2022.3.62f3c1`
- Core packages in `Packages/manifest.json`:
  - `com.unity.entities` `1.3.14`
  - `com.unity.entities.graphics` `1.3.2`
  - `com.unity.physics` `1.3.14`
  - `com.unity.render-pipelines.universal` `14.0.12`

### Quick Start

1. Open this folder in Unity Hub.
2. Let Package Manager resolve and import dependencies.
3. Run scenes in order:
   - `Assets/Scenes/Demo01_MovingCubes.unity`
   - `Assets/Scenes/Demo02_BouncingBalls.unity`
   - `Assets/Scenes/Demo03_FlockingAgents.unity`
   - `Assets/Scenes/Demo04_TowerDefense.unity`

Each demo relies on SubScene baking. If you duplicate/create scenes, keep the authoring GameObject inside SubScene and wait for baking before Play.

### Hub Entrypoints

- Learning checklist: `LEARNING_CHECKLIST.md`
- Extension template: `Assets/Scripts/DOTS/Templates/DemoTemplate/`
- Performance doc: `DOTS_Performance_Optimization_Engineering_Document.md`
- Script overview: `Assets/Scripts/DOTS/README.md`
