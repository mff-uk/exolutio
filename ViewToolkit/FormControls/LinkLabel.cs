using System;
using System.Windows;
using System.Windows.Input;

namespace EvoX.ViewToolkit.FormControls
{
    public class LinkLabel: System.Windows.Controls.Label
    {
        public LinkLabel()
        {
            this.BorderThickness = new Thickness(0,0,0,1);
            this.Margin = new Thickness(0,0,0,3);
            this.Padding = new Thickness(0);
            this.BorderBrush = ViewToolkitResources.BlackBrush;
            this.Cursor = Cursors.Hand;
            this.MouseUp += new MouseButtonEventHandler(LinkLabel_MouseUp);
            this.Focusable = false;
        }

        void LinkLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            InvokeClick(e);
        }

        public event EventHandler<MouseButtonEventArgs> Click;

        public void InvokeClick(MouseButtonEventArgs e)
        {
            EventHandler<MouseButtonEventArgs> handler = Click;
            if (handler != null) handler(this, e);
        }
    }
}