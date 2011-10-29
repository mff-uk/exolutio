using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.Utils {
    public class PrintVisitor : IAstVisitor {

        StringBuilder sb;

        public string AstToString(OclExpression expr) {
            sb = new StringBuilder();
            expr.Accept(this);
            return sb.ToString();
        }

        #region IAstVisitor Members

        public void Visit(BooleanLiteralExp node) {
            sb.AppendFormat("{0}", node.Value);
        }

        public void Visit(CollectionLiteralExp node) {
            CollectionType col = (CollectionType)node.Type;
            sb.AppendFormat("{0}({1}){ ", col.Name, col.ElementType.Name);
            PrintArgs(node.Parts,",", part => {
                if (part is CollectionRange) {
                    CollectionRange range = (CollectionRange)part;
                    range.First.Accept(this);
                    sb.Append("..");
                    range.Last.Accept(this);
                }
                else if (part is CollectionItem) {
                    CollectionItem item = (CollectionItem)part;
                    item.Item.Accept(this);
                }
            });
            sb.Append("}");
        }

        public void Visit(EnumLiteralExp node) {
            throw new NotImplementedException();
        }

        public void Visit(ErrorExp node) {
            sb.Append(" ERROR ");
        }

        public void Visit(IfExp node) {
            sb.Append("if ");
            node.Condition.Accept(this);
            sb.Append(" then");
            node.ThenExpression.Accept(this);
            sb.Append(" else ");
            node.ElseExpression.Accept(this);
            sb.Append(" endIf ");
        }

        public void Visit(IntegerLiteralExp node) {
            sb.Append(node.Value);
        }

        public void Visit(InvalidLiteralExp node) {
            sb.Append(" invalid ");
        }

        public void Visit(IterateExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
                sb.Append("->");
            }
            sb.AppendFormat("iterate( ");
            PrintArgs(node.Iterator, ",", (v) => {
                VariableDeclaration(v);
            });
            VariableDeclaration(node.Result);
            sb.Append(" | ");
            node.Body.Accept(this);
            sb.Append(")");
        }

        public void Visit(IteratorExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
                sb.Append("->");
            }
            sb.Append(node.IteratorName);
            sb.Append("(");
            if (node.Iterator != null) {
                PrintArgs(node.Iterator, ",", (v) => {
                    VariableDeclaration(v);
                });
                sb.Append(" | ");
            }
            node.Body.Accept(this);
            sb.Append(")");
        }

        public void Visit(LetExp node) {
            sb.AppendFormat("let ");
            VariableDeclaration(node.Variable);
            sb.Append(" in ");
            node.InExpression.Accept(this);
        }

        public void Visit(NullLiteralExp node) {
            sb.Append("null");
        }

        public void Visit(OperationCallExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
                sb.Append(".");
            }
            sb.Append(node.ReferredOperation.Name);
            sb.Append("(");
            PrintArgs(node.Arguments, ",", (arg) => {
                arg.Accept(this);
            });
            sb.Append(")");
        }

        public void Visit(PropertyCallExp node) {
            if (node.Source != null) {
                node.Source.Accept(this);
                sb.Append(".");
            }
            sb.Append(node.ReferredProperty.Name);
        }

        public void Visit(RealLiteralExp node) {
            sb.Append(node.Value);
        }

        public void Visit(StringLiteralExp node) {
            sb.Append("\"");
            sb.Append(node.Value);
            sb.Append("\"");
        }

        public void Visit(TupleLiteralExp node) {
            sb.Append("Tuple");
            sb.Append("{");
            PrintArgs(node.Parts, ",", (v) => {
                sb.AppendFormat("{0} : {1} = ", v.Key, v.Value.Type.Name);
                v.Value.Value.Accept(this);
            });
            sb.Append("} ");
        }

        public void Visit(TypeExp node) {
            sb.Append(node.ReferredType.Name);
        }

        public void Visit(UnlimitedNaturalLiteralExp node) {
        }

        public void Visit(VariableExp node) {
            sb.Append(node.referredVariable.Name);
        }

        public void VariableDeclaration(VariableDeclaration decl) {
            sb.Append(decl.Name);
            if (decl.PropertyType != null) {
                sb.AppendFormat(" : {0}", decl.PropertyType.Name);
            }
            if (decl.Value != null) {
                sb.Append(" = ");
                decl.Value.Accept(this);
            }
        }

        #endregion

        void PrintArgs<T>(IEnumerable<T> source, string separator, Action<T> argPrinter) {
            bool isOther = false;
            foreach (T arg in source) {
                if (isOther) {
                    sb.AppendFormat("{0} ", separator);
                }
                isOther = true;
                argPrinter(arg);
            }
        }
    }


}
