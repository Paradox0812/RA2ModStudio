using System.Text.Json;
using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.AI;

/// <summary>把不可信 provider 参数转换为绑定本地快照的受限编辑计划。</summary>
internal sealed class Ra2AiAuthoringToolAdapter
{
    private const int MaximumSectionLength = 256;
    private const int MaximumKeyLength = 256;
    private const int MaximumValueLength = 8192;
    private const int MaximumJsonDepth = 32;

    private static readonly HashSet<string> RootProperties =
        new(StringComparer.Ordinal) { "outcome", "summary", "operations", "message" };
    private static readonly HashSet<string> OperationProperties =
        new(StringComparer.Ordinal) { "kind", "section", "key", "value" };

    public Ra2AiEditPlanCreationResult TryCreatePlan(
        Ra2AiToolCall toolCall,
        Ra2AiAuthoringRequestContext requestContext)
    {
        ArgumentNullException.ThrowIfNull(toolCall);
        ArgumentNullException.ThrowIfNull(requestContext);

        if (!string.Equals(
                toolCall.Name,
                Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName,
                StringComparison.Ordinal))
        {
            return Failed(
                Ra2AiEditProposalFailureKind.UnsupportedTool,
                "模型返回了当前版本不支持的编辑工具。");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(
                toolCall.ArgumentsJson,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth
                });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return InvalidJson();

            Ra2AiEditPlanCreationResult? propertyFailure =
                ValidateProperties(root, RootProperties);
            if (propertyFailure is not null)
                return propertyFailure;

            if (!TryReadBoundedString(
                    root,
                    "outcome",
                    maximumLength: 64,
                    allowEmpty: false,
                    out string outcome))
            {
                return InvalidJson();
            }

            if (string.Equals(outcome, "needs_clarification", StringComparison.Ordinal))
            {
                if (root.TryGetProperty("summary", out _) ||
                    root.TryGetProperty("operations", out _) ||
                    !TryReadBoundedString(
                        root,
                        "message",
                        Ra2IniEditPlan.MaximumSummaryLength,
                        allowEmpty: false,
                        out string clarificationMessage))
                {
                    return InvalidJson();
                }

                return Ra2AiEditPlanCreationResult.Clarification(clarificationMessage);
            }

            if (!string.Equals(outcome, "proposal", StringComparison.Ordinal) ||
                root.TryGetProperty("message", out _))
            {
                return InvalidJson();
            }

            if (!TryReadBoundedString(
                    root,
                    "summary",
                    Ra2IniEditPlan.MaximumSummaryLength,
                    allowEmpty: false,
                    out string summary) ||
                !root.TryGetProperty("operations", out JsonElement operationsElement) ||
                operationsElement.ValueKind != JsonValueKind.Array)
            {
                return InvalidJson();
            }

            int operationCount = operationsElement.GetArrayLength();
            if (operationCount is < 1 or > Ra2IniEditPlan.MaximumOperationCount)
            {
                return Failed(
                    Ra2AiEditProposalFailureKind.InvalidOperation,
                    "结构化修改操作数量超出允许范围。");
            }

            List<Ra2IniEditOperation> operations = new(operationCount);
            foreach (JsonElement operationElement in operationsElement.EnumerateArray())
            {
                Ra2AiEditPlanCreationResult? operationFailure =
                    TryCreateOperation(operationElement, operations);
                if (operationFailure is not null)
                    return operationFailure;
            }

            Ra2AuthoringSnapshot snapshot = requestContext.Snapshot;
            return Ra2AiEditPlanCreationResult.FromPlan(new Ra2IniEditPlan(
                Guid.NewGuid(),
                snapshot.DocumentId,
                snapshot.EditRevision,
                snapshot.FieldRegistry.Revision,
                operations,
                summary,
                Ra2AiAuthoringToolCatalog.TrustedPlanOrigin));
        }
        catch (JsonException)
        {
            return InvalidJson();
        }
        catch (ArgumentException)
        {
            return Failed(
                Ra2AiEditProposalFailureKind.InvalidOperation,
                "结构化修改参数不符合当前编辑计划约束。");
        }
    }

    private static Ra2AiEditPlanCreationResult? TryCreateOperation(
        JsonElement element,
        ICollection<Ra2IniEditOperation> operations)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return InvalidJson();

        Ra2AiEditPlanCreationResult? propertyFailure =
            ValidateProperties(element, OperationProperties);
        if (propertyFailure is not null)
            return propertyFailure;

        if (!TryReadBoundedString(element, "kind", 64, allowEmpty: false, out string kind) ||
            !TryReadBoundedString(
                element,
                "section",
                MaximumSectionLength,
                allowEmpty: false,
                out string section) ||
            !TryReadBoundedString(
                element,
                "key",
                MaximumKeyLength,
                allowEmpty: false,
                out string key) ||
            !TryReadBoundedString(
                element,
                "value",
                MaximumValueLength,
                allowEmpty: true,
                out string value))
        {
            return InvalidJson();
        }

        Ra2IniEditOperationKind operationKind;
        if (string.Equals(kind, "upsert_field", StringComparison.Ordinal))
            operationKind = Ra2IniEditOperationKind.UpsertField;
        else if (string.Equals(kind, "replace_field_value", StringComparison.Ordinal))
            operationKind = Ra2IniEditOperationKind.ReplaceFieldValue;
        else
        {
            return Failed(
                Ra2AiEditProposalFailureKind.InvalidOperation,
                "模型返回了当前版本不支持的编辑操作。");
        }

        operations.Add(new Ra2IniEditOperation(operationKind, section, key, value));
        return null;
    }

    private static Ra2AiEditPlanCreationResult? ValidateProperties(
        JsonElement element,
        IReadOnlySet<string> allowedProperties)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!seen.Add(property.Name))
            {
                return Failed(
                    Ra2AiEditProposalFailureKind.DuplicateArgumentProperty,
                    "结构化修改参数包含重复属性。");
            }

            if (!allowedProperties.Contains(property.Name))
            {
                return Failed(
                    Ra2AiEditProposalFailureKind.UnknownArgumentProperty,
                    "结构化修改参数包含当前版本不支持的属性。");
            }
        }

        return null;
    }

    private static bool TryReadBoundedString(
        JsonElement parent,
        string propertyName,
        int maximumLength,
        bool allowEmpty,
        out string value)
    {
        value = string.Empty;
        if (!parent.TryGetProperty(propertyName, out JsonElement element) ||
            element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        string raw = element.GetString() ?? string.Empty;
        if ((!allowEmpty && string.IsNullOrWhiteSpace(raw)) ||
            raw.Length > maximumLength ||
            raw.Contains('\0'))
        {
            return false;
        }

        value = allowEmpty ? raw : raw.Trim();
        return true;
    }

    private static Ra2AiEditPlanCreationResult InvalidJson()
        => Failed(
            Ra2AiEditProposalFailureKind.InvalidArgumentsJson,
            "模型返回的结构化修改参数格式无效。");

    private static Ra2AiEditPlanCreationResult Failed(
        Ra2AiEditProposalFailureKind failureKind,
        string message)
        => Ra2AiEditPlanCreationResult.Failed(failureKind, message);

}
