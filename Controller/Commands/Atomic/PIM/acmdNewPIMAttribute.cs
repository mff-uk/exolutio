using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    public class acmdNewPIMAttribute : StackedCommand
    {
        private Guid schemaGuid;

        private Guid attributeGuid = Guid.Empty;

        private Guid classGuid = Guid.Empty;

        /// <summary>
        /// If set before execution, creates a new attribute with this GUID.
        /// After execution contains GUID of the created attribute.
        /// </summary>
        public Guid AttributeGuid
        {
            get { return attributeGuid; }
            set
            {
                if (!Executed) attributeGuid = value;
                else throw new ExolutioCommandException("Cannot set AttributeGuid after command execution.", this);
            }
        }

        public acmdNewPIMAttribute()
        {
        }

        public acmdNewPIMAttribute(Controller c, Guid pimClassGuid, Guid pimSchemaGuid)
            : base(c)
        {
            schemaGuid = pimSchemaGuid;
            classGuid = pimClassGuid;
        }

        public override bool CanExecute()
        {
            if (classGuid != Guid.Empty && Project.VerifyComponentType<PIMClass>(classGuid)
                && schemaGuid != Guid.Empty && Project.VerifyComponentType<PIMSchema>(schemaGuid)) return true;
            ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
            return false;
        }
        
        internal override void CommandOperation()
        {
            if (AttributeGuid == Guid.Empty) AttributeGuid = Guid.NewGuid();
            PIMClass pimClass = Project.TranslateComponent<PIMClass>(classGuid);
            PIMAttribute pimAttribute = new PIMAttribute(Project, AttributeGuid, pimClass, Project.TranslateComponent<PIMSchema>(schemaGuid));
            Report = new CommandReport(CommandReports.PIM_component_added, pimAttribute);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAttribute a = Project.TranslateComponent<PIMAttribute>(AttributeGuid);
            Project.TranslateComponent<PIMSchema>(schemaGuid).PIMAttributes.Remove(a);
            Project.TranslateComponent<PIMClass>(classGuid).PIMAttributes.Remove(a);
            Project.mappingDictionary.Remove(AttributeGuid);
            return OperationResult.OK;
        }
    }
}
