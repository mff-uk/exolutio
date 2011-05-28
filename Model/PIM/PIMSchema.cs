using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using System.Collections.ObjectModel;
using Exolutio.Model.Versioning;
using System.Linq; 

namespace Exolutio.Model.PIM
{
    public class PIMSchema : Schema
    {
        public PIMSchema(Project p) : base(p)
        {
            InitCollections();
        }
        public PIMSchema(Project p, Guid g) : base(p, g)
        {
            InitCollections();
        }

        public void InitCollections()
        {
            PIMAssociations = new UndirectCollection<PIMAssociation>(Project);
            PIMAssociations.MemberAdded += RegisterPIMComponent;
            PIMAssociations.MemberRemoved += UnregisterPIMComponent;
            
            PIMAttributes = new UndirectCollection<PIMAttribute>(Project);
            PIMAttributes.MemberAdded += RegisterPIMComponent;
            PIMAttributes.MemberRemoved += UnregisterPIMComponent;
            
            PIMAssociationEnds = new UndirectCollection<PIMAssociationEnd>(Project);
            PIMAssociationEnds.MemberAdded += RegisterPIMComponent;
            PIMAssociationEnds.MemberRemoved += UnregisterPIMComponent;

            PIMClasses = new UndirectCollection<PIMClass>(Project);
            PIMClasses.MemberAdded += RegisterPIMComponent;
            PIMClasses.MemberRemoved += UnregisterPIMComponent;
        }

        #region generic component registration

        private void RegisterPIMComponent(PIMComponent component)
        {
            component.Schema = this;
            if (Project.UsesVersioning)
            {
                Version.NotifyItemAdded(this, component);
            }
            if (ComponentAdded != null)
            {
                ComponentAdded(this, component);
            }
        }

        private void UnregisterPIMComponent(PIMComponent component)
        {
            if (Project.UsesVersioning)
            {
                Version.NotifyItemRemoved(this, component);
            }
            if (ComponentRemoved != null)
            {
                ComponentRemoved(this, component);
            }
            component.Schema = null;
        }

        #endregion

        public UndirectCollection<PIMAssociation> PIMAssociations { get; private set; }

        public UndirectCollection<PIMAttribute> PIMAttributes { get; private set; }

        public UndirectCollection<PIMAssociationEnd> PIMAssociationEnds { get; private set; }

        public UndirectCollection<PIMClass> PIMClasses { get; set; }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            this.WrapAndSerializeCollection("PIMClasses", "PIMClass", PIMClasses, parentNode, context);
            this.WrapAndSerializeCollection("PIMAssociations", "PIMAssociation", PIMAssociations, parentNode, context);
            base.SerializeRemaining(parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            context.CurrentSchemaGuid = this;

            this.DeserializeWrappedCollection("PIMClasses", PIMClasses, PIMClass.CreateInstance, parentNode, context);
            this.DeserializeWrappedCollection("PIMAssociations", PIMAssociations, PIMAssociation.CreateInstance, parentNode, context);

            context.CurrentSchemaGuid = Guid.Empty;
            base.DeserializeRemaining(parentNode, context);
        }
        #endregion

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PIMSchema(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PIMSchema copyPIMSchema = (PIMSchema) copyComponent;
            copyPIMSchema.SetProjectVersion(projectVersion);

            this.CopyCollection(PIMClasses, copyPIMSchema.PIMClasses, projectVersion, createdCopies);
            this.CopyCollection(PIMAssociations, copyPIMSchema.PIMAssociations, projectVersion, createdCopies);
            this.CopyRefCollection(PIMAttributes, copyPIMSchema.PIMAttributes, projectVersion, createdCopies);
            this.CopyRefCollection(PIMAssociationEnds, copyPIMSchema.PIMAssociationEnds, projectVersion, createdCopies);
        }

        #endregion

        protected override string DefaultString
        {
            get { return "PIMSchema"; }
        }

        public override IEnumerable<Component> SchemaComponents
        {
            get { return ModelIterator.GetPIMComponents(this).Cast<Component>(); }
        }

        public delegate void ComponentEvent(Schema psmSchema, Component component);
        public event ComponentEvent ComponentAdded;
        public event ComponentEvent ComponentRemoved;
    }
}
