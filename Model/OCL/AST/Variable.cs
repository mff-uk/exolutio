using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// <b>THIS CLASS IS NOT USED ANYWHERE</b>
    /// 
    /// Variables are typed elements for passing data in expressions. The variable can be used in expressions where the variable
    /// is in scope. This metaclass represents among others the variables self and result and the variables defined using the Let
    /// expression.
    /// </summary>
    public class Variable 
    {
        public Variable(VariableDeclaration decl) {
            RepresentedParameter = decl;
        }

        public Variable(VariableDeclaration decl, OclExpression init):this(decl) {
            InitExpression = init;
        }

        /// <summary>
        /// The OclExpression that represents the initial value of the variable. Depending on the role that
        /// a variable declaration plays, the init expression might be mandatory.
        /// </summary>
        public OclExpression InitExpression
        {
            get;
            protected set;
        }

        /// <summary>
        /// The Parameter in the current operation this variable is representing. Any access to the
        /// variable represents an access to the parameter value.
        /// 
        /// U typu representedParameter je v dokumentaci docela velky zmatek, podle definice by tam mel byt typ parameter,
        /// ale dale v dokumentaci se s nim pracuje jako s VariableDeclaration.
        /// </summary> 

        public VariableDeclaration RepresentedParameter
        {
            get;
            protected set;
        }


    }
}
