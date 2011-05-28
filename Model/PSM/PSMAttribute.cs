using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model.PSM
{
    public class PSMAttribute : PSMComponent, IHasCardinality, IPSMSemanticComponent
    {
        public PSMAttribute(Project p) : base(p) { }
        public PSMAttribute(Project p, Guid g) : base(p, g) { }
        public PSMAttribute(Project p, PSMClass c, PSMSchema schema, int index = -1)
            : base(p)
        {
            schema.PSMAttributes.Add(this);
            if (index == -1)
            {
                c.PSMAttributes.Add(this);
            }
            else
            {
                c.PSMAttributes.Insert(this, index);
            }
            PSMClass = c;
        }
        public PSMAttribute(Project p, Guid g, PSMClass c, PSMSchema schema, int index = -1)
            : base(p, g)
        {
            schema.PSMAttributes.Add(this);
            if (index == -1)
            {
                c.PSMAttributes.Add(this);
            }
            else
            {
                c.PSMAttributes.Insert(this, index);
            }
            PSMClass = c;
        }

        private bool element;

        public bool Element
        {
            get { return element; }
            set { element = value; NotifyPropertyChanged("Element"); }
        }

        private Guid psmClassGuid;

        public PSMClass PSMClass
        {
            get { return psmClassGuid == null ? null : Project.TranslateComponent<PSMClass>(psmClassGuid); }
            set { psmClassGuid = value; NotifyPropertyChanged("PSMClass"); }
        }

        private Guid attributeTypeGuid;

        public AttributeType AttributeType
        {
            get
            {
                return attributeTypeGuid != Guid.Empty ? Project.TranslateComponent<AttributeType>(attributeTypeGuid) : null;
            }
            set
            {
                if (value != null)
                {
                    attributeTypeGuid = value;
                }
                else
                {
                    attributeTypeGuid = Guid.Empty;
                }
                NotifyPropertyChanged("AttributeType");
            }
        }

        private string defaultValue;
        public string DefaultValue
        {
            get { return defaultValue; }
            set
            {
                defaultValue = value;
                NotifyPropertyChanged("DefaultValue");
            }
        }

        #region IHasCardinality Members

        private uint lower = 1;
        public uint Lower
        {
            get { return lower; }
            set
            {
                lower = value; 
                NotifyPropertyChanged("Lower");
                NotifyPropertyChanged("CardinalityString");
            }
        }

        private UnlimitedInt upper = 1;
        public UnlimitedInt Upper
        {
            get { return upper; }
            set
            {
                upper = value; 
                NotifyPropertyChanged("Upper"); 
                NotifyPropertyChanged("CardinalityString");
            }
        }

        public string CardinalityString
        {
            get { return this.GetCardinalityString(); }
        }

        #endregion

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Serialize(parentNode, context);

            this.SerializeCardinality(parentNode, context);

            if (AttributeType != null)
            {
                this.SerializeAttributeType(AttributeType, parentNode, context);
            }

            if (!String.IsNullOrEmpty(DefaultValue))
            {
                XAttribute defaultValueAttribute = new XAttribute("DefaultValue", DefaultValue);
                parentNode.Add(defaultValueAttribute);
            }

            XAttribute elementAttribute = new XAttribute("Element", SerializationContext.EncodeValue(this.Element));
            parentNode.Add(elementAttribute);

            this.SerializeIDRef(PSMClass, "psmClassID", parentNode, context, false);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            this.DeserializeCardinality(parentNode, context);

            attributeTypeGuid = this.DeserializeAttributeType(parentNode, context);

            if (parentNode.Attribute("DefaultValue") != null)
            {
                DefaultValue = SerializationContext.DecodeString(parentNode.Attribute("DefaultValue").Value);
            }

            Element = SerializationContext.DecodeBool(parentNode.Attribute("Element").Value);

            psmClassGuid = this.DeserializeIDRef("psmClassID", parentNode, context, false);

            this.PSMSchema.PSMAttributes.Add(this);
        }

        public static PSMAttribute CreateInstance(Project project)
        {
            return new PSMAttribute(project, Guid.Empty);
        }

        #endregion

        public override string ToString()
        {
            return "PSMAttribute: " + (psmClassGuid == Guid.Empty ? '"' + Name + '"' : '"'
                + PSMClass.Name + '.' + Name + '"') + " " + lower + ".." + upper;
        }

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PSMAttribute(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMAttribute copyPSMAttribute = (PSMAttribute)copyComponent;
            copyPSMAttribute.Lower = this.Lower;
            copyPSMAttribute.Upper = this.Upper;
            copyPSMAttribute.Element = this.Element;
            copyPSMAttribute.DefaultValue = this.DefaultValue;

            copyPSMAttribute.psmClassGuid = createdCopies.GetGuidForCopyOf(PSMClass);
            if (AttributeType != null)
            {
                copyPSMAttribute.attributeTypeGuid = createdCopies.GetGuidForCopyOf(AttributeType);
            }
        }

        #endregion
    }
}