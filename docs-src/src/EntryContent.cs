using System;
using System.Linq;

namespace Blog {

    public class EntryContent {
        public string Title;
        public string BodyBeforeCut;
        public string BodyAfterCut;
        public string Excerpt;
        public string MainImageUrl;

        public JsLibs Libs;
        public JsLibs LibsBeforeCut;

        public bool HasCut {
            get { return !String.IsNullOrEmpty(BodyAfterCut); }
        }
    }

}
