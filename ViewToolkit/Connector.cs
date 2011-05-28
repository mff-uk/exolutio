using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Exolutio.SupportingClasses;
using Exolutio.ViewToolkit.Geometries;

namespace Exolutio.ViewToolkit
{
    public class Connector : Control, ISelectable
    {
        public Connector()
        {
#if SILVERLIGHT

#else 
            Pen = ViewToolkitResources.SolidBlackPen;
#endif
            Fill = ViewToolkitResources.BlackBrush;

            VirtualReferentialThumb = new VirtualReferentialThumb();
#if SILVERLIGHT
#else
            PreviewMouseDown += Connector_PreviewMouseDown;            
#endif
        }

        /// <summar>y
        /// Fill for the junction cap (black is used for diamonds, white for triangle arrows..)
        /// </summary>
        private Brush Fill;

        #region Implementation of ISelectable

        public Rect GetBounds()
        {
            return RectExtensions.GetEncompassingRectangle(Points.Select(p => p.CanvasPosition));
        }

        public bool CanBeDraggedInGroup
        {
            get { throw new NotImplementedException(); }
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
                InvalidateVisual();
                InvokeSelectedChanged();
            }
        }

        public event Action SelectedChanged;

        public void InvokeSelectedChanged()
        {
            Action handler = SelectedChanged;
            if (handler != null) handler();
        }


        private bool highlighted;
        public bool Highlighted
        {
            get { return highlighted; }
            set { highlighted = value; }
        }

        void Connector_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ExolutioCanvas.SelectableItem_PreviewMouseDown(this, e);
        }
        
        #endregion

        private ExolutioCanvas exolutioCanvas;
        public ExolutioCanvas ExolutioCanvas
        {
            get { return exolutioCanvas; }
            set { exolutioCanvas = value; }
        }

        private Node startNode;

        private Node endNode;

        public Node StartNode
        {
            get { return startNode; }
            private set { startNode = value; }
        }

        public Node EndNode
        {
            get { return endNode; }
            private set { endNode = value; }
        }

        public bool AutoPosModeOnly { get; set; }

        private Geometry geometry;

        private bool sourceMeasureValid = true;

        private bool targetMeasureValid = true;

        public void SetPoints(ObservablePointCollection points)
        {
            for (int index = 0; index < points.Count; index++)
            {
                this.Points[index].Placement = EPlacementKind.AbsoluteSubCanvas;
                rPoint rPoint = points[index];
                this.Points[index].SetPreferedPosition(rPoint);
            }
            for (int index = 0; index < points.Count; index++)
            {
                this.Points[index].Placement = EPlacementKind.AbsoluteSubCanvas;
            }
            AdjustEndPoints();
        }

        public void InvalidateGeometry()
        {
            bool valid = AdjustEndPoints();
            if (!valid)
                return;

            PathGeometry path = new PathGeometry();

            PathFigure startFigure = null;
            PathFigure endFigure = null;
            Point startPointShifted = StartPoint.CanvasPosition;
            Point endPointShifted = EndPoint.CanvasPosition;

            //#region define startFigure

            //if (StartCapStyle != EJunctionCapStyle.Straight)
            //{
            //    Point start = StartPoint.CanvasPosition;
            //    Point end = Points[1].CanvasPosition;

            //    startFigure = new PathFigure { StartPoint = start };
            //    PolyLineSegment seg = new PolyLineSegment();
            //    seg.Points.Add(end);
            //    startFigure.Segments.Add(seg);

            //    switch (StartCapStyle)
            //    {
            //        case EJunctionCapStyle.FullArrow:
            //        case EJunctionCapStyle.Arrow:
            //        case EJunctionCapStyle.Triangle:
            //            Point dummy = new Point();
            //            if (StartCapStyle == EJunctionCapStyle.Arrow)
            //                startFigure = JunctionGeometryHelper.CalculateArrow(startFigure, end, start,
            //                                                                    StartCapStyle != EJunctionCapStyle.Arrow, ref dummy);
            //            else
            //                startFigure = JunctionGeometryHelper.CalculateArrow(startFigure, end, start,
            //                                                                    StartCapStyle != EJunctionCapStyle.Arrow,
            //                                                                    ref startPointShifted);
            //            break;
            //        case EJunctionCapStyle.FullDiamond:
            //        case EJunctionCapStyle.Diamond:
            //            startFigure = JunctionGeometryHelper.CalculateDiamond(startFigure, end, start, ref startPointShifted);
            //            break;
            //    }
            //}

            //#endregion

            //#region define endFigure

            //if (EndCapStyle != EJunctionCapStyle.Straight)
            //{
            //    endFigure = new PathFigure();
            //    Point start = EndPoint.CanvasPosition;
            //    Point end = Points[Points.Count - 2].CanvasPosition;

            //    PolyLineSegment seg = new PolyLineSegment();
            //    seg.Points.Add(end);
            //    endFigure.Segments.Add(seg);
            //    switch (EndCapStyle)
            //    {
            //        case EJunctionCapStyle.Arrow:
            //        case EJunctionCapStyle.FullArrow:
            //        case EJunctionCapStyle.Triangle:
            //            Point dummy = new Point();
            //            if (StartCapStyle == EJunctionCapStyle.Arrow)
            //                endFigure = JunctionGeometryHelper.CalculateArrow(endFigure, end, start, EndCapStyle != EJunctionCapStyle.Arrow,
            //                                                                  ref dummy);
            //            else
            //                endFigure = JunctionGeometryHelper.CalculateArrow(endFigure, end, start, EndCapStyle != EJunctionCapStyle.Arrow,
            //                                                                  ref endPointShifted);
            //            break;
            //        case EJunctionCapStyle.Diamond:
            //        case EJunctionCapStyle.FullDiamond:
            //            endFigure = JunctionGeometryHelper.CalculateDiamond(endFigure, end, start, ref endPointShifted);
            //            break;
            //    }
            //}

            //#endregion

            #region create junctionFigure

            PathFigure junctionFigure = new PathFigure { StartPoint = startPointShifted };
            PolyLineSegment segment = new PolyLineSegment();
            for (int i = 1; i < Points.Count - 1; i++)
            {
                segment.Points.Add(Points[i].CanvasPosition);
            }
            segment.Points.Add(endPointShifted);
#if SILVERLIGHT
#else 
            segment.IsSmoothJoin = true;
#endif
            junctionFigure.Segments.Add(segment);
            junctionFigure.IsFilled = false;

            #endregion

            if (startFigure != null) path.Figures.Add(startFigure);
            if (endFigure != null) path.Figures.Add(endFigure);

            path.FillRule = FillRule.Nonzero;
            path.Figures.Add(junctionFigure);

            geometry = path;
            InvalidateVisual();

            VirtualReferentialThumb.CanvasPosition = GetVirtualCenterPosition();

            //if (Association != null)
            //{
            //    Association.UpdateNameLabelPosition();
            //}
        }

        public Point FindClosestPoint(Point point)
        {
            //if (simpleAssociationJunction != null)
            //    return JunctionGeometryHelper.FindClosestPoint(simpleAssociationJunction, point);
            //else if (Diamond != null)
            //    return Diamond.GetBounds().GetCenter();
            //else
            return GetBounds().GetCenter();
        }

        private Point GetVirtualCenterPosition()
        {
            Point p = FindClosestPoint(
                new Point((StartPoint.CanvasPosition.X + EndPoint.CanvasPosition.X) / 2,
                          (StartPoint.CanvasPosition.Y + EndPoint.CanvasPosition.Y) / 2));
            return p;
            //if (nameLabel == null)
            //{
            //    return null;
            //}

            //if (!ViewHelper.UseDiamond)
            //{
            //    Point p = FindClosestPoint(
            //        new Point((simpleAssociationJunction.StartPoint.CanvasPosition.X + simpleAssociationJunction.AssociationEnd.CanvasPosition.X) / 2,
            //                  (simpleAssociationJunction.StartPoint.CanvasPosition.Y + simpleAssociationJunction.AssociationEnd.CanvasPosition.Y) / 2));

            //    return new Point(p.X - nameLabel.ActualWidth / 2, p.Y - nameLabel.ActualHeight / 2);
            //}
            //else
            //{
            //    Diamond.UpdateLayout();
            //    return new Point(Diamond.Left - nameLabel.ActualWidth / 2,
            //                     Diamond.Top - nameLabel.ActualHeight - 5);
            //}
        }

#if SILVERLIGHT

        List<Line> lines = new List<Line>();

        public void InvalidateVisual()
        {
            bool reset = lines.Count != Points.Count - 1;
            if (reset)
            {
                foreach (Line line in lines)
                {
                    ExolutioCanvas.Children.Remove(line);
                }

                lines.Clear();
            }

            for (int i = 0; i < Points.Count - 1; i++)
            {
                Line l = reset ? new Line() : lines[i];

                l.X1 = Points[i].CanvasPosition.X;
                l.Y1 = Points[i].CanvasPosition.Y;

                l.X2 = Points[i + 1].CanvasPosition.X;
                l.Y2 = Points[i + 1].CanvasPosition.Y;
                l.Stroke = selected ? ViewToolkitResources.SelectedBorderBrush : ViewToolkitResources.BlackBrush;
                l.StrokeThickness = selected ? 2 : 1;
                l.Cursor = Cursors.Hand;
                l.MouseLeftButtonDown += Connector_PreviewMouseDown;

                if (reset)
                {
                    ExolutioCanvas.Children.Add(l);
                    Canvas.SetZIndex(l, -1);
                    lines.Add(l);
                    ContextMenuService.SetContextMenu(l, ContextMenu);
                }
            }
        }
#endif

        /// <summary>
        /// Adjusts the end points position to an optimal position
        /// (when their <see cref="DragThumb.Placement"/> is set 
        /// to <see cref="EPlacementKind.ParentAutoPos"/>).
        /// </summary>
        public bool AdjustEndPoints()
        {
#if SILVERLIGHT
            if (!sourceMeasureValid)
                sourceMeasureValid = StartNode.GetBounds().Width != 0;
            if (!targetMeasureValid)
                targetMeasureValid = EndNode.GetBounds().Width != 0;
#endif

            if (StartNode == null || !StartNode.IsMeasureValid || EndNode == null || !EndNode.IsMeasureValid || StartPoint == null || EndPoint == null ||
                !targetMeasureValid || !sourceMeasureValid)
                return false;

            #region set source junctionEnd position

            double angle = StartNode.BoundsAngle;
            if (AutoPosModeOnly && StartPoint.Placement != EPlacementKind.ParentAutoPos)
                StartPoint.Placement = EPlacementKind.ParentAutoPos;

            if (StartNode.IsMeasureValid /*&& (viewHelperPointsCollection == null || Points.Count == viewHelperPointsCollection.Count)*/
                /*&& (StartPoint.Placement == EPlacementKind.ParentAutoPos || viewHelperPointsCollection == null || viewHelperPointsCollection.PointsInvalid)*/)
            {
                Rect r1 = GeometryHelper.GetFirstElementBounds(this);
                Rect r2 = GeometryHelper.GetFirstButOneElementBounds(this);

                Point p1 = GeometryHelper.RectangleRectangleCenterIntersection(r1, r2, true, angle);

                if (StartPoint.Placement != EPlacementKind.ParentAutoPos && StartPoint.Position.AlmostEqual(p1) && !((IDraggable)StartPoint).DragThumb.IsDragged)
                    StartPoint.Placement = EPlacementKind.ParentAutoPos;

                if (StartPoint.Placement == EPlacementKind.ParentAutoPos)
                { // simplified to this.. 
                    StartPoint.SetPreferedPosition(p1);
                }

                //if (viewHelperPointsCollection == null)
                //{
                //    StartPoint.SetPreferedPosition(p1);
                //}
                //else if (!viewHelperPointsCollection.First().AlmostEqual(p1))
                //{
                //    if (viewHelperPointsCollection.PointsInvalid)
                //    {
                //        viewHelperPointsCollection[0].Set(p1);
                //        StartPoint.SetPreferedPosition(p1);
                //        viewHelperPointsCollection.PointsInvalid = false;
                //    }
                //    else
                //    {
                //        if (!sourceMeasureValid)
                //        {
                //            if (p1 != StartPoint.Position)
                //            {
                //                if (!AutoPosModeOnly)
                //                {
                //                    StartPoint.Placement = EPlacementKind.AbsoluteSubCanvas;
                //                    Point snapped = r1.Normalize().SnapPointToRectangle(StartPoint.Position);
                //                    if (snapped != StartPoint.Position)
                //                        StartPoint.SetPreferedPosition(snapped);
                //                }
                //            }
                //        }
                //        else
                //        {
                //            viewHelperPointsCollection[0].Set(p1);
                //            StartPoint.SetPreferedPosition(p1);
                //        }
                //    }
                //}
                //else
                //{
                //    StartPoint.SetPreferedPosition(p1);
                //    StartPoint.Placement = EPlacementKind.ParentAutoPos;
                //}
                sourceMeasureValid = true;
            }

            #endregion

            #region set end junctionEnd position

            angle = EndNode.BoundsAngle;
            if (AutoPosModeOnly && EndPoint.Placement != EPlacementKind.ParentAutoPos)
                EndPoint.Placement = EPlacementKind.ParentAutoPos;

            if (EndNode.IsMeasureValid /*&& (viewHelperPointsCollection == null || Points.Count == viewHelperPointsCollection.Count)*/
                /*&& (EndPoint.Placement == EPlacementKind.ParentAutoPos || viewHelperPointsCollection == null || viewHelperPointsCollection.PointsInvalid)*/)
            {
                Rect r1 = GeometryHelper.GetLastElementBounds(this);
                Rect r2 = GeometryHelper.GetLastButOneElementBounds(this);

                Point p2 = GeometryHelper.RectangleRectangleCenterIntersection(r1, r2, true, angle);


                if (EndPoint.Placement != EPlacementKind.ParentAutoPos && EndPoint.Position.AlmostEqual(p2) && !((IDraggable)EndPoint).DragThumb.IsDragged)
                    EndPoint.Placement = EPlacementKind.ParentAutoPos;

                if (EndPoint.Placement == EPlacementKind.ParentAutoPos)
                { // simplified to this.. 
                    EndPoint.SetPreferedPosition(p2);
                }

                //if (viewHelperPointsCollection == null)
                //{
                //    EndPoint.SetPreferedPosition(p2);
                //}
                //else if (!viewHelperPointsCollection.Last().AlmostEqual(p2))
                //{
                //    if (viewHelperPointsCollection.PointsInvalid)
                //    {
                //        viewHelperPointsCollection.Last().Set(p2);
                //        EndPoint.SetPreferedPosition(p2);
                //        viewHelperPointsCollection.PointsInvalid = false;
                //    }
                //    else
                //    {
                //        if (!targetMeasureValid)
                //        {
                //            if (p2 != EndPoint.Position)
                //            {
                //                EndPoint.Placement = EPlacementKind.AbsoluteSubCanvas;
                //                Point snapped = r1.Normalize().SnapPointToRectangle(EndPoint.Position);
                //                if (snapped != EndPoint.Position)
                //                    EndPoint.SetPreferedPosition(snapped);
                //            }
                //        }
                //        else
                //        {
                //            viewHelperPointsCollection.Last().Set(p2);
                //            EndPoint.SetPreferedPosition(p2);
                //        }
                //    }
                //}
                //else
                //{
                //    EndPoint.SetPreferedPosition(p2);
                //    EndPoint.Placement = EPlacementKind.ParentAutoPos;
                //}
                targetMeasureValid = true;
            }

            #endregion

            return true;
        }

#if SILVERLIGHT

#else 
        
        /// <summary>
        /// Pen used to draw the junction
        /// </summary>
        /// <value><see cref="Pen"/></value>
        public Pen Pen { get; set; }

        /// <summary>
        /// Connects <see cref="Points"/> to draw the junction. 
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Points.Count > 0)
            {
                if (geometry == null)
                    InvalidateGeometry();

                drawingContext.DrawGeometry(Brushes.Transparent, ViewToolkitResources.TransparentPen, geometry);
                if (Selected)
                    drawingContext.DrawGeometry(Fill, ViewToolkitResources.JunctionSelectedPen, geometry);
                drawingContext.DrawGeometry(Fill, Pen, geometry);
            }
        }
#endif
        private readonly List<ConnectorPoint> points = new List<ConnectorPoint>();

        public IList<ConnectorPoint> Points
        {
            get { return points; }
        }

        public ConnectorPoint EndPoint
        {
            get { return Points.LastOrDefault(); }
        }

        public ConnectorPoint StartPoint
        {
            get { return Points.FirstOrDefault(); }
        }

        public delegate void ConnectorPointEventHandler(ConnectorPoint point);

        public event ConnectorPointEventHandler ConnectorPointMoved;

        internal void InvokeConnectorPointMoved(ConnectorPoint point)
        {
            ConnectorPointEventHandler handler = ConnectorPointMoved;
            if (handler != null) handler(point);
        }

        public void AddedToCanvas()
        {

        }

        public void RemovedFromCanvas()
        {
            foreach (ConnectorPoint connectorPoint in Points)
            {
                if (connectorPoint.ParentControl != null)
                {
                    connectorPoint.ParentControl.InnerConnectorControl.Children.Remove(connectorPoint);
                }
            }
#if SILVERLIGHT
            foreach (Line line in lines)
            {
                ExolutioCanvas.Children.Remove(line);
            }

#endif
            StartNode.Connectors.Remove(this);
            EndNode.Connectors.Remove(this);
        }

        public void Connect(Node node1, Node node2)
        {
            if (StartNode != null)
            {
                StartNode.Connectors.Remove(this);
            }
            if (EndNode != null)
            {
                EndNode.Connectors.Remove(this);
            }
            StartNode = node1;
            EndNode = node2;

            Point[] optimalConnection = GeometryHelper.ComputeOptimalConnection(node1, node2);

#if SILVERLIGHT
            if (node1.GetBounds().Width == 0)
            {
                sourceMeasureValid = false;
            }
            if (node2.GetBounds().Width == 0)
            {
                targetMeasureValid = false;
            }
#endif

            ConnectorPoint startPoint = new ConnectorPoint(ExolutioCanvas) { Placement = EPlacementKind.ParentAutoPos, ParentControl = node1, Connector = this, OrderInConnector = 0 };
            ConnectorPoint endPoint = new ConnectorPoint(ExolutioCanvas) { Placement = EPlacementKind.ParentAutoPos, ParentControl = node2, Connector = this, OrderInConnector = 1 };

            startPoint.SetPreferedPosition(optimalConnection[0]);
            endPoint.SetPreferedPosition(optimalConnection[1]);

            foreach (ConnectorPoint point in Points)
            {
                if (point.OrderInConnector == 0)
                {
                    point.ParentControl.InnerConnectorControl.Children.Remove(point);
                }
                if (point.OrderInConnector == Points.Count - 1)
                {
                    point.ParentControl.InnerConnectorControl.Children.Remove(point);
                }
            }
            points.Clear();
            points.Add(startPoint);
            points.Add(endPoint);
            StartNode.InnerConnectorControl.Children.Add(startPoint);
            EndNode.InnerConnectorControl.Children.Add(endPoint);

            StartNode.Connectors.Add(this);
            EndNode.Connectors.Remove(this);

            InvalidateGeometry();
        }

        #region

        public VirtualReferentialThumb VirtualReferentialThumb { get; private set; }

        public void SnapNodeToConnector(Node node)
        {
            node.SnapTo(VirtualReferentialThumb, true);
        }

        #endregion

#if SILVERLIGHT

        private ContextMenu contextMenu;

        public ContextMenu ContextMenu
        {
            get { return contextMenu; }
            set
            {
                contextMenu = value;
                foreach (Line line in lines)
                {
                    ContextMenuService.SetContextMenu(line, ContextMenu);
                }
            }
        }
#endif

        #if SILVERLIGHT
        public event Action MouseDown;
        
        public void InvokeMouseDown()
        {
            Action handler = MouseDown;
            if (handler != null) handler();
        }

        #endif
               
    }
}