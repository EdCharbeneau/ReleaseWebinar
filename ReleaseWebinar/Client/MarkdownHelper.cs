using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReleaseWebinar.Client
{
    public static class MarkdownHelper
    {
        public static string ToMarkdown(this string value) => new ReverseMarkdown.Converter().Convert(value);
        public static string ToHtml(this string value) => new MarkdownSharp.Markdown().Transform(value);
    }

}
