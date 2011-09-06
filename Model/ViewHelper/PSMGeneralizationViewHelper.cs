using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Exolutio.SupportingClasses;
using Exolutio.Model.Serialization;
using Exolutio.Model.PSM;

namespace Exolutio.Model.ViewHelper
{
    /// <summary>
    /// Stores visualization information for Associations 
    /// </summary>
    public class PSMGeneralizationViewHelper : ConnectionViewHelper, IComponentViewHelper
    {
        private Guid generalizationGuid;
        public PSMGeneralization PSMGeneralization
        {
            get { return Project.TranslateComponent<PSMGeneralization>(generalizationGuid); }
            set 
            { 
                generalizationGuid = value; NotifyPropertyChanged("PSMGeneralization");
            }
        }

        public PSMGeneralizationViewHelper(Diagram diagram)
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
            get { return PSMGeneralization; }
            set { PSMGeneralization = (PSMGeneralization) value; }
        }

        public override Versioning.IExolutioCloneable Clone(ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            return new PSMGeneralizationViewHelper(projectVersion.Project.TranslateComponent<Diagram>(createdCopies.GetGuidForCopyOf(Diagram)));
        }

        public override void FillCopy(Versioning.IExolutioCloneable copyComponent, ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);
            PSMGeneralizationViewHelper copyGeneralizationViewHelper = (PSMGeneralizationViewHelper)copyComponent;
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