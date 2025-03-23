using Intellisense.FileSystem.CompletionResultRetrievers;
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
    CompletionResult GetPathIntellisenseOptions(string path);
}

internal class FileSystemIntellisenseService : IFileSystemIntellisenseService
{
    private readonly IFileSystemPathCompletionResultRetriever[] _retrievers;

    public FileSystemIntellisenseService(IFileSystemReader reader)
    {
        _retrievers =
        [
            new ChildrenCompletionResultRetriever(reader),
            new SiblingCompletionResultRetriever(reader)
        ];
    }

    public CompletionResult GetPathIntellisenseOptions(string path)
    {
        try
        {
            // Only supports rooted path at this time

            IFileSystemPath fileSystemPath = RootedPath.Create(path);

            return _retrievers
                .Select(x => x.GetCompletionResult(fileSystemPath))
                .FirstOrDefault(x => x.Entries.Count > 0,CompletionResult.Empty);
        }
        catch (IOException)
        {
            // TODO: Add error logger. If a DI container ends up being introduced, extract out new() calls.
            return CompletionResult.Empty;
        }
    }
}
