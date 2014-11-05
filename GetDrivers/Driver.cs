using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GetDrivers {
    class Driver {
        public string Path { get; set; }
        public string Class { get; set; }

        public override string ToString() {
            return string.Format("Path: {0} -- Class: {1}", Path, Class);
        }
    }
}
