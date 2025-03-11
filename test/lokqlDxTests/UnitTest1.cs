using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Windows.Media;
using FluentAssertions;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Intellisense;
using Intellisense.FileSystem;
using NotNullStrings;
using Xunit;

namespace lokqlDxTests;

public class UnitTest1
{
    [UIFact]
    public void Test1()
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
        var fileSystemIntellisenseService = FileSystemIntellisenseServiceProvider.GetFileSystemIntellisenseService(fileSystem);

        var result = fileSystemIntellisenseService.GetPathIntellisenseOptions(RootedPath.CreateOrThrow("/Fol"));

        var textArea = new TextArea();
        var window = new CompletionWindow(textArea);

        var list = window.CompletionList;

        foreach (var entry in result.Entries)
        {
            list.CompletionData.Add(new MyCompletionData(entry, "", 0));
        }
        list.SelectItem(result.Filter);
        list.SelectedItem.Text.Should().Be("Folder1");
        var items = new MyCompletionData[list.ListBox.Items.Count];
        list.ListBox.Items.CopyTo(items,0);
        items.Select(x => x.Text).Should().BeEquivalentTo("Folder1", "Folder2");

    }
}

public class MyCompletionData(IntellisenseEntry entry, string prefix, int rewind) : ICompletionData
{
    public ImageSource? Image => null;

    public string Text => entry.Name;

    // Use this property if you want to show a fancy UIElement in the list.
    public object Content => Text;

    public object Description
        => entry.Syntax.IsBlank()
            ? entry.Description
            : $@"{entry.Description}
Usage: {entry.Syntax}";


    public double Priority => 1.0;

    public void Complete(TextArea textArea, ISegment completionSegment,
        EventArgs insertionRequestEventArgs)
    {
        var seg = new TextSegment
        {
            StartOffset = completionSegment.Offset - rewind,
            Length = completionSegment.Length + rewind
        };
        textArea.Document.Replace(seg, prefix + Text);
    }
}
