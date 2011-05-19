using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model.OCL.Types;

namespace EvoX.Model.OCL.AST
{
    /// <summary>
    /// A BooleanLiteralExp represents the value true or false of the predefined type Boolean.
    /// </summary>
    public class BooleanLiteralExp : PrimitiveLiteralExp
    {
        public bool Value
        {
            get;
            set;
        }

        public override Types.Classifier Type
        {
            get
            {
                return new BooleanType();
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }
    }
}
