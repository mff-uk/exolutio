using System;
using System.Windows.Controls;
using Exolutio.View;

namespace SilverlightClient
{
    public abstract class DiagramTab : TabItem
    {

        public abstract DiagramView DiagramView { get; }

        public string Title
        {
            get
            {
                return Header.ToString();
            }
            set
            {
                Header = value;
            }
        }


        public void BringDocumentHeaderToView(bool moveToFirst)
        {
            if (moveToFirst)
            {
                ((DockManager) this.Parent).Items.Remove(this);
                ((DockManager)this.Parent).Items.Insert(0, this);
            }
            else
            {
                ((DockManager)this.Parent).SelectedItem = this;
            }
        }


    }

}