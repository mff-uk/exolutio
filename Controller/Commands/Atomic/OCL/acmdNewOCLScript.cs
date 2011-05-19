using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.OCL;

namespace EvoX.Controller.Commands.Atomic.PIM
{
    public class acmdNewOCLScript : StackedCommand
    {
        private Guid schemaGuid;

        private Guid oclScriptGuid = Guid.Empty;

        /// <summary>
        /// If set before execution, creates a new ocl script with this GUID.
        /// After execution contains GUID of the created ocl script.
        /// </summary>
        public Guid OCLScriptGuid
        {
            get { return oclScriptGuid; }
            set
            {
                if (!Executed) oclScriptGuid = value;
                else throw new EvoXCommandException("Cannot set OCLScriptGuid after command execution.", this);
            }
        }

        public acmdNewOCLScript(Controller c, Guid pimSchemaGuid)
            : base(c)
        {
            schemaGuid = pimSchemaGuid;
        }

        public override bool CanExecute()
        {
            if (schemaGuid != Guid.Empty && Project.VerifyComponentType<Schema>(schemaGuid)) return true;
            ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
            return false;
        }
        
        internal override void CommandOperation()
        {
            if (OCLScriptGuid == Guid.Empty) OCLScriptGuid = Guid.NewGuid();
            Schema schema = Project.TranslateComponent<Schema>(schemaGuid);
            OCLScript oclScript = new OCLScript(Project, OCLScriptGuid, schema);
            Report = new CommandReport("New OCL script was added to '{0}'.", schema);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            OCLScript oclScript = Project.TranslateComponent<OCLScript>(OCLScriptGuid);
            Project.TranslateComponent<Schema>(schemaGuid).OCLScripts.Remove(oclScript);
            Project.mappingDictionary.Remove(OCLScriptGuid);
            return OperationResult.OK;
        }
    }
}
