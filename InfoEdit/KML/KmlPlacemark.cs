using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using InfoEdit.KML;

namespace InfoEdit.KML文件
{
    /// <summary>
    /// include the following five elements
    /// name
    /// description
    /// styleUrl
    /// ExtendedData (empty by default)
    /// LineString
    /// </summary>
    public class KmlPlacemark
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("description")]
        public string _description;

        [XmlElement("styleUrl")]
        public string StyleUrl { get; set; }

        [XmlElement("ExtendedData")]
        public string ExtendedData { get; set; }

        [XmlElement("LineString")]
        public KmlLineString LineString { get; set; }
        [XmlElement("Polygon")]
        public KmlPolygon Polygon { get; set; }
        [XmlElement("Point")]
        public KmlPoint Point { get; set; }
        public KmlPlacemark()
        {
        }

        public KmlPlacemark(string name, string description, string styleUrl, KmlPoint point, KmlLineString line, KmlPolygon polygon)
        {
            Name = name;
            _description = description;
            StyleUrl = styleUrl;
            ExtendedData = string.Empty;
            if (point != null)
            {
                Point = point;
            }
            if (line != null)
            {
                LineString = line;
            }
            if (polygon != null)
            {
                Polygon = polygon;
            }
        }
    }
}
