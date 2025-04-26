namespace Intellisense.FileSystem;



public interface IPathWhitelist
{
    // TODO: object param
    public bool IsWhitelisted(string path);
}


public class PathExtensionWhitelist(IFileSystemReader reader) : IPathWhitelist
{
    // TODO: possible to do impl without null?
    private HashSet<string>? _extensions;

    public void SetExtensions(IEnumerable<string>? extensions)
    {
        if (extensions is null)
        {
            _extensions = null;
            return;
        }

        _extensions = CreateExtensionsSet(extensions);

    }

    private static HashSet<string> CreateExtensionsSet(IEnumerable<string> extensions)
    {
        var normalized = extensions.Select(x =>
            {
                if (x.StartsWith("."))
                {
                    return x;
                }

                // TODO: handle index edge case
                return $".{x}";

            }
        );
        return normalized.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public bool IsWhitelisted(string path)
    {
        if (_extensions is null)
        {
            return true;
        }
        var ext = reader.GetExtension(path);
        return _extensions.Contains(ext) || reader.IsDirectory(path);
    }
}