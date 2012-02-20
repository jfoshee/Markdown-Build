# Markdown Build

**Markdown Build** is a tool for building a tree 
of Markdown files. It takes two parameters:  
Source Directory and
Destination Directory. 
Files are recursively copied from the source to the definition transforming any markdown files to html along the way.

### Markdown Transformed
Files with known Markdown extensions are transformed to html and written to the output directory. 
Default extensions are txt, md, mkdn and markdown.
So, for example, index.md becomes index.html in the output directory.

### Wrapped in Standard HTML tags
The markdown html output is wrapped in standard html and body tags. Head and title tags are also provided. 
The title is taken from the name of the Markdown file without the extension.

### Other Files Copied
Non-markdown files are simply copied to the destination without being changed.
Typically these would be images, stylesheets, javascripts, etc.

### Recursive
The build is recursive to all children of the source directory. 
The directory structure is duplicated at the destination. 
Thus, relative paths are maintained.

### Script and Stylesheet References
Javascript (.js) and Cascading Stylesheet (.css) files are automatically referenced by all Markdown files in the same directory. 

### Partials
Markdown files named with a leading underscore (e.g. "\_header.md") are treated as partials. 
That means they are transformed to html then injected into all non-partial markdown files in the same directory.
When a partial is injected into the html output, it is wrapped in a div tag.
The id of the div tag is the partial file name without the leading underscore or extension. 
Thus you may style a partial (e.g. \#header).
Partial files alone are not written to the output directory.
That is, you would not see \_header.html.














