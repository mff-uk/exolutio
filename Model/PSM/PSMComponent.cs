using System;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM.XPath;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using System.Collections.Generic;

namespace Exolutio.Model.PSM
{
    public abstract class PSMComponent : Component
    {
        protected PSMComponent(Project p) : base(p) { InitializeCollections(); }

        protected PSMComponent(Project p, Guid g) : base(p, g) { InitializeCollections(); }

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

        public UndirectCollection<PIMGeneralization> UsedGeneralizations { get; private set; }

        public abstract string XPath { get; }

        public abstract Path GetXPathFull(bool followGeneralizations = true);

        private void InitializeCollections()
        {
            UsedGeneralizations = new UndirectCollection<PIMGeneralization>(Project);
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            if (Interpretation != null)
            {
                this.SerializeIDRef(Interpretation, "InterpretationID", parentNode, context);
            }
            this.WrapAndSerializeIDRefCollection("UsedGeneralizations", "PIMGeneralization", "usedGeneralizationsID", UsedGeneralizations,
                                         parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            interpretationGuid = this.DeserializeIDRef("InterpretationID", parentNode, context, true);
            this.DeserializeWrappedIDRefCollection("UsedGeneralizations", "usedGeneralizationsID", UsedGeneralizations, parentNode, context);
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