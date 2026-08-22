# AUTOMATION-HLI-1C Host Boundary Code-Fact Audit

审计日期：2026-08-22
状态：Completed / Read-only code-fact audit
前置证据：`Docs/AUTOMATION-HLI-1B_StageLedger.md`

## 1. 审计目标

核实 HLI-1B 的 Application Preview 结果如何在未来 Gateway consumer 场景下进入现有
A3 Host 生命周期，并回答以下问题：

1. 是否需要新增 public Apply API、proposal handle 或 Preview store；
2. 是否需要让 Workspace 接受外部 `RegisterPreview/AdoptPreview`；
3. 现有 generation、单槽位、显式确认、live currency、single-use 和 Undo 是否足够；
4. HLI-1C 是否需要修改生产代码、Shell 或 public API；
5. 哪些边界仍缺少直接测试证据。

本审计只读取源码、测试和权威文档，没有修改生产代码。

## 2. 已核实的权威边界

- HLI-0B：Application 不持有 active Preview，不执行 Apply/Undo/Save；IDE Host 拥有
  active slot、generation、currency 和事务提交。
- A3：调用方只提交 `PreviewId + ExplicitConfirmationGranted`；Workspace 在锁内 claim
  后才进入唯一 `IRa2EditorTransactionPort`。
- HLI-1B：Application 是 semantic Preview 唯一算法权威；IDE Preview service 只做
  snapshot/plan 投影和 Host wrapper 映射。
- HLI-2A/2B 尚未实现：当前没有 Capability Gateway、wire DTO、CLI 或独立 Agent host。

## 3. 当前真实调用链

```text
Ra2AiAuthoringCoordinator
  -> IRa2IniAuthoringWorkspace.Preview(snapshot, plan, token)
     -> generation++ / active slot clear
     -> IRa2IniEditPreviewService.Preview(...)
        -> Ra2AutomationEditPreviewService.PreviewForHost(...)
        -> Ra2IniEditPreview.FromAutomation(...)
     -> generation still current ? store one successful Host Preview : do not store
  -> A4 proposal/policy/presentation
  -> IRa2IniAuthoringWorkspace.Apply(PreviewId + explicit confirmation)
     -> claim and clear active slot before transaction
     -> IRa2EditorTransactionPort.Apply(Host Preview)
        -> Shell captures live Session/Editor/Registry/Caret
        -> Ra2IniEditPreviewCurrencyEvaluator
        -> SessionController.ApplyProgrammaticText
        -> one editor sync + one semantic Undo unit
        -> no Save
```

未来 Gateway adapter 的安全插入点已经存在：它应在 IDE 内实现
`IRa2IniEditPreviewService`，调用 Gateway 后用 `Ra2IniEditPreview.FromAutomation` 生成 Host
wrapper。Workspace 仍从调用开始前控制 generation，因此无需开放第二个注册入口。

## 4. 类型、所有权与可见性事实

| 概念 | 当前类型 | 可见性 | 唯一所有者 / 生命周期 |
|---|---|---|---|
| immutable document/registry input | `Ra2AutomationDocumentSnapshot` | public Experimental | caller；单次调用 |
| bounded edit intent | `Ra2AutomationEditPlan` | public Experimental | caller；单次调用 |
| semantic Preview result | `Ra2AutomationEditPreviewResult` | public Experimental，构造器 internal | Application service 创建；调用方只读 |
| Host projection | `Ra2IniEditPreview` | internal | IDE adapter；未注册前无 Apply 权威 |
| Preview invocation seam | `IRa2IniEditPreviewService` | internal | Workspace dependency |
| active Preview/generation | `Ra2IniAuthoringWorkspace` | internal | Workspace；单槽、非持久化 |
| Apply request/result | `Ra2IniEditApplyRequest/Result` | internal | Host 调用栈 |
| live currency | `Ra2IniEditPreviewCurrencyEvaluator` | internal | 提交瞬间计算，不缓存 |
| transaction authority | `IRa2EditorTransactionPort` | internal | Shell private implementation |
| AI proposal/policy | `Ra2AiAuthoringCoordinator` | internal | A4 presentation lifecycle |

所有 A3 Host 类型均为 internal。Application Experimental 命名空间没有 public Apply、
Save、Store、Session、Undo 或 Transaction surface；exported allowlist 保持 29。

## 5. PreviewId 的真实语义

`Ra2AutomationEditPreviewResult.PreviewId` 是成功 Preview 的相关身份，但不是全局可应用
令牌。只有满足以下条件后，它才成为当前 Host 的一次性 Apply 身份：

1. 结果由 Host admission seam 转换为 `Ra2IniEditPreview`；
2. Workspace 调用开始时捕获的 generation 仍为当前代次；
3. 结果成功且被写入唯一 active slot；
4. 后续 Apply 的 ID 与 active slot 完全匹配；
5. 用户显式确认；
6. Shell 在提交瞬间通过全部 live currency 检查。

因此，从 Gateway 或其他调用方获得一个 PreviewId，不能直接调用事务端口，也不能使
Workspace 自动承认它。当前没有全局 Preview 字典、静态 store 或跨进程恢复机制。

## 6. 已有安全证据

| 边界 | 当前证据 |
|---|---|
| 未确认不消费 | `Apply_RequiresConfirmationThenConsumesPreviewExactlyOnce` |
| 匹配确认只消费一次 | 同一测试及 Coordinator replay 测试 |
| 新 Preview 替换旧槽位 | `Preview_ReplacingActiveSlotInvalidatesPreviousPreview` |
| 旧异步结果不能覆盖新结果 | `Preview_WhenOlderGenerationCompletesLast_DoesNotReplaceNewerPreview` |
| 显式 invalidate/discard | Workspace 与 Coordinator tests |
| live identity/version/session/editor/registry 检查 | Currency evaluator tests |
| Shell 唯一事务顺序 | `Ra2AuthoringShellTransactionBoundaryTests` |
| 不调用 Save/Writer | Shell transaction static boundary |
| Application 无 Apply surface | Application/IDE boundary tests |
| HLI-1B public result identity projection | `Ra2IniEditPreview.FromAutomation` 与 Preview tests |

本轮实际运行：

```text
Application.Tests: Passed 82/82
Host lifecycle targeted baseline: Passed 32/32
Latest trusted full non-UI baseline: HLI-1B Passed 2526/2526
```

## 7. 仍缺少的直接证据

### 7.1 Gateway-like adapter proof

当前 concrete service 已走 Application，但没有一项测试以“未来 Gateway adapter”形态
实现 `IRa2IniEditPreviewService`，再证明结果只能通过 Workspace generation/active slot
进入 A3。

### 7.2 Unadmitted result proof

尚无直接测试证明：在 Workspace 外单独生成的 public PreviewId，提交给 Workspace 必须
得到 `PreviewUnavailable`，不能命中任何全局状态。

### 7.3 Projection integrity gap

`Ra2IniEditPreview.FromAutomation` 已检查 DocumentId、Version、RegistryRevision、PlanId 和
FilePath，但尚未：

- 逐项确认 result operation evidence 与调用方 plan 的 kind/section/key/value 一致；
- 确认 operation evidence span 落在 snapshot text 边界内；
- 用既有 `Ra2TextChangeSet.Apply(snapshot.Text)` 验证 CandidateText 与 Changes 一致。

canonical Application service 当前会产生一致结果，但未来 Gateway adapter 的响应错配不能
只依赖 PlanId 防御；PlanId 是调用方提供的相关 ID，不是内容指纹。

### 7.4 Failed transaction single-use

已有成功后的 replay 测试；仍应直接锁定事务端口返回 stale/rejected 或抛出非致命异常后，
该 Preview 同样已经消费，不能重放。

### 7.5 Workspace invocation binding gap

Workspace 只判断 service 返回的 Preview 是否成功，没有显式确认返回 wrapper 的 Snapshot
和 Plan 就是本次 invocation 的 immutable 实例。Shell live currency 会阻止多数错误提交，
但错误 wrapper 仍可能暂时成为活动提案。Workspace 应在写入 active slot 前以 exact
instance binding guard 拒绝 adapter 替换 authority envelope。

### 7.6 Surface freeze

已有零散反射/静态断言，但缺少一个 HLI-1C 聚合门禁，精确锁定 Workspace、Preview seam、
Apply request 和 Transaction Port 不出现 public、Save、Session injection 或 raw result
registration surface。

其中 7.3/7.5 需要两处窄 internal 防御性强化；其余只需测试。它们不改变正常 canonical
路径、public API、Apply/Undo 或 Shell 行为。

## 8. 被否决的实现方向

### 新增 `RegisterPreview` / `AdoptPreview`

否决。外部结果在完成后直接注册会绕开 Workspace 在 invocation 开始前建立的 generation，
使较旧异步结果可能覆盖较新 active slot；还会扩大可伪造/误用入口。

### 新增 public proposal-handle store

否决。Application/Gateway 不得持有 active editor 生命周期；PreviewId 已足够表达相关身份，
实际 Apply 权威来自 Workspace active slot，而不是 ID 本身。

### 将 Apply 暴露为 Gateway capability

否决。它会把 live editor、确认、Undo 和 Save 边界推向 Application/Gateway，违反 HLI-0B。

### 在 HLI-1C 提前实现 Gateway adapter

否决。HLI-2A 尚未冻结 Gateway descriptor/invocation contract，提前实现只能产生临时接口
和返工。HLI-1C 只锁定 adapter 必须实现的现有 IDE seam。

## 9. 兼容性风险

- 当前 Host path 使用 `PreviewForHost`，保留既有不受 public 8M/10k 限制的行为；未来
  Gateway public path 使用 8M/10k。HLI-2B 切换内置 AI 时必须明确 budget policy，HLI-1C
  不静默改变它。
- A3 只有一个程序化语义 Undo 状态，不是多级事务栈；HLI-1C 不扩展 Undo。
- Shell code-behind 是真实 transaction authority，但本阶段只读审计，禁止修改。
- `Ra2AutomationEditPreviewResult` 是进程内 CLR result，不是 wire DTO；HLI-2A 不能直接
  推断其 JSON/IPC 稳定性。

## 10. 审计结论

现有 `IRa2IniEditPreviewService` 是未来 Gateway consumer 的正确 Host admission seam，
不需要新增 handle/store/register API；Workspace/A3/Shell 的所有权方向正确。

但 Host projection 和 Workspace admission 各缺一层响应错配防御。建议 HLI-1C 只修改
`Ra2IniEditPreview.cs` 与 `Ra2IniAuthoringWorkspace.cs` 的 internal guard，并增加边界测试；
不新增 public API、不修改 Shell。若这两处之外仍需生产改动，应停止并生成 R3 修订契约。
