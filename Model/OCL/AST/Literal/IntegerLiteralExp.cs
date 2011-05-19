using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model.OCL.Types;

namespace EvoX.Model.OCL.AST
{
    /// <summary>
    /// An IntegerLiteralExp denotes a value of the predefined type Integer.
    /// </summary>
    public class IntegerLiteralExp : NumericLiteralExp
    {
        public long Value
        {
            get;
            set;
        }

        public override Types.Classifier Type
        {
            get
            {
                return new IntegerType();
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }
    }
}
