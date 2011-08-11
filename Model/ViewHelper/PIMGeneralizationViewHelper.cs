using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.PIM;
using Exolutio.SupportingClasses;
using Exolutio.Model.Serialization;

namespace Exolutio.Model.ViewHelper
{
    /// <summary>
    /// Stores visualization information for Associations 
    /// </summary>
    public class PIMGeneralizationViewHelper : ConnectionViewHelper, IComponentViewHelper
    {
        private Guid generalizationGuid;
        public PIMGeneralization PIMGeneralization
        {
            get { return Project.TranslateComponent<PIMGeneralization>(generalizationGuid); }
            set 
            { 
                generalizationGuid = value; NotifyPropertyChanged("PIMGeneralization");
            }
        }

        public PIMGeneralizationViewHelper(Diagram diagram)
            : base(diagram)
        {

        }

        private readonly ObservablePointCollection points = new ObservablePointCollection();

        public override ObservablePointCollection Points
        {
            get
            {
                return points;
            }
        }

        public Component Component
        {
            get { return PIMGeneralization; }
            set { PIMGeneralization = (PIMGeneralization) value; }
        }

        public override Versioning.IExolutioCloneable Clone(ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            return new PIMGeneralizationViewHelper(projectVersion.Project.TranslateComponent<Diagram>(createdCopies.GetGuidForCopyOf(Diagram)));
        }

        public override void FillCopy(Versioning.IExolutioCloneable copyComponent, ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);
            PIMGeneralizationViewHelper copyGeneralizationViewHelper = (PIMGeneralizationViewHelper)copyComponent;
            copyGeneralizationViewHelper.Points.AppendRangeAsCopy(this.Points);
        }

        public override void Serialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Serialize(parentNode, context);

            this.SerializePointsCollection(Points, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            this.DeserializePointsCollection(Points, parentNode, context);
        }
    }
}