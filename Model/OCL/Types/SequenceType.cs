using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types {
    /// <summary>
    /// SequenceType is a collection type that describes a list of elements where each element may occur multiple times in the
    /// sequence. The elements are ordered by their position in the sequence.
    /// </summary>
    public class SequenceType : CollectionType {
        public SequenceType(TypesTable.TypesTable tt, Classifier elemetnType, Classifier superClassifier)
            : base(tt, CollectionKind.Sequence, elemetnType, superClassifier) {
        }

        public override Classifier CommonSuperType(Classifier other) {
            Classifier common = CommonSuperType<SequenceType>((tt, el) => (SequenceType)tt.Library.CreateCollection(OCL.CollectionKind.Sequence, el), other);
            if (common == null)
                return base.CommonSuperType(other);
            else
                return common;
        }
    }
}
