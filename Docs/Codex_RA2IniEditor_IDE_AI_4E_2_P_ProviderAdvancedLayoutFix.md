# Codex Task: RA2IniEditor.IDE AI-4E-2-P Provider Advanced UI Layout Fix

## 0. Context

AI-4E-2 manual smoke found that the current AI Assistant Advanced provider UI has visible layout problems.

Observed issues from screenshots:

```text
1. Advanced area is too form-like and crowded inside the narrow right tool well.
2. Provider ComboBox popup expands/overlays awkwardly and can cover nearby content.
3. Text lines are clipped horizontally in the right panel.
4. Model / intent controls look like disabled WPF form fields and consume unnecessary space.
5. Composer / Advanced layout still has too much native WPF feel.
```

This task is a **UI layout fix only** for the AI Assistant Advanced area.

Do not change provider behavior.

Do not change DeepSeek send-flow semantics.

---

## 1. Goal

Make the Advanced provider UI fit the narrow right-side tool well.

Target result:

```text
1. No provider ComboBox popup that overlays across the panel/editor.
2. Provider selection should be compact and predictable.
3. Long status/hint text must wrap instead of being clipped.
4. Model/intent information should be compact read-only text unless real selection is implemented later.
5. The UI should feel like an IDE chat panel, not a stacked WPF settings form.
```

---

## 2. Hard Boundaries

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

## 3. Files Allowed

Allowed:

```text
RA2IniEditor.IDE/Views/ShellWindow.xaml
RA2IniEditor.IDE/Views/ShellWindow.xaml.cs, only if current provider UI state wiring needs minimal adjustment
RA2IniEditor.IDE/ViewModels/ShellViewModel.cs, only if current AI panel state is there
RA2IniEditor.Tests/IDE/IdeShellBoundaryTests.cs
RA2IniEditor.Tests/IDE/WpfAutomationHarnessBoundaryTests.cs
Docs/Codex_CurrentPhase.md
Docs/RA2IniEditor_IDE_Full_Codex_Context.md
```

Do not modify `DeepSeekRa2AiClient.cs` or `DeepSeekRa2AiClientFactory.cs` in this task unless there is a compile error caused by existing UI wiring.

---

## 4. Required UI Changes

### 4.1 Replace Provider ComboBox with compact selector

Current ComboBox/dropdown is not suitable for the narrow right panel.

Replace it with one of these compact patterns:

Preferred:

```text
segmented buttons / toggle buttons:
[Mock] [DeepSeek]
```

Alternative:

```text
two radio buttons:
(•) Mock   ( ) DeepSeek
```

Rules:

```text
1. No popup dropdown for provider selection.
2. Must fit inside right tool well width.
3. Mock remains default.
4. DeepSeek selection remains explicit.
5. Selecting DeepSeek must not auto-send.
```

Preserve AutomationId:

```text
AiAssistant.ProviderSelector
```

If the control type changes, keep the AutomationId on the selector container.

---

### 4.2 Remove fake Model ComboBox

Current model row appears as a disabled ComboBox and increases WPF form feel.

Replace with compact read-only text:

```text
模型：DEEPSEEK_MODEL 环境变量，未设置时使用默认模型。
```

or:

```text
模型：Mock / DeepSeek 环境变量配置
```

Preserve:

```text
AiAssistant.ModelSelector
```

by placing the AutomationId on the read-only model summary text/container.

Do not implement real model selector in this phase.

---

### 4.3 Remove / demote intent selector

The product decision is:

```text
任务意图默认 Auto，由后续 PromptBuilder / DeepSeek 判断。
```

Do not show a prominent disabled "自动判断意图" ComboBox.

Instead use compact text:

```text
意图：自动判断
```

If existing tests require `AiAssistant.TaskKindSelector`, keep it as a hidden/collapsed or compact read-only placeholder, not as a visible dropdown.

---

### 4.4 Wrap provider status and environment hint

These lines must wrap:

```text
Status
DEEPSEEK_API_KEY hint
DeepSeek selected / missing configuration text
```

Required:

```text
TextWrapping="Wrap"
```

or equivalent.

No long status line should be clipped in the right tool well.

Preserve / add:

```text
AiAssistant.ProviderStatus
AiAssistant.DeepSeekEnvironmentHint
```

---

### 4.5 Keep Advanced compact

Advanced should be compact and readable.

Suggested structure:

```text
进阶
Provider: [Mock] [DeepSeek]
状态：Mock 模式 / DeepSeek 已选择 / DeepSeek 未配置
模型：DEEPSEEK_MODEL 环境变量
上下文：发送时使用当前有限上下文
提示：API Key 通过 DEEPSEEK_API_KEY 环境变量配置
```

Avoid:

```text
multiple stacked disabled ComboBoxes
wide dropdown popups
large bordered setting boxes
text clipping
API key input
save key button
Apply button
```

---

## 5. Provider Behavior Must Remain

The behavior from AI-4E-2 must remain:

```text
Mock is default.
DeepSeek must be explicitly selected.
Mock send uses FakeRa2AiClient.
DeepSeek send uses environment-only configuration.
Missing DEEPSEEK_API_KEY shows MissingConfiguration.
No API key UI.
No settings persistence.
No Apply / Insert.
No editor text mutation.
```

This task changes only visual layout and control shape.

---

## 6. AutomationIds

Preserve existing:

```text
AiAssistant.AdvancedButton
AiAssistant.AdvancedOptions
AiAssistant.ModelSelector
AiAssistant.ProviderSelector
AiAssistant.ProviderStatus
AiAssistant.DeepSeekEnvironmentHint
AiAssistant.GenerateButton
AiAssistant.PromptBox
AiAssistant.SafetyFooter
AiAssistant.ChatHistory
AiAssistant.ContextSummary
```

Forbidden:

```text
AiAssistant.ApiKeyTextBox
AiAssistant.SaveApiKeyButton
AiAssistant.ApplyButton
```

If provider selector changes from ComboBox to ToggleButton/RadioButton group, do not remove the selector AutomationId; move it to the grouping container.

Allowed additions:

```text
AiAssistant.ProviderMockOption
AiAssistant.ProviderDeepSeekOption
AiAssistant.IntentAutoText
```

---

## 7. Tests

Update boundary tests only.

Required checks:

```text
1. AiAssistant.ProviderSelector still exists.
2. AiAssistant.ProviderMockOption exists, if added.
3. AiAssistant.ProviderDeepSeekOption exists, if added.
4. AiAssistant.ProviderStatus exists.
5. AiAssistant.DeepSeekEnvironmentHint exists.
6. AiAssistant.ModelSelector still exists but is not a real API key/model settings UI.
7. No AiAssistant.ApiKeyTextBox exists.
8. No AiAssistant.SaveApiKeyButton exists.
9. No AiAssistant.ApplyButton exists.
10. Mock is still the default provider.
11. Generate/send behavior still follows AI-4E-2.
```

Source-boundary tests may assert that provider selection no longer uses a ComboBox popup if practical.

Avoid pixel-perfect tests.

Do not require real DeepSeek or API key.

---

## 8. Validation Commands

Run full validation because Shell XAML may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 9. Manual Smoke Checklist

After implementation:

```text
1. Open AI Assistant.
2. Open Advanced.
3. Confirm Provider no longer uses an awkward ComboBox popup.
4. Confirm [Mock] [DeepSeek] or equivalent compact selector fits in the panel.
5. Confirm long status and DEEPSEEK_API_KEY hint text wraps.
6. Confirm no API key input exists.
7. Confirm no save key button exists.
8. Confirm no Apply button exists.
9. Confirm Mock is default.
10. Select DeepSeek and confirm selection is visible.
11. Send in Mock mode and confirm Fake response.
12. Send in DeepSeek mode without DEEPSEEK_API_KEY and confirm MissingConfiguration.
13. Confirm no editor text changes and no dirty state.
```

---

## 10. Final Report Format

Report:

```text
1. Phase completed: AI-4E-2-P.
2. Files changed.
3. Provider UI layout changes.
4. ComboBox/dropdown issue resolved? yes/no.
5. Text wrapping changes.
6. AutomationIds preserved/added.
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
