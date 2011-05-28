using System.Collections.Generic;
using System.Windows;

namespace Exolutio.ViewToolkit
{
    /// <summary>
    /// Interface for control to which <see cref="DragThumb"/> can be snapped.
    /// </summary>
    public interface IReferentialElement
    {
        /// <summary>
        /// Position of the element on canvas
        /// </summary>
        /// <value><see cref="Point"/></value>
        Point CanvasPosition { get; }
        /// <summary>
        /// List of controls that were snapped to this element
        /// </summary>
        IList<ISnappable> FellowTravellers { get; }
    }
}