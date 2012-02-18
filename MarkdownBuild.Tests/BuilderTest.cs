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
            File.WriteAllText(markdownFile, "## Test");
            var htmlFile = Path.Combine(directory, "test.html");

            // Act
            Subject.TransformFile(markdownFile, htmlFile);

            // Assert
            TextFileAssert.Contains(htmlFile, "<h2>Test");
        }

        [TestMethod]
        public void ShouldTransformFileWithHeaderAndFooter()
        {
            // Arrange
            var directory = TestDirectory();
            var markdownFile = Path.Combine(directory, "test.md");
            File.WriteAllText(markdownFile, "### Content");
            var htmlFile = Path.Combine(directory, "test.html");
            var header = "~~ Header! ~~";
            var footer = "@@ Footer! @@";

            // Act
            Subject.TransformFile(markdownFile, htmlFile, header, footer);

            // Assert
            TextFileAssert.StartsWith(htmlFile, header);
            TextFileAssert.Contains(htmlFile, "<h3>Content");
            TextFileAssert.EndsWith(htmlFile, footer);
        }

        [TestMethod]
        public void ShouldTransformAllFilesInDirectory()
        {
            // Arrange
            var destinationDirectory = Path.Combine(TestDirectory(), "Dest");
            var sourceDirectory = Path.Combine(TestDirectory(), "Source");
            Directory.CreateDirectory(sourceDirectory);
            File.WriteAllText(Path.Combine(sourceDirectory, "a.md"), "_A_");
            File.WriteAllText(Path.Combine(sourceDirectory, "b.md"), "_B_");

            // Act
            Subject.TransformFiles(sourceDirectory, destinationDirectory);

            // Assert
            DirectoryAssert.Exists(destinationDirectory);
            TextFileAssert.Contains(Path.Combine(destinationDirectory, "a.html"), "<em>A</em>");
            TextFileAssert.Contains(Path.Combine(destinationDirectory, "b.html"), "<em>B</em>");
        }

        // TODO: Recursive
        // TODO: _header and _footer
        // TODO: Just copy non markdown files
        // TODO: Reference Style.css

        //[TestMethod]
        //public void ShouldInjectStyleReferenceIfCssFileFound()
        //{
        //    // Arrange
        //    var directory = TestDirectory();
        //    var stylePath = Path.Combine(directory, "style.css");
        //    File.WriteAllText(stylePath, @"p{ font-family:""Times New Roman""; }");
        //    var markdownFile = Path.Combine(directory, "foo.md");
        //    File.WriteAllText(markdownFile, "## Test");
        //    var htmlFile = Path.Combine(directory, "bar.html");

        //    // Act
        //    Subject.TransformFile(markdownFile, htmlFile);

        //    // Assert
        //    TextFileAssert.Contains(htmlFile, @"<link rel=""stylesheet"" type=""text/css"" href=""style.css"" />");
        //}
    }
}
