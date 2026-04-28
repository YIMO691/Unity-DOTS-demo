# Unity DOTS Demo Hub

面向初学者的 Unity DOTS 学习与扩展项目集（Entities / Jobs / Burst / Unity Physics / Entities Graphics）。

## 环境要求

- Unity：2022.3.62f3c1
- 关键包版本（见 Packages/manifest.json）：
  - com.unity.entities 1.3.14
  - com.unity.entities.graphics 1.3.2
  - com.unity.physics 1.3.14
  - com.unity.render-pipelines.universal 14.0.12

## 快速开始

1. 用 Unity Hub 打开本项目根目录。
2. 等待 Package Manager 解析依赖并完成导入（如提示重启/重导入则接受）。
3. 依次打开以下主场景运行：
   - Assets/Scenes/Demo01_MovingCubes.unity
   - Assets/Scenes/Demo02_BouncingBalls.unity
   - Assets/Scenes/Demo03_FlockingAgents.unity
   - Assets/Scenes/Demo04_TowerDefense.unity

每个 Demo 都使用 SubScene 进行 Baking。若你复制/新建场景，请确保 SubScene 中包含对应的 Authoring GameObject，且 SubScene 已完成烘焙后再进入 Play Mode。

## Demo 导航（建议顺序）

1. Demo01 Moving Cubes：批量生成与移动（ECS + Burst/Jobs）
   - 代码：Assets/Scripts/DOTS/Demo01_MovingCubes/
   - 说明：Assets/Scripts/DOTS/Demo01_MovingCubes/README.md
2. Demo02 Bouncing Balls：Unity Physics 批量物理模拟
   - 代码：Assets/Scripts/DOTS/Demo02_BouncingBalls/
   - 说明：Assets/Scripts/DOTS/Demo02_BouncingBalls/README.md
3. Demo03 Flocking Agents：并行友好的群集行为
   - 代码：Assets/Scripts/DOTS/Demo03_FlockingAgents/
   - 说明：Assets/Scripts/DOTS/Demo03_FlockingAgents/README.md
4. Demo04 Tower Defense：多系统协作（刷怪/寻敌/伤害/投射物/清理）
   - 代码：Assets/Scripts/DOTS/Demo04_TowerDefense/
   - 说明：Assets/Scripts/DOTS/Demo04_TowerDefense/README.md

## 推荐阅读

- DOTS 性能优化工程文档：DOTS_Performance_Optimization_Engineering_Document.md
- 脚本总览：Assets/Scripts/DOTS/README.md

## 扩展方式（新增一个 Demo）

推荐保持以下结构与约定：

1. 在 Assets/Scripts/DOTS/ 下新建 DemoXX_YourDemo/ 文件夹。
2. 定义小而清晰的 IComponentData 组件（只放数据）。
3. 用 Authoring + Baker<T> 提供编辑器配置入口。
4. 用一个或多个 System/Job 处理逻辑（把热路径写成 Burst 友好的代码）。
5. 用一个 SubScene 放置 Authoring GameObject，确保可重复烘焙与复现。

