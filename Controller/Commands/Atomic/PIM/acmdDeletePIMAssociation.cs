using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using System.Diagnostics;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Complex.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    public class acmdDeletePIMAssociation : StackedCommand
    {
        public Guid AssociationGuid { get; set; }

        private Dictionary<Guid, Guid> endGuids;

        private List<Guid> classGuids;
        
        public acmdDeletePIMAssociation(Controller c, Guid pimAssociationGuid)
            : base(c)
        {
            AssociationGuid = pimAssociationGuid;
        }

        public override bool CanExecute()
        {
            if (AssociationGuid != Guid.Empty
                && Project.VerifyComponentType<PIMAssociation>(AssociationGuid)) return true;
            ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
            return false;
        }

        internal override void CommandOperation()
        {
            PIMAssociation a = Project.TranslateComponent<PIMAssociation>(AssociationGuid);
            PIMSchema s = a.PIMSchema;
            endGuids = new Dictionary<Guid, Guid>();
            classGuids = new List<Guid>();
            Debug.Assert(a.PIMClasses.Count == 2, "PIMAssociation " + a.Name + " has " + a.PIMClasses.Count + " PIMClasses on delete.");
            Report = new CommandReport(CommandReports.PIM_component_deleted, a);
            foreach (PIMAssociationEnd e in a.PIMAssociationEnds)
            {
                s.PIMAssociationEnds.Remove(e);
                if (e.PIMClass != null)
                {
                    e.PIMClass.PIMAssociationEnds.Remove(e);
                    classGuids.Add(e.PIMClass);
                    endGuids.Add(e.PIMClass, e);
                }
                Project.mappingDictionary.Remove(e);
            }
            s.PIMAssociations.Remove(a);
            Project.mappingDictionary.Remove(a);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            if (endGuids.Count != 2) return OperationResult.Failed;
            PIMClass class1 = Project.TranslateComponent<PIMClass>(classGuids[0]);
            PIMClass class2 = Project.TranslateComponent<PIMClass>(classGuids[1]);
            PIMSchema pimSchema = class1.PIMSchema;
            new PIMAssociation(
                Project,
                AssociationGuid,
                pimSchema,
                new KeyValuePair<PIMClass, Guid>(class1, endGuids[class1]),
                new KeyValuePair<PIMClass, Guid>(class2, endGuids[class2])
                );
            return OperationResult.OK;
        }

        internal override MacroCommand PrePropagation()
        {
            IEnumerable<CommandBase> deleteFromDiagrams =
                acmdRemoveComponentFromDiagram.CreateCommandsToRemoveFromAllDiagrams(Controller, AssociationGuid);
            
            List<PSMAssociation> list = Project.TranslateComponent<PIMAssociation>(AssociationGuid).GetInterpretedComponents().Cast<PSMAssociation>().ToList<PSMAssociation>();
            if (list.Count == 0 && deleteFromDiagrams.Count() == 0) return null;

            MacroCommand command = new MacroCommand(Controller);
            command.Report = new CommandReport("Pre-propagation (delete PIM association)");

            command.Commands.AddRange(deleteFromDiagrams);

            if (list.Count > 0)
            {
                List<PSMAssociation> list2 = new List<PSMAssociation>(list);
                foreach (PSMAssociation a1 in list2)
                {
                    foreach (PSMAssociation a2 in list2)
                    {
                        if (a1.IsDescendantFrom(a2)) list.Remove(a1);
                    }
                }

                foreach (PSMAssociation a in list)
                {
                    cmdDeletePSMAssociation d = new cmdDeletePSMAssociation(Controller) {Propagate = false};
                    d.Set(a);
                    command.Commands.Add(d);
                }
            }

            return command;
        }

    }
}
