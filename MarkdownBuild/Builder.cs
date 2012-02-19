using System.IO;
using System.Linq;
using MarkdownSharp;

namespace MarkdownBuild
{
    public class Builder
    {
        public IMarkdownBuildOptions Options { get; set; }
        internal Markdown Markdown { get; set; }

        public Builder()
        {
            Markdown = new Markdown();
            Options = new DefaultMarkdownBuildOptions();
        }

        public void TransformFile(string markdownFile, string htmlFile)
        {
            TransformFile(markdownFile, htmlFile, "", "");
        }

        public void TransformFile(string markdownFile, string htmlFile, string header, string footer)
        {
            var md = File.ReadAllText(markdownFile);
            var html = header + Markdown.Transform(md) + footer;
            File.WriteAllText(htmlFile, html);
        }

        public void TransformFiles(string sourceDirectory, string destinationDirectory)
        {
            Directory.CreateDirectory(destinationDirectory);
            foreach (var sourceFileName in Directory.EnumerateFiles(sourceDirectory))
            {
                var extension = Path.GetExtension(sourceFileName).Substring(1);
                if (Options.MarkdownExtensions.Contains(extension))
                {
                    // Transform markdown files to html
                    var fileName = Path.GetFileNameWithoutExtension(sourceFileName);
                    var destFileName = Path.Combine(destinationDirectory, fileName + ".html");
                    TransformFile(sourceFileName, destFileName);
                }
                else
                {
                    // Copy non-markdown files
                    var filename = Path.GetFileName(sourceFileName);
                    var destFileName = Path.Combine(destinationDirectory, filename);
                    File.Copy(sourceFileName, destFileName);
                }
            }
            // Recurse into subdirectories
            foreach (var dir in Directory.EnumerateDirectories(sourceDirectory))
                TransformFiles(dir, Path.Combine(destinationDirectory, Path.GetFileName(dir)));
        }
    }
}
