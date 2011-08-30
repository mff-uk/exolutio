using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using Exolutio.Model.PSM;

namespace Exolutio.Model.PIM
{
    public class PIMOperation : PIMComponent
    {
        #region Constructors
        public PIMOperation(Project p) : base(p) { InitializeCollections(); }
        public PIMOperation(Project p, Guid g) : base(p, g) { InitializeCollections(); }
        /// <summary>
        /// Constructs a PIMOperation and registers it with schema 
        /// <paramref name="schema"/> and inserts it into <see cref="PIMClass"/> 
        /// <paramref name="c"/>
        /// </summary>
        /// <param name="p">Project</param>
        /// <param name="c"><see cref="PIMClass"/> to insert to</param>
        /// <param name="schema"><see cref="PIMSchema"/> to register with
        /// </param>
        public PIMOperation(Project p, PIMClass c, PIMSchema schema, int index = -1)
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
        public PIMOperation(Project p, Guid g, PIMClass c, PIMSchema schema, int index = -1)
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
            parameters = new ObservableCollection<PIMOperationParameter>();
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

        private ObservableCollection<PIMOperationParameter> parameters;

        public ObservableCollection<PIMOperationParameter> Parameters
        {
            get { return parameters; }
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            this.SerializeIDRef(PIMClass, "pimClassID", parentNode, context, false);
            if (ResultType != null)
            {
                this.SerializeToChildElement("ResultType", ResultType, parentNode, context);
            }
            this.WrapAndSerializeCollection("Parameters", "Parameter", Parameters, parentNode, context, true);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            if (parentNode.Element(context.ExolutioNS + "ResultType") != null)
            {
                this.DeserializeFromChildElement("ResultType", parentNode, context);
            }

            if (parentNode.Element(context.ExolutioNS + "Parameters") != null)
            {
                foreach (XElement parameterElement in parentNode.Element(context.ExolutioNS + "Parameters").Elements(context.ExolutioNS + "Parameter"))
                {
                    PIMOperationParameter pimOperationParameter = new PIMOperationParameter();
                    pimOperationParameter.PIMOperation = this;
                    pimOperationParameter.Deserialize(parameterElement, context);
                    Parameters.Add(pimOperationParameter);
                }
            }

            pimClassGuid = this.DeserializeIDRef("pimClassID", parentNode, context, false);
        }
        public static PIMOperation CreateInstance(Project project)
        {
            return new PIMOperation(project, Guid.Empty);
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
            return new PIMOperation(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PIMOperation copyPIMOperation = (PIMOperation)copyComponent;
            
            copyPIMOperation.pimClassGuid = createdCopies.GetGuidForCopyOf(PIMClass);
        }

        #endregion
    }
}