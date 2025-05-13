using Intellisense.Concurrency;

namespace Intellisense;

public class IntellisenseClient(IIntellisenseService intellisenseService)
{
    private readonly ExclusiveRequestSession _session = new();

    public async Task<CompletionResult> GetCompletionResultAsync(string input)
    {
        try
        {
            return await _session.RunAsync(token => intellisenseService.GetCompletionResultAsync(input, token));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new IntellisenseException(e);
        }

    }

    public async Task CancelRequestAsync() => await _session.CancelRequestAsync();
}
