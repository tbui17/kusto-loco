using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    IEnumerable<IntellisenseEntry> GetPathIntellisenseOptions(string path);
}

public class FileSystemIntellisenseService(IFileSystem fileSystem) : IFileSystemIntellisenseService
{

    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        IgnoreInaccessible = true
    };

    public IEnumerable<IntellisenseEntry> GetPathIntellisenseOptions(string path)
    {
        if (!fileSystem.Path.IsPathRooted(path))
        {
            return [];
        }

        if (fileSystem.File.Exists(path))
        {
            return [];
        }

        if (fileSystem.Directory.Exists(path))
        {
            var result = fileSystem.DirectoryInfo.New(path).EnumerateFileSystemInfos("*", EnumerationOptions);
            if (fileSystem.Path.EndsInDirectorySeparator(path))
            {
                return result.Select(x => new IntellisenseEntry { Name = x.Name });
            }

            return result.Select(x => new IntellisenseEntry { Name = $"/{x.Name}" });
        }


        // partial or invalid paths

        var fileName = fileSystem.Path.TrimEndingDirectorySeparator(fileSystem.Path.GetFileName(path));
        var dir = fileSystem.DirectoryInfo.New(fileSystem.Path.GetDirectoryName(path)!);
        var entries = dir
            .EnumerateFileSystemInfos("*", EnumerationOptions)
            .Where(x => x.Name.StartsWith(fileName))
            .Select(x => new IntellisenseEntry { Name = x.Name });

        return entries;
    }
}
