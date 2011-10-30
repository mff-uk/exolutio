using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.AST;

namespace Exolutio.Model.OCL.Utils {
    class CanonicPrinterVisitor:IAstVisitor {

        StringBuilder sb;

        public string AstToString(OclExpression expr) {
            sb = new StringBuilder();
            expr.Accept(this);
            return sb.ToString();
        }

        #region IAstVisitor Members

        public void Visit(BooleanLiteralExp node) {
            sb.AppendFormat("({0}:{1})", node.GetType().Name, node.Value);
        }

        public void Visit(CollectionLiteralExp node) {
            sb.Append("CollectionLiteral(" + node.Type.Name + ")");
        }

        public void Visit(EnumLiteralExp node) {
            throw new NotImplementedException();
        }

        public void Visit(ErrorExp node) {
            sb.Append("ErrorExp");

        }

        public void Visit(IfExp node) {
            sb.Append("If(");
            node.Condition.Accept(this);
            sb.Append(")");
            node.ThenExpression.Accept(this);
            sb.Append("Else");
            node.ElseExpression.Accept(this);
            sb.Append("EndIf");

        }

        public void Visit(IntegerLiteralExp node) {
            sb.AppendFormat("({0}:{1})", node.GetType().Name, node.Value);

        }

        public void Visit(InvalidLiteralExp node) {
            sb.AppendFormat("({0})", node.GetType().Name);

        }

        public void Visit(IterateExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
            }
            sb.AppendFormat("->iterate(");
        }

        public void Visit(IteratorExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
            }
            sb.Append("->");
            sb.Append(node.IteratorName);
            sb.Append("(");
            node.Body.Accept(this);
            sb.Append(")");


        }

        public void Visit(LetExp node) {
            sb.AppendFormat("(let {0}:{1} = ", node.Variable.Name, node.Variable.PropertyType);
            node.Variable.Value.Accept(this);
            sb.Append("in");
            node.InExpression.Accept(this);
            sb.Append(")");
        }

        public void Visit(NullLiteralExp node) {
            sb.Append("null");

        }

        public void Visit(OperationCallExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
            }
            sb.Append(".");
            sb.Append(node.ReferredOperation.Name);
            sb.Append("(");
            foreach (var arg in node.Arguments) {
                arg.Accept(this);
                sb.Append(",");
            }
            sb.Append(")");


        }

        public void Visit(PropertyCallExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
            }
            sb.Append(".");
            sb.Append(node.ReferredProperty.Name);



        }

        public void Visit(RealLiteralExp node) {
            throw new NotImplementedException();
        }

        public void Visit(StringLiteralExp node) {
            throw new NotImplementedException();
        }

        public void Visit(TupleLiteralExp node) {
            throw new NotImplementedException();
        }

        public void Visit(TypeExp node) {
            throw new NotImplementedException();
        }

        public void Visit(UnlimitedNaturalLiteralExp node) {
            throw new NotImplementedException();
        }

        public void Visit(VariableExp node) {
            sb.Append(node.referredVariable.Name);

        }

        #endregion
    }
}
