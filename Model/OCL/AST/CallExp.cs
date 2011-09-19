using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A CallExp is an expression that refers to a feature (operation, property) or to a predefined iterator for collections. Its
    /// result value is the evaluation of the corresponding feature. This is an abstract metaclass.
    /// </summary>
    public abstract class CallExp : OclExpression
    {
        public CallExp(OclExpression source,Classifier type):base(type) {
            this.Source = source;
        }

        /// <summary>
        /// The result value of the source expression is the instance that performs the property call.
        /// </summary>
        public virtual OclExpression Source
        {
            get;
            set;
        }
    }
}
