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
    [PublicCommand("Add selected associations to content model (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdContentToContentModel : MacroCommand
    {
        [PublicArgument("Parent PSM Association Member", typeof(PSMAssociationMember))]
        [Scope(ScopeAttribute.EScope.PSMClass | ScopeAttribute.EScope.PSMContentModel | ScopeAttribute.EScope.PSMSchemaClass)]
        public Guid ParentAssociationMemberGuid { get; set; }

        [PublicArgument("Associations", typeof(PSMAssociation))]
        [ConsistentWith("ParentAssociationMemberGuid", PSMAssociationMemberParameterConsistency.Key)]
        public List<Guid> Associations { get; set; }

        [PublicArgument("Type", SuggestedValue = PSMContentModelType.Sequence)]
        public PSMContentModelType Type { get; set; }

        /// <summary>
        /// Preffered Guid of the new PSM content model
        /// </summary>
        public Guid ContentModelGuid { get; set; }

        /// <summary>
        /// Preffered Guid of the new PSM association
        /// </summary>
        public Guid AssociationGuid { get; set; }

        public cmdContentToContentModel()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdContentToContentModel(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid parentPSMAssociationMemberGuid, IEnumerable<Guid> psmAssociations, PSMContentModelType type, Guid newContentModelGuid, Guid newAssociationGuid)
        {
            ParentAssociationMemberGuid = parentPSMAssociationMemberGuid;
            Associations = psmAssociations.ToList();
            Type = type;
            ContentModelGuid = newContentModelGuid;
            AssociationGuid = newAssociationGuid;
        }

        protected override void GenerateSubCommands()
        {
            if (ContentModelGuid == Guid.Empty) ContentModelGuid = Guid.NewGuid();
            if (AssociationGuid == Guid.Empty) AssociationGuid = Guid.NewGuid();

            PSMAssociationMember parent = Project.TranslateComponent<PSMAssociationMember>(ParentAssociationMemberGuid);
            IEnumerable<PSMAssociation> associations = Project.TranslateComponentCollection<PSMAssociation>(Associations);

            Commands.Add(new acmdNewPSMContentModel(Controller, Type, parent.PSMSchema) { ContentModelGuid = ContentModelGuid });
            Commands.Add(new acmdNewPSMAssociation(Controller, parent, ContentModelGuid, parent.PSMSchema) { AssociationGuid = AssociationGuid });
            foreach (PSMAssociation a in associations)
            {
                Commands.Add(new acmdReconnectPSMAssociation(Controller, a, ContentModelGuid));
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
            Report = new CommandReport(CommandReports.COMPLEX_NEW_PSM_CM);
            base.CommandOperation();
        }
        
    }
}
