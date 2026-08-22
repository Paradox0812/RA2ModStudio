using System.Text;

namespace RA2IniEditor.IDE.AI;

/// <summary>按接收顺序保存单次 AI 请求的累计文本和尚未呈现的增量文本。</summary>
internal sealed class Ra2AiIncrementalTextBuffer
{
    private readonly object _syncRoot = new();
    private readonly StringBuilder _pendingText = new();
    private readonly StringBuilder _accumulatedText = new();

    public int PendingCharacterCount
    {
        get
        {
            lock (_syncRoot)
                return _pendingText.Length;
        }
    }

    public void Append(string delta)
    {
        ArgumentNullException.ThrowIfNull(delta);
        if (delta.Length == 0)
            return;

        lock (_syncRoot)
        {
            _pendingText.Append(delta);
            _accumulatedText.Append(delta);
        }
    }

    public string DrainPending()
    {
        lock (_syncRoot)
        {
            if (_pendingText.Length == 0)
                return string.Empty;

            string pending = _pendingText.ToString();
            _pendingText.Clear();
            return pending;
        }
    }

    public string GetAccumulatedText()
    {
        lock (_syncRoot)
            return _accumulatedText.ToString();
    }

    public bool AccumulatedTextEquals(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        lock (_syncRoot)
            return string.Equals(_accumulatedText.ToString(), text, StringComparison.Ordinal);
    }
}
