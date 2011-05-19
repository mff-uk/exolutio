using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PIM
{
    public class acmdNewPIMAssociation : StackedCommand
    {
        private Guid schemaGuid;

        private Guid associationGuid = Guid.Empty;

        private Guid class1Guid = Guid.Empty;

        private Guid class2Guid = Guid.Empty;

        private Guid ae1Guid = Guid.Empty;

        private Guid ae2Guid = Guid.Empty;

        /// <summary>
        /// If set before execution, creates a new association with this GUID.
        /// After execution contains GUID of the created association.
        /// </summary>
        public Guid AssociationGuid
        {
            get { return associationGuid; }
            set
            {
                if (!Executed) associationGuid = value;
                else throw new EvoXCommandException("Cannot set AssociationGuid after command execution.", this);
            }
        }

        public acmdNewPIMAssociation(Controller c, Guid pimClass1Guid, Guid pimClass2Guid, Guid pimSchemaGuid)
            : base(c)
        {
            schemaGuid = pimSchemaGuid;
            class1Guid = pimClass1Guid;
            class2Guid = pimClass2Guid;
            ae1Guid = Guid.NewGuid();
            ae2Guid = Guid.NewGuid();
        }

        public acmdNewPIMAssociation(Controller c, Guid pimClass1Guid, Guid assocEnd1Guid, Guid pimClass2Guid, Guid assocEnd2Guid, Guid pimSchemaGuid)
            : base(c)
        {
            schemaGuid = pimSchemaGuid;
            class1Guid = pimClass1Guid;
            class2Guid = pimClass2Guid;
            ae1Guid = assocEnd1Guid;
            ae2Guid = assocEnd2Guid;
        }

        public override bool CanExecute()
        {
            if (class1Guid != Guid.Empty
                && Project.VerifyComponentType<PIMClass>(class1Guid)
                && class2Guid != Guid.Empty
                && Project.VerifyComponentType<PIMClass>(class2Guid)
                && schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PIMSchema>(schemaGuid)
                && ae1Guid != Guid.Empty
                && ae2Guid != Guid.Empty) return true;
            ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
            return false;
        }

        internal override void CommandOperation()
        {
            if (AssociationGuid == Guid.Empty) AssociationGuid = Guid.NewGuid();
            new PIMAssociation(
                Project,
                AssociationGuid,
                Project.TranslateComponent<PIMSchema>(schemaGuid),
                new KeyValuePair<PIMClass, Guid>(Project.TranslateComponent<PIMClass>(class1Guid), ae1Guid),
                new KeyValuePair<PIMClass, Guid>(Project.TranslateComponent<PIMClass>(class2Guid), ae2Guid)
                );
            Report = new CommandReport(CommandReports.PIM_component_added, Project.TranslateComponent<PIMAssociation>(AssociationGuid));
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAssociation a = Project.TranslateComponent<PIMAssociation>(AssociationGuid);
            PIMSchema s = Project.TranslateComponent<PIMSchema>(schemaGuid);
            foreach (PIMAssociationEnd e in a.PIMAssociationEnds)
            {
                s.PIMAssociationEnds.Remove(e);
                if (e.PIMClass != null) e.PIMClass.PIMAssociationEnds.Remove(e);
                Project.mappingDictionary.Remove(e);
            }
            s.PIMAssociations.Remove(a);
            Project.mappingDictionary.Remove(a);

            return OperationResult.OK;
        }
    }
}
