# AGENT-KNOWLEDGE-1 RA2 Logic and Skill Source Audit

更新时间：2026-08-23  
状态：Completed / source inventory frozen

## 1. 搜索范围

本次只使用公开规范、扩展官方文档、ModEnc 和项目已经核验的 Field Registry/阶段文档。未把论坛片段、
模型记忆或示例 INI 当成字段事实源。

| 来源 | 用途 | 结论 |
|---|---|---|
| Agent Skills specification | Skill 包格式、name/description、渐进披露、references | 采用 `SKILL.md` 兼容包；v1 不允许 scripts |
| DeepSeek Harness skills subsystem | registry/provider/consumer 分离、按需加载、scope/shadow | 借鉴架构，不引入 Harness/Cordis/Node 依赖 |
| MCP prompts/resources/tools | 区分指令、资源与可执行工具，能力协商和输入验证 | Skill 不授予工具；未来 Host capability 另行协商 |
| ModEnc Projectile/Warhead 及项目 source verification docs | 原版/YR 对象链与字段上下文 | 武器、弹体、弹头、Techno 等按引用闭包拆 Skill |
| Ares 3.0 documentation | Side/Country、SuperWeapon、Weapon/Warhead、Animation 等扩展逻辑 | 需要 version-aware cross-cutting Skill |
| Phobos documentation/GitHub docs | custom trajectories、扩展/替代逻辑 | 不得把 Phobos Trajectory 与 vanilla trajectory 字段随意混用 |
| BuiltIn v3.2 Field Registry + `Ra2SectionKind` | 项目实际覆盖面和 trust | Skill 引导工作流，字段事实仍由 captured registry 决定 |

主要入口：

- <https://agentskills.io/specification>
- <https://github.com/deepseek-ai/deepseek-harness/blob/master/docs/subsystems/skills.md>
- <https://modelcontextprotocol.io/specification/2025-06-18/server/prompts>
- <https://modelcontextprotocol.io/specification/2025-06-18/server/resources>
- <https://modelcontextprotocol.io/specification/2025-06-18/server/tools>
- <https://modenc.renegadeprojects.com/Projectile>
- <https://modenc.renegadeprojects.com/Warhead>
- <https://ares-developers.github.io/Ares-docs/>
- <https://github.com/Phobos-developers/Phobos/blob/develop/docs/New-or-Enhanced-Logics.md>

## 2. RA2 逻辑分区结果

### 已有 typed/current-document 能力可直接支撑

- INI document、Section/field/reference query；
- Field Schema/trust；
- 单字段 Upsert/Replace Preview；
- 新 Section Preview；
- Weapon/Projectile/Warhead skeleton；
- direct-fire complete weapon-chain profile；
- 主工作区 Diff、显式 Apply、不自动 Save。

### 有知识与字段证据，但还缺完整 typed Profile

- Techno 子类完整对象；
- Projectile trajectory 与 Warhead special behavior；
- TaskForce/ScriptType/TeamType/AITriggerType；
- SuperWeapon + provider building/effect closure；
- Side/Country/faction；
- art/animation、particle/radiation、terrain/resource、sound/EVA；
- type-list registration 与跨文件 reference closure。

这些 Skill 当前可改进 Chat 解释和设计质量，但 Work 必须对未实现 complete profile fail closed。

### 需要未来可执行 capability/plugin

- Cameo/Icon 生成与调色板/尺寸验证；
- VOX 生成、切片、VXLSE III 导入包/VXL/HVA 验证；
- SHP 动画、帧序列、palette、编码；
- Asset Assembly Graph，把产物绑定到 rules/art 并生成可审阅 Artifact。

## 3. 关键架构结论

1. Skill = 版本化程序性知识；Field Registry = 字段事实；Content Profile = 对象完整度；Capability = 可执行动作；Host = 权限和生命周期。
2. v1 只加载应用随包 BuiltIn Skill；不扫描项目、用户或网络目录，不支持 shadow、热重载和 scripts。
3. `allowed-tools` 不作为授权来源。有效能力永远是 Mode policy、Host policy、local capability 和 snapshot
   availability 的交集。
4. 首次目录只注入匹配领域的一个主 Skill、显式 Ares/Phobos 时的一个兼容 Skill，以及 Field trust Skill；
   总指令预算 14 KiB，超限不加载尾部 Skill。
5. Skill 内容 hash/version 随请求可追踪，但 v1 不持久化 provider reasoning 或 raw secret-bearing payload。

