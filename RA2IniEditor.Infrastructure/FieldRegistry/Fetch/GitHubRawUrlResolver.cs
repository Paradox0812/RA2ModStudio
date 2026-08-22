namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal sealed class GitHubRawUrlResolver : IFieldRegistryRawUrlResolver
{
    private const string RawHost = "raw.githubusercontent.com";
    private const string GitHubHost = "github.com";

    public bool TryResolve(string inputUrl, out string resolvedUrl, out string errorMessage)
    {
        resolvedUrl = string.Empty;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(inputUrl))
        {
            errorMessage = "URL cannot be empty.";
            return false;
        }

        if (!Uri.TryCreate(inputUrl.Trim(), UriKind.Absolute, out Uri? uri))
        {
            errorMessage = "URL is not valid.";
            return false;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Only https URLs are supported.";
            return false;
        }

        if (string.Equals(uri.Host, RawHost, StringComparison.OrdinalIgnoreCase))
        {
            if (uri.Segments.Length < 5)
            {
                errorMessage = "raw.githubusercontent.com URL must include owner, repo, branch, and path.";
                return false;
            }

            resolvedUrl = StripQueryAndFragment(uri);
            return true;
        }

        if (!string.Equals(uri.Host, GitHubHost, StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Only github.com and raw.githubusercontent.com URLs are supported.";
            return false;
        }

        return TryResolveGitHubBlobUrl(uri, out resolvedUrl, out errorMessage);
    }

    private static bool TryResolveGitHubBlobUrl(Uri uri, out string resolvedUrl, out string errorMessage)
    {
        resolvedUrl = string.Empty;
        errorMessage = string.Empty;

        string[] segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length < 5 ||
            !string.Equals(segments[2], "blob", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "github.com URL must use /owner/repo/blob/branch/path.";
            return false;
        }

        string owner = segments[0];
        string repo = segments[1];
        string branch = segments[3];
        string path = string.Join('/', segments.Skip(4));
        if (string.IsNullOrWhiteSpace(owner) ||
            string.IsNullOrWhiteSpace(repo) ||
            string.IsNullOrWhiteSpace(branch) ||
            string.IsNullOrWhiteSpace(path))
        {
            errorMessage = "github.com blob URL must include owner, repo, branch, and path.";
            return false;
        }

        resolvedUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{path}";
        return true;
    }

    private static string StripQueryAndFragment(Uri uri)
        => new UriBuilder(uri)
        {
            Query = string.Empty,
            Fragment = string.Empty
        }.Uri.AbsoluteUri;
}
