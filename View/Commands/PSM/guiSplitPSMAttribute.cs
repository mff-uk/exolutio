using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;

namespace Exolutio.View.Commands.PSM
{
    public class guiSplitPSMAttribute : guiSelectionDependentCommand
    {
        public uint Count = 2;

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
                command.Commands.Add(new cmdSplitPSMAttribute(Current.Controller) { PSMAttributeGuid = a, Count = Count});
            }
            command.Execute();
        }

        public override string Text
        {
            get
            {
                return Count.ToString();
            }
        }

        public override string ScreenTipText
        {
            get { return "Split PSM attributes into " + Count; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddAttributes);
            }
        }
    }
}