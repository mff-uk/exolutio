using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A CollectionLiteralExp represents a reference to collection literal.
    /// </summary>
    public class CollectionLiteralExp : LiteralExp
    {
        /// <summary>
        /// The kind of collection literal that is specified by this CollectionLiteralExp.
        /// </summary>
        public CollectionKind Kind
        {
            get;
            set;
        }


        /// <summary>
        /// The parts of the collection literal expression.
        /// </summary>
        public List<CollectionLiteralPart> Parts
        {
            get;
            set;
        }
    }
}
