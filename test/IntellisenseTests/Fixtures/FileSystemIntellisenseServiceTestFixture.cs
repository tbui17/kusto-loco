using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Intellisense;
using Intellisense.FileSystem;
using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.Logging.Testing;

namespace IntellisenseTests.Fixtures;



internal class FileSystemIntellisenseServiceTestFixture
{
    public readonly FileSystemIntellisenseService FileSystemIntellisenseService;
    private readonly FakeLogger<IFileSystemIntellisenseService> _logger;

    public FileSystemIntellisenseServiceTestFixture(
        Dictionary<string, MockFileData> fileData,
        MockFileSystemOptions? options = null
    )
    {
        options ??= new MockFileSystemOptions { CreateDefaultTempDir = false };
        var fileSystem = new MockFileSystem(fileData, options);
        var reader = new FileSystemReader(fileSystem);
        _logger = new FakeLogger<IFileSystemIntellisenseService>();
        FileSystemIntellisenseService = new FileSystemIntellisenseService(reader, _logger, new RootedPathFactory());
    }

    public FileSystemIntellisenseServiceTestFixture(IFileSystemReader reader)
    {
        _logger = new FakeLogger<IFileSystemIntellisenseService>();
        FileSystemIntellisenseService = new FileSystemIntellisenseService(reader, _logger, new RootedPathFactory());
    }

    public IReadOnlyList<FakeLogRecord> GetLogs() => _logger.Collector.GetSnapshot();

    public CompletionResult GetPathIntellisenseOptions(string path) => FileSystemIntellisenseService.GetPathIntellisenseOptions(path);
}
