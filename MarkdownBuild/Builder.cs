using System.IO;
using MarkdownSharp;

namespace MarkdownBuild
{
    public class Builder
    {
        internal Markdown Markdown { get; set; }

        public Builder()
        {
            Markdown = new Markdown();
        }

        public void TransformFile(string markdownFile, string htmlFile)
        {
            var md = File.ReadAllText(markdownFile);
            var html = Markdown.Transform(md);
            File.WriteAllText(htmlFile, html);
        }
    }
}
