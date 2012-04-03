using System;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.PSM.XPath;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;
using Exolutio.Model.PIM;

namespace Exolutio.Model.PSM
{
    public class PSMGeneralization : PSMComponent, IPSMSemanticComponent
    {
        public PSMGeneralization(Project p) : base(p) { }
        public PSMGeneralization(Project p, Guid g) : base(p, g) { }
        public PSMGeneralization(Project p, PSMClass general, PSMClass specific, PSMSchema schema)
            : base(p)
        {
            General = general;
            Specific = specific;
            General.GeneralizationsAsGeneral.Add(this);
            Specific.GeneralizationAsSpecific = this;
            schema.PSMGeneralizations.Add(this);
        }
        public PSMGeneralization(Project p, Guid g, PSMClass general, PSMClass specific, PSMSchema schema)
            : base(p, g)
        {
            General = general;
            Specific = specific;
            General.GeneralizationsAsGeneral.Add(this);
            Specific.GeneralizationAsSpecific = this;
            schema.PSMGeneralizations.Add(this);
        }

        private Guid generalGuid;

        public PSMClass General
        {
            get { return Project.TranslateComponent<PSMClass>(generalGuid); }
            set
            {
                if (generalGuid != Guid.Empty) General.GeneralizationsAsGeneral.CollectionChanged -= Parent_GeneralizationsAsGeneral_CollectionChanged;
                generalGuid = value; 
                NotifyPropertyChanged("General");
                General.GeneralizationsAsGeneral.CollectionChanged += Parent_GeneralizationsAsGeneral_CollectionChanged;
            }
        }

        private void Parent_GeneralizationsAsGeneral_CollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            NotifyPropertyChanged("Index");
        }

        private Guid specificGuid;

        public PSMClass Specific
        {
            get { return specificGuid == Guid.Empty ? null : Project.TranslateComponent<PSMClass>(specificGuid); }
            set 
            {
                if (specificGuid != Guid.Empty) Specific.GeneralizationAsSpecific = null;
                specificGuid = value;
                Specific.GeneralizationAsSpecific = this;
                NotifyPropertyChanged("Specific");
            }
        }

        public int Index
        {
            get
            {
                return General.GeneralizationsAsGeneral.IndexOf(this);
            }
        }

        public override string XPath
        {
            get { return /*string.Format("{0}{1}", General.XPath, !string.IsNullOrEmpty(this.Name) ? "/" + this.Name :*/ string.Empty/*)*/; }
        }

        public override Path GetXPathFull(bool followGeneralizations)
        {
            // it makes no sense to ask for a path to generalization. 
            // Classes should be queried instead. 
            return null;
        }

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);

            // node type attribute with values PSMClass / PSMContentModel 
            // is not neccesary for deserialization
            // but is included in the serialized document for better readability

            {
                XElement generalElement = new XElement(context.ExolutioNS + "General");
                string nodeTypeValue = string.Empty;
                nodeTypeValue = "PSMClass";
                XAttribute nodeTypeAttribute = new XAttribute("NodeType", nodeTypeValue);
                generalElement.Add(nodeTypeAttribute);
                this.SerializeIDRef(General, "generalID", generalElement, context);
                parentNode.Add(generalElement);
            }

            {
                XElement specificElement = new XElement(context.ExolutioNS + "Specific");
                string nodeTypeValue = string.Empty;
                nodeTypeValue = "PSMClass";
                XAttribute nodeTypeAttribute = new XAttribute("NodeType", nodeTypeValue);
                specificElement.Add(nodeTypeAttribute);
                this.SerializeIDRef(Specific, "specificID", specificElement, context);
                parentNode.Add(specificElement);
            }
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            {
                XElement generalNode = parentNode.Element(context.ExolutioNS + "General");
                if (generalNode == null)
                {
                    context.Log.AddErrorFormat("'General' subelement missing in node {0}", parentNode);
                    return;
                }

                generalGuid = this.DeserializeIDRef("generalID", generalNode, context);
            }

            {
                XElement associationChildNode = parentNode.Element(context.ExolutioNS + "Specific");
                if (associationChildNode == null)
                {
                    context.Log.AddErrorFormat("'Specific' subelement missing in node {0}", parentNode);
                    return;
                }

                specificGuid = this.DeserializeIDRef("specificID", associationChildNode, context);
            }
        }
        public static PSMGeneralization CreateInstance(Project project)
        {
            return new PSMGeneralization(project, Guid.Empty);
        }


        #endregion

        public override string ToString()
        {
            string s = "PSMGeneralization: General: ";
            if (General != null) s += General.ToString();
            s += " Specific: ";
            if (Specific != null) s += Specific.ToString();
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

            PSMGeneralization copyPSMGeneralization = (PSMGeneralization)copyComponent;
            copyPSMGeneralization.generalGuid = createdCopies.GetGuidForCopyOf(General);
            copyPSMGeneralization.specificGuid = createdCopies.GetGuidForCopyOf(Specific);
        }

        #endregion
    }
}
