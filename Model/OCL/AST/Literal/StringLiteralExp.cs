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

        private string value;

        /// <summary>
        /// A StringLiteralExp denotes a value of the predefined type String.
        /// </summary>
        public string Value
        {
            get { return value; }
            set
            {
                // HACK: J.M. : temporary hack to remove apostrophes left by the compiler
                if (value.StartsWith(@"'") && value.EndsWith(@"'") && value.Length >= 2)
                {
                    this.value = value.Substring(1, value.Length - 2);
                }
                else
                {
                    this.value = value;
                }
            }
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }

        public override void Accept(IAstVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
