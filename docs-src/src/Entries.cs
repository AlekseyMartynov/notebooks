using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blog {

    public class Entries {
        const int NPP = 5; // Hello from 2001...

        string[] _postPaths;
        string[] _articlePaths;

        public readonly int PageCount;

        public Entries() {
            Console.WriteLine(nameof(Entries));

            InitPaths(out var postPaths, out var articlePaths);

            var pageCount = postPaths.Length / NPP;
            if(pageCount * NPP < postPaths.Length)
                pageCount++;
            if(pageCount == 0)
                pageCount = 1;

            _postPaths = postPaths;
            _articlePaths = articlePaths;
            PageCount = pageCount;
        }

        static void InitPaths(out string[] posts, out string[] articles) {
            var postList = new List<string>();

            postList.AddRange(Directory.EnumerateFiles("../notebooks", "*.ipynb", SearchOption.TopDirectoryOnly));

            if(Directory.Exists("posts")) {
                postList.AddRange(Directory.EnumerateFiles("posts", "*.md", SearchOption.AllDirectories));
            }

            posts = postList
                .OrderByDescending(Path.GetFileName)
                .ToArray();

            if(Directory.Exists("articles")) {
                articles = Directory.GetFiles("articles", "*.md", SearchOption.AllDirectories);
            } else {
                articles = Array.Empty<string>();
            }
        }

        public IEnumerable<string> PostPaths {
            get { return _postPaths; }
        }

        public IEnumerable<string> AllPaths {
            get { return _postPaths.Concat(_articlePaths); }
        }

        public IEnumerable<string> GetPostPathsForPage(int pageIndex) {
            return _postPaths.Skip(NPP * pageIndex).Take(NPP);
        }

        public string GetPageUrl(int pageIndex) {
            if(pageIndex < 0)
                return null;
            if(pageIndex == 0)
                return $"{BlogInfo.PathPrefix}/";
            if(pageIndex < PageCount)
                return $"{BlogInfo.PathPrefix}/page/{1 + pageIndex}/";
            return null;
        }

        public string GetNextPostPath(string postPath) {
            var index = Array.IndexOf(_postPaths, postPath);
            if(index < 1)
                return null;
            return _postPaths[index - 1];
        }

        public string GetPrevPostPath(string postPath) {
            var index = Array.IndexOf(_postPaths, postPath);
            if(index < 0 || index == _postPaths.Length - 1)
                return null;
            return _postPaths[index + 1];
        }
    }

}
