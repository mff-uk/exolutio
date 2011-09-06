using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Complex.PSM;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdDeletePSMGeneralization : AtomicCommand
    {
        Guid deletedGeneralizationGuid, schemaGuid, generalClass, specificClass;
        int index;

        public acmdDeletePSMGeneralization(Controller c, Guid psmGeneralizationGuid)
            : base(c)
        {
            deletedGeneralizationGuid = psmGeneralizationGuid;
            schemaGuid = Project.TranslateComponent<PSMGeneralization>(deletedGeneralizationGuid).PSMSchema;
        }

        public override bool CanExecute()
        {
            if (!(deletedGeneralizationGuid != Guid.Empty
                && schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PSMSchema>(schemaGuid)
                && Project.VerifyComponentType<PSMGeneralization>(deletedGeneralizationGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            return true;
        }

        internal override void CommandOperation()
        {
            PSMGeneralization g = Project.TranslateComponent<PSMGeneralization>(deletedGeneralizationGuid);
            Report = new CommandReport(CommandReports.PSM_component_deleted, g);
            specificClass = g.Specific;
            generalClass = g.General;
            g.Specific.GeneralizationAsSpecific = null;
            index = g.General.GeneralizationsAsGeneral.Remove(g);
            Project.TranslateComponent<PSMSchema>(schemaGuid).PSMGeneralizations.Remove(g);
            Project.mappingDictionary.Remove(deletedGeneralizationGuid);
        }
        
        internal override OperationResult UndoOperation()
        {
            new PSMGeneralization(Project, deletedGeneralizationGuid, Project.TranslateComponent<PSMClass>(generalClass), Project.TranslateComponent<PSMClass>(specificClass), Project.TranslateComponent<PSMSchema>(schemaGuid));
            return OperationResult.OK;
        }
    }
}
