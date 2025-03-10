using Intellisense.FileSystem.FileCompletionResultRetrievers;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    CompletionResult GetPathIntellisenseOptions(RootedPath rootedPath);
}

public class FileSystemIntellisenseService(IFileSystemReader reader) : IFileSystemIntellisenseService
{
    private FileCompletionResultRetriever CreateRetriever(RootedPath rootedPath)
    {
        var path = rootedPath.Value;
        if (reader.IsRoot(path))
        {
            return new RootFileCompletionResultRetriever(reader);
        }

        if (reader.IsDirectory(path))
        {
            return new DirFileCompletionResultRetriever(reader);
        }

        return new PartialMatchFileCompletionResultRetriever(reader);
    }

    public CompletionResult GetPathIntellisenseOptions(RootedPath rootedPath)
    {
        return CreateRetriever(rootedPath).GetCompletionResult(rootedPath.Value);
    }
}
