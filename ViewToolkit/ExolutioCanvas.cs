using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Exolutio.ViewToolkit
{
    public partial class ExolutioCanvas : Canvas
    {
        public ExolutioCanvas()
        {
#if SILVERLIGHT
            this.LayoutUpdated += ExolutioCanvas_LayoutUpdated;
            this.Background = ViewToolkitResources.GoldBrush;
#else

#endif
            InitializeStates();
            Loaded += Canvas_Loaded;
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            //InitializeZooming();
        }

        #region selection

        private readonly List<ISelectable> selectedItems = new List<ISelectable>();
        internal IList<ISelectable> SelectedItems
        {
            get { return selectedItems; }
        }

        internal void ClearCanvasSelectedItems()
        {
            foreach (ISelectable selectedItem in SelectedItems.ToArray())
            {
                selectedItem.Selected = false;
            }
            SelectedItems.Clear();
        }

        public event Action CanvasSelectionCleared;

        internal void InvokeCanvasSelectionCleared()
        {
            Action handler = CanvasSelectionCleared;
            if (handler != null) handler();
        }

        #endregion


        #region Add/remove objects

        public event Action ContentChanged;

        public void InvokeContentChanged()
        {
            Action handler = ContentChanged;
            if (handler != null) handler();
            GlobalViewEvents.InvokeCanvasContentChanged();
        }

        public void AddNode(Node node)
        {
            this.Children.Add(node);
            node.ExolutioCanvas = this;
            node.AddedToCanvas();
            InvalidateMeasure();
            InvokeContentChanged();
        }

        public void RemoveNode(Node node)
        {
            SelectedItems.Remove(node);
            node.RemovedFromCanvas();
            this.Children.Remove(node);
            node.ExolutioCanvas = null;
            InvalidateMeasure();
            InvokeContentChanged();
        }

        public void AddConnector(Connector connector)
        {
            this.Children.Add(connector);
            connector.ExolutioCanvas = this;
            connector.AddedToCanvas();
            Canvas.SetZIndex(connector, -1);
            InvalidateMeasure();
            InvokeContentChanged();
        }

        public void RemoveConnector(Connector connector)
        {
            SelectedItems.Remove(connector);
            connector.RemovedFromCanvas();
            this.Children.Remove(connector);
            connector.ExolutioCanvas = null;
            InvalidateMeasure();
            InvokeContentChanged();
        }

        #endregion

#if SILVERLIGHT
        void ExolutioCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            FixSize();
        }
                
        public void FixSize()
        {
            ScrollViewer parentSV = Parent as ScrollViewer;
            Size size = ComputeCanvasSize(null);
            if (parentSV != null)
            {
                Size renderSize = parentSV.RenderSize;
                //if (RenderSize.Width > 0)
                //{
                //    size = new Size(Math.Max(renderSize.Width, size.Width), Math.Max(renderSize.Height, size.Height));
                //}
            }

            if (Parent != null)
            {
                Size parentSize = ((UIElement)Parent).RenderSize;
                
                size = new Size(Math.Max(parentSize.Width - 10, size.Width), Math.Max(parentSize.Height - 10, size.Height));
            }

            {
                this.Width = size.Width;
                this.Height = size.Height;
            }
        }
#else
        /// <summary>
        /// This is called by ScrollView, returns desired Canvas size
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns>Desired Canvas size</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size size = ComputeCanvasSize(constraint);
            return size;
        }
#endif
        #region Resizing



        private Size ComputeCanvasSize(Size? constraint)
        {
            Size size = new Size();
            foreach (UIElement element in this.Children)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                //measure desired size for each child
                if (constraint != null)
                {
                    element.Measure(constraint.Value);
                }
                Size desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }
            //for aesthetic reasons add extra points
            size.Width += 10;
            size.Height += 10;
            return size;
        }

        #endregion

#if SILVERLIGHT
        public ContextMenu ContextMenu
        {
            get;
            set;
        }
#endif

        //#region Zoom

        //private ScrollViewer ParentScrollViewer { get; set; }

        //private ScaleTransform ScaleTransform
        //{
        //    get { return (ScaleTransform) this.LayoutTransform; }
        //}

        //private Zoomer Zoomer { get; set; }

        //private void InitializeZooming()
        //{
        //    this.LayoutTransform = new ScaleTransform(1, 1);
        //    this.Zoomer = new Zoomer();
        //    Zoomer.PropertyChanged += Zoom;
        //    this.MouseWheel += Zoomer.Canvas_MouseWheel;

        //    FrameworkElement p = this.Parent as FrameworkElement;
        //    while (p != null && !(p is ScrollViewer))
        //    {
        //        p = p.Parent as FrameworkElement;
        //    }
        //    if (p is ScrollViewer)
        //    {
        //        ParentScrollViewer = (ScrollViewer) p;
        //    }
        //}

        //private void Zoom(object sender, PropertyChangedEventArgs e)
        //{
        //    ScaleTransform.ScaleX = Zoomer.ScaleX;
        //    ScaleTransform.ScaleY = Zoomer.ScaleY;
        //}

        //private void ShowZoomer()
        //{
        //    this.Children.Add(Zoomer);
        //    double comparedHeight = ParentScrollViewer != null ? ParentScrollViewer.ActualHeight : this.ActualHeight;

        //    Canvas.SetTop(Zoomer, comparedHeight - 50);
        //    Zoomer.Visibility = System.Windows.Visibility.Visible;
        //}

        //private void HideZoomer()
        //{
        //    this.Children.Remove(Zoomer);
        //    Zoomer.Visibility = System.Windows.Visibility.Collapsed;
        //}

        //private void ShowHideZoomer(MouseEventArgs e)
        //{
        //    if (State == ECanvasState.Normal && !normalState.IsMouseLeftButtonDown)
        //    {
        //        Point position = e.GetPosition(this);

        //        double comparedHeight = ParentScrollViewer != null ? ParentScrollViewer.ActualHeight : this.ActualHeight;

        //        if (Zoomer.Visibility == System.Windows.Visibility.Collapsed
        //            && comparedHeight - position.Y < 50)
        //        {
        //            ShowZoomer();
        //        }

        //        if (Zoomer.Visibility == System.Windows.Visibility.Visible
        //            && comparedHeight - position.Y >= 50)
        //        {
        //            HideZoomer();
        //        }
        //    }
        //}

        //#endregion

        #region State handling

        ///// <summary>
        ///// Instance of <see cref="DraggingConnectionState"/> for this ExolutioCanvas.
        ///// </summary>
        //public DraggingConnectionState draggingConnectionState { get; private set; }

        ///// <summary>
        ///// Instance of <see cref="DraggingElementState"/> for this ExolutioCanvas.
        ///// </summary>
        //public DraggingElementState draggingElementState { get; private set; }

        /// <summary>
        /// Instance of <see cref="NormalState"/> for this ExolutioCanvas.
        /// </summary>
        public NormalState normalState { get; private set; }
        public TakingSnapshotState takingSnapshotState { get; private set; }

        /// <summary>
        /// Current canvas state
        /// </summary>
        private ExolutioCanvasState CurrentState;

        private ECanvasState state;

        /// <summary>
        /// Sets <see cref="ExolutioCanvasState">state</see> of the canvas
        /// </summary>
        public ECanvasState State
        {
            get
            {
                return state;
            }
            set
            {
                if (CurrentState != null)
                    CurrentState.StateLeft();
                state = value;
                switch (state)
                {
                    case ECanvasState.DraggingConnection:
                        throw new NotImplementedException("Member ExolutioCanvas.State not implemented.");
                    //CurrentState = draggingConnectionState;
                    case ECanvasState.Normal:
                        CurrentState = normalState;
                        break;
                    case ECanvasState.TakingSnapshot:
                        CurrentState = takingSnapshotState;
                        break;
                    case ECanvasState.DraggingElement:
                        throw new NotImplementedException("Member ExolutioCanvas.State not implemented.");
                    //CurrentState = draggingElementState;
                }
                if (CurrentState != null)
                    CurrentState.StateActivated();
            }
        }

        private void InitializeStates()
        {
            normalState = new NormalState(this);
            this.takingSnapshotState = new TakingSnapshotState(this);
            this.State = ECanvasState.Normal;
            this.MouseMove += ExolutioCanvas_MouseMove;
#if SILVERLIGHT
            this.MouseLeftButtonUp += ExolutioCanvas_MouseUp;
            this.MouseLeftButtonDown += ExolutioCanvas_MouseDown;
#else
            this.MouseUp += ExolutioCanvas_MouseUp;
            this.MouseDown += ExolutioCanvas_MouseDown;
#endif
        }

        internal void ExolutioCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
#if SILVERLIGHT
            CurrentState.LeftButton = true; 
#else
            base.OnMouseDown(e);
#endif
            CurrentState.Canvas_MouseDown(e);
        }

        internal void ExolutioCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
#if SILVERLIGHT
            bool wasRightButton = CurrentState.RightButton;
            CurrentState.LeftButton = false;
            CurrentState.RightButton = false;
#endif
            CurrentState.Canvas_MouseUp(e);


#if SILVERLIGHT
            if (e.OriginalSource == this && wasRightButton)
#else
            if (e.ChangedButton == MouseButton.Right && e.OriginalSource == this)
#endif
            {
                this.ContextMenu.IsOpen = true;
            }
        }



        internal void ExolutioCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            CurrentState.Canvas_MouseMove(e);
#if SILVERLIGHT
            mousePosition = e.GetPosition(this);
        }

        /// <summary>
        /// In silverlight, we keep track of mouse position via MouseMove event, since 
        /// Mouse class from WPF is not available. 
        /// </summary>
        private Point mousePosition; 
#else
            if (mouseLabel == null)
            {
                mouseLabel = new System.Windows.Controls.Label();
                this.Children.Add(mouseLabel);
            }
            mouseLabel.Content = string.Format("{0}, {1}", Mouse.GetPosition(this).X, Mouse.GetPosition(this).Y);

            //ShowHideZoomer(e);
        }

        private System.Windows.Controls.Label mouseLabel;
#endif

        public Point GetMousePosition()
        {
#if SILVERLIGHT
            return mousePosition;
#else
            return Mouse.GetPosition(this);
#endif
        }

        public void SelectableItem_PreviewMouseDown(ISelectable item, MouseButtonEventArgs e)
        {
            CurrentState.SelectableItem_PreviewMouseDown(item, e);
        }

        #endregion

        public event Action ScreenShotView;

        public bool InScreenshotView { get; private set; }

        public void EnterScreenshotView()
        {
            #if SILVERLIGHT 
            #else
            mouseLabel.Visibility = Visibility.Hidden;
            #endif
            InScreenshotView = true; 
            if (ScreenShotView != null)
            {
                ScreenShotView();
            }
        }

        public void ExitScreenshotView()
        {
            InScreenshotView = false;
            #if SILVERLIGHT
            #else
            mouseLabel.Visibility = Visibility.Visible;
            #endif
            if (ScreenShotView != null)
            {
                ScreenShotView();
            }
        }
    }
}