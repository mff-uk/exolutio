using System;
using System.Collections.Generic;
using System.Xml.Linq;
using EvoX.Model.PIM;
using EvoX.Model.Serialization;
using EvoX.SupportingClasses;

namespace EvoX.Model.ViewHelper
{
	/// <summary>
	/// View helper for <see cref="XCase.Model.AssociationEnd"/>. Stores position of 
	/// labels and points of that part of association that connects the <see cref="AssociationEnd"/>
	/// to the view helper.
	/// </summary>
	public class PIMAssociationEndViewHelper : PositionableElementViewHelper
	{
        public PIMAssociationEndViewHelper(Diagram diagram, PIMAssociationViewHelper associationViewHelper)
			: base(diagram)
		{
			AssociationViewHelper = associationViewHelper;
			CardinalityLabelViewHelper = new LabelViewHelper(diagram);
            RoleLabelViewHelper = new LabelViewHelper(diagram);
			
			points = new ObservablePointCollection();
		}
        
        private Guid associationEndGuid;
        public PIMAssociationEnd AssociationEnd
		{
            get { return Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid); }
            set { associationEndGuid = value; NotifyPropertyChanged("PIMClass"); }
		}

		private PIMAssociationViewHelper associationViewHelper;
		public PIMAssociationViewHelper AssociationViewHelper
		{
			get
			{
				return associationViewHelper;
			}
			private set
			{
				associationViewHelper = value;
				NotifyPropertyChanged("AssociationViewHelper");
			}
		}

        public LabelViewHelper CardinalityLabelViewHelper { get; set; }

        public LabelViewHelper RoleLabelViewHelper { get; set; }

		private readonly ObservablePointCollection points = new ObservablePointCollection();

		public ObservablePointCollection Points
		{
			get
			{
				return points;
			}
		}

        public override Versioning.IEvoXCloneable Clone(ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            throw new NotImplementedException("Member PIMAssociationEndViewHelper.Clone not implemented.");
        }

        public override void FillCopy(Versioning.IEvoXCloneable copyComponent, ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);
            PIMAssociationEndViewHelper copyAssociationEndViewHelper = (PIMAssociationEndViewHelper) copyComponent;
			this.CardinalityLabelViewHelper.FillCopy(copyAssociationEndViewHelper.CardinalityLabelViewHelper, projectVersion, createdCopies);
			this.RoleLabelViewHelper.FillCopy(copyAssociationEndViewHelper.RoleLabelViewHelper, projectVersion, createdCopies);
			copyAssociationEndViewHelper.Points.AppendRangeAsCopy(this.Points);
		}

        public override void Serialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Serialize(parentNode, context);

            this.SerializeIDRef(AssociationEnd, "AssociationEndID", parentNode, context);
            this.SerializeToChildElement("CardinalityLabelViewHelper", CardinalityLabelViewHelper, parentNode, context);
            this.SerializeToChildElement("RoleLabelViewHelper", RoleLabelViewHelper, parentNode, context);
            this.SerializePointsCollection(Points, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            associationEndGuid = this.DeserializeIDRef("AssociationEndID", parentNode, context);
            CardinalityLabelViewHelper.DeserializeFromChildElement("CardinalityLabelViewHelper", parentNode, context);
            RoleLabelViewHelper.DeserializeFromChildElement("RoleLabelViewHelper", parentNode, context);
            this.DeserializePointsCollection(Points, parentNode, context);
        }

        public static PIMAssociationEndViewHelper CreateInstance(Diagram diagram, PIMAssociationViewHelper associationViewHelper)
        {
            return new PIMAssociationEndViewHelper(diagram, associationViewHelper);
        }
	}
}