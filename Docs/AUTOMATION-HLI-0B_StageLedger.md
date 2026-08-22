# AUTOMATION-HLI-0B Stage Ledger

日期：2026-08-22  
状态：Completed / Confirmed contract stage  
契约：`Docs/AUTOMATION-HLI-0B_MinimumCapabilityContract.md`

## Stage Result Ledger

| Stage | Goal | Verification | State | Next entry |
|---|---|---|---|---|
| HLI-0B-0 | 审核 Headless 必要性和总体方向 | HLI-0A matrix + current code facts | Completed | HLI-0B contract |
| HLI-0B-1 | 冻结能力/所有权/API候选 | Contract self-review | Completed | User review |
| HLI-0B-2 | 用户确认并回归返工风险 | Explicit user confirmation + HLI-1A0 source audit | Completed | HLI-1A0 |

## 结论

HLI-0B 在加入 HLI-1A0 八项修正后足够可靠，可以作为后续纵向迁移的架构基线。
它不会承诺“零变化”，但通过以下门禁把潜在返工限制为受控迁移：

- 首切片只搬 22 文件 Query foundation，不搬完整 TextModel/Diagnostics/Preview；
- IDE 和 Application 共享同一 internal semantic foundation；
- 外部 public API 只保留高层 Experimental service/DTO；
- A3 Apply/Undo、Save、Registry runtime 和 WPF 生命周期不下移；
- 新 Headless 测试项目与现有 IDE 集成测试分别证明两个边界；
- 每个后续切片独立契约、等价测试和停止门禁。

## Verification

- HLI-0B 自身没有源码/API/项目变更。
- HLI-1A0 特征测试作为执行证据记录在对应 Stage Ledger。
- Build/test/package 不归入 HLI-0B；HLI-1A0 已运行自己的定向验证。

## Boundary confirmation

- Legacy 未恢复。
- Shell/XAML/Dock 未修改。
- Parser、Diagnostics、Field Registry、Completion、AI、Search、Apply/Save 语义未修改。
- 没有创建 Application 项目或 public API。

