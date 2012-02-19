using System.Collections.Generic;

namespace MarkdownBuild
{
    public interface IMarkdownBuildOptions
    {
        /// <summary>
        /// List of extensions for markdown files that should be transformed.
        /// </summary>
        IEnumerable<string> MarkdownExtensions { get; }
    }
}
