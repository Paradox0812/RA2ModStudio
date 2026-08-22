# RA2IniEditor.IDE v0.5.0-preview 烟测清单

## 基础启动

- [ ] 启动 `RA2IniEditor.IDE.exe`。
- [ ] 窗口左上角显示应用图标。
- [ ] 任务栏显示应用图标。
- [ ] 主窗口无启动异常。

## 打开项目

- [ ] 使用 `文件 -> 打开项目...` 打开真实 RA2/YR MOD 文件夹。
- [ ] 项目浏览器显示 INI 文件。
- [ ] 能打开 `rulesmd.ini` / `artmd.ini` / `aimd.ini`。

## Hover 验证

- [ ] `[General] BaseDefenseDelay=.25` 显示基地威胁响应延迟，不再显示“生产防御建筑”。
- [ ] 普通 verified 字段 Hover 不显示冗长来源审计。
- [ ] inferred 字段只显示轻量风险脚注。
- [ ] `BalloonHoverHeight` 这类字段提示未实现或不建议使用。

## Diagnostics 验证

- [ ] 明显未知字段显示 Unknown Key。
- [ ] 错误上下文字段尽量显示 WrongContext。
- [ ] obsolete / non-existent 字段显示对应风险。
- [ ] inferred 字段不会刷屏污染 Issues 面板。

## Quick Peek / Completion

- [ ] Field Quick Peek 可以打开。
- [ ] Quick Peek 能看到字段可信度详情。
- [ ] Add Property / Completion 候选区域正常显示。

## 保存与备份

- [ ] 修改一个测试字段后 dirty 状态正常。
- [ ] `Ctrl+S` 可以保存。
- [ ] 保存后生成备份。
- [ ] Revert 可以放弃内存修改。

## 窗口布局持久化

- [ ] 拖动并重排底部/右侧工具面板后正常关闭，重启后布局恢复。
- [ ] 关闭被取消时不写入新布局；正常关闭只保留一个有效 v1 文件。
- [ ] “恢复默认布局”后无需再次正常关闭，下一次启动仍使用默认布局。
- [ ] Search、Find References、Problems、Output、Project Explorer 和 AI 命令在恢复后仍能激活当前工具实例。
- [ ] 将浮动窗口保存到左侧副屏后断开副屏，重启时窗口回到当前显示器且标题拖动区域可见。
- [ ] 在可用环境分别检查 100%、125% 和 150% Windows 缩放；混合 DPI 不可靠时应安全回退而不是出现完全离屏窗口。

## 性能观察

- [ ] 大文件打开没有明显卡死。
- [ ] Navigator 跳转可用。
- [ ] Issues 刷新没有明显长时间阻塞。

## AI 助手验证

- [ ] AI 面板默认显示 DeepSeek V4 Flash。
- [ ] 模型列表只有 DeepSeek V4 Flash / V4 Pro，两项均可切换。
- [ ] 配置状态和联网、费用、不修改文件提示可见，且不显示 API Key 或完整 endpoint。
- [ ] 超过 8000 字符的输入在请求前被拒绝，原文仍保留。
- [ ] 取消、超时或失败后，已接收文本仍可复制，失败轮次不进入后续对话上下文。
- [ ] 大型 Markdown 超限时降级为完整只读纯文本，不产生自动重试或模型 fallback。

## 发布结论

- [ ] 可以发给测试用户。
- [ ] 需要回退。
- [ ] 需要记录字段误报 / UI 问题后再发。
