namespace RA2IniEditor.IDE.ViewModels.FieldBrowser;

internal sealed class Ra2AddPropertyValueHintProvider
{
    public string GetHint(Ra2AddPropertyItemViewModel? item)
    {
        if (item is null)
            return "请手动输入字段值。";

        return item.TypeDisplay switch
        {
            "Integer" => "整数。示例：400",
            "Float" => "浮点数。示例：1.5",
            "Percent" => "百分比。示例：100%",
            "Boolean" => "布尔值。常用：yes/no",
            "Enum" when string.Equals(item.Key, "Armor", StringComparison.OrdinalIgnoreCase) =>
                "枚举值。常用：none, flak, plate, light, medium, heavy, wood, steel, concrete。",
            "Enum" => "枚举值。请手动输入允许的值。",
            "Reference" => $"引用字段。目标类型：{GetReferenceTarget(item)}",
            "MultiSelect" => "列表字段。多个值用逗号分隔。",
            "Color" => "颜色字段。示例：255,0,0",
            "ColorDefinition" => "颜色定义字段。示例：255,0,0",
            "Coordinate" => "坐标字段。示例：0,0",
            "Verses" => "伤害倍率字段。示例：100%,100%,100%。",
            _ => "请手动输入字段值。"
        };
    }

    private static string GetReferenceTarget(Ra2AddPropertyItemViewModel item)
    {
        if (item.Key.Contains("Primary", StringComparison.OrdinalIgnoreCase) ||
            item.Key.Contains("Secondary", StringComparison.OrdinalIgnoreCase) ||
            item.Key.Contains("Weapon", StringComparison.OrdinalIgnoreCase))
        {
            return "Weapon";
        }

        if (string.Equals(item.Key, "Projectile", StringComparison.OrdinalIgnoreCase))
            return "Projectile";

        if (string.Equals(item.Key, "Warhead", StringComparison.OrdinalIgnoreCase))
            return "Warhead";

        return "Section ID";
    }
}
