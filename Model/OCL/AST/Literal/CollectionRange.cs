using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A CollectionRange represents a range of integers.
    /// </summary>
    public class CollectionRange : CollectionLiteralPart
    {
        public OclExpression First
        {
            set;
            get;
        }

        public OclExpression Last
        {
            set;
            get;
        }

        public override Types.Classifier Type {
            get {
                return First.Type.CommonSuperType(Last.Type);
            }

            protected set {
                throw new InvalidOperationException();
            }
        }
    }
}
