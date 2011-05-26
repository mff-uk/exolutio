using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using EvoX.SupportingClasses;
using Vector = EvoX.ViewToolkit.Vector;

namespace EvoX.ViewToolkit
{
    internal class DragThumb: ISnappable, IReferentialElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DragThumb"/> class.
        /// </summary>
        /// <param name="evoXCanvas"> where the control is created</param>
        /// <param name="draggedControl">controll actually dragged</param>
        public DragThumb(EvoXCanvas evoXCanvas, Control draggedControl)
        {
            EvoXCanvas = evoXCanvas;
            DraggedControl = draggedControl;
            //Placement = EPlacementKind.AbsoluteCanvas;
        }

        /// <summary>
        /// Canvas where the control is placed
        /// </summary>
        /// <value><see cref="EvoXCanvas"/></value>
        public EvoX.ViewToolkit.EvoXCanvas EvoXCanvas { get; set; }

        private Control draggedControl;
        /// <summary>
        /// Control must implement IDraggable
        /// </summary>
        public Control DraggedControl
        {
            get { return draggedControl; }
            set
            {
                if (!(value is IDraggable))
                {
                    throw new ArgumentException("DraggedControl must implement IDraggable. ");
                }
                draggedControl = value;
            }
        }

        private Node parentControl;

        private Point parentLastPos = new Point();

        private void parentControl_LayoutUpdated(object sender, EventArgs e)
        {
            Point parentPos = new Point(Canvas.GetLeft(ParentControl), Canvas.GetTop(ParentControl));
            if (parentPos != parentLastPos)
            {
                InvokePositionChanged();
                FellowTravellersUpdate();
                parentLastPos = parentPos;
            }
        }

        /// <summary>
        /// Control that owns this DraggedControl (see <see cref="EPlacementKind.AbsoluteSubCanvas"/> 
        /// and <see cref="EPlacementKind.ParentAutoPos"/>)
        /// </summary>
        /// <value><see cref="Control"/></value>
        internal Node ParentControl
        {
            get { return parentControl; }
            set
            {
                if (parentControl != null)
                {
                    parentControl.LayoutUpdated -= parentControl_LayoutUpdated;
                }
                parentControl = value;
                if (parentControl != null)
                {
                    parentControl.LayoutUpdated += parentControl_LayoutUpdated;
                }
                UpdatePos(this);
            }
        }

        private EPlacementKind placement;

        /// <summary>
        /// Placement defines how the CanvasPosition is computed.
        /// </summary>
        /// <value><see cref="EPlacementKind"/></value>
        public EPlacementKind Placement
        {
            get { return placement; }
            internal set { placement = value; }
        }

        private EPlacementCenter placementCenter = EPlacementCenter.TopLeftCorner;
        public EPlacementCenter PlacementCenter
        {
            get { return placementCenter; }
            set { placementCenter = value; }
        }

        private bool movable = true;

        /// <summary>
        /// Tells whether this element can be moved or not (PSM/PIM Diagram difference).
        /// Default value is true.
        /// </summary>
        public bool Movable
        {
            get
            {
                return movable;
            }
            set
            {
                movable = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this control can be dragged when selected in group.
        /// If false, the control can be dragged only when it is the only control selected
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can be dragged in group; otherwise, <c>false</c>.
        /// </value>
        public bool CanBeDraggedInGroup
        {
            get
            {
                return (Placement == EPlacementKind.AbsoluteCanvas);
            }
        }

        private bool IsDragging { get; set; }

        private double x;

        /// <summary>
        /// X coordinate of the control
        /// </summary>
        /// <value><see cref="Double"/></value>
        public double X
        {
            get { return x; }
            set
            {
                if (value != x || Double.IsNaN(Left))
                {
                    x = value;
                    UpdatePos(this);
                }
            }
        }

        private double y;

        /// <summary>
        /// Y coordinate of the control
        /// </summary>
        public double Y
        {
            get { return y; }
            set
            {
                if (value != y || Double.IsNaN(Top))
                {
                    y = value;
                    UpdatePos(this);
                }
            }
        }

        /// <summary>
        /// Position of the control (<see cref="X"/> and <see cref="Y"/> joined)
        /// </summary>
        public Point Position
        {
            get
            {
                return new Point(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        #region top left bottom right

        /// <summary>
        /// Gets the y coordiante for the top of the control
        /// </summary>
        /// <value><see cref="Double"/></value>
        public double Top
        {
            get { return Canvas.GetTop(DraggedControl); }
            //set { Canvas.SetTop(this, value); }
        }

        /// <summary>
        /// Gets the x coordiante for the left of the control
        /// </summary>
        /// <value><see cref="Double"/></value>
        public double Left
        {
            get { return Canvas.GetLeft(DraggedControl); }
            //set { Canvas.SetLeft(this, value); }
        }

        /// <summary>
        /// Gets the y coordiante for the bottom of the control
        /// </summary>
        /// <value><see cref="Double"/></value>
        public double Bottom
        {
            get { return Canvas.GetTop(DraggedControl) + DraggedControl.ActualHeight; }
            //set { Canvas.SetTop(this, value - this.ActualHeight); }
        }

        /// <summary>
        /// Gets the x coordiante for the right of the control
        /// </summary>
        /// <value><see cref="Double"/></value>
        public double Right
        {
            get { return Canvas.GetLeft(DraggedControl) + DraggedControl.ActualWidth; }
            //set { Canvas.SetLeft(this, value - this.ActualWidth); }
        }
        #endregion

        /// <summary>
        /// Occurs when position of the control changed.
        /// </summary>
        public event Action PositionChanged;

        public event Action Dropped;

        /// <summary>
        /// Invokes the PositionChanged event
        /// </summary>
        private void InvokePositionChanged()
        {
            Action positionChangedAction = PositionChanged;
            if (positionChangedAction != null) positionChangedAction();
        }

        /// <summary>
        /// Updates the x and y coordinates of an element
        /// </summary>
        /// <param name="element">The element whose position is updated</param>
        internal static void UpdatePos(ISnappable element)
        {
            DragThumb thumb = (DragThumb) element;
            double _x = Canvas.GetLeft(thumb.DraggedControl);
            double _y = Canvas.GetTop(thumb.DraggedControl);

            double placementModifierX = 0;
            double placementModifierY = 0;

            if (thumb.PlacementCenter == EPlacementCenter.Center)
            {
                placementModifierX = -thumb.DraggedControl.ActualWidth/2;
                placementModifierY = -thumb.DraggedControl.ActualHeight/2;
            }

            Point idealPosition = new Point();

            switch (thumb.Placement)
            {
                case EPlacementKind.AbsoluteCanvas:
                case EPlacementKind.ParentAutoPos:
                case EPlacementKind.AbsoluteSubCanvas:
                    idealPosition.X = thumb.X + placementModifierX;
                    idealPosition.Y = thumb.Y + placementModifierY;
                    break;
                case EPlacementKind.RelativeCanvas:
                    if (thumb.ReferentialElement != null)
                    {
                        idealPosition.X = thumb.X + thumb.ReferentialElement.CanvasPosition.X + placementModifierX;
                        idealPosition.Y = thumb.Y + thumb.ReferentialElement.CanvasPosition.Y + placementModifierY;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Canvas.SetLeft(thumb.DraggedControl, idealPosition.X);
            Canvas.SetTop(thumb.DraggedControl, idealPosition.Y);

            if (_x != Canvas.GetLeft(thumb.DraggedControl) || _y != Canvas.GetTop(thumb.DraggedControl))
            {
                ((DragThumb) element).InvokePositionChanged();
                ((DragThumb) element).FellowTravellersUpdate();
            }

            if (thumb.DraggedControl is Node)
            {
                ((Node)thumb.DraggedControl).SetPositionSilent(thumb.X, thumb.Y);
            }
        }

        /// <summary>
        /// Position in the coordinate system of the <see cref="EvoXCanvas"/>.
        /// The value depends on <see cref="Placement"/>.
        /// </summary>
        /// <seealso cref="Placement"/>
        /// <seealso cref="EPlacementKind"/>
        /// <value><see cref="Point"/></value>
        public Point CanvasPosition
        {
            get
            {
                switch (Placement)
                {
                    case EPlacementKind.AbsoluteCanvas:
                        return Position;
                    case EPlacementKind.RelativeCanvas:
                        return new Point(ReferentialElement.CanvasPosition.X + X, ReferentialElement.CanvasPosition.Y + Y);
                    case EPlacementKind.AbsoluteSubCanvas:
                    case EPlacementKind.ParentAutoPos:
                        if (ParentControl == null)
                        {
                            throw new Exception("Placement is set to ParentAutoPos or AbsoluteSubCanvas but ParentControl is null. ");
                        }
                        return new Point(Canvas.GetLeft(ParentControl) + X, Canvas.GetTop(ParentControl) + Y);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private readonly IList<ISnappable> fellowTravellers = new List<ISnappable>();
        public IList<ISnappable> FellowTravellers { get { return fellowTravellers; } }

        /// <summary>
        /// Updates positiosn of <see cref="FellowTravellers"/>
        /// </summary>
        private void FellowTravellersUpdate()
        {
            if (FellowTravellers != null)
            {
                foreach (ISnappable element in FellowTravellers)
                {
                    UpdatePos(element);
                    if (element is DragThumb)
                        ((DragThumb)element).FellowTravellersUpdate();
                }
                if (FellowTravellersUpdated != null)
                    FellowTravellersUpdated();
            }
        }

        public event Action FellowTravellersUpdated;

        /// <summary>
        /// Returns bounding rectangle of the control
        /// </summary>
        /// <returns></returns>
        public Rect GetBounds()
        {
            Rect r1 = new Rect
            {
                Y = Math.Round(Canvas.GetTop(DraggedControl)),
                X = Math.Round(Canvas.GetLeft(DraggedControl)),
                Height = Math.Round(DraggedControl.ActualHeight),
                Width = Math.Round(DraggedControl.ActualWidth)
            };

            return r1;
        }

        private Point DragStartPoint { get; set; }

        private Point PrevPoint { get; set; }

        /// <summary>
        /// Position of the mouse cursor during drag.
        /// </summary>
        internal Point MousePoint { get; private set; }

        /// <summary>
        /// Initializes dragging
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. This event data reports details about the mouse button that was pressed and the handled state.</param>
        public void OnMouseDown(MouseButtonEventArgs e)
        {
            DraggedControl.Focus();
            #if SILVERLIGHT
            #else
            if (e.ChangedButton == MouseButton.Left && Movable && e.ClickCount != 2)
            #endif
            {
                DragStartPoint = e.GetPosition(EvoXCanvas);
                PrevPoint = DragStartPoint;
                MousePoint = new Point(e.GetPosition(EvoXCanvas).X - e.GetPosition(DraggedControl).X,
                                       e.GetPosition(EvoXCanvas).Y - e.GetPosition(DraggedControl).Y);
                IsDragging = true;
                DraggedControl.CaptureMouse();
                e.Handled = true;
                DragStarted();
            }
        }

        /// <summary>
        /// Proceeds with dragging.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        public void OnMouseMove(MouseEventArgs e)
        {
            if (IsDragging && Movable)
            {
                Point newPoint = e.GetPosition(EvoXCanvas);
                Vector delta = Vector.SubtractPoints(newPoint,PrevPoint);
                PrevPoint = newPoint;
                MousePoint = Vector.AddVector(MousePoint, delta);
                this.DragDelta(new DragDeltaEventArgs(delta.X, delta.Y));
            }
        }

        /// <summary>
        /// Ends dragging
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the mouse button was released.</param>
        public void OnMouseUp(MouseButtonEventArgs e)
        {
            #if SILVERLIGHT
            #else
            if (e.ChangedButton == MouseButton.Left)
            #endif
            {
                IsDragging = false;
                DraggedControl.ReleaseMouseCapture();
                //e.Handled = true;
                DragCompleted(PrevPoint, Vector.SubtractPoints(PrevPoint, DragStartPoint));
            }
        }

        /// <summary>
        /// Cancels dragging.
        /// </summary>
        public void CancelDrag()
        {
            IsDragging = false;
            DraggedControl.ReleaseMouseCapture();
        }

        /// <summary>
        /// Returns true if the control can be dragged when it 
        /// is the only control selected. Default is true, 
        /// can be overriden in subclasses. 
        /// </summary>
        public bool AllowDragIfSelectedAlone
        {
            get { return true; }
        }

        /// <summary>
        /// Returns true when the control is the only control dragged.
        /// </summary>
        /// <param name="item">item.</param>
        /// <returns>Returns true when the control is the only control dragged.</returns>
        private bool DraggingAllone(ISelectable item)
        {
            return (EvoXCanvas.SelectedItems.Count <= 1 && (item is IDraggable) && (item as IDraggable).DragThumb.AllowDragIfSelectedAlone);
        }

        private Dictionary<DragThumb, rPoint> startPositions;

        public bool IsDragged { get; private set; }

        /// <summary>
        /// Called when dragging starts, stores old positions of the dragged element
        /// </summary>
        private void DragStarted()
        {
            startPositions = new Dictionary<DragThumb, rPoint>();
            IsDragged = true; 

            if (!(this is ISelectable) ||
                !EvoXCanvas.SelectedItems.Contains(this as ISelectable))
            {
                startPositions.Add(this, new rPoint(this.Left, this.Top));
            }
            else
            {
                foreach (ISelectable item in EvoXCanvas.SelectedItems)
                {
                    if (item.CanBeDraggedInGroup || DraggingAllone(item))
                    {
                        DragThumb thumb = item as DragThumb;
                        if (thumb != null)
                        {
                            startPositions.Add(thumb, new rPoint(thumb.Position) { tag = thumb.Placement });
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                string.Format("Dragging is implemented only for DragThumb and descendants. Object {0} of type {1} does not derive from DrugThumb. ", item, item.GetType()));
                        }
                    }
                }
            }

            //if (this is IAlignable)
            //{
            //    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(EvoXCanvas);
            //    visualAidsAdorner = new VisualAidsAdorner(EvoXCanvas, this);
            //    adornerLayer.Add(visualAidsAdorner);
            //}

        }

        /// <summary>
        /// Called during dragging. Checks whether dragging can proceed (not out of bounds) and 
        /// also calls method <see cref="AdjustDrag"/> that allows subclasses of DragThumb
        /// to alter default dragging.
        /// </summary>
        /// <param name="deltaEventArgs">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        private void DragDelta(DragDeltaEventArgs deltaEventArgs)
        {
            IEnumerable<IDraggable> draggedElements = EvoXCanvas.SelectedItems.Where
                (item => item is IDraggable && (item.CanBeDraggedInGroup 
                    || DraggingAllone(item))).Cast<IDraggable>();
            if (draggedElements.Count() > 0)
            {
                IsDragged = true; 
                double minLeft = draggedElements.Min(item => item.GetBounds().Left);
                double minTop = draggedElements.Min(item => item.GetBounds().Top);

                double deltaHorizontal = Math.Max(-minLeft, deltaEventArgs.HorizontalChange);
                double deltaVertical = Math.Max(-minTop, deltaEventArgs.VerticalChange);
                deltaEventArgs = new DragDeltaEventArgs(deltaHorizontal, deltaVertical);

                ((IDraggable)DraggedControl).AdjustDrag(ref deltaEventArgs);

                foreach (IDraggable draggedElement in draggedElements)
                {
                    DragThumb dragThumb = draggedElement.DragThumb;
                    dragThumb.x = dragThumb.x + deltaEventArgs.HorizontalChange;
                    dragThumb.y = dragThumb.y + deltaEventArgs.VerticalChange;
                    if (dragThumb.Placement == EPlacementKind.ParentAutoPos)
                        dragThumb.Placement = EPlacementKind.AbsoluteSubCanvas;
                    UpdatePos(dragThumb);
                }
                InvokePositionChanged();
                EvoXCanvas.InvalidateMeasure();
                //if (visualAidsAdorner != null)
                //{
                //    visualAidsAdorner.d = deltaEventArgs;
                //    visualAidsAdorner.InvalidateVisual();
                //}
            }
        }

        /// <summary>
        /// Drags the completed.
        /// </summary>
        /// <param name="finalPoint">The final point.</param>
        /// <param name="totalShift">The total shift.</param>
        private void DragCompleted(Point finalPoint, Vector totalShift)
        {
            IsDragged = false;
            //if (visualAidsAdorner != null)
            //{
            //    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(EvoXCanvas);
            //    adornerLayer.Remove(visualAidsAdorner);
            //    visualAidsAdorner = null;
            //}


            if (DragStartPoint != finalPoint)
            {
                
                //DiagramController controller = EvoXCanvas.Controller;

                //MacroCommand<DiagramController> moveMacroCommand =
                //    MacroCommandFactory<DiagramController>.Factory().Create(controller);
                //moveMacroCommand.Description = CommandDescription.MOVE_MACRO;

                //JunctionPointCommand.PointMoveDataDictionary pointMoveDataCollection = null;

                //foreach (KeyValuePair<DragThumb, rPoint> pair in startPositions)
                //{
                //    if (pair.Key is IAlignable)
                //    {
                //        IAlignable element = (IAlignable)pair.Key;

                //        DragThumb dragThumb = element as DragThumb;

                //        double _x;
                //        double _y;
                //        if (dragThumb != null && dragThumb.Placement == EPlacementKind.RelativeCanvas)
                //        {
                //            _x = dragThumb.Left - dragThumb.ReferentialElement.CanvasPosition.X;
                //            _y = dragThumb.Top - dragThumb.ReferentialElement.CanvasPosition.Y;
                //        }
                //        else
                //        {
                //            _x = element.Left;
                //            _y = element.Top;
                //        }

                //        CommandBase command = ViewController.CreateMoveCommand(
                //                _x,
                //                _y,
                //                element.ViewHelper,
                //                controller);
                //        moveMacroCommand.Commands.Add(command);
                //    }
                //    else if (pair.Key is JunctionPoint)
                //    {
                //        JunctionPoint junctionPoint = (JunctionPoint)pair.Key;

                //        JunctionPointCommand.PointMoveData data = new JunctionPointCommand.PointMoveData
                //        {
                //            Index = junctionPoint.OrderInConnector,
                //            OldPosition = pair.Value,
                //            NewPosition = new rPoint(junctionPoint.Position) { tag = junctionPoint.Placement },
                //        };

                //        if (pointMoveDataCollection == null)
                //            pointMoveDataCollection = new JunctionPointCommand.PointMoveDataDictionary();

                //        if (!pointMoveDataCollection.ContainsKey(junctionPoint.Connector.viewHelperPointsCollection))
                //        {
                //            pointMoveDataCollection[junctionPoint.Connector.viewHelperPointsCollection] = new List<JunctionPointCommand.PointMoveData>();
                //        }
                //        pointMoveDataCollection[junctionPoint.Connector.viewHelperPointsCollection].Add(data);
                //    }
                //}

                //// add one command for each affected junction 
                //if (pointMoveDataCollection != null)
                //{
                //    JunctionPointCommand junctionPointCommand = (JunctionPointCommand)JunctionPointCommandFactory.Factory().Create(controller);
                //    junctionPointCommand.Action = JunctionPointCommand.EJunctionPointAction.MovePoints;
                //    junctionPointCommand.PointMoveDataCollection = pointMoveDataCollection;
                //    junctionPointCommand.Description = CommandDescription.MOVE_JUNCTION_POINTS;
                //    moveMacroCommand.Commands.Add(junctionPointCommand);
                //}

                //moveMacroCommand.Execute();

                if (Dropped != null)
                    Dropped();
            }
        }

        

        #region snapy - zatim neimplementovano
        /// <summary>
        /// AdjustDrag is called when DragThumb is dragged via mouse upon its canvas. Inheriting classes are free
        /// to modify <paramref name="deltaEventArgs"/> and so alter the position of the DragThumb after drag. Override 
        /// this method to implement snapping, aligning etc.
        /// </summary>
        /// <example>
        /// <see cref="ConnectorPoint"/> overrides AdjustDrag to make it snapped to the borders of its provider. 
        /// </example>
        /// <param name="deltaEventArgs"></param>
        private void AdjustDrag(ref DragDeltaEventArgs deltaEventArgs)
        {
            //if (this is IAlignable && visualAidsAdorner != null && visualAidsAdorner.adjustmentToSnap != null)
            //{
            //    Vector adj = visualAidsAdorner.adjustmentToSnap.Value;
            //    //deltaEventArgs = new DragDeltaEventArgs(adj.X, adj.Y);
            //    if (Math.Abs(adj.X) > VisualAidsAdorner.ADJUSTMENT_RATIO || Math.Abs(adj.Y) > VisualAidsAdorner.ADJUSTMENT_RATIO)
            //    {
            //        visualAidsAdorner.InvalidateVisual();
            //    }
            //    else
            //    {
            //        deltaEventArgs = new DragDeltaEventArgs(adj.X, adj.Y);
            //    }
            //}
        }

        #endregion 

        #region Implementation of ISnappable

        /// <summary>
        /// Snaps this control to <paramref name="referentialElement"/>. 
        /// </summary>
        /// <param name="referentialElement">the control that this element is snapped to</param>
        /// <param name="recalcPosition">if set to <c>true</c> offset is recalculated from current position.</param>
        public void SnapTo(IReferentialElement referentialElement, bool recalcPosition)
        {
            ReferentialElement = referentialElement;
            referentialElement.FellowTravellers.Add(this);
            Placement = EPlacementKind.RelativeCanvas;
            UIElement _uie = referentialElement as UIElement;
            if (recalcPosition && _uie != null)
            {
                this.x = Canvas.GetLeft(this.DraggedControl) - Canvas.GetLeft(_uie);
                this.x = !double.IsNaN(x) ? x : 0;
                this.y = Canvas.GetTop(this.DraggedControl) - Canvas.GetTop(_uie);
                this.y = !double.IsNaN(y) ? y : 0;
            }
            UpdatePos(this);
        }

        /// <summary>
        /// Unsnaps the element from its referential element
        /// </summary>
        /// <param name="element">The element which is unsnapped</param>
        public static void UnsnapElement(ISnappable element)
        {
            element.ReferentialElement.FellowTravellers.Remove(element);
            element.ReferentialElement = null;
            if (element is DragThumb)
            {
                ((DragThumb)element).Placement = EPlacementKind.AbsoluteCanvas;
                UpdatePos(element);
            }
        }

        private IReferentialElement referentialElement;

        /// <summary>
        /// Control that the element is snapped to.
        /// </summary>
        /// <value>control implementing <see cref="IReferentialElement"/></value>
        public IReferentialElement ReferentialElement
        {
            get { return referentialElement; }
            set { referentialElement = value; UpdatePos(this); }
        }

        /// <summary>
        /// X coordinate of the element in the coordinate system of <see cref="ReferentialElement"/>/
        /// </summary>
        /// <value><see cref="Double"/></value>
        public double SnapOffsetX
        {
            get { return X; }
            set { X = value; }
        }

        /// <summary>
        /// Y coordinate of the element in the coordinate system of <see cref="ReferentialElement"/>/
        /// </summary>
        /// <value><see cref="Double"/></value>
        public double SnapOffsetY
        {
            get { return Y; }
            set { Y = value; }
        }

        #endregion

        /// <summary>
        /// Returns the string representation of a <see cref="T:System.Windows.Controls.Control"/> object.
        /// </summary>
        /// <returns>A string that represents the control.</returns>
        public override string ToString()
        {
            return String.Format("DT: rel: [{0:0},{1:0}] abs [{2:0},{3:0}]", X, Y, CanvasPosition.X, CanvasPosition.Y);
        }

    }
}