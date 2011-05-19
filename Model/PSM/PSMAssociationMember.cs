using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using EvoX.Model.Serialization;
using EvoX.Model.Versioning;

namespace EvoX.Model.PSM
{
    public abstract class PSMAssociationMember : PSMComponent
    {
        protected PSMAssociationMember(Project p) : base(p)
        {
            InitializeCollections();
        }

        protected PSMAssociationMember(Project p, Guid g)
            : base(p, g)
        {
            InitializeCollections();
        }

        private void InitializeCollections()
        {
            ChildPSMAssociations = new UndirectCollection<PSMAssociation>(Project);
        }

        private Guid parentAssociationGuid;

        public PSMAssociation ParentAssociation
        {
            get
            {
                return parentAssociationGuid == Guid.Empty ? null : Project.TranslateComponent<PSMAssociation>(parentAssociationGuid);
            }
            set
            {
                parentAssociationGuid = value == null ? Guid.Empty : value;
                NotifyPropertyChanged("ParentAssociation");
            }
        }

        #region associations

        public UndirectCollection<PSMAssociation> ChildPSMAssociations { get; private set; }

        #endregion

        #region Implementation of IEvoXSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            if (ParentAssociation != null)
            {
                this.SerializeIDRef(ParentAssociation, "parentAssociationID", parentNode, context);
            }

            this.WrapAndSerializeIDRefCollection("ChildPSMAssociations", "ChildPSMAssociation", "childPSMAssociationID", ChildPSMAssociations, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            parentAssociationGuid = this.DeserializeIDRef("parentAssociationID", parentNode, context, true);

            this.DeserializeWrappedIDRefCollection("ChildPSMAssociations", "childPSMAssociationID", ChildPSMAssociations, parentNode, context);
        }
        #endregion

        #region Implementation of IEvoXCloneable

        public override void FillCopy(IEvoXCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMAssociationMember copyPSMAssociationMember = (PSMAssociationMember) copyComponent;
            if (ParentAssociation != null)
            {
                copyPSMAssociationMember.parentAssociationGuid = createdCopies.GetGuidForCopyOf(ParentAssociation);
            }

            this.CopyRefCollection(ChildPSMAssociations, copyPSMAssociationMember.ChildPSMAssociations, projectVersion, createdCopies, true);
        }

        #endregion
    }
}