using RA2IniEditor.IDE.AI;
using System.Net.Http;
using System.Reflection;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class DeepSeekRa2AiClientFactoryTests
{
    private const string TestApiKey = "test-env-api-key-placeholder";
    private const string RetiredModelEnvironmentVariable = "DEEPSEEK_MODEL";

    [Fact]
    public void CreateOptionsFromEnvironment_ReadsApiKeyIntoOptions()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);

        DeepSeekRa2AiClientOptions options = DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment();

        Assert.Equal(TestApiKey, options.ApiKey);
    }

    [Fact]
    public void CreateOptionsFromEnvironment_UsesDefaultBaseUrlWhenMissing()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);

        DeepSeekRa2AiClientOptions options = DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment();

        Assert.Equal(DeepSeekRa2AiClientFactory.DefaultBaseUrl, options.BaseUrl);
    }

    [Fact]
    public void CreateOptionsFromEnvironment_UsesBaseUrlWhenValid()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);
        scope.Set(DeepSeekRa2AiClientFactory.BaseUrlEnvironmentVariable, "https://deepseek.example/v1/chat/completions");

        DeepSeekRa2AiClientOptions options = DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment();

        Assert.Equal("https://deepseek.example/v1/chat/completions", options.BaseUrl);
    }

    [Fact]
    public void CreateConfigurationSnapshot_InvalidBaseUrlIsNotSilentlyReplaced()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);
        scope.Set(DeepSeekRa2AiClientFactory.BaseUrlEnvironmentVariable, "not-a-url");

        DeepSeekRa2AiConfigurationSnapshot snapshot =
            DeepSeekRa2AiClientFactory.CreateConfigurationSnapshot();

        Assert.Equal(DeepSeekRa2AiConfigurationState.InvalidBaseUrl, snapshot.State);
        Assert.Equal("not-a-url", snapshot.Options.BaseUrl);
        Assert.Equal(DeepSeekRa2AiEndpointKind.Invalid, snapshot.EndpointKind);
        Assert.True(snapshot.UsesCustomEndpoint);
    }

    [Fact]
    public void CreateOptionsFromEnvironment_UsesV4FlashByDefault()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);

        DeepSeekRa2AiClientOptions options = DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment();

        Assert.Equal("deepseek-v4-flash", options.Model);
    }

    [Fact]
    public void CreateOptionsFromEnvironment_UsesExplicitV4Pro()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);

        DeepSeekRa2AiClientOptions options = DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment(
            DeepSeekRa2AiModel.V4Pro);

        Assert.Equal("deepseek-v4-pro", options.Model);
    }

    [Fact]
    public void CreateOptionsFromEnvironment_IgnoresRetiredModelEnvironmentVariable()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);
        scope.Set(RetiredModelEnvironmentVariable, "deepseek-v4-pro");

        DeepSeekRa2AiClientOptions options = DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment();

        Assert.Equal("deepseek-v4-flash", options.Model);
    }

    [Fact]
    public void CreateOptionsFromEnvironment_UsesDefaultTimeoutWhenMissing()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);

        DeepSeekRa2AiClientOptions options = DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment();

        Assert.Equal(120, DeepSeekRa2AiClientFactory.DefaultTimeoutSeconds);
        Assert.Equal(TimeSpan.FromSeconds(DeepSeekRa2AiClientFactory.DefaultTimeoutSeconds), options.Timeout);
    }

    [Fact]
    public void CreateClientFromEnvironment_DisablesCompetingHttpClientTimeout()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);

        IRa2AiClient client = DeepSeekRa2AiClientFactory.CreateClientFromEnvironment();
        DeepSeekRa2AiClient deepSeekClient = Assert.IsType<DeepSeekRa2AiClient>(client);
        FieldInfo httpClientField = Assert.IsAssignableFrom<FieldInfo>(typeof(DeepSeekRa2AiClient).GetField(
            "_httpClient",
            BindingFlags.Instance | BindingFlags.NonPublic));
        HttpClient httpClient = Assert.IsType<HttpClient>(httpClientField.GetValue(deepSeekClient));

        Assert.Equal(System.Threading.Timeout.InfiniteTimeSpan, httpClient.Timeout);
    }

    [Fact]
    public void CreateOptionsFromEnvironment_UsesTimeoutWhenValid()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);
        scope.Set(DeepSeekRa2AiClientFactory.TimeoutSecondsEnvironmentVariable, "42");

        DeepSeekRa2AiClientOptions options = DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment();

        Assert.Equal(TimeSpan.FromSeconds(42), options.Timeout);
    }

    [Fact]
    public void CreateConfigurationSnapshot_InvalidTimeoutIsNotSilentlyReplaced()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);
        scope.Set(DeepSeekRa2AiClientFactory.TimeoutSecondsEnvironmentVariable, "invalid");

        DeepSeekRa2AiConfigurationSnapshot snapshot =
            DeepSeekRa2AiClientFactory.CreateConfigurationSnapshot();

        Assert.Equal(DeepSeekRa2AiConfigurationState.InvalidTimeout, snapshot.State);
        Assert.Equal(TimeSpan.Zero, snapshot.Options.Timeout);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void CreateConfigurationSnapshot_OutOfRangeTimeoutIsInvalid(string value)
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);
        scope.Set(DeepSeekRa2AiClientFactory.TimeoutSecondsEnvironmentVariable, value);

        DeepSeekRa2AiConfigurationSnapshot snapshot =
            DeepSeekRa2AiClientFactory.CreateConfigurationSnapshot();

        Assert.Equal(DeepSeekRa2AiConfigurationState.InvalidTimeout, snapshot.State);
        Assert.Equal(TimeSpan.Zero, snapshot.Options.Timeout);
    }

    [Fact]
    public void CreateOptionsFromEnvironment_MissingApiKeyDoesNotThrow()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();

        DeepSeekRa2AiClientOptions options = DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment();

        Assert.Equal(string.Empty, options.ApiKey);
        Assert.Equal(DeepSeekRa2AiClientFactory.DefaultBaseUrl, options.BaseUrl);
        Assert.Equal(DeepSeekRa2AiClientFactory.DefaultModel, options.Model);
    }

    [Fact]
    public void CreateConfigurationSnapshot_MissingApiKeyReportsSafeState()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();

        DeepSeekRa2AiConfigurationSnapshot snapshot =
            DeepSeekRa2AiClientFactory.CreateConfigurationSnapshot();

        Assert.Equal(DeepSeekRa2AiConfigurationState.MissingApiKey, snapshot.State);
        Assert.False(snapshot.UsesCustomEndpoint);
        Assert.Equal(DeepSeekRa2AiEndpointKind.Official, snapshot.EndpointKind);
        Assert.Equal(DeepSeekRa2AiModel.V4Flash, snapshot.Model);
    }

    [Theory]
    [InlineData("https://api.deepseek.com", (int)DeepSeekRa2AiEndpointKind.Official)]
    [InlineData("https://API.DEEPSEEK.COM/CHAT/COMPLETIONS", (int)DeepSeekRa2AiEndpointKind.Official)]
    [InlineData("https://api.deepseek.com/chat/completions/", (int)DeepSeekRa2AiEndpointKind.Official)]
    [InlineData("https://api.deepseek.com:444", (int)DeepSeekRa2AiEndpointKind.Custom)]
    [InlineData("https://deepseek.example/v1", (int)DeepSeekRa2AiEndpointKind.Custom)]
    [InlineData("http://localhost:11434/v1", (int)DeepSeekRa2AiEndpointKind.Custom)]
    public void CreateConfigurationSnapshot_ClassifiesCanonicalCompletionEndpoint(
        string baseUrl,
        int expectedKind)
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);
        scope.Set(DeepSeekRa2AiClientFactory.BaseUrlEnvironmentVariable, baseUrl);

        DeepSeekRa2AiConfigurationSnapshot snapshot =
            DeepSeekRa2AiClientFactory.CreateConfigurationSnapshot();

        Assert.Equal(DeepSeekRa2AiConfigurationState.Ready, snapshot.State);
        Assert.Equal((DeepSeekRa2AiEndpointKind)expectedKind, snapshot.EndpointKind);
        Assert.Equal(expectedKind != (int)DeepSeekRa2AiEndpointKind.Official, snapshot.UsesCustomEndpoint);
    }

    [Fact]
    public void CreateClient_UsesTheExactSnapshotOptionsInstance()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);
        DeepSeekRa2AiConfigurationSnapshot snapshot =
            DeepSeekRa2AiClientFactory.CreateConfigurationSnapshot(DeepSeekRa2AiModel.V4Pro);
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, "changed-after-snapshot");

        DeepSeekRa2AiClient client = Assert.IsType<DeepSeekRa2AiClient>(
            DeepSeekRa2AiClientFactory.CreateClient(snapshot));
        FieldInfo optionsField = Assert.IsAssignableFrom<FieldInfo>(typeof(DeepSeekRa2AiClient).GetField(
            "_options",
            BindingFlags.Instance | BindingFlags.NonPublic));

        Assert.Same(snapshot.Options, optionsField.GetValue(client));
        Assert.Equal(TestApiKey, snapshot.Options.ApiKey);
        Assert.Equal(DeepSeekRa2AiConfigurationState.Ready, snapshot.State);
    }

    [Fact]
    public void CreateOptionsFromEnvironment_ToStringDoesNotExposeApiKey()
    {
        using EnvironmentVariableScope scope = CreateCleanScope();
        scope.Set(DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable, TestApiKey);

        DeepSeekRa2AiClientOptions options = DeepSeekRa2AiClientFactory.CreateOptionsFromEnvironment();

        Assert.DoesNotContain(TestApiKey, options.ToString());
        Assert.Contains("ApiKey=***", options.ToString());
    }

    private static EnvironmentVariableScope CreateCleanScope()
    {
        EnvironmentVariableScope scope = new([
            DeepSeekRa2AiClientFactory.ApiKeyEnvironmentVariable,
            DeepSeekRa2AiClientFactory.BaseUrlEnvironmentVariable,
            RetiredModelEnvironmentVariable,
            DeepSeekRa2AiClientFactory.TimeoutSecondsEnvironmentVariable
        ]);
        scope.ClearAll();
        return scope;
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly Dictionary<string, string?> _previousValues;

        public EnvironmentVariableScope(IEnumerable<string> names)
        {
            _previousValues = names.ToDictionary(
                name => name,
                Environment.GetEnvironmentVariable,
                StringComparer.Ordinal);
        }

        public void Set(string name, string value)
            => Environment.SetEnvironmentVariable(name, value);

        public void ClearAll()
        {
            foreach (string name in _previousValues.Keys)
                Environment.SetEnvironmentVariable(name, null);
        }

        public void Dispose()
        {
            foreach (KeyValuePair<string, string?> item in _previousValues)
                Environment.SetEnvironmentVariable(item.Key, item.Value);
        }
    }
}
