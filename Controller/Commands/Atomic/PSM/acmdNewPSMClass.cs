using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdNewPSMClass : StackedCommand
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
                else throw new EvoXCommandException("Cannot set ClassGuid after command execution.", this);
            }
        }

        public acmdNewPSMClass(Controller c, Guid psmSchemaGuid)
            : base(c)
        {
            schemaGuid = psmSchemaGuid;
        }

        public override bool CanExecute()
        {
            return schemaGuid != Guid.Empty && Project.VerifyComponentType<PSMSchema>(schemaGuid);
        }
        
        internal override void CommandOperation()
        {
            if (ClassGuid == Guid.Empty) ClassGuid = Guid.NewGuid();
            PSMClass psmClass = new PSMClass(Project, ClassGuid, Project.TranslateComponent<PSMSchema>(schemaGuid));
            Report = new CommandReport(CommandReports.PSM_component_added, psmClass);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMClass c = Project.TranslateComponent<PSMClass>(ClassGuid);
            Project.TranslateComponent<PSMSchema>(schemaGuid).Roots.Remove(c);
            Project.TranslateComponent<PSMSchema>(schemaGuid).PSMClasses.Remove(c);
            Project.mappingDictionary.Remove(ClassGuid);
            return OperationResult.OK;
        }
    }
}
