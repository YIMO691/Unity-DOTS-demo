# Unity DOTS Learning Checklist

> 用法建议：每完成一项就勾选，确保"会跑、会改、会测量"。
>
> **项目工程已完成**：所有 5 个场景 (Demo01-04 + DemoHub) 均已创建，GUID 引用完整，
> 编辑器自动搭建脚本就绪，EditMode 和 PlayMode 测试覆盖到位。
> 以下为个人学习与动手练习任务。

## Demo01 Moving Cubes（ECS 入门）

- [ ] 打开 `Demo01_MovingCubes` 场景并确认实体正常生成。
- [ ] 理解 `IComponentData` 在 `CubeComponents.cs` 中的作用。
- [ ] 跟踪 `CubeSpawnerAuthoring -> Baker -> 实体` 的转换流程。
- [ ] 在 `MoveSystem.cs` 修改速度规则并验证结果。
- [ ] 使用 Profiler 对比修改前后的 CPU 时间。

## Demo02 Bouncing Balls（DOTS Physics）

- [ ] 打开 `Demo02_BouncingBalls` 场景并观察批量物理行为。
- [ ] 理解 `BallSpawnerSystem` 的生成逻辑与节奏。
- [ ] 修改球数量/初速度/重力相关参数并记录现象。
- [ ] 理解 `BallResetSystem` 的重置策略。
- [ ] 用 Profiler 观察 Physics 与 Simulation 的时间占比。

## Demo03 Flocking Agents（并行群集）

- [ ] 打开 `Demo03_FlockingAgents` 场景并确认群集行为稳定。
- [ ] 阅读 `BoidMovementSystem`，定位对齐/聚合/分离计算。
- [ ] 调整邻域半径与权重，观察群体形态变化。
- [ ] 检查边界系统（`BoidBoundarySystem`）如何约束行为。
- [ ] 评估实体数量从 1k 到 10k 的性能变化。

## Demo04 Tower Defense（多系统协作）

- [ ] 打开 `Demo04_TowerDefense` 场景并确认刷怪、寻敌、投射物、伤害流程完整。
- [ ] 梳理系统顺序：`WaveSpawner -> Targeting -> Projectile -> Damage -> Cleanup`。
- [ ] 修改塔射程/攻速/伤害并验证平衡变化。
- [ ] 修改敌人血量/速度与波次参数并观察系统耦合。
- [ ] 用 Profiler 检查热点系统与结构性变更成本。

## 跨 Demo 进阶任务

- [ ] 选一个 Demo，把核心热路径改写为更 Burst-friendly 的形式。
- [ ] 新增一个组件并拆分一个"过大系统"，保持职责单一。
- [ ] 尝试减少结构性变更（例如复用实体代替频繁创建/销毁）。
- [ ] 在不改玩法目标的前提下，把 95% 分位帧时间压低。
- [ ] 使用模板目录 `Assets/Scripts/DOTS/Templates/DemoTemplate/` 新建 `Demo05`。
