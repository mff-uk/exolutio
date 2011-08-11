using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using Exolutio.Model.PSM;
using Exolutio.Model.ViewHelper;
using Exolutio.ViewToolkit;
using Vector = System.Windows.Vector;

namespace Exolutio.View
{
    /// <summary>
    /// A class for automatic layout of PSM diagrams.
    /// These diagrams have strictly tree structure where also the order of children is important,
    /// that's why user layouting is not supported and fixed automatic layout performed.
    /// </summary>
    public class VerticalTreeRightAngles: VerticalTree
    {
        public override void LayoutDiagram(PSMDiagramView psmDiagramView)
        {
            base.LayoutDiagram(psmDiagramView);
            
            foreach (PSMAssociationView pimAssociationView in psmDiagramView.RepresentantsCollection.Values.OfType<PSMAssociationView>())
            {
                DrawAssociation(pimAssociationView);
            }
        }

        private void DrawAssociation(PSMAssociationView psmAssociationView)
        {
            Connector connector = psmAssociationView.Connector;
            if (connector.StartNode == null || connector.EndNode == null)
                return; 
            double y = (int) (connector.StartNode.CanvasPosition.Y + connector.StartNode.ActualHeight/2);
            double x = (int) (connector.EndNode.CanvasPosition.X + connector.EndNode.ActualWidth/2);
            bool movelabel = false; 

            if (connector.Points.Count != 3)
            {
                if (connector.Points.Count != 2)
                {
                    
                }
                else
                {
                    connector.BreakAtPoint(new Point(x, y));
                    connector.Points[1].IsInvisible = true;
                    
                }                
            }

            if (connector.StartNode.X <= x && x < connector.StartNode.X + connector.StartNode.ActualWidth)
            {
                y = connector.StartNode.Y + connector.StartNode.ActualHeight;
            }

            movelabel = connector.Points[1].CanvasPosition != new Point(x, y);

            connector.Points[1].SetPreferedPosition(x, y);

            if (movelabel)
            {
                if (psmAssociationView.PSMAssociation.IsNamed)
                {
                    Rect r = connector.GetBounds();
                    Point center = connector.GetVirtualCenterPosition();
                    if (connector.StartPoint.CanvasPosition.X <= connector.EndPoint.CanvasPosition.X)
                    {
                        psmAssociationView.NameLabel.X = (r.Width / 2);
                    }
                    else
                    {
                        psmAssociationView.NameLabel.X = -(r.Width / 2);
                    }                    
                    psmAssociationView.NameLabel.Y = 0;
                }
            }
        }
    }
}
