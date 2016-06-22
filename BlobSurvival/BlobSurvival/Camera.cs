using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
/**
 * Camera.java
 * Assignment: Final Project / Blob Suurvival
 * Purpose: Draw the 3D blbos to the panel 
 * with Math!
 *
 * @version 6/21/2016
 * @author Alex Maclean
 */
namespace BlobSurvival
{
    class Camera
    {
        public double X, Y, Z, Zr = 0, Xr = 0, Yr = 0;
        double _xOffset = 0, _yOffset = 0;
        private const double Focal = 0.4;
        private const double Scale = 250;

        // set the center of the area that the camera will draw to.
        public void SetCenter(int x, int y)
        {
            _xOffset = x;
            _yOffset = y;
        }

        //initalize a camera at specefied location with rotation of 0
        public Camera(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        //changes the cameras rotation
        public void UpdateAngel(double xr, double yr, double zr)
        {
            Xr = xr;
            Yr = yr;
            Zr = zr;

        }

        //changes the cameras location
        public void UpdateLocation(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // get the 2D x location given 3D point
        private double GetPaintX(Blob v)
        {
            return _xOffset - (v.X  /(v.Z * Focal)) * Scale;
        }

        // get the 2D y location given 3D point
        private double GetPaintY(Blob v)
        {
            return (v.Y / (v.Z * Focal)) * Scale + _yOffset;
        }

        //rotate a vertex backward around the camers rotational vector.
        private Blob TransformVertex(Blob v)
        {
            var v2 = new Blob(v.X,v.Y,v.Z,v.Radius,v.Color);
            var vx = v.X;
            var vy = v.Y;
            var vz = v.Z;
            v2.X = Math.Cos(Yr) * (Math.Sin(Zr) * (vy - Y) + Math.Cos(Zr) * (vx - X)) - Math.Sin(Yr) * (vz - Z);
            v2.Y = Math.Sin(Xr) * (Math.Cos(Yr) * (vz - Z) + Math.Sin(Yr) * (Math.Sin(Zr) * (vy - Y) + Math.Cos(Zr) * (vx - X))) + Math.Cos(Xr) * (Math.Cos(Zr) * (vy - Y) - Math.Sin(Zr) * (vx - X));
            v2.Z = Math.Cos(Xr) * (Math.Cos(Yr) * (vz - Z) + Math.Sin(Yr) * (Math.Sin(Zr) * (vy - Y) + Math.Cos(Zr) * (vx - X))) - Math.Sin(Xr) * (Math.Cos(Zr) * (vy - Y) - Math.Sin(Zr) * (vx - X));
            return v2;
        }

        //given a list of blobs, draws all with a specefied graphics, Z-Orderd correctly.
        public void RenderObjects(List<Blob> os, PaintEventArgs e)
        {
            var g = e.Graphics;
            var blobsTemp = os.Select(TransformVertex).ToList();
            blobsTemp.Sort((a, b) => a.Z.CompareTo(b.Z));
            foreach (var bl in blobsTemp) {
                RenderBlob(bl, g);
            }
        }
        //draws a circle representing a blob to the drawing pannel given a graphics 
        public void RenderBlob(Blob blob, Graphics g)
        {
            if (blob.Z < 0)
            {
                var distance =  Math.Sqrt(Math.Pow( blob.X, 2) + Math.Pow(blob.Y, 2) + Math.Pow(blob.Z, 2));
                var size = (blob.Radius/(distance*Focal))*Scale;
                var drawX = (int) Math.Round(GetPaintX(blob) - size);
                var drawY = (int) Math.Round(GetPaintY(blob) - size);
                var colscale = distance/15;
                var col = SafeColor(blob.Color.R/colscale, blob.Color.G/colscale, blob.Color.B/colscale);
                if (drawX < 10000 && drawY < 10000 && drawX > -1000 && drawY > -1000) 
                g.FillEllipse(new SolidBrush(col), drawX, drawY, (int) Math.Round(size*2),(int) Math.Round(size*2));
            }
        }

        //returns a valid color given RGB valuse posibly out of range
        private static Color SafeColor(double r, double g, double b)
        {
            return Color.FromArgb(255, (int)Math.Max(Math.Min(r, 255), 0), (int)Math.Max(Math.Min(g, 255), 0),(int)Math.Max(Math.Min(b, 255), 0));
        }

    }
}
