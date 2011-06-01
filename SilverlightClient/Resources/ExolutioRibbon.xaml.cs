using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightClient.Resources
{
    public partial class ExolutioRibbon : UserControl
    {
        public ExolutioRibbon()
        {
            InitializeComponent();
        }

        public bool PIMMode
        {
            get { return gPIM.Visibility == Visibility.Visible; }
            set 
            { 
                gPIM.Visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; 
                gPSM.Visibility = !value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; 
                gGrammar.Visibility = !value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; 
            }
        }
    }
}
