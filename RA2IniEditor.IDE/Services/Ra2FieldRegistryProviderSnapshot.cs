using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Services;

/// <summary>
/// 表示一次稳定捕获的字段库 Provider 及其发布修订号。
/// </summary>
internal sealed class Ra2FieldRegistryProviderSnapshot
{
    internal Ra2FieldRegistryProviderSnapshot(IRa2FieldDefinitionProvider provider, long revision)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        if (revision <= 0)
            throw new ArgumentOutOfRangeException(nameof(revision), revision, "Revision must be positive.");

        Revision = revision;
    }

    public IRa2FieldDefinitionProvider Provider { get; }

    public long Revision { get; }
}
