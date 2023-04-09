using System;
using System.Linq;

namespace Blog {

    public static class BlogInfo {
        public const string
            PathPrefix = "/notebooks",
            AbsoluteUrl = "https://blog.amartynov.ru" + PathPrefix,
            RssFileName = "rss.xml",
            RssAbsoluteUrl = AbsoluteUrl + "/" + RssFileName,
            LegacyIDRedirectScriptFileName = "legacy-id-redirect.js",
            Title = "blog.amartynov.ru",
            SeoTitle = "Aleksey Martynov's Blog",
            Description = "Персональный блог",
            LicenseName = "Creative Commons Attribution 3.0 Unported",
            LicenseFullUrl = "https://creativecommons.org/licenses/by/3.0/",
            LicenseDeedUrl = LicenseFullUrl + "deed.ru";

        public static readonly (string Title, string Url)[] ContactLinks = new[] {
            ("REDACTED", "REDACTED"),
        };

        public static string GetGravatarUrl(int size) {
            return "https://www.gravatar.com/avatar/REDACTED?s=" + size;
        }
    }

}
