using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;
using Exolutio.Controller.Commands.Reflection;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("Content to PSM class (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdContentToPSMClass : ComposedCommand
    {
        [PublicArgument("Parent PSM Association Member", typeof(PSMAssociationMember))]
        [Scope(ScopeAttribute.EScope.PSMClass | ScopeAttribute.EScope.PSMContentModel | ScopeAttribute.EScope.PSMSchemaClass)]
        public Guid ParentAssociationMemberGuid { get; set; }

        [PublicArgument("Associations", typeof(PSMAssociation))]
        [ConsistentWith("ParentAssociationMemberGuid", PSMAssociationMemberParameterConsistency.Key)]
        public List<Guid> Associations { get; set; }

        /// <summary>
        /// Preffered Guid of the new PSM content model
        /// </summary>
        [GeneratedIDArgument("ClassGuid", typeof(PSMClass))]
        public Guid ClassGuid { get; set; }

        /// <summary>
        /// Preffered Guid of the new PSM association
        /// </summary>
        [GeneratedIDArgument("AssociationGuid", typeof(PSMAssociation))]
        public Guid AssociationGuid { get; set; }

        public cmdContentToPSMClass()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdContentToPSMClass(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid parentPSMAssociationMemberGuid, IEnumerable<Guid> psmAssociations, Guid newClassGuid, Guid newAssociationGuid)
        {
            ParentAssociationMemberGuid = parentPSMAssociationMemberGuid;
            Associations = psmAssociations.ToList();
            ClassGuid = newClassGuid;
            AssociationGuid = newAssociationGuid;
        }

        internal override void GenerateSubCommands()
        {
            if (ClassGuid == Guid.Empty) ClassGuid = Guid.NewGuid();
            if (AssociationGuid == Guid.Empty) AssociationGuid = Guid.NewGuid();

            PSMAssociationMember parent = Project.TranslateComponent<PSMAssociationMember>(ParentAssociationMemberGuid);
            IEnumerable<PSMAssociation> associations = Project.TranslateComponentCollection<PSMAssociation>(Associations);

            Commands.Add(new acmdNewPSMClass(Controller, parent.PSMSchema) { ClassGuid = ClassGuid });
            Commands.Add(new acmdNewPSMAssociation(Controller, parent, ClassGuid, parent.PSMSchema) { AssociationGuid = AssociationGuid });
            Commands.Add(new acmdRenameComponent(Controller, AssociationGuid, ""));
            foreach (PSMAssociation a in associations)
            {
                Commands.Add(new acmdReconnectPSMAssociation(Controller, a, ClassGuid));
            }

        }

        public override bool CanExecute()
        {
            if (ParentAssociationMemberGuid == Guid.Empty || Associations == null) return false;
            PSMAssociationMember parent = Project.TranslateComponent<PSMAssociationMember>(ParentAssociationMemberGuid);
            IEnumerable<PSMAssociation> associations = Project.TranslateComponentCollection<PSMAssociation>(Associations);
            if (associations.Any(a => a.Parent != parent)) return false;
            return true;
        }

        internal override void CommandOperation()
        {
            Report = new CommandReport(CommandReports.COMPLEX_INSERT_PSM_CLASS);
            base.CommandOperation();
        }

    }
}
