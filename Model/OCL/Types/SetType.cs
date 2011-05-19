using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.Types
{
    /// <summary>
    /// SetType is a collection type that describes a set of elements where each distinct element occurs only once in the set. The
    /// elements are not ordered.
    /// </summary>
    public class SetType : CollectionType
    {
        override public CollectionKind CollectionKind
        {
            get
            {
                return CollectionKind.Set;
            }
        }

        static NonCompositeType<SetType> simpleRepresentation = NonCompositeType<SetType>.Instance;
        public override NonCompositeType SimpleRepresentation
        {
            get
            {
                return simpleRepresentation;
            }
        }

        public override Classifier CommonSuperType(Classifier other)
        {
            Classifier common = CommonSuperType<SetType>(other);
            if (common == null)
                return base.CommonSuperType(other);
            else
                return common;
        }

        public SetType(Classifier elemetnType)
            : base(elemetnType)
        {
        }

        public SetType()
            : base()
        {
        }
    }
}
