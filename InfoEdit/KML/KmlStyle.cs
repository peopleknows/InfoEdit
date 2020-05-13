using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace InfoEdit.KML文件
{
    public class KmlStyle
    {
        public readonly static string[,] linestyleTable ={
                                                      {"green","7fFFFF00","10"},
                                                      {"red",  "7f0000FF","10"},
                                                      {"blue", "7fFF0000","10"},
                                                      {"black","50000014","10"}
                                                     };
        public readonly static string[,] polygonstyleTable ={
                                                      {"poly1","7fFFFF00","10","7f0000FF"},
                                                      {"poly2",  "7f0000FF","10","7fFFFF00"},
                                                      {"poly3", "7fFF0000","10","50000014"},
                                                      {"poly4","50000014","10","7f0000FF"}
                                                     };
        public readonly static string[,] iconHrefs = {
            {"downArrowIcon","http://maps.google.com/mapfiles/kml/pal4/icon28.png"},
            {"globeIcon","http://maps.google.com/mapfiles/kml/pal3/icon19.png"},
            {"highlightPlacemark","http://maps.google.com/mapfiles/kml/paddle/red-stars.png"},
            {"normalPlacemark","http://maps.google.com/mapfiles/kml/paddle/wht-blank.png"}
        };

        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlElement("LineStyle")]
        public kmlLineStyle lineStyle { get; set; }
        [XmlElement("IconStyle")]
        public IconStyle iconStyle { get; set; }
        [XmlElement("PolyStyle")]
        public kmlPolyStyle polygonStyle { get; set; }
        // constructor
        public KmlStyle()
        {
        }

        public KmlStyle(string id, string color, string width)
        {
            ID = id;
            lineStyle = new kmlLineStyle(color, width);
        }
        public KmlStyle(string id, string href)
        {
            ID = id;
            iconStyle = new IconStyle(href);
        }
        public KmlStyle(string id, string color, string width, string fillColor)
        {
            ID = id;
            lineStyle = new kmlLineStyle(color, width);
            polygonStyle = new kmlPolyStyle(fillColor);
        }

        public static List<KmlStyle> getStyleList()
        {
            List<KmlStyle> rtn = new List<KmlStyle>();
            for (int i = 0; i < linestyleTable.GetLength(0); i++)
            {
                KmlStyle style = new KmlStyle(linestyleTable[i, 0], linestyleTable[i, 1], linestyleTable[i, 2]);
                rtn.Add(style);
            }
            for (int m = 0; m < iconHrefs.GetLength(0); m++)
            {
                KmlStyle style = new KmlStyle(iconHrefs[m, 0], iconHrefs[m, 1]);
                rtn.Add(style);
            }
            for (int n = 0; n < polygonstyleTable.GetLength(0); n++)
            {
                KmlStyle style = new KmlStyle(polygonstyleTable[n, 0], polygonstyleTable[n, 1], polygonstyleTable[n, 2], polygonstyleTable[n, 3]);
                rtn.Add(style);
            }
            return rtn;
        }


        // inner class
        public class kmlLineStyle
        {
            [XmlElement("color")]
            public string Color { get; set; }

            [XmlElement("width")]
            public string Width { get; set; }

            public kmlLineStyle()
            {
            }

            public kmlLineStyle(string color, string width)
            {
                Color = color;
                Width = width;
            }
        }
        public class IconStyle
        {
            [XmlElement("href")]
            public string Href { get; set; }



            public IconStyle()
            {
            }

            public IconStyle(string href)
            {
                Href = href;
            }
        }

        public class kmlPolyStyle
        {
            [XmlElement("color")]
            public string Color { get; set; }

            public kmlPolyStyle()
            {
            }

            public kmlPolyStyle(string fillColor)
            {
                Color = fillColor;
            }
        }
    }
}
