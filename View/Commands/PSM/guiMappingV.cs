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
using PSMtoPIM_v;

namespace Exolutio.View.Commands.PSM
{
    public class guiMappingV : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PSMDiagram)) return false;

            return true;

        }

        public override void Execute(object parameter)
        {
            MappingV m = new MappingV();
            m.Execute(Current.Project, (Current.ActiveDiagram as PSMDiagram).PSMSchema);
        }

        public override string Text
        {
            get
            {
                return "Mapping V";
            }
        }

        public override string ScreenTipText
        {
            get { return "Mapping of PSM to PIM - Viktorinova method"; }
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