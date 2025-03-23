using Intellisense.FileSystem.CompletionResultRetrievers;

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
    private readonly IPathCompletionResultRetriever[] _retrievers;

    public FileSystemIntellisenseService(IFileSystemReader reader)
    {
        IFileSystemPathCompletionResultRetriever[] rootedPathRetrievers =
        [
            new RootChildrenRootedPathCompletionResultRetriever(reader),
            new ChildrenRootedPathCompletionResultRetriever(reader),
            new SiblingRootedPathCompletionResultRetriever(reader)
        ];

        _retrievers =
        [
            new RootedPathCompletionResultRetriever(rootedPathRetrievers)
        ];
    }

    public CompletionResult GetPathIntellisenseOptions(string path)
    {
        try
        {
            return _retrievers
                .Select(x => x.GetCompletionResult(path))
                .FirstOrDefault(x => x.Entries.Count > 0,CompletionResult.Empty);
        }
        catch (IOException)
        {
            // TODO: Add error logger. If a DI container ends up being introduced, extract out new() calls.
            return CompletionResult.Empty;
        }
    }
}
