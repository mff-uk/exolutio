using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.Types
{
    public class UnlimitedNaturalType:PrimitiveType
    {


        public UnlimitedNaturalType()
            : base("UnlimitedNatural")
        { }

        public override bool ConformsToRegister(Classifier other)
        {
            if (other.GetType() == typeof(IntegerType))
                return true;
            return false;// other conformsto rules bring transitivity from IntegerType 
            
        }
    }
}
