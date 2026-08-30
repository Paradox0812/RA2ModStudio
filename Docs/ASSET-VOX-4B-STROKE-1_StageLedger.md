# ASSET-VOX-4B-STROKE-1 — Stage Result Ledger

状态：Implemented / automated verified / physical WPF acceptance pending  
日期：2026-08-30  
批准范围：STROKE-0 → STROKE-5

## 阶段结果

| 阶段 | 结果 | 证据 |
|---|---|---|
| STROKE-0 | 完成 | R3 契约获批；Shell、Provider、Apply/Save、writer、public API、持久化冻结。 |
| STROKE-1 | 完成 | 唯一 `Ra2VoxelSemanticMaskEditor` 增加多 seed 原子入口；单 seed 入口委托同一核心；输入去重、镜像、擦除、空输入、非法坐标与 8192 seed 上限均有定向测试。 |
| STROKE-2 | 完成 | 视口以 4 DIP 最大间距采样当前最前方外露面；路径去重；黄色/红色临时 overlay；捕获、场景、模式、相机和生命周期变化安全取消。 |
| STROKE-3 | 完成 | ViewModel 冻结笔划上下文，释放时一次提交；成功只产生一个 undo 并发布一次 composition，取消/失败不污染 layer/history。 |
| STROKE-4 | 完成 | 增加 session-only 部件/材质审阅维度、固定 8+8 色映射、紧凑切换/图例及 4 个 AutomationId。 |
| STROKE-5 | 完成 | Debug build、focused/full tests、差异审计和 IdeOnly clean package 完成；实体 WPF 验收交给用户。 |

## 实现边界

- 连续拖动只收集 canonical 表面 seed；最终 footprint 仍由 Application 唯一编辑器按现有大小/镜像规则计算。
- 拖动期间不修改人工蒙版，不建立历史，不重建正式语义场景；临时高亮只表示 seed，不冒充最终 footprint。
- 部件/材质颜色只用于审阅显示，不写 palette index，不改变 effective assignment、AI/人工优先级或导出结果。
- 未进行真实 DeepSeek/Tencent 调用；未修改 Shell、Apply/Save、VOX/VXL/HVA writer、public API、持久化、INI、Field Registry 或 legacy。

## 自动验证

```text
dotnet build .\RA2IniEditor.IDE.sln -c Debug --no-restore
  通过：0 error；1 条既有、无关的 CS8602 warning

Application focused (Ra2VoxelSemanticMaskingTests)
  9/9 通过

IDE affected (Viewport / Workspace / StrokePointer)
  57/57 通过

Application full
  302/302 通过

IDE full
  2885/2885 通过

AssetHost full
  50/50 通过

IdeOnly clean package
  通过：1417 个文件，输出 artifacts/RA2IniEditor.IDE.SourceClean.zip
```

物理 WPF 输入、100%/125% DPI 和视觉连贯性不能由上述测试替代。

## 用户手动验收

1. 左键单击后释放一次生效；一次撤销完全恢复。
2. 慢速、快速左键拖动均连续；拖动中显示临时路径，释放后只提交一次。
3. 从模型拖入空白再返回时，空白段不桥接、不命中背面。
4. 大小 3、镜像和擦除继续生效，整条笔划仍只有一个撤销项。
5. 笔划中按右键或切换模式/场景会取消未提交笔划；右键随后可正常旋转。
6. “部件 / 材质”切换只改变模型和图例的审阅颜色，不改变蒙版、撤销历史或最终色板。
7. 在 1920×1080、100%/125% DPI 下确认图例自然换行、主视图尺寸稳定。

## 剩余风险

- WPF 3D 命中与鼠标捕获的实体体验尚未手工验收。
- 临时路径只显示命中 seed；大小/镜像扩展后的完整范围只在释放后显示，这是已批准的性能边界。
- 本阶段不提供隐藏/内部体素绘制、套索、填充或持久化语义蒙版。
