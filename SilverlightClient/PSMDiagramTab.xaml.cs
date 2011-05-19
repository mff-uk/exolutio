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
using EvoX.View;

namespace SilverlightClient
{
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
