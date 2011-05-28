using System.Windows;
using Exolutio.SupportingClasses;

namespace Exolutio.ViewToolkit.Geometries
{
    public static class PointExt
    {
        public static bool AlmostEqual(this Point point1, Point point2)
        {
            return System.Math.Abs(point2.X - point1.X) < 3 && System.Math.Abs(point2.Y - point1.Y) < 3;
        }
    }
}