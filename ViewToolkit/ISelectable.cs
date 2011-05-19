using System;
using System.Windows;

namespace EvoX.ViewToolkit
{
    public interface ISelectable
    {
        Rect GetBounds();
        bool CanBeDraggedInGroup { get; }

        bool Selected { get; set; }
        bool Highlighted { get; set; }
        event Action SelectedChanged;
    }
}