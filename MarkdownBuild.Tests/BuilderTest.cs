using System.IO;
using MarkdownSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestDrivenDesign;

namespace MarkdownBuild.Tests
{
    [TestClass]
    public class BuilderTest : TestBase<Builder>
    {
        [TestMethod]
        public void ShouldHaveMarkdownInstance()
        {
            Markdown markdown = Subject.Markdown;
            Assert.IsNotNull(markdown);
        }

        [TestMethod]
        public void ShouldTransformFileToHtml()
        {
            // Arrange
            var directory = TestDirectory();
            var markdownFile = Path.Combine(directory, "test.md");
            var htmlFile = Path.Combine(directory, "test.html");
            File.WriteAllText(markdownFile, "## Test");

            // Act
            Subject.TransformFile(markdownFile, htmlFile);

            // Assert
            TextFileAssert.Contains(htmlFile, "<h2>Test");
        }
    }
}
