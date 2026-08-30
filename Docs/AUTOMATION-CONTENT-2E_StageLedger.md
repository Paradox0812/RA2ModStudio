# CONTENT-2E SuperWeapon / Support Power Complete Profiles Stage Ledger

状态：**Completed / automated verified**  
日期：2026-08-25  
风险：R3  
连续阶段：`2E-0 -> 2E-5`

## 1. 交付结论

Work 模式现在可以在捕获项目的唯一 `rulesmd.ini` 或 `rules.ini` 中生成并预览两类
source-backed Ares 完整对象：

- `ares-unitdelivery-superweapon-complete`：注册、provider/AlwaysGranted、公共超武字段和
  `Deliver.Types` / `Deliver.Owner` 闭包；
- `ares-genericwarhead-superweapon-complete`：注册、provider/AlwaysGranted、公共超武字段和
  `SW.Damage` / 既存 `SW.Warhead` 引用闭包。

其它明确 SuperWeapon 类型进入既有 model-owned project plan；Field Registry 与 Diagnostics 只提供
审阅证据，不拥有内容否决权。所有成功路径仍进入既有 Project Preview / Diff / 显式 Apply / compound
Undo，不自动 Apply、Save，也不创建或写入素材。

## 2. Stage Result Ledger

| Stage | 结果 | 主要产物 | 自审 |
|---|---|---|---|
| 2E-0 | Completed | 官方来源能力矩阵；冻结 UnitDelivery、GenericWarhead、公共字段、targeting 与 provider 语义 | 通过；未把缺乏通用默认值的 `Action` 伪造为默认值 |
| 2E-1 | Completed | internal immutable profile catalog；rules-only 项目捕获；现有模板编译器复用 | 通过；未增加 public API、持久化或第二 Preview 权威 |
| 2E-2 | Completed | UnitDelivery typed complete profile、引用/所有权/targeting/provider 负例 | 通过；只引用既有 Techno，不暗中新建依赖 |
| 2E-3 | Completed | GenericWarhead typed complete profile、既存 Warhead 引用与不修改依赖证据 | 通过；不新建空 Warhead，不修改已有 Warhead |
| 2E-4 | Completed | Work intent/tool/prompt/adapter 接线；generic fallback；两个 Ares/Phobos Skill | 通过；Chat、正常 Work 两调用与一次 repair 上限不变 |
| 2E-5 | Completed | 全量测试、构建、clean package、契约/能力/决策/API 文档收口 | 通过；真实 provider/WPF/游戏内行为保留为人工验收 |

## 3. Diff Intent Table

| 变更区域 | 意图 | 明确未改变 |
|---|---|---|
| Application template compiler/catalog | 增加两个确定性 SuperWeapon profile，并允许 source-bounded profile 字段绕过陈旧 Registry Enum 否决 | parser、Registry 数据/优先级、Diagnostics、Completion |
| IDE Work routing/tool adapter | 将 typed profile 或 generic SuperWeapon 计划送入既有 Project Preview | 任意路径、自动 Apply/Save、资产写入 |
| Shell 项目快照捕获 | 经批准允许唯一 rules 目标；匹配 art 存在时一并捕获 | XAML、Dock、布局、AutomationId、项目文件枚举权威 |
| BuiltIn Skills | 增加 Ares typed 与 Phobos extension 知识，更新核心 SuperWeapon Skill | capability、权限、脚本、外部 Skill root |
| Tests/docs | 冻结成功/失败、事务复用和边界证据 | legacy 与无关产品行为 |

## 4. Public API / Decision / Debt Flush

- Public API：新增 **0** 个 exported type、method、enum 或持久化字段。模板目录数据增加两个 descriptor；
  provider-visible intent capability ID 为 IDE-internal Experimental additive change。
- Decision：接受“两个 typed common profiles + model-owned generic fallback”；typed document plan 在 IDE
  内包装为既有 Project Plan，避免伪造 `AssetManifest` 或改变 public project-template result invariant。
- Shell exception：仅 `ShellWindow.xaml.cs` 的项目快照捕获允许 rules-only；由用户明确批准。
- Technical debt：本阶段未引入实现捷径。未覆盖的 SuperWeapon 类型、真实 DeepSeek 参数服从度、模组版本
  差异和游戏内行为属于显式能力边界/验证风险，不伪记为已完成。

## 5. Verification Matrix

| Gate | 结果 |
|---|---|
| Application focused `--filter SuperWeapon` | 8/8 passed |
| IDE focused `--filter SuperWeapon` | 14/14 passed |
| IDE related project/Skill suite | 79/79 passed |
| Application full | 196/196 passed |
| IDE full | 2722/2722 passed；0 failed / 0 skipped |
| Debug build | Passed；0 warnings / 0 errors |
| IdeOnly clean package | Passed；1241 files；排除 `.vs/bin/obj/artifacts/TestResults/old zip` |
| Real DeepSeek / WPF / game runtime | **Not run / manual acceptance required** |

## 6. 人工验收边界

按最终契约中的 UnitDelivery 与 GenericWarhead 用例执行。应确认：

1. Project Diff 只修改 rules；art 不是前置条件；
2. `应用到项目` 只修改内存态且一次 Undo 可恢复；
3. 不自动保存、不创建素材；
4. 通用类型在事实不足时澄清，在事实充分时产生可审阅计划，而不是被 Registry Enum 拒绝。

## 7. Stop Confirmation

所有必选自动化门禁已通过；未触发 public API、持久化、parser、Registry 数据、XAML 或写权限扩大。
`CONTENT-2E` 可以在“两个 typed profile + generic fallback”的准确范围内关闭，不能宣称所有
SuperWeapon 类型均已 typed-certified 或已通过游戏内验证。

## 8. CONTENT-2E-FIX1 Project Context Selection（2026-08-25）

- 真实 Work 验收发现：第一轮已正确选择 UnitDelivery 项目能力，第二轮也正确调用
  `expand_ini_project_content_template`，但 bounded replan 的上下文选择器仍只把旧的
  `ProjectRulesArtBindingPreview` 识别为项目作用域，导致 typed SuperWeapon 工具收到当前文档上下文并被
  Host 正确拒绝为“单文档请求不能调用项目内容模板工具”。
- 修复将项目作用域分类收敛到 `Ra2AiAuthoringToolCatalog.UsesProjectContext`；PromptBuilder 与 bounded
  replan coordinator 共同复用该判定。四种项目能力现在统一选择请求期捕获的 Project Context。
- 保留 adapter 的单文档/项目工具隔离，不扩大路径、Apply、Save、素材、Shell 或 provider 权限。
- 新增工具类别/作用域一致性测试，以及三种 SuperWeapon 项目模式的上下文选择回归测试。
- 自动证据：Release focused 19/19、IDE full 2726/2726、Release build 0 warnings / 0 errors、
  IdeOnly clean package 1241 files。Debug focused 首次运行被正在运行的 IDE/Visual Studio 锁定输出 DLL，
  未强制终止用户进程；真实 DeepSeek 复测仍由用户执行。

## 9. CONTENT-2E-FIX2 Natural Object Identity Resolution（2026-08-25）

- 真实自然语言验收暴露了两个连续问题：第一轮把已选择的 SuperWeapon 能力附带为不一致的 domain/
  completion 元数据，先被 Work 契约拒绝；通过后，执行轮又可能把“盟军发电厂 / GI / IFV”等显示名称
  当成 Section ID，导致既有对象查询失败。
- Intent normalization 现在对三个 SuperWeapon 项目能力统一固定 `superweapon + Complete`，并要求第一轮
  把自然/显示名称推断为 canonical Section 候选后，通过原有 `get_section` 查询验证。
- typed profile 增加请求期防御性身份规范化：只在捕获 rules 语义模型中，对所需 Building/Techno/Warhead
  类型的 Section、`Name`、`UIName` 做精确且唯一匹配；不做模糊匹配，不硬编码 GAPOWR/E1/FV，不缓存或持久化。
  规范化后仍继续经过原有类型、引用、字段和 canonical Preview 门禁；多义或缺失对象仍被拒绝。
- 新增 intent 元数据、候选查询提示、唯一/多义别名、adapter 与完整两轮自然语言 pipeline 回归。端到端用例
  模拟第一轮元数据漂移、GAPOWR/E1/FV 查询成功、第二轮返回显示别名，最终生成 typed Project Proposal，
  无 repair、无 Apply/Save。
- 自动证据：Application focused 10/10、IDE related 61/61、SuperWeapon integration 18/18、Application full
  198/198、IDE full 2733/2733、Release build 0 errors / 1 个既有 nullable warning、IdeOnly clean
  package 1241 files。真实 DeepSeek/WPF/游戏内行为仍由用户验收。
