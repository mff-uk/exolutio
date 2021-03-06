﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;

namespace Exolutio.Model.OCL.Compiler {
    partial class OCLSyntaxParser {
        public ErrorCollection Errors {
            get;
            private set;
        }

        public OCLSyntaxParser(ITokenStream input, ErrorCollection errColl)
            : this(input) {
            Errors = errColl;
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
