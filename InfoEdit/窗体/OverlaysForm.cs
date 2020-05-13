using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;

namespace InfoEdit
{
    public partial class OverlaysForm : DevExpress.XtraEditors.XtraForm
    {
        private Form1 parentForm = new Form1();
        public List<GMapOverlay> overlays = new List<GMapOverlay>();//名字，GMapOverlay查找

        public GMapOverlay currentOverlay = null;//= new GMapOverlay();
        public GMapOverlay CurrentOverlay
        {
            get {return currentOverlay; }
            set {currentOverlay=value; }
        }
        public List<GMapOverlay> OverLays
        { 
            get {return overlays; }
            set {overlays=value; }
        }

        public OverlaysForm()
        {
            InitializeComponent();
            //parentForm = (Form1)this.ParentForm;
        }

        private void OverlaysForm_Load(object sender, EventArgs e)
        {
            try
            {
                parentForm = (Form1)this.ParentForm;
                ShowGMapOverlayInfo(currentOverlay);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ShowGMapOverlayInfo(GMapOverlay gMapOverlay)
        {
            if (gMapOverlay != null)
            {
                this.gridControl1.DataSource = OverLayPointDt(gMapOverlay);
                this.gridView1.BestFitColumns();
            }
        }

        

        private DataTable OverLayPointDt(GMapOverlay overlay)
        {
            DataTable dt = new DataTable(overlay.Id);
            dt.Columns.Add("Name");
            dt.Columns.Add("PointId");
            dt.Columns.Add("WGSLat");
            dt.Columns.Add("WGSLng");
            dt.Columns.Add("Type");
            dt.Columns.Add("Tag");
            foreach(GMapMarker marker in overlay.Markers)
            {
                DataRow dr = dt.NewRow();
                dr["Name"]=overlay.Id;
                dr["PointId"]=overlay.Markers.IndexOf(marker)+1;
                dr["WGSLat"] = marker.Position.Lat;
                dr["WGSLng"]= marker.Position.Lng;
                dr["Type"]="轨道信息点";
                dr["Tag"]=null;
                dt.Rows.Add(dr);
            }
            return dt;
        }

        private DataTable OverLayRouteDt(GMapOverlay overlay)
        {
            //var markers = overlay.Markers;
            DataTable dt = new DataTable(overlay.Id);
            dt.Columns.Add("Name");
            dt.Columns.Add("TrackArcId");
            dt.Columns.Add("Length");
            dt.Columns.Add("GoogleLength");
            dt.Columns.Add("Type");
            foreach (GMapRoute route in overlay.Routes)
            {
                DataRow dr = dt.NewRow();
                dr["Name"] = overlay.Id;
                dr["TrackArcId"] = overlay.Routes.IndexOf(route) + 1;
                dr["Length"] = route.Tag;
                dr["GoogleLength"] = GetDistanceGoogle(route.Points[0], route.Points[1]);//谷歌距离
                dr["Type"] = "轨道片";
                dt.Rows.Add(dr);
            }
            return dt;
        }

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

        private void OverlaysForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void gridView1_CustomDrawEmptyForeground(object sender, DevExpress.XtraGrid.Views.Base.CustomDrawEventArgs e)
        {
            if (gridView1.RowCount == 0)
            {
                //文本
                string str = "暂未查找到匹配的数据!";
                //字体
                Font f = new Font("微软雅黑", 16);
                //显示位置
                Rectangle r = new Rectangle(gridControl1.Width / 2 - 100, e.Bounds.Top + 45, e.Bounds.Right - 5, e.Bounds.Height - 5);
                //显示颜色
                e.Graphics.DrawString(str, f, Brushes.Gray, r);
            }
        }

        private void btnShowPoint_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void btnShowTrackPiece_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.gridControl1.MainView = gridView2;
            if (currentOverlay != null)
            {
                this.gridControl1.DataSource = OverLayRouteDt(currentOverlay);
                this.gridView2.BestFitColumns();
            }
        }

        private void barCheckItem1_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barCheckItem1.Checked)
            {
                barCheckItem2.Checked = false;
                this.gridControl1.MainView = gridView1;
                if (currentOverlay != null)
                {
                    this.gridControl1.DataSource = OverLayPointDt(currentOverlay);
                    this.gridView1.BestFitColumns();
                }
            }
        }

        private void barCheckItem2_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (barCheckItem2.Checked)
            {
                this.gridControl1.MainView = gridView2;
                barCheckItem1.Checked = false;
                if (currentOverlay != null)
                {
                    this.gridControl1.DataSource = OverLayRouteDt(currentOverlay);
                    this.gridView2.BestFitColumns();
                }
            }
        }

        private void btnExportExcel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SaveFileDialog sFD = new SaveFileDialog();
            sFD.RestoreDirectory = true;
            sFD.Filter = "Excel表格文件(*.xlsx)|*.xlsx";
            sFD.InitialDirectory = Application.StartupPath;
            if (sFD.ShowDialog() == DialogResult.OK)
            {
                Form1 form = new Form1();
                DataTable dt = (DataTable)this.gridControl1.DataSource;
                form.ExportToXlxs(dt, sFD.FileName);
                form.Dispose();
            }
        }
    }
    
}