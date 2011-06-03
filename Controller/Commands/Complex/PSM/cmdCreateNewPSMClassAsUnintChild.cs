using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("Add PSM class as uninterpreted child (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdCreateNewPSMClassAsUnintChild : MacroCommand
    {
        [PublicArgument("Parent PSM class", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
        public Guid ParentPSMClassGuid { get; set; }

        /// <summary>
        /// Preffered Guid of the new PSM class
        /// </summary>
        public Guid ClassGuid { get; set; }

        /// <summary>
        /// Preffered Guid of the new PSM association
        /// </summary>
        public Guid AssociationGuid { get; set; }

        public cmdCreateNewPSMClassAsUnintChild()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdCreateNewPSMClassAsUnintChild(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid parentPSMClassGuid, Guid newClassGuid, Guid newAssociationGuid)
        {
            ParentPSMClassGuid = parentPSMClassGuid;
            ClassGuid = newClassGuid;
            AssociationGuid = newAssociationGuid;
        }

        protected override void GenerateSubCommands()
        {
            if (ClassGuid == Guid.Empty) ClassGuid = Guid.NewGuid();
            if (AssociationGuid == Guid.Empty) AssociationGuid = Guid.NewGuid();

            PSMClass parent = Project.TranslateComponent<PSMClass>(ParentPSMClassGuid);

            Commands.Add(new acmdNewPSMClass(Controller, parent.PSMSchema) { ClassGuid = ClassGuid });
            Commands.Add(new acmdNewPSMAssociation(Controller, parent, ClassGuid, parent.PSMSchema) { AssociationGuid = AssociationGuid });
        }

        public override bool CanExecute()
        {
            if (ParentPSMClassGuid == Guid.Empty) return false;
            if (!Project.VerifyComponentType<PSMClass>(ParentPSMClassGuid)) return false;
            return true;
        }

        internal override void CommandOperation()
        {
            Report = new CommandReport(CommandReports.COMPLEX_NEW_PSM_CLASS_AS_CHILD);
            base.CommandOperation();
        }
        
    }
}
