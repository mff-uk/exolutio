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
        public Environment Environment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Source expression of iterotor. Itertor is calling on this expression.</param>
        /// <param name="isPre">Is marked by pre</param>
        /// <param name="refOperation">Called operation</param>
        /// <param name="args">Parameters of operation</param>
        /// <param name="environment">Environment</param>
        public OperationCallExp(OclExpression source, bool isPre, Operation refOperation, List<OclExpression> args, 
            Environment environment = null):base(source,isPre,refOperation.ReturnType) {
            this.ReferredOperation = refOperation;
            this.Arguments = args;
            this.Environment = environment;
            }

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

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }

        public override void Accept(IAstVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
