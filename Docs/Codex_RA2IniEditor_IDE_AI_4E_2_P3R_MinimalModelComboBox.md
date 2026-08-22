# Codex Task: RA2IniEditor.IDE AI-4E-2-P3R Minimal Model ComboBox Restore

## 0. Context

User refined the AI Assistant Advanced UI decision:

```text
1. 进阶区不应该放 Provider / Status / Intent / API Key 等大量信息。
2. 这里只保留一个“选择模型”即可。
3. 之前的下拉栏需要改回来。
```

This task supersedes the segmented/radio-only recommendation from the earlier P3 document.

The desired UI is:

```text
Advanced:
  one compact model ComboBox only
```

The ComboBox should select between:

```text
Mock
DeepSeek
```

No provider/status/API-key settings form should be shown.

---

## 1. Goal

Simplify the AI Assistant Advanced area to a single model selection dropdown.

Required result:

```text
1. Advanced only exposes model selection.
2. The model selector is a ComboBox/dropdown.
3. Options are Mock and DeepSeek.
4. Mock remains default.
5. DeepSeek is explicitly selected by user before live DeepSeek send flow.
6. No Provider / Status / Intent / API Key hint rows remain as permanent UI.
```

---

## 2. Product Decision

The user-facing control is:

```text
模型: [ Mock ▼ ]
```

Options:

```text
Mock
DeepSeek
```

Internally, the selected model may map to provider mode.

User-facing UI should not separately show:

```text
Provider
Status
Intent
API Key
BaseUrl
Timeout
```

DeepSeek missing configuration should be shown only as a chat message after the user selects DeepSeek and sends a prompt.

---

## 3. Hard Boundaries

Do not implement or change:

```text
DeepSeek adapter behavior
DeepSeek request/response mapping
API key loading rules
PromptBuilder
ContextProvider
Field Registry evidence retrieval
Diagnostics summary
Apply / Insert
file modification
Field Registry writes
whole-project context
auto-send context
diagnostic auto-fix
streaming output
retry loops
```

Do not modify:

```text
Field Registry services
diagnostics behavior
parser semantics
completion / hover / quick peek behavior
save preflight
BuiltIn field registry JSON
legacy files
solution / project files
```

Shell changes are allowed only inside the existing AI Assistant Advanced / composer UI.

---

## 4. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs, only if model selection state wiring is required
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs, only if current AI panel state is already there
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Do not modify DeepSeekRa2AiClient / DeepSeekRa2AiClientFactory / PromptBuilder / ContextProvider in this task.

---

## 5. Required UI Changes

### 5.1 Restore a single ComboBox

Use a ComboBox only for model selection.

Required AutomationId:

```text
AiAssistant.ModelSelector
```

The ComboBox must contain:

```text
Mock
DeepSeek
```

Mock must be selected by default.

### 5.2 Remove heavy Advanced rows

Remove/demote permanent visible rows:

```text
Provider: ...
Status: ...
Intent: ...
API Key through DEEPSEEK_API_KEY ...
BaseUrl ...
Timeout ...
```

Do not keep these as visible form rows.

### 5.3 ComboBox layout constraints

The previous dropdown caused UI crowding/overlay problems when too much content was present. This task restores the dropdown only as a single small model selector.

Requirements:

```text
1. ComboBox width must fit the right panel / composer Advanced area.
2. Do not stretch it across the editor.
3. MaxDropDownHeight should be small because there are only two items.
4. Dropdown items should not contain long text.
5. No extra disabled ComboBoxes for model/intent/status.
```

Suggested visible width:

```text
100-160 px
```

or fit to the Advanced popover/panel width.

### 5.4 Missing configuration message

If DeepSeek is selected and user sends without `DEEPSEEK_API_KEY`:

```text
show MissingConfiguration in chat history
```

Do not show a permanent API key explanation block in Advanced.

Optional tooltip is allowed:

```text
DeepSeek 使用 DEEPSEEK_API_KEY 环境变量。
```

Tooltip must not display a key.

---

## 6. Behavior Must Remain

Behavior from AI-4E-2 must remain:

```text
Mock is default.
DeepSeek is explicit.
Mock uses FakeRa2AiClient.
DeepSeek uses environment-only configuration.
Missing DEEPSEEK_API_KEY shows MissingConfiguration when sending.
No API key UI.
No settings persistence.
No Apply / Insert.
No editor text mutation.
```

This task changes only UI presentation.

---

## 7. AutomationIds

Preserve:

```text
AiAssistant.AdvancedButton
AiAssistant.AdvancedOptions
AiAssistant.ModelSelector
AiAssistant.GenerateButton
AiAssistant.PromptBox
AiAssistant.SafetyFooter
AiAssistant.ChatHistory
AiAssistant.ContextSummary
AiAssistant.Composer
```

`AiAssistant.ProviderSelector` may be removed from visible UI if it only existed for the old provider/status layout. If tests depend on it, update tests to the new model-selector contract.

Do not add:

```text
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
AiAssistant.ApplyButton
```

Avoid permanent visible:

```text
AiAssistant.ProviderStatus
AiAssistant.DeepSeekEnvironmentHint
AiAssistant.IntentAutoText
```

unless hidden/collapsed for compatibility; preferred is to update tests and remove them from visible contract.

---

## 8. Tests

Update boundary tests to match the new minimal ComboBox contract.

Required checks:

```text
1. AiAssistant.ModelSelector exists.
2. Model selector is a ComboBox or equivalent dropdown control.
3. Model selector contains Mock.
4. Model selector contains DeepSeek.
5. Mock is default.
6. No AiAssistant.ApiKeyTextBox exists.
7. No AiAssistant.SaveApiKeyButton exists.
8. No AiAssistant.ApplyButton exists.
9. No permanent Provider/Status/API key hint form block is required.
10. Generate/send behavior remains AI-4E-2 behavior.
11. DeepSeek missing configuration appears in chat after send attempt if behavior test exists.
```

Avoid pixel-perfect tests.

Do not require real DeepSeek or API key.

---

## 9. Manual Smoke Checklist

After implementation:

```text
1. Open AI Assistant.
2. Open Advanced.
3. Confirm only a model dropdown is shown.
4. Confirm options are Mock and DeepSeek.
5. Confirm Mock is selected by default.
6. Confirm there are no permanent Provider/Status/Intent/API Key rows.
7. Confirm no API key input exists.
8. Confirm no Save API key button exists.
9. Confirm no Apply button exists.
10. Select DeepSeek.
11. Send without DEEPSEEK_API_KEY and confirm missing configuration appears in chat.
12. Select Mock and confirm Mock send still works.
13. Confirm no editor text changes and no dirty state.
```

---

## 10. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 11. Final Report Format

Report:

```text
1. Phase completed: AI-4E-2-P3R.
2. Files changed.
3. Advanced UI simplification.
4. Model ComboBox restore details.
5. Removed/demoted Provider/Status/API hint rows.
6. AutomationIds preserved/updated.
7. Commands run.
8. Build result.
9. Test result.
10. Package result.
11. Confirmation provider behavior unchanged.
12. Confirmation no API key UI/settings persistence added.
13. Confirmation no Apply/Insert/file modification behavior added.
14. Manual smoke steps or result.
15. Remaining risks.
16. Recommended next phase.
```
