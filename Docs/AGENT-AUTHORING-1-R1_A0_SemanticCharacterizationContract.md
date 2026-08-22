# AGENT-AUTHORING-1-R1 A0 语义特征锁定契约

状态：已实现并通过自动化验证  
日期：2026-07-23  
风险等级：R1  
治理模式：A0 单任务卡即时收口  

## 1. 目标

在建立只读语言门面之前，用自动化测试锁定 Core 解析器与 IDE Span 感知解析器当前可观察到的兼容性事实。

A0 只描述现状，不决定哪一条解析路径更正确，也不尝试将两条路径统一。

## 2. 与原 AGENT-AUTHORING-1 契约的关系

本文件是 `AGENT-AUTHORING-1_HighLevelIniAuthoringArchitectureContract.md` 的 R1
前置补充。发生冲突时，后续 A1-A5 设计采用以下修正规则：

- A1 必须组合 Core 与 IDE TextModel 的双视图结果，不能宣称已有单一统一编译器。
- 编辑并发令牌使用独立 `SessionId`、编辑 `Revision` 和内容摘要，不能复用加载快照版本。
- Field Registry 必须在 Reload 成功后发布新的内部 Revision。
- Preview 由 IDE 工作区持有，并通过单次使用的 `PreviewId` 应用；不得接受调用者提交的任意 Preview 对象。
- A3 Apply 之前必须先验证 AvalonEdit 事务行为，并迁移当前单槽程序化 Undo。
- A4 第一版只在完整、正常结束的响应上解析结构化 JSON；不得逐 token 修改源码。
- A5 外部协议继续保持 R4 独立门禁。

## 3. 允许修改

- 新增 Core/TextModel 跨路径特征测试。
- 更新本阶段契约、CurrentPhase 和完整上下文。

## 4. 禁止修改

- `RA2IniEditor.Core` 和 `RA2IniEditor.IDE` 生产代码。
- Parser、Validator、Serializer、SemanticModel 和 Diagnostics 行为。
- Field Registry 数据、Provider 优先级和 Reload 行为。
- Completion、编辑会话、Undo/Redo 和保存链路。
- AI 请求、流式协议和提示词。
- Shell、XAML、AvalonDock、项目文件和依赖。
- Legacy 项目或旧版编辑器入口。

## 5. 锁定的代码事实

| 场景 | Core 当前结果 | IDE TextModel 当前结果 |
|---|---|---|
| `// full-line comment` | `IniCommentLine` | `Raw` |
| `[E1] trailing text` | `IniUnknownLine` | `SectionHeader`，名称为 `E1` |
| `Primary=120mm; comment` | 值为 `120mm`，保留注释后缀 | 值为 `120mm`，保留注释 Span |
| `Primary=120mm # comment` | `#` 仍属于值 | `# comment` 为内联注释 |
| 文档以换行结尾 | 产生末尾 `IniBlankLine` | 不产生合成空行，换行保存在前一行 |
| CRLF/LF 混合 | `NewLine` 使用首先检测到的 CRLF | `NewLineKind.Mixed` |
| RA2IniEditor covered 注释 | `IniCoveredKeyValueLine` | 普通 `Comment` |
| `=missingKey` | 空 Key 的 `IniKeyValueLine` | `Raw` |

这些差异必须在 A1 的分析结果中显式保留或报告，不得通过静默规范化隐藏。

## 6. 实现

新增：

```text
RA2IniEditor.Tests/IDE/Ra2IniParserConsistencyCharacterizationTests.cs
```

测试直接调用现有 `IniParser.Parse` 与 `Ra2IniTextDocumentParser.Parse`，没有引入
测试专用生产入口、镜像解析器或额外规范层。

## 7. 验收标准

- 八个特征场景均通过。
- 现有 Core round-trip、IDE TextModel 和换行测试继续通过。
- IDE-only solution 构建通过。
- 无 public API、生产代码和项目依赖变化。
- 不因为测试结果修改任一解析器。

## 8. 验证结果

```text
新增特征测试：8/8 passed
Core/TextModel 相关测试：26/26 passed
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore：
passed，0 warnings，0 errors
```

首次定向测试在编译测试项目时显示既有
`BuiltInFieldRegistryPackLoaderTests.cs:1961` CS8602 警告；随后 solution build 为
0 warnings / 0 errors。本阶段没有触碰该文件。

## 9. 自审

- 范围：通过。仅新增一个测试文件和阶段文档。
- 架构：通过。没有创建第三条解析路径。
- 兼容性：通过。只锁定当前事实，没有改变运行时输出。
- 可演进性：通过。A1 可直接以这些场景作为双视图门面的最低一致性矩阵。
- 停点：已到达。A0 不进入 A1 生产实现。

## 10. 下一安全入口

```text
AGENT-AUTHORING-1-R1 A1-A
ReadonlyLanguageAnalysisFacadeContract
```

A1-A 只允许建立 internal、只读、无 WPF 类型泄漏的分析事实模型和适配门面。
Field Registry Revision、编辑 Session Revision、Planner 和 Apply 均不属于 A1-A。
