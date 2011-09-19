using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST {
    public class ErrorExp :OclExpression{
        public ErrorExp(Classifier type):base(type) {
            
        }
        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }
}
