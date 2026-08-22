using System.Text;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2TextFirstFileWriterTests
{
    [Fact]
    public void Write_UsesUtf8NoBomForUtf8Metadata()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string path = workspace.PathFor("rules.ini");
        Ra2TextFirstFileWriter writer = new();

        Ra2TextFileWriteResult result = writer.Write(CreatePlan(
            path,
            "[E1]\nName=中文",
            new Ra2EditorTextEncodingMetadata(Ra2EditorTextEncodingKind.Utf8, "UTF-8", false)));

        Assert.True(result.Success, result.Message);
        byte[] bytes = File.ReadAllBytes(path);
        Assert.False(HasPrefix(bytes, [0xEF, 0xBB, 0xBF]));
        Assert.Equal("[E1]\nName=中文", Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public void Write_UsesUtf8BomForUtf8BomMetadata()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string path = workspace.PathFor("rules.ini");
        Ra2TextFirstFileWriter writer = new();

        Ra2TextFileWriteResult result = writer.Write(CreatePlan(
            path,
            "[E1]\nStrength=125",
            new Ra2EditorTextEncodingMetadata(Ra2EditorTextEncodingKind.Utf8Bom, "UTF-8 BOM", true)));

        Assert.True(result.Success, result.Message);
        byte[] bytes = File.ReadAllBytes(path);
        Assert.True(HasPrefix(bytes, [0xEF, 0xBB, 0xBF]));
    }

    [Fact]
    public void Write_UsesUtf16LeForUtf16LeMetadata()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string path = workspace.PathFor("rules.ini");
        Ra2TextFirstFileWriter writer = new();

        Ra2TextFileWriteResult result = writer.Write(CreatePlan(
            path,
            "[E1]\nStrength=125",
            new Ra2EditorTextEncodingMetadata(Ra2EditorTextEncodingKind.Utf16Le, "UTF-16 LE", true)));

        Assert.True(result.Success, result.Message);
        byte[] bytes = File.ReadAllBytes(path);
        Assert.True(HasPrefix(bytes, [0xFF, 0xFE]));
        Assert.Equal("[E1]\nStrength=125", Encoding.Unicode.GetString(bytes[2..]));
    }

    [Fact]
    public void Write_UsesUtf16BeForUtf16BeMetadata()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string path = workspace.PathFor("rules.ini");
        Ra2TextFirstFileWriter writer = new();

        Ra2TextFileWriteResult result = writer.Write(CreatePlan(
            path,
            "[E1]\nStrength=125",
            new Ra2EditorTextEncodingMetadata(Ra2EditorTextEncodingKind.Utf16Be, "UTF-16 BE", true)));

        Assert.True(result.Success, result.Message);
        byte[] bytes = File.ReadAllBytes(path);
        Assert.True(HasPrefix(bytes, [0xFE, 0xFF]));
        Assert.Equal("[E1]\nStrength=125", Encoding.BigEndianUnicode.GetString(bytes[2..]));
    }

    [Fact]
    public void Write_UnknownEncodingFallsBackToUtf8NoBom()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string path = workspace.PathFor("rules.ini");
        Ra2TextFirstFileWriter writer = new();

        Ra2TextFileWriteResult result = writer.Write(CreatePlan(
            path,
            "[E1]\nName=中文",
            Ra2EditorTextEncodingMetadata.Unknown));

        Assert.True(result.Success, result.Message);
        byte[] bytes = File.ReadAllBytes(path);
        Assert.False(HasPrefix(bytes, [0xEF, 0xBB, 0xBF]));
        Assert.Equal("[E1]\nName=中文", Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public void Write_PreservesPlanTextNewlinesWithoutNormalizing()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string path = workspace.PathFor("rules.ini");
        const string text = "[E1]\r\nStrength=125\nName=Test\r";
        Ra2TextFirstFileWriter writer = new();

        Ra2TextFileWriteResult result = writer.Write(CreatePlan(path, text, Ra2EditorTextEncodingMetadata.Unknown));

        Assert.True(result.Success, result.Message);
        Assert.Equal(text, File.ReadAllText(path, Encoding.UTF8));
    }

    [Fact]
    public void Write_WhenStoreFailsReturnsFailureResult()
    {
        FailingIniFileStore store = new();
        Ra2TextFirstFileWriter writer = new(store);

        Ra2TextFileWriteResult result = writer.Write(CreatePlan(
            "rules.ini",
            "[E1]\nStrength=125",
            Ra2EditorTextEncodingMetadata.Unknown));

        Assert.False(result.Success);
        Assert.NotNull(result.Exception);
        Assert.Contains("simulated", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("[E1]\nStrength=125", store.LastText);
    }

    private static Ra2EditorSavePlan CreatePlan(
        string filePath,
        string text,
        Ra2EditorTextEncodingMetadata metadata)
        => new(
            filePath,
            text,
            Ra2IniNewLineKind.Lf,
            Ra2EditorNewLineSavePolicy.PreserveCurrentText,
            canSave: true,
            "Save plan can be written.",
            metadata);

    private static bool HasPrefix(byte[] bytes, byte[] prefix)
    {
        if (bytes.Length < prefix.Length)
            return false;

        for (int index = 0; index < prefix.Length; index++)
        {
            if (bytes[index] != prefix[index])
                return false;
        }

        return true;
    }

    private sealed class FailingIniFileStore : RA2IniEditor.Infrastructure.IO.IIniFileStore
    {
        public string? LastText { get; private set; }

        public RA2IniEditor.Infrastructure.IO.IniTextReadResult ReadText(string path)
            => throw new NotSupportedException();

        public RA2IniEditor.Infrastructure.IO.IniTextWriteResult WriteText(string path, string text, Encoding encoding)
        {
            LastText = text;
            InvalidOperationException exception = new("simulated write failure");
            return new RA2IniEditor.Infrastructure.IO.IniTextWriteResult(false, path, exception.Message, exception);
        }
    }

    private sealed class TestWorkspace : IDisposable
    {
        private TestWorkspace(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static TestWorkspace Create()
        {
            string root = Path.Combine(Path.GetTempPath(), "RA2IniEditor.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new TestWorkspace(root);
        }

        public string PathFor(string relativePath)
        {
            string path = Path.Combine(Root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
