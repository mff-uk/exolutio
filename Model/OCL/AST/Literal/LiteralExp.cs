using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A LiteralExp is an expression with no arguments producing a value. In general the result value is identical with the
    /// expression symbol.
    /// </summary>
    public class LiteralExp : OclExpression
    {
        protected LiteralExp(Types.Classifier type):base(type) {
           
        }

       
    }
}
