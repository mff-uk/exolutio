using System;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.View.Commands.PSM
{
    public class guiShiftPSMAttributeCommand : guiSelectionDependentCommand
    {
        public bool Up;
        
        public override bool CanExecute(object parameter)
        {
            if (Current.ActiveDiagram == null) return false;
          
            IEnumerable<PSMAttribute> selectedAttributes = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMAttribute).Cast<PSMAttribute>();
            return selectedAttributes.Count() > 0 && selectedAttributes.All(a => a.PSMClass.PSMAttributes.Count > 1);
        }

        public override void Execute(object parameter)
        {
            IEnumerable<PSMAttribute> selectedAttributes = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMAttribute).Cast<PSMAttribute>();
            MacroCommand macro = new MacroCommand(Current.Controller);
            foreach (PSMAttribute a in 
                Up
                ? selectedAttributes.OrderBy(a => a.PSMClass.PSMAttributes.IndexOf(a))
                : selectedAttributes.OrderByDescending(a => a.PSMClass.PSMAttributes.IndexOf(a)))
            {
                macro.Commands.Add(new acmdShiftPSMAttribute(Current.Controller, a, Up));
            }
            
            macro.Execute();
        }

        public override string Text
        {
            get
            {
                return Up ? "Up" : "Down";
            }
        }

        public override string ScreenTipText
        {
            get { return "Shift attribute"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(Up ? ExolutioResourceNames.navigate_up : ExolutioResourceNames.navigate_down); }
        }
    }
}