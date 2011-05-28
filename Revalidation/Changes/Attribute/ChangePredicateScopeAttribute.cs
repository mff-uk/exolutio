using System;

namespace Exolutio.Revalidation.Changes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ChangePredicateScopeAttribute : Attribute
    {
        public EChangePredicateScope Scope { get; set; }

        public ChangePredicateScopeAttribute(EChangePredicateScope scope)
        {
            Scope = scope;
        }
    }
}