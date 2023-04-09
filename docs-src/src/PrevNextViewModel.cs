using System;
using System.Linq;

namespace Blog {

    public class PrevNextViewModel {
        public string
            PrevUrl, PrevTitle,
            NextUrl, NextTitle;

        public bool HasPrev {
            get { return !String.IsNullOrEmpty(PrevUrl); }
        }

        public bool HasNext {
            get { return !String.IsNullOrEmpty(NextUrl); }
        }
    }

}
