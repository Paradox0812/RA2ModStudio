# GIT-BASELINE-1 — Local Verified Baseline Ledger

日期：2026-08-24  
状态：Completed / locally verified  
分支：`codex/content-2d-baseline`  
本地注释标签：`content-2d01-verified`

## 1. 目标

将截至 `CONTENT-2D-0/1` 的已验证 IDE-only 工作树固化为可回退的本地 Git
基线。该阶段只管理版本控制状态，不改变产品代码、public API、持久化格式、
INI 语义、UI 行为或运行时权限。

## 2. 纳入范围

- 截至 `CONTENT-2D-0/1` 的 Application、IDE、测试和文档变更。
- Chat / Work、两阶段 Work 编排、RA2 内置 Skill、CONTENT-1、CONTENT-2A/2B
  与 CONTENT-2D-0/1 的已验证实现和证据。
- 本阶段 Git 台账与当前状态索引。

## 3. 排除范围与卫生门禁

`.gitignore` 已覆盖且本次提交不包含：

```text
.vs/
bin/
obj/
artifacts/
TestResults/
Logs/
*.user
*.suo
*.zip / *.7z / *.rar
```

审计结果：无删除项、无超过 5 MiB 的候选文件、无敏感扩展名、无软链接、
无已跟踪的禁入目录或压缩包。唯一凭据签名命中位于 DeepSeek loopback 测试，
经分类确认是测试占位值，不符合供应商密钥形态。

## 4. 复用的产品验证证据

GIT-BASELINE-1 不修改产品源代码，仅复用紧邻本阶段且针对同一工作树完成的
`CONTENT-2D-0/1` 验证：

```text
Debug build: Passed, 0 warnings, 0 errors
Compiler/Template focused: Passed 37/37
Application.Tests: Passed 162/162
IDE Agent/Template focused: Passed 106/106
IDE non-UI: Passed 2610/2610
IdeOnly clean package: Passed, 1183 files
Package SHA256: D1266B57A383626D2B1B396BF07C9FB4F37BCB4B26BA1257E4FC35F50A40F5F2
```

本阶段另执行工作树、暂存区、空白错误、敏感文件、禁入路径、分支、标签和
最终 clean status 检查。没有重复运行完整构建或测试。

## 5. 边界确认

- Legacy solution/project 未恢复、未构建、未提交。
- GIT-BASELINE-1 本身未修改 Shell、XAML、AutomationId 或产品语义。
- 基线提交包含此前经契约实施和验证的 Shell/AI/UI 变更；它们不是本阶段新增修改。
- 未配置、创建或推送任何远端；远端备份需要用户另行提供仓库并授权。
- 未改写历史、未 force push、未删除用户文件。

## 6. 后续入口

产品开发的下一安全入口仍是 `CONTENT-2D-2 Project Multi-Document Transaction`
代码事实审计与最终契约。Git 的下一步仅在用户提供远端仓库并授权后执行
remote 配置与首次 push。
