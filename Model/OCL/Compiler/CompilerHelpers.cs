using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;

namespace Exolutio.Model.OCL.Compiler {
    static class CompilerHelpers {
        public static List<string> ToStringList(this List<IToken> tokens) {
            return tokens.Select(a => a.Text).ToList();
        }

        public static T SetCodeSource<T>(this T exp, AST.IExpressionSource source) where T : AST.OclExpression {
            exp.CodeSource = source;
            return exp;
        }
    }
}
