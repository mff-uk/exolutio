using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EvoX.ViewToolkit
{
    /// <summary>
    /// Interaction logic for EvoXCanvasWithZoomer.xaml
    /// </summary>
    public partial class EvoXCanvasWithZoomer : UserControl
    {
        public EvoXCanvasWithZoomer()
        {
            InitializeComponent();

            zoomer.PropertyChanged+=Zoomer_PropertyChanged;
            scrollViewer.ScrollChanged += new ScrollChangedEventHandler(scrollViewer_ScrollChanged);
        }

        void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            EvoXCanvasWithZoomer_SizeChanged(null, null);
        }

        private void Zoomer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            scaleTransform.ScaleX = zoomer.ScaleX;
            scaleTransform.ScaleY = zoomer.ScaleY;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventArgs eventArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton);
            eventArgs.RoutedEvent = EvoXCanvas.MouseDownEvent;
            eventArgs.Source = EvoXCanvas; 
            EvoXCanvas.EvoXCanvas_MouseDown(EvoXCanvas, eventArgs);
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            MouseEventArgs eventArgs = new MouseEventArgs(e.MouseDevice, e.Timestamp);
            eventArgs.RoutedEvent = EvoXCanvas.MouseMoveEvent;
            eventArgs.Source = EvoXCanvas;
            EvoXCanvas.EvoXCanvas_MouseMove(EvoXCanvas, e);
            ShowHideZoomer(e);

        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventArgs eventArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton);
            eventArgs.RoutedEvent = EvoXCanvas.MouseUpEvent;
            if (e.OriginalSource == scrollViewer)
            {
                eventArgs.Source = EvoXCanvas;
            }
            EvoXCanvas.EvoXCanvas_MouseUp(EvoXCanvas, eventArgs);
        }

        private void ShowHideZoomer(MouseEventArgs e)
        {
            Point position = e.GetPosition(this);

            double comparedHeight = this.ActualHeight;

            if (zoomer.Visibility != System.Windows.Visibility.Visible
                && comparedHeight - position.Y < 50)
            {
                zoomer.Visibility = System.Windows.Visibility.Visible;
            }

            if (zoomer.Visibility == System.Windows.Visibility.Visible
                && comparedHeight - position.Y >= 50)
            {
                zoomer.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void EvoXCanvasWithZoomer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollViewer.Width = this.ActualWidth;
            scrollViewer.Height = this.ActualHeight;

            const int height = 0;
            const int scrollbarSize = 20;

            if (scrollViewer.ComputedHorizontalScrollBarVisibility == System.Windows.Visibility.Visible)
            {
                Canvas.SetBottom(zoomer, height + scrollbarSize);
            }
            else
            {
                Canvas.SetBottom(zoomer, height);
            }

            if (scrollViewer.ComputedVerticalScrollBarVisibility == System.Windows.Visibility.Visible)
            {
                zoomer.Width = this.ActualWidth - scrollbarSize;
            }
            else
            {
                zoomer.Width = this.ActualWidth;
            }
        }
    }
}
