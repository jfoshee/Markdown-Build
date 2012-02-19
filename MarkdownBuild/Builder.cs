using System.IO;
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
            foreach (var dir in Directory.EnumerateDirectories(sourceDirectory))
                TransformFiles(dir, Path.Combine(destinationDirectory, Path.GetFileName(dir)));
            foreach (var md in Directory.EnumerateFiles(sourceDirectory))
            {
                var filename = Path.GetFileNameWithoutExtension(md);
                var html = Path.Combine(destinationDirectory, filename + ".html");
                TransformFile(md, html);
            }
        }
    }
}
