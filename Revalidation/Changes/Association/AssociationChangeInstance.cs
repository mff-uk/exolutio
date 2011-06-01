using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;

namespace Exolutio.Revalidation.Changes
{
    [ChangePredicateScope(EChangePredicateScope.PSMAssociation)]
    public abstract class AssociationChangeInstance : ChangeInstance, IChangeInstance<PSMAssociation>
    {
        protected AssociationChangeInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangePredicateScope Scope
        {
            get { return EChangePredicateScope.PSMAssociation; }
        }

        PSMAssociation IChangeInstance<PSMAssociation>.Component
        {
            get { return (PSMAssociation)Component; }
        }

        public PSMAssociation PSMAssociation
        {
            get { return ((IChangeInstance<PSMAssociation>)this).Component; }
        }


    }

    public class AssociationAddedInstance : AssociationChangeInstance
    {
        public AssociationAddedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        [ChangePredicateParameter]
        public PSMAssociationMember Parent { get; private set; }

        [ChangePredicateParameter]
        public int Index { get; private set; }

        public override string ToString()
        {
            return string.Format("A new association '{0}' was added under '{1}' at position '{2}'.", PSMAssociation, Parent, Index);
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
            PSMAssociation psmAssociation = ((PSMAssociation)candidate);
            return new AssociationAddedInstance(candidate, oldVersion, newVersion) { Parent = psmAssociation.Parent, Index = psmAssociation.Index };
        }
    }

    public class AssociationRemovedInstance : AssociationChangeInstance
    {
        public AssociationRemovedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override string ToString()
        {
            return string.Format("Association '{0}' was removed.", PSMAssociation);
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
            return new AssociationRemovedInstance(candidate, oldVersion, newVersion);
        }
    }

    public class AssociationRenamedInstance : AssociationChangeInstance
    {
        public AssociationRenamedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Sedentary; }
        }

        public string OldName { get; set; }

        [ChangePredicateParameter]
        public string NewName { get; set; }

        public override string ToString()
        {
            return string.Format("Association '{0}' was renamed from '{1}' to '{2}'.", PSMAssociation, OldName, NewName);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return ExistingTest(candidate, oldVersion, newVersion) && candidate.Name != candidate.GetInVersion(oldVersion).Name;
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return new AssociationRenamedInstance(candidate, oldVersion, newVersion) { OldName = candidate.GetInVersion(oldVersion).Name, NewName = candidate.Name };
        }
    }

    public class AssociationMovedInstance : AssociationChangeInstance
    {
        public AssociationMovedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Migratory; }
        }

        [ChangePredicateParameter]
        public PSMAssociationMember NewParent { get; set; }

        [ChangePredicateParameter]
        public int NewIndex { get; set; }

        public PSMAssociationMember OldParent { get; set; }

        public int OldIndex { get; set; }

        public override string ToString()
        {
            return string.Format("Association '{0}' was moved from '{1}' to '{2}' to index '{3}'.", PSMAssociation, OldParent, NewParent, NewIndex);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAssociation psmAssociation = (PSMAssociation)candidate;
            PSMAssociation psmAssociationO = (PSMAssociation)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) &&
                   !AreLinked(psmAssociationO.Parent, psmAssociation.Parent);
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAssociation psmAssociation = (PSMAssociation)candidate;
            PSMAssociation psmAssociationO = (PSMAssociation)candidate.GetInVersion(oldVersion);
            return new AssociationMovedInstance(candidate, oldVersion, newVersion)
            {
                NewParent = psmAssociation.Parent,
                OldParent = psmAssociationO.Parent,
                NewIndex = psmAssociation.Index,
                OldIndex = psmAssociationO.Index
            };
        }
    }

    public class AssociationIndexChangedInstance : AssociationChangeInstance
    {
        public AssociationIndexChangedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Migratory; }
        }

        public int OldIndex { get; set; }

        [ChangePredicateParameter]
        public int NewIndex { get; set; }

        public override string ToString()
        {
            return string.Format("Association '{0}' index changed from '{1}' to '{2}'.", PSMAssociation,
                OldIndex,
                NewIndex);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAssociation psmAssociation = (PSMAssociation)candidate;
            PSMAssociation psmAssociationO = (PSMAssociation)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) 
                && AreLinked(psmAssociationO.Parent, psmAssociation.Parent)
                && psmAssociation.Index != psmAssociationO.Index;
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAssociation psmAssociation = (PSMAssociation)candidate;
            PSMAssociation psmAssociationO = (PSMAssociation)candidate.GetInVersion(oldVersion);
            return new AssociationIndexChangedInstance(candidate, oldVersion, newVersion)
            {
                OldIndex = psmAssociationO.Index,
                NewIndex = psmAssociation.Index
            };
        }
    }
}