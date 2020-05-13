using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;

namespace InfoEdit
{
    public class FileItem
    {
        private FileInfo fileInfo;//文件
        private string filePath;//文件路径
        private long fileSize;//文件大小
        private string dataType;//文件类型
        private string fileName;//文件名称
        private string dataTime;//修改时间
        //修改版本
        private int version;
        //多个车站文件集
        //private List<StationFile> stationFiles;
        private int stationCount;//车站个数

        public FileItem()
        {
            fileInfo = null;
            filePath = Application.StartupPath;
            fileSize = fileInfo.Length;
            dataType = fileInfo.Extension;
            fileName = fileInfo.Name;
            dataTime = fileInfo.LastWriteTime.ToString();
            version = 1;//初始情况下为1
            //stationFiles = null;
            stationCount = 1;
        }

        public FileItem(FileInfo f)
        {
            fileInfo = f;
            filePath = f.FullName;
            fileSize = f.Length;
            dataType = f.Extension;
            fileName = f.Name;
            dataTime = f.LastWriteTime.ToString();
            version = 1;
        }
        /// <summary>
        /// 得到
        /// </summary>
        public void GetStationFile(string xmlFile)
        {

        }
        public void GetStationCount(string xmlFile)
        {
            if (xmlFile != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFile);
                XmlNode node = doc.DocumentElement;
                StationCount = CertainNodeCount(node, "station");
            }
        }
        private int CertainNodeCount(XmlNode node, string nodeName)
        {
            int count = 0;
            if (node.Name != nodeName)
            {
                if (node.HasChildNodes)
                {
                    XmlNodeList list = node.ChildNodes;
                    foreach (XmlNode n in list)
                    {
                        count += CertainNodeCount(n, nodeName);
                    }
                }
            }
            else
            {
                count++;
            }
            return count;
        }

        public FileInfo FileInfo { get { return fileInfo; } set { fileInfo = value; } }
        //文件的绝对路径
        public string FilePath { get { return filePath; } set { filePath = value; } }
        //文件大小
        public long FileSize { get { return fileSize; } set { fileSize = value; } }
        //文件类型
        public string DataType { get { return dataType; } set { dataType = value; } }
        //文件名称
        public string FileName { get { return fileName; } set { fileName = value; } }
        //文件上一次修改的时间
        public string DataTime { get { return dataTime; } set { dataTime = value; } }
        //文件的版本
        public int Version { get { return version; } set { version = value; } }
        //public List<StationFile> StationFiles { get { return stationFiles; } set { stationFiles = value; } }
        public int StationCount { get { return stationCount; } set { stationCount = value; } }
    }
}
