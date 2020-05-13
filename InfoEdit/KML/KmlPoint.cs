using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfoEdit.KML文件
{
    public class KmlPoint
    {
        public string coordinates { get; set; }
        public KmlPoint()
        {
        }
        public KmlPoint(string coords)
        {
            coordinates = coords;
        }

    }
}
