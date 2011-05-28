using Exolutio.Model.PSM;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation.Changes
{
    [ChangePredicateScope(EChangePredicateScope.PSMAttribute)]
    public abstract class AttributeChangeInstance : ChangeInstance, IChangeInstance<PSMAttribute>
    {
        protected AttributeChangeInstance(PSMComponent component, Version oldVersion, Version newVersion) 
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangePredicateScope Scope
        {
            get { return EChangePredicateScope.PSMAttribute; }
        }

        PSMAttribute IChangeInstance<PSMAttribute>.Component
        {
            get { return (PSMAttribute) Component; }
        }

        public PSMAttribute PSMAttribute
        {
            get { return ((IChangeInstance<PSMAttribute>) this).Component; }
        }
    }

    public class AttributeAddedChangeInstance : AttributeChangeInstance
    {
        public AttributeAddedChangeInstance(PSMComponent component, Version oldVersion, Version newVersion) 
            : base(component, oldVersion, newVersion)
        {
        }

        [ChangePredicateParameter]
        public PSMClass Parent
        {
            get { return PSMAttribute.PSMClass; }
        }

        [ChangePredicateParameter]
        public int Index
        {
            get { return Parent.PSMAttributes.IndexOf(PSMAttribute); }
        }

        public override string ToString()
        {
            return string.Format("A new attribute '{0}' was added to class '{1}' at position '{2}'.", PSMAttribute, Parent, Index);
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Addition; }
        }   

        public static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return AdditionTest(candidate, oldVersion, newVersion);
        }

        public static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return new AttributeAddedChangeInstance(candidate, oldVersion, newVersion);
        }
    }

    public class AttributeRemovedChangeInstance : AttributeChangeInstance
    {
        public AttributeRemovedChangeInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public override string ToString()
        {
            return string.Format("Attribute '{0}' was removed.", PSMAttribute);
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Removal; }
        }

        public static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return RemovalTest(candidate, oldVersion, newVersion);
        }

        public static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return new AttributeRemovedChangeInstance(candidate, oldVersion, newVersion);
        }
    }
}