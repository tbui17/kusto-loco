using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace KustoLoco.Core.Intellisense;

public interface IIntellisenseService
{
    IEnumerable<IntellisenseEntry> GetPathIntellisenseOptions(string path);
}

public class IntellisenseService(IFileSystem fileSystem) : IIntellisenseService
{

    public virtual IEnumerable<IntellisenseEntry> GetPathIntellisenseOptions(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            return [];
        }

        if (fileSystem.File.Exists(path))
        {
            return [];
        }

        var enumerationOptions = new EnumerationOptions
        {
            IgnoreInaccessible = true
        };

        if (fileSystem.Directory.Exists(path))
        {
            var result = fileSystem.DirectoryInfo.New(path).EnumerateFileSystemInfos("*", enumerationOptions);
            if (Path.EndsInDirectorySeparator(path))
            {
                return result.Select(x => new IntellisenseEntry { Name = $"{x.Name}" });
            }

            return result.Select(x => new IntellisenseEntry { Name = $"/{x.Name}" });
        }


        // partial or invalid paths

        var fileName = Path.TrimEndingDirectorySeparator(Path.GetFileName(path));
        var dir = fileSystem.DirectoryInfo.New(Path.GetDirectoryName(path)!);
        var entries = dir
            .EnumerateFileSystemInfos("*", enumerationOptions)
            .Where(x => x.Name.StartsWith(fileName))
            .Select(x => new IntellisenseEntry { Name = x.Name });

        return entries;
    }
}
