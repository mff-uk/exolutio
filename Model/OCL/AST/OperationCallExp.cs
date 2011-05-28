using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// An OperationCallExp refers to an operation defined in a Classifier. The expression may contain a list of argument
    /// expressions if the operation is defined to have parameters. In this case, the number and types of the arguments must match
    /// the parameters.
    /// </summary>
    public class OperationCallExp : FeatureCallExp
    {
        /// <summary>
        /// The arguments denote the arguments to the operation call. This is only useful when the
        /// operation call is related to an Operation that takes parameters.
        /// </summary>
        public List<OclExpression> Arguments
        {
            get;
            set;
        }

        /// <summary>
        /// The Operation to which this OperationCallExp is a reference. This is an Operation of a
        /// Classifier that is defined in the UML model.
        /// </summary>
        public Operation ReferredOperation
        {
            get;
            set;
        }
    }
}
