using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class RemoteSourcePresetEditorViewModelTests
{
    [Fact]
    public void Validate_AcceptsGitHubBlobUrl()
    {
        RemoteSourcePresetEditorViewModel viewModel = new(new FieldRegistryRemoteSourcePresetEditModel(
            null,
            "Ares Docs",
            "https://github.com/owner/repo/blob/main/docs/fields.md",
            "Docs",
            "ares, docs",
            true));

        Assert.True(viewModel.Validate());
        Assert.Contains("valid", viewModel.ValidationMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Ares Docs", viewModel.ToEditModel().Name);
    }

    [Fact]
    public void Validate_RejectsEmptyName()
    {
        RemoteSourcePresetEditorViewModel viewModel = new(new FieldRegistryRemoteSourcePresetEditModel(
            null,
            "",
            "https://github.com/owner/repo/blob/main/docs/fields.md",
            "",
            "",
            true));

        Assert.False(viewModel.Validate());
        Assert.Contains("name", viewModel.ValidationMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_RejectsUnsupportedUrl()
    {
        RemoteSourcePresetEditorViewModel viewModel = new(new FieldRegistryRemoteSourcePresetEditModel(
            null,
            "Docs",
            "https://example.com/docs/fields.md",
            "",
            "",
            true));

        Assert.False(viewModel.Validate());
        Assert.Contains("github.com", viewModel.ValidationMessage, StringComparison.OrdinalIgnoreCase);
    }
}
