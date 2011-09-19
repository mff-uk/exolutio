using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// An IntegerLiteralExp denotes a value of the predefined type Integer.
    /// </summary>
    public class IntegerLiteralExp : NumericLiteralExp
    {
        public IntegerLiteralExp(long value,Types.Classifier type):base(type) {
            Value = value;
        }

        public long Value
        {
            get;
            set;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }
}
