namespace RA2IniEditor.Application.Automation.Experimental;

public static class Ra2AutomationCapabilityIds
{
    public const int CurrentVersion = 1;
    public const string DocumentSectionGet = "ini.document.section.get";
    public const string DocumentReferencesFind = "ini.document.references.find";
    public const string DocumentDiagnosticsValidate = "ini.document.diagnostics.validate";
    public const string DocumentEditPreview = "ini.document.edit.preview";
    public const string DocumentFieldSchemaGet = "ini.document.field-schema.get";
    public const string DocumentReferenceResolve = "ini.document.reference.resolve";
    public const string ContentTemplateExpand = "ini.content.template.expand";
}

public enum Ra2AutomationCapabilityRisk
{
    Query = 0,
    Edit = 1
}

public enum Ra2AutomationCapabilityStability
{
    Experimental = 0
}

public sealed class Ra2AutomationCapabilityDescriptor
{
    internal Ra2AutomationCapabilityDescriptor(
        string id,
        int version,
        Ra2AutomationCapabilityRisk risk,
        Ra2AutomationCapabilityStability stability,
        int maximumDocumentCharacters,
        int? maximumResultItems,
        int? maximumOperations)
    {
        Id = id;
        Version = version;
        Risk = risk;
        Stability = stability;
        MaximumDocumentCharacters = maximumDocumentCharacters;
        MaximumResultItems = maximumResultItems;
        MaximumOperations = maximumOperations;
    }

    public string Id { get; }
    public int Version { get; }
    public Ra2AutomationCapabilityRisk Risk { get; }
    public Ra2AutomationCapabilityStability Stability { get; }
    public int MaximumDocumentCharacters { get; }
    public int? MaximumResultItems { get; }
    public int? MaximumOperations { get; }
}
