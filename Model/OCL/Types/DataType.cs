using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// Matches UML.Classes.Kernel.DateType from UML SuperStructure
    /// </summary>
    public abstract class DataType:Classifier
    {
        public DataType(TypesTable.TypesTable tt,Namespace ns, string name, Classifier superClassifier)
            : base(tt,ns,name,superClassifier)
        { }
    }
}
