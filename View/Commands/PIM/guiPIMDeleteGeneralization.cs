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
    public class guiPIMDeleteGeneralization : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PIMDiagram)) return false;

            IEnumerable<PIMGeneralization> selectedGeneralizations = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMGeneralization).Cast<PIMGeneralization>();
            return selectedGeneralizations.Count() > 0;
        }

        public override void Execute(object parameter)
        {
            IEnumerable<PIMGeneralization> selectedGeneralizations = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMGeneralization).Cast<PIMGeneralization>();

            MacroCommand macro = new MacroCommand(Current.Controller);
            foreach (PIMGeneralization g in selectedGeneralizations)
            {
                macro.Commands.Add(new Exolutio.Controller.Commands.Atomic.PIM.acmdDeletePIMGeneralization(Current.Controller, g));
            }
            macro.Execute();
        }

        public override string Text
        {
            get
            {
                return "Generalizations";
            }
        }

        public override string ScreenTipText
        {
            get { return "Delete selected generalizations"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.delete); }
        }

    }
}