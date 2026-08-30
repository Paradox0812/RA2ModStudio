using System.IO;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.AuthoringDiff;

internal enum Ra2AuthoringReviewFailureKind
{
    None = 0,
    InvalidPreview,
    Canceled,
    ProjectionFailed
}

internal enum Ra2AuthoringReviewMode
{
    Result = 0,
    Changes,
    ObjectContext
}

internal enum Ra2AuthoringReviewOutlineKind
{
    Created = 0,
    Modified,
    Registration,
    Related,
    Unresolved,
    FileChange
}

internal enum Ra2AuthoringRelationState
{
    Available = 0,
    Partial,
    Unavailable
}

internal sealed record Ra2AuthoringReviewChangeLocation(
    Guid DocumentId,
    int CandidateOffset,
    int CandidateLength,
    int CandidateLineNumber,
    int CandidateEndLineNumber,
    int RemovedLineCount,
    string? SectionName,
    int? SectionOccurrence);

internal sealed class Ra2AuthoringReviewOutlineItem
{
    public Ra2AuthoringReviewOutlineItem(
        Guid documentId,
        string fileName,
        string sectionName,
        int occurrence,
        Ra2AuthoringReviewOutlineKind kind,
        string reason,
        int candidateOffset,
        int candidateLength,
        int candidateLineNumber,
        bool isExecutableChange,
        string? contextText = null,
        string? contextFileName = null)
    {
        DocumentId = documentId;
        FileName = fileName;
        SectionName = sectionName;
        Occurrence = occurrence;
        Kind = kind;
        Reason = reason;
        CandidateOffset = candidateOffset;
        CandidateLength = candidateLength;
        CandidateLineNumber = candidateLineNumber;
        IsExecutableChange = isExecutableChange;
        ContextText = contextText;
        ContextFileName = contextFileName;
    }

    public Guid DocumentId { get; }
    public string FileName { get; }
    public string SectionName { get; }
    public int Occurrence { get; }
    public Ra2AuthoringReviewOutlineKind Kind { get; }
    public string Reason { get; }
    public int CandidateOffset { get; }
    public int CandidateLength { get; }
    public int CandidateLineNumber { get; }
    public bool IsExecutableChange { get; }
    public string? ContextText { get; }
    public string? ContextFileName { get; }
    public string Badge => Kind switch
    {
        Ra2AuthoringReviewOutlineKind.Created => "新增",
        Ra2AuthoringReviewOutlineKind.Modified => "修改",
        Ra2AuthoringReviewOutlineKind.Registration => "注册",
        Ra2AuthoringReviewOutlineKind.Related => "关联",
        Ra2AuthoringReviewOutlineKind.Unresolved => "未解析",
        _ => "文件"
    };
    public string DisplayName => SectionName;
    public string AccessibleName => $"{FileName}，{Badge}，{SectionName}";
}

internal sealed class Ra2AuthoringReviewDocument
{
    public Ra2AuthoringReviewDocument(
        Guid documentId,
        string filePath,
        string relativePath,
        string sourceText,
        string candidateText,
        IRa2FieldDefinitionProvider fieldProvider,
        IReadOnlyList<Ra2AuthoringReviewChangeLocation> changedLocations,
        IReadOnlyList<Ra2AuthoringReviewOutlineItem> outlineItems,
        Ra2AuthoringRelationState relationState,
        string relationMessage)
    {
        DocumentId = documentId;
        FilePath = filePath;
        RelativePath = relativePath;
        SourceText = sourceText;
        CandidateText = candidateText;
        FieldProvider = fieldProvider;
        ChangedLocations = changedLocations;
        OutlineItems = outlineItems;
        RelationState = relationState;
        RelationMessage = relationMessage;
    }

    public Guid DocumentId { get; }
    public string FilePath { get; }
    public string DisplayName => Path.GetFileName(FilePath);
    public string RelativePath { get; }
    public string SourceText { get; }
    public string CandidateText { get; }
    public IRa2FieldDefinitionProvider FieldProvider { get; }
    public IReadOnlyList<Ra2AuthoringReviewChangeLocation> ChangedLocations { get; }
    public IReadOnlyList<Ra2AuthoringReviewOutlineItem> OutlineItems { get; }
    public Ra2AuthoringRelationState RelationState { get; }
    public string RelationMessage { get; }
}

internal sealed class Ra2AuthoringReviewProjection
{
    private Ra2AuthoringReviewProjection(
        Ra2AuthoringReviewFailureKind failureKind,
        string message,
        IReadOnlyList<Ra2AuthoringReviewDocument> documents,
        Ra2AuthoringDiffProjection diff)
    {
        FailureKind = failureKind;
        Message = message;
        Documents = documents;
        Diff = diff;
    }

    public bool Succeeded => FailureKind == Ra2AuthoringReviewFailureKind.None;
    public Ra2AuthoringReviewFailureKind FailureKind { get; }
    public string Message { get; }
    public IReadOnlyList<Ra2AuthoringReviewDocument> Documents { get; }
    public Ra2AuthoringDiffProjection Diff { get; }

    public static Ra2AuthoringReviewProjection Success(
        IReadOnlyList<Ra2AuthoringReviewDocument> documents,
        Ra2AuthoringDiffProjection diff)
        => new(Ra2AuthoringReviewFailureKind.None, "审阅投影已生成。", documents, diff);

    public static Ra2AuthoringReviewProjection Failure(
        Ra2AuthoringReviewFailureKind kind,
        string message,
        Ra2AuthoringDiffProjection diff)
        => new(kind, message, [], diff);
}

internal sealed class Ra2AuthoringReviewProjectionBuilder
{
    public const int MaximumRelatedItems = 64;
    public const int MaximumUnresolvedItems = 32;
    public const int MaximumContextCharacters = 1024 * 1024;

    private readonly Ra2AuthoringDiffProjectionBuilder _diffBuilder = new();
    private readonly Ra2AutomationDocumentQueryService _queryService = new();
    private readonly Ra2DocumentSemanticModelBuilder _semanticBuilder = new();

    public Ra2AuthoringReviewProjection Build(Ra2AiEditProposal proposal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2AuthoringDiffProjection diff = proposal.Scope == Ra2AiAuthoringScope.Project
                ? _diffBuilder.Build(proposal.ProjectPreview, cancellationToken)
                : _diffBuilder.Build(proposal.Preview, cancellationToken);

            IReadOnlyList<ReviewInput> inputs = proposal.Scope == Ra2AiAuthoringScope.Project
                ? BuildProjectInputs(proposal.ProjectPreview)
                : BuildDocumentInputs(proposal.Preview);
            if (inputs.Count == 0)
                return Ra2AuthoringReviewProjection.Failure(Ra2AuthoringReviewFailureKind.InvalidPreview, "当前提案没有可审阅的候选文档。", diff);

            Dictionary<Guid, CandidateDocument> candidateUniverse = BuildCandidateUniverse(proposal, inputs);
            HashSet<string> executableSections = inputs
                .SelectMany(input => input.Plan.SectionCreations.Select(item => item.SectionName)
                    .Concat(input.Plan.Operations.Select(item => item.SectionName))
                    .Select(sectionName => DocumentSectionIdentity(input.Source.DocumentId, sectionName)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            List<Ra2AuthoringReviewDocument> documents = [];
            int relatedBudget = MaximumRelatedItems;
            int unresolvedBudget = MaximumUnresolvedItems;
            int contextBudget = MaximumContextCharacters;
            foreach (ReviewInput input in inputs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Ra2AuthoringReviewDocument document = BuildDocument(
                    input,
                    candidateUniverse,
                    executableSections,
                    ref relatedBudget,
                    ref unresolvedBudget,
                    ref contextBudget,
                    cancellationToken);
                documents.Add(document);
            }

            return Ra2AuthoringReviewProjection.Success(Array.AsReadOnly(documents.ToArray()), diff);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Ra2AuthoringReviewProjection.Failure(
                Ra2AuthoringReviewFailureKind.Canceled,
                "审阅投影已取消。",
                Ra2AuthoringDiffProjection.Failure(Ra2AuthoringDiffFailureKind.Canceled, "差异预览已取消。"));
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        {
            return Ra2AuthoringReviewProjection.Failure(
                Ra2AuthoringReviewFailureKind.ProjectionFailed,
                "无法生成完整审阅投影。",
                Ra2AuthoringDiffProjection.Failure(Ra2AuthoringDiffFailureKind.InvalidPreview, "差异预览不可用。"));
        }
    }

    private Ra2AuthoringReviewDocument BuildDocument(
        ReviewInput input,
        IReadOnlyDictionary<Guid, CandidateDocument> candidateUniverse,
        IReadOnlySet<string> executableSections,
        ref int relatedBudget,
        ref int unresolvedBudget,
        ref int contextBudget,
        CancellationToken cancellationToken)
    {
        if (!Ra2AuthoringDiffProjectionBuilder.TryMapChanges(
                input.Source.Text,
                input.CandidateText,
                input.Changes,
                cancellationToken,
                out IReadOnlyList<Ra2AuthoringMappedChange> mappedChanges,
                out Ra2AuthoringDiffProjection? failure))
        {
            throw new InvalidOperationException(failure?.Message ?? "Change mapping failed.");
        }

        Ra2DocumentSemanticModel model = _semanticBuilder.Build(
            new Ra2DocumentSnapshot(input.Source.FilePath, input.CandidateText, input.Source.Version),
            input.Source.FieldRegistry.Provider);
        List<Ra2AuthoringReviewChangeLocation> locations = BuildLocations(input.Source.DocumentId, mappedChanges, model);
        List<Ra2AuthoringReviewOutlineItem> outline = BuildChangedOutline(input, mappedChanges, model);
        Ra2AuthoringRelationState relationState = AddRelations(
            input,
            model,
            outline,
            candidateUniverse,
            executableSections,
            ref relatedBudget,
            ref unresolvedBudget,
            ref contextBudget,
            cancellationToken,
            out string relationMessage);

        string relativePath = input.ProjectRootPath is null
            ? Path.GetFileName(input.Source.FilePath)
            : Path.GetRelativePath(input.ProjectRootPath, input.Source.FilePath);
        return new Ra2AuthoringReviewDocument(
            input.Source.DocumentId,
            input.Source.FilePath,
            relativePath,
            input.Source.Text,
            input.CandidateText,
            input.Source.FieldRegistry.Provider,
            Array.AsReadOnly(locations.ToArray()),
            Array.AsReadOnly(outline.ToArray()),
            relationState,
            relationMessage);
    }

    private static List<Ra2AuthoringReviewChangeLocation> BuildLocations(
        Guid documentId,
        IReadOnlyList<Ra2AuthoringMappedChange> mappedChanges,
        Ra2DocumentSemanticModel model)
    {
        List<Ra2AuthoringReviewChangeLocation> locations = [];
        foreach (Ra2AuthoringMappedChange change in mappedChanges)
        {
            Ra2SectionSymbol? section = FindSectionAtAnchor(model, change.CandidateSpan.Start);
            locations.Add(new Ra2AuthoringReviewChangeLocation(
                documentId,
                change.CandidateSpan.Start,
                change.CandidateSpan.Length,
                change.CandidateStartLine + 1,
                Math.Max(change.CandidateStartLine + 1, change.CandidateEndLine),
                change.RemovedLineCount,
                section?.Name,
                section is null ? null : GetOccurrence(model, section)));
        }
        return locations;
    }

    private static List<Ra2AuthoringReviewOutlineItem> BuildChangedOutline(
        ReviewInput input,
        IReadOnlyList<Ra2AuthoringMappedChange> mappedChanges,
        Ra2DocumentSemanticModel model)
    {
        HashSet<string> createdNames = input.Plan.SectionCreations
            .Select(item => item.SectionName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        HashSet<string> targetNames = input.Plan.Operations
            .Select(item => item.SectionName)
            .Concat(createdNames)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        List<Ra2AuthoringReviewOutlineItem> items = [];
        HashSet<(string Name, int Occurrence)> added = new(new SectionIdentityComparer());

        foreach (Ra2SectionSymbol section in model.Sections)
        {
            if (!targetNames.Contains(section.Name) || !mappedChanges.Any(change => Touches(section, change.CandidateSpan)))
                continue;
            int occurrence = GetOccurrence(model, section);
            if (!added.Add((section.Name, occurrence)))
                continue;
            items.Add(CreateChangedItem(input, section, occurrence, createdNames.Contains(section.Name)));
        }

        foreach (string targetName in targetNames)
        {
            Ra2SectionSymbol[] matches = model.Sections.Where(section => string.Equals(section.Name, targetName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length == 0)
                continue;
            Ra2SectionSymbol section = matches[0];
            int occurrence = GetOccurrence(model, section);
            if (added.Add((section.Name, occurrence)))
                items.Add(CreateChangedItem(input, section, occurrence, createdNames.Contains(section.Name)));
        }

        foreach (Ra2AuthoringMappedChange change in mappedChanges)
        {
            if (FindSectionAtAnchor(model, change.CandidateSpan.Start) is not null)
                continue;
            items.Add(new Ra2AuthoringReviewOutlineItem(
                input.Source.DocumentId,
                Path.GetFileName(input.Source.FilePath),
                $"文件级变更 · 行 {change.CandidateStartLine + 1}",
                0,
                Ra2AuthoringReviewOutlineKind.FileChange,
                "该变更不位于可识别的 Section 内。",
                change.CandidateSpan.Start,
                change.CandidateSpan.Length,
                change.CandidateStartLine + 1,
                true));
        }

        return items
            .OrderBy(item => item.CandidateOffset)
            .ThenBy(item => item.SectionName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Ra2AuthoringReviewOutlineItem CreateChangedItem(
        ReviewInput input,
        Ra2SectionSymbol section,
        int occurrence,
        bool created)
        => new(
            input.Source.DocumentId,
            Path.GetFileName(input.Source.FilePath),
            section.Name,
            occurrence,
            created ? Ra2AuthoringReviewOutlineKind.Created : Ra2AuthoringReviewOutlineKind.Modified,
            created ? "本次提案新增 Section。" : "本次提案修改 Section。",
            section.HeaderSpan.Start,
            section.BodySpan.End - section.HeaderSpan.Start,
            section.HeaderLineNumber,
            true);

    private Ra2AuthoringRelationState AddRelations(
        ReviewInput input,
        Ra2DocumentSemanticModel model,
        List<Ra2AuthoringReviewOutlineItem> outline,
        IReadOnlyDictionary<Guid, CandidateDocument> candidateUniverse,
        IReadOnlySet<string> executableSections,
        ref int relatedBudget,
        ref int unresolvedBudget,
        ref int contextBudget,
        CancellationToken cancellationToken,
        out string message)
    {
        if (outline.Count == 0)
        {
            message = "未能建立可靠的直接引用上下文。";
            return Ra2AuthoringRelationState.Unavailable;
        }

        CandidateDocument sourceCandidate = candidateUniverse[input.Source.DocumentId];
        HashSet<string> changedIdentities = outline.Where(item => item.IsExecutableChange)
            .Select(item => Identity(item.DocumentId, item.SectionName, item.Occurrence))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        HashSet<string> relatedIdentities = new(StringComparer.OrdinalIgnoreCase);
        bool partial = false;

        foreach (Ra2AuthoringReviewOutlineItem changed in outline.Where(item => item.IsExecutableChange).ToArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2AutomationSectionQueryResult sectionResult = _queryService.GetSection(
                sourceCandidate.Snapshot,
                new Ra2AutomationSectionQuery(changed.SectionName, changed.Occurrence),
                cancellationToken);
            if (!sectionResult.Succeeded || sectionResult.Section is null)
            {
                partial = true;
                continue;
            }

            Ra2AutomationSectionFact section = sectionResult.Section;
            foreach (IGrouping<string, Ra2AutomationFieldFact> fieldGroup in section.Fields.GroupBy(field => field.Key, StringComparer.OrdinalIgnoreCase))
            {
                int fieldOccurrence = 0;
                foreach (Ra2AutomationFieldFact field in fieldGroup)
                {
                    bool semanticReference = model.References.Any(reference =>
                        reference.LineNumber == field.LineNumber &&
                        string.Equals(reference.SourceSectionName, section.Name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(reference.SourceKey, field.Key, StringComparison.OrdinalIgnoreCase));
                    bool schemaReference = input.Source.FieldRegistry.Provider.TryGetField(section.Kind, field.Key, out Ra2FieldDefinition definition) &&
                        definition.ValueMetadata.ValueKind is Ra2FieldValueKind.Reference or Ra2FieldValueKind.ReferenceList;
                    if (!semanticReference && !schemaReference)
                    {
                        fieldOccurrence++;
                        continue;
                    }

                    for (int referenceIndex = 0; referenceIndex <= MaximumRelatedItems; referenceIndex++)
                    {
                        if (relatedBudget <= 0)
                        {
                            Ra2AutomationReferenceResolveResult probe = _queryService.ResolveReference(
                                sourceCandidate.Snapshot,
                                new Ra2AutomationReferenceResolveQuery(section.Name, field.Key, section.Occurrence, fieldOccurrence, referenceIndex),
                                cancellationToken);
                            if (probe.FailureKind != Ra2AutomationReferenceResolveFailureKind.ReferenceIndexOutOfRange)
                                partial = true;
                            break;
                        }
                        Ra2AutomationReferenceResolveResult resolved = _queryService.ResolveReference(
                            sourceCandidate.Snapshot,
                            new Ra2AutomationReferenceResolveQuery(section.Name, field.Key, section.Occurrence, fieldOccurrence, referenceIndex),
                            cancellationToken);
                        if (resolved.FailureKind == Ra2AutomationReferenceResolveFailureKind.ReferenceIndexOutOfRange)
                            break;
                        if (!resolved.Succeeded || resolved.Fact is null)
                        {
                            if (resolved.FailureKind is not Ra2AutomationReferenceResolveFailureKind.EmptyReference and
                                not Ra2AutomationReferenceResolveFailureKind.UnsupportedReference)
                                partial = true;
                            break;
                        }

                        relatedBudget--;
                        AddResolvedRelation(
                            resolved.Fact,
                            sourceCandidate,
                            candidateUniverse,
                            executableSections,
                            changedIdentities,
                            relatedIdentities,
                            outline,
                            ref unresolvedBudget,
                            ref contextBudget,
                            cancellationToken,
                            ref partial);
                    }
                    fieldOccurrence++;
                }
            }
        }

        int relatedCount = outline.Count(item => item.Kind == Ra2AuthoringReviewOutlineKind.Related);
        int unresolvedCount = outline.Count(item => item.Kind == Ra2AuthoringReviewOutlineKind.Unresolved);
        if (relatedCount == 0 && unresolvedCount == 0)
        {
            message = "未发现可可靠解析的直接引用 Section。";
            return partial ? Ra2AuthoringRelationState.Partial : Ra2AuthoringRelationState.Unavailable;
        }

        message = partial
            ? $"已显示 {relatedCount} 个直接关联；部分关系因边界或歧义未展开。"
            : $"已显示 {relatedCount} 个直接关联 Section。";
        return partial ? Ra2AuthoringRelationState.Partial : Ra2AuthoringRelationState.Available;
    }

    private void AddResolvedRelation(
        Ra2AutomationReferenceResolutionFact fact,
        CandidateDocument sourceCandidate,
        IReadOnlyDictionary<Guid, CandidateDocument> candidateUniverse,
        IReadOnlySet<string> executableSections,
        HashSet<string> changedIdentities,
        HashSet<string> relatedIdentities,
        List<Ra2AuthoringReviewOutlineItem> outline,
        ref int unresolvedBudget,
        ref int contextBudget,
        CancellationToken cancellationToken,
        ref bool partial)
    {
        List<(CandidateDocument Document, Ra2AutomationSectionFact Section)> matches = [];
        IEnumerable<CandidateDocument> candidates = fact.IsTargetDefined
            ? [sourceCandidate]
            : candidateUniverse.Values;
        foreach (CandidateDocument candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2AutomationSectionQueryResult query = _queryService.GetSection(
                candidate.Snapshot,
                new Ra2AutomationSectionQuery(fact.TargetSectionName),
                cancellationToken);
            if (query.Succeeded && query.Section is not null)
                matches.Add((candidate, query.Section));
            else if (query.FailureKind == Ra2AutomationSectionQueryFailureKind.AmbiguousSection)
            {
                matches.Add((candidate, null!));
                matches.Add((candidate, null!));
            }
        }

        if (matches.Count != 1 || matches[0].Section is null)
        {
            partial = true;
            if (unresolvedBudget-- <= 0)
                return;
            string unresolvedIdentity = $"unresolved:{fact.TargetSectionName}";
            if (!relatedIdentities.Add(unresolvedIdentity))
                return;
            outline.Add(new Ra2AuthoringReviewOutlineItem(
                sourceCandidate.Snapshot.DocumentId,
                Path.GetFileName(sourceCandidate.Snapshot.FilePath),
                fact.TargetSectionName,
                0,
                Ra2AuthoringReviewOutlineKind.Unresolved,
                matches.Count == 0 ? "未在当前提案快照中找到目标 Section。" : "目标 Section 在提案快照中不唯一。",
                0,
                0,
                1,
                false));
            return;
        }

        (CandidateDocument targetDocument, Ra2AutomationSectionFact target) = matches[0];
        if (executableSections.Contains(DocumentSectionIdentity(targetDocument.Snapshot.DocumentId, target.Name)))
            return;
        string identity = Identity(targetDocument.Snapshot.DocumentId, target.Name, target.Occurrence);
        if (changedIdentities.Contains(identity) || !relatedIdentities.Add(identity))
            return;

        int length = target.FullSpan.Length;
        string? contextText = null;
        if (length <= contextBudget)
        {
            contextText = targetDocument.Snapshot.Text.Substring(target.FullSpan.Start, length);
            contextBudget -= length;
        }
        else
        {
            partial = true;
        }

        outline.Add(new Ra2AuthoringReviewOutlineItem(
            targetDocument.Snapshot.DocumentId,
            Path.GetFileName(targetDocument.Snapshot.FilePath),
            target.Name,
            target.Occurrence,
            Ra2AuthoringReviewOutlineKind.Related,
            contextText is null ? "直接引用；内容已省略。" : "直接引用；未修改，仅供审阅。",
            target.FullSpan.Start,
            target.FullSpan.Length,
            target.HeaderLineNumber,
            false,
            contextText,
            Path.GetFileName(targetDocument.Snapshot.FilePath)));
    }

    private static Dictionary<Guid, CandidateDocument> BuildCandidateUniverse(
        Ra2AiEditProposal proposal,
        IReadOnlyList<ReviewInput> inputs)
    {
        Dictionary<Guid, CandidateDocument> result = [];
        if (proposal.Scope == Ra2AiAuthoringScope.Document)
        {
            ReviewInput input = inputs[0];
            result[input.Source.DocumentId] = CandidateDocument.From(input.Source, input.CandidateText);
            return result;
        }

        Dictionary<Guid, ReviewInput> changed = inputs.ToDictionary(input => input.Source.DocumentId);
        foreach (Ra2AutomationDocumentSnapshot source in proposal.ProjectPreview.Snapshot.Documents)
        {
            string text = changed.TryGetValue(source.DocumentId, out ReviewInput? input) ? input.CandidateText : source.Text;
            result[source.DocumentId] = CandidateDocument.From(source, text);
        }
        return result;
    }

    private static IReadOnlyList<ReviewInput> BuildDocumentInputs(Ra2IniEditPreview preview)
        => preview.Succeeded && preview.CandidateText is not null
            ? [new ReviewInput(preview.Snapshot.ToAutomationSnapshot(), preview.CandidateText, preview.AutomationResult.Changes, preview.Plan, null)]
            : [];

    private static IReadOnlyList<ReviewInput> BuildProjectInputs(Ra2ProjectEditPreview preview)
    {
        if (!preview.Succeeded)
            return [];
        List<ReviewInput> inputs = [];
        foreach (Ra2AutomationEditPreviewResult documentPreview in preview.DocumentPreviews)
        {
            Ra2AutomationDocumentSnapshot source = preview.Snapshot.Documents.Single(document => document.DocumentId == documentPreview.DocumentId);
            Ra2AutomationEditPlan plan = preview.Plan.DocumentPlans.Single(document => document.ExpectedDocumentId == source.DocumentId);
            if (documentPreview.CandidateText is null)
                continue;
            inputs.Add(new ReviewInput(source, documentPreview.CandidateText, documentPreview.Changes, plan, preview.Snapshot.ProjectRootPath));
        }
        return Array.AsReadOnly(inputs.ToArray());
    }

    private static Ra2SectionSymbol? FindSectionAtAnchor(Ra2DocumentSemanticModel model, int offset)
    {
        Ra2SectionSymbol? direct = model.FindSectionAtOffset(offset);
        if (direct is not null)
            return direct;
        if (offset > 0)
            return model.FindSectionAtOffset(offset - 1);
        return null;
    }

    private static int GetOccurrence(Ra2DocumentSemanticModel model, Ra2SectionSymbol section)
        => model.Sections.Where(item => string.Equals(item.Name, section.Name, StringComparison.OrdinalIgnoreCase))
            .TakeWhile(item => !ReferenceEquals(item, section)).Count();

    private static bool Touches(Ra2SectionSymbol section, Ra2AutomationTextSpan change)
    {
        int sectionStart = section.HeaderSpan.Start;
        int sectionEnd = section.BodySpan.End;
        return change.Length == 0
            ? change.Start >= sectionStart && change.Start <= sectionEnd
            : change.Start < sectionEnd && change.End > sectionStart;
    }

    private static string Identity(Guid documentId, string sectionName, int occurrence)
        => $"{documentId:N}:{sectionName}:{occurrence}";

    private static string DocumentSectionIdentity(Guid documentId, string sectionName)
        => $"{documentId:N}:{sectionName}";

    private sealed record ReviewInput(
        Ra2AutomationDocumentSnapshot Source,
        string CandidateText,
        IReadOnlyList<Ra2AutomationTextChange> Changes,
        Ra2AutomationEditPlan Plan,
        string? ProjectRootPath);

    private sealed record CandidateDocument(Ra2AutomationDocumentSnapshot Snapshot)
    {
        public static CandidateDocument From(Ra2AutomationDocumentSnapshot source, string text)
            => new(new Ra2AutomationDocumentSnapshot(
                source.DocumentId,
                source.Version,
                source.FilePath,
                text,
                source.IsEditable,
                source.FieldRegistry));
    }

    private sealed class SectionIdentityComparer : IEqualityComparer<(string Name, int Occurrence)>
    {
        public bool Equals((string Name, int Occurrence) x, (string Name, int Occurrence) y)
            => x.Occurrence == y.Occurrence && string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string Name, int Occurrence) obj)
            => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name), obj.Occurrence);
    }
}
