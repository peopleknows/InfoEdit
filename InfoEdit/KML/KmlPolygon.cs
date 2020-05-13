using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfoEdit.KML
{/// <summary>
 /// include tessellate (defalut value is 1), and coordinate (include a string of line)
 /// </summary>
    public class KmlPolygon
    {
        public string extrude { get; set; }

        public string altitudeMode { get; set; }

        public outerBoundaryI outerBoundaryIs { get; set; }

        //public string outerBoundaryIs { get; set; }
        public class outerBoundaryI
        {

            public LinearRing LinearRing { get; set; }
            public outerBoundaryI()
            {

            }
            public outerBoundaryI(LinearRing l)
            {
                LinearRing = l;
            }
        }

        public class LinearRing
        {
            public string coordinates { get; set; }
            public LinearRing(string coords)
            {
                coordinates = coords;
            }
            public LinearRing()
            {

            }
        }
        public KmlPolygon()
        {

        }
        public KmlPolygon(List<string> points)
        {
            extrude = "1";
            altitudeMode = "relativeToGround";
            LinearRing l = new LinearRing(string.Join(" ", points.ToArray()));
            outerBoundaryIs = new outerBoundaryI(l);
        }
    }
}
