# RA2IniEditor.IDE 产品愿景与需求基线

状态：Accepted  
确认日期：2026-08-22  
事实来源：用户在当前任务中确认的最终意图，以及已确认的 IDE-only、Agent、
搜索、字段库、AI、UI 和自动化架构契约。

## 1. 最终产品目标

用户以自然语言描述一个完整的 RA2 / YR / Ares / Phobos Mod 制作需求，Agent
负责把它转化为可执行计划，并协调完成：

- INI 内容的创建、查询、修改、诊断和验证；
- Cameo / Icon 等游戏图标素材的生成、调色板处理、命名和项目绑定；
- VOX / VXL 单位素材及其 body / turret / barrel 组成部分的生成与导入准备；
- SHP 动画帧、序列、调色板、锚点和导出素材的生成；
- 素材与 INI 引用关系的装配、预览、差异审查和最终提交；
- 后续可重复执行的自动化任务、产物追踪和验证报告。

目标交互应接近：

```text
自然语言需求
  -> Agent 分解目标和约束
  -> 调用版本化 INI / Asset / Validation capabilities
  -> 生成可追踪的 Preview 与 Artifacts
  -> 按风险策略确认或执行
  -> 在 IDE 中展示 INI、图标、素材和诊断结果
  -> 保存、打包或交给外部工具完成受控后处理
```

“自动完成”表示 Agent 能够独立编排完整工作流，不表示模型可以绕过权限、
预览、事务和验证直接改写任意文件。低风险批处理、确认策略和无人值守级别
需要后续单独契约；在此之前，写入继续以显式预览和确认作为安全默认值。

## 2. 产品定位

- 当前产品是 source-first 的 RA2IniEditor.IDE，不是旧式表格编辑器。
- INI、MAP、VOX/VXL、SHP、PCX/PNG 等实际工程文件始终是事实源。
- 不创建 `.iproj` 或数据库作为第二套项目真相。
- 文本编辑器是主要工作区，字段库、AI、诊断、搜索和素材工具围绕源码工作。
- 最终产品应可独立完成真实 Mod 制作任务，而不是 Mock、演示 UI 或提示词外壳。
- legacy MainWindow、表格编辑器、旧对象工作台和旧复制工作流不得恢复。

## 3. Agent 需求

Agent 是高层协调对象，不是一个直接持有所有领域逻辑的巨型类。

Agent 必须：

1. 从显式项目/文档/字段库/素材快照读取事实；
2. 通过版本化 Capability Gateway 调用中立领域能力；
3. 将自然语言转换为结构化操作，而不是只返回 Markdown 示例；
4. 对修改生成确定性 Preview、ChangeSet、诊断差异和风险信息；
5. 保留文档身份、版本、光标、Dirty、Undo/Redo 和活动会话所有权；
6. 拒绝过期、歧义、越界、冲突或证据不足的操作；
7. 追踪长任务、事件、产物、失败和取消状态；
8. 不直接依赖 WPF ViewModel、AvalonEdit 控件、Shell 或供应商 DTO；
9. 不绕过现有 Save / Backup / Rollback 路径写盘；
10. 对所有外部工具和模型输出保持不信任，先本地校验再进入项目。

## 4. INI 能力需求

- 打开和浏览项目中的规范 INI 文件与 Section。
- 编辑、补全、Hover、Quick Peek、引用查询和诊断。
- 单文件查找与替换、项目级查找。
- 基于真实当前文本的结构化字段 Upsert / Replace Preview。
- 当前文件事务应用、单次 Undo/Redo、显式保存。
- 后续扩展到新 Section、模板实例化、多文件计划和项目级原子提交。
- 不允许 Agent 以“重写整个文件”替代最小语义变更。

## 5. 字段库需求

- 保持 `Project > Global > BuiltIn` 优先级。
- 不向用户暴露缺乏证据的占位字段作为可靠补全事实。
- 区分 source-verified、reviewed/inferred 和 diagnostic-only 等可信度。
- AA/AG 等字段按真实上下文判定，不按字段名进行宽泛推断。
- Hover 保持轻量；更完整来源与可信度进入 Quick Peek / AI Evidence。
- 诊断区分错误上下文、废弃、不存在和伪字段风险。
- 字段学习、导入、Apply 和 Rollback 必须可审查、可恢复。

## 6. AI Provider 与对话需求

- 生产模型为 DeepSeek V4 Flash 与 DeepSeek V4 Pro，默认 V4 Flash。
- 产品代码不保留 Fake/Mock provider；测试替身只能存在于测试项目。
- 支持 SSE 流式响应、增量显示、取消、超时和断流保留。
- 明确分类网络、协议、超时、取消、Provider、配置和消费端失败。
- 当前不自动重试、不静默切换模型或供应商。
- 只传输有预算、已清洗且与当前任务相关的上下文。
- 模型不能直接获得编辑权；只有本地验证的结构化提案能进入编辑事务。
- AI 面板保持 Copilot 式紧凑信息层级，只显示简要上下文。

## 7. 素材与图标需求

### 7.1 Cameo / Icon

- 支持文本和参考图输入。
- 生成适合 RA2 UI 的构图、尺寸、调色板和 Remap 方案。
- 产出原始图、游戏格式候选、预览图、Manifest 和 INI 绑定建议。
- 每项产物都有来源、参数、版本、哈希、验证和失败信息。

### 7.2 VOX / VXL

近期确认路线：

```text
自然语言/参考图 -> VOX 体素模型 -> 无损二维切片序列
-> SliceStack Manifest -> VXLSE III 导入 -> VXL/HVA 人工校正与保存
```

切片契约必须包含 part、axis、order、width/height/depth、origin/pivot、palette、
transparent/remap index 和稳定命名。切片必须一像素对应一体素，禁止缩放、
抗锯齿、插值和 JPEG。近期不开发完整 VOX->VXL 二进制编译器；长期是否
替代 VXLSE III 为未决策项。

### 7.3 SHP 动画

- 支持动作描述、帧数、方向数、速度和循环规则。
- 生成一致的逐帧图像、锚点、透明/Remap 调色板和序列 Manifest。
- 提供抖动、边界、调色板越界、帧尺寸和循环连续性检查。
- 真实 SHP 编码器、外部工具适配和授权边界需独立契约。

## 8. UI 与工作区需求

- WPF + AvalonDock 是当前桌面实现基线。
- 视觉方向参考现代 Visual Studio，默认几何基准为 1920x1080，并适配 DPI、
  WorkArea 和不同分辨率。
- 主编辑区优先；右侧、底部和浮动工具窗可调整、停靠、恢复和持久化。
- Search 是独立浮动 Dock 工具，可关闭后恢复，不使用 Mock 结果。
- 全部一级和二级界面共享字体、颜色、间距、边框、控件模板和矢量图标体系。
- 深色主题当前后置；不得以位图控件替代生产 XAML 控件。

## 9. 自动化与运行时测试需求

- 建立 UI-neutral Application 层和 Capability Gateway 后再引入 Job Runtime。
- CLI、内置 AI 和未来独立 Agent 复用同一语义实现。
- 游戏运行时通过独立 `RA2TestHost` 和 `IRuntimeAdapter` 隔离。
- TestCase、Setup、Steps、Assertions、Trace、Result 必须确定性、可重放。
- LLM 可以生成和分析测试，但不能自行定义通过标准。
- 优先采集结构化 Trace，不以截图或视频作为唯一测试 Oracle。

## 10. 当前非目标与后置项

- 自动重试和模型 fallback。
- 深色主题。
- 项目级批量替换和自动保存。
- 无确认的多文件写入。
- 完整 VOX->VXL 二进制编译器。
- 在 Capability Gateway 稳定前建设大型 Job/Event 框架。
- 恢复 legacy 产品或把历史契约当作当前需求。

