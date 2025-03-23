﻿using Intellisense.FileSystem.CompletionResultRetrievers;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    CompletionResult GetPathIntellisenseOptions(string path);
}

internal class FileSystemIntellisenseService : IFileSystemIntellisenseService
{
    private readonly IPathCompletionResultRetriever[] _retrievers;

    public FileSystemIntellisenseService(IFileSystemReader reader)
    {
        IRootedPathCompletionResultRetriever[] rootedPathRetrievers =
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
                .FirstOrDefault(x => x is not null) ?? CompletionResult.Empty;
        }
        catch (IOException)
        {
            // TODO: Add error logger. If a DI container ends up being introduced, extract out new() calls.
            return CompletionResult.Empty;
        }
    }
}
