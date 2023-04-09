using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace Blog {

    public class RSS {

        public static string Generate(BlogDataCache blogData) {
            var postPaths = blogData.GetEntries().PostPaths.Take(20);

            var atomNs = (XNamespace)"http://www.w3.org/2005/Atom";
            var ccNs = (XNamespace)"http://backend.userland.com/creativeCommonsRssModule";

            var feedDate = postPaths
                .DefaultIfEmpty()
                .Max(i => blogData.GetPathObj(i).Date);

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(
                    "rss",
                    new XAttribute("version", "2.0"),
                    new XAttribute(XNamespace.Xmlns + "atom", atomNs),
                    new XAttribute(XNamespace.Xmlns + "creativeCommons", ccNs),
                    new XElement(
                        "channel",
                        new[] {
                            new XElement("title", BlogInfo.Title),
                            new XElement("link", BlogInfo.AbsoluteUrl),
                            new XElement(
                                atomNs + "link",
                                new XAttribute("href", BlogInfo.RssAbsoluteUrl),
                                new XAttribute("rel", "self"),
                                new XAttribute("type", "application/rss+xml")
                            ),
                            new XElement("description", BlogInfo.Description),
                            new XElement(ccNs + "license", BlogInfo.LicenseFullUrl),
                            new XElement("pubDate", FormatDate(feedDate)),
                            new XElement(
                                "image",
                                new XElement("title", BlogInfo.Title),
                                new XElement("link", BlogInfo.AbsoluteUrl),
                                new XElement("url", BlogInfo.GetAvatarUrl())
                            )
                        }
                        .Concat(postPaths.Select(p => {
                            var pathObj = blogData.GetPathObj(p);
                            var content = blogData.GetContent(p);

                            // https://stackoverflow.com/q/3155801
                            var escapedUrl = Utils.MakeUrlAbsolute(pathObj.Url);

                            var html = PrepareEntryHtml(content.BodyBeforeCut);
                            if(content.HasCut) {
                                html += String.Format(
                                    "<p><a href=\"{0}\">{1}</a></p>",
                                    WebUtility.HtmlEncode(escapedUrl),
                                    "Читать на сайте…"
                                );
                            }

                            return new XElement(
                                "item",
                                new XElement("guid", new XAttribute("isPermaLink", "true"), escapedUrl),
                                new XElement("link", escapedUrl),
                                new XElement("pubDate", FormatDate(pathObj.Date)),
                                new XElement("title", content.Title),
                                new XElement("description", new XCData(html))
                            );
                        }))
                        .ToArray()
                    )
                )
            );

            using(var w = new UTF8Writer()) {
                doc.Save(w);
                return w.ToString();
            }
        }

        static string FormatDate(DateTime d) {
            return String.Concat(
                d.ToString("d MMM yyyy HH':'mm':'ss", CultureInfo.InvariantCulture),
                " ",
                d.ToString("zzzz", CultureInfo.InvariantCulture).Replace(":", "")
            );
        }

        static string PrepareEntryHtml(string html) {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var docNode = doc.DocumentNode;

            var vukBlocks = docNode.Descendants("div")
                .Where(i => i.GetAttributeValue("class", "").StartsWith("vuk-"))
                .ToArray();

            foreach(var node in vukBlocks)
                node.Name = "blockquote";

            foreach(var node in docNode.Descendants()) {
                foreach(var attrName in node.Attributes.Select(i => i.Name).ToArray()) {
                    if(attrName == "alt" || attrName == "width" || attrName == "height")
                        continue;

                    if(attrName == "src" || attrName == "href") {
                        node.SetAttributeValue(
                            attrName,
                            Utils.MakeUrlAbsolute(node.GetAttributeValue(attrName, null))
                        );
                    } else {
                        node.Attributes.Remove(attrName);
                    }
                }

                if(node.Name == "img")
                    node.SetAttributeValue("border", "0");

                if(node.Name == "code" && node.ParentNode.Name != "pre")
                    node.Name = "em";
            }

            return docNode.InnerHtml;
        }

        class UTF8Writer : StringWriter {
            public override Encoding Encoding {
                get { return Encoding.UTF8; }
            }
        }

    }

}
