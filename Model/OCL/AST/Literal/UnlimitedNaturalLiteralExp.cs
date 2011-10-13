using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;


namespace Exolutio.Model.OCL.AST {
    /// <summary>
    /// An UnlimitedNaturalLiteralExp denotes a value of the predefined type UnlimitedNatural.
    /// </summary>
    public class UnlimitedNaturalLiteralExp : NumericLiteralExp {

        public UnlimitedNaturalLiteralExp(Classifier type)
            : base(type) {
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }

        public override void Accept(IAstVisitor visitor) {
            visitor.Visit(this);
        }
    }
}


