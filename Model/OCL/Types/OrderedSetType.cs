using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// OrderedSetType is a collection type that describes a set of elements where each distinct element occurs only once in the
    /// set. The elements are ordered by their position in the sequence.
    /// </summary>
    public class OrderedSetType : CollectionType
    {
        override public CollectionKind CollectionKind
        {
            get
            {
                return CollectionKind.OrderedSet;
            }
        }

        static NonCompositeType<OrderedSetType> simpleRepresentation = NonCompositeType<OrderedSetType>.Instance;
        public override NonCompositeType SimpleRepresentation
        {
            get
            {
                return simpleRepresentation;
            }
        }

        public override Classifier CommonSuperType(Classifier other)
        {
            Classifier common = CommonSuperType<OrderedSetType>(other);
            if (common == null)
                return base.CommonSuperType(other);
            else
                return common;
        }

        public OrderedSetType(Classifier elemetnType )
            : base(elemetnType)
        {
        }

        public OrderedSetType()
            : base()
        {
        }
    }
}
