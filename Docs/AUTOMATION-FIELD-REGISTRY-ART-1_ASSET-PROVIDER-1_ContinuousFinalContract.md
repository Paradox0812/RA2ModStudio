# FIELD-REGISTRY-ART-1 + ASSET-PROVIDER-1 — Continuous Final Contract

日期：2026-08-24  
状态：用户已授权生成、自审并连续执行  
前置：`CONTENT-2D-3 / ASSET-MANIFEST-1` completed  
风险：R4（BuiltIn authoring schema + Experimental public API）

## 1. 目标

连续完成两个阶段：

1. `FIELD-REGISTRY-ART-1`：以 YR `artmd.ini` 字段说明和实际条目为来源证据，为通用
   `ArtObject` 补齐 `Cameo`、`AltCameo`、`Voxel`、`Remapable` 的精确 authoring schema；
2. 将现有 `techno-rules-art-asset-binding` 的 Cameo 绑定从 `PendingSchema` 提升为
   `Proposed`，并让同一个 Project Plan 同时包含 body 与 cameo 两个 art 字段操作；
3. `ASSET-PROVIDER-1`：提供首个 headless Existing-Asset Provider，把 Host 显式提交的、
   有界二进制素材匹配到现有 Manifest，返回不可变产物、SHA-256 和有限验证级别；
4. Provider 结果全成功或零产物，不获得文件系统、Apply、Save、网络或模型权限。

## 2. 来源审计与范围裁决

本阶段采用以下证据：

- YR `artmd.ini` 文件头对 `Cameo`、`Voxel`、`Remapable` 的字段语义与默认值说明；
- 同一 YR `artmd.ini` 中大量 `Cameo/AltCameo/Voxel/Remapable` 实际条目；
- 既有 BuiltIn v3.2 `ArtObject.Image` 与 `ArtObject.Theater` 的 source-backed 组织方式。

本阶段不提升 `CameoPalette`、`CameoPCX` 或任意 Ares/Phobos 扩展字段。它们的适用上下文、
文件格式与 provider 能力不同，必须在独立来源审计后处理，不能由字段名类推。

## 3. FIELD-REGISTRY-ART-1 数据契约

新增四条 `ArtObject` BuiltIn v3.2 定义：

| Key | Editor | Schema | 语义 |
|---|---|---|---|
| `Cameo` | Reference | Reference | 侧边栏常规图标资源 ID |
| `AltCameo` | Reference | Reference | 侧边栏替代/升级图标资源 ID |
| `Voxel` | Boolean | Boolean / YesNo | 对象主体是否按 VXL/HVA 加载 |
| `Remapable` | Boolean | Boolean / YesNo | 图像是否允许按所属方颜色重映射 |

四条均为 `sourceKind=Yuri`、`FallbackReference`、`Conservative`、允许用户覆盖、不阻断保存，
并使用新的 source-verified quality 标识。不得改变 provider priority、loader、导入/学习或
Field Registry UI。

项目模板的 art 叶定义由：

```text
Image=<bodyAssetId>
```

提升为：

```text
Image=<bodyAssetId>
Cameo=<cameoAssetId>
```

对应 Cameo binding 必须改为 `Proposed`；Manifest closure 必须证明两个 Proposed binding 都有
精确 operation。Project Preview 预期总操作数由 2 增为 3。

## 4. ASSET-PROVIDER-1 Public Experimental 契约

新增独立于 INI Gateway 的 provider surface：

- `IRa2AutomationAssetProvider`：只暴露 descriptor 与 `Resolve`；
- `Ra2AutomationExistingAssetProvider`：首个确定性 passthrough provider；
- `Ra2AutomationAssetProviderDescriptor`：provider ID/version/supported kinds；
- `Ra2AutomationAssetSource`：Host 显式提供的 requirement ID、文件名、kind 和防御性复制内容；
- `Ra2AutomationAssetArtifact`：匹配后的文件名/kind/长度/SHA-256/验证级别和复制内容；
- `Ra2AutomationAssetProviderResult`：成功产物或失败证据；
- `Ra2AutomationAssetProviderFailureKind`、`Ra2AutomationAssetVerificationLevel`。

固定边界：

```text
maximum one source/artifact bytes = 16 MiB
maximum aggregate bytes = 64 MiB
maximum requirements = existing Manifest limit 32
```

扩展名映射固定为：

```text
ShpAnimation -> .shp
Cameo       -> .shp
VxlModel    -> .vxl
HvaAnimation-> .hva
```

Provider 只证明 requirement identity、文件名、kind、扩展名、非空、大小与 SHA-256；验证级别必须
明确为 `IdentityExtensionAndHash`，不得声称已经解析 SHP/VXL/HVA、验证帧数/尺寸/调色板或游戏可用性。

## 5. 原子性与失败契约

- Manifest 含任何 `PendingSchema` binding 时返回 `InvalidManifest`；
- source 缺失、多余、重复，或文件名/kind/扩展名不匹配时整体失败；
- aggregate content 超限时整体失败；
- 取消映射为 `Canceled`；非致命意外映射为 `ProviderFailed`，不得泄漏异常细节；
- 失败结果的 Artifacts 必须为空；成功结果的 related failure IDs 必须为空；
- 产物顺序严格跟随 Manifest requirement 顺序；同一输入得到同一 SHA-256 与 shape；
- 输入和输出内容均防御性复制，调用方不能通过修改原数组改变 provider facts。

## 6. 架构与权限边界

- Application 继续只依赖 Core；Provider 不使用 `File/Directory/Environment/Process`；
- Provider 不加入当前 INI Capability Gateway。本阶段先冻结素材 provider protocol，未来 Host
  组合 Manifest、Provider 与持久化事务时再单独增加 capability；
- Asset Manifest 仍是需求/绑定事实，Artifact 仍不是 Apply/Save authority；
- 不新增序列化/wire 承诺、Artifact Registry、Job/Event、缓存或后台执行；
- 不调用真实 DeepSeek、图像模型或其他付费服务。

## 7. 允许与禁止文件

允许：

- `RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json`
- 对应 Infrastructure tests
- Core minimal BuiltIn provider 的同名 ArtObject source gate 与对应 tests
- Application `Automation/Experimental` 资产契约和纯 provider 算法
- 项目模板编译器及 Application tests
- 本阶段、Public API、Decision、Debt、CurrentStatus/Context 文档

禁止：任何 XAML、Shell、IDE Host、parser、diagnostics、completion、Hover、Save/Backup、Undo/Redo、
Field Registry priority/loader/import/learning、project files、legacy。

AutomationId：无新增、无修改。

## 8. 连续阶段与门禁

| Stage | 内容 | 必选门禁 |
|---|---|---|
| FRA-1A | 来源、字段 shape 与作用域锁定 | JSON 静态解析；exact identity 无重复 |
| FRA-1B | 四条 schema + Cameo Project operation | BuiltIn 定向；project template/production provider 定向 |
| FRA-1C | 阶段审查 | no placeholder；no provider/loader semantic diff；Manifest closure |
| AP-1A | public provider contract + allowlist | reflection/immutability/limits/defensive-copy |
| AP-1B | Existing-Asset Provider | success/determinism/mismatch/missing/extra/limit/cancel/no-partial |
| AP-1C | 阶段审查与完整回归 | Application full；IDE non-UI full；Debug build；clean package |

## 9. 自我审查结论

契约通过。为避免返工，实施前已做四项修正：

1. 加入同一来源已明确的 `AltCameo`，避免下一次图标 profile 再改同一基础字段包；
2. 不把 `CameoPalette/CameoPCX` 混入通用 `ArtObject`；
3. Provider 返回精确的有限验证级别，不把扩展名和 hash 冒充格式解析；
4. Provider 与文件持久化、INI Gateway 分离，避免过早公开路径权限或打破 Application 依赖方向。

完成这两个阶段后，可验收的是：完整的 rules/art body+Cameo INI Preview，以及可被未来 Host
消费的首个真实、有界 Existing-Asset Provider。仍不能声称已经生成、编码、落盘或运行时验证
SHP/Cameo/VXL/HVA。
