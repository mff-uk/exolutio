using System.Windows.Controls.Primitives;

namespace Exolutio.ViewToolkit
{
    public static class WPFExtensions
    {
        public static bool IsDeltaEmpty(this DragDeltaEventArgs args)
        {
            return args.HorizontalChange == 0 && args.VerticalChange == 0;
        }
    }
}