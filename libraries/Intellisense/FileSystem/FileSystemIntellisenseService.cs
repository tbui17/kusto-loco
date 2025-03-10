using Intellisense.FileSystem.FileCompletionResultRetrievers;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    CompletionResult GetPathIntellisenseOptions(RootedPath rootedPath);
}

public class FileSystemIntellisenseService(IFileSystemReader reader, ICompletionResultFactory completionResultFactory) : IFileSystemIntellisenseService
{
    private FileCompletionResultRetriever CreateRetriever(RootedPath rootedPath)
    {
        var path = rootedPath.Value;
        if (reader.IsRoot(path))
        {
            return new RootFileCompletionResultRetriever(completionResultFactory);
        }

        if (reader.IsDirectory(path))
        {
            return new DirectoryCompletionResultRetriever(completionResultFactory);
        }

        return new PartialMatchFileCompletionResultRetriever(reader,completionResultFactory);
    }

    public CompletionResult GetPathIntellisenseOptions(RootedPath rootedPath)
    {
        return CreateRetriever(rootedPath).GetCompletionResult(rootedPath.Value);
    }
}
