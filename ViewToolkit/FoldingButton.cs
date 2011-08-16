using System.Windows;
using System.Windows.Controls;

namespace Exolutio.ViewToolkit
{
    public class FoldingButton: Button
    {
        public FoldingButton()
        {
            this.Margin = ViewToolkitResources.Thickness0;
            this.Padding = ViewToolkitResources.Thickness0;
            this.Width = 13;
            this.Height = 13;
            Content = "-";
        }

        private bool folded; 
        public bool Folded
        {
            get { return folded; }
            set
            {
                folded = value;
                if (folded)
                {
                    Content = "+";
                }
                else
                {
                    Content = "-";
                }
            }
        }
    }
}