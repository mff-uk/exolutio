using System.Linq;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands
{
    [Scope(ScopeAttribute.EScope.PSMClass)]
    public class guiLocateRepresentedPSMClass : guiScopeCommand
    {
        public override bool CanExecute(object parameter = null)
        {
            return ScopeObject is PSMClass && ((PSMClass)ScopeObject).IsStructuralRepresentative;
        }

        public override void Execute(object parameter = null)
        {
            PSMClass psmClass = ((PSMClass)ScopeObject);
            if (psmClass.IsStructuralRepresentative)
            {
                Diagram diagram = ModelIterator.GetDiagramForComponent(psmClass.RepresentedClass);
                Current.MainWindow.FocusComponent(diagram, psmClass.RepresentedClass);    
            }
        }

        public override string Text
        {
            get { return "Find represented class"; }
        }

        public override string ScreenTipText
        {
            get { return "Go to represented class"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.zoomIn);;
            }
        }
    }
}