using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// An IfExp results in one of two alternative expressions depending on the evaluated value of a condition. Note that both the
    /// thenExpression and the elseExpression are mandatory. The reason behind this is that an if expression should always result
    /// in a value, which cannot be guaranteed if the else part is left out.
    /// </summary>
    public class IfExp : OclExpression
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Return type of If expression.</param>
        /// <param name="condition">If condition</param>
        /// <param name="thenExpr"></param>
        /// <param name="elseExpr"></param>
        public IfExp(Classifier type,OclExpression condition, OclExpression thenExpr,OclExpression elseExpr) : base(type) {
            this.Condition = condition;
            this.ThenExpression = thenExpr;
            this.ElseExpression = elseExpr;
        }
        /// <summary>
        /// The OclExpression that represents the boolean condition. If this condition evaluates to true,
        /// the result of the if expression is identical to the result of the thenExpression. If this condition
        /// evaluates to false, the result of the if expression is identical to the result of the
        /// elseExpression.
        /// </summary>
        public OclExpression Condition
        {
            get;
            set;
        }

        /// <summary>
        /// The OclExpression that represents the then part of the if expression.
        /// </summary>
        public OclExpression ThenExpression
        {
            get;
            set;
        }

        /// <summary>
        /// The OclExpression that represents the else part of the if expression.
        /// </summary>
        public OclExpression ElseExpression
        {
            get;
            set;
        }

        public override Types.Classifier Type {
            get {
                return ThenExpression.Type.CommonSuperType(ElseExpression.Type);
            }
            protected set {
               
            }
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }

        public override void Accept(IAstVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
