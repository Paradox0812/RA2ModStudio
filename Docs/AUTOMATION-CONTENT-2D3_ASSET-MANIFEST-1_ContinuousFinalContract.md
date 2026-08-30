# CONTENT-2D-3 + ASSET-MANIFEST-1 — Continuous Final Contract

日期：2026-08-24  
状态：Accepted by user / self-reviewed / implementation authorized  
前置：`CONTENT-2D-2` completed  
风险：R4

## 1. 目标

在现有项目快照、项目计划和项目 Preview 之上增加首个生产级 rules/art 消费者，并输出独立、
不可变、无写权限的资产需求清单。调用方可以用同一个项目模板结果完成：

1. 为现有 TechnoType 在 rules 文档写入 `Image=<artSectionId>`；
2. 在配对 art 文档创建或更新 `[artSectionId]`，写入 `Image=<bodyAssetId>`；
3. 获得后续 SHP/图标生成器可消费的资产 Manifest；
4. 把项目计划继续交给现有 `PreviewProject`，不新增 Apply/Save 权威。

## 2. 架构不变量

- `Ra2ContentTemplateCompiler` 仍是每个文档唯一模板编译器；项目编译器只负责编排和闭合验证。
- `Ra2AutomationProjectEditPlan` 仍是 INI 修改唯一真相；Manifest 不参与 Apply，也不写磁盘。
- 只接受唯一的 `rulesmd.ini + artmd.ini` 或 `rules.ini + art.ini` 配对，不按模糊文件名猜测。
- 任一叶文档编译失败，项目结果的 Plan、Manifest、Warnings 全部为空。
- Manifest 中 `Proposed` 绑定必须与生成的叶操作精确对应；`PendingSchema` 绑定不得生成操作。
- 不绕过 Field Registry。缺少 authorable schema 的 `Cameo/Voxel/Remapable` 只记录为 PendingSchema。
- 不修改 parser、classifier、Field Registry、diagnostics、completion、save 或 Shell/UI。

## 3. Public Experimental 契约

新增：

- `Ra2AutomationAssetKind`：`ShpAnimation`、`Cameo`、`VxlModel`、`HvaAnimation`；
- `Ra2AutomationAssetBindingState`：`Proposed`、`PendingSchema`；
- `Ra2AutomationAssetBindingFact`：DocumentId/FilePath/Section/Key/Value/State；
- `Ra2AutomationAssetRequirement`：ID、文件名、类型、生成 brief、可选尺寸/调色板、绑定列表；
- `Ra2AutomationAssetManifest`：ProjectSessionId、TemplateId/Version、稳定排序的 Requirements；
- `Ra2AutomationProjectTemplateExpansionResult`：项目事实、failure、Plan、Manifest、Warnings。

现有 `Ra2AutomationTemplateDescriptor` additive 增加 `IsProjectTemplate` 与
`ProducesAssetManifest`；现有六个模板均为 false。`Ra2AutomationTemplateOutputKind` 追加
`ProjectBinding`。`IRa2AutomationTemplateService` 与唯一 Gateway 追加：

```text
ExpandProjectTemplate(projectSnapshot, request, cancellationToken)
```

Gateway capability 追加 `ini.project.content.template.expand`，risk Edit，stability Experimental。

## 4. 首个项目模板

ID：`techno-rules-art-asset-binding`，version 1，参数：

```text
ownerSectionId : Identifier, required
artSectionId   : Identifier, required
bodyAssetId    : Identifier, required
cameoAssetId   : Identifier, required
assetBrief     : String, required
```

输出：

- rules 叶计划：`RequireExisting ownerSectionId`，写 `Image=artSectionId`；
- art 叶计划：`CreateOrUpdate artSectionId`，写 `Image=bodyAssetId`；
- body SHP requirement：Proposed 绑定到 art `Image`；
- cameo SHP requirement：60x48、`cameo.pal`，`Cameo=cameoAssetId` 为 PendingSchema，不生成操作。

模板不创建完整 Techno，不生成素材，不验证文件存在，不自动 Apply/Save。

## 5. 失败与限制

- 缺失/重复/版本错误参数沿用现有 template failure；
- 缺少、重复或错误的 rules/art 配对分别返回 ProjectDocumentNotFound/
  ProjectDocumentAmbiguous/InvalidArguments；
- owner 不存在或 kind 不兼容沿用 RequiredSection 失败；
- 任一字段 schema 缺失/阻断或文档超限整体失败；
- Project Plan 继续受 8 文档、256 aggregate work 上限；本 profile 固定 2 文档。

## 6. 分阶段门禁

| Stage | 内容 | 必选门禁 |
|---|---|---|
| 2D-3A | 契约、public result、项目编译器、首个 profile、Gateway | public allowlist/reflection；pairing/no-partial/determinism；项目 Preview 成功 |
| 2D-3B | 复核项目模板对既有事务和字段门禁的消费 | Application focused/full；现有单文档模板回归 |
| AM-1A | Manifest DTO 与绑定闭合检查 | immutability/limits；Proposed 必有操作；PendingSchema 无操作 |
| AM-1B | 文档、ledger、全量验证与 clean package | Debug build；Application/IDE full；package hygiene |

## 7. 允许与禁止文件

允许：Application Automation/Experimental、项目模板编译器、对应 Application/IDE tests、
本阶段和项目状态/治理文档。禁止：任何 XAML、`ShellWindow.xaml.cs`、字段库 JSON/loader、parser、
diagnostics、completion、hover、save/backup、legacy。

AutomationId：无新增、无修改。

## 8. 验收判定

两个阶段只有在以下条件同时成立时完成：

1. rules/art 两个叶计划可由现有 Project Preview 一次预览；
2. 失败不泄漏 partial Plan/Manifest/Warnings；
3. Manifest 每个 Proposed binding 有精确操作证据，PendingSchema 不污染计划；
4. public allowlist、Gateway catalog/method surface 被 reflection 锁定；
5. Application/IDE 全量测试与 clean package 通过。

## 9. 自我审查

契约拒绝了三条容易返工的方案：将文件写权限放进 Manifest、复制第二套项目 Preview、以及为
缺失 Art schema 绕过字段库。保留的明确债务是 Cameo/Voxel 等 Art 字段 schema，偿还触发点为后续
获批的 `FIELD-REGISTRY-ART-*` 阶段。该债务不影响项目事务层和 body SHP 绑定验收，但会限制图标
绑定成为可应用 INI 操作。

完成后可验收的是“INI 结构化修改平台及首条 rules/art 跨文档链”；不能据此声称素材生成、任意
RA2 对象覆盖、AI UI 自动选择项目模板或 Cameo/VXL 写入已经完成。
