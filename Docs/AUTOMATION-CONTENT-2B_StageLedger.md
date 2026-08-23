# CONTENT-2B Projectile / Warhead Complete Profiles Stage Ledger

更新时间：2026-08-23  
状态：Completed / Verified

## 1. Phase Result

新增三个 source-gated complete profile：

- `weapon-projectile-arcing-complete` v1：1 个新 Projectile，8 operations；
- `weapon-projectile-homing-complete` v1：1 个新 Projectile，5 operations；
- `weapon-warhead-yr-core-complete` v1：1 个新 Warhead，13 operations。

三者都要求唯一既有 Weapon，只生成 canonical Preview，不 Apply/Save。Catalog 从 3 增至 6；
Application exported allowlist 保持 59，Gateway catalog 7 / methods 9 保持不变。

## 2. Diff Intent

| 文件组 | 意图 | 边界 |
|---|---|---|
| `Ra2AutomationTemplateService.cs` | profile descriptor、definition 与范围预检 | 复用 compiler；无第二 plan/apply |
| `Ra2AiInteractionRoute.cs` | 精确区分 Arcing/Homing/Warhead 与扩展弹道拒绝 | Work only；Chat 不变 |
| `Ra2AiAuthoringToolCatalog.cs` | 每个 route 只暴露一个固定 schema | 无 raw INI/Apply/Save 参数 |
| `Ra2AiPromptBuilder.cs` | 投影模板 ID、参数与互斥边界 | Skill/Field Registry/Host 权威不变 |
| 两个 test projects | 目录、计划、范围、路由、schema、prompt、adapter | 预览原子性与零 Apply |
| Docs | 契约、证据、当前状态与决策 | 不改历史台账 |

## 3. Verification Matrix

| Gate | Result |
|---|---|
| Application focused template tests | Passed 16/16 |
| IDE focused template/route/tool tests | Passed 28/28 |
| `dotnet restore .\RA2IniEditor.IDE.sln` | Passed / up-to-date |
| Debug build `--no-restore` | Passed / 0 warnings / 0 errors（incremental）；focused IDE compile 另报告既有 test CS8602 warning |
| Application full tests | Passed 151/151 |
| IDE non-UI full tests | Passed 2601/2601 |
| IdeOnly clean package | Passed / 1177 files |
| Real DeepSeek / computer control / physical UI | NotRun / not required by contract |

## 4. Failure And Boundary Review

- `ROT<=0`、`InfDeath` 非 0..10、`CellSpread` 非 0..11、负 `PercentAtMax/ProneDamage`：
  `InvalidArguments`，无 plan。
- `Verses` 非 11 个百分比：`InvalidArguments`，无 plan。
- 当前文档存在 `[ArmorTypes]`：YR core Warhead 拒绝，避免冒充 Ares custom-armor 完整 profile。
- Arcing 与 Homing 同时请求，或明确 Phobos/Vertical/Airburst 等未支持弹道：provider 调用前拒绝。
- Section 冲突、owner 缺失/错 kind、schema/trust/stale/cancel/limit：继续由既有 canonical 链 fail closed。

## 5. Deferred Governance Queue

### DecisionLog

已登记：原版 Projectile 弹道族必须拆分；YR core Warhead 不覆盖 Ares `Versus.*`。

### PublicApiLedger

已刷新：CONTENT-2B public API 零变更；allowlist 保持 59。

### TechnicalDebt

无临时 helper、兼容 adapter、TODO 或第二写入路径。以下是明确后续能力，不登记为隐藏债务：

- Ares custom ArmorTypes / `Versus.*` profile；
- Phobos `Trajectory.*` profile；
- Airburst/Splits 等复合 Projectile profile。

## 6. Stop Confirmation

- Legacy 未恢复。
- Shell/XAML/Dock/AutomationId 未修改。
- BuiltIn v3.2、provider priority、parser、diagnostics、completion、Hover、Save、Undo/Redo 未修改。
- 未真实调用 DeepSeek，未使用电脑操控。
- 下一推荐阶段：`CONTENT-2C AI Programming Tuple Profiles`。
