using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A PrimitiveLiteralExp literal denotes a value of a primitive type.
    /// </summary>
    public class PrimitiveLiteralExp : LiteralExp
    {
        protected PrimitiveLiteralExp(Types.Classifier type) : base(type) { }

        
    }
}
