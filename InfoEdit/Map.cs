using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars;
using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace InfoEdit
{
    public partial class Map : DevExpress.XtraEditors.XtraForm
    {
        private Form1 parentForm = new Form1();
        private GMapOverlay markersOverlay = new GMapOverlay("markers");
        private GMapOverlay routeOverlay = new GMapOverlay("routes");
        public TopologyElement.Topology topology;//= new TopologyElement.Topology();
        public List<TopologyElement.Topology> Topologies  { get; set; }

        public Map()
        {
            InitializeComponent();
            parentForm = (Form1)this.Parent;
            
        }
        private void Map_Load(object sender, EventArgs e)
        {
            //if(Topologies!=null)
            //{
            //    foreach (TopologyElement.Topology topo in Topologies)
            //    {
            //        Map_OnTopology(topo);
            //    }
            //}
            if (topology != null)
            { Map_OnTopology(topology); }
            //(this.ParentForm as Form1).OnTopology += new Action<TopologyElement.Topology>(Map_OnTopology);
        }
        private void Map_FormClosed(object sender, FormClosedEventArgs e)
        {
            //(this.ParentForm as Form1).OnTopology -= new Action<TopologyElement.Topology>(Map_OnTopology);
        }
        private void Map_OnTopology(TopologyElement.Topology topo)
        {
            if (topo != null)
            {
                List<PointLatLng> gpoints = new List<PointLatLng>();
                foreach (TopologyElement.Point p in topo.Points)
                {
                    PointLatLng pll = PointToMapPoint(p);
                    AddMarkers(PointToMarker(p));//加入的是未纠偏的经纬度坐标(原始信息);
                    gpoints.Add(pll);
                }
                ///
                double minlat = gpoints.Min(a => a.Lat);
                double maxlat = gpoints.Max(a => a.Lat);
                double minlng = gpoints.Min(a => a.Lng);
                double maxlng = gpoints.Max(a => a.Lng);

                PointLatLng lefttop = new PointLatLng(minlat, minlng);

                PointLatLng center = new PointLatLng((minlat + maxlat) / 2.0, (minlng + maxlng) / 2.0);

                var idmin = topo.TracksId.Min();
                var idmax = topo.TracksId.Max();
                for (int i = idmin; i <= idmax; i++)
                {
                    List<TopologyElement.Arc> arcs = topo.Arcs.FindAll(a => a.TrackID == i);
                    List<TopologyElement.Point> points = topo.Points.FindAll(b => b.TrackId == i);
                    for (int j = 0; j < points.Count - 1; j++)
                    {
                        var pieceid = topo.PiecesArcs.Find(c => c.StartId == points[j].ID);
                        AddRoutes(TwoPointsToLine(points[j], points[j + 1], pieceid.Id.ToString()));
                    }
                }
                this.gmap.Overlays.Add(markersOverlay);
                this.gmap.Overlays.Add(routeOverlay);
                lefttop.Lat += maxlat - minlat;

                RectLatLng area = new RectLatLng();
                area.LocationTopLeft = lefttop;
                area.Size = new SizeLatLng(maxlat - minlat, maxlng - minlng);
                this.gmap.SelectedArea = area;
                this.gmap.SetZoomToFitRect(area);
            }

        }

        private GMapOverlay txtPointsOverlay = new GMapOverlay("txtPoints");
        private void TrackGeoInfo_ShowFileLatLng(List<string[]> lnglat)
        {
            foreach (string[] a in lnglat)
            {
                double[] latlng = new double[2];
                latlng[0] = Convert.ToDouble(a[0]);//经度
                latlng[1] = Convert.ToDouble(a[1]);//纬度
                txtPointsOverlay.Markers.Add(TxtPointToMarker(GetTxtPoint(latlng)));
            }
            this.gmap.Overlays.Add(txtPointsOverlay);
        }


        private PointLatLng GetTxtPoint(double[] latlon)
        {
            return new PointLatLng(latlon[1], latlon[0]);
        }
        private GMapMarker TxtPointToMarker(PointLatLng point)
        {
            GMapMarker gMapMarker = new GMarkerGoogle(point, GMarkerGoogleType.lightblue_dot);
            gMapMarker.ToolTipText = string.Format("纬度{0},经度{1}", point.Lat, point.Lng);
            gMapMarker.ToolTip.Foreground = Brushes.Black;
            gMapMarker.ToolTip.TextPadding = new Size(20, 10);
            return gMapMarker;

        }
        #region  地图操作

        private DateTime timemousedown;
        private int timemillSecond = 200;
        private int mousedistance = 30;
        private int mousepress_x = 0, mousepress_y = 0;
        bool isMouseDrag = false;//false:鼠标单击 true:鼠标拖拽

        bool isMouseDown = false;
        bool isMarkerEnter = false;
        bool isMarkerDrag = false;
        private GMapOverlay mouseOverlay = new GMapOverlay("mouseOverlay");
        GMapMarker currentMarker;

        private void gmap_Load(object sender, EventArgs e)
        {
            //加载谷歌中国地图
            GMapProvider.TimeoutMs = 0;
            gmap.MapProvider = GMapProviders.GoogleChinaMap;
            gmap.MapProvider = GMap.NET.MapProviders.GoogleChinaMapProvider.Instance;

            ////加载谷歌卫星地图
            //gmap.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
            //GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

            gmap.DragButton = System.Windows.Forms.MouseButtons.Left;
            gmap.Zoom = 12;
            gmap.ShowCenter = false;
            gmap.MaxZoom = 24;
            gmap.MinZoom = 2;
            this.gmap.Position = new PointLatLng(39.923518, 116.539009);
            this.gmap.IsAccessible = false;
            GMapProvider.TimeoutMs = 1000;
            TopologyElement.Point pt = new TopologyElement.Point();
            pt.Lat = 39.923518;
            pt.Lon = 116.539009;
            pt.ID = 1;
        }

        #region 添加点、图层等操作

        /// <summary>
        /// 获得纠偏后的地理坐标点
        /// </summary>
        /// <param name="pt">拓扑元素点</param>
        /// <returns></returns>
        private PointLatLng PointToMapPoint(TopologyElement.Point point)
        {
            double[] correctLatLng = transform(point.Lat, point.Lon);
            return new PointLatLng(correctLatLng[0], correctLatLng[1]);
        }

        /// <summary>
        /// 拓扑点转GMap标志点
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public GMapMarker PointToMarker(TopologyElement.Point point)
        {
            GMapMarker gMapMarker = new GMarkerGoogle(PointToMapPoint(point), GMarkerGoogleType.green);
            gMapMarker.ToolTipText = string.Format("ID{0}:{1},{2}", point.ID, point.Lat, point.Lon);
            gMapMarker.ToolTip.Foreground = Brushes.Black;
            gMapMarker.ToolTip.TextPadding = new Size(20, 10);
            return gMapMarker;
        }

        //private GMapMarkerImage PointToMarkerImage(TopologyElement.Point point)
        //{
        //    GMarkerGoogle a = new GMarkerGoogle(PointToMapPoint(point), GMarkerGoogleType.blue_pushpin);
        //    GMapMarkerImage gMapMarker = new GMapMarkerImage(PointToMapPoint(point),a.Bitmap);
        //    gMapMarker.ToolTipText = string.Format("ID{0}:{1},{2}", point.ID, point.Lat, point.Lon);
        //    gMapMarker.ToolTip.Foreground = Brushes.Black;
        //    gMapMarker.ToolTip.TextPadding = new Size(20, 10);
        //    return gMapMarker;
        //}

        /// <summary>
        /// 添加到图层
        /// </summary>
        /// <param name="gMapMarker"></param>
        private void AddMarkers(GMapMarker gMapMarker)
        {
            markersOverlay.Markers.Add(gMapMarker);
        }

        /// <summary>
        /// 添加GMapMarkerImage的点
        /// </summary>
        /// <param name="gMapMarker"></param>
        //private void AddMarkers(GMapMarkerImage gMapMarker)
        //{ markersOverlay.Markers.Add(gMapMarker); }


        /// <summary>
        /// 拓扑点转直线
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        private GMapRoute TwoPointsToLine(TopologyElement.Point point1, TopologyElement.Point point2)
        {
            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(PointToMapPoint(point1));
            points.Add(PointToMapPoint(point2));
            GMapRoute route = new GMapRoute(points, "");
            route.Stroke = new Pen(Color.Green, 3);
            return route;
        }

        private GMapRoute TwoPointsToLine(TopologyElement.Point point1, TopologyElement.Point point2, string arcId)
        {
            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(PointToMapPoint(point1));
            points.Add(PointToMapPoint(point2));
            GMapRoute route = new GMapRoute(points, arcId);
            route.Stroke = new Pen(Color.BlueViolet, 3);
            return route;
        }


        /// <summary>
        /// 添加至图层
        /// </summary>
        /// <param name="route"></param>
        private void AddRoutes(GMapRoute route)
        {
            routeOverlay.Routes.Add(route);
        }

        #endregion

        #region 切换地图源操作
        private int maptype = 1;//开始时是谷歌中国地图
        private void btnChangeMap_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ChangeMap(ref maptype);
        }

        /// <summary>
        /// 切换地图源
        /// </summary>
        /// <param name="maptype">MapType的参数</param>
        private void ChangeMap(ref int maptype)
        {
            switch (maptype)
            {
                case 1:
                    SetMap((int)MapType.GoogleChinaSatelliteMap);
                    maptype = 2;
                    break;
                case 2:
                    SetMap((int)MapType.GaoDeMap);
                    maptype = 3;
                    break;
                case 3:
                    SetMap((int)MapType.GoogleChinaMap);
                    maptype = 1;
                    break;
                default:
                    break;
            }
        }
        enum MapType
        {
            GoogleChinaMap = 1,//谷歌中国地图
            GoogleChinaSatelliteMap = 2,//谷歌卫星地图
            GaoDeMap = 3
        }

        /// <summary>
        /// 切换地图
        /// </summary>
        /// <param name="mapType">地图的类型</param>
        /// <returns></returns>
        private void SetMap(int mapType)
        {
            GMapProvider.TimeoutMs = 0;
            switch (mapType)
            {
                //谷歌中国地图
                case 1:
                    gmap.MapProvider = GMapProviders.GoogleChinaMap;
                    gmap.MapProvider = GMap.NET.MapProviders.GoogleChinaMapProvider.Instance;
                    break;
                //谷歌卫星地图
                case 2:
                    gmap.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
                    break;
                //高德地图(暂时为空)
                case 3:
                    gmap.MapProvider = GMapProviders.EmptyProvider;
                    break;
                default:
                    break;
            }
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            gmap.ReloadMap();
            GMapProvider.TimeoutMs = 1000;
        }

        #endregion

        #region 计算距离
        private const double EARTH_RADIUS = 6378137.0;//地球赤道半径(单位：m。6378137m是1980年的标准，比1975年的标准6378140少3m）

        /// <summary>
        /// 角度数转换为弧度公式
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static double radians(double d)
        {
            return d * Math.PI / 180.0;
        }
        /// <summary>
        /// 弧度转换为角度数公式
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static double degrees(double d)
        {
            return d * (180 / Math.PI);
        }
        /// <summary>
        /// 计算两点之间的距离
        /// 单位：米
        /// </summary>
        /// <param name="Degree1"></param>
        /// <param name="Degree2"></param>
        /// <returns></returns>
        public static double GetDistance(PointLatLng PointLatLng1, PointLatLng PointLatLng2)
        {
            double radLat1 = radians(PointLatLng1.Lat);
            double radLat2 = radians(PointLatLng2.Lat);
            double a = radLat1 - radLat2;
            double b = radians(PointLatLng1.Lng) - radians(PointLatLng2.Lng);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }
        /// <summary>
        /// 计算两个经纬度之间的直接距离(google 算法)
        /// </summary>
        public static double GetDistanceGoogle(PointLatLng pointlatlng1, PointLatLng pointlatlng2)
        {
            double radLat1 = radians(pointlatlng1.Lat);
            double radLng1 = radians(pointlatlng1.Lng);
            double radLat2 = radians(pointlatlng2.Lat);
            double radLng2 = radians(pointlatlng2.Lng);
            double s = Math.Acos(Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Cos(radLng1 - radLng2) + Math.Sin(radLat1) * Math.Sin(radLat2));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }
        /// <summary>
        /// 以一个经纬度为中心计算出四个顶点
        /// </summary>
        /// <param name="Degree1">中心点</param>
        /// <param name="distance">半径(米)</param>
        /// <returns></returns>
        public static PointLatLng[] GetDegreeCoordinates(PointLatLng pointlatlng, double distance)
        {
            double dlng = 2 * Math.Asin(Math.Sin(distance / (2 * EARTH_RADIUS)) / Math.Cos(pointlatlng.Lng));
            dlng = degrees(dlng);//一定转换成角度数
            double dlat = distance / EARTH_RADIUS;
            dlat = degrees(dlat);//一定转换成角度数
            return new PointLatLng[] { new PointLatLng( Math.Round(pointlatlng.Lat + dlat,6),Math.Round(pointlatlng.Lng - dlng,6)),//left-top
                                   new PointLatLng(Math.Round(pointlatlng.Lat- dlat,6),Math.Round(pointlatlng.Lng - dlng,6)),//left-bottom
                                   new PointLatLng( Math.Round(pointlatlng.Lat + dlat,6),Math.Round(pointlatlng.Lng + dlng,6)),//right-top
                                   new PointLatLng(Math.Round(pointlatlng.Lat - dlat,6),Math.Round(pointlatlng.Lng + dlng,6)) //right-bottom
            };
        }
        #endregion

        #region GPS纠偏算法
        /**
        * gps纠偏算法，适用于google,高德体系的地图
        */
        public static double pi = 3.1415926535897932384626;
        public static double x_pi = 3.14159265358979324 * 3000.0 / 180.0;
        public static double a = 6378245.0;
        public static double ee = 0.00669342162296594323;

        public static double transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y
                    + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        public static double transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1
                    * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0
                    * pi)) * 2.0 / 3.0;
            return ret;
        }
        public static double[] transform(double lat, double lon)
        {
            if (outOfChina(lat, lon))
            {
                return new double[] { lat, lon };
            }
            double dLat = transformLat(lon - 105.0, lat - 35.0);
            double dLon = transformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;
            return new double[] { mgLat, mgLon };
        }
        public static bool outOfChina(double lat, double lon)
        {
            if (lon < 72.004 || lon > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        /** 
         * 84 to 火星坐标系 (GCJ-02) World Geodetic System ==> Mars Geodetic System 
         * 
         * @param lat 
         * @param lon 
         * @return 
         */
        public static double[] gps84_To_Gcj02(double lat, double lon)
        {
            if (outOfChina(lat, lon))
            {
                return new double[] { lat, lon };
            }
            double dLat = transformLat(lon - 105.0, lat - 35.0);
            double dLon = transformLon(lon - 105.0, lat - 35.0);
            double radLat = lat / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            double mgLat = lat + dLat;
            double mgLon = lon + dLon;
            return new double[] { mgLat, mgLon };
        }

        /** 
         * * 火星坐标系 (GCJ-02) to 84 * * @param lon * @param lat * @return 
         * */
        public static double[] gcj02_To_Gps84(double lat, double lon)
        {
            double[] gps = transform(lat, lon);
            double lontitude = lon * 2 - gps[1];
            double latitude = lat * 2 - gps[0];
            return new double[] { latitude, lontitude };
        }
        /** 
         * 火星坐标系 (GCJ-02) 与百度坐标系 (BD-09) 的转换算法 将 GCJ-02 坐标转换成 BD-09 坐标 
         * 
         * @param lat 
         * @param lon 
         */
        public static double[] gcj02_To_Bd09(double lat, double lon)
        {
            double x = lon, y = lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            double tempLon = z * Math.Cos(theta) + 0.0065;
            double tempLat = z * Math.Sin(theta) + 0.006;
            double[] gps = { tempLat, tempLon };
            return gps;
        }

        /** 
         * * 火星坐标系 (GCJ-02) 与百度坐标系 (BD-09) 的转换算法 * * 将 BD-09 坐标转换成GCJ-02 坐标 * * @param 
         * bd_lat * @param bd_lon * @return 
         */
        public static double[] bd09_To_Gcj02(double lat, double lon)
        {
            double x = lon - 0.0065, y = lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            double tempLon = z * Math.Cos(theta);
            double tempLat = z * Math.Sin(theta);
            double[] gps = { tempLat, tempLon };
            return gps;
        }

        /**将gps84转为bd09 
         * @param lat 
         * @param lon 
         * @return 
         */
        public static double[] gps84_To_bd09(double lat, double lon)
        {
            double[] gcj02 = gps84_To_Gcj02(lat, lon);
            double[] bd09 = gcj02_To_Bd09(gcj02[0], gcj02[1]);
            return bd09;
        }
        public static double[] bd09_To_gps84(double lat, double lon)
        {
            double[] gcj02 = bd09_To_Gcj02(lat, lon);
            double[] gps84 = gcj02_To_Gps84(gcj02[0], gcj02[1]);
            //保留小数点后六位  
            gps84[0] = retain6(gps84[0]);
            gps84[1] = retain6(gps84[1]);
            return gps84;
        }

        /**保留小数点后六位 
         * @param num 
         * @return 
         */
        private static double retain6(double num)
        {
            string result = String.Format("%.6f", num);
            return Convert.ToDouble(result);
        }


        #endregion


        #region 测距模式和地图编辑模式

        private void gmap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //MouseEventArgs me = (MouseEventArgs)e;
            //if(me.Button==MouseButtons.Left)
            //{
            //    currentPoint.Lat = gmap.FromLocalToLatLng(e.X, e.Y).Lat;
            //    currentPoint.Lng = gmap.FromLocalToLatLng(e.X, e.Y).Lng;
            //    if (gmap.Zoom != gmap.MaxZoom)
            //    { this.gmap.Zoom += 4; this.gmap.Position = currentPoint; }

            //}
        }
        private void gmap_OnMapDrag()
        {
        }

        private void gmap_MouseDown(object sender, MouseEventArgs e)
        {
            timemousedown = DateTime.Now;
            mousepress_x = e.X;
            mousepress_y = e.Y;
            isMouseDown = true;

            if (e.Button == MouseButtons.Right)
            {
                popupMenu2.ShowPopup(Control.MousePosition);
            }
        }

        int isSetPoint = 0;//设置起止点，1为起点，2为终点
        PointLatLng startPoint;
        PointLatLng endPoint;


        private void gmap_MouseClick(object sender, MouseEventArgs e)
        {
            if (isMarkerEditable)
            {
                if (timemousedown.AddMilliseconds(timemillSecond) > DateTime.Now)
                {
                    isMouseDrag = false;
                }
                else
                {
                    isMouseDrag = true;
                }
                int x_dis = (mousepress_x > e.X) ? mousepress_x - e.X : e.X - mousepress_x;
                int y_dis = (mousepress_y > e.Y) ? mousepress_y - e.Y : e.Y - mousepress_y;
                if (mousedistance < System.Math.Sqrt(x_dis * x_dis + y_dis * x_dis))
                {
                    isMouseDrag = true;//拖拽
                }
                if (!isMouseDrag)
                {
                    PointLatLng p = gmap.FromLocalToLatLng(e.X, e.Y);
                    GMapMarker marker = new GMarkerGoogle(p, GMarkerGoogleType.blue_dot);
                    marker.ToolTipText = string.Format("纬度:{0},经度:{1}", marker.Position.Lat, marker.Position.Lng);
                    mouseOverlay.Markers.Add(marker);

                }
            }
            if (isCalDistanceMode)
            {
                if (timemousedown.AddMilliseconds(timemillSecond) > DateTime.Now)
                {
                    isMouseDrag = false;
                }
                else
                {
                    isMouseDrag = true;
                }
                int x_dis = (mousepress_x > e.X) ? mousepress_x - e.X : e.X - mousepress_x;
                int y_dis = (mousepress_y > e.Y) ? mousepress_y - e.Y : e.Y - mousepress_y;
                if (mousedistance < System.Math.Sqrt(x_dis * x_dis + y_dis * x_dis))
                {
                    isMouseDrag = true;//拖拽
                }
                if (!isMouseDrag)
                {
                    PointLatLng point = this.gmap.FromLocalToLatLng(e.X, e.Y);
                    if (isSetPoint == 0)
                    {
                        isSetPoint = 1;//设置起始点
                    }
                    else if (isSetPoint == 1)
                    {
                        isSetPoint = 2;
                    }
                    else if (isSetPoint == 2)
                    {
                        isSetPoint = 1;
                    }
                    switch (isSetPoint)
                    {
                        case 1:
                            //起点
                            if (clickMarker != null)
                            { startPoint = clickMarker.Position; }
                            else
                            { startPoint = point; }
                            clickMarker = null;
                            txtStatic.Caption= "当前为起始点";
                            //txt_PointId.Caption = "当前为起始点";
                            //纬度，经度
                            txt_Coordinate.EditValue = string.Format("{0},{1}", startPoint.Lat.ToString(), startPoint.Lng.ToString());
                            break;
                        case 2:
                            //终点
                            if (clickMarker != null)
                            {
                                endPoint = clickMarker.Position;
                            }
                            else
                            {
                                endPoint = point;
                            }
                            clickMarker = null;
                            txtStatic.Caption = "当前为结束点";
                            //txt_PointId.Caption = "当前为结束点";
                            txt_Coordinate.EditValue = string.Format("{0},{1}", endPoint.Lat.ToString(), endPoint.Lng.ToString());
                            break;
                        default:
                            break;
                    }
                    PointLatLng zero = new PointLatLng(0, 0);
                    if (startPoint != zero && endPoint != zero)
                    {
                        txtLength.Caption = string.Format("测距距离:{0}", GetDistance(startPoint, endPoint).ToString());
                    }
                }
            }
        }


        private void gmap_OnMarkerEnter(GMapMarker item)//设置选中的Marker
        {
            isMarkerEnter = true;
            currentMarker = item;
        }

        private void gmap_OnMarkerLeave(GMapMarker item)//取消选中的Marker
        {
            isMarkerEnter = false;
        }

        private void gmap_MouseMove(object sender, MouseEventArgs e)//鼠标移动事件
        {
            PointLatLng p = gmap.FromLocalToLatLng(e.X, e.Y);
            if (isMarkerEditable)//可编辑的情况下可以移动
            {
                if (isMarkerEnter && isMouseDown)//如果按下且是Marker
                {
                    isMarkerDrag = true;
                }
                if (!isMouseDown)//鼠标未按下
                {
                    isMarkerDrag = false;//则图标不可移动
                }
                if (isMarkerDrag)//如果可移动
                {
                    currentMarker.Position = p;//则位置随着鼠标移动
                }
            }
            txt_MousePos.Caption = string.Format("鼠标位置:{0},{1}", p.Lat, p.Lng);
        }

        private void gmap_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;//鼠标未按下
        }

        bool isCalDistanceMode = false;//是否是测距模式
        bool isMarkerEditable = false;//点是否可以编辑

        private void btnCalDis_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            isCalDistanceMode = !isCalDistanceMode;
            if (isMarkerDrag && isCalDistanceMode)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("两个模式不能一起选择");
            }
        }

        private void gmap_OnMapZoomChanged()
        {
        }

        private GMapMarker clickMarker = null;
        private void gmap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            clickMarker = item;
        }

        private void gmap_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnChangeMap_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ChangeMap(ref maptype);
        }
       

        private void btnCalDis_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            isCalDistanceMode = !isCalDistanceMode;
            if (isMarkerDrag && isCalDistanceMode)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("两个模式不能一起选择");
            }
        }

        private void btnMarkerEditMode_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            isMarkerEditable = !isMarkerEditable;
            if (isMarkerDrag && isCalDistanceMode)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("两个模式不能一起选择");
            }
        }
        #endregion

        #endregion
        
    }
}