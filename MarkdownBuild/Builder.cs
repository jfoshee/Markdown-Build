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
            var html = header + Markdown.Transform(md) + footer;
            File.WriteAllText(htmlFile, html);
        }

        public void TransformFiles(string sourceDirectory, string destinationDirectory)
        {
            Directory.CreateDirectory(destinationDirectory);
            var header = GetHeader(sourceDirectory);
            foreach (var sourceFileName in Directory.EnumerateFiles(sourceDirectory))
            {
                var extension = GetExtension(sourceFileName);
                if (Options.MarkdownExtensions.Contains(extension))
                {
                    // Transform markdown files to html
                    var fileName = Path.GetFileNameWithoutExtension(sourceFileName);
                    var destFileName = Path.Combine(destinationDirectory, fileName + ".html");
                    TransformFile(sourceFileName, destFileName, String.Format(header, fileName), "</body></html>");
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
            return Path.GetExtension(sourceFileName).Substring(1);
        }

        private static void CopyToDirectory(string sourceFileName, string destinationDirectory)
        {
            var filename = Path.GetFileName(sourceFileName);
            var destFileName = Path.Combine(destinationDirectory, filename);
            File.Copy(sourceFileName, destFileName);
        }

        private string GetHeader(string sourceDirectory)
        {
            var header = new StringBuilder("<!DOCTYPE html><html><head><title>{0}</title></head><body>");
            AppendReferences(header, StyleSheets(sourceDirectory), Resources.StyleReference);
            AppendReferences(header, Scripts(sourceDirectory), Resources.JavascriptReference);
            return header.ToString();
        }

        private static void AppendReferences(StringBuilder header, IEnumerable<string> fileNames, string referenceFormat)
        {
            foreach (var fileName in fileNames)
            {
                var reference = String.Format(referenceFormat, fileName);
                header.AppendLine(reference);
            }
        }

        private IEnumerable<string> StyleSheets(string sourceDirectory)
        {
            return EnumerateFileNames(sourceDirectory, "css");
        }

        private IEnumerable<string> Scripts(string sourceDirectory)
        {
            return EnumerateFileNames(sourceDirectory, "js");
        }

        private static IEnumerable<string> EnumerateFileNames(string sourceDirectory, string extension)
        {
            return Directory
                .EnumerateFiles(sourceDirectory, "*." + extension)
                .Select(f => Path.GetFileName(f));
        }
    }
}
