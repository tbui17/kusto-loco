using Avalonia.Headless.XUnit;
using Avalonia.Input;
using AwesomeAssertions;
using lokqlDxComponentsTests.Fixtures;

namespace lokqlDxComponentsTests;

public class AutocompletionTest
{
    [AvaloniaFact]
    public async Task PrependTextWorks()
    {
        var f = new AutocompletionTestFixture();

        f.ResourceProvider.KqlOperatorEntries = [
            new("summarize")
        ];



        await f.Editor.TextArea.Type("traffic |sum");

        await f.CompletionWindow.ShouldEventuallySatisfy(x =>
            x.IsOpen.Should().BeTrue()
        );

        await f.Editor.TextArea.Press(PhysicalKey.Enter);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be("traffic | summarize"));
    }

    [AvaloniaFact]
    public async Task RemovePrefixWorks()
    {
        var f = new AutocompletionTestFixture();

        f.ResourceProvider.KqlFunctionEntries = [
            new("setting1")
        ];



        await f.Editor.TextArea.Type("?sett");

        await f.CompletionWindow.ShouldEventuallySatisfy(x =>
            x.IsOpen.Should().BeTrue()
        );

        await f.Editor.TextArea.Press(PhysicalKey.Enter);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be("setting1"));
    }
    
    [AvaloniaFact]
    public async Task NonMatchWindowNotOpen()
    {
        var f = new AutocompletionTestFixture();

        f.ResourceProvider.InternalCommands = [new("abc"), new("def")];


        await f.Editor.TextArea.Type(".xyz");

        await f.CompletionWindow.ShouldEventuallySatisfy(x => x.IsOpen.Should().BeFalse());
    }

    [AvaloniaTheory]
    [InlineData(".ab", new[] { "abc" })]
    [InlineData(".de", new[] { "def" })]
    public async Task PartialMatchFiltersResults(string input, string[] expected)
    {
        var f = new AutocompletionTestFixture();

        f.ResourceProvider.InternalCommands = [new("abc"), new("def")];


        await f.Editor.TextArea.Type(input);

        await f.CompletionWindow.ShouldEventuallySatisfy(c => c
            .GetCurrentCompletionListEntries()
            .Select(x => x.Text)
            .Should()
            .BeEquivalentTo(expected)
        );
    }
    
    [AvaloniaFact]
    public async Task PathsWork()
    {
        var f = new AutocompletionTestFixture();
        var data = new List<string>
        {
            "/folder1",
            "/file1.txt"
        };
        var expected = data.Select(x => x[1..]);
        f.SetFileSystemData(data);

        f.ResourceProvider._intellisenseClient.AddInternalCommands([
                new()
                {
                    Name = "load",
                    SupportedExtensions = [],
                    SupportsFiles = true
                }
            ]
        );



        await f.Editor.TextArea.Type(".load /");

        await f.CompletionWindow.ShouldEventuallySatisfy(x =>
            x.GetCurrentCompletionListEntries().Select(e => e.Text).Should().BeEquivalentTo(expected)
        );
    }

}
