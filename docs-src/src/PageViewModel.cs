using System;
using System.Collections.Generic;
using System.Linq;

namespace Blog {

    public class PageViewModel {
        public IEnumerable<PageItemViewModel> Items;
        public PrevNextViewModel PrevNext;
    }

}
