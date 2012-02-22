using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using Exolutio.Model.PSM;

namespace Exolutio.Model.PIM
{
    public class PIMClass : PIMComponent
    {
        #region Constructors
        public PIMClass(Project p)
            : base(p)
        {
            InitCollections();
        }
        public PIMClass(Project p, Guid g) : base(p, g) { InitCollections(); }
        /// <summary>
        /// Constructs a PIMClass and registers it with the PIMSchema <paramref name="Schema"/>
        /// </summary>
        /// <param name="P">Project to wich this PIMClass belongs to</param>
        /// <param name="Schema">PIMSchema</param>
        public PIMClass(Project p, PIMSchema schema)
            : base(p)
        {
            schema.PIMClasses.Add(this);
            InitCollections(); 
        }
        /// <summary>
        /// Constructs a PIMClass and registers it with the PIMSchema <paramref name="Schema"/>
        /// </summary>
        /// <param name="P">Project to wich this PIMClass belongs to</param>
        /// <param name="Schema">PIMSchema</param>
        public PIMClass(Project p, Guid g, PIMSchema schema)
            : base(p, g)
        {
            schema.PIMClasses.Add(this);
            InitCollections(); 
        }
        
        public void InitCollections()
        {
            PIMAssociationEnds = new UndirectCollection<PIMAssociationEnd>(Project);
            PIMAttributes = new UndirectCollection<PIMAttribute>(Project);
            PIMOperations = new UndirectCollection<ModelOperation>(Project);
            GeneralizationsAsGeneral = new UndirectCollection<PIMGeneralization>(Project);
        }

        #endregion

        public UndirectCollection<PIMAssociationEnd> PIMAssociationEnds { get; private set; }

        public UndirectCollection<PIMAttribute> PIMAttributes { get; private set; }

        public UndirectCollection<ModelOperation> PIMOperations { get; private set; }

        public UndirectCollection<PIMGeneralization> GeneralizationsAsGeneral { get; private set; }

        private Guid generalizationAsSpecificGuid;
        public PIMGeneralization GeneralizationAsSpecific
        {
            get { return generalizationAsSpecificGuid == Guid.Empty ? null : Project.TranslateComponent<PIMGeneralization>(generalizationAsSpecificGuid); }
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
        
        public override ComponentList<PSMComponent> GetInterpretedComponents()
        {
            ComponentList<PSMComponent> list = new ComponentList<PSMComponent>();
            foreach (PSMSchema schema in Project.LatestVersion.PSMSchemas)
            {
                foreach (PSMClass c in schema.PSMClasses)
                {
                    if (c.Interpretation == this) list.Add(c);
                }
            }
            return list;
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            this.WrapAndSerializeCollection("PIMAttributes", "PIMAttribute", PIMAttributes, parentNode, context);
            this.WrapAndSerializeCollection("PIMOperations", "PIMOperation", PIMOperations, parentNode, context);
            this.WrapAndSerializeIDRefCollection("GeneralizationAsGeneral", "PIMGeneralization", "pimGeneralizationsAsGeneralID", GeneralizationsAsGeneral,
                                                 parentNode, context);
            if (GeneralizationAsSpecific != null) this.SerializeIDRef(GeneralizationAsSpecific, "pimGeneralizationAsSpecificID", parentNode, context);
            this.WrapAndSerializeIDRefCollection("PIMAssociationEnds", "PIMAssociationEnd", "pimAssociationEndID", PIMAssociationEnds,
                                                 parentNode, context);
            this.SerializeSimpleValueToAttribute("abstract", Abstract, parentNode, context);
            this.SerializeSimpleValueToAttribute("final", Final, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            this.DeserializeWrappedCollection("PIMAttributes", PIMAttributes, PIMAttribute.CreateInstance, parentNode, context);
            this.DeserializeWrappedCollection("PIMOperations", PIMOperations, ModelOperation.CreateInstance, parentNode, context, true);
            this.DeserializeWrappedIDRefCollection("GeneralizationsAsGeneral", "pimGeneralizationsAsGeneralID", GeneralizationsAsGeneral, parentNode, context);
            generalizationAsSpecificGuid = this.DeserializeIDRef("pimGeneralizationAsSpecificID", parentNode, context, true);
            this.DeserializeWrappedIDRefCollection("PIMAssociationEnds", "pimAssociationEndID", PIMAssociationEnds, parentNode, context);
            
            bool succeeded, result;
            result = Boolean.TryParse(this.DeserializeSimpleValueFromAttribute("abstract", parentNode, context, true), out succeeded);
            Abstract = succeeded ? result : false;
            result = Boolean.TryParse(this.DeserializeSimpleValueFromAttribute("final", parentNode, context, true), out succeeded);
            Final = succeeded ? result : false;
        }

        public static PIMClass CreateInstance(Project project)
        {
            return new PIMClass(project, Guid.Empty);
        }

        #endregion

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PIMClass(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PIMClass copyPIMClass = (PIMClass)copyComponent;
            this.CopyCollection(PIMAttributes, copyPIMClass.PIMAttributes, projectVersion, createdCopies);
            this.CopyCollection(PIMOperations, copyPIMClass.PIMOperations, projectVersion, createdCopies);
            this.CopyRefCollection(GeneralizationsAsGeneral, copyPIMClass.GeneralizationsAsGeneral, projectVersion, createdCopies, true);
            if (GeneralizationAsSpecific != null)
            {
                copyPIMClass.generalizationAsSpecificGuid = createdCopies.GetGuidForCopyOf(GeneralizationAsSpecific);
            }
            this.CopyRefCollection(PIMAssociationEnds, copyPIMClass.PIMAssociationEnds, projectVersion, createdCopies, true);
            copyPIMClass.Abstract = this.Abstract;
            copyPIMClass.Final = this.Final;
        }

        #endregion

        public override string ToString()
        {
            return "PIMClass: \"" + Name + "\"";
        }
    }
}
