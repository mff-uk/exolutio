using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdNewPSMSchema : StackedCommand
    {
        private Guid schemaGuid;

        private Guid schemaClassGuid;

        private Guid projectVersionGuid;

        public Guid ProjectVersionGuid
        {
            get { return projectVersionGuid; }
            set
            {
                if (!Executed) projectVersionGuid = value;
                else throw new ExolutioCommandException("Cannot set SchemaGuid after command execution.", this);
            }
        }

        /// <summary>
        /// If set before execution, creates a new schema with this GUID.
        /// After execution contains GUID of the created schema.
        /// </summary>
        public Guid SchemaGuid
        {
            get { return schemaGuid; }
            set
            {
                if (!Executed) schemaGuid = value;
                else throw new ExolutioCommandException("Cannot set SchemaGuid after command execution.", this);
            }
        }

        /// <summary>
        /// If set before execution, creates a new schemaClass with this GUID.
        /// After execution contains GUID of the created schemaClass.
        /// </summary>
        public Guid SchemaClassGuid
        {
            get { return schemaClassGuid; }
            set
            {
                if (!Executed) schemaClassGuid = value;
                else throw new ExolutioCommandException("Cannot set SchemaClassGuid after command execution.", this);
            }
        }


        public acmdNewPSMSchema(Controller c)
            : base(c)
        { }

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            if (SchemaGuid == Guid.Empty) SchemaGuid = Guid.NewGuid();
            if (SchemaClassGuid == Guid.Empty) SchemaClassGuid = Guid.NewGuid();

            PSMSchema psmSchema;

            if (!Project.UsesVersioning)
            {
                psmSchema = new PSMSchema(Project, SchemaGuid, Project.SingleVersion);
            }
            else
            {
                psmSchema = new PSMSchema(Project, SchemaGuid, Project.TranslateComponent<ProjectVersion>(projectVersionGuid));
            }

            new PSMSchemaClass(Project, SchemaClassGuid, psmSchema);
            Report = new CommandReport(CommandReports.PSM_component_added, psmSchema);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMSchema s = Project.TranslateComponent<PSMSchema>(SchemaGuid);
            s.UnRegisterPSMSchemaClass(s.PSMSchemaClass);
            ProjectVersion schemaVersion = s.ProjectVersion;
            schemaVersion.PSMSchemas.Remove(s);
            Project.mappingDictionary.Remove(SchemaClassGuid);
            Project.mappingDictionary.Remove(SchemaGuid);
            return OperationResult.OK;
        }
    }
}
