using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.Types
{
    /// <summary>
    /// Matches UML.Classes.Kernel.DateType from UML SuperStructure
    /// </summary>
    public abstract class DataType:Classifier
    {
        public DataType(string name)
            : base(name)
        { }
    }
}
