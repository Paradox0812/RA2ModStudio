using System.Text;
using RA2IniEditor.IDE.Views;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class ShellDockLayoutStoreTests
{
    private const string ValidLayout = "<?xml version=\"1.0\" encoding=\"utf-8\"?><LayoutRoot />";

    [Fact]
    public void ValidLayout_RoundTripsAsUtf8WithoutBom()
    {
        WithTemporaryDirectory(directory =>
        {
            ShellDockLayoutStore store = new(directory);
            Assert.True(store.TryWrite(ValidLayout).Succeeded);
            Assert.True(store.TryRead(out string? restored).Succeeded);
            Assert.Equal(ValidLayout, restored);
            Assert.False(File.ReadAllBytes(Path.Combine(directory, ShellDockLayoutStore.LayoutFileName))
                .AsSpan().StartsWith(Encoding.UTF8.Preamble));
        });
    }

    [Fact]
    public void MissingLayout_ReturnsNotFound()
    {
        WithTemporaryDirectory(directory =>
        {
            ShellDockLayoutStore store = new(directory);
            ShellDockLayoutOperationResult result = store.TryRead(out string? serialized);
            Assert.False(result.Succeeded);
            Assert.Equal(ShellDockLayoutFailureKind.NotFound, result.FailureKind);
            Assert.Null(serialized);
        });
    }

    [Fact]
    public void OverLimitLayout_ReturnsTooLarge()
    {
        WithTemporaryDirectory(directory =>
        {
            File.WriteAllBytes(
                Path.Combine(directory, ShellDockLayoutStore.LayoutFileName),
                new byte[ShellDockLayoutStore.MaximumFileLength + 1]);
            ShellDockLayoutOperationResult result = new ShellDockLayoutStore(directory).TryRead(out _);
            Assert.Equal(ShellDockLayoutFailureKind.TooLarge, result.FailureKind);
        });
    }

    [Theory]
    [InlineData("<!DOCTYPE LayoutRoot [<!ENTITY xxe SYSTEM 'file:///c:/windows/win.ini'>]><LayoutRoot />")]
    [InlineData("<LayoutRoot>")]
    [InlineData("<UnexpectedRoot />")]
    [InlineData("<?xml version=\"1.0\" encoding=\"utf-16\"?><LayoutRoot />")]
    public void UnsafeOrMalformedXml_IsRejected(string text)
    {
        WithTemporaryDirectory(directory =>
        {
            File.WriteAllText(
                Path.Combine(directory, ShellDockLayoutStore.LayoutFileName),
                text,
                new UTF8Encoding(false));
            ShellDockLayoutOperationResult result = new ShellDockLayoutStore(directory).TryRead(out _);
            Assert.Equal(ShellDockLayoutFailureKind.UnsafeXml, result.FailureKind);
        });
    }

    [Fact]
    public void InvalidUtf8_IsRejected()
    {
        WithTemporaryDirectory(directory =>
        {
            File.WriteAllBytes(
                Path.Combine(directory, ShellDockLayoutStore.LayoutFileName),
                [0x3C, 0xFF, 0x3E]);
            ShellDockLayoutOperationResult result = new ShellDockLayoutStore(directory).TryRead(out _);
            Assert.Equal(ShellDockLayoutFailureKind.UnsafeXml, result.FailureKind);
        });
    }

    [Fact]
    public void FailedAtomicWrite_PreservesPriorValidFile()
    {
        WithTemporaryDirectory(directory =>
        {
            ShellDockLayoutStore store = new(directory);
            Assert.True(store.TryWrite(ValidLayout).Succeeded);
            string path = Path.Combine(directory, ShellDockLayoutStore.LayoutFileName);
            byte[] before = File.ReadAllBytes(path);
            ShellDockLayoutStore failingStore = new(
                directory,
                (_, _, _) => throw new IOException("simulated write failure"));

            ShellDockLayoutOperationResult result = failingStore.TryWrite("<LayoutRoot><LayoutPanel /></LayoutRoot>");

            Assert.Equal(ShellDockLayoutFailureKind.IoFailure, result.FailureKind);
            Assert.Equal(before, File.ReadAllBytes(path));
        });
    }

    [Fact]
    public void Quarantine_UsesOneBoundedDiagnosticPath()
    {
        WithTemporaryDirectory(directory =>
        {
            string layoutPath = Path.Combine(directory, ShellDockLayoutStore.LayoutFileName);
            string invalidPath = Path.Combine(directory, ShellDockLayoutStore.InvalidLayoutFileName);
            File.WriteAllText(layoutPath, "<LayoutRoot>", new UTF8Encoding(false));
            File.WriteAllText(invalidPath, "old", new UTF8Encoding(false));

            Assert.True(new ShellDockLayoutStore(directory).TryQuarantine().Succeeded);
            Assert.False(File.Exists(layoutPath));
            Assert.Equal("<LayoutRoot>", File.ReadAllText(invalidPath));
        });
    }

    private static void WithTemporaryDirectory(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), $"ra2-shell-layout-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            action(directory);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
