using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using InfoEdit.KML文件;
using InfoEdit.KML;

namespace InfoEdit
{
    [XmlRoot("kml", Namespace = "http://www.opengis.net/kml/2.2")]
    public class Kml
    {
        [XmlElement("Document")]
        public KmlDocument document { get; set; }

        // constructor
        public Kml()
        {
        }

        public Kml(KmlDocument doc)
        {
            document = doc;
        }

        public void GenerateKmlFile(string KmlFileName)
        {
            using (FileStream fs = new FileStream(KmlFileName, FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    XmlSerializer serializer = new XmlSerializer(this.GetType());
                    serializer.Serialize(sw, this);
                }
            }
        }
        /// <summary>
        /// coordinates lon,lat,alt 经纬高度多个用;隔开
        /// </summary>
        public class dataFormat
        {
            public string type { get; set; }
            public string coordinates { get; set; }

            public string name { get; set; }

            public string description { get; set; }
            public string styleUrl { get; set; }

        }
        /// <summary>
        /// 调用此方法
        /// </summary>
        /// <param name="xmlname">xml名称</param>
        /// <param name="filename">导出文件名</param>
        /// <param name="lst">数据</param>
        /// <returns></returns>
        public static int CreateKml(string xmlname, string filename, List<dataFormat> lst)
        {
            List<KmlPlacemark> ls = new List<KmlPlacemark>();
            foreach (dataFormat d in lst)
            {
                string type = d.type;

                string coords = d.coordinates.Trim();

                string styleUrl = d.styleUrl;

                string name = d.name;
                if (string.IsNullOrEmpty(name))
                {
                    name = "";
                }
                string description = d.description;
                if (string.IsNullOrEmpty(description))
                {
                    description = "";
                }

                if (type == "point")
                {
                    if (string.IsNullOrEmpty(styleUrl))
                    {
                        styleUrl = "#downArrowIcon";
                    }
                    KmlPoint p = new KmlPoint(coords);
                    KmlPlacemark placemark = new KmlPlacemark(name, description, styleUrl, p, null, null);
                    ls.Add(placemark);
                }
                else if (type == "line")
                {
                    if (string.IsNullOrEmpty(styleUrl))
                    {
                        styleUrl = "#blue";
                    }
                    KmlLineString line = new KmlLineString(coords.Split(';').ToList());
                    KmlPlacemark placemark = new KmlPlacemark(name, description, styleUrl, null, line, null);
                    ls.Add(placemark);
                }
                else if (type == "polygon")
                {
                    if (string.IsNullOrEmpty(styleUrl))
                    {
                        styleUrl = "#blue";
                    }
                    KmlPolygon polygon = new KmlPolygon(coords.Split(';').ToList());
                    KmlPlacemark placemark = new KmlPlacemark(name, description, styleUrl, null, null, polygon);
                    ls.Add(placemark);
                }

            }
            try
            {
                KmlDocument document = new KmlDocument(xmlname, filename, ls);
                Kml kml = new Kml(document);
                kml.GenerateKmlFile("a.kml");
                return 1;
            }
            catch (Exception e)
            {
                return 0;
            }

        }
    }
}
