using FluentAssertions;
using Intellisense.FileSystem;
using Xunit;

namespace IntellisenseTests;


public class FileIoCommandParserTests
{
    private readonly FileIoCommandParser _parser = new();

    [Fact]
    public void LastArgumentIsRootedPath_QuotedRootedPath_True()
    {
        const string text = """
                            .load "/myFile.txt"
                            """;
        var command = _parser.Parse(text);
        command.Should().NotBeNull();
    }

    [Fact]
    public void Parse_QuotedRootedPath_True()
    {
        const string text = """
                            .load "/myFile.txt"
                            """;
        var command = _parser.Parse(text);
        command.Should().NotBeNull();
    }

    [InlineData(".load /myFile.txt", "/myFile.txt")]
    [InlineData(".save /myFil", "/myFil")]
    [InlineData(".load -f /myFile.txt", "/myFile.txt")]
    [InlineData(".save -abc /myFile", "/myFile")]
    [InlineData(".save C:/myFile", "C:/myFile")]
    [InlineData(".load E:/myF-ile.txt", "E:/myF-ile.txt")]
    [InlineData(".load /myF--ile.txt", "/myF--ile.txt")]
    [InlineData("""
                .load -f "/.load -f myFile.txt"
                """, "/.load -f myFile.txt")]
    [InlineData("""
                .load /asjdio /admsd q "asdj" asdq /abcd
                """, "/abcd")]
    [Theory]
    public void LastArgumentIsRootedPath_ValidCases_True(string text, string expected)
    {
        var command = _parser.Parse(text);
        command.Should().Be(expected);
    }

    [InlineData(".loada /myFile.txt")]
    [InlineData(".load /myFile.txt -f")]
    [InlineData(".load /myFile.txt --f")]
    [InlineData(".Load /myFile.txt")]
    [Theory]
    public void LastArgumentIsRootedPath_InvalidCases_False(string text)
    {
        var command = _parser.Parse(text);
        command.Should().BeNull();
    }
}
