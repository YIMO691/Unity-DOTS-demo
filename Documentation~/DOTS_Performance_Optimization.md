# 使用 Unity DOTS 进行性能优化

## 1. 概述

Unity DOTS（Data-Oriented Technology Stack，面向数据的技术栈）是 Unity 用于构建高性能模拟的面向数据运行时与工具模型。其核心包包括 Entities、Burst、Jobs、Unity Physics 和 Entities Graphics。

传统的 Unity 开发通常是面向对象的：游戏玩法行为分散在许多 `MonoBehaviour` 对象中，每个对象拥有自己的状态，Unity 会调用诸如 `Update` 之类的生命周期方法。DOTS 采取了不同的方法：它将大量相似数据以紧凑的组件数组形式存储，然后在系统（System）与作业（Job）中处理这些数据。当很多实体每帧执行相似逻辑时，这种模型尤其高效。

本项目中的演示展示了典型的 DOTS 用例：

- Demo 01：由 Burst 编译的作业更新大量移动方块。
- Demo 02：由 Unity Physics 模拟大量物理小球。
- Demo 03：使用共享数据与易并行逻辑的群集（flocking）智能体。
- Demo 04：塔防：包含波次、敌人、塔与投射物。

这些示例规模不大，但它们体现的优化原则与大型游戏和模拟中的原则一致。

## 2. 为什么 DOTS 能提升性能

### 2.1 数据局部性

面向对象代码往往将数据分散在许多托管对象中。当成千上万个对象每帧更新时，CPU 可能会把大量时间花在等待内存上，而不是做有效工作。

DOTS 将组件数据存放在 chunk（块）中。具有相同组件布局的实体会被分组在一起，使系统能够顺序遍历内存，从而提升 CPU 缓存效率。

例如，与其让每个方块都挂一个包含位置与速度字段的脚本，Demo 01 将移动数据存储在 ECS 组件中，并在一个系统里更新所有匹配的实体。

### 2.2 更好的并行性

传统的 `MonoBehaviour.Update` 逻辑通常运行在主线程上，除非开发者手动搭建线程模型。DOTS 围绕 Jobs 设计：当数据访问规则清晰时，系统可以把工作调度到多个工作线程上。

这对以下场景很有用：

- 移动成千上万个实体。
- 更新大量群集智能体。
- 处理投射物、敌人或粒子。
- 运行大规模物理模拟或空间查询。

### 2.3 Burst 编译

Burst 会把兼容的 C# Job 编译为高度优化的原生代码。当代码以 Burst 友好的方式编写时，结果可能比普通托管 C# 快得多。

Burst 最适合配合以下内容使用：

- 简单的值类型。
- `Unity.Mathematics`。
- 原生容器（Native containers）。
- 可预测的循环。
- 尽量少的托管分配。

### 2.4 减少托管分配

传统 Unity 代码常常会因为 LINQ、临时列表、字符串、闭包或在 `Update` 里创建对象而意外产生垃圾。垃圾回收会导致帧时间尖刺（frame spikes）。

DOTS 鼓励显式的内存所有权与非托管的组件数据。这能降低 GC 压力，让性能更可预测。

### 2.5 可扩展的模拟设计

DOTS 不只是“用更快的方式写同样的代码”，它推动不同的架构：

- 状态存储在组件中。
- 行为由系统实现。
- 系统一次处理许多实体。
- 数据依赖显式化。

这种风格更适合大量相似对象的模拟扩展。

## 3. DOTS 与传统 OOP 的对比

### 3.1 传统的面向对象 Unity

在传统 Unity 中，一个游戏对象往往同时拥有数据与行为：

```csharp
public class MovingCube : MonoBehaviour
{
    public float Speed;

    private void Update()
    {
        transform.position += Vector3.forward * Speed * Time.deltaTime;
    }
}
```

这种方式容易理解、原型开发快。但当成千上万个对象运行相似逻辑时，开销会更明显。

### 3.2 DOTS 风格的设计

在 DOTS 中，数据与行为分离：

```csharp
public struct MoveSpeed : IComponentData
{
    public float Value;
}
```

然后由系统处理所有拥有所需数据的实体：

```csharp
public partial struct MoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (transform, speed) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveSpeed>>())
        {
            transform.ValueRW.Position.x += speed.ValueRO.Value * deltaTime;
        }
    }
}
```

DOTS 版本看起来可能没那么“面向对象”，但它给引擎更多机会去优化内存布局、调度与编译。

## 4. DOTS 的优势

### 4.1 大量实体下的高性能

DOTS 在许多实体共享相似行为时最强。比如：

- RTS 单位。
- 子弹与投射物。
- 人群模拟。
- 群集智能体。
- 物理对象。
- 程序化世界。
- 大量敌人或粒子。

### 4.2 可预测的帧时间

由于 DOTS 减少托管分配并支持显式 Job 调度，它有助于避免由垃圾回收或主线程逻辑过载引发的随机帧尖刺。

### 4.3 更好的 CPU 利用率

现代 CPU 核心数很多，传统 Unity 代码往往无法充分利用。DOTS 更容易把模拟工作分摊到多核上。

### 4.4 清晰的数据所有权

ECS 系统会定义它读取与写入哪些组件。团队一旦理解这种架构，大型模拟往往更容易推理。

### 4.5 适合以模拟为核心的游戏

当核心挑战是模拟规模（而非精细的手工对象行为）时，DOTS 尤其有用。

## 5. 劣势与权衡

### 5.1 学习曲线更陡

DOTS 需要学习新概念：

- Entities（实体）。
- Components（组件）。
- Systems（系统）。
- Bakers 与 Authoring（烘焙与编写/编辑）。
- Jobs（作业）。
- Burst 限制。
- 原生容器。
- 系统更新顺序。
- SubScenes 与烘焙工作流。

这相对 `MonoBehaviour` 开发是一次显著转变。

### 5.2 更需要架构纪律

DOTS 在数据流规划得当时效果最好。设计不佳的 ECS 代码依然可能很慢、难调试或过度复杂。

常见错误包括：

- 游戏过程中产生过多结构性变更（structural changes）。
- 未理解回放（playback）时机就使用 entity command buffer。
- 低效的查询方式。
- 在高频模拟路径混入托管对象。
- 对不需要的“小功能”过度使用 ECS。

### 5.3 调试可能更困难

传统 Unity 调试更直观：选中 GameObject、查看字段、观察脚本运行。DOTS 调试通常涉及：

- Entity Debugger / Entities Hierarchy。
- 组件检查。
- 系统顺序。
- Job 依赖。
- Burst 编译行为。

这套流程很强大，但不够熟悉。

### 5.4 不是所有功能都需要 DOTS

UI、菜单、简单脚本交互、叙事触发器与小型玩法功能通常从 DOTS 获益不大。此时普通 `MonoBehaviour` 代码可能更简单、更易维护。

### 5.5 混合架构的复杂度

许多真实项目会同时使用 GameObjects 与 Entities，它们之间的边界会增加复杂度：

- 用于制作的 GameObject 会被烘焙为实体。
- 运行时 GameObject 与 Entity 的通信需要设计。
- 渲染、物理、动画、VFX 可能采用不同工作流。

混合项目需要明确的归属规则。

## 6. 实用优化指南

### 6.1 只优化热点路径

在真正重要的地方使用 DOTS：

- 大规模重复模拟。
- 每帧对大量对象的循环。
- CPU 受限的玩法系统。
- 昂贵的物理或空间逻辑。

不要因为 DOTS 存在就把所有东西都转成 ECS。

### 6.2 让组件小而聚焦

好的 ECS 组件通常是小型数据容器：

```csharp
public struct Health : IComponentData
{
    public float Value;
}
```

避免把大量互不相关的数据塞进一个组件。更小的组件能让系统查询更精确、数据布局更好。

### 6.3 避免频繁结构性变更

添加/移除组件、创建/销毁实体属于结构性变更，它们比修改组件值更昂贵。

请谨慎使用。对于子弹等高频对象，如果创建/销毁开销变大，可考虑对象池或基于状态的复用。

### 6.4 优先使用 Burst 友好代码

使用：

- `Unity.Mathematics` 的 `float3`、`quaternion` 与数学函数。
- 非托管结构体。
- 必要时使用原生容器。
- 简单循环。

避免：

- Job 中使用托管对象。
- 热点路径做字符串操作。
- 在模拟代码里用 LINQ。
- 在帧循环内产生分配。

### 6.5 优化前后都要剖析（Profile）

优化应以测量为准。使用：

- Unity Profiler。
- Entities Profiler 工具。
- Burst Inspector。
- Timeline 视图。
- 渲染相关时使用 Frame Debugger。

只有当它改善了测得的瓶颈时，改动才算优化。

## 7. 建议学习路径

### Stage 1：ECS 基础

学习：

- 什么是 entity。
- 什么是 `IComponentData`。
- 系统如何查询组件。
- 烘焙如何把 GameObjects 转为 Entities。
- SubScenes 如何使用。

练习：

- 生成实体。
- 移动实体。
- 添加与移除组件。
- 运行时检查实体。

Demo 01 是最佳起点。

### Stage 2：Jobs 与 Burst

学习：

- `IJobEntity`。
- Job 调度。
- 组件读/写访问。
- Burst 兼容代码。
- 常见 Burst 限制。

练习：

- 把一个简单系统改成 Burst Job。
- 对比主线程与 Job 化后的性能。
- 用 Profiler 测量差异。

### Stage 3：物理与渲染

学习：

- Unity Physics 组件。
- 动态与静态刚体。
- Entities Graphics。
- 预制体烘焙。
- 碰撞体设置。

练习：

- 构建物理预制体。
- 生成大量动态刚体。
- 重置或回收实体。

Demo 02 在这里很有帮助。

### Stage 4：模拟系统

学习：

- 系统更新顺序。
- Entity command buffers。
- Dynamic buffers（动态缓冲）。
- 空间划分。
- 避免重复的昂贵搜索。

练习：

- 群集（flocking）。
- 塔的目标选择。
- 投射物模拟。
- 波次生成。

Demo 03 与 Demo 04 是很好的练习。

### Stage 5：高级优化

学习：

- Chunk 遍历。
- Aspects。
- Enableable components（可启用组件）。
- Blob assets。
- 自定义烘焙。
- 实体池。
- 空间加速结构。
- 若多人相关则学习 NetCode。

练习：

- 用空间哈希替换朴素敌人搜索。
- 池化投射物而不是销毁它们。
- 用 blob assets 存静态路径数据。
- 按读/写职责拆分系统。

## 8. 如何深入

### 8.1 学习内存布局

理解 DOTS 就要理解内存布局为何重要。学习：

- CPU 缓存行为。
- 顺序访问 vs 随机访问。
- 结构体数组（SoA）思维。
- 伪共享（false sharing）。
- 数据依赖。

这些知识解释了 DOTS 为什么快。

### 8.2 构建更大规模的模拟

小 demo 有用，但更深的理解来自规模。尝试：

- 10,000 个移动智能体。
- 5,000 个投射物。
- 数百座塔与敌人。
- 大型物理压力测试。
- 多个独立系统同时运行。

然后进行剖析与优化。

### 8.3 对比等价的 OOP 与 DOTS 实现

把同一功能做两遍：

- 一次用 `MonoBehaviour`。
- 一次用 DOTS。

测量：

- CPU 时间。
- GC 分配。
- FPS。
- 帧尖刺。
- 代码复杂度。

这能帮助判断 DOTS 何时值得投入成本。

### 8.4 学习混合架构

大多数真实 Unity 项目不会是纯 DOTS。学习如何组合：

- GameObject 制作（authoring）。
- ECS 运行时模拟。
- GameObject UI。
- DOTS 渲染。
- 传统动画或 VFX。

关键问题不是“DOTS 还是 OOP？”，而是“游戏的哪一部分适合哪种模型？”

### 8.5 阅读并研究包示例

Unity 包自带的示例很有价值，因为它们展示了官方模式。研究：

- Entities 示例。
- Unity Physics 示例。
- Entities Graphics 示例。
- Baking 示例。

不要只复制代码，更要关注系统为何这样组织。

## 9. 工程建议

面向未来的 DOTS 学习与项目实践建议：

1. 先做出正确的简单玩法循环。
2. 先剖析，再优化。
3. 只把高量级模拟迁入 DOTS。
4. 通过 GameObject bakers 保持制作流程友好。
5. 避免过度结构性变更。
6. 对热点系统使用 Burst。
7. 保持渲染与模拟数据同步。
8. 尽早构建调试工具。
9. 记录系统更新顺序。
10. 在确定架构前对比 DOTS 与 OOP 实现。

## 10. 结论

DOTS 是 Unity 中强大的、面向性能的编程模型。它的主要优势不只是“更快”，而是它鼓励能在现代硬件上良好扩展的数据布局与执行模式。

与传统的面向对象 Unity 编程相比，DOTS 提供更好的内存局部性、并行性、Burst 优化与可预测的帧时间。代价是更高的学习曲线、更复杂的调试，以及对架构设计的更高要求。

最佳工程策略是选择性采用：把需要规模与性能的部分交给 DOTS，而在更简单、更高产的地方继续使用传统 Unity 工作流。