using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Text.RegularExpressions;
using System.IO;

namespace InfoEdit
{
    public partial class ToolForm : DevExpress.XtraEditors.XtraForm
    {
        private List<TextEdit> txtCtrls = new List<TextEdit>();

        public ToolForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ca.ToolTipController.SetToolTip(this.ca, "经纬度格式为数字度");
        }

        #region 属性
        ///经纬度GroupControl设置
        public string ddd
        { get
            { return txt_ddd.Text.Trim(); }
            set
            {
                 txt_ddd.Text = value; 
            }
        }
        public string dms
        { get {return txt_dms.Text.Trim(); }
            set
            {
                txt_dms.Text = value;
            }
        }
        public string s
        {
            get { return txt_s.Text.Trim(); }
            set
            {
               txt_s.Text = value;
            }
        }
        public string ms
        {
            get { return txt_ms.Text.Trim(); }
            set
            {
                txt_ms.Text = value;
            }
        }

        //参考点GroupControl设置
        public double Lat
        {
            get
            { return Convert.ToDouble(txt_Lat.EditValue); }
            set
            {
                double lat_a = Convert.ToDouble(value);
                if (0 < lat_a && lat_a < 90)
                {txt_Lat.EditValue=lat_a; }
                else
                { txt_Lat.EditValue = null; }
            }
        }
        public double Lon
        {
            get {return Convert.ToDouble(txt_Lon.EditValue); }
            set
            {
                double lon_a = Convert.ToDouble(value);
                if(0<lon_a&&lon_a<180)
                {
                    txt_Lon.EditValue = lon_a;
                }
                else
                { txt_Lon.EditValue = null; }
            }
        }
        public double Hgt
        {
            get { return Convert.ToDouble(txt_Lon.EditValue); }
            set
            { txt_Lon.EditValue = value; }
        }
        public double x { get { return Convert.ToDouble(txt_x.EditValue); } set { txt_x.EditValue = value; } }
        public double y { get { return Convert.ToDouble(txt_y.EditValue); } set { txt_y.EditValue = value; } }
        public double z { get { return Convert.ToDouble(txt_z.EditValue); } set { txt_z.EditValue = value; } }

        //比较点GroupControl设置
        public double Lat2
        {
            get
            { return Convert.ToDouble(txt_Lat2.EditValue); }
            set
            {
                double lat_a = Convert.ToDouble(value);
                if (0 < lat_a && lat_a < 90)
                { txt_Lat2.EditValue = lat_a; }
                else
                { txt_Lat2.EditValue = null; }
            }
        }
        public double Lon2
        {
            get { return Convert.ToDouble(txt_Lon2.EditValue); }
            set
            {
                double lon_a = Convert.ToDouble(value);
                if (0 < lon_a && lon_a < 180)
                {
                    txt_Lon2.EditValue = lon_a;
                }
                else
                { txt_Lon2.EditValue = null; }
            }
        }
        public double Hgt2
        {
            get { return Convert.ToDouble(txt_Lon2.EditValue); }
            set
            {
                txt_Lon2.EditValue = value;
            }
        }
        public double x2 { get { return Convert.ToDouble(txt_x2.EditValue); } set { txt_x2.EditValue = value; } }
        public double y2 { get { return Convert.ToDouble(txt_y2.EditValue); } set { txt_y2.EditValue = value; } }
        public double z2 { get { return Convert.ToDouble(txt_z2.EditValue); } set { txt_z2.EditValue = value; } }

        //相对位置
        public double deltax { get { return Convert.ToDouble(txt_Deltax.EditValue); } set { txt_Deltax.EditValue = value; } }
        public double deltay { get { return Convert.ToDouble(txt_Deltay.EditValue); } set { txt_Deltay.EditValue = value; } }
        public double deltaz { get { return Convert.ToDouble(txt_Deltaz.EditValue); } set { txt_Deltaz.EditValue = value; } }

        public double Distance { get {return Convert.ToDouble(txt_Distance.EditValue); } set {txt_Distance.EditValue=value; }}

        public double Heading { get {return Convert.ToDouble(txt_Heading.EditValue); } set {txt_Heading.EditValue=value; } }

        #endregion 属性

        #region BLH2XYZ
        private const double a = 6378137;
        private const double f = 1 / 298.257222101;

        private static void BLH2XYZ(double B, double L, double H, out double X, out double Y, out double Z)
        {
            // 三个输入B、L、H，三个输出X、Y、Z
            // 角度转弧度
            B = B * Math.PI / 180;
            L = L * Math.PI / 180;
            // 计算相关参数
            double e2 = GetE2();
            double N = GetN(B);
            // 转化计算
            X = (N + H) * Math.Cos(B) * Math.Cos(L);
            Y = (N + H) * Math.Cos(B) * Math.Sin(L);
            Z = (N * (1 - e2) + H) * Math.Sin(B);
        }
        private static void BLH2XYZ(double[] blh,out double[] xyz)
        {
            double[] _xyz = new double[3];
            double B = blh[0];
            double L = blh[1];
            double H = blh[2];
            BLH2XYZ(B, L, H, out _xyz[0], out _xyz[1], out _xyz[2]);
            xyz = _xyz;
        }
        private static double GetE2()
        {
            double e2 = 2 * f - f * f;
            return e2;
        }
        private static double GetN(double B)
        {
            double e2 = GetE2();
            double sinB = Math.Sin(B);
            double N = a / Math.Sqrt(1 - e2 * sinB * sinB);
            return N;
        }

        #endregion

        #region WGS2UTM
        static double pi = Math.PI;
        static double sm_a = 6378137.0;
        static double sm_b = 6356752.314;
        //static double sm_EccSquared = 6.69437999013e-03;

        static double UTMScaleFactor = 0.9996;

        private static double[] LatLonToUTM(double[] latlon)
        {
            double[] utmxyz = LatLonToUTM(latlon[0], latlon[1]);
            return utmxyz;
        }

        //得到的结果是：x坐标，y坐标，区域编号
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

        #region ECEF2WGS84
        public static string ECEFtoWGS84(double x, double y, double z)

        {
            double a, b, c, d;
            double Longitude;//经度
            double Latitude;//纬度
            double Altitude;//海拔高度
            double p, q;
            double N;
            a = 6378137.0;
            b = 6356752.31424518;
            c = Math.Sqrt(((a * a) - (b * b)) / (a * a));
            d = Math.Sqrt(((a * a) - (b * b)) / (b * b));
            p = Math.Sqrt((x * x) + (y * y));
            q = Math.Atan2((z * a), (p * b));
            Longitude = Math.Atan2(y, x);
            Latitude = Math.Atan2((z + (d * d) * b * Math.Pow(Math.Sin(q), 3)), (p - (c * c) * a * Math.Pow(Math.Cos(q), 3)));
            N = a / Math.Sqrt(1 - ((c * c) * Math.Pow(Math.Sin(Latitude), 2)));
            Altitude = (p / Math.Cos(Latitude)) - N;
            Longitude = Longitude * 180.0 / Math.PI;
            Latitude = Latitude * 180.0 / Math.PI;
            return Longitude + "," + Latitude + "," + Altitude;
        }

        #endregion

        #region WGS84下经纬度的距离
        public static double getDistance(double lng1, double lat1, double lng2, double lat2)
        {
            double radLat1 = lat1 * Math.PI / 180;
            double radLat2 = lat2 * Math.PI / 180;
            double a = radLat1 - radLat2;
            double b = lng1 * Math.PI / 180 - lng2 * Math.PI / 180;
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1)
                    * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * 6378137.0;// 取WGS84标准参考椭球中的地球长半径(单位:m)
            s = Math.Round(s * 10000) / 10000;
            return s;
        }
        #endregion

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            if (chooseFile != null)
            {
                //获取没有后缀的文件名字;
                string filename = chooseFile.Remove(chooseFile.LastIndexOf("."), 4);
                List<string[]> listLatLng = new List<string[]>();
                ReadFormatTxt(chooseFile, ref listLatLng);
                //
                SaveFile(filename, cbOpertate.SelectedIndex, listLatLng);
            }
            else
            {
                XtraMessageBox.Show("请选择一个文件进行操作");
            }
        }

        /// <summary>
        /// 批量转换操作
        /// </summary>
        /// <param name="methordIndex">方法索引，就是comboxEdit的index</param>
        private void SaveFile(string filepath,int methodIndex,List<string[]> list)
        {
            try
            {
                if (list != null)
                {
                    if (methodIndex == 0)//单位转换操作
                    {
                        List<string[]> s = new List<string[]>();
                        if (cbTransfer.SelectedIndex == 0)//转换成数字度
                        {
                            //暂时无法确定取得文件中格式的问题
                        }
                        else if (cbTransfer.SelectedIndex == 1)//转换成度分秒
                        {
                            filepath = filepath + "度分秒格式" + ".txt";
                            foreach (string[] a in list)
                            {
                                string[] latlng = new string[2];
                                latlng[0] = ConvertDigitalToDegrees(a[0]);
                                latlng[1] = ConvertDigitalToDegrees(a[1]);
                                s.Add(latlng);
                            }
                            SaveFile(filepath, s);
                            XtraMessageBox.Show("录入成功，文件保存至：" + filepath.Substring(filepath.LastIndexOf("\\") + 1));
                        }
                        else if (cbTransfer.SelectedIndex == 2)//转换成秒
                        {
                            filepath = filepath + "秒单位" + ".txt";
                            foreach (string[] a in list)
                            {
                                string[] latlng = new string[2];
                                latlng[0] = ConvertDigitalToSecond(a[0]);
                                latlng[1] = ConvertDigitalToSecond(a[1]);
                                s.Add(latlng);
                            }
                            SaveFile(filepath, s);
                            XtraMessageBox.Show("录入成功，文件保存至：" + filepath.Substring(filepath.LastIndexOf("\\") + 1));
                        }
                        else if (cbTransfer.SelectedIndex == 3)//转换成毫秒
                        {
                            filepath = filepath + "毫秒单位" + ".txt";
                            foreach (string[] a in list)
                            {
                                string[] latlng = new string[2];
                                ConvertDigitalToSecond(a[0], out latlng[0]);
                                ConvertDigitalToSecond(a[1], out latlng[1]);
                                s.Add(latlng);
                            }
                            SaveFile(filepath, s);
                            XtraMessageBox.Show("录入成功，文件保存至：" + filepath.Substring(filepath.LastIndexOf("\\") + 1));
                        }
                        else
                        {
                            XtraMessageBox.Show("请在经纬度操作下选择任意一项进行操作");
                        }

                    }
                    else if (methodIndex == 1)//求相对坐标操作
                    {
                        List<string[]> s = new List<string[]>();
                        if (cbTransferCoords.SelectedIndex == 0)//BLH2XYZ转ECEF下坐标
                        {
                            filepath = filepath + "_ECEF坐标" + ".txt";
                            foreach (string[] a in list)
                            {
                                double[] xyz = new double[3];
                                BLH2XYZ(Convert.ToDouble(a[0]), Convert.ToDouble(a[1]), Convert.ToDouble(a[2]), out xyz[0], out xyz[1], out xyz[2]);
                                string[] x_y = new string[2] { xyz[0].ToString(), xyz[1].ToString() };
                                s.Add(x_y);
                            }
                            SaveFile(filepath, s);
                            XtraMessageBox.Show("录入成功，文件保存至：" + filepath.Substring(filepath.LastIndexOf("\\") + 1));
                        }
                        else if(cbTransferCoords.SelectedIndex == 1)//WGS84UTM下坐标
                        {
                            filepath = filepath + "_UTM坐标" + ".txt";
                            foreach (string[] a in list)
                            {
                                double[] ad = new double[2] { Convert.ToDouble(a[0]), Convert.ToDouble(a[1]) };
                                double[] xyzone = new double[3];
                                xyzone = LatLonToUTM(ad);
                                string[] xyzone2 = new string[3] { xyzone[0].ToString(), xyzone[1].ToString(), xyzone[2].ToString() };
                                s.Add(xyzone2);
                            }
                            SaveFile(filepath, s);
                            XtraMessageBox.Show("录入成功，文件保存至：" + filepath.Substring(filepath.LastIndexOf("\\") + 1));
                        }
                        else if(cbTransferCoords.SelectedIndex==2)//WGS84转CGCS2000
                        {
                            filepath = filepath + "_CGCS2000坐标" + ".txt";
                            foreach (string[] a in list)
                            {
                                double[] latlng = new double[2];
                                latlng = transform(Convert.ToDouble(a[2]), Convert.ToDouble(a[1]));
                                string[] ll = new string[2] { latlng[0].ToString(), latlng[1].ToString() };
                                s.Add(ll);
                            }
                            SaveFile(filepath, s);
                            XtraMessageBox.Show("录入成功，文件保存至：" + filepath.Substring(filepath.LastIndexOf("\\") + 1));
                        }
                        else
                        {
                            XtraMessageBox.Show("请在坐标转换操作下选择任意一项进行操作");
                        }
                    }
                    else if (methodIndex == 2)//求航向角和增量航向角
                    {
                        filepath = filepath + "航向角和增量航向角.txt";
                        List<string[]> deltas = new List<string[]>();
                        for (int j = 0; j < list.Count - 2; j++)
                        {
                            string[] ss = new string[3];
                            double a = GetBear(list[j], list[j + 1]);
                            double b = GetBear(list[j + 1], list[j + 2]);
                            ss[0] = a.ToString();
                            ss[1] = b.ToString();
                            ss[2] = (b - a).ToString();
                            deltas.Add(ss);
                        }
                        SaveFile(filepath, deltas);
                        XtraMessageBox.Show("录入成功，文件保存至：" + filepath.Substring(filepath.LastIndexOf("\\") + 1));
                    }
                    else
                    {
                        XtraMessageBox.Show("请选择一种操作进行批量转换");
                    }
                }
                else
                {
                    XtraMessageBox.Show("文件内容为空!");
                }
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 得到增量航向角
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double GetBear(string[] a, string[] b)
        {
            double lat = Convert.ToDouble(a[1]);//纬度
            double lon = Convert.ToDouble(a[0]);//经度
            double latb = Convert.ToDouble(b[1]);
            double lonb = Convert.ToDouble(b[0]);
            double bear = GetAngle(lat, lon, latb, lonb);
            return bear;
        }



        //public static List<string[]> list = new List<string[]>();//保存txt文件中的经纬度高
        //public static List<string[]> listLatLng = new List<string[]>();//保存txt文件中的经纬度
        //public int listColumn;//判断文件有几列

        /// <summary>
        /// 去除txt文件每行中的Tab键
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="list"></param>
        private void ReadFormatTxt(string filePath,ref List<string[]> list)
        {
            try
            {
                list.Clear();//首先清空list
                StreamReader sr = new StreamReader(filePath);
                string[] lines = File.ReadAllLines(filePath, Encoding.Default);
                foreach (var line in lines)
                {
                    string temp = line.Trim();
                    if (temp != "")
                    {
                        string[] arr = temp.Split(new char[] { '\t', ' ',',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr.Length > 0)
                        {
                            list.Add(arr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        
        /// <summary>
        /// 保存经纬度操作后的文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="LatLng"></param>
        private void SaveFile(string filePath,List<string[]> LatLng)
        {
            try
            {
                StreamWriter sw = new StreamWriter(filePath, false);
                string s = "";
                foreach (string[] a in LatLng)
                {
                    for(int j=0;j<a.Length;j++)
                    { s += a[j] + "\t"; }
                    //LatLon中间加tab键
                    s += "\r\n";
                }
                sw.Write(s);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
        
        private string[] filePaths = null;
        private void btnOpenFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog oFD = new OpenFileDialog();
            oFD.InitialDirectory = Application.StartupPath;
            oFD.Title = "打开文件";
            oFD.Multiselect = true;
            oFD.Filter = "文本文件(*.txt)|*.txt";
            //oFD.Filter = "表格文件(*.xls;*.xlsx)|*.xls;*.xlsx|文本文件(*.txt)|*.txt|XML文件(*.xml)|*.xml|所有文件|*.*";
            //oFD.FilterIndex = 2;
            oFD.RestoreDirectory = true;
            cbFiles.Properties.Items.Clear();
            if (oFD.ShowDialog() == DialogResult.OK)
            {
                filePaths = oFD.FileNames;
                foreach (string s in filePaths)
                {
                    cbFiles.Properties.Items.Add(s.Substring(s.LastIndexOf("\\") + 1));
                }
            }
        }
        #region 单位转换


        /// <summary>
        /// 数字度转度分秒、秒、毫秒    eg：122.286791987025--->秒440232.451--  *1000得到毫秒440232451
        /// </summary>
        /// <param name="digitalDegree"></param>
        /// <param name="s"></param>
        /// <param name="ss"></param>
        public void DDDtoOthers(string digitalDegree, out string dms, out string s, out string ms)
        {
            if (string.IsNullOrEmpty(digitalDegree) || string.IsNullOrWhiteSpace(digitalDegree))
            {
                dms = null;
                s = null;
                ms = null;
            }
            else
            {
                double dDegree = Convert.ToDouble(digitalDegree);
                const double num = 60.0000;
                int dms_d = (int)dDegree;
                double temp = (dDegree - dms_d) * num;
                int dms_m = (int)temp;
                double dms_s = (temp - dms_m) * num;
                dms_s = Math.Round(dms_s, 3);
                double second = dms_d * num * num + dms_m * num + dms_s;
                //
                dms = ConvertDigitalToDegrees(dDegree);

                s = second.ToString();

                ms = (second * 1000.00000000).ToString();
            }
        }

        
        /// <summary>
        /// 度分秒转数字度、秒、毫秒
        /// </summary>
        /// <param name="dms">经纬度度分秒</param>
        /// <param name="ddd">数字度</param>
        /// <param name="s">秒</param>
        /// <param name="ms">毫秒</param>
        private void DMStoOthers(string dms,out string ddd,out string s,out string ms)
        {
            if (string.IsNullOrWhiteSpace(dms) || string.IsNullOrEmpty(dms))
            {
                ddd = null;
                s = null;
                ms = null;
            }
            else
            {
                ddd = ConvertDegreesToDigital(dms).ToString();
                ConvertDigitalToSecond(ddd,out s,out ms);
            }
        }


        /// <summary>
        /// 秒转毫秒、数字度、度分秒
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ddd"></param>
        /// <param name="dms"></param>
        /// <param name="ms"></param>
        private void  StoOthers(string s,out string ddd,out string dms,out string ms)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
            {
                ddd = null;
                dms = null;
                ms = null;
            }
            else
            {
                double s_a = Convert.ToDouble(s);
                ms = (s_a * 1000.0).ToString();
                ddd = (s_a / 3600.0).ToString();
                dms = ConvertDigitalToDegrees(ddd);
            }
        }

        private void MStoOthers(string ms,out string s,out string ddd,out string dms)
        {
            if(string.IsNullOrEmpty(ms)||string.IsNullOrWhiteSpace(ms))
            {
                s = null;
                ddd = null;
                dms = null;
            }
            else
            {
                double ms_a = Convert.ToDouble(ms);
                s = MsTos(ms);
                ddd = MsToDigitalDegree(ms);
                dms = ConvertDigitalToDegrees(ddd);
            }

        }
        public string ConvertDigitalToDegrees(string ddd)
        {
            double digitalDegeree = Convert.ToDouble(ddd);
            return ConvertDigitalToDegrees(digitalDegeree);
        }

        
        /// <summary>
        /// 数字度转字符串度分秒  eg:122.286791987025---->122°17′12.25″
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
            dms_s = Math.Round(dms_s, 2);
            string degree = "" + dms_d + "°" + dms_m + "'" + dms_s + "″";
            return degree;
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
            int d = degrees.IndexOf("°");
            if (d < 0)
            {
                return digitalDegree;
            }
            string degree = degrees.Substring(0, d);
            digitalDegree += Convert.ToDouble(degree);

            int m = degrees.IndexOf("'"); //分的符号对应的 Unicode 代码为：2032[1]（六十进制），显示为′。
            if (m < 0)
            {
                return digitalDegree;
            }
            string minute = degrees.Substring(d + 1, m - d - 1);
            digitalDegree += ((Convert.ToDouble(minute)) / num);

            int s = degrees.IndexOf("″");  //秒的符号对应的 Unicode 代码为：2033[1]（六十进制），显示为″。
            if (s < 0)
            {
                return digitalDegree;
            }
            string second = degrees.Substring(m + 1, s - m - 1);
            digitalDegree += (Convert.ToDouble(second) / (num * num));

            return digitalDegree;
        }
        //秒转毫秒
        private string StoMs(string s)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
            {
                return null;
            }
            else
            {
                double s_q = Convert.ToDouble(s);
                return (s_q * 1000.0).ToString();
            }
        }

        /// <summary>
        /// 毫秒转秒
        /// </summary>
        /// <param name="ms">毫秒</param>
        /// <returns></returns>
        private string MsTos(string ms)
        {
            if (string.IsNullOrEmpty(ms) || string.IsNullOrWhiteSpace(ms))
            {
                return null;
            }
            else
            {
                double ms_a = Convert.ToDouble(ms);
                return (ms_a / 1000.0).ToString();
                
            }
        }
        //数字度转秒和毫秒  eg：122.286791987025--->秒440232.451--  *1000得到毫秒440232451
        public void ConvertDigitalToSecond(string digitalDegree, out string s, out string ss)
        {
            double dDegree = Convert.ToDouble(digitalDegree);
            const double num = 60.0000;
            int dms_d = (int)dDegree;
            double temp = (dDegree - dms_d) * num;
            int dms_m = (int)temp;
            double dms_s = (temp - dms_m) * num;
            dms_s = Math.Round(dms_s, 3);
            double second = dms_d * num * num + dms_m * num + dms_s;
            s = second.ToString();
            ss = (second * 1000.00000000).ToString();
        }
        public string ConvertDigitalToSecond(string digitalDegree)
        {
            double dDegree = Convert.ToDouble(digitalDegree);
            const double num = 60.0000;
            int dms_d = (int)dDegree;
            double temp = (dDegree - dms_d) * num;
            int dms_m = (int)temp;
            double dms_s = (temp - dms_m) * num;
            dms_s = Math.Round(dms_s, 3);
            double second = dms_d * num * num + dms_m * num + dms_s;
            return second.ToString();
        }

        public void ConvertDigitalToSecond(string digitalDegree,out string ss)
        {
            double dDegree = Convert.ToDouble(digitalDegree);
            const double num = 60.0000;
            int dms_d = (int)dDegree;
            double temp = (dDegree - dms_d) * num;
            int dms_m = (int)temp;
            double dms_s = (temp - dms_m) * num;
            dms_s = Math.Round(dms_s, 3);
            double second = dms_d * num * num + dms_m * num + dms_s;

            ss = (second * 1000.00000000).ToString();
        }

        /// <summary>
        /// 毫秒转数字度
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        private string MsToDigitalDegree(string ms)
        {
            if (string.IsNullOrWhiteSpace(ms) || string.IsNullOrEmpty(ms))
            {
                return null;
            }
            else
            {
                double ms_a = Convert.ToDouble(ms);
                return (ms_a / 1000.0 / 3600.0).ToString();
            }
            
        }

        #endregion

        #region GPS纠偏算法
        /**
        * gps纠偏算法，适用于google,高德体系的地图
        */
        //public static double pi = 3.1415926535897932384626;
        public static double x_pi = 3.14159265358979324 * 3000.0 / 180.0;
        public static double A = 6378245.0;
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
            dLat = (dLat * 180.0) / ((A * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (A / sqrtMagic * Math.Cos(radLat) * pi);
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
            dLat = (dLat * 180.0) / ((A * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (A / sqrtMagic * Math.Cos(radLat) * pi);
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

        #region 计算航向角
        
        private double Radian(double degree)
        {
            return degree * Math.PI / 180;
        }
        private double Degree(double radian)
        {
            return radian * 180 / Math.PI;
        }
        
        private double GetAngle(double lat_a, double lng_a, double lat_b, double lng_b)
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
            return heading;
        }


        #endregion



        private void btnSingleTransfer_Click(object sender, EventArgs e)
        {
            try
            {
                string ddd_a, dms_a, s_a, ms_a = null;
                if (!string.IsNullOrEmpty(ddd) && !string.IsNullOrWhiteSpace(ddd))
                {
                    DDDtoOthers(txt_ddd.Text, out dms_a, out s_a, out ms_a);
                    dms = dms_a;
                    s = s_a;
                    ms = ms_a;
                }
                else if (!string.IsNullOrEmpty(dms) && !string.IsNullOrWhiteSpace(dms))
                {
                    DMStoOthers(txt_dms.Text, out ddd_a, out s_a, out ms_a);
                    ddd = ddd_a;
                    s = s_a;
                    ms = ms_a;
                }
                else if (!string.IsNullOrEmpty(ms) && !string.IsNullOrWhiteSpace(ms))
                {
                    MStoOthers(txt_ms.Text, out s_a, out ddd_a, out dms_a);
                    ddd = ddd_a;
                    dms = dms_a;
                    s = s_a;
                }
                else if (!string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s))
                {
                    StoOthers(txt_s.Text, out ddd_a, out dms_a, out ms_a);
                    ddd = ddd_a;
                    dms = dms_a;
                    ms = ms_a;
                }
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }

        }
        
        private void btnClear_Click(object sender, EventArgs e)
        {
            ddd = null;
            dms = null;
            s = null;
            ms = null;
            cbTransfer.SelectedIndex = -1;
        }
        private string chooseFile = null;
        private void cbFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbFiles.SelectedIndex != -1)
            { chooseFile = filePaths[cbFiles.SelectedIndex]; }
        }

        private void btnClear2_Click(object sender, EventArgs e)
        {
            txt_Lat.EditValue = 0.0;
            txt_Lon.EditValue = 0.0;
            txt_Hgt.EditValue = 0.0;
            txt_x.EditValue = 0.0;
            txt_y.EditValue = 0.0;
            txt_z.EditValue = 0.0;
            cbTransferCoords.SelectedIndex = -1;
        }

        private void btnClear3_Click(object sender, EventArgs e)
        {
            lbl_x.Text = "x";
            lbl_y.Text = "y";
            lbl_z.Text = "z";
            txt_Lat2.EditValue = 0.0;
            txt_Lon2.EditValue = 0.0;
            txt_Hgt2.EditValue = 0.0;
            txt_x2.EditValue = 0.0;
            txt_y2.EditValue = 0.0;
            txt_z2.EditValue = 0.0;
            cbTransferCoords.SelectedIndex = -1;
        }

        private void btnTransfer2_Click(object sender, EventArgs e)
        {
            try
            { GetXYZ(cbTransferCoords.SelectedIndex); }
            catch(Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }

        private void GetXYZ(int methodindex)
        {
            if(methodindex==0)
            {
                lbl_x.Text = "x";
                lbl_y.Text = "y";
                lbl_z.Text = "z";
                double[] BLH = new double[3];
                BLH[0] = Lat;
                BLH[1] = Lon;
                BLH[2] = Hgt;
                double[] xyz = new double[3];
                BLH2XYZ(BLH,out xyz);
                x = xyz[0];
                y = xyz[1];
                z = xyz[2];
            }
            else if(methodindex==1)
            {
                lbl_x.Text = "x";
                lbl_y.Text = "y";
                lbl_z.Text = "zone";
                double[] WGSLatLon = new double[2];
                WGSLatLon[0] = Lat;
                WGSLatLon[1] = Lon;
                double[] utmxyz = LatLonToUTM(WGSLatLon);
                x = utmxyz[0];
                y = utmxyz[1];
                z = utmxyz[2];
            }
            else if(methodindex==2)
            {
                lbl_x.Text = "Lat";
                lbl_y.Text = "Lon";
                lbl_z.Text = "";
                double[] latlon = new double[2];
                latlon[0] = Lat;
                latlon[1] = Lon;
                double[] newLatlon=transform(latlon[0], latlon[1]);
                x = newLatlon[0];
                y = newLatlon[1];
                z = 0.0;
            }
            else
            {
                lbl_x.Text = "x";
                lbl_y.Text = "y";
                lbl_z.Text = "z";
                x = 0.0;
                y = 0.0;
                z = 0.0;
            }
        }

        private void btnTransfer3_Click(object sender, EventArgs e)
        {
            try
            { GetXYZ2(cbTransferCoords.SelectedIndex); }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }

        private void GetXYZ2(int methodindex)
        {
            if (methodindex == 0)
            {
                lbl_x2.Text = "x";
                lbl_y2.Text = "y";
                lbl_z2.Text = "z";
                double[] BLH = new double[3];
                BLH[0] = Lat2;
                BLH[1] = Lon2;
                BLH[2] = Hgt2;
                double[] xyz = new double[3];
                BLH2XYZ(BLH, out xyz);
                x2 = xyz[0];
                y2 = xyz[1];
                z2 = xyz[2];
            }
            else if (methodindex == 1)
            {
                lbl_x2.Text = "x";
                lbl_y2.Text = "y";
                lbl_z2.Text = "zone";
                double[] WGSLatLon = new double[2];
                WGSLatLon[0] = Lat2;
                WGSLatLon[1] = Lon2;
                double[] utmxyz = LatLonToUTM(WGSLatLon);
                x2 = utmxyz[0];
                y2 = utmxyz[1];
                z2 = utmxyz[2];
            }
            else if (methodindex == 2)
            {
                lbl_x2.Text = "Lat";
                lbl_y2.Text = "Lon";
                lbl_z2.Text = "";
                double[] latlon = new double[2];
                latlon[0] = Lat2;
                latlon[1] = Lon2;
                double[] newLatlon = transform(latlon[0], latlon[1]);
                x2 = newLatlon[0];
                y2 = newLatlon[1];
                z2 = 0.0;
            }
            else
            {
                lbl_x2.Text = "x";
                lbl_y2.Text = "y";
                lbl_z2.Text = "z";
                x2 = 0.0;
                y2 = 0.0;
                z2 = 0.0;
            }
        }

        private void GetDelta()
        {
            deltax = x2 - x;
            deltay = y2 - y;
            deltaz = z2 - z;
        }

        private void GetDeltaDistance(int methodindex)
        {
            if (methodindex == 0)//BLH2XYZ
            {
                Distance = getDistance(Lon, Lat, Lon2, Lat2);
                //Distance = Math.Sqrt(Math.Pow(deltax, 2) + Math.Pow(deltay, 2) + Math.Pow(deltaz, 2));
            }
            else if (methodindex == 1)//WGS842UTM
            {
                
            }
            else if (methodindex == 2)//WGS2CGCS
            {
                Distance = 0.0;
            }
            else
            {
                Distance = 0.0;
            }
        }


        private void btnTransfer4_Click(object sender, EventArgs e)
        {
            try
            {
                GetXYZ(cbTransferCoords.SelectedIndex);
                GetXYZ2(cbTransferCoords.SelectedIndex);
                GetDelta();
                GetDeltaDistance(cbTransferCoords.SelectedIndex);
                Heading=GetAngle(Lat, Lon, Lat2, Lon2);

            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }
    }
}