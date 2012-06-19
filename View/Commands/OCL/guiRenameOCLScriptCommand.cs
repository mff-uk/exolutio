using System.Collections.Generic;
using Exolutio.Controller.Commands.Atomic.MacroWrappers;
using Exolutio.Dialogs;
using Exolutio.Model.OCL;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.ConstraintConversion;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.OCL
{
    public class guiRenameOCLScriptCommand : guiActiveOCLScriptCommand
    {
        public override void Execute(object parameter = null)
        {
            if (Current.ActiveOCLScript != null)
            {
                string newName;
                if (ExolutioInputBox.Show("Enter new name of the script", Current.ActiveOCLScript.Name, out newName) == true)
                {
                    cmdRenameComponent c = new cmdRenameComponent(Current.Controller);
                    c.ComponentGuid = Current.ActiveOCLScript;
                    c.NewName = newName;
                    c.Execute();
                }
            }

        }

        public override string Text
        {
            get { return "Rename current OCL Script"; }
        }

        public override string ScreenTipText
        {
            get { return "Rename current OCL Script"; }
        }

        public override bool CanExecute(object parameter = null)
        {
            return Current.ActiveDiagram != null && Current.ActiveOCLScript != null;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.document_edit); }
        }
    }
}