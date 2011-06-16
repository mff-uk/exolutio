using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Exolutio.Controller.Commands.Versioning;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Versioning
{
    public class guiCreateVersionLink : guiSelectionDependentCommand
    {
        public override void Execute(object parameter = null)
        {
            IList<DiagramView> topDiagramViews = Current.MainWindow.DiagramTabManager.GetTopDiagramViews();
            /* there must be two diagrams, each with one selected component, both components must be of the 
               same type and they must not be linked already */
            DiagramView diagramView1 = topDiagramViews[0];
            DiagramView diagramView2 = topDiagramViews[1];
            IEnumerable<Component> selectedComponents1 = diagramView1.GetSelectedComponents();
            IEnumerable<Component> selectedComponents2 = diagramView2.GetSelectedComponents();
            Component component1 = selectedComponents1.First();
            Component component2 = selectedComponents2.First();

            cmdCreateVersionLink cmdCreateVersionLink = new cmdCreateVersionLink(Current.Controller);
            cmdCreateVersionLink.Set(component1, component2);
            cmdCreateVersionLink.Execute();
            Current.InvokeSelectionChanged();
        }


        public override string Text
        {
            get { return "Add version link"; }
        }

        public override string ScreenTipText
        {
            get { return "Add version link between two constructs."; }
        }

        public override bool CanExecute(object parameter = null)
        {
            if (Current.MainWindow == null || Current.MainWindow.DiagramTabManager == null || Current.Project == null)
                return false; 
            IList<DiagramView> topDiagramViews = Current.MainWindow.DiagramTabManager.GetTopDiagramViews();
            /* there must be two diagrams, each with one selected component, both components must be of the 
               same type and they must not be linked already */
            if (Current.Project.UsesVersioning && topDiagramViews.Count == 2)
            {
                DiagramView diagramView1 = topDiagramViews[0];
                DiagramView diagramView2 = topDiagramViews[1];

                if (Current.Project.VersionManager.AreItemsLinked(diagramView1.Diagram, diagramView2.Diagram))
                {
                    IEnumerable<Component> selectedComponents1 = diagramView1.GetSelectedComponents();
                    IEnumerable<Component> selectedComponents2 = diagramView2.GetSelectedComponents();
                    if (selectedComponents1.Count() == 1 && selectedComponents2.Count() == 1)
                    {
                        Component component1 = selectedComponents1.First();
                        Component component2 = selectedComponents2.First();
                        if (component1.GetType() == component2.GetType() && component1.Version != component2.Version
                            && !component1.ExistsInVersion(component2.Version) && !component2.ExistsInVersion(component1.Version))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.associate); }
        }
    }

    public class guiRemoveVersionLink : guiSelectionDependentCommand
    {
        public override void Execute(object parameter = null)
        {
            IList<DiagramView> topDiagramViews = Current.MainWindow.DiagramTabManager.GetTopDiagramViews();
            /* there must be two diagrams, each with one selected component, both components must be of the 
               same type and they must not be linked already */
            DiagramView diagramView1 = topDiagramViews[0];
            DiagramView diagramView2 = topDiagramViews[1];
            IEnumerable<Component> selectedComponents1 = diagramView1.GetSelectedComponents();
            IEnumerable<Component> selectedComponents2 = diagramView2.GetSelectedComponents();
            Component component1 = selectedComponents1.First();
            Component component2 = selectedComponents2.First();

            cmdRemoveVersionLink cmdRemoveVersionLink = new cmdRemoveVersionLink(Current.Controller);
            cmdRemoveVersionLink.Set(component1, component2);
            cmdRemoveVersionLink.Execute();
            Current.InvokeSelectionChanged();
        }


        public override string Text
        {
            get { return "Remove version link"; }
        }

        public override string ScreenTipText
        {
            get { return "Remove version link between two constructs."; }
        }

        public override bool CanExecute(object parameter = null)
        {
            if (Current.MainWindow == null || Current.MainWindow.DiagramTabManager == null || Current.Project == null)
                return false; 
            IList<DiagramView> topDiagramViews = Current.MainWindow.DiagramTabManager.GetTopDiagramViews();
            /* there must be two diagrams, each with one selected component, both components must be of the 
               same type and they must be linked already */
            if (Current.Project.UsesVersioning && topDiagramViews.Count == 2)
            {
                DiagramView diagramView1 = topDiagramViews[0];
                DiagramView diagramView2 = topDiagramViews[1];

                if (Current.Project.VersionManager.AreItemsLinked(diagramView1.Diagram, diagramView2.Diagram))
                {
                    IEnumerable<Component> selectedComponents1 = diagramView1.GetSelectedComponents();
                    IEnumerable<Component> selectedComponents2 = diagramView2.GetSelectedComponents();
                    if (selectedComponents1.Count() == 1 && selectedComponents2.Count() == 1)
                    {
                        Component component1 = selectedComponents1.First();
                        Component component2 = selectedComponents2.First();
                        if (component1.GetType() == component2.GetType() && component1.Version != component2.Version
                            && Current.Project.VersionManager.AreItemsLinked(component1, component2))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.branch_delete); }
        }
    }


}