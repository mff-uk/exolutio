using System.Collections.Generic;
using System.Linq;
using Exolutio.Model;
using Exolutio.Model.OCL;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.ConstraintConversion;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.OCL
{
    public class guiRemoveOCLScriptCommand : guiActiveOCLScriptCommand
    {
        public override void Execute(object parameter = null)
        {
            if (Current.ActiveOCLScript != null)
            {
                Schema schema = Current.ActiveOCLScript.Schema;
                schema.OCLScripts.Remove(Current.ActiveOCLScript);
                if (schema.OCLScripts.Count > 0)
                {
                    Current.ActiveOCLScript = (schema.OCLScripts.First());
                }
                else
                {
                    Current.ActiveOCLScript = null;
                }
            }
        }

        public override string Text
        {
            get { return "Remove OCL Script"; }
        }

        public override string ScreenTipText
        {
            get { return "Remove current OCL Script"; }
        }

        public override bool CanExecute(object parameter = null)
        {
            return Current.ActiveDiagram != null && Current.ActiveOCLScript != null;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.document_delete); }
        }
    }
}