using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.IO;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Cleanup;

public sealed class FieldRegistryGeneralizationCleanupApplyWriter
{
    private const string ManifestFileName = "manifest.json";
    private const string RepairManifestFileName = "generalization-repair-manifest.json";
    private const string RepairOperationName = "GeneralizationRepair";

    private static readonly JsonSerializerOptions ReadJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly JsonSerializerOptions WriteJsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly Ra2SectionKind[] UnitKinds =
    [
        Ra2SectionKind.Infantry,
        Ra2SectionKind.Vehicle,
        Ra2SectionKind.Aircraft
    ];

    private static readonly Ra2SectionKind[] TechnoKinds =
    [
        Ra2SectionKind.Infantry,
        Ra2SectionKind.Vehicle,
        Ra2SectionKind.Aircraft,
        Ra2SectionKind.Building
    ];

    private readonly IFieldRegistryApplyPathResolver _pathResolver;

    public FieldRegistryGeneralizationCleanupApplyWriter()
        : this(new FieldRegistryApplyPathResolver())
    {
    }

    internal FieldRegistryGeneralizationCleanupApplyWriter(IFieldRegistryApplyPathResolver pathResolver)
    {
        _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
    }

    public FieldRegistryGeneralizationCleanupApplyResult ApplyGlobal(
        string globalFieldRegistryRootPath,
        DateTimeOffset? timestamp = null)
    {
        return Apply(new FieldRegistryGeneralizationCleanupApplyRequest(
            FieldRegistryApplyTargetScope.Global,
            null,
            globalFieldRegistryRootPath,
            timestamp));
    }

    public FieldRegistryGeneralizationRepairPreview BuildGlobalPreview(string globalFieldRegistryRootPath)
    {
        return BuildPreview(new FieldRegistryGeneralizationCleanupApplyRequest(
            FieldRegistryApplyTargetScope.Global,
            null,
            globalFieldRegistryRootPath));
    }

    public FieldRegistryGeneralizationCleanupApplyResult ApplyProject(
        string projectRootPath,
        string globalFieldRegistryRootPath,
        DateTimeOffset? timestamp = null)
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
            throw new ArgumentException("Project root path cannot be empty.", nameof(projectRootPath));

        return Apply(new FieldRegistryGeneralizationCleanupApplyRequest(
            FieldRegistryApplyTargetScope.Project,
            projectRootPath,
            globalFieldRegistryRootPath,
            timestamp));
    }

    internal FieldRegistryGeneralizationCleanupApplyResult Apply(FieldRegistryGeneralizationCleanupApplyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string targetFilePath = _pathResolver.ResolveTargetPackPath(
            request.TargetScope,
            request.ProjectRootPath,
            request.GlobalFieldRegistryRootPath,
            FieldRegistryApplyWriteRequest.DefaultTargetPackFileName);
        if (!File.Exists(targetFilePath))
        {
            return new FieldRegistryGeneralizationCleanupApplyResult(
                targetFilePath,
                null,
                null,
                0,
                0,
                0,
                0,
                [$"Target pack does not exist: {targetFilePath}"]);
        }

        FieldRegistryPackDto pack = LoadPack(targetFilePath);
        pack.Fields ??= new List<FieldRegistryFieldDto>();
        RepairSummary summary = ApplyRepairs(pack.Fields);

        if (summary.Added.Count == 0 && summary.Updated.Count == 0 && summary.Removed.Count == 0)
        {
            return new FieldRegistryGeneralizationCleanupApplyResult(
                targetFilePath,
                null,
                null,
                0,
                0,
                0,
                summary.Skipped.Count,
                Array.AsReadOnly(summary.Warnings.ToArray()));
        }

        string backupDirectory = CreateUniqueBackupDirectory(_pathResolver.ResolveBackupDirectory(
            request.TargetScope,
            request.ProjectRootPath,
            request.GlobalFieldRegistryRootPath,
            request.Timestamp));
        Directory.CreateDirectory(backupDirectory);
        string backupFilePath = Path.Combine(backupDirectory, Path.GetFileName(targetFilePath));
        File.Copy(targetFilePath, backupFilePath, overwrite: false);

        string manifestFilePath = Path.Combine(backupDirectory, ManifestFileName);
        string repairManifestFilePath = Path.Combine(backupDirectory, RepairManifestFileName);
        FieldRegistryGeneralizationRepairManifest repairManifest = new(
            RepairOperationName,
            request.Timestamp.UtcDateTime.ToString("O"),
            targetFilePath,
            backupFilePath,
            "user-import.fields.json",
            Array.AsReadOnly(summary.Added.ToArray()),
            Array.AsReadOnly(summary.Updated.ToArray()),
            Array.AsReadOnly(summary.Removed.ToArray()),
            Array.AsReadOnly(summary.Skipped.ToArray()),
            Array.AsReadOnly(summary.Warnings.ToArray()));
        AtomicTextFileWriter.WriteText(
            repairManifestFilePath,
            JsonSerializer.Serialize(repairManifest, WriteJsonOptions),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        FieldRegistryApplyBackupManifest manifest = new(
            request.TargetScope.ToString(),
            targetFilePath,
            backupFilePath,
            targetFileExisted: true,
            request.Timestamp.UtcDateTime.ToString("O"),
            summary.Added.Count,
            summary.Updated.Count,
            summary.Skipped.Count + summary.Removed.Count,
            RepairOperationName);
        AtomicTextFileWriter.WriteText(
            manifestFilePath,
            JsonSerializer.Serialize(manifest, WriteJsonOptions),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        AtomicTextFileWriter.WriteText(
            targetFilePath,
            JsonSerializer.Serialize(pack, WriteJsonOptions),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        return new FieldRegistryGeneralizationCleanupApplyResult(
            targetFilePath,
            backupDirectory,
            manifestFilePath,
            summary.Added.Count,
            summary.Updated.Count,
            summary.Removed.Count,
            summary.Skipped.Count,
            Array.AsReadOnly(summary.Warnings.ToArray()));
    }

    internal FieldRegistryGeneralizationRepairPreview BuildPreview(FieldRegistryGeneralizationCleanupApplyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string targetFilePath = _pathResolver.ResolveTargetPackPath(
            request.TargetScope,
            request.ProjectRootPath,
            request.GlobalFieldRegistryRootPath,
            FieldRegistryApplyWriteRequest.DefaultTargetPackFileName);
        if (!File.Exists(targetFilePath))
        {
            return new FieldRegistryGeneralizationRepairPreview(
                FieldRegistryApplyWriteRequest.DefaultTargetPackFileName,
                targetFilePath,
                Array.Empty<FieldRegistryGeneralizationAbstractFieldPreview>(),
                Array.Empty<FieldRegistryGeneralizationRemovedConcreteFieldPreview>(),
                Array.Empty<FieldRegistryGeneralizationSkippedFieldPreview>(),
                [$"Target pack does not exist: {targetFilePath}"]);
        }

        FieldRegistryPackDto pack = ClonePack(LoadPack(targetFilePath));
        pack.Fields ??= new List<FieldRegistryFieldDto>();
        RepairSummary summary = ApplyRepairs(pack.Fields);
        return ToPreview(targetFilePath, summary);
    }

    private static RepairSummary ApplyRepairs(List<FieldRegistryFieldDto> fields)
    {
        RepairSummary summary = new();
        foreach (IGrouping<string, FieldRegistryFieldDto> group in fields
            .Where(field => !string.IsNullOrWhiteSpace(field.Key))
            .GroupBy(field => field.Key!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToArray())
        {
            if (TryApplyGroup(fields, group.Key, TechnoKinds, Ra2SectionKind.Techno, summary))
                continue;

            TryApplyGroup(fields, group.Key, UnitKinds, Ra2SectionKind.Unit, summary);
        }

        return summary;
    }

    private static bool TryApplyGroup(
        List<FieldRegistryFieldDto> fields,
        string key,
        IReadOnlyList<Ra2SectionKind> requiredKinds,
        Ra2SectionKind targetKind,
        RepairSummary summary)
    {
        List<FieldRegistryFieldDto> groupFields = fields
            .Where(field => string.Equals(field.Key?.Trim(), key, StringComparison.OrdinalIgnoreCase))
            .ToList();
        List<FieldRegistryFieldDto> matches = requiredKinds
            .Select(kind => groupFields.LastOrDefault(field => FieldMatches(field, key, kind)))
            .Where(field => field is not null)
            .Cast<FieldRegistryFieldDto>()
            .ToList();
        if (matches.Count != requiredKinds.Count)
            return false;

        if (!matches.All(match => IsCompatible(matches[0], match)))
        {
            string reason = $"incompatible concrete field schema for {targetKind}.";
            summary.Skipped.Add($"{key}: {reason}");
            summary.SkippedFields.Add(new FieldRegistryGeneralizationSkippedFieldPreview(
                key,
                string.Join(", ", matches.Select(match => TryGetSingleAppliesTo(match, out string appliesTo) ? appliesTo : "Unknown")),
                reason));
            return false;
        }

        FieldRegistryFieldDto? existingTarget = groupFields.LastOrDefault(field => FieldMatches(field, key, targetKind));
        if (existingTarget is not null && !IsCompatible(matches[0], existingTarget))
        {
            string reason = $"existing {targetKind} field is not compatible with concrete fields.";
            summary.Skipped.Add($"{key}: {reason}");
            summary.SkippedFields.Add(new FieldRegistryGeneralizationSkippedFieldPreview(
                key,
                string.Join(", ", matches.Select(match => TryGetSingleAppliesTo(match, out string appliesTo) ? appliesTo : "Unknown")),
                reason));
            return false;
        }

        FieldRegistryFieldDto generalized = CreateGeneralizedDto(matches, existingTarget, targetKind, summary.Warnings);
        if (existingTarget is null)
        {
            fields.Add(generalized);
            summary.Added.Add(DescribeField(key, targetKind));
        }
        else
        {
            CopyDto(generalized, existingTarget);
            summary.Updated.Add(DescribeField(key, targetKind));
        }

        summary.AbstractFields.Add(new FieldRegistryGeneralizationAbstractFieldPreview(
            existingTarget is null ? "新增" : "更新",
            key,
            targetKind.ToString(),
            requiredKinds.Select(kind => kind.ToString()).ToArray(),
            GetValueKindText(generalized),
            GetAllowedValueTexts(generalized),
            existingTarget is null
                ? "将新增抽象字段，并移除兼容的具体重复字段。"
                : "将更新已有抽象字段，并移除兼容的具体重复字段。"));

        foreach (FieldRegistryFieldDto match in matches)
        {
            if (fields.Remove(match) && TryGetSingleAppliesTo(match, out string appliesTo))
            {
                summary.Removed.Add(DescribeField(key, appliesTo));
                summary.RemovedConcreteFields.Add(new FieldRegistryGeneralizationRemovedConcreteFieldPreview(
                    key,
                    appliesTo,
                    targetKind.ToString(),
                    "schema compatible"));
            }
        }

        return true;
    }

    private static bool FieldMatches(FieldRegistryFieldDto field, string key, Ra2SectionKind appliesTo)
    {
        return string.Equals(field.Key?.Trim(), key, StringComparison.OrdinalIgnoreCase) &&
               NormalizeAppliesTo(field.AppliesTo).Length == 1 &&
               string.Equals(NormalizeAppliesTo(field.AppliesTo)[0], appliesTo.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCompatible(FieldRegistryFieldDto left, FieldRegistryFieldDto right)
    {
        return string.Equals(left.EditorKind, right.EditorKind, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(left.Schema?.Type, right.Schema?.Type, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(left.Schema?.BooleanStyle, right.Schema?.BooleanStyle, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(left.Schema?.Separator ?? ",", right.Schema?.Separator ?? ",", StringComparison.Ordinal) &&
               string.Equals(left.Schema?.EnumName, right.Schema?.EnumName, StringComparison.OrdinalIgnoreCase);
    }

    private static FieldRegistryFieldDto CreateGeneralizedDto(
        IReadOnlyList<FieldRegistryFieldDto> concreteFields,
        FieldRegistryFieldDto? existingTarget,
        Ra2SectionKind targetKind,
        List<string> warnings)
    {
        FieldRegistryFieldDto first = existingTarget ?? concreteFields[0];
        string? displayName = existingTarget?.DisplayName ?? FirstNonWhiteSpace(concreteFields.Select(field => field.DisplayName));
        string? description = existingTarget?.Description ?? FirstNonWhiteSpace(concreteFields.Select(field => field.Description));
        AddMetadataConflictWarnings(first.Key, targetKind, concreteFields, existingTarget, warnings);

        return new FieldRegistryFieldDto
        {
            Key = first.Key,
            AppliesTo = [targetKind.ToString()],
            EditorKind = first.EditorKind,
            SourceKind = first.SourceKind,
            Description = description,
            DisplayName = displayName,
            Aliases = MergeAliases(concreteFields, existingTarget),
            Schema = MergeSchema(concreteFields, existingTarget)
        };
    }

    private static FieldRegistryValueSchemaDto? MergeSchema(
        IReadOnlyList<FieldRegistryFieldDto> concreteFields,
        FieldRegistryFieldDto? existingTarget)
    {
        FieldRegistryValueSchemaDto? first = existingTarget?.Schema ?? concreteFields[0].Schema;
        if (first is null)
            return null;

        return new FieldRegistryValueSchemaDto
        {
            Type = first.Type,
            BooleanStyle = first.BooleanStyle,
            EnumName = first.EnumName,
            Separator = first.Separator,
            AllowedValues = MergeAllowedValues(concreteFields, existingTarget)
        };
    }

    private static List<FieldRegistryAllowedValueDto> MergeAllowedValues(
        IReadOnlyList<FieldRegistryFieldDto> concreteFields,
        FieldRegistryFieldDto? existingTarget)
    {
        Dictionary<string, FieldRegistryAllowedValueDto> values = new(StringComparer.OrdinalIgnoreCase);
        AddAllowedValues(existingTarget, values, overwrite: false);
        foreach (FieldRegistryFieldDto field in concreteFields)
            AddAllowedValues(field, values, overwrite: false);

        return values.Values
            .OrderBy(value => value.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AddAllowedValues(
        FieldRegistryFieldDto? field,
        Dictionary<string, FieldRegistryAllowedValueDto> values,
        bool overwrite)
    {
        if (field?.Schema?.AllowedValues is null)
            return;

        foreach (FieldRegistryAllowedValueDto value in field.Schema.AllowedValues)
        {
            if (string.IsNullOrWhiteSpace(value.Value))
                continue;

            string key = value.Value.Trim();
            if (overwrite || !values.ContainsKey(key))
                values[key] = CloneAllowedValue(value);
        }
    }

    private static FieldRegistryAllowedValueDto CloneAllowedValue(FieldRegistryAllowedValueDto value)
    {
        return new FieldRegistryAllowedValueDto
        {
            Value = value.Value,
            DisplayName = value.DisplayName,
            Description = value.Description,
            Priority = value.Priority
        };
    }

    private static void CopyDto(FieldRegistryFieldDto source, FieldRegistryFieldDto target)
    {
        target.Key = source.Key;
        target.AppliesTo = source.AppliesTo;
        target.EditorKind = source.EditorKind;
        target.SourceKind = source.SourceKind;
        target.Description = source.Description;
        target.DisplayName = source.DisplayName;
        target.Aliases = source.Aliases;
        target.Schema = source.Schema;
    }

    private static void AddMetadataConflictWarnings(
        string? key,
        Ra2SectionKind targetKind,
        IReadOnlyList<FieldRegistryFieldDto> concreteFields,
        FieldRegistryFieldDto? existingTarget,
        List<string> warnings)
    {
        AddFieldTextConflictWarning(key, targetKind, "displayName", concreteFields.Select(field => field.DisplayName), existingTarget?.DisplayName, warnings);
        AddFieldTextConflictWarning(key, targetKind, "description", concreteFields.Select(field => field.Description), existingTarget?.Description, warnings);

        IEnumerable<IGrouping<string, FieldRegistryAllowedValueDto>> valueGroups = concreteFields
            .Concat(existingTarget is null ? [] : [existingTarget])
            .SelectMany(field => field.Schema?.AllowedValues ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value.Value))
            .GroupBy(value => value.Value!.Trim(), StringComparer.OrdinalIgnoreCase);
        foreach (IGrouping<string, FieldRegistryAllowedValueDto> group in valueGroups)
        {
            if (HasConflict(group.Select(value => value.DisplayName)) ||
                HasConflict(group.Select(value => value.Description)))
                warnings.Add($"{key}: allowed value '{group.Key}' metadata conflict while merging into {targetKind}; existing abstract metadata is preferred when available.");
        }
    }

    private static void AddFieldTextConflictWarning(
        string? key,
        Ra2SectionKind targetKind,
        string propertyName,
        IEnumerable<string?> concreteValues,
        string? abstractValue,
        List<string> warnings)
    {
        List<string> values = concreteValues
            .Append(abstractValue)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (values.Count > 1)
            warnings.Add($"{key}: {propertyName} conflict while merging into {targetKind}; existing abstract value is preferred when available.");
    }

    private static bool HasConflict(IEnumerable<string?> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .Count() > 1;
    }

    private static string? FirstNonWhiteSpace(IEnumerable<string?> values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static List<string>? MergeAliases(IReadOnlyList<FieldRegistryFieldDto> concreteFields, FieldRegistryFieldDto? existingTarget)
    {
        List<string> aliases = concreteFields
            .Concat(existingTarget is null ? [] : [existingTarget])
            .SelectMany(field => field.Aliases ?? [])
            .Where(alias => !string.IsNullOrWhiteSpace(alias))
            .Select(alias => alias.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(alias => alias, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return aliases.Count == 0 ? null : aliases;
    }

    private static bool TryGetSingleAppliesTo(FieldRegistryFieldDto field, out string appliesTo)
    {
        string[] values = NormalizeAppliesTo(field.AppliesTo);
        appliesTo = values.Length == 1 ? values[0] : string.Empty;
        return values.Length == 1;
    }

    private static string DescribeField(string? key, Ra2SectionKind kind)
    {
        return $"{key} | {kind}";
    }

    private static string DescribeField(string? key, string appliesTo)
    {
        return $"{key} | {appliesTo}";
    }

    private static FieldRegistryPackDto LoadPack(string targetFilePath)
    {
        string json = File.ReadAllText(targetFilePath);
        FieldRegistryPackDto? pack = string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<FieldRegistryPackDto>(json, ReadJsonOptions);
        return pack ?? new FieldRegistryPackDto { Fields = new List<FieldRegistryFieldDto>() };
    }

    private static FieldRegistryPackDto ClonePack(FieldRegistryPackDto pack)
    {
        return JsonSerializer.Deserialize<FieldRegistryPackDto>(
                   JsonSerializer.Serialize(pack, WriteJsonOptions),
                   ReadJsonOptions) ??
               new FieldRegistryPackDto { Fields = new List<FieldRegistryFieldDto>() };
    }

    private static FieldRegistryGeneralizationRepairPreview ToPreview(string targetFilePath, RepairSummary summary)
    {
        return new FieldRegistryGeneralizationRepairPreview(
            Path.GetFileName(targetFilePath),
            targetFilePath,
            Array.AsReadOnly(summary.AbstractFields.ToArray()),
            Array.AsReadOnly(summary.RemovedConcreteFields.ToArray()),
            Array.AsReadOnly(summary.SkippedFields.ToArray()),
            Array.AsReadOnly(summary.Warnings.ToArray()));
    }

    private static string GetValueKindText(FieldRegistryFieldDto field)
    {
        return field.Schema?.Type ?? field.EditorKind ?? string.Empty;
    }

    private static IReadOnlyList<string> GetAllowedValueTexts(FieldRegistryFieldDto field)
    {
        return Array.AsReadOnly((field.Schema?.AllowedValues ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value.Value))
            .Select(value => value.Value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray());
    }

    private static string[] NormalizeAppliesTo(IReadOnlyList<string>? values)
    {
        if (values is null || values.Count == 0)
            return [Ra2SectionKind.Unknown.ToString()];

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string CreateUniqueBackupDirectory(string preferredDirectory)
    {
        if (!Directory.Exists(preferredDirectory))
            return preferredDirectory;

        for (int index = 1; index < 1000; index++)
        {
            string candidate = $"{preferredDirectory}-{index:000}";
            if (!Directory.Exists(candidate))
                return candidate;
        }

        throw new IOException($"Could not allocate a unique backup directory for '{preferredDirectory}'.");
    }

    private sealed class RepairSummary
    {
        public List<FieldRegistryGeneralizationAbstractFieldPreview> AbstractFields { get; } = new();

        public List<FieldRegistryGeneralizationRemovedConcreteFieldPreview> RemovedConcreteFields { get; } = new();

        public List<FieldRegistryGeneralizationSkippedFieldPreview> SkippedFields { get; } = new();

        public List<string> Added { get; } = new();

        public List<string> Updated { get; } = new();

        public List<string> Removed { get; } = new();

        public List<string> Skipped { get; } = new();

        public List<string> Warnings { get; } = new();
    }
}

internal sealed class FieldRegistryGeneralizationRepairManifest
{
    public FieldRegistryGeneralizationRepairManifest(
        string operation,
        string timestampUtc,
        string targetFilePath,
        string backupFilePath,
        string targetPackFileName,
        IReadOnlyList<string> addedAbstractFields,
        IReadOnlyList<string> updatedAbstractFields,
        IReadOnlyList<string> removedConcreteFields,
        IReadOnlyList<string> skippedFields,
        IReadOnlyList<string> warnings)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        TimestampUtc = timestampUtc ?? throw new ArgumentNullException(nameof(timestampUtc));
        TargetFilePath = targetFilePath ?? throw new ArgumentNullException(nameof(targetFilePath));
        BackupFilePath = backupFilePath ?? throw new ArgumentNullException(nameof(backupFilePath));
        TargetPackFileName = targetPackFileName ?? throw new ArgumentNullException(nameof(targetPackFileName));
        AddedAbstractFields = addedAbstractFields ?? throw new ArgumentNullException(nameof(addedAbstractFields));
        UpdatedAbstractFields = updatedAbstractFields ?? throw new ArgumentNullException(nameof(updatedAbstractFields));
        RemovedConcreteFields = removedConcreteFields ?? throw new ArgumentNullException(nameof(removedConcreteFields));
        SkippedFields = skippedFields ?? throw new ArgumentNullException(nameof(skippedFields));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    public string SchemaVersion { get; } = "1";

    public string Operation { get; }

    public string TimestampUtc { get; }

    public string TargetFilePath { get; }

    public string BackupFilePath { get; }

    public string TargetPackFileName { get; }

    public IReadOnlyList<string> AddedAbstractFields { get; }

    public IReadOnlyList<string> UpdatedAbstractFields { get; }

    public IReadOnlyList<string> RemovedConcreteFields { get; }

    public IReadOnlyList<string> SkippedFields { get; }

    public IReadOnlyList<string> Warnings { get; }
}
