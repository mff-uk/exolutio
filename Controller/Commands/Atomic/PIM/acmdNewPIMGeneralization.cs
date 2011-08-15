using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdNewPIMGeneralization : StackedCommand
    {
        private Guid schemaGuid;

        private Guid generalizationGuid = Guid.Empty;

        private Guid generalClassGuid = Guid.Empty;

        private Guid specificClassGuid = Guid.Empty;

        /// <summary>
        /// If set before execution, creates a new association with this GUID.
        /// After execution contains GUID of the created association.
        /// </summary>
        public Guid GeneralizationGuid
        {
            get { return generalizationGuid; }
            set
            {
                if (!Executed) generalizationGuid = value;
                else throw new ExolutioCommandException("Cannot set GeneralizationGuid after command execution.", this);
            }
        }

        public acmdNewPIMGeneralization(Controller c, Guid generalGuid, Guid specificGuid, Guid pimSchemaGuid)
            : base(c)
        {
            schemaGuid = pimSchemaGuid;
            generalClassGuid = generalGuid;
            specificClassGuid = specificGuid;
        }

        public override bool CanExecute()
        {
            if (generalClassGuid != Guid.Empty
                && Project.VerifyComponentType<PIMClass>(generalClassGuid)
                && specificClassGuid != Guid.Empty
                && Project.VerifyComponentType<PIMClass>(specificClassGuid)
                && schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PIMSchema>(schemaGuid))
            {
                PIMClass specific = Project.TranslateComponent<PIMClass>(specificClassGuid);
                PIMClass general = Project.TranslateComponent<PIMClass>(generalClassGuid);
                if (specific.GeneralizationAsSpecific != null)
                {
                    ErrorDescription = CommandErrors.CMDERR_NO_MULTIPLE_INHERITANCE;
                    return false;
                }
                else if (specificClassGuid == generalClassGuid)
                {
                    ErrorDescription = CommandErrors.CMDERR_NO_SELF_INHERITANCE;
                    return false;
                }
                else if (general.GetGeneralClasses().Contains(specific))
                {
                    ErrorDescription = CommandErrors.CMDERR_CYCLIC_INHERITANCE;
                    return false;
                }
                else return true;
            }
            else
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
        }

        internal override void CommandOperation()
        {
            if (GeneralizationGuid == Guid.Empty) GeneralizationGuid = Guid.NewGuid();
            new PIMGeneralization(
                Project,
                GeneralizationGuid,
                Project.TranslateComponent<PIMSchema>(schemaGuid),
                Project.TranslateComponent<PIMClass>(generalClassGuid),
                Project.TranslateComponent<PIMClass>(specificClassGuid));
            Report = new CommandReport(CommandReports.PIM_component_added, Project.TranslateComponent<PIMGeneralization>(GeneralizationGuid));
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMGeneralization g = Project.TranslateComponent<PIMGeneralization>(GeneralizationGuid);
            PIMSchema s = Project.TranslateComponent<PIMSchema>(schemaGuid);
            g.General.GeneralizationsAsGeneral.Remove(g);
            g.Specific.GeneralizationAsSpecific = null;
            s.PIMGeneralizations.Remove(g);
            Project.mappingDictionary.Remove(g);

            return OperationResult.OK;
        }
    }
}
