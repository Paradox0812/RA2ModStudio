# Field Registry Description Verification - Inferred Backlog Recovery - 2026-06-03

Phase: `FR-DQ-3F-InferredBacklogRecovery-ManualPatch`

## 1. Goal

在用户允许“非完全权威来源”和“一定字段名推论”的前提下，重新处理 3E 迁出的低可信 backlog 字段。
本阶段不把这些字段伪装为 `source-verified`，而是恢复为明确低权重的 inferred fallback。

## 2. Scope

Scanned / modified file:

```text
RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json
```

Recovered source:

```text
FR-DQ-3E low-confidence runtime backlog rows
```

## 3. Result

```text
Runtime fields before 3F: 3519
Recovered inferred rows: 1590
Runtime fields after 3F: 5109
Runtime needs-more-evidence rows: 0
Runtime inferred rows: 1591
Runtime source-verified rows: 2051
Unsupported schema.type=Text rows: 0
Direct placeholder / generic Hover risk rows: 0
```

## 4. Recovered Rows by appliesTo

| appliesTo | count |
|---|---:|
| `Techno` | 1215 |
| `ArtObject` | 101 |
| `Building` | 70 |
| `Warhead` | 53 |
| `Global` | 38 |
| `Vehicle` | 28 |
| `Weapon` | 19 |
| `Country` | 11 |
| `Infantry` | 9 |
| `Sound` | 8 |
| `Terrain` | 8 |
| `Banner` | 7 |
| `Eva` | 6 |
| `AI` | 4 |
| `Unit` | 4 |
| `Side` | 3 |
| `Aircraft` | 2 |
| `LaserTrail` | 1 |
| `ParticleSystem` | 1 |
| `Tiberium` | 1 |
| `VoxelAnim` | 1 |

## 5. Recovered Rows by sourceKind

| sourceKind | count |
|---|---:|
| `Yuri` | 1139 |
| `Phobos` | 400 |
| `Ares` | 51 |

## 6. Recovered Rows by editorKind

| editorKind | count |
|---|---:|
| `Text` | 1097 |
| `Reference` | 172 |
| `Integer` | 170 |
| `Boolean` | 103 |
| `MultiSelect` | 27 |
| `Enum` | 15 |
| `Float` | 6 |

## 7. Description Policy

每条恢复字段的描述均使用固定前缀 `推断型字段：`，并明确说明：

- 暂未完成官方逐条核验；
- 依据字段名、appliesTo 上下文和既有社区资料线索推断；
- 仅作为 BuiltIn 宽松兜底；
- 不能替代 ModEnc、Ares 或 Phobos 官方字段页。

## 8. Quality Policy

本阶段没有新增 `source-verified` 断言。恢复行使用：

- `community-source-assisted-inferred-*`：存在外部 URL 或较明确来源线索，但未逐条核验。
- `community-reference-inferred-*`：存在本地资料包、教程、旧 INI Bible 或人工审查痕迹。
- `name-inferred-*`：主要依赖字段名和上下文推断。

## 9. Static Validation

- JSON parse: passed.
- `schema.type=Text`: 0.
- `needs-more-evidence*`: 0.
- Direct Hover placeholder fragments: 0.
- No provider priority, Hover, Quick Peek, Completion, Diagnostics, Save, AI provider, XAML, project files, or legacy editor behavior changed.

## 10. Risk

这版会显著降低 Unknown Key 误报，但也会把部分未逐条核验字段重新暴露给 Hover / Quick Peek / AI Evidence。
风险通过 `推断型字段` 描述和 `inferred` quality 标签显式标识，不再把它们混同于官方核验字段。

## 11. Extra Cleanup

- Cleaned legacy `SellBack / Global` raw tutorial snippet that still contained an English `placeholder` fragment; reclassified it as `community-reference-inferred-yuri-20260603` with an inferred integer description.
