# AGENT-CONTEXT-3 Stage Ledger

| Stage | Scope | Status | Evidence |
|---|---|---|---|
| C3-0 | Code-fact audit and final contract | Completed | Contract records existing snapshot, two-call, HLI, and Shell boundaries |
| C3-1 | Immutable bounded context projection | Completed | Provider projection contains only symbolic targets, file names, revisions and registry revisions |
| C3-2 | Shared conversation/subject/project context | Completed | Focused tests compare the conversation block and project projection across both Work requests |
| C3-3 | Model-requested bounded HLI queries | Completed | `get_section` and `resolve_reference` pass through the existing Gateway; path target is rejected |
| C3-4 | Pipeline/Shell integration and observability | Completed | Shell passes captured contexts and existing Gateway; no XAML change; focused boundary suite 100/100 |
| C3-5 | Full verification, documentation, clean package | Completed | restore/build passed; Application 188/188; IDE 2673/2673; clean package is the final recorded gate |
| C3-FIX1 | Query target continuity and actionable wrong-document failure | Completed | Manual provider probe exposed a current-document/project scope mismatch; Host now normalizes field edits with explicit rules/art queries to the project route, carries the resolved target into execution rules, and reports cross-document Section location without retargeting. Focused 23/23; Application 188/188; IDE 2675/2675 |

## Deferred governance queue

| Item | Trigger | Flush point |
|---|---|---|
| Public API review | Provider-visible intent JSON changes | C3-5 |
| Architecture decision | Shared two-call context and local query loop | C3-5 |
| Technical debt audit | Any compatibility fallback or incomplete verification | C3-5 |
| Project status refresh | Mandatory gates complete | C3-5 |

## Final review

- Work provider calls remain exactly 2; Chat remains 1.
- The local query hop is not a provider call and cannot read arbitrary paths.
- Application public API, Gateway catalog/methods, persistence, parser, Field Registry,
  Preview, Apply, Save and Undo/Redo contracts are unchanged.
- `ShellWindow.xaml` and all AutomationIds are unchanged. The code-behind change is
  limited to passing already-captured contexts and the existing Gateway.
- No compatibility adapter or implementation shortcut remains to enter a technical-debt
  register. The original real DeepSeek C3 probe failed before FIX1 with `未找到目标 Section。`;
  the code path is repaired and fully automated-tested, but the same real-provider scenario
  remains a manual product acceptance item after FIX1.
