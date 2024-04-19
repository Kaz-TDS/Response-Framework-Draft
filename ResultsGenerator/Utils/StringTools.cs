using System;
using System.Globalization;

namespace TDS.ResultsGenerator.Utils
{
    public static class StringTools
    {
        public static string ToPascalCase(string input)
        {
            var output = input.ToLower().Replace("_", " ");
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            output = info.ToTitleCase(output).Replace(" ", string.Empty);
            return output;
        }
    }
}