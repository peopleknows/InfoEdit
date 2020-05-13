using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars.Helpers;
using System.Xml;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using System.Xml.Serialization;
using System.IO;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList;
using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Data.SqlClient;
using Microsoft.Win32;
using NPOI;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using NPOI.SS.Formula.Eval;
using InfoEdit.KML文件;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Visio;

namespace InfoEdit
{
    public partial class Form1 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public List<GMapOverlay> gMapOverlays = new List<GMapOverlay>();//经纬度文件的GmapOverlay
        //站场拓扑图层和普通文件暂时不分开了;
        //public List<GMapOverlay> topoOverlays = new List<GMapOverlay>();//站场拓扑的图层
        public List<CustomAttribute> customAttributes = new List<CustomAttribute>();//自定义属性
        public CustomAttribute currentAttribute = null;//当前属性

        private OverlaysForm overlays;
        private GMapOverlay currentOverlay = null;//当前图层
        public List<TopologyElement.Topology> topologies = new List<TopologyElement.Topology>();//xml文件的拓扑

        public DataTable chooseFiles = new DataTable();
        public List<string> FilePaths = new List<string>();//所有打开的文件

        public OpenFileDialog oFD = new OpenFileDialog();
        public SaveFileDialog sFD = new SaveFileDialog();
        public Form1()
        {
            InitializeComponent();
            InitFileTree(2);//初始设置文件管理框
            InitialOFD();//初始设置同一个打开文件
            InitialSFD();//初始设置保存文件
            InitialSetPropCtrl();//

            this.gmap.Overlays.Add(mouseOverlay);//暂时出问题
            //初始设置界面皮肤
            if (!mvvmContext1.IsDesignMode)
                InitializeBindings();
            SkinHelper.InitSkinGallery(skinRibbonGalleryBarItem3, true);

            //初始设置地图文件
            InitialBindingDataTable();
             //设置边框颜色
            this.toolTipController1.Appearance.BorderColor = System.Drawing.Color.Blue;
            //设置显示箭头图标
            this.toolTipController1.ShowBeak = true;
            this.toolTipController1.ShowBeak = true;
            this.flowLayoutPanel1.Resize += new System.EventHandler(this.flowLayoutPanel1_Resize);
        }

        #region 初始化设置
        /// <summary>
        /// oFD打开文件的初始化
        /// </summary>
        void InitialOFD()
        {
            oFD.InitialDirectory = System.Windows.Forms.Application.StartupPath;//刚开始的路径是初始路径
            oFD.Title = "打开文件";
            oFD.Multiselect = false;//初始设置不允许多选路径
            oFD.RestoreDirectory = false;//关闭前不还原当前目录
        }
        /// <summary>
        /// sFD文件保存的初始化
        /// </summary>
        void InitialSFD()
        {
            sFD.InitialDirectory = System.Windows.Forms.Application.StartupPath;//刚开始的路径是初始路径
            sFD.Title = "保存文件";
            sFD.RestoreDirectory = false;//关闭前不还原当前目录
        }
        /// <summary>
        /// 界面皮肤控件的初始化
        /// </summary>
        void InitializeBindings()
        {
            var fluent = mvvmContext1.OfType<MainViewModel>();
        }
        /// <summary>
        /// 文件管理框的初始化
        /// </summary>
        void InitialBindingDataTable()
        {
            chooseFiles.Columns.Add("IsChoose", typeof(bool)).SetOrdinal(0);
            chooseFiles.Columns.Add("IsOverlayVisible", typeof(bool)).SetOrdinal(1);
            chooseFiles.Columns.Add("IsLine", typeof(bool)).SetOrdinal(2);
            chooseFiles.Columns.Add("IsLocked", typeof(bool)).SetOrdinal(3);
            chooseFiles.Columns.Add("Name");
            this.gridControl1.DataSource = chooseFiles;
        }
        /// <summary>
        /// 初始设置PropertyGridControl
        /// </summary>
        void InitialSetPropCtrl()
        {
            checkBtnPropertySort.CheckedChanged += new EventHandler(checkBtnSort);
            checkBtnAZSort.CheckedChanged += new EventHandler(checkBtnSort);
            SetBarButtonToolTip(checkBtnPropertySort, "分组排序");
            SetBarButtonToolTip(checkBtnAZSort, "按字母排序");
            checkBtnPropertySort.Checked = true;
            propertyGridControl1.ExpandAllRows();
            propertyGridControl1.OptionsBehavior.PropertySort = DevExpress.XtraVerticalGrid.PropertySort.NoSort;

            CustomAttribute ca1 = new CustomAttribute();//初始的自定义属性

            propertyGridControl1.SelectedObject = ca1;
            //
            DevExpress.XtraVerticalGrid.Rows.BaseRow br = propertyGridControl1.GetRowByCaption("线路");   
            //通过循环遍历设置属性的中文名称       
            //foreach (DevExpress.XtraVerticalGrid.Rows.PGridEditorRow per in br.ChildRows)
                foreach (DevExpress.XtraVerticalGrid.Rows.EditorRow per in br.ChildRows)
                {
                if (per.ChildRows.Count > 0)
                {                    //利用递归解决多层可扩展属性的caption的赋值  
                    SetCustomAttributeCaption(per);
                }
                string dicKey = per.Properties.FieldName;
                if (CustomAttribute.dic.ContainsKey(dicKey))
                    per.Properties.Caption = CustomAttribute.dic[dicKey];
                per.Height = 23;//设置属性行高度                          
            }
        }
        #endregion
        
        //编号错误
        public string IdError { get; set; }
        //地理信息错误
        public string GeoInfoError { get; set; }
        //拓扑关系错误记录
        public string TopoError { get; set; }
       
        #region 通用方法

        #region 打开、保存文件、判断文件
        /// <summary>
        /// s设置打开文件框是否多选和文件类型
        /// </summary>
        /// <param name="isMultiSelect">是否可以多项选择文件</param>
        /// <param name="fileType">同一文件类型"Xml文件(*.xml)|*.xml";</param>
        private void SetOFD(bool isMultiSelect, string fileType)
        {
            oFD.Multiselect = isMultiSelect;
            oFD.Filter = fileType;//"Xml文件(*.xml)|*.xml";
        }
        private void SetSFD(string initialDirectory,string fileType)
        {
            sFD.InitialDirectory = initialDirectory;
            sFD.Filter = fileType;
        }
        /// <summary>
        /// 根据打开的文件重新设置打开目录
        /// </summary>
        /// <param name="filepath"></param>
        void OFDSaveLastFilePath(string filepath)
        {
            oFD.InitialDirectory = filepath.Substring(0, filepath.LastIndexOf("\\") + 1);
        }
        /// <summary>
        /// 返回某级父级目录
        /// </summary>
        /// <param name="index">比如上上级index为2</param>
        /// <returns></returns>
        public static DirectoryInfo GetDirectoryInfo(int index)
        {
            string directoryindex = "";
            for(int i=0;i<index;i++)
            {
                directoryindex+=@"..\";
            }
            DirectoryInfo di = new DirectoryInfo(string.Format("{0}{1}", System.Windows.Forms.Application.StartupPath, directoryindex));
            return di;
        }
        /// <summary>
        /// 保存文件，注意文件Filter的格式书写
        /// </summary>
        /// <param name="fileType">"Xml文件(*.xml)|*.xml";</param>
        /// <returns></returns>
        private SaveFileDialog SaveFile(string fileType)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = fileType;//设置默认文件类型显示顺序
            s.FilterIndex = 1;//保存对话框是否记忆上次打开的目录
            s.RestoreDirectory = true;//点了保存按钮进入
            //获取文件名，不带路径
            //获取文件路径，不带文件名
            //FilePath = localFilePath.Substring(0, localFilePath.LastIndexOf("\\"));
            //给文件名前加上时间
            //newFileName = DateTime.Now.ToString("yyyyMMdd") + fileNameExt;
            //在文件名里加字符
            //saveFileDialog1.FileName.Insert(1,"dameng");
            //System.IO.FileStream fs = (System.IO.FileStream)sfd.OpenFile();//输出文件
            ////fs输出带文字或图片的文件，就看需求了 
            return s;
        }
        /// <summary>
        /// 判断是否是固定格式的文件(轨道地理信息文件/车站固定数据文件)
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="keyname"></param>
        /// <returns></returns>
        public bool IsCertainFile(string filename, string keyname)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.Name == keyname)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 读格式文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="list"></param>
        private void ReadFormatTxt(string filePath, ref List<string[]> list)
        {
            try
            {
                list.Clear();//首先清空list
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath, Encoding.Default);
                    foreach (var line in lines)
                    {
                        string temp = line.Trim();
                        if (temp != "")
                        {
                            string[] arr = temp.Split(new char[] { '\t', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (arr.Length > 0)
                            {
                                list.Add(arr);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region (未用)判断窗口打开或者是否已存在
        ////查看窗口是否已经被打开或存在
        //private bool HaveOpened(XtraForm MdiParentForm, string MdiChildFormType)
        //{
        //    bool bReturn = false;
        //    for (int i = 0; i < MdiParentForm.MdiChildren.Length; i++)
        //    {
        //        if (MdiParentForm.MdiChildren[i].GetType().Name == MdiChildFormType)
        //        {
        //            //MdiParentForm.MdiChildren[i].BringToFront();
        //            MdiParentForm.MdiChildren[i].Activate();
        //            bReturn = true;
        //            break;
        //        }
        //    }
        //    return bReturn;
        //}
        ////判断有这个窗体且只有一个
        //private bool HaveOpenedOneChildForm(XtraForm MdiParentForm, string MdiChildFormType)
        //{
        //    bool bReturn = false;
        //    int MdiChildFormCount = 0;
        //    XtraForm childform = null;
        //    for (int i = 0; i < MdiParentForm.MdiChildren.Length; i++)
        //    {
        //        if (MdiParentForm.MdiChildren[i].GetType().Name == MdiChildFormType)
        //        {
        //            MdiChildFormCount++;
        //            //MdiParentForm.MdiChildren[i].BringToFront();
        //            //MdiParentForm.MdiChildren[i].Activate();
        //            //bReturn = true;
        //            //break;
        //            childform = (XtraForm)MdiParentForm.MdiChildren[i];
        //        }
        //    }
        //    if (MdiChildFormCount == 1)
        //    {
        //        childform.Activate();
        //        bReturn = true;
        //    }
        //    return bReturn;
        //}
        ////MDI窗体不重复打开同一类型子窗体
        //public void OpenChildForm(XtraForm formChild)
        //{
        //    formChild.Name = formChild.GetType().FullName;
        //    bool isOpened = false;
        //    foreach (XtraForm form in this.MdiChildren)
        //    {
        //        //如果要显示的子窗体已经在子窗体的子窗体数组中，就把新建的多余的销毁
        //        if (formChild.GetType().ToString() == form.GetType().ToString())
        //        {
        //            form.Activate();
        //            formChild.Dispose();
        //            isOpened = true;
        //            break;
        //        }
        //    }
        //    if (!isOpened)
        //    {
        //        formChild.MdiParent = this;
        //        formChild.Show();
        //    }
        //}
        #endregion

        #region 捕获异常信息
        /// <summary>
        /// 固定的字符串信息写入异常日志
        /// </summary>
        /// <param name="//errorMSG"></param>
        public static void WriteLog(string errorMSG)
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "\\Log";
            DateTime dt = DateTime.Now;
            string strFileName = strPath + "\\" + dt.ToString("yyyyMMdd") + "Error.txt";
            if (!Directory.Exists(strPath))
            {
                Directory.CreateDirectory(strPath);
            }
            FileStream fs;
            StreamWriter sw;
            if (File.Exists(strFileName))
            {
                fs = new FileStream(strFileName, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(strFileName, FileMode.Create, FileAccess.ReadWrite);
            }
            sw = new StreamWriter(fs);
            sw.WriteLine(dt.ToString() + errorMSG);
            sw.Close();
            fs.Close();
        }
        /// <summary>
        /// 写入登录历史文件信息
        /// </summary>
        /// <param name="errorMSG"></param>
        public static void WriteHistoryFile(string filename)
        {
            DirectoryInfo d = GetDirectoryInfo(3);
            string strPath = d.FullName + "Log\\history.txt";
            FileStream fs;
            StreamWriter sw;
            if (File.Exists(strPath))
            {
                StreamReader sr = new StreamReader(strPath);
                string[] lines = File.ReadAllLines(strPath);
                if (!lines.Contains(filename))//历史文件中有该路径
                {
                    fs = new FileStream(strPath, FileMode.Append, FileAccess.Write); sw = new StreamWriter(fs);
                    sw.WriteLine(filename);
                    sw.Close();
                    fs.Close();
                }
            }
            else
            {
                fs = new FileStream(strPath, FileMode.Create, FileAccess.ReadWrite); sw = new StreamWriter(fs);
                sw.WriteLine(filename);
                sw.Close();
                fs.Close();
            }
        }
        /// <summary>
        /// 捕捉异常的特定信息
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static string ErrorMsg(Exception ex)
        {
            return ex.InnerException.Message + "  " + ex.Message;
        }
        /// <summary>
        /// 把读出文档的问题写入日志
        /// </summary>
        /// <param name="exception"></param>
        public void WriteError(Exception exception)
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "\\Log";
            DateTime dt = DateTime.Now;
            string strFileName = "\\Error.txt";
            if (!Directory.Exists(strPath))
            {
                Directory.CreateDirectory(strPath);
            }
            FileStream fs;
            StreamWriter sw;
            if (File.Exists(strFileName))
            {
                fs = new FileStream(strFileName, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(strFileName, FileMode.Create, FileAccess.ReadWrite);
            }
            sw = new StreamWriter(fs);
            sw.WriteLine(ErrorMsg(exception));
            sw.Close();
            fs.Close();
        }
        public static void WriteLog(Exception exception)
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + "\\Log";
            DateTime dt = DateTime.Now;
            string strFileName = strPath + "\\" + dt.ToString("yyyyMMdd") + ".txt";
            if (!Directory.Exists(strPath))
            {
                Directory.CreateDirectory(strPath);
            }
            FileStream fs;
            StreamWriter sw;
            if (File.Exists(strFileName))
            {
                fs = new FileStream(strFileName, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(strFileName, FileMode.Create, FileAccess.ReadWrite);
            }
            sw = new StreamWriter(fs);
            sw.WriteLine(dt.ToString("HH:mm:ss") + exception.Message);
            sw.Close();
            fs.Close();
        }
        /// <summary>
        /// 返回错误窗口
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private XtraForm ShowErrorForm(string error)
        {
            XtraForm form = new XtraForm();
            form.Text = "错误信息";
            form.Size = new Size(400, 200);
            form.WindowState = FormWindowState.Normal;
            form.StartPosition = FormStartPosition.CenterScreen;
            MemoEdit txt = new MemoEdit();
            txt.Dock = DockStyle.Fill;
            form.Controls.Add(txt);
            txt.Text = error;
            txt.Select(0, 0);
            return form;
        }
        #endregion
        
        #region 枚举值
        /// <summary>
        /// 获得类型的枚举名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetEnumNameByKey<T>(int key)
        {
            return Enum.GetName(typeof(T), key);
        }
        #region 一些枚举值
        enum Tracktype : int
        {
            单线正线 = 1,
            上行正线 = 2,
            下行正线 = 3,
            侧线 = 4,
            其他 = 5
        }
        enum AdjacentTrackType
        {
            是 = 1,
            否 = 0
        }
        enum StartEdgeType
        {
            上行线的起始管辖边界 = 0,
            下行线的起始管辖边界 = 1
        }
        enum EndEdgeType
        {
            上行线的结束管辖边界 = 255,
            下行线的结束管辖边界 = 254
        }
        enum AdjacentEdgeType
        {
            上行线的开始管辖边界 = 0,
            下行线的开始管辖边界 = 1,
            上行线的结束管辖边界 = 255,
            下行线的结束管辖边界 = 254,
            未知 = 85
        }
        enum Direction
        {
            上行方向 = 0,
            下行方向 = 1
        }
        #endregion
        #endregion

        #region 自定义类方法 建立拓扑点和线表
        /// <summary>
        /// 根据文件中的车站属性建立拓扑关系        
        /// </summary>
        /// <param name="station"></param>
        private void CreateTopo(Station station, ref TopologyElement.Topology topo)
        {
            ///首先根据轨道轨道片建立一些点(没有类型的差别，只有根据索引判断类型)
            ///还有点的集合
            ///弧段主要建立轨道、道岔、轨道片
            if (string.IsNullOrEmpty(IdError))
            {
                SetPointsArcs(station, topo);//建立topo的所有属性
                SetAllXY(station, topo);//设置所有轨道片的x坐标和y坐标，此时编号未设置根据latlonToUTM计算的;
                SetAllLatLon(station, topo);//设置所有轨道片的经纬度
                SetAllArcId(topo);//设置所有轨道片的编号(此时未设置道岔起始点终点)
                SetAllArcLength(station, topo);//设置所有轨道片的长度和航向角
            }
            else
            {
                XtraMessageBox.Show("编号出错无法建立拓扑！" + "\r\n" + "【提示：点击编号检查以查询】");///暂时属于为解决的事情
                topo = null;
            }
        }
        private void SetTrackMenu(TrackInfo trackInfo,ref TopologyElement.Topology topo)
        {
            foreach (Track t in trackInfo.Tracks)
            {
                if (t.TrackType == 5)//如果是其他
                {
                    topo.TrackMenu.Add(t.TrackID, "CrossLine");
                }
                else //1，2，3，4
                {
                    topo.TrackMenu.Add(t.TrackID, "TrackArc");
                }
            }
        }
        /// <summary>
        /// 得到轨道片、轨道、渡线、点的拓扑关系
        /// </summary>
        /// <param name="index">索引位置开始增加id</param>
        /// <param name="station"></param>
        /// <returns></returns>
        private void SetPointsArcs(Station station, TopologyElement.Topology topo)
        {
            TrackGIS trackGIS = station.TrackInfo.TrackGIS;
            //Track类型的弧的id，TrackPiece型的，CrossLine型-渡线。
            List<int> crosslineid = FindTrackID(station.TrackInfo, 5);//
            List<int> sidingid = FindTrackID(station.TrackInfo, 4);//侧线
            List<int> trackid = FindTrackID(station.TrackInfo);//正线
            foreach (TrackPieceGIS tpg in trackGIS.TrackPieceGIS)//
            {
                if (trackid.Contains(tpg.TrackID))//如果是正线
                {
                    SetTrackPieces(topo, ReturnIndex(topo.PointsId), ReturnIndex(topo.ArcsId), tpg);
                }
                if (sidingid.Contains(tpg.TrackID))//如果是侧线
                {
                    SetTrackPieces(topo, ReturnIndex(topo.PointsId), ReturnIndex(topo.ArcsId), tpg);
                }
                if (crosslineid.Contains(tpg.TrackID))//如果是渡线
                {
                    SetCrosslinePieces(topo, ReturnIndex(topo.PointsId), ReturnIndex(topo.ArcsId), tpg);
                }
            }
        }
        /// <summary>
        /// 添加一个轨道的 轨道类型的弧、轨道片编号集，轨道点编号集
        /// </summary>
        /// <param name="pointindex">点编号的索引</param>
        /// <param name="arcindex">弧编号的索引值</param>
        /// <param name="gis">提供一个轨道的所有轨道片</param>
        private void SetTrackPieces(TopologyElement.Topology topo, int pointindex, int arcindex, TrackPieceGIS gis)
        {
            int trackid = gis.TrackID;
            int piececount = gis.TrackPieces.Count;
            int pointcount = piececount + 1;//轨道点比轨道片个数多一个
            List<int> pointid = new List<int>();
            List<int> piecesid = new List<int>();
            //单独添加弧编号表
            for (int i = 0; i < piececount; i++)
            {
                piecesid.Add(ReturnIndex(topo.ArcsId) + 1);//局部的轨道片id
                topo.ArcsId.Add(ReturnIndex(topo.ArcsId) + 1);//全局的弧id
            }
            //单独添加点编号表
            for (int j = 0; j < pointcount; j++)//单独添加point的id
            {
                pointid.Add(ReturnIndex(topo.PointsId) + 1);//局部的点id
                topo.PointsId.Add(ReturnIndex(topo.PointsId) + 1);//全局的点id
            }
            //添加轨道片类型的弧表，包含了轨道id，起始点、结束点id，点编号集合、片编号集合
            TopologyElement.TrackArc trackArc = new TopologyElement.TrackArc()
            { TrackID = trackid, Id = trackid, StartId = pointindex + 1, EndId = pointindex + pointcount };
            trackArc.PointsId = pointid;
            trackArc.PiecesId = piecesid;
            topo.TrackArcs.Add(trackArc);//添加轨道类型的弧
            topo.TrackArcID.Add(trackid);//添加轨道类型的轨道编号
            topo.TracksId.Add(trackid);//添加所有类型的轨道编号
            //
            topo.Arcs.Add(trackArc);//添加轨道片弧
            //
            int temp = 1;
            foreach (TrackPiece tp in gis.TrackPieces)
            {
                TopologyElement.TrackPieceArc trackPieceArc = new TopologyElement.TrackPieceArc()
                { TrackID = trackid, Id = arcindex + temp, TrackPieceIndex = gis.TrackPieces.IndexOf(tp) + 1, StartId = pointindex + temp, EndId = pointindex + temp + 1 };
                temp++;
                topo.PiecesArcs.Add(trackPieceArc);
                //piecesarcs.Add(trackPieceArc);
                //
                topo.Arcs.Add(trackPieceArc);//添加弧
                                             //arcs.Add(trackPieceArc);
                                             //实例化一个新的点
                TopologyElement.Point point = new TopologyElement.Point()
                { ID = pointindex + temp - 1, TrackId = trackid };
                topo.Points.Add(point);
                //points.Add(point);
            }
            //多增加一个点至全局点集 结束纬度和经度
            TopologyElement.Point lastpoint = new TopologyElement.Point()
            { TrackId = trackid, ID = pointindex + pointcount };
            //设置点的前向弧编号、后向弧编号
            topo.Points.Add(lastpoint);
            //points.Add(lastpoint);
        }
        /// <summary>
        /// 添加一个轨道的 渡线型的弧、轨道片编号集，轨道点编号集
        /// </summary>
        /// <param name="pointindex"></param>
        /// <param name="arcindex"></param>
        /// <param name="gis"></param>
        private void SetCrosslinePieces(TopologyElement.Topology topo, int pointindex, int arcindex, TrackPieceGIS gis)
        {
            int trackid = gis.TrackID;
            int piececount = gis.TrackPieces.Count;
            int pointcount = piececount + 1;//轨道点比轨道片个数多一个
            List<int> pointid = new List<int>();
            List<int> piecesid = new List<int>();
            //单独添加弧编号表
            for (int i = 0; i < piececount; i++)
            {
                piecesid.Add(ReturnIndex(topo.ArcsId) + 1);//局部的轨道片id
                topo.ArcsId.Add(ReturnIndex(topo.ArcsId) + 1);//全局的弧id
            }
            //单独添加点编号表
            for (int j = 0; j < pointcount; j++)//单独添加point的id
            {
                pointid.Add(ReturnIndex(topo.PointsId) + 1);//局部的点id
                topo.PointsId.Add(ReturnIndex(topo.PointsId) + 1);//全局的点id
            }
            //添加轨道片类型的弧表，包含了轨道id，起始点、结束点id，点编号集合、片编号集合
            TopologyElement.Crossline crosslineArc = new TopologyElement.Crossline()
            { TrackID = trackid, Id = trackid, StartId = pointindex + 1, EndId = pointindex + pointcount };
            crosslineArc.PointsId = pointid;
            crosslineArc.PiecesId = piecesid;
            topo.CrossLines.Add(crosslineArc);
            //
            topo.CrossLineID.Add(trackid);
            topo.TracksId.Add(trackid);
            //
            topo.Arcs.Add(crosslineArc);
            //
            int temp = 1;
            foreach (TrackPiece tp in gis.TrackPieces)
            {
                TopologyElement.TrackPieceArc trackPieceArc = new TopologyElement.TrackPieceArc()
                { TrackID = trackid, Id = arcindex + temp, TrackPieceIndex = gis.TrackPieces.IndexOf(tp) + 1, StartId = pointindex + temp, EndId = pointindex + temp + 1 };
                temp++;
                topo.PiecesArcs.Add(trackPieceArc);
                //
                topo.Arcs.Add(trackPieceArc);
                //实例化一个新的点
                TopologyElement.Point point = new TopologyElement.Point()
                { ID = pointindex + temp - 1, TrackId = trackid };
                topo.Points.Add(point);
            }
            //多增加一个点至全局点集
            TopologyElement.Point lastpoint = new TopologyElement.Point()
            { TrackId = trackid, ID = pointindex + pointcount };
            topo.Points.Add(lastpoint);
        }
        /// <summary>
        /// 删除重复记录的id(注意在检查id和数目不符合的情况下使用)
        /// </summary>
        /// <param name="list"></param>
        /// <returns>去重重复id后的list</returns>
        private List<int> RemoveSameId(List<int> list)
        {
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = list.Count - 1; j > 1; j--)
                    {
                        if (list[i] == list[j])
                        {
                            list.Remove(j);
                        }
                    }
                }
            }
            return list;
        }
        #region 坐标设置及经纬度单位转换
        #region 设置所有的坐标、经纬度、编号
        /// <summary>
        /// 设置所有点的XY坐标
        /// </summary>
        /// <param name="station">Xml序列化下的车站类</param>
        /// <param name="points">点集</param>
        private void SetAllXY(Station station, TopologyElement.Topology topo)
        {
            List<TopologyElement.Point> points = topo.Points;
            int TrackArcMin = points.Min(p => p.TrackId);//根据轨道编号
            int TrackArcMax = points.Max(p => p.TrackId);
            for (int i = TrackArcMin; i <= TrackArcMax; i++)
            {
                SetXY(station, points, i);
            }
        }
        /// <summary>
        /// 无论轨道是什么类型首先设置所有轨道的前后向编号
        /// </summary>
        /// <param name="points">某一车站拓扑结构下全局的点集合</param>
        /// <param name="trackArcs">全局的轨道集合</param>
        /// <param name="crosslines">全局的渡线集合</param>
        /// <param name="tracksId">全局的轨道类型弧编号集合</param>
        /// <param name="crosslineid">全局的渡线类型弧编号集合</param>
        private void SetAllArcId(TopologyElement.Topology topo)
        {
            List<TopologyElement.Point> points = topo.Points;
            List<TopologyElement.TrackArc> trackArcs = topo.TrackArcs;
            List<TopologyElement.Crossline> crosslines = topo.CrossLines;
            List<int> tracksId = topo.TrackArcID;
            List<int> crosslineid = topo.CrossLineID;
            List<int> arcsid = points.Select(p => p.TrackId).Distinct().ToList();//
            List<int> tracksid = trackArcs.Select(t => t.TrackID).Distinct().ToList();
            List<int> crossid = crosslines.Select(c => c.TrackID).Distinct().ToList();
            //
            if (tracksid.All(a => arcsid.Any(b => b == a)))//判断轨道编号集合中是否包含轨道类型弧的编号集合
            {
                int min = tracksid.Min();
                int max = tracksid.Max();
                for (int i = min; i <= max; i++)
                {
                    List<TopologyElement.Point> list = points.FindAll(p => p.TrackId == i);//找到点集中同属于一个轨道的集合
                    List<TopologyElement.TrackArc> trackArc = trackArcs.FindAll(ta => ta.Id == i);
                    SetArcId(list, trackArc, i);
                }
            }
            if (crossid.All(a => arcsid.Any(b => b == a)))
            {
                int min = crossid.Min();
                int max = crossid.Max();
                for (int i = min; i <= max; i++)
                {
                    List<TopologyElement.Point> list = points.FindAll(p => p.TrackId == i);//找到点集中同属于一个轨道的集合
                    List<TopologyElement.Crossline> crossline = crosslines.FindAll(ta => ta.Id == i);
                    SetArcId(list, crossline, i);
                }
            }
            ///测试
            //foreach (TopologyElement.TrackArc trackArc in trackArcs)
            //{
            //    Console.WriteLine("轨道{0}下的点-前向后向弧集合", trackArc.Id);
            //    List<TopologyElement.Point> list = points.FindAll(p => p.TrackId == trackArc.Id);//找到点集中同属于一个轨道的集合
            //    foreach (TopologyElement.Point p in list)
            //    {
            //        Console.WriteLine("{0}点的前一个弧编号为{1}，后一个弧编号为{2},点的X坐标:{3},点的Y坐标:{4}", p.ID, p.Arc1Id, p.Arc2Id, p.X, p.Y);
            //    }
            //}
            //foreach (TopologyElement.Crossline crossline in crosslines)
            //{
            //    Console.WriteLine("渡线{0}下的点-前向后向弧集合", crossline.Id);
            //    List<TopologyElement.Point> list = points.FindAll(p => p.TrackId == crossline.Id);//找到点集中同属于一个轨道的集合
            //    foreach (TopologyElement.Point p in list)
            //    {
            //        Console.WriteLine("{0}点的前一个弧编号为{1}，后一个弧编号为{2},点的X坐标:{3},点的Y坐标:{4}", p.ID, p.Arc1Id, p.Arc2Id, p.X, p.Y);
            //    }
            //}
        }
        /// <summary>
        /// 设置所有点的经纬度
        /// </summary>
        /// <param name="station"></param>
        /// <param name="topo"></param>
        private void SetAllLatLon(Station station, TopologyElement.Topology topo)
        {
            List<TopologyElement.Point> points = topo.Points;
            int TrackArcMin = points.Min(p => p.TrackId);//根据轨道编号
            int TrackArcMax = points.Max(p => p.TrackId);
            for (int i = TrackArcMin; i <= TrackArcMax; i++)
            {
                SetLatLon(station, points, i);
            }
        }
        /// <summary>
        /// 设置所有弧的长度(未纠偏)
        /// </summary>
        /// <param name="station"></param>
        /// <param name="topo"></param>
        private void SetAllArcLength(Station station,TopologyElement.Topology topo)
        {
            int TrackArcMin = topo.TracksId.Min();//根据轨道编号
            int TrackArcMax = topo.TracksId.Max();
            for (int i = TrackArcMin; i <= TrackArcMax; i++)
            {
                var points = topo.Points.FindAll(a => a.TrackId == i);//找到所有点集合
                var arcs = topo.Arcs.FindAll(b => b.TrackID == i);
                foreach(TopologyElement.Arc arc in arcs)
                {
                    TopologyElement.Point A = points.Find(c => c.ID == arc.StartId);
                    TopologyElement.Point B = points.Find(c => c.ID == arc.EndId);
                    arc.Length = GetLength(A, B);
                    arc.Angle = GetHeading(A, B);
                }
            }
        }
        #endregion
        /// <summary>
        /// 给全局的某一个轨道下的点设置坐标
        /// </summary>
        /// <param name="station">Xml序列化的车站类对象</param>
        /// <param name="points"></param>
        /// <param name="TrackArcid"></param>
        private void SetXY(Station station, List<TopologyElement.Point> points, int TrackArcid)
        {
            List<TopologyElement.Point> temp = new List<TopologyElement.Point>();
            if (TrackArcid != 0)
            {
                var list = points.Where(p => p.TrackId == TrackArcid).ToList();
                if (list.Count == GetArcXY(station, TrackArcid).Count)
                {
                    var xys = GetArcXY(station, TrackArcid);
                    for (int i = 0; i < xys.Count; i++)
                    {
                        list[i].X = xys[i][0];
                        list[i].Y = xys[i][1];
                    }
                }
            }
        }
        /// <summary>
        /// 给全局的某一个轨道下的点设置坐标
        /// </summary>
        /// <param name="station"></param>
        /// <param name="points"></param>
        /// <param name="TrackArcid"></param>
        private void SetLatLon(Station station, List<TopologyElement.Point> points, int TrackArcid)
        {
            if (TrackArcid != 0)
            {
                var list = points.Where(p => p.TrackId == TrackArcid).ToList();
                if (list.Count == GetArcLatLon(station, TrackArcid).Count)
                {
                    var xys = GetArcLatLon(station, TrackArcid);
                    for (int i = 0; i < xys.Count; i++)
                    {
                        list[i].Lat = xys[i][0];
                        list[i].Lon = xys[i][1];
                    }
                }
            }
        }
        /// <summary>
        /// 给全局的某一个轨道下的点设置坐标
        /// </summary>
        /// <param name="station"></param>
        /// <param name="points"></param>
        /// <param name="TrackArcid"></param>
        private void SetArcLength(Station station, List<TopologyElement.Point> points, int TrackArcid)
        {
            if (TrackArcid != 0)
            {
                var list = points.Where(p => p.TrackId == TrackArcid).ToList();//返回同一个轨道下的点集合
            }
        }
        #endregion
        #region 初始设置弧的编号,一整个轨道的起始点的前向弧编号为0,结束点的后向弧编号为999,道岔连接的Arc3id未设置
        /// <summary>
        /// 给某一个轨道下点和轨道片集赋前后向弧的编号
        /// </summary>
        /// <param name="points">包含索引轨道编号的点</param>
        /// <param name="trackArcs">同一车站下的包含点的点集</param>
        /// <param name="TrackArcid">轨道编号</param>
        private void SetArcId(List<TopologyElement.Point> points, List<TopologyElement.TrackArc> trackArcs, int TrackArcid)
        {
            TopologyElement.TrackArc trackArc = new TopologyElement.TrackArc();
            if (TrackArcid != 0)
            {
                trackArc=trackArcs.Find(a => a.TrackID == TrackArcid);//所有添加好的TrackArc集合
                List<int> piecesid = trackArc.PiecesId;
                if (piecesid.Count == (points.Count() - 1))//如果轨道片和点个数差一，说明没有不符
                { SetArcId(piecesid, points); }//设置每个点的前向和后向弧编号Arc1Id,Arc2Id
            }
            else
            {
                Console.WriteLine("轨道的编号不正确");
            }
        }
        private void SetArcId(List<TopologyElement.Point> points, List<TopologyElement.Crossline> crosslines, int TrackArcid)
        {
            TopologyElement.Crossline cl = new TopologyElement.Crossline();
            if (TrackArcid != 0)
            {
                cl = crosslines.Find(c => c.TrackID == TrackArcid);
                List<int> piecesid = cl.PiecesId;
                if (piecesid.Count == (points.Count() - 1))//如果轨道片和点个数差一，说明没有不符
                { SetArcId(piecesid, points); }//设置每个点的前向和后向弧编号Arc1Id,Arc2Id
                //foreach (TopologyElement.Point p in points)
                //{
                //    Console.WriteLine("{0}点的前一个弧编号为{1}，后一个弧编号为{2}", p.ID, p.Arc1Id, p.Arc2Id);
                //}
            }
            else
            {
                Console.WriteLine("轨道的编号不正确");
            }
        }
        /// <summary>
        /// 设置同一轨道下的所有轨道片点的前向弧编号Arc1Id，和后向弧编号Arc2Id
        /// </summary>
        /// <param name="arcids">轨道的轨道片编号</param>
        /// <param name="points">轨道上的点集合</param>
        private void SetArcId(List<int> arcids, List<TopologyElement.Point> points)
        {
            int min = arcids.Min();
            int max = arcids.Max();
            for (int i = 0; i < points.Count(); i++)
            {
                if (i == 0)
                {
                    points[i].Arc1Id = 0;//正线起始点的上一个弧段设置为0；
                    points[i].Arc2Id = min;
                }
                else if (i == (points.Count - 1))
                {
                    points[i].Arc1Id = max;
                    points[i].Arc2Id = 999;//最后一个点的下一个弧段设置为999
                }
                else
                {
                    points[i].Arc1Id = min + i - 1;
                    points[i].Arc2Id = min + i;
                }
            }
        }
        /// <summary>
        /// 返回当前点列表的最后一位编号
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private int ReturnIndex(List<int> list)
        {
            if (list.Count == 0)
            {
                return 0;
            }
            else
            {
                return list[list.Count - 1];
            }
        }
        /// <summary>
        /// 根据类型找到某一种类型的轨道编号
        /// </summary>
        /// <param name="trackinfo"></param>
        /// <param name="type">类型1-单线正线，2-上行正线，3-下行正线，4-侧线，5-其他</param>
        /// <returns></returns>
        private List<int> FindTrackID(TrackInfo trackinfo, int type)
        {
            List<int> trackid = new List<int>();
            foreach (Track t in trackinfo.Tracks)
            {
                if (t.TrackType == type)
                {
                    trackid.Add(t.TrackID);
                }
            }
            return trackid;
        }
        /// <summary>
        /// 找到轨道类型的编号
        /// </summary>
        /// <param name="trackinfo"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<int> FindTrackID(TrackInfo trackinfo)
        {
            int[] index = new int[3] { 1, 2, 3 };
            List<int> trackid = new List<int>();
            foreach (Track t in trackinfo.Tracks)
            {
                if (index.Contains(t.TrackType))
                {
                    trackid.Add(t.TrackID);
                }
            }
            return trackid;
        }
        #endregion
        #region 设置坐标X，Y，及经纬度下的距离，方位角
        /// <summary>
        /// 返回一个轨道所有的轨道片点的经纬度坐标
        /// </summary>
        /// <param name="track">Xml序列化TrackInfo类:轨道信息</param>
        /// <param name="trackgis">Xml序列化TrackGIS类:轨道地理信息</param>
        /// <param name="index">轨道的编号:不能赋值为0</param>
        /// <returns></returns>
        private List<double[]> GetXY(TrackInfo track, TrackGIS trackgis, int index)
        {
            List<double[]> xy = new List<double[]>();
            double[] startxy = new double[2];
            double[] startlatlon = new double[2];
            double[] endlatlon = new double[2];
            Track tra = null;
            foreach (Track t in track.Tracks)
            {
                if (track.Tracks.IndexOf(t) + 1 == index)
                {
                    tra = t;
                    startlatlon = GetStartLatLon(t);
                    startxy = GetUtmXY(startlatlon[0], startlatlon[1]);
                    xy.Add(startxy);
                    break;
                }
            }
            foreach (TrackPieceGIS tpg in trackgis.TrackPieceGIS)
            {
                if (tpg.TrackID == index)
                {
                    foreach (TrackPiece p in tpg.TrackPieces)
                    {
                        if (tpg.TrackPieces.IndexOf(p) == 0)
                        {
                            continue;
                        }
                        else
                        {
                            xy.Add(GetUtmXY(ConvertMsToDigitalDegree(p.DeltaLat + tra.StartLat), ConvertMsToDigitalDegree(p.DeltaLon + tra.StartLon)));
                        }
                    }
                    break;
                }
            }
            endlatlon = GetEndLatLon(tra);
            xy.Add(GetUtmXY(endlatlon[0], endlatlon[1]));
            return xy;
        }
        /// <summary>
        /// 返回一个轨道所有的轨道片点的经纬度单位数字度
        /// </summary>
        /// <param name="track"></param>
        /// <param name="trackgis"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private List<double[]> GetLatLon(TrackInfo track, TrackGIS trackgis, int index)
        {
            List<double[]> latlon = new List<double[]>();
            double[] startxy = new double[2];
            double[] startlatlon = new double[2];
            double[] endlatlon = new double[2];
            Track tra = null;
            foreach (Track t in track.Tracks)
            {
                if (track.Tracks.IndexOf(t) + 1 == index)
                {
                    tra = t;
                    startlatlon = GetStartLatLon(t);
                    latlon.Add(startlatlon);
                    break;
                }
            }
            foreach (TrackPieceGIS tpg in trackgis.TrackPieceGIS)
            {
                if (tpg.TrackID == index)
                {
                    foreach (TrackPiece p in tpg.TrackPieces)
                    {
                        if (tpg.TrackPieces.IndexOf(p) == 0)
                        {
                            continue;
                        }
                        else
                        {
                            double[] bl = new double[2];
                            bl[0] = ConvertMsToDigitalDegree(p.DeltaLat + tra.StartLat);
                            bl[1] = ConvertMsToDigitalDegree(p.DeltaLon + tra.StartLon);
                            latlon.Add(bl);
                        }
                    }
                    break;
                }
            }
            endlatlon = GetEndLatLon(tra);
            latlon.Add(endlatlon);
            return latlon;
        }
        /// <summary>
        /// 返回arcid下的xy
        /// </summary>
        /// <param name="station"></param>
        /// <param name="TrackArcid"></param>
        /// <returns></returns>
        private List<double[]> GetArcXY(Station station, int TrackArcid)
        {
            return GetXY(station.TrackInfo, station.TrackInfo.TrackGIS, TrackArcid);
        }
        private List<double[]> GetArcLatLon(Station station, int TrackArcid)
        {
            return GetLatLon(station.TrackInfo, station.TrackInfo.TrackGIS, TrackArcid);
        }
        /// <summary>
        /// 返回轨道的起始纬度和经度
        /// </summary>
        /// <param name="track">Xml序列化Track类</param>
        /// <returns>转换成数字度的二维数组，格式[0]=lat，格式[1]=lon</returns>
        private double[] GetStartLatLon(Track track)
        {
            double[] latlon = new double[2];
            latlon[0] = ConvertMsToDigitalDegree(track.StartLat);
            latlon[1] = ConvertMsToDigitalDegree(track.StartLon);
            return latlon;
        }
        /// <summary>
        /// 返回结束纬度和经度
        /// </summary>
        /// <param name="track">Xml序列化Track类</param>
        /// <returns>转换成数字度的二维数组，格式[0]=lat，格式[1]=lon</returns>
        private double[] GetEndLatLon(Track track)
        {
            double[] latlon = new double[2];
            latlon[0] = ConvertMsToDigitalDegree(track.EndLat);
            latlon[1] = ConvertMsToDigitalDegree(track.EndLon);
            return latlon;
        }
        /// <summary>
        /// 返回经纬度转换后的utm坐标
        /// </summary>
        /// <param name="lat">数字纬度</param>
        /// <param name="lon">数字经度</param>
        /// <returns>数组中是x坐标，y坐标</returns>
        private double[] GetUtmXY(double lat, double lon)
        {
            double[] xy = new double[2];
            xy[0] = LatLonToUTM(lat, lon)[0];
            xy[1] = LatLonToUTM(lat, lon)[1];
            return xy;
        }
        /// <summary>
        /// 返回经纬度转换后的utm坐标
        /// </summary>
        /// <param name="latlon">数字纬度经度的二维数组,格式[0]lat,[1]lon</param>
        /// <returns>数组中是x坐标，y坐标，单位M</returns>
        private double[] GetUtmXY(double[] latlon)
        {
            double[] xy = new double[2];
            xy[0] = LatLonToUTM(latlon[0], latlon[1])[0];
            xy[1] = LatLonToUTM(latlon[0], latlon[1])[1];
            return xy;
        }
        #endregion
        #region 设置轨道片的长度，航向角
        /// <summary>
        /// 求轨道片弧的长度
        /// </summary>
        /// <param name="A">同一个弧段起点</param>
        /// <param name="B">同一个弧段的终点</param>
        /// <returns></returns>
        private double GetLength(TopologyElement.Point A, TopologyElement.Point B)
        {
            double dx = A.X - B.X;
            double dy = A.Y - B.Y;
            return Math.Sqrt(Math.Pow(dx, 2.0) + Math.Pow(dy, 2.0));
        }
        /// <summary>
        /// 计算航向角，已经在内计算转东经北纬坐标
        /// </summary>
        /// <param name="lat_a"></param>
        /// <param name="lng_a"></param>
        /// <param name="lat_b"></param>
        /// <param name="lng_b"></param>
        /// <returns></returns>
        private double GetHeading(double lat_a, double lng_a, double lat_b, double lng_b)
        {
            double[] utmlatlona = LatLonToUTM(lat_a, lng_a);
            double[] utmlatlonb = LatLonToUTM(lat_b, lng_b);
            //单位角度
            double deltaE = utmlatlona[0] - utmlatlonb[0];//纬度y坐标
            double deltaN = utmlatlona[1] - utmlatlonb[1];//经度x坐标
            double heading = Math.Atan2(deltaE, deltaN);
            //转角度
            heading = heading * 180 / Math.PI;
            if (heading < 0)
            {
                heading = (heading + 2 * Math.PI) * 180 / Math.PI;
            }
            else
            {
            }
            return heading;
        }
        /// <summary>
        /// 计算航向角
        /// </summary>
        /// <param name="y_a">UTM纬度坐标</param>
        /// <param name="x_a">UTM经度坐标</param>
        /// <param name="y_b">UTM纬度坐标</param>
        /// <param name="x_b">UTM经度坐标</param>
        /// <returns>航向角</returns>
        private double GetHeading2(double y_a, double x_a, double y_b, double x_b)
        {
            //单位角度
            double deltaE = x_a - x_b;//纬度y坐标
            double deltaN = y_a - y_b;//经度x坐标
            double heading = Math.Atan2(deltaE, deltaN);
            //转角度
            heading = heading * 180 / Math.PI;
            if (heading < 0)
            {
                heading = (heading + 2 * Math.PI) * 180 / Math.PI;
            }
            else
            {
            }
            return heading;
        }
        private double GetHeading(TopologyElement.Point A,TopologyElement.Point B)
        {
            return GetHeading(A.Lat, A.Lon, B.Lat, B.Lon);
        }
        #endregion
        #region 单位转换
        /// <summary>
        /// 数字度转字符串度分秒    eg:122.286791987025---->122°17′12.25″
        /// </summary>
        /// <param name="digitalDegree"></param>
        /// <returns></returns>
        public string ConvertDigitalToDegrees(double digitalDegree)
        {
            const double num = 60;
            int dms_d = (int)digitalDegree;
            double temp = (digitalDegree - dms_d) * num;
            int dms_m = (int)temp;
            double dms_s = (temp - dms_m) * num;
            //dms_s = Math.Round(dms_s, 2);
            string degree = "" + dms_d + "°" + dms_m + "'" + dms_s + "″";
            return degree;
        }
        /// <summary>
        /// 数字度转毫秒    eg：122.286791987025--->秒440232.451--  *1000得到毫秒440232451
        /// </summary>
        /// <param name="digitalDegree"></param>
        /// <param name="s"></param>
        /// <param name="ss"></param>
        public void ConvertDigitalToSecond(string digitalDegree, out string s, out string ss)
        {
            double dDegree = Convert.ToDouble(digitalDegree);
            const double num = 60.0000;
            int dms_d = (int)dDegree;
            double temp = (dDegree - dms_d) * num;
            int dms_m = (int)temp;
            double dms_s = (temp - dms_m) * num;
            //dms_s = Math.Round(dms_s, 3);
            double second = dms_d * num * num + dms_m * num + dms_s;
            s = second.ToString();
            ss = (second * 1000.0).ToString();
        }
        /// <summary>
        /// 度分秒转数字度
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public double ConvertDegreesToDigital(string degrees)
        {
            const double num = 60;
            double digitalDegree = 0.0;
            int d = degrees.IndexOf('°');
            if (d < 0)
            {
                return digitalDegree;
            }
            string degree = degrees.Substring(0, d);
            digitalDegree += Convert.ToDouble(degree);
            int m = degrees.IndexOf('′');//分的符号对应的 Unicode 代码为：2032[1]（六十进制），显示为′。
            if (m < 0)
            {
                return digitalDegree;
            }
            string minute = degrees.Substring(d + 1, m - d - 1);
            digitalDegree += ((Convert.ToDouble(minute)) / num);
            int s = degrees.IndexOf('″');           //秒的符号对应的 Unicode 代码为：2033[1]（六十进制），显示为″。
            if (s < 0)
            {
                return digitalDegree;
            }
            string second = degrees.Substring(m + 1, s - m - 1);
            digitalDegree += (Convert.ToDouble(second) / (num * num));
            return digitalDegree;
        }
        /// <summary>
        /// 毫秒转数字度
        /// </summary>
        /// <param name="ms">毫秒单位</param>
        /// <returns>保留7位小数</returns>
        public double ConvertMsToDigitalDegree(double ms)
        {
            return ms / 3600000.000;
            //return Math.Round(ms / 3600000, 7);
        }
        #endregion
        #region   WGS2UTM坐标转换
        static double pi = Math.PI;
        static double sm_a = 6378137.0;
        static double sm_b = 6356752.314;
        //static double sm_EccSquared = 6.69437999013e-03;
        static double UTMScaleFactor = 0.9996;
        /// <summary>
        /// 得到的结果是：x坐标，y坐标，区域编号
        /// </summary>
        /// <param name="lat">纬度(数字度)</param>
        /// <param name="lon">经度(数字度)</param>
        /// <returns>x坐标，y坐标，区域编号，单位米制</returns>
        public static double[] LatLonToUTM(double lat, double lon)
        {
            double zone = Math.Floor((lon + 180.0) / 6) + 1;
            double cm = UTMCentralMeridian(zone);
            double[] xy = new double[2];
            MapLatLonToXY(lat / 180.0 * pi, lon / 180 * pi, cm, out xy);
            /* Adjust easting and northing for UTM system. */
            xy[0] = xy[0] * UTMScaleFactor + 500000.0;
            xy[1] = xy[1] * UTMScaleFactor;
            if (xy[1] < 0.0)
            {
                xy[1] = xy[1] + 10000000.0;
            }
            return new double[] { xy[0], xy[1], zone };
        }
        public static double UTMCentralMeridian(double zone)
        {
            double cmeridian;
            double deg = -183.0 + (zone * 6.0);
            cmeridian = deg / 180.0 * pi;
            return cmeridian;
        }
        internal static void MapLatLonToXY(double phi, double lambda, double lambda0, out double[] xy)
        {
            double N, nu2, ep2, t, t2, l;
            double l3coef, l4coef, l5coef, l6coef, l7coef, l8coef;
            double tmp;
            /* Precalculate ep2 */
            ep2 = (Math.Pow(sm_a, 2.0) - Math.Pow(sm_b, 2.0)) / Math.Pow(sm_b, 2.0);
            /* Precalculate nu2 */
            nu2 = ep2 * Math.Pow(Math.Cos(phi), 2.0);
            /* Precalculate N */
            N = Math.Pow(sm_a, 2.0) / (sm_b * Math.Sqrt(1 + nu2));
            /* Precalculate t */
            t = Math.Tan(phi);
            t2 = t * t;
            tmp = (t2 * t2 * t2) - Math.Pow(t, 6.0);
            /* Precalculate l */
            l = lambda - lambda0;
            /* Precalculate coefficients for l**n in the equations below
               so a normal human being can read the expressions for easting
               and northing
               -- l**1 and l**2 have coefficients of 1.0 */
            l3coef = 1.0 - t2 + nu2;
            l4coef = 5.0 - t2 + 9 * nu2 + 4.0 * (nu2 * nu2);
            l5coef = 5.0 - 18.0 * t2 + (t2 * t2) + 14.0 * nu2
                - 58.0 * t2 * nu2;
            l6coef = 61.0 - 58.0 * t2 + (t2 * t2) + 270.0 * nu2
                - 330.0 * t2 * nu2;
            l7coef = 61.0 - 479.0 * t2 + 179.0 * (t2 * t2) - (t2 * t2 * t2);
            l8coef = 1385.0 - 3111.0 * t2 + 543.0 * (t2 * t2) - (t2 * t2 * t2);
            /* Calculate easting (x) */
            xy = new double[2];
            xy[0] = N * Math.Cos(phi) * l
                + (N / 6.0 * Math.Pow(Math.Cos(phi), 3.0) * l3coef * Math.Pow(l, 3.0))
                + (N / 120.0 * Math.Pow(Math.Cos(phi), 5.0) * l5coef * Math.Pow(l, 5.0))
                + (N / 5040.0 * Math.Pow(Math.Cos(phi), 7.0) * l7coef * Math.Pow(l, 7.0));
            /* Calculate northing (y) */
            xy[1] = ArcLengthOfMeridian(phi)
                + (t / 2.0 * N * Math.Pow(Math.Cos(phi), 2.0) * Math.Pow(l, 2.0))
                + (t / 24.0 * N * Math.Pow(Math.Cos(phi), 4.0) * l4coef * Math.Pow(l, 4.0))
                + (t / 720.0 * N * Math.Pow(Math.Cos(phi), 6.0) * l6coef * Math.Pow(l, 6.0))
                + (t / 40320.0 * N * Math.Pow(Math.Cos(phi), 8.0) * l8coef * Math.Pow(l, 8.0));
            return;
        }
        internal static double ArcLengthOfMeridian(double phi)
        {
            double alpha, beta, gamma, delta, epsilon, n;
            double result;
            /* Precalculate n */
            n = (sm_a - sm_b) / (sm_a + sm_b);
            /* Precalculate alpha */
            alpha = ((sm_a + sm_b) / 2.0)
               * (1.0 + (Math.Pow(n, 2.0) / 4.0) + (Math.Pow(n, 4.0) / 64.0));
            /* Precalculate beta */
            beta = (-3.0 * n / 2.0) + (9.0 * Math.Pow(n, 3.0) / 16.0)
               + (-3.0 * Math.Pow(n, 5.0) / 32.0);
            /* Precalculate gamma */
            gamma = (15.0 * Math.Pow(n, 2.0) / 16.0)
                + (-15.0 * Math.Pow(n, 4.0) / 32.0);
            /* Precalculate delta */
            delta = (-35.0 * Math.Pow(n, 3.0) / 48.0)
                + (105.0 * Math.Pow(n, 5.0) / 256.0);
            /* Precalculate epsilon */
            epsilon = (315.0 * Math.Pow(n, 4.0) / 512.0);
            /* Now calculate the sum of the series and return */
            result = alpha
                * (phi + (beta * Math.Sin(2.0 * phi))
                    + (gamma * Math.Sin(4.0 * phi))
                    + (delta * Math.Sin(6.0 * phi))
                    + (epsilon * Math.Sin(8.0 * phi)));
            return result;
        }
        #endregion

        #endregion

        #endregion

        #region  菜单栏操作

        #region 文件栏
        private void btnNew_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
        }
        private void btnOpen_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetOFD(false, "Xml文件(*.xml)|*.xml");
            if (oFD.ShowDialog() == DialogResult.OK)
            {
                string file = oFD.FileName;
                if (!IsHaveFile(file))
                {
                    OpenFileTree(file, true);
                    //AddFileRow(file);
                    AddFileRow(file, false, true, true, true);
                }
            }
        }

       
        /// <summary>
        /// FilePaths是否含有当前文件
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <returns></returns>
        private bool IsHaveFile(string filename)
        {
            if(FilePaths.Contains(filename))
            {
                return true;
            }
            else//不含当前文件则添加路径
            {
                FilePaths.Add(filename);
                return false;
            }
        }
        
        private void btnSavee_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetSFD(oFD.InitialDirectory, "Xml文件(*.xml)|*.xml");
            if (sFD.ShowDialog() == DialogResult.OK)
            {
                Station station = new Station();
                ///TrackInfo
                TrackInfo trackinfo = new TrackInfo();
                trackinfo.Tracks = SerializeTracks();
                trackinfo.TrackGIS = SerializeTrackGIS();
                trackinfo.TrackFileProperty = SerializeTrackFileProperty();
                station.TrackInfo = trackinfo;
                ///TrainControlData
                TrainControlData trainControlData = SerializeTrainControlData();
                station.TrainControlData = trainControlData;
                ///StationPoperties
                StationProperties stationProperties = SerializeStationProperties();
                station.StationProperties = stationProperties;
                //
                ToXmlString(station, sFD.FileName);
            }
        }
        private void btnSaveAs_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
        }
        private void btnSavePDF_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
        }
        private void btnXmlToBin_ItemClick(object sender, ItemClickEventArgs e)
        {
        }
        private void btnAddFileModel_ItemClick(object sender, ItemClickEventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(string.Format(@"{0}..\..\..\", System.Windows.Forms.Application.StartupPath));
            string filepath =di.FullName + "工程文件\\model.xml";
            if (File.Exists(filepath))
            {
                OpenFileTree(filepath,false);
            }
            else
            {
                XtraMessageBox.Show("模板文件不存在或者更名！");
            }
        }

        private void btnCloseFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            //想要关闭的其实是xml文件
            //DoDelete();//关闭地图文件
        }

        #region NPOI操作Excel文件
        private void btnAddExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            SetSFD(GetDirectoryInfo(3).FullName, "Excel2007 files(*.xlsx)|*.xlsx");
            if (sFD.ShowDialog() == DialogResult.OK)
            {
                DataTable dt = (DataTable)this.tracks_gctrl.DataSource;
                DataTable dt2 = (DataTable)this.trackPieces_gctrl.DataSource;
                DataTable dt3 = (DataTable)this.stationEdgeData_gctrl.DataSource;
                DataTable dt4 = (DataTable)this.switchdata_gctrl.DataSource;
                DataTable dt5 = (DataTable)this.balisedata_gctrl.DataSource;
                //
                XSSFWorkbook workBook = new XSSFWorkbook();
                //ExportToXlxs(dt, sFD.FileName);//生成一个表
                //生成多个表
                try
                {
                    CreateSheet(workBook, dt, dt.TableName);
                    CreateSheet(workBook, dt2, dt2.TableName);
                    CreateSheet(workBook, dt3, dt3.TableName);
                    CreateSheet(workBook, dt4, dt4.TableName);
                    CreateSheet(workBook, dt5, dt5.TableName);
                    string path = sFD.FileName;
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    using (FileStream file = new FileStream(path, FileMode.Create))
                    {
                        workBook.Write(file);  //创建Excel文件。
                        file.Close();
                    }
                    string content = string.Format("保存至{0}", sFD.FileName);
                    XtraMessageBox.Show(content);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message);
                }
            }
        }
        /// <summary>
        /// 导出表格数据到xlsx
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="filePath">xlsx文件</param>
        public  void ExportToXlxs(DataTable dt,string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && null != dt && dt.Rows.Count > 0)
            {
                XSSFWorkbook book = new XSSFWorkbook();
                ISheet sheet = book.CreateSheet(dt.TableName);
                IRow row = sheet.CreateRow(0);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(dt.Columns[i].Caption);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    row = sheet.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        row.CreateCell(j).SetCellValue(Convert.ToString(dt.Rows[i][j]));
                    }
                }
                ///设置列的宽度
                for (int k = 0; k < dt.Columns.Count; k++)
                {
                    string colName = dt.Columns[k].ColumnName.ToString();
                    int colNameLength = colName.Length;
                    int colMaxLength = GetMaxLength(dt, colName);
                    if(colNameLength<=colMaxLength)
                    {
                        sheet.SetColumnWidth(k, (colMaxLength + 4) * 256);
                    }
                    else
                    {
                        sheet.SetColumnWidth(k, (colNameLength + 4) * 256);
                    }
                }
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    book.Write(fs);
                }
                book = null;
            }
            else
            {
                XtraMessageBox.Show("数据为空！");
            }
        }
        private ISheet CreateSheet(XSSFWorkbook workBook, DataTable dt, string sheetName)
        {
            ISheet sheet = null;
            //workBook.CreateSheet(sheetName);
            if (null != dt && dt.Rows.Count > 0)
            {
                sheet=workBook.CreateSheet(sheetName);
                IRow row = sheet.CreateRow(0);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(dt.Columns[i].Caption);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    row = sheet.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        row.CreateCell(j).SetCellValue(Convert.ToString(dt.Rows[i][j]));
                    }
                }
                ///设置列的宽度
                for (int k = 0; k < dt.Columns.Count; k++)
                {
                    string colName = dt.Columns[k].ColumnName.ToString();
                    int colNameLength = colName.Length;
                    int colMaxLength = GetMaxLength(dt, colName);
                    if (colNameLength <= colMaxLength)
                    {
                        sheet.SetColumnWidth(k, (colMaxLength + 4) * 256);
                    }
                    else
                    {
                        sheet.SetColumnWidth(k, (colNameLength + 4) * 256);
                    }
                }
                return sheet;
            }
            else
            {
                sheet = null;
                return sheet;
            }
        }
        /// <summary>
        /// 计算DataTable的整列字符串长度最大值
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="coulumindex">列索引值</param>
        /// <returns></returns>
        private static int GetMaxLength(DataTable dt,string colName)
        {
            System.Data.DataColumn maxlengthColumn = new System.Data.DataColumn();//重新定义一列，用来存放内容的长度
            maxlengthColumn.ColumnName = "MaxLength";
            maxlengthColumn.DataType = typeof(int);//新列类型
            //其中name为列名,用到的函数为len
            maxlengthColumn.Expression = "len(convert("+colName+",'System.String'))";//len用来计算长度
            dt.Columns.Add(maxlengthColumn);
            object maxLength = dt.Compute("Max(MaxLength)", "");//max表达式进行计算
            dt.Columns.Remove(maxlengthColumn);//移除新增列
            try
            {
                return Convert.ToInt32(maxLength);
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
                return 0;
            }
        }
        /// <summary>        
        /// 从 Xlxs文件导入数据到 DataTable       
        /// </summary>       
        /// <param name="filePath"></param>       
        /// <param name="SheetName">工作表名称</param>        
        /// <returns></returns>        
        public static DataTable ImportToDataTable(string filePath, String SheetName)
        {
            string extension = System.IO.Path.GetExtension(filePath);
            XSSFWorkbook xssfworkbook;
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                xssfworkbook = new XSSFWorkbook(file);//.xlsx
            }
            ISheet sheet = xssfworkbook.GetSheet(SheetName);
            if (sheet == null)
            {
                return new DataTable();
            }
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
            DataTable dt = new DataTable(SheetName);
            for (int j = 0; j < (sheet.GetRow(0).LastCellNum); j++)
            {
                ICell cell = sheet.GetRow(0).Cells[j];
                dt.Columns.Add(cell.ToString());
            }
            rows.MoveNext();
            while (rows.MoveNext())
            {
                XSSFRow row = (XSSFRow)rows.Current;
                int rowindex = row.RowNum;//行指数
                int lastcolumn = NotNullCellCount(row) - 1;
                if (lastcolumn != 0)//如果row不是合并项,即有内容的cell个数不为1
                {
                    DataRow Row = dt.NewRow();
                    for (int i = 0; i < lastcolumn; i++)
                    {
                        ICell cell = row.GetCell(i);
                        Row[i] = cell == null ? null : cell.ToString();
                    }
                    dt.Rows.Add(Row);
                }
            }
            xssfworkbook = null;
            return dt;
        }
        /// <summary>
        /// 从 Xlxs文件导入数据到 DataTable 
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="SheetIndex">工作表的索引</param>
        /// <param name="headerindex">该工作表标题的行数</param>
        /// <returns></returns>
        public static DataTable ImportToDataTable(string filePath, int SheetIndex,int headerindex)
        {
            XSSFWorkbook xssfworkbook;
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                xssfworkbook = new XSSFWorkbook(file);
            }
            ISheet sheet = xssfworkbook.GetSheetAt(SheetIndex);
            String SheetName = xssfworkbook.GetSheetName(SheetIndex);
            string filename = filePath.Substring(filePath.LastIndexOf("\\"));
            if (sheet == null)
            {
                return new DataTable();
            }
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
            DataTable dt = new DataTable(filename + SheetName);
            for (int j = 0; j < NotNullCellCount((XSSFRow)sheet.GetRow(headerindex)); j++)
            {
                ICell cell = sheet.GetRow(headerindex).Cells[j];
                dt.Columns.Add(cell.ToString());
            }
            while (rows.MoveNext())
            {
                XSSFRow row = (XSSFRow)rows.Current;
                int rowindex = row.RowNum;//行指数
                int lastcolumn = NotNullCellCount(row)-1 ;
                if(lastcolumn!=0&&lastcolumn!=-1)//如果row不是合并项或者不属于本表,即有内容的cell个数不为1或0
                {
                    DataRow Row = dt.NewRow();
                    for (int i = 0; i < lastcolumn; i++)
                    {
                        ICell cell = row.GetCell(i);
                        Row[i] = cell == null ? null : cell.ToString();
                    }
                    dt.Rows.Add(Row);
                }
            }
            xssfworkbook = null;
            return dt;
        }
        /// <summary>
        /// 返回不为空的cell值
        /// </summary>
        /// <param name="Row"></param>
        /// <returns></returns>
        public static int NotNullCellCount(XSSFRow Row)
        {
            int j = 0;
            for (int i = 0; i < Row.Cells.Count; i++)
            {
                ICell cell = Row.GetCell(i);
                Console.WriteLine(Row.PhysicalNumberOfCells.ToString());
                if(!string.IsNullOrEmpty(GetCellValue(cell).ToString().Trim()))
                {
                    j++;
                }
            }
            return j;
        }
        /// <summary>
        /// 返回不为空的cell值
        /// </summary>
        /// <param name="Row"></param>
        /// <returns></returns>
        public static int NotNullCellCount(HSSFRow Row)
        {
            int j = 0;
            for (int i = 0; i < Row.Cells.Count; i++)
            {
                ICell cell = Row.GetCell(i);
                Console.WriteLine(Row.PhysicalNumberOfCells.ToString());
                if (!string.IsNullOrEmpty(GetCellValue(cell).ToString().Trim()))
                {
                    j++;
                }
            }
            return j;
        }
        /// <summary>
        /// 获取单元格的值
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static object GetCellValue(ICell item)
        {
            if (item == null)
            {
                return string.Empty;
            }
            switch (item.CellType)
            {
                case CellType.BOOLEAN:
                    return item.BooleanCellValue;
                case CellType.ERROR:
                    return ErrorEval.GetText(item.ErrorCellValue);
                case CellType.FORMULA:
                    switch (item.CachedFormulaResultType)
                    {
                        case CellType.BOOLEAN:
                            return item.BooleanCellValue;
                        case CellType.ERROR:
                            return ErrorEval.GetText(item.ErrorCellValue);
                        case CellType.NUMERIC:
                            if (DateUtil.IsCellDateFormatted(item))
                            {
                                return item.DateCellValue.ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                return item.NumericCellValue;
                            }
                        case CellType.STRING:
                            string str = item.StringCellValue;
                            if (!string.IsNullOrEmpty(str))
                            {
                                return str.ToString();
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case CellType.Unknown:
                        case CellType.BLANK:
                        default:
                            return string.Empty;
                    }
                case CellType.NUMERIC:
                    if (DateUtil.IsCellDateFormatted(item))
                    {
                        return item.DateCellValue.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        return item.NumericCellValue;
                    }
                case CellType.STRING:
                    string strValue = item.StringCellValue;
                    return strValue.ToString().Trim();
                case CellType.Unknown:
                case CellType.BLANK:
                default:
                    return string.Empty;
            }
        }
        /// <summary>        
        /// 从 Xls文件导入数据到 DataTable       
        /// </summary>       
        /// <param name="filePath"></param>       
        /// <param name="SheetName">工作表名称</param>        
        /// <returns></returns>        
        public static DataTable ImportToDataTable2(string filePath, String SheetName)
        {
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);//.xlsx
            }
            ISheet sheet = hssfworkbook.GetSheet(SheetName);
            if (sheet == null)
            {
                return new DataTable();
            }
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
            DataTable dt = new DataTable(SheetName);
            for (int j = 0; j < (sheet.GetRow(0).LastCellNum); j++)
            {
                ICell cell = sheet.GetRow(0).Cells[j];
                dt.Columns.Add(cell.ToString());
            }
            rows.MoveNext();
            while (rows.MoveNext())
            {
                HSSFRow row = (HSSFRow)rows.Current;
                int rowindex = row.RowNum;//行指数
                int lastcolumn = NotNullCellCount(row) - 1;
                if (lastcolumn != 0)//如果row不是合并项,即有内容的cell个数不为1
                {
                    DataRow Row = dt.NewRow();
                    for (int i = 0; i < lastcolumn; i++)
                    {
                        ICell cell = row.GetCell(i);
                        Row[i] = cell == null ? null : cell.ToString();
                    }
                    dt.Rows.Add(Row);
                }
            }
            hssfworkbook = null;
            return dt;
        }
        /// <summary>
        /// 从 Xls文件导入数据到 DataTable 
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="SheetIndex">工作表指数</param>
        /// <returns></returns>
        public static DataTable ImportToDataTable2(string filePath, int SheetIndex)
        {
            HSSFWorkbook hssfworkbook;
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
            }
            ISheet sheet = hssfworkbook.GetSheetAt(SheetIndex);
            String SheetName = hssfworkbook.GetSheetName(SheetIndex);
            if (sheet == null)
            {
                return new DataTable();
            }
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
            DataTable dt = new DataTable(SheetName);
            for (int j = 0; j < (sheet.GetRow(0).LastCellNum); j++)
            {
                ICell cell = sheet.GetRow(0).Cells[j];
                dt.Columns.Add(cell.ToString());
            }
            rows.MoveNext();
            rows.MoveNext();
            while (rows.MoveNext())
            {
                HSSFRow row = (HSSFRow)rows.Current;
                int rowindex = row.RowNum;//行指数
                int lastcolumn = NotNullCellCount(row) - 1;
                if (lastcolumn != 0)//如果row不是合并项,即有内容的cell个数不为1
                {
                    DataRow Row = dt.NewRow();
                    for (int i = 0; i < lastcolumn; i++)
                    {
                        ICell cell = row.GetCell(i);
                        Row[i] = cell == null ? null : cell.ToString();
                    }
                    dt.Rows.Add(Row);
                }
            }
            hssfworkbook = null;
            return dt;
        }
        #endregion
        
        #region 添加历史文件
        private void AddHistoryFile(string path)
        {
            try
            {
                RegistryKey software = Registry.CurrentUser.OpenSubKey("SOFTWARE", true); //打开注册表中的software       
                RegistryKey ReadKey = software.OpenSubKey("FolderPath", true);// 打开自定义的文件目录项
                if (ReadKey == null)
                {
                    ReadKey = software.CreateSubKey("FolderPath");  //不存在则新建
                    ReadKey.SetValue("OpenFolderDir", path);   //写入文件目录
                    ReadKey.Close();
                    Registry.CurrentUser.Close();
                }
                else
                {
                    ReadKey.SetValue("OpenFolderDir", path);
                    ReadKey.Close();
                    Registry.CurrentUser.Close();
                }
            }
            catch (Exception)
            {
            }
        }
        private void ReadHistoryFile()
        {
            try
            {
                RegistryKey software = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
                RegistryKey ReadKey = software.OpenSubKey("FolderPath", true);
                string path = ReadKey.GetValue("OpenFolderDir").ToString();
                //TO DO
            }
            catch (Exception ex) 
            {
                //TO DO
                WriteLog(ex.Message);
            }
        }
        #endregion

        #endregion
        
        #region 站场图栏
        #endregion

        #region GIS栏

        #region 地图文件框复选框改变事件
        //选择
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (!gridView8.IsNewItemRow(gridView1.FocusedRowHandle))
            {
                gridView8.CloseEditor();
                gridView8.UpdateCurrentRow();
            }
            CheckState check = (sender as DevExpress.XtraEditors.CheckEdit).CheckState;
            if (check == CheckState.Checked)
            {
                //事件
            }
        }
        //连线
        private void repositoryItemCheckEdit2_CheckedChanged(object sender, EventArgs e)
        {
            if (!gridView8.IsNewItemRow(gridView8.FocusedRowHandle))
            {
                gridView8.CloseEditor();
                gridView8.UpdateCurrentRow();
            }
            CheckState check = (sender as DevExpress.XtraEditors.CheckEdit).CheckState;
            int index = this.gridView8.FocusedRowHandle;
            string name = this.gridView8.GetRowCellDisplayText(index, "Name");
            if (check == CheckState.Checked)
            {
                //事件
                var overlay = gMapOverlays.Find(a => a.Id == name.Trim());
                var color = customAttributes.Find(a => a.LineName == overlay.Id).LineColor;
                if (overlay.Markers.Count < 50)
                {
                    for (int j = 0; j < overlay.Markers.Count - 1; j++)
                    {
                        GMapRouteExt route = TwoPointToRoute(overlay.Markers[j].Position, overlay.Markers[j + 1].Position, j, color);
                        route.IsVisible = true; 
                        overlay.Routes.Add(route);
                    }
                }
            }
            else
            {
                var overlay= gMapOverlays.Find(a => a.Id == name.Trim());
                overlay.Routes.Clear();
            }
        }
        //锁定
        private void repositoryItemCheckEdit3_CheckedChanged(object sender, EventArgs e)
        {
            if (!gridView8.IsNewItemRow(gridView8.FocusedRowHandle))
            {
                gridView8.CloseEditor();
                gridView8.UpdateCurrentRow();
            }
            ///
            CheckState check = (sender as DevExpress.XtraEditors.CheckEdit).CheckState;
            if (check == CheckState.Checked)
            {
                //事件
                DataTable dt = (DataTable)this.gridControl1.DataSource;
                XtraMessageBox.Show(dt.Columns[gridView8.FocusedRowHandle].ToString());
            }
        }
        //显示
        private void repositoryItemCheckEdit4_CheckedChanged(object sender, EventArgs e)
        {
            if (!gridView8.IsNewItemRow(gridView8.FocusedRowHandle))
            {
                gridView8.CloseEditor();
                gridView8.UpdateCurrentRow();
            }
            CheckState check = (sender as DevExpress.XtraEditors.CheckEdit).CheckState;
            int index = this.gridView8.FocusedRowHandle;
            string name = this.gridView8.GetRowCellDisplayText(index, "Name");
            //
            var overlay = gMapOverlays.Find(a => a.Id == name.Trim());
            var attribute = customAttributes.Find(a => a.LineName == overlay.Id);
            if (check == CheckState.Checked)
            {
                //事件
                overlay.IsVisibile = true;
                attribute.IsVisible = false;
            }
            else
            {
                overlay.IsVisibile = false;
                attribute.IsVisible = true;
            }
            SetCurrentCustomAttribute(overlay.Id);
            this.propertyGrid.Refresh();
        }

        private void SetAllIsVisible()
        {
            bool state = Convert.ToBoolean(this.gridView8.GetRowCellValue(0, "IsOverlayVisible"));
            foreach (GMapOverlay overlay in gMapOverlays)
            {
                SetIsVisible(overlay.Id,state);
            }
        }

        private void SetIsVisible(string overlayname,bool state)
        {
            var overlay = gMapOverlays.Find(a => a.Id == overlayname.Trim());
            var attribute = customAttributes.Find(a => a.LineName == overlay.Id);
            if (state)
            {
                overlay.IsVisibile = true;
                attribute.IsVisible = false;
            }
            else
            {
                overlay.IsVisibile = false;
                attribute.IsVisible = true;
            }
        }

        private void SetAllIsLine()
        {
            bool state = Convert.ToBoolean(this.gridView8.GetRowCellValue(0, "IsLine"));
            foreach (GMapOverlay overlay in gMapOverlays)
            {
                SetIsLine(overlay.Id, state);
            }
        }
        private void SetIsLine(string overlayname,bool state)
        {
            var overlay = gMapOverlays.Find(a => a.Id == overlayname.Trim());
            if (state)
            {
                //事件
                var color = customAttributes.Find(a => a.LineName == overlay.Id).LineColor;
                if (overlay.Markers.Count < 50)
                {
                    for (int j = 0; j < overlay.Markers.Count - 1; j++)
                    {
                        GMapRouteExt route = TwoPointToRoute(overlay.Markers[j].Position, overlay.Markers[j + 1].Position, j, color);
                        route.IsVisible = true;
                        overlay.Routes.Add(route);
                    }
                    this.gmap.Refresh();
                }
            }
            else
            {
                overlay.Routes.Clear();
            }
        }

        private int GetRowFromName(string overlayname)
        {
            int index = 0;
            DataTable dt = (DataTable)this.gridControl1.DataSource;
            foreach(DataRow dr in dt.Rows)
            {
                if(dr["Name"].ToString()==overlayname)
                {
                    index = dt.Rows.IndexOf(dr);
                    break;
                }
            }
            return index;
        }


        #region  点击复选框改变数据表值
        private void repositoryItemCheckEdit1_QueryCheckStateByValue(object sender, DevExpress.XtraEditors.Controls.QueryCheckStateByValueEventArgs e)
        {
            string val = "";
            if (e.Value != null)
            {
                val = e.Value.ToString();
            }
            else
            {
                val = "True";//默认为选中 
            }
            switch (val)
            {
                case "True":
                    e.CheckState = CheckState.Checked;
                    break;
                case "False":
                    e.CheckState = CheckState.Unchecked;
                    break;
                case "Yes":
                    goto case "True";
                case "No":
                    goto case "False";
                case "1":
                    goto case "True";
                case "0":
                    goto case "False";
                default:
                    e.CheckState = CheckState.Checked;
                    break;
            }
            e.Handled = true;
        }
        private void repositoryItemCheckEdit2_QueryCheckStateByValue(object sender, DevExpress.XtraEditors.Controls.QueryCheckStateByValueEventArgs e)
        {
            string val = "";
            if (e.Value != null)
            {
                val = e.Value.ToString();
            }
            else
            {
                val = "True";//默认为选中 
            }
            switch (val)
            {
                case "True":
                    e.CheckState = CheckState.Checked;
                    break;
                case "False":
                    e.CheckState = CheckState.Unchecked;
                    break;
                case "Yes":
                    goto case "True";
                case "No":
                    goto case "False";
                case "1":
                    goto case "True";
                case "0":
                    goto case "False";
                default:
                    e.CheckState = CheckState.Checked;
                    break;
            }
            e.Handled = true;
        }
        private void repositoryItemCheckEdit3_QueryCheckStateByValue(object sender, DevExpress.XtraEditors.Controls.QueryCheckStateByValueEventArgs e)
        {
            string val = "";
            if (e.Value != null)
            {
                val = e.Value.ToString();
            }
            else
            {
                val = "True";//默认为选中 
            }
            switch (val)
            {
                case "True":
                    e.CheckState = CheckState.Checked;
                    break;
                case "False":
                    e.CheckState = CheckState.Unchecked;
                    break;
                case "Yes":
                    goto case "True";
                case "No":
                    goto case "False";
                case "1":
                    goto case "True";
                case "0":
                    goto case "False";
                default:
                    e.CheckState = CheckState.Checked;
                    break;
            }
            e.Handled = true;
        }
        private void repositoryItemCheckEdit4_QueryCheckStateByValue(object sender, DevExpress.XtraEditors.Controls.QueryCheckStateByValueEventArgs e)
        {
            string val = "";
            if (e.Value != null)
            {
                val = e.Value.ToString();
            }
            else
            {
                val = "True";//默认为选中 
            }
            switch (val)
            {
                case "True":
                    e.CheckState = CheckState.Checked;
                    break;
                case "False":
                    e.CheckState = CheckState.Unchecked;
                    break;
                case "Yes":
                    goto case "True";
                case "No":
                    goto case "False";
                case "1":
                    goto case "True";
                case "0":
                    goto case "False";
                default:
                    e.CheckState = CheckState.Checked;
                    break;
            }
            e.Handled = true;
        }
        private void gridView8_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            try
            {
                if (gMapOverlays.Count != 0)
                {
                    string key = this.gridView8.GetRowCellValue(e.FocusedRowHandle, "Name").ToString();
                    var Overlay = gMapOverlays.Find(a => a.Id == key.Trim());
                    SetPerfectPos(true, false, Overlay);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
                XtraMessageBox.Show(ex.Message);
            }
        }

        private void gridView8_Click(object sender, EventArgs e)
        {
            try
            {
                Point pt = gridControl1.PointToClient(Control.MousePosition);
                DevExpress.XtraGrid.Views.Grid.ViewInfo.GridHitInfo info = gridView8.CalcHitInfo(pt);
                if (info.InColumn && info.Column != null)
                {
                    string s = info.Column.FieldName.ToString();
                    switch (s)
                    {
                        case "IsChoose":
                            SetAllCheck("IsChoose", GetIsAllCheck("IsChoose"));
                            break;
                        case "IsOverlayVisible":
                            SetAllCheck("IsOverlayVisible", GetIsAllCheck("IsOverlayVisible"));
                            SetAllIsVisible();//全选/全不选的显示操作
                            break;
                        case "IsLine":
                            SetAllCheck("IsLine", GetIsAllCheck("IsLine"));
                            SetAllIsLine();//全选/全不选的连线操作
                            break;
                        case "IsLocked":
                            SetAllCheck("IsLocked", GetIsAllCheck("IsLocked"));
                            break;
                        default:
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
                XtraMessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 设置GridView8某列全选
        /// </summary>
        /// <param name="columnFieldName">列的FieldName</param>
        /// <param name="checkState">选中状态</param>
        private void SetAllCheck(string columFieldName, bool checkState)
        {
            for (int i = 0; i < gridView8.DataRowCount; i++)
            {
                gridView8.SetRowCellValue(i, gridView8.Columns[columFieldName], checkState);
            }
            gridControl1.Refresh();
            gridView8.RefreshData();
        }

        /// <summary>
        /// GridView8判断某列是否全选
        /// </summary>
        private bool GetIsAllCheck(string columnFieldName)
        {
            DataTable dt = (DataTable)this.gridControl1.DataSource;
            //如果不是全选，则返回全选，是全选返回未选
            List<bool> states = new List<bool>();
            for (int j = 0; j < dt.Rows.Count; j++)
            {
                var state = dt.Rows[j][columnFieldName];
                states.Add(Convert.ToBoolean(state));
            }
            if (states.TrueForAll(a => a))//判断是否全为false(全未选)-true;不是全选(即全为false)的情况下就返回false
            {
                //全为true返回false
                return false;
            }
            else
            {
                //否则返回true
                return true;
            }
        }




        #endregion

        #endregion

        #region 地图文件框添加文件
        /// <summary>
        /// 在地图文件添加新栏
        /// </summary>
        /// <param name="filename">文件名称</param>
        private void AddFileRow(string filename)
        {
            DataRow dr = chooseFiles.NewRow();
            dr["IsChoose"] = false;
            dr["IsOverlayVisible"] = true;
            dr["IsLine"] = true;
            dr["IsLocked"] = true;
            dr["Name"] = filename.Substring(filename.LastIndexOf("\\") + 1);
            chooseFiles.Rows.Add(dr);
        }
        /// <summary>
        /// 在文件管理栏添加新栏
        /// </summary>
        /// <param name="filename">文件名称</param>
        /// <param name="isChoose">图层是否选择</param>
        /// <param name="isOverlayVisible">图层是否显示</param>
        /// <param name="isLine">是否连线</param>
        /// <param name="isLocked">是否锁定</param>
        private void AddFileRow(string filename,bool isChoose,bool isOverlayVisible,bool isLine,bool isLocked)
        {
            DataRow dr = chooseFiles.NewRow();
            dr["IsChoose"] = isChoose;
            dr["IsOverlayVisible"] = isOverlayVisible;
            dr["IsLine"] = isLine;
            dr["IsLocked"] = isLocked;
            dr["Name"] = filename.Substring(filename.LastIndexOf("\\") + 1);
            chooseFiles.Rows.Add(dr);
        }
        private void btnAddLatFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            SetOFD(true, "文本文件(*.txt)|*.txt");
            if (oFD.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] filenames = oFD.FileNames;
                    foreach (string s in filenames)
                    {
                        if (!IsHaveFile(s))
                        {
                            List<string[]> latlngs = new List<string[]>();
                            ReadFormatTxt(s, ref latlngs);//读格式的文件返回经纬度
                            //之后想添加约简算法
                            ShowFileLatLng(latlngs, s, false);//在地图控件上显示文件
                           // AddFileRow(s, false, true, false, true);
                        }
                    }
                    OFDSaveLastFilePath(filenames[filenames.Count() - 1]);//根据最后一个文件设置下一次打开的路径
                    var overlay = gMapOverlays.Last();
                    SetPerfectPos(false, false, overlay);
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message);
                    XtraMessageBox.Show("地图文件格式不正确！");
                }
            }
        }

        private void btnSaveLatLng_ItemClick(object sender, ItemClickEventArgs e)
        {
            List<string> selectFiles = GetSelectFiles();
            if(selectFiles!=null&&selectFiles.Count!=0)
            {
                foreach(string s in selectFiles)
                {
                    SaveLatLng(s);
                    
                }
            }
        }
    
        #endregion

        #region 从Excel文件xlsx(列控数据表)中提取数据
        private void btnImportExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            SetOFD(false, "Excel表格文件(*.xlsx,*.xls)|*.xlsx;*.xls");
            if(oFD.ShowDialog()==DialogResult.OK)
            {
                string filetype = System.IO.Path.GetExtension(oFD.FileName);
                if (filetype == ".xlsx")
                {
                    XSSFWorkbook xSSFSheets = new XSSFWorkbook(oFD.FileName);
                    DataTable dt = ImportToDataTable(oFD.FileName, 0, 1);
                    DataTable dt2 = ImportToDataTable(oFD.FileName, 1, 1);
                    DataTable dt4 = ImportToDataTable(oFD.FileName, 3, 1);
                    //DataTable dt3 = ImportToDataTable(oFD.FileName, 2, 1);
                    //注意此处一个xlxs文件读取了三个DataTable表还有
                    AddExcelMap(dt, oFD.FileName);
                    AddExcelMap(dt2, oFD.FileName);
                    AddExcelMap(dt4, oFD.FileName);
                }
                else if (filetype == ".xls") 
                {
                    //XSSFWorkbook xSSFSheets = new XSSFWorkbook(oFD.FileName);
                    //for (int i = 0; i < xSSFSheets.NumberOfSheets; i++)
                    //{
                    //    DataTable dt = ImportToDataTable(oFD.FileName, i, 1);
                    //    AddExcelMap(dt, oFD.FileName);
                    //}
                }
            }
        }
        private void AddExcelMap(DataTable dt,string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && null != dt && dt.Rows.Count > 0)
            {
                DataTableToMap(dt, "纬度(毫秒)", "经度(毫秒)");
            }
        }
        /// <summary>
        /// 得到数据表中的纬度经度数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private void DataTableToMap(DataTable dt,string columnLatName,string columnLngName)
        {
            List<string[]> latlngs = new List<string[]>();
            //List<PointLatLng> points = new List<PointLatLng>();
            for (int i = 1; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                double[] latlng = new double[2];
                //PointLatLng p = new PointLatLng();
                //如果纬度列和经度列的内容不为空
                if (!string.IsNullOrEmpty(dr[columnLatName].ToString()) && !string.IsNullOrEmpty(dr[columnLngName].ToString()))
                {
                    double lat = Math.Round((Convert.ToDouble(dr[columnLatName].ToString().Trim()) / 3600000.0), 10);
                    double lng = Math.Round((Convert.ToDouble(dr[columnLngName].ToString().Trim()) / 3600000.0), 10);
                    latlng = transform(lat, lng);
                    latlngs.Add(new string[2] { lng.ToString(), lat.ToString() });
                }
            }
            ShowFileLatLng(latlngs, dt.TableName, true);
            //AddFileRow(dt.TableName, false, true, false, true);
        }
        #endregion

        #region 添加并显示地图文件

        /// <summary>
        /// 返回一个随机的枚举值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RandomEnum<T>()
        {
            T[] results = Enum.GetValues(typeof(T)) as T[];
            Random random = new Random();
            T result = results[random.Next(0, results.Length)];
            return result;
        }

        public GMarkerGoogleType GetRandomMarkerGoogleType()
        {
            int[] markerColor = new int[] { 0, 2, 8, 13, 18, 21, 24, 27, 31 };
            Random random = new Random();
            int index = random.Next(0, 9);
            GMarkerGoogleType type = (GMarkerGoogleType)markerColor[index];
            return type;
        }

        /// <summary>
        /// 返回随机颜色
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Color GetRandomColor()
        {
            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);
            //  对于C#的随机数，没什么好说的
            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);

            //  为了在白色背景上显示，尽量生成深色
            int int_Red = RandomNum_First.Next(256);
            int int_Green = RandomNum_Sencond.Next(256);
            int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;
            return System.Drawing.Color.FromArgb(int_Red, int_Green, int_Blue);
        }


        /// <summary>
        /// 显示地图文件
        /// </summary>
        /// <param name="lnglat">s[0]经度，s[1]纬度</param>
        /// <param name="filename">文件名称</param>
        /// <param name="isVisible">是否设置最佳视图</param>
        private void ShowFileLatLng(List<string[]> lnglat,string filename,bool isShowPerfectView)
        {
            try
            {
                GMapOverlay fileOverlay = new GMapOverlay();
                fileOverlay.Id = filename.Substring(filename.LastIndexOf("\\") + 1);
                List<PointLatLng> points = new List<PointLatLng>();
                int i = 0;
                var t = GetRandomMarkerGoogleType();
                var c = GetRandomColor();
                //第一个点
                double[] first = new double[2] { Convert.ToDouble(lnglat[0][0]), Convert.ToDouble(lnglat[0][1]) };//经度//纬度
                PointLatLng pfirst = GetTxtPoint(first);
                fileOverlay.Markers.Add(PointToCustomMarker(pfirst, 0, t));
                points.Add(pfirst);
                //
                for (int j = 1; j < lnglat.Count - 1; j++)
                {
                    double[] ll = new double[2] { Convert.ToDouble(lnglat[j][0]), Convert.ToDouble(lnglat[j][1]) };//经度//纬度
                    PointLatLng p = GetTxtPoint(ll);
                    fileOverlay.Markers.Add(PointToGoogleExt(p, j, t, c, true));
                    points.Add(p);
                }
                //最后一个点
                double[] last = new double[2] { Convert.ToDouble(lnglat[lnglat.Count - 1][0]), Convert.ToDouble(lnglat[lnglat.Count - 1][1]) };//经度//纬度
                PointLatLng plast = GetTxtPoint(last);
                fileOverlay.Markers.Add(PointToCustomMarker(plast, lnglat.Count - 1, t));
                points.Add(plast);
                //
                //if (points.Count < 50)//设置如果点数如果小于50则添加路线
                //{
                //    for (int j = 0; j < points.Count - 1; j++)
                //    {
                //        GMapRouteExt route = TwoPointToRoute(points[j], points[j + 1], j, c);
                //        route.IsVisible = false;
                //        fileOverlay.Routes.Add(route);
                //    }
                //}                
                gMapOverlays.Add(fileOverlay);
                this.gmap.Overlays.Add(fileOverlay);
                AddCustomAttribute(fileOverlay, false, false, c);
                if (isShowPerfectView)//如果设置最佳视图
                {
                    SetPerfectPos(points);
                }
                AddFileRow(fileOverlay.Id, false, true, false, true);
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
                XtraMessageBox.Show(ex.Message);
            }
        }
        

        private GMarkerGoogleExt PointToGoogleExt(PointLatLng point,int index,GMarkerGoogleType t, System.Drawing.Color c,bool isCircle)
        {
            GMarkerGoogleExt marker = new GMarkerGoogleExt(point, t, c, 5, isCircle);
            marker.ToolTipText = string.Format("点编号{0}:纬度{1},经度{2}", index, point.Lat, point.Lng);
            marker.ToolTip.Foreground = Brushes.Black;
            marker.ToolTip.TextPadding = new Size(20, 10);
            //
            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            return marker;
        }
        /// <summary>
        /// 添加并显示地图文件,注意点集的经纬度是纠偏前的经纬度
        /// </summary>
        /// <param name="points">轨道测点点集</param>
        /// <param name="filename">文件名称</param>
        /// <param name="isShowPerfectView">是否设置最佳视图</param>
        private void ShowFileLatLng(List<PointLatLng> points, string filename, bool isShowPerfectView)
        {
            try
            {
                GMapOverlay fileOverlay = new GMapOverlay();
                fileOverlay.Id = filename.Substring(filename.LastIndexOf("\\") + 1);
                int i = 0;
                //var t = RandomEnum<GMarkerGoogleType>();
                var t = GetRandomMarkerGoogleType();
                var c = GetRandomColor();
                for (int count = 0; count < points.Count - 1; count++)
                {
                    PointLatLng p = points[count];
                    int j = points.IndexOf(p) + 1;
                    i++;
                    //GMapMarker m = PointToMarker(p, j, t);
                    GMarkerGoogleExt m = PointToCustomMarker(p, j, t);
                    fileOverlay.Markers.Add(m);
                    double[] latlng = transform(p.Lat, p.Lng);
                    points.Add(GetTxtPoint(latlng));
                }
                //添加路径


                gMapOverlays.Add(fileOverlay);
                this.gmap.Overlays.Add(fileOverlay);
                AddCustomAttribute(fileOverlay, false, false, System.Drawing.Color.Black);
                if (isShowPerfectView)//如果设置最佳视图
                { SetPerfectPos(points); }
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
                XtraMessageBox.Show(ex.Message);
            }
        }

        private void ShowTopologyGIS(TopologyElement.Topology topo)
        {
            try
            {
                GMapOverlay topoOverLay = new GMapOverlay();
                List<PointLatLng> gpoints = new List<PointLatLng>();
                foreach (TopologyElement.Point p in topo.Points)
                {
                    PointLatLng pll = PointToMapPoint(p);
                    //AddMarkers(PointToMarker(p));//加入的是未纠偏的经纬度坐标(原始信息);
                    gpoints.Add(pll);
                    //topoOverLay.Markers.Add(PointToMarker(p));
                    topoOverLay.Markers.Add(PointToCustomMarker(p));//
                }
                var idmin = topo.TracksId.Min();
                var idmax = topo.TracksId.Max();
                for (int i = idmin; i <= idmax; i++)
                {
                    List<TopologyElement.Arc> arcs = topo.Arcs.FindAll(a => a.TrackID == i);
                    List<TopologyElement.Point> points = topo.Points.FindAll(b => b.TrackId == i);
                    for (int j = 0; j < points.Count - 1; j++)
                    {
                        var pieceid = topo.PiecesArcs.Find(c => c.StartId == points[j].ID);
                        //这个直接添加Arc
                        //GMapRoute route = ArcToRoute(pieceid, PointToMapPoint(points[j]), PointToMapPoint(points[j + 1]));
                        //自定义的路线
                        GMapRouteExt route= ArcToCustomRoute(pieceid, PointToMapPoint(points[j]), PointToMapPoint(points[j + 1]));
                        topoOverLay.Routes.Add(route);
                    }
                }
                //此处GMapOverLay将经纬度文件和拓扑分开
                topoOverLay.Id = topo.filename;
                gMapOverlays.Add(topoOverLay);
                this.gmap.Overlays.Add(topoOverLay);
                //添加属性框
                AddCustomAttribute(topoOverLay, true, false, System.Drawing.Color.BlueViolet);
                SetPerfectPos(true, true, topoOverLay);//设置最佳视图
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
                XtraMessageBox.Show(ex.Message);
            }
        }

        #region 经纬度点转换为标注
        private PointLatLng GetTxtPoint(double[] latlon)
        {
            double[] correctlatlng = transform(latlon[1], latlon[0]);
            PointLatLng point = new PointLatLng(correctlatlng[0], correctlatlng[1]);
            return point;
        }
        /// <summary>
        /// 经过纠偏
        /// </summary>
        /// <param name="point">标注纠偏之后的位置</param>
        /// <param name="index">点编号</param>
        /// <returns></returns>
        private GMapMarker PointToMarker(PointLatLng point, int index)
        {
            GMapMarker gMapMarker = new GMarkerGoogle(point, GMarkerGoogleType.lightblue_dot);
            gMapMarker.ToolTipText = string.Format("点编号{0}:纬度{1},经度{2}", index, point.Lat, point.Lng);
            gMapMarker.ToolTip.Foreground = Brushes.Black;
            gMapMarker.ToolTip.TextPadding = new Size(20, 10);
            return gMapMarker;
        }
        /// <summary>
        /// 根据点坐标，索引和类型转GMapMarker
        /// </summary>
        /// <param name="point"></param>
        /// <param name="index"></param>
        /// <param name="System.Drawing.ColorIndex">谷歌标注类型</param>
        /// <returns></returns>
        private GMapMarker PointToMarker(PointLatLng point, int index, GMarkerGoogleType t)
        {
            GMapMarker gMapMarker = new GMarkerGoogle(point, t);
            gMapMarker.ToolTipText = string.Format("点编号{0}:纬度{1},经度{2}", index, point.Lat, point.Lng);
            gMapMarker.ToolTip.Foreground = Brushes.Black;
            gMapMarker.ToolTip.TextPadding = new Size(20, 10);
            //
            gMapMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            return gMapMarker;
        }

        /// <summary>
        /// 根据点坐标，索引和类型转GMarkerGoogleExt
        /// </summary>
        /// <param name="point"></param>
        /// <param name="index"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private GMarkerGoogleExt PointToCustomMarker(PointLatLng point, int index, GMarkerGoogleType t)
        {
            GMarkerGoogleExt gMapMarker = new GMarkerGoogleExt(point, t);
            gMapMarker.ToolTipText = string.Format("点编号{0}:纬度{1},经度{2}", index, point.Lat, point.Lng);
            gMapMarker.ToolTip.Foreground = Brushes.Black;
            gMapMarker.ToolTip.TextPadding = new Size(20, 10);
            //
            gMapMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            return gMapMarker;
        }

        #endregion

        #endregion

        #region 保存地图文件
        /// <summary>
        /// 保存到原始文件
        /// </summary>
        /// <param name="overlayname"></param>
        private void SaveLatLng(string overlayname)
        {
            string filePath = FilePaths.Find(a => a.Contains(overlayname.Trim()));
            filePath = filePath.Substring(0,filePath.LastIndexOf(".")) + "-副本.txt";
            var overlay = gMapOverlays.Find(a => a.Id == overlayname);
            StreamWriter sw = new StreamWriter(filePath, false);
            string ss = "";
            foreach (GMarkerGoogleExt marker in overlay.Markers)
            {
                double[] correctlatlng = gcj02_To_Gps84(marker.Position.Lat, marker.Position.Lng);
                ss += correctlatlng[1] + "\t" +correctlatlng[0] + "\r\n";
            }
            sw.Write(ss);
            sw.Flush();
            sw.Close();
            filePath = string.Format("文件保存至{0}", filePath);
            XtraMessageBox.Show(filePath);
        }

        #endregion
        
        #region 导出KML文件

        private void btnSaveKML_ItemClick(object sender, ItemClickEventArgs e)
        {
            //Test();
            List<string> selectFiles = GetSelectFiles();
            if (selectFiles != null && selectFiles.Count != 0)
            {
                foreach (string s in selectFiles)
                {
                    SaveKml(s);
                }
            }
        }

        /// <summary>
        /// 保存kml文件
        /// </summary>
        /// <param name="overlayname"></param>
        private void SaveKml(string overlayname)
        {
            string filePath = FilePaths.Find(a => a.Contains(overlayname.Trim()));
            List<string[]> latlngs = new List<string[]>();
            ReadFormatTxt(filePath, ref latlngs);//读格式的文件返回经纬度
            filePath = filePath.Substring(0, filePath.LastIndexOf(".")) + ".kml";
            string s = "";
            foreach(string[] ss in latlngs)
            {
                s += string.Format("{0},{1},{2}", ss[0], ss[1], 0) + " ";
            }
            if (!string.IsNullOrEmpty(s))
            { SaveKml(filePath, s); }
        }

        private void SaveKml(string filepath,string xml)
        {
            //生成KML文件，注意大小写
            FileStream fs = new FileStream(filepath, FileMode.Create);
            XmlTextWriter w = new XmlTextWriter(fs, Encoding.UTF8);
            // 开始文档
            w.WriteStartDocument();
            w.WriteStartElement("kml", "http://www.opengis.net/kml/2.2");
            //开始一个元素
            w.WriteStartElement("Document");
            //添加子元素
            w.WriteElementString("name", "Paths");
            w.WriteElementString("description", "Examples of paths.");

            w.WriteStartElement("Style");
            //向先前创建的元素中添加一个属性
            w.WriteAttributeString("id", "yellowLineGreenPoly");

            w.WriteStartElement("LineStyle");
            w.WriteElementString("System.Drawing.Color", "7f00ffff");
            w.WriteElementString("width", "4");
            w.WriteEndElement();

            w.WriteStartElement("PolyStyle");
            w.WriteElementString("System.Drawing.Color", "7f00ffff");
            w.WriteEndElement();
            // 关闭style元素
            w.WriteEndElement();

            w.WriteStartElement("Placemark");
            w.WriteElementString("name", "Absolute Extruded");
            w.WriteElementString("description", "Transparent green wall with yellow outlines");
            w.WriteElementString("styleUrl", "#yellowLineGreenPoly");

            w.WriteStartElement("LineString");
            w.WriteElementString("extrude", "1");
            w.WriteElementString("tessellate", "1");
            w.WriteElementString("altitudeMode", "clampedToGround");

            w.WriteStartElement("coordinates");

            // 将路径坐标写在这里           
            w.WriteString(xml);

            // 关闭所有元素
            w.WriteEndDocument();

            // 关闭流
            w.Close();
        }

        #endregion


        #region 设置最佳视图

        /// <summary>
        /// 是否显示最佳视图
        /// </summary>
        /// <param name="isCurrentOverlay">是否设置当前图层</param>
        /// <param name="isSetPerfectPos">是否设置最佳视图</param>
        /// <param name="overlay">tuc</param>
        private void SetPerfectPos(bool isCurrentOverlay,bool isSetPerfectPos,GMapOverlay overlay)
        {
            if (isCurrentOverlay)
            {
                currentOverlay = overlay;
                txtCurrentOverlay.Caption = string.Format("当前图层:{0}", overlay.Id);
               
            }
            if (isSetPerfectPos)
            {
                SetPerfectPos(overlay.Markers);
            }
        }
       
        /// <summary>
        /// 设置最佳视图
        /// </summary>
        /// <param name="gpoints">所有点的范围</param>
        private void SetPerfectPos(List<PointLatLng> gpoints)
        {

            if (gpoints.Count != 0)
            {
                double minlat = gpoints.Min(a => a.Lat);
                double maxlat = gpoints.Max(a => a.Lat);
                double minlng = gpoints.Min(a => a.Lng);
                double maxlng = gpoints.Max(a => a.Lng);
                PointLatLng lefttop = new PointLatLng(minlat, minlng);
                PointLatLng center = new PointLatLng((minlat + maxlat) / 2.0, (minlng + maxlng) / 2.0);
                lefttop.Lat += maxlat - minlat;
                RectLatLng area = new RectLatLng();
                area.LocationTopLeft = lefttop;
                area.Size = new SizeLatLng(maxlat - minlat, maxlng - minlng);
                this.gmap.SelectedArea = area;
                this.gmap.SetZoomToFitRect(area);
            }
        }
        /// <summary>
        /// 设置最佳视图
        /// </summary>
        /// <param name="gMarkers">GMarkers</param>
        private void SetPerfectPos(GMap.NET.ObjectModel.ObservableCollectionThreadSafe<GMapMarker> gMarkers)
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (GMapMarker m in gMarkers)
            {
                points.Add(m.Position);
            }
            SetPerfectPos(points);
        }

        #endregion

      

        #region 添加或者设置PropCtrl中的内容

        /// <summary>
        /// 添加到全局变量的CustomAttributes
        /// </summary>
        /// <param name="overlay">图层</param>
        /// <param name="IsSetPropCtrl">是否添加到属性框里</param>
        /// <param name="isVisible">是否隐藏这一栏</param>
        private void AddCustomAttribute(GMapOverlay overlay,bool IsSetPropCtrl,bool isVisible,System.Drawing.Color color)
        {
            CustomAttribute attribute = new CustomAttribute(overlay.Id,overlay.Markers.Count,isVisible,((GMarkerGoogle)overlay.Markers[0]).Type,color);
            customAttributes.Add(attribute);
            currentAttribute = attribute;
            if(IsSetPropCtrl)
            {
                SetCurrentCustomAttribute(overlay.Id);
            }
        }

        /// <summary>
        /// 设置当前的自定义属性
        /// </summary>
        /// <param name="overlayname">当前图层的名称</param>
        private void SetCurrentCustomAttribute(string overlayname)
        {
            var currentCustom = customAttributes.Find(a => a.LineName == overlayname);
            this.propertyGridControl1.SelectedObject = currentCustom;
            currentAttribute = currentCustom;
            //
            DevExpress.XtraVerticalGrid.Rows.BaseRow br = propertyGridControl1.GetRowByCaption("线路");
            //通过循环遍历设置属性的中文名称       
            //foreach (DevExpress.XtraVerticalGrid.Rows.PGridEditorRow per in br.ChildRows)
            foreach (DevExpress.XtraVerticalGrid.Rows.EditorRow per in br.ChildRows)
            {
                if (per.ChildRows.Count > 0)
                {   //利用递归解决多层可扩展属性的caption的赋值  
                    SetCustomAttributeCaption(per);
                }
                string dicKey = per.Properties.FieldName;
                if (CustomAttribute.dic.ContainsKey(dicKey))
                    per.Properties.Caption = CustomAttribute.dic[dicKey];
                per.Height = 23;//设置属性行高度                          
            }
        }
        

        /// <summary>        
        /// 设置自定义属性的描述        
        /// </summary>        
        private void SetCustomAttributeCaption(DevExpress.XtraVerticalGrid.Rows.EditorRow EditorRow)
        {
            foreach (DevExpress.XtraVerticalGrid.Rows.EditorRow per_child in EditorRow.ChildRows)
            {
                if (per_child.ChildRows.Count > 0)
                {
                    //利用递归解决多层可扩展属性的caption的赋值  
                    SetCustomAttributeCaption(per_child);
                }
                //FieldName属性包含了该属性的父属性FieldName;通过 . 分割                
                string[] per_child_FieldName = per_child.Properties.FieldName.Split('.');
                string dicKey = per_child_FieldName[per_child_FieldName.GetLength(0) - 1];
                if (CustomAttribute.dic.ContainsKey(dicKey))
                    per_child.Properties.Caption = CustomAttribute.dic[dicKey];
                per_child.Height = 23;//设置属性行高度           
            }
        }

        #region 属性框事件
        //CheckButton.CheckeChange事件
        void checkBtnSort(object sender, EventArgs e)
        {
            CheckButton thisChk = (CheckButton)sender;
            if (thisChk == checkBtnPropertySort)
            {
                if (checkBtnPropertySort.Checked)
                    SetBarButtonDown(checkBtnAZSort, false);
                else
                    SetBarButtonDown(checkBtnAZSort, true);
            }
            else
            {
                if (checkBtnAZSort.Checked)
                    SetBarButtonDown(checkBtnPropertySort, false);
                else
                    SetBarButtonDown(checkBtnPropertySort, true);
            }
            UpdatePropertyGrid();
        }
        //设置按钮的鼠标悬浮气泡提示信息
        static void SetBarButtonToolTip(CheckButton chkBtn, string value)
        {
            DevExpress.Utils.SuperToolTip superToolTip = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipTitleItem toolTipTitleItem = new DevExpress.Utils.ToolTipTitleItem();
            toolTipTitleItem.Text = value;
            superToolTip.Items.Add(toolTipTitleItem);
            chkBtn.SuperTip = superToolTip;
        }
        //设置按钮是否按下
        void SetBarButtonDown(CheckButton chkBtn, bool value)
        {
            chkBtn.CheckedChanged -= new EventHandler(checkBtnSort);
            chkBtn.Checked = value;
            chkBtn.CheckedChanged += new EventHandler(checkBtnSort);
        }
        //更改控件排序方式
        void UpdatePropertyGrid()
        {
            this.propertyGridControl1.OptionsView.ShowRootCategories = this.checkBtnPropertySort.Checked;
        }
        #endregion
        
        //属性框cell的值改变
        private void propertyGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            try
            {
                var rowFieldName = e.Row.Name.ToString();
                switch (rowFieldName)
                {
                    case "LineStyle":
                        ChangeLineStyle((GMarkerGoogleType)e.Value);
                        break;
                    case "IsVisible":
                        currentAttribute.IsVisible = (bool)e.Value;
                        break;
                    case "LineSystem.Drawing.Color":
                        ChangeLineMarkerColor((System.Drawing.Color)e.Value);
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
                XtraMessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 更改线路标注或者颜色（xml文件和其他文件分开处理）

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        private void ChangeLineStyle(GMarkerGoogleType type)
        {
            var overlay = gMapOverlays.Find(a => a.Id == currentAttribute.LineName);
            currentOverlay = overlay;
            //改变customAttribute的属性
            var customattribute = customAttributes.Find(a => a.LineName == currentAttribute.LineName);
            customattribute.LineStyle = type;
            if(overlay.Id.Contains(".xml"))//
            {
                List<GMarkerGoogleExt> points = new List<GMarkerGoogleExt>();
                foreach(GMapMarker marker in overlay.Markers)
                {
                    GMarkerGoogleExt newmarker = UpdateMarker(marker, type);
                    points.Add(newmarker);
                }
                overlay.Markers.Clear();
                foreach(GMarkerGoogleExt a in points)
                {
                    overlay.Markers.Add(a);
                }
                this.gmap.Refresh();
            }
            else
            {
                GMarkerGoogleExt pfirst = UpdateMarker(overlay.Markers[0],type);
                GMarkerGoogleExt plast = UpdateMarker(overlay.Markers.Last(), type);
                overlay.Markers.RemoveAt(0);
                overlay.Markers.Insert(0,pfirst);
                overlay.Markers.RemoveAt(overlay.Markers.IndexOf(overlay.Markers.Last()));
                overlay.Markers.Add(plast);
                this.gmap.Refresh();
            }
        }

        /// <summary>
        /// 改变线路文件的起始点和终点的Marker样式
        /// </summary>
        /// <param name="c">颜色</param>
        private void ChangeLineMarkerColor(System.Drawing.Color c)
        {
            var overlay = gMapOverlays.Find(a => a.Id == currentAttribute.LineName);
            var customattribute = customAttributes.Find(a => a.LineName == currentAttribute.LineName);
            customattribute.LineColor = c;
            if (!(overlay.Id.Contains(".xml")))//不是xml文件
            {
                for(int i=1;i<overlay.Markers.Count-1;i++)
                {
                    GMarkerGoogleExt newmarker = UpdateMarker(overlay.Markers[i], c);
                    overlay.Markers.RemoveAt(i);
                    overlay.Markers.Insert(i,newmarker);
                }
                foreach(GMapRouteExt route in overlay.Routes)
                {
                    route.Stroke = new Pen(c, 3);
                }
                this.gmap.Refresh();
            }
            else
            {
                foreach (GMapRouteExt route in overlay.Routes)
                {
                    route.Stroke = new Pen(c, 3);
                }
                this.gmap.Refresh();
            }
        }
        
        /// <summary>
        /// 更新Marker的标注类型
        /// </summary>
        /// <param name="marker"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private GMarkerGoogleExt UpdateMarker(GMapMarker marker,GMarkerGoogleType type)
        {
            GMarkerGoogleExt newmarker = new GMarkerGoogleExt(marker.Position, type);
            newmarker.ToolTipText = marker.ToolTipText;
            newmarker.ToolTip.Foreground = Brushes.Black;
            newmarker.ToolTip.TextPadding = new Size(20, 10);
            return newmarker;
        }

        /// <summary>
        /// 更新Marker的颜色
        /// </summary>
        /// <param name="marker"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private GMarkerGoogleExt UpdateMarker(GMapMarker marker, System.Drawing.Color c)
        {
            GMarkerGoogleExt newmarker = new GMarkerGoogleExt(marker.Position, ((GMarkerGoogle)marker).Type, c,5, true);
            newmarker.ToolTipText = marker.ToolTipText;
            newmarker.ToolTip.Foreground = Brushes.Black;
            newmarker.ToolTip.TextPadding = new Size(20, 10);
            return newmarker;
        }

        /// <summary>
        /// 更新Marker的位置和ToolTipText
        /// </summary>
        /// <returns></returns>
        private GMarkerGoogleExt UpdateMarker(GMarkerGoogleExt marker,int index,PointLatLng point)
        {
            GMarkerGoogleExt newmarker = marker;
            newmarker.Position = point;
            newmarker.ToolTipText= string.Format("点编号{0}:经度{1},纬度{2}", index, point.Lng, point.Lat);
            newmarker.ToolTip.Foreground = Brushes.Black;
            newmarker.ToolTip.TextPadding = new Size(20, 10);
            return marker;
        }
        #endregion


        private void txtLatLng_ItemClick(object sender, ItemClickEventArgs e)
        {
        }
        private void txtLatLng_EditValueChanged(object sender, EventArgs e)
        {
            txtLatLng.AutoFillWidth = true;
            txtLatLng.Size = new Size(40, 20);
        }
        private void btnOverlay_ItemClick(object sender, ItemClickEventArgs e)
        {
            overlays = new OverlaysForm();
            if (currentOverlay != null)
            {
                overlays.CurrentOverlay = currentOverlay;
                overlays.OverLays.Add(currentOverlay);
            }
            overlays.ShowDialog(this);
        }

        #region 定位至当前图层、关闭且删除当前文件
        private void btnCloseCurrentFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            DoDelete();
            this.gmap.Refresh();
        }
        /// <summary>
        /// 关闭且删除当前文件
        /// </summary>
        public void DoDelete()
        {
            List<string> selectedFiles = GetSelectFiles();
            if (selectedFiles != null&&selectedFiles.Count!=0)
            {
                foreach (string s in selectedFiles)
                {
                    var obj = FilePaths.Find(a => a.Contains(s));
                    FilePaths.Remove(obj);//从保存文件路径的全局变量中删除文件
                    var map = gMapOverlays.FindAll(a => a.Id == s);
                    var attribute = customAttributes.Find(a => a.LineName == s.Trim());

                    customAttributes.Remove(attribute);//丛自定义属性中删除该文件的属性
                    foreach (GMapOverlay o in map)
                    {
                        o.Dispose();
                        this.gmap.Overlays.Remove(o);
                        gMapOverlays.Remove(o);
                    }
                }
            }
            this.gridView8.DeleteSelectedRows();
            gridView8.RefreshData();
            this.gridView8.OptionsBehavior.Editable = true;
        }

        /// <summary>
        /// 获取地图文件选择列的文件
        /// </summary>
        /// <returns></returns>
        private List<string> GetSelectFiles()
        {
            List<string> selectedFiles = new List<string>();
            for (int i = 0; i < gridView8.DataRowCount; i++)
            {
                var value = gridView8.GetDataRow(i)["IsChoose"].ToString().Trim();
                if (value == "True")//选择
                {
                    gridView8.SelectRow(i);
                    selectedFiles.Add(gridView8.GetDataRow(i)["Name"].ToString().Trim());
                }
                else if (value == "False")
                {
                    gridView8.UnselectRow(i);
                    continue;
                }
            }
            return selectedFiles;
        }

        /// <summary>
        /// 点击GridControl行定位至当前文件图层同时设置当前图层
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                string st = "";
                DevExpress.XtraGrid.Views.Base.ColumnView cv = (DevExpress.XtraGrid.Views.Base.ColumnView)gridControl1.FocusedView;//重新获取此ID 否则无法从表头连删获取不到id
                int focusedhandle = cv.FocusedRowHandle;
                object rowIdObj = gridView8.GetRowCellValue(focusedhandle, "Name");
                st = rowIdObj.ToString().Trim();
                this.tctrl_TrackGeoInfo.SelectedTabPage = pgGIS;
                //var currentmap = gMapOverlays.Find(a => a.Id == st);
                var currentmap = this.gmap.Overlays.First(a => a.Id == st);
                SetPerfectPos(true, true, currentmap);
                SetCurrentCustomAttribute(st);
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
                XtraMessageBox.Show(ex.Message);
            }
        }
        private void gridView8_RowCountChanged(object sender, EventArgs e)
        {
            if (gridView8.RowCount == 0)
            {
                this.gmap.Overlays.Clear();
            }
        }




        #endregion

        #region 适应全屏幕
        private void btn_ViewAll_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (gMapOverlays.Count != 0)//如果没有打开地图文件
                {
                    List<PointLatLng> points = new List<PointLatLng>();
                    foreach (GMapOverlay overlay in gMapOverlays)
                    {
                        if (overlay.Markers.Count != 0)
                        { points.AddRange(SearchCertainPoint(overlay)); }
                    }
                    SetPerfectPos(points);
                }
            }
            catch(Exception ex)
            {
                WriteLog(ex.Message);
                XtraMessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 找到图层中的左上和右下的地理点
        /// </summary>
        /// <returns></returns>
        private List<PointLatLng> SearchCertainPoint(GMapOverlay overlay)
        {
            GMap.NET.ObjectModel.ObservableCollectionThreadSafe<GMapMarker> markers = overlay.Markers;
            var minlat = markers.Min(a => a.Position.Lat);
            var maxlat = markers.Max(a => a.Position.Lat);
            var minlng = markers.Min(a => a.Position.Lng);
            var maxlng = markers.Max(a => a.Position.Lng);
            PointLatLng lefttop = new PointLatLng(maxlat, minlng);
            PointLatLng rightbottom = new PointLatLng(minlat, maxlng);
            return new List<PointLatLng>() { lefttop, rightbottom };
        }
        #endregion

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
            //gMapMarker.Tag可以用于设置其他的信息
            gMapMarker.ToolTipText = string.Format("点编号{0}:经度{1},纬度{2}", point.ID, point.Lon, point.Lat);
            gMapMarker.ToolTip.Foreground = Brushes.Black;
            gMapMarker.ToolTip.TextPadding = new Size(20, 10);
            return gMapMarker;
        }

        /// <summary>
        /// 拓扑点转GMap标志点
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public GMarkerGoogleExt PointToCustomMarker(TopologyElement.Point point)
        {
            GMarkerGoogleExt gMapMarker = new GMarkerGoogleExt(PointToMapPoint(point), GMarkerGoogleType.green);
            //gMapMarker.Tag可以用于设置其他的信息
            gMapMarker.ToolTipText = string.Format("点编号{0}:经度{1},纬度{2}", point.ID,point.Lon,point.Lat);
            gMapMarker.ToolTip.Foreground = Brushes.Black;
            gMapMarker.ToolTip.TextPadding = new Size(20, 10);
            gMapMarker.AddRect = false;
            return gMapMarker;
        }
        
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
            route.Stroke = new Pen(System.Drawing.Color.Green, 3);
            return route;
        }
        private GMapRoute TwoPointsToLine(TopologyElement.Point point1, TopologyElement.Point point2, string arcId)
        {
            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(PointToMapPoint(point1));
            points.Add(PointToMapPoint(point2));
            GMapRoute route = new GMapRoute(points, arcId);
            route.Stroke = new Pen(System.Drawing.Color.BlueViolet, 3);
            return route;
        }
        private GMapRoute ArcToRoute(TopologyElement.Arc arc,PointLatLng point1, PointLatLng point2)
        {
            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(point1);
            points.Add(point2);
            GMapRoute route = new GMapRoute(points, arc.Id.ToString());
            route.Stroke = new Pen(System.Drawing.Color.BlueViolet, 3);
            route.Tag = arc.Length;
            return route;
        }
        private GMapRouteExt ArcToCustomRoute(TopologyElement.Arc arc, PointLatLng point1, PointLatLng point2)
        {
            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(point1);
            points.Add(point2);
            GMapRouteExt route = new GMapRouteExt(points, arc.Id.ToString());
            route.Stroke = new Pen(System.Drawing.Color.BlueViolet, 3);
            route.Tag = arc.Length;
            route.Name = arc.Id.ToString();
            return route;
        }

        private GMapRouteExt TwoPointToRoute(PointLatLng point1,PointLatLng point2,int index,System.Drawing.Color c)
        {
            string name = string.Format("路线编号{0}", index);
            GMapRouteExt route = new GMapRouteExt(new PointLatLng[] { point1, point2 }, name);
            route.Stroke = new Pen(c, 3);
            route.Tag = route.Distance;
            return route;
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
                    map_Normal.Checked = true;
                    break;
                //谷歌卫星地图
                case 2:
                    rb_SatelliteMap.Checked = true;
                    break;
                //高德地图(暂时为空)
                case 3:
                    rb_OthersMap.Checked = true;
                    break;
                default:
                    break;
            }
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_SatelliteMap.Checked)
            {
                gmap.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                gmap.ReloadMap();
                GMapProvider.TimeoutMs = 1000;
                maptype = 2;
            }
        }
        private void rb_OthersMap_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_OthersMap.Checked)
            {
                gmap.MapProvider = GMapProviders.EmptyProvider;
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                gmap.ReloadMap();
                GMapProvider.TimeoutMs = 1000;
                maptype = 3;
            }
        }
        private void map_Normal_CheckedChanged(object sender, EventArgs e)
        {
            if (map_Normal.Checked)
            {
                gmap.MapProvider = GMapProviders.GoogleChinaMap;
                gmap.MapProvider = GMap.NET.MapProviders.GoogleChinaMapProvider.Instance; GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                gmap.ReloadMap();
                GMapProvider.TimeoutMs = 1000;
                maptype = 1;
            }
        }
        private PointLatLng startPos = PointLatLng.Empty;
        private PointLatLng endPos = PointLatLng.Empty;
        private void btnStartPoint_Click(object sender, EventArgs e)
        {
            if (clickMarker != null)
            {
                startPoint = clickMarker.Position;
            }
            if (startPos == PointLatLng.Empty)
            {
                startPos = clickPos;
            }
        }

        private void simpleButton4_Click(object sender,EventArgs e)
        {
            float bear = Convert.ToSingle(txtBearing.EditValue);
            this.gmap.Bearing += bear;
        }

        private void btnRotate_Click(object sender, EventArgs e)
        {
            PointLatLng centerP = GetCenterLatLng();
            PointLatLng targetP = GetTargetLatLng();
            if(centerP==PointLatLng.Empty||targetP==PointLatLng.Empty)
            { }
            else
            {
                string overlayname = currentOverlay.Id;
                object txtRotateAngle = this.txtRotateAngle.EditValue;
                Migration(centerP, targetP, overlayname,txtRotateAngle);
            }
        }

        private PointLatLng GetCenterLatLng()
        {
            PointLatLng p = new PointLatLng();
            if (txtCenterLatLng.EditValue != null)
            {
                string s = txtCenterLatLng.EditValue.ToString();
                string[] ss = s.Split(new char[] { ',', '\t', ' ','，' });
                p.Lng = Convert.ToDouble(ss[0]);
                p.Lat = Convert.ToDouble(ss[1]);
            }
            return p;
        }
    
        private PointLatLng GetTargetLatLng()
        {
            PointLatLng p = new PointLatLng();
            if (txtTargetLatLng.EditValue != null)
            {
                string s = txtTargetLatLng.EditValue.ToString();
                string[] ss = s.Split(new char[] { ',', '\t', ' ','，' });
                p.Lng = Convert.ToDouble(ss[0]);
                p.Lat = Convert.ToDouble(ss[1]);
            }
            return p;
        }
        /// <summary>
        /// 根据角度单位获得经纬度偏移
        /// </summary>
        /// <param name="centerP"></param>
        /// <param name="targetP"></param>
        /// <returns>返回二维数组0-经度，1-纬度</returns>
        private double[] GetDelta(PointLatLng centerP,PointLatLng targetP)
        {
            double[] delta = new double[2];
            delta[0] = targetP.Lng - centerP.Lng;
            delta[1] = targetP.Lat - centerP.Lat;
            return delta;
        }

        //private double[] GetDelta(PointLatLng centerP,PointLatLng targetP,double angle)
        //{
        //    double[] delta = new double[2];
        //    double radian = DegreesToRadians(angle);
        //    delta[0] = (targetP.Lng - centerP.Lng)*Math.Cos(radian)+ (targetP.Lat-centerP.Lat)* Math.Sin(radian) ;
        //    delta[1] = -(targetP.Lat - centerP.Lat) * Math.Sin(radian) + (targetP.Lat - centerP.Lat) * Math.Cos(radian) ;
        //    return delta;
        //}

        private double[] GetDelta(PointLatLng centerP,PointLatLng targetP,double degree)
        {
            double[] delta = new double[2];
            double radian = DegreesToRadians(degree);
            double distance = GetDistanceGoogle(centerP, targetP);
                //GetDistance(centerP, targetP);
            MyLatLng myLatLng = new MyLatLng(centerP.Lng, centerP.Lat);
            MyLatLng newlatlng = getMyLatLng2(myLatLng, distance, radian);
            delta[0] = newlatlng.m_Longitude - myLatLng.m_Longitude;
            delta[1] = newlatlng.m_Latitude - myLatLng.m_Latitude;
            return delta;
        }

        private void Migration(PointLatLng centerP,PointLatLng targetP,string overlayname,object txtRotateAngle)
        {
            double angle = Convert.ToDouble(txtRotateAngle);
            if (txtRotateAngle == null || angle == 0)
            {
                MigrationOverlay(centerP, targetP, overlayname);
            }
            else if(0<angle&& angle<360)
            {
                MigrationOverlay(centerP, targetP, overlayname);
                var overlay = gMapOverlays.Find(a => a.Id == overlayname);
                RotateOverlay(overlay.Markers[0].Position, overlayname, angle);
            }
        }

        private void MigrationOverlay(PointLatLng centerP,PointLatLng targetP,string overlayname)
        {
            var overlay = gMapOverlays.Find(a => a.Id == overlayname);
            double[] delta = GetDelta(centerP, targetP);
            List<GMarkerGoogleExt> newPoints = new List<GMarkerGoogleExt>();
            foreach (GMarkerGoogleExt marker in overlay.Markers)
            {
                int index = overlay.Markers.IndexOf(marker);
                PointLatLng newposition = new PointLatLng(marker.Position.Lat + delta[1], marker.Position.Lng + delta[0]);
                GMarkerGoogleExt marker2 = UpdateMarker(marker, index, newposition);
                newPoints.Add(marker2);
            }
            overlay.Markers.Clear();
            foreach (GMarkerGoogleExt m in newPoints)
            {
                overlay.Markers.Add(m);
            }
        }

        private void RotateOverlay(PointLatLng centerP,string overlayname,double angle)
        {
            var overlay = gMapOverlays.Find(a => a.Id == overlayname);
            List<GMarkerGoogleExt> newPoints = new List<GMarkerGoogleExt>();
            foreach (GMarkerGoogleExt marker in overlay.Markers)
            {
                double[] delta = GetDelta(centerP, marker.Position, angle);
                int index = overlay.Markers.IndexOf(marker);
                PointLatLng newposition = new PointLatLng(marker.Position.Lat + delta[1], marker.Position.Lng + delta[0]);
                GMarkerGoogleExt marker2 = UpdateMarker(marker, index, newposition);
                newPoints.Add(marker2);
            }
            overlay.Markers.Clear();
            foreach (GMarkerGoogleExt m in newPoints)
            {
                overlay.Markers.Add(m);
            }
        }

        private void SetTxtLatLng(PointLatLng p)
        {
            txtLatLng.EditValue = string.Format("{0},{1}", p.Lng, p.Lat);
        }
        private void btnEndPoint_Click(object sender, EventArgs e)
        {
            if (clickMarker != null)
            {
                endPoint = clickMarker.Position;
                txtLatLng.EditValue = string.Format("{0}{1}", clickMarker.Position.Lng, clickMarker.Position.Lat);
            }
             if (endPos == PointLatLng.Empty)
            {
                endPos = clickPos;
            }
        }
        private void btnCal_Click(object sender, EventArgs e)
        {
            if (startPoint == null)
            {
                XtraMessageBox.Show("缺少起始点");
            }
            if (endPoint == null)
            {
                XtraMessageBox.Show("缺少结束点");
            }
            else if(startPoint!=null&endPoint!=null)
            {
                if (rb_Dis.Checked)
                {
                    txtCalResult.EditValue = GetDistance(startPoint, endPoint);
                }
                if (rb_Heading.Checked)
                {
                    double[] a = GetUtmXY(startPoint.Lat, startPoint.Lng);
                    double[] b = GetUtmXY(endPoint.Lat, endPoint.Lng);
                    txtCalResult.EditValue = GetHeading2(a[0], a[1], b[0], b[1]);
                }
            }
            else
            {
                if (rb_Dis.Checked)
                {
                    txtCalResult.EditValue = GetDistance(startPos, endPos);
                }
                if (rb_Heading.Checked)
                {
                    double[] a = GetUtmXY(startPos.Lat, startPos.Lng);
                    double[] b = GetUtmXY(endPos.Lat, endPos.Lng);
                    txtCalResult.EditValue = GetHeading2(a[0], a[1], b[0], b[1]);
                }
                startPos = PointLatLng.Empty;
                endPos = PointLatLng.Empty;
            }
        }
        #endregion
        private void btnRadius_ItemClick(object sender, ItemClickEventArgs e)
        {
        }


        private void lblCenterLatLng_Click(object sender, EventArgs e)
        {
            txtCenterLatLng.EditValue = null;
        }

        private void lblTargetLatLng_Click(object sender, EventArgs e)
        {
            txtTargetLatLng.EditValue = null;
        }

        #region 查找周围点操作
        //度 转换成 弧度
        public static double DegreesToRadians(double degrees)
        {
            const double degToRadFactor = Math.PI / 180;
            return degrees * degToRadFactor;
        }
        //弧度 转换成 度
        public static double RadiansToDegrees(double radians)
        {
            const double radToDegFactor = 180 / Math.PI;
            return radians * radToDegFactor;
        }
        /**
        * 求B点经纬度
        * @param A 已知点的经纬度，
        * @param distance   AB两地的距离  单位km
        * @param angle  AB连线与正北方向的夹角（0~360）
        * @return  B点的经纬度
        */
        public static MyLatLng getMyLatLng(MyLatLng A, double distance, double angle)
        {
            double dx = distance * 1000 * Math.Sin(DegreesToRadians(angle));
            double dy = distance * 1000 * Math.Cos(DegreesToRadians(angle));
            double bjd = (dx / A.Ed + A.m_RadLo) * 180 / Math.PI;
            double bwd = (dy / A.Ec + A.m_RadLa) * 180 / Math.PI;
            return new MyLatLng(bjd, bwd);
        }
        /**
        * 求B点经纬度
        * @param A 已知点的经纬度，
        * @param distance   AB两地的距离  单位m
        * @param angle  AB连线与正北方向的夹角（0~360）
        * @return  B点的经纬度
        */
        public static MyLatLng getMyLatLng2(MyLatLng A, double distance, double angle)
        {
            double dx = distance * Math.Sin(DegreesToRadians(angle));
            double dy = distance * Math.Cos(DegreesToRadians(angle));
            double bjd = (dx / A.Ed + A.m_RadLo) * 180 / Math.PI;
            double bwd = (dy / A.Ec + A.m_RadLa) * 180 / Math.PI;
            return new MyLatLng(bjd, bwd);
        }
        //描述：以centerP为圆心，绘制半径为radius的圆
        //gMapControl：Gmap控制器		overlay：图层
        //centerP：圆心点	 radius：圆半径(单位:m)
        public static void DrawEllipse2(GMapControl gMapControl, GMapOverlay overlay, PointLatLng centerP, int radius)
        {
            try
            {
                if (radius <= 0)
                    return;
                List<PointLatLng> latLngs = new List<PointLatLng>();
                MyLatLng centerLatLng = new MyLatLng(centerP.Lng, centerP.Lat);
                // 0 - 360度 寻找半径为radius，圆心为centerP的圆上点的经纬度
                for (int i = 0; i < 360; i++)
                {
                    //获取目标经纬度,单位为m
                    MyLatLng tempLatLng = getMyLatLng2(centerLatLng, radius, i);
                    //将自定义的经纬度类 转换成 标准经纬度类
                    PointLatLng p = new PointLatLng(tempLatLng.m_Latitude, tempLatLng.m_Longitude);
                    //通过绘制标记点的方式绘制圆
                    GMapMarker gMapMarker = new GMarkerGoogle(p, GMarkerGoogleType.red);
                    overlay.Markers.Add(gMapMarker);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        //描述：以centerP为圆心，绘制半径为radius的圆
        //gMapControl：Gmap控制器		overlay：图层
        //centerP：圆心点	 radius：圆半径(单位: km)  name:多边形id
        public static void DrawEllipse2(GMapControl gMapControl, GMapOverlay overlay, PointLatLng centerP, int radius, string name)
        {
            try
            {
                if (radius <= 0)
                    return;
                List<PointLatLng> latLngs = new List<PointLatLng>();
                MyLatLng centerLatLng = new MyLatLng(centerP.Lng, centerP.Lat);
                // 0 - 360度 寻找半径为radius，圆心为centerP的圆上点的经纬度
                for (int i = 0; i < 360; i++)
                {
                    //获取目标经纬度
                    MyLatLng tempLatLng = getMyLatLng2(centerLatLng, radius, i);
                    //将自定义的经纬度类 转换成 标准经纬度类
                    PointLatLng p = new PointLatLng(tempLatLng.m_Latitude, tempLatLng.m_Longitude);
                    latLngs.Add(p);
                }
                //安全性检查
                if (latLngs.Count < 20)
                {
                    return;
                }
                //通过绘制多边形的方式绘制圆
                GMapPolygon gpol = new GMapPolygon(latLngs, name);
                gpol.Stroke = new Pen(System.Drawing.Color.Red, 1.0f);
                gpol.Fill = new SolidBrush(System.Drawing.Color.FromArgb(20, System.Drawing.Color.Red));
                gpol.IsHitTestVisible = true;
                overlay.Polygons.Add(gpol);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        //描述：以centerP为圆心，绘制半径为radius的圆
        //gMapControl：Gmap控制器		overlay：图层
        //centerP：圆心点	 radius：圆半径(单位: km)  name:多边形id
        public static void DrawEllipse2(GMapControl gMapControl, GMapOverlay overlay, PointLatLng centerP, double radius, string name)
        {
            try
            {
                if (radius <= 0)
                    return;
                List<PointLatLng> latLngs = new List<PointLatLng>();
                MyLatLng centerLatLng = new MyLatLng(centerP.Lng, centerP.Lat);
                // 0 - 360度 寻找半径为radius，圆心为centerP的圆上点的经纬度
                for (int i = 0; i < 360; i++)
                {
                    //获取目标经纬度
                    MyLatLng tempLatLng = getMyLatLng2(centerLatLng, radius, i);
                    //将自定义的经纬度类 转换成 标准经纬度类
                    PointLatLng p = new PointLatLng(tempLatLng.m_Latitude, tempLatLng.m_Longitude);
                    latLngs.Add(p);
                }
                //安全性检查
                if (latLngs.Count < 20)
                {
                    return;
                }
                //通过绘制多边形的方式绘制圆
                GMapPolygon gpol = new GMapPolygon(latLngs, name);
                gpol.Stroke = new Pen(System.Drawing.Color.Red, 1.0f);
                gpol.Fill = new SolidBrush(System.Drawing.Color.FromArgb(20, System.Drawing.Color.Red));
                gpol.IsHitTestVisible = true;
                overlay.Polygons.Add(gpol);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                currentOverlay.Polygons.Clear();
                searchResultCombox.Properties.Items.Clear();
                string s = txt_centerPos.Text.Trim();
                double radius = Convert.ToDouble(txt_Radius.EditValue);
                string[] centerlatlng = s.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                PointLatLng centerP = new PointLatLng(Convert.ToDouble(centerlatlng[1]), Convert.ToDouble(centerlatlng[0]));
                DrawEllipse2(this.gmap, currentOverlay, centerP, radius, "Circle");
                foreach (GMapOverlay o in this.gmap.Overlays)
                {
                    string oname = o.Id;
                    foreach (GMapMarker m in o.Markers)
                    {
                        bool flag = isInsidePolygon(m.Position, centerP, radius);
                        if (flag)
                        {
                            string ss = string.Format("{0},{1},{2},{3}",oname, m.ToolTipText.Substring(0, m.ToolTipText.LastIndexOf(':')), m.Position.Lng, m.Position.Lat);
                            searchResultCombox.Properties.Items.Add(ss);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
                WriteLog(ex.Message);
            }
        }
        private bool isInsidePolygon(PointLatLng p, PointLatLng centerP, double radius)
        {
            bool flag = false;
            double length = GetDistance(p, centerP);
            if (0 < length && length <= radius)
            {
                flag = true;
            }
            return flag;
        }
        private void btnClearResult_Click(object sender, EventArgs e)
        {
            txt_centerPos.EditValue = null;
            txt_Radius.Text = null;
            searchResultCombox.Properties.Items.Clear();
            searchResultCombox.Text = null;
            if(this.gmap.Overlays.Count!=0)
            {
                foreach(GMapOverlay o in this.gmap.Overlays)
                {
                    if(o.Polygons.Count!=0)
                    {
                        o.Polygons.Clear();
                    }
                }
            }
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
        //public static double pi = 3.1415926535897932384626;
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
        /// <summary>
        /// 纠偏GCJ02之后
        /// </summary>
        /// <param name="lat">WGS84下纬度</param>
        /// <param name="lon">WGS84下经度</param>
        /// <returns>double[0]=lat,double[1]=lng</returns>
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

        #region GMap:测距模式和地图编辑模式
        private void gmap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
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
                popupMenu1.ShowPopup(Control.MousePosition);
            }
        }
        int isSetPoint = 0;//设置起止点，1为起点，2为终点
        PointLatLng startPoint = PointLatLng.Empty;
        PointLatLng endPoint = PointLatLng.Empty;
        PointLatLng clickPos = PointLatLng.Empty;
        private void gmap_MouseClick(object sender, MouseEventArgs e)
        {
            clickPos = this.gmap.FromLocalToLatLng(e.X, e.Y);
            SetTxtLatLng(clickPos);
            if(clickMarker != null&&clickMarker.Position!=clickPos)
            {
                ((GMarkerGoogleExt)clickMarker).AddRect = false;
            }
            if (isMarkerEditable)//编辑模式下可以移动和添加标注
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
                    marker.IsVisible = true;
                    marker.ToolTipText = string.Format("经度{0},纬度:{1}", marker.Position.Lng, marker.Position.Lat);
                    mouseOverlay.Markers.Add(marker);
                }
            }
            if (isCalDistanceMode)//测距模式下不能移动和添加标注
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
                            txtStatic.Caption = "当前为起始点";
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
                            txtLatLng.EditValue = string.Format("{0},{1}", endPoint.Lat.ToString(), endPoint.Lng.ToString());
                            txtLatLng.AutoFillWidthInMenu = DevExpress.Utils.DefaultBoolean.True;
                            break;
                        default:
                            break;
                    }
                    if (startPoint != PointLatLng.Empty && endPoint != PointLatLng.Empty)
                    {
                        txtLength.Caption = string.Format("测距距离:{0}", GetDistance(startPoint, endPoint).ToString());
                    }
                    if(clickRoute!=null)
                    {
                        txtStatic.Caption = "当前为轨道片";
                        txtLatLng.EditValue = string.Format("GMap距离{0},自己算的距离{1}", clickRoute.Distance,clickRoute.Tag);
                        txtLatLng.AutoFillWidthInMenu = DevExpress.Utils.DefaultBoolean.True;
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
                    string txt = currentMarker.ToolTipText;
                    txt = string.Format("{0}:经度{1},纬度{2}", txt.Substring(0, txt.LastIndexOf(":")), p.Lng, p.Lat);
                    
                }
            }
            txtMousePos.Caption = string.Format("鼠标位置:{0},{1}", p.Lat, p.Lng);
        }
        private void gmap_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;//鼠标未按下
        }
        bool isCalDistanceMode = false;//是否是测距模式
        bool isMarkerEditable = false;//点是否可以编辑
        private void btnCalDis_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
        }
        //private GMapMarker clickMarker = null;
        private GMarkerGoogleExt clickMarker = null;
        private void gmap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            SetPerfectPos(true, false, item.Overlay);
            SetCurrentCustomAttribute(item.Overlay.Id);
            if (item is GMarkerGoogleExt)
            {
                clickMarker = item as GMarkerGoogleExt;
                clickMarker.AddRect = !clickMarker.AddRect;
                SetTxtLatLng(clickMarker.Position);
            }
            gmap.Refresh();
        }

        private void gmap_KeyDown(object sender, KeyEventArgs e)
        {
        }
        private void btnCalDisMode_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            isCalDistanceMode = btnCalDisMode.Checked;
        }
        private void btnEditModee_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            isMarkerEditable = btnEditModee.Checked;
        }
        private void btnEditModee_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(btnEditModee.Checked)
            {
                this.gridView8.OptionsBehavior.Editable = true;
                btnCalDisMode.Checked = false;
            }
        }
        private void btnCalDisMode_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (btnCalDisMode.Checked)
            {
                btnEditModee.Checked = false;
            }
        }

        private GMapRouteExt clickRoute = null;
        private void gmap_OnRouteClick(GMapRoute item, MouseEventArgs e)
        {
            if(item is GMapRouteExt)
            {
                clickRoute = item as GMapRouteExt;
                txtLength.Caption = string.Format("测距距离:{0}", item.DistanceGoogle.ToString());
            }
            
        }
        private void gmap_OnRouteEnter(GMapRoute item)
        {
        }
        #endregion

        #endregion

        #endregion

        #region 工具栏
        private void btnTransferDMS_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ToolForm toolform = new ToolForm();
            toolform.Show();
        }
        private void btnTransferCoords_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ToolForm toolform = new ToolForm();
            toolform.Show();
        }
        private void btnTxt_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
        }
        #endregion

        #region 帮助栏
        private void btnHelper_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Tip tipForm = new Tip();
            tipForm.Show();
        }
        private void btnHistory_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            HistoryForm historyForm = new HistoryForm();
            historyForm.Show();
        }
        #endregion

        #endregion

        #region 文件管理框显示

        /// <summary>
        /// 根据车站数量生成文件管理框图
        /// </summary>
        /// <param name="stationCount">车站数</param>
        public void InitFileTree(int stationCount)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID");
            dt.Columns.Add("ParentID");
            dt.Columns.Add("菜单");
            dt.Columns.Add("Tag");
            DataRow dr = dt.NewRow();
            int pId = 0;
            int Id = 1;
            dr["ID"] = Id++;//1
            dr["ParentID"] = pId;
            dr["菜单"] = "工程";
            dr["Tag"] = System.Windows.Forms.Application.StartupPath;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["ID"] = Id++;//2
            dr["ParentID"] = 1;
            dr["菜单"] = "地图总览";
            dr["Tag"] = null;
            dt.Rows.Add(dr);
            //每个车站
            for (int i = 0; i < stationCount; i++)
            {
                int a = Id;
                dr = dt.NewRow();
                dr["ID"] = Id++;//3
                dr["ParentID"] = 1;
                dr["菜单"] = string.Format("车站{0}", i + 1);
                dr["Tag"] = null;
                dt.Rows.Add(dr);
                for (int j = 0; j < 3; j++)//每个车站都包含车站总览、轨道地理信息、和站场图
                {
                    int pid = Id;
                    dr = dt.NewRow();
                    dr["ID"] = Id++;//4、5、6
                    dr["ParentID"] = pid - j - 1;
                    dr["菜单"] = choose(j);
                    dr["Tag"] = null;
                    dt.Rows.Add(dr);
                }
            }
            this.treeList1.OptionsBehavior.Editable = false;
            this.treeList1.DataSource = dt;
            this.treeList1.ExpandAll();
            this.treeList1.OptionsView.AutoWidth = false;
            this.treeList1.BestFitColumns();
        }

        /// <summary>
        /// 初始设置属性文件框的节点
        /// </summary>
        /// <param name="fileItem"></param>
        public void InitFileTree(FileItem fileItem)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID");
            dt.Columns.Add("ParentID");
            dt.Columns.Add("菜单");
            dt.Columns.Add("Tag");
            DataRow dr = dt.NewRow();
            int pId = 0;
            int Id = 1;
            dr["ID"] = Id++;//1
            dr["ParentID"] = pId;
            dr["菜单"] = "工程";
            dr["Tag"] = fileItem.FilePath;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr["ID"] = Id++;//2
            dr["ParentID"] = 1;
            dr["菜单"] = "地图总览";
            dr["Tag"] = null;
            dt.Rows.Add(dr);
            //每个车站
            for (int i = 0; i < fileItem.StationCount; i++)
            {
                int a = Id;
                dr = dt.NewRow();
                dr["ID"] = Id++;//3
                dr["ParentID"] = 1;
                dr["菜单"] = string.Format("车站{0}", i + 1);
                dr["Tag"] = null;
                dt.Rows.Add(dr);
                for (int j = 0; j < 3; j++)//每个车站都包含车站总览、轨道地理信息、和站场图
                {
                    int pid = Id;
                    dr = dt.NewRow();
                    dr["ID"] = Id++;//4、5、6
                    dr["ParentID"] = pid - j - 1;
                    dr["菜单"] = choose(j);
                    dr["Tag"] = null;
                    dt.Rows.Add(dr);
                }
            }
            this.treeList1.OptionsBehavior.Editable = false;
            this.treeList1.DataSource = dt;
            this.treeList1.ExpandAll();
            this.treeList1.OptionsView.AutoWidth = false;
            this.treeList1.BestFitColumns();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private string choose(int i)
        {
            string s = null;
            switch (i)
            {
                case 0:
                    s = "车站总览";
                    break;
                case 1:
                    s = "轨道地理信息";
                    break;
                case 2:
                    s = "站场图";
                    break;
                default:
                    break;
            }
            return s;
        }
        /// <summary>
        /// 设置节点图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_GetSelectImage(object sender, DevExpress.XtraTreeList.GetSelectImageEventArgs e)
        {
            if (e.Node == null) return;
            TreeListNode node = e.Node;
            int ID = 1;
            if (node.HasChildren)
            { ID = 0; }
            if (ID == 1)
                e.NodeImageIndex = 2;
            else
                e.NodeImageIndex = 0;
            if (e.Node.GetDisplayText("菜单") == "地图总览")
                e.NodeImageIndex = 1;
        }
        /// <summary>
        /// 判断节点数有几个
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
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
        private void ShowFileTree(string xmlFile)
        {
            try
            {
                FileInfo f = new FileInfo(xmlFile);
                FileItem item = new FileItem(f);
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFile);
                XmlNode node = doc.DocumentElement;
                item.StationCount = CertainNodeCount(node, "Station");
                if (item.StationCount == 0)
                {
                    XtraMessageBox.Show("没有找到车站数据！");
                }
                InitFileTree(item);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }
        public static string choosemenu = null;
        private void treeList1_DoubleClick(object sender, EventArgs e)
        {
            TreeList tree = sender as TreeList;
            TreeListNode clickNode = this.treeList1.FocusedNode;
            choosemenu = clickNode.GetDisplayText("菜单");
            ShowTabpage(choosemenu);
        }
        private void ShowTabpage(string menuname)
        {
            if (!string.IsNullOrEmpty(menuname))
            {
                switch (menuname)
                {
                    case "地图总览":
                        this.tctrl_TrackGeoInfo.SelectedTabPage = pgGIS;
                        break;
                    case "车站总览":
                        this.tctrl_TrackGeoInfo.SelectedTabPage = tg_Headinfo;
                        break;
                    case "轨道地理信息":
                        this.tctrl_TrackGeoInfo.SelectedTabPage =tp_TrackPieces ;//trackGeoInfo轨道地理信息
                        break;
                    case "站场图":
                        this.tctrl_TrackGeoInfo.SelectedTabPage = pgStation; ;//stationPic站场图
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region 轨道地理信息显示

        #region  读写数据库操作
        private void SaveXmlToSql()
        {
            //定义连接字符串
            string connString = "server=.;database=testDB;uid=sa;pwd=;";
            string sql = "";
            //建立连接对象
            using (SqlConnection sConn = new SqlConnection(connString))
            {
                using (SqlCommand sCmd = new SqlCommand(sql, sConn))
                {
                    //打开数据库连接
                    sConn.Open();
                }
            }
        }
        #endregion

        #region 读xml文件操作:记录错误信息、生成数据表、显示数据表
        /// <summary>
        /// 根据xml文件生成其下的文件版本,是否显示拓扑关系
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <param name="isShowTopo"></param>
        public void OpenFileTree(string file,bool isShowTopo)
        {
            string hint = "";
            List<DataTable> dts = new List<DataTable>();
            try
            {
                ShowFileTree(file);
                TrackFileProperty tf = GetTrackFileProperty(file);
                List<Track> tracks = GetFileTracks(file);
                if (IsCertainFile(file, "TrackFileProperty"))
                {
                    // dts.Add(GetTrackHeadInfoDt(file));
                    dts.Add(GetTrackHeadInfoDt(tf));
                    hint += "载入轨道地理信息头" + "\n";
                }
                if (IsCertainFile(file, "TrackInfo"))
                {
                    // dts.Add(GetTracksDt(file));
                    dts.Add(GetTracksDt(tracks));
                    hint += "载入轨道数据" + "\n";
                }
                if (IsCertainFile(file, "TrackGIS"))
                {
                    dts.Add(GetTrackGISDt(file));
                    hint += "载入轨道片数据" + "\n";
                }
                if (IsCertainFile(file, "SwitchData"))
                {
                    dts.Add(GetSwitchDataDt(file));
                    hint += "载入道岔数据" + "\n";
                }
                if (IsCertainFile(file, "BaliseData"))
                {
                    dts.Add(GetBaliseDataDt(file));
                    hint += "载入应答器数据" + "\n";
                }
                if (IsCertainFile(file, "StationEdgeData"))
                {
                    dts.Add(GetStationEdgeDataDt(file));
                    hint += "载入管辖边界数" + "\n";
                }
                if (IsCertainFile(file, "Station"))
                {
                    ShowStationProperty(GetStationProperties(file));
                    hint += "载入车站索引信息" + "\n";
                }
                else
                {
                    WriteLog("不存在车站数据头");
                }
                OFDSaveLastFilePath(file);//保存上一次打开目录
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            finally
            {
                XtraMessageBox.Show(hint);
                if (dts != null)
                { ShowDataTables(dts); }
                if (isShowTopo)
                {
                    if (IsCertainFile(file, "Station"))
                    {
                        TopologyElement.Topology FileTopo = new TopologyElement.Topology();
                        FileTopo.filename = file.Substring(file.LastIndexOf('\\') + 1);
                        CreateTopo(GetStation(file), ref FileTopo);//创建拓扑
                        if (FileTopo != null)
                        {
                            topologies.Add(FileTopo);//添加拓扑
                            ShowTopologyGIS(FileTopo);
                        }
                    }
                }
            }
        }
        private void ShowDataTables(List<DataTable> dts)
        {
            foreach (DataTable dt in dts)
            {
                if (dt.TableName == "轨道信息")
                {
                    if (dt.Rows.Count != 0)
                    {
                        this.tracks_gctrl.DataSource = dt;
                        this.tracks_gctrl.RefreshDataSource();
                        this.gridView3.BestFitColumns();
                    }
                    else
                    {
                        this.tracks_gctrl.DataSource = null;
                        this.tracks_gctrl.RefreshDataSource();
                        this.gridView3.BestFitColumns();
                    }
                }
                if (dt.TableName == "轨道片信息")
                {
                    if (dt.Rows.Count != 0)
                    {
                        this.trackPieces_gctrl.DataSource = dt;
                        this.trackPieces_gctrl.RefreshDataSource();
                        this.gridView1.BestFitColumns();
                    }
                    else
                    {
                        this.trackPieces_gctrl.DataSource = null;
                        this.trackPieces_gctrl.RefreshDataSource();
                        this.gridView1.BestFitColumns();
                    }
                }
                if (dt.TableName == "道岔信息")
                {
                    this.switchdata_gctrl.DataSource = dt;
                    this.switchdata_gctrl.RefreshDataSource();
                    this.gridView5.BestFitColumns();
                }
                if (dt.TableName == "应答器信息")
                {
                    this.balisedata_gctrl.DataSource = dt;
                    this.balisedata_gctrl.RefreshDataSource();
                    this.gridView7.BestFitColumns();
                }
                if (dt.TableName == "管辖边界信息")
                {
                    this.stationEdgeData_gctrl.DataSource = dt;
                    this.stationEdgeData_gctrl.RefreshDataSource();
                    this.gridView6.BestFitColumns();
                }
                if (dt.TableName == "轨道地理信息头")
                {
                    this.trackheadinfo_gctrl.DataSource = dt;
                    this.trackheadinfo_gctrl.RefreshDataSource();
                    this.gridView4.BestFitColumns();
                }
            }
        }
        private void ShowStationProperty(StationProperties s)
        {
            txt_StationNum.EditValue = s.StationNum;
            txt_StationName.EditValue = s.StationName;
            txt_UplinkStationCount.EditValue = s.UplinkStationCount;
            txt_UplinkStationNum.EditValue = s.UplinkStationNum;
            txt_DownlinkStationCount.EditValue = s.DownlinkStationCount;
            txt_DownlinkStationNum.EditValue = s.DownlinkStationNum;
            txt_TrackGISFileSize.EditValue = s.TrackGISFileSize;
            txt_TrackGISCRC.EditValue = s.TrackGISCRC;
            se_TrackGISVersion.EditValue = s.TrackGISVersion;
            //
            txt_miniLat.EditValue = s.MinimumLat;
            txt_miniLon.EditValue = s.MinimumLon;
            txt_maxLat.EditValue = s.MaximumLat;
            txt_maxLon.EditValue = s.MaximumLon;
            //
            txt_trainControlDataCRC.EditValue = s.TrainControlDataCRC;
            txt_trainControlDataSize.EditValue = s.TrainControlDataSize;
            se_trainControlDataVersion.EditValue = s.TrainControlDataversion;
        }

        #region XML文件反序列化
        /// <summary>
        /// 得到轨道地理信息中的文件属性
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public TrackFileProperty GetTrackFileProperty(string xmlFile)
        {
            using (FileStream fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xs = new XmlSerializer(typeof(TrackFileProperty));
                XmlSerializer x = new XmlSerializer(typeof(Station));
                Station s = new Station();
                TrackFileProperty tf = new TrackFileProperty();
                try
                {
                    if (IsCertainFile(xmlFile, "Station"))
                    {
                        s = (Station)x.Deserialize(fs);
                        tf = s.TrackInfo.TrackFileProperty;
                    }
                    else if (IsCertainFile(xmlFile, "TrackInfo"))
                    {
                        tf = (TrackFileProperty)xs.Deserialize(fs);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.InnerException.Message + " " + ex.Message);
                }
                return tf;
            }
        }
        private double maxLat;
        private double maxLon;
        private double minLat;
        private double minLon;
        /// <summary>
        /// 用于初次建立拓扑和添加错误
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public Station GetStation(string xmlFile)
        {
            using (FileStream fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Station));
                Station s = new Station();
                try
                {
                    IdError = null;
                    s = (Station)xs.Deserialize(fs);
                    //获取最大最小经度纬度，为以后渲染使用
                    maxLat = s.StationProperties.MaximumLat;
                    maxLon = s.StationProperties.MaximumLon;
                    minLat = s.StationProperties.MinimumLat;
                    minLon = s.StationProperties.MinimumLon;
                    ///记录错误
                    if (!string.IsNullOrEmpty(StationPropError(s.StationProperties)))
                    {
                        s.Error += StationPropError(s.StationProperties) + "\r\n";
                    }
                    if (!string.IsNullOrEmpty(TrackInfoError(s.TrackInfo)))
                    {
                        s.Error += TrackInfoError(s.TrackInfo) + "\r\n";
                    }
                    if (!string.IsNullOrEmpty(TrainControlDataError(s.TrainControlData)))
                    {
                        s.Error += TrainControlDataError(s.TrainControlData) + "\r\n";
                    }
                    if (!string.IsNullOrEmpty(s.Error))
                    {
                        IdError += s.Error;
                        WriteLog("编号错误：" + "\r\n" + IdError);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.InnerException.Message + " " + ex.Message);
                }
                return s;
            }
        }
        /// <summary>
        /// 得到车站属性
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public StationProperties GetStationProperties(string xmlFile)
        {
            using (FileStream fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Station));
                XmlSerializer x = new XmlSerializer(typeof(StationProperties));
                Station s = new Station();
                StationProperties sp = new StationProperties();
                try
                {
                    if (IsCertainFile(xmlFile, "Station"))
                    {
                        s = (Station)xs.Deserialize(fs);
                        sp = s.StationProperties;
                    }
                    else if (IsCertainFile(xmlFile, "StationProperties"))
                    {
                        sp = (StationProperties)x.Deserialize(fs);
                    }
                    else
                    {
                        WriteLog("文件不含有名称Station或StationProperties的节点,所以找不到站场信息头数据");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.InnerException.Message + " " + ex.Message);
                }
                return sp;
            }
        }
        /// <summary>
        /// 得到轨道地理信息中的轨道信息
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public List<Track> GetFileTracks(string xmlFile)
        {
            List<Track> tracks = new List<Track>();
            using (FileStream fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xs = new XmlSerializer(typeof(TrackInfo));
                XmlSerializer x = new XmlSerializer(typeof(Station));
                Station s = new Station();
                TrackInfo trackInfo = new TrackInfo();
                try
                {
                    if (IsCertainFile(xmlFile, "Station"))
                    {
                        s = (Station)x.Deserialize(fs);
                        tracks = s.TrackInfo.Tracks;
                    }
                    else if (IsCertainFile(xmlFile, "TrackInfo"))
                    {
                        trackInfo = (TrackInfo)xs.Deserialize(fs);
                        tracks = trackInfo.Tracks;
                    }
                    else
                    {
                        WriteLog("文件不含有名称Station或TrackInfo的节点，所以找不到轨道信息");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.InnerException.Message + " " + ex.Message);
                }
                return tracks;
            }
        }
        /// <summary>
        /// 得到工程文件中的轨道片信息
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public TrackGIS GetFileTrackGIS(string xmlFile)
        {
            string error = null;
            using (FileStream fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer x = new XmlSerializer(typeof(Station));
                XmlSerializer xs = new XmlSerializer(typeof(TrackInfo));
                Station station = new Station();
                TrackInfo trackInfo = new TrackInfo();
                TrackGIS trackGIS = new TrackGIS();
                try
                {
                    if (IsCertainFile(xmlFile, "Station"))
                    {
                        station = (Station)x.Deserialize(fs);
                        trackInfo = station.TrackInfo;
                        trackGIS = trackInfo.TrackGIS;
                    }
                    else if (IsCertainFile(xmlFile, "TrackInfo"))
                    {
                        trackInfo = (TrackInfo)xs.Deserialize(fs);
                        trackGIS = trackInfo.TrackGIS;
                    }
                    else
                    {
                        WriteLog("文件不含有名称Station或TrackInfo的节点，所以找不到轨道片信息");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.InnerException.Message + " " + ex.Message);
                }
                return trackGIS;
            }
        }
        /// <summary>
        /// 得到道岔数据表
        /// </summary>
        public List<Switch> GetSwitchData(string xmlFile)
        {
            string error = null;
            using (FileStream fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xs = new XmlSerializer(typeof(TrainControlData));
                XmlSerializer x = new XmlSerializer(typeof(Station));
                Station station = new Station();
                TrainControlData trainControlData = new TrainControlData();
                SwitchData switchData = new SwitchData();
                List<Switch> switches = new List<Switch>();
                try
                {
                    if (IsCertainFile(xmlFile, "Station"))
                    {
                        station = (Station)x.Deserialize(fs);
                        trainControlData = station.TrainControlData;
                        switchData = trainControlData.SwitchData;
                        switches = switchData.Switches;
                    }
                    else if (IsCertainFile(xmlFile, "TrainControlData"))
                    {
                        trainControlData = (TrainControlData)xs.Deserialize(fs);
                        switchData = trainControlData.SwitchData;
                        switches = switchData.Switches;
                    }
                    else
                    {
                        WriteLog("文件不含有名称Station或TrainControlData的节点，所以找不到道岔数据");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.InnerException.Message + " " + ex.Message);
                }
                return switches;
            }
        }
        public List<Balise> GetBaliseData(string xmlFile)
        {
            string error = null;
            using (FileStream fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer x = new XmlSerializer(typeof(Station));
                XmlSerializer xs = new XmlSerializer(typeof(TrainControlData));
                Station station = new Station();
                TrainControlData trainControlData = new TrainControlData();
                BaliseData baliseData = new BaliseData();
                List<Balise> balises = new List<Balise>();
                try
                {
                    if (IsCertainFile(xmlFile, "Station"))
                    {
                        station = (Station)x.Deserialize(fs);
                        trainControlData = station.TrainControlData;
                        baliseData = trainControlData.BaliseData;
                        balises = baliseData.Balises;
                    }
                    else if (IsCertainFile(xmlFile, "TrainControlData"))
                    {
                        trainControlData = (TrainControlData)xs.Deserialize(fs);
                        baliseData = trainControlData.BaliseData;
                        balises = baliseData.Balises;
                    }
                    else
                    {
                        WriteLog("文件不含有名称Station或TrainControlData的节点，所以找不到应答器数据");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.InnerException.Message + " " + ex.Message);
                }
                return balises;
            }
        }
        public List<StartEdge> GetStartEdgeData(string xmlFile)
        {
            string error = null;
            using (FileStream fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer x = new XmlSerializer(typeof(Station));
                XmlSerializer xs = new XmlSerializer(typeof(TrainControlData));
                Station station = new Station();
                TrainControlData trainControlData = new TrainControlData();
                StationEdgeData stationEdgeData = new StationEdgeData();
                List<StartEdge> startEdges = new List<StartEdge>();
                try
                {
                    if (IsCertainFile(xmlFile, "Station"))
                    {
                        station = (Station)x.Deserialize(fs);
                        trainControlData = station.TrainControlData;
                        stationEdgeData = trainControlData.StationEdgeData;
                        startEdges = stationEdgeData.StartEdges;
                    }
                    else if (IsCertainFile(xmlFile, "TrainControlData"))
                    {
                        trainControlData = (TrainControlData)xs.Deserialize(fs);
                        stationEdgeData = trainControlData.StationEdgeData;
                        startEdges = stationEdgeData.StartEdges;
                    }
                    else
                    {
                        WriteLog("文件不含有名称Station或TrainControlData的节点，所以找不到起始管辖数据");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.InnerException.Message + " " + ex.Message);
                }
                return startEdges;
            }
        }
        public List<EndEdge> GetEndEdgeData(string xmlFile)
        {
            string error = null;
            using (FileStream fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer x = new XmlSerializer(typeof(Station));
                XmlSerializer xs = new XmlSerializer(typeof(TrainControlData));
                Station station = new Station();
                TrainControlData trainControlData = new TrainControlData();
                StationEdgeData stationEdgeData = new StationEdgeData();
                List<EndEdge> endEdges = new List<EndEdge>();
                try
                {
                    if (IsCertainFile(xmlFile, "Station"))
                    {
                        station = (Station)x.Deserialize(fs);
                        trainControlData = station.TrainControlData;
                        stationEdgeData = trainControlData.StationEdgeData;
                        endEdges = stationEdgeData.EndEdges;
                    }
                    else if (IsCertainFile(xmlFile, "TrainControlData"))
                    {
                        trainControlData = (TrainControlData)xs.Deserialize(fs);
                        stationEdgeData = trainControlData.StationEdgeData;
                        endEdges = stationEdgeData.EndEdges;
                    }
                    else
                    {
                        WriteLog("文件不含有名称Station或TrainControlData的节点，所以找不到结束管辖数据");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.InnerException.Message + " " + ex.Message);
                }
                return endEdges;
            }
        }
        #region  记录xml序列化文件原始数据的错误
        private string StationPropError(StationProperties s)
        {
            if (!string.IsNullOrEmpty(s.Error))
            { return "==========StationProperties车站文件属性==========" + "\r\n" + s.Error; }
            else
            { return null; }
        }
        private string TrackInfoError(TrackInfo t)
        {
            string s = null;
            string a = TrackFilePropError(t.TrackFileProperty);
            if (!string.IsNullOrEmpty(a))
            {
                s += "\r\n" + "==========TrackFileProperties车站数据信息头==========" + "\r\n" + a;
            }
            string b = TracksProp(t.Tracks);
            if (!string.IsNullOrEmpty(b))
            {
                s += "\r\n" + "==========轨道属性数据==========" + "\r\n" + b;
            }
            string c = TrackGISError(t.TrackGIS);
            if (!string.IsNullOrEmpty(c))
            {
                s += "\r\n" + "==========TrackGIS轨道地理信息数据==========" + "\r\n" + c;
            }
            return s;
        }
        private string TrackFilePropError(TrackFileProperty tf)
        {
            if (!string.IsNullOrEmpty(tf.Error))
            {
                return "TrackFileProperty轨道文件属性有误：" + "\r\n" + tf.Error;
            }
            else
            { return null; }
        }
        private string TracksProp(List<Track> tracks)
        {
            string s = null;
            foreach (Track t in tracks)
            {
                if (!string.IsNullOrEmpty(t.Error))
                {
                    s += string.Format("轨道{0}的属性数据有误：", tracks.IndexOf(t) + 1) + "\r\n";
                    s += t.Error;
                }
            }
            return s;
        }
        private string TrackGISError(TrackGIS trackGIS)
        {
            string s = null;
            foreach (TrackPieceGIS t in trackGIS.TrackPieceGIS)
            {
                string b = null;
                int id = trackGIS.TrackPieceGIS.IndexOf(t) + 1;
                if (!string.IsNullOrEmpty(t.Error))
                {
                    b += string.Format("轨道{0}的地理属性有误：", trackGIS.TrackPieceGIS.IndexOf(t) + 1) + "\r\n";
                    b += t.Error;
                }
                foreach (TrackPiece tp in t.TrackPieces)
                {
                    if (!string.IsNullOrEmpty(tp.Error))
                    {
                        b += string.Format("轨道片{0}的属性数据有误：", t.TrackPieces.IndexOf(tp) + 1) + "\r\n";
                        b += tp.Error;
                    }
                }
                if (!string.IsNullOrEmpty(b))
                {
                    s += b;
                }
            }
            if (!string.IsNullOrEmpty(s))
            {
                return s;
            }
            else
            {
                return null;
            }
        }
        private string TrainControlDataError(TrainControlData tc)
        {
            string s = null;
            string a = TrainControlDataPropError(tc.TrainControlDataProperty);
            if (!string.IsNullOrEmpty(a))
            {
                s += "\r\n" + "=====TrainControlDataProperty固定应用数据=====" + "\r\n" + a;
            }
            string b = StationEdgeDataError(tc.StationEdgeData);
            if (!string.IsNullOrEmpty(b))
            {
                s += "\r\n" + "=====StationEdgeData管辖边界数据====" + "\r\n" + b;
            }
            string c = SwitchDataError(tc.SwitchData);
            if (!string.IsNullOrEmpty(c))
            {
                s += "\r\n" + "=====SwitchData道岔数据=====" + "\r\n" + c;
            }
            string d = BaliseDataError(tc.BaliseData);
            if (!string.IsNullOrEmpty(d))
            {
                s += "\r\n" + "=====BaliseData应答器数据=====" + "\r\n" + d;
            }
            return s;
        }
        private string TrainControlDataPropError(TrainControlDataProperty tp)
        {
            string s = null;
            string a = tp.Error;
            if (!string.IsNullOrEmpty(tp.Error))
            {
                s += "TrainControlDataProperty固定应用数据头有误：" + "\r\n" + tp.Error;
            }
            string b = StartEdgeInfoError(tp.StartEdgeInfo);
            if (!string.IsNullOrEmpty(b))
            {
                s += "StartEdge起始管辖数据有误：" + "\r\n" + b;
            }
            string c = EndEdgeInfoError(tp.EndEdgeInfo);
            if (!string.IsNullOrEmpty(c))
            {
                s += "EndEdge结束管辖数据有误：" + "\r\n" + c;
            }
            return s;
        }
        private string StartEdgeInfoError(List<StartEdgeInfo> s)
        {
            string a = null;
            foreach (StartEdgeInfo si in s)
            {
                if (!string.IsNullOrEmpty(si.Error))
                {
                    a += string.Format("起始边界{0}的属性数据有误：", s.IndexOf(si) + 1) + "\r\n";
                    a += si.Error;
                }
            }
            return a;
        }
        private string EndEdgeInfoError(List<EndEdgeInfo> e)
        {
            string a = null;
            foreach (EndEdgeInfo ei in e)
            {
                if (!string.IsNullOrEmpty(ei.Error))
                {
                    a += string.Format("结束边界{0}的属性数据有误：", e.IndexOf(ei) + 1) + "\r\n";
                    a += ei.Error;
                }
            }
            return a;
        }
        private string StationEdgeDataError(StationEdgeData s)
        {
            string error = null;
            foreach (StartEdge se in s.StartEdges)
            {
                if (!string.IsNullOrEmpty(se.Error))
                {
                    error += string.Format("起始管辖边界{0}的地理位置数据有误：", s.StartEdges.IndexOf(se) + 1) + "\r\n";
                    error += se.Error;
                }
            }
            foreach (EndEdge e in s.EndEdges)
            {
                if (!string.IsNullOrEmpty(e.Error))
                {
                    error += string.Format("结束管辖边界{0}的地理位置数据有误：", s.EndEdges.IndexOf(e) + 1) + "\r\n";
                    error += e.Error;
                }
            }
            return error;
        }
        private string SwitchDataError(SwitchData switchData)
        {
            string a = null;
            foreach (Switch s in switchData.Switches)
            {
                if (!string.IsNullOrEmpty(s.Error))
                {
                    a += string.Format("道岔{0}的属性数据有误：", switchData.Switches.IndexOf(s) + 1) + "\r\n";
                    a += s.Error;
                }
            }
            return a;
        }
        private string BaliseDataError(BaliseData baliseData)
        {
            string a = null;
            foreach (Balise b in baliseData.Balises)
            {
                if (!string.IsNullOrEmpty(b.Error))
                {
                    a += string.Format("应答器{0}的属性数据有误：", baliseData.Balises.IndexOf(b) + 1) + "\r\n";
                    a += b.Error;
                }
            }
            return a;
        }
        #endregion
        #endregion
        #region  数据表
        public DataTable GetTrackHeadInfoDt(TrackFileProperty tf)
        {
            DataTable dt = new DataTable("轨道地理信息头");
            dt.Columns.Add("Length");
            dt.Columns.Add("Value");
            dt.Columns.Add("Tag");
            //TrackFileProperty tf = new TrackFileProperty();
            try
            {
                //tf = GetTrackFileProperty(xmlFile);
                //
                DataRow dr = dt.NewRow();
                dr["Length"] = GetLength(tf.TrackFileType);
                dr["Value"] = tf.TrackFileType;
                dr["Tag"] = "文件类型";
                dt.Rows.Add(dr);
                //
                dr = dt.NewRow();
                dr["Length"] = GetLength(tf.TrackFileStructVersion);
                dr["Value"] = tf.TrackFileStructVersion;
                dr["Tag"] = "文件结构版本";
                dt.Rows.Add(dr);
                //
                dr = dt.NewRow();
                dr["Length"] = GetLength(tf.TrackFileDataVersion);
                dr["Value"] = tf.TrackFileDataVersion;
                dr["Tag"] = "文件数据版本";
                dt.Rows.Add(dr);
                //
                dr = dt.NewRow();
                dr["Length"] = GetLength(tf.StationNum);
                dr["Value"] = tf.StationNum;
                dr["Tag"] = "车站编号";
                dt.Rows.Add(dr);
                //
                dr = dt.NewRow();
                dr["Length"] = GetLength(tf.TrackCount);
                dr["Value"] = tf.TrackCount;
                dr["Tag"] = "轨道个数";
                dt.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            return dt;
        }
        public DataTable GetTrackHeadInfoDt(string xmlFile)
        {
            DataTable dt = new DataTable("轨道地理信息头");
            dt.Columns.Add("Length");
            dt.Columns.Add("Value");
            dt.Columns.Add("Tag");
            TrackFileProperty tf = new TrackFileProperty();
            try
            {
                tf = GetTrackFileProperty(xmlFile);
                //
                DataRow dr = dt.NewRow();
                dr["Length"] = GetLength(tf.TrackFileType);
                dr["Value"] = tf.TrackFileType;
                dr["Tag"] = "文件类型";
                dt.Rows.Add(dr);
                //
                dr = dt.NewRow();
                dr["Length"] = GetLength(tf.TrackFileStructVersion);
                dr["Value"] = tf.TrackFileStructVersion;
                dr["Tag"] = "文件结构版本";
                dt.Rows.Add(dr);
                //
                dr = dt.NewRow();
                dr["Length"] = GetLength(tf.TrackFileDataVersion);
                dr["Value"] = tf.TrackFileDataVersion;
                dr["Tag"] = "文件数据版本";
                dt.Rows.Add(dr);
                //
                dr = dt.NewRow();
                dr["Length"] = GetLength(tf.StationNum);
                dr["Value"] = tf.StationNum;
                dr["Tag"] = "车站编号";
                dt.Rows.Add(dr);
                //
                dr = dt.NewRow();
                dr["Length"] = GetLength(tf.TrackCount);
                dr["Value"] = tf.TrackCount;
                dr["Tag"] = "轨道个数";
                dt.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(tf.StationNum);
                WriteLog(ex.Message);
            }
            return dt;
        }
        /// <summary>
        /// 得到对象的数据类型的字节大小
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private object GetLength(object a)
        {
            return System.Runtime.InteropServices.Marshal.SizeOf(a);
        }
        /// <summary>
        /// 得到轨道信息的DataTable
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public DataTable GetTracksDt(List<Track> tracks)
        {
            DataTable dt = new DataTable("轨道信息");
            dt.Columns.Add("TrackId");
            dt.Columns.Add("TrackType");
            dt.Columns.Add("StartMileage");
            dt.Columns.Add("StartLat");
            dt.Columns.Add("StartLon");
            dt.Columns.Add("EndMileage");
            dt.Columns.Add("EndLat");
            dt.Columns.Add("EndLon");
            dt.Columns.Add("StartAddress");
            dt.Columns.Add("EndAddress");
            //List<Track> tracks = new List<Track>();
            try
            {
                //tracks = GetFileTracks(xmlFile);
                foreach (Track p in tracks)
                {
                    if ((tracks.IndexOf(p) + 1) != p.TrackID)
                    {
                        IdError += string.Format("TrackInfo中轨道{0}的编号不正确", (tracks.IndexOf(p) + 1)) + "\r\n";
                    }
                    DataRow dr = dt.NewRow();
                    dr["TrackId"] = p.TrackID;
                    dr["TrackType"] = GetEnumNameByKey<Tracktype>(p.TrackType);
                    dr["StartMileage"] = p.StartMileage;
                    dr["StartLat"] = p.StartLat;
                    dr["StartLon"] = p.StartLon;
                    dr["EndMileage"] = p.EndMileage;
                    dr["EndLat"] = p.EndLat;
                    dr["EndLon"] = p.EndLon;
                    dr["StartAddress"] = p.StartAddress;
                    dr["EndAddress"] = p.EndAddress;
                    dt.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            return dt;
        }
        /// <summary>
        /// 得到轨道信息的DataTable
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public DataTable GetTracksDt(string xmlFile)
        {
            DataTable dt = new DataTable("轨道信息");
            dt.Columns.Add("TrackId");
            dt.Columns.Add("TrackType");
            dt.Columns.Add("StartMileage");
            dt.Columns.Add("StartLat");
            dt.Columns.Add("StartLon");
            dt.Columns.Add("EndMileage");
            dt.Columns.Add("EndLat");
            dt.Columns.Add("EndLon");
            dt.Columns.Add("StartAddress");
            dt.Columns.Add("EndAddress");
            List<Track> tracks = new List<Track>();
            try
            {
                tracks = GetFileTracks(xmlFile);
                foreach (Track p in tracks)
                {
                    if ((tracks.IndexOf(p) + 1) != p.TrackID)
                    {
                        IdError += string.Format("TrackInfo中轨道{0}的编号不正确", (tracks.IndexOf(p) + 1)) + "\r\n";
                    }
                    DataRow dr = dt.NewRow();
                    dr["TrackId"] = p.TrackID;
                    dr["TrackType"] = GetEnumNameByKey<Tracktype>(p.TrackType);
                    dr["StartMileage"] = p.StartMileage;
                    dr["StartLat"] = p.StartLat;
                    dr["StartLon"] = p.StartLon;
                    dr["EndMileage"] = p.EndMileage;
                    dr["EndLat"] = p.EndLat;
                    dr["EndLon"] = p.EndLon;
                    dr["StartAddress"] = p.StartAddress;
                    dr["EndAddress"] = p.EndAddress;
                    dt.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            return dt;
        }
        /// <summary>
        /// 得到轨道片记录点数据DataTable
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public DataTable GetTrackGISDt(string xmlFile)
        {
            DataTable dt = new DataTable("轨道片信息");
            dt.Columns.Add("TrackID");
            dt.Columns.Add("TrackPieceID");
            dt.Columns.Add("DeltaPos");
            dt.Columns.Add("DeltaLat");
            dt.Columns.Add("DeltaLon");
            dt.Columns.Add("DeltaHeading");
            dt.Columns.Add("AdjacentTrack");
            List<TrackPieceGIS> trackPieceGIS = GetFileTrackGIS(xmlFile).TrackPieceGIS;
            try
            {
                foreach (TrackPieceGIS tp in trackPieceGIS)
                {
                    List<TrackPiece> ps = tp.TrackPieces;
                    if ((trackPieceGIS.IndexOf(tp) + 1) != tp.TrackID)
                    {
                        IdError += string.Format("TrackGIS中的轨道地理信息{0}编号不正确", tp.TrackID) + "\r\n";
                    }
                    for (int j = 0; j < ps.Count; j++)
                    {
                        DataRow dr = dt.NewRow();
                        int pieceID = j + 1;
                        dr["TrackID"] = tp.TrackID;
                        dr["TrackPieceID"] = pieceID;
                        dr["DeltaPos"] = ps[j].DeltaPos;
                        dr["DeltaLat"] = ps[j].DeltaLat;
                        dr["DeltaLon"] = ps[j].DeltaLon;
                        dr["DeltaHeading"] = ps[j].DeltaHeading;
                        dr["AdjacentTrack"] = ps[j].AdjacentTrack;
                        dt.Rows.Add(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
            return dt;
        }
        /// <summary>
        /// 得到道岔数据表
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public DataTable GetSwitchDataDt(string xmlFile)
        {
            DataTable dt = new DataTable("道岔信息");
            dt.Columns.Add("Type");
            dt.Columns.Add("SwitchNum");
            dt.Columns.Add("SwitchDirection");
            dt.Columns.Add("SwitchPOI");
            dt.Columns.Add("SwitchPos");
            dt.Columns.Add("SwitchReversePos");
            dt.Columns.Add("LastOffset");
            dt.Columns.Add("SwitchPosOffset");
            dt.Columns.Add("NextOffset");
            dt.Columns.Add("PreSwitchTrackNum");
            dt.Columns.Add("SwitchPosTrackNum");
            dt.Columns.Add("SwitchReverseTrackNum");
            List<Switch> switches = new List<Switch>();
            try
            {
                switches = GetSwitchData(xmlFile);
                for (int i = 0; i < switches.Count; i++)
                {
                    Switch s = switches[i];
                    int pid = i + 1;
                    DataRow dr = dt.NewRow();
                    dr["Type"] = s.Type;
                    dr["SwitchNum"] = s.SwitchNum;
                    dr["SwitchDirection"] = GetEnumNameByKey<Direction>(s.SwitchDirection);
                    dr["SwitchPOI"] = s.SwitchPOI;
                    dr["SwitchPos"] = s.SwitchPos;
                    dr["SwitchReversePos"] = s.SwitchReversePos;
                    dr["LastOffset"] = s.LastOffset;
                    dr["SwitchPosOffset"] = s.SwitchPosOffset;
                    dr["NextOffset"] = s.NextOffset;
                    dr["PreSwitchTrackNum"] = s.PreSwitchTrackNum;
                    dr["SwitchPosTrackNum"] = s.SwitchPosTrackNum;
                    dr["SwitchReverseTrackNum"] = s.SwitchReverseTrackNum;
                    dt.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
            return dt;
        }
        /// <summary>
        /// 得到应答器数据表
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public DataTable GetBaliseDataDt(string file)
        {
            DataTable dt = new DataTable("应答器信息");
            dt.Columns.Add("Type");
            dt.Columns.Add("LastOffset");
            dt.Columns.Add("NextOffset");
            dt.Columns.Add("BaliseNum");
            dt.Columns.Add("DataSize");
            dt.Columns.Add("BaliseProperty");
            dt.Columns.Add("BalisePos");
            dt.Columns.Add("BaliseTrackNum");
            dt.Columns.Add("BaliseLoc");
            dt.Columns.Add("BaliseDataPort");
            List<Balise> balises = GetBaliseData(file);
            for (int i = 0; i < balises.Count; i++)
            {
                Balise b = balises[i];
                int pid = i + 1;
                DataRow dr = dt.NewRow();
                dr["Type"] = b.Type;
                dr["LastOffset"] = b.LastOffset;
                dr["NextOffset"] = b.NextOffset;
                dr["BaliseNum"] = b.BaliseNum;
                dr["DataSize"] = b.DataSize;
                dr["BaliseProperty"] = b.BaliseProperty;
                dr["BalisePos"] = b.BaliseProperty;
                dr["BaliseTrackNum"] = b.BaliseTrackNum;
                dr["BaliseLoc"] = b.BaliseLoc;
                dr["BaliseDataPort"] = b.BaliseDataPort;
                dt.Rows.Add(dr);
            }
            return dt;
        }
        public DataTable GetStationEdgeDataDt(string file)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("StartEdge", "起始管辖边界");
            dic.Add("EndEdge", "结束管辖边界");
            DataTable dt = new DataTable("管辖边界信息");
            dt.Columns.Add("NameType");
            dt.Columns.Add("Type");
            dt.Columns.Add("AdjacentDataOffset");
            dt.Columns.Add("AdjacentTsrsNum");
            dt.Columns.Add("AdjacentStationNum");
            dt.Columns.Add("AdjacentEdgeType");
            dt.Columns.Add("AdjacentTrackNum");
            dt.Columns.Add("Position");
            dt.Columns.Add("EdgeTrackNum");
            List<StartEdge> startEdges = GetStartEdgeData(file);
            List<EndEdge> endEdges = GetEndEdgeData(file);
            for (int i = 0; i < startEdges.Count; i++)
            {
                StartEdge s = startEdges[i];
                string name = null;
                dic.TryGetValue(s.GetType().Name, out name);
                DataRow dr = dt.NewRow();
                dr["NameType"] = name;
                dr["Type"] = GetEnumNameByKey<StartEdgeType>(s.Type);
                dr["AdjacentDataOffset"] = s.AdjacentDataOffset;
                dr["AdjacentTsrsNum"] = s.AdjacentTsrsNum;
                dr["AdjacentStationNum"] = s.AdjacentStationNum;
                dr["AdjacentEdgeType"] = GetEnumNameByKey<AdjacentEdgeType>(s.AdjacentEdgeType);
                dr["AdjacentTrackNum"] = s.AdjacentTrackNum;
                dr["Position"] = s.Position;
                dr["EdgeTrackNum"] = s.EdgeTrackNum;
                dt.Rows.Add(dr);
            }
            for (int i = 0; i < endEdges.Count; i++)
            {
                EndEdge e = endEdges[i];
                //DataRow dr = dt.NewRow();
                //int pid = i + 1;
                DataRow dr = dt.NewRow();
                string name = null;
                dic.TryGetValue(e.GetType().Name, out name);
                dr["NameType"] = name;
                dr["Type"] = GetEnumNameByKey<EndEdgeType>(e.Type);
                dr["AdjacentDataOffset"] = e.AdjacentDataOffset;
                dr["AdjacentTsrsNum"] = e.AdjacentTsrsNum;
                dr["AdjacentStationNum"] = e.AdjacentStationNum;
                dr["AdjacentEdgeType"] = GetEnumNameByKey<AdjacentEdgeType>(e.AdjacentEdgeType);
                dr["AdjacentTrackNum"] = e.AdjacentTrackNum;
                dr["Position"] = e.Position;
                dr["EdgeTrackNum"] = e.EdgeTrackNum;
                dt.Rows.Add(dr);
            }
            return dt;
        }
        #endregion
        #region   序列化函数
        /// <summary>
        /// 反序列化得到目标XmlElement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public static T ToXmlTarget<T>(string xmlFile)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var xmlStream = new FileStream(xmlFile, FileMode.Open))
            {
                StreamReader sr = new StreamReader(xmlFile);
                XmlTextReader xmlReader = new XmlTextReader(sr);
                xmlReader.Namespaces = false;
                var target = (T)xmlSerializer.Deserialize(xmlReader);
                xmlReader.Close();
                return target;
            }
        }
        /// <summary>
        /// XML反序列化，注意反序列化xml文件必须没有注释
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="XmlString"></param>
        /// <returns></returns>
        public static T XmlDeserialize<T>(string xml, string xmlRootName = "TrackInfo")
        {
            T result = default(T);
            using (StringReader sr = new StringReader(xml))
            {
                XmlSerializer xmlSerializer = string.IsNullOrWhiteSpace(xmlRootName) ?
                    new XmlSerializer(typeof(T)) : new XmlSerializer(typeof(T), new XmlRootAttribute(xmlRootName));
                result = (T)xmlSerializer.Deserialize(sr);
            }
            return result;
        }
        /// <summary>
        /// 反序列化得到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T DeserializerFromXml<T>(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new ArgumentException(filePath + "not Exists");
                using (StreamReader reader = new StreamReader(filePath))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    T ret = (T)xs.Deserialize(reader);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
        /// <summary>
        /// 序列化生成xml文件(eg:如果想要生成轨道地理信息文件，则使用TrackInfo trackInfo)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetO"></param>
        /// <param name="xmlFile"></param>
        public static void ToXmlString<T>(T targetO, string xmlFile)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                FileStream fs = new FileStream(xmlFile, FileMode.Create);
                var xmlWriteSettings = new XmlWriterSettings();
                xmlWriteSettings.Indent = true;
                xmlWriteSettings.IndentChars = "    ";
                xmlWriteSettings.NewLineChars = "\r\n";
                xmlWriteSettings.Encoding = Encoding.UTF8;
                xmlWriteSettings.OmitXmlDeclaration = false;
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter b = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (XmlWriter xmlWriter = XmlWriter.Create(fs, xmlWriteSettings))
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    xmlSerializer.Serialize(xmlWriter, targetO, namespaces);//TrackInfo
                    b.Serialize(fs, targetO);
                    xmlWriter.Close();
                }
                fs.Close();
                XtraMessageBox.Show("序列化成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                XtraMessageBox.Show(ex.Message);
            }
        }
        #endregion
        #endregion

        #region 写xml文件操作，写二进制文件
        /// <summary>
        /// 保存xml文件
        /// </summary>
        private void SaveXml()
        {
        }
        /// <summary>
        /// Xml文件转二进制文件
        /// </summary>
        private void XmlToBin()
        {
        }
        #region  生成xml文件
        /// <summary>
        /// 根据GridView2返回轨道地理信息
        /// </summary>
        /// <returns></returns>
        private List<Track> SerializeTracks()
        {
            int rows = this.gridView2.RowCount;
            List<Track> tracks = new List<Track>();
            for(int i=0;i<rows;i++)
            {
                DataRow dr = this.gridView2.GetDataRow(i);
                byte trackId = Convert.ToByte(dr["TrackId"]);
                //int type = (int)Enum.Parse(typeof(Tracktype), dr["Tracktype"].ToString());
                int tracktype = (int)Enum.Parse(typeof(Tracktype), dr["Tracktype"].ToString());
                uint startMileage = Convert.ToUInt32(dr["StartMileage"]);
                int startlat = Convert.ToInt32(dr["StartLat"]);
                int startlon = Convert.ToInt32(dr["StartLon"]);
                uint endMileage = Convert.ToUInt32(dr["EndMileage"]);
                int endlat = Convert.ToInt32(dr["EndLat"]);
                int endlon = Convert.ToInt32(dr["EndLon"]);
                ushort startaddress = Convert.ToUInt16(dr["StartAddress"]);
                ushort endaddress = Convert.ToUInt16(dr["EndAddress"]);
                Track t = new Track(trackId, tracktype, startMileage, startlat, startlon, endMileage, endlat, endlon, startaddress, endaddress);
                tracks.Add(t);
                Serialize(t);
                //Console.WriteLine(t.EndLat);
            }
            return tracks;
        }
        /// <summary>
        /// 根据GridView4返回TrackGIS
        /// </summary>
        /// <returns></returns>
        private TrackGIS SerializeTrackGIS()
        {
            TrackGIS trackgis = new TrackGIS();
            List<TrackPieceGIS> trackPieceGIS = new List<TrackPieceGIS>();
            int rows = this.gridView4.RowCount;
            List<DataRow> Drs = new List<DataRow>();
            for(int i=0;i<rows;i++)// 把所有数据表的行加进来
            {
                Drs.Add(this.gridView4.GetDataRow(i));
            }
            //找到所有TrackID
            var trackidindex = Drs.Select(a => Convert.ToInt32(a["TrackID"])).ToList();
            int min = trackidindex.Min();
            int max = trackidindex.Max();
            for(int j=min;j<=max;j++)
            {
                var datarows = Drs.FindAll(a => Convert.ToInt32(a["TrackID"]) == j).ToList();
                trackPieceGIS.Add(SerializeTrackPieceGIS(datarows));
            }
            trackgis.TrackPieceGIS = trackPieceGIS;
            return trackgis;
        }
        /// <summary>
        /// 根据数据表中的同一TrackID行返回轨道片
        /// </summary>
        /// <param name="drs"></param>
        /// <returns></returns>
        private TrackPieceGIS SerializeTrackPieceGIS(List<DataRow> drs)
        {
            int TrackID = Convert.ToInt32(drs[0]["TrackID"]);
            TrackPieceGIS trackPieceGIS = new TrackPieceGIS();
            List<TrackPiece> pieces = new List<TrackPiece>();
            foreach(DataRow dr in drs)
            {
                TrackPiece trackpiece = new TrackPiece();
                trackpiece.DeltaPos = Convert.ToUInt32(dr["DeltaPos"]);
                trackpiece.DeltaLat = Convert.ToInt32(dr["DeltaLat"]);
                trackpiece.DeltaLon = Convert.ToInt32(dr["DeltaLon"]);
                trackpiece.DeltaHeading = Convert.ToInt16(dr["DeltaHeading"]);
                trackpiece.AdjacentTrack = Convert.ToInt32(dr["AdjacentTrack"]);
                pieces.Add(trackpiece);
            }
            trackPieceGIS.TrackID = TrackID;
            trackPieceGIS.TrackPieces = pieces;
            return trackPieceGIS;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private TrackFileProperty SerializeTrackFileProperty()
        {
            TrackFileProperty trackFileProperty = new TrackFileProperty();
            int rows = this.gridView1.RowCount;
            List<DataRow> Drs = new List<DataRow>();
            for (int i = 0; i < rows; i++)// 把所有数据表的行加进来
            {
                Drs.Add(this.gridView1.GetDataRow(i));
            }
            //trackFileProperty.TrackFileType = Convert.ToByte(Drs.Find(a => a["Tag"].ToString().Trim() == "文件类型")["Value"]);
            trackFileProperty.TrackFileStructVersion = Convert.ToByte(Drs.Find(a => a["Tag"].ToString().Trim() == "文件结构版本")["Value"]);
            trackFileProperty.TrackFileDataVersion = Convert.ToByte(Drs.Find(a => a["Tag"].ToString().Trim() == "文件数据版本")["Value"]);
            trackFileProperty.TrackCount = Convert.ToByte(Drs.Find(a => a["Tag"].ToString().Trim() == "轨道个数")["Value"]);
            trackFileProperty.StationNum = Convert.ToUInt16(Drs.Find(a => a["Tag"].ToString().Trim() == "车站编号")["Value"]);
            return trackFileProperty;
        }
        /// <summary>
        /// TrainControlData
        /// </summary>
        private TrainControlData SerializeTrainControlData()
        {
            TrainControlData trainControlData = new TrainControlData();
            trainControlData.SwitchData = SerializeSwitchData();
            trainControlData.BaliseData = SerializeBaliseData();
            trainControlData.StationEdgeData = SerializeStationEdgeData();
            ///因为没有固定设备的数据显示出来
            TrainControlDataProperty t = new TrainControlDataProperty();
            t.StartEdgeInfo = new List<StartEdgeInfo>() { new StartEdgeInfo() };
            t.EndEdgeInfo = new List<EndEdgeInfo>() { new EndEdgeInfo() };
            ////
            trainControlData.TrainControlDataProperty = t;
            return trainControlData;
        }
        private SwitchData SerializeSwitchData()
        {
            SwitchData switchData = new SwitchData();
            int rows = this.gridView5.RowCount;
            List<DataRow> Drs = new List<DataRow>();
            for (int i = 0; i < rows; i++)// 把所有数据表的行加进来
            {
                Drs.Add(this.gridView5.GetDataRow(i));
            }
            List<Switch> switches = new List<Switch>();
            foreach(DataRow r in Drs)
            {
                Switch s = new Switch();
                int switchDirection = (int)Enum.Parse(typeof(Direction), r["SwitchDirection"].ToString());
                s.Type = Convert.ToByte(r["Type"]);
                s.SwitchNum = Convert.ToByte(r["SwitchNum"]);
                s.SwitchDirection = Convert.ToByte(switchDirection);
                s.SwitchPOI = Convert.ToUInt32(r["SwitchPOI"]);
                s.SwitchPos = Convert.ToUInt32(r["SwitchPos"]);
                s.SwitchReversePos = Convert.ToUInt32(r["SwitchReversePos"]);
                s.LastOffset = Convert.ToUInt16(r["LastOffset"]);
                s.SwitchPosOffset = Convert.ToUInt16(r["SwitchPosOffset"]);
                s.NextOffset = Convert.ToUInt16(r["NextOffset"]);
                s.PreSwitchTrackNum = Convert.ToByte(r["PreSwitchTrackNum"]);
                s.SwitchPosTrackNum = Convert.ToByte(r["SwitchPosTrackNum"]);
                s.SwitchReverseTrackNum = Convert.ToByte(r["SwitchReverseTrackNum"]);
                switches.Add(s);
            }
            switchData.Switches = switches;
            return switchData;
        }
        private BaliseData SerializeBaliseData()
        {
            BaliseData baliseData = new BaliseData();
            int rows = this.gridView7.RowCount;
            List<DataRow> Drs = new List<DataRow>();
            for (int i = 0; i < rows; i++)// 把所有数据表的行加进来
            {
                Drs.Add(this.gridView7.GetDataRow(i));
            }
            List<Balise> balises = new List<Balise>();
            foreach(DataRow dr in Drs)
            {
                Balise b = new Balise();
                b.Type = Convert.ToByte(dr["Type"]);
                b.LastOffset = Convert.ToUInt16(dr["LastOffset"]);
                b.NextOffset = Convert.ToUInt16(dr["NextOffset"]);
                b.BaliseNum = Convert.ToUInt16(dr["BaliseNum"]);
                b.DataSize = Convert.ToByte(dr["DataSize"]);
                b.BaliseProperty = Convert.ToByte(dr["BaliseProperty"]);
                b.BalisePos = Convert.ToUInt32(dr["BalisePos"]);
                b.BaliseTrackNum = Convert.ToByte(dr["BaliseTrackNum"]);
                b.BaliseLoc = Convert.ToByte(dr["BaliseLoc"]);
                b.BaliseDataPort = Convert.ToUInt32(dr["BaliseDataPort"]);
                balises.Add(b);
            }
            baliseData.Balises = balises;
            return baliseData;
        }
        private StationEdgeData SerializeStationEdgeData()
        {
            StationEdgeData stationEdgeData = new StationEdgeData();
            List<StartEdge> startEdges = new List<StartEdge>();
            List<EndEdge> endEdges = new List<EndEdge>();
            List<DataRow> Drs = new List<DataRow>();
            int rows = this.gridView6.RowCount;
            for (int i = 0; i < rows; i++)// 把所有数据表的行加进来
            {
                Drs.Add(this.gridView6.GetDataRow(i));
            }
            var startrows = Drs.FindAll(a => a["NameType"].ToString().Trim() == "起始管辖边界");
            var endrows = Drs.FindAll(a => a["NameType"].ToString().Trim() == "结束管辖边界");
            foreach(DataRow dr in startrows)
            {
                StartEdge s = new StartEdge();
                int i = (int)Enum.Parse(typeof(StartEdgeType), dr["Type"].ToString().Trim());
                s.Type = (Byte)i;
                s.AdjacentDataOffset = Convert.ToUInt32(dr["AdjacentDataOffset"]);
                s.AdjacentTsrsNum = Convert.ToUInt32(dr["AdjacentTsrsNum"]);
                s.AdjacentStationNum = Convert.ToUInt16(dr["AdjacentStationNum"]);
                s.AdjacentEdgeType = (Byte)i;
                s.AdjacentTrackNum = Convert.ToByte(dr["AdjacentTrackNum"]);
                s.Position = Convert.ToUInt32(dr["Position"]);
                s.EdgeTrackNum = Convert.ToByte(dr["EdgeTrackNum"]);
                startEdges.Add(s);
            }
            foreach(DataRow dr in endrows)
            {
                EndEdge e = new EndEdge();
                int i = (int)Enum.Parse(typeof(EndEdgeType), dr["Type"].ToString().Trim());
                e.Type = (Byte)i;
                e.AdjacentDataOffset = Convert.ToUInt32(dr["AdjacentDataOffset"]);
                e.AdjacentTsrsNum = Convert.ToUInt32(dr["AdjacentTsrsNum"]);
                e.AdjacentStationNum = Convert.ToUInt16(dr["AdjacentStationNum"]);
                e.AdjacentEdgeType = (Byte)i;
                e.AdjacentTrackNum = Convert.ToByte(dr["AdjacentTrackNum"]);
                e.Position = Convert.ToUInt32(dr["Position"]);
                e.EdgeTrackNum = Convert.ToByte(dr["EdgeTrackNum"]);
                endEdges.Add(e);
            }
            stationEdgeData.StartEdges = startEdges;
            stationEdgeData.EndEdges = endEdges;
            return stationEdgeData;
        }
        /// <summary>
        /// StationProperties
        /// </summary>
        /// <returns></returns>
        private StationProperties SerializeStationProperties()
        {
            StationProperties stationProperties = new StationProperties();
            stationProperties.StationName = txt_StationName.EditValue.ToString();
            stationProperties.StationNum = Convert.ToUInt16(txt_DownlinkStationNum.EditValue);
            stationProperties.UplinkStationCount = Convert.ToByte(txt_UplinkStationCount.EditValue);
            stationProperties.UplinkStationNum = Convert.ToUInt16(txt_UplinkStationNum.EditValue);
            stationProperties.DownlinkStationCount = Convert.ToByte(txt_DownlinkStationCount.EditValue);
            stationProperties.DownlinkStationNum = Convert.ToUInt16(txt_DownlinkStationNum.EditValue);
            stationProperties.MaximumLat = Convert.ToInt32(txt_maxLat.EditValue);
            stationProperties.MinimumLon = Convert.ToInt32(txt_miniLon.EditValue);
            stationProperties.MaximumLon = Convert.ToInt32(txt_maxLon.EditValue);
            stationProperties.MinimumLat = Convert.ToInt32(txt_miniLat.EditValue);
            stationProperties.TrackGISCRC = Convert.ToUInt32(txt_TrackGISCRC.EditValue);
            stationProperties.TrackGISFileSize = Convert.ToUInt16(txt_TrackGISFileSize.EditValue);
            stationProperties.TrackGISVersion = Convert.ToByte(se_TrackGISVersion.EditValue);
            stationProperties.TrainControlDataCRC = Convert.ToUInt32(txt_trainControlDataCRC.EditValue);
            stationProperties.TrainControlDataSize = Convert.ToUInt32(txt_trainControlDataSize.EditValue);
            stationProperties.TrainControlDataversion = Convert.ToByte(se_trainControlDataVersion.EditValue);
            return stationProperties;
        }
        #endregion
        /// <summary>
        /// 序列化object对象为xml字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string Serialize(object obj)
        {
            return Serialize(obj, false, true);
        }
        /// <summary>
        /// 序列化object对象为XML字符串
        /// </summary>
        /// <param name="obj">实体类或List集合类</param>
        /// <param name="isOmitXmlDeclaration">是否去除Xml声明<?xml version="1.0" encoding="utf-8"?></param>
        /// <param name="isIndent">是否缩进显示</param>
        /// <returns></returns>
        private static string Serialize(object obj,bool isOmitXmlDeclaration,bool isIndent)
        {
            try
            {
                string xmlString;
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                //去除xml声明
                xmlWriterSettings.OmitXmlDeclaration = isOmitXmlDeclaration;
                xmlWriterSettings.Indent = isIndent;
                xmlWriterSettings.Encoding = Encoding.UTF8;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
                    {
                        //去除默认命名空间xmlns:xsd和xmlns:xsi
                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        ns.Add("", "");
                        //序列化对象
                        XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
                        xmlSerializer.Serialize(xmlWriter, obj, ns);
                    }
                    xmlString = Encoding.UTF8.GetString(memoryStream.ToArray());
                }
                return xmlString.TrimStart('s');
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        private static string Serialize<T>(T model)where T:class
        {
            string xml;
            using (var ms = new MemoryStream())
            {
                XmlSerializer xmlSer = new XmlSerializer(typeof(T));
                xmlSer.Serialize(ms, model);
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                xml = sr.ReadToEnd();
            }
            return xml;
        }
        #endregion

        #region  找不到数据则显示没有匹配到数据
        private void gridView4_CustomDrawEmptyForeground(object sender, DevExpress.XtraGrid.Views.Base.CustomDrawEventArgs e)
        {
            if (gridView4.RowCount == 0)
            {
                //文本
                string str = "暂未查找到匹配的数据!";
                //字体
                System.Drawing.Font f = new System.Drawing.Font("微软雅黑", 16);
                //显示位置
                Rectangle r = new Rectangle(trackheadinfo_gctrl.Width / 2 - 100, e.Bounds.Top + 45, e.Bounds.Right - 5, e.Bounds.Height - 5);
                //显示颜色
                e.Graphics.DrawString(str, f, Brushes.Gray, r);
            }
        }
        private void gridView2_CustomDrawEmptyForeground(object sender, DevExpress.XtraGrid.Views.Base.CustomDrawEventArgs e)
        {
            if (gridView1.RowCount == 0)
            {
                //文本
                string str = "暂未查找到匹配的数据!";
                //字体
                System.Drawing.Font f = new System.Drawing.Font("微软雅黑", 16);
                //显示位置
                Rectangle r = new Rectangle(switchdata_gctrl.Width / 2 - 100, e.Bounds.Top + 45, e.Bounds.Right - 5, e.Bounds.Height - 5);
                //显示颜色
                e.Graphics.DrawString(str, f, Brushes.Gray, r);
            }
        }
        private void gridView1_CustomDrawEmptyForeground(object sender, DevExpress.XtraGrid.Views.Base.CustomDrawEventArgs e)
        {
            if (gridView1.RowCount == 0)
            {
                //文本
                string str = "暂未查找到匹配的数据!";
                //字体
                System.Drawing.Font f = new System.Drawing.Font("微软雅黑", 16);
                //显示位置
                Rectangle r = new Rectangle(trackPieces_gctrl.Width / 2 - 100, e.Bounds.Top + 45, e.Bounds.Right - 5, e.Bounds.Height - 5);
                //显示颜色
                e.Graphics.DrawString(str, f, Brushes.Gray, r);
            }
        }
        private void gridView6_CustomDrawEmptyForeground(object sender, DevExpress.XtraGrid.Views.Base.CustomDrawEventArgs e)
        {
            if (gridView6.RowCount == 0)
            {
                //文本
                string str = "暂未查找到匹配的数据!";
                //字体
                System.Drawing.Font f = new System.Drawing.Font("微软雅黑", 16);
                //显示位置
                Rectangle r = new Rectangle(stationEdgeData_gctrl.Width / 2 - 100, e.Bounds.Top + 45, e.Bounds.Right - 5, e.Bounds.Height - 5);
                //显示颜色
                e.Graphics.DrawString(str, f, Brushes.Gray, r);
            }
        }
        private void gridView5_CustomDrawEmptyForeground(object sender, DevExpress.XtraGrid.Views.Base.CustomDrawEventArgs e)
        {
            if (gridView5.RowCount == 0)
            {
                //文本
                string str = "暂未查找到匹配的数据!";
                //字体
                System.Drawing.Font f = new System.Drawing.Font("微软雅黑", 16);
                //显示位置
                Rectangle r = new Rectangle(switchdata_gctrl.Width / 2 - 100, e.Bounds.Top + 45, e.Bounds.Right - 5, e.Bounds.Height - 5);
                //显示颜色
                e.Graphics.DrawString(str, f, Brushes.Gray, r);
            }
        }
        private void gridView7_CustomDrawEmptyForeground(object sender, DevExpress.XtraGrid.Views.Base.CustomDrawEventArgs e)
        {
            if (gridView7.RowCount == 0)
            {
                //文本
                string str = "暂未查找到匹配的数据!";
                //字体
                System.Drawing.Font f = new System.Drawing.Font("微软雅黑", 16);
                //显示位置
                Rectangle r = new Rectangle(balisedata_gctrl.Width / 2 - 100, e.Bounds.Top + 45, e.Bounds.Right - 5, e.Bounds.Height - 5);
                //显示颜色
                e.Graphics.DrawString(str, f, Brushes.Gray, r);
            }
        }
        #endregion

        #region 单位转换工具
        private void btnTransferDMS_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ToolForm toolForm = new ToolForm();
            toolForm.ShowDialog(this);
        }
        private void btnTransferCoords_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ToolForm toolForm = new ToolForm();
            toolForm.ShowDialog(this);
        }
        #endregion

        #region FlowLayout控件大小
        int oldWidth = 0;
        private void Form1_Load(object sender, EventArgs e)
        {
            oldWidth = splitContainerControl5.Panel2.Width;
        }
        private void flowLayoutPanel1_Resize(object sender, EventArgs e)
        {
            if (oldWidth != this.flowLayoutPanel1.Width)
            {
                foreach (Control c in this.flowLayoutPanel1.Controls)
                {
                    c.Size = new Size(this.flowLayoutPanel1.Width - 2, c.Height);
                    oldWidth = this.splitContainerControl5.Panel2.Width;
                }
            }
        }


        #endregion
        #endregion
        
        #region 站场图绘制

        private void btnOpenVsdx_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
        

        private void barEditItem2_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
        



        #endregion




    }
}
