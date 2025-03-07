namespace Intellisense.FileSystem.FileCompletionResultRetrievers;

internal interface IFileCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(RootedPath rootedPath);
}
