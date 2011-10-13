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
        public IToken StartToken {
            protected set;
            get;
        }
        public IToken EndToken {
            protected set;
            get;
        }

        public CodeErrorItem(string text, IToken startToken, IToken endToken):base(text){
            StartToken = StartToken;
            EndToken = endToken;
        }
    }
}
