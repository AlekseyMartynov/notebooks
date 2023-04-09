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

            var match = Regex.Match(Path.GetFileName(path), @"^(\d{8})\.(?:(\d{2,4})\.)?(.+?)\.md$");
            if(!match.Success)
                throw new Exception();

            var dateString = match.Groups[1].Value;
            if(!dateString.StartsWith("0"))
                Date = DateTime.ParseExact(dateString, "yyyyMMdd", null);

            SlugText = match.Groups[3].Value;

            if(match.Groups[2].Length > 0)
                LegacyID = Convert.ToInt32(match.Groups[2].Value);
        }

        public string DateText {
            get { return Date.ToString("dd.MM.yyyy"); }
        }

        public string Url {
            get { return $"{BlogInfo.PathPrefix}/{SlugText}/"; }
        }
    }

}
