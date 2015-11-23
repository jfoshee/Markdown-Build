using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var html =
                header + Environment.NewLine +
                Markdown.Transform(md) +
                Environment.NewLine + footer;
            File.WriteAllText(htmlFile, html, Encoding.UTF8);
        }

        public void TransformFiles(string sourceDirectory, string destinationDirectory)
        {
            Directory.CreateDirectory(destinationDirectory);
            var header = GetHeader(sourceDirectory);
            var footer = GetFooter(sourceDirectory);
            foreach (var sourceFileName in Directory.EnumerateFiles(sourceDirectory))
            {
                var extension = GetExtension(sourceFileName);
                if (Options.MarkdownExtensions.Contains(extension))
                {
                    // Transform markdown files to html
                    var fileName = Path.GetFileNameWithoutExtension(sourceFileName);
                    if (fileName.StartsWith(Resources.PartialPrefix))
                        continue;
                    var destFileName = Path.Combine(destinationDirectory, fileName + ".html");
                    TransformFile(sourceFileName, destFileName, String.Format(header, fileName), footer);
                }
                else
                    // Copy non-markdown files
                    CopyToDirectory(sourceFileName, destinationDirectory);
            }
            // Recurse into subdirectories
            foreach (var dir in Directory.EnumerateDirectories(sourceDirectory))
                TransformFiles(dir, Path.Combine(destinationDirectory, Path.GetFileName(dir)));
        }

        /// <summary>
        /// Gets extension without leading period
        /// </summary>
        private static string GetExtension(string sourceFileName)
        {
            var extension = Path.GetExtension(sourceFileName) ?? String.Empty;
            if (extension.StartsWith("."))
                return extension.Substring(1);
            return String.Empty;
        }

        private static void CopyToDirectory(string sourceFileName, string destinationDirectory)
        {
            var filename = Path.GetFileName(sourceFileName);
            var destFileName = Path.Combine(destinationDirectory, filename);
            File.Copy(sourceFileName, destFileName, overwrite: true);
        }

        private static string GetHeader(string sourceDirectory)
        {
            var header = new StringBuilder(Resources.HtmlHeader);
            AppendReferences(header, StyleSheets(sourceDirectory), Resources.StyleReference);
            return header.ToString();
        }

        private string GetFooter(string sourceDirectory)
        {
            var footer = new StringBuilder("");
            AppendReferences(footer, Scripts(sourceDirectory), Resources.JavascriptReference);
            IncludePartials(footer, Partials(sourceDirectory));
            footer.AppendLine("</body></html>");
            return footer.ToString();
        }

        private void IncludePartials(StringBuilder footer, IEnumerable<string> partials)
        {
            foreach (var partial in partials)
            {
                var md = File.ReadAllText(partial);
                var id = Path.GetFileNameWithoutExtension(partial).Substring(1);
                var html = String.Format(Resources.PartialBlock, id, Markdown.Transform(md).TrimEnd());
                footer.AppendLine(html);
            }
        }

        private static void AppendReferences(StringBuilder header, IEnumerable<string> fileNames, string referenceFormat)
        {
            header.AppendLine();
            foreach (var fileName in fileNames)
            {
                var reference = String.Format(referenceFormat, fileName);
                header.AppendLine(reference);
            }
        }

        private static IEnumerable<string> Partials(string sourceDirectory)
        {
            return Directory
                .EnumerateFiles(sourceDirectory, Resources.PartialPrefix + "*");
        }

        private static IEnumerable<string> StyleSheets(string sourceDirectory)
        {
            return EnumerateFileNames(sourceDirectory, "css");
        }

        private static IEnumerable<string> Scripts(string sourceDirectory)
        {
            return EnumerateFileNames(sourceDirectory, "js");
        }

        private static IEnumerable<string> EnumerateFileNames(string sourceDirectory, string extension)
        {
            var fullSourcePath = Path.GetFullPath(sourceDirectory);
            return Directory
                .EnumerateFiles(fullSourcePath, "*." + extension, SearchOption.AllDirectories)
                .Select(f => f
                    .Replace(fullSourcePath, string.Empty)
                    .Replace(Path.DirectorySeparatorChar, '/')
                    .Substring(1) // Remove leading slash
                )
                .OrderBy(path => !path.Contains("bower"))
                .ThenBy(path => path);
        }
    }
}
