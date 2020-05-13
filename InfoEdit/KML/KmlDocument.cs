using InfoEdit.KML文件;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace InfoEdit
{
    public class KmlDocument
    {
        [XmlElement("name")]
        public string Name { get; set; }

        private string _description;
        [XmlElement("description")]
        public XmlNode Description { get; set; }


        [XmlElement("Placemark")]
        public List<KmlPlacemark> Placemarks { get; set; }

        [XmlElement("Style")]
        public List<KmlStyle> styles { get; set; }

        public KmlDocument()
        {
        }

        public KmlDocument(string name, string description, List<KmlPlacemark> placemarks)
        {
            Name = name;
            _description = description;
            Placemarks = placemarks;
            styles = KmlStyle.getStyleList();
        }
    }
}

