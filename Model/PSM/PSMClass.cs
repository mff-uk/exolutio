using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model.PSM
{
    public class PSMClass : PSMAssociationMember, IPSMSemanticComponent
    {
        public PSMClass(Project p) : base(p)
        {
            InitializeCollections(); 
        }

        public PSMClass(Project p, Guid g) : base(p, g)
        {
            InitializeCollections();
        }

        public PSMClass(Project p, PSMSchema schema, bool isRoot) : base(p)
        {
            InitializeCollections();
            if (isRoot)
            {
                schema.Roots.Add(this);
            }
            schema.PSMClasses.Add(this);
        }

        public PSMClass(Project p, PSMSchema schema, int rootIndex = -1)
            : base(p)
        {
            InitializeCollections();
            if (rootIndex == -1)
            {
                schema.Roots.Add(this);
            }
            else
            {
                schema.Roots.Insert(this, rootIndex);
            }
            schema.PSMClasses.Add(this);
        }

        public PSMClass(Project p, Guid g, PSMSchema schema, int rootIndex = -1)
            : base(p, g)
        {
            InitializeCollections();
            if (rootIndex == -1)
            {
                schema.Roots.Add(this);
            }
            else
            {
                schema.Roots.Insert(this, rootIndex);
            }
            schema.PSMClasses.Add(this);
        }

        private void InitializeCollections()
        {
            PSMAttributes =new UndirectCollection<PSMAttribute>(Project);
            GeneralizationsAsGeneral = new UndirectCollection<PSMGeneralization>(Project);
        }

        private Guid representedClassGuid;

        /// <summary>
        /// Gets the PSM Class represented by this structural representative. Returns
        /// <c>null</c> for classes that are not structural representatives.
        /// </summary>
        public PSMClass RepresentedClass
        {
            get
            {
                return representedClassGuid == Guid.Empty ? null : Project.TranslateComponent<PSMClass>(representedClassGuid);
            }
            set 
            {
                // TODO: class must be from the same schema or from a referenced schema
                representedClassGuid = value == null ? Guid.Empty : value; NotifyPropertyChanged("RepresentedClass"); NotifyPropertyChanged("IsStructuralRepresentative");
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the class is a structural representative of another class
        /// </summary>
        /// <seealso cref="RepresentedClass"/>
        /// <seealso cref="HasStructuralRepresentatives"/>
        public bool IsStructuralRepresentative
        {
            get { return RepresentedClass != null; }
        }

        /// <summary>
        /// Returns <c>true</c> if structural representatives of this class exist (may also reside in other 
        /// <see cref="PSMSchema"/>s).
        /// </summary>
        public bool HasStructuralRepresentatives
        {
            get { return Project.mappingDictionary.Values.OfType<PSMClass>().Any(c => c.RepresentedClass == this); }
        }

        public ReadOnlyCollection<PSMClass> Representants
        {
            get
            {
                List<PSMClass> reprs = new List<PSMClass>();

                foreach (PSMSchema s in ProjectVersion.PSMSchemas)
                {
                    foreach (PSMClass c in s.PSMClasses)
                    {
                        if (c.RepresentedClass == this) reprs.Add(c);
                    }
                }
                
                return reprs.AsReadOnly();
            }
        }

        public UndirectCollection<PSMAttribute> PSMAttributes { get; private set; }

        public UndirectCollection<PSMGeneralization> GeneralizationsAsGeneral { get; private set; }

        private Guid generalizationAsSpecificGuid;
        public PSMGeneralization GeneralizationAsSpecific
        {
            get { return generalizationAsSpecificGuid == Guid.Empty ? null : Project.TranslateComponent<PSMGeneralization>(generalizationAsSpecificGuid); }
            set { generalizationAsSpecificGuid = value == null ? Guid.Empty : value; NotifyPropertyChanged("GeneralizationAsSpecific"); }
        }

        private bool final = false;
        public bool Final
        {
            get { return final; }
            set { final = value; NotifyPropertyChanged("Final"); }
        }

        private bool abstr = false;
        public bool Abstract
        {
            get { return abstr; }
            set { abstr = value; NotifyPropertyChanged("Abstract"); }
        }
        
        public override string XPath
        {
            get { return ParentAssociation.XPath; }
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            if (IsStructuralRepresentative)
            {
                XElement representsElement = new XElement(context.ExolutioNS + "RepresentedClass");
                this.SerializeIDRef(RepresentedClass, "representedPSMClassId", representsElement, context);
                parentNode.Add(representsElement);
            }
            this.WrapAndSerializeIDRefCollection("GeneralizationsAsGeneral", "PSMGeneralization", "psmGeneralizationsAsGeneralID", GeneralizationsAsGeneral,
                                                 parentNode, context);
            if (GeneralizationAsSpecific != null) this.SerializeIDRef(GeneralizationAsSpecific, "psmGeneralizationAsSpecificID", parentNode, context);

            this.WrapAndSerializeCollection("PSMAttributes", "PSMAttribute", PSMAttributes, parentNode, context);
            this.SerializeSimpleValueToAttribute("abstract", Abstract, parentNode, context);
            this.SerializeSimpleValueToAttribute("final", Final, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            XElement representedClassElement = parentNode.Element(context.ExolutioNS + "RepresentedClass");
            if (representedClassElement != null)
            {
                representedClassGuid = this.DeserializeIDRef("representedPSMClassId", representedClassElement, context, true);
            }
            
            this.DeserializeWrappedIDRefCollection("GeneralizationsAsGeneral", "psmGeneralizationsAsGeneralID", GeneralizationsAsGeneral, parentNode, context);
            generalizationAsSpecificGuid = this.DeserializeIDRef("psmGeneralizationAsSpecificID", parentNode, context, true);
            this.DeserializeWrappedCollection("PSMAttributes", PSMAttributes, PSMAttribute.CreateInstance, parentNode, context);

            bool succeeded, result;
            result = Boolean.TryParse(this.DeserializeSimpleValueFromAttribute("abstract", parentNode, context, true), out succeeded);
            Abstract = succeeded ? result : false;
            result = Boolean.TryParse(this.DeserializeSimpleValueFromAttribute("final", parentNode, context, true), out succeeded);
            Final = succeeded ? result : false;
        }
        public static PSMClass CreateInstance(Project project)
        {
            return new PSMClass(project, Guid.Empty);
        }

        #endregion

        public override string ToString()
        {
            return "PSMClass: \"" + Name + "\"";
        }

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PSMClass(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMClass copyPSMClass = (PSMClass) copyComponent;
            if (RepresentedClass != null)
            {
                copyPSMClass.representedClassGuid = createdCopies.GetGuidForCopyOf(RepresentedClass);
            }
            this.CopyCollection(PSMAttributes, copyPSMClass.PSMAttributes, projectVersion, createdCopies);
            copyPSMClass.Abstract = this.Abstract;
            copyPSMClass.Final = this.Final;

        }

        #endregion
    }
}