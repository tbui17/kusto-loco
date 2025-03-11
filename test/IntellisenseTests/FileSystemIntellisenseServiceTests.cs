using System;
using System.Collections.Generic;
using System.IO.Abstractions;
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
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/NonExistentDir/"));

        result.GetFilteredEntries().Should().BeEmpty();

    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirNoDirectorySeparatorSuffix_Empty()
    {
        var result =
            _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/NonExistentDir"));

        result.GetFilteredEntries().Should().BeEmpty();
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirNoDirectorySeparator_OnlyDir()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/Folder1"));


        result
            .GetFilteredEntries().Select(x => x.Name)
            .Should()
            .BeEquivalentTo("Folder1");
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffix_Children()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/Folder1/"));

        result
            .GetFilteredEntries()
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
    public void
        GetPathIntellisenseOptions_RootWithDirSeparator_Children()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("C:/"));

        result
            .GetFilteredEntries()
            .Select(x => x.Name)
            .Should()
            .BeEquivalentTo("File1.txt", "File2.txt", "Folder1", "Folder2");
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_RootWithColon_Empty()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("C:"));
        result.GetFilteredEntries().Should().BeEmpty();
    }


    [Fact]
    public void GetPathIntellisenseOptions_PartialFileInput_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/Folder1/MyF"));

        result.GetFilteredEntries().Select(x => x.Name).Should().BeEquivalentTo("MyFile1.txt", "MyFile2.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialDirInputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("C:/Fol"));

        result.GetFilteredEntries().Select(x => x.Name).Should().BeEquivalentTo("Folder1", "Folder2");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialFileInputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("C:/Fil"));

        result.GetFilteredEntries().Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "File2.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_InputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("C:/F"));

        result.GetFilteredEntries().Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "File2.txt", "Folder1", "Folder2");
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentPath_Empty()
    {
        var result =
            _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/NonExistentPath.txt"));

        result.GetFilteredEntries().Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentRootPath_Empty()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("D:"));

        result.GetFilteredEntries().Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentRootPathAtRoot_Empty()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("D:/Abc"));

        result.GetFilteredEntries().Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_RootedRelativePath_RelativeChildren()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/Folder1/Folder2/Folder3/Folder4/../../"));

        result
            .GetFilteredEntries()
            .Select(x => x.Name)
            .Should()
            .BeEquivalentTo("File5.txt", "Folder3");
    }
}

public class FileSystemIntellisenseServiceIntegrationTests
{
    private readonly IFileSystemIntellisenseService _fileSystemIntellisenseService;

    public FileSystemIntellisenseServiceIntegrationTests()
    {
        _fileSystemIntellisenseService = FileSystemIntellisenseServiceProvider.GetFileSystemIntellisenseService(new FileSystem());
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffixAtRoot_HasChild()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/"));

        result
            .GetFilteredEntries()
            .Take(1)
            .Should()
            .NotBeEmpty();
    }
}

file static class CompletionResultExtensions
{
    public static IEnumerable<IntellisenseEntry> GetFilteredEntries(this CompletionResult completionResult)
    {
        // AvalonEdit.CompletionList.SelectItems filters by entri
        return completionResult.Entries.Where(x =>
            x.Name.StartsWith(completionResult.Filter, StringComparison.OrdinalIgnoreCase)
        );
    }
}
