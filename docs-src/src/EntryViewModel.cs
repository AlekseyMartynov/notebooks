using System;
using System.Linq;

namespace Blog {

    public class EntryViewModel {
        public string Url;
        public string Title;
        public bool HasDate;
        public string Date;
        public string BodyHtml;
        public PrevNextViewModel PrevNext;
    }

}
