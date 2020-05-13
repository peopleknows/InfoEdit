using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace InfoEdit
{
    public class StationFile : FileItem
    {
        private string stationName;
        private Dictionary<string, DataTable> stationFileItems;

        public StationFile()
        {
            this.stationFileItems = new Dictionary<string, DataTable>();
            stationFileItems.Add("车站总览", null);
            stationFileItems.Add("轨道地理信息", null);
            stationFileItems.Add("站场图", null);
        }
        public StationFile(string staName, Dictionary<string, DataTable> dts)
        {
            stationName = staName;
            stationFileItems = dts;
        }

        public string StationName { get { return stationName; } set { stationName = value; } }
        public Dictionary<string, DataTable> StationFileItems { get { return stationFileItems; } set { stationFileItems = value; } }
    }
}
