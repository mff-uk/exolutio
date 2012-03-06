using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Exolutio.SupportingClasses;
using Exolutio.Model.Serialization;

namespace Exolutio.Model.ViewHelper
{
    /// <summary>
    /// Stores visualization information for Associations 
    /// </summary>
    public class PSMAssociationViewHelper : ViewHelper, IComponentViewHelper
    {
        private Guid associationGuid;
        public PSMAssociation PSMAssociation
        {
            get { return Project.TranslateComponent<PSMAssociation>(associationGuid); }
            set 
            { 
                associationGuid = value; NotifyPropertyChanged("PSMAssociation");
                
                if (MainLabelViewHelper == null)
                {
                    MainLabelViewHelper = new LabelViewHelper(Diagram);
                }
            }
        }
        public PSMAssociationViewHelper(Diagram diagram)
            : base(diagram)
        {
            MainLabelViewHelper = new LabelViewHelper(diagram);
            CardinalityLabelViewHelper = new LabelViewHelper(diagram);
        }

        public Component Component
        {
            get { return PSMAssociation; }
            set { PSMAssociation = (PSMAssociation) value; }
        }

        public LabelViewHelper MainLabelViewHelper { get; set; }

        public LabelViewHelper CardinalityLabelViewHelper { get; set; }

        //private readonly ObservablePointCollection points = new ObservablePointCollection();

        ///// <summary>
        ///// Returns association points.
        ///// </summary>
        //public override ObservablePointCollection Points
        //{
        //    get { return points; }
        //}

        public override Versioning.IExolutioCloneable Clone(ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            return new PSMAssociationViewHelper(projectVersion.Project.TranslateComponent<Diagram>(createdCopies.GetGuidForCopyOf(Diagram)));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion, ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            PSMAssociationViewHelper copyAssociationViewHelper = (PSMAssociationViewHelper)copyComponent;

            MainLabelViewHelper.FillCopy(copyAssociationViewHelper.MainLabelViewHelper, projectVersion, createdCopies);
            CardinalityLabelViewHelper.FillCopy(copyAssociationViewHelper.CardinalityLabelViewHelper, projectVersion, createdCopies);
        }

        public override void Serialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Serialize(parentNode, context);

            //this.SerializePointsCollection(Points, parentNode, context);
            this.SerializeToChildElement("MainLabelViewHelper", MainLabelViewHelper, parentNode, context);
            this.SerializeToChildElement("CardinalityLabelViewHelper", CardinalityLabelViewHelper, parentNode, context);

        }

        public override void Deserialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            //this.DeserializePointsCollection(Points, parentNode, context);
            MainLabelViewHelper.DeserializeFromChildElement("MainLabelViewHelper", parentNode, context);
            CardinalityLabelViewHelper.DeserializeFromChildElement("CardinalityLabelViewHelper", parentNode, context);
        }
    }
}