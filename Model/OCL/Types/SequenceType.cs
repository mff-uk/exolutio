using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// SequenceType is a collection type that describes a list of elements where each element may occur multiple times in the
    /// sequence. The elements are ordered by their position in the sequence.
    /// </summary>
    public class SequenceType : CollectionType
    {
        override public CollectionKind CollectionKind
        {
            get
            {
                return CollectionKind.Sequence;
            }
        }

        static NonCompositeType < SequenceType> simpleRepresentation =  NonCompositeType < SequenceType>.Instance;
        public override NonCompositeType  SimpleRepresentation
        {
            get
            {
                return simpleRepresentation;
            }
        }

        public override Classifier CommonSuperType(Classifier other)
        {
            Classifier common = CommonSuperType<SequenceType>(other);
            if (common == null)
                return base.CommonSuperType(other);
            else
                return common;
        }

        public SequenceType(Classifier elemetnType)
            : base(elemetnType)
        {
        }

        public SequenceType()
            : base()
        {
        }
    }
}
