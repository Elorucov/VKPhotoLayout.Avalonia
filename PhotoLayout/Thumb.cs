using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoLayout {
    public class Thumb {
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public bool LastColumn { get; internal set; }
        public bool LastRow { get; internal set; }
        public bool ColumnItem { get; internal set; }
        public bool EndFirstRow { get; internal set; }
    }
}
