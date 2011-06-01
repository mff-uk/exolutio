using System;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model.PSM
{
    public class PSMAssociation : PSMComponent, IHasCardinality, IPSMSemanticComponent
    {
        public PSMAssociation(Project p) : base(p) { }
        public PSMAssociation(Project p, Guid g) : base(p, g) { }
        public PSMAssociation(Project p, PSMAssociationMember parent, PSMAssociationMember child, PSMSchema schema)
            : base(p)
        {
            Parent = parent;
            Child = child;
            if (Parent != null)
            {
                Parent.ChildPSMAssociations.Add(this);
            }
            if (Child != null) Child.ParentAssociation = this;
            schema.PSMAssociations.Add(this);
        }
        public PSMAssociation(Project p, Guid g, PSMAssociationMember parent, PSMAssociationMember child, PSMSchema schema)
            : base(p, g)
        {
            Parent = parent;
            Child = child;
            if (Parent != null)
            {
                Parent.ChildPSMAssociations.Add(this);
            }
            if (Child != null) Child.ParentAssociation = this;
            schema.PSMAssociations.Add(this);
        }
        public PSMAssociation(Project p, PSMAssociationMember parent, PSMAssociationMember child, int index, PSMSchema schema)
            : base(p)
        {
            Parent = parent;
            Child = child;
            if (Parent != null)
            {
                Parent.ChildPSMAssociations.Insert(this, index);
            }
            if (Child != null) Child.ParentAssociation = this;
            schema.PSMAssociations.Add(this);
        }
        public PSMAssociation(Project p, Guid g, PSMAssociationMember parent, PSMAssociationMember child, int index, PSMSchema schema)
            : base(p, g)
        {
            Parent = parent;
            Child = child;
            if (Parent != null)
            {
                Parent.ChildPSMAssociations.Insert(this, index);
            }
            if (Child != null) Child.ParentAssociation = this;
            schema.PSMAssociations.Add(this);
        }

        private Guid parentGuid;

        public PSMAssociationMember Parent
        {
            get { return parentGuid == Guid.Empty ? null : Project.TranslateComponent<PSMAssociationMember>(parentGuid); }
            set
            {
                if (parentGuid != Guid.Empty)
                {
                    Parent.ChildPSMAssociations.CollectionChanged -= Parent_ChildPSMAssociations_CollectionChanged;
                }
                parentGuid = value; 
                NotifyPropertyChanged("Parent");
                if (value != null)
                {
                    Parent.ChildPSMAssociations.CollectionChanged += Parent_ChildPSMAssociations_CollectionChanged;
                }
            }
        }

        private void Parent_ChildPSMAssociations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            NotifyPropertyChanged("Index");
        }

        private Guid childGuid;

        public PSMAssociationMember Child
        {
            get { return childGuid == Guid.Empty ? null : Project.TranslateComponent<PSMAssociationMember>(childGuid); }
            set { childGuid = value; NotifyPropertyChanged("Child"); }
        }

        public int Index
        {
            get
            {
                return Parent.ChildPSMAssociations.IndexOf(this);
            }
        }

        public override string XPath
        {
            get { return string.Format("{0}{1}", Parent.XPath, !string.IsNullOrEmpty(this.Name) ? this.Name + "/" : string.Empty); }
        }

        #region IHasCardinality Members

        private uint lower = 1;
        private UnlimitedInt upper = 1;

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

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            this.SerializeCardinality(parentNode, context);

            // node type attribute with values PSMClass / PSMContentModel 
            // is not neccesary for deserialization
            // but is included in the serialized document for better readability

            {
                XElement associationParentElement = new XElement(context.ExolutioNS + "Parent");
                string nodeTypeValue = string.Empty;
                if (Parent is PSMClass)
                {
                    nodeTypeValue = "PSMClass";
                }
                else if (Parent is PSMContentModel)
                {
                    nodeTypeValue = "PSMContentModel";
                }
                else if (Parent is PSMSchemaClass)
                {
                    nodeTypeValue = "PSMSchemaClass";
                }
                XAttribute nodeTypeAttribute = new XAttribute("NodeType", nodeTypeValue);
                associationParentElement.Add(nodeTypeAttribute);
                this.SerializeIDRef(Parent, "parentID", associationParentElement, context);
                parentNode.Add(associationParentElement);
            }

            {
                XElement associationChildElement = new XElement(context.ExolutioNS + "Child");
                string nodeTypeValue = string.Empty;
                if (Child is PSMClass)
                {
                    nodeTypeValue = "PSMClass";
                }
                if (Child is PSMContentModel)
                {
                    nodeTypeValue = "PSMContentModel";
                }
                else if (Child is PSMSchemaClass)
                {
                    nodeTypeValue = "PSMSchemaClass";
                }
                XAttribute nodeTypeAttribute = new XAttribute("NodeType", nodeTypeValue);
                associationChildElement.Add(nodeTypeAttribute);
                this.SerializeIDRef(Child, "childID", associationChildElement, context);
                parentNode.Add(associationChildElement);
            }
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            this.DeserializeCardinality(parentNode, context);

            {
                XElement associationParentNode = parentNode.Element(context.ExolutioNS + "Parent");
                if (associationParentNode == null)
                {
                    context.Log.AddErrorFormat("'Parent' subelement missing in node {0}", parentNode);
                    return;
                }

                parentGuid = this.DeserializeIDRef("parentID", associationParentNode, context);
            }

            {
                XElement associationChildNode = parentNode.Element(context.ExolutioNS + "Child");
                if (associationChildNode == null)
                {
                    context.Log.AddErrorFormat("'Child' subelement missing in node {0}", parentNode);
                    return;
                }

                childGuid = this.DeserializeIDRef("childID", associationChildNode, context);
            }
        }
        public static PSMAssociation CreateInstance(Project project)
        {
            return new PSMAssociation(project, Guid.Empty);
        }


        #endregion

        public override string ToString()
        {
            string s = "PSMAssociation from ";
            if (Parent != null) s += Parent.ToString();
            s += " to ";
            if (Child != null) s += Child.ToString();
            return s;
        }

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PSMAssociation(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMAssociation copyPSMAssociation = (PSMAssociation)copyComponent;
            copyPSMAssociation.Lower = this.Lower;
            copyPSMAssociation.Upper = this.Upper;
            copyPSMAssociation.parentGuid = createdCopies.GetGuidForCopyOf(Parent);
            copyPSMAssociation.childGuid = createdCopies.GetGuidForCopyOf(Child);
        }

        #endregion
    }
}
