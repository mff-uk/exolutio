using System;
using EvoX.Model.PSM;
using EvoX.Model.Versioning;

namespace EvoX.Model.PIM
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
        
        #region Implementation of IEvoXCloneable

        public override void FillCopy(IEvoXCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PIMComponent copyPIMComponent = (PIMComponent) copyComponent;
            // TODO: FillCopy Interpreted components
        }

        #endregion
    }
}