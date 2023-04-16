using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Blog {
    public delegate Microsoft.AspNetCore.Html.IHtmlContent RazorBlock(object _);

    public static class Utils {
        public static bool RenderTrackers;

        public static string MakeUrlAbsolute(string url) {
            if(String.IsNullOrEmpty(url) || Regex.IsMatch(url, @"^\w+\:"))
                return url;
            return new Uri(new Uri(BlogInfo.AbsoluteUrl), url).ToString();
        }

        public static void EnsureParentDir(string path) {
            var dir = Path.GetDirectoryName(path);
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public static string DosToUnix(string text) {
            return text.Replace("\r", "");
        }

    }
}
