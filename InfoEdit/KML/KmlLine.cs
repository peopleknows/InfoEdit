using InfoEdit.KML文件;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfoEdit
{
    public class KmlLine
    {
        private List<KmlPoint> pointList;

        public KmlLine()
        {
            pointList = new List<KmlPoint>();
        }

        public void Add(KmlPoint point)
        {
            pointList.Add(point);
        }

        public override string ToString()
        {
            string rtn = string.Empty;
            foreach (KmlPoint point in pointList)
                rtn += point.ToString() + " ";
            return rtn;
        }
    }
}
