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

        public OrderedSetType(TypesTable.TypesTable tt, Classifier elemetnType, Classifier superClassifier)
            : base(tt, CollectionKind.OrderedSet, elemetnType, superClassifier) {
        }
  

        public override Classifier CommonSuperType(Classifier other)
        {
            Classifier common = CommonSuperType<OrderedSetType>((tt, el) =>(OrderedSetType)tt.Library.CreateCollection(OCL.CollectionKind.OrderedSet, el), other);
            if (common == null)
                return base.CommonSuperType(other);
            else
                return common;
        }
    }
}
