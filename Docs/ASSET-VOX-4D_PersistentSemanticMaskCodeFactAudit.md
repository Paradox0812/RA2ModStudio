# ASSET-VOX-4D — Persistent Semantic Mask Code-Fact Audit

状态：Reviewed / R4 contract gate / runtime unchanged  
日期：2026-08-30

## 1. 任务目标

让当前体素工作区的语义分划可以显式保存到项目内 sidecar，并在以后载入同一工作几何时恢复；保存的内容
仍通过现有语义合成、着色、固化和 VOX 导出链消费，不修改模型几何或把审阅颜色写进色板。

## 2. 当前真实实现

- `Ra2VoxelSemanticEvidencePackage` 由当前 `Ra2VoxelSceneSnapshot` 确定性生成，携带 `SourceSnapshotHash` 和
  `PackageHash`，区域 ID 固定为当前 2×4×3 空间分箱结果。
- 语义作者状态有三层：已接受的 Agent suggestions、`_semanticManualOverrides` 人工区域覆盖、
  `Ra2VoxelSemanticManualMaskLayer` 稀疏人工 cell 覆盖。
- `Ra2VoxelSemanticMaskComposer` 是唯一 per-cell 优先级合成器：cell human > region human > accepted Agent > Unknown。
- cell layer 绑定 canonical snapshot hash、canonical occupied-cell ordering 和 occupancy count；它当前明确是
  session-only 且没有 serializer。
- `ClearSemanticState` 会在源模型、生成源、工作几何、项目和 ViewModel 生命周期变化时清除这些状态；当前
  没有 dirty 标记或恢复入口。
- `AtomicTextFileWriter` 已提供同目录临时文件 + replace/move 的文本原子提交，不需要第二个写入算法。
- 当前 VOX Save-As 是独立物化事务；语义 sidecar 不应并入该 writer 或项目 Apply/Save。

## 3. 已排除的错误方向

- 只保存最终 VOX 颜色：会丢失部件/材质标签、来源优先级和阵营色批准含义。
- 保存整个 `Ra2VoxelSemanticMaskComposition` 并全部恢复为人工 cell：会把 Agent 建议错误提升为人工权威。
- 只保存区域 ID：会丢失细粒度表面画笔覆盖。
- 只保存 cell index 而没有 snapshot/evidence hash：存在把旧分划套入新几何的风险。
- 在哈希失配时按最近坐标或区域名猜测迁移：重演此前 Host 过度干预问题，且可能产生静默错误。
- 把 sidecar 塞进 VOX/VXL/HVA、自定义 chunk 或项目 INI：扩大 writer、兼容性和游戏资产边界。

## 4. 正确复用路径

```text
current working snapshot/evidence
    + accepted Agent layer
    + human region layer
    + human cell layer
        ↓ explicit Save
project-contained semantic sidecar v1
        ↓ strict Load + exact hash checks
existing session layers
        ↓ existing LayerResolver + MaskComposer
existing style/colour/freeze/export path
```

- Application 语义模型保持不变；IDE-internal store 只负责持久化映射、路径、JSON 与 typed result。
- cell 以同一 snapshot 的 canonical index 分组保存；恢复时检查范围、唯一性、总数和重建后的 layer hash。
- Agent suggestion 与人工区域覆盖分开保存；只有已经由用户接受的 Agent 层进入 sidecar。
- 使用现有 `AtomicTextFileWriter`；不新增依赖或平行原子写入器。

## 5. 风险结论

风险为 **R4 / StopForReview**：这是新的持久化 schema 和恢复契约。当前用户已同意总体方向，但实现前仍需
审批精确 v1 格式、哈希失配策略、覆盖行为和 UI 契约。此审计只允许写文档，不允许运行时代码变化。
