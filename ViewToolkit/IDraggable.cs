using System.Windows;
using System.Windows.Controls.Primitives;

namespace EvoX.ViewToolkit
{
    internal interface IDraggable
    {
        DragThumb DragThumb { get; }
        Rect GetBounds();

        void AdjustDrag(ref DragDeltaEventArgs deltaEventArgs);

        double X { get; set; }

        double Y { get; set; }
    }
}