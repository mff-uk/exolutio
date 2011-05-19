using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model.OCL.Types;

namespace EvoX.Model.OCL.AST
{
    /// <summary>
    /// Variables are typed elements for passing data in expressions. The variable can be used in expressions where the variable
    /// is in scope. This metaclass represents among others the variables self and result and the variables defined using the Let
    /// expression.
    /// </summary>
    public class Variable : VariableExp
    {
        /// <summary>
        /// The OclExpression that represents the initial value of the variable. Depending on the role that
        /// a variable declaration plays, the init expression might be mandatory.
        /// </summary>
        public OclExpression InitExpression
        {
            get;
            set;
        }

        /// <summary>
        /// The Parameter in the current operation this variable is representing. Any access to the
        /// variable represents an access to the parameter value.
        /// </summary>
        public Parameter representedParameter
        {
            get;
            set;
        }


    }
}
