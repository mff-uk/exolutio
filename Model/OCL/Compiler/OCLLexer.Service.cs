using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Compiler {
   public  partial class OCLLexerService {
       public ErrorCollection Errors {
           get;
           private set;
       }

       public ParserErrors ReportError {
           get;
           private set;
       }
    }
}
