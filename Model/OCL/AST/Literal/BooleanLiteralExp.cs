using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A BooleanLiteralExp represents the value true or false of the predefined type Boolean.
    /// </summary>
    public class BooleanLiteralExp : PrimitiveLiteralExp
    {
        public BooleanLiteralExp(bool value,Classifier type):base(type) {
            Value = value;
        }

        public bool Value
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
