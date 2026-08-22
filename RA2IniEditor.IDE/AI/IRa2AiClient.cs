using System;
using System.Threading;
using System.Threading.Tasks;

namespace RA2IniEditor.IDE.AI;

internal delegate ValueTask Ra2AiContentDeltaHandler(
    string delta,
    CancellationToken cancellationToken);

internal interface IRa2AiClient
{
    Task<Ra2AiResponse> SendAsync(Ra2AiRequest request, CancellationToken cancellationToken);

    async Task<Ra2AiResponse> SendStreamingAsync(
        Ra2AiRequest request,
        Ra2AiContentDeltaHandler onContentDelta,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(onContentDelta);

        Ra2AiResponse response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccess)
            return response;

        try
        {
            await onContentDelta(response.Text, cancellationToken).ConfigureAwait(false);
            return cancellationToken.IsCancellationRequested
                ? Ra2AiResponse.CreateCancelled(response.Text, response.Diagnostics)
                : response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Ra2AiResponse.CreateCancelled(response.Text, response.Diagnostics);
        }
    }
}
