using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdNewPIMClass : AtomicCommand
    {
        private Guid schemaGuid;

        private Guid classGuid = Guid.Empty;

        /// <summary>
        /// If set before execution, creates a new class with this GUID.
        /// After execution contains GUID of the created class.
        /// </summary>
        public Guid ClassGuid
        {
            get { return classGuid; }
            set
            {
                if (!Executed) classGuid = value;
                else throw new ExolutioCommandException("Cannot set ClassGuid after command execution.", this);
            }
        }
        
        public acmdNewPIMClass(Controller c, Guid pimSchemaGuid)
            : base(c)
        {
            schemaGuid = pimSchemaGuid;
        }

        public override bool CanExecute()
        {
            if (schemaGuid != Guid.Empty && Project.VerifyComponentType<PIMSchema>(schemaGuid)) return true;
            ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
            return false;
        }
        
        internal override void CommandOperation()
        {
            if (ClassGuid == Guid.Empty) ClassGuid = Guid.NewGuid();
            PIMClass pimClass = new PIMClass(Project, ClassGuid, Project.TranslateComponent<PIMSchema>(schemaGuid));
            Report = new CommandReport(CommandReports.PIM_component_added, pimClass);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMClass c = Project.TranslateComponent<PIMClass>(ClassGuid);
            Project.TranslateComponent<PIMSchema>(schemaGuid).PIMClasses.Remove(c);
            Project.mappingDictionary.Remove(ClassGuid);
            return OperationResult.OK;
        }
    }
}
