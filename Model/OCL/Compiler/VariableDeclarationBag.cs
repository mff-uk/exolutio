using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.AST;
using Antlr.Runtime;

namespace Exolutio.Model.OCL.Compiler {
    class VariableDeclarationBag {
        public string Name {
            get;
            private set;
        }

        public Classifier Type {
            get;
            private set;
        }

        public OclExpression Expression {
            get;
            private set;
        }

        //public IToken StartToken {
        //    get;
        //    private set;
        //}

        //public IToken EndToken {
        //    get;
        //    private set;
        //}

        public VariableDeclarationBag(string name, Classifier type, OclExpression expr) {
            this.Name = name;
            this.Type = type;
            this.Expression = expr;
        }
    }
}
