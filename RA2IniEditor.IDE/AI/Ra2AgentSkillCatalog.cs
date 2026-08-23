using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RA2IniEditor.IDE.AI;

[Flags]
internal enum Ra2AgentSkillMode
{
    None = 0,
    Chat = 1,
    Work = 2,
    Both = Chat | Work
}

internal sealed record Ra2AgentSkillDescriptor(
    string Name,
    string Description,
    string Version,
    IReadOnlyList<string> Domains,
    Ra2AgentSkillMode Modes,
    string Instructions,
    string ContentHash);

internal sealed class Ra2AgentSkillCatalog
{
    internal const int MaximumSkillCount = 64;
    internal const int MaximumSkillCharacters = 96 * 1024;
    internal const int MaximumSelectedSkillCharacters = 14 * 1024;

    private static readonly Regex SkillNamePattern = new(
        "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly IReadOnlyList<Ra2AgentSkillDescriptor> _skills;

    internal Ra2AgentSkillCatalog(IEnumerable<Ra2AgentSkillDescriptor> skills)
    {
        ArgumentNullException.ThrowIfNull(skills);
        Ra2AgentSkillDescriptor[] array = skills.ToArray();
        if (array.Length > MaximumSkillCount || array.Any(skill => skill is null))
            throw new ArgumentException("The built-in skill catalog is invalid.", nameof(skills));
        if (array.GroupBy(skill => skill.Name, StringComparer.Ordinal).Any(group => group.Count() > 1))
            throw new ArgumentException("Built-in skill names must be unique.", nameof(skills));
        _skills = Array.AsReadOnly(array.OrderBy(skill => skill.Name, StringComparer.Ordinal).ToArray());
    }

    public IReadOnlyList<Ra2AgentSkillDescriptor> Skills => _skills;

    public IReadOnlyList<Ra2AgentSkillDescriptor> Select(
        string domainIntentId,
        Ra2AiUserMode userMode,
        string? userPrompt = null)
    {
        string domain = string.IsNullOrWhiteSpace(domainIntentId)
            ? "ini-document"
            : domainIntentId.Trim();
        Ra2AgentSkillMode requiredMode = userMode == Ra2AiUserMode.Work
            ? Ra2AgentSkillMode.Work
            : Ra2AgentSkillMode.Chat;

        List<Ra2AgentSkillDescriptor> selected = _skills
            .Where(skill => (skill.Modes & requiredMode) != 0 &&
                            skill.Domains.Contains(domain, StringComparer.Ordinal))
            .Take(1)
            .ToList();
        if (!string.Equals(domain, "ares-phobos", StringComparison.Ordinal) &&
            ContainsExtensionMarker(userPrompt))
        {
            Ra2AgentSkillDescriptor? extensionSkill = _skills.FirstOrDefault(skill =>
                (skill.Modes & requiredMode) != 0 &&
                skill.Domains.Contains("ares-phobos", StringComparer.Ordinal));
            if (extensionSkill is not null)
                selected.Add(extensionSkill);
        }
        if (!string.Equals(domain, "field-schema", StringComparison.Ordinal))
        {
            Ra2AgentSkillDescriptor? trustSkill = _skills.FirstOrDefault(skill =>
                (skill.Modes & requiredMode) != 0 &&
                skill.Domains.Contains("field-schema", StringComparer.Ordinal));
            if (trustSkill is not null && selected.All(skill => skill.Name != trustSkill.Name))
                selected.Add(trustSkill);
        }

        int characters = 0;
        return selected
            .Where(skill =>
            {
                int next = checked(characters + skill.Instructions.Length);
                if (next > MaximumSelectedSkillCharacters)
                    return false;
                characters = next;
                return true;
            })
            .ToArray();
    }

    private static bool ContainsExtensionMarker(string? prompt)
        => !string.IsNullOrWhiteSpace(prompt) &&
           (prompt.Contains("ares", StringComparison.OrdinalIgnoreCase) ||
            prompt.Contains("phobos", StringComparison.OrdinalIgnoreCase));

    public static Ra2AgentSkillCatalog LoadBundled(string? rootPath = null)
    {
        string root = rootPath ?? Path.Combine(AppContext.BaseDirectory, "AgentSkills");
        if (!Directory.Exists(root))
            return new Ra2AgentSkillCatalog([]);

        DirectoryInfo rootDirectory = new(root);
        DirectoryInfo[] directories = rootDirectory.GetDirectories()
            .OrderBy(directory => directory.Name, StringComparer.Ordinal)
            .Take(MaximumSkillCount + 1)
            .ToArray();
        if (directories.Length > MaximumSkillCount)
            throw new InvalidDataException("The built-in skill count exceeds the supported limit.");

        List<Ra2AgentSkillDescriptor> skills = [];
        foreach (DirectoryInfo directory in directories)
        {
            if (!SkillNamePattern.IsMatch(directory.Name))
                throw new InvalidDataException($"Invalid built-in skill directory '{directory.Name}'.");
            if (Directory.Exists(Path.Combine(directory.FullName, "scripts")))
                throw new InvalidDataException($"Built-in skill '{directory.Name}' contains executable scripts, which v1 forbids.");

            string path = Path.Combine(directory.FullName, "SKILL.md");
            if (!File.Exists(path))
                throw new InvalidDataException($"Built-in skill '{directory.Name}' has no SKILL.md.");
            FileInfo file = new(path);
            if (file.Length > MaximumSkillCharacters * 4L)
                throw new InvalidDataException($"Built-in skill '{directory.Name}' exceeds the byte limit.");
            string text = File.ReadAllText(path, Encoding.UTF8);
            if (text.Length > MaximumSkillCharacters || text.IndexOf('\0') >= 0)
                throw new InvalidDataException($"Built-in skill '{directory.Name}' exceeds the text limit.");

            skills.Add(Parse(directory.Name, text));
        }

        return new Ra2AgentSkillCatalog(skills);
    }

    private static Ra2AgentSkillDescriptor Parse(string directoryName, string text)
    {
        string normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
            throw new InvalidDataException($"Built-in skill '{directoryName}' has no YAML frontmatter.");
        int end = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (end < 0)
            throw new InvalidDataException($"Built-in skill '{directoryName}' has unterminated frontmatter.");

        string frontmatter = normalized[4..end];
        string body = normalized[(end + 5)..].Trim();
        Dictionary<string, string> values = new(StringComparer.Ordinal);
        bool inMetadata = false;
        foreach (string rawLine in frontmatter.Split('\n'))
        {
            if (rawLine.Length == 0)
                continue;
            if (!char.IsWhiteSpace(rawLine[0]))
                inMetadata = rawLine.Equals("metadata:", StringComparison.Ordinal);
            int separator = rawLine.IndexOf(':');
            if (separator <= 0)
                continue;
            string key = rawLine[..separator].Trim();
            string value = rawLine[(separator + 1)..].Trim().Trim('"', '\'');
            if (inMetadata && char.IsWhiteSpace(rawLine[0]))
                key = "metadata." + key;
            values[key] = value;
        }

        string name = Required(values, "name", directoryName);
        string description = Required(values, "description", directoryName);
        if (!string.Equals(name, directoryName, StringComparison.Ordinal) || !SkillNamePattern.IsMatch(name))
            throw new InvalidDataException($"Built-in skill '{directoryName}' has an invalid name.");
        if (description.Length > 1024 || body.Length == 0)
            throw new InvalidDataException($"Built-in skill '{directoryName}' has invalid content.");

        string version = values.GetValueOrDefault("metadata.version", "1");
        string[] domains = Required(values, "metadata.ra2-domains", directoryName)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (domains.Length == 0 || domains.Any(domain => !SkillNamePattern.IsMatch(domain)))
            throw new InvalidDataException($"Built-in skill '{directoryName}' has invalid RA2 domains.");
        Ra2AgentSkillMode modes = ParseModes(values.GetValueOrDefault("metadata.ra2-modes", "chat,work"), directoryName);
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized))).ToLowerInvariant();
        return new Ra2AgentSkillDescriptor(
            name,
            description,
            version,
            Array.AsReadOnly(domains),
            modes,
            body,
            hash);
    }

    private static string Required(IReadOnlyDictionary<string, string> values, string key, string directoryName)
        => values.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidDataException($"Built-in skill '{directoryName}' is missing '{key}'.");

    private static Ra2AgentSkillMode ParseModes(string value, string directoryName)
    {
        Ra2AgentSkillMode modes = Ra2AgentSkillMode.None;
        foreach (string token in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            modes |= token switch
            {
                "chat" => Ra2AgentSkillMode.Chat,
                "work" => Ra2AgentSkillMode.Work,
                _ => throw new InvalidDataException($"Built-in skill '{directoryName}' has an invalid mode.")
            };
        }
        return modes == Ra2AgentSkillMode.None
            ? throw new InvalidDataException($"Built-in skill '{directoryName}' has no mode.")
            : modes;
    }
}
