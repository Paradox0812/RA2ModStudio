# AUTOMATION-CONTENT-1E-0 BuiltIn Template Source Audit

日期：2026-08-23  
结论：Passed / 允许实现首个生产模板

## 选定模板

稳定 ID：`weapon-projectile-warhead-skeleton`  
版本：`1`  
用途：在当前 INI 文档末尾创建一个 Weapon、一个 Projectile、一个 Warhead section，并只写入
Weapon 的两个确定关系字段：`Projectile=<projectileId>`、`Warhead=<warheadId>`。

该模板是“关系骨架”，不是完整可用武器数值预设。它不生成 Damage、ROF、Range、Speed、Verses
等玩法默认值，不宣称自动完成平衡，也不创建注册列表、素材或跨文件绑定。

## 代码与字段来源证据

1. `Ra2DocumentSemanticModelBuilder.TryGetReferenceTarget` 明确把 Weapon.`Projectile` 解析为
   `Ra2SectionKind.Projectile`，把 Weapon.`Warhead` 解析为 `Ra2SectionKind.Warhead`。
2. BuiltIn v3.2 字段包中两项均精确 `appliesTo: [Weapon]`、`schema.type: Reference`、
   `sourceKind: Yuri`、`quality: source-verified-modenc-weapon-core-20260603`。
3. `Projectile` 的字段来源记录为 ModEnc Projectile page；`Warhead` 的字段来源记录为
   ModEnc Warhead page，检查日期均为 2026-06-03。
4. 模板没有固定字段值或猜测的 target-kind；三个 section ID 全部由调用者显式提供。

权威文件：

- `RA2IniEditor.Application/Language/Ra2DocumentSemanticModelBuilder.cs`
- `RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json`

## 边界核验

- current-document only；不需要 multi-file transaction。
- 不自动修改 `WeaponTypes`、`Warheads`、`Projectiles` 等注册列表，不分配数字索引。
- 不生成 Image/Icon/VOX/VXL/SHP，不读取文件或网络。
- 只展开成 `Ra2AutomationEditPlan`；后续仍必须走 canonical Preview 和显式 Apply。
- 若 effective provider 覆盖、删除或降级任一关系字段，Expansion 会以 FieldSchemaNotFound 或
  BlockedFieldTrust fail closed；模板自身不能覆盖字段库事实。
- 目标 section 已存在、名称冲突、snapshot/revision stale 时不会返回可 Apply 的候选。

## 剩余风险

- 空 Projectile/Warhead section 只是可继续编辑的结构骨架；用户仍需补齐具体行为字段。
- 当前字段库没有通用 reference target-kind schema；该模板只使用语义模型已明确识别的两个关系。
- 注册列表自动维护属于 CONTENT-2/multi-file authoring，未包含在本阶段。

## Gate 结论

该模板满足 CONTENT-1E-0：真实 authoring workflow、关系字段 source-backed、无猜测默认值、无 R4
能力依赖。允许进入 1E service/Gateway 实现；不得在同阶段增加第二个未经审计的生产模板。
