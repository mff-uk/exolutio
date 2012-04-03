using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;


namespace Exolutio.Model.OCL.AST
{
    public class StringLiteralExp : PrimitiveLiteralExp
    {
        public StringLiteralExp(string value,Classifier type):base(type) {
            Value = value;
        }

        /// <summary>
        /// A StringLiteralExp denotes a value of the predefined type String.
        /// </summary>
        public string Value {
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
