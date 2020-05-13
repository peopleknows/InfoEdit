using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfoEdit
{
     public class TopologyElement//拓扑点线的属性数据
    {
        /// <summary>
        /// 点
        /// </summary>
          public class Point
        {
            public int ID { get; set; }//点id
            public double X { get; set; }//X坐标
            public double Y { get; set; }//Y坐标

            public double Lat { get; set; }
            public double Lon { get; set; }
            // public bool IsVisited { get; set; }//搜索

            public int Arc1Id { get; set; }//线段1主要是弧的编号
            public int Arc2Id { get; set; }//线段2
            public int Arc3Id { get; set; }//线段3
            public int Arc4Id { get; set; }//线段4
            public int TrackId { get; set; }//所属的轨道编号
        }
        


        public class Signal:  Point
        {
            public int Direction { get; set; }//防护方向

            public int NextObjectID { get; set; }//下一个数据对象的id

            public int NextObjectType { get; set; }//下一个数据对象的类型
        }

        public class Balise:  Point
        {
            public int Direction { get; set; }//防护方向

            public int NextObjectID { get; set; }//下一个数据对象的id

            public int NextObjectType { get; set; }//下一个数据对象的类型

        }

           abstract public class Arc
           {
            public int Id { get; set; }//线段ID
            public int StartId { get; set; }//起点ID
            public int EndId { get; set; }//终点ID
            public double Length { get; set; }//长度(是经纬度求距离/公里标)
            public double Angle { get; set; }//角度(计划用经纬度与正北方向夹角表示，在绘制站场图形的时候并不会用到,只会用来拟合直线)
            public double Radius { get; set; }//曲率半径
            public int TrackID { get; set; }//所属轨道ID
           }
        
        /// <summary>
        /// 轨道片类型的弧：包括了轨道片在轨道中的位置索引，从0开始
        /// </summary>
        public class TrackPieceArc:Arc
        {
            public int TrackPieceIndex { get; set; }
        }

        /// <summary>
        /// 轨道类型的弧:新增了点集和轨道片的id
        /// </summary>
        public class TrackArc:Arc
        {
            public List<int> PointsId { get; set; }
            public List<int> PiecesId { get; set; }

        }

        /// <summary>
        /// 渡线类型的弧，增加了方向，点集，轨道片集
        /// </summary>
        public class Crossline:Arc
        {
            public List<int> PointsId { get; set; }
            public List<int> PiecesId { get; set; } 
            public int Direction { get; set; }//这里的方向计划定义为上行至下行过渡，或者下行向上行过渡

        }


        //整理好的拓扑类
        public class Topology
        {
            public string filename;
            private List<Signal> signals = new List<Signal>();
            //信号机设备点
            public List<Signal> Signals { get {return signals; } set {signals=value; } }
            //应答器设备点
            private List<Balise> balises = new List<Balise>();
            public List<Balise> Balises { get {return balises; } set {balises=value; } }
            //不同类型的弧表
            private List<TrackArc> trackArcs = new List<TrackArc>();
            public List<TrackArc> TrackArcs { get {return trackArcs; } set {trackArcs=value; } }

            private List<TrackPieceArc> piecesArcs = new List<TrackPieceArc>();
            public List<TrackPieceArc> PiecesArcs { get {return piecesArcs; } set {piecesArcs=value; } }
            //
            private List<Crossline> crosslines = new List<Crossline>();
            public List<Crossline> CrossLines { get {return crosslines; } set {crosslines=value; } }
            //点集合--无论是什么点
            private List<Point> points = new List<Point>();
            public List<Point> Points { get {return points; } set {points=value; } }
            //点编号集合
            private List<int> pointsId = new List<int>();
            public List<int> PointsId { get {return pointsId; } set {pointsId=value; } }
            //弧集合
            private List<Arc> arcs = new List<Arc>();
            public List<Arc> Arcs { get {return arcs; } set {arcs=value; } }
            //弧编号集合
            private List<int> arcsid = new List<int>();
            public List<int> ArcsId { get {return arcsid; } set {arcsid=value; } }
            //正线类型编号表
            private List<int> trackArcsId = new List<int>();
            public List<int> TrackArcID { get {return trackArcsId; } set {trackArcsId=value; } }
            
            //渡线类型编号表
            private List<int> crossArcsId = new List<int>();
            public List<int> CrossLineID { get {return crossArcsId; } set {crossArcsId=value; } }

            private List<int> tracksId = new List<int>();
            public List<int> TracksId { get {return tracksId; } set {tracksId=value; } }

            public Dictionary<int,string> TrackMenu { get; set; }//轨道编号及类型

            public void ClearAll()
            {
                this.Points.Clear();
                this.Arcs.Clear();
                //
                this.PointsId.Clear();
                this.ArcsId.Clear();
                this.TrackArcID.Clear();
                this.CrossLineID.Clear();
                //
                this.TrackArcs.Clear();
                this.PiecesArcs.Clear();
                this.CrossLines.Clear();
            }
            

            /// <summary>
            /// 计算两条直线逆时针方向的夹角
            /// </summary>
            /// <param name = "start" > 起始点 </ param >
            /// < param name="inflection">拐点</param>
            /// <param name = "end" > 终止点 </ param >
            /// < returns > 弧度 </ returns >
            public double GetAngleOfTwoArcs(Point start, Point inflection, Point end)
            {
                double aa = (start.X - inflection.X) * (start.X - inflection.X) + (start.Y - inflection.Y) * (start.Y - inflection.Y);//平差？
                double bb = (inflection.X - end.X) * (inflection.X - end.X) + ((inflection.Y - end.Y)) * ((inflection.Y - end.Y));
                double cc = (start.X - end.X) * (start.X - end.X) + (start.Y - end.Y) * (start.Y - end.Y);

                double cos = (aa + bb - cc) / (2 * Math.Sqrt(aa) * Math.Sqrt(bb));
                double angle = Math.Acos(cos);

                if (Math.Abs(start.Y - inflection.Y) >= 0.000001)
                {
                    double coeff1 = (inflection.X - start.X) / (start.Y - inflection.Y);
                    double coeff2 = (start.X * inflection.Y - inflection.X * start.Y) / (start.Y - inflection.Y);

                    if (start.Y > inflection.Y)
                    {
                        if (end.X + coeff1 * end.Y + coeff2 <= 0)
                            return angle;
                        return (Math.PI * 2 - angle);
                    }
                    if (start.Y < inflection.Y)
                    {
                        if (end.X + coeff1 * end.Y + coeff2 <= 0)
                            return (Math.PI * 2 - angle);
                        return angle;
                    }
                }
                else
                {
                    if (inflection.X > start.X)
                    {
                        if (end.Y < start.Y)
                            return angle;
                        return (Math.PI * 2 - angle);
                    }
                    if (end.Y < start.Y)
                        return (Math.PI * 2 - angle);
                    return angle;
                }
                return 10.0f;
            }
        }




    }

}
