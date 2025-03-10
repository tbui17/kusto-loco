namespace Intellisense.FileSystem.FileCompletionResultRetrievers;

internal abstract class FileCompletionResultRetriever
{
    internal abstract CompletionResult GetCompletionResult(string path);
}
