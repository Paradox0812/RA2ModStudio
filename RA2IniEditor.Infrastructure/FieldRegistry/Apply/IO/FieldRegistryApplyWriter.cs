using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.IO;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;

internal sealed class FieldRegistryApplyWriter : IFieldRegistryApplyWriter
{
    private const string ManifestFileName = "manifest.json";

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

    private readonly IFieldRegistryApplyPathResolver _pathResolver;

    public FieldRegistryApplyWriter()
        : this(new FieldRegistryApplyPathResolver())
    {
    }

    public FieldRegistryApplyWriter(IFieldRegistryApplyPathResolver pathResolver)
    {
        _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
    }

    public FieldRegistryApplyWriteResult Write(FieldRegistryApplyWriteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        string targetFilePath = _pathResolver.ResolveTargetPackPath(
            request.Plan.TargetScope,
            request.ProjectRootPath,
            request.GlobalFieldRegistryRootPath,
            request.TargetPackFileName);

        if (request.Plan.AddCount == 0 && request.Plan.UpdateCount == 0)
        {
            return new FieldRegistryApplyWriteResult(
                targetFilePath,
                null,
                null,
                0,
                0,
                request.Plan.SkipCount,
                []);
        }

        string backupDirectory = CreateUniqueBackupDirectory(_pathResolver.ResolveBackupDirectory(
            request.Plan.TargetScope,
            request.ProjectRootPath,
            request.GlobalFieldRegistryRootPath,
            request.Timestamp));
        Directory.CreateDirectory(backupDirectory);

        bool targetFileExisted = File.Exists(targetFilePath);
        string? backupFilePath = null;
        if (targetFileExisted)
        {
            backupFilePath = Path.Combine(backupDirectory, Path.GetFileName(targetFilePath));
            File.Copy(targetFilePath, backupFilePath, overwrite: false);
        }

        FieldRegistryPackDto pack = targetFileExisted
            ? LoadPack(targetFilePath)
            : CreateDefaultPack(request.Timestamp);
        pack.Fields ??= new List<FieldRegistryFieldDto>();

        List<string> warnings = new();
        int addedCount = 0;
        int updatedCount = 0;
        foreach (FieldRegistryApplyPlanItem item in request.Plan.Items)
        {
            if (item.OperationKind == FieldRegistryApplyOperationKind.Add)
            {
                pack.Fields.Add(ToDto(item));
                addedCount++;
                continue;
            }

            if (item.OperationKind == FieldRegistryApplyOperationKind.Update)
            {
                FieldRegistryFieldDto? existing = FindMatchingField(pack.Fields, item);
                if (existing is null)
                {
                    pack.Fields.Add(ToDto(item));
                    addedCount++;
                    warnings.Add($"Update target '{item.Key}' for {item.AppliesTo} was not found; added it instead.");
                    continue;
                }

                UpdateDto(existing, item);
                updatedCount++;
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
        AtomicTextFileWriter.WriteText(
            targetFilePath,
            JsonSerializer.Serialize(pack, WriteJsonOptions),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        FieldRegistryApplyBackupManifest manifest = new(
            request.Plan.TargetScope.ToString(),
            targetFilePath,
            backupFilePath,
            targetFileExisted,
            request.Timestamp.UtcDateTime.ToString("O"),
            addedCount,
            updatedCount,
            request.Plan.SkipCount,
            request.Plan.Mode.ToString());
        string manifestFilePath = Path.Combine(backupDirectory, ManifestFileName);
        AtomicTextFileWriter.WriteText(
            manifestFilePath,
            JsonSerializer.Serialize(manifest, WriteJsonOptions),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        return new FieldRegistryApplyWriteResult(
            targetFilePath,
            backupDirectory,
            manifestFilePath,
            addedCount,
            updatedCount,
            request.Plan.SkipCount,
            Array.AsReadOnly(warnings.ToArray()));
    }

    private static void ValidateRequest(FieldRegistryApplyWriteRequest request)
    {
        if (!request.Plan.CanApplyInFuture || request.Plan.ErrorCount > 0 || request.Plan.RejectCount > 0)
            throw new InvalidOperationException("Apply plan contains errors or rejects and cannot be written.");

        if (request.Plan.TargetScope == FieldRegistryApplyTargetScope.Project &&
            string.IsNullOrWhiteSpace(request.ProjectRootPath))
        {
            throw new InvalidOperationException("Project target requires a project root path.");
        }

        if (string.IsNullOrWhiteSpace(request.GlobalFieldRegistryRootPath))
            throw new ArgumentException("Global field registry root path cannot be empty.", nameof(request));

        FieldRegistryApplyPathResolver.ValidateTargetPackFileName(request.TargetPackFileName);
    }

    private static FieldRegistryPackDto LoadPack(string targetFilePath)
    {
        string json = File.ReadAllText(targetFilePath);
        FieldRegistryPackDto? pack = string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<FieldRegistryPackDto>(json, ReadJsonOptions);
        return pack ?? CreateDefaultPack(DateTimeOffset.UtcNow);
    }

    private static FieldRegistryPackDto CreateDefaultPack(DateTimeOffset timestamp)
    {
        return new FieldRegistryPackDto
        {
            Name = "User Import",
            Kind = "User",
            Version = "local-user-import",
            GeneratedAt = timestamp.UtcDateTime.ToString("O"),
            Fields = new List<FieldRegistryFieldDto>()
        };
    }

    private static FieldRegistryFieldDto ToDto(FieldRegistryApplyPlanItem item)
    {
        return new FieldRegistryFieldDto
        {
            Key = item.PreviewDefinition.Key,
            AppliesTo = [item.AppliesTo.ToString()],
            EditorKind = item.PreviewDefinition.EditorKind.ToString(),
            SourceKind = item.PreviewDefinition.SourceKind.ToString(),
            Description = item.PreviewDefinition.Description,
            DisplayName = item.PreviewDefinition.DisplayName,
            Aliases = item.PreviewDefinition.Aliases.Count == 0
                ? null
                : item.PreviewDefinition.Aliases.ToList(),
            Schema = ToSchemaDto(item.PreviewDefinition.ValueMetadata)
        };
    }

    private static void UpdateDto(FieldRegistryFieldDto target, FieldRegistryApplyPlanItem item)
    {
        target.Key = item.PreviewDefinition.Key;
        target.AppliesTo = [item.AppliesTo.ToString()];
        target.EditorKind = item.PreviewDefinition.EditorKind.ToString();
        target.SourceKind = item.PreviewDefinition.SourceKind.ToString();
        target.Description = item.PreviewDefinition.Description;
        target.DisplayName = item.PreviewDefinition.DisplayName;
        target.Aliases = item.PreviewDefinition.Aliases.Count == 0
            ? null
            : item.PreviewDefinition.Aliases.ToList();
        target.Schema = ToSchemaDto(item.PreviewDefinition.ValueMetadata);
    }

    private static FieldRegistryValueSchemaDto? ToSchemaDto(Ra2FieldValueMetadata metadata)
    {
        if (!metadata.HasSchema)
            return null;

        return new FieldRegistryValueSchemaDto
        {
            Type = metadata.ValueKind == Ra2FieldValueKind.Unknown ? null : metadata.ValueKind.ToString(),
            BooleanStyle = metadata.BooleanStyle == Ra2FieldBooleanValueStyle.Unknown ? null : metadata.BooleanStyle.ToString(),
            EnumName = metadata.EnumName,
            Separator = metadata.Separator == "," ? null : metadata.Separator,
            AllowedValues = metadata.AllowedValues.Count == 0
                ? null
                : metadata.AllowedValues.Select(value => new FieldRegistryAllowedValueDto
                {
                    Value = value.Value,
                    DisplayName = value.DisplayName,
                    Description = value.Description,
                    Priority = value.Priority == 0 ? null : value.Priority
                }).ToList()
        };
    }

    private static FieldRegistryFieldDto? FindMatchingField(List<FieldRegistryFieldDto> fields, FieldRegistryApplyPlanItem item)
    {
        return fields.FirstOrDefault(field =>
            string.Equals(field.Key?.Trim(), item.Key, StringComparison.OrdinalIgnoreCase) &&
            AppliesToSetEquals(field.AppliesTo, item.AppliesTo));
    }

    private static bool AppliesToSetEquals(IReadOnlyList<string>? values, Ra2SectionKind appliesTo)
    {
        string[] normalized = NormalizeAppliesTo(values);
        return normalized.Length == 1 &&
            string.Equals(normalized[0], appliesTo.ToString(), StringComparison.OrdinalIgnoreCase);
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
}
