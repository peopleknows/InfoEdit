using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfoEdit.KML文件
{
    /// <summary>
    /// include tessellate (defalut value is 1), and coordinate (include a string of line)
    /// </summary>
    public class KmlLineString
    {
        public string extrude { get; set; }


        public string altitudeMode { get; set; }


        public string coordinates { get; set; }

        public string visibility { get; set; }
        public KmlLineString()
        {

        }

        public KmlLineString(List<string> points)
        {
            extrude = "1";
            altitudeMode = "relativeToGround";
            visibility = "0";
            coordinates = string.Join(" ", points.ToArray());
        }
    }
}
