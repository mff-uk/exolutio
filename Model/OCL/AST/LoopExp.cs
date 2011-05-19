using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.AST
{
    /// <summary>
    /// A LoopExp is an expression that represents a loop construct over a collection. It has an iterator variable that represents the
    /// elements of the collection during iteration. The body expression is evaluated for each element in the collection. The result
    /// of a loop expression depends on the specific kind and its name.
    /// </summary>
    public class LoopExp : CallExp
    {
        /// <summary>
        /// The OclExpression that is evaluated for each element in the source collection.
        /// </summary>
        public OclExpression Body
        {
            get;
            set;
        }

        /// <summary>
        /// The iterator variables. These variables are, each in its turn, bound to every element value of the
        /// source collection while evaluating the body expression.
        /// </summary>
        public List<Variable> Iterator
        {
            get;
            set;
        }
    }
}
