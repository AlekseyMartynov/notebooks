using System;
using System.Linq;

namespace Blog {

    public static class BlogInfo {
        public const string
            PathPrefix = "/notebooks",
            AbsoluteUrl = "https://alekseymartynov.github.io" + PathPrefix,
            RssFileName = "rss.xml",
            RssAbsoluteUrl = AbsoluteUrl + "/" + RssFileName,
            LegacyIDRedirectScriptFileName = "legacy-id-redirect.js",
            Title = "AM’s Notebooks",
            Description = "Misc tech notes",
            LicenseName = "Creative Commons Attribution 3.0 Unported",
            LicenseFullUrl = "https://creativecommons.org/licenses/by/3.0/",
            LicenseDeedUrl = LicenseFullUrl + "deed.ru";

        public static string GetAvatarUrl() {
            return "https://github.com/AlekseyMartynov.png";
        }
    }

}
