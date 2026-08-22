# Codex Task: RA2IniEditor.IDE AI-6F Subject-aware Field Evidence Expansion

## 0. Context

Manual smoke after AI-6D shows that conversation/current subject is now working, but the Field Registry evidence package is still too narrow.

Observed behavior:

```text
User follow-up: 将刚刚的单位修改为盟军背景
AI understands the previous unit, but still says:
- Field Registry evidence is limited
- some fields such as Owner / Primary are marked as unverified
```

This means:

```text
Conversation Context / Current Subject works.
But Field Evidence Retrieval does not yet use CurrentSubject / previous draft / follow-up intent deeply enough.
```

This task improves evidence retrieval. It does not change DeepSeek adapter behavior or PromptBuilder safety rules.

---

## 1. Goal

Expand Field Registry evidence retrieval for follow-up draft editing requests by using:

```text
1. current user prompt
2. current IDE context
3. current subject from AI-6C
4. recent conversation draft metadata
5. previous assistant draft field keys
6. intent/profile seed keys
```

The goal is to provide enough field evidence for common follow-up requests such as:

```text
把这个单位改成盟军单位
改成苏军单位
加上对空武器
让它部署成防空炮
让它能载人
让它隐形侦察
```

without sending the whole registry.

---

## 2. Problem Statement

Current retrieval is likely too narrow because it mainly uses:

```text
current caret key
current section
selected text
current user prompt keywords
basic seed profiles
```

But follow-up requests depend on prior AI draft context:

```text
"这个单位" refers to prior [LAAV].
"改成盟军背景" implies Owner / RequiredHouses / ForbiddenHouses / Prerequisite / UIName / Image context.
"在这个基础上" implies previous draft field keys should be preserved as evidence.
```

Therefore, field evidence should include:

```text
1. fields already used in prior assistant draft
2. fields related to the requested modification
3. profile fields for the current subject kind
```

---

## 3. Hard Boundaries

Do not implement or change:

```text
DeepSeek adapter behavior
provider selection behavior
API key loading
PromptBuilder no-hallucinated-fields safety rules
Apply / Insert
file modification
Field Registry writes
whole-project context
unbounded chat history
cross-session memory
hidden memory
settings persistence
auto-send context
diagnostic auto-fix
streaming output
retry loops
```

Do not modify:

```text
Field Registry loader / writer / apply / rollback / import / learning services
diagnostics behavior
parser semantics
completion / hover / quick peek behavior
save preflight
BuiltIn Field Registry JSON
legacy files
solution / project files
```

Evidence remains advisory and bounded.

---

## 4. Required Design

### 4.1 Expand evidence request inputs

Current evidence retrieval should accept optional:

```text
CurrentSubject
ConversationContext
PreviousDraftFieldKeys
FollowUpIntent
```

Suggested extension options:

```text
Option A:
  Extend Ra2AiContextRequest with CurrentSubject / ConversationContext.

Option B:
  Add Ra2AiEvidenceRequest used by Ra2FieldRegistryAiEvidenceProvider.

Option C:
  Keep current context request stable and add a small subject-aware evidence helper invoked before PromptBuilder.
```

Prefer the smallest implementation that avoids ShellWindow.xaml.cs bloat.

### 4.2 Extract previous draft field keys

From recent assistant draft/code blocks, extract field keys used in clean INI blocks:

```ini
Strength=200
Armor=light
Primary=LAAVMissile
Owner=<TODO_OWNER>
```

Extract keys:

```text
Strength
Armor
Primary
Owner
```

Then confirm them through active Field Registry provider before returning evidence.

Do not assume unconfirmed keys are valid evidence.

### 4.3 Subject-kind profile

If CurrentSubject.Kind = Unit, include UnitCore / VehicleCore profiles.

If CurrentSubject.Kind = Weapon, include WeaponCore / Projectile / Warhead context as needed.

For follow-up "这个单位", the subject kind should guide profile selection.

### 4.4 Follow-up intent profile

Map common follow-up intents to seed keys:

#### Faction / owner change

Keywords:

```text
盟军
苏军
尤里
阵营
国家
所属
Owner
RequiredHouses
ForbiddenHouses
```

Seed keys:

```text
Owner
RequiredHouses
ForbiddenHouses
Prerequisite
UIName
Name
Image
```

#### Weapon / anti-air change

Keywords:

```text
防空
对空
飞机
空军
AA
导弹
武器
```

Seed keys:

```text
Primary
Secondary
ElitePrimary
EliteSecondary
Damage
ROF
Range
Projectile
Warhead
AA
AG
Verses
```

#### Deploy / transform

Keywords:

```text
部署
展开
变形
deploy
```

Seed keys:

```text
DeploysInto
UndeploysInto
DeployToFire
DeployFire
IsSimpleDeployer
Deployer
DeployTime
```

#### Transport

Keywords:

```text
运输
载人
乘客
Passengers
```

Seed keys:

```text
Passengers
PipScale
OpenTopped
SizeLimit
```

#### Stealth / scout

Keywords:

```text
隐形
侦察
潜行
探测
雷达
```

Seed keys:

```text
Cloakable
CloakingSpeed
Sensors
SensorsSight
DetectDisguise
Sight
Speed
```

Only return confirmed fields.

---

## 5. Evidence Budget

Increase evidence budget carefully.

Suggested:

```text
Top evidence returned to PromptBuilder: 16
Hard cap: 24
```

Prioritize:

```text
1. exact current key
2. fields used in current subject / prior draft
3. follow-up intent seed fields
4. subject kind profile fields
5. generic draft profile fields
```

Do not include the entire registry.

---

## 6. PromptBuilder Safety Must Remain

Do not weaken:

```text
clean INI blocks should use evidence-backed field keys
unverified fields go to "可选 / 使用前需验证"
object IDs / values may be newly generated but must be listed in follow-up definitions
prior draft is conversation draft, not applied file state
```

If evidence is still insufficient, the model should say which fields are unverified rather than hallucinating.

---

## 7. Tests

Required tests:

```text
1. Follow-up prompt "改成盟军单位" retrieves Owner / RequiredHouses / ForbiddenHouses when provider confirms them.
2. Follow-up prompt "改成苏军单位" retrieves faction/owner-related fields.
3. Previous assistant draft field keys are extracted and confirmed as evidence.
4. CurrentSubject.Kind=Unit triggers UnitCore / VehicleCore evidence profiles.
5. Anti-air follow-up retrieves Primary / Projectile / Warhead / AA / AG evidence when available.
6. Unconfirmed seed keys are not returned.
7. Evidence count remains bounded.
8. No provider reload / file IO / registry mutation occurs.
9. Existing exact key lookup behavior remains.
10. PromptBuilder no-hallucinated-field tests remain valid.
```

---

## 8. Manual Smoke Checklist

After implementation:

```text
1. Ask AI to design a light AA vehicle.
2. Ask: 将刚刚的单位修改为盟军背景.
3. Confirm the model recognizes the prior unit.
4. Confirm Field Registry evidence no longer says all fields are generic/unverified when common fields exist.
5. Confirm Owner / RequiredHouses / ForbiddenHouses or equivalent owner-related fields are evidenced if present.
6. Confirm unverified fields are still placed under optional/verify.
7. Confirm no editor text changes and no dirty state.
```

---

## 9. Validation Commands

Run full validation:

```powershell
dotnet restore .\RA2IniEditor.IDE.sln
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
dotnet test .\RA2IniEditor.Tests\RA2IniEditor.Tests.csproj -c Debug --no-build
powershell -ExecutionPolicy Bypass -File .\tools\package-source-clean.ps1 -Profile IdeOnly
```

---

## 10. Acceptance Criteria

Accepted when:

```text
1. Subject-aware follow-up requests produce broader but bounded evidence.
2. Prior draft field keys can seed evidence retrieval.
3. Faction/owner follow-up requests include relevant evidence when provider confirms fields.
4. No-hallucinated-fields rule remains intact.
5. No DeepSeek/provider/UI/file-write behavior is changed.
```
