# Unity DOTS Demo Hub — 面试复习指南

## 一、项目总览

用 Unity DOTS 技术栈实现的 5 个递进式 Demo，展示 ECS、Job System、Burst、Physics、Entities Graphics 的核心应用。

| 指标 | 数据 |
|------|------|
| Demo | 5 个 + 主菜单 |
| ECS 系统 | 20+ 个 |
| 测试 | 17 个（EditMode 10 + PlayMode 7），全部通过 |
| Batchmode 帧耗时 | 每帧 1.22–1.44 ms |
| GC 分配 | 0 B（Batchmode，CPU only） |
| Unity | 2022.3.62f3c1 LTS |
| DOTS | Entities 1.3.14 / Physics 1.3.14 / Graphics 1.3.2 |

---

## 二、技术栈详解

### 2.1 ECS (Entity Component System)

面向数据的架构。Entity = 整数 ID，Component = 纯数据 struct，System = 纯逻辑。

传统 OOP:  GameObject → MonoBehaviour → 数据分散在堆上，单线程 Update，有 GC
DOTS ECS:    Entity → IComponentData(struct) → 数据在 Chunk 中连续存储，Job 多线程处理

- **Entity**: 只是一个整数 ID，没有任何数据
- **IComponentData**: 纯数据 struct（值类型），存在 Chunk 中
- **ISystem**: 无托管分配的结构体系统，Burst 兼容（本项目标准）
- **SystemBase**: 旧的类系统，有 GC 开销（本项目不使用）

### 2.2 C# Job System

把逻辑拆成 Job，Worker Thread 并行执行。

- `IJobEntity`: 对每个匹配的实体执行一次 Execute，最常用
- `IJobChunk`: 按 Chunk 批量操作，适合需要 Chunk 级别控制的场景
- `IJobParallelFor`: 按 NativeArray 索引并行

### 2.3 Burst Compiler

LLVM 编译器把 C# Job 编译成高度优化的原生代码。

- 只支持值类型（struct）和 NativeContainer
- 不支持托管对象（class、string、delegate、try-catch）
- 标注 `[BurstCompile]` 的系统必须所有代码路径都 Burst 兼容
- 编译后性能通常提升 5–50 倍

### 2.4 Unity Physics

DOTS 原生物理引擎，完全基于 ECS。

- `PhysicsVelocity`: 线速度和角速度
- `PhysicsCollider`: 碰撞体
- 无需 Rigidbody，无需 MonoBehaviour

### 2.5 Entities Graphics

ECS 实体的 GPU 实例化渲染，替代传统 MeshRenderer。

- `MaterialMeshInfo`: 绑定 Mesh + Material 到实体
- 支持 LOD、Shadow、Motion Vector
- 所有 Demo 的可见实体都通过它渲染

### 2.6 SubScene Baking

编辑时将 GameObject + MonoBehaviour 转换成纯 ECS 实体 + 组件的机制。

- `Baker<T>`: 泛型 Baker，将 MonoBehaviour 数据转换成 IComponentData
- `GetEntity()`: 获取 GameObject 对应的 Entity，声明 TransformUsageFlags
- `AddComponent()` / `AddBuffer()`: 向实体添加组件或动态缓冲区
- Baking 完成后实体序列化在 SubScene 中，运行时直接加载，无需实例化 Prefab

---

## 三、架构设计

### 3.1 统一管线

每个 Demo 遵循完全相同的模式：

```
MonoBehaviour Authoring  →  Baker<T>  →  IComponentData  →  ISystem  →  IJobEntity
     (Inspector 配置)       (编辑时烘焙)    (运行时数据)      (逻辑处理)    (并行执行)
```

### 3.2 一次性生成器模式

Demo01–03 和 Demo05 使用相同的生成器模式：

1. SubScene 中放 Authoring GameObject，Baker 烘焙成 Config 组件
2. SpawnSystem 首次运行时：查询 Config → 批量 Instantiate 实体 → Destroy 自己
3. 其他 System 查询生成的实体进行每帧更新

```csharp
OnCreate: state.RequireForUpdate<Config>();
OnUpdate: 查询 Config → 循环生成 → ecb.DestroyEntity(spawner) → ecb.Playback
```

### 3.3 Demo04 管线系统模式

Demo04 不同：生成器不销毁自己，持续按波次生成敌人。10 个系统严格排序：

```
TowerSpawn → WaveProgression → EnemySpawn → EnemyMovement
  → TowerTargeting → ProjectileMovement → Damage
  → Cleanup → BaseHealth → GameState
```

排序由 `[UpdateBefore]` / `[UpdateAfter]` 属性强制。

### 3.4 实体池模式

Demo04 的敌人使用对象池，消除热路径上的 Instantiate/DestroyEntity：

```
初始化:
  GrowPool() → ecb.Instantiate(N 个) → 加 PooledEnemy 标签 → 存入 buffer

激活（刷怪）:
  取出池顶实体 → 移除 PooledEnemy → 添加 EnemyTag → 设置位置/血量/路径

回收（死亡/到达基地）:
  移除 EnemyTag → 添加 PooledEnemy → 推回池 buffer → 移到 (0, -100, 0)
```

所有 6 个敌人查询系统加 `.WithNone<PooledEnemy>()` 排除休眠实体。

### 3.5 共享基础设施

`Assets/Scripts/Shared/` 下的公共代码：

| 文件 | 职责 |
|------|------|
| `CommonComponents.cs` | `MoveSpeed`、`Velocity` — 消除跨 Demo 重复 struct |
| `SpawnerHelper.cs` | ECB 生命周期管理（disposable wrapper） |
| `GUIStyleHelper.cs` | GUI 样式工厂 — 消除 6 处重复的 EnsureStyles() |
| `DemoHUD.cs` / `DemoHubUI.cs` / `DemoBackButton.cs` | 运行时 UI |

---

## 四、Demo 逐个详解

### Demo01 — 移动方块（ECS 入门）

**做什么**: 1000 个蓝色方块在 44m×36m 矩形区域内随机方向移动，碰壁反弹。

**用了什么**: `IJobEntity` + `BurstCompile` + `EntityCommandBuffer` + `LocalTransform`

**怎么做的**:
- CubeSpawnerAuthoring → Baker 烘焙成 CubeSpawnerConfig
- SpawnCubesSystem（一次性生成器）用 SpawnerHelper 批量生成
- MoveSystem 用嵌套 IJobEntity 并行更新位置

**核心算法 — 边界包裹**:
```
position += direction * speed * deltaTime
if position.x > halfExtents.x:
    position.x = -halfExtents.x + (position.x - halfExtents.x)  // 保留超出量
```
保留超出量避免实体在边界聚集（clumping）。

**关键数据**: 1000 实体 → 1.22 ms, 0 B GC

---

### Demo02 — 弹跳球（DOTS Physics）

**做什么**: 200 个物理球在四面墙容器内弹跳，掉到 -10 以下自动重置到上方。

**用了什么**: `Unity Physics` (PhysicsVelocity + PhysicsCollider) + 自定义重置系统

**怎么做的**:
- BallSpawnerSystem 设置 `PhysicsVelocity`（随机线速度 + 角速度）
- BallResetSystem 检测 `position.y < resetHeight` → 重置位置和速度

**随机种子技巧**:
```csharp
uint seed = math.hash(new uint3(spawnSeed + 1, entityIndex + 1, tick + 1));
```
`[EntityIndexInQuery]` + 每帧 tick → 每个实体每帧都有确定性但不同的随机结果。

**关键数据**: 200球 → 1.23 ms, 0 B GC

---

### Demo03 — 集群智能体（算法核心，面试重点）

**做什么**: 500 个 Agent 实现 Boids 三种力：分离、对齐、聚合。两种模式可运行时切换。

**Boids 三步计算**:
```
Separation:  远离距离 < separationRadius 的邻居
Alignment:   速度方向向邻居平均值靠拢
Cohesion:    位置向邻居中心点靠拢

steering = normalize(desired) * maxSpeed - currentVelocity
velocity += (separation * w + alignment * w + cohesion * w) * deltaTime
```

**Basic 模式** — 随机采样:
- 每个 Agent 随机选 8 个邻居: `(entityIndex + i * 31) % totalCount`
- 跳跃采样避免偏向相邻实体
- 复杂度 O(n × 8)，但大规模时 NativeArray 随机访问导致 Cache Miss

**SpatialHash 模式** — 空间哈希:
- `NativeParallelMultiHashMap<int2, int>` 做空间哈希表
- Key = int2 格子坐标，Value = 实体索引
- 每个 Agent 查相邻 3×3 = 9 个格子内的所有 Agent
- CellSize = NeighborRadius，保证邻居在 3×3 范围内

**为什么需要 `state.Dependency.Complete()`？**
- SpatialHashInsertJob 写哈希表 → SpatialHashBoidJob 读哈希表
- 两个 Job 先后依赖，必须同步（否则可能读到半写的数据）
- 这是 SpatialHash 在小规模时比 Basic 慢 ~8% 的原因

**关键数据**:
| 模式 | 500 Agent | 说明 |
|------|-----------|------|
| Basic | 1.25 ms | 8 采样/实体 |
| SpatialHash | 1.36 ms | 哈希表构建 + 9 格查询 |

---

### Demo04 — 塔防（架构核心，面试重点）

**做什么**: 完整塔防游戏 — 3 座塔、5 波敌人、3 种类型、胜利/失败判定。

**系统管线**:
```
TowerSpawn → WaveProgression → EnemySpawn → EnemyMovement → TowerTargeting
  → ProjectileMovement → Damage → Cleanup → BaseHealth → GameState
```

**10 个系统的职责**:

| 系统 | 职责 |
|------|------|
| TowerSpawnSystem | 一次性生成 3 座塔 |
| WaveProgressionSystem | 波次状态机：计时器递减、波次切换 |
| EnemySpawnSystem | 从实体池激活敌人（弹出、设属性） |
| EnemyMovementSystem | 沿路点移动，到达终点加 EnemyReachedBase |
| TowerTargetingSystem | 锁定最近敌人，检查伤害量防过量杀伤，发射投射物 |
| ProjectileMovementSystem | 投射物追踪目标 |
| DamageSystem | 累计 DamageEvent buffer，扣血 |
| CleanupSystem | 死亡敌人回池，过期投射物销毁 |
| BaseHealthSystem | 处理到达基地的敌人，扣基地血，判定失败 |
| GameStateSystem | 检查胜利条件（所有波次完成 + 无敌人生还） |

**波次定义** — `WaveDefinition (IBufferElementData)`:
```
Wave 1: 5 普通敌人
Wave 2: 8 普通（血量×1.15）
Wave 3: 5 普通 + 3 快速（速度×1.6，血量×0.7）
Wave 4: 10 普通 + 5 快速（血量×1.4）
Wave 5: 8 普通 + 5 快速 + 2 Boss（血量×4，速度×0.65）
```

**实体池**: 预创建 → 池 buffer → 弹出激活 → 死亡回池。池空自动扩容 20 个。

**投射物命中机制**: 到达目标判定距离内不生成可见投射物，直接通过 ECB.AppendToBuffer 添加 DamageEvent。

**关键数据**: 5 波完整 → 1.44 ms (batchmode), 5.19 ms (Editor Play Mode)

---

### Demo05 — 流场寻路（算法核心）

**做什么**: 40×40 网格 + BFS 流场 + 200 个 Agent 沿梯度并行移动到目标。鼠标点击拖动目标。

**BFS 流场算法**:
```
1. 目标世界坐标 → 格子坐标
2. 全部格子初始化: cost = 255, direction = (0, 0)
3. 目标格子: cost = 0, 加入队列
4. BFS 循环:
    从队列头取出格子
    遍历 4 个邻居
    如果邻居未访问:
      方向 = normalize(当前格子 - 邻居格子)  ← 指向目标
      代价 = 当前代价 + 1
      加入队列
```

**数据流**:
- `FlowFieldGrid`: 网格尺寸、CellSize、原点（Singleton 组件）
- `FlowFieldCell`: 每个格子 `Direction (float2)` + `Cost (byte)`（Buffer Element）
- `PathTarget`: 目标世界位置
- BFS 复杂度 O(gridSize) = 1600 格，恒定
- Agent 移动 O(n)，不依赖格子数

**Agent 怎么移动**:
```
AgentMoveJob (Burst IJobEntity):
  世界坐标 → 格子坐标 → BufferLookup[cellIndex].Direction
  如果 Direction ≈ 0（到达目标）→ 漂向网格中心
  position += direction * speed * deltaTime
```

**关键数据**: 211 Agent → 5.84 ms (Editor Play Mode，含 BFS + 渲染)

---

## 五、关键设计决策

### 为什么全用 ISystem 不是 SystemBase？
ISystem 是 struct，0 托管分配，Burst 兼容。SystemBase 是 class，每次调用有 GC。

### 为什么 IJobEntity 不是 IJobChunk？
IJobEntity 代码简洁、直接操作组件引用。IJobChunk 适合按 Chunk 批处理。本项目 95% 场景 IJobEntity 够用。

### 为什么 Demo04 显式排序？
10 个系统有严格前后依赖（先生成塔才能寻敌，先受伤才能检查死亡），显式排序保证正确性。

### 为什么拆 WaveSpawnerSystem？
280 行一个系统做三件事 → 拆成 3 个小系统（70 + 55 + 115 行）+ 1 个工具类。单一职责、易理解、易测试。

### 为什么 SpatialHash 要 .Complete()？
哈希表写完马上被读 → 必须同步屏障。代价是打断并行链，但保证了正确性。

---

## 六、常见面试追问

| 问题 | 答案要点 |
|------|---------|
| **DOTS 和传统 Unity 最大区别？** | 数据局部性（Chunk 连续存储）→ 多线程（Job）→ 0 GC（struct 组件）→ Burst 编译。传统 OOP 数据分散堆上、单线程 Update、有 GC |
| **什么是 Chunk？** | 16KB 内存块，存相同 Archetype 的实体。组件数据在 Chunk 内连续排列，CPU 缓存友好 |
| **Archetype 是什么？** | 实体组件类型的组合。同 Archetype 的实体存同一种 Chunk。增删组件改变 Archetype，触发实体迁移 |
| **ECB 为什么延迟执行？** | Job 中做结构性变更（Instantiate/Destroy/AddComponent）会破坏正在迭代的数据。ECB 记录操作到队列，Playback 时统一执行 |
| **Burst 编译限制？** | 不能用 class、string、delegate、try-catch、静态可变变量。只能操作 struct 和 NativeContainer |
| **0 GC 怎么做到的？** | 全部组件是 struct（值类型），存在 Chunk 中，Job 直接操作原生内存，不经过托管堆 |
| **5000 Agent 时 Demo03 怎么优化？** | Basic 模式 O(n²) 退化严重，切 SpatialHash。可用 IJobParallelFor 做哈希插入，减少 Complete() |
| **Demo04 为什么 10 个系统比 1 个好？** | 每个系统只做一件事、只查最少组件。修改/复用/测试更安全。Pipeline 中间插入新步骤容易 |

---

## 七、复习顺序

1. **Demo01**（15分钟）: 理解 Authoring→Baker→Component→System→Job 管线
2. **Demo03**（30分钟）: 两种计算模式 + SpatialHash 权衡
3. **Demo04**（30分钟）: 10 系统管线 + 实体池 + 波次状态机
4. **Demo05**（15分钟）: BFS 流场算法
5. **Demo02**（10分钟）: 快速过 Physics 集成
6. **Shared 代码**（10分钟）: 公共组件和工具类

总复习时间约 2 小时。

---

*配合 [CLAUDE.md](../CLAUDE.md)（架构参考）和 [Benchmark.md](Benchmark.md)（性能数据）使用。*
