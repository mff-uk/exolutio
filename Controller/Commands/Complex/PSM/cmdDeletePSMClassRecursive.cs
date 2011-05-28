using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    public class cmdDeletePSMClassRecursive : MacroCommand
    {
        public Guid ClassGuid { get; set; }

        public cmdDeletePSMClassRecursive()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePSMClassRecursive(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid classGuid)
        {
            ClassGuid = classGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            PSMClass psmClass = Project.TranslateComponent<PSMClass>(ClassGuid);
            //if (psmClass.ParentAssociation == null)
            //{
                foreach (PSMAttribute a in psmClass.PSMAttributes)
                {
                    cmdDeletePSMAttribute da = new cmdDeletePSMAttribute(Controller);
                    da.Set(a);
                    Commands.Add(da);
                }
                foreach (PSMAssociation a in psmClass.ChildPSMAssociations)
                {
                    cmdDeletePSMAssociationRecursive da = new cmdDeletePSMAssociationRecursive(Controller);
                    da.Set(a);
                    Commands.Add(da);
                }
                Commands.Add(new acmdSetPSMClassInterpretation(Controller, ClassGuid, Guid.Empty));
                Commands.Add(new acmdRenameComponent(Controller, ClassGuid, ""));
                Commands.Add(new acmdDeletePSMClass(Controller, ClassGuid));
            /*}
            else
            {
                cmdDeletePSMAssociation cmddelete = new cmdDeletePSMAssociation(Controller);
                cmddelete.Set(psmClass.ParentAssociation);
                Commands.Add(cmddelete);
            }*/
        }

        public override bool CanExecute()
        {
            if (ClassGuid == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PSM_CLASS_RECURSIVE);
        }

    }
}
