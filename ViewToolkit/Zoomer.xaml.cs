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
    /// Interaction logic for Zoomer.xaml
    /// </summary>
    public partial class Zoomer: INotifyPropertyChanged
    {
        public Zoomer()
        {
            InitializeComponent();
        }
        
        public double ScaleX
        {
            get { return scaleX; }
            set { scaleX = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ScaleX"));}
        }

        public double ScaleY
        {
            get { return scaleY; }
            set { scaleY = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ScaleY")); }
        }

        private void bReset_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value = 1;
        }

        private ScrollViewer scrollViewer = null;

        private double scaleX = 1;
        private double scaleY = 1;


        public void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.Parent is ScrollViewer)
            {
                scrollViewer = this.Parent as ScrollViewer;
            }

            var zsv = zoomSlider.Value;
            double ho = 0;
            
            if (scrollViewer != null)
            {
                ho = scrollViewer.HorizontalOffset;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                zoomSlider.Value += e.Delta > 0 ? 0.1 : -0.1;
                e.Handled = true;
            }

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToHorizontalOffset(ho * (1 + (zoomSlider.Value - zsv)));
            }

            ScaleX = zoomSlider.Value;
            ScaleY = zoomSlider.Value;
        }

        private void bZoomIn_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value += 0.1;
            ScaleX = zoomSlider.Value;
            ScaleY = zoomSlider.Value;
        }

        private void bZoomOut_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value -= 0.1;
            ScaleX = zoomSlider.Value;
            ScaleY = zoomSlider.Value;
        }

        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            {
                ScaleX = zoomSlider.Value;
                ScaleY = zoomSlider.Value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
