# ASSET-VOX-4A Stage Result Ledger

日期：2026-08-29  
风险：R4（跨 Application/IDE/ViewModel/XAML 的会话内语义与着色链；无 public/persistence/writer 变更）

| Stage | 结果 | 关键证据 |
|---|---|---|
| 4A-1 | Completed | 新增 internal 词表、确定性 2×4×3 空间区域、镜像配对、二进制掩码、snapshot/package hash 和文本证据。 |
| 4A-2 | Completed | text-only required-tool 主分析 + 审阅；语义指纹不一致时第三轮仲裁；未知 ID/错误哈希拒绝。 |
| 4A-3 | Completed | AI 建议层与人工覆盖层分离；人工优先、同 hash 复分析保留、镜像联动、逐行撤销、remap 人工批准。 |
| 4A-4 | Completed | 显式掩码进入现有 style/colourizer；3D 语义视图和外露面区域选择进入现有 workspace。 |
| 4A-5 | Completed | 契约、DecisionLog、用户/开发文档、定向与全量验证、IdeOnly clean package。 |

## Verification Matrix

| Gate | 结果 |
|---|---|
| Application semantic masking focused | Passed 3/3 |
| IDE compiler/UI/viewport focused | Passed 12/12 before full gate |
| Debug solution build | Passed；0 errors；1 个任务前既有 nullable warning |
| Application full | Passed 296/296 |
| IDE full | Passed 2860/2860 |
| AssetHost full | Passed 50/50 |
| IdeOnly clean package | Passed；1406 source files；标准构建/缓存/压缩包排除生效 |
| Live DeepSeek/Tencent | NotRun by contract |
| Physical WPF visual smoke | NotRun；需要用户重启后截图验收 |

## Diff Intent

| 区域 | 意图 |
|---|---|
| Application voxel authoring | 新增 internal evidence/layer/mask/style integration；不改 canonical snapshot/writer。 |
| IDE AssetAuthoring | 新增 text-only compiler；扩展 coordinator/3D renderer 消费有效语义。 |
| ViewModel/XAML | 显式 AI 建议、人工覆盖、镜像联动、remap 批准和语义审阅入口。 |
| Tests | 覆盖 hash、两/三轮、人工优先、remap、着色几何不变、3D/UI contract。 |
| Docs | 固化权威边界、交付状态与后续风险。 |

## Deferred Governance / Technical Debt

- 当前 Host 区域是稳定的空间分区，不是视觉模型或人工笔刷到单体素级的精细分割；高精度语义边界进入 4B。
- DeepSeek 官方文本模型没有像素观察能力；玻璃、灯、开口等仍需用户证据/人工覆盖，不能宣称自动权威识别。
- 当前只在现有 style plan 已提供对应颜色角色时执行材质掩码；色彩角色自动合成/参考图取色需独立契约。
- 物理 WPF 点击拾取与 100%/125% DPI 截图需用户运行新进程验收。

## Stop rule

4A-1 → 4A-5 完成后停止。未进入真实 provider 调用、4B 几何/精细笔刷、Shell、Apply/Save 或 VXL/HVA。
