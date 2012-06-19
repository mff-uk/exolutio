//#define showlabels

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using Exolutio.SupportingClasses;
using Exolutio.ViewToolkit.Geometries;

namespace Exolutio.ViewToolkit
{
#if SILVERLIGHT
    /// <summary>
    /// This cotrol represents one point in a junction. Points can be dragged.
    /// Junction is drawn as a polyline connecting its JunctionPoints. 
    /// </summary>
    public class ConnectorPoint : ContentControl, ISelectable, IDraggable, IReferentialElement
#else
    public class ConnectorPoint : Control, ISelectable, IDraggable, IReferentialElement
#endif
    {
        /// <summary>
        /// Connector where this point belongs to
        /// </summary>
        public Connector Connector { get; set; }

        public ExolutioCanvas ExolutioCanvas { get { return Connector != null ? Connector.ExolutioCanvas : null; } }

        /// <summary>
        /// Order of the point in <see cref="Connector"/>
        /// </summary>
        /// <value><see cref="Int32"/>, <code>0</code> for the first point in junction</value>
        public int OrderInConnector { get; set; }

#if showlabels
        /// <summary>
        /// Shows point position in a label, must be compiled with showlabels directive
        /// </summary>
        private SnappableLabel l;
#endif

        private readonly DragThumb dragThumb;

        DragThumb IDraggable.DragThumb
        {
            get { return dragThumb; }
        }

        public double X
        {
            get { return dragThumb.X; }
            set { dragThumb.X = value; }
        }

        public double Y
        {
            get { return dragThumb.Y; }
            set { dragThumb.Y = value; }
        }

        #if SILVERLIGHT
        private bool allowDisconnect = false;
        #else
        private bool allowDisconnect = false;
        #endif
        public bool AllowDisconnect
        {
            get { return allowDisconnect; }
            set { allowDisconnect = value; }
        }

        public bool Disconnected { get; set; }

        public Point Position
        {
            get
            {
                return new Point(X, Y);
            }
        }

        /// <summary>
        /// Placement defines how the CanvasPosition is computed.
        /// </summary>
        /// <value><see cref="EPlacementKind"/></value>
        public EPlacementKind Placement
        {
            get { return dragThumb.Placement; }
            internal set { dragThumb.Placement = value; }
        }

        /// <summary>
        /// Position of the element on canvas
        /// </summary>
        /// <value><see cref="Point"/></value>
        public Point CanvasPosition
        {
            get { return dragThumb.CanvasPosition; }
        }

        public bool IsInvisible { get; set; }

        public static implicit operator Point(ConnectorPoint p)
        {
            return p.Position;
        }

        public IList<ISnappable> FellowTravellers
        {
            get { return dragThumb.FellowTravellers; }
        }

        public void FellowTravellersUpdate()
        {
            dragThumb.FellowTravellersUpdate();
        }

        /// <summary>
        /// Node that owns this Point.
        /// </summary>
        internal Node ParentControl
        {
            get { return dragThumb.ParentControl; }
            set
            {
                dragThumb.ParentControl = value;
                if (value != null)
                {
                    dragThumb.ParentControl.PositionChanged += dragThumb_PositionChanged;
                }
            }
        }

        void dragThumb_PositionChanged(DragDeltaEventArgs dragDeltaEventArgs)
        {
            InvokePositionChanged(dragDeltaEventArgs);
        }

        public void ResetPoint()
        {
            Placement = EPlacementKind.ParentAutoPos;
            Connector.InvalidateGeometry();
        }

        /// <summary>
        /// Moves the point to specified location.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        public void SetPreferedPosition(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Moves the point to specified location. (Normal coordinates, not canvas global coordinates returned by CanvasPosition).
        /// </summary>
        /// <param name="preferedPosition">new location</param>
        public void SetPreferedPosition(Point preferedPosition)
        {
            SetPreferedPosition(preferedPosition.X, preferedPosition.Y);
        }

        public void SnapTo(Node node, bool recalcPosition)
        {
            this.dragThumb.SnapTo(node.DragThumb, recalcPosition);
        }

        public void AdjustDrag(ref DragDeltaEventArgs deltaEventArgs)
        {
            if (ParentControl != null)
            {
                #region disconnecting points

                Rect bounds = ParentControl.GetBounds();

                Point p = new Point(CanvasPosition.X + deltaEventArgs.HorizontalChange,
                                    CanvasPosition.Y + deltaEventArgs.VerticalChange);
                Point snapped = bounds.SnapPointToRectangle(p);

                if (AllowDisconnect && !Disconnected && dragThumb.DraggingAllone(this))
                {
                    Vector diff = Vector.SubtractPoints(ExolutioCanvas.GetMousePosition(), CanvasPosition);
                    if (diff.Length < 20)
                    {
                        deltaEventArgs = new DragDeltaEventArgs(snapped.X - CanvasPosition.X,
                                                                snapped.Y - CanvasPosition.Y);
                    }
                    else
                    {
                        DisconnectPoint();
                        deltaEventArgs = new DragDeltaEventArgs(0, 0);
                    }
                }
                else if (!Disconnected)
                {
                    deltaEventArgs = new DragDeltaEventArgs(snapped.X - CanvasPosition.X, snapped.Y - CanvasPosition.Y);
                }

                #endregion
            }
            else
            {
                #region snapping of break points

                if (!Disconnected && this.OrderInConnector > 0 && this.OrderInConnector < Connector.Points.Count - 1)
                {
                    ConnectorPoint rightNeighbour = Connector.Points.ElementAtOrDefault(OrderInConnector + 1);
                    ConnectorPoint leftNeighbour = Connector.Points.ElementAtOrDefault(OrderInConnector - 1);

                    const double SNAP_RATIO = 14;

                    foreach (ConnectorPoint neighbour in new[] {leftNeighbour, rightNeighbour})
                    {
                        bool isEndPoint = neighbour == Connector.StartPoint || neighbour == Connector.EndPoint;
                        if (neighbour != null &&
                            (!isEndPoint || neighbour.Placement == EPlacementKind.AbsoluteSubCanvas))
                        {
                            double nx = neighbour.CanvasPosition.X;
                            double ny = neighbour.CanvasPosition.Y;
                            double deltax = deltaEventArgs.HorizontalChange;
                            double deltay = deltaEventArgs.VerticalChange;
                            double diff;
                            if (ShouldSnap(dragThumb.MousePoint.Y, ny, SNAP_RATIO, out diff))
                            {
                                ShouldSnap(CanvasPosition.Y + deltaEventArgs.VerticalChange, ny, SNAP_RATIO, out diff);
                                deltay += diff;
                                deltaEventArgs = new DragDeltaEventArgs(deltax, deltay);
                            }
                            else if (ShouldSnap(dragThumb.MousePoint.X, nx, SNAP_RATIO, out diff))
                            {
                                ShouldSnap(CanvasPosition.X + deltaEventArgs.HorizontalChange, nx, SNAP_RATIO, out diff);
                                deltax += diff;
                                deltaEventArgs = new DragDeltaEventArgs(deltax, deltay);
                            }
                        }
                    }
                }

                #endregion
            }
        }

        protected static bool ShouldSnap(double point, double refPoint, double ratio, out double diff)
        {
            diff = refPoint - point;
            return Math.Abs(diff) < ratio;
        }

        private void DisconnectPoint()
        {
            this.Disconnected = true;
            Point mousePos = ExolutioCanvas.GetMousePosition();
            this.ParentControl.InnerConnectorControl.Children.Remove(this);
            this.ParentControl.Connectors.Remove(this.Connector);
            if (this.Connector.StartPoint == this)
            {
                this.Connector.StartNode = null;
            }
            if (this.Connector.EndPoint == this)
            {
                this.Connector.EndNode = null;
            }
            this.ParentControl = null;
            this.ExolutioCanvas.Children.Add(this);
            this.Placement = EPlacementKind.AbsoluteCanvas;
            this.dragThumb.Position = mousePos;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorPoint"/> class.
        /// </summary>
        /// <param name="exolutioCanvas">Canvas where the point is created.</param>
        public ConnectorPoint(ExolutioCanvas exolutioCanvas)
        {
            Width = 0;
            Height = 0;
            dragThumb = new DragThumb(exolutioCanvas, this);
            dragThumb.PositionChanged += InvokePositionChanged;
            dragThumb.FellowTravellersUpdated += AlignFellowTravellers;
            OrderInConnector = -1;
            Background = ViewToolkitResources.GoldBrush;

            Cursor = Cursors.Hand;
            
            this.SetValue(TemplateProperty, null);

#if SILVERLIGHT

            this.Width = 10;
            this.Height = 10;
            this.Content = new Ellipse() { Fill = Background };
            this.RenderTransform = new TranslateTransform() {X = -5, Y = -5};

#endif
#if SILVERLIGHT
#else
            this.PreviewMouseDown += ConnectorPoint_PreviewMouseDown;
#endif
        }

        void ConnectorPoint_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dragThumb.ExolutioCanvas.SelectableItem_PreviewMouseDown(this, e);
        }
        
        void AlignFellowTravellers()
        {
            if (NodesInBadPosition())
            {
                ParentControl.GetBounds().AlignLabelsToPoint(this.Position, FellowTravellers.OfType<DragThumb>().Select(t => t.DraggedControl).OfType<Node>());

                foreach (DragThumb fellowTraveller in FellowTravellers)
                {
                    DragThumb.UpdatePos(fellowTraveller);
                }
            }
        }

        private bool NodesInBadPosition()
        {
            if (FellowTravellers != null && FellowTravellers.Count > 0)
            {
                if (ParentControl != null)
                {
                    Rect elementBounds = ParentControl.GetBounds();
                    foreach (DragThumb fellowTraveller in FellowTravellers)
                    {
                        if (NodeInBadPosition(fellowTraveller.DraggedControl, elementBounds)) return true;
                    }
                }
            }
            return false;
        }

        private static bool NodeInBadPosition(Control node, Rect elementBounds)
        {
            const int rectangleCorrectionH = 3;
            const int rectangleCorrectionV = 6;

            Rect bounds = GeometryHelper.GetBasicControlBounds(node);

            bounds.X += rectangleCorrectionV;
            bounds.Y += rectangleCorrectionH;
            bounds.Width = Math.Max(bounds.Width - 2 * rectangleCorrectionV, 0);
            bounds.Height = Math.Max(bounds.Height - 2 * rectangleCorrectionH, 0);
            if (bounds.IntersectsWith(elementBounds))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ellipse radius (used in <see cref="OnRender"/>).
        /// </summary>
        private const double radius = 5;

#if SILVERLIGHT

#else

        /// <summary>
        /// Draws an ellipse around the point. 
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (!IsInvisible)
            {
                drawingContext.DrawEllipse(Background, null, new Point(0, 0), radius, radius);
            }
        }
#endif

        /// <summary>
        /// Invokes the PositionChanged event
        /// </summary>
        protected void InvokePositionChanged(DragDeltaEventArgs dragDeltaEventArgs)
        {
            //InvokePositionChanged();

            if (Connector != null)
            {
                Connector.InvokeConnectorPointMoved(this);
                Connector.InvalidateGeometry();
            }
        }



        #region mouse events redirected to drag thumb
#if SILVERLIGHT
        protected override void  OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            ConnectorPoint_PreviewMouseDown(this, e);
 	        base.OnMouseLeftButtonDown(e);

#else
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
#endif
            {
                dragThumb.OnMouseDown(e);
            }
        }

#if SILVERLIGHT
        protected override void  OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
 	        base.OnMouseLeftButtonUp(e);

#else
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
#endif
            //if (Draggable)
            {
                dragThumb.OnMouseUp(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            //if (Draggable)
            {
                dragThumb.OnMouseMove(e);
            }
        }

        #endregion

        #region Implementation of ISelectable

        public Rect GetBounds()
        {
            return GeometryHelper.GetBasicControlBounds(this);
        }



        public bool CanBeDraggedInGroup
        {
            get { return true; }
        }

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (selected)
                {
                    ExolutioCanvas.SelectedItems.AddIfNotContained(this);
                }
                else
                {
                    ExolutioCanvas.SelectedItems.Remove(this);
                }
                InvokeSelectedChanged();
            }
        }

        public event Action SelectedChanged;

        public void InvokeSelectedChanged()
        {
            Action handler = SelectedChanged;
            if (handler != null) handler();
        }


        public bool Highlighted { get; set; }

        #endregion

#if SILVERLIGHT
        public ContextMenu ContextMenu
        {
            get { return ContextMenuService.GetContextMenu(this); }
            set
            {
                ContextMenuService.SetContextMenu(this, value);
                if (this.Content != null)
                {
                    ContextMenuService.SetContextMenu((DependencyObject) this.Content, value);
                }
            }
        }
#endif

    }
}
