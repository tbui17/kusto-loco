namespace Intellisense.FileSystem;

public class FileSystemIntellisenseServiceWithParser(IFileSystemIntellisenseService fileSystemIntellisenseService) : IFileSystemIntellisenseService
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

    public CompletionResult GetPathIntellisenseOptions(string lineText)
    {
        if (GetIOQueryArgument(lineText) is not { } path)
        {
            return CompletionResult.Empty;
        }
        return fileSystemIntellisenseService.GetPathIntellisenseOptions(path);
    }
}
