using System;
using Exolutio.Model.OCL.AST;

namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    public class ExpressionNotSupportedInXPath: Exception
    {
        public OclExpression Expression
        {
            get; set; 
        }

        public ExpressionNotSupportedInXPath(OclExpression expression)
        {
            Expression = expression; 
        }

        public ExpressionNotSupportedInXPath(OclExpression expression, string message) : base(message)
        {
            Expression = expression; 
        }
    }
}