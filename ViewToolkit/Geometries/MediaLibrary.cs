using System.Windows.Media;
using System.Windows.Media.Effects;

namespace XCase.View.Geometries
{
	/// <summary>
	/// Contains definitions of some graphical object shared by the controls.
	/// </summary>
	public class MediaLibrary
	{
		/// <summary>
		/// Pen to drow selected junctions
		/// </summary>
		public static Pen JunctionSelectedPen = new Pen(new SolidColorBrush(Colors.LightPink) { Opacity = 0.4 }, 5);

		/// <summary>
		/// Pen to draw not selected junction (is transparent => junction is wider than it appears and this enlarges the area
		/// that can be clicked)
		/// </summary>
		public static Pen JunctionTransparentPen = new Pen(Brushes.Transparent, 10);
		
		/// <summary>
		/// Black pen with width = 1
		/// </summary>
		public static Pen SolidBlackPen = new Pen(Brushes.Black, 1);
		
		/// <summary>
		/// Dashed black pen (less striking)
		/// </summary>
		public static Pen DashedBlackPen = new Pen(new SolidColorBrush(Colors.Black) { Opacity = 0.6 }, 1) { DashStyle = DashStyles.Dot };

		/// <summary>
		/// Pen used to draw selection rectangle
		/// </summary>
		public static Pen RubberbandPen = new Pen(new SolidColorBrush(Colors.Blue) { Opacity = 0.3 }, 0.5);
		
		/// <summary>
		/// Pink dropshadow effect
		/// </summary>
		public static Effect DropShadowEffect = new DropShadowEffect { BlurRadius = 12, Color = Colors.Pink, ShadowDepth = 0 };
		
		/// <summary>
		/// Effect used to highlight structural representative's represented class
		/// </summary>
		public static Effect RepresentedHighlight = new DropShadowEffect { Color = Colors.LightBlue, BlurRadius = 20, ShadowDepth = 0 };
		
		/// <summary>
		/// Effect used to highlight selected elements
		/// </summary>
		public static Effect SelectedHighlight = new DropShadowEffect { Color = Colors.Red, BlurRadius = 20, ShadowDepth = 0 };
		
		/// <summary>
		/// Hovered junction point opacity
		/// </summary>
		public const double PointOpacityHover = 0.5;
		
		/// <summary>
		/// Normal junction point opacity
		/// </summary>
		public const double PointOpacityNormal = 0.1;
	}
}