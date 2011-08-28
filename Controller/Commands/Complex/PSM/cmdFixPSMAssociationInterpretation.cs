using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;
using Exolutio.Model.PIM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("FIX PSM ASSOCIATIONS INTERPRETATIONS", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdFixPSMAssociationInterpretation : ComposedCommand
    {
        public StringBuilder report = new StringBuilder();
        string newline = Environment.NewLine;
        
        public cmdFixPSMAssociationInterpretation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdFixPSMAssociationInterpretation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        protected override void GenerateSubCommands()
        {
            report.Append("Fixing association interpretations" + newline);
            foreach (PSMAssociation a in Project.LatestVersion.PSMSchemas.SelectMany(s => s.PSMAssociations).Where(assoc => assoc.Interpretation != null && assoc.InterpretedAssociationEnd == null))
            {
                PIMAssociationEnd e;
                PIMAssociation pimassoc = a.Interpretation as PIMAssociation;
                PSMClass child = a.Child as PSMClass;
                if (child == null)
                {
                    report.Append("child not class: " + a.ToString() + newline);
                    continue;
                }
                PIMClass childInterpretation = child.Interpretation as PIMClass;
                if (pimassoc.PIMAssociationEnds.Where(ae => ae.PIMClass == childInterpretation).Count() > 1)
                {
                    report.Append("cannot fix - self reference detected: " + a.ToString() + newline);
                    continue;
                }
                else e = pimassoc.PIMAssociationEnds.Single(ae => ae.PIMClass == childInterpretation);
                report.Append("fixed OK: " + a.ToString() + newline);
                Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, a, e, a.Interpretation) { Propagate = false });
            }
        }

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(report.ToString());
        }
    }
}
