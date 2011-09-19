using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A VariableExp is an expression that consists of a reference to a variable. References to the variables self and result or to
    /// variables defined by Let expressions are examples of such variable expressions.
    /// </summary>
    public class VariableExp:OclExpression
    {
        public VariableExp(VariableDeclaration variable)
            : base(variable.PropertyType) {
            referredVariable = variable;
        }
        /// <summary>
        /// The Variable to which this variable expression refers.
        /// </summary>
        public VariableDeclaration referredVariable
        {
            get;
            set;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }
}
