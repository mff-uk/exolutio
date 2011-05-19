using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;


namespace SilverlightClient
{
    public class DockManager : TabControl
    {
        public DiagramTab ActiveDocument
        {
            get { return this.SelectedItem is DiagramTab ? (DiagramTab)this.SelectedItem : null; }
            set { if (value != null) this.SelectedItem = value; }
        }

        public IEnumerable<DiagramTab> Documents
        {
            get { return Items.Cast<DiagramTab>(); }

        }
    }
}