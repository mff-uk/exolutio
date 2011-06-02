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
    /// <summary>
    /// Deletes root PSM class, its attributes and content associations
    /// </summary>
    [PublicCommand("Delete root PSM class, its attributes and content associations (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdDeleteRootPSMClass : MacroCommand
    {
        [PublicArgument("Deleted PSM class", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
        public Guid ClassGuid { get; set; }

        public cmdDeleteRootPSMClass()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeleteRootPSMClass(Controller c)
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
            foreach (PSMAttribute a in psmClass.PSMAttributes)
            {
                cmdDeletePSMAttribute da = new cmdDeletePSMAttribute(Controller);
                da.Set(a);
                Commands.Add(da);
            }
            foreach (PSMAssociation a in psmClass.ChildPSMAssociations)
            {
                cmdDeletePSMAssociation da = new cmdDeletePSMAssociation(Controller);
                da.Set(a);
                Commands.Add(da);
            }

            Commands.Add(new acmdRenameComponent(Controller, ClassGuid, ""));
            foreach (PSMClass representant in psmClass.Representants)
            {
                Commands.Add(new acmdSetRepresentedClass(Controller, representant, Guid.Empty));
            }
            Commands.Add(new acmdSetPSMClassInterpretation(Controller, ClassGuid, Guid.Empty));
            Commands.Add(new acmdDeletePSMClass(Controller, ClassGuid));
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
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PSM_CLASS);
        }

    }
}
