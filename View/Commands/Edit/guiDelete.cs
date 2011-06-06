using System;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using Exolutio.Model;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic.PSM;
using System.Diagnostics;
using Exolutio.Model.PIM;

namespace Exolutio.View.Commands
{
    public class guiDelete : guiSelectionDependentCommand
    {
        public guiDelete()
        {
            Gesture = KeyGestures.Delete;
        }
        
        public override bool CanExecute(object parameter)
        {
            return GuiCommands.PSMDelete.CanExecute(null) || GuiCommands.PIMDelete.CanExecute(null);

        }

        public override void Execute(object parameter)
        {
            if (Current.ActiveDiagram is PIMDiagram) GuiCommands.PIMDelete.Execute();
            else if (Current.ActiveDiagram is PSMDiagram) GuiCommands.PSMDelete.Execute();
            else Debug.Assert(false, "unknown diagram type");
        }

        public override string Text
        {
            get
            {
                return "Delete";
            }
        }

        public override string ScreenTipText
        {
            get { return "Delete components"; }
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