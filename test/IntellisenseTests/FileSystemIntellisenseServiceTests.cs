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
        });
        _fileSystemIntellisenseService = new FileSystemIntellisenseService(fileSystem);
    }

    [Fact]
    public void GetPathIntellisenseOptions_ValidDirNoDirectorySeparatorSuffix_ReturnsDirectoryChildrenWithDirectorySeparator()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1");


        results.Select(x => x.Name).Should().BeEquivalentTo("/File1.txt","/File2.txt","/MyFile1.txt","/MyFile2.txt","/Folder2");
    }

    [Fact]
    public void GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffix_ReturnsDirectoryChildrenWithoutDirectorySeparator()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/");


        results.Select(x => x.Name).Should().BeEquivalentTo("File1.txt","File2.txt","MyFile1.txt","MyFile2.txt","Folder2");
    }

    [Fact]
    public void GetPathIntellisenseOptions_ValidFilePath_ReturnsEmptyCollection()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/File1.Txt");


        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialPathInput_ReturnsPathsStartingWithInput()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/MyF");


        results.Select(x => x.Name).Should().BeEquivalentTo("MyFile1.txt","MyFile2.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentPath_ReturnsEmptyCollection()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/8a759e74-d7d3-4618-99d5-b917e0a8a605");

        results.Should().BeEmpty();
    }


}
