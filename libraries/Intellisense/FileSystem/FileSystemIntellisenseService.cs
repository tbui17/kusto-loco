﻿using System.IO.Abstractions;

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
            var fileName0 = fileSystem.Path.GetFileName(path);
            return new CompletionResult
            {
                Entries = [new IntellisenseEntry {Name = fileName0}],
                Rewind = fileName0.Length
            };
        }

        if (fileSystem.Directory.Exists(path))
        {
            if (!fileSystem.Path.EndsInDirectorySeparator(path))
            {
                var fileName0 = fileSystem.Path.GetFileName(path);
                return new CompletionResult
                {
                    Entries = [new IntellisenseEntry {Name = fileName0}],
                    Rewind = fileName0.Length
                };
            }

            var result = GetOptionsFromFileSystem(path);

            return new CompletionResult
            {
                Entries = result,
                Prefix = path[^1].ToString(),
                Rewind = 1
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

        var fileName = fileSystem.Path.GetFileName(path);
        var entries = GetOptionsFromFileSystem(dirPath).Where(x => x.Name.StartsWith(fileName));

        return new CompletionResult
        {
            Entries = entries,
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
