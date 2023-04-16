using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Blog {

    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class MainController : Controller {
        BlogDataCache _blogData;

        public MainController(BlogDataCache blogData) {
            _blogData = blogData;
        }

        [Route("")]
        public IActionResult Index() {
            return Page(1);
        }

        [Route("page/{pageNumber:int}")]
        public IActionResult Page(int pageNumber) {
            var pageIndex = pageNumber - 1;
            var entries = _blogData.GetEntries();

            if(pageIndex < 0 || pageIndex > entries.PageCount - 1)
                throw new ArgumentOutOfRangeException();

            var items = new List<PageItemViewModel>();
            var libs = new JsLibs();

            foreach(var path in entries.GetPostPathsForPage(pageIndex)) {
                var pathObj = _blogData.GetPathObj(path);
                var content = _blogData.GetContent(path);

                libs.Add(content.LibsBeforeCut);

                items.Add(new PageItemViewModel {
                    Url = pathObj.Url,
                    Date = pathObj.DateText,
                    Title = content.Title,
                    PreviewHtml = content.BodyBeforeCut,
                    HasCut = content.HasCut
                });
            }

            var vm = new PageViewModel {
                Items = items,
                PrevNext = new PrevNextViewModel {
                    PrevTitle = "Раньше",
                    NextTitle = "Позже",
                    PrevUrl = entries.GetPageUrl(pageIndex + 1),
                    NextUrl = entries.GetPageUrl(pageIndex - 1),
                }
            };

            var layoutTitle = BlogInfo.Title;
            if(pageIndex > 0)
                layoutTitle += $" ― страница {1 + pageIndex} из {entries.PageCount}";

            SetLayoutData(new LayoutData {
                Title = layoutTitle,
                CanonicalAbsoluteUrl = Utils.MakeUrlAbsolute(entries.GetPageUrl(pageIndex)),
                RssAbsoluteUrl = BlogInfo.RssAbsoluteUrl,
                Libs = libs,
                RobotsTag = "noarchive, nosnippet"
            });

            return View("~/views/Page.cshtml", vm);
        }

        [Route("{slug:regex(^[[^.]]+$)}")]
        public IActionResult Entry(string slug) {
            var entries = _blogData.GetEntries();

            var entryPath = entries.AllPaths.FirstOrDefault(i => _blogData.GetPathObj(i).SlugText == slug);
            if(entryPath == null)
                return NotFound();

            var entryPathObj = _blogData.GetPathObj(entryPath);
            var entryContent = _blogData.GetContent(entryPath);

            var prevNext = new PrevNextViewModel();

            var prevPath = entries.GetPrevPostPath(entryPath);
            if(prevPath != null) {
                prevNext.PrevUrl = _blogData.GetPathObj(prevPath).Url;
                prevNext.PrevTitle = _blogData.GetContent(prevPath).Title;
            }

            var nextPath = entries.GetNextPostPath(entryPath);
            if(nextPath != null) {
                prevNext.NextUrl = _blogData.GetPathObj(nextPath).Url;
                prevNext.NextTitle = _blogData.GetContent(nextPath).Title;
            }

            var body = new StringBuilder(entryContent.BodyBeforeCut);
            if(entryContent.HasCut) {
                body
                    .Append("<span id=\"more\"></span>\n")
                    .Append(entryContent.BodyAfterCut);
            }

            var vm = new EntryViewModel {
                Url = entryPathObj.Url,
                HumanDate = entryPathObj.DateText,
                MachineDate = entryPathObj.Date.ToString("yyyy-MM-dd"),
                HasDate = entryPathObj.Date > DateTime.MinValue,
                Title = entryContent.Title,
                BodyHtml = body.ToString(),
                PrevNext = prevNext,
                GitHubUrl = entryPathObj.GetJupyterSourceUrl(),
                IsFossil = !entryPathObj.IsDraft && entryPathObj.Date.Year < 2018,
            };

            SetLayoutData(new LayoutData {
                Title = entryContent.Title,
                CanonicalAbsoluteUrl = Utils.MakeUrlAbsolute(entryPathObj.Url),
                OpenGraphType = "article",
                OpenGraphDescription = entryContent.Excerpt,
                OpenGraphImageAbsoluteUrl = Utils.MakeUrlAbsolute(entryContent.MainImageUrl),
                RenderDisqus = true,
                Libs = entryContent.Libs,
                RobotsTag = entryPathObj.IsDraft ? "noindex" : "noarchive"
            });

            return View("~/views/Entry.cshtml", vm);
        }

        [Route(BlogInfo.LegacyIDRedirectScriptFileName)]
        public IActionResult LegacyIDRedirectScript() {
            var js = new StringBuilder()
                .Append("function go(u){window.location.href=u};")
                .Append("/p=(\\d+)/.exec(window.location.search);")
                .Append("switch(~~RegExp.$1){");

            var pathObjList = _blogData.GetEntries().AllPaths
                .Select(p => _blogData.GetPathObj(p))
                .Where(p => p.LegacyID > 0)
                .OrderBy(p => p.LegacyID);

            foreach(var p in pathObjList)
                js.Append("case " + p.LegacyID + ":go('" + p.Url + "');break;");

            js.Append("default:go('/');");
            js.Append("}");

            return Content(js.ToString(), "text/javascript");
        }

        [Route(BlogInfo.RssFileName)]
        public IActionResult Rss() {
            return Content(RSS.Generate(_blogData), "application/rss+xml; charset=utf-8");
        }

        [Route("404.html")]
        public IActionResult Custom404() {
            SetLayoutData(new LayoutData {
                Title = "Страница не найдена",
                Libs = new JsLibs(),
                RobotsTag = "noindex"
            });

            return View("~/views/404.cshtml");
        }

        void SetLayoutData(LayoutData data) {
            ViewData["layoutData"] = data;
        }

    }

}
