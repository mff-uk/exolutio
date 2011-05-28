using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Complex.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    public class acmdDeletePIMAttribute : StackedCommand
    {
        private Guid schemaGuid;

        private Guid classGuid = Guid.Empty;

        private int index = -1;

        private Guid attributeGuid = Guid.Empty;
        [PublicArgument("Deleted attribute")]
        public PIMAttribute PIMAttribute
        {
            get { return Project.TranslateComponent<PIMAttribute>(attributeGuid); }
            set { attributeGuid = value; }
        }

        public acmdDeletePIMAttribute()
        {
        }

        public acmdDeletePIMAttribute(Controller c, Guid pimAttributeGuid)
            : base(c)
        {
            attributeGuid = pimAttributeGuid;
            schemaGuid = Project.TranslateComponent<PIMAttribute>(attributeGuid).PIMSchema;
        }

        public override bool CanExecute()
        {
            if (attributeGuid != Guid.Empty && Project.VerifyComponentType<PIMAttribute>(attributeGuid)
                   && schemaGuid != Guid.Empty && Project.VerifyComponentType<PIMSchema>(schemaGuid))
                return true;
            ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
            return false;
        }
        
        internal override void CommandOperation()
        {
            PIMAttribute a = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            Report = new CommandReport(CommandReports.PIM_component_deleted, a);
            Project.TranslateComponent<PIMSchema>(schemaGuid).PIMAttributes.Remove(a);
            classGuid = a.PIMClass;
            index = Project.TranslateComponent<PIMClass>(classGuid).PIMAttributes.Remove(a);
            Project.mappingDictionary.Remove(attributeGuid);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            new PIMAttribute(Project, attributeGuid, Project.TranslateComponent<PIMClass>(classGuid), Project.TranslateComponent<PIMSchema>(schemaGuid), index);
            return OperationResult.OK;
        }

        internal override MacroCommand PrePropagation()
        {
            List<PSMAttribute> list = Project.TranslateComponent<PIMAttribute>(attributeGuid).GetInterpretedComponents().Cast<PSMAttribute>().ToList<PSMAttribute>();
            if (list.Count == 0) return null;

            MacroCommand command = new MacroCommand(Controller);
            command.Report = new CommandReport("Pre-propagation (delete PIM attribute)");

            foreach (PSMAttribute a in list)
            {
                cmdDeletePSMAttribute d = new cmdDeletePSMAttribute(Controller) { Propagate = false };
                d.Set(a);
                command.Commands.Add(d);
            }

            return command;
        }

    }
}
