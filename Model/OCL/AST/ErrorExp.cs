using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST {
    /// <summary>
    /// ErrorExp represens error in AST.
    /// </summary>
    public class ErrorExp :OclExpression{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public ErrorExp(Classifier type):base(type) {
            
        }
        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }

        public override void Accept(IAstVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
