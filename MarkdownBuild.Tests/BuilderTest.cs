using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
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

        [TestMethod]
        public void ShouldAddReferencesToCssFiles()
        {
            StringAssert.Contains(Resources.StyleReference, "<link");
            VerifyReferencesAddedToFilesOfExtension("css", Resources.StyleReference, "p {}");
        }

        [TestMethod]
        public void ShouldAddReferencesToJsFiles()
        {
            StringAssert.StartsWith(Resources.JavascriptReference, "<script");
            StringAssert.EndsWith(Resources.JavascriptReference, "</script>", "The closing script tag is required");
            VerifyReferencesAddedToFilesOfExtension("js", Resources.JavascriptReference, "alert('1');");
        }

        private void VerifyReferencesAddedToFilesOfExtension(string extension, string reference, string content)
        {
            // Arrange & Act
            var dst = SetupAndTransform(src =>
            {
                WriteText(src, "file1." + extension, content);
                WriteText(src, "file2." + extension, content);
                WriteText(src, "a.md", "");
            });

            // Assert
            var file = Path.Combine(dst, "a.html");
            TestContext.AddResultFile(file);
            TextFileAssert.Contains(file, String.Format(reference, "file1." + extension));
            TextFileAssert.Contains(file, String.Format(reference, "file2." + extension));
        }

        [TestMethod]
        public void ShouldAddHtmlHeadAndBodyTags()
        {
            // Arrange & Act
            var destination = SetupAndTransform(src => WriteText(src, "a.md", "foo"));

            // Assert
            var file = Path.Combine(destination, "a.html");
            TestContext.AddResultFile(file);
            TextFileAssert.StartsWith(file, "<!DOCTYPE html><html><head><title>a</title></head><meta charset=\"UTF-8\"><body>");
            TextFileAssert.Contains(file, "foo");
            TextFileAssert.EndsWith(file, "</body></html>");
        }

        [TestMethod]
        public void ShouldBeEncodedAsUtf8()
        {
            // Arrange
            var directory = TestDirectory();
            var markdownFile = Path.Combine(directory, "test.md");
            File.WriteAllText(markdownFile, "Test");
            var htmlFile = Path.Combine(directory, "test.html");

            // Act
            Subject.TransformFile(markdownFile, htmlFile);

            // Assert
            var byteOrderMark = File.ReadAllBytes(htmlFile).Take(3);
            byteOrderMark.Should().Equal(0xEF, 0xBB, 0xBF);
        }

        [TestMethod]
        public void ShouldGetTitleFromFileName()
        {
            // Arrange & Act
            var destination = SetupAndTransform(src => WriteText(src, "MyExpectedTitle.md", "foo"));

            // Assert
            AssertFileContains(destination, "MyExpectedTitle.html", "<title>MyExpectedTitle</title>");
        }

        //[TestMethod]
        //public void ShouldGetTitleFromFirstH1()
        //{
        //    // Arrange & Act
        //    var destination = SetupAndTransform(src => WriteText(src, "abc.md", "foo # Expected Title \n bar"));

        //    // Assert
        //    AssertFileContains(destination, "abc.html", "<title>Expected Title</title>");
        //}

        #region Partials

        [TestMethod]
        public void ShouldIncludeAllPartialsInAllPagesInDirectory()
        {
            // Arrange & Act
            var dest = SetupAndTransform(src =>
                {
                    WriteText(src, "_pA.md", "## p1");
                    WriteText(src, "_pB.md", "## p2");
                    WriteText(src, "a.md", "a");
                    WriteText(src, "b.md", "b");
                });

            // Assert
            AssertFileContains(dest, "a.html", "<h2>p1");
            AssertFileContains(dest, "a.html", "<h2>p2");
            AssertFileContains(dest, "b.html", "<h2>p1");
            AssertFileContains(dest, "b.html", "<h2>p2");
        }

        [TestMethod]
        public void PartialsShouldBeWrappedInDivWithIdFromFileName()
        {
            // Arrange & Act
            var dest = SetupAndTransform(src =>
            {
                WriteText(src, "_Expected.md", "## p1");
                WriteText(src, "a.md", "a");
            });

            // Assert
            AssertFileContains(dest, "a.html", "<div id=\"Expected\"><h2>p1</h2></div>");
        }

        [TestMethod]
        public void PartialsShouldNotBeInOutputDirectory()
        {
            // Arrange & Act
            var dest = SetupAndTransform(src => WriteText(src, "_part.md", "bla"));

            // Assert
            Assert.IsFalse(File.Exists(Path.Combine(dest, "_part.html")));
        }

        // Partials should be at the end
        // Partials must have a markdown extension

        #endregion

        [TestMethod, DeploymentItem(@"MarkdownBuild.Tests\Example", "Example")]
        public void Example()
        {
            Subject.TransformFiles("Example", TestDirectory());
            var indexPath = Path.Combine(TestDirectory(), "index.html");
            Console.WriteLine(indexPath);
            //System.Diagnostics.Process.Start(indexPath); // Uncomment to open page in browser
        }

        private string SetupAndTransform(Action<string> actOnSourceDirectory)
        {
            // Arrange
            var src = TestSubdirectory("S");
            var dst = TestSubdirectory("D");
            actOnSourceDirectory(src);

            // Act
            Subject.TransformFiles(src, dst);

            // Assert
            return dst;
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

        // TODO: Should styles be in head tag?
        // TODO: Other css or js extensions? Or case-insensitive?
        // TODO: Ignore white space in tests
    }
}
