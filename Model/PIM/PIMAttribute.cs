using System;
using System.Xml;
using System.Xml.Linq;
using EvoX.Model.Serialization;
using EvoX.Model.Versioning;
using EvoX.Model.PSM;

namespace EvoX.Model.PIM
{
    public class PIMAttribute : PIMComponent, IHasCardinality
    {
        #region Constructors
        public PIMAttribute(Project p) : base(p) { }
        public PIMAttribute(Project p, Guid g) : base(p, g) { }
        /// <summary>
        /// Constructs a PIMAttribute and registers it with schema 
        /// <paramref name="schema"/> and inserts it into <see cref="PIMClass"/> 
        /// <paramref name="c"/>
        /// </summary>
        /// <param name="p">Project</param>
        /// <param name="c"><see cref="PIMClass"/> to insert to</param>
        /// <param name="schema"><see cref="PIMSchema"/> to register with
        /// </param>
        public PIMAttribute(Project p, PIMClass c, PIMSchema schema, int index = -1)
            : base(p)
        {
            schema.PIMAttributes.Add(this);
            if (index == -1)
                c.PIMAttributes.Add(this);
            else c.PIMAttributes.Insert(this, index);
            PIMClass = c;
        }
        /// <summary>
        /// Constructs a PIMAttribute and registers it with schema <paramref name="schema"/> and inserts it into <see cref="PIMClass"/> <paramref name="c"/>. Also sets <see cref="Guid"/> to <paramref name="g"/>
        /// </summary>
        /// <param name="p">Project</param>
        /// <param name="g"><see cref="Guid"/> to be set</param>
        /// <param name="c"><see cref="PIMClass"/> to insert to</param>
        /// <param name="schema"><see cref="PIMSchema"/> to register with</param>
        public PIMAttribute(Project p, Guid g, PIMClass c, PIMSchema schema, int index = -1)
            : base(p, g)
        {
            schema.PIMAttributes.Add(this);
            if (index == -1)
                c.PIMAttributes.Add(this);
            else c.PIMAttributes.Insert(this, index);
            PIMClass = c;
        }
        #endregion
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

        private Guid pimClassGuid;

        public PIMClass PIMClass
        {
            get { return Project.TranslateComponent<PIMClass>(pimClassGuid); }
            set { pimClassGuid = value; NotifyPropertyChanged("PIMClass"); }
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

        public override ComponentList<PSMComponent> GetInterpretedComponents()
        {
            ComponentList<PSMComponent> list = new ComponentList<PSMComponent>();
            foreach (PSMSchema schema in Project.LatestVersion.PSMSchemas)
            {
                foreach (PSMAttribute a in schema.PSMAttributes)
                {
                    if (a.Interpretation == this) list.Add(a);
                }
            }
            return list;
        }
        
        #region Implementation of IEvoXSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
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

            this.SerializeIDRef(PIMClass, "pimClassID", parentNode, context, false);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            this.DeserializeCardinality(parentNode, context);
            this.attributeTypeGuid = this.DeserializeAttributeType(parentNode, context);

            if (parentNode.Attribute("DefaultValue") != null)
            {
                DefaultValue = SerializationContext.DecodeString(parentNode.Attribute("DefaultValue").Value);
            }

            pimClassGuid = this.DeserializeIDRef("pimClassID", parentNode, context, false);

            this.PIMSchema.PIMAttributes.Add(this);
        }
        public static PIMAttribute CreateInstance(Project project)
        {
            return new PIMAttribute(project, Guid.Empty);
        }

        #endregion

        public override string ToString()
        {

            return "PIMAttribute: " + (pimClassGuid == Guid.Empty ? '"' + Name + '"' : '"'
                + PIMClass.Name + '.' + Name + '"') + " " + lower + ".." + upper; ;
        }

        #region Implementation of IEvoXCloneable

        public override IEvoXCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PIMAttribute(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IEvoXCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PIMAttribute copyPIMAttribute = (PIMAttribute)copyComponent;
            copyPIMAttribute.Lower = this.Lower;
            copyPIMAttribute.Upper = this.Upper;
            copyPIMAttribute.DefaultValue = this.DefaultValue;

            if (AttributeType != null)
            {
                copyPIMAttribute.attributeTypeGuid = createdCopies.GetGuidForCopyOf(AttributeType);
            }
            copyPIMAttribute.pimClassGuid = createdCopies.GetGuidForCopyOf(PIMClass);
        }

        #endregion
    }
}