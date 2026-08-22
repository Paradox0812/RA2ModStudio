# RA2IniEditor IDE Handoff v0.4.20

目标版本：v0.4.20 Harvest Parser Contract / Prototype  
基准版本：v0.4.19.1 Field Registry Manager Local Status Stabilization

## 1. 本次范围

v0.4.20 只实现字段采集解析原型：

```text
Raw harvested document text
  -> Harvest parser
  -> Field registry candidate records
  -> Parse warnings
```

Parser 只处理调用方传入的本地 raw text，不访问网络，不读取文件，不写入 active 字段库。

## 2. 新增契约

新增 internal Harvest 类型位于：

```text
RA2IniEditor.Infrastructure/FieldRegistry/Harvest/
```

类型：

- `FieldRegistryHarvestDocument`
- `FieldRegistryHarvestCandidate`
- `FieldRegistryHarvestConfidence`
- `FieldRegistryHarvestWarning`
- `FieldRegistryHarvestParseResult`
- `IFieldRegistryHarvestParser`
- `MarkdownFieldRegistryHarvestParser`

这些类型不进入 Core，不扩大 public API，不引用 WPF / AvalonEdit / legacy。

## 3. Parser 支持的最小规则

### INI-like key

支持：

```text
Owner=
Strength=600
Custom.Flag=yes
```

输出：

- `Key` 为等号左侧 trim 后内容；
- `Confidence = High`；
- `LineNumber` 为 one-based 行号；
- `RawLine` 保留原始行。

### Markdown table

支持：

```text
| Key | AppliesTo | Type | Description |
| --- | --- | --- | --- |
| Owner | Infantry | list | Owner countries |
```

表头大小写和空格不敏感，支持 `Applies To`、`Editor Kind`。

输出：

- `Key`
- `AppliesToRaw`
- `EditorKindRaw`
- `Description`
- `Confidence = High`

### Bullet candidate

支持：

```text
- Owner: owner countries
* Strength - hit points
```

输出：

- `Key`
- `Description`
- `Confidence = Medium`

## 4. 去重规则

同一个 document 内按 key 大小写不敏感去重：

1. 保留 confidence 更高的 candidate。
2. confidence 相同时保留先出现的 candidate。
3. 被丢弃的重复项记录 warning。
4. 不做跨 document 去重。

## 5. Warning 行为

当前 warning 覆盖：

- INI-like key 为空。
- INI-like key 非法。
- bullet key 为空。
- bullet key 非法。
- markdown table key 为空。
- markdown table key 非法。
- markdown table row 列数低于 header。
- duplicate key candidate。

warning 只描述 parse 问题，不阻止其他行继续解析。

## 6. 明确未做

v0.4.20 未实现：

- GitHub fetch。
- 网络访问。
- 自动下载 Ares / Phobos / YR 文档。
- harvest source crawler。
- normalize pipeline。
- validate pipeline。
- preview / apply / rollback。
- active 字段库写入。
- Field Registry Manager UI 按钮。
- 字段库编辑器。
- Completion。
- 保存。
- dirty。
- 编辑。
- TextChanged 编辑链路。
- `ProjectSaveService`。
- legacy Analysis。
- `ObjectAggregator`。
- `ProjectLoader`。
- 跨文件对象聚合。

## 7. 后续建议

下一阶段建议进入：

```text
v0.4.21 Normalize / Validate Pipeline
```

建议范围：

- `HarvestCandidate -> normalized candidate`
- raw `AppliesTo` 到 `Ra2SectionKind`
- raw `Type / EditorKind` 到 `FieldEditorKind`
- duplicate / conflict report

不要在 normalize 阶段直接写 active 字段库；active 写入、preview、apply、rollback 应继续后置。
