# AI Assistant Live Smoke Report

## 1. Environment

```text
Phase: AI-7A Live Smoke / AI Module Stabilization Checklist
Report date: 2026-06-03
Product boundary: RA2IniEditor.IDE-only
Report type: smoke checklist and issue-capture document
Source changes in this phase: none
GUI smoke execution by Codex: Not run
Live DeepSeek smoke execution by Codex: Not run
Manual user results: User verified / Pass
User feedback: Currently no obvious issues found.
```

This report records the live smoke scenarios run manually by the user against the AI Assistant MVP / Alpha candidate. Codex did not execute GUI or live DeepSeek smoke in this phase; manual scenario results are recorded from user-provided feedback.

## 2. Scenarios Run

| # | Scenario | Codex status | Manual status |
|---|---|---|---|
| 1 | Mock mode baseline | Not run | User verified / Pass |
| 2 | DeepSeek missing API key | Not run | User verified / Pass |
| 3 | DeepSeek live response | Not run | User verified / Pass |
| 4 | Stable draft generation | Not run | User verified / Pass |
| 5 | Conversation continuity | Not run | User verified / Pass |
| 6 | Evidence expansion | Not run | User verified / Pass |
| 7 | Markdown rendering and copy | Not run | User verified / Pass |
| 8 | Enter-to-send | Not run | User verified / Pass |

## 3. Results

### 3.1 Mock Mode Baseline

Status: User verified / Pass

Checklist:

- Open AI Assistant.
- Keep model/provider as Mock.
- Send a simple prompt.
- Confirm fake response appears.
- Confirm no network or API key is required.
- Confirm editor text is unchanged.
- Confirm dirty state is unchanged.

Observed result:

```text
User verified / Pass.
User feedback: currently no obvious issues found.
```

### 3.2 DeepSeek Missing API Key

Status: User verified / Pass

Checklist:

- Clear `DEEPSEEK_API_KEY` for the launched process.
- Select DeepSeek.
- Send a simple prompt.
- Confirm a MissingConfiguration message appears in chat.
- Confirm no crash.
- Confirm no API key input UI appears.

Observed result:

```text
User verified / Pass.
User feedback: currently no obvious issues found.
```

### 3.3 DeepSeek Live Response

Status: User verified / Pass

Checklist:

- Set `DEEPSEEK_API_KEY` in the environment.
- Launch IDE from a process that can see the environment variable.
- Select DeepSeek.
- Send a field explanation prompt such as `解释 Strength 字段`.
- Confirm response appears.
- Confirm editor text is unchanged.
- Confirm dirty state is unchanged.
- Confirm no Apply / Insert button exists.

Observed result:

```text
User verified / Pass.
User feedback: currently no obvious issues found.
```

### 3.4 Stable Draft Generation

Status: User verified / Pass

Prompt:

```text
帮我设计一个轻型防空车
```

Checklist:

- Output is structured Markdown.
- Clean INI block has no explanatory inline comments.
- Missing faction uses a TODO owner placeholder if the user did not specify faction.
- Generated IDs are listed under follow-up definitions.
- Field rationale is outside code blocks.
- Unverified fields go to optional / verify-before-use.

Observed result:

```text
User verified / Pass.
User feedback: currently no obvious issues found.
```

### 3.5 Conversation Continuity

Status: User verified / Pass

Follow-up prompt after scenario 3.4:

```text
在这个单位基础上，把它改成苏军单位。
```

Checklist:

- AI understands "这个单位" as the previous draft subject.
- AI does not claim the subject already exists in project files.
- Output remains draft/advisory.
- Faction/owner-related field evidence is used when available.

Observed result:

```text
User verified / Pass.
User feedback: currently no obvious issues found.
```

### 3.6 Evidence Expansion

Status: User verified / Pass

Prompt variants:

```text
把这个单位改成盟军背景。
给这个单位加上对空武器。
让这个单位可以部署成防空炮。
让这个单位可以运输步兵。
让这个单位隐形侦察。
```

Checklist:

- Evidence is no longer obviously too narrow for common fields.
- Unconfirmed seed keys are not treated as evidence.
- Output does not hallucinate unsupported field keys into clean draft.

Observed result:

```text
User verified / Pass.
User feedback: currently no obvious issues found.
```

### 3.7 Markdown Rendering And Copy

Status: User verified / Pass

Use a response containing:

```text
headings
bullet list
pipe table
inline code
fenced ini block
```

Checklist:

- Headings render.
- Lists render.
- Pipe tables render.
- Inline code renders without raw backticks.
- Fenced code blocks render as code cards.
- Copy full message copies original Markdown.
- Copy code block copies code without fence markers.

Observed result:

```text
User verified / Pass.
User feedback: currently no obvious issues found.
```

### 3.8 Enter-To-Send

Status: User verified / Pass

Checklist:

- Type a normal prompt and press Enter.
- Confirm the prompt sends.
- Type a multi-line prompt with Shift+Enter.
- Confirm newline is inserted and the message is not sent.
- Empty prompt plus Enter is a no-op.

Observed result:

```text
User verified / Pass.
User feedback: currently no obvious issues found.
```

## 4. Issues Found

Codex did not execute live GUI or live DeepSeek smoke in this phase. The user completed manual smoke and reported that there are currently no obvious issues.

Current issue table:

| Severity | Issue | Status | Notes |
|---|---|---|---|
| Blocking | None confirmed | User verified / Pass | User reports no obvious issues |
| High | None confirmed | User verified / Pass | User reports no obvious issues |
| Medium | None confirmed | User verified / Pass | User reports no obvious issues |
| Low | None confirmed | User verified / Pass | User reports no obvious issues |

Issue classification to use when manual results are provided:

- Blocking: crash, file mutation, dirty-state mutation, API key leak, broken save/diagnostics, or DeepSeek cannot work despite direct API success.
- High: context continuity broken, evidence too narrow for common follow-up, or Markdown rendering breaks code copy.
- Medium: UI layout/spacing, minor wording, or non-critical error messages.
- Low: cosmetic polish.

## 5. Fix Recommendations

No fix recommendation is made because the user reported no obvious issues in manual smoke.

Recommended next step:

```text
No immediate fix task is recommended from this smoke result.
```

If a clear bug is found, create a separate fix task before modifying source code.

## 6. MVP Readiness Judgment

Current judgment: AI Assistant MVP / Alpha ready.

AI Assistant MVP / Alpha is considered ready based on user-provided manual smoke results. Codex did not independently execute GUI or live DeepSeek smoke.

Readiness criteria to confirm:

- Mock mode works.
- DeepSeek mode works when `DEEPSEEK_API_KEY` is set.
- Missing API key is handled cleanly.
- No file modification or dirty state occurs.
- Stable draft output is acceptable.
- Follow-up references like "这个单位" work.
- Evidence retrieval is sufficient for common unit/weapon follow-ups.
- Markdown rendering and copy behavior are usable.
- No API key or sensitive data is exposed.

## 7. Validation

Validation for this documentation-only phase:

```powershell
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

Results:

```text
dotnet test: passed, 1432 tests
IdeOnly package: passed, packaged file count 765
```
