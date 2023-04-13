using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blog {

    public class Entries {
        const int NPP = 5; // Hello from 2001...

        string[] _postPaths;
        string[] _articlePaths;
        string[] _draftPaths;

        public readonly int PageCount;

        public Entries() {
            Console.WriteLine(nameof(Entries));

            InitPaths(out var postPaths, out var articlePaths, out _draftPaths);

            var pageCount = postPaths.Length / NPP;
            if(pageCount * NPP < postPaths.Length)
                pageCount++;
            if(pageCount == 0)
                pageCount = 1;

            _postPaths = postPaths;
            _articlePaths = articlePaths;
            PageCount = pageCount;
        }

        static void InitPaths(out string[] posts, out string[] articles, out string[] drafts) {
            var postList = new List<string>();
            var draftList = new List<string>();

            var postSequence = Directory.EnumerateFiles("../notebooks", "*.ipynb", SearchOption.TopDirectoryOnly);

            if(Directory.Exists("posts")) {
                postSequence = postSequence.Concat(Directory.EnumerateFiles("posts", "*.md", SearchOption.AllDirectories));
            }

            foreach(var p in postSequence) {
                if(p.Contains(EntryPath.DRAFT_MARKER))
                    draftList.Add(p);
                else
                    postList.Add(p);
            }

            posts = postList
                .OrderByDescending(Path.GetFileName)
                .ToArray();

            drafts = draftList.ToArray();

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
            get { return _postPaths.Concat(_articlePaths).Concat(_draftPaths); }
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
