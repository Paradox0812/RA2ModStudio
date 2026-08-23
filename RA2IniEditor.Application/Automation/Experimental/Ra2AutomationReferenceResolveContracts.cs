using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation.Experimental;

public enum Ra2AutomationReferenceResolveFailureKind
{
    None = 0,
    DocumentTooLarge,
    SectionNotFound,
    AmbiguousSection,
    FieldNotFound,
    AmbiguousField,
    UnsupportedReference,
    EmptyReference,
    ReferenceIndexOutOfRange,
    ResultLimitExceeded,
    Canceled,
    AnalysisFailed
}

public enum Ra2AutomationReferenceResolutionBasis
{
    SemanticKnown = 0,
    FieldSchemaDeclared
}

public sealed class Ra2AutomationReferenceResolveQuery
{
    public Ra2AutomationReferenceResolveQuery(
        string sectionName,
        string key,
        int? sectionOccurrence = null,
        int? fieldOccurrence = null,
        int referenceIndex = 0)
    {
        SectionName = ValidateIdentifier(sectionName, nameof(sectionName), allowBrackets: false);
        Key = ValidateIdentifier(key, nameof(key), allowBrackets: true);
        if (sectionOccurrence < 0)
            throw new ArgumentOutOfRangeException(nameof(sectionOccurrence));
        if (fieldOccurrence < 0)
            throw new ArgumentOutOfRangeException(nameof(fieldOccurrence));
        if (referenceIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(referenceIndex));

        SectionOccurrence = sectionOccurrence;
        FieldOccurrence = fieldOccurrence;
        ReferenceIndex = referenceIndex;
    }

    public string SectionName { get; }
    public string Key { get; }
    public int? SectionOccurrence { get; }
    public int? FieldOccurrence { get; }
    public int ReferenceIndex { get; }

    private static string ValidateIdentifier(string value, string parameterName, bool allowBrackets)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A reference source identifier is required.", parameterName);
        string normalized = value.Trim();
        if (normalized.Length > 256 || normalized.IndexOfAny(['\r', '\n', '\0', '=']) >= 0 ||
            (!allowBrackets && normalized.IndexOfAny(['[', ']']) >= 0))
        {
            throw new ArgumentException("The reference source identifier is too long or contains unsupported characters.", parameterName);
        }

        return normalized;
    }
}

public sealed class Ra2AutomationReferenceResolutionFact
{
    internal Ra2AutomationReferenceResolutionFact(
        string sourceSectionName,
        int sourceSectionOccurrence,
        string sourceKey,
        int sourceFieldOccurrence,
        int sourceLineNumber,
        Ra2AutomationTextSpan sourceSpan,
        string rawEffectiveToken,
        int referenceIndex,
        string targetSectionName,
        Ra2SectionKind targetSectionKind,
        Ra2AutomationReferenceResolutionBasis basis,
        bool isTargetDefined,
        int targetDefinitionCount,
        bool isSchemaDeclaredReference)
    {
        SourceSectionName = sourceSectionName;
        SourceSectionOccurrence = sourceSectionOccurrence;
        SourceKey = sourceKey;
        SourceFieldOccurrence = sourceFieldOccurrence;
        SourceLineNumber = sourceLineNumber;
        SourceSpan = sourceSpan;
        RawEffectiveToken = rawEffectiveToken;
        ReferenceIndex = referenceIndex;
        TargetSectionName = targetSectionName;
        TargetSectionKind = targetSectionKind;
        Basis = basis;
        IsTargetDefined = isTargetDefined;
        TargetDefinitionCount = targetDefinitionCount;
        IsSchemaDeclaredReference = isSchemaDeclaredReference;
    }

    public string SourceSectionName { get; }
    public int SourceSectionOccurrence { get; }
    public string SourceKey { get; }
    public int SourceFieldOccurrence { get; }
    public int SourceLineNumber { get; }
    public Ra2AutomationTextSpan SourceSpan { get; }
    public string RawEffectiveToken { get; }
    public int ReferenceIndex { get; }
    public string TargetSectionName { get; }
    public Ra2SectionKind TargetSectionKind { get; }
    public Ra2AutomationReferenceResolutionBasis Basis { get; }
    public bool IsTargetDefined { get; }
    public int TargetDefinitionCount { get; }
    public bool IsSchemaDeclaredReference { get; }
}

public sealed class Ra2AutomationReferenceResolveResult
{
    internal Ra2AutomationReferenceResolveResult(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceResolveFailureKind failureKind,
        string message,
        Ra2AutomationReferenceResolutionFact? fact)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!Enum.IsDefined(failureKind))
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A result message is required.", nameof(message));
        if ((failureKind == Ra2AutomationReferenceResolveFailureKind.None) != (fact is not null))
            throw new ArgumentException("Reference resolution result state is inconsistent.", nameof(fact));

        Succeeded = failureKind == Ra2AutomationReferenceResolveFailureKind.None;
        FailureKind = failureKind;
        Message = message;
        DocumentId = snapshot.DocumentId;
        Version = snapshot.Version;
        FilePath = snapshot.FilePath;
        FieldRegistryRevision = snapshot.FieldRegistry.Revision;
        Fact = fact;
    }

    public bool Succeeded { get; }
    public Ra2AutomationReferenceResolveFailureKind FailureKind { get; }
    public string Message { get; }
    public Guid DocumentId { get; }
    public int Version { get; }
    public string FilePath { get; }
    public long FieldRegistryRevision { get; }
    public Ra2AutomationReferenceResolutionFact? Fact { get; }
}
