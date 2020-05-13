using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using DevExpress.XtraEditors;

namespace InfoEdit
{

    #region
    [Serializable]
    [XmlRoot("Station")]
    public class Station
    {
        private StationProperties stationProperties;
        private TrackInfo trackInfo;
        private TrainControlData trainControlData;
        [NonSerialized]
        private string error = "";

        public Station()
        {
            this.StationProperties = new StationProperties();
            this.TrackInfo = new TrackInfo();
            this.TrainControlData = new TrainControlData();
            
        }
        public Station(StationProperties s, TrackInfo t, TrainControlData tc)
        {
            this.StationProperties = s;
            this.TrackInfo = t;
            this.TrainControlData = tc;
        }
        [XmlElement("StationProperties")]
        public StationProperties StationProperties { get { return stationProperties; } set { stationProperties = value; } }

        [XmlElement("TrackInfo")]
        public TrackInfo TrackInfo { get { return trackInfo; } set { trackInfo = value; } }

        [XmlElement("TrainControlData")]
        public TrainControlData TrainControlData { get { return trainControlData; } set { trainControlData = value; } }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; }set { error = value; } }
    }
    [Serializable]
    [XmlRoot("StationProperties")]
    public class StationProperties
    {
        private ushort stationNum;//车站编号1-65535
        private string stationName;//上行方向车站个数
        private byte uplinkStationCount;//上行方向车站个数1-255
        private ushort uplinkStationNum;//上行方向车站编号1-65535
        private byte downlinkStationCount;//下行方向车站个数1-255
        private ushort downlinkStationNum;//下行方向车站编号1-65535
        private ushort trackGISFileSize;//轨道地理信息数据大小
        private uint trackGISCRC;//轨道地理信息CRC
        private byte trackGISVersion;//轨道地理信息数据版本
        private int minimumLat;//最小纬度-32400000～324000000
        private int minimumLon;//最小经度-648000000～648000000
        private int maximumLat;//最大纬度-324000000～324000000
        private int maximumLon;//最大经度-648000000～648000000
        private uint trainControlDataSize;//固定应用数据大小
        private uint trainControlDataCRC;//固定应用数据文件CRC
        private byte trainControlDataversion;//固定应用数据版本，从1开始加1
        [NonSerialized]
        private string error = "";

        public StationProperties()
        {
            this.StationNum = 1;
            this.StationNum = 1;
            this.StationName = "车站名称";
            this.UplinkStationCount = 1;
            this.UplinkStationNum = 1;
            this.DownlinkStationCount = 1;
            this.DownlinkStationNum = 1;
            this.TrackGISFileSize = 0;
            this.TrackGISCRC = 0;
            this.TrackGISVersion = 1;
            this.MinimumLat = 0;
            this.MinimumLon = 0;
            this.MaximumLat = 0;
            this.MaximumLon = 0;
            this.TrainControlDataSize = 0;
            this.TrainControlDataCRC = 0;
            this.TrainControlDataversion = 1;
        }
        public StationProperties(ushort stationnum, string stationname, byte uplinkstationCount, ushort uplinkstationNum, byte downlinkstationCount, ushort downlinkStationNum, ushort trackGISFileSize, uint trackGISCRC, byte trackGISversion, int minimumLat, int minimumLon, int maximumLat, int maximumLon, uint trainControlDataSize, uint trainControlDataCRC, byte trainControlDataversion)
        {
            this.StationNum = stationnum;
            this.StationName = stationname;
            this.UplinkStationCount = uplinkstationCount;
            this.UplinkStationNum = uplinkstationNum;
            this.DownlinkStationCount = downlinkstationCount;
            this.DownlinkStationNum = downlinkStationNum;
            this.TrackGISFileSize = trackGISFileSize;
            this.TrackGISCRC = trackGISCRC;
            this.TrackGISVersion = trackGISversion;
            this.MinimumLat = minimumLat;
            this.MinimumLon = minimumLon;
            this.MaximumLat = maximumLat;
            this.MaximumLon = maximumLon;
            this.TrainControlDataSize = trainControlDataSize;
            this.TrainControlDataCRC = trainControlDataCRC;
            this.TrainControlDataversion = trainControlDataversion;
        }

        [XmlElement("StationNum")]
        public UInt16 StationNum
        {
            get { return stationNum; }
            set
            {
                if (value == 0)
                {
                    error += string.Format("StationNum={0}——车站编号超出范围", value) + "\r\n";
                    stationNum = 1;
                }
                else
                { stationNum = value; }
            }
        }

        [XmlElement("StationName")]
        public string StationName
        {
            get { return stationName; }
            set { stationName = value; }
        }

        [XmlElement("UplinkStationCount")]
        public byte UplinkStationCount
        {
            get { return uplinkStationCount; }
            set
            {
                if (value == 0)
                {
                    error += string.Format("UplinkStationCount={0}——上行方向车站个数超出范围", value) + "\r\n";
                    uplinkStationCount = 1;
                    //Console.WriteLine(error);
                    ////throw new Exception("上行方向车站个数超出范围"); 
                }
                else
                { uplinkStationCount = value; }
            }
        }
        [XmlElement("UplinkStationNum")]
        public UInt16 UplinkStationNum
        {
            get { return uplinkStationNum; }
            set
            {
                if (value == 0)
                {
                    error += string.Format("UplinkStationNum={0}——下行方向车站编号超出范围", value) + "\r\n";
                    uplinkStationNum = 1;
                   // Console.WriteLine(error);
                    ////throw new Exception("下行方向车站编号超出范围"); 
                }
                else
                { uplinkStationNum = value; }
            }
        }

        [XmlElement("DownlinkStationCount")]
        public byte DownlinkStationCount
        {
            get { return downlinkStationCount; }
            set
            {
                if (value == 0)
                {
                    error += string.Format("DownlinkStationCount={0}——下行方向车站个数超出范围", value) + "\r\n";
                    downlinkStationCount = 1;
                    ////throw new Exception("下行方向车站个数超出范围"); 
                }
                else
                { downlinkStationCount = value; }
            }
        }

        [XmlElement("DownlinkStationNum")]
        public UInt16 DownlinkStationNum
        {
            get { return downlinkStationNum; }
            set
            {
                if (value == 0)
                {
                    error += string.Format("DownlinkStationNum={0}——下行方向车站编号超出范围",value)+"\r\n";
                    downlinkStationNum = 1;
                    ////throw new Exception("下行方向车站编号超出范围");
                }
                else
                { downlinkStationNum = value; }
            }
        }
        [XmlElement("TrackGISFileSize")]
        public UInt16 TrackGISFileSize { get { return trackGISFileSize; } set { trackGISFileSize = value; } }

        [XmlElement("TrackGISCRC")]
        public uint TrackGISCRC { get { return trackGISCRC; } set { trackGISCRC = value; } }

        [XmlElement("TrackGISVersion")]
        public byte TrackGISVersion
        {
            get
            {
                return trackGISVersion;
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("TrackGISVersion={0}——轨道地理信息数据版本超出范围",value)+"\r\n";
                    trackGISVersion = 1;
                    ////throw new Exception("轨道地理信息数据版本超出范围"); 
                }
                else
                { trackGISVersion = value; }
            }
        }

        [XmlElement("MinimumLat")]
        public int MinimumLat
        {
            get { return minimumLat; }
            set
            {
                if (-32400000 <= value && value <= 324000000)
                {
                    minimumLat = value;
                }
                else
                {
                    error += string.Format("MinimumLat={0}——最小纬度超出范围",value)+"\r\n";
                    minimumLat = 0;
                    ////throw new Exception("最小纬度超出范围");
                }
            }
        }
        [XmlElement("MinimumLon")]
        public int MinimumLon
        {
            get { return minimumLon; }
            set
            {
                if (-648000000 <= value && value <= 648000000)
                {
                    minimumLon = value;
                }
                else
                {
                    error += string.Format("MinimumLon={0}——最小经度超出范围",value)+"\r\n";
                    minimumLon = 0;
                    //throw new Exception("最小经度超出范围");
                }
            }
        }
        [XmlElement("MaximumLat")]
        public int MaximumLat
        {
            get { return maximumLat; }
            set
            {
                if (-32400000 <= value && value <= 324000000)
                {
                    maximumLat = value;
                }
                else
                {
                    error += string.Format("MaximumLat={0}——最大纬度超出范围",value)+"\r\n";
                    maximumLat = 0;
                    //throw new Exception("最大纬度超出范围");
                }
            }
        }
        [XmlElement("MaximumLon")]
        public int MaximumLon
        {
            get { return maximumLon; }
            set
            {
                if (-648000000 <= value && value <= 648000000)
                {
                    maximumLon = value;
                }
                else
                {
                    error += string.Format("MaxiMumLon={0}——最大经度超出范围", value) + "\r\n";
                    maximumLon = 0;
                       //throw new Exception("最大经度超出范围");
                }
            }
        }
        [XmlElement("TrainControlDataSize")]
        public uint TrainControlDataSize { get { return trainControlDataSize; } set { trainControlDataSize = value; } }

        [XmlElement("TrainControlDataCRC")]
        public uint TrainControlDataCRC { get { return trainControlDataCRC; } set { trainControlDataCRC = value; } }

        [XmlElement("TrainControlDataversion")]
        public byte TrainControlDataversion { get { return trainControlDataversion; } set { trainControlDataversion = value; } }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set { error = value; } }

    }
    
    #endregion

    #region 车站数据包括轨道地理信息文件和固定应用数据

    #region  轨道地理信息文件包括信息头和TrackGIS轨道片记录点的数据
    [Serializable]
    [XmlRoot("TrackInfo")]
    public class TrackInfo
    {
        private TrackFileProperty trackFileProperty;
        private List<Track> tracks;
        private TrackGIS trackGIS;
        [NonSerialized]
        private string error = "";
        public TrackInfo()
        {
            this.TrackFileProperty = new TrackFileProperty();
            this.Tracks = new List<Track>();
            this.TrackGIS = new TrackGIS();

        }
        /// <summary>
        /// 轨道地理信息文件包括信息头和轨道片记录点数据
        /// </summary>
        /// <param name="th">信息头</param>
        /// <param name="tg">轨道片记录点数据</param>
        public TrackInfo(TrackFileProperty th,List<Track> tracks, TrackGIS tg)
        {
            this.TrackFileProperty = th;
            this.Tracks = tracks;
            this.TrackGIS = tg;

        }

        [XmlElement("TrackFileProperty")]
        public TrackFileProperty TrackFileProperty { get { return trackFileProperty; } set { trackFileProperty = value; } }

        [XmlElement("Track")]
        public List<Track> Tracks { get {return tracks; } set {tracks=value; } }

        [XmlElement("TrackGIS")]
        public TrackGIS TrackGIS { get { return trackGIS; } set { trackGIS = value; } }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }
    }

    #region 轨道地理信息头(包含轨道地理信息文件属性和轨道集)


    #region 轨道地理信息文件属性
    [Serializable]
    [XmlRoot("TrackFileProperty")]
    public class TrackFileProperty
    {
        private byte trackFileType;//文件类型固定为1,无符号8位
        private byte trackFileStructVersion;//文件结构版本，从1开始，无符号16位
        private byte trackFileDataVersion;//文件数据版本，从1开始，无符号16位
        private UInt16 stationNum;//车站编号，无符号16位
        private byte trackCount;//轨道个数，无符号8位
        [NonSerialized]
        public string error = "";

        public TrackFileProperty()
        {
            this.trackFileType = 1;
            this.TrackFileStructVersion = 1;
            this.TrackFileDataVersion = 1;
            this.StationNum = 1;
            this.TrackCount = 1;
        }
        /// <summary>
        /// 轨道地理信息文件属性
        /// </summary>
        /// <param name="fsv">文件类型固定为1,无符号8位</param>
        /// <param name="fdv">文件结构版本，从1开始，无符号16位</param>
        /// <param name="sn">文件数据版本，从1开始，无符号16位</param>
        /// <param name="tc">车站编号，无符号16位,不能为0</param>
        /// <param name="ts">轨道个数，无符号8位，不能为0</param>
        public TrackFileProperty(byte fsv, byte fdv, UInt16 sn, byte tc, Track[] ts)
        {
            this.trackFileType = 1;
            this.TrackFileStructVersion = fsv;
            this.TrackFileDataVersion = fdv;
            this.StationNum = sn;
            this.TrackCount = tc;
        }

        [XmlElement("TrackFileType")]
        public byte TrackFileType
        { get { return trackFileType; } }
        [XmlElement("TrackFileStructVersion")]
        public byte TrackFileStructVersion
        {
            get
            {
               return trackFileStructVersion;
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("TrackFileStructVersion={0}——文件结构版本参数超出范围", value) + "\r\n";
                    trackFileStructVersion = 1;
                    //throw new Exception("文件结构版本参数超出范围"); 
                }
                else
                { trackFileStructVersion = value; }
            }
        }

        [XmlElement("TrackFileDataVersion")]
        public byte TrackFileDataVersion
        {
            get
            {
               return trackFileDataVersion;
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("TrackFileDataVersion={0}——文件数据版本超出范围",value)+"\r\n";
                    trackFileDataVersion = 1;
                    //throw new Exception("文件数据版本超出范围");
                }
                else
                { trackFileDataVersion = value; }
            }
        }
        [XmlElement("StationNum")]
        public UInt16 StationNum
        {
            get
            {
               return stationNum;
            }
            set
            {

                if (value == 0)
                {
                    error += string.Format("StationNum={0}——车站编号超出范围", value) + "\r\n";
                    stationNum = 1;
                    //error += "StationNum:车站编号超出范围";
                    ////throw new Exception("车站编号超出范围");
                }
                else
                { stationNum = value; }
            }
        }

        [XmlElement("TrackCount")]
        public byte TrackCount
        {
            get
            {
               return trackCount; 
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("TrackCount={0}——轨道个数超出范围", value) + "\r\n";
                    trackCount = 1;
                }
                else
                {
                    trackCount = value;
                }
            }
        }
        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }


    }
    #endregion 

    #region 轨道属性
    [Serializable]
    [XmlRoot("Track")]
    public class Track
    {
        //字段
        //轨道编号，单位无符号数1-255
        private byte trackID;
        //轨道特性
        private int trackType;
        //轨道特性名称
        private string trackTypeName;
        //起始位置，单位厘米，固定为0
        private uint startMileage;
        //起始纬度，单位毫秒
        private int startLat;
        //起始经度，单位毫秒
        private int startLon;
        //结束位置，单位厘米
        private uint endMileage;
        //结束纬度，单位毫秒
        private int endLat;
        //结束经度，单位毫秒
        private int endLon;
        //第一个轨道片的字节偏移
        private UInt16 startAddress;
        //最后一个轨道片的字节偏移，文件开始地址到当前轨道的最后一个轨道片记录点起始地址的字节偏移
        private UInt16 endAddress;
        [NonSerialized]
        public string error = "";


        public Track()
        {
            this.TrackID = 1;
            this.TrackType = 5;
            this.StartMileage = 0;
            this.StartLat = 0;
            this.StartLon = 0;
            this.EndMileage = 0;
            this.EndLat = 0;
            this.EndLon = 0;
            this.StartAddress = 0;
            this.EndAddress = 0;
        }
        /// <summary>
        /// 轨道数据
        /// </summary>
        /// <param name="ti">轨道编号，单位无符号数1-255</param>
        /// <param name="ty">轨道特性，1-5</param>
        /// <param name="sm">起始位置，单位厘米，固定为0</param>
        /// <param name="sLat">起始纬度，单位毫秒，-324000000-324000000</param>
        /// <param name="sLon">起始经度，单位毫秒，-648000000-648000000</param>
        /// <param name="em">结束位置，单位厘米</param>
        /// <param name="eLat">结束纬度，单位毫秒，-324000000-324000000</param>
        /// <param name="eLon">结束经度，单位毫秒，-648000000-648000000</param>
        /// <param name="sA">第一个轨道片的字节偏移</param>
        /// <param name="eA">最后一个轨道片的字节偏移，文件开始地址到当前轨道的最后一个轨道片记录点起始地址的字节偏移</param>
        public Track(byte ti, int ty, uint sm, int sLat, int sLon, uint em, int eLat, int eLon, UInt16 sA, UInt16 eA)
        {
            this.TrackID = ti;
            this.TrackType = ty;
            this.StartMileage = sm;
            this.StartLat = sLat;
            this.StartLon = sLon;
            this.EndMileage = em;
            this.EndLat = eLat;
            this.EndLon = eLon;
            this.StartAddress = sA;
            this.EndAddress = eA;

        }


        [XmlElement("TrackID")]
        public byte TrackID
        {
            get
            {
                return trackID;
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("TrackID={0}——轨道号参数超出范围", value) + "\r\n";
                    trackID = 1;
                    Console.WriteLine(error);
                }
                else
                { trackID = value; }
            }
        }
        //轨道特性，1-单线正线，2-上行正线，3-下行正线，4-侧线，5-其他
        [XmlElement("TrackType")]
        public int TrackType
        {
            get
            {
                return trackType;
            }
            set
            {
                if (1 <= value && value <= 5)
                {
                    trackType = value;
                }
                else
                {
                    error += string.Format("TrackType={0}——轨道特性参数超出范围", value) + "\r\n";
                    trackType = 5;
                }
            }
        }

        [XmlElement("StartMileage")]
        public uint StartMileage { get { return startMileage; } set { startMileage = value; } }

        [XmlElement("StartLat")]
        public int StartLat
        {
            get
            { return startLat; }
            set
            {
                if (-324000000 <= value && value <= 324000000)
                { startLat = value; }
                else
                {
                    error += string.Format("StartMileage={0}——起始纬度参数超出范围", value) + "\r\n";
                    startLat = 0;
                }
            }
        }
        [XmlElement("StartLon")]
        public int StartLon
        {
            get
            {
               return startLon;
            }
            set
            {
                if (-648000000 <= value && value <= 648000000)
                { startLon = value; }
                else
                {
                    error += string.Format("StartLon={0}——起始经度参数超出范围", value) + "\r\n";
                    startLon = 0;
                }
            }
        }

        [XmlElement("EndMileage")]
        public uint EndMileage { get { return endMileage; } set { endMileage = value; } }

        [XmlElement("EndLat")]
        public int EndLat
        {
            get
            {
                return endLat; 
            }
            set
            {
                if (-324000000 <= value && value <= 324000000)
                { endLat = value; }
                else
                {
                    error += string.Format("EndLat={0}——结束纬度参数超出范围", value) + "\r\n";
                    endLat = 0;
                }
            }
        }

        [XmlElement("EndLon")]
        public int EndLon
        {
            get
            {
                return endLon;
            }
            set
            {
                if (-648000000 <= value && value <= 648000000)
                { endLon = value; }
                else
                {
                    error += string.Format("EndLon={0}——结束经度参数超出范围", value) + "\r\n";
                    endLon = 0;
                }
            }
        }

        [XmlElement("StartAddress")]
        public UInt16 StartAddress { get { return startAddress; } set { startAddress = value; } }

        [XmlElement("EndAddress")]
        public UInt16 EndAddress { get { return endAddress; } set { endAddress = value; } }
        [XmlIgnore]
        public string TrackTypeName
        { get { return trackTypeName; } set { trackTypeName = value; } }
        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }

    }

    #endregion



    #endregion
    #region 轨道片数据(包括多个轨道的轨道片数据)
    [Serializable]
    [XmlRoot("TrackGIS")]
    public class TrackGIS
    {
        private List<TrackPieceGIS> trackPieceGIS;

        [NonSerialized]
        private string error = "==========轨道片地理信息数据=========="+"\r\n";
        public TrackGIS()
        {
            this.TrackPieceGIS = new List<TrackPieceGIS>();
        }

        /// <summary>
        /// 轨道片GIS数据
        /// </summary>
        /// <param name="tp"></param>
        public TrackGIS(List<TrackPieceGIS> tp)
        {
            this.TrackPieceGIS = tp;
        }

        [XmlElement("Track")]
        public List<TrackPieceGIS> TrackPieceGIS { get { return trackPieceGIS; } set { trackPieceGIS = value; } }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }

    }

    #region 每个轨道的轨道片记录数据(包括轨道的ID还有此轨道轨道片的数据)
    [Serializable]
    [XmlRoot("Track")]
    public class TrackPieceGIS
    {
        private int trackID;
        private List<TrackPiece> trackPieces;
        [NonSerialized]
        public string error = "";

        public TrackPieceGIS()
        {
            this.TrackID = 1;
            this.TrackPieces = new List<TrackPiece>();
        }
        /// <summary>
        /// 轨道片数据包括其所属的轨道号
        /// </summary>
        /// <param name="trackid">所属轨道号</param>
        /// <param name="ts">轨道片GIS集</param>
        public TrackPieceGIS(int trackid, List<TrackPiece> ts)
        {
            this.TrackID = trackid;
            this.TrackPieces = ts;
        }
        [XmlElement("TrackID")]
        public int TrackID
        {
            get
            {
                return trackID; 
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("TrackID={0}——轨道片所在轨道号超出范围", value) + "\r\n";
                    trackID = 1;
                    //error += "TrackID:轨道片所在轨道号超出范围";
                    //throw new Exception("轨道片所在轨道号超出范围"); 
                }
                else
                { trackID = value; }
            }
        }

        [XmlElement("TrackPiece")]
        public List<TrackPiece> TrackPieces { get { return trackPieces; } set { trackPieces = value; } }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }
    }
    #endregion

    #region 轨道片记录点属性
    [Serializable]
    [XmlRoot("TrackPiece")]
    public class TrackPiece
    {
        private uint deltaPos;//位置增量，厘米，
        private int deltaLat;//纬度增量，毫秒，该轨道起始纬度到该轨道片记录点纬度的增量
        private int deltaLon;//经度增量，单位毫秒，
        private short deltaHeading;//增量航向角，
        private int adjacentTrack;//邻近轨道，1-是，0-否
        [NonSerialized]
        private string error = "";
        
        public TrackPiece()
        {
            this.DeltaPos = 0;
            this.DeltaLat = 0;
            this.DeltaLon = 0;
            this.DeltaHeading = 0;
            this.AdjacentTrack = 0;

        }
        /// <summary>
        /// 轨道片记录点属性
        /// </summary>
        /// <param name="dPos">位置增量，厘米</param>
        /// <param name="dLat">纬度增量，毫秒，该轨道起始纬度到该轨道片记录点纬度的增量，-8388608-8388607</param>
        /// <param name="dLon">经度增量，单位毫秒，-8388608-8388607</param>
        /// <param name="dH">增量航向角-31416-31416</param>
        /// <param name="aT">邻近轨道，1-是，0-否</param>
        public TrackPiece(uint dPos, int dLat, int dLon, short dH, int aT)
        {
            this.DeltaPos = dPos;
            this.DeltaLat = dLat;
            this.DeltaLon = dLon;
            this.DeltaHeading = dH;
            this.AdjacentTrack = aT;
        }
        [XmlElement("DeltaPos")]
        public uint DeltaPos { get { return deltaPos; } set { deltaPos = value; } }
        [XmlElement("DeltaLat")]
        public int DeltaLat
        {
            get
            { return deltaLat; 
            }
            set
            {
                if (-8388608 <= value && value <= 8388607)
                {
                    deltaLat = value;
                }
                else
                {
                    error += string.Format("DeltaLat={0}——纬度增量参数超出范围", value) + "\r\n";
                    deltaLat = 0;
                    //error += "DeltaLat:纬度增量参数超出范围";
                    //throw new Exception("纬度增量参数超出范围");
                }
            }
        }
        [XmlElement("DeltaLon")]
        public int DeltaLon
        {
            get
            {return deltaLon;
            }
            set
            {
                if (-8388608 <= value && value <= 8388607)
                { deltaLon = value; }
                else
                {
                    error +=string.Format("DeltaLon={0}——经度增量参数超出范围",value) + "\r\n";
                    deltaLon = 0;
                    //error += "DeltaLon:经度增量参数超出范围";
                    //throw new Exception("经度增量参数超出范围");
                }
            }
        }
        [XmlElement("DeltaHeading")]
        public short DeltaHeading
        {
            get
            {
                return deltaHeading;
            }
            set
            {
                if (-31416 <= value && value <= 31416)
                { deltaHeading = value; }
                else
                {
                    error +=string.Format("DeltaHeading={0}——增量航向角参数超出范围",value) + "\r\n";
                    deltaHeading = 0;
                    //error += "DeltaHeading:增量航向角参数超出范围";
                    //throw new Exception("增量航向角参数超出范围");
                }
            }
        }
        [XmlElement("AdjacentTrack")]
        public int AdjacentTrack
        {
            get
            {
                return adjacentTrack;
            }
            set
            {
                if (value == 1 || value == 0)
                { adjacentTrack = value; }
                else
                {
                    error += string.Format("AdjacentTrack={0}——邻近轨道参数超出范围",value) + "\r\n";
                    adjacentTrack = 0;
                    //error += "AdjacentTrack:邻近轨道参数超出范围";
                    //throw new Exception("邻近轨道参数超出范围");
                }
            }
        }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }

    }
    #endregion

    #endregion

    #endregion
    #endregion


    #region 固定应用数据（管辖边界、应答器、道岔）

    [Serializable]
    [XmlRoot("TrackControlData")]
    public class TrainControlData
    {
        private TrainControlDataProperty trainControlDataProperty;//数据头
        private StationEdgeData stationEdgeData;//管辖边界数据
        private SwitchData switchData;//道岔数据
        private BaliseData baliseData;//应答器数据
        [NonSerialized]
        private string error = "==========TrackControlData固定应用数据==========";

        public TrainControlData()
        {
            this.TrainControlDataProperty = new TrainControlDataProperty();
            this.StationEdgeData = new StationEdgeData();
            this.SwitchData = new SwitchData();
            this.BaliseData = new BaliseData();
        }
        /// <summary>
        /// 固定应用数据(包括数据头，管辖边界数据，道岔数据，应答器数据)
        /// </summary>
        /// <param name="tc">数据头-固定应用数据属性</param>
        /// <param name="s">车站管辖边界数据</param>
        /// <param name="sd">道岔数据</param>
        /// <param name="bd">应答器数据</param>
        public TrainControlData(TrainControlDataProperty tc, StationEdgeData s, SwitchData sd, BaliseData bd)
        {
            this.TrainControlDataProperty = tc;
            this.StationEdgeData = s;
            this.SwitchData = sd;
            this.BaliseData = bd;
        }

        [XmlElement("TrainControlDataProperty")]
        public TrainControlDataProperty TrainControlDataProperty { get { return trainControlDataProperty; } set { trainControlDataProperty = value; } }

        [XmlElement("StationEdgeData")]
        public StationEdgeData StationEdgeData { get { return stationEdgeData; } set { stationEdgeData = value; } }
        [XmlElement("SwitchData")]
        public SwitchData SwitchData { get { return switchData; } set { switchData = value; } }

        [XmlElement("BaliseData")]
        public BaliseData BaliseData { get { return baliseData; } set { baliseData = value; } }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }
    }
    #region 固定应用数据头
    [Serializable]
    [XmlRoot("TrainControlDataProperty")]
    public class TrainControlDataProperty
    {
        private byte fileType;//文件类型，无符号8位，固定为2
        private UInt16 fileStructVersion;//文件结构版本，无符号16位,每次变化加一
        private UInt16 fileDataVersion;//文件数据版本，无符号16位，每次变化加一
        private UInt16 stationNum;//车站编号，无符号8位
        private byte startEdgeCount;// 起始管辖边界数目，无符号8位
        private List<StartEdgeInfo> startEdgeInfo;//起始管辖边界1-N
        private byte endEdgeCount;//结束管辖边界数目，无符号8位
        private List<EndEdgeInfo> endEdgeInfo;//结束管辖边界1-N
        private UInt16 baliseDataByteOffset;//应答器数据开始的字节偏移，无符号16位
        [NonSerialized]
        private string error = "";
        public TrainControlDataProperty()
        {
            this.fileType = 2;
            this.FileStructVersion = 1;
            this.FileDataVersion = 1;
            this.StationNum = 1;
            this.StartEdgeCount = 1;
            this.StartEdgeInfo = new List<StartEdgeInfo>();
            this.EndEdgeCount = 1;
            this.EndEdgeInfo = new List<EndEdgeInfo>();
            this.BaliseDataByteOffset = 1;
        }
        /// <summary>
        /// 固定应用数据头，文件类型固定为2
        /// </summary>
        /// <param name="filestructversion">文件结构版本，无符号16位,从1开始每次变化加一</param>
        /// <param name="filedataversion">文件数据版本，无符号16位，从1开始每次变化加一</param>
        /// <param name="stationnum">车站编号，无符号8位，不能为0</param>
        /// <param name="startedgecount">起始管辖边界数目，无符号8位，不能为0</param>
        /// <param name="startedgeinfo">起始管辖边界1-N，起始管辖边界数据集</param>
        /// <param name="endedgeCount">结束管辖边界数目，无符号8位，不能为0</param>
        /// <param name="endedgeinfo">结束管辖边界1-N，结束管辖边界数据集</param>
        /// <param name="balisedatabyteoffset">应答器数据开始的字节偏移，无符号16位</param>
        public TrainControlDataProperty(UInt16 filestructversion, UInt16 filedataversion, UInt16 stationnum, byte startedgecount, List<StartEdgeInfo> startedgeinfo, byte endedgeCount, List<EndEdgeInfo> endedgeinfo, UInt16 balisedatabyteoffset)
        {
            this.fileType = 2;
            this.FileStructVersion = filestructversion;
            this.FileDataVersion = filedataversion;
            this.StationNum = stationnum;
            this.StartEdgeCount = startedgecount;
            this.StartEdgeInfo = startedgeinfo;
            this.EndEdgeCount = endedgeCount;
            this.EndEdgeInfo = endedgeinfo;
            this.BaliseDataByteOffset = balisedatabyteoffset;
        }

        [XmlElement("FileType")]
        public byte FileType { get { return fileType; } }
        [XmlElement("FileStructVersion")]
        public UInt16 FileStructVersion
        {
            get
            {
                return fileStructVersion;
            }
            set
            {

                if (value == 0)
                {
                    error += string.Format("FileStructVersion={0}——文件结构版本参数超出范围", value) + "\r\n";
                    fileStructVersion = 1;
                    //error += "FileStructVersion:文件结构版本参数超出范围";
                    //throw new Exception("文件结构版本参数超出范围");
                }
                else
                {
                    fileStructVersion = value;
                }
            }
        }
        [XmlElement("FileDataVersion")]
        public UInt16 FileDataVersion
        {
            get
            {
                return fileDataVersion;
            }
            set
            {

                if (value == 0)
                {
                    error += string.Format("FileDataVersion={0}——文件数据版本参数超出范围",value) + "\r\n";
                    fileDataVersion = 1;
                    //error += "FileDataVersion:文件数据版本参数超出范围";
                    //throw new Exception("文件数据版本参数超出范围");
                }
                else
                { fileDataVersion = value; }
               
            }
        }
        [XmlElement("StationNum")]
        public UInt16 StationNum
        {
            get
            {
                return stationNum;
            }
            set
            {

                if (value == 0)
                {
                    error += string.Format("StationNum={0}——车站编号参数超出范围",value) + "\r\n";
                    stationNum = 1;
                    //error += "StationNum:车站编号参数超出范围";
                    //throw new Exception("车站编号参数超出范围");
                }
                else
                { stationNum = value; }
                    
            }
        }
        [XmlElement("StartEdgeCount")]
        public byte StartEdgeCount
        {
            get
            {
                return startEdgeCount;
            }
            set
            {

                if (value == 0)
                {
                    error += string.Format("StartEdgeCount={0}——起始管辖边界数目参数超出范围",value) + "\r\n";
                    startEdgeCount = 1;
                    //error += "StartEdgeCount:起始管辖边界数目参数超出范围";
                    //throw new Exception("起始管辖边界数目参数超出范围"); 
                }
                else
                { startEdgeCount = value; }
            }
        }

        [XmlElement("StartEdge")]
        public List<StartEdgeInfo> StartEdgeInfo { get { return startEdgeInfo; } set { startEdgeInfo = value; } }
        [XmlElement("EndEdgeCount")]
        public byte EndEdgeCount
        {
            get
            {
                return endEdgeCount;
            }
            set
            {

                if (value == 0)
                {
                    error += string.Format("EndEdgeCount={0}——结束管辖边界数目超出范围",value) + "\r\n";
                    endEdgeCount = 1;
                    //error += "EndEdgeCount:结束管辖边界数目超出范围";
                    //throw new Exception("结束管辖边界数目超出范围");
                }
                else
                {
                    endEdgeCount = value;
                }
            }
        }

        [XmlElement("EndEdge")]
        public List<EndEdgeInfo> EndEdgeInfo { get { return endEdgeInfo; } set { endEdgeInfo = value; } }

        public UInt16 BaliseDataByteOffset
        {
            get
            {
                return baliseDataByteOffset;
            }
            set
            {

                if (value == 0)
                {
                    error += string.Format("BaliseDataByteOffset={0}——应答器数据开始的字节偏移超出范围", value) + "\r\n";
                    baliseDataByteOffset = 1;
                    //throw new Exception("应答器数据开始的字节偏移超出范围");
                }
                else
                {
                    baliseDataByteOffset = value;
                }
            }
        }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }

    }
    [Serializable]
    [XmlRoot("StartEdge")]
    public class StartEdgeInfo
    {
        private byte startEdgeID;//起始管辖边界的ID，无符号数8位，不能为0
        private byte trackNum;//起始管辖边界的轨道号，无符号数8位，不能为0
        private UInt16 byteOffset;//起始管辖边界的字节偏移，无符号16位
        [NonSerialized]
        private string error = "";
        public StartEdgeInfo()
        {
            this.StartEdgeID = 1;
            this.TrackNum = 1;
            this.ByteOffset = 0;
        }
        /// <summary>
        /// 起始管辖边界数据info
        /// </summary>
        /// <param name="startedgeId">起始管辖边界的ID，无符号数8位，不能为0</param>
        /// <param name="tracknum">起始管辖边界的轨道号，无符号数8位，不能为0</param>
        /// <param name="byteoffset">起始管辖边界的字节偏移，无符号16位</param>
        public StartEdgeInfo(byte startedgeId, byte tracknum, UInt16 byteoffset)
        {
            this.StartEdgeID = startedgeId;
            this.TrackNum = tracknum;
            this.ByteOffset = byteoffset;
        }
        [XmlElement("StartEdgeID")]
        public byte StartEdgeID
        {
            get
            { return startEdgeID; }
            set
            {
                try
                {
                    if (value == 0)
                    {
                        error += string.Format("StartEdgeID={0}——起始管辖边界编号超出范围", value) + "\r\n";
                        startEdgeID = 1;
                        //error += "StartEdgeID:起始管辖边界编号超出范围";
                        //throw new Exception("起始管辖边界编号超出范围");
                    }
                    else
                    { startEdgeID = value; }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        [XmlElement("TrackNum")]
        public byte TrackNum
        {
            get
            {
                return trackNum;
            }
            set
            {

                if (value == 0)
                {
                    error += string.Format("TrackNum={0}——起始管辖边界的轨道号超出范围",value) + "\r\n";
                    trackNum = 1;
                    //error += "TrackNum:起始管辖边界的轨道号超出范围";
                    //throw new Exception("起始管辖边界的轨道号超出范围");
                }
                else
                { trackNum = value; }
            }
        }

        [XmlElement("ByteOffset")]
        public UInt16 ByteOffset
        {
            get { return byteOffset; }
            set
            {
                byteOffset = value;
            }
        }
        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }

    }
    [Serializable]
    [XmlRoot("EndEdge")]
    public class EndEdgeInfo
    {
        private byte endEdgeID;//结束管辖边界的ID，无符号数8位，不能为0
        private byte trackNum;//结束管辖边界的轨道号，无符号数8位，不能为0
        private UInt16 byteOffset;//结束管辖边界的字节偏移，无符号16位
        [NonSerialized]
        private string error = "";

        public EndEdgeInfo()
        {
            this.EndEdgeID = 1;
            this.TrackNum = 1;
            this.ByteOffset = 0;
            //error = "";
        }
        /// <summary>
        /// 结束管辖边界数据info
        /// </summary>
        /// <param name="endedgeId">起始管辖边界的ID，无符号数8位，不能为0</param>
        /// <param name="tracknum">起始管辖边界的轨道号，无符号数8位，不能为0</param>
        /// <param name="byteoffset">起始管辖边界的字节偏移，无符号16位</param>
        public EndEdgeInfo(byte endedgeId, byte tracknum, UInt16 byteoffset)
        {
            this.EndEdgeID = endedgeId;
            this.TrackNum = tracknum;
            this.ByteOffset = byteoffset;
            //error = "";
        }
        [XmlElement("EndEdgeID")]
        public byte EndEdgeID
        {
            get
            {

                return endEdgeID;
            }
            set
            {

                if (value == 0)
                {
                    error += string.Format("EndEdgeID={0}——结束管辖边界号超出范围", value) + "\r\n";
                    endEdgeID = 1;
                    //throw new Exception("结束管辖边界号超出范围");
                }
                else
                { endEdgeID = value; }
            }
        }

        [XmlElement("TrackNum")]
        public byte TrackNum
        {
            get
            { return trackNum; }
            set
            {

                if (value == 0)
                {
                    error +=string.Format("TrackNum={0}——结束管辖边界所在的轨道号超出范围",value) + "\r\n";
                    trackNum = 1;
                    //error += "TrackNum:结束管辖边界所在的轨道号超出范围";
                    //throw new Exception("结束管辖边界所在的轨道号超出范围");
                }
                else
                { trackNum = value; }
            }
        }

        [XmlElement("ByteOffset")]
        public UInt16 ByteOffset
        { get { return byteOffset; } set { byteOffset = value; } }
        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }
    }

    #endregion

    #region 车站管辖边界数据
    [Serializable]
    [XmlRoot("StationEdgeData")]
    public class StationEdgeData
    {
        private List<StartEdge> startEdges;//车站起始管辖边界数据
        private List<EndEdge> endEdges;//车站结束管辖边界数据
        [NonSerialized]
        public string error = "==========StationEdgeData车站管辖边界数据==========";
        public StationEdgeData()
        {
            this.StartEdges = new List<StartEdge>();
            this.EndEdges = new List<EndEdge>();
        }
        /// <summary>
        /// 车站管辖边界数据包括起始管辖边界数据集和结束管辖边界数据集
        /// </summary>
        /// <param name="startedges">车站起始管辖边界数据集</param>
        /// <param name="endedges">车站结束管辖边界数据集</param>
        public StationEdgeData(List<StartEdge> startedges, List<EndEdge> endedges)
        {
            this.StartEdges = startedges;
            this.EndEdges = endedges;
        }
        [XmlElement("StartEdge")]
        public List<StartEdge> StartEdges { get { return startEdges; } set { startEdges = value; } }

        [XmlElement("EndEdge")]
        public List<EndEdge> EndEdges { get { return endEdges; } set { endEdges = value; } }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }
    }
    [Serializable]
    [XmlRoot("StartEdge")]
    public class StartEdge
    {
        private byte type;//条目类型，无符号8位，0x00-上行线的起始管辖边界，0x01-下行线的起始管辖边界
        private uint adjacentDataOffset;//相邻数据对象的字节偏移，无符号16位
        private uint adjacentTsrsNum;//相邻车站所在Tsrs设备编号，无符号32位
        private UInt16 adjacentStationNum;//相邻车站编号，无符号16位
        private byte adjacentEdgeType;//相邻车站管辖边界类型，无符号8位
        /*0x00-上行线的开始管辖边界0
          0x01-下行线的开始管辖边界 1
          0xFF-上行线的结束管辖边界 255
          0xFE-下行线的结束管辖边界 254
          0x55-未知*/

        private byte adjacentTrackNum;//相邻车站轨道号，无符号8位，如果没有用0
        private uint position;//位置，单位厘米，无符号32位
        private byte edgeTrackNum;//管辖边界所在轨道号，无符号8位
        private byte byteCount;//总字节数=16
        [NonSerialized]
        private string error;

        public StartEdge()
        {
            this.Type = 0;
            this.AdjacentDataOffset = 0;
            this.AdjacentTsrsNum = 0;
            this.AdjacentStationNum = 0;
            this.AdjacentEdgeType = 85;
            this.AdjacentTrackNum = 0;
            this.Position = 0;
            this.EdgeTrackNum = 1;
            this.byteCount = 16;
        }

        /// <summary>
        /// 起始管辖边界数据，总字节数16
        /// </summary>
        /// <param name="ty">条目类型，无符号8位，0x00-上行线的起始管辖边界，0x01-下行线的起始管辖边界，只能为0或1</param>
        /// <param name="ado">相邻数据对象的字节偏移，无符号16位</param>
        /// <param name="atsn">相邻车站所在Tsrs设备编号，无符号32位</param>
        /// <param name="asn">相邻车站编号，无符号16位</param>
        /// <param name="aet">相邻车站管辖边界类型，无符号8位，0x00-上行线的开始管辖边界(0)，0x01-下行线的开始管辖边界(1),0xFF-上行线的结束管辖边界(255), ,0xFE-下行线的结束管辖边界(254),0x55-未知(85)</param>
        /// <param name="atn">相邻车站轨道号，无符号8位，如果没有用0</param>
        /// <param name="pos">位置，单位厘米，无符号32位</param>
        /// <param name="etn">管辖边界所在轨道号，无符号8位,不能为0</param>
        public StartEdge(byte ty, uint ado, uint atsn, UInt16 asn, byte aet, byte atn, uint pos, byte etn)
        {
            this.Type = ty;
            this.AdjacentDataOffset = ado;
            this.AdjacentTsrsNum = atsn;
            this.AdjacentStationNum = asn;
            this.AdjacentEdgeType = aet;
            this.AdjacentTrackNum = atn;
            this.Position = pos;
            this.EdgeTrackNum = etn;
            this.byteCount = 16;
        }

        public string GetEnumNameByValue<T>(int Key)
        {
            return Enum.GetName(typeof(T), Key);
        }
        [XmlElement("Type")]
        public byte Type
        {
            get
            {
                return type;
            }
            set
            {
                if (value == 1 || value == 0)
                { type = value; }
                else
                {
                    error += string.Format("Type={0}——条目类型参数超出范围", value) + "\r\n";
                    type = 0;
                    //throw new Exception("条目类型参数超出范围"); 
                }
            }
        }

        /// <summary>
        /// 属于相邻轨道类型则为真
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool judgeAdjacentEdgeType(byte value)
        {
            bool flag = false;
            if (value == 0 || value == 1)
            { flag = true; }
            else if (value == 254 || value == 255)
            {
                flag = true;
            }
            else if (value == 85)
            {
                flag = true;
            }
            return flag;

        }

        [XmlElement("AdjacentDataOffset")]
        public uint AdjacentDataOffset { get { return adjacentDataOffset; } set { adjacentDataOffset = value; } }
        [XmlElement("AdjacentTsrsNum")]
        public uint AdjacentTsrsNum { get { return adjacentTsrsNum; } set { adjacentTsrsNum = value; } }
        [XmlElement("AdjacentStationNum")]
        public UInt16 AdjacentStationNum { get { return adjacentStationNum; } set { adjacentStationNum = value; } }
        [XmlElement("AdjacentEdgeType")]
        public byte AdjacentEdgeType
        {
            get
            {
                return adjacentEdgeType;
            }
            set
            {
                if (judgeAdjacentEdgeType(value))
                { adjacentEdgeType = value; }
                else
                {
                    error += string.Format("AdjacentEdgeType={0}——相邻车站管辖边界类型超出范围",value) + "\r\n";
                    adjacentEdgeType = 85;
                    //throw new Exception("相邻车站管辖边界类型超出范围");
                }
            }
        }
        [XmlElement("AdjacentTrackNum")]
        public byte AdjacentTrackNum
        {
            get { return adjacentTrackNum; }
            set { adjacentTrackNum = value; }
        }
        [XmlElement("Position")]
        public uint Position { get { return position; } set { position = value; } }
        [XmlElement("EdgeTrackNum")]
        public byte EdgeTrackNum
        {
            get
            {
                return edgeTrackNum;
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("EdgeTrackNum={0}——起始管辖边界所在轨道号",value) + "\r\n";
                    edgeTrackNum = 1;
                    //throw new Exception("起始管辖边界所在轨道号");
                }
                else
                { edgeTrackNum = value; }
            }
        }
        public byte ByteCount { get { return byteCount; } }
        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }

    }
    [Serializable]
    [XmlRoot("EndEdge")]
    public class EndEdge
    {
        private byte type;//条目类型，无符号8位，0xFF-上行线的结束管辖边界-255,0xFE-下行线的结束管辖边界-254
        private uint adjacentDataOffset;//相邻数据对象的字节偏移，无符号16位
        private uint adjacentTsrsNum;//相邻车站所在Tsrs设备编号，无符号32位
        private UInt16 adjacentStationNum;//相邻车站编号，无符号16位
        private byte adjacentEdgeType;//相邻车站管辖边界类型，无符号8位
        private byte adjacentTrackNum;//相邻车站轨道号，无符号8位，如果没有用0
        private uint position;//位置，单位厘米，无符号32位
        private byte edgeTrackNum;//管辖边界所在轨道号，无符号8位
        private byte byteCount;//总字节数=16
        [NonSerialized]
        private string error = "";

        public EndEdge()
        {
            this.Type = 255;
            this.AdjacentDataOffset = 0;
            this.AdjacentTsrsNum = 0;
            this.AdjacentStationNum = 0;
            this.AdjacentEdgeType = 85;
            this.AdjacentTrackNum = 0;
            this.Position = 0;
            this.EdgeTrackNum = 1;
            this.byteCount = 16;
        }

        /// <summary>
        /// 结束管辖边界数据，总字节为16
        /// </summary>
        /// <param name="ty">条目类型，无符号8位，0xFF-上行线的结束管辖边界-255,0xFE-下行线的结束管辖边界-254</param>
        /// <param name="ado">相邻数据对象的字节偏移，无符号16位</param>
        /// <param name="atsn">相邻车站所在Tsrs设备编号，无符号32位</param>
        /// <param name="asn">相邻车站编号，无符号16位</param>
        /// <param name="aet">相邻车站管辖边界类型，无符号8位，0x00-上行线的开始管辖边界(0)，0x01-下行线的开始管辖边界(1),0xFF-上行线的结束管辖边界(255), ,0xFE-下行线的结束管辖边界(254),0x55-未知(85)</param>
        /// <param name="atn">相邻车站轨道号，无符号8位，如果没有用0</param>
        /// <param name="pos">位置，单位厘米，无符号32位</param>
        /// <param name="etn">管辖边界所在轨道号，无符号8位，不能为0</param>
        public EndEdge(byte ty, uint ado, uint atsn, UInt16 asn, byte aet, byte atn, uint pos, byte etn)
        {
            this.Type = ty;
            this.AdjacentDataOffset = ado;
            this.AdjacentTsrsNum = atsn;
            this.AdjacentStationNum = asn;
            this.AdjacentEdgeType = aet;
            this.AdjacentTrackNum = atn;
            this.Position = pos;
            this.EdgeTrackNum = etn;
            this.byteCount = 16;
        }

        private bool judgeAdjacentEdgeType(byte value)
        {
            bool flag = false;
            if (value == 0 || value == 1)
            { flag = true; }
            else if (value == 254 || value == 255)
            {
                flag = true;
            }
            else if (value == 85)
            {
                flag = true;
            }
            return flag;

        }
        public string GetEnumNameByValue<T>(int Key)
        {
            return Enum.GetName(typeof(T), Key);
        }
        [XmlElement("Type")]
        public byte Type
        {
            get
            {
                return type;
            }
            set
            {
                if (value == 254 || value == 255)
                { type = value; }
                else
                {
                    error += string.Format("Type={0}——条目类型超出范围",value) + "\r\n";
                    type = 255;
                    //throw new Exception("条目类型超出范围");
                }
            }

        }

        [XmlElement("AdjacentDataOffset")]
        public uint AdjacentDataOffset { get { return adjacentDataOffset; } set { adjacentDataOffset = value; } }
        [XmlElement("AdjacentTsrsNum")]
        public uint AdjacentTsrsNum { get { return adjacentTsrsNum; } set { adjacentTsrsNum = value; } }
        [XmlElement("AdjacentStationNum")]
        public UInt16 AdjacentStationNum { get { return adjacentStationNum; } set { adjacentStationNum = value; } }
        [XmlElement("AdjacentEdgeType")]
        public byte AdjacentEdgeType
        {
            get
            {
                return adjacentEdgeType;
            }
            set
            {
                if (judgeAdjacentEdgeType(value))
                { adjacentEdgeType = value; }
                else
                {
                    error += string.Format("AdjacentEdgeType={0}——相邻车站管辖边界类型超出范围", value) + "\r\n";
                    adjacentEdgeType = 85;
                    //throw new Exception("相邻车站管辖边界类型超出范围");
                }
            }
        }
        [XmlElement("AdjacentTrackNum")]
        public byte AdjacentTrackNum { get { return adjacentTrackNum; } set { adjacentTrackNum = value; } }
        [XmlElement("Position")]
        public uint Position { get { return position; } set { position = value; } }
        [XmlElement("EdgeTrackNum")]
        public byte EdgeTrackNum
        {
            get
            {
                return edgeTrackNum;
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("EdgeTrackNum={0}——结束管辖编辑所在轨道号超出范围",value) + "\r\n";
                    edgeTrackNum = 1;
                    //throw new Exception("结束管辖编辑所在轨道号超出范围");
                }
                else
                { edgeTrackNum = value; }
            }
        }
        public byte ByteCount { get { return byteCount; } }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }
    }
    #endregion
    [Serializable]
    [XmlRoot("SwitchData")]
    public class SwitchData
    {
        private List<Switch> switches;
        [NonSerialized]
        private string error = "";
        public SwitchData()
        {
            this.Switches = new List<Switch>();
        }

        public SwitchData(List<Switch> sws)
        {
            this.Switches = sws;
        }

        [XmlElement("Switch")]
        public List<Switch> Switches { get { return switches; } set { switches = value; } }
        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }
    }
    [Serializable]
    [XmlRoot("Switch")]
    public class Switch
    {
        private byte type;//条目类型，无符号8位，取值范围 0x06
        private byte switchNum;//道岔编号，无符号8位
        private byte switchDirection;
        //道岔方向，无符号8位，0岔尖所在轨道的位置偏移减小的方向（上行方向），1-偏移增长方向，下行方向
        private uint switchPOI;//道岔岔尖位置，厘米，无符号32位
        private uint switchPos;//道岔定位位置，厘米，无符号32位
        private uint switchReversePos;//道岔反位位置，厘米，无符号32位
        private UInt16 lastOffset;//岔前连接的数据对象的偏移，无符号16位
        private UInt16 switchPosOffset;//定位连接的数据对象的偏移，无符号16位
        private UInt16 nextOffset;//反位连接的数据对象的偏移，无符号16位
        private byte preSwitchTrackNum;//道岔岔前所在轨道，无符号8位
        private byte switchPosTrackNum;//道岔定位所在轨道，无符号8位
        private byte switchReverseTrackNum;//道岔反位所在轨道，无符号8位
        private byte byteCount;//无符号8位，恒为24即24字节
        [NonSerialized]
        private string error = "";

        public Switch()
        {
            this.Type = 0;
            this.SwitchNum = 1;
            this.SwitchDirection = 0;
            this.SwitchPOI = 0;
            this.SwitchPos = 0;
            this.SwitchReversePos = 0;
            this.LastOffset = 0;
            this.SwitchPosOffset = 0;
            this.NextOffset = 0;
            this.PreSwitchTrackNum = 1;
            this.SwitchPosTrackNum = 1;
            this.SwitchReverseTrackNum = 1;
            this.byteCount = 24;

        }

        /// <summary>
        /// 道岔数据，总字节数24
        /// </summary>
        /// <param name="t">条目类型，0-6</param>
        /// <param name="sn">道岔编号，无符号8位，不能为0</param>
        /// <param name="sd">道岔岔尖方向0=上行方向，1=下行方向</param>
        /// <param name="sPOI">道岔岔尖位置</param>
        /// <param name="sP">道岔定位位置</param>
        /// <param name="srP">道岔反位位置</param>
        /// <param name="lo">岔前连接的数据对象的偏移</param>
        /// <param name="spo">定位连接的数据对象的偏移</param>
        /// <param name="no">反位连接的数据对象的偏移</param>
        /// <param name="pstn">道岔岔前所在的轨道，不能为0</param>
        /// <param name="sptn">道岔定位所在轨道，不能为0</param>
        /// <param name="srtn">道岔反位所在轨道，不能为0</param>
        public Switch(byte t, byte sn, byte sd, uint sPOI, uint sP, uint srP, UInt16 lo, UInt16 spo, UInt16 no, byte pstn, byte sptn, byte srtn)
        {
            this.Type = t;
            this.SwitchNum = sn;
            this.SwitchDirection = sd;
            this.SwitchPOI = sPOI;
            this.SwitchPos = sP;
            this.SwitchReversePos = srP;
            this.LastOffset = lo;
            this.SwitchPosOffset = spo;
            this.NextOffset = no;
            this.PreSwitchTrackNum = pstn;
            this.SwitchPosTrackNum = sptn;
            this.SwitchReverseTrackNum = srtn;
            this.byteCount = 24;
        }


        [XmlElement("Type")]
        public byte Type
        {
            get
            {
                return type;
            }
            set
            {
                if (value > 6)
                {
                    error+=string.Format("Type={0}——条目类型超出范围",value) + "\r\n";
                    type = 0;
                    //throw new Exception("条目类型超出范围"); 
                }
                else
                { type = value; }
            }

        }
        [XmlElement("SwitchNum")]
        public byte SwitchNum
        {
            get
            {
                return switchNum;
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("SwitchNum={0}——道岔编号超出范围", value) + "\r\n";
                    switchNum = 1;
                    //throw new Exception("道岔编号超出范围");
                }
                else
                { switchNum = value; }
            }
        }
        [XmlElement("SwitchDirection")]
        public byte SwitchDirection
        {
            get
            {
                return switchDirection;
            }
            set
            {
                if (value > 1)
                {
                    error+=string.Format("SwitchDirection={0}——道岔方向参数超出范围",value) + "\r\n";
                    switchDirection = 0;
                    //throw new Exception("道岔方向参数超出范围"); 
                }
                else
                { switchDirection = value; }
            }
        }
        [XmlElement("SwitchPOI")]
        public uint SwitchPOI { get { return switchPOI; } set { switchPOI = value; } }
        [XmlElement("SwitchPos")]
        public uint SwitchPos { get { return switchPos; } set { switchPos = value; } }
        [XmlElement("SwitchReversePos")]
        public uint SwitchReversePos { get { return switchReversePos; } set { switchReversePos = value; } }
        [XmlElement("LastOffset")]
        public UInt16 LastOffset { get { return lastOffset; } set { lastOffset = value; } }
        [XmlElement("SwitchPosOffset")]
        public UInt16 SwitchPosOffset { get { return switchPosOffset; } set { switchPosOffset = value; } }
        [XmlElement("NextOffset")]
        public UInt16 NextOffset { get { return nextOffset; } set { nextOffset = value; } }
        [XmlElement("PreSwitchTrackNum")]
        public byte PreSwitchTrackNum
        {
            get
            {
                return preSwitchTrackNum;
            }
            set
            {
                if (value == 0)
                {
                    error+=string.Format("PreSwitchTrackNum={0}——道岔岔前所在轨道号超出范围",value) + "\r\n";
                    preSwitchTrackNum = 1;
                    //throw new Exception("道岔岔前所在轨道号超出范围"); 
                }
                else
                { preSwitchTrackNum = value; }
            }
        }
        [XmlElement("SwitchPosTrackNum")]
        public byte SwitchPosTrackNum
        {
            get
            {
                return switchPosTrackNum;
            }
            set
            {
                if (value == 0)
                {
                    error+=string.Format("SwitchPosTrackNum={0}——道岔定位所在轨道号超出范围",value) + "\r\n";
                    switchPosTrackNum = 1;
                    //throw new Exception("道岔定位所在轨道号超出范围"); 
                }
                else
                { switchPosTrackNum = value; }
            }
        }
        [XmlElement("SwitchReverseTrackNum")]
        public byte SwitchReverseTrackNum
        {
            get
            {
                return switchReverseTrackNum;
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("SwitchReverseTrackNum={0}——道岔反位所在轨道号超出范围", value) + "\r\n";
                    switchReverseTrackNum = 1;
                    //throw new Exception("道岔反位所在轨道号超出范围"); 
                }
                else
                { switchReverseTrackNum = value; }
            }
        }
        [XmlIgnore]
        public byte ByteCount { get; }
        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }

    }


    [Serializable]
    [XmlRoot("BaliseData")]
    public class BaliseData
    {
        private List<Balise> balises;
        [NonSerialized]
        private string error = "";
        public BaliseData()
        {
            this.Balises = new List<Balise>();
        }

        public BaliseData(List<Balise> bs)
        {
            this.Balises = bs;
        }

        [XmlElement("Balise")]
        public List<Balise> Balises { get { return balises; } set { balises = value; } }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }
    }
    [Serializable]
    [XmlRoot("Balise")]
    public class Balise
    {
        private byte type;//条目类型，无符号8位，取值范围 0x33--0-51
        private UInt16 lastOffset;//岔前连接的数据对象的偏移，无符号16位
        private UInt16 nextOffset;//反位连接的数据对象的偏移，无符号16位
        private uint baliseNum;//无符号24位，范围4294967295
        private byte dataSize;//数据大小，无符号8位，1-255
        private byte baliseProperty;//应答器属性和所在应答器组方向与位置偏移增加方向关系
        /*低2位应答器属性 
         00=虚拟应答器，01=实体应答器，01=实体应答器，10=带区间数据的应答器，11=预留
         高2位所在应答器组方向与位置增加方向关系
         00=双向，01=相反方向，10=相同，11=预留
         中间四位固定为0000*/

        private uint balisePos;//应答器位置，无符号32位
        private byte baliseTrackNum;//应答器所在轨道号，无符号8位
        private byte baliseLoc;//本应答器在应答器组中的位置及应答器组中所包含的应答器数量
        /*低3位本应答器在应答器组中的位置，000=1，111=8
         高3位应答器组中所包含的应答器数量，000=1，111=8
         中间2位固定为0*/

        private uint baliseDataPort;//应答器报文，N字节,无符号数,报文长度不超过104字节
        [NonSerialized] 
        private string error = "";
        public Balise()
        {
            this.Type = 0;
            this.LastOffset = 0;
            this.NextOffset = 0;
            this.BaliseNum = 1;
            this.DataSize = 1;
            this.BaliseProperty = 0;
            this.BalisePos = 0;
            this.BaliseTrackNum = 1;
            this.BaliseLoc = 0;
            this.BaliseDataPort = 0;
        }

        /// <summary>
        /// 设置应答器数据
        /// </summary>
        /// <param name="t">条目类型0-51</param>
        /// <param name="lastoffset">上一个数据对象的字节偏移</param>
        /// <param name="nextoffset"> 下一个数据对象的字节偏移</param>
        /// <param name="balisenum"> 应答器编号1-4294967295</param>
        /// <param name="datasize">数据大小，不能为0，1-255</param>
        /// <param name="baliseproperty">应答器属性、位置设置，应答器属性和所在应答器组方向与位置偏移增加方向关系，低2位应答器属性，00=虚拟应答器，01=实体应答器，01=实体应答器，10=带区间数据的应答器，11=预留，高2位所在应答器组方向与位置增加方向关系，00=双向，01=相反方向，10=相同，11=预留，中间四位固定为0000；设置范围：0-3，64-67，128-131，192-195</param>
        /// <param name="balisepos">应答器位置</param>
        /// <param name="balisetrackNum">应答器所在轨道号，不能为0</param>
        /// <param name="baliseloc">应答器在组中位置本应答器在应答器组中的位置及应答器组中所包含的应答器数量,低3位本应答器在应答器组中的位置，000=1，111=8,高3位应答器组中所包含的应答器数量，000=1，111=8,中间2位固定为0，取值范围：0-3，64-67，128-131，192-195</param>
        /// <param name="balisedataport">应答器报文N字节，暂时设置为uint</param>
        public Balise(byte t, UInt16 lastoffset, UInt16 nextoffset, UInt32 balisenum, byte datasize, byte baliseproperty, uint balisepos, byte balisetrackNum, byte baliseloc, uint balisedataport)
        {
            this.Type = t;
            this.LastOffset = lastoffset;
            this.NextOffset = nextoffset;
            this.BaliseNum = balisenum;
            this.DataSize = datasize;
            this.BaliseProperty = baliseproperty;
            this.BalisePos = balisepos;
            this.BaliseTrackNum = balisetrackNum;
            this.BaliseLoc = baliseloc;
            this.BaliseDataPort = balisedataport;
        }

        [XmlElement("Type")]
        public byte Type
        {
            get
            {
                return type;
            }
            set
            {

                if (value > 51)
                {
                    error+=string.Format("Type={0}——条目类型超出范围",value) + "\r\n";
                    type = 0;
                    //throw new Exception("条目类型超出范围");
                }
                else
                { type = value; }
            }
        }
        [XmlElement("LastOffset")]
        public UInt16 LastOffset { get { return lastOffset; } set { lastOffset = value; } }
        [XmlElement("NextOffset")]
        public UInt16 NextOffset { get { return nextOffset; } set { nextOffset = value; } }

        [XmlElement("BaliseNum")]
        public uint BaliseNum
        {
            get
            {
                return baliseNum;
            }
            set
            {
                if (value > 4294967295 || value == 0)
                {
                    error+=string.Format("BaliseNum={0}——应答器编号超出范围",value) + "\r\n";
                    baliseNum = 0;
                    //throw new Exception("应答器编号超出范围"); 
                }
                else
                { baliseNum = value; }
            }
        }
        [XmlElement("DataSize")]
        public byte DataSize
        {
            get
            {
                return dataSize;
            }
            set
            {
                if (value == 0)
                {
                    error += string.Format("DataSize={0}——数据大小超出范围", value) + "\r\n";
                    dataSize = 1;
                    //throw new Exception("数据大小超出范围");
                }
                else
                { dataSize = value; }
            }
        }

        [XmlElement("BaliseProperty")]
        public byte BaliseProperty  //判断属性的范围0-3   64-67      128-131   192-195
        {
            get
            {
                return baliseProperty;
            }
            set
            {
                if (judgeProp(value))
                { baliseProperty = value; }
                else
                {
                    error += string.Format("BaliseProperty={0}——应答器属性参数超出范围", value) + "\r\n";
                    baliseProperty = 0;
                    //throw new Exception("应答器属性参数超出范围");
                }
            }
        }

        [XmlElement("BalisePos")]
        public uint BalisePos { get { return balisePos; } set { balisePos = value; } }

        [XmlElement("BaliseTrackNum")]
        public byte BaliseTrackNum
        {
            get
            { return baliseTrackNum; }
            set
            {
                if (value == 0)
                {
                    error += string.Format("BaliseTrackNum={0}——应答器所在轨道号超出范围", value) + "\r\n";
                    baliseTrackNum = 1;
                    //throw new Exception("应答器所在轨道号超出范围");
                }
                else
                { baliseTrackNum = value; }
            }
        }

        [XmlElement("BaliseLoc")]
        public byte BaliseLoc
        {
            get
            {
                return baliseLoc;
            }
            set
            {
                if (judgeLoc(value))
                {
                    baliseLoc = value;
                }
                else
                {
                    error += string.Format("BaliseLoc={0}——应答器在组中位置参数超出范围", value) + "\r\n";
                    baliseLoc = 0;
                    //throw new Exception("应答器在组中位置参数超出范围");
                }
            }
        }

        [XmlElement("BaliseDataPort")]
        public uint BaliseDataPort { get { return baliseDataPort; } set { baliseDataPort = value; } }

        [XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public string Error { get {return error; } set {error=value; } }
        private bool judgeProp(byte value)//判断属性的范围0-3   64-67      128-131   192-195
        {
            bool flag = false;
            if (0 <= value && value <= 3)
            { flag = true; }
            else if (64 <= value && value <= 67)
            { flag = true; }
            else if (128 <= value && value <= 131)
            { flag = true; }
            else if (192 <= value && value <= 195)
            { flag = true; }

            return flag;
        }

        private bool judgeLoc(byte value)
        {
            bool flag = false;
            if (0 <= value && value <= 7)
            { flag = true; }
            else if (32 <= value && value <= 39)
            { flag = true; }
            else if (64 <= value && value <= 71)
            { flag = true; }
            else if (96 <= value && value <= 103)
            { flag = true; }
            else if (128 <= value && value <= 135)
            { flag = true; }
            else if (160 <= value && value <= 167)
            { flag = true; }
            else if (192 <= value && value <= 199)
            { flag = true; }
            else if (224 <= value && value <= 231)
            { flag = true; }
            return flag;
        }
    }

    #endregion

}
