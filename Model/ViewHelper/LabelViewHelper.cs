using System;
using System.Collections.Generic;
using System.Xml.Linq;
using EvoX.Model.Serialization;
using EvoX.Model.Versioning;

namespace EvoX.Model.ViewHelper
{
	/// <summary>
	/// View helper used for labels for connectors, stores position
	/// of the label. 
	/// </summary>
	public class LabelViewHelper: PositionableElementViewHelper
	{
		public LabelViewHelper(Diagram diagram) : 
			base(diagram)
		{
			LabelVisible = true; 
		}

		private bool labelVisible;
		public bool LabelVisible
		{
			get { return labelVisible; }
			set { labelVisible = value; NotifyPropertyChanged("LabelVisible"); }
		}

        public override IEvoXCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            return new LabelViewHelper(projectVersion.Project.TranslateComponent<Diagram>(createdCopies.GetGuidForCopyOf(Diagram)));
        }

        public override void FillCopy(IEvoXCloneable copyComponent, ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);
            LabelViewHelper copyLabelViewHelper = (LabelViewHelper)copyComponent;
			copyLabelViewHelper.LabelVisible = LabelVisible;
		}

        public override void Serialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Serialize(parentNode, context);

            this.SerializeSimpleValueToElement("LabelVisible", LabelVisible, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            LabelVisible = bool.Parse(this.DeserializeSimpleValueFromElement("LabelVisible", parentNode, context));
        }
    }
}