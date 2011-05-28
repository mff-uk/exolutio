using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.OCL;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model
{
    public abstract class Schema : ExolutioVersionedObjectNotAPartOfSchema
    {
        protected Schema(Project p) : base(p) { InitializeCollections(); }
        protected Schema(Project p, Guid g) : base(p, g) { InitializeCollections(); }

        public void InitializeCollections()
        {
            OCLScripts = new UndirectCollection<OCLScript>(Project);
        }

        protected abstract string DefaultString { get; }

        private string caption;
        public string Caption
        {
            get { return !string.IsNullOrEmpty(caption) ? caption : DefaultString; }
            set { caption = value; }
        }

        public abstract IEnumerable<Component> SchemaComponents { get; }

        public UndirectCollection<OCLScript> OCLScripts { get; private set; }

        #region Implementation of IExolutioCloneable

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            Schema copySchema = (Schema) copyComponent;

            copySchema.SetProjectVersion(projectVersion);
        }

        #endregion

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);

            XAttribute captionAttribute = new XAttribute("Caption", Caption);
            parentNode.Add(captionAttribute);
        }

        protected void SerializeRemaining(XElement parentNode, SerializationContext context)
        {
            this.WrapAndSerializeCollection("OCLScripts", "OCLScript", OCLScripts, parentNode, context, skipEmpty: true);
        }

        protected void DeserializeRemaining(XElement parentNode, SerializationContext context)
        {
            this.DeserializeWrappedCollection("OCLScripts", OCLScripts, OCLScript.CreateInstance, parentNode, context, missingAsEmpty: true);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            if (parentNode.Attribute("Caption") != null)
            {
                Caption = SerializationContext.DecodeString(parentNode.Attribute("Caption").Value);
            }

            SetProjectVersion(context.CurrentProjectVersion);
        }
        #endregion

        public override string ToString()
        {
            return Caption;
        }
    }
}