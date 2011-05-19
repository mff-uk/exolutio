using System;
using EvoX.Model.Versioning;

namespace EvoX.Model
{
    public abstract class EvoXVersionedObject : EvoXObject, IVersionedItem
    {
        protected EvoXVersionedObject(Project p) : base(p)
        {
        }

        protected EvoXVersionedObject(Project p, Guid g) : base(p, g)
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