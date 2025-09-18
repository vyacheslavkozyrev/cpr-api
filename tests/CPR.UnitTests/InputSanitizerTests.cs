using Xunit;
using CPR.Application.Services;
using System;

namespace CPR.UnitTests;

public class InputSanitizerTests
{
    [Theory]
    [InlineData("<script>alert('xss')</script>", "")] // Script tags are completely removed
    [InlineData("<p>Hello <b>world</b></p>", "Hello world")] // HTML tags removed, content preserved
    [InlineData("Normal text", "Normal text")]
    [InlineData("<img src='x' onerror='alert(1)'>", "")] // HTML tags removed
    [InlineData("Text with <script>bad</script> content", "Text with  content")] // Script tag removed, space preserved
    public void SanitizeText_RemovesHtmlTags(string input, string expected)
    {
        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Normal feedback content", true)]
    [InlineData("Content with <script> tags", true)] // After sanitization, this becomes valid
    [InlineData("", false)]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", false)] // Too long (1001 chars)
    [InlineData("Content with 50% special chars !@#$%^&*()", true)] // Special chars ratio is about 40%, still valid
    public void IsValidContent_ValidatesContent(string input, bool expected)
    {
        // Act
        var result = InputSanitizer.IsValidContent(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitizeText_HandlesNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => InputSanitizer.SanitizeText(null!));
    }

    [Fact]
    public void IsValidContent_HandlesNullInput()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => InputSanitizer.IsValidContent(null!));
    }

    [Fact]
    public void SanitizeText_RemovesMultipleScriptTags()
    {
        // Arrange
        var input = "<script>alert(1)</script>Normal text<script>alert(2)</script>";

        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        Assert.Equal("Normal text", result);
    }

    [Fact]
    public void SanitizeText_RemovesNestedTags()
    {
        // Arrange
        var input = "<div><script>alert('nested')</script></div>";

        // Act
        var result = InputSanitizer.SanitizeText(input);

        // Assert
        Assert.Equal("", result);
    }
}