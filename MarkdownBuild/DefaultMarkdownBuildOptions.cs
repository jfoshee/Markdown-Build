using System.Collections.Generic;

namespace MarkdownBuild
{
    class DefaultMarkdownBuildOptions : IMarkdownBuildOptions
    {
        public IEnumerable<string> MarkdownExtensions
        {
            get { return new string[] { "txt", "md", "mkdn", "markdown" }; }
        }
    }
}
