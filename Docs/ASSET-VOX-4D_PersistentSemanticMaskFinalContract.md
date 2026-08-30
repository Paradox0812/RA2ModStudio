# ASSET-VOX-4D — Persistent Semantic Mask Final Contract

状态：Completed / implemented / automated verified / physical WPF acceptance pending  
日期：2026-08-30  
风险：R4 / StopForReview

实现结果：用户于 2026-08-30 批准最终契约；4D-0 → 4D-5 已连续完成。实现严格保持本契约的三层
provenance、项目内路径、显式 Save/Open、原子写入和全有或全无载入边界。自动验证结果详见
`Docs/ASSET-VOX-4D_StageLedger.md`。

## 1. 交付目标

在现有“语义”页增加显式保存/载入，使以下作者状态可跨 IDE 会话恢复：

1. 已被用户接受的 Agent 区域建议；
2. 人工区域部件/材质/阵营色覆盖；
3. 人工体素画笔覆盖。

恢复后继续使用既有优先级、着色和导出链。sidecar 不包含几何、色板、最终 RGB、相机、undo/redo 或临时笔划。

## 2. 文件与路径契约

- 扩展名固定为 `.semantic.json`。
- 推荐文件名为 `<模型完整文件名>.semantic.json`，例如 `body-candidate.vox.semantic.json`、
  `tank.vxl.semantic.json`。
- 保存和载入都使用原生 Save/Open 对话框；默认目录为当前项目或当前文件源目录。
- 目标必须位于当前项目根目录内；拒绝项目外路径、目录目标、重解析点文件，以及包含重解析点的项目内目录链。
- 保存只由用户点击“保存分划…”触发；不自动写入、不参与项目 Apply/Save、不随普通模型保存暗中执行。
- 覆盖既有 sidecar 必须经过原生 overwrite prompt；确认后复用 `AtomicTextFileWriter` 原子替换。
- 载入只读取用户选择的 sidecar；第一版不自动发现/自动应用，避免旧 sidecar 在打开模型时制造隐式状态变化。
- 文件最大 `32 MiB`；使用严格 UTF-8，writer 不写 BOM。reader 允许标准 UTF-8 BOM，但拒绝无效字节。

sidecar 不保存或重建工作几何。若当前内存几何尚未导出，保存 sidecar 仍安全，但以后必须先载入或重建
完全相同 canonical hash 的模型才能恢复。

## 3. v1 序列化形状

根对象固定为：

```json
{
  "schema": "ra2-voxel-semantic-sidecar",
  "version": 1,
  "sourceSnapshotHash": "64-char SHA-256",
  "evidencePackageHash": "64-char SHA-256",
  "cellCount": 20261,
  "manualLayerHash": "64-char SHA-256",
  "agentSuggestionsAccepted": true,
  "agentSuggestions": [],
  "humanRegionOverrides": [],
  "humanCellGroups": []
}
```

### 3.1 区域项

`agentSuggestions` 与 `humanRegionOverrides` 使用同一字段形状：

```text
regionId
partRole
materialRole
remapIntent
confidence
reason
```

- enum 使用名称字符串；reason 规范化为单行且最多 512 字符。
- Agent 数组只在 `agentSuggestionsAccepted=true` 时允许非空，且不得包含 `ExplicitlyApproved`。
- human 数组不得包含 `Candidate`；`ExplicitlyApproved` 是唯一持久化阵营色批准状态。
- 每个数组最多 48 项，`regionId` 在数组内唯一且必须存在于当前 evidence；Agent confidence 必须为 0..1，
  human confidence 固定为 1。

### 3.2 人工 cell 分组

为避免百万项重复字符串，按赋值分组：

```text
partRole
materialRole
remapIntent
reason
cellIndices[]
```

- cell index 使用 canonical occupied-cell ordering，组内升序唯一。
- 总 index 数不得超过 `cellCount` 和 `Ra2VoxelSceneSnapshot.MaximumOccupancyCount`。
- 每个 index 必须在 `[0, cellCount)`，所有组之间不得重复。
- part/material 必须明确；remap 只能 `None` 或 `ExplicitlyApproved`；reason 最多 128 字符。
- 保存时按 assignment tuple、cell index 确定排序；相同作者状态产生稳定 JSON 内容。
- 载入后用现有 `Ra2VoxelSemanticManualMaskLayer` 重建并校验 `manualLayerHash`。

### 3.3 JSON 兼容性

- `schema` 或 `version` 不支持时拒绝，不尝试降级或猜测。
- v1 拒绝重复属性、未知属性、缺失必需属性、错误类型、非法 enum 名称、NaN/Infinity、重复 region/index
  和超限集合。
- 字段名区分大小写；enum 名称按当前契约精确匹配。
- 后续新增字段必须提升 schema version 或明确更新 v1 兼容规则，不能让旧 reader 静默解释新语义。

## 4. 保存快照契约

保存开始时一次性捕获：

```text
working snapshot hash + cell count
evidence package hash
accepted Agent suggestions（仅已接受）
human region overrides
human manual cell layer + layer hash
semantic authoring revision
```

- 没有当前几何、没有 evidence 或没有任何已接受/人工分划时禁用保存。
- 捕获后序列化和写入可以后台执行；完成时若 authoring revision 已变化，文件仍是开始时的一致快照，
  但工作区保持 dirty，并提示“保存的是较早版本，请再次保存”。不得把较早写入冒充当前已保存。
- 写入失败或取消不改变 dirty、当前语义层、style preview、候选或模型。
- 成功写入当前 revision 后记录当前 sidecar 路径并清除 dirty；不清除 undo/redo。

## 5. 载入与原子恢复契约

载入分两步，提交前不得修改 ViewModel：

1. 严格读取、解析、结构与资源校验；
2. 针对当前 `ActiveGeometrySnapshot` 本地重建 evidence，验证：

```text
sourceSnapshotHash == current CanonicalHash
evidencePackageHash == rebuilt PackageHash
cellCount == current OccupancyCount
all region IDs exist
reconstructed manual layer hash == manualLayerHash
```

只有全部通过才一次性替换 Agent accepted layer、human region layer、human cell layer 和 evidence。成功后：

- 清空语义 undo/redo，载入内容成为新的会话基线；
- 清除旧 style preview 与 frozen candidate 一次，因为其 composition 已过期；
- 切换到 Semantics/浏览模式，刷新列表和正式场景一次；
- dirty=false，记录 sidecar 路径，并显示恢复的区域/体素数量。

任何失败均保持当前 evidence、suggestions、manual layers、undo/redo、composition、style preview、候选和模型不变。
哈希失配只给出可读原因，不支持强制载入、部分载入、最近坐标迁移或 region 名猜测。

## 6. Dirty 与覆盖保护

新增 session-only `SemanticAuthoringRevision` 和 `IsSemanticSidecarDirty`：

- 接受/丢弃 Agent 建议、区域人工覆盖、撤销区域覆盖、画笔 commit、画笔 undo/redo 都推进 revision 并置 dirty。
- review dimension、选中区域、相机、编辑模式、画笔大小和临时笔划不影响 dirty。
- 成功保存当前 revision或成功载入后清除 dirty。
- 载入 sidecar 覆盖 dirty 状态前显示一次确认；取消则完全不读取或替换。
- 选择新源、生成新源或采用新工作几何前，如果 dirty，现有工作区按钮路径显示一次“未保存分划将丢失”确认。
- 项目切换/窗口关闭的全局拦截需要 Shell 生命周期修改，本阶段冻结 Shell，因此不声称提供全局关闭保护；
  状态行必须持续显示“分划有未保存修改”。

## 7. UI 契约

只修改现有“语义”页第一行工具区，不增加窗口或新面板：

1. 在 `撤销 / 重做` 后增加 `保存分划…`、`载入分划…` 两个按钮。
2. 继续复用现有语义状态文本；其下一行增加紧凑 persistence status：
   `未保存 / 已保存：文件名 / 已载入：文件名 / 失配或失败原因`。
3. 保存/载入忙碌时禁用语义编辑和另一个持久化动作；不弹出自定义进度窗口。
4. Save/Open/Overwrite/Discard 使用原生对话框；业务失败写状态行，不弹重复错误框。
5. 不改变现有主 3D、详情页尺寸、splitter、横向滚动或 Shell 布局。

新增 AutomationIds：

```text
VoxelStyle.Semantics.SaveSidecar
VoxelStyle.Semantics.LoadSidecar
VoxelStyle.Semantics.PersistenceStatus
```

全部既有 4A/4B/FIX2/STROKE-1 AutomationIds 保持不变。

## 8. 类型化结果

IDE-internal failure kinds 至少区分：

```text
None
InvalidPath
OutsideProject
ReparsePointRejected
NotFound
TooLarge
InvalidUtf8
InvalidJson
UnsupportedSchema
InvalidShape
ResourceLimitExceeded
SnapshotMismatch
EvidenceMismatch
LayerHashMismatch
IoFailure
Canceled
```

保存和载入返回不可变 result，不向 UI 泄露原始异常；无 public C# API。

## 9. 允许文件

```text
RA2IniEditor.IDE/AssetAuthoring/Ra2VoxelSemanticSidecarStore.cs（新增，唯一 serializer/store）
RA2IniEditor.IDE/ViewModels/AssetAuthoring/Ra2VoxelStyleWorkspaceViewModel.cs
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml
RA2IniEditor.IDE/Views/AssetAuthoring/Ra2VoxelStyleWorkspaceView.xaml.cs
RA2IniEditor.Tests/IDE/Ra2VoxelSemanticSidecarStoreTests.cs（新增）
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceViewModelTests.cs
RA2IniEditor.Tests/IDE/Ra2VoxelStyleWorkspaceUiContractTests.cs
Docs/ASSET-VOX-4D_*.md
Docs/PublicApiLedger.md
Docs/DecisionLog.md
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
实现完成后必要的产品说明文档
```

不得修改项目文件或增加依赖。若 `AtomicTextFileWriter` 因可访问性不能由 IDE 复用，先停止；不得复制一个
新的原子写入器或扩大 Infrastructure public API。

## 10. 冻结边界

- ShellWindow、ShellViewModel、全局关闭/项目切换生命周期、菜单、toolbar、docking。
- Tencent/DeepSeek、prompt、轮次、Provider Host、网络和真实调用。
- working geometry、质量/对称/Agent 算法、VOX/VXL/HVA reader/writer、palette/style compiler/colourizer。
- 项目 Apply/Save、INI、Field Registry、Diagnostics、Completion、legacy。
- `Ra2VoxelSceneSnapshot` schema、现有 Application 语义类型和 public API。
- 不实现迁移、merge、自动保存、云同步、跨哈希重投影或 sidecar 内嵌几何。

## 11. 分阶段计划

| 阶段 | 内容 | 必选门 |
|---|---|---|
| 4D-0 | 事实审计、R4 契约、schema/public ledger candidate | 用户最终审批前 runtime 0 change |
| 4D-1 | 唯一 sidecar DTO/store、严格 parser、确定性 writer、项目路径与原子写入 | round-trip/negative/security tests 全绿 |
| 4D-2 | ViewModel snapshot/save/load 原子事务、revision/dirty、状态恢复 | stale/失配/失败零污染测试全绿 |
| 4D-3 | 紧凑 Save/Load/status UI、原生对话框和局部 discard guard | UI contract + View 构造测试全绿 |
| 4D-4 | 保存→新 VM→载入→composition/hash 等价；回归着色失效一次 | focused Application/IDE 全绿 |
| 4D-5 | 顺序 full build/test、diff/边界审计、文档、clean package、手动烟测 | 全部门通过才可 automated complete |

## 12. 自动验证矩阵

### Store / schema

- 相同输入产生稳定 JSON；保存→载入后 Agent/human region/cell layer 与 layer hash 相等。
- 空/最大合法集合、cell 分组排序、Reason 规范化、UTF-8 中文。
- 覆盖路径、项目外、reparse、超限、无效 UTF-8/JSON、重复/未知字段、schema/version、enum、region/index、
  hash、IO 和取消失败。
- 写入失败保留旧 sidecar；成功无临时文件残留。

### ViewModel

- region、remap、cell stroke、undo/redo、accepted suggestions 都能 round-trip，来源优先级不变。
- Begin load 不改变状态；成功只发布一次；所有失败逐字段证明旧状态/preview/candidate/history 不变。
- save 期间 revision 变化保持 dirty；当前 revision 成功保存才清 dirty。
- load 清空 local undo/redo、使旧 style/frozen candidate 失效一次，但不改 working geometry。

### UI

- 3 个 AutomationId 唯一；现有 semantic/viewport IDs 和布局结构不变。
- dirty discard 仅出现在既有工作区破坏性按钮路径；取消不继续操作。
- 1920×1080、100%/125% DPI 下第一行自然换行、状态可读、主视图不缩放跳变。

### 命令

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Ra2VoxelSemanticSidecar|FullyQualifiedName~Ra2VoxelStyleWorkspace"
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
dotnet test .\RA2IniEditor.AssetHost.Tests\RA2IniEditor.AssetHost.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

真实 Provider 不是验证项。

## 13. 手动验收

1. 对当前模型建立 accepted Agent、区域人工覆盖和多部件 cell 笔划。
2. 点击“保存分划…”，在项目内保存 `模型.vox.semantic.json`。
3. 关闭 IDE；重新打开同一项目和模型，点击“载入分划…”。
4. 确认部件/材质显示、阵营色批准和 cell 边界与保存前一致。
5. 确认载入后 undo/redo 为空；新笔划可产生新的撤销历史。
6. 修改或换用不同几何后尝试载入，必须拒绝并保留当前状态。
7. 损坏或截断 sidecar 必须提示失败且不清除当前分划。
8. 保存后修改一个笔划，状态显示未保存；再次保存后恢复已保存。
9. 在 100%/125% DPI 下检查按钮换行和主视图稳定性。

## 14. 自审

### 已闭合

- 来源权威：分层保存，不把 Agent 内容提升为人工覆盖。
- 身份闭合：snapshot + evidence + layer 三重哈希；失配不迁移、不部分载入。
- 原子性：保存原子替换；载入先构造后一次发布；失败零污染。
- 规模闭合：32 MiB、48 region、1,000,000 cell 与重复检测上限。
- 路径闭合：项目内、无 reparse、显式 Save/Open/Overwrite。
- 复用闭合：现有 evidence、layer、composer、AtomicTextFileWriter；没有第二套语义或写入算法。
- UI 闭合：只加两个按钮和一条状态，不改变主布局。
- 兼容闭合：严格 version 1，未知版本不猜测；public C# API 零变化但 serialized contract 入 ledger。

### 明确边界

- 不保存几何；没有匹配模型就无法恢复。
- 不自动保存、不自动载入、不做跨哈希迁移。
- 不保存 undo/redo、相机、选中、style preview 或最终候选。
- Shell 冻结，因此不承诺窗口关闭/项目切换的全局未保存拦截；只覆盖体素工作区内破坏性按钮路径。
- 真实 WPF 对话框、路径和 DPI 仍需人工验收。

### 审查结论

该设计保存的是用户真正关心的分层语义，而不是颜色截图；同时避免 AI 权威升级和旧蒙版误套新模型。
未发现必须修改 writer、Shell、Application schema 或 public API 的依赖。结论：**契约可实施，但 R4 最终
审批前不得进入 4D-1。**

## 15. 最终审批口令

```text
批准 ASSET-VOX-4D 最终契约，连续执行 4D-0 → 4D-5；新增项目内 .semantic.json 持久化格式，但不修改 Shell、项目 Apply/Save、VOX/VXL/HVA writer、Provider、public C# API 或几何/色板语义。
```
