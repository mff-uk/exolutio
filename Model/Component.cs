using System;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model
{
    public abstract class Component : ExolutioVersionedObject
    {
        protected Component(Project p) : base(p) { }
        protected Component(Project p, Guid g) : base(p, g) {}

        private Guid schemaGuid;
        public Schema Schema
        {
            get 
            {
                return schemaGuid == Guid.Empty ? null : Project.TranslateComponent<Schema>(schemaGuid);
            }
            set
            {
                schemaGuid = value != null ? value : Guid.Empty;
            }
        }

        private string name = "defaultname";
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged("Name"); NotifyPropertyChanged("IsNamed"); }
        }

        public bool IsNamed
        {
            get { return !String.IsNullOrEmpty(Name); }
        }

        public override ProjectVersion ProjectVersion
        {
            get { return Schema.ProjectVersion; }
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);

            // serialize all values of Name, incl. null and empty (both as "")
            {
                XAttribute nameAttribute = new XAttribute("Name", SerializationContext.EncodeValue(Name));
                parentNode.Add(nameAttribute);
            }
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            this.schemaGuid = context.CurrentSchemaGuid;

            if (parentNode.Attribute("Name") != null)
            {
                this.Name = SerializationContext.DecodeString(parentNode.Attribute("Name").Value);
            }
            else
            {
                //this.Name = string.Empty;
            }

            this.schemaGuid = context.CurrentSchemaGuid;
        }
        #endregion

        #region Implementation of IExolutioCloneable

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);
            Component _copyComponent = (Component) copyComponent;
            _copyComponent.schemaGuid = createdCopies.GetGuidForCopyOf(Schema);
            _copyComponent.Name = this.Name;
        }

        #endregion
    }
}
