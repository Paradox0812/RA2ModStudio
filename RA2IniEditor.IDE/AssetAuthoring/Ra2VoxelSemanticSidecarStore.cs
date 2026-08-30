extern alias Ra2Application;

using System.Text;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using RA2IniEditor.Infrastructure.IO;
using Ra2VoxelSceneSnapshot = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSceneSnapshot;
using Ra2VoxelSemanticAssignment = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticAssignment;
using Ra2VoxelSemanticCellOverride = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticCellOverride;
using Ra2VoxelSemanticEvidenceBuilder = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEvidenceBuilder;
using Ra2VoxelSemanticEvidencePackage = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticEvidencePackage;
using Ra2VoxelSemanticManualMaskLayer = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticManualMaskLayer;
using Ra2VoxelSemanticMaterialRole = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticMaterialRole;
using Ra2VoxelSemanticPartRole = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticPartRole;
using Ra2VoxelSemanticRemapIntent = Ra2Application::RA2IniEditor.Application.Automation.Experimental.VoxelAuthoring.Ra2VoxelSemanticRemapIntent;

namespace RA2IniEditor.IDE.AssetAuthoring;

internal enum Ra2VoxelSemanticSidecarFailureKind
{
    None = 0,
    InvalidPath,
    OutsideProject,
    ReparsePointRejected,
    NotFound,
    TooLarge,
    InvalidUtf8,
    InvalidJson,
    UnsupportedSchema,
    InvalidShape,
    ResourceLimitExceeded,
    SnapshotMismatch,
    EvidenceMismatch,
    LayerHashMismatch,
    IoFailure,
    Canceled
}

internal sealed record Ra2VoxelSemanticSidecarState(
    Ra2VoxelSemanticEvidencePackage Evidence,
    bool AgentSuggestionsAccepted,
    IReadOnlyList<Ra2VoxelSemanticAssignment> AgentSuggestions,
    IReadOnlyList<Ra2VoxelSemanticAssignment> HumanRegionOverrides,
    Ra2VoxelSemanticManualMaskLayer HumanCellLayer);

internal sealed record Ra2VoxelSemanticSidecarResult(
    Ra2VoxelSemanticSidecarFailureKind FailureKind,
    string Message,
    Ra2VoxelSemanticSidecarState? State = null)
{
    internal bool IsSuccess => FailureKind == Ra2VoxelSemanticSidecarFailureKind.None;
}

internal sealed class Ra2VoxelSemanticSidecarStore
{
    internal const string Schema = "ra2-voxel-semantic-sidecar";
    internal const int Version = 1;
    internal const long MaximumFileBytes = 32L * 1024 * 1024;
    private const int MaximumRegionAssignments = 48;
    private const int MaximumReasonLength = 512;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly JsonSerializerOptions WriterOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(null, allowIntegerValues: false) }
    };

    internal Ra2VoxelSemanticSidecarResult Save(
        string projectRoot,
        string filePath,
        Ra2VoxelSceneSnapshot snapshot,
        Ra2VoxelSemanticSidecarState state,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelSemanticSidecarResult? pathFailure = ValidatePath(projectRoot, filePath, mustExist: false, out string fullPath);
            if (pathFailure is not null) return pathFailure;
            Ra2VoxelSemanticSidecarResult? stateFailure = ValidateState(snapshot, state);
            if (stateFailure is not null) return stateFailure;

            var document = new SidecarDocument(
                Schema,
                Version,
                snapshot.CanonicalHash,
                state.Evidence.PackageHash,
                snapshot.OccupancyCount,
                state.HumanCellLayer.LayerHash,
                state.AgentSuggestionsAccepted,
                state.AgentSuggestions.OrderBy(value => value.RegionId, StringComparer.Ordinal).Select(ToDocument).ToArray(),
                state.HumanRegionOverrides.OrderBy(value => value.RegionId, StringComparer.Ordinal).Select(ToDocument).ToArray(),
                GroupCells(state.HumanCellLayer.Overrides));
            string json = JsonSerializer.Serialize(document, WriterOptions) + Environment.NewLine;
            if (StrictUtf8.GetByteCount(json) > MaximumFileBytes)
                return Failure(Ra2VoxelSemanticSidecarFailureKind.TooLarge, "语义分划文件超过 32 MiB 上限。");
            cancellationToken.ThrowIfCancellationRequested();
            AtomicTextFileWriter.WriteAtomically(fullPath, json, StrictUtf8);
            return new(Ra2VoxelSemanticSidecarFailureKind.None, "语义分划已保存。", state);
        }
        catch (OperationCanceledException) { return Failure(Ra2VoxelSemanticSidecarFailureKind.Canceled, "语义分划保存已取消。"); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        { return Failure(Ra2VoxelSemanticSidecarFailureKind.IoFailure, $"无法保存语义分划：{ex.Message}"); }
    }

    internal Ra2VoxelSemanticSidecarResult Load(
        string projectRoot,
        string filePath,
        Ra2VoxelSceneSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2VoxelSemanticSidecarResult? pathFailure = ValidatePath(projectRoot, filePath, mustExist: true, out string fullPath);
            if (pathFailure is not null) return pathFailure;
            var info = new FileInfo(fullPath);
            if (info.Length > MaximumFileBytes)
                return Failure(Ra2VoxelSemanticSidecarFailureKind.TooLarge, "语义分划文件超过 32 MiB 上限。");
            byte[] bytes = File.ReadAllBytes(fullPath);
            cancellationToken.ThrowIfCancellationRequested();
            string json;
            try
            {
                ReadOnlySpan<byte> payload = bytes.AsSpan();
                if (payload.StartsWith(Encoding.UTF8.Preamble)) payload = payload[Encoding.UTF8.Preamble.Length..];
                json = StrictUtf8.GetString(payload);
            }
            catch (DecoderFallbackException)
            { return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidUtf8, "语义分划文件不是严格 UTF-8 文本。"); }

            using JsonDocument parsed = JsonDocument.Parse(json, new() { MaxDepth = 32, CommentHandling = JsonCommentHandling.Disallow, AllowTrailingCommas = false });
            if (HasDuplicateProperties(parsed.RootElement))
                return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidJson, "语义分划文件包含重复属性。");
            SidecarDocument? document;
            try
            {
                document = JsonSerializer.Deserialize<SidecarDocument>(json, new JsonSerializerOptions(WriterOptions)
                {
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
                });
            }
            catch (JsonException ex)
            { return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidJson, $"语义分划 JSON 无效：{ex.Message}"); }
            if (document is null) return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidJson, "语义分划 JSON 为空。");
            if (!string.Equals(document.Schema, Schema, StringComparison.Ordinal) || document.Version != Version)
                return Failure(Ra2VoxelSemanticSidecarFailureKind.UnsupportedSchema, "不支持该语义分划格式版本。");
            if (!IsHash(document.SourceSnapshotHash) || !IsHash(document.EvidencePackageHash) || !IsHash(document.ManualLayerHash) ||
                document.CellCount is < 0 or > Ra2VoxelSceneSnapshot.MaximumOccupancyCount)
                return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidShape, "语义分划根属性无效。");
            if (!string.Equals(document.SourceSnapshotHash, snapshot.CanonicalHash, StringComparison.OrdinalIgnoreCase) || document.CellCount != snapshot.OccupancyCount)
                return Failure(Ra2VoxelSemanticSidecarFailureKind.SnapshotMismatch, "语义分划不属于当前工作几何。");

            Ra2VoxelSemanticEvidencePackage evidence = Ra2VoxelSemanticEvidenceBuilder.Build(snapshot, cancellationToken);
            if (!string.Equals(document.EvidencePackageHash, evidence.PackageHash, StringComparison.OrdinalIgnoreCase))
                return Failure(Ra2VoxelSemanticSidecarFailureKind.EvidenceMismatch, "语义分划的区域证据与当前算法结果不一致。");
            HashSet<string> regionIds = evidence.Regions.Select(value => value.RegionId).ToHashSet(StringComparer.Ordinal);
            Ra2VoxelSemanticSidecarResult? assignmentFailure = ParseAssignments(document.AgentSuggestions, regionIds, "AI 建议", isHuman: false, out var suggestions);
            if (assignmentFailure is not null) return assignmentFailure;
            assignmentFailure = ParseAssignments(document.HumanRegionOverrides, regionIds, "人工区域覆盖", isHuman: true, out var overrides);
            if (assignmentFailure is not null) return assignmentFailure;
            if (document.AgentSuggestionsAccepted && suggestions.Count == 0)
                return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidShape, "已接受 AI 建议时至少需要一条 AI 建议。");
            Ra2VoxelSemanticSidecarResult? cellFailure = ParseCells(document.HumanCellGroups, snapshot.OccupancyCount, out var cells);
            if (cellFailure is not null) return cellFailure;
            var layer = new Ra2VoxelSemanticManualMaskLayer(snapshot.CanonicalHash, snapshot.OccupancyCount, cells);
            if (!string.Equals(document.ManualLayerHash, layer.LayerHash, StringComparison.OrdinalIgnoreCase))
                return Failure(Ra2VoxelSemanticSidecarFailureKind.LayerHashMismatch, "语义分划的人工体素层哈希不匹配。");
            var state = new Ra2VoxelSemanticSidecarState(evidence, document.AgentSuggestionsAccepted, suggestions, overrides, layer);
            return new(Ra2VoxelSemanticSidecarFailureKind.None, "语义分划已载入。", state);
        }
        catch (OperationCanceledException) { return Failure(Ra2VoxelSemanticSidecarFailureKind.Canceled, "语义分划载入已取消。"); }
        catch (JsonException ex) { return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidJson, $"语义分划 JSON 无效：{ex.Message}"); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        { return Failure(Ra2VoxelSemanticSidecarFailureKind.IoFailure, $"无法载入语义分划：{ex.Message}"); }
    }

    private static Ra2VoxelSemanticSidecarResult? ValidateState(Ra2VoxelSceneSnapshot snapshot, Ra2VoxelSemanticSidecarState state)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(state);
        if (!string.Equals(snapshot.CanonicalHash, state.Evidence.SourceSnapshotHash, StringComparison.Ordinal) ||
            !string.Equals(snapshot.CanonicalHash, state.HumanCellLayer.SourceSnapshotHash, StringComparison.Ordinal) ||
            snapshot.OccupancyCount != state.HumanCellLayer.CellCount)
            return Failure(Ra2VoxelSemanticSidecarFailureKind.SnapshotMismatch, "语义状态与当前工作几何不匹配。");
        HashSet<string> ids = state.Evidence.Regions.Select(value => value.RegionId).ToHashSet(StringComparer.Ordinal);
        Ra2VoxelSemanticEvidencePackage rebuilt = Ra2VoxelSemanticEvidenceBuilder.Build(snapshot);
        if (!string.Equals(rebuilt.PackageHash, state.Evidence.PackageHash, StringComparison.Ordinal))
            return Failure(Ra2VoxelSemanticSidecarFailureKind.EvidenceMismatch, "语义区域证据不是当前工作几何的确定性结果。");
        if ((state.AgentSuggestionsAccepted && state.AgentSuggestions.Count == 0) ||
            !AssignmentsValid(state.AgentSuggestions, ids, isHuman: false) ||
            !AssignmentsValid(state.HumanRegionOverrides, ids, isHuman: true))
            return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidShape, "语义区域赋值无效。");
        return null;
    }

    private static Ra2VoxelSemanticSidecarResult? ValidatePath(string projectRoot, string filePath, bool mustExist, out string fullPath)
    {
        fullPath = string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(projectRoot) || string.IsNullOrWhiteSpace(filePath) ||
                !filePath.EndsWith(".semantic.json", StringComparison.OrdinalIgnoreCase))
                return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidPath, "请选择 .semantic.json 文件。");
            string root = Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            fullPath = Path.GetFullPath(filePath);
            string prefix = root + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return Failure(Ra2VoxelSemanticSidecarFailureKind.OutsideProject, "语义分划文件必须位于当前项目目录内。");
            if (!Directory.Exists(root) || !Directory.Exists(Path.GetDirectoryName(fullPath)))
                return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidPath, "语义分划目标目录不存在。");
            for (string? cursor = Path.GetDirectoryName(fullPath); cursor is not null && cursor.StartsWith(root, StringComparison.OrdinalIgnoreCase); cursor = Path.GetDirectoryName(cursor))
            {
                if ((File.GetAttributes(cursor) & FileAttributes.ReparsePoint) != 0)
                    return Failure(Ra2VoxelSemanticSidecarFailureKind.ReparsePointRejected, "语义分划路径不能经过重解析点。");
                if (string.Equals(cursor, root, StringComparison.OrdinalIgnoreCase)) break;
            }
            if (File.Exists(fullPath) && (File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
                return Failure(Ra2VoxelSemanticSidecarFailureKind.ReparsePointRejected, "语义分划文件不能是重解析点。");
            if (mustExist && !File.Exists(fullPath))
                return Failure(Ra2VoxelSemanticSidecarFailureKind.NotFound, "找不到语义分划文件。");
            return null;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or IOException or UnauthorizedAccessException)
        { return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidPath, $"语义分划路径无效：{ex.Message}"); }
    }

    private static bool AssignmentsValid(IReadOnlyList<Ra2VoxelSemanticAssignment> values, HashSet<string> regionIds, bool isHuman) =>
        values.Count <= MaximumRegionAssignments &&
        values.Select(value => value.RegionId).Distinct(StringComparer.Ordinal).Count() == values.Count &&
        values.All(value => regionIds.Contains(value.RegionId) && Enum.IsDefined(value.PartRole) && Enum.IsDefined(value.MaterialRole) &&
            Enum.IsDefined(value.RemapIntent) && double.IsFinite(value.Confidence) && value.Confidence is >= 0 and <= 1 &&
            value.Reason is not null && value.Reason.Length <= MaximumReasonLength &&
            (isHuman ? value.Confidence == 1d : value.RemapIntent != Ra2VoxelSemanticRemapIntent.ExplicitlyApproved));

    private static Ra2VoxelSemanticSidecarResult? ParseAssignments(AssignmentDocument[]? documents, HashSet<string> regionIds, string label, bool isHuman, out IReadOnlyList<Ra2VoxelSemanticAssignment> values)
    {
        values = [];
        if (documents is null || documents.Length > MaximumRegionAssignments)
            return Failure(Ra2VoxelSemanticSidecarFailureKind.ResourceLimitExceeded, $"{label}数量超过限制或缺失。");
        var parsed = documents.Select(value => new Ra2VoxelSemanticAssignment(value.RegionId ?? string.Empty, value.PartRole, value.MaterialRole, value.RemapIntent, value.Confidence, value.Reason ?? string.Empty)).ToArray();
        if (!AssignmentsValid(parsed, regionIds, isHuman))
            return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidShape, $"{label}包含无效区域、枚举、置信度或重复项。");
        values = parsed;
        return null;
    }

    private static Ra2VoxelSemanticSidecarResult? ParseCells(CellGroupDocument[]? groups, int cellCount, out IReadOnlyList<Ra2VoxelSemanticCellOverride> values)
    {
        values = [];
        if (groups is null || groups.Length > cellCount)
            return Failure(Ra2VoxelSemanticSidecarFailureKind.ResourceLimitExceeded, "人工体素分组超过限制或缺失。");
        var result = new List<Ra2VoxelSemanticCellOverride>();
        var seen = new HashSet<int>();
        foreach (CellGroupDocument group in groups)
        {
            if (!Enum.IsDefined(group.PartRole) || !Enum.IsDefined(group.MaterialRole) || !Enum.IsDefined(group.RemapIntent) ||
                group.Reason is null || group.Reason.Length > MaximumReasonLength || group.CellIndices is null)
                return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidShape, "人工体素分组属性无效。");
            foreach (int index in group.CellIndices)
            {
                if (index < 0 || index >= cellCount || !seen.Add(index))
                    return Failure(Ra2VoxelSemanticSidecarFailureKind.InvalidShape, "人工体素索引越界或重复。");
                result.Add(new(index, group.PartRole, group.MaterialRole, group.RemapIntent, group.Reason));
            }
        }
        values = result.OrderBy(value => value.CellIndex).ToArray();
        return null;
    }

    private static AssignmentDocument ToDocument(Ra2VoxelSemanticAssignment value) =>
        new(value.RegionId, value.PartRole, value.MaterialRole, value.RemapIntent, value.Confidence, value.Reason);

    private static CellGroupDocument[] GroupCells(IReadOnlyList<Ra2VoxelSemanticCellOverride> values) => values
        .GroupBy(value => new { value.PartRole, value.MaterialRole, value.RemapIntent, value.Reason })
        .OrderBy(group => group.Key.PartRole).ThenBy(group => group.Key.MaterialRole).ThenBy(group => group.Key.RemapIntent).ThenBy(group => group.Key.Reason, StringComparer.Ordinal)
        .Select(group => new CellGroupDocument(group.Key.PartRole, group.Key.MaterialRole, group.Key.RemapIntent, group.Key.Reason,
            group.Select(value => value.CellIndex).OrderBy(value => value).ToArray()))
        .ToArray();

    private static bool HasDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in element.EnumerateObject())
                if (!names.Add(property.Name) || HasDuplicateProperties(property.Value)) return true;
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (JsonElement item in element.EnumerateArray()) if (HasDuplicateProperties(item)) return true;
        return false;
    }

    private static bool IsHash(string? value) => value is { Length: 64 } && value.All(char.IsAsciiHexDigit);
    private static Ra2VoxelSemanticSidecarResult Failure(Ra2VoxelSemanticSidecarFailureKind kind, string message) => new(kind, message);

    private sealed record SidecarDocument(
        string Schema, int Version, string SourceSnapshotHash, string EvidencePackageHash, int CellCount, string ManualLayerHash,
        bool AgentSuggestionsAccepted, AssignmentDocument[] AgentSuggestions, AssignmentDocument[] HumanRegionOverrides, CellGroupDocument[] HumanCellGroups);
    private sealed record AssignmentDocument(
        string RegionId, Ra2VoxelSemanticPartRole PartRole, Ra2VoxelSemanticMaterialRole MaterialRole,
        Ra2VoxelSemanticRemapIntent RemapIntent, double Confidence, string Reason);
    private sealed record CellGroupDocument(
        Ra2VoxelSemanticPartRole PartRole, Ra2VoxelSemanticMaterialRole MaterialRole,
        Ra2VoxelSemanticRemapIntent RemapIntent, string Reason, int[] CellIndices);
}
