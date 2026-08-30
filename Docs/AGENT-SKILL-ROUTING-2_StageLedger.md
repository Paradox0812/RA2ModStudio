# AGENT-SKILL-ROUTING-2 Stage Result Ledger

Date: 2026-08-25  
Status: Implemented / automated verification passed / real provider not run  
Contract: `Docs/AGENT-SKILL-ROUTING-2_ModelSelectedSkillManifestContinuousFinalContract.md`

## 1. Stage results

| Stage | Detailed plan | Self-review gate | Result | Evidence |
|---|---|---|---|---|
| 2A Catalog Manifest | Project the existing immutable catalog into ordered body-free metadata and keep catalog lookup canonical | No second registry, no body leakage, deterministic IDs/hashes | Passed | Manifest projection/order/hash/body-absence tests |
| 2B Intent Schema | Add selected Skill IDs and knowledge gaps to the strict first-call tool schema/package/parser; derive the ID enum from the current manifest | Exact root shape, bounded arrays/items, unknown IDs retained for local diagnosis | Passed | Dynamic schema, valid/unknown/duplicate/overflow/parser tests |
| 2C Resolver | Merge capability requirements, model order, cross-cutting trust and extension fallback; enforce mode and 14 KiB body budget | Required-first, stable dedup, explicit unavailable/omitted facts, deterministic fallback | Passed | capability/mode/unknown/budget/fallback tests |
| 2D Pipeline | Share the PromptBuilder catalog with call one; resolve after intent validation; pass explicit resolution to call two | Work remains two calls, Chat remains one, execution does not independently reselect | Passed | pipeline/prompt/project integration tests |
| 2E Boundaries | Verify body isolation, dynamic catalog enum, no unknown injection, explicit resolution report and unchanged tool authority | No third call, no path/apply/save/network expansion, fixed prompt/body budgets | Passed | focused 67/67 |
| 2F Release Gate | Full restore/build/test, diff/hygiene audit, docs and clean package | All mandatory gates green; no real provider or GUI claim | Passed | commands below |

## 2. Implemented data flow

```text
Bundled Skill files
  -> one Ra2AgentSkillCatalog snapshot
      -> compact manifest -> DeepSeek call 1
      -> validated selected_skill_ids + knowledge_gaps
      -> local required/requested/mode/budget resolver
      -> full active Skill bodies -> DeepSeek call 2
```

- Manifest fields: ID, version, description, domains, modes, instruction character count and SHA-256.
- First-call output: maximum 6 ordered Skill recommendations and 6 knowledge gaps.
- Required capability Skills cannot be removed by a model omission.
- Unknown/mode-incompatible IDs and budget omissions are observable internal facts, not silent substitutions.
- Chat retains the existing one-call local selection path.

## 3. Diff intent table

| Area | Intended change | Explicit non-change |
|---|---|---|
| Skill catalog | Add manifest projection and one resolver | Loader/root/script policy unchanged |
| Intent tool | Catalog-aware schema and two bounded arrays | Capability IDs, route authority and provider unchanged |
| Prompt builder | Consume explicit resolution and display resolution facts | Authoring tools and application safety rules unchanged |
| Pipeline | Same snapshot from first manifest through second injection | Exactly two Work calls; no retry/third call |
| Tests/docs | Add boundary matrix and current-state evidence | No UI automation or real DeepSeek |

## 4. Verification matrix

| Command | Result |
|---|---|
| Focused Skill/intent/pipeline/project tests | Passed 67/67 |
| `dotnet restore .\RA2IniEditor.IDE.sln` | Passed |
| `dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore` | Passed, 0 warnings / 0 errors |
| `dotnet test .\RA2IniEditor.Application.Tests\RA2IniEditor.Application.Tests.csproj -c Debug --no-build` | Passed 188/188 |
| `dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build` | Passed 2668/2668 |
| `git diff --check` | Passed; repository line-ending notices only |
| `powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly` | Passed; 1213 files |
| real DeepSeek / GUI | Not run by contract |

## 5. API, persistence, and authority review

- Public .NET API: unchanged. All new records, properties and JSON handling are IDE-internal.
- Provider-visible Experimental JSON: `selected_skill_ids` and `knowledge_gaps` are now required in the first Work
  tool result; the catalog-derived ID enum is request-local and non-persistent.
- Persistence: none.
- Apply/Undo/Redo/Save: unchanged.
- Shell/XAML/AutomationIds: unchanged.
- Field Registry/parser/Completion/Hover/Diagnostics: unchanged.
- Legacy editor: not restored.

## 6. Deferred governance queue

| ID | Status | Follow-up |
|---|---|---|
| AGENT-SKILL-ROUTING-2-D001 | Open | Real DeepSeek acceptance should verify selection quality and schema compliance without changing local authority |
| AGENT-SKILL-ROUTING-2-D002 | Open | Skill sources remain prose/frontmatter; machine-readable per-rule provenance is not yet available |
| AGENT-SKILL-ROUTING-2-D003 | Open | Unknown/budget/gap facts are internal diagnostics and are not yet surfaced in the UI |
| AGENT-SKILL-ROUTING-2-D004 | Open | No automatic repair/retry/fallback exists after a malformed first or second provider response |
| AGENT-SKILL-ROUTING-2-D005 | Controlled | External/hot-reload/executable Skills remain forbidden pending a separate trust/versioning contract |

## 7. Review conclusion

The package is complete against its contract. It removes the previous split authority where call one could not see
Skills and call two independently chose them. Model selection is now useful but non-authoritative: capability and
safety boundaries remain local. No mandatory failure or unreviewed scope expansion remains.
