using System.Text.Encodings.Web;
using System.Text.Json;
using System.IO;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2FieldAnnotationJsonStore : IRa2FieldAnnotationStore
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    public Ra2FieldAnnotationLoadResult Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return new Ra2FieldAnnotationLoadResult(Ra2FieldAnnotationPack.Empty(), ["Annotation sidecar was not found."]);

        try
        {
            string json = File.ReadAllText(path);
            FieldAnnotationPackDto? dto = JsonSerializer.Deserialize<FieldAnnotationPackDto>(json, ReadOptions);
            if (dto is null)
                return new Ra2FieldAnnotationLoadResult(Ra2FieldAnnotationPack.Empty(), ["Annotation sidecar is empty."], success: false);

            List<string> warnings = [];
            List<Ra2FieldAnnotationEntry> entries = [];
            HashSet<string> seenKeys = new(StringComparer.OrdinalIgnoreCase);
            foreach (FieldAnnotationEntryDto entry in dto.Entries ?? [])
            {
                if (string.IsNullOrWhiteSpace(entry.SectionKind) || string.IsNullOrWhiteSpace(entry.Key))
                {
                    warnings.Add("Skipped annotation entry with empty sectionKind or key.");
                    continue;
                }

                string identity = $"{entry.SectionKind.Trim()}::{entry.Key.Trim()}";
                if (!seenKeys.Add(identity))
                    warnings.Add($"Duplicate annotation entry '{identity}' uses last-wins behavior.");

                entries.Add(new Ra2FieldAnnotationEntry(
                    entry.SectionKind,
                    entry.Key,
                    entry.DisplayName ?? string.Empty,
                    entry.Aliases ?? [],
                    entry.Note));
            }

            return new Ra2FieldAnnotationLoadResult(new Ra2FieldAnnotationPack(
                dto.Version <= 0 ? 1 : dto.Version,
                dto.Language ?? "zh-CN",
                entries), warnings);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return new Ra2FieldAnnotationLoadResult(
                Ra2FieldAnnotationPack.Empty(),
                [$"Failed to load annotation sidecar: {ex.Message}"],
                success: false);
        }
    }

    public Ra2FieldAnnotationSaveResult Save(string path, Ra2FieldAnnotationPack pack)
    {
        try
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            FieldAnnotationPackDto dto = new()
            {
                Version = pack.Version,
                Language = pack.Language,
                Entries = pack.Entries.Select(entry => new FieldAnnotationEntryDto
                {
                    SectionKind = entry.SectionKind,
                    Key = entry.Key,
                    DisplayName = entry.DisplayName,
                    Aliases = entry.Aliases.ToArray(),
                    Note = entry.Note
                }).ToArray()
            };
            File.WriteAllText(path, JsonSerializer.Serialize(dto, WriteOptions));
            return Ra2FieldAnnotationSaveResult.Succeeded();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Ra2FieldAnnotationSaveResult.Failed(ex.Message);
        }
    }

    private sealed class FieldAnnotationPackDto
    {
        public int Version { get; set; }

        public string? Language { get; set; }

        public FieldAnnotationEntryDto[]? Entries { get; set; }
    }

    private sealed class FieldAnnotationEntryDto
    {
        public string? SectionKind { get; set; }

        public string? Key { get; set; }

        public string? DisplayName { get; set; }

        public string[]? Aliases { get; set; }

        public string? Note { get; set; }
    }
}
