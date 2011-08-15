using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Complex.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    public class acmdDeletePIMGeneralization : StackedCommand
    {
        Guid deletedGeneralizationGuid, schemaGuid, generalClass, specificClass;
        int index;

        public acmdDeletePIMGeneralization(Controller c, Guid pimGeneralizationGuid)
            : base(c)
        {
            deletedGeneralizationGuid = pimGeneralizationGuid;
            schemaGuid = Project.TranslateComponent<PIMGeneralization>(deletedGeneralizationGuid).PIMSchema;
        }

        public override bool CanExecute()
        {
            if (!(deletedGeneralizationGuid != Guid.Empty
                && schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PIMSchema>(schemaGuid)
                && Project.VerifyComponentType<PIMGeneralization>(deletedGeneralizationGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            return true;
        }

        internal override void CommandOperation()
        {
            PIMGeneralization g = Project.TranslateComponent<PIMGeneralization>(deletedGeneralizationGuid);
            Report = new CommandReport(CommandReports.PIM_component_deleted, g);
            specificClass = g.Specific;
            generalClass = g.General;
            g.Specific.GeneralizationAsSpecific = null;
            index = g.General.GeneralizationsAsGeneral.Remove(g);
            Project.TranslateComponent<PIMSchema>(schemaGuid).PIMGeneralizations.Remove(g);
            Project.mappingDictionary.Remove(deletedGeneralizationGuid);
        }
        
        internal override OperationResult UndoOperation()
        {
            new PIMGeneralization(Project, deletedGeneralizationGuid, Project.TranslateComponent<PIMSchema>(schemaGuid), Project.TranslateComponent<PIMClass>(generalClass), Project.TranslateComponent<PIMClass>(specificClass));
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
