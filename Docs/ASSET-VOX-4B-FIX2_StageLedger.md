# ASSET-VOX-4B-FIX2 Stage Result Ledger

日期：2026-08-30  
状态：Implemented / automated verified / physical WPF smoke pending

## 阶段结果

| 阶段 | 结果 | 证据 |
|---|---|---|
| FIX2-0 | 完成 | 最终契约获用户批准；范围、输入所有权、冻结边界和验证门已冻结。 |
| FIX2-1 | 完成 | SceneBuilder 发布 scene-lifetime face hit map；单体素六面双三角形、同批相邻体素、交叉面拒绝和失败空表测试通过。 |
| FIX2-2 | 完成 | 覆盖整个主视图的 `InputSurface` 统一接收输入；左键即时语义命中，旧左键相机/MouseUp 阈值路径删除。 |
| FIX2-3 | 完成 | 右键任意位置 Orbit，Shift+右键/中键 Pan，滚轮 Zoom；丢失捕获、清场和 Dispose 终止手势；失败反馈接入既有状态区。 |
| FIX2-4 | 完成 | 受影响测试、Debug build、文档、差异审计和 IdeOnly 干净打包完成。 |

## Verification Matrix

| 验证项 | 结果 |
|---|---|
| `Ra2VoxelViewportSceneBuilderTests` 等受影响 IDE 测试 | Passed，35/35 |
| `dotnet build .\\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed，0 warning / 0 error |
| IdeOnly clean package | Passed；1413 files，排除 `.git/.vs/bin/obj/artifacts/TestResults` 等 |
| 真实 DeepSeek/Tencent 调用 | NotRun（契约禁止） |
| 物理 WPF 鼠标烟测 | NotRun；需用户重启新构建后执行 |

已知的测试编译警告：首次定向测试编译报告 `BuiltInFieldRegistryPackLoaderTests.cs:1983` 的既有 CS8602；随后完整
Debug build 为 0 warning。该文件不在本阶段范围，未修改。

## Diff Intent Table

| 文件族 | 意图 | 非目标确认 |
|---|---|---|
| SceneBuilder / tests | 精确外露面到 canonical coordinate 命中映射 | 不修改几何、色板、蒙版或序列化 |
| Viewport XAML/code | 分离左键语义操作与右键相机操作 | 不改布局、AutomationId 或 Shell |
| Workspace view/VM/tests | 可读命中失败反馈，复用既有 `HandleSemanticCellClick` | 不建立第二套画笔/着色/历史链 |
| 文档 | 固化行为、决策、验证和手工验收 | 不改历史交付结论 |

## 边界确认

- Legacy 未恢复。
- Shell 未修改。
- Application 几何/语义/着色算法未修改。
- Apply/Save、VOX/VXL/HVA 写出、Provider、public API、持久化、INI、Field Registry 均未修改。
- AutomationId `VoxelStyle.Preview.Viewport3D` 保持不变。

## 手工验收

1. 完全关闭旧进程并启动新 Debug 构建。
2. 在“语义”页选择“画笔”，左键单击模型：应立即产生着色变化并可撤销。
3. 左键单击空白：不应改变蒙版，应显示“未命中模型表面”。
4. 在模型与空白处分别右键拖动：两处都应旋转，且不得触发画笔。
5. 验证 Shift+右键/中键平移、滚轮缩放；切换视图后相机连续性保持。

## 剩余风险与下一阶段

- 自动化已证明路由和精确映射不变量，但真实 WPF 输入路由仍需上述物理验收。
- 连续拖涂仍明确延期；只有单击可靠性验收后，才应另立 stroke/节流/原子 undo 契约。
- 推荐下一阶段：先完成 FIX2 物理烟测；通过后再评估 `ASSET-VOX-4B-STROKE-1`，不要直接扩展功能。
