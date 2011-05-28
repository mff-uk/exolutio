using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands
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
            Current.MainWindow.FocusComponent(pimComponent);
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
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.zoomIn); ;
            }
        }
    }
}