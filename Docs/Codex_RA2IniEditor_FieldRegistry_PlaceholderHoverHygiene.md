# Codex Task: RA2IniEditor.IDE Field Registry Placeholder Hover Hygiene

## 0. Context

Manual UI smoke found that hover display still exposes placeholder / template text from the Field Registry.

Observed example:

```text
Integer MinDebris
YR 内置参考字段：MinDebris。适用于 Techno 类型配置，值类型为 文本。原始英文说明已经至模板，不能直接用于 Hover
来源 Global    适用 Building
```

Problem:

```text
The UI layout is acceptable, but the Field Registry content quality is not.
Placeholder/template descriptions must not be shown directly in Hover.
```

This is not a UI chrome issue. It is Field Registry content / display hygiene.

---

## 1. Goal

Prevent placeholder or template text from appearing in Hover / Quick Peek / AI evidence display surfaces.

Required result:

```text
1. Detect Field Registry description placeholders.
2. Do not show placeholder text as if it were a real field description.
3. Preserve valid source/provenance/type/section information.
4. Do not fabricate descriptions.
5. Add tests for placeholder suppression.
```

---

## 2. Problem Scope

The issue may come from one or more of:

```text
BuiltIn field pack JSON
Global active field pack JSON
Local field registry import/normalization
Hover display resolver
Quick Peek display resolver
AI Field Evidence display formatting
```

Implementation must first inspect the existing data path before changing files.

---

## 3. Hard Boundaries

Do not change:

```text
Field Registry priority: Project > Global > BuiltIn
Field Registry loader behavior except display hygiene if needed
Parser semantics
Diagnostics semantics
Completion semantics
Save preflight
AI provider behavior
Apply / Rollback / Cleanup writer behavior
Shell layout / toolbar / icon resources
legacy files
solution / project files
```

Do not fabricate official descriptions.

If a field only has placeholder description, show a neutral fallback such as:

```text
暂无可用字段说明。
```

or omit the description line, while still showing:

```text
field name
value type
section/type applicability
source/provenance
```

---

## 4. Read-only Inspection First

Before implementation, inspect:

```text
Field Registry built-in pack files
Global / Project active field pack loading path
Ra2FieldDisplayResolver or equivalent hover display logic
Ra2HoverProvider
Ra2FieldQuickPeekService
Ra2FieldRegistryAiEvidenceProvider
tests related to Hover / Quick Peek / Field Registry display
```

Find all placeholder patterns currently present.

Possible placeholder patterns include:

```text
原始英文说明
不能直接用于 Hover
placeholder
TODO
TBD
待补充
未整理
模板
占位
```

Do not assume this list is complete.

---

## 5. Recommended Implementation

### 5.1 Add description hygiene helper

Preferred small helper:

```text
Ra2FieldDescriptionHygiene
```

or equivalent internal helper.

Responsibilities:

```text
bool IsPlaceholderDescription(string? text)
string? SanitizeDescription(string? text)
```

Rules:

```text
1. Null/empty/whitespace -> no description.
2. Known placeholder/template text -> no description.
3. Real descriptions -> preserved.
4. No fake official explanations.
```

### 5.2 Apply at display boundary

Preferred place:

```text
Display resolver / Hover / Quick Peek formatting boundary
```

Do not mutate the source registry data unless the placeholder was introduced by generated built-in pack and there is an approved data cleanup phase.

Reason:

```text
Suppressing at display boundary avoids changing import/registry semantics and prevents multiple surfaces from leaking placeholders.
```

If multiple surfaces format descriptions independently, either:

```text
1. route them through shared display resolver, or
2. reuse the hygiene helper in each display boundary.
```

### 5.3 Optional data cleanup

If built-in JSON contains placeholder descriptions, record them in a report.

Do not mass-edit built-in JSON unless user approves a separate data quality phase.

---

## 6. Tests

Add focused tests for:

```text
1. Hover does not display "原始英文说明...不能直接用于 Hover".
2. Hover preserves valid description text.
3. Hover still displays field name / value type / applicability / source.
4. Quick Peek does not display placeholder descriptions, if it uses the same display path.
5. AI evidence display does not include placeholder description text, if applicable.
6. Field Registry priority remains unchanged.
7. Completion behavior remains unchanged.
8. Diagnostics behavior remains unchanged.
```

If existing tests are narrow, add a unit test for the hygiene helper plus one integration-like hover resolver test.

Avoid pixel-perfect UI tests.

---

## 7. Manual Smoke Checklist

After implementation:

```text
1. Open an INI section containing MinDebris or another affected field.
2. Hover the field.
3. Confirm placeholder/template text is not displayed.
4. Confirm field name, type, applicability, and source still display.
5. Hover a field with a real description.
6. Confirm real description still displays.
7. Check Quick Peek / AI evidence if applicable.
8. Confirm no change to completion/diagnostics/save behavior.
```

---

## 8. Validation Commands

Run full validation because source/tests/data display behavior may change:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 9. Final Report Format

Report:

```text
1. Phase completed.
2. Files inspected.
3. Files changed.
4. Placeholder patterns found.
5. Display hygiene implementation.
6. Surfaces affected: Hover / Quick Peek / AI evidence.
7. Tests added/updated.
8. Commands run.
9. Build result.
10. Test result.
11. Package result.
12. Confirmation Field Registry priority unchanged.
13. Confirmation no parser/diagnostics/completion/save behavior changed.
14. Remaining data quality risks.
15. Recommended next phase.
```
