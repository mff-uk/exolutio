using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A FeatureCallExp expression is an expression that refers to a feature that is defined for a Classifier in the UML model to
    /// which this expression is attached. Its result value is the evaluation of the corresponding feature.
    /// </summary>
    public class FeatureCallExp : OclExpression
    {
        /// <summary>
        /// Boolean indicating whether the expression accesses the precondition-time value of the referred feature.
        /// </summary>
        public bool IsPre
        {
            get;
            set;
        }
    }
}
