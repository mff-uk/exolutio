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
using XSDtoPSM;

namespace Exolutio.View.Commands.PSM
{
    public class guiXSDtoPSM : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            return true;

        }

        public override void Execute(object parameter)
        {
            xSDtoPSM m = new xSDtoPSM();
            m.Execute(Current.Project);
        }

        public override string Text
        {
            get
            {
                return "XSD to PSM import";
            }
        }

        public override string ScreenTipText
        {
            get { return "Import an XSD schema to PSM"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.XmlSchema);
            }
        }
    }
}