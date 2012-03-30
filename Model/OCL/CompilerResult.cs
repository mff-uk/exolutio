using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Compiler;
using Exolutio.Model.OCL.TypesTable;

namespace Exolutio.Model.OCL {
    public class CompilerResult {
        public Constraints Constraints {
            private set;
            get;
        }

        public ErrorCollection Errors {
            private set;
            get;
        }

        public TypesTable.Library Library {
            private set;
            get;
        }

        public IBridgeToOCL Bridge { get; set; }

        public CompilerResult(Constraints con, ErrorCollection errColl, Library lib, IBridgeToOCL bridge) {
            this.Constraints = con;
            this.Errors = errColl;
            this.Library = lib;
            this.Bridge = bridge;
        }
    }

    public class ExpressionCompilerResult {
        public OclExpression Expression {
            private set;
            get;
        }

        public ErrorCollection Errors {
            private set;
            get;
        }

        public TypesTable.Library Library {
            private set;
            get;
        }

        public ExpressionCompilerResult(OclExpression expression, ErrorCollection errColl, Library lib) {
            this.Expression = expression;
            this.Errors = errColl;
            this.Library = lib;
        }
    }
}
