using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST {
    public class PrintVisitor:IAstVisitor<string> {

        StringBuilder sb;

        public string AstToString(OclExpression expr) {
            sb = new StringBuilder();
            expr.Accept(this);
            return sb.ToString();
        }

        #region IAstVisitor<string> Members

        public string Visit(BooleanLiteralExp node) {
            sb.Append(node.GetType().Name);
            sb.Append(":");
            sb.Append(node.Value);
            return string.Empty;
        }

        public string Visit(CollectionLiteralExp node) {
            sb.Append(node.Type.Name);
            return string.Empty;
        }

        public string Visit(EnumLiteralExp node) {
            throw new NotImplementedException();
            
        }

        public string Visit(ErrorExp node) {
            sb.Append("ErrorExp");
            return string.Empty;
        }

        public string Visit(IfExp node) {
            sb.Append("If(" );
            sb.Append(node.Condition.Accept(this));
            sb.Append(") ");
            sb.Append(node.ThenExpression.Accept(this));
            sb.Append("Else");
            sb.Append(node.ElseExpression.Accept(this));
            sb.Append("EndIf");
            return string.Empty;
        }

        public string Visit(IntegerLiteralExp node) {
            sb.Append(node.GetType().Name);
            sb.Append(":");
            sb.Append(node.Value);
            return string.Empty;
        }

        public string Visit(InvalidLiteralExp node) {
            sb.Append(node.GetType().Name);
            return string.Empty;
        }

        public string Visit(IterateExp node) {
            throw new NotImplementedException();
        }

        public string Visit(IteratorExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
            }
            sb.Append("->");
            sb.Append(node.IteratorName);
            sb.Append("(");
            node.Body.Accept(this);
            sb.Append(")");
            
            return string.Empty;
        }

        public string Visit(LetExp node) {
            throw new NotImplementedException();
        }

        public string Visit(NullLiteralExp node) {
            sb.Append("null");
            return string.Empty;
        }

        public string Visit(OperationCallExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
            }
            sb.Append(".");
            sb.Append(node.ReferredOperation.Name);
            sb.Append("(");
            foreach(var arg in node.Arguments){
                arg.Accept(this);
                sb.Append(",");
            }
            sb.Append(")");

            return string.Empty;
        }

        public string Visit(PropertyCallExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
            }
            sb.Append(".");
            sb.Append(node.ReferredProperty.Name);
            

            return string.Empty;
        }

        public string Visit(RealLiteralExp node) {
            throw new NotImplementedException();
        }

        public string Visit(StringLiteralExp node) {
            throw new NotImplementedException();
        }

        public string Visit(TupleLiteralExp node) {
            throw new NotImplementedException();
        }

        public string Visit(TypeExp node) {
            throw new NotImplementedException();
        }

        public string Visit(UnlimitedNaturalLiteralExp node) {
            throw new NotImplementedException();
        }

        public string Visit(VariableExp node) {
            sb.Append(node.referredVariable.Name);
            return string.Empty;
        }

        #endregion
    }
}
