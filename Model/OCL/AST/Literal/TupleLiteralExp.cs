using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A TupleLiteralExp denotes a tuple value. It contains a name and a value for each part of the tuple type.
    /// </summary>
    public class TupleLiteralExp : LiteralExp
    {
        /// <summary>
        /// The Variable declarations defining the parts of the literal.
        /// </summary>
        public List<TupleLiteralPart> Parts
        {
            get;
            set;
        }
    }
}
