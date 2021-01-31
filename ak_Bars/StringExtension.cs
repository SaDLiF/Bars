using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ak_Bars
{
    public static class StringExtension
    {
        public static string GetLastLink(this string str)
        {
            List<string> link = new List<string>();
            string parentDirectory = Path.GetDirectoryName(str);
            var regex = Regex.Replace(parentDirectory, @"\\", "/");
            if (regex == "disk:")
                regex = "/";
            return regex;
        }
    }
}
