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
    public class acmdDeleteOCLScript : AtomicCommand
    {
        Guid deletedClassGuid, schemaGuid;

        public acmdDeleteOCLScript(Controller c, Guid pimClassGuid)
            : base(c)
        {
            deletedClassGuid = pimClassGuid;
            schemaGuid = Project.TranslateComponent<PIMClass>(deletedClassGuid).PIMSchema;
        }

        public override bool CanExecute()
        {
            if (!(deletedClassGuid != Guid.Empty
                && schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PIMSchema>(schemaGuid)
                && Project.VerifyComponentType<PIMClass>(deletedClassGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            if (Project.TranslateComponent<PIMClass>(deletedClassGuid).PIMAssociationEnds.Count > 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CLASS_HAS_ASSOCIATIONS;
                return false;
            }
            if (Project.TranslateComponent<PIMClass>(deletedClassGuid).PIMAttributes.Count > 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CLASS_HAS_ATTRIBUTES;
                return false;
            }
            return true;
        }

        internal override void CommandOperation()
        {
            PIMClass c = Project.TranslateComponent<PIMClass>(deletedClassGuid);
            Report = new CommandReport(CommandReports.PIM_component_deleted, c);
            Project.TranslateComponent<PIMSchema>(schemaGuid).PIMClasses.Remove(c);
            Project.mappingDictionary.Remove(deletedClassGuid);
        }
        
        internal override OperationResult UndoOperation()
        {
            new PIMClass(Project, deletedClassGuid, Project.TranslateComponent<PIMSchema>(schemaGuid));
            return OperationResult.OK;
        }

        internal override MacroCommand PrePropagation()
        {
            IEnumerable<CommandBase> deleteFromDiagrams =
                acmdRemoveComponentFromDiagram.CreateCommandsToRemoveFromAllDiagrams(Controller, deletedClassGuid);

            List<PSMClass> list = Project.TranslateComponent<PIMClass>(deletedClassGuid).GetInterpretedComponents().Cast<PSMClass>().ToList<PSMClass>();
            if (list.Count == 0 && deleteFromDiagrams.Count() == 0) return null;

            MacroCommand command = new MacroCommand(Controller);
            command.Report = new CommandReport("Pre-propagation (delete PIM class)");
            
            command.Commands.AddRange(deleteFromDiagrams);

            if (list.Count > 0)
            {
                List<PSMClass> list2 = new List<PSMClass>(list);
                foreach (PSMClass c1 in list2)
                {
                    foreach (PSMClass c2 in list2)
                    {
                        if (c1.IsDescendantFrom(c2)) list.Remove(c1);
                    }
                }

                foreach (PSMClass c in list)
                {
                    cmdDeletePSMClassAndParent d = new cmdDeletePSMClassAndParent(Controller) {Propagate = false};
                    d.Set(c);
                    command.Commands.Add(d);
                }
            }
            return command;
        }
    }
}
