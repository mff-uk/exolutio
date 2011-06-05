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
using Exolutio.Model;

namespace Exolutio.View.Commands.PSM
{
    public class guiDeletePSMSchema : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PSMDiagram)) return false;

            return true;

        }

        public override void Execute(object parameter)
        {
            cmdDeletePSMSchema c = new cmdDeletePSMSchema(Current.Controller) { SchemaGuid = Current.ActiveDiagram.Schema };
            c.Execute();
        }

        public override string Text
        {
            get
            {
                return "Delete PSM schema";
            }
        }

        public override string ScreenTipText
        {
            get { return "Delete PSM schema"; }
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