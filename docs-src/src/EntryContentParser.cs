using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Markdig;

namespace Blog {

    public class EntryContentParser {
        const string MAIN_IMAGE_ATTR = "data-og";

        MarkdownPipeline _pipeline;

        public EntryContentParser() {
            _pipeline = new MarkdownPipelineBuilder()
                .UsePipeTables()
                .UseEmphasisExtras()
                .UseGenericAttributes()
                .UseAutoLinks()
                .UseCustomContainers()
                .UseEmojiAndSmiley()
                .Build();
        }

        public EntryContent Parse(string path) {
            Console.WriteLine($"{nameof(EntryContentParser)}.{nameof(Parse)}({path})");

            var md = ReadMarkdown(path);
            CheckWhitespace(md);

            var slices = Slice(md);

            var htmlBefore = MarkdownToHtml(slices.Before);
            var htmlAfter = MarkdownToHtml(slices.After);

            var allHtmlDescendants = htmlBefore.Descendants().Concat(htmlAfter.Descendants());

            var mainImageUrl = FindMainImageUrl(allHtmlDescendants);
            foreach(var n in allHtmlDescendants)
                n.Attributes[MAIN_IMAGE_ATTR]?.Remove();

            return new EntryContent {
                Title = HtmlToText(MarkdownToHtml(slices.Title)),
                BodyBeforeCut = htmlBefore.InnerHtml,
                BodyAfterCut = htmlAfter.InnerHtml,
                Excerpt = GenerateExcerpt(htmlBefore),
                MainImageUrl = mainImageUrl,
                Libs = DiscoverRequiredLibs(allHtmlDescendants),
                LibsBeforeCut = DiscoverRequiredLibs(htmlBefore.Descendants())
            };
        }

        static string ReadMarkdown(string path) {
            var text = File.ReadAllText(path);
            text = Utils.DosToUnix(text);
            if(path.EndsWith(".ipynb"))
                text = JupyterToMarkdown.Convert(text);
            return text;
        }

        static void CheckWhitespace(string md) {
            if(Regex.IsMatch(md, @" \n"))
                throw new Exception("Trailing whitespaces");

            if(md.Contains("\n\n\n"))
                throw new Exception("Multi empty lines");

            if(md.Contains("\t"))
                throw new Exception("Tabs!");
        }

        static (string Title, string Before, string After) Slice(string md) {
            const string cutMark = "<!--more-->";

            var bodyAt = md.IndexOf("\n\n");

            var title = md.Substring(0, bodyAt);
            var before = md.Substring(bodyAt);
            var after = "";

            var cutAt = before.IndexOf(cutMark);
            if(cutAt > -1) {
                after = before.Substring(cutAt + cutMark.Length);
                before = before.Substring(0, cutAt);
            }

            return (title.Trim(), before.Trim(), after.Trim());
        }

        HtmlNode MarkdownToHtml(string md) {
            var doc = new HtmlDocument();
            doc.LoadHtml(!String.IsNullOrEmpty(md) ? Markdown.ToHtml(md, _pipeline) : "");

            var node = doc.DocumentNode;
            Texturize(node);
            SetImageDimensions(node);
            SetNoHighlight(node);
            PatchRootedUrls(node);
            AddTargetBlank(node);
            return node;
        }

        static string HtmlToText(HtmlNode html) {
            var text = html.InnerText.Trim();
            text = Regex.Replace(text, "\\s+", " ");
            return WebUtility.HtmlDecode(text);
        }

        static void SetImageDimensions(HtmlNode html) {
            foreach(var img in html.Descendants("img")) {
                var src = img.GetAttributeValue("src", "");
                if(src.Contains(":"))
                    continue;

                var localPath = "wwwroot" + src;
                var size = Image.FromFile(localPath).Size;
                var maxWidth = 650;

                var retinaAttr = img.Attributes["data-retina"];
                if(retinaAttr != null) {
                    var scale = 2;
                    if(retinaAttr.Value.Length > 0)
                        scale = Int32.Parse(retinaAttr.Value);

                    size.Width = size.Width / scale;
                    size.Height = size.Height / scale;
                    retinaAttr.Remove();
                }

                if(size.Width <= maxWidth) {
                    img.SetAttributeValue("width", size.Width.ToString());
                    img.SetAttributeValue("height", size.Height.ToString());
                } else {
                    img.SetAttributeValue("width", maxWidth.ToString());
                }
            }
        }

        static string FindMainImageUrl(IEnumerable<HtmlNode> nodeEnumerator) {
            return nodeEnumerator
                .Where(n => n.GetAttributeValue(MAIN_IMAGE_ATTR, null) != null)
                .Select(n => {
                    switch(n.Name) {
                        case "img":
                            return n.GetAttributeValue("src", null);
                        case "a":
                            return n.GetAttributeValue("href", null);
                    }
                    throw new NotSupportedException();
                })
                .FirstOrDefault()
            ?? nodeEnumerator
                .Where(n => n.Name == "img")
                .Select(n => n.GetAttributeValue("src", null))
                .Where(i => !i.StartsWith("data:"))
                .FirstOrDefault();
        }

        static string GenerateExcerpt(HtmlNode html) {
            var text = HtmlToText(html);

            if(text.Length > 300) {
                var cutIndex = text.IndexOf(' ', 290);
                if(cutIndex > 0)
                    text = text.Substring(0, cutIndex);
            }

            while(Char.IsPunctuation(text.Last()))
                text = text.Substring(0, text.Length - 1);

            return text + "…";
        }

        static void Texturize(HtmlNode node) {
            if(node is HtmlTextNode text) {
                text.Text = Texturize(text.Text);
            } else if(node.Name != "code") {
                foreach(var child in node.ChildNodes)
                    Texturize(child);
            }
        }

        static string Texturize(string text) {
            text = Regex.Replace(text, @"(^|\s)-($|\s)", "$1—$2");
            text = Regex.Replace(text, @"(^|[^.])\.{3}($|[^.])", "$1…$2");
            text = Regex.Replace(text, @"(^|\s)&quot;(\S)", "$1“$2");
            text = Regex.Replace(text, @"(\S)&quot;($|\s|\p{P})", "$1”$2");
            text = text.Replace("'", "’");
            return text;
        }

        static void SetNoHighlight(HtmlNode node) {
            foreach(var code in node.Descendants().Where(IsPreCode)) {
                if(!HasLanguageClass(code))
                    code.SetAttributeValue("class", "nohighlight");
            }
        }

        static bool IsPreCode(HtmlNode node) {
            return node.Name == "code" && node.ParentNode.Name == "pre";
        }

        static bool HasLanguageClass(HtmlNode node) {
            return node.GetAttributeValue("class", "").Contains("language-");
        }

        static JsLibs DiscoverRequiredLibs(IEnumerable<HtmlNode> nodeEnumerator) {
            return new JsLibs {
                Fancybox = nodeEnumerator.Any(n => n.GetAttributeValue("data-fancybox", null) != null),
                HLJS = nodeEnumerator.Any(n => IsPreCode(n) && HasLanguageClass(n)),
                MEJS = nodeEnumerator.Any(n => n.Name == "audio")
            };
        }

        static void PatchRootedUrls(HtmlNode root) {
            PatchRootedUrls(root, "a", "href");
            PatchRootedUrls(root, "img", "src");
            PatchRootedUrls(root, "audio", "src");
        }

        static void PatchRootedUrls(HtmlNode root, string tagName, string attrName) {
            foreach(var i in root.Descendants(tagName)) {
                var url = i.GetAttributeValue(attrName, "");
                if(url.StartsWith('/') && !url.StartsWith(BlogInfo.PathPrefix))
                    i.SetAttributeValue(attrName, BlogInfo.PathPrefix + url);
            }
        }

        static void AddTargetBlank(HtmlNode root) {
            foreach(var i in root.Descendants("a")) {
                var href = i.GetAttributeValue("href", "");
                var target = i.GetAttributeValue("target", "");
                if(String.IsNullOrEmpty(target) && href.Contains(":"))
                    i.SetAttributeValue("target", "_blank");
            }
        }
    }

}
