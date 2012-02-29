using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;

namespace Exolutio.Model.OCL.Bridge {
    static class BridgeHelpers {
        static public Classifier GetTypeByCardinality(TypesTable.TypesTable tt, IHasCardinality card, Classifier baseType) {
            if (card.Upper > 1) {
                return tt.Library.CreateCollection(CollectionKind.Set, baseType);
            }
            else {
                return baseType;
            }
        }
    }
}
