using System;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic.PIM;
using Exolutio.Model.PIM;

namespace Exolutio.View.Commands.PIM
{
    public class guiShiftPIMAttributeCommand : guiSelectionDependentCommand
    {
        public bool Up;
        
        public override bool CanExecute(object parameter)
        {
            if (Current.ActiveDiagram == null) return false;
          
            IEnumerable<PIMAttribute> selectedAttributes = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMAttribute).Cast<PIMAttribute>();
            return selectedAttributes.Count() > 0;
        }

        public override void Execute(object parameter)
        {
            IEnumerable<PIMAttribute> selectedAttributes = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMAttribute).Cast<PIMAttribute>();
            MacroCommand macro = new MacroCommand(Current.Controller);
            foreach (PIMAttribute a in 
                Up
                ? selectedAttributes.OrderBy(a => a.PIMClass.PIMAttributes.IndexOf(a))
                : selectedAttributes.OrderByDescending(a => a.PIMClass.PIMAttributes.IndexOf(a)))
            {
                macro.Commands.Add(new acmdShiftPIMAttribute(Current.Controller, a, Up));
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