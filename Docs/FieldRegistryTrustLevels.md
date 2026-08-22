# 字段可信度说明

RA2IniEditor.IDE 的字段库用于 Hover、Completion、Quick Peek 和 Diagnostics。由于 RA2/YR、Ares、Phobos 以及社区资料来源复杂，字段库不会把所有字段都视为同等权威。

## 可信度层级

| 层级 | 含义 |
|---|---|
| `source-verified` | 已由较可靠来源核验的字段 |
| `source-verified-guardrail` | 已确认字段存在，但当前上下文不应作为普通字段使用 |
| `community-source-assisted-inferred` | 有社区资料或扩展文档线索，但未逐条核验 |
| `community-reference-inferred` | 基于社区资料、旧教程、INI Bible 等参考推断 |
| `name-inferred` | 主要依据字段名和所属上下文推断 |
| `manual-curated` | 人工整理字段，可能仍需后续来源补强 |
| `auto-extracted` | 自动抽取字段，可信度最低，应逐步清理或核验 |
| `obsolete` | 废弃字段或旧引擎残留字段 |
| `non-existent` | 原始注释中出现但程序未实现或不建议使用的字段 |
| `pseudo-field` | 注册列表、索引项或伪字段，不应作为普通 key 使用 |

## Hover 展示原则

Hover 不展示完整审计信息，只展示必要字段说明：

- 已核验字段：不额外显示“官方核验”徽标。
- 推断字段：显示一行“推断说明，仅供参考”。
- guardrail 字段：提示疑似上下文错误。
- obsolete / non-existent 字段：提示废弃或未实现。

## 详细信息在哪里看

完整可信度和来源摘要应通过以下位置查看：

- Field Quick Peek。
- Field Registry Manager。
- 字段库文档或后续导出的审计表。

## 使用建议

字段库应作为辅助工具，而不是强制规则。对于大型 MOD、私有扩展或新版本 Phobos/Ares 字段，建议通过 Project 字段包补充或覆盖 BuiltIn。
