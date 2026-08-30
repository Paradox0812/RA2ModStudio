namespace RA2IniEditor.AssetProviders.TencentHy3D;

internal static class TencentHy3DConstants
{
    internal const string Protocol = "ra2-voxel-generation/1";
    internal const string ProviderId = "tencent-hy3d-openai-compatible";
    internal const string ProviderVersion = "1.0.0";
    internal const string ModelId = "hunyuan-3d-professional";
    internal const string ModelRevision = "3.1-geometry";
    internal const string ApiKeyEnvironmentVariable = "RA2INI_HY3D_API_KEY";
    internal const string BaseUrlEnvironmentVariable = "RA2INI_HY3D_BASE_URL";
    internal const string FreeOnlyConfirmationEnvironmentVariable = "RA2INI_HY3D_FREE_ONLY_CONFIRMED";
    internal const string OfficialOrigin = "https://api.ai3d.cloud.tencent.com";
    internal const string SubmitPath = "/v1/ai3d/submit";
    internal const string QueryPath = "/v1/ai3d/query";
    internal const int MaximumImageBytes = 6 * 1024 * 1024;
    internal const int MaximumJsonBytes = 1024 * 1024;
    internal const long MaximumArtifactBytes = 256L * 1024 * 1024;
    internal const int MaximumRedirects = 3;
}

