using System.Drawing;
/**
 * Blob.java
 * Assignment: Final Project / Blob Suurvival
 * Purpose: stores data about a blob.
 *
 * @version 6/21/2016
 * @author Alex Maclean
 */
namespace BlobServer
{
    class Blob
    {
        public double X;
        public double Y;
        public double Z;
        public double Radius;
        public Color Color;

        //initalize a blob given location color and size
        public Blob(double x, double y, double z, double radius, Color color)
        {
            X = x;
            Y = y;
            Z = z;
            Radius = radius;
            Color = color;
        }

        //makes a unshared copy of a blob
        public Blob(Blob b)
        {
            X = b.X;
            Y = b.Y;
            Z = b.Z;
            Radius = b.Radius;
            Color = b.Color;
        }
    }
}

