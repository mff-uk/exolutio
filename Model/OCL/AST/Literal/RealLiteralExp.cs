using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A RealLiteralExp denotes a value of the predefined type Real.
    /// </summary>
    public class RealLiteralExp : NumericLiteralExp
    {
        public double Value
        {
            get;
            set;
        }

        public override Types.Classifier Type
        {
            get
            {
                return new RealType();
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }
    }
}
