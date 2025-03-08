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

    public FileSystemIntellisenseServiceTests()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["C:/File1.txt"] = new(""),
                ["C:/Folder1/File1.txt"] = new(""),
                ["C:/Folder1/File2.txt"] = new(""),
                ["C:/Folder1/MyFile1.txt"] = new(""),
                ["C:/Folder1/MyFile2.txt"] = new(""),
                ["C:/Folder1/Folder2"] = new MockDirectoryData()
            }
        );
        _fileSystemIntellisenseService = new FileSystemIntellisenseService(fileSystem);
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirDirectorySeparatorSuffix_Empty()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/NonExistentDir/").Entries;

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirNoDirectorySeparatorSuffix_Empty()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/NonExistentDir").Entries;

        results.Should().BeEmpty();
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirNoDirectorySeparatorSuffix_SingleDirectory()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1");

        result.Rewind.Should().Be("Folder1".Length);
        result.Prefix.Should().Be(string.Empty);

        result
            .Entries.Should()
            .ContainSingle()
            .Which.Name.Should()
            .Be("Folder1");
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffix_DirectoryChildren()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/");

        result.Prefix.Should().Be("/");
        result.Rewind.Should().Be(1);

        result
            .Entries
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
    public void GetPathIntellisenseOptions_ValidFilePath_SingleFile()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/File1.txt");

        result.Rewind.Should().Be("File1.txt".Length);
        result.Prefix.Should().Be(string.Empty);
        result.Entries.Should().ContainSingle().Which.Name.Should().Be("File1.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialPathInput_PathsStartingWithInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/MyF");

        result.Rewind.Should().Be(3);
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("MyFile1.txt", "MyFile2.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentPath_Empty()
    {
        var results =
            _fileSystemIntellisenseService.GetPathIntellisenseOptions("/NonExistentPath.txt").Entries;

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentRootPath_Empty()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("D:").Entries;

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonRootedPath_Empty()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("./Folder1/MyFile1.txt").Entries;

        results.Should().BeEmpty();
    }
}
