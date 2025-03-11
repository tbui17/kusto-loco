using Intellisense.FileSystem.FileCompletionResultRetrievers;

namespace Intellisense.FileSystem;

/// <summary>
/// Retrieves completion results from a given file system path.
/// </summary>
public interface IFileSystemIntellisenseService
{
    CompletionResult GetPathIntellisenseOptions(RootedPath rootedPath);
}

internal class FileSystemIntellisenseService(IFileSystemReader reader, ICompletionResultFactory completionResultFactory) : IFileSystemIntellisenseService
{

    private readonly RootDirectoryCompletionResultRetriever _rootDirectoryCompletionResultRetriever = new(completionResultFactory);
    private readonly DirectoryCompletionResultRetriever _directoryCompletionResultRetriever = new(completionResultFactory);
    private readonly RootedPathCompletionResultRetriever _rootedPathCompletionResultRetriever = new(reader, completionResultFactory);

    public CompletionResult GetPathIntellisenseOptions(RootedPath rootedPath)
    {
        return GetRetriever(rootedPath).GetCompletionResult(rootedPath);
    }

    private IFileCompletionResultRetriever GetRetriever(RootedPath rootedPath)
    {
        var path = rootedPath.Value;
        if (reader.IsRoot(path))
        {
            return _rootDirectoryCompletionResultRetriever;
        }

        if (reader.IsDirectory(path))
        {
            return _directoryCompletionResultRetriever;
        }

        return _rootedPathCompletionResultRetriever;
    }
}
