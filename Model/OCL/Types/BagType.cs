using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
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


        public override Classifier CommonSuperType(Classifier other)
        {
            Classifier common = CommonSuperType<BagType>((tt, el) => (BagType)tt.Library.CreateCollection(OCL.CollectionKind.Bag,el), other);
            if (common == null)
                return base.CommonSuperType(other);
            else
                return common;
        }

        public BagType(TypesTable.TypesTable tt, Classifier elemetnType, Classifier superClassifier)
            : base(tt,elemetnType,superClassifier)
        { }

   
    }
}
