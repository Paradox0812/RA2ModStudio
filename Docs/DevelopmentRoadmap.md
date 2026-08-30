# RA2IniEditor.IDE 开发路线图

更新时间：2026-08-26
目标来源：`Docs/ProductVisionAndRequirements.md`  
当前能力来源：`Docs/CurrentCapabilities.md`

## 1. 排序原则

1. 先建立可独立消费的高层 INI capability，再扩展素材和长任务。
2. 迁移现有算法，不复制 parser、diagnostics 或 edit planner。
3. Preview、Apply、Save、Artifact commit 的所有权分层，不把模型输出当权威。
4. 每个新领域先冻结数据契约和失败语义，再接 UI 或供应商。
5. 每阶段形成可演进的纵向切片，避免先搭建没有消费者的大框架。

## 2. 路线总览

| 阶段 | 目标 | 主要交付 | 当前状态 |
|---|---|---|---|
| HLI-0A | 审计现有能力和依赖锥 | 能力矩阵、复用决策、缺口 | Completed |
| HLI-0B | 冻结最小 UI-neutral capability contract | Application 候选、四个能力、Host-only 边界 | Confirmed / contract completed |
| HLI-1A0 | 依赖锥精确特征化 | 22 文件 Query 闭包、调用方影响、语义与等价测试门禁 | Completed / Verified 7/7 |
| HLI-1A1 | Headless Document Query 首切片 | Application/Application.Tests、Section、单文档 Reference | Completed / Verified |
| HLI-1A2 | Headless Diagnostics | neutral 唯一核心、IDE adapter、Validate Experimental API | Completed / Verified |
| HLI-1B | Headless Edit Preview | A2 等价 snapshot/plan/preview/change set | Completed / Verified |
| HLI-1C | IDE Host Boundary Confirmation | 复用 Preview seam、加固 Host binding -> 现有 A3 Apply/Undo | Completed / Verified |
| HLI-2A | 最小 Capability Gateway | descriptor、版本、限制、typed routing、取消 | Completed / Verified |
| HLI-2B | 内置 AI 改为 Gateway consumer | 唯一 Host adapter、public budget、发送前成本门禁 | Completed / Verified |
| HLI-2C | 首个高层 Agent 闭环 | 自然语言 -> query -> preview -> 用户 Apply -> diagnostics | Completed / Verified |
| POST-HLI-0 | Semantic / Host 优先级审计 | 代码事实、复用矩阵、语义优先路线 | Completed / DocsOnly |
| CONTENT-1 | 语义对象/模板层 | Schema/Resolve query、Section Preview、模板编译、IDE Apply | Completed / Verified |
| AGENT-MODE-1 | Chat / Work 与完整度路由 | 显式模式、skeleton/complete split、direct-fire complete profile | Completed / Verified |
| AGENT-KNOWLEDGE-1 | BuiltIn RA2 Skill 层 | 18 个按需领域/项目绑定 Skill、来源审计、prompt 边界 | Completed / Verified through CONTENT-2E |
| AGENT-SKILL-ROUTING-2 | Work 模型选 Skill Manifest | 第一轮元数据选取、Host 必选合并/模式/预算解析、第二轮正文注入 | Completed / automated verified |
| AGENT-CONTEXT-3 | 两阶段共享上下文与受限项目查询 | 同一会话/主题/快照投影；两调用之间通过 HLI 查询命名 Section/引用 | Completed / automated verified |
| CONTENT-2A | Techno Complete Profile | 现有 Techno Primary/Secondary 两条完整 direct-fire 链 | Completed / Verified |
| CONTENT-2B | Projectile / Warhead Profiles | Arcing/Homing Projectile 与 YR core Warhead | Completed / Verified |
| CONTENT-2C | AI Programming Tuple Profiles | 代码事实审计完成；typed tuple/动态 key/引用闭包契约与实现延期 | Audit completed / deferred by user |
| CONTENT-2D-0/1 | 对象闭包与当前文档注册 | internal closure model + deterministic numbered registration allocator | Completed / verified |
| CONTENT-2D-2 | 项目级多文档事务 | rules/art unified Preview、atomic Apply/rollback、compound Undo | Completed / Verified |
| CONTENT-2D-3+ | 完整对象 Profile 扩展 | Rules–Art body+Cameo binding completed；SuperWeapon UnitDelivery/GenericWarhead completed；Techno/Faction 与其它超武 remain | Partial / source-gated |
| CONTENT-2E | SuperWeapon / 支援技能 Profiles | Ares UnitDelivery、GenericWarhead typed complete；其它明确类型 generic reviewed fallback | Completed / automated verified |
| CONTENT-PROJECT-UI-1 | Work 项目提案接线 | 复用已有 rules/art template、Project Preview/Diff、atomic Apply/Undo | Completed / verified |
| DIFF-REVIEW-1 | 完整候选审阅 | canonical Result、unified Changes、有界对象上下文、文档/Section 导航 | Completed / automated verified；manual visual pending |
| HOST-1 | 独立 Agent Host | wire/session/permission、read/query/preview、IDE-mediated Apply | Deferred after stable CONTENT profiles |
| ASSET-ICON-1 | Cameo/Icon 流水线 | Manifest/INI binding/Existing Provider completed；palette、codec、generation、Host persistence remain | Partial / provider foundation verified |
| ASSET-VOX-1/4 | VOX 流水线 | canonical VOX、provider Host、GLB voxelization、Agent geometry/style review、人工语义蒙版、accepted candidate、verified VOX export | VOX review/export and session semantic masks completed；project Apply、VXL/HVA deferred |
| ASSET-SHP-1 | SHP 动画流水线 | frame spec、palette、anchor、preview、export adapter | Deferred |
| AUTOMATION-1 | Job/Event/Artifact Runtime | 状态机、取消、恢复、产物登记、审计 | Deferred |
| ASSEMBLY-1 | 多产物自动装配 | INI + icon + VXL/SHP 引用图、提交策略 | Deferred |
| RUNTIME-1 | 独立运行时测试宿主 | RA2TestHost、IRuntimeAdapter、Trace、deterministic tests | Deferred |

## 3. 近期连续路线：先让高层 Agent 可用

### HLI-1A0：Dependency Cone Characterization

已完成精确清单与测试契约，没有移动源码。已确认：

- 首个 Query 闭包为 22 个 Classification/Language 文件，不包含完整 TextModel；
- 63 个 production、41 个 test 文件受 namespace/assembly 影响；
- 内部实现保持 internal，以精确 IVT + project-level global using 控制改动面；
- 重复 Section occurrence、Reference 空成功/无法解析失败和既有双解析语义已锁定；
- 新 Application.Tests (`net8.0`) 将作为真正 Headless 证明。

### HLI-1A1/1A2/1B：最小纵向迁移

- `RA2IniEditor.Application` 和首个 Section/Reference 切片已按 R3/R2 契约完成。
- Diagnostics 唯一核心、IDE adapter 与 public Validate 已完成。
- HLI-1B 已完成：TextModel/change 与唯一 semantic Preview engine 位于 Application，
  IDE 保留 thin Host adapter；新增 11 个 Experimental public types，allowlist 精确为 29。
- HLI-1C 已完成：Host projection 校验 operation/span/candidate-change 闭合，Workspace
  admission 校验 invocation wrapper 实例绑定；public API 0 change，Shell/Apply/Save 不变。
- 每个能力必须有 snapshot、version、limits、cancellation 和 typed failure。
- 不移动 A3 Apply、Save、Shell、WPF 或 Registry runtime singleton。

### HLI-1C/2A/2B：接回真实产品

- IDE capture 当前快照；Application 负责确定性 query/preview；IDE host 负责 Apply。
- HLI-2A 已完成：Gateway 使用固定四项 immutable catalog 与 typed façade，只委托现有
  Query/Preview service；新增 6 个 Experimental public 类型，allowlist 精确为 35，
  Application 94/94、完整非 UI 2537/2537。
- Gateway 只路由已冻结能力，不提供任意文件、任意命令、Apply/Save 或 generic patch。
- HLI-2B 已把 A4-R1 唯一 Host adapter 改为 Gateway consumer，并保留 official/custom endpoint
  和 required-tool policy。当前采用 public 8 MiB/10k/128 budget；超限明确编辑在 provider
  请求前本地拒绝，advisory 仍可使用截断上下文。

### HLI-2C：近期产品验收点（已完成）

用户在 AI 面板表达明确的当前文件修改需求后，Agent 能够：

1. 查询真实 Section/字段/诊断；
2. 生成结构化修改；
3. 显示本地 Preview 和诊断差异；
4. 经确认应用为一个 Undo 单元；
5. IDE 重新分析并展示结果；
6. 不自动保存。

该闭环已通过 94/94、37/37、2549/2549 与 IdeOnly clean package 门禁。Minimum HLI-v1
可视为完成，但不包含独立 Agent host、模板、多文件、素材或 Runtime Test。

### POST-HLI-0：Semantic / Host 优先级审计（已完成）

代码事实确认应先完成 `CONTENT-1`，再冻结独立 Agent Host，最后进入素材侧：

- Gateway 已可被非 WPF 进程内宿主引用，但 snapshot 直接携带
  `IRa2FieldDefinitionProvider`，不是 wire DTO；
- 当前没有 CLI/IPC/RPC/permission/session/audit Host 基础；
- CONTENT-1 可以复用现有 schema、Section/reference、diagnostics、Preview 和 IDE Apply；
- 当前缺少 Schema/Resolve Gateway facts、CreateSection、语义 template model 与 expansion；
- 先做 Host 会围绕不完整的四能力目录冻结协议，CONTENT-1 后必然扩展。

完整证据见 `Docs/AUTOMATION-POST-HLI-0_SemanticHostPriorityCodeFactAudit.md`。

### AGENT-MODE-1 / AGENT-KNOWLEDGE-1（已完成）

- Chat 默认零编辑工具；Work 只进入既有结构化 Preview/Apply 边界。
- 明确“骨架/框架”才使用 skeleton；普通可用 direct-fire 武器链使用 complete profile。
- 当前 18 个 BuiltIn RA2 Skill 按领域或明确 project capability 渐进披露，禁止 scripts、外部根和直接工具权限。
- Field Registry 继续是字段 schema/trust 事实源，Content Profile 是对象完整度事实源，Host 是写入权限源。
- 最新门禁：Application 147/147、IDE non-UI 2580/2580、clean package 1171 files。

### CONTENT-2A（已完成）

- 新增现有 Techno Primary/Secondary 双 direct-fire 完整 profile：27 参数、6 Sections、30 operations。
- 循环/交替意图在模型调用前 fail closed；不把双槽或 Burst 冒充 Gattling/Cycle。
- 最新门禁：Application 148/148、IDE non-UI 2591/2591、clean package 1174 files。

### CONTENT-2B（已完成）

- 新增 Arcing Projectile、Homing Projectile 与 YR core Warhead 三个独立 complete profile。
- 弹道族互斥；Phobos/Vertical/Airburst 等未支持机制在 provider 前拒绝。
- YR core Warhead 遇到 `[ArmorTypes]` fail closed，不冒充 Ares custom-armor profile。
- 最新门禁：Application 151/151、IDE non-UI 2601/2601、clean package 1177 files。

## 4. 素材路线

### ASSET-ICON-1

先建立中立 `AssetRequest`、`ArtifactDescriptor`、palette/size/profile 和 provider
adapter。首个纵向切片建议是一个 Cameo：文本/参考图输入 -> 生成 -> 裁剪/量化
-> 预览 -> Manifest -> 受控复制到项目 -> 生成 INI binding proposal。

### ASSET-VOX-1

近期不追求直接写 VXL。先完成：

```text
描述/参考图 -> VOX -> body/turret/barrel 切片 -> SliceStack Manifest
-> VXLSE III import package -> 人工导入与修整结果登记
```

在真实切片导入样本通过前，不开发二进制 VXL writer。

2026-08-26 系统侦察进一步确认：生成侧应采用 provider-neutral 的参考图/image-to-3D adapter，
体素、palette、VOX 与 SliceStack 由本地确定性核心负责；VXLSE III 保持首版最终收口权威，
Vengi/C&C parser 只作交叉验证。当前 Existing Asset Provider 只能闭合最终 `.vxl/.hva`，不能把
VOX/PNG 中间产物冒充为成功 VXL。`ASSET-VOX-1A` 已实现分离式 Body/Turret/Barrel 装配契约、
受限 VXL/HVA 元数据探针和真实 Body/Turret 样本交叉验证。用户提供的 VXLSE file `1.3.9.3281` / product
`1.4.0.0` 及随包源码已经冻结 Downward/Rightward 切片寻址、direct-alpha occupancy、palette expansion 和
nearest-colour 行为；pivot/mount、normal、HVA 和游戏表现仍待 1B 之后验收。详见
`Docs/ASSET-VOX-1A_GoldenProbeAndSeparatedAssemblyFinalContract.md`。

2026-08-26：`ASSET-VOX-1B Canonical Voxel Core` 已完成并自动验证。当前已有 internal 单部件规范快照、
palette/quantizer、受限 MagicaVoxel VOX 交换、有界 Westwood VXL span 解码，以及 VXLSE-compatible RGBA/PNG
SliceStack。真实 PNG -> 用户提供 VXLSE -> decoded VXL 结构验收已闭合 `3x4x5` 非对称 5-cell fixture；
它仍不能宣称最终 VXL/HVA 或 GameReady。1C provider-neutral Host 已完成：可探测可信本地 provider、执行
单次有界 image-to-mesh 任务、返回经哈希验证的 GLB/PNG 候选及来源记录；真实模型 adapter、视觉验收、
项目接入和直接 VXL writer 仍分别冻结到后续独立阶段。

### ASSET-SHP-1

先冻结动画规格、帧尺寸、方向/序列、anchor、palette、remap 和验证结果，再决定
复用现有编码器、调用外部工具还是实现受限 writer。不得从图像生成结果直接
推断游戏格式写入成功。

## 5. 自动装配与安全策略

素材能力稳定后，Agent 才能生成跨产物 Assembly Plan：

- ArtifactId 与内容哈希；
- body/turret/barrel、SHP、Cameo 与 INI Section 的引用关系；
- 文件冲突、覆盖策略和目标路径；
- 预览、诊断、回滚与最终提交结果。

无人值守程度应由显式 policy 决定。默认策略继续是：读取和生成可自动执行，
项目写入先 Preview，覆盖/删除/保存和外部付费调用需要明确授权。

## 6. 主要返工防线

- 不把现有 IDE internal 类型直接宣布为稳定外部 API。
- 不复制 parser、diagnostics、reference 或 Preview 算法。
- 不让 Gateway 持有 active editor、Save 或 WPF 生命周期。
- 不先建设通用 Job/Event 大框架再寻找使用者。
- 不将 provider DTO、图像模型输出或 VXLSE GUI 状态作为领域事实。
- 不把项目文本搜索冒充语义引用查询。
- 不把 VOX 切片准备写成“已经生成 VXL”。

## 7. 下一安全入口

当前停止点是：

```text
AGENT-QUERY-2 completed / automated verified
```

AGENT-QUERY-2 已让 Work 在捕获项目中搜索、补查并规范绑定现有对象，同时保持 Project Diff/显式 Apply/
compound Undo 权威。下一步应先用真实 DeepSeek 验收自然语言实体检索；UI 只读摘要按
`AGENT-TRACE-1_CompactRetrievalSummaryUiContract.md` 单独批准后实现。随后再决定扩展下一批 source-backed
SuperWeapon profile 或审计自动化游戏测试 Host。`ASSET-HOST-1` 的显式持久化/冲突/回滚仍后置，
CONTENT-2C AI 写入继续冻结。
HOST-1 在这些高优先级语义面稳定后
冻结 wire/session/permission。持久化模板、外部/可执行 Skill、multi-file、public Apply/Save、
Job Runtime 和素材写入仍不在范围内。
## ASSET-VOX next boundary after 1E-UI

`ASSET-VOX-1E-UI-R2` is implemented and automated verified. The review workspace now accepts canonical VOX or an
explicit VXL/PAL pair, and ordinary shading no longer depends on remap metadata. The next asset stage should not expand
the same UI ad hoc.
Choose one separately contracted direction after manual review:

1. persistent semantic-mask interchange/import beyond the completed 4B session editor; or
2. an accepted-preview handoff that keeps AssetHost/project-write/VXL-HVA authority explicit and reviewable.

2026-08-28：第二条中的最小 handoff 已由 `ASSET-VOX-3B` 完成：用户显式固化一个不可变候选，并可导出经
canonical codec 回读验证的 VOX 副本。它不是项目 Apply/Save，也不写 VXL/HVA。下一安全路线应在以下两者中
单独立约：语义材质/部件识别与上色，或 VOX -> 分件 VXL/HVA 的确定性 materialization。

2026-08-29：第一条的会话内 authoring 已由 `ASSET-VOX-4A/4B` 完成：DeepSeek 提供可选初稿，用户可在现有
3D 工作区用区域赋值和稀疏表面画笔完成材质边界。持久化 mask interchange 和 VXL/HVA 仍需独立契约。

2026-08-30：`ASSET-VOX-4B-STROKE-1` 已把稀疏表面画笔升级为单事务连续笔划，并增加部件/材质审阅配色。
自动验证完成，当前安全停止点是用户物理 WPF 鼠标与 DPI 验收。通过后再决定持久化 mask interchange、
上色工作流深化或 VOX → 分件 VXL/HVA；不在本阶段自动展开。
