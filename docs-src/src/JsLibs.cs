using System;
using System.Collections.Generic;
using System.Linq;

namespace Blog {

    public class JsLibs {
        const string CDNJS = "https://cdnjs.cloudflare.com/ajax/libs";

        const string
            FANCYAPPS_VER = "5.0.15",
            HLJS_VER = "11.7.0",
            MEJS_VER = "4.2.17";

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
                yield return $"{CDNJS}/normalize/8.0.1/normalize.min.css";

                if(Fancybox)
                    yield return $"{CDNJS}/fancyapps-ui/{FANCYAPPS_VER}/fancybox/fancybox.min.css";

                if(HLJS)
                    yield return $"{CDNJS}/highlight.js/{HLJS_VER}/styles/github.min.css";

                if(MEJS)
                    yield return $"{CDNJS}/mediaelement/{MEJS_VER}/mediaelementplayer.min.css";
            }
        }

        public IEnumerable<string> IncludeScriptUrls {
            get {
                if(Fancybox)
                    yield return $"{CDNJS}/fancyapps-ui/{FANCYAPPS_VER}/fancybox/fancybox.umd.min.js";

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
                if(Fancybox)
                    yield return "Fancybox.bind();";

                if(HLJS)
                    yield return "hljs.highlightAll();";

                if(MEJS)
                    yield return "document.querySelectorAll('.entry audio').forEach(function(i) { i.style.width = '100%'; new MediaElementPlayer(i); });";
            }
        }
    }

}
