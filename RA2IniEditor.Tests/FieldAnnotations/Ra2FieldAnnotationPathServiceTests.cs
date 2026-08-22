using RA2IniEditor.IDE.FieldAnnotations;
using Xunit;

namespace RA2IniEditor.Tests.FieldAnnotations;

public sealed class Ra2FieldAnnotationPathServiceTests
{
    [Fact]
    public void GetProjectAnnotationPath_ReturnsProjectSidecarPath()
    {
        Ra2FieldAnnotationPathService service = new();

        string path = service.GetProjectAnnotationPath(@"C:\Mods\YR");

        Assert.Equal(@"C:\Mods\YR\.ra2ide\field-annotations.zh-CN.json", path);
    }

    [Fact]
    public void GetProjectAnnotationPath_EmptyProjectRoot_ReturnsEmptyPath()
    {
        Ra2FieldAnnotationPathService service = new();

        Assert.Equal(string.Empty, service.GetProjectAnnotationPath(""));
    }
}
