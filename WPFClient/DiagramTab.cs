using System;
using System.Windows.Data;
using AvalonDock;
using Exolutio.Model;
using Exolutio.View;

namespace Exolutio.WPFClient
{
    public abstract class DiagramTab: DocumentContent
    {
        public abstract DiagramView DiagramView { get; }


        public void BringDocumentHeaderToView(bool moveToFirst)
        {
            if (ContainerPane != null)
            {
                base.Activate();
            }
			if (moveToFirst)
				ContainerPane.Items.MoveCurrentToFirst();
        }

        Binding titleBinding;

        public void BindTab(Diagram diagram)
        {    
            titleBinding = new Binding("Caption");
            titleBinding.Source = diagram;
            titleBinding.Mode = BindingMode.OneWay;
            this.SetBinding(TitleProperty, titleBinding);
        }

        public void UnBindTab()
        {
            BindingOperations.ClearBinding(this, TitleProperty);
        }

        protected override void OnClosed()
        {
            UnBindTab();
            base.OnClosed();
        }
    }
}