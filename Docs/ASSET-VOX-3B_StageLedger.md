# ASSET-VOX-3B Stage Result Ledger

日期：2026-08-28  
阶段：Accepted Candidate & VOX Export  
契约：`Docs/ASSET-VOX-3B_AcceptedCandidateVoxExportFinalContract.md`

## 预实施自审

- Risk：R4。原因是新增用户显式触发的素材文件写入；影响被限制在独立 Save-As，不接入项目 Apply/Save。
- Architecture：复用现有 canonical snapshot 和 `Ra2MagicaVoxelCodec`；不创建第二套模型或 writer。
- Data：最终候选为 immutable/session-only derived state；不序列化、不进入项目事实源。
- Reuse：复用现有工作区、预览模式、状态/取消机制、SaveFileDialog 和 canonical codec。
- Public API：0 change；新增类型全部为 IDE internal。
- Stop rules：任何回读不一致、目标不安全、覆盖未确认或全量门禁失败都不得判定完成。

## 3B-1 — Accepted candidate authority

状态：Completed / focused verified

- 新增不可变 `Ra2VoxelAcceptedCandidate` 和六种可物化候选类型。
- 记录 canonical hash、候选名称、建议 VOX 文件名和 session generation。
- 不序列化，不改变 Application public contract。

## 3B-2 — Workspace candidate lifecycle

状态：Completed / focused verified

- 原始、直接、平滑、Agent 几何、普通着色和对比度着色可显式固化。
- Difference、Structure Regions、Region Mask、Palette 不能固化。
- 单纯切换审阅视图不清除最终候选；源/工作几何/风格结果变化会清除。
- 用户可显式用另一个可物化候选替换旧候选。

## 3B-3 — Verified VOX export

状态：Completed / focused verified

- 复用 `Ra2MagicaVoxelCodec` 执行编码、回读和确定性重编码验证。
- 同目录临时文件、物理 flush 和原子 publish。
- 当前源不可覆盖；其他目标需显式覆盖许可；取消和失败不产生半成品。

## 3B-4 — Product UI and tests

状态：Completed / focused verified

- 保留 `VoxelStyle.AcceptSession` 并更新为“固化最终候选”。
- 新增 `VoxelStyle.FinalCandidate.Status` 和 `VoxelStyle.ExportVox`。
- 定向用例覆盖候选生命周期、不可物化视图、导出、覆盖、取消和 UI 契约。
- 定向结果：23/23 passed。

## 3B-5 — Final verification and documentation

状态：Completed / automated verified

- `dotnet restore .\RA2IniEditor.IDE.sln`：passed；所有项目已是最新。
- `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore`：passed；0 warnings / 0 errors。
- AssetHost tests：50/50 passed。
- Application tests：285/285 passed。
- IDE tests：2844/2844 passed。
- 3B focused tests：23/23 passed。
- IdeOnly clean package：passed；1389 files，输出 `artifacts/RA2IniEditor.IDE.SourceClean.zip`。
- 人工 WPF Save-As smoke 不在自动测试范围，交由用户在新构建上验收。

## Diff Intent Audit

- `RA2IniEditor.IDE/AssetAuthoring`：新增最终候选和 VOX 导出事务。
- `RA2IniEditor.IDE/ViewModels/AssetAuthoring`：接入候选生命周期和导出状态。
- `RA2IniEditor.IDE/Views/AssetAuthoring`：在既有工作区增加固化/导出 UI。
- `RA2IniEditor.Tests/IDE`：新增行为、负向和 UI 契约测试。
- `Docs`：记录已批准契约、实现事实和当前能力。
- 未触及 Shell、INI、Field Registry、Application codec、项目 Apply/Save 或 VXL/HVA。

## Remaining risks

- 自动测试可以认证字节和事务边界，不能替代 Windows SaveFileDialog 与实际用户目录权限的人工 smoke。
- 当前导出是单模型 VOX 副本；炮塔/炮管装配、VXL/HVA 和项目绑定仍未实现。

## Final risk recheck

- Final risk remains R4 because the feature writes an explicitly chosen asset file.
- No unexpected public API, persistence, Shell, INI or project-transaction expansion was found.
- Mandatory automated gates passed. The only NotRun item is the physical WPF dialog/interaction smoke.
