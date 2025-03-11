using System.IO.Abstractions;

namespace Intellisense.FileSystem;

internal interface ICompletionResultFactory
{
    public CompletionResult Create();
    public CompletionResult Create(string path);
    public CompletionResult Create(ParentChildPathPair parentChildPathPair);
}

internal class CompletionResultFactory(IFileSystemReader reader) : ICompletionResultFactory
{
    public CompletionResult Create()
    {
        return CompletionResult.Empty;
    }

    public CompletionResult Create(string path)
    {
        return new CompletionResult
        {
            Entries = GetChildren(path)
        };
    }

    public CompletionResult Create(ParentChildPathPair parentChildPathPair)
    {
        var children = GetChildren(parentChildPathPair.ParentPath);
        return new CompletionResult
        {
            Entries = children,
            Filter = parentChildPathPair.CurrentPath
        };
    }

    private static IntellisenseEntry CreateEntry(IFileSystemInfo fileSystemInfo)
    {
        return new IntellisenseEntry { Name = fileSystemInfo.Name };
    }

    private List<IntellisenseEntry> GetChildren(string path)
    {
        try
        {
            return reader.GetChildren(path).Select(CreateEntry).ToList();
        }
        catch (DirectoryNotFoundException e)
        {
            Console.Error.WriteLine($"The directory '{path}' was previously found, but now it it is not. {e}");
            return [];
        }
    }
}
