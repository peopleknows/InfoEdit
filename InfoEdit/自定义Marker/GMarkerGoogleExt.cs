 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
namespace GMap.NET.WindowsForms.Markers
{
    public  class GMarkerGoogleExt:GMarkerGoogle
    {
        public double KiloPos { get; set; }
        public GMarkerGoogleExt(PointLatLng p,GMarkerGoogleType MarkerType,Color b,int r,bool isCircle):base(p,MarkerType)
        {
            this.IsHitTestVisible = true;
            InRouteIndex = -1;
            this.isCircle = isCircle;
            InitialCircleParas(b, r); 
        }

        public GMarkerGoogleExt(PointLatLng p, GMarkerGoogleType MarkerType) : base(p, MarkerType)
        {
            this.IsHitTestVisible = true;
            InRouteIndex = -1;
            this.isCircle = false; 
            InitialCircleParas(Color.Red);
        }
         public GMarkerGoogleExt(PointLatLng p, Bitmap Image, Color b, int r, bool isCircle) : base(p, Image)
        {
            this.IsHitTestVisible = true;
            InRouteIndex = -1;
            this.isCircle = isCircle; 
            InitialCircleParas(b, r); 
        }

        public GMarkerGoogleExt(PointLatLng p, Bitmap Image, GMapRouteExt route, Color b, int r, bool isCircle) : base(p, Image)
        {
            this.BindRoute = route;
            InRouteIndex = -1;
            this.isCircle = isCircle;
            InitialCircleParas(b, r);
        }
        private void InitialCircleParas(Color b, int r)
        {
            Size = new System.Drawing.Size(2 * r, 2 * r);
            Offset = new System.Drawing.Point(-r, -r);
            OutPen = new Pen(b, 2);
        }
        private void InitialCircleParas(Color b)
        {  
            OutPen = new Pen(b, 2);
        }
        private bool isCircle; 
        public GMapRouteExt BindRoute { get; set; } 
        public int InRouteIndex { get; set; }

        public Pen Pen
        {
            get;
            set;
        }

        public Pen OutPen
        {
            get;
            set;
        }
        public bool AddRect { get; set; }
        public override void OnRender(Graphics g)
        {
            Rectangle rect = new Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
            if (!isCircle)
            {
                base.OnRender(g);
                if (this.AddRect)
                {
                    g.DrawRectangle(new Pen(Color.Red, this.OutPen.Width), rect);
                }
            }
            else
            {

                if (OutPen != null)
                {
                    g.DrawEllipse(OutPen, rect);
                }
                if (this.AddRect)
                {
                    g.DrawRectangle(new Pen(Color.Red,this.OutPen.Width), rect);
                }
            }
        }

        public override void Dispose()
        {
            if (Pen != null)
            {
                Pen.Dispose();
                Pen = null;
            }

            if (OutPen != null)
            {
                OutPen.Dispose();
                OutPen = null;
            }

            base.Dispose();
        }
    } 
}
