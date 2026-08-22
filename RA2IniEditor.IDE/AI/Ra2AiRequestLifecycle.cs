using System.Threading;

namespace RA2IniEditor.IDE.AI;

/// <summary>管理 AI 面板中唯一活动请求的身份、取消和完成顺序。</summary>
internal sealed class Ra2AiRequestLifecycle
{
    private readonly object _syncRoot = new();
    private Ra2AiRequestSession? _currentSession;

    public bool IsActive
    {
        get
        {
            lock (_syncRoot)
                return _currentSession is not null;
        }
    }

    public bool TryStart(out Ra2AiRequestSession? session)
    {
        lock (_syncRoot)
        {
            if (_currentSession is not null)
            {
                session = null;
                return false;
            }

            session = new Ra2AiRequestSession();
            _currentSession = session;
            return true;
        }
    }

    public bool TryCancelCurrent()
    {
        Ra2AiRequestSession? session;
        lock (_syncRoot)
            session = _currentSession;

        return session?.TryCancel() == true;
    }

    public bool TryComplete(Ra2AiRequestSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        lock (_syncRoot)
        {
            if (!ReferenceEquals(_currentSession, session))
                return false;

            _currentSession = null;
            return true;
        }
    }
}

/// <summary>拥有单次 AI 请求的取消源；由请求发起方在 finally 中释放。</summary>
internal sealed class Ra2AiRequestSession : IDisposable
{
    private readonly CancellationTokenSource _cancellationSource = new();
    private readonly CancellationToken _token;
    private int _cancellationRequested;
    private int _disposed;

    public Ra2AiRequestSession()
    {
        _token = _cancellationSource.Token;
    }

    public CancellationToken Token => _token;

    public bool IsCancellationRequested => _token.IsCancellationRequested;

    public void Cancel()
        => TryCancel();

    internal bool TryCancel()
    {
        if (Interlocked.CompareExchange(ref _cancellationRequested, 1, 0) != 0)
            return false;

        if (Volatile.Read(ref _disposed) != 0)
            return false;

        try
        {
            _cancellationSource.Cancel();
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        _cancellationSource.Dispose();
    }
}
