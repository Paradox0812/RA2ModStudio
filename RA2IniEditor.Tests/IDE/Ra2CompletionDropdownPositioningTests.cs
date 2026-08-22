using RA2IniEditor.IDE.ViewModels.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionDropdownPositioningTests
{
    [Theory]
    [InlineData(null, -10, 0)]
    [InlineData("", 10, 0)]
    [InlineData("abc", -1, 0)]
    [InlineData("abc", 2, 2)]
    [InlineData("abc", 30, 3)]
    public void NormalizeCaretOffset_ClampsWithoutThrowing(string? text, int caretOffset, int expected)
    {
        Assert.Equal(expected, Ra2CompletionDropdownPositioning.NormalizeCaretOffset(text, caretOffset));
    }

    [Theory]
    [InlineData(null, 0, false)]
    [InlineData("", 0, false)]
    [InlineData("abc", -1, false)]
    [InlineData("abc", 4, false)]
    [InlineData("abc", 0, true)]
    [InlineData("abc", 3, true)]
    public void CanShowNearCaret_RejectsEmptyTextAndOutOfRangeOffsets(string? text, int caretOffset, bool expected)
    {
        Assert.Equal(expected, Ra2CompletionDropdownPositioning.CanShowNearCaret(text, caretOffset));
    }
}
