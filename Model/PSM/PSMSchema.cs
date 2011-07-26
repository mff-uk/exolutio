using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model.PSM
{
    public class PSMSchema : Schema
    {
        public PSMSchema(Project p) : base(p) { InitCollections(); }
        public PSMSchema(Project p, Guid g) : base(p, g) { InitCollections(); }
        public PSMSchema(Project p, Guid g, ProjectVersion projectVersion) : base(p, g) { InitCollections(); projectVersion.PSMSchemas.Add(this); }

        public void InitCollections()
        {
            PSMAssociations = new UndirectCollection<PSMAssociation>(Project);
            PSMAssociations.MemberAdded += RegisterPSMComponent;
            PSMAssociations.MemberRemoved += UnregisterPSMComponent;

            PSMAttributes = new UndirectCollection<PSMAttribute>(Project);
            PSMAttributes.MemberAdded += RegisterPSMComponent;
            PSMAttributes.MemberRemoved += UnregisterPSMComponent;

            PSMSchemaReferences = new UndirectCollection<PSMSchemaReference>(Project);
            PSMSchemaReferences.MemberAdded += RegisterPSMComponent;
            PSMSchemaReferences.MemberRemoved += UnregisterPSMComponent;

            PSMClasses = new UndirectCollection<PSMClass>(Project);
            PSMClasses.MemberAdded += RegisterPSMComponent;
            PSMClasses.MemberRemoved += UnregisterPSMComponent;

            PSMContentModels = new UndirectCollection<PSMContentModel>(Project);
            PSMContentModels.MemberAdded += RegisterPSMComponent;
            PSMContentModels.MemberRemoved += UnregisterPSMComponent;

            Roots = new UndirectCollection<PSMAssociationMember>(Project);
        }

        protected override string DefaultString
        {
            get { return string.Format("PSMSchema {0}", ProjectVersion.PSMSchemas.IndexOf(this) + 1); }
        }

        public PSMDiagram PSMDiagram
        {
            get { return ProjectVersion.PSMDiagrams.FirstOrDefault(d => d.PSMSchema == this); }
        }

        public override IEnumerable<Component> SchemaComponents
        {
            get { return ModelIterator.GetPSMComponents(this).Cast<Component>(); }
        }

        public delegate void ComponentEvent(Schema psmSchema, Component component);
        public event ComponentEvent ComponentAdded;
        public event ComponentEvent ComponentRemoved;

        #region generic component registration

        private void RegisterPSMComponent(PSMComponent component)
        {
            component.Schema = this;
            if (ComponentAdded != null)
            {
                ComponentAdded(this, component);
            }
            if (Project.UsesVersioning)
            {
                Version.NotifyItemAdded(this, component);
            }
        }

        private void UnregisterPSMComponent(PSMComponent component)
        {
            if (Project.UsesVersioning)
            {
                Version.NotifyItemRemoved(this, component);
            }
            component.Schema = null;
            if (ComponentRemoved != null)
            {
                ComponentRemoved(this, component);
            }
        }

        #endregion

        public UndirectCollection<PSMAssociation> PSMAssociations { get; private set; }

        public UndirectCollection<PSMAttribute> PSMAttributes { get; private set; }

        public UndirectCollection<PSMSchemaReference> PSMSchemaReferences { get; private set; }

        public UndirectCollection<PSMClass> PSMClasses { get; private set; }

        public UndirectCollection<PSMContentModel> PSMContentModels { get; private  set; }

        public UndirectCollection<PSMAssociationMember> Roots { get; private set; }

        #region schema class

        private Guid psmSchemaClassGuid;

        public PSMSchemaClass PSMSchemaClass
        {
            get
            {
                return psmSchemaClassGuid == Guid.Empty ? null : Project.TranslateComponent<PSMSchemaClass>(psmSchemaClassGuid);
            }
        }

        public string XMLNamespaceOrDefaultNamespace
        {
            get 
            {
                return @"http://www.example.org/";
            }
        }

        public IEnumerable<PSMClass> TopClasses
        {
            get { return ModelIterator.GetChildNodes(PSMSchemaClass).OfType<PSMClass>(); }
        }

        /// <summary>
        /// Returns schema class, classes and content models
        /// </summary>
        public IEnumerable<PSMAssociationMember> PSMNodes
        {
            get { return new PSMAssociationMember[] {PSMSchemaClass}.Concat(PSMClasses).Concat(PSMContentModels); }
        }

        public void RegisterPSMSchemaClass(PSMSchemaClass psmSchemaClass)
        {
            psmSchemaClass.Schema = this; 
            psmSchemaClassGuid = psmSchemaClass;
            RegisterPSMComponent(psmSchemaClass);
            NotifyPropertyChanged("PSMSchemaClass");
        }

        public void UnRegisterPSMSchemaClass(PSMSchemaClass psmSchemaClass)
        {
            psmSchemaClass.Schema = null; 
            psmSchemaClassGuid = Guid.Empty;
            UnregisterPSMComponent(psmSchemaClass);
            NotifyPropertyChanged("PSMSchemaClass");       
        }

        #endregion

        #region Implementation of IExolutioSerializable
        
        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            if (this.PSMSchemaReferences.Count > 0)
            {
                this.WrapAndSerializeCollection("PSMSchemaReferences", "PSMSchemaReference", PSMSchemaReferences, parentNode, context);
            }
            if (PSMSchemaClass != null)
            {
                this.SerializeToChildElement("PSMSchemaClass", PSMSchemaClass, parentNode, context);
            }
            this.WrapAndSerializeIDRefCollection("Roots", "Root", "rootID", Roots, parentNode, context);
            this.WrapAndSerializeCollection("PSMClasses", "PSMClass", PSMClasses, parentNode, context);
            this.WrapAndSerializeCollection("PSMAssociations", "PSMAssociation", PSMAssociations, parentNode, context);
            this.WrapAndSerializeCollection("PSMContentModels", "PSMContentModel", PSMContentModels, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            context.CurrentSchemaGuid = this;

            if (parentNode.Element(context.ExolutioNS + "PSMSchemaReferences") != null)
            {
                this.DeserializeWrappedCollection("PSMSchemaReferences", PSMSchemaReferences, PSMSchemaReference.CreateInstance, parentNode, context);
            }

            if (parentNode.Element(context.ExolutioNS + "PSMSchemaClass") != null)
            {
                PSMSchemaClass objPSMSchemaClass = new PSMSchemaClass(Project, Guid.Empty);
                objPSMSchemaClass.DeserializeFromChildElement("PSMSchemaClass", parentNode, context);
                RegisterPSMSchemaClass(objPSMSchemaClass);
            }

            this.DeserializeWrappedIDRefCollection("Roots", "rootID", Roots, parentNode, context);
            this.DeserializeWrappedCollection("PSMClasses", PSMClasses, PSMClass.CreateInstance, parentNode, context);
            this.DeserializeWrappedCollection("PSMAssociations", PSMAssociations, PSMAssociation.CreateInstance, parentNode, context);
            this.DeserializeWrappedCollection("PSMContentModels", PSMContentModels, PSMContentModel.CreateInstance, parentNode, context);

            foreach (PSMAssociation psmAssociation in PSMAssociations)
            {
                // assignment has sideffects
                psmAssociation.Parent = psmAssociation.Parent; 
            }

            context.CurrentSchemaGuid = Guid.Empty;
        }

        public static PSMSchema CreateInstance(Project project)
        {
            return new PSMSchema(project, Guid.Empty);
        }

        #endregion

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PSMSchema(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMSchema copyPSMSchema = (PSMSchema) copyComponent;
            copyPSMSchema.SetProjectVersion(projectVersion);

            if (PSMSchemaClass != null)
            {
                PSMSchemaClass psmSchemaClass = PSMSchemaClass.CreateTypedCopy(projectVersion, createdCopies);
                copyPSMSchema.RegisterPSMSchemaClass(psmSchemaClass);
            }
            this.CopyCollection(PSMClasses, copyPSMSchema.PSMClasses, projectVersion, createdCopies);
            this.CopyCollection(PSMAssociations, copyPSMSchema.PSMAssociations, projectVersion, createdCopies);
            this.CopyCollection(PSMContentModels, copyPSMSchema.PSMContentModels, projectVersion, createdCopies);
            this.CopyRefCollection(PSMAttributes, copyPSMSchema.PSMAttributes, projectVersion, createdCopies);
            this.CopyRefCollection(Roots, copyPSMSchema.Roots, projectVersion, createdCopies);
        }

        #endregion
    }
}
