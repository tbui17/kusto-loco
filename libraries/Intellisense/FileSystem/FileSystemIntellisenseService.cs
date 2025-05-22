using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    /// <summary>
    /// Retrieves intellisense completion results from a given path.
    /// </summary>
    /// <returns>
    /// An empty completion result if the path is invalid, does not exist, or does not have any children.
    /// </returns>
    Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path);
}

internal class FileSystemIntellisenseService(
    IPathFactory pathFactory,
    IEnumerable<IFileSystemPathCompletionResultRetriever> retrievers
)
    : IFileSystemIntellisenseService
{
    public async Task<CompletionResult> GetPathIntellisenseOptionsAsync(
        string path
    )
    {
        var pathObj = pathFactory.Create(path);

        var result = await retrievers
            .Select(x => x.GetCompletionResultAsync(pathObj))
            .WhenEach()
            .Where(x => !x.IsEmpty())
            .FirstOrDefaultAsync();

        return result ?? CompletionResult.Empty;
    }
}

file static class AsyncExtensions
{
    public static async IAsyncEnumerable<T> WhenEach<T>(this IEnumerable<Task<T>> tasks)
    {
        var enumerator = Task.WhenEach(tasks);

        await foreach (var item in enumerator)
        {
            yield return await item;

        }
    }
}
