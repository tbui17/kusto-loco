using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using Intellisense;
using Intellisense.FileSystem;
using Xunit;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceTests
{
    private readonly FileSystemIntellisenseServiceAdaptor _fileSystemIntellisenseServiceAdaptor;

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
        var fileSystemIntellisenseService =
            FileSystemIntellisenseServiceProvider.GetFileSystemIntellisenseService(fileSystem);
        _fileSystemIntellisenseServiceAdaptor = new FileSystemIntellisenseServiceAdaptor(fileSystemIntellisenseService);
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirDirectorySeparatorSuffix_Empty()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("/NonExistentDir/");
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirNoDirectorySeparatorSuffix_Empty()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("/NonExistentDir");
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffix_DirectoryChildren()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("/Folder1/");
        result
            .Should()
            .BeEquivalentTo("File1.txt",
                "File2.txt",
                "MyFile1.txt",
                "MyFile2.txt",
                "Folder2"
            );
    }

    [Fact]
    public void GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffixAtRoot_DirectoryChildren()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("C:/");
        result
            .Should()
            .BeEquivalentTo("File1.txt",
                "File2.txt",
                "Folder1",
                "Folder2"
            );
    }

    [Fact]
    public void GetPathIntellisenseOptions_ValidDirNoDirectorySeparatorSuffixAtRoot_Empty()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("C:");
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_ExclusiveValidFilePath_Siblings()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("/Folder1/MyFile1.txt");
        result.Should().BeEquivalentTo("MyFile1.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialFileInput_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("/Folder1/MyF");
        result.Should().BeEquivalentTo("MyFile1.txt", "MyFile2.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialDirInputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("C:/Fol");
        result.Should().BeEquivalentTo("Folder1", "Folder2");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialFileInputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("C:/Fil");
        result.Should().BeEquivalentTo("File1.txt", "File2.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_InputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("C:/F");
        result
            .Should()
            .BeEquivalentTo("File1.txt",
                "File2.txt",
                "Folder1",
                "Folder2"
            );
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentPath_Empty()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("/NonExistentPath.txt");
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentRootPath_Empty()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("D:");
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentRootPathAtRoot_Empty()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("D:/Abc");
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_RootedRelativePath_RelativeChildren()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("/Folder1/Folder2/Folder3/Folder4/../../");
        result.Should().BeEquivalentTo("File5.txt", "Folder3");
    }

    [Fact]
    public void A()
    {
        Path.GetFileName("/").Should().BeEmpty();
        Path.GetFileName("C:/Abc/").Should().BeEmpty();
        Path.GetFileName("C:/").Should().BeEmpty();
    }
}

public class FileSystemIntellisenseServiceIntegrationTests
{
    private readonly FileSystemIntellisenseServiceAdaptor _fileSystemIntellisenseServiceAdaptor;

    public FileSystemIntellisenseServiceIntegrationTests()
    {
        _fileSystemIntellisenseServiceAdaptor =
            new FileSystemIntellisenseServiceAdaptor(FileSystemIntellisenseServiceProvider
                .GetFileSystemIntellisenseService()
            );
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffixAtRoot_HasChild()
    {
        var result = _fileSystemIntellisenseServiceAdaptor.Execute("/");

        result.Take(1).Should().NotBeEmpty();
    }
}

internal class FileSystemIntellisenseServiceAdaptor(IFileSystemIntellisenseService service)
{
    public IEnumerable<string> Execute(string path)
    {
        var result = service.GetPathIntellisenseOptions(RootedPath.CreateOrThrow(path));
        var entries = result.Entries.Select(x => x.Name);
        if (result is FilterCompletionResult f)
        {
            return entries.Where(x => x.Contains(f.Filter, StringComparison.OrdinalIgnoreCase));
        }

        return entries;
    }
}
