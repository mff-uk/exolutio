using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdNewPSMGeneralization : AtomicCommand
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

        public acmdNewPSMGeneralization(Controller c, Guid generalGuid, Guid specificGuid, Guid psmSchemaGuid)
            : base(c)
        {
            schemaGuid = psmSchemaGuid;
            generalClassGuid = generalGuid;
            specificClassGuid = specificGuid;
        }

        public override bool CanExecute()
        {
            if (generalClassGuid != Guid.Empty
                && Project.VerifyComponentType<PSMClass>(generalClassGuid)
                && specificClassGuid != Guid.Empty
                && Project.VerifyComponentType<PSMClass>(specificClassGuid)
                && schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PSMSchema>(schemaGuid))
            {
                PSMClass specific = Project.TranslateComponent<PSMClass>(specificClassGuid);
                PSMClass general = Project.TranslateComponent<PSMClass>(generalClassGuid);
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
            new PSMGeneralization(
                Project,
                GeneralizationGuid,
                Project.TranslateComponent<PSMClass>(generalClassGuid),
                Project.TranslateComponent<PSMClass>(specificClassGuid),
                Project.TranslateComponent<PSMSchema>(schemaGuid));
            Report = new CommandReport(CommandReports.PIM_component_added, Project.TranslateComponent<PSMGeneralization>(GeneralizationGuid));
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMGeneralization g = Project.TranslateComponent<PSMGeneralization>(GeneralizationGuid);
            PSMSchema s = Project.TranslateComponent<PSMSchema>(schemaGuid);
            g.General.GeneralizationsAsGeneral.Remove(g);
            g.Specific.GeneralizationAsSpecific = null;
            s.PSMGeneralizations.Remove(g);
            Project.mappingDictionary.Remove(g);

            return OperationResult.OK;
        }
    }
}
