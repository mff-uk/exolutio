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
    public class cmdDeletePSMAssociationMember : MacroCommand
    {
        public Guid AssociationMemberGuid { get; set; }
        
        public cmdDeletePSMAssociationMember()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePSMAssociationMember(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid associationMemberGuid)
        {
            AssociationMemberGuid = associationMemberGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            PSMAssociationMember associationMember = Project.TranslateComponent<PSMAssociationMember>(AssociationMemberGuid);
            if (associationMember is PSMClass)
            {
                cmdDeletePSMClassRecursive dc = new cmdDeletePSMClassRecursive(Controller);
                dc.Set(associationMember);
                Commands.Add(dc);
            }
            else if (associationMember is PSMContentModel)
            {
                cmdDeletePSMContentModelRecursive dcm = new cmdDeletePSMContentModelRecursive(Controller);
                dcm.Set(associationMember);
                Commands.Add(dcm);
            }
            else if (associationMember is PSMSchemaClass)
            {
                //Happens only when deleting PSM Schema. We clear the SchemaClass and let the atomic operation "delete PSM Schema" delete the actual class
                foreach (PSMAssociation a in associationMember.ChildPSMAssociations)
                {
                    cmdDeletePSMAssociationRecursive da = new cmdDeletePSMAssociationRecursive(Controller);
                    da.Set(a);
                    Commands.Add(da);
                }
            }
        }

        public override bool CanExecute()
        {
            if (AssociationMemberGuid == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PSM_ASSOC_MEMBER);
        }

    }
}
