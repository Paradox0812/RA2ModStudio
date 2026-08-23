# AUTOMATION-CONTENT-1 Stage Ledger

更新时间：2026-08-23  
状态：Completed / Verified（视觉人工验收待用户后续执行）

## 阶段状态

| 阶段 | 状态 | 结果 |
|---|---|---|
| CONTENT-1A Field Schema Query | Completed / Verified | effective provider + trust typed query；allowlist 40，catalog 5 |
| CONTENT-1B Reference Resolve | Completed / Verified | current-document typed resolution；不猜 target kind；allowlist 45，catalog 6 |
| CONTENT-1C Section Creation Preview | Completed / Verified | additive Section 创建进入唯一 EditPlan/Preview；allowlist 47 |
| CONTENT-1D Internal Template Domain | Completed / Verified | internal definition/parameter/compiler；类型、默认值、范围、schema/trust、冲突、取消均 fail closed；public diff 0 |
| CONTENT-1E Template Gateway | Completed / Verified | source-backed `weapon-projectile-warhead-skeleton` v1；allowlist 58，catalog 7，Gateway methods 9 |
| CONTENT-1F IDE Agent Integration | Completed / Verified | required template tool -> Gateway -> Preview -> proposal -> existing Coordinator atomic Apply；public diff 0 |
| CONTENT-UI-1 Main Workspace Diff | Completed / Verified | 临时只读主工作区 Diff；关闭可恢复，Dismiss/Invalidate 终止；无第二写入权威 |

## 最终验证

```text
dotnet restore .\RA2IniEditor.IDE.sln: Passed
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore: Passed, 0 warnings, 0 errors
dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build: Passed 146/146
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build: Passed 2568/2568
CONTENT-UI-1 focused: Passed 20/20
IdeOnly clean package: Passed, 1147 files
Computer control / real DeepSeek / physical DPI visual smoke: NotRun by request/contract
```

## Post-completion narrow fix：AI-AUTHORING-NONSTRICT-1

- 实机反馈：普通字段编辑连续返回“结构化修改参数格式无效”；提示词要求字符串 value 仍失败。
- 修复：字段工具适配器有限容忍尾逗号、可唯一推断 outcome、缺失展示摘要、单 operation 对象和
  JSON number value；语义/权限仍严格，复合 value、未知/重复属性、raw patch、Apply/Save 继续拒绝。
- Prompt 明确要求 summary、operations array 和字符串 value；错误消息按结构分类且不回显参数。
- 验证：Adapter/Prompt 首轮刻画如预期失败；修复后 24/24；SSE/Prompt/Tool/Coordinator/CONTENT
  集成 88/88；Debug build 0 warning / 0 error；完整 non-UI 2576/2576。
- Package：本窄边界修复未重新打包；上方 1147-file 包是修复前 CONTENT-1 完成包。

## Public / authority 审查

- Application exported Experimental allowlist 精确 58；所有新增 public 类型均由 Gateway/IDE consumer 使用。
- 1D、1F、UI-1 public diff 0；Template definition/compiler、proposal、Diff、Workspace、Apply 均保持 internal。
- Field Registry 仍是 schema/trust 事实源，不是对象模板库；provider priority/data/load semantics 零变化。
- Template Expansion 只产生 canonical `Ra2AutomationEditPlan`；Preview、currency、Apply、Undo、Problems refresh 继续沿用唯一链路。
- Diff 只读取 immutable Preview；Apply/Dismiss 只调用现有 Coordinator；关闭标签不丢失提案。
- 未公开 Apply/Save、wire/session/persistence、multi-file、Job/Event/Artifact 或素材 provider。

## 剩余风险处理结果

| 风险 | 处理 |
|---|---|
| 未经来源核验的对象默认值 | Closed：首个模板只含两个 source-backed 引用关系，不生成玩法默认值 |
| 大文档 Diff 卡住 UI | Closed by guard：后台、可取消；8 MiB / 200k lines / 20k rows / 2k hunks fail closed |
| Diff 与 Candidate 不一致 | Closed by guard：按有序 change 线性重放并逐字验证 candidate |
| 关闭 Diff 意外 Dismiss | Closed：关闭只释放 View；卡片可重开；Dismiss 才消费 proposal |
| 第二 Apply authority | Closed：UI 没有 editor/TransactionPort 写入，只路由既有 Coordinator |
| Blocked/Stale 仍可应用 | Closed：状态投影禁用按钮并显示语义状态；最终 Apply 仍由 coordinator/workspace 复核 |
| 不同分辨率 | Automated contract covered：固定行号/marker 栏与 640 DIP 紧凑返回按钮；物理 DPI 仍人工验收 |
| 空 Projectile/Warhead 可被误解为完整武器 | Controlled/documented：明确称“关系骨架”，不宣称完整可用对象 |
| DeepSeek 非严格工具参数漂移 | Closed for known unambiguous shapes：有限格式容忍后仍进入同一 Preview；含糊形态 fail closed |

## 未实现且明确后置

- 独立 Agent Host、wire/session/permission/audit。
- 模板持久化、用户模板、任意模板库、注册列表与数字索引维护。
- 项目级引用语义、多文件原子事务、自动 Apply/Save。
- Icon/Cameo、VOX/VXL、SHP、Artifact/Job/Event Runtime 与自动绑定。

## 下一安全入口

`HOST-1 Independent Agent Host Boundary` 代码事实审计与最终契约。先冻结可序列化 snapshot、
session/permission/audit 和 IDE-mediated apply protocol；不得直接把当前进程内 provider、Workspace
或 Save authority 暴露给外部 Agent。
