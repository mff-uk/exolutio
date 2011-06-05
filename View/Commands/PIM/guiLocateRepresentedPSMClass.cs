using System;
using System.Linq;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
using Exolutio.ViewToolkit;

namespace Exolutio.View.Commands
{
    [Scope(ScopeAttribute.EScope.PIMAssociation | ScopeAttribute.EScope.PIMAssociationEnd)]
    public class guiResetAssociationPointsCommand : guiScopeCommand
    {
        public override bool CanExecute(object parameter = null)
        {
            return true; 
        }

        public override void Execute(object parameter = null)
        {
            if (ScopeObject is PIMAssociationEnd)
            {
                PIMAssociationView view = (PIMAssociationView) Current.ActiveDiagramView.RepresentantsCollection[((PIMAssociationEnd)ScopeObject).PIMAssociation];
                if (view.SourceEnd == ScopeObject)
                {
                    view.Connector.StartPoint.ResetPoint();
                }
                if (view.TargetEnd == ScopeObject)
                {
                    view.Connector.EndPoint.ResetPoint();
                }
            }

            if (ScopeObject is PIMAssociation)
            {
                PIMAssociationView view = (PIMAssociationView) Current.ActiveDiagramView.RepresentantsCollection[(PIMAssociation)ScopeObject];
                view.Connector.StartPoint.ResetPoint();
                view.Connector.EndPoint.ResetPoint();
            }
        }

        public override string Text
        {
            get
            {
                if (ScopeObject is PIMAssociationEnd)
                    return "Reset association end position";
                else
                    return "Reset association ends' positions";
            }
        }

        public override string ScreenTipText
        {
            get { return Text; }
        }
    }
}