extern alias Ra2Application;

using System.IO;
using System.Text;
using System.Text.Json;
using RA2IniEditor.IDE.AI;
using Ra2VoxelGeometryEvidenceSlice = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryEvidenceSlice;
using Ra2VoxelGeometryEvidenceSliceBuilder = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryEvidenceSliceBuilder;
using Ra2VoxelGeometryProposal = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryProposal;
using Ra2VoxelGeometryProposalAction = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryProposalAction;
using Ra2VoxelGeometryProposalOperation = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryProposalOperation;
using Ra2VoxelGeometryProposalPartitionProjector = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryProposalPartitionProjector;
using Ra2VoxelGeometryProposalResolution = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryProposalResolution;
using Ra2VoxelGeometryProposalValidator = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelGeometryProposalValidator;
using Ra2VoxelMeshCoverageEvidence = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelMeshCoverageEvidence;
using Ra2VoxelSemanticPartition = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticPartition;
using Ra2VoxelSymmetryEvidencePackage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSymmetryEvidencePackage;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelSemanticCompilerFailureKind
{
    None = 0,
    CompilerUnavailable,
    CompilerTimeout,
    CompilerProviderFailure,
    MalformedProposal,
    ClarificationRequired,
    UnsupportedGeometry,
    InvalidPartition,
    EvidenceQueryRejected,
    ArbitrationFailed,
    Cancelled
}

internal sealed record Ra2VoxelSemanticCompilerResult(
    Ra2VoxelSemanticCompilerFailureKind FailureKind,
    string Message,
    Ra2VoxelSemanticPartition? Partition,
    Ra2VoxelGeometryProposal? Proposal,
    IReadOnlyList<Ra2AiRequest> Requests)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelSemanticCompilerFailureKind.None &&
        Partition is not null && Proposal is not null;
}

internal sealed class Ra2VoxelSemanticSymmetryCompiler
{
    internal const string ToolName = "propose_ra2_voxel_geometry";
    private const int MaximumPromptCharacters = 32_768;
    private const int MaximumArgumentsCharacters = 64 * 1024;
    private readonly IRa2AiClient _client;

    internal Ra2VoxelSemanticSymmetryCompiler(IRa2AiClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    internal async Task<Ra2VoxelSemanticCompilerResult> CompileAsync(
        Ra2VoxelSymmetryEvidencePackage evidence,
        Ra2VoxelMeshCoverageEvidence coverage,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(coverage);
        List<Ra2AiRequest> requests = [];
        Ra2VoxelGeometryEvidenceSlice? detailSlice = null;

        ParsedTurn primary = await SendAsync(
            BuildRequest(evidence, detailSlice, CompilerRole.Primary, null, null),
            evidence,
            requests,
            cancellationToken).ConfigureAwait(false);
        if (primary.Failure is not null) return primary.Failure;
        if (primary.Outcome == "query")
        {
            var slice = Ra2VoxelGeometryEvidenceSliceBuilder.Build(
                evidence, coverage, primary.QueryRegionIds, cancellationToken);
            if (!slice.IsSuccess)
                return Failure(Ra2VoxelSemanticCompilerFailureKind.EvidenceQueryRejected, slice.Message, requests);
            detailSlice = slice.Slice;
            primary = await SendAsync(
                BuildRequest(evidence, detailSlice, CompilerRole.PrimaryAfterQuery, null, null),
                evidence,
                requests,
                cancellationToken).ConfigureAwait(false);
            if (primary.Failure is not null) return primary.Failure;
            if (primary.Outcome == "query")
                return Failure(Ra2VoxelSemanticCompilerFailureKind.EvidenceQueryRejected,
                    "Only one bounded geometry-evidence request is allowed.", requests);
        }
        Ra2VoxelSemanticCompilerResult? primaryFailure = ValidateOutcome(primary, evidence, requests);
        if (primaryFailure is not null) return primaryFailure;
        Ra2VoxelGeometryProposal primaryProposal = primary.Proposal!;

        ParsedTurn review = await SendAsync(
            BuildRequest(evidence, detailSlice, CompilerRole.Reviewer, primaryProposal, null),
            evidence,
            requests,
            cancellationToken).ConfigureAwait(false);
        if (review.Failure is not null) return review.Failure;
        if (review.Outcome == "query")
            return Failure(Ra2VoxelSemanticCompilerFailureKind.EvidenceQueryRejected,
                "The review pass cannot request new evidence.", requests);
        Ra2VoxelSemanticCompilerResult? reviewFailure = ValidateOutcome(review, evidence, requests);
        if (reviewFailure is not null) return reviewFailure;
        Ra2VoxelGeometryProposal reviewProposal = review.Proposal!;

        Ra2VoxelGeometryProposal finalProposal;
        if (string.Equals(primaryProposal.ExecutableFingerprint, reviewProposal.ExecutableFingerprint, StringComparison.Ordinal))
        {
            finalProposal = reviewProposal.WithResolution(Ra2VoxelGeometryProposalResolution.Agreement);
        }
        else
        {
            ParsedTurn arbitration = await SendAsync(
                BuildRequest(evidence, detailSlice, CompilerRole.Arbitrator, primaryProposal, reviewProposal),
                evidence,
                requests,
                cancellationToken).ConfigureAwait(false);
            if (arbitration.Failure is not null)
                return arbitration.Failure;
            if (arbitration.Outcome != "proposal" || arbitration.Proposal is null)
                return Failure(Ra2VoxelSemanticCompilerFailureKind.ArbitrationFailed,
                    string.IsNullOrWhiteSpace(arbitration.Message)
                        ? "The arbitration pass did not return a final geometry proposal."
                        : arbitration.Message,
                    requests);
            string? invalidArbitration = Ra2VoxelGeometryProposalValidator.Validate(evidence, arbitration.Proposal, cancellationToken);
            if (invalidArbitration is not null)
                return Failure(Ra2VoxelSemanticCompilerFailureKind.ArbitrationFailed, invalidArbitration, requests);
            finalProposal = arbitration.Proposal.WithResolution(Ra2VoxelGeometryProposalResolution.Arbitration);
        }

        Ra2VoxelSemanticPartition partition = Ra2VoxelGeometryProposalPartitionProjector.Project(
            evidence, finalProposal, cancellationToken);
        return new(
            Ra2VoxelSemanticCompilerFailureKind.None,
            string.Empty,
            partition,
            finalProposal,
            Array.AsReadOnly(requests.ToArray()));
    }

    private async Task<ParsedTurn> SendAsync(
        Ra2AiRequest request,
        Ra2VoxelSymmetryEvidencePackage evidence,
        List<Ra2AiRequest> requests,
        CancellationToken cancellationToken)
    {
        requests.Add(request);
        if (request.PromptCharacterCount > MaximumPromptCharacters)
            return ParsedTurn.FromFailure(Failure(
                Ra2VoxelSemanticCompilerFailureKind.MalformedProposal,
                "The bounded geometry prompt exceeds the prompt limit.",
                requests));
        Ra2AiResponse response;
        try
        {
            response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ParsedTurn.FromFailure(Failure(
                Ra2VoxelSemanticCompilerFailureKind.Cancelled,
                "Voxel geometry analysis was cancelled.",
                requests));
        }
        Ra2VoxelSemanticCompilerResult? transport = MapTransportFailure(response, requests);
        return transport is null ? Parse(response, evidence) : ParsedTurn.FromFailure(transport);
    }

    private static Ra2VoxelSemanticCompilerResult? ValidateOutcome(
        ParsedTurn turn,
        Ra2VoxelSymmetryEvidencePackage evidence,
        IReadOnlyList<Ra2AiRequest> requests)
    {
        if (turn.Outcome == "clarification")
            return Failure(Ra2VoxelSemanticCompilerFailureKind.ClarificationRequired, turn.Message, requests);
        if (turn.Outcome == "unsupported")
            return Failure(Ra2VoxelSemanticCompilerFailureKind.UnsupportedGeometry, turn.Message, requests);
        if (turn.Outcome != "proposal" || turn.Proposal is null)
            return Failure(Ra2VoxelSemanticCompilerFailureKind.MalformedProposal, turn.Message, requests);
        string? invalid = Ra2VoxelGeometryProposalValidator.Validate(evidence, turn.Proposal);
        return invalid is null
            ? null
            : Failure(Ra2VoxelSemanticCompilerFailureKind.InvalidPartition, invalid, requests);
    }

    private static Ra2AiRequest BuildRequest(
        Ra2VoxelSymmetryEvidencePackage evidence,
        Ra2VoxelGeometryEvidenceSlice? detailSlice,
        CompilerRole role,
        Ra2VoxelGeometryProposal? primary,
        Ra2VoxelGeometryProposal? review)
    {
        StringBuilder system = new();
        system.AppendLine("You are the geometry-planning Agent for an RA2 vehicle voxel review.");
        system.AppendLine($"Call {ToolName} exactly once and return no prose.");
        system.AppendLine("You own structural interpretation and the edit direction. The host does not classify parts for you.");
        system.AppendLine("Return sparse operations only: omitted targets are preserved. Use only host target IDs; never return coordinates or paths.");
        system.AppendLine("add_mirror preserves selected occupied cells and adds missing mirrored counterparts. remove_source removes only selected occupied cells whose mirror is absent.");
        system.AppendLine("bridge_center_gap fills only the Host-listed one/two-cell X-axis center-seam gaps whose occupied anchors already exist on both sides. Use it only with seam-gap targets; do not treat arbitrary holes as seam gaps.");
        system.AppendLine("Center-seam targets already contain complete bounded facts and cannot be requested through the detail query.");
        system.AppendLine("Do not request or infer colour/material changes. The host only expands known targets and enforces minimum geometry safety.");
        system.AppendLine(role switch
        {
            CompilerRole.Primary => "Act as the primary analyst. You may request one bounded detail slice when aggregate evidence is insufficient; otherwise propose executable operations.",
            CompilerRole.PrimaryAfterQuery => "The requested bounded detail slice is now present. Return an executable sparse proposal; no further query is allowed.",
            CompilerRole.Reviewer => "Review the normalized primary proposal against the same evidence. Return your own complete sparse proposal. Preserve it when supported; change executable operations only for an evidence-based reason.",
            CompilerRole.Arbitrator => "The primary and reviewer executable operations differ. Arbitrate the disagreement and return the sole final sparse proposal. You may select or merge supported operations; do not request more evidence.",
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        });

        StringBuilder user = new();
        user.Append(evidence.ToPromptText());
        if (detailSlice is not null)
        {
            user.AppendLine("bounded_detail_slice:");
            user.Append(detailSlice.ToPromptText());
        }
        if (primary is not null)
        {
            user.AppendLine("primary_normalized_json:");
            user.AppendLine(SerializeProposal(primary));
        }
        if (review is not null)
        {
            user.AppendLine("review_normalized_json:");
            user.AppendLine(SerializeProposal(review));
        }
        string systemText = system.ToString();
        string userText = user.ToString();
        return new(
            Ra2AiIntent.Auto,
            role switch
            {
                CompilerRole.Primary => "Plan voxel geometry",
                CompilerRole.PrimaryAfterQuery => "Plan voxel geometry with requested evidence",
                CompilerRole.Reviewer => "Review voxel geometry proposal",
                _ => "Arbitrate voxel geometry proposal"
            },
            string.Concat(systemText, Environment.NewLine, userText),
            tools: [BuildTool()],
            toolChoice: Ra2AiToolChoiceMode.Required,
            systemPromptText: systemText,
            userContentText: userText);
    }

    private static Ra2AiToolDefinition BuildTool() => new(
        ToolName,
        "Request bounded geometry evidence or return a sparse RA2 voxel geometry proposal.",
        """
        {"type":"object","properties":{"outcome":{"type":"string","enum":["proposal","query","clarification","unsupported"]},"message":{"type":"string","maxLength":512},"evidence_hash":{"type":"string","minLength":64,"maxLength":64},"reviewed_plane_twice_x":{"type":"integer","minimum":0,"maximum":510},"query_region_ids":{"type":"array","maxItems":8,"items":{"type":"string","minLength":1,"maxLength":64}},"operations":{"type":"array","maxItems":64,"items":{"type":"object","properties":{"target_id":{"type":"string","minLength":1,"maxLength":96},"action":{"type":"string","enum":["add_mirror","remove_source","bridge_center_gap"]},"confidence":{"type":"number","minimum":0,"maximum":1},"reason":{"type":"string","maxLength":512}},"required":["target_id","action"]}},"unresolved_assumptions":{"type":"array","maxItems":32,"items":{"type":"string","maxLength":512}}},"required":["outcome","evidence_hash","reviewed_plane_twice_x"]}
        """);

    private static ParsedTurn Parse(Ra2AiResponse response, Ra2VoxelSymmetryEvidencePackage evidence)
    {
        Ra2AiToolCall[] matchingCalls = response.ToolCalls
            .Where(call => string.Equals(call.Name, ToolName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matchingCalls.Length != 1)
            return ParsedTurn.Malformed($"tool_call_count:{matchingCalls.Length}");
        if (matchingCalls[0].ArgumentsJson.Length > MaximumArgumentsCharacters)
            return ParsedTurn.Malformed("tool_arguments_too_large");
        try
        {
            using JsonDocument document = JsonDocument.Parse(NormalizeArgumentsJson(matchingCalls[0].ArgumentsJson), new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
                MaxDepth = 14
            });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return ParsedTurn.Malformed("root_not_object");
            string outcome = ReadRequiredString(root, 32, "outcome").ToLowerInvariant();
            string message = ReadOptionalString(root, 512, "message");
            string hash = ReadRequiredString(root, 64, "evidence_hash", "evidenceHash").ToUpperInvariant();
            if (hash.Length != 64 || hash.Any(value => !Uri.IsHexDigit(value)))
                throw new ProposalContractException("invalid_value:evidence_hash");
            int plane = ReadInteger(root, "reviewed_plane_twice_x", "reviewedPlaneTwiceX");
            if (!string.Equals(hash, evidence.PackageHash, StringComparison.Ordinal))
                throw new ProposalContractException("evidence_hash_mismatch");
            if (plane != evidence.SelectedPlaneTwiceX)
                throw new ProposalContractException("reviewed_plane_mismatch");
            List<string> assumptions = TryGetProperty(root, out JsonElement assumptionElement,
                    "unresolved_assumptions", "unresolvedAssumptions")
                ? ReadStrings(assumptionElement, 32, 512, allowEmpty: true)
                : [];
            if (outcome is "clarification" or "unsupported")
                return new(outcome, message, [], null, null);
            if (outcome == "query")
            {
                if (!TryGetProperty(root, out JsonElement queryElement, "query_region_ids", "queryRegionIds"))
                    throw new ProposalContractException("missing_property:query_region_ids");
                return new(outcome, message, ReadStrings(queryElement, 8, 96, allowEmpty: false), null, null);
            }
            if (outcome != "proposal") return ParsedTurn.Malformed("invalid_value:outcome");
            if (!TryGetProperty(root, out JsonElement operationsElement, "operations"))
                throw new ProposalContractException("missing_property:operations");
            if (operationsElement.ValueKind != JsonValueKind.Array || operationsElement.GetArrayLength() is < 1 or > 64)
                throw new ProposalContractException("invalid_value:operations");
            List<Ra2VoxelGeometryProposalOperation> operations = [];
            foreach (JsonElement item in operationsElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    throw new ProposalContractException("invalid_type:operations.item");
                double confidence = TryGetProperty(item, out _, "confidence") ? ReadDouble(item, "confidence") : 0.5d;
                if (!double.IsFinite(confidence) || confidence is < 0d or > 1d)
                    throw new ProposalContractException("invalid_value:confidence");
                operations.Add(new(
                    ReadRequiredString(item, 96, "target_id", "targetId"),
                    ReadAction(ReadRequiredString(item, 32, "action")),
                    confidence,
                    ReadOptionalString(item, 512, "reason")));
            }
            return new(outcome, message, [], new(hash, plane, operations, assumptions), null);
        }
        catch (ProposalContractException exception)
        {
            return ParsedTurn.Malformed(exception.Message);
        }
        catch (JsonException)
        {
            return ParsedTurn.Malformed("invalid_json");
        }
        catch (Exception exception) when (exception is InvalidDataException or InvalidOperationException or FormatException or OverflowException)
        {
            return ParsedTurn.Malformed("invalid_bounded_shape");
        }
    }

    private static Ra2VoxelSemanticCompilerResult? MapTransportFailure(
        Ra2AiResponse response,
        IReadOnlyList<Ra2AiRequest> requests) => response.Kind switch
        {
            Ra2AiResponseKind.ToolCalls => null,
            Ra2AiResponseKind.Cancelled => Failure(Ra2VoxelSemanticCompilerFailureKind.Cancelled, "Voxel geometry analysis was cancelled.", requests),
            Ra2AiResponseKind.MissingConfiguration => Failure(Ra2VoxelSemanticCompilerFailureKind.CompilerUnavailable, "DeepSeek is not configured for voxel geometry analysis.", requests),
            Ra2AiResponseKind.Timeout => Failure(Ra2VoxelSemanticCompilerFailureKind.CompilerTimeout, "DeepSeek voxel geometry analysis timed out.", requests),
            _ => Failure(Ra2VoxelSemanticCompilerFailureKind.CompilerProviderFailure, "DeepSeek did not return a structured voxel geometry proposal.", requests)
        };

    private static string SerializeProposal(Ra2VoxelGeometryProposal proposal) => JsonSerializer.Serialize(new
    {
        evidence_hash = proposal.EvidencePackageHash,
        reviewed_plane_twice_x = proposal.ReviewedPlaneTwiceX,
        operations = proposal.Operations.Select(value => new
        {
            target_id = value.TargetId,
            action = FormatAction(value.Action),
            confidence = value.Confidence,
            reason = value.Reason
        }),
        unresolved_assumptions = proposal.UnresolvedAssumptions
    });

    private static string NormalizeArgumentsJson(string value)
    {
        string current = value.Trim();
        if (current.StartsWith("```", StringComparison.Ordinal))
        {
            int firstLine = current.IndexOf('\n');
            int closing = current.LastIndexOf("```", StringComparison.Ordinal);
            if (firstLine >= 0 && closing > firstLine) current = current[(firstLine + 1)..closing].Trim();
        }
        for (int depth = 0; depth < 2; depth++)
        {
            using JsonDocument wrapper = JsonDocument.Parse(current, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
                MaxDepth = 16
            });
            if (wrapper.RootElement.ValueKind == JsonValueKind.String)
            {
                current = wrapper.RootElement.GetString()?.Trim() ?? throw new InvalidDataException();
                continue;
            }
            if (wrapper.RootElement.ValueKind == JsonValueKind.Object &&
                !TryGetProperty(wrapper.RootElement, out _, "outcome") &&
                TryGetProperty(wrapper.RootElement, out JsonElement arguments, "arguments", "parameters"))
            {
                current = arguments.ValueKind == JsonValueKind.String
                    ? arguments.GetString()?.Trim() ?? throw new InvalidDataException()
                    : arguments.GetRawText();
                continue;
            }
            return current;
        }
        return current;
    }

    private static bool TryGetProperty(JsonElement element, out JsonElement value, params string[] names)
    {
        bool found = false;
        JsonElement candidate = default;
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!names.Any(name => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))) continue;
            if (found) throw new ProposalContractException($"duplicate_alias:{names[0]}");
            candidate = property.Value;
            found = true;
        }
        value = candidate;
        return found;
    }

    private static string ReadRequiredString(JsonElement root, int maximum, params string[] names)
    {
        if (!TryGetProperty(root, out JsonElement value, names))
            throw new ProposalContractException($"missing_property:{names[0]}");
        if (value.ValueKind != JsonValueKind.String)
            throw new ProposalContractException($"invalid_type:{names[0]}");
        string text = value.GetString() ?? string.Empty;
        if (text.Length > maximum || text.IndexOf('\0') >= 0 || string.IsNullOrWhiteSpace(text))
            throw new ProposalContractException($"invalid_value:{names[0]}");
        return string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string ReadOptionalString(JsonElement root, int maximum, params string[] names)
    {
        if (!TryGetProperty(root, out JsonElement value, names)) return string.Empty;
        if (value.ValueKind != JsonValueKind.String)
            throw new ProposalContractException($"invalid_type:{names[0]}");
        string text = value.GetString() ?? string.Empty;
        if (text.Length > maximum || text.IndexOf('\0') >= 0)
            throw new ProposalContractException($"invalid_value:{names[0]}");
        return string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static int ReadInteger(JsonElement root, params string[] names)
    {
        if (!TryGetProperty(root, out JsonElement value, names))
            throw new ProposalContractException($"missing_property:{names[0]}");
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int numeric)) return numeric;
        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out int parsed)) return parsed;
        throw new ProposalContractException($"invalid_value:{names[0]}");
    }

    private static double ReadDouble(JsonElement root, params string[] names)
    {
        if (!TryGetProperty(root, out JsonElement value, names))
            throw new ProposalContractException($"missing_property:{names[0]}");
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out double numeric)) return numeric;
        if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out double parsed)) return parsed;
        throw new ProposalContractException($"invalid_value:{names[0]}");
    }

    private static List<string> ReadStrings(JsonElement element, int maximumCount, int maximumLength, bool allowEmpty)
    {
        if (element.ValueKind != JsonValueKind.Array || element.GetArrayLength() > maximumCount ||
            (!allowEmpty && element.GetArrayLength() == 0))
            throw new ProposalContractException("invalid_value:string_array");
        List<string> values = [];
        foreach (JsonElement item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
                throw new ProposalContractException("invalid_type:string_array.item");
            string text = item.GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text) || text.Length > maximumLength || text.IndexOfAny(['\r', '\n', '\0']) >= 0)
                throw new ProposalContractException("invalid_value:string_array.item");
            values.Add(text.Trim());
        }
        if (values.Distinct(StringComparer.Ordinal).Count() != values.Count)
            throw new ProposalContractException("duplicate_value:string_array");
        return values;
    }

    private static Ra2VoxelGeometryProposalAction ReadAction(string value) =>
        value.Trim().ToLowerInvariant().Replace('-', '_').Replace(' ', '_') switch
        {
            "add_mirror" or "mirror_add" or "add_mirrored_counterpart" => Ra2VoxelGeometryProposalAction.AddMirror,
            "remove_source" or "remove_unmatched" => Ra2VoxelGeometryProposalAction.RemoveSource,
            "bridge_center_gap" or "bridge_seam" or "fill_center_seam" => Ra2VoxelGeometryProposalAction.BridgeCenterGap,
            _ => throw new ProposalContractException("invalid_value:action")
        };

    private static string FormatAction(Ra2VoxelGeometryProposalAction value) => value switch
    {
        Ra2VoxelGeometryProposalAction.AddMirror => "add_mirror",
        Ra2VoxelGeometryProposalAction.RemoveSource => "remove_source",
        Ra2VoxelGeometryProposalAction.BridgeCenterGap => "bridge_center_gap",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static Ra2VoxelSemanticCompilerResult Failure(
        Ra2VoxelSemanticCompilerFailureKind kind,
        string message,
        IReadOnlyList<Ra2AiRequest> requests) => new(
            kind,
            message,
            null,
            null,
            Array.AsReadOnly(requests.ToArray()));

    private enum CompilerRole
    {
        Primary,
        PrimaryAfterQuery,
        Reviewer,
        Arbitrator
    }

    private sealed record ParsedTurn(
        string Outcome,
        string Message,
        IReadOnlyList<string> QueryRegionIds,
        Ra2VoxelGeometryProposal? Proposal,
        Ra2VoxelSemanticCompilerResult? Failure)
    {
        internal static ParsedTurn Malformed(string message) => new("malformed", message, [], null, null);
        internal static ParsedTurn FromFailure(Ra2VoxelSemanticCompilerResult failure) => new("failure", failure.Message, [], null, failure);
    }

    private sealed class ProposalContractException(string message) : Exception(message);
}
