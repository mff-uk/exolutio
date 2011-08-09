using System;

namespace Exolutio.Model
{
    public class ExolutioVersionedObjectNotAPartOfSchema : ExolutioVersionedObject
    {
        protected Guid projectVersionGuid;

        public ExolutioVersionedObjectNotAPartOfSchema(Project p) : base(p) {
        }

        public ExolutioVersionedObjectNotAPartOfSchema(Project p, Guid g)
            : base(p, g)
        {
        }

        public override ProjectVersion ProjectVersion
        {
            get
            {
                return projectVersionGuid == Guid.Empty ? null : Project.TranslateComponent<ProjectVersion>(projectVersionGuid);
            }
        }

        public void SetProjectVersion(ProjectVersion projectVersion)
        {
            ProjectVersion oldVersion = null;
            if (projectVersionGuid != Guid.Empty)
            {
                oldVersion = ProjectVersion;
                if (oldVersion != null && oldVersion.Project.UsesVersioning && oldVersion != projectVersion)
                {
                    oldVersion.Version.NotifyItemRemoved(this);
                }
            }

            projectVersionGuid = projectVersion != null ? projectVersion : Guid.Empty;
            if (projectVersion != null && projectVersion.Project.UsesVersioning && oldVersion != projectVersion)
            {
                projectVersion.Version.NotifyItemAdded(this);
            }
        }
    }
}