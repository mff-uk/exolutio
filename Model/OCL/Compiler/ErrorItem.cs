using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;

namespace Exolutio.Model.OCL.Compiler {
    public class ErrorItem {
        public string Text {
            protected set;
            get;
        }

        public ErrorItem(string text) {
            Text = text;
        }
    }

    public class CodeErrorItem:ErrorItem {
        public CommonToken StartToken {
            protected set;
            get;
        }
        public CommonToken EndToken {
            protected set;
            get;
        }

        public CodeErrorItem(string text, CommonToken startToken, CommonToken endToken):base(text){
            StartToken = StartToken;
            EndToken = endToken;
        }
    }
}
