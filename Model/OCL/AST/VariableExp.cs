using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.AST
{
    /// <summary>
    /// A VariableExp is an expression that consists of a reference to a variable. References to the variables self and result or to
    /// variables defined by Let expressions are examples of such variable expressions.
    /// </summary>
    public class VariableExp
    {
        /// <summary>
        /// The Variable to which this variable expression refers.
        /// </summary>
        public Variable referredVariable
        {
            get;
            set;
        }
    }
}
