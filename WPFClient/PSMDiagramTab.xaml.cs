using System;
using System.Collections.Generic;
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
using Exolutio.View;

namespace Exolutio.WPFClient
{
    /// <summary>
    /// Interaction logic for PSMDiagramTab.xaml
    /// </summary>
    public partial class PSMDiagramTab 
    {
        public PSMDiagramTab()
        {
            InitializeComponent();
        }

        #region Overrides of DiagramTab

        public override DiagramView DiagramView
        {
            get { return PSMDiagramView; }
        }

        #endregion
    }
}
