using System;
using EvoX.Model.Versioning;

namespace EvoX.Model.PSM
{
    public class PSMSchemaClass : PSMAssociationMember
    {
        public PSMSchemaClass(Project p) : base(p) { }
        public PSMSchemaClass(Project p, Guid g) : base(p, g) { }
        public PSMSchemaClass(Project p, PSMSchema schema)
            : base(p)
        {
            schema.Roots.Add(this);
            this.Schema = schema;
            schema.RegisterPSMSchemaClass(this);
        }
        public PSMSchemaClass(Project p, Guid g, PSMSchema schema)
            : base(p, g)
        {
            schema.Roots.Add(this);
            this.Schema = schema;
            schema.RegisterPSMSchemaClass(this);
        }

        public override string ToString()
        {
            return "PSMSchemaClass: " + Name;
        }

        #region Implementation of IEvoXCloneable

        public override IEvoXCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PSMSchemaClass(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IEvoXCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMSchemaClass copyPSMSchemaClass = (PSMSchemaClass) copyComponent;
        }

        #endregion
    }
}