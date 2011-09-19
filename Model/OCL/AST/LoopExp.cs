using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A LoopExp is an expression that represents a loop construct over a collection. It has an iterator variable that represents the
    /// elements of the collection during iteration. The body expression is evaluated for each element in the collection. The result
    /// of a loop expression depends on the specific kind and its name.
    /// </summary>
    public class LoopExp : CallExp
    {
        public LoopExp(OclExpression source, OclExpression body, List<VariableDeclaration> iterators, Classifier type)
            : base(source,type) {
            this.Body = body;
            this.Iterator = iterators;
        }

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
        public List<VariableDeclaration> Iterator
        {
            get;
            set;
        }
    }
}
