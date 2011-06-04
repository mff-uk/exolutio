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
    public class guiShiftAssociationCommand : guiSelectionDependentCommand
    {
        public bool Left;
        
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PSMDiagram)) return false;
          
            IEnumerable<PSMAssociation> selectedAssociations = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMAssociation).Cast<PSMAssociation>();
            return selectedAssociations.Count() > 0;
        }

        public override void Execute(object parameter)
        {
            IEnumerable<PSMAssociation> selectedAssociations = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMAssociation).Cast<PSMAssociation>();
            MacroCommand macro = new MacroCommand(Current.Controller);
            foreach (PSMAssociation a in 
                Left 
                ? selectedAssociations.OrderBy(a => a.Parent.ChildPSMAssociations.IndexOf(a))
                : selectedAssociations.OrderByDescending(a => a.Parent.ChildPSMAssociations.IndexOf(a)))
            {
                macro.Commands.Add(new acmdShiftPSMAssociation(Current.Controller, a, Left));
            }
            
            macro.Execute();
        }

        public override string Text
        {
            get
            {
                return Left ? "Left" : "Right";
            }
        }

        public override string ScreenTipText
        {
            get { return "Shift association"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(Left ? ExolutioResourceNames.navigate_left : ExolutioResourceNames.navigate_right); }
        }
    }
}