using System;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic.PSM;
using System.Diagnostics;

namespace Exolutio.View.Commands.PSM
{
    public class guiDeletePSMAttribute : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PSMDiagram)) return false;

            IEnumerable<PSMAttribute> selectedAttributes = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMAttribute).Cast<PSMAttribute>();

            return selectedAttributes.Count() > 0;

        }

        public override void Execute(object parameter)
        {
            IEnumerable<PSMAttribute> selectedAttributes = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMAttribute).Cast<PSMAttribute>();

            MacroCommand command = new MacroCommand(Current.Controller);
            foreach (PSMAttribute a in selectedAttributes)
            {
                command.Commands.Add(new cmdDeletePSMAttribute(Current.Controller) { AttributeGuid = a });
            }
            command.Execute();
        }

        public override string Text
        {
            get
            {
                return "Delete attributes";
            }
        }

        public override string ScreenTipText
        {
            get { return "Delete PSM attributes"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.delete);
            }
        }
    }
}