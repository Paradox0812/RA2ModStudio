# FIELD-REGISTRY-ART-1 + ASSET-PROVIDER-1 — Stage Ledger

日期：2026-08-24  
契约：`Docs/AUTOMATION-FIELD-REGISTRY-ART-1_ASSET-PROVIDER-1_ContinuousFinalContract.md`

## Stage Result Ledger

| Stage | Goal | Verification | State |
|---|---|---|---|
| FRA-1A | 来源、字段 shape、程序集与复用边界审计 | YR artmd source + active v3.2 + project compiler/provider boundary | Completed |
| FRA-1B | ArtObject schema + Cameo project operation | JSON 4 rows/0 duplicate；project template 9/9；field/Core focused 731/731 | Completed |
| FRA-1C | Field/Manifest closure review | 3 Project operations；body/Cameo bindings both Proposed；no provider/loader change | Completed |
| AP-1A | Provider public contract | reflection/allowlist/immutability/limits/defensive-copy included in focused 24/24 | Completed |
| AP-1B | Existing-Asset deterministic resolver | success/hash/order/mismatch/missing/extra/aggregate/cancel/no-partial included in focused 24/24 | Completed |
| AP-1C | Full verification and package | restore/build；Application 186/186；IDE 2634/2634；package 1206 files | Completed |

## Deferred Governance Queue

### PublicApiLedger Pending Entries

| Stage | API | Stability | Expected next use |
|---|---|---|---|
| AP-1 | Existing-Asset Provider contracts/result/artifact | Experimental / implemented | Asset Host persistence、future generator adapters |

### TechnicalDebt Pending Entries

| ID | Area | Reason | Risk | Repayment trigger | Status |
|---|---|---|---|---|---|
| ASSET-PROVIDER-1-D001 | Binary format validation | v1 only verifies identity/extension/hash | malformed SHP/VXL/HVA can pass provider | first codec/parser adapter | Accepted / open |
| ASSET-PROVIDER-1-D002 | Asset persistence/transaction | Application provider intentionally has no file authority | artifacts are not placed into project | ASSET-HOST-1 | Accepted / open |

### DecisionLog Candidate Entries

| Stage | Decision | Status | Rejected alternative |
|---|---|---|---|
| FRA-1/AP-1 | source-backed Art schema and pure provider are separate authorities | Accepted / implemented | Manifest/provider bypass Field Registry or write files directly |

## Review Result

- YR source-backed `Cameo/AltCameo/Voxel/Remapable` 只进入 ArtObject schema 和 Core minimal source gate；
  Field Registry priority/loader/import/learning 均未改变。
- 当前 SHP profile 只新增 `Cameo=<cameoAssetId>`，没有错误写入 `Voxel/Remapable`。
- Public provider interface 具备外部可实现的严格工厂；Artifact 自行计算 SHA-256，Result 强制
  Manifest 顺序、kind、文件名和 Proposed binding 闭合。
- Existing-Asset Provider 不读取/写入文件，不调用网络/模型；失败返回零 Artifact。
- Gateway catalog 9、methods 11 保持不变；Application allowlist 69 -> 77。

## Final Verification

```text
dotnet restore RA2IniEditor.IDE.sln: Passed / up-to-date
dotnet build Debug --no-restore: Passed, 0 errors; final rerun has 1 pre-existing test CS8602 warning
Application.Tests: Passed 186/186
FRA/AP focused: Passed 24/24；field/Core focused 731/731
IDE non-UI tests: Passed 2634/2634
IdeOnly clean package: Passed, 1206 files
Legacy restored: No
Shell/XAML changed by this continuous package: No
Parser/diagnostics/completion/save/provider-priority semantics changed: No
```
