using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;


namespace SilverlightClient
{
    public class DockManager : TabControl
    {
        public TabItem ActiveDocument
        {
            get { return this.SelectedItem is TabItem ? (TabItem)this.SelectedItem : null; }
            set { if (value != null) this.SelectedItem = value; }
        }

        public IEnumerable<DiagramTab> Documents
        {
            get { return Items.OfType<DiagramTab>(); }

        }
    }
}