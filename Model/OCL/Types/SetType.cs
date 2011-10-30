using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
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



        public override Classifier CommonSuperType(Classifier other)
        {
            Classifier common = CommonSuperType<SetType>((tt, el) => (SetType)tt.Library.CreateCollection(OCL.CollectionKind.Set, el), other);
            if (common == null)
                return base.CommonSuperType(other);
            else
                return common;
        }

        public SetType(TypesTable.TypesTable tt, Classifier elemetnType, Classifier superClassifier)
            : base(tt,elemetnType,superClassifier)
        {
        }

       
    }
}
