# Field Registry Description Verification - AI Schema Recheck

Phase: FR-DQ-3D-TeamTypes-AITriggerTypes-SchemaRecheck-ManualApply

## 1. Scope

本阶段针对 `Docs/FieldRegistryUnresolvedRows_2026-06-03.md` 中误挂在 `AI` schema 下的 AI 编程相关字段进行再次核对。重点来源族：

- ModEnc `[TeamTypes]` / TeamTypes Flags；
- ModEnc `[TaskForces]`；
- ModEnc `[ScriptTypes]` / ScriptActions；
- ModEnc `[AITriggerTypes]`；
- 与 AI 脚本动作相关的 Infantry / Building / General 字段。

本阶段不修改 Field Registry provider priority、Hover、Quick Peek、AI Evidence、parser、diagnostics、completion、save preflight、XAML/UI 或 legacy。

## 2. Source Summary

- `TeamTypes` 页面确认 TeamType 由 TaskForce 与 Script 组成，并包含 `Name`、`Recruiter`、`Autocreate` 等行为字段。
- `TaskForces` 页面确认 TaskForce 节由 0..5 的 unit entries 与 `Group` 等字段组成。
- `ScriptTypes` 页面确认 ScriptType 节是 0..49 的 action,argument 列表。
- `AITriggerTypes` 页面确认 AITrigger 是复杂的 `ID=Name,Team1,OwnerHouse,...` comma-list 格式，不应被拆成 `[AI]` 普通字段。
- `Agent`、`Engineer`、`Infiltrate`、`Spyable`、`Grinding`、`AICaptureLowMoney` 页面进一步确认部分旧 `AI` 行实际属于 Infantry / Building / Global 语义。

## 3. Rows Promoted to Precise Schema

### TeamType canonical rows

新增 source-backed `TeamType` 行 32 条，代表字段：

```text
Name
TaskForce
Script
House
Group
Autocreate
Recruiter
AreTeamMembersRecruitable
Priority
Max
Full
Annoyance
GuardSlower
Whiner
Loadable
OnTransOnly
TransportsReturnOnUnload
TransportWaypoint
UseTransportOrigin
Prebuild
Reinforce
Droppod
Aggressive
Suicide
OnlyTargetHouseEnemy
IsBaseDefense
AvoidThreats
IonImmune
MindControlDecision
Tag
Waypoint
VeteranLevel
```

### TaskForce canonical rows

```text
Name / TaskForce
Group / TaskForce
```

### Infantry / Building / Global recheck rows

```text
Agent / Infantry
Infiltrate / Infantry
Engineer / Infantry
Deployer / Infantry
Grinding / Building
Spyable / Building
AICaptureLowMoneyMark / Global
```

## 4. Legacy Guardrail Rows

以下旧行被改为 source-backed legacy guardrail，避免继续被 unresolved 清单误认为“完全未查”：

```text
TeamTypes flags / AI
TeamTypes flags / Techno
TaskForce / AI
Script / AI
Group / AI
x / Script
D1 / D2 / X1 / X2 / XX.000000 / YY.000000 / ZZ.000000 / INDEX / AI
Agent / AI, Techno
Infiltrate / AI, Techno
Engineer / AI, Techno
Deployer / AI, Techno
Spyable / AI, Techno
Grinding / AI
AICaptureLowMoneyMark / AI, Techno
```

这些 guardrail 不声称原上下文有效，只说明可靠来源指向其他 schema。

## 5. Still Unresolved

仍保留为 `NeedsMoreEvidence` 的 AI rows：

```text
AICaptureWounded / AI
SuspendPriority / AI
tempValue / AI
Threat / AI
```

原因：本轮未找到足够直接、可复用到 BuiltIn 描述的可靠字段页，暂不编造说明。

## 6. Result Summary

```text
BuiltIn v3.2 field count: 5109
Source-verified rows: 1870
NeedsMoreEvidence / unresolved guardrail rows: 0
Direct Hover-risk rows: 0
New canonical rows added in this phase: 39
AI unresolved rows remaining: 4
```

## 7. Next Step

建议下一阶段继续按 unresolved 来源族处理：

```text
FR-DQ-3E-TechnoResidualSourceFamilyRecheck
```

优先从 `Techno` unresolved 中剥离明显属于 Infantry / Building / Weapon / Warhead / Global / Ares / Phobos 的错挂行。
