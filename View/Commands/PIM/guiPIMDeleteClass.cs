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
using Exolutio.Controller.Commands.Complex.PIM;
using Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers;

namespace Exolutio.View.Commands.PIM
{
    public class guiPIMDeleteClass : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PIMDiagram)) return false;

            IEnumerable<PIMClass> selectedClasses = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMClass).Cast<PIMClass>();
            return selectedClasses.Count() > 0;
        }

        public override void Execute(object parameter)
        {
            IEnumerable<PIMClass> selectedClasses = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMClass).Cast<PIMClass>();

            MacroCommand macro = new MacroCommand(Current.Controller);
            foreach (PIMClass c in selectedClasses)
            {
                macro.Commands.Add(new Exolutio.Controller.Commands.Complex.PIM.cmdDeletePIMClass(Current.Controller) { ClassGuid = c });
            }
            macro.Execute();
        }

        public override string Text
        {
            get
            {
                return "Classes";
            }
        }

        public override string ScreenTipText
        {
            get { return "Delete selected classes"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.delete); }
        }

    }
}