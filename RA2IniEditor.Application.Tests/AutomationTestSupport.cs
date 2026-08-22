using System.Text;
using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Tests;

internal static class AutomationTestSupport
{
    public static Ra2AutomationDocumentSnapshot Snapshot(
        string text,
        IRa2FieldDefinitionProvider? provider = null,
        string filePath = "rulesmd.ini",
        int version = 1)
        => new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            version,
            filePath,
            text,
            isEditable: false,
            new Ra2AutomationFieldRegistrySnapshot(provider ?? new EmptyFieldDefinitionProvider(), 7));

    public static string Slice(string text, Ra2AutomationTextSpan span)
        => text.Substring(span.Start, span.Length);

    public static string BuildLargeDocument(int approximateMegabytes)
    {
        int targetLength = approximateMegabytes * 1024 * 1024;
        StringBuilder builder = new(targetLength + 128);
        builder.AppendLine("[InfantryTypes]");
        builder.AppendLine("0=E1");
        builder.AppendLine("[E1]");
        builder.AppendLine("Primary=SharedWeapon");
        builder.AppendLine("[SharedWeapon]");
        builder.AppendLine("Damage=90");
        const string filler = "; deterministic automation query characterization 0123456789\n";
        while (builder.Length < targetLength)
            builder.Append(filler);

        return builder.ToString();
    }

    public static string BuildManyFields(int count)
    {
        StringBuilder builder = new(count * 18 + 32);
        builder.AppendLine("[ManyFields]");
        for (int index = 0; index < count; index++)
            builder.Append("Key").Append(index).Append("=Value").Append(index).AppendLine();

        return builder.ToString();
    }

    public static string BuildManyReferences(int count)
    {
        StringBuilder builder = new(count * 24 + 64);
        builder.AppendLine("[InfantryTypes]");
        builder.AppendLine("0=E1");
        builder.AppendLine("[E1]");
        for (int index = 0; index < count; index++)
            builder.Append("Primary=SharedWeapon").AppendLine();

        return builder.ToString();
    }

    internal sealed class EmptyFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => [];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => false;
    }

    internal sealed class CancelingFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        private readonly CancellationTokenSource _source;

        public CancelingFieldDefinitionProvider(CancellationTokenSource source)
        {
            _source = source;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            _source.Cancel();
            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => [];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
        {
            _source.Cancel();
            return false;
        }
    }

    internal sealed class ThrowingFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        private readonly Exception _exception;

        public ThrowingFieldDefinitionProvider(Exception exception)
        {
            _exception = exception;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
            => throw _exception;

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => throw _exception;

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => throw _exception;
    }
}
