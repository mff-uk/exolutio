using System;
using System.Windows;
using System.Windows.Media;

namespace EvoX.ViewToolkit
{
    public struct Vector
    {
        public Vector(double x, double y) : this()
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public static Vector SubtractPoints(Point p1, Point p2)
        {
            return new Vector { X = p1.X - p2.X, Y = p1.Y - p2.Y };
        }

        public static Point AddVector(Point point, Vector vector)
        {
            return new Point(point.X + vector.X, point.Y + vector.Y);
        }

        public double Length
        {
            get { return Math.Sqrt(X*X + Y*Y); }
        }

        public static Vector operator *(Vector v, double d)
        {
            return new Vector(v.X * d, v.Y * d);
        }

        public static Vector operator *(double d, Vector v)
        {
            return new Vector(v.X * d, v.Y * d);
        }

        public static Point operator +(Vector v, Point p)
        {
            return new Point(p.X + v.X, p.Y + v.Y);
        }

        public static Point operator +(Point p, Vector v)
        {
            return new Point(p.X + v.X, p.Y + v.Y);
        }

        public static Vector operator *(Vector v, Matrix m)
        {
            return new Vector(v.X*m.M11 + v.Y*m.M21, v.X*m.M12 + v.Y*m.M22);
        }

        public void Normalize()
        {
            X = X/Length;
            Y = Y/Length;
        }
    }
}