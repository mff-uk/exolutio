namespace EvoX.ViewToolkit
{
    /// <summary>
    /// Type of placement on the canvas, determines the meaning of 
    /// X and Y coordinates and the way <see cref="DragThumb"/>.<see cref="DragThumb.CanvasPosition"/> is computed.
    /// </summary>
    public enum EPlacementKind
    {
        /// <summary>
        /// X and Y coordinates are absolute coordinates on the canvas. CanvasPosition directly returns X and Y.
        /// </summary>
        AbsoluteCanvas,
        /// <summary>
        /// Control is placed relatively to another control (ReferentialElement) on the canvas. 
        /// X and Y are offset from the referential control. 
        /// CanvasPosition returns coordinates of the referential control plus the offsets X and Y.
        /// </summary>
        RelativeCanvas,
        /// <summary>
        /// Control is placed on absolute coordinates on another control (ParentControl), X and Y are coordinates in the 
        /// coordinate system of the ParentControl. CanvasPosition returns coordinate of ParentControl plus the x and 
        /// y coordinates.
        /// </summary>
        AbsoluteSubCanvas,
        /// <summary>
        /// Similar to AbsoluteSubCanvas, but the position of the control is controlled automatically by the 
        /// parent control (auto position).
        /// </summary>
        ParentAutoPos
    }
}