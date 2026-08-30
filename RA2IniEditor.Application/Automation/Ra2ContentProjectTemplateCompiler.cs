using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation;

internal sealed class Ra2ContentProjectTemplateCompiler
{
    private readonly Ra2ContentTemplateCompiler _documentCompiler;

    public Ra2ContentProjectTemplateCompiler(Ra2ContentTemplateCompiler documentCompiler)
    {
        _documentCompiler = documentCompiler ?? throw new ArgumentNullException(nameof(documentCompiler));
    }

    public Ra2AutomationProjectTemplateExpansionResult CompileTechnoRulesArtBinding(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationTemplateExpansionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            (Ra2AutomationDocumentSnapshot? rules, Ra2AutomationDocumentSnapshot? art, Ra2AutomationTemplateExpansionFailureKind pairFailure) =
                ResolveRulesArtPair(snapshot);
            if (pairFailure != Ra2AutomationTemplateExpansionFailureKind.None)
            {
                return Failure(
                    snapshot,
                    pairFailure,
                    pairFailure == Ra2AutomationTemplateExpansionFailureKind.ProjectDocumentAmbiguous
                        ? "The project contains an ambiguous rules/art document pair."
                        : "The project does not contain one complete rules/art document pair.");
            }

            KeyValuePair<string, string>[] arguments = request.Arguments
                .Select(argument => new KeyValuePair<string, string>(argument.Name, argument.Value))
                .ToArray();
            Ra2ContentTemplateCompilationResult rulesCompilation = _documentCompiler.Compile(
                CreateRulesDefinition(request.TemplateId, request.TemplateVersion),
                arguments,
                rules!,
                cancellationToken);
            if (!rulesCompilation.Succeeded)
                return Failure(snapshot, MapFailure(rulesCompilation.FailureKind), rulesCompilation.Message);

            cancellationToken.ThrowIfCancellationRequested();
            Ra2ContentTemplateCompilationResult artCompilation = _documentCompiler.Compile(
                CreateArtDefinition(request.TemplateId, request.TemplateVersion),
                arguments,
                art!,
                cancellationToken);
            if (!artCompilation.Succeeded)
                return Failure(snapshot, MapFailure(artCompilation.FailureKind), artCompilation.Message);

            Dictionary<string, string> bound = request.Arguments.ToDictionary(argument => argument.Name, argument => argument.Value, StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(bound["assetBrief"]) ||
                bound["assetBrief"].Trim().Length > Ra2AutomationAssetRequirement.MaximumBriefLength)
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.InvalidArguments, "assetBrief cannot be empty.");
            if (!IsValidAssetStem(bound["bodyAssetId"]) || !IsValidAssetStem(bound["cameoAssetId"]) ||
                string.Equals(bound["bodyAssetId"], bound["cameoAssetId"], StringComparison.OrdinalIgnoreCase))
            {
                return Failure(
                    snapshot,
                    Ra2AutomationTemplateExpansionFailureKind.InvalidArguments,
                    "bodyAssetId and cameoAssetId must be distinct Windows-safe file name stems.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            Ra2AutomationProjectEditPlan projectPlan = new(
                Guid.NewGuid(),
                snapshot.ProjectSessionId,
                snapshot.ProjectRevision,
                [rulesCompilation.Plan!, artCompilation.Plan!],
                $"Bind {bound["ownerSectionId"]} to art and asset requirements",
                $"content-template:{request.TemplateId}@{request.TemplateVersion}");

            Ra2AutomationAssetBindingFact bodyBinding = new(
                art!.DocumentId,
                art.FilePath,
                bound["artSectionId"],
                "Image",
                bound["bodyAssetId"],
                Ra2AutomationAssetBindingState.Proposed);
            Ra2AutomationAssetBindingFact cameoBinding = new(
                art.DocumentId,
                art.FilePath,
                bound["artSectionId"],
                "Cameo",
                bound["cameoAssetId"],
                Ra2AutomationAssetBindingState.Proposed);
            Ra2AutomationAssetManifest manifest = new(
                snapshot.ProjectSessionId,
                request.TemplateId,
                request.TemplateVersion,
                [
                    new Ra2AutomationAssetRequirement(
                        "techno-body-shp",
                        $"{bound["bodyAssetId"]}.shp",
                        Ra2AutomationAssetKind.ShpAnimation,
                        bound["assetBrief"],
                        width: null,
                        height: null,
                        palette: null,
                        [bodyBinding]),
                    new Ra2AutomationAssetRequirement(
                        "techno-cameo",
                        $"{bound["cameoAssetId"]}.shp",
                        Ra2AutomationAssetKind.Cameo,
                        $"Create a 60x48 cameo for: {bound["assetBrief"]}",
                        width: 60,
                        height: 48,
                        palette: "cameo.pal",
                        [cameoBinding])
                ]);

            if (!ManifestBindingsMatchPlan(manifest, projectPlan))
                return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.ExpansionFailed, "Asset manifest binding evidence did not match the project edit plan.");

            cancellationToken.ThrowIfCancellationRequested();

            Ra2AutomationTemplateWarningFact[] warnings = rulesCompilation.Warnings
                .Concat(artCompilation.Warnings)
                .Select(warning => new Ra2AutomationTemplateWarningFact(
                    Ra2AutomationTemplateWarningKind.FieldTrustCaution,
                    warning.SectionName,
                    warning.Key,
                    warning.TrustLevel,
                    warning.Message))
                .ToArray();

            return new Ra2AutomationProjectTemplateExpansionResult(
                snapshot,
                Ra2AutomationTemplateExpansionFailureKind.None,
                "The project template expansion and asset manifest succeeded.",
                projectPlan,
                manifest,
                warnings);
        }
        catch (OperationCanceledException)
        {
            return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.Canceled, "Project template expansion was canceled.");
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        {
            return Failure(snapshot, Ra2AutomationTemplateExpansionFailureKind.ExpansionFailed, "Project template expansion failed unexpectedly.");
        }
    }

    private static Ra2ContentTemplateDefinition CreateRulesDefinition(string id, int version)
        => new(
            id,
            version,
            "Techno rules binding",
            CreateParameters(),
            [
                new Ra2ContentTemplateSectionSpec(
                    Ra2ContentTemplateValueSource.Parameter("ownerSectionId"),
                    Ra2SectionKind.Techno,
                    [new Ra2ContentTemplateFieldSpec(
                        "Image",
                        Ra2ContentTemplateValueSource.Parameter("artSectionId"),
                        Ra2ContentTemplateFieldValidationPolicy.OpenReference)],
                    Ra2ContentTemplateSectionTargetMode.RequireExisting)
            ]);

    private static Ra2ContentTemplateDefinition CreateArtDefinition(string id, int version)
        => new(
            id,
            version,
            "Art image binding",
            CreateParameters(),
            [
                new Ra2ContentTemplateSectionSpec(
                    Ra2ContentTemplateValueSource.Parameter("artSectionId"),
                    Ra2SectionKind.ArtObject,
                    [
                        new Ra2ContentTemplateFieldSpec(
                            "Image",
                            Ra2ContentTemplateValueSource.Parameter("bodyAssetId"),
                            Ra2ContentTemplateFieldValidationPolicy.OpenReference),
                        new Ra2ContentTemplateFieldSpec(
                            "Cameo",
                            Ra2ContentTemplateValueSource.Parameter("cameoAssetId"),
                            Ra2ContentTemplateFieldValidationPolicy.OpenReference)
                    ],
                    Ra2ContentTemplateSectionTargetMode.CreateOrUpdate)
            ]);

    private static Ra2ContentTemplateParameter[] CreateParameters()
        =>
        [
            new("ownerSectionId", Ra2ContentTemplateParameterKind.Identifier, required: true),
            new("artSectionId", Ra2ContentTemplateParameterKind.Identifier, required: true),
            new("bodyAssetId", Ra2ContentTemplateParameterKind.Identifier, required: true),
            new("cameoAssetId", Ra2ContentTemplateParameterKind.Identifier, required: true),
            new("assetBrief", Ra2ContentTemplateParameterKind.String, required: true)
        ];

    private static (Ra2AutomationDocumentSnapshot? Rules, Ra2AutomationDocumentSnapshot? Art, Ra2AutomationTemplateExpansionFailureKind Failure)
        ResolveRulesArtPair(Ra2AutomationProjectSnapshot snapshot)
    {
        Ra2AutomationDocumentSnapshot[] rulesMd = ByFileName(snapshot, "rulesmd.ini");
        Ra2AutomationDocumentSnapshot[] artMd = ByFileName(snapshot, "artmd.ini");
        Ra2AutomationDocumentSnapshot[] rules = ByFileName(snapshot, "rules.ini");
        Ra2AutomationDocumentSnapshot[] art = ByFileName(snapshot, "art.ini");

        bool mdComplete = rulesMd.Length == 1 && artMd.Length == 1;
        bool classicComplete = rules.Length == 1 && art.Length == 1;
        if (mdComplete && classicComplete)
            return (null, null, Ra2AutomationTemplateExpansionFailureKind.ProjectDocumentAmbiguous);
        if (rulesMd.Length > 1 || artMd.Length > 1 || rules.Length > 1 || art.Length > 1)
            return (null, null, Ra2AutomationTemplateExpansionFailureKind.ProjectDocumentAmbiguous);
        if (mdComplete)
            return (rulesMd[0], artMd[0], Ra2AutomationTemplateExpansionFailureKind.None);
        if (classicComplete)
            return (rules[0], art[0], Ra2AutomationTemplateExpansionFailureKind.None);

        return (null, null, Ra2AutomationTemplateExpansionFailureKind.ProjectDocumentNotFound);
    }

    private static Ra2AutomationDocumentSnapshot[] ByFileName(Ra2AutomationProjectSnapshot snapshot, string fileName)
        => snapshot.Documents
            .Where(document => string.Equals(Path.GetFileName(document.FilePath), fileName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

    private static bool IsValidAssetStem(string value)
        => !string.IsNullOrWhiteSpace(value) &&
           value is not "." and not ".." &&
           value.Length <= Ra2AutomationAssetRequirement.MaximumFileNameLength - 4 &&
           !value.EndsWith(".shp", StringComparison.OrdinalIgnoreCase) &&
           value.IndexOfAny(['\\', '/', ':', '*', '?', '"', '<', '>', '|']) < 0;

    private static bool ManifestBindingsMatchPlan(
        Ra2AutomationAssetManifest manifest,
        Ra2AutomationProjectEditPlan plan)
    {
        foreach (Ra2AutomationAssetBindingFact binding in manifest.Requirements.SelectMany(requirement => requirement.Bindings))
        {
            Ra2AutomationEditPlan? documentPlan = plan.DocumentPlans.SingleOrDefault(item => item.ExpectedDocumentId == binding.DocumentId);
            bool hasOperation = documentPlan is not null && documentPlan.Operations.Any(operation =>
                operation.Kind == Ra2AutomationEditOperationKind.UpsertField &&
                string.Equals(operation.SectionName, binding.SectionName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(operation.Key, binding.Key, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(operation.Value, binding.Value, StringComparison.Ordinal));
            if ((binding.State == Ra2AutomationAssetBindingState.Proposed) != hasOperation)
                return false;
        }

        return true;
    }

    private static Ra2AutomationTemplateExpansionFailureKind MapFailure(Ra2ContentTemplateCompilationFailureKind failure)
        => failure switch
        {
            Ra2ContentTemplateCompilationFailureKind.MissingArgument => Ra2AutomationTemplateExpansionFailureKind.MissingRequiredArgument,
            Ra2ContentTemplateCompilationFailureKind.UnknownArgument => Ra2AutomationTemplateExpansionFailureKind.UnknownArgument,
            Ra2ContentTemplateCompilationFailureKind.DuplicateArgument => Ra2AutomationTemplateExpansionFailureKind.DuplicateArgument,
            Ra2ContentTemplateCompilationFailureKind.InvalidArgumentValue or Ra2ContentTemplateCompilationFailureKind.InvalidFieldValue => Ra2AutomationTemplateExpansionFailureKind.InvalidArguments,
            Ra2ContentTemplateCompilationFailureKind.FieldSchemaNotFound => Ra2AutomationTemplateExpansionFailureKind.FieldSchemaNotFound,
            Ra2ContentTemplateCompilationFailureKind.BlockedFieldTrust => Ra2AutomationTemplateExpansionFailureKind.BlockedFieldTrust,
            Ra2ContentTemplateCompilationFailureKind.RequiredSectionNotFound => Ra2AutomationTemplateExpansionFailureKind.RequiredSectionNotFound,
            Ra2ContentTemplateCompilationFailureKind.RequiredSectionKindMismatch => Ra2AutomationTemplateExpansionFailureKind.RequiredSectionKindMismatch,
            Ra2ContentTemplateCompilationFailureKind.OperationLimitExceeded => Ra2AutomationTemplateExpansionFailureKind.OperationLimitExceeded,
            Ra2ContentTemplateCompilationFailureKind.DocumentTooLarge => Ra2AutomationTemplateExpansionFailureKind.DocumentTooLarge,
            Ra2ContentTemplateCompilationFailureKind.Canceled => Ra2AutomationTemplateExpansionFailureKind.Canceled,
            _ => Ra2AutomationTemplateExpansionFailureKind.ExpansionFailed
        };

    private static Ra2AutomationProjectTemplateExpansionResult Failure(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationTemplateExpansionFailureKind failureKind,
        string message)
        => new(snapshot, failureKind, message, null, null);
}
