using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using System.Reflection;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationProjectTemplateServiceTests
{
    private const string TemplateId = "techno-rules-art-asset-binding";

    [Fact]
    public void Catalog_DeclaresOneProjectManifestTemplateWithoutChangingExistingScope()
    {
        IReadOnlyList<Ra2AutomationTemplateDescriptor> templates = new Ra2AutomationTemplateService().GetTemplates();
        Assert.Equal(9, templates.Count);
        Ra2AutomationTemplateDescriptor project = Assert.Single(templates, item => item.Id == TemplateId);
        Assert.Equal(Ra2AutomationTemplateOutputKind.ProjectBinding, project.OutputKind);
        Assert.True(project.IsProjectTemplate);
        Assert.True(project.ProducesAssetManifest);
        Assert.Equal(["ownerSectionId", "artSectionId", "bodyAssetId", "cameoAssetId", "assetBrief"], project.Parameters.Select(item => item.Name));
        Assert.All(templates.Where(item => item != project), item =>
        {
            Assert.False(item.IsProjectTemplate);
            Assert.False(item.ProducesAssetManifest);
        });
    }

    [Fact]
    public void ExpandProjectTemplate_ProducesClosedRulesArtPlanManifestAndPreview()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AutomationTemplateExpansionRequest request = Request();
        Ra2AutomationCapabilityGateway gateway = new();

        Ra2AutomationProjectTemplateExpansionResult result = gateway.ExpandProjectTemplate(snapshot, request);

        Assert.True(result.Succeeded, result.Message);
        Assert.NotNull(result.Plan);
        Assert.NotNull(result.AssetManifest);
        Assert.Equal(snapshot.ProjectSessionId, result.ProjectSessionId);
        Assert.Equal(snapshot.ProjectRevision, result.ProjectRevision);
        Assert.Equal(2, result.Plan!.DocumentPlans.Count);

        Ra2AutomationEditPlan rules = result.Plan.DocumentPlans[0];
        Ra2AutomationEditPlan art = result.Plan.DocumentPlans[1];
        Assert.Empty(rules.SectionCreations);
        Ra2AutomationEditOperation rulesImage = Assert.Single(rules.Operations);
        Assert.Equal("HTNK", rulesImage.SectionName);
        Assert.Equal("Image", rulesImage.Key);
        Assert.Equal("HTNKART", rulesImage.Value);
        Assert.Equal("HTNKART", Assert.Single(art.SectionCreations).SectionName);
        Assert.Equal(2, art.Operations.Count);
        Ra2AutomationEditOperation artImage = Assert.Single(art.Operations, item => item.Key == "Image");
        Assert.Equal("Image", artImage.Key);
        Assert.Equal("HTNKBODY", artImage.Value);
        Ra2AutomationEditOperation artCameo = Assert.Single(art.Operations, item => item.Key == "Cameo");
        Assert.Equal("HTNKICON", artCameo.Value);

        Ra2AutomationAssetManifest manifest = result.AssetManifest!;
        Assert.Equal(2, manifest.Requirements.Count);
        Ra2AutomationAssetRequirement body = manifest.Requirements[0];
        Assert.Equal(Ra2AutomationAssetKind.ShpAnimation, body.Kind);
        Assert.Equal("HTNKBODY.shp", body.FileName);
        Assert.Equal(Ra2AutomationAssetBindingState.Proposed, Assert.Single(body.Bindings).State);
        Ra2AutomationAssetRequirement cameo = manifest.Requirements[1];
        Assert.Equal(Ra2AutomationAssetKind.Cameo, cameo.Kind);
        Assert.Equal("HTNKICON.shp", cameo.FileName);
        Assert.Equal(60, cameo.Width);
        Assert.Equal(48, cameo.Height);
        Assert.Equal("cameo.pal", cameo.Palette);
        Assert.Equal(Ra2AutomationAssetBindingState.Proposed, Assert.Single(cameo.Bindings).State);
        Assert.Contains(result.Plan.DocumentPlans.SelectMany(item => item.Operations), item => item.Key == "Cameo");

        Ra2AutomationProjectEditPreviewResult preview = gateway.PreviewProject(snapshot, result.Plan);
        Assert.True(preview.Succeeded, preview.Message);
        Assert.Equal(2, preview.DocumentPreviews.Count);
        Assert.Equal(3, preview.TotalOperationCount);
        Assert.Equal(1, preview.TotalSectionCreationCount);
    }

    [Fact]
    public void ExpandProjectTemplate_UpdatesExistingArtSectionWithoutDuplicateCreation()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot("[HTNKART]\nImage=OLD\n");

        Ra2AutomationProjectTemplateExpansionResult result = new Ra2AutomationTemplateService()
            .ExpandProjectTemplate(snapshot, Request());

        Assert.True(result.Succeeded, result.Message);
        Ra2AutomationEditPlan art = result.Plan!.DocumentPlans[1];
        Assert.Empty(art.SectionCreations);
        Assert.Equal(2, art.Operations.Count);
        Assert.Equal("HTNKBODY", Assert.Single(art.Operations, item => item.Key == "Image").Value);
        Assert.Equal("HTNKICON", Assert.Single(art.Operations, item => item.Key == "Cameo").Value);
    }

    [Fact]
    public void ExpandProjectTemplate_FailsClosedForMissingPairLeafFailureDuplicateArgumentsAndCancellation()
    {
        Ra2AutomationTemplateService service = new();
        Ra2AutomationProjectSnapshot missingArt = ProjectSnapshot(documents: [RulesSnapshot()]);
        AssertFailure(service.ExpandProjectTemplate(missingArt, Request()), Ra2AutomationTemplateExpansionFailureKind.ProjectDocumentNotFound);

        Ra2AutomationProjectSnapshot missingOwner = ProjectSnapshot(rulesText: "[VehicleTypes]\n0=OTHER\n\n[OTHER]\nImage=OLD\n");
        AssertFailure(service.ExpandProjectTemplate(missingOwner, Request()), Ra2AutomationTemplateExpansionFailureKind.RequiredSectionNotFound);

        Ra2AutomationTemplateExpansionRequest duplicate = new(
            TemplateId,
            1,
            Request().Arguments.Concat([new Ra2AutomationTemplateArgument("artSectionId", "OTHERART")]));
        AssertFailure(service.ExpandProjectTemplate(ProjectSnapshot(), duplicate), Ra2AutomationTemplateExpansionFailureKind.DuplicateArgument);

        Ra2AutomationTemplateExpansionRequest invalidAsset = new(
            TemplateId,
            1,
            Request().Arguments.Select(item => item.Name == "bodyAssetId"
                ? new Ra2AutomationTemplateArgument(item.Name, "BAD?ASSET")
                : item));
        AssertFailure(service.ExpandProjectTemplate(ProjectSnapshot(), invalidAsset), Ra2AutomationTemplateExpansionFailureKind.InvalidArguments);

        Ra2AutomationTemplateExpansionRequest duplicateAsset = new(
            TemplateId,
            1,
            Request().Arguments.Select(item => item.Name == "cameoAssetId"
                ? new Ra2AutomationTemplateArgument(item.Name, "HTNKBODY")
                : item));
        AssertFailure(service.ExpandProjectTemplate(ProjectSnapshot(), duplicateAsset), Ra2AutomationTemplateExpansionFailureKind.InvalidArguments);

        Ra2AutomationTemplateExpansionRequest suffixedAsset = new(
            TemplateId,
            1,
            Request().Arguments.Select(item => item.Name == "bodyAssetId"
                ? new Ra2AutomationTemplateArgument(item.Name, "HTNKBODY.shp")
                : item));
        AssertFailure(service.ExpandProjectTemplate(ProjectSnapshot(), suffixedAsset), Ra2AutomationTemplateExpansionFailureKind.InvalidArguments);

        using CancellationTokenSource source = new();
        source.Cancel();
        AssertFailure(service.ExpandProjectTemplate(ProjectSnapshot(), Request(), source.Token), Ra2AutomationTemplateExpansionFailureKind.Canceled);
    }

    [Fact]
    public void ExpandProjectTemplate_RejectsBothClassicAndMdPairsAsAmbiguous()
    {
        Ra2AutomationDocumentSnapshot rulesMd = RulesSnapshot();
        Ra2AutomationDocumentSnapshot artMd = ArtSnapshot();
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot(documents:
        [
            rulesMd,
            artMd,
            RulesSnapshot(Guid.Parse("55555555-5555-5555-5555-555555555555"), "C:\\mod\\rules.ini"),
            ArtSnapshot(Guid.Parse("66666666-6666-6666-6666-666666666666"), "C:\\mod\\art.ini")
        ]);

        AssertFailure(
            new Ra2AutomationTemplateService().ExpandProjectTemplate(snapshot, Request()),
            Ra2AutomationTemplateExpansionFailureKind.ProjectDocumentAmbiguous);
    }

    [Fact]
    public void AssetManifestContracts_AreImmutableBoundedAndNotSerializableAuthority()
    {
        Type[] types =
        [
            typeof(Ra2AutomationAssetKind),
            typeof(Ra2AutomationAssetBindingState),
            typeof(Ra2AutomationAssetBindingFact),
            typeof(Ra2AutomationAssetRequirement),
            typeof(Ra2AutomationAssetManifest),
            typeof(Ra2AutomationProjectTemplateExpansionResult)
        ];
        Assert.All(types, type =>
            Assert.DoesNotContain(type.GetCustomAttributes(), attribute =>
                attribute.GetType().Namespace?.Contains("Serialization", StringComparison.Ordinal) == true ||
                attribute.GetType().Name.Contains("Json", StringComparison.Ordinal)));
        Assert.All(types.Where(type => type.IsClass), type =>
            Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)));
        Assert.All(
            types.Where(type => type.IsClass).SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)),
            property => Assert.Null(property.SetMethod));
        Assert.Equal(["ShpAnimation", "Cameo", "VxlModel", "HvaAnimation"], Enum.GetNames<Ra2AutomationAssetKind>());
        Assert.Equal(["Proposed", "PendingSchema"], Enum.GetNames<Ra2AutomationAssetBindingState>());

        Ra2AutomationProjectTemplateExpansionResult result = new Ra2AutomationTemplateService()
            .ExpandProjectTemplate(ProjectSnapshot(), Request());
        IList<Ra2AutomationAssetRequirement> requirements = Assert.IsAssignableFrom<IList<Ra2AutomationAssetRequirement>>(result.AssetManifest!.Requirements);
        IList<Ra2AutomationAssetBindingFact> bindings = Assert.IsAssignableFrom<IList<Ra2AutomationAssetBindingFact>>(requirements[0].Bindings);
        Assert.True(requirements.IsReadOnly);
        Assert.True(bindings.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => requirements.Clear());
        Assert.Throws<NotSupportedException>(() => bindings.Clear());
    }

    [Fact]
    public void AssetManifestContracts_RejectPathsDuplicatesInvalidDimensionsAndPartialFailurePayload()
    {
        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AutomationAssetBindingFact binding = new(
            snapshot.Documents[1].DocumentId,
            snapshot.Documents[1].FilePath,
            "HTNKART",
            "Image",
            "HTNKBODY",
            Ra2AutomationAssetBindingState.Proposed);
        Assert.Throws<ArgumentException>(() => new Ra2AutomationAssetRequirement(
            "body", "..\\HTNKBODY.shp", Ra2AutomationAssetKind.ShpAnimation, "brief", null, null, null, [binding]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationAssetRequirement(
            "body", "HTNKBODY.shp", Ra2AutomationAssetKind.ShpAnimation, "brief", 60, null, null, [binding]));

        Ra2AutomationAssetRequirement requirement = new(
            "body", "HTNKBODY.shp", Ra2AutomationAssetKind.ShpAnimation, "brief", null, null, null, [binding]);
        Assert.Throws<ArgumentException>(() => new Ra2AutomationAssetManifest(
            snapshot.ProjectSessionId, TemplateId, 1, [requirement, requirement]));

        Ra2AutomationProjectTemplateExpansionResult success = new Ra2AutomationTemplateService()
            .ExpandProjectTemplate(snapshot, Request());
        Assert.Throws<ArgumentException>(() => new Ra2AutomationProjectTemplateExpansionResult(
            snapshot,
            Ra2AutomationTemplateExpansionFailureKind.ExpansionFailed,
            "failed",
            success.Plan,
            success.AssetManifest));
    }

    [Fact]
    public void TemplateServiceInterface_HasExactProjectMethodAndGatewayMatchesDirectService()
    {
        MethodInfo[] methods = typeof(IRa2AutomationTemplateService)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["ExpandProjectTemplate", "ExpandTemplate", "GetTemplates"], methods.Select(method => method.Name));

        MethodInfo projectMethod = Assert.Single(methods, method => method.Name == "ExpandProjectTemplate");
        Assert.Equal(typeof(Ra2AutomationProjectTemplateExpansionResult), projectMethod.ReturnType);
        Assert.Equal(
            [typeof(Ra2AutomationProjectSnapshot), typeof(Ra2AutomationTemplateExpansionRequest), typeof(CancellationToken)],
            projectMethod.GetParameters().Select(parameter => parameter.ParameterType));

        Ra2AutomationProjectSnapshot snapshot = ProjectSnapshot();
        Ra2AutomationTemplateExpansionRequest request = Request();
        Ra2AutomationProjectTemplateExpansionResult direct = new Ra2AutomationTemplateService().ExpandProjectTemplate(snapshot, request);
        Ra2AutomationProjectTemplateExpansionResult gateway = new Ra2AutomationCapabilityGateway().ExpandProjectTemplate(snapshot, request);
        Assert.Equal(direct.Succeeded, gateway.Succeeded);
        Assert.Equal(direct.FailureKind, gateway.FailureKind);
        Assert.Equal(direct.Plan!.DocumentPlans.Select(PlanShape), gateway.Plan!.DocumentPlans.Select(PlanShape));
        Assert.Equal(
            direct.AssetManifest!.Requirements.Select(item => $"{item.RequirementId}|{item.FileName}|{item.Kind}"),
            gateway.AssetManifest!.Requirements.Select(item => $"{item.RequirementId}|{item.FileName}|{item.Kind}"));
    }

    [Fact]
    public void ExpandProjectTemplate_WorksAgainstProductionBuiltInImageSchemas()
    {
        BuiltInRa2FieldDefinitionProvider provider = new();
        Ra2AutomationProjectSnapshot snapshot = new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            4,
            "C:\\mod",
            [
                new Ra2AutomationDocumentSnapshot(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    9,
                    "C:\\mod\\rulesmd.ini",
                    "[VehicleTypes]\n0=HTNK\n\n[HTNK]\nImage=OLDART\n",
                    true,
                    new Ra2AutomationFieldRegistrySnapshot(provider, 19)),
                new Ra2AutomationDocumentSnapshot(
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    9,
                    "C:\\mod\\artmd.ini",
                    "",
                    true,
                    new Ra2AutomationFieldRegistrySnapshot(provider, 19))
            ]);

        Ra2AutomationProjectTemplateExpansionResult result = new Ra2AutomationTemplateService()
            .ExpandProjectTemplate(snapshot, Request());

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(3, result.Plan!.DocumentPlans.Sum(item => item.Operations.Count));
    }

    [Fact]
    public void ExpandProjectTemplate_AcceptsNewAssetIdsWhenLocalRegistryContainsObservedImageEnums()
    {
        IRa2FieldDefinitionProvider provider = new ObservedEnumImageProvider();
        Ra2AutomationProjectSnapshot snapshot = new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            4,
            "C:\\mod",
            [
                new Ra2AutomationDocumentSnapshot(
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    9,
                    "C:\\mod\\rulesmd.ini",
                    "[VehicleTypes]\n0=HTNK\n\n[HTNK]\nImage=HTNK\n",
                    true,
                    new Ra2AutomationFieldRegistrySnapshot(provider, 19)),
                new Ra2AutomationDocumentSnapshot(
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    9,
                    "C:\\mod\\artmd.ini",
                    string.Empty,
                    true,
                    new Ra2AutomationFieldRegistrySnapshot(provider, 19))
            ]);

        Ra2AutomationProjectTemplateExpansionResult result = new Ra2AutomationTemplateService()
            .ExpandProjectTemplate(snapshot, Request());

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(
            ["HTNKART", "HTNKBODY", "HTNKICON"],
            result.Plan!.DocumentPlans.SelectMany(plan => plan.Operations).Select(operation => operation.Value));
    }

    private static Ra2AutomationTemplateExpansionRequest Request()
        => new(
            TemplateId,
            1,
            [
                new("ownerSectionId", "HTNK"),
                new("artSectionId", "HTNKART"),
                new("bodyAssetId", "HTNKBODY"),
                new("cameoAssetId", "HTNKICON"),
                new("assetBrief", "A heavy allied battle tank with twin barrels")
            ]);

    private static Ra2AutomationProjectSnapshot ProjectSnapshot(
        string artText = "",
        string rulesText = "[VehicleTypes]\n0=HTNK\n\n[HTNK]\nImage=OLDART\n",
        IEnumerable<Ra2AutomationDocumentSnapshot>? documents = null)
        => new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            4,
            "C:\\mod",
            documents ?? [RulesSnapshot(text: rulesText), ArtSnapshot(text: artText)]);

    private static Ra2AutomationDocumentSnapshot RulesSnapshot(
        Guid? id = null,
        string filePath = "C:\\mod\\rulesmd.ini",
        string text = "[VehicleTypes]\n0=HTNK\n\n[HTNK]\nImage=OLDART\n")
        => Snapshot(id ?? Guid.Parse("22222222-2222-2222-2222-222222222222"), filePath, text);

    private static Ra2AutomationDocumentSnapshot ArtSnapshot(
        Guid? id = null,
        string filePath = "C:\\mod\\artmd.ini",
        string text = "")
        => Snapshot(id ?? Guid.Parse("33333333-3333-3333-3333-333333333333"), filePath, text);

    private static Ra2AutomationDocumentSnapshot Snapshot(Guid id, string filePath, string text)
        => new(id, 9, filePath, text, true, new Ra2AutomationFieldRegistrySnapshot(new ImageProvider(), 19));

    private static void AssertFailure(
        Ra2AutomationProjectTemplateExpansionResult result,
        Ra2AutomationTemplateExpansionFailureKind failureKind)
    {
        Assert.False(result.Succeeded);
        Assert.Equal(failureKind, result.FailureKind);
        Assert.Null(result.Plan);
        Assert.Null(result.AssetManifest);
        Assert.Empty(result.Warnings);
    }

    private static string PlanShape(Ra2AutomationEditPlan plan)
        => string.Join(
            ";",
            plan.SectionCreations.Select(item => $"S:{item.SectionName}:{item.ExpectedSectionKind}")
                .Concat(plan.Operations.Select(item => $"O:{item.SectionName}:{item.Key}:{item.Value}")));

    private sealed class ImageProvider : IRa2FieldDefinitionProvider
    {
        private static readonly Ra2FieldDefinition[] Definitions =
        [
            Field("Image", Ra2SectionKind.Vehicle),
            Field("Image", Ra2SectionKind.ArtObject),
            Field("Cameo", Ra2SectionKind.ArtObject)
        ];

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = Definitions.FirstOrDefault(item => item.AppliesTo.Contains(sectionKind) && string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => Definitions.Where(item => item.AppliesTo.Contains(sectionKind)).ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => Definitions.Any(item => item.AppliesTo.Contains(sectionKind) && string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));

        private static Ra2FieldDefinition Field(string key, Ra2SectionKind sectionKind)
            => new(
                key,
                [sectionKind],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.Yuri,
                valueMetadata: new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference),
                registryQuality: "source-verified");
    }

    private sealed class ObservedEnumImageProvider : IRa2FieldDefinitionProvider
    {
        private static readonly Ra2FieldDefinition[] Definitions =
        [
            Field("Image", Ra2SectionKind.Vehicle, ["HTNK", "GTNK"]),
            Field("Image", Ra2SectionKind.ArtObject, ["HTNK"]),
            Field("Cameo", Ra2SectionKind.ArtObject, ["HTNK"])
        ];

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = Definitions.FirstOrDefault(item =>
                item.AppliesTo.Contains(sectionKind) &&
                string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => Definitions.Where(item => item.AppliesTo.Contains(sectionKind)).ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => Definitions.Any(item =>
                item.AppliesTo.Contains(sectionKind) &&
                string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));

        private static Ra2FieldDefinition Field(
            string key,
            Ra2SectionKind sectionKind,
            IEnumerable<string> observedValues)
            => new(
                key,
                [sectionKind],
                FieldEditorKind.Enum,
                Ra2FieldSourceKind.User,
                valueMetadata: new Ra2FieldValueMetadata(
                    Ra2FieldValueKind.Enum,
                    allowedValues: observedValues.Select(value => new Ra2FieldAllowedValue(value)).ToArray()),
                registryQuality: "source-verified");
    }
}
