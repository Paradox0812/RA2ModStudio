# CONTENT-2D-0/1 — Object Closure and Current-Document Registration Final Contract

更新时间：2026-08-24  
状态：Implemented / automated verification passed

## 1. 目标

在不新增写入通道、不修改 Field Registry、不触及多文件事务的前提下，为 Content Template
增加可复用的对象注册声明和确定性数字索引分配能力。该能力是后续完整 Techno、SuperWeapon、
AI tuple 和 rules/art 绑定的基础，但本阶段不新增任何面向 AI 的 complete profile。

## 2. 对象闭包边界

完整对象由模板定义拥有，闭包由既有 Section/Field 声明和新增 Registration 声明共同表达：

```text
Template Definition
├─ Parameters                 对象 ID 与字段参数
├─ Sections                   创建/要求存在的对象及命名字段
├─ Registrations              显式类型列表入口
└─ Existing Field References  ReferenceReachable 依赖边
```

注册策略分型：

| 策略 | 语义 | 本阶段运行时支持 |
|---|---|---|
| `ExplicitNumberedList` | `[VehicleTypes]`、`[SuperWeaponTypes]` 等数字列表 | 是，当前文档 |
| `ReferenceReachable` | Weapon/Projectile/Warhead 等由有效引用到达 | 沿用既有字段引用，不生成伪注册项 |
| `StructuredTuple` | TaskForce/Script/AITrigger 等有序元组 | 仅保留后续边界，不实现 |
| `CrossFileArtifact` | rules/art/素材依赖 | 仅保留后续边界，不实现 |

Field Registry 继续只拥有字段 schema/trust；项目对象 ID 和数字注册行不得写入 Field Registry。

## 3. 内部数据契约

新增 internal-only `Ra2ContentTemplateRegistrationSpec`：

- `RegistrySectionName`：固定且经过标识符校验的注册 Section；
- `ObjectIdSource`：Literal 或已声明 Parameter；
- `ExpectedObjectKind`：被注册对象的预期 Section kind；
- `Policy`：本阶段只允许 `ExplicitNumberedList`。

`Ra2ContentTemplateDefinition` 以 additive optional registrations 参数持有声明；既有构造调用和
六个现有模板的输出必须零变化。Definition 的 128 work-item 上限同时计算 Section、Field 和
Registration 声明。

## 4. 编译与分配规则

每个 Registration 必须满足：

1. 解析后的对象 ID 对应模板中唯一一个兼容 kind 的 Section 声明；
2. 注册 Section 必须在当前 Snapshot 中唯一存在，且分类与对象 kind 兼容；
3. 所有注册 Key 必须是 invariant、非负、无符号十进制整数；
4. 所有注册 Value 必须是合法对象 ID；
5. 已有数字 Key 不得重复；同一对象 ID 不得占用多个索引；
6. 对象已在唯一索引注册时幂等，不产生 operation；
7. 新对象使用 `max(existingIndex) + 1`，空列表使用 `0`；不填洞、不重排、不规范化旧索引；
8. `int.MaxValue` 后无可用索引时失败；
9. 同一 Plan 的多个 Registration 按声明顺序分配，并看到前面已保留的索引/对象；
10. 生成普通 `UpsertField` operation，继续进入唯一 Preview/Apply/Undo 链。

任何异常均整体失败，不返回 partial Plan 或 partial warning。

## 5. 失败语义

编译器内部新增精确 failure：

- `RegistrationTargetNotDeclared`
- `RegistrationSectionNotFound`
- `RegistrationSectionKindMismatch`
- `InvalidRegistrationList`
- `DuplicateRegistration`
- `RegistrationIndexOverflow`

本阶段没有 public template 使用 Registration，因此不扩展 public expansion enum；未来 Profile
启用时必须另行决定稳定的 public failure 投影，不能静默归入成功。

## 6. 明确禁止

- 不创建或修改 Techno/SuperWeapon/AI profile；
- 不修改当前 Weapon/Projectile/Warhead profile；
- 不实现 `artmd.ini`、多文档 Preview/Apply/Undo 或自动 Save；
- 不修改 parser、classifier、diagnostics、Field Registry、BuiltIn 数据；
- 不修改 Shell、XAML、Dock、AutomationId 或项目文件；
- 不以 raw INI、正则文本拼接或第二写入服务绕过 canonical Preview。

## 7. 验收矩阵

1. 既有 Definition/Compiler 测试保持兼容；
2. 空注册列表分配 `0`；
3. 稀疏列表在最大索引后追加且不填洞；
4. 已注册对象幂等；
5. 非数字/负数/重复索引/重复对象/非法对象 ID/溢出全部 fail closed；
6. 注册目标未被模板声明、注册 Section 缺失或 kind 不兼容时精确失败；
7. 多项注册按声明顺序稳定分配；
8. Registration 计入 128 work-item 上限；
9. 生成 Plan 可由现有 Preview 正确投影；
10. Application/IDE non-UI 回归、Debug build 和 IdeOnly clean package 通过。

## 8. 下一阶段边界

`CONTENT-2D-2` 才能引入项目级多文档 Snapshot/Plan/Preview/Apply/Undo；在该契约完成前，
任何 `artmd.ini` 同步都只能报告为未解决依赖，不能宣称完整对象已经闭合。
