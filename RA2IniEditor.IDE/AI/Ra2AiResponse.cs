namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiResponse
{
    private const string TimeoutMessage = "DeepSeek provider request timed out.";
    private const string MissingConfigurationMessage =
        "DeepSeek configuration is missing or invalid.";
    private const string AuthoringToolNotInvokedMessage =
        "DeepSeek did not invoke the required structured-edit tool.";

    private Ra2AiResponse(
        Ra2AiResponseKind kind,
        string text = "",
        string? errorMessage = null,
        Ra2AiStreamFinishKind finishKind = Ra2AiStreamFinishKind.Unknown,
        Ra2AiFailureKind failureKind = Ra2AiFailureKind.None,
        Ra2AiRequestDiagnostics? diagnostics = null,
        IReadOnlyList<Ra2AiToolCall>? toolCalls = null,
        string? localRejectionMessage = null)
    {
        Ra2AiToolCall[] toolCallArray = toolCalls?.ToArray() ?? [];
        bool isToolCallResponse = kind == Ra2AiResponseKind.ToolCalls;
        if (isToolCallResponse != (toolCallArray.Length > 0) ||
            (isToolCallResponse && finishKind != Ra2AiStreamFinishKind.ToolCalls) ||
            (!isToolCallResponse && toolCallArray.Length != 0))
        {
            throw new ArgumentException("AI response tool-call state is inconsistent.");
        }
        bool isLocalRejection = kind == Ra2AiResponseKind.LocalRejection;
        if (isLocalRejection != !string.IsNullOrWhiteSpace(localRejectionMessage) ||
            (isLocalRejection && (failureKind != Ra2AiFailureKind.None || errorMessage is not null)))
        {
            throw new ArgumentException("AI local-rejection state is inconsistent.");
        }

        Kind = kind;
        Text = text ?? string.Empty;
        ErrorMessage = errorMessage;
        FinishKind = finishKind;
        FailureKind = failureKind;
        Diagnostics = diagnostics;
        ToolCalls = Array.AsReadOnly(toolCallArray);
        LocalRejectionMessage = localRejectionMessage;
    }

    public Ra2AiResponseKind Kind { get; }

    public string Text { get; }

    public string? ErrorMessage { get; }

    public Ra2AiStreamFinishKind FinishKind { get; }

    /// <summary>获取 transport 生成的安全失败分类；旧生产者可保留为 None。</summary>
    public Ra2AiFailureKind FailureKind { get; }

    public Ra2AiRequestDiagnostics? Diagnostics { get; }

    public bool IsSuccess => Kind == Ra2AiResponseKind.Success;

    public bool IsSuccessfulTerminal
        => Kind is Ra2AiResponseKind.Success or Ra2AiResponseKind.ToolCalls;

    public IReadOnlyList<Ra2AiToolCall> ToolCalls { get; }

    /// <summary>获取只由本地 pipeline 生成、可安全直接显示的拒绝原因。</summary>
    public string? LocalRejectionMessage { get; }

    public static Ra2AiResponse CreateSuccess(
        string text,
        Ra2AiRequestDiagnostics? diagnostics = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Successful response text is required.", nameof(text));

        return new Ra2AiResponse(
            Ra2AiResponseKind.Success,
            text,
            finishKind: Ra2AiStreamFinishKind.Stop,
            diagnostics: diagnostics);
    }

    public static Ra2AiResponse CreateToolCalls(
        IEnumerable<Ra2AiToolCall> toolCalls,
        string text = "",
        Ra2AiRequestDiagnostics? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(toolCalls);
        Ra2AiToolCall[] callArray = toolCalls.ToArray();
        if (callArray.Length == 0 || callArray.Any(call => call is null))
            throw new ArgumentException("A tool-call response requires complete calls.", nameof(toolCalls));

        return new Ra2AiResponse(
            Ra2AiResponseKind.ToolCalls,
            text ?? string.Empty,
            finishKind: Ra2AiStreamFinishKind.ToolCalls,
            diagnostics: diagnostics,
            toolCalls: callArray);
    }

    public static Ra2AiResponse CreateCancelled(
        string partialText = "",
        Ra2AiRequestDiagnostics? diagnostics = null)
        => new(
            Ra2AiResponseKind.Cancelled,
            partialText ?? string.Empty,
            diagnostics: diagnostics);

    public static Ra2AiResponse CreateAuthoringToolNotInvoked(
        string providerText,
        Ra2AiRequestDiagnostics? diagnostics = null)
        => new(
            Ra2AiResponseKind.AuthoringToolNotInvoked,
            providerText ?? string.Empty,
            AuthoringToolNotInvokedMessage,
            finishKind: Ra2AiStreamFinishKind.Stop,
            diagnostics: diagnostics);

    public static Ra2AiResponse CreateTimeout(
        string partialText,
        Ra2AiFailureKind failureKind,
        Ra2AiRequestDiagnostics? diagnostics = null)
    {
        if (failureKind is not (Ra2AiFailureKind.TotalTimeout
            or Ra2AiFailureKind.StreamingIdleTimeout
            or Ra2AiFailureKind.Unknown))
        {
            throw new ArgumentException("Timeout failure kind is invalid.", nameof(failureKind));
        }

        return new Ra2AiResponse(
            Ra2AiResponseKind.Timeout,
            partialText ?? string.Empty,
            TimeoutMessage,
            failureKind: failureKind,
            diagnostics: diagnostics);
    }

    public static Ra2AiResponse CreateMissingConfiguration(
        Ra2AiRequestDiagnostics? diagnostics = null)
        => new(
            Ra2AiResponseKind.MissingConfiguration,
            errorMessage: MissingConfigurationMessage,
            failureKind: Ra2AiFailureKind.MissingConfiguration,
            diagnostics: diagnostics);

    public static Ra2AiResponse CreateProviderFailure(
        Ra2AiFailureKind failureKind,
        string safeErrorMessage,
        Ra2AiStreamFinishKind finishKind = Ra2AiStreamFinishKind.Unknown,
        Ra2AiRequestDiagnostics? diagnostics = null)
    {
        if (failureKind == Ra2AiFailureKind.None)
            throw new ArgumentException("Provider failure kind is required.", nameof(failureKind));
        if (string.IsNullOrWhiteSpace(safeErrorMessage))
            throw new ArgumentException("Safe provider error message is required.", nameof(safeErrorMessage));
        if (finishKind == Ra2AiStreamFinishKind.Stop)
            throw new ArgumentException("Provider failure cannot have a Stop finish kind.", nameof(finishKind));

        return new Ra2AiResponse(
            Ra2AiResponseKind.ProviderError,
            errorMessage: safeErrorMessage,
            finishKind: finishKind,
            failureKind: failureKind,
            diagnostics: diagnostics);
    }

    public static Ra2AiResponse CreateLocalRejection(
        string safeUserMessage,
        Ra2AiRequestDiagnostics? diagnostics = null)
    {
        if (string.IsNullOrWhiteSpace(safeUserMessage))
            throw new ArgumentException("A safe local-rejection message is required.", nameof(safeUserMessage));

        return new Ra2AiResponse(
            Ra2AiResponseKind.LocalRejection,
            diagnostics: diagnostics,
            localRejectionMessage: safeUserMessage.Trim());
    }

    public static Ra2AiResponse CreateIncomplete(
        string partialText,
        Ra2AiStreamFinishKind finishKind,
        Ra2AiFailureKind failureKind = Ra2AiFailureKind.None,
        string? safeErrorMessage = null,
        Ra2AiRequestDiagnostics? diagnostics = null)
    {
        string normalizedText = partialText ?? string.Empty;
        if (finishKind == Ra2AiStreamFinishKind.Stop)
            throw new ArgumentException("Incomplete response cannot have a Stop finish kind.", nameof(finishKind));
        if (normalizedText.Length == 0 && finishKind == Ra2AiStreamFinishKind.Unknown)
        {
            throw new ArgumentException(
                "Incomplete response requires partial text or a non-unknown finish kind.",
                nameof(partialText));
        }

        return new Ra2AiResponse(
            Ra2AiResponseKind.Incomplete,
            normalizedText,
            safeErrorMessage,
            finishKind,
            failureKind,
            diagnostics);
    }

    internal Ra2AiResponse WithDiagnostics(Ra2AiRequestDiagnostics diagnostics)
        => new(
            Kind,
            Text,
            ErrorMessage,
            FinishKind,
            FailureKind,
            diagnostics,
            ToolCalls,
            LocalRejectionMessage);
}
