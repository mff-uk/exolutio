using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Compiler {
    public class LexerErrors {
        ErrorCollection Error;

        public LexerErrors(ErrorCollection destination) {
            Error = destination;
        }
    }
}
