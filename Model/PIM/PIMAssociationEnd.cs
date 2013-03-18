using System;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

namespace Exolutio.Model.PIM
{
    public class PIMAssociationEnd : PIMComponent, IHasCardinality
    {
        #region Constructors
        public PIMAssociationEnd(Project p) : base(p) { }
        public PIMAssociationEnd(Project p, Guid g) : base(p, g) { }
        public PIMAssociationEnd(Project p, PIMClass c, PIMSchema schema)
            : base(p)
        {
            schema.PIMAssociationEnds.Add(this);
            c.PIMAssociationEnds.Add(this);
            PIMClass = c;
        }
        public PIMAssociationEnd(Project p, Guid g, PIMClass c, PIMSchema schema)
            : base(p, g)
        {
            schema.PIMAssociationEnds.Add(this);
            c.PIMAssociationEnds.Add(this);
            PIMClass = c;
        }
        public PIMAssociationEnd(Project p, PIMAssociation a, PIMSchema schema)
            : base(p)
        {
            schema.PIMAssociationEnds.Add(this);
            a.PIMAssociationEnds.Add(this);
            PIMAssociation = a;
        }
        public PIMAssociationEnd(Project p, Guid g, PIMAssociation a, PIMSchema schema)
            : base(p, g)
        {
            schema.PIMAssociationEnds.Add(this);
            a.PIMAssociationEnds.Add(this);
            PIMAssociation = a;
        }
        public PIMAssociationEnd(Project p, PIMClass c, PIMAssociation a, PIMSchema schema)
            : base(p)
        {
            schema.PIMAssociationEnds.Add(this);
            c.PIMAssociationEnds.Add(this);
            a.PIMAssociationEnds.Add(this);
            PIMClass = c;
            PIMAssociation = a;
        }
        public PIMAssociationEnd(Project p, Guid g, PIMClass c, PIMAssociation a, PIMSchema schema)
            : base(p, g)
        {
            schema.PIMAssociationEnds.Add(this);
            c.PIMAssociationEnds.Add(this);
            a.PIMAssociationEnds.Add(this);
            PIMClass = c;
            PIMAssociation = a;
        }
        #endregion

        #region IHasCardinality Members

        private uint lower = 1;

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

        private UnlimitedInt upper = 1;

        public UnlimitedInt Upper
        {
            get { return upper; }
            set { upper = value; NotifyPropertyChanged("Upper"); }
        }

        public string CardinalityString
        {
            get { return this.GetCardinalityString(); }
        }

        #endregion

        private Guid pimClassGuid;

        public PIMClass PIMClass
        {
            get { return Project.TranslateComponent<PIMClass>(pimClassGuid); }
            set { pimClassGuid = value; NotifyPropertyChanged("PIMClass"); }
        }

        private Guid pimAssociationGuid;

        public PIMAssociation PIMAssociation
        {
            get { return Project.TranslateComponent<PIMAssociation>(pimAssociationGuid); }
            set { pimAssociationGuid = value; NotifyPropertyChanged("PIMAssociation"); }
        }

		private bool? isNavigable;

		public bool? IsNavigable
		{
			get { return isNavigable; }
			set
			{
				isNavigable = value;
				NotifyPropertyChanged("IsNavigable");
			}
		}

		private bool? isShared;

		public bool? IsShared
		{
			get { return isShared; }
			set
			{
				isShared = value;
				NotifyPropertyChanged("IsShared");
			}
		}

		private bool? isComposite;

		public bool? IsComposite
		{
			get { return isComposite; }
			set
			{
				isComposite = value;
				NotifyPropertyChanged("IsComposite");
			}
		}

        #region Implementation of IExolutioSerializable

        public override void Serialize(XElement parentNode, SerializationContext context)
        {
            base.Serialize(parentNode, context);
            this.SerializeIDRef(PIMClass, "refPIMClassID", parentNode, context);
            this.SerializeIDRef(PIMAssociation, "refPIMAssociationID", parentNode, context);
			if (IsShared.HasValue)
				this.SerializeSimpleValueToAttribute("IsShared", IsShared, parentNode, context);
			if (IsNavigable.HasValue)
				this.SerializeSimpleValueToAttribute("IsNavigable", IsNavigable, parentNode, context);
			if (IsComposite.HasValue)
				this.SerializeSimpleValueToAttribute("IsComposite", IsComposite, parentNode, context);
            this.SerializeCardinality(parentNode, context);
        }

        public override void Deserialize(XElement parentNode, SerializationContext context)
        {
            base.Deserialize(parentNode, context);
            pimClassGuid = this.DeserializeIDRef("refPIMClassID", parentNode, context);
            pimAssociationGuid = this.DeserializeIDRef("refPIMAssociationID", parentNode, context);
            this.DeserializeCardinality(parentNode, context);
	        bool result;
			if (Boolean.TryParse(this.DeserializeSimpleValueFromAttribute("IsShared", parentNode, context, true), out result))
				IsShared = result;
			if (Boolean.TryParse(this.DeserializeSimpleValueFromAttribute("IsNavigable", parentNode, context, true), out result))
				IsNavigable = result;
			if (Boolean.TryParse(this.DeserializeSimpleValueFromAttribute("IsComposite", parentNode, context, true), out result))
				IsComposite = result; 
            this.PIMSchema.PIMAssociationEnds.Add(this);
        }
        public static PIMAssociationEnd CreateInstance(Project project)
        {
            return new PIMAssociationEnd(project, Guid.Empty);
        }

        #endregion

        public override string ToString()
        {
            string s = "PIMAssociationEnd: ";
            if (!string.IsNullOrEmpty(Name)) s += "\"" + Name + "\" ";
            if (pimAssociationGuid != Guid.Empty && PIMAssociation.Name != null) s += "A: " + PIMAssociation.Name + " ";
            if (pimClassGuid != Guid.Empty && PIMClass.Name != null) s += "[" + PIMClass.Name + "] ";
            if (this.HasNondefaultCardinality())
            {
                s += "{" + lower + ".." + Upper + "}";
            }
            return s;
        }

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new PIMAssociationEnd(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PIMAssociationEnd copyPIMAssociationEnd = (PIMAssociationEnd)copyComponent;
            copyPIMAssociationEnd.Lower = this.Lower;
            copyPIMAssociationEnd.Upper = this.Upper;
	        copyPIMAssociationEnd.IsNavigable = this.IsNavigable;
	        copyPIMAssociationEnd.IsShared = this.IsShared;
	        copyPIMAssociationEnd.isComposite = this.IsComposite;
            copyPIMAssociationEnd.pimClassGuid = createdCopies.GetGuidForCopyOf(PIMClass);
            copyPIMAssociationEnd.pimAssociationGuid = createdCopies.GetGuidForCopyOf(PIMAssociation);
        }

        #endregion
    }
}