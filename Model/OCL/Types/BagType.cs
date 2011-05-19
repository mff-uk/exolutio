using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.Types
{
    /// <summary>
    /// BagType is a collection type that describes a multiset of elements where each element may occur multiple times in the
    /// bag.
    /// </summary>
    public class BagType : CollectionType
    {
        override public CollectionKind CollectionKind
        {
            get
            {
                return CollectionKind.Bag;
            }
        }

        static  NonCompositeType<BagType> simpleRepresentation = NonCompositeType<BagType>.Instance;
        public override  NonCompositeType SimpleRepresentation
        {
            get
            {
                return simpleRepresentation;
            }
        }

        public override Classifier CommonSuperType(Classifier other)
        {
            Classifier common = CommonSuperType<BagType>(other);
            if (common == null)
                return base.CommonSuperType(other);
            else
                return common;
        }

        public BagType(Classifier elemetnType)
            : base(elemetnType)
        { }

        public BagType() : base() { }
    }
}
