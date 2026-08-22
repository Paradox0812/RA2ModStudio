using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.ViewModels.FieldAnnotations;

internal sealed class Ra2SectionKindDisplayNameProvider
{
    public string GetDisplayName(Ra2SectionKind? sectionKind)
        => sectionKind is null ? "全部类型" : GetDisplayName(sectionKind.Value);

    public string GetDisplayName(Ra2SectionKind sectionKind)
    {
        return sectionKind switch
        {
            Ra2SectionKind.Unknown => "未知",
            Ra2SectionKind.Global => "全局",
            Ra2SectionKind.Techno => "科技对象",
            Ra2SectionKind.Unit => "单位",
            Ra2SectionKind.Infantry => "步兵",
            Ra2SectionKind.Vehicle => "战车类",
            Ra2SectionKind.Aircraft => "飞行器",
            Ra2SectionKind.Building => "建筑类",
            Ra2SectionKind.Weapon => "武器",
            Ra2SectionKind.Projectile => "抛射体",
            Ra2SectionKind.Warhead => "弹头",
            Ra2SectionKind.Animation => "动画",
            Ra2SectionKind.VoxelAnimation => "体素动画",
            Ra2SectionKind.SuperWeapon => "超级武器",
            Ra2SectionKind.Terrain => "地形",
            Ra2SectionKind.Overlay => "覆盖层",
            Ra2SectionKind.Smudge => "污渍",
            Ra2SectionKind.Particle => "粒子",
            Ra2SectionKind.ParticleSystem => "粒子系统",
            Ra2SectionKind.Sound => "音效",
            Ra2SectionKind.TaskForce => "特遣队",
            Ra2SectionKind.Script => "脚本",
            Ra2SectionKind.TeamType => "队伍类型",
            Ra2SectionKind.AITrigger => "AI 触发",
            Ra2SectionKind.AI => "AI",
            Ra2SectionKind.Country => "阵营",
            Ra2SectionKind.ArtObject => "美术对象",
            Ra2SectionKind.Side => "阵营大类",
            Ra2SectionKind.AttachEffect => "附加效果",
            Ra2SectionKind.Shield => "护盾",
            Ra2SectionKind.LaserTrail => "激光尾迹",
            Ra2SectionKind.DigitalDisplay => "数字显示",
            Ra2SectionKind.Banner => "横幅显示",
            Ra2SectionKind.Insignia => "徽章/标识",
            Ra2SectionKind.Radiation => "辐射",
            Ra2SectionKind.Eva => "EVA 事件",
            Ra2SectionKind.Tiberium => "资源/矿石",
            Ra2SectionKind.MiscObject => "其他对象",
            _ => sectionKind.ToString()
        };
    }

    public string GetDisplayName(string sectionKind)
        => Enum.TryParse(sectionKind, ignoreCase: true, out Ra2SectionKind parsed)
            ? GetDisplayName(parsed)
            : sectionKind;
}
