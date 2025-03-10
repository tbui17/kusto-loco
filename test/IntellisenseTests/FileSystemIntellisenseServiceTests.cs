using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using Intellisense;
using Intellisense.FileSystem;
using Xunit;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceTests
{
    private readonly IFileSystemIntellisenseService _fileSystemIntellisenseService;

    public FileSystemIntellisenseServiceTests()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["C:/File1.txt"] = new(""),
                ["C:/File2.txt"] = new(""),
                ["C:/Folder1/File1.txt"] = new(""),
                ["C:/Folder1/File2.txt"] = new(""),
                ["C:/Folder1/MyFile1.txt"] = new(""),
                ["C:/Folder1/MyFile2.txt"] = new(""),
                ["C:/Folder1/Folder2/Folder3/Folder4"] = new MockDirectoryData(),
                ["C:/Folder1/Folder2/File5.txt"] = new(""),
                ["C:/Folder2/File1.txt"] = new("")
            },
            new MockFileSystemOptions
            {
                CreateDefaultTempDir = false
            }
        );
        _fileSystemIntellisenseService = FileSystemIntellisenseServiceProvider.GetFileSystemIntellisenseService(fileSystem);
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirDirectorySeparatorSuffix_Empty()
    {
        var result =  _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/NonExistentDir/"));
        result.Should().BeOfType<EmptyCompletionResult>();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirNoDirectorySeparatorSuffix_Empty()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/NonExistentDir"));
        result.Should().BeOfType<FilterCompletionResult>();
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirNoDirectorySeparator_SiblingPathsMatchingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/Folder1"));
        result.Should().BeOfType<FilterCompletionResult>();
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffix_DirectoryChildren()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/Folder1/"));
        result.Should().BeOfType<BasicCompletionResult>();
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffixAtRoot_DirectoryChildren()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("C:/"));
        result.Should().BeOfType<BasicCompletionResult>();
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirNoDirectorySeparatorSuffixAtRoot_Empty()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("C:"));
        result.Should().BeOfType<EmptyCompletionResult>();
    }

    [Fact]
    public void GetPathIntellisenseOptions_ExclusiveValidFilePath_Siblings()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/Folder1/MyFile1.txt"));
        result.Should().BeOfType<FilterCompletionResult>();
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialFileInput_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/Folder1/MyF"));

        result.Should().BeOfType<FilterCompletionResult>();
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialDirInputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("C:/Fol"));

        result.Should().BeOfType<FilterCompletionResult>();
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialFileInputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("C:/Fil"));

        result.Should().BeOfType<FilterCompletionResult>();
    }

    [Fact]
    public void GetPathIntellisenseOptions_InputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("C:/F"));

        result.Should().BeOfType<FilterCompletionResult>();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentPath_Empty()
    {
        var result =
            _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/NonExistentPath.txt"));
        result.Should().BeOfType<FilterCompletionResult>();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentRootPath_Empty()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("D:"));

        result.Should().BeOfType<EmptyCompletionResult>();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentRootPathAtRoot_Empty()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("D:/Abc"));

        result.Should().BeOfType<EmptyCompletionResult>();
    }

    [Fact]
    public void GetPathIntellisenseOptions_RootedRelativePath_RelativeChildren()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/Folder1/Folder2/Folder3/Folder4/../../"));
        result.Should().BeOfType<BasicCompletionResult>();
    }
}

public class FileSystemIntellisenseServiceIntegrationTests
{
    private readonly IFileSystemIntellisenseService _fileSystemIntellisenseService;

    public FileSystemIntellisenseServiceIntegrationTests()
    {
        _fileSystemIntellisenseService = FileSystemIntellisenseServiceProvider.GetFileSystemIntellisenseService();
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffixAtRoot_HasChild()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/"));

        result.Prefix.Should().Be("");
        result.Rewind.Should().Be(0);

        result
            .Entries
            .Take(1)
            .Should()
            .NotBeEmpty();
    }
}
