using System;
using AvalonDock;
using EvoX.Model;
using EvoX.View;

namespace EvoX.WPFClient
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

    }
}