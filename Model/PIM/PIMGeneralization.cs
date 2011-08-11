using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using System.Collections.ObjectModel;
using Exolutio.Model.Versioning;
using Exolutio.Model.PSM;
using System.Text;

namespace Exolutio.Model.PIM
{
    public class PIMGeneralization : PIMComponent
    {
        #region Constructors

        public PIMGeneralization(Project p) : base(p) { }
        public PIMGeneralization(Project p, Guid g) : base(p, g) { }
        public PIMGeneralization(Project p, PIMSchema schema, PIMClass generalClass, PIMClass specificClass)
            : base(p)
        {
            schema.PIMGeneralizations.Add(this);
            General = generalClass;
            Specific = specificClass;
            generalClass.GeneralizationsAsGeneral.Add(this);
            specificClass.GeneralizationAsSpecific = this;
        }
        public PIMGeneralization(Project p, Guid g, PIMSchema schema, PIMClass generalClass, PIMClass specificClass)
            : base(p, g)
        {
            schema.PIMGeneralizations.Add(this);
            General = generalClass;
            Specific = specificClass;
            generalClass.GeneralizationsAsGeneral.Add(this);
            specificClass.GeneralizationAsSpecific = this;
        }
        #endregion

        private Guid generalGuid;
        public PIMClass General
        {
            get { return Project.TranslateComponent<PIMClass>(generalGuid); }
            set { generalGuid = value; NotifyPropertyChanged("General"); }
        }

        private Guid specificGuid;
        public PIMClass Specific
        {
            get { return Project.TranslateComponent<PIMClass>(specificGuid); }
            set { specificGuid = value; NotifyPropertyChanged("Specific"); }
        }

        /*public override ComponentList<PSMComponent> GetInterpretedComponents()
        {
            ComponentList<PSMComponent> list = new ComponentList<PSMComponent>();
            foreach (PSMSchema schema in Project.LatestVersion.PSMSchemas)
            {
                foreach (PSMAssociation a in schema.PSMAssociations)
                {
                    if (a.Interpretation == this) list.Add(a);
                }
            }
            return list;
        }*/

        #region Implementation of IExolutioSerializable


        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            this.SerializeIDRef(General, "refGeneralID", parentNode, context);
            this.SerializeIDRef(Specific, "refSpecificID", parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            generalGuid = this.DeserializeIDRef("refGeneralID", parentNode, context);
            specificGuid = this.DeserializeIDRef("refSpecificID", parentNode, context);
        }
        public static PIMGeneralization CreateInstance(Project project)
        {
            return new PIMGeneralization(project, Guid.Empty);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder s = new StringBuilder("PIMGeneralization");
            //s.Append(" \"" + Name + "\"");
            s.Append(':');
            s.Append(' ');
            s.Append(Specific.ToString());
            s.Append(" -> ");
            s.Append(General.ToString());
            return s.ToString();
        }

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PIMGeneralization(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PIMGeneralization copyPIMGeneralization = (PIMGeneralization)copyComponent;
            copyPIMGeneralization.generalGuid = createdCopies.GetGuidForCopyOf(General);
            copyPIMGeneralization.specificGuid = createdCopies.GetGuidForCopyOf(Specific);
        }

        #endregion
    }
}
