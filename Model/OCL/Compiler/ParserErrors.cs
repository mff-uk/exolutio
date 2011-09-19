using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Compiler {
    public class ParserErrors {
        ErrorCollection Error;

        public ParserErrors(ErrorCollection destination) {
            Error = destination;
        }
    }
}
