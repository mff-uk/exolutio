using Exolutio.ResourceLibrary;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Complex.PIM;

namespace Exolutio.View.Commands.PIM
{
    public class guiSplitPIMAttribute : guiSelectionDependentCommand
    {
        public uint Count = 2;

        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PIMDiagram)) return false;

            IEnumerable<PIMAttribute> selectedAttributes = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMAttribute).Cast<PIMAttribute>();

            return selectedAttributes.Count() > 0;

        }

        public override void Execute(object parameter)
        {
            IEnumerable<PIMAttribute> selectedAttributes = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMAttribute).Cast<PIMAttribute>();

            MacroCommand command = new MacroCommand(Current.Controller);
            foreach (PIMAttribute a in selectedAttributes)
            {
                command.Commands.Add(new cmdSplitPIMAttribute(Current.Controller) { PIMAttributeGuid = a, Count = Count });
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
            get { return "Split PIM attributes into " + Count; }
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