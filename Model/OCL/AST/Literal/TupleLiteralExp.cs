using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A TupleLiteralExp denotes a tuple value. It contains a name and a value for each part of the tuple type.
    /// </summary>
    public class TupleLiteralExp : LiteralExp
    {

        public TupleLiteralExp(Dictionary<string, TupleLiteralPart> parts, Classifier type):base(type) {
            // tady by mozna moch bejt nakej check typu
            this.Parts = parts;
        }


        /// <summary>
        /// The Variable declarations defining the parts of the literal.
        /// </summary>
        public Dictionary<string,TupleLiteralPart> Parts
        {
            get;
            protected set;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }

        public override void Accept(IAstVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
