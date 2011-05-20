using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands
{
    [Scope(ScopeAttribute.EScope.PSMAssociation | ScopeAttribute.EScope.PSMAttribute | ScopeAttribute.EScope.PSMClass)]
    public class guiLocateInterpretedComponent : guiScopeCommand
    {
        public override bool CanExecute(object parameter)
        {
            return ScopeObject is PSMComponent && ((PSMComponent) ScopeObject).Interpretation != null;
        }

        public override void Execute(object parameter)
        {
            PIMComponent pimComponent = ((PSMComponent)ScopeObject).Interpretation;
            if (pimComponent is PIMAttribute)
            {
                pimComponent = ((PIMAttribute) pimComponent).PIMClass;
            }

            Diagram diagram = ModelIterator.GetDiagramForComponent(pimComponent);
            Current.MainWindow.FocusComponent(diagram, pimComponent);
        }
        
        public override string Text
        {
            get { return "Find interpreted component "; }
        }

        public override string ScreenTipText
        {
            get { return Text; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.zoomIn); ;
            }
        }
    }
}