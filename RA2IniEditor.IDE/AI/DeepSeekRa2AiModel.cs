namespace RA2IniEditor.IDE.AI;

/// <summary>
/// 产品允许用户显式选择的 DeepSeek 模型。
/// </summary>
internal enum DeepSeekRa2AiModel
{
    V4Flash = 0,
    V4Pro = 1
}

/// <summary>
/// 供 UI 绑定使用的模型显示项；API 标识只由目录维护。
/// </summary>
internal sealed record DeepSeekRa2AiModelOption(
    DeepSeekRa2AiModel Value,
    string DisplayName,
    string ApiModelId);

/// <summary>
/// DeepSeek 模型显示名称与 API 标识的唯一映射来源。
/// </summary>
internal static class DeepSeekRa2AiModelCatalog
{
    private static readonly IReadOnlyList<DeepSeekRa2AiModelOption> ModelOptions =
    [
        new(DeepSeekRa2AiModel.V4Flash, "DeepSeek V4 Flash", "deepseek-v4-flash"),
        new(DeepSeekRa2AiModel.V4Pro, "DeepSeek V4 Pro", "deepseek-v4-pro")
    ];

    public static IReadOnlyList<DeepSeekRa2AiModelOption> Options => ModelOptions;

    public static DeepSeekRa2AiModel Default => DeepSeekRa2AiModel.V4Flash;

    public static string GetApiModelId(DeepSeekRa2AiModel model)
        => GetOption(model).ApiModelId;

    public static DeepSeekRa2AiModelOption GetOption(DeepSeekRa2AiModel model)
        => model switch
        {
            DeepSeekRa2AiModel.V4Flash => ModelOptions[0],
            DeepSeekRa2AiModel.V4Pro => ModelOptions[1],
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, "Unsupported DeepSeek model.")
        };
}
