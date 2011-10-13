using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Compiler;

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

        public CompilerResult(Constraints con, ErrorCollection errColl) {
            Constraints = con;
            Errors = errColl;
        }
    }
}
