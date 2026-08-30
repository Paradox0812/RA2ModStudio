# ASSET-VOX-4B Stage Result Ledger

日期：2026-08-29  
状态：4B-0 → 4B-5 completed

| Stage | 状态 | 证据 |
|---|---|---|
| 4B-0 | Completed | 代码事实审计与最终契约已固化，自审未发现 Shell/writer/persistence/public API 依赖。 |
| 4B-1 | Completed | 新增 hash-bound 稀疏 cell override、表面画笔、镜像原子编辑、composition 与现有 style integration；核心 focused 6/6。 |
| 4B-2 | Completed | 既有 3D 短点击携带 canonical coordinate；语义页新增紧凑浏览/画笔/擦除工具条、独立画笔目标、大小、镜像和 undo/redo。 |
| 4B-3 | Completed | 最终 composition 进入语义 3D 预览和现有 palette-safe colourizer；坐标/占用不变，writer 未修改。 |
| 4B-4 | Completed | Workspace/ViewModel/viewport/UI focused 28/28；新增镜像、擦除、stale、优先级、undo/redo 和 composition 测试。 |
| 4B-5 | Completed | Debug build 0 errors / 1 个任务前既有 nullable warning；Application 299/299、IDE 2862/2862、AssetHost 50/50；IdeOnly clean package 1410 files。 |

## Verification Matrix

| Gate | 结果 |
|---|---|
| Application semantic focused | Passed 6/6 |
| Workspace/ViewModel/viewport/UI focused | Passed 28/28 |
| Debug solution build | Passed；0 errors；1 个任务前既有 nullable warning |
| Application full | Passed 299/299 |
| IDE full | Passed 2862/2862 |
| AssetHost full | Passed 50/50 |
| IdeOnly clean package | Passed；1410 files；标准 build/cache/archive exclusions 生效 |
| Live DeepSeek/Tencent | NotRun by contract |
| Physical WPF 100%/125% smoke | NotRun；需要用户重启后验收 |

## Deferred Governance / Remaining Risk

- 表面画笔只编辑可通过旋转点击到的外露体素；隐藏/内部切片编辑不在 4B 范围。
- 画笔状态是会话内状态，不随项目或模型文件持久化。
- 真实 WPF 的按钮换行、短点击与拖动区分仍需用户在 100%/125% DPI 手工验收。
- 不计划 4C；若未来需要持久化 mask interchange 或内部切片编辑，应重新立约。

## 2026-08-29 physical-smoke correction

- 用户物理验收确认画笔可进入但短点击没有反馈。根因是编辑模式可在没有语义证据时假启用，并且单元格画笔
  额外依赖预先选择区域行；切回原始预览后，已激活画笔也不会重新进入语义预览。
- 浏览/画笔/擦除现在统一复用 `PrepareSemanticRegionsAsync`：尚无证据时自动准备本地区域且不调用 DeepSeek。
  画笔直接采用 3D 命中的区域，保留独立部件/材质目标，不再要求先选择列表行。
- 离开语义预览会显式回到浏览模式，重复进入画笔会重新切到语义预览，不再显示无法执行的假激活状态。
- 回归验证：workspace/ViewModel/viewport/UI focused 28/28；顺序重跑 Debug build 0 warnings / 0 errors。
  首次并行 build/test 因共享 WPF `obj` 目录发生临时生成文件竞争而失败，未计为产品失败，已保留在交付报告。
