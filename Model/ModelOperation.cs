using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Exolutio.Model.PIM;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model
{
    public class ModelOperation : Component
    {
        #region Constructors
        public ModelOperation(Project p) : base(p) { InitializeCollections(); }
        public ModelOperation(Project p, Guid g) : base(p, g) { InitializeCollections(); }
        /// <summary>
        /// Constructs a PIMOperation and registers it with schema 
        /// <paramref name="schema"/> and inserts it into <see cref="PIMClass"/> 
        /// <paramref name="c"/>
        /// </summary>
        /// <param name="p">Project</param>
        /// <param name="c"><see cref="PIMClass"/> to insert to</param>
        /// <param name="schema"><see cref="PIMSchema"/> to register with
        /// </param>
        public ModelOperation(Project p, PIMClass c, PIMSchema schema, int index = -1)
            : base(p)
        {
            InitializeCollections(); 

            //schema.PIMOperations.Add(this);
            //if (index == -1)
            //    c.PIMOperations.Add(this);
            //else c.PIMOperations.Insert(this, index);
            PIMClass = c;
        }
        /// <summary>
        /// Constructs a PIMOperation and registers it with schema <paramref name="schema"/> and inserts it into <see cref="PIMClass"/> <paramref name="c"/>. Also sets <see cref="Guid"/> to <paramref name="g"/>
        /// </summary>
        /// <param name="p">Project</param>
        /// <param name="g"><see cref="Guid"/> to be set</param>
        /// <param name="c"><see cref="PIMClass"/> to insert to</param>
        /// <param name="schema"><see cref="PIMSchema"/> to register with</param>
        public ModelOperation(Project p, Guid g, PIMClass c, PIMSchema schema, int index = -1)
            : base(p, g)
        {
            InitializeCollections(); 

            //schema.PIMOperation.Add(this);
            //if (index == -1)
            //    c.PIMOperation.Add(this);
            //else c.PIMOperation.Insert(this, index);
            PIMClass = c;
        }

        #endregion

        private void InitializeCollections()
        {
            Parameters = new UndirectCollection<ModelOperationParameter>(Project);
            Parameters.CollectionChanged += parameters_CollectionChanged;
        }

        void parameters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Parameters");
        }

        private Guid pimClassGuid;

        public PIMClass PIMClass
        {
            get { return pimClassGuid != Guid.Empty ? Project.TranslateComponent<PIMClass>(pimClassGuid) : null; }
            set { pimClassGuid = value; NotifyPropertyChanged("PIMClass"); }
        }

        private Guid declaringTypeGuid; 

        public AttributeType DeclaringType
        {
            get { return declaringTypeGuid != Guid.Empty ? Project.TranslateComponent<AttributeType>(declaringTypeGuid) : null; }
            set { declaringTypeGuid = value; NotifyPropertyChanged("DeclaringType"); }
        }

        private Guid resultTypeGuid;

        public AttributeType ResultType
        {
            get
            {
                return resultTypeGuid != Guid.Empty ? Project.TranslateComponent<AttributeType>(resultTypeGuid) : null;
            }
            set
            {
                resultTypeGuid = value;
                NotifyPropertyChanged("ResultType");
            }
        }

        private string summary;
        public string Summary
        {
            get { return summary; }
            set { summary = value; NotifyPropertyChanged("Summary");}
        }

        public UndirectCollection<ModelOperationParameter> Parameters { get; private set; }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            if (PIMClass != null)
            {
                this.SerializeIDRef(PIMClass, "pimClassID", parentNode, context, false);
            }
            if (DeclaringType != null)
            {
                this.SerializeAttributeType(DeclaringType, parentNode, context, "DeclaringType");
            }
            if (ResultType != null)
            {
                this.SerializeAttributeType(ResultType, parentNode, context, "ResultType");
            }
            this.WrapAndSerializeCollection("Parameters", "Parameter", Parameters, parentNode, context, true);
            if (!String.IsNullOrEmpty(Summary))
            {
                this.SerializeSimpleValueToCDATA("Summary", Summary, parentNode, context);
            }
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            this.resultTypeGuid = this.DeserializeAttributeType(parentNode, context, "ResultType", true);
            this.DeserializeWrappedCollection("Parameters", Parameters, ModelOperationParameter.CreateInstance, parentNode, context);
            foreach (ModelOperationParameter parameter in Parameters)
            {
                parameter.ModelOperation = this;
            }
            pimClassGuid = this.DeserializeIDRef("pimClassID", parentNode, context, true);
            declaringTypeGuid = this.DeserializeAttributeType(parentNode, context, "DeclaringType", true);
            Summary = this.DeserializeSimpleValueFromCDATA("Summary", parentNode, context, true);
        }
        public static ModelOperation CreateInstance(Project project)
        {
            return new ModelOperation(project, Guid.Empty);
        }

        #endregion

        public override string ToString()
        {
            return "PIMOperation: " + (pimClassGuid == Guid.Empty ? '"' + Name + '"' : '"'
                + PIMClass.Name + '.' + Name + '"');
        }

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new ModelOperation(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            ModelOperation copyModelOperation = (ModelOperation)copyComponent;
            
            if (PIMClass != null)
            {
                copyModelOperation.pimClassGuid = createdCopies.GetGuidForCopyOf(PIMClass);
            }
            if (DeclaringType != null)
            {
                copyModelOperation.declaringTypeGuid = createdCopies.GetGuidForCopyOf(DeclaringType);
            }

            this.CopyCollection<ModelOperationParameter>(Parameters, copyModelOperation.Parameters, projectVersion, createdCopies);
        }

        #endregion
    }
}