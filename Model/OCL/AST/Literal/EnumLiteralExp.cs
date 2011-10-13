using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// An EnumLiteralExp represents a reference to an enumeration literal.
    /// </summary>
    public class EnumLiteralExp : LiteralExp
    {
        public EnumLiteralExp(EnumLiteral referredEnumLiteral):base(referredEnumLiteral.Type) {
            this.ReferredEnumLiteral = referredEnumLiteral;
        }

        /// <summary>
        /// The EnumLiteral to which the enum expression refers.
        /// </summary>
        public EnumLiteral ReferredEnumLiteral
        {
            get;
            set;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }

        public override void Accept(IAstVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
