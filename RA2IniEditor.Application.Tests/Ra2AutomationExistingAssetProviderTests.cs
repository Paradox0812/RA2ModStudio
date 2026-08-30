using System.Reflection;
using System.Security.Cryptography;
using RA2IniEditor.Application.Automation.Experimental;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationExistingAssetProviderTests
{
    [Fact]
    public void Descriptor_DeclaresExactStablePassthroughCapability()
    {
        Ra2AutomationAssetProviderDescriptor descriptor = new Ra2AutomationExistingAssetProvider().GetDescriptor();

        Assert.Equal("existing-asset-passthrough", descriptor.Id);
        Assert.Equal(1, descriptor.Version);
        Assert.Equal(
            [
                Ra2AutomationAssetKind.ShpAnimation,
                Ra2AutomationAssetKind.Cameo,
                Ra2AutomationAssetKind.VxlModel,
                Ra2AutomationAssetKind.HvaAnimation
            ],
            descriptor.SupportedKinds);
        IList<Ra2AutomationAssetKind> kinds = Assert.IsAssignableFrom<IList<Ra2AutomationAssetKind>>(descriptor.SupportedKinds);
        Assert.True(kinds.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => kinds.Clear());
    }

    [Fact]
    public void Resolve_ProducesManifestOrderedHashedArtifactsWithDefensiveCopies()
    {
        Ra2AutomationAssetManifest manifest = Manifest(
            Requirement("body", "BODY.shp", Ra2AutomationAssetKind.ShpAnimation),
            Requirement("cameo", "ICON.shp", Ra2AutomationAssetKind.Cameo));
        byte[] bodyBytes = [1, 2, 3, 4];
        byte[] cameoBytes = [9, 8, 7];
        Ra2AutomationAssetSource body = new("body", "BODY.shp", Ra2AutomationAssetKind.ShpAnimation, bodyBytes);
        Ra2AutomationAssetSource cameo = new("cameo", "ICON.shp", Ra2AutomationAssetKind.Cameo, cameoBytes);
        bodyBytes[0] = 99;
        cameoBytes[0] = 99;

        Ra2AutomationAssetProviderResult result = new Ra2AutomationExistingAssetProvider()
            .Resolve(manifest, [cameo, body]);

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(Ra2AutomationAssetProviderFailureKind.None, result.FailureKind);
        Assert.Empty(result.RelatedRequirementIds);
        Assert.Equal(["body", "cameo"], result.Artifacts.Select(artifact => artifact.RequirementId));
        Ra2AutomationAssetArtifact bodyArtifact = result.Artifacts[0];
        Assert.Equal(Convert.ToHexString(SHA256.HashData(new byte[] { 1, 2, 3, 4 })), bodyArtifact.Sha256);
        Assert.Equal(Ra2AutomationAssetVerificationLevel.IdentityExtensionAndHash, bodyArtifact.VerificationLevel);
        byte[] copy = bodyArtifact.CopyContent();
        copy[0] = 55;
        Assert.Equal(1, bodyArtifact.CopyContent()[0]);
        Assert.Equal(1, body.CopyContent()[0]);
    }

    [Fact]
    public void Resolve_IsDeterministicForTheSameManifestAndSources()
    {
        Ra2AutomationAssetManifest manifest = Manifest(Requirement("body", "BODY.shp", Ra2AutomationAssetKind.ShpAnimation));
        Ra2AutomationAssetSource source = new("body", "BODY.shp", Ra2AutomationAssetKind.ShpAnimation, [4, 3, 2, 1]);
        Ra2AutomationExistingAssetProvider provider = new();

        Ra2AutomationAssetProviderResult first = provider.Resolve(manifest, [source]);
        Ra2AutomationAssetProviderResult second = provider.Resolve(manifest, [source]);

        Assert.True(first.Succeeded);
        Assert.True(second.Succeeded);
        Assert.Equal(first.ProviderId, second.ProviderId);
        Assert.Equal(first.ProviderVersion, second.ProviderVersion);
        Assert.Equal(first.Artifacts.Select(Shape), second.Artifacts.Select(Shape));
    }

    [Fact]
    public void Resolve_FailsClosedForPendingSchemaAndUnsupportedManifestExtension()
    {
        Ra2AutomationExistingAssetProvider provider = new();
        Ra2AutomationAssetManifest pending = Manifest(Requirement(
            "cameo",
            "ICON.shp",
            Ra2AutomationAssetKind.Cameo,
            Ra2AutomationAssetBindingState.PendingSchema));
        Ra2AutomationAssetManifest wrongExtension = Manifest(Requirement("cameo", "ICON.png", Ra2AutomationAssetKind.Cameo));

        AssertFailure(
            provider.Resolve(pending, [new("cameo", "ICON.shp", Ra2AutomationAssetKind.Cameo, [1])]),
            Ra2AutomationAssetProviderFailureKind.InvalidManifest,
            "cameo");
        AssertFailure(
            provider.Resolve(wrongExtension, [new("cameo", "ICON.png", Ra2AutomationAssetKind.Cameo, [1])]),
            Ra2AutomationAssetProviderFailureKind.InvalidManifest,
            "cameo");
    }

    [Fact]
    public void Resolve_FailsClosedForMissingUnexpectedDuplicateAndMismatchedSources()
    {
        Ra2AutomationAssetManifest manifest = Manifest(Requirement("body", "BODY.shp", Ra2AutomationAssetKind.ShpAnimation));
        Ra2AutomationExistingAssetProvider provider = new();
        Ra2AutomationAssetSource valid = new("body", "BODY.shp", Ra2AutomationAssetKind.ShpAnimation, [1]);

        AssertFailure(provider.Resolve(manifest, []), Ra2AutomationAssetProviderFailureKind.MissingSource, "body");
        AssertFailure(
            provider.Resolve(manifest, [valid, new("extra", "EXTRA.shp", Ra2AutomationAssetKind.ShpAnimation, [2])]),
            Ra2AutomationAssetProviderFailureKind.UnexpectedSource,
            "extra");
        AssertFailure(
            provider.Resolve(manifest, [valid, valid]),
            Ra2AutomationAssetProviderFailureKind.SourceMismatch,
            "body");
        AssertFailure(
            provider.Resolve(manifest, [new("body", "OTHER.shp", Ra2AutomationAssetKind.ShpAnimation, [1])]),
            Ra2AutomationAssetProviderFailureKind.SourceMismatch,
            "body");
    }

    [Fact]
    public void Resolve_EnforcesAggregateLimitWithoutReturningPartialArtifacts()
    {
        const int sourceLength = 13 * 1024 * 1024;
        byte[] sharedInput = new byte[sourceLength];
        Ra2AutomationAssetRequirement[] requirements = Enumerable.Range(0, 5)
            .Select(index => Requirement($"asset-{index}", $"ASSET{index}.shp", Ra2AutomationAssetKind.ShpAnimation))
            .ToArray();
        Ra2AutomationAssetSource[] sources = Enumerable.Range(0, 5)
            .Select(index => new Ra2AutomationAssetSource(
                $"asset-{index}",
                $"ASSET{index}.shp",
                Ra2AutomationAssetKind.ShpAnimation,
                sharedInput))
            .ToArray();

        Ra2AutomationAssetProviderResult result = new Ra2AutomationExistingAssetProvider()
            .Resolve(Manifest(requirements), sources);

        AssertFailure(result, Ra2AutomationAssetProviderFailureKind.AggregateContentLimitExceeded, "asset-0");
        Assert.Equal(5, result.RelatedRequirementIds.Count);
    }

    [Fact]
    public void Resolve_MapsCancellationToZeroArtifactFailure()
    {
        Ra2AutomationAssetManifest manifest = Manifest(Requirement("body", "BODY.shp", Ra2AutomationAssetKind.ShpAnimation));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Ra2AutomationAssetProviderResult result = new Ra2AutomationExistingAssetProvider().Resolve(
            manifest,
            [new("body", "BODY.shp", Ra2AutomationAssetKind.ShpAnimation, [1])],
            cancellation.Token);

        AssertFailure(result, Ra2AutomationAssetProviderFailureKind.Canceled);
    }

    [Fact]
    public void SourceContract_RejectsPathsEmptyAndOversizedContent()
    {
        Assert.Throws<ArgumentException>(() => new Ra2AutomationAssetSource(
            "body", "folder\\BODY.shp", Ra2AutomationAssetKind.ShpAnimation, [1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationAssetSource(
            "body", "BODY.shp", Ra2AutomationAssetKind.ShpAnimation, []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ra2AutomationAssetSource(
            "body",
            "BODY.shp",
            Ra2AutomationAssetKind.ShpAnimation,
            new byte[Ra2AutomationAssetSource.MaximumContentBytes + 1]));
    }

    [Fact]
    public void ProviderContracts_HaveExactImmutableTypedSurface()
    {
        Type[] immutableTypes =
        [
            typeof(Ra2AutomationAssetProviderDescriptor),
            typeof(Ra2AutomationAssetSource),
            typeof(Ra2AutomationAssetArtifact),
            typeof(Ra2AutomationAssetProviderResult)
        ];
        Assert.All(immutableTypes, type =>
            Assert.All(type.GetProperties(BindingFlags.Public | BindingFlags.Instance), property => Assert.Null(property.SetMethod)));
        Assert.Empty(typeof(Ra2AutomationAssetProviderResult).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Single(typeof(Ra2AutomationAssetProviderDescriptor).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Single(typeof(Ra2AutomationAssetArtifact).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        ConstructorInfo sourceConstructor = Assert.Single(typeof(Ra2AutomationAssetSource).GetConstructors());
        Assert.Equal(
            [typeof(string), typeof(string), typeof(Ra2AutomationAssetKind), typeof(byte[])],
            sourceConstructor.GetParameters().Select(parameter => parameter.ParameterType));

        MethodInfo[] interfaceMethods = typeof(IRa2AutomationAssetProvider).GetMethods();
        Assert.Equal(["GetDescriptor", "Resolve"], interfaceMethods.Select(method => method.Name).OrderBy(name => name, StringComparer.Ordinal));
        MethodInfo resolve = interfaceMethods.Single(method => method.Name == "Resolve");
        Assert.Equal(typeof(Ra2AutomationAssetProviderResult), resolve.ReturnType);
        Assert.Equal(
            [typeof(Ra2AutomationAssetManifest), typeof(IReadOnlyList<Ra2AutomationAssetSource>), typeof(CancellationToken)],
            resolve.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.True(resolve.GetParameters()[2].IsOptional);
        Assert.Equal(
            ["CreateFailure", "CreateSuccess"],
            typeof(Ra2AutomationAssetProviderResult)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .OrderBy(name => name, StringComparer.Ordinal));

        Assert.Equal(
            ["None", "InvalidManifest", "MissingSource", "UnexpectedSource", "SourceMismatch", "AggregateContentLimitExceeded", "Canceled", "ProviderFailed"],
            Enum.GetNames<Ra2AutomationAssetProviderFailureKind>());
        Assert.Equal(["IdentityExtensionAndHash"], Enum.GetNames<Ra2AutomationAssetVerificationLevel>());
    }

    [Fact]
    public void ResultFactories_RejectUnclosedSuccessAndNoneFailure()
    {
        Ra2AutomationAssetManifest manifest = Manifest(Requirement("body", "BODY.shp", Ra2AutomationAssetKind.ShpAnimation));
        Ra2AutomationAssetProviderDescriptor descriptor = new(
            "test-provider",
            1,
            [Ra2AutomationAssetKind.ShpAnimation]);
        Ra2AutomationAssetArtifact wrong = new(
            "other",
            "OTHER.shp",
            Ra2AutomationAssetKind.ShpAnimation,
            [1],
            Ra2AutomationAssetVerificationLevel.IdentityExtensionAndHash);

        Assert.Throws<ArgumentException>(() => Ra2AutomationAssetProviderResult.CreateSuccess(
            manifest,
            descriptor,
            "success",
            [wrong]));
        Assert.Throws<ArgumentException>(() => Ra2AutomationAssetProviderResult.CreateFailure(
            manifest,
            descriptor,
            Ra2AutomationAssetProviderFailureKind.None,
            "not a failure"));

        Ra2AutomationAssetManifest pending = Manifest(Requirement(
            "body",
            "BODY.shp",
            Ra2AutomationAssetKind.ShpAnimation,
            Ra2AutomationAssetBindingState.PendingSchema));
        Ra2AutomationAssetArtifact matching = new(
            "body",
            "BODY.shp",
            Ra2AutomationAssetKind.ShpAnimation,
            [1],
            Ra2AutomationAssetVerificationLevel.IdentityExtensionAndHash);
        Assert.Throws<ArgumentException>(() => Ra2AutomationAssetProviderResult.CreateSuccess(
            pending,
            descriptor,
            "success",
            [matching]));
    }

    private static string Shape(Ra2AutomationAssetArtifact artifact)
        => $"{artifact.RequirementId}|{artifact.FileName}|{artifact.Kind}|{artifact.ContentLength}|{artifact.Sha256}|{artifact.VerificationLevel}";

    private static void AssertFailure(
        Ra2AutomationAssetProviderResult result,
        Ra2AutomationAssetProviderFailureKind expectedFailure,
        string? expectedRelatedId = null)
    {
        Assert.False(result.Succeeded);
        Assert.Equal(expectedFailure, result.FailureKind);
        Assert.Empty(result.Artifacts);
        if (expectedRelatedId is not null)
            Assert.Contains(expectedRelatedId, result.RelatedRequirementIds);
    }

    private static Ra2AutomationAssetManifest Manifest(params Ra2AutomationAssetRequirement[] requirements)
        => new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "asset-provider-test",
            1,
            requirements);

    private static Ra2AutomationAssetRequirement Requirement(
        string requirementId,
        string fileName,
        Ra2AutomationAssetKind kind,
        Ra2AutomationAssetBindingState bindingState = Ra2AutomationAssetBindingState.Proposed)
        => new(
            requirementId,
            fileName,
            kind,
            "test asset",
            width: null,
            height: null,
            palette: null,
            [new Ra2AutomationAssetBindingFact(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "C:\\mod\\artmd.ini",
                $"ART_{requirementId}",
                "Image",
                Path.GetFileNameWithoutExtension(fileName),
                bindingState)]);
}
