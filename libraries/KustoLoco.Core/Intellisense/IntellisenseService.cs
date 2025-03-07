using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KustoLoco.Core.Intellisense;

public interface IIntellisenseService
{
    string? ParseRootedPath(string lineText);
    IList<IntellisenseEntry> GetPathIntellisenseOptions(string path);
}

public class IntellisenseService : IIntellisenseService
{

    public string? ParseRootedPath(string lineText)
    {
        lineText = lineText.TrimStart();

        var isIoQuery = lineText.StartsWith(".save ") || lineText.StartsWith(".load ");
        if (!isIoQuery)
        {
            return null;
        }

        var path = lineText[5..].TrimStart();

        if (!Path.IsPathRooted(path))
        {
            return null;
        }

        return path;
    }

    public IList<IntellisenseEntry> GetPathIntellisenseOptions(string path)
    {
        if (!Directory.Exists(path))
        {
            return [];
        }

        var entries = new DirectoryInfo(path)
            .GetFileSystemInfos()
            .Select(x => new IntellisenseEntry{Name = x.Name})
            .ToList();

        return entries;
    }


}
