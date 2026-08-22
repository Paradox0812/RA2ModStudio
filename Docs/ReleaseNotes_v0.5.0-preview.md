# RA2IniEditor.IDE v0.5.0-preview Release Notes

## 版本定位

`v0.5.0-preview` 是 RA2IniEditor.IDE 的字段库、Hover、诊断与 IDE 外壳技术预览版。它已经可以用于内部测试和小范围试用，但仍不应被视为正式稳定版。

## 本版重点

- IDE-only 源码包继续保持 clean source 形态。
- BuiltIn 字段库完成 P0 / P1 / P2 高风险示例值注释清理。
- 修复 `BaseDefenseDelay` 等字段的错误示例值说明。
- 将低可信字段转换为 verified / inferred / guardrail / obsolete / non-existent 等分层表达。
- Hover 采用轻量风险脚注，不展示冗长来源审计信息。
- Field Quick Peek 开始承载更详细的可信度说明。
- Unknown Key 诊断开始区分上下文错误、废弃字段、未实现字段和伪字段。
- AI Assistant 的 DeepSeek 响应支持 SSE 增量显示、取消、超时和不完整终态隔离；流式内容不会自动写入编辑器。
- AI Assistant 生产路径已移除 Mock，仅提供 DeepSeek V4 Flash / V4 Pro，默认 V4 Flash；配置快照、端点信任、出站脱敏、输入/输出预算和安全诊断已收口。
- 补齐应用图标、任务栏图标、用户说明文档、已知问题和烟测清单。

## 已完成能力

| 模块 | 状态 |
|---|---|
| Source Editor | 可打开、查看和编辑 INI 文本 |
| Navigator | 支持当前文件 section 跳转 |
| Hover | 提供轻量字段说明和必要风险提示 |
| Field Quick Peek | 提供字段详情和可信度摘要 |
| Completion / Add Property | 提供字段候选和详情区域 |
| Issues | 提供 Unknown / WrongContext / Obsolete / NonExistent / PseudoField 风险提示 |
| Field Registry | 支持 BuiltIn / Global / Project 组合字段库 |
| Search | 支持项目级只读查找、当前文件内存查找、结果导航和当前文件预览式 Replace All |
| AI Assistant | 支持 DeepSeek V4 Flash / V4 Pro 流式草稿，默认 Flash；仅 Completed 回答进入最近对话上下文 |
| 保存与备份 | 支持保存、备份、撤销、重做、Revert |

## 测试状态

当前基线：`FR-DQ-3H Fix2` + `AI-REL-3 ProviderTrustPrivacyAndResourceHardening`。

已由用户本地验证：

```text
dotnet build 通过
dotnet test 2171 / 2171 通过
AI-REL-3 各阶段定向测试通过
AI 面板 Flash / Pro 选择与安全提示 UI smoke 通过
真实 DeepSeek 多 delta 与部分响应取消 smoke 通过
真实 DeepSeek V4 Flash / V4 Pro 最小请求各一次通过
IdeOnly clean source package 通过
```

UI 自动化 smoke 测试默认跳过，需要在交互式桌面会话中设置环境变量后单独运行。

## 已知限制

- 本版本仍为 preview，不保证所有字段说明完全权威。
- 部分字段是社区资料辅助推断或字段名推断。
- 不提供在线字段查询。
- 不提供 MIX / VXL / SHP / 地图编辑能力。
- 不做强阻断式保存 gate。
- 大型真实 MOD 的性能和误报率仍需要更多样本验证。
- Search 为按需扫描；大于 8 MB 的延迟预览文件会跳过并报告。项目级/多文件替换尚不提供。

## 建议反馈方向

- 哪些 Hover 字段说明仍然不准确。
- 哪些 Unknown Key 属于误报。
- 哪些 WrongContext 诊断过度或不足。
- 哪些 Ares / Phobos 字段仍缺失。
- 大文件打开、Navigator 跳转、Issues 刷新是否卡顿。
- Field Registry Manager 是否足够好用。
