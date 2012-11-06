using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model
{
    public class AttributeType : ExolutioVersionedObjectNotAPartOfSchema
    {
        public AttributeType(Project p)
            : base(p)
        {
            InitCollections();
        }

        private void InitCollections()
        {
            Operations = new UndirectCollection<ModelOperation>(Project);
        }

        public AttributeType(Project p, Guid g) : base(p, g) { InitCollections(); }

        /// <summary>
        /// Gets or sets a value indicating whether the type is defined by external 
        /// specification (e.g. XML Schema DataTypes) and should never be modified in the model.
        /// </summary>
        public bool IsSealed { get; set; }

        public string Name { get; set; }

        private Guid baseTypeGuid;

        /// <summary>
        /// Base type in the inheritance hierarchy
        /// </summary>
        public AttributeType BaseType
        {
            get { return baseTypeGuid != Guid.Empty ? Project.TranslateComponent<AttributeType>(baseTypeGuid) : null; }
            set
            {
                if (value != null)
                {
                    baseTypeGuid = value;
                }
                else
                {
                    baseTypeGuid = Guid.Empty;
                }
                NotifyPropertyChanged("BaseType");
            }
        }

        public string XSDDefinition { get; set; }

        public bool XSDAtomic { get; set; }

        public string XSDTypeName
        {
            get
            {
                if (XSDAtomic)
                {
                    return "xs:" + Name;
                }
                else
                {
                    return Name;
                }

            }
        }

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

        public bool IsModelType
        {
            get { return componentGuid != Guid.Empty; }
        }

        private Guid componentGuid;

        public Component Component
        {
            get
            {
                return componentGuid == Guid.Empty ? null : Project.TranslateComponent<Component>(componentGuid);
            }
            set
            {
                componentGuid = value != null ? value : Guid.Empty;
            }
        }

        public UndirectCollection<ModelOperation> Operations { get; private set; }

        private Guid representedPIMTypeGuid;
        public AttributeType RepresentedPIMType
        {
            get
            {
                return representedPIMTypeGuid == Guid.Empty ? null : Project.TranslateComponent<AttributeType>(representedPIMTypeGuid);
            }
            set
            {
                representedPIMTypeGuid = value != null ? value : Guid.Empty;
            }
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);

            XAttribute nameAttribute = new XAttribute("Name", SerializationContext.EncodeValue(Name));
            parentNode.Add(nameAttribute);

            XAttribute isSealedAttribute = new XAttribute("IsSealed", SerializationContext.EncodeValue(IsSealed));
            parentNode.Add(isSealedAttribute);

            if (BaseType != null)
            {
                this.SerializeIDRef(BaseType, "baseTypeID", parentNode, context);
            }

            if (Component != null)
            {
                this.SerializeIDRef(Component, "componentId", parentNode, context);
            }

            if (RepresentedPIMType != null)
            {
                this.SerializeIDRef(Component, "representedPIMTypeID", parentNode, context);
            }

            if (XSDDefinition != null)
            {
                XCData xsdDefinitionCData = new XCData(XSDDefinition);

                XElement xsdDefinitionElement = new XElement(context.ExolutioNS + "XSDDefinition");
                xsdDefinitionElement.Add(xsdDefinitionCData);
                parentNode.Add(xsdDefinitionElement);
            }

            if (XSDAtomic)
            {
                XAttribute xsdAtomicAttribute = new XAttribute("XSDAtomic", SerializationContext.EncodeValue(true));
                parentNode.Add(xsdAtomicAttribute);
            }

            this.WrapAndSerializeCollection("Operations", "Operation", Operations, parentNode, context, true);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            this.IsSealed = SerializationContext.DecodeBool(parentNode.Attribute("IsSealed").Value);
            this.Name = SerializationContext.DecodeString(parentNode.Attribute("Name").Value);
            baseTypeGuid = this.DeserializeIDRef("baseTypeID", parentNode, context, true);

            XAttribute baseTypeAttribute = parentNode.Attribute("baseType");
            if (baseTypeAttribute != null)
            {
                if (context.TypeDict != null && context.TypeDict.ContainsKey(baseTypeAttribute.Value))
                {
                    baseTypeGuid = context.TypeDict[baseTypeAttribute.Value];
                }
            }

            XAttribute xsdAtomicAttribute = parentNode.Attribute("xsdAtomic");
            if (xsdAtomicAttribute != null)
            {
                this.XSDAtomic = SerializationContext.DecodeBool(xsdAtomicAttribute.Value);
            }

            componentGuid = this.DeserializeIDRef("componentId", parentNode, context, true);
            representedPIMTypeGuid = this.DeserializeIDRef("representedPIMTypeID", parentNode, context, true);

            XElement xsdDefinitionElement = parentNode.Element(context.ExolutioNS + "XSDDefinition");
            if (xsdDefinitionElement != null)
            {
                this.XSDDefinition = ((XCData)xsdDefinitionElement.Nodes().First()).Value;
            }

            XAttribute xsdAtomicAtribute = parentNode.Attribute("XSDAtomic");
            if (xsdAtomicAtribute != null)
            {
                this.XSDAtomic = true;

            }

            XAttribute xsdDefinitionAttribute = parentNode.Attribute("XSDDefinition");
            if (xsdDefinitionAttribute != null)
            {
                this.XSDDefinition = xsdDefinitionAttribute.Value;
            }

            this.DeserializeWrappedCollection("Operations", Operations, ModelOperation.CreateInstance, parentNode, context, true);

            SetProjectVersion(context.CurrentProjectVersion);
        }

        public static AttributeType CreateInstance(Project project)
        {
            return new AttributeType(project, Guid.Empty);
        }

        #endregion

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new AttributeType(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            AttributeType copyAttributeType = (AttributeType)copyComponent;
            copyAttributeType.Name = this.Name;
            copyAttributeType.IsSealed = this.IsSealed;
            copyAttributeType.XSDDefinition = this.XSDDefinition;
            copyAttributeType.XSDAtomic = this.XSDAtomic;
            if (BaseType != null)
            {
                copyAttributeType.baseTypeGuid = createdCopies.GetGuidForCopyOf(BaseType);
            }
            if (Schema != null)
            {
                copyAttributeType.schemaGuid = createdCopies.GetGuidForCopyOf(Schema);
            }
            if (Component != null)
            {
                copyAttributeType.componentGuid = createdCopies.GetGuidForCopyOf(Component);
            }
            if (RepresentedPIMType != null)
            {
                copyAttributeType.representedPIMTypeGuid = RepresentedPIMType;
            }
            this.CopyCollection(Operations, copyAttributeType.Operations, projectVersion, createdCopies);
            copyAttributeType.SetProjectVersion(projectVersion);
        }

        #endregion

        public override string ToString()
        {
            return "AttributeType: " + Name;
        }
    }


}
