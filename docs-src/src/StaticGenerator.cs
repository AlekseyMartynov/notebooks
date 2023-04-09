using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Blog {

    public class StaticGenerator : IDisposable {
        const string INDEX_HTML = "index.html";

        WebClient _web;
        string _targetPath;
        BlogDataCache _blogData;

        public StaticGenerator(BlogDataCache blogData, string targetPath) {
            _web = new WebClient();
            _blogData = blogData;
            _targetPath = targetPath;
        }

        public void Dispose() {
            _web.Dispose();
        }

        public void Generate() {
            var entries = _blogData.GetEntries();

            foreach(var p in entries.AllPaths) {
                var url = _blogData.GetPathObj(p).Url;
                Download(url, url + INDEX_HTML);
            }

            for(var i = 0; i < entries.PageCount; i++) {
                var url = entries.GetPageUrl(i);
                Download(url, url + INDEX_HTML);
            }

            Download($"{BlogInfo.PathPrefix}/{BlogInfo.LegacyIDRedirectScriptFileName}");
            Download($"{BlogInfo.PathPrefix}/{BlogInfo.RssFileName}");
            Download($"{BlogInfo.PathPrefix}/404.html");
        }

        void Download(string url, string saveAs) {
            if(!saveAs.StartsWith('/'))
                throw new ArgumentException();

            saveAs = _targetPath + saveAs;
            if(File.Exists(saveAs))
                throw new InvalidOperationException();

            var text = _web.DownloadString("http://localhost:8099" + url);
            text = Utils.DosToUnix(text);

            Utils.EnsureParentDir(saveAs);
            File.WriteAllText(saveAs, text);
        }

        void Download(string path) {
            Download(path, path);
        }

    }

}
