using System.Collections.Generic;
using System.Windows.Media;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Versioning;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Versioning
{
    public class guiCreateVersionLinkBetweenSchemas : guiSelectionDependentCommand
    {
        public override void Execute(object parameter = null)
        {
            IList<DiagramView> topDiagramViews = Current.MainWindow.DiagramTabManager.GetTopDiagramViews();
            /* there must be two diagrams, each with one selected component, both components must be of the 
               same type and they must not be linked already */
            DiagramView diagramView1 = topDiagramViews[0];
            DiagramView diagramView2 = topDiagramViews[1];
            
            cmdCreateVersionLink cmdCreateVersionLinkS = new cmdCreateVersionLink(Current.Controller);
            cmdCreateVersionLinkS.Set(diagramView1.Diagram.Schema, diagramView2.Diagram.Schema);
            cmdCreateVersionLink cmdCreateVersionLinkD = new cmdCreateVersionLink(Current.Controller);
            cmdCreateVersionLinkD.Set(diagramView1.Diagram, diagramView2.Diagram);
            MacroCommand m = new MacroCommand(Current.Controller);
            m.Commands.Add(cmdCreateVersionLinkS);
            m.Commands.Add(cmdCreateVersionLinkD);
            m.CheckFirstOnlyInCanExecute = true;
            m.Execute();

            Current.InvokeSelectionChanged();
        }


        public override string Text
        {
            get { return "Link schemas"; }
        }

        public override string ScreenTipText
        {
            get { return "Add version link between two schemas."; }
        }

        public override bool CanExecute(object parameter = null)
        {
            if (Current.MainWindow == null || Current.MainWindow.DiagramTabManager == null || Current.Project == null)
                return false; 
            IList<DiagramView> topDiagramViews = Current.MainWindow.DiagramTabManager.GetTopDiagramViews();
            /* there must be two diagrams, each with one selected component,
               the diagrams must not be linked */
            if (Current.Project.UsesVersioning && topDiagramViews.Count == 2)
            {
                DiagramView diagramView1 = topDiagramViews[0];
                DiagramView diagramView2 = topDiagramViews[1];

                if (diagramView1.Diagram.Version != diagramView2.Diagram.Version &&
                    !diagramView1.Diagram.ExistsInVersion(diagramView2.Diagram.Version) &&
                    !diagramView2.Diagram.ExistsInVersion(diagramView1.Diagram.Version))
                {
                    return true;
                }
            }
            return false;
        }

        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.associate); }
        }
    }
}