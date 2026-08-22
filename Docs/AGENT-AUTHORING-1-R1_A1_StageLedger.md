# AGENT-AUTHORING-1-R1 A1 Stage Ledger

状态：Completed  
日期：2026-07-23  
风险：R3（Field Registry 运行时发布生命周期增加 internal Revision/Snapshot）  
权威契约：`Docs/AGENT-AUTHORING-1-R1_A1_ContinuousContract.md`

## 1. 阶段结果

| Task Card | 状态 | 关键结果 | 定向验证 |
|---|---|---|---|
| A1-B1 | Completed | Runtime Service 发布稳定 Provider Snapshot；初始 Revision=1，成功 Reload 每次递增一次 | FieldRegistryRuntimeServiceTests 18/18 |
| A1-A1 | Completed | 中立 Request 与 UI 无关 DiagnosticFact | Contract tests |
| A1-A2 | Completed | 显式成功/失败 Result；诊断防御性复制；安全失败摘要 | Contract tests 3/3 |
| A1-A3 | Completed | 复用 TextModel、Semantic Builder、CurrentFileReadonlyDiagnosticService 的只读门面 | Service tests |
| A1-C | Completed | Snapshot 漂移、诊断等价、源码边界和 A0 特征门禁 | 合并定向 45/45 |

## 2. 实际代码形状

```text
FieldRegistryRuntimeService
  -> CaptureProviderSnapshot(): Provider + Revision
  -> Ra2LanguageAnalysisRequest
  -> Ra2IniLanguageAnalysisService
       -> Ra2IniTextDocumentParser
       -> Ra2DocumentSemanticModelBuilder
       -> CurrentFileReadonlyDiagnosticService
  -> Ra2IniLanguageAnalysisResult
       -> TextDocument + SemanticModel + DiagnosticFact[]
```

- Loader 在发布 gate 外执行，构造完 next provider/state/provenance 后一次发布 Snapshot。
- 旧 Snapshot 持有旧 Provider 实例和旧 Revision，不因后续 Reload 漂移。
- 一次分析只消费 Request 捕获的 Provider，不读取 `FieldRegistryRuntimeService`。
- 现有诊断结果按原顺序逐属性复制，不复制字段、引用或链路诊断算法。
- `AnalysisVersion` 是诊断关联标签；Field Registry Revision 是独立知识版本。

## 3. Diff Intent Table

| 文件 | 意图 | 行为影响 |
|---|---|---|
| `Services/Ra2FieldRegistryProviderSnapshot.cs` | 新增稳定 Provider/Revision 值对象 | internal only |
| `Services/FieldRegistryRuntimeService.cs` | 原子发布 Provider Snapshot，保留现有 getter/Reload | Provider 优先级与加载语义不变 |
| `Language/Ra2LanguageAnalysisRequest.cs` | 中立单文档输入 | internal only |
| `Language/Ra2DiagnosticFact.cs` | 去 ViewModel 的诊断事实 | internal only |
| `Language/Ra2LanguageAnalysisFailureKind.cs` | 显式失败分类 | internal only |
| `Language/Ra2IniLanguageAnalysisResult.cs` | 成功/失败不变量与只读结果 | internal only |
| `Language/IRa2IniLanguageAnalysisService.cs` | A2 可复用分析端口 | internal only |
| `Language/Ra2IniLanguageAnalysisService.cs` | 组合现有语言/诊断能力 | 无 Shell 接线 |
| 三个 A1 测试文件 | 契约、等价、Snapshot 与边界证明 | 测试 only |
| `FieldRegistryRuntimeServiceTests.cs` | Revision/旧 Snapshot 回归 | 测试 only |

## 4. Public / Contract Ledger

无 public API 变更。新增类型全部为 `internal`。

| internal contract | 预期下一用途 | 稳定性 | 证据 |
|---|---|---|---|
| `Ra2FieldRegistryProviderSnapshot` | A2 Preview 捕获字段知识版本 | A1 accepted | Runtime tests |
| `Ra2LanguageAnalysisRequest` | 当前/候选文本统一分析输入 | A1 accepted | Contract/boundary tests |
| `Ra2DiagnosticFact` | Plan/Preview 的中立诊断事实 | A1 accepted | Diagnostic equivalence |
| `IRa2IniLanguageAnalysisService` | A2 Authoring Workspace 只读端口 | A1 accepted | Service tests |
| `Ra2IniLanguageAnalysisResult` | A2 before/after 分析结果 | A1 accepted | Contract/service tests |

## 5. Technical Debt

`AGENT-AUTHORING-A1-TD-001` — Open / Controlled

- 事实：门面构建一次 SemanticModel；现有 `CurrentFileReadonlyDiagnosticService` 为保持原诊断路径会再构建一次。
- 接受原因：A1 禁止重构诊断内部契约；复用完整诊断比复制算法更可靠。
- 当前影响：仅潜在分析成本，无结果分歧。
- 偿还触发：A2 Preview 性能证明确为主要延迟，或诊断服务未来获得稳定的 domain-fact 输入端口。
- 禁止的提前修复：不得为了消除重复构建而复制或分叉诊断算法。

## 6. Decisions

| Decision | 状态 | 结论 |
|---|---|---|
| Runtime Service 拥有 Provider Revision | Accepted | Snapshot 在成功发布时递增，分析请求显式捕获 |
| 使用中立 Request | Accepted | 不把 `CurrentSourceSnapshot/SourceEditorState` 固化为 Authoring 长期输入 |
| 适配现有诊断服务 | Accepted | UI ViewModel 只存在于门面内部适配层，不泄漏到契约 |
| 不在 A1 合并 Core/TextModel | Accepted | A0 八项可观察差异继续作为兼容性边界 |

## 7. Verification Matrix

| 检查 | 命令/方式 | 结果 |
|---|---|---|
| Card 1 | `dotnet test ... --filter FullyQualifiedName~FieldRegistryRuntimeServiceTests` | 18/18 passed |
| Contract models | `dotnet test ... --filter FullyQualifiedName~Ra2IniLanguageAnalysisContractTests` | 3/3 passed |
| A1 + A0 + diagnostics | 合并定向 filter | 45/45 passed |
| IDE-only build | `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | passed，0 warnings / 0 errors |
| Full non-UI tests | `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build` | 2355/2355 passed |
| Clean source package | `package-source-clean.ps1 -Profile IdeOnly` | passed，989 files |

未运行 UI Automation：A1 无 UI、Shell、XAML、Dock 或视觉行为变化。

## 8. Deferred Governance Queue — Flushed

### PublicApiLedger Pending Entries

已在本账本第 4 节收口；无外部 public API。

### TechnicalDebt Pending Entries

已在本账本第 5 节登记 `AGENT-AUTHORING-A1-TD-001`。

### DecisionLog Candidate Entries

已在本账本第 6 节记录 A1 accepted decisions；项目当前无独立 DecisionLog。

### CurrentStatus Pending Updates

更新 `Docs/Codex_CurrentPhase.md` 与
`Docs/RA2IniEditor_IDE_Full_Codex_Context.md`。

### Superseded Docs Pending Entries

`Docs/AGENT-AUTHORING-1-R1_A1A_ReadonlyLanguageAnalysisFacadeContract.md`
由连续最终契约覆盖；保留作为历史候选，不回写历史状态。

## 9. 边界确认

- Legacy 未恢复、未构建、未打包。
- Shell、Dock、XAML、主题和 UI 未修改。
- Core parser/validator、TextModel 解析规则、诊断算法、Completion、Save Preflight、
  Field Registry provider priority 与 BuiltIn 数据未修改。
- 未新增依赖，未修改项目文件。

## 10. 下一阶段

推荐：`AGENT-AUTHORING-1-R1 A2-A EditableSessionIdentityAndRevisionContract`。

下一阶段必须先契约化：

- 编辑会话身份与独立 Edit Revision；
- Field Registry Revision 与 Edit Revision 的双版本关系；
- Preview 的 current/candidate 分析所有权；
- stale preview 拒绝规则；
- 仍不开放 Agent Apply、自动保存或 writer 访问。
