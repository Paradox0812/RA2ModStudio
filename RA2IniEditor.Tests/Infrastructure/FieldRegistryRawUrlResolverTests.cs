using RA2IniEditor.Infrastructure.FieldRegistry.Fetch;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryRawUrlResolverTests
{
    [Fact]
    public void TryResolve_RawGitHubUrl_ReturnsOriginalUrl()
    {
        GitHubRawUrlResolver resolver = new();

        bool resolved = resolver.TryResolve(
            "https://raw.githubusercontent.com/owner/repo/main/docs/fields.md",
            out string resolvedUrl,
            out string errorMessage);

        Assert.True(resolved, errorMessage);
        Assert.Equal("https://raw.githubusercontent.com/owner/repo/main/docs/fields.md", resolvedUrl);
    }

    [Fact]
    public void TryResolve_GitHubBlobUrl_ConvertsToRawGitHubUrl()
    {
        GitHubRawUrlResolver resolver = new();

        bool resolved = resolver.TryResolve(
            "https://github.com/owner/repo/blob/main/docs/fields.md",
            out string resolvedUrl,
            out string errorMessage);

        Assert.True(resolved, errorMessage);
        Assert.Equal("https://raw.githubusercontent.com/owner/repo/main/docs/fields.md", resolvedUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("http://raw.githubusercontent.com/owner/repo/main/docs/fields.md")]
    [InlineData("https://example.com/owner/repo/main/docs/fields.md")]
    [InlineData("https://github.com/owner/repo/tree/main/docs")]
    public void TryResolve_UnsupportedUrl_ReturnsFalse(string url)
    {
        GitHubRawUrlResolver resolver = new();

        bool resolved = resolver.TryResolve(url, out string resolvedUrl, out string errorMessage);

        Assert.False(resolved);
        Assert.Equal(string.Empty, resolvedUrl);
        Assert.NotEmpty(errorMessage);
    }
}
