using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// An IteratorExp is an expression that evaluates its body expression for each element of a collection. It acts as a loop
    /// construct that iterates over the elements of its source collection and results in a value. The type of the iterator expression
    /// depends on the name of the expression, and sometimes on the type of the associated source expression. The IteratorExp
    /// represents all other predefined collection operations that use an iterator. This includes select, collect, reject, forAll, exists,
    /// etc. The OCL Standard Library defines a number of predefined iterator expressions.
    /// </summary>
    public class IteratorExp : LoopExp
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Source expression of iterotor. Itertor is calling on this expression.</param>
        /// <param name="body">Iterator body expression</param>
        /// <param name="name">Name of iterator operation</param>
        /// <param name="iterators">Iterators variable</param>
        /// <param name="returnType">Returns type of iterator</param>
        public IteratorExp(OclExpression source, OclExpression body, string name, List<VariableDeclaration> iterators, Classifier returnType)
            : base(source, body, iterators,returnType) {
                this.IteratorName = name;
        }

        public string IteratorName {
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
