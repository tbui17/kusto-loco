using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Intellisense.FileSystem.Paths;
using Vanara.PInvoke;

namespace Intellisense.FileSystem.Shares;

internal interface IHostReader
{
    public Task<IEnumerable<string>> GetHostsAsync();
}

internal interface IDriveShareUnion
{
    public string Drive { get; }
}

internal class DriveHostShareInfo(NetApiUseInfo info) : IDriveShareUnion
{
    public string Drive => info.Drive;
}

internal class DriveHost(UncPath path, NetApiUseInfo info) : IDriveShareUnion
{
    public string Drive => info.Drive;
    public string Host => path.Host;
}

public readonly record struct NetApiUseInfo(string Drive, string RemoteUncPath);

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal class Win32ApiHostReader(IPathFactory pathFactory) : IHostReader
{
    private static IEnumerable<NetApiUseInfo> GetRemoteHosts() => NetApi32
        .NetUseEnum<NetApi32.USE_INFO_0>()
        .Select(x => new NetApiUseInfo(x.ui0_local, x.ui0_remote));

    private IEnumerable<IDriveShareUnion> GetShareItems(IEnumerable<NetApiUseInfo> results) =>
        results.Select(IDriveShareUnion (x) =>
            {
                if (pathFactory.Create(x.RemoteUncPath) is UncPath p)
                {
                    var info = new DriveHost(p, x);
                    return info;
                }

                return new DriveHostShareInfo(x);
            }
        );

    public async Task<IEnumerable<string>> GetHostsAsync()
    {
        await Task.CompletedTask;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return [];
        }

        return GetShareItems(GetRemoteHosts()).OfType<DriveHost>().Select(x => x.Host);
    }
}
