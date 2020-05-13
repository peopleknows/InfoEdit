using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraBars;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using DevExpress.XtraEditors;

namespace InfoEdit
{
    public partial class MapOperator : DevExpress.XtraEditors.XtraForm
    {
        public GMapOverlay OverLay { get; set; }
        public MapOperator()
        {
            InitializeComponent();
        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {
            //加载谷歌中国地图
            GMapProvider.TimeoutMs = 0;
            gmap.MapProvider = GMapProviders.GoogleChinaMap;
            gmap.MapProvider = GMap.NET.MapProviders.GoogleChinaMapProvider.Instance;
            gmap.DragButton = System.Windows.Forms.MouseButtons.Left;
            gmap.Zoom = 12;
            gmap.ShowCenter = false;
            gmap.MaxZoom = 24;
            gmap.MinZoom = 2;
            this.gmap.Position = new PointLatLng(39.923518, 116.539009);
            this.gmap.IsAccessible = false;
            GMapProvider.TimeoutMs = 1000;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                gmap.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                gmap.ReloadMap();
                GMapProvider.TimeoutMs = 1000;
            }
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

        private void map_Normal_CheckedChanged(object sender, EventArgs e)
        {
            if (map_Normal.Checked)
            {
                gmap.MapProvider = GMapProviders.GoogleChinaMap;
                gmap.MapProvider = GMap.NET.MapProviders.GoogleChinaMapProvider.Instance; GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                gmap.ReloadMap();
                GMapProvider.TimeoutMs = 1000;
            }
        }

        private void gmap_MouseMove(object sender, MouseEventArgs e)
        {
            PointLatLng p = gmap.FromLocalToLatLng(e.X, e.Y);
            txtMousePos.Caption = string.Format("鼠标位置:{0},{1}", p.Lat, p.Lng);
        }
        

        private GMapMarker clickMarker = null;
        private void gmap_OnMarkerClick(GMap.NET.WindowsForms.GMapMarker item, MouseEventArgs e)
        {
            clickMarker = item;
        }

        private PointLatLng startPoint;
        private PointLatLng endPoint;
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if(clickMarker!=null)
            {
                startPoint = clickMarker.Position;
                txtPointLatLng.EditValue = string.Format("{0},{1}", clickMarker.Position.Lng, clickMarker.Position.Lat);
            }
        }

        private void btnEndPoint_Click(object sender, EventArgs e)
        {
            if(clickMarker!=null)
            {
                endPoint = clickMarker.Position;
                txtPointLatLng.EditValue = string.Format("{0}{1}", clickMarker.Position.Lng, clickMarker.Position.Lat);
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            if(startPoint == null)
            {
                XtraMessageBox.Show("缺少起始点");
            }
            if (endPoint == null)
            {
                XtraMessageBox.Show("缺少结束点");
            }
            else
            {
                if(rb_Dis.Checked)
                {
                }
                if(rb_Heading.Checked)
                {

                }
            }
        }
    }
}