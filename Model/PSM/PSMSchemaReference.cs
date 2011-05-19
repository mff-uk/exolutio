using System;
using System.Xml;
using System.Xml.Linq;
using EvoX.Model.Serialization;
using EvoX.Model.Versioning;

namespace EvoX.Model.PSM
{
    public class PSMSchemaReference : PSMComponent
    {
        public PSMSchemaReference(Project p) : base(p)
        {
        }

        public PSMSchemaReference(Project p, Guid g) : base(p, g)
        {
        }

        public PSMSchemaReference(Project p, Guid g, PSMSchema referencedPSMSchema)
            : base(p, g)
        {
            ReferencedPSMSchema = referencedPSMSchema;
        }

        public PSMSchemaReference(Project p, PSMSchema referencedPSMSchema)
            : base(p)
        {
            ReferencedPSMSchema = referencedPSMSchema;
        }

        public enum EReferenceType
        {
            Include, 
            Import
        }

        public EReferenceType ReferenceType { get; set; }

        public string Namespace { get; set; }

        public string SchemaLocation { get; set; }

        public string NamespacePrefix { get; set; }

        private Guid referencedSchemaGuid;

        public PSMSchema ReferencedPSMSchema
        {
            get
            {
                return referencedSchemaGuid == Guid.Empty ? null : Project.TranslateComponent<PSMSchema>(referencedSchemaGuid);
            }
            set
            {
                referencedSchemaGuid = value == null ? Guid.Empty : value; NotifyPropertyChanged("ReferencedPSMSchema");
            }
        }

        #region IEvoXSerializable Members

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            throw new NotImplementedException("Member PSMSchemaReference.Serialize not implemented.");
            //base.Serialize(parentNode, context);

            //this.SerializeIDRef(ReferencedPSMSchema, "ReferencedPSMSchema", parentNode, context);

            //XmlAttribute referenceTypeAttribute = context.Document.CreateAttribute("ReferenceType");
            //referenceTypeAttribute.Value = ReferenceType.ToString();
            //parentNode.Attributes.Append(referenceTypeAttribute);


            //if (!string.IsNullOrEmpty(Namespace))
            //{
            //    XmlElement namespaceElement = context.Document.CreateElement(SerializationContext.EvoXPrefix, "Namespace", SerializationContext.EvoXNamespace);
            //    XmlText namespaceTextNode = context.Document.CreateTextNode(Namespace);
            //    namespaceElement.AppendChild(namespaceTextNode);
            //    parentNode.AppendChild(namespaceElement);
            //}

            //if (!string.IsNullOrEmpty(SchemaLocation))
            //{
            //    XmlElement schemaLocationElement = context.Document.CreateElement(SerializationContext.EvoXPrefix, "SchemaLocation", SerializationContext.EvoXNamespace);
            //    XmlText schemaLocationTextNode = context.Document.CreateTextNode(Namespace);
            //    schemaLocationElement.AppendChild(schemaLocationTextNode);
            //    parentNode.AppendChild(schemaLocationElement);
            //}

            //if (!string.IsNullOrEmpty(NamespacePrefix))
            //{
            //    XmlElement namespacePrefixElement = context.Document.CreateElement(SerializationContext.EvoXPrefix, "NamespacePrefix", SerializationContext.EvoXNamespace);
            //    XmlText namespacePrefixTextElement = context.Document.CreateTextNode(NamespacePrefix);
            //    namespacePrefixElement.AppendChild(namespacePrefixTextElement);
            //    parentNode.AppendChild(namespacePrefixElement);
            //}
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            throw new NotImplementedException("Member PSMSchemaReference.Deserialize not implemented.");
            //base.Deserialize(parentNode, context);

            //referencedSchemaGuid = this.DeserializeIDRef("ReferencedPSMSchema", parentNode, context);

            //if (parentNode.HasAttribute("ReferenceType"))
            //{
            //    this.ReferenceType = SerializationContext.DecodeSchemaReferenceType(parentNode.Attributes["ReferenceType"].Value);
            //}

            //XmlNodeList nodeList = parentNode.GetElementsByTagName("Namespace", SerializationContext.EvoXNamespace);
            //if (nodeList.Count > 0)
            //{
            //    XmlElement namespaceElement = (XmlElement)nodeList[0];
            //    if (namespaceElement.HasChildNodes && namespaceElement.FirstChild is XmlText)
            //    {
            //        this.Namespace = namespaceElement.FirstChild.Value;
            //    }
            //}

            //XmlNodeList nodeList2 = parentNode.GetElementsByTagName("SchemaLocation", SerializationContext.EvoXNamespace);
            //if (nodeList2.Count > 0)
            //{
            //    XmlElement schemaLocationElement = (XmlElement)nodeList2[0];
            //    if (schemaLocationElement.HasChildNodes && schemaLocationElement.FirstChild is XmlText)
            //    {
            //        this.SchemaLocation = schemaLocationElement.FirstChild.Value;
            //    }
            //}

            //XmlNodeList nodeList3 = parentNode.GetElementsByTagName("NamespacePrefix", SerializationContext.EvoXNamespace);
            //if (nodeList.Count > 0)
            //{
            //    XmlElement namespacePrefixElement = (XmlElement)nodeList[0];
            //    if (namespacePrefixElement.HasChildNodes && namespacePrefixElement.FirstChild is XmlText)
            //    {
            //        this.Namespace = namespacePrefixElement.FirstChild.Value;
            //    }
            //}
        }

        public static PSMSchemaReference CreateInstance(Project project)
        {
            return new PSMSchemaReference(project, Guid.Empty);
        }

        #endregion

        #region Implementation of IEvoXCloneable

        public override IEvoXCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PSMSchemaReference(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IEvoXCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMSchemaReference copyPSMSchemaReference = (PSMSchemaReference)copyComponent;
            copyPSMSchemaReference.Namespace = this.Namespace;
            copyPSMSchemaReference.NamespacePrefix = this.NamespacePrefix;
            copyPSMSchemaReference.SchemaLocation = this.SchemaLocation;
            copyPSMSchemaReference.ReferenceType = this.ReferenceType;
            copyPSMSchemaReference.referencedSchemaGuid = createdCopies.GetGuidForCopyOf(ReferencedPSMSchema);
        }

        #endregion
    }
}