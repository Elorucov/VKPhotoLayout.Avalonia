using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoLayout {
    public class Thumb {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool LastColumn { get; set; }
        public bool LastRow { get; set; }
        public bool ColumnItem { get; set; }
        public bool EndFirstRow { get; set; }
    }
}
