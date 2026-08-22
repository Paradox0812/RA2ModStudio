# Codex Task Addendum: RA2IniEditor.IDE AI-5B No Hallucinated Fields Constraint

## 0. Context

User feedback for AI-5B:

```text
应该强化对不能使用字段库中没有的虚构字段的约束。
```

This addendum strengthens the stable draft PromptBuilder update.

The AI assistant may generate new object IDs, weapon IDs, warhead IDs, projectile IDs, and art IDs as draft references, but it must not invent **field keys** that are not supported by the current Field Registry evidence.

---

## 1. Core Rule

For clean copyable INI draft blocks:

```text
Do not use field keys that are not present in Field Registry evidence.
```

If a field key is not present in the provided evidence:

```text
1. Do not include it in the clean INI block by default.
2. Put it under "可选 / 使用前需验证" if it may be useful but not verified.
3. Clearly mark it as not confirmed by Field Registry evidence.
```

---

## 2. Important Distinction

The model must distinguish:

```text
Field keys
Object IDs / values
```

### 2.1 Field keys

Examples:

```ini
Strength=200
Armor=light
Primary=LAAVMissile
Speed=10
```

These are field keys:

```text
Strength
Armor
Primary
Speed
```

Field keys should be grounded in Field Registry evidence.

### 2.2 Object IDs / values

Examples:

```ini
Primary=LAAVMissile
Projectile=LAAVMissileP
Warhead=LAAVMissileWH
Owner=<TODO_OWNER>
```

These are values / references:

```text
LAAVMissile
LAAVMissileP
LAAVMissileWH
<TODO_OWNER>
```

The model may create new object IDs as draft references, but must list them under follow-up definitions / TODO.

---

## 3. PromptBuilder Requirement

AI-5B must update `Ra2AiPromptBuilder` to include a rule equivalent to:

```text
For clean copyable INI blocks, only use field keys that appear in Field Registry Evidence.
If you want to use a field key not found in evidence, do not put it in the clean draft. Instead, list it under "可选 / 使用前需验证" and explain that it was not confirmed by Field Registry evidence.
You may create new object IDs as values, such as weapon/warhead/projectile IDs, but every new referenced ID must be listed under "需要补充的定义".
```

---

## 4. Output Template Addition

Generated draft output should include:

```markdown
## 可选 / 使用前需验证

- SomeField：未在当前 Field Registry Evidence 中确认；如果项目/Ares/Phobos 支持，请手动验证后再加入。
```

If there are no unverified fields:

```markdown
## 可选 / 使用前需验证

- 无。
```

---

## 5. Tests to Add / Update

AI-5B tests must verify that the prompt contains rules for:

```text
1. Clean INI blocks should only use field keys from Field Registry evidence.
2. Unverified field keys must not be placed in clean draft blocks by default.
3. Unverified field keys should be placed under "可选 / 使用前需验证".
4. Newly invented object IDs are allowed as values only if listed under "需要补充的定义".
5. The prompt distinguishes field keys from object IDs / values.
```

Avoid full prompt equality assertions. Use stable substring / section assertions.

---

## 6. Non-goals

Do not implement actual post-generation validation in this phase.

This addendum only strengthens the prompt contract.

Future phase may add:

```text
AI-5D: Draft validation against Field Registry evidence
```

which can scan generated INI drafts and flag unknown keys before copy/insert.
