using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using Intellisense.FileSystem;
using Xunit;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceTests
{
    private readonly FileSystemIntellisenseService _fileSystemIntellisenseService;
    private readonly MockFileSystem _fileSystem;

    public FileSystemIntellisenseServiceTests()
    {
        _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["C:/File1.txt"] = new(""),
                ["C:/Folder1/File1.txt"] = new(""),
                ["C:/Folder1/File2.txt"] = new(""),
                ["C:/Folder1/MyFile1.txt"] = new(""),
                ["C:/Folder1/MyFile2.txt"] = new(""),
                ["C:/Folder1/Folder2"] = new MockDirectoryData()
            }
        );
        _fileSystemIntellisenseService = new FileSystemIntellisenseService(_fileSystem);
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirNoDirectorySeparatorSuffix_ReturnsDirectoryChildrenWithDirectorySeparator()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1").Entries;

        results
            .Select(x => x.Name)
            .Should()
            .BeEquivalentTo(
                $"{_fileSystem.Path.DirectorySeparatorChar}File1.txt",
                $"{_fileSystem.Path.DirectorySeparatorChar}File2.txt",
                $"{_fileSystem.Path.DirectorySeparatorChar}MyFile1.txt",
                $"{_fileSystem.Path.DirectorySeparatorChar}MyFile2.txt",
                $"{_fileSystem.Path.DirectorySeparatorChar}Folder2"
            );
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirDirectorySeparatorSuffix_ReturnsEmptyCollection()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/NonExistentDir/").Entries;


        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirNoDirectorySeparatorSuffix_ReturnsEmptyCollection()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/NonExistentDir").Entries;


        results.Should().BeEmpty();
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffix_ReturnsDirectoryChildrenWithoutDirectorySeparator()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/").Entries;

        results
            .Select(x => x.Name)
            .Should()
            .BeEquivalentTo(
                "File1.txt",
                "File2.txt",
                "MyFile1.txt",
                "MyFile2.txt",
                "Folder2"
            );
    }

    [Fact]
    public void GetPathIntellisenseOptions_ValidFilePath_ReturnsEmptyCollection()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/File1.Txt").Entries;


        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialPathInput_ReturnsPathsStartingWithInput()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/MyF").Entries;


        results.Select(x => x.Name).Should().BeEquivalentTo("MyFile1.txt", "MyFile2.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentPath_ReturnsEmptyCollection()
    {
        var results =
            _fileSystemIntellisenseService.GetPathIntellisenseOptions("/8a759e74-d7d3-4618-99d5-b917e0a8a605").Entries;

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentRootPath_ReturnsEmptyCollection()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("D:").Entries;

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonRootedPath_ReturnsEmptyCollection()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("./Folder1/MyFile1.txt").Entries;

        results.Should().BeEmpty();
    }
}
