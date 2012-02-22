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
    [PublicCommand("Delete root PSM class (recursive)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMClassRecursive : ComposedCommand
    {
        [PublicArgument("PSMClass", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
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

        internal override void GenerateSubCommands()
        {
            PSMClass psmClass = Project.TranslateComponent<PSMClass>(ClassGuid);
            foreach (PSMAssociation a in psmClass.ChildPSMAssociations)
            {
                cmdDeletePSMAssociationRecursive da = new cmdDeletePSMAssociationRecursive(Controller);
                da.Set(a);
                Commands.Add(da);
            }
            Commands.Add(new cmdDeleteRootPSMClass(Controller) { ClassGuid = ClassGuid });
        }

        public override bool CanExecute()
        {
            if (ClassGuid == Guid.Empty) return false;
            if (Project.TranslateComponent<PSMClass>(ClassGuid).ParentAssociation != null)
            {
                ErrorDescription = CommandErrors.CMDERR_CLASS_NOT_ROOT;
                return false;
            }
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PSM_CLASS_RECURSIVE);
        }

    }
}
