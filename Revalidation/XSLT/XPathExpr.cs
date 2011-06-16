using System;
using System.Collections.Generic;
using System.Diagnostics;
using Exolutio.SupportingClasses;

namespace Exolutio.Revalidation.XSLT
{
    public class XPathExpr
    {
        private readonly string _expr;

        public XPathExpr(string expression)
        {
            _expr = expression;
        }

        public XPathExpr(string format, params object[] args)
            : this(string.Format(format, args))
        {

        }

        public XPathExpr(XPathExpr expression)
        {
            _expr = expression._expr;
        }

        public override string ToString()
        {
            return _expr;
        }

        public static implicit operator string(XPathExpr expression)
        {
            return expression.ToString();
        }

        public XPathExpr AppendPredicate(string predicate)
        {
            string res;
            if (predicate.StartsWith("[") && predicate.EndsWith("]"))
            {
                res = _expr + predicate;
            }
            else
            {
                res = _expr + "[" + predicate + "]";
            }
            return new XPathExpr(res);
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(_expr);
        }

        public static XPathExpr DummyUpStep(XPathExpr xPathExpr)
        {
            return new XPathExpr("..{0}", xPathExpr._expr.Substring(xPathExpr._expr.LastIndexOf("/")));
        }

        public bool HasPrefix(XPathExpr prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            return this.ToString().StartsWith(prefix);
        }

        public XPathExpr InsertAfterPrefix(XPathExpr prefix, string insertedExpr)
        {
            Debug.Assert(HasPrefix(prefix));
            return new XPathExpr(this.ToString().Insert(prefix.ToString().Length, insertedExpr));
        }

        public XPathExpr Append(string step)
        {
            return new XPathExpr(this._expr + step);
        }

        public static bool IsNullOrEmpty(XPathExpr expr)
        {
            return expr == null || expr.IsEmpty();
        }

        /// <summary>
        /// Returns "|" XPath operator
        /// </summary>
        public const string PIPE_OPERATOR = " | ";

        /// <summary>
        /// Returns " or " XPath operator
        /// </summary>
        public const string OR_OPERATOR = " or ";

        public static readonly XPathExpr INVALID_PATH_EXPRESSION = new XPathExpr("###");

        public static XPathExpr ConcatWithOrOperator(IEnumerable<XPathExpr> xpathExpressions)
        {
            string result = xpathExpressions.ConcatWithSeparator(PIPE_OPERATOR);
            return new XPathExpr(result);
        }

        /// <summary>
        /// Returns "$cg" XPath expression
        /// </summary>
        public static XPathExpr CurrentGroupVariableExpr
        {
            get
            {
                return new XPathExpr("$cg");
            }
        }

        public XPathExpr NoCurrentGroup()
        {
            return new XPathExpr(this._expr.Replace("/" + CurrentGroupVariableExpr, String.Empty));
        }

        #region equality members

        public bool Equals(XPathExpr other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._expr, _expr);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(XPathExpr)) return false;
            return Equals((XPathExpr)obj);
        }

        public override int GetHashCode()
        {
            return (_expr != null ? _expr.GetHashCode() : 0);
        }

        public static bool operator ==(XPathExpr left, XPathExpr right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(XPathExpr left, XPathExpr right)
        {
            return !Equals(left, right);
        }

        #endregion

    }
}