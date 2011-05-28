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
    public class PIMAssociationViewHelper : ConnectionViewHelper, IComponentViewHelper
    {
        private Guid associationGuid;
        public PIMAssociation PIMAssociation
        {
            get { return Project.TranslateComponent<PIMAssociation>(associationGuid); }
            set 
            { 
                associationGuid = value; NotifyPropertyChanged("PIMAssociation");
                if (associationEndsViewHelpers.Count == 0)
                {
                    CreateEndsViewHelpers(PIMAssociation);
                }
                if (MainLabelViewHelper == null)
                {
                    MainLabelViewHelper = new LabelViewHelper(Diagram);
                }
            }
        }

        public override Versioning.IExolutioCloneable Clone(ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            return new PIMAssociationViewHelper(projectVersion.Project.TranslateComponent<Diagram>(createdCopies.GetGuidForCopyOf(Diagram)));
        }

        public PIMAssociationViewHelper(Diagram diagram)
            : base(diagram)
        {
            MainLabelViewHelper = new LabelViewHelper(diagram);
        }

        public Component Component
        {
            get { return PIMAssociation; }
            set { PIMAssociation = (PIMAssociation) value; }
        }

        #region Main Label properties

        public LabelViewHelper MainLabelViewHelper { get; set; }

        #endregion

        private bool useDiamond;
        public bool UseDiamond
        {
            get
            {
                return useDiamond;
            }
            set
            {
                if (AssociationEndsViewHelpers.Count > 2 && value == false)
                {
                    throw new InvalidOperationException("Association must have an association diamond. It has more than 2 ends. ");
                }

                useDiamond = value;
                NotifyPropertyChanged("UseDiamond");
            }
        }

        public bool ForceUseDiamond { get; set; }

        /// <summary>
        /// Returns association points, but only for associations that do not use central diamond 
        /// (associations with more than two ends)
        /// </summary>
        /// <seealso cref="UseDiamond"/>
        public override ObservablePointCollection Points
        {
            get
            {
                if (UseDiamond)
                {
                    throw new InvalidOperationException("Points collection is valid only if UseDiamond is false.");
                }
                return associationEndsViewHelpers[0].Points;
            }
        }


        private readonly List<PIMAssociationEndViewHelper> associationEndsViewHelpers = new List<PIMAssociationEndViewHelper>();
        public IList<PIMAssociationEndViewHelper> AssociationEndsViewHelpers
        {
            get
            {
                return associationEndsViewHelpers;
            }
        }

        /// <summary>
        /// Creates <see cref="PIMAssociationEndViewHelper"/> for each <paramref name="association"/>'s end.  
        /// </summary>
        /// <param name="association">model association</param>
        public void CreateEndsViewHelpers(PIMAssociation association)
        {
            //associationEndsViewHelpers.Clear();

            List<PIMAssociationEndViewHelper> used = new List<PIMAssociationEndViewHelper>();

            // add new ends
            foreach (PIMAssociationEnd associationEnd in association.PIMAssociationEnds)
            {
                PIMAssociationEndViewHelper endViewHelper = associationEndsViewHelpers.Where(helper => helper.AssociationEnd == associationEnd).SingleOrDefault();

                if (endViewHelper == null)
                {
                    endViewHelper = new PIMAssociationEndViewHelper(Diagram, this);
                    endViewHelper.AssociationEnd = associationEnd;
                    associationEndsViewHelpers.Add(endViewHelper);
                }

                used.Add(endViewHelper);
            }

            if (used.Count == 2 && associationEndsViewHelpers.Count > 2 && UseDiamond)
            {
                associationEndsViewHelpers[0].Points.Clear();
            }

            associationEndsViewHelpers.RemoveAll(helper => !used.Contains(helper));

            if (association.PIMAssociationEnds.Count > 2 || ForceUseDiamond)
            {
                UseDiamond = true;
            }
            else
            {
                UseDiamond = false;
            }
        }

        public override void FillCopy(Versioning.IExolutioCloneable copyComponent, ProjectVersion projectVersion, Versioning.ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyComponent, projectVersion, createdCopies);
            PIMAssociationViewHelper copyAssociationViewHelper = (PIMAssociationViewHelper)copyComponent;
            copyAssociationViewHelper.UseDiamond = UseDiamond;

            MainLabelViewHelper.FillCopy(copyAssociationViewHelper.MainLabelViewHelper, projectVersion, createdCopies);

            for (int i = 0; i < AssociationEndsViewHelpers.Count; i++)
            {
                PIMAssociationEndViewHelper associationEndViewHelper = AssociationEndsViewHelpers[i];
                PIMAssociationEnd copyEnd = projectVersion.Project.TranslateComponent<PIM.PIMAssociation>(createdCopies.GetGuidForCopyOf(associationEndViewHelper.AssociationEnd.PIMAssociation)).PIMAssociationEnds[i];
                System.Diagnostics.Debug.Assert(copyEnd.PIMClass == projectVersion.Project.TranslateComponent<PIM.PIMClass>(createdCopies.GetGuidForCopyOf(associationEndViewHelper.AssociationEnd.PIMClass)));
                PIMAssociationEndViewHelper endViewHelperCopy = new PIMAssociationEndViewHelper(copyAssociationViewHelper.Diagram, copyAssociationViewHelper);
                endViewHelperCopy.AssociationEnd = copyEnd;
                associationEndViewHelper.FillCopy(endViewHelperCopy, projectVersion, createdCopies);
                copyAssociationViewHelper.AssociationEndsViewHelpers.Add(endViewHelperCopy);
            }
        }

        public override void Serialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Serialize(parentNode, context);

            this.SerializeSimpleValueToElement("UseDiamond", UseDiamond, parentNode, context);
            //this.SerializePointsCollection(Points, parentNode, context);
            this.SerializeToChildElement("MainLabelViewHelper", MainLabelViewHelper, parentNode, context);
            this.WrapAndSerializeCollection("AssociationEndsViewHelpers", "AssociationEndViewHelper", AssociationEndsViewHelpers, parentNode, context);
        }

        public override void Deserialize(XElement parentNode, Serialization.SerializationContext context)
        {
            base.Deserialize(parentNode, context);

            UseDiamond = bool.Parse(this.DeserializeSimpleValueFromElement("UseDiamond", parentNode, context));
            //this.DeserializePointsCollection(Points, parentNode, context);
            MainLabelViewHelper.DeserializeFromChildElement("MainLabelViewHelper", parentNode, context);

            int i = 0;
            foreach (XElement aeElement in parentNode.Element(context.ExolutioNS + "AssociationEndsViewHelpers").Elements(context.ExolutioNS + "AssociationEndViewHelper"))
            {
                AssociationEndsViewHelpers[i++].Deserialize(aeElement, context);
            }
        }
    }
}