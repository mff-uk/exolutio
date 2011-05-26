using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Effects;
using EvoX.SupportingClasses;

namespace EvoX.ViewToolkit
{
    public class Node : ContentControl, ISelectable, IDraggable
    {
        private EvoXCanvas evoXCanvas;
        public EvoXCanvas EvoXCanvas
        {
            get { return evoXCanvas; }
            set { evoXCanvas = value; }
        }

        private double x;
        public double X
        {
            get { return x; }
            set
            {
                x = value;
                if (EvoXCanvas != null)
                {
                    Canvas.SetLeft(this, x);
                    if (dragThumb != null)
                    {
                        dragThumb.X = value;
                    }
                }
            }
        }

        private double y;
        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
                if (EvoXCanvas != null)
                {
                    Canvas.SetTop(this, y);
                    if (dragThumb != null)
                    {
                        dragThumb.Y = value;
                    }
                }
            }
        }

        public Point CanvasPosition
        {
            get { return dragThumb.CanvasPosition; }
        }

        public Point Position { get { return new Point(X, Y); } }

        public double BoundsAngle { get; set; }

        public void SetPositionSilent(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public EPlacementCenter PlacementCenter
        {
            get { return dragThumb.PlacementCenter; }
            set { dragThumb.PlacementCenter = value; }
        }

        public EPlacementKind Placement
        {
            get { return dragThumb.Placement; }
            set { dragThumb.Placement = value; }
        }

        internal Canvas InnerConnectorControl { get; private set; }
        public ContentControl InnerContentControl { get; private set; }
        protected Grid InnerGrid { get; private set; }

        public Node()
        {
            InnerGrid = new Grid();
            this.Content = InnerGrid;
            InnerConnectorControl = new Canvas();
            InnerContentControl = new ContentControl();
            InnerGrid.Children.Add(InnerConnectorControl);
            InnerGrid.Children.Add(InnerContentControl);

            Loaded += delegate { InvokePositionChanged(); };
            SizeChanged += delegate { InvokePositionChanged(); };
            PositionChanged += OnPositionChanged;
#if SILVERLIGHT
#else
            PreviewMouseDown += Node_PreviewMouseDown;
#endif
        }


        private DragThumb dragThumb;

        public virtual bool Draggable { get { return true; } }

        DragThumb IDraggable.DragThumb
        {
            get { return DragThumb; }
        }

        internal DragThumb DragThumb
        {
            get { return dragThumb; }
        }

        public void SnapTo(IReferentialElement referentialElement, bool recalcPosition)
        {
            DragThumb.SnapTo(referentialElement, recalcPosition);
        }

        private readonly List<Connector> connectors = new List<Connector>();

        public List<Connector> Connectors
        {
            get { return connectors; }
        }

        public void UpdateCanvasPosition(bool fixRelativePlacement)
        {
            if (EvoXCanvas != null)
            {
                if (fixRelativePlacement && dragThumb != null)
                {
                    ViewToolkit.DragThumb.UpdatePos(this.dragThumb);
                }
                else
                {
                    Canvas.SetLeft(this, X);
                    Canvas.SetTop(this, Y);
                }
            }
        }

        internal void AddedToCanvas()
        {
            this.UpdateCanvasPosition(true);
            if (Draggable)
            {
                this.dragThumb = new DragThumb(EvoXCanvas, this);
                dragThumb.Placement = EPlacementKind.AbsoluteCanvas;
                dragThumb.PositionChanged += InvokePositionChanged;
            }
        }

        internal void RemovedFromCanvas()
        {
            this.DragThumb.ParentControl = null;
            this.dragThumb = null;
        }

#if SILVERLIGHT
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Node_PreviewMouseDown(this, e);
            base.OnMouseLeftButtonDown(e);

#else
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
#endif
            if (Draggable)
            {
                dragThumb.OnMouseDown(e);
            }
        }
#if SILVERLIGHT
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
#else
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
#endif
            if (Draggable)
            {
                dragThumb.OnMouseUp(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Draggable)
            {
                dragThumb.OnMouseMove(e);
            }
        }

        #region Implementation of ISelectable

        public Rect GetBounds()
        {
            return GeometryHelper.GetBasicControlBounds(this);
        }

        public void AdjustDrag(ref DragDeltaEventArgs deltaEventArgs)
        {
            // do nothing
        }

        public virtual bool CanBeDraggedInGroup
        {
            get { return true; }
        }

        private bool selected;
        public virtual bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (selected)
                {
                    EvoXCanvas.SelectedItems.AddIfNotContained(this);
                }
                else
                {
                    EvoXCanvas.SelectedItems.Remove(this);
                }
                InvokeSelectedChanged();
            }
        }

        private bool highlighted;

        public virtual bool Highlighted
        {
            get
            {
                return highlighted;
            }
            set
            {
                highlighted = value;
                this.BorderBrush = V
            }
        }

        void Node_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            EvoXCanvas.SelectableItem_PreviewMouseDown(this, e);
        }

        public event Action SelectedChanged;

        public void InvokeSelectedChanged()
        {
            Action handler = SelectedChanged;
            if (handler != null) handler();
        }

        #endregion

        public event Action PositionChanged;

        protected virtual void OnPositionChanged()
        {
            if (EvoXCanvas != null)
            {
                EvoXCanvas.InvokeContentChanged();
            }
        }

        public void InvokePositionChanged()
        {
            Action handler = PositionChanged;
            if (handler != null) handler();
        }

#if SILVERLIGHT

        public bool IsMeasureValid
        {
            get { return true; }
        }



#endif

        //public override string ToString()
        //{
        //    return String.Format("SPH: rel: [{0:0},{1:0}] abs [{2:0},{3:0}]", X, Y, dragThumb.CanvasPosition.X, dragThumb.CanvasPosition.Y);
        //}


    }
}