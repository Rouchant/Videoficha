namespace Videoficha;

// StringExtensions.cs
using System;
using System.Collections.Generic;

public static class StringExtensions
{
    public static IEnumerable<string> SplitLines(this string source)
    {
        return source.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
    }
}
