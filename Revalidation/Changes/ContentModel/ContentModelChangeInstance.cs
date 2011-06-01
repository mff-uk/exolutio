using System;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation.Changes
{
    [ChangePredicateScope(EChangePredicateScope.PSMContentModel)]
    public abstract class ContentModelChangeInstance : ChangeInstance, IChangeInstance<PSMContentModel>
    {
        protected ContentModelChangeInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangePredicateScope Scope
        {
            get { return EChangePredicateScope.PSMContentModel; }
        }

        PSMContentModel IChangeInstance<PSMContentModel>.Component
        {
            get { return (PSMContentModel)Component; }
        }

        public PSMContentModel PSMContentModel
        {
            get { return ((IChangeInstance<PSMContentModel>)this).Component; }
        }
    }

    public class ContentModelAddedInstance : ContentModelChangeInstance
    {
        public ContentModelAddedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        [ChangePredicateParameter]
        public PSMAssociation ParentAssociation { get; private set; }

        public override string ToString()
        {
            return string.Format("A new content model '{0}' was added under '{1}'.", PSMContentModel, ParentAssociation);
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Addition; }
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return AdditionTest(candidate, oldVersion, newVersion);
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMContentModel psmContentModel = ((PSMContentModel)candidate);
            return new ContentModelAddedInstance(candidate, oldVersion, newVersion) { ParentAssociation = psmContentModel.ParentAssociation };
        }
    }

    public class ContentModelRemovedInstance : ContentModelChangeInstance
    {
        public ContentModelRemovedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override string ToString()
        {
            return string.Format("Content model '{0}' was removed.", PSMContentModel);
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Removal; }
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return RemovalTest(candidate, oldVersion, newVersion);
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return new ContentModelRemovedInstance(candidate, oldVersion, newVersion);
        }
    }

    public class ContentModelMovedInstance : ContentModelChangeInstance
    {
        public ContentModelMovedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Migratory; }
        }

        [ChangePredicateParameter]
        public PSMAssociation NewParentAssociation { get; set; }

        public PSMAssociation OldParentAssociation { get; set; }

        public override string ToString()
        {
            return string.Format("ContentModel '{0}' was moved from '{1}' to '{2}'.", PSMContentModel, OldParentAssociation, NewParentAssociation);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMContentModel psmContentModel = (PSMContentModel)candidate;
            PSMContentModel psmContentModelO = (PSMContentModel)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) &&
                   !AreLinked(psmContentModelO.ParentAssociation, psmContentModel.ParentAssociation);
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMContentModel psmContentModel = (PSMContentModel)candidate;
            PSMContentModel psmContentModelO = (PSMContentModel)candidate.GetInVersion(oldVersion);
            return new ContentModelMovedInstance(candidate, oldVersion, newVersion)
            {
                NewParentAssociation = psmContentModel.ParentAssociation,
                OldParentAssociation = psmContentModelO.ParentAssociation
            };
        }
    }

    public class ContentModelTypeChangedInstance : ContentModelChangeInstance
    {
        public ContentModelTypeChangedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Sedentary; }
        }

        public PSMContentModelType OldType { get; set; }

        [ChangePredicateParameter]
        public PSMContentModelType NewType { get; set; }

        public override string ToString()
        {
            return string.Format("Content model '{0}' type changed from '{1}' to '{2}'.", PSMContentModel,
                OldType,
                NewType);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMContentModel psmContentModel = (PSMContentModel)candidate;
            PSMContentModel psmContentModelO = (PSMContentModel)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) && psmContentModel.Type != psmContentModelO.Type;
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMContentModel psmContentModel = (PSMContentModel)candidate;
            PSMContentModel psmContentModelO = (PSMContentModel)candidate.GetInVersion(oldVersion);
            return new ContentModelTypeChangedInstance(candidate, oldVersion, newVersion)
            {
                OldType = psmContentModelO.Type,
                NewType = psmContentModel.Type
            };
        }
    }
}