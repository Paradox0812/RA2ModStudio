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
    private static readonly HashSet<string> TemplateRootProperties =
        new(StringComparer.Ordinal) { "outcome", "template_id", "template_version", "arguments", "message" };
    private static readonly HashSet<string> OperationProperties =
        new(StringComparer.Ordinal) { "kind", "section", "key", "value" };
    private static readonly HashSet<string> TemplateArgumentProperties =
        new(StringComparer.Ordinal) { "name", "value" };

    private readonly IRa2AutomationCapabilityGateway _gateway;

    public Ra2AiAuthoringToolAdapter()
        : this(new Ra2AutomationCapabilityGateway())
    {
    }

    internal Ra2AiAuthoringToolAdapter(IRa2AutomationCapabilityGateway gateway)
        => _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));

    public Ra2AiEditPlanCreationResult TryCreatePlan(
        Ra2AiToolCall toolCall,
        Ra2AiAuthoringRequestContext requestContext)
    {
        ArgumentNullException.ThrowIfNull(toolCall);
        ArgumentNullException.ThrowIfNull(requestContext);

        if (string.Equals(
                toolCall.Name,
                Ra2AiAuthoringToolCatalog.ExpandIniContentTemplateToolName,
                StringComparison.Ordinal))
        {
            return TryCreateTemplatePlan(toolCall, requestContext);
        }

        if (!string.Equals(toolCall.Name, Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName, StringComparison.Ordinal))
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
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth
                });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return InvalidJson("结构化修改参数根必须是 JSON 对象。");

            Ra2AiEditPlanCreationResult? propertyFailure =
                ValidateProperties(root, RootProperties);
            if (propertyFailure is not null)
                return propertyFailure;

            if (!TryResolveOutcome(root, out string outcome, out Ra2AiEditPlanCreationResult? outcomeFailure))
                return outcomeFailure!;

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
                    return InvalidJson("结构化修改的 clarification 参数形态无效。");
                }

                return Ra2AiEditPlanCreationResult.Clarification(clarificationMessage);
            }

            if (!string.Equals(outcome, "proposal", StringComparison.Ordinal))
            {
                return InvalidJson("结构化修改的 outcome 不是 proposal 或 needs_clarification。");
            }

            if (!TryValidateOptionalBoundedString(
                    root,
                    "message",
                    Ra2IniEditPlan.MaximumSummaryLength))
                return InvalidJson("结构化修改 proposal 的 message 必须是有界字符串。");

            string summary = "AI 结构化修改建议";
            if (root.TryGetProperty("summary", out _) &&
                !TryReadBoundedString(
                    root,
                    "summary",
                    Ra2IniEditPlan.MaximumSummaryLength,
                    allowEmpty: false,
                    out summary))
            {
                return InvalidJson("结构化修改的 summary 必须是非空字符串。");
            }

            if (!root.TryGetProperty("operations", out JsonElement operationsElement))
                return InvalidJson("结构化修改缺少 operations。");

            JsonElement[] operationElements;
            if (operationsElement.ValueKind == JsonValueKind.Array)
                operationElements = operationsElement.EnumerateArray().ToArray();
            else if (operationsElement.ValueKind == JsonValueKind.Object)
                operationElements = [operationsElement];
            else
                return InvalidJson("结构化修改的 operations 必须是数组或单个操作对象。");

            int operationCount = operationElements.Length;
            if (operationCount is < 1 or > Ra2IniEditPlan.MaximumOperationCount)
            {
                return Failed(
                    Ra2AiEditProposalFailureKind.InvalidOperation,
                    "结构化修改操作数量超出允许范围。");
            }

            List<Ra2IniEditOperation> operations = new(operationCount);
            foreach (JsonElement operationElement in operationElements)
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
            return InvalidJson("模型返回的结构化修改参数不是有效 JSON。");
        }
        catch (ArgumentException)
        {
            return Failed(
                Ra2AiEditProposalFailureKind.InvalidOperation,
                "结构化修改参数不符合当前编辑计划约束。");
        }
    }

    private Ra2AiEditPlanCreationResult TryCreateTemplatePlan(
        Ra2AiToolCall toolCall,
        Ra2AiAuthoringRequestContext requestContext)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(
                toolCall.ArgumentsJson,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth
                });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return InvalidJson("内容模板参数根必须是 JSON 对象。");

            Ra2AiEditPlanCreationResult? propertyFailure = ValidateProperties(root, TemplateRootProperties);
            if (propertyFailure is not null)
                return propertyFailure;
            if (!TryResolveTemplateOutcome(root, out string outcome, out Ra2AiEditPlanCreationResult? outcomeFailure))
                return outcomeFailure!;

            if (string.Equals(outcome, "needs_clarification", StringComparison.Ordinal))
            {
                if (!TryReadBoundedString(
                        root,
                        "message",
                        Ra2IniEditPlan.MaximumSummaryLength,
                        allowEmpty: false,
                        out string message))
                    return InvalidJson("内容模板 clarification 必须包含非空有界 message。");

                // Clarification never creates a plan. Non-strict providers may echo proposal-shaped
                // fields beside the message; keeping them inert is safer than rejecting the user-facing
                // clarification or attempting to execute an explicitly non-proposal outcome.
                return Ra2AiEditPlanCreationResult.Clarification(message);
            }

            if (!string.Equals(outcome, "proposal", StringComparison.Ordinal))
                return InvalidJson("内容模板的 outcome 不是 proposal 或 needs_clarification。");
            if (!TryValidateOptionalBoundedString(
                    root,
                    "message",
                    Ra2IniEditPlan.MaximumSummaryLength))
                return InvalidJson("内容模板 proposal 的 message 必须是有界字符串。");
            if (!TryReadBoundedString(root, "template_id", 128, allowEmpty: false, out string templateId))
                return InvalidJson("内容模板 proposal 缺少有效 template_id。");
            if (!TryReadPositiveTemplateVersion(root, out int templateVersion))
                return InvalidJson("内容模板 proposal 缺少有效正整数 template_version。");
            if (!root.TryGetProperty("arguments", out JsonElement argumentsElement))
                return InvalidJson("内容模板 proposal 缺少 arguments。");

            Ra2AiEditPlanCreationResult? argumentFailure =
                TryReadTemplateArguments(argumentsElement, out List<Ra2AutomationTemplateArgument> arguments);
            if (argumentFailure is not null)
                return argumentFailure;

            Ra2AutomationTemplateExpansionResult expansion = _gateway.ExpandTemplate(
                requestContext.Snapshot.ToAutomationSnapshot(),
                new Ra2AutomationTemplateExpansionRequest(templateId, templateVersion, arguments));
            if (expansion.Succeeded)
                return Ra2AiEditPlanCreationResult.FromPlan(expansion.Plan!);

            return Failed(
                MapTemplateFailure(expansion.FailureKind),
                LocalizeTemplateFailure(expansion.FailureKind, expansion.Message));
        }
        catch (JsonException)
        {
            return InvalidJson("内容模板参数不是有效 JSON。");
        }
        catch (ArgumentException)
        {
            return Failed(Ra2AiEditProposalFailureKind.InvalidOperation, "模板调用参数不符合本地契约。 ");
        }
    }

    private static Ra2AiEditProposalFailureKind MapTemplateFailure(
        Ra2AutomationTemplateExpansionFailureKind failureKind)
        => failureKind switch
        {
            Ra2AutomationTemplateExpansionFailureKind.MissingRequiredArgument => Ra2AiEditProposalFailureKind.MissingArguments,
            Ra2AutomationTemplateExpansionFailureKind.UnknownArgument => Ra2AiEditProposalFailureKind.UnknownArgumentProperty,
            Ra2AutomationTemplateExpansionFailureKind.DuplicateArgument => Ra2AiEditProposalFailureKind.DuplicateArgumentProperty,
            Ra2AutomationTemplateExpansionFailureKind.Canceled => Ra2AiEditProposalFailureKind.PreviewCancelled,
            Ra2AutomationTemplateExpansionFailureKind.InvalidArguments => Ra2AiEditProposalFailureKind.InvalidOperation,
            _ => Ra2AiEditProposalFailureKind.TemplateExpansionRejected
        };

    private static bool TryResolveTemplateOutcome(
        JsonElement root,
        out string outcome,
        out Ra2AiEditPlanCreationResult? failure)
    {
        if (root.TryGetProperty("outcome", out _))
        {
            if (TryReadBoundedString(root, "outcome", 64, allowEmpty: false, out outcome))
            {
                failure = null;
                return true;
            }

            outcome = string.Empty;
            failure = InvalidJson("内容模板的 outcome 必须是非空字符串。");
            return false;
        }

        bool hasProposalPayload = root.TryGetProperty("template_id", out _) ||
                                  root.TryGetProperty("template_version", out _) ||
                                  root.TryGetProperty("arguments", out _);
        bool hasMessage = root.TryGetProperty("message", out _);
        if (!hasProposalPayload && !hasMessage)
        {
            outcome = string.Empty;
            failure = InvalidJson("内容模板缺少 outcome，且无法唯一判定 proposal 或 clarification。");
            return false;
        }

        outcome = hasProposalPayload ? "proposal" : "needs_clarification";
        failure = null;
        return true;
    }

    private static bool TryReadPositiveTemplateVersion(JsonElement root, out int version)
    {
        version = 0;
        if (!root.TryGetProperty("template_version", out JsonElement element))
            return false;
        if (element.ValueKind == JsonValueKind.Number)
            return element.TryGetInt32(out version) && version > 0;
        return element.ValueKind == JsonValueKind.String &&
               int.TryParse(element.GetString(), System.Globalization.NumberStyles.None,
                   System.Globalization.CultureInfo.InvariantCulture, out version) &&
               version > 0;
    }

    private static Ra2AiEditPlanCreationResult? TryReadTemplateArguments(
        JsonElement element,
        out List<Ra2AutomationTemplateArgument> arguments)
    {
        arguments = [];
        if (element.ValueKind == JsonValueKind.Array)
        {
            int count = element.GetArrayLength();
            Ra2AiEditPlanCreationResult? countFailure = ValidateTemplateArgumentCount(count);
            if (countFailure is not null)
                return countFailure;

            arguments = new List<Ra2AutomationTemplateArgument>(count);
            foreach (JsonElement argumentElement in element.EnumerateArray())
            {
                if (argumentElement.ValueKind != JsonValueKind.Object)
                    return InvalidJson("内容模板 arguments 数组项必须是 name/value 对象。");
                Ra2AiEditPlanCreationResult? propertyFailure =
                    ValidateProperties(argumentElement, TemplateArgumentProperties);
                if (propertyFailure is not null)
                    return propertyFailure;
                if (!TryReadBoundedString(argumentElement, "name", 256, allowEmpty: false, out string name) ||
                    !argumentElement.TryGetProperty("value", out JsonElement valueElement) ||
                    !TryReadBoundedTemplateScalar(valueElement, out string value))
                {
                    return InvalidJson("内容模板参数 name 必须是字符串，value 必须是字符串、数字或布尔值。");
                }

                arguments.Add(new Ra2AutomationTemplateArgument(name, value));
            }

            return null;
        }

        if (element.ValueKind != JsonValueKind.Object)
            return InvalidJson("内容模板 arguments 必须是参数对象或 name/value 数组。");

        JsonProperty[] properties = element.EnumerateObject().ToArray();
        Ra2AiEditPlanCreationResult? objectCountFailure = ValidateTemplateArgumentCount(properties.Length);
        if (objectCountFailure is not null)
            return objectCountFailure;

        arguments = new List<Ra2AutomationTemplateArgument>(properties.Length);
        foreach (JsonProperty property in properties)
        {
            if (string.IsNullOrWhiteSpace(property.Name) || property.Name.Length > 256 || property.Name.Contains('\0') ||
                !TryReadBoundedTemplateScalar(property.Value, out string value))
            {
                return InvalidJson("内容模板参数对象只允许有界名称和字符串、数字或布尔标量值。");
            }

            arguments.Add(new Ra2AutomationTemplateArgument(property.Name, value));
        }

        return null;
    }

    private static Ra2AiEditPlanCreationResult? ValidateTemplateArgumentCount(int count)
        => count is < 1 or > Ra2AutomationTemplateExpansionRequest.MaximumArgumentCount
            ? Failed(Ra2AiEditProposalFailureKind.InvalidOperation, "模板参数数量超出允许范围。")
            : null;

    private static bool TryReadBoundedTemplateScalar(JsonElement element, out string value)
    {
        value = string.Empty;
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                string raw = element.GetString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(raw) || raw.Length > MaximumValueLength || raw.Contains('\0'))
                    return false;
                value = raw.Trim();
                return true;
            case JsonValueKind.Number:
                string numericText = element.GetRawText();
                if (numericText.Length == 0 || numericText.Length > MaximumValueLength)
                    return false;
                value = numericText;
                return true;
            case JsonValueKind.True:
                value = "yes";
                return true;
            case JsonValueKind.False:
                value = "no";
                return true;
            default:
                return false;
        }
    }

    private static string LocalizeTemplateFailure(
        Ra2AutomationTemplateExpansionFailureKind failureKind,
        string localMessage)
    {
        if (failureKind == Ra2AutomationTemplateExpansionFailureKind.InvalidArguments)
        {
            return localMessage switch
            {
                "ownerWeaponSlot must be Primary or Secondary." => "武器槽位必须是 Primary 或 Secondary。",
                "verses must contain exactly 11 percentage tokens." => "Verses 必须包含恰好 11 个百分比值。",
                "primaryVerses and secondaryVerses must each contain exactly 11 percentage tokens." =>
                    "Primary 与 Secondary 的 Verses 都必须各包含恰好 11 个百分比值。",
                "rot must be an integer greater than zero." => "ROT 必须是大于 0 的整数。",
                "infDeath must be between 0 and 10." => "InfDeath 必须在 0 到 10 之间。",
                "cellSpread must be between 0 and 11." => "CellSpread 必须在 0 到 11 之间。",
                "percentAtMax must be non-negative." => "PercentAtMax 不能为负数。",
                "proneDamage must be non-negative." => "ProneDamage 不能为负数。",
                "The YR core Warhead profile does not support documents with an [ArmorTypes] section; use an Ares custom-armor profile." =>
                    "当前文档包含 [ArmorTypes]；YR 核心弹头模板不适用，需要使用 Ares 自定义护甲模板。",
                _ => "内容模板参数不符合当前 Profile 的约束。"
            };
        }

        return failureKind switch
        {
            Ra2AutomationTemplateExpansionFailureKind.TemplateNotFound => "模型请求的内容模板不存在。",
            Ra2AutomationTemplateExpansionFailureKind.TemplateVersionMismatch => "模型请求的内容模板版本不可用。",
            Ra2AutomationTemplateExpansionFailureKind.MissingRequiredArgument => "内容模板缺少必填参数。",
            Ra2AutomationTemplateExpansionFailureKind.UnknownArgument => "内容模板包含未知参数。",
            Ra2AutomationTemplateExpansionFailureKind.DuplicateArgument => "内容模板包含重复参数。",
            Ra2AutomationTemplateExpansionFailureKind.FieldSchemaNotFound => "当前字段库无法证明模板需要的字段。",
            Ra2AutomationTemplateExpansionFailureKind.BlockedFieldTrust => "当前字段库阻止模板写入低可信字段。",
            Ra2AutomationTemplateExpansionFailureKind.DocumentTooLarge => "当前文档超过内容模板预览资源上限。",
            Ra2AutomationTemplateExpansionFailureKind.Canceled => "已取消内容模板展开。",
            _ => "内容模板无法在当前文档安全展开。"
        };
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
            !TryReadBoundedIniValue(
                element,
                "value",
                MaximumValueLength,
                out string value))
        {
            return InvalidJson("结构化修改操作的 kind、section、key 必须是字符串，value 必须是字符串或数字。");
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

    private static bool TryValidateOptionalBoundedString(
        JsonElement parent,
        string propertyName,
        int maximumLength)
    {
        if (!parent.TryGetProperty(propertyName, out JsonElement element))
            return true;
        if (element.ValueKind != JsonValueKind.String)
            return false;

        string raw = element.GetString() ?? string.Empty;
        return raw.Length <= maximumLength && !raw.Contains('\0');
    }

    private static bool TryReadBoundedIniValue(
        JsonElement parent,
        string propertyName,
        int maximumLength,
        out string value)
    {
        value = string.Empty;
        if (!parent.TryGetProperty(propertyName, out JsonElement element))
            return false;

        if (element.ValueKind == JsonValueKind.String)
        {
            string raw = element.GetString() ?? string.Empty;
            if (raw.Length > maximumLength || raw.Contains('\0'))
                return false;
            value = raw;
            return true;
        }

        if (element.ValueKind != JsonValueKind.Number)
            return false;

        string numericText = element.GetRawText();
        if (numericText.Length == 0 || numericText.Length > maximumLength)
            return false;
        value = numericText;
        return true;
    }

    private static bool TryResolveOutcome(
        JsonElement root,
        out string outcome,
        out Ra2AiEditPlanCreationResult? failure)
    {
        if (root.TryGetProperty("outcome", out _))
        {
            if (TryReadBoundedString(root, "outcome", 64, allowEmpty: false, out outcome))
            {
                failure = null;
                return true;
            }

            outcome = string.Empty;
            failure = InvalidJson("结构化修改的 outcome 必须是非空字符串。");
            return false;
        }

        bool hasOperations = root.TryGetProperty("operations", out _);
        bool hasMessage = root.TryGetProperty("message", out _);
        if (!hasOperations && !hasMessage)
        {
            outcome = string.Empty;
            failure = InvalidJson("结构化修改缺少 outcome，且无法从 operations/message 唯一判定结果类型。");
            return false;
        }

        outcome = hasOperations ? "proposal" : "needs_clarification";
        failure = null;
        return true;
    }

    private static Ra2AiEditPlanCreationResult InvalidJson(
        string message = "模型返回的结构化修改参数格式无效。")
        => Failed(
            Ra2AiEditProposalFailureKind.InvalidArgumentsJson,
            message);

    private static Ra2AiEditPlanCreationResult Failed(
        Ra2AiEditProposalFailureKind failureKind,
        string message)
        => Ra2AiEditPlanCreationResult.Failed(failureKind, message);

}
