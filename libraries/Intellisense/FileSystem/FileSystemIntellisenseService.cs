using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    CompletionResult GetPathIntellisenseOptions(string path);
}

public class FileSystemIntellisenseService(IFileSystem fileSystem) : IFileSystemIntellisenseService
{
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        IgnoreInaccessible = true
    };

    public CompletionResult GetPathIntellisenseOptions(string path)
    {
        if (!fileSystem.Path.IsPathRooted(path))
        {
            return new CompletionResult();
        }

        if (fileSystem.Directory.Exists(path))
        {
            if (path.EndsWith(':'))
            {
                return new CompletionResult();
            }
            if (!fileSystem.Path.EndsInDirectorySeparator(path))
            {
                return CreateSingleEntryCompletionResult(path);
            }

            var result = GetOptionsFromFileSystem(path);

            return new CompletionResult
            {
                Entries = result
            };
        }

        if (fileSystem.Path.GetDirectoryName(path) is not { } dirPath)
        {
            return new CompletionResult();
        }

        if (!fileSystem.Directory.Exists(dirPath))
        {
            return new CompletionResult();
        }

        var fileName = fileSystem.Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return new CompletionResult();
        }
        var entries = GetOptionsFromFileSystem(dirPath).Where(x => x.Name.StartsWith(fileName));

        return new CompletionResult
        {
            Entries = entries,
            Rewind = fileName.Length
        };
    }

    private CompletionResult CreateSingleEntryCompletionResult(string path)
    {
        var fileName = fileSystem.Path.GetFileName(path);
        return new CompletionResult
        {
            Entries = [new IntellisenseEntry {Name = fileName}],
            Rewind = fileName.Length
        };
    }

    private IEnumerable<IntellisenseEntry> GetOptionsFromFileSystem(string dirPath)
    {
        return fileSystem
            .DirectoryInfo.New(dirPath)
            .EnumerateFileSystemInfos("*", EnumerationOptions)
            .Select(x => new IntellisenseEntry { Name = x.Name });
    }
}
