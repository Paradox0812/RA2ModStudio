using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels.FieldDetails;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.Language.FieldQuickPeek;

internal enum Ra2FieldQuickPeekStatus
{
    Available,
    NotKeyValueLine,
    NotFound
}

internal sealed class Ra2FieldQuickPeekRequest
{
    public Ra2FieldQuickPeekRequest(
        Ra2DocumentSemanticModel model,
        int offset,
        IRa2FieldDefinitionProvider fieldProvider,
        IFieldRegistryProvenanceProvider provenanceProvider)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Offset = offset;
        FieldProvider = fieldProvider ?? throw new ArgumentNullException(nameof(fieldProvider));
        ProvenanceProvider = provenanceProvider ?? throw new ArgumentNullException(nameof(provenanceProvider));
    }

    public Ra2DocumentSemanticModel Model { get; }

    public int Offset { get; }

    public IRa2FieldDefinitionProvider FieldProvider { get; }

    public IFieldRegistryProvenanceProvider ProvenanceProvider { get; }
}

internal sealed class Ra2FieldQuickPeekResult
{
    private Ra2FieldQuickPeekResult(
        Ra2FieldQuickPeekStatus status,
        string key,
        Ra2SectionKind sectionKind,
        Ra2FieldDetailsViewModel details)
    {
        Status = status;
        Key = key;
        SectionKind = sectionKind;
        Details = details;
    }

    public Ra2FieldQuickPeekStatus Status { get; }

    public string Key { get; }

    public Ra2SectionKind SectionKind { get; }

    public Ra2FieldDetailsViewModel Details { get; }

    public static Ra2FieldQuickPeekResult NotKeyValueLine()
        => new(Ra2FieldQuickPeekStatus.NotKeyValueLine, string.Empty, Ra2SectionKind.Unknown, Ra2FieldDetailsViewModel.NotFound(string.Empty, Ra2SectionKind.Unknown));

    public static Ra2FieldQuickPeekResult NotFound(string key, Ra2SectionKind sectionKind)
        => new(Ra2FieldQuickPeekStatus.NotFound, key, sectionKind, Ra2FieldDetailsViewModel.NotFound(key, sectionKind));

    public static Ra2FieldQuickPeekResult Available(
        string key,
        Ra2SectionKind sectionKind,
        Ra2FieldDetailsViewModel details)
        => new(Ra2FieldQuickPeekStatus.Available, key, sectionKind, details);
}

internal sealed class Ra2FieldQuickPeekService
{
    private readonly Ra2CaretContextService _caretContextService = new();

    public Ra2FieldQuickPeekResult Resolve(Ra2FieldQuickPeekRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        int offset = Math.Clamp(request.Offset, 0, request.Model.Snapshot.Text.Length);
        Ra2CaretContext context = _caretContextService.GetContext(request.Model, offset);
        if (context.KeyValue is null)
            return Ra2FieldQuickPeekResult.NotKeyValueLine();

        FieldRegistryProvenanceLookupResult provenance = request.ProvenanceProvider.TryGetFieldWithProvenance(
            context.KeyValue.SectionKind,
            context.KeyValue.Key);
        if (provenance.Found && provenance.Definition is not null)
        {
            return Ra2FieldQuickPeekResult.Available(
                context.KeyValue.Key,
                context.KeyValue.SectionKind,
                Ra2FieldDetailsViewModel.FromProvenance(provenance, context.KeyValue.SectionKind));
        }

        if (request.FieldProvider.TryGetField(context.KeyValue.SectionKind, context.KeyValue.Key, out Ra2FieldDefinition definition))
        {
            return Ra2FieldQuickPeekResult.Available(
                context.KeyValue.Key,
                context.KeyValue.SectionKind,
                Ra2FieldDetailsViewModel.FromDefinition(definition, context.KeyValue.SectionKind));
        }

        return Ra2FieldQuickPeekResult.NotFound(context.KeyValue.Key, context.KeyValue.SectionKind);
    }

    public bool CanResolveKeyValueLine(Ra2DocumentSemanticModel model, int offset)
    {
        ArgumentNullException.ThrowIfNull(model);
        Ra2CaretContext context = _caretContextService.GetContext(model, Math.Clamp(offset, 0, model.Snapshot.Text.Length));
        return context.KeyValue is not null;
    }
}
