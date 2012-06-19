using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.Bridge {
    public interface IBridgeToOCL 
    {
        TypesTable.TypesTable TypesTable {
            get;
        }

        Schema Schema { get;  }
        Classifier Find(Component component);
    }
}
