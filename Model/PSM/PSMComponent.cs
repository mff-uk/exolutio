using System;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.PIM;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model.PSM
{
    public abstract class PSMComponent : Component
    {
        protected PSMComponent(Project p) : base(p) { }
        
        protected PSMComponent(Project p, Guid g) : base(p, g) { }

        public PSMSchema PSMSchema
        {
            get { return (PSMSchema)Schema; }
        }

        public string SchemaQualifiedName
        {
            get { return string.Format("{0}:{1}", PSMSchema, this.Name); }
        }

        private Guid interpretationGuid;

        public PIMComponent Interpretation
        {
            get
            {
                return interpretationGuid == Guid.Empty ? null : Project.TranslateComponent<PIMComponent>(interpretationGuid);
            }
            set 
            {
                interpretationGuid = value == null ? Guid.Empty : value; NotifyPropertyChanged("Interpretation");
            }
        }

        public abstract string XPath { get; }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            if (Interpretation != null)
            {
                this.SerializeIDRef(Interpretation, "InterpretationID", parentNode, context);
            }
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            interpretationGuid = this.DeserializeIDRef("InterpretationID", parentNode, context, true);
        }
        #endregion

        #region Implementation of IExolutioCloneable

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMComponent copyPSMComponent = (PSMComponent) copyComponent;
            if (Interpretation != null)
            {
                copyPSMComponent.interpretationGuid = createdCopies.GetGuidForCopyOf(Interpretation);
            }
        }

        #endregion
    }
}