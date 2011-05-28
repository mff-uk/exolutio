using System;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;

namespace Exolutio.Model.PIM
{
    public abstract class PIMComponent : Component
    {
        protected PIMComponent(Project p) : base(p) { }

        protected PIMComponent(Project p, Guid g) : base(p, g) { }

        public PIMSchema PIMSchema
        {
            get { return (PIMSchema) Schema; }
        }

        public virtual ComponentList<PSMComponent> GetInterpretedComponents() { return new ComponentList<PSMComponent>(); }
        
        #region Implementation of IExolutioCloneable

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PIMComponent copyPIMComponent = (PIMComponent) copyComponent;
            // TODO: FillCopy Interpreted components
        }

        #endregion
    }
}