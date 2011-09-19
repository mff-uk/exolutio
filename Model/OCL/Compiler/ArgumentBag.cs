using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.AST;
using Antlr.Runtime;

namespace Exolutio.Model.OCL.Compiler {
    class ArgumentBag {
        public ArgumentBag(OclExpression expr, CommonToken start, CommonToken stop) {
            this.Expression = expr;
            this.Start = start;
            this.Stop = stop;
        }

        public OclExpression Expression {
            get;
            protected set;
        }

        public CommonToken Start {
            get;
            protected set;
        }
        public CommonToken Stop {
            get;
            protected set;
        }
    }
}
