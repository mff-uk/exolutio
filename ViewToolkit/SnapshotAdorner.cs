using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System;
using System.Windows.Controls;
using System.Windows.Shapes;
using Exolutio.SupportingClasses;

namespace Exolutio.ViewToolkit
{
    public class SnapshotAdorner : Adorner
    {
        private Point? startPoint, endPoint;
        private Rectangle rubberband;
        private VisualCollection visuals;
        private Canvas adornerCanvas;
        private readonly ExolutioCanvas ExolutioCanvas;

        protected override int VisualChildrenCount
        {
            get
            {
                return this.visuals.Count;
            }
        }


		/// <summary>
        /// Initializes a new instance of the <see cref="SnapshotAdorner"/> class.
		/// </summary>
		/// <param name="ExolutioCanvas">The designer canvas.</param>
		/// <param name="dragStartPoint">The drag start point.</param>
        public SnapshotAdorner(ExolutioCanvas ExolutioCanvas, Point? dragStartPoint)
            : base(ExolutioCanvas)
        {
            this.ExolutioCanvas = ExolutioCanvas;
            this.startPoint = dragStartPoint;
			this.endPoint = dragStartPoint;

            this.adornerCanvas = new Canvas();
            this.adornerCanvas.Background = Brushes.Transparent;
            this.visuals = new VisualCollection(this);
            this.visuals.Add(this.adornerCanvas);

            this.rubberband = new Rectangle();
            this.rubberband.Stroke = Brushes.Navy;
            this.rubberband.StrokeThickness = 1;
            this.rubberband.StrokeDashArray = new DoubleCollection(new double[] { 2 });

            this.adornerCanvas.Children.Add(this.rubberband);
        }

		/// <summary>
		/// Updates selection according to new cursor position.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                endPoint = e.GetPosition(this);
                UpdateRubberband();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.visuals[index];
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.adornerCanvas.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        private void UpdateRubberband()
        {
            double left = Math.Min(this.startPoint.Value.X, this.endPoint.Value.X);
            double top = Math.Min(this.startPoint.Value.Y, this.endPoint.Value.Y);

            double width = Math.Abs(this.startPoint.Value.X - this.endPoint.Value.X);
            double height = Math.Abs(this.startPoint.Value.Y - this.endPoint.Value.Y);

            this.rubberband.Width = width;
            this.rubberband.Height = height;
            Canvas.SetLeft(this.rubberband, left);
            Canvas.SetTop(this.rubberband, top);

            ExolutioCanvas.takingSnapshotState.selectionRectangle = new Rect(left, top, width, height);
        }

		/// <summary>
		/// Updates the selection and removes the adorner.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the mouse button was released.</param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            // remove adorner (=this) from adorner layer
            AdornerLayer adornerLayer = this.Parent as AdornerLayer;
            if (adornerLayer != null) 
            {
                adornerLayer.Remove(this);
            }

            //e.Handled = true;
        }
    }
}
