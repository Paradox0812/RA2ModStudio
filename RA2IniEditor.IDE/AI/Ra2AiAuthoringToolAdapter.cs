using System.IO;
using System.Text;
using System.Text.Json;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.AI;

/// <summary>把不可信 provider 参数转换为绑定本地快照的受限编辑计划。</summary>
internal sealed class Ra2AiAuthoringToolAdapter
{
    private const int MaximumSectionLength = 256;
    private const int MaximumKeyLength = 256;
    private const int MaximumValueLength = 8192;
    private const int MaximumJsonDepth = 32;
    private const string ProjectRulesArtTemplateId = "techno-rules-art-asset-binding";
    private const int ProjectRulesArtTemplateVersion = 1;
    private const string UnitDeliverySuperWeaponTemplateId = "ares-unitdelivery-superweapon-complete";
    private const string GenericWarheadSuperWeaponTemplateId = "ares-genericwarhead-superweapon-complete";

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
            if (requestContext.Scope != Ra2AiAuthoringScope.Document)
                return Failed(Ra2AiEditProposalFailureKind.UnsupportedTool, "项目请求不能调用单文档内容模板工具。");
            return TryCreateTemplatePlan(toolCall, requestContext);
        }

        if (string.Equals(
                toolCall.Name,
                Ra2AiAuthoringToolCatalog.PreviewIniProjectEditPlanToolName,
                StringComparison.Ordinal))
        {
            if (requestContext.Scope != Ra2AiAuthoringScope.Project)
                return Failed(Ra2AiEditProposalFailureKind.UnsupportedTool, "单文档请求不能调用项目编辑工具。");
            return TryCreateProjectPlan(toolCall, requestContext);
        }

        if (string.Equals(
                toolCall.Name,
                Ra2AiAuthoringToolCatalog.ExpandIniProjectContentTemplateToolName,
                StringComparison.Ordinal))
        {
            if (requestContext.Scope != Ra2AiAuthoringScope.Project)
                return Failed(Ra2AiEditProposalFailureKind.UnsupportedTool, "单文档请求不能调用项目内容模板工具。");
            return TryCreateTemplatePlan(toolCall, requestContext, isProjectTemplate: true);
        }

        if (!string.Equals(toolCall.Name, Ra2AiAuthoringToolCatalog.PreviewIniEditPlanToolName, StringComparison.Ordinal))
        {
            return Failed(
                Ra2AiEditProposalFailureKind.UnsupportedTool,
                "模型返回了当前版本不支持的编辑工具。");
        }

        if (requestContext.Scope != Ra2AiAuthoringScope.Document)
            return Failed(Ra2AiEditProposalFailureKind.UnsupportedTool, "项目请求不能调用单文档编辑工具。");

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
                ValidateNoDuplicateProperties(root);
            if (propertyFailure is not null)
                return propertyFailure;

            if (!TryResolveOutcome(root, out string outcome, out Ra2AiEditPlanCreationResult? outcomeFailure))
                return outcomeFailure!;

            if (string.Equals(outcome, "needsclarification", StringComparison.Ordinal))
            {
                if (!TryReadBoundedString(
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

            string summary = "AI 结构化修改建议";
            if (root.TryGetProperty("summary", out _) &&
                TryReadBoundedString(
                    root,
                    "summary",
                    Ra2IniEditPlan.MaximumSummaryLength,
                    allowEmpty: false,
                    out string modelSummary))
            {
                summary = modelSummary;
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
                    TryCreateOperation(operationElement, operations, allowAdditiveProperties: true);
                if (operationFailure is not null)
                    return operationFailure;
            }

            Ra2AuthoringSnapshot snapshot = requestContext.Snapshot;
            IReadOnlyList<Ra2AutomationSectionCreateOperation> sectionCreations =
                InferMissingSectionCreations(snapshot.ToAutomationSnapshot(), operations);
            return Ra2AiEditPlanCreationResult.FromPlan(new Ra2IniEditPlan(
                Guid.NewGuid(),
                snapshot.DocumentId,
                snapshot.EditRevision,
                snapshot.FieldRegistry.Revision,
                sectionCreations,
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

    private static Ra2AiEditPlanCreationResult TryCreateProjectPlan(
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
                return InvalidJson("项目结构化修改参数根必须是 JSON 对象。");

            Ra2AiEditPlanCreationResult? propertyFailure =
                ValidateNoDuplicateProperties(root);
            if (propertyFailure is not null)
                return propertyFailure;

            if (!TryResolveProjectOutcome(root, out string outcome, out Ra2AiEditPlanCreationResult? outcomeFailure))
                return outcomeFailure!;
            if (string.Equals(outcome, "needsclarification", StringComparison.Ordinal))
            {
                if (!TryReadBoundedString(
                        root,
                        "message",
                        Ra2IniEditPlan.MaximumSummaryLength,
                        allowEmpty: false,
                        out string clarificationMessage))
                {
                    return InvalidJson("项目结构化修改 clarification 必须包含非空有界 message。");
                }

                return Ra2AiEditPlanCreationResult.Clarification(clarificationMessage);
            }

            if (!string.Equals(outcome, "proposal", StringComparison.Ordinal))
                return InvalidJson("项目结构化修改的 outcome 不是 proposal 或 needs_clarification。");
            string summary = "AI 项目结构化修改建议";
            if (root.TryGetProperty("summary", out _) &&
                TryReadBoundedString(
                    root,
                    "summary",
                    Ra2IniEditPlan.MaximumSummaryLength,
                    allowEmpty: false,
                    out string modelSummary))
            {
                summary = modelSummary;
            }

            if (!root.TryGetProperty("documents", out JsonElement documentsElement) ||
                documentsElement.ValueKind is not (JsonValueKind.Array or JsonValueKind.Object))
            {
                return InvalidJson("项目结构化修改缺少 documents 对象或数组。");
            }

            JsonElement[] documentElements = documentsElement.ValueKind == JsonValueKind.Array
                ? documentsElement.EnumerateArray().ToArray()
                : [documentsElement];
            if (documentElements.Length is < 1 or > 2)
            {
                return Failed(
                    Ra2AiEditProposalFailureKind.InvalidOperation,
                    "项目结构化修改只能包含当前 rules/art 配对中的一到两个文档计划。");
            }

            Ra2AutomationProjectSnapshot snapshot = requestContext.ProjectSnapshot!;
            HashSet<string> targets = new(StringComparer.OrdinalIgnoreCase);
            List<Ra2IniEditPlan> documentPlans = new(documentElements.Length);
            foreach (JsonElement documentElement in documentElements)
            {
                if (documentElement.ValueKind != JsonValueKind.Object)
                    return InvalidJson("项目 documents 项必须是对象。");
                Ra2AiEditPlanCreationResult? documentPropertyFailure =
                    ValidateNoDuplicateProperties(documentElement);
                if (documentPropertyFailure is not null)
                    return documentPropertyFailure;
                if (!TryReadBoundedString(documentElement, "target", 16, allowEmpty: false, out string target))
                {
                    return InvalidJson("项目文档 target 必须是 rules 或 art。");
                }
                target = target.ToLowerInvariant();
                if (target is not ("rules" or "art"))
                    return InvalidJson("项目文档 target 必须是 rules 或 art。");
                if (!targets.Add(target))
                {
                    return Failed(
                        Ra2AiEditProposalFailureKind.InvalidOperation,
                        "项目结构化修改不能重复声明同一个 rules/art 目标。");
                }

                Ra2AutomationDocumentSnapshot? targetSnapshot = FindProjectDocument(snapshot, target);
                if (targetSnapshot is null)
                {
                    return Failed(
                        Ra2AiEditProposalFailureKind.RequestContextUnavailable,
                        "当前项目快照无法解析模型声明的 rules/art 目标。");
                }
                if (!documentElement.TryGetProperty("operations", out JsonElement operationsElement))
                    return InvalidJson("项目文档计划缺少 operations。");

                JsonElement[] operationElements = operationsElement.ValueKind switch
                {
                    JsonValueKind.Array => operationsElement.EnumerateArray().ToArray(),
                    JsonValueKind.Object => [operationsElement],
                    _ => []
                };
                if (operationElements.Length is < 1 or > Ra2IniEditPlan.MaximumOperationCount)
                {
                    return Failed(
                        Ra2AiEditProposalFailureKind.InvalidOperation,
                        "项目文档操作数量超出允许范围。");
                }

                List<Ra2IniEditOperation> operations = new(operationElements.Length);
                foreach (JsonElement operationElement in operationElements)
                {
                    Ra2AiEditPlanCreationResult? operationFailure =
                        TryCreateOperation(operationElement, operations, allowAdditiveProperties: true);
                    if (operationFailure is not null)
                        return operationFailure;
                }

                IReadOnlyList<Ra2AutomationSectionCreateOperation> sectionCreations =
                    InferMissingSectionCreations(targetSnapshot, operations);
                documentPlans.Add(new Ra2IniEditPlan(
                    Guid.NewGuid(),
                    targetSnapshot.DocumentId,
                    targetSnapshot.Version,
                    targetSnapshot.FieldRegistry.Revision,
                    sectionCreations,
                    operations,
                    $"{summary} · {target}",
                    Ra2AiAuthoringToolCatalog.TrustedPlanOrigin));
            }

            Ra2AutomationProjectEditPlan projectPlan = new(
                Guid.NewGuid(),
                snapshot.ProjectSessionId,
                snapshot.ProjectRevision,
                documentPlans,
                summary,
                Ra2AiAuthoringToolCatalog.TrustedPlanOrigin);
            return Ra2AiEditPlanCreationResult.FromProjectPlan(projectPlan, assetManifest: null);
        }
        catch (JsonException)
        {
            return InvalidJson("模型返回的项目结构化修改参数不是有效 JSON。");
        }
        catch (ArgumentException)
        {
            return Failed(
                Ra2AiEditProposalFailureKind.InvalidOperation,
                "项目结构化修改超过最低结构或资源安全界限。");
        }
    }

    private static bool TryResolveProjectOutcome(
        JsonElement root,
        out string outcome,
        out Ra2AiEditPlanCreationResult? failure)
    {
        if (root.TryGetProperty("outcome", out _))
        {
            if (TryReadBoundedString(root, "outcome", 64, allowEmpty: false, out outcome))
            {
                outcome = NormalizeProtocolToken(outcome);
                failure = null;
                return true;
            }

            outcome = string.Empty;
            failure = InvalidJson("项目结构化修改的 outcome 必须是非空字符串。");
            return false;
        }

        bool hasProposalPayload = root.TryGetProperty("documents", out _);
        bool hasMessage = root.TryGetProperty("message", out _);
        if (!hasProposalPayload && !hasMessage)
        {
            outcome = string.Empty;
            failure = InvalidJson("项目结构化修改缺少 outcome，且无法判定 proposal 或 clarification。");
            return false;
        }

        outcome = hasProposalPayload ? "proposal" : "needsclarification";
        failure = null;
        return true;
    }

    private static Ra2AutomationDocumentSnapshot? FindProjectDocument(
        Ra2AutomationProjectSnapshot snapshot,
        string target)
    {
        string[] names = target == "rules"
            ? ["rulesmd.ini", "rules.ini"]
            : ["artmd.ini", "art.ini"];
        Ra2AutomationDocumentSnapshot[] matches = snapshot.Documents
            .Where(item => names.Contains(Path.GetFileName(item.FilePath), StringComparer.OrdinalIgnoreCase))
            .ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private static IReadOnlyList<Ra2AutomationSectionCreateOperation> InferMissingSectionCreations(
        Ra2AutomationDocumentSnapshot snapshot,
        IReadOnlyList<Ra2IniEditOperation> operations)
    {
        HashSet<string> existingSections = new Ra2IniTextDocumentParser()
            .Parse(snapshot.Text)
            .SectionHeaders
            .Select(line => line.SectionName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return operations
            .Where(operation => operation.Kind == Ra2IniEditOperationKind.UpsertField &&
                                !existingSections.Contains(operation.SectionName))
            .Select(operation => operation.SectionName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(sectionName => new Ra2AutomationSectionCreateOperation(sectionName, Ra2SectionKind.Unknown))
            .ToArray();
    }

    private Ra2AiEditPlanCreationResult TryCreateTemplatePlan(
        Ra2AiToolCall toolCall,
        Ra2AiAuthoringRequestContext requestContext,
        bool isProjectTemplate = false)
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

            if (string.Equals(outcome, "needsclarification", StringComparison.Ordinal))
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
                TryReadTemplateArguments(
                    argumentsElement,
                    allowEmptyAssetBrief: isProjectTemplate,
                    out List<Ra2AutomationTemplateArgument> arguments);
            if (argumentFailure is not null)
                return argumentFailure;

            if (isProjectTemplate)
            {
                Ra2AiEditPlanCreationResult? normalizationFailure =
                    TryNormalizeProjectTemplateArguments(
                        templateId,
                        templateVersion,
                        arguments,
                        out arguments);
                if (normalizationFailure is not null)
                    return normalizationFailure;
            }

            Ra2AutomationTemplateExpansionRequest expansionRequest =
                new(templateId, templateVersion, arguments);
            if (isProjectTemplate)
            {
                if (IsRulesOnlySuperWeaponTemplate(templateId))
                {
                    Ra2AutomationProjectSnapshot projectSnapshot = requestContext.ProjectSnapshot!;
                    Ra2AutomationDocumentSnapshot? rulesSnapshot = FindProjectDocument(projectSnapshot, "rules");
                    if (rulesSnapshot is null)
                    {
                        return Failed(
                            Ra2AiEditProposalFailureKind.RequestContextUnavailable,
                            "当前项目快照没有唯一的 rules.ini 或 rulesmd.ini 目标。");
                    }

                    Ra2AutomationTemplateExpansionResult rulesExpansion = _gateway.ExpandTemplate(
                        rulesSnapshot,
                        expansionRequest);
                    if (!rulesExpansion.Succeeded)
                        return TemplateFailure(rulesExpansion.FailureKind, rulesExpansion.Message, isProjectTemplate: true);

                    Ra2AutomationProjectEditPlan rulesProjectPlan = new(
                        Guid.NewGuid(),
                        projectSnapshot.ProjectSessionId,
                        projectSnapshot.ProjectRevision,
                        [rulesExpansion.Plan!],
                        $"Expand {templateId}",
                        $"ContentTemplate/{templateId}@{templateVersion}");
                    return Ra2AiEditPlanCreationResult.FromProjectPlan(rulesProjectPlan, assetManifest: null);
                }

                Ra2AutomationProjectTemplateExpansionResult projectExpansion =
                    _gateway.ExpandProjectTemplate(requestContext.ProjectSnapshot!, expansionRequest);
                return projectExpansion.Succeeded
                    ? Ra2AiEditPlanCreationResult.FromProjectPlan(
                        projectExpansion.Plan!,
                        projectExpansion.AssetManifest!)
                    : TemplateFailure(
                        projectExpansion.FailureKind,
                        projectExpansion.Message,
                        isProjectTemplate: true);
            }

            Ra2AutomationTemplateExpansionResult expansion = _gateway.ExpandTemplate(
                requestContext.Snapshot.ToAutomationSnapshot(),
                expansionRequest);
            return expansion.Succeeded
                ? Ra2AiEditPlanCreationResult.FromPlan(expansion.Plan!)
                : TemplateFailure(expansion.FailureKind, expansion.Message);
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

    private static bool IsRulesOnlySuperWeaponTemplate(string templateId)
        => templateId is UnitDeliverySuperWeaponTemplateId or GenericWarheadSuperWeaponTemplateId;

    private static Ra2AiEditPlanCreationResult TemplateFailure(
        Ra2AutomationTemplateExpansionFailureKind templateFailureKind,
        string localMessage,
        bool isProjectTemplate = false)
    {
        Ra2AiEditProposalFailureKind failureKind = MapTemplateFailure(templateFailureKind);
        string message = LocalizeTemplateFailure(templateFailureKind, localMessage, isProjectTemplate);
        return Ra2AiEditPlanCreationResult.Failed(
            failureKind,
            message,
            Ra2AiStructuredFailureEvidence.FromTemplate(failureKind, templateFailureKind, message));
    }

    private static bool TryResolveTemplateOutcome(
        JsonElement root,
        out string outcome,
        out Ra2AiEditPlanCreationResult? failure)
    {
        if (root.TryGetProperty("outcome", out _))
        {
            if (TryReadBoundedString(root, "outcome", 64, allowEmpty: false, out outcome))
            {
                outcome = NormalizeProtocolToken(outcome);
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

        outcome = hasProposalPayload ? "proposal" : "needsclarification";
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
        bool allowEmptyAssetBrief,
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
                    !TryReadBoundedTemplateScalar(
                        valueElement,
                        allowEmptyAssetBrief && string.Equals(name, "assetBrief", StringComparison.OrdinalIgnoreCase),
                        out string value))
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
                !TryReadBoundedTemplateScalar(
                    property.Value,
                    allowEmptyAssetBrief && string.Equals(property.Name, "assetBrief", StringComparison.OrdinalIgnoreCase),
                    out string value))
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

    private static Ra2AiEditPlanCreationResult? TryNormalizeProjectTemplateArguments(
        string templateId,
        int templateVersion,
        IReadOnlyList<Ra2AutomationTemplateArgument> source,
        out List<Ra2AutomationTemplateArgument> normalized)
    {
        normalized = source.ToList();
        if (!string.Equals(templateId, ProjectRulesArtTemplateId, StringComparison.Ordinal) ||
            templateVersion != ProjectRulesArtTemplateVersion)
        {
            return null;
        }

        string[] canonicalNames =
        [
            "ownerSectionId",
            "artSectionId",
            "bodyAssetId",
            "cameoAssetId",
            "assetBrief"
        ];
        if (source.Any(argument => !canonicalNames.Contains(argument.Name, StringComparer.OrdinalIgnoreCase)))
        {
            normalized = [];
            return Failed(
                Ra2AiEditProposalFailureKind.UnknownArgumentProperty,
                "项目内容模板包含未知参数。");
        }
        if (source.GroupBy(argument => argument.Name, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
        {
            normalized = [];
            return Failed(
                Ra2AiEditProposalFailureKind.DuplicateArgumentProperty,
                "项目内容模板包含重复参数。");
        }

        normalized = canonicalNames
            .Select(name => (Name: name, Argument: source.SingleOrDefault(argument =>
                string.Equals(argument.Name, name, StringComparison.OrdinalIgnoreCase))))
            .Where(item => item.Argument is not null)
            .Select(item => new Ra2AutomationTemplateArgument(item.Name, item.Argument!.Value))
            .ToList();

        foreach (string assetName in new[] { "bodyAssetId", "cameoAssetId" })
        {
            int index = normalized.FindIndex(argument => argument.Name == assetName);
            if (index >= 0 && normalized[index].Value.EndsWith(".shp", StringComparison.OrdinalIgnoreCase))
            {
                normalized[index] = new Ra2AutomationTemplateArgument(
                    assetName,
                    normalized[index].Value[..^4]);
            }
        }

        Ra2AutomationTemplateArgument? brief = normalized.SingleOrDefault(argument => argument.Name == "assetBrief");
        if (brief is not null && !string.IsNullOrWhiteSpace(brief.Value))
            return null;

        normalized.RemoveAll(argument => argument.Name == "assetBrief");

        string? owner = normalized.SingleOrDefault(argument => argument.Name == "ownerSectionId")?.Value;
        string? art = normalized.SingleOrDefault(argument => argument.Name == "artSectionId")?.Value;
        if (!string.IsNullOrWhiteSpace(owner) && !string.IsNullOrWhiteSpace(art))
        {
            normalized.Add(new Ra2AutomationTemplateArgument(
                "assetBrief",
                $"Prepare body and cameo assets for {owner.Trim()} using art section {art.Trim()}."));
        }

        return null;
    }

    private static bool TryReadBoundedTemplateScalar(
        JsonElement element,
        bool allowEmptyString,
        out string value)
    {
        value = string.Empty;
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                string raw = element.GetString() ?? string.Empty;
                if ((!allowEmptyString && string.IsNullOrWhiteSpace(raw)) ||
                    raw.Length > MaximumValueLength ||
                    raw.Contains('\0'))
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
        string localMessage,
        bool isProjectTemplate = false)
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
                "assetBrief cannot be empty." => "素材说明为空或超出长度限制。可以省略该说明，由 IDE 自动生成。",
                "bodyAssetId and cameoAssetId must be distinct Windows-safe file name stems." =>
                    "Body 与 Cameo 必须使用两个不同的、不含路径或扩展名的安全资源 ID。",
                _ when localMessage.StartsWith("Template argument '", StringComparison.Ordinal) =>
                    "内容模板中的 Section 或资源 ID 含非法字符，或数值不符合声明类型。",
                _ when localMessage.StartsWith("A resolved template section name", StringComparison.Ordinal) =>
                    "内容模板解析出的 Section ID 含非法字符。",
                _ when localMessage.StartsWith("Value for field '", StringComparison.Ordinal) =>
                    "模板字段值未通过当前字段语义校验。",
                _ when isProjectTemplate =>
                    "项目模板参数无效；请检查 provider、effect 引用、类型专用参数或 rules/art 绑定标识符。",
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
            Ra2AutomationTemplateExpansionFailureKind.RequiredSectionNotFound =>
                "rules 文档中找不到请求指定的现有对象 Section。",
            Ra2AutomationTemplateExpansionFailureKind.RequiredSectionKindMismatch =>
                "请求指定的现有 Section 存在，但其对象类型与当前 Profile 要求不兼容。",
            Ra2AutomationTemplateExpansionFailureKind.ProjectDocumentNotFound =>
                "当前项目缺少唯一完整的 rules/art 文件配对。",
            Ra2AutomationTemplateExpansionFailureKind.ProjectDocumentAmbiguous =>
                "当前项目存在重复或冲突的 rules/art 文件配对。",
            Ra2AutomationTemplateExpansionFailureKind.OperationLimitExceeded =>
                "项目模板生成的结构化操作超过安全上限。",
            Ra2AutomationTemplateExpansionFailureKind.DocumentTooLarge => "当前文档超过内容模板预览资源上限。",
            Ra2AutomationTemplateExpansionFailureKind.Canceled => "已取消内容模板展开。",
            _ => "内容模板无法在当前文档安全展开。"
        };
    }

    private static Ra2AiEditPlanCreationResult? TryCreateOperation(
        JsonElement element,
        ICollection<Ra2IniEditOperation> operations,
        bool allowAdditiveProperties = false)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return InvalidJson();

        Ra2AiEditPlanCreationResult? propertyFailure = allowAdditiveProperties
            ? ValidateNoDuplicateProperties(element)
            : ValidateProperties(element, OperationProperties);
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
        kind = NormalizeProtocolToken(kind);
        if (string.Equals(kind, "upsertfield", StringComparison.Ordinal))
            operationKind = Ra2IniEditOperationKind.UpsertField;
        else if (string.Equals(kind, "replacefieldvalue", StringComparison.Ordinal))
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

    private static Ra2AiEditPlanCreationResult? ValidateNoDuplicateProperties(JsonElement element)
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
        }
        return null;
    }

    private static string NormalizeProtocolToken(string value)
    {
        StringBuilder builder = new(value.Length);
        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
                builder.Append(char.ToLowerInvariant(character));
        }
        return builder.ToString();
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
                outcome = NormalizeProtocolToken(outcome);
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

        outcome = hasOperations ? "proposal" : "needsclarification";
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
