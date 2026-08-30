# ASSET-VOX-3B Accepted Candidate & VOX Export — Final Contract

状态：Approved / implementation in progress  
批准日期：2026-08-28  
风险等级：R4（显式素材文件写入；不扩大到项目保存或 VXL/HVA）

## 1. 目标

让体素风格工作区把用户正在审阅的、可物化的体素候选显式固化为会话内最终候选，并把该候选安全导出为 MagicaVoxel `.vox` 副本。生成、平滑、Agent 几何提案和上色只负责产生候选；固化动作建立唯一导出权威。

## 2. 阶段拆分

- 3B-1：定义不可变 `AcceptedCandidate` 会话对象和候选种类。
- 3B-2：把原始、直接、平滑、Agent 几何、普通着色和对比度着色接入显式固化。
- 3B-3：新增受限 VOX 导出服务，复用 canonical `Ra2MagicaVoxelCodec`。
- 3B-4：接入现有工作区 UI，并增加自动化、状态和负向用例。
- 3B-5：执行范围审计、完整构建/测试/干净打包并维护项目状态文档。

## 3. 候选权威

可固化：

- Original
- Direct
- Refined（仅当真实存在并通过既有候选门禁）
- Symmetry / Agent Geometry Candidate（仅当既有结构结果成功且仍绑定当前证据）
- Styled Result
- Contrast Styled Result

不可固化：

- Difference
- Structure Regions
- Region Mask
- Palette
- 任何失败、陈旧、缺失或仍在执行中的结果

固化后保存不可变 canonical snapshot、候选种类、显示名、建议文件名、session generation 和 canonical hash。切换审阅页不改变已固化候选；载入/生成新源、采用新的工作几何、重新编译风格或修改风格要求会使旧候选失效。用户可显式用另一个可物化候选替换当前最终候选。

## 4. 导出事务

- 只允许 `.vox`，且目标父目录必须已经存在。
- 第一版只支持“导出副本”，不得覆盖当前载入的源 VOX。
- 覆盖其他已存在目标必须先经过系统 `SaveFileDialog` 覆盖确认。
- 拒绝把现有重解析点文件作为覆盖目标。
- 编码必须复用 `Ra2MagicaVoxelCodec.Write`，不得创建第二套 VOX writer。
- 在目标同目录创建唯一临时文件，写入后执行 `Flush(true)`。
- 使用 `Ra2MagicaVoxelCodec.Read` 回读临时文件，再次编码并要求字节完全相同。
- 只有回读门禁成功后才通过同卷原子 move/replace 发布；失败或取消清理临时文件并保留原目标。
- 成功结果报告最终路径、字节数和 SHA-256；不自动打开、注册、应用或保存项目。

## 5. UI 契约

保留：

- `VoxelStyle.AcceptSession`，显示文本改为“固化最终候选”。

新增：

- `VoxelStyle.FinalCandidate.Status`
- `VoxelStyle.ExportVox`

“导出 VOX…”只在存在已固化候选且工作区空闲时可用。导出状态进入既有状态条；取消不是错误，写入或验证失败是错误。

## 6. 冻结边界

本阶段不修改：

- Shell、菜单、Dock 与布局
- INI、Field Registry、Completion、Diagnostics、Undo/Redo、Save Preflight
- 项目 Apply/Save、Manifest 或素材自动注册
- VXL/HVA writer、游戏法线、炮塔/炮管装配写出
- 真实 DeepSeek/Tencent 调用、Provider 协议和 public API

## 7. 验收门禁

- 原始候选不调用 Provider 即可固化并导出。
- 可物化候选可显式替换；审阅视图不能固化。
- 固化候选在纯视图切换后保持不变，在真实输入变化后失效。
- 导出文件可由 canonical codec 回读并确定性重编码。
- 当前源覆盖拒绝，其他目标无确认不覆盖，取消不产生文件，失败不遗留临时文件。
- UI AutomationId 和按钮文案有契约测试。
- 完成 IDE-only restore、build、三组测试与 clean package；人工 WPF 交互保留为单独 smoke。

