using System.Linq;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Local
{
    [Scope(ScopeAttribute.EScope.PSMClass)]
    public class guiLocateRepresentedPSMClass : guiScopeCommand
    {
        public override bool CanExecute(object parameter)
        {
            return ScopeObject is PSMClass && ((PSMClass)ScopeObject).IsStructuralRepresentative;
        }

        public override void Execute(object parameter)
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
                return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.zoomIn);;
            }
        }
    }
}