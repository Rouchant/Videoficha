using System;
using System.Collections.Generic;

namespace Videoficha.Shared
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitLines(this string source)
        {
            return source.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
