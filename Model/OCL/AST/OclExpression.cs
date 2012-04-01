using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    public abstract class OclExpression:TypedElement
    {
        public OclExpression(Classifier type)
        {
            Type = type;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) {
            throw new InvalidOperationException();
        }

        public virtual void Accept(IAstVisitor visitor) {
            throw new InvalidOperationException();
        }

        public override string ToString() {
            Utils.PrintVisitor printer = new Utils.PrintVisitor();
            return printer.AstToString(this);
        }

     //   public OclExpression() { }
        private IExpressionSource codeSource;
        private static readonly IExpressionSource InvalidCodeSource = new InvalidExpressionSource();

        public IExpressionSource CodeSource {
            get{
                if (codeSource == null) {
                    return InvalidCodeSource;
                }
                return codeSource;
            }
            set {
                codeSource = value;
            }
        }


        /// <summary>
        /// True when the expression is the top-level expression in the invariant. 
        /// The value is assigned manually, not by parsing. 
        /// </summary>
        public bool IsInvariant { get; set; }

        /// <summary>
        /// True when the expression is part of the error message of an invariant. 
        /// e.g <code>self.ID</code> in the following: <code>message: 'This ID is wrong: {self.ID}'.</code>
        /// Assigned in <see cref="CompilerResult.CompileExpressionsInMessages"/>.
        /// </summary>
        public bool IsMessageInlinedSubexpression { get; set; }

        /// <summary>
        /// True when the expression is a part of 'body' of some iterator experession. 
        /// NOTE: also true when he body is 
        /// The value is assigned manually, not by parsing. 
        /// </summary>
        public bool IsPartOfIteratorBody { get; set; }

        /// <summary>
        /// When <see cref="IsInvariant"/> == true, this property holds a reference 
        /// to the containing constraint. 
        /// The value is assigned manually, not by parsing.
        /// </summary>
        public ClassifierConstraintBlock ClassifierConstraintBlock { get; set; }
    }
}
