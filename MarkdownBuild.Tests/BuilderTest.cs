using System.Collections.Generic;
using System.IO;
using System.Linq;
using MarkdownSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
        public void ShouldHaveDefaultOptions()
        {
            // Arrange
            IMarkdownBuildOptions options = Subject.Options;

            // Assert
            Assert.IsNotNull(options);
            var extensions = options.MarkdownExtensions.ToList();
            CollectionAssert.Contains(extensions, "txt");
            CollectionAssert.Contains(extensions, "md");
            CollectionAssert.Contains(extensions, "mkdn");
            CollectionAssert.Contains(extensions, "markdown");
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
            var sourceDirectory = TestSubdirectory("Source");
            WriteText(sourceDirectory, "a.md", "_A_");
            WriteText(sourceDirectory, "b.md", "_B_");
            var destinationDirectory = Path.Combine(TestDirectory(), "Dest");

            // Act
            Subject.TransformFiles(sourceDirectory, destinationDirectory);

            // Assert
            DirectoryAssert.Exists(destinationDirectory);
            AssertFileContains(destinationDirectory, "a.html", "<em>A</em>");
            AssertFileContains(destinationDirectory, "b.html", "<em>B</em>");
        }

        [TestMethod]
        public void ShouldRecurseIntoSubdirectories()
        {
            // Arrange
            var sourceDirectory = TestSubdirectory("Src");
            var sub1 = Path.Combine(sourceDirectory, "s1", "s11");
            var sub2 = Path.Combine(sourceDirectory, "s2");
            Directory.CreateDirectory(sub1);
            Directory.CreateDirectory(sub2);
            WriteText(sub1, "i.txt", "_I_");
            WriteText(sub2, "j.txt", "_J_");
            var destinationDirectory = TestSubdirectory("Dest");

            // Act
            Subject.TransformFiles(sourceDirectory, destinationDirectory);

            // Assert
            TextFileAssert.Contains(Path.Combine(destinationDirectory, "s1", "s11", "i.html"), "<em>I</em>");
            TextFileAssert.Contains(Path.Combine(destinationDirectory, "s2", "j.html"), "<em>J</em>");
        }

        [TestMethod]
        public void ShouldOnlyCopyNonMarkdownFilesGivenExtensions()
        {
            // Arrange
            var mockOptions = new Mock<IMarkdownBuildOptions>();
            mockOptions.SetReturnsDefault<IEnumerable<string>>(new string[] { "foo", "bar" });
            Subject.Options = mockOptions.Object;
            var src = TestSubdirectory("S");
            var dst = TestSubdirectory("D");
            WriteText(src, "a.foo", "_a_");
            WriteText(src, "b.bar", "_b_");
            var justCopyFileName = "ignore.txt";
            var expected = "_not transformed_";
            WriteText(src, justCopyFileName, expected);

            // Act
            Subject.TransformFiles(src, dst);

            // Assert
            AssertFileContains(dst, "a.html", "<em>a");
            AssertFileContains(dst, "b.html", "<em>b");
            AssertFileContains(dst, justCopyFileName, expected);
            TextFileAssert.AreEqual(Path.Combine(src, justCopyFileName), Path.Combine(dst, justCopyFileName));
        }

        private void AssertFileContains(string dst, string fileName, string text)
        {
            TextFileAssert.Contains(Path.Combine(dst, fileName), text);
        }

        private void WriteText(string directory, string fileName, string text)
        {
            File.WriteAllText(Path.Combine(directory, fileName), text);
        }

        private string TestSubdirectory(string subdirectoryName)
        {
            var src = Path.Combine(TestDirectory(), subdirectoryName);
            Directory.CreateDirectory(src);
            return src;
        }

        // TODO: Reference .css files
        // TODO: Reference .js files
        // TODO: _header and _footer

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
