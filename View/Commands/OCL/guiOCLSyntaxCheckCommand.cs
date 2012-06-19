using System.Collections.Generic;
using System.Text;
using Exolutio.Model.OCL;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.ConstraintConversion;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.OCL
{
    public class guiOCLSyntaxCheckCommand : guiActiveOCLScriptCommand
    {
        public override void Execute(object parameter = null)
        {
            if (Current.ActiveDiagram != null)
            {
                OCLScript script = Current.ActiveOCLScript;
                if (script != null)
                {
                    var res = script.CompileToAst();
                    StringBuilder sb = new StringBuilder();
                    if (res.Errors.HasError)
                    {
                        sb.AppendLine("Errors:");
                        foreach (var er in res.Errors.Errors)
                        {
                            sb.AppendLine(er.ToString());
                        }
                    }
                    else
                    {
                        sb.AppendLine("Compilation OK.");
                        foreach (var context in res.Constraints.ClassifierConstraintBlocks)
                        {
                            sb.AppendLine("context " + context.Context.ToString());
                            foreach (var constraint in context.Invariants)
                            {
                                sb.AppendLine("inv: " + constraint.ToString());
                            }
                        }
                    }
                    Exolutio.Dialogs.ExolutioMessageBox.Show("OCL Compilation", "OCL compilation result", sb.ToString());
                }

            }
        }

        public override string Text
        {
            get { return "OCL syntax check"; }
        }

        public override string ScreenTipText
        {
            get { return "Checks syntax and correctnes of the OCL script"; }
        }

        public override bool CanExecute(object parameter = null)
        {
            return Current.ActiveDiagram != null && Current.ActiveOCLScript != null;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.document_check); }
        }
    }
}