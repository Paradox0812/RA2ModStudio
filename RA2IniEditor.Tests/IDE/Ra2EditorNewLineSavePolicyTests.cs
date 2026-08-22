using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorNewLineSavePolicyTests
{
    [Theory]
    [InlineData("[A]\r\nKey=1\r\n")]
    [InlineData("[A]\nKey=1\n")]
    [InlineData("[A]\rKey=1\r")]
    [InlineData("[A]\r\nKey=1\n")]
    [InlineData("[A]")]
    public void GetDefaultPolicy_ReturnsPreserveCurrentText(string text)
    {
        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);

        Ra2EditorNewLineSavePolicy policy = new Ra2EditorNewLinePolicyProvider().GetDefaultPolicy(document);

        Assert.Equal(Ra2EditorNewLineSavePolicy.PreserveCurrentText, policy);
    }

    [Fact]
    public void GetDefaultPolicy_RejectsNullDocument()
    {
        Assert.Throws<ArgumentNullException>(() => new Ra2EditorNewLinePolicyProvider().GetDefaultPolicy(null!));
    }
}
