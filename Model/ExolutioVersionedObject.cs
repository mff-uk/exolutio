using System;
using Exolutio.Model.Versioning;

namespace Exolutio.Model
{
    public abstract class ExolutioVersionedObject : ExolutioObject, IVersionedItem
    {
        protected ExolutioVersionedObject(Project p) : base(p)
        {
        }

        protected ExolutioVersionedObject(Project p, Guid g) : base(p, g)
        {
        }

        #region IVersionedItem Members


        public Versioning.Version Version
        {
            get { return ProjectVersion.Version; }
        }

        public abstract ProjectVersion ProjectVersion { get; }

        #endregion
    }
}