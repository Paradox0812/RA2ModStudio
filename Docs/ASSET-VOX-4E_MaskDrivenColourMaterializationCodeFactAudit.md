# ASSET-VOX-4E — Mask-Driven Colour Materialization Code-Fact Audit

更新时间：2026-08-30  
状态：Audit completed / DocsOnly / implementation not started

## 1. 审计结论

`ASSET-VOX-4E` 不需要新建第二套上色引擎。现有自然语言样式编译器、RA2 palette-safe
colourizer、语义组合、对比度候选、候选固化和 canonical `.vox` 导出链路均可复用。

当前阻塞点位于语义事实和样式计划之间：语义组合只在样式计划编译完成后注入，样式编译请求及缓存身份
都不知道当前 mask 需要哪些材质颜色角色；后置集成又按类别任取第一个角色。直接继续编码会产生缺少角色、
错误复用缓存或同类角色绑定不确定的问题。因此，4E 实现前必须先冻结 typed semantic colour
requirements、确定性 role binding、缓存失效和失败语义。

本次只完成代码事实审计和状态更新，没有修改运行时、测试、XAML、项目文件或持久化格式。

## 2. 4D 人工验收边界

用户于 2026-08-30 明确报告“保存和导入测试通过”。本审计将 4D 的显式 Save/Import 主路径记录为
`Passed (user-reported physical acceptance)`。

以下项目没有随该表述自动判定为通过：

- 错误模型/过期快照 sidecar 的拒绝及当前状态不变；
- 存在未保存语义修改时的单次确认提示；
- 连续画笔在 100%/125% DPI 下的真实指针体验。

这些项目继续保持 `NotRun/Unknown`，但不阻止已由用户明确选择的 4E 侦察。

## 3. 审计范围

读取并核对：

- `Ra2VoxelStyleContracts.cs`：typed role/rule/plan 与 palette 编译约束；
- `Ra2VoxelStyleCompiler.cs`、`Ra2VoxelStylePlanCache.cs`：请求、缓存键和缓存载入；
- `Ra2VoxelColourizer.cs`：确定性区域规则、显式 mask 和几何不变式；
- `Ra2VoxelSemanticMasking.cs`、`Ra2VoxelSemanticMaskEditing.cs`：语义组合与显式 mask 物化；
- `Ra2VoxelPaletteContrast.cs`：审阅用对比度候选；
- `Ra2VoxelStylePreviewCoordinator.cs`、`Ra2VoxelStyleWorkspaceViewModel.cs`：编译、预览、固化与导出接线；
- 1E、3B、4A、4B 合同及相应 style/compiler/semantic 测试。

未修改或扩展 Shell、项目 Apply/Save、sidecar v1、Provider、public API、VOX/VXL/HVA writer。

## 4. 现有权威链路

```text
VOX/VXL/GLB -> canonical working snapshot
              + style source pack + active palette
              -> Ra2VoxelStyleCompiler
              -> Ra2CompiledVoxelStylePlan

4A/4B assignments -> Ra2VoxelSemanticMaskComposition
                   -> Ra2VoxelSemanticStyleIntegrator (后置注入)
                   -> explicit masks + effective plan
                   -> Ra2VoxelColourizer
                   -> ordinary / contrast styled candidates
                   -> explicit freeze
                   -> canonical MagicaVoxel .vox export
```

所有权保持为：

```text
模型输出              = 不可信样式提案
validated style plan  = palette 角色和规则候选
semantic composition = 当前快照的语义分划权威
colourizer            = 确定性着色和几何不变式权威
用户显式 freeze/export = 候选选择与磁盘副本写出权威
```

## 5. 已确认的可复用能力

### 5.1 样式与色盘

- `Ra2VoxelStyleCompiler` 已输出 bounded typed roles/rules，支持 `BodyBase`、`Glass`、
  `Rubber`、`BareMetal`、`Accent`、`Remap` 等类别。
- `Ra2VoxelStylePlanCompiler` 已校验颜色来源、色盘索引、透明色、remap 范围、role/rule 引用和
  interior 覆盖。
- `Ra2VoxelColourizer` 只改变已占用 cell 的 palette index，并验证尺寸、坐标、占用和 mask
  snapshot hash 不变。
- `Ra2VoxelPaletteContrastOptimizer` 只调整非精确的 body 明暗角色；精确色盘选择、语义材质和
  remap 角色保持不变。

### 5.2 语义组合

- 4A/4B 已将 Agent、区域人工覆盖和 cell 人工覆盖合成为逐 cell 的
  `Ra2VoxelSemanticMaskComposition`。
- 权威优先级已冻结为 `CellHumanOverride > RegionHumanOverride > AgentSuggestion > Unknown`。
- 语义组合和显式 mask 均绑定 working snapshot canonical hash；旧几何不能静默复用。
- 当前 integrator 已能按语义组生成本地 `Ra2VoxelExplicitMask`，模型无需且不得生成逐体素坐标。

### 5.3 候选与导出

- ordinary styled 和 contrast styled 两类候选已经存在，且不会替换 source/working snapshot。
- ViewModel 已支持显式固化候选；3B export service 已通过 canonical codec 回读验证 `.vox` 副本。
- 4E 无需触碰项目 Apply/Save，也无需引入 VXL/HVA writer。

## 6. 必须解决的代码事实缺口

### 6.1 语义事实进入编译过晚

`CompilePreviewAsync` 先仅使用 part role、geometry hash、model identity 编译 base plan，之后才调用
`Ra2VoxelSemanticStyleIntegrator.Integrate`。因此编译器不知道当前组合实际包含 Glass、Rubber、
BareMetal、Accent 或 approved remap，也不能保证生成所需颜色角色。

结果是 mask 存在但计划缺少相应角色时，只能追加 unresolved 信息并跳过该组上色。

### 6.2 缓存身份不包含语义组合

当前 style cache key 仅包含：

```text
source pack hash
palette hash
target part role
geometry facts hash
compiler revision
model identity
colour metric id
```

同一几何、样式和色盘下修改语义 mask，不会使 base plan 缓存失效。若 4E 仅把语义摘要加入 prompt
而不同时修改 cache key、缓存 JSON 验证和 compiler revision，缓存会成为错误权威。

### 6.3 role binding 目前有歧义

integrator 使用 `GroupBy(Category).First()` 选择颜色角色：

- 同一类别存在多个角色时，实际绑定取决于 plan 顺序；
- `Light` 与 `Accent` 都映射到 `Accent`；
- `PaintedSurface` 映射 `BodyBase`，`DarkOpening` 映射 `BodyDark`；
- PartRole 参与 mask 分组和排序，但不参与颜色角色选择。

这不满足“相同输入得到唯一、可解释绑定”的 4E 准入要求。不得把 `First()` 行为固化成隐式合同。

### 6.4 PartRole 与 MaterialRole 的职责尚未冻结

现有 style vocabulary 能稳定表达材质和 body 明暗，但不能表达
`Body/Chassis/Turret/Barrel/Wheel` 各自独立的 palette family。4E v1 必须明确二选一：

1. 推荐最小边界：MaterialRole 决定颜色，PartRole 仅用于审阅、分组和诊断；
2. 扩展边界：新增 `(PartRole, MaterialRole, RemapIntent) -> roleId` typed binding。

第二种会扩大 tool schema、plan hash、cache schema 和测试面，不应在实现时临时决定。

### 6.5 自动化证据不足

现有测试证明了单个 Rubber mask、几何不变和普通/对比度候选，但没有完整覆盖：

- 所有 `MaterialRole` 的 required-role/binding 矩阵；
- 语义 composition hash 改变导致 cache miss；
- 同类别多个 role 的拒绝或显式绑定；
- 缺少角色、重复绑定、未知材质和无 remap palette 的 typed failure；
- ordinary/contrast 两条链路都保持显式语义 palette 选择；
- 语义组合到固化/VOX 导出的端到端回读。

## 7. 推荐的 4E 最小架构

### 7.1 新增只读 semantic colour requirements

在调用 style compiler 前，由本地 composition 确定性投影一个 immutable、snapshot-bound 摘要，至少包含：

- source snapshot hash；
- semantic composition hash；
- 已出现的 MaterialRole 及 cell count；
- explicitly approved remap 是否存在及 cell count；
- Unknown cell count；
- 可选的 PartRole/MaterialRole 计数，仅作事实而非坐标。

该摘要不包含逐 cell 坐标，不授予文件、Apply、Save 或 writer 权限。

### 7.2 让摘要参与请求和缓存身份

- 将 requirements 的 canonical hash 加入 compilation context、prompt、cache key 和缓存 JSON 验证；
- bump compiler/cache schema revision，旧缓存应安全 miss，不做隐式迁移；
- 语义 composition 改变必须触发新的样式编译或明确的本地重绑定路径，不能静默命中旧缓存。

### 7.3 冻结显式 typed role binding

推荐在 FinalContract 中采用 MaterialRole 级 binding：每个当前实际出现且非 Unknown 的材质恰好绑定一个
validated roleId；approved remap 恰好绑定一个 Remap role。PartRole 在 4E v1 不改变 palette family。

本地 validator 必须拒绝：缺失、重复、引用不存在 role、类别不兼容、无 remap range 却绑定 remap。
integrator 只消费已验证 binding 并物化 explicit masks，不再按类别 `First()` 猜测。

### 7.4 保持单一 colourizer 和现有导出

- 不新增第二套颜色应用器；继续使用 `Ra2VoxelColourizer`。
- Unknown cells 保留 base style；已绑定语义组由后置 explicit mask rule 覆盖。
- 继续验证 geometry/occupancy 不变、palette hash 匹配和 mask snapshot hash 匹配。
- ordinary/contrast 都走同一 effective semantic plan；contrast 不得改 semantic/remap/精确角色。
- 候选仍需用户显式 freeze/export；不接项目 Apply/Save，不写 VXL/HVA。

## 8. 失败和停止规则

以下情况必须 fail closed，并保留 source/working snapshot 与当前已固化候选：

- requirements、composition 或 mask 的 snapshot hash 不匹配；
- semantic requirements hash 与 cache/plan 身份不一致；
- required binding 缺失、重复、类别不兼容或引用不存在 role；
- approved remap 存在但 palette 无 remap range；
- colourizer 检测到 geometry、occupancy、palette 或 mask 不一致；
- compiler 返回 clarification、unsupported、malformed、timeout、cancel 或 provider failure。

不得把 unresolved semantic group 静默降级为“4E 成功”。若合同允许部分预览，UI 和结果类型必须明确显示
`Partial/Unresolved`，且不能固化为通过候选；该选择仍需 FinalContract 决定。

## 9. 实现风险与预计文件边界

当前 DocsOnly 审计风险为 `R0 / Immediate`。后续实现风险为 `R4 / StopForReview`，因为它会改变 AI tool
schema、style cache identity、plan/binding 合同和候选准入语义。

预计允许修改（以 FinalContract 为准）：

- Application voxel style/semantic internal contracts、integrator；
- IDE style compiler、preview coordinator 和 cache 序列化；
- 对应 Application/IDE 非 UI 测试；
- 4E Contract、Stage Ledger 和当前状态文档。

预计禁止修改：

- `ShellWindow.xaml` / `.cs`、全局布局和既有 AutomationIds；
- 4D `.semantic.json` v1 schema/store；
- 项目 Apply/Save、backup/rollback、Provider；
- public API allowlist、VOX canonical codec、VXL/HVA writer；
- INI parser、Field Registry、diagnostics、completion、Hover。

## 10. FinalContract 必须裁决的问题

1. 4E v1 是否采用“MaterialRole 决定颜色，PartRole 仅审阅”的最小边界。
2. role binding 是 style plan 的正式 typed 字段，还是独立的 validated binding plan。
3. 缺失材质角色时是整体失败，还是只读 partial preview 且禁止固化。
4. Unknown cells 是否明确保留 base style（本审计推荐是）。
5. composition 修改后必须重新调用模型，还是允许在角色全集足够时本地重绑定。
6. ordinary/contrast 候选的默认展示与固化资格。
7. 精确的 AutomationId/UI 影响；若需要 UI 改动，必须另出 UI contract 并等待批准。

## 11. 预计验证门禁

实现阶段至少需要：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.AssetHost.Tests\RA2IniEditor.AssetHost.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

并需要一轮真实 WPF 人工验收：修改语义标注后重新着色、所有材质可解释、Unknown 保持 base style、
ordinary/contrast 固化正确、导出 `.vox` 回读一致。VXL/HVA 和游戏内验收不属于 4E。

## 12. 当前停止点

代码事实审计已完成；运行时实现尚未开始。下一安全动作是编写
`ASSET-VOX-4E_MaskDrivenColourMaterializationFinalContract.md`，明确第 10 节的裁决并等待用户批准。

