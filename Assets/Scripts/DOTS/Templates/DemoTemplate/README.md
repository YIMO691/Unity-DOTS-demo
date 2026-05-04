# Demo Template

用于快速创建新 Demo 的最小模板。

## 包含文件

- `TemplateComponents.cs`：示例组件（Tag、速度、方向）。
- `TemplateAuthoring.cs`：Authoring + Baker，负责把编辑器参数转换为 ECS 组件。
- `TemplateMoveSystem.cs`：最小移动系统（Burst + ISystem）。

## 使用步骤

1. 复制本目录为 `Assets/Scripts/DOTS/DemoXX_YourDemo/`。
2. 全局替换 `Template` 前缀为你的 Demo 名称。
3. 在场景或 SubScene 中创建带 `TemplateAuthoring` 的 GameObject。
4. 进入 Play Mode，确认实体按系统逻辑更新。
5. 按需拆分系统并补充专用组件。

## 约定

- 组件保持小而清晰，避免“全能组件”。
- 热路径代码优先 Burst 友好风格。
- 结构性变更（创建/销毁/加减组件）集中到明确系统中。
