using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Blog {

    public class EntryPath {
        public readonly DateTime Date;
        public readonly int LegacyID;
        public readonly string SlugText;

        public EntryPath(string path) {
            Console.WriteLine($"{nameof(EntryPath)}({path})");

            var parsed = TryParseJupyterPath(path, out Date, out SlugText);

            if(!parsed)
                parsed = TryParseMarkdownPath(path, out Date, out LegacyID, out SlugText);

            if(!parsed)
                throw new Exception();
        }

        public string DateText {
            get { return Date.ToString("dd.MM.yyyy"); }
        }

        public string Url {
            get { return $"{BlogInfo.PathPrefix}/{SlugText}/"; }
        }

        static bool TryParseMarkdownPath(string path, out DateTime date, out int legacyID, out string slugText) {
            var match = Regex.Match(Path.GetFileName(path), @"^(\d{8})\.(?:(\d{2,4})\.)?(.+?)\.md$");
            if(match.Success) {
                date = ParseDate(match.Groups[1].Value);
                legacyID = ParseLegacyID(match.Groups[2].Value);
                slugText = match.Groups[3].Value;
                return true;
            } else {
                date = default;
                legacyID = default;
                slugText = default;
                return false;
            }
        }

        static bool TryParseJupyterPath(string path, out DateTime date, out string slugText) {
            var match = Regex.Match(Path.GetFileName(path), @"^(\d{8})-(.+?)\.ipynb$");
            if(match.Success) {
                date = ParseDate(match.Groups[1].Value);
                slugText = match.Groups[2].Value;
                return true;
            } else {
                date = default;
                slugText = default;
                return false;
            }
        }

        static DateTime ParseDate(string text) {
            if(!text.StartsWith("0"))
                return DateTime.ParseExact(text, "yyyyMMdd", null);
            return default;
        }

        static int ParseLegacyID(string text) {
            if(!String.IsNullOrEmpty(text))
                return Convert.ToInt32(text);
            return default;
        }
    }

}
