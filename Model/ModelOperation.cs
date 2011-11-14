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
            parameters = new ObservableCollection<ModelOperationParameter>();
            parameters.CollectionChanged += parameters_CollectionChanged;
        }

        void parameters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Parameters");
        }

        private Guid pimClassGuid;

        public PIMClass PIMClass
        {
            get { return Project.TranslateComponent<PIMClass>(pimClassGuid); }
            set { pimClassGuid = value; NotifyPropertyChanged("PIMClass"); }
        }

        private Guid declaringAttributeType; 

        public AttributeType DeclaringAttributeType
        {
            get { return Project.TranslateComponent<AttributeType>(declaringAttributeType); }
            set { declaringAttributeType = value; NotifyPropertyChanged("DeclaringAttributeType"); }
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
                if (value != null)
                {
                    resultTypeGuid = value;
                }
                else
                {
                    resultTypeGuid = Guid.Empty;
                }
                NotifyPropertyChanged("ResultType");
            }
        }

        private ObservableCollection<ModelOperationParameter> parameters;

        public ObservableCollection<ModelOperationParameter> Parameters
        {
            get { return parameters; }
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            if (PIMClass != null)
            {
                this.SerializeIDRef(PIMClass, "pimClassID", parentNode, context, false);
            }
            if (DeclaringAttributeType != null)
            {
                this.SerializeIDRef(DeclaringAttributeType, "declaringAttributeTypeID", parentNode, context, false);
            }
            if (ResultType != null)
            {
                this.SerializeAttributeType(ResultType, parentNode, context, "ResultType");
            }
            this.WrapAndSerializeCollection("Parameters", "Parameter", Parameters, parentNode, context, true);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            this.resultTypeGuid = this.DeserializeAttributeType(parentNode, context, "ResultType", true);
            
            if (parentNode.Element(context.ExolutioNS + "Parameters") != null)
            {
                foreach (XElement parameterElement in parentNode.Element(context.ExolutioNS + "Parameters").Elements(context.ExolutioNS + "Parameter"))
                {
                    ModelOperationParameter modelOperationParameter = new ModelOperationParameter();
                    modelOperationParameter.ModelOperation = this;
                    modelOperationParameter.Deserialize(parameterElement, context);
                    Parameters.Add(modelOperationParameter);
                }
            }

            pimClassGuid = this.DeserializeIDRef("pimClassID", parentNode, context, true);
            declaringAttributeType = this.DeserializeIDRef("declaringAttributeTypeID", parentNode, context, true);
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
            
            copyModelOperation.pimClassGuid = createdCopies.GetGuidForCopyOf(PIMClass);
        }

        #endregion
    }
}