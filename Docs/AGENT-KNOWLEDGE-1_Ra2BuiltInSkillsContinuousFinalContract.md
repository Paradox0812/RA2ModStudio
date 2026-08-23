# AGENT-KNOWLEDGE-1 RA2 Built-in Skills Continuous Final Contract

更新时间：2026-08-23  
状态：Self-approved / implemented; final verification in stage ledger  
前置：`AGENT-MODE-1 Chat / Work Mode Continuous Final Contract`

## 1. 目标

在 Chat/Work 权限分离之后，为内置 Agent 增加可版本化、按领域选择、可审计的 RA2 知识层。它提升意图
理解、对象依赖闭包和完整度判断，但不增加文件权限，不替代 Field Registry，不创建第二 Preview/Apply。

## 2. 允许与禁止

允许：IDE internal catalog/loader/resolver、Prompt 注入、只读 BuiltIn `SKILL.md`、测试、AI panel Mode UI、
首个 complete direct-fire Content Profile、Experimental descriptor 的 OutputKind additive 元数据。

禁止：外部 Skill 安装/下载、scripts、热重载/shadow、动态 plugin、Host/CLI/wire、自动 Apply/Save、跨文件
事务、字段库数据修改、DeepSeek transport/retry、legacy、全局 Dock 重构。

## 3. 数据与权限契约

```text
Ra2AgentSkillDescriptor
  Name / Description / Version / Domains / Modes
  Instructions / SHA-256 ContentHash

Selection = ExactDomainPrimary
          + ExplicitAresPhobosCompatibility (optional)
          + FieldSchemaTrust (cross-cutting)

EffectiveCapability = HostPolicy
                    ∩ UserModePolicy
                    ∩ DeclaredLocalCapability
                    ∩ SnapshotAvailability
```

- catalog/descriptor/resolver 全部 IDE internal；不形成 wire/public API；
- v1 root 固定为 output `AgentSkills/<kebab-name>/SKILL.md`；最多 64 包、单包 96 Ki chars；
- name 必须与目录一致；description <= 1024；必须声明 version/domains/modes；
- 禁止 `scripts/`；无目录遍历、符号路径跟随、网络加载或自修改；
- catalog 在 PromptBuilder 创建时捕获，单请求使用 immutable selection；
- 最多加载 3 个 Skill，总 instruction <= 14 KiB；稳定排序；缺包降级为空 catalog，不授予能力；坏包 fail fast；
- Skill 文本进入 application instruction 区，并显式声明“不授予 tool/file/apply/save/network/shell 权限”。

## 4. 已实现 Skill 清单

| Skill | 领域 | Chat | Work |
|---|---|---:|---:|
| `ra2-ini-document` | INI 文档/Section/最小修改 | 是 | 仅现有 typed 能力 |
| `ra2-field-schema-trust` | 字段类型、上下文、来源、trust | 是 | cross-cutting gate |
| `ra2-reference-registration` | 引用闭包/类型列表/索引 | 是 | 未有 Profile 时 advisory |
| `ra2-techno-authoring` | Infantry/Vehicle/Aircraft/Building | 是 | 完整对象 fail closed |
| `ra2-weapon-chain` | Host->Weapon->Projectile->Warhead | 是 | direct-fire complete + skeleton |
| `ra2-projectile-trajectory` | vanilla/Ares/Phobos 弹道 | 是 | 完整 Profile 待后续 |
| `ra2-warhead-damage` | Verses/Armor/范围/特效 | 是 | 完整 Profile 待后续 |
| `ra2-ai-programming` | TaskForce/Script/Team/AITrigger | 是 | tuple Profile 待后续 |
| `ra2-superweapon-authoring` | 超武/provider/effect/target | 是 | type Profile 待后续 |
| `ra2-faction-authoring` | Side/Country/House/ownership | 是 | multi-file Profile 待后续 |
| `ra2-art-animation` | art/animation/SHP/VXL binding | 是 | asset capability 待后续 |
| `ra2-particle-radiation` | Particle/System/Radiation | 是 | Profile 待后续 |
| `ra2-terrain-resource` | Terrain/Overlay/Smudge/Tiberium | 是 | map/project capability 待后续 |
| `ra2-sound-eva` | Sound/Voice/EVA/asset binding | 是 | multi-file/artifact 待后续 |
| `ra2-ares-phobos-extensions` | 版本/冲突/替代规则 | 是 | cross-cutting gate |

## 5. Chat / Work 与完整度

- Chat 永远零 authoring tool，可解释、列依赖、输出明确标记的草稿；
- Work 只有精确字段修改、显式 skeleton 或已注册 complete profile 才获得唯一 required tool；
- 普通“搭建/构建可用武器链”走 complete profile；只有骨架/框架/占位/空结构等显式词才走 skeleton；
- Unit/Building/SuperWeapon/AI/asset 等未有 typed complete profile 的 Work 请求在模型调用前本地拒绝；
- complete direct-fire profile 绑定唯一现有 TechnoType，并生成三段非空引用闭合 Section；整体 Preview、
  整体失败、整体 Apply，不自动 Save。

## 6. 连续阶段与门禁

### KNOWLEDGE-1A — Source/Standard Audit

完成外部规范、ModEnc/Ares/Phobos、本地 Field Registry/SectionKind/Capability 的单次来源审计。

### KNOWLEDGE-1B — BuiltIn Package Contract

实现 Agent Skills-compatible 目录、frontmatter 校验、size/count/name/mode/domain/hash 门禁；v1 禁脚本与外部根。

### KNOWLEDGE-1C — Resolver and Prompt Composition

稳定领域路由、显式扩展兼容层、Field trust cross-cutting、预算和 authority-neutral Prompt 注入。

### KNOWLEDGE-1D — Seed Domain Pack

实现上表 15 个 Skill；每个 Skill 只包含影响决策的 RA2 工作流，不复制字段大全。

### MODE-1C — Complete Direct-fire Profile

新增 `CompleteObject` descriptor、RequireExisting owner target、15 个参数、15 个原子 operation；保留 skeleton v1 原义。

### KNOWLEDGE-1E — Verification/Docs

Skill validator、loader/resolver/prompt、Mode router、template/compiler、public API、Shell boundary、全量测试和 clean package。

## 7. 测试矩阵

- 包：15 个目录、唯一 kebab name、必需 frontmatter、无 scripts、hash 稳定、坏包 fail fast；
- 选择：weapon 主 Skill + trust；Phobos trajectory 主 Skill + extension + trust；预算不超；
- 模式：Chat 修改请求零工具；Work 普通武器链 complete；显式骨架 skeleton；unsupported 不发送；
- 完整链：owner 唯一、kind compatible、3 新 Section、15 operations、11 Verses token、trust/schema gate；
- 原子性：缺参、重复/未知参数、Section 冲突、owner 缺失/重复/错 kind、blocked field、stale/cancel/limit 均无 partial plan；
- UI：4 个新 AutomationId、默认 Chat、请求/提案期间不可切换、Dock topology 不变；
- API：Application exported allowlist 59；只新增 `Ra2AutomationTemplateOutputKind`，其余 Skill/Mode internal。

## 8. 自审与批准

风险级别：实现 R3；文档 R0。用户已明确授权自审后连续执行。

自审通过理由：

1. 复用现有 PromptBuilder -> ToolCatalog -> Gateway -> Preview -> Coordinator -> Host Apply；
2. Skill、Field Registry、Content Profile、Capability/Host 权威没有混层；
3. v1 不引入第三方运行时、依赖、外部包或可执行脚本；
4. 未支持完整对象 fail closed，避免“空 Section 冒充完成”；
5. Skill 与 Mode internal-first；唯一 public diff 有明确 consumer 和机器可读完整度用途；
6. 用户现有 skeleton 保持兼容；普通武器链修正为 complete，不再误路由。

## 9. 后续 Skill 与 Capability 路线

按“先 typed profile，再开放 Work”的顺序：

1. `CONTENT-2A Techno Complete Profiles`；
2. `CONTENT-2B Projectile/Warhead Profiles`；
3. `CONTENT-2C AI Programming Tuple Profiles`；
4. `CONTENT-2D SuperWeapon/Faction/Multi-file Profiles`；
5. `HOST-1 Independent Agent Host`；
6. `ASSET-SKILL-1`：`ra2-cameo-icon`、`ra2-voxel-model`、`ra2-shp-animation`、`ra2-asset-assembly`，
   每个必须依赖独立 capability/plugin、Artifact Preview 和 Host permission，不能只有 Prompt。

