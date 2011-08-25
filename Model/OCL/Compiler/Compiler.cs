using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Exolutio.Model.OCL.Compiler {
    class Compiler {
        public OCLParser Parser {
            get;
            protected set;
        }
        public OCLLexer Lexer {
            get;
            protected set;
        }

        public Error Errors {
            get;
            protected set;
        }
    }
}
