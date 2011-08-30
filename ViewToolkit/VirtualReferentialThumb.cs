using System.Collections.Generic;
using System.Windows;

namespace Exolutio.ViewToolkit
{
    /// <summary>
    /// Implementation of <see cref="IReferentialElement"/> that as no
    /// visualization and its "position" can be controlled from 
    /// outside. This can be used for non-standard 
    /// scenarios of snapping.
    /// </summary>
    public class VirtualReferentialThumb : IReferentialElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualReferentialThumb"/> class.
        /// </summary>
        public VirtualReferentialThumb()
        {
            FellowTravellers = new List<ISnappable>();
        }

        private Point canvasPosition;

        /// <summary>
        /// Position of the element on canvas (can be set freely from 
        /// outside, otherwise remains constant)
        /// </summary>
        /// <value><see cref="Point"/></value>
        public Point CanvasPosition
        {
            get { return canvasPosition; }
            set { canvasPosition = value;
                FellowTravellersUpdate();
            }
        }

        /// <summary>
        /// Updates positiosn of <see cref="FellowTravellers"/>
        /// </summary>
        public void FellowTravellersUpdate()
        {
            if (FellowTravellers != null)
            {
                foreach (DragThumb element in FellowTravellers)
                {
                    DragThumb.UpdatePos(element);
                }
            }
        }

        /// <summary>
        /// List of controls that were snapped to this element
        /// </summary>
        /// <value></value>
        public IList<ISnappable> FellowTravellers
        {
            get;
            private set;
        }
    }
}