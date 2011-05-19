using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace EvoX.Model.OCL.AST
{
    /// <summary>
    /// An EnumLiteralExp represents a reference to an enumeration literal.
    /// </summary>
    public class EnumLiteralExp : LiteralExp
    {
        /// <summary>
        /// The EnumLiteral to which the enum expression refers.
        /// </summary>
        public EnumLiteral ReferredEnumLiteral
        {
            get;
            set;
        }

        public override Types.Classifier Type
        {
            get
            {
                return ReferredEnumLiteral.Type;
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }
    }
}
