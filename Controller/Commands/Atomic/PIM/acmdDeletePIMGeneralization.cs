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
    internal class acmdDeletePIMGeneralization : AtomicCommand
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

        internal override PropagationMacroCommand PrePropagation()
        {
            PIMGeneralization g = Project.TranslateComponent<PIMGeneralization>(deletedGeneralizationGuid);
            IEnumerable<PSMAttribute> atts = Project.LatestVersion.PSMSchemas.SelectMany(s => s.PSMAttributes).Where(att => att.UsedGeneralizations.Contains(g));
            IEnumerable<PSMAssociation> assocs = Project.LatestVersion.PSMSchemas.SelectMany(s => s.PSMAssociations).Where(assoc => assoc.UsedGeneralizations.Contains(g));
            
            PropagationMacroCommand command = new PropagationMacroCommand(Controller);
            command.Report = new CommandReport("Pre-propagation (delete PIM generalization)");

            foreach (PSMAttribute a in atts)
            {
                command.Commands.Add(new cmdDeletePSMAttribute(Controller) { AttributeGuid = a });
            }

            foreach (PSMAssociation a in assocs)
            {
                command.Commands.Add(new cmdDeletePSMAssociation(Controller) { AssociationGuid = a });
            }
            
            return command;
        }
    }
}
