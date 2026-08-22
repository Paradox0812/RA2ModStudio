namespace RA2IniEditor.Application.Diagnostics;

internal static class Ra2DiagnosticLimitGuard
{
    public static void ThrowIfAdditionExceeds(int currentCount, int maximumResultItems)
    {
        if (maximumResultItems < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumResultItems));

        if (currentCount >= maximumResultItems)
            throw new Ra2DiagnosticResultLimitExceededException();
    }
}

internal sealed class Ra2DiagnosticResultLimitExceededException : Exception
{
}
