# ASSET-VOX-4B — Agent-Seeded Human Semantic Editing Code Fact Audit

状态：Completed / implementation gate passed  
日期：2026-08-29

## 1. 已有权威链路

- `Ra2VoxelSemanticEvidenceBuilder` 以 working snapshot hash 为身份生成 2×4×3 粗区域和镜像配对。
- `Ra2VoxelSemanticMaskCompiler` 仅消费文本几何证据，生成最多三轮的 DeepSeek 建议。
- `Ra2VoxelSemanticLayerResolver` 已固定 `HumanOverride > AgentSuggestion > Unknown`。
- `Ra2VoxelStyleWorkspaceViewModel` 已支持接受/丢弃建议、区域级人工赋值、镜像联动和 remap 人工批准。
- `Ra2VoxelViewport3D` 已能把 3D 点击还原到最近占用体素并选中所属语义区域。
- `Ra2VoxelSemanticStyleIntegrator` 已将语义区域转为 `Ra2VoxelExplicitMask`，复用既有 style compiler、palette
  quantization、colourizer、review 和 3B 固化/VOX 导出链。

## 2. 实际缺口

当前人工覆盖只能修改整个 Host 粗区域。一个粗区域内同时包含车窗、车体和附件时，用户无法调整边界；重新细分
Host 区域或让文本模型返回体素坐标都会扩大模型权威并引入脆弱限制。正确缺口是一个绑定当前 working hash 的
体素级人工覆盖层。

## 3. 复用结论

- 继续保留 AI 建议层和区域人工覆盖层，不修改 DeepSeek 工具契约。
- 复用 3D 点击坐标恢复；扩展事件携带体素坐标，不建立第二套拾取。
- 复用 `Ra2VoxelExplicitMask` 与现有 colourizer；人工细分只改变显式 mask membership，不直接写 palette index。
- 复用当前工作区的中央 3D 和下方“语义”详情页；不增加窗口、Shell 面板或第三列布局。

## 4. 风险结论

`R3 / Immediate`：新增跨 Application/IDE 的会话内编辑模型并改变局部 UI 交互，但不改变 public API、持久化、
writer、项目 Apply/Save 或几何权威。用户已明确要求契约审查通过后执行，本审计未发现必须升级到 R4 的条件。

