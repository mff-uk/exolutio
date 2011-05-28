using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    public class IntegerType : PrimitiveType
    {
        public IntegerType()
            : base("Integer")
        { }

        public override bool ConformsToRegister(Classifier other)
        {
            if (other.GetType() == typeof(RealType))
                return true;
            return false;// other conformsto rules bring transitivity from RealType 
        }    
    }
}
