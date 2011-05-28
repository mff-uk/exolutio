using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A CollectionItem represents an individual element of a collection.
    /// </summary>
    public class CollectionItem : CollectionLiteralPart
    {
        public OclExpression Item
        {
            get;
            set;
        }

        public override Types.Classifier Type
        {
            get
            {
                return Item.Type;
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }
    }
}
