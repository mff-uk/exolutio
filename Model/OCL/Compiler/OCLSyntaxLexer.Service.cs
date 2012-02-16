using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;

namespace Exolutio.Model.OCL.Compiler {
    public partial class OCLSyntaxLexer : Antlr.Runtime.Lexer{

        public ErrorCollection Errors {
            get;
            private set;
        }

        public OCLSyntaxLexer(ICharStream input,ErrorCollection errorColl):this(input) {
            Errors = errorColl;
        }

        partial void OnCreated() {
            if (Errors == null) {
                Errors = new ErrorCollection();
            }
        }

        public override void ReportError(Antlr.Runtime.RecognitionException e) {
            Errors.AddError(new CodeErrorItem(e.ToString(), e.Token, e.Token));
            base.ReportError(e);
                                                                                                                                                    
        }
    }
}
