using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// An IterateExp is an expression that evaluates its body expression for each element of a collection. It acts as a loop
    /// construct that iterates over the elements of its source collection and results in a value. An iterate expression evaluates its
    /// body expression for each element of its source collection. The evaluated value of the body expression in each iterationstep
    /// becomes the new value for the result variable for the succeeding iteration-step. The result can be of any type and is
    /// defined by the result association. The IterateExp is the most fundamental collection expression defined in the OCL
    /// Expressions package.
    /// </summary>
    public class IterateExp : LoopExp
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Source expression of iterotor. Itertor is calling on this expression.</param>
        /// <param name="body">Iterate expression</param>
        /// <param name="iterator">Iterator variable</param>
        /// <param name="resultVariable">Accumulate variable</param>
        public IterateExp(OclExpression source, OclExpression body, VariableDeclaration iterator, VariableDeclaration resultVariable)
            : base(source, body, iterator,resultVariable.PropertyType) {
                this.Result = resultVariable;
        }

        /// <summary>
        /// The Variable that represents the result variable.
        /// </summary>
        public VariableDeclaration Result
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
