namespace RA2IniEditor.AssetHost.Tests;

public sealed class Ra2ProviderProcessEnvironmentTests
{
    [Fact]
    public void Start_info_preserves_only_the_minimum_windows_runtime_environment()
    {
        Ra2GenerationProviderConfiguration configuration = AssetHostTestFixture.CreateConfiguration(
            AssetHostTestFixture.CreateUnusedWorkspacePath());

        System.Diagnostics.ProcessStartInfo startInfo = Ra2ProviderProcessRunner.CreateStartInfo(
            configuration,
            Ra2ProviderOperation.Probe,
            runDirectory: null);

        var expectedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DOTNET_NOLOGO",
            "DOTNET_CLI_TELEMETRY_OPTOUT"
        };
        foreach (string variableName in new[] { "SystemRoot", "WINDIR", "TEMP", "TMP" })
        {
            string? expectedValue = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrWhiteSpace(expectedValue))
            {
                continue;
            }

            expectedNames.Add(variableName);
            Assert.Equal(expectedValue, startInfo.Environment[variableName]);
        }

        Assert.Equal(
            expectedNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase),
            startInfo.Environment.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase));
        Assert.Equal("1", startInfo.Environment["DOTNET_NOLOGO"]);
        Assert.Equal("1", startInfo.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"]);
        Assert.DoesNotContain("RA2INI_HY3D_API_KEY", startInfo.Environment.Keys, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("HTTPS_PROXY", startInfo.Environment.Keys, StringComparer.OrdinalIgnoreCase);
    }
}
