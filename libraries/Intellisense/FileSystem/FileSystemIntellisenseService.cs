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

        if (fileSystem.File.Exists(path))
        {
            return new CompletionResult();
        }

        if (fileSystem.Directory.Exists(path))
        {
            var result = fileSystem.DirectoryInfo.New(path).EnumerateFileSystemInfos("*", EnumerationOptions);
            if (fileSystem.Path.EndsInDirectorySeparator(path))
            {
                return new CompletionResult
                {
                    Entries = result.Select(x => new IntellisenseEntry { Name = x.Name })
                };

            }

            return new CompletionResult
            {
                Entries = result.Select(x => new IntellisenseEntry { Name = $"{fileSystem.Path.DirectorySeparatorChar}{x.Name}" })
            };
        }


        // partial or invalid paths

        if (fileSystem.Path.GetDirectoryName(path) is not { } dirPath)
        {
            return new CompletionResult();
        }

        if (!fileSystem.Directory.Exists(dirPath))
        {
            return new CompletionResult();
        }

        var dir = fileSystem.DirectoryInfo.New(dirPath);
        var fileName = fileSystem.Path.TrimEndingDirectorySeparator(fileSystem.Path.GetFileName(path));
        var entries = dir
            .EnumerateFileSystemInfos("*", EnumerationOptions)
            .Where(x => x.Name.StartsWith(fileName))
            .Select(x => new IntellisenseEntry { Name = x.Name });

        return new CompletionResult
        {
            Entries = entries
        };
    }
}
