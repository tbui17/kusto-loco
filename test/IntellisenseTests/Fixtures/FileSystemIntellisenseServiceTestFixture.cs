﻿using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Intellisense;
using Intellisense.FileSystem;
using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.Logging.Testing;

namespace IntellisenseTests.Fixtures;

internal class FileSystemIntellisenseServiceTestFixture
{
    private readonly FileSystemIntellisenseService _fileSystemIntellisenseService;
    private readonly FakeLogger<IFileSystemIntellisenseService> _logger;

    public FileSystemIntellisenseServiceTestFixture(
        Dictionary<string, MockFileData> fileData,
        MockFileSystemOptions? options = null
    )
    {
        fileData = fileData.ToDictionary(
            x => MockUnixSupport.Path(x.Key),
            x => x.Value
        );
        options ??= new MockFileSystemOptions { CreateDefaultTempDir = false };
        var fileSystem = new MockFileSystem(fileData, options);
        var reader = new FileSystemReader(fileSystem);
        _logger = new FakeLogger<IFileSystemIntellisenseService>();
        _fileSystemIntellisenseService = new FileSystemIntellisenseService(reader, _logger, new RootedPathFactory());
    }

    public FileSystemIntellisenseServiceTestFixture(IFileSystemReader reader)
    {
        _logger = new FakeLogger<IFileSystemIntellisenseService>();
        _fileSystemIntellisenseService = new FileSystemIntellisenseService(reader, _logger, new RootedPathFactory());
    }

    public IReadOnlyList<FakeLogRecord> GetLogs() => _logger.Collector.GetSnapshot();

    public CompletionResult GetPathIntellisenseOptions(string path)
    {
        path = MockUnixSupport.Path(path);
        return _fileSystemIntellisenseService.GetPathIntellisenseOptions(path);
    }
}
