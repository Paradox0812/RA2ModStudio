# Codex Task: RA2IniEditor.IDE AI-5A Stable Draft Output Contract

## 0. Context

DeepSeek live response has been tested. It can produce usable RA2 INI drafts, but output is unstable:

- Same requirement may produce different factions.
- INI blocks may include inline comments even when the user expects clean copyable INI.
- The model may include uncertain or hallucinated fields.
- Output structure varies between runs.

This task defines a stricter draft-output contract for the RA2 Modding Assistant.

This task is contract / planning first. Do not implement source changes in this task.

## 1. Goal

Make AI-generated RA2/YR/Ares/Phobos INI drafts more stable, reviewable, and safe.

The assistant should generate drafts in a predictable structure:

1. Assumptions
2. Clean INI draft blocks
3. Field rationale / evidence
4. Warnings / TODO
5. Suggested next steps

The generated INI block should be clean and copyable.

## 2. Required Product Rules

### 2.1 Faction / owner stability

If user does not specify faction, do not randomly choose Allied / Soviet / Yuri.

Preferred behavior:

1. Use current context if it clearly implies faction.
2. Otherwise write: `Assumption: faction not specified, using neutral placeholder Owner=<TODO_OWNER>.`
3. Use TODO placeholders rather than inventing a faction.

### 2.2 Clean INI block

Copyable INI blocks must not contain explanatory comments by default.

Explanations should be placed outside the INI block.

### 2.3 Separate files / sections

When output includes multiple file targets, separate them clearly:

- rulesmd.ini draft
- artmd.ini draft

Do not mix art fields into rules blocks unless explicitly explained.

### 2.4 Mark TODO references

If the draft references new IDs, mark required follow-up definitions:

- weapon
- warhead
- projectile
- art voxel / SHP / cameo assets

### 2.5 Field evidence

For each important field, include concise rationale outside the INI block.

### 2.6 Uncertain fields

If a field is not found in Field Registry evidence, the assistant must either omit it from the clean INI draft or include it only under "Optional / verify before use".

## 3. PromptBuilder Requirements

Update future PromptBuilder rules for draft-generation intent.

The prompt must instruct the model:

1. Answer in Chinese.
2. Output clean INI blocks without inline comments unless the user asks for annotated INI.
3. Put explanations outside code blocks.
4. Do not randomly choose faction/Owner if not specified.
5. Use TODO placeholders for unspecified owner/faction/art IDs.
6. Mark generated INI as draft.
7. Include assumptions and uncertainties.
8. Include field rationale grounded in Field Registry evidence.
9. Do not claim the draft has been applied to any file.

## 4. Recommended Output Template

For unit prototype generation:

```markdown
## 假设

- 阵营：未指定，使用 `<TODO_OWNER>` 占位。
- 定位：轻型防空车。
- 当前草案仅供复制和人工审查，不会自动写入文件。

## rulesmd.ini 草稿

```ini
[LAAV]
Name=Light Anti-Air Vehicle
UIName=Name:LAAV
Primary=LAAVMissile
Strength=200
Armor=light
Speed=10
Owner=<TODO_OWNER>
TechLevel=2
Cost=500
```

## 需要补充的定义

- [LAAVMissile]
- [LAAVMissileP]
- [LAAVMissileWH]
- artmd.ini 中 [LAAV] 图像定义

## 字段依据

- Strength：生命值 / 耐久度。
- Armor：装甲类型。
- Primary：主武器引用。
- Speed：移动速度。

## 注意事项

- `<TODO_OWNER>` 需要替换为实际阵营。
- 武器链需要根据项目已有 Warhead / Projectile 规则确认。
```

## 5. Future Implementation Split

Recommended:

- AI-5A: Stable draft output contract
- AI-5B: PromptBuilder draft-output template update
- AI-5C: AI panel copy-code-block UX
- AI-5D: Draft validation against Field Registry evidence

## 6. Tests to Plan

Future AI-5B tests should verify:

1. Prompt instructs no inline comments in copyable INI blocks.
2. Prompt asks for assumptions section.
3. Prompt asks for TODO placeholders when faction is missing.
4. Prompt asks for separate rules/art sections.
5. Prompt asks for field rationale outside code blocks.
6. Prompt forbids claiming changes were applied.
7. Prompt marks output as draft.

## 7. Final Report Format

Report:

1. Phase completed: AI-5A.
2. Files changed.
3. Draft stability rules added.
4. Output template decisions.
5. Tests planned.
6. Confirmation no source code changed.
7. Recommended next phase.
