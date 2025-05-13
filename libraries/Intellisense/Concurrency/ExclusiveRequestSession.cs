namespace Intellisense.Concurrency;


internal class ExclusiveRequestSession
{
    private CancellationTokenSource _cts = new();

    /// <summary>
    /// Ensures that only the response of the most recent request is returned.
    /// </summary>
    public async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> fn)
    {
        var cts = Cancel();
        var result = await fn(cts.Token);
        cts.Token.ThrowIfCancellationRequested();
        return result;
    }

    private CancellationTokenSource Cancel()
    {
        var cts = new CancellationTokenSource();
        var prevCts = Interlocked.Exchange(ref _cts, cts);
        prevCts.Cancel();
        return cts;
    }

    public async Task CancelRequestAsync() => await _cts.CancelAsync();
}