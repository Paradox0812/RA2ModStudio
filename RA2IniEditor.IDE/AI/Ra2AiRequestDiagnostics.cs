namespace RA2IniEditor.IDE.AI;

/// <summary>
/// 单次 DeepSeek 请求产生的非敏感观测事实；不持久化、不包含正文或 endpoint。
/// </summary>
internal sealed class Ra2AiRequestDiagnostics
{
    public Ra2AiRequestDiagnostics(
        string requestId,
        string modelId,
        int promptCharacterCount,
        TimeSpan? timeToHeaders,
        TimeSpan? timeToFirstContent,
        TimeSpan totalDuration,
        int contentDeltaCount,
        int contentCharacterCount,
        int? httpStatusCode)
    {
        if (string.IsNullOrWhiteSpace(requestId))
            throw new ArgumentException("Request id is required.", nameof(requestId));
        if (!DeepSeekRa2AiClientOptions.IsSupportedModelId(modelId))
            throw new ArgumentException("Model id is not supported.", nameof(modelId));
        if (promptCharacterCount < 0)
            throw new ArgumentOutOfRangeException(nameof(promptCharacterCount));
        if (totalDuration < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(totalDuration));
        if (contentDeltaCount < 0)
            throw new ArgumentOutOfRangeException(nameof(contentDeltaCount));
        if (contentCharacterCount < 0)
            throw new ArgumentOutOfRangeException(nameof(contentCharacterCount));

        RequestId = requestId;
        ModelId = modelId.Trim();
        PromptCharacterCount = promptCharacterCount;
        TimeToHeaders = timeToHeaders;
        TimeToFirstContent = timeToFirstContent;
        TotalDuration = totalDuration;
        ContentDeltaCount = contentDeltaCount;
        ContentCharacterCount = contentCharacterCount;
        HttpStatusCode = httpStatusCode;
    }

    public string RequestId { get; }

    public string ModelId { get; }

    public int PromptCharacterCount { get; }

    public TimeSpan? TimeToHeaders { get; }

    public TimeSpan? TimeToFirstContent { get; }

    public TimeSpan TotalDuration { get; }

    public int ContentDeltaCount { get; }

    public int ContentCharacterCount { get; }

    public int? HttpStatusCode { get; }
}
