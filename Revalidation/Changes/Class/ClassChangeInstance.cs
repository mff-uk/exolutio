using System;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation.Changes
{
    [ChangePredicateScope(EChangePredicateScope.PSMClass)]
    public abstract class ClassChangeInstance : ChangeInstance, IChangeInstance<PSMClass>
    {
        protected ClassChangeInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangePredicateScope Scope
        {
            get { return EChangePredicateScope.PSMClass; }
        }

        PSMClass IChangeInstance<PSMClass>.Component
        {
            get { return (PSMClass)Component; }
        }

        public PSMClass PSMClass
        {
            get { return ((IChangeInstance<PSMClass>)this).Component; }
        }


    }

    public class ClassAddedInstance : ClassChangeInstance
    {
        public ClassAddedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        [ChangePredicateParameter]
        public PSMAssociation ParentAssociation { get; private set; }

        public override string ToString()
        {
            return string.Format("A new class '{0}' was added under '{1}'.", PSMClass, ParentAssociation);
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
            PSMClass psmClass = ((PSMClass)candidate);
            return new ClassAddedInstance(candidate, oldVersion, newVersion) { ParentAssociation = psmClass.ParentAssociation };
        }
    }

    public class ClassRemovedInstance : ClassChangeInstance
    {
        public ClassRemovedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override string ToString()
        {
            return string.Format("Class '{0}' was removed.", PSMClass);
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
            return new ClassRemovedInstance(candidate, oldVersion, newVersion);
        }
    }

    public class ClassRenamedInstance : ClassChangeInstance
    {
        public ClassRenamedInstance(PSMComponent component, Version oldVersion, Version newVersion)
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
            return string.Format("Class '{0}' was renamed from '{1}' to '{2}'.", PSMClass, OldName, NewName);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return ExistingTest(candidate, oldVersion, newVersion) && candidate.Name != candidate.GetInVersion(oldVersion).Name;
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return new ClassRenamedInstance(candidate, oldVersion, newVersion) { OldName = candidate.GetInVersion(oldVersion).Name, NewName = candidate.Name };
        }
    }

    public class ClassMovedInstance : ClassChangeInstance
    {
        public ClassMovedInstance(PSMComponent component, Version oldVersion, Version newVersion)
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
            return string.Format("Class '{0}' was moved from '{1}' to '{2}'.", PSMClass, OldParentAssociation, NewParentAssociation);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMClass psmClass = (PSMClass)candidate;
            PSMClass psmClassO = (PSMClass)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) &&
                   !AreLinked(psmClassO.ParentAssociation, psmClass.ParentAssociation);
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMClass psmClass = (PSMClass)candidate;
            PSMClass psmClassO = (PSMClass)candidate.GetInVersion(oldVersion);
            return new ClassMovedInstance(candidate, oldVersion, newVersion)
            {
                NewParentAssociation = psmClass.ParentAssociation,
                OldParentAssociation = psmClassO.ParentAssociation
            };
        }
    }

    public class SRIntroducedInstance : ClassChangeInstance
    {
        public SRIntroducedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Sedentary; }
        }

        [ChangePredicateParameter]
        public PSMClass RepresentedPSMClass { get; set; }

        public override string ToString()
        {
            return string.Format("Class '{0}' was made a structural representant of class '{1}'", PSMClass, RepresentedPSMClass);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMClass psmClass = (PSMClass)candidate;
            PSMClass psmClassO = (PSMClass)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) && !psmClassO.IsStructuralRepresentative && psmClass.IsStructuralRepresentative;
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMClass psmClass = (PSMClass)candidate;
            return new SRIntroducedInstance(candidate, oldVersion, newVersion)
                       {
                           RepresentedPSMClass = psmClass.RepresentedClass
                       };
        }
    }

    public class SRRemovedInstance : ClassChangeInstance
    {
        public SRRemovedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Sedentary; }
        }

        public PSMClass OldRepresentedPSMClass { get; set; }

        public override string ToString()
        {
            return string.Format("Class '{0}' is no longer a structural representant of class '{1}'", PSMClass, OldRepresentedPSMClass);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMClass psmClass = (PSMClass)candidate;
            PSMClass psmClassO = (PSMClass)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) && psmClassO.IsStructuralRepresentative && !psmClass.IsStructuralRepresentative;
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMClass psmClassO = (PSMClass)candidate.GetInVersion(oldVersion);
            return new SRRemovedInstance(candidate, oldVersion, newVersion) { OldRepresentedPSMClass = psmClassO.RepresentedClass };
        }
    }
    
    public class SRChangedInstance : ClassChangeInstance
    {
        public SRChangedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Sedentary; }
        }

        [ChangePredicateParameter]
        public PSMClass RepresentedPSMClass { get; set; }
        
        public PSMClass OldRepresentedPSMClass { get; set; }

        public override string ToString()
        {
            return string.Format("Class '{0}' was made a structural representant of class '{1}'", PSMClass, RepresentedPSMClass);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMClass psmClass = (PSMClass)candidate;
            PSMClass psmClassO = (PSMClass)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) && psmClassO.IsStructuralRepresentative && psmClass.IsStructuralRepresentative
                && !AreLinked(psmClassO.RepresentedClass, psmClass.RepresentedClass);
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMClass psmClass = (PSMClass)candidate;
            PSMClass psmClassO = (PSMClass)candidate.GetInVersion(oldVersion);
            return new SRChangedInstance(candidate, oldVersion, newVersion)
            {
                RepresentedPSMClass = psmClass.RepresentedClass,
                OldRepresentedPSMClass = psmClassO.RepresentedClass
            };
        }
    }
}