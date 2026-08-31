# ASSET-VOX-4E Rev.6 — Directional Surface, Semantic Boundary and Technique Differentiation

日期：2026-08-31  
状态：Approved / Implemented / focused automated verification passed / physical visual acceptance pending

## 1. 目标

Rev.6 修正真实地面模型暴露的三类质量问题：侧面与底面重叠时形成不明黑带、前后端面缺少方向辨识、五种技法在最终模型上缺少可观察差异。同时把之前确认的“部件/材质分类预览”恢复为全局预览入口。

## 2. 权威与边界

- 人工单位类型、人工 RA2 色盘基准 index、4D 有效语义组合继续是权威输入。
- DeepSeek 只提出有界 raw style roles/bindings；人工基准色和技法不进入 Provider request，由 Host 在模型提案后确定性应用。
- 新方向面和语义边界均是 current snapshot 上的 session-only 派生蒙版；不写入 `.semantic.json`、项目设置、VOX/VXL/HVA 或 public API。
- 不修改 Provider/AssetHost JSON、项目 Apply/Save、writer、Shell、legacy 或 4D sidecar schema。

## 3. Directional Surface

- 无更强方向事实时，以 XY 中较长的包围盒轴作为 longitudinal axis；另一水平轴为 lateral axis。
- geometry mask 独立标记 `LongitudinalEndExposed` 与 `LateralSideExposed`，不再把全部 X/Y 外露面压成同一种 Side。
- 主要表面顺序为 Top → LongitudinalEnd → Lateral/Side → Under。前后端使用 BodyMid；长侧面保持人工 BodyBase。
- 同时 Side+Under 的体素不得使用 Underside，从源头阻止平滑侧面产生连续黑带。只有不属于可见侧面的真实向下结构才使用 Underside。
- Strong edge 只扩展到 top silhouette/ridge，不再把每个双面相交体素都描亮；Subtle 保留三面交点级覆盖。

## 4. Semantic Boundary

- Host 在相邻可见体素的 effective `PartRole` 或技法允许的 `MaterialRole` 发生变化时派生一体素边界。
- 仅 `RegionId`/空间分区变化不构成边界，避免把自动分区画成棋盘或切片线。
- 边界只选择 PaintedSurface 一侧；Glass、Rubber、BareMetal、Light、DarkOpening、Accent 和 approved remap 均为受保护 direct material，后置精确规则继续拥有最终优先级。
- 边界复用现有 EdgeOrRidge 色族，不新增语义材质、颜色主题、随机纹理或 remap。
- Part boundary 始终可用；Material boundary 由 technique 的 Conservative/Balanced/Strong policy 决定；small-part ownership 由 Accent policy 决定。

## 5. Technique differentiation

五种技法的 typed local policy 修订为 revision 2。最终结果必须通过下列至少一项产生差异，而不是依赖模型输出措辞：

| 技法 | 主要确定性差异 |
|---|---|
| balanced-rts-volume | 中等明暗、Subtle edge、Balanced material boundary |
| strong-silhouette-readability | 更大明暗级差、Strong top silhouette、Strong boundary、small-part emphasis |
| subtle-matte-shading | 最小明暗级差、Subtle edge、Conservative material boundary |
| semantic-material-separation | 克制体积、独立更强 edge index、Strong material boundary、量化失败 Block |
| compact-unit-clarity | Strong 但受控的 top silhouette、Strong boundary、small-part emphasis、紧凑级差 |

Host 自动化测试要求五种 policy signature 互异，并在同一 RA2 indexed-ramp fixture、同一人工基准色上产生五个不同的 voxel candidate hash。

## 6. Skill revision 2

通用、地面、空中、大型水面四个内置 colouring Skill 已同步：

- 明确人工基准色和技法为 Host-local，不伪称 DeepSeek 看见或选择它们；
- 明确 longitudinal end、lateral side、side+under 禁止黑带；
- 明确 effective semantic boundary、RegionId-only seam 忽略、direct material 保护；
- 保持禁止坐标输出、蒙版扩张、自动 remap、二进制写入和 GameReady 声明。

Skill 内容哈希变化只使 derived style cache 失效，不改变 Provider schema 或调用次数。

## 7. UI 合同

- `VoxelStyle.Semantics.ReviewPart`、`ReviewMaterial`、`ReviewDimension`、`ReviewLegend` AutomationId 保持。
- 分类预览控件移到中央全局 `VoxelStyle.Preview.Toolbar`，新增容器 `VoxelStyle.Preview.SemanticReviewControls`。
- 点击“部件”或“材质”只改变 review dimension，并强制显示 Semantics 3D、退出 slice fallback；不得改变五阶段 workflow stage、语义组合或持久化状态。
- 图例只在 Semantics view 显示；没有 semantic evidence 时入口禁用。

## 8. 质量门禁

- `UndersideSideLeak > 0`：Blocked。
- 存在可读 longitudinal end 机会但没有 BodyMid end cell：Blocked。
- direct material 被 boundary 覆盖：Blocked。
- boundary 有选择但最终无可见 accent：Blocked。
- Strong geometry edge coverage 超过 25%：NeedsReview，防止全轮廓发亮。
- 继续保留 palette legality、BodyBase exact anchor、geometry identity、semantic/remap precedence 和 physical visual acceptance 门。

## 9. 验证与剩余门

已通过：Debug solution build；Application 358/358；AssetHost 50/50；IDE 2920/2920；VoxelColour focused 55/55；VoxelStyle workspace focused 28/28；Skill catalog 18/18。最终工具栏 WrapPanel 收口后另有 IDE project build 0 warning/0 error、UI contract 3/3。四个 Skill 的 bundled `quick_validate.py` 因两个可用 Python 均缺少 PyYAML 未启动，未安装依赖；项目内 bundled Skill catalog/parser 测试已作为权威替代验证通过。

仍待用户手动：真实地面/空中/大型水面模型多视角、至少三种技法、部件/材质分类预览入口、100%/125% DPI、导出回读视觉。未完成前 `VisualAcceptance` 保持 Pending，不生成最终 clean package。
