namespace Intellisense;

internal static class PathExtensions
{
    public static bool EndsWithDirectorySeparator(this string path)
    {
        return path.Length > 0 && path[^1].IsDirectorySeparator();
    }

    public static bool IsDirectorySeparator(this char path)
    {
        return path == Path.DirectorySeparatorChar || path == Path.AltDirectorySeparatorChar;
    }
}
