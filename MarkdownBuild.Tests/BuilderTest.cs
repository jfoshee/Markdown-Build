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

        [TestMethod]
        public void ShouldRecurseIntoSubdirectories()
        {
            // Arrange
            var destinationDirectory = Path.Combine(TestDirectory(), "D");
            var sourceDirectory = Path.Combine(TestDirectory(), "Src");
            var sub1 = Path.Combine(sourceDirectory, "s1", "s11");
            var sub2 = Path.Combine(sourceDirectory, "s2");
            Directory.CreateDirectory(sub1);
            Directory.CreateDirectory(sub2);
            File.WriteAllText(Path.Combine(sub1, "i.txt"), "_I_");
            File.WriteAllText(Path.Combine(sub2, "j.txt"), "_J_");

            // Act
            Subject.TransformFiles(sourceDirectory, destinationDirectory);

            // Assert
            DirectoryAssert.Exists(destinationDirectory);
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
            File.WriteAllText(Path.Combine(src, "a.foo"), "_a_");
            File.WriteAllText(Path.Combine(src, "b.bar"), "_b_");
            File.WriteAllText(Path.Combine(src, "ignore.txt"), "_ignore_");

            // Act
            Subject.TransformFiles(src, dst);

            // Assert
            TextFileAssert.Contains(Path.Combine(dst, "a.html"), "<em>a");
            TextFileAssert.Contains(Path.Combine(dst, "b.html"), "<em>b");
            TextFileAssert.Contains(Path.Combine(dst, "ignore.txt"), "_ignore_");
            TextFileAssert.AreEqual(Path.Combine(src, "ignore.txt"), Path.Combine(dst, "ignore.txt"));
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
