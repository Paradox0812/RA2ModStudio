using System.Threading.Channels;

namespace RA2IniEditor.AssetHost;

internal sealed class Ra2GenerationProgressDispatcher : IAsyncDisposable
{
    private readonly IProgress<Ra2GenerationProgress>? _observer;
    private readonly Channel<Ra2GenerationProgress> _channel;
    private readonly Task _pump;

    internal Ra2GenerationProgressDispatcher(IProgress<Ra2GenerationProgress>? observer)
    {
        _observer = observer;
        _channel = Channel.CreateBounded<Ra2GenerationProgress>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false
        });
        _pump = PumpAsync();
    }

    internal void Publish(Ra2GenerationProgress progress)
    {
        _channel.Writer.TryWrite(progress);
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();
        await _pump.ConfigureAwait(false);
    }

    private async Task PumpAsync()
    {
        if (_observer is null)
        {
            await foreach (Ra2GenerationProgress _ in _channel.Reader.ReadAllAsync().ConfigureAwait(false))
            {
            }

            return;
        }

        DateTimeOffset lastDelivery = DateTimeOffset.MinValue;
        await foreach (Ra2GenerationProgress progress in _channel.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            TimeSpan remaining = TimeSpan.FromMilliseconds(100) - (DateTimeOffset.UtcNow - lastDelivery);
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining).ConfigureAwait(false);
            }

            try
            {
                _observer.Report(progress);
            }
            catch
            {
                // Presentation observers cannot control or fail the provider process.
            }

            lastDelivery = DateTimeOffset.UtcNow;
        }
    }
}
