using System;
using System.Windows;

namespace Exolutio.ViewToolkit
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