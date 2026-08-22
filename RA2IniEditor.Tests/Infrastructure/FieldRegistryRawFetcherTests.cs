using System.Net;
using System.Text;
using RA2IniEditor.Infrastructure.FieldRegistry.Fetch;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryRawFetcherTests
{
    [Fact]
    public async Task FetchAsync_Success_ReturnsRawTextAndMetadata()
    {
        using HttpClient httpClient = new(new StaticHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Owner=\nStrength=600", Encoding.UTF8, "text/plain")
        }));
        GitHubRawFieldRegistryFetcher fetcher = new(httpClient);

        FieldRegistryRawFetchResult result = await fetcher.FetchAsync(
            new FieldRegistryRawFetchRequest("https://raw.githubusercontent.com/owner/repo/main/fields.md"),
            CancellationToken.None);

        Assert.Equal("fields.md", result.SourceName);
        Assert.Equal("Owner=\nStrength=600", result.Text);
        Assert.True(result.ByteCount > 0);
    }

    [Fact]
    public async Task FetchAsync_FailureStatus_Throws()
    {
        using HttpClient httpClient = new(new StaticHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            ReasonPhrase = "Not Found",
            Content = new StringContent("missing", Encoding.UTF8, "text/plain")
        }));
        GitHubRawFieldRegistryFetcher fetcher = new(httpClient);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fetcher.FetchAsync(
                new FieldRegistryRawFetchRequest("https://raw.githubusercontent.com/owner/repo/main/fields.md"),
                CancellationToken.None));

        Assert.Contains("404", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FetchAsync_ResponseTooLarge_Throws()
    {
        using HttpClient httpClient = new(new StaticHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("123456", Encoding.UTF8, "text/plain")
        }));
        GitHubRawFieldRegistryFetcher fetcher = new(httpClient);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fetcher.FetchAsync(
                new FieldRegistryRawFetchRequest("https://raw.githubusercontent.com/owner/repo/main/fields.md", maxBytes: 5),
                CancellationToken.None));

        Assert.Contains("byte limit", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FetchAsync_UnsupportedUrl_ThrowsWithoutSendingRequest()
    {
        CountingHttpMessageHandler handler = new();
        using HttpClient httpClient = new(handler);
        GitHubRawFieldRegistryFetcher fetcher = new(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fetcher.FetchAsync(
                new FieldRegistryRawFetchRequest("https://example.com/fields.md"),
                CancellationToken.None));

        Assert.Equal(0, handler.SendCount);
    }

    [Fact]
    public async Task FetchAsync_NonTextContent_Throws()
    {
        using HttpClient httpClient = new(new StaticHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([1, 2, 3])
            {
                Headers =
                {
                    ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png")
                }
            }
        }));
        GitHubRawFieldRegistryFetcher fetcher = new(httpClient);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fetcher.FetchAsync(
                new FieldRegistryRawFetchRequest("https://raw.githubusercontent.com/owner/repo/main/image.png"),
                CancellationToken.None));

        Assert.Contains("Unsupported content type", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FetchAsync_Cancellation_ThrowsOperationCanceled()
    {
        using HttpClient httpClient = new(new CancelingHttpMessageHandler());
        GitHubRawFieldRegistryFetcher fetcher = new(httpClient);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            fetcher.FetchAsync(
                new FieldRegistryRawFetchRequest("https://raw.githubusercontent.com/owner/repo/main/fields.md"),
                cancellation.Token));
    }

    private sealed class StaticHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StaticHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }

    private sealed class CountingHttpMessageHandler : HttpMessageHandler
    {
        public int SendCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SendCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class CancelingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromCanceled<HttpResponseMessage>(cancellationToken);
    }
}
