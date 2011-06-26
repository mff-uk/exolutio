using System;
using System.Windows;
using System.Windows.Input;

namespace Exolutio.ViewToolkit.FormControls
{
    public class LinkLabel: System.Windows.Controls.TextBlock
    {
        private static TextDecoration textDecorations = new TextDecoration(TextDecorationLocation.Underline, ViewToolkitResources.SolidBlackPen, 2, TextDecorationUnit.Pixel, TextDecorationUnit.Pixel);

        public LinkLabel()
        {
            this.Margin = new Thickness(0,0,0,3);
            this.Padding = ViewToolkitResources.Thickness0;
            this.TextDecorations.Add(textDecorations); 
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