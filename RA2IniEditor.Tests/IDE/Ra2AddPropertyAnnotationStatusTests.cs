using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.ViewModels.FieldBrowser;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AddPropertyAnnotationStatusTests
{
    [Fact]
    public void FromLoadResult_LoadedShowsRelativeSidecarPath()
    {
        Ra2FieldAnnotationStatusViewModel status = Ra2FieldAnnotationStatusViewModel.FromLoadResult(
            @"C:\Project\.ra2ide\field-annotations.zh-CN.json",
            new Ra2FieldAnnotationLoadResult(Ra2FieldAnnotationPack.Empty(), []));

        Assert.True(status.IsLoaded);
        Assert.False(status.HasWarnings);
        Assert.Equal("字段注释：已加载 .ra2ide/field-annotations.zh-CN.json", status.StatusText);
    }

    [Fact]
    public void FromLoadResult_NotFoundShowsFallbackStatus()
    {
        Ra2FieldAnnotationStatusViewModel status = Ra2FieldAnnotationStatusViewModel.FromLoadResult(
            "",
            new Ra2FieldAnnotationLoadResult(Ra2FieldAnnotationPack.Empty(), ["Annotation sidecar was not found."]));

        Assert.False(status.IsLoaded);
        Assert.True(status.HasWarnings);
        Assert.Equal("字段注释：未找到项目注释库，已回退到字段库。", status.StatusText);
    }

    [Fact]
    public void FromLoadResult_FailedShowsFallbackStatusAndWarnings()
    {
        Ra2FieldAnnotationStatusViewModel status = Ra2FieldAnnotationStatusViewModel.FromLoadResult(
            "bad.json",
            new Ra2FieldAnnotationLoadResult(Ra2FieldAnnotationPack.Empty(), ["bad json"], success: false));

        Assert.False(status.IsLoaded);
        Assert.True(status.HasWarnings);
        Assert.Equal(["bad json"], status.Warnings);
        Assert.Equal("字段注释：加载失败，已回退到字段库。", status.StatusText);
    }
}
