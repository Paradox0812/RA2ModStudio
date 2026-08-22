# FR-DQ-4 Effective Placeholder Retirement and Runtime Visibility

## 1. 契约状态

```text
Package: FR-DQ-4
Risk: R4（BuiltIn Registry 数据迁移、兼容性与 Completion 可见性）
Contract state: Confirmed by user on 2026-07-20
Execution mode: Continuous StagePackage
Current entry: FR-DQ-4-0
Automatic source promotion: Forbidden
Automatic deletion by quality prefix alone: Forbidden
```

用户确认先清退当前字段库中的有效占位残留，再进入发布验收。本契约将“占位字段”拆分为字段身份、描述质量和运行时表面三个问题，禁止把所有 inferred 或同名跨上下文字段直接批量删除。

执行顺序：

```text
FR-DQ-4-0 -> 4A -> 4B -> 4C -> 4D -> 4E -> 4F -> 4G -> 4H
```

## 2. 当前代码事实

运行时 BuiltIn 文件：

```text
RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json
```

加载与消费事实：

1. `BuiltInFieldRegistryPackLoader` 只加载 v3.2 embedded pack。
2. `LocalFieldRegistryLoader` 把 `key/appliesTo/editorKind/sourceKind/description/displayName/quality/aliases/examples/schema` 转换为 `Ra2FieldDefinition`。
3. JSON 中的 `sources` 与 `builtinPolicy` 是源文件审计信息，不进入当前运行时 DTO。
4. `LocalRa2FieldDefinitionProvider` 的有效查找顺序是 exact section -> Unit -> Techno -> Global -> Unknown，按适用对象决定是否经过抽象层。
5. `Ra2CompletionProvider` 当前把 `GetFields(sectionKind)` 的结果直接作为字段名候选，不按可信度过滤。
6. `Ra2FieldTrustClassifier` 已能识别 VerifiedGuardrail / Inferred / AutoExtracted / Obsolete 等状态，但部分历史 quality 会落入 Unknown；空 quality 的 BuiltIn-like 行会被视为 Verified。

## 3. 冻结基线

### 3.1 运行时数据基线

| Metric | Count |
|---|---:|
| Runtime BuiltIn rows | 4878 |
| Verified（按当前分类器） | 1897 |
| Inferred | 1590 |
| Inferred uniform-template descriptions | 1524 |
| AutoExtracted | 810 |
| VerifiedGuardrail | 329 |
| Obsolete | 20 |
| Unknown trust classification | 193 |
| ManualCurated | 39 |
| Empty quality treated as Verified | 5 |
| Exact case-insensitive key + sorted appliesTo duplicates | 0 |
| Inferred/AutoExtracted/empty-quality rows whose key has a higher-quality row elsewhere | 330 narrow raw candidates |
| All audit candidates whose key has a higher-quality row elsewhere | 574 broad raw candidates |

`330` 与 `574` 都只是不同口径的原始审查候选，不是自动删除数量。相同 key 在不同上下文可能合法表示不同字段。

### 3.2 直接占位形态

在 810 条 AutoExtracted 行中，当前静态形态至少包括：

| Shape | Count |
|---|---:|
| Description length <= 12 | 394 |
| Exact `字段名 + 字段` shape | 125 |
| Generic type-only description | 267 |
| Raw legacy/tutorial fragments | 15 |

这些集合可能互相重叠，不能相加作为删除数量。

### 3.3 回滚锚点

完整 clean source：

```text
artifacts/RA2IniEditor.IDE.SourceClean.FR-DQ-4-0.Rollback.zip
Bytes: 4,130,764
Entries: 911
SHA-256: 8C84047EB182395B0FA3C937FAF00D881ABB8B17173B62AA9374FB97B2EEE811
Excluded-entry scan: 0
Legacy root project matches: 0
```

BuiltIn v3.2 单文件锚点：

```text
artifacts/builtin-yr-ares-phobos-fallback-v3.2.FR-DQ-4-0.Rollback.fields.json
Bytes: 6,412,057
SHA-256: 39256F8EEA11C45A05FB87863E532F28AF3F9C01A3ED0A153044D6276E10BDE5
Source hash match: true
```

## 4. 功能目标

1. 清除运行时 BuiltIn 中没有实际信息的统一模板和泛化占位说明。
2. 删除已经被新权威行明确替代的旧字段行。
3. 保留真正字段并重写准确说明，不把描述问题误判为字段不存在。
4. 保留 wrong-context / obsolete 等诊断事实，但不把它们作为 Completion 正常候选。
5. 消除 AutoExtracted、空 quality 和无法识别的历史 quality，而不是把它们静默当作 Verified。
6. 保持 Project > Global > BuiltIn、lookup、Hover 数据源、Diagnostics 分类、保存和编辑语义。
7. 所有迁移都有可复核 manifest、回滚锚点和测试证据。

## 5. 非目标

- 不追求让所有 inferred 字段数量为 0。
- 不以降低 Unknown Key 数量为理由恢复无证据占位行。
- 不修改 Field Registry import/apply/rollback/learning 语义。
- 不修改 provider priority 或 `LocalRa2FieldDefinitionProvider` 查找顺序。
- 不修改 Completion commit、值补全、排序或键入交互。
- 不修改 Hover、Quick Peek、Diagnostics 的数据源或提示结构。
- 不修改 parser、save preflight、backup、undo/redo、AI、XAML、项目文件或 legacy。
- 不在线自动抓取资料，不自动把 community/name inferred 晋级为 source-verified。

## 6. 行身份与处置模型

### 6.1 审计身份

审计清单中的稳定身份为：

```text
case-insensitive key
+ sorted exact appliesTo set
+ current quality
+ normalized description digest/summary
```

JSON 数组索引不得作为跨阶段身份。一个 JSON 行包含多个 appliesTo 时，处置必须针对整行明确说明，禁止隐式拆分。

### 6.2 处置类型

| Disposition | Meaning |
|---|---|
| `SupersededRemove` | 已有明确权威替代，旧行从 runtime 删除 |
| `Quarantine` | 无足够证据证明当前身份有效，迁出 runtime 并登记 backlog |
| `DiagnosticOnlyKeep` | 字段/风险事实有效，但仅供 Hover/Diagnostics，不进入 Completion |
| `PromoteAndRewrite` | 字段身份有效，重写实际说明并使用证据支持的 quality |
| `RetainReviewed` | 当前字段和说明可继续保留，登记复核理由 |

### 6.3 硬规则

- 不得仅凭 `inferred`、`auto-extracted` 或描述长度删除。
- `SupersededRemove` 必须记录替代行身份或明确错误上下文证据。
- `PromoteAndRewrite` 必须记录来源类别，不得只依据字段名晋级 source-verified。
- `Quarantine` 不进入运行时 JSON；完整旧状态由 4-0 锚点保存，manifest 保留身份和理由。
- `DiagnosticOnlyKeep` 必须继续可被字段查找和 Diagnostics 使用，但不进入字段名 Completion。

## 7. AA / AG 示例契约

| Row | Disposition |
|---|---|
| AA / Projectile | RetainReviewed（source verified canonical） |
| AG / Projectile | RetainReviewed（source verified canonical） |
| AA / Techno、Weapon | DiagnosticOnlyKeep |
| AG / Techno、Weapon | DiagnosticOnlyKeep |

期望行为：Projectile Completion 正常显示 AA/AG；单位与 Weapon Completion 不显示；用户手写在错误上下文时仍可得到 Wrong Context 诊断与解释。

## 8. 阶段计划与文件预算

### FR-DQ-4-0 BaselineAndRollbackAnchor

目标：建立完整源码与 BuiltIn 单文件回滚锚点，冻结当前指标。

允许：

- `artifacts/RA2IniEditor.IDE.SourceClean.FR-DQ-4-0.Rollback.zip`
- `artifacts/builtin-yr-ares-phobos-fallback-v3.2.FR-DQ-4-0.Rollback.fields.json`
- 本契约文档

禁止：所有 runtime、测试与 BuiltIn 数据修改。

### FR-DQ-4A EffectiveInventoryAndDispositionContract

目标：生成稳定身份、信任桶、模板形态、同 key 跨上下文和处置候选清单。

允许文件：

- `Docs/FieldRegistryPlaceholderRetirementAudit_2026-07-20.md`
- `Docs/FieldRegistryPlaceholderRetirementCandidates_2026-07-20.csv`
- 本契约文档

验收：候选清单不以 JSON index 为身份；每行具有 Disposition 或 `PendingManualReview`；统计可从当前 JSON 重算。

### FR-DQ-4B DiagnosticOnlyCompletionExclusion

目标：让 VerifiedGuardrail / Obsolete / NonExistent / PseudoField 退出字段名 Completion，同时保留 lookup、Hover 和 Diagnostics。

允许文件：

- `RA2IniEditor.IDE/Language/Ra2CompletionProvider.cs`
- `RA2IniEditor.Tests/IDE/Ra2CompletionProviderTests.cs`
- `RA2IniEditor.Tests/IDE/Ra2FieldTrustClassifierTests.cs`（仅必要时）
- 本契约文档（仅阶段 ledger）

禁止修改 Completion commit、值候选、provider、Diagnostics 或 UI。

### FR-DQ-4C SupersededPlaceholderRetirement

目标：只删除 manifest 中已经明确确认替代关系或错误上下文的旧行。

允许文件：

- BuiltIn v3.2 JSON
- BuiltIn loader tests
- candidate manifest
- 阶段验证文档

不得把 330 个 raw candidates 全部自动删除。

### FR-DQ-4D InferredTemplateDisposition

目标：逐批消除当前 1524 条统一模板描述。有效字段重写，证据不足字段 quarantine，诊断字段转 diagnostic-only。

默认每卡按 context family 分批，单卡最多修改 5 个文件；不得一次性删除全部 1524 行。

### FR-DQ-4E AutoExtractedPromotionOrRetirement

目标：分 context family 让 runtime `auto-extracted` 最终为 0。每行只能晋级、隔离、诊断保留或删除，禁止换一个泛化 quality 继续留存。

### FR-DQ-4F QualityNormalization

目标：runtime Unknown quality 和空 quality 均为 0。优先修正 JSON quality；除非现有合法质量体系无法表达，否则不扩展 runtime enum/classifier。

### FR-DQ-4G RuntimeSurfaceAndUnknownKeyRegression

目标：验证 Completion、Hover、Quick Peek、Diagnostics 和真实项目 Unknown Key 变化，不修改这些表面的既有架构。

### FR-DQ-4H FullVerificationDocumentationAndPackage

目标：完成全量 restore/build/test、IdeOnly clean package、文档和治理收口。

## 9. Public API 与数据模型

- 不新增或修改外部 public API。
- 不改变 `Ra2FieldDefinition` 序列化形状。
- disposition manifest 是审计/迁移工件，不进入产品运行时。
- 首选复用 `Ra2FieldTrustClassifier` 做 Completion 的窄过滤，不在 Core/Infrastructure 新增 UI 可见性字段。
- 若后续发现需要改变 public schema、registry owner 或 lookup 顺序，立即停线并重新确认契约。

## 10. AutomationIds

本包不修改 XAML/UI，不新增 AutomationId。所有现有 AutomationId 必须保持。

## 11. 验证矩阵

每个数据阶段至少验证：

1. JSON 可解析。
2. loader warnings 为预期或 0。
3. exact identity duplicates 为 0。
4. 当前阶段目标计数与 manifest 一致。
5. 关键 source-verified rows 和 AA/AG 行仍存在。
6. BuiltIn loader targeted tests 通过。

4B 额外验证：

- diagnostic-only trust levels 不进入 key Completion。
- Verified / Inferred / ManualCurated 正常候选行为保持。
- `TryGetField` 和 Diagnostics 仍能取得 guardrail。

包级验证：

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

## 12. 最终验收

```text
Uniform inferred template descriptions in runtime: 0
auto-extracted runtime rows: 0
empty quality: 0
unknown quality classification: 0
confirmed superseded placeholder rows: 0
diagnostic-only rows offered by key Completion: 0
exact key + appliesTo duplicates: 0
unreviewed deletion by quality/length only: 0
Project > Global > BuiltIn priority changes: 0
Completion commit / parser / save / Hover source changes: 0
```

允许运行时字段总数下降和真实项目 Unknown Key 增加，但必须在 4G 量化；不得以恢复占位行换取更低 Unknown Key 数字。

## 13. DeepSeek 分工

本包不委派 DeepSeek。字段身份、迁移、兼容性和 Completion 语义属于 Codex R4 审查责任。若未来仅需格式化已确认 manifest，可另行生成边界明确的任务包，但不得让 DeepSeek决定处置。

## 14. 停线条件

- 需要修改 provider priority、lookup hierarchy 或 `Ra2FieldDefinition` public shape；
- 需要删除无法提供替代身份/错误证据的同名跨上下文行；
- 需要让 guardrail 退出 lookup 或 Diagnostics；
- 需要修改 Completion commit、Hover、Quick Peek、Diagnostics、parser、save、XAML、项目文件或 legacy；
- 单个 Task Card 超过 5 文件且无法拆分；
- JSON、targeted test、build 或 full suite 失败且修复超出当前卡；
- manifest 与 runtime 计数无法重算一致；
- 实际风险高于本契约。

## 15. Stage Result Ledger

| Stage | Status | Evidence | Next entry |
|---|---|---|---|
| FR-DQ-4-0 | Completed | Full source and JSON rollback anchors created; hashes and exclusions verified | 4A |
| FR-DQ-4A | Completed | 2947-row manifest generated; SHA-256 `7F6885CC85D0B975BA156441D5B9C9833E0A71E6F93FE0EF5EAC8C3AAA15B7F2`; 349 DiagnosticOnlyKeep; 2598 PendingManualReview | 4B |
| FR-DQ-4B | Completed | Diagnostic-only trust levels excluded from key Completion; real BuiltIn AA/AG regression and focused tests passed 22/22 | 4C |
| FR-DQ-4C | Completed | 12 broad Techno inferred rows retired only where a ModEnc General-backed Global replacement exists; BuiltIn/Completion/trust tests passed 627/627 | 4D |
| FR-DQ-4D | Completed | Five bounded Techno key ranges retired the remaining 1147 templates; runtime uniform templates 0, duplicates 0; final scoped tests passed 703/703 | 4E |
| FR-DQ-4E | Completed | Five bounded source/key-range cards quarantined all 810 unverified auto-extracted rows; runtime auto-extracted 0; scoped tests passed 709/709 | 4F |
| FR-DQ-4F | Completed | Community-reviewed labels normalized; 66 inferred rows retained; five empty-quality mojibake rows repaired; scoped tests passed 712/712 | 4G |
| FR-DQ-4G | Completed | Real v3.2 Completion/Hover/Quick Peek/Diagnostics/highlighting regression passed 900/900; fixed sample delta +3 Unknown Key | 4H |
| FR-DQ-4H | Completed | restore passed; Debug build 0 warnings/0 errors; full non-UI tests 2274/2274; IdeOnly clean package and hygiene audit passed | Package closed |

## 16. Deferred Governance Queue

### PublicApiLedger Pending Entries

无。若实际实现需要 public API，触发停线。

### DecisionLog Candidate Entries

- Guardrail/obsolete 等诊断事实保留在 lookup，但退出字段名 Completion。
- 统一 inferred 模板表示描述待处理，不自动证明字段无效。
- AutoExtracted 最终必须晋级、隔离或删除，不能继续作为长期 runtime trust 状态。

### CurrentStatus Pending Updates

- Governance flush 1 completed on 2026-07-20: 4A-4C completed; 4D in progress at 30 promoted rows and 1482 remaining runtime templates.
- `Docs/Codex_CurrentPhase.md`, `Docs/RA2IniEditor_IDE_Full_Codex_Context.md`, and `Docs/ContextCapsule_FR_DQ_4.md` refreshed from the stage ledger and targeted verification evidence.
- Governance flush 2 completed after the 4D failure stop: rollback/replay restored encoding-safe JSON; manifest is `349 DiagnosticOnlyKeep / 50 PromoteAndRewrite / 13 SupersededRemove / 3 Quarantine / 2532 PendingManualReview`; 1458 uniform templates remain.
- Encoding decision: Field Registry bulk rewrites may not transport non-ASCII source through the Windows PowerShell default console code page; use an encoding-stable file or ASCII-only command input plus explicit UTF-8, identity assertions, JSON round-trip validation and atomic replacement.
- Governance flush 3 completed after four additional cards: all non-Techno uniform templates are gone; 1147 broad Techno templates remain, and the manifest is `349 DiagnosticOnlyKeep / 60 PromoteAndRewrite / 13 SupersededRemove / 304 Quarantine / 2221 PendingManualReview`.
- Governance flush 4 completed after five Techno key-range cards: 4D is complete with zero uniform templates; runtime rows 3414; manifest is `349 DiagnosticOnlyKeep / 60 PromoteAndRewrite / 13 SupersededRemove / 1451 Quarantine / 1074 PendingManualReview`; 4E starts from 810 auto-extracted rows.
- Governance flush 5 completed after five 4E source/key-range cards: runtime auto-extracted rows are zero; runtime rows 2604; manifest is `349 DiagnosticOnlyKeep / 60 PromoteAndRewrite / 13 SupersededRemove / 2261 Quarantine / 264 PendingManualReview`; scoped tests passed 709/709 and 4F starts from Unknown 193 / Inferred 66 / empty-quality 5.
- Governance flush 6 completed after three 4F quality-family cards: runtime empty quality and unrecognized trust are zero; manifest is `349 DiagnosticOnlyKeep / 65 PromoteAndRewrite / 259 RetainReviewed / 13 SupersededRemove / 2261 Quarantine / 0 PendingManualReview`; scoped tests passed 712/712 and 4G is active.
- Governance flush 7 completed at the 4G stage boundary: surface-focused tests passed 900/900; the fixed Vehicle sample reports +3 Unknown Key warnings versus the 4-0 baseline; no repository `.ini` corpus exists for real-project occurrence counting; 4H is active.
- Governance flush 8 completed at package closure: 4H full verification passed; product docs and long-term status docs were synchronized; no PublicApiLedger or TechnicalDebt entry remains pending; FR-DQ-4 is closed.
