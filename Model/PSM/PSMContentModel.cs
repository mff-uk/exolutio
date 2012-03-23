using System;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.PSM.XPath;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model.PSM
{
    public class PSMContentModel : PSMAssociationMember
    {
        public PSMContentModel(Project p) : base(p) { }
        public PSMContentModel(Project p, Guid g) : base(p, g) { }
        public PSMContentModel(Project p, PSMSchema schema, bool isRoot)
            : base(p)
        {
            schema.PSMContentModels.Add(this);
            if (isRoot)
            {
                schema.Roots.Add(this);
            }
        }
        public PSMContentModel(Project p, PSMSchema schema, int rootIndex = -1)
            : base(p)
        {
            if (rootIndex == -1)
            {
                schema.Roots.Add(this);
            }
            else
            {
                schema.Roots.Insert(this, rootIndex);
            }
            schema.PSMContentModels.Add(this);
        }
        public PSMContentModel(Project p, Guid g, PSMSchema schema, int rootIndex = -1)
            : base(p, g) 
        {
            if (rootIndex == -1)
            {
                schema.Roots.Add(this);
            }
            else
            {
                schema.Roots.Insert(this, rootIndex);
            }
            schema.PSMContentModels.Add(this);
        }

        private PSMContentModelType type;
        public PSMContentModelType Type
        {
            get { return type; }
            set { type = value; NotifyPropertyChanged("Type"); }
        }

        public override string XPath
        {
            get { return ParentAssociation.XPath; }
        }

        public override Path XPathFull
        {
            get
            {
                Path result = ParentAssociation.XPathFull.DeepCopy();
                return result;
            }
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            XAttribute typeAttribute = new XAttribute("Type", SerializationContext.EncodeValue(Type));
            parentNode.Add(typeAttribute);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            Type = SerializationContext.DecodeContentModelType(parentNode.Attribute("Type").Value);
        }
        public static PSMContentModel CreateInstance(Project project)
        {
            return new PSMContentModel(project, Guid.Empty);
        }

        #endregion

        public override string ToString()
        {
            return "PSMContentModel: " + Enum.GetName(type.GetType(), type);
        }
        
        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PSMContentModel(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMContentModel copyPSMContentModel = (PSMContentModel) copyComponent;
            copyPSMContentModel.Type = this.Type;
        }

        #endregion
    }
}