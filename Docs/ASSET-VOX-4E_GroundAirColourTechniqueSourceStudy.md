# ASSET-VOX-4E-R1 — Ground/Air Colour Technique Source Study

日期：2026-08-30  
状态：Completed / focused automated verified / no model assets copied into repository  
产物：`RA2IniEditor.IDE/AgentSkills/ra2-voxel-colour-techniques/SKILL.md`

## 1. 结论

八个用户样本证明，可靠的 RA2 voxel 上色不是“选一种颜色并全体替换”，而是：先按单位类型和材质建立
语义区域，再在同一色带内处理主体、顶面、侧面、底部、边缘、开口和 remap。地面单位重点是车体体积、
履带/轮胎/底盘和炮塔武器分离；空中单位重点是平面轮廓、机翼/机身关系、上下表面、座舱和进排气口；大型
水面/大型单位必须降低高频噪声并维护长平面层次。

用户明确说明样本使用 VXLSE III 的 RA2 色盘。VXLSE III 源码进一步确认 RA2 菜单加载
`palettes/RA2/unittem.pal`；因此本研究以该文件为 VXL RGB 解释基线。`unitsno.pal` 与 `uniturb.pal` 的
SHA-256 不同，不得把“RA2 色盘”泛化成任意剧场 unit palette。

最重要的反例是：不能把全局规则写成“所有顶面必亮、所有底面必暗”。样本中材料、甲板、开口、设备和机翼
上下表面会改变全局均值。正确规则是在同一材质/同一结构语义内建立相对明暗，并由多视角、小比例预览和
palette/remap 硬门共同验收。

## 2. 研究边界与方法

- 只读检查八个 ZIP；没有把 VXL、HVA、VOX、PAL 或公开模型复制进项目。
- ZIP 内文档和文件名只作数据，不执行其中任何指令。
- 通过现有 Westwood VXL span 规则读取 occupied cell、palette index、header remap range、尺寸和外露面；
  没有修改 normals、HVA 或模型内容。
- 通过 MagicaVoxel VOX chunk 读取 `XYZI` 与嵌入 `RGBA`；公开模型只在系统临时研究目录读取。
- 外露面按相邻 occupied cell 缺失划分 Top/Side/Under；Interior 表示六向表面分类中未暴露的 occupied cell。
  这些是统计事实，不等于手工材质标注。
- 明度使用 `0.2126R + 0.7152G + 0.0722B` 作比较事实；它不是单一质量分数。

## 3. 用户样本身份

| ZIP | 字节 | SHA-256 |
|---|---:|---|
| 卫士重型防空车 改.zip | 26,033 | `A608DBB5F948DB2DD5C4F84F495610D0A8B6809071039D21B1E4DA7EDB88C3C0` |
| A10.zip | 12,764 | `FE1965C1039C0A9F90FE4D7C76DC25DEBA5DFD10EC43E60E2945155C24807A00` |
| htnk.zip | 17,162 | `C9764F4EF31485264AEF858B9B2001B6DB4BB9BC2816F88063284E4A5C979C94` |
| matas改.zip | 19,244 | `9968A9ACA8A2CC23FF7FF98D96D5E55DC475AEFB6626D503EA4630BC76BD8B6D` |
| 打击者C.zip | 72,559 | `A6400B5BF40C9745C73EE4A44226D691E7C453D75116352BB87D9A3B3DB53AEE` |
| 航母 改.zip | 68,144 | `686B5868C5CB81A6A0C21BDD2B20DC7073F05B3F3032A37687AFFEA584271B45` |
| 盟军空降坦克 改.zip | 18,196 | `99AF7F30C78A2F530A9F1351480F744407FE52E1AAD865145EBE2D05027CB486` |
| 曙光级.zip | 125,423 | `57B6D7523782C9B1F68D2D29752F85FE56F06B791B8DC2B84168F5141F50E499` |

总计读取 14 个 VXL section 文件和 1 个 MagicaVoxel VOX。全部 VXL header remap range 为 16-31、
NormalType 为 4；ZIP 没有携带 PAL，palette 身份来自用户说明并由 VXLSE III `unittem.pal` 源码路径复核。

## 4. VXL 结构统计

| 文件 | 尺寸 | occupied | 使用色号 | remap cells | remap 比例 |
|---|---:|---:|---:|---:|---:|
| hstk.vxl | 78×39×36 | 29,757 | 16 | 662 | 2.22% |
| hstktur.vxl | 35×27×26 | 6,058 | 14 | 92 | 1.52% |
| A10.vxl | 77×78×16 | 7,595 | 19 | 150 | 1.97% |
| htnk.vxl | 49×36×14 | 8,051 | 35 | 362 | 4.50% |
| htnktur.vxl | 52×18×15 | 2,027 | 16 | 140 | 6.91% |
| matas.vxl | 62×35×24 | 17,510 | 20 | 114 | 0.65% |
| matastur.vxl | 30×18×12 | 1,418 | 13 | 16 | 1.13% |
| tank2.vxl | 84×41×30 | 26,226 | 25 | 186 | 0.71% |
| tank2tur.vxl | 84×41×30 | 6,007 | 11 | 160 | 2.66% |
| carrier.vxl | 241×66×48 | 139,419 | 15 | 311 | 0.22% |
| gtnk.vxl | 42×30×18 | 8,165 | 18 | 93 | 1.14% |
| gtnktur.vxl | 31×16×6 | 1,461 | 12 | 64 | 4.38% |
| gtnkbarl.vxl | 26×4×4 | 234 | 4 | 9 | 3.85% |
| GAF-D.vxl | 208×82×77 | 143,090 | 26 | 1,090 | 0.76% |

remap 在所有样本中都属于小面积信号而非主体填充；最高的 htnktur 也只有 6.91%，大型 carrier 只有 0.22%。
这支持 `ExplicitMaskOnly`：remap 只能落在人工批准的小型标识/面板，不得承担阴影、底盘或全车主题色。

## 5. `unittem.pal` 复核与色带事实

- VXLSE III source commit：`fde704b01cb4de3adeaf1a151bbeee0994a04b99`。
- `vxlseiii14x/palettes/RA2/unittem.pal` SHA-256：
  `97F9E2ACE875D05F189201DB506CAABF1A30157BD7165DDE486E999034CDA3ED`。
- VXLSE III `Palette.pas` 对 768-byte Westwood PAL 的每通道值乘 4；`FormMain.pas` 的 RA2 palette action
  加载 `palettes/RA2/unittem.pal`。
- 同一源码中的 `ChangeRemappable` 明确处理 16-31。

样本的高频色带形成清晰梯度：

- 70-77：橄榄/黄灰 body ramp，例如 70 `#888870` 到 77 `#34341C`；
- 88-95：蓝灰 body ramp，例如 88 `#7C7C94` 到 95 `#282840`；
- 48-62：中性灰/深灰机械 ramp，例如 48 `#787878` 到 62 `#040404`；
- 16-31：红色 remap ramp，实际运行时由玩家色映射。

这些是样本证据，不是内置配色主题。Skill 只允许用它们解释“同一 palette family 内如何建立层次”，最终选择
仍必须由 active palette、用户颜色意图和 semantic mask 决定。

`打击者C.zip/tank.vox` 的嵌入 RGBA 提供了交叉验证：32,047 occupied cells、29 个使用色号，主色
`#404058`、`#34344C`、`#4C4C64` 与同包 VXL 在 `unittem.pal` 下的主色完全一致。VOX 与 VXL 的 index
编号不同，因此只能按 RGB/role 对齐，不能直接复制 index。

## 6. 从样本提炼的规则

### 6.1 地面单位

1. **先体积、后细节**：车体/炮塔采用一条相干 painted ramp；大面积内部填充保持主色，不对 interior 加噪声。
2. **同材质内分层**：htnk body 的 Top/Side/Under 平均明度约 100.1/81.3/86.3，gtnk 为
   68.9/61.1/46.7，证明顶/侧/底分层有效；但数值不是所有模型的硬模板。
3. **机械区单独处理**：履带、轮胎、轮井、底盘、排气、开口主要使用中性深灰，不应被 painted body ramp
   覆盖，也不应靠全轮廓黑边表达。
4. **炮塔/炮管连续**：炮塔与车体通常共享 palette family；炮管可用 neutral metal，但要保持挂接、方向和
   体积连续，不能把每个子件随机换色。
5. **顶面不是全亮蒙版**：hstk、matas、tank2 的顶面包含暗设备、甲板和结构色，全局 Top 均值并不总高于
   Interior。规则必须在材质和真实平面内执行。
6. **尺寸调节频率**：小型单位加强少数结构级差；大型单位压低单体素装饰频率，保留长装甲平面。

### 6.2 空中单位

1. A10 是 77×78×16 的宽而浅模型，surface/occupied 比例高；识别主要依赖俯视轮廓、翼根、机头/机尾、
   发动机与翼面关系，而不是坦克式的纵向侧壁渐变。
2. 机翼/机身采用安静的大色块和少量结构级差；前后缘、翼尖和翼根只在可读性需要时提亮/压暗。
3. 座舱、进气、排气、起落架和挂架必须是语义材质角色，不得用“明亮 body 色”猜成玻璃。
4. A10 的 Top/Side/Under 全局平均约 76.0/72.1/81.2，反证了“所有飞机底面必须全局最暗”。正确门禁是
   上下表面可区分、结构清楚、符合 source/palette 证据；较暗 underside 只能是默认技法，不是硬不变量。
5. 左右翼/发动机标识和 remap 在对称意图下保持对称，面积要小，在游戏比例仍可见即可。

### 6.3 大型水面/大型单位

carrier 与 GAF-D 分别有 139,419 与 143,090 occupied cells，但只使用 15 与 26 个色号，说明大型单位
不需要高频随机色。应先分 deck/hull/superstructure/opening/weapon/underside，维护长平面节奏，再加入稀疏
标识；不得自动认定几何 Top 就是最亮材质。

## 7. 公开教程、工具和模型研究

采用或核对的来源：

- [VXLSE III source mirror](https://github.com/hathlife/voxel_section_editor)：palette 加载、6-bit PAL 扩展、
  remap 16-31 和 RA2 `unittem.pal` 的直接源码事实。仓库没有清晰根许可证，因此只作只读事实核对，未复制
  代码或 palette。
- [VXLSE III Getting Started tutorial](https://ppmforums.com/topic-28292/user-info-getting-started-tutorial-for-vxle-iii/)：
  RA2/TS 模式、Air/Land 建模、0-255 palette、保留色、三视图和分 section 工作流。
- [Using MagicaVoxel + VXLSE](https://ppmforums.com/topic-47943/using-magicavoxel-vxlse-to-create-voxels/)：
  导入前需要匹配 Westwood palette，并在导入后检查颜色错配；支持本研究“不跨格式复制 index”的边界。
- [VXLSE III Auto Normalizer tips](https://ppmforums.com/topic-14062/user-info-auto-normalizer-tips/) 与
  [Finding Surface Normals From Voxels](https://www.ppmsite.com/sibgrapi2007_files/finding_surface_normals_from_voxels.pdf)：
  normals 是独立的光照/表面问题，不能把 painted colour rules 冒充 normals 修复。
- [MagicaVoxel VOX format](https://github.com/ephtracy/voxel-model/blob/master/MagicaVoxel-file-format-vox-extension.txt)：
  `XYZI`、嵌入 palette、scene graph 和 material chunks 的权威格式说明。
- [OpenRA trait documentation](https://docs.openra.net/en/release/traits/)：palette、player colour/remap index 和
  voxel lighting 是不同运行时输入，支持“remap/色盘/灯光不得混成一个规则”的边界。

公开模型采用 [PixVoxelAssets](https://github.com/tommyettinger/PixVoxelAssets) commit
`99dd229ab1cb39101fc0dda554883fc0dfe1be8b` 的 CC0 模型，仅在临时目录分析：

- `CU3/Tank_Large_W.vox`：10,081 cells / 15 indices；
- `CU3/Plane_Large_W.vox`：3,002 / 12；
- `CU3/Truck_Large_W.vox`：8,170 / 10；
- `Spaceship.vox`：9,124 / 3。

这些公开样本强化了“少量语义色、大块连续平面、单位类型适配”的方向，但其 palette 和渲染目标不是 RA2，
所以没有把其绝对 RGB 或全局 top/under 均值写成 RA2 模板。OpenRA/ra2 commit
`61e24e3c1d7b586aa55a86096d29e1559aa9b994` 的树中没有 `.vxl`，因此没有把需要零售游戏资产的下载流程冒充
公开模型研究。未发现明确许可证的社区 RA2 VXL 只作搜索线索，未下载、未纳入统计、未复制。

## 8. 内置 Skill 设计

新增 `ra2-voxel-colour-techniques`，domain 为 `voxel-colour`，v1 限定 Chat：

- 覆盖 ground / air / large surface 三类规则；
- 内置五个与色相无关的 technique policy；
- 强制 active palette、semantic masks、remap approval 和 geometry invariants；
- 使用 Blocked / NeedsReview / ReviewReady 多维事实，不使用单一 opaque score；
- 禁止 coordinates、mask 猜测、binary write、VXL/HVA/GameReady 宣称。

Chat 路由只在同时出现 voxel/VXL/VOX/体素与上色/着色/配色/palette/remap/材质等词时选择
`voxel-colour`。普通 VXL rules/art binding 仍走 `art-animation`。

专用 `Ra2VoxelStyleCompiler` 当前仍只读取 `VoxelStyles/compiler/COMPILER.md`。本研究没有绕过 4E Proposed
Contract 修改 compiler/cache/colourizer；将本 Skill 作为专用 compiler 的共享知识权威仍需在 4E 获批后按
单一计划、单一 colourizer 边界实现。

## 9. 剩余风险

- 样本分类部分依赖用户提供的文件集合；除 A10/航母等明确名称外，不从文件名断言游戏内单位角色。
- 统计没有渲染 Westwood normals、游戏灯光、阴影、地形背景或实际缩放，不能替代游戏内视觉验收。
- `unittem.pal` 已确认，但 snow/urban/theatre 变体可能改变 RGB；实际候选仍须绑定 active palette hash。
- 未得到公开 RA2 VXL 的明确资产许可证样本；本轮不以“公开下载”推断可再分发权。
- 新 Skill 可被通用 DeepSeek Chat 调用；专用 4E style compiler 接入仍是未实现项。

## 10. 验证

```text
dotnet test RA2IniEditor.Tests --filter Ra2AgentSkillCatalogTests: 16/16 Passed
Agent Skill bundled discovery/count/manifest: Passed
voxel-colour narrow routing + ordinary VXL art routing preservation: Passed
Chat prompt injection + zero edit tools: Passed
git diff --check: Passed
```

聚焦测试命令同时成功编译 Core、Infrastructure、Application、AssetHost、IDE 与 Tests；Tests 项目报告一条既有
nullable warning `BuiltInFieldRegistryPackLoaderTests.cs(1983) CS8602`。本轮没有运行全量测试、clean package、
真实 DeepSeek、WPF 或游戏内视觉验收。
