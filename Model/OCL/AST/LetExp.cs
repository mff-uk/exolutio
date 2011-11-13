using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A LetExp is a special expression that defined a new variable with an initial value. A variable defined by a LetExp cannot
    /// change its value. The value is always the evaluated value of the initial expression. The variable is visible in the in
    /// expression.
    /// </summary>
    public  class LetExp : OclExpression
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="variable">Substitute variable</param>
        /// <param name="inExpr">Substitute expression in expression</param>
        public LetExp(Types.VariableDeclaration variable, OclExpression inExpr)
            : base(inExpr.Type) {
            this.Variable = variable;
            this.InExpression = inExpr;
        }

        /// <summary>
        /// The Variable introduced by the Let expression.
        /// </summary>
        public Types.VariableDeclaration Variable
        {
            get;
            set;
        }

        /// <summary>
        /// The OclExpression in whose environment the defined variable is visible.
        /// </summary>
        public OclExpression InExpression
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
