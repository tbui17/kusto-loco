﻿namespace Intellisense.FileSystem.FileCompletionResultRetrievers;

internal class RootFileCompletionResultRetriever(ICompletionResultFactory completionResultFactory)
    : IFileCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(RootedPath rootedPath)
    {
        var path = rootedPath.Value;
        if (path.EndsWith(':'))
        {
            return completionResultFactory.Create();
        }

        return completionResultFactory.Create(path);
    }
}
