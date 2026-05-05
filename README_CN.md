# Unity DOTS Demo Hub

[English](README.md) · [中文](README_CN.md)

高性能 Unity DOTS/ECS 演示集合 — 从基础实体移动到完整玩法循环。
适用于学习、面试展示和性能基准测试。

[![Tests](https://github.com/YIMO691/Unity-DOTS-demo/actions/workflows/test.yml/badge.svg)](https://github.com/YIMO691/Unity-DOTS-demo/actions/workflows/test.yml)

Unity 2022.3 LTS · Entities 1.3.14 · Unity Physics 1.3.14 · Entities Graphics 1.3.2 · URP 14.0.12

## Demo 列表

| # | Demo | 核心技术 | 规模 | 关键模式 |
|---|------|---------|------|---------|
| 01 | **移动方块** | IJobEntity, Burst, ECB, 边界环绕 | 1k–50k | 并行实体更新 |
| 02 | **弹跳球** | Unity Physics, PhysicsVelocity, 碰撞体烘焙 | 100–1k | 物理 + 重置循环 |
| 03 | **集群智能体** | Boids 算法, Basic/SpatialHash 双模式, 邻域采样 | 500–10k | 并行群集 |
| 04 | **塔防** | 波次刷怪, 寻敌, 投射物, 胜负判定 | 100–500 | 玩法循环 + 实体池 |
| 05 | **流场寻路** | BFS 梯度场, BufferLookup, Burst IJobEntity | 100–1k | 网格寻路 |

所有场景在 `Assets/Scenes/` 下。每个 Demo 都有自动搭建脚本（`Assets/Editor/`）。

## 快速开始

1. 安装 **Unity 2022.3.62f3c1**（或兼容的 2022.3 LTS 版本）。
2. `git clone https://github.com/YIMO691/Unity-DOTS-demo.git`
3. 用 Unity Hub 打开项目根目录，等待 Package Manager 导入完成。
4. 首次打开时自动搭建脚本会创建场景、材质和预制体。
5. 打开 `Assets/Scenes/DemoHub.unity` 进入主菜单，或直接打开任意 Demo 场景。
6. 按 **Play**。

SubScene 烘焙是必需的 — 作者 GameObject 必须放在 SubScene 内，等烘焙完成后再进入 Play 模式。

## 架构

每个 Demo 遵循相同的管线：

```
MonoBehaviour Authoring  →  Baker<T>  →  IComponentData  →  ISystem / IJobEntity
```

`Assets/Scripts/Shared/` 中的共享基础设施：

| 文件 | 用途 |
|------|------|
| `CommonComponents.cs` | 共享 `MoveSpeed` 和 `Velocity` 结构体 |
| `SpawnerHelper.cs` | 可释放的 ECB 包装器，供一次性生成器使用 |
| `GUIStyleHelper.cs` | 通用 GUI 样式工厂方法 |
| `DemoHUD.cs` / `DemoHubUI.cs` / `DemoBackButton.cs` | 运行时 UI 叠加层 + 菜单 |

### Demo04 系统管线

```
TowerSpawn → WaveProgression → EnemySpawn → EnemyMovement → TowerTargeting
    → ProjectileMovement → Damage → Cleanup → BaseHealth → GameState
```

实体池：敌人预创建后通过 `PooledEnemy` 标签复用。全部 10 个系统用 `.WithNone<PooledEnemy>()` 排除休眠实体。

### Demo05 流场寻路

BFS 从目标格子向外扩散，遍历 40×40 网格。每个格子存储指向目标的方向向量。
200+ 个 Agent 通过 Burst 编译的 `IJobEntity` 中的 `BufferLookup<FlowFieldCell>` 读取梯度 —
零逐体寻路开销。在场景中点击并拖动鼠标可实时移动目标点。

## 性能

### Batchmode（无头模式，纯 CPU）

| Demo | 实体数 | 帧耗时 | GC 分配 |
|------|--------|--------|---------|
| 移动方块 | 1,000 | 1.22 ms | 0 B |
| 弹跳球 | 200 | 1.23 ms | 0 B |
| 集群 (Basic) | 500 | 1.25 ms | 0 B |
| 集群 (SpatialHash) | 500 | 1.36 ms | 0 B |
| 塔防 | 5 波 | 1.44 ms | 0 B |

### Editor Play Mode（含 GPU 渲染）

| Demo | 实体数 | 平均 FPS | 帧耗时 | GC/帧 |
|------|--------|----------|--------|-------|
| 移动方块 | 1,009 | 217 | 4.60 ms | 1.4 KB |
| 弹跳球 | 219 | 218 | 4.59 ms | 1.2 KB |
| 集群 (Basic) | 515 | 212 | 4.71 ms | 1.7 KB |
| 集群 (SpatialHash) | 516 | 208 | 4.81 ms | 1.4 KB |
| 塔防 | 64 峰值 | 193 | 5.19 ms | 2.4 KB |
| 流场寻路 | 211 | 171 | 5.84 ms | 0.9 KB |

Play Mode 中的 GC 来自 Editor 和渲染开销 — DOTS ECS 代码本身零分配。
完整方法和高压配置见 [`Documentation~/Benchmark.md`](Documentation~/Benchmark.md)。

## 技术栈

| 技术 | 作用 |
|------|------|
| **ECS** (Entities 1.3.14) | 面向数据架构，Chunk 组件存储 |
| **C# Job System** | 多线程并行处理 |
| **Burst Compiler** | Job 原生代码编译 |
| **Unity Physics** | DOTS 原生碰撞和动力学 |
| **Entities Graphics** | ECS 实体 GPU 实例化渲染 |
| **SubScene Baking** | 编辑时作者数据转实体 |
| **URP** | 通用渲染管线，Forward+ 渲染 |

## 项目结构

```
Assets/
├── DOTS_DemoAssets/Demo01–05/    预制体、材质、搭建标记
├── Editor/
│   ├── DemoHubSetup.cs            主菜单场景 + 返回按钮注入
│   ├── Demo01MovingCubesSetup.cs  自动创建场景和资产
│   ├── Demo02BouncingBallsSetup.cs
│   ├── Demo03FlockingAgentsSetup.cs
│   ├── Demo04TowerDefenseSetup.cs
│   └── Demo05PathfindingSetup.cs
├── Scenes/Demo01–05 + DemoHub     主场景和 SubScene
├── Scripts/
│   ├── Shared/                    共享组件、SpawnerHelper、GUIStyleHelper、UI
│   └── DOTS/Demo01–05 + Templates ECS 系统、组件、Authoring
├── Tests/
│   ├── EditMode/                  10 个单元测试（组件、配置、算法）
│   └── PlayMode/                  7 个冒烟测试 + 6 个基准测试
└── Settings/                      URP 管线资产
```

## 测试

**17 个测试** — 全部通过。

```powershell
# EditMode（无头）
& "<Unity>/Editor/Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -quit

# PlayMode（无头）
& "<Unity>/Editor/Unity.exe" -batchmode -projectPath . -runTests -testPlatform PlayMode -quit
```

或在 Unity Editor 中使用 **Window > General > Test Runner**。

### CI

Push 和 PR 触发器已启用。`check-license` 门控在没有 Unity License 时自动跳过（绿色通过而非红色失败）。激活步骤：

1. 复制你的 Unity 许可证：`cat $env:PROGRAMDATA\Unity\Unity_lic.ulf`
2. 在 **Settings > Secrets and variables > Actions** 中添加 `UNITY_LICENSE` secret
3. 每次 push 自动运行 CI

[GameCI 许可证配置文档 →](https://game.ci/docs/github/activation)

## 参与贡献

- 优先使用 `ISystem` 和 `IJobEntity`。在允许的代码路径上加 `[BurstCompile]`。
- 结构性变更用 `EntityCommandBuffer`。使用 `LocalTransform`（而非旧版 `Transform`）。
- 保持组件小而专注。用 `Baker<T>` 转换作者数据。
- 提交 PR 前验证目标 Demo 正常运行。

## 文档

| 文档 | 内容 |
|------|------|
| [`CLAUDE.md`](CLAUDE.md) | AI 助手指导和架构参考 |
| [`CHANGELOG.md`](CHANGELOG.md) | 版本历史 |
| [`Documentation~/Benchmark.md`](Documentation~/Benchmark.md) | 方法、batchmode + Play Mode 结果、高压测试指南 |
| [`Documentation~/Interview_Guide.md`](Documentation~/Interview_Guide.md) | 面试讲解要点 |
| [`Documentation~/LEARNING_CHECKLIST.md`](Documentation~/LEARNING_CHECKLIST.md) | 动手学习清单 |
| [`Documentation~/DOTS_Performance_Optimization.md`](Documentation~/DOTS_Performance_Optimization.md) | DOTS 性能优化理论 |

## 许可证

MIT — 详见 [LICENSE](LICENSE)。
