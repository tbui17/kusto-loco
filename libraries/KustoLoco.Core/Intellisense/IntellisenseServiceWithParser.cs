using System.Collections.Generic;

namespace KustoLoco.Core.Intellisense;

public class IntellisenseServiceWithParser(IIntellisenseService intellisenseService) : IIntellisenseService
{
    public string? GetIOQueryArgument(string lineText)
    {
        lineText = lineText.TrimStart();


        var isIoQuery = lineText.StartsWith(".save ") || lineText.StartsWith(".load ");
        if (!isIoQuery)
        {
            return null;
        }

        var path = lineText[5..].Trim();
        return path;
    }

    public IEnumerable<IntellisenseEntry> GetPathIntellisenseOptions(string lineText)
    {
        if (GetIOQueryArgument(lineText) is not { } path)
        {
            return [];
        }
        return intellisenseService.GetPathIntellisenseOptions(path);
    }
}
