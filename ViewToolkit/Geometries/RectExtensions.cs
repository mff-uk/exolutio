using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Exolutio.ViewToolkit;

namespace Exolutio.ViewToolkit.Geometries
{
	/// <summary>
	/// Static methods add some operations on <see cref="Rect"/> class.
	/// </summary>
	public static class RectExtensions
	{
		/// <summary>
		/// Gets the encompassing rectangle containing all <paramref name="points"/>
		/// </summary>
		/// <param name="points">points to encompass</param>
		public static Rect GetEncompassingRectangle(IEnumerable<Point> points)
		{
			Rect result = Rect.Empty;
			if (points != null)
			{
				foreach (Point point in points)
				{
					result.Union(point);
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the encompassing rectangle containing all <paramref name="rectangles"/>
		/// </summary>
		/// <param name="rectangles">rectangles to encompass</param>
		/// <returns></returns>
		public static Rect GetEncompassingRectangle(IEnumerable<Rect> rectangles)
		{
			Rect result = rectangles.First();
			foreach (Rect rect in rectangles)
			{
				result.Union(rect);
			}
			return result;
		}

		/// <summary>
		/// Gets the encompassing rectangle containing all <paramref name="controls"/>
		/// </summary>
		/// <param name="controls">controls to encompass</param>
		/// <returns></returns>
		public static Rect GetEncompassingRectangle(IEnumerable<Node> controls)
		{
			Rect result = controls.First().GetBounds();
			foreach (Node control in controls)
			{
				Rect rect = control.GetBounds();
				result.Union(rect);
			}
			return result;
		}

		/// <summary>
		/// Moves the rectangle to coordinates [0, 0]
		/// </summary>
		/// <param name="rectangle">moved rectangle</param>
		public static Rect Normalize(this Rect rectangle)
		{
			return new Rect(0, 0, rectangle.Width, rectangle.Height);
		}

        #if SILVERLIGHT
        
        public static bool IntersectsWith(this Rect r1, Rect r2)
        {
            bool fIntersect =  ( r2.Left < r1.Right
                                && r2.Right > r1.Left
                                && r2.Top < r1.Bottom
                                && r2.Bottom > r1.Top
                                );
            return fIntersect;
        }
 
        #endif 

        public static Rect Offset(this Rect rectangle, double x, double y)
        {
            return new Rect(rectangle.X + x, rectangle.Y + y, rectangle.Width, rectangle.Height);
        }

		/// <summary>
		/// Snaps the point to rectangle (puts it on the the closest edge of the rectangle
		/// </summary>
		/// <param name="rectangle">The rectangle.</param>
		/// <param name="point">The point to be snapped.</param>
		/// <returns></returns>
		public static Point SnapPointToRectangle(this Rect rectangle, Point point)
		{
			return SnapPointToRectangle(rectangle, point, 0);
		}

		/// <summary>
		/// Snaps the point to tilted rectangle (puts it on the the closest edge of the rectangle
		/// </summary>
		/// <param name="rectangle">The rectangle.</param>
		/// <param name="point">The point to be snapped.</param>
		/// <param name="angle">The angle of <paramref name="rectangle"/> tilt in degrees.</param>
		/// <returns></returns>
		public static Point SnapPointToRectangle(this Rect rectangle, Point point, double angle)
		{
			Point result = point;
			Point c = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2); 

			if (angle != 0)
			{
				Point old = result;
				old.X -= c.X;
				old.Y -= c.Y;
				point.X = (old.X * Math.Cos(angle) - old.Y * Math.Sin(angle));
				point.Y = (old.Y * Math.Sin(angle) + old.X * Math.Cos(angle));
				point.X += c.X;
				point.Y += c.Y;

			}

		    if (rectangle.Contains(point))
			{
				// find closest edge 
				double[] dist = new double[4];
				dist[0] = result.X - rectangle.Left;
				dist[1] = rectangle.Right - result.X;
				dist[2] = result.Y - rectangle.Top;
				dist[3] = rectangle.Bottom - result.Y;

			    double min = dist.Min();
                if (dist[0] == min)
                {
                    result.X -= dist[0];
                }
                else if (dist[1] == min)
                {
                    result.X += dist[1];
                }
                else if (dist[2] == min)
                {
                    result.Y -= dist[2];
                }
                else if (dist[3] == min)
                {
                    result.Y += dist[3];
                }
			}
			else
			{
				if (result.Y <= rectangle.Top)
				{
				    result.Y = rectangle.Top;
				}
				else if (result.Y >= rectangle.Bottom)
				{
				    result.Y = rectangle.Bottom;
				}
			    if (result.X <= rectangle.Left)
			    {
			        result.X = rectangle.Left;
			    }
			    else if (result.X >= rectangle.Right)
			    {
			        result.X = rectangle.Right;
			    }
			}

			if (angle != 0)
			{
				Point old = result;
				old.X -= c.X;
				old.Y -= c.Y;
				angle -= Math.PI / 2;
				result.X = (old.X * Math.Cos(angle) - old.Y * Math.Sin(angle));
				result.Y = -(old.Y * Math.Sin(angle) + old.X * Math.Cos(angle));
				result.X += c.X;
				result.Y += c.Y;
			}

			return result;
		}

		/// <summary>
		/// Gets the center of the rectangle
		/// </summary>
		/// <param name="rectangle">The rectangle.</param>
		/// <returns></returns>
		public static Point GetCenter(this Rect rectangle)
		{
			return new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);

		}

        /// <summary>
        /// Aligns the labels to point in rectangle 
        /// so that their bounds don't intersect neither with the rectangle nor with each other
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="point">The point to which align to</param>
        /// <param name="labels">The labels the align</param>
        public static void AlignLabelsToPoint(this Rect rectangle, Point point, IEnumerable<Node> labels)
        {
            rectangle = rectangle.Normalize();
            ERectEdge position = rectangle.ReturnEdgesContainingPoint(rectangle.SnapPointToRectangle(point));

            double totalHeight = labels.Sum(item => item.ActualHeight);
            if ((position & ERectEdge.Left) == ERectEdge.Left)
            {
                double y = -totalHeight / 2;
                foreach (Node control in labels)
                {
                    control.X = -control.ActualWidth;
                    control.Y = y;
                    //control.ViewHelper.SetPositionSilent(control.X, control.Y);
                    y += control.ActualHeight;
                }
            }
            else if ((position & ERectEdge.Right) == ERectEdge.Right)
            {
                double y = -totalHeight / 2;
                foreach (Node control in labels)
                {
                    control.X = 0;
                    control.Y = y;
                    //control.ViewHelper.SetPositionSilent(control.X, control.Y);
                    y += control.ActualHeight;
                }
            }
            else if ((position & ERectEdge.Top) == ERectEdge.Top)
            {
                double y = -4;
                foreach (Node control in labels)
                {
                    y -= control.ActualHeight;
                    control.X = -40;
                    control.Y = y;
                    //control.ViewHelper.SetPositionSilent(control.X, control.Y);
                }
            }
            else if ((position & ERectEdge.Bottom) == ERectEdge.Bottom)
            {
                double y = 4;
                foreach (Node control in labels)
                {
                    control.X = -40;
                    control.Y = y;
                    //control.ViewHelper.SetPositionSilent(control.X, control.Y);
                    y += control.ActualHeight;
                }
            }
        }
		/// <summary>
		/// Returns the edges containing point.
		/// </summary>
		/// <param name="rectangle">The rectangle.</param>
		/// <param name="point">The point</param>
		/// <returns></returns>
		public static ERectEdge ReturnEdgesContainingPoint(this Rect rectangle, Point point)
		{
			ERectEdge result;
			if (!rectangle.Contains(point))
			{
				result = ERectEdge.Outside;
			}
			else
			{
				result = ERectEdge.Inside;
				if (point.X == rectangle.Left)
					result |= ERectEdge.Left;
				if (point.X == rectangle.Right)
					result |= ERectEdge.Right;
				if (point.Y == rectangle.Top)
					result |= ERectEdge.Top;
				if (point.Y == rectangle.Bottom)
					result |= ERectEdge.Bottom;
				if (result != ERectEdge.Inside)
				{
					result = (result & (~ERectEdge.Inside));
				}
			}

			return result;
		}
	}

	/// <summary>
	/// Possible placement of a point on a rectangle
	/// </summary>
	[Flags]
	public enum ERectEdge
	{
		/// <summary>
		/// outside the rectangle
		/// </summary>
		Outside = 0,
		/// <summary>
		/// inside the rectangle
		/// </summary>
		Inside = 1,
		/// <summary>
		/// on the top edge
		/// </summary>
		Top = 2,
		/// <summary>
		/// on the bottom edge
		/// </summary>
		Bottom = 4,
		/// <summary>
		/// on the left edge 
		/// </summary>
		Left = 8,
		/// <summary>
		/// on the right edge
		/// </summary>
		Right = 16 
	}
}