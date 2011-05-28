using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;


namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// An UnlimitedNaturalLiteralExp denotes a value of the predefined type UnlimitedNatural.
    /// </summary>
    public class UnlimitedNaturalLiteralExp : NumericLiteralExp
    {
        //TODO: Doplnit Value do UnlimitedNaturalLiteralExp 
        public override Types.Classifier Type
        {
            get
            {
                return new UnlimitedNaturalType();
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }
}
    }

        
