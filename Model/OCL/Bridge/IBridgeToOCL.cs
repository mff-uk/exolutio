using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Bridge {
    public interface IBridgeToOCL {
        TypesTable.TypesTable TypesTable {
            get;
        }
    }
}
