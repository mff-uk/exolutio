using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.AST;
using Antlr.Runtime;
using Antlr.Runtime.Tree;

namespace Exolutio.Model.OCL.Compiler {
    class CodeSource : IExpressionSource {
        public CodeSource(IToken token) {
            Line = token.Line;
            Column = token.CharPositionInLine;
        }

        public CodeSource(CommonTree tree) {
            Line = tree.Line;
            Column = tree.CharPositionInLine;
        }

        #region IExpressionSource Members

        public bool IsFromCode {
            get { return true; }
        }

        public int Line {
            get;
            private set;
        }

        public int Column {
            get;
            private set;
        }

        #endregion
    }
}
