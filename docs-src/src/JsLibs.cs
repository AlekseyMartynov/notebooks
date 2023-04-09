using System;
using System.Collections.Generic;
using System.Linq;

namespace Blog {

    public class JsLibs {
        const string CDNJS = "https://cdnjs.cloudflare.com/ajax/libs";

        const string
            JQ_VER = "1.12.4",
            FANCYBOX_VER = "3.2.5",
            HLJS_VER = "9.12.0",
            MEJS_VER = "4.2.7";

        public bool Fancybox;
        public bool HLJS;
        public bool MEJS;

        public void Add(JsLibs other) {
            Fancybox = Fancybox || other.Fancybox;
            HLJS = HLJS || other.HLJS;
            MEJS = MEJS || other.MEJS;
        }

        public IEnumerable<string> CssUrls {
            get {
                yield return $"{CDNJS}/normalize/8.0.0/normalize.min.css";

                if(Fancybox)
                    yield return $"{CDNJS}/fancybox/{FANCYBOX_VER}/jquery.fancybox.min.css";

                if(HLJS)
                    yield return $"{CDNJS}/highlight.js/{HLJS_VER}/styles/github.min.css";

                if(MEJS)
                    yield return $"{CDNJS}/mediaelement/{MEJS_VER}/mediaelementplayer.min.css";
            }
        }

        public IEnumerable<string> IncludeScriptUrls {
            get {
                yield return $"{CDNJS}/jquery/{JQ_VER}/jquery.min.js";

                if(Fancybox)
                    yield return $"{CDNJS}/fancybox/{FANCYBOX_VER}/jquery.fancybox.min.js";

                if(HLJS) {
                    yield return $"{CDNJS}/highlight.js/{HLJS_VER}/highlight.min.js";
                    yield return $"{CDNJS}/highlight.js/{HLJS_VER}/languages/dockerfile.min.js";
                }

                if(MEJS)
                    yield return $"{CDNJS}/mediaelement/{MEJS_VER}/mediaelement-and-player.min.js";
            }
        }

        public IEnumerable<string> InitScripts {
            get {
                if(HLJS)
                    yield return "hljs.initHighlightingOnLoad();";

                if(MEJS)
                    yield return "$('.entry audio').width('100%').mediaelementplayer();";
            }
        }
    }

}
