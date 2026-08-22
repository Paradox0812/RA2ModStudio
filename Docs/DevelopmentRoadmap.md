# RA2IniEditor.IDE 开发路线图

更新时间：2026-08-23
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
| HLI-2C | 首个高层 Agent 闭环 | 自然语言 -> query -> preview -> 用户 Apply -> diagnostics | Final contract / awaiting implementation approval |
| CONTENT-1 | 语义对象/模板层 | 新 Section、对象模板、跨文件计划、Artifact plan | Deferred |
| ASSET-ICON-1 | Cameo/Icon 流水线 | provider abstraction、palette、preview、manifest、INI binding | Deferred |
| ASSET-VOX-1 | VOX/SliceStack 流水线 | VOX、切片、part/pivot/palette manifest、VXLSE III 导入包 | Deferred |
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

### HLI-2C：近期产品验收点

用户在 AI 面板表达明确的当前文件修改需求后，Agent 能够：

1. 查询真实 Section/字段/诊断；
2. 生成结构化修改；
3. 显示本地 Preview 和诊断差异；
4. 经确认应用为一个 Undo 单元；
5. IDE 重新分析并展示结果；
6. 不自动保存。

该闭环通过后，高层 INI 接口才可视为近期可用。

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

当前下一入口是：

```text
HLI-2C 首个高层 Agent 闭环的代码事实审计与最终契约
```

HLI-2C 代码事实审计与最终契约已完成，见
`Docs/AUTOMATION-HLI-2C_FirstAgentLoopCodeFactAudit.md` 与
`Docs/AUTOMATION-HLI-2C_FirstAgentLoopFinalContract.md`。确认后按 2C-1..2C-4 连续实施；
不得新增 public Agent façade、Apply/Save、wire 或 Job Runtime。
