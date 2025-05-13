using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Vanara.PInvoke;

namespace Intellisense.FileSystem.Shares;

internal partial class Win32ApiShareReader(
    ILogger<Win32ApiShareReader> logger,
    IHostRepository hostRepository,
    CancellationTokenSource cts,
    IConnectionVerifier verifier
)
    : IShareReader, IDisposable
{
    private readonly CancellationToken _token = cts.Token;

    public async Task<IEnumerable<string>> GetSharesAsync(string host)
    {
        using var _ = logger.BeginScope(new()
            {
                [nameof(host)] = host
            }
        );

        // win32 NetShareEnum is synchronous and will stall when querying nonexistent addresses
        // so we check first by pinging (which is cancellable)
        // if this takes a long time it probably doesn't exist
        var shouldContinue = await verifier.CanConnectAsync(host);

        if (!shouldContinue)
        {
            return [];
        }


        List<NetApi32.SHARE_INFO_1> shares;

        try
        {
            shares = await AccessResource(host);
        }
        catch (UnauthorizedAccessException e)
        {
            logger.LogInformation(e, "Access denied while fetching shares.");
            return [];
        }
        catch (FileNotFoundException e)
        {
            logger.LogDebug(e, "Could not find network path.");
            return [];
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Unexpected exception while fetching shares.");
            throw;
        }

        if (shares.Count > 0)
        {
            logger.LogDebug("Found hosts with shares. {@Hosts}", shares);
            // we want hosts with shares for completion
            await hostRepository.AddAsync(host);
        }
        else
        {
            logger.LogDebug("Found no shares.");
        }

        return shares.Select(x => x.shi1_netname);
    }
}

internal partial class Win32ApiShareReader
{
    private const int MaxSessions = 2;
    private readonly SemaphoreSlim _semaphore = new(MaxSessions);

    private async Task<List<NetApi32.SHARE_INFO_1>> AccessResource(string host)
    {
        // we can still stall with even when the host exists (firewall? privileges?)
        // we can try ignoring the thread if it doesn't complete within the timeout
        // but this might exhaust the thread pool, so we'll manage usage with a semaphore


        logger.LogTrace("Attempting to acquire lock. {CurrentCount}", _semaphore.CurrentCount);

        try
        {
            await _semaphore.WaitAsync(_token);
        }
        catch (OperationCanceledException e)
        {
            logger.LogTrace(e, "Resource busy. Cannot accept additional requests.");
            throw;
        }

        logger.LogTrace("Acquired lock.");

        // ReSharper disable once MethodSupportsCancellation
        var shareTask = Task.Run(() => GetNetworkSharesAndRelease(host));
        var timeoutTask = _token.Delay();
        var firstTask = await Task.WhenAny(timeoutTask, shareTask);

        if (firstTask == timeoutTask)
        {
            logger.LogDebug("Timed out while fetching shares.");
            return [];
        }

        return await shareTask;
    }

    private List<NetApi32.SHARE_INFO_1> GetNetworkSharesAndRelease(string host)
    {
        logger.LogDebug("Fetching shares from Win32 API for host.");
        try
        {
            return EnumerateShares(host).ToList();
        }
        finally
        {
            var previousCount = _semaphore.Release();
            logger.LogTrace("Released lock. {PreviousLockCount}", previousCount);
        }
    }

    public void Dispose() => _semaphore.Dispose();
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal partial class Win32ApiShareReader
{
    private static IEnumerable<NetApi32.SHARE_INFO_1> EnumerateShares(string host) =>
        NetApi32.NetShareEnum<NetApi32.SHARE_INFO_1>(host);
}

file static class TokenExtensions
{
    public static async Task Delay(this CancellationToken token, TimeSpan? pollTime = null)
    {
        if (!token.CanBeCanceled)
        {
            return;
        }

        var time = pollTime ?? TimeSpan.FromMilliseconds(100);

        while (!token.IsCancellationRequested)
        {
            // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016
            await Task.Delay(time);
#pragma warning restore CA2016
        }
    }
}
