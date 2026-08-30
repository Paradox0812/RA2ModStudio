# AGENT-KNOWLEDGE-1-R2 Rules/Art Binding Source Audit

更新时间：2026-08-25  
状态：Implemented / automated verification completed; real provider acceptance pending

## 1. 审计目标与可信范围

本阶段只证明内置 Agent 对 **TechnoType rules(md).ini ↔ art(md).ini 视觉绑定** 具有足够完整、
可追溯的知识。它不宣称单个 Prompt Skill 已覆盖所有 RA2/YR/Ares/Phobos 机制，也不把社区经验当成
游戏官方规范。

来源优先级：

1. 当前捕获项目和原版/实际运行 INI 数据；
2. ModEnc 对 YR 可读字段和逆向行为的专页；
3. Ares/Phobos 官方版本化文档；
4. Field Registry、Diagnostics 与其他社区样本只作辅助证据。

## 2. 采用的来源

| 来源 | 采用事实 | 限制 |
|---|---|---|
| 本地 `H:\RA2\YR_Test\rulesmd.ini` | `[HTNK]` 是既有 VehicleType 风格对象；当前未声明 `Image` 或 `ArtImageSwap` | `artmd.ini` 当前为空，不能单独证明资源绑定 |
| [ModEnc TechnoTypes](https://modenc.renegadeprojects.com/TechnoTypes) | rules Object ID 的 `Image` 指向 art Object Image；`Cameo/AltCameo/Voxel/Remapable` 属于 art | 社区维护、基于逆向与原版数据 |
| [ModEnc rules Image](https://modenc.renegadeprojects.com/Image/On_most_objects_in_rules%28md%29.ini) | rules `Image` 的值是 art Section；缺省为对象 ID | 不把所有扩展版本混为一谈 |
| [ModEnc engine file flow](https://modenc.renegadeprojects.com/How_The_Engine_Uses_Files) | Object → rules Image → art Section → Cameo/asset 的加载链 | 对特定家族例外需结合专页 |
| [ModEnc art Image](https://modenc.renegadeprojects.com/Image/On_Animations_and_other_objects_in_art%28md%29.ini) | vanilla art-side `Image` 主要适用于 Building/Animation；其他 Techno 不能视为通用重命名 | Phobos 有显式扩展例外 |
| [ModEnc Voxel](https://modenc.renegadeprojects.com/Voxel_%28INI_flag%29) | `Voxel=yes` 位于 art；Vehicle/Aircraft 是主要 VXL/HVA 家族 | 不证明具体素材文件存在 |
| [Phobos Customizable unit image](https://phobos.readthedocs.io/en/build-47/Fixed-or-Improved-Logics.html#customizable-unit-image-in-art) | `[General] ArtImageSwap=true` 才把 art-side `Image` 扩展到所有 TechnoTypes | 默认关闭；不得静默启用全局开关 |
| [Ares PCX Cameos](https://ares-developers.github.io/Ares-docs/new/pcxcameos.html) | `CameoPCX/AltCameoPCX` 是显式 Ares 扩展且保留 `.pcx` | 不替代普通 SHP `Cameo` |

未采用不存在的 EA RA2/YR 官方源码仓库作为证据。公开 EA GitHub 当前没有可访问的
`electronicarts/CnC_Red_Alert_2` 仓库。

## 3. 已确认的模型知识

### 3.1 标准引用图

```text
rules [OwnerSection] Image=ArtSection
                     |
                     v
art   [ArtSection] Cameo=CameoAsset
                   AltCameo=AltCameoAsset (仅明确要求时)
```

- 用户提示中的 `Art`、`Body`、`Cameo` 是角色名称；普通 Techno 不存在由这三个标签直接展开成
  rules `Art=/Body=/Cameo=` 的规则。
- 既有 owner 不重复注册；新 owner 只加入一个正确类型列表，且不得重编号。art Section 无数字注册表。
- 简单跨文档绑定至少应按字段所有权分成 rules 与 art 两个 document plan，不能退化成 rules-only。

### 3.2 Body 与对象家族

- Infantry/Vehicle/Aircraft 在 vanilla YR 中通常以 art Section ID 作为主资源 basename；不能普遍用
  art-side `Image=DifferentBody` 改名。
- Building/Animation 支持 art-side `Image` 资源覆盖。
- Phobos 只有在项目事实已建立 `ArtImageSwap=true` 时，才可把该行为扩展到其他 Techno。
- VXL/HVA Vehicle/Aircraft 使用 art `Voxel=yes`；SHP、Sequence、palette、Remapable、FLH、turret/barrel、
  facings 与 damage/buildup animation 都是独立维度，不能因“绑定美术”一次性臆造。

### 3.3 不确定性处理

当用户给出不同的 ArtSection 与 BodyAsset，但对象家族/运行时开关无法从输入上下文确定时，模型必须
返回一个具体 clarification；不得写字面 `Art/Body` 字段，也不得静默打开全局 `ArtImageSwap`。

## 4. 现有 Skill 体系复核

- 原 15 个 Skill 的领域划分仍有效，但 `ExactDomainPrimary + FieldTrust` 对跨域 rules/art 请求不足。
- `ra2-art-animation` 只说明概念分离，没有冻结 Object → Image → art Section → asset/cameo 的精确图。
- `ra2-field-schema-trust@1` 把 Registry 写成全局 authoring authority，与 NF6 的 model-owned project
  路由冲突；v2 已改为 typed current-document Profile 严格、generic project advisory。
- 新增 `ra2-rules-art-binding@1` 后，Project Work 无论第一阶段把 domain 归一成什么，都由
  PromptBuilder 按 capability 显式选择该 Skill；不会再偶然退回 `ra2-art-animation`。
- Skill 仍只提供知识，不增加 tool、路径、Apply、Save、网络、Shell 或素材权限。

## 5. 尚未宣称的能力

- 没有把全部 Ares/Phobos 字段、所有 Building animation、Infantry Sequence、Voxel turret/barrel 组合
  塞入一个常驻 Skill；这些应按具体请求和版本继续渐进披露。
- 模型执行阶段目前没有收到完整 project semantic model；若当前 IDE context 不含对象家族或
  `ArtImageSwap` 事实，正确行为是 clarification，而非猜测。
- 自动测试能证明正确知识进入真实第二阶段 prompt，不能证明 DeepSeek 每次都遵循它；真实 provider
  结果仍需手工验收 Project Diff。

## 6. 验收提示词

```text
给 HTNK 绑定美术：Art=HTNKART，Body=HTNKBODY，Cameo=HTNKICON。
```

禁止结果：rules `[HTNK] Art=/Body=/Cameo=`。  
允许结果：按已知 runtime 形成正确 rules/art 两文档计划；或在不同 Art/Body 映射缺少
`ArtImageSwap`/对象家族事实时，返回点明缺失事实的 clarification。

## 7. 自动验证

- Debug build 最终复跑：0 errors，0 warnings。
- Skill/catalog/prompt/pipeline/project focused：84/84。
- Application full：188/188。
- IDE full：2660/2660。
- `skill-creator` 的 `quick_validate.py`：NotRun；本机 Python 3.11/3.14 均缺少脚本所需 PyYAML，未安装
  新依赖。等价 frontmatter/name/count/content/no-scripts/hash 验证已由 production
  `Ra2AgentSkillCatalog.LoadBundled` 与测试执行。
- 真实 DeepSeek/WPF：NotRun；需要用户在新进程中按第 6 节复验。
