# ASSET-VOX-4E Rev.5 — 色阶、主体可见性与可见表面覆盖修正

状态：Approved / Implemented / Automated verified / Physical visual pending  
日期：2026-08-31

本修订由真实 VXLSE III RA2 `unittem` 色盘与用户模型暴露的问题触发，修正 Rev.3 的局部颜色家族、几何覆盖和语义完成度规则。它不改变 4D sidecar、Provider 协议、项目 Save/Apply、VOX/VXL writer 或 Shell。

## 1. 已确认问题

- 单靠全色盘 OKLab hue/chroma 阈值会把基准色 `#72 / #707058` 与相邻棕黄 ramp 混为一组，产生跨色阶的 BodyMid/Dark/Under。
- WholePart 的 BodyBase 会被 Side/Top/Under/Edge 全部覆盖，真实样本上人工基准色可能没有任何可见体素。
- `familyCount >= 2` 对克制边缘过宽，会使 Edge 角色成为主体面积色。
- 以全部空间分区或全部占用体素的 Unknown 数量作为完成条件，会把封闭内部体素误报为必须人工处理。
- 上色输入变化清除结果快照后，预览模式仍停在 Result/Contrast/Mask/Palette，导致旧 SliceStack 图像残留。

## 2. 修订后的权威规则

1. BodyBase 仍是人工从 active palette 选择的 exact opaque/non-remap index。
2. 对 chromatic anchor，优先只在其所在的 16 项连续索引 ramp 内选择 BodyLight/Mid/Dark/Under/Edge；若该 ramp 层级不完整，只能在同一 ramp 内降级并产生 review warning，不得静默跳到相邻 ramp。
3. 中性灰阶在本地 ramp 无法形成层级时可以使用原有 neutral family，以保留非索引渐变色盘的合法能力。
4. SideExposed 使用 BodyBase；TopExposed 使用 BodyLight；Interior 使用 BodyDark；UnderExposed 使用 Underside。BodyMid 保留为计划/质量家族角色，但不再覆盖全部侧面主体。
5. `Subtle` Edge 只覆盖三类轴向表面家族均暴露的高置信角点；`Strong` Edge 可覆盖两类及以上；`None` 不生成 edge mask。旧 1E colourizer 入口保持原行为，4E materializer 显式传入 technique edge policy。
6. 质量报告必须记录 BodyBase 可见应用数和可用机会数。存在可用侧面机会但 BodyBase 可见数为零时为 `Blocked`。
7. 语义覆盖只以非 Interior 的可见表面为分母；MaterialRole 已知或 remap 已明确批准即视为已知。隐藏内部 Unknown 不触发覆盖警告。
8. 可见覆盖率 `>=98%` 不产生覆盖警告；`90%–<98%` 为 `PartialVisibleSurfaceCoverage`；`<90%` 为 `LowVisibleSurfaceCoverage`。覆盖不足只进入 `NeedsReview`，不得按分区数量禁止编译上色。
9. 工作流不得要求所有生成分区均完成。应显示可见表面 `known/total` 与百分比，并说明其余未分类分区可按需处理。
10. Result/Contrast/RegionMask/Palette 因本地输入变化失效时，预览回到 Semantics 3D；没有语义证据时回到 Original 3D，并清除 SliceStack fallback 状态。

## 3. 身份与版本

- Colour family metric：`indexed-ramp-oklab-v2`
- Selector：`indexed-ramp-oklab-family-selector/2`
- Normalizer：`ra2-voxel-style-normalizer/2`
- Quality policy/report：`ra2-voxel-colour-quality/2`
- 所有新覆盖数据均为 runtime-derived internal facts；public API 与持久化 schema 零变化。

## 4. 验收边界

自动化必须覆盖：RA2 风格 ramp 邻近干扰色、Subtle/Strong/None edge mask、隐藏内部 Unknown、Side→BodyBase，以及上色输入变化后的 3D 回退。真实模型的视觉质量、100%/125% DPI 和相机交互仍由人工验收；未经验收不得宣称 GameReady。
