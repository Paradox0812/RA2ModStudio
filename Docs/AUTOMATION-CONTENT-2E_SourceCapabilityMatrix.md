# CONTENT-2E Source / Capability Matrix

状态：**Completed / frozen for 2E-0**  
日期：2026-08-25  
适用范围：YR rules INI、Ares 3 SuperWeapon types、Phobos additive extensions

## 1. 来源优先级

| 层级 | 权威来源 | 本阶段用途 |
|---|---|---|
| Ares 3 | [Ares SuperWeapon types](https://ares-developers.github.io/Ares-docs/new/superweapons/types/index.html) | `Type`、类型专用字段和默认行为 |
| Ares 3 | [UnitDelivery](https://ares-developers.github.io/Ares-docs/new/superweapons/types/unitdelivery.html) | `Deliver.Types`、`Deliver.Owner`、默认 AI targeting |
| Ares 3 | [GenericWarhead](https://ares-developers.github.io/Ares-docs/new/superweapons/types/genericwarhead.html) | `SW.Damage`、`SW.Warhead`、默认 AI targeting 和 DamageSelf 风险 |
| Ares 3 | [Targeting](https://ares-developers.github.io/Ares-docs/new/superweapons/targeting.html) | targeting 组合与 AI 可用性约束 |
| Ares 3 | [Availability](https://ares-developers.github.io/Ares-docs/new/superweapons/availability.html) | `SW.AlwaysGranted` 和 provider 闭包 |
| YR / ModEnc | [Action](https://modenc.renegadeprojects.com/Action:)、[Actions](https://modenc.renegadeprojects.com/Actions) | `Action` 是 SuperWeapon UI/行为标志；错误或缺失值可能产生异常行为 |
| Phobos | [New or Enhanced Logics](https://phobos.readthedocs.io/en/latest/New-or-Enhanced-Logics.html) | additive extension 的知识边界；本阶段不声明 typed Phobos profile |

冲突策略：官方 Ares 文档优先于社区示例；Phobos 只扩展、不覆盖 Ares base type；来源没有冻结的值必须由模型显式给出或澄清，Host 不凭经验补默认值。

## 2. 冻结结论

### 2.1 共同字段与闭包

| 项目 | 冻结规则 |
|---|---|
| SuperWeapon identity | `superWeaponId` 必须是 canonical identifier，目标 Section 不得已存在 |
| 注册 | 在 `[SuperWeaponTypes]` 以既有数字键最大值 + 1 注册；同一值已注册时保持幂等；畸形、重复数字键时整包失败 |
| `Type` | typed profile 固定为 `UnitDelivery` 或 `GenericWarhead` |
| `Action` | **无跨类型、跨 UI 场景的官方统一字面值**；作为必填显式参数，Host 只做单行有界结构校验，不硬编码 `Custom` |
| 展示/充能 | `UIName`、`Name`、`RechargeTime`、`SidebarImage` 由请求显式给出；`SidebarImage` 只作为 INI 引用，不检查素材文件 |
| Provider | `providerMode=building` 时要求唯一既存 Building，并只允许 `SuperWeapon` / `SuperWeapon2`；不写 `SW.AlwaysGranted` |
| AlwaysGranted | `providerMode=always-granted` 时写 `SW.AlwaysGranted=yes`，不得伪造或修改 provider Building |
| Preview / Apply | 只形成 canonical Project Preview；用户显式 Apply；一次 compound Undo；不自动 Save |

`Action` 不能由字段库旧 Enum 或社区惯例替模型做内容决定。typed profile 的确定性来自“模型必须显式提交字面值 + Host 结构验证”，而不是程序静默发明默认值。

### 2.2 Ares UnitDelivery Complete

| 项目 | 值 / 约束 |
|---|---|
| `Type` | 固定 `UnitDelivery` |
| 效果字段 | `Deliver.Types=<1..16 个 canonical TechnoType IDs>` |
| 所有权 | `Deliver.Owner` 必须显式给出；Ares 文档枚举：`invoker`、`neutral`、`special`、`civilian`；文档默认是 `invoker`，但本 profile 不静默省略 |
| AI targeting | Ares 文档默认 `ParaDrop`；本 profile 要求显式 `SW.AITargeting`，以便预览准确表达模型选择 |
| 引用闭包 | 每个 delivery ID 必须在同一捕获 rules 文档中唯一解析为 Infantry、Vehicle、Aircraft 或 Building |
| 禁止行为 | 不创建 Techno、art Section、SHP/VXL/Cameo 或其它素材 |

### 2.3 Ares GenericWarhead Complete

| 项目 | 值 / 约束 |
|---|---|
| `Type` | 固定 `GenericWarhead` |
| 效果字段 | `SW.Damage=<integer>`、`SW.Warhead=<existing Warhead ID>` |
| AI targeting | Ares 文档默认 `Offensive`；本 profile 要求显式 `SW.AITargeting` |
| 引用闭包 | `warheadId` 必须在同一捕获 rules 文档中唯一解析为 Warhead |
| `DamageSelf` | 不作为默认输出；只有用户意图明确时才允许通用模型轨处理，typed v1 不修改 provider 的该行为 |
| 既存 Warhead | 不创建空 Warhead，不修改其 `CellSpread`；缺少 `CellSpread` 只形成可审阅风险信息 |

## 3. Capability 矩阵

| 请求 | Capability | 执行轨 | 完成度 |
|---|---|---|---|
| 明确 Ares UnitDelivery 且参数闭合 | `ares-unitdelivery-superweapon-complete` | deterministic typed profile | Complete object |
| 明确 Ares GenericWarhead 且引用既存 Warhead | `ares-genericwarhead-superweapon-complete` | deterministic typed profile | Complete object |
| 其它明确 SuperWeapon / support power 类型 | `superweapon-project-edit` | model-owned bounded project plan | Model-owned / needs review |
| engine、type、provider 或 effect 不明确 | advisory clarification | 无计划 | Needs clarification |
| 素材生成、游戏 Hook、运行时验证 | unsupported / deferred | 无计划 | Out of scope |

## 4. Source-based test vectors

### 正向

1. `Type=UnitDelivery`，`Deliver.Types=E1,FV`，`Deliver.Owner=invoker`，`SW.AITargeting=ParaDrop`；两个引用均唯一存在。
2. `Type=GenericWarhead`，`SW.Warhead=EMPWH`，`SW.Damage=1`，`SW.AITargeting=Offensive`；Warhead 唯一存在。
3. Building provider 的 `SuperWeapon` 与 `SuperWeapon2` 两槽分别覆盖。
4. `SW.AlwaysGranted=yes` 路径不产生 provider operation。
5. `Action` 使用模型显式提交的单行有界值，Host 不用 Field Registry Enum 否决。

### 负向

1. 空、重复、超过 16 项或含非法 identifier 的 `Deliver.Types`。
2. delivery ID 缺失、重复 Section 或解析为非 Techno family。
3. GenericWarhead 引用缺失、重复或非 Warhead Section。
4. Building provider 缺失、重复、非 Building 或槽位不是 `SuperWeapon` / `SuperWeapon2`。
5. provider 与 AlwaysGranted 同时选择或均未选择。
6. `Action` 缺失、包含换行/NUL 或超限。
7. `[SuperWeaponTypes]` 含重复数字 key、非数字 key 或编号溢出。

## 5. 2E-0 审查结果

**通过。** 两个 typed profile 的 `Type`、类型专用字段、引用关系和 provider/AlwaysGranted 闭包均有来源支撑。
`Action` 没有官方统一默认值，因此冻结为显式必填参数；这避免把社区惯例伪装成引擎事实。

实现前置阻塞：当前 Shell 只捕获 `rules + art` 配对的项目快照，无法满足已批准契约中的 rules-only / art-optional 要求。2E-1 必须先获得一次窄边界 Shell 捕获接线授权；不得以伪造 art manifest、磁盘旁路或 current-document 降级规避。
