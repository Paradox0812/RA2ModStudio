# ASSET-VOX-1C-P2 Stage Result Ledger

Date: 2026-08-26  
Package state: Completed / real remote Geometry path certified

| Stage | Goal | Evidence | State |
|---|---|---|---|
| P2-0 | Audit official API/cost and Host reuse; freeze final contract | Code-fact audit + final contract | Completed |
| P2-1 | Implement remote provider adapter and request/query mapping | Internal provider executable + model-free request/query tests | Completed |
| P2-2 | Implement secret/cost/download/provenance gates | HTTPS/download/at-most-once/zero-public/Host-probe tests | Completed |
| P2-3 | Execute authorized free-only shape calls | First three attempts produced no Job; after the user explicitly authorized one additional post-fix call, it completed and produced Host-validated artifacts | Completed; calls used: 4/4 authorized, successful jobs: 1 |
| P2-4 | Regression/package/docs closeout | Build 0/0; AssetHost 47/47; Application 228/228; IDE 2779/2779; package 1309 | Completed |

## Verification matrix

| Gate | Result |
|---|---|
| `dotnet restore .\RA2IniEditor.IDE.sln` | Passed |
| `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed, 0 warnings / 0 errors |
| AssetHost/provider tests | Passed, 47/47; live opt-in branch not entered during final regression |
| Application tests | Passed, 228/228 |
| IDE tests | Passed, 2779/2779 |
| IdeOnly clean package | Passed, 1309 files |
| Provider exported public types | 0 |
| Real Tencent submit/query | Post-fix call 4 reached `DONE` after 42 polls / about 2m10s; successful jobs 1 |
| Non-billable connectivity isolation | Normal environment reached HTTP 401 with an invalid key; cleared Host environment failed; `SystemRoot/WINDIR/TEMP/TMP` together restored HTTP reachability |
| Real artifacts | GLB 8,991,920 bytes; PNG 77,888 bytes; sanitized provider JSON 1,076 bytes |
| GLB structural audit | glTF 2.0; declared length exact; 1 scene / node / mesh / primitive; 249,567 vertices; 499,698 triangles |
| Provider credit fields | Not returned; Tencent console remains the only free-pack consumption authority |

Approved test image facts: PNG, 1000x563, 529,376 bytes,
SHA-256 `76A47D80D7C1D58251724971703F93A6E31DD5C8E27FA38B73F4E4A620BBCCF1`.

## Diff intent audit

| Area | Intended | Result |
|---|---|---|
| New Tencent adapter + tests | Yes | Implemented |
| Existing AssetHost core/protocol | Narrow exception | Protocol unchanged; process runner now retains four explicit Windows runtime variables after clearing inherited environment |
| Application canonical voxel core | No | Unchanged |
| Shell/XAML/AutomationIds | No | Unchanged |
| INI/Field Registry/editor/save semantics | No | Unchanged |
| Generated model/image in source package | No | Real evidence exists only under excluded `artifacts/`; clean package excludes it |

## Deferred governance queue

- Accepted decision: keep Tencent HTTP and secret behavior in a separate local adapter; AssetHost protocol and provider-
  neutral lifecycle remain unchanged, with only the documented four-variable Windows child-runtime allowlist exception.
- Accepted decision: require a dedicated provider key and explicit free-only confirmation; never reuse generic keys.
- Accepted debt: a killed local adapter cannot cancel a Tencent job after successful submit because no cancellation API
  is documented. Impact is bounded to already-submitted remote work; the adapter never resubmits and the Host still
  returns no candidate on cancellation/timeout. Revisit if Tencent exposes a cancel endpoint.
- Public API: none; all new implementation is internal and the provider assembly exports zero types.

## Further-call gate

The original three-call budget was exhausted, and the user explicitly authorized one additional post-fix call. That
fourth call succeeded. No fifth generation may be submitted without new explicit authorization. The Host now retains
only `SystemRoot`, `WINDIR`, `TEMP` and `TMP`, while API keys, proxy variables and arbitrary user environment remain
excluded.

Before any future opt-in live test, all four facts must be true:

1. `RA2INI_HY3D_API_KEY` exists as a Windows User environment variable;
2. Tencent console shows a non-expired free resource pack with sufficient remaining credits and postpaid disabled;
3. `RA2INI_HY3D_FREE_ONLY_CONFIRMED=1` exists as a Windows User environment variable.
4. The user explicitly authorizes an additional generation call beyond the completed 4/4 run history.

Run exactly one call after that authorization. Inspect `ResultCreditConsumed` and the console balance before considering
any later call. Stop on any paid-resource or postpaid indication.
